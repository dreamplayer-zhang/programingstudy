using RootTools;
using RootTools.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RootTools_Vision
{
    public class MainWindow_ViewModel : ObservableObject
    {

        private DataListView_ViewModel dataListViewVM;
        public DataListView_ViewModel DataListViewVM
        {
            get => this.dataListViewVM;
            set
            {
                SetProperty(ref this.dataListViewVM, value);
            }
        }


        #region [ViewModels]
        CloneImageViewer_ViewModel imageViewerVM = new CloneImageViewer_ViewModel();
        public CloneImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
            set => this.imageViewerVM = value;
        }


        LogViewer_ViewModel logViewerVM = new LogViewer_ViewModel();
        public LogViewer_ViewModel LogViewerVM
        {
            get => this.logViewerVM;
            set => this.logViewerVM = value;
        }
        #endregion

        public MainWindow_ViewModel()
        {
            CameraInfo camInfo = new CameraInfo();
            this.DataListViewVM = new DataListView_ViewModel();
            this.dataListViewVM.Init(camInfo);


            ///
            Settings setting = new Settings();
            SettingItem_Database frontSettings = setting.GetItem<SettingItem_Database>();

            DatabaseManager.Instance.SetDatabase(1, frontSettings.SerevrName, frontSettings.DBName, frontSettings.DBUserID, frontSettings.DBPassword);

            Initialize();
            Logger.AddMsg(LOG_MESSAGE_TYPE.Process, "Start Program");
        }

        public void Initialize()
        {
            this.ConnectionTimer = new Timer(TimerConnection, null, 1000, 3000);
        }

        private Timer ConnectionTimer;
        private bool isTryingConnection = false;

        private void TimerConnection(Object obj)
        {
            if (this.remoteInspetcionManager.IsConnected == false && this.isTryingConnection == false)
            {
                isTryingConnection = true;
                Logger.AddMsg(LOG_MESSAGE_TYPE.Network, "Try Connect...");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    remoteInspetcionManager.TryConnect();
                });
                isTryingConnection = false;
            }
        }

        private ClonableWorkFactory remoteInspetcionManager = new ClonableWorkFactory();

        #region [Command]
        public ICommand WindowClosingCommand
        {
            get => new RelayCommand(() =>
            {
                remoteInspetcionManager.Exit();
                remoteInspetcionManager.Clear();
                Application.Current.Shutdown();
            });
        }
        #endregion

    }
}
