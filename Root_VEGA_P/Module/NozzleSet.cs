using RootTools;
using RootTools.Control;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System.Collections.Generic;

namespace Root_VEGA_P.Module
{
    public class NozzleSet : NotifyProperty
    {
        #region ToolBox
        public string GetTools(ToolBox toolBox)
        {
            for (int n = 0; n < m_aNozzle.Count; n++)
            {
                toolBox.GetDIO(ref m_aNozzle[n].m_do, m_particleCounter, m_aNozzle[n].p_id);
            }
            return "OK";
        }
        #endregion

        #region Nozzle
        public class Nozzle
        {
            public DIO_O m_do = null;
            public string p_id { get; set; }

            public void Write(bool bOpen)
            {
                m_do?.Write(bOpen);
            }

            public Nozzle(string id)
            {
                p_id = id;
            }
        }
        List<Nozzle> m_aNozzle = new List<Nozzle>();
        #endregion

        #region Nozzle Open
        public class Open
        {
            public List<bool> m_aOpen = new List<bool>();
            public int p_nNozzle
            {
                get { return m_aOpen.Count; }
                set
                {
                    if (m_aOpen.Count == value) return;
                    while (m_aOpen.Count > value) m_aOpen.RemoveAt(m_aOpen.Count - 1);
                    while (m_aOpen.Count < value) m_aOpen.Add(false); 
                }
            }

            public void RunTree(Tree tree)
            {
                for (int n = 0; n < p_nNozzle; n++)
                {
                    m_aOpen[n] = tree.Set(m_aOpen[n], m_aOpen[n], m_nozzleSet.m_aNozzle[n].p_id, "Nozzle Open");
                }
            }

            NozzleSet m_nozzleSet; 
            public Open(NozzleSet nozzleSet)
            {
                m_nozzleSet = nozzleSet; 
            }

            public Open(Open open)
            {
                m_nozzleSet = open.m_nozzleSet;
                p_nNozzle = open.p_nNozzle;
                for (int n = 0; n < p_nNozzle; n++) m_aOpen[n] = open.m_aOpen[n];
            }
        }
        public Open m_open; 
        void InitOpen()
        {
            m_open = new Open(this); 
        }
        #endregion

        #region Property
        public int p_nNozzle
        {
            get { return m_aNozzle.Count; }
            set
            {
                if (m_aNozzle.Count == value) return;
                while (m_aNozzle.Count > value) m_aNozzle.RemoveAt(m_aNozzle.Count - 1);
                while (m_aNozzle.Count < value) m_aNozzle.Add(new Nozzle("Nozzle" + m_aNozzle.Count.ToString("00")));
            }
        }
        #endregion

        #region Nozzle Run
        public string RunNozzle(Open open)
        {
            for (int n = 0; n < p_nNozzle; n++) m_aNozzle[n].Write(open.m_aOpen[n]); 
            return "OK";
        }

        public string RunNozzle(int nNozzle)
        {
            for (int n = 0; n < p_nNozzle; n++) m_aNozzle[n].Write(n == nNozzle);
            return "OK";
        }
        #endregion

        #region Tree
        public void RunTreeName(Tree tree)
        {
            p_nNozzle = tree.Set(p_nNozzle, p_nNozzle, "Count", "Nozzle Count");
        }
        #endregion

        #region Init Nozzle
        Registry m_reg;
        void InitNozzle(string id)
        {
            m_reg = new Registry(id);
            p_nNozzle = m_reg.Read("nNozzle", 1);
        }
        #endregion

        ParticleCounter m_particleCounter;
        public NozzleSet(ParticleCounter particleCounter)
        {
            m_particleCounter = particleCounter;
            InitNozzle(particleCounter.p_id);
            InitOpen(); 
        }
    }
}
