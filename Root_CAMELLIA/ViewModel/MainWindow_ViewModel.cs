﻿using Met = Root_CAMELLIA.LibSR_Met;
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
using Root_CAMELLIA.Control;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Emgu.CV;
using System.Threading;
using Emgu.CV.Structure;
using System.Data;

namespace Root_CAMELLIA
{
    public class MainWindow_ViewModel : ObservableObject
    {
        private MainWindow m_MainWindow;

        private DataManager _DataManager;
        public DataManager DataManager {
            get
            {
                return _DataManager;
            }
            set
            {
                SetProperty(ref _DataManager, value);
            }
        }

        BitmapSource m_imageSource;
        public BitmapSource p_imageSource
        {
            get
            {
                return m_imageSource;
            }
            set
            {
                SetProperty(ref m_imageSource, value);
            }
        }

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

        public Module_FDC p_Module_FDC
        {
            get
            {
                return m_Module_FDC;
            }
            set
            {
                SetProperty(ref m_Module_FDC, value);
            }
        }
        private Module_FDC m_Module_FDC;

        public Module_FDC p_Module_FDC_Vision
        {
            get
            {
                return m_Module_FDC_Vision;
            }
            set
            {
                SetProperty(ref m_Module_FDC_Vision, value);
            }
        }
        private Module_FDC m_Module_FDC_Vision;

        public Module_FFU p_Module_FFU
        {
            get
            {
                return m_Module_FFU;
            }
            set
            {
                SetProperty(ref m_Module_FFU, value);
            }
        }
        private Module_FFU m_Module_FFU;

        public Run_Measure p_Run_Measure
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
        private Run_Measure m_Run_Measure;


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

        ObservableCollection<Met.ContourMap> m_ContourMapCollection = new ObservableCollection<Met.ContourMap>();
        public ObservableCollection<Met.ContourMap> p_ContourMapCollection
        {
            get
            {
                return m_ContourMapCollection;
            }
            set
            {
                SetProperty(ref m_ContourMapCollection, value);
            }
        }


        Met.ContourMap m_ContourMapGraph = new Met.ContourMap();
        public Met.ContourMap p_ContourMapGraph
        {
            get
            {
                return m_ContourMapGraph;
            }
            set
            {
                SetProperty(ref m_ContourMapGraph, value);
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

            Run_Measure measure = (Run_Measure)p_Module_Camellia.CloneModuleRun("Measure");
            this.p_StageCenterPulse = measure.m_StageCenterPos_pulse;

            //if(p_Module_Camellia.p_CamVRS != null)
            //    p_Module_Camellia.p_CamVRS.Connect();
            SplashScreenHelper.ShowText("NanoView Initialize...");
            InitNanoView();
            if (p_InitNanoview)
            {
                InitTimer();
            }
           

            p_ContourMapCollection.Add(p_ContourMapGraph);


            p_Module_Camellia.p_CamVRS.Captured += GetImage;
        }

        private void GetImage(object obj, EventArgs e)
        {
            Thread.Sleep(100);
            RootTools.Camera.BaslerPylon.Camera_Basler p_CamVRS = p_Module_Camellia.p_CamVRS;
            Mat mat = new Mat(new System.Drawing.Size(p_CamVRS.GetRoiSize().X, p_CamVRS.GetRoiSize().Y), Emgu.CV.CvEnum.DepthType.Cv8U, 3, p_CamVRS.p_ImageViewer.p_ImageData.GetPtr(), (int)p_CamVRS.p_ImageViewer.p_ImageData.p_Stride * 3);
            Image<Bgra, byte> img = mat.ToImage<Bgra, byte>();

            //CvInvoke.Imshow("aa",img.Mat);
            //CvInvoke.WaitKey(0);
            //CvInvoke.DestroyAllWindows();
            //p_rootViewer.p_ImageData = new ImageData(p_CamVRS.p_ImageViewer.p_ImageData);
            //lock (lockObject)
            //{

            p_imageSource = ImageHelper.ToBitmapSource(img);
            //}
            //p_rootViewer.SetImageSource();
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

        public ObservableCollection<UIElement> p_DrawPointElement
        {
            get
            {
                return m_DrawPointElement;
            }
            set
            {
                //m_DrawPointElement = value;
                SetProperty(ref m_DrawPointElement, value);
            }
        }
        private ObservableCollection<UIElement> m_DrawPointElement = new ObservableCollection<UIElement>();

        public ObservableCollection<UIElement> p_DrawCandidatePointElement
        {
            get
            {
                return m_DrawCandidatePointElement;
            }
            set
            {
                //m_DrawPointElement = value;
                SetProperty(ref m_DrawCandidatePointElement, value);
            }
        }
        private ObservableCollection<UIElement> m_DrawCandidatePointElement = new ObservableCollection<UIElement>();

        public DataTable PointListItem
        {
            get
            {
                return _PointListItem;
            }
            set
            {
                SetProperty(ref _PointListItem, value);
            }
        }
        private DataTable _PointListItem = new DataTable();


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

        private bool m_InitNanoview = false;
        public bool p_InitNanoview
        {
            get
            {
                return m_InitNanoview;
            }
            set
            {
                SetProperty(ref m_InitNanoview, value);
            }
        }

        private System.Windows.Media.Brush m_ProgressColor;

        public SolidColorBrush RouteBrush { get; set; } = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 255));
        public SolidColorBrush test = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 1, 211, 40));
        #endregion

        private void InitNanoView()
        {
            string config = "";
            config = SettingViewModel.m_reg.Read(BaseDefine.RegNanoViewConfig, config);
            int port = -1;
            port = SettingViewModel.m_reg.Read(BaseDefine.RegNanoViewPort, port);

            if (config != "" && port != -1)
            {
                if (App.m_nanoView.InitializeSR(config, port) == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    p_InitNanoview = true;
                    SettingViewModel.LoadParameter();
                    SplashScreenHelper.ShowText("NanoView Initialize Done");
                }
                else
                {
                    SplashScreenHelper.ShowText("NanoView Initialize Error");
                }
            }
            SettingViewModel.LoadSettingData();


        }

        private void UpdateGaugeUI()
        {
            GaugeListItems.Clear();
            InitFDC();
        }

        private void UpdateFanUI()
        {
            FFUListItems.Clear();
            InitFFU();
        }

        Dispatcher dispatcher = null;
        private void Init()
        {
            DataManager = DataManager.Instance;
            //App.m_nanoView.InitializeSR();

            //NanoView = new Met.Nanoview();
            

            App.m_engineer.Init("Camellia");
            ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_camellia.mwvm = this;

            p_Module_Camellia = ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_camellia;
            p_Module_FDC = ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_FDC;
            p_Module_FDC_Vision = ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_FDC_Vision;
            p_Module_FFU = ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_FFU;
            InitFDC();
            InitFFU();

            p_Module_FDC.ValueUpdate += OnUpdateValue;
            p_Module_FDC_Vision.ValueUpdate += OnUpdateValue;
            dispatcher = Application.Current.Dispatcher;
        }

        private void InitFDC()
        {
            int cols = 0;
            for (int i = 0; i < p_Module_FDC.p_lData; i++)
            {
                GaugeChart gaugeChart = new GaugeChart();
                gaugeChart.p_from = p_Module_FDC.p_aData[i].m_mmLimit.X;
                gaugeChart.p_to = p_Module_FDC.p_aData[i].m_mmLimit.Y;
                gaugeChart.p_name = p_Module_FDC.p_aData[i].m_pid;
                gaugeChart.p_unit = p_Module_FDC.p_aData[i].p_sUnit;

                GaugeListItem gauge = new GaugeListItem();
                gauge.p_comId = p_Module_FDC.p_aData[i].p_nUnitID;
                gauge.p_moduleID = p_Module_FDC.p_id;
                if (i < 5)
                {
                    gauge.p_rowIndex = 0;
                }
                else
                {
                    gauge.p_rowIndex = 1;
                }
                gauge.p_columnIndex = cols;
                gauge.Gauge = gaugeChart;
                cols++;
                if (cols > 4)
                {
                    cols = 0;
                }
                GaugeListItems.Add(gauge);
            }

            for (int i = 0; i < p_Module_FDC_Vision.p_lData; i++)
            {
                GaugeChart gaugeChart = new GaugeChart();
                gaugeChart.p_from = p_Module_FDC_Vision.p_aData[i].m_mmLimit.X;
                gaugeChart.p_to = p_Module_FDC_Vision.p_aData[i].m_mmLimit.Y;
                gaugeChart.p_name = p_Module_FDC_Vision.p_aData[i].m_pid;
                gaugeChart.p_unit = p_Module_FDC_Vision.p_aData[i].p_sUnit;

                GaugeListItem gauge = new GaugeListItem();
                gauge.p_comId = p_Module_FDC_Vision.p_aData[i].p_nUnitID;
                gauge.p_moduleID = p_Module_FDC_Vision.p_id;

                gauge.p_rowIndex = 1;

                gauge.p_columnIndex = cols;
                gauge.Gauge = gaugeChart;
                cols++;
                if (cols > 4)
                {
                    cols = 0;
                }
                GaugeListItems.Add(gauge);
            }
        }

        private void InitFFU()
        {
            int cols = 0;
            for(int i = 0; i < p_Module_FFU.p_aUnit[0].p_aFan.Count; i++)
            {
                FFUListItem FanItem = new FFUListItem();
                FanItem.Unit = p_Module_FFU.p_aUnit[0].m_aFan[i];
                if(i < 2)
                {
                    FanItem.p_rowIndex = 0;
                }
                else
                {
                    FanItem.p_rowIndex = 1;
                }
                FanItem.p_columnIndex = cols;
                cols++;
                if(cols > 1)
                {
                    cols = 0;
                }

                FFUListItems.Add(FanItem);
            }

            cols = 2;
            for(int i = 0; i < p_Module_FFU.p_aUnit[0].m_aHumidity.Count; i++)
            {
                FFUListItem HumidityItem = new FFUListItem();
                HumidityItem.Unit = p_Module_FFU.p_aUnit[0].m_aHumidity[i];
                if(i < 1)
                {
                    HumidityItem.p_rowIndex = 0;
                }
                else
                {
                    HumidityItem.p_rowIndex = 1;
                }
                HumidityItem.p_columnIndex = cols;

                FFUListItems.Add(HumidityItem);
            }

            cols = 3;
            for (int i = 0; i < p_Module_FFU.p_aUnit[0].m_aTemp.Count; i++)
            {
                FFUListItem TemperatureItem = new FFUListItem();
                TemperatureItem.Unit = p_Module_FFU.p_aUnit[0].m_aTemp[i];
                if (i < 1)
                {
                    TemperatureItem.p_rowIndex = 0;
                }
                else
                {
                    TemperatureItem.p_rowIndex = 1;
                }
                TemperatureItem.p_columnIndex = cols;

                FFUListItems.Add(TemperatureItem);
            }

        }

        private void OnUpdateValue(object sender, EventArgs args)
        {
            dispatcher.Invoke(() =>
            {
                string p_id = ((Module_FDC)sender).p_id;
                int itemIndex = ((Module_FDC)sender).m_iData;
                for (int i = 0; i < GaugeListItems.Count; i++)
                {
                    if(GaugeListItems[i].p_moduleID == p_id)
                    {
                        if(p_id == "FDC")
                        {
                            if(GaugeListItems[i].p_comId == p_Module_FDC.m_aData[itemIndex].p_nUnitID)
                            {
                                GaugeListItems[i].Gauge.p_value = p_Module_FDC.m_aData[itemIndex].p_fValue;
                            }
                        }
                        else
                        {
                            if (GaugeListItems[i].p_comId == p_Module_FDC_Vision.m_aData[itemIndex].p_nUnitID)
                            {
                                GaugeListItems[i].Gauge.p_value = p_Module_FDC_Vision.m_aData[itemIndex].p_fValue;
                            }
                        }
                    }
                }
            });
        }

        private void ViewModelInit()
        {
            EngineerViewModel = new Dlg_Engineer_ViewModel(this);
            SettingViewModel = new Dlg_Setting_ViewModel(this);
            RecipeViewModel = new Dlg_RecipeManager_ViewModel(this);   
            PMViewModel = new Dlg_PM_ViewModel(this);
            ReviewViewModel = new Dlg_Review_ViewModel(this);
            LoginViewModel = new Dlg_Login_ViewModel(this);
            //StageMapViewModel = new Dlg_StageMapSetting_ViewModel(this);
        }

        private void DialogInit(MainWindow main)
        {
            dialogService = new DialogService(main);
            dialogService.Register<Dlg_Engineer_ViewModel, Dlg_Engineer>();
            dialogService.Register<Dlg_RecipeManager_ViewModel, Dlg_RecipeManager>();
            dialogService.Register<Dlg_Setting_ViewModel, Dlg_Setting>();
            dialogService.Register<Dlg_PM_ViewModel, Dlg_PM>();
            dialogService.Register<Dlg_Review_ViewModel, Dlg_Review>();
            dialogService.Register<Dlg_Login_ViewModel, Dlg_Login>();
            dialogService.Register<Dlg_StageMapSetting_ViewModel, Dlg_StageMapSetting>();
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
        public Dlg_Review_ViewModel ReviewViewModel;
        public Dlg_Login_ViewModel LoginViewModel;
        public Dlg_StageMapSetting_ViewModel StageMapViewModel;
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
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        private DispatcherTimer m_LightSourcetimer = new DispatcherTimer();
        private string m_LightSourcePath = "";
        public string p_LightSourcePath { get; set; }
        public void InitTimer()
        {
            //m_timer.Interval = TimeSpan.FromMilliseconds(100);
            //m_timer.Tick += M_timer_Tick;
            //m_timer.Start();
            p_LightSourcePath = SettingViewModel.m_reg.Read(BaseDefine.RegLightSourcePath, m_LightSourcePath);

            m_LightSourcetimer.Interval = TimeSpan.FromMinutes(5);
            m_LightSourcetimer.Tick += LightSourceTimer_Tick;
            m_LightSourcetimer.Start();

            //p_LightSourcePath = SettingViewModel.p_LightSourceLogPath;
            //p_LightSourcePath = SettingViewModel.m_reg.Read(BaseDefine.RegLightSourcePath, m_LightSourcePath);
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {

            //tbTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        private void LightSourceTimer_Tick(object sender, EventArgs e)
        {
            if(p_LightSourcePath != "")
            {
                 App.m_nanoView.LightSourceLogging(p_LightSourcePath);
            }
        }

        public void LightSourceTimer_Stop()
        {
            m_LightSourcetimer.Stop();
        }

        GaugeChart m_Test = new GaugeChart();
        public GaugeChart p_Test
        {
            get
            {
                return m_Test;
            }
            set
            {
                SetProperty(ref m_Test, value);
            }
        }

        ObservableCollection<GaugeListItem> _GaugeListItem = new ObservableCollection<GaugeListItem>();
        public ObservableCollection<GaugeListItem> GaugeListItems
        {
            get
            {
                return _GaugeListItem;
            }
            set
            {
                //_GaugeListItem = value;
                SetProperty(ref _GaugeListItem, value);
                //RaisePropertyChanged("GaugeListItems");
                //RaisePropertyChanged("GaugeListItems");
            }
        }

        ObservableCollection<FFUListItem> _FFUListItem = new ObservableCollection<FFUListItem>();
        public ObservableCollection<FFUListItem> FFUListItems
        {
            get
            {
                return _FFUListItem;
            }
            set
            {
                //_GaugeListItem = value;
                SetProperty(ref _FFUListItem, value);
                //RaisePropertyChanged("GaugeListItems");
                //RaisePropertyChanged("GaugeListItems");
            }
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
                        try
                        {
                            RecipeViewModel.UpdateLayerGridView();
                        }
                        catch
                        {

                        }
                        RecipeViewModel.UpdateView(true);
                        p_DrawCandidatePointElement = new ObservableCollection<UIElement>(RecipeViewModel.p_DrawCandidatePointElement);
                        p_DrawPointElement = new ObservableCollection<UIElement>(RecipeViewModel.p_DrawPointElement);
                        //PointListItem = RecipeViewModel.PointListItem;
                        PointListItem = RecipeViewModel.PointListItem.Copy();
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
                    if (!Login())
                    {
                        return;
                    }
                    //var viewModel = new Dlg_RecipeManager_ViewModel(this);
                    ////viewModel.dataManager = RecipeViewModel.dataManager;
                    bool isRecipeLoad = false;
                    if (DataManager.Instance.recipeDM.TeachRecipeName != "")
                    {
                        isRecipeLoad = true;
                    }
                    RecipeViewModel.UpdateListView(isRecipeLoad);
                    RecipeViewModel.UpdateView(isRecipeLoad, true);
                    Nullable<bool> result = dialogService.ShowDialog(RecipeViewModel);
                   
                    isRecipeLoad = false;
                    if (DataManager.Instance.recipeDM.TeachRecipeName != "")
                    {
                        isRecipeLoad = true;
                    }

                    if (!isRecipeLoad)
                        RecipeViewModel.ClearData();
                    RecipeViewModel.UpdateListView(isRecipeLoad);
                    try
                    {
                       RecipeViewModel.UpdateLayerGridView();
                    }
                    catch
                    {

                    }
                    RecipeViewModel.UpdateView(isRecipeLoad, true);

                    if (isRecipeLoad)
                    {
                        p_DrawCandidatePointElement = new ObservableCollection<UIElement>(RecipeViewModel.p_DrawCandidatePointElement);
                        PointListItem = RecipeViewModel.PointListItem.Copy();
                    }
                    p_DrawPointElement = new ObservableCollection<UIElement>(RecipeViewModel.p_DrawPointElement);
                    DrawMeasureRoute();

                });
            }
        }

        public ICommand CmdLogin
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!BaseDefine.Configuration.LoginSuccess)
                        Login();
                });
            }
        }

        public ICommand CmdLogout
        {
            get
            {
                return new RelayCommand(() =>
                {
                    BaseDefine.Configuration.LoginSuccess = false;
                });
            }
        }

        bool Login()
        {
            if (BaseDefine.Configuration.LoginSuccess)
            {
                return true;
            }
            var loginViewmodel = LoginViewModel;
            var loginDialog = dialogService.GetDialog(loginViewmodel) as Dlg_Login;
            Nullable<bool> LoginResult = loginDialog.ShowDialog();
            if (!(bool)LoginResult)
            {
                return false;
            }
            BaseDefine.Configuration.LoginSuccess = true;
            return true;
        }

        public ICommand CmdEngineer
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!Login())
                    {
                        return;
                    }

                    var viewModel = EngineerViewModel;
                    var dialog = dialogService.GetDialog(viewModel) as Dlg_Engineer;
                    viewModel.p_pmParameter.SetRecipeData(DataManager.recipeDM.MeasurementRD);
                    viewModel.SetMovePoint(DataManager.recipeDM.MeasurementRD);
                    dialog.HandlerUI.Init(App.m_engineer.m_handler);
                    dialog.LogUI.Init(LogView._logView);
                    dialog.ToolBoxUI.Init(App.m_engineer);
                    Nullable<bool> result = dialog.ShowDialog();

                    if (m_Module_FDC.m_aData[0].p_IsUpdate || m_Module_FDC_Vision.m_aData[0].p_IsUpdate )
                    {
                        UpdateGaugeUI();
                        m_Module_FDC.m_aData[0].p_IsUpdate = false;
                        m_Module_FDC_Vision.m_aData[0].p_IsUpdate = false;
                    }

                    if (m_Module_FFU.p_aUnit[0].p_aFan[0].p_IsUpdate)
                    {
                        UpdateFanUI();
                        m_Module_FFU.p_aUnit[0].p_aFan[0].p_IsUpdate = false;
                    }
                });
            }
        }

        public ICommand CmdReview
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //////test tes = new test();
                    ////// tes.ShowDialog();
                    //Random rand = new Random();
                    //for (int i = 0; i < GaugeListItems.Count; i++)
                    //{
                    //    GaugeListItems[i].Gauge.p_value = double.Parse(rand.Next(0, 100).ToString("#.##"));
                    //    //GaugeListItems[i].p_name = "strasgding" + i;
                    //}
                    //for (int i = 0; i < FFUListItems.Count; i++)
                    //{
                    //    Module_FFU.Unit.Fan fan = FFUListItems[i].Unit as Module_FFU.Unit.Fan;
                    //    if (fan != null)
                    //    {
                    //        if (fan.p_bRun == false)
                    //            fan.p_bRun = true;
                    //        else if (fan.p_bRun)
                    //            fan.p_bRun = false;
                    //    }
                    //    //HumidityListItem humidity = FanListItems[i] as HumidityListItem;
                    //    //if (humidity != null)
                    //    //{
                    //    //    humidity.Humidity.p_nHumidity = rand.Next(0, 40);
                    //    //}

                    //    //TemperatureListItem Temp = FanListItems[i] as TemperatureListItem;
                    //    //if (Temp != null)
                    //    //{
                    //    //    Temp.Temperature.p_nTemp = rand.Next(0, 40);
                    //    //}

                    //}

                    //Dlg_Review review = new Dlg_Review();
                    //review.ShowDialog();
                    ////GaugeListItem gauge = new GaugeListItem();
                    ////gauge.Gauge = new GaugeChart();
                    ////gauge.p_columnIndex = 0;
                    ////gauge.p_rowIndex = 3;
                    ////gauge.Gauge.p_name = "aftest";
                    ////gauge.Gauge.p_value = 40;
                    ////GaugeListItems.Add(gauge);
                    //////p_Test.p_value = rand.Next(0,100);
                    //        ///
                    //        // GaugeListItems = new ObservableCollection<GaugeListItem>();
                    //    int cols = 0;
                    // GaugeListItems.Clear();

                    //for(int i= 0; i < GaugeListItems.Count; i++)
                    //{

                    //}
                    var viewModel = ReviewViewModel;
                    var dialog = dialogService.GetDialog(viewModel) as Dlg_Review;
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
                    if (!Login())
                    {
                        return;
                    }

                    var viewModel = SettingViewModel;
                    if (m_InitNanoview)
                    {
                        viewModel.LoadParameter();
                    }
                    viewModel.LoadSettingData();
                    viewModel.LoadConfig();
                    Nullable<bool> result = dialogService.ShowDialog(viewModel);
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
                    DataManager.Instance.m_SaveMeasureData.ThreadStop();
                    App.m_engineer.BuzzerOff();
                    Application.Current.Shutdown();
                });
            }
        }

        public object ModuleCamellia { get; private set; }
        #endregion

        #region Event
        public void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            
            //UIElement el = (UIElement)sender;
            if(e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                System.Windows.Point pt = e.GetPosition((UIElement)sender);

            }


        }
        #endregion
    }

    public class FFUListItem : ObservableObject
    {
        Module_FFU.UnitState _unit;
        public Module_FFU.UnitState Unit
        {
            get
            {
                return _unit;
            }
            set
            {
                SetProperty(ref _unit, value);
            }
        }

        int m_columnIndex = 0;
        public int p_columnIndex
        {
            get
            {
                return m_columnIndex;
            }
            set
            {
                SetProperty(ref m_columnIndex, value);
                RaisePropertyChanged("p_columnIndex");
            }
        }


        int m_rowIndex = 0;
        public int p_rowIndex
        {
            get
            {
                return m_rowIndex;
            }
            set
            {
                SetProperty(ref m_rowIndex, value);
                RaisePropertyChanged("p_rowIndex");
            }
        }
    }

    //public class FanListItem : ObservableObject
    //{
    //    public FanListItem()
    //    {

    //    }
    //    Module_FFU.Unit.Fan _fan;
    //    public Module_FFU.Unit.Fan Fan
    //    {
    //        get
    //        {
    //            return _fan;
    //        }
    //        set
    //        {
    //            SetProperty(ref _fan, value);
    //        }
    //    }

    //    int m_columnIndex = 0;
    //    public int p_columnIndex
    //    {
    //        get
    //        {
    //            return m_columnIndex;
    //        }
    //        set
    //        {
    //            SetProperty(ref m_columnIndex, value);
    //            RaisePropertyChanged("p_columnIndex");
    //        }
    //    }


    //    int m_rowIndex = 0;
    //    public int p_rowIndex
    //    {
    //        get
    //        {
    //            return m_rowIndex;
    //        }
    //        set
    //        {
    //            SetProperty(ref m_rowIndex, value);
    //            RaisePropertyChanged("p_rowIndex");
    //        }
    //    }
    //}

    //public class HumidityListItem : ObservableObject
    //{
    //    public HumidityListItem()
    //    {

    //    }
    //    Module_FFU.Unit.Humidity _humidity;
    //    public Module_FFU.Unit.Humidity Humidity
    //    {
    //        get
    //        {
    //            return _humidity;
    //        }
    //        set
    //        {
    //            SetProperty(ref _humidity, value);
    //        }
    //    }

    //    int m_columnIndex = 0;
    //    public int p_columnIndex
    //    {
    //        get
    //        {
    //            return m_columnIndex;
    //        }
    //        set
    //        {
    //            SetProperty(ref m_columnIndex, value);
    //            RaisePropertyChanged("p_columnIndex");
    //        }
    //    }

    //    int m_rowIndex = 0;
    //    public int p_rowIndex
    //    {
    //        get
    //        {
    //            return m_rowIndex;
    //        }
    //        set
    //        {
    //            SetProperty(ref m_rowIndex, value);
    //            RaisePropertyChanged("p_rowIndex");
    //        }
    //    }
    //}

    //public class TemperatureListItem : ObservableObject
    //{
    //    public TemperatureListItem()
    //    {

    //    }
    //    Module_FFU.Unit.Temp _temperature;
    //    public Module_FFU.Unit.Temp Temperature
    //    {
    //        get
    //        {
    //            return _temperature;
    //        }
    //        set
    //        {
    //            SetProperty(ref _temperature, value);
    //        }
    //    }

    //    int m_columnIndex = 0;
    //    public int p_columnIndex
    //    {
    //        get
    //        {
    //            return m_columnIndex;
    //        }
    //        set
    //        {
    //            SetProperty(ref m_columnIndex, value);
    //            RaisePropertyChanged("p_columnIndex");
    //        }
    //    }

    //    int m_rowIndex = 0;
    //    public int p_rowIndex
    //    {
    //        get
    //        {
    //            return m_rowIndex;
    //        }
    //        set
    //        {
    //            SetProperty(ref m_rowIndex, value);
    //            RaisePropertyChanged("p_rowIndex");
    //        }
    //    }
    //}

    public class GaugeListItem : ObservableObject
    {
        public GaugeListItem()
        {
            //Gauge = new GaugeChart();
        }


        GaugeChart _gauge = new GaugeChart();
        public GaugeChart Gauge
        {
            get
            {
                return _gauge;
            }
            set
            {
                SetProperty(ref _gauge, value);

            }
        }

        public int p_comId { get; set; }
        public string p_moduleID { get; set; }

        int m_columnIndex = 0;
        public int p_columnIndex
        {
            get
            {
                return m_columnIndex;
            }
            set
            {
                SetProperty(ref m_columnIndex, value);
                RaisePropertyChanged("p_columnIndex");
            }
        }


        int m_rowIndex = 0;
        public int p_rowIndex
        {
            get
            {
                return m_rowIndex;
            }
            set
            {
                SetProperty(ref m_rowIndex, value);
                RaisePropertyChanged("p_rowIndex");
            }
        }
    }
}