using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Threading;

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

        double _fRotateSpeed = 1;
        public double p_fRotateSpeed
        {
            get { return _fRotateSpeed; }
            set
            {
                if (_fRotateSpeed == value) return;
                _fRotateSpeed = value;
                OnPropertyChanged();
            }
        }

        Storage.eMagazine _eMagazine = Storage.eMagazine.Magazine1;
        public Storage.eMagazine p_eMagazine
        {
            get { return _eMagazine; }
            set
            {
                if (_eMagazine == value) return;
                _eMagazine = value;
                OnPropertyChanged();
            }
        }

        int _iMagazine = 0;
        public int p_iMagazine
        {
            get { return _iMagazine; }
            set
            {
                if (_iMagazine == value) return;
                _iMagazine = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Strips
        public class Strips : NotifyProperty
        {
            string _sSend = "....";
            public string p_sSend
            {
                get { return _sSend; }
                set
                {
                    _sSend = value;
                    OnPropertyChanged();
                }
            }

            string _sReceive = "....";
            public string p_sReceive
            {
                get { return _sReceive; }
                set
                {
                    _sReceive = value;
                    OnPropertyChanged();
                    if (p_sSend != p_sReceive) p_sError = "Error";
                }
            }

            string _sError = "";
            public string p_sError
            {
                get { return _sError; }
                set
                {
                    _sError = value;
                    OnPropertyChanged();
                }
            }

            public Strips(string sSend)
            {
                p_sSend = sSend;
            }
        }

        Queue<Strips> m_qSend = new Queue<Strips>();
        public void AddStripSend(string sStrip)
        {
            Strips strips = new Strips(sStrip);
            m_qSend.Enqueue(strips);
        }

        Queue<string> m_qReceive = new Queue<string>();
        public void AddStripReceive(string sStrip)
        {
            m_qReceive.Enqueue(sStrip);
            AddProtocol(p_id, eCmd.StripReceive, sStrip);
        }

        public void ClearStripResult()
        {
            p_aSend.Clear();
            p_aReceive.Clear();
            AddProtocol(p_id, eCmd.ResultClear, 0);
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        public ObservableCollection<Strips> p_aSend = new ObservableCollection<Strips>();
        public ObservableCollection<Strips> p_aReceive = new ObservableCollection<Strips>();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (m_qSend.Count > 0) p_aSend.Add(m_qSend.Dequeue());
            if (m_qReceive.Count > 0)
            {
                string sStrip = m_qReceive.Dequeue(); 
                if (p_aSend.Count > 0)
                {
                    Strips strip = p_aSend[0];
                    p_aSend.RemoveAt(0);
                    strip.p_sReceive = sStrip;
                    p_aReceive.Add(strip);
                }
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

        #region GAF
        ALID m_alidAirEmergency;
        void InitALID()
        {
            m_alidAirEmergency = m_gaf.GetALID(this, "Air Emergency", "Air Emergency");
        }
        #endregion

        #region ToolBox
        public TCPIPServer m_tcpip; 
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
                    m_alidAirEmergency.p_bSet = true;
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
                    m_alidAirEmergency.p_bSet = true;
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
            if (m_bBlink == false) m_doLamp.AllOff();
            else
            {
                switch (EQ.p_eState)
                {
                    case EQ.eState.Ready: m_doLamp.Write(eLamp.Yellow); break;
                    case EQ.eState.Run: m_doLamp.Write(eLamp.Green); break;
                    case EQ.eState.Error: m_doLamp.Write(eLamp.Red); break;
                    default: m_doLamp.AllOff(); break;
                }
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
            PickerSet,
            StripSend,
            StripReceive,
            ResultClear,
            SetRotateSpeed,
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
                            AddProtocol(asRead[0], eCmd, asRead[2]);
                            SetMode(asRead[2]);
                            RunTree(Tree.eMode.Init); 
                            break;
                        case eCmd.SetWidth:
                            AddProtocol(asRead[0], eCmd, asRead[2]);
                            p_widthStrip = Convert.ToDouble(asRead[2]);
                            RunTree(Tree.eMode.Init);
                            break;
                        case eCmd.EQLeState:
                            AddProtocol(asRead[0], eCmd, asRead[2]);
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
                            break;
                        case eCmd.PickerSet:
                            AddProtocol(asRead[0], eCmd, asRead[2]);
                            EQ.p_bPickerSet = Convert.ToBoolean(asRead[2]);
                            break;
                        case eCmd.StripSend:
                            AddProtocol(asRead[0], eCmd, asRead[2]);
                            AddStripSend(asRead[2]); 
                            break;
                        case eCmd.SetRotateSpeed:
                            AddProtocol(asRead[0], eCmd, asRead[2]);
                            p_fRotateSpeed = Convert.ToDouble(asRead[2]);
                            RunTree(Tree.eMode.Init);
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
            InitTimer(); 
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
