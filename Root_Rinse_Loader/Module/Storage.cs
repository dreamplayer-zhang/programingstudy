using Root_Rinse_Loader.Engineer;
using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_Rinse_Loader.Module
{
    public class Storage : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            foreach (Magazine magazine in m_aMagazine) magazine.GetTools(m_toolBox);
            m_stack.GetTools(m_toolBox);
            p_sInfo = m_toolBox.GetDIO(ref m_dioPusher, this, "Pusher", "Backward", "Forward");
            p_sInfo = m_toolBox.GetDIO(ref m_diOverload, this, "Overload");
            p_sInfo = m_toolBox.GetAxis(ref m_axis, this, "Elevator"); 
            if (bInit) 
            {
                InitALID(); 
                InitPosElevator(); 
            }
        }
        #endregion

        #region GAF
        ALID m_alidHome; 
        ALID m_alidPusherOverload;
        ALID m_alidProtrusion;
        void InitALID()
        {
            m_alidHome = m_gaf.GetALID(this, "Home Error", "Home Error");
            m_alidPusherOverload = m_gaf.GetALID(this, "PusherOverload", "Pusher Overload Error");
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
            DIO_IO m_dioClamp;
            DIO_I m_diProtrusion; 
            public void GetTools(ToolBox toolBox)
            {
                m_storage.p_sInfo = toolBox.GetDIO(ref m_diCheck, m_storage, m_id + ".Check");
                m_storage.p_sInfo = toolBox.GetDIO(ref m_dioClamp, m_storage, m_id + ".Clamp");
                m_storage.p_sInfo = toolBox.GetDIO(ref m_diProtrusion, m_storage, m_id + ".Protrusion");
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
                    m_rail.RunRotate(false);
                    m_roller.RunRotate(false); 
                    return true;
                }
            }
            if (m_rail.IsStartOn())
            {
                m_alidProtrusion.Run(true, "Check Rail Sensor");
                m_rail.RunRotate(false);
                m_roller.RunRotate(false);
                return true;
            }
            return false; 
        }
        #endregion

        #region Stack
        public class Stack : NotifyProperty
        {
            DIO_I m_diLevel;
            public DIO_I[] m_diCheck = new DIO_I[4]; 
            public void GetTools(ToolBox toolBox)
            {
                m_storage.p_sInfo = toolBox.GetDIO(ref m_diLevel, m_storage, m_id + ".Level");
                m_storage.p_sInfo = toolBox.GetDIO(ref m_diCheck[0], m_storage, m_id + ".Check0");
                m_storage.p_sInfo = toolBox.GetDIO(ref m_diCheck[1], m_storage, m_id + ".Check1");
                m_storage.p_sInfo = toolBox.GetDIO(ref m_diCheck[2], m_storage, m_id + ".Check2");
                m_storage.p_sInfo = toolBox.GetDIO(ref m_diCheck[3], m_storage, m_id + ".Check3");
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

            public void CheckSensor()
            {
                p_bLevel = m_diLevel.p_bIn;
                p_bCheck = m_diCheck[0].p_bIn || m_diCheck[1].p_bIn || m_diCheck[2].p_bIn || m_diCheck[3].p_bIn; 
            }

            string m_id;
            public Storage m_storage; 
            public Stack(string id, Storage storage)
            {
                m_id = id;
                m_storage = storage; 
            }
        }
        public Stack m_stack; 
        void InitStack()
        {
            m_stack = new Stack("Stack", this); 
        }
        #endregion

        #region Pusher
        DIO_I2O m_dioPusher;
        DIO_I m_diOverload;
        public string RunPusher()
        {
            int msWait = (int)(1000 * m_dioPusher.m_secTimeout); 
            StopWatch sw = new StopWatch();
            m_rail.CheckStrip(false); 
            m_dioPusher.Write(true);
            while (m_dioPusher.p_bDone == false)
            {
                Thread.Sleep(10);
                if (m_diOverload.p_bIn)
                {
                    m_dioPusher.Write(false);
                    m_alidPusherOverload.p_bSet = true; 
                    return "Overload Sensor Check";
                }
                if (sw.ElapsedMilliseconds > msWait)
                {
                    m_dioPusher.Write(false);
                    Thread.Sleep(msWait); 
                    return "Run Pusher Timeout";
                }
            }
            m_dioPusher.Write(false);
            string sDone = m_dioPusher.WaitDone();
            m_rail.CheckStrip(true);
            return sDone;
        }
        #endregion

        #region Elevator
        Loader p_loader
        {
            get
            {
                RinseL_Handler handler = (RinseL_Handler)m_engineer.ClassHandler();
                return (handler == null) ? null : handler.m_loader;
            }
        }
        bool IsLoaderDanger()
        {
            return false; 
            //if (p_loader == null) return true;
            //return p_loader.IsLoaderDanger(); 
        }

        Axis m_axis;
        void InitPosElevator()
        {
            m_axis.AddPos(Enum.GetNames(typeof(eMagazine)));
            m_axis.AddPos("Stack");
            m_axis.AddPos("Stack_Down");
            m_axis.AddPos("StackTop");
        }

        double p_posStackTop {  get { return m_axis.GetPosValue("StackTop"); } }

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

        public string MoveStack_Down()
        {
            return MoveElevator(m_axis.GetPosValue("Stack_Down"));
        }

        double m_pulseDown = 10000; 
        double m_posStackReady = -100000;
        double m_fJogScale = 1; 
        public string MoveStackReady()
        {
            if (IsLoaderDanger()) return "Check Loader Position";
            if (IsProtrusion()) return "Protrusion Error";
            if (m_axis.p_posCommand > m_posStackReady - m_pulseDown) MoveStack();
            if (m_stack.p_bLevel)
            {
                m_axis.Jog(-m_fJogScale);
                while (m_stack.p_bLevel && (EQ.IsStop() == false)) Thread.Sleep(10);
                m_axis.StopAxis();
                Thread.Sleep(500);
            }
            m_axis.Jog(m_fJogScale);
            while (!m_stack.p_bLevel && (EQ.IsStop() == false) && (m_axis.p_posCommand < p_posStackTop)) Thread.Sleep(10);
            m_posStackReady = m_axis.p_posCommand;
            m_axis.StopAxis();
            m_axis.WaitReady();
            Thread.Sleep(500);
            return "OK";
        }

        public bool IsHighPos()
        {
            return m_axis.p_posCommand > (p_posStackTop + 1000); 
        }

        public bool p_bIsEnablePick
        {
            get { return Math.Abs(m_posStackReady - m_axis.p_posCommand) < 10; }
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
            m_pulseDown = tree.Set(m_pulseDown, m_pulseDown, "Stack Down", "Stack Down (pulse)");
        }
        #endregion

        #region RunMagazine
        double m_secRunDelay = 0; 
        public string RunMagazine()
        {
            while ((EQ.p_bStop == false) && (EQ.p_eState == EQ.eState.Run))
            {
                Thread.Sleep(1);
                if (Run(RunMagazine(m_rinse.p_eMagazine, m_rinse.p_iMagazine))) return p_sInfo;
                m_rinse.p_iMagazine++;
                if (m_rinse.p_iMagazine >= 20)
                {
                    m_rinse.p_iMagazine = 0;
                    eMagazine eMagazine = m_rinse.p_eMagazine; 
                    switch(eMagazine)
                    {
                        case eMagazine.Magazine1:
                            Thread.Sleep(10000);
                            m_rinse.SendFinish();
                            EQ.p_eState = EQ.eState.Ready;

                            return "OK";
                        case eMagazine.Magazine2: m_rinse.p_eMagazine = eMagazine.Magazine1; break;
                        case eMagazine.Magazine3: m_rinse.p_eMagazine = eMagazine.Magazine2; break;
                        case eMagazine.Magazine4: m_rinse.p_eMagazine = eMagazine.Magazine3; break;
                    }
                }
            }
            return (EQ.p_eState == EQ.eState.Run) ? "EQ Stop" : "OK";
        }

        string RunMagazine(eMagazine eMagazine, int iMagazine)
        {
            if (m_aMagazine[(int)eMagazine].p_bCheck == false) return "OK";
            if (m_aMagazine[(int)eMagazine].p_bClamp == false) return "OK";
            if (Run(MoveMagazine(eMagazine, iMagazine, true))) return p_sInfo;
            if (IsStopMagazine()) return (EQ.p_eState == EQ.eState.Run) ? "EQ Stop" : "OK";
            if (Run(RunPusher())) return p_sInfo;
            if (IsStopMagazine()) return (EQ.p_eState == EQ.eState.Run) ? "EQ Stop" : "OK";
            return "OK"; 
        }

        bool IsStopMagazine()
        {
            StopWatch sw = new StopWatch();
            int msDelay = (int)(500 * m_secRunDelay);
            while (sw.ElapsedMilliseconds < msDelay)
            {
                Thread.Sleep(100);
                if (EQ.p_eState != EQ.eState.Run) return true;
                if (EQ.p_bStop) return true;
            }
            return false; 
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
                m_alidHome.Run(true, "Protrusion Error");
                return "Protrusion Error";
            }
            foreach (Magazine magazine in m_aMagazine) magazine.RunClamp(magazine.p_bCheck);
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            m_alidHome.Run(p_sInfo != "OK", p_sInfo);
            return p_sInfo;
        }

        public override void Reset()
        {
            base.Reset();
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

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeElevator(tree.GetTree("Elevator"));
            m_secRunDelay = tree.Set(m_secRunDelay, m_secRunDelay, "Run Delay", "Run Delay (sec)"); 
        }
        #endregion

        RinseL m_rinse;
        Rail m_rail;
        Roller m_roller; 
        public Storage(string id, IEngineer engineer, RinseL rinse, Rail rail, Roller roller)
        {
            p_id = id;
            m_rinse = rinse;
            m_rail = rail;
            m_roller = roller; 
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
                case RinseL.eRunMode.Magazine:
                    StartRun(m_runMagazine.Clone()); 
                    break;
                case RinseL.eRunMode.Stack:
                    MoveStack(); 
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

        ModuleRunBase m_runReady;
        ModuleRunBase m_runMagazine;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay (sec)");
            AddModuleRunList(new Run_MoveMagazine(this), false, "Move Elevator Magazine Position");
            AddModuleRunList(new Run_MoveStack(this), false, "Move Elevator Stack Position");
            AddModuleRunList(new Run_Pusher(this), false, "Move Elevator & Run Pusher");
            AddModuleRunList(new Run_Clamp(this), false, "Run Clamp");
            m_runReady = AddModuleRunList(new Run_StackReady(this), false, "Run Stack Move Ready Position");
            m_runMagazine = AddModuleRunList(new Run_RunMagazine(this), false, "Run Magazine");
        }

        public class Run_Delay : ModuleRunBase
        {
            Storage m_module;
            public Run_Delay(Storage module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            int m_secDelay = 1;
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
                Thread.Sleep(1000 * m_secDelay); 
                return "OK";
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

        public class Run_Pusher : ModuleRunBase
        {
            Storage m_module;
            public Run_Pusher(Storage module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            eMagazine m_eMagazine = eMagazine.Magazine1;
            int m_iIndex = 0;
            public override ModuleRunBase Clone()
            {
                Run_Pusher run = new Run_Pusher(m_module);
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
                if (m_module.Run(m_module.MoveMagazine(m_eMagazine, m_iIndex, true))) return p_sInfo;
                return m_module.RunPusher(); 
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

        public class Run_RunMagazine : ModuleRunBase
        {
            Storage m_module;
            public Run_RunMagazine(Storage module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_RunMagazine run = new Run_RunMagazine(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunMagazine();
            }
        }
        #endregion
    }
}
