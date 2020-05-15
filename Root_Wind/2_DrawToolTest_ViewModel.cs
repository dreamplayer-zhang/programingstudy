using RootTools;
using RootTools.Memory;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Globalization;

namespace Root_Wind
{
    class _2_DrawToolTest_ViewModel : ObservableObject
    {
        Wind_Engineer m_Engineer;
        MemoryTool m_MemoryModule;
        ImageData m_Image;

        DrawHelper m_DrawHelper;
        DrawData m_DD;

        DrawingMode _Mode = DrawingMode.None;
        public DrawingMode p_Mode
        {
            get
            {
                return _Mode;
            }
            set
            {
                SetProperty(ref _Mode, value);
                SetToolMode();
            }
        }

        string sPool = "pool";
        string sGroup = "group";
        string sMem = "mem";
        public int MemWidth = 40000;
        public int MemHeight = 40000;
        public _2_DrawToolTest_ViewModel(Wind_Engineer engineer, IDialogService dialogService)
        {
            m_Engineer = engineer;
            Init(engineer, dialogService);

            }


        #region Properties
        private ImageViewer_ViewModel m_ImageViewer;
        public ImageViewer_ViewModel p_ImageViewer
        {
            get
            {
                return m_ImageViewer;
            }
            set
            {
                SetProperty(ref m_ImageViewer, value);
                    }
                }

        private ObservableCollection<UIElement> _UIelement = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_UIElement
        {
            get
            {
                return _UIelement;
            }
            set
            {
                SetProperty(ref _UIelement, value);
            }
        }

        private System.Windows.Input.Cursor _recipeCursor;
        public System.Windows.Input.Cursor RecipeCursor
        {
            get
            {
                return _recipeCursor;
            }
            set
            {
                SetProperty(ref _recipeCursor, value);
            }
        }

        string _test = "";
        public string Test
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
        private System.Windows.Input.MouseEventArgs _mouseEvent;
        public System.Windows.Input.MouseEventArgs MouseEvent
        {
            get
            {
                return _mouseEvent;
            }
            set
            {
                SetProperty(ref _mouseEvent, value);
            }
        }

        #endregion

        #region Func
        CPoint GetMemPoint(int canvasX, int canvasY)
        {
            int nX = p_ImageViewer.p_View_Rect.X + canvasX * p_ImageViewer.p_View_Rect.Width / p_ImageViewer.p_CanvasWidth;
            int nY = p_ImageViewer.p_View_Rect.Y + canvasY * p_ImageViewer.p_View_Rect.Height / p_ImageViewer.p_CanvasHeight;
            return new CPoint(nX, nY);
        }
        CPoint GetCanvasPoint(int memX, int memY)
        {
            if (p_ImageViewer.p_View_Rect.Width > 0 && p_ImageViewer.p_View_Rect.Height > 0)
            {
                
                int nX = (int)Math.Round((double)(memX - p_ImageViewer.p_View_Rect.X) * p_ImageViewer.p_CanvasWidth / p_ImageViewer.p_View_Rect.Width, MidpointRounding.ToEven);
                //int xx = (memX - p_ROI_Rect.X) * ViewWidth / p_ROI_Rect.Width;
                int nY = (int)Math.Round((double)(memY - p_ImageViewer.p_View_Rect.Y) * p_ImageViewer.p_CanvasHeight / p_ImageViewer.p_View_Rect.Height, MidpointRounding.AwayFromZero);
                return new CPoint(nX, nY);
            }
            return new CPoint(0, 0);
        }
        System.Windows.Media.Color ConvertColor(System.Drawing.Color color)
        {
            System.Windows.Media.Color c = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
            return c;
        }
        #endregion

        #region Method
        void Init(Wind_Engineer engineer, IDialogService dialogService)
        {
            m_MemoryModule = engineer.ClassMemoryTool();
            MemoryPool memoryPool = m_MemoryModule.GetPool(sPool, true);
            memoryPool.p_gbPool = 10;
            memoryPool.GetGroup(sGroup).CreateMemory(sMem, 1, 1, new CPoint(MemWidth, MemHeight));

            m_DD = new DrawData();
            m_Image = new ImageData(memoryPool.GetMemory(sGroup, sMem));
            p_ImageViewer = new ImageViewer_ViewModel(m_Image, dialogService);
            }

