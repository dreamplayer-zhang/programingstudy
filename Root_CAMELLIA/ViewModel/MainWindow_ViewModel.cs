using Met = LibSR_Met;
using Root_CAMELLIA.Data;
using RootTools;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Root_CAMELLIA
{
    public class MainWindow_ViewModel : ObservableObject
    {
        private MainWindow m_MainWindow;
        DialogService dialogService;
        public DataManager DataManager;
        public Met.Nanoview NanoView;

        public MainWindow_ViewModel(MainWindow mainwindow)
        {
            m_MainWindow = mainwindow;

            Init();
            ViewModelInit();
            DialogInit(mainwindow);
        }
        private void Init()
        {
            DataManager = DataManager.Instance;
            NanoView = new Met.Nanoview();
            //NanoView.InitializeSR(@".\Reference", 2000); //(FilePath, PortNum)

            App.m_engineer.Init("Camellia");
            ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_camellia.Nanoview = NanoView;
            ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_camellia.mwvm = this;
        }

        private void ViewModelInit()
        {
            EngineerViewModel = new Dlg_Engineer_ViewModel(this);
            PMViewModel = new Dlg_PM_ViewModel(this);
            RecipeViewModel = new Dlg_RecipeManager_ViewModel(this);
        }

        private void DialogInit(MainWindow main)
        {
            dialogService = new DialogService(main);
            dialogService.Register<Dlg_Engineer_ViewModel, Dlg_Engineer>();
            dialogService.Register<Dlg_PM_ViewModel, Dlg_PM>();
            dialogService.Register<Dlg_RecipeManager_ViewModel, Dlg_RecipeManager>();
        }

        #region ViewModel
        public Dlg_PM_ViewModel PMViewModel;
        public Dlg_Engineer_ViewModel EngineerViewModel;
        public Dlg_RecipeManager_ViewModel RecipeViewModel
        {
            get
            {
                return _RecipeViewModel;
            }
            set
            {
                SetProperty(ref _RecipeViewModel, value);
            }
        }
        private Dlg_RecipeManager_ViewModel _RecipeViewModel;
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }
        private void M_timer_Tick(object sender, EventArgs e)
        {
            //tbTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        #endregion

        #region ICommand
        public ICommand CmdPM
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var viewModel = PMViewModel;
                    Nullable<bool> result = dialogService.ShowDialog(viewModel);
                });
            }
        }

        public ICommand CmdLoad
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeViewModel.dataManager.recipeDM.RecipeOpen();
                    RecipeViewModel.UpdateListView(true);
                    RecipeViewModel.UpdateView(true);
                });
            }
        }
        public ICommand CmdRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var viewModel = new Dlg_RecipeManager_ViewModel(this);
                    viewModel.dataManager = RecipeViewModel.dataManager;
                    viewModel.UpdateListView(true);
                    viewModel.UpdateView(true);
                    Nullable<bool> result = dialogService.ShowDialog(viewModel);

                    RecipeViewModel.UpdateListView(true);
                    RecipeViewModel.UpdateView(true);
                });
            }
        }
        public ICommand CmdEngineer
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var viewModel = EngineerViewModel;
                    var dialog = dialogService.GetDialog(viewModel) as Dlg_Engineer;
                    dialog.EngineerUI.Init(App.m_engineer);
                    Nullable<bool> result = dialog.ShowDialog();
                });
            }
        }
        public ICommand CmdSetting
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //m_Vision.StartRun(p_RunLADS);
                });
            }
        }
        public ICommand CmdExit
        {
            get
            {
                return new RelayCommand(() =>
                {               
                    m_MainWindow.Close();
                    App.m_engineer.ThreadStop();
                });
            }
        }
        #endregion

    }
}