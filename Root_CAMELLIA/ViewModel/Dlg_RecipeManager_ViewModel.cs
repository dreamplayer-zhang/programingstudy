﻿using Camellia2Stage;
using ColorPickerLib.Controls;
using Root_CAMELLIA.Data;
using Root_CAMELLIA.Draw;
using Root_CAMELLIA.ShapeDraw;
using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Root_CAMELLIA
{
    public class Dlg_RecipeManager_ViewModel : ObservableObject, IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
        private MainWindow_ViewModel MainViewModel;
        private Dlg_Setting_ViewModel m_SettingViewModel;
        public Dlg_Setting_ViewModel p_SettingViewModel
        {
            get { return m_SettingViewModel; }
            set
            {
                SetProperty(ref m_SettingViewModel, value);
            }
        }

        public DataManager dataManager { get; set; }
        ShapeEllipse shape = new ShapeEllipse();

        public Dlg_RecipeManager_ViewModel(MainWindow_ViewModel main)
        {
            MainViewModel = main;
            p_SettingViewModel = main.SettingViewModel;
            dataManager = DataManager.Instance;

            RecipePath = dataManager.recipeDM.TeachingRecipePath;
            Init();
            InitStage();
            SetStage();
            SetSelectRect();
            SetViewRect();
            InitLayer();
            UpdateLayerGridView();
        }
        public void Init()
        {
            //pointListItem = new DataTable();
            PointListItem.Columns.Add(new DataColumn("ListIndex"));
            PointListItem.Columns.Add(new DataColumn("ListX"));
            PointListItem.Columns.Add(new DataColumn("ListY"));
            PointListItem.Columns.Add(new DataColumn("ListRoute"));

            RouteThickness = "3";
            RouteThick = 3;
            XPosition = "0.000 mm";
            YPosition = "0.000 mm";
            CurrentTheta = "0 °";
            CurrentRadius = "0.000 mm";
            PointCount = "0";
            Percentage = "0";
            ZoomScale = 1;

            CenterX = (int)(1000 * 0.5f);
            CenterY = (int)(1000 * 0.5f);
            RatioX = 1000 / BaseDefine.ViewSize;
            RatioY = 1000 / BaseDefine.ViewSize;
            OffsetScale = 100;

            DataViewPosition.X = 0;
            DataViewPosition.Y = 0;
            DataViewPosition.Width = 1000;
            DataViewPosition.Height = 1000;
            CurrentCandidatePoint = -1;
            CurrentSelectPoint = -1;


            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.SystemIdle);    //객체생성

            timer.Interval = TimeSpan.FromMilliseconds(100);    //시간간격 설정

            timer.Tick += new EventHandler(MouseHover);          //이벤트 추가

            timer.Start();
        }

        #region Collection Stage Canvas 
        /// <summary>
        /// Main Stage Canvas
        /// </summary>
        public ObservableCollection<UIElement> p_DrawElement
        {
            get
            {
                return m_DrawElement;
            }
            set
            {
                SetProperty(ref m_DrawElement, value);
                //m_DrawElement = value;
            }
        }
        private ObservableCollection<UIElement> m_DrawElement = new ObservableCollection<UIElement>();

        /// <summary>
        /// Preview Stage Canvas
        /// </summary>
        public ObservableCollection<UIElement> p_PreviewDrawElement
        {
            get
            {
                return m_PreviewDrawElement;
            }
            set
            {
                SetProperty(ref m_PreviewDrawElement, value);
            }
        }
        private ObservableCollection<UIElement> m_PreviewDrawElement = new ObservableCollection<UIElement>();

        private List<UIElement> previewTemp = new List<UIElement>();

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

        public ObservableCollection<UIElement> p_DrawSelectElement
        {
            get
            {
                return m_DrawSelectElement;
            }
            set
            {
                m_DrawSelectElement = value;
            }
        }
        private ObservableCollection<UIElement> m_DrawSelectElement = new ObservableCollection<UIElement>();
        #endregion

        #region List Draw Object
        public List<ShapeManager> Shapes = new List<ShapeManager>();
        public List<ShapeManager> PreviewShapes = new List<ShapeManager>();
        public List<GeometryManager> Geometry = new List<GeometryManager>();
        public List<GeometryManager> ViewRectGeometry = new List<GeometryManager>();
        public List<GeometryManager> SelectGeometry = new List<GeometryManager>();
        public List<TextManager> TextBlocks = new List<TextManager>();
        #endregion

        #region Property

        public double RatioX
        {
            get; set;
        }
        public double RatioY
        {
            get; set;
        }
        public int CenterX
        {
            get; set;
        }
        public int CenterY
        {
            get; set;
        }
        public int OffsetX
        {
            get; set;
        }
        public int OffsetY
        {
            get; set;
        }
        public int OffsetScale
        {
            get; set;
        }
        public int PreviewCenterX
        {
            get; set;
        }
        public int PreviewCenterY

        {
            get; set;
        }

        public int CurrentCandidatePoint
        {
            get; set;
        }
        public int CurrentSelectPoint
        {
            get; set;
        }
        public int RouteThick
        {
            get;
            set;
        }

        public bool ShiftKeyDown { get; set; } = false;
        public bool CtrlKeyDown { get; set; } = false;
        public bool ColorPickerOpened
        {
            get; set;
        }

        public bool IsShowIndex { get; set; } = false;
        public bool IsKeyboardShowIndex { get; set; } = false;
        public bool IsLockUI { get; set; } = false;
        bool SetStartEndPointMode { get; set; } = false;
        public bool ShowRoute
        {
            get; set;
        }

        public string LockState
        {
            get
            {
                return lockState;
            }
            set
            {
                lockState = value;
                RaisePropertyChanged("LockState");
            }
        }
        public string lockState = "Lock UI";

        public bool StageMouseHover
        {
            get; set;
        }
        public bool StageMouseHoverUpdate
        {
            get; set;
        }

        public bool MouseMove
        {
            get; set;
        }
        public bool LeftMouseDown
        {
            get; set;
        }
        public bool RightMouseDown
        {
            get; set;
        }
        public System.Windows.Point MousePoint
        {
            get; set;
        }
        public System.Windows.Point CurrentMousePoint
        {
            get; set;
        }
        public System.Windows.Point MoveMousePoint
        {
            get; set;
        }
        public System.Windows.Point SelectStartPoint
        {
            get; set;
        }
        public System.Windows.Point SelectEndPoint
        {
            get; set;
        }
        public bool Drag
        {
            get; set;
        }

        public int ZoomScale
        {
            get
            {
                return _nZoomScale;
            }
            set
            {
                if (0 < value && value < 16)
                {
                    if (ZoomScale < value)
                    {
                        _nZoomScale = value;
                        //VerticalScroll.Maximum = HorizontalScroll.Maximum = 10 * nZoomScale;
                        //VerticalScroll.Visibility = Visibility.Visible;
                        //HorizontalScroll.Visibility = Visibility.Visible;
                        CamelliaStage.ZoomScale = value;
                        if (OffsetX != 0)
                        {
                            OffsetX *= 2;
                        }
                        if (OffsetY != 0)
                        {
                            OffsetY *= 2;
                        }
                    }
                    else
                    {
                        _nZoomScale = value;
                        CamelliaStage.ZoomScale = value;
                        // VerticalScroll.Maximum = HorizontalScroll.Maximum = 10 * nZoomScale;
                        if (_nZoomScale == 1)
                        {

                            OffsetX = OffsetY = 0;
                        }
                        else
                        {
                            if (OffsetX != 0)
                            {
                                OffsetX /= 2;
                            }
                            if (OffsetY != 0)
                            {
                                OffsetY /= 2;
                            }
                        }
                    }
                }
            }
        }
        int _nZoomScale;

        public string Percentage
        {
            get
            {
                return strPercentage;
            }
            set
            {
                strPercentage = value;
                RaisePropertyChanged("Percentage");
            }
        }
        private string strPercentage;

        public string XPosition
        {
            get
            {
                return _XPosition;
            }
            set
            {

                _XPosition = value;
                RaisePropertyChanged("XPosition");
            }
        }
        private string _XPosition;

        public string YPosition
        {
            get
            {
                return _YPosition;
            }
            set
            {
                _YPosition = value;
                RaisePropertyChanged("YPosition");
            }
        }
        private string _YPosition;

        public string RouteThickness
        {
            get
            {
                return routeThickness;
            }
            set
            {
                routeThickness = value;
                RaisePropertyChanged("RouteThickness");
            }
        }
        private string routeThickness;

        public string CurrentTheta
        {
            get
            {
                return _CurrentTheta;
            }
            set
            {
                _CurrentTheta = value;
                RaisePropertyChanged("CurrentTheta");
            }
        }
        private string _CurrentTheta;

        public string CurrentRadius
        {
            get
            {
                return _CurrentRadius;
            }
            set
            {
                _CurrentRadius = value;
                RaisePropertyChanged("CurrentRadius");
            }
        }
        private string _CurrentRadius;

        public string PointCount
        {
            get
            {
                return _PointCount;
            }
            set
            {
                _PointCount = value;
                RaisePropertyChanged("PointCount");
            }
        }
        private string _PointCount;

        public DataTable PointListItem
        {
            get
            {
                return _PointListItem;
            }
            set
            {
                _PointListItem = value;
                RaisePropertyChanged("PointListItem");
            }
        }
        private DataTable _PointListItem = new DataTable();

        public ObservableCollection<double> ReflectanceListItem
        {
            get
            {
                return _ReflectanceListItem;
            }
            set
            {
                _ReflectanceListItem = value;
                RaisePropertyChanged("ReflectanceListItem");
            }
        }

        private ObservableCollection<double> _ReflectanceListItem = new ObservableCollection<double>();

        public ObservableCollection<double> TransmittanceListItem
        {
            get
            {
                return _TransmittanceListItem;
            }
            set
            {
                _TransmittanceListItem = value;
                RaisePropertyChanged("TransmittanceListItem");
            }
        }

        private ObservableCollection<double> _TransmittanceListItem = new ObservableCollection<double>();

        public ObservableCollection<string> MaterialListItem
        {
            get
            {
                return _MaterialListItem;
            }
            set
            {
                _MaterialListItem = value;
                RaisePropertyChanged("MaterialListItem");
            }
        }

        private ObservableCollection<string> _MaterialListItem = new ObservableCollection<string>();

        public ObservableCollection<string> SelectedModels
        {
            get
            {
                return _SelectedModels;
            }
            set
            {
                _SelectedModels = value;
                RaisePropertyChanged("SelectedModels");
            }
        }

        private ObservableCollection<string> _SelectedModels = new ObservableCollection<string>();

        public ObservableCollection<ModelData.LayerData> GridLayerData
        {
            get
            {
                return _GridLayerData;
            }
            set
            {
                _GridLayerData = value;
                SetProperty(ref _GridLayerData, value);
            }
        }

        private ObservableCollection<ModelData.LayerData> _GridLayerData = new ObservableCollection<ModelData.LayerData>();


        CamelliaStage _camelliaStage = new CamelliaStage();
        public CamelliaStage CamelliaStage
        {
            get
            {
                return _camelliaStage;
            }
            set
            {
                _camelliaStage = value;
                SetProperty(ref _camelliaStage, value);
            }
        }

        public string PointAddMode
        {
            get
            {
                return pointAddMode;
            }
            set
            {
                pointAddMode = value;
                RaisePropertyChanged("PointAddMode");
            }
        }

        private string pointAddMode = "Normal";

        private string _VISIntegrationTime = "20";
        public string VISIntegrationTime
        {
            get
            {
                return _VISIntegrationTime;
            }
            set
            {
                int val = 0;
                if (value == "")
                {
                    _VISIntegrationTime = val.ToString();
                    dataManager.recipeDM.TeachingRD.VISIntegrationTime = val;
                }
                else if (int.TryParse(value, out val))
                {
                    _VISIntegrationTime = val.ToString();
                    dataManager.recipeDM.TeachingRD.VISIntegrationTime = val;     
                }
                else
                {
                    _VISIntegrationTime = dataManager.recipeDM.TeachingRD.VISIntegrationTime.ToString();
                }
                RaisePropertyChanged("VISIntegrationTime");
            }
        }

        private string _NIRIntegrationTime = "150";
        public string NIRIntegrationTime
        {
            get
            {
                return _NIRIntegrationTime;
            }
            set
            {
                int val;
                if (value == "")
                {
                    _NIRIntegrationTime = "0";
                    dataManager.recipeDM.TeachingRD.NIRIntegrationTime = 0;
                }
                else if(int.TryParse(value, out val))
                {
                    _NIRIntegrationTime = val.ToString();
                    dataManager.recipeDM.TeachingRD.NIRIntegrationTime = val;
                }
                else
                {
                    _NIRIntegrationTime = dataManager.recipeDM.TeachingRD.NIRIntegrationTime.ToString();
                }
               
              
                RaisePropertyChanged("NIRIntegrationTime");
            }
        }

        private float _LowerWaveLength = 350.0f;
        public float LowerWaveLength
        {
            get
            {
                return _LowerWaveLength;
            }
            set
            {
                _LowerWaveLength = value;
                dataManager.recipeDM.TeachingRD.LowerWaveLength = _LowerWaveLength;
                RaisePropertyChanged("LowerWaveLength");
            }
        }

        private float _UpperWaveLength = 0.0f;
        public float UpperWaveLength
        {
            get
            {
                return _UpperWaveLength;
            }
            set
            {
                _UpperWaveLength = value;
                dataManager.recipeDM.TeachingRD.UpperWaveLength = _UpperWaveLength;
                RaisePropertyChanged("UpperWaveLength");
            }
        }

        private int _ThicknessLMIteration = 0;
        public int ThicknessLMIteration
        {
            get
            {
                return _ThicknessLMIteration;
            }
            set
            {
                _ThicknessLMIteration = value;
                dataManager.recipeDM.TeachingRD.LMIteration = _ThicknessLMIteration;
                RaisePropertyChanged("ThicknessLMIteration");
            }
        }

        //private float _DampingFactor = 0.0f;
        private string _DampingFactor = "0.00";
        public string DampingFactor
        {
            get
            {
                return _DampingFactor;
            }
            set
            {
                float val = 0.0f;
                if (float.TryParse(value, out val))
                {
                    _DampingFactor = val.ToString("N2");
                }
                else
                {
                    _DampingFactor = dataManager.recipeDM.TeachingRD.DampingFactor.ToString("N2");
                    val = dataManager.recipeDM.TeachingRD.DampingFactor;
                }
                dataManager.recipeDM.TeachingRD.DampingFactor = val;
                RaisePropertyChanged("DampingFactor");
                //if (value == "")
                //{
                //    _DampingFactor = dataManager.recipeDM.TeachingRD.DampingFactor.ToString("N2");
                //    val = dataManager.recipeDM.TeachingRD.DampingFactor;
                //}
                //else if (float.TryParse(value, out val))
                //{
                //    _DampingFactor = val.ToString("N2");
                //}
                //else
                //{
                //    _DampingFactor = dataManager.recipeDM.TeachingRD.DampingFactor.ToString("N2");
                //    val = dataManager.recipeDM.TeachingRD.DampingFactor;
                //}
                //dataManager.recipeDM.TeachingRD.DampingFactor = val;

            }
        }

        private bool _IsReflectanceCheck = true;
        public bool IsReflectanceCheck
        {
            get
            {
                return _IsReflectanceCheck;
            }
            set
            {
                TransmittanceSelectedIndex = -1;
                _IsReflectanceCheck = value;
                RaisePropertyChanged("IsReflectanceCheck");
            }
        }

        private bool _IsTransmittanceCheck = false;
        public bool IsTransmittanceCheck
        {
            get
            {
                return _IsTransmittanceCheck;
            }
            set
            {
                ReflectanceSelectedIndex = -1;
                _IsTransmittanceCheck = value;
                RaisePropertyChanged("IsTransmittanceCheck");
            }
        }

        private string _WaveLengthValue = "0.0";
        public string WaveLengthValue
        {
            get
            {
                return _WaveLengthValue;
            }
            set
            {
                double val;
                if(double.TryParse(value, out val))
                {
                    _WaveLengthValue = val.ToString("N3");
                }
                else
                {
                    MessageBox.Show("Invalid Value Entered", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                RaisePropertyChanged("WaveLengthValue");
            }
        }

        private int _ReflectanceSelectedIndex = -1;
        public int ReflectanceSelectedIndex
        {
            get
            {
                return _ReflectanceSelectedIndex;
            }
            set
            {
                _ReflectanceSelectedIndex = value;
                RaisePropertyChanged("ReflectanceSelectedIndex");
            }
        }

        private int _TransmittanceSelectedIndex = -1;
        public int TransmittanceSelectedIndex
        {
            get
            {
                return _TransmittanceSelectedIndex;
            }
            set
            {
                _TransmittanceSelectedIndex = value;
                RaisePropertyChanged("TransmittanceSelectedIndex");
            }
        }

        private string _ModelPath = "";
        public string ModelPath
        {
            get
            {
                return _ModelPath;
            }
            set
            {
                _ModelPath = value;
                RaisePropertyChanged("ModelPath");
            }
        }

        private int _MaterialSelectIndex = -1;
        public int MaterialSelectIndex
        {
            get
            {
                return _MaterialSelectIndex;
            }
            set
            {
                _MaterialSelectIndex = value;
                RaisePropertyChanged("MaterialSelectIndex");
            }
        }

        private int _GridLayerIndex = -1;
        public int GridLayerIndex
        {
            get
            {
                return _GridLayerIndex;
            }
            set
            {
                _GridLayerIndex = value;
                RaisePropertyChanged("GridLayerIndex");
            }
        }

        private bool _UseThickness = true;
        public bool p_UseThickness
        {
            get
            {
                return _UseThickness;
            }
            set
            {
                if (!value)
                {
                    p_UseTransmittance = value;
                }
                dataManager.recipeDM.TeachingRD.UseThickness = value;
                SetProperty(ref _UseThickness, value);
            }
        }

        private bool _UseTransmittance = true;
        public bool p_UseTransmittance
        {
            get
            {
                return _UseTransmittance;
            }
            set
            {
                if (!p_UseThickness && value)
                {
                    MessageBox.Show("Need Using Thickness Measurement", "Check Thickness", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                if (IsTransmittanceCheck)
                {
                    IsTransmittanceCheck = false;
                    IsReflectanceCheck = true;
                }
                dataManager.recipeDM.TeachingRD.UseTransmittance = value;
                SetProperty(ref _UseTransmittance, value);
            }
        }

        private string _RecipePath = "";
        public string RecipePath
        {
            get
            {
                return _RecipePath;
            }
            set
            {
                SetProperty(ref _RecipePath, value);
            }
        }
        #endregion

        #region DataStage     
        private Rect DataViewPosition = new Rect();
        #endregion

        #region ViewStage
        public Circle viewStageField = new Circle();
        public ShapeDraw.Line viewStageLineHole = new ShapeDraw.Line();
        public Arc[] viewStageEdgeHoleArc = new Arc[8];
        public Circle[] ViewStageGuideLine = new Circle[4];
        public Arc[] viewStageDoubleHoleArc = new Arc[8];

        public Arc[] viewStageTopHoleArc = new Arc[2];
        public Arc[] viewStageBotHoleArc = new Arc[2];
        #endregion

        #region Geometry, Shape
        private DrawGeometryManager drawGeometryManager = new DrawGeometryManager();

        private List<ShapeEllipse> listCandidatePoint = new List<ShapeEllipse>();
        private List<ShapeEllipse> listPreviewCandidatePoint = new List<ShapeEllipse>();
        private List<ShapeEllipse> listSelectedPoint = new List<ShapeEllipse>();
        private List<ShapeEllipse> listPreviewSelectedPoint = new List<ShapeEllipse>();
        #endregion

        #region CircleHole
        private Circle viewStageCircleHole = new Circle();
        public List<Circle> dataStageCircleHole = new List<Circle>();
        #endregion

        #region ViewRect
        GeometryManager viewRect;
        #endregion

        #region Stage
        GeometryManager stage;
        TextManager textManager;
        ShapeManager dataPoint;
        ShapeManager dataRoute;
        GeometryManager selectRectangle;
        System.Windows.Controls.Image LockImage = new System.Windows.Controls.Image();
        #endregion

        #region RouteOrder        
        List<int> RouteOrder = new List<int>();
        public int ReorderCnt { get; set; } = 0;
        public List<CCircle> ListReorderPoint { get; set; } = new List<CCircle>();
        public int Limit { get; set; } = 30;
        #endregion

        #region Brush
        private SolidColorBrush normalBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(208, 221, 221, 221));
        private SolidColorBrush buttonSelectBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 134, 255, 117));
        //SolidColorBrush routeBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 128));
        public SolidColorBrush RouteBrush
        {
            get
            {
                return routeBrush;
            }
            set
            {
                routeBrush = value;
                RaisePropertyChanged("RouteBrush");
            }
        }
        private SolidColorBrush routeBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 255));

        public SolidColorBrush ReorderBrush
        {
            get
            {
                return _ReorderBrush;
            }
            set
            {
                _ReorderBrush = value;
                RaisePropertyChanged("ReorderBrush");
            }
        }
        private SolidColorBrush _ReorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(208, 221, 221, 221));

        public SolidColorBrush LockBrush
        {
            get
            {
                return _LockBrush;
            }
            set
            {
                _LockBrush = value;
                RaisePropertyChanged("LockBrush");
            }
        }
        private SolidColorBrush _LockBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(208, 221, 221, 221));

        public SolidColorBrush ShowIndexBrush
        {
            get
            {
                return _ShowIndexBrush;
            }
            set
            {
                _ShowIndexBrush = value;
                RaisePropertyChanged("ShowIndexBrush");
            }
        }
        private SolidColorBrush _ShowIndexBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(208, 221, 221, 221));

        public SolidColorBrush ShiftBrush
        {
            get
            {
                return _ShiftBrush;
            }
            set
            {
                _ShiftBrush = value;
                RaisePropertyChanged("ShiftBrush");
            }
        }
        SolidColorBrush _ShiftBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 221, 221, 221));

        public SolidColorBrush CtrlBrush
        {
            get
            {
                return _CtrlBrush;
            }
            set
            {
                _CtrlBrush = value;
                RaisePropertyChanged("CtrlBrush");
            }
        }
        SolidColorBrush _CtrlBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 221, 221, 221));

        public SolidColorBrush SBrush
        {
            get
            {
                return _SBrush;
            }
            set
            {
                _SBrush = value;
                RaisePropertyChanged("SBrush");
            }
        }
        SolidColorBrush _SBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 221, 221, 221));
        #endregion

        #region Mouse Event
        private void MouseHover(object sender, EventArgs e)
        {
            Thread.Sleep(1);
            if (IsLockUI)
            {
                return;
            }
            if (StageMouseHover)
            {
                bool bSelected = false;

                int nIndex = 0;
                double dMin = 9999;
                int nMinIndex = 0;

                foreach (ShapeEllipse se in listCandidatePoint)
                {
                    double dDistance = GetDistance(se, new System.Windows.Point(MousePoint.X, MousePoint.Y));

                    if (dDistance < dMin)
                    {
                        dMin = dDistance;
                        nMinIndex = nIndex;
                    }
                    nIndex++;
                }

                int idx = 0;
                if (!SetStartEndPointMode)
                {
                    foreach (ShapeEllipse se in listCandidatePoint)
                    {
                        if (se.Equals(listCandidatePoint[nMinIndex]))
                        {
                            bSelected = true;
                            se.SetBrush(GeneralTools.GbSelect);
                            listPreviewCandidatePoint[nMinIndex].SetBrush(GeneralTools.GbSelect);
                            shape = se;
                        }
                        else
                        {
                            se.SetBrush(GeneralTools.StageHoleBrush);
                            listPreviewCandidatePoint[idx].SetBrush(GeneralTools.StageHoleBrush);
                        }
                        idx++;
                    }
                    if (bSelected)
                    {
                        CurrentCandidatePoint = listCandidatePoint.IndexOf(shape);
                    }
                    else
                    {
                        CurrentCandidatePoint = -1;
                    }
                }
                bSelected = false;
                idx = 0;
                int nDummyIdx = -1;
                foreach (ShapeEllipse se in listSelectedPoint)
                {
                    if (se.CenterX == listCandidatePoint[nMinIndex].CenterX && se.CenterY == listCandidatePoint[nMinIndex].CenterY)
                    {
                        bSelected = true;
                        se.SetBrush(GeneralTools.SelectedOverBrush);
                        listPreviewSelectedPoint[idx].SetBrush(GeneralTools.SelectedOverBrush);
                        shape = se;
                    }
                    else
                    {
                        if (SetStartEndPointMode)
                        {
                            if (dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count > 0)
                            {
                                CCircle circle = dataManager.recipeDM.TeachingRD.DataSelectedPoint[idx];
                                if (ContainsData(ListReorderPoint, circle, out nDummyIdx))
                                {
                                    se.SetBrush(System.Windows.Media.Brushes.Cyan);
                                    listPreviewSelectedPoint[idx].SetBrush(System.Windows.Media.Brushes.Cyan);
                                }
                                else
                                {
                                    se.SetBrush(System.Windows.Media.Brushes.DarkBlue);
                                    listPreviewSelectedPoint[idx].SetBrush(System.Windows.Media.Brushes.DarkBlue);
                                }
                            }
                            else
                            {
                                se.SetBrush(System.Windows.Media.Brushes.DarkBlue);
                                listPreviewSelectedPoint[idx].SetBrush(System.Windows.Media.Brushes.DarkBlue);
                            }
                        }
                        else
                        {
                            se.SetBrush(GeneralTools.GbHole);
                            listPreviewSelectedPoint[idx].SetBrush(GeneralTools.GbHole);

                        }
                    }
                    idx++;
                }
                if (bSelected)
                {
                    CurrentCandidatePoint = -1;
                    CurrentSelectPoint = listSelectedPoint.IndexOf(shape);
                }
                else
                {
                    CurrentSelectPoint = -1;
                }
            }
            else
            {
                if (StageMouseHoverUpdate)
                {
                    int idx = 0;

                    foreach (ShapeEllipse se in listCandidatePoint)
                    {
                        se.SetBrush(GeneralTools.StageHoleBrush);
                        listPreviewCandidatePoint[idx].SetBrush(GeneralTools.StageHoleBrush);
                        idx++;
                    }
                    idx = 0;
                    foreach (ShapeEllipse se in listSelectedPoint)
                    {

                        if (SetStartEndPointMode)
                        {
                            int nDummyidx = -1;
                            CCircle circle = dataManager.recipeDM.TeachingRD.DataSelectedPoint[idx];
                            if (ContainsData(ListReorderPoint, circle, out nDummyidx))
                            {
                                se.SetBrush(System.Windows.Media.Brushes.Cyan);
                                listPreviewSelectedPoint[idx].SetBrush(System.Windows.Media.Brushes.Cyan);
                            }
                            else
                            {
                                se.SetBrush(System.Windows.Media.Brushes.DarkBlue);
                                listPreviewSelectedPoint[idx].SetBrush(System.Windows.Media.Brushes.DarkBlue);
                            }
                            //se.SetBrush(System.Windows.Media.Brushes.DarkBlue);
                            //listPreviewSelectedPoint[idx].SetBrush(System.Windows.Media.Brushes.DarkBlue);
                        }
                        else
                        {
                            se.SetBrush(GeneralTools.GbHole);
                            listPreviewSelectedPoint[idx].SetBrush(GeneralTools.GbHole);
                        }
                        idx++;
                    }
                    StageMouseHoverUpdate = false;
                }

            }
        }
        public void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            UIElement el = sender as UIElement;
            //el.Focus();
            StageMouseHover = true;
            StageMouseHoverUpdate = false;
        }
        public void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            StageMouseHover = false;
            StageMouseHoverUpdate = true;
        }
        public void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

            UIElement el = (UIElement)sender;

            //el.Focus();
            System.Windows.Point pt = e.GetPosition((UIElement)sender);
            MousePoint = new System.Windows.Point(pt.X, pt.Y);
            PrintMousePosition(MousePoint);
            if (IsLockUI)
            {
                return;
            }

            if (ColorPickerOpened)
            {
                return;
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                MouseMove = true;
                RightMouseDown = true;
                if (ZoomScale == 1)
                {

                    return;
                }

                SelectStartPoint = new System.Windows.Point(pt.X, pt.Y);
                SelectEndPoint = new System.Windows.Point(pt.X, pt.Y);

                MoveMousePoint = new System.Windows.Point(pt.X, pt.Y);
                int nOffsetDiffX = (int)(MoveMousePoint.X - CurrentMousePoint.X);
                int nOffsetDiffY = (int)(-MoveMousePoint.Y + CurrentMousePoint.Y);




                //CurrentMousePoint = MoveMousePoint;
                if (Math.Abs(OffsetX + nOffsetDiffX) < 500 * ZoomScale)
                {
                    OffsetX += (int)(MoveMousePoint.X - CurrentMousePoint.X);
                    CurrentMousePoint = new System.Windows.Point(MoveMousePoint.X, CurrentMousePoint.Y);
                }
                if (Math.Abs(OffsetY + nOffsetDiffY) < 500 * ZoomScale)
                {
                    OffsetY -= (int)(MoveMousePoint.Y - CurrentMousePoint.Y);
                    CurrentMousePoint = new System.Windows.Point(CurrentMousePoint.X, MoveMousePoint.Y);
                }

                RedrawStage();
                // UpdateView();
                MouseMove = false;

            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                Mouse.Capture((UIElement)sender, CaptureMode.Element);
                if (Drag)
                {
                    SelectEndPoint = new System.Windows.Point(pt.X, pt.Y);

                    //DrawSelectRect();
                    RedrawStage();
                }
            }
        }
        public void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            int lines = e.Delta * System.Windows.Forms.SystemInformation.MouseWheelScrollLines / 240;

            System.Windows.Point pt = e.GetPosition((UIElement)sender);

            

            int nOffsetDiffX = (int)(CenterX - pt.X);
            int nOffsetDiffY = (int)(-CenterY + pt.Y);

            if (lines > 0 && !IsLockUI)
            {
                if (ZoomScale < 8)
                {
                    if (Math.Abs(OffsetX + nOffsetDiffX) < 500 * ZoomScale)
                    {
                        OffsetX += (int)(CenterX - pt.X);
                        //CurrentMousePoint = new System.Windows.Point(MoveMousePoint.X, CurrentMousePoint.Y);
                        CamelliaStage.RenderCenterX = (pt.X - CenterX);
                    }
                    if (Math.Abs(OffsetY + nOffsetDiffY) < 500 * ZoomScale)
                    {
                        OffsetY -= (int)(CenterY - pt.Y);
                        //CurrentMousePoint = new System.Windows.Point(CurrentMousePoint.X, MoveMousePoint.Y);
                        CamelliaStage.RenderCenterY = pt.Y - CenterY;
                    }
                    
                    //OffsetX += (CenterX - (int)pt.X);
                    //OffsetY += -(CenterY - (int)pt.Y);
                }
                ZoomScale *= 2;

            }
            else if (lines < 0 && !IsLockUI)
            {
                if (Math.Abs(OffsetX + nOffsetDiffX) < 500 * ZoomScale)
                {
                    OffsetX += (int)(CenterX - pt.X);
                    //CurrentMousePoint = new System.Windows.Point(MoveMousePoint.X, CurrentMousePoint.Y);
                    CamelliaStage.RenderCenterX =  pt.X - CenterX;
                }
                if (Math.Abs(OffsetY + nOffsetDiffY) < 500 * ZoomScale)
                {
                    OffsetY -= (int)(CenterY - pt.Y);
                    //CurrentMousePoint = new System.Windows.Point(CurrentMousePoint.X, MoveMousePoint.Y);
                    CamelliaStage.RenderCenterY = pt.Y - CenterY;
                }
                //OffsetX += (CenterX - (int)pt.X);
                //OffsetY += -(CenterY - (int)pt.Y);
                ZoomScale /= 2;
                if (ZoomScale == 1)
                {
                    OffsetX = OffsetY = 0;
                }


            }
            RedrawStage();
        }
        public void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point pt = e.GetPosition((UIElement)sender);

            if (IsLockUI)
            {
                return;
            }
            if (ColorPickerOpened)
            {
                return;
            }

            SelectStartPoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);
            SelectEndPoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);
            LeftMouseDown = true;
            Drag = true;
        }
        public void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UIElement el = (UIElement)sender;
            el.Focus();
            Drag = false;
            System.Windows.Point pt = e.GetPosition((UIElement)sender);
            el.ReleaseMouseCapture();

            if (IsLockUI)
            {
                return;
            }
            if (ColorPickerOpened)
            {
                LeftMouseDown = false;
                ColorPickerOpened = false;
                return;
            }
            if (SetStartEndPointMode)
            {
                MethodStartEndSelect();

                SelectStartPoint = new System.Windows.Point(pt.X, pt.Y);
                SelectEndPoint = new System.Windows.Point(pt.X, pt.Y);
                UpdateListView();
                RedrawStage();
            }
            else
            {
                if (Math.Abs(SelectStartPoint.X - SelectEndPoint.X) > Limit || Math.Abs(SelectStartPoint.Y - SelectEndPoint.Y) > Limit)
                {
                    if (ShiftKeyDown == false && CtrlKeyDown == false)
                    {
                        dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Clear();
                        dataManager.recipeDM.TeachingRD.DataSelectedPoint.Clear();
                    }


                    double dStartSelectX = (SelectStartPoint.X - CenterX - OffsetX) / ZoomScale / RatioX;
                    double dStartSelectY = (-SelectStartPoint.Y + CenterY - OffsetY) / ZoomScale / RatioY;
                    double dEndSelectX = (SelectEndPoint.X - CenterX - OffsetX) / ZoomScale / RatioX;
                    double dEndSelectY = (-SelectEndPoint.Y + CenterY - OffsetY) / ZoomScale / RatioY;

                    double top = Math.Max(dStartSelectY, dEndSelectY);
                    double bottom = Math.Min(dStartSelectY, dEndSelectY);
                    double left = Math.Min(dStartSelectX, dEndSelectX);
                    double right = Math.Max(dStartSelectX, dEndSelectX);

                    foreach (CCircle circle in dataManager.recipeDM.TeachingRD.DataCandidatePoint)
                    {
                        if (circle.x > left && circle.x < right && circle.y > bottom && circle.y < top)
                        {
                            int _index = -1;
                            if (ContainsData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index) && ((ShiftKeyDown && CtrlKeyDown) || !CtrlKeyDown))
                            {
                                DeletePointNotInvalidate(_index, 1);
                                //m_DM.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Delete - Index : " + (_index + 1).ToString()
                                //    + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());
                            }
                            if (!ShiftKeyDown)
                            {
                                if (!ContainsData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index))
                                {
                                    dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                                    dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                                    RouteOrder.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count - 1);
                                }
                                //    m_DM.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Add - Index : " + m_DM.m_RDM.TeachingRD.DataSelectedPoint.Count.ToString()
                                //        + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());
                            }
                        }
                    }
                }
                else
                {
                    if (CurrentCandidatePoint == -1 && CurrentSelectPoint == -1)
                    {
                        MethodPointSelect(pt);
                    }
                    else
                    {
                        MethodCircleSelect();
                    }
                }

                SelectStartPoint = new System.Windows.Point(pt.X, pt.Y);
                SelectEndPoint = new System.Windows.Point(pt.X, pt.Y);
                UpdateListView();
                UpdateView();

            }



            LeftMouseDown = false;
        }
        public void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MousePoint = e.GetPosition((UIElement)sender);
            CurrentMousePoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);
            if (ColorPickerOpened)
            {
                ColorPickerOpened = false;
                return;
            }
        }
        public void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            RightMouseDown = false;
        }
        public void OnMouseMovePreView(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Point pt = e.GetPosition((UIElement)sender);
            PrintMousePositionPreView(pt);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (ZoomScale == 1)
                {
                    return;
                }

                int nPreviewX = (int)(pt.X - CenterX);
                int nPreviewY = (int)(pt.Y - CenterY);

                OffsetX = -(int)((nPreviewX * ZoomScale));
                OffsetY = (int)((nPreviewY * ZoomScale));

                

                RedrawStage();
            }
        }
        public void OnMouseLeftButtonDownPreView(object sender, MouseButtonEventArgs e)
        {
            MousePoint = e.GetPosition((UIElement)sender);

            if (ZoomScale == 1)
            {
                return;
            }

            int nPreviewX = (int)(MousePoint.X - CenterX);
            int nPreviewY = (int)(MousePoint.Y - CenterY);

            OffsetX = -(int)((nPreviewX * ZoomScale));
            OffsetY = (int)((nPreviewY * ZoomScale));


            RedrawStage();
        }
        public void OnMouseWheelPreView(object sender, MouseWheelEventArgs e)
        {
            int lines = e.Delta * System.Windows.Forms.SystemInformation.MouseWheelScrollLines / 240;

            MousePoint = e.GetPosition((UIElement)sender);

            if (lines > 0)
            {
                if (ZoomScale < 32)
                {
                    OffsetX = (int)(CenterX - MousePoint.X) * ZoomScale;
                    OffsetY = -(int)(CenterY - MousePoint.Y) * ZoomScale;
                }
                ZoomScale *= 2;

            }
            else if (lines < 0)
            {
                OffsetX = (int)(CenterX - MousePoint.X) * ZoomScale;
                OffsetY = -(int)(CenterY - MousePoint.Y) * ZoomScale;
                ZoomScale /= 2;
                if (ZoomScale == 1)
                {
                    OffsetX = OffsetY = 0;
                }
            }
            RedrawStage();

        }
        #endregion

        #region Method
        private void InitStage()
        {
            OpenStageCircleHole();
        }

        private void OpenStageCircleHole()
        {
            if (dataStageCircleHole.Count != 0)
            {
                dataStageCircleHole.Clear();
            }

            string fileName = BaseDefine.Dir_StageHole; //Todo 수정해야함
            try
            {
                StreamReader sr = new StreamReader(fileName);
                while (!sr.EndOfStream)
                {
                    string sLine = sr.ReadLine().Trim();
                    string sText = sLine.Substring(sLine.IndexOf(':') + 1);

                    if (sLine == string.Empty || sText == string.Empty)
                    {
                        return;
                    }

                    if (sText.IndexOf('~') == -1)
                    {
                        string[] str = sText.Split(',');

                        Circle circle = new Circle(double.Parse(str[0]), double.Parse(str[1]), double.Parse(str[2]), double.Parse(str[2]));
                        dataStageCircleHole.Add(circle);
                    }
                }
            }
            catch { }
        }

        private void RedrawStage()
        {
            int index = 0;
            //stage.
            CustomEllipseGeometry stageField = Geometry[index] as CustomEllipseGeometry;
            viewStageField.Set(GeneralTools.DataStageField);
            viewStageField.Transform(RatioX, RatioY);
            viewStageField.ScaleOffset(ZoomScale, OffsetX, OffsetY);
            stageField.SetData(viewStageField, CenterX, CenterY);
            Geometry[index] = stageField;
            index++;

            CustomRectangleGeometry rectLine = Geometry[index] as CustomRectangleGeometry;
            viewStageLineHole.Set(GeneralTools.DataStageLineHole);
            viewStageLineHole.Transform(RatioX, RatioY);
            viewStageLineHole.ScaleOffset(ZoomScale, OffsetX, OffsetY);
            rectLine.SetData(drawGeometryManager.GetRect(viewStageLineHole, CenterX, CenterY));
            Geometry[index] = rectLine;
            index++;
            for (int i = 0; i < GeneralTools.GuideLineNum; i++)
            {
                CustomEllipseGeometry guideLine = Geometry[index] as CustomEllipseGeometry;
                ViewStageGuideLine[i].Set(GeneralTools.DataStageGuideLine[i]);
                ViewStageGuideLine[i].Transform(RatioX, RatioY);
                ViewStageGuideLine[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
                guideLine.SetData(ViewStageGuideLine[i], CenterX, CenterY, 5 * ZoomScale);
                Geometry[index] = guideLine;
                index++;
            }

            // 엣지부분 흰색 영역
            for (int i = 0; i < 2 * GeneralTools.EdgeNum; i++)
            {
                viewStageEdgeHoleArc[i].Set(GeneralTools.DataStageEdgeHoleArc[i]);
                viewStageEdgeHoleArc[i].Transform(RatioX, RatioY);
                viewStageEdgeHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
            }

 
            for (int n = 0; n < GeneralTools.EdgeNum; n++)
            {
                CustomPathGeometry edgeArc = Geometry[index] as CustomPathGeometry;

                PathFigure path = drawGeometryManager.AddDoubleHole(viewStageEdgeHoleArc[2 * n + 0], viewStageEdgeHoleArc[2 * n + 1], CenterX, CenterY);

                edgeArc.SetData(path);
                Geometry[index] = edgeArc;
                index++;
                drawGeometryManager.ClearSegments();
            }


            // 긴 타원형 홀
            for (int i = 0; i < 2 * GeneralTools.DoubleHoleNum; i++)
            {
                viewStageDoubleHoleArc[i].Set(GeneralTools.DataStageDoubleHoleArc[i]);
                viewStageDoubleHoleArc[i].Transform(RatioX, RatioY);
                viewStageDoubleHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
            }

            for (int i = 0; i < GeneralTools.DoubleHoleNum; i++)
            {
                CustomPathGeometry doubleHole = Geometry[index] as CustomPathGeometry;
                PathFigure path = drawGeometryManager.AddDoubleHole(viewStageDoubleHoleArc[2 * i + 0], viewStageDoubleHoleArc[2 * i + 1], CenterX, CenterY);

                doubleHole.SetData(path);
                Geometry[index] = doubleHole;
                index++;

                drawGeometryManager.ClearSegments();
            }

            // 윗부분 및 아랫부분 타원홀
            for (int i = 0; i < 2; i++)
            {
                viewStageTopHoleArc[i].Set(GeneralTools.DataStageTopHoleArc[i]);
                viewStageTopHoleArc[i].Transform(RatioX, RatioY);
                viewStageTopHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);

                viewStageBotHoleArc[i].Set(GeneralTools.DataStageBotHoleArc[i]);
                viewStageBotHoleArc[i].Transform(RatioX, RatioY);
                viewStageBotHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
            }

            Arc[] arc;
            for (int i = 0; i < 2; i++)
            {
                CustomPathGeometry topBotDoubleHole = Geometry[index] as CustomPathGeometry;
                if (i == 0)
                {
                    arc = viewStageTopHoleArc;
                }
                else
                {
                    arc = viewStageBotHoleArc;
                }

                PathFigure path = drawGeometryManager.AddDoubleHole(arc[0], arc[1], CenterX, CenterY);

                topBotDoubleHole.SetData(path);
                Geometry[index] = topBotDoubleHole;
                index++;
                drawGeometryManager.ClearSegments();
            }

            // 스테이지 홀
            int idx = 0;
            foreach (Circle circle in dataStageCircleHole)
            {
                CustomEllipseGeometry circleHole = Geometry[index] as CustomEllipseGeometry;
                viewStageCircleHole.Set(circle);
                viewStageCircleHole.Transform(RatioX, RatioY);
                viewStageCircleHole.ScaleOffset(ZoomScale, OffsetX, OffsetY);
                drawGeometryManager.GetRect(ref viewStageCircleHole, CenterX, CenterY);
                circleHole.SetData(viewStageCircleHole, (int)(viewStageCircleHole.Width / 2),
                    (int)(viewStageCircleHole.Y + (viewStageCircleHole.Height / 2) + viewStageCircleHole.Y));
                Geometry[index] = circleHole;
                index++;
                idx++;
            }


            // 스테이지 엣지

            CustomEllipseGeometry stageEdge = Geometry[index] as CustomEllipseGeometry;

            viewStageField.Set(GeneralTools.DataStageField);
            viewStageField.Transform(RatioX, RatioY);
            viewStageField.ScaleOffset(ZoomScale, OffsetX, OffsetY);
            stageEdge.SetData(viewStageField, CenterX, CenterY, 3 * ZoomScale);
            Geometry[index] = stageEdge;

            index++;


            CustomRectangleGeometry lockRect = Geometry[index] as CustomRectangleGeometry;

            Rect shadeRect;
            if (IsLockUI)
            {
                shadeRect = new Rect(0, 0, 1000, 1000);
            }
            else
            {
                shadeRect = new Rect(0, 0, 0, 0);
            }
            lockRect.SetData(shadeRect, 100);
            Geometry[index] = lockRect;
            index++;



            int shapeIndex = 0;
            for (int i = 0; i < dataManager.recipeDM.TeachingRD.DataCandidatePoint.Count; i++)
            {
                ShapeEllipse dataCandidatePoint = Shapes[i] as ShapeEllipse;
                CCircle circle = new CCircle(dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].x, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].y, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].width,
                    dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].height, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].MeasurementOffsetX, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].MeasurementOffsetY);
                circle.Transform(RatioX, RatioY);

                circle.ScaleOffset(ZoomScale, OffsetX, OffsetY);

                Circle c = drawGeometryManager.GetRect(circle, CenterX, CenterY);
                dataCandidatePoint.SetData(c, (int)(circle.width), (int)(circle.height), 95);
                Shapes[i] = dataCandidatePoint;
                shapeIndex++;
            }



            if (ShowRoute)
            {
                for (int i = 0; i < dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Count - 1; i++)
                {
                    ShapeArrowLine arrowLine = Shapes[shapeIndex] as ShapeArrowLine;
                    CCircle from = dataManager.recipeDM.TeachingRD.DataSelectedPoint[dataManager.recipeDM.TeachingRD.DataMeasurementRoute[i]];
                    CCircle to = dataManager.recipeDM.TeachingRD.DataSelectedPoint[dataManager.recipeDM.TeachingRD.DataMeasurementRoute[i + 1]];

                    from.Transform(RatioX, RatioY);
                    from.ScaleOffset(ZoomScale, OffsetX, OffsetY);

                    to.Transform(RatioX, RatioY);
                    to.ScaleOffset(ZoomScale, OffsetX, OffsetY);

                    PointF[] line = { new PointF((float)from.x + CenterX, (float)-from.y + CenterY), new PointF((float)to.x + CenterX, (float)-to.y + CenterY) };
                    arrowLine.SetData(line, routeBrush, (int)to.width, RouteThick * ZoomScale, 0, 97);
                    Shapes[shapeIndex] = arrowLine;
                    shapeIndex++;
                }
            }


            int nDummyIdx = -1;
            for (int i = 0; i < dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count; i++)
            {
                ShapeEllipse dataSelectedPoint = Shapes[shapeIndex] as ShapeEllipse;
                CCircle circle = new CCircle(dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].x, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].y, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].width,
                    dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].height, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].MeasurementOffsetX, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].MeasurementOffsetY);
                circle.Transform(RatioX, RatioY);

                circle.ScaleOffset(ZoomScale, OffsetX, OffsetY);
                Circle c = drawGeometryManager.GetRect(circle, CenterX, CenterY);
                dataSelectedPoint.SetData(c, (int)(circle.width), (int)(circle.height), 96, true);
                if (SetStartEndPointMode)
                {
                    CCircle reorderCircle = dataManager.recipeDM.TeachingRD.DataSelectedPoint[i];
                    if (ContainsData(ListReorderPoint, reorderCircle, out nDummyIdx))
                    {
                        dataSelectedPoint.SetBrush(System.Windows.Media.Brushes.Cyan);
                    }
                    else
                    {
                        dataSelectedPoint.SetBrush(System.Windows.Media.Brushes.DarkBlue);
                    }
                }
                else
                {
                    dataSelectedPoint.SetBrush(GeneralTools.GbHole);
                }
                Shapes[shapeIndex] = dataSelectedPoint;
                shapeIndex++;

                if (IsShowIndex || IsKeyboardShowIndex)
                {
                    if (TextBlocks.Count > 0)
                    {
                        TextManager textBlock = TextBlocks[i];
                        textBlock.SetData((RouteOrder[i] + 1).ToString(), (int)c.Width, 98, dataSelectedPoint.CanvasLeft, dataSelectedPoint.CanvasTop - c.Height);
                        textBlock.SetVisibility(true);
                        TextBlocks[i] = textBlock;
                    }
                }
                else if (!IsShowIndex && !IsKeyboardShowIndex)
                {
                    if (TextBlocks.Count > 0)
                    {
                        TextManager textBlock = TextBlocks[i];
                        textBlock.SetVisibility(false);
                        TextBlocks[i] = textBlock;
                    }
                }

            }


            Rect rect = DataViewPosition;
            rect.X = rect.X / ZoomScale - OffsetX / ZoomScale * 1000 / (double)1000;
            rect.Y = rect.Y / ZoomScale - OffsetY / ZoomScale * 1000 / (double)1000;
            rect.Width /= ZoomScale;
            rect.Height /= ZoomScale;

            Rect ViewRect = drawGeometryManager.GetRect(rect, CenterX, CenterY);
            if (ViewRect.X < 0)
            {
                if (ViewRect.Width + ViewRect.X < 0)
                {
                    ViewRect.Width = 0;
                }
                else
                {
                    ViewRect.Width += ViewRect.X;
                }
                ViewRect.X = 0;
            }
            if (ViewRect.Y < 0)
            {

                if (ViewRect.Height + ViewRect.Y < 0)
                {
                    ViewRect.Height = 0;
                }
                else
                {
                    ViewRect.Height += ViewRect.Y;
                }
                ViewRect.Y = 0;
            }
            double dHeight = 1000 - ViewRect.Y - ViewRect.Height;
            if (ViewRect.Y + ViewRect.Height > 1000)
            {
                dHeight = 0;
            }
            double dWidth = 1000 - ViewRect.X - ViewRect.Width;
            if (ViewRect.X + ViewRect.Width > 1000)
            {
                dWidth = 0;
            }
            Rect TopRect = new Rect(ViewRect.X, 0, ViewRect.Width, ViewRect.Y);
            Rect BottomRect = new Rect(ViewRect.X, ViewRect.Y + ViewRect.Height, ViewRect.Width, dHeight);
            Rect LeftRect = new Rect(0, 0, ViewRect.X, 1000);
            Rect RightRect = new Rect(ViewRect.X + ViewRect.Width, 0, dWidth, 1000);

            CustomRectangleGeometry stageShade = ViewRectGeometry[0] as CustomRectangleGeometry;
            stageShade.SetGroupData(TopRect, 0);
            stageShade.SetGroupData(BottomRect, 1);
            stageShade.SetGroupData(LeftRect, 2);
            stageShade.SetGroupData(RightRect, 3);
            ViewRectGeometry[0] = stageShade;

            if (IsLockUI)
            {
                LockImage.Visibility = Visibility.Visible;
            }
            else
            {
                LockImage.Visibility = Visibility.Hidden;
            }

            // select Rect
            CustomRectangleGeometry select = SelectGeometry[0] as CustomRectangleGeometry;
            Rect selectRect = new Rect(Math.Min(SelectStartPoint.X, SelectEndPoint.X), Math.Min(SelectStartPoint.Y, SelectEndPoint.Y),
                Math.Abs(SelectStartPoint.X - SelectEndPoint.X), Math.Abs(SelectStartPoint.Y - SelectEndPoint.Y));
            select.SetData(selectRect, 99);
            SelectGeometry[0] = select;
        }

        public void SetSelectRect()
        {
                selectRectangle = new CustomRectangleGeometry(GeneralTools.SelectPointBrush);
                CustomRectangleGeometry select = selectRectangle as CustomRectangleGeometry;

                Rect selectRect = new Rect(Math.Min(SelectStartPoint.X, SelectEndPoint.X), Math.Min(SelectStartPoint.Y, SelectEndPoint.Y),
                    Math.Abs(SelectStartPoint.X - SelectEndPoint.X), Math.Abs(SelectStartPoint.Y - SelectEndPoint.Y));

                select.SetData(selectRect, 99);
                SelectGeometry.Add(select);
                m_DrawSelectElement.Add(select.path);
        }

        public void UpdateView(bool MeasurementLoad = false, bool bMain = false)
        {
            
            //dataManager.recipeDM.TeachingRD.Clone(dataManager.recipeDM.MeasurementRD);
            
            if (bMain)
            {
                ZoomScale = 1;
                OffsetX = 0;
                OffsetY = 0;
                m_DrawElement.Clear();
                Geometry.Clear();
                SetStage();
                dataManager.recipeDM.TeachingRD = new RecipeData();
                p_UseThickness = p_SettingViewModel.p_UseThickness;
            }
            RecipeData data = dataManager.recipeDM.TeachingRD;
            if (MeasurementLoad)
            {
                dataManager.recipeDM.MeasurementRD.Clone(dataManager.recipeDM.TeachingRD);
                UpdateParameter();
            }
            CurrentCandidatePoint = -1;
            CurrentSelectPoint = -1;

            p_DrawPointElement.Clear();
            Shapes.Clear();
            TextBlocks.Clear();
            p_DrawSelectElement.Clear();
            SelectGeometry.Clear();
            SetPoint(false, data);
            SetSelectRect();

            p_PreviewDrawElement.Clear();
            previewTemp.Clear();
            //SetStage(true);
            SetPoint(true, data);

            ViewRectGeometry.Clear();
            SetViewRect();

            p_PreviewDrawElement = new ObservableCollection<UIElement>(previewTemp);

        }

        public void UpdateListView(bool MeasurementLoad = false)
        {
            RecipeData data = dataManager.recipeDM.TeachingRD;
            if (MeasurementLoad)
            {
                dataManager.recipeDM.MeasurementRD.Clone(dataManager.recipeDM.TeachingRD);
            }
            RecipePath = dataManager.recipeDM.TeachingRecipePath;
            PointListItem.Clear();
            RouteOrder.Clear();
            int nCount = 0;
            int nSelCnt = dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count;
            int[] MeasurementOrder = new int[nSelCnt];

            for (int i = 0; i < nSelCnt; i++)
            {
                MeasurementOrder[dataManager.recipeDM.TeachingRD.DataMeasurementRoute[i]] = i;
            }

            for (int i = 0; i < MeasurementOrder.Count(); i++)
            {
                RouteOrder.Add(MeasurementOrder[i]);
            }

            DataRow row;
            for (int i = 0; i < nSelCnt; i++, nCount++)
            {

                CCircle c = dataManager.recipeDM.TeachingRD.DataSelectedPoint[i];
                int nRoute = MeasurementOrder[i];
                row = PointListItem.NewRow();
                row["ListIndex"] = (nCount + 1).ToString();
                row["ListX"] = Math.Round(c.x, 3).ToString();
                row["ListY"] = Math.Round(c.y, 3).ToString();
                row["ListRoute"] = (nRoute + 1).ToString();
                PointListItem.Rows.Add(row);
            }
            PointCount = PointListItem.Rows.Count.ToString();
        }

        public void UpdateLayerGridView()
        {
            MaterialListItem.Clear();
            GridLayerData.Clear();
            LayerCount = 2;
            if (dataManager.recipeDM.TeachingRD.ModelRecipePath != "")
            {
                dataManager.recipeDM.LoadModel(dataManager.recipeDM.TeachingRD.ModelRecipePath);
                MaterialListItem = new ObservableCollection<string>(dataManager.recipeDM.ModelData.MaterialList.ToArray());
                ModelPath = dataManager.recipeDM.TeachingRD.ModelRecipePath;
            }
            else
            {
                dataManager.recipeDM.ModelData.MaterialList.Clear();
                App.m_nanoView.m_MaterialList.Clear();
                //LibSR_Met.DataManager.GetInstance().m_LayerData.Clear();
                
                //App.m_nanoView..Clear();
                InitLayer();
            }
            InitLayerGrid();    

            DeleteGridCombo();
            UpdateGridCombo();

            UpdateLayerGrid();
        }

        public void InitLayer()
        {
            App.m_nanoView.m_LayerList.Clear();
            dataManager.recipeDM.ModelData.AddLayer();
            dataManager.recipeDM.ModelData.AddLayer();
        }

        private void UpdateParameter()
        {
            UpdateMeasurementParameter();
            UpdateThicknessParameter();
        }

        private void UpdateMeasurementParameter()
        {
            NIRIntegrationTime = dataManager.recipeDM.TeachingRD.NIRIntegrationTime.ToString();
            VISIntegrationTime = dataManager.recipeDM.TeachingRD.VISIntegrationTime.ToString();

            p_UseThickness = dataManager.recipeDM.TeachingRD.UseThickness;
            p_UseTransmittance = dataManager.recipeDM.TeachingRD.UseTransmittance;

            WaveLengthValue = "0.0";
            ReflectanceListItem.Clear();
            ReflectanceSelectedIndex = -1;
            for (int i = 0; i < dataManager.recipeDM.TeachingRD.WaveLengthReflectance.Count; i++)
            {
                ReflectanceListItem.Add(dataManager.recipeDM.TeachingRD.WaveLengthReflectance[i]);
            }

            TransmittanceListItem.Clear();
            TransmittanceSelectedIndex = -1;
            for (int i = 0; i < dataManager.recipeDM.TeachingRD.WaveLengthTransmittance.Count; i++)
            {
                TransmittanceListItem.Add(dataManager.recipeDM.TeachingRD.WaveLengthTransmittance[i]);
            }
        }
        private void UpdateThicknessParameter()
        {
            LowerWaveLength = dataManager.recipeDM.TeachingRD.LowerWaveLength;
            UpperWaveLength = dataManager.recipeDM.TeachingRD.UpperWaveLength;
            ThicknessLMIteration = dataManager.recipeDM.TeachingRD.LMIteration;
            DampingFactor = dataManager.recipeDM.TeachingRD.DampingFactor.ToString();
        }

        private void SetViewRect()
        {
            //Preview Stage Shade
            {
                Rect rect = DataViewPosition;
                rect.X = rect.X / ZoomScale - OffsetX / ZoomScale * 1000 / (double)1000;
                rect.Y = rect.Y / ZoomScale - OffsetY / ZoomScale * 1000 / (double)1000;
                rect.Width /= ZoomScale;
                rect.Height /= ZoomScale;

                Rect ViewRect = drawGeometryManager.GetRect(rect, CenterX, CenterY);
                if (ViewRect.X < 0)
                {
                    if (ViewRect.Width + ViewRect.X < 0)
                    {
                        ViewRect.Width = 0;
                    }
                    else
                    {
                        ViewRect.Width += ViewRect.X;
                    }
                    ViewRect.X = 0;
                }
                if (ViewRect.Y < 0)
                {

                    if (ViewRect.Height + ViewRect.Y < 0)
                    {
                        ViewRect.Height = 0;
                    }
                    else
                    {
                        ViewRect.Height += ViewRect.Y;
                    }
                    ViewRect.Y = 0;
                }
                double dHeight = 1000 - ViewRect.Y - ViewRect.Height;
                if (ViewRect.Y + ViewRect.Height > 1000)
                {
                    dHeight = 0;
                }
                double dWidth = 1000 - ViewRect.X - ViewRect.Width;
                if (ViewRect.X + ViewRect.Width > 1000)
                {
                    dWidth = 0;
                }
                Rect TopRect = new Rect(ViewRect.X, 0, ViewRect.Width, ViewRect.Y);
                Rect BottomRect = new Rect(ViewRect.X, ViewRect.Y + ViewRect.Height, ViewRect.Width, dHeight);
                Rect LeftRect = new Rect(0, 0, ViewRect.X, 1000);
                Rect RightRect = new Rect(ViewRect.X + ViewRect.Width, 0, dWidth, 1000);

                viewRect = new CustomRectangleGeometry(GeneralTools.StageShadeBrush);
                CustomRectangleGeometry stageShade = viewRect as CustomRectangleGeometry;

                stageShade.SetData(TopRect);
                stageShade.AddGroup(stageShade);

                CustomRectangleGeometry botRect = new CustomRectangleGeometry(GeneralTools.StageShadeBrush);
                botRect.SetData(BottomRect);
                stageShade.AddGroup(botRect);

                CustomRectangleGeometry leftRect = new CustomRectangleGeometry(GeneralTools.StageShadeBrush);
                leftRect.SetData(LeftRect);
                stageShade.AddGroup(leftRect);

                CustomRectangleGeometry rightRect = new CustomRectangleGeometry(GeneralTools.StageShadeBrush);
                rightRect.SetData(RightRect);
                stageShade.AddGroup(rightRect);
                ViewRectGeometry.Add(stageShade);
                //p_PreviewDrawElement.Add(stageShade.path);
                previewTemp.Add(stageShade.path);
            }
        }

        public void SetPoint(bool preview, RecipeData recipe)
        {
            ObservableCollection<UIElement> temp = new ObservableCollection<UIElement>();
            if (!preview)
            {
                listCandidatePoint.Clear();
            }
            else
            {
                listPreviewCandidatePoint.Clear();
            }

            for (int i = 0; i < recipe.DataCandidatePoint.Count; i++)
            {
                dataPoint = new ShapeEllipse(GeneralTools.StageHoleBrush);
                ShapeEllipse dataCandidatePoint = dataPoint as ShapeEllipse;

                CCircle circle = new CCircle(recipe.DataCandidatePoint[i].x, recipe.DataCandidatePoint[i].y, recipe.DataCandidatePoint[i].width,
                    recipe.DataCandidatePoint[i].height, recipe.DataCandidatePoint[i].MeasurementOffsetX, recipe.DataCandidatePoint[i].MeasurementOffsetY);
                circle.Transform(RatioX, RatioY);

                if (!preview)
                {
                    circle.ScaleOffset(ZoomScale, OffsetX, OffsetY);
                }
                Circle c = drawGeometryManager.GetRect(circle, CenterX, CenterY);
                dataCandidatePoint.SetData(c, (int)(circle.width), (int)(circle.height), 95);
                if (!preview)
                {
                    Shapes.Add(dataCandidatePoint);
                    temp.Add(dataCandidatePoint.UIElement);
                    listCandidatePoint.Add(dataCandidatePoint);
                }
                else
                {
                    PreviewShapes.Add(dataCandidatePoint);
                    previewTemp.Add(dataCandidatePoint.UIElement);
                    listPreviewCandidatePoint.Add(dataCandidatePoint);
                }
               
            }
           

            if (!preview)
            {
                if (ShowRoute)
                {
                    for (int i = 0; i < recipe.DataMeasurementRoute.Count - 1; i++)
                    {
                        dataRoute = new ShapeArrowLine(RouteBrush, RouteThick * ZoomScale);
                        ShapeArrowLine arrowLine = dataRoute as ShapeArrowLine;
                        CCircle from = recipe.DataSelectedPoint[recipe.DataMeasurementRoute[i]];
                        CCircle to = recipe.DataSelectedPoint[recipe.DataMeasurementRoute[i + 1]];

                        from.Transform(RatioX, RatioY);
                        from.ScaleOffset(ZoomScale, OffsetX, OffsetY);

                        to.Transform(RatioX, RatioY);
                        to.ScaleOffset(ZoomScale, OffsetX, OffsetY);

                        PointF[] line = { new PointF((float)from.x + CenterX, (float)-from.y + CenterY), new PointF((float)to.x + CenterX, (float)-to.y + CenterY) };
                        arrowLine.SetData(line, routeBrush, (int)to.width, RouteThick * ZoomScale, 0, 97);
                        Shapes.Add(arrowLine);
                        //p_DrawPointElement.Add(arrowLine.UIElement);
                        temp.Add(arrowLine.UIElement);
                    }
                }
            }

            if (!preview)
            {
                listSelectedPoint.Clear();
            }
            else
            {
                listPreviewSelectedPoint.Clear();
            }
            int nDummyIdx = -1;
            for (int i = 0; i < recipe.DataSelectedPoint.Count; i++)
            {
                dataPoint = new ShapeEllipse(GeneralTools.GbHole);
                ShapeEllipse dataSelectedPoint = dataPoint as ShapeEllipse;
                CCircle circle = new CCircle(recipe.DataSelectedPoint[i].x, recipe.DataSelectedPoint[i].y, recipe.DataSelectedPoint[i].width,
                    recipe.DataSelectedPoint[i].height, recipe.DataSelectedPoint[i].MeasurementOffsetX, recipe.DataSelectedPoint[i].MeasurementOffsetY);
                circle.Transform(RatioX, RatioY);
                if (!preview)
                {
                    circle.ScaleOffset(ZoomScale, OffsetX, OffsetY);
                }

                Circle c = drawGeometryManager.GetRect(circle, CenterX, CenterY);
                dataSelectedPoint.SetData(c, (int)(circle.width), (int)(circle.height), 96, true);
                if (SetStartEndPointMode)
                {
                    CCircle reorderCircle = dataManager.recipeDM.TeachingRD.DataSelectedPoint[i];
                    if (ContainsData(ListReorderPoint, reorderCircle, out nDummyIdx))
                    {
                        dataSelectedPoint.SetBrush(System.Windows.Media.Brushes.Cyan);
                    }
                    else
                    {
                        dataSelectedPoint.SetBrush(System.Windows.Media.Brushes.DarkBlue);
                    }
                }
                if (!preview)
                {
                    Shapes.Add(dataSelectedPoint);
                    //p_DrawPointElement.Add(dataSelectedPoint.UIElement);
                    temp.Add(dataSelectedPoint.UIElement);
                    listSelectedPoint.Add(dataSelectedPoint);
                    textManager = new TextManager(new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 255)));
                    textManager.SetData((RouteOrder[i] + 1).ToString(), (int)c.Width, 98, dataSelectedPoint.CanvasLeft, dataSelectedPoint.CanvasTop - c.Height);
                    if (!IsShowIndex && !IsKeyboardShowIndex)
                    {
                        textManager.SetVisibility(false);
                    }
                    TextBlocks.Add(textManager);

                    //p_DrawPointElement.Add(textManager.Text);
                    temp.Add(textManager.Text);
                }
                else
                {
                    PreviewShapes.Add(dataSelectedPoint);
                    previewTemp.Add(dataSelectedPoint.UIElement);
                    listPreviewSelectedPoint.Add(dataSelectedPoint);
                }
            }

            if (!preview)
            {
                p_DrawPointElement = temp;
            }
        }

        public void SetStage()
        {
            ObservableCollection<UIElement> temp = new ObservableCollection<UIElement>();
            GeneralTools.GbHole.GradientOrigin = new System.Windows.Point(0.3, 0.3);
            // 스테이지

            stage = new CustomEllipseGeometry(GeneralTools.Gb, System.Windows.SystemColors.ControlBrush);
            CustomEllipseGeometry stageField = stage as CustomEllipseGeometry;

            viewStageField.Set(GeneralTools.DataStageField);
            viewStageField.Transform(RatioX, RatioY);
            viewStageField.ScaleOffset(ZoomScale, OffsetX, OffsetY);
            stageField.SetData(viewStageField, CenterX, CenterY);

            Geometry.Add(stageField);
            temp.Add(stageField.path);



            // Stage 중간 흰색 라인
            stage = new CustomRectangleGeometry(GeneralTools.ActiveBrush, GeneralTools.ActiveBrush);
            CustomRectangleGeometry rectLine = stage as CustomRectangleGeometry;
            viewStageLineHole.Set(GeneralTools.DataStageLineHole);
            viewStageLineHole.Transform(RatioX, RatioY);

            viewStageLineHole.ScaleOffset(ZoomScale, OffsetX, OffsetY);

            rectLine.SetData(drawGeometryManager.GetRect(viewStageLineHole, CenterX, CenterY));

            Geometry.Add(rectLine);
            temp.Add(rectLine.path);


            // Stage 점선 가이드라인
            for (int i = 0; i < GeneralTools.GuideLineNum; i++)
            {

                stage = new CustomEllipseGeometry(GeneralTools.GuideLineBrush, "3,1", 5, 0.1d);

                CustomEllipseGeometry guideLine = stage as CustomEllipseGeometry;
                ViewStageGuideLine[i] = new Circle();
                ViewStageGuideLine[i].Set(GeneralTools.DataStageGuideLine[i]);
                ViewStageGuideLine[i].Transform(RatioX, RatioY);

                ViewStageGuideLine[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
                guideLine.SetData(ViewStageGuideLine[i], CenterX, CenterY, 5 * ZoomScale);
                Geometry.Add(guideLine);
                temp.Add(guideLine.path);

            }

            // 엣지부분 흰색 영역
            for (int i = 0; i < 2 * GeneralTools.EdgeNum; i++)
            {
                viewStageEdgeHoleArc[i] = new Arc();
                viewStageEdgeHoleArc[i].Set(GeneralTools.DataStageEdgeHoleArc[i]);
                viewStageEdgeHoleArc[i].Transform(RatioX, RatioY);
                viewStageEdgeHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
            }

            for (int n = 0; n < GeneralTools.EdgeNum; n++)
            {
                stage = new CustomPathGeometry(GeneralTools.ActiveBrush);
                CustomPathGeometry edgePath = stage as CustomPathGeometry;

                PathFigure path = drawGeometryManager.AddDoubleHole(viewStageEdgeHoleArc[2 * n + 0], viewStageEdgeHoleArc[2 * n + 1], CenterX, CenterY);

                edgePath.SetData(path);
                Geometry.Add(edgePath);
                temp.Add(edgePath.path);
                drawGeometryManager.ClearSegments();
            }


            // 긴 타원형 홀
            for (int i = 0; i < 2 * GeneralTools.DoubleHoleNum; i++)
            {

                viewStageDoubleHoleArc[i] = new Arc();
                viewStageDoubleHoleArc[i].Set(GeneralTools.DataStageDoubleHoleArc[i]);
                viewStageDoubleHoleArc[i].Transform(RatioX, RatioY);
                viewStageDoubleHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
            }

            for (int i = 0; i < GeneralTools.DoubleHoleNum; i++)
            {
                stage = new CustomPathGeometry(GeneralTools.ActiveBrush);
                CustomPathGeometry doubleHole = stage as CustomPathGeometry;

                PathFigure path = drawGeometryManager.AddDoubleHole(viewStageDoubleHoleArc[2 * i + 0], viewStageDoubleHoleArc[2 * i + 1], CenterX, CenterY);

                doubleHole.SetData(path);
                Geometry.Add(doubleHole);
                temp.Add(doubleHole.path);
                drawGeometryManager.ClearSegments();
            }

            // 윗부분 및 아랫부분 타원홀
            for (int i = 0; i < 2; i++)
            {
                viewStageTopHoleArc[i] = new Arc();
                viewStageTopHoleArc[i].Set(GeneralTools.DataStageTopHoleArc[i]);
                viewStageTopHoleArc[i].Transform(RatioX, RatioY);
                viewStageTopHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
                viewStageBotHoleArc[i] = new Arc();
                viewStageBotHoleArc[i].Set(GeneralTools.DataStageBotHoleArc[i]);
                viewStageBotHoleArc[i].Transform(RatioX, RatioY);
                viewStageBotHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
            }

            Arc[] arc;
            for (int i = 0; i < 2; i++)
            {
                stage = new CustomPathGeometry(GeneralTools.ActiveBrush);
                CustomPathGeometry topBotDoubleHole = stage as CustomPathGeometry;
                if (i == 0)
                {
                    arc = viewStageTopHoleArc;
                }
                else
                {
                    arc = viewStageBotHoleArc;
                }

                PathFigure path = drawGeometryManager.AddDoubleHole(arc[0], arc[1], CenterX, CenterY);

                topBotDoubleHole.SetData(path);
                Geometry.Add(topBotDoubleHole);
                temp.Add(topBotDoubleHole.path);
                drawGeometryManager.ClearSegments();
            }


            // 스테이지 홀
            foreach (Circle circle in dataStageCircleHole)
            {
                stage = new CustomEllipseGeometry(GeneralTools.ActiveBrush, GeneralTools.ActiveBrush);
                CustomEllipseGeometry circleHole = stage as CustomEllipseGeometry;
                viewStageCircleHole.Set(circle);
                viewStageCircleHole.Transform(RatioX, RatioY);
                viewStageCircleHole.ScaleOffset(ZoomScale, OffsetX, OffsetY);
                drawGeometryManager.GetRect(ref viewStageCircleHole, CenterX, CenterY);
                circleHole.SetData(viewStageCircleHole, (int)(viewStageCircleHole.Width / 2),
                    (int)(viewStageCircleHole.Y + (viewStageCircleHole.Height / 2) + viewStageCircleHole.Y));
                Geometry.Add(circleHole);
                temp.Add(circleHole.path);
            }


            // 스테이지 엣지


            stage = new CustomEllipseGeometry(System.Windows.SystemColors.ControlBrush, 3);

            CustomEllipseGeometry stageEdge = stage as CustomEllipseGeometry;

            viewStageField.Set(GeneralTools.DataStageField);
            viewStageField.Transform(RatioX, RatioY);
            viewStageField.ScaleOffset(ZoomScale, OffsetX, OffsetY);

            stageEdge.SetData(viewStageField, CenterX, CenterY, 3 * ZoomScale);
            Geometry.Add(stageEdge);
            temp.Add(stageEdge.path);

            stage = new CustomRectangleGeometry(GeneralTools.StageShadeBrush, GeneralTools.StageShadeBrush);
            CustomRectangleGeometry lockRect = stage as CustomRectangleGeometry;
            Rect shadeRect = new Rect(0, 0, 0, 0);
            lockRect.SetData(shadeRect, 100);
            Geometry.Add(lockRect);
            //p_DrawElement.Add(lockRect.path);
            temp.Add(lockRect.path);

            LockImage.Source = new BitmapImage(new Uri(BaseDefine.Dir_LockImg, UriKind.RelativeOrAbsolute));
            LockImage.Width = 100;
            LockImage.Visibility = Visibility.Hidden;
            Canvas.SetLeft(LockImage, 850);
            Canvas.SetTop(LockImage, 50);
            //m_DrawElement.Add(LockImage);
            temp.Add(LockImage);

            p_DrawElement = temp;
        }

        public void RouteOptimizaionFunc()
        {
            int nTotalPoint = dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count;
            if (nTotalPoint == 0)
            {
                return;
            }
            double[,] distance = new double[nTotalPoint, nTotalPoint];
            List<int> listPoint = new List<int>();
            List<int> listWentPoint = new List<int>();
            List<double[,]> listDoublePoint = new List<double[,]>();

            int nCurrentIdx = 0;
            List<Data> data = new List<Data>();
            for (int i = 0; i < nTotalPoint; i++)
            {
                for (int j = 0; j < nTotalPoint; j++)
                {
                    double x = Math.Pow(dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].x - dataManager.recipeDM.TeachingRD.DataSelectedPoint[j].x, 2);
                    double y = Math.Pow(dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].y - dataManager.recipeDM.TeachingRD.DataSelectedPoint[j].y, 2);
                    distance[i, j] = Math.Sqrt(x + y);
                    data.Add(new Data(i, j, distance[i, j]));
                }
                listPoint.Add(0);
                listWentPoint.Add(0);
            }
            data.Sort((x1, x2) => x1.Dist.CompareTo(x2.Dist));
            foreach (var dt in data)
            {
                if (dt.Dist != 0)
                    Console.WriteLine(dt.Current + "->" + dt.Next + ", Dist : " + dt.Dist);
            }

            int min_index = 0;
            int currentPoint = 0;
            listWentPoint[0] = 1;
            if (SetStartEndPointMode)
            {
                nCurrentIdx = ListReorderPoint.Count;
                for (int i = 0; i < nCurrentIdx; i++)
                {
                    listWentPoint[i] = 1;
                    listPoint[i] = i;
                }
                if (nCurrentIdx == 0 || nCurrentIdx == 1)
                {
                    currentPoint = nCurrentIdx = 0;
                }
                else
                {
                    currentPoint = nCurrentIdx -= 1;
                }
            }


            for (int i = nCurrentIdx + 1; i < nTotalPoint; i++)
            {
                double min_dist = 1000000;
                for (int j = 0; j < nTotalPoint; j++)
                {
                    if (listWentPoint[j] == 0 && distance[currentPoint, j] < min_dist)
                    {
                        min_dist = distance[currentPoint, j];
                        min_index = j;
                    }
                }
                listPoint[i] = min_index;
                listWentPoint[min_index] = 1;
                currentPoint = min_index;
            }
            dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Clear();
            for (int i = 0; i < dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count; i++)
            {
                dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(listPoint[i]);
            }

            UpdateListView();
            UpdateView();


        }

        public void InitKeyButton()
        {
            IsKeyboardShowIndex = false;
            SBrush = normalBrush;
            CtrlKeyDown = false;
            CtrlBrush = normalBrush;
            ShiftKeyDown = false;
            ShiftBrush = normalBrush;
            PointAddMode = "Normal";
        }

        public void ReadPreset(string presetName)
        {
            dataManager.recipeDM.TeachingRD.ClearPoint();
            dataManager.recipeDM.PresetData = (PresetData)GeneralFunction.Read(dataManager.recipeDM.PresetData, BaseDefine.Dir_Preset + presetName);
            dataManager.recipeDM.TeachingRD.DataCandidatePoint = dataManager.recipeDM.PresetData.DataCandidatePoint;
            dataManager.recipeDM.TeachingRD.DataSelectedPoint = dataManager.recipeDM.PresetData.DataSelectedPoint;
            dataManager.recipeDM.TeachingRD.DataMeasurementRoute = dataManager.recipeDM.PresetData.DataMeasurementRoute;

            dataManager.recipeDM.TeachingRD.CheckCircleSize();
            CheckSelectedPoint();
        }

        public void CheckSelectedPoint()
        {
            int index;
            List<CCircle> temp = new List<CCircle>();
            foreach (var item in dataManager.recipeDM.TeachingRD.DataSelectedPoint)
            {
                temp.Add(item);
            }
            dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Clear();
            dataManager.recipeDM.TeachingRD.DataSelectedPoint.Clear();
            foreach (var item in temp)
            {
                if (ContainsData(dataManager.recipeDM.TeachingRD.DataCandidatePoint, item, out index))
                {
                    dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                    dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(item);
                }
            }
        }

        public bool ContainsData(List<CCircle> list, CCircle circle, out int nIndex)
        {
            bool bRst = false;
            nIndex = -1;

            int nCount = 0;
            foreach (var item in list)
            {
                if (Math.Round(item.x, 3) == Math.Round(circle.x, 3) && Math.Round(item.y, 3) == Math.Round(circle.y, 3)
                    && Math.Round(item.width, 3) == Math.Round(circle.width, 3) && Math.Round(item.height, 3) == Math.Round(circle.height, 3))
                {
                    bRst = true;
                    nIndex = nCount;
                }
                nCount++;
            }
            return bRst;
        }

        private void MethodPointSelect(System.Windows.Point pt)
        {
            double dDistance = 0;
            double dMin = 9999;
            int nIndex = 0;
            int nMinIndex = -1;

            foreach (ShapeEllipse se in listCandidatePoint)
            {
                dDistance = GetDistance(se, new System.Windows.Point(pt.X, pt.Y));

                if (dDistance < dMin)
                {
                    dMin = dDistance;
                    nMinIndex = nIndex;
                }
                nIndex++;
            }

            if (nMinIndex != -1)
            {
                AddPoint(nMinIndex);
            }
        }

        private double GetDistance(ShapeEllipse eg, System.Windows.Point pt)
        {
            double dResult = Math.Sqrt(Math.Pow(eg.CenterX - pt.X, 2) + Math.Pow(eg.CenterY - pt.Y, 2));

            return Math.Round(dResult, 3);
        }

        public void DeletePointNotInvalidate(int nIndex, int nRange)
        {
            for (int i = nIndex; i < nIndex + nRange; i++)
            {
                double dPointX = dataManager.recipeDM.TeachingRD.DataSelectedPoint[nIndex].x;
                double dPointY = dataManager.recipeDM.TeachingRD.DataSelectedPoint[nIndex].y;

                dataManager.recipeDM.TeachingRD.DataSelectedPoint.RemoveAt(nIndex);
                int nDeleteIndex = dataManager.recipeDM.TeachingRD.DataMeasurementRoute.FindIndex(s => s.Equals(nIndex));
                dataManager.recipeDM.TeachingRD.DataMeasurementRoute.RemoveAt(nDeleteIndex);
                for (int j = 0; j < dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Count; j++)
                {
                    if (dataManager.recipeDM.TeachingRD.DataMeasurementRoute[j] > nIndex)
                    {
                        dataManager.recipeDM.TeachingRD.DataMeasurementRoute[j] = dataManager.recipeDM.TeachingRD.DataMeasurementRoute[j] - 1;
                    }
                }
            }
        }

        private void MethodStartEndSelect()
        {
            if (CurrentSelectPoint != -1)
            {

                int index = -1;
                int nReorderIdx = -1;
                CCircle circle = dataManager.recipeDM.TeachingRD.DataSelectedPoint[CurrentSelectPoint];

                if (ContainsData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out index) && !ContainsData(ListReorderPoint, circle, out nReorderIdx))
                {
                    DeletePoint(index, 1);
                    dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                    dataManager.recipeDM.TeachingRD.DataSelectedPoint.Insert(ReorderCnt, circle);
                    ListReorderPoint.Add(circle);
                    dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Sort();

                    ReorderCnt++;

                    if (ReorderCnt == dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count)
                    {
                        SetStartEndPointMode = false;
                        ReorderCnt = 0;
                        ReorderBrush = normalBrush;
                    }
                }
            }
        }

        private void MethodCircleSelect()
        {
            double dOffsetX = 0;
            double dOffsetY = 0;
            if (CurrentCandidatePoint != -1)
            {
                CCircle circle = dataManager.recipeDM.TeachingRD.DataCandidatePoint[CurrentCandidatePoint];
                circle.MeasurementOffsetX = dOffsetX;
                circle.MeasurementOffsetY = dOffsetY;

                int _index = -1;
                if (ContainsSelectedData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index) && ((ShiftKeyDown && CtrlKeyDown) || !CtrlKeyDown))
                {
                    DeletePoint(_index, 1);
                }
                else
                {
                    if (!ShiftKeyDown)
                    {
                        if (!ContainsData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index))
                        {
                            dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                            dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                            RouteOrder.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count - 1);
                        }
                    }
                }
            }
            if (CurrentSelectPoint != -1)
            {
                CCircle circle = dataManager.recipeDM.TeachingRD.DataSelectedPoint[CurrentSelectPoint];

                int _index = -1;
                if (ContainsSelectedData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index) && ((ShiftKeyDown && CtrlKeyDown) || !CtrlKeyDown))
                {
                    DeletePoint(_index, 1);
                }
                else
                {
                    if (!ShiftKeyDown)
                    {
                        if (!ContainsData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index))
                        {
                            dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                            dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                            RouteOrder.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count - 1);
                        }
                    }
                }
            }
        }

        private void AddPoint(int nIndex)
        {
            double dOffsetX = 0;
            double dOffsetY = 0;

            CCircle circle = dataManager.recipeDM.TeachingRD.DataCandidatePoint[nIndex];
            circle.MeasurementOffsetX = dOffsetX;
            circle.MeasurementOffsetY = dOffsetY;

            int _index = -1;
            if (!ContainsSelectedData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index))
            {
                if (!ShiftKeyDown)
                {
                    if (!ContainsData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index))
                    {
                        dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                        dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                        RouteOrder.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count - 1);
                    }
                }
            }
            else
            {
                if (!CtrlKeyDown)
                {
                    DeletePoint(_index, 1);
                }
            }
        }

        public void DeletePoint(int nIndex, int nRange)
        {
            for (int i = nIndex; i < nIndex + nRange; i++)
            {
                double dPointX = dataManager.recipeDM.TeachingRD.DataSelectedPoint[nIndex].x;
                double dPointY = dataManager.recipeDM.TeachingRD.DataSelectedPoint[nIndex].y;

                dataManager.recipeDM.TeachingRD.DataSelectedPoint.RemoveAt(nIndex);
                int nDeleteIndex = dataManager.recipeDM.TeachingRD.DataMeasurementRoute.FindIndex(s => s.Equals(nIndex));
                dataManager.recipeDM.TeachingRD.DataMeasurementRoute.RemoveAt(nDeleteIndex);
                for (int j = 0; j < dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Count; j++)
                {
                    if (dataManager.recipeDM.TeachingRD.DataMeasurementRoute[j] > nIndex)
                    {
                        dataManager.recipeDM.TeachingRD.DataMeasurementRoute[j] = dataManager.recipeDM.TeachingRD.DataMeasurementRoute[j] - 1;
                    }
                }

            }

            //UpdateView();
            UpdateListView();
        }

        public bool ContainsSelectedData(List<CCircle> list, CCircle circle, out int nIndex)
        {
            bool bRst = false;
            nIndex = -1;

            int nCount = 0;
            foreach (var item in list)
            {
                if (Math.Round(item.x, 3) == Math.Round(circle.x, 3) && Math.Round(item.y, 3) == Math.Round(circle.y, 3)
                    && Math.Round(item.width, 3) == Math.Round(circle.width, 3) && Math.Round(item.height, 3) == Math.Round(circle.height, 3)
                    && Math.Round(item.MeasurementOffsetX, 3) == Math.Round(circle.MeasurementOffsetX, 3) && Math.Round(item.MeasurementOffsetY, 3) == Math.Round(circle.MeasurementOffsetY, 3))
                {
                    bRst = true;
                    nIndex = nCount;
                }
                nCount++;
            }
            return bRst;
        }

        private void PrintMousePosition(System.Windows.Point pt)
        {
            double mouseX = ((((int)pt.X - CenterX) - OffsetX) / (double)ZoomScale) / RatioX;
            double mouseY = ((((int)-pt.Y + CenterY) - OffsetY) / (double)ZoomScale) / RatioY;

            XPosition = Math.Round(mouseX, 3).ToString("0.###") + " mm";
            YPosition = Math.Round(mouseY, 3).ToString("0.###") + " mm";
            CurrentTheta = Math.Round((Math.Atan2(mouseY, mouseX) * 180 / Math.PI), 3).ToString("0.###") + " °";
            CurrentRadius = Math.Round((Math.Sqrt(Math.Pow(mouseX, 2) + Math.Pow(mouseY, 2))), 3).ToString("0.###") + " mm";
        }

        private void PrintMousePositionPreView(System.Windows.Point pt)
        {
            double mouseX = ((int)pt.X - CenterX) / RatioX;
            double mouseY = ((int)-pt.Y + CenterY) / RatioY;

            XPosition = Math.Round(mouseX, 3).ToString("0.###") + " mm";
            YPosition = Math.Round(mouseY, 3).ToString("0.###") + " mm";
            CurrentTheta = Math.Round((Math.Atan2(mouseY, mouseX) * 180 / Math.PI), 3).ToString("0.###") + " °";
            CurrentRadius = Math.Round((Math.Sqrt(Math.Pow(mouseX, 2) + Math.Pow(mouseY, 2))), 3).ToString("0.###") + " mm";
        }

        public void UpdateGridCombo()
        {
            string Material = "None";
            bool exist = false;

            foreach (string str in MaterialListItem)
            {
                exist = false;
                Material = Path.GetFileNameWithoutExtension(str);
                for (int i = 0; i < GridLayerData[0].Host1.Count; i++)
                {
                    if (GridLayerData[0].Host1[i].Name == Material)
                    {
                        exist = true;
                    }
                }

                if (!exist)
                {
                    for (int i = 0; i < GridLayerData.Count; i++)
                    {

                        GridLayerData[i].Host1.Add(new ModelData.LayerData.PathEntity(str, Material));
                        GridLayerData[i].Guest1.Add(new ModelData.LayerData.PathEntity(str, Material));
                        GridLayerData[i].Guest2.Add(new ModelData.LayerData.PathEntity(str, Material));
                    }
                }
            }
        }

        public void DeleteGridCombo()
        {
            int cnt = GridLayerData[0].Host1.Count;
            for (int i = 0; i < GridLayerData.Count; i++)
            {
                for (int j = cnt - 1; j > 0; j--)
                {
                    GridLayerData[i].Host1.RemoveAt(j);
                    GridLayerData[i].Guest1.RemoveAt(j);
                    GridLayerData[i].Guest2.RemoveAt(j);
                }
            }
        }

        public bool CheckMaterialUse()
        {
            if (MaterialSelectIndex == -1)
            {
                return true;
            }
            string material = MaterialListItem[MaterialSelectIndex];
            for (int i = 0; i < GridLayerData.Count; i++)
            {
                if (GridLayerData[i].SelectedHost1 == material || GridLayerData[i].SelectedGuest1 == material || GridLayerData[i].SelectedGuest2 == material)
                {
                    MessageBox.Show("The material is in use, can't delete", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return true;
                }
            }
            return false;
        }

        private void UpdateLayerGrid()
        {
            int cnt = LayerCount - 1;
            for (int i = 0; i < GridLayerData.Count; i++)
            {
                GridLayerData[i].UpdateGridLayer(cnt);
                cnt--;
            }
        }

        private void UpdateLayerModel()
        {
            int cnt = LayerCount - 1;
            for (int i = 0; i < GridLayerData.Count; i++)
            {
                GridLayerData[i].UpdateModelLayer(cnt);
                cnt--;
            }
        }

        private bool CheckLayerHost()
        {
            int cnt = LayerCount - 1;
            for(int i = 0; i < GridLayerData.Count; i++)
            {
                if (!GridLayerData[i].CheckLayerHost(cnt))
                {
                    return false;
                }
                cnt--;
            }
            return true;
        }

        private int _layerCount = 2;
        public int LayerCount
        {
            get
            {
                return _layerCount;
            }
            set
            {
                _layerCount = value;
            }
        }

        public void InitLayerGrid()
        {
            LayerCount = dataManager.recipeDM.ModelData.GetLayerCount();
            if (LayerCount == 0)
            {
                LayerCount = 2;
            }
            string[] strHeader = { "Sub.", "1st L.", "2nd L.", "3rd L.", "4th L.", "5th L.", "6th L.", "7th L.", "8th L.", "9th L.", "10th L." };

            int rows = LayerCount - 1;
            for (int i = 0; i < LayerCount; i++)
            {
                if (i == 0)
                {
                    GridLayerData.Add(new ModelData.LayerData("Amb."));
                }
                else
                {
                    GridLayerData.Add(new ModelData.LayerData(strHeader[rows]));
                }

                rows--;
            }
        }
        #endregion

        #region ICommand&MethodAction

        #region ICommand
        public ICommand CmdRouteOptimizaion
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI)
                    {
                        return;
                    }
                    RouteOptimizaionFunc();
                });
            }
        }
        public ICommand CmdReorderPoint
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI)
                    {
                        return;
                    }
                    if (dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count == 0)
                    {
                        return;
                    }

                    if (ShiftKeyDown)
                    {
                        ShiftKeyDown = false;
                        ShiftBrush = normalBrush;
                    }
                    if (CtrlKeyDown)
                    {
                        CtrlKeyDown = false;
                        CtrlBrush = normalBrush;
                    }

                    if (!SetStartEndPointMode)
                    {
                        ListReorderPoint.Clear();
                        ReorderCnt = 0;
                        SetStartEndPointMode = true;
                        ReorderBrush = buttonSelectBrush;
                        PointAddMode = "Reorder Mode";
                    }
                    else
                    {
                        SetStartEndPointMode = false;
                        ReorderBrush = normalBrush;
                        PointAddMode = "Normal";
                    }
                    UpdateView();
                });
            }
        }
        public ICommand CmdShowIndex
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsShowIndex)
                    {
                        ShowIndexBrush = normalBrush;
                        IsShowIndex = false;
                    }
                    else
                    {
                        ShowIndexBrush = buttonSelectBrush;
                        IsShowIndex = true;

                    }
                    InitKeyButton();
                    UpdateView();
                });
            }
        }
        public ICommand CmdUILock
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI)
                    {
                        LockBrush = normalBrush;
                        LockState = "Lock UI";
                        IsLockUI = false;
                    }
                    else
                    {
                        LockBrush = buttonSelectBrush;
                        LockState = "UnLock UI";
                        IsLockUI = true;

                        ZoomScale = 1;
                    }
                    InitKeyButton();
                    RedrawStage();
                });
            }
        }
        public ICommand CmdClose
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CloseRequested(this, new DialogCloseRequestedEventArgs(false));
                });
            }
        }
        public ICommand Cmd13Point
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    ReadPreset("13 Point.prs");

                    UpdateListView();
                    UpdateView();
                });
            }
        }
        public ICommand Cmd25Point
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    ReadPreset("25 Point.prs");

                    UpdateListView();
                    UpdateView();
                });
            }
        }
        public ICommand Cmd49Point
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    ReadPreset("49 Point.prs");
                    UpdateListView();
                    UpdateView();
                });
            }
        }
        public ICommand Cmd73Point
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    ReadPreset("73 Point.prs");

                    UpdateListView();
                    UpdateView();
                });
            }
        }
        public ICommand CmdSelectPercentage
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    double dPercent = -1;
                    if (double.TryParse(Percentage, out dPercent))
                    {
                        if (0 <= dPercent && dPercent <= 100)
                        {
                            dataManager.recipeDM.TeachingRD.DataSelectedPoint.Clear();
                            dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Clear();

                            int nCount = 0;
                            int nMin = 0;
                            int nMax = 0;

                            nMin = 0;
                            nMax = dataManager.recipeDM.TeachingRD.DataCandidatePoint.Count - 1;
                            nCount = (int)(dataManager.recipeDM.TeachingRD.DataCandidatePoint.Count * dPercent / 100);

                            foreach (CCircle c in GeneralFunction.GetSelectedRandom(dataManager.recipeDM.TeachingRD.DataCandidatePoint, nCount, nMin, nMax))
                            {
                                CCircle circle = c;

                                dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                                dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                            }
                            UpdateListView();
                            UpdateView();
                        }
                        else
                        {
                            Percentage = 0.ToString();
                        }

                    }
                    else
                    {
                        Percentage = 0.ToString();
                    }

                });
            }
        }
        public ICommand CmdSelectAll
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    dataManager.recipeDM.TeachingRD.DataSelectedPoint.Clear();
                    dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Clear();

                    foreach (CCircle c in dataManager.recipeDM.TeachingRD.DataCandidatePoint)
                    {
                        CCircle circle = c;

                        dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                        dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                    }

                    UpdateListView();
                    UpdateView();

                });
            }
        }
        public ICommand CmdDeleteAll
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    dataManager.recipeDM.TeachingRD.DataSelectedPoint.Clear();
                    dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Clear();
                    UpdateListView();
                    UpdateView();
                });
            }
        }
        public ICommand CmdReset
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    dataManager.recipeDM.TeachingRD.ClearPoint();

                    UpdateListView();
                    UpdateView();


                });
            }
        }
        public ICommand CmdPreset1
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    dataManager.recipeDM.TeachingRD.ClearCandidatePoint();
                    dataManager.recipeDM.TeachingRD.DataCandidatePoint = (List<CCircle>)GeneralFunction.Read(dataManager.recipeDM.TeachingRD.DataCandidatePoint, BaseDefine.Dir_StageMap + "Preset 1.smp");
                    CheckSelectedPoint();
                    UpdateListView();

                    UpdateView();

                });
            }
        }
        public ICommand CmdPreset2
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    dataManager.recipeDM.TeachingRD.ClearCandidatePoint();
                    dataManager.recipeDM.TeachingRD.DataCandidatePoint = (List<CCircle>)GeneralFunction.Read(dataManager.recipeDM.TeachingRD.DataCandidatePoint, BaseDefine.Dir_StageMap + "Preset 2.smp");
                    CheckSelectedPoint();

                    UpdateListView();
                    UpdateView();

                });
            }
        }

        public ICommand CmdRecipeNew
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (dataManager.recipeDM.RecipeNew())
                    {
                        if (!p_SettingViewModel.p_UseThickness)
                        {
                            p_UseThickness = false;
                        }
                        UpdateListView(false);
                        UpdateLayerGridView();
                        UpdateParameter();
                        UpdateView();

                        RecipePath = dataManager.recipeDM.TeachingRecipePath;
                    }
                });
            }
        }
        public ICommand CmdRecipeOpen
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (dataManager.recipeDM.RecipeOpen() == true)
                    {
                        UpdateListView(true);
                        UpdateLayerGridView();
                        UpdateParameter();
                        UpdateView(true);

                        RecipePath = dataManager.recipeDM.TeachingRecipePath;
                    }
                });
            }
        }
        public ICommand CmdRecipeSave
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (p_SettingViewModel.p_ExceptNIR)
                    {
                        dataManager.recipeDM.TeachingRD.NIRIntegrationTime = 0;
                    }
                    dataManager.recipeDM.RecipeSave();
                    MainViewModel.DataManager = dataManager;
                    MainViewModel.RecipeViewModel.dataManager = dataManager;
                    MainViewModel.RecipeViewModel.UpdateListView();
                    MainViewModel.RecipeViewModel.UpdateLayerGridView();
                    MainViewModel.RecipeViewModel.UpdateView();
                });
            }
        }
        public ICommand CmdRecipeSaveAs
        {
            get
            {
                return new RelayCommand(() =>
                {
                    dataManager.recipeDM.RecipeSaveAs();
                });
            }
        }

        private bool _isFocused = false;
        public bool MyIsFocused
        {
            get
            {
                return _isFocused;
            }
            set
            {
                _isFocused = false;
                RaisePropertyChanged("MyIsFocused");
                _isFocused = value;
                RaisePropertyChanged("MyIsFocused");
            }
        }

        public ICommand CmdAddWaveLength
        {
            get
            {
                return new RelayCommand(() =>
                {
                    double value = Convert.ToDouble(WaveLengthValue);
                    if (p_SettingViewModel.p_ExceptNIR)
                    {
                        if(value < 350 || value > 950)
                        {
                            MessageBox.Show("350nm ~ 950nm");
                            WaveLengthValue = "0";
                            MyIsFocused = true;
                            return;
                        }
                    }
                    else
                    {
                        if(value < 350 || value > 1500)
                        {
                            MessageBox.Show("350nm ~ 1500nm");
                            WaveLengthValue = "0";
                            MyIsFocused = true;
                            return;
                        }
                    }
                    if (IsReflectanceCheck)
                    {
                        dataManager.recipeDM.TeachingRD.WaveLengthReflectance.Add(value);
                        ReflectanceListItem.Add(value);
                        ReflectanceListItem = new ObservableCollection<double>(ReflectanceListItem.OrderBy(x=>x));
                    }
                    else if(IsTransmittanceCheck)
                    {
                        dataManager.recipeDM.TeachingRD.WaveLengthTransmittance.Add(value);
                        TransmittanceListItem.Add(value);
                        TransmittanceListItem = new ObservableCollection<double>(TransmittanceListItem.OrderBy(x => x));
                    }
                    WaveLengthValue = "0";
                    MyIsFocused = true;
                });
            }
        }

        public ICommand CmdDeleteWaveLength
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsReflectanceCheck)
                    {
                        if(ReflectanceSelectedIndex == -1)
                        {
                            return;
                        }
                        dataManager.recipeDM.TeachingRD.WaveLengthReflectance.RemoveAt(ReflectanceSelectedIndex);
                        ReflectanceListItem.RemoveAt(ReflectanceSelectedIndex);
                    }
                    else if (IsTransmittanceCheck)
                    {
                        if (TransmittanceSelectedIndex == -1)
                        {
                            return;
                        }
                        dataManager.recipeDM.TeachingRD.WaveLengthTransmittance.RemoveAt(TransmittanceSelectedIndex);
                        TransmittanceListItem.RemoveAt(TransmittanceSelectedIndex);
                    }
                });
            }
        }

        public ICommand CmdAddMaterial
        {
            get
            {
                return new RelayCommand(() =>
                {
                    bool res = dataManager.recipeDM.AddMaterial();
                    if(res)
                    {
                        MaterialListItem = new ObservableCollection<string>(dataManager.recipeDM.ModelData.MaterialList.ToArray());
                    }

                    UpdateGridCombo();
                });
            }
        }

        public ICommand CmdDeleteMaterial
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //MaterialListItem.Clear();
                    if(CheckMaterialUse() == false)
                    {
                        UpdateLayerModel();
                        dataManager.recipeDM.DeleteMaterial(MaterialSelectIndex);
                        MaterialListItem.RemoveAt(MaterialSelectIndex);
                        DeleteGridCombo();
                        UpdateGridCombo();

                        UpdateLayerGrid();
                    }
                });
            }
        }

        public ICommand CmdOpenModel
        {
            get
            {
                return new RelayCommand(() =>
                {
                  
                    if(dataManager.recipeDM.OpenModel() == true)
                    {
                        GridLayerData.Clear();
                        MaterialListItem = new ObservableCollection<string>(dataManager.recipeDM.ModelData.MaterialList.ToArray());
                        ModelPath = dataManager.recipeDM.TeachingRD.ModelRecipePath;

                        InitLayerGrid();
                        
                        DeleteGridCombo();
                        UpdateGridCombo();
                        
                        UpdateLayerGrid();
                    }
                });
            }
        }

        public ICommand CmdSaveModel
        {
            get
            {
                return new RelayCommand(() =>
                {
                    UpdateLayerModel();
                    if (!CheckLayerHost())
                    {
                        MessageBox.Show("The host must exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (dataManager.recipeDM.SaveModel())
                    {
                        ModelPath = dataManager.recipeDM.TeachingRD.ModelRecipePath;
                    }
                });
            }
        }

        public ICommand CmdInsertLayer
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (GridLayerIndex == -1)
                    {
                        MessageBox.Show("Select Layer row");
                        return;
                    }
                    if(GridLayerData.Count == 12)
                    {
                        MessageBox.Show("Cannot add any more.");
                        return;
                    }
                    int selIdx = GridLayerIndex;

                    UpdateLayerModel();
                    int index = GridLayerData.Count - GridLayerIndex;
                    dataManager.recipeDM.ModelData.AddLayer(index);

                    GridLayerData.Clear();
                    InitLayerGrid();

                    DeleteGridCombo();
                    UpdateGridCombo();

                    UpdateLayerGrid();

                    GridLayerIndex = selIdx;

                });
            }
        }

        public ICommand CmdDeleteLayer
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (GridLayerIndex == -1)
                    {
                        MessageBox.Show("Select Layer row");
                        return;
                    }
                    if (GridLayerData.Count == 2)
                    {
                        MessageBox.Show("Cannot delete any more.");
                        return;
                    }
                    int selIdx = GridLayerIndex;
                    UpdateLayerModel();
                    int index = GridLayerData.Count - GridLayerIndex - 1;
                    dataManager.recipeDM.ModelData.DeleteLayer(index);

                    GridLayerData.Clear();
                    InitLayerGrid();

                    DeleteGridCombo();
                    UpdateGridCombo();

                    UpdateLayerGrid();

                    GridLayerIndex = selIdx;
                });
            }
        }


        #endregion

        #region MethodAction
        public void OnListViewMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            MaterialSelectIndex = -1;
        }

        public void OnCheckboxChange(object sender, RoutedEventArgs e)
        {
            UpdateView();
        }
        public void OnThicknessChanged(object sender, TextChangedEventArgs e)
        {
            int thick = 0;
            RouteThick = 3;
            if (int.TryParse(RouteThickness, out thick))
            {
                RouteThick = thick;
                if (0 < thick && thick <= 10)
                {
                    RouteThick = thick;
                }
                else if (thick > 10)
                {
                    RouteThick = 10;
                    RouteThickness = "10";
                }
                else if (thick < 0)
                {
                    RouteThick = 1;
                    RouteThickness = "1";
                }
            }
            else
            {
                RouteThickness = "3";
            }
            UpdateView();
        }
        public void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            ColorPicker co = sender as ColorPicker;
            RouteBrush.Color = (System.Windows.Media.Color)co.SelectedColor;
            MainViewModel.RouteBrush.Color = (System.Windows.Media.Color)co.SelectedColor;
            RedrawStage();
        }
        public void colorPicker_Opened(object sender, RoutedEventArgs e)
        {
            ColorPickerOpened = true;
            if (IsKeyboardShowIndex)
            {
                IsKeyboardShowIndex = false;
                SBrush = normalBrush;
                UpdateView();
            }
            if (ShiftKeyDown)
            {
                ShiftKeyDown = false;
                ShiftBrush = normalBrush;
            }
            if (CtrlKeyDown)
            {
                CtrlKeyDown = false;
                CtrlBrush = normalBrush;
            }
            PointAddMode = "Normal";
        }
        public void OnCanvasKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.S))
            {
                if (IsShowIndex)
                {
                    SBrush = buttonSelectBrush;
                    IsKeyboardShowIndex = false;
                    PointAddMode = "Already Displaying";
                }
                else if (!IsKeyboardShowIndex)
                {
                    SBrush = buttonSelectBrush;
                    IsKeyboardShowIndex = true;
                    PointAddMode = "Show Index";
                    RedrawStage();
                }
            }
            if (SetStartEndPointMode)
            {
                return;
            }
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                ShiftKeyDown = true;
                ShiftBrush = buttonSelectBrush;
                PointAddMode = "Delete Select Mode";
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                CtrlKeyDown = true;
                CtrlBrush = buttonSelectBrush;
                PointAddMode = "Add Select Mode";
            }
            if (ShiftKeyDown && CtrlKeyDown)
            {
                PointAddMode = "Delete Select Mode";
            }
        }

        public void OnCanvasKeyUp(object sender, KeyEventArgs e)
        {

            if (Keyboard.IsKeyUp(Key.S))
            {
                SBrush = normalBrush;
                if (IsShowIndex)
                {
                    if (ShiftKeyDown)
                    {
                        PointAddMode = "Delete Select Mode";
                    }
                    else if (CtrlKeyDown)
                    {
                        PointAddMode = "Add Select Mode";
                    }
                    else
                    {
                        PointAddMode = "Normal";
                    }
                }
                else if (IsKeyboardShowIndex)
                {
                    IsKeyboardShowIndex = false;
                    RedrawStage();
                    if (ShiftKeyDown)
                    {
                        PointAddMode = "Delete Select Mode";
                    }
                    else if (CtrlKeyDown)
                    {
                        PointAddMode = "Add Select Mode";
                    }
                    else
                    {
                        PointAddMode = "Normal";
                    }
                }
            }
            if (SetStartEndPointMode)
            {
                if (ShiftKeyDown)
                {
                    ShiftKeyDown = false;
                    ShiftBrush = normalBrush;
                }
                if (CtrlKeyDown)
                {
                    CtrlKeyDown = false;
                    CtrlBrush = normalBrush;
                }
                return;
            }
            if (Keyboard.IsKeyUp(Key.LeftShift))
            {
                ShiftKeyDown = false;
                ShiftBrush = normalBrush;
                if (CtrlKeyDown)
                {
                    PointAddMode = "Add Select Mode";
                }
                else
                {
                    PointAddMode = "Normal";
                }
            }
            if (Keyboard.IsKeyUp(Key.LeftCtrl))
            {
                CtrlKeyDown = false;
                CtrlBrush = normalBrush;
                if (ShiftKeyDown)
                {
                    PointAddMode = "Delete Select Mode";
                }
                else
                {
                    PointAddMode = "Normal";
                }
            }
        }
        #endregion

        #endregion

        struct Data
        {
            public int Current
            {
                get; private set;
            }
            public int Next
            {
                get; private set;
            }
            public double Dist
            {
                get; private set;
            }

            public Data(int curr, int next, double dist)
            {
                Current = curr;
                Next = next;
                Dist = dist;
            }
        }
    }
}
