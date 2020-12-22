﻿using Met = LibSR_Met;
using Root_CAMELLIA.Data;
using RootTools;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Root_CAMELLIA.Module;
using System.Drawing;
using Root_CAMELLIA.Draw;
using System.Windows.Media;

namespace Root_CAMELLIA
{
    public class MainWindow_ViewModel : ObservableObject
    {
        private MainWindow m_MainWindow;
        public DataManager DataManager;
        public Met.Nanoview NanoView;

        #region Property
        public Module_Camellia p_Module_Camellia
        {
            get
            {
                return m_Module_Camellia;
            }
            set
            {
                SetProperty(ref m_Module_Camellia, value);
            }
        }
        private Module_Camellia m_Module_Camellia;

        public Module_Camellia.Run_Measure p_Run_Measure
        {
            get
            {
                return m_Run_Measure;
            }
            set
            {
                SetProperty(ref m_Run_Measure, value);
            }
        }
        private Module_Camellia.Run_Measure m_Run_Measure;

        Met.RTGraph m_RTGraph = new Met.RTGraph();
        public Met.RTGraph p_RTGraph
        {
            get
            {
                return m_RTGraph;
            }
            set
            {
                SetProperty(ref m_RTGraph, value);
            }
        }

        RPoint m_StageCenterPulse = new RPoint();
        public RPoint p_StageCenterPulse
        {
            get
            {
                return m_StageCenterPulse;
            }
            set
            {
                SetProperty(ref m_StageCenterPulse, value);
            }
        }

        public MainWindow_ViewModel(MainWindow mainwindow)
        {
            m_MainWindow = mainwindow;

            Init();
            ViewModelInit();
            DialogInit(mainwindow);

            Module_Camellia.Run_Measure measure = (Module_Camellia.Run_Measure)p_Module_Camellia.CloneModuleRun("Measure");
            this.p_StageCenterPulse = measure.m_StageCenterPos_pulse;

			//테스트하였는데 뭔가 문제있음. 바로 Initialize하면 컨버터에 오류발생
            App.m_nanoView.InitializeSR(@"C:\Users\ATI\Desktop\ATI_LIB_v18_6(backup20201208)\Init_ATI_NIR.cfg", 2);
        }

        public double p_ArrowX1
        {
            get
            {
                return m_ArrowX1;
            }
            set
            {
                SetProperty(ref m_ArrowX1, value);
            }
        }
        private double m_ArrowX1 = 0.0f;

        public double p_ArrowX2
        {
            get
            {
                return m_ArrowX2;
            }
            set
            {
                SetProperty(ref m_ArrowX2, value);
                if (p_ArrowX1 < p_ArrowX2)
                {
                    m_GradientStartTempPoint.X = 0;
                }
                else if (p_ArrowX1 > p_ArrowX2)
                {
                    m_GradientStartTempPoint.X = 1;
                }
                else
                {
                    m_GradientStartTempPoint.X = 0.5;
                }

                if (p_ArrowX1 < p_ArrowX2)
                {
                    m_GradientEndTempPoint.X = 1;
                }
                else if (p_ArrowX1 > p_ArrowX2)
                {
                    m_GradientEndTempPoint.X = 0;
                }
                else
                {
                    m_GradientEndTempPoint.X = 0.5;
                }
            }
        }
        private double m_ArrowX2 = 0.0f;

        public double p_ArrowY1
        {
            get
            {
                return m_ArrowY1;
            }
            set
            {
                SetProperty(ref m_ArrowY1, value);
            }
        }
        private double m_ArrowY1 = 0.0f;

        public double p_ArrowY2
        {
            get
            {
                return m_ArrowY2;
            }
            set
            {
                SetProperty(ref m_ArrowY2, value);
                if (p_ArrowY1 < p_ArrowY2)
                {
                    m_GradientStartTempPoint.Y = 0;
                }
                else if (p_ArrowY1 > p_ArrowY2)
                {
                    m_GradientStartTempPoint.Y = 1;
                }
                else
                {
                    m_GradientStartTempPoint.Y = 0.5;
                }

                if (p_ArrowY1 < p_ArrowY2)
                {
                    m_GradientEndTempPoint.Y = 1;
                }
                else if (p_ArrowY1 > p_ArrowY2)
                {
                    m_GradientEndTempPoint.Y = 0;
                }
                else
                {
                    m_GradientEndTempPoint.Y = 0.5;
                }

                p_GradientStartPoint = m_GradientStartTempPoint;
                p_GradientEndPoint = m_GradientEndTempPoint;
            }
        }
        private double m_ArrowY2 = 0.0f;

        public Visibility p_ArrowVisible
        {
            get
            {
                return m_ArrowVisible;
            }
            set
            {
                SetProperty(ref m_ArrowVisible, value);
            }
        }
        private Visibility m_ArrowVisible = Visibility.Hidden;

        private System.Windows.Point m_GradientStartTempPoint = new System.Windows.Point();
        private System.Windows.Point m_GradientEndTempPoint = new System.Windows.Point();

        private System.Windows.Point m_GradientStartPoint = new System.Windows.Point();
        public System.Windows.Point p_GradientStartPoint
        {
            get
            {
                return m_GradientStartPoint;
            }
            set
            {
                SetProperty(ref m_GradientStartPoint, value);
            }
        }

        private System.Windows.Point m_GradientEndPoint = new System.Windows.Point();
        public System.Windows.Point p_GradientEndPoint
        {
            get
            {
                return m_GradientEndPoint;
            }
            set
            {
                SetProperty(ref m_GradientEndPoint, value);
            }
        }

