using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Module;
using RootTools.ParticleCounter;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_VEGA_P.Module
{
    public class ParticleCounterSet
    {
        #region ToolBox
        LasairIII m_particleCounter;
        public void GetTools(ToolBox toolBox, bool bInit)
        {
            m_regulator.GetTools(toolBox, bInit);
            m_nozzleSet.GetTools(toolBox);
            toolBox.Get(ref m_particleCounter, m_module, m_sID + "LasAir3");
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
                toolBox.GetDIO(ref m_diBackFlow, m_module, m_sID + "Back Flow");
                toolBox.GetComm(ref m_rs232, m_module, m_sID + "Regulator");
                if (bInit) m_rs232.p_bConnect = true;
            }

            public bool IsBackFlow()
            {
                return m_diBackFlow.p_bIn;
            }

            public string RunPump(double hPa)
            {
                string sInfo = ConnectRS232();
                if (sInfo != "OK") return sInfo;
                if (hPa < 0) hPa = 0;
                if (hPa > 5) hPa = 5;
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

            string m_sID; 
            ModuleBase m_module;
            public Regulator(ModuleBase module, string sID)
            {
                m_module = module;
                m_sID = sID; 
            }
        }
        Regulator m_regulator;
        #endregion

        #region FlowSensor
        int m_nUnitFlowSensor = 1;

        string ReadFlowSensor(ref double fFlow)
        {
            return m_vegaP.m_flowSensor.Read(m_nUnitFlowSensor, ref fFlow); 
        }

        double m_fFlow = 0;
        double m_fFlowSum = 0;
        int m_nFlowCount = 0; 
        public void ClearFlowSensorData()
        {
            m_fFlow = 0;
            m_fFlowSum = 0;
            m_nFlowCount = 0; 
        }

        public string ReadFlowSensor()
        {
            double fFlow = 0;
            string sRead = ReadFlowSensor(ref fFlow);
            m_fFlowSum += fFlow;
            m_nFlowCount++;
            m_fFlow = m_fFlowSum / m_nFlowCount;
            return sRead; 
        }

        public void RunTreeFlowSensor(Tree tree)
        {
            m_nUnitFlowSensor = tree.Set(m_nUnitFlowSensor, m_nUnitFlowSensor, "Unit", "Flow Sensor Modbus Unit ID"); 
        }
        #endregion

        #region Result
        [Serializable]
        public class Result
        {
            public DateTime m_dateTime = DateTime.Now;
            public bool m_bBackFlow = false;
            public double m_fFlow = 0;
            public List<string> m_asSize = new List<string>();
            public List<int> m_aCount = new List<int>(); 

            public Result(bool bBackFlow, double fFlow, LasairIII lasairIII)
            {
                m_bBackFlow = bBackFlow;
                m_fFlow = fFlow;
                foreach (string sSize in lasairIII.m_asParticleSize) m_asSize.Add(sSize);
                foreach (int nCount in lasairIII.m_aCount) m_aCount.Add(nCount); 
            }
            public bool Save()
            {
                return true;
                //bool res = true;
                //string PodInfoPath = App.RecipeRootPath
            }
        }
        public List<Result> m_aResult = new List<Result>(); 
        #endregion

        #region Run
        public string RunParticleCounter(List<string> asNozzle)
        {
            try
            {
                foreach (string sFile in asNozzle)
                {
                    if (m_nozzleSet.RunNozzle(sFile) == "OK")
                    {
                        if (Run(m_regulator.RunPump(m_nozzleSet.p_hPa))) return m_sInfo;
                        Thread.Sleep((int)(1000 * m_vegaP.m_secPumpDelay));
                        if (Run(m_particleCounter.StartRun(m_vegaP.m_sample))) return m_sInfo;
                        bool bBackFlow = false;
                        while (m_particleCounter.IsBusy())
                        {
                            Thread.Sleep(100);
                            if (EQ.IsStop()) return "EQ Stop";
                            bBackFlow |= m_regulator.IsBackFlow(); //forget
                            if (Run(ReadFlowSensor())) return m_sInfo;
                        }
                        Thread.Sleep((int)(1000 * m_vegaP.m_secPumpDelay));
                        Result result = new Result(bBackFlow, m_fFlow, m_particleCounter);
                        m_aResult.Add(result);
                        if (Run(m_regulator.RunPump(0))) return m_sInfo;
                        Thread.Sleep((int)(1000 * m_vegaP.m_secPumpDelay));
                    }
                }
                return "OK";
            }
            finally
            {
                m_regulator.RunPump(0);
                m_nozzleSet.RunCloseAllNozzle();
            }
        }

        string m_sInfo = "";
        bool Run(string sInfo)
        {
            if (EQ.IsStop()) sInfo = "EQ Stop";
            m_sInfo = sInfo;
            return sInfo != "OK";
        }
        #endregion

        #region Tree
        public void RunTree(Tree tree)
        {
            m_nozzleSet.RunTreeSetup(tree.GetTree("NozzleSet"));
            RunTreeFlowSensor(tree.GetTree("Flow Sensor"));
        }
        #endregion

        string m_sID = ""; 
        ModuleBase m_module;
        public NozzleSet m_nozzleSet;
        VEGA_P m_vegaP; 
        public ParticleCounterSet(ModuleBase module, VEGA_P vegaP, string sID = "")
        {
            m_module = module;
            m_regulator = new Regulator(m_module, sID);
            m_nozzleSet = new NozzleSet(m_module, sID);
            m_vegaP = vegaP; 
            m_sID = sID; 
        }

        public void ThreadStop()
        {
        }

        #region ModuleRun
        public void InitModuleRuns(bool bRecipe)
        {
            m_module.AddModuleRunList(new Run_Pump(this), false, "Run Pump");
            m_module.AddModuleRunList(new Run_ReadFlow(this), false, "Read Flow Sensor");
            m_module.AddModuleRunList(new Run_ParticleCount(this), bRecipe, "Run Particle Counter");
        }

        public class Run_Pump : ModuleRunBase
        {
            ParticleCounterSet m_particleCounterSet;
            public Run_Pump(ParticleCounterSet particleCounterSet)
            {
                m_particleCounterSet = particleCounterSet;
                InitModuleRun(particleCounterSet.m_module, m_particleCounterSet.m_sID);
            }

            int m_secDelay = 1;
            public override ModuleRunBase Clone()
            {
                Run_Pump run = new Run_Pump(m_particleCounterSet);
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
                m_particleCounterSet.m_regulator.RunPump(m_hPa);
                Thread.Sleep(1000 * m_secDelay);
                m_particleCounterSet.m_regulator.RunPump(0);
                return "OK";
            }
        }

        public class Run_ReadFlow : ModuleRunBase
        {
            ParticleCounterSet m_particleCounterSet;
            public Run_ReadFlow(ParticleCounterSet particleCounterSet)
            {
                m_particleCounterSet = particleCounterSet;
                InitModuleRun(particleCounterSet.m_module, m_particleCounterSet.m_sID);
            }

            public override ModuleRunBase Clone()
            {
                Run_ReadFlow run = new Run_ReadFlow(m_particleCounterSet);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                double fFlow = 0;
                try
                {
                    return m_particleCounterSet.ReadFlowSensor(ref fFlow);
                }
                finally
                {
                    m_particleCounterSet.m_module.m_log.Info("Read Flow Sensor : " + fFlow.ToString()); 
                }
            }
        }

        public class Run_ParticleCount : ModuleRunBase
        {
            ParticleCounterSet m_particleCounterSet;
            public Run_ParticleCount(ParticleCounterSet particleCounterSet)
            {
                m_particleCounterSet = particleCounterSet;
                InitModuleRun(particleCounterSet.m_module, m_particleCounterSet.m_sID);
            }

            public int m_nCount = 0;
            public List<string> m_asNozzle = new List<string>(); 
            public override ModuleRunBase Clone()
            {
                Run_ParticleCount run = new Run_ParticleCount(m_particleCounterSet);
                run.m_nCount = m_nCount;
                run.m_asNozzle.Clear();
                foreach (string sNozzle in m_asNozzle) run.m_asNozzle.Add(sNozzle); 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nCount = tree.Set(m_nCount, m_nCount, "Count", "Particle Count Repeat Count", bVisible);
                while (m_asNozzle.Count < m_nCount) m_asNozzle.Add("");
                while (m_asNozzle.Count > m_nCount) m_asNozzle.RemoveAt(m_asNozzle.Count - 1);
                RunTreeNozzle(tree.GetTree("NozzleSet", false, bVisible), bVisible);
            }

            void RunTreeNozzle(Tree tree, bool bVisible)
            {
                List<string> asFile = m_particleCounterSet.m_nozzleSet.p_asFile; 
                for (int n = 0; n < m_asNozzle.Count; n++)
                {
                    m_asNozzle[n] = tree.Set(m_asNozzle[n], m_asNozzle[n], asFile, (n + 1).ToString("00"), "NozzleSet Name", bVisible); 
                }
            }

            public override string Run()
            {
                return m_particleCounterSet.RunParticleCounter(m_asNozzle);
            }
        }
        #endregion
    }
}
