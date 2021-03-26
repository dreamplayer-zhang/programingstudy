using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Root_ASIS.Module
{
    public class Sorter0 : ModuleBase
    {
        #region ToolBox
        Axis m_axisX;
        Axis m_axisZ;
        DIO_I m_diEmg;
        DIO_I m_diSafeZ;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetAxis(ref m_axisX, this, "Axis X");
            p_sInfo = m_toolBox.GetAxis(ref m_axisZ, this, "Axis Z");
            p_sInfo = m_toolBox.GetDIO(ref m_diEmg, this, "Emegency");
            p_sInfo = m_toolBox.GetDIO(ref m_diSafeZ, this, "Safe Z Position");
            m_picker.GetTools(this, bInit);
            if (bInit) InitTools();
        }

        void InitTools()
        {
            m_axisX.AddPos(Enum.GetNames(typeof(ePosX)));
            m_axisZ.AddPos(Enum.GetNames(typeof(ePosZ)));
        }
        #endregion

        #region DIO
        bool _bEMG = false;
        public bool p_bEMG
        {
            get { return _bEMG; }
            set
            {
                if (_bEMG == value) return;
                EMGStop("Emergency Sensor Checked");
                _bEMG = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Picker
        PickerFix m_picker;
        void InitPicker()
        {
            m_picker = new PickerFix(p_id + ".Picker", this);
        }
        #endregion

        #region AxisMoveX
        public enum ePosX
        {
            Cleaner0,
            Cleaner1,
            TrayLeft,
            TrayRight
        }
        string AxisMoveX(ePosX ePosX, double xOffset = 0)
        {
            if (Run(m_axisX.StartMove(ePosX, xOffset))) return p_sInfo; 
            return m_axisX.WaitReady(); 
        }

        double GetTrayOffsetX(int xTray)
        {
            double x0 = m_axisX.GetPosValue(ePosX.TrayLeft);
            double x1 = m_axisX.GetPosValue(ePosX.TrayRight);
            return xTray * (x1 - x0) / (m_trays.p_szTray.X - 1);
        }
        #endregion

        #region AxisMoveZ
        public enum ePosZ
        {
            Cleaner0,
            Cleaner1,
            Ready,
            TrayBottom,
            TrayTop
        }
        string AxisMoveZ(ePosZ ePosZ, double zOffset = 0)
        {
            if (Run(m_axisZ.StartMove(ePosZ, zOffset))) return p_sInfo;
            if (Run(m_axisZ.WaitReady())) return p_sInfo;
            if ((ePosZ != ePosZ.TrayBottom) && (ePosZ != ePosZ.TrayTop)) return "OK";
            return m_diSafeZ.p_bIn ? "OK" : "Safe Z Sensor not Detected";
        }

        double GetTrayOffsetZ(int zTray)
        {
            double z0 = m_axisZ.GetPosValue(ePosZ.TrayBottom); 
            double z1 = m_axisZ.GetPosValue(ePosZ.TrayTop);
            return zTray * (z1 - z0) / (m_trays.p_szTray.Y - 1); 
        }
        #endregion

        #region AxisMoveXZ
        string AxisMoveReady(Cleaner.eCleaner eCleaner)
        {
            ePosX ePosX = (eCleaner == Cleaner.eCleaner.Cleaner0) ? ePosX.Cleaner0 : ePosX.Cleaner1; 
            if (Run(AxisMoveX(ePosX))) return p_sInfo;
            return AxisMoveZ(ePosZ.Ready); 
        }

        string AxisMoveCleaner(Cleaner.eCleaner eCleaner, double zOffset)
        {
            if (Run(AxisMoveReady(eCleaner))) return p_sInfo;
            ePosZ ePosZ = (eCleaner == Cleaner.eCleaner.Cleaner0) ? ePosZ.Cleaner0 : ePosZ.Cleaner1;
            return AxisMoveZ(ePosZ, zOffset); 
        }

        string AxisMoveTray(CPoint cpTray)
        {
            double xOffset = GetTrayOffsetX(cpTray.X);
            double zOffset = GetTrayOffsetZ(cpTray.Y);
            if (Run(AxisMoveZ(ePosZ.TrayBottom, zOffset))) return p_sInfo; 
            return AxisMoveX(ePosX.TrayLeft, xOffset); 
        }
        #endregion

        #region RunLoad
        int m_nTry = 1;
        int m_nShake = 0;
        double m_dzShake = 2;
        Cleaner.eCleaner m_eCleanerLoad = Cleaner.eCleaner.Cleaner0; 
        string RunLoad(Cleaner.eCleaner eCleaner)
        {
            if (EQ.p_eState == EQ.eState.Run)
            {
                if (m_trays.p_bFull) return "Run Load Cancel : Tray Full";
                if (m_picker.p_infoStrip != null) return "Run Load Cancel : Picker Already has Strip";
                if (m_aCleaner[eCleaner].p_infoStrip1 == null) return "Run Load Cancel : Cleaner has no Strip";
            }
            try
            {
                for (int nTry = 0; nTry < m_nTry; nTry++)
                {
                    if (Run(AxisMoveReady(eCleaner))) return p_sInfo;
                    if (Run(AxisMoveCleaner(eCleaner, 0))) return p_sInfo;
                    if (Run(m_picker.RunVacuum(true))) return p_sInfo; 
                    for (int nShake = 0; nShake < m_nShake; nShake++)
                    {
                        if (Run(AxisMoveCleaner(eCleaner, -m_dzShake))) return p_sInfo;
                        if (Run(AxisMoveCleaner(eCleaner, -m_dzShake / 10))) return p_sInfo;
                    }
                    if (Run(AxisMoveReady(eCleaner))) return p_sInfo;
                    if (m_picker.m_dioVacuum.p_bIn)
                    {
                        m_picker.p_infoStrip = m_aCleaner[eCleaner].p_infoStrip1;
                        m_aCleaner[eCleaner].p_infoStrip1 = null;
                        m_eCleanerLoad = eCleaner;
                        m_cpTrayUnload = m_trays.GetTrayPosition(m_picker.p_infoStrip);
                        return "OK"; 
                    }
                }
                return "RunLoad Error"; 
            }
            finally { AxisMoveReady(eCleaner); }
        }

        void RunTreeLoad(Tree tree)
        {
            m_nTry = tree.Set(m_nTry, m_nTry, "Try", "Load Try Count");
            m_nShake = tree.Set(m_nShake, m_nShake, "Shake", "Shake Count");
            m_dzShake = tree.Set(m_dzShake, m_dzShake, "dShake", "Shake Width (unit)"); 
        }
        #endregion

        #region RunUnload
        CPoint m_cpTrayUnload = null;
        string RunUnload(CPoint cpTray)
        {
            if (m_trays.p_bFull) return "Run Unload Cancel : Tray Full";
            if (m_picker.p_infoStrip == null) return "Run Unload Cancel : Picker has no Strip";
            ePosX ePosReturn = (m_eCleanerLoad == Cleaner.eCleaner.Cleaner0) ? ePosX.Cleaner1 : ePosX.Cleaner0;
            try
            {
                if (Run(AxisMoveZ(ePosZ.TrayBottom, GetTrayOffsetZ(cpTray.Y)))) return p_sInfo;
                while (m_trays.m_cpNeedPaper != null)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                }
                if (Run(AxisMoveTray(cpTray))) return p_sInfo;
                Unload();
                m_trays.AddSort(cpTray);
                m_picker.p_infoStrip = null;
                if (cpTray == m_cpTrayUnload)
                {
                    m_trays.m_cpNeedPaper = m_cpTrayUnload;
                    m_cpTrayUnload = null;
                }
                if (Run(m_axisX.StartMove(ePosReturn))) return p_sInfo; 
                return m_axisX.WaitReady(); 
            }
            finally { m_axisX.StartMove(ePosReturn); }
        }

        double m_dzUnload = 1;
        string Unload()
        {
            double z = m_axisZ.p_posCommand;
            try
            {
                if (Run(m_axisZ.StartMove(z + m_dzUnload))) return p_sInfo;
                if (Run(m_axisZ.WaitReady())) return p_sInfo;
                if (Run(m_picker.RunVacuum(false))) return p_sInfo;
                if (Run(m_axisZ.StartMove(z))) return p_sInfo;
                if (Run(m_axisZ.WaitReady())) return p_sInfo;
                return m_diSafeZ.p_bIn ? "OK" : "Safe Z Sensor not Detected";
            }
            finally { m_axisZ.StartMove(z); }
        }

        void RunTreeUnload(Tree tree)
        {
            m_dzUnload = tree.Set(m_dzUnload, m_dzUnload, "dZ Unload", "Unload Down dZ (unit)");
        }
        #endregion

        #region Check Thread
        bool m_bThreadCheck = false;
        Thread m_threadCheck;
        void InitThreadCheck()
        {
            m_threadCheck = new Thread(new ThreadStart(RunThreadCheck));
            m_threadCheck.Start();
        }

        void RunThreadCheck()
        {
            m_bThreadCheck = true;
            Thread.Sleep(2000);
            while (m_bThreadCheck)
            {
                Thread.Sleep(10);
                p_bEMG = m_diEmg.p_bIn;
            }
        }

        void EMGStop(string sMsg)
        {
            m_axisX.StopAxis(false);
            m_axisX.ServoOn(false);
            m_axisX.p_eState = Axis.eState.Init;
            EQ.p_bStop = true;
            p_sInfo = sMsg;
        }
        #endregion

        #region Override
        public override string StateReady()
        {
            if (EQ.p_eState != EQ.eState.Run) return "OK";
            if (m_picker.p_infoStrip != null)
            {
                if (m_trays.m_cpNeedPaper != null) return "OK";
                return StartRunUnload(m_cpTrayUnload); 
            }
            else
            {
                if (m_aCleaner[m_eCleanerLoad].p_infoStrip1 != null) return StartRunLoad(m_eCleanerLoad);
                m_eCleanerLoad = 1 - m_eCleanerLoad;
                return "OK";
            }
        }

        string StartRunLoad(Cleaner.eCleaner eCleaner)
        {
            Run_Load run = (Run_Load)m_runLoad.Clone();
            run.m_eCleaner = eCleaner;
            return StartRun(run); 
        }

        string StartRunUnload(CPoint cpTray)
        {
            Run_Unload run = (Run_Unload)m_runUnload.Clone();
            run.m_cpTray = cpTray;
            return StartRun(run); 
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            RunTreeLoad(tree.GetTree("Load", false));
            RunTreeUnload(tree.GetTree("Unload", false));
            m_picker.RunTree(tree.GetTree("Picker", false));
        }

        public override void Reset()
        {
            AxisMoveReady(Cleaner.eCleaner.Cleaner1);
            base.Reset();
        }
        #endregion

        Dictionary<Cleaner.eCleaner, Cleaner> m_aCleaner;
        Trays m_trays; 
        public Sorter0(string id, IEngineer engineer, Dictionary<Cleaner.eCleaner, Cleaner> aCleaner, Trays trays)
        {
            m_aCleaner = aCleaner;
            m_trays = trays;
            InitPicker();
            base.InitBase(id, engineer);
            InitThreadCheck();
        }

        public override void ThreadStop()
        {
            m_picker.ThreadStop();
            if (m_bThreadCheck)
            {
                m_bThreadCheck = false;
                m_threadCheck.Join();
            }
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runLoad;
        ModuleRunBase m_runUnload;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            AddModuleRunList(new Run_MoveReady(this), false, "Move Ready");
            AddModuleRunList(new Run_MoveTray(this), false, "Move Tray");
            m_runLoad = AddModuleRunList(new Run_Load(this), false, "Run Load");
            m_runUnload = AddModuleRunList(new Run_Unload(this), false, "Run Unload");
        }

        public class Run_Delay : ModuleRunBase
        {
            Sorter0 m_module;
            public Run_Delay(Sorter0 module)
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

        public class Run_MoveReady : ModuleRunBase
        {
            Sorter0 m_module;
            public Run_MoveReady(Sorter0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public Cleaner.eCleaner m_eCleaner = Cleaner.eCleaner.Cleaner1; 
            public override ModuleRunBase Clone()
            {
                Run_MoveReady run = new Run_MoveReady(m_module);
                run.m_eCleaner = m_eCleaner;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eCleaner = (Cleaner.eCleaner)tree.Set(m_eCleaner, m_eCleaner, "Position", "Sorter0 Move Position", bVisible);
            }

            public override string Run()
            {
                return m_module.AxisMoveReady(m_eCleaner);
            }
        }

        public class Run_MoveTray : ModuleRunBase
        {
            Sorter0 m_module;
            public Run_MoveTray(Sorter0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public CPoint m_cpTray = new CPoint(); 
            public override ModuleRunBase Clone()
            {
                Run_MoveTray run = new Run_MoveTray(m_module);
                run.m_cpTray = new CPoint(m_cpTray);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_cpTray = tree.Set(m_cpTray, m_cpTray, "Tray", "Sorter0 Move Position", bVisible);
            }

            public override string Run()
            {
                return m_module.AxisMoveTray(m_cpTray);
            }
        }

        public class Run_Load : ModuleRunBase
        {
            Sorter0 m_module;
            public Run_Load(Sorter0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public Cleaner.eCleaner m_eCleaner = Cleaner.eCleaner.Cleaner1;
            public override ModuleRunBase Clone()
            {
                Run_Load run = new Run_Load(m_module);
                run.m_eCleaner = m_eCleaner;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eCleaner = (Cleaner.eCleaner)tree.Set(m_eCleaner, m_eCleaner, "Position", "Sorter0 Move Position", bVisible);
            }

            public override string Run()
            {
                return m_module.RunLoad(m_eCleaner);
            }
        }

        public class Run_Unload : ModuleRunBase
        {
            Sorter0 m_module;
            public Run_Unload(Sorter0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public CPoint m_cpTray = new CPoint();
            public override ModuleRunBase Clone()
            {
                Run_Unload run = new Run_Unload(m_module);
                run.m_cpTray = new CPoint(m_cpTray);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_cpTray = tree.Set(m_cpTray, m_cpTray, "Tray", "Sorter0 Move Position", bVisible);
            }

            public override string Run()
            {
                return m_module.RunUnload(m_cpTray);
            }
        }
        #endregion
    }
}