        public ObservableCollection<UIElement> p_DrawRouteElement
        {
            get
            {
                return m_DrawRouteElement;
            }
            set
            {
                m_DrawRouteElement = value;
            }
        }
        private ObservableCollection<UIElement> m_DrawRouteElement = new ObservableCollection<UIElement>();

        public double p_Progress
        {
            get
            {
                return m_Progress;
            }
            set
            {
                SetProperty(ref m_Progress, value);
                if(value < 30)
                {
                    p_ProgressColor = System.Windows.Media.Brushes.Red;
                }
                else if (value < 65)
                {
                    p_ProgressColor = System.Windows.Media.Brushes.Yellow;
                }else if(value < 90)
                {
                    p_ProgressColor = test;
                }
                else if(value == 100)
                {
                    p_ProgressColor = System.Windows.Media.Brushes.Blue;
                }
            }
        }
        private double m_Progress = 0;

        public System.Windows.Media.Brush p_ProgressColor
        {
            get
            {
                return m_ProgressColor;
            }
            set
            {
                SetProperty(ref m_ProgressColor, value);
            }
        }
        private System.Windows.Media.Brush m_ProgressColor;

        public SolidColorBrush RouteBrush { get; set; } = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 255));
        public SolidColorBrush test = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 1, 211, 40));
        #endregion

        private void Init()
        {
            DataManager = DataManager.Instance;
            //App.m_nanoView.InitializeSR();

            //NanoView = new Met.Nanoview();
            

            App.m_engineer.Init("Camellia");
            ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_camellia.mwvm = this;

            p_Module_Camellia = ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_camellia;

        }

        private void ViewModelInit()
        {
            EngineerViewModel = new Dlg_Engineer_ViewModel(this);
            RecipeViewModel = new Dlg_RecipeManager_ViewModel(this);   
            SettingViewModel = new Dlg_Setting_ViewModel(this);
            PMViewModel = new Dlg_PM_ViewModel(this);
        }

        private void DialogInit(MainWindow main)
        {
            dialogService = new DialogService(main);
            dialogService.Register<Dlg_Engineer_ViewModel, Dlg_Engineer>();
            dialogService.Register<Dlg_RecipeManager_ViewModel, Dlg_RecipeManager>();
            dialogService.Register<Dlg_Setting_ViewModel, Dlg_Setting>();
            dialogService.Register<Dlg_PM_ViewModel, Dlg_PM>();
        }

        private void DrawMeasureRoute()
        {
            p_DrawRouteElement.Clear();
            int RatioX = (int)BaseDefine.CanvasWidth / BaseDefine.ViewSize;
            int RatioY = (int)BaseDefine.CanvasHeight / BaseDefine.ViewSize;
            for (int i = 0; i < DataManager.recipeDM.MeasurementRD.DataMeasurementRoute.Count - 1; i++)
            {
                ShapeManager dataRoute = new ShapeArrowLine(RouteBrush, 8);
                ShapeArrowLine arrowLine = dataRoute as ShapeArrowLine;
                CCircle from = DataManager.recipeDM.MeasurementRD.DataSelectedPoint[DataManager.recipeDM.TeachingRD.DataMeasurementRoute[i]];
                CCircle to = DataManager.recipeDM.MeasurementRD.DataSelectedPoint[DataManager.recipeDM.TeachingRD.DataMeasurementRoute[i + 1]];

                from.Transform(RatioX, RatioY);
                to.Transform(RatioX, RatioY);

                PointF[] line = { new PointF((float)from.x + (float)(BaseDefine.CanvasWidth / 2), (float)-from.y + (float)(BaseDefine.CanvasHeight / 2)), new PointF((float)to.x + (float)(BaseDefine.CanvasWidth / 2), (float)-to.y + (float)(BaseDefine.CanvasHeight / 2)) };
                arrowLine.SetData(line, RouteBrush, 15, 5, 80);
                p_DrawRouteElement.Add(arrowLine.CanvasArrowLine);
                //Shapes[shapeIndex] = arrowLine;
                //shapeIndex++;
            }
        }

        #region ViewModel
        public Dlg_PM_ViewModel PMViewModel;
        public Dlg_Setting_ViewModel SettingViewModel;
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

        #region Dialog
        DialogService dialogService;
        private Dlg_RecipeManager DlgRecipeManager; 
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
                    if (RecipeViewModel.dataManager.recipeDM.RecipeOpen())
                    {
                        RecipeViewModel.UpdateListView(true);
                        RecipeViewModel.UpdateLayerGridView();
                        RecipeViewModel.UpdateView(true);
                        DrawMeasureRoute();
                        p_Progress = 0;
                    }

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
                    RecipeViewModel.UpdateLayerGridView();
                    RecipeViewModel.UpdateView(true);

                    DrawMeasureRoute();

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
                    dialog.HandlerUI.Init(App.m_engineer.m_handler);
                    dialog.LogUI.Init(LogView.m_logView);
                    dialog.ToolBoxUI.Init(App.m_engineer);
                    Nullable<bool> result = dialog.ShowDialog();
                    ////dialog.ShowDialog();
                    ////Nullable<bool> result = dialogService.ShowDialog(viewModel);
                });
            }
        }
        public ICommand CmdSetting
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var viewModel = SettingViewModel;
                    Nullable<bool> result = dialogService.ShowDialog(viewModel);
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