using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Module;
using RootTools.ParticleCounter;
using RootTools.ToolBoxs;
using RootTools.Trees;
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
                toolBox.GetDIO(ref m_diBackFlow, m_module, "BackFlow");
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

        #region Tree
        public void RunTree(Tree tree)
        {
            m_nozzleSet.RunTreeSetup(tree.GetTree("NozzleSet"));
            //RunTreeFlow(tree.GetTree("Flow Sensor"));
            //RunTreeLasair(tree.GetTree("Lasair"));
        }
        #endregion


        ModuleBase m_module; 
        public ParticleCounterSet(ModuleBase module)
        {
            m_module = module;
            m_regulator = new Regulator(m_module);
            m_nozzleSet = new NozzleSet(m_module);
        }
    }
}
