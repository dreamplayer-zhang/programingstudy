using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.ComponentModel;
using System.Threading;

namespace Root_ASIS.Module
{
    public  class Cleaner : ModuleBase
    {
        #region eCleaner
        public enum eCleaner
        {
            Cleaner0,
            Cleaner1
        }
        #endregion 

        #region ToolBox
        Axis[] m_axis = new Axis[2];
        DIO_I[] m_diSensor = new DIO_I[2];
        DIO_I[] m_diProduct = new DIO_I[2];
        DIO_I2O[] m_dioGrip = new DIO_I2O[2];
        DIO_O[] m_doAlign = new DIO_O[2];
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axis[0], this, "Pusher");
            p_sInfo = m_toolBox.Get(ref m_axis[1], this, "Gripper");
            p_sInfo = m_toolBox.Get(ref m_diSensor[0], this, "Pusher Sensor");
            p_sInfo = m_toolBox.Get(ref m_diSensor[1], this, "Gripper Sensor");
            p_sInfo = m_toolBox.Get(ref m_diProduct[0], this, "Pusher Product");
            p_sInfo = m_toolBox.Get(ref m_diProduct[1], this, "Gripper Product");
            p_sInfo = m_toolBox.Get(ref m_dioGrip[0], this, "Pusher Grip", "Off", "Grip");
            p_sInfo = m_toolBox.Get(ref m_dioGrip[1], this, "Gripper Grip", "Off", "Grip");
            p_sInfo = m_toolBox.Get(ref m_doAlign[0], this, "Pusher Align");
            p_sInfo = m_toolBox.Get(ref m_doAlign[1], this, "Gripper Align");
            m_blowTop.GetTools("Blow Top", m_toolBox);
            m_blowBottom.GetTools("Blow Bottom", m_toolBox);
            if (m_railWidth != null) m_railWidth.GetTools(m_toolBox); 
            if (bInit) InitTools();
        }

        void InitTools()
        {
            InitPosPusher();
            InitPosGripper(); 
        }
        #endregion

        #region Rail Width
        public class RailWidth
        {
            Axis m_axis; 
            public void GetTools(ToolBox toolBox)
            {
                m_cleaner.p_sInfo = toolBox.Get(ref m_axis, m_cleaner, "RailWidth"); 
            }

            double[] m_posAxis = new double[2];
            double[] m_fWidth = new double[2]; 
            public string ChangeWidth(bool bWait)
            {
                double fScale = (Strip.p_szStrip.X - m_fWidth[0]) / (m_fWidth[1] - m_fWidth[0]);
                double fPos = fScale * (m_posAxis[1] - m_posAxis[0]);
                string sMove = m_axis.StartMove(fPos);
                if (sMove != "OK") return sMove;
                if (bWait == false) return "OK"; 
                return m_axis.WaitReady(); 
            }

            public void RunTree(Tree tree)
            {
                RunTreePos(tree.GetTree("Position 0", false), 0);
                RunTreePos(tree.GetTree("Position 1", false), 1);
            }

            void RunTreePos(Tree tree, int nIndex)
            {
                m_posAxis[nIndex] = tree.Set(m_posAxis[nIndex], m_posAxis[nIndex], "Axis", "Axis Position (unit)");
                m_fWidth[nIndex] = tree.Set(m_fWidth[nIndex], m_fWidth[nIndex], "Width", "Rail Width (mm)"); 
            }

            Cleaner m_cleaner; 
            public RailWidth(Cleaner cleaner)
            {
                m_cleaner = cleaner; 
            }
        }
        RailWidth m_railWidth = null; 
        #endregion

        #region DIO_Blow
        public class DIO_Blow
        {
            DIO_IO[] m_dioBlow = new DIO_IO[3];
            public void GetTools(string sGroup, ToolBox toolBox)
            {
                m_cleaner.p_sInfo = toolBox.Get(ref m_dioBlow[0], m_cleaner, sGroup + "0");
                m_cleaner.p_sInfo = toolBox.Get(ref m_dioBlow[1], m_cleaner, sGroup + "1");
                m_cleaner.p_sInfo = toolBox.Get(ref m_dioBlow[2], m_cleaner, sGroup + "2");
            }

            public bool m_bUseDI = false;
            public bool[] m_abBlowOn = new bool[3] { false, false, false };
            public void RunTree(Tree tree)
            {
                m_bUseDI = tree.Set(m_bUseDI, m_bUseDI, "Use DI", "Use DI or SW");
                for (int n = 0; n < 3; n++)
                {
                    m_abBlowOn[n] = tree.Set(m_abBlowOn[n], m_abBlowOn[n], "Blow On " + n.ToString(), "Blow On or Off", !m_bUseDI);
                }
            }

            bool _bBlowOn = false;
            public bool p_bBlowOn
            {
                get { return _bBlowOn; }
                set
                {
                    _bBlowOn = value;
                    for (int n = 0; n < 3; n++)
                    {
                        bool bBlowOn = m_bUseDI ? m_dioBlow[n].p_bIn : m_abBlowOn[n];
                        m_dioBlow[n].Write(bBlowOn && value && m_cleaner.p_bUseBlow);
                    }
                }
            }

            string m_id;
            Cleaner m_cleaner;
            public DIO_Blow(string id, Cleaner cleaner)
            {
                m_id = id;
                m_cleaner = cleaner;
            }
        }
        public DIO_Blow m_blowTop;
        public DIO_Blow m_blowBottom;

        bool _bUseBlow = false;
        public bool p_bUseBlow
        {
            get { return _bUseBlow; }
            set
            {
                if (_bUseBlow == value) return;
                m_log.Info("bUseBlow = " + value.ToString());
                _bUseBlow = value;
                OnPropertyChanged();
            }
        }

        public bool p_bBlowOn
        {
            set
            {
                m_blowTop.p_bBlowOn = value;
                m_blowBottom.p_bBlowOn = value; 

            }
        }
        #endregion

        #region DIO Function
        bool m_bUseCheckProduct = true; 
        string CheckProduct(int nSide, bool bCheck)
        {
            if (m_bUseCheckProduct == false) return "OK"; 
            if (m_diProduct[nSide].p_bIn == bCheck) return "OK";
            return "Not OK"; 
        }

        double m_secAlign = 0.1; 
        string RunAlign(int nSide)
        {
            int msAlign = (int)(1000 * m_secAlign); 
            m_doAlign[nSide].Write(true);
            Thread.Sleep(msAlign);
            m_doAlign[nSide].Write(false);
            Thread.Sleep(100);
            return "OK";
        }

        string RunGrip(int nSide, bool bGrip)
        {
            m_dioGrip[nSide].Write(bGrip);
            return m_dioGrip[nSide].WaitDone(); 
        }

        void RunTreeDIO(Tree tree)
        {
            m_secAlign = tree.Set(m_secAlign, m_secAlign, "Align", "DO Align Delay (sec)");
            m_bUseCheckProduct = tree.Set(m_bUseCheckProduct, m_bUseCheckProduct, "Product", "Check ProductSensor"); 
        }
        #endregion

        #region Axis Pusher
        public enum ePosPusher
        {
            Load,
            Ready,
        }
        void InitPosPusher()
        {
            m_axis[0].AddPos(Enum.GetNames(typeof(ePosPusher)));
        }
        #endregion

        #region Axis Gripper
        public enum ePosGripper
        {
            Start,
            Done,
        }

        public enum eSpeed
        {
            Clean
        }

        void InitPosGripper()
        {
            m_axis[1].AddPos(Enum.GetNames(typeof(ePosGripper)));
            m_axis[1].AddSpeed(Enum.GetNames(typeof(eSpeed))); 
        }
        #endregion

        #region RunClean
        public string RunCleaner()
        {
            while (p_infoStrip1 != null)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop"; 
            }
            try
            {
                StartBackgroudWork();
                if (m_diSensor[0].p_bIn || m_diSensor[1].p_bIn) return "Check Clean Sensor";
                double dY = Strip.p_szStrip.Y - Strip.m_szStripTeach.Y;
                if (CheckProduct(0, true) != "OK") return "Pusher Side Product Sensor not Detected";
                if (CheckProduct(1, false) != "OK") return "Gripper Side Product Sensor Detected";
                m_axis[0].StartMove(ePosPusher.Load, -dY / 2);
                m_axis[1].StartMove(ePosGripper.Start);
                if (Run(m_axis[0].WaitReady())) return p_sInfo;
                if (Run(RunAlign(0))) return p_sInfo;
                m_dioGrip[1].Write(false);
                if (Run(RunGrip(0, true))) return p_sInfo;
                if (Run(RunGrip(1, false))) return p_sInfo;
                m_axis[0].StartMove(ePosPusher.Ready, -dY / 2);
                if (Run(m_axis[0].WaitReady())) return p_sInfo;
                if (Run(m_axis[1].WaitReady())) return p_sInfo;
                if (Run(RunGrip(1, true))) return p_sInfo;
                if (Run(RunGrip(0, false))) return p_sInfo;
                m_axis[0].StartMove(0);
                p_bBlowOn = true;
                m_axis[1].StartMove(ePosGripper.Start, -Strip.p_szStrip.Y, eSpeed.Clean);
                if (Run(m_axis[1].WaitReady())) return p_sInfo;
                p_bBlowOn = false;
                m_axis[1].StartMove(ePosGripper.Done, -dY / 2);
                if (Run(m_axis[1].WaitReady())) return p_sInfo;
                if (Run(RunGrip(1, false))) return p_sInfo;
                m_axis[1].StartMove(0);
                if (Run(RunAlign(1))) return p_sInfo;
                if (Run(m_axis[0].WaitReady())) return p_sInfo;
                if (Run(m_axis[1].WaitReady())) return p_sInfo;
                if (CheckProduct(0, false) != "OK") return "Pusher Side Product Sensor Detected";
                if (CheckProduct(1, true) != "OK") return "Gripper Side Product Sensor not Detected";
                if (m_bgw0.IsBusy || m_bgw1.IsBusy)
                {
                    m_bRunBGW = false;
                    return "Check Strip Length Error";
                }
                if (Run(CompareLength(0))) return p_sInfo;
                if (Run(CompareLength(1))) return p_sInfo;
                p_infoStrip1 = p_infoStrip0;
                p_infoStrip0 = null;
                return "OK";
            }
            finally
            {
                p_bBlowOn = false; 
                RunGrip(0, false);
                RunGrip(1, false);
                m_axis[0].StartMove(0);
                m_axis[1].StartMove(0);
                m_bRunBGW = false;
            }
        }

        public string StartCleaner()
        {
            return StartRun(m_runClean); //forget Recipe
        }

        double m_fCompareError = 10; 
        string CompareLength(int nSide)
        {
            double fError = 100 * Math.Abs(m_aLength[nSide] - Strip.p_szStrip.Y);
            if (fError < m_fCompareError) return "OK"; 
            return "Check Strip Length Error : " + nSide.ToString(); 
        }

        void RunTreeLength(Tree tree)
        {
            m_fCompareError = tree.Set(m_fCompareError, m_fCompareError, "Error", "Check Length Error (%)"); 
        }
        #endregion

        #region BackgroundWork
        bool m_bRunBGW = false; 
        double[] m_aLength = new double[2]; 
        BackgroundWorker m_bgw0 = new BackgroundWorker();
        BackgroundWorker m_bgw1 = new BackgroundWorker();
        void InitBackgroundWork()
        {
            m_bgw0.DoWork += M_bgw0_DoWork;
            m_bgw1.DoWork += M_bgw1_DoWork;
        }

        void StartBackgroudWork()
        {
            m_bRunBGW = true;
            m_bgw0.RunWorkerAsync();
            m_bgw1.RunWorkerAsync();
        }

        private void M_bgw1_DoWork(object sender, DoWorkEventArgs e)
        {
            CheckLength(0); 
        }

        private void M_bgw0_DoWork(object sender, DoWorkEventArgs e)
        {
            CheckLength(1);
        }

        void CheckLength(int nSide)
        {
            m_aLength[nSide] = 0;
            while (m_diSensor[nSide].p_bIn == false)
            {
                Thread.Sleep(1);
                if (m_bRunBGW == false) return; 
            }
            double fStart = m_axis[nSide].p_posCommand;
            while (m_diSensor[nSide].p_bIn)
            {
                Thread.Sleep(1);
                if (m_bRunBGW == false) return;
            }
            m_aLength[nSide] = Math.Abs(m_axis[nSide].p_posCommand - fStart); 
        }
        #endregion

        #region InfoStrip
        InfoStrip _infoStrip0 = null;
        public InfoStrip p_infoStrip0
        {
            get { return _infoStrip0; }
            set
            {
                _infoStrip0 = value;
                OnPropertyChanged();
                m_reg.Write("iStrip0", (value == null) ? -1 : value.p_iStrip);
            }
        }

        InfoStrip _infoStrip1 = null;
        public InfoStrip p_infoStrip1
        {
            get { return _infoStrip1; }
            set
            {
                _infoStrip1 = value;
                OnPropertyChanged();
                m_reg.Write("iStrip1", (value == null) ? -1 : value.p_iStrip);
            }
        }

        void InitStrip()
        {
            int iStrip0 = m_reg.Read("iStrip0", -1);
            if (iStrip0 >= 0) p_infoStrip0 = new InfoStrip(iStrip0);
            int iStrip1 = m_reg.Read("iStrip1", -1);
            if (iStrip1 >= 0) p_infoStrip1 = new InfoStrip(iStrip1);
        }
        #endregion

        #region Override
        public override string StateReady()
        {
            if (EQ.p_eState != EQ.eState.Run) return "OK";
            if (p_infoStrip0 == null) return "OK";
            if (p_infoStrip1 != null) return "OK";
            StartRun(m_runClean);
            return "OK";
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            RunTreeDIO(tree.GetTree("DIO", false));
            RunTreeLength(tree.GetTree("Check Length", false));
            m_blowTop.RunTree(tree.GetTree("Blow Top", false));
            m_blowBottom.RunTree(tree.GetTree("Blow Bottom", false));
        }

        public override void Reset()
        {
            if (m_railWidth != null) m_railWidth.ChangeWidth(false); 
            base.Reset();
        }
        #endregion

        Registry m_reg;
        int m_nID = 0; 
        public Cleaner(string id, int nID, IEngineer engineer)
        {
            m_nID = nID; 
            m_reg = new Registry(id);
            m_blowTop = new DIO_Blow("Blow Top", this);
            m_blowBottom = new DIO_Blow("Blow Bottom", this);
            if (nID == 0) m_railWidth = new RailWidth(this); 
            InitStrip();
            InitBackgroundWork(); 
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            Reset();
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runClean;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            m_runClean = AddModuleRunList(new Run_Cleaner(this), false, "Run Cleaner");
        }

        public class Run_Delay : ModuleRunBase
        {
            Cleaner m_module;
            public Run_Delay(Cleaner module)
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

        public class Run_Cleaner : ModuleRunBase
        {
            Cleaner m_module;
            public Run_Cleaner(Cleaner module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Cleaner run = new Run_Cleaner(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunCleaner();
            }
        }
        #endregion
    }
}
