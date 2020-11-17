using RootTools;
using RootTools_Vision;
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

namespace Root_WIND2
{
    class FrontsideROI_ViewModel : RootViewer_ViewModel
    {
        Recipe m_Recipe;
        /// <summary>
        /// 전체 Memory의 좌표와 ROI Memory 좌표의 Offset
        /// </summary>
        CPoint BoxOffset = new CPoint();

        TShape CurrentShape;
        TShape SelectShape;      

        CPoint ModifyPointBuffer = new CPoint();

        ToolProcess eToolProcess;
        ToolType eToolType;
        ModifyType eModifyType;

        public void Init(Setup_ViewModel setup, Recipe recipe)
        {
            base.init();
            p_VisibleMenu = Visibility.Collapsed;

            BufferInspROI.CollectionChanged += BufferInspROI_CollectionChanged;
            SetBackGroundWorker();

            p_ROILayer = setup.m_MainWindow.m_ROILayer;
            m_Recipe = recipe;
        }
        public void SetOrigin(object e)
        {
            TRect InspAreaBuf = e as TRect;
            BoxOffset = new CPoint(InspAreaBuf.MemoryRect.Left, InspAreaBuf.MemoryRect.Top);
            ImageData OriginImageData = InspAreaBuf.Tag as ImageData;
            if (OriginImageData.p_Size.X == 0 || OriginImageData.p_Size.Y == 0)
            {
                p_ImgSource = null;
            }
            else
            {
                p_ImageData = OriginImageData;
                base.SetRoiRect();
            }          
        }

