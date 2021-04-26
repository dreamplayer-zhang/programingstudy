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

        #region NozzleSet
        NozzleSet m_nozzleSet;
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
                    while (m_set.m_particleCounter.IsBusy())
                    {
                        Thread.Sleep(100);
                        bBackFlow |= m_set.m_regulator.IsBackFlow();
                        if (EQ.IsStop()) return "EQ Stop";
                    }
                    if (bBackFlow) return "BackFlow Sensor Check";
                    if (Run(m_set.m_regulator.RunPump(0))) return m_sInfo;
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

            public void RunTree(Tree tree)
            {
                RunTreeNozzle(tree.GetTree("Nozzle Open")); 
                m_hPa = tree.GetTree("Regulator").Set(m_hPa, m_hPa, "Pressure", "Regulator Pressure (hPa)"); 
                m_sample.RunTree(tree.GetTree("Sampling")); 
            }

            void RunTreeNozzle(Tree tree)
            {
                for (int n = 0; n < m_aOpen.Count; n++)
                {
                    m_aOpen[n] = tree.Set(m_aOpen[n], m_aOpen[n], n.ToString("00"), "Nozzle Open"); 
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
                m_sample = particleCounterSet.m_particleCounter.m_sample.Clone(); 
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

        ModuleBase m_module; 
        public ParticleCounterSet(ModuleBase module, FlowSensor flowSensor)
        {
            m_module = module;
            m_regulator = new Regulator(m_module);
            m_nozzleSet = new NozzleSet(m_module);
            m_flowSensor = flowSensor; 
        }
    }
}
