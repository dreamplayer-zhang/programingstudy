using Emgu.CV;
using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Root_WindII
{
    public class RACSetup_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        public enum SETUP_VIEWER_STATE
        {
            Normal,
            Center,
            Origin,
            Pitch,
            ShotKey,
            Rular,
        }

        public delegate void EventViewerStateChagned();
        public delegate void EventOriginBoxDone();
        public delegate void EventOriginPointDone();
        public delegate void EventOriginBoxReset();
        public delegate void EventPitchPointDone();
        public delegate void EventCenteringPointDone();

        #region [Color]

        class DefineColors
        {
            public static SolidColorBrush OriginColor = Brushes.Blue;
            public static SolidColorBrush OriginBoxColor = Brushes.Blue;

            public static SolidColorBrush CenteringColor = Brushes.Green;
            public static SolidColorBrush CenteringBoxColor = Brushes.Green;

            public static SolidColorBrush CenteringPointColor = Brushes.Red;

            public static SolidColorBrush ShotCenterPointColor = Brushes.Purple;
            public static SolidColorBrush ShotKeyPointColor = Brushes.Chocolate;

            public static SolidColorBrush PitchColor = Brushes.Yellow;
            public static SolidColorBrush PitchBoxColor = Brushes.Yellow;
        }

        #endregion

        #region [Event]
        public event EventViewerStateChagned ViewerStateChanged;
        public event EventOriginPointDone OriginPointDone;
        public event EventOriginBoxDone OriginBoxDone;
        public event EventOriginBoxReset OriginBoxReset;
        public event EventPitchPointDone PitchPointDone;
        public event EventCenteringPointDone CenteringPointDone;
        #endregion

        #region [ViewerState]

        public RACSetup_ImageViewer_ViewModel()
        {
            p_VisibleMenu = System.Windows.Visibility.Collapsed;

            this.ViewerState = SETUP_VIEWER_STATE.Normal;
            this.ViewerStateChanged += ViewerStateChanged_Callback;

            //CompositionTarget.Rendering += RedrawShapes;
            InitializeUIElement();
        }

        private SETUP_VIEWER_STATE viewerState = SETUP_VIEWER_STATE.Normal;
        public SETUP_VIEWER_STATE ViewerState
        {
            get => this.viewerState;
            set
            {
                switch (value)
                {
                    case SETUP_VIEWER_STATE.Normal:
                        break;
                    case SETUP_VIEWER_STATE.Center:
                        centerState = PROCESS_CENTER_STATE.CenterLeftTop;
                        break;
                    case SETUP_VIEWER_STATE.Origin:
                        originState = PROCESS_ORIGIN_STATE.OriginLeftBottom;
                        break;
                    case SETUP_VIEWER_STATE.Pitch:
                        pitchState = PROCESS_PITCH_STATE.PitchRightTop;
                        break;
                    case SETUP_VIEWER_STATE.Rular:
                        break;
                }

                SetProperty<SETUP_VIEWER_STATE>(ref this.viewerState, value);

                if (ViewerStateChanged != null)
                    this.ViewerStateChanged();
            }
        }

        public void ViewerStateChanged_Callback()
        {
            this.DisplayViewerState = this.ViewerState.ToString();
            if (this.ViewerState == SETUP_VIEWER_STATE.Normal)
            {
                this.IsCenterChecked = false;
                this.IsOriginChecked = false;
                this.IsPitchChecked = false;
                this.IsShotKeyChecked = false;
                this.IsRularChecked = false;
            }
        }

        #endregion

        #region [Properties]
        private bool isCenterChecked = false;
        public bool IsCenterChecked
        {
            get => this.isCenterChecked;
            set
            {
                if (value)
                {
                    IsOriginChecked = false;
                    IsPitchChecked = false;
                    IsShotKeyChecked = false;
                    isRularChecked = false;
                }
                else
                {
                    if (p_UIElement.Contains(CenterLeftTop_UI))
                    {
                        p_UIElement.Remove(CenterLeftTop_UI);
                    }
                    if (p_UIElement.Contains(CenterRightBottom_UI))
                    {
                        p_UIElement.Remove(CenterRightBottom_UI);
                    }
                    //else
                    //{
                    //this.originLeftBottom.X = this.originBox.Left;
                    //this.originLeftBottom.Y = this.originBox.Bottom;
                    RedrawShapes(this, new EventArgs());
                    // }
                }
                SetProperty(ref isCenterChecked, value);
            }
        }

        private bool isShotKeyChecked = false;
        public bool IsShotKeyChecked
        {
            get => isShotKeyChecked;
            set
            {
                if (value)
                {
                    this.IsOriginChecked = false;
                    IsCenterChecked = false;
                    IsPitchChecked = false;
                    //isShotKeyChecked = false;
                    this.IsRularChecked = false;
                    this.IsRularChecked = false;
                }

                SetProperty(ref isShotKeyChecked, value);
            }
        }

        private bool isOriginChecked = false;
        public bool IsOriginChecked
        {
            get => this.isOriginChecked;
            set
            {
                if (value == true)
                {
                    this.IsOriginChecked = false;
                    IsCenterChecked = false;
                    IsPitchChecked = false;
                    IsShotKeyChecked = false;
                    this.IsRularChecked = false;
                    this.IsRularChecked = false;
                }
                else
                {
                    if (!p_UIElement.Contains(OriginBox_UI))
                    {
                        ClearObjects();
                    }
                    else
                    {
                        this.originLeftBottom.X = this.originBox.Left;
                        this.originLeftBottom.Y = this.originBox.Bottom;
                        RedrawShapes(this, new EventArgs());
                    }
                }

                SetProperty<bool>(ref this.isOriginChecked, value);
            }
        }

        private bool isOriginEnable = true;
        public bool IsOriginEnable
        {
            get => this.isOriginEnable;
            set
            {
                SetProperty<bool>(ref this.isOriginEnable, value);
            }
        }

        private bool isShotKeyEnable = false;
        public bool IsShotKeyEnable
        {
            get => this.isShotKeyEnable;
            set
            {
                SetProperty<bool>(ref this.isShotKeyEnable, value);
            }
        }

        private bool isPitchChecked = false;
        public bool IsPitchChecked
        {
            get => this.isPitchChecked;
            set
            {
                if (value == true)
                {
                    this.IsOriginChecked = false;
                    IsCenterChecked = false;
                    IsShotKeyChecked = false;
                    this.IsRularChecked = false;
                }
                else
                {
                    if (!p_UIElement.Contains(PitchBox_UI))
                    {
                        this.pitchRightBottom.X = this.originRightTop.X;
                        this.pitchRightBottom.Y = this.originLeftBottom.Y;

                        this.pitchRightTop.X = this.originRightTop.X;
                        this.pitchRightTop.Y = this.originRightTop.Y;
                    }
                    else
                    {
                        this.pitchRightTop.X = this.pitchBox.Right;
                        this.pitchRightBottom.X = this.pitchBox.Right;
                        this.pitchRightTop.Y = this.pitchBox.Top;
                        this.pitchRightBottom.Y = this.pitchBox.Bottom;
                    }
                }
                SetProperty<bool>(ref this.isPitchChecked, value);
            }
        }


        private bool isPitchEnable = false;
        public bool IsPitchEnable
        {
            get => this.isPitchEnable;
            set
            {
                SetProperty<bool>(ref this.isPitchEnable, value);
            }
        }

        private bool isRularChecked = false;
        public bool IsRularChecked
        {
            get => this.isRularChecked;
            set
            {
                if (value == true)
                {
                    this.IsPitchChecked = false;
                    this.IsOriginChecked = false;
                }
                SetProperty<bool>(ref this.isRularChecked, value);
            }
        }

        private string displayViewerState = FRONT_ORIGIN_VIEWER_STATE.Normal.ToString();
        public string DisplayViewerState
        {
            get => this.displayViewerState;
            set
            {
                SetProperty<string>(ref this.displayViewerState, value);
            }
        }
        #endregion

        #region Data
        string deviceID = "";
        public string DeviceID
        {
            get => deviceID;
            set
            {
                SetProperty(ref deviceID, value);
            }
        }
        int unitX = 0;
        public int UnitX
        {
            get => unitX;
            set
            {
                SetProperty(ref unitX, value);
            }
        }
        int unitY = 0;
        public int UnitY
        {
            get => unitY;
            set
            {
                SetProperty(ref unitY, value);
            }
        }
        double diePitchX = 0;
        public double DiePitchX
        {
            get => diePitchX;
            set
            {
                SetProperty(ref diePitchX, value);
            }
        }
        double diePitchY = 0;
        public double DiePitchY
        {
            get => diePitchY;
            set
            {
                SetProperty(ref diePitchY, value);
            }
        }
        double scribeLaneX = 0;
        public double ScribeLaneX
        {
            get => scribeLaneX;
            set
            {
                SetProperty(ref scribeLaneX, value);
            }
        }
        double scribeLaneY = 0;
        public double ScribeLaneY
        {
            get => scribeLaneY;
            set
            {
                SetProperty(ref scribeLaneY, value);
            }
        }

        double shotOffsetX = 0;
        public double ShotOffsetX
        {
            get => shotOffsetX;
            set
            {
                SetProperty(ref shotOffsetX, value);
            }
        }
        double shotOffsetY = 0;
        public double ShotOffsetY
        {
            get => shotOffsetY;
            set
            {
                SetProperty(ref shotOffsetY, value);
            }
        }
        double mapOffsetX = 0;
        public double MapOffsetX
        {
            get => mapOffsetX;
            set
            {
                SetProperty(ref mapOffsetX, value);
            }
        }
        double mapOffsetY = 0;
        public double MapOffsetY
        {
            get => mapOffsetY;
            set
            {
                SetProperty(ref mapOffsetY, value);
            }
        }
        double smiOffsetX = 0;
        public double SmiOffsetX
        {
            get => smiOffsetX;
            set
            {
                SetProperty(ref smiOffsetX, value);
            }
        }
        double smiOffsetY = 0;
        public double SmiOffsetY
        {
            get => smiOffsetY;
            set
            {
                SetProperty(ref smiOffsetY, value);
            }
        }
        double originDieX = 0;
        public double OriginDieX
        {
            get => originDieX;
            set
            {
                SetProperty(ref originDieX, value);
            }
        }
        double originDieY = 0;
        public double OriginDieY
        {
            get => originDieY;
            set
            {
                SetProperty(ref originDieY, value);
            }
        }
        int shotSizeX = 0;
        public int ShotSizeX
        {
            get => shotSizeX;
            set
            {
                if (shotSizeX == value)
                    return;
                SetProperty(ref shotSizeX, value);
                // RedrawShot();
            }
        }
        int shotSizeY = 0;
        public int ShotSizeY
        {
            get => shotSizeY;
            set
            {
                if (shotSizeY == value)
                    return;
                SetProperty(ref shotSizeY, value);
                //RedrawShot();
            }
        }

        void RedrawShot()
        {
            if (p_UIElement.Contains(ShotKeyPoint_UI))
                p_UIElement.Remove(ShotKeyPoint_UI);
            DrawShotKeyPoint();

            for (int i = 0; i < totalShot; i++)
            {
                if (ShotList[i] != null && p_UIElement.Contains(ShotList_UI[i]))
                {
                    p_UIElement.Remove(ShotList_UI[i]);
                }
            }
            ShotList_UI.Clear();
            ShotList.Clear();

            totalShot = ShotSizeX * ShotSizeY;
            for (byte i = 0; i < totalShot; i++)
            {

                Grid grid = new Grid();
                grid.Children.Add(new Line());
                grid.Children.Add(new Line());
                grid.Children.Add(new Line());
                grid.Children.Add(new Line());
                grid.Width = DiePitchX / 4;
                grid.Height = DiePitchY / 4;
                ShotList_UI.Add(grid);
                ShotList.Add(new CRect());
            }
            DrawShotList();
        }
        #endregion

        #region [Command]
        public RelayCommand btnOriginCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (this.IsOriginChecked == true)
                    {
                        this.ViewerState = SETUP_VIEWER_STATE.Origin;
                        this.DisplayViewerState = this.ViewerState.ToString();
                    }
                    else
                    {
                        this.ViewerState = SETUP_VIEWER_STATE.Normal;
                        this.DisplayViewerState = this.ViewerState.ToString();

                    }
                });
            }
        }

        public RelayCommand btnShotKeyCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (this.IsShotKeyChecked)
                    {
                        ViewerState = SETUP_VIEWER_STATE.ShotKey;
                        this.DisplayViewerState = this.ViewerState.ToString();
                    }
                    else
                    {
                        ViewerState = SETUP_VIEWER_STATE.Normal;
                        p_Cursor = Cursors.Arrow;
                        this.DisplayViewerState = this.ViewerState.ToString();
                    }
                });
            }
        }

        public RelayCommand btnObjectClearCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ClearAllObjects();
                });
            }
        }

        public RelayCommand btnCenterCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsCenterChecked)
                    {
                        ViewerState = SETUP_VIEWER_STATE.Center;
                        DisplayViewerState = ViewerState.ToString();
                    }
                    else
                    {
                        this.ViewerState = SETUP_VIEWER_STATE.Normal;
                        this.DisplayViewerState = this.ViewerState.ToString();
                    }
                });
            }
        }

        public RelayCommand btnPitchCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (this.IsPitchChecked == true)
                    {
                        this.ViewerState = SETUP_VIEWER_STATE.Pitch;
                        this.DisplayViewerState = this.ViewerState.ToString();
                    }
                    else
                    {
                        this.ViewerState = SETUP_VIEWER_STATE.Normal;
                        this.DisplayViewerState = this.ViewerState.ToString();
                    }
                });
            }
        }

        public RelayCommand btnRularCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (this.IsRularChecked == true)
                    {
                        this.ViewerState = SETUP_VIEWER_STATE.Rular;
                        this.DisplayViewerState = this.ViewerState.ToString();
                    }
                    else
                    {
                        this.ViewerState = SETUP_VIEWER_STATE.Normal;
                        this.DisplayViewerState = this.ViewerState.ToString();
                    }
                });
            }
        }

        public RelayCommand btnOpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._openImage();
                });
            }
        }

        public RelayCommand btnSaveCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._saveImage();
                });
            }
        }

        public RelayCommand btnClearCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._clearImage();
                });
            }
        }

        public RelayCommand btnViewFullCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.DisplayFull();
                });
            }
        }

        public RelayCommand btnViewBoxCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.DisplayBox();
                });
            }
        }
        #endregion

        #region [Draw 관련 멤버]

        Grid OriginLeftBottom_UI = null;
        Grid OriginRightTop_UI = null;
        Grid OriginBox_UI = null;

        Grid PitchRightTop_UI = null;
        Grid PitchRightBottom_UI = null;
        Grid PitchBox_UI = null;


        Grid CenterPoint_UI = null;
        CPoint centerPoint = new CPoint();

        Grid ShotCenterPoint_UI = null;
        CPoint ShotCenterPoint = new CPoint();

        Grid ShotKeyPoint_UI = null;
        CPoint ShotKeyPoint = new CPoint();

        List<Grid> ShotList_UI = new List<Grid>();
        List<CRect> ShotList = new List<CRect>();

        Grid CenterLeftTop_UI = null;
        Grid CenterRightBottom_UI = null;
        Grid CenterLTBox_UI = null;
        Grid CenterLTPoint_UI = null;
        CPoint CenterLTPoint = new CPoint();
        //Grid CenterLeftTopRT_UI = null;
        //Grid CenterRightBottomRT_UI = null;
        Grid CenterRTBox_UI = null;
        Grid CenterRTPoint_UI = null;
        CPoint CenterRTPoint = new CPoint();
        //Grid CenterLeftTopRB_UI = null;
        //Grid CenterRightBottomRB_UI = null;
        Grid CenterRBBox_UI = null;
        Grid CenterRBPoint_UI = null;
        CPoint CenterRBPoint = new CPoint();

        CPoint centerLeftTop = new CPoint();
        CPoint centerRightBottom = new CPoint();
        CRect centerBoxLT = new CRect();
        CRect centerBoxRT = new CRect();
        CRect centerBoxRB = new CRect();

        CPoint originLeftBottom = new CPoint();
        CPoint originRightTop = new CPoint();
        CRect originBox = new CRect();

        CPoint pitchRightTop = new CPoint();
        CPoint pitchRightBottom = new CPoint();
        CRect pitchBox = new CRect();

        public bool IsLoad { get; set; } = false;

        int totalShot = 0;

        public void InitializeUIElement()
        {
            OriginLeftBottom_UI = new Grid();
            OriginLeftBottom_UI.Children.Add(new Line());
            OriginLeftBottom_UI.Children.Add(new Line());

            OriginRightTop_UI = new Grid();
            OriginRightTop_UI.Children.Add(new Line());
            OriginRightTop_UI.Children.Add(new Line());

            PitchRightTop_UI = new Grid();

            Line line1 = new Line();
            line1.X1 = 0;
            line1.Y1 = -10;
            line1.X2 = 0;
            line1.Y2 = 40;
            line1.Stroke = DefineColors.PitchColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = new Line();
            line2.X1 = -40;
            line2.Y1 = 0;
            line2.X2 = 10;
            line2.Y2 = 0;
            line2.Stroke = DefineColors.PitchColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            PitchRightTop_UI.Children.Add(line1);
            PitchRightTop_UI.Children.Add(line2);

            PitchRightBottom_UI = new Grid();

            Line line3 = new Line();
            line3.X1 = 0;
            line3.Y1 = -40;
            line3.X2 = 0;
            line3.Y2 = 10;
            line3.Stroke = DefineColors.PitchColor;
            line3.StrokeThickness = 3;
            line3.Opacity = 1;

            Line line4 = new Line();
            line4.X1 = 10;
            line4.Y1 = 0;
            line4.X2 = -40;
            line4.Y2 = 0;
            line4.Stroke = DefineColors.PitchColor;
            line4.StrokeThickness = 3;
            line4.Opacity = 1;

            PitchRightBottom_UI.Children.Add(line3);
            PitchRightBottom_UI.Children.Add(line4);


            ShotCenterPoint_UI = new Grid();
            ShotCenterPoint_UI.Children.Add(new Line());
            ShotCenterPoint_UI.Children.Add(new Line());

            ShotKeyPoint_UI = new Grid();
            ShotKeyPoint_UI.Children.Add(new Line());
            ShotKeyPoint_UI.Children.Add(new Line());

            CenterLeftTop_UI = new Grid();
            CenterLeftTop_UI.Tag = "Centering";
            CenterLeftTop_UI.Children.Add(new Line());
            CenterLeftTop_UI.Children.Add(new Line());

            CenterPoint_UI = new Grid();
            CenterPoint_UI.Tag = "Centering";
            CenterPoint_UI.Children.Add(new Line());
            CenterPoint_UI.Children.Add(new Line());


            CenterRightBottom_UI = new Grid();
            CenterRightBottom_UI.Tag = "Centering";
            CenterRightBottom_UI.Children.Add(new Line());
            CenterRightBottom_UI.Children.Add(new Line());

            CenterLTBox_UI = new Grid();
            CenterLTBox_UI.Tag = "CenteringLT";
            CenterLTBox_UI.Children.Add(new Line());
            CenterLTBox_UI.Children.Add(new Line());
            CenterLTBox_UI.Children.Add(new Line());
            CenterLTBox_UI.Children.Add(new Line());

            CenterLTPoint_UI = new Grid();
            CenterLTPoint_UI.Tag = "Centering";
            CenterLTPoint_UI.Children.Add(new Line());
            CenterLTPoint_UI.Children.Add(new Line());

            CenterRTBox_UI = new Grid();
            CenterRTBox_UI.Tag = "CenteringRT";
            CenterRTBox_UI.Children.Add(new Line());
            CenterRTBox_UI.Children.Add(new Line());
            CenterRTBox_UI.Children.Add(new Line());
            CenterRTBox_UI.Children.Add(new Line());

            CenterRTPoint_UI = new Grid(); 
            CenterRTPoint_UI.Tag = "Centering";
            CenterRTPoint_UI.Children.Add(new Line());
            CenterRTPoint_UI.Children.Add(new Line());

            CenterRBBox_UI = new Grid();
            CenterRBBox_UI.Tag = "CenteringRB";
            CenterRBBox_UI.Children.Add(new Line());
            CenterRBBox_UI.Children.Add(new Line());
            CenterRBBox_UI.Children.Add(new Line());
            CenterRBBox_UI.Children.Add(new Line());


            CenterRBPoint_UI = new Grid();
            CenterRBPoint_UI.Tag = "Centering";
            CenterRBPoint_UI.Children.Add(new Line());
            CenterRBPoint_UI.Children.Add(new Line());

            OriginBox_UI = new Grid();
            OriginBox_UI.Children.Add(new Line()); // Left
            OriginBox_UI.Children.Add(new Line()); // Top
            OriginBox_UI.Children.Add(new Line()); // Right
            OriginBox_UI.Children.Add(new Line()); // Bottom

            PitchBox_UI = new Grid();
            PitchBox_UI.Children.Add(new Line()); // Left
            PitchBox_UI.Children.Add(new Line()); // Top
            PitchBox_UI.Children.Add(new Line()); // Right
            PitchBox_UI.Children.Add(new Line()); // Bottom
        }
        #endregion

        #region [Mouse Event Overrides]
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);

            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;

            switch (this.ViewerState)
            {
                case SETUP_VIEWER_STATE.Normal:
                    ProcessNormal(e);
                    break;
                case SETUP_VIEWER_STATE.Center:
                    ProcessCenter(e);
                    break;
                case SETUP_VIEWER_STATE.Origin:
                    ProcessOrigin(e);
                    break;
                case SETUP_VIEWER_STATE.Pitch:
                    ProcessPitch(e);
                    break;
                case SETUP_VIEWER_STATE.ShotKey:
                    ProcessShotKey(e);
                    break;
                case SETUP_VIEWER_STATE.Rular:

                    break;
            }
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);

            switch (this.ViewerState)
            {
                case SETUP_VIEWER_STATE.Normal:
                    break;
                case SETUP_VIEWER_STATE.Center:
                    if (centerState == PROCESS_CENTER_STATE.CenterLeftTop)
                    {
                        centerLeftTop.X = p_MouseMemX;
                        centerLeftTop.Y = p_MouseMemY;
                        DrawCenterLeftTopPoint(centerLeftTop);
                    }
                    else if (centerState == PROCESS_CENTER_STATE.CenterRightBottom)
                    {
                        centerRightBottom.X = p_MouseMemX;
                        centerRightBottom.Y = p_MouseMemY;
                        DrawCenterRightBottomPoint(centerRightBottom);
                    }
                    break;
                case SETUP_VIEWER_STATE.Origin:
                    if (this.originState == PROCESS_ORIGIN_STATE.OriginLeftBottom)
                    {
                        originLeftBottom.X = p_MouseMemX;
                        originLeftBottom.Y = p_MouseMemY;
                        DrawOriginLeftBottomPoint(originLeftBottom);
                    }
                    else if (this.originState == PROCESS_ORIGIN_STATE.OriginRightTop)
                    {
                        originRightTop.X = p_MouseMemX;
                        originRightTop.Y = p_MouseMemY;
                        DrawOriginRightTopPoint(originRightTop);
                    }
                    break;
                case SETUP_VIEWER_STATE.ShotKey:
                    p_Cursor = Cursors.Pen;
                    DrawShotKeyPoint();
                    break;
                case SETUP_VIEWER_STATE.Pitch:
                    if (this.pitchState == PROCESS_PITCH_STATE.PitchRightTop)
                    {
                        DrawingPitchPoint();
                    }
                    break;
                case SETUP_VIEWER_STATE.Rular:
                    break;
            }
        }
        public override void PreviewMouseUp(object sender, MouseEventArgs e)
        {

        }
        public override void SetRoiRect()
        {
            base.SetRoiRect();
            RedrawShapes(this, new EventArgs());
        }
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            base.CanvasMovePoint_Ref(point, nX, nY);
            RedrawShapes(this, new EventArgs());
        }

        public override void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.MouseWheel(sender, e);

            RedrawShapes(sender, e);
        }

        #endregion


        #region [Process Normal]
        public void ProcessNormal(MouseEventArgs e)
        {

        }
        #endregion

        #region [Process Align]
        private enum PROCESS_CENTER_STATE
        {
            None,
            CenterLeftTop,
            CenterRightBottom
        }
        PROCESS_CENTER_STATE centerState = PROCESS_CENTER_STATE.None;

        private enum CENTERING_POS
        {
            None,
            LT,
            RT,
            RB
        }
        CENTERING_POS centerPos = CENTERING_POS.LT;
        public void ProcessCenter(MouseEventArgs e)
        {
            CPoint canvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint memPt = GetMemPoint(canvasPt);

            switch (centerState)
            {
                case PROCESS_CENTER_STATE.None:
                    break;
                case PROCESS_CENTER_STATE.CenterLeftTop:
                    ClearObjects();
                    p_Cursor = Cursors.Arrow;
                    centerLeftTop = memPt;
                    //if (centerPos == CENTERING_POS.None)
                    //    centerPos = CENTERING_POS.LT;
                    DrawCenterLeftTopPoint(centerLeftTop);

                    centerState = PROCESS_CENTER_STATE.CenterRightBottom;
                    break;
                case PROCESS_CENTER_STATE.CenterRightBottom:
                    

                    p_Cursor = Cursors.Arrow;
                    
                    centerRightBottom.X = memPt.X;
                    centerRightBottom.Y = memPt.Y;

                    centerState = PROCESS_CENTER_STATE.CenterLeftTop;

                    switch (centerPos)
                    {
                        case CENTERING_POS.LT:
                            centerBoxLT.Left = centerLeftTop.X;
                            centerBoxLT.Right = centerRightBottom.X;
                            centerBoxLT.Top = centerLeftTop.Y;
                            centerBoxLT.Bottom = centerRightBottom.Y;
                            break;
                        case CENTERING_POS.RT:
                            centerBoxRT.Left = centerLeftTop.X;
                            centerBoxRT.Right = centerRightBottom.X;
                            centerBoxRT.Top = centerLeftTop.Y;
                            centerBoxRT.Bottom = centerRightBottom.Y;
                            break;
                        case CENTERING_POS.RB:
                            centerBoxRB.Left = centerLeftTop.X;
                            centerBoxRB.Right = centerRightBottom.X;
                            centerBoxRB.Top = centerLeftTop.Y;
                            centerBoxRB.Bottom = centerRightBottom.Y;
                            break;
                        case CENTERING_POS.None:
                            if (nearGrid.Tag.ToString().Contains("LT"))
                            {
                                centerBoxLT.Left = centerLeftTop.X;
                                centerBoxLT.Right = centerRightBottom.X;
                                centerBoxLT.Top = centerLeftTop.Y;
                                centerBoxLT.Bottom = centerRightBottom.Y;
                            }
                            else if (nearGrid.Tag.ToString().Contains("RT"))
                            {
                                centerBoxRT.Left = centerLeftTop.X;
                                centerBoxRT.Right = centerRightBottom.X;
                                centerBoxRT.Top = centerLeftTop.Y;
                                centerBoxRT.Bottom = centerRightBottom.Y;
                            }
                            else
                            {
                                centerBoxRB.Left = centerLeftTop.X;
                                centerBoxRB.Right = centerRightBottom.X;
                                centerBoxRB.Top = centerLeftTop.Y;
                                centerBoxRB.Bottom = centerRightBottom.Y;
                            }
                            break;
                    }

                    DrawCenterRightBottomPoint(centerRightBottom);
                    //DrawingPitchPoint();
                    DrawCenterBox(pos : centerPos);
                    CalcCenterPoint();
                    if (p_UIElement.Contains(CenterRightBottom_UI))
                    {
                        p_UIElement.Remove(CenterRightBottom_UI);
                    }

                    if (centerPos == CENTERING_POS.RB)
                        centerPos = CENTERING_POS.None;

                    if (centerPos != CENTERING_POS.None)
                        centerPos++;

                    if(centerPos == CENTERING_POS.None)
                    {
                        ViewerState = SETUP_VIEWER_STATE.Normal;
                        SetCenter();
                        CalcCenter();
                    }


                    //SetOrigin();

                    //originState = PROCESS_ORIGIN_STATE.None;
                    
                    break;
            }
        }

        #endregion

        #region [Process Origin]
        private enum PROCESS_ORIGIN_STATE
        {
            None,
            OriginLeftBottom,
            OriginRightTop,
        }

        PROCESS_ORIGIN_STATE originState = PROCESS_ORIGIN_STATE.None;

        public void ProcessOrigin(MouseEventArgs e)
        {
            CPoint canvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint memPt = GetMemPoint(canvasPt);

            switch (originState)
            {
                case PROCESS_ORIGIN_STATE.None:
                    break;
                case PROCESS_ORIGIN_STATE.OriginLeftBottom:

                    // Origin 
                    ClearObjects();

                    p_Cursor = Cursors.Arrow;
                    originLeftBottom = memPt;
                    DrawOriginLeftBottomPoint(originLeftBottom);

                    if (this.OriginPointDone != null)
                        this.OriginPointDone();

                    originState = PROCESS_ORIGIN_STATE.OriginRightTop;
                    break;
                case PROCESS_ORIGIN_STATE.OriginRightTop:

                    p_Cursor = Cursors.Arrow;

                    if ((memPt.X - originLeftBottom.X) > 30000 || (originLeftBottom.Y - memPt.Y) > 30000)
                    {
                        MessageBox.Show("Origin(혹은 검사) 영역의 크기는 높이 30000(혹은 너비 30000)을 넘을 수 없습니다.");
                        return;
                    }

                    originRightTop.X = memPt.X;
                    originRightTop.Y = memPt.Y;

                    originBox.Left = originLeftBottom.X;
                    originBox.Right = originRightTop.X;
                    originBox.Top = originRightTop.Y;
                    originBox.Bottom = originLeftBottom.Y;

                    DrawOriginRightTopPoint(originRightTop);
                    DrawingPitchPoint();
                    DrawOriginBox();

                    SetOrigin();

                    originState = PROCESS_ORIGIN_STATE.None;
                    ViewerState = SETUP_VIEWER_STATE.Normal;
                    break;
            }
        }




        #endregion

        #region [Process ShotKey]
        public void ProcessShotKey(MouseEventArgs e)
        {
            CPoint canvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint memPt = GetMemPoint(canvasPt);

            ShotKeyPoint = memPt;
            p_Cursor = Cursors.Pen;

            if (p_UIElement.Contains(ShotKeyPoint_UI))
                p_UIElement.Remove(ShotKeyPoint_UI);
            DrawShotKeyPoint();

            double shotCenterX = ShotKeyPoint.X + (DiePitchX / 4 * shotSizeX / 2);
            double shotCenterY = ShotKeyPoint.Y - (DiePitchY / 4 * ShotSizeY / 2);
            ShotOffsetX = (centerPoint.X - shotCenterX) * 4;
            ShotOffsetY = (shotCenterY - centerPoint.Y) * 4;
            ShotCenterPoint = new CPoint((int)(centerPoint.X - ShotOffsetX / 4), (int)(centerPoint.Y + ShotOffsetY / 4));
            //ShotCenterPoint.X = (int)(ShotKeyPoint.X + (DiePitchX * ShotSizeX));
            //ShotCenterPoint.Y = (int)(ShotKeyPoint.X + (DiePitchY * ShotSizeY));
            for (int i = 0; i < totalShot; i++)
            {
                if (ShotList.Count != 0 && ShotList[i] != null && p_UIElement.Contains(ShotList_UI[i]))
                    p_UIElement.Remove(ShotList_UI[i]);
            }
            totalShot = ShotSizeX * ShotSizeY;
            ShotList.Clear();
            ShotList_UI.Clear();
            for (byte i = 0; i < totalShot; i++)
            {

                Grid grid = new Grid();
                grid.Children.Add(new Line());
                grid.Children.Add(new Line());
                grid.Children.Add(new Line());
                grid.Children.Add(new Line());
                grid.Width = DiePitchX / 4;
                grid.Height = DiePitchY / 4;
                ShotList_UI.Add(grid);
                ShotList.Add(new CRect());
            }

            DrawShotList();


            if (p_UIElement.Contains(ShotCenterPoint_UI))
                p_UIElement.Remove(ShotCenterPoint_UI);

            DrawShotCenterPoint();


        }
        #endregion

        #region [Process Pitch]
        private enum PROCESS_PITCH_STATE
        {
            None,
            PitchRightTop,
        }

        PROCESS_PITCH_STATE pitchState = PROCESS_PITCH_STATE.None;

        public void ProcessPitch(MouseEventArgs e)
        {
            CPoint canvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint memPt = GetMemPoint(canvasPt);

            switch (pitchState)
            {
                case PROCESS_PITCH_STATE.None:
                    break;
                case PROCESS_PITCH_STATE.PitchRightTop:
                    p_Cursor = System.Windows.Input.Cursors.Arrow;

                    DrawPitchPoint();

                    int offset1 = Math.Abs(this.pitchRightTop.Y - this.originRightTop.Y);
                    int offset2 = Math.Abs(this.pitchRightBottom.Y - this.originLeftBottom.Y);
                    int offsetY = offset1 > offset2 ? offset1 : offset2;
                    int offsetX = Math.Abs(this.pitchRightTop.X - this.originRightTop.X);

                    this.pitchRightTop.Y = this.originRightTop.Y - offsetY;
                    this.pitchRightBottom.Y = this.originLeftBottom.Y + offsetY;

                    this.pitchBox.Left = this.originLeftBottom.X;
                    this.pitchBox.Right = this.originRightTop.X + offsetX;
                    this.pitchBox.Top = this.originRightTop.Y - offsetY;
                    this.pitchBox.Bottom = this.originLeftBottom.Y;

                    DrawPitchBox();


                    SetPitch();

                    ViewerState = SETUP_VIEWER_STATE.Normal;
                    pitchState = PROCESS_PITCH_STATE.None;
                    break;
            }
        }
        #endregion

        #region [Process Rular]
        public void ProcessRular(MouseEventArgs e)
        {

        }
        #endregion


        #region [Draw Method]

        public void SetOriginBox(CPoint originPt, int originWidth, int originHeight, int diePitchX, int diePitchY)
        {
            if (diePitchX == 0 || diePitchY == 0) return;

            this.IsPitchEnable = true;

            this.originLeftBottom.X = originPt.X;
            this.originLeftBottom.Y = originPt.Y;

            this.originRightTop.X = originPt.X + originWidth;
            this.originRightTop.Y = originPt.Y - originHeight;

            this.originBox.Left = this.originLeftBottom.X;
            this.originBox.Right = this.originRightTop.X;
            this.originBox.Top = this.originRightTop.Y;
            this.originBox.Bottom = this.originLeftBottom.Y;

            this.pitchRightTop.X = originPt.X + diePitchX;
            this.pitchRightTop.Y = originPt.Y - diePitchY;

            int offsetX = Math.Abs(diePitchX - originWidth);
            int offsetY = Math.Abs(diePitchY - originHeight);

            this.pitchRightBottom.X = originPt.X + diePitchX;
            this.pitchRightBottom.Y = originPt.Y + offsetY;

            this.pitchBox.Left = originPt.X - offsetX;
            this.pitchBox.Right = this.pitchRightBottom.X;
            this.pitchBox.Top = this.pitchRightTop.Y;
            this.pitchBox.Bottom = this.pitchRightBottom.Y;

            DrawOriginLeftBottomPoint(this.originLeftBottom, true);
            DrawOriginRightTopPoint(this.originRightTop, true);
            DrawPitchPoint(true);

            DrawOriginBox();
            DrawPitchBox();
        }

        Grid GetCenterNearlestUI(CPoint canvasPt)
        {
            Line near = CenterLTBox_UI.Children[0] as Line;

            double leftPt = Math.Abs(near.X1 - canvasPt.X);
            double topPt = Math.Abs(near.Y1 -canvasPt.Y);
            Grid grid = CenterLTBox_UI;
            double minDistance = double.MaxValue;
            //double top = double.MaxValue;

            //GetCanvasPoint(new CPoint(left, top)

           int i = 0;
            for(i = 0; i < p_UIElement.Count; i++)
            {
                if (p_UIElement[i] is Grid && ((Grid)p_UIElement[i]).Tag != null && ((Grid)p_UIElement[i]).Tag.ToString().Contains("Centering"))
                {
                    CPoint pt;
                    if (((Grid)p_UIElement[i]).Tag.ToString().Contains("LT"))
                    {
                        pt = GetCanvasPoint(new CPoint(centerBoxLT.Left, centerBoxLT.Top));
                    }
                    else if (((Grid)p_UIElement[i]).Tag.ToString().Contains("RT"))
                    {
                        pt = GetCanvasPoint(new CPoint(centerBoxRT.Left, centerBoxRT.Top));
                    }
                    else if (((Grid)p_UIElement[i]).Tag.ToString().Contains("RB"))
                    {
                        pt = GetCanvasPoint(new CPoint(centerBoxRB.Left, centerBoxRB.Top));
                    }
                    else
                    {
                        continue;
                    }

                    double distance = Math.Sqrt((canvasPt.X - pt.X) * (canvasPt.X - pt.X) + (canvasPt.Y - pt.Y) * (canvasPt.Y - pt.Y));
                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        //top = Math.Abs(canvasPt.Y - TopPos.X2);
                        grid = (Grid)p_UIElement[i];
                    }
                }
            } 
            return grid;
        }

        Grid nearGrid = null;
        private void DrawCenterLeftTopPoint(CPoint memPt, bool bRecipeLoaded = false)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            Grid grid = null;
            switch (centerPos)
            {
                case CENTERING_POS.None:
                    grid = GetCenterNearlestUI(canvasPt);
                    break;
                case CENTERING_POS.LT:
                    grid = CenterLTBox_UI;
                    break;
                case CENTERING_POS.RT:
                    grid = CenterRTBox_UI;
                    break;
                case CENTERING_POS.RB:
                    grid = CenterRBBox_UI;
                    break;
            }
            nearGrid = grid;
            //if(centerPos != CENTERING_POS.None)
            //    nearGrid = grid;
            //else
            //{
            //    nearGrid == 
            //}
            CenterLeftTop_UI.Width = 40;
            CenterLeftTop_UI.Height = 40;

            Line line1 = CenterLeftTop_UI.Children[0] as Line;
            line1.X1 = 0;
            line1.Y1 = 40;
            line1.X2 = 0;
            line1.Y2 = -10;
            line1.Stroke = DefineColors.CenteringColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = CenterLeftTop_UI.Children[1] as Line;
            line2.X1 = -10;
            line2.Y1 = 0;
            line2.X2 = 40;
            line2.Y2 = 0;
            line2.Stroke = DefineColors.CenteringColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            Canvas.SetLeft(CenterLeftTop_UI, canvasPt.X);
            Canvas.SetTop(CenterLeftTop_UI, canvasPt.Y);
            if (!p_UIElement.Contains(CenterLeftTop_UI))
            {
                p_UIElement.Add(CenterLeftTop_UI);
            }



        }

        private void DrawOriginLeftBottomPoint(CPoint memPt, bool bRecipeLoaded = false)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            OriginLeftBottom_UI.Width = 40;
            OriginLeftBottom_UI.Height = 40;

            Line line1 = OriginLeftBottom_UI.Children[0] as Line;
            line1.X1 = 0;
            line1.Y1 = -40;
            line1.X2 = 0;
            line1.Y2 = 10;
            line1.Stroke = DefineColors.OriginColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = OriginLeftBottom_UI.Children[1] as Line;
            line2.X1 = -10;
            line2.Y1 = 0;
            line2.X2 = 40;
            line2.Y2 = 0;
            line2.Stroke = DefineColors.OriginColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            Canvas.SetLeft(OriginLeftBottom_UI, canvasPt.X);
            Canvas.SetTop(OriginLeftBottom_UI, canvasPt.Y);
            if (!p_UIElement.Contains(OriginLeftBottom_UI))
            {
                p_UIElement.Add(OriginLeftBottom_UI);
            }

            //if (bRecipeLoaded == false)
            //{
            //    // Recipe
            //    RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            //    OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();

            //    originRecipe.Clear();

            //    originRecipe.OriginX = memPt.X;
            //    originRecipe.OriginY = memPt.Y;
            //}
        }

        private void DrawCenterRightBottomPoint(CPoint memPt, bool brecipeLoaded = false)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            CenterRightBottom_UI.Width = 40;
            CenterRightBottom_UI.Height = 40;

            Line line1 = CenterRightBottom_UI.Children[0] as Line;
            line1.X1 = 0;
            line1.Y1 = 10;
            line1.X2 = 0;
            line1.Y2 = -40;
            line1.Stroke = DefineColors.CenteringColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = CenterRightBottom_UI.Children[1] as Line;
            line2.X1 = -40;
            line2.Y1 = 0;
            line2.X2 = 10;
            line2.Y2 = 0;
            line2.Stroke = DefineColors.CenteringColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            Canvas.SetLeft(CenterRightBottom_UI, canvasPt.X);
            Canvas.SetTop(CenterRightBottom_UI, canvasPt.Y);

            if (!p_UIElement.Contains(CenterRightBottom_UI))
            {
                p_UIElement.Add(CenterRightBottom_UI);
            }


        }

        private void DrawOriginRightTopPoint(CPoint memPt, bool bRecipeLoaded = false)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            OriginRightTop_UI.Width = 40;
            OriginRightTop_UI.Height = 40;


            Line line1 = OriginRightTop_UI.Children[0] as Line;
            line1.X1 = 0;
            line1.Y1 = -10;
            line1.X2 = 0;
            line1.Y2 = 40;
            line1.Stroke = DefineColors.OriginColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = OriginRightTop_UI.Children[1] as Line;
            line2.X1 = -40;
            line2.Y1 = 0;
            line2.X2 = 10;
            line2.Y2 = 0;
            line2.Stroke = DefineColors.OriginColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            Canvas.SetLeft(OriginRightTop_UI, canvasPt.X);
            Canvas.SetTop(OriginRightTop_UI, canvasPt.Y);

            if (!p_UIElement.Contains(OriginRightTop_UI))
            {
                p_UIElement.Add(OriginRightTop_UI);
            }

            //if (bRecipeLoaded == false)
            //{
            //    // Recipe
            //    RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            //    OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();

            //    originRecipe.OriginWidth = memPt.X - originRecipe.OriginX;
            //    originRecipe.OriginHeight = originRecipe.OriginY - memPt.Y;

            //    originRecipe.DiePitchX = originRecipe.OriginWidth;
            //    originRecipe.DiePitchY = originRecipe.OriginHeight;
            //}
        }

        private void DrawPitchPoint(bool bRecipeLoaded = false)
        {
            int offset1 = Math.Abs(this.pitchRightTop.Y - this.originRightTop.Y);
            int offset2 = Math.Abs(this.pitchRightBottom.Y - this.originLeftBottom.Y);
            int offsetY = offset1 > offset2 ? offset1 : offset2;
            int offsetX = Math.Abs(this.pitchRightTop.X - this.originRightTop.X);

            this.pitchRightTop.Y = this.originRightTop.Y - offsetY;
            this.pitchRightBottom.Y = this.originLeftBottom.Y;

            CPoint canvasPt1 = GetCanvasPoint(pitchRightTop);

            Canvas.SetLeft(PitchRightTop_UI, canvasPt1.X);
            Canvas.SetTop(PitchRightTop_UI, canvasPt1.Y);

            CPoint canvasPt2 = GetCanvasPoint(pitchRightBottom);

            Canvas.SetLeft(PitchRightBottom_UI, canvasPt2.X);
            Canvas.SetTop(PitchRightBottom_UI, canvasPt2.Y);

            if (!this.p_UIElement.Contains(PitchRightTop_UI))
                this.p_UIElement.Add(PitchRightTop_UI);

            if (!this.p_UIElement.Contains(PitchRightBottom_UI))
                this.p_UIElement.Add(PitchRightBottom_UI);

            //if (bRecipeLoaded == false)
            //{
            //    // Recipe
            //    RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            //    OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();

            //    originRecipe.DiePitchX = originRecipe.OriginWidth + offsetX;
            //    originRecipe.DiePitchY = originRecipe.OriginHeight + offsetY;
            //}
        }

       

        private void DrawingPitchPoint()
        {
            CPoint memPt = new CPoint(p_MouseMemX, p_MouseMemY);

            PitchRightTop_UI.Width = 40;
            PitchRightTop_UI.Height = 40;

            PitchRightBottom_UI.Width = 40;
            PitchRightBottom_UI.Height = 40;


            this.pitchRightTop.X = memPt.X < originRightTop.X ? originRightTop.X : memPt.X;
            this.pitchRightTop.Y = memPt.Y > originRightTop.Y ? originRightTop.Y : memPt.Y;

            this.pitchRightBottom.X = memPt.X < originRightTop.X ? originRightTop.X : memPt.X;
            this.pitchRightBottom.Y = memPt.Y < originLeftBottom.Y ? originLeftBottom.Y : memPt.Y;

            CPoint canvasPt1 = GetCanvasPoint(pitchRightTop);

            Canvas.SetLeft(PitchRightTop_UI, canvasPt1.X);
            Canvas.SetTop(PitchRightTop_UI, canvasPt1.Y);

            CPoint canvasPt2 = GetCanvasPoint(pitchRightBottom);

            Canvas.SetLeft(PitchRightBottom_UI, canvasPt2.X);
            Canvas.SetTop(PitchRightBottom_UI, canvasPt2.Y);

            if (!p_UIElement.Contains(PitchRightTop_UI))
            {
                p_UIElement.Add(PitchRightTop_UI);
            }

            if (!p_UIElement.Contains(PitchRightBottom_UI))
            {
                p_UIElement.Add(PitchRightBottom_UI);
            }
        }

        private void RedrawPoint(CPoint memPt, Grid grid)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            OriginLeftBottom_UI.Width = 40;
            OriginLeftBottom_UI.Height = 40;

            Line line1 = grid.Children[0] as Line;
            line1.X1 = -10;
            line1.Y1 = 10;
            line1.X2 = 10;
            line1.Y2 = -10;
            line1.Stroke = DefineColors.CenteringPointColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = grid.Children[1] as Line;
            line2.X1 = 10;
            line2.Y1 = 10;
            line2.X2 = -10;
            line2.Y2 = -10;
            line2.Stroke = DefineColors.CenteringPointColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            Canvas.SetLeft(grid, canvasPt.X);
            Canvas.SetTop(grid, canvasPt.Y);
            //if (!p_UIElement.Contains(grid))
            //{
            //    p_UIElement.Add(grid);
            //}
        }
        public void CheckPointRange(CRect Image, ref int x, ref int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x >= Image.Width) x = Image.Width - 1;
            if (y >= Image.Height) y = Image.Height - 1;
        }
        private void CalcCenterPoint()
        {
            byte[] byteRect = null;
            CRect box = new CRect();
            Grid grid = new Grid();
            CPoint pt = new CPoint();
            CPoint startPT = new CPoint();
            switch (centerPos)
            {
                case CENTERING_POS.LT:  
                    box = centerBoxLT;
                    byteRect = p_ImageData.GetRectByteArray(centerBoxLT);
                    grid = CenterLTPoint_UI;
                    pt = new CPoint(0, 1);
                    startPT = new CPoint(centerBoxLT.Width / 2, 0);
                    break;
                case CENTERING_POS.RT:
                    box = centerBoxRT;
                    byteRect = p_ImageData.GetRectByteArray(centerBoxRT);
                    grid = CenterRTPoint_UI;
                    pt = new CPoint(0, 1);
                    startPT = new CPoint(centerBoxRT.Width / 2, 0);
                    break;
                case CENTERING_POS.RB:
                    box = centerBoxRB;
                    byteRect = p_ImageData.GetRectByteArray(centerBoxRB);
                    grid = CenterRBPoint_UI;
                    pt = new CPoint(0, -1);
                    startPT = new CPoint(centerBoxRB.Width / 2, centerBoxRB.Height);
                    break;
                case CENTERING_POS.None:
                    if (nearGrid.Tag.ToString().Contains("LT"))
                    {
                        box = centerBoxLT;
                        byteRect = p_ImageData.GetRectByteArray(centerBoxLT);
                        grid = CenterLTPoint_UI;
                        pt = new CPoint(0, 1);
                        startPT = new CPoint(centerBoxLT.Width / 2, 0);
                    }
                    else if (nearGrid.Tag.ToString().Contains("RT"))
                    {
                        box = centerBoxRT;
                        byteRect = p_ImageData.GetRectByteArray(centerBoxRT);
                        grid = CenterRTPoint_UI;
                        pt = new CPoint(0, 1);
                        startPT = new CPoint(centerBoxRT.Width / 2, 0);
                    }
                    else
                    {
                        box = centerBoxRB;
                        byteRect = p_ImageData.GetRectByteArray(centerBoxRB);
                        grid = CenterRBPoint_UI;
                        pt = new CPoint(0, -1);
                        startPT = new CPoint(centerBoxRB.Width / 2, centerBoxRB.Height);
                    }
                    break;
            }
            

            //Mat mat = new Mat(,);
            byte minValue = Byte.MaxValue;
            byte maxValue = Byte.MinValue;

            byte min = 0, max = 0;
            double vectorX = 0;
            double vectorY = 0;

           Parallel.For(0, box.Height, x =>
           {
               int LineSum = 0;
               for (int j = 0; j < box.Width; j++)
               {
                   //int x = (int)(j + 0.5);
                   //int y = (int)(vectorX + 0.5);
                   //CheckPointRange(Image, ref x, ref y);
                   LineSum += byteRect[x * box.Width + j];
               }

               byte LineAvg = (byte)(LineSum / box.Width);
               if (LineAvg < minValue)
               {
                   minValue = LineAvg;
               }
               if (LineAvg > maxValue)
               {
                   maxValue = LineAvg;
               }
               //vectorX++;
               vectorY++;
               min = minValue;
               max = maxValue;

           });


            double dProx = 0;
            if (max - min < 50)
            {
                dProx = 0;
            }
            else
            {
                dProx = (double)(min + (max - min) * 30 * 0.01);
            }


            byte prev = 0;
            byte current = 0;

            int x = 0, y = 0;
            double positionX = 0, positionY = 0;
            Point result = new Point();
            for (int i = 0; i < box.Height; i++)
            {

                x = (int)(startPT.X + positionX + 0.5);
                y = (int)(startPT.Y + positionY + 0.5);
                CheckPointRange(box, ref x, ref y);
               current = byteRect[y * box.Width + x];
                if ((current >= dProx && dProx > prev) ||
                   (current <= dProx && dProx < prev))
                {
                    result.X = x;
                    result.Y = y;
                    break;
                }

                prev = current;

                positionX += pt.X;
                positionY += pt.Y;
            }

            if (p_UIElement.Contains(grid))
                p_UIElement.Remove(grid);
            Line line1 = grid.Children[0] as Line;
            line1.X1 = -10;
            line1.Y1 = 10;
            line1.X2 = 10;
            line1.Y2 = -10;
            line1.Stroke = DefineColors.CenteringPointColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = grid.Children[1] as Line;
            line2.X1 = 10;
            line2.Y1 = 10;
            line2.X2 = -10;
            line2.Y2 = -10;
            line2.Stroke = DefineColors.CenteringPointColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;
            CPoint memPt = new CPoint((int)(box.Left + result.X), (int)(box.Top + result.Y));
            CPoint canvasPt = GetCanvasPoint(new CPoint((int)(box.Left + result.X), (int)(box.Top + result.Y)));
            Canvas.SetLeft(grid, canvasPt.X);
            Canvas.SetTop(grid, canvasPt.Y);
            p_UIElement.Add(grid);

            switch (centerPos)
            {
                case CENTERING_POS.LT:
                    CenterLTPoint.X = memPt.X;
                    CenterLTPoint.Y = memPt.Y;
                    break;
                case CENTERING_POS.RT:
                    CenterRTPoint.X = memPt.X;
                    CenterRTPoint.Y = memPt.Y;
                    break;
                case CENTERING_POS.RB:
                    CenterRBPoint.X = memPt.X;
                    CenterRBPoint.Y = memPt.Y;
                    break;
                case CENTERING_POS.None:
                    if (nearGrid.Tag.ToString().Contains("LT"))
                    {
                        CenterLTPoint.X = memPt.X;
                        CenterLTPoint.Y = memPt.Y;
                    }
                    else if (nearGrid.Tag.ToString().Contains("RT"))
                    {
                        CenterRTPoint.X = memPt.X;
                        CenterRTPoint.Y = memPt.Y;
                    }
                    else
                    {
                        CenterRBPoint.X = memPt.X;
                        CenterRBPoint.Y = memPt.Y;
                    }
                    break;
            }
                    //pt = canvasPt;
        }

        private void DrawCenterBox(Grid grid = null, CENTERING_POS pos = CENTERING_POS.None)
        {
            CRect centerBox = new CRect();

            Grid redrawGrid = null;
            if (grid != null)
                redrawGrid = grid;
            else
                redrawGrid = nearGrid;

            switch (pos)
            {
                case CENTERING_POS.LT:
                    centerBox = centerBoxLT;
                    break;
                case CENTERING_POS.RT:
                    centerBox = centerBoxRT;
                    break;
                case CENTERING_POS.RB:
                    centerBox = centerBoxRB;
                    break;
                case CENTERING_POS.None:
                    if (redrawGrid.Tag.ToString().Contains("LT"))
                        centerBox = centerBoxLT;
                    else if (redrawGrid.Tag.ToString().Contains("RT"))
                        centerBox = centerBoxRT;
                    else
                        centerBox = centerBoxRB;
                    break;
            }
            int left = centerBox.Left;
            int top = centerBox.Top;

            int right = centerBox.Right;
            int bottom = centerBox.Bottom;

            

            CPoint canvasLeftTop = GetCanvasPoint(new CPoint(left, top));
            CPoint canvasLeftBottom = GetCanvasPoint(new CPoint(left, bottom));
            CPoint canvasRightTop = GetCanvasPoint(new CPoint(right, top));
            CPoint canvasRightBottom = GetCanvasPoint(new CPoint(right, bottom));

            int offset = 40; // OriginPoint Line 길이

            
            redrawGrid.Width = Math.Abs(canvasRightTop.X - canvasLeftTop.X);
            redrawGrid.Height = Math.Abs(canvasLeftBottom.Y - canvasLeftTop.Y);

            
            // Left
            Line leftLine = redrawGrid.Children[0] as Line;
            leftLine.X1 = 0;
            leftLine.Y1 = 0;
            leftLine.X2 = 0;
            leftLine.Y2 = redrawGrid.Height;
            leftLine.Stroke = DefineColors.CenteringColor;
            leftLine.StrokeThickness = 1;
            leftLine.Opacity = 0.75;
            //leftLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Top
            Line topLine = redrawGrid.Children[1] as Line;
            topLine.X1 = 0;
            topLine.Y1 = 0;
            topLine.X2 = redrawGrid.Width;
            topLine.Y2 = 0;
            topLine.Stroke = DefineColors.CenteringColor;
            topLine.StrokeThickness = 1;
            topLine.Opacity = 0.75;
            //topLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Right
            Line rightLine = redrawGrid.Children[2] as Line;
            rightLine.X1 = redrawGrid.Width;
            rightLine.Y1 = 0;
            rightLine.X2 = redrawGrid.Width;
            rightLine.Y2 = redrawGrid.Height;
            rightLine.Stroke = DefineColors.CenteringColor;
            rightLine.StrokeThickness = 1;
            rightLine.Opacity = 0.75;
            //rightLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // bottom
            Line bottomLine = redrawGrid.Children[3] as Line;
            bottomLine.X1 = 0;
            bottomLine.Y1 = redrawGrid.Height;
            bottomLine.X2 = redrawGrid.Width;
            bottomLine.Y2 = redrawGrid.Height;
            bottomLine.Stroke = DefineColors.CenteringColor;
            bottomLine.StrokeThickness = 1;
            bottomLine.Opacity = 0.75;
            //bottomLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            Canvas.SetLeft(redrawGrid, canvasLeftTop.X);
            Canvas.SetTop(redrawGrid, canvasLeftTop.Y);

            if (!p_UIElement.Contains(redrawGrid))
            {
                p_UIElement.Add(redrawGrid);
            }
        }

        private void DrawOriginBox()
        {
            int left = this.originBox.Left;
            int top = this.originBox.Top;

            int right = this.originBox.Right;
            int bottom = this.originBox.Bottom;

            
            CPoint canvasLeftTop = GetCanvasPoint(new CPoint(left, top));
            CPoint canvasLeftBottom = GetCanvasPoint(new CPoint(left, bottom));
            CPoint canvasRightTop = GetCanvasPoint(new CPoint(right, top));
            CPoint canvasRightBottom = GetCanvasPoint(new CPoint(right, bottom));

            int offset = 40; // OriginPoint Line 길이

            OriginBox_UI.Width = Math.Abs(canvasRightTop.X - canvasLeftTop.X);
            OriginBox_UI.Height = Math.Abs(canvasLeftBottom.Y - canvasLeftTop.Y);

            // Left
            Line leftLine = OriginBox_UI.Children[0] as Line;
            leftLine.X1 = 0;
            leftLine.Y1 = 0;
            leftLine.X2 = 0;
            leftLine.Y2 = OriginBox_UI.Height - offset;
            leftLine.Stroke = DefineColors.OriginBoxColor;
            leftLine.StrokeThickness = 1;
            leftLine.Opacity = 0.75;
            leftLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Top
            Line topLine = OriginBox_UI.Children[1] as Line;
            topLine.X1 = 0;
            topLine.Y1 = 0;
            topLine.X2 = OriginBox_UI.Width - offset;
            topLine.Y2 = 0;
            topLine.Stroke = DefineColors.OriginBoxColor;
            topLine.StrokeThickness = 1;
            topLine.Opacity = 0.75;
            topLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Right
            Line rightLine = OriginBox_UI.Children[2] as Line;
            rightLine.X1 = OriginBox_UI.Width;
            rightLine.Y1 = offset;
            rightLine.X2 = OriginBox_UI.Width;
            rightLine.Y2 = OriginBox_UI.Height;
            rightLine.Stroke = DefineColors.OriginBoxColor;
            rightLine.StrokeThickness = 1;
            rightLine.Opacity = 0.75;
            rightLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // bottom
            Line bottomLine = OriginBox_UI.Children[3] as Line;
            bottomLine.X1 = offset;
            bottomLine.Y1 = OriginBox_UI.Height;
            bottomLine.X2 = OriginBox_UI.Width;
            bottomLine.Y2 = OriginBox_UI.Height;
            bottomLine.Stroke = DefineColors.OriginBoxColor;
            bottomLine.StrokeThickness = 1;
            bottomLine.Opacity = 0.75;
            bottomLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            Canvas.SetLeft(OriginBox_UI, canvasLeftTop.X);
            Canvas.SetTop(OriginBox_UI, canvasLeftTop.Y);

            if (!p_UIElement.Contains(OriginBox_UI))
            {
                p_UIElement.Add(OriginBox_UI);
            }
        }


        private void DrawPitchBox()
        {
            int left = pitchBox.Left;
            int top = pitchBox.Top;

            int right = pitchBox.Right;
            int bottom = pitchBox.Bottom;


            DiePitchX = pitchBox.Width * 4;
            DiePitchY = pitchBox.Height * 4;

            ScribeLaneX = (pitchBox.Width - originBox.Width) * 4;
            ScribeLaneY = (pitchBox.Height - originBox.Height) * 4;

            CPoint canvasLeftTop = GetCanvasPoint(new CPoint(left, top));
            CPoint canvasLeftBottom = GetCanvasPoint(new CPoint(left, bottom));
            CPoint canvasRightTop = GetCanvasPoint(new CPoint(right, top));
            CPoint canvasRightBottom = GetCanvasPoint(new CPoint(right, bottom));

            PitchBox_UI.Width = Math.Abs(canvasRightTop.X - canvasLeftTop.X);
            PitchBox_UI.Height = Math.Abs(canvasLeftBottom.Y - canvasLeftTop.Y);

            // Left
            Line leftLine = PitchBox_UI.Children[0] as Line;
            leftLine.X1 = 0;
            leftLine.Y1 = 0;
            leftLine.X2 = 0;
            leftLine.Y2 = PitchBox_UI.Height;
            leftLine.Stroke = DefineColors.PitchBoxColor;
            leftLine.StrokeThickness = 1;
            leftLine.Opacity = 1;
            leftLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Top
            Line topLine = PitchBox_UI.Children[1] as Line;
            topLine.X1 = 0;
            topLine.Y1 = 0;
            topLine.X2 = PitchBox_UI.Width;
            topLine.Y2 = 0;
            topLine.Stroke = DefineColors.PitchBoxColor;
            topLine.StrokeThickness = 1;
            topLine.Opacity = 1;
            topLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Right
            Line rightLine = PitchBox_UI.Children[2] as Line;
            rightLine.X1 = PitchBox_UI.Width;
            rightLine.Y1 = 0;
            rightLine.X2 = PitchBox_UI.Width;
            rightLine.Y2 = PitchBox_UI.Height;
            rightLine.Stroke = DefineColors.PitchBoxColor;
            rightLine.StrokeThickness = 1;
            rightLine.Opacity = 1;
            rightLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // bottom
            Line bottomLine = PitchBox_UI.Children[3] as Line;
            bottomLine.X1 = 0;
            bottomLine.Y1 = PitchBox_UI.Height;
            bottomLine.X2 = PitchBox_UI.Width;
            bottomLine.Y2 = PitchBox_UI.Height;
            bottomLine.Stroke = DefineColors.PitchBoxColor;
            bottomLine.StrokeThickness = 1;
            bottomLine.Opacity = 1;
            bottomLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            Canvas.SetLeft(PitchBox_UI, canvasLeftTop.X);
            Canvas.SetTop(PitchBox_UI, canvasLeftTop.Y);

            if (!p_UIElement.Contains(PitchBox_UI))
            {
                p_UIElement.Add(PitchBox_UI);
            }
        }

        private void RedrawShapes(object sender, EventArgs e)
        {
            if (p_UIElement.Contains(OriginLeftBottom_UI))
            {
                DrawOriginLeftBottomPoint(originLeftBottom);
            }
            if (p_UIElement.Contains(OriginRightTop_UI))
            {
                DrawOriginRightTopPoint(originRightTop);
            }
            if (p_UIElement.Contains(PitchRightTop_UI))
            {
                DrawPitchPoint();
            }

            if (p_UIElement.Contains(OriginBox_UI))
            {
                DrawOriginBox();
            }

            if (p_UIElement.Contains(PitchBox_UI))
            {
                DrawPitchBox();
            }

            //if (p_UIElement.Contains(CenterLeftTop_UI))
            //{
            //    DrawCenterLeftTopPoint(centerLeftTop);
            //}

            //if (p_UIElement.Contains(CenterRightBottom_UI))
            //{
            //    DrawCenterLeftTopPoint(centerRightBottom);
            //}

            if (p_UIElement.Contains(CenterLTBox_UI))
            {
                DrawCenterBox(CenterLTBox_UI, CENTERING_POS.LT);
            }

            if (p_UIElement.Contains(CenterRTBox_UI))
            {
                DrawCenterBox(CenterRTBox_UI, CENTERING_POS.RT);
            }

            if (p_UIElement.Contains(CenterRBBox_UI))
            {
                DrawCenterBox(CenterRBBox_UI, CENTERING_POS.RB);
            }

            if (p_UIElement.Contains(CenterLTPoint_UI))
            {
                RedrawPoint(CenterLTPoint, CenterLTPoint_UI);
            }
            if (p_UIElement.Contains(CenterRTPoint_UI))
            {
                RedrawPoint(CenterRTPoint, CenterRTPoint_UI);
            }
            if (p_UIElement.Contains(CenterRBPoint_UI))
            {
                RedrawPoint(CenterRBPoint, CenterRBPoint_UI);
            }

            if (p_UIElement.Contains(CenterPoint_UI))
            {
                DrawCenter();
            }

            if (p_UIElement.Contains(ShotCenterPoint_UI))
            {
                DrawShotCenterPoint();
            }

            if (p_UIElement.Contains(ShotKeyPoint_UI))
            {
                DrawShotKeyPoint();
            }

            for(int i = 0; i < totalShot; i++)
            {
                if (ShotList[i] != null && p_UIElement.Contains(ShotList_UI[i]))
                {
                    DrawShot(i);
                }
            }
            //if (p_UIElement.Contains(CenterLTPoint_UI))
            //{
            //    CalcCenter();
            //}
        }

        void Get3PointCircle(CPoint p0, CPoint p1, CPoint p2, out double cx, out double cy, out double r)
        {
            double dy1, dy2, d, d2, yi;
            CPoint P1 = new CPoint((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2);
            CPoint P2 = new CPoint((p0.X + p2.X) / 2, (p0.Y + p2.Y) / 2);
            dy1 = p0.Y - p1.Y;
            dy2 = p0.Y - p2.Y;
            cx = 0;
            cy = 0;
            r = 0;
            if (dy1 != 0)
            {
                d = (p1.X - p0.X) / dy1;
                yi = P1.Y - d * P1.X;
                if (dy2 != 0)
                {
                    d2 = (p2.X - p0.X) / dy2;
                    if (d != d2) cx = (yi - (P2.Y - d2 * P2.X)) / (d2 - d);
                    else return;
                }
                else if (p2.X - p0.X == 0) return;
                else cx = P2.X;
            }
            else if (dy2 != 0 && p1.X - p0.X != 0)
            {
                d = (p2.X - p0.X) / dy2;
                yi = P2.Y - d * P2.X;
                cx = P1.X;
            }
            else return;
            cy = d * cx + yi;
            r = Math.Sqrt((p0.X - cx) * (p0.X - cx) + (p0.Y - cy) * (p0.Y - cy));
        }

        void DrawCenter()
        {
            Line line1 = CenterPoint_UI.Children[0] as Line;
            line1.X1 = -10;
            line1.Y1 = 10;
            line1.X2 = 10;
            line1.Y2 = -10;
            line1.Stroke = DefineColors.CenteringPointColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = CenterPoint_UI.Children[1] as Line;
            line2.X1 = 10;
            line2.Y1 = 10;
            line2.X2 = -10;
            line2.Y2 = -10;
            line2.Stroke = DefineColors.CenteringPointColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;
            CPoint memPt = new CPoint((int)centerPoint.X, (int)centerPoint.Y);
            CPoint canvasPt = GetCanvasPoint(new CPoint((int)centerPoint.X, (int)(centerPoint.Y)));
            Canvas.SetLeft(CenterPoint_UI, canvasPt.X);
            Canvas.SetTop(CenterPoint_UI, canvasPt.Y);

        }
        
        void SetData()
        {
            if (!IsLoad)
            {
                MapOffsetX = (centerPoint.X - originBox.Left) * 4;
                MapOffsetY = (originBox.Bottom - centerPoint.Y) * 4;
            }
            //CPoint canvasPt = new CPoint(centerPoint.X - , p_MouseY);
            CPoint memPt = new CPoint((int)(centerPoint.X - MapOffsetX / 4), (int)(centerPoint.Y + MapOffsetY/ 4));

            originLeftBottom.X = memPt.X;
            originLeftBottom.Y = memPt.Y;
            DrawOriginLeftBottomPoint(originLeftBottom);
            originRightTop.X = (int)(originLeftBottom.X + DiePitchX /4 - ScribeLaneX / 4);
            originRightTop.Y = (int)(originLeftBottom.Y - DiePitchY/ 4 + scribeLaneY / 4);


            //originRightTop.X = memPt.X;
            //originRightTop.Y = memPt.Y;

            originBox.Left = originLeftBottom.X;
            originBox.Right = originRightTop.X;
            originBox.Top = originRightTop.Y;
            originBox.Bottom = originLeftBottom.Y;

            DrawOriginRightTopPoint(originRightTop);
            DrawingSetPitchPoint();
            DrawOriginBox();

            DrawPitchBox();


            if (!IsLoad)
            {
                double shotCenterX = ShotKeyPoint.X + (DiePitchX / 4 * shotSizeX / 2);
                double shotCenterY = ShotKeyPoint.Y - (DiePitchY / 4 * ShotSizeY / 2);
                ShotOffsetX = (centerPoint.X - shotCenterX) * 4;
                ShotOffsetY = (shotCenterY - centerPoint.Y) * 4;
            }


            //memPt = new CPoint((int)(centerPoint.X - xmlData.MapOffsetX / 4), (int)(centerPoint.Y + xmlData.MapOffsetY / 4));
            if (p_UIElement.Contains(ShotCenterPoint_UI))
                p_UIElement.Remove(ShotCenterPoint_UI);

            DrawShotCenterPoint();



            if (p_UIElement.Contains(ShotKeyPoint_UI))
                p_UIElement.Remove(ShotKeyPoint_UI);
            DrawShotKeyPoint();






            for (int i= 0; i < totalShot; i++)
            {
                if (ShotList.Count != 0 && ShotList[i] != null && p_UIElement.Contains(ShotList_UI[i]))
                    p_UIElement.Remove(ShotList_UI[i]);
            }
            totalShot = ShotSizeX * ShotSizeY;
            ShotList.Clear();
            ShotList_UI.Clear();
            for (byte i = 0; i < totalShot; i++)
            {
                
                Grid grid = new Grid();
                grid.Children.Add(new Line());
                grid.Children.Add(new Line());
                grid.Children.Add(new Line());
                grid.Children.Add(new Line());
                grid.Width = DiePitchX / 4; 
                grid.Height = DiePitchY / 4;
                ShotList_UI.Add(grid);
                ShotList.Add(new CRect());
            }

            DrawShotList();
        }

        private void DrawShot(int idx)
        {
            double left = ShotList[idx].Left;
            double top = ShotList[idx].Top;

            double right = ShotList[idx].Right;
            double bottom = ShotList[idx].Bottom;

            CPoint canvasLeftTop = GetCanvasPoint(new CPoint((int)left, (int)top));
            CPoint canvasLeftBottom = GetCanvasPoint(new CPoint((int)left, (int)bottom));
            CPoint canvasRightTop = GetCanvasPoint(new CPoint((int)right, (int)top));
            CPoint canvasRightBottom = GetCanvasPoint(new CPoint((int)right, (int)bottom));

            ShotList_UI[idx].Width = Math.Abs(canvasRightTop.X - canvasLeftTop.X);
            ShotList_UI[idx].Height = Math.Abs(canvasLeftBottom.Y - canvasLeftTop.Y);

            Line leftLine = ShotList_UI[idx].Children[0] as Line;
            leftLine.X1 = 0;
            leftLine.Y1 = 0;
            leftLine.X2 = 0;
            leftLine.Y2 = ShotList_UI[idx].Height;
            leftLine.Stroke = DefineColors.PitchBoxColor;
            leftLine.StrokeThickness = 2;
            leftLine.Opacity = 1;
            //leftLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Top
            Line topLine = ShotList_UI[idx].Children[1] as Line;
            topLine.X1 = 0;
            topLine.Y1 = 0;
            topLine.X2 = ShotList_UI[idx].Width;
            topLine.Y2 = 0;
            topLine.Stroke = DefineColors.PitchBoxColor;
            topLine.StrokeThickness = 2;
            topLine.Opacity = 1;
            //topLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Right
            Line rightLine = ShotList_UI[idx].Children[2] as Line;
            rightLine.X1 = ShotList_UI[idx].Width;
            rightLine.Y1 = 0;
            rightLine.X2 = ShotList_UI[idx].Width;
            rightLine.Y2 = ShotList_UI[idx].Height;
            rightLine.Stroke = DefineColors.PitchBoxColor;
            rightLine.StrokeThickness = 2;
            rightLine.Opacity = 1;
            //rightLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // bottom
            Line bottomLine = ShotList_UI[idx].Children[3] as Line;
            bottomLine.X1 = 0;
            bottomLine.Y1 = ShotList_UI[idx].Height;
            bottomLine.X2 = ShotList_UI[idx].Width;
            bottomLine.Y2 = ShotList_UI[idx].Height;
            bottomLine.Stroke = DefineColors.PitchBoxColor;
            bottomLine.StrokeThickness = 2;
            bottomLine.Opacity = 1;
            //bottomLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            //ShotList[idx] = new CPoint((int)x, (int)y);
            //CPoint memPt;
            //memPt.X = ShotList[idx].Left;
            //CPoint canvasPt = GetCanvasPoint(memPt);
            Canvas.SetLeft(ShotList_UI[idx], canvasLeftTop.X);
            Canvas.SetTop(ShotList_UI[idx], canvasLeftTop.Y);

            if (!p_UIElement.Contains(ShotList_UI[idx]))
            {
                p_UIElement.Add(ShotList_UI[idx]);
            }
        }
        private void DrawShotList()
        {
            //double x = ShotKeyPoint.X;
            //double y = ShotKeyPoint.Y;
            int sizeX = ShotSizeX;
            int sizeY = ShotSizeY;
            for (int i = 0; i < sizeY; i++)
            {
                double y = ShotKeyPoint.Y;
                y -= i * DiePitchY / 4;
                for (int j = 0; j < sizeX; j++)
                {
                    double x = ShotKeyPoint.X;
                    x += j * DiePitchX / 4;

                    double left = x;
                    double top = y - DiePitchY / 4;

                    double right = x + DiePitchX / 4;
                    double bottom = y;

                    ShotList[(i * sizeX) + j].Left = (int)left;
                    ShotList[(i * sizeX) + j].Top = (int)top;
                    ShotList[(i * sizeX) + j].Right = (int)right;
                    ShotList[(i * sizeX) + j].Bottom = (int)bottom;

                    CPoint canvasLeftTop = GetCanvasPoint(new CPoint((int)left, (int)top));
                    CPoint canvasLeftBottom = GetCanvasPoint(new CPoint((int)left, (int)bottom));
                    CPoint canvasRightTop = GetCanvasPoint(new CPoint((int)right, (int)top));
                    CPoint canvasRightBottom = GetCanvasPoint(new CPoint((int)right, (int)bottom));

                    ShotList_UI[(i * sizeX) + j].Width = Math.Abs(canvasRightTop.X - canvasLeftTop.X);
                    ShotList_UI[(i * sizeX) + j].Height = Math.Abs(canvasLeftBottom.Y - canvasLeftTop.Y);

                    // Left
                    Line leftLine = ShotList_UI[(i * sizeX) + j].Children[0] as Line;
                    leftLine.X1 = 0;
                    leftLine.Y1 = 0;
                    leftLine.X2 = 0;
                    leftLine.Y2 = ShotList_UI[(i * sizeX) + j].Height;
                    leftLine.Stroke = DefineColors.PitchBoxColor;
                    leftLine.StrokeThickness = 2;
                    leftLine.Opacity = 1;
                    //leftLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

                    // Top
                    Line topLine = ShotList_UI[(i * sizeX) + j].Children[1] as Line;
                    topLine.X1 = 0;
                    topLine.Y1 = 0;
                    topLine.X2 = ShotList_UI[(i * sizeX) + j].Width;
                    topLine.Y2 = 0;
                    topLine.Stroke = DefineColors.PitchBoxColor;
                    topLine.StrokeThickness = 2;
                    topLine.Opacity = 1;
                    //topLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

                    // Right
                    Line rightLine = ShotList_UI[(i * sizeX) + j].Children[2] as Line;
                    rightLine.X1 = ShotList_UI[(i * sizeX) + j].Width;
                    rightLine.Y1 = 0;
                    rightLine.X2 = ShotList_UI[(i * sizeX) + j].Width;
                    rightLine.Y2 = ShotList_UI[(i * sizeX) + j].Height;
                    rightLine.Stroke = DefineColors.PitchBoxColor;
                    rightLine.StrokeThickness = 2;
                    rightLine.Opacity = 1;
                    //rightLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

                    // bottom
                    Line bottomLine = ShotList_UI[(i * sizeX) + j].Children[3] as Line;
                    bottomLine.X1 = 0;
                    bottomLine.Y1 = ShotList_UI[(i * sizeX) + j].Height;
                    bottomLine.X2 = ShotList_UI[(i * sizeX) + j].Width;
                    bottomLine.Y2 = ShotList_UI[(i * sizeX) + j].Height;
                    bottomLine.Stroke = DefineColors.PitchBoxColor;
                    bottomLine.StrokeThickness = 2;
                    bottomLine.Opacity = 1;
                    //bottomLine.StrokeDashArray = new DoubleCollection() { 6, 2 };


                    //CPoint memPt = ShotList[(i * xmlData.ShotX) + j];
                    //CPoint canvasPt = GetCanvasPoint(memPt);

                    Canvas.SetLeft(ShotList_UI[(i * sizeX) + j], canvasLeftTop.X);
                    Canvas.SetTop(ShotList_UI[(i * sizeX) + j], canvasLeftTop.Y);

                    if (!p_UIElement.Contains(ShotList_UI[(i * sizeX) + j]))
                    {
                        p_UIElement.Add(ShotList_UI[(i * sizeX) + j]);
                    }
                    
                }
            }
        }

        private void DrawShotKeyPoint()
        {
            Line line1 = ShotKeyPoint_UI.Children[0] as Line;
            line1.X1 = -10;
            line1.Y1 = 10;
            line1.X2 = 10;
            line1.Y2 = -10;
            line1.Stroke = DefineColors.ShotKeyPointColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = ShotKeyPoint_UI.Children[1] as Line;
            line2.X1 = 10;

            line2.Y1 = 10;
            line2.X2 = -10;
            line2.Y2 = -10;
            line2.Stroke = DefineColors.ShotKeyPointColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            //ShotCenterPoint = new CPoint((int)(centerPoint.X - xmlData.ShotOffsetX / 4), (int)(centerPoint.Y + xmlData.ShotOffsetY / 4));
            if (IsLoad)
            {
                ShotKeyPoint = new CPoint((int)(ShotCenterPoint.X - (ShotSizeX * DiePitchX / 4) / 2),(int)(ShotCenterPoint.Y + (ShotSizeY * DiePitchY / 4) / 2));
                IsLoad = false;
            }
            else
            {
                
            }

            CPoint memPt = ShotKeyPoint;
            CPoint canvasPt = GetCanvasPoint(memPt);
            Canvas.SetLeft(ShotKeyPoint_UI, canvasPt.X);
            Canvas.SetTop(ShotKeyPoint_UI, canvasPt.Y);

            if (!p_UIElement.Contains(ShotKeyPoint_UI))
            {
                p_UIElement.Add(ShotKeyPoint_UI);
            }
        }

        private void DrawShotCenterPoint()
        {
            Line line1 = ShotCenterPoint_UI.Children[0] as Line;
            line1.X1 = -10;
            line1.Y1 = 10;
            line1.X2 = 10;
            line1.Y2 = -10;
            line1.Stroke = DefineColors.ShotCenterPointColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = ShotCenterPoint_UI.Children[1] as Line;
            line2.X1 = 10;

            line2.Y1 = 10;
            line2.X2 = -10;
            line2.Y2 = -10;
            line2.Stroke = DefineColors.ShotCenterPointColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;
            //if (IsLoad)
            //{
                ShotCenterPoint = new CPoint((int)(centerPoint.X - ShotOffsetX / 4), (int)(centerPoint.Y + ShotOffsetY / 4));
            //}

            CPoint memPt = ShotCenterPoint;
            CPoint canvasPt = GetCanvasPoint(memPt);
            Canvas.SetLeft(ShotCenterPoint_UI, canvasPt.X);
            Canvas.SetTop(ShotCenterPoint_UI, canvasPt.Y);

            if (!p_UIElement.Contains(ShotCenterPoint_UI))
            {
                p_UIElement.Add(ShotCenterPoint_UI);
            }
        }

        private void DrawingSetPitchPoint()
        {

            PitchRightTop_UI.Width = 40;
            PitchRightTop_UI.Height = 40;

            PitchRightBottom_UI.Width = 40;
            PitchRightBottom_UI.Height = 40;


            this.pitchRightTop.X = (int)(originRightTop.X + ScribeLaneX / 4);
            this.pitchRightTop.Y = (int)(originRightTop.Y - ScribeLaneY / 4);

            this.pitchRightBottom.X = (int)(originRightTop.X + ScribeLaneX / 4);
            this.pitchRightBottom.Y = originLeftBottom.Y;

            CPoint canvasPt1 = GetCanvasPoint(pitchRightTop);

            Canvas.SetLeft(PitchRightTop_UI, canvasPt1.X);
            Canvas.SetTop(PitchRightTop_UI, canvasPt1.Y);

            CPoint canvasPt2 = GetCanvasPoint(pitchRightBottom);

            Canvas.SetLeft(PitchRightBottom_UI, canvasPt2.X);
            Canvas.SetTop(PitchRightBottom_UI, canvasPt2.Y);

            if (!p_UIElement.Contains(PitchRightTop_UI))
            {
                p_UIElement.Add(PitchRightTop_UI);
            }

            if (!p_UIElement.Contains(PitchRightBottom_UI))
            {
                p_UIElement.Add(PitchRightBottom_UI);
            }

            pitchBox.Left = originLeftBottom.X;
            pitchBox.Bottom = originLeftBottom.Y;
            pitchBox.Right = pitchRightBottom.X;
            pitchBox.Top = pitchRightTop.Y;
        }

        void CalcCenter()
        {
            double cx, cy,r;
            Get3PointCircle(CenterLTPoint, CenterRTPoint, CenterRBPoint,out cx, out cy, out r);
            centerPoint = new CPoint((int)cx, (int)cy);

            if (p_UIElement.Contains(CenterPoint_UI))
                p_UIElement.Remove(CenterPoint_UI);
            Line line1 = CenterPoint_UI.Children[0] as Line;
            line1.X1 = -10;
            line1.Y1 = 10;
            line1.X2 = 10;
            line1.Y2 = -10;
            line1.Stroke = DefineColors.CenteringPointColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = CenterPoint_UI.Children[1] as Line;
            line2.X1 = 10;
            line2.Y1 = 10;
            line2.X2 = -10;
            line2.Y2 = -10;
            line2.Stroke = DefineColors.CenteringPointColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;
            CPoint memPt = new CPoint((int)cx, (int)cy);
            CPoint canvasPt = GetCanvasPoint(new CPoint((int)cx, (int)(cy)));
            Canvas.SetLeft(CenterPoint_UI, canvasPt.X);
            Canvas.SetTop(CenterPoint_UI, canvasPt.Y);
            p_UIElement.Add(CenterPoint_UI);
            SetData();
        }
        #endregion

        #region [Viewer Method]
        public void DisplayFull()
        {
            this.p_Zoom = 1;
        }


        public void DisplayBox()
        {
            if (p_UIElement.Contains(OriginBox_UI))
            {
                int offsetX = this.pitchRightTop.X - this.originRightTop.X;
                int offsetY = this.originRightTop.Y - this.pitchRightTop.Y;

                int left = this.originLeftBottom.X - offsetX;
                int top = this.originRightTop.Y - offsetY;

                int right = this.originRightTop.X + offsetX;
                int bottom = this.originLeftBottom.Y + offsetY;

                int width = right - left;
                int height = bottom - top;

                double full_ratio = 1;
                double ratio = 1;

                if (this.p_CanvasHeight > this.p_CanvasWidth)
                {
                    full_ratio = full_ratio = (double)this.p_ImageData.p_Size.Y / (double)this.p_CanvasHeight;
                }
                else
                {
                    full_ratio = full_ratio = (double)this.p_ImageData.p_Size.X / (double)this.p_CanvasWidth;
                }


                double canvas_w_h_ratio = (double)(this.p_CanvasHeight) / (double)(p_CanvasWidth); // 가로가 더 길 경우 1 이하
                double box_w_h_ratio = (double)height / (double)width;

                if (box_w_h_ratio > canvas_w_h_ratio) // Canvas보다 가로 비율이 더 높을 경우,  box의 세로에 맞춰야함.
                {
                    ratio = (double)height / (double)this.p_CanvasHeight;
                }
                else
                {
                    ratio = (double)width / (double)this.p_CanvasWidth;
                }


                this.p_Zoom = ratio / full_ratio;

                this.p_View_Rect = new System.Drawing.Rectangle(new System.Drawing.Point(left, top), new System.Drawing.Size(width, height));

                this.SetRoiRect();

            }
            else
            {
                MessageBox.Show("Origin 영역이 설정되지 않았습니다");
            }
        }


        public void ClearAllObjects(bool isFromParent = false)
        {
            this.IsOriginEnable = false;

            this.IsPitchEnable = false;
            this.IsCenterChecked = false;
            this.IsShotKeyEnable = false;
            this.IsShotKeyChecked = false;
           this.p_UIElement.Clear();

            viewerState = SETUP_VIEWER_STATE.Center;
            this.centerState = PROCESS_CENTER_STATE.None;
            this.originState = PROCESS_ORIGIN_STATE.None;
            this.pitchState = PROCESS_PITCH_STATE.None;
            centerPos = CENTERING_POS.LT;

            p_Cursor = Cursors.Arrow;
            if (this.OriginBoxReset != null && isFromParent == false)
                this.OriginBoxReset();
        }

        /// <summary>
        /// 양방향에서 Origin을 Clear할 수 있으므로
        /// Parent에서 Origin Clear를 요청한 경우 OriginBoxReset을 발생 시키지 않는다.
        /// </summary>
        /// <param name="isFromParent"></param>
        /// 
        public void ClearObjects(bool isFromParent = false)
        {

            for(int i = 0; i < p_UIElement.Count; i++)
            {
                if(p_UIElement[i] is Grid && ((Grid)(p_UIElement[i])).Tag != null &&((Grid)(p_UIElement[i])).Tag.ToString().Contains("Centering"))
                {
  
                    continue;
                }
                p_UIElement.Remove(p_UIElement[i]);
                i = 0;
            }
            //this.p_UIElement.Clear();

            //this.alignState = PROCESS_ALIGN_STATE.None;
            if (IsOriginChecked)
            {
                //IsOriginEnable = false;
                this.IsPitchEnable = false;
                this.IsShotKeyEnable = false;
            }

            this.originState = PROCESS_ORIGIN_STATE.None;
            this.pitchState = PROCESS_PITCH_STATE.None;
            //centerPos = CENTERING_POS.LT;
            if (this.OriginBoxReset != null && isFromParent == false)
                this.OriginBoxReset();
        }

        public void SetCenter()
        {
            //if(!IsOriginEnable)
            //    IsOriginEnable = true;
        }

        public void SetOrigin()
        {
            if(!IsPitchEnable)
            this.IsPitchEnable = true;

            if (this.OriginBoxDone != null)
                this.OriginBoxDone();
        }

        public void SetPitch()
        {
            if (!IsShotKeyEnable)
                IsShotKeyEnable = true;

            if (this.PitchPointDone != null)
                this.PitchPointDone();
        }

        public void DataLoadDone()
        {
            ClearAllObjects();
            IsOriginEnable = true;
            IsPitchEnable = true;
            IsShotKeyEnable = true;
        }

        #endregion
    }
}