        #region Property
        /// <summary>
        /// ROI UI Shapes
        /// </summary>
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
        private ObservableCollection<UIElement> m_UIElements = new ObservableCollection<UIElement>();
        /// <summary>
        /// ROI List
        /// </summary>
        public ObservableCollection<InspectionROI> p_cInspROI
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
        private ObservableCollection<InspectionROI> m_cInspROI = new ObservableCollection<InspectionROI>();
        /// <summary>
        /// Selected ROI
        /// </summary>
        public InspectionROI p_SelectedROI
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
                _ReadROI();
            }
        }
        private InspectionROI m_SelectedROI;
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
        /// Selected Tool Index
        /// </summary>
        public int p_SelectedToolIndex
        {
            get
            {
                return m_SelectedToolIndex;
            }
            set
            {
                eToolProcess = ToolProcess.None;
                eToolType = (ToolType)value;
                SetProperty(ref m_SelectedToolIndex, value);
            }
        }
        private int m_SelectedToolIndex = 0;
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
        #endregion

        #region Stack History
        private Stack<TShape[]> History = new Stack<TShape[]>();
        public ObservableCollection<TShape> BufferInspROI = new ObservableCollection<TShape>();
        private void BufferInspROI_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {           
            var shapes = sender as ObservableCollection<TShape>;
            foreach (TShape shape in shapes)
            {
                if (!p_UIElements.Contains(shape.UIElement))
                    p_UIElements.Add(shape.UIElement);
            }

            if (shapes.Count() == 0)
                p_UIElements.Clear();

            TShape[] Work = new TShape[20];
            shapes.CopyTo(Work, 0);

            History.Push(Work);
        }
        #endregion

        #region Override
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftCtrl && m_KeyEvent.IsDown)
                    return;

            CPoint memPt = new CPoint(p_MouseMemX, p_MouseMemY);
            CPoint canvasPt = GetCanvasPoint(memPt - BoxOffset);
            switch (eToolProcess)
            {
                case ToolProcess.None:
                    if (eToolType != ToolType.None)
                    {
                        if (BufferInspROI.Contains(SelectShape))
                            BufferInspROI.Remove(SelectShape);

                        StartDraw(eToolType, memPt, canvasPt);
                        if (eToolType != ToolType.Select)
                        {
                            BufferInspROI.Add(CurrentShape);
                        }
                        else
                        {
                            BufferInspROI.Add(SelectShape);
                        }
                        eToolProcess = ToolProcess.Drawing;
                        break;
                    }
                    if (eModifyType != ModifyType.None)
                    {
                        StartModify(memPt);
                        break;
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
                        eToolType = ToolType.None;
                        p_SelectedToolIndex = 0;
                    }
                    break;
                case ToolProcess.Modifying:
                    break;
            }
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            p_MouseMemX += BoxOffset.X;
            p_MouseMemY += BoxOffset.Y;

            CPoint memPt = new CPoint(p_MouseMemX, p_MouseMemY);
            CPoint canvasPt = GetCanvasPoint(memPt - BoxOffset);

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
                case ToolType.Select:
                    StartDrawSelectRect(Brushes.Red, thickness, 1, startMemPt, startCanvasPt);
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
                CurrentShape.UIElement.MouseEnter += UIElement_MouseEnter;
                CurrentShape.UIElement.MouseLeave += UIElement_MouseLeave;
                CurrentShape.UIElement.MouseLeftButtonDown += UIElement_MouseLeftButtonDown;
            }
        }
        private void Drawing(ToolType type, CPoint startMemPt, CPoint startCanvasPt)
        {
            switch (type)
            {
                case ToolType.None:
                    break;
                case ToolType.Select:
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
                case ToolType.Select:
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
            //foreach (CPoint pt in line.Data)
            //{
            //    base.DrawPixelBitmap(pt - BoxOffset, 255, 0, 0, 255);
            //}
            //base.SetLayerSource();
        }

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

            CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
            CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);
            CPoint canvasLT = new CPoint(GetCanvasPoint(LT - BoxOffset));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB - BoxOffset));

            Canvas.SetLeft(rect.CanvasRect, canvasLT.X);
            Canvas.SetTop(rect.CanvasRect, canvasLT.Y);

            rect.CanvasRect.Width = Math.Abs(canvasRB.X - canvasLT.X);
            rect.CanvasRect.Height = Math.Abs(canvasRB.Y - canvasLT.Y);
        }
        private void DrawDoneRect(CPoint currentMemPt, CPoint currentCanvasPt)
        {
            TRect rect = CurrentShape as TRect;
            rect.CanvasRect.Fill = rect.FillBrush;
            //rect.SetData();
            CreateModifyTool_Rect(rect);

            ContextMenu cMenu = new ContextMenu();
            MenuItem menuAdd = new MenuItem();
            menuAdd.Header = "Add";
            menuAdd.Click += MenuAdd_Click;
            MenuItem menuDelete = new MenuItem();
            menuDelete.Header = "Delete";
            menuDelete.Click += MenuDelete_Click;

            cMenu.Items.Add(menuAdd);
            cMenu.Items.Add(menuDelete);

            rect.CanvasRect.ContextMenu = cMenu;
        }

        private void StartDrawSelectRect(Brush color, double thickness, double opacity, CPoint startMemPt, CPoint startCanvasPt)
        {
            SelectShape = new TRect(color, thickness, opacity);
            TRect rect = SelectShape as TRect;
            rect.CanvasRect.StrokeDashArray = new DoubleCollection { 4, 1 };
            rect.MemPointBuffer = startMemPt;
            rect.MemoryRect.Left = startMemPt.X;
            rect.MemoryRect.Top = startMemPt.Y;
        }
        private void DrawingSelectRect(CPoint currentMemPt, CPoint currentCanvasPt)
        {
            TRect rect = SelectShape as TRect;

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

            CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
            CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);
            CPoint canvasLT = new CPoint(GetCanvasPoint(LT - BoxOffset));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB - BoxOffset));

            Canvas.SetLeft(rect.CanvasRect, canvasLT.X);
            Canvas.SetTop(rect.CanvasRect, canvasLT.Y);

            rect.CanvasRect.Width = Math.Abs(canvasRB.X - canvasLT.X);
            rect.CanvasRect.Height = Math.Abs(canvasRB.Y - canvasLT.Y);
        }
        private void DrawDoneSelectRect(CPoint currentMemPt, CPoint currentCanvasPt)
        {
            TRect rect = SelectShape as TRect;
            rect.CanvasRect.Fill = Brushes.Transparent;
            rect.CanvasRect.Cursor = Cursors.ScrollAll;
            rect.CanvasRect.MouseEnter += SelectRect_MouseEnter;
            rect.CanvasRect.MouseLeave += SelectRect_MouseLeave;
            var asdf = p_ImageData;
            CRect nowRect = new CRect(rect.MemoryRect.Left-BoxOffset.X, rect.MemoryRect.Top - BoxOffset.Y, rect.MemoryRect.Right - BoxOffset.X, rect.MemoryRect.Bottom- BoxOffset.Y);
            //CRect nowRect = new CRect(rect.MemoryRect.Left - , )
            //CRect nowRect = new CRect(rect.MemoryRect.Left, rect.MemoryRect.Top, rect.MemoryRect.Right, rect.MemoryRect.Bottom);
            //ImageData rectImageData = new ImageData(rect.MemoryRect.Width, rect.MemoryRect.Height, 1);
            ImageData rectImageData = new ImageData(nowRect.Width, nowRect.Height, 4);
            


            //rectImageData.SetData(p_ImageData.GetPtr(), nowRect, (int)p_ImageData.p_Stride, 1);
            rectImageData.SetData(p_ROILayer.GetPtr(), nowRect, (int)p_ROILayer.p_Stride, 4);


            Image imageData = new Image();
            imageData.Width = 200;
            imageData.Height = 200;
            imageData.Opacity = 0.5;
            
            
            imageData.Source = rectImageData.GetBitMapSource(4);
            var fffds = rectImageData.GetBitMapSource(4).PixelWidth;
            p_UIElements.Add(imageData);
            var adsf =rectImageData.m_aBuf;
            //System.Drawing.Bitmap bitmap = rectImageData.GetBitmapToArray(rectImageData.p_Size.X, rectImageData.p_Size.Y, rectImageData.m_aBuf);
            

            // Rect만큼 ImageLayer 가져와서
            // 복사해서 CanvasRect안에 뿌리고
            // 이동..
            // 마우스 Up
            // ImageLayer에 CanvasRect안의 Data Set

            //그냥 캔버스위에 픽셀그리듯이 칠하고...
            //선택해서 이동/복사는...
        }

        private void StartDrawPolygon(Brush color, double thickness, double opacity, CPoint startMemPt, CPoint startCanvasPt)
        {
            CurrentShape = new TPolygon(color, thickness, opacity);
            TPolygon polygon = CurrentShape as TPolygon;
            polygon.CanvasPolygon.Visibility = Visibility.Hidden;
            p_UIElements.Add(polygon.CanvasPolyLine);
           
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
            p_UIElements.Remove(polygon.CanvasPolyLine);

            CreateModifyTool_Polygon(polygon);
        }

        private void MenuAdd_Click(object sender, RoutedEventArgs e)
        {
            byte r = p_SelectedROI.p_Color.R;
            byte g = p_SelectedROI.p_Color.G;
            byte b = p_SelectedROI.p_Color.B;
            DrawRectBitmap(CurrentShape as TRect, r, g, b, 255, BoxOffset);
            BufferInspROI.Clear();
            SetLayerSource();
        }
        private void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            DrawRectBitmap(CurrentShape as TRect, 0, 0, 0, 0, BoxOffset);
            BufferInspROI.Clear();
            SetLayerSource();
        }
        #endregion

        #region Modify Method
        private void UIElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftCtrl && m_KeyEvent.IsDown)
                    return;
            if (p_SelectedToolIndex == 0 )
            {
                TShape shape = (sender as Shape).Tag as TShape;
                if (shape.isSelected)
                {
                    shape.isSelected = false;
                }
                else
                {
                    shape.isSelected = true;
                }
            }
        }
        private void UIElement_MouseEnter(object sender, MouseEventArgs e)
        {
            if(p_SelectedToolIndex == 0)
                p_Cursor = Cursors.Hand;
        }
        private void UIElement_MouseLeave(object sender, MouseEventArgs e)
        {
            p_Cursor = Cursors.Arrow;
        }

        private void StartModify(CPoint currentMemPt)
        {
            ModifyPointBuffer = currentMemPt - BoxOffset;
            eToolProcess = ToolProcess.Modifying;
        }
        private void ModifyingShape(CPoint currentMemPt)
        {
            if (SelectShape != null)
            {
                Modifying_SelectRect(SelectShape as TRect, currentMemPt - BoxOffset);
            }
            foreach (TShape shape in BufferInspROI)
            {
                switch (shape)
                {
                    case TLine:
                        Modifying_Line(shape as TLine, currentMemPt - BoxOffset);
                        break;
                    case TRect:
                        Modifying_Rect(shape as TRect, currentMemPt - BoxOffset);
                        break;
                    case TPolygon:
                        Modifying_Polygon(shape as TPolygon, currentMemPt - BoxOffset);
                        break;
                }
            }
            ModifyPointBuffer = currentMemPt - BoxOffset;


        }
        private void ModifyDone()
        {
            foreach (TShape shape in BufferInspROI)
            {
                if (shape.isSelected)
                {
                    if (p_UIElements.Contains(shape.ModifyTool))
                        p_UIElements.Remove(shape.ModifyTool);

                    switch (shape)
                    {
                        case TLine:
                            CreateModifyTool_Line(shape as TLine);
                            break;
                        case TRect:
                            CreateModifyTool_Rect(shape as TRect);
                            TRect rect = shape as TRect;
                            //rect.SetData();
                            break;
                        case TPolygon:
                            CreateModifyTool_Polygon(shape as TPolygon);
                            break;
                    }
                    shape.ModifyTool.Visibility = Visibility.Visible;
                }
            }
            p_Cursor = Cursors.Arrow;
            eToolProcess = ToolProcess.None;
            eModifyType = ModifyType.None;
            if(SelectShape != null)
                if (SelectShape.UIElement.IsMouseOver)
                    eModifyType = ModifyType.ScrollAll;
        }

        private void CreateModifyTool_Line(TLine line)
        {
            double left, top, width, height;
            CPoint StartPt = GetCanvasPoint(line.MemoryStartPoint - BoxOffset);
            CPoint EndPt = GetCanvasPoint(line.MemoryEndPoint - BoxOffset);

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
            p_UIElements.Add(ModifyGrid);
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
            p_UIElements.Add(rect.ModifyTool);
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
            p_UIElements.Add(polygon.ModifyTool);
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
            if(e.LeftButton == MouseButtonState.Released)
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
        private void Modifying_SelectRect(TRect rect, CPoint currentMemPt)
        {
            int offset_x = currentMemPt.X - ModifyPointBuffer.X;
            int offset_y = currentMemPt.Y - ModifyPointBuffer.Y;
            CPoint ptOffset = new CPoint(offset_x, offset_y);

            int left, top, right, bottom;
            left = rect.MemoryRect.Left;
            top = rect.MemoryRect.Top;
            right = rect.MemoryRect.Right;
            bottom = rect.MemoryRect.Bottom;

            left += ptOffset.X;
            top += ptOffset.Y;
            right += ptOffset.X;
            bottom += ptOffset.Y;
            p_Cursor = Cursors.Cross;

            rect.MemoryRect.Left = left;
            rect.MemoryRect.Top = top;
            rect.MemoryRect.Right = right;
            rect.MemoryRect.Bottom = bottom;

            RedrawRect(rect);

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
                    if (p_UIElements.Contains(shape.ModifyTool))
                        p_UIElements.Remove(shape.ModifyTool);
                    switch (shape)
                    {
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
                    shape.ModifyTool.Visibility = Visibility.Visible;
                }
            }
        }
        private void RedrawLine(TLine line)
        {
            CPoint startPt = GetCanvasPoint(line.MemoryStartPoint - BoxOffset);
            CPoint endPt = GetCanvasPoint(line.MemoryEndPoint - BoxOffset);

            line.CanvasLine.X1 = startPt.X;
            line.CanvasLine.Y1 = startPt.Y;
            line.CanvasLine.X2 = endPt.X;
            line.CanvasLine.Y2 = endPt.Y;
        }
        private void RedrawRect(TRect rect)
        {
            CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
            CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);

            CPoint canvasLT = new CPoint(GetCanvasPoint(LT-BoxOffset));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB-BoxOffset));
            int width = Math.Abs(canvasRB.X - canvasLT.X);
            int height = Math.Abs(canvasRB.Y - canvasLT.Y);
            rect.CanvasRect.Width = width;
            rect.CanvasRect.Height = height;
            Canvas.SetLeft(rect.CanvasRect, canvasLT.X);
            Canvas.SetTop(rect.CanvasRect, canvasLT.Y);
            Canvas.SetRight(rect.CanvasRect, canvasRB.X);
            Canvas.SetBottom(rect.CanvasRect, canvasRB.Y);

            //MakeModifyTool(shape);
            //if (shape.isSelected)
            //    shape.ModifyTool.Visibility = Visibility.Visible;
        }
        private void RedrawPolygon(TPolygon polygon)
        {
            polygon.CanvasPolygon.Points.Clear();
            foreach (CPoint memoryPt in polygon.ListMemoryPoint)
            {
                CPoint canvasPt = GetCanvasPoint(memoryPt - BoxOffset);
                polygon.CanvasPolygon.Points.Add(new Point(canvasPt.X, canvasPt.Y));
            }
        }
        #endregion

        #region ROI Async Method
        BackgroundWorker Worker_SaveROI = new BackgroundWorker();
        BackgroundWorker Worker_ClearROI = new BackgroundWorker();
        BackgroundWorker Worker_ReadROI = new BackgroundWorker();
        private void SetBackGroundWorker()
        {
            Worker_ClearROI.DoWork += Worker_ClearROI_DoWork;
            Worker_ClearROI.RunWorkerCompleted += Worker_ClearROI_RunWorkerCompleted;
            Worker_SaveROI.DoWork += Worker_SaveROI_DoWork;
            Worker_SaveROI.RunWorkerCompleted += Worker_SaveROI_RunWorkerCompleted;
            Worker_ReadROI.DoWork += Worker_ReadROI_DoWork;
            Worker_ReadROI.RunWorkerCompleted += Worker_ReadROI_RunWorkerCompleted;
        }

        private unsafe void Worker_ReadROI_DoWork(object sender, DoWorkEventArgs e)
        {
            IntPtr ptrMem = p_ROILayer.GetPtr();
            if (ptrMem == IntPtr.Zero)
                return;

            byte[] buf = new byte[p_ROILayer.p_Size.X * p_ROILayer.p_nByte];
            for (int y = 0; y < p_ROILayer.p_Size.Y; y++)
            {
                Marshal.Copy(buf, 0, ptrMem + (p_ROILayer.p_Size.X * p_ROILayer.p_nByte * y), buf.Length);
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
            IntPtr ptrMem = p_ROILayer.GetPtr();
            if (ptrMem == IntPtr.Zero)
                return;

            byte[] buf = new byte[p_ROILayer.p_Size.X * p_ROILayer.p_nByte];
            for (int y = 0; y < p_ROILayer.p_Size.Y; y++)
            {
                Marshal.Copy(buf, 0, ptrMem+(p_ROILayer.p_Size.X * p_ROILayer.p_nByte * y), buf.Length);
            }
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
            IntPtr ptrMem = p_ROILayer.GetPtr();
            if (ptrMem == IntPtr.Zero)
                return;
            byte* bitmapPtr = (byte*)ptrMem.ToPointer();

            p_SelectedROI.p_Data.Clear();
            PointLine DotLine = new PointLine();
            bool bStart = false;
            for (int j = 0; j < p_ImageData.p_Size.Y; j++)
            {
                for (int i = 0; i < p_ImageData.p_Size.X; i++)
                {
                    var b = bitmapPtr[(j * p_ROILayer.p_Size.X + i) * 4 + 0];
                    var g = bitmapPtr[(j * p_ROILayer.p_Size.X + i) * 4 + 1];
                    var r = bitmapPtr[(j * p_ROILayer.p_Size.X + i) * 4 + 2];
                    var a = bitmapPtr[(j * p_ROILayer.p_Size.X + i) * 4 + 3];

                    if (a > 0)
                    {
                        if (!bStart)
                        {
                            DotLine = new PointLine();
                            DotLine.StartPt = new CPoint(i, j);
                            bStart = true;
                        }
                    }
                    if (a == 0)
                    {
                        if (bStart)
                        {
                            DotLine.Width = i - DotLine.StartPt.X;
                            bStart = false;
                            p_SelectedROI.p_Data.Add(DotLine);                            
                        }
                    }

                }
            }
        }
        private void Worker_SaveROI_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            p_PageEnable = true;
            p_PageOpacity = 1;
            p_LoadingOpacity = 0;
        }
        #endregion

        #region ICommand
        private void _CreateROI()
        {
            InspectionROI roi = new InspectionROI();
            roi.p_Color = Colors.AliceBlue;
            p_cInspROI.Add(roi);
            p_SelectedROI = p_cInspROI.Last();
        }
        private void _DeleteROI()
        {
            p_cInspROI.Remove(p_SelectedROI);
            p_SelectedROI = p_cInspROI.Last();
        }
        private void _ClearROI()
        {
            if (p_ImageData == null)
                return;

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
        private void _ReadROI()
        {
            if (p_ImageData == null)
                return;
            p_PageEnable = false;
            p_PageOpacity = 0.3;
            p_LoadingOpacity = 1;
            Worker_ReadROI.RunWorkerAsync();
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
            Select,
            Line,
            Rect,
            Circle,
            Polygon,
        }
        #endregion
    }
}
