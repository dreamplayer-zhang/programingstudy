using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_LogView
{
    public class LogViewer
    {
        public List<LogGroup> m_aLog = new List<LogGroup>();

        public LogGroup_UI OpenLog(string sFile)
        {
            LogGroup group = new LogGroup();
            m_aLog.Add(group);
            group.OpenLog(sFile);
            return group.p_ui; 
        }
    }
}
