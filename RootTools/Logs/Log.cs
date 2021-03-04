using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;

namespace RootTools
{
    public class Log
    {
        #region Data
        public enum eLevel
        {
            Info,
            Warn,
            Error, 
            Fatal
        };

        public class Data
        {
            public string m_sDate; 
            public string p_sTime { get; set; }
            public eLevel p_eLevel { get; set; }
            public string p_sLogger { get; set; }
            public string p_sMessage { get; set; }
            public string p_sStackTrace { get; set; }
            public Brush p_sColor { get; set; }

            public string p_sLog
            {
                get { return p_sTime + "\t" + p_eLevel.ToString() + "\t" + p_sLogger + "\t" + p_sMessage + "\t" + p_sStackTrace; }
            }

            public Data(string id, eLevel eLevel, string sMessage)
            {
                DateTime dt = DateTime.Now;
                m_sDate = dt.ToShortDateString();
                p_sTime = dt.Hour.ToString("00") + '.' + dt.Minute.ToString("00") + '.' + dt.Second.ToString("00") + '.' + dt.Millisecond.ToString("000"); 
                p_eLevel = eLevel;
                p_sLogger = id;
                p_sMessage = sMessage.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
                StackTrace st = new StackTrace(true);
                int n = Math.Min(st.FrameCount, 6);
                p_sStackTrace = GetStackString(st.GetFrame(n--));
                while (n > 3) p_sStackTrace += " => " + GetStackString(st.GetFrame(n--));
                switch (eLevel)
                {
                    case eLevel.Fatal:
                    case eLevel.Error: p_sColor = Brushes.Yellow; break;
                    case eLevel.Warn: p_sColor = Brushes.GreenYellow; break;
                    default: p_sColor = Brushes.White; break;
                }
            }

            string GetStackString(StackFrame stackFrame)
            {
                MethodBase mb = stackFrame.GetMethod();
                return mb.DeclaringType.Name + "." + mb.Name;
            }
        }

        #endregion 

        #region Write
        void Add(eLevel eLevel, string sMessage)
        {
            Data data = new Data(p_id, eLevel, sMessage);
            foreach (LogGroup group in m_aLogGroup) group.AddData(data); 
        }

        public void Info(string str)
        {
            Add(eLevel.Info, str); 
        }

        public void Warn(string str)
        {
            Add(eLevel.Warn, str);
        }

        public void Error(string str)
        {
            EQ.p_eState = EQ.eState.Error;
            EQ.p_bStop = true;
            Add(eLevel.Error, str);
        }

        public void Error(string str, string sArgument)
        {
            EQ.p_eState = EQ.eState.Error;
            EQ.p_bStop = true;
            Add(eLevel.Error, str + " : " + sArgument);
        }

        public void Error(Exception e, string str)
        {
            EQ.p_eState = EQ.eState.Error;
            EQ.p_bStop = true;
            Add(eLevel.Error, str + " : " + e.Message);
        }

        public void Fatal(string str)
        {
            EQ.p_eState = EQ.eState.Error;
            EQ.p_bStop = true;
            Add(eLevel.Fatal, str);
        }
        #endregion

        public string p_id { get; set; }
        public List<LogGroup> m_aLogGroup;
        public Log(string id, params LogGroup[] aLogGroup)
        {
            p_id = id;
            m_aLogGroup = new List<LogGroup>(); 
            foreach (LogGroup group in aLogGroup) m_aLogGroup.Add(group); 
        }
    }
}
