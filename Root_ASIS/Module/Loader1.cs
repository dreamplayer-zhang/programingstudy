using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_ASIS.Module
{
    public class Loader1 : ModuleBase
    {
        #region ToolBox
        Axis m_axis;
        DIO_I m_diEmg;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axis, this, "Axis");
            p_sInfo = m_toolBox.Get(ref m_diEmg, this, "Emegency");
            m_picker.GetTools(this, bInit);
            if (bInit) InitTools();
        }

        void InitTools()
        {
            InitPosition();
        }
        #endregion

        #region Picker
        Picker m_picker; 
        void InitPicker()
        {
            m_picker = new Picker(p_id + ".Picker", this); 
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

        #region Axis
        public enum ePos
        {
            Boat0,
            Turnover
        }
        void InitPosition()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
        }

        string AxisMove(ePos ePos)
        {
            if (m_picker.IsDown()) return "AxisMove Error : Picker Down";
            if (Run(m_axis.StartMove(ePos))) return p_sInfo;
            return m_axis.WaitReady();
        }
        #endregion

        #region RunLoad
        public string RunLoad()
        {
            if (m_boat0.p_bDone == false) return "Boat0 not Done";
            if (m_picker.p_infoStrip != null) return "Picker already Load"; 
            if (Run(AxisMove(ePos.Boat0))) return p_sInfo;
            if (Run(m_picker.RunLoad(null))) return p_sInfo;
            m_picker.p_infoStrip = m_boat0.p_infoStrip;
            m_boat0.p_infoStrip = null; 
            return "OK";
        }
        #endregion

        #region RunUnload
        public string RunUnload()
        {
            try
            {
                if (m_turnover.p_infoStrip0 != null) return "Turnover is not Ready";
                if (m_picker.p_infoStrip == null) return "Picker has no Strip";
                if (Run(AxisMove(ePos.Turnover))) return p_sInfo;
                if (Run(m_picker.RunUnload())) return p_sInfo;
                if (Run(AxisMove(ePos.Boat0))) return p_sInfo;
                m_turnover.p_infoStrip0 = m_picker.p_infoStrip;
                m_picker.p_infoStrip = null;
                return "OK";
            }
            finally { AxisMove(ePos.Boat0); }
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
                switch (m_axis.p_eState)
                {
                    case Axis.eState.Home:
                    case Axis.eState.Jog:
                    case Axis.eState.Move:
                        if (m_picker.IsDown()) EMGStop("Picker Down when Axis Move"); 
                        break;
                }
            }
        }

        void EMGStop(string sMsg)
        {
            m_axis.StopAxis(false);
            m_axis.ServoOn(false);
            m_axis.p_eState = Axis.eState.Init;
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
                if (m_turnover.p_infoStrip0 == null) StartRun(m_runUnload); 
            }
            else
            {
                if (m_boat0.p_bDone) StartRun(m_runLoad); 
            }
            return "OK";
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            m_picker.RunTree(tree.GetTree("Picker", false)); 
        }

        public override void Reset()
        {
            if (m_picker.p_infoStrip != null)
            {
                if (Run(AxisMove(ePos.Turnover))) return;
                m_picker.RunUnload();
            }
            m_picker.Reset();
            AxisMove(ePos.Boat0);
            base.Reset();
        }
        #endregion

        Boat m_boat0;
        Turnover m_turnover;
        public Loader1(string id, IEngineer engineer, Boat boat0, Turnover turnover)
        {
            m_turnover = turnover;
            m_boat0 = boat0;
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
            AddModuleRunList(new Run_Move(this), false, "Move Loader1");
            AddModuleRunList(new Run_Picker(this), false, "Run Picker");
            m_runLoad = AddModuleRunList(new Run_Load(this), false, "Run Load");
            m_runUnload = AddModuleRunList(new Run_Unload(this), false, "Run Unload");
        }

        public class Run_Delay : ModuleRunBase
        {
            Loader1 m_module;
            public Run_Delay(Loader1 module)
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
            Loader1 m_module;
            public Run_Move(Loader1 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public ePos m_ePos = ePos.Boat0;
            public override ModuleRunBase Clone()
            {
                Run_Move run = new Run_Move(m_module);
                run.m_ePos = m_ePos;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_ePos = (ePos)tree.Set(m_ePos, m_ePos, "Position", "Loader1 Move Position", bVisible);
            }

            public override string Run()
            {
                return m_module.AxisMove(m_ePos);
            }
        }

        public class Run_Picker : ModuleRunBase
        {
            Loader1 m_module;
            public Run_Picker(Loader1 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bLoad = true;
            public override ModuleRunBase Clone()
            {
                Run_Picker run = new Run_Picker(m_module);
                run.m_bLoad = m_bLoad;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bLoad = tree.Set(m_bLoad, m_bLoad, "Load", "Load = true, Unload = false", bVisible);
            }

            public override string Run()
            {
                if (m_bLoad) return m_module.m_picker.RunLoad(null);
                else return m_module.m_picker.RunUnload();
            }
        }

        public class Run_Load : ModuleRunBase
        {
            Loader1 m_module;
            public Run_Load(Loader1 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Load run = new Run_Load(m_module);
                return run;
            }

            string m_sLoad = "Boat0";
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                tree.Set(m_sLoad, m_sLoad, "Position", "Load from", bVisible, true);
            }

            public override string Run()
            {
                return m_module.RunLoad();
            }
        }

        public class Run_Unload : ModuleRunBase
        {
            Loader1 m_module;
            public Run_Unload(Loader1 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Unload run = new Run_Unload(m_module);
                return run;
            }

            string m_sUnoad = "Turnover";
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                tree.Set(m_sUnoad, m_sUnoad, "Position", "Unload to", bVisible, true);
            }

            public override string Run()
            {
                return m_module.RunUnload();
            }
        }
        #endregion
    }
}
