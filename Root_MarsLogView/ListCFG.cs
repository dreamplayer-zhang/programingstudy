using RootTools;
using RootTools.Trees;
using System.Collections.ObjectModel;

namespace Root_MarsLogView
{
    public class ListCFG : NotifyProperty
    {
        public ObservableCollection<Mars_CFG> p_aCFGView { get; set; }

        string[] m_asLog;
        public void Add(string sLog, string[] asLog)
        {
            m_asLog = asLog;
            if (asLog.Length < 9) m_mars.AddError("PRC Length", sLog);
            m_mars.WriteEvent(sLog);
            Mars_CFG cfg = new Mars_CFG(asLog);
            p_aCFGView.Add(cfg);
            if (p_aCFGView.Count > m_maxView) p_aCFGView.RemoveAt(0);
        }

        int m_maxView = 250;
        public void RunTree(Tree tree)
        {
            m_maxView = tree.Set(m_maxView, m_maxView, "Max List", "PRC Max List View Count");
        }

        MarsLogViewer m_mars;
        public ListCFG(MarsLogViewer mars)
        {
            m_mars = mars;
            p_aCFGView = new ObservableCollection<Mars_CFG>();
        }
    }
}