        private void MouseLeftDown()
        {
            switch (p_Mode)
            {
                case DrawingMode.None:
                    {
                        RedrawUIElement();
                        break;
                    }
                case DrawingMode.Rectangle:
                    {
                        StartDrawingRect();
                        break;
                    }
                case DrawingMode.Str:
                    {
                        AddString();
                        break;
                    }
                case DrawingMode.Ruler:
                    {
                        StartDrawingLine();
                        break;
            }
                case DrawingMode.Pt:
                    {
                        AddPoint();
                        break;
        }
            }

        }
        private void MouseMove()
        {
            if (p_Mode == DrawingMode.None)
            {
                if (MouseEvent.LeftButton == MouseButtonState.Pressed)
                    RedrawUIElement();
            }
            if (p_Mode == DrawingMode.Rectangle)
            {
                if (MouseEvent.LeftButton == MouseButtonState.Pressed)
                    DrawingRectProgress();
            }
            if (p_Mode == DrawingMode.Ruler)
            {
                    DrawingLineProgress();
            }
            if (m_DrawHelper != null && m_DrawHelper.DrawnRect != null)
            {
                //CPoint CanvasPt = new CPoint(CanvasX, CanvasY);
            }


        }
        private void MouseLeftUp()
        {
            if (p_Mode == DrawingMode.Rectangle)
            {
                DrawRectDone();
            }
            if (p_Mode == DrawingMode.Ruler)
            {
                DrawLineDone();
        }
            else
                RedrawUIElement();
        }

