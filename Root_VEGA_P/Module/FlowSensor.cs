using RootTools;
using RootTools.Comm;
using RootTools.Trees;
using System.Collections.Generic;

namespace Root_VEGA_P.Module
{
    public class FlowSensor : NotifyProperty
    {
        public Modbus m_modbus;
        public string GetTools(VEGA_P vegaP)
        {
            return vegaP.m_toolBox.GetComm(ref m_modbus, vegaP, "Modbus");
        }

        public class Channel
        {
            Modbus m_modbus;
            public string Read(ref double fFlow)
            {
                int nFlow = 0;
                m_modbus.ReadHoldingRegister((byte)p_nUnit, 5, ref nFlow);
                fFlow = nFlow / 10.0;
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

        public string Read(int nChannel, ref double fFlow)
        {
            if (nChannel < 0) return "Invalid Channel";
            if (nChannel > m_aChannel.Count) return "Invalid Channel";
            return m_aChannel[nChannel - 1].Read(ref fFlow);
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
}
