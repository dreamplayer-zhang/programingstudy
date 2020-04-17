using RootTools;
using RootTools.Inspects;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Root_Vega
{
    class _2_4_PositionViewModel : ObservableObject
    {
        Vega_Engineer m_Engineer;
        MemoryTool m_MemoryModule;
        ImageData m_Image;
        DrawHelper m_DrawHelper;
        Recipe m_Recipe;
        Roi m_Roi;
        Stack<Feature> m_PasteFeature = new Stack<Feature>();

        string sPool = "pool";
        string sGroup = "group";
        string sMem = "mem";

        public _2_4_PositionViewModel(Vega_Engineer engineer, IDialogService dialogService)
        {
            m_Engineer = engineer;
            Init(engineer, dialogService);
            p_ImageViewer.m_AfterLoaded += Redraw;
        }
        void Init(Vega_Engineer engineer, IDialogService dialogService)
        {
            m_Recipe = engineer.m_recipe;
            m_DrawHelper = new DrawHelper();
            m_Roi = new Roi("Position", Roi.Item.Position);

            m_MemoryModule = engineer.ClassMemoryTool();
            //m_MemoryModule.GetPool(sPool).p_gbPool = 3;
            //m_MemoryModule.GetPool(sPool).GetGroup(sGroup).CreateMemory(sMem, 1, 1, new CPoint(MemWidth, MemHeight));
            //m_MemoryModule.GetPool(sPool).GetGroup(sGroup).GetMemory(sMem);

            m_Image = new ImageData(m_MemoryModule.GetMemory(sPool, sGroup, sMem));
            p_ImageViewer = new ImageViewer_ViewModel(m_Image, dialogService);
        }
        //int aa = 0;
        #region Property
        private KeyEventArgs _keyEvent;
        public KeyEventArgs KeyEvent
        {
            get
            {
                return _keyEvent;
            }
            set
            {
                SetProperty(ref _keyEvent, value);
            }
        }
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

        private System.Windows.Input.Cursor _Cursor;
        public System.Windows.Input.Cursor p_Cursor
        {
            get
            {
                return _Cursor;
            }
            set
            {
                SetProperty(ref _Cursor, value);
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


        private bool _feature_isChecked = false;
        public bool p_Feature_isChecked
        {
            get
            {
                return _feature_isChecked;
            }
            set
            {
                if (IfFeatureIsFull())
                    return;

                SetProperty(ref _feature_isChecked, value);
                if (value == true)
                {
                    m_PositionProgress = PositionProgress.Start;
                    p_Cursor = Cursors.Pen;
                }
                if (value == false)
                {
                    m_PositionProgress = PositionProgress.None;
                    p_Cursor = Cursors.Arrow;
                }
            }
        }

        private int _FeatureCnt = 0;
        public int p_FeatureCnt
        {
            get
            {
                return _FeatureCnt;
            }
            set
            {
                SetProperty(ref _FeatureCnt, value);
            }
        }

        private string _test;
        public string test
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

        private void StartDrawing()
        {
            m_DrawHelper.Feature = new Feature();

            m_DrawHelper.Feature.m_rtRoi.Left = p_ImageViewer.p_MouseMemX;
            m_DrawHelper.Feature.m_rtRoi.Top = p_ImageViewer.p_MouseMemY;

            m_DrawHelper.DrawnRect = new System.Windows.Shapes.Rectangle();
            m_DrawHelper.DrawnRect.Stroke = System.Windows.Media.Brushes.Red;
            m_DrawHelper.DrawnRect.StrokeThickness = 2;
            m_DrawHelper.DrawnRect.StrokeDashArray = new DoubleCollection { 3, 2 };

            m_DrawHelper.Rect_StartPt = new CPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);

            CPoint CanvasPt = GetCanvasPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);

            Canvas.SetLeft(m_DrawHelper.DrawnRect, CanvasPt.X);
            Canvas.SetTop(m_DrawHelper.DrawnRect, CanvasPt.Y);

            m_DrawHelper.ListFeatureTemp.Add(m_DrawHelper.Feature);
            p_UIElement.Add(m_DrawHelper.DrawnRect);

        }
        private void DrawingProgress()
        {
            m_DrawHelper.Rect_EndPt = new CPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);

            m_DrawHelper.Feature.m_rtRoi.Left = m_DrawHelper.Rect_StartPt.X;
            m_DrawHelper.Feature.m_rtRoi.Top = m_DrawHelper.Rect_StartPt.Y;
            m_DrawHelper.Feature.m_rtRoi.Right = m_DrawHelper.Rect_EndPt.X;
            m_DrawHelper.Feature.m_rtRoi.Bottom = m_DrawHelper.Rect_EndPt.Y;

            if (m_DrawHelper.Rect_EndPt.X < m_DrawHelper.Rect_StartPt.X)
            {
                m_DrawHelper.Feature.m_rtRoi.Left = m_DrawHelper.Rect_EndPt.X;
                m_DrawHelper.Feature.m_rtRoi.Right = m_DrawHelper.Rect_StartPt.X;

            }
            if (m_DrawHelper.Rect_EndPt.Y < m_DrawHelper.Rect_StartPt.Y)
            {
                m_DrawHelper.Feature.m_rtRoi.Top = m_DrawHelper.Rect_EndPt.Y;
                m_DrawHelper.Feature.m_rtRoi.Bottom = m_DrawHelper.Rect_StartPt.Y;
            }
            m_DrawHelper.Feature.m_rtRoi.Width = Math.Abs(m_DrawHelper.Rect_StartPt.X - m_DrawHelper.Rect_EndPt.X);
            m_DrawHelper.Feature.m_rtRoi.Height = Math.Abs(m_DrawHelper.Rect_StartPt.Y - m_DrawHelper.Rect_EndPt.Y);


            CPoint StartPt = GetCanvasPoint(m_DrawHelper.Rect_StartPt.X, m_DrawHelper.Rect_StartPt.Y);
            CPoint NowPt = GetCanvasPoint(m_DrawHelper.Rect_EndPt.X, m_DrawHelper.Rect_EndPt.Y);

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
        private void DrawingDone()
        {
            m_DrawHelper.DrawnRect.StrokeDashArray = new DoubleCollection(1);

            p_FeatureCnt = m_DrawHelper.ListFeatureTemp.Count + m_Roi.m_Position.m_ListFeature.Count;
            m_PasteFeature.Clear();
        }
        private void Redraw()
        {
            p_UIElement.Clear();
            for (int i = 0; i < m_Roi.m_Position.m_ListFeature.Count; i++)
            {
                m_DrawHelper.DrawnRect = new Rectangle();
                m_DrawHelper.DrawnRect.Stroke = Brushes.Blue;
                m_DrawHelper.DrawnRect.StrokeThickness = 2;

                CPoint LeftTopPt = GetCanvasPoint(m_Roi.m_Position.m_ListFeature[i].m_rtRoi.Left, m_Roi.m_Position.m_ListFeature[i].m_rtRoi.Top);
                CPoint RightBottomPt = GetCanvasPoint(m_Roi.m_Position.m_ListFeature[i].m_rtRoi.Right, m_Roi.m_Position.m_ListFeature[i].m_rtRoi.Bottom);

                Canvas.SetLeft(m_DrawHelper.DrawnRect, LeftTopPt.X);
                Canvas.SetTop(m_DrawHelper.DrawnRect, LeftTopPt.Y);

                m_DrawHelper.DrawnRect.Width = Math.Abs(LeftTopPt.X - RightBottomPt.X);
                m_DrawHelper.DrawnRect.Height = Math.Abs(LeftTopPt.Y - RightBottomPt.Y);
                p_UIElement.Add(m_DrawHelper.DrawnRect);
            }
            for (int i = 0; i < m_DrawHelper.ListFeatureTemp.Count; i++)
            {
                m_DrawHelper.DrawnRect = new Rectangle();
                m_DrawHelper.DrawnRect.Stroke = Brushes.Red;
                m_DrawHelper.DrawnRect.StrokeThickness = 2;

                CPoint LeftTopPt = GetCanvasPoint(m_DrawHelper.ListFeatureTemp[i].m_rtRoi.Left, m_DrawHelper.ListFeatureTemp[i].m_rtRoi.Top);
                CPoint RightBottomPt = GetCanvasPoint(m_DrawHelper.ListFeatureTemp[i].m_rtRoi.Right, m_DrawHelper.ListFeatureTemp[i].m_rtRoi.Bottom);

                Canvas.SetLeft(m_DrawHelper.DrawnRect, LeftTopPt.X);
                Canvas.SetTop(m_DrawHelper.DrawnRect, LeftTopPt.Y);

                m_DrawHelper.DrawnRect.Width = Math.Abs(LeftTopPt.X - RightBottomPt.X);
                m_DrawHelper.DrawnRect.Height = Math.Abs(LeftTopPt.Y - RightBottomPt.Y);
                p_UIElement.Add(m_DrawHelper.DrawnRect);
            }
        }
        private bool IfFeatureIsFull()
        {
            if (p_FeatureCnt == 4)
            {
                // Redraw();
                m_PositionProgress = PositionProgress.None;
                p_Cursor = Cursors.Arrow;
                return true;
            }
            return false;
        }

        Point m_LeftDownPos = new Point();
        int m_ClickTimer = 0;
        private void _mouseLeftDown()
        {
            switch (m_PositionProgress)
            {
                case PositionProgress.None:
                    {
                        Redraw();
                        break;
                    }
                case PositionProgress.Start:
                    {
                        m_LeftDownPos.X = p_ImageViewer.p_MouseX;
                        m_LeftDownPos.Y = p_ImageViewer.p_MouseY;
                        m_ClickTimer = 1000 * DateTime.Now.Second + DateTime.Now.Millisecond;
                        break;
                    }
                case PositionProgress.Drawing:
                    {
                        break;
                    }
                case PositionProgress.Done:
                    {
                        break;
                    }
            }
        }
        private void _mouseLeftUp()
        {
            switch (m_PositionProgress)
            {
                case PositionProgress.None:
                    {
                        break;
                    }
                case PositionProgress.Start:
                    {
                        m_ClickTimer = 1000 * DateTime.Now.Second + DateTime.Now.Millisecond - m_ClickTimer;
                        if (m_ClickTimer < 0)
                            m_ClickTimer += 60000;

                        if ((m_ClickTimer < 500 && Math.Abs(m_LeftDownPos.X - p_ImageViewer.p_MouseX) + Math.Abs(m_LeftDownPos.Y - p_ImageViewer.p_MouseY) < 10)
                            || (m_LeftDownPos.X == p_ImageViewer.p_MouseX && m_LeftDownPos.Y == p_ImageViewer.p_MouseY))
                        {
                            if (IfFeatureIsFull())
                                return;

                            StartDrawing();
                            m_PositionProgress = PositionProgress.Drawing;
                            break;
                        }
                        break;
                    }
                case PositionProgress.Drawing:
                    {
                        break;
                    }
                case PositionProgress.Done:
                    {
                        break;
                    }
            }
        }
        private void _mouseMove()
        {
            test = m_PositionProgress.ToString();
            switch (m_PositionProgress)
            {
                case PositionProgress.None:
                    {
                        Redraw();
                        break;
                    }
                case PositionProgress.Start:
                    {
                        Redraw();
                        break;
                    }
                case PositionProgress.Drawing:
                    {
                        Redraw();
                        m_DrawHelper.DrawnRect.StrokeDashArray = new DoubleCollection { 3, 2 };
                        DrawingProgress();
                        break;
                    }
                case PositionProgress.Done:
                    {
                        break;
                    }
            }
        }
        private void _mouseRightDown()
        {
            switch (m_PositionProgress)
            {
                case PositionProgress.None:
                    {
                        break;
                    }
                case PositionProgress.Start:
                    {
                        break;
                    }
                case PositionProgress.Drawing:
                    {
                        DrawingDone();
                        m_PositionProgress = PositionProgress.Start;
                        IfFeatureIsFull();
                        break;
                    }
                case PositionProgress.Done:
                    {
                        break;
                    }
            }
        }

        private void _btnClear()
        {
            p_UIElement.Clear();
            if (p_Feature_isChecked)
            {
                m_PositionProgress = PositionProgress.Start;
                p_Cursor = Cursors.Pen;
            }
            else
                m_PositionProgress = PositionProgress.None;


            m_DrawHelper.ListFeatureTemp.Clear();
            m_Roi.m_Position.m_ListFeature.Clear();

            p_FeatureCnt = 0;
            //if (Feature_IsChecked)
            //{
            //    Feature_IsChecked = false;
            //    m_PositionProgress = PositionProgress.None;
            //}
        }
        private void _btnDone()
        {
            foreach (Rectangle rect in p_UIElement)
            {
                rect.Stroke = Brushes.Blue;
            }
            m_Roi.m_Position.m_ListFeature.AddRange(m_DrawHelper.ListFeatureTemp);
            m_DrawHelper.ListFeatureTemp.Clear();

            p_FeatureCnt = m_Roi.m_Position.m_ListFeature.Count();

            int stride = m_Image.p_Size.X;
            IntPtr ptr = m_Image.m_ptrImg;
            foreach (Feature feature in m_Roi.m_Position.m_ListFeature)
            {
                ImageData id = new ImageData(feature.m_rtRoi.Width, feature.m_rtRoi.Height);
                id.SetData(ptr, feature.m_rtRoi, stride);

                feature.m_Feature = id;
            }


            //Add Roi(Position) to Recipe Data
            m_Recipe.p_RecipeData.p_Roi.Add(m_Roi);

            m_PositionProgress = PositionProgress.None;
            p_Feature_isChecked = false;
            p_Cursor = Cursors.Arrow;


            //m_Image.SaveRectImage(m_Roi.m_Position.m_ListFeature[0].m_rtRoi);

            //var a = m_Image;
        }

        private void _KeyDown()
        {
            if (KeyEvent.KeyboardDevice.Modifiers == ModifierKeys.Control && p_Feature_isChecked)
                if (KeyEvent.Key == Key.Z)
                {
                    if (p_FeatureCnt == 4 && _feature_isChecked == true)
                    {
                        m_PositionProgress = PositionProgress.Start;
                        p_Cursor = Cursors.Pen;
                    }
                    if (m_DrawHelper.ListFeatureTemp.Count > 0)
                    {
                        //가장 최근 RECT 삭제.
                        m_PasteFeature.Push(m_DrawHelper.ListFeatureTemp.Last());
                        m_DrawHelper.ListFeatureTemp.RemoveAt(m_DrawHelper.ListFeatureTemp.Count - 1);
                        p_FeatureCnt = m_DrawHelper.ListFeatureTemp.Count + m_Roi.m_Position.m_ListFeature.Count;
                        Redraw();
                    }
                }
            if (KeyEvent.Key == Key.Y)
            {

                if (m_DrawHelper.ListFeatureTemp.Count < 4 && m_PasteFeature.Any())
                {
                    //Done이 안된 경우(즉,버퍼에 삭제된 Rect가 남아있는경우) 가장 최근 Rect 추가.
                    m_DrawHelper.ListFeatureTemp.Add(m_PasteFeature.Pop());
                    p_FeatureCnt = m_DrawHelper.ListFeatureTemp.Count + m_Roi.m_Position.m_ListFeature.Count;
                    Redraw();
                    IfFeatureIsFull();
                }

            }
        }




        #endregion

        #region Command
        public ICommand KeyDownCommand
        {
            get
            {
                return new RelayCommand(_KeyDown);
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
                return new RelayCommand(Redraw);
            }
        }
        public ICommand btnDone
        {
            get
            {
                return new RelayCommand(_btnDone);
            }
        }
        public ICommand btnClear
        {
            get
            {
                return new RelayCommand(_btnClear);
            }
        }
        #endregion

        //PositionMode m_PositionMode = PositionMode.None;
        PositionProgress m_PositionProgress = PositionProgress.None;
        enum PositionMode
        {
            None,
            Feature,
            EdgeBox,
        }
        enum PositionProgress
        {
            None,
            Start,
            Drawing,
            Done,
        }
        public class DrawHelper
        {
            public List<Rectangle> ListDrawnRect = new List<Rectangle>();
            public Rectangle DrawnRect;
            public CPoint Rect_StartPt;
            public CPoint Rect_EndPt;

            public List<Feature> ListFeatureTemp = new List<Feature>();
            public Feature Feature;
        }

    }
}
