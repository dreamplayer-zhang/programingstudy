using RootTools;
using RootTools.Trees;
using System.Collections.ObjectModel;

namespace Root_MarsLogView
{
    public class ListFNC : NotifyProperty
    {
        public ObservableCollection<Mars_FNC> p_aFNC { get; set; }
        public ObservableCollection<Mars_FNC> p_aFNCView { get; set; }
        Mars_FNC Get(string[] asLog)
        {
            foreach (Mars_FNC fnc in p_aFNC)
            {
                if (fnc.IsSame(asLog)) return fnc;
            }
            return null;
        }

        string[] m_asLog;
        public void Add(string sLog, string[] asLog)
        {
            m_asLog = asLog;
            if (asLog.Length < 8) m_mars.AddError("FNC Length", sLog);
            string sStatus = GetString(5);
            Mars_FNC fnc = Get(asLog);
            if (sStatus == Mars_FNC.eStatus.Start.ToString())
            {
                if (fnc != null)
                {
                    m_mars.AddError("FNC Not Ended", sLog);
                    m_mars.WriteEvent(fnc.GetEndLog(asLog));
                    p_aFNC.Remove(fnc);
                }
                m_mars.WriteEvent(sLog);
                fnc = new Mars_FNC(asLog);
                p_aFNC.Add(fnc);
                p_aFNCView.Add(fnc);
                if (p_aFNCView.Count > m_maxView) p_aFNCView.RemoveAt(0);
            }
            else if (sStatus == Mars_FNC.eStatus.End.ToString())
            {
                if (fnc != null)
                {
                    fnc.End(asLog);
                    m_mars.WriteEvent(sLog);
                    p_aFNC.Remove(fnc);
                }
                else m_mars.AddError("FNC Not Started", sLog);
            }
        }

        string GetString(int nIndex)
        {
            if (m_asLog.Length <= nIndex) return "";
            return m_asLog[nIndex];
        }

        int m_maxView = 250;
        public void RunTree(Tree tree)
        {
            m_maxView = tree.Set(m_maxView, m_maxView, "Max List", "FNC Max List View Count");
        }

        MarsLogViewer m_mars;
        public ListFNC(MarsLogViewer mars)
        {
            m_mars = mars;
            p_aFNC = new ObservableCollection<Mars_FNC>();
            p_aFNCView = new ObservableCollection<Mars_FNC>();
        }
    }
}
