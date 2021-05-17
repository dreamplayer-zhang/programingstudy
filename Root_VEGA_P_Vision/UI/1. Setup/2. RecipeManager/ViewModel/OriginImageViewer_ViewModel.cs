using RootTools;
using RootTools_Vision;
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

namespace Root_VEGA_P_Vision
{
    public delegate void EventOriginBoxDone();
    public delegate void EventOriginPointDone();
    public delegate void EventOriginBoxReset();
    public class OriginImageViewer_ViewModel : RootViewer_ViewModel
    {
        #region [Color]

        public class DefineColors
        {
            public static SolidColorBrush OriginColor = Brushes.Blue;
            public static SolidColorBrush OriginBoxColor = Brushes.Blue;
        }

        #endregion
        #region [Event]
        public event EventOriginPointDone OriginPointDone;
        public event EventOriginBoxDone OriginBoxDone;
        public event EventOriginBoxReset OriginBoxReset;
        #endregion
        #region [Properties]
        private bool isOriginChecked = false;
        public bool IsOriginChecked
        {
            get => isOriginChecked;
            set
            {
                if (!value)
                {
                    if (!p_UIElement.Contains(OriginBox_UI))
                    {
                        ClearObjects();
                    }
                    else
                    {
                        originLeftBottom.X = originBox.Left;
                        originLeftBottom.Y = originBox.Bottom;
                        RedrawShapes();
                    }
                }

                SetProperty(ref isOriginChecked, value);
            }
        }
        #endregion
        #region [ViewerState]
        EUVOriginRecipe originRecipe;

        public OriginImageViewer_ViewModel()
        {
            p_VisibleMenu = System.Windows.Visibility.Collapsed;
            RecipeVision recipe = GlobalObjects.Instance.Get<RecipeVision>();
            originRecipe = recipe.GetItem<EUVOriginRecipe>();
            InitializeUIElement();
        }

        #endregion
        #region [Draw Method]

