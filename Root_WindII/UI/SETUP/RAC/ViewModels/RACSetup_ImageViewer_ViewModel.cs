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
            Origin,
            Pitch,
            Rular,
        }

        public delegate void EventViewerStateChagned();
        public delegate void EventOriginBoxDone();
        public delegate void EventOriginPointDone();
        public delegate void EventOriginBoxReset();
        public delegate void EventPitchPointDone();

        #region [Color]

        class DefineColors
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

        public RACSetup_ImageViewer_ViewModel()
        {
            p_VisibleMenu = System.Windows.Visibility.Collapsed;

            this.ViewerState = SETUP_VIEWER_STATE.Normal;
            this.ViewerStateChanged += ViewerStateChanged_Callback;

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
                        RedrawShapes();
                    }
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


        CPoint originLeftBottom = new CPoint();
        CPoint originRightTop = new CPoint();
        CRect originBox = new CRect();

        CPoint pitchRightTop = new CPoint();
        CPoint pitchRightBottom = new CPoint();
        CRect pitchBox = new CRect();

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
                case SETUP_VIEWER_STATE.Origin:
                    ProcessOrigin(e);
                    break;
                case SETUP_VIEWER_STATE.Pitch:
                    ProcessPitch(e);
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

                    this.pitchBox.Left = this.originLeftBottom.X - offsetX;
                    this.pitchBox.Right = this.originRightTop.X + offsetX;
                    this.pitchBox.Top = this.originRightTop.Y - offsetY;
                    this.pitchBox.Bottom = this.originLeftBottom.Y + offsetY;

                    DrawPitchBox();

                    if (this.PitchPointDone != null)
                        this.PitchPointDone();

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
            this.pitchRightBottom.Y = this.originLeftBottom.Y + offsetY;

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


        /// <summary>
        /// 양방향에서 Origin을 Clear할 수 있으므로
        /// Parent에서 Origin Clear를 요청한 경우 OriginBoxReset을 발생 시키지 않는다.
        /// </summary>
        /// <param name="isFromParent"></param>
        public void ClearObjects(bool isFromParent = false)
        {
            this.IsPitchEnable = false;
            this.p_UIElement.Clear();

            this.originState = PROCESS_ORIGIN_STATE.None;
            this.pitchState = PROCESS_PITCH_STATE.None;

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
