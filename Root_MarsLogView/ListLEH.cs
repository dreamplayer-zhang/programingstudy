using RootTools;
using RootTools.Trees;
using System.Collections.ObjectModel;

namespace Root_MarsLogView
{
    public class ListLEH : NotifyProperty
    {
        public ObservableCollection<Mars_LEH> p_aLEH { get; set; }
        public ObservableCollection<Mars_LEH> p_aLEHView { get; set; }
        Mars_LEH Get(string[] asLog)
        {
            foreach (Mars_LEH leh in p_aLEH)
            {
                if (leh.IsSame(asLog)) return leh;
            }
            return null;
        }

        string[] m_asLog;
        public void Add(string sLog, string[] asLog)
        {
            m_asLog = asLog;
            if (asLog.Length < 9) m_mars.AddError("LEH Length", sLog);
            Mars_LEH.eEvent eEvent = Mars_LEH.GetEvent(GetString(4));
            Mars_LEH leh = Get(asLog);
            if (eEvent == Mars_LEH.eEvent.CarrierLoad)
            {
                if (leh != null)
                {
                    m_mars.AddError("LEH Not CarrierUnloaded", sLog);
                    SetEvent(leh, Mars_LEH.eEvent.CarrierUnload, sLog, asLog);
                    p_aLEH.Remove(leh);
                }
                m_mars.WriteEvent(sLog);
                leh = new Mars_LEH(sLog, asLog);
                p_aLEH.Add(leh);
                p_aLEHView.Add(leh);
                if (p_aLEHView.Count > m_maxView) p_aLEHView.RemoveAt(0);
            }
            else
            {
                if (leh != null)
                {
                    SetEvent(leh, eEvent, sLog, asLog);
                    if (eEvent == Mars_LEH.eEvent.CarrierUnload) p_aLEH.Remove(leh);
                }
                else m_mars.AddError("LEH Not CarrierLoaded", sLog);
            }
        }

        void SetEvent(Mars_LEH leh, Mars_LEH.eEvent eEvent, string sLog, string[] asLog)
        {
            int dEvent = (int)eEvent - (int)leh.p_eEvent;
            if (dEvent < 1)
            {
                m_mars.AddError("LEH Invalid Event", sLog);
                return; 
            }
            while (leh.p_eEvent < eEvent) m_mars.WriteEvent(leh.GetEndLog(leh.p_eEvent + 1, asLog));
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
            m_maxView = tree.Set(m_maxView, m_maxView, "Max List", "LEH Max List View Count");
        }

        MarsLogViewer m_mars;
        public ListLEH(MarsLogViewer mars)
        {
            m_mars = mars;
            p_aLEH = new ObservableCollection<Mars_LEH>();
            p_aLEHView = new ObservableCollection<Mars_LEH>();
        }

        public void ThreadStop(string sDate, string sTime)
        {
            foreach (Mars_LEH leh in p_aLEH)
            {
                m_mars.AddError("LEH ThreadStop", leh.m_sLog);
                string[] asLog = leh.m_asLog;
                asLog[0] = sDate;
                asLog[1] = sTime;
                SetEvent(leh, Mars_LEH.eEvent.CarrierUnload, leh.m_sLog, asLog);
            }
        }
    }
}