        private void RedrawUIElement()
        {
            RedrawRect();
            RedrawStr();
            RedrawPt();
            RedrawLine();
        }
        private void RedrawLine()
        {
            if (m_DD.m_LineData.Count > 0)
            {
                for (int i = 0; i < m_DD.m_LineData.Count; i++)
                {
                    Line RedrawnLine = new Line();
                    CPoint StartPt = GetCanvasPoint(m_DD.m_LineData[i].m_ptS.X, m_DD.m_LineData[i].m_ptS.Y);
                    CPoint EndPt = GetCanvasPoint(m_DD.m_LineData[i].m_ptE.X, m_DD.m_LineData[i].m_ptE.Y);
                    RedrawnLine.X1 = StartPt.X;
                    RedrawnLine.Y1 = StartPt.Y;
                    RedrawnLine.X2 = EndPt.X;
                    RedrawnLine.Y2 = EndPt.Y;
            
                    
                    RedrawnLine.Stroke = System.Windows.Media.Brushes.Red;
                    RedrawnLine.StrokeThickness = 2;
                    p_UIElement.Add(RedrawnLine);
                }
            }

        }
        private void RedrawRect()
        {
            if (m_DD.m_RectData.Count > 0)
            {
                p_UIElement.Clear();
                for (int i = 0; i < m_DD.m_RectData.Count; i++)
                {
                    System.Windows.Shapes.Rectangle RedrawnRect = new System.Windows.Shapes.Rectangle();
                    CPoint LeftTopPt = GetCanvasPoint(m_DD.m_RectData[i].m_rt.Left, m_DD.m_RectData[i].m_rt.Top);
                    CPoint RighBottomPt = GetCanvasPoint(m_DD.m_RectData[i].m_rt.Right, m_DD.m_RectData[i].m_rt.Bottom);
                    RedrawnRect.Stroke = new SolidColorBrush(ConvertColor(m_DD.m_RectData[i].m_color));
                    RedrawnRect.StrokeThickness = 2;


                    Canvas.SetLeft(RedrawnRect, LeftTopPt.X);
                    Canvas.SetTop(RedrawnRect, LeftTopPt.Y);

                    RedrawnRect.Width = Math.Abs(LeftTopPt.X - RighBottomPt.X);
                    RedrawnRect.Height = Math.Abs(LeftTopPt.Y - RighBottomPt.Y);
                    p_UIElement.Add(RedrawnRect);
                }
            }
        }
        private void RedrawPt()
        {
            if (m_DD.m_PointData.Count > 0)
            {
                for (int i = 0; i < m_DD.m_PointData.Count; i++)
                {
                    Line RedrawnHorizon = new Line();
                    Line RedrawnVertical = new Line();

                    CPoint ptPoint = GetCanvasPoint(m_DD.m_PointData[i].m_pt.X, m_DD.m_PointData[i].m_pt.Y);
                    RedrawnHorizon.StrokeThickness = 2;
                    RedrawnVertical.StrokeThickness = 2;

                    RedrawnHorizon.Stroke = new SolidColorBrush(ConvertColor(m_DD.m_PointData[i].m_color));
                    RedrawnVertical.Stroke = new SolidColorBrush(ConvertColor(m_DD.m_PointData[i].m_color));

                    RedrawnVertical.X1 = ptPoint.X;
                    RedrawnVertical.Y1 = ptPoint.Y - m_DD.m_PointData[i].m_size;
                    RedrawnVertical.X2 = ptPoint.X;
                    RedrawnVertical.Y2 = ptPoint.Y + m_DD.m_PointData[i].m_size;

                    RedrawnHorizon.X1 = ptPoint.X - m_DD.m_PointData[i].m_size;
                    RedrawnHorizon.Y1 = ptPoint.Y;
                    RedrawnHorizon.X2 = ptPoint.X + m_DD.m_PointData[i].m_size;
                    RedrawnHorizon.Y2 = ptPoint.Y;

                    p_UIElement.Add(RedrawnHorizon);
                    p_UIElement.Add(RedrawnVertical);
                }
            }
        }
        private void RedrawStr()
        {
            if (m_DD.m_StringData.Count > 0)
        {
                for (int i = 0; i < m_DD.m_StringData.Count; i++)
            {
                    TextBlock RedrawnTB = new TextBlock();
                    CPoint TbPt = GetCanvasPoint(m_DD.m_StringData[i].m_pt.X, m_DD.m_StringData[i].m_pt.Y);
                    RedrawnTB.Text = m_DD.m_StringData[i].m_str;
                    RedrawnTB.Foreground = new SolidColorBrush(ConvertColor(m_DD.m_StringData[i].m_color));
                    Canvas.SetLeft(RedrawnTB, TbPt.X);
                    Canvas.SetTop(RedrawnTB, TbPt.Y);
                    //if (ViewWidth < Canvas.GetLeft(RedrawnTB) + RedrawnTB.ActualWidth)
                    //{
                    //    if (ViewWidth > Canvas.GetLeft(RedrawnTB))
                    //    {
                    //        RedrawnTB.Width = ViewWidth - Canvas.GetLeft(RedrawnTB);
                    //    }
                    //    else
                    //    {
                    //        RedrawnTB.Width = 0;
                    //    }

                    //}
                    //if (ViewHeight < Canvas.GetTop(RedrawnTB) + RedrawnTB.ActualHeight)
                    //{
                    //    if (ViewHeight > Canvas.GetTop(RedrawnTB))
                    //    {
                    //        RedrawnTB.Height = ViewHeight - Canvas.GetTop(RedrawnTB);
                    //    }
                    //    else
                    //    {
                    //        RedrawnTB.Height = 0;
                    //    }
                    //}
                    //if (Canvas.GetLeft(RedrawnTB) < 0)
                    //{
                    //    if (Math.Abs(Canvas.GetLeft(RedrawnTB)) < RedrawnTB.ActualWidth)
                    //    {
                    //        RedrawnTB.Width = RedrawnTB.Width - Math.Abs(Canvas.GetLeft(RedrawnTB));
                    //        Canvas.SetLeft(RedrawnTB, 0);
                    //    }
                    //    else
                    //    {
                    //        RedrawnTB.Height = 0;
                    //    }
                    //}
                    //if (Canvas.GetTop(RedrawnTB) < 0)
                    //{
                    //    if (Math.Abs(Canvas.GetTop(RedrawnTB)) < RedrawnTB.ActualHeight)
                    //    {
                    //        RedrawnTB.Height = RedrawnTB.Height - Math.Abs(Canvas.GetTop(RedrawnTB));
                    //        Canvas.SetTop(RedrawnTB, 0);
                    //    }
                    //    else
                    //    {
                    //        RedrawnTB.Height = 0;
                    //    }
                    //}
                    p_UIElement.Add(RedrawnTB);
                }

                }
            }

        private void SetToolMode()
        {
            if (p_Mode == DrawingMode.None)
            {
                p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.None;
                RecipeCursor = System.Windows.Input.Cursors.Arrow;
            }
            else
            {
                p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.Tool;
                RecipeCursor = System.Windows.Input.Cursors.Cross;
            }
        }

