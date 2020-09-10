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
    public class ROI_ViewModel : RootViewer_ViewModel
    {
        public ROI_ViewModel(ImageData image = null, IDialogService dialogService = null)
        {
            base.init(image, dialogService);
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

        private FrameworkElement Clone(FrameworkElement e)
        {
            System.Xml.XmlDocument document = new System.Xml.XmlDocument();
            document.LoadXml(System.Windows.Markup.XamlWriter.Save(e));

            return (FrameworkElement)System.Windows.Markup.XamlReader.Load(new System.Xml.XmlNodeReader(document));
        }
        private TShape tshape;
        private CPoint PointBuffer;
        private ToolProcess eToolProcess;
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
        private Cursor m_Cursor = Cursors.Arrow;
        public Cursor p_Cursor
        {
            get         
            {
                if (m_Cursor == null)
                    test = "null";
                else
                    test = m_Cursor.ToString();
                return m_Cursor;
            }
            set
            {
                test = value.ToString();
                SetProperty(ref m_Cursor, value);
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
            CPoint CanvasPt= new CPoint(p_MouseX, p_MouseY);
            CPoint MemPt = GetMemPoint(CanvasPt);
            switch (eToolProcess)
            {
                case ToolProcess.None:
                    if (eToolType == ToolType.None)
                    {
                        if (isOutsideAllShape(MemPt))
                        {
                            foreach (TShape shape in Shapes)
                            {
                                shape.isSelected = false;
                            }
                        }
                        string cursor = p_Cursor.ToString();
                        switch (cursor)
                        {
                            case "ScrollAll":
                                PointBuffer = MemPt;
                                SetState(ToolProcess.Modify);
                                break;
                        }
                        //커서를보고 뭐할건지결정
                    }
                    else
                    {
                        tshape = StartDraw(tshape, eToolType, MemPt);
                        Shapes.Add(tshape);
                        SetState(ToolProcess.Drawing);
                    }
                    
                    break;
                case ToolProcess.Start:
                    break;
                case ToolProcess.Drawing:
                    tshape = DrawDone(tshape, eToolType, CanvasPt);
                    //Done
                    break;
                case ToolProcess.Done:
                    break;
                case ToolProcess.Modify:
                    break;
            }
        }
        public void PreviewMouseUp(object sender, MouseEventArgs e)
        {
            switch (eToolProcess)
            {
                case ToolProcess.None:
                    break;
                case ToolProcess.Start:
                    break;
                case ToolProcess.Drawing:
                    break;
                case ToolProcess.Done:
                    break;
                case ToolProcess.Modify:
                    {
                        foreach (TShape shape in Shapes)
                        {
                            if (shape.isSelected)
                            {
                                MakeModifyTool(shape);
                            }
                        }

                        SetState(ToolProcess.None);
                    }
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
                case ToolProcess.Start:
                    break;
                case ToolProcess.Drawing:
                    tshape = Drawing(tshape, eToolType, MemPt);
                    break;
                case ToolProcess.Done:
                    break;
                case ToolProcess.Modify:
                    {
                        if (e.LeftButton == MouseButtonState.Pressed)
                        {
                            p_Cursor = Cursors.ScrollAll;
                            ModifyRect(MemPt);                           
                        }
                    }
                    break;
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
                    shape = new TRect(Brushes.Yellow, 1);
                    TRect rect = shape as TRect;
                    rect.MemPointBuffer = memPt;
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
                    if (rect.MemPointBuffer.X> memPt.X)
                    {
                        rect.MemoryRect.Right = rect.MemoryRect.Left;
                        rect.MemoryRect.Left = memPt.X;
                    }
                    else
                    {
                        rect.MemoryRect.Left = rect.MemPointBuffer.X;
                        rect.MemoryRect.Right = memPt.X;
                    }
                    if (rect.MemPointBuffer.Y > memPt.Y)
                    {
                        rect.MemoryRect.Bottom = rect.MemoryRect.Top;
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
        private TShape DrawDone(TShape shape, ToolType toolType, CPoint nowPt)
        {
            switch (toolType)
            {
                case ToolType.Point:
                    break;
                case ToolType.Line:
                    break;
                case ToolType.Rect:
                    TRect rect = shape as TRect;
                    rect.CanvasRect.Fill = rect.FillBrush;
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
            SetState(ToolProcess.None);
            return shape;
        }

        #endregion

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
            Debug.WriteLine("Rect Mouse Left Donw : " + rect.GetHashCode());
            if (rect.isSelected)
                rect.isSelected = false;
            else
                rect.isSelected = true;
        }
        private bool isOutsideAllShape(CPoint memPt)
        {
            foreach (TShape shape in Shapes)
            {
                TRect rect = shape as TRect;
                double left, top, right, bottom;

                left = rect.MemoryRect.Left;
                top = rect.MemoryRect.Top;
                right = rect.MemoryRect.Right;
                bottom = rect.MemoryRect.Bottom;
                if (left < memPt.X && memPt.X < right && top < memPt.Y && memPt.Y < bottom)
                {
                    return false;
                }       
            }
            return true;
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
            tshape.ModifyTool = modifyTool;
            p_ROIElement.Add(modifyTool);
        }
        private void ModifyPoint_MouseEnter(object sender, MouseEventArgs e)
        {
            CPoint index = (sender as Ellipse).Tag as CPoint;

            if (index.X == 0)
            {
                if (index.Y == 0)
                    p_Cursor = Cursors.SizeNWSE;
                if (index.Y == 1)
                    p_Cursor = Cursors.SizeWE;
                if (index.Y == 2)
                    p_Cursor = Cursors.SizeNESW;
            }
            if (index.X == 1)
            {
                if (index.Y == 0)
                    p_Cursor = Cursors.SizeNS;
                if (index.Y == 1)
                    p_Cursor = Cursors.ScrollAll;
                if (index.Y == 2)
                    p_Cursor = Cursors.SizeNS;
            }
            if (index.X == 2)
            {
                if (index.Y == 0)
                    p_Cursor = Cursors.SizeNESW;
                if (index.Y == 1)
                    p_Cursor = Cursors.SizeWE;
                if (index.Y == 2)
                    p_Cursor = Cursors.SizeNWSE;
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

            if (true)
            {
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

                        left += ptOffset.X;
                        top += ptOffset.Y;
                        right += ptOffset.X;
                        bottom += ptOffset.Y;

                        rect.MemoryRect.Left = left;
                        rect.MemoryRect.Top = top;
                        rect.MemoryRect.Right = right;
                        rect.MemoryRect.Top = top;
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
                        
                        Debug.WriteLine("Modifying :" + shape.UIElement.GetHashCode());

                    }
                }
                PointBuffer = memPt;
            }
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
        public enum ToolProcess
        {
            None,
            Start,
            Drawing,
            Modify,
            Done,
        }
    }

}
