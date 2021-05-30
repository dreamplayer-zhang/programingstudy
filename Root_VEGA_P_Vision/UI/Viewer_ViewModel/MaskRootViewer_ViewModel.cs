using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Root_VEGA_P_Vision.Engineer;
using RootTools;
using RootTools.Database;
using RootTools.Memory;
using RootTools_Vision;
using RootTools_Vision.WorkManager3;

namespace Root_VEGA_P_Vision
{
    public class MaskRootViewer_ViewModel : BaseViewer_ViewModel
    {
        #region [ColorDefines]
        private class ImageViewerColorDefines
        {
            public static SolidColorBrush MasterPosition = Brushes.Magenta;
            public static SolidColorBrush MasterPostionMove = Brushes.Yellow;
            public static SolidColorBrush ChipPosition = Brushes.Blue;
            public static SolidColorBrush ChipPositionMove = Brushes.Yellow;
            public static SolidColorBrush PostionFail = Brushes.Red;
            public static SolidColorBrush Defect = Brushes.Red;
        }

        private class MapViewerColorDefines
        {
            public static SolidColorBrush NoChip = Brushes.LightGray;
            public static SolidColorBrush Normal = Brushes.DimGray;
            public static SolidColorBrush Snap = Brushes.LightSkyBlue;
            public static SolidColorBrush Position = Brushes.LightYellow;
            public static SolidColorBrush Inspection = Brushes.Gold;
            public static SolidColorBrush ProcessDefect = Brushes.YellowGreen;
            public static SolidColorBrush ProcessDefectWafer = Brushes.Green;

            public static SolidColorBrush GetWorkplaceStateColor(WORK_TYPE state)
            {
                switch (state)
                {
                    case WORK_TYPE.NONE:
                        return Normal;
                    case WORK_TYPE.SNAP:
                        return Snap;
                    case WORK_TYPE.ALIGNMENT:
                        return Position;
                    case WORK_TYPE.INSPECTION:
                        return Inspection;
                    case WORK_TYPE.DEFECTPROCESS:
                        return ProcessDefect;
                    case WORK_TYPE.DEFECTPROCESS_ALL:
                        return ProcessDefectWafer;
                    default:
                        return Normal;
                }
            }
        }
        #endregion


        ToolProcess m_eToolProcess;
        public ToolType m_eToolType;
        ThresholdMode m_eThresholdMode;
        System.Windows.Media.Color m_Color = Colors.Navy;
        Color m_EraseColor = Color.FromArgb(0,0,0,0);

        Stack<Work> m_History;
        Work m_CurrentWork;
        TShape m_CurrentShape;
        MaskTools_ViewModel recipeSetting;
        ItemMask m_cInspROI;

        public ItemMask p_cInspROI
        {
            get => m_cInspROI;
            set => SetProperty(ref m_cInspROI, value);
        }

        public MaskRootViewer_ViewModel(string imagedata, MaskTools_ViewModel recipeSetting) : base(imagedata)
        {
            p_VisibleMenu = Visibility.Collapsed;
            this.recipeSetting = recipeSetting;

            m_cInspROI = new ItemMask();
            m_History = new Stack<Work>();

            Init_PenCursor();
            InitInspMgr();
        }

        #region Override

        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;

            CPoint memPt = new CPoint(p_MouseMemX, p_MouseMemY);


            if (m_eToolType != ToolType.None)
            {
                m_CurrentWork = new Work(m_eToolType);
            }

            bool IsDraw = recipeSetting.IsDraw;

