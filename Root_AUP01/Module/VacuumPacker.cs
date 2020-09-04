using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_AUP01.Module
{
    public class VacuumPacker : ModuleBase
    {
        #region ToolBox
        DIO_IO[] m_dioVacuum = new DIO_IO[2] { null, null };
        DIO_O[] m_doBlow = new DIO_O[2];
        Axis m_axisLoad;

        DIO_I2O[] m_dioStep1 = new DIO_I2O[2] { null, null };
        DIO_I2O m_dioStep2;
        DIO_O[] m_doHeater = new DIO_O[2] { null, null };

        Axis m_axisGuide;
        DIO_IO m_dioVacuumPump;
        DIO_O m_doVacuumPumpBlow;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_dioVacuum[0], this, "Bottom Vacuum");
            p_sInfo = m_toolBox.Get(ref m_doBlow[0], this, "Bottom Blow");
            p_sInfo = m_toolBox.Get(ref m_dioVacuum[1], this, "Top Vacuum");
            p_sInfo = m_toolBox.Get(ref m_doBlow[1], this, "Top Blow");
            p_sInfo = m_toolBox.Get(ref m_axisLoad, this, "Load");

            p_sInfo = m_toolBox.Get(ref m_dioStep1[0], this, "Step1Left", "Up", "Down");
            p_sInfo = m_toolBox.Get(ref m_dioStep1[1], this, "Step1Right", "Up", "Down");
            p_sInfo = m_toolBox.Get(ref m_dioStep2, this, "Step2", "Up", "Down");
            p_sInfo = m_toolBox.Get(ref m_doHeater[0], this, "Bottom Heater");
            p_sInfo = m_toolBox.Get(ref m_doHeater[1], this, "Top Heater");

            p_sInfo = m_toolBox.Get(ref m_axisGuide, this, "Guide");
            p_sInfo = m_toolBox.Get(ref m_dioVacuumPump, this, "VacuumPump");
            p_sInfo = m_toolBox.Get(ref m_doVacuumPumpBlow, this, "Blow");

            if (bInit) InitTools();
        }

        void InitTools()
        {
            m_dioVacuum[0].Write(false);
            m_dioVacuum[1].Write(false);
            m_dioStep1[0].Write(false);
            m_dioStep1[1].Write(false);
            m_dioStep2.Write(false);
            m_dioVacuumPump.Write(false);
            m_doVacuumPumpBlow.Write(false);

            InitAxisPosLoad(); 
            InitAxisPosGuide(); 
        }
        #endregion

        #region Timeout
        double m_sVac = 2;
        double m_sBlow = 0.5;
        double m_sSolStep1 = 5;
        double m_sSolStep2 = 5;
        double m_sVacuumPump = 20;
        void RunTreeDIOWait(Tree tree)
        {
            m_sVac = tree.Set(m_sVac, m_sVac, "Vac", "Vacuum Sensor Wait (sec)");
            m_sBlow = tree.Set(m_sBlow, m_sBlow, "Blow", "Blow Time (sec)");
            m_sSolStep1 = tree.Set(m_sSolStep1, m_sSolStep1, "Step1", "Sol Value Move Wait (sec)");
            m_sSolStep2 = tree.Set(m_sSolStep2, m_sSolStep2, "Step2", "Sol Value Move Wait (sec)");
            m_sVacuumPump = tree.Set(m_sVacuumPump, m_sVacuumPump, "Vacuum Pump", "Vacuum Sensor Wait (sec)");
        }
        #endregion

        #region DIO Function
        string RunSol(DIO_I2O dio, bool bOn, double sWait)
        {
            dio.Write(bOn);
            int msWait = (int)(1000 * sWait);
            while (dio.p_bDone != true)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return p_id + " EQ Stop";
                if (dio.m_swWrite.ElapsedMilliseconds > msWait) return dio.m_id + " Sol Valve Move Timeout";
            }
            return "OK";
        }
        #endregion

        #region Vacuum Function
        string RunVacuum(bool bOn)
        {
            m_dioVacuum[0].Write(bOn);
            m_dioVacuum[1].Write(bOn);
            if (bOn == false)
            {
                m_doBlow[0].Write(true);
                m_doBlow[1].Write(true);
                Thread.Sleep((int)(1000 * m_sBlow));
                m_doBlow[0].Write(false);
                m_doBlow[1].Write(false);
                return "OK";
            }
            int msVac = (int)(1000 * m_sVac);
            while ((m_dioVacuum[0].p_bIn != bOn) || (m_dioVacuum[1].p_bIn != bOn))
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return p_id + " EQ Stop";
                if (m_dioVacuum[0].m_swWrite.ElapsedMilliseconds > msVac) return "Vacuum Sensor Timeout";
            }
            return "OK";
        }
        #endregion

        #region Load Function
        enum eLoad
        {
            Ready,
            VacuumPump,
            Load
        }
        void InitAxisPosLoad()
        {
            m_axisLoad.AddPos(Enum.GetNames(typeof(eLoad)));
        }

        string RunLoad(eLoad load)
        {
            m_axisLoad.StartMove(load);
            return m_axisLoad.WaitReady(3);
        }
        #endregion

        #region Step Function
        string RunStep1(bool bOn)
        {
            m_dioStep1[0].Write(bOn);
            m_dioStep1[1].Write(bOn);
            int msWait = (int)(1000 * m_sSolStep1);
            while ((m_dioStep1[0].p_bDone != true) || (m_dioStep1[0].p_bDone != true))
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return p_id + " EQ Stop";
                if (m_dioStep1[0].m_swWrite.ElapsedMilliseconds > msWait) return m_dioStep1[0].m_id + " Sol Valve Move Timeout";
            }
            return "OK";
        }

        string RunStep2(bool bOn)
        {
            return RunSol(m_dioStep2, bOn, m_sSolStep2);
        }
        #endregion

        #region Heater
        string RunHeater(double secHeat)
        {
            int msHeat = (int)(1000 * secHeat);
            StopWatch sw = new StopWatch();
            try
            {
                m_doHeater[0].Write(true);
                m_doHeater[1].Write(true);
                while (sw.ElapsedMilliseconds < msHeat)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return p_id + " EQ Stop";
                }
                return "OK";
            }
            finally
            {
                m_doHeater[0].Write(false);
                m_doHeater[1].Write(false);
            }
        }
        #endregion

        #region Guide Function
        enum eGuide
        {
            Ready,
            Open
        }
        void InitAxisPosGuide()
        {
            m_axisGuide.AddPos(Enum.GetNames(typeof(eGuide))); 
        }

        string RunGuide(eGuide guide)
        {
            m_axisGuide.StartMove(guide);
            return m_axisGuide.WaitReady(3);
        }

        string RunVacuumPump(bool bOn)
        {
            if (bOn == false)
            {
                m_dioVacuumPump.Write(false);
                return "OK";
            }
            m_dioVacuumPump.Write(true);
            int msTimeout = (int)(1000 * m_sVacuumPump);
            StopWatch sw = new StopWatch();
            while (m_dioVacuumPump.p_bIn == false)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return p_id + " EQ Stop";
                if (sw.ElapsedMilliseconds > msTimeout) return "Vacuum Pump Sensor Timeout";
            }
            return "OK";
        }
        #endregion

        #region Override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            RunTreeDIOWait(tree.GetTree("Timeout", false));
        }

        public override void Reset()
        {
            base.Reset();
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
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }
        #endregion

        public VacuumPacker(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
            AddModuleRunList(new Run_Vacuum(this), true, "Run Load");
            AddModuleRunList(new Run_Guide(this), true, "Run Guide");
            AddModuleRunList(new Run_Load(this), true, "Run Load");
            AddModuleRunList(new Run_Step1(this), true, "Run Step1");
            AddModuleRunList(new Run_Step2(this), true, "Run Step2");
            AddModuleRunList(new Run_VacuumPump(this), true, "Run VacuumPump");
            AddModuleRunList(new Run_Heating(this), true, "Run Heating");
        }

        public class Run_Delay : ModuleRunBase
        {
            VacuumPacker m_module;
            public Run_Delay(VacuumPacker module)
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

        public class Run_Vacuum : ModuleRunBase
        {
            VacuumPacker m_module;
            public Run_Vacuum(VacuumPacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bVacOn = false; 
            public override ModuleRunBase Clone()
            {
                Run_Vacuum run = new Run_Vacuum(m_module);
                run.m_bVacOn = m_bVacOn;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bVacOn = tree.Set(m_bVacOn, m_bVacOn, "Vacuum", "Run Vacuum", bVisible);
            }

            public override string Run()
            {
                return m_module.RunVacuum(m_bVacOn);
            }
        }

        public class Run_Guide : ModuleRunBase
        {
            VacuumPacker m_module;
            public Run_Guide(VacuumPacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            eGuide m_eGuide = eGuide.Ready; 
            public override ModuleRunBase Clone()
            {
                Run_Guide run = new Run_Guide(m_module);
                run.m_eGuide = m_eGuide;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eGuide = (eGuide)tree.Set(m_eGuide, m_eGuide, "Guide", "Run Guide", bVisible);
            }

            public override string Run()
            {
                return m_module.RunGuide(m_eGuide);
            }
        }

        public class Run_Load : ModuleRunBase
        {
            VacuumPacker m_module;
            public Run_Load(VacuumPacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            eLoad m_eLoad = eLoad.Ready;
            public override ModuleRunBase Clone()
            {
                Run_Load run = new Run_Load(m_module);
                run.m_eLoad = m_eLoad;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eLoad = (eLoad)tree.Set(m_eLoad, m_eLoad, "Load", "Run Load", bVisible);
            }

            public override string Run()
            {
                return m_module.RunLoad(m_eLoad);
            }
        }

        public class Run_Step1 : ModuleRunBase
        {
            VacuumPacker m_module;
            public Run_Step1(VacuumPacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bOn = false;
            public override ModuleRunBase Clone()
            {
                Run_Step1 run = new Run_Step1(m_module);
                run.m_bOn = m_bOn;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bOn = tree.Set(m_bOn, m_bOn, "Down", "Run Load", bVisible);
            }

            public override string Run()
            {
                return m_module.RunStep1(m_bOn);
            }
        }

        public class Run_Step2 : ModuleRunBase
        {
            VacuumPacker m_module;
            public Run_Step2(VacuumPacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bOn = false;
            public override ModuleRunBase Clone()
            {
                Run_Step2 run = new Run_Step2(m_module);
                run.m_bOn = m_bOn;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bOn = tree.Set(m_bOn, m_bOn, "Down", "Run Step2", bVisible);
            }

            public override string Run()
            {
                return m_module.RunStep2(m_bOn);
            }
        }

        public class Run_VacuumPump : ModuleRunBase
        {
            VacuumPacker m_module;
            public Run_VacuumPump(VacuumPacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bOn = false;
            public override ModuleRunBase Clone()
            {
                Run_VacuumPump run = new Run_VacuumPump(m_module);
                run.m_bOn = m_bOn;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bOn = tree.Set(m_bOn, m_bOn, "On", "Run VacuumPump", bVisible);
            }

            public override string Run()
            {
                return m_module.RunVacuumPump(m_bOn);
            }
        }

        public class Run_Heating : ModuleRunBase
        {
            VacuumPacker m_module;
            public Run_Heating(VacuumPacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_sHeat = 30; 
            public override ModuleRunBase Clone()
            {
                Run_Heating run = new Run_Heating(m_module);
                run.m_sHeat = m_sHeat; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sHeat = tree.Set(m_sHeat, m_sHeat, "Time", "Heating Time (sec)", bVisible);
            }

            public override string Run()
            {
                return m_module.RunHeater(m_sHeat);
            }
        }
        #endregion
    }
}
