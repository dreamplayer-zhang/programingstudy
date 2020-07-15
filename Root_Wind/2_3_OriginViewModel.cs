using RootTools;
using RootTools.Memory;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
//using RootTools_CLR;//CLR INSP 테스트
using System.Data;//DB 연동 테스트
using RootTools.Inspects;

namespace Root_Wind
{
    class _2_3_OriginViewModel : ObservableObject
    {
        Wind_Engineer m_Engineer;
        MemoryTool m_MemoryModule;
        public ImageData m_Image;
        DrawHelper m_DrawHelper;
        DrawData m_DD;
        Recipe m_Recipe;

        //string sPool = "pool";
        //string sGroup = "group";
        //string sMem = "mem";
        //public int MemWidth = 10000;
        //public int MemHeight = 10000;

        public _2_3_OriginViewModel(Wind_Engineer engineer, IDialogService dialogService)
        {
            m_Engineer = engineer;
            Init(engineer, dialogService);
        }

        void Init(Wind_Engineer engineer, IDialogService dialogService)
        {
            m_DD = new DrawData();
            m_Recipe = engineer.m_recipe;

            m_MemoryModule = engineer.ClassMemoryTool();
            //MemoryPool memoryPool = m_MemoryModule.GetPool(sPool, true);
            //memoryPool.p_gbPool = 1;
            //memoryPool.GetGroup(sGroup).CreateMemory(sMem, 1, 1, new CPoint(MemWidth, MemHeight));

            //m_Image = new ImageData(m_MemoryModule.GetMemory(App., sGroup, sMem));
            //p_ImageViewer = new ImageViewer_ViewModel(m_Image, dialogService);
            
            RedrawOrigin();
        }

        #region Property
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

