using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    #region [Subclass]
    public enum LOG_MESSAGE_TYPE
    {
        Process = 0,
        Network,
    }

    public class LogEntry : ObservableObject
    {
        public int Index { get; set; }
        public DateTime DateTime { get; set; }

        public string Type { get; set; }

        public string Message { get; set; }
    }
    #endregion

    public class LogViewer_ViewModel : ObservableObject
    {

        public LogViewer_ViewModel()
        {
            WorkEventManager.AddLog += AddLog_Callback;
        }

        private void AddLog_Callback(object obj, LogArgs args)
        {
            Application.Current.Dispatcher.Invoke(new Action(()=>
            {
                LogEntries.Add(new LogEntry() { DateTime = DateTime.Now, Type = args.type.ToString(), Index = LogEntries.Count, Message = args.msg });

            }));
        }

        #region [Properties]
        private ObservableCollection<LogEntry> logEntries = new ObservableCollection<LogEntry>();
        public ObservableCollection<LogEntry> LogEntries
        {
            get => this.logEntries;
            set
            {
                SetProperty<ObservableCollection<LogEntry>>(ref this.logEntries, value);
            }
        }
        #endregion
    }
}
