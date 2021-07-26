using RootTools;
using RootTools_Vision;
using RootTools_CLR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Root_WindII
{
    public class FrontsideMeasurement_ViewModel : RootViewer_ViewModel, IPage
    {
        private class DefineColors
        {
            public static SolidColorBrush OriginBoxColor = Brushes.Blue;
            public static SolidColorBrush RefCoordBoxColor = Brushes.Yellow;
            public static SolidColorBrush RefCoordCrossColor = Brushes.Magenta;

            public static SolidColorBrush InspROIBoxColor = Brushes.Cyan;
            public static SolidColorBrush FirstInspROIBoxColor = Brushes.Red;
        }
        /// <summary>
        /// 전체 Memory의 좌표와 ROI Memory 좌표의 Offset
        /// </summary>
        CPoint OriginOffset = new CPoint();

        Grid OriginBox;

        TShape CurrentShape;
        TShape CropShape;

        CPoint ModifyPointBuffer = new CPoint();

        ToolProcess eToolProcess;
        ToolType eToolType;
        ModifyType eModifyType;
        SelectedTeaching eSelectedTeaching;

        private readonly VerticalWire_RecipeTeaching_ViewModel vericalWireVM;
        public VerticalWire_RecipeTeaching_ViewModel VericalWireVM
        {
            get => this.vericalWireVM;
        }
        public FrontsideMeasurement_ViewModel()
        { 
            if (GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr() == IntPtr.Zero && GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").m_eMode != ImageData.eMode.OtherPCMem)
                return;

            base.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());
            p_VisibleMenu = Visibility.Collapsed;

            this.vericalWireVM = new VerticalWire_RecipeTeaching_ViewModel();

            BufferInspROI.CollectionChanged += BufferInspROI_CollectionChanged;
            SetBackGroundWorker();

            p_ROILayer = GlobalObjects.Instance.GetNamed<ImageData>("MaskImage");

            InitializeUIlements();
        }

        public void InitializeUIlements()
        {
            OriginBox = new Grid();
            OriginBox.Children.Add(new Line()); // Left
            OriginBox.Children.Add(new Line()); // Top
            OriginBox.Children.Add(new Line()); // Right
            OriginBox.Children.Add(new Line()); // Bottom

            p_ViewElement.Add(OriginBox);
        }


        public void LoadRecipe()
        {
            p_cInspROI.Clear();

            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();
            foreach (RecipeType_Mask mask in recipe.GetItem<MaskRecipe>().MaskList)
            {
                ItemMask roi = new ItemMask();
                roi.p_Color = mask.ColorIndex;
                roi.p_Index = p_cInspROI.Count();
                roi.p_FullSize = (long)(originRecipe.OriginWidth) * (long)(originRecipe.OriginHeight);
                roi.p_Data = mask.ToPointLineList();

                p_cInspROI.Add(roi);
            }

            if (p_cInspROI.Count > 0)
            {
                p_SelectedROI = p_cInspROI.First();
            }
        }

        #region Property
        /// <summary>
        /// ROI List
        /// </summary>
        public ObservableCollection<ItemMask> p_cInspROI
        {
            get
            {
                return m_cInspROI;
            }
            set
            {
                m_cInspROI = value;
            }
        }
        private ObservableCollection<ItemMask> m_cInspROI = new ObservableCollection<ItemMask>();
        /// <summary>
        /// Selected ROI
        /// </summary>
        public ItemMask p_SelectedROI
        {
            get
            {
                if (m_SelectedROI != null)
                    p_ToolEnable = true;
                else
                {
                    p_ToolEnable = false;
                }

                return m_SelectedROI;
            }
            set
            {
                SetProperty(ref m_SelectedROI, value);
                if (value != null)
                    _ReadROI();
            }
        }
        private ItemMask m_SelectedROI;
        /// <summary>
        /// Enable Draw Tool
        /// </summary>
        public bool p_ToolEnable
        {
            get
            {
                return m_ToolEnable;
            }
            set
            {
                SetProperty(ref m_ToolEnable, value);
            }
        }
        private bool m_ToolEnable = false;

        /// <summary>
        /// ROI Page Opacity
        /// </summary>
        public double p_PageOpacity
        {
            get
            {
                return m_PageOpacity;
            }
            set
            {
                SetProperty(ref m_PageOpacity, value);
            }
        }
        private double m_PageOpacity = 1;
        /// <summary>
        /// ROI Page Enable
        /// </summary>
        public bool p_PageEnable
        {
            get
            {
                return m_PageEnable;
            }
            set
            {
                SetProperty(ref m_PageEnable, value);
            }
        }
        private bool m_PageEnable = true;
        /// <summary>
        /// Loading Control Opacity
        /// </summary>
        public double p_LoadingOpacity
        {
            get
            {
                return m_LoadingOpacity;
            }
            set
            {
                SetProperty(ref m_LoadingOpacity, value);
            }
        }
        private double m_LoadingOpacity = 0;

        private int selectedRefCoordItem;
        public int SelectedRefCoordItem
        {
            get
            {
                return selectedRefCoordItem;
            }
            set
            {
                SetProperty(ref selectedRefCoordItem, value);
            }
        }

        private int selectedROIItem;
        public int SelectedROIItem
        {
            get
            {
                return selectedROIItem;
            }
            set
            {
                SetProperty(ref selectedROIItem, value);
            }
        }
        #endregion

        #region [Viewer Method]
        public void DisplayFull()
        {
            this.p_Zoom = 1;

            this.SetImageSource();
            Redraw();
        }

        public void DisplayBox()
        {
            this.SetRoiRect();

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
            Redraw();
        }
        #endregion

        #region Stack History
        private Stack<TShape[]> History = new Stack<TShape[]>();
        public ObservableCollection<TShape> BufferInspROI = new ObservableCollection<TShape>();
        private void BufferInspROI_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var shapes = sender as ObservableCollection<TShape>;
            foreach (TShape shape in shapes)
            {
                if (!p_UIElement.Contains(shape.UIElement))
                    p_UIElement.Add(shape.UIElement);
            }

            if (shapes.Count() == 0)
                p_UIElement.Clear();

            TShape[] Work = new TShape[50];
            shapes.CopyTo(Work, 0);

            History.Push(Work);
        }
        #endregion

        #region Override
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;

            CPoint memPt = new CPoint(p_MouseMemX, p_MouseMemY);
            memPt = CheckOriginBox(memPt);

            CPoint canvasPt = GetCanvasPoint(memPt - OriginOffset);
            switch (eToolProcess)
            {
                case ToolProcess.None:

                    if (eToolType != ToolType.None)
                    {
                        BufferInspROI.Clear();
                        if (eToolType == ToolType.Point)
                        {
                            if (BufferInspROI.Contains(CropShape))
                                BufferInspROI.Remove(CropShape);

                            StartDraw(eToolType, memPt, canvasPt);
                            BufferInspROI.Add(CropShape);
                        }
                        else
                        {
                            StartDraw(eToolType, memPt, canvasPt);
                            BufferInspROI.Add(CurrentShape);
                        }

                        eToolProcess = ToolProcess.Drawing;
                        break;
                    }
                    if (eModifyType != ModifyType.None)
                    {
                        StartModify(memPt);
                        break;
                    }
                    if (CropShape != null)
                    {
                        if (BufferInspROI.Contains(CropShape))
                        {
                            BufferInspROI.Remove(CropShape);
                            CropShape = null;
                        }

                    }
                    break;
                case ToolProcess.Drawing:
                    if (eToolType == ToolType.Polygon && (CurrentShape as TPolygon).CanvasPolygon.Visibility == Visibility.Hidden)
                    {
                        AddPolygonPoint(memPt, canvasPt);
                    }
                    else
                    {
                        DrawDone(eToolType, memPt, canvasPt);
                        eToolProcess = ToolProcess.None;
                    }
                    break;
                case ToolProcess.Modifying:
                    break;
            }
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);

            CPoint memPt = new CPoint(p_MouseMemX, p_MouseMemY);
            memPt = CheckOriginBox(memPt);

            CPoint canvasPt = GetCanvasPoint(memPt);

            switch (eToolProcess)
            {
                case ToolProcess.None:
                    break;
                case ToolProcess.Drawing:
                    Drawing(eToolType, memPt, canvasPt);
                    break;
                case ToolProcess.Modifying:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        ModifyingShape(memPt);
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
                    ModifyDone();
                    break;
            }
        }
        public override void SetRoiRect()
        {
            base.SetRoiRect();
            Redraw();
        }
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            base.CanvasMovePoint_Ref(point, nX, nY);
            Redraw();
        }
        #endregion

        #region General Draw Method
        private void StartDraw(ToolType type, CPoint startMemPt, CPoint startCanvasPt, Brush color = null, double thickness = 2, double opacity = 0.5)
        {
            if (color == null)
                color = Brushes.Red;

            switch (type)
            {
                case ToolType.Point:
                    StartDrawCropTool(Brushes.Black, 1.5, 1, startMemPt, startCanvasPt);
                    break;
                case ToolType.Line:
                    StartDrawLine(color, thickness, opacity, startMemPt, startCanvasPt);
                    break;
                case ToolType.Rect:
                    StartDrawRect(color, thickness, opacity, startMemPt, startCanvasPt);
                    break;
                case ToolType.Circle:
                    break;
                case ToolType.Polygon:
                    StartDrawPolygon(color, thickness, opacity, startMemPt, startCanvasPt);
                    break;
            }
            if (CurrentShape != null)
            {
                CurrentShape.UIElement.MouseLeave += UIElement_MouseLeave;
            }
        }

        private CPoint CheckOriginBox(CPoint memPt)
        {
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            int left = originRecipe.OriginX + 1;
            int right = originRecipe.OriginX + originRecipe.OriginWidth;
            int top = originRecipe.OriginY - originRecipe.OriginHeight + 1;
            int bottom = originRecipe.OriginY;

            CPoint checkedPt = new CPoint();

            checkedPt.X = (memPt.X < left) ? left : (memPt.X > right) ? right : memPt.X;
            checkedPt.Y = (memPt.Y < top) ? top : (memPt.Y > bottom) ? bottom : memPt.Y;

            return checkedPt;
        }
        private void Drawing(ToolType type, CPoint startMemPt, CPoint startCanvasPt)
        {
            switch (type)
            {
                case ToolType.None:
                    break;
                case ToolType.Point:
                    DrawingSelectRect(startMemPt, startCanvasPt);
                    break;
                case ToolType.Line:
                    DrawingLine(startMemPt, startCanvasPt);
                    break;
                case ToolType.Rect:
                    DrawingRect(startMemPt, startCanvasPt);
                    break;
                case ToolType.Circle:
                    break;
                case ToolType.Polygon:
                    DrawingPolygon(startMemPt, startCanvasPt);
                    break;
            }
        }
        private void DrawDone(ToolType type, CPoint startMemPt, CPoint startCanvasPt)
        {
            switch (type)
            {
                case ToolType.None:
                    break;
                case ToolType.Point:
                    DrawDoneSelectRect(startMemPt, startCanvasPt);
                    break;
                case ToolType.Line:
                    DrawDoneLine(startMemPt, startCanvasPt);
                    break;
                case ToolType.Rect:
                    DrawDoneRect(startMemPt, startCanvasPt);
                    break;
                case ToolType.Circle:
                    break;
                case ToolType.Polygon:
                    DrawDonePolygon(startMemPt, startCanvasPt);
                    break;
            }
        }
        #endregion

        #region Draw Method

        #region Line
        private void StartDrawLine(Brush color, double thickness, double opacity, CPoint startMemPt, CPoint startCanvasPt)
        {
            CurrentShape = new TLine(color, thickness, opacity);
            TLine line = CurrentShape as TLine;
            line.MemoryStartPoint = startMemPt;
            line.CanvasLine.X1 = startCanvasPt.X;
            line.CanvasLine.Y1 = startCanvasPt.Y;
            line.CanvasLine.X2 = startCanvasPt.X;
            line.CanvasLine.Y2 = startCanvasPt.Y;
        }
        private void DrawingLine(CPoint currentMemPt, CPoint currentCanvasPt)
        {
            TLine line = CurrentShape as TLine;
            line.CanvasLine.X2 = currentCanvasPt.X;
            line.CanvasLine.Y2 = currentCanvasPt.Y;
        }
        private void DrawDoneLine(CPoint currentMemPt, CPoint currentCanvasPt)
        {
            TLine line = CurrentShape as TLine;
            line.MemoryEndPoint = currentMemPt;
            line.SetData();
            CreateModifyTool_Line(line);
            foreach (CPoint pt in line.Data)
            {
                base.DrawPixelBitmap(pt - OriginOffset, 255, 0, 0, 255);
            }
            base.SetLayerSource();
        }
        #endregion

        #region Rect
        private void StartDrawRect(Brush color, double thickness, double opacity, CPoint startMemPt, CPoint startCanvasPt)
        {
            CurrentShape = new TRect(color, thickness, opacity);
            TRect rect = CurrentShape as TRect;
            rect.MemPointBuffer = startMemPt;
            rect.MemoryRect.Left = startMemPt.X;
            rect.MemoryRect.Top = startMemPt.Y;
        }
        private void DrawingRect(CPoint currentMemPt, CPoint currentCanvasPt)
        {
            TRect rect = CurrentShape as TRect;

            if (rect.MemPointBuffer.X > currentMemPt.X)
            {
                rect.MemoryRect.Left = currentMemPt.X;
                rect.MemoryRect.Right = rect.MemPointBuffer.X;
            }
            else
            {
                rect.MemoryRect.Left = rect.MemPointBuffer.X;
                rect.MemoryRect.Right = currentMemPt.X;
            }
            if (rect.MemPointBuffer.Y > currentMemPt.Y)
            {
                rect.MemoryRect.Top = currentMemPt.Y;
                rect.MemoryRect.Bottom = rect.MemPointBuffer.Y;
            }
            else
            {
                rect.MemoryRect.Top = rect.MemPointBuffer.Y;
                rect.MemoryRect.Bottom = currentMemPt.Y;
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
        private void DrawDoneRect(CPoint currentMemPt, CPoint currentCanvasPt)
        {
            if (eSelectedTeaching == SelectedTeaching.None)
                return;

            TRect rect = CurrentShape as TRect;
            BufferInspROI.Clear();

            if (eSelectedTeaching == SelectedTeaching.RefCoord)
            {
                VerticalWire_RefCoordItem refCoordItem = (vericalWireVM.RefCoordListItem[this.SelectedRefCoordItem]) as VerticalWire_RefCoordItem;
                VerticalWire_RefCoordItem_ViewModel refCoordItem_ViewModel = refCoordItem.DataContext as VerticalWire_RefCoordItem_ViewModel;
                
                refCoordItem_ViewModel.RefX = rect.MemoryRect.Left;
                refCoordItem_ViewModel.RefY = rect.MemoryRect.Top;
                refCoordItem_ViewModel.RefW = rect.MemoryRect.Width;
                refCoordItem_ViewModel.RefH = rect.MemoryRect.Height;

                switch(refCoordItem_ViewModel.eArrangeType)
                {
                    case VerticalWire_RefCoordItem_ViewModel.ArrangeType.LT :
                        refCoordItem_ViewModel.RefCoord = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
                        break;
                    case VerticalWire_RefCoordItem_ViewModel.ArrangeType.LB:
                        refCoordItem_ViewModel.RefCoord = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Bottom);
                        break;
                    case VerticalWire_RefCoordItem_ViewModel.ArrangeType.RT:
                        refCoordItem_ViewModel.RefCoord = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Top);
                        break;
                    case VerticalWire_RefCoordItem_ViewModel.ArrangeType.RB:
                        refCoordItem_ViewModel.RefCoord = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);
                        break;
                    case VerticalWire_RefCoordItem_ViewModel.ArrangeType.Center:
                        refCoordItem_ViewModel.RefCoord = new CPoint((rect.MemoryRect.Left + rect.MemoryRect.Right)/2, (rect.MemoryRect.Top + rect.MemoryRect.Bottom)/2);
                        break;
                }
                DrawRefCoordElement();
            }

            else if(eSelectedTeaching == SelectedTeaching.InspROI)
            {
                VerticalWire_ROIItem roiItem = (vericalWireVM.ROIListItem[this.SelectedRefCoordItem]) as VerticalWire_ROIItem;
                VerticalWire_ROIItem_ViewModel roiItemItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;

                int roiW = rect.MemoryRect.Right - rect.MemoryRect.Left;
                int roiH = rect.MemoryRect.Bottom - rect.MemoryRect.Top;

                IntPtr imgPtr  = GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr(roiItemItem_ViewModel.SelectedChannel);
                long stride = GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").p_Stride;

                byte[] roiBuffer = new byte[roiW * roiH];
                int idx = 0;

                unsafe
                {
                    byte *p = (byte*)imgPtr.ToPointer();
                    for (Int64 r = rect.MemoryRect.Top; r < rect.MemoryRect.Bottom; r++)
                    {
                        for (Int64 c = rect.MemoryRect.Left; c < (long)rect.MemoryRect.Right; c++)
                        {
                            roiBuffer[idx++] = p[r * stride + c];
                        }
                    }
                    
                    CLR_IP.Cpp_Threshold(roiBuffer, roiBuffer, roiW, roiH, true, roiItemItem_ViewModel.Threshold);
                    var detectList = CLR_IP.Cpp_Labeling(roiBuffer, roiBuffer, roiW, roiH);

                    roiItemItem_ViewModel.WireRectList.Clear();
                    
                    foreach (Cpp_LabelParam list in detectList)
                    {
                        if (list.area < 5)
                            continue;

                        TRect tmpRect = new TRect();
                        tmpRect.MemoryRect.Left = list.boundLeft + rect.MemoryRect.Left;
                        tmpRect.MemoryRect.Right = list.boundRight + rect.MemoryRect.Left;
                        tmpRect.MemoryRect.Top = list.boundTop + rect.MemoryRect.Top;
                        tmpRect.MemoryRect.Bottom = list.boundBottom + rect.MemoryRect.Top;
                        tmpRect.MemoryRect.Width = (int)list.width;
                        tmpRect.MemoryRect.Height = (int)list.height;

                        roiItemItem_ViewModel.WireRectList.Add(tmpRect);
                    }

                    if (roiItemItem_ViewModel.WireRectList.Count > 50)
                        MessageBox.Show("Vertical Wire가 비정상적으로 많이 검출 되었습니다. \n Threshod 값을 조절하세요.", "Error");

                    else { 
                        roiItemItem_ViewModel.ArrangeDetectPoint();
                        DrawInspROIElement(roiItemItem_ViewModel.WireRectList);
                    }
                }
                
            }
        }
        #endregion

        #region Polygon
        private void StartDrawPolygon(Brush color, double thickness, double opacity, CPoint startMemPt, CPoint startCanvasPt)
        {
            CurrentShape = new TPolygon(color, thickness, opacity);
            TPolygon polygon = CurrentShape as TPolygon;
            polygon.CanvasPolygon.Visibility = Visibility.Hidden;
            p_UIElement.Add(polygon.CanvasPolyLine);

            polygon.ListMemoryPoint.Add(startMemPt);
            polygon.CanvasPolyLine.Points.Add(new Point(startCanvasPt.X, startCanvasPt.Y));
            polygon.CanvasPolyLine.Points.Add(new Point(startCanvasPt.X, startCanvasPt.Y));
        }
        private void DrawingPolygon(CPoint currentMemPt, CPoint currentCanvasPt)
        {
            TPolygon polygon = CurrentShape as TPolygon;
            int lineCnt = polygon.CanvasPolyLine.Points.Count;
            Point nowPt = new Point(currentCanvasPt.X, currentCanvasPt.Y);
            Point startPt = polygon.CanvasPolyLine.Points.First();
            polygon.CanvasPolyLine.Points[lineCnt - 1] = nowPt;

            if (startPt.X - 5 < nowPt.X && nowPt.X < startPt.X + 5 &&
                startPt.Y - 5 < nowPt.Y && nowPt.Y < startPt.Y + 5)
            {
                polygon.CanvasPolygon.Points = polygon.CanvasPolyLine.Points;
                polygon.CanvasPolygon.Visibility = Visibility.Visible;
            }
            else
            {
                polygon.CanvasPolygon.Visibility = Visibility.Hidden;
            }
        }
        private void AddPolygonPoint(CPoint currentMemPt, CPoint currentCanvasPt)
        {
            TPolygon polygon = CurrentShape as TPolygon;
            polygon.ListMemoryPoint.Add(currentMemPt);
            polygon.CanvasPolyLine.Points.Add(new Point(currentCanvasPt.X, currentCanvasPt.Y));
        }
        private void DrawDonePolygon(CPoint currentMemPt, CPoint currentCanvasPt)
        {
            TPolygon polygon = CurrentShape as TPolygon;
            p_UIElement.Remove(polygon.CanvasPolyLine);

            CreateModifyTool_Polygon(polygon);
        }
        #endregion

        #region Crop
        private void StartDrawCropTool(Brush color, double thickness, double opacity, CPoint startMemPt, CPoint startCanvasPt)
        {
            CropShape = new TCropTool(color, thickness, opacity);
            TCropTool crop = CropShape as TCropTool;
            crop.CanvasRect.StrokeDashArray = new DoubleCollection { 3, 1 };
            crop.MemPointBuffer = startMemPt;
            crop.MemoryRect.Left = startMemPt.X;
            crop.MemoryRect.Top = startMemPt.Y;
        }
        private void DrawingSelectRect(CPoint currentMemPt, CPoint currentCanvasPt)
        {
            //TCropTool crop = CropShape as TCropTool;

            //if (crop.MemPointBuffer.X > currentMemPt.X)
            //{
            //    crop.MemoryRect.Left = currentMemPt.X;
            //    crop.MemoryRect.Right = crop.MemPointBuffer.X;
            //}
            //else
            //{
            //    crop.MemoryRect.Left = crop.MemPointBuffer.X;
            //    crop.MemoryRect.Right = currentMemPt.X;
            //}
            //if (crop.MemPointBuffer.Y > currentMemPt.Y)
            //{
            //    crop.MemoryRect.Top = currentMemPt.Y;
            //    crop.MemoryRect.Bottom = crop.MemPointBuffer.Y;
            //}
            //else
            //{
            //    crop.MemoryRect.Top = crop.MemPointBuffer.Y;
            //    crop.MemoryRect.Bottom = currentMemPt.Y;
            //}
            //double pixSizeX = p_CanvasWidth / p_View_Rect.Width;
            //double pixSizeY = p_CanvasHeight / p_View_Rect.Height;
            //CPoint LT = new CPoint(crop.MemoryRect.Left, crop.MemoryRect.Top);
            //CPoint RB = new CPoint(crop.MemoryRect.Right, crop.MemoryRect.Bottom);
            //CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
            //CPoint canvasRB = new CPoint(GetCanvasPoint(RB));

            //Canvas.SetLeft(crop.CanvasRect, canvasLT.X - pixSizeX / 2);
            //Canvas.SetTop(crop.CanvasRect, canvasLT.Y - pixSizeY / 2);

            //crop.CanvasRect.Width = Math.Abs(canvasRB.X - canvasLT.X + pixSizeX);
            //crop.CanvasRect.Height = Math.Abs(canvasRB.Y - canvasLT.Y + pixSizeY);
        }
        private void DrawDoneSelectRect(CPoint currentMemPt, CPoint currentCanvasPt)
        {
            TCropTool crop = CropShape as TCropTool;
            Canvas.SetZIndex(crop.CanvasRect, 99);
            crop.CanvasRect.Fill = Brushes.Transparent;
            crop.CanvasRect.Cursor = Cursors.ScrollAll;
            crop.CanvasRect.MouseEnter += SelectRect_MouseEnter;
            crop.CanvasRect.MouseLeave += SelectRect_MouseLeave;

            double left, top, width, height;
            left = Canvas.GetLeft(crop.CanvasRect);
            top = Canvas.GetTop(crop.CanvasRect);
            width = crop.CanvasRect.Width;
            height = crop.CanvasRect.Height;

            CRect nowRect = new CRect(crop.MemoryRect.Left, crop.MemoryRect.Top, crop.MemoryRect.Right, crop.MemoryRect.Bottom);

            ImageData rectImageData = new ImageData(nowRect.Width, nowRect.Height, 4);
            rectImageData.SetData(p_ROILayer.GetPtr(), nowRect, (int)p_ROILayer.p_Stride, 4);


            Image CropImage = new Image();
            CropImage.Opacity = 1;
            CropImage.Source = rectImageData.GetBitMapSource(4);
            Canvas.SetLeft(CropImage, left);
            Canvas.SetTop(CropImage, top);
            CropImage.Width = width;
            CropImage.Height = height;
            //CropArea.Children.Add(CropImage);

            crop.isSelected = true;
            crop.CropImage = CropImage;
            crop.CropImageData = rectImageData;
            p_UIElement.Add(CropImage);
        }
        #endregion

        #endregion

        #region Modify Method
        private void UIElement_MouseLeave(object sender, MouseEventArgs e)
        {
            p_Cursor = Cursors.Arrow;
        }

        private void StartModify(CPoint currentMemPt)
        {
            ModifyPointBuffer = currentMemPt;
            eToolProcess = ToolProcess.Modifying;
            if (CropShape != null)
            {
                if (m_KeyEvent == null)
                {
                    DrawRectBitmap((CropShape as TCropTool).MemoryRect, 0, 0, 0, 0, OriginOffset);
                    SetLayerSource();
                    return;
                }
                if (m_KeyEvent.Key == Key.LeftCtrl && m_KeyEvent.IsDown)
                {
                }
                else
                {
                    CRect rect = (CropShape as TCropTool).MemoryRect;
                    CRect cropRect = new CRect(rect.Left, rect.Top, rect.Right + 1, rect.Bottom + 1);
                    DrawRectBitmap((CropShape as TCropTool).MemoryRect, 0, 0, 0, 0, OriginOffset);
                }

            }
            SetLayerSource();
        }
        private void ModifyingShape(CPoint currentMemPt)
        {
            foreach (TShape shape in BufferInspROI)
            {
                switch (shape)
                {
                    case TCropTool:
                        Modifying_CropTool(CropShape as TCropTool, currentMemPt);
                        break;
                    case TLine:
                        Modifying_Line(shape as TLine, currentMemPt);
                        break;
                    case TRect:
                        Modifying_Rect(shape as TRect, currentMemPt);
                        break;
                    case TPolygon:
                        Modifying_Polygon(shape as TPolygon, currentMemPt);
                        break;
                }
            }
            ModifyPointBuffer = currentMemPt;


        }
        private void ModifyDone()
        {
            foreach (TShape shape in BufferInspROI)
            {
                if (shape.isSelected)
                {
                    if (p_UIElement.Contains(shape.ModifyTool))
                        p_UIElement.Remove(shape.ModifyTool);

                    switch (shape)
                    {
                        case TCropTool:
                            TCropTool crop = shape as TCropTool;
                            CRect selectRect = new CRect(crop.MemoryRect.Left, crop.MemoryRect.Top, crop.MemoryRect.Right + 1, crop.MemoryRect.Bottom + 1);
                            byte r = p_SelectedROI.p_Color.R;
                            byte g = p_SelectedROI.p_Color.G;
                            byte b = p_SelectedROI.p_Color.B;
                            byte a = p_SelectedROI.p_Color.A;
                            //현재 memoryRect의 시작주소를 p_roilayer에서 가져와서 cropshape의 imagedata만큼 복사?
                            CropRectSetData(crop.CropImageData, selectRect);
                            //p_UIElement.Clear();
                            SetLayerSource();
                            break;
                        case TLine:
                            CreateModifyTool_Line(shape as TLine);
                            break;
                        case TRect:
                            CreateModifyTool_Rect(shape as TRect);
                            break;
                        case TPolygon:
                            CreateModifyTool_Polygon(shape as TPolygon);
                            break;
                    }
                    //shape.ModifyTool.Visibility = Visibility.Visible;
                }
            }
            p_Cursor = Cursors.Arrow;
            eToolProcess = ToolProcess.None;
            eModifyType = ModifyType.None;
            if (CropShape != null)
                if (CropShape.UIElement.IsMouseOver)
                    eModifyType = ModifyType.ScrollAll;
        }

        private void CreateModifyTool_Line(TLine line)
        {
            double left, top, width, height;
            CPoint StartPt = GetCanvasPoint(line.MemoryStartPoint - OriginOffset);
            CPoint EndPt = GetCanvasPoint(line.MemoryEndPoint - OriginOffset);

            left = (StartPt.X < EndPt.X) ? StartPt.X : EndPt.X;
            top = (StartPt.Y < EndPt.Y) ? StartPt.Y : EndPt.Y;
            width = Math.Abs(EndPt.X - StartPt.X);
            height = Math.Abs(EndPt.Y - StartPt.Y);

            Grid ModifyGrid = new Grid();
            ModifyGrid.Tag = line;
            Canvas.SetLeft(ModifyGrid, left - 5);
            Canvas.SetTop(ModifyGrid, top - 5);
            ModifyGrid.Width = width + 10;
            ModifyGrid.Height = height + 10;

            Ellipse LineModifyPointLeft = new Ellipse();
            LineModifyPointLeft.MouseEnter += LineModifyPointLeft_MouseEnter;
            LineModifyPointLeft.MouseLeave += LineModifyPoint_MouseLeave;
            LineModifyPointLeft.Width = 10;
            LineModifyPointLeft.Height = 10;
            LineModifyPointLeft.Stroke = Brushes.Gray;
            LineModifyPointLeft.StrokeThickness = 2;
            LineModifyPointLeft.Fill = Brushes.White;
            LineModifyPointLeft.HorizontalAlignment = HorizontalAlignment.Left;

            Ellipse LineModifyPointRight = new Ellipse();
            LineModifyPointRight.Tag = line;
            LineModifyPointRight.MouseEnter += LineModifyPointRight_MouseEnter;
            LineModifyPointRight.MouseLeave += LineModifyPoint_MouseLeave;
            LineModifyPointRight.Width = 10;
            LineModifyPointRight.Height = 10;
            LineModifyPointRight.Stroke = Brushes.Gray;
            LineModifyPointRight.StrokeThickness = 2;
            LineModifyPointRight.Fill = Brushes.White;
            LineModifyPointRight.HorizontalAlignment = HorizontalAlignment.Right;

            if (left == StartPt.X)
            {
                LineModifyPointLeft.Tag = line.MemoryStartPoint;
                LineModifyPointRight.Tag = line.MemoryEndPoint;
                if (StartPt.Y < EndPt.Y)
                {
                    LineModifyPointLeft.VerticalAlignment = VerticalAlignment.Top;
                    LineModifyPointRight.VerticalAlignment = VerticalAlignment.Bottom;
                }
                else
                {
                    LineModifyPointLeft.VerticalAlignment = VerticalAlignment.Bottom;
                    LineModifyPointRight.VerticalAlignment = VerticalAlignment.Top;
                }
            }
            else
            {
                LineModifyPointLeft.Tag = line.MemoryEndPoint;
                LineModifyPointRight.Tag = line.MemoryStartPoint;
                if (StartPt.Y < EndPt.Y)
                {
                    LineModifyPointLeft.VerticalAlignment = VerticalAlignment.Bottom;
                    LineModifyPointRight.VerticalAlignment = VerticalAlignment.Top;
                }
                else
                {
                    LineModifyPointLeft.VerticalAlignment = VerticalAlignment.Top;
                    LineModifyPointRight.VerticalAlignment = VerticalAlignment.Bottom;
                }
            }

            ModifyGrid.Children.Add(LineModifyPointLeft);
            ModifyGrid.Children.Add(LineModifyPointRight);

            line.ModifyTool = ModifyGrid;
            line.isSelected = true;
            p_UIElement.Add(ModifyGrid);
        }
        private void CreateModifyTool_Rect(TRect rect)
        {
            double left, top, width, height;
            left = Canvas.GetLeft(rect.CanvasRect);
            top = Canvas.GetTop(rect.CanvasRect);
            width = rect.CanvasRect.Width;
            height = rect.CanvasRect.Height;

            Grid ModifyGrid = new Grid();
            Canvas.SetLeft(ModifyGrid, left - 5);
            Canvas.SetTop(ModifyGrid, top - 5);
            ModifyGrid.Width = width + 10;
            ModifyGrid.Height = height + 10;

            Border OutLine = new Border();
            OutLine.BorderBrush = Brushes.Gray;
            OutLine.Margin = new Thickness(5);
            OutLine.BorderThickness = new Thickness(2);

            ModifyGrid.Children.Add(OutLine);

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Ellipse ModifyPoint = new Ellipse();
                    ModifyPoint.Tag = new CPoint(i, j);
                    ModifyPoint.MouseEnter += ModifyPoint_MouseEnter;
                    ModifyPoint.MouseLeave += ModifyPoint_MouseLeave;
                    ModifyPoint.Width = 10;
                    ModifyPoint.Height = 10;
                    ModifyPoint.Stroke = Brushes.Gray;
                    ModifyPoint.StrokeThickness = 2;
                    ModifyPoint.Fill = Brushes.White;
                    if (i == 0)
                        ModifyPoint.HorizontalAlignment = HorizontalAlignment.Left;
                    if (i == 1)
                        ModifyPoint.HorizontalAlignment = HorizontalAlignment.Center;
                    if (i == 2)
                        ModifyPoint.HorizontalAlignment = HorizontalAlignment.Right;
                    if (j == 0)
                        ModifyPoint.VerticalAlignment = VerticalAlignment.Top;
                    if (j == 1)
                        ModifyPoint.VerticalAlignment = VerticalAlignment.Center;
                    if (j == 2)
                        ModifyPoint.VerticalAlignment = VerticalAlignment.Bottom;

                    ModifyGrid.Children.Add(ModifyPoint);
                }
            rect.ModifyTool = ModifyGrid;
            rect.isSelected = true;
            p_UIElement.Add(rect.ModifyTool);
        }
        private void CreateModifyTool_Polygon(TPolygon polygon)
        {
            double left, top, right, bottom;

            polygon.CanvasPolyLine.Points.Remove(polygon.CanvasPolyLine.Points.Last());
            left = polygon.CanvasPolyLine.Points.Min(Point => Point.X);
            top = polygon.CanvasPolyLine.Points.Min(Point => Point.Y);
            right = polygon.CanvasPolyLine.Points.Max(Point => Point.X);
            bottom = polygon.CanvasPolyLine.Points.Max(Point => Point.Y);

            double width = Math.Abs(right - left);
            double height = Math.Abs(bottom - top);

            Grid ModifyGrid = new Grid();
            Canvas.SetLeft(ModifyGrid, left - 5);
            Canvas.SetTop(ModifyGrid, top - 5);
            ModifyGrid.Width = width + 10;
            ModifyGrid.Height = height + 10;

            Border OutLine = new Border();
            OutLine.BorderBrush = Brushes.Gray;
            OutLine.Margin = new Thickness(5);
            OutLine.BorderThickness = new Thickness(2);

            ModifyGrid.Children.Add(OutLine);

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Ellipse ModifyPoint = new Ellipse();
                    ModifyPoint.Tag = new CPoint(i, j);
                    ModifyPoint.MouseEnter += ModifyPoint_MouseEnter;
                    ModifyPoint.MouseLeave += ModifyPoint_MouseLeave;
                    ModifyPoint.Width = 10;
                    ModifyPoint.Height = 10;
                    ModifyPoint.Stroke = Brushes.Gray;
                    ModifyPoint.StrokeThickness = 2;
                    ModifyPoint.Fill = Brushes.White;
                    if (i == 0)
                        ModifyPoint.HorizontalAlignment = HorizontalAlignment.Left;
                    if (i == 1)
                        ModifyPoint.HorizontalAlignment = HorizontalAlignment.Center;
                    if (i == 2)
                        ModifyPoint.HorizontalAlignment = HorizontalAlignment.Right;
                    if (j == 0)
                        ModifyPoint.VerticalAlignment = VerticalAlignment.Top;
                    if (j == 1)
                        ModifyPoint.VerticalAlignment = VerticalAlignment.Center;
                    if (j == 2)
                        ModifyPoint.VerticalAlignment = VerticalAlignment.Bottom;

                    ModifyGrid.Children.Add(ModifyPoint);
                }
            polygon.ModifyTool = ModifyGrid;

            polygon.isSelected = true;
            p_UIElement.Add(polygon.ModifyTool);
        }


        private void LineModifyPointLeft_MouseEnter(object sender, MouseEventArgs e)
        {
            Ellipse ModifyPoint = sender as Ellipse;
            CPoint LeftPoint = ModifyPoint.Tag as CPoint;
            TLine line = (ModifyPoint.Parent as Grid).Tag as TLine;

            if (ModifyPoint.VerticalAlignment == VerticalAlignment.Top)
                p_Cursor = Cursors.SizeNWSE;
            else
                p_Cursor = Cursors.SizeNESW;

            if (line.MemoryStartPoint == LeftPoint)
                eModifyType = ModifyType.LineStart;
            else
                eModifyType = ModifyType.LineEnd;
        }
        private void LineModifyPointRight_MouseEnter(object sender, MouseEventArgs e)
        {
            Ellipse ModifyPoint = sender as Ellipse;
            CPoint RightPoint = ModifyPoint.Tag as CPoint;
            TLine line = (ModifyPoint.Parent as Grid).Tag as TLine;

            if (ModifyPoint.VerticalAlignment == VerticalAlignment.Top)
                p_Cursor = Cursors.SizeNESW;
            else
                p_Cursor = Cursors.SizeNWSE;

            if (line.MemoryStartPoint == RightPoint)
                eModifyType = ModifyType.LineStart;
            else
                eModifyType = ModifyType.LineEnd;
        }
        private void LineModifyPoint_MouseLeave(object sender, MouseEventArgs e)
        {
            p_Cursor = Cursors.Arrow;
        }
        private void ModifyPoint_MouseEnter(object sender, MouseEventArgs e)
        {
            CPoint index = (sender as Shape).Tag as CPoint;
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
            if (e.LeftButton == MouseButtonState.Released)
                eModifyType = ModifyType.None;
        }
        private void SelectRect_MouseEnter(object sender, MouseEventArgs e)
        {
            eModifyType = ModifyType.ScrollAll;
        }
        private void SelectRect_MouseLeave(object sender, MouseEventArgs e)
        {
            eModifyType = ModifyType.None;
        }

        private void Modifying_Line(TLine line, CPoint currentMemPt)
        {
            int offset_x = currentMemPt.X - ModifyPointBuffer.X;
            int offset_y = currentMemPt.Y - ModifyPointBuffer.Y;
            CPoint ptOffset = new CPoint(offset_x, offset_y);
            if (line.isSelected)
            {
                line.ModifyTool.Visibility = Visibility.Collapsed;
                switch (eModifyType)
                {
                    case ModifyType.LineStart:
                        p_Cursor = Cursors.Cross;
                        line.MemoryStartPoint += ptOffset;
                        break;
                    case ModifyType.LineEnd:
                        p_Cursor = Cursors.Cross;
                        line.MemoryEndPoint += ptOffset;
                        break;
                }
                RedrawLine(line);
            }
        }
        private void Modifying_CropTool(TCropTool crop, CPoint currentMemPt)
        {
            int offset_x = currentMemPt.X - ModifyPointBuffer.X;
            int offset_y = currentMemPt.Y - ModifyPointBuffer.Y;
            CPoint ptOffset = new CPoint(offset_x, offset_y);

            int left, top, right, bottom;
            left = crop.MemoryRect.Left;
            top = crop.MemoryRect.Top;
            right = crop.MemoryRect.Right;
            bottom = crop.MemoryRect.Bottom;

            left += ptOffset.X;
            top += ptOffset.Y;
            right += ptOffset.X;
            bottom += ptOffset.Y;
            p_Cursor = Cursors.Cross;

            crop.MemoryRect.Left = left;
            crop.MemoryRect.Top = top;
            crop.MemoryRect.Right = right;
            crop.MemoryRect.Bottom = bottom;

            RedrawCropTool(crop);
        }
        private void Modifying_Rect(TRect rect, CPoint currentMemPt)
        {
            int offset_x = currentMemPt.X - ModifyPointBuffer.X;
            int offset_y = currentMemPt.Y - ModifyPointBuffer.Y;
            CPoint ptOffset = new CPoint(offset_x, offset_y);

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
                        left += ptOffset.X;
                        top += ptOffset.Y;
                        right += ptOffset.X;
                        bottom += ptOffset.Y;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.Left:
                        left += ptOffset.X;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.Right:
                        right += ptOffset.X;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.Top:
                        top += ptOffset.Y;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.Bottom:
                        bottom += ptOffset.Y;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.LeftTop:
                        left += ptOffset.X;
                        top += ptOffset.Y;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.LeftBottom:
                        left += ptOffset.X;
                        bottom += ptOffset.Y;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.RightTop:
                        right += ptOffset.X;
                        top += ptOffset.Y;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.RightBottom:
                        right += ptOffset.X;
                        bottom += ptOffset.Y;
                        p_Cursor = Cursors.Cross;
                        break;
                }

                rect.MemoryRect.Left = left;
                rect.MemoryRect.Top = top;
                rect.MemoryRect.Right = right;
                rect.MemoryRect.Bottom = bottom;

                RedrawRect(rect);
            }
        }
        private void Modifying_Polygon(TPolygon polygon, CPoint currentMemPt)
        {
            int offset_x = currentMemPt.X - ModifyPointBuffer.X;
            int offset_y = currentMemPt.Y - ModifyPointBuffer.Y;
            CPoint ptOffset = new CPoint(offset_x, offset_y);
            if (polygon.isSelected)
            {
                polygon.ModifyTool.Visibility = Visibility.Collapsed;
                int left, top, right, bottom;
                left = polygon.ListMemoryPoint.Min(Point => Point.X);
                top = polygon.ListMemoryPoint.Min(Point => Point.Y);
                right = polygon.ListMemoryPoint.Max(Point => Point.X);
                bottom = polygon.ListMemoryPoint.Max(Point => Point.Y);

                switch (eModifyType)
                {
                    case ModifyType.ScrollAll:
                        left += ptOffset.X;
                        top += ptOffset.Y;
                        right += ptOffset.X;
                        bottom += ptOffset.Y;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.Left:
                        left += ptOffset.X;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.Right:
                        right += ptOffset.X;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.Top:
                        top += ptOffset.Y;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.Bottom:
                        bottom += ptOffset.Y;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.LeftTop:
                        left += ptOffset.X;
                        top += ptOffset.Y;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.LeftBottom:
                        left += ptOffset.X;
                        bottom += ptOffset.Y;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.RightTop:
                        right += ptOffset.X;
                        top += ptOffset.Y;
                        p_Cursor = Cursors.Cross;
                        break;
                    case ModifyType.RightBottom:
                        right += ptOffset.X;
                        bottom += ptOffset.Y;
                        p_Cursor = Cursors.Cross;
                        break;
                }
            }
        }
        #endregion

        #region Redraw Method
        private void Redraw()
        {
            foreach (TShape shape in BufferInspROI)
            {
                switch (shape)
                {
                    case TCropTool:
                        RedrawCropTool(shape as TCropTool);
                        break;
                    case TPoint:
                        break;
                    case TLine:
                        RedrawLine(shape as TLine);
                        break;
                    case TRect:
                        RedrawRect(shape as TRect);
                        break;
                    case TEllipse:
                        break;
                    case TPolygon:
                        RedrawPolygon(shape as TPolygon);
                        break;
                }
                if (shape.isSelected)
                {
                    if (p_UIElement.Contains(shape.ModifyTool))
                        p_UIElement.Remove(shape.ModifyTool);
                    switch (shape)
                    {
                        case TCropTool:
                            break;
                        case TLine:
                            CreateModifyTool_Line(shape as TLine);
                            break;
                        case TRect:
                            CreateModifyTool_Rect(shape as TRect);
                            break;
                        case TPolygon:
                            CreateModifyTool_Polygon(shape as TPolygon);
                            break;
                    }
                    if (shape.ModifyTool != null)
                        shape.ModifyTool.Visibility = Visibility.Visible;
                }
            }

            RedrawOriginBox();
        }
        private void RedrawLine(TLine line)
        {
            CPoint startPt = GetCanvasPoint(line.MemoryStartPoint);
            CPoint endPt = GetCanvasPoint(line.MemoryEndPoint);

            line.CanvasLine.X1 = startPt.X;
            line.CanvasLine.Y1 = startPt.Y;
            line.CanvasLine.X2 = endPt.X;
            line.CanvasLine.Y2 = endPt.Y;
        }
        private void RedrawCropTool(TCropTool crop)
        {
            Image CropImage = crop.CropImage;

            double pixSizeX = p_CanvasWidth / p_View_Rect.Width;
            double pixSizeY = p_CanvasHeight / p_View_Rect.Height;

            CPoint LT = new CPoint(crop.MemoryRect.Left, crop.MemoryRect.Top);
            CPoint RB = new CPoint(crop.MemoryRect.Right, crop.MemoryRect.Bottom);

            CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB));
            int width = Math.Abs(canvasRB.X - canvasLT.X);
            int height = Math.Abs(canvasRB.Y - canvasLT.Y);
            crop.CanvasRect.Width = width + pixSizeX;
            crop.CanvasRect.Height = height + pixSizeY;
            Canvas.SetLeft(crop.CanvasRect, canvasLT.X - pixSizeX / 2);
            Canvas.SetTop(crop.CanvasRect, canvasLT.Y - pixSizeY / 2);
            if (CropImage != null)
            {
                CropImage.Width = width + pixSizeX;
                CropImage.Height = height + pixSizeY;
                Canvas.SetLeft(CropImage, canvasLT.X - pixSizeX / 2);
                Canvas.SetTop(CropImage, canvasLT.Y - pixSizeY / 2);
            }


        }
        private void RedrawRect(TRect rect)
        {
            double pixSizeX = (double)p_CanvasWidth / (double)p_View_Rect.Width;
            double pixSizeY = (double)p_CanvasHeight / (double)p_View_Rect.Height;

            CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
            CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);

            CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB));
            int width = Math.Abs(canvasRB.X - canvasLT.X);
            int height = Math.Abs(canvasRB.Y - canvasLT.Y);
            rect.CanvasRect.Width = width + pixSizeX;
            rect.CanvasRect.Height = height + pixSizeY;


            Canvas.SetLeft(rect.CanvasRect, canvasLT.X - pixSizeX / 2);
            Canvas.SetTop(rect.CanvasRect, canvasLT.Y - pixSizeY / 2);
            Canvas.SetRight(rect.CanvasRect, canvasRB.X);
            Canvas.SetBottom(rect.CanvasRect, canvasRB.Y);
        }
        private void RedrawPolygon(TPolygon polygon)
        {
            polygon.CanvasPolygon.Points.Clear();
            foreach (CPoint memoryPt in polygon.ListMemoryPoint)
            {
                CPoint canvasPt = GetCanvasPoint(memoryPt);
                polygon.CanvasPolygon.Points.Add(new Point(canvasPt.X, canvasPt.Y));
            }
        }

        private void RedrawOriginBox()
        {

            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            if (originRecipe.OriginWidth == 0 || originRecipe.OriginHeight == 0) return;

            int left = originRecipe.OriginX;
            int top = originRecipe.OriginY - originRecipe.OriginHeight;

            int right = originRecipe.OriginX + originRecipe.OriginWidth;
            int bottom = originRecipe.OriginY;

            CPoint canvasLeftTop = GetCanvasPoint(new CPoint(left, top));
            CPoint canvasLeftBottom = GetCanvasPoint(new CPoint(left, bottom));
            CPoint canvasRightTop = GetCanvasPoint(new CPoint(right, top));
            CPoint canvasRightBottom = GetCanvasPoint(new CPoint(right, bottom));


            // Canvas 이동
            this.p_LayerCanvasOffsetX = canvasLeftTop.X;
            this.p_LayerCanvasOffsetY = canvasLeftTop.Y;

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

        void DrawRefCoordElement()
        {
            VerticalWire_RefCoordItem refCoordItem = (vericalWireVM.RefCoordListItem[this.SelectedRefCoordItem]) as VerticalWire_RefCoordItem;
            VerticalWire_RefCoordItem_ViewModel refCoordItem_ViewModel = refCoordItem.DataContext as VerticalWire_RefCoordItem_ViewModel;

            TShape rectRefCoordBox = new TRect(DefineColors.RefCoordBoxColor, 2, 0.5);
            TRect rect = rectRefCoordBox as TRect;

            rect.MemoryRect.Left = refCoordItem_ViewModel.RefX;
            rect.MemoryRect.Top = refCoordItem_ViewModel.RefY;
            rect.MemoryRect.Right = refCoordItem_ViewModel.RefX + refCoordItem_ViewModel.RefW;
            rect.MemoryRect.Bottom = refCoordItem_ViewModel.RefY + refCoordItem_ViewModel.RefH;

            BufferInspROI.Add(rectRefCoordBox);
            RedrawRect(rect);

            if (refCoordItem_ViewModel.RefCoord != null)
            {
                TShape rectRefCoordCross1 = new TLine(DefineColors.RefCoordCrossColor, 4, 1);
                TLine line1 = rectRefCoordCross1 as TLine;
                line1.MemoryStartPoint = new CPoint(refCoordItem_ViewModel.RefCoord.X - 10, refCoordItem_ViewModel.RefCoord.Y - 10);
                line1.MemoryEndPoint = new CPoint(refCoordItem_ViewModel.RefCoord.X + 10, refCoordItem_ViewModel.RefCoord.Y + 10);

                BufferInspROI.Add(rectRefCoordCross1);
                RedrawLine(line1);

                TShape rectRefCoordCross2 = new TLine(DefineColors.RefCoordCrossColor, 4, 1);
                TLine line2 = rectRefCoordCross2 as TLine;
                line2.MemoryStartPoint = new CPoint(refCoordItem_ViewModel.RefCoord.X + 10, refCoordItem_ViewModel.RefCoord.Y - 10);
                line2.MemoryEndPoint = new CPoint(refCoordItem_ViewModel.RefCoord.X - 10, refCoordItem_ViewModel.RefCoord.Y + 10);

                BufferInspROI.Add(rectRefCoordCross2);
                RedrawLine(line2);
            }
        }

        void DrawInspROIElement(List<TRect> detectList)
        {
            bool isFirst = true;
            foreach(TRect rt in detectList)
            {
                if (rt.MemoryRect.Width < 5 || rt.MemoryRect.Height < 5)
                    continue;

                TShape rectROIBox;
                if (isFirst)
                {
                    rectROIBox = new TRect(DefineColors.FirstInspROIBoxColor, 2, 0.5);
                    isFirst = false;
                }
                else
                    rectROIBox = new TRect(DefineColors.InspROIBoxColor, 2, 0.5);

                TRect rect = rectROIBox as TRect;
                rect.MemoryRect.Left = rt.MemoryRect.Left;
                rect.MemoryRect.Top = rt.MemoryRect.Top;
                rect.MemoryRect.Right = rt.MemoryRect.Right;
                rect.MemoryRect.Bottom = rt.MemoryRect.Bottom;

                BufferInspROI.Add(rectROIBox);
                RedrawRect(rect);
            }
        }
        #endregion

        #region ROI Async Method
        BackgroundWorker Worker_SaveROI = new BackgroundWorker();
        BackgroundWorker Worker_ClearROI = new BackgroundWorker();
        BackgroundWorker Worker_ReadROI = new BackgroundWorker();
        BackgroundWorker Worker_ShowAll = new BackgroundWorker();
        public void SetBackGroundWorker()
        {
            Worker_ClearROI.DoWork += Worker_ClearROI_DoWork;
            Worker_ClearROI.RunWorkerCompleted += Worker_ClearROI_RunWorkerCompleted;
            Worker_SaveROI.DoWork += Worker_SaveROI_DoWork;
            Worker_SaveROI.RunWorkerCompleted += Worker_SaveROI_RunWorkerCompleted;
            Worker_ReadROI.DoWork += Worker_ReadROI_DoWork;
            Worker_ReadROI.RunWorkerCompleted += Worker_ReadROI_RunWorkerCompleted;
            Worker_ShowAll.DoWork += Worker_ShowAll_DoWork;
            Worker_ShowAll.RunWorkerCompleted += Worker_ShowAll_RunWorkerCompleted;
        }

        private void Worker_ShowAll_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            p_PageEnable = true;
            p_PageOpacity = 1;
            p_LoadingOpacity = 0;
            SetLayerSource();
        }

        private unsafe void Worker_ShowAll_DoWork(object sender, DoWorkEventArgs e)
        {
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
            int originWidth = originRecipe.OriginWidth;
            int originHeight = originRecipe.OriginHeight;

            IntPtr ptrMem = p_ROILayer.GetPtr();
            if (ptrMem == IntPtr.Zero)
                return;

            byte[] buf = new byte[originWidth * p_ROILayer.p_nByte];
            for (int y = 0; y < originHeight; y++)
            {
                Marshal.Copy(buf, 0, (IntPtr)((long)ptrMem + (long)p_ROILayer.p_Size.X * p_ROILayer.p_nByte * y), buf.Length);
            }


            //UInt32 clr = p_SelectedROI.p_Color.A;
            //clr = ((UInt32)clr << 8);
            //clr += p_SelectedROI.p_Color.R;
            //clr = ((UInt32)clr << 8);
            //clr += p_SelectedROI.p_Color.G;
            //clr = ((UInt32)clr << 8);
            //clr += p_SelectedROI.p_Color.B;

            byte* bitmapPtr = (byte*)ptrMem.ToPointer();
            UInt32* fPtr = (UInt32*)bitmapPtr;

            foreach (ItemMask roi in p_cInspROI)
            {
                UInt32 clr = roi.p_Color.A;
                clr = ((UInt32)clr << 8);
                clr += roi.p_Color.R;
                clr = ((UInt32)clr << 8);
                clr += roi.p_Color.G;
                clr = ((UInt32)clr << 8);
                clr += roi.p_Color.B;
                foreach (PointLine data in roi.p_Data)
                {
                    for (int x = data.StartPt.X; x < data.StartPt.X + data.Width; x++)
                    {
                        fPtr[(data.StartPt.Y * p_ROILayer.p_Size.X + x)] += clr;
                    }
                }
            }
        }

        private unsafe void Worker_ReadROI_DoWork(object sender, DoWorkEventArgs e)
        {
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
            int originWidth = originRecipe.OriginWidth;
            int originHeight = originRecipe.OriginHeight;

            IntPtr ptrMem = p_ROILayer.GetPtr();
            if (ptrMem == IntPtr.Zero)
                return;

            byte[] buf = new byte[originWidth * p_ROILayer.p_nByte];
            for (int y = 0; y < originHeight; y++)
            {
                Marshal.Copy(buf, 0, (IntPtr)((long)ptrMem + (long)p_ROILayer.p_Size.X * p_ROILayer.p_nByte * y), buf.Length);
            }


            UInt32 clr = p_SelectedROI.p_Color.A;
            clr = ((UInt32)clr << 8);
            clr += p_SelectedROI.p_Color.R;
            clr = ((UInt32)clr << 8);
            clr += p_SelectedROI.p_Color.G;
            clr = ((UInt32)clr << 8);
            clr += p_SelectedROI.p_Color.B;

            byte* bitmapPtr = (byte*)ptrMem.ToPointer();
            UInt32* fPtr = (UInt32*)bitmapPtr;

            foreach (PointLine data in p_SelectedROI.p_Data)
            {
                for (int x = data.StartPt.X; x < data.StartPt.X + data.Width; x++)
                {
                    fPtr[(data.StartPt.Y * p_ROILayer.p_Size.X + x)] = clr;
                }
            }
        }
        private void Worker_ReadROI_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            p_PageEnable = true;
            p_PageOpacity = 1;
            p_LoadingOpacity = 0;
            SetLayerSource();
        }
        private void Worker_ClearROI_DoWork(object sender, DoWorkEventArgs e)
        {
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
            int originWidth = originRecipe.OriginWidth;
            int originHeight = originRecipe.OriginHeight;


            IntPtr ptrMem = p_ROILayer.GetPtr();
            IntPtr ptrorigin = p_ImageData.GetPtr();
            if (ptrMem == IntPtr.Zero)
                return;

            byte[] buf = new byte[originWidth * p_ROILayer.p_nByte];
            for (int i = 0; i < originHeight; i++)
            {
                Marshal.Copy(buf, 0, (IntPtr)((long)ptrMem + (long)p_ROILayer.p_Size.X * p_ROILayer.p_nByte * i), buf.Length);
            }

            p_SelectedROI.p_Data.Clear();
            SetRecipeData();
        }
        private void Worker_ClearROI_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            p_PageEnable = true;
            p_PageOpacity = 1;
            p_LoadingOpacity = 0;
            SetLayerSource();
            eToolProcess = ToolProcess.None;
            eModifyType = ModifyType.None;
            //BufferInspROI.Clear();
        }
        private unsafe void Worker_SaveROI_DoWork(object sender, DoWorkEventArgs e)
        {
            // 이거 Origin Bounding Box 안에서만 계산하도록 변경 필요?
            // 혹은 ROI Bounding Box

            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
            int originWidth = originRecipe.OriginWidth;
            int originHeight = originRecipe.OriginHeight;

            IntPtr ptrMem = p_ROILayer.GetPtr();
            if (ptrMem == IntPtr.Zero)
                return;
            byte* bitmapPtr = (byte*)ptrMem.ToPointer();
            if (p_SelectedROI == null)
                return;
            p_SelectedROI.p_Data.Clear();
            List<PointLine> tempPointLines = new List<PointLine>();
            bool bStart = false;
            CPoint size = p_ImageData.p_Size;
            int selectIndex = p_cInspROI.IndexOf(p_SelectedROI);
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
            p_SelectedROI.p_Data = tempPointLines;
            SetRecipeData();
        }

        public void SetRecipeData()
        {
            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            if (recipe.GetItem<MaskRecipe>().MaskList.Count != p_cInspROI.Count)
            {
                recipe.GetItem<MaskRecipe>().Clear();
                recipe.GetItem<MaskRecipe>().OriginPoint = this.OriginOffset;
                for (int i = 0; i < p_cInspROI.Count; i++)
                {
                    recipe.GetItem<MaskRecipe>().MaskList.Add(new RecipeType_Mask(p_cInspROI[i].p_Data, p_cInspROI[i].p_Color));
                }
            }
            else
            {
                recipe.GetItem<MaskRecipe>().OriginPoint = this.OriginOffset;
                for (int i = 0; i < p_cInspROI.Count; i++)
                {
                    recipe.GetItem<MaskRecipe>().MaskList[i] = new RecipeType_Mask(p_cInspROI[i].p_Data, p_cInspROI[i].p_Color);
                }
            }

            p_LoadingOpacity = 0;
        }

        private void Worker_SaveROI_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            p_PageEnable = true;
            p_PageOpacity = 1;
            p_LoadingOpacity = 0;
        }
        #endregion

        #region ICommand
        public ICommand LoadedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

                    OriginOffset.X = originRecipe.OriginX;
                    OriginOffset.Y = originRecipe.OriginY - originRecipe.OriginHeight;

                    this.p_LayerMemoryOffsetX = OriginOffset.X;
                    this.p_LayerMemoryOffsetY = OriginOffset.Y;

                    this.DisplayBox();

                    LoadRecipe();
                });
            }
        }
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

        private void _CreateROI()
        {
            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            OriginRecipe origin = recipe.GetItem<OriginRecipe>();

            ItemMask roi = new ItemMask();
            roi.p_Color = Colors.AliceBlue;
            roi.p_Index = p_cInspROI.Count();
            roi.p_FullSize = (long)(origin.OriginWidth) * (long)(origin.OriginHeight);

            p_cInspROI.Add(roi);
            p_SelectedROI = p_cInspROI.Last();

            recipe.GetItem<MaskRecipe>().MaskList.Add(new RecipeType_Mask());
        }
        private void _DeleteROI()
        {
            p_cInspROI.Remove(p_SelectedROI);
            if (p_cInspROI.Count > 0)
                p_SelectedROI = p_cInspROI.Last();
            for (int i = 0; i < p_cInspROI.Count; i++)
            {
                p_cInspROI[i].p_Index = i;
            }
            if (p_cInspROI.Count == 0)
            {
                _ClearROI();
            }
            else
            {
                p_SelectedROI = p_cInspROI.Last();
            }

            SetRecipeData();
        }
        private void _ClearROI()
        {
            if (p_ImageData == null)
                return;

            BufferInspROI.Clear();

            p_PageEnable = false;
            p_PageOpacity = 0.3;
            p_LoadingOpacity = 1;
            Worker_ClearROI.RunWorkerAsync();
        }
        private void _SaveROI()
        {
            if (p_ImageData == null)
                return;

            p_PageEnable = false;
            p_PageOpacity = 0.3;
            p_LoadingOpacity = 1;

            Worker_SaveROI.RunWorkerAsync();
        }
        public void _ReadROI()
        {
            if (Worker_ReadROI.IsBusy)
                return;

            if (p_ImageData == null)
                return;
            p_PageEnable = false;
            p_PageOpacity = 0.3;
            p_LoadingOpacity = 1;
            Worker_ReadROI.RunWorkerAsync();
        }

        public void _ShowAll()
        {
            if (Worker_ShowAll.IsBusy)
                return;

            if (p_ImageData == null)
                return;
            p_PageEnable = false;
            p_PageOpacity = 0.3;
            p_LoadingOpacity = 1;
            Worker_ShowAll.RunWorkerAsync();
        }

        public void Load()
        {
            p_cInspROI.Clear();

            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();
            foreach (RecipeType_Mask mask in recipe.GetItem<MaskRecipe>().MaskList)
            {
                ItemMask roi = new ItemMask();
                roi.p_Color = Colors.AliceBlue;
                roi.p_Index = p_cInspROI.Count();
                roi.p_FullSize = (long)(originRecipe.OriginWidth) * (long)(originRecipe.OriginHeight);

                List<PointLine> pointLines = new List<PointLine>();
                mask.CopyPointLinesTo(ref pointLines);
                roi.p_Data = pointLines;

                p_cInspROI.Add(roi);
            }

            if (p_cInspROI.Count == 0) return;

            p_SelectedROI = p_cInspROI.First();
        }

        public ICommand ShowAll
        {
            get
            {
                return new RelayCommand(_ShowAll);
            }
        }

        public ICommand DeleteROI
        {
            get
            {
                return new RelayCommand(_DeleteROI);
            }
        }
        public ICommand ClearROI
        {
            get
            {
                return new RelayCommand(_ClearROI);
            }
        }
        public ICommand CreateROI
        {
            get
            {
                return new RelayCommand(_CreateROI);
            }
        }
        public ICommand SaveROI
        {
            get
            {
                return new RelayCommand(_SaveROI);
            }
        }

        public ICommand RefCoordItemClickedCommand
        {
            get => new RelayCommand(() =>
            {
                int selectedItem = this.VericalWireVM.SelectedItemIdx;
                
                
                if(selectedItem < this.VericalWireVM.ROIItemOffset)
                {
                    this.SelectedRefCoordItem = selectedItem;
                    eToolProcess = ToolProcess.None;
                    eToolType = ToolType.Rect;
                    eSelectedTeaching = SelectedTeaching.RefCoord;

                    BufferInspROI.Clear();
                    DrawRefCoordElement();
                }
                else
                {
                    this.SelectedROIItem = selectedItem - this.VericalWireVM.ROIItemOffset;
                    eToolProcess = ToolProcess.None;
                    eToolType = ToolType.Rect;
                    eSelectedTeaching = SelectedTeaching.InspROI;

                    BufferInspROI.Clear();

                    VerticalWire_ROIItem roiItem = (vericalWireVM.ROIListItem[this.SelectedRefCoordItem]) as VerticalWire_ROIItem;
                    VerticalWire_ROIItem_ViewModel roiItemItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;
                    DrawInspROIElement(roiItemItem_ViewModel.WireRectList);
                }
            });
        }
        #endregion

        #region Enum
        private enum ToolProcess
        {
            None,
            Drawing,
            Modifying,
        }
        private enum ToolType
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
        public enum SelectedTeaching
        {
            None,
            RefCoord,
            InspROI,
        }
        #endregion
    }
}
