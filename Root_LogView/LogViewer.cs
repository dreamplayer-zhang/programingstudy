using System;
using System.Collections.Generic;
namespace Root_LogView
{
    public class LogViewer
    {
        #region Log
        public List<LogGroup> m_aLog = new List<LogGroup>();

        public LogGroup_UI OpenLog(string sFile)
        {
            LogGroup group = new LogGroup(this, m_logClip);
            m_aLog.Add(group);
            group.OpenLog(sFile);
            return group.p_ui; 
        }
        #endregion

        #region Clip
        public LogGroup m_logClip = null;

        public void SendClip(LogGroup.Filter filter)
        {
            foreach (LogGroup group in m_aLog)
            {
                group.InvalidFilter(filter);
                group.SendClip();
            }
        }
        #endregion

        public LogViewer()
        {
            m_logClip = new LogGroup(this, null);
        }
    }
}
