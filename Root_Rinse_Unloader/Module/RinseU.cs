using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Root_Rinse_Unloader.Module
{
    public class RinseU : ModuleBase
    {
        #region eRunMode
        public enum eRunMode
        {
            Magazine,
            Stack
        }
        string[] m_asRunMode = Enum.GetNames(typeof(eRunMode)); 

        eRunMode _eMode = eRunMode.Stack;
        public eRunMode p_eMode
        {
            get { return _eMode; }
            set
            {
                if (_eMode == value) return;
                _eMode = value;
                OnPropertyChanged();
            }
        }

        double _widthStrip = 77;
        public double p_widthStrip
        {
            get { return _widthStrip; }
            set
            {
                if (_widthStrip == value) return;
                _widthStrip = value;
                OnPropertyChanged();
            }
        }

        int _iMagazin = 0;
        public int p_iMagazine
        {
            get { return _iMagazin; }
            set
            {
                if (_iMagazin == value) return;
                _iMagazin = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Loader EQ State
        EQ.eState _eStateLoader = EQ.eState.Init;
        public EQ.eState p_eStateLoader 
        {
            get { return _eStateLoader; }
            set
            {
                if (_eStateLoader == value) return;
                _eStateLoader = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region ToolBox
        TCPIPServer m_tcpip; 
        public override void GetTools(bool bInit)
        {
            GetToolsDIO();
            p_sInfo = m_toolBox.Get(ref m_tcpip, this, "TCPIP");
            if (bInit) 
            {
                EQ.m_EQ.OnChanged += M_EQ_OnChanged; 
                m_tcpip.EventReciveData += M_tcpip_EventReciveData;
            }
        }

        private void M_EQ_OnChanged(_EQ.eEQ eEQ, dynamic value)
        {
            if (eEQ == _EQ.eEQ.State)
            {
                AddProtocol(p_id, eCmd.EQUeState, value);
                switch ((EQ.eState)value)
                {
                    case EQ.eState.Error: RunBuzzer(eBuzzer.Error); break;
                    case EQ.eState.Home: RunBuzzer(eBuzzer.Home); break;
                    case EQ.eState.Ready: RunBuzzerOff(); break;
                }
            }
        }
        #endregion

        #region DIO
        public enum eLamp
        {
            Red,
            Yellow,
            Green
        }
        string[] m_asLamp = Enum.GetNames(typeof(eLamp));
        public enum eBuzzer
        {
            Error,
            Warning,
            Finish,
            Home,
        }
        string[] m_asBuzzer = Enum.GetNames(typeof(eBuzzer));

        DIO_I m_diEMG;
        DIO_I m_diAir;
        DIO_I m_diDoorLock;
        DIO_I m_diBuzzerOff;
        DIO_Os m_doLamp;
        DIO_Os m_doBuzzer;
        void GetToolsDIO()
        {
            p_sInfo = m_toolBox.Get(ref m_diEMG, this, "Emergency");
            p_sInfo = m_toolBox.Get(ref m_diAir, this, "Air Pressure");
            p_sInfo = m_toolBox.Get(ref m_diDoorLock, this, "Door Lock");
            p_sInfo = m_toolBox.Get(ref m_diBuzzerOff, this, "Buzzer Off");
            p_sInfo = m_toolBox.Get(ref m_doLamp, this, "Lamp", m_asLamp);
            p_sInfo = m_toolBox.Get(ref m_doBuzzer, this, "Buzzer", m_asBuzzer);
        }

        bool _bEMG = false;
        public bool p_bEMG
        {
            get { return _bEMG; }
            set
            {
                if (_bEMG == value) return;
                _bEMG = value;
                OnPropertyChanged();
                if (value)
                {
                    EQ.p_bStop = true;
                    EQ.p_eState = EQ.eState.Error;
                }
            }
        }

        bool _bAir = false;
        public bool p_bAir
        {
            get { return _bAir; }
            set
            {
                if (_bAir == value) return;
                _bAir = value;
                OnPropertyChanged();
                if (value)
                {
                    EQ.p_bStop = true;
                    EQ.p_eState = EQ.eState.Error;
                }
            }
        }

        bool _bDoorLock = false;
        public bool p_bDoorLock
        {
            get { return _bDoorLock; }
            set
            {
                if (_bDoorLock == value) return;
                _bDoorLock = value;
                OnPropertyChanged();
                EQ.p_bDoorOpen = value;
            }
        }

        bool _bBuzzerOff = false;
        public bool p_bBuzzerOff
        {
            get { return _bBuzzerOff; }
            set
            {
                if (_bBuzzerOff == value) return;
                _bBuzzerOff = value;
                OnPropertyChanged();
                if (value) RunBuzzerOff();
            }
        }

        public void RunBuzzer(eBuzzer eBuzzer)
        {
            m_doBuzzer.Write(eBuzzer);
        }

        public void RunBuzzerOff()
        {
            m_doBuzzer.AllOff();
        }

        public bool m_bBlink = false;
        StopWatch m_swBlick = new StopWatch();
        void RunThreadDIO()
        {
            p_bEMG = m_diEMG.p_bIn;
            p_bAir = m_diAir.p_bIn;
            p_bDoorLock = m_diDoorLock.p_bIn;
            p_bBuzzerOff = m_diBuzzerOff.p_bIn;
            if (m_swBlick.ElapsedMilliseconds < 500) return;
            m_swBlick.Start();
            m_bBlink = !m_bBlink;
            switch (EQ.p_eState)
            {
                case EQ.eState.Ready: m_doLamp.Write(eLamp.Green); break;
                case EQ.eState.Run: m_doLamp.Write(eLamp.Yellow); break;
                case EQ.eState.Error: m_doLamp.Write(eLamp.Red); break;
                default: m_doLamp.AllOff(); break;
            }
        }
        #endregion

        #region Protocol
        public enum eCmd
        {
            Unknown,
            SetMode,
            SetWidth,
            EQLeState,
            EQUeState,
        }
        public string[] m_asCmd = Enum.GetNames(typeof(eCmd)); 

        public class Protocol
        {
            public string p_sCmd
            {
                get { return m_id + "," + m_eCmd.ToString() + "," + m_value.ToString(); }
            }

            public string m_id; 
            public eCmd m_eCmd;
            public dynamic m_value; 
            public Protocol(string id, eCmd eCmd, dynamic value)
            {
                m_id = id;
                m_eCmd = eCmd;
                m_value = value; 
            }
        }
        #endregion

        #region Thread Send
        Protocol m_protocolSend = null;
        Queue<Protocol> m_qProtocolSend = new Queue<Protocol>();
        Queue<Protocol> m_qProtocolReply = new Queue<Protocol>();
        bool m_bRunSend = false;
        Thread m_threadSend;
        void InitThread()
        {
            m_threadSend = new Thread(new ThreadStart(RunThreadSend));
            m_threadSend.Start();
        }

        void RunThreadSend()
        {
            m_bRunSend = true;
            Thread.Sleep(5000);
            while (m_bRunSend)
            {
                Thread.Sleep(10);
                RunThreadDIO();
                if (m_qProtocolReply.Count > 0)
                {
                    Protocol protocol = m_qProtocolReply.Dequeue();
                    m_tcpip.Send(protocol.p_sCmd);
                    Thread.Sleep(10); 
                }
                else if ((m_qProtocolSend.Count > 0) && (m_protocolSend == null))
                {
                    m_protocolSend = m_qProtocolSend.Dequeue();
                    m_tcpip.Send(m_protocolSend.p_sCmd);
                    Thread.Sleep(10);
                }
            }
        }

        public Protocol AddProtocol(string id, eCmd eCmd, dynamic value)
        {
            Protocol protocol = new Protocol(id, eCmd, value);
            if (id == p_id) m_qProtocolSend.Enqueue(protocol);
            else m_qProtocolReply.Enqueue(protocol);
            return protocol;
        }
        #endregion

        #region TCPIP
        private void M_tcpip_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            try
            {
                string sRead = Encoding.Default.GetString(aBuf, 0, nSize);
                string[] asRead = sRead.Split(',');
                if (asRead.Length < 3)
                {
                    p_sInfo = "Invalid Protocol";
                    return;
                }
                eCmd eCmd = GetCmd(asRead[1]);
                if (asRead[0] == p_id) m_protocolSend = null;
                else
                {
                    switch (eCmd)
                    {
                        case eCmd.SetMode:
                            SetMode(asRead[2]);
                            AddProtocol(asRead[0], eCmd, asRead[2]);
                            RunTree(Tree.eMode.Init); 
                            break;
                        case eCmd.SetWidth:
                            p_widthStrip = Convert.ToDouble(asRead[2]);
                            AddProtocol(asRead[0], eCmd, asRead[2]);
                            RunTree(Tree.eMode.Init);
                            break;
                        case eCmd.EQLeState:
                            p_eStateLoader = GetEQeState(asRead[2]);
                            //switch (GetEQeState(asRead[2]))
                            switch(p_eStateLoader)
                            {
                                case EQ.eState.Home: 
                                    if (EQ.p_eState != EQ.eState.Run) EQ.p_eState = EQ.eState.Home; //forget
                                    break;
                                case EQ.eState.Run: 
                                    EQ.p_eState = EQ.eState.Run; 
                                    break;
                            }
                            AddProtocol(asRead[0], eCmd, asRead[2]);
                            break;
                    }
                }
            }
            catch (Exception e) { p_sInfo = "EventRecieveData Exception : " + e.Message; }
        }

        eCmd GetCmd(string sCmd)
        {
            for (int n = 0; n < m_asCmd.Length; n++)
            {
                if (sCmd == m_asCmd[n]) return (eCmd)n;
            }
            return eCmd.Unknown; 
        }

        void SetMode(string sMode)
        {
            for (int n = 0; n < m_asRunMode.Length; n++)
            {
                if (sMode == m_asRunMode[n]) p_eMode = (eRunMode)n;
            }
        }

        EQ.eState GetEQeState(string sState)
        {
            string[] asState = Enum.GetNames(typeof(EQ.eState)); 
            for(int n = 0; n < asState.Length; n++)
            {
                if (asState[n] == sState) return (EQ.eState)n;
            }
            return EQ.eState.Null; 
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            p_eMode = (eRunMode)tree.Set(p_eMode, p_eMode, "Mode", "RunMode", true, true);
            p_widthStrip = tree.Set(p_widthStrip, p_widthStrip, "Width", "Strip Width (mm)", true, true);
        }
        #endregion

        public RinseU(string id, IEngineer engineer)
        {
            p_id = id;
            InitBase(id, engineer);

            InitThread(); 
        }

        public override void ThreadStop()
        {
            if (m_bRunSend)
            {
                m_bRunSend = false;
                m_threadSend.Join(); 
            }
            base.ThreadStop();
        }

    }
}
