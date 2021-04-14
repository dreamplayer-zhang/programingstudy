using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Root_VEGA_P.Module
{
    public class ParticleCounterSet
    {
        /*
        #region ToolBox
        public void GetTools(bool bInit)
        {

        }
        #endregion

        #region Regulator
        public class Regulator : NotifyProperty
        {
            DIO_I m_diBackFlow;
            RS232 m_rs232;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                toolBox.GetDIO(ref m_diBackFlow, m_module, p_id + ".BackFlow");
                toolBox.GetComm(ref m_rs232, m_module, p_id +".Regulator");
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

            string p_id { get; set; }
            ModuleBase m_module;
            public Regulator(string id, ModuleBase module)
            {
                p_id = id;
                m_module = module;
            }
        }
        Regulator m_regulator;
        void InitRegulator()
        {
            m_regulator = new Regulator(p_id, m_module);
        }
        #endregion

        #region NozzleSet
        NozzleSet m_nozzleSet;
        void InitNozzleSet()
        {
            m_nozzleSet = new NozzleSet(m_module);
        }
        #endregion

*/
    }
}