        private void StartDrawingRect()
        {
            if (m_DrawHelper == null)
                m_DrawHelper = new DrawHelper();
            //if (p_UIElement.Contains(m_DrawHelper.DrawnRect))
            //    p_UIElement.Remove(m_DrawHelper.DrawnRect);

            m_DrawHelper.DrawnRect = new System.Windows.Shapes.Rectangle();

            m_DrawHelper.RectMemory_StartPt = new CPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);
            m_DrawHelper.RectCanvas_StartPt = GetCanvasPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);
                
            Canvas.SetLeft(m_DrawHelper.DrawnRect, m_DrawHelper.RectCanvas_StartPt.X);
            Canvas.SetTop(m_DrawHelper.DrawnRect, m_DrawHelper.RectCanvas_StartPt.Y);

            m_DrawHelper.DrawnRect.Stroke = System.Windows.Media.Brushes.Red;
            m_DrawHelper.DrawnRect.StrokeThickness = 2;
            m_DrawHelper.DrawnRect.StrokeDashArray = new DoubleCollection { 3, 2 };
            p_UIElement.Add(m_DrawHelper.DrawnRect);

        }
        private void DrawingRectProgress()
        {
            if (m_DrawHelper.DrawnRect != null)
            {
                m_DrawHelper.RectMemory_EndPt = new CPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);
                m_DrawHelper.RectCanvas_EndPt = GetCanvasPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);

                if (m_DrawHelper.RectMemory_EndPt.X < m_DrawHelper.RectMemory_StartPt.X)
                {
                    Canvas.SetLeft(m_DrawHelper.DrawnRect, m_DrawHelper.RectCanvas_EndPt.X);
                        }
                if (m_DrawHelper.RectMemory_EndPt.Y < m_DrawHelper.RectMemory_StartPt.Y)
                        {
                    Canvas.SetTop(m_DrawHelper.DrawnRect, m_DrawHelper.RectCanvas_EndPt.Y);
                        }
                m_DrawHelper.DrawnRect.Width = Math.Abs(m_DrawHelper.RectCanvas_EndPt.X - m_DrawHelper.RectCanvas_StartPt.X);
                m_DrawHelper.DrawnRect.Height = Math.Abs(m_DrawHelper.RectCanvas_EndPt.Y - m_DrawHelper.RectCanvas_StartPt.Y);
                    }
                        }
        private void DrawRectDone()
                        {
            if (m_DrawHelper.RectMemory_StartPt != m_DrawHelper.RectMemory_EndPt && m_DrawHelper.RectMemory_EndPt != null)
                    {
                m_DrawHelper.RectOrigin = new CRect();

                m_DrawHelper.RectOrigin.Left = m_DrawHelper.RectMemory_StartPt.X;
                m_DrawHelper.RectOrigin.Top = m_DrawHelper.RectMemory_StartPt.Y;
                m_DrawHelper.RectOrigin.Right = m_DrawHelper.RectMemory_EndPt.X;
                m_DrawHelper.RectOrigin.Bottom = m_DrawHelper.RectMemory_EndPt.Y;

                if (m_DrawHelper.RectMemory_EndPt.X < m_DrawHelper.RectMemory_StartPt.X)
                        {
                    m_DrawHelper.RectOrigin.Left = m_DrawHelper.RectMemory_EndPt.X;
                    m_DrawHelper.RectOrigin.Right = m_DrawHelper.RectMemory_StartPt.X;
                        }
                if (m_DrawHelper.RectMemory_EndPt.Y < m_DrawHelper.RectMemory_StartPt.Y)
                        {
                    m_DrawHelper.RectOrigin.Top = m_DrawHelper.RectMemory_EndPt.Y;
                    m_DrawHelper.RectOrigin.Bottom = m_DrawHelper.RectMemory_StartPt.Y;
                        }
                m_DD.AddRectData(m_DrawHelper.RectOrigin, System.Drawing.Color.Red);
                m_DrawHelper.DrawnRect.StrokeDashArray = new DoubleCollection(1);
                    }
                        else
                        {
                m_DrawHelper.DrawnRect = null;
                        }
                    }

        private void AddString()
                {
            if (m_DrawHelper == null)
                m_DrawHelper = new DrawHelper();

            m_DrawHelper.TbOrigin = new TextBlock();
            m_DrawHelper.Str_MemoryPt = new CPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);
            m_DrawHelper.Str_CanvasPt = GetCanvasPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);

            string text = "Test";
            System.Drawing.Color str_color = System.Drawing.Color.Red;
            CPoint str_point = m_DrawHelper.Str_MemoryPt;
            m_DD.AddString(text, str_point, str_color);


            m_DrawHelper.TbOrigin.Text = text;
            m_DrawHelper.TbOrigin.Foreground = new SolidColorBrush(ConvertColor(str_color));
            m_DrawHelper.TbOrigin.Background = null;
            Canvas.SetLeft(m_DrawHelper.TbOrigin, m_DrawHelper.Str_CanvasPt.X);
            Canvas.SetTop(m_DrawHelper.TbOrigin, m_DrawHelper.Str_CanvasPt.Y);
            p_UIElement.Add(m_DrawHelper.TbOrigin);
                        }
        private void StartDrawingLine()
        {
            if (m_DrawHelper == null)
                m_DrawHelper = new DrawHelper();

            m_DrawHelper.DrawnLine = new Line();

            m_DrawHelper.Line_Memory_StartPt = new CPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);
            m_DrawHelper.Line_Canvas_StartPt = GetCanvasPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);

            m_DrawHelper.DrawnLine.X1 = m_DrawHelper.Line_Canvas_StartPt.X;
            m_DrawHelper.DrawnLine.Y1 = m_DrawHelper.Line_Canvas_StartPt.Y;


            p_UIElement.Add(m_DrawHelper.DrawnLine);
        }
        private void DrawingLineProgress()
        {
            if (m_DrawHelper != null && m_DrawHelper.DrawnLine != null)
            {
                m_DrawHelper.DrawnLine.Stroke = System.Windows.Media.Brushes.Red;
                m_DrawHelper.DrawnLine.StrokeThickness = 2;
                m_DrawHelper.DrawnLine.StrokeDashArray = new DoubleCollection { 3, 2 };

                m_DrawHelper.Line_Memory_EndPt = new CPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);
                m_DrawHelper.Line_Canvas_EndPt = GetCanvasPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);

                //if(m_DrawHelper.Line_Memory_EndPt.X < m_DrawHelper.Line_Memory_StartPt.X)
                //{
                //    m_DrawHelper.DrawnLine.X1 = m_DrawHelper.Line_Memory_EndPt.X;
                //}
                //if(m_DrawHelper.Line_Memory_EndPt.Y < m_DrawHelper.Line_Memory_StartPt.Y)
                //{
                //    m_DrawHelper.DrawnLine.Y1 = m_DrawHelper.Line_Memory_EndPt.Y;
                //}
                m_DrawHelper.DrawnLine.X2 = m_DrawHelper.Line_Canvas_EndPt.X;
                m_DrawHelper.DrawnLine.Y2 = m_DrawHelper.Line_Canvas_EndPt.Y;
            }
        }
        private void DrawLineDone()
        {
            m_DrawHelper.DrawnLine.StrokeDashArray = new DoubleCollection(1);
            m_DD.AddLineData(m_DrawHelper.Line_Memory_StartPt, m_DrawHelper.Line_Memory_EndPt, System.Drawing.Color.Red);
            m_DrawHelper.DrawnLine = null;
        }

        private void AddPoint()
                        {
            if (m_DrawHelper == null)
                m_DrawHelper = new DrawHelper();


            
            m_DrawHelper.DrawnHorizon = new Line();
            m_DrawHelper.DrawnVertical = new Line();

            m_DrawHelper.pt_MemoryPt = new CPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);
            m_DrawHelper.pt_CanvasPt = GetCanvasPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);


            int size = 5;
            System.Drawing.Color pt_Color = System.Drawing.Color.Red;
            CPoint pt_Point = m_DrawHelper.pt_MemoryPt;

            m_DD.AddPointData(PointData.PointType.Plus, size, m_DrawHelper.pt_MemoryPt, pt_Color);

            m_DrawHelper.DrawnHorizon.Stroke = System.Windows.Media.Brushes.Red;
            m_DrawHelper.DrawnVertical.Stroke = new SolidColorBrush(ConvertColor(pt_Color));
            m_DrawHelper.DrawnHorizon.StrokeThickness = 2;
            m_DrawHelper.DrawnVertical.StrokeThickness = 2;

            m_DrawHelper.DrawnVertical.X1 = m_DrawHelper.pt_CanvasPt.X;
            m_DrawHelper.DrawnVertical.Y1 = m_DrawHelper.pt_CanvasPt.Y - size;
            m_DrawHelper.DrawnVertical.X2 = m_DrawHelper.pt_CanvasPt.X;
            m_DrawHelper.DrawnVertical.Y2 = m_DrawHelper.pt_CanvasPt.Y + size;

            m_DrawHelper.DrawnHorizon.X1 = m_DrawHelper.pt_CanvasPt.X - size;
            m_DrawHelper.DrawnHorizon.Y1 = m_DrawHelper.pt_CanvasPt.Y;
            m_DrawHelper.DrawnHorizon.X2 = m_DrawHelper.pt_CanvasPt.X + size;
            m_DrawHelper.DrawnHorizon.Y2 = m_DrawHelper.pt_CanvasPt.Y;

            p_UIElement.Add(m_DrawHelper.DrawnHorizon);
            p_UIElement.Add(m_DrawHelper.DrawnVertical);          
                    }

        #endregion

        #region Command
        public ICommand CanvasMouseMove
            {
            get
            {
                return new RelayCommand(MouseMove);
            }
        }

        
        public ICommand CanvasMouseLeftDown
        {
            get
            {
                return new RelayCommand(MouseLeftDown);
            }
        }
        public ICommand CanvasMouseLeftUp
        {
            get
            {
                return new RelayCommand(MouseLeftUp);
            }
        }
        public ICommand CanvasMouseWheel
        {
            get
            {
                return new RelayCommand(RedrawUIElement);
            }
        }
        public ICommand btnClear
        {
            get
            {
                return new RelayCommand(_btnClear);
            }
        }
        public ICommand btnTabNone
        {
            get
            {
                return new RelayCommand(_SetModeNone);
            }
        }
        public ICommand btnDrawOrigin
        {
            get
        {
                return new RelayCommand(_btnDrawOrigin);
                }
                }
        #endregion

        #region CommandFunc
        private void _SetModeNone()
            {
            p_Mode = DrawingMode.None;
        }
        private void _btnDrawOrigin()
        {
        }
        private void _btnDrawStr()
        {
            if (p_Mode == DrawingMode.Str)
        {
                p_Mode = DrawingMode.None;
                RecipeCursor = System.Windows.Input.Cursors.Arrow;
            }
            else
            {

                p_Mode = DrawingMode.Str;
                RecipeCursor = System.Windows.Input.Cursors.Hand;
            }
        }
        private void _btnDrawLine()
        {
            if (_Mode == DrawingMode.Ruler)
            {
                _Mode = DrawingMode.None;
                RecipeCursor = System.Windows.Input.Cursors.Arrow;
            }
            else
            {
                _Mode = DrawingMode.Ruler;
                RecipeCursor = System.Windows.Input.Cursors.Cross;
            }
        }
        private void _btnDrawPt()
        {
            if (_Mode == DrawingMode.Pt)
            {
                _Mode = DrawingMode.None;
                RecipeCursor = System.Windows.Input.Cursors.Arrow;
            }
            else
            {
                _Mode = DrawingMode.Pt;
                RecipeCursor = System.Windows.Input.Cursors.Cross;
            }
        }
        private void _btnClear()
        {
            p_UIElement.Clear();
            m_DD = new DrawData();
            m_DrawHelper = new DrawHelper();
        }
        #endregion
    }
    public enum DrawingMode
        {
        None,
        Rectangle,
        Str,
        Pt,
        Ruler
        }
    public class DrawHelper
    {
        public Line DrawnHorizon;
        public Line DrawnVertical;
        public CPoint pt_MemoryPt;
        public CPoint pt_CanvasPt;
  
        public Line DrawnLine;
        public CPoint Line_Memory_StartPt;
        public CPoint Line_Memory_EndPt;
        public CPoint Line_Canvas_StartPt;
        public CPoint Line_Canvas_EndPt;

        public TextBlock TbOrigin;
        public CPoint Str_MemoryPt;
        public CPoint Str_CanvasPt;

        public CRect RectOrigin;
        public System.Windows.Shapes.Rectangle DrawnRect;
        public CPoint RectMemory_StartPt;
        public CPoint RectMemory_EndPt;
        public CPoint RectCanvas_StartPt;
        public CPoint RectCanvas_EndPt;
    }

}

namespace ModeConverter
{
    using Root_Wind;

    public class ModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DrawingMode mode = (DrawingMode)value;
            return (int)mode;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int nValue = (int)value;
            DrawingMode mode = (DrawingMode)nValue;
            return mode;
        }
    }
}

