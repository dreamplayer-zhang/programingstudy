using RootTools;
using RootTools.Trees;
using System.Collections.ObjectModel;

namespace Root_MarsLogView
{
    public class ListCFG : NotifyProperty
    {
        public ObservableCollection<Mars_CFG> p_aCFGView { get; set; }
        public ObservableCollection<Mars_CFG> p_aCFG { get; set; }

        Mars_CFG Get(string[] asLog)
        {
            foreach (Mars_CFG cfg in p_aCFG)
            {
                if (cfg.IsSame(asLog)) return cfg;
            }
            return null;
        }

        string[] m_asLog;
        //public void Add(string sLog, string[] asLog)
        //{
        //    m_asLog = asLog;
        //    if (asLog.Length < 9) m_mars.AddError("PRC Length", sLog);
        //    m_mars.WriteEvent(sLog);
        //    Mars_CFG cfg = new Mars_CFG(sLog, asLog);
        //    p_aCFGView.Add(cfg);
        //    if (p_aCFGView.Count > m_maxView) p_aCFGView.RemoveAt(0);
        //}
        public void Add(int iTCP, string sLog, string[] asLog)
        {
            m_asLog = asLog;
            if (asLog.Length < 10) m_mars.AddError("PRC Length", sLog);
            Mars_CFG cfg = Get(asLog);
            if (cfg != null)
            {
                m_mars.AddError("CFG Not Ended", cfg.m_sLog);
                p_aCFG.Remove(cfg);
            }
            m_mars.WriteEvent(sLog);
            cfg = new Mars_CFG(iTCP, sLog, asLog);
            p_aCFG.Add(cfg);
            p_aCFGView.Add(cfg);
            if (p_aCFGView.Count > m_maxView) p_aCFGView.RemoveAt(0);
        }

        public void Reset(int iTCP, string sDate, string sTime)
        {
            foreach (Mars_CFG fnc in p_aCFG)
            {
                if (fnc.m_iTCP == iTCP)
                {
                    m_mars.AddError("FNC Reset", fnc.m_sLog);
                    string[] asLog = fnc.m_asLog;
                    asLog[0] = sDate;
                    asLog[1] = sTime;
                }
            }
            for (int n = p_aCFG.Count - 1; n >= 0; n--)
            {
                if (p_aCFG[n].m_iTCP == iTCP) p_aCFG.RemoveAt(n);
            }
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
            p_aCFG = new ObservableCollection<Mars_CFG>();
            p_aCFGView = new ObservableCollection<Mars_CFG>();
        }
        public void ThreadStop(string sDate, string sTime)
        {
            foreach (Mars_CFG fnc in p_aCFG)
            {
                m_mars.AddError("FNC ThreadStop", fnc.m_sLog);
                string[] asLog = fnc.m_asLog;
                asLog[0] = sDate;
                asLog[1] = sTime;
            }
        }
    }
}