            switch (m_eToolProcess)
            {

                case ToolProcess.None:

                    switch (m_eToolType)
                    {
                        case ToolType.None:
                            break;
                        case ToolType.Pen:
                            m_CurrentWork.Points.Add(memPt);
                            //if (m_timer.IsBusy == false) 
                            //    m_timer.RunWorkerAsync();
                            if(IsDraw)
                                Pen(memPt, _nThickness);
                            else
                                Eraser(memPt, _nThickness);

                            m_eToolProcess = ToolProcess.Drawing;
                            break;
                        case ToolType.Eraser:
                            m_CurrentWork.Points.Add(memPt);
                            Eraser(memPt, _nThickness);
                            m_eToolProcess = ToolProcess.Drawing;
                            break;
                        case ToolType.Rect:
                            if (IsDraw)
                                Start_Rect(memPt);
                            else
                                Erase_Rect(memPt);

                            m_eToolProcess = ToolProcess.Drawing;
                            break;
                        case ToolType.Circle:
                            break;
                        case ToolType.Crop:
                            break;
                        case ToolType.Threshold:
                            Start_Rect(memPt);
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

            m_eToolType = recipeSetting.m_eToolType;
            p_nThickness = recipeSetting.p_nThickness;
            PenCursor.Visibility = Visibility.Collapsed;

            bool IsDraw = recipeSetting.IsDraw;

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
                            PenCursor.Visibility = Visibility.Visible;
                            Draw_PenCursor();
                            if (e.LeftButton == MouseButtonState.Pressed)
                            {
                                m_CurrentWork.Points.Add(memPt);
                                if (IsDraw)
                                    Pen(memPt, _nThickness);
                                else
                                    Eraser(memPt, _nThickness);
                            }
                            break;
                        case ToolType.Eraser:
                            PenCursor.Visibility = Visibility.Visible;

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
                            if (recipeSetting.IsDraw)
                                Done_Rect();
                            else
                                Done_EraseRect();
                            m_eToolProcess = ToolProcess.None;
                            break;
                        case ToolType.Circle:
                            break;
                        case ToolType.Crop:
                            break;
                        case ToolType.Threshold:
                            m_eThresholdMode = (ThresholdMode)recipeSetting.p_nselectedUpdown;
                            p_nThreshold = recipeSetting.p_nThreshold;
                            Done_Threshold();
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

        Queue<CPoint> pointsq = new Queue<CPoint>();

        #region Tool
        private unsafe void Pen(CPoint cPt, int size)
        {
            IntPtr ptrMem = p_ROILayer.GetPtr();
            if (ptrMem == IntPtr.Zero)
                return;
            byte* imagePtr = (byte*)ptrMem.ToPointer();
            uint* fPtr = (uint*)imagePtr;

            //pointsq.Enqueue(cPt);

            Parallel.For(cPt.X, cPt.X + size, i =>
            {
                for (int j = cPt.Y; j < cPt.Y + size; j++)
                {
                    CPoint pixelPt = new CPoint(i, j);

                    int pos = (j * p_ROILayer.p_Size.X + i) /** 4*/;
                    uint clr = m_Color.A;
                    clr = clr << 8;
                    clr += m_Color.R;
                    clr = clr << 8;
                    clr += m_Color.G;
                    clr = clr << 8;
                    clr += m_Color.B;
                    fPtr[pos] = clr;
                }
            });
            //SetLayerSource();
            SetMaskLayerSource();
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
        private void Erase_Rect(CPoint cPt)
        {
            Brush brush = new SolidColorBrush(m_EraseColor);
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
            SetMaskLayerSource();
            //SetLayerSource();
            m_History.Push(m_CurrentWork);
            p_UIElement.Remove(rect.CanvasRect);
        }
        private void Done_EraseRect()
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
                    DrawPixelBitmap(pixelPt, m_EraseColor.R, m_EraseColor.G, m_EraseColor.B, m_EraseColor.A);
                }
            });
            SetMaskLayerSource();

            //SetLayerSource();
            m_History.Push(m_CurrentWork);
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
            //SetLayerSource();
            SetMaskLayerSource();



            p_UIElement.Remove(rect.CanvasRect);
        }

        #endregion

        #region Property

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
            PenCursor.StrokeDashArray = new DoubleCollection { 3, 4 };
            p_ViewElement.Add(PenCursor);

        }
        void Draw_PenCursor()
        {
            if (PenCursor == null)
                return;

            double pixSizeX = p_CanvasWidth / (double)p_View_Rect.Width;
            double pixSizeY = p_CanvasHeight / (double)p_View_Rect.Height;
            PenCursor.Width = pixSizeX * _nThickness;
            PenCursor.Height = pixSizeY * _nThickness;
            Point newPt = GetCanvasDoublePoint(new CPoint(p_MouseMemX, p_MouseMemY));
            Canvas.SetLeft(PenCursor, newPt.X - pixSizeX / 2);
            Canvas.SetTop(PenCursor, newPt.Y - pixSizeY / 2);

        }
        #endregion

        #region Inspection
        void InitInspMgr()
        {

            GlobalObjects.Instance.GetNamed<WorkManager>(TabName).InspectionStart += InspectionStart_Callback;
            GlobalObjects.Instance.GetNamed<WorkManager>(TabName).PositionDone += PositionDone_Callback;
            GlobalObjects.Instance.GetNamed<WorkManager>(TabName).InspectionDone += InspectionDone_Callback;
            GlobalObjects.Instance.GetNamed<WorkManager>(TabName).ProcessDefectWaferStart += ProcessDefectWaferStart_Callback;
            GlobalObjects.Instance.GetNamed<WorkManager>(TabName).IntegratedProcessDefectDone += ProcessDefectWaferDone_Callback;


        }

        private void ProcessDefectWaferDone_Callback(object sender, IntegratedProcessDefectDoneEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                //DatabaseManager.Instance.SelectData();
                //m_DataViewer_VM.pDataTable = DatabaseManager.Instance.pDefectTable;
            }));
        }

        private void ProcessDefectWaferStart_Callback(object sender, ProcessDefectWaferStartEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                RemoveObjectsByTag("defect");
            }));
        }

        private void InspectionDone_Callback(object sender, InspectionDoneEventArgs args)
        {
            Workplace workplace = sender as Workplace;
            if (workplace == null || workplace.DefectList == null) return;
            List<string> textList = new List<string>();
            List<CRect> rectList = new List<CRect>();


            foreach (Defect defectInfo in workplace.DefectList)
            {
                //string text = "X : " + defectInfo.m_fAbsX + ", Y: " + defectInfo.m_fAbsY + "\n" + "Size : " + defectInfo.m_fSize + "pxl";
                string text = "";
                rectList.Add(new CRect((int)defectInfo.p_rtDefectBox.Left, (int)defectInfo.p_rtDefectBox.Top, (int)defectInfo.p_rtDefectBox.Right, (int)defectInfo.p_rtDefectBox.Bottom));
                textList.Add(text);
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                DrawRectDefect(rectList, textList);
            }));
        }

        private void PositionDone_Callback(object obj, PositionDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                string test = "";

                test += "Trans : {" + workplace.OffsetX.ToString() + ", " + workplace.OffsetX.ToString() + "}" + "\n";
                DrawRectMasterFeature(args.ptOldStart, args.ptOldEnd, args.ptNewStart, args.ptNewEnd, test, args.bSuccess);

            }));
        }

        private void InspectionStart_Callback(object sender, InspectionStartArgs e)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                Clear();
                ClearObjects();
            }));

        }
        public void Clear()
        {
            Shapes.Clear();
            InfoTextBolcks.Clear();
            p_DrawElement.Clear();
        }
        public void DrawRectMasterFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text, bool bSuccess)
        {
            AddDrawRect(ptOldStart, ptOldEnd, ImageViewerColorDefines.MasterPosition, "position");
            AddDrawRect(ptNewStart, ptNewEnd, bSuccess ? ImageViewerColorDefines.MasterPostionMove : ImageViewerColorDefines.PostionFail, "position");
            //ImageViewerVM.DrawText(ptNew)
        }


        private string currentRecipe = "";
        private MapViewer_ViewModel mapViewerVM;
        public MapViewer_ViewModel MapViewerVM
        {
            get => mapViewerVM;
            set
            {
                SetProperty(ref mapViewerVM, value);
            }
        }
        private Database_DataView_VM m_DataViewer_VM = new Database_DataView_VM();
        public Database_DataView_VM p_DataViewer_VM
        {
            get { return this.m_DataViewer_VM; }
            set { SetProperty(ref m_DataViewer_VM, value); }
        }
        public void LoadRecipe()
        {
            if (currentRecipe != GlobalObjects.Instance.Get<RecipeVision>().Name)
                currentRecipe = GlobalObjects.Instance.Get<RecipeVision>().Name;
        }

        public void DrawRectDefect(List<CRect> rectList, List<string> text)
        {
            DrawRect(rectList, ColorType.Defect, text);
        }

        public void Inspection()
        {
            if (GlobalObjects.Instance.GetNamed<WorkManager>(TabName) != null)
            {
                p_ImageData.GetPtr(SelectedIdx);
                EUVOriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeVision>().GetItem<EUVOriginRecipe>();
                OriginRecipe insporigin = GlobalObjects.Instance.Get<RecipeVision>().GetItem<OriginRecipe>();
                insporigin.OriginX = originRecipe.StainOrigin.Origin.X;
                insporigin.OriginY = originRecipe.StainOrigin.Origin.Y;
                insporigin.OriginWidth = originRecipe.StainOrigin.OriginSize.X;
                insporigin.OriginHeight = originRecipe.StainOrigin.OriginSize.Y;

                SetMaskInspROI();

                GlobalObjects.Instance.GetNamed<WorkManager>(TabName).Start();
            }
        }
        unsafe void SetMaskInspROI()
        {
            EUVOriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeVision>().GetItem<EUVOriginRecipe>();
            int originWidth = originRecipe.StainOrigin.OriginSize.X;
            int originHeight = originRecipe.StainOrigin.OriginSize.Y;

            IntPtr ptrMem = p_ROILayer.GetPtr(SelectedIdx);
            if (ptrMem == IntPtr.Zero)
                return;
            byte* bitmapPtr = (byte*)ptrMem.ToPointer();

            List<PointLine> tempPointLines = new List<PointLine>();
            bool bStart = false;
            CPoint size = p_ImageData.p_Size;
            PointLine pointLine = null;
            for (int j = 0; j < originHeight; j++)
            {
                for (int i = 0; i < originWidth; i++)
                {
                    byte a = bitmapPtr[(j * p_ROILayer.p_Size.X + i) * 4 + 3];
                    if (a > 0)
                    {
                        if (!bStart)
                        {
                            pointLine = new PointLine();
                            pointLine.StartPt = new CPoint(i, j);
                            bStart = true;
                        }
                    }
                    if (a == 0 || i == originWidth - 1)
                    {
                        if (bStart)
                        {
                            pointLine.Width = i - pointLine.StartPt.X + 1;
                            bStart = false;
                            tempPointLines.Add(pointLine);
                        }
                    }
                }
            }
            p_cInspROI.p_Data = tempPointLines;
            SetRecipeData();
        }
        public void SetRecipeData()
        {
            //Stain은 Origin Recipe 영역을 전부다 쓸꺼임
            RecipeVision recipe = GlobalObjects.Instance.Get<RecipeVision>();
            EUVOriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeVision>().GetItem<EUVOriginRecipe>();

            recipe.GetItem<MaskRecipe>().OriginPoint = new CPoint(originRecipe.StainOrigin.OriginSize.X, originRecipe.StainOrigin.OriginSize.Y);
            if (recipe.GetItem<MaskRecipe>().MaskList.Count > 0)
                recipe.GetItem<MaskRecipe>().MaskList.Clear();

            recipe.GetItem<MaskRecipe>().MaskList.Add(new RecipeType_Mask(p_cInspROI.p_Data, Color.FromArgb(255, 255, 0, 0)));
        }

        public override void SetRoiRect()
        {
            Shapes.CollectionChanged += Shapes_CollectionChanged;
            InfoTextBolcks.CollectionChanged += Texts_CollectionChanged;

            base.SetRoiRect();
            RedrawShapes();
            ReWriteText();

            Shapes.CollectionChanged -= Shapes_CollectionChanged;
            InfoTextBolcks.CollectionChanged -= Texts_CollectionChanged;
        }
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            Shapes.CollectionChanged += Shapes_CollectionChanged;
            InfoTextBolcks.CollectionChanged += Texts_CollectionChanged;

            base.CanvasMovePoint_Ref(point, nX, nY);
            RedrawShapes();
            ReWriteText();

            Shapes.CollectionChanged -= Shapes_CollectionChanged;
            InfoTextBolcks.CollectionChanged -= Texts_CollectionChanged;
        }
        static long time1 = 0;
        static StopWatch watch1 = new StopWatch();
        private void ReWriteText()
        {

            watch1.p_secTimeout = 1;
            if (watch1.IsRunning == true)
            {


                time1 += watch1.ElapsedMilliseconds;
                watch1.Restart();
            }
            else
            {
                watch1.Start();
            }
            if (time1 < 1000 / 20)
            {
                return;
            }
            else
            {
                time1 = 0;
            }


            foreach (InfoTextBolck info in InfoTextBolcks)
            {
                TRect rect = info.pos;
                if (!this.p_View_Rect.Contains(rect.MemoryRect.Left, rect.MemoryRect.Top) || !this.p_View_Rect.Contains(rect.MemoryRect.Right, rect.MemoryRect.Bottom))
                {

                    info.grid.Visibility = Visibility.Hidden;
                    continue;
                }
                else
                {
                    info.grid.Visibility = Visibility.Visible;
                }
                CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
                CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);

                CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
                CPoint canvasRB = new CPoint(GetCanvasPoint(RB));
                int width = Math.Abs(canvasRB.X - canvasLT.X);
                int height = Math.Abs(canvasRB.Y - canvasLT.Y);
                rect.CanvasRect.Width = width;
                rect.CanvasRect.Height = height;
                Canvas.SetLeft(info.grid, canvasLT.X);
                Canvas.SetTop(info.grid, canvasRB.Y);
            }
        }

        private void RedrawShapes()
        {
            // 개선중..
            //watch.p_secTimeout = 1;
            //if (watch.IsRunning == true)
            //{
            //    time += watch.ElapsedMilliseconds;
            //    watch.Restart();
            //}
            //else
            //{
            //    watch.Start();
            //}

            //if (time < 1000 / 20)
            //{
            //    return;
            //}
            //else
            //{
            //    time = 0;
            //}

            foreach (TShape shape in Shapes)
            {
                TRect rect = shape as TRect;
                if (!this.p_View_Rect.Contains(rect.MemoryRect.Left, rect.MemoryRect.Top) || !this.p_View_Rect.Contains(rect.MemoryRect.Right, rect.MemoryRect.Bottom))
                {
                    rect.CanvasRect.Visibility = Visibility.Hidden;
                    continue;
                }
                else
                {
                    rect.CanvasRect.Visibility = Visibility.Visible;
                }

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
        }

        private void Texts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var infoTexts = sender as ObservableCollection<InfoTextBolck>;
            foreach (InfoTextBolck text in infoTexts)
            {
                if (!p_DrawElement.Contains(text.grid))
                    p_DrawElement.Add(text.grid);
            }
        }
        private void Shapes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var shapes = sender as ObservableCollection<TShape>;
            foreach (TShape shape in shapes)
            {
                if (!p_DrawElement.Contains(shape.UIElement))
                    p_DrawElement.Add(shape.UIElement);
            }
        }
        public ObservableCollection<TShape> Shapes = new ObservableCollection<TShape>();
        public ObservableCollection<InfoTextBolck> InfoTextBolcks = new ObservableCollection<InfoTextBolck>();

        public void DrawRect(List<CRect> RectList, ColorType color, List<String> textList = null, int FontSz = 15)
        {
            int i = 0;
            foreach (CRect rectPoint in RectList)
            {
                SetShapeColor(color);
                TRect rect = rectInfo as TRect;

                rect.MemPointBuffer = new CPoint(rectPoint.Left, rectPoint.Top);
                rect.MemoryRect.Left = rectPoint.Left;
                rect.MemoryRect.Top = rectPoint.Top ;
                rectInfo = Drawing(rectInfo, new CPoint(rectPoint.Right, rectPoint.Bottom ));

                Shapes.Add(rectInfo);
                p_DrawElement.Add(rectInfo.UIElement);

                if (textList[i] != null)
                {
                    Grid textGrid = WriteInfoText(textList[i++], rect, color, FontSz);
                    InfoTextBolcks.Add(new InfoTextBolck(textGrid, rect));
                }
            }
        }
        public struct InfoTextBolck
        {
            public InfoTextBolck(Grid grid, TRect pos)
            {
                this.grid = grid;
                this.pos = pos;
            }
            public Grid grid;
            public TRect pos;
        }
        private System.Windows.Media.SolidColorBrush GetColorBrushType(ColorType color)
        {
            switch (color)
            {
                case ColorType.MasterFeature:
                    return Brushes.DarkMagenta;
                case ColorType.ChipFeature:
                    return Brushes.DarkBlue;
                case ColorType.FeatureMatching:
                    return Brushes.Gold;
                case ColorType.Defect:
                    return Brushes.Red;
                default:
                    return Brushes.Black;
            }
        }
        private Grid WriteInfoText(string text, TRect rect, ColorType color, int Fontsz)
        {
            Grid grid = new Grid();
            TextBlock tb = new TextBlock();

            CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
            CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);

            CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB));
            int width = Math.Abs(canvasRB.X - canvasLT.X);
            int height = Math.Abs(canvasRB.Y - canvasLT.Y);
            rect.CanvasRect.Width = width;
            rect.CanvasRect.Height = height;
            Canvas.SetLeft(grid, canvasLT.X);
            Canvas.SetTop(grid, canvasRB.Y);

            tb.Foreground = GetColorBrushType(color);
            tb.FontSize = Fontsz;
            if (color == ColorType.Defect)
                tb.FontWeight = System.Windows.FontWeights.Bold;


            tb.Text = text;
            grid.Children.Add(tb);
            p_DrawElement.Add(grid);
            return grid;
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

        TShape rectInfo;
        private void SetShapeColor(ColorType color)
        {
            switch (color)
            {
                case ColorType.MasterFeature:
                    rectInfo = new TRect(Brushes.DarkMagenta, 4, 1);
                    break;
                case ColorType.ChipFeature:
                    rectInfo = new TRect(Brushes.DarkBlue, 4, 1);
                    break;
                case ColorType.FeatureMatching:
                    rectInfo = new TRect(Brushes.Gold, 4, 1);
                    break;
                case ColorType.FeatureMatchingFail:
                    rectInfo = new TRect(Brushes.Red, 4, 1);
                    break;
                case ColorType.Defect:
                    rectInfo = new TRect(Brushes.Red, 4, 1);
                    break;
                default:
                    rectInfo = new TRect(Brushes.Black, 4, 1);
                    break;
            }

        }
        public enum ColorType
        {
            MasterFeature,
            ChipFeature,
            FeatureMatching,
            FeatureMatchingFail,
            Defect,
        }
        #endregion


        #region ICommand
        public ICommand btnIllum1
        {
            get => new RelayCommand(() =>
            {
                SelectedIdx = 0;
                SetImageSource();
            });
        }
        public ICommand btnIllum2
        {
            get => new RelayCommand(() =>
            {
                SelectedIdx = 1;
                SetImageSource();
            });
        }
        public ICommand cmdClear
        {
            get
            {
                return new RelayCommand(() =>
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
}
