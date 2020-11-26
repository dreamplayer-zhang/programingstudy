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
        public DataManager DataManager;
        public Met.Nanoview NanoView { get; set; }
        public MainWindow_ViewModel(MainWindow mainwindow)
        {
            m_MainWindow = mainwindow;
            Init();
            DialogInit(mainwindow);
        }
        private void Init()
        {
            DataManager = DataManager.Instance;
            RecipeViewModel = new Dlg_RecipeManager_ViewModel(this);
            //NanoView = new Met.Nanoview();
        }

        private void DialogInit(MainWindow main)
        {
            dialogService = new DialogService(main);
            dialogService.Register<Dlg_RecipeManager_ViewModel, Dlg_RecipeManger>();
            //dialogService.Register<Dlg_RecipeManager_ViewModel, Dlg_Engineer>();
        }

        #region ViewModel
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

        #region Dialog
        DialogService dialogService;
        private Dlg_RecipeManger DlgRecipeManager;
        private Dlg_Engineer DlgEngineer;
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
                    //var viewModel = new Dlg_RecipeManager_ViewModel(this);
                    var viewModel = new Dlg_RecipeManager_ViewModel(this); ;
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
                   var asdf = RecipeViewModel.dataManager;
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
                });
            }
        }
        #endregion

        #region 이전코드
        //public MainWindow_ViewModel(MainWindow main)
        //{
        //    DataManager = main.DataManager;
        //    Init();
        //}

        //public void Init()
        //{
        //    PointListItem.Columns.Add(new DataColumn("ListIndex"));
        //    PointListItem.Columns.Add(new DataColumn("ListX"));
        //    PointListItem.Columns.Add(new DataColumn("ListY"));
        //    PointListItem.Columns.Add(new DataColumn("ListRoute"));
        //}

        //public void UpdateListView()
        //{
        //    PointListItem.Clear();
        //    int nCount = 0;
        //    int nSelCnt = DataManager.recipeDM.TeachingRD.DataSelectedPoint.Count;
        //    int[] MeasurementOrder = new int[nSelCnt];

        //    for (int i = 0; i < nSelCnt; i++)
        //    {
        //        MeasurementOrder[DataManager.recipeDM.TeachingRD.DataMeasurementRoute[i]] = i;
        //    }

        //    DataRow row;
        //    for (int i = 0; i < nSelCnt; i++, nCount++)
        //    {

        //        CCircle c = DataManager.recipeDM.TeachingRD.DataSelectedPoint[i];
        //        int nRoute = MeasurementOrder[i];
        //        row = PointListItem.NewRow();
        //        row["ListIndex"] = (nCount + 1).ToString();
        //        row["ListX"] = Math.Round(c.x, 3).ToString();
        //        row["ListY"] = Math.Round(c.y, 3).ToString();
        //        row["ListRoute"] = (nRoute + 1).ToString();
        //        PointListItem.Rows.Add(row);

        //    }
        //    PointCount = PointListItem.Rows.Count.ToString();
        //}

        //public DataManager DataManager { get; set; }

        //private ObservableCollection<UIElement> m_MainDrawElement = new ObservableCollection<UIElement>();
        //public ObservableCollection<UIElement> p_MainDrawElement
        //{
        //    get
        //    {
        //        return m_MainDrawElement;
        //    }
        //    set
        //    {
        //        m_MainDrawElement = value;
        //    }
        //}

        //public string PointCount { get; set; } = "0";

        //public ObservableCollection<ShapeManager> Shapes = new ObservableCollection<ShapeManager>();
        //public ObservableCollection<GeometryManager> Geometry = new ObservableCollection<GeometryManager>();

        //DataTable pointListItem = new DataTable();
        //public DataTable PointListItem
        //{
        //    get
        //    {
        //        return pointListItem;
        //    }
        //    set
        //    {
        //        pointListItem = value;
        //        RaisePropertyChanged("PointListItem");
        //    }
        //}
        #endregion


    }
}