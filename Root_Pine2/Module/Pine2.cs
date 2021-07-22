using QRCoder;
using Root_Pine2.Engineer;
using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.Printer;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.IO;
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
        DIO_O m_doFFU_Handler;
        DIO_O m_doFFU_Vision;
        DIO_O m_doIonizer; 
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetDIO(ref m_dioStart, this, "Start", false);
            m_toolBox.GetDIO(ref m_dioStop, this, "Stop", false);
            m_toolBox.GetDIO(ref m_dioReset, this, "Reset", false);
            m_toolBox.GetDIO(ref m_dioHome, this, "Home", false);
            m_toolBox.GetDIO(ref m_dioPickerSet, this, "PickerSet", false);
            m_toolBox.GetDIO(ref m_diEmergency, this, "Emergency");
            m_toolBox.GetDIO(ref m_diDoorOpen, this, "Door Open");
            m_toolBox.GetDIO(ref m_diCDA, this, "CDA");
            m_toolBox.GetDIO(ref m_doFFU_Handler, this, "FFU Handler");
            m_toolBox.GetDIO(ref m_doFFU_Vision, this, "FFU Vision");
            m_toolBox.GetDIO(ref m_doIonizer, this, "Ionizer");
            m_toolBox.Get(ref m_printer.m_srp350, this, "Printer"); 
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
                    m_dioHome.Write(bBlink);
                    if (m_dioHome.p_bIn) EQ.p_eState = EQ.eState.Home;
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
                    m_dioHome.Write(bBlink);
                    if (m_dioHome.p_bIn) EQ.p_eState = EQ.eState.Home;
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

        public string m_sFilePickerSet = "c:\\Recipe\\PickerSet.RunPine2";
        void RunTreePickerSet(Tree tree)
        {
            m_sFilePickerSet = tree.SetFile(m_sFilePickerSet, m_sFilePickerSet, "RunPine2", "File", "PickerSet ModuleRun File"); 
        }
        #endregion

        #region Emergency
        bool _bEmergency = false; 
        public bool p_bEmergency
        {
            get { return _bEmergency; }
            set
            {
                m_alidEMG.p_bSet = value;
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
                m_alidCDA.p_bSet = value; 
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
            m_alidDoorOpen.p_bSet = m_diDoorOpen.p_bIn;
            p_bCDA = m_diCDA.p_bIn;
        }
        #endregion

        #region GAF
        ALID m_alidEMG;
        ALID m_alidCDA;
        ALID m_alidDoorOpen;
        public ALID m_alidNewLot;
        public ALID m_alidSummary;
        void InitALID()
        {
            m_alidEMG = m_gaf.GetALID(this, "Emergency", "Emergency Button Pressed");
            m_alidCDA = m_gaf.GetALID(this, "CDA", "Check CDA");
            m_alidDoorOpen = m_gaf.GetALID(this, "DoorOpen", "Door Open");
            m_alidDoorOpen.p_bEQError = false; 
            m_alidNewLot = m_gaf.GetALID(this, "NewLot", "New Lot Communication Error");
            m_alidSummary = m_gaf.GetALID(this, "Summary", "Summary Error");
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
                m_doLamp.Write(eLamp.Yellow, bBlink && ((EQ.p_eState == EQ.eState.Ready) || (EQ.p_eState == EQ.eState.Home)));
                m_doLamp.Write(eLamp.Green, bBlink && ((EQ.p_eState == EQ.eState.Run) || (EQ.p_eState == EQ.eState.Home)));
                m_doLamp.Write(eLamp.Red, bBlink && ((EQ.p_eState == EQ.eState.Error) || (EQ.p_eState == EQ.eState.Home)));
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
                if (data == null) return "OK"; 
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
                switch (value)
                {
                    case eRunMode.Magazine: _eMode = value; break;
                    case eRunMode.Stack:
                        if (m_handler.m_magazineEVSet.IsMagazineUp() == false) _eMode = value;
                        break; 
                }
                OnPropertyChanged();
            }
        }

        public double m_widthDefaultStrip = 95; 
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

        public double m_thicknessDefault = 0;
        double _thickness = 0;
        public double p_thickness
        {
            get { return _thickness; }
            set
            {
                if (_thickness == value) return;
                _thickness = value;
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

        bool _bUseKeyence = true;
        public bool p_bUseKeyence
        {
            get { return _bUseKeyence; }
            set
            {
                if (_bUseKeyence == value) return;
                m_log.Info("p_bUseBCD = " + value.ToString());
                _bUseKeyence = value;
                OnPropertyChanged();
            }
        }

        bool _bCheckPaper = true;
        public bool p_bCheckPaper
        {
            get { return _bCheckPaper; }
            set
            {
                if (_bCheckPaper == value) return;
                m_log.Info("p_bCheckPaper = " + value.ToString());
                _bCheckPaper = value;
                OnPropertyChanged();
            }
        }
        
        bool _bUseBlow = true;
        public bool p_bUseBlow
        {
            get { return _bUseBlow; }
            set
            {
                _bUseBlow = value;
                OnPropertyChanged(); 
            }
        }

        bool _bUseIonBlow = true;
        public bool p_bUseIonBlow
        {
            get { return _bUseIonBlow; }
            set
            {
                _bUseIonBlow = value;
                OnPropertyChanged(); 
            }
        }

        bool _bUseAlignBlow = true;
        public bool p_bUseAlignBlow
        {
            get { return _bUseAlignBlow; }
            set
            {
                _bUseAlignBlow = value;
                OnPropertyChanged(); 
            }
        }

        void RunTreeMode(Tree tree)
        {
            p_eMode = (eRunMode)tree.Set(p_eMode, p_eMode, "Mode", "RunMode");
            p_b3D = tree.Set(p_b3D, p_b3D, "3D", "RunMode");
            p_bUseKeyence = tree.Set(p_bUseKeyence, p_bUseKeyence, "Keyence", "RunMode");
            p_bCheckPaper = tree.Set(p_bCheckPaper, p_bCheckPaper, "Check Paper", "RunMode");
            p_bUseBlow = tree.Set(p_bUseBlow, p_bUseBlow, "Blow", "Use Blow");
            p_bUseIonBlow = tree.Set(p_bUseIonBlow, p_bUseIonBlow, "Ion Blow", "Use Blow");
            p_bUseAlignBlow = tree.Set(p_bUseAlignBlow, p_bUseAlignBlow, "Align Blow", "Use Blow");
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
            Thread.Sleep(5000);
            StopWatch sw = new StopWatch();
            bool bBlink = false; 
            while (m_bRunDIO)
            {
                Thread.Sleep(10);
                if (sw.ElapsedMilliseconds > 400)
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
            RunTreeMode(tree.GetTree("Mode"));
            p_widthStrip = tree.GetTree("Strip").Set(p_widthStrip, p_widthStrip, "Width", "Strip Width (mm)");
            p_thickness = tree.GetTree("Strip").Set(p_thickness, p_thickness, "Thickness", "Strip Thickness (um)");
            m_widthDefaultStrip = tree.GetTree("Default Strip").Set(m_widthDefaultStrip, m_widthDefaultStrip, "Width", "Strip Width (mm)");
            m_thicknessDefault = tree.GetTree("Default Strip").Set(m_thicknessDefault, m_thicknessDefault, "Thickness", "Strip Thickness (um)");
            p_lStack = tree.GetTree("Stack").Set(p_lStack, p_lStack, "Stack Count", "Strip Max Stack Count");
            p_lStackPaper = tree.GetTree("Stack").Set(p_lStackPaper, p_lStackPaper, "Paper Count", "Paper Max Stack Count");
            RunTreeVisionOption(tree.GetTree("VisionOption"));
            m_printer.RunTree(tree.GetTree("Printer"));
            RunTreePickerSet(tree.GetTree("PickerSet"));
            if (m_handler.m_aBoats.Count == 0) return; 
            m_handler.m_aBoats[eVision.Top3D]?.RunTreeClean(tree.GetTree("Top3D Clean"));
            m_handler.m_aBoats[eVision.Top2D]?.RunTreeClean(tree.GetTree("Top2D Clean"));
            m_handler.m_aBoats[eVision.Bottom]?.RunTreeClean(tree.GetTree("Bottom Clean"));
        }
        #endregion

        #region Recipe
        const string c_sExt = ".pine2";
        public void RecipeSave()
        {
            if (m_handler.p_sRecipe == "") return; 
            string sPath = EQ.c_sPathRecipe + "\\" + m_handler.p_sRecipe;
            Directory.CreateDirectory(sPath);
            string sFile = sPath + "\\Pine2" + c_sExt;
            m_treeRootSetup.m_job = new Job(sFile, true, m_log);
            RunTree(Tree.eMode.JobSave);
            m_treeRootSetup.m_job.Close();
        }

        public void RecipeOpen(string sRecipe)
        {
            string sPath = EQ.c_sPathRecipe + "\\" + sRecipe;
            Directory.CreateDirectory(sPath);
            string sFile = sPath + "\\Pine2" + c_sExt;
            m_treeRootSetup.m_job = new Job(sFile, false, m_log);
            RunTree(Tree.eMode.JobOpen);
            m_treeRootSetup.m_job.Close();
        }
        #endregion

        #region Lot
        string _sOperator = ""; 
        public string p_sOperator
        {
            get { return _sOperator; }
            set
            {
                _sOperator = value;
                OnPropertyChanged(); 
            }
        }

        Registry m_reg = new Registry("Pine2");
        string _sLotID = "";
        public string p_sLotID
        {
            get { return _sLotID; }
            set
            {
                if (_sLotID == value) return; 
                _sLotID = value;
                OnPropertyChanged();
                m_reg.Write("LotID", value);
            }
        }

        int _iBundle = 0;
        public int p_iBundle
        {
            get { return _iBundle; }
            set
            {
                _iBundle = value;
                OnPropertyChanged();
                m_reg.Write("Bundle", value); 
            }
        }
        #endregion

        #region VisionOption
        public class VisionOption : NotifyProperty
        {
            bool _bLotMix = false;
            public bool p_bLotMix
            {
                get { return _bLotMix; }
                set
                {
                    _bLotMix = value;
                    OnPropertyChanged(); 
                }
            }

            bool _bBarcode = false; 
            public bool p_bBarcode
            {
                get { return _bBarcode; }
                set
                {
                    _bBarcode = value;
                    OnPropertyChanged();
                }
            }

            int _nBarcode = 0;
            public int p_nBarcode
            {
                get { return _nBarcode; }
                set
                {
                    _nBarcode = value;
                    OnPropertyChanged(); 
                }
            }

            int _lBarcode = 0;
            public int p_lBarcode
            {
                get { return _lBarcode; }
                set
                {
                    _lBarcode = value;
                    OnPropertyChanged();
                }
            }

            public void RunTree(Tree tree)
            {
                p_bLotMix = tree.Set(p_bLotMix, p_bLotMix, "LotMix", "Lot Mix Inspect");
                p_bBarcode = tree.Set(p_bBarcode, p_bBarcode, "Barcode", "Barcode Inspect");
                p_nBarcode = tree.Set(p_nBarcode, p_nBarcode, "Barcode Start", "Barcode Start Position (pixel)", p_bBarcode);
                p_lBarcode = tree.Set(p_lBarcode, p_lBarcode, "Barcode Length", "Barcode Length (pixel)", p_bBarcode);
            }
        }
        public Dictionary<eVision, VisionOption> m_aVisionOption = new Dictionary<eVision, VisionOption>(); 
        void InitVisionOption()
        {
            m_aVisionOption.Add(eVision.Top3D, new VisionOption());
            m_aVisionOption.Add(eVision.Top2D, new VisionOption());
            m_aVisionOption.Add(eVision.Bottom, new VisionOption());
        }

        void RunTreeVisionOption(Tree tree)
        {
            m_aVisionOption[eVision.Top3D].RunTree(tree.GetTree("Top3D"));
            m_aVisionOption[eVision.Top2D].RunTree(tree.GetTree("Top2D"));
            m_aVisionOption[eVision.Bottom].RunTree(tree.GetTree("Bottom"));
        }
        #endregion

        #region SRP-350III
        public class Printer
        {
            public SRP350 m_srp350;
            QRCodeGenerator m_qrGenerator = new QRCodeGenerator(); 

            public class Doc
            {
                public string m_sSorter = "";
                public string m_sBundle;
                public InfoStrip m_InfoStrip = null;
                public int m_nStrip = 0;
                public InfoStrip.eResult m_eResult = InfoStrip.eResult.Init; 
                public DateTime m_dtNow = DateTime.Now; 

                public Doc(int iSorter, int iBundle, int nStrip, InfoStrip.eResult eResult)
                {
                    m_eResult = eResult;
                    for (int n = 0; n < 8; n++) m_sSorter += ((n == iSorter) ? n.ToString() : "_");
                    m_sBundle = iBundle.ToString("00");
                    m_nStrip = nStrip; 
                }
            }
            Queue<Doc> m_qDoc = new Queue<Doc>();

            public void AddPrint(int iSorter, int iBundle, int nStrip)
            {
                Doc doc = new Doc(iSorter, iBundle, nStrip, InfoStrip.eResult.Init);
                m_qDoc.Enqueue(doc); 
            }

            public void AddPrint(int iSorter, int iBundle, int nStrip, InfoStrip.eResult eResult)
            {
                if (eResult == InfoStrip.eResult.Init) return; 
                Doc doc = new Doc(iSorter, iBundle, nStrip, eResult);
                m_qDoc.Enqueue(doc);
            }

            public DispatcherTimer m_timer = new DispatcherTimer();
            void InitTimer()
            {
                m_timer.Interval = TimeSpan.FromSeconds(0.1);
                m_timer.Tick += M_timer_Tick;
                m_timer.Start();
            }

            private void M_timer_Tick(object sender, EventArgs e)
            {
                if (m_qDoc.Count == 0) return;
                PrintDoc(m_qDoc.Dequeue()); 
            }

            void PrintDoc(Doc doc)
            {
                string sRecipe = m_handler.p_sRecipe;
                string sLot = m_handler.m_pine2.p_sLotID; 
                string sVS = "_S00_C" + doc.m_sBundle; 
                m_srp350.Start();
                Write("Machine ID : Pine2 #" + m_iMachine.ToString());
                Write("--------------------------------");
                Write("Operator : " + m_handler.m_pine2.p_sOperator);
                Write("Recipe : " + sRecipe);
                Write("Lot ID : " + sLot);
                Write("3D Inspect : " + m_handler.m_pine2.p_b3D.ToString());
                Write("VS File : " + sLot + sVS);
                Write("--------------------------------");
                if (doc.m_eResult != InfoStrip.eResult.Init) Write("Result : " + doc.m_eResult.ToString());
                Write("Bundle : " + doc.m_sBundle);
                Write("Sorter : " + doc.m_sSorter);
                Write("Strip Count : " + doc.m_nStrip.ToString());
                Write("");
                Write(doc.m_dtNow.ToString("yyyy-MM-dd HH:mm:ss"));
                string sRecipeLot = "/" + sRecipe + "_" + sLot; 
                string sQR = "/M" + m_iMachine.ToString() + "V2/" + sRecipe + sRecipeLot + sRecipeLot + sVS; 
                m_srp350.WriteQR(sQR); 
                m_srp350.End(); 
            }

            void Write(string sWrite)
            {
                m_srp350.WriteText(sWrite);
                m_log.Info(sWrite); 
            }

            int m_iMachine = 1;
            int m_szFont = 9;
            public void RunTree(Tree tree)
            {
                m_iMachine = tree.Set(m_iMachine, m_iMachine, "Machine", "Machine ID");
                m_szFont = tree.Set(m_szFont, m_szFont, "Size", "Fonr Size");
            }

            Log m_log; 
            Pine2_Handler m_handler; 
            public Printer(Pine2_Handler handler)
            {
                m_handler = handler;
                m_log = LogView.GetLog("Summary"); 
                InitTimer();
            }
        }
        public Printer m_printer;
        #endregion

        Pine2_Handler m_handler; 
        public Pine2(string id, IEngineer engineer)
        {
            InitVisionOption(); 
            p_id = id;
            m_handler = (Pine2_Handler)engineer.ClassHandler();
            m_printer = new Printer(m_handler);
            p_sLotID = m_reg.Read("LotID", ""); 
            p_iBundle = m_reg.Read("Bundle", 0); 
            InitBase(id, engineer); 

            InitThread();
        }

        public override void ThreadStop()
        {
            m_printer.m_timer.Stop(); 
            m_display.m_timer.Stop(); 
            ThreadDIOStop(); 
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_SendSortInfo(this), false, "Send Sort Info");
            AddModuleRunList(new Run_SortTest(this), false, "Sort Test");
        }

        public class Run_SendSortInfo : ModuleRunBase
        {
            Pine2 m_module;
            public Run_SendSortInfo(Pine2 module)
            {
                m_infoStrip.p_id = "0000";
                m_infoStrip.m_iBundle = 0; 
                m_module = module;
                InitModuleRun(module);
            }

            InfoStrip m_infoStrip = new InfoStrip(0);
            public override ModuleRunBase Clone()
            {
                Run_SendSortInfo run = new Run_SendSortInfo(m_module);
                run.m_infoStrip = m_infoStrip; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_infoStrip.m_eWorks = (eWorks)tree.Set(m_infoStrip.m_eWorks, m_infoStrip.m_eWorks, "eWorks", "Select Boat", bVisible);
                m_infoStrip.p_id = tree.Set(m_infoStrip.p_id, m_infoStrip.p_id, "StripID", "Strip ID", bVisible);
                m_infoStrip.m_iBundle = tree.Set(m_infoStrip.m_iBundle, m_infoStrip.m_iBundle, "Bundle", "Bundle", bVisible);
            }

            public override string Run()
            {
                m_module.m_handler.SendSortInfo(m_infoStrip);
                return "OK";
            }
        }

        public class Run_SortTest : ModuleRunBase
        {
            Pine2 m_module;
            public Run_SortTest(Pine2 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            CPoint m_szMap = new CPoint(2, 3);
            InfoStrip.eResult m_eResultTop = InfoStrip.eResult.GOOD;
            InfoStrip.eResult m_eResultBottom = InfoStrip.eResult.GOOD;
            string m_sMapTop = "111111";
            string m_sMapBottom = "111111";
            InfoStrip m_infoStrip = new InfoStrip(0);
            public override ModuleRunBase Clone()
            {
                Run_SortTest run = new Run_SortTest(m_module);
                run.m_szMap = new CPoint(m_szMap); 
                run.m_eResultTop = m_eResultTop;
                run.m_eResultBottom = m_eResultBottom;
                run.m_sMapTop = m_sMapTop;
                run.m_sMapBottom = m_sMapBottom;
                run.m_infoStrip = m_infoStrip;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_szMap = tree.Set(m_szMap, m_szMap, "MapSize", "MapSize", bVisible);
                m_eResultTop = (InfoStrip.eResult)tree.Set(m_eResultTop, m_eResultTop, "Top Result", "Top Result", bVisible);
                m_sMapTop = tree.Set(m_sMapTop, m_sMapTop, "Top Unit", "Top Unit", bVisible); 
                m_eResultBottom = (InfoStrip.eResult)tree.Set(m_eResultBottom, m_eResultBottom, "Bottom Result", "Bottom Result", bVisible);
                m_sMapBottom = tree.Set(m_sMapBottom, m_sMapBottom, "Bottom Unit", "Bottom Unit", bVisible);
            }

            public override string Run()
            {
                m_infoStrip.SetResult(eVision.Top2D, m_eResultTop.ToString(), m_szMap.X.ToString(), m_szMap.Y.ToString(), m_sMapTop);
                m_infoStrip.SetResult(eVision.Bottom, m_eResultBottom.ToString(), m_szMap.X.ToString(), m_szMap.Y.ToString(), m_sMapBottom);
                return m_module.m_handler.m_summary.SetSort(false, m_infoStrip); 
            }
        }
        #endregion
    }
}
