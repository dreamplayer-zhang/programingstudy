using RootTools.Comm;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.Control.Xenax
{
    public class XenaxAxis : Axis
    {
        #region Property
        double _pulsepUnit = 1;
        double p_pulsepUnit
        {
            get { return _pulsepUnit; }
            set
            {
                if (_pulsepUnit == value) return;
                double fRatio = _pulsepUnit / value;
                foreach (string sKey in p_asPos) m_aPos[sKey] *= fRatio;
                foreach (Speed speed in m_aSpeed) speed.m_v *= fRatio;
                m_trigger.m_aPos[0] *= fRatio;
                m_trigger.m_aPos[1] *= fRatio;
                m_trigger.m_dPos *= fRatio;
                _pulsepUnit = value;
                RunTree(Tree.eMode.Init);
                RunTree(Tree.eMode.RegWrite);
            }
        }

        bool m_bAbsoluteEncoder = false;
        void RunTreeSettingProperty(Tree tree)
        {
            m_sUnit = tree.Set(m_sUnit, m_sUnit, "Unit", "Xenax Axis Unit");
            double pulseUnit = tree.Set(p_pulsepUnit, p_pulsepUnit, "Pulse/Unit", "Pulse / Unit");
            if (tree.p_treeRoot.p_eMode != Tree.eMode.RegRead) p_pulsepUnit = pulseUnit;
            m_bAbsoluteEncoder = tree.Set(m_bAbsoluteEncoder, m_bAbsoluteEncoder, "Absolute Encoder", "Absolute Encoder");
        }
        #endregion

        #region Serial Comm
        public enum eComm
        {
            Disable,
            RS232,
            TCPIP,
        }
        eComm _eComm = eComm.Disable; 
        public eComm p_eComm
        {
            get { return _eComm; }
            set
            {
                if (_eComm == value) return;
                _eComm = value;
                OnPropertyChanged();
                InitComm(); 
            }
        }

        UserControl _commUI = null;
        public UserControl p_commUI
        {
            get { return _commUI; }
            set
            {
                _commUI = value;
                OnPropertyChanged(); 
            }
        }

        IComm m_comm = null;
        void InitComm()
        {
            switch (p_eComm)
            {
                case eComm.RS232:
                    RS232 rs232 = new RS232(p_id + ".RS232", p_log);
                    m_comm = rs232;
                    p_commUI = rs232.p_ui;
                    rs232.OnReceive += Rs232_OnReceive;
                    rs232.p_bConnect = true;
                    break;
                case eComm.TCPIP:
                    TCPIPClient tcpip = new TCPIPClient(p_id + ".TCPIP", p_log);
                    m_comm = tcpip;
                    p_commUI = tcpip.p_ui; 
                    tcpip.EventReciveData += Tcpip_EventReciveData;
                    break;
                default: 
                    m_comm = null;
                    p_commUI = null; 
                    break; 
            }
        }

        private void Rs232_OnReceive(string sRead)
        {
            p_sInfo = OnReceive(sRead); 
        }

        private void Tcpip_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            string sRead = Encoding.Default.GetString(aBuf, 0, nSize);
            p_sInfo = OnReceive(sRead);
        }

        void RunTreeSettingComm(Tree tree)
        {
            p_eComm = (eComm)tree.Set(p_eComm, p_eComm, "Type", "Communication Type");
        }
        #endregion

        #region Protocol
        public enum eCmd
        {
            None,
            HomeDir,
            Home,
            Move,
            JogPlus,
            JogMius,
            JogStop,
            GetPos,
            GetState,
            SetSpeed,
            ServoOn,
            ServoOff,
            Event,
            GetErrorCode,
            GetErrorString,
            SoftLimitLeft,
            SoftLimitRight,
            ML,
        }
        public Dictionary<eCmd, string> m_aCmd = new Dictionary<eCmd, string>();
        void InitCmd()
        {
            m_aCmd.Add(eCmd.None, "None");
            m_aCmd.Add(eCmd.HomeDir, "DRHR"); 
            m_aCmd.Add(eCmd.Home, "REF"); 
            m_aCmd.Add(eCmd.Move, "G"); 
            m_aCmd.Add(eCmd.JogPlus, "JP");
            m_aCmd.Add(eCmd.JogMius, "JM");
            m_aCmd.Add(eCmd.JogStop, "SM");
            m_aCmd.Add(eCmd.GetPos, "TP");
            m_aCmd.Add(eCmd.GetState, "TS");
            m_aCmd.Add(eCmd.SetSpeed, "SP");
            m_aCmd.Add(eCmd.ServoOn, "PWC");
            m_aCmd.Add(eCmd.ServoOff, "PQ");
            m_aCmd.Add(eCmd.Event, "EVT1");
            m_aCmd.Add(eCmd.GetErrorCode, "TE");
            m_aCmd.Add(eCmd.GetErrorString, "TES");
            m_aCmd.Add(eCmd.SoftLimitLeft, "LL");
            m_aCmd.Add(eCmd.SoftLimitRight, "LR");
            m_aCmd.Add(eCmd.ML, "ML");
        }

        eCmd GetCmd(string sRead, ref string sData)
        {
            foreach (eCmd eCmd in m_aCmd.Keys)
            {
                string sCmd = m_aCmd[eCmd]; 
                if (sRead.Length >= sCmd.Length)
                {
                    if (sCmd == sRead.Substring(0, sCmd.Length))
                    {
                        sData = sRead.Substring(sCmd.Length, sRead.Length - sCmd.Length);
                        sData = sData.Replace("\n", "");
                        sData = sData.Replace("\r", "");
                        return eCmd;
                    }
                }
            }
            return eCmd.None; 
        }

        public enum eEvent
        {
            None,
            PowerOff,
            Inposition,
            InMotion,
            Error,
            HomeDone
        }
        public Dictionary<eEvent, string> m_aEvent = new Dictionary<eEvent, string>();
        void InitEvent()
        {
            m_aEvent.Add(eEvent.None, "@N");
            m_aEvent.Add(eEvent.PowerOff, "@S0");
            m_aEvent.Add(eEvent.Inposition, "@S1");
            m_aEvent.Add(eEvent.InMotion, "@S2");
            m_aEvent.Add(eEvent.Error, "@S9");
            m_aEvent.Add(eEvent.HomeDone, "@H");
        }

        eEvent GetEvent(string sRead)
        {
            foreach (eEvent eEvent in m_aEvent.Keys)
            {
                if (sRead.Contains(m_aEvent[eEvent])) return eEvent; 
            }
            return eEvent.None; 
        }

        public class Protocol
        {
            public string m_sSend = "";
            public string m_sCmd = "";
            public Protocol(string sCmd)
            {
                m_sCmd = sCmd;
                m_sSend = sCmd + "\r";
            }

            public Protocol(string sCmd, int nValue)
            {
                m_sCmd = sCmd; 
                m_sSend = sCmd + nValue.ToString() + "\r";
            }
        }
        Protocol m_protocolSend = null;
        Queue<Protocol> m_qProtocol = new Queue<Protocol>();

        string m_sErrorCode = ""; 
        string OnReceive(string sRead)
        {
            sRead = sRead.ToUpper();
            if (sRead[0] == '@') return OnReceiveEvent(sRead); 
            if (m_protocolSend != null)
            {
                string sData = "";
                eCmd eCmd = GetCmd(sRead, ref sData);
                switch (eCmd)
                {
                    case eCmd.GetPos: OnRecieveGetPos(ConvInt(sData)); break;
                    case eCmd.GetErrorCode: m_sErrorCode = sData; break;
                    case eCmd.GetErrorString: p_sInfo = "Xenax Error : " + sData + " (" + m_sErrorCode + ")"; break;
                }
                m_protocolSend = null;
            }
            return "OK";
        }

        int ConvInt(string sData)
        {
            try { return Convert.ToInt32(sData); }
            catch (Exception e) { p_sInfo = "GetInt32 Error : " + sData + ", " + e.Message; }
            return 0; 
        }

        string OnReceiveEvent(string sRead)
        {
            switch (GetEvent(sRead))
            {
                case eEvent.PowerOff: 
                    p_bServoOn = false; 
                    break;
                case eEvent.Inposition:
                    p_bServoOn = true;
                    p_sensorInPos = true; 
                    if ((p_eState == eState.Jog) || (p_eState == eState.Move)) p_eState = eState.Ready; 
                    break;
                case eEvent.InMotion:
                    p_sensorInPos = false; 
                    break;
                case eEvent.HomeDone:
                    /*if (p_eState == eState.Home) */p_eState = eState.Ready;
                    break;
                case eEvent.Error:
                    p_eState = eState.Error;
                    m_qProtocol.Enqueue(new Protocol(m_aCmd[eCmd.GetErrorCode]));
                    m_qProtocol.Enqueue(new Protocol(m_aCmd[eCmd.GetErrorString]));
                    p_sensorAlarm = true;
                    p_sensorEmergency = true; 
                    break; 
            }
            return "OK"; 
        }
        #endregion

        #region Position & Velocity
        public override void SetCommandPosition(double fPos) { }
        public override void SetActualPosition(double fPos) { }
        public override void OverrideVelocity(double v) { }

        void AddGetPos()
        {
            m_qProtocol.Enqueue(new Protocol(m_aCmd[eCmd.GetPos]));
        }

        void OnRecieveGetPos(int nPos)
        {
            p_posActual = nPos;
            p_posCommand = nPos;
        }

        int m_nSpeed = 0; 
        void AddSpeed(double fSpeed)
        {
            int nSpeed = (int)Math.Abs(fSpeed); 
            if (nSpeed == m_nSpeed) return;
            m_nSpeed = nSpeed; 
            m_qProtocol.Enqueue(new Protocol(m_aCmd[eCmd.SetSpeed], nSpeed)); 
        }
        #endregion

        #region Jog
        public override string Jog(double fScale, string sSpeed = null)
        {
            p_log.Info(p_id + " Jog Start : " + fScale.ToString());
            if (IsInterlock()) return p_id + m_sCheckInterlock;
            p_sInfo = base.Jog(fScale, sSpeed);
            if (p_sInfo != "OK") return p_sInfo;
            AddSpeed(fScale * m_speedNow.m_v * p_pulsepUnit);
            m_qProtocol.Enqueue(new Protocol(m_aCmd[(fScale > 0) ? eCmd.JogPlus : eCmd.JogMius]));
            p_eState = eState.Jog; 
            return "OK";
        }

        public override void StopAxis(bool bSlowStop = true)
        {
            m_qProtocol.Enqueue(new Protocol(m_aCmd[eCmd.JogStop]));
        }
        #endregion

        #region Move
        public override string StartMove(double fPos, string sSpeed = null)
        {
            if (IsInterlock()) return p_id + m_sCheckInterlock;
            //p_sInfo = base.StartMove(fPos, sSpeed);
            //if (p_sInfo != "OK") return p_sInfo;
            //AddSpeed(m_speedNow.m_v * p_pulsepUnit);
            m_qProtocol.Enqueue(new Protocol(m_aCmd[eCmd.Move], (int)(fPos * p_pulsepUnit)));
            p_eState = eState.Move; 
            Thread.Sleep(10);
            return "OK";
        }

        public override string StartMove(double fPos, double v, double acc = -1, double dec = -1)
        {
            if (IsInterlock()) return p_id + m_sCheckInterlock;
            acc = (acc < 0) ? GetSpeedValue(eSpeed.Move).m_acc : acc;
            dec = (dec < 0) ? GetSpeedValue(eSpeed.Move).m_dec : dec;
            p_sInfo = base.StartMove(fPos, v, acc, dec);
            if (p_sInfo != "OK") return p_sInfo;
            AddSpeed(v * p_pulsepUnit);
            m_qProtocol.Enqueue(new Protocol(m_aCmd[eCmd.Move], (int)(fPos * p_pulsepUnit)));
            p_eState = eState.Move; 
            Thread.Sleep(10);
            return "OK";
        }

        public override string StartMoveV(double vStart, double posAt, double vChange, double posTo, double acc = -1, double dec = -1)
        {
            if (IsInterlock()) return p_id + m_sCheckInterlock;
            acc = (acc < 0) ? GetSpeedValue(eSpeed.Move).m_acc : acc;
            dec = (dec < 0) ? GetSpeedValue(eSpeed.Move).m_dec : dec;
            p_sInfo = base.StartMoveV(vStart, posAt, vChange, posTo, acc, dec);
            if (p_sInfo != "OK") return p_sInfo;
            AddSpeed(vStart * p_pulsepUnit);
            m_qProtocol.Enqueue(new Protocol(m_aCmd[eCmd.Move], (int)posTo));
            p_eState = eState.Move; 
            Thread.Sleep(10);
            return "OK";
        }
        #endregion

        #region Shift
        public override string StartShift(double dfPos, string sSpeed = null)
        {
            double fPos = p_posCommand + dfPos;
            return StartMove(fPos, sSpeed);
        }

        public override string StartShift(double dfPos, double v, double acc = -1, double dec = -1)
        {
            double fPos = p_posCommand + dfPos;
            return StartMove(fPos, v, acc, dec);
        }
        #endregion

        #region ServoOn & Reset
        public override void ServoOn(bool bOn)
        {
            if (EQ.p_bSimulate) return;
            m_qProtocol.Enqueue(new Protocol(m_aCmd[bOn ? eCmd.ServoOn : eCmd.ServoOff]));
        }

        public override string ResetAlarm()
        {
            p_sensorAlarm = false;
            p_sensorEmergency = false; 
            if (p_eState == eState.Error) p_eState = eState.Init; 
            return "OK";
        }
        #endregion

        #region Home
        public enum eHomeDir
        {
            Positive,
            Negative
        }
        eHomeDir m_eHomeDir = eHomeDir.Negative;

        public override string StartHome()
        {
            if (IsInterlock()) return p_id + m_sCheckInterlock;
            if (m_bAbsoluteEncoder)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            p_sInfo = base.StartHome();
            if (p_sInfo != "OK") return p_sInfo;
            m_qProtocol.Enqueue(new Protocol(m_aCmd[eCmd.Event]));
            m_qProtocol.Enqueue(new Protocol(m_aCmd[eCmd.ML], m_nML));
            m_qProtocol.Enqueue(new Protocol(m_aCmd[eCmd.HomeDir], (int)m_eHomeDir));
            m_qProtocol.Enqueue(new Protocol(m_aCmd[eCmd.Home]));
            return "OK";
        }

        int _progressHome = 0;
        public int p_progressHome
        {
            get { return _progressHome; }
            set
            {
                if (_progressHome == value) return;
                _progressHome = value;
                OnPropertyChanged();
            }
        }

        int m_nML = 2000; 
        void RunTreeSettingHome(Tree tree)
        {
            m_nML = tree.Set(m_nML, m_nML, "ML", "ML"); 
            m_eHomeDir = (eHomeDir)tree.Set(m_eHomeDir, m_eHomeDir, "Dir", "Search Home Direction");
        }
        #endregion

        #region Trigger
        public override void RunTrigger(bool bOn, Trigger trigger = null)
        {
        }
        #endregion

        #region UI Binding
        public override UserControl p_ui
        {
            get
            {
                XenaxAxis_UI ui = new XenaxAxis_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        #region Interlock
        public class CSensor
        {
            public bool m_bHome = false;
            public bool m_bPlus = false;
            public bool m_bMinus = false;
            public string m_strAxisName = "";
            public CSensor(string strAxisName)
            {
                m_strAxisName = strAxisName;
            }
        }

        public List<CSensor> m_aSensors = new List<CSensor>();

        string m_sCheckInterlock = "";
        bool IsInterlock()
        {
            m_sCheckInterlock = CheckInterlock();
            return (m_sCheckInterlock != "OK");
        }

        string CheckInterlock()
        {
            for (int i = 0; i < m_aDIO_I.Count; i++)
            {
                if (m_bDIO_I[i])
                {
                    if (!m_aDIO_I[i].p_bIn)
                    {
                        string[] id = m_aDIO_I[i].m_id.Split('.');
                        return " : " + id[1] + " Interlock Error";
                    }
                }
            }
            if (p_vaccumDIO_I != null)
            {
                if (p_vaccumDIO_I.p_bIn)
                {
                    string[] id = p_vaccumDIO_I.m_id.Split('.');
                    return " : " + id[1] + "Interlock Error";
                }
            }
            return "OK";
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_threadRun;
        void InitThread()
        {
            if (m_bThread) return;
            m_bThread = true;
            m_threadRun = new Thread(new ThreadStart(RunThread));
            m_threadRun.Start();
        }

        void RunThread()
        {
            Thread.Sleep(2000);
            while (m_bThread)
            {
                Thread.Sleep(20);
                //if (m_qProtocol.Count == 0) AddGetPos();
                //else 
                if (m_protocolSend == null && m_qProtocol.Count != 0)
                {
                    m_protocolSend = m_qProtocol.Dequeue();
                    if (m_comm != null) m_comm.Send(m_protocolSend.m_sSend);
                }
                switch (p_eState)
                {
                    case eState.Home:
                        if (EQ.IsStop())
                        {
                            StopAxis();
                            ServoOn(false);
                            p_eState = eState.Init;
                        }
                        break;
                    case eState.Move:
                        break;
                    case eState.Jog:
                        if (EQ.IsStop()) StopAxis();
                        break;
                    default: break;
                }
            }
        }
        #endregion

        #region Tree
        public override void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeSpeed(m_treeRoot.GetTree("Speed"), m_sUnit);
            RunTreePos(m_treeRoot.GetTree("Position"), m_sUnit);
            m_trigger.RunTree(m_treeRoot.GetTree("Trigger", false), m_sUnit);

            bool bIOVisible = m_aDIO_I.Count > 0;
            RunTreeIOLock(m_treeRoot.GetTree("I/O Lock", true, bIOVisible), m_sUnit);
        }

        public override void RunTreeSetting(Tree.eMode mode)
        {
            m_treeRootSetting.p_eMode = mode;
            RunTreeSettingComm(m_treeRootSetting.GetTree("Communication"));
            RunTreeSettingProperty(m_treeRootSetting.GetTree("Property"));
            RunTreeSettingHome(m_treeRootSetting.GetTree("Home"));
        }
        #endregion

        public XenaxListAxis m_listAxis;
        public void Init(XenaxListAxis listAxis, string id, Log log)
        {
            m_listAxis = listAxis;
            p_eComm = eComm.RS232;
            InitCmd();
            InitEvent(); 
            InitBase(id, log);
            InitThread();
        }

        public void ThreadStop()
        {
            ServoOn(false);
            if (m_bThread)
            {
                m_bThread = false;
                m_threadRun.Join();
            }
        }
    }
}
