using Root_CAMELLIA.Data;
using Root_CAMELLIA.DictionarySet;
using Root_CAMELLIA.Draw;
using Root_CAMELLIA.ShapeDraw;
using RootTools;
using RootTools.Inspects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit;

namespace Root_CAMELLIA
{
    public class Dlg_RecipeManager_ViewModel : ObservableObject, IDialogRequestClose
    {
        //private System.Windows.Controls.Primitives.ScrollBar VerticalScroll;
        //private System.Windows.Controls.Primitives.ScrollBar HorizontalScroll;

        private string strXPosition;
        private string strYPosition;
        private string strCurrentTheta;
        private string strCurrentRadius;


        public ObservableCollection<ShapeManager> Shapes = new ObservableCollection<ShapeManager>();
        public ObservableCollection<ShapeManager> PreviewShapes = new ObservableCollection<ShapeManager>();
        public ObservableCollection<GeometryManager> Geometry = new ObservableCollection<GeometryManager>();
        public ObservableCollection<GeometryManager> PreviewGeometry = new ObservableCollection<GeometryManager>();
        public ObservableCollection<GeometryManager> ViewRectGeometry = new ObservableCollection<GeometryManager>();
        public ObservableCollection<GeometryManager> SelectGeometry = new ObservableCollection<GeometryManager>();
        public ObservableCollection<TextManager> TextBlocks = new ObservableCollection<TextManager>();

        private ObservableCollection<UIElement> m_DrawElement = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_DrawElement
        {
            get
            {
                return m_DrawElement;
            }
            set
            {
                m_DrawElement = value;
            }
        }

        private ObservableCollection<UIElement> m_PreviewDrawElement = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_PreviewDrawElement
        {
            get
            {
                return m_PreviewDrawElement;
            }
            set
            {
                m_PreviewDrawElement = value;
            }
        }

        private void TextBlocks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var textBlock = sender as ObservableCollection<TextManager>;
            //ShapeManager shape = shapes.;
            //if(!IsShowIndex && !IsKeyboardShowIndex)
            //{
            //    textBlock[textBlock.Count - 1].Text.Visibility = Visibility.Hidden;
            //}
            p_DrawElement.Add(textBlock[textBlock.Count - 1].Text);
            
        }

        private void Shapes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var shapes = sender as ObservableCollection<ShapeManager>;
            //ShapeManager shape = shapes.;
            p_DrawElement.Add(shapes[shapes.Count - 1].UIElement);
            //foreach (ShapeManager shape in shapes)
            //    {
            //        if (!p_DrawElement.Contains(shape.UIElement))

