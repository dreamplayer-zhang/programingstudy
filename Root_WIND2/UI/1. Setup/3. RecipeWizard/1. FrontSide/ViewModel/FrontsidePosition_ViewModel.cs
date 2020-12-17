using Emgu.CV;
using Emgu.CV.Structure;
using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
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
using System.Drawing;
using Brushes = System.Windows.Media.Brushes;
using Rectangle = System.Windows.Shapes.Rectangle;
using Image = System.Drawing.Image;
using System.Runtime.InteropServices;

namespace Root_WIND2
{
    class FrontsidePosition_ViewModel : RootViewer_ViewModel
    {
        BoxProcess eBoxProcess;
        ModifyType eModifyType;

        TShape BOX;
        ImageData BoxImage;

        TRect InspArea;

        CPoint PointBuffer;

        Recipe m_Recipe;
        OriginRecipe m_OriginRecipe;
        PositionRecipe m_PositionRecipe;

        public void init(Setup_ViewModel setup, Recipe recipe)
        {
            base.init(ProgramManager.Instance.Image, ProgramManager.Instance.DialogService);
            p_VisibleMenu = System.Windows.Visibility.Visible;

            m_Recipe = recipe;
            m_OriginRecipe = recipe.GetRecipe<OriginRecipe>();
            m_PositionRecipe = recipe.GetRecipe<PositionRecipe>();
            p_Origin = new CPoint(m_OriginRecipe.OriginX, m_OriginRecipe.OriginY);
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
                SetRoiRect();
                m_Origin = new CPoint(m_OriginRecipe.OriginX, m_OriginRecipe.OriginY);

                if (InspArea == null)
                    InspArea = new TRect(Brushes.Yellow, 1, 1);

                InspArea.MemoryRect.Left = m_Origin.X - m_OriginRecipe.InspectionBufferOffsetX;
                InspArea.MemoryRect.Bottom = m_Origin.Y + m_OriginRecipe.InspectionBufferOffsetY;
                InspArea.MemoryRect.Right = m_Origin.X + m_OriginRecipe.DiePitchX + m_OriginRecipe.InspectionBufferOffsetX;
                InspArea.MemoryRect.Top = m_Origin.Y - m_OriginRecipe.DiePitchY - m_OriginRecipe.InspectionBufferOffsetY;
                AddInspArea(InspArea);
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

        public ObservableCollection<Visibility> p_VisibleEmpty
        {
            get
            {
                return m_VisibleEmpty;
            }
            set
            {
                SetProperty(ref m_VisibleEmpty, value);
            }
        }
        private ObservableCollection<Visibility> m_VisibleEmpty = new ObservableCollection<Visibility>();

        public int[] p_nMarkIndex
        {
            get
            {
                return m_nMarkIndex;
            }
            set
            {
                SetProperty(ref m_nMarkIndex, value);
            }
        }
        private int[] m_nMarkIndex = new int[3];
        public ObservableCollection<UIElement> p_MasterMark
        {
            get
            {
                return m_MasterMark;
            }
            set
            {
                m_MasterMark = value;
            }
        }
        private ObservableCollection<UIElement> m_MasterMark = new ObservableCollection<UIElement>();
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
        private ObservableCollection<UIElement> m_ShotMark = new ObservableCollection<UIElement>();
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
        private ObservableCollection<UIElement> m_ChipMark = new ObservableCollection<UIElement>();

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

        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
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
            if (InspArea != null)
            {
                AddInspArea();
            }
            if (BOX == null)
                return;
            else
            {
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

        }

        private void AddInspArea(TRect rect = null)
        {
            if (InspArea != null)
                if (p_ViewElement.Contains(InspArea.CanvasRect))
                    p_ViewElement.Remove(InspArea.CanvasRect);

            if (rect != null)
            {
                InspArea = new TRect(rect.FillBrush, rect.CanvasRect.StrokeThickness, rect.CanvasRect.Opacity);
                InspArea.MemoryRect.Left = rect.MemoryRect.Left;
                InspArea.MemoryRect.Top = rect.MemoryRect.Top;
                InspArea.MemoryRect.Right = rect.MemoryRect.Right;
                InspArea.MemoryRect.Bottom = rect.MemoryRect.Bottom;
            }
            else
                rect = InspArea;
            CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
            CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);
            CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB));

