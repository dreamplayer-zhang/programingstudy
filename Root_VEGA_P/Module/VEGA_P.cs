using RootTools;
using RootTools.Comm;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;

namespace Root_VEGA_P.Module
{
    public class VEGA_P :ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_flowSensor.GetTools(this); 
            if (bInit)
            {
            }
        }
        #endregion

        #region Flow Sensor
        public class FlowSensor : NotifyProperty
        {
            Modbus m_modbus; 
            public string GetTools(VEGA_P vegaP)
            {
                return vegaP.m_toolBox.GetComm(ref m_modbus, vegaP, "Modbus"); 
            }

            public class Channel
            {
                Modbus m_modbus;
                List<int> m_aRead = new List<int>(); 
                public string Read()
                {
                    m_modbus.ReadHoldingRegister((byte)p_nUnit, 0, m_aRead); 
                    for (int n = 0; n < 0x28; n+=4)
                    {
                        string sLog = "";
                        for (int i = 0; i < 4; i++) sLog += m_aRead[n + i].ToString() + "\t";
                        m_vegaP.m_log.Info(sLog); 
                    }
                    return "OK"; 
                }

                public string p_id { get; set; }
                int p_nUnit { get; set; }
                VEGA_P m_vegaP; 
                public Channel(string id, int nUnit, VEGA_P vegaP)
                {
                    p_nUnit = nUnit; 
                    p_id = id + "." + nUnit.ToString("00");
                    m_vegaP = vegaP; 
                    m_modbus = vegaP.m_flowSensor.m_modbus;
                    for (int n = 0; n <= 0x28; n++) m_aRead.Add(0); 
                }
            }
            public List<Channel> m_aChannel = new List<Channel>();
            public int p_nChannel
            {
                get { return m_aChannel.Count; }
                set
                {
                    if (m_aChannel.Count == value) return;
                    while (m_aChannel.Count > value) m_aChannel.RemoveAt(m_aChannel.Count - 1);
                    while (m_aChannel.Count < value) m_aChannel.Add(new Channel(p_id, m_aChannel.Count + 1, m_vegaP));
                    OnPropertyChanged(); 
                }
            }

            public string Read(int nChannel)
            {
                if (nChannel < 0) return "Invalid Channel";
                if (nChannel >= m_aChannel.Count) return "Invalid Channel";
                return m_aChannel[nChannel].Read(); 
            }

            public void RunTree(Tree tree)
            {
                p_nChannel = tree.Set(p_nChannel, p_nChannel, "Channel", "Flow Sensor Channel Count"); 
            }

            public string p_id { get; set; }
            VEGA_P m_vegaP;
            public FlowSensor(string id, VEGA_P vegaP)
            {
                p_id = id;
                m_vegaP = vegaP; 
            }
        }
        public FlowSensor m_flowSensor; 
        void InitFlowSensor()
        {
            m_flowSensor = new FlowSensor("FlowSensor", this);
        }
        #endregion

        public VEGA_P(string id, IEngineer engineer)
        {
            InitFlowSensor(); 
            InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
}
