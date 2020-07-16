﻿using RootTools.Module;
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
        public virtual void OverrideVelocity(double v) { }
        #endregion

        #region List Position
        Dictionary<string, double> m_aPos = new Dictionary<string, double>();
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

        public void RunTreePos(Tree tree)
        {
            RunTreePosLimit(tree.GetTree("SW Limit", false));
            RunTreePosition(tree.GetTree("Position"));
        }

        void RunTreePosition(Tree tree)
        {
            m_aPos[p_asPos[0]] = tree.Set(m_aPos[p_asPos[0]], 0, p_asPos[0], "Axis Position (pulse)", m_bSWLimit[0]);
            m_aPos[p_asPos[1]] = tree.Set(m_aPos[p_asPos[1]], 0, p_asPos[1], "Axis Position (pulse)", m_bSWLimit[1]);
            for (int n = 2; n < p_asPos.Count; n++)
            {
                m_aPos[p_asPos[n]] = tree.Set(m_aPos[p_asPos[n]], 0, p_asPos[n], "Axis Position (pulse)");
            }
        }
        #endregion

        #region SW Limit
        bool[] m_bSWLimit = new bool[2] { false, false };

        protected string CheckSWLimit(ref double fPosDst)
        {
            if (EQ.p_bSimulate) return "OK";
            double fPosNow = p_posCommand;
            double fPosMinusLimit = m_aPos[p_asPos[0]];
            if (m_bSWLimit[0])
            {
                if (fPosDst >= fPosMinusLimit) return "OK";
                if (fPosNow < fPosMinusLimit) return "SW Minus Limit Error : " + fPosDst.ToString();
                fPosDst = fPosMinusLimit;
                return "OK";
            }
            double fPosPlusLimit = m_aPos[p_asPos[1]];
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
            if (m_bSWLimit[0] && (vJog < 0) && (fPosNow <= m_aPos[p_asPos[0]])) return "SW Minus Limit Error";
            if (m_bSWLimit[1] && (vJog > 0) && (fPosNow >= m_aPos[p_asPos[1]])) return "SW Plus Limit Error";
            return "OK";
        }

        void ThreadCheck_SWLimit()
        {
            double fPos = p_posActual;
            bool bSWLimit0 = m_bSWLimit[0] && (fPos > m_aPos[p_asPos[0]]);
            if (bSWLimit0) p_sInfo = p_id + ": Servo SW limit(-) !!";
            bool bSWLimit1 = m_bSWLimit[1] && (fPos > m_aPos[p_asPos[1]]);
            if (bSWLimit1) p_sInfo = p_id + ": Servo SW limit(+) !!";
            if (bSWLimit0 || bSWLimit1) StopAxis();
        }

        void RunTreePosLimit(Tree tree)
        {
            m_bSWLimit[0] = tree.Set(m_bSWLimit[0], m_bSWLimit[0], "Minus", "Use SW Minus Limit");
            m_bSWLimit[1] = tree.Set(m_bSWLimit[1], m_bSWLimit[1], "Plus", "Use SW Plus Limit");
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

        public void RunTreeSpeed(Tree tree)
        {
            foreach (Speed speed in m_aSpeed) speed.RunTree(tree.GetTree(speed.m_id, false));
        }
        #endregion

        #region Jog
        protected Speed m_speedNow;
        protected StopWatch m_swMove = new StopWatch();
        public int p_fJogScale { get; set; }

        public virtual string Jog(double fScale, string sSpeed = null)
        {
            m_speedNow = (sSpeed != null) ? GetSpeedValue(sSpeed) : GetSpeedValue(eSpeed.Move);
            m_swMove.Start();
            if (EQ.IsStop()) return p_id + " EQ Stop";
            if (EQ.p_bSimulate) return "OK";
            if (p_eState != eState.Ready) return p_id + " Axis State not Ready : " + p_eState.ToString();
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

        double m_posDst = 0;
        int m_msMoveTime = 0;
        public virtual string StartMove(double fPos, string sSpeed = null)
        {
            m_posDst = fPos;
            m_speedNow = (sSpeed != null) ? GetSpeedValue(sSpeed) : GetSpeedValue(EQ.p_bDoorOpen ? eSpeed.Move_DoorOpen : eSpeed.Move);
            m_swMove.Start();
            if (EQ.IsStop()) return p_id + " EQ Stop";
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

        public string WaitReady(double dInPos = -1)
        {
            while (p_eState == eState.Move || p_eState == eState.Home)
            {
                Thread.Sleep(10);
                //if (m_swMove.ElapsedMilliseconds > m_msMoveTime)
                //{
                //    p_eState = eState.Init;
                //    return p_id + " Axis Move Timeout";
                //}
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
        bool _sensorHome = false; 
        public virtual bool p_sensorHome 
        { 
            get { return _sensorHome; }
            set 
            {
                if (_sensorHome == value) return;
                _sensorHome = value;
                p_sInfo = "Sensor Home = " + value.ToString();
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
            if (p_bSeroOn == false) return p_id + " ServoOn Error";
            return "OK";
        }

        bool _bServoOn = false; 
        public bool p_bSeroOn 
        { 
            get { return _bServoOn; }
            set
            {
                if (_bServoOn == value) return;
                _bServoOn = value;
                p_sInfo = p_id + " Servo On = " + value.ToString();
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
                p_sInfo = "Sensor Minus Limit = " + value.ToString();
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
                p_sInfo = "Sensor Plus Limit = " + value.ToString();
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
                p_sInfo = "Sensor InPosition = " + value.ToString();
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
                p_sInfo = "Sensor Alarm = " + value.ToString();
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
                p_sInfo = "Sensor Emergency = " + value.ToString();
                OnPropertyChanged();
            }
        }
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
        }

        private void M_treeRootSetting_UpdateTree()
        {
            RunTreeSetting(Tree.eMode.Update);
            RunTreeSetting(Tree.eMode.RegWrite);
            RunTreeSetting(Tree.eMode.Init);
        }

        public virtual void RunTreeSetting(Tree.eMode mode) { }
        #endregion

        #region RelayCommand
        private void MJogFast()
        {
            if (p_eState > Axis.eState.Ready) return;
            Jog(-0.31, eSpeed.Jog.ToString());
        }

        private void PJogFast()
        {
            if (p_eState > Axis.eState.Ready) return;
            Jog(0.31, eSpeed.Jog.ToString());
        }

        private void JogFinish()
        {
            if (p_eState != Axis.eState.Jog) return;
            StopAxis(true);
        }

        private void Move()
        {
            if (p_eState > Axis.eState.Ready) return;
            StartMove(p_strSelPos, 0, eSpeed.Move.ToString());
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
        #endregion

        protected void InitBase(string id, Log log)
        {
            p_id = id;
            p_log = log;
            InitTree(); 
            InitSetting();
            EQ.m_EQ.OnDoorOpen += M_EQ_OnDoorOpen;
        }
    }
}
