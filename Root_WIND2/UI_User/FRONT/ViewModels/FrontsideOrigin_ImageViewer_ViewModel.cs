using RootTools;
using RootTools_Vision;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Root_WIND2.UI_User
{
    public enum FRONT_ORIGIN_VIEWER_STATE
    {
        Normal,
        Origin,
        Pitch,
        Rular,
    }


    public delegate void EventViewerStateChagned();
    public delegate void EventOriginBoxDone();
    public delegate void EventOriginPointDone();
    public delegate void EventOriginBoxReset();
    public delegate void EventPitchPointDone();

    class FrontsideOrigin_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        #region [Color]

        public class DefineColors
        {
            public static SolidColorBrush OriginColor = Brushes.Blue;
            public static SolidColorBrush OriginBoxColor = Brushes.Blue;

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
        #endregion

        #region [ViewerState]

        public FrontsideOrigin_ImageViewer_ViewModel()
        {
            p_VisibleMenu = System.Windows.Visibility.Collapsed;

            this.ViewerState = FRONT_ORIGIN_VIEWER_STATE.Normal;
            this.ViewerStateChanged += ViewerStateChanged_Callback;

            InitializeUIElement();
        }

        private FRONT_ORIGIN_VIEWER_STATE viewerState = FRONT_ORIGIN_VIEWER_STATE.Normal;
        public FRONT_ORIGIN_VIEWER_STATE ViewerState
        {
            get => this.viewerState;
            set
            {
                switch (value)
                {
                    case FRONT_ORIGIN_VIEWER_STATE.Normal:
                        break;
                    case FRONT_ORIGIN_VIEWER_STATE.Origin:
                        originState = PROCESS_ORIGIN_STATE.OriginLeftBottom;
                        break;
                    case FRONT_ORIGIN_VIEWER_STATE.Pitch:
                        pitchState = PROCESS_PITCH_STATE.PitchRightTop;
                        break;
                    case FRONT_ORIGIN_VIEWER_STATE.Rular:
                        break;
                }

                SetProperty<FRONT_ORIGIN_VIEWER_STATE>(ref this.viewerState, value);

                if (ViewerStateChanged != null)
                    this.ViewerStateChanged();
            }
        }

        public void ViewerStateChanged_Callback()
        {
            this.DisplayViewerState = this.ViewerState.ToString();
            if (this.ViewerState == FRONT_ORIGIN_VIEWER_STATE.Normal)
            {
                this.IsOriginChecked = false;
                this.IsPitchChecked = false;
                this.IsRularChecked = false;
            }
        }

        #endregion

        #region [Properties]
        private bool isOriginChecked = false;
        public bool IsOriginChecked
        {
            get => this.isOriginChecked;
            set
            {
                if (value == true)
                {
                    this.IsPitchChecked = false;
                    this.IsRularChecked = false;
                }
                SetProperty<bool>(ref this.isOriginChecked, value);
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
                    this.IsRularChecked = false;
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

        #region [Command]
        public RelayCommand btnOriginCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (this.IsOriginChecked == true)
                    {
                        this.ViewerState = FRONT_ORIGIN_VIEWER_STATE.Origin;
                        this.DisplayViewerState = this.ViewerState.ToString();
                    }
                    else
                    {
                        this.ViewerState = FRONT_ORIGIN_VIEWER_STATE.Normal;
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
                        this.ViewerState = FRONT_ORIGIN_VIEWER_STATE.Pitch;
                        this.DisplayViewerState = this.ViewerState.ToString();
                    }
                    else
                    {
                        this.ViewerState = FRONT_ORIGIN_VIEWER_STATE.Normal;
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
                        this.ViewerState = FRONT_ORIGIN_VIEWER_STATE.Rular;
                        this.DisplayViewerState = this.ViewerState.ToString();
                    }
                    else
                    {
                        this.ViewerState = FRONT_ORIGIN_VIEWER_STATE.Normal;
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

        TRect InspArea;
        
        Grid OriginLeftBottom_UI = null;
        Grid OriginRightTop_UI = null;
        Grid OriginBox_UI = null;

        Grid PitchRightTop_UI = null;
        Grid PitchBox_UI = null;


        CPoint originLeftBottom = new CPoint();
        CPoint originRightTop = new CPoint();
        CPoint pitchRightTop = new CPoint();


        public void InitializeUIElement()
        {
            OriginLeftBottom_UI = new Grid();
            OriginLeftBottom_UI.Children.Add(new Line());
            OriginLeftBottom_UI.Children.Add(new Line());

            OriginRightTop_UI = new Grid();
            OriginRightTop_UI.Children.Add(new Line());
            OriginRightTop_UI.Children.Add(new Line());

            PitchRightTop_UI = new Grid();
            PitchRightTop_UI.Children.Add(new Line());
            PitchRightTop_UI.Children.Add(new Line());


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
                case FRONT_ORIGIN_VIEWER_STATE.Normal:
                    ProcessNormal(e);
                    break;
                case FRONT_ORIGIN_VIEWER_STATE.Origin:
                    ProcessOrigin(e);
                    break;
                case FRONT_ORIGIN_VIEWER_STATE.Pitch:
                    ProcessPitch(e);
                    break;
                case FRONT_ORIGIN_VIEWER_STATE.Rular:

                    break;
            }
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);

        }
        public override void PreviewMouseUp(object sender, MouseEventArgs e)
        {

        }
        public override void SetRoiRect()
        {
            base.SetRoiRect();
            RedrawShapes();
        }
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            base.CanvasMovePoint_Ref(point, nX, nY);
            RedrawShapes();
        }

        public override void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.MouseWheel(sender, e);

            RedrawShapes();
        }

        #endregion


        #region [Process Normal]
        public void ProcessNormal(MouseEventArgs e)
        {

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

            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            switch (originState)
            {
                case PROCESS_ORIGIN_STATE.None:
                    break;
                case PROCESS_ORIGIN_STATE.OriginLeftBottom:

                    // Origin 
                    ClearOrigin();

                    p_Cursor = Cursors.Arrow;
                    originLeftBottom = memPt;
                    DrawOriginLeftBottomPoint(originLeftBottom);

                    if (this.OriginPointDone != null)
                        this.OriginPointDone();

                    originState = PROCESS_ORIGIN_STATE.OriginRightTop;
                    break;
                case PROCESS_ORIGIN_STATE.OriginRightTop:

                    p_Cursor = Cursors.Arrow;

                    if(originLeftBottom.X > memPt.X || originLeftBottom.Y < memPt.Y)
                    {
                        MessageBox.Show("원점(Origin) 위치보다 오른쪽(또는 위쪽)에 위치해야합니다.");
                        return;
                    }

                    if( (originRightTop.X - originLeftBottom.X) > 30000 || (originLeftBottom.Y - originRightTop.Y) > 30000)
                    {
                        MessageBox.Show("Origin(혹은 검사) 영역의 크기는 높이 30000(혹은 너비 30000)을 넘을 수 없습니다.");
                        return;
                    }
                    
                    originRightTop = memPt;
                    pitchRightTop = memPt;

                    DrawOriginRightTopPoint(originRightTop);
                    DrawPitchPoint(pitchRightTop);
                    DrawOriginBox();

                    SetOrigin();

                    originState = PROCESS_ORIGIN_STATE.None;
                    ViewerState = FRONT_ORIGIN_VIEWER_STATE.Normal;
                    break;
            }
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

            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            switch (pitchState)
            {
                case PROCESS_PITCH_STATE.None:
                    break;
                case PROCESS_PITCH_STATE.PitchRightTop:
                    p_Cursor = Cursors.Arrow;

                    if (originRightTop.X > memPt.X || originRightTop.Y < memPt.Y)
                    {
                        MessageBox.Show("칩영역보다 오른쪽(또는 위쪽)에 위치해야합니다.");
                        return;
                    }

                    pitchRightTop = memPt;

                    DrawPitchPoint(pitchRightTop);
                    DrawPitchBox();

                    if (this.PitchPointDone != null)
                        this.PitchPointDone();

                    ViewerState = FRONT_ORIGIN_VIEWER_STATE.Normal;
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

        private void DrawOriginLeftBottomPoint(CPoint memPt)
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

            // Recipe
            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();

            originRecipe.Clear();

            originRecipe.OriginX = memPt.X;
            originRecipe.OriginY = memPt.Y;
        }

        private void DrawOriginRightTopPoint(CPoint memPt)
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

            // Recipe
            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();

            originRecipe.OriginWidth = memPt.X - originRecipe.OriginX;
            originRecipe.OriginHeight = originRecipe.OriginY - memPt.Y;

            originRecipe.DiePitchX = memPt.X - originRecipe.OriginX;
            originRecipe.DiePitchY = originRecipe.OriginY - memPt.Y;
        }

        private void DrawPitchPoint(CPoint memPt)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            PitchRightTop_UI.Width = 40;
            PitchRightTop_UI.Height = 40;

            Line line1 = PitchRightTop_UI.Children[0] as Line;
            line1.X1 = 0;
            line1.Y1 = -10;
            line1.X2 = 0;
            line1.Y2 = 40;
            line1.Stroke = DefineColors.PitchColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = PitchRightTop_UI.Children[1] as Line;
            line2.X1 = -40;
            line2.Y1 = 0;
            line2.X2 = 10;
            line2.Y2 = 0;
            line2.Stroke = DefineColors.PitchColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            Canvas.SetLeft(PitchRightTop_UI, canvasPt.X);
            Canvas.SetTop(PitchRightTop_UI, canvasPt.Y);

            if (!p_UIElement.Contains(PitchRightTop_UI))
            {
                p_UIElement.Add(PitchRightTop_UI);
            }

            // Recipe
            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();

            originRecipe.DiePitchX = memPt.X - originRecipe.OriginX;
            originRecipe.DiePitchY = originRecipe.OriginY - memPt.Y;
        }

        private void DrawOriginBox()
        {
            int left = this.originLeftBottom.X;
            int top = this.originRightTop.Y;

            int right = this.originRightTop.X;
            int bottom = this.originLeftBottom.Y;
            
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
            int offsetX = this.pitchRightTop.X - this.originRightTop.X;
            int offsetY = this.originRightTop.Y - this.pitchRightTop.Y;

            int left = this.originLeftBottom.X - offsetX;
            int top = this.originRightTop.Y - offsetY;

            int right = this.originRightTop.X + offsetX;
            int bottom = this.originLeftBottom.Y + offsetY;

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
            leftLine.Opacity = 0.75;
            leftLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Top
            Line topLine = PitchBox_UI.Children[1] as Line;
            topLine.X1 = 0;
            topLine.Y1 = 0;
            topLine.X2 = PitchBox_UI.Width;
            topLine.Y2 = 0;
            topLine.Stroke = DefineColors.PitchBoxColor;
            topLine.StrokeThickness = 1;
            topLine.Opacity = 0.75;
            topLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Right
            Line rightLine = PitchBox_UI.Children[2] as Line;
            rightLine.X1 = PitchBox_UI.Width;
            rightLine.Y1 = 0;
            rightLine.X2 = PitchBox_UI.Width;
            rightLine.Y2 = PitchBox_UI.Height;
            rightLine.Stroke = DefineColors.PitchBoxColor;
            rightLine.StrokeThickness = 1;
            rightLine.Opacity = 0.75;
            rightLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // bottom
            Line bottomLine = PitchBox_UI.Children[3] as Line;
            bottomLine.X1 = 0;
            bottomLine.Y1 = PitchBox_UI.Height;
            bottomLine.X2 = PitchBox_UI.Width;
            bottomLine.Y2 = PitchBox_UI.Height;
            bottomLine.Stroke = DefineColors.PitchBoxColor;
            bottomLine.StrokeThickness = 1;
            bottomLine.Opacity = 0.75;
            bottomLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            Canvas.SetLeft(PitchBox_UI, canvasLeftTop.X);
            Canvas.SetTop(PitchBox_UI, canvasLeftTop.Y);

            if (!p_UIElement.Contains(PitchBox_UI))
            {
                p_UIElement.Add(PitchBox_UI);
            }
        }

        private void RedrawShapes()
        {
            if (p_UIElement.Contains(OriginLeftBottom_UI))
            {
                DrawOriginLeftBottomPoint(originLeftBottom);
            }
            if(p_UIElement.Contains(OriginRightTop_UI))
            {
                DrawOriginRightTopPoint(originRightTop);
            }
            if (p_UIElement.Contains(PitchRightTop_UI))
            {
                DrawPitchPoint(pitchRightTop);
            }

            if(p_UIElement.Contains(OriginBox_UI))
            {
                DrawOriginBox();
            }

            if(p_UIElement.Contains(PitchBox_UI))
            {
                DrawPitchBox();
            }
        }


        private void DrawOriginArea(CPoint padding)
        {
            if (InspArea == null)
            {
                InspArea = new TRect(Brushes.Yellow, 2, 0.5);
            }
            int left = originLeftBottom.X - padding.X;
            int bottom = originLeftBottom.Y + padding.Y;
            int right = pitchRightTop.X + padding.X;
            int top = pitchRightTop.Y - padding.Y;

            //InspArea의 크기가 변할때만 Delegate Call

            if (InspArea.MemoryRect.Left != left || InspArea.MemoryRect.Right != right ||
                InspArea.MemoryRect.Top != top || InspArea.MemoryRect.Bottom != bottom)
            {
                InspArea.MemoryRect.Left = left;
                InspArea.MemoryRect.Bottom = bottom;
                InspArea.MemoryRect.Right = right;
                InspArea.MemoryRect.Top = top;
            }

            InspArea.MemoryRect.Left = left;
            InspArea.MemoryRect.Bottom = bottom;
            InspArea.MemoryRect.Right = right;
            InspArea.MemoryRect.Top = top;


            CPoint viewOriginPt = originLeftBottom;
            CPoint viewPitchPt = pitchRightTop;

            if (p_View_Rect.Width == 0) return;
            if (p_View_Rect.Height == 0) return;
            double pixSizeX = p_CanvasWidth / p_View_Rect.Width;
            double pixSizeY = p_CanvasHeight / p_View_Rect.Height;

            CPoint LT = new CPoint(viewOriginPt.X - padding.X, viewPitchPt.Y - padding.Y);
            CPoint RB = new CPoint(viewPitchPt.X + padding.X, viewOriginPt.Y + padding.Y);

            CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB));

            Canvas.SetLeft(InspArea.CanvasRect, canvasLT.X - pixSizeX / 2);
            Canvas.SetTop(InspArea.CanvasRect, canvasLT.Y - pixSizeY / 2);

            InspArea.CanvasRect.Width = Math.Abs(canvasRB.X - canvasLT.X + pixSizeX);
            InspArea.CanvasRect.Height = Math.Abs(canvasRB.Y - canvasLT.Y + pixSizeY);

            if (p_UIElement.Contains(InspArea.CanvasRect))
            {
                p_UIElement.Remove(InspArea.CanvasRect);
            }
            p_UIElement.Add(InspArea.CanvasRect);

        }

        #endregion

        #region [Viewer Method]
        public void DisplayFull()
        {
            this.p_Zoom = 1;
        }

        public void DisplayBox()
        {
            if(p_UIElement.Contains(OriginBox_UI))
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


        /// <summary>
        /// 양방향에서 Origin을 Clear할 수 있으므로
        /// Parent에서 Origin Clear를 요청한 경우 OriginBoxReset을 발생 시키지 않는다.
        /// </summary>
        /// <param name="isFromParent"></param>
        public void ClearOrigin(bool isFromParent = false)
        {
            this.IsPitchEnable = false;
            this.p_UIElement.Clear();

            if (this.OriginBoxReset != null && isFromParent == false)
                this.OriginBoxReset();
        }

        public void SetOrigin()
        {
            this.IsPitchEnable = true;

            if (this.OriginBoxDone != null)
                this.OriginBoxDone();
        }

        #endregion

    }
}
