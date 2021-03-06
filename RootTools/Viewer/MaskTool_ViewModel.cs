using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    public class MaskTool_ViewModel : RootViewer_ViewModel
    {
        public MaskTool_ViewModel(ImageData image = null, IDialogService dialogService = null)
        {
            base.init(image, dialogService);
            p_VisibleMenu = Visibility.Visible;
            Shapes.CollectionChanged += Shapes_CollectionChanged;
        }

        private void Shapes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var shapes = sender as ObservableCollection<TShape>;
            foreach (TShape shape in shapes)
            {
                if(!p_ROIElement.Contains(shape.UIElement))
                    p_ROIElement.Add(shape.UIElement);
            }
            TShape[] Work = new TShape[20];
            shapes.CopyTo(Work, 0);

            History.Push(Work);
        }

        private TShape tshape;
        private CPoint PointBuffer;
        private ToolProcess eToolProcess;
        private ModifyType eModifyType;
        private ToolType eToolType;
        private Stack<TShape[]> History = new Stack<TShape[]>();
        public ObservableCollection<TShape> Shapes = new ObservableCollection<TShape>();


        public ICommand Clear
        {
            get
            {
                return new RelayCommand(_clear);
            }
        }
        public void _clear()
        {
            p_Cursor = Cursors.Arrow;
            Shapes.Clear();
            p_ROIElement.Clear();
            foreach (TShape shape in Shapes)
            {
                shape.isSelected = false;
            }
            //SelectedShapes.Clear();
        }

        #region Property
        private ObservableCollection<UIElement> m_ROIElement = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_ROIElement
        {
            get
            {
                return m_ROIElement;
            }
            set
            {
                m_ROIElement = value;
            }
        }
        private ObservableCollection<UIElement> m_ModifyElement = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_ModifyElement
        {
            get
            {
                return m_ModifyElement;
            }
            set
            {
                m_ModifyElement = value;
            }
        }

        private string _test;
        public string test
        {
            get
            {
                return _test;
            }
            set
            {
                SetProperty(ref _test, value);
            }
        }
        private int m_SelectedToolIndex;
        public int p_SelectedToolIndex
        {
            get
            {
                return m_SelectedToolIndex;
            }
            set
            {
                if (value == -1)
                    value = 0;

                eToolType = (ToolType)value;
                SetProperty(ref m_SelectedToolIndex, value);
            }
        }
        #endregion

        #region Command
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            if (m_KeyEvent == null)
                return;
            if (m_KeyEvent.Key == Key.LeftCtrl && m_KeyEvent.IsDown)
                return;
            CPoint CanvasPt= new CPoint(p_MouseX, p_MouseY);
            CPoint MemPt = GetMemPoint(CanvasPt);
            switch (eToolProcess)
            {
                case ToolProcess.None:
                    if (eToolType == ToolType.None)
                    {
                        if (isOutsideAllShape(MemPt))
                        {
                            Debug.WriteLine("isOutSide");
                            foreach (TShape shape in Shapes)
                            {
                                shape.isSelected = false;
                            }
                        }
                        if (p_Cursor != Cursors.Arrow)
                        {
                            PointBuffer = MemPt;
                            string cursor = p_Cursor.ToString();
                            SetState(ToolProcess.Modifying);
                            Debug.WriteLine("Preview Mouse Down");
                            Debug.WriteLine("Set Modify: " + eModifyType);            
                        }
                    }
                    else
                    {
                        tshape = StartDraw(tshape, eToolType, MemPt);
                        Shapes.Add(tshape);
                        SetState(ToolProcess.Drawing);
                    }                    
                    break;
                case ToolProcess.Drawing:
                    tshape = DrawDone(tshape, eToolType);
                    SetState(ToolProcess.None);
                    break;
                case ToolProcess.Modifying:
                    break;
            }
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint MemPt = GetMemPoint(CanvasPt);
            switch (eToolProcess)
            {
                case ToolProcess.None:
                    break;
                case ToolProcess.Drawing:
                    tshape = Drawing(tshape, eToolType, MemPt);
                    break;
                case ToolProcess.Modifying:
                    {
                        if (e.LeftButton == MouseButtonState.Pressed)
                            ModifyRect(MemPt);
                    }
                    break;
            }
        }
        public override void PreviewMouseUp(object sender, MouseEventArgs e)
        {
            switch (eToolProcess)
            {
                case ToolProcess.None:
                    break;
                case ToolProcess.Drawing:
                    break;
                case ToolProcess.Modifying:
                    {
                        foreach (TShape shape in Shapes)
                        {
                            if (shape.isSelected)
                            {
                                MakeModifyTool(shape);
                                shape.ModifyTool.Visibility = Visibility.Visible;
                            }
                        }

                        SetState(ToolProcess.None);
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
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            base.CanvasMovePoint_Ref(point, nX, nY);
            RedrawShapes();
        }
        #endregion

        private void RedrawShapes()
        {
            foreach (TShape shape in Shapes)
            {
                TRect rect = shape as TRect;
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

                MakeModifyTool(shape);
                if (shape.isSelected)
                    shape.ModifyTool.Visibility = Visibility.Visible;
            }
        }
        private TShape StartDraw(TShape shape, ToolType toolType, CPoint memPt)
        {
            switch (toolType)
            {
                case ToolType.Point:
                    break;
                case ToolType.Line:
                    break;
                case ToolType.Rect:
                    shape = new TRect(Brushes.Yellow, 1, 0.5);
                    TRect rect = shape as TRect;
                    rect.MemPointBuffer = memPt;
                    rect.MemoryRect.Left = memPt.X;
                    rect.MemoryRect.Top = memPt.Y;
                    break;
                case ToolType.Circle:
                    break;
                case ToolType.Polygon:
                    break;
            }
            return shape;

        }
        private TShape Drawing(TShape shape, ToolType toolType, CPoint memPt)
        {
            switch (toolType)
            {
                case ToolType.Point:
                    break;
                case ToolType.Line:
                    break;
                case ToolType.Rect:
                    TRect rect = shape as TRect;
                    // memright가 0인상태로 canvas rect width가 정해져서 버그...
                    // 0이면 min정해줘야되나
                    if (rect.MemPointBuffer.X> memPt.X)
                    {
                        rect.MemoryRect.Left = memPt.X;
                    }
                    else
                    {
                        rect.MemoryRect.Left = rect.MemPointBuffer.X;
                        rect.MemoryRect.Right = memPt.X;
                    }
                    if (rect.MemPointBuffer.Y > memPt.Y)
                    {
                        rect.MemoryRect.Top = memPt.Y;
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

                    break;
                case ToolType.Circle:
                    break;
                case ToolType.Polygon:
                    break;
            }
            return shape;
        }
        private TShape DrawDone(TShape shape, ToolType toolType)
        {
            switch (toolType)
            {
                case ToolType.Point:
                    break;
                case ToolType.Line:
                    break;
                case ToolType.Rect:
                    TRect rect = shape as TRect;
                    //rect.CanvasRect.Fill = rect.FillBrush;
                    rect.CanvasRect.Tag = rect;
                    rect.CanvasRect.MouseEnter += CanvasRect_MouseEnter;
                    rect.CanvasRect.MouseLeave += CanvasRect_MouseLeave;
                    rect.CanvasRect.MouseLeftButtonDown += CanvasRect_MouseLeftButtonDown;
                    MakeModifyTool(rect);
                    p_SelectedToolIndex = 0;
                    break;
                case ToolType.Circle:
                    break;
                case ToolType.Polygon:
                    break;
            }
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
            Debug.WriteLine("Rect Mouse Left Donw : " + rect.UIElement.GetHashCode());
            if (rect.isSelected)
                rect.isSelected = false;
            else
                rect.isSelected = true;
            Debug.WriteLine("Selected: " + rect.isSelected);
        }
        
        private void MakeModifyTool(TShape shape)
        {
            if (p_ROIElement.Contains(shape.ModifyTool))
                p_ROIElement.Remove(shape.ModifyTool);

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
            p_ROIElement.Add(modifyTool);
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
        private void ModifyRect(CPoint memPt)
        {
            int offset_x = memPt.X - PointBuffer.X;
            int offset_y = memPt.Y - PointBuffer.Y;
            CPoint ptOffset = new CPoint(offset_x, offset_y);

            foreach (TShape shape in Shapes)
            {
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

                    Debug.WriteLine("Type :" + eModifyType);
                    Debug.WriteLine("Modifying :" + shape.UIElement.GetHashCode());
                }
                PointBuffer = memPt;
            }
        }

        private bool isOutsideAllShape(CPoint memPt)
        {
            foreach (TShape shape in Shapes)
            {
                TRect rect = shape as TRect;
                double left, top, right, bottom;

                left = rect.MemoryRect.Left -50;
                top = rect.MemoryRect.Top -50;
                right = rect.MemoryRect.Right +50;
                bottom = rect.MemoryRect.Bottom +50;
                if (left < memPt.X && memPt.X < right && top < memPt.Y && memPt.Y < bottom)
                {
                    return false;
                }       
            }
            return true;
        }
        public List<TShape> GetListTShape()
        {
            return Shapes.ToList();
        }
        public ToolProcess SetState(ToolProcess state)
        {
            eToolProcess = state;
            return state;
        }
        public enum ToolType
        {
            None,
            Point,
            Line,
            Rect,
            Circle,
            Polygon,
        }
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
        public enum ToolProcess
        {
            None,
            Drawing,
            Modifying,
        }
    }

}
