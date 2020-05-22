using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
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

        #region Data
        List<Data> m_aLog = new List<Data>(); 
        public class Data
        {
            public string p_sTime { get; set; }
            public string p_sLevel { get; set; }
            public string p_sLogger { get; set; }
            public string p_sMessage { get; set; }
            public string p_sStackTrace { get; set; }
            public Brush p_sColor { get; set; } 

            public Data(string sLog)
            {
                string[] asLog = sLog.Split('\t');
                if (asLog.Length > 0) p_sTime = asLog[0];
                if (asLog.Length > 1) p_sLevel = asLog[1];
                if (asLog.Length > 2) p_sLogger = asLog[2];
                if (asLog.Length > 3) p_sMessage = asLog[3];
                if (asLog.Length > 4) p_sStackTrace = asLog[4];
                switch (p_sLevel)
                {
                    case "Fatal":
                    case "Error": p_sColor = Brushes.Yellow; break;
                    case "Warn": p_sColor = Brushes.GreenYellow; break;
                    default: p_sColor = Brushes.White; break;
                }
            }
        }
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
        public string m_sFilterTime = "";
        public string m_sFilterLogger = "";
        public string m_sFilterMessage = "";
        public string m_sFilterStackTrace = "";

        public ObservableCollection<Data> p_aLogFilter { get; set; }
        public void InvalidFilter()
        {
            p_aLogFilter.Clear(); 
            foreach (Data data in m_aLog)
            {
                if (IsVisible(data)) p_aLogFilter.Add(data); 
            }
        }

        bool IsVisible(Data data)
        {
            if (IsVisible(m_sFilterTime, data.p_sTime) == false) return false;
            if (IsVisible(m_sFilterLogger, data.p_sLogger) == false) return false;
            if (IsVisible(m_sFilterMessage, data.p_sMessage) == false) return false;
            if (IsVisible(m_sFilterStackTrace, data.p_sStackTrace) == false) return false;
            return true; 
        }

        bool IsVisible(string sFilter, string sData)
        {
            if (sFilter == "") return true;
            return sData.Contains(sFilter); 
        }
        #endregion

        public LogGroup()
        {
            p_aLogFilter = new ObservableCollection<Data>(); 
        }
    }
}