            int width = Math.Abs(canvasRB.X - canvasLT.X);
            int height = Math.Abs(canvasRB.Y - canvasLT.Y);

            Canvas.SetLeft(InspArea.CanvasRect, canvasLT.X);
            Canvas.SetTop(InspArea.CanvasRect, canvasLT.Y);
            Canvas.SetRight(InspArea.CanvasRect, canvasRB.X);
            Canvas.SetBottom(InspArea.CanvasRect, canvasRB.Y);
            InspArea.CanvasRect.Width = width;
            InspArea.CanvasRect.Height = height;

            p_ViewElement.Add(InspArea.CanvasRect);
        }

        private void BoxDone(object e)
        {
            TRect Box = e as TRect;
            int byteCnt = p_ImageData.p_nByte;

            BoxImage = new ImageData(Box.MemoryRect.Width, Box.MemoryRect.Height, byteCnt);
            
            BoxImage.m_eMode = ImageData.eMode.ImageBuffer;
            BoxImage.SetData(p_ImageData
                , new CRect(Box.MemoryRect.Left, Box.MemoryRect.Top, Box.MemoryRect.Right, Box.MemoryRect.Bottom)
                , (int)p_ImageData.p_Stride, byteCnt);

            Dispatcher.CurrentDispatcher.BeginInvoke(new ThreadStart(() =>
            {
                p_BoxImgSource = BoxImage.GetBitMapSource(byteCnt);
            }));

            p_PointXY = new CPoint(Box.MemoryRect.Left, Box.MemoryRect.Top);
            p_SizeWH = new CPoint(Box.MemoryRect.Width, Box.MemoryRect.Height);
            p_Offset = m_PointXY - m_Origin;
        }

   

        private void CheckEmpty()
        {
            if (p_VisibleEmpty.Count == 0)
            {
                p_VisibleEmpty.Add(new Visibility());
                p_VisibleEmpty.Add(new Visibility());
                p_VisibleEmpty.Add(new Visibility());
            }

            if (p_MasterMark.Count < 1)
                p_VisibleEmpty[0] = Visibility.Visible;
            else
                p_VisibleEmpty[0] = Visibility.Collapsed;

            if (p_ShotMark.Count < 1)
                p_VisibleEmpty[1] = Visibility.Visible;
            else
                p_VisibleEmpty[1] = Visibility.Collapsed;

            if (p_ChipMark.Count < 1)
                p_VisibleEmpty[2] = Visibility.Visible;
            else
                p_VisibleEmpty[2] = Visibility.Collapsed;
        }

        private void _saveImage()
        {
        }
        private void _addMasterMark()
        {
            var asdf = p_nMarkIndex;
            if (BoxImage == null)
                return;
            //RecipeType_FeatureData rtf = new RecipeType_FeatureData(m_Offset.X, m_Offset.Y, m_SizeWH.X, m_SizeWH.Y, BoxImage.GetByteArray());
            m_PositionRecipe.AddMasterFeature(m_Offset.X, m_Offset.Y, m_SizeWH.X, m_SizeWH.Y, BoxImage.p_nByte, BoxImage.GetByteArray());

            FeatureControl fc = new FeatureControl();
            fc.p_Offset = m_Offset;
            fc.p_ImageSource = p_BoxImgSource;
            p_MasterMark.Add(fc);
            CheckEmpty();
        }
        private void _addShotMark()
        {
            if (BoxImage == null)
                return;

            m_PositionRecipe.AddShotFeature(m_Offset.X, m_Offset.Y, m_SizeWH.X, m_SizeWH.Y, BoxImage.p_nByte, BoxImage.GetByteArray());

            FeatureControl fc = new FeatureControl();
            fc.p_Offset = m_Offset;
            fc.p_ImageSource = p_BoxImgSource;
            p_ShotMark.Add(fc);
            CheckEmpty();
        }
        private void _addChipMark()
        {
            if (BoxImage == null)
                return;

            m_PositionRecipe.AddChipFeature(m_Offset.X, m_Offset.Y, m_SizeWH.X, m_SizeWH.Y, BoxImage.p_nByte, BoxImage.GetByteArray());

            FeatureControl fc = new FeatureControl();
            fc.p_Offset = m_Offset;
            fc.p_ImageSource = p_BoxImgSource;
            p_ChipMark.Add(fc);
            CheckEmpty();
        }
        private void _cmdContext()
        {
            ContextMenu context = new ContextMenu();
            
            
        }

        public void LoadPositonMark()
        {
            p_MasterMark.Clear();
            p_ChipMark.Clear();
            m_PositionRecipe = m_Recipe.GetRecipe<PositionRecipe>();

            //List<ImageData> listMasterFeautreimage = m_PositionRecipe.ListMasterImageFeatures;
            List<RecipeType_ImageData> listMasterFeature = m_PositionRecipe.ListMasterFeature;
            for(int i = 0; i < listMasterFeature.Count; i ++)
            {
                FeatureControl fc = new FeatureControl();
                CPoint offset = new CPoint(listMasterFeature[i].PositionX, listMasterFeature[i].PositionY);
                int nW = listMasterFeature[i].Width;
                int nH = listMasterFeature[i].Height;
                byte[] rawdata = listMasterFeature[i].RawData;
                fc.p_Offset = offset;
                //fc.p_ImageSource = listMasterFeautreimage[i].GetBitMapSource();
                fc.p_ImageSource = ImageHelper.GetBitmapSourceFromBitmap(listMasterFeature[i].GetFeatureBitmap());
                p_MasterMark.Add((fc.Clone() as FeatureControl));
            }

            //List<ImageData> listShotFeautreimage = m_PositionRecipe.ListShotImageFeatures;
            List<RecipeType_ImageData> listShotFeature = m_PositionRecipe.ListShotFeature;
            for (int i = 0; i < listShotFeature.Count; i++)
            {
                FeatureControl fc = new FeatureControl();
                CPoint offset = new CPoint(listShotFeature[i].PositionX, listShotFeature[i].PositionY);
                int nW = listShotFeature[i].Width;
                int nH = listShotFeature[i].Height;
                byte[] rawdata = listShotFeature[i].RawData;
                fc.p_Offset = offset;
                //fc.p_ImageSource = listShotFeature[i].GetBitMapSource();
                fc.p_ImageSource = ImageHelper.GetBitmapSourceFromBitmap(listShotFeature[i].GetFeatureBitmap());
                p_ShotMark.Add((fc.Clone() as FeatureControl));
            }

            //List<ImageData> listDieFeautreimage = m_PositionRecipe.ListDieImageFeatures;
            List<RecipeType_ImageData> listDieFeature = m_PositionRecipe.ListDieFeature;
            for (int i = 0; i < listDieFeature.Count; i++)
            {
                FeatureControl fc = new FeatureControl();
                CPoint offset = new CPoint(listDieFeature[i].PositionX, listDieFeature[i].PositionY);
                int nW = listDieFeature[i].Width;
                int nH = listDieFeature[i].Height;
                byte[] rawdata = listDieFeature[i].RawData;
                fc.p_Offset = offset;
                //fc.p_ImageSource = listDieFeature[i].GetBitMapSource();
                fc.p_ImageSource = ImageHelper.GetBitmapSourceFromBitmap(listDieFeature[i].GetFeatureBitmap());
                p_ChipMark.Add((fc.Clone() as FeatureControl));
            }
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
                return new RelayCommand(_addMasterMark);
            }
        }
        public ICommand AddShotMark
        {
            get
            {
                return new RelayCommand(_addShotMark);
            }
        }
        public ICommand AddChipMark
        {
            get
            {
                return new RelayCommand(_addChipMark);
            }
        }

        public ICommand DeleteMasterMark
        {
            get
            {
                return new RelayCommand(_addChipMark);
            }
        }

        private enum BoxProcess
        {
            None,
            Drawing,
            Modifying,
        }

    }
}
