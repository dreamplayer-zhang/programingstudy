using Root_VEGA_P_Vision.Engineer;
using RootTools;
using RootTools.Memory;
using RootTools_Vision;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Root_VEGA_P_Vision
{
    public class RecipeWizard_VM : RootViewer_ViewModel
    {
        public Page_RecipeWizard RecipeWizard_UI;

        ToolProcess m_eToolProcess;
        ToolType m_eToolType;
        ThresholdMode m_eThresholdMode;
        Color m_Color = Colors.Red;

        Stack<Work> m_History;
        Work m_CurrentWork;
        TShape m_CurrentShape;

        public RecipeWizard_VM(Setup_ViewModel setup)
        {
            RecipeWizard_UI = new Page_RecipeWizard();

            MemoryTool Memory = GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>().ClassMemoryTool();
            p_ImageData = new ImageData(Memory.GetMemory("Vision.Memory", "Vision", "EIP_Plate.Main.Front"));
            p_ROILayer = new ImageData(Memory.GetMemory("Vision.Memory", "Vision", "EIP_Plate.Main.Back"));
            base.init(p_ImageData);


            m_History = new Stack<Work>();
            m_UIElements = new ObservableCollection<UIElement>();

            Init_PenCursor();
        }

        #region Override
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            if(m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;

            CPoint memPt = new CPoint(p_MouseMemX, p_MouseMemY);

            if (m_eToolType != ToolType.None)
            {
                m_CurrentWork = new Work(m_eToolType);
            }

            switch (m_eToolProcess)
            {
                case ToolProcess.None:

                    switch (m_eToolType)
                    {
                        case ToolType.None:
                            break;
                        case ToolType.Pen:
                            m_CurrentWork.Points.Add(memPt);
                            Pen(memPt, _nThickness);
                            m_eToolProcess = ToolProcess.Drawing;
                            break;
                        case ToolType.Eraser:
                            m_CurrentWork.Points.Add(memPt);
                            Eraser(memPt, _nThickness);
                            m_eToolProcess = ToolProcess.Drawing;
                            break;
                        case ToolType.Rect:
                            Start_Rect(memPt);
                            Debug.WriteLine("Start Draw Rect@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                            m_eToolProcess = ToolProcess.Drawing;
                            break;
                        case ToolType.Circle:
                            break;
                        case ToolType.Crop:
                            break;
                        case ToolType.Threshold:
                            Start_Rect(memPt);
                            Debug.WriteLine("Start Draw Threshold@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                            m_eToolProcess = ToolProcess.Drawing;
                            break;
                    }
                    break;
                case ToolProcess.Drawing:
                    switch (m_eToolType)
                    {
                        case ToolType.None:
                            break;
                        case ToolType.Pen:
                            m_CurrentWork.Points.Add(memPt);
                            Pen(memPt, _nThickness);
                            break;
                        case ToolType.Eraser:
                            m_CurrentWork.Points.Add(memPt);
                            Eraser(memPt, _nThickness);
                            break;
                        case ToolType.Rect:
                            break;
                        case ToolType.Circle:
                            break;
                        case ToolType.Crop:
                            break;
                        case ToolType.Threshold:
                            break;
                    }
                    break;
                case ToolProcess.Modifying:
                    break;
                default:
                    break;
            }
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                {
                    PenCursor.Visibility = Visibility.Collapsed;
                    return;
                }

            CPoint memPt = new CPoint(p_MouseMemX, p_MouseMemY);

            switch (m_eToolProcess)
            {
                case ToolProcess.None:
                    if (m_eToolType == ToolType.Pen || m_eToolType == ToolType.Eraser)
                    {
                        PenCursor.Visibility = Visibility.Visible;
                        Draw_PenCursor();
                    }
                    break;
                case ToolProcess.Drawing:
                    switch (m_eToolType)
                    {
                        case ToolType.None:
                            break;
                        case ToolType.Pen:
                            Draw_PenCursor();
                            if (e.LeftButton == MouseButtonState.Pressed)
                            {                             
                                m_CurrentWork.Points.Add(memPt);
                                Pen(memPt, _nThickness);
                            }
                            break;
                        case ToolType.Eraser:
                            Draw_PenCursor();
                            if (e.LeftButton == MouseButtonState.Pressed)
                            {
                                m_CurrentWork.Points.Add(memPt);
                                Eraser(memPt, _nThickness);
                            }
                            break;
                        case ToolType.Rect:
                            if (e.LeftButton == MouseButtonState.Pressed)
                            {
                                Drawing_Rect(memPt);
                            }
                            break;
                        case ToolType.Circle:
                            break;
                        case ToolType.Crop:
                            break;
                        case ToolType.Threshold:
                            if (e.LeftButton == MouseButtonState.Pressed)
                            {
                                Drawing_Rect(memPt);
                            }
                            break;
                    }
                    break;
                case ToolProcess.Modifying:
                    break;
                default:
                    break;
            }

        }
        public override void PreviewMouseUp(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("Mouse Up");
            switch (m_eToolProcess)
            {
                case ToolProcess.None:
                    break;
                case ToolProcess.Drawing:
                    switch (m_eToolType)
                    {
                        case ToolType.None:
                            break;
                        case ToolType.Pen:
                            m_eToolProcess = ToolProcess.None;
                            break;
                        case ToolType.Eraser:
                            m_eToolProcess = ToolProcess.None;
                            break;
                        case ToolType.Rect:
                            Done_Rect();
                            Debug.WriteLine("Done Rect");
                            m_eToolProcess = ToolProcess.None;
                            break;
                        case ToolType.Circle:
                            break;
                        case ToolType.Crop:
                            break;
                        case ToolType.Threshold:
                            Done_Threshold();
                            Debug.WriteLine("Done Threshold");
                            m_eToolProcess = ToolProcess.None;
                            break;
                        default:
                            break;
                    }
                    m_History.Push(m_CurrentWork);
                    break;
                case ToolProcess.Modifying:
                    break;
                default:
                    break;
            }
        }
        public override void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.MouseWheel(sender, e);
            if (m_eToolType == ToolType.Pen || m_eToolType == ToolType.Eraser)
                Draw_PenCursor();
        }
        #endregion

        #region Tool
        void UncheckTool(ToolType type)
        {
            PenCursor.Visibility = Visibility.Collapsed;
            switch (type)
            {
                case ToolType.Pen:
                    PenCursor.Visibility = Visibility.Visible;
                    bEraserCheck = false;
                    bRectCheck = false;
                    bCircleCheck = false;
                    bCropCheck = false;
                    bThresholdCheck = false;
                    break;
                case ToolType.Eraser:
                    PenCursor.Visibility = Visibility.Visible;
                    bPenCheck = false;
                    bRectCheck = false;
                    bCircleCheck = false;
                    bCropCheck = false;
                    bThresholdCheck = false;
                    break;
                case ToolType.Rect:
                    bPenCheck = false;
                    bEraserCheck = false;
                    bCircleCheck = false;
                    bCropCheck = false;
                    bThresholdCheck = false;
                    break;
                case ToolType.Circle:
                    bPenCheck = false;
                    bEraserCheck = false;
                    bRectCheck = false;
                    bCropCheck = false;
                    bThresholdCheck = false;
                    break;
                case ToolType.Crop:
                    bPenCheck = false;
                    bEraserCheck = false;
                    bRectCheck = false;
                    bCircleCheck = false;
                    bThresholdCheck = false;
                    break;
                case ToolType.Threshold:
                    bPenCheck = false;
                    bEraserCheck = false;
                    bRectCheck = false;
                    bCircleCheck = false;
                    bCropCheck = false;
                    break;
            }


        }
        private void Pen(CPoint cPt, int size)
        {        
            Parallel.For(cPt.X, cPt.X + size, i =>
            {
                for (int j = cPt.Y; j < cPt.Y+size; j++)
                {
                    CPoint pixelPt = new CPoint(i, j);
                    DrawPixelBitmap(pixelPt, m_Color.R, m_Color.G, m_Color.B, m_Color.A);
                }
            });
            SetLayerSource();
        }
        private void Eraser(CPoint cPt, int size)
        {
            Parallel.For(cPt.X, cPt.X + size, i =>
            {
                for (int j = cPt.Y; j < cPt.Y + size; j++)
                {
                    CPoint pixelPt = new CPoint(i, j);
                    DrawPixelBitmap(pixelPt, 0, 0, 0, 0);
                }
            });
            SetLayerSource();
        }
        private void Start_Rect(CPoint cPt)
        {
            Brush brush = new SolidColorBrush(m_Color);
            m_CurrentShape = new TRect(brush, 2, 0.5);
            TRect rect = m_CurrentShape as TRect;

            rect.MemPointBuffer = cPt;
            rect.MemoryRect.Left = cPt.X;
            rect.MemoryRect.Top = cPt.Y;

            p_UIElement.Add(rect.CanvasRect);
        }
        private void Drawing_Rect(CPoint cPt)
        {
            TRect rect = m_CurrentShape as TRect;

            if (rect.MemPointBuffer.X > cPt.X)
            {
                rect.MemoryRect.Left = cPt.X;
                rect.MemoryRect.Right = rect.MemPointBuffer.X;
            }
            else
            {
                rect.MemoryRect.Left = rect.MemPointBuffer.X;
                rect.MemoryRect.Right = cPt.X;
            }
            if (rect.MemPointBuffer.Y > cPt.Y)
            {
                rect.MemoryRect.Top = cPt.Y;
                rect.MemoryRect.Bottom = rect.MemPointBuffer.Y;
            }
            else
            {
                rect.MemoryRect.Top = rect.MemPointBuffer.Y;
                rect.MemoryRect.Bottom = cPt.Y;
            }

            double pixSizeX = (double)p_CanvasWidth / (double)p_View_Rect.Width;
            double pixSizeY = (double)p_CanvasHeight / (double)p_View_Rect.Height;

            CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
            CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);
            Point canvasLT = new Point(GetCanvasDoublePoint(LT).X, GetCanvasDoublePoint(LT).Y);
            Point canvasRB = new Point(GetCanvasDoublePoint(RB).X, GetCanvasDoublePoint(RB).Y);

            Canvas.SetLeft(rect.CanvasRect, canvasLT.X - pixSizeX / 2);
            Canvas.SetTop(rect.CanvasRect, canvasLT.Y - pixSizeY / 2);

            rect.CanvasRect.Width = Math.Abs(canvasRB.X - canvasLT.X + pixSizeX);
            rect.CanvasRect.Height = Math.Abs(canvasRB.Y - canvasLT.Y + pixSizeY);
        }
        private void Done_Rect()
        {
            TRect rect = m_CurrentShape as TRect;
            rect.MemoryRect.Left = rect.MemoryRect.Left;
            rect.MemoryRect.Right = rect.MemoryRect.Right;
            rect.MemoryRect.Top = rect.MemoryRect.Top;
            rect.MemoryRect.Bottom = rect.MemoryRect.Bottom;

            Parallel.For(rect.MemoryRect.Top, rect.MemoryRect.Bottom + 1, y =>
              {
                  for (int x = rect.MemoryRect.Left; x <= rect.MemoryRect.Right; x++)
                  {
                      CPoint pixelPt = new CPoint(x, y);
                      m_CurrentWork.Points.Add(pixelPt);
                      DrawPixelBitmap(pixelPt, m_Color.R, m_Color.G, m_Color.B, m_Color.A);
                  }
              });
            SetLayerSource();
            p_UIElement.Remove(rect.CanvasRect);
        }
        private unsafe void Done_Threshold()
        {
            TRect rect = m_CurrentShape as TRect;

            CRect ThresholdRect = new CRect(rect.MemoryRect.Left, rect.MemoryRect.Top, rect.MemoryRect.Right + 1, rect.MemoryRect.Bottom + 1);
            IntPtr ptr = p_ImageData.GetPtr();
            for (int i = ThresholdRect.Height - 1; i >= 0; i--)
            {
                byte* gv = (byte*)((IntPtr)((long)ptr + ThresholdRect.Left * p_ImageData.p_nByte + ((long)i + ThresholdRect.Top) * p_ImageData.p_Stride));
                for (int j = 0; j < ThresholdRect.Width; j++)
                {

                    if (m_eThresholdMode == ThresholdMode.Down)
                    {
                        if (gv[j] < _nThreshold)
                        {
                            CPoint pixelPt = new CPoint(ThresholdRect.Left + j, ThresholdRect.Top + i);
                            m_CurrentWork.Points.Add(pixelPt);
                            DrawPixelBitmap(pixelPt, m_Color.R, m_Color.G, m_Color.B, m_Color.A);
                        }
                    }
                    else if (m_eThresholdMode == ThresholdMode.Up)
                    {
                        if (gv[j] > _nThreshold)
                        {
                            CPoint pixelPt = new CPoint(ThresholdRect.Left + j, ThresholdRect.Top + i);
                            m_CurrentWork.Points.Add(pixelPt);
                            DrawPixelBitmap(pixelPt, m_Color.R, m_Color.G, m_Color.B, m_Color.A);
                        }
                    }
                }
            }
            SetLayerSource();

            //ImageData rectImage = new ImageData(ThresholdRect.Width, ThresholdRect.Height, p_ImageData.p_nByte);
            //rectImage.SetData(p_ImageData, ThresholdRect, (int)p_ImageData.p_Stride, p_ImageData.GetBytePerPixel());
            //var sdf = rectImage.m_aBuf;
            //rect.Left* nByte +((long)i + rect.Top) * stride)
            //for (int i = 0; i < rectImage.m_aBuf.Length; i++)
            //{
            //    if (rectImage.m_aBuf[i] < 50)
            //    {
            //        //0,0 /0,1 /1,0 /1,1
            //        //시작점
            //        var asdf = (ThresholdRect.Top * p_ROILayer.p_Size.X + ThresholdRect.Left);
            //    }
            //}


            //Parallel.For(rect.MemoryRect.Top, rect.MemoryRect.Bottom + 1, y =>
            //{
            //    for (int x = rect.MemoryRect.Left; x <= rect.MemoryRect.Right; x++)
            //    {
            //        CPoint pixelPt = new CPoint(x, y);


            //        m_CurrentWork.Points.Add(pixelPt);
            //       
            //    }
            //});

            p_UIElement.Remove(rect.CanvasRect);
        }

        private void Start_Circle(CPoint cPt)
        {

        }
        #endregion

        #region Property
        public ObservableCollection<UIElement> p_UIElements
        {
            get
            {
                return m_UIElements;
            }
            set
            {
                m_UIElements = value;
            }
        }
        private ObservableCollection<UIElement> m_UIElements;

        public int p_nThickness
        {
            get
            {
                return _nThickness;
            }
            set
            {
                SetProperty(ref _nThickness, value);
            }
        }
        private int _nThickness = 5;

        public int p_nThreshold
        {
            get
            {
                return _nThreshold;
            }
            set
            {                
                SetProperty(ref _nThreshold, value);
            }
        }
        private int _nThreshold = 50;

        public int p_nThresholdMode
        {
            get
            {
                return _nThresholdMode;
            }
            set
            {
                m_eThresholdMode = (ThresholdMode)value;
                SetProperty(ref _nThresholdMode, value);
            }
        }
        private int _nThresholdMode = 0;

        public bool bPenCheck
        {
            get
            {
                return _bPenCheck;
            }
            set
            {
                if (value)
                {
                    m_eToolType = ToolType.Pen;
                    UncheckTool(m_eToolType);
                }
                SetProperty(ref _bPenCheck, value);
            }
        }
        private bool _bPenCheck;

        public bool bEraserCheck
        {
            get
            {
                return _bEraserCheck;
            }
            set
            {
                if (value)
                {
                    m_eToolType = ToolType.Eraser;
                    UncheckTool(m_eToolType);
                }
                SetProperty(ref _bEraserCheck, value);
            }
        }
        private bool _bEraserCheck;

        public bool bRectCheck
        {
            get
            {
                return _bRectCheck;
            }
            set
            {
                if (value)
                {
                    m_eToolType = ToolType.Rect;
                    UncheckTool(m_eToolType);
                }
                SetProperty(ref _bRectCheck, value);
            }
        }
        private bool _bRectCheck;

        public bool bCircleCheck
        {
            get
            {
                return _bCircleCheck;
            }
            set
            {
                if (value)
                {
                    m_eToolType = ToolType.Circle;
                    UncheckTool(m_eToolType);
                }
                SetProperty(ref _bCircleCheck, value);
            }
        }
        private bool _bCircleCheck; 

        public bool bCropCheck
        {
            get
            {
                return _bCropCheck;
            }
            set
            {
                if (value)
                {
                    m_eToolType = ToolType.Crop;
                    UncheckTool(m_eToolType);
                }
                SetProperty(ref _bCropCheck, value);
            }
        }
        private bool _bCropCheck;

        public bool bThresholdCheck
        {
            get
            {
                return _bThresholdCheck;
            }
            set
            {
                if (value)
                {
                    m_eToolType = ToolType.Threshold;
                    UncheckTool(m_eToolType);
                }
                SetProperty(ref _bThresholdCheck, value);
            }
        }
        private bool _bThresholdCheck;

        

        #endregion

        #region Pen/Eraser Cursor
        Rectangle PenCursor;
        void Init_PenCursor()
        {
            if (p_ViewElement.Contains(PenCursor))
                p_ViewElement.Remove(PenCursor);

            PenCursor = new Rectangle();
            PenCursor.Opacity = 0.5;
            PenCursor.Fill = Brushes.LightCoral;
            PenCursor.Stroke = Brushes.LightCoral;
            PenCursor.StrokeThickness = 1;
            PenCursor.StrokeDashArray = new DoubleCollection{3, 4};

            p_ViewElement.Add(PenCursor);

        }
        void Draw_PenCursor()
        {
            if (PenCursor == null)
                return;

            double pixSizeX = (double)p_CanvasWidth / (double)p_View_Rect.Width;
            double pixSizeY = (double)p_CanvasHeight / (double)p_View_Rect.Height;
            PenCursor.Width = pixSizeX * _nThickness;
            PenCursor.Height = pixSizeY * _nThickness;
            Point newPt = GetCanvasDoublePoint(new CPoint(p_MouseMemX, p_MouseMemY));
            Canvas.SetLeft(PenCursor, newPt.X-pixSizeX/2);
            Canvas.SetTop(PenCursor, newPt.Y-pixSizeY/2);

        }
        #endregion

        #region ICommand
        public ICommand cmdClear
        {
            get
            {
                return new RelayCommand(()=> 
                {
                    IntPtr ptrMem = p_ROILayer.GetPtr();
                    if (ptrMem == IntPtr.Zero)
                        return;

                    byte[] buf = new byte[p_ROILayer.p_Size.X * p_ROILayer.GetBytePerPixel()];
                    for (int i = 0; i < p_ROILayer.p_Size.Y; i++)
                    {
                        Marshal.Copy(buf, 0, (IntPtr)((long)ptrMem + (long)p_ROILayer.p_Size.X * p_ROILayer.GetBytePerPixel() * i), buf.Length);
                    }
                    SetLayerSource();
                });
            }
        }
        #endregion
    }
    class Work
    {
        public ToolType eToolType;
        public BlockingCollection<CPoint> Points = new BlockingCollection<CPoint>();
        public BlockingCollection<PointLine> Datas = new BlockingCollection<PointLine>();

        public Work(ToolType type)
        {
            eToolType = type;
        }

        public object GetPoints()
        {
            return Points;
        }
        public object GetDatas()
        {
            return Datas;
        }
    }

    public enum ThresholdMode
    {
        Down,
        Up,
    }
    public enum ToolProcess
        {
            None,
            Drawing,
            Modifying,
        }
    public enum ToolType
        {
            None,
            Pen,
            Eraser,
            Rect,
            Circle,
            Crop,
            Threshold,
        }

}
