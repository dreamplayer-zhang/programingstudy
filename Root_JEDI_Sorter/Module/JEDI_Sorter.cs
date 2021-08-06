using Root_JEDI_Sorter.Engineer;
using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.IO;
using System.Threading;

namespace Root_JEDI_Sorter.Module
{
    public class JEDI_Sorter : ModuleBase
    {
        #region ToolBox
        DIO_IO m_dioStart;
        DIO_IO m_dioStop;
        DIO_IO m_dioReset;
        DIO_IO m_dioHome;
        DIO_I m_diEmergency;
        DIO_I m_diDoorOpen;
        DIO_I[] m_diCDA = new DIO_I[2] { null, null };
        DIO_O m_doFFU;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetDIO(ref m_dioStart, this, "Start", false);
            m_toolBox.GetDIO(ref m_dioStop, this, "Stop", false);
            m_toolBox.GetDIO(ref m_dioReset, this, "Reset", false);
            m_toolBox.GetDIO(ref m_dioHome, this, "Home", false);
            m_toolBox.GetDIO(ref m_diEmergency, this, "Emergency");
            m_toolBox.GetDIO(ref m_diDoorOpen, this, "Door Open");
            m_toolBox.GetDIO(ref m_diCDA[0], this, "CDA 1");
            m_toolBox.GetDIO(ref m_diCDA[1], this, "CDA 2");
            m_toolBox.GetDIO(ref m_doFFU, this, "FFU");
            m_lamp.GetTools(m_toolBox, this);
            m_buzzer.GetTools(m_toolBox, this);
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
                    ((JEDI_Sorter_Engineer)m_engineer).m_ajin.m_listAxis.RunEmergency();
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
            p_bCDA = m_diCDA[0].p_bIn || m_diCDA[1].p_bIn;
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

        #region override
        public override string StateHome()
        {
            return "OK";
        }

        public override void Reset()
        {
            m_buzzer.RunBuzzerOff();
            base.Reset();
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            m_buzzer.RunTree(tree.GetTree("Buzzer"));
            m_tray.RunTree(tree);
        }
        #endregion

        #region Recipe
        const string c_sExt = ".JEDI";
        public void RecipeSave()
        {
            if (m_handler.p_sRecipe == "") return;
            string sPath = EQ.c_sPathRecipe + "\\" + m_handler.p_sRecipe;
            Directory.CreateDirectory(sPath);
            string sFile = sPath + "\\JEDI" + c_sExt;
            m_treeRootSetup.m_job = new Job(sFile, true, m_log);
            RunTree(Tree.eMode.JobSave);
            m_treeRootSetup.m_job.Close();
        }

        public void RecipeOpen(string sRecipe)
        {
            string sPath = EQ.c_sPathRecipe + "\\" + sRecipe;
            Directory.CreateDirectory(sPath);
            string sFile = sPath + "\\JEDI" + c_sExt;
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

        Registry m_reg = new Registry("JEDI");
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
        #endregion

        public Tray m_tray = new Tray(); 
        JEDI_Sorter_Handler m_handler;
        public JEDI_Sorter(string id, IEngineer engineer)
        {
            p_id = id;
            m_handler = (JEDI_Sorter_Handler)engineer.ClassHandler();
            p_sLotID = m_reg.Read("LotID", "");
            InitBase(id, engineer);

            InitThread();
        }

        public override void ThreadStop()
        {
            ThreadDIOStop();
            base.ThreadStop();
        }

    }
}
