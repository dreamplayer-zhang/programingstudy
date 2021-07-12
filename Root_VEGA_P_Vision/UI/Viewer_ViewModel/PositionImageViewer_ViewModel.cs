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
    public delegate void FeatureBoxDoneEvent(object e);
    public delegate void ManualAlignDoneEvent(CPoint Top, CPoint Btm);
    public class PositionImageViewer_ViewModel: BaseViewer_ViewModel
    {
        #region [Color]
        private class DefineColors
        {
            public static SolidColorBrush OriginBoxColor = new SolidColorBrush(Color.FromRgb(0, 122,255));
            public static SolidColorBrush ManualAlignColor = new SolidColorBrush(Color.FromRgb(88,86,214));
        }
        #endregion
        public event FeatureBoxDoneEvent FeatureBoxDone;
        public event ManualAlignDoneEvent ManualAlignDone;

        public RecipeBase recipe;
        public PositionImageViewer_ViewModel(string imageData,RecipeBase recipe):base(imageData)
        {
            this.recipe = recipe;
            p_VisibleMenu = Visibility.Collapsed;
            p_ROILayer = null;
            InitializeUIElements();
        }
        private enum BoxProcess
        {
            None,
            Drawing,
            Modifying,
        }
        public enum ModifyType
        {
            None,
            LineStart,
            LineEnd,
            ScrollAll,
            Left,
            Right,
            Top,
            Bottom,
            LeftTop,
            RightTop,
            LeftBottom,
            RightBottom,
        }
        public enum AlignProcess
        {
            Top,Bottom,None
        }
        AlignProcess eAlignProcess;
        BoxProcess eBoxProcess;
        ModifyType eModifyType;
        public AlignBtnState btnState;
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

            AlignTop = new Grid();
            AlignTop.Children.Add(new Line());
            AlignTop.Children.Add(new Line());

            AlignBottom = new Grid();
            AlignBottom.Children.Add(new Line());
            AlignBottom.Children.Add(new Line());

            AlignLine = new Grid();
            AlignLine.Children.Add(new Line());
        }

        CPoint AlignTopPt, AlignBtmPt;
        Grid AlignTop, AlignBottom, AlignLine;
        private void DrawAlignTopPoint(CPoint memPt, bool bRecipeLoaded = false)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            AlignTop.Width = 40;
            AlignTop.Height = 40;

            Line line1 = AlignTop.Children[0] as Line;
            line1.X1 = -20;
            line1.Y1 = -20;
            line1.X2 = 20;
            line1.Y2 = 20;
            line1.Stroke = DefineColors.ManualAlignColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = AlignTop.Children[1] as Line;
            line2.X1 = 20;
            line2.Y1 = -20;
            line2.X2 = -20;
            line2.Y2 = 20;
            line2.Stroke = DefineColors.ManualAlignColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            Canvas.SetLeft(AlignTop, canvasPt.X);
            Canvas.SetTop(AlignTop, canvasPt.Y);

            if (!p_UIElement.Contains(AlignTop))
            {
                p_UIElement.Add(AlignTop);
            }
        }
        private void DrawAlignResults(bool bRecipeLoaded = false)
        {
            CPoint canvasLTPt = GetCanvasPoint(AlignTopPt);
            CPoint canvasRBPt = GetCanvasPoint(AlignBtmPt);

            Line line1 = AlignLine.Children[0] as Line;
            line1.X1 = canvasLTPt.X;
            line1.Y1 = canvasLTPt.Y;
            line1.X2 = canvasRBPt.X;
            line1.Y2 = canvasRBPt.Y;
            line1.Stroke = DefineColors.ManualAlignColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            if (!p_UIElement.Contains(AlignLine))
            {
                p_UIElement.Add(AlignLine);
            }

            if (bRecipeLoaded == false)
            {
                // Recipe

            }
        }
        private void DrawAlignBottomPoint(CPoint memPt, bool bRecipeLoaded = false)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            AlignBottom.Width = 40;
            AlignBottom.Height = 40;

            Line line1 = AlignBottom.Children[0] as Line;
            line1.X1 = -20;
            line1.Y1 = -20;
            line1.X2 = 20;
            line1.Y2 = 20;
            line1.Stroke = DefineColors.ManualAlignColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = AlignBottom.Children[1] as Line;
            line2.X1 = 20;
            line2.Y1 = -20;
            line2.X2 = -20;
            line2.Y2 = 20;
            line2.Stroke = DefineColors.ManualAlignColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            Canvas.SetLeft(AlignBottom, canvasPt.X);
            Canvas.SetTop(AlignBottom, canvasPt.Y);

            if (!p_UIElement.Contains(AlignBottom))
            {
                p_UIElement.Add(AlignBottom);
            }

            if (bRecipeLoaded == false)
            {
                // Recipe

            }
        }
        #region [Overrides]
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;
            if(btnState == AlignBtnState.ManualAlign)
                ProcessDrawLine(e);
            else
                ProcessDrawBox(e);
        }

        public void ProcessDrawLine(MouseEventArgs e)
        {
            CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint MemPt = GetMemPoint(CanvasPt);
            switch(eAlignProcess)
            {
                case AlignProcess.None:
                    eAlignProcess = AlignProcess.Top;
                    ProcessDrawLine(e);
                    break;

                case AlignProcess.Top:
                    ClearObjects();
                    AlignTopPt = MemPt;
                    DrawAlignTopPoint(AlignTopPt);
                    eAlignProcess = AlignProcess.Bottom;
                    break;

                case AlignProcess.Bottom:
                    AlignBtmPt = MemPt;
                    DrawAlignBottomPoint(AlignBtmPt);
                    DrawAlignResults();

                    if (ManualAlignDone != null)
                        ManualAlignDone(AlignTopPt, AlignBtmPt);
                    eAlignProcess = AlignProcess.None;
                    break;
            }
        }
        public void ProcessDrawBox(MouseEventArgs e)
        {
            CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint MemPt = GetMemPoint(CanvasPt);
            switch (eBoxProcess)
            {
                case BoxProcess.None:
                    if (p_Cursor != Cursors.Arrow)
                    {
                        mousePointBuffer = MemPt;
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

                        if (FeatureBoxDone != null)
                            FeatureBoxDone(BOX);
                        eBoxProcess = BoxProcess.None;
                        eModifyType = ModifyType.None;
                    }
                    break;
            }

            CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint MemPt = GetMemPoint(CanvasPt);
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
            shape = new TRect(DefineColors.OriginBoxColor, 2, 0.4);
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
                    Ellipse modifyPoint = new Ellipse();
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
            if (BOX == null) return;
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

            if(p_UIElement.Contains(AlignBottom))
                DrawAlignBottomPoint(AlignBtmPt);
            if(p_UIElement.Contains(AlignTop))
                DrawAlignTopPoint(AlignTopPt);
            if (p_UIElement.Contains(AlignLine))
                DrawAlignResults();

        }

        public void RedrawManualAlignLine()
        {

        }
        public void RedrawOriginBox()
        {
            EUVOriginRecipe originRecipe = recipe.GetItem<EUVOriginRecipe>();
            OriginInfo originInfo = originRecipe.TDIOriginInfo;

            if (TabName.Contains("Stain"))
                originInfo = originRecipe.StainOriginInfo;
            else if (TabName.Contains("Main"))
                originInfo = originRecipe.TDIOriginInfo;
            else if (TabName.Contains("Top"))
                originInfo = originRecipe.SideTBOriginInfo;
            else if (TabName.Contains("Left"))
                originInfo = originRecipe.SideLROriginInfo;

            int originWidth = originInfo.OriginSize.X;
            int originHeight = originInfo.OriginSize.Y;

            if (originWidth == 0 || originHeight == 0) return;

            int left = originInfo.Origin.X;
            int top = originInfo.Origin.Y;

            int right = originInfo.Origin.X + originWidth;
            int bottom = originInfo.Origin.Y+originHeight;

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
