﻿using RootTools;
using RootTools.Trees;
using System.Collections.ObjectModel;

namespace Root_MarsLogView
{
    public class ListXFR : NotifyProperty
    {
        public ObservableCollection<Mars_XFR> p_aXFR { get; set; }
        public ObservableCollection<Mars_XFR> p_aXFRView { get; set; }
        Mars_XFR Get(string[] asLog)
        {
            foreach (Mars_XFR xfr in p_aXFR)
            {
                if (xfr.IsSame(asLog)) return xfr;
            }
            return null;
        }

        string[] m_asLog;
        public void Add(string sLog, string[] asLog)
        {
            m_asLog = asLog;
            if (asLog.Length < 14) m_mars.AddError("XFR Length", sLog);
            string sStatus = GetString(5);
            Mars_XFR xfr = Get(asLog);
            if (sStatus == Mars_XFR.eStatus.Start.ToString())
            {
                if (xfr != null)
                {
                    m_mars.AddError("XFR Not Ended", sLog);
                    m_mars.WriteEvent(xfr.GetEndLog(asLog));
                    p_aXFR.Remove(xfr);
                }
                m_mars.WriteEvent(sLog);
                xfr = new Mars_XFR(asLog);
                p_aXFR.Add(xfr);
                p_aXFRView.Add(xfr);
                if (p_aXFRView.Count > m_maxView) p_aXFRView.RemoveAt(0);
            }
            else if (sStatus == Mars_XFR.eStatus.End.ToString())
            {
                if (xfr != null)
                {
                    xfr.End(asLog);
                    m_mars.WriteEvent(sLog);
                    p_aXFR.Remove(xfr);
                }
                else m_mars.AddError("XFR Not Started", sLog);
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
            m_maxView = tree.Set(m_maxView, m_maxView, "Max List", "XFR Max List View Count");
        }

        MarsLogViewer m_mars;
        public ListXFR(MarsLogViewer mars)
        {
            m_mars = mars;
            p_aXFR = new ObservableCollection<Mars_XFR>();
            p_aXFRView = new ObservableCollection<Mars_XFR>();
        }
    }
}