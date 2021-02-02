using RootTools;
using RootTools_Vision;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Root_WIND2.UI_Temp
{

    public delegate void FeatureBoxDoneEvent(object e);

    class FrontsideAlignment_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        #region [Color]
        private class DefineColors
        {
            public static SolidColorBrush OriginBoxColor = Brushes.Blue;
        }
        #endregion

        public event FeatureBoxDoneEvent FeatureBoxDone;

        public FrontsideAlignment_ImageViewer_ViewModel()
        {
            p_VisibleMenu = System.Windows.Visibility.Collapsed;

            InitializeUIElements();
        }

        private enum BoxProcess
        {
            None,
            Drawing,
            Modifying,
        }

        BoxProcess eBoxProcess;
        ModifyType eModifyType;

        CPoint mousePointBuffer;

        Grid OriginBox;

        TShape BOX;


        private void InitializeUIElements()
        {
            BOX = new TShape();

            OriginBox = new Grid();
            OriginBox.Children.Add(new Line()); // Left
            OriginBox.Children.Add(new Line()); // Top
            OriginBox.Children.Add(new Line()); // Right
            OriginBox.Children.Add(new Line()); // Bottom

            p_ViewElement.Add(OriginBox);
        }

        public void SetViewRect()
        {
            DisplayBox();
        }


        #region [Command]
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

        #region [Viewer Method]
        public void DisplayFull()
        {
            this.p_Zoom = 1;

            this.SetImageSource();
            this.RedrawShapes();
        }

        public void DisplayBox()
        {
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            int offsetX = originRecipe.DiePitchX - originRecipe.OriginWidth;
            int offsetY = originRecipe.DiePitchY - originRecipe.OriginHeight;

            int left = originRecipe.OriginX - offsetX;
            int bottom = originRecipe.OriginY + offsetY;
            int right = originRecipe.OriginX + originRecipe.OriginWidth + offsetX;
            int top = originRecipe.OriginY - originRecipe.OriginHeight - offsetY;

            int width = originRecipe.OriginWidth + offsetX * 2;
            int height = originRecipe.OriginHeight + offsetY * 2;

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


            this.SetImageSource();
            this.RedrawShapes();

        }

        #endregion

        #region [Overrides]
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;
            CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint MemPt = GetMemPoint(CanvasPt);
            switch (eBoxProcess)
            {
                case BoxProcess.None:
                    if (p_Cursor != Cursors.Arrow)
                    {
                        mousePointBuffer = MemPt;
                        string cursor = p_Cursor.ToString();
                        eBoxProcess = BoxProcess.Modifying;
                        break;
                    }
                    else
                    {
                        if (p_UIElement.Contains(BOX.UIElement))
                        {
                            p_UIElement.Remove(BOX.UIElement);
                            p_UIElement.Remove(BOX.ModifyTool);
                        }


                        BOX = StartDraw(BOX, MemPt);
                        p_UIElement.Add(BOX.UIElement);
                        eBoxProcess = BoxProcess.Drawing;
                    }
                    break;
                case BoxProcess.Drawing:
                    BOX = DrawDone(BOX);
                    if (FeatureBoxDone != null)
                        FeatureBoxDone(BOX);

                    eBoxProcess = BoxProcess.None;
                    break;
                case BoxProcess.Modifying:
                    break;
            }
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint MemPt = GetMemPoint(CanvasPt);
            switch (eBoxProcess)
            {
                case BoxProcess.None:
                    break;
                case BoxProcess.Drawing:
                    BOX = Drawing(BOX, MemPt);
                    break;
                case BoxProcess.Modifying:
                    {
                        if (e.LeftButton == MouseButtonState.Pressed)
                            BOX = ModifyRect(BOX, MemPt);

                    }
                    break;
            }
        }
        public override void PreviewMouseUp(object sender, MouseEventArgs e)
        {
            switch (eBoxProcess)
            {
                case BoxProcess.None:
                    break;
                case BoxProcess.Drawing:
                    break;
                case BoxProcess.Modifying:
                    {
                        if (BOX.isSelected)
                        {
                            MakeModifyTool(BOX);
                            BOX.ModifyTool.Visibility = Visibility.Visible;
                        }

                        if(FeatureBoxDone != null)
                            FeatureBoxDone(BOX);
                        eBoxProcess = BoxProcess.None;
                        eModifyType = ModifyType.None;
                    }
                    break;
            }
        }
        public override void SetRoiRect()
        {
            base.SetRoiRect();
            RedrawShapes();
        }
        public override void _openImage()
        {
            base._openImage();

        }
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            base.CanvasMovePoint_Ref(point, nX, nY);
            RedrawShapes();
        }
        private TShape StartDraw(TShape shape, CPoint memPt)
        {
            shape = new TRect(Brushes.Blue, 2, 0.4);
            TRect rect = shape as TRect;
            rect.MemPointBuffer = memPt;
            rect.MemoryRect.Left = memPt.X;
            rect.MemoryRect.Top = memPt.Y;

            return shape;
        }
        private TShape Drawing(TShape shape, CPoint memPt)
        {
            TRect rect = shape as TRect;
            // memright가 0인상태로 canvas rect width가 정해져서 버그...
            // 0이면 min정해줘야되나
            if (rect.MemPointBuffer.X > memPt.X)
            {
                rect.MemoryRect.Left = memPt.X;
                rect.MemoryRect.Right = rect.MemPointBuffer.X;
            }
            else
            {
                rect.MemoryRect.Left = rect.MemPointBuffer.X;
                rect.MemoryRect.Right = memPt.X;
            }
            if (rect.MemPointBuffer.Y > memPt.Y)
            {
                rect.MemoryRect.Top = memPt.Y;
                rect.MemoryRect.Bottom = rect.MemPointBuffer.Y;
            }
            else
            {
                rect.MemoryRect.Top = rect.MemPointBuffer.Y;
                rect.MemoryRect.Bottom = memPt.Y;
            }

            CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
            CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);
            CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB));

            int width = Math.Abs(canvasRB.X - canvasLT.X);
            int height = Math.Abs(canvasRB.Y - canvasLT.Y);
            Canvas.SetLeft(rect.CanvasRect, canvasLT.X);
            Canvas.SetTop(rect.CanvasRect, canvasLT.Y);
            Canvas.SetRight(rect.CanvasRect, canvasRB.X);
            Canvas.SetBottom(rect.CanvasRect, canvasRB.Y);
            rect.CanvasRect.Width = width;
            rect.CanvasRect.Height = height;

            return shape;
        }
        private TShape DrawDone(TShape shape)
        {
            TRect rect = shape as TRect;
            rect.CanvasRect.Fill = rect.FillBrush;
            rect.CanvasRect.Tag = rect;
            rect.CanvasRect.MouseEnter += CanvasRect_MouseEnter;
            rect.CanvasRect.MouseLeave += CanvasRect_MouseLeave;
            rect.CanvasRect.MouseLeftButtonDown += CanvasRect_MouseLeftButtonDown;
            MakeModifyTool(rect);

            return shape;
        }
        private void CanvasRect_MouseEnter(object sender, MouseEventArgs e)
        {
            p_Cursor = Cursors.Hand;
        }
        private void CanvasRect_MouseLeave(object sender, MouseEventArgs e)
        {
            p_Cursor = Cursors.Arrow;
        }
        private void CanvasRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TRect rect = (sender as Rectangle).Tag as TRect;
            if (rect.isSelected)
                rect.isSelected = false;
            else
                rect.isSelected = true;

        }
        private void MakeModifyTool(TShape shape)
        {
            if (p_UIElement.Contains(shape.ModifyTool))
                p_UIElement.Remove(shape.ModifyTool);

            TRect rect = shape as TRect;

            double left, top;
            left = Canvas.GetLeft(rect.CanvasRect);
            top = Canvas.GetTop(rect.CanvasRect);

            Grid modifyTool = new Grid();
            Canvas.SetLeft(modifyTool, left - 5);
            Canvas.SetTop(modifyTool, top - 5);
            modifyTool.Visibility = Visibility.Collapsed;
            modifyTool.Width = rect.CanvasRect.Width + 10;
            modifyTool.Height = rect.CanvasRect.Height + 10;

            Border outline = new Border();
            outline.BorderBrush = Brushes.Gray;
            outline.Margin = new Thickness(4);
            outline.BorderThickness = new Thickness(1);
            modifyTool.Children.Add(outline);

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    System.Windows.Shapes.Ellipse modifyPoint = new System.Windows.Shapes.Ellipse();
                    modifyPoint.Tag = new CPoint(i, j);
                    modifyPoint.MouseEnter += ModifyPoint_MouseEnter;
                    modifyPoint.MouseLeave += ModifyPoint_MouseLeave;
                    modifyPoint.Width = 10;
                    modifyPoint.Height = 10;
                    modifyPoint.Stroke = Brushes.Gray;
                    modifyPoint.StrokeThickness = 2;
                    modifyPoint.Fill = Brushes.White;
                    if (i == 0)
                        modifyPoint.HorizontalAlignment = HorizontalAlignment.Left;
                    if (i == 1)
                        modifyPoint.HorizontalAlignment = HorizontalAlignment.Center;
                    if (i == 2)
                        modifyPoint.HorizontalAlignment = HorizontalAlignment.Right;
                    if (j == 0)
                        modifyPoint.VerticalAlignment = VerticalAlignment.Top;
                    if (j == 1)
                        modifyPoint.VerticalAlignment = VerticalAlignment.Center;
                    if (j == 2)
                        modifyPoint.VerticalAlignment = VerticalAlignment.Bottom;

                    modifyTool.Children.Add(modifyPoint);
                }
            rect.ModifyTool = modifyTool;
            p_UIElement.Add(modifyTool);
        }
        private void ModifyPoint_MouseEnter(object sender, MouseEventArgs e)
        {
            CPoint index = (sender as System.Windows.Shapes.Ellipse).Tag as CPoint;

            if (index.X == 0)
            {
                if (index.Y == 0)
                {
                    p_Cursor = Cursors.SizeNWSE;
                    eModifyType = ModifyType.LeftTop;
                }
                if (index.Y == 1)
                {
                    p_Cursor = Cursors.SizeWE;
                    eModifyType = ModifyType.Left;
                }
                if (index.Y == 2)
                {
                    p_Cursor = Cursors.SizeNESW;
                    eModifyType = ModifyType.LeftBottom;
                }
            }
            if (index.X == 1)
            {
                if (index.Y == 0)
                {
                    p_Cursor = Cursors.SizeNS;
                    eModifyType = ModifyType.Top;
                }
                if (index.Y == 1)
                {
                    p_Cursor = Cursors.ScrollAll;
                    eModifyType = ModifyType.ScrollAll;
                }
                if (index.Y == 2)
                {
                    p_Cursor = Cursors.SizeNS;
                    eModifyType = ModifyType.Bottom;
                }
            }
            if (index.X == 2)
            {
                if (index.Y == 0)
                {
                    p_Cursor = Cursors.SizeNESW;
                    eModifyType = ModifyType.RightTop;
                }
                if (index.Y == 1)
                {
                    p_Cursor = Cursors.SizeWE;
                    eModifyType = ModifyType.Right;
                }
                if (index.Y == 2)
                {
                    p_Cursor = Cursors.SizeNWSE;
                    eModifyType = ModifyType.RightBottom;
                }
            }

        }
        private void ModifyPoint_MouseLeave(object sender, MouseEventArgs e)
        {
            p_Cursor = Cursors.Arrow;
        }
        private TShape ModifyRect(TShape shape, CPoint memPt)
        {
            int offset_x = memPt.X - mousePointBuffer.X;
            int offset_y = memPt.Y - mousePointBuffer.Y;
            CPoint ptOffset = new CPoint(offset_x, offset_y);

            TRect rect = shape as TRect;
            if (rect.isSelected)
            {
                rect.ModifyTool.Visibility = Visibility.Collapsed;
                int left, top, right, bottom;
                left = rect.MemoryRect.Left;
                top = rect.MemoryRect.Top;
                right = rect.MemoryRect.Right;
                bottom = rect.MemoryRect.Bottom;

                switch (eModifyType)
                {
                    case ModifyType.ScrollAll:
                        p_Cursor = Cursors.ScrollAll;
                        left += ptOffset.X;
                        top += ptOffset.Y;
                        right += ptOffset.X;
                        bottom += ptOffset.Y;
                        break;
                    case ModifyType.Left:
                        left += ptOffset.X;
                        p_Cursor = Cursors.SizeWE;
                        break;
                    case ModifyType.Right:
                        right += ptOffset.X;
                        p_Cursor = Cursors.SizeWE;
                        break;
                    case ModifyType.Top:
                        top += ptOffset.Y;
                        p_Cursor = Cursors.SizeNS;
                        break;
                    case ModifyType.Bottom:
                        bottom += ptOffset.Y;
                        p_Cursor = Cursors.SizeNS;
                        break;
                    case ModifyType.LeftTop:
                        left += ptOffset.X;
                        top += ptOffset.Y;
                        p_Cursor = Cursors.SizeNWSE;
                        break;
                    case ModifyType.LeftBottom:
                        left += ptOffset.X;
                        bottom += ptOffset.Y;
                        p_Cursor = Cursors.SizeNESW;
                        break;
                    case ModifyType.RightTop:
                        right += ptOffset.X;
                        top += ptOffset.Y;
                        p_Cursor = Cursors.SizeNESW;
                        break;
                    case ModifyType.RightBottom:
                        right += ptOffset.X;
                        bottom += ptOffset.Y;
                        p_Cursor = Cursors.SizeNWSE;
                        break;
                }

                rect.MemoryRect.Left = left;
                rect.MemoryRect.Top = top;
                rect.MemoryRect.Right = right;
                rect.MemoryRect.Bottom = bottom;

                CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
                CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);

                CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
                CPoint canvasRB = new CPoint(GetCanvasPoint(RB));
                int width = Math.Abs(canvasRB.X - canvasLT.X);
                int height = Math.Abs(canvasRB.Y - canvasLT.Y);
                rect.CanvasRect.Width = width;
                rect.CanvasRect.Height = height;
                Canvas.SetLeft(rect.CanvasRect, canvasLT.X);
                Canvas.SetTop(rect.CanvasRect, canvasLT.Y);
                Canvas.SetRight(rect.CanvasRect, canvasRB.X);
                Canvas.SetBottom(rect.CanvasRect, canvasRB.Y);
            }
            mousePointBuffer = memPt;
            return shape;
        }


        #endregion


        #region [Draw Method]
        private void RedrawShapes()
        {
            if (p_UIElement.Contains(BOX.UIElement))
            {
                TRect rect = BOX as TRect;
                CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
                CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);

                CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
                CPoint canvasRB = new CPoint(GetCanvasPoint(RB));
                int width = Math.Abs(canvasRB.X - canvasLT.X);
                int height = Math.Abs(canvasRB.Y - canvasLT.Y);
                rect.CanvasRect.Width = width;
                rect.CanvasRect.Height = height;
                Canvas.SetLeft(rect.CanvasRect, canvasLT.X);
                Canvas.SetTop(rect.CanvasRect, canvasLT.Y);
                Canvas.SetRight(rect.CanvasRect, canvasRB.X);
                Canvas.SetBottom(rect.CanvasRect, canvasRB.Y);

                MakeModifyTool(BOX);
                if (BOX.isSelected)
                    BOX.ModifyTool.Visibility = Visibility.Visible;
            }

            RedrawOriginBox();
        }

        private void RedrawOriginBox()
        {

            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            if (originRecipe.OriginWidth == 0 || originRecipe.OriginHeight == 0) return;

            int left = originRecipe.OriginX;
            int top = originRecipe.OriginY - originRecipe.OriginHeight;

            int right = originRecipe.OriginX - originRecipe.OriginWidth;
            int bottom = originRecipe.OriginY;

            CPoint canvasLeftTop = GetCanvasPoint(new CPoint(left, top));
            CPoint canvasLeftBottom = GetCanvasPoint(new CPoint(left, bottom));
            CPoint canvasRightTop = GetCanvasPoint(new CPoint(right, top));
            CPoint canvasRightBottom = GetCanvasPoint(new CPoint(right, bottom));

            OriginBox.Width = Math.Abs(canvasRightTop.X - canvasLeftTop.X);
            OriginBox.Height = Math.Abs(canvasLeftBottom.Y - canvasLeftTop.Y);

            // Left
            Line leftLine = OriginBox.Children[0] as Line;
            leftLine.X1 = 0;
            leftLine.Y1 = 0;
            leftLine.X2 = 0;
            leftLine.Y2 = OriginBox.Height;
            leftLine.Stroke = DefineColors.OriginBoxColor;
            leftLine.StrokeThickness = 2;
            leftLine.Opacity = 1;

            // Top
            Line topLine = OriginBox.Children[1] as Line;
            topLine.X1 = 0;
            topLine.Y1 = 0;
            topLine.X2 = OriginBox.Width;
            topLine.Y2 = 0;
            topLine.Stroke = DefineColors.OriginBoxColor;
            topLine.StrokeThickness = 2;
            topLine.Opacity = 1;

            // Right
            Line rightLine = OriginBox.Children[2] as Line;
            rightLine.X1 = OriginBox.Width;
            rightLine.Y1 = 0;
            rightLine.X2 = OriginBox.Width;
            rightLine.Y2 = OriginBox.Height;
            rightLine.Stroke = DefineColors.OriginBoxColor;
            rightLine.StrokeThickness = 2;
            rightLine.Opacity = 1;

            // bottom
            Line bottomLine = OriginBox.Children[3] as Line;
            bottomLine.X1 = 0;
            bottomLine.Y1 = OriginBox.Height;
            bottomLine.X2 = OriginBox.Width;
            bottomLine.Y2 = OriginBox.Height;
            bottomLine.Stroke = DefineColors.OriginBoxColor;
            bottomLine.StrokeThickness = 2;
            bottomLine.Opacity = 1;

            Canvas.SetLeft(OriginBox, canvasLeftTop.X);
            Canvas.SetTop(OriginBox, canvasLeftTop.Y);

            if (!p_ViewElement.Contains(OriginBox))
            {
                p_ViewElement.Add(OriginBox);
            }
        }

        public void FeatureBoxClear()
        {
            if (p_UIElement.Contains(BOX.UIElement))
            {
                p_UIElement.Remove(BOX.UIElement);
                p_UIElement.Remove(BOX.ModifyTool);
            }

            BOX = new TShape();

            RedrawShapes();
        }


        #endregion

    }
}
