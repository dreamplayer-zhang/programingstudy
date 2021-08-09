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
        ObservableCollection<ParameterBase> MeasurementClass;

        // Measurement
        public readonly RootTools_Vision.VerticalWire_RecipeTeaching verticalWire = new RootTools_Vision.VerticalWire_RecipeTeaching();
        public readonly RootTools_Vision.PBI_RecipeTeaching pbi = new RootTools_Vision.PBI_RecipeTeaching();

        private class DefineColors
        {
            public static SolidColorBrush OriginBoxColor = Brushes.Blue;
            public static SolidColorBrush RefCoordBoxColor = Brushes.Yellow;
            public static SolidColorBrush RefCoordCrossColor = Brushes.Magenta;

            public static SolidColorBrush InspROIPointColor = Brushes.Cyan;
            public static SolidColorBrush FirstInspROIPointColor = Brushes.Red;
        }
        /// <summary>
        /// 전체 Memory의 좌표와 ROI Memory 좌표의 Offset
        /// </summary>
        CPoint OriginOffset = new CPoint();

        Grid OriginBox;

        TShape CurrentShape;
        TShape CropShape;

        ToolProcess eToolProcess;
        ToolType eToolType;
        VerticalWireTeaching eSelectedTeaching;
        PBITeaching ePBITeaching;

        private readonly VerticalWire_RecipeTeaching_ViewModel vericalWireVM;
        public VerticalWire_RecipeTeaching_ViewModel VericalWireVM
        {
            get => this.vericalWireVM;
        }

        private readonly PBI_RecipeTeaching_ViewModel pbiVM;
        public PBI_RecipeTeaching_ViewModel PBIVM
        {
            get => this.pbiVM;
        }

        public FrontsideMeasurement_ViewModel()
        {
            if (GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr() == IntPtr.Zero && GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").m_eMode != ImageData.eMode.OtherPCMem)
                return;

            base.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());
            p_VisibleMenu = Visibility.Collapsed;

            this.MeasurementClass = ParameterBase.GetMeasurementClass();

            this.MeasurementList = new ObservableCollection<string>();
            foreach (ParameterBase pb in this.MeasurementClass)
                MeasurementList.Add(pb.Name);

            this.vericalWireVM = new VerticalWire_RecipeTeaching_ViewModel();
            this.vericalWireVM.InspCollectionChanged += InspROIItem_CollectionChanged;
            this.vericalWireVM.RefCollectionChanged += RefCoordItem_CollectionChanged;
            this.vericalWireVM.RecipeSave += VerticalWire_RecipeSave;
           
            this.pbiVM = new PBI_RecipeTeaching_ViewModel();
            this.pbiVM.RecipeSave += PBI_RecipeSave;

            BufferInspROI.CollectionChanged += BufferInspROI_CollectionChanged;

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
            this.SelectedItem = 0;
            VerticalWire_RecipeLoad();
            PBI_RecipeLoad();
        }

        public void ChangePage()
        {
            if (MeasurementList[SelectedMode] == "VerticalWire")
            {
                SetPage(verticalWire);
                verticalWire.DataContext = VericalWireVM;
            }
            else if (MeasurementList[SelectedMode] == "PBI")
            {
                {
                    SetPage(pbi);
                    pbi.DataContext = PBIVM;
                }
            }
        }

        public void SetPage(UserControl page)
        {
            CurrentPanel = page;
        }

        #region Property
        private UserControl currentPanel;
        public UserControl CurrentPanel
        {
            get
            {
                return currentPanel;
            }
            set
            {
                SetProperty(ref currentPanel, value);
            }
        }

        private int selectedItem;
        public int SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                SetProperty(ref selectedItem, value);
            }
        }

        private int selectedMode = 0;
        public int SelectedMode
        {
            get
            {
                return selectedMode;
            }
            set
            {
                SetProperty(ref selectedMode, value);
                ChangePage();
            }
        }

        private ObservableCollection<string> mrrangeMethod;
        public ObservableCollection<string> MeasurementList
        {
            get => this.mrrangeMethod;
            set
            {
                mrrangeMethod = value;
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
                    DrawDone(eToolType, memPt, canvasPt);
                    eToolProcess = ToolProcess.None;
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
                    break;
                case ToolType.Line:
                    break;
                case ToolType.Rect:
                    StartDrawRect(color, thickness, opacity, startMemPt, startCanvasPt);
                    break;
                case ToolType.Circle:
                    break;
                case ToolType.Polygon:
                    break;
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
                    break;
                case ToolType.Line:
                    break;
                case ToolType.Rect:
                    DrawingRect(startMemPt, startCanvasPt);
                    break;
                case ToolType.Circle:
                    break;
                case ToolType.Polygon:
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
                    break;
                case ToolType.Line:
                    break;
                case ToolType.Rect:
                    DrawDoneRect(startMemPt, startCanvasPt);
                    break;
                case ToolType.Circle:
                    break;
                case ToolType.Polygon:
                    break;
            }
        }
        #endregion

        #region [DrawMethod]
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
            BufferInspROI.Clear();

            if (MeasurementList[SelectedMode] == "VerticalWire")
                DrawDoneRect_VerticalWire(currentMemPt, currentCanvasPt);
            else if (MeasurementList[SelectedMode] == "PBI")
                DrawDoneRect_PBI(currentMemPt, currentCanvasPt);

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
                        break;
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

                    this.DisplayBox();
                    ChangePage();
                    LoadRecipe();
                });
            }
        }
        public ICommand UnloadedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    VerticalWire_RecipeSave();
                    PBI_RecipeSave();
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

        public ICommand ControlClickedCommand
        {
            get => new RelayCommand(() =>
            {
                if (MeasurementList[SelectedMode] == "VerticalWire")
                {
                    VerticalWire_ControlClicked();
                }
                else if (MeasurementList[SelectedMode] == "PBI")
                {
                    PBI_ControlClicked();
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
        #endregion

        // Measurement Teaching Method
        #region[Vertical Wire Teaching]
        private void DrawDoneRect_VerticalWire(CPoint currentMemPt, CPoint currentCanvasPt)
        {
            TRect rect = CurrentShape as TRect;

            if (eSelectedTeaching == VerticalWireTeaching.RefCoord)
            {
                VerticalWire_RefCoordItem refCoordItem = (vericalWireVM.RefCoordListItem[this.SelectedItem]) as VerticalWire_RefCoordItem;
                VerticalWire_RefCoordItem_ViewModel refCoordItem_ViewModel = refCoordItem.DataContext as VerticalWire_RefCoordItem_ViewModel;

                refCoordItem_ViewModel.RefX = rect.MemoryRect.Left;
                refCoordItem_ViewModel.RefY = rect.MemoryRect.Top;
                refCoordItem_ViewModel.RefW = rect.MemoryRect.Width;
                refCoordItem_ViewModel.RefH = rect.MemoryRect.Height;

                switch (refCoordItem_ViewModel.eArrangeType)
                {
                    case VerticalWire_RefCoordItem_ViewModel.ArrangeType.LT:
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
                        refCoordItem_ViewModel.RefCoord = new CPoint((rect.MemoryRect.Left + rect.MemoryRect.Right) / 2, (rect.MemoryRect.Top + rect.MemoryRect.Bottom) / 2);
                        break;
                }
                DrawRefCoordElement();
            }

            else if (eSelectedTeaching == VerticalWireTeaching.InspROI)
            {
                VerticalWire_ROIItem roiItem = (vericalWireVM.ROIListItem[this.SelectedItem]) as VerticalWire_ROIItem;
                VerticalWire_ROIItem_ViewModel roiItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;

                int roiW = rect.MemoryRect.Right - rect.MemoryRect.Left;
                int roiH = rect.MemoryRect.Bottom - rect.MemoryRect.Top;

                IntPtr imgPtr = GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr(roiItem_ViewModel.SelectedChannel);
                long stride = GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").p_Stride;

                byte[] roiBuffer = new byte[roiW * roiH];
                int idx = 0;

                unsafe
                {
                    byte* p = (byte*)imgPtr.ToPointer();
                    for (Int64 r = rect.MemoryRect.Top; r < rect.MemoryRect.Bottom; r++)
                    {
                        for (Int64 c = rect.MemoryRect.Left; c < (long)rect.MemoryRect.Right; c++)
                        {
                            roiBuffer[idx++] = p[r * stride + c];
                        }
                    }

                    CLR_IP.Cpp_Threshold(roiBuffer, roiBuffer, roiW, roiH, true, roiItem_ViewModel.Threshold);
                    var detectList = CLR_IP.Cpp_Labeling(roiBuffer, roiBuffer, roiW, roiH);

                    roiItem_ViewModel.WirePointList.Clear();

                    foreach (Cpp_LabelParam list in detectList)
                    {
                        //if ((list.area > roiItem_ViewModel.WireSize - 10) && (list.area < roiItem_ViewModel.WireSize + 10))
                        if (list.area > 5)
                        {
                            Point tmpPoint = new Point();
                            tmpPoint.X = rect.MemoryRect.Left + list.centerX;
                            tmpPoint.Y = rect.MemoryRect.Top + list.centerY;

                            roiItem_ViewModel.WirePointList.Add(tmpPoint);
                        }
                    }

                    if (roiItem_ViewModel.WirePointList.Count > 50)
                    {
                        MessageBox.Show("비정상적으로 많은 Vertical Wire가  검출 되었습니다.", "Error");
                    }
                    else
                    {
                        roiItem_ViewModel.ArrangeDetectPoint();
                        DrawInspROIElement(roiItem_ViewModel.WirePointList);
                    }
                }
            }
        }
        void DrawRefCoordElement()
        {
            VerticalWire_RefCoordItem refCoordItem = (vericalWireVM.RefCoordListItem[this.SelectedItem]) as VerticalWire_RefCoordItem;
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

        void DrawInspROIElement(List<Point> detectList)
        {
            bool isFirst = true;
            foreach (Point pt in detectList)
            {
                TShape rectRefCoordCross1, rectRefCoordCross2;
                if (isFirst)
                {
                    rectRefCoordCross1 = new TLine(DefineColors.FirstInspROIPointColor, 3, 1);
                    rectRefCoordCross2 = new TLine(DefineColors.FirstInspROIPointColor, 3, 1);
                    isFirst = false;
                }
                else
                {
                    rectRefCoordCross1 = new TLine(DefineColors.InspROIPointColor, 3, 1);
                    rectRefCoordCross2 = new TLine(DefineColors.InspROIPointColor, 3, 1);
                }

                TLine line1 = rectRefCoordCross1 as TLine;
                line1.MemoryStartPoint = new CPoint((int)pt.X - 10, (int)pt.Y - 10);
                line1.MemoryEndPoint = new CPoint((int)pt.X + 10, (int)pt.Y + 10);

                BufferInspROI.Add(rectRefCoordCross1);
                RedrawLine(line1);

                TLine line2 = rectRefCoordCross2 as TLine;
                line2.MemoryStartPoint = new CPoint((int)pt.X + 10, (int)pt.Y - 10);
                line2.MemoryEndPoint = new CPoint((int)pt.X - 10, (int)pt.Y + 10);

                BufferInspROI.Add(rectRefCoordCross2);
                RedrawLine(line2);
            }
        }
        void InspROIItem_CollectionChanged()
        {
            BufferInspROI.Clear();

            VerticalWire_ROIItem roiItem = (vericalWireVM.ROIListItem[this.SelectedItem]) as VerticalWire_ROIItem;
            VerticalWire_ROIItem_ViewModel roiItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;
            DrawInspROIElement(roiItem_ViewModel.WirePointList);
        }
        void RefCoordItem_CollectionChanged()
        {
            BufferInspROI.Clear();
            DrawRefCoordElement();
        }
        void VerticalWire_RecipeSave()
        {
            VerticalWireRecipe recipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<VerticalWireRecipe>();
            if (recipe == null)
                return;
            recipe.Clear(); 

            IntPtr imgPtrR = GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr(0);
            IntPtr imgPtrG = GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr(1);
            IntPtr imgPtrB = GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr(2);
            long stride = GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").p_Stride;

            foreach (UIElement item in vericalWireVM.RefCoordListItem)
            {
                VerticalWire_RefCoordItem refItem = item as VerticalWire_RefCoordItem;
                VerticalWire_RefCoordItem_ViewModel refItem_ViewModel = refItem.DataContext as VerticalWire_RefCoordItem_ViewModel;

                Rect rt = new Rect();
                rt.X = refItem_ViewModel.RefX;
                rt.Y = refItem_ViewModel.RefY;
                rt.Width = refItem_ViewModel.RefW;
                rt.Height = refItem_ViewModel.RefH;

                recipe.RefCoordArrageMethod.Add(refItem_ViewModel.SelectedArrageMethod);
                recipe.RefCoord.Add(rt);

                byte[] roiBuffer = new byte[(int)rt.Width * (int)rt.Height * 3];

                unsafe
                {
                    byte* r = (byte*)imgPtrR.ToPointer();
                    byte* g = (byte*)imgPtrG.ToPointer();
                    byte* b = (byte*)imgPtrB.ToPointer();

                    Parallel.For((int)rt.Y, ((int)rt.Y + (int)rt.Height), row =>
                    //for(int row = (int)rt.Y; row < (int)rt.Y + rt.Height; row++)
                    {
                        int idx = (row - (int)rt.Y) * (int)(rt.Width * 3);
                        for (int col = (int)rt.X; col < rt.X + rt.Width; col++)
                        {
                            roiBuffer[idx++] = r[(Int64)row * stride + col];
                            roiBuffer[idx++] = g[(Int64)row * stride + col];
                            roiBuffer[idx++] = b[(Int64)row * stride + col];
                        }
                    });

                    recipe.RawData.Add(roiBuffer);
                }
            }

            foreach (UIElement item in vericalWireVM.ROIListItem)
            {
                VerticalWire_ROIItem roiItem = item as VerticalWire_ROIItem;
                VerticalWire_ROIItem_ViewModel roiItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;

                recipe.InspPoint.Add(roiItem_ViewModel.WirePointList);
                recipe.InspROISelectedCoord.Add(roiItem_ViewModel.SelectedRefCoord);
                recipe.InspROIArrageMethod.Add(roiItem_ViewModel.SelectedArrageMethod);
            }
        }

        void VerticalWire_RecipeLoad()
        {
            VerticalWireRecipe recipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<VerticalWireRecipe>();
            if (recipe == null)
                return;
            if (recipe.RefCoord.Count == 0)
            {
                vericalWireVM.ChipNuminOrigin = 1;
                vericalWireVM.AddRefItem();
                vericalWireVM.AddROIItem();
                return;
            }

            vericalWireVM.RefCoordListItem.Clear();
            vericalWireVM.ChipNuminOrigin = 0;

            for (int i = 0; i < recipe.RefCoord.Count; i++)
            {
                vericalWireVM.ChipNuminOrigin++;
                vericalWireVM.AddRefItem();

                VerticalWire_RefCoordItem refItem = vericalWireVM.RefCoordListItem[i] as VerticalWire_RefCoordItem;
                VerticalWire_RefCoordItem_ViewModel refItem_ViewModel = refItem.DataContext as VerticalWire_RefCoordItem_ViewModel;

                refItem_ViewModel.RefX = (int)recipe.RefCoord[i].X;
                refItem_ViewModel.RefY = (int)recipe.RefCoord[i].Y;
                refItem_ViewModel.RefW = (int)recipe.RefCoord[i].Width;
                refItem_ViewModel.RefH = (int)recipe.RefCoord[i].Height;

                refItem_ViewModel.SelectedArrageMethod = recipe.RefCoordArrageMethod[i];
            }
             
            vericalWireVM.ROIListItem.Clear();
            for (int i = 0; i < recipe.InspROISelectedCoord.Count; i++)
            {
                vericalWireVM.AddROIItem();

                VerticalWire_ROIItem roiItem = vericalWireVM.ROIListItem[i] as VerticalWire_ROIItem;
                VerticalWire_ROIItem_ViewModel roiItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;

                roiItem_ViewModel.RefCoordNum = vericalWireVM.ChipNuminOrigin;
                roiItem_ViewModel.WirePointList = recipe.InspPoint[i];
                roiItem_ViewModel.SelectedRefCoord = recipe.InspROISelectedCoord[i];
                roiItem_ViewModel.SelectedArrageMethod = recipe.InspROIArrageMethod[i];
            }

            this.VericalWireVM.RefItemIdx = 0;
            this.VericalWireVM.ROIItemIdx = 0;
        }

        public void VerticalWire_ControlClicked()
        {
            int selectedItem = this.VericalWireVM.SelectedItemIdx;

            if (selectedItem < this.VericalWireVM.ROIItemOffset)
            {
                this.SelectedItem = selectedItem;
                eToolProcess = ToolProcess.None;
                eToolType = ToolType.Rect;
                eSelectedTeaching = VerticalWireTeaching.RefCoord;

                BufferInspROI.Clear();
                if (this.SelectedItem >= 0)
                    DrawRefCoordElement();
            }
            else
            {
                this.SelectedItem = selectedItem - this.VericalWireVM.ROIItemOffset;
                eToolProcess = ToolProcess.None;
                eToolType = ToolType.Rect;
                eSelectedTeaching = VerticalWireTeaching.InspROI;

                BufferInspROI.Clear();

                VerticalWire_ROIItem roiItem = (vericalWireVM.ROIListItem[this.SelectedItem]) as VerticalWire_ROIItem;
                VerticalWire_ROIItem_ViewModel roiItem_ViewModel = roiItem.DataContext as VerticalWire_ROIItem_ViewModel;

                if (this.SelectedItem >= 0)
                    DrawInspROIElement(roiItem_ViewModel.WirePointList);
            }
        }

        public enum VerticalWireTeaching
        {
            None,
            RefCoord,
            InspROI,
        }
        #endregion

        #region [PBI Teaching]

        private void DrawDoneRect_PBI(CPoint currentMemPt, CPoint currentCanvasPt)
        {
            TRect rect = CurrentShape as TRect;

            if (ePBITeaching == PBITeaching.Feature)
            {
                PBI_FeatureItem featureItem = (PBIVM.FeatureListItem[this.SelectedItem]) as PBI_FeatureItem;
                PBI_FeatureItem_ViewModel featureItem_ViewModel = featureItem.DataContext as PBI_FeatureItem_ViewModel;

                int roiW = rect.MemoryRect.Right - rect.MemoryRect.Left;
                int roiH = rect.MemoryRect.Bottom - rect.MemoryRect.Top;

                featureItem_ViewModel.RefX = rect.MemoryRect.Left;
                featureItem_ViewModel.RefY = rect.MemoryRect.Top;
                featureItem_ViewModel.RefW = rect.MemoryRect.Width;
                featureItem_ViewModel.RefH = rect.MemoryRect.Height;

                DrawPBIFeatureElement();
            }
        }

        void DrawPBIFeatureElement()
        {
            PBI_FeatureItem featureItem = (PBIVM.FeatureListItem[this.SelectedItem]) as PBI_FeatureItem;
            PBI_FeatureItem_ViewModel featureItem_ViewModel = featureItem.DataContext as PBI_FeatureItem_ViewModel;

            TShape rectRefCoordBox = new TRect(DefineColors.RefCoordBoxColor, 2, 0.5);
            TRect rect = rectRefCoordBox as TRect;

            rect.MemoryRect.Left = featureItem_ViewModel.RefX;
            rect.MemoryRect.Top = featureItem_ViewModel.RefY;
            rect.MemoryRect.Right = featureItem_ViewModel.RefX + featureItem_ViewModel.RefW;
            rect.MemoryRect.Bottom = featureItem_ViewModel.RefY + featureItem_ViewModel.RefH;

            BufferInspROI.Add(rectRefCoordBox);
            RedrawRect(rect);         
        }
        public void PBI_ControlClicked()
        {
            eToolProcess = ToolProcess.None;
            eToolType = ToolType.Rect;
            ePBITeaching = PBITeaching.Feature;

            this.SelectedItem = PBIVM.SelectedItemIdx;

            BufferInspROI.Clear();
            if (this.PBIVM.SelectedItemIdx >= 0)
                DrawPBIFeatureElement();

        }

        public void PBI_RecipeSave()
        {
            PBIRecipe recipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<PBIRecipe>();
            if (recipe == null)
                return;
            recipe.Clear();

            IntPtr imgPtrR = GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr(0);
            IntPtr imgPtrG = GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr(1);
            IntPtr imgPtrB = GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr(2);
            long stride = GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").p_Stride;

            foreach (UIElement item in PBIVM.FeatureListItem)
            {
                PBI_FeatureItem featureItem = item as PBI_FeatureItem;
                PBI_FeatureItem_ViewModel featureItem_ViewModel = featureItem.DataContext as PBI_FeatureItem_ViewModel;

                Rect rt = new Rect();
                rt.X = featureItem_ViewModel.RefX;
                rt.Y = featureItem_ViewModel.RefY;
                rt.Width = featureItem_ViewModel.RefW;
                rt.Height = featureItem_ViewModel.RefH;

                recipe.FeatureInfo.Add(rt);

                byte[] roiBuffer = new byte[(int)rt.Width * (int)rt.Height * 3];

                unsafe
                {
                    byte* r = (byte*)imgPtrR.ToPointer();
                    byte* g = (byte*)imgPtrG.ToPointer();
                    byte* b = (byte*)imgPtrB.ToPointer();

                    Parallel.For((int)rt.Y, ((int)rt.Y + (int)rt.Height), row =>
                    {
                        int idx = (row - (int)rt.Y) * (int)(rt.Width * 3);
                        for (int col = (int)rt.X; col < rt.X + rt.Width; col++)
                        {
                            roiBuffer[idx++] = r[(Int64)row * stride + col];
                            roiBuffer[idx++] = g[(Int64)row * stride + col];
                            roiBuffer[idx++] = b[(Int64)row * stride + col];
                        }
                    });

                    recipe.RawData.Add(roiBuffer);
                }
            }
        }

        public void PBI_RecipeLoad()
        {
            PBIRecipe recipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<PBIRecipe>();
            if (recipe == null)
                return;
            if (recipe.FeatureInfo.Count == 0)
            {
                PBIVM.AddFeatureItem();
                return;
            }

            PBIVM.FeatureListItem.Clear();
            vericalWireVM.ChipNuminOrigin = 0;

            for (int i = 0; i < recipe.FeatureInfo.Count; i++)
            {
                PBIVM.AddFeatureItem();

                PBI_FeatureItem featureItem = (PBIVM.FeatureListItem[i]) as PBI_FeatureItem;
                PBI_FeatureItem_ViewModel featureItem_ViewModel = featureItem.DataContext as PBI_FeatureItem_ViewModel;

                featureItem_ViewModel.RefX = (int)recipe.FeatureInfo[i].X;
                featureItem_ViewModel.RefY = (int)recipe.FeatureInfo[i].Y;
                featureItem_ViewModel.RefW = (int)recipe.FeatureInfo[i].Width;
                featureItem_ViewModel.RefH = (int)recipe.FeatureInfo[i].Height;
            }
        }

        public enum PBITeaching
        {
            None,
            Feature,
        }
        #endregion
    }
}
