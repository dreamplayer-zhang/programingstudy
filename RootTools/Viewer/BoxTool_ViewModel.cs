using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RootTools
{
    public class BoxTool_ViewModel :RootViewer_ViewModel
    {
        public BoxTool_ViewModel(ImageData image = null, IDialogService dialogService = null)
        {
            base.init(image, dialogService);
            p_VisibleMenu = Visibility.Visible;
            p_VisibleTool = Visibility.Collapsed;
            
        }
        public event boxDone BoxDone;
        public delegate void boxDone(object e);
        TShape BOX;
        CPoint PointBuffer;
        public Grid Origin_UI;
        public Grid Pitch_UI;
        public TRect InspArea;

        BoxProcess eBoxProcess;
        ModifyType eModifyType;
        public enum ModifyType
        {
            None,
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
        private enum BoxProcess
        {
            None,
            Drawing,
            Modifying,
        }
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            if(m_KeyEvent !=null)
                if (m_KeyEvent.Key == Key.LeftCtrl && m_KeyEvent.IsDown)
                    return;
            CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint MemPt = GetMemPoint(CanvasPt);
            switch (eBoxProcess)
            {
                case BoxProcess.None:
                    //마우스가 rect밖에있으면 선택취소
                    // 그다음 다시그리기
                    // 안에있으면 선택
                    if (p_Cursor != Cursors.Arrow)
                    {
                        PointBuffer = MemPt;
                        string cursor = p_Cursor.ToString();
                        eBoxProcess = BoxProcess.Modifying;
                        break;
                    }
                    else
                    {
                        if (BOX != null)
                        {
                            if (p_ViewElement.Contains(BOX.UIElement))
                            {
                                p_ViewElement.Remove(BOX.UIElement);
                                p_ViewElement.Remove(BOX.ModifyTool);
                            }
                        }
                        BOX = StartDraw(BOX, MemPt);
                        p_ViewElement.Add(BOX.UIElement);
                        eBoxProcess = BoxProcess.Drawing;
                    }
                    break;
                case BoxProcess.Drawing:
                    BOX = DrawDone(BOX);
                    
                    BoxDone(BOX);
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
                        BoxDone(BOX);
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
            if (BOX != null)
            {
                BoxDone(BOX);
            }
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
            //Ctrl키로 클릭해야 복수선택되게
            //선택은 항상 한개만?
            //foreach(TShape shape in Shapes)
            //{
            //    int cnt = 0;
            //    if (shape.isSelected)
            //        cnt++;
            //}

            TRect rect = (sender as Rectangle).Tag as TRect;
            if (rect.isSelected)
                rect.isSelected = false;
            else
                rect.isSelected = true;
            
        }
        private void MakeModifyTool(TShape shape)
        {
            if (p_ViewElement.Contains(shape.ModifyTool))
                p_ViewElement.Remove(shape.ModifyTool);

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
            p_ViewElement.Add(modifyTool);
        }
        private void ModifyPoint_MouseEnter(object sender, MouseEventArgs e)
        {
            CPoint index = (sender as Ellipse).Tag as CPoint;

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
            int offset_x = memPt.X - PointBuffer.X;
            int offset_y = memPt.Y - PointBuffer.Y;
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
            PointBuffer = memPt;
            return shape;
        }
        private void RedrawShapes()
        {
            if (BOX == null)
                return;

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


            if (p_ViewElement.Contains(Origin_UI))
            {
                CPoint memPtOriginBOX = Origin_UI.Tag as CPoint;
                AddOriginPoint(memPtOriginBOX, Brushes.Red);
            }
            if (p_ViewElement.Contains(Pitch_UI))
            {
                CPoint memPtPitchBOX = Pitch_UI.Tag as CPoint;
                AddPitchPoint(memPtPitchBOX, Brushes.Green);
            }
            if (InspArea != null)
            {
                AddInspArea();
            }
        }
        public void AddOriginPoint(CPoint originMemPt, Brush color)
        {
            if (p_ViewElement.Contains(Origin_UI))
                p_ViewElement.Remove(Origin_UI);
            if (originMemPt.X == 0 && originMemPt.Y == 0)
                return;
            CPoint canvasPt = GetCanvasPoint(originMemPt);

            Origin_UI = new Grid();
            
            Origin_UI.Tag = originMemPt;
            Origin_UI.Width = 20;
            Origin_UI.Height = 20;
            Canvas.SetLeft(Origin_UI, canvasPt.X - 10);
            Canvas.SetTop(Origin_UI, canvasPt.Y - 10);
            Canvas.SetZIndex(Origin_UI, 99);
            Line line1 = new Line();
            line1.X1 = 0;
            line1.Y1 = 0;
            line1.X2 = 1;
            line1.Y2 = 1;
            line1.Stroke = color;
            line1.StrokeThickness = 2;
            line1.Stretch = Stretch.Fill;
            Line line2 = new Line();
            line2.X1 = 0;
            line2.Y1 = 1;
            line2.X2 = 1;
            line2.Y2 = 0;
            line2.Stroke = color;
            line2.StrokeThickness = 3;
            line2.Stretch = Stretch.Fill;

            Origin_UI.Children.Add(line1);
            Origin_UI.Children.Add(line2);
            p_ViewElement.Add(Origin_UI);
        }
        public void AddPitchPoint(CPoint originMemPt, Brush color)
        {
            if (p_ViewElement.Contains(Pitch_UI))
                p_ViewElement.Remove(Pitch_UI);
            if (originMemPt.X == 0 && originMemPt.Y == 0)
                return;
            CPoint canvasPt = GetCanvasPoint(originMemPt);
            Pitch_UI = new Grid();
            Pitch_UI.Tag = originMemPt;
            Pitch_UI.Width = 20;
            Pitch_UI.Height = 20;
            Canvas.SetLeft(Pitch_UI, canvasPt.X - 10);
            Canvas.SetTop(Pitch_UI, canvasPt.Y - 10);
            Canvas.SetZIndex(Pitch_UI, 99);
            Line line1 = new Line();
            line1.X1 = 0;
            line1.Y1 = 0;
            line1.X2 = 1;
            line1.Y2 = 1;
            line1.Stroke = color;
            line1.StrokeThickness = 2;
            line1.Stretch = Stretch.Fill;
            Line line2 = new Line();
            line2.X1 = 0;
            line2.Y1 = 1;
            line2.X2 = 1;
            line2.Y2 = 0;
            line2.Stroke = color;
            line2.StrokeThickness = 3;
            line2.Stretch = Stretch.Fill;

            Pitch_UI.Children.Add(line1);
            Pitch_UI.Children.Add(line2);
            p_ViewElement.Add(Pitch_UI);
        }
        public void AddInspArea(TRect rect = null)
        {
            if(InspArea != null)
                if (p_ViewElement.Contains(InspArea.CanvasRect))
                    p_ViewElement.Remove(InspArea.CanvasRect);
            if (rect != null)
            {
                InspArea = new TRect(rect.FillBrush, rect.CanvasRect.StrokeThickness, rect.CanvasRect.Opacity);
                InspArea.MemoryRect.Left = rect.MemoryRect.Left;
                InspArea.MemoryRect.Top= rect.MemoryRect.Top;
                InspArea.MemoryRect.Right = rect.MemoryRect.Right;
                InspArea.MemoryRect.Bottom = rect.MemoryRect.Bottom;
            }
            else
                rect = InspArea;
            CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
            CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);
            CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB));

            int width = Math.Abs(canvasRB.X - canvasLT.X);
            int height = Math.Abs(canvasRB.Y - canvasLT.Y);

            Canvas.SetLeft(InspArea.CanvasRect, canvasLT.X);
            Canvas.SetTop(InspArea.CanvasRect, canvasLT.Y);
            Canvas.SetRight(InspArea.CanvasRect, canvasRB.X);
            Canvas.SetBottom(InspArea.CanvasRect, canvasRB.Y);
            InspArea.CanvasRect.Width = width;
            InspArea.CanvasRect.Height = height;

            p_ViewElement.Add(InspArea.CanvasRect);
        }
        public void Clear()
        {
            if (p_ViewElement.Contains(Origin_UI))
                p_ViewElement.Remove(Origin_UI);
            if (p_ViewElement.Contains(Pitch_UI))
                p_ViewElement.Remove(Pitch_UI);
            if (p_ViewElement.Contains(InspArea.CanvasRect))
                p_ViewElement.Remove(InspArea.CanvasRect);
            BoxDone(BOX);
        }

    }
}
