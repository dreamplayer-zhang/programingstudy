using Root_Rinse_Unloader.Engineer;
using RootTools;
using RootTools.Control;
using RootTools.GAFs;
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
                InitALID(); 
                InitPosElevator();
            }
        }
        #endregion

        #region GAF
        ALID m_alidMagazineFull;
        ALID m_alidProtrusion;
        void InitALID()
        {
            m_alidMagazineFull = m_gaf.GetALID(this, "MagazineFull", "MagazineFull Error");
            m_alidProtrusion = m_gaf.GetALID(this, "Protrusion", "Protrusion");
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
            DIO_I m_diProtrusion;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_storage.p_sInfo = toolBox.GetDIO(ref m_diCheck, m_storage, m_id + ".Check");
                m_storage.p_sInfo = toolBox.GetDIO(ref m_dioClamp, m_storage, m_id + ".Clamp");
                m_storage.p_sInfo = toolBox.GetDIO(ref m_diProtrusion, m_storage, m_id + ".Protrusion");
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

            public bool IsProtrusion()
            {
                return m_diProtrusion.p_bIn;
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

        public bool IsProtrusion()
        {
            foreach (Magazine magazine in m_aMagazine)
            {
                if (magazine.IsProtrusion())
                {
                    m_alidProtrusion.Run(true, "Check Storage : Strip Protrusion");
                    m_handler.m_rail.RunRotate(false);
                    m_handler.m_roller.RunRotate(false);
                    return true;
                }
            }
            if (m_handler.m_rail.IsArriveOn())
            {
                m_alidProtrusion.Run(true, "Check Rail Sensor");
                return true;
            }
            return false;
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
        Loader p_loader
        {
            get
            {
                RinseU_Handler handler = (RinseU_Handler)m_engineer.ClassHandler();
                return (handler == null) ? null : handler.m_loader;
            }
        }
        bool IsLoaderDanger()
        {
            if (p_loader == null) return true;
            return p_loader.IsLoaderDanger();
        }

        public Axis m_axis;
        void InitPosElevator()
        {
            m_axis.AddPos(Enum.GetNames(typeof(eMagazine)));
            m_axis.AddPos("Stack");
        }

        string MoveElevator(double fPos, bool bWait = true)
        {
            if (fPos == m_axis.p_posCommand) return "OK";
            if (IsProtrusion()) return "Protrusion Error";
            m_axis.StartMove(fPos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        int m_dZ = 6000;
        public string MoveMagazine(eMagazine eMagazine, int iIndex, bool bWait)
        {
            if ((iIndex < 0) || (iIndex >= 20)) return "Invalid Index";
            if (IsLoaderDanger()) return "Check Loader Position";
            double fPos = m_axis.GetPosValue(eMagazine) - iIndex * m_dZ;
            return MoveElevator(fPos, bWait);
        }

        public string MoveStack()
        {
            return MoveElevator(m_axis.GetPosValue("Stack"));
        }

        public bool IsHighPos()
        {
            return m_axis.p_posCommand > (m_axis.GetPosValue("Stack") + 1000);
        }

        double m_fJogScale = 1;
        public string MoveStackReady()
        {
            return MoveStack(); 
            /*
            if (m_stack.p_bLevel)
            {
                m_axis.Jog(-m_fJogScale);
                while (m_stack.p_bLevel && (EQ.IsStop() == false)) Thread.Sleep(10);
                m_axis.StopAxis();
                Thread.Sleep(500);
            }
            m_axis.Jog(m_fJogScale);
            while (!m_stack.p_bLevel && (EQ.IsStop() == false)) Thread.Sleep(10);
            m_axis.StopAxis();
            m_axis.WaitReady();
            Thread.Sleep(500);
            return "OK";
            */
        }

        public void RunLoadUp()
        {
            MoveMagazine(eMagazine.Magazine3, 10, true);
            m_aMagazine[0].RunClamp(false);
            m_aMagazine[1].RunClamp(false);
        }

        public void RunLoadDown()
        {
            MoveMagazine(eMagazine.Magazine1, 0, true);
            m_aMagazine[2].RunClamp(false);
            m_aMagazine[3].RunClamp(false);
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
            if (IsProtrusion())
            {
                p_eState = eState.Error; 
                return "Protrusion Error";
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
        RinseU_Handler m_handler;
        public Storage(string id, IEngineer engineer, RinseU rinse)
        {
            p_id = id;
            m_rinse = rinse;
            m_handler = (RinseU_Handler)engineer.ClassHandler(); 
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
                case RinseU.eRunMode.Magazine:
                    StartMoveMagazine(false);
                    break;
                case RinseU.eRunMode.Stack:
                    MoveStack(); 
                    StartMoveStackReady();
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
            if (bNext) m_rinse.p_iMagazine++;
            if (Run(SetEnableMagazine())) return p_sInfo; 
            return MoveMagazine(m_rinse.p_eMagazine, m_rinse.p_iMagazine, true);
        }

        string SetEnableMagazine()
        {
            while (IsMagazineExist(m_rinse.p_eMagazine, m_rinse.p_iMagazine) == false)
            {
                if (Run(SetNextMagazine(m_rinse.p_eMagazine))) return p_sInfo;
            }
            return "OK"; 
        }

        bool IsMagazineExist(eMagazine eMagazine, int iMagazine)
        {
            if (iMagazine >= 20) return false;
            bool bCheck = m_aMagazine[(int)eMagazine].p_bCheck;
            bool bClamp = m_aMagazine[(int)eMagazine].p_bClamp;
            return (bCheck && bClamp);
        }

        string SetNextMagazine(eMagazine eMagazine)
        {
            m_rinse.p_iMagazine = 0;
            switch (eMagazine)
            {
                case eMagazine.Magazine1:
                    m_alidMagazineFull.p_bSet = true;
                    m_rinse.RunBuzzer(RinseU.eBuzzer.Finish);
                    EQ.p_eState = EQ.eState.Ready;
                    return "Magazine Full";
                case eMagazine.Magazine2: m_rinse.p_eMagazine = eMagazine.Magazine1; break;
                case eMagazine.Magazine3: m_rinse.p_eMagazine = eMagazine.Magazine2; break;
                case eMagazine.Magazine4: m_rinse.p_eMagazine = eMagazine.Magazine3; break;
            }
            return "OK";
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
