using Root_Vega.Module;
using RootTools;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Root_Vega
{
    class _2_7_EdgeBoxViewModel : ObservableObject
    {
        int m_nThreshold = 10;
        public int p_nThreshold
        {
            get { return m_nThreshold; }
            set { SetProperty(ref m_nThreshold, value); }
        }
        protected Dispatcher _dispatcher;
        Vega_Engineer m_Engineer;
        DialogService m_DialogService;
        MemoryTool m_MemoryModule;
        List<string> m_astrMem = new List<String> { "Top", "Left", "Right", "Bottom" };
        public List<DrawHistoryWorker> m_DrawHistoryWorker_List = new List<DrawHistoryWorker>();
        private List<SimpleShapeDrawerVM> m_SimpleShapeDrawer_List = new List<SimpleShapeDrawerVM>();
        public List<SimpleShapeDrawerVM> p_SimpleShapeDrawer_List
        {
            get
            {
                return m_SimpleShapeDrawer_List;
            }
            set
            {
                SetProperty(ref m_SimpleShapeDrawer_List, value);
            }
        }

        public _2_7_EdgeBoxViewModel(Vega_Engineer engineer, IDialogService dialogService)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            m_Engineer = engineer;
            m_DialogService = (DialogService)dialogService;
            Init(dialogService);
        }

        void Init(IDialogService dialogService)
        {

            //p_ImageViewer = new ImageViewer_ViewModel(m_Image, dialogService);
            //p_SimpleShapeDrawer = new PositionDrawerVM(p_ImageViewer);
            //p_SimpleShapeDrawer.RectangleKeyValue = Key.D1;
            //p_ImageViewer.SetDrawer((DrawToolVM)p_SimpleShapeDrawer);

            m_MemoryModule = m_Engineer.ClassMemoryTool();
            if (m_MemoryModule != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    p_ImageViewer_List.Add(new ImageViewer_ViewModel(new ImageData(m_MemoryModule.GetMemory("SideVision.Memory", "SideVision", m_astrMem[i])), dialogService)); //!! m_Image 는 추후 각 part에 맞는 이미지가 들어가게 수정.
                    m_DrawHistoryWorker_List.Add(new DrawHistoryWorker());
                }

                for (int i = 0; i < 4; i++)
                {
                    p_SimpleShapeDrawer_List.Add(new SimpleShapeDrawerVM(p_ImageViewer_List[i]));
                    p_SimpleShapeDrawer_List[i].RectangleKeyValue = Key.D1;
                }

                for (int i = 0; i < 4; i++)
                {
                    p_ImageViewer_List[i].SetDrawer((DrawToolVM)p_SimpleShapeDrawer_List[i]);
                    p_ImageViewer_List[i].m_HistoryWorker = m_DrawHistoryWorker_List[i];
                }

                p_ImageViewer_Top = p_ImageViewer_List[0];
                p_ImageViewer_Left = p_ImageViewer_List[1];
                p_ImageViewer_Right = p_ImageViewer_List[2];
                p_ImageViewer_Bottom = p_ImageViewer_List[3];
            }

            return;
        }

        private List<ImageViewer_ViewModel> m_ImageViewer_List = new List<ImageViewer_ViewModel>();
        public List<ImageViewer_ViewModel> p_ImageViewer_List
        {
            get
            {
                return m_ImageViewer_List;
            }
            set
            {
                SetProperty(ref m_ImageViewer_List, value);
            }
        }

        private ImageViewer_ViewModel m_ImageViewer_Left;
        public ImageViewer_ViewModel p_ImageViewer_Left
        {
            get
            {
                return m_ImageViewer_Left;
            }
            set
            {
                SetProperty(ref m_ImageViewer_Left, value);
            }
        }

        private ImageViewer_ViewModel m_ImageViewer_Top;
        public ImageViewer_ViewModel p_ImageViewer_Top
        {
            get
            {
                return m_ImageViewer_Top;
            }
            set
            {
                SetProperty(ref m_ImageViewer_Top, value);
            }
        }

        private ImageViewer_ViewModel m_ImageViewer_Right;
        public ImageViewer_ViewModel p_ImageViewer_Right
        {
            get
            {
                return m_ImageViewer_Right;
            }
            set
            {
                SetProperty(ref m_ImageViewer_Right, value);
            }
        }

        private ImageViewer_ViewModel m_ImageViewer_Bottom;
        public ImageViewer_ViewModel p_ImageViewer_Bottom
        {
            get
            {
                return m_ImageViewer_Bottom;
            }
            set
            {
                SetProperty(ref m_ImageViewer_Bottom, value);
            }
        }

        enum eEdgeFindDirection { LEFT, TOP, RIGHT, BOTTOM };
        void Inspect()
        {
            Rect rtLeft1 = new Rect(p_SimpleShapeDrawer_List[0].m_ListRect[0].StartPos, p_SimpleShapeDrawer_List[0].m_ListRect[0].EndPos);
            Rect rtLeft2 = new Rect(p_SimpleShapeDrawer_List[0].m_ListRect[1].StartPos, p_SimpleShapeDrawer_List[0].m_ListRect[1].EndPos);
            Rect rtBottom = new Rect(p_SimpleShapeDrawer_List[0].m_ListRect[2].StartPos, p_SimpleShapeDrawer_List[0].m_ListRect[2].EndPos);
            Rect rtRight1 = new Rect(p_SimpleShapeDrawer_List[0].m_ListRect[3].StartPos, p_SimpleShapeDrawer_List[0].m_ListRect[3].EndPos);
            Rect rtRight2 = new Rect(p_SimpleShapeDrawer_List[0].m_ListRect[4].StartPos, p_SimpleShapeDrawer_List[0].m_ListRect[4].EndPos);
            Rect rtTop = new Rect(p_SimpleShapeDrawer_List[0].m_ListRect[5].StartPos, p_SimpleShapeDrawer_List[0].m_ListRect[5].EndPos);
            System.Drawing.Point ptLeft1 = GetEdge(p_ImageViewer_Top.p_ImageData, rtLeft1, eEdgeFindDirection.LEFT);
            System.Drawing.Point ptLeft2 = GetEdge(p_ImageViewer_Top.p_ImageData, rtLeft2, eEdgeFindDirection.LEFT);
            System.Drawing.Point ptBottom = GetEdge(p_ImageViewer_Top.p_ImageData, rtBottom, eEdgeFindDirection.BOTTOM);
            System.Drawing.Point ptRight1 = GetEdge(p_ImageViewer_Top.p_ImageData, rtRight1, eEdgeFindDirection.RIGHT);
            System.Drawing.Point ptRight2 = GetEdge(p_ImageViewer_Top.p_ImageData, rtRight2, eEdgeFindDirection.RIGHT);
            System.Drawing.Point ptTop = GetEdge(p_ImageViewer_Top.p_ImageData, rtTop, eEdgeFindDirection.TOP);

            DrawLine(ptLeft1, ptLeft2);
            DrawLine(ptRight1, ptRight2);

            return;
        }

        void DrawLine(System.Drawing.Point pt1, System.Drawing.Point pt2)
        {
            Line myLine = new Line();
            myLine.Stroke = System.Windows.Media.Brushes.Lime;
            myLine.X1 = pt1.X;
            myLine.X2 = pt2.X;
            myLine.Y1 = pt1.Y;
            myLine.Y2 = pt2.Y;
            myLine.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            myLine.VerticalAlignment = VerticalAlignment.Center;
            myLine.StrokeThickness = 2;

            p_ImageViewer_Top.m_BasicTool.m_ListShape.Add(myLine);
            UIElementInfo uei = new UIElementInfo(new System.Windows.Point(myLine.X1, myLine.Y1), new System.Windows.Point(myLine.X2, myLine.Y2));
            p_ImageViewer_Top.m_BasicTool.m_ListRect.Add(uei);
            p_ImageViewer_Top.m_BasicTool.m_Element.Add(myLine);

            return;
        }

        public RelayCommand CommandInspect
        {
            get
            {
                return new RelayCommand(Inspect);
            }
            set
            {
            }
        }

        System.Drawing.Point GetEdge(ImageData img, Rect rcROI, eEdgeFindDirection eDirection)
        {
            int nSum = 0;
            double dAverage = 0.0;
            int nEdgeY = 0;
            int nEdgeX = 0;

            unsafe
            {
                switch (eDirection)
                {
                    case eEdgeFindDirection.TOP:
                        for (int i = 0; i < rcROI.Height; i++)
                        {
                            byte* bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
                            for (int j = 0; j < rcROI.Width; j++)
                            {
                                nSum += *bp;
                                bp++;
                            }
                            dAverage = nSum / rcROI.Width;
                            if (dAverage < p_nThreshold)
                            {
                                nEdgeY = (int)rcROI.Bottom - i;
                                nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
                                break;
                            }
                            nSum = 0;
                        }
                        break;
                    case eEdgeFindDirection.LEFT:
                        for (int i = 0; i < rcROI.Width; i++)
                        {
                            byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i));
                            for (int j = 0; j < rcROI.Height; j++)
                            {
                                nSum += *bp;
                                bp += img.p_Stride;
                            }
                            dAverage = nSum / rcROI.Height;
                            if (dAverage < p_nThreshold)
                            {
                                nEdgeX = (int)rcROI.Right - i;
                                nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
                                break;
                            }
                            nSum = 0;
                        }
                        break;
                    case eEdgeFindDirection.RIGHT:
                        for (int i = 0; i < rcROI.Width; i++)
                        {
                            byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
                            for (int j = 0; j < rcROI.Height; j++)
                            {
                                nSum += *bp;
                                bp += img.p_Stride;
                            }
                            dAverage = nSum / rcROI.Height;
                            if (dAverage < p_nThreshold)
                            {
                                nEdgeX = (int)rcROI.Left + i;
                                nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
                                break;
                            }
                            nSum = 0;
                        }
                        break;
                    case eEdgeFindDirection.BOTTOM:
                        for (int i = 0; i < rcROI.Height; i++)
                        {
                            byte* bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
                            for (int j = 0; j < rcROI.Width; j++)
                            {
                                nSum += *bp;
                                bp++;
                            }
                            dAverage = nSum / rcROI.Width;
                            if (dAverage < p_nThreshold)
                            {
                                nEdgeY = (int)rcROI.Top + i;
                                nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
                                break;
                            }
                            nSum = 0;
                        }
                        break;
                }
            }

            return new System.Drawing.Point(nEdgeX, nEdgeY);
        }
    }
}
