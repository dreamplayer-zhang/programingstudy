using Microsoft.Win32;
using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Root_LogView
{
    public class LogGroup : NotifyProperty
    {
        public LogGroup_UI p_ui
        {
            get
            {
                LogGroup_UI ui = new LogGroup_UI();
                ui.Init(this);
                return ui; 
            }
        }

        public enum eLevel
        {
            Info,
            Warn,
            Error,
            Fatal,
        }

        #region Data
        public class Data
        {
            public string p_sTime { get; set; }
            public eLevel p_eLevel { get; set; }
            public string p_sLogger { get; set; }
            public string p_sMessage { get; set; }
            public string p_sStackTrace { get; set; }
            public Brush p_sColor { get; set; }

            public bool m_bSame = false;
            public bool IsSame(Data data)
            {
                if (p_sTime != data.p_sTime) return false;
                if (p_eLevel != data.p_eLevel) return false;
                if (p_sLogger != data.p_sLogger) return false;
                if (p_sMessage != data.p_sMessage) return false;
                if (p_sStackTrace != data.p_sStackTrace) return false;
                m_bSame = true; 
                return true; 
            }

            public Data(string sLog)
            {
                string[] asLog = sLog.Split('\t');
                if (asLog.Length > 0) p_sTime = asLog[0];
                if (asLog.Length > 2) p_sLogger = asLog[2];
                if (asLog.Length > 3) p_sMessage = asLog[3];
                if (asLog.Length > 4) p_sStackTrace = asLog[4];
                switch ((asLog.Length > 1) ? asLog[1] : "")
                {
                    case "Fatal":
                        p_eLevel = eLevel.Fatal;
                        p_sColor = Brushes.Yellow;
                        break;
                    case "Error":
                        p_eLevel = eLevel.Error;
                        p_sColor = Brushes.Yellow; 
                        break;
                    case "Warn":
                        p_eLevel = eLevel.Warn;
                        p_sColor = Brushes.GreenYellow;
                        break;
                    default:
                        p_eLevel = eLevel.Info;
                        p_sColor = Brushes.White; 
                        break;
                }
            }
        }
        List<Data> m_aLog = new List<Data>();
        Stack<List<Data>> m_stackLog = new Stack<List<Data>>(); 
        #endregion

        #region FileOpen
        public string OpenLog(string sFile)
        {
            FileStream fs = null;
            StreamReader sr = null;
            try
            {
                fs = new FileStream(sFile, FileMode.Open);
                sr = new StreamReader(fs, Encoding.Default);
                string sLine = sr.ReadLine();
                while (sLine != null)
                {
                    CalcDataGrid(sLine);
                    sLine = sr.ReadLine();
                }
                return "OK";
            }
            catch (Exception e)
            {
                return sFile + " OpenLog Error " + e.Message;
            }
            finally
            {
                if (sr != null) sr.Dispose();
                InvalidFilter(); 
            }
        }

        public void CalcDataGrid(string sLog)
        {
            Data data = new Data(sLog);
            m_aLog.Add(data); 
        }
        #endregion

        #region Filter
        public class Filter
        {
            public string m_sTime = "";
            public eLevel m_eLevel = eLevel.Info; 
            public string m_sLogger = "";
            public string m_sMessage = "";
            public string m_sStackTrace = "";

            public bool IsVisible(Data data)
            {
                if (IsVisible(m_sTime, data.p_sTime) == false) return false;
                if (IsVisible(m_sLogger, data.p_sLogger) == false) return false;
                if (IsVisible(m_sMessage, data.p_sMessage) == false) return false;
                if (IsVisible(m_sStackTrace, data.p_sStackTrace) == false) return false;
                if ((int)m_eLevel > (int)data.p_eLevel) return false; 
                return true;
            }

            bool IsVisible(string sFilter, string sData)
            {
                if (sData == null) return false; 
                if (sFilter == "") return true;
                return sData.Contains(sFilter);
            }
        }
        public Filter m_filter = new Filter(); 

        public ObservableCollection<Data> p_aLogFilter { get; set; }
        public void InvalidFilter(Filter filter = null)
        {
            if (filter == null) filter = m_filter; 
            p_aLogFilter.Clear(); 
            foreach (Data data in m_aLog)
            {
                if (filter.IsVisible(data)) p_aLogFilter.Add(data); 
            }
        }
        #endregion

        #region Clip
        public void StackLog()
        {
            if (m_logClip == null) return; 
            List<Data> aLog = new List<Data>();
            foreach (Data data in m_logClip.m_aLog) aLog.Add(data);
            m_logClip.m_stackLog.Push(aLog); 
        }

        public void Undo()
        {
            if (m_stackLog.Count <= 0) return; 
            m_aLog = m_stackLog.Pop();
            InvalidFilter(); 
        }

        public void SendClip(System.Collections.IList aData)
        {
            if (m_logClip == null) return;
            foreach (Data data in aData) m_logClip.CheckSame(data);
            foreach (Data data in aData) if (data.m_bSame == false) m_logClip.m_aLog.Add(data); 
            m_logClip.InvalidFilter();
        }

        public void SendClip()
        {
            if (m_logClip == null) return;
            foreach (Data data in p_aLogFilter) m_logClip.CheckSame(data);
            foreach (Data data in p_aLogFilter) if (data.m_bSame == false) m_logClip.m_aLog.Add(data);
            m_logClip.InvalidFilter();
        }

        void CheckSame(Data data)
        {
            Parallel.ForEach(m_aLog, dat =>
            {
                if (data.m_bSame == false) data.IsSame(dat); 
            }); 
        }

        public void RemoveSelection(System.Collections.IList aData)
        {
            foreach (Data data in aData) m_aLog.Remove(data);
            InvalidFilter(); 
        }

        public void RemoveAll()
        {
            m_aLog.Clear();
            InvalidFilter();
        }

        public void FileSave(System.Collections.IList aData, bool bSimple = false)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = bSimple ? "Log Text Files (*.txt)|*.txt" : "Log Files (*.log)|*.log";
            if (dlg.ShowDialog() == false) return;
            try
            {
                StreamWriter sw = new StreamWriter(new FileStream(dlg.FileName, FileMode.Create));
                if (bSimple)
                {
                    int maxLogger = 0; 
                    foreach (Data data in aData)
                    {
                        if (maxLogger < data.p_sLogger.Length) maxLogger = data.p_sLogger.Length; 
                    }
                    maxLogger++; 
                    foreach (Data data in aData)
                    {
                        sw.Write(data.p_sTime + " ");
                        string sLogger = data.p_sLogger; 
                        for (int i = 0; i < maxLogger - data.p_sLogger.Length; i++) sLogger += ' ';
                        sw.Write(sLogger);
                        sw.WriteLine(data.p_sMessage);
                    }
                }
                else
                {
                    foreach (Data data in aData)
                    {
                        sw.Write(data.p_sTime + "\t");
                        sw.Write(data.p_eLevel.ToString() + "\t");
                        sw.Write(data.p_sLogger + "\t");
                        sw.Write(data.p_sMessage + "\t");
                        sw.WriteLine(data.p_sStackTrace);
                    }
                }
                sw.Close();
            }
            catch (Exception) { }

        }
        #endregion

        public LogViewer m_logViewer; 
        public LogGroup m_logClip = null; 
        public LogGroup(LogViewer logViewer, LogGroup logClip)
        {
            m_logViewer = logViewer; 
            m_logClip = logClip; 
            p_aLogFilter = new ObservableCollection<Data>(); 
        }
    }
}
