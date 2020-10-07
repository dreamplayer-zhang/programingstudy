using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System.ComponentModel;
using System.Threading;

namespace Root_ASIS.Module
{
    public class Picker : NotifyProperty
    {
        #region ToolBox
        DIO_I2O m_dioDown;
        DIO_IO m_dioVacuum;
        DIO_O m_doBlow; 
        public void GetTools(ModuleBase module, bool bInit)
        {
            module.p_sInfo = module.m_toolBox.Get(ref m_dioDown, module, "UpDown", "Up", "Down");
            module.p_sInfo = module.m_toolBox.Get(ref m_dioVacuum, module, "Vacuum");
            module.p_sInfo = module.m_toolBox.Get(ref m_doBlow, module, "Blow");
        }
        #endregion

        #region DIO Functions
        public bool IsDown()
        {
            return m_dioDown.m_aBitDI[1].p_bOn || (m_dioDown.m_aBitDI[0].p_bOn == false);
        }

        bool m_bFastUp = false;
        bool IsMoveDone(bool bDown)
        {
            if (bDown || m_bFastUp == false) return m_dioDown.p_bDone;
            return (m_dioDown.m_aBitDI[1].p_bOn == false);
        }

        double m_secUpDown = 5;
        string RunDown(bool bDown)
        {
            m_dioDown.Write(bDown);
            Thread.Sleep(100);
            int msWait = (int)(1000 * m_secUpDown);
            while (IsMoveDone(bDown) != true)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return p_id + " EQ Stop";
                if (m_dioDown.m_swWrite.ElapsedMilliseconds > msWait) return m_dioDown.m_id + " Sol Valve Move Timeout";
            }
            return "OK"; 
        }

