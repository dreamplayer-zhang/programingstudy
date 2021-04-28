using System.Windows;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    class MainVM:ObservableObject
    {
        public MainWindow mainWindow;
        public SelectMode selectMode;
        public WindowState mainWindowState;

        public MainVM(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            mainWindowState = mainWindow.WindowState;
        }
        public MainVM(SelectMode selectMode)
        {
            this.selectMode = selectMode;
        }

        public WindowState MWindowState
        {
            get => mainWindowState;
            set => SetProperty(ref mainWindowState, value);
        }
        #region [Relay Command]
        public ICommand ModeSelectCommand
        {
            get => new RelayCommand(() => UIManager.Instance.ChangeMainUI(UIManager.Instance.ModeWindow));
        }
        public ICommand ChangeUISetupCommand
        {
            get => new RelayCommand(() => UIManager.Instance.ChangeUISetup());
        }
        public ICommand ChangeUIReviewCommand
        {
            get => new RelayCommand(() => UIManager.Instance.ChangeUIReview());
        }
        public ICommand ChangeUIRunCommand
        {
            get => new RelayCommand(() => UIManager.Instance.ChangeUIRun());
        }
        public ICommand WindowLoadedCommand
        {
            get => new RelayCommand(() =>mainWindow.Window_Loaded());
        }
        public ICommand WindowClosingCommand
        {
            get => new RelayCommand(() => mainWindow.Window_Closing());
        }
        public ICommand MenuItemExitCommand
        {
            get => new RelayCommand(() =>
            {
                mainWindow.Close();
                Application.Current.Shutdown();
            });
        }
        #endregion
    }
}
