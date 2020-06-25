using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

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
                p_log.Info(p_sName + " State : " + _eState.ToString() + " -> " + value.ToString());
                _eState = value;
                OnPropertyChanged();
            }
        }

        public delegate void dgOnChangeAxis();
        public event dgOnChangeAxis OnChangeAxis;

        string _sName = "";
        public string p_sName
        {
            get { return _sName; }
            set
            {
                if (_sName == value) return;
                _sName = value;
                OnPropertyChanged(); 
                if (OnChangeAxis != null) OnChangeAxis(); 
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

        Log _log; 
        public Log p_log
        {
            get { return _log; }
            set
            {
                _log = value;
                if (m_treeRootPos != null) m_treeRootPos.m_log = value;
                if (m_treeRootSpeed != null) m_treeRootSpeed.m_log = value;
                if (m_treeRootSetting != null) m_treeRootSetting.m_log = value;
            }
        }
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
        public virtual void OverrideVelocity(double v) { }
        #endregion

        #region List Position
        Dictionary<string, double> m_aPos = new Dictionary<string, double>();
        public List<string> m_asPos = new List<string>();

        public void AddPos(params dynamic[] aPos)
        {
            foreach (dynamic value in aPos) AddPos(value.ToString());
            RunTreePos(Tree.eMode.RegRead);
            RunTreePos(Tree.eMode.Init);
        }

        void AddPos(string sPos)
        {
            foreach (string sKey in m_aPos.Keys)
            {
                if (sKey == sPos) return;
            }
            m_aPos.Add(sPos, 0);
            m_asPos.Add(sPos);
        }

        public double GetPosValue(Enum pos)
        {
            return GetPosValue(pos.ToString());
        }

        public double GetPosValue(string sPos)
        {
            foreach (string sKey in m_asPos)
            {
                if (sKey == sPos) return m_aPos[sPos];
            }
            return 0;
        }

        public void SetPositionValue(string sPos, bool bCommand = true)
        {
            foreach (string sKey in m_asPos)
            {
                if (sKey == sPos)
                {
                    m_aPos[sPos] = bCommand ? p_posCommand : p_posActual;
                    RunTreePos(Tree.eMode.RegWrite);
                    RunTreePos(Tree.eMode.Init);
                }
            }
        }

        public bool IsInPos(Enum pos, double posError = 10)
        {
            double dPos = GetPosValue(pos) - p_posCommand;
            return Math.Abs(dPos) <= posError;
        }

        public enum ePosition
        {
            SWLimit_Minus,
            SWLimit_Plus,
            Position_0,
            Position_1,
            Position_2,
        }
        public TreeRoot m_treeRootPos = null;
        void InitPosition()
        {
            m_treeRootPos = new TreeRoot(p_id + ".Position", p_log);
            m_treeRootPos.UpdateTree += M_treeRootPos_UpdateTree;
            for (int i = 0; i < Enum.GetNames(typeof(ePosition)).Length; i++)
            {
                AddPos(((ePosition)i).ToString());
            }
            RunTreePos(Tree.eMode.RegRead);
        }

        private void M_treeRootPos_UpdateTree()
        {
            RunTreePos(Tree.eMode.Update);
            RunTreePos(Tree.eMode.RegWrite);
            RunTreePos(Tree.eMode.Init);
        }

        void RunTreePos(Tree.eMode mode)
        {
            m_treeRootPos.p_eMode = mode;
            RunTreePosLimit(m_treeRootPos.GetTree("SW Limit"));
            RunTreePos(m_treeRootPos.GetTree("Position"));
        }

        void RunTreePos(Tree tree)
        {
            m_aPos[m_asPos[0]] = tree.Set(m_aPos[m_asPos[0]], 0, m_asPos[0], "Axis Position (pulse)", m_bSWLimit[0]);
            m_aPos[m_asPos[1]] = tree.Set(m_aPos[m_asPos[1]], 0, m_asPos[1], "Axis Position (pulse)", m_bSWLimit[1]);
            for (int n = 2; n < m_asPos.Count; n++)
            {
                m_aPos[m_asPos[n]] = tree.Set(m_aPos[m_asPos[n]], 0, m_asPos[n], "Axis Position (pulse)");
            }
        }
        #endregion

        #region SW Limit
        bool[] m_bSWLimit = new bool[2] { false, false };

        protected string CheckSWLimit(ref double fPosDst)
        {
            if (EQ.p_bSimulate) return "OK";
            double fPosNow = p_posCommand;
            double fPosMinusLimit = m_aPos[m_asPos[0]];
            if (m_bSWLimit[0])
            {
                if (fPosDst >= fPosMinusLimit) return "OK";
                if (fPosNow < fPosMinusLimit) return "SW Minus Limit Error : " + fPosDst.ToString();
                fPosDst = fPosMinusLimit;
                return "OK";
            }
            double fPosPlusLimit = m_aPos[m_asPos[1]];
            if (m_bSWLimit[1])
            {
                if (fPosDst <= fPosPlusLimit) return "OK";
                if (fPosNow > fPosPlusLimit) return "SW Plus Limit Error : " + fPosDst.ToString();
                fPosDst = fPosPlusLimit;
                return "OK";
            }
            return "OK";
        }

        string CheckSWLimit(double vJog)
        {
            if (vJog == 0) return "OK";
            double fPosNow = p_posCommand;
            if (m_bSWLimit[0] && (vJog < 0) && (fPosNow <= m_aPos[m_asPos[0]])) return "SW Minus Limit Error";
            if (m_bSWLimit[1] && (vJog > 0) && (fPosNow >= m_aPos[m_asPos[2]])) return "SW Plus Limit Error";
            return "OK";
        }

        void ThreadCheck_SWLimit()
        {
            double fPos = p_posActual;
            bool bSWLimit0 = m_bSWLimit[0] && (fPos > m_aPos[m_asPos[0]]);
            if (bSWLimit0) p_sInfo = p_sName + ": Servo SW limit(-) !!";
            bool bSWLimit1 = m_bSWLimit[1] && (fPos > m_aPos[m_asPos[1]]);
            if (bSWLimit1) p_sInfo = p_sName + ": Servo SW limit(+) !!";
            if (bSWLimit0 || bSWLimit1) StopAxis();
        }

        void RunTreePosLimit(Tree tree)
        {
            m_bSWLimit[0] = tree.Set(m_bSWLimit[0], m_bSWLimit[0], "Minus", "Use SW Minus Limit");
            m_bSWLimit[1] = tree.Set(m_bSWLimit[1], m_bSWLimit[1], "Minus", "Use SW Minus Limit");
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

            public void RunTree(Tree tree)
            {
                m_v = tree.Set(m_v, m_v, "Velociry", "Axis Moving Velocity (pulse / sec)");
                m_acc = tree.Set(m_acc, m_acc, "Accelation", "Accelation Time (sec)");
                m_dec = tree.Set(m_dec, m_dec, "Decelation", "Decelation Time (sec)");
            }
        }
        List<Speed> m_aSpeed = new List<Speed>();
        public List<string> m_asSpeed = new List<string>();

        public void AddSpeed(params dynamic[] aSpeed)
        {
            foreach (dynamic value in aSpeed) AddSpeed(value.ToString());
            RunTreeSpeed(Tree.eMode.RegRead);
            RunTreeSpeed(Tree.eMode.Init);
        }

        void AddSpeed(string sSpeed)
        {
            foreach (string sKey in m_asSpeed)
            {
                if (sKey == sSpeed) return;
            }
            m_aSpeed.Add(new Speed(sSpeed));
            m_asSpeed.Add(sSpeed);
        }

        protected Speed GetSpeedValue(Enum speed)
        {
            return GetSpeedValue(speed.ToString());
        }

        Speed GetSpeedValue(string sSpeed)
        {
            foreach (Speed speed in m_aSpeed)
            {
                if (speed.m_id == sSpeed) return speed;
            }
            return null;
        }

        public enum eSpeed
        {
            Jog,
            Move,
            Move_DoorOpen,
        }
        public TreeRoot m_treeRootSpeed = null;
        void InitSpeed()
        {
            p_fJogScale = 100; 
            m_treeRootSpeed = new TreeRoot(p_id + ".Speed", p_log);
            m_treeRootSpeed.UpdateTree += M_treeRootSpeed_UpdateTree; 
            for (int i = 0; i < Enum.GetNames(typeof(eSpeed)).Length; i++)
            {
                AddSpeed(((eSpeed)i).ToString());
            }
            RunTreeSpeed(Tree.eMode.RegRead);
        }

        private void M_treeRootSpeed_UpdateTree()
        {
            RunTreeSpeed(Tree.eMode.Update);
            RunTreeSpeed(Tree.eMode.RegWrite);
            RunTreeSpeed(Tree.eMode.Init);
        }

        void RunTreeSpeed(Tree.eMode mode)
        {
            m_treeRootSpeed.p_eMode = mode;
            foreach (Speed speed in m_aSpeed) speed.RunTree(m_treeRootSpeed.GetTree(speed.m_id, false)); 
        }
        #endregion

        #region Jog
        protected Speed m_speedNow;
        protected StopWatch m_swMove = new StopWatch();
        public int p_fJogScale { get; set; }

        public virtual string Jog(double fScale, Speed speed = null)
        {
            m_speedNow = (speed == null) ? GetSpeedValue(eSpeed.Move) : speed;
            m_swMove.Start();
            if (EQ.IsStop()) return p_sName + " EQ Stop";
            if (EQ.p_bSimulate) return "OK";
            if (p_eState != eState.Ready) return p_sName + " Axis State not Ready : " + p_eState.ToString();
            return CheckSWLimit(fScale * m_speedNow.m_v);
        }

        public virtual void StopAxis(bool bSlowStop = true) { }
        #endregion

        #region Move
        public string StartMove(Enum pos, double fOffset = 0, Enum speed = null)
        {
            return StartMove(pos.ToString(), fOffset, speed); 
        }

        public string StartMove(string sPos, double fOffset = 0, Enum speed = null)
        {
            double fPos = GetPosValue(sPos) + fOffset;
            return StartMove(fPos, (speed == null) ? null : speed.ToString());
        }

        double m_posDst = 0;
        int m_msMoveTime = 0;
        public virtual string StartMove(double fPos, string sSpeed = null)
        {
            m_posDst = fPos;
            m_speedNow = (sSpeed == null) ? GetSpeedValue(eSpeed.Move) : GetSpeedValue(sSpeed);
            m_swMove.Start();
            if (EQ.IsStop()) return p_sName + " EQ Stop";
            if (EQ.p_bSimulate) return "OK";
            if (p_eState != eState.Ready) return p_sName + " Axis State not Ready : " + p_eState.ToString();
            double dPos = fPos - p_posCommand;
            m_msMoveTime = (int)(1000 * (dPos / m_speedNow.m_v + m_speedNow.m_acc + m_speedNow.m_dec + 1));
            return CheckSWLimit(ref fPos);
        }

        public virtual string StartMove(double fPos, double v, double acc = -1, double dec = -1)
        {
            m_posDst = fPos;
            m_speedNow = null;
            m_swMove.Start();
            if (EQ.IsStop()) return p_sName + " EQ Stop";
            if (EQ.p_bSimulate) return "OK";
            if (p_eState != eState.Ready) return p_sName + " Axis State not Ready : " + p_eState.ToString();
            double dPos = fPos - p_posCommand;
            m_msMoveTime = (int)(1000 * (dPos / v + acc + dec + 1));
            return CheckSWLimit(ref fPos);
        }

        public string WaitReady(double dInPos = -1)
        {
            while (p_eState == eState.Move || p_eState == eState.Home)
            {
                Thread.Sleep(10);
                if (m_swMove.ElapsedMilliseconds > m_msMoveTime)
                {
                    p_eState = eState.Init;
                    return p_sName + " Axis Move Timeout";
                }
            }
            switch (p_eState)
            {
                case eState.Ready:
                    if (dInPos >= 0)
                    {
                        double dPos = m_posDst - p_posActual;
                        if (Math.Abs(dPos) > dInPos) return "WaitReady InPosition Error : " + dPos.ToString();
                    }
                    return "OK";
                default: return "WaitReady Error p_eState = " + p_eState.ToString();
            }
        }

        private void M_EQ_OnDoorOpen()
        {
            if (p_eState != eState.Move) return;
            if (m_speedNow == null) return; 
            if (m_speedNow.m_id != eSpeed.Move.ToString()) return;
            m_speedNow = GetSpeedValue(eSpeed.Move_DoorOpen); 
            OverrideVelocity(m_speedNow.m_v);
        }
        #endregion

        #region Home
        public virtual string StartHome()
        {
            switch (p_eState)
            {
                case eState.Home:
                case eState.Move:
                case eState.Jog: return p_sName + " StartHome Error, eState = " + p_eState.ToString();
            }
            m_swMove.Start();
            string sStartHome = ResetAlarm();
            p_bServoOn = true;
            if (p_bServoOn == false) return p_sName + " ServoOn Error";
            return "OK";
        }

        public virtual bool p_bServoOn { get; set; }

        public virtual string ResetAlarm() { return "OK"; }
        #endregion

        #region Setting
        public TreeRoot m_treeRootSetting = null;
        void InitSetting()
        {
            m_treeRootSetting = new TreeRoot(p_id + ".Setting", p_log);
            m_treeRootSetting.UpdateTree += M_treeRootSetting_UpdateTree;
            RunTreeSetting(Tree.eMode.RegRead); 
        }

        private void M_treeRootSetting_UpdateTree()
        {
            RunTreeSetting(Tree.eMode.Update);
            RunTreeSetting(Tree.eMode.RegWrite);
            RunTreeSetting(Tree.eMode.Init);
        }

        public virtual void RunTreeSetting(Tree.eMode mode) { }
        #endregion

        #region Trigger
        protected class Trigger
        {
            public double[] m_aPos = new double[2] { 0, 0 };
            public double m_dPos = 10;
            public bool m_bCmd = true;

            public void Set(double fPos0, double fPos1, double dPos, bool bCmd)
            {
                m_aPos[0] = fPos0;
                m_aPos[1] = fPos1;
                m_dPos = dPos;
                m_bCmd = bCmd;
            }

            public void RunTree(Tree tree)
            {
                m_aPos[0] = tree.Set(m_aPos[0], m_aPos[0], "Start", "Start Position (pulse)");
                m_aPos[1] = tree.Set(m_aPos[1], m_aPos[1], "End", "End Position (pulse)");
                m_dPos = tree.Set(m_dPos, m_dPos, "Interval", "Trigger Interval (pulse)");
                m_bCmd = tree.Set(m_bCmd, m_bCmd, "Command Encoder", "use Command Encoder (false = Actual)");
            }
        }
        protected Trigger m_trigger = new Trigger(); 

        public void SetTrigger(double fPos0, double fPos1, double dPos, bool bCmd)
        {
            m_trigger.Set(fPos0, fPos1, dPos, bCmd);
            RunTrigger(true); 
        }

        public virtual void RunTrigger(bool bOn) { }
        #endregion

        public void Init(string id, Log log)
        {
            p_id = id;
            p_log = log;
            InitPosition();
            InitSpeed();
            InitSetting();
            EQ.m_EQ.OnDoorOpen += M_EQ_OnDoorOpen;
        }
    }
}