            //    }
        }

        private void PreviewShapes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var shapes = sender as ObservableCollection<ShapeManager>;
            p_PreviewDrawElement.Add(shapes[shapes.Count - 1].UIElement);
            //foreach (ShapeManager shape in shapes)
            //{
            //    if (!p_PreviewDrawElement.Contains(shape.UIElement))
            //        p_PreviewDrawElement.Add(shape.UIElement);
            //}

        }

        private void Geometry_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            var shapes = sender as ObservableCollection<GeometryManager>;
            foreach (GeometryManager geometry in shapes)
            {

                if (!p_DrawElement.Contains(geometry.path))
                    p_DrawElement.Add(geometry.path);
            }

        }

        private void PreviewGeometry_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            var shapes = sender as ObservableCollection<GeometryManager>;
            foreach (GeometryManager geometry in shapes)
            {

                if (!p_PreviewDrawElement.Contains(geometry.path))
                    p_PreviewDrawElement.Add(geometry.path);
            }

        }

        private void ViewRectGeometry_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            var shapes = sender as ObservableCollection<GeometryManager>;
            foreach (GeometryManager geometry in shapes)
            {

                if (!p_PreviewDrawElement.Contains(geometry.path))
                    p_PreviewDrawElement.Add(geometry.path);
            }

        }

        private void SelectGeometry_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            var shapes = sender as ObservableCollection<GeometryManager>;
            foreach (GeometryManager geometry in shapes)
            {

                if (!p_DrawElement.Contains(geometry.path))
                    p_DrawElement.Add(geometry.path);
            }

        }

        public Dlg_RecipeManager_ViewModel(MainWindow mainWindow)
        {

            // Event
            Shapes.CollectionChanged += Shapes_CollectionChanged;
            Geometry.CollectionChanged += Geometry_CollectionChanged;
            PreviewGeometry.CollectionChanged += PreviewGeometry_CollectionChanged;
            PreviewShapes.CollectionChanged += PreviewShapes_CollectionChanged;
            ViewRectGeometry.CollectionChanged += ViewRectGeometry_CollectionChanged;
            SelectGeometry.CollectionChanged += SelectGeometry_CollectionChanged;
            TextBlocks.CollectionChanged += TextBlocks_CollectionChanged;
            // 추후 수정 예정. 하나로 묶기..

            //VerticalScroll = rcpMgr.VerticalScroll;
            //HorizontalScroll = rcpMgr.HorizontalScroll;
            dataManager = mainWindow.DataManager;
            Init();
            InitStage();
            SetStage(false);
            SetStage(true);
            SetViewRect();

        }

        public DataManager dataManager { get; set; }
        public void SetDataManager(DataManager DM)
        {
            dataManager = DM;
        }

        #region Init
        public void Init()
        {
            //pointListItem = new DataTable();
            PointListItem.Columns.Add(new DataColumn("ListIndex"));
            PointListItem.Columns.Add(new DataColumn("ListX"));
            PointListItem.Columns.Add(new DataColumn("ListY"));
            PointListItem.Columns.Add(new DataColumn("ListRoute"));

          
            RouteThickness = "3";
            RouteThick = 3;
            XPosition = "0.000 mm";
            YPosition = "0.000 mm";
            CurrentTheta = "0 °";
            CurrentRadius = "0.000 mm";
            PointCount = "0";
            Percentage = "0";
            ZoomScale = 1;

            //StageMainView
            //VerticalScroll.Value = VerticalScroll.Minimum;
            //HorizontalScroll.Value = HorizontalScroll.Minimum;

            CenterX = (int)(1000 * 0.5f);
            CenterY = (int)(1000 * 0.5f);
            RatioX = 1000 / BaseDefine.ViewSize;
            RatioY = 1000 / BaseDefine.ViewSize;
            OffsetScale = 100;
            
            DataViewPosition.X = 0;
            DataViewPosition.Y = 0;
            DataViewPosition.Width = 1000;
            DataViewPosition.Height = 1000;
            CurrentCandidatePoint = -1;
            CurrentSelectPoint = -1;

            dataManager.recipeDM.TeachingRD.ClearPoint();

            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.SystemIdle);    //객체생성

            timer.Interval = TimeSpan.FromMilliseconds(100);    //시간간격 설정

            timer.Tick += new EventHandler(MouseHover);          //이벤트 추가

            timer.Start();

        }
        #endregion

        ShapeEllipse shape = new ShapeEllipse();
        private void MouseHover(object sender, EventArgs e)
        {
            Thread.Sleep(1);
            if (IsLockUI)
            {
                return;
            }
            if (StageMouseHover)
            {
                bool bSelected = false;

                int nIndex = 0;
                double dMin = 9999;
                int nMinIndex = 0;

                foreach (ShapeEllipse se in listCandidatePoint)
                {
                    double dDistance = GetDistance(se, new System.Windows.Point(MousePoint.X, MousePoint.Y));

                    if (dDistance < dMin)
                    {
                        dMin = dDistance;
                        nMinIndex = nIndex;
                    }
                    nIndex++;
                }

                int idx = 0;
                if (!SetStartEndPointMode)
                {
                    foreach (ShapeEllipse se in listCandidatePoint)
                    {
                        if (se.Equals(listCandidatePoint[nMinIndex]))
                        {
                            bSelected = true;
                            se.SetBrush(GeneralTools.GbSelect);
                            listPreviewCandidatePoint[nMinIndex].SetBrush(GeneralTools.GbSelect);
                            shape = se;
                        }
                        else
                        {
                            se.SetBrush(GeneralTools.StageHoleBrush);
                            listPreviewCandidatePoint[idx].SetBrush(GeneralTools.StageHoleBrush);
                        }
                        idx++;
                    }
                    if (bSelected)
                    {
                        CurrentCandidatePoint = listCandidatePoint.IndexOf(shape);
                    }
                    else
                    {
                        CurrentCandidatePoint = -1;
                    }
                }
                bSelected = false;
                idx = 0;
                int nDummyIdx = -1;
                foreach (ShapeEllipse se in listSelectedPoint)
                {
                    if (se.CenterX == listCandidatePoint[nMinIndex].CenterX && se.CenterY == listCandidatePoint[nMinIndex].CenterY)
                    {
                        bSelected = true;
                        se.SetBrush(GeneralTools.SelectedOverBrush);
                        listPreviewSelectedPoint[idx].SetBrush(GeneralTools.SelectedOverBrush);
                        shape = se;
                    }
                    else
                    {
                        if (SetStartEndPointMode)
                        {
                            if (dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count > 0)
                            {
                                CCircle circle = dataManager.recipeDM.TeachingRD.DataSelectedPoint[idx];
                                if (ContainsData(ListReorderPoint, circle, out nDummyIdx))
                                {
                                    se.SetBrush(System.Windows.Media.Brushes.Cyan);
                                    listPreviewSelectedPoint[idx].SetBrush(System.Windows.Media.Brushes.Cyan);
                                }
                                else
                                {
                                    se.SetBrush(System.Windows.Media.Brushes.DarkBlue);
                                    listPreviewSelectedPoint[idx].SetBrush(System.Windows.Media.Brushes.DarkBlue);
                                }
                            }
                            else
                            {
                                se.SetBrush(System.Windows.Media.Brushes.DarkBlue);
                                listPreviewSelectedPoint[idx].SetBrush(System.Windows.Media.Brushes.DarkBlue);
                            }
                        }
                        else
                        {
                        se.SetBrush(GeneralTools.GbHole);
                        listPreviewSelectedPoint[idx].SetBrush(GeneralTools.GbHole);

                        }
                    }
                    idx++;
                }
                if (bSelected)
                {
                    CurrentCandidatePoint = -1;
                    CurrentSelectPoint = listSelectedPoint.IndexOf(shape);
                }
                else
                {
                    CurrentSelectPoint = -1;
                }
            }
            else
            {
                if (StageMouseHoverUpdate)
                {
                    int idx = 0;
                  
                    foreach (ShapeEllipse se in listCandidatePoint)
                    {
                        se.SetBrush(GeneralTools.StageHoleBrush);
                        listPreviewCandidatePoint[idx].SetBrush(GeneralTools.StageHoleBrush);
                        idx++;
                    }
                    idx = 0;
                    foreach (ShapeEllipse se in listSelectedPoint)
                    {
                        
                        if (SetStartEndPointMode)
                        {
                            int nDummyidx = -1;
                            CCircle circle = dataManager.recipeDM.TeachingRD.DataSelectedPoint[idx];
                            if (ContainsData(ListReorderPoint, circle, out nDummyidx))
                            {
                                se.SetBrush(System.Windows.Media.Brushes.Cyan);
                                listPreviewSelectedPoint[idx].SetBrush(System.Windows.Media.Brushes.Cyan);
                            }
                            else
                            {
                                se.SetBrush(System.Windows.Media.Brushes.DarkBlue);
                                listPreviewSelectedPoint[idx].SetBrush(System.Windows.Media.Brushes.DarkBlue);
                            }
                            //se.SetBrush(System.Windows.Media.Brushes.DarkBlue);
                            //listPreviewSelectedPoint[idx].SetBrush(System.Windows.Media.Brushes.DarkBlue);
                        }
                        else
                        {
                        se.SetBrush(GeneralTools.GbHole);
                        listPreviewSelectedPoint[idx].SetBrush(GeneralTools.GbHole);
                        }
                        idx++;
                    }
                    StageMouseHoverUpdate = false;
                }
             
            }
        }
        #region Stage

        #region DataStage     

        private Rect DataViewPosition = new Rect();
        #endregion

        #region ViewStage
        public Circle viewStageField = new Circle();
        public ShapeDraw.Line viewStageLineHole = new ShapeDraw.Line();
        public Arc[] viewStageEdgeHoleArc = new Arc[8];
        public Circle[] ViewStageGuideLine = new Circle[4];
        public Arc[] viewStageDoubleHoleArc = new Arc[8];

        public Arc[] viewStageTopHoleArc = new Arc[2];
        public Arc[] viewStageBotHoleArc = new Arc[2];
        #endregion

        #region Geometry, Shape
        private DrawGeometryManager drawGeometryManager = new DrawGeometryManager();

        private List<ShapeEllipse> listCandidatePoint = new List<ShapeEllipse>();
        private List<ShapeEllipse> listPreviewCandidatePoint = new List<ShapeEllipse>();
        private List<ShapeEllipse> listSelectedPoint = new List<ShapeEllipse>();
        private List<ShapeEllipse> listPreviewSelectedPoint = new List<ShapeEllipse>();

        #endregion
        #region Getter Setter
        //public Circle DataStageField { get; set; } = new Circle();
        //public ShapeDraw.Line DataStageLineHole { get; set; } = new ShapeDraw.Line();
        //public PointF[] DataStageEdgeHolePoint { get; set; } = new PointF[64];
        //public Arc[] DataStageEdgeHoleArc { get; set; } = new Arc[8];
        //public Circle[] DataStageGuideLine { get; set; } = new Circle[4];
        //public Arc[] DataStageDoubleHoleArc { get; set; } = new Arc[8];
        //public Arc[] DataStageTopHoleArc { get; set; } = new Arc[2];
        //public Arc[] DataStageBotHoleArc { get; set; } = new Arc[2];

        //public int ArcPointNum { get; set; }
        //public int ArcPointNum2 { get; set; }
        //public int EdgeNum { get; set; }
        //public int DoubleHoleNum { get; set; }
        //public int GuideLineNum { get; set; }
   
        #endregion

        private void InitStage()
        {
            //ArcPointNum = 63;
            //EdgeNum = 4;
            //DoubleHoleNum = 4;
            //GuideLineNum = 4;

            //DataStageField.Set(0, 0, BaseDefine.ViewSize, BaseDefine.ViewSize);
            //DataStageLineHole.Set(0, 0, BaseDefine.ViewSize, 7);
            OpenStageCircleHole();


            //double dRadiusIn = 130;
            //double dRadiusOut = 155;
            //DataStageEdgeHolePoint[0] = new PointF((float)15, (float)Math.Sqrt(16675));
            //DataStageEdgeHolePoint[1] = new PointF((float)Math.Sqrt(16347.75), (float)23.5);
            //DataStageEdgeHolePoint[2] = new PointF((float)Math.Sqrt(23472.75), (float)23.5);
            //DataStageEdgeHolePoint[3] = new PointF((float)15, (float)Math.Sqrt(23800));
            //DataStageEdgeHolePoint[4] = new PointF((float)Math.Sqrt(16347.75), (float)-23.5);
            //DataStageEdgeHolePoint[5] = new PointF((float)15, (float)-Math.Sqrt(16675));
            //DataStageEdgeHolePoint[6] = new PointF((float)15, (float)-Math.Sqrt(23800));
            //DataStageEdgeHolePoint[7] = new PointF((float)Math.Sqrt(23472.75), (float)-23.5);
            //DataStageEdgeHolePoint[8] = new PointF((float)-15, (float)-Math.Sqrt(16675));
            //DataStageEdgeHolePoint[9] = new PointF((float)-Math.Sqrt(16347.75), (float)-23.5);
            //DataStageEdgeHolePoint[10] = new PointF((float)-Math.Sqrt(23472.75), (float)-23.5);
            //DataStageEdgeHolePoint[11] = new PointF((float)-15, (float)-Math.Sqrt(23800));
            //DataStageEdgeHolePoint[12] = new PointF((float)-Math.Sqrt(16347.75), (float)23.5);
            //DataStageEdgeHolePoint[13] = new PointF((float)-15, (float)Math.Sqrt(16675));
            //DataStageEdgeHolePoint[14] = new PointF((float)-15, (float)Math.Sqrt(23800));
            //DataStageEdgeHolePoint[15] = new PointF((float)-Math.Sqrt(23472.75), (float)23.5);

            //for (int i = 0; i < EdgeNum; i++)
            //{
            //    DataStageEdgeHoleArc[2 * i + 0] = new Arc(0, 0, dRadiusIn, Math.Atan2(DataStageEdgeHolePoint[4 * i + 0].Y, DataStageEdgeHolePoint[4 * i + 0].X), Math.Atan2(DataStageEdgeHolePoint[4 * i + 1].Y, DataStageEdgeHolePoint[4 * i + 1].X), ArcPointNum, false);
            //    DataStageEdgeHoleArc[2 * i + 1] = new Arc(0, 0, dRadiusOut, Math.Atan2(DataStageEdgeHolePoint[4 * i + 2].Y, DataStageEdgeHolePoint[4 * i + 2].X), Math.Atan2(DataStageEdgeHolePoint[4 * i + 3].Y, DataStageEdgeHolePoint[4 * i + 3].X), ArcPointNum, false);
            //}

            //double dRadiusHole = 6;
            //double dInLength = 69.3;
            //double dOutLength = 77.85;
            //for (int i = 0; i < 2 * DoubleHoleNum; i++)
            //{
            //    DataStageDoubleHoleArc[i] = new Arc();
            //}
            //DataStageDoubleHoleArc[0].InitArc(dInLength, dInLength, dRadiusHole, (3 / (float)4) * Math.PI, (7 / (float)4) * Math.PI, ArcPointNum, true);
            //DataStageDoubleHoleArc[1].InitArc(dOutLength, dOutLength, dRadiusHole, (7 / (float)4) * Math.PI, (3 / (float)4) * Math.PI, ArcPointNum, true);
            //DataStageDoubleHoleArc[2].InitArc(dInLength, -dInLength, dRadiusHole, (1 / (float)4) * Math.PI, (5 / (float)4) * Math.PI, ArcPointNum, true);
            //DataStageDoubleHoleArc[3].InitArc(dOutLength, -dOutLength, dRadiusHole, (5 / (float)4) * Math.PI, (1 / (float)4) * Math.PI, ArcPointNum, true);
            //DataStageDoubleHoleArc[4].InitArc(-dInLength, -dInLength, dRadiusHole, (7 / (float)4) * Math.PI, (3 / (float)4) * Math.PI, ArcPointNum, true);
            //DataStageDoubleHoleArc[5].InitArc(-dOutLength, -dOutLength, dRadiusHole, (3 / (float)4) * Math.PI, (7 / (float)4) * Math.PI, ArcPointNum, true);
            //DataStageDoubleHoleArc[6].InitArc(-dInLength, dInLength, dRadiusHole, (5 / (float)4) * Math.PI, (1 / (float)4) * Math.PI, ArcPointNum, true);
            //DataStageDoubleHoleArc[7].InitArc(-dOutLength, dOutLength, dRadiusHole, (1 / (float)4) * Math.PI, (5 / (float)4) * Math.PI, ArcPointNum, true);

            //DataStageTopHoleArc[0] = new Arc(0, 145, dRadiusHole, Math.PI, 0, ArcPointNum, true);
            //DataStageTopHoleArc[1] = new Arc(0, 0, dRadiusOut, Math.Atan2(Math.Sqrt(23989), 6), Math.Atan2(Math.Sqrt(23989), -6), ArcPointNum, true);
            //DataStageBotHoleArc[0] = new Arc(0, -145, dRadiusHole, 0, Math.PI, ArcPointNum, true);
            //DataStageBotHoleArc[1] = new Arc(0, 0, dRadiusOut, Math.Atan2(-Math.Sqrt(23989), -6), Math.Atan2(-Math.Sqrt(23989), 6), ArcPointNum, true);


            //for (int i = 0; i < GuideLineNum; i++)
            //{
            //    DataStageGuideLine[i] = new Circle();

            //}
            //DataStageGuideLine[0].Set(0, 0, 49, 49);         
            //DataStageGuideLine[1].Set(0, 0, 98, 98);
            //DataStageGuideLine[2].Set(0, 0, 150, 150);
            //DataStageGuideLine[3].Set(0, 0, 196, 196);
        }

        public List<Circle> dataStageCircleHole = new List<Circle>();
        Circle viewStageCircleHole = new Circle();
        public void OpenStageCircleHole()
        {
            if (dataStageCircleHole.Count != 0)
            {
                dataStageCircleHole.Clear();
            }

            string fileName = BaseDefine.Dir_StageHole; //Todo 수정해야함
            StreamReader sr = new StreamReader(fileName);
            while (!sr.EndOfStream)
            {
                string sLine = sr.ReadLine().Trim();
                string sText = sLine.Substring(sLine.IndexOf(':') + 1);

                if (sLine == string.Empty || sText == string.Empty)
                {
                    return;
                }

                if (sText.IndexOf('~') == -1)
                {
                    string[] str = sText.Split(',');

                    Circle circle = new Circle(double.Parse(str[0]), double.Parse(str[1]), double.Parse(str[2]), double.Parse(str[2]));
                    dataStageCircleHole.Add(circle);
                }
            }
        }

        private void RedrawStage()
        {
            int index = 0;
            //stage.
            CustomEllipseGeometry stageField = Geometry[index] as CustomEllipseGeometry;
            viewStageField.Set(GeneralTools.DataStageField);
            viewStageField.Transform(RatioX, RatioY);
            viewStageField.ScaleOffset(ZoomScale, OffsetX, OffsetY);
            stageField.SetData(viewStageField, CenterX, CenterY);
            Geometry[index] = stageField;
            index++;

            CustomRectangleGeometry rectLine = Geometry[index] as CustomRectangleGeometry;
            viewStageLineHole.Set(GeneralTools.DataStageLineHole);
            viewStageLineHole.Transform(RatioX, RatioY);
            viewStageLineHole.ScaleOffset(ZoomScale, OffsetX, OffsetY);
            rectLine.SetData(drawGeometryManager.GetRect(viewStageLineHole, CenterX, CenterY));
            Geometry[index] = rectLine;
            index++;
            for (int i = 0; i < GeneralTools.GuideLineNum; i++)
            {
                CustomEllipseGeometry guideLine = Geometry[index] as CustomEllipseGeometry;
                ViewStageGuideLine[i].Set(GeneralTools.DataStageGuideLine[i]);
                ViewStageGuideLine[i].Transform(RatioX, RatioY);
                ViewStageGuideLine[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
                guideLine.SetData(ViewStageGuideLine[i], CenterX, CenterY, 5 * ZoomScale);
                Geometry[index] = guideLine;
                index++;
            }

            // 엣지부분 흰색 영역
            for (int i = 0; i < 2 * GeneralTools.EdgeNum; i++)
            {
                viewStageEdgeHoleArc[i].Set(GeneralTools.DataStageEdgeHoleArc[i]);
                viewStageEdgeHoleArc[i].Transform(RatioX, RatioY);
                viewStageEdgeHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
            }

            PointF[] points;
            PointF[] pt = new PointF[2];
            System.Windows.Point StartPoint;
            for (int n = 0; n < GeneralTools.EdgeNum; n++)
            {
                CustomPathGeometry edgeArc = Geometry[index] as CustomPathGeometry;

                PathFigure path = drawGeometryManager.AddDoubleHole(viewStageEdgeHoleArc[2 * n + 0], viewStageEdgeHoleArc[2 * n + 1], CenterX, CenterY);

                edgeArc.SetData(path);
                Geometry[index] = edgeArc;
                index++;
                drawGeometryManager.ClearSegments();
            }


            // 긴 타원형 홀
            for (int i = 0; i < 2 * GeneralTools.DoubleHoleNum; i++)
            {
                viewStageDoubleHoleArc[i].Set(GeneralTools.DataStageDoubleHoleArc[i]);
                viewStageDoubleHoleArc[i].Transform(RatioX, RatioY);
                viewStageDoubleHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
            }

            for (int i = 0; i < GeneralTools.DoubleHoleNum; i++)
            {
                CustomPathGeometry doubleHole = Geometry[index] as CustomPathGeometry;
                PathFigure path = drawGeometryManager.AddDoubleHole(viewStageDoubleHoleArc[2 * i + 0], viewStageDoubleHoleArc[2 * i + 1], CenterX, CenterY);

                doubleHole.SetData(path);
                Geometry[index] = doubleHole;
                index++;

                drawGeometryManager.ClearSegments();
            }

            // 윗부분 및 아랫부분 타원홀
            for (int i = 0; i < 2; i++)
            {
                viewStageTopHoleArc[i].Set(GeneralTools.DataStageTopHoleArc[i]);
                viewStageTopHoleArc[i].Transform(RatioX, RatioY);
                viewStageTopHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);

                viewStageBotHoleArc[i].Set(GeneralTools.DataStageBotHoleArc[i]);
                viewStageBotHoleArc[i].Transform(RatioX, RatioY);
                viewStageBotHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
            }

            Arc[] arc;
            for (int i = 0; i < 2; i++)
            {
                CustomPathGeometry topBotDoubleHole = Geometry[index] as CustomPathGeometry;
                if (i == 0)
                {
                    arc = viewStageTopHoleArc;
                }
                else
                {
                    arc = viewStageBotHoleArc;
                }

                PathFigure path = drawGeometryManager.AddDoubleHole(arc[0], arc[1], CenterX, CenterY);

                topBotDoubleHole.SetData(path);
                Geometry[index] = topBotDoubleHole;
                index++;
                drawGeometryManager.ClearSegments();
            }

            // 스테이지 홀
            int idx = 0;
            foreach (Circle circle in dataStageCircleHole)
            {
                CustomEllipseGeometry circleHole = Geometry[index] as CustomEllipseGeometry;
                viewStageCircleHole.Set(circle);
                viewStageCircleHole.Transform(RatioX, RatioY);
                viewStageCircleHole.ScaleOffset(ZoomScale, OffsetX, OffsetY);
                drawGeometryManager.GetRect(ref viewStageCircleHole, CenterX, CenterY);
                circleHole.SetData(viewStageCircleHole, (int)(viewStageCircleHole.Width / 2),
                    (int)(viewStageCircleHole.Y + (viewStageCircleHole.Height / 2) + viewStageCircleHole.Y));
                Geometry[index] = circleHole;
                index++;
                idx++;
            }


            // 스테이지 엣지

            CustomEllipseGeometry stageEdge = Geometry[index] as CustomEllipseGeometry;

            viewStageField.Set(GeneralTools.DataStageField);
            viewStageField.Transform(RatioX, RatioY);
            viewStageField.ScaleOffset(ZoomScale, OffsetX, OffsetY);
            stageEdge.SetData(viewStageField, CenterX, CenterY, 3 * ZoomScale);
            Geometry[index] = stageEdge;

            index++;

            int shapeIndex = 0;
            for (int i = 0; i < dataManager.recipeDM.TeachingRD.DataCandidatePoint.Count; i++)
            {
                ShapeEllipse dataCandidatePoint = Shapes[i] as ShapeEllipse;
                CCircle circle = new CCircle(dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].x, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].y, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].width,
                    dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].height, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].MeasurementOffsetX, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].MeasurementOffsetY);
                circle.Transform(RatioX, RatioY);

                circle.ScaleOffset(ZoomScale, OffsetX, OffsetY);

                Circle c = drawGeometryManager.GetRect(circle, CenterX, CenterY);
                dataCandidatePoint.SetData(c, (int)(circle.width), (int)(circle.height));
                Shapes[i] = dataCandidatePoint;
                shapeIndex++;
            }



            if (ShowRoute)
            {
                for (int i = 0; i < dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Count - 1; i++)
                {
                    CustomPathGeometry routeLine = Geometry[index] as CustomPathGeometry;
                    CCircle from = dataManager.recipeDM.TeachingRD.DataSelectedPoint[dataManager.recipeDM.TeachingRD.DataMeasurementRoute[i]];
                    CCircle to = dataManager.recipeDM.TeachingRD.DataSelectedPoint[dataManager.recipeDM.TeachingRD.DataMeasurementRoute[i + 1]];

                    from.Transform(RatioX, RatioY);
                    from.ScaleOffset(ZoomScale, OffsetX, OffsetY);

                    to.Transform(RatioX, RatioY);
                    to.ScaleOffset(ZoomScale, OffsetX, OffsetY);

                    PointF[] line = { new PointF((float)from.x + CenterX, (float)-from.y + CenterY), new PointF((float)to.x + CenterX, (float)-to.y + CenterY) };
                    StartPoint = new System.Windows.Point((float)from.x + CenterX, (float)-from.y + CenterY); // 시작포인트
                    drawGeometryManager.AddLine(line);
                    routeLine.SetBrush(RouteBrush);
                    routeLine.SetData(drawGeometryManager.GetPathFigure(StartPoint), RouteThick * ZoomScale);
                    Geometry[index] = routeLine;
                    drawGeometryManager.ClearSegments();
                    index++;
                }
            }


            int nDummyIdx = -1;
            for (int i = 0; i < dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count; i++)
            {
                ShapeEllipse dataSelectedPoint = Shapes[shapeIndex] as ShapeEllipse;
                CCircle circle = new CCircle(dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].x, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].y, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].width,
                    dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].height, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].MeasurementOffsetX, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].MeasurementOffsetY);
                circle.Transform(RatioX, RatioY);

                circle.ScaleOffset(ZoomScale, OffsetX, OffsetY);
                Circle c = drawGeometryManager.GetRect(circle, CenterX, CenterY);
                dataSelectedPoint.SetData(c, (int)(circle.width), (int)(circle.height), true);
                if (SetStartEndPointMode)
                {
                    CCircle reorderCircle = dataManager.recipeDM.TeachingRD.DataSelectedPoint[i];
                    if (ContainsData(ListReorderPoint, reorderCircle, out nDummyIdx))
                    {
                        dataSelectedPoint.SetBrush(System.Windows.Media.Brushes.Cyan);
                    }
                    else
                    {
                        dataSelectedPoint.SetBrush(System.Windows.Media.Brushes.DarkBlue);
                    }
                }
                else
                {
                    dataSelectedPoint.SetBrush(GeneralTools.GbHole);
                }
                Shapes[shapeIndex] = dataSelectedPoint;
                shapeIndex++;

                if (IsShowIndex || IsKeyboardShowIndex)
                {
                    if(TextBlocks.Count > 0)
                    {
                        TextManager textBlock = TextBlocks[i];
                        textBlock.SetData((RouteOrder[i] + 1).ToString(), (int)c.Width, dataSelectedPoint.CanvasLeft, dataSelectedPoint.CanvasTop - c.Height);
                        textBlock.SetVisibility(true);
                        TextBlocks[i] = textBlock;

                    }
                }
                else if(!IsShowIndex && !IsKeyboardShowIndex)
                {
                    if (TextBlocks.Count > 0)
                    {
                        TextManager textBlock = TextBlocks[i];
                        textBlock.SetVisibility(false);
                        TextBlocks[i] = textBlock;

                    }
                }

            }





            Rect rect = DataViewPosition;
            rect.X = rect.X / ZoomScale - OffsetX / ZoomScale * 1000 / (double)1000;
            rect.Y = rect.Y / ZoomScale - OffsetY / ZoomScale * 1000 / (double)1000;
            rect.Width /= ZoomScale;
            rect.Height /= ZoomScale;

            Rect ViewRect = drawGeometryManager.GetRect(rect, CenterX, CenterY);
            if (ViewRect.X < 0)
            {
                if (ViewRect.Width + ViewRect.X < 0)
                {
                    ViewRect.Width = 0;
                }
                else
                {
                    ViewRect.Width += ViewRect.X;
                }
                ViewRect.X = 0;
            }
            if (ViewRect.Y < 0)
            {

                if (ViewRect.Height + ViewRect.Y < 0)
                {
                    ViewRect.Height = 0;
                }
                else
                {
                    ViewRect.Height += ViewRect.Y;
                }
                ViewRect.Y = 0;
            }
            double dHeight = 1000 - ViewRect.Y - ViewRect.Height;
            if (ViewRect.Y + ViewRect.Height > 1000)
            {
                dHeight = 0;
            }
            double dWidth = 1000 - ViewRect.X - ViewRect.Width;
            if (ViewRect.X + ViewRect.Width > 1000)
            {
                dWidth = 0;
            }
            Rect TopRect = new Rect(ViewRect.X, 0, ViewRect.Width, ViewRect.Y);
            Rect BottomRect = new Rect(ViewRect.X, ViewRect.Y + ViewRect.Height, ViewRect.Width, dHeight);
            Rect LeftRect = new Rect(0, 0, ViewRect.X, 1000);
            Rect RightRect = new Rect(ViewRect.X + ViewRect.Width, 0, dWidth, 1000);

            CustomRectangleGeometry stageShade = ViewRectGeometry[0] as CustomRectangleGeometry;
            stageShade.SetGroupData(TopRect, 0);
            stageShade.SetGroupData(BottomRect, 1);
            stageShade.SetGroupData(LeftRect, 2);
            stageShade.SetGroupData(RightRect, 3);
            ViewRectGeometry[0] = stageShade;

            // select Rect
            CustomRectangleGeometry select = SelectGeometry[0] as CustomRectangleGeometry;
            Rect selectRect = new Rect(Math.Min(SelectStartPoint.X, SelectEndPoint.X), Math.Min(SelectStartPoint.Y, SelectEndPoint.Y),
                Math.Abs(SelectStartPoint.X - SelectEndPoint.X), Math.Abs(SelectStartPoint.Y - SelectEndPoint.Y));
            select.SetData(selectRect);
            SelectGeometry[0] = select;

        }

        GeometryManager viewRect;
        private void SetViewRect()
        {
            //Preview Stage Shade
            {

                Rect rect = DataViewPosition;
                rect.X = rect.X / ZoomScale - OffsetX / ZoomScale * 1000 / (double)1000;
                rect.Y = rect.Y / ZoomScale - OffsetY / ZoomScale * 1000 / (double)1000;
                rect.Width /= ZoomScale;
                rect.Height /= ZoomScale;

                Rect ViewRect = drawGeometryManager.GetRect(rect, CenterX, CenterY);
                if (ViewRect.X < 0)
                {
                    if (ViewRect.Width + ViewRect.X < 0)
                    {
                        ViewRect.Width = 0;
                    }
                    else
                    {
                        ViewRect.Width += ViewRect.X;
                    }
                    ViewRect.X = 0;
                }
                if (ViewRect.Y < 0)
                {

                    if (ViewRect.Height + ViewRect.Y < 0)
                    {
                        ViewRect.Height = 0;
                    }
                    else
                    {
                        ViewRect.Height += ViewRect.Y;
                    }
                    ViewRect.Y = 0;
                }
                double dHeight = 1000 - ViewRect.Y - ViewRect.Height;
                if (ViewRect.Y + ViewRect.Height > 1000)
                {
                    dHeight = 0;
                }
                double dWidth = 1000 - ViewRect.X - ViewRect.Width;
                if (ViewRect.X + ViewRect.Width > 1000)
                {
                    dWidth = 0;
                }
                Rect TopRect = new Rect(ViewRect.X, 0, ViewRect.Width, ViewRect.Y);
                Rect BottomRect = new Rect(ViewRect.X, ViewRect.Y + ViewRect.Height, ViewRect.Width, dHeight);
                Rect LeftRect = new Rect(0, 0, ViewRect.X, 1000);
                Rect RightRect = new Rect(ViewRect.X + ViewRect.Width, 0, dWidth, 1000);

                viewRect = new CustomRectangleGeometry(GeneralTools.StageShadeBrush);
                CustomRectangleGeometry stageShade = viewRect as CustomRectangleGeometry;

                stageShade.SetData(TopRect);
                stageShade.AddGroup(stageShade);

                CustomRectangleGeometry botRect = new CustomRectangleGeometry(GeneralTools.StageShadeBrush);
                botRect.SetData(BottomRect);
                stageShade.AddGroup(botRect);

                CustomRectangleGeometry leftRect = new CustomRectangleGeometry(GeneralTools.StageShadeBrush);
                leftRect.SetData(LeftRect);
                stageShade.AddGroup(leftRect);

                CustomRectangleGeometry rightRect = new CustomRectangleGeometry(GeneralTools.StageShadeBrush);
                rightRect.SetData(RightRect);
                stageShade.AddGroup(rightRect);
                ViewRectGeometry.Add(stageShade);
            }
            ViewRectGeometry.CollectionChanged -= ViewRectGeometry_CollectionChanged;
        }


        GeometryManager stage;
        TextManager textManager;
        ShapeManager dataPoint;
        GeometryManager selectRectangle;
        private void SetStage(bool preview)
        {
            GeneralTools.GbHole.GradientOrigin = new System.Windows.Point(0.3, 0.3);
            // 스테이지
            stage = new CustomEllipseGeometry(GeneralTools.Gb, System.Windows.SystemColors.ControlBrush);
            CustomEllipseGeometry stageField = stage as CustomEllipseGeometry;

            viewStageField.Set(GeneralTools.DataStageField);
            viewStageField.Transform(RatioX, RatioY);
            if (!preview)
            {
                viewStageField.ScaleOffset(ZoomScale, OffsetX, OffsetY);
            }
            stageField.SetData(viewStageField, CenterX, CenterY);
            if (!preview)
            {
                Geometry.Add(stageField);
            }
            else
            {
                PreviewGeometry.Add(stageField);
            }

            // Stage 중간 흰색 라인
            stage = new CustomRectangleGeometry(GeneralTools.ActiveBrush, GeneralTools.ActiveBrush);
            CustomRectangleGeometry rectLine = stage as CustomRectangleGeometry;
            viewStageLineHole.Set(GeneralTools.DataStageLineHole);
            viewStageLineHole.Transform(RatioX, RatioY);
            if (!preview)
            {
                viewStageLineHole.ScaleOffset(ZoomScale, OffsetX, OffsetY);
            }
            rectLine.SetData(drawGeometryManager.GetRect(viewStageLineHole, CenterX, CenterY));
            if (!preview)
            {
                Geometry.Add(rectLine);
            }
            else
            {
                PreviewGeometry.Add(rectLine);
            }

            // Stage 점선 가이드라인
            for (int i = 0; i < GeneralTools.GuideLineNum; i++)
            {

                stage = new CustomEllipseGeometry(GeneralTools.GuideLineBrush, "3,1", 5, 0.1d);

                CustomEllipseGeometry guideLine = stage as CustomEllipseGeometry;
                ViewStageGuideLine[i] = new Circle();
                ViewStageGuideLine[i].Set(GeneralTools.DataStageGuideLine[i]);
                ViewStageGuideLine[i].Transform(RatioX, RatioY);
                if (!preview)
                {
                    ViewStageGuideLine[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
                    guideLine.SetData(ViewStageGuideLine[i], CenterX, CenterY, 5 * ZoomScale);
                    Geometry.Add(guideLine);
                }
                else
                {
                    guideLine.SetData(ViewStageGuideLine[i], CenterX, CenterY, 5);
                    PreviewGeometry.Add(guideLine);
                }
            }

            // 엣지부분 흰색 영역
            for (int i = 0; i < 2 * GeneralTools.EdgeNum; i++)
            {
                viewStageEdgeHoleArc[i] = new Arc();
                viewStageEdgeHoleArc[i].Set(GeneralTools.DataStageEdgeHoleArc[i]);
                viewStageEdgeHoleArc[i].Transform(RatioX, RatioY);
                if (!preview)
                {
                    viewStageEdgeHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
                }
            }

            PointF[] points;
            PointF[] pt = new PointF[2];
            System.Windows.Point StartPoint;
            for (int n = 0; n < GeneralTools.EdgeNum; n++)
            {
                stage = new CustomPathGeometry(GeneralTools.ActiveBrush);
                CustomPathGeometry edgePath = stage as CustomPathGeometry;

                PathFigure path = drawGeometryManager.AddDoubleHole(viewStageEdgeHoleArc[2 * n + 0], viewStageEdgeHoleArc[2 * n + 1], CenterX, CenterY);

                edgePath.SetData(path);
                if (!preview)
                {
                    Geometry.Add(edgePath);
                }
                else
                {
                    PreviewGeometry.Add(edgePath);
                }
                drawGeometryManager.ClearSegments();
            }


            // 긴 타원형 홀
            for (int i = 0; i < 2 * GeneralTools.DoubleHoleNum; i++)
            {

                viewStageDoubleHoleArc[i] = new Arc();
                viewStageDoubleHoleArc[i].Set(GeneralTools.DataStageDoubleHoleArc[i]);
                viewStageDoubleHoleArc[i].Transform(RatioX, RatioY);
                if (!preview)
                {
                    viewStageDoubleHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
                }

            }

            for (int i = 0; i < GeneralTools.DoubleHoleNum; i++)
            {
                stage = new CustomPathGeometry(GeneralTools.ActiveBrush);
                CustomPathGeometry doubleHole = stage as CustomPathGeometry;

                PathFigure path = drawGeometryManager.AddDoubleHole(viewStageDoubleHoleArc[2 * i + 0], viewStageDoubleHoleArc[2 * i + 1], CenterX, CenterY);

                doubleHole.SetData(path);
                if (!preview)
                {
                    Geometry.Add(doubleHole);
                }
                else
                {
                    PreviewGeometry.Add(doubleHole);
                }
                drawGeometryManager.ClearSegments();
            }

            // 윗부분 및 아랫부분 타원홀
            for (int i = 0; i < 2; i++)
            {
                viewStageTopHoleArc[i] = new Arc();
                viewStageTopHoleArc[i].Set(GeneralTools.DataStageTopHoleArc[i]);
                viewStageTopHoleArc[i].Transform(RatioX, RatioY);
                if (!preview)
                {
                    viewStageTopHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
                }
                viewStageBotHoleArc[i] = new Arc();
                viewStageBotHoleArc[i].Set(GeneralTools.DataStageBotHoleArc[i]);
                viewStageBotHoleArc[i].Transform(RatioX, RatioY);
                if (!preview)
                {
                    viewStageBotHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
                }
            }

            Arc[] arc;
            for (int i = 0; i < 2; i++)
            {
                stage = new CustomPathGeometry(GeneralTools.ActiveBrush);
                CustomPathGeometry topBotDoubleHole = stage as CustomPathGeometry;
                if (i == 0)
                {
                    arc = viewStageTopHoleArc;
                }
                else
                {
                    arc = viewStageBotHoleArc;
                }

                PathFigure path = drawGeometryManager.AddDoubleHole(arc[0], arc[1], CenterX, CenterY);

                topBotDoubleHole.SetData(path);
                if (!preview)
                {
                    Geometry.Add(topBotDoubleHole);
                }
                else
                {
                    PreviewGeometry.Add(topBotDoubleHole);
                }
                drawGeometryManager.ClearSegments();
            }


            // 스테이지 홀
            foreach (Circle circle in dataStageCircleHole)
            {
                stage = new CustomEllipseGeometry(GeneralTools.ActiveBrush, GeneralTools.ActiveBrush);
                CustomEllipseGeometry circleHole = stage as CustomEllipseGeometry;
                viewStageCircleHole.Set(circle);
                viewStageCircleHole.Transform(RatioX, RatioY);
                if (!preview)
                {
                    viewStageCircleHole.ScaleOffset(ZoomScale, OffsetX, OffsetY);
                }
                drawGeometryManager.GetRect(ref viewStageCircleHole, CenterX, CenterY);
                circleHole.SetData(viewStageCircleHole, (int)(viewStageCircleHole.Width / 2),
                    (int)(viewStageCircleHole.Y + (viewStageCircleHole.Height / 2) + viewStageCircleHole.Y));
                if (!preview)
                {
                    Geometry.Add(circleHole);
                }
                else
                {
                    PreviewGeometry.Add(circleHole);
                }
            }


            // 스테이지 엣지


            stage = new CustomEllipseGeometry(System.Windows.SystemColors.ControlBrush, 3);

            CustomEllipseGeometry stageEdge = stage as CustomEllipseGeometry;

            viewStageField.Set(GeneralTools.DataStageField);
            viewStageField.Transform(RatioX, RatioY);
            if (!preview)
            {
                viewStageField.ScaleOffset(ZoomScale, OffsetX, OffsetY);
            }

            if (!preview)
            {
                stageEdge.SetData(viewStageField, CenterX, CenterY, 3 * ZoomScale);
                Geometry.Add(stageEdge);
            }
            else
            {
                stageEdge.SetData(viewStageField, CenterX, CenterY, 3);
                PreviewGeometry.Add(stageEdge);
            }




            if (!preview)
            {
                listCandidatePoint.Clear();
            }
            else
            {
                listPreviewCandidatePoint.Clear();
            }

            for (int i = 0; i < dataManager.recipeDM.TeachingRD.DataCandidatePoint.Count; i++)
            {
                dataPoint = new ShapeEllipse(GeneralTools.StageHoleBrush);
                ShapeEllipse dataCandidatePoint = dataPoint as ShapeEllipse;

                CCircle circle = new CCircle(dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].x, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].y, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].width,
                    dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].height, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].MeasurementOffsetX, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].MeasurementOffsetY);
                circle.Transform(RatioX, RatioY);

                if (!preview)
                {
                    circle.ScaleOffset(ZoomScale, OffsetX, OffsetY);
                }
                Circle c = drawGeometryManager.GetRect(circle, CenterX, CenterY);
                dataCandidatePoint.SetData(c, (int)(circle.width), (int)(circle.height));
                if (!preview)
                {
                    //Geometry.Add(dataCandidatePoint);
                    Shapes.Add(dataCandidatePoint);
                    listCandidatePoint.Add(dataCandidatePoint);
                }
                else
                {
                    PreviewShapes.Add(dataCandidatePoint);
                    listPreviewCandidatePoint.Add(dataCandidatePoint);
                }
            }

            if (!preview)
            {
                if (ShowRoute)
                {
                    for (int i = 0; i < dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Count - 1; i++)
                    {
                        stage = new CustomPathGeometry(RouteBrush, RouteThick * ZoomScale);
                        CustomPathGeometry routeLine = stage as CustomPathGeometry;
                        CCircle from = dataManager.recipeDM.TeachingRD.DataSelectedPoint[dataManager.recipeDM.TeachingRD.DataMeasurementRoute[i]];
                        CCircle to = dataManager.recipeDM.TeachingRD.DataSelectedPoint[dataManager.recipeDM.TeachingRD.DataMeasurementRoute[i + 1]];

                        from.Transform(RatioX, RatioY);
                        from.ScaleOffset(ZoomScale, OffsetX, OffsetY);

                        to.Transform(RatioX, RatioY);
                        to.ScaleOffset(ZoomScale, OffsetX, OffsetY);

                        PointF[] line = { new PointF((float)from.x + CenterX, (float)-from.y + CenterY), new PointF((float)to.x + CenterX, (float)-to.y + CenterY) };
                        StartPoint = new System.Windows.Point((float)from.x + CenterX, (float)-from.y + CenterY); // 시작포인트
                        drawGeometryManager.AddLine(line);
                        routeLine.SetData(drawGeometryManager.GetPathFigure(StartPoint), RouteThick * ZoomScale);
                        Geometry.Add(routeLine);
                        drawGeometryManager.ClearSegments();
                    }
                }
            }

            if (!preview)
            {
                listSelectedPoint.Clear();
            }
            else
            {
                listPreviewSelectedPoint.Clear();
            }
            int nDummyIdx = -1;
            for (int i = 0; i < dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count; i++)
            {
                dataPoint = new ShapeEllipse(GeneralTools.GbHole);
                ShapeEllipse dataSelectedPoint = dataPoint as ShapeEllipse;
                CCircle circle = new CCircle(dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].x, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].y, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].width,
                    dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].height, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].MeasurementOffsetX, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].MeasurementOffsetY);
                circle.Transform(RatioX, RatioY);
                if (!preview)
                {
                    circle.ScaleOffset(ZoomScale, OffsetX, OffsetY);
                }

                Circle c = drawGeometryManager.GetRect(circle, CenterX, CenterY);
                dataSelectedPoint.SetData(c, (int)(circle.width), (int)(circle.height), true);
                if (SetStartEndPointMode)
                {
                    CCircle reorderCircle = dataManager.recipeDM.TeachingRD.DataSelectedPoint[i];
                    if (ContainsData(ListReorderPoint, reorderCircle, out nDummyIdx))
                    {
                        dataSelectedPoint.SetBrush(System.Windows.Media.Brushes.Cyan);
                    }
                    else
                    {
                       dataSelectedPoint.SetBrush(System.Windows.Media.Brushes.DarkBlue);
                    }
                }
                if (!preview)
                {
                    Shapes.Add(dataSelectedPoint);
                    listSelectedPoint.Add(dataSelectedPoint);
                }
                else
                {
                    PreviewShapes.Add(dataSelectedPoint);
                    listPreviewSelectedPoint.Add(dataSelectedPoint);
                }
                
                textManager = new TextManager(new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 255)));
                textManager.SetData((RouteOrder[i] + 1).ToString(), (int)c.Width, dataSelectedPoint.CanvasLeft, dataSelectedPoint.CanvasTop - c.Height);
                if (!IsShowIndex && !IsKeyboardShowIndex)
                {
                    textManager.SetVisibility(false);
                }
                TextBlocks.Add(textManager);


            }


            if (!preview)
            {
                selectRectangle = new CustomRectangleGeometry(GeneralTools.SelectPointBrush);
                CustomRectangleGeometry select = selectRectangle as CustomRectangleGeometry;

                Rect selectRect = new Rect(Math.Min(SelectStartPoint.X, SelectEndPoint.X), Math.Min(SelectStartPoint.Y, SelectEndPoint.Y),
                    Math.Abs(SelectStartPoint.X - SelectEndPoint.X), Math.Abs(SelectStartPoint.Y - SelectEndPoint.Y));

                select.SetData(selectRect);
                SelectGeometry.Add(select);
            }

            if (IsLockUI)
            {
                stage = new CustomRectangleGeometry(GeneralTools.StageShadeBrush, GeneralTools.StageShadeBrush);
                CustomRectangleGeometry lockRect = stage as CustomRectangleGeometry;
                Rect shadeRect = new Rect(0, 0, 1000, 1000);
                lockRect.SetData(shadeRect);

                if (!preview)
                {
                    Geometry.Add(lockRect);
                }
                else
                {
                    //PreviewGeometry.Add(lockRect);
                }

                System.Windows.Controls.Image myImage = new System.Windows.Controls.Image();
                myImage.Source = new BitmapImage(new Uri(BaseDefine.Dir_LockImg, UriKind.RelativeOrAbsolute));
                myImage.Width = 200;        
                Canvas.SetLeft(myImage, 750);
                Canvas.SetTop(myImage, 50);
                m_DrawElement.Add(myImage);

            }


            if (!preview)
            {
                //AddElement(Geometry);
                Geometry.CollectionChanged -= Geometry_CollectionChanged;
                Shapes.CollectionChanged -= Shapes_CollectionChanged;
                SelectGeometry.CollectionChanged -= SelectGeometry_CollectionChanged;
                TextBlocks.CollectionChanged -= TextBlocks_CollectionChanged;
            }
            else
            {
                PreviewGeometry.CollectionChanged -= PreviewGeometry_CollectionChanged;
                PreviewShapes.CollectionChanged -= PreviewShapes_CollectionChanged;
            }

        }


        #region Brush

        private SolidColorBrush normalBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(208, 221, 221, 221));
        private SolidColorBrush buttonSelectBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 134, 255, 117));

        //SolidColorBrush routeBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 128));
        private SolidColorBrush routeBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 255));

        public SolidColorBrush RouteBrush
        {
            get
            {
                return routeBrush;
            }
            set
            {
                routeBrush = value;
                RaisePropertyChanged("RouteBrush");
            }
        } 

        #endregion

        #region Getter, Setter
        public int CurrentCandidatePoint { get; set; }
        public int CurrentSelectPoint { get; set; }

        public bool Drag { get; set; }

        public bool ShowRoute { get; set; }
        //public Canvas StageMainView { get; set; }

        private Canvas StagePreView { get; set; }

        private string strPercentage;
        public string Percentage
        {
            get
            {
                return strPercentage;
            }
            set
            {
                strPercentage = value;
                RaisePropertyChanged("Percentage");
            }
        }
        public string XPosition
        {
            get
            {
                return strXPosition;
            }
            set
            {
                strXPosition = value;
                RaisePropertyChanged("XPosition");
            }
        }

        public string ListIndex { get; set; }
        public string ListX { get; set; }
        public string ListY { get; set; }
        public string ListRoute { get; set; }


        private string strPointCount;
        public string PointCount
        {
            get
            {
                return strPointCount;
            }
            set
            {
                strPointCount = value;
                RaisePropertyChanged("PointCount");
            }
        }

        DataTable pointListItem = new DataTable();
        public DataTable PointListItem
        {
            get
            {
                return pointListItem;
            }
            set
            {
                pointListItem = value;
                RaisePropertyChanged("PointListItem");
            }
        }

        public int RouteThick { get; set; }

        private string routeThickness;
        public string RouteThickness
        {
            get
            {
                return routeThickness;
            }
            set
            {
                routeThickness = value;
                RaisePropertyChanged("RouteThickness");
            }
        }

        public string YPosition
        {
            get
            {
                return strYPosition;
            }
            set
            {
                strYPosition = value;
                RaisePropertyChanged("YPosition");
            }
        }

        public string CurrentTheta
        {
            get
            {
                return strCurrentTheta;
            }
            set
            {
                strCurrentTheta = value;
                RaisePropertyChanged("CurrentTheta");
            }
        }

        public string CurrentRadius
        {
            get
            {
                return strCurrentRadius;
            }
            set
            {
                strCurrentRadius = value;
                RaisePropertyChanged("CurrentRadius");
            }
        }


        public bool MouseMove { get; set; }
        public bool LeftMouseDown { get; set; }
        public bool RightMouseDown { get; set; }
        public System.Windows.Point MousePoint { get; set; }

        public System.Windows.Point CurrentMousePoint { get; set; }
        public System.Windows.Point MoveMousePoint { get; set; }
        public System.Windows.Point SelectStartPoint { get; set; }
        public System.Windows.Point SelectEndPoint { get; set; }

        public double RatioX { get; set; }
        public double RatioY { get; set; }

        public int CenterX { get; set; }
        public int CenterY { get; set; }

        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int OffsetScale { get; set; }

        public int PreviewCenterX { get; set; }
        public int PreviewCenterY { get; set; }

        int nZoomScale;
        public int ZoomScale
        {
            get
            {
                return nZoomScale;
            }
            set
            {
                if (0 < value && value < 64)
                {
                    if (ZoomScale < value)
                    {
                        nZoomScale = value;
                        //VerticalScroll.Maximum = HorizontalScroll.Maximum = 10 * nZoomScale;
                        //VerticalScroll.Visibility = Visibility.Visible;
                        //HorizontalScroll.Visibility = Visibility.Visible;


                        if (OffsetX != 0)
                        {
                            OffsetX *= 2;
                        }
                        if (OffsetY != 0)
                        {
                            OffsetY *= 2;
                        }
                    }
                    else
                    {
                        nZoomScale = value;
                       // VerticalScroll.Maximum = HorizontalScroll.Maximum = 10 * nZoomScale;

                        if (nZoomScale == 1)
                        {

                            OffsetX = OffsetY = 0;
                        }
                        else
                        {
                            if (OffsetX != 0)
                            {
                                OffsetX /= 2;
                            }
                            if (OffsetY != 0)
                            {
                                OffsetY /= 2;
                            }
                        }
                    }
                }
            }
        }

        #endregion


        public void UpdateView(bool bMain = false)
        {
            CurrentCandidatePoint = -1;
            CurrentSelectPoint = -1;

            p_DrawElement.Clear();
            Geometry.Clear();
            Shapes.Clear();
            TextBlocks.Clear();
            SelectGeometry.Clear();
            Shapes.CollectionChanged += Shapes_CollectionChanged;
            Geometry.CollectionChanged += Geometry_CollectionChanged;
            SelectGeometry.CollectionChanged += SelectGeometry_CollectionChanged;
            TextBlocks.CollectionChanged += TextBlocks_CollectionChanged;
            SetStage(false);

            p_PreviewDrawElement.Clear();
            PreviewGeometry.Clear();
            PreviewGeometry.CollectionChanged += PreviewGeometry_CollectionChanged;
            PreviewShapes.Clear();
            PreviewShapes.CollectionChanged += PreviewShapes_CollectionChanged;
            SetStage(true);

            ViewRectGeometry.Clear();
            ViewRectGeometry.CollectionChanged += ViewRectGeometry_CollectionChanged;
            SetViewRect();
        }

        List<int> RouteOrder = new List<int>();
        public void UpdateListView()
        {
            PointListItem.Clear();
            RouteOrder.Clear();
            int nCount = 0;
            int nSelCnt = dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count;
            int[] MeasurementOrder = new int[nSelCnt];

            for (int i = 0; i < nSelCnt; i++)
            {
                MeasurementOrder[dataManager.recipeDM.TeachingRD.DataMeasurementRoute[i]] = i;
            }

            for(int i = 0; i < MeasurementOrder.Count(); i++)
            {
                RouteOrder.Add(MeasurementOrder[i]);
            }

            DataRow row;
            for (int i = 0; i < nSelCnt; i++, nCount++)
            {

                CCircle c = dataManager.recipeDM.TeachingRD.DataSelectedPoint[i];
                int nRoute = MeasurementOrder[i];
                row = PointListItem.NewRow();
                row["ListIndex"] = (nCount + 1).ToString();
                row["ListX"] = Math.Round(c.x, 3).ToString();
                row["ListY"] = Math.Round(c.y, 3).ToString();
                row["ListRoute"] = (nRoute + 1).ToString();
                PointListItem.Rows.Add(row);

            }
            PointCount = PointListItem.Rows.Count.ToString();
        }

        private SolidColorBrush reorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(208, 221, 221, 221));

        public SolidColorBrush ReorderBrush
        {
            get
            {
                return reorderBrush;
            }
            set
            {
                reorderBrush = value;
                RaisePropertyChanged("ReorderBrush");
            }
        }

        private SolidColorBrush lockBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(208, 221, 221, 221));
        public SolidColorBrush LockBrush
        {
            get
            {
                return lockBrush;
            }
            set
            {
                lockBrush = value;
                RaisePropertyChanged("LockBrush");
            }
        }
        private SolidColorBrush showIndexBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(208, 221, 221, 221));
        public SolidColorBrush ShowIndexBrush
        {
            get
            {
                return showIndexBrush;
            }
            set
            {
                showIndexBrush = value;
                RaisePropertyChanged("ShowIndexBrush");
            }
        }
        public bool IsShowIndex { get; set; } = false;
        public bool IsKeyboardShowIndex { get; set; } = false;
        public bool IsLockUI { get; set; } = false;
        public string lockState { get; set; } = "Lock UI";
        public string LockState
        {
            get
            {
                return lockState;
            }
            set
            {
                lockState = value;
                RaisePropertyChanged("LockState");
            }
        }

        struct Data
        {
            public int Current { get; private set; }
            public int Next { get; private set; }
            public double Dist { get; private set; }

            public Data(int curr, int next, double dist)
            {
                Current = curr;
                Next = next;
                Dist = dist;
            }
        }

        public void RouteOptimizaionFunc()
        {

            int nTotalPoint = dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count;
            if(nTotalPoint == 0)
            {
                return;
            }
            double[,] distance = new double[nTotalPoint, nTotalPoint];
            //double[,] distTable = new double[nTotalPoint, nTotalPoint];
            List<int> listPoint = new List<int>();
            List<int> listWentPoint = new List<int>();
            List<double[,]> listDoublePoint = new List<double[,]>();

            int nCurrentIdx = 0;
            //SortedTupleBag<Data, int> keyValues = new SortedTupleBag<Data, int>();
            List<Data> data = new List<Data>();

            double dMaxValue = 0;
            for (int i = 0; i < nTotalPoint; i++)
            {
                for(int j = 0; j < nTotalPoint; j++)
                {
                    double x = Math.Pow(dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].x - dataManager.recipeDM.TeachingRD.DataSelectedPoint[j].x,2);
                    double y = Math.Pow(dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].y - dataManager.recipeDM.TeachingRD.DataSelectedPoint[j].y,2);
                    distance[i, j] = Math.Sqrt(x + y);
                    data.Add(new Data(i, j, distance[i, j]));
                }
                listPoint.Add(0);
                listWentPoint.Add(0);
            }
            data.Sort((x1, x2) => x1.Dist.CompareTo(x2.Dist));
            foreach(var dt in data)
            {
                if(dt.Dist != 0)
                    Console.WriteLine(dt.Current +"->"+ dt.Next + ", Dist : " + dt.Dist);
            }

            int min_index = 0;
            int currentPoint = 0;
            listWentPoint[0] = 1;
            if (SetStartEndPointMode)
            {
                nCurrentIdx = ListReorderPoint.Count;
                for (int i = 0; i < nCurrentIdx; i++)
                {
                    listWentPoint[i] = 1;
                    listPoint[i] = i;
                }
                if(nCurrentIdx == 0 || nCurrentIdx == 1)
                {
                currentPoint = nCurrentIdx = 0;
                }
                else
                {
                    currentPoint = nCurrentIdx -= 1;
                }
            }
            
            
            for (int i = nCurrentIdx + 1; i < nTotalPoint; i++)
            {
                double min_dist = 1000000;
                for (int j = 0; j < nTotalPoint; j++)
                {
                    if (listWentPoint[j] == 0 && distance[currentPoint, j] < min_dist)
                    {
                        min_dist = distance[currentPoint, j];
                        min_index = j;
                    }
                }
                listPoint[i] = min_index;
                listWentPoint[min_index] = 1;
                currentPoint = min_index;
            }
            dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Clear();
            for (int i = 0; i < dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count; i++)
            {
                dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(listPoint[i]);
            }

            UpdateListView();
            UpdateView();


        }

        bool SetStartEndPointMode { get; set; } = false;

        #region Command
        public ICommand RouteOptimizaion
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI)
                    {
                        return;
                    }
                    RouteOptimizaionFunc();
                });
            }
        }
        public ICommand ReorderPoint
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI)
                    {
                        return;
                    }
                    if(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count == 0)
                    {
                        return;
                    }

                    if (ShiftKeyDown)
                    {
                        ShiftKeyDown = false;
                        ShiftBrush = normalBrush;
                    }
                    if (CtrlKeyDown)
                    {
                        CtrlKeyDown = false;
                        CtrlBrush = normalBrush;
                    }

                    if (!SetStartEndPointMode)
                    {
                        ListReorderPoint.Clear();
                        ReorderCnt = 0;
                        SetStartEndPointMode = true;
                        ReorderBrush = buttonSelectBrush;
                        PointAddMode = "Reorder Mode";
                    }
                    else
                    {
                        SetStartEndPointMode = false;
                        ReorderBrush = normalBrush;
                        PointAddMode = "Normal";
                    }
                    //RedrawStage();
                    UpdateView();
                });
            }
        }
        public ICommand ShowIndex
        {
            get
            {
                return new RelayCommand(() =>
                {  
                    if (IsShowIndex)
                    {
                        ShowIndexBrush = normalBrush;
                        IsShowIndex = false;
                    }
                    else
                    {
                        ShowIndexBrush = buttonSelectBrush;
                        IsShowIndex = true;

                    }
                    InitKeyButton();
                    UpdateView();
                });
            }
        }
        public void InitKeyButton()
        {
            IsKeyboardShowIndex = false;
            SBrush = normalBrush;
            CtrlKeyDown = false;
            CtrlBrush = normalBrush;
            ShiftKeyDown = false;
            ShiftBrush = normalBrush;
            PointAddMode = "Normal";
        }
        public ICommand UILock
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI)
                    {
                        LockBrush = normalBrush;
                        LockState = "Lock UI";
                        IsLockUI = false;
                    }
                    else
                    {
                        LockBrush = buttonSelectBrush;
                        LockState = "UnLock UI";
                        IsLockUI = true;

                        ZoomScale = 1;
                        //RedrawStage();
                    }
                    InitKeyButton();
                    UpdateView();
                });
            }
        }
        public ICommand btn13Point
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    ReadPreset("13 Point.prs");

                    UpdateListView();
                    UpdateView();
                });
            }
        }

        public ICommand btnClose
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CloseRequested(this, new DialogCloseRequestedEventArgs(false));
                });
            }
        }

        public ICommand btn25Point
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    ReadPreset("25 Point.prs");

                    UpdateListView();
                    UpdateView();
                });
            }
        }

        public ICommand btn49Point
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    ReadPreset("49 Point.prs");
                    UpdateListView();
                    UpdateView();
                });
            }
        }

        public ICommand btn73Point
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    ReadPreset("73 Point.prs");

                    UpdateListView();
                    UpdateView();
                });
            }
        }
        #endregion

        public void ReadPreset(string presetName)
        {
            dataManager.recipeDM.TeachingRD.ClearPoint();
            dataManager.recipeDM.PresetData = (PresetData)GeneralFunction.Read(dataManager.recipeDM.PresetData, BaseDefine.Dir_Preset + presetName);
            dataManager.recipeDM.TeachingRD.DataCandidatePoint = dataManager.recipeDM.PresetData.DataCandidatePoint;
            dataManager.recipeDM.TeachingRD.DataSelectedPoint = dataManager.recipeDM.PresetData.DataSelectedPoint;
            dataManager.recipeDM.TeachingRD.DataMeasurementRoute = dataManager.recipeDM.PresetData.DataMeasurementRoute;

            dataManager.recipeDM.TeachingRD.CheckCircleSize();
            CheckSelectedPoint();
        }

        public void CheckSelectedPoint()
        {
            int index;
            List<CCircle> temp = new List<CCircle>();
            foreach (var item in dataManager.recipeDM.TeachingRD.DataSelectedPoint)
            {
                temp.Add(item);
            }
            dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Clear();
            dataManager.recipeDM.TeachingRD.DataSelectedPoint.Clear();
            foreach (var item in temp)
            {
                if (ContainsData(dataManager.recipeDM.TeachingRD.DataCandidatePoint, item, out index))
                {
                    dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                    dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(item);
                }
            }
        }

        public bool ContainsData(List<CCircle> list, CCircle circle, out int nIndex)
        {
            bool bRst = false;
            nIndex = -1;

            int nCount = 0;
            foreach (var item in list)
            {
                //if (Math.Round(item.x, 3) == Math.Round(circle.x, 3) && Math.Round(item.y, 3) == Math.Round(circle.y, 3))
                if (Math.Round(item.x, 3) == Math.Round(circle.x, 3) && Math.Round(item.y, 3) == Math.Round(circle.y, 3)
                    && Math.Round(item.width, 3) == Math.Round(circle.width, 3) && Math.Round(item.height, 3) == Math.Round(circle.height, 3))
                {
                    bRst = true;
                    nIndex = nCount;
                }
                nCount++;
            }
            return bRst;
        }

        public void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

            UIElement el = (UIElement)sender;

            el.Focus();
            System.Windows.Point pt = e.GetPosition((UIElement)sender);
            MousePoint = new System.Windows.Point(pt.X, pt.Y);

            if (IsLockUI)
            {
                return;
            }
          
            if (!LeftMouseDown)
            {
                PrintMousePosition(MousePoint);

            }

            if (ColorPickerOpened)
            {
                return;
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                MouseMove = true;
                RightMouseDown = true;
                if (ZoomScale == 1)
                {
                   
                    return;
                }

                SelectStartPoint = new System.Windows.Point(pt.X, pt.Y);
                SelectEndPoint = new System.Windows.Point(pt.X, pt.Y);

                MoveMousePoint = new System.Windows.Point(pt.X, pt.Y);
                int nOffsetDiffX = (int)(MoveMousePoint.X - CurrentMousePoint.X);
                int nOffsetDiffY = (int)(-MoveMousePoint.Y + CurrentMousePoint.Y);

                OffsetX += (int)(MoveMousePoint.X - CurrentMousePoint.X);
                OffsetY -= (int)(MoveMousePoint.Y - CurrentMousePoint.Y);

                CurrentMousePoint = MoveMousePoint;


                //if (Math.Abs(OffsetX + nOffsetDiffX) < OffsetScale * (HorizontalScroll.Maximum - HorizontalScroll.Minimum) / 2)
                //{

                //    CurrentMousePoint.X = MoveMousePoint.X;
                //    hScrollBar1.Value = (hScrollBar1.Minimum + hScrollBar1.Maximum) / 2 - (int)Math.Round(nOffsetX / (double)nOffSetScale);

                //}
                //if (Math.Abs(nOffsetY + nOffsetDiffY) < nOffSetScale * (vScrollBar1.Maximum - vScrollBar1.Minimum) / 2)
                //{

                //    ptCurrentMouse.Y = ptLastMouse.Y;
                //    vScrollBar1.Value = (vScrollBar1.Minimum + vScrollBar1.Maximum) / 2 + (int)Math.Round(nOffsetY / (double)nOffSetScale);
                //}

                RedrawStage();
                // UpdateView();
                MouseMove = false;

            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                Mouse.Capture((UIElement)sender, CaptureMode.Element);
                if (Drag)
                {
                    SelectEndPoint = new System.Windows.Point(pt.X, pt.Y);

                    RedrawStage();
                }
            }
        }

        public void OnMouseMovePreView(object sender, System.Windows.Input.MouseEventArgs e)
        {

            System.Windows.Point pt = e.GetPosition((UIElement)sender);
            PrintMousePositionPreView(pt);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (ZoomScale == 1)
                {
                    return;
                }

                int nPreviewX = (int)(pt.X - CenterX);
                int nPreviewY = (int)(pt.Y - CenterY);

                OffsetX = -(int)((nPreviewX * ZoomScale));
                OffsetY = (int)((nPreviewY * ZoomScale));

                RedrawStage();
            }
        }

        public void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            int lines = e.Delta * System.Windows.Forms.SystemInformation.MouseWheelScrollLines / 120;

            System.Windows.Point pt = e.GetPosition((UIElement)sender);



            if (lines > 0 && !IsLockUI)
            {
                if (ZoomScale < 32)
                {
                    OffsetX += (CenterX - (int)pt.X);
                    OffsetY += -(CenterY - (int)pt.Y);
                }
                ZoomScale *= 2;

            }
            else if (lines < 0 && !IsLockUI)
            {
                OffsetX += (CenterX - (int)pt.X);
                OffsetY += -(CenterY - (int)pt.Y);
                ZoomScale /= 2;
                if (ZoomScale == 1)
                {
                    OffsetX = OffsetY = 0;
                }


            }
            RedrawStage();
        }

        public void OnMouseWheelPreView(object sender, MouseWheelEventArgs e)
        {
            int lines = e.Delta * System.Windows.Forms.SystemInformation.MouseWheelScrollLines / 120;

            MousePoint = e.GetPosition((UIElement)sender);

            if (lines > 0)
            {
                if (ZoomScale < 32)
                {
                    OffsetX = (int)(CenterX - MousePoint.X) * ZoomScale;
                    OffsetY = -(int)(CenterY - MousePoint.Y) * ZoomScale;
                }
                ZoomScale *= 2;

            }
            else if (lines < 0)
            {
                OffsetX = (int)(CenterX - MousePoint.X) * ZoomScale;
                OffsetY = -(int)(CenterY - MousePoint.Y) * ZoomScale;
                ZoomScale /= 2;
                if (ZoomScale == 1)
                {
                    OffsetX = OffsetY = 0;
                }
            }
            RedrawStage();

        }

        public void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point pt = e.GetPosition((UIElement)sender);

            if (IsLockUI)
            {
                return;
            }
            if (ColorPickerOpened)
            {
                return;
            }

            SelectStartPoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);
            SelectEndPoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);
            LeftMouseDown = true;
            Drag = true;
        }

        public void OnMouseLeftButtonDownPreView(object sender, MouseButtonEventArgs e)
        {
            MousePoint = e.GetPosition((UIElement)sender);

            if (ZoomScale == 1)
            {
                return;
            }

            int nPreviewX = (int)(MousePoint.X - CenterX);
            int nPreviewY = (int)(MousePoint.Y - CenterY);

            OffsetX = -(int)((nPreviewX * ZoomScale));
            OffsetY = (int)((nPreviewY * ZoomScale));


            RedrawStage();
        }

        public int ReorderCnt { get; set; } = 0;
        public int Limit { get; set; } = 30;
        public void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UIElement el = (UIElement)sender;
            Drag = false;
            System.Windows.Point pt = e.GetPosition((UIElement)sender);
            el.ReleaseMouseCapture();

            if (IsLockUI)
            {
                return;
            }
            if (ColorPickerOpened)
            {
                LeftMouseDown = false;
                ColorPickerOpened = false;
                return;
            }
            if (SetStartEndPointMode)
            {
                MethodStartEndSelect();

                SelectStartPoint = new System.Windows.Point(pt.X, pt.Y);
                SelectEndPoint = new System.Windows.Point(pt.X, pt.Y);
                UpdateListView();
                RedrawStage();
            }
            else
            {
                if (Math.Abs(SelectStartPoint.X - SelectEndPoint.X) > Limit || Math.Abs(SelectStartPoint.Y - SelectEndPoint.Y) > Limit)
                {
                    if (ShiftKeyDown == false && CtrlKeyDown == false)
                    {
                        dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Clear();
                        dataManager.recipeDM.TeachingRD.DataSelectedPoint.Clear();
                    }


                    double dStartSelectX = (SelectStartPoint.X - CenterX - OffsetX) / ZoomScale / RatioX;
                    double dStartSelectY = (-SelectStartPoint.Y + CenterY - OffsetY) / ZoomScale / RatioY;
                    double dEndSelectX = (SelectEndPoint.X - CenterX - OffsetX) / ZoomScale / RatioX;
                    double dEndSelectY = (-SelectEndPoint.Y + CenterY - OffsetY) / ZoomScale / RatioY;

                    double top = Math.Max(dStartSelectY, dEndSelectY);
                    double bottom = Math.Min(dStartSelectY, dEndSelectY);
                    double left = Math.Min(dStartSelectX, dEndSelectX);
                    double right = Math.Max(dStartSelectX, dEndSelectX);

                    foreach (CCircle circle in dataManager.recipeDM.TeachingRD.DataCandidatePoint)
                    {
                        if (circle.x > left && circle.x < right && circle.y > bottom && circle.y < top)
                        {
                            int _index = -1;
                            if (ContainsData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index) && ((ShiftKeyDown && CtrlKeyDown) ||!CtrlKeyDown))
                            {
                                DeletePointNotInvalidate(_index, 1);
                                //m_DM.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Delete - Index : " + (_index + 1).ToString()
                                //    + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());
                            }
                            if (!ShiftKeyDown)
                            {
                                if (!ContainsData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index)){
                                        dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                                        dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                                        RouteOrder.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count - 1);
                                }
                                //    m_DM.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Add - Index : " + m_DM.m_RDM.TeachingRD.DataSelectedPoint.Count.ToString()
                                //        + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());
                            }
                        }
                    }
                }
                else
                {
                    if (CurrentCandidatePoint == -1 && CurrentSelectPoint == -1)
                    {
                        MethodPointSelect(pt);
                    }
                    else
                    {
                        MethodCircleSelect();
                    }
                }

                SelectStartPoint = new System.Windows.Point(pt.X, pt.Y);
                SelectEndPoint = new System.Windows.Point(pt.X, pt.Y);
                UpdateListView();
                UpdateView();

            }

          

            LeftMouseDown = false;
        }

        private void MethodPointSelect(System.Windows.Point pt)
        {
            double dDistance = 0;
            double dMin = 9999;
            int nIndex = 0;
            int nMinIndex = -1;

            foreach (ShapeEllipse se in listCandidatePoint)
            {
                dDistance = GetDistance(se, new System.Windows.Point(pt.X, pt.Y));

                if (dDistance < dMin)
                {
                    dMin = dDistance;
                    nMinIndex = nIndex;
                }
                nIndex++;
            }

            if (nMinIndex != -1)
            {
                AddPoint(nMinIndex);
            }
        }


        private void AddPoint(int nIndex)
        {
            double dOffsetX = 0;
            double dOffsetY = 0;

            //if (double.TryParse(tbOffsetX.Text, out dOffsetX) && double.TryParse(tbOffsetY.Text, out dOffsetY))
            //{
            CCircle circle = dataManager.recipeDM.TeachingRD.DataCandidatePoint[nIndex];
            circle.MeasurementOffsetX = dOffsetX;
            circle.MeasurementOffsetY = dOffsetY;

            int _index = -1;
            if (!ContainsSelectedData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index))
            {
                if (!ShiftKeyDown)
                {
                    if (!ContainsData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index))
                    {
                        dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                        dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                        RouteOrder.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count - 1);
                    }
                }

                //m_DM.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Add - Index : " + m_DM.m_RDM.TeachingRD.DataSelectedPoint.Count.ToString()
                //                    + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());

                //InvalidateView();
            }
            else
            {
                if (!CtrlKeyDown)
                {
                    DeletePoint(_index, 1);
                }
            }
            //}
            //else
            //{
            //    MessageBox.Show("Input is Not Valid");
            //}
        }

        private double GetDistance(ShapeEllipse eg, System.Windows.Point pt)
        {
            double dResult = 0f;
            float x = 0, y = 0, maxX = -1, maxY = -1, minX = 9999, minY = 9999;


            dResult = Math.Sqrt(Math.Pow(eg.CenterX  - pt.X , 2) + Math.Pow(eg.CenterY - pt.Y,2));

            //x = (maxX + minX) * 0.5f;
            //y = (maxY + minY) * 0.5f;

            // dResult = Math.Sqrt(Math.Pow(x - (double)pt.X, 2) + Math.Pow(y - (double)pt.Y, 2));

            return Math.Round(dResult, 3);
        }


        public void DeletePointNotInvalidate(int nIndex, int nRange)
        {
            for (int i = nIndex; i < nIndex + nRange; i++)
            {
                double dPointX = dataManager.recipeDM.TeachingRD.DataSelectedPoint[nIndex].x;
                double dPointY = dataManager.recipeDM.TeachingRD.DataSelectedPoint[nIndex].y;

                dataManager.recipeDM.TeachingRD.DataSelectedPoint.RemoveAt(nIndex);
                int nDeleteIndex = dataManager.recipeDM.TeachingRD.DataMeasurementRoute.FindIndex(s => s.Equals(nIndex));
                dataManager.recipeDM.TeachingRD.DataMeasurementRoute.RemoveAt(nDeleteIndex);
                for (int j = 0; j < dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Count; j++)
                {
                    if (dataManager.recipeDM.TeachingRD.DataMeasurementRoute[j] > nIndex)
                    {
                        dataManager.recipeDM.TeachingRD.DataMeasurementRoute[j] = dataManager.recipeDM.TeachingRD.DataMeasurementRoute[j] - 1;
                    }
                }
            }
        }

        public List<CCircle> ListReorderPoint { get; set; } = new List<CCircle>();
        private void MethodStartEndSelect()
        {
            if (CurrentSelectPoint != -1)
            {

                int index = -1;
                int nReorderIdx = -1;
                CCircle circle = dataManager.recipeDM.TeachingRD.DataSelectedPoint[CurrentSelectPoint];

                if (ContainsData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out index) && !ContainsData(ListReorderPoint, circle, out nReorderIdx))
                {
                    //if (index == 0)
                    //{
                    //    return;
                    //}
                    //foreach (int i in dataManager.recipeDM.TeachingRD.DataMeasurementRoute)
                    //{
                    //    if (i == 0)
                    //    {
                    //        start = startIdx;
                    //    }
                    //    startIdx++;
                    //}
                    DeletePoint(index, 1);
                    dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                    dataManager.recipeDM.TeachingRD.DataSelectedPoint.Insert(ReorderCnt, circle);
                    ListReorderPoint.Add(circle);
                    dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Sort();

                    ReorderCnt++;

                    if (ReorderCnt == dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count)
                    {
                        SetStartEndPointMode = false;
                        ReorderCnt = 0;
                        ReorderBrush = normalBrush;
                    }
                    //RouteOptimizaionFunc();
                }


                //else
                //{
                //    foreach (int i in dataManager.recipeDM.TeachingRD.DataMeasurementRoute)
                //    {
                //        if (i == dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Count - 1)
                //        {
                //            end = endIdx;
                //            //before = dataMA
                //        }
                //        endIdx++;
                //    }
                //    int t = dataManager.recipeDM.TeachingRD.DataMeasurementRoute[CurrentSelectPoint];
                //    dataManager.recipeDM.TeachingRD.DataMeasurementRoute[CurrentSelectPoint] = dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Count - 1;
                //    dataManager.recipeDM.TeachingRD.DataMeasurementRoute[end] = t;
                //}


            }
        }

        private void MethodCircleSelect()
        {
            double dOffsetX = 0;
            double dOffsetY = 0;
            if (CurrentCandidatePoint != -1)
            {

                //if (double.TryParse(tbOffsetX.Text, out dOffsetX) && double.TryParse(tbOffsetY.Text, out dOffsetY))
                //{
                CCircle circle = dataManager.recipeDM.TeachingRD.DataCandidatePoint[CurrentCandidatePoint];
                circle.MeasurementOffsetX = dOffsetX;
                circle.MeasurementOffsetY = dOffsetY;

                int _index = -1;
                if (ContainsSelectedData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index) && ((ShiftKeyDown && CtrlKeyDown) || !CtrlKeyDown))
                {
                    DeletePoint(_index, 1);
                    //m_DM.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Delete - Index : " + (_index + 1).ToString()
                    //                + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());
                }
                else
                {
                    if (!ShiftKeyDown)
                    {
                        if (!ContainsData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index))
                        {
                            dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                            dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                            RouteOrder.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count - 1);
                        }
                    }
                    //dataManager.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Add - Index : " + m_DM.m_RDM.TeachingRD.DataSelectedPoint.Count.ToString()
                    //                + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());

                    //UpdateView();
                    //UpdateListView();
                }
            }
            if (CurrentSelectPoint != -1)
            {
                dOffsetX = 0;
                dOffsetY = 0;

                //if (double.TryParse(tbOffsetX.Text, out dOffsetX) && double.TryParse(tbOffsetY.Text, out dOffsetY))
                //{
                CCircle circle = dataManager.recipeDM.TeachingRD.DataSelectedPoint[CurrentSelectPoint];

                int _index = -1;
                if (ContainsSelectedData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index) && ((ShiftKeyDown && CtrlKeyDown) || !CtrlKeyDown))
                {
                    DeletePoint(_index, 1);
                    //dataManager.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Delete - Index : " + (_index + 1).ToString()
                    //                + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());
                }
                else
                {
                    if (!ShiftKeyDown)
                    {
                        if (!ContainsData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index))
                        {
                            dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                            dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                            RouteOrder.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count - 1);
                        }
                    }
                    //dataManager.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Add - Index : " + m_DM.m_RDM.TeachingRD.DataSelectedPoint.Count.ToString()
                    //                + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());

                    //SetListViewTab(0);
                    //InvalidateView();
                    //UpdateView();
                    //UpdateListView();
                }
            }
        }

        public void DeletePoint(int nIndex, int nRange)
        {
            for (int i = nIndex; i < nIndex + nRange; i++)
            {
                double dPointX = dataManager.recipeDM.TeachingRD.DataSelectedPoint[nIndex].x;
                double dPointY = dataManager.recipeDM.TeachingRD.DataSelectedPoint[nIndex].y;

                dataManager.recipeDM.TeachingRD.DataSelectedPoint.RemoveAt(nIndex);
                int nDeleteIndex = dataManager.recipeDM.TeachingRD.DataMeasurementRoute.FindIndex(s => s.Equals(nIndex));
                dataManager.recipeDM.TeachingRD.DataMeasurementRoute.RemoveAt(nDeleteIndex);
                for (int j = 0; j < dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Count; j++)
                {
                    if (dataManager.recipeDM.TeachingRD.DataMeasurementRoute[j] > nIndex)
                    {
                        dataManager.recipeDM.TeachingRD.DataMeasurementRoute[j] = dataManager.recipeDM.TeachingRD.DataMeasurementRoute[j] - 1;
                    }
                }

            }

            //UpdateView();
            UpdateListView();
        }

        public bool ContainsSelectedData(List<CCircle> list, CCircle circle, out int nIndex)
        {
            bool bRst = false;
            nIndex = -1;

            int nCount = 0;
            foreach (var item in list)
            {
                //if (Math.Round(item.x, 3) == Math.Round(circle.x, 3) && Math.Round(item.y, 3) == Math.Round(circle.y, 3))
                if (Math.Round(item.x, 3) == Math.Round(circle.x, 3) && Math.Round(item.y, 3) == Math.Round(circle.y, 3)
                    && Math.Round(item.width, 3) == Math.Round(circle.width, 3) && Math.Round(item.height, 3) == Math.Round(circle.height, 3)
                    && Math.Round(item.MeasurementOffsetX, 3) == Math.Round(circle.MeasurementOffsetX, 3) && Math.Round(item.MeasurementOffsetY, 3) == Math.Round(circle.MeasurementOffsetY, 3))
                {
                    bRst = true;
                    nIndex = nCount;
                }
                nCount++;
            }
            return bRst;
        }


        public void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MousePoint = e.GetPosition((UIElement)sender);
            CurrentMousePoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);
            if (ColorPickerOpened)
            {
                ColorPickerOpened = false;
                return;
            }
        }

        public void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            RightMouseDown = false;
            
        }

        public void OnMouseRightButtonDownPreView(object sender, MouseButtonEventArgs e)
        {
            MousePoint = e.GetPosition(StagePreView);
            CurrentMousePoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);
        }

        public ICommand btnSelectPercentage
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    double dPercent = -1;
                    if (double.TryParse(Percentage, out dPercent))
                    {
                        if (0 <= dPercent && dPercent <= 100)
                        {
                            dataManager.recipeDM.TeachingRD.DataSelectedPoint.Clear();
                            dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Clear();

                            int nCount = 0;
                            int nMin = 0;
                            int nMax = 0;

                            nMin = 0;
                            nMax = dataManager.recipeDM.TeachingRD.DataCandidatePoint.Count - 1;
                            nCount = (int)(dataManager.recipeDM.TeachingRD.DataCandidatePoint.Count * dPercent / 100);

                            foreach (CCircle c in GeneralFunction.GetSelectedRandom(dataManager.recipeDM.TeachingRD.DataCandidatePoint, nCount, nMin, nMax))
                            {
                                CCircle circle = c;

                                dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                                dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                            }
                            UpdateListView();
                            UpdateView();
                        }
                        else
                        {
                            Percentage = 0.ToString();
                        }

                    }
                    else
                    {
                        Percentage = 0.ToString();
                    }

                });
            }
        }

        public ICommand btnSelectAll
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    dataManager.recipeDM.TeachingRD.DataSelectedPoint.Clear();
                    dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Clear();

                    foreach (CCircle c in dataManager.recipeDM.TeachingRD.DataCandidatePoint)
                    {
                        CCircle circle = c;

                        dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                        dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                    }

                    UpdateListView();
                    UpdateView();

                });
            }
        }

        public ICommand btnDeleteAll
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    dataManager.recipeDM.TeachingRD.DataSelectedPoint.Clear();
                    dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Clear();
                    UpdateListView();
                    UpdateView();
                });
            }
        }

        public ICommand btnReset
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    dataManager.recipeDM.TeachingRD.ClearPoint();
                    //dataSelectedPoint.Clear();

                    UpdateListView();
                    UpdateView();


                });
            }
        }

        public ICommand btnPreset1
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    dataManager.recipeDM.TeachingRD.ClearCandidatePoint();
                    dataManager.recipeDM.TeachingRD.DataCandidatePoint = (List<CCircle>)GeneralFunction.Read(dataManager.recipeDM.TeachingRD.DataCandidatePoint, BaseDefine.Dir_StageMap + "Preset 1.smp");
                    CheckSelectedPoint();
                    UpdateListView();

                    UpdateView();

                });
            }
        }

        public ICommand btnPreset2
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (IsLockUI || SetStartEndPointMode)
                    {
                        return;
                    }
                    dataManager.recipeDM.TeachingRD.ClearCandidatePoint();
                    dataManager.recipeDM.TeachingRD.DataCandidatePoint = (List<CCircle>)GeneralFunction.Read(dataManager.recipeDM.TeachingRD.DataCandidatePoint, BaseDefine.Dir_StageMap + "Preset 2.smp");
                    CheckSelectedPoint();

                    UpdateListView();
                    UpdateView();

                });
            }
        }



        public ICommand btnCustomize
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }

        #endregion

        private void PrintMousePosition(System.Windows.Point pt)
        {
            double mouseX = ((((int)pt.X - CenterX) - OffsetX) / (double)ZoomScale) / RatioX;
            double mouseY = ((((int)-pt.Y + CenterY) - OffsetY) / (double)ZoomScale) / RatioY;

            XPosition = Math.Round(mouseX, 3).ToString("0.###") + " mm";
            YPosition = Math.Round(mouseY, 3).ToString("0.###") + " mm";
            CurrentTheta = Math.Round((Math.Atan2(mouseY, mouseX) * 180 / Math.PI), 3).ToString("0.###") + " °";
            CurrentRadius = Math.Round((Math.Sqrt(Math.Pow(mouseX, 2) + Math.Pow(mouseY, 2))), 3).ToString("0.###") + " mm";

        }

        private void PrintMousePositionPreView(System.Windows.Point pt)
        {
            double mouseX = ((int)pt.X - CenterX) / RatioX;
            double mouseY = ((int)-pt.Y + CenterY) / RatioY;



            XPosition = Math.Round(mouseX, 3).ToString("0.###") + " mm";
            YPosition = Math.Round(mouseY, 3).ToString("0.###") + " mm";
            CurrentTheta = Math.Round((Math.Atan2(mouseY, mouseX) * 180 / Math.PI), 3).ToString("0.###") + " °";
            CurrentRadius = Math.Round((Math.Sqrt(Math.Pow(mouseX, 2) + Math.Pow(mouseY, 2))), 3).ToString("0.###") + " mm";

        }

        public void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {

            UIElement el = sender as UIElement;
            el.Focus();
            StageMouseHover = true;
            StageMouseHoverUpdate = false;
        }

        public void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {   
            StageMouseHover = false;
            StageMouseHoverUpdate = true;
        }

        public bool StageMouseHover { get; set; }
        public bool StageMouseHoverUpdate { get; set; }

        public void OnCheckboxChange(object sender, RoutedEventArgs e)
        {
            UpdateView();
        }

        public void OnThicknessChanged(object sender, TextChangedEventArgs e)
        {
            int thick = 0;
            RouteThick = 3;
            if (int.TryParse(RouteThickness, out thick))
            {
                RouteThick = thick;
                if (0 < thick && thick <= 10)
                {
                    RouteThick = thick;
                }
                else if (thick > 10)
                {
                    RouteThick = 10;
                    RouteThickness = "10";
                }
                else if (thick < 0)
                {
                    RouteThick = 1;
                    RouteThickness = "1";
                }
            }
            else
            {
                RouteThickness = "3";
            }
            UpdateView();
        }

        public void OnNew_Click(object sender, RoutedEventArgs e)
        {
            dataManager.recipeDM.RecipeNew();
        }
        public void OnOpen_Click(object sender, RoutedEventArgs e)
        {
            dataManager.recipeDM.RecipeOpen();
        }
        public void OnSave_Click(object sender, RoutedEventArgs e)
        {
            dataManager.recipeDM.RecipeSave();
        }
        public void OnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            dataManager.recipeDM.RecipeSaveAs();
        }



        public void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            ColorPicker co = sender as ColorPicker;
            RouteBrush.Color = (System.Windows.Media.Color)co.SelectedColor;
            RedrawStage();
            //UpdateView();
        }

        public bool ColorPickerOpened { get; set; }
        public void colorPicker_Opened(object sender, RoutedEventArgs e)
        {
            ColorPickerOpened = true;
            if (IsKeyboardShowIndex)
            {
                IsKeyboardShowIndex = false;
                SBrush = normalBrush;
                UpdateView();
            }
            if (ShiftKeyDown)
            {
                ShiftKeyDown = false;
                ShiftBrush = normalBrush;
            }
            if (CtrlKeyDown)
            {
                CtrlKeyDown = false;
                CtrlBrush = normalBrush;
            }
            PointAddMode = "Normal";
        }

        public bool ShiftKeyDown { get; set; } = false;
        public bool CtrlKeyDown { get; set; } = false;


        SolidColorBrush shiftBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 221, 221, 221));
        SolidColorBrush ctrlBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 221, 221, 221));
        SolidColorBrush sBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 221, 221, 221));
        public SolidColorBrush ShiftBrush
        {
            get
            {
                return shiftBrush;
            }
            set
            {
                shiftBrush = value;
                RaisePropertyChanged("ShiftBrush");
            }
        }
        public SolidColorBrush CtrlBrush
        {
            get
            {
                return ctrlBrush;
            }
            set
            {
                ctrlBrush = value;
                RaisePropertyChanged("CtrlBrush");
            }
        }
        public SolidColorBrush SBrush
        {
            get
            {
                return sBrush;
            }
            set
            {
                sBrush = value;
                RaisePropertyChanged("SBrush");
            }
        }
        public void OnCanvasKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.S))
            {
                if (IsShowIndex)
                {
                    SBrush = buttonSelectBrush;
                    IsKeyboardShowIndex = false;
                    PointAddMode = "Already Displaying";
                }
                else if (!IsKeyboardShowIndex)
                {
                    SBrush = buttonSelectBrush;
                    IsKeyboardShowIndex = true;
                    PointAddMode = "Show Index";
                    RedrawStage();
                }
            }
            if (SetStartEndPointMode)
            {
                return;
            }
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                ShiftKeyDown = true;
                ShiftBrush = buttonSelectBrush;
                PointAddMode = "Delete Select Mode";
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                CtrlKeyDown = true;
                CtrlBrush = buttonSelectBrush;
                PointAddMode = "Add Select Mode";
            }
            if(ShiftKeyDown && CtrlKeyDown)
            {
                PointAddMode = "Delete Select Mode";
            }
        }
        public void OnCanvasKeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyUp(Key.S))
            {
                SBrush = normalBrush;
                if (IsShowIndex)
                {
                    PointAddMode = "Normal";
                }
                else if (IsKeyboardShowIndex)
                {
                    IsKeyboardShowIndex = false;
                    //UpdateView();
                    RedrawStage();
                    PointAddMode = "Normal";
                }
            }
            if (SetStartEndPointMode)
            {
                if (ShiftKeyDown)
                {
                    ShiftKeyDown = false;
                    ShiftBrush = normalBrush;
                }
                if (CtrlKeyDown)
                {
                    CtrlKeyDown = false;
                    CtrlBrush = normalBrush;
                }
                return;
            }
            if (Keyboard.IsKeyUp(Key.LeftShift))
            {
                ShiftKeyDown = false;
                ShiftBrush = normalBrush;
                if (CtrlKeyDown)
                {
                    PointAddMode = "Add Select Mode";
                }
                else
                {
                    PointAddMode = "Normal";
                }
            }
            if (Keyboard.IsKeyUp(Key.LeftCtrl))
            {
                CtrlKeyDown = false;
                CtrlBrush = normalBrush;
                if (ShiftKeyDown)
                {
                    PointAddMode = "Delete Select Mode";
                }
                else
                {
                    PointAddMode = "Normal";
                }
            }
        }

        private string pointAddMode = "Normal";


        public string PointAddMode {
            get
            {
                return pointAddMode;
            }
            set
            {
                pointAddMode = value;
                RaisePropertyChanged("PointAddMode");
            }
        }

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
    }
}
