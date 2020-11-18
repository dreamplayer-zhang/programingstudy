using Root_CAMELLIA.Draw;
using Root_CAMELLIA.ShapeDraw;
using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Root_CAMELLIA.Stage
{
    public class StageMap_ViewModel : ObservableObject
    {
        public StageMap_ViewModel()
        {
            SetStage(false);
        }

        public virtual void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MessageBox.Show("OnMouseWheel");
        }

        public virtual void OnMouseMove(object sender, MouseEventArgs e)
        {
            MessageBox.Show("OnMouseMove");
        }

        public int CenterX { get; set; }
        public int CenterY { get; set; }
        public double RatioX { get; set; }
        public double RatioY { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
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

        public ObservableCollection<UIElement> p_StageElement
        {
            get
            {
                return m_StageElement;
            }
            set
            {
                m_StageElement = value;
            }
        }
        private ObservableCollection<UIElement> m_StageElement = new ObservableCollection<UIElement>();

        public Circle viewStageField = new Circle();
        public Line viewStageLineHole = new Line();
        public Arc[] viewStageEdgeHoleArc = new Arc[8];
        public Circle[] ViewStageGuideLine = new Circle[4];
        public Arc[] viewStageDoubleHoleArc = new Arc[8];

        public Arc[] viewStageTopHoleArc = new Arc[2];
        public Arc[] viewStageBotHoleArc = new Arc[2];

        private DrawGeometryManager drawGeometryManager = new DrawGeometryManager();
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
                m_StageElement.Add(stageField.path);
            }
            else
            {
                //PreviewGeometry.Add(stageField);
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
                m_StageElement.Add(rectLine.path);
            }
            else
            {
                //PreviewGeometry.Add(rectLine);
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
                    m_StageElement.Add(guideLine.path);
                }
                else
                {
                    guideLine.SetData(ViewStageGuideLine[i], CenterX, CenterY, 5);
                    //PreviewGeometry.Add(guideLine);
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
                    m_StageElement.Add(edgePath.path);
                }
                else
                {
                    //PreviewGeometry.Add(edgePath);
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
                    m_StageElement.Add(doubleHole.path);
                }
                else
                {
                    //PreviewGeometry.Add(doubleHole);
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
                    m_StageElement.Add(topBotDoubleHole.path);
                }
                else
                {
                    //PreviewGeometry.Add(topBotDoubleHole);
                }
                drawGeometryManager.ClearSegments();
            }


            //// 스테이지 홀
            //foreach (Circle circle in dataStageCircleHole)
            //{
            //    stage = new CustomEllipseGeometry(GeneralTools.ActiveBrush, GeneralTools.ActiveBrush);
            //    CustomEllipseGeometry circleHole = stage as CustomEllipseGeometry;
            //    viewStageCircleHole.Set(circle);
            //    viewStageCircleHole.Transform(RatioX, RatioY);
            //    if (!preview)
            //    {
            //        viewStageCircleHole.ScaleOffset(ZoomScale, OffsetX, OffsetY);
            //    }
            //    drawGeometryManager.GetRect(ref viewStageCircleHole, CenterX, CenterY);
            //    circleHole.SetData(viewStageCircleHole, (int)(viewStageCircleHole.Width / 2),
            //        (int)(viewStageCircleHole.Y + (viewStageCircleHole.Height / 2) + viewStageCircleHole.Y));
            //    if (!preview)
            //    {
            //        Geometry.Add(circleHole);
            //    }
            //    else
            //    {
            //        PreviewGeometry.Add(circleHole);
            //    }
            //}


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
                m_StageElement.Add(stageEdge.path);
            }
            else
            {
                stageEdge.SetData(viewStageField, CenterX, CenterY, 3);
                //PreviewGeometry.Add(stageEdge);
            }

        }
    }
}
