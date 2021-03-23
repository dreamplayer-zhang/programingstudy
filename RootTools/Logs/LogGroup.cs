using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace RootTools
{
    public class LogGroup
    {
        const int c_lLog = 500; 

        #region Proterty
        public string p_id { get; set; }

        public UserControl p_ui
        {
            get
            {
                LogGroup_UI ui = new LogGroup_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        #region Data
        public ConcurrentQueue<Log.Data> m_qLog = new ConcurrentQueue<Log.Data>();
        public void AddData(Log.Data data)
        {
            if (m_eLevelMin > data.p_eLevel) return;
            m_qLog.Enqueue(data); 
        }

        public void TimerSave()
        {
            while (m_qLog.Count > 0) LogSave(); 
        }

        void LogSave()
        {
            string sPath = LogView._logView.p_sPath;
            try
            {
                if (m_qLog.Count <= 0) return;
                //string sDate;
                Log.Data LogData;
                if(m_qLog.TryPeek(out LogData))
                {
                    string sDate = LogData.m_sDate;
                    sPath += "\\" + sDate;
                    Directory.CreateDirectory(sPath);
                    using (StreamWriter writer = new StreamWriter(sPath + "\\" + sDate + "_" + p_id + ".txt", true, Encoding.Default))
                    {
                        while (m_qLog.Count > 0)
                        {
                            Log.Data data;
                            if(m_qLog.TryPeek(out data))
                            {
                                if (data != null)
                                {
                                    Log.Data dequeue;
                                    if (data.m_sDate != sDate) return;
                                    if (m_qLog.TryDequeue(out dequeue))
                                    {
                                        writer.WriteLine(data.p_sLog);
                                        p_aLog.Add(data);
                                        while (p_aLog.Count > c_lLog) p_aLog.RemoveAt(0);
                                    }
                                }
                                else
                                {
                                    Log.Data dequeue;
                                    m_qLog.TryDequeue(out dequeue);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        public ObservableCollection<Log.Data> p_aLog { get; set; }
        #endregion

        Log.eLevel m_eLevelMin;              // 최소 레벨
        public LogGroup(string sGroup, Log.eLevel eLevelMin)
        {
            p_aLog = new ObservableCollection<Log.Data>();
            m_eLevelMin = eLevelMin; 
            p_id = sGroup;
        }
    }
}
