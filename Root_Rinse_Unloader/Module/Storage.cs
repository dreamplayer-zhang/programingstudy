using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_Rinse_Unloader.Module
{
    public class Storage : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            foreach (Magazine magazine in m_aMagazine) magazine.GetTools(m_toolBox, bInit);
            m_stack.GetTools(m_toolBox);
            p_sInfo = m_toolBox.GetAxis(ref m_axis, this, "Elevator");
            if (bInit)
            {
                InitPosElevator();
            }
        }
        #endregion

        #region Magazine
        public enum eMagazine
        {
            Magazine1,
            Magazine2,
            Magazine3,
            Magazine4,
        }
        public string[] m_asMagazine = Enum.GetNames(typeof(eMagazine));
        public class Magazine : NotifyProperty
        {
            DIO_I m_diCheck;
            public DIO_IO m_dioClamp;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_storage.p_sInfo = toolBox.GetDIO(ref m_diCheck, m_storage, m_id + ".Check");
                m_storage.p_sInfo = toolBox.GetDIO(ref m_dioClamp, m_storage, m_id + ".Clamp");
                if (bInit) m_dioClamp.Write(!m_diCheck.p_bIn); 
            }

            bool _bCheck = false;
            public bool p_bCheck
            {
                get { return _bCheck; }
                set
                {
                    if (_bCheck == value) return;
                    _bCheck = value;
                    OnPropertyChanged();
                }
            }

            bool _bClamp = false;
            public bool p_bClamp
            {
                get { return _bClamp; }
                set
                {
                    if (_bClamp == value) return;
                    _bClamp = value;
                    OnPropertyChanged();
                }
            }

            public void CheckSensor()
            {
                p_bCheck = m_diCheck.p_bIn;
                p_bClamp = !m_dioClamp.p_bIn;
            }

            public void RunClamp(bool bClamp)
            {
                m_dioClamp.Write(!bClamp);
            }

            public eMagazine m_eMagazine = eMagazine.Magazine1;
            public string m_id;
            public Storage m_storage;
            public Magazine(eMagazine eMagazine, Storage storage)
            {
                m_eMagazine = eMagazine;
                m_id = eMagazine.ToString();
                m_storage = storage;
            }
        }

        public List<Magazine> m_aMagazine = new List<Magazine>();
        void InitMagazine()
        {
            for (int n = 0; n < 4; n++) m_aMagazine.Add(new Magazine((eMagazine)n, this));
        }

        public string RunClamp(bool bClamp, double secDelay)
        {
            foreach (Magazine magazine in m_aMagazine) magazine.RunClamp(bClamp);
            Thread.Sleep((int)secDelay);
            foreach (Magazine magazine in m_aMagazine)
            {
                if (magazine.p_bCheck && (magazine.p_bClamp != bClamp)) return "Clamp Sensor Error";
            }
            return "OK";
        }
        #endregion

        #region Stack
        public class Stack : NotifyProperty
        {
            DIO_I m_diLevel;
            public void GetTools(ToolBox toolBox)
            {
                m_storage.p_sInfo = toolBox.GetDIO(ref m_diLevel, m_storage, m_id + ".Level");
            }

            bool _bLevel = false;
            public bool p_bLevel
            {
                get { return _bLevel; }
                set
                {
                    if (_bLevel == value) return;
                    _bLevel = value;
                    OnPropertyChanged();
                }
            }

            public void CheckSensor()
            {
                p_bLevel = m_diLevel.p_bIn; 
            }

            string m_id;
            Storage m_storage;
            public Stack(string id, Storage storage)
            {
                m_id = id;
                m_storage = storage;
            }
        }
        Stack m_stack;
        void InitStack()
        {
            m_stack = new Stack("Stack", this);
        }
        #endregion

        #region Elevator
        Axis m_axis;
        void InitPosElevator()
        {
            m_axis.AddPos(Enum.GetNames(typeof(eMagazine)));
            m_axis.AddPos("Stack");
        }

        int m_dZ = 6000;
        public string MoveMagazine(eMagazine eMagazine, int iIndex, bool bWait)
        {
            if ((iIndex < 0) || (iIndex >= 20)) return "Invalid Index";
            m_axis.StartMove(eMagazine, -iIndex * m_dZ);
            if (bWait) return m_axis.WaitReady();
            return "OK"; 
        }

        public string MoveStack()
        {
            m_axis.StartMove("Stack");
            return m_axis.WaitReady();
        }

        double m_posStackReady = -100000;
        double m_fJogScale = 1;
        public string MoveStackReady()
        {
            MoveStack();
            return "OK";
        }

        public bool p_bIsEnablePick
        {
            get { return Math.Abs(m_posStackReady - m_axis.p_posCommand) < 10; }
        }

        void RunTreeElevator(Tree tree)
        {
            m_dZ = tree.Set(m_dZ, m_dZ, "dZ", "Magazine Slot Pitch (pulse)");
            m_fJogScale = tree.Set(m_fJogScale, m_fJogScale, "Jog Scale", "Jog Move Scale (0 ~ 1)");
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
                m_stack.CheckSensor(); 
                foreach (Magazine magazine in m_aMagazine) magazine.CheckSensor();
            }
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            foreach (Magazine magazine in m_aMagazine) magazine.RunClamp(magazine.p_bCheck);
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeElevator(tree.GetTree("Elevator"));
        }
        #endregion

        RinseU m_rinse;
        public Storage(string id, IEngineer engineer, RinseU rinse)
        {
            p_id = id;
            m_rinse = rinse;
            InitMagazine();
            InitStack();
            InitBase(id, engineer);
            InitThreadCheck();
        }

        public override void ThreadStop()
        {
            foreach (Magazine magazine in m_aMagazine) magazine.RunClamp(false);
            if (m_bThreadCheck)
            {
                m_bThreadCheck = false;
                m_threadCheck.Join();
            }
            base.ThreadStop();
        }

        #region StartRun
        public void StartRun()
        {
            switch (m_rinse.p_eMode)
            {
                case RinseU.eRunMode.Stack:
                    StartMoveStackReady();
                    break;
                case RinseU.eRunMode.Magazine:
                    StartMoveMagazine(false);
                    break; 
            }
        }
        #endregion

        #region ModuleRun
        public string StartMoveStackReady()
        {
            StartRun(m_runReady.Clone());
            return "OK";
        }

        public string StartMoveMagazine(bool bNext)
        {
            Run_RunNextMagazine run = (Run_RunNextMagazine)m_runNextMagazine.Clone();
            run.m_bNext = bNext; 
            return StartRun(run); 
        }

        public string MoveMagazine(bool bNext)
        {
            if (bNext)
            {
                m_rinse.p_iMagazine++;
                if (m_rinse.p_iMagazine >= 20)
                {
                    m_rinse.p_iMagazine = 0;
                    if (Run(SetNextMagazine(m_rinse.p_eMagazine))) return p_sInfo;
                }
            }
            return MoveMagazine(m_rinse.p_eMagazine, m_rinse.p_iMagazine, true); 
        }

        string SetNextMagazine(eMagazine eMagazine)
        {
            switch (eMagazine)
            {
                case eMagazine.Magazine1: return "Magazine Full";
                case eMagazine.Magazine2: m_rinse.p_eMagazine = eMagazine.Magazine1; break;
                case eMagazine.Magazine3: m_rinse.p_eMagazine = eMagazine.Magazine2; break;
                case eMagazine.Magazine4: m_rinse.p_eMagazine = eMagazine.Magazine3; break;
            }
            bool bCheck = m_aMagazine[(int)m_rinse.p_eMagazine].p_bCheck;
            bool bClamp = m_aMagazine[(int)m_rinse.p_eMagazine].p_bClamp; 
            if (bCheck && bClamp) return "OK";
            return SetNextMagazine(m_rinse.p_eMagazine); 
        }

        ModuleRunBase m_runReady;
        ModuleRunBase m_runNextMagazine;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_MoveStack(this), false, "Move Elevator Stack Position");
            AddModuleRunList(new Run_MoveMagazine(this), false, "Move Elevator Magazine Position");
            AddModuleRunList(new Run_Clamp(this), false, "Run Clamp");
            m_runReady = AddModuleRunList(new Run_StackReady(this), false, "Run Stack Move Ready Position");
            m_runNextMagazine = AddModuleRunList(new Run_RunNextMagazine(this), false, "Run Magazine");
        }

        public class Run_MoveStack : ModuleRunBase
        {
            Storage m_module;
            public Run_MoveStack(Storage module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_MoveStack run = new Run_MoveStack(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.MoveStack();
            }
        }

        public class Run_MoveMagazine : ModuleRunBase
        {
            Storage m_module;
            public Run_MoveMagazine(Storage module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            eMagazine m_eMagazine = eMagazine.Magazine1;
            int m_iIndex = 0;
            public override ModuleRunBase Clone()
            {
                Run_MoveMagazine run = new Run_MoveMagazine(m_module);
                run.m_eMagazine = m_eMagazine;
                run.m_iIndex = m_iIndex;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eMagazine = (eMagazine)tree.Set(m_eMagazine, m_eMagazine, "Pos", "Elevator Position", bVisible);
                m_iIndex = tree.Set(m_iIndex, m_iIndex, "Index", "Magazine Index", bVisible);
            }

            public override string Run()
            {
                return m_module.MoveMagazine(m_eMagazine, m_iIndex, true);
            }
        }

        public class Run_Clamp : ModuleRunBase
        {
            Storage m_module;
            public Run_Clamp(Storage module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bClamp = false;
            double m_secDelay = 4;
            public override ModuleRunBase Clone()
            {
                Run_Clamp run = new Run_Clamp(m_module);
                run.m_bClamp = m_bClamp;
                run.m_secDelay = m_secDelay;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bClamp = tree.Set(m_bClamp, m_bClamp, "Clamp", "Run Clamp", bVisible);
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Run Clamp Delay (sec)", bVisible);
            }

            public override string Run()
            {
                return m_module.RunClamp(m_bClamp, m_secDelay);
            }
        }

        public class Run_StackReady : ModuleRunBase
        {
            Storage m_module;
            public Run_StackReady(Storage module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_StackReady run = new Run_StackReady(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.MoveStackReady();
            }
        }

        public class Run_RunNextMagazine : ModuleRunBase
        {
            Storage m_module;
            public Run_RunNextMagazine(Storage module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public bool m_bNext = true; 
            public override ModuleRunBase Clone()
            {
                Run_RunNextMagazine run = new Run_RunNextMagazine(m_module);
                run.m_bNext = m_bNext; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bNext = tree.Set(m_bNext, m_bNext, "Next", "Move Next Magazine", bVisible); 
            }

            public override string Run()
            {
                return m_module.MoveMagazine(m_bNext);
            }
        }
        #endregion
    }
}