        private System.Windows.Input.Cursor _originCursor;
        public System.Windows.Input.Cursor OriginCursor
        {
            get
            {
                return _originCursor;
            }
            set
            {
                SetProperty(ref _originCursor, value);
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

        private bool _origin_IsChecked = false;
        public bool Origin_IsChecked
        {
            get
            {
                return _origin_IsChecked;
            }
            set
            {
                SetProperty(ref _origin_IsChecked, value);
                _btnDrawOrigin();
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
        string _test2 = "";
        public string Test2
        {
            get
            {

                return _test2;
            }
            set
            {
                SetProperty(ref _test2, value);
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

        void SaveRectImage()
        {  
            m_Image.SaveRectImage(m_DD.m_OriginData.m_rt);
        }

        #endregion

        #region Command

        public ICommand btnSaveClick
        {
            get
            {
                return new RelayCommand(SaveRectImage);
            }
        }
        public ICommand CanvasMouseLeftDown
        {
            get
            {
                return new RelayCommand(_mouseLeftDown);
            }
        }
        public ICommand CanvasMouseLeftUp
        {
            get
            {
                return new RelayCommand(_mouseLeftUp);
            }
        }
        public ICommand CanvasMouseMove
        {
            get
            {
                return new RelayCommand(_mouseMove);
            }
        }
        public ICommand CanvasMouseRightDown
        {
            get
            {
                return new RelayCommand(_mouseRightDown);
            }
        }
        public ICommand CanvasMouseWheel
        {
            get
            {
                return new RelayCommand(RedrawOrigin);
            }
        }        
        public ICommand btnClear
        {
            get
            {
                return new RelayCommand(_btnClear);
            }
        }
        public ICommand btnInspTest
        {
            get
            {
                return new RelayCommand(_btnInspTest);
            }
        }
        public ICommand btnDrawOrigin
        {
            get
            {
                return new RelayCommand(_btnDrawOrigin);
            }
        }

        private void _mouseLeftDown()
        {
            switch (m_OriginProgress)
            {      
                case OriginProgress.None:
                    {
                        RedrawOrigin();
                        break;
                    }
                case OriginProgress.Start:
                    {
                        StartDrawingOrigin();
                        m_OriginProgress = OriginProgress.Drawing;
                        break;
                    }
                case OriginProgress.Drawing:
                    {
                        RedrawOrigin();
                        break;
                    }
                case OriginProgress.Done:
                    {
                        RedrawOrigin();
                        
                        if (m_MouseHitType == HitType.Body)
                        {
                            if(p_UIElement.Count >0)
                            {
                                int result = p_UIElement.IndexOf(m_DrawHelper.DrawnRect);
                                System.Windows.Shapes.Rectangle rect = (System.Windows.Shapes.Rectangle)p_UIElement[result];
                                rect.Stroke = m_DrawHelper.DrawnRect.Stroke = System.Windows.Media.Brushes.GreenYellow;
                                rect.StrokeDashArray = new DoubleCollection { 3, 2 };
                                m_DD.m_OriginData.m_color = System.Drawing.Color.GreenYellow;
                                m_OriginProgress = OriginProgress.Select;
                            }      
                        }
                        break;
                    }
                case OriginProgress.Select:
                    {
                        if (m_MouseHitType != HitType.None)
                        {
                            p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.Tool;
                            m_DrawHelper.PreMousePt = new CPoint(p_ImageViewer.p_MouseX, p_ImageViewer.p_MouseY);
                            m_DrawHelper.preRect.Left = (int)Canvas.GetLeft(m_DrawHelper.DrawnRect);
                            m_DrawHelper.preRect.Right = (int)(m_DrawHelper.preRect.Left + m_DrawHelper.DrawnRect.Width);
                            m_DrawHelper.preRect.Top = (int)Canvas.GetTop(m_DrawHelper.DrawnRect);
                            m_DrawHelper.preRect.Bottom = (int)(m_DrawHelper.preRect.Top + m_DrawHelper.DrawnRect.Height);

                            m_OriginProgress = OriginProgress.Adjusting;
                        }
                        if (m_MouseHitType == HitType.None)
                        {
                            p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.None;
                            m_OriginProgress = OriginProgress.Done;
                            m_DD.m_OriginData.m_color = System.Drawing.Color.Red;
                            RedrawOrigin();
                            Debug.WriteLine("Drag:State->Done // Move");
                        }
                        break;
                    }
                case OriginProgress.Adjusting:
                    {
                    break;
            }
        }
        }
        private void _mouseMove()
        {
            Test = m_OriginProgress.ToString();
            CPoint MousePoint = new CPoint(p_ImageViewer.p_MouseX, p_ImageViewer.p_MouseY);
            switch (m_OriginProgress)
            {
                case OriginProgress.None:
                    {
                        RedrawOrigin();
                        RedrawUIElement();
                        break;
                    }
                case OriginProgress.Start:
                    {
                        RedrawOrigin();
                        break;
                    }
                case OriginProgress.Drawing:
                    {
                        DrawingRectProgress();
                        break;
                    }
                case OriginProgress.Done:
                    {
                        RedrawOrigin();
                        if (MouseEvent.LeftButton == MouseButtonState.Released)
                        {
                            m_MouseHitType = SetHitType(MousePoint);
                            if (m_MouseHitType == HitType.Body)
                            {
                            SetMouseCursor();
                        }
                            else
                            {

                                m_MouseHitType = HitType.None;
                                SetMouseCursor();
                    }
                        }
                        break;
                    }
                case OriginProgress.Select:
                    {
                        RedrawOrigin();
                        if (MouseEvent.LeftButton == MouseButtonState.Pressed)
                        {
                            if (m_MouseHitType != HitType.None)
                            {
                                p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.Tool;
                                m_DrawHelper.PreMousePt = new CPoint(p_ImageViewer.p_MouseX, p_ImageViewer.p_MouseY);
                                m_DrawHelper.preRect.Left = (int)Canvas.GetLeft(m_DrawHelper.DrawnRect);
                                m_DrawHelper.preRect.Right = (int)(m_DrawHelper.preRect.Left + m_DrawHelper.DrawnRect.Width);
                                m_DrawHelper.preRect.Top = (int)Canvas.GetTop(m_DrawHelper.DrawnRect);
                                m_DrawHelper.preRect.Bottom = (int)(m_DrawHelper.preRect.Top + m_DrawHelper.DrawnRect.Height);

                                m_OriginProgress = OriginProgress.Adjusting;
                        }
                        }
                        if (MouseEvent.LeftButton == MouseButtonState.Released)
                        {
                            m_MouseHitType = SetHitType(MousePoint);
                            SetMouseCursor();
                        }
                        break;
                    }
                case OriginProgress.Adjusting:
                    {
                       // m_MouseHitType = SetHitType(MousePoint);
                       // SetMouseCursor();
                        if (MouseEvent.LeftButton == MouseButtonState.Pressed)
                        {
                            AdjustOrigin(MousePoint);
            }
                        break;
        }
            }
        }
        private void _mouseRightDown()
        {
            switch (m_OriginProgress)
            {
                case OriginProgress.None:
                    {
                        break;
                    }
                case OriginProgress.Start:
                    {
                        break;
                    }
                case OriginProgress.Drawing:
                    {
                        DrawOriginDone();
                        m_OriginProgress = OriginProgress.Done;
                        break;
                    }
                case OriginProgress.Done:
                    {
                        break;
                    }
                case OriginProgress.Select:
                    {
                        break;
            }
                case OriginProgress.Adjusting:
                    {
                        break;
        }
            }
        }
        private void _mouseLeftUp()
        {
            switch (m_OriginProgress)
            {
                case OriginProgress.None:
                    {
                        break;
                    }
                case OriginProgress.Start:
                    {
                        break;
                    }
                case OriginProgress.Drawing:
                    {
                        break;
                    }
                case OriginProgress.Done:
                    {
                        break;
                    }
                case OriginProgress.Select:
                    {
                        break;
                    }
                case OriginProgress.Adjusting:
                    {
                        p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.None;
                        m_OriginProgress = OriginProgress.Select;
                        break;
                    }
            }
        }

        private HitType SetHitType(CPoint point)
        {
            double left = Canvas.GetLeft(m_DrawHelper.DrawnRect);
            double top = Canvas.GetTop(m_DrawHelper.DrawnRect);
            double right = left + m_DrawHelper.DrawnRect.Width;
            double bottom = top + m_DrawHelper.DrawnRect.Height;

            const double GAP = 10;
            if (point.X < left) return HitType.None;
            if (point.X > right) return HitType.None;
            if (point.Y < top) return HitType.None;
            if (point.Y > bottom) return HitType.None;
            if (-1 * GAP <= point.X - left && point.X - left <= GAP)
            {
                if (-1 * GAP <= point.Y - top && point.Y - top <= GAP)
                    return HitType.UL;
                if (-1 * GAP <= bottom - point.Y && bottom - point.Y <= GAP)
                    return HitType.LL;
                return HitType.L;
            }
            if (-1 * GAP < right - point.X && right - point.X <= GAP)
            {
                if (-1 * GAP <= point.Y - top && point.Y - top <= GAP)
                    return HitType.UR;
                if (-1 * GAP <= bottom - point.Y && bottom - point.Y <= GAP) 
                    return HitType.LR;
                return HitType.R;
            }
            if (-1 * GAP <= point.Y - top && point.Y - top <= GAP)
                return HitType.T;
            if (-1 * GAP <= bottom - point.Y && bottom - point.Y <= GAP)
                return HitType.B;
            if (left == 0)
                return HitType.None;


            return HitType.Body;         
        }
        private void SetMouseCursor()
        {
            // See what cursor we should display.
            Cursor desired_cursor = Cursors.Arrow;
            switch (m_MouseHitType)
            {
                case HitType.None:
                    desired_cursor = Cursors.Arrow;
                    break;
                case HitType.Body:
                    desired_cursor = Cursors.ScrollAll;
                    break;
                case HitType.UL:
                case HitType.LR:
                    desired_cursor = Cursors.SizeNWSE;
                    break;
                case HitType.LL:
                case HitType.UR:
                    desired_cursor = Cursors.SizeNESW;
                    break;
                case HitType.T:
                case HitType.B:
                    desired_cursor = Cursors.SizeNS;
                    break;
                case HitType.L:
                case HitType.R:
                    desired_cursor = Cursors.SizeWE;
                    break;
            }
            // Display the desired cursor.
            if (OriginCursor != desired_cursor) OriginCursor = desired_cursor;
        }
        private void AdjustOrigin(CPoint CurrentPoint)
        {
            int offset_x = CurrentPoint.X - m_DrawHelper.PreMousePt.X;
            int offset_y = CurrentPoint.Y - m_DrawHelper.PreMousePt.Y;
            CPoint Offset = new CPoint(offset_x, offset_y);
            int new_x = m_DrawHelper.preRect.Left;
            int new_y = m_DrawHelper.preRect.Top;
            int new_width = m_DrawHelper.preRect.Width;
            int new_height = m_DrawHelper.preRect.Height;

            switch (m_MouseHitType)
            {
                case HitType.Body:
                    new_x += Offset.X;
                    new_y += Offset.Y;
                    break;
                case HitType.UL:
                    new_x += Offset.X;
                    new_y += Offset.Y;
                    new_width -= Offset.X;
                    new_height -= Offset.Y;
                    break;
                case HitType.UR:
                    new_y += Offset.Y;
                    new_width += Offset.X;
                    new_height -= Offset.Y;
                    break;
                case HitType.LR:
                    new_width += Offset.X;
                    new_height += Offset.Y;
                    break;
                case HitType.LL:
                    new_x += Offset.X;
                    new_width -= Offset.X;
                    new_height += Offset.Y;
                    break;
                case HitType.L:
                    new_x += Offset.X;
                    new_width -= Offset.X;
                    break;
                case HitType.R:
                    new_width += Offset.X;
                    break;
                case HitType.B:
                    new_height += Offset.Y;
                    break;
                case HitType.T:
                    new_y += Offset.Y;
                    new_height -= Offset.Y;
                    break;
            }

            Canvas.SetLeft(m_DrawHelper.DrawnRect, new_x);
            Canvas.SetTop(m_DrawHelper.DrawnRect, new_y);
            
            if(new_height < 50)
            {
                new_height = 50;
            }
            if(new_width < 50)
            {
                new_width = 50;
            }
            m_DrawHelper.DrawnRect.Width = new_width;
            m_DrawHelper.DrawnRect.Height = new_height;

            CPoint MemLeftTop = GetMemPoint((int)new_x, (int)new_y);
            CPoint MemRightBot = GetMemPoint((int)(new_x+new_width), (int)(new_y+new_height));
            m_DD.m_OriginData.m_rt.Left = MemLeftTop.X;
            m_DD.m_OriginData.m_rt.Top = MemLeftTop.Y;
            m_DD.m_OriginData.m_rt.Right = MemRightBot.X;
            m_DD.m_OriginData.m_rt.Bottom = MemRightBot.Y;

        }

        private void StartDrawingOrigin()
        {
            if (m_DrawHelper == null)
                m_DrawHelper = new DrawHelper();

            ClearUI();
           // m_DD.m_OriginData = null;

            m_DrawHelper.DrawnRect = new System.Windows.Shapes.Rectangle();


            m_DrawHelper.Rect_StartPt = new CPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);
            CPoint CanvasPt = GetCanvasPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);

            Canvas.SetLeft(m_DrawHelper.DrawnRect, CanvasPt.X);
            Canvas.SetTop(m_DrawHelper.DrawnRect, CanvasPt.Y);

            m_DrawHelper.DrawnRect.Stroke = System.Windows.Media.Brushes.Red;
            m_DrawHelper.DrawnRect.StrokeThickness = 2;
            m_DrawHelper.DrawnRect.StrokeDashArray = new DoubleCollection { 3, 2 };
            p_UIElement.Add(m_DrawHelper.DrawnRect);

        }
        private void DrawingRectProgress()
        {
            if (m_DrawHelper.DrawnRect != null)
            {
                m_DrawHelper.Rect_EndPt = new CPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);
                CPoint StartPt = GetCanvasPoint(m_DrawHelper.Rect_StartPt.X, m_DrawHelper.Rect_StartPt.Y);
                CPoint NowPt = GetCanvasPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);


                Canvas.SetLeft(m_DrawHelper.DrawnRect, StartPt.X);
                Canvas.SetTop(m_DrawHelper.DrawnRect, StartPt.Y);


                if (m_DrawHelper.Rect_EndPt.X < m_DrawHelper.Rect_StartPt.X)
                {
                    Canvas.SetLeft(m_DrawHelper.DrawnRect, NowPt.X);
                }
                if (m_DrawHelper.Rect_EndPt.Y < m_DrawHelper.Rect_StartPt.Y)
                {
                    Canvas.SetTop(m_DrawHelper.DrawnRect, NowPt.Y);
                }
                m_DrawHelper.DrawnRect.Width = Math.Abs(StartPt.X - NowPt.X);
                m_DrawHelper.DrawnRect.Height = Math.Abs(StartPt.Y - NowPt.Y);

            }
        }
        private void DrawOriginDone()
        {
            m_DD.m_OriginData = new RectData(new CRect(), System.Drawing.Color.Red);
            m_DD.m_OriginData.m_rt.Left = m_DrawHelper.Rect_StartPt.X;
            m_DD.m_OriginData.m_rt.Top = m_DrawHelper.Rect_StartPt.Y;
            m_DD.m_OriginData.m_rt.Right = m_DrawHelper.Rect_EndPt.X;
            m_DD.m_OriginData.m_rt.Bottom = m_DrawHelper.Rect_EndPt.Y;


            if (m_DrawHelper.Rect_EndPt.X < m_DrawHelper.Rect_StartPt.X)
            {
                m_DD.m_OriginData.m_rt.Left = m_DrawHelper.Rect_EndPt.X;
                m_DD.m_OriginData.m_rt.Right = m_DrawHelper.Rect_StartPt.X;

            }
            if (m_DrawHelper.Rect_EndPt.Y < m_DrawHelper.Rect_StartPt.Y)
            {
                m_DD.m_OriginData.m_rt.Top = m_DrawHelper.Rect_EndPt.Y;
                m_DD.m_OriginData.m_rt.Bottom = m_DrawHelper.Rect_StartPt.Y;

            }
            m_DrawHelper.DrawnRect.StrokeDashArray = new DoubleCollection(1);

            if(m_Recipe.RecipeData.RoiList == null)
                m_Recipe.RecipeData.RoiList = new ObservableCollection<Roi>();
            Roi OriginRoi = new Roi("Origin", Roi.Item.None);
            OriginRoi.Origin.OriginRect = m_DD.m_OriginData.m_rt;
            m_Recipe.RecipeData.RoiList.Add(OriginRoi);
           
           

            OriginCursor = Cursors.Arrow;

        }

        private void RedrawOrigin()
        {
            if (m_DD.m_OriginData != null)
            {
                p_UIElement.Clear();

                m_DrawHelper.DrawnRect = new System.Windows.Shapes.Rectangle();
                CPoint LeftTopPt = GetCanvasPoint(m_DD.m_OriginData.m_rt.Left, m_DD.m_OriginData.m_rt.Top);
                CPoint RighBottomPt = GetCanvasPoint(m_DD.m_OriginData.m_rt.Right, m_DD.m_OriginData.m_rt.Bottom);
                m_DrawHelper.DrawnRect.Stroke = new SolidColorBrush(ConvertColor(m_DD.m_OriginData.m_color));
                m_DrawHelper.DrawnRect.StrokeThickness = 2;

                if(m_OriginProgress == OriginProgress.Select)
                    m_DrawHelper.DrawnRect.StrokeDashArray = new DoubleCollection { 3, 2 };


                Canvas.SetLeft(m_DrawHelper.DrawnRect, LeftTopPt.X);
                Canvas.SetTop(m_DrawHelper.DrawnRect, LeftTopPt.Y);

                m_DrawHelper.DrawnRect.Width = Math.Abs(LeftTopPt.X - RighBottomPt.X);
                m_DrawHelper.DrawnRect.Height = Math.Abs(LeftTopPt.Y - RighBottomPt.Y);
                p_UIElement.Add(m_DrawHelper.DrawnRect);
            }
        }
        private void ClearUI()
        {
            p_UIElement.Clear();
            m_DD = new DrawData();
        }
        private void _btnClear()
        {
            
            //ClearUI();
            //if (Origin_IsChecked)
            //{
            //    Origin_IsChecked = false;
            //    m_OriginProgress = OriginProgress.None;
            //}
        }
        private void _btnDrawOrigin()
        {
            if (!Origin_IsChecked)
            {
                m_OriginProgress = OriginProgress.None;
                ClearUI();
            }
            else
            {
                ClearUI();
                m_OriginProgress = OriginProgress.Start;
                OriginCursor = Cursors.Cross;
        }
            //if (m_OriginProgress == OriginProgress.Start ||
            //    m_OriginProgress == OriginProgress.Done)
            //{
            //    m_OriginProgress = OriginProgress.None;
            //    OriginCursor = Cursors.Arrow;
            //}
            //else
            //{
            //    m_OriginProgress = OriginProgress.Start;
            //    OriginCursor = Cursors.Cross;
            //}

        }

        //insp 결과 display를 위해 임시 redrawUI 구현
        private void RedrawUIElement()
        {
            RedrawRect();
            RedrawStr();
            //RedrawPt();
            //RedrawLine();
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
//        CLR_Inspection clrDemo = new CLR_Inspection();
        private void _btnInspTest()
        {
            return;
        }

        #endregion


        OriginProgress m_OriginProgress = OriginProgress.None;
        HitType m_MouseHitType = HitType.None;
        enum OriginProgress
        {
            None,
            Start,
            Drawing,
            Done,
            Select,
            Adjusting,
        }
        enum HitType
        {
            None, Body, UL, UR, LR, LL, L, R, T, B
        };
        public class DrawHelper
        {
            public System.Windows.Shapes.Rectangle DrawnRect;
            public CPoint Rect_StartPt;
            public CPoint Rect_EndPt;
            public CPoint PreMousePt;
            public CRect preRect = new CRect();
        }

    }

}
