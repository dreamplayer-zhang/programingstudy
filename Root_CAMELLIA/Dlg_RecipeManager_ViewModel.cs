using Root_CAMELLIA.Data;
using Root_CAMELLIA.ShapeDraw;
using RootTools;
using RootTools.Inspects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Root_CAMELLIA
{
    class Dlg_RecipeManager_ViewModel : ObservableObject
    {


        private System.Windows.Controls.Primitives.ScrollBar VerticalScroll;
        private System.Windows.Controls.Primitives.ScrollBar HorizontalScroll;

        private string strXPosition;
        private string strYPosition;
        private string strCurrentTheta;
        private string strCurrentRadius;

        public Dlg_RecipeManager_ViewModel(MainWindow mainWindow, Dlg_RecipeManger rcpMgr)
        {
            StageMainView = rcpMgr.StageMainView;
            StagePreView = rcpMgr.StagePreView;
            int a;
            VerticalScroll = rcpMgr.VerticalScroll;
            HorizontalScroll = rcpMgr.HorizontalScroll;
            dataManager = mainWindow.DataManager;
            Init();
            InitStage();
            SetStage(StageMainView);
            DrawStage(StageMainView);
            SetStage(StagePreView);
            //DrawStage(StagePreView);
        }

        public DataManager dataManager { get; set; }
        public void SetDataManager(DataManager DM)
        {
            dataManager = DM;
        }

        #region Init
        public void Init()
        {
            pointListItem = new DataTable();
            PointListItem.Columns.Add(new DataColumn("ListIndex"));
            PointListItem.Columns.Add(new DataColumn("ListX"));
            PointListItem.Columns.Add(new DataColumn("ListY"));
            PointListItem.Columns.Add(new DataColumn("ListRoute"));


            XPosition = "0.000 mm";
            YPosition = "0.000 mm";
            CurrentTheta = "0 °";
            CurrentRadius = "0.000 mm";
            PointCount = "0";
            Percentage = "0";
            ZoomScale = 1;
            VerticalScroll.Value = VerticalScroll.Minimum;
            HorizontalScroll.Value = HorizontalScroll.Minimum;
            CenterX = (int)(StageMainView.Width * 0.5f);
            CenterY = (int)(StageMainView.Height * 0.5f);
            RatioX = StageMainView.Width / BaseDefine.ViewSize;
            RatioY = StageMainView.Height / BaseDefine.ViewSize;
            OffsetScale = 100;

            PreviewCenterX = (int)(StagePreView.Width * 0.5f);
            PreviewCenterY = (int)(StagePreView.Width * 0.5f);

            dataViewPosition.X = 0;
            dataViewPosition.Y = 0;
            dataViewPosition.Width = StagePreView.Width;
            dataViewPosition.Height = StagePreView.Height;
            CurrentCandidatePoint = -1;
            CurrentSelectPoint = -1;
            
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.SystemIdle);    //객체생성

            timer.Interval = TimeSpan.FromMilliseconds(50);    //시간간격 설정

            timer.Tick += new EventHandler(MouseHover);          //이벤트 추가

            timer.Start();

        }
        #endregion


        public int CurrentCandidatePoint { get; set; }
        public int CurrentSelectPoint { get; set; }


        private void MouseHover(object sender, EventArgs e)
        {
            if (StageMouseHover)
            {
                System.Windows.Point pt = MousePoint;
                IsCircleHover(pt);
            }
        }

        private void IsCircleHover(System.Windows.Point e)
        {
            bool bSelected = false;

            foreach (EllipseGeometry eg in listCandidatePoint)
            {
                if (IsInCircle(eg, new System.Windows.Point(e.X, e.Y)))
                {
                    currentEllipseGeometry = eg;
                    bSelected = true;
                }
            }

            if (!bSelected)
            {
                CurrentCandidatePoint = -1;
                currentEllipseGeometry = new EllipseGeometry();
            }
            else
            {
                CurrentCandidatePoint = listCandidatePoint.IndexOf(currentEllipseGeometry);
            }

            bSelected = false;

            foreach (EllipseGeometry eg in listSelectedPoint)
            {
                if (IsInCircle(eg, new System.Windows.Point(e.X, e.Y)))
                {
                    selectEllipseGeometry = eg;
                    bSelected = true;
                }
            }

            if (!bSelected)
            {
                CurrentSelectPoint = -1;
                selectEllipseGeometry = new EllipseGeometry();
            }
            else
            {
                CurrentCandidatePoint = -1;
                currentEllipseGeometry = new EllipseGeometry();
                CurrentSelectPoint = listSelectedPoint.IndexOf(selectEllipseGeometry);
            }

            SetStage(StageMainView);
            DrawStage(StageMainView);
            //SetStage(StagePreView);
            //DrawStage(StagePreView);

        }

        private bool IsInCircle(EllipseGeometry eg, System.Windows.Point pt)
        {
            double dist = Math.Pow(eg.Center.X - pt.X, 2) + Math.Pow(eg.Center.Y - pt.Y, 2);
            bool bRst;
            if (dist <= Math.Pow(eg.RadiusX, 2))
            {
                bRst = true;
            }
            else
            {
                bRst = false;
            }

            return bRst;
        }


        #region Stage

        #region DataStage     
        public Circle dataStageField = new Circle();
        public ShapeDraw.Line dataStageLineHole = new ShapeDraw.Line();
        public PointF[] dataStageEdgeHolePoint = new PointF[64];
        public Arc[] dataStageEdgeHoleArc = new Arc[8];
        public Circle[] dataStageGuideLine = new Circle[4];
        public Arc[] dataStageDoubleHoleArc = new Arc[8];

        public Arc[] dataStageTopHoleArc = new Arc[2];
        public Arc[] dataStageBotHoleArc = new Arc[2];
        public Rect dataViewPosition = new Rect();
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

        #region Geometry
        private DrawGeometryManager drawGeometryManager = new DrawGeometryManager();
        private EllipseGeometry stageGeometry = new EllipseGeometry();
        private EllipseGeometry[] guideEllipseGeometry = new EllipseGeometry[4];
        private EllipseGeometry[] circleEllipseGeometry;
        private RectangleGeometry rectangleGeometry = new RectangleGeometry();
        private EllipseGeometry currentEllipseGeometry = new EllipseGeometry();
        private EllipseGeometry selectEllipseGeometry = new EllipseGeometry();

        private PathGeometry[] edgeArcPathGeometry = new PathGeometry[4];
        private PathGeometry[] doubleHoleArcPathGeometry = new PathGeometry[4];

        private PathGeometry[] topBotDoubleHoleArcPathGeometry = new PathGeometry[2];

        private PathGeometry[] routePathGeometry = new PathGeometry[BaseDefine.MaxPoint/2];

        private RectangleGeometry[] previewRectangleGeometry = new RectangleGeometry[4];

        private RectangleGeometry selectRectangleGeometry = new RectangleGeometry();

        private List<EllipseGeometry> listCandidatePoint = new List<EllipseGeometry>();
        private List<EllipseGeometry> listPreviewCandidatePoint = new List<EllipseGeometry>();
        private List<EllipseGeometry> listSelectedPoint = new List<EllipseGeometry>();
        private List<EllipseGeometry> listPreviewSelectedPoint = new List<EllipseGeometry>();
        private List<PathGeometry> listRouteLine = new List<PathGeometry>();

        EllipseGeometry[] ellipses = new EllipseGeometry[BaseDefine.MaxPoint];
        EllipseGeometry[] selectEllipses = new EllipseGeometry[BaseDefine.MaxPoint];
        EllipseGeometry[] previewEllipses = new EllipseGeometry[BaseDefine.MaxPoint];

        System.Windows.Shapes.Path stagePath = new System.Windows.Shapes.Path();
        System.Windows.Shapes.Path[] guideLinePath = new System.Windows.Shapes.Path[4];
        System.Windows.Shapes.Path rectanglePath = new System.Windows.Shapes.Path();

        System.Windows.Shapes.Path[] edgePath = new System.Windows.Shapes.Path[4];
        System.Windows.Shapes.Path[] doubleHolePath = new System.Windows.Shapes.Path[4];

        System.Windows.Shapes.Path[] topBotDoubleHolePath = new System.Windows.Shapes.Path[2];
        System.Windows.Shapes.Path[] circlePath;
        System.Windows.Shapes.Path stageEdgePath = new System.Windows.Shapes.Path();
        System.Windows.Shapes.Path[] candidatePath = new System.Windows.Shapes.Path[BaseDefine.MaxPoint];
        System.Windows.Shapes.Path selectRectPath = new System.Windows.Shapes.Path();
        #endregion

        #region Getter Setter
        public int ArcPointNum { get; set; }
        public int ArcPointNum2 { get; set; }
        public int EdgeNum { get; set; }
        public int DoubleHoleNum { get; set; }
        public int GuideLineNum { get; set; }

        #endregion

        private void InitStage()
        {
            ArcPointNum = 63;
            EdgeNum = 4;
            DoubleHoleNum = 4;
            GuideLineNum = 4;

            for(int i = 0; i < ellipses.Length; i++)
            {
                ellipses[i] = new EllipseGeometry();
                previewEllipses[i] = new EllipseGeometry();
                selectEllipses[i] = new EllipseGeometry();
                EllipsePath[i] = new System.Windows.Shapes.Path();
                PreviewEllipsePath[i] = new System.Windows.Shapes.Path();
                SelectEllipsePath[i] = new System.Windows.Shapes.Path();
               // candidatePath = new System.Windows.Shapes.Path();
            }

            for(int i = 0; i < BaseDefine.MaxPoint/2; i++)
            {
                routePathGeometry[i] = new PathGeometry();
                RouteLinePath[i] = new System.Windows.Shapes.Path();
            }

            for(int i = 0; i < GuideLineNum; i++)
            {
                guideLinePath[i] = new System.Windows.Shapes.Path();
                edgeArcPathGeometry[i] = new PathGeometry();
            }
            for(int i = 0; i < EdgeNum; i++)
            {
                edgePath[i] = new System.Windows.Shapes.Path();
            }
            for(int i = 0; i < DoubleHoleNum; i++)
            {
                doubleHolePath[i] = new System.Windows.Shapes.Path();
                doubleHoleArcPathGeometry[i] = new PathGeometry();
            }
            for(int i = 0; i < 2; i++)
            {
                topBotDoubleHolePath[i] = new System.Windows.Shapes.Path();
                topBotDoubleHoleArcPathGeometry[i] = new PathGeometry();
            }
          
            dataStageField.Set(0, 0, BaseDefine.ViewSize, BaseDefine.ViewSize);
            dataStageLineHole.Set(0, 0, BaseDefine.ViewSize, 7);

            OpenStageCircleHole();
            circleEllipseGeometry = new EllipseGeometry[dataStageCircleHole.Count()];
            circlePath = new System.Windows.Shapes.Path[dataStageCircleHole.Count()];
            for(int i = 0; i < dataStageCircleHole.Count(); i++)
            {
                circlePath[i] = new System.Windows.Shapes.Path();
            }

            for(int i = 0; i < GuideLineNum; i++)
            {
            guideEllipseGeometry[i] = new EllipseGeometry();
            }

            double dRadiusIn = 130;
            double dRadiusOut = 155;
            dataStageEdgeHolePoint[0] = new PointF((float)15, (float)Math.Sqrt(16675));
            dataStageEdgeHolePoint[1] = new PointF((float)Math.Sqrt(16347.75), (float)23.5);
            dataStageEdgeHolePoint[2] = new PointF((float)Math.Sqrt(23472.75), (float)23.5);
            dataStageEdgeHolePoint[3] = new PointF((float)15, (float)Math.Sqrt(23800));
            dataStageEdgeHolePoint[4] = new PointF((float)Math.Sqrt(16347.75), (float)-23.5);
            dataStageEdgeHolePoint[5] = new PointF((float)15, (float)-Math.Sqrt(16675));
            dataStageEdgeHolePoint[6] = new PointF((float)15, (float)-Math.Sqrt(23800));
            dataStageEdgeHolePoint[7] = new PointF((float)Math.Sqrt(23472.75), (float)-23.5);
            dataStageEdgeHolePoint[8] = new PointF((float)-15, (float)-Math.Sqrt(16675));
            dataStageEdgeHolePoint[9] = new PointF((float)-Math.Sqrt(16347.75), (float)-23.5);
            dataStageEdgeHolePoint[10] = new PointF((float)-Math.Sqrt(23472.75), (float)-23.5);
            dataStageEdgeHolePoint[11] = new PointF((float)-15, (float)-Math.Sqrt(23800));
            dataStageEdgeHolePoint[12] = new PointF((float)-Math.Sqrt(16347.75), (float)23.5);
            dataStageEdgeHolePoint[13] = new PointF((float)-15, (float)Math.Sqrt(16675));
            dataStageEdgeHolePoint[14] = new PointF((float)-15, (float)Math.Sqrt(23800));
            dataStageEdgeHolePoint[15] = new PointF((float)-Math.Sqrt(23472.75), (float)23.5);
            
            for (int i = 0; i < EdgeNum; i++)
            {
                dataStageEdgeHoleArc[2 * i + 0] = new Arc(0, 0, dRadiusIn, Math.Atan2(dataStageEdgeHolePoint[4 * i + 0].Y, dataStageEdgeHolePoint[4 * i + 0].X), Math.Atan2(dataStageEdgeHolePoint[4 * i + 1].Y, dataStageEdgeHolePoint[4 * i + 1].X), ArcPointNum, false);
                dataStageEdgeHoleArc[2 * i + 1] = new Arc(0, 0, dRadiusOut, Math.Atan2(dataStageEdgeHolePoint[4 * i + 2].Y, dataStageEdgeHolePoint[4 * i + 2].X), Math.Atan2(dataStageEdgeHolePoint[4 * i + 3].Y, dataStageEdgeHolePoint[4 * i + 3].X), ArcPointNum, false);
            }

            double dRadiusHole = 6;
            double dInLength = 69.3;
            double dOutLength = 77.85;
            for (int i = 0; i < 2 * DoubleHoleNum; i++)
            {
                dataStageDoubleHoleArc[i] = new Arc();
            }
            dataStageDoubleHoleArc[0].InitArc(dInLength, dInLength, dRadiusHole, (3 / (float)4) * Math.PI, (7 / (float)4) * Math.PI, ArcPointNum, true);
            dataStageDoubleHoleArc[1].InitArc(dOutLength, dOutLength, dRadiusHole, (7 / (float)4) * Math.PI, (3 / (float)4) * Math.PI, ArcPointNum, true);
            dataStageDoubleHoleArc[2].InitArc(dInLength, -dInLength, dRadiusHole, (1 / (float)4) * Math.PI, (5 / (float)4) * Math.PI, ArcPointNum, true);
            dataStageDoubleHoleArc[3].InitArc(dOutLength, -dOutLength, dRadiusHole, (5 / (float)4) * Math.PI, (1 / (float)4) * Math.PI, ArcPointNum, true);
            dataStageDoubleHoleArc[4].InitArc(-dInLength, -dInLength, dRadiusHole, (7 / (float)4) * Math.PI, (3 / (float)4) * Math.PI, ArcPointNum, true);
            dataStageDoubleHoleArc[5].InitArc(-dOutLength, -dOutLength, dRadiusHole, (3 / (float)4) * Math.PI, (7 / (float)4) * Math.PI, ArcPointNum, true);
            dataStageDoubleHoleArc[6].InitArc(-dInLength, dInLength, dRadiusHole, (5 / (float)4) * Math.PI, (1 / (float)4) * Math.PI, ArcPointNum, true);
            dataStageDoubleHoleArc[7].InitArc(-dOutLength, dOutLength, dRadiusHole, (1 / (float)4) * Math.PI, (5 / (float)4) * Math.PI, ArcPointNum, true);

            dataStageTopHoleArc[0] = new Arc(0, 145, dRadiusHole, Math.PI, 0, ArcPointNum, true);
            dataStageTopHoleArc[1] = new Arc(0, 0, dRadiusOut, Math.Atan2(Math.Sqrt(23989), 6), Math.Atan2(Math.Sqrt(23989), -6), ArcPointNum, true);
            dataStageBotHoleArc[0] = new Arc(0, -145, dRadiusHole, 0, Math.PI, ArcPointNum, true);
            dataStageBotHoleArc[1] = new Arc(0, 0, dRadiusOut, Math.Atan2(-Math.Sqrt(23989), -6), Math.Atan2(-Math.Sqrt(23989), 6), ArcPointNum, true);


            for (int i = 0; i < GuideLineNum; i++)
            {
                dataStageGuideLine[i] = new Circle();
            }
            dataStageGuideLine[0].Set(0, 0, 49, 49);
            dataStageGuideLine[1].Set(0, 0, 98, 98);
            dataStageGuideLine[2].Set(0, 0, 150, 150);
            dataStageGuideLine[3].Set(0, 0, 196, 196);
        }

        public List<Circle> dataStageCircleHole = new List<Circle>();
        Circle viewStageCircleHole = new Circle();
        public void OpenStageCircleHole()
        {
            if (dataStageCircleHole.Count != 0)
            {
                dataStageCircleHole.Clear();
            }

            string fileName = @"C:\Camellia\StageCircleHole.txt";
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

        private void SetStage(Canvas canvas)
        {
            // 스테이지
            viewStageField.Set(dataStageField);
            viewStageField.Transform(RatioX, RatioY);
            if (canvas.Equals(StageMainView))
            {
                viewStageField.ScaleOffset(ZoomScale, OffsetX, OffsetY);
            }
            stageGeometry = drawGeometryManager.AddCircle(viewStageField, CenterX, CenterY);
            
            // Stage 중간 흰색 라인
            viewStageLineHole.Set(dataStageLineHole);
            viewStageLineHole.Transform(RatioX, RatioY);
            if (canvas.Equals(StageMainView))
            {
                viewStageLineHole.ScaleOffset(ZoomScale, OffsetX, OffsetY);
            }
            rectangleGeometry = drawGeometryManager.AddRect(GetRect(viewStageLineHole), CenterX, CenterY);


            // Stage 점선 가이드라인
            for (int i = 0; i < GuideLineNum; i++)
            {
                ViewStageGuideLine[i] = new Circle();
                ViewStageGuideLine[i].Set(dataStageGuideLine[i]);
                ViewStageGuideLine[i].Transform(RatioX, RatioY);
                if (canvas.Equals(StageMainView))
                {
                    ViewStageGuideLine[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
                }          
                guideEllipseGeometry[i] = drawGeometryManager.AddCircle(ViewStageGuideLine[i], CenterX, CenterY);
            }

            // 엣지부분 흰색 영역
            for (int i = 0; i < 2 * EdgeNum; i++)
            {
                viewStageEdgeHoleArc[i] = new Arc();
                viewStageEdgeHoleArc[i].Set(dataStageEdgeHoleArc[i]);
                viewStageEdgeHoleArc[i].Transform(RatioX, RatioY);
                if (canvas.Equals(StageMainView))
                {
                    viewStageEdgeHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
                }
            }

            PointF[] points;
            PointF[] pt = new PointF[2];
            System.Windows.Point StartPoint;
            for (int n = 0; n < EdgeNum; n++)
            {
                //edgeArcPathGeometry[n] = new PathGeometry();
                edgeArcPathGeometry[n].Clear();
                points = GetPoints(viewStageEdgeHoleArc[2 * n + 0].Points);
                StartPoint = new System.Windows.Point(points[0].X, points[0].Y); // 시작포인트
                drawGeometryManager.AddPath(points);

                pt[0] = GetPoint(viewStageEdgeHoleArc[2 * n + 0].Points[viewStageEdgeHoleArc[2 * n + 0].Points.Count() - 1]);
                pt[1] = GetPoint(viewStageEdgeHoleArc[2 * n + 1].Points[0]);
                drawGeometryManager.AddLine(pt);

                points = GetPoints(viewStageEdgeHoleArc[2 * n + 1].Points);
                drawGeometryManager.AddPath(points);

                pt[0] = GetPoint(viewStageEdgeHoleArc[2 * n + 1].Points[viewStageEdgeHoleArc[2 * n + 1].Points.Count() - 1]);
                pt[1] = GetPoint(viewStageEdgeHoleArc[2 * n + 0].Points[0]);
                drawGeometryManager.AddLine(pt);

                edgeArcPathGeometry[n].Figures.Add(drawGeometryManager.GetPathFigure(StartPoint));
                drawGeometryManager.ClearSegments();
            }


            // 긴 타원형 홀
            for (int i = 0; i < 2 * DoubleHoleNum; i++)
            {
                viewStageDoubleHoleArc[i] = new Arc();
                viewStageDoubleHoleArc[i].Set(dataStageDoubleHoleArc[i]);
                viewStageDoubleHoleArc[i].Transform(RatioX, RatioY);
                if (canvas.Equals(StageMainView))
                {
                    viewStageDoubleHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
                }
            }

            for (int i = 0; i < DoubleHoleNum; i++)
            {
                // doubleHoleArcPathGeometry[i] = new PathGeometry();
                doubleHoleArcPathGeometry[i].Clear();
                points = GetPoints(viewStageDoubleHoleArc[2 * i + 0].Points);
                drawGeometryManager.AddPath(points);
                StartPoint = new System.Windows.Point(points[0].X, points[0].Y); // 시작포인트

                pt[0] = GetPoint(viewStageDoubleHoleArc[2 * i + 0].Points[viewStageDoubleHoleArc[2 * i + 0].Points.Count() - 1]);
                pt[1] = GetPoint(viewStageDoubleHoleArc[2 * i + 1].Points[0]);
                drawGeometryManager.AddLine(pt);

                points = GetPoints(viewStageDoubleHoleArc[2 * i + 1].Points);
                drawGeometryManager.AddPath(points);

                pt[0] = GetPoint(viewStageDoubleHoleArc[2 * i + 1].Points[viewStageDoubleHoleArc[2 * i + 1].Points.Count() - 1]);
                pt[1] = GetPoint(viewStageDoubleHoleArc[2 * i + 0].Points[0]);
                drawGeometryManager.AddLine(pt);

                doubleHoleArcPathGeometry[i].Figures.Add(drawGeometryManager.GetPathFigure(StartPoint));
                drawGeometryManager.ClearSegments();
            }

            // 윗부분 및 아랫부분 타원홀
            for (int i = 0; i < 2; i++)
            {
                viewStageTopHoleArc[i] = new Arc();
                viewStageTopHoleArc[i].Set(dataStageTopHoleArc[i]);
                viewStageTopHoleArc[i].Transform(RatioX, RatioY);
                if (canvas.Equals(StageMainView))
                {
                    viewStageTopHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
                }
                viewStageBotHoleArc[i] = new Arc();
                viewStageBotHoleArc[i].Set(dataStageBotHoleArc[i]);
                viewStageBotHoleArc[i].Transform(RatioX, RatioY);
                if (canvas.Equals(StageMainView))
                {
                    viewStageBotHoleArc[i].ScaleOffset(ZoomScale, OffsetX, OffsetY);
                }
            }

            Arc[] arc;
            for (int i = 0; i < 2; i++)
            {
                //topBotDoubleHoleArcPathGeometry[i] = new PathGeometry();
                topBotDoubleHoleArcPathGeometry[i].Clear();
                if (i == 0)
                {
                    arc = viewStageTopHoleArc;
                }
                else
                {
                    arc = viewStageBotHoleArc;
                }

                points = GetPoints(arc[0].Points);
                drawGeometryManager.AddPath(points);
                StartPoint = new System.Windows.Point(points[0].X, points[0].Y); // 시작포인트

                pt[0] = GetPoint(arc[0].Points[arc[0].Points.Count() - 1]);
                pt[1] = GetPoint(arc[1].Points[0]);
                drawGeometryManager.AddLine(pt);

                points = GetPoints(arc[1].Points);
                drawGeometryManager.AddPath(points);

                pt[0] = GetPoint(arc[1].Points[arc[1].Points.Count() - 1]);
                pt[1] = GetPoint(arc[0].Points[0]);
                drawGeometryManager.AddLine(pt);

                topBotDoubleHoleArcPathGeometry[0].Figures.Add(drawGeometryManager.GetPathFigure(StartPoint));
                drawGeometryManager.ClearSegments();
            }


            // 스테이지 홀
            int idx = 0;
            foreach (Circle circle in dataStageCircleHole)
            {
                viewStageCircleHole.Set(circle);
                viewStageCircleHole.Transform(RatioX, RatioY);
                if (canvas.Equals(StageMainView))
                {
                    viewStageCircleHole.ScaleOffset(ZoomScale, OffsetX, OffsetY);
                }
                GetRect(ref viewStageCircleHole);
                circleEllipseGeometry[idx] = new EllipseGeometry();
                circleEllipseGeometry[idx] = drawGeometryManager.AddCircle(viewStageCircleHole,
                    (int)(viewStageCircleHole.Width / 2),
                    (int)(viewStageCircleHole.Y + (viewStageCircleHole.Height / 2) + viewStageCircleHole.Y));

                idx++;
            }



                if (canvas.Equals(StageMainView))
                {
                    listCandidatePoint.Clear();
                   // g.Children.Clear();
                }
                else
                {
                    listPreviewCandidatePoint.Clear();
                   // g2.Children.Clear();
                }

                for (int i = 0; i < dataManager.recipeDM.TeachingRD.DataCandidatePoint.Count; i++)
                {
                    CCircle circle = new CCircle(dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].x, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].y, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].width,
                        dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].height, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].MeasurementOffsetX, dataManager.recipeDM.TeachingRD.DataCandidatePoint[i].MeasurementOffsetY);
                    // circle = dataManager.recipeDM.TeachingRD.DataCandidatePoint[i];
                    circle.Transform(RatioX, RatioY);

                    if (canvas.Equals(StageMainView))
                    {
                        circle.ScaleOffset(ZoomScale, OffsetX, OffsetY);
                    }

                    //EllipseGeometry ellipses = new EllipseGeometry();
                    Circle c = GetRect(circle);
                    drawGeometryManager.AddCircle(ellipses[i],c, (int)(circle.width / 2), (int)(circle.height / 2));
                    // mgpCircle.AddEllipse(GetRect(c));
                    if (canvas.Equals(StageMainView))
                    {

                        listCandidatePoint.Add(ellipses[i]);
                    }
                    else
                    {
                        listPreviewCandidatePoint.Add(ellipses[i]);
                    }
                }
               // i = 0;
               
              
                //foreach (CCircle c in dataManager.recipeDM.TeachingRD.DataCandidatePoint)
                //{
                //    c.Transform(RatioX, RatioY);
                //    if (canvas.Equals(StageMainView))
                //    {
                //        c.ScaleOffset(ZoomScale, OffsetX, OffsetY);
                //    }

                //    //   EllipseGeometry ellipses = new EllipseGeometry();

                //    //  ellipses = drawGeometryManager.AddCircle(GetRect(c), (int)(c.width / 2), (int)(c.height / 2));
                    
                //    if (canvas.Equals(StageMainView))
                //    {
                //        drawGeometryManager.AddCircle(ellipses[i], GetRect(c), (int)(c.width / 2), (int)(c.height / 2));
                //       // g.Children.Add(ellipses[i]);
                //        //listCandidatePoint.Add(ellipses[i]);
                //    }
                //    else
                //    {
                        
                //        drawGeometryManager.AddCircle(previewEllipses[i], GetRect(c), (int)(c.width / 2), (int)(c.height / 2));
                //      //  listPreviewCandidatePoint.Add(previewEllipses[i]);
                //      //  g2.Children.Add(previewEllipses[i]);
                //    }
                //    i++;
                //}


                if (canvas.Equals(StageMainView))
                {
                    listSelectedPoint.Clear();
                }
                else
                {
                    listPreviewSelectedPoint.Clear();
                }
            //foreach (CCircle c in dataManager.recipeDM.TeachingRD.DataSelectedPoint)
            //{
            //for(int i = 0; i < dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count; i++)
            //{ 
            //    CCircle circle = new CCircle(dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].x, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].y, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].width,
            //        dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].height, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].MeasurementOffsetX, dataManager.recipeDM.TeachingRD.DataSelectedPoint[i].MeasurementOffsetY);
            //    circle.Transform(RatioX, RatioY);
            //    if (canvas.Equals(StageMainView))
            //    {
            //        circle.ScaleOffset(ZoomScale, OffsetX, OffsetY);
            //    }

            //    EllipseGeometry ellipses = new EllipseGeometry();
            //    Circle c = GetRect(circle);
            //    ellipses = drawGeometryManager.AddCircle(c, (int)(circle.width / 2), (int)(circle.height / 2));
            //    //GraphicsPath mgpCircle = new GraphicsPath();
            //    //mgpCircle.AddEllipse(GetRect(c, bPreviewer));
            //    if (canvas.Equals(StageMainView))                    
            //    {
            //        listSelectedPoint.Add(ellipses);
            //    }
            //    else
            //    {
            //        //listPreviewSelectedPoint.Add(mgpCircle);
            //    }
            //}
            if (canvas.Equals(StageMainView))
            {
                listRouteLine.Clear();
                if (ShowRoute)
                {
                    for (int i = 0; i < dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Count - 1; i++)
                    {
                        routePathGeometry[i].Clear();
                        CCircle from = dataManager.recipeDM.TeachingRD.DataSelectedPoint[dataManager.recipeDM.TeachingRD.DataMeasurementRoute[i]];
                        CCircle to = dataManager.recipeDM.TeachingRD.DataSelectedPoint[dataManager.recipeDM.TeachingRD.DataMeasurementRoute[i + 1]];

                        from.Transform(RatioX, RatioY);
                        from.ScaleOffset(ZoomScale, OffsetX, OffsetY);

                        to.Transform(RatioX, RatioY);
                        to.ScaleOffset(ZoomScale, OffsetX, OffsetY);

                        PointF[] line = { new PointF((float)from.x + CenterX, (float)-from.y + CenterY), new PointF((float)to.x + CenterX, (float)-to.y + CenterY) };
                        StartPoint = new System.Windows.Point((float)from.x + CenterX, (float)-from.y + CenterY); // 시작포인트
                        drawGeometryManager.AddLine(line);
                        routePathGeometry[i].Figures.Add(drawGeometryManager.GetPathFigure(StartPoint));
                        listRouteLine.Add(routePathGeometry[i]);
                        drawGeometryManager.ClearSegments();
                    }
                }
            }
           
           





            int s = 0;
                foreach (CCircle c in dataManager.recipeDM.TeachingRD.DataSelectedPoint)
                {
                    c.Transform(RatioX, RatioY);
                    if (canvas.Equals(StageMainView))
                    {
                        c.ScaleOffset(ZoomScale, OffsetX, OffsetY);
                    }

                

                    drawGeometryManager.AddCircle(selectEllipses[s], GetRect(c), (int)(c.width / 2), (int)(c.height / 2));

                    if (canvas.Equals(StageMainView))
                    {

                        listSelectedPoint.Add(selectEllipses[s]);
                    }
                    else
                    {
                        listPreviewSelectedPoint.Add(selectEllipses[s]);
                    }
                    s++;
                }
            
        }
 
        public Circle GetRect(CCircle _circle)
        {
            Circle circle = new Circle();
            circle.X = (int)(((_circle.x + _circle.MeasurementOffsetX) - _circle.width * 0.5f) + CenterX);
            circle.Y = -(int)((-(_circle.y + _circle.MeasurementOffsetY) - _circle.height * 0.5f) + CenterY);

            circle.Width = (int)(_circle.width);
            circle.Height = (int)(_circle.height);

            return circle;
        }

        public void GetRect(ref Circle _circle)
        {
            _circle.X = (int)((_circle.X - _circle.Width * 0.5f) + CenterX);
            _circle.Y = (int)((-_circle.Y - _circle.Height * 0.5f) + CenterY);

            _circle.Width = (int)(_circle.Width);
            _circle.Height = (int)(_circle.Height);
        }

        public Rect GetRect(ShapeDraw.Line _line)
        {
            Rect rect = new Rect();

            rect.X = (int)((_line.X - _line.Width * 0.5f) + CenterX);
            rect.Y = (int)((-_line.Y - _line.Height * 0.5f) + CenterY);

            rect.Width = (int)(_line.Width);
            rect.Height = (int)(_line.Height);

            return rect;
        }

        public Rect GetRect(Rect _rect)
        {
            Rect rect = new Rect();

            rect.X = (int)((_rect.X - _rect.Width * 0.5f) + CenterX);
            rect.Y = (int)((-_rect.Y - _rect.Height * 0.5f) + CenterY);
            rect.Width = (int)(_rect.Width);
            rect.Height = (int)(_rect.Height);

            return rect;
        }

        public PointF GetPoint(PointF _pointF)
        {
            PointF pointF = new PointF();

            pointF.X = _pointF.X + CenterX;
            pointF.Y = -_pointF.Y + CenterY;

            return pointF;
        }

        public PointF[] GetPoints(PointF[] _points)
        {
            int nNum = _points.Count(); ;
            PointF[] points = new PointF[nNum];

            for (int i = 0; i < nNum; i++)
            {
                points[i].X = _points[i].X + CenterX;
                points[i].Y = -_points[i].Y + CenterY;
            }

            return points;
        }

        #region Brush
        readonly SolidColorBrush activeBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 220, 220, 220));
        readonly SolidColorBrush guideLineBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
        readonly SolidColorBrush stageShadeBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 0));
        readonly SolidColorBrush selectPointBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 255));
        readonly SolidColorBrush stageHoleBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 128, 128, 128));


        RadialGradientBrush gb = new RadialGradientBrush(
            new GradientStopCollection() { new GradientStop(new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 130, 130, 130)).Color, 0.3),
                    new GradientStop(new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 110, 110, 110)).Color, 0.6),
                    new GradientStop(new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 90, 90, 90)).Color, 1)});

        RadialGradientBrush gbSelect = new System.Windows.Media.RadialGradientBrush(
            new GradientStopCollection() {
                 new GradientStop(new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 210, 255)).Color, 0.2),
               
                    new GradientStop(new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 58, 167, 213)).Color, 1)});

        RadialGradientBrush gbHole = new System.Windows.Media.RadialGradientBrush(
            new GradientStopCollection() {
                 new GradientStop(new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 200, 200)).Color, 0.1),
                new GradientStop(new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 239, 59, 54)).Color, 0.3),
                    new GradientStop(new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 125, 125)).Color, 1)});
        #endregion


        private void DrawStage(Canvas canvas)
        {
            gbHole.GradientOrigin = new System.Windows.Point(0.3, 0.3); 

            if (stagePath.Parent != null)
            {
            (stagePath.Parent as Canvas).Children.Clear();
            }
           // drawPath = new System.Windows.Shapes.Path();
            stagePath.Stroke = System.Windows.SystemColors.ControlBrush;
            stagePath.Fill = gb;
            stagePath.Data = stageGeometry;
            canvas.Children.Add(stagePath);

           
            for (int i = 0; i < GuideLineNum; i++)
            {
               // drawPath = new System.Windows.Shapes.Path();
               
                guideLinePath[i].Stroke = guideLineBrush;
                guideLinePath[i].Opacity = 0.1d;
                if (canvas.Equals(StageMainView))
                {
                    guideLinePath[i].StrokeThickness = 5 * ZoomScale;
                }
                else
                {
                    guideLinePath[i].StrokeThickness = 5;
                }
                guideLinePath[i].StrokeDashArray = DoubleCollection.Parse("3, 1");
                guideLinePath[i].Data = guideEllipseGeometry[i];
                canvas.Children.Add(guideLinePath[i]);
            }
           
            //drawPath = new System.Windows.Shapes.Path();
            rectanglePath.Stroke = activeBrush;
            rectanglePath.Fill = activeBrush;
            rectanglePath.Data = rectangleGeometry;
            canvas.Children.Add(rectanglePath);


            for(int i = 0; i < EdgeNum; i++)
            {
               // drawPath = new System.Windows.Shapes.Path();
                edgePath[i].Fill = activeBrush;
                edgePath[i].Data = edgeArcPathGeometry[i];
                canvas.Children.Add(edgePath[i]);
            }

            for(int i = 0; i < DoubleHoleNum; i++)
            {
             
                doubleHolePath[i].Fill = activeBrush;
                doubleHolePath[i].Data = doubleHoleArcPathGeometry[i];
                canvas.Children.Add(doubleHolePath[i]);
            }

            for(int i = 0; i < 2; i++)
            {
                //topDoubleHolePath = new System.Windows.Shapes.Path();
                topBotDoubleHolePath[i].Fill = activeBrush;
                topBotDoubleHolePath[i].Data = topBotDoubleHoleArcPathGeometry[i];
                canvas.Children.Add(topBotDoubleHolePath[i]);
            }

            for(int i = 0; i < dataStageCircleHole.Count(); i++)
            {
                //drawPath = new System.Windows.Shapes.Path();
                circlePath[i].Fill = activeBrush;
                circlePath[i].Data = circleEllipseGeometry[i];
                canvas.Children.Add(circlePath[i]);
            }

            //drawPath = new System.Windows.Shapes.Path();
            stageEdgePath.Stroke = System.Windows.SystemColors.ControlBrush;
            if (canvas.Equals(StageMainView))
            {
                stageEdgePath.StrokeThickness = 5 * ZoomScale;
            }
            else
            {
                stageEdgePath.StrokeThickness = 5;
            }
            stageEdgePath.Data = stageGeometry;
            canvas.Children.Add(stageEdgePath);






            ////Preview Stage Shade
            //if (canvas.Equals(StagePreView))
            //{
            //    Rect rect = dataViewPosition;
            //    rect.X = rect.X / ZoomScale - OffsetX / ZoomScale * StagePreView.Width / (double)StageMainView.Width;
            //    rect.Y = rect.Y / ZoomScale - OffsetY / ZoomScale * StagePreView.Height / (double)StageMainView.Height;
            //    rect.Width /= ZoomScale;
            //    rect.Height /= ZoomScale;

            //    Rect ViewRect = GetRect(rect);
            //    if (ViewRect.X < 0)
            //    {
            //        if (ViewRect.Width + ViewRect.X < 0)
            //        {
            //            ViewRect.Width = 0;
            //        }
            //        else
            //        {
            //            ViewRect.Width += ViewRect.X;
            //        }
            //        ViewRect.X = 0;
            //    }
            //    if (ViewRect.Y < 0)
            //    {

            //        if (ViewRect.Height + ViewRect.Y < 0)
            //        {
            //            ViewRect.Height = 0;
            //        }
            //        else
            //        {
            //            ViewRect.Height += ViewRect.Y;
            //        }
            //        ViewRect.Y = 0;
            //    }
            //    double dHeight = StagePreView.Height - ViewRect.Y - ViewRect.Height;
            //    if (ViewRect.Y + ViewRect.Height > StagePreView.Height)
            //    {
            //        dHeight = 0;
            //    }
            //    double dWidth = StagePreView.Width - ViewRect.X - ViewRect.Width;
            //    if (ViewRect.X + ViewRect.Width > StagePreView.Width)
            //    {
            //        dWidth = 0;
            //    }
            //    Rect TopRect = new Rect(ViewRect.X, 0, ViewRect.Width, ViewRect.Y);
            //    Rect BottomRect = new Rect(ViewRect.X, ViewRect.Y + ViewRect.Height, ViewRect.Width, dHeight);
            //    Rect LeftRect = new Rect(0, 0, ViewRect.X, StagePreView.Height);
            //    Rect RightRect = new Rect(ViewRect.X + ViewRect.Width, 0, dWidth, StagePreView.Height);

            //    for (int i = 0; i < 4; i++)
            //    {
            //        previewRectangleGeometry[i] = new RectangleGeometry();
            //    }
            //    previewRectangleGeometry[0] = drawGeometryManager.AddRect(TopRect, CenterX, CenterY);
            //    previewRectangleGeometry[1] = drawGeometryManager.AddRect(BottomRect, CenterX, CenterY);
            //    previewRectangleGeometry[2] = drawGeometryManager.AddRect(LeftRect, CenterX, CenterY);
            //    previewRectangleGeometry[3] = drawGeometryManager.AddRect(RightRect, CenterX, CenterY);
            //    GeometryGroup shadeGroup = new GeometryGroup();
            //    shadeGroup.FillRule = FillRule.Nonzero;
            //    for (int i = 0; i < 4; i++)
            //    {
            //        shadeGroup.Children.Add(previewRectangleGeometry[i]);
            //    }


            //    if (ZoomScale != 1)
            //    {
            //        drawPath = new System.Windows.Shapes.Path();
            //        drawPath.Fill = stageShadeBrush;
            //        drawPath.Data = shadeGroup;
            //        canvas.Children.Add(drawPath);
            //    }
            //}

            GeometryGroup pointGroup = new GeometryGroup();
            pointGroup.FillRule = FillRule.Nonzero;
            //for(int i = 0; i < listCandidatePoint.Count; i++)
            //{
            //    if(EllipsePath[i].Parent != null)
            //    {
            //    (EllipsePath[i].Parent as Canvas).Children.Clear();

            //    }
            //}
            int s = 0;
            if (canvas.Equals(StageMainView))
            {
                //for(int j = 0; j < i; j++)
                //{
                //    //  drawPath = new System.Windows.Shapes.Path();
                //    EllipsePath[j].Fill = stageHoleBrush;
                //    EllipsePath[j].Data = ellipses[j];
                //canvas.Children.Add(EllipsePath[j]);

                //}
                if (CurrentCandidatePoint != -1)
                {
                    currentEllipseGeometry = listCandidatePoint[CurrentCandidatePoint];
                }
                foreach (EllipseGeometry eg in listCandidatePoint)
                {
                    // drawPath.clear = new System.Windows.Shapes.Path();

                    if (currentEllipseGeometry.Equals(eg))
                    {
                        EllipsePath[s].Fill = gbSelect;
                    }
                    else
                    {
                        EllipsePath[s].Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 128, 128, 128));
                    }
                    EllipsePath[s].Data = eg;
                    canvas.Children.Add(EllipsePath[s]);
                    //pointGroup.Children.Add(eg);
                    s++;
                }
            }
            //else
            //{
            //    drawPath = new System.Windows.Shapes.Path();
            //    drawPath.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 128, 128, 128));
            //    drawPath.Data = g2;
            //    canvas.Children.Add(drawPath);
            //    for (int j = 0; j < i; j++)
            //    {
            //        //drawPath = new System.Windows.Shapes.Path();
            //        //EllipsePath[j]
            //        PreviewEllipsePath[j].Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 128, 128, 128));
            //        PreviewEllipsePath[j].Data = previewEllipses[j];
            //        canvas.Children.Add(PreviewEllipsePath[j]);

            //    }
            //    foreach (EllipseGeometry eg in listPreviewCandidatePoint)
            //    {
            //        pointGroup.Children.Add(eg);
            //    }
            //    drawPath = new System.Windows.Shapes.Path();
            //    drawPath.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 128, 128, 128)); ;
            //    drawPath.Data = pointGroup;
            //    canvas.Children.Add(drawPath);
            //}


            s = 0;
            if (canvas.Equals(StageMainView))
            {
                if (CurrentSelectPoint != -1)
                {
                    selectEllipseGeometry = listSelectedPoint[CurrentSelectPoint];
                }
                else
                {
                    selectEllipseGeometry = new EllipseGeometry();
                }
                foreach (EllipseGeometry eg in listSelectedPoint)
                {
                    //drawPath = new System.Windows.Shapes.Path();
                    if (!selectEllipseGeometry.Equals(eg))
                    {
                        SelectEllipsePath[s].Fill = gbHole;
                    }
                    else
                    {
                        SelectEllipsePath[s].Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 128));
                    }
                    SelectEllipsePath[s].Data = eg;
                    canvas.Children.Add(SelectEllipsePath[s]);
                    //pointGroup.Children.Add(eg);
                    s++;
                }
            }
            //else
            //{
            //    if (CurrentSelectPoint != -1)
            //    {
            //        selectEllipseGeometry = listPreviewSelectedPoint[CurrentSelectPoint];
            //    }
            //    foreach (EllipseGeometry eg in listPreviewSelectedPoint)
            //    {
            //        drawPath = new System.Windows.Shapes.Path();
            //        if (!selectEllipseGeometry.Equals(eg))
            //        {
            //            drawPath.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 255, 0, 0));
            //        }
            //        else
            //        {
            //            drawPath.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 128));
            //        }
            //        drawPath.Data = eg;
            //        canvas.Children.Add(drawPath);
            //        //pointGroup.Children.Add(eg);
            //    }

            //    //foreach (EllipseGeometry eg in listPreviewSelectedPoint)
            //    //{
            //    //    pointGroup.Children.Add(eg);
            //    //}
            //    //drawPath = new System.Windows.Shapes.Path();
            //    //if (!selectEllipseGeometry.Equals(eg))
            //    //{
            //    //    drawPath.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 255, 0, 0));
            //    //}
            //    //else
            //    //{
            //    //    drawPath.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 128));
            //    //}
            //    //drawPath.Data = pointGroup;
            //    //canvas.Children.Add(drawPath);
            //}

            s = 0;
            foreach(PathGeometry pg in listRouteLine)
            {
                RouteLinePath[s].Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 128));
                RouteLinePath[s].StrokeThickness = 3;
                RouteLinePath[s].Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 128));
                RouteLinePath[s].Data = pg;
                canvas.Children.Add(RouteLinePath[s]);
                s++;
            }


            // Select Rect
            if (canvas.Equals(StageMainView))
            {
                Rect selectRect = new Rect(Math.Min(SelectStartPoint.X, SelectEndPoint.X), Math.Min(SelectStartPoint.Y, SelectEndPoint.Y),
                    Math.Abs(SelectStartPoint.X - SelectEndPoint.X), Math.Abs(SelectStartPoint.Y - SelectEndPoint.Y));

                selectRectangleGeometry = drawGeometryManager.AddRect(selectRect, CenterX, CenterY);

                //drawPath = new System.Windows.Shapes.Path();
                selectRectPath.Fill = selectPointBrush;
                selectRectPath.Data = selectRectangleGeometry;
                canvas.Children.Add(selectRectPath);
            }
        }
        #endregion

        System.Windows.Shapes.Path[] EllipsePath = new System.Windows.Shapes.Path[50000];
        System.Windows.Shapes.Path[] SelectEllipsePath = new System.Windows.Shapes.Path[50000];
        System.Windows.Shapes.Path[] PreviewEllipsePath = new System.Windows.Shapes.Path[50000];
        System.Windows.Shapes.Path[] RouteLinePath = new System.Windows.Shapes.Path[25000];

        #region Getter, Setter


        public bool ShowRoute { get; set; }
        public Canvas StageMainView { get; set; }

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

        DataTable pointListItem;
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

        public System.Windows.Point CurrentMousePoint{ get; set; }
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
        public int ZoomScale { 
            get
            {
                return nZoomScale;
            }
            set
            {
                if(0 < value && value < 64)
                {
                    if(ZoomScale < value)
                    {
                        nZoomScale = value;
                        VerticalScroll.Maximum = HorizontalScroll.Maximum = 10 * nZoomScale;
                        VerticalScroll.Visibility = Visibility.Visible;
                        HorizontalScroll.Visibility = Visibility.Visible;


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
                        VerticalScroll.Maximum = HorizontalScroll.Maximum = 10 * nZoomScale;

                        if (nZoomScale == 1)
                        {
                            VerticalScroll.Visibility = Visibility.Hidden;
                            HorizontalScroll.Visibility = Visibility.Hidden;
                            VerticalScroll.Value = VerticalScroll.Maximum;
                            HorizontalScroll.Value = HorizontalScroll.Maximum;
                            HorizontalScroll.Track.ViewportSize = double.NaN;
                            HorizontalScroll.Track.Thumb.Height = 100;

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

        #region Event


        public void UpdateView(bool bMain = false)
        {
            CurrentCandidatePoint = -1;
            CurrentSelectPoint = -1;
            SetStage(StageMainView);
            DrawStage(StageMainView);  
            if (!bMain)
            {
             //   SetStage(StagePreView);
              //  DrawStage(StagePreView);
            }
        }



        public void UpdateListView()
        {
            PointListItem.Clear();
            int nCount = 0;
            int nSelCnt = dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count;
            int[] MeasurementOrder = new int[nSelCnt];

            for(int i = 0; i < nSelCnt; i++)
            {
                MeasurementOrder[dataManager.recipeDM.TeachingRD.DataMeasurementRoute[i]] = i;
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
              //  object[] item = { (nCount + 1).ToString(), Math.Round(c.x, 3).ToString(), Math.Round(c.y, 3).ToString(), (nRoute + 1).ToString() };
                //row.ItemArray = item;
                PointListItem.Rows.Add(row);

            }
            PointCount = PointListItem.Rows.Count.ToString();
        }

        public ICommand btn13Point
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ReadPreset("13 Point.prs");

                    UpdateView();

                    UpdateListView();
                });
            }
        }

        public ICommand btn25Point
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ReadPreset("25 Point.prs");

                    UpdateView();

                    UpdateListView();
                });
            }
        }

        public ICommand btn49Point
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ReadPreset("49 Point.prs");
      
                    UpdateView();

                    UpdateListView();
                });
            }
        }

        public ICommand btn73Point
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ReadPreset("73 Point.prs");

                    UpdateView();

                    UpdateListView();
                });
            }
        }

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

        public bool MouseMove { get; set; }
        public bool LeftMouseDown { get; set; }
        public bool RightMouseDown { get; set; }
        public System.Windows.Point MousePoint { get; set; }
        public void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            
            UIElement el = (UIElement)sender;

            System.Windows.Point pt = e.GetPosition(StageMainView);
            MousePoint = new System.Windows.Point(pt.X, pt.Y);
            if (!RightMouseDown && !LeftMouseDown)
            {
            PrintMousePosition(MousePoint);

            }

            //IsCircleHover(MousePoint);
            //if(CurrentCandidatePoint != -1)
            //{
            //    UpdateView();
            //}

            


            if (e.RightButton == MouseButtonState.Pressed)
            {
                MouseMove = true;
                RightMouseDown = true;
                if (LeftMouseDown || ZoomScale == 1)
                {
                    if (LeftMouseDown)
                    {
                        SelectStartPoint = new System.Windows.Point(pt.X, pt.Y);
                        SelectEndPoint = new System.Windows.Point(pt.X, pt.Y);
                        UpdateView(true);
                    }
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
                UpdateView();
                MouseMove = false;

            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                Mouse.Capture((UIElement)sender, CaptureMode.Element);
                //el.CaptureMouse();
                if (Drag)
                {
                    SelectEndPoint = new System.Windows.Point(pt.X, pt.Y);

                    UpdateView(true);
                }


                //SelectEndPoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);
                //UpdateView(true);
                //MouseMove = true;
            }
            //else if (e.RightButton == MouseButtonState.Released)
            //{
            //    RightMouseDown = false;
            //    MouseMove = false;
            //}
            //else if (e.LeftButton == MouseButtonState.Released)
            //{
            //   // MouseMove = false;
            //    //if (MouseMove)
            //    //{
            //        el.ReleaseMouseCapture();
            //        SelectStartPoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);
            //        SelectEndPoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);
            //        UpdateView(true);
            //        LeftMouseDown = false;
            //        MouseMove = false;
            //    //}
            //    RightMouseDown = false;
            //    //SetStage(StagePreView);
            //    //DrawStage(StagePreView);
            //}
        }

        public void OnMouseMovePreView(object sender, System.Windows.Input.MouseEventArgs e)
        {

            System.Windows.Point pt = e.GetPosition(StagePreView);
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

                UpdateView();
            }
        }

        public void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            int lines = e.Delta * SystemInformation.MouseWheelScrollLines / 120;

            System.Windows.Point pt = e.GetPosition(StageMainView);
            

            
            if (lines > 0)
            {

               
                if(ZoomScale < 32)
                {
                    OffsetX += (CenterX - (int)pt.X);
                    OffsetY += -(CenterY - (int)pt.Y);
                }
                ZoomScale *= 2;
     
            }
            else if(lines < 0)
            {
                OffsetX += (CenterX - (int)pt.X);
                OffsetY += -(CenterY - (int)pt.Y);
                ZoomScale /= 2;
                if(ZoomScale == 1)
                {
                    OffsetX = OffsetY = 0;
                }


            }
            UpdateView();
        }

        public void OnMouseWheelPreView(object sender, MouseWheelEventArgs e)
        {
            int lines = e.Delta * SystemInformation.MouseWheelScrollLines / 120;

            MousePoint = e.GetPosition(StagePreView);

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
                    OffsetX = OffsetY = 1;
                }
            }
            UpdateView();
        }


        public bool Drag { get; set; }
        public void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point pt = e.GetPosition(StageMainView);

            SelectStartPoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);
            SelectEndPoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);
            Drag = true;

            //if (MouseMove)
            //{
            //    return;
            //}
            //LeftMouseDown = true;
            //MousePoint = e.GetPosition(StageMainView);

            //SelectStartPoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);
            //SelectEndPoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);



            //Ellipse currentDot = new Ellipse();
            //currentDot.Stroke = new SolidColorBrush(Colors.Green);
            //currentDot.StrokeThickness = 1;

            //currentDot.Height = 20;
            //currentDot.Width = 20;
            //currentDot.Fill = new SolidColorBrush(Colors.Green);


            //Canvas.SetLeft(currentDot, pt.X - 10) ;
            //Canvas.SetTop(currentDot, pt.Y - 10);
            //StageMainView.Children.Add(currentDot);
            //StageMainView.InvalidateVisual();
        }

        public void OnMouseLeftButtonDownPreView(object sender, MouseButtonEventArgs e)
        {
            MousePoint = e.GetPosition(StagePreView);

            //Ellipse currentDot = new Ellipse();
            //currentDot.Stroke = new SolidColorBrush(Colors.Green);
            //currentDot.StrokeThickness = 1;

            //currentDot.Height = 20;
            //currentDot.Width = 20;
            //currentDot.Fill = new SolidColorBrush(Colors.Green);


            //Canvas.SetLeft(currentDot, pt.X - 10);
            //Canvas.SetTop(currentDot, pt.Y - 10);
            //StageMainView.Children.Add(currentDot);


            //System.Windows.Controls.Label label = new System.Windows.Controls.Label();



            //label.Content = XPosition + "," + YPosition;

            //label.FontSize = 20;

            //Canvas.SetLeft(label, pt.X - 10);
            //Canvas.SetTop(label, pt.Y  - 10);


            //StageMainView.Children.Add(label);


            if (ZoomScale == 1)
            {
                return;
            }

            int nPreviewX = (int)(MousePoint.X - CenterX);
            int nPreviewY = (int)(MousePoint.Y - CenterY);

            OffsetX = -(int)((nPreviewX * ZoomScale));
            OffsetY = (int)((nPreviewY * ZoomScale));



            UpdateView();

        }

        public int Limit { get; set; } = 5;
        public void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UIElement el = (UIElement)sender;
            Drag = false;
            System.Windows.Point pt = e.GetPosition(StageMainView);
            el.ReleaseMouseCapture();

            if(Math.Abs(SelectStartPoint.X - SelectEndPoint.X) > Limit || Math.Abs(SelectStartPoint.Y - SelectEndPoint.Y) > Limit)
            {
                dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Clear();
                dataManager.recipeDM.TeachingRD.DataSelectedPoint.Clear();
              


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
                        if (ContainsData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index))
                        {
                            DeletePointNotInvalidate(_index, 1);
                            //m_DM.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Delete - Index : " + (_index + 1).ToString()
                            //    + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());
                        }
                        //if (bShiftKeyDown == false)
                        //{
                        dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                        dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                        //    m_DM.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Add - Index : " + m_DM.m_RDM.TeachingRD.DataSelectedPoint.Count.ToString()
                        //        + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());
                        //}
                    }
                }
            }
            else
            {
                if (CurrentCandidatePoint == -1 && CurrentSelectPoint == -1)
                {
                   // MethodPointSelect(pt);
                }
                else
                {
                    MethodCircleSelect();
                }
            }



            SelectStartPoint = new System.Windows.Point(pt.X, pt.Y);
            SelectEndPoint = new System.Windows.Point(pt.X, pt.Y);
            
            UpdateView(true);
            UpdateListView();
            //UIElement el = (UIElement)sender;
            //MousePoint = e.GetPosition(StageMainView);

            //SelectStartPoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);
            //SelectEndPoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);

            //el.ReleaseMouseCapture();
            //UpdateView(true);
            //LeftMouseDown = false;
            //MouseMove = false;
            //RightMouseDown = false;


            //MethodCircleSelect();





            //Ellipse currentDot = new Ellipse();
            //currentDot.Stroke = new SolidColorBrush(Colors.Green);
            //currentDot.StrokeThickness = 1;

            //currentDot.Height = 20;
            //currentDot.Width = 20;
            //currentDot.Fill = new SolidColorBrush(Colors.Green);


            //Canvas.SetLeft(currentDot, pt.X - 10) ;
            //Canvas.SetTop(currentDot, pt.Y - 10);
            //StageMainView.Children.Add(currentDot);
            //StageMainView.InvalidateVisual();
        }

        private void MethodPointSelect(System.Windows.Point pt)
        {
            double dDistance = 0;
            double dMin = 9999;
            int nIndex = 0;
            int nMinIndex = -1;

            foreach (EllipseGeometry eg in listCandidatePoint)
            {
                dDistance = GetDistance(eg, new System.Windows.Point(pt.X, pt.Y));

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
                dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                //m_DM.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Add - Index : " + m_DM.m_RDM.TeachingRD.DataSelectedPoint.Count.ToString()
                //                    + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());

                //InvalidateView();
            }
            //}
            //else
            //{
            //    MessageBox.Show("Input is Not Valid");
            //}
        }

        private double GetDistance(EllipseGeometry eg, System.Windows.Point pt)
        {
            double dResult = 0f;
            float x = 0, y = 0, maxX = -1, maxY = -1, minX = 9999, minY = 9999;

            //foreach (PointF p in GP.PathPoints)
            //{
            //    if (maxX < p.X)
            //    {
            //        maxX = p.X;
            //    }
            //    if (p.X < minX)
            //    {
            //        minX = p.X;
            //    }

            //    if (maxY < p.Y)
            //    {
            //        maxY = p.Y;
            //    }
            //    if (p.Y < minY)
            //    {
            //        minY = p.Y;
            //    }
            //}
            //e.Bounds.l
            //eg.

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
                if (ContainsSelectedData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index))
                {
                    DeletePoint(_index, 1);
                    //m_DM.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Delete - Index : " + (_index + 1).ToString()
                    //                + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());
                }
                else
                {
                    dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                    dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                    //dataManager.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Add - Index : " + m_DM.m_RDM.TeachingRD.DataSelectedPoint.Count.ToString()
                    //                + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());

                    UpdateView();
                    UpdateListView();
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
                if (ContainsSelectedData(dataManager.recipeDM.TeachingRD.DataSelectedPoint, circle, out _index))
                {
                    DeletePoint(_index, 1);
                    //dataManager.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Delete - Index : " + (_index + 1).ToString()
                    //                + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());
                }
                else
                {
                    dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                    dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                    //dataManager.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Point Editor - Add - Index : " + m_DM.m_RDM.TeachingRD.DataSelectedPoint.Count.ToString()
                    //                + ", X : " + Math.Round(circle.x, 3).ToString() + ", Y : " + Math.Round(circle.y, 3).ToString());

                    //SetListViewTab(0);
                    //InvalidateView();
                    UpdateView();
                    UpdateListView();
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

            UpdateView();
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
            MousePoint = e.GetPosition(StageMainView);
            CurrentMousePoint = new System.Windows.Point(MousePoint.X, MousePoint.Y);
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
                    double dPercent = -1;
                    if(double.TryParse(Percentage,out dPercent))
                    {
                        if(0 <= dPercent && dPercent <= 100)
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
                            UpdateView();
                            UpdateListView();
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
                    dataManager.recipeDM.TeachingRD.DataSelectedPoint.Clear();
                    dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Clear();

                    foreach (CCircle c in dataManager.recipeDM.TeachingRD.DataCandidatePoint)
                    {
                        CCircle circle = c;

                        dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Add(dataManager.recipeDM.TeachingRD.DataSelectedPoint.Count);
                        dataManager.recipeDM.TeachingRD.DataSelectedPoint.Add(circle);
                    }

                    UpdateView();
                    UpdateListView();

                });
            }
        }

        public ICommand btnDeleteAll
        {
            get
            {
                return new RelayCommand(() =>
                {
                    dataManager.recipeDM.TeachingRD.DataSelectedPoint.Clear();
                    dataManager.recipeDM.TeachingRD.DataMeasurementRoute.Clear();
                    UpdateView();
                    UpdateListView();
                });
            }
        }

        public ICommand btnReset
        {
            get
            {
                return new RelayCommand(() =>
                {
                    dataManager.recipeDM.TeachingRD.ClearPoint();
                    //dataSelectedPoint.Clear();

                    UpdateView();
                    UpdateListView();

                });
            }
        }

        public ICommand btnPreset1
        {
            get
            {
                return new RelayCommand(() =>
                {
                    dataManager.recipeDM.TeachingRD.ClearCandidatePoint();
                    dataManager.recipeDM.TeachingRD.DataCandidatePoint = (List<CCircle>)GeneralFunction.Read(dataManager.recipeDM.TeachingRD.DataCandidatePoint, BaseDefine.Dir_StageMap + "Preset 1.smp");
                    //dataManager.recipeDM.TeachingRD.DataSelectedPoint = dataManager.recipeDM.PresetData.DataSelectedPoint;
                    //dataManager.recipeDM.TeachingRD.DataMeasurementRoute = dataManager.recipeDM.PresetData.DataMeasurementRoute;
                    CheckSelectedPoint();

                    UpdateView();

                    UpdateListView();
                });
            }
        }

        public ICommand btnPreset2
        {
            get
            {
                return new RelayCommand(() =>
                {
                    dataManager.recipeDM.TeachingRD.ClearCandidatePoint();
                    dataManager.recipeDM.TeachingRD.DataCandidatePoint = (List<CCircle>)GeneralFunction.Read(dataManager.recipeDM.TeachingRD.DataCandidatePoint, BaseDefine.Dir_StageMap + "Preset 2.smp"); 
                    //dataManager.recipeDM.TeachingRD.DataSelectedPoint = dataManager.recipeDM.PresetData.DataSelectedPoint;
                    //dataManager.recipeDM.TeachingRD.DataMeasurementRoute = dataManager.recipeDM.PresetData.DataMeasurementRoute;
                    CheckSelectedPoint();

                    UpdateView();

                    UpdateListView();
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
            double mouseX = ((((int)pt.X - CenterX) - OffsetX)/ (double)ZoomScale) / RatioX;
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
            StageMouseHover = true;
        }

        public void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            StageMouseHover = false;
        }

        public bool StageMouseHover { get; set; }

        public void OnCheckboxChange(object sender, RoutedEventArgs e)
        {
            UpdateView();
        }
    }

   
}
