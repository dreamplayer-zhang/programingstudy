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
        public void Add(int iTCP, string sLog, string[] asLog)
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
                    fnc.End(asLog); 
                    m_mars.WriteEvent(fnc.GetEndLog(asLog));
                    p_aFNC.Remove(fnc);
                }
                m_mars.WriteEvent(sLog);
                fnc = new Mars_FNC(iTCP, sLog, asLog);
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
            string sLog = m_asLog[nIndex];
            if (sLog[sLog.Length - 1] == '\'') sLog = sLog.Substring(0, sLog.Length - 1);
            if (sLog[0] == '\'') sLog = sLog.Substring(1, sLog.Length - 1);
            return sLog;
        }

        public void Reset(int iTCP, string sDate, string sTime)
        {
            foreach (Mars_FNC fnc in p_aFNC)
            {
                if (fnc.m_iTCP == iTCP)
                {
                    m_mars.AddError("FNC Reset", fnc.m_sLog);
                    string[] asLog = fnc.m_asLog;
                    asLog[0] = sDate;
                    asLog[1] = sTime;
                    fnc.End(asLog);
                    m_mars.WriteEvent(fnc.GetEndLog(asLog));
                }
            }
            for (int n = p_aFNC.Count - 1; n >= 0; n--)
            {
                if (p_aFNC[n].m_iTCP == iTCP) p_aFNC.RemoveAt(n);
            }
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

        public void ThreadStop(string sDate, string sTime)
        {
            foreach (Mars_FNC fnc in p_aFNC)
            {
                m_mars.AddError("FNC ThreadStop", fnc.m_sLog);
                string[] asLog = fnc.m_asLog;
                asLog[0] = sDate;
                asLog[1] = sTime;
                fnc.End(asLog);
                m_mars.WriteEvent(fnc.GetEndLog(asLog));
            }
        }
    }
}
