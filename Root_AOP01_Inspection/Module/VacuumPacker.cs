using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_AOP01_Inspection.Module
{
    public class VacuumPacker : ModuleBase
    {
        int TestNum = 1;
        #region ToolBox
        DIO_IO[] m_dioVacuum = new DIO_IO[2] { null, null };
        DIO_O[] m_doBlow = new DIO_O[2];
        Axis m_axisLoad;

        DIO_I2O[] m_dioStep1 = new DIO_I2O[2] { null, null };
        DIO_I2O m_dioStep2;
        //DIO_O[] m_doHeater = new DIO_O[2] { null, null };

        Axis m_axisGuide;
        DIO_IO m_dioVacuumPump;
        DIO_O m_doVacuumPumpBlow;

        DIO_I2O m_dioTopClampUpDown; // LJM 201020 Add
        DIO_I2O m_dioBtmClampUpDown; // LJM 201020 Add
        DIO_I2O m_dioTopHeaterUpdown; // LJM 201020 Add
        DIO_I2O m_dioBtmHeaterUpdown; // LJM 201020 Add
        DIO_I2O m_dioTopTotalHeaterUpdown; // LJM 201020 Add
        DIO_O m_doHeater; // LJM 201020 Add






        public override void GetTools(bool bInit)
        {
            
            if(TestNum == 0) // 기존
            {
                p_sInfo = m_toolBox.Get(ref m_dioVacuum[0], this, "Bottom Vacuum");
                p_sInfo = m_toolBox.Get(ref m_doBlow[0], this, "Bottom Blow");
                p_sInfo = m_toolBox.Get(ref m_dioVacuum[1], this, "Top Vacuum");
                p_sInfo = m_toolBox.Get(ref m_doBlow[1], this, "Top Blow");
                p_sInfo = m_toolBox.Get(ref m_axisLoad, this, "Load");

                p_sInfo = m_toolBox.Get(ref m_dioStep1[0], this, "Step1Left", "Up", "Down");
                p_sInfo = m_toolBox.Get(ref m_dioStep1[1], this, "Step1Right", "Up", "Down");
                p_sInfo = m_toolBox.Get(ref m_dioStep2, this, "Step2", "Up", "Down");

                //p_sInfo = m_toolBox.Get(ref m_doHeater[0], this, "Bottom Heater");
                //p_sInfo = m_toolBox.Get(ref m_doHeater[1], this, "Top Heater");
                p_sInfo = m_toolBox.Get(ref m_doHeater, this, "Heater Heating"); // 상면히터만사용하기로함.
                p_sInfo = m_toolBox.Get(ref m_axisGuide, this, "Guide");
                p_sInfo = m_toolBox.Get(ref m_dioVacuumPump, this, "VacuumPump");
                p_sInfo = m_toolBox.Get(ref m_doVacuumPumpBlow, this, "Blow");
            }
            else if(TestNum == 1) // 1차 실링 테스트 // LJM 201020 Add
            {
                p_sInfo = m_toolBox.Get(ref m_axisLoad, this, "Load");
                p_sInfo = m_toolBox.Get(ref m_axisGuide, this, "Guide");
                p_sInfo = m_toolBox.Get(ref m_dioTopClampUpDown, this, "TopClamp", "Up", "Down");
                p_sInfo = m_toolBox.Get(ref m_dioBtmClampUpDown, this, "BtmClamp", "Down", "Up");
                p_sInfo = m_toolBox.Get(ref m_dioTopHeaterUpdown, this, "TopHeater", "Up", "Down");
                p_sInfo = m_toolBox.Get(ref m_dioBtmHeaterUpdown, this, "BtmHeater", "Down", "Up");
                p_sInfo = m_toolBox.Get(ref m_dioTopTotalHeaterUpdown, this, "TotalHeater", "Down", "Up");

                p_sInfo = m_toolBox.Get(ref m_doHeater, this, "Heater Heating");
            }

            
            if (bInit) InitTools();
        }

        void InitTools()
        {
            if(TestNum == 0)
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
            else if(TestNum == 1) // LJM 201020 Add
            {
                m_dioTopClampUpDown.Write(false);
                m_dioBtmClampUpDown.Write(false);
                m_dioTopHeaterUpdown.Write(false);
                m_dioBtmHeaterUpdown.Write(false);
                m_dioTopTotalHeaterUpdown.Write(true);
                m_doHeater.Write(false);
                InitAxisPosLoad();
                InitAxisPosGuide();
            }
            

            
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
            Heating,//Load
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
            RunSol(m_dioTopTotalHeaterUpdown, !bOn, m_sSolStep2);
            RunSol(m_dioTopClampUpDown, bOn, m_sSolStep2);
            RunSol(m_dioBtmClampUpDown, bOn, m_sSolStep2);

            //m_dioTopTotalHeaterUpdown.Write(!bOn);
            //m_dioTopClampUpDown.Write(bOn);
            //m_dioBtmClampUpDown.Write(bOn);



            //m_dioStep1[0].Write(bOn);
            //m_dioStep1[1].Write(!bOn);
            //int msWait = (int)(1000 * m_sSolStep1);
            //while ((m_dioStep1[0].p_bDone != true) || (m_dioStep1[0].p_bDone != true))
            //{
            //    Thread.Sleep(10);
            //    if (EQ.IsStop()) return p_id + " EQ Stop";
            //    if (m_dioStep1[0].m_swWrite.ElapsedMilliseconds > msWait) return m_dioStep1[0].m_id + " Sol Valve Move Timeout";
            //}
            return "OK";
        }

        string RunStep2(bool bOn)
        {
            RunSol(m_dioTopHeaterUpdown, bOn, m_sSolStep2);
            RunSol(m_dioBtmHeaterUpdown, bOn, m_sSolStep2);
            return "OK";
            //return RunSol(m_dioStep2, bOn, m_sSolStep2);
        }
        

        #endregion

        #region Heater
        string RunHeater(double secHeat)
        {
            int msHeat = (int)(1000 * secHeat);
            StopWatch sw = new StopWatch();
            try
            {
                m_doHeater.Write(true);
                while (sw.ElapsedMilliseconds < msHeat)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return p_id + " EQ Stop";
                }
                return "OK";
            }
            finally
            {
                m_doHeater.Write(false);
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
            AddModuleRunList(new Run_Packing(this), true, "Run Packing");
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

            //double m_sHeat = 30; 
            public override ModuleRunBase Clone()
            {
                Run_Heating run = new Run_Heating(m_module);
                //run.m_sHeat = m_sHeat; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                //m_sHeat = tree.Set(m_sHeat, m_sHeat, "Time", "Heating Time (sec)", bVisible);
            }

            public override string Run()
            {
                return "";// m_module.RunHeater(m_sHeat);
            }
        }
        

        public class Run_Packing : ModuleRunBase
        {
            VacuumPacker m_module;

            //eLoad m_eLoad = eLoad.Ready;            
            //eGuide m_eGuide = eGuide.Ready;


            double m_sCartride = 1;
            double m_fDeg = 720;
            double m_v = 120;
            double m_acc = 2;
            double m_sHeat = 0;
            double m_sVacTime = 0;

            public Run_Packing(VacuumPacker module)
            {
                m_module = module;
                InitModuleRun(module);
                
            }

            public override ModuleRunBase Clone()
            {
                Run_Packing run = new Run_Packing(m_module);
                run.m_sCartride = m_sCartride;
                run.m_fDeg = m_fDeg;
                run.m_v = m_v;
                run.m_acc = m_acc;
                run.m_sVacTime = m_sVacTime;
                run.m_sHeat = m_sHeat;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                //m_eLoad = (eLoad)tree.Set(m_eLoad, m_eLoad, "Load", "Run Load", bVisible);
                //m_eGuide = (eGuide)tree.Set(m_eGuide, m_eGuide, "Guide", "Run Guide", bVisible);
                m_sHeat = tree.Set(m_sHeat, m_sHeat, "Heating Time", "Heating Time (sec)", bVisible);
                m_sVacTime = tree.Set(m_sVacTime, m_sVacTime, "Vaccume Time", "Vaccume Time (sec)", bVisible);

            }

            public override string Run()
            {
                for (int i = 0; i < 1; i++)
                {
                    if(m_module.TestNum == 1)
                    {
                        if (m_module.Run(m_module.RunSol(m_module.m_dioTopTotalHeaterUpdown, true, m_module.m_sSolStep2))) return p_sInfo; // Total 실린더 Up
                        if (m_module.Run(m_module.RunGuide(eGuide.Ready))) return p_sInfo; // side 축 레디위치로 이동.
                        if (m_module.Run(m_module.RunLoad(eLoad.VacuumPump))) return p_sInfo; // Load 축 이동 하드웨어 리밋정도까지 최대한 이동한 작업점까지.
                        if (m_module.Run(m_module.RunGuide(eGuide.Open))) return p_sInfo; // side 축 open위치로 이동.
                        if (m_module.Run(m_module.RunStep1(true))) return p_sInfo; // 상단, 하단클램프 및 Total 실린더 다운.
                        Thread.Sleep((int)m_sVacTime * 1000); // 진공상태 대기.
                        if (m_module.Run(m_module.RunLoad(eLoad.Heating))) return p_sInfo; // Load 축 이동 히팅기랑 간섭없는 위치로 이동.
                        if (m_module.Run(m_module.RunStep2(true))) return p_sInfo; // 상부 히팅 다운 하부 히팅 업.
                        if (m_module.Run(m_module.RunHeater(m_sHeat))) return p_sInfo; // 히팅
                        if (m_module.Run(m_module.RunStep2(false))) return p_sInfo; // 상부 히팅 업 하부 히팅 다운.
                        if (m_module.Run(m_module.RunStep1(false))) return p_sInfo; // 상단, 하단클램프 및 Total 실린더 업.
                        if (m_module.Run(m_module.RunGuide(eGuide.Ready))) return p_sInfo; // side 축 레디위치로 이동.
                        if (m_module.Run(m_module.RunLoad(eLoad.Ready))) return p_sInfo;
                    }
                    else
                    {
                        if (m_module.Run(m_module.RunStep1(true))) return p_sInfo; // Step1 위쪽, 아래쪽/ Step2 위쪽 작은실린더/ Load = Y축/ Guide = X축 , 
                        if (m_module.Run(m_module.RunLoad(eLoad.Ready))) return p_sInfo;
                        if (m_module.Run(m_module.RunGuide(eGuide.Ready))) return p_sInfo;
                        if (m_module.Run(m_module.RunLoad(eLoad.VacuumPump))) return p_sInfo;
                        if (m_module.Run(m_module.RunGuide(eGuide.Open))) return p_sInfo;
                        if (m_module.Run(m_module.RunStep1(false))) return p_sInfo;
                        //if (m_module.Run(m_module.RunVacuum(true))) return p_sInfo;                  
                        if (m_module.Run(m_module.RunStep2(true))) return p_sInfo;
                        //if (m_module.Run(m_module.RunHeater(m_sHeat))) return p_sInfo;
                        if (m_module.Run(m_module.RunStep2(false))) return p_sInfo;
                        if (m_module.Run(m_module.RunStep1(true))) return p_sInfo;
                        if (m_module.Run(m_module.RunGuide(eGuide.Ready))) return p_sInfo;
                        if (m_module.Run(m_module.RunLoad(eLoad.Ready))) return p_sInfo;
                        //if (m_module.Run(m_module.RunLoad(eLoad.Load))) return p_sInfo;
                        //if (m_module.Run(m_module.RunLoad(eLoad.Load))) return p_sInfo;
                        //if (m_module.Run(m_module.RunLoad(eLoad.Load))) return p_sInfo;
                    }
                }
                return "OK";
            }
        }
        #endregion
    }
}
