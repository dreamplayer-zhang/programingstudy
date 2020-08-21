using RootTools;
using RootTools.Trees;
using System.Collections.ObjectModel;

namespace Root_MarsLogView
{
    public class ListPRC : NotifyProperty
    {
        public ObservableCollection<Mars_PRC> p_aPRC { get; set; }
        public ObservableCollection<Mars_PRC> p_aPRCView { get; set; }
        Mars_PRC Get(string[] asLog)
        {
            foreach (Mars_PRC prc in p_aPRC)
            {
                if (prc.IsSame(asLog)) return prc; 
            }
            return null; 
        }

        string[] m_asLog; 
        public void Add(string sLog, string[] asLog)
        {
            m_asLog = asLog; 
            if (asLog.Length < 14) m_mars.AddError("PRC Length", sLog);
            string sStatus = GetString(5);
            Mars_PRC prc = Get(asLog);
            if (sStatus == Mars_PRC.eStatus.Start.ToString())
            {
                if (prc != null)
                {
                    m_mars.AddError("PRC Not Ended", prc.m_sLog);
                    prc.End(asLog);
                    m_mars.WriteEvent(prc.GetEndLog(asLog));
                    p_aPRC.Remove(prc);
                }
                m_mars.WriteEvent(sLog);
                prc = new Mars_PRC(sLog, asLog);
                p_aPRC.Add(prc);
                p_aPRCView.Add(prc);
                if (p_aPRCView.Count > m_maxView) p_aPRCView.RemoveAt(0); 
            }
            else if (sStatus == Mars_PRC.eStatus.End.ToString())
            {
                if (prc != null)
                {
                    prc.End(asLog);
                    m_mars.WriteEvent(sLog);
                    p_aPRC.Remove(prc);
                }
                else m_mars.AddError("PRC Not Started", sLog);
            }
        }

        string GetString(int nIndex)
        {
            if (m_asLog.Length <= nIndex) return "";
            string sLog = m_asLog[nIndex]; 
            if (sLog[sLog.Length - 1] == '\'') sLog = sLog.Substring(0, sLog.Length - 1);
            if (sLog[0] == '\'') sLog = sLog.Substring(1, sLog.Length - 1); 
            return sLog; 
        }

        int m_maxView = 250; 
        public void RunTree(Tree tree)
        {
            m_maxView = tree.Set(m_maxView, m_maxView, "Max List", "PRC Max List View Count"); 
        }

        MarsLogViewer m_mars; 
        public ListPRC(MarsLogViewer mars)
        {
            m_mars = mars; 
            p_aPRC = new ObservableCollection<Mars_PRC>();
            p_aPRCView = new ObservableCollection<Mars_PRC>();
        }

        public void ThreadStop(string sDate, string sTime)
        {
            foreach (Mars_PRC prc in p_aPRC)
            {
                m_mars.AddError("PRC ThreadStop", prc.m_sLog);
                string[] asLog = prc.m_asLog;
                asLog[0] = sDate;
                asLog[1] = sTime; 
                prc.End(asLog);
                m_mars.WriteEvent(prc.GetEndLog(asLog));
            }
        }
    }
}