        double m_secVac = 2;
        double m_secBlow = 0.5; 
        string RunVacuum(bool bOn)
        {
            m_dioVacuum.Write(bOn);
            if (bOn == false)
            {
                m_doBlow.Write(true);
                Thread.Sleep((int)(1000 * m_secBlow));
                m_doBlow.Write(false);
                return "OK";
            }
            int msVac = (int)(1000 * m_secVac);
            while (m_dioVacuum.p_bIn != bOn)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return p_id + " EQ Stop";
                if (m_dioVacuum.m_swWrite.ElapsedMilliseconds > msVac) return "Vacuum Sensor Timeout";
            }
            return "OK";
        }

        void RunTreeTimeout(Tree tree)
        {
            m_secUpDown = tree.Set(m_secUpDown, m_secUpDown, "UpDown", "Picker Up Down Time (sec)");
            m_bFastUp = tree.Set(m_bFastUp, m_bFastUp, "FastUp", "Do not Wait Up Sensor On"); 
            m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum Sensor Wait (sec)");
            m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Blow Time (sec)");
            m_secBackground = tree.Set(m_secBackground, m_secBackground, "Background", "Background Run Timeout (sec)"); 
        }
        #endregion

        #region BackgroundWorker
        readonly object m_csLock = new object();

        BackgroundWorker m_bgw = new BackgroundWorker();
        void InitBackgroundWork()
        {
            m_bgw.DoWork += M_bgwLoad_DoWork;
        }

        public bool p_bBusy
        {
            get { return m_bgw.IsBusy; }
        }

        double m_secBackground = 5; 
        public string WaitReady()
        {
            long msBackground = (long)(1000 * m_secBackground); 
            while (m_swBusy.ElapsedMilliseconds < msBackground)
            {
                Thread.Sleep(10);
                if (p_bBusy == false) return "OK";
            }
            return "Background Run Timeout"; 
        }

        bool m_bLoadBGW = true;
        StopWatch m_swBusy = new StopWatch(); 
        private void M_bgwLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            m_swBusy.Start(); 
            if (m_bLoadBGW) RunLoad();
            else RunUnload(); 
        }
        #endregion

        #region RunLoad
        public string StartLoad()
        {
            if (p_bBusy) return "Picker BackgroundWorker Busy";
            m_bLoadBGW = true; 
            m_bgw.RunWorkerAsync(); 
            return "OK"; 
        }

        int m_nRetry = 3;
        public string RunLoad()
        {
            lock (m_csLock)
            {
                try
                {
                    for (int n = 0; n < m_nRetry; n++)
                    {
                        if (Run(RunDown(false))) return m_sRun;
                        if (Run(RunDown(true))) return m_sRun;
                        if (Run(RunVacuum(true))) return m_sRun;
                        if (Run(RunDown(false))) return m_sRun;
                        if (m_dioVacuum.p_bIn) return "OK";
                    }
                    return p_id + " Run Load Error"; 
                }
                finally { RunDown(false); }
            }
        }

        double m_secLoadEVDown = 0.5;
        public string RunLoadEV(LoadEV loadEV)
        {
            lock (m_csLock)
            {
                try
                {
                    for (int n = 0; n < m_nRetry; n++)
                    {
                        if (Run(RunVacuum(false))) return m_sRun;
                        while (loadEV.p_bDone == false)
                        {
                            Thread.Sleep(10);
                            if (EQ.IsStop()) return "EQ Stop";
                        }
                        if (Run(RunDown(true))) return m_sRun;
                        if (Run(RunVacuum(true))) return m_sRun;
                        loadEV.p_bBlow = true;
                        loadEV.StartDown(m_secLoadEVDown);
                        RunShake();
                        if (Run(RunDown(false))) return m_sRun;
                        loadEV.p_bBlow = false;
                        if (m_dioVacuum.p_bIn)
                        {
                            loadEV.StartLoad();
                            return "OK";
                        }
                    }
                    return p_id + " Run Load Error";
                }
                finally
                {
                    loadEV.p_bBlow = false;
                    RunDown(false);
                }
            }
        }

        void RunTreeLoad(Tree tree)
        {
            m_nRetry = tree.Set(m_nRetry, m_nRetry, "Retry", "Load Retry Count");
            m_secLoadEVDown = tree.Set(m_secLoadEVDown, m_secLoadEVDown, "LoadEV Down", "LoadElevator Down Time (sec)");
            RunTreeShake(tree.GetTree("Shake", false)); 
        }

        int m_nShake = 3;
        double m_secShakeUP = 0.1;
        double m_secShakeDown = 0.08;
        void RunShake()
        {
            for (int n = 0; n < m_nShake; n++)
            {
                m_dioDown.Write(false);
                Thread.Sleep((int)(1000 * m_secShakeUP));
                m_dioDown.Write(true);
                Thread.Sleep((int)(1000 * m_secShakeDown));
            }
        }

        void RunTreeShake(Tree tree)
        {
            m_nShake = tree.Set(m_nShake, m_nShake, "Count", "Load Shake Count");
            m_secShakeUP = tree.Set(m_secShakeUP, m_secShakeUP, "Up Time", "Shake Up Time (sec)");
            m_secShakeDown = tree.Set(m_secShakeDown, m_secShakeDown, "Down Time", "Shake Down Time (sec)");
        }
        #endregion

        #region RunUnload
        public string StartUnload()
        {
            if (p_bBusy) return "Picker BackgroundWorker Busy";
            m_bLoadBGW = false;
            m_bgw.RunWorkerAsync();
            return "OK"; 
        }

        public string RunUnload()
        {
            lock (m_csLock)
            {
                if (Run(RunDown(true))) return m_sRun;
                if (Run(RunVacuum(false))) return m_sRun;
                if (Run(RunDown(false))) return m_sRun;
                return "OK";
            }
        }
        #endregion

        #region Thread Vacuum Check
        bool m_bThread = false;
        Thread m_thread;
        void InitThread()
        {
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }

        void RunThread()
        {
            m_bThread = true;
            Thread.Sleep(2000);
            while (m_bThread)
            {
                Thread.Sleep(10);
                p_bVacuumError = ((p_infoStrip != null) && m_dioVacuum.p_bOut && (m_dioVacuum.p_bIn == false)); 
            }
        }

        bool _bVacuumError = false; 
        public bool p_bVacuumError
        {
            get { return _bVacuumError; }
            set
            {
                if (_bVacuumError == value) return;
                _bVacuumError = value;
                OnPropertyChanged(); 
                if (value)
                {
                    EQ.p_bStop = true;
                    m_module.p_sInfo = "Picker Vacuum Check Error"; 
                }
            }
        }
        #endregion

        #region InfoStrip
        InfoStrip _infoStrip = null;
        public InfoStrip p_infoStrip
        {
            get { return _infoStrip; }
            set
            {
                _infoStrip = value;
                OnPropertyChanged();
                m_reg.Write("iStrip", (value == null) ? -1 : value.p_iStrip); 
            }
        }

        void InitStrip()
        {
            int iStrip = m_reg.Read("iStrip", -1);
            if (iStrip < 0) return;
            p_infoStrip = new InfoStrip(iStrip); 
        }
        #endregion

        #region Tree & Reset
        public void RunTree(Tree tree)
        {
            RunTreeLoad(tree.GetTree("Load", false));
            RunTreeTimeout(tree.GetTree("Timeout", false)); 
        }

        public void Reset()
        {
            m_dioDown.Write(false);
            if (m_dioVacuum.p_bIn == false) p_infoStrip = null; 
        }

        string m_sRun = "";
        bool Run(string sOK)
        {
            m_sRun = sOK;
            return sOK != "OK";
        }
        #endregion

        public string p_id { get; set; }
        ModuleBase m_module;
        Registry m_reg; 
        public Picker(string id, ModuleBase module)
        {
            p_id = id;
            m_module = module;
            m_reg = new Registry(p_id);
            InitStrip(); 
            InitThread();
            InitBackgroundWork(); 
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Join(); 
            }
        }
    }
}
