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
        private ToolProcess eToolProgress;
        private ToolType eToolType;
        private Stack<TShape[]> History = new Stack<TShape[]>();
        //private List<TShape> SelectedShapes = new List<TShape>();
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
            CPoint nowPt= new CPoint(p_MouseX, p_MouseY);

            switch (eToolProgress)
            {
                case ToolProcess.None:
                    if (eToolType == ToolType.None)
                    {
                        if (isOutsideAllShape(nowPt))
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
                                PointBuffer = nowPt;
                                SetState(ToolProcess.Modify);
                                break;
                        }
                        //커서를보고 뭐할건지결정
                    }
                    else
                    {
                        tshape = StartDraw(tshape, eToolType, nowPt);
                        Shapes.Add(tshape);
                        SetState(ToolProcess.Drawing);
                    }
                    
                    break;
                case ToolProcess.Start:
                    break;
                case ToolProcess.Drawing:
                    tshape = DrawDone(tshape, eToolType, nowPt);
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
            foreach (TShape shape in Shapes)
            {
                if (shape.isSelected)
                {
                    MakeModifyTool(shape);
                    shape.isSelected = true;
                }
            }
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {           
            base.MouseMove(sender, e);
            CPoint nowPt = new CPoint(p_MouseX, p_MouseY);
            switch (eToolProgress)
            {
                case ToolProcess.None:
                    break;
                case ToolProcess.Start:
                    break;
                case ToolProcess.Drawing:
                    tshape = Drawing(tshape, eToolType, nowPt);
                    break;
                case ToolProcess.Done:
                    break;
                case ToolProcess.Modify:
                    {
                        if (e.LeftButton == MouseButtonState.Pressed)
                        {
                            p_Cursor = Cursors.ScrollAll;
                            
                            Debug.WriteLine("MouseMove in Modify");
                            ModifyRect(nowPt);                           
                        }
                        else
                        {
                            SetState(ToolProcess.None);
                        }
                    }
                    break;
            }
        }

        private TShape StartDraw(TShape shape, ToolType toolType, CPoint nowPt)
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
                    rect.StartPointBuffer = nowPt;
                    break;
                case ToolType.Circle:
                    break;
                case ToolType.Polygon:
                    break;
            }
            return shape;

        }
        private TShape Drawing(TShape shape, ToolType toolType, CPoint nowPt)
        {
            switch (toolType)
            {
                case ToolType.Point:
                    break;
                case ToolType.Line:
                    break;
                case ToolType.Rect:
                    TRect rect = shape as TRect;                   
                    double left, top, right, bottom;
                    if (rect.StartPointBuffer.X > nowPt.X)
                    {
                        Canvas.SetLeft(rect.CanvasRect, nowPt.X);
                        Canvas.SetRight(rect.CanvasRect, rect.StartPointBuffer.X);
                    }
                    else
                    {
                        Canvas.SetLeft(rect.CanvasRect, rect.StartPointBuffer.X);
                        Canvas.SetRight(rect.CanvasRect, nowPt.X);
                    }

                    if (rect.StartPointBuffer.Y > nowPt.Y)
                    {
                        Canvas.SetTop(rect.CanvasRect, nowPt.Y);
                        Canvas.SetBottom(rect.CanvasRect, rect.StartPointBuffer.Y);
                    }
                    else
                    {
                        Canvas.SetTop(rect.CanvasRect, rect.StartPointBuffer.Y);
                        Canvas.SetBottom(rect.CanvasRect, nowPt.Y);
                    }

                     left = Canvas.GetLeft(rect.CanvasRect);
                     top = Canvas.GetTop(rect.CanvasRect);
                     right = Canvas.GetRight(rect.CanvasRect);
                     bottom = Canvas.GetBottom(rect.CanvasRect);

                    rect.CanvasRect.Width = Math.Abs(right - left);
                    rect.CanvasRect.Height = Math.Abs(bottom - top);
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

                    double left, top, right, bottom;
                    left = Canvas.GetLeft(rect.CanvasRect);
                    top = Canvas.GetTop(rect.CanvasRect);
                    right = Canvas.GetRight(rect.CanvasRect);
                    bottom = Canvas.GetBottom(rect.CanvasRect);

                    CPoint LeftTop = new CPoint((int)left, (int)top);
                    CPoint RightBottom = new CPoint((int)right, (int)bottom);

                    rect.MemoryRect.Left = GetMemPoint(LeftTop).X;
                    rect.MemoryRect.Top = GetMemPoint(LeftTop).Y;
                    rect.MemoryRect.Right = GetMemPoint(RightBottom).X;
                    rect.MemoryRect.Bottom = GetMemPoint(RightBottom).Y;

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
            if (rect.isSelected)
                CancleSelect(rect);
            else
                Select(rect);
        }
        private void Select(TShape tshape)
        {
            TShape shape = Shapes.Where(s => s == tshape).FirstOrDefault();
            Debug.WriteLine("Select :"+shape.GetHashCode());
            shape.isSelected = true;
        }
        private void CancleSelect(TShape tshape)
        {
            TShape shape = Shapes.Where(s => s == tshape).FirstOrDefault();
            Debug.WriteLine("Cancle :" + shape.GetHashCode());
            shape.isSelected = false;
        }

        private bool isOutsideAllShape(CPoint pt)
        {
            bool isInside = true;
            foreach (TShape shape in Shapes)
            {
                TRect rect = shape as TRect;
                double left = Canvas.GetLeft(rect.CanvasRect);
                double top = Canvas.GetTop(rect.CanvasRect);
                double right = Canvas.GetRight(rect.CanvasRect);
                double bottom = Canvas.GetBottom(rect.CanvasRect);
                Debug.WriteLine("isOutSideAll :" + shape.UIElement.GetHashCode());
                if (pt.X < right && pt.X > left &&
                    pt.Y > top && pt.Y < bottom)
                    isInside = false;
            }
            return isInside;
        }
        
        private void MakeModifyTool(TShape shape)
        {
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

        private void ModifyRect(CPoint nowPt)
        {
            int offset_x = nowPt.X - PointBuffer.X;
            int offset_y = nowPt.Y - PointBuffer.Y;
            CPoint ptOffset = new CPoint(offset_x, offset_y);

            if (true)
            {
                foreach (TShape shape in Shapes)
                {
                    if (shape.isSelected)
                    {
                        
                        shape.ModifyTool.Visibility = Visibility.Collapsed;
                        double left = Canvas.GetLeft(shape.UIElement);
                        double top = Canvas.GetTop(shape.UIElement);
                        double newleft = int.Parse(left.ToString());
                        double newtop = int.Parse(top.ToString());
                        newleft += ptOffset.X;
                        newtop += ptOffset.Y;
                        
                        Debug.WriteLine("Modifying :" + shape.UIElement.GetHashCode());
                        Canvas.SetLeft(shape.UIElement, newleft);
                        Canvas.SetTop(shape.UIElement, newtop);
                    }
                }
                PointBuffer = nowPt;
            }
        }

        public ToolProcess SetState(ToolProcess state)
        {
            eToolProgress = state;
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
