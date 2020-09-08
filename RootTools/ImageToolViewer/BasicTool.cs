using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RootTools
{
    public class BasicTool : ObservableObject
    {

        public BasicTool()
        {
            InitCrossLine();
            Shapes.CollectionChanged += M_listShape_CollectionChanged;   
        }

        private void M_listShape_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            History.Push(Shapes);
            //p_BasicElement로 Add/Remove
        }

        Line Vertical, Horizon;
        TShape tshape;
        public ToolProgress eToolProgress = ToolProgress.None;
        public ToolType eToolType = ToolType.None;
        public Stack<ObservableCollection<TShape>> History = new Stack<ObservableCollection<TShape>>();
        public ObservableCollection<TShape> Shapes = new ObservableCollection<TShape>();
        public List<TShape> SelectedShapes = new List<TShape>();

        public CPoint m_ptMouseBuffer;
        public ICommand Clear
        {
            get
            {
                return new RelayCommand(_clear);
            }
        }
        public void _clear()
        {
            Shapes.Clear();
            p_BasicElement.Clear();
            InitCrossLine();
        }


        #region Property
        /// <summary>
        /// UI Proerty
        /// </summary>

        private ObservableCollection<UIElement> m_BasicElement = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_BasicElement
        {
            get
            {
                return m_BasicElement;
            }
            set
            {
                m_BasicElement = value;
            }
        }

        private Cursor m_Cursor;
        public Cursor p_Cursor
        {
            get
            {
                return m_Cursor;
            }
            set
            {
                SetProperty(ref m_Cursor, value);
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

        public void InitCrossLine()
        {
            if (p_BasicElement.Contains(Vertical) && p_BasicElement.Contains(Horizon))
            {
                p_BasicElement.Remove(Vertical);
                p_BasicElement.Remove(Horizon);
            }

            Vertical = new Line();
            Horizon = new Line();

            Brush LineBrush = Brushes.Silver;
            double LineThick = 1;
            DoubleCollection LineDash = new DoubleCollection { 3, 4 };

            Vertical.Stroke = LineBrush;
            Vertical.StrokeThickness = LineThick;
            Vertical.StrokeDashArray = LineDash;
            Horizon.Stroke = LineBrush;
            Horizon.StrokeThickness = LineThick;
            Horizon.StrokeDashArray = LineDash;

            p_BasicElement.Add(Vertical);
            p_BasicElement.Add(Horizon);
        }
        public void DrawCrossLine(CPoint nowPt, int canvasWidth, int canvasHeight)
        {
            try
            {
                Vertical.X1 = nowPt.X;
                Vertical.X2 = nowPt.X;

                Horizon.Y1 = nowPt.Y;
                Horizon.Y2 = nowPt.Y;


                Vertical.Y2 = canvasHeight;
                Horizon.X2 = canvasWidth;
            }
            catch (Exception e)
            {
                return;
            }
            return;
        }
       
        public void DrawTool(CPoint canvasPt, CPoint memPt, MouseEventArgs e = null)
        {
            bool LeftDown = false;
            if (e != null)
                LeftDown = e.LeftButton == MouseButtonState.Pressed;

            switch (eToolProgress)
            {
                case ToolProgress.None:
                    switch (eToolType)
                    {
                        case ToolType.None:
                            {
                                //Clear Select
                                if (LeftDown)
                                {
                                    bool binside = false;
                                    foreach (TShape shape in Shapes)
                                    {
                                        TRect rect = shape as TRect;
                                        if (canvasPt.X < rect.CanvasRight &&
                                            canvasPt.X > rect.CanvasLeft &&
                                            canvasPt.Y > rect.CanvasTop &&
                                            canvasPt.Y < rect.CanvasBottom)
                                            binside = true;
                                    }
                                    if (binside == false)
                                        foreach (TShape shape in Shapes)
                                            CancleSelect(shape);
                                }
                                //if (LeftDown && p_Cursor != Cursors.Arrow)
                                //{
                                //    m_ptMouseBuffer = canvasPt;
                                //    SetState(ToolProgress.Adjust);
                                //}
                                break;
                            }
                        case ToolType.Rect:
                            {
                                if (LeftDown)
                                    SetState(ToolProgress.Start);
                                break;
                            }
                    }
                    break;
                case ToolProgress.Start:
                    switch (eToolType)
                    {
                        case ToolType.Rect:
                            {
                                tshape = new TRect(Brushes.Yellow, 1);
                                TRect rect = tshape as TRect;

                                rect.SetLeftTop(canvasPt, memPt);
                                p_BasicElement.Add(rect.CanvasRect);
                                SetState(ToolProgress.Drawing);
                                break;
                            }
                    }
                    break;
                case ToolProgress.Drawing:
                    if (LeftDown)
                        SetState(ToolProgress.Done);
                    else
                        MakeTShape(tshape, canvasPt, memPt);
                    break;
                case ToolProgress.Adjust:
                    //if (LeftDown)
                    //    SetState(ToolProgress.Done);

                    //    foreach (TShape shape in SelectedShapes)
                    //            ModifyTShape(shape, canvasPt, memPt);
                    //    break;
                    //listSelectedShape 를 for문돌리면서 크기 수정?
                    break;
                case ToolProgress.Done:
                    switch (eToolType)
                    {
                        case ToolType.Rect:
                            {
                                TRect rect = tshape as TRect;
                                rect.SetRightBottom(canvasPt, memPt);
                                //rect.CanvasRect.Fill = Brushes.Transparent;
                                rect.CanvasRect.Tag = rect;
                                rect.CanvasRect.Cursor = Cursors.Hand;
                                rect.CanvasRect.MouseLeftButtonDown += CanvasRect_MouseLeftButtonDown;
                                SetState(ToolProgress.None);
                                p_SelectedToolIndex = 0;
                                break;
                            }
                    }
                    Shapes.Add(tshape);
                    break;
            }
        }
        private void SelectTShape(TShape tshape)
        {
            TRect rect = tshape as TRect;
            rect.isSelected = true;
            double left = rect.CanvasLeft;
            double top = rect.CanvasTop;
            double right = rect.CanvasRight;
            double bottom = rect.CanvasBottom;

            //Rectangle ModifiedRect = new Rectangle();
            //Canvas.SetLeft(ModifiedRect, left - 2);
            //Canvas.SetTop(ModifiedRect, top - 2);

            //ModifiedRect.Width = rect.CanvasRect.Width + 4;
            //ModifiedRect.Height = rect.CanvasRect.Height + 4;
            //ModifiedRect.Stroke = Brushes.DodgerBlue;
            //ModifiedRect.StrokeThickness = 1;
            //p_BasicElement.Add(ModifiedRect);

            int index = 0;
            for (double i = left - 5; i <= right + 5; i += (int)rect.CanvasRect.Width / 2)
                for (double j = top - 5; j <= bottom + 5; j += (int)rect.CanvasRect.Height / 2)
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Tag = rect;
                    ellipse.Width = 10;
                    ellipse.Height = 10;
                    ellipse.Fill = Brushes.White;
                    Canvas.SetLeft(ellipse, i);
                    Canvas.SetTop(ellipse, j);
                    Canvas.SetZIndex(ellipse, 99);
                    if (index == 0)
                        ellipse.Cursor = Cursors.SizeNWSE;
                    if (index == 1)
                        ellipse.Cursor = Cursors.SizeWE;
                    if (index == 2)
                        ellipse.Cursor = Cursors.SizeNESW;
                    if (index == 3)
                        ellipse.Cursor = Cursors.SizeNS;
                    if (index == 4)
                        ellipse.Cursor = Cursors.ScrollAll;
                    if (index == 5)
                        ellipse.Cursor = Cursors.SizeNS;
                    if (index == 6)
                        ellipse.Cursor = Cursors.SizeNESW;
                    if (index == 7)
                        ellipse.Cursor = Cursors.SizeWE;
                    if (index == 8)
                        ellipse.Cursor = Cursors.SizeNWSE;

                    index++;
                    p_BasicElement.Add(ellipse);
                }
            SelectedShapes.Add(tshape);
       
        }
        private void CancleSelect(TShape tshape)
        {
            TRect rect = tshape as TRect;
            rect.isSelected = false;
            SelectedShapes.Remove(rect);

            List<UIElement> delete = new List<UIElement>();
            foreach (UIElement ui in p_BasicElement)
            {
                if (ui.GetType() == typeof(Ellipse))
                    if ((ui as Ellipse).Tag == rect)
                        delete.Add(ui);
            }
            foreach (UIElement ui in delete)
            {
                p_BasicElement.Remove(ui);
            }
        }
        private void CanvasRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TRect rect = (sender as Rectangle).Tag as TRect;
            if (rect.isSelected)
                CancleSelect(rect);
            else
                SelectTShape(rect);   
        }

        public void MakeTShape(TShape tshape, CPoint canvasNowPt, CPoint memNowPt)
        {
            switch (tshape)
            {
                case TPoint tPoint:
                    {
                        break;
                    }
                case TLine tLine:
                    {
                        break;
                    }
                case TRect tRect:
                    {
                        if (tRect.CanvasLeft > canvasNowPt.X)
                        {
                            Canvas.SetLeft(tRect.CanvasRect, canvasNowPt.X);
                        }
                        if (tRect.CanvasTop > canvasNowPt.Y)
                        {
                            Canvas.SetTop(tRect.CanvasRect, canvasNowPt.Y);
                        }

                        tRect.CanvasRect.Width = Math.Abs(canvasNowPt.X - tRect.CanvasLeft);
                        tRect.CanvasRect.Height = Math.Abs(canvasNowPt.Y - tRect.CanvasTop);

                        break;
                    }
            }


        }

        public ToolProgress SetState(ToolProgress state)
        {
            eToolProgress = state;
            return state;
        }

        public void ModifyTShape(TShape tshape, CPoint canvasPt, CPoint memPt)
        {
            int offset_x = canvasPt.X - m_ptMouseBuffer.X;
            int offset_y = canvasPt.Y - m_ptMouseBuffer.Y;
            CPoint Offset = new CPoint(offset_x, offset_y);

           
            switch (tshape)
            {
                case TRect tRect:
                    int new_x1 = tRect.CanvasLeft;
                    int new_y1 = tRect.CanvasTop;
                    int new_x2 = tRect.CanvasRight;
                    int new_y2 = tRect.CanvasBottom;
                    if (p_Cursor == Cursors.ScrollAll)
                    {
                        new_x1 += Offset.X;
                        new_y1 += Offset.Y;
                        new_x2 += Offset.X;
                        new_y2 += Offset.Y;
                    }
                    CPoint LT = new CPoint(new_x1, new_y1);
                    CPoint RB = new CPoint(new_x2, new_y2);
                    tRect.SetLeftTop(LT, memPt);
                    tRect.SetRightBottom(RB, memPt);
                    break;
            }




        }


        //안쓰는중...
        public HitType SetHitType(TShape shape, CPoint canvasNowPt)
        {
            switch (shape)
            {
                case TRect tRect:
                    {
                        double left = Canvas.GetLeft(tRect.CanvasRect);
                        double top = Canvas.GetTop(tRect.CanvasRect);
                        double right = left + tRect.CanvasRect.Width;
                        double bottom = top + tRect.CanvasRect.Height;

                        const double GAP = 10;
                        if (canvasNowPt.X < left)
                            return HitType.None;
                        if (canvasNowPt.X > right)
                            return HitType.None;
                        if (canvasNowPt.Y < top)
                            return HitType.None;
                        if (canvasNowPt.Y > bottom)
                            return HitType.None;
                        if (-1 * GAP <= canvasNowPt.X - left && canvasNowPt.X - left <= GAP)
                        {
                            if (-1 * GAP <= canvasNowPt.Y - top && canvasNowPt.Y - top <= GAP)
                                return HitType.UL;
                            if (-1 * GAP <= bottom - canvasNowPt.Y && bottom - canvasNowPt.Y <= GAP)
                                return HitType.LL;
                            return HitType.L;
                        }
                        if (-1 * GAP < right - canvasNowPt.X && right - canvasNowPt.X <= GAP)
                        {
                            if (-1 * GAP <= canvasNowPt.Y - top && canvasNowPt.Y - top <= GAP)
                                return HitType.UR;
                            if (-1 * GAP <= bottom - canvasNowPt.Y && bottom - canvasNowPt.Y <= GAP)
                                return HitType.LR;
                            return HitType.R;
                        }
                        if (-1 * GAP <= canvasNowPt.Y - top && canvasNowPt.Y - top <= GAP)
                            return HitType.T;
                        if (-1 * GAP <= bottom - canvasNowPt.Y && bottom - canvasNowPt.Y <= GAP)
                            return HitType.B;
                        if (left == 0)
                            return HitType.None;

                        return HitType.Body;
                        break;
                    }

            }
            return HitType.None;
        }        
        public Cursor SetMouseCursor(HitType m_MouseHitType)
        {
            // See what cursor we should display.
            Cursor _cursor = Cursors.Arrow;
            switch (m_MouseHitType)
            {
                case HitType.None:
                    _cursor = Cursors.Arrow;
                    break;
                case HitType.Body:
                    _cursor = Cursors.ScrollAll;
                    break;
                case HitType.UL:
                case HitType.LR:
                    _cursor = Cursors.SizeNWSE;
                    break;
                case HitType.LL:
                case HitType.UR:
                    _cursor = Cursors.SizeNESW;
                    break;
                case HitType.T:
                case HitType.B:
                    _cursor = Cursors.SizeNS;
                    break;
                case HitType.L:
                case HitType.R:
                    _cursor = Cursors.SizeWE;
                    break;
            }

            return _cursor;
        }
        private FrameworkElement Clone(FrameworkElement e)
        {
            System.Xml.XmlDocument document = new System.Xml.XmlDocument();
            document.LoadXml(System.Windows.Markup.XamlWriter.Save(e));

            return (FrameworkElement)System.Windows.Markup.XamlReader.Load(new System.Xml.XmlNodeReader(document));
        }
    }
    




    public class BasicShape
    {
        /// <summary>
        /// Basic Shape Init
        /// </summary>
        /// <param name="shapeType">Rectangle, Line, others..</param>
        /// <param name="brush">Shape Brush</param>
        /// <param name="thickness">Shape Line Thickness</param>
        public BasicShape(Shape shapeType, Brush brush, double thickness)
        {
            m_CanvasShape = shapeType;
            m_CanvasShape.Stroke = brush;
            m_CanvasShape.StrokeThickness = thickness;
        }
        public void SetStartPoint(CPoint memStartPt, CPoint canvasStartPt)
        {
            m_MemoryStartPt = memStartPt;
            m_CanvasStartPt = canvasStartPt;
        }
        public void SetEndPoint(CPoint memEndPt, CPoint canvasEndPt)
        {
            m_MemoryEndPt = memEndPt;
            m_CanvasEndPt = canvasEndPt;
            
        }

        /// <summary>
        /// Shape in Canvas
        /// </summary>
        public Shape m_CanvasShape;
        /// <summary>
        /// Object in Memory
        /// </summary>
        public Rect m_MemoryObject;
        /// <summary>
        /// Shape(Rectangle, Line) Canvas Start Point
        /// </summary>
        public CPoint m_CanvasStartPt;
        
        private CPoint _canvasEndPt;
        /// <summary>
        /// Shape(Rectangle, Line) Canvas End Point
        /// </summary>
        public CPoint m_CanvasEndPt
        {
            get
            {
                return _canvasEndPt;
            }
            set
            {
                m_CanvasShape.Width = Math.Abs(value.X - m_CanvasStartPt.X);
                m_CanvasShape.Height= Math.Abs(value.Y - m_CanvasStartPt.Y);

                _canvasEndPt = value;

            }
        }
        /// <summary>
        /// Memory Start Point
        /// </summary>
        public CPoint m_MemoryStartPt;
        private CPoint _memoryEndPt;
        /// <summary>
        /// Memory End Point
        /// </summary>
        public CPoint m_MemoryEndPt
        {
            get
            {
                return _memoryEndPt;
            }
            set
            {
                m_MemoryObject.Width = Math.Abs(value.X - m_MemoryStartPt.X);
                m_MemoryObject.Height = Math.Abs(value.Y - m_MemoryStartPt.Y);
                _memoryEndPt = value;
            }
        }

    }

    public enum ToolType
    {
        None,
        Rect,
        Line,
        Circle,
        Polygon,
    }
    public enum ToolProgress
    {
        None,
        Start,
        Drawing,
        Adjust,
        Done,
    }
    public enum HitType
    {
        None, Body, UL, UR, LR, LL, L, R, T, B
    };
    public class TList<T> : List<T>
    {
        public event EventHandler Change;

        public new void Add(T item)
        {
            if (Change != null)
                Change(this, null);
            base.Add(item);
        }
    }

}