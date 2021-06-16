using Root_Pine2.Engineer;
using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace Root_Pine2.Module
{
    public class Pine2 : ModuleBase
    {
        #region ToolBox
        DIO_IO m_dioStart;
        DIO_IO m_dioStop;
        DIO_IO m_dioReset;
        DIO_IO m_dioHome;
        DIO_IO m_dioPickerSet;
        DIO_I m_diEmergency;
        DIO_I m_diDoorOpen; 
        DIO_I m_diCDA;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetDIO(ref m_dioStart, this, "Start");
            m_toolBox.GetDIO(ref m_dioStop, this, "Stop");
            m_toolBox.GetDIO(ref m_dioReset, this, "Reset");
            m_toolBox.GetDIO(ref m_dioHome, this, "Home");
            m_toolBox.GetDIO(ref m_dioPickerSet, this, "PickerSet");
            m_toolBox.GetDIO(ref m_diEmergency, this, "Emergency");
            m_toolBox.GetDIO(ref m_diDoorOpen, this, "Door Open");
            m_toolBox.GetDIO(ref m_diCDA, this, "CDA");
            m_lamp.GetTools(m_toolBox, this);
            m_buzzer.GetTools(m_toolBox, this);
            m_display.GetTools(m_toolBox, this, bInit); 
            if (bInit)
            {
                InitALID();
                EQ.m_EQ.OnChanged += M_EQ_OnChanged;
            }
        }

        private void M_EQ_OnChanged(_EQ.eEQ eEQ, dynamic value)
        {
            m_buzzer.OnEQChanged(eEQ, value); 
        }
        #endregion

        #region DIO
        EQ.eState m_eEQState = EQ.eState.Idle; 
        void RunThreadDIO(bool bBlink)
        {
            if (m_eEQState != EQ.p_eState)
            {
                m_eEQState = EQ.p_eState;
                m_dioStart.Write(false);
                m_dioStop.Write(false);
                m_dioReset.Write(false);
                m_dioHome.Write(false); 
            }
            switch (EQ.p_eState)
            {
                case EQ.eState.Init:
                    m_dioHome.Write(bBlink);
                    if (m_dioHome.p_bIn) EQ.p_eState = EQ.eState.Home; 
                    break;
                case EQ.eState.Ready:
                    m_dioStart.Write(bBlink);
                    if (m_dioStart.p_bIn) EQ.p_eState = EQ.eState.Run;
                    m_dioReset.Write(bBlink);
                    if (m_dioReset.p_bIn) m_handler.Reset(); 
                    break;
                case EQ.eState.Run:
                    m_dioStop.Write(bBlink);
                    if (m_dioStop.p_bIn) EQ.p_eState = EQ.eState.Ready; 
                    break;
                case EQ.eState.Error:
                    m_dioReset.Write(bBlink);
                    if (m_dioReset.p_bIn) m_handler.Reset();
                    break;
            }
        }
        #endregion

        #region PickerSet 
        bool _diPickerSet = false; 
        public bool p_diPickerSet
        {
            get { return _diPickerSet; }
            set
            {
                if (_diPickerSet == value) return;
                _diPickerSet = value;
                OnPropertyChanged(); 
            }
        }

        void RunThreadPickerSet(bool bBlink)
        {
            m_dioPickerSet.Write(bBlink && EQ.p_bPickerSet);
            if (m_dioPickerSet.p_bIn) p_diPickerSet = true; 
        }

        public string WaitPickerSet(ref double sec)
        {
            p_diPickerSet = false; 
            StopWatch sw = new StopWatch(); 
            while (p_diPickerSet == false)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop"; 
            }
            sec = sw.ElapsedMilliseconds / 1000.0;
            return "OK"; 
        }
        #endregion

        #region Emergency
        bool _bEmergency = false; 
        public bool p_bEmergency
        {
            get { return _bEmergency; }
            set
            {
                if (_bEmergency == value) return;
                _bEmergency = value;
                if (value)
                {
                    EQ.p_bStop = true;
                    EQ.p_eState = EQ.eState.Error;
                    ((Pine2_Engineer)m_engineer).m_ajin.m_listAxis.RunEmergency();
                }
                OnPropertyChanged(); 
            }
        }

        bool _bCDA = false; 
        public bool p_bCDA
        {
            get { return _bCDA; }
            set
            {
                if (_bCDA == value) return;
                _bCDA = value; 
                if (value)
                {
                    EQ.p_eState = EQ.eState.Error;
                    EQ.p_bStop = true; 
                }
                OnPropertyChanged();
            }
        }

        void RunThreadEMG()
        {
            p_bEmergency = m_diEmergency.p_bIn;
            EQ.p_bDoorOpen = m_diDoorOpen.p_bIn;
            p_bCDA = m_diCDA.p_bIn;
        }
        #endregion

        #region GAF
        //public ALID m_alidAirEmergency;
        void InitALID()
        {
            //m_alidAirEmergency = m_gaf.GetALID(this, "Air Emergency", "Air Emergency");
        }
        #endregion

        #region Lamp
        public enum eLamp
        {
            Red,
            Yellow,
            Green
        }
        class Lamp
        {
            DIO_Os m_doLamp; 
            public void GetTools(ToolBox toolBox, ModuleBase module)
            {
                toolBox.GetDIO(ref m_doLamp, module, "Lamp", Enum.GetNames(typeof(eLamp)), false);
            }

            public void RunLamp(bool bBlink)
            {
                switch (EQ.p_eState)
                {
                    case EQ.eState.Init:
                        m_doLamp.AllOff();
                        m_doLamp.Write(eLamp.Yellow, bBlink);
                        break;
                    case EQ.eState.Home:
                        m_doLamp.Write(eLamp.Yellow, true);
                        m_doLamp.Write(eLamp.Green, true);
                        m_doLamp.Write(eLamp.Red, true);
                        break;
                    case EQ.eState.Ready: m_doLamp.Write(eLamp.Yellow); break;
                    case EQ.eState.Run: m_doLamp.Write(eLamp.Green); break;
                    case EQ.eState.Error: m_doLamp.Write(eLamp.Red); break;
                }
            }
        }
        Lamp m_lamp = new Lamp();
        #endregion

        #region Buzzer
        public enum eBuzzer
        {
            Error,
            Warning,
            Finish,
            Home,
        }
        public class Buzzer
        {
            DIO_Os m_doBuzzer;
            public void GetTools(ToolBox toolBox, ModuleBase module)
            {
                toolBox.GetDIO(ref m_doBuzzer, module, "Buzzer", Enum.GetNames(typeof(eBuzzer)), false);
            }

            StopWatch m_swBuzzer = new StopWatch();
            bool m_bBuzzerOn = false; 
            public void RunBuzzer(eBuzzer eBuzzer)
            {
                m_doBuzzer.Write(eBuzzer);
                m_swBuzzer.Start();
                m_bBuzzerOn = true; 
            }

            public void RunBuzzerOff()
            {
                if (m_bBuzzerOn == false) return; 
                m_doBuzzer.AllOff();
                m_bBuzzerOn = false;
            }

            int m_secBuzzerOff = 10;
            public void CheckBuzzerOff()
            {
                if (m_swBuzzer.ElapsedMilliseconds < (1000 * m_secBuzzerOff)) return;
                m_doBuzzer.AllOff();
                m_bBuzzerOn = false;
            }

            public void OnEQChanged(_EQ.eEQ eEQ, dynamic value)
            {
                switch (eEQ)
                {
                    case _EQ.eEQ.State:
                        switch ((EQ.eState)value)
                        {
                            case EQ.eState.Error: RunBuzzer(eBuzzer.Error); break;
                            case EQ.eState.Home: RunBuzzer(eBuzzer.Home); break;
                            case EQ.eState.Ready: RunBuzzerOff(); break;
                        }
                        break;
                }
            }

            public void RunTree(Tree tree)
            {
                m_secBuzzerOff = tree.Set(m_secBuzzerOff, m_secBuzzerOff, "Buzzer Off", "Buzzer Off Delay (sec)");
            }
        }
        public Buzzer m_buzzer = new Buzzer(); 

        public void RunBuzzerOff()
        {
            m_buzzer.RunBuzzerOff(); 
        }
        #endregion

        #region LED Display
        public class Display
        {
            Modbus[] m_modbus = new Modbus[2] { null, null };
            public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
            {
                toolBox.GetComm(ref m_modbus[0], module, "Display0");
                toolBox.GetComm(ref m_modbus[1], module, "Display1");
                if (bInit)
                {
                    m_modbus[0].Connect();
                    m_modbus[1].Connect();
                }
            }

            public string Write(int nComm, int nUnit, string sMsg)
            {
                Data data = new Data(nComm, nUnit, sMsg);
                m_qSend.Enqueue(data); 
                return "OK";
            }

            #region Data
            class Data
            {
                public int m_nComm = 0; 
                public int m_nUnit = 1;
                public List<int> m_aSend = new List<int>(); 
                public Data(int nCumm, int nUnit, string sMsg)
                {
                    m_nComm = nCumm; 
                    m_nUnit = nUnit;
                    while (sMsg.Length < 4) sMsg += " ";
                    byte[] aMsg = Encoding.ASCII.GetBytes(sMsg);
                    for (int n = 0; n < 4; n++) aMsg[n] = GetCode(aMsg[n]); 
                    m_aSend.Add(256 * aMsg[0] + aMsg[1]); 
                    m_aSend.Add(256 * aMsg[2] + aMsg[3]);
                }

                byte GetCode(byte ch)
                {
                    if ((ch >= '0') && (ch <= '9')) return (byte)(ch - '0');
                    if ((ch >= 'A') && (ch <= 'Z')) return (byte)(ch - 'A' + 10);
                    return 0x3f;
                }
            }
            Queue<Data> m_qSend = new Queue<Data>(); 

            string Send(Data data)
            {
                m_modbus[data.m_nComm].Connect(); 
                return m_modbus[data.m_nComm].WriteHoldingRegister((byte)data.m_nUnit, 1, data.m_aSend); 
            }
            #endregion

            #region Timer
            public DispatcherTimer m_timer = new DispatcherTimer();
            void InitTimer()
            {
                m_timer.Interval = TimeSpan.FromSeconds(0.1);
                m_timer.Tick += M_timer_Tick; 
                m_timer.Start();
            }

            private void M_timer_Tick(object sender, EventArgs e)
            {
                if (m_qSend.Count == 0) return;
                Send(m_qSend.Dequeue()); 
            }
            #endregion

            public Display()
            {
                InitTimer(); 
            }
        }
        public Display m_display = new Display(); 
        #endregion

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

        double _widthStrip = 95;
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

        int _lStack = 50; 
        public int p_lStack
        {
            get { return _lStack; }
            set
            {
                _lStack = value;
                OnPropertyChanged(); 
            }
        }

        int _lStackPaper = 100;
        public int p_lStackPaper
        {
            get { return _lStackPaper; }
            set
            {
                _lStackPaper = value;
                OnPropertyChanged();
            }
        }

        bool _b3D = true; 
        public bool p_b3D
        {
            get { return _b3D; }
            set
            {
                if (_b3D == value) return;
                _b3D = value;
                OnPropertyChanged(); 
            }
        }

        string _sRecipe = "";
        public string p_sRecipe
        {
            get { return _sRecipe; }
            set
            {
                if (_sRecipe == value) return;
                _sRecipe = value;
                m_handler.p_sRecipe = value; 
            }
        }
        #endregion

        #region Thread DIO
        bool m_bRunDIO = false;
        Thread m_threadDIO;
        void InitThread()
        {
            m_threadDIO = new Thread(new ThreadStart(RunThreadDIO));
            m_threadDIO.Start();
        }

        void RunThreadDIO()
        {
            m_bRunDIO = true;
            Thread.Sleep(2000);
            StopWatch sw = new StopWatch();
            bool bBlink = false; 
            while (m_bRunDIO)
            {
                Thread.Sleep(10);
                if (sw.ElapsedMilliseconds > 500)
                {
                    sw.Start();
                    bBlink = !bBlink; 
                }
                RunThreadEMG(); 
                RunThreadDIO(bBlink); 
                RunThreadPickerSet(bBlink); 
                m_lamp.RunLamp(bBlink);
                m_buzzer.CheckBuzzerOff(); 
            }
        }

        void ThreadDIOStop()
        {
            if (m_bRunDIO == false) return;
            m_bRunDIO = false;
            m_threadDIO.Join(); 
        }
        #endregion

        #region StateHome
        public override string StateHome()
        {
            return "OK";
        }
        #endregion

        #region Reset
        public override void Reset()
        {
            m_buzzer.RunBuzzerOff(); 
            base.Reset();
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_buzzer.RunTree(tree.GetTree("Buzzer")); 
            p_eMode = (eRunMode)tree.Set(p_eMode, p_eMode, "Mode", "RunMode");
            p_widthStrip = tree.Set(p_widthStrip, p_widthStrip, "Width", "Strip Width (mm)");
            p_lStack = tree.Set(p_lStack, p_lStack, "Stack Count", "Strip Max Stack Count");
            p_lStackPaper = tree.Set(p_lStackPaper, p_lStackPaper, "Paper Count", "Paper Max Stack Count");
            p_sRecipe = tree.Set(p_sRecipe, p_sRecipe, "Recipe", "Recipe"); 
        }
        #endregion

        Pine2_Handler m_handler; 
        public Pine2(string id, IEngineer engineer)
        {
            p_id = id;
            m_handler = (Pine2_Handler)engineer.ClassHandler(); 
            InitBase(id, engineer);

            InitThread();
        }

        public override void ThreadStop()
        {
            m_display.m_timer.Stop(); 
            ThreadDIOStop(); 
            base.ThreadStop();
        }
    }
}
