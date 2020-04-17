using RootTools.Trees;
using System.Collections.ObjectModel;

namespace RootTools.DMC
{
    public class DMCListDIO
    {
        public string p_sHeader { get; set; }

        public ObservableCollection<DMCDIO> m_aDIO = new ObservableCollection<DMCDIO>();

        public int p_lDIO
        { 
            get { return m_aDIO.Count; }
            set
            {
                while (m_aDIO.Count < value) m_aDIO.Add(new DMCDIO(m_aDIO.Count, p_sHeader));
                if (value > 0) while (m_aDIO.Count > value) m_aDIO.RemoveAt(m_aDIO.Count - 1); 
            }
        }

        public void RunTree(Tree tree)
        {
            p_lDIO = tree.Set(p_lDIO, p_lDIO, "Count", "DIO Count");
            Tree treeDIO = tree.GetTree("DIO ID", false);
            foreach (DMCDIO dio in m_aDIO) dio.RunTree(treeDIO); 
        }

        public DMCListDIO(string sHeader)
        {
            p_sHeader = sHeader; 
        }
    }
}
