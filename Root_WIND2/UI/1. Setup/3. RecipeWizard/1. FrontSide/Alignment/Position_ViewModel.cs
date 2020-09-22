using Emgu.CV;
using Emgu.CV.Structure;
using RootTools;
using RootTools_Vision;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Root_WIND2
{
    class Position_ViewModel : RootViewer_ViewModel
    {
        TShape BOX;
        ImageData BoxImage;
        CPoint PointBuffer;

        BoxProcess eBoxProcess;
        ModifyType eModifyType;

        Recipe m_Recipe;
        RecipeData_Origin m_RecipeData_Origin;
        RecipeData_Position m_RecipeData_Position;
        public void init(Setup_ViewModel setup, Recipe recipe)
        {
            base.init(setup.m_MainWindow.m_Image, setup.m_MainWindow.dialogService);
            p_VisibleMenu = System.Windows.Visibility.Visible;
            p_VisibleTool = System.Windows.Visibility.Collapsed;

            m_Recipe = recipe;
            m_RecipeData_Origin = recipe.GetRecipeData(typeof(RecipeData_Origin)) as RecipeData_Origin; ;
            m_RecipeData_Position = recipe.GetRecipeData(typeof(RecipeData_Position)) as RecipeData_Position;
            p_Origin = new CPoint(m_RecipeData_Origin.OriginX, m_RecipeData_Origin.OriginY);
            CheckEmpty();
        }

        private CPoint m_PointXY = new CPoint();
        public CPoint p_PointXY
        {
            get
            {
                return m_PointXY;
            }
            set
            {
                SetProperty(ref m_PointXY, value);
            }
        }
        private CPoint m_SizeWH = new CPoint();
        public CPoint p_SizeWH
        {
            get
            {
                return m_SizeWH;
            }
            set
            {
                SetProperty(ref m_SizeWH, value);
            }
        }
        private CPoint m_Origin = new CPoint();
        public CPoint p_Origin
        {
            get
            {
                return m_Origin;
            }
            set
            {
                SetProperty(ref m_Origin, value);
            }
        }
        private CPoint m_Offset = new CPoint();
        public CPoint p_Offset
        {
            get
            {
                return m_Offset;
            }
            set
            {
                SetProperty(ref m_Offset, value);
            }
        }

        private ObservableCollection<UIElement> m_WaferMark = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_WaferMark
        {
            get
            {
                return m_WaferMark;
            }
            set
            {
                m_WaferMark = value;
            }
        }
        private ObservableCollection<UIElement> m_ShotMark = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_ShotMark
        {
            get
            {
                return m_ShotMark;
            }
            set
            {
                m_ShotMark = value;
            }
        }
        private ObservableCollection<UIElement> m_ChipMark = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_ChipMark
        {
            get
            {
                return m_ChipMark;
            }
            set
            {
                m_ChipMark = value;
            }
        }

        private BitmapSource m_BoxImgSource;
        public BitmapSource p_BoxImgSource
        {
            get
            {
                return m_BoxImgSource;
            }
            set
            {
                SetProperty(ref m_BoxImgSource, value);
            }
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
        private enum BoxProcess
        {
            None,
            Drawing,
            Modifying,
        }
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftCtrl && m_KeyEvent.IsDown)
                    return;
            CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint MemPt = GetMemPoint(CanvasPt);
            switch (eBoxProcess)
            {
                case BoxProcess.None:
                    if (p_Cursor != Cursors.Arrow)
                    {
                        PointBuffer = MemPt;
                        string cursor = p_Cursor.ToString();
                        eBoxProcess = BoxProcess.Modifying;
                        break;
                    }
                    else
                    {
                        if (BOX != null)
                        {
                            if (p_ViewElement.Contains(BOX.UIElement))
                            {
                                p_ViewElement.Remove(BOX.UIElement);
                                p_ViewElement.Remove(BOX.ModifyTool);
                            }
                        }
                        BOX = StartDraw(BOX, MemPt);
                        p_ViewElement.Add(BOX.UIElement);
                        eBoxProcess = BoxProcess.Drawing;
                    }
                    break;
                case BoxProcess.Drawing:
                    BOX = DrawDone(BOX);

                    BoxDone(BOX);
                    eBoxProcess = BoxProcess.None;
                    break;
                case BoxProcess.Modifying:
                    break;
            }
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint MemPt = GetMemPoint(CanvasPt);
            switch (eBoxProcess)
            {
                case BoxProcess.None:
                    break;
                case BoxProcess.Drawing:
                    BOX = Drawing(BOX, MemPt);
                    break;
                case BoxProcess.Modifying:
                    {
                        if (e.LeftButton == MouseButtonState.Pressed)
                            BOX = ModifyRect(BOX, MemPt);

                    }
                    break;
            }
        }
        public override void PreviewMouseUp(object sender, MouseEventArgs e)
        {
            switch (eBoxProcess)
            {
                case BoxProcess.None:
                    break;
                case BoxProcess.Drawing:
                    break;
                case BoxProcess.Modifying:
                    {
                        if (BOX.isSelected)
                        {
                            MakeModifyTool(BOX);
                            BOX.ModifyTool.Visibility = Visibility.Visible;
                        }
                        BoxDone(BOX);
                        eBoxProcess = BoxProcess.None;
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
        public override void _openImage()
        {
            base._openImage();

        }
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            base.CanvasMovePoint_Ref(point, nX, nY);
            RedrawShapes();
        }
        private TShape StartDraw(TShape shape, CPoint memPt)
        {
            shape = new TRect(Brushes.Blue, 2, 0.4);
            TRect rect = shape as TRect;
            rect.MemPointBuffer = memPt;
            rect.MemoryRect.Left = memPt.X;
            rect.MemoryRect.Top = memPt.Y;

            return shape;
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
        private TShape DrawDone(TShape shape)
        {
            TRect rect = shape as TRect;
            rect.CanvasRect.Fill = rect.FillBrush;
            rect.CanvasRect.Tag = rect;
            rect.CanvasRect.MouseEnter += CanvasRect_MouseEnter;
            rect.CanvasRect.MouseLeave += CanvasRect_MouseLeave;
            rect.CanvasRect.MouseLeftButtonDown += CanvasRect_MouseLeftButtonDown;
            MakeModifyTool(rect);

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
            TRect rect = (sender as Rectangle).Tag as TRect;
            if (rect.isSelected)
                rect.isSelected = false;
            else
                rect.isSelected = true;

        }
        private void MakeModifyTool(TShape shape)
        {
            if (p_ViewElement.Contains(shape.ModifyTool))
                p_ViewElement.Remove(shape.ModifyTool);

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
                    System.Windows.Shapes.Ellipse modifyPoint = new System.Windows.Shapes.Ellipse();
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
            p_ViewElement.Add(modifyTool);
        }
        private void ModifyPoint_MouseEnter(object sender, MouseEventArgs e)
        {
            CPoint index = (sender as System.Windows.Shapes.Ellipse).Tag as CPoint;

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
        private TShape ModifyRect(TShape shape, CPoint memPt)
        {
            int offset_x = memPt.X - PointBuffer.X;
            int offset_y = memPt.Y - PointBuffer.Y;
            CPoint ptOffset = new CPoint(offset_x, offset_y);

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
            }
            PointBuffer = memPt;
            return shape;
        }
        private void RedrawShapes()
        {
            if (BOX == null)
                return;

            TRect rect = BOX as TRect;
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

            MakeModifyTool(BOX);
            if (BOX.isSelected)
                BOX.ModifyTool.Visibility = Visibility.Visible;

        }



        private void BoxDone(object e)
        {
            TRect Box = e as TRect;
            BoxImage = new ImageData(Box.MemoryRect.Width, Box.MemoryRect.Height);
            BoxImage.m_eMode = ImageData.eMode.ImageBuffer;
            BoxImage.SetData(p_ImageData.GetPtr(), Box.MemoryRect, (int)p_ImageData.p_Stride);

            Dispatcher.CurrentDispatcher.BeginInvoke(new ThreadStart(() =>
            {
                p_BoxImgSource = BoxImage.GetBitMapSource();
            }));
            p_PointXY = new CPoint(Box.MemoryRect.Left, Box.MemoryRect.Top);
            p_SizeWH = new CPoint(Box.MemoryRect.Width, Box.MemoryRect.Height);
            p_Offset = m_Origin - m_PointXY;
        }

   

        private void CheckEmpty()
        {
            if (p_WaferMark.Count < 1)
                p_WaferMark.Add(TbEmpty());
            if (p_ShotMark.Count < 1)
                p_ShotMark.Add(TbEmpty());
            if (p_ChipMark.Count < 1)
                p_ChipMark.Add(TbEmpty());
        }
        private TextBlock TbEmpty()
        {
            TextBlock tb = new TextBlock();
            tb.Text = "Empty";
            tb.Margin = new Thickness(20, 0, 0, 0);
            tb.FontSize = 15;
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            return tb;
        }

        private void _saveImage()
        {
        }
        private void _addWaferMark()
        {
            RecipeType_FeatureData rtf = new RecipeType_FeatureData(m_Offset.X, m_Offset.Y, m_SizeWH.X, m_SizeWH.Y, BoxImage.GetByteArray());
            m_RecipeData_Position.AddMasterFeature(rtf);
            FeatureControl fc = new FeatureControl();
            fc.p_Offset = m_Offset;
            fc.p_ImageSource = BoxImage.GetBitMapSource();
            p_WaferMark.Add(fc);
        }
        private void _addShotMark()
        {
        }
        private void _addChipMark()
        {
        }
        public ICommand SaveImage
        {
            get
            {
                return new RelayCommand(CheckEmpty);
            }
        }
        public ICommand AddWaferMark
        {
            get
            {
                return new RelayCommand(_addWaferMark);
            }
        }
        public ICommand AddShotMark
        {
            get
            {
                return new RelayCommand(_addWaferMark);
            }
        }
        public ICommand AddChipMark
        {
            get
            {
                return new RelayCommand(_addWaferMark);
            }
        }
    }
}
