using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_ASIS.Module
{
    public class Loader0 : ModuleBase
    {
        #region ToolBox
        Axis m_axis;
        DIO_I m_diPaperFull; 
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axis, this, "Axis");
            p_sInfo = m_toolBox.Get(ref m_diPaperFull, this, "Paper Full"); 
            m_aPicker[ePicker.Strip].GetTools(this, bInit);
            m_aPicker[ePicker.Paper].GetTools(this, bInit);
            if (bInit) InitTools();
        }

        void InitTools()
        {
            InitPosition();
        }
        #endregion

        #region Picker
        public enum ePicker
        {
            Strip,
            Paper
        }
        Dictionary<ePicker, Picker> m_aPicker = new Dictionary<ePicker, Picker>();

        void InitPicker()
        {
            m_aPicker.Add(ePicker.Strip, new Picker(p_id + ".StripPicker", this));
            m_aPicker.Add(ePicker.Paper, new Picker(p_id + ".PaperPicker", this));
        }
        #endregion

        #region Axis
        public enum ePos
        {
            LoadEV,
            MGZ, 
            Boat
        }
        void InitPosition()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
        }

        double m_fPosScale = 1; 
        double m_dPosPaper = 0;
        string AxisMove(ePos ePos, ePicker ePicker)
        {
            if (m_aPicker[ePicker.Strip].IsDown()) return "AxisMove Error : Strip Picker Down";
            if (m_aPicker[ePicker.Paper].IsDown()) return "AxisMove Error : Paper Picker Down";
            double fOffset = ePicker == ePicker.Paper ? m_dPosPaper : 0; 
            switch (ePos)
            {
                case ePos.LoadEV: fOffset += Strip.m_szStripTeach.X - Strip.p_szStrip.X; break;
                case ePos.MGZ: fOffset -= Strip.m_szStripTeach.X - Strip.p_szStrip.X; break;
            }
            if (Run(m_axis.StartMove(ePos, fOffset * m_fPosScale))) return p_sInfo;
            return m_axis.WaitReady(); 
        }

        void RunTreeAxis(Tree tree)
        {
            m_fPosScale = tree.Set(m_fPosScale, m_fPosScale, "Scale", "Axis Move Scale");
            m_dPosPaper = tree.Set(m_dPosPaper, m_dPosPaper, "Paper Picker", "Distance between Pickers (unit)"); 
        }
        #endregion

        #region RunLoad
        public string RunLoad(ePos ePos)
        {
            switch (ePos)
            {
                case ePos.LoadEV: return RunLoadEV();
                case ePos.MGZ: return RunLoadMGZ(); 
            }
            return "OK"; 
        }

        string RunLoadEV()
        {
            if (m_loadEV.p_bDone == false) return "LoadEV not Done";
            if (Run(AxisMove(ePos.LoadEV, m_loadEV.p_bPaper ? ePicker.Paper : ePicker.Strip))) return p_sInfo;
            if (m_loadEV.p_bPaper == false)
            {
                if (m_aPicker[ePicker.Paper].p_infoStrip != null) m_aPicker[ePicker.Paper].StartUnload();
                if (Run(m_aPicker[ePicker.Strip].RunLoadEV(m_loadEV))) return p_sInfo;
                m_aPicker[ePicker.Strip].p_infoStrip = m_loadEV.GetNewInfoStrip();
                if (Run(m_aPicker[ePicker.Paper].WaitReady())) return p_sInfo;
                m_aPicker[ePicker.Paper].p_infoStrip = null; 
            }
            else
            {
                if (Run(m_aPicker[ePicker.Paper].RunLoadEV(m_loadEV))) return p_sInfo;
                m_aPicker[ePicker.Paper].p_infoStrip = new InfoStrip(-1); 
            }
            return "OK";
        }

        string RunLoadMGZ()
        {
            return "Not Yet"; 
        }
        #endregion

        #region RunUnload
        public string RunUnload(ePicker ePicker)
        {
            switch (ePicker)
            {
                case ePicker.Strip: 
                    if (Run(AxisMove(ePos.Boat, 0))) return p_sInfo;
                    if (Run(m_aPicker[ePicker].RunUnload())) return p_sInfo;
                    m_boat.p_infoStrip = m_aPicker[ePicker.Strip].p_infoStrip;
                    m_aPicker[ePicker.Strip].p_infoStrip = null; 
                    break;
                case ePicker.Paper: 
                    if (Run(AxisMove(ePos.LoadEV, ePicker.Strip))) return p_sInfo;
                    if (Run(m_aPicker[ePicker].RunUnload())) return p_sInfo;
                    break; 
            }
            return "OK";
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
                switch (m_axis.p_eState)
                {
                    case Axis.eState.Home:
                    case Axis.eState.Jog:
                    case Axis.eState.Move:
                        if (m_aPicker[ePicker.Paper].IsDown() || m_aPicker[ePicker.Strip].IsDown())
                        {
                            m_axis.StopAxis(false);
                            m_axis.ServoOn(false);
                            m_axis.p_eState = Axis.eState.Init;
                            EQ.p_bStop = true;
                            p_sInfo = "Picker Down when Axis Move"; 
                        }
                        break;
                }
            }
        }
        #endregion

        #region Override
        public override string StateReady()
        {
            if (EQ.p_eState != EQ.eState.Run) return "OK"; 
            if (m_aPicker[ePicker.Strip].p_infoStrip != null)
            {
                if (m_boat.p_bReady) return StartRunUnload(ePicker.Strip);
                AxisMove(ePos.Boat, ePicker.Strip);
            }
            else
            {
                if (Strip.p_bUseMGZ)
                {
                    AxisMove(ePos.MGZ, ePicker.Strip); 
                    //forget
                }
                else
                {
                    if (m_loadEV.p_bDone) return StartRunLoad(ePos.LoadEV);
                }
            }
            if (m_aPicker[ePicker.Paper].p_infoStrip != null) return StartRunUnload(ePicker.Paper); 
            return "OK";
        }

        string StartRunLoad(ePos ePos)
        {
            Run_Load run = (Run_Load)m_runLoad.Clone();
            run.m_ePos = ePos;
            StartRun(run);
            return "OK";
        }

        string StartRunUnload(ePicker ePicker)
        {
            Run_Unload run = (Run_Unload)m_runUnload.Clone();
            run.m_ePicker = ePicker;
            StartRun(run);
            return "OK"; 
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            RunTreeAxis(tree.GetTree("Axis", false));
            m_aPicker[ePicker.Strip].RunTree(tree.GetTree("Strip Picker", false));
            m_aPicker[ePicker.Paper].RunTree(tree.GetTree("Paper Picker", false));
        }

        public override void Reset()
        {
            if (m_aPicker[ePicker.Paper].p_infoStrip != null) RunUnload(ePicker.Paper); 
            if (m_aPicker[ePicker.Strip].p_infoStrip != null)
            {
                if (Run(AxisMove(ePos.LoadEV, ePicker.Strip))) return;
                m_aPicker[ePicker.Strip].RunUnload(); 
            }
            m_aPicker[ePicker.Strip].Reset();
            m_aPicker[ePicker.Paper].Reset();
            AxisMove(ePos.LoadEV, 0); 
            base.Reset();
        }
        #endregion

        LoadEV m_loadEV;
        Boat m_boat; 
        public Loader0(string id, IEngineer engineer, LoadEV loadEV, Boat boat)
        {
            m_loadEV = loadEV;
            m_boat = boat; 
            InitPicker();
            base.InitBase(id, engineer);
            InitThreadCheck();
        }

        public override void ThreadStop()
        {
            m_aPicker[ePicker.Strip].ThreadStop();
            m_aPicker[ePicker.Paper].ThreadStop(); 
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
            AddModuleRunList(new Run_Move(this), false, "Move Loader0");
            m_runLoad = AddModuleRunList(new Run_Load(this), false, "Run Load");
            m_runUnload = AddModuleRunList(new Run_Unload(this), false, "Run Unload");

        }

        public class Run_Delay : ModuleRunBase
        {
            Loader0 m_module;
            public Run_Delay(Loader0 module)
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

        public class Run_Move : ModuleRunBase
        {
            Loader0 m_module;
            public Run_Move(Loader0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public ePos m_ePos = ePos.LoadEV;
            public ePicker m_ePicker = ePicker.Strip; 
            public override ModuleRunBase Clone()
            {
                Run_Move run = new Run_Move(m_module);
                run.m_ePos = m_ePos;
                run.m_ePicker = m_ePicker; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_ePos = (ePos)tree.Set(m_ePos, m_ePos, "Position", "Loader0 Move Position", bVisible);
                m_ePicker = (ePicker)tree.Set(m_ePicker, m_ePicker, "Picker", "Loader0 Picker", bVisible);
            }

            public override string Run()
            {
                return m_module.AxisMove(m_ePos, m_ePicker); 
            }
        }

        public class Run_Load : ModuleRunBase
        {
            Loader0 m_module;
            public Run_Load(Loader0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public ePos m_ePos = ePos.LoadEV;
            public override ModuleRunBase Clone()
            {
                Run_Load run = new Run_Load(m_module);
                run.m_ePos = m_ePos;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_ePos = (ePos)tree.Set(m_ePos, m_ePos, "Position", "Loader0 Move Position", bVisible);
            }

            public override string Run()
            {
                return m_module.RunLoad(m_ePos);
            }
        }

        public class Run_Unload : ModuleRunBase
        {
            Loader0 m_module;
            public Run_Unload(Loader0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public ePicker m_ePicker = ePicker.Strip; 
            public override ModuleRunBase Clone()
            {
                Run_Unload run = new Run_Unload(m_module);
                run.m_ePicker = m_ePicker;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_ePicker = (ePicker)tree.Set(m_ePicker, m_ePicker, "Picker", "Loader0 Picker", bVisible);
            }

            public override string Run()
            {
                return m_module.RunUnload(m_ePicker);
            }
        }
        #endregion

    }
}
