using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Module;
using RootTools.ParticleCounter;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Root_VEGA_P.Module
{
    public class ParticleCounter : ModuleBase
    {
        #region ToolBox
        LasairIII m_particleCounter; 
        public override void GetTools(bool bInit)
        {
            m_regulator.GetTools(m_toolBox, bInit);
            m_nozzleSet.GetTools(m_toolBox);
            p_sInfo = m_toolBox.Get(ref m_particleCounter, this, "LasAir3"); 
            if (bInit) { }
        }
        #endregion

        #region Regulator
        public class Regulator : NotifyProperty
        {
            DIO_I m_diBackFlow; 
            RS232 m_rs232; 
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                toolBox.GetDIO(ref m_diBackFlow, m_particleCounter, "BackFlow"); 
                toolBox.GetComm(ref m_rs232, m_particleCounter, "Regulator");
                if (bInit) m_rs232.p_bConnect = true;
            }

            public bool IsBackFlow()
            {
                return m_diBackFlow.p_bIn; 
            }

            public double m_hPa = 0; 
            public string RunPump(double hPa)
            {
                string sInfo = ConnectRS232();
                if (sInfo != "OK") return sInfo;
                if (hPa < 0) hPa = 0;
                if (hPa > 5) hPa = 5; 
                m_hPa = hPa;
                int nPower = (int)(200 * hPa); 
                m_rs232.Send("SET " + nPower.ToString());
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
        FlowSensor m_flowSensor;
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
            /*
            if (Run(m_nozzleSet.RunNozzle(run.m_open))) return p_sInfo;
            if (Run(m_regulator.RunPump(run.m_nPump))) return p_sInfo;
            if (Run(m_lasair.StartRun(run.m_sample))) return p_sInfo;
            while (m_lasair.IsBusy())
            {
                Thread.Sleep(100);
                if (m_lasair.IsTimeout()) return "Particle Counter Run Timeout";
                if (EQ.IsStop()) return "EQ Stop";
            }
            */

            string sPath = "c:\\ParticleCountResult";
            Directory.CreateDirectory(sPath);

            int iNozzle = 0;
            while (EQ.IsStop() == false)
            {
                if (Run(m_nozzleSet.RunNozzle(iNozzle))) return p_sInfo;
                if (Run(m_regulator.RunPump(run.m_hPa))) return p_sInfo;
                if (Run(m_particleCounter.StartRun(run.m_sample))) return p_sInfo;
                bool bBackFlow = false; 
                while (m_particleCounter.IsBusy())
                {
                    Thread.Sleep(100);
                    bBackFlow |= m_regulator.IsBackFlow(); 
                    if (EQ.IsStop()) return "EQ Stop";
                }
                if (Run(m_regulator.RunPump(0))) return p_sInfo;
                string sTime = GetTime(); 
                SaveResult(sPath + "\\Nozzle" + iNozzle.ToString("00") + ".txt", sTime, bBackFlow);
                iNozzle = (iNozzle + 1) % m_nozzleSet.p_nNozzle;
                Thread.Sleep(5000); 
            }
            return "OK"; 
        }

        void SaveResult(string sFile, string sTime, bool bBackFlow)
        {
            using (StreamWriter sw = new StreamWriter(sFile, true, Encoding.Default))
            {
                sw.Write(sTime);
                foreach (int nCount in m_particleCounter.m_aCount) sw.Write("\t" + nCount.ToString());
                sw.WriteLine("\t" + (bBackFlow ? "BackFlow" : "OK"));
            }
        }

        string GetTime()
        {
            DateTime dt = DateTime.Now;
            return dt.Day.ToString() + '.' + dt.Hour.ToString("00") + '.' + dt.Minute.ToString("00") + '.' + dt.Second.ToString("00");
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            if (Run(base.StateHome())) return p_sInfo;
            return "OK";
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_nozzleSet.RunTreeSetup(tree.GetTree("NozzleSet"));
            RunTreeFlow(tree.GetTree("Flow Sensor"));
            RunTreeLasair(tree.GetTree("Lasair"));
        }
        #endregion

        public ParticleCounter(string id, IEngineer engineer, FlowSensor flowSensor)
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
                run.m_hPa = m_hPa; 
                run.m_secDelay = m_secDelay;
                return run;
            }

            double m_hPa = 2; 
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_hPa = tree.Set(m_hPa, m_hPa, "hPa", "Pump Power (1 ~ 5 hPa)", bVisible); 
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Delay Time (sec)", bVisible);
            }

            public override string Run()
            {
                if (m_module.Run(m_module.m_regulator.RunPump(m_hPa))) return p_sInfo; 
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
                m_aOpen = m_module.m_nozzleSet.GetCloneOpen(); 
                m_sample = new ParticleCounterBase.Sample(); 
                InitModuleRun(module);
            }

            public double m_hPa = 2;
            public List<bool> m_aOpen;
            public ParticleCounterBase.Sample m_sample; 
            public override ModuleRunBase Clone()
            {
                Run_Run run = new Run_Run(m_module);
                run.m_hPa = m_hPa;
                for (int n = 0; n < m_aOpen.Count; n++) run.m_aOpen[n] = m_aOpen[n]; 
                run.m_sample = m_sample.Clone(); 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_hPa = tree.Set(m_hPa, m_hPa, "hPa", "Pump Power (1 ~ 5 hPa)", bVisible);
                m_sample.RunTree(tree.GetTree("Sample"), bVisible);
                m_module.m_nozzleSet.RunTreeOpen(tree.GetTree("Nozzle"), m_aOpen); 
            }

            public override string Run()
            {
                return m_module.RunCount(this); 
            }
        }
        #endregion
    }
}
