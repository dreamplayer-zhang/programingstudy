using RootTools;
using RootTools.Comm;
using RootTools.Module;
using RootTools.ParticleCounter;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System.Threading;

namespace Root_VEGA_P.Module
{
    public class ParticleCounter : ModuleBase
    {
        #region ToolBox
        LasairIII m_lasair; 
        public override void GetTools(bool bInit)
        {
            m_regulator.GetTools(m_toolBox, bInit);
            m_nozzleSet.GetTools(m_toolBox);
            p_sInfo = m_toolBox.Get(ref m_lasair, this, "LasAir3"); 
            if (bInit) { }
        }
        #endregion

        #region Regulator
        public class Regulator : NotifyProperty
        {
            RS232 m_rs232; 
            public string GetTools(ToolBox toolBox, bool bInit)
            {
                string sInfo = toolBox.GetComm(ref m_rs232, m_particleCounter, "Regulator");
                if (bInit) m_rs232.p_bConnect = true;
                return sInfo; 
            }

            public int m_nPump = 0; 
            public string RunPump(int nPump)
            {
                string sInfo = ConnectRS232();
                if (sInfo != "OK") return sInfo;
                m_nPump = nPump; 
                m_rs232.Send("SET " + nPump.ToString());
                return "OK";
            }

            string ConnectRS232()
            {
                if (m_rs232.p_bConnect) return "OK"; 
                m_rs232.p_bConnect = true;
                Thread.Sleep(100); 
                return m_rs232.p_bConnect ? "OK" : "RS232 Connect Error";
            }

            ParticleCounter m_particleCounter;
            public Regulator(ParticleCounter particleCounter)
            {
                m_particleCounter = particleCounter; 
            }
        }
        Regulator m_regulator; 
        void InitRegulator()
        {
            m_regulator = new Regulator(this); 
        }
        #endregion

        #region NozzleSet
        NozzleSet m_nozzleSet; 
        void InitNozzleSet()
        {
            m_nozzleSet = new NozzleSet(this); 
        }
        #endregion

        #region Flow Sensor
        int m_nUnitFlow = 1;
        VEGA_P.FlowSensor m_flowSensor;
        public string RunReadFlow()
        {
            return "OK"; //forget
        }

        void RunTreeFlow(Tree tree)
        {
            m_nUnitFlow = tree.Set(m_nUnitFlow, m_nUnitFlow, "Unit", "Modbus Unit ID"); 
        }
        #endregion

        #region Lasair
        int m_nUnitLasair = 1;
        void RunTreeLasair(Tree tree)
        {
            m_nUnitLasair = tree.Set(m_nUnitLasair, m_nUnitLasair, "Unit", "Modbus Unit ID");
        }
        #endregion

        #region Run
        public string RunCount(Run_Run run)
        {
            if (Run(m_nozzleSet.RunNozzle(run.m_open))) return p_sInfo; 
            if (Run(m_regulator.RunPump(run.m_nPump))) return p_sInfo;
            if (Run(m_lasair.StartRun(run.m_sample))) return p_sInfo; 
            while (m_lasair.IsBusy())
            {
                Thread.Sleep(100);
                if (m_lasair.IsTimeout()) return "Particle Counter Run Timeout";
                if (EQ.IsStop()) return "EQ Stop"; 
            }
            return "OK"; 
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_nozzleSet.RunTreeName(tree.GetTree("NozzleSet"));
            RunTreeFlow(tree.GetTree("Flow Sensor"));
            RunTreeLasair(tree.GetTree("Lasair"));
        }
        #endregion

        public ParticleCounter(string id, IEngineer engineer, VEGA_P.FlowSensor flowSensor)
        {
            p_id = id;
            m_flowSensor = flowSensor; 
            InitRegulator();
            InitNozzleSet();
            InitBase(id, engineer);
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            AddModuleRunList(new Run_Pump(this), false, "Run Pump");
            AddModuleRunList(new Run_ReadFlow(this), false, "Read Flow Sensor");
            AddModuleRunList(new Run_Run(this), false, "Run Particle Counter");
        }

        public class Run_Delay : ModuleRunBase
        {
            ParticleCounter m_module;
            public Run_Delay(ParticleCounter module)
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
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Delay Time (sec)", bVisible);
            }

            public override string Run()
            {
                Thread.Sleep(1000 * m_secDelay);
                return "OK";
            }
        }

        public class Run_Pump : ModuleRunBase
        {
            ParticleCounter m_module;
            public Run_Pump(ParticleCounter module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            int m_secDelay = 1;
            public override ModuleRunBase Clone()
            {
                Run_Pump run = new Run_Pump(m_module);
                run.m_nPump = m_nPump; 
                run.m_secDelay = m_secDelay;
                return run;
            }

            int m_nPump = 1000; 
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nPump = tree.Set(m_nPump, m_nPump, "Pump", "Pump Power (0 ~ 1000)", bVisible); 
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Delay Time (sec)", bVisible);
            }

            public override string Run()
            {
                if (m_module.Run(m_module.m_regulator.RunPump(m_nPump))) return p_sInfo; 
                Thread.Sleep(1000 * m_secDelay);
                if (m_module.Run(m_module.m_regulator.RunPump(0))) return p_sInfo;
                return "OK";
            }
        }

        public class Run_ReadFlow : ModuleRunBase
        {
            ParticleCounter m_module;
            public Run_ReadFlow(ParticleCounter module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_ReadFlow run = new Run_ReadFlow(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunReadFlow();
            }
        }

        public class Run_Run : ModuleRunBase
        {
            ParticleCounter m_module;
            public Run_Run(ParticleCounter module)
            {
                m_module = module;
                m_open = new NozzleSet.Open(m_module.m_nozzleSet.m_open);
                m_sample = new ParticleCounterBase.Sample(); 
                InitModuleRun(module);
            }

            public int m_nPump = 100;
            public NozzleSet.Open m_open;
            public ParticleCounterBase.Sample m_sample; 
            public override ModuleRunBase Clone()
            {
                Run_Run run = new Run_Run(m_module);
                run.m_nPump = m_nPump;
                run.m_open = new NozzleSet.Open(m_open);
                run.m_sample = new ParticleCounterBase.Sample(m_sample); 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nPump = tree.Set(m_nPump, m_nPump, "Pump", "Regulator Pump Power (0 ~ 1000)");
                m_sample.RunTree(tree.GetTree("Sample")); 
                m_open.RunTree(tree.GetTree("Nozzle")); 
            }

            public override string Run()
            {
                return m_module.RunCount(this); 
            }
        }
        #endregion
    }
}