        public void SetOriginBox(CPoint originPt, int originWidth, int originHeight)
        {
            originLeftBottom.X = originPt.X;
            originLeftBottom.Y = originPt.Y;

            originRightTop.X = originPt.X + originWidth;
            originRightTop.Y = originPt.Y - originHeight;

            originBox.Left = originLeftBottom.X;
            originBox.Right = originRightTop.X;
            originBox.Top = originRightTop.Y;
            originBox.Bottom = originLeftBottom.Y;

            DrawOriginLeftBottomPoint(originLeftBottom, true);
            DrawOriginRightTopPoint(originRightTop, true);

            DrawOriginBox();
        }
        public void SetOriginBox(CPoint originPt, CPoint originSize)
        {
            originLeftBottom.X = originPt.X;
            originLeftBottom.Y = originPt.Y;

            originRightTop.X = originPt.X + originSize.X;
            originRightTop.Y = originPt.Y + originSize.Y;

            originBox.Left = originLeftBottom.X;
            originBox.Right = originRightTop.X;
            originBox.Top = originRightTop.Y;
            originBox.Bottom = originLeftBottom.Y;

            DrawOriginLeftBottomPoint(originLeftBottom, true);
            DrawOriginRightTopPoint(originRightTop, true);

            DrawOriginBox();
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
            line1.Stroke = Brushes.Red;
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

            if (bRecipeLoaded == false)
            {
                // Recipe
                //originRecipe.Clear();                
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

            if (bRecipeLoaded == false)
            {
                // Recipe

            }
        }

        private void DrawOriginBox()
        {
            int left = originBox.Left;
            int top = originBox.Top;

            int right = originBox.Right;
            int bottom = originBox.Bottom;

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
            if (p_UIElement.Contains(OriginBox_UI))
            {
                DrawOriginBox();
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
            string memid = p_ImageData.m_MemData.p_id;

            switch (originState)
            {
                case PROCESS_ORIGIN_STATE.None:
                    originState = PROCESS_ORIGIN_STATE.OriginLeftBottom;
                    ProcessOrigin(e);
                    break;
                case PROCESS_ORIGIN_STATE.OriginLeftBottom:
                    ClearObjects();

                    p_Cursor = Cursors.Arrow;
                    originLeftBottom = memPt;
                    DrawOriginLeftBottomPoint(originLeftBottom);

                    if (memid.Contains("Main"))
                        originRecipe.TDIOrigin.Origin = memPt;
                    else if (memid.Contains("Stain"))
                        originRecipe.StainOrigin.Origin = memPt;
                    else if (memid.Contains("Top"))
                        originRecipe.SideTBOrigin.Origin = memPt;
                    else if (memid.Contains("Left"))
                        originRecipe.SideLROrigin.Origin = memPt;

                    if (OriginPointDone != null)
                        OriginPointDone();

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

                    if (originBox.Left > originBox.Right)
                    {
                        int tmp = originBox.Right;
                        originBox.Right = originBox.Left;
                        originBox.Left = tmp;
                    }
                    if(originBox.Top>originBox.Bottom)
                    {
                        int tmp = originBox.Bottom;
                        originBox.Bottom = originBox.Top;
                        originBox.Top = tmp;
                    }
                    int w = originBox.Right - originBox.Left;
                    int h = originBox.Top + originBox.Bottom;

                    if (memid.Contains("Main"))
                        originRecipe.TDIOrigin.OriginSize = new CPoint(w,h);
                    else if (memid.Contains("Stain"))
                        originRecipe.StainOrigin.OriginSize = new CPoint(w,h);
                    else if (memid.Contains("Top"))
                        originRecipe.SideTBOrigin.OriginSize = new CPoint(w,h);
                    else if (memid.Contains("Left"))
                        originRecipe.SideLROrigin.OriginSize = new CPoint(w,h);

                    DrawOriginRightTopPoint(originRightTop);
                    DrawOriginBox();
                    SetOrigin();

                    originState = PROCESS_ORIGIN_STATE.None;
                    break;
            }
        }
        #endregion

        #region [Viewer Method]
        /// <summary>
        /// 양방향에서 Origin을 Clear할 수 있으므로
        /// Parent에서 Origin Clear를 요청한 경우 OriginBoxReset을 발생 시키지 않는다.
        /// </summary>
        /// <param name="isFromParent"></param>
        public void ClearObjects(bool isFromParent = false)
        {
            p_UIElement.Clear();
            originState = PROCESS_ORIGIN_STATE.None;

            if (OriginBoxReset != null && isFromParent == false)
                OriginBoxReset();
        }

        public void SetOrigin()
        {
            if (OriginBoxDone != null)
                OriginBoxDone();
        }

        #endregion

        #region [Draw 관련 멤버]

        Grid OriginLeftBottom_UI = null;
        Grid OriginRightTop_UI = null;
        Grid OriginBox_UI = null;

        CPoint originLeftBottom = new CPoint();
        CPoint originRightTop = new CPoint();
        CRect originBox = new CRect();

        public void InitializeUIElement()
        {
            OriginLeftBottom_UI = new Grid();
            OriginLeftBottom_UI.Children.Add(new Line());
            OriginLeftBottom_UI.Children.Add(new Line());

            OriginRightTop_UI = new Grid();
            OriginRightTop_UI.Children.Add(new Line());
            OriginRightTop_UI.Children.Add(new Line());

            OriginBox_UI = new Grid();
            OriginBox_UI.Children.Add(new Line()); // Left
            OriginBox_UI.Children.Add(new Line()); // Top
            OriginBox_UI.Children.Add(new Line()); // Right
            OriginBox_UI.Children.Add(new Line()); // Bottom

        }
        #endregion

        #region [Mouse Event Overrides]
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);

            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;

            ProcessOrigin(e);

        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);


            if (originState == PROCESS_ORIGIN_STATE.OriginLeftBottom)
            {
                originLeftBottom.X = p_MouseMemX;
                originLeftBottom.Y = p_MouseMemY;
                DrawOriginLeftBottomPoint(originLeftBottom);
            }
            else if (originState == PROCESS_ORIGIN_STATE.OriginRightTop)
            {
                originRightTop.X = p_MouseMemX;
                originRightTop.Y = p_MouseMemY;
                DrawOriginRightTopPoint(originRightTop);
            }


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
    }
}
