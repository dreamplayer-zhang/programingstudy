using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_ProcessMediator
{
    class MainWindow_ViewModel : ObservableObject
    {
        private ObservableCollection<LogItem> logItems = new ObservableCollection<LogItem>();

        public ObservableCollection<LogItem> LogItems
        {
            get => this.logItems;
            set
            {
                SetProperty<ObservableCollection<LogItem>>(ref this.logItems, value);
            }
        }

        public bool IsServer { get; set; }
        public bool IsClient { get; set; }


        #region [Command]
        public RelayCommand btnConnectCommand
        {
            get => new RelayCommand(() =>
            {
                string serverName = "test";
                string clientName = "test";
                if(this.IsServer)
                {
                    PipeServer server = new PipeServer(serverName);
                }
                else
                {
                    PipeClient client = new PipeClient(clientName, serverName);
                }
            });
        }

        #endregion

        public MainWindow_ViewModel()
        {
            logItems.Add(new LogItem() { Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), Name = "test", Message = " Fuck yuou" });
        }
    }

    public class LogItem
    {
        public string Time
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }
    }
}
