using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Module;
using RootTools.ParticleCounter;
using RootTools.ToolBoxs;
using RootTools.Trees;
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
            toolBox.Get(ref m_particleCounter, m_module, "LasAir3");
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
                toolBox.GetDIO(ref m_diBackFlow, m_module, "Back Flow");
                toolBox.GetComm(ref m_rs232, m_module, "Regulator");
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

            ModuleBase m_module;
            public Regulator(ModuleBase module)
            {
                m_module = module;
            }
        }
        Regulator m_regulator;
        #endregion

        #region FlowSensor
        FlowSensor m_flowSensor;
        int m_nUnitFlowSensor = 1;

        public string ReadFlowSensor(ref double fFlow)
        {
            return m_flowSensor.Read(m_nUnitFlowSensor, ref fFlow); 
        }

        public void RunTreeFlowSensor(Tree tree)
        {
            m_nUnitFlowSensor = tree.Set(m_nUnitFlowSensor, m_nUnitFlowSensor, "Unit", "Flow Sensor Modbus Unit ID"); 
        }
        #endregion

        #region Run
        public class RunCount
        {
            public string Run()
            {
                try
                {
                    if (Run(m_set.m_nozzleSet.RunNozzle(m_aOpen))) return m_sInfo;
                    if (Run(m_set.m_regulator.RunPump(m_hPa))) return m_sInfo;
                    if (Run(m_set.m_particleCounter.StartRun(m_sample))) return m_sInfo;
                    bool bBackFlow = false;
                    double fFlow = 0;
                    List<double> m_aFlow = new List<double>(); 
                    while (m_set.m_particleCounter.IsBusy())
                    {
                        Thread.Sleep(100);
                        bBackFlow |= m_set.m_regulator.IsBackFlow();
                        if (Run(m_set.ReadFlowSensor(ref fFlow))) return m_sInfo;
                        m_aFlow.Add(fFlow); 
                        if (EQ.IsStop()) return "EQ Stop";
                    }
                    if (bBackFlow) return "BackFlow Sensor Check";
                    if (Run(m_set.m_regulator.RunPump(0))) return m_sInfo;
                    fFlow = 0;
                    foreach (double value in m_aFlow) fFlow += value;
                    if (m_aFlow.Count > 0) fFlow /= m_aFlow.Count; 
                    //forget Result ??
                    return "OK";
                }
                finally
                {
                    m_set.m_regulator.RunPump(0);
                    m_set.m_nozzleSet.RunCloseAllNozzle(); 
                }
            }

            string m_sInfo = "";
            bool Run(string sInfo)
            {
                if (EQ.IsStop()) sInfo = "EQ Stop";
                m_sInfo = sInfo;
                return sInfo != "OK";
            }

/*
void SaveResult(string sFile, string sTime, bool bBackFlow)
{
    using (StreamWriter sw = new StreamWriter(sFile, true, Encoding.Default))
    {
        sw.Write(sTime);
        foreach (int nCount in m_particleCounter.m_aCount) sw.Write("\t" + nCount.ToString());
        sw.WriteLine("\t" + (bBackFlow ? "BackFlow" : "OK"));
    }
}
*/

            public void RunTree(Tree tree, bool bVisible)
            {
                RunTreeNozzle(tree.GetTree("Nozzle Open", true, bVisible), bVisible);
                m_hPa = tree.GetTree("Regulator", true, bVisible).Set(m_hPa, m_hPa, "Pressure", "Regulator Pressure (hPa)", bVisible); 
            }

            void RunTreeNozzle(Tree tree, bool bVisible)
            {
                for (int n = 0; n < m_aOpen.Count; n++)
                {
                    m_aOpen[n] = tree.Set(m_aOpen[n], m_aOpen[n], n.ToString("00"), "Nozzle Open", bVisible); 
                }
            }

            List<bool> m_aOpen;
            double m_hPa = 3;
            ParticleCounterSet m_set;
            ParticleCounterBase.Sample m_sample;
            public RunCount(ParticleCounterSet particleCounterSet)
            {
                m_set = particleCounterSet;
                m_aOpen = particleCounterSet.m_nozzleSet.GetCloneOpen();
                m_sample = particleCounterSet.m_sample; 
            }
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
        NozzleSet m_nozzleSet;
        ParticleCounterBase.Sample m_sample; 
        public ParticleCounterSet(ModuleBase module, FlowSensor flowSensor, ParticleCounterBase.Sample sample, string sID = "")
        {
            m_module = module;
            m_regulator = new Regulator(m_module);
            m_nozzleSet = new NozzleSet(m_module);
            m_flowSensor = flowSensor;
            m_sample = sample; 
            m_sID = sID; 
        }

        public void ThreadStop()
        {
        }

        #region ModuleRun
        public void InitModuleRuns()
        {
            m_module.AddModuleRunList(new Run_Pump(this), false, "Run Pump");
            m_module.AddModuleRunList(new Run_ReadFlow(this), false, "Read Flow Sensor");
            m_module.AddModuleRunList(new Run_ParticleCount(this), true, "Run Particle Counter");
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
                m_runCount = new RunCount(particleCounterSet);
                InitModuleRun(particleCounterSet.m_module, m_particleCounterSet.m_sID);
            }

            RunCount m_runCount; 
            public override ModuleRunBase Clone()
            {
                Run_ParticleCount run = new Run_ParticleCount(m_particleCounterSet);
                run.m_runCount = new RunCount(m_particleCounterSet); 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_runCount.RunTree(tree, bVisible); 
            }

            public override string Run()
            {
                return m_runCount.Run();
            }
        }
        #endregion
    }
}
