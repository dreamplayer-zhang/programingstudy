using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System.Threading;

namespace Root_ASIS.Module
{
    public class LoadEV : ModuleBase
    {
        #region ToolBox
        DIO_I2O2 m_dioEV;
        DIO_I m_diTop;
        DIO_I m_diCheck;
        DIO_I m_diBlowAlarm; 
        DIO_I m_diPaper;
        DIO_I m_diPaperCheck;
        DIO_I m_diPaperFull; 
        DIO_O m_doIonBlow;
        DIO_O m_doAlignBlow;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_dioEV, this, "Elevator", "Down", "Up");
            p_sInfo = m_toolBox.Get(ref m_diTop, this, "Top");
            p_sInfo = m_toolBox.Get(ref m_diCheck, this, "Check");
            p_sInfo = m_toolBox.Get(ref m_diBlowAlarm, this, "BlowAlarm");
            p_sInfo = m_toolBox.Get(ref m_diPaper, this, "Paper");
            p_sInfo = m_toolBox.Get(ref m_diPaperCheck, this, "PaperCheck");
            p_sInfo = m_toolBox.Get(ref m_diPaperFull, this, "PaperFull");
            p_sInfo = m_toolBox.Get(ref m_doIonBlow, this, "IonBlow");
            p_sInfo = m_toolBox.Get(ref m_doAlignBlow, this, "AlignBlow");
            if (bInit) InitTools();
        }

        void InitTools()
        {
        }
        #endregion

        #region Property
        bool _bPaper = false; 
        public bool p_bPaper
        {
            get { return _bPaper; }
            set
            {
                if (_bPaper == value) return;
                m_log.Info("p_bPaper = " + value.ToString()); 
                _bPaper = value;
                OnPropertyChanged();
                if (value) p_infoStrip = null; 
            }
        }

        bool _bCheck = false; 
        public bool p_bCheck
        {
            get { return _bCheck; }
            set
            {
                if (_bCheck == value) return;
                m_log.Info("p_bCheck = " + value.ToString());
                _bCheck = value;
                OnPropertyChanged();
                if (value == false) p_infoStrip = null; 
            }
        }

        bool _bBlow = false; 
        public bool p_bBlow
        {
            get { return _bBlow; }
            set
            {
                if (_bBlow == value) return; 
                _bBlow = value;
                m_log.Info("p_bBlow = " + value.ToString());
                OnPropertyChanged();
                m_doAlignBlow.Write(value);
                m_doIonBlow.Write(value); 
            }
        }
        #endregion

        #region InfoStrip
        int _iStrip = 0; 
        public int p_iStrip
        {
            get { return _iStrip; }
            set
            {
                if (_iStrip == value) return;
                _iStrip = value;
                OnPropertyChanged(); 
            }
        }

        InfoStrip _infoStrip = null;
        public InfoStrip p_infoStrip
        {
            get { return _infoStrip; }
            set
            {
                _infoStrip = value;
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region MoveEV
        public enum eMove
        {
            Up,
            Stop,
            Down
        }

        eMove _eMove = eMove.Stop; 
        public eMove p_eMove
        {
            get { return _eMove; }
            set
            {
                if (_eMove == value) return;
                m_log.Info("p_eMove : " + _eMove.ToString() + " -> " + value.ToString());
                m_dioEV.m_aBitDO[0].Write(false);
                m_dioEV.m_aBitDO[1].Write(false);
                if ((_eMove != eMove.Stop) && (value != eMove.Stop)) Thread.Sleep(100); 
                _eMove = value;
                OnPropertyChanged(); 
                switch (value)
                {
                    case eMove.Up: m_dioEV.Write(true); break;
                    case eMove.Down: m_dioEV.Write(false); break;
                }
            }
        }
        #endregion

        #region RunLoad
        public string RunLoad(double secTimeout)
        {
            StopWatch sw = new StopWatch();
            int msTimeout = (int)(1000 * secTimeout);
            p_bCheck = m_diCheck.p_bIn;
            if (m_diBlowAlarm.p_bIn) return "Blow Alarm DI Detected"; 
            try
            {
                if (m_dioEV.m_aBitDI[1].p_bOn)
                {
                    p_eMove = eMove.Down;
                    while (m_dioEV.m_aBitDI[1].p_bOn)
                    {
                        Thread.Sleep(10);
                        if (sw.ElapsedMilliseconds > msTimeout) return "RunLoad Timeout : Up Sensor";
                    }
                    if (m_diTop.p_bIn == false) p_eMove = eMove.Stop;
                }
                if (m_diTop.p_bIn)
                {
                    p_eMove = eMove.Down;
                    while (m_diTop.p_bIn)
                    {
                        Thread.Sleep(10);
                        if (sw.ElapsedMilliseconds > msTimeout) return "RunLoad Timeout : Top Sensor Down";
                    }
                    p_eMove = eMove.Stop;
                }
                p_eMove = eMove.Up;
                while (m_diTop.p_bIn == false)
                {
                    Thread.Sleep(1);
                    if (sw.ElapsedMilliseconds > msTimeout) return "RunLoad Timeout : Top Sensor Up";
                }
                p_eMove = eMove.Stop;
                p_bPaper = m_diPaper.p_bIn;
                AddNewInfoStrip(); 
                return "OK";
            }
            finally
            {
                p_eMove = eMove.Stop; 
            }
        }

        void AddNewInfoStrip()
        {
            if (p_infoStrip != null) return;
            if (p_bCheck == false) return;
            if (p_bPaper) return;
            p_infoStrip = new InfoStrip(p_iStrip);
            p_iStrip++; 
        }
        #endregion

        #region RunDown
        public string RunDown(double secDown)
        {
            StopWatch sw = new StopWatch();
            int msDown = (int)(1000 * secDown);
            try
            {
                p_eMove = eMove.Down; 
                while ((sw.ElapsedMilliseconds < msDown) && (EQ.IsStop() == false)) Thread.Sleep(10);
            }
            finally
            {
                p_eMove = eMove.Stop; 
            }
            return "OK"; 
        }
        #endregion

        #region Override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
        }

        public override void Reset()
        {
            base.Reset();
            StartLoad(); 
        }
        #endregion

        #region public Functions
        public void StartLoad()
        {
            StartRun(m_runLoad);
        }

        public void StartDown()
        {
            StartRun(m_runDown);
        }
        #endregion

        public LoadEV(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            p_eMove = eMove.Stop;
            p_bBlow = false; 
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runLoad;
        ModuleRunBase m_runDown; 
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Just Time Delay");
            m_runLoad = AddModuleRunList(new Run_Load(this), false, "Run Elevator Load");
            m_runDown = AddModuleRunList(new Run_Down(this), false, "Run Elevator Down");
            AddModuleRunList(new Run_Blow(this), false, "Run Blow");
        }

        public class Run_Delay : ModuleRunBase
        {
            LoadEV m_module;
            public Run_Delay(LoadEV module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_secDelay = 2;
            public override ModuleRunBase Clone()
            {
                Run_Delay run = new Run_Delay(m_module);
                run.m_secDelay = m_secDelay;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }

            public override string Run()
            {
                Thread.Sleep((int)(1000 * m_secDelay));
                return "OK";
            }
        }

        public class Run_Load : ModuleRunBase
        {
            LoadEV m_module;
            public Run_Load(LoadEV module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_secTimeout = 8; 
            public override ModuleRunBase Clone()
            {
                Run_Load run = new Run_Load(m_module);
                run.m_secTimeout = m_secTimeout; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secTimeout = tree.Set(m_secTimeout, m_secTimeout, "Timeout", "RunLoad Timeout (sec)", bVisible);
            }

            public override string Run()
            {
                return m_module.RunLoad(m_secTimeout); 
            }
        }

        public class Run_Down : ModuleRunBase
        {
            LoadEV m_module;
            public Run_Down(LoadEV module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_secDown = 3;
            public override ModuleRunBase Clone()
            {
                Run_Down run = new Run_Down(m_module);
                run.m_secDown = m_secDown;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secDown = tree.Set(m_secDown, m_secDown, "Down Time", "RunDown Time (sec)", bVisible);
            }

            public override string Run()
            {
                return m_module.RunDown(m_secDown);
            }
        }

        public class Run_Blow : ModuleRunBase
        {
            LoadEV m_module;
            public Run_Blow(LoadEV module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bBlow = true;
            public override ModuleRunBase Clone()
            {
                Run_Blow run = new Run_Blow(m_module);
                run.m_bBlow = m_bBlow;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bBlow = tree.Set(m_bBlow, m_bBlow, "Blow", "Run Blow", bVisible);
            }

            public override string Run()
            {
                if (m_module.m_diBlowAlarm.p_bIn) return "Blow Alarm DI Detected";
                m_module.p_bBlow = m_bBlow; 
                return "OK";
            }
        }
        #endregion
    }
}
