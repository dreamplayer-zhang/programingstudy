using RootTools;
using RootTools_Vision;
using System;
using System.Runtime.InteropServices;
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
    public class OriginImageViewer_ViewModel : BaseViewer_ViewModel
    {
        #region [Color]

        public class DefineColors
        {
            public static SolidColorBrush OriginColor = new SolidColorBrush(Color.FromRgb(0,122,255));
            public static SolidColorBrush OriginBoxColor = new SolidColorBrush(Color.FromRgb(0, 122,255));
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
                        originLeftTop.X = originBox.Left;
                        originLeftTop.Y = originBox.Top;
                        RedrawShapes();
                    }
                }

                SetProperty(ref isOriginChecked, value);
            }
        }
        #endregion
        #region [ViewerState]
        EUVOriginRecipe originRecipe;
        OriginInfo originInfo;
        RecipeBase recipe;
        public OriginImageViewer_ViewModel(string imageData):base(imageData)
        {
            p_VisibleMenu = Visibility.Collapsed;
            p_ROILayer = null;
            recipe = GlobalObjects.Instance.Get<RecipeCoverFront>();
            originRecipe = recipe.GetItem<EUVOriginRecipe>();
            p_ROILayer = null;

            if (imageData.Contains("Main"))
            {
                originInfo = originRecipe.TDIOriginInfo;
                GlobalObjects.Instance.Get<RecipeCoverBack>().GetItem<EUVOriginRecipe>().TDIOriginInfo = originInfo;
                GlobalObjects.Instance.Get<RecipePlateFront>().GetItem<EUVOriginRecipe>().TDIOriginInfo = originInfo;
                GlobalObjects.Instance.Get<RecipePlateBack>().GetItem<EUVOriginRecipe>().TDIOriginInfo = originInfo;
            }
            else if (imageData.Contains("Stain"))
            {
                originInfo = originRecipe.StainOriginInfo;
                GlobalObjects.Instance.Get<RecipeCoverBack>().GetItem<EUVOriginRecipe>().StainOriginInfo = originInfo;
                GlobalObjects.Instance.Get<RecipePlateFront>().GetItem<EUVOriginRecipe>().StainOriginInfo = originInfo;
                GlobalObjects.Instance.Get<RecipePlateBack>().GetItem<EUVOriginRecipe>().StainOriginInfo = originInfo;
            }
            else if (imageData.Contains("Top"))
            {
                originInfo = originRecipe.SideTBOriginInfo;
                GlobalObjects.Instance.Get<RecipeCoverBack>().GetItem<EUVOriginRecipe>().SideTBOriginInfo = originInfo;
                GlobalObjects.Instance.Get<RecipePlateFront>().GetItem<EUVOriginRecipe>().SideTBOriginInfo = originInfo;
                GlobalObjects.Instance.Get<RecipePlateBack>().GetItem<EUVOriginRecipe>().SideTBOriginInfo = originInfo;
            }
            else if (imageData.Contains("Left"))
            {
                originInfo = originRecipe.SideLROriginInfo;
                GlobalObjects.Instance.Get<RecipeCoverBack>().GetItem<EUVOriginRecipe>().SideLROriginInfo = originInfo;
                GlobalObjects.Instance.Get<RecipePlateFront>().GetItem<EUVOriginRecipe>().SideLROriginInfo = originInfo;
                GlobalObjects.Instance.Get<RecipePlateBack>().GetItem<EUVOriginRecipe>().SideLROriginInfo = originInfo;
            }


            InitializeUIElement();
        }

        #endregion
        #region [Draw Method]

        public void SetOriginBox(CPoint originPt, CPoint originSize)
        {
            originLeftTop.X = originPt.X;
            originLeftTop.Y = originPt.Y;

            originRightBottom.X = originPt.X + originSize.X;
            originRightBottom.Y = originPt.Y + originSize.Y;

            originBox.Left = originLeftTop.X;
            originBox.Right = originRightBottom.X;
            originBox.Top = originLeftTop.Y;
            originBox.Bottom = originRightBottom.Y;

            DrawOriginLeftTopPoint(originLeftTop, true);
            DrawOriginRightBottomPoint(originRightBottom, true);

            DrawOriginBox();
        }


        private void DrawOriginLeftTopPoint(CPoint memPt, bool bRecipeLoaded = false)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            OriginLeftTop_UI.Width = 40;
            OriginLeftTop_UI.Height = 40;

            Line line1 = OriginLeftTop_UI.Children[0] as Line;
            line1.X1 = 0;
            line1.Y1 = 40;
            line1.X2 = 0;
            line1.Y2 = -10;
            line1.Stroke = DefineColors.OriginBoxColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = OriginLeftTop_UI.Children[1] as Line;
            line2.X1 = -10;
            line2.Y1 = 0;
            line2.X2 = 40;
            line2.Y2 = 0;
            line2.Stroke = DefineColors.OriginColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            Canvas.SetLeft(OriginLeftTop_UI, canvasPt.X);
            Canvas.SetTop(OriginLeftTop_UI, canvasPt.Y);

            if (!p_UIElement.Contains(OriginLeftTop_UI))
            {
                p_UIElement.Add(OriginLeftTop_UI);
            }
        }

        private void DrawOriginRightBottomPoint(CPoint memPt, bool bRecipeLoaded = false)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            OriginRightBottom_UI.Width = 40;
            OriginRightBottom_UI.Height = 40;


            Line line1 = OriginRightBottom_UI.Children[0] as Line;
            line1.X1 = 0;
            line1.Y1 = 10;
            line1.X2 = 0;
            line1.Y2 = -40;
            line1.Stroke = DefineColors.OriginColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = OriginRightBottom_UI.Children[1] as Line;
            line2.X1 = -40;
            line2.Y1 = 0;
            line2.X2 = 10;
            line2.Y2 = 0;
            line2.Stroke = DefineColors.OriginColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            Canvas.SetLeft(OriginRightBottom_UI, canvasPt.X);
            Canvas.SetTop(OriginRightBottom_UI, canvasPt.Y);

            if (!p_UIElement.Contains(OriginRightBottom_UI))
            {
                p_UIElement.Add(OriginRightBottom_UI);
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
            OriginBox_UI.Height = Math.Abs(canvasRightTop.Y - canvasRightBottom.Y);

            // Left
            Line leftLine = OriginBox_UI.Children[0] as Line;
            leftLine.X1 = 0;
            leftLine.Y1 = 0;
            leftLine.X2 = 0;
            leftLine.Y2 = OriginBox_UI.Height /*- offset*/;
            leftLine.Stroke = DefineColors.OriginBoxColor;
            leftLine.StrokeThickness = 1;
            leftLine.Opacity = 0.75;
            leftLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Top
            Line topLine = OriginBox_UI.Children[1] as Line;
            topLine.X1 = 0;
            topLine.Y1 = 0;
            topLine.X2 = OriginBox_UI.Width /*- offset*/;
            topLine.Y2 = 0;
            topLine.Stroke = DefineColors.OriginBoxColor;
            topLine.StrokeThickness = 1;
            topLine.Opacity = 0.75;
            topLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Right
            Line rightLine = OriginBox_UI.Children[2] as Line;
            rightLine.X1 = OriginBox_UI.Width;
            rightLine.Y1 = 0/*offset*/;
            rightLine.X2 = OriginBox_UI.Width;
            rightLine.Y2 = OriginBox_UI.Height;
            rightLine.Stroke = DefineColors.OriginBoxColor;
            rightLine.StrokeThickness = 1;
            rightLine.Opacity = 0.75;
            rightLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // bottom
            Line bottomLine = OriginBox_UI.Children[3] as Line;
            bottomLine.X1 = 0/*offset*/;
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
            if (p_UIElement.Contains(OriginLeftTop_UI))
            {
                DrawOriginLeftTopPoint(originLeftTop);
            }
            if (p_UIElement.Contains(OriginRightBottom_UI))
            {
                DrawOriginRightBottomPoint(originRightBottom);
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
            OriginLeftTop,
            OriginRightBottom,
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
                    originState = PROCESS_ORIGIN_STATE.OriginLeftTop;
                    ProcessOrigin(e);
                    break;
                case PROCESS_ORIGIN_STATE.OriginLeftTop:
                    ClearObjects();

                    p_Cursor = Cursors.Arrow;
                    originLeftTop = memPt;
                    DrawOriginLeftTopPoint(originLeftTop);

                    originInfo.Origin = memPt;

                    if (OriginPointDone != null)
                        OriginPointDone();

                    originState = PROCESS_ORIGIN_STATE.OriginRightBottom;
                    break;
                case PROCESS_ORIGIN_STATE.OriginRightBottom:
                    p_Cursor = Cursors.Arrow;

                    originRightBottom.X = memPt.X;
                    originRightBottom.Y = memPt.Y;

                    originBox.Left = originLeftTop.X;
                    originBox.Right = originRightBottom.X;
                    originBox.Top = originLeftTop.Y;
                    originBox.Bottom = originRightBottom.Y;

                    int w = originBox.Right - originBox.Left;
                    int h = originBox.Bottom - originBox.Top;

                    originInfo.OriginSize = new CPoint(w, h);

                    DrawOriginRightBottomPoint(originRightBottom);
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

        Grid OriginLeftTop_UI = null;
        Grid OriginRightBottom_UI = null;
        Grid OriginBox_UI = null;

        CPoint originLeftTop = new CPoint();
        CPoint originRightBottom = new CPoint();
        CRect originBox = new CRect();

        public void InitializeUIElement()
        {
            OriginLeftTop_UI = new Grid();
            OriginLeftTop_UI.Children.Add(new Line());
            OriginLeftTop_UI.Children.Add(new Line());

            OriginRightBottom_UI = new Grid();
            OriginRightBottom_UI.Children.Add(new Line());
            OriginRightBottom_UI.Children.Add(new Line());

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


            if (originState == PROCESS_ORIGIN_STATE.OriginLeftTop)
            {
                originLeftTop.X = p_MouseMemX;
                originLeftTop.Y = p_MouseMemY;
                DrawOriginLeftTopPoint(originLeftTop);
            }
            else if (originState == PROCESS_ORIGIN_STATE.OriginRightBottom)
            {
                originRightBottom.X = p_MouseMemX;
                originRightBottom.Y = p_MouseMemY;
                DrawOriginRightBottomPoint(originRightBottom);
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

        #region MasterImage
        public CPoint Offset = new CPoint();
        public void _saveMasterImage()
        {
            int posX = Offset.X;
            int posY = Offset.Y;
            int width = p_ImageData.p_Size.X;
            int height = p_ImageData.p_Size.Y;
            int byteCount = 1;
            byte[] rawdata = p_ImageData.GetByteArray();

            string memid = p_ImageData.m_MemData.p_id;

            originRecipe.SaveMasterImage(/*recipe.RecipeFolderPath*/@"C:\Recipe\",memid.Split('.')[1] + "_MasterImage.bmp", 
                posX, posY, width, height, byteCount, rawdata,originInfo);
        }
        public void _loadMasterImage()
        {
            string memid = p_ImageData.m_MemData.p_id;
            RecipeType_ImageData imgData = originRecipe.TDIOriginInfo.MasterImage;

            originRecipe.LoadMasterImage(recipe.RecipeFolderPath, memid.Split('.')[1] + "_MasterImage.bmp", originInfo);
            imgData = originInfo.MasterImage;

            ImageData BoxImageData = new ImageData(imgData.Width, imgData.Height, imgData.ByteCnt);

            BoxImageData.m_eMode = ImageData.eMode.ImageBuffer;
            BoxImageData.SetData(Marshal.UnsafeAddrOfPinnedArrayElement(imgData.RawData, 0)
                , new CRect(0, 0, imgData.Width, imgData.Height)
                , imgData.Width, imgData.ByteCnt);

            Offset = new CPoint(imgData.PositionX, imgData.PositionY);
            p_ImageData = BoxImageData;

            SetRoiRect();
        }
        #endregion
    }
}
