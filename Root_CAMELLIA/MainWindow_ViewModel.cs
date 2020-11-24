﻿using Root_CAMELLIA.Data;
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
        public Dlg_RecipeManager_ViewModel RecipeViewModel;
        public DataManager DataManager;
        public MainWindow_ViewModel(MainWindow mainwindow)
        {
            m_MainWindow = mainwindow;
            Init();
            DialogInit(mainwindow);
        }

        private void Init()
        {
            DataManager = new DataManager();            
        }
        private void DialogInit(MainWindow main)
        {
            dialogService = new DialogService(main);
            dialogService.Register<Dlg_RecipeManager_ViewModel, Dlg_RecipeManger>();
            //dialogService.Register<Dlg_RecipeManager_ViewModel, Dlg_Engineer>();
        }

        #region Property
        public ObservableCollection<UIElement> p_MainStage
        {
            get
            {
                return m_MainStage;
            }
            set
            {
                SetProperty(ref m_MainStage, value);
            }
        }
        private ObservableCollection<UIElement> m_MainStage = new ObservableCollection<UIElement>();
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
        public ICommand CmdRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var viewModel = new Dlg_RecipeManager_ViewModel(this);
                    viewModel.StageChanged += ViewModel_StageChanged1;
                    
                    Nullable<bool> result = dialogService.ShowDialog(viewModel);
                });
            }
        }

        private void ViewModel_StageChanged1(object e)
        {
            p_MainStage = e as ObservableCollection<UIElement>;

        }

        private void RecipeViewModel_StageChanged(object e)
        {
            p_MainStage = e as ObservableCollection<UIElement>;
        }

        public ICommand CmdEngineer
        {
            get
            {
                return new RelayCommand(() =>
                {

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