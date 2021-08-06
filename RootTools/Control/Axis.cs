using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.Control
{
    public class Axis : NotifyProperty
    {
        #region Property
        public string p_id { get; set; }

        public enum eState
        {
            Init,
            Home,
            Ready,
            Move,
            Jog,
            Error
        }
        eState _eState = eState.Init;
        public eState p_eState
        {
            get { return _eState; }
            set
            {
                if (_eState == value) return;
                p_log.Info(p_id + " State : " + _eState.ToString() + " -> " + value.ToString());
                _eState = value;
                OnPropertyChanged();
            }
        }

        public Listp_sInfo m_infoList = new Listp_sInfo();
        string _sInfo = "Info";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                OnPropertyChanged();
                if (value == "OK") return;
                p_log.Info("Info : " + _sInfo);
                m_infoList.Add(_sInfo);
                EQ.p_sInfo = p_id + " : " + value;
            }
        }

        public Log p_log { get; set; }

        public double p_dRelPos { get; set; }
        #endregion

        #region UI
        public virtual UserControl p_ui { get; set; }
        #endregion

        #region Position & Velocity
        double _posCommand = 0;
        public double p_posCommand
        {
            get { return _posCommand; }
            set
            {
                if (_posCommand == value) return;
                _posCommand = value;
                OnPropertyChanged();
            }
        }

        double _posActual = 0;
        public double p_posActual
        {
            get { return _posActual; }
            set
            {
                if (_posActual == value) return;
                _posActual = value;
                OnPropertyChanged();
            }
        }

        double _vNow = 0;
        public double p_vNow
        {
            get { return _vNow; }
            set
            {
                if (_vNow == value) return;
                _vNow = value;
                OnPropertyChanged();
            }
        }

        public virtual void SetCommandPosition(double fPos) { }
        public virtual void SetActualPosition(double fPos) { }
        public virtual double GetActualPosition() { return 0; }
        public virtual void OverrideVelocity(double v) { }
        #endregion

        #region List Position
        public Dictionary<string, double> m_aPos = new Dictionary<string, double>();
        public List<string> m_asPos = new List<string>(); 
        public ObservableCollection<string> p_asPos { get; set; }
        public string p_strSelPos { get; set; }
        
        public void AddPos(params string[] asPos)
        {
            foreach (string sPos in asPos) AddPos(sPos);
            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
        }

        void AddPos(string sPos)
        {
            foreach (string sKey in m_aPos.Keys)
            {
                if (sKey == sPos) return;
            }
            m_aPos.Add(sPos, 0);
            p_asPos.Add(sPos);
            m_asPos.Add(sPos); 
        }

        public double GetPosValue(Enum pos)
        {
            return GetPosValue(pos.ToString());
        }

        public double GetPosValue(string sPos)
        {
            foreach (string sKey in p_asPos)
            {
                if (sKey == sPos) return m_aPos[sPos];
            }
            return 0;
        }

        public void SetPositionValue(string sPos, bool bCommand = true)
        {
            foreach (string sKey in p_asPos)
            {
                if (sKey == sPos)
                {
                    m_aPos[sPos] = bCommand ? p_posCommand : p_posActual;
                    RunTree(Tree.eMode.RegWrite);
                    RunTree(Tree.eMode.Init);
                }
            }
        }

        public bool IsInPos(Enum pos, double posError = 20)
        {
            double dPos = GetPosValue(pos) - p_posCommand;
            return Math.Abs(dPos) <= posError;
        }

        public enum ePosition
        {
            SWLimit_Minus,
            SWLimit_Plus,
            SWBoardLimit_Minus,
            SWBoardLimit_Plus,
            Position_0,
            Position_1,
            Position_2,
        }
        void InitPosition()
        {
            p_asPos = new ObservableCollection<string>(); 
            for (int i = 0; i < Enum.GetNames(typeof(ePosition)).Length; i++)
            {
                AddPos(((ePosition)i).ToString());
            }
            RunTree(Tree.eMode.RegRead);
        }
        public virtual void RunTreePos(Tree tree, string sUnit)
        {
            RunTreePosLimit(tree.GetTree("SW Limit", false));
            RunTreePosition(tree.GetTree("Position"), sUnit);
        }

        void RunTreePosition(Tree tree, string sUnit)
        {
            string sDesc = "Axis Position (" + sUnit + ")";
            m_aPos[p_asPos[0]] = tree.Set(m_aPos[p_asPos[0]], 0.0, p_asPos[0], sDesc, m_bSWLimit[0]);
            m_aPos[p_asPos[1]] = tree.Set(m_aPos[p_asPos[1]], 0.0, p_asPos[1], sDesc, m_bSWLimit[1]);

            m_aPos[p_asPos[2]] = tree.Set(m_aPos[p_asPos[2]], 0.0, p_asPos[2], sDesc, m_bSWBoardLimit);
            m_aPos[p_asPos[3]] = tree.Set(m_aPos[p_asPos[3]], 0.0, p_asPos[3], sDesc, m_bSWBoardLimit);

            for (int n = 4; n < p_asPos.Count; n++)
            {
                m_aPos[p_asPos[n]] = tree.Set(m_aPos[p_asPos[n]], 0.0, p_asPos[n], sDesc);
            }

        }
        #endregion

        #region SW Limit
        protected bool[] m_bSWLimit = new bool[2] { false, false };
        protected bool m_bSWBoardLimit = false;

        protected string CheckSWLimit(ref double fPosDst)
        {
            if (EQ.p_bSimulate) return "OK";
            double fPosNow = p_posCommand;
            double fPosMinusLimit = m_aPos[p_asPos[0]];
            if (m_bSWLimit[0])
            {
                if (fPosDst >= fPosMinusLimit) return "OK";
                if (fPosNow < fPosMinusLimit)
                {
                    EQ.p_bStop = true;
                    return "SW Minus Limit Error : " + fPosDst.ToString();
                }
                fPosDst = fPosMinusLimit;
                return "OK";
            }
            double fPosPlusLimit = m_aPos[p_asPos[1]];
            if (m_bSWLimit[1])
            {
                if (fPosDst <= fPosPlusLimit) return "OK";
                if (fPosNow > fPosPlusLimit)
                {
                    EQ.p_bStop = true;
                    return "SW Plus Limit Error : " + fPosDst.ToString();
                }
                fPosDst = fPosPlusLimit;
                return "OK";
            }
            return "OK";
        }

        string CheckSWLimit(double vJog)
        {
            if (vJog == 0) return "OK";
            double fPosNow = p_posCommand;
            if (m_bSWLimit[0] && (vJog < 0) && (fPosNow <= m_aPos[p_asPos[0]]))
            {
                EQ.p_bStop = true;
                return "SW Minus Limit Error";
            }
            if (m_bSWLimit[1] && (vJog > 0) && (fPosNow >= m_aPos[p_asPos[1]]))
            {
                EQ.p_bStop = true;
                return "SW Plus Limit Error";
            }
            return "OK";
        }

        bool bLimitPlusCheck = false;
        bool bLimitMinusCheck = false;
        public void ThreadCheck_SWLimit(bool isPlus)
        {
            bool isError = false;
            double fPos = p_posActual;
            bool bSWLimit0 = m_bSWLimit[0] && (fPos < m_aPos[p_asPos[0]]);
            if (bSWLimit0 && !isPlus && !bLimitMinusCheck)
            {
                isError = true;
                p_log.Info(p_id + ": Servo SW limit(-) !!");
                bLimitMinusCheck = true;
            }
            bool bSWLimit1 = m_bSWLimit[1] && (fPos > m_aPos[p_asPos[1]]);
            if (bSWLimit1 && isPlus && !bLimitPlusCheck)
            {
                isError = true;
                p_log.Info(p_id + ": Servo SW limit(+) !!");
                bLimitPlusCheck = true;
            }

            if (fPos > m_aPos[p_asPos[0]])
                bLimitMinusCheck = false;

            if (fPos < m_aPos[p_asPos[1]])
                bLimitPlusCheck = false;

            if (isError)
            {
                StopAxis();
            }
        }

        void RunTreePosLimit(Tree tree)
        {
            m_bSWLimit[0] = tree.Set(m_bSWLimit[0], m_bSWLimit[0], "Minus", "Use SW Minus Limit");
            m_bSWLimit[1] = tree.Set(m_bSWLimit[1], m_bSWLimit[1], "Plus", "Use SW Plus Limit");
            m_bSWBoardLimit = tree.Set(m_bSWBoardLimit, m_bSWBoardLimit, "Board SW Limit", "Use Board SW Limit");
        }
        #endregion

        #region List Speed
        public class Speed
        {
            public string m_id;
            public double m_v = 1000;
            public double m_acc = 0.5;
            public double m_dec = 0.5;

            public Speed(string id)
            {
                m_id = id;
            }

            public void RunTree(Tree tree, string sUnit)
            {
                m_v = tree.Set(m_v, -1.0, "Velocity", "Axis Moving Velocity (" + sUnit + " / sec)");
                m_acc = tree.Set(m_acc, m_acc, "Accelation", "Accelation Time (sec)");
                m_dec = tree.Set(m_dec, m_dec, "Decelation", "Decelation Time (sec)");
            }
        }
        protected List<Speed> m_aSpeed = new List<Speed>();
        public List<string> m_asSpeed = new List<string>(); 
        public ObservableCollection<string> p_asSpeed { get; set; }

        public void AddSpeed(params string[] asSpeed)
        {
            foreach (string sSpeed in asSpeed) AddSpeed(sSpeed);
            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
        }

        void AddSpeed(string sSpeed)
        {
            foreach (string sKey in p_asSpeed)
            {
                if (sKey == sSpeed) return;
            }
            m_aSpeed.Add(new Speed(sSpeed));
            p_asSpeed.Add(sSpeed);
            m_asSpeed.Add(sSpeed);
        }

        public Speed GetSpeedValue(Enum speed)
        {
            return GetSpeedValue(speed.ToString());
        }

        public Speed GetSpeedValue(string sSpeed)
        {
            foreach (Speed speed in m_aSpeed)
            {
                if (speed.m_id == sSpeed) return speed;
            }
            return null;
        }

        public void SetSpeed(string sSpeed, Speed speed)
        {
            for (int n = 0; n < m_aSpeed.Count; n++)
            {
                if (m_aSpeed[n].m_id == sSpeed) m_aSpeed[n] = speed; 
            }
        }

        public enum eSpeed
        {
            Home_First,
            Home_Last,
            Jog,
            Move,
            Move_DoorOpen,
        }
        void InitSpeed()
        {
            p_asSpeed = new ObservableCollection<string>();
            p_fJogScale = 100;
            for (int i = 0; i < Enum.GetNames(typeof(eSpeed)).Length; i++)
            {
                AddSpeed(((eSpeed)i).ToString());
            }
            RunTree(Tree.eMode.RegRead);
        }

        public void RunTreeSpeed(Tree tree, string sUnit)
        {
            foreach (Speed speed in m_aSpeed) speed.RunTree(tree.GetTree(speed.m_id, false), sUnit);
        }
        #endregion

        #region I/O Interlock
        public List<DIO_I> m_aDIO_I = new List<DIO_I>();
        public List<bool> m_bDIO_I = new List<bool>();
        public DIO_I p_vaccumDIO_I { get; set; }
        public bool p_IsLifterDown { get; set; } = false;

        public void AddIO(DIO_I io)
        {
            m_aDIO_I.Add(io);
            m_bDIO_I.Add(false);
            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
        }
        public void RunTreeIOLock(Tree tree, string sUnit)
        {
            for (int i = 0; i < m_aDIO_I.Count; i++)
            {
                string[] id = m_aDIO_I[i].m_id.Split('.');
                m_bDIO_I[i] = tree.Set(m_bDIO_I[i], m_bDIO_I[i], id[1], id[1] + " in Ready Position");
            }
        }
        #endregion

        #region Jog
        protected Speed m_speedNow;
        protected StopWatch m_swMove = new StopWatch();
        public int p_fJogScale { get; set; }

        public virtual string Jog(double fScale, string sSpeed = null)
        {
            m_speedNow = (sSpeed != null) ? GetSpeedValue(sSpeed) : GetSpeedValue(eSpeed.Jog);
            m_swMove.Start();
            if (EQ.IsStop()) return p_id + " EQ Stop";
            if (EQ.p_bSimulate) return "OK";
            //if (p_eState != eState.Ready) return p_id + " Axis State not Ready : " + p_eState.ToString();
            return CheckSWLimit(fScale * m_speedNow.m_v);
        }

        public virtual void StopAxis(bool bSlowStop = true) { }
        #endregion

        #region Move
        public string StartMove(Enum pos, double fOffset = 0, Enum speed = null)
        {
            return StartMove(pos.ToString(), fOffset, (speed != null) ? speed.ToString() : null);
        }

        public string StartMove(string sPos, double fOffset = 0, string sSpeed = null)
        {
            double fPos = GetPosValue(sPos) + fOffset;
            return StartMove(fPos, sSpeed);
        }

        public bool m_bCheckStop = true; 
        public double m_posDst = 0;
        int m_msMoveTime = 0;
        public virtual string StartMove(double fPos, string sSpeed = null)
        {
            m_posDst = fPos;
            m_speedNow = (sSpeed != null) ? GetSpeedValue(sSpeed) : GetSpeedValue(EQ.p_bDoorOpen ? eSpeed.Move_DoorOpen : eSpeed.Move);
            m_swMove.Start();
            if (m_bCheckStop && EQ.IsStop()) return p_id + " EQ Stop";
            if (EQ.p_bSimulate) return "OK";
            if (p_eState != eState.Ready) return p_id + " Axis State not Ready : " + p_eState.ToString();
            double dPos = fPos - p_posCommand;
            m_msMoveTime = (int)(1000 * (dPos / m_speedNow.m_v + m_speedNow.m_acc + m_speedNow.m_dec + 1));
            return CheckSWLimit(ref fPos);
        }

        public virtual string StartMove(double fPos, double v, double acc = -1, double dec = -1)
        {
            m_posDst = fPos;
            m_speedNow = null;
            m_swMove.Start();
            if (EQ.IsStop()) return p_id + " EQ Stop";
            if (EQ.p_bSimulate) return "OK";
            if (p_eState != eState.Ready) return p_id + " Axis State not Ready : " + p_eState.ToString();
            double dPos = fPos - p_posCommand;
            m_msMoveTime = (int)(1000 * (dPos / v + acc + dec + 1));
            return CheckSWLimit(ref fPos);
        }

        public virtual string StartMoveV(double vStart, double posAt, double vChange, double posTo, double acc = -1, double dec = -1)
        {
            m_posDst = posTo;
            m_speedNow = null;
            m_swMove.Start();
            if (EQ.IsStop()) return p_id + " EQ Stop";
            if (EQ.p_bSimulate) return "OK";
            if (p_eState != eState.Ready) return p_id + " Axis State not Ready : " + p_eState.ToString();
            double dPos = posTo - p_posCommand;
            m_msMoveTime = (int)(1000 * (dPos / Math.Min(vStart, vChange) + acc + dec + 1));
            return "OK"; 
        }

        public virtual string WaitReady(double dInPos = -1)
        {
            while (p_eState == eState.Move || p_eState == eState.Home)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop"; 
                //if (m_swMove.ElapsedMilliseconds > m_msMoveTime)
                //{
                //    p_eState = eState.Init;
                //    return p_id + " Axis Move Timeout";
                //}
            }
            switch (p_eState)
            {
                case eState.Ready:
                    double dPos = 0; 
                    if (dInPos >= 0)
                    {
                        for (int n = 0; n < 10; n++)
                        {
                            dPos = m_posDst - p_posCommand;
                            if (Math.Abs(dPos) < dInPos) return "OK"; 
                            p_log.Info("WaitReady InPosition Error #" + n.ToString() + " : " + dPos.ToString());
                            Thread.Sleep(100); 
                        }
                        if (Math.Abs(dPos) > dInPos) return "WaitReady InPosition Error : " + dPos.ToString();
                    }
                    return "OK";
                default: return "WaitReady Error p_eState = " + p_eState.ToString();
            }
        }
        private void M_EQ_OnChanged(_EQ.eEQ eEQ, dynamic value)
        {
            if (eEQ != _EQ.eEQ.DoorOpen) return;
            if (value == false) return;
            if (p_eState != eState.Move) return;
            if (m_speedNow == null) return;
            if (m_speedNow.m_id != eSpeed.Move.ToString()) return;
            m_speedNow = GetSpeedValue(eSpeed.Move_DoorOpen);
            OverrideVelocity(m_speedNow.m_v);
        }

        #endregion

        #region Shift
        public virtual string StartShift(double dfPos, string sSpeed = null)
        {
            double fPos = p_posCommand + dfPos;
            m_posDst = fPos;
            m_speedNow = (sSpeed != null) ? GetSpeedValue(sSpeed) : GetSpeedValue(EQ.p_bDoorOpen ? eSpeed.Move_DoorOpen : eSpeed.Move);
            m_swMove.Start();
            if (EQ.IsStop()) return p_id + " EQ Stop";
            if (EQ.p_bSimulate) return "OK";
            double dPos = fPos - p_posCommand;
            m_msMoveTime = (int)(1000 * (dPos / m_speedNow.m_v + m_speedNow.m_acc + m_speedNow.m_dec + 1));
            return CheckSWLimit(ref fPos);
        }

        public virtual string StartShift(double dfPos, double v, double acc = -1, double dec = -1)
        {
            double fPos = p_posCommand + dfPos; 
            m_posDst = fPos;
            m_speedNow = null;
            m_swMove.Start();
            if (EQ.IsStop()) return p_id + " EQ Stop";
            if (EQ.p_bSimulate) return "OK";
            double dPos = fPos - p_posCommand;
            m_msMoveTime = (int)(1000 * (dPos / v + acc + dec + 1));
            return CheckSWLimit(ref fPos);
        }
        #endregion

        #region Home
        bool _sensorHome = false; 
        public virtual bool p_sensorHome 
        { 
            get { return _sensorHome; }
            set 
            {
                if (_sensorHome == value) return;
                _sensorHome = value;
                p_log.Info("Sensor Home = " + value.ToString());
                OnPropertyChanged(); 
            }
        }

        public virtual string StartHome()
        {
            switch (p_eState)
            {
                case eState.Home:
                case eState.Move:
                case eState.Jog: return p_id + " StartHome Error, eState = " + p_eState.ToString();
            }
            m_swMove.Start();
            string sStartHome = ResetAlarm();
            ServoOn(true);
            Thread.Sleep(10); 
            //if (p_bServoOn == false) return p_id + " ServoOn Error";
            return "OK";
        }

        bool _bServoOn = false; 
        public bool p_bServoOn 
        { 
            get { return _bServoOn; }
            set
            {
                if (_bServoOn == value) return;
                _bServoOn = value;
                p_log.Info(p_id + " Servo On = " + value.ToString());
                OnPropertyChanged(); 
            }
        }

        public virtual void ServoOn(bool bOn) { }

        public virtual string ResetAlarm() { return "OK"; }
        #endregion

        #region Sensor
        bool _sensorMinusLimit = false;
        public virtual bool p_sensorMinusLimit
        {
            get { return _sensorMinusLimit; }
            set
            {
                if (_sensorMinusLimit == value) return;
                _sensorMinusLimit = value;
                p_log.Info("Sensor Minus Limit = " + value.ToString());
                OnPropertyChanged();
            }
        }

        bool _sensorPlusLimit = false;
        public virtual bool p_sensorPlusLimit
        {
            get { return _sensorPlusLimit; }
            set
            {
                if (_sensorPlusLimit == value) return;
                _sensorPlusLimit = value;
                p_log.Info("Sensor Plus Limit = " + value.ToString());
                OnPropertyChanged();
            }
        }

        bool _sensorInPos = false;
        public virtual bool p_sensorInPos
        {
            get { return _sensorInPos; }
            set
            {
                if (_sensorInPos == value) return;
                _sensorInPos = value;
                p_log.Info("Sensor InPosition = " + value.ToString());
                OnPropertyChanged();
            }
        }

        bool _sensorAlarm = false;
        public virtual bool p_sensorAlarm
        {
            get { return _sensorAlarm; }
            set
            {
                if (_sensorAlarm == value) return;
                _sensorAlarm = value;
                p_log.Info("Sensor Alarm = " + value.ToString());
                OnPropertyChanged();
            }
        }

        bool _sensorEmergency = false;
        public virtual bool p_sensorEmergency
        {
            get { return _sensorEmergency; }
            set
            {
                if (_sensorEmergency == value) return;
                _sensorEmergency = value;
                p_log.Info("Sensor Emergency = " + value.ToString());
                OnPropertyChanged();
            }
        }
      
        #endregion

        #region Trigger
        public class Trigger : ObservableObject
        {
            public double[] m_aPos = new double[2] { 0, 0 };
            public double m_dPos = 10;
            public double p_dPos
            {
                get
                {
                    return m_dPos;
                }
                set
                {
                    SetProperty(ref m_dPos, value);
                }
            }
            public bool m_bCmd = true;
            public double m_dUpTime = -1;
            public double p_dUptime
            {
                get
                {
                    return m_dUpTime;
                }
                set
                {
                    SetProperty(ref m_dUpTime, value);
                }
            }

            public Trigger Clone()
            {
                Trigger trigger = new Trigger();
                trigger.m_aPos[0] = m_aPos[0];
                trigger.m_aPos[1] = m_aPos[1];
                trigger.m_dPos = m_dPos;
                trigger.m_dUpTime = m_dUpTime; 
                trigger.m_bCmd = m_bCmd; 
                return trigger; 
            }

            public void Set(double fPos0, double fPos1, double dPos, bool bCmd, double dUpTime)
            {
                m_aPos[0] = fPos0;
                m_aPos[1] = fPos1;
                m_dPos = dPos;
                m_bCmd = bCmd;
                m_dUpTime = dUpTime; 
            }

            public int GetLine()
            {
                return (int)Math.Round((m_aPos[1] - m_aPos[0]) / m_dPos);
            }

            public void RunTree(Tree tree, string sUnit, bool bVisible = true)
            {
                m_aPos[0] = tree.Set(m_aPos[0], m_aPos[0], "Start", "Start Position (" + sUnit + ")", bVisible);
                m_aPos[1] = tree.Set(m_aPos[1], m_aPos[1], "End", "End Position (" + sUnit + ")", bVisible);
                m_dPos = tree.Set(m_dPos, m_dPos, "Interval", "Trigger Interval (" + sUnit + ")", bVisible);
                m_bCmd = tree.Set(m_bCmd, m_bCmd, "Command Encoder", "use Command Encoder (false = Actual)", bVisible);
            }
        }
        public Trigger m_trigger = new Trigger();
        public Trigger p_Trigger
        {
            get
            {
                return m_trigger;
            }
            set
            {
                m_trigger = value;
                OnPropertyChanged();
            }
        }
        public string m_sUnit = "unit";

        public void SetTrigger(double fPos0, double fPos1, double dPos, bool bCmd)
        {
            m_trigger.Set(fPos0, fPos1, dPos, bCmd, -1);
            RunTrigger(true, m_trigger);
            p_log.Info("SetTrigger : " + dPos.ToString() + ", " + fPos0.ToString() + " ~ " + fPos1.ToString()); 
        }

        public void SetTrigger(double fPos0, double fPos1, double dPos, double dUptime, bool bCmd)
        {
            m_trigger.Set(fPos0, fPos1, dPos, bCmd, dUptime);
            RunTrigger(true, m_trigger);
            p_log.Info("SetTrigger : " + dPos.ToString() + ", " + fPos0.ToString() + " ~ " + fPos1.ToString());
        }

        public virtual void RunTrigger(bool bOn, Trigger trigger = null) { }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot = null;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(p_id, p_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;

            InitPosition();
            InitSpeed();
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
            RunTree(Tree.eMode.Init);
        }

        public virtual void RunTree(Tree.eMode mode) { }

        public TreeRoot m_treeRootSetting = null;
        void InitSetting()
        {
            m_treeRootSetting = new TreeRoot(p_id + ".Setting", p_log);
            m_treeRootSetting.UpdateTree += M_treeRootSetting_UpdateTree;
            
            RunTreeSetting(Tree.eMode.RegRead);
            RunTreeSetting(Tree.eMode.Update);
        }

        private void M_treeRootSetting_UpdateTree()
        {
            RunTreeSetting(Tree.eMode.Update);
            RunTreeSetting(Tree.eMode.RegWrite);
            RunTreeSetting(Tree.eMode.Init);
        }

        public virtual void RunTreeSetting(Tree.eMode mode) { }

        public TreeRoot m_treeRootInterlock = null;
        void InitInterlock()
        {
            m_treeRootInterlock = new TreeRoot(p_id + ".Interlock", p_log);
            m_treeRootInterlock.UpdateTree += M_treeRootInterlock_UpdateTree;
            RunTreeInterlock(Tree.eMode.RegRead);
        }

        private void M_treeRootInterlock_UpdateTree()
        {
            RunTreeInterlock(Tree.eMode.Update);
            RunTreeInterlock(Tree.eMode.RegWrite);
            RunTreeInterlock(Tree.eMode.Init);
        }

        public virtual void RunTreeInterlock(Tree.eMode mode) { }
        #endregion

        #region RelayCommand
        private void MJogFast()
        {
            if (p_eState != Axis.eState.Ready) return;
            Jog(-0.31, eSpeed.Jog.ToString());
        }

        private void PJogFast()
        {
            if (p_eState != Axis.eState.Ready) return;
            Jog(0.31, eSpeed.Jog.ToString());
        }

        private void JogFinish()
        {
            if (p_eState != Axis.eState.Jog) return;
            StopAxis(true);
        }

        private void Move()
        {
            if (p_eState != Axis.eState.Ready) return;
            StartMove(p_strSelPos, 0, eSpeed.Move.ToString());
        }

        private void MRelativeMove()
        {
            int nDir = -1;
            if (p_eState != Axis.eState.Ready) return;
            try
            {
                double dPos = Convert.ToInt32(p_dRelPos);
                double fPos = p_posCommand + (nDir * dPos);
                StartMove(fPos, eSpeed.Move.ToString());
            }
            catch (Exception) { }
        }

        private void PRelativeMove()
        {
            int nDir = 1;
            if (p_eState != Axis.eState.Ready) return;
            try
            {
                double dPos = Convert.ToInt32(p_dRelPos);
                double fPos = p_posCommand + (nDir * dPos);
                StartMove(fPos, eSpeed.Move.ToString());
            }
            catch (Exception) { }
        }

        public RelayCommand MJogFastCommand
        {
            get
            {
                return new RelayCommand(MJogFast);
            }
            set
            {
            }
        }
        public RelayCommand PJogFastCommand
        {
            get
            {
                return new RelayCommand(PJogFast);
            }
            set
            {
            }
        }
        public RelayCommand JogFinishCommand
        {
            get
            {
                return new RelayCommand(JogFinish);
            }
            set
            {
            }
        }

        public RelayCommand MoveCommand
        {
            get
            {
                return new RelayCommand(Move);
            }
            set
            {
            }
        }

        public RelayCommand MRelativeMoveCommand
        {
            get
            {
                return new RelayCommand(MRelativeMove);
            }
            set
            {
            }
        }

        public RelayCommand PRelativeMoveCommand
        {
            get
            {
                return new RelayCommand(PRelativeMove);
            }
            set
            {
            }
        }
        #endregion

        #region Compensation
        /*Compensation Table 사용 순서 : Compensation Set -> Compensation Enable IsEnable*/
        /* 축 설정하는거 enable이 안되면 축설정 할라고 일단 놔둠
            a = CAXM.AxmStatusSetActPos(2, 0);
            b = CAXM.AxmStatusSetCmdPos(2, 0);
            a = CAXM.AxmStatusSetCmdPos(0, 0);
            b = CAXM.AxmStatusSetActPos(0, 0);
            uint r = CAXM.AxmStatusSetPosType(2, 1, 7044215, 0);
            r = CAXM.AxmStatusSetPosType(0, 1, 5015422, 0);
            r= CAXM.AxmStatusGetPosType(2, ref aa,ref a,ref b);
            m_BST.m_LM.WriteLog(LOG.DATAPROCESS, "    ");
         */

        public virtual void SetAxis()
        {

        }
        public virtual bool CompensationSet(double startpos, double[] Position, double[] dpCorrection)
        {
            return false;
        }
        public virtual bool EnableCompensation(int isenable)
        {
            return false;
        }
        #endregion
        protected void InitBase(string id, Log log)
        {
            p_id = id;
            p_log = log;
            InitTree(); 
            InitSetting();
            InitInterlock();
            EQ.m_EQ.OnChanged += M_EQ_OnChanged;
        }
    }
}
