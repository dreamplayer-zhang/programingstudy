using RootTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace Root_WindII_Option.UI
{
    public class BacksideInspect_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        #region [ColorDefines]
        public class ColorDefines
        {
            public static SolidColorBrush Circle = Brushes.Blue;
            public static SolidColorBrush SearchCenterPoint = Brushes.Magenta;
            public static SolidColorBrush SearchCircle = Brushes.Yellow;
            public static SolidColorBrush Map = Brushes.YellowGreen;
            public static SolidColorBrush MapFill = Brushes.Transparent;

            public static SolidColorBrush ExclusivePolyStroke = Brushes.Red;
            public static SolidColorBrush ExclusivePolyFill = Brushes.Red;
        }

        #endregion

        #region [Properties]
        private bool isColorChecked = true;
        public bool IsColorChecked
        {
            get => this.isColorChecked;
            set
            {
                if (value == true)
                {
                    this.IsRChecked = false;
                    this.IsGChecked = false;
                    this.IsBChecked = false;
                }
                p_eColorViewMode = eColorViewMode.All;
                SetProperty<bool>(ref this.isColorChecked, value);
            }
        }

        private bool isRChecked = false;
        public bool IsRChecked
        {
            get => this.isRChecked;
            set
            {
                if (value == true)
                {
                    this.IsColorChecked = false;
                    this.IsGChecked = false;
                    this.IsBChecked = false;
                }
                p_eColorViewMode = eColorViewMode.R;
                SetProperty<bool>(ref this.isRChecked, value);
            }
        }

        private bool isGChecked = false;
        public bool IsGChecked
        {
            get => this.isGChecked;
            set
            {
                if (value == true)
                {
                    this.IsRChecked = false;
                    this.IsColorChecked = false;
                    this.IsBChecked = false;
                }
                p_eColorViewMode = eColorViewMode.G;
                SetProperty<bool>(ref this.isGChecked, value);
            }
        }

        private bool isBChecked = false;
        public bool IsBChecked
        {
            get => this.isBChecked;
            set
            {
                if (value == true)
                {
                    this.IsRChecked = false;
                    this.IsGChecked = false;
                    this.IsColorChecked = false;
                }
                p_eColorViewMode = eColorViewMode.B;
                SetProperty<bool>(ref this.isBChecked, value);
            }
        }

        private bool isDefectChecked = true;
        public bool IsDefectChecked
        {
            get => this.isDefectChecked;
            set
            {
                if (value)
                {
                    foreach (TRect rt in rectList)
                    {
                        if (p_DrawElement.Contains(rt.UIElement) == false)
                        {
                            p_DrawElement.Add(rt.UIElement);
                        }
                    }
                }
                else
                {
                    foreach (TRect rt in rectList)
                    {
                        if (p_DrawElement.Contains(rt.UIElement) == true)
                        {
                            p_DrawElement.Remove(rt.UIElement);
                        }
                    }
                }

                SetProperty(ref this.isDefectChecked, value);
            }
        }
        #endregion

        public class DrawDefines
        {
            public static int RectTickness = 4;
        }


        List<TRect> rectList;

        public BacksideInspect_ImageViewer_ViewModel()
        {
            this.p_VisibleMenu = System.Windows.Visibility.Collapsed;

            rectList = new List<TRect>();

            InitializeUIElements();
        }

        #region [UI Members]
        private CPoint searchedCenterMemoryPoint = new CPoint();
        private Grid SearchedCenterPointUI;

        private List<Polygon> ExclusivePolyList;
        private List<List<Point>> exclusivePolyMemPointsList = new List<List<Point>>();
        #endregion
        private void InitializeUIElements()
        {

            #region [SearchedCircle]
            SearchedCenterPointUI = new Grid();
            SearchedCenterPointUI.Width = 30;
            SearchedCenterPointUI.Height = 30;

            Line line1 = new Line(); // H
            line1.X1 = 0;
            line1.Y1 = 0;
            line1.X2 = 30;
            line1.Y2 = 30;
            line1.Stroke = ColorDefines.SearchCenterPoint;
            line1.StrokeThickness = 3;

            Line line2 = new Line(); // H
            line2.X1 = 0;
            line2.Y1 = 30;
            line2.X2 = 30;
            line2.Y2 = 0;
            line2.Stroke = ColorDefines.SearchCenterPoint;
            line2.StrokeThickness = 3;

            Canvas.SetZIndex(line1, 98);
            Canvas.SetZIndex(line2, 98);

            SearchedCenterPointUI.Children.Add(line1);
            SearchedCenterPointUI.Children.Add(line2);

            #endregion


            #region [Exclusive Polygon]
            ExclusivePolyList = new List<Polygon>();
            #endregion
        }
        #region [Overrides]

        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            //RedrawShapes();
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            RedrawShapes();

        }
        public override void PreviewMouseUp(object sender, MouseEventArgs e)
        {
            base.PreviewMouseUp(sender, e);
            //RedrawShapes();
        }


        public override void SetRoiRect()
        {
            base.SetRoiRect();
            RedrawShapes();
        }

        public void ReadCenterPoint()
        {
            try
            {
                if (File.Exists(Constants.FilePath.BacksideCenterPointFilePath))
                {
                    using (var sr = new StreamReader(Constants.FilePath.BacksideCenterPointFilePath))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(BacksideCircleData));

                        BacksideCircleData data = (BacksideCircleData)xs.Deserialize(sr);
                        CPoint centerPt = data.CenterPoint;

                        SetSearchedCenter(centerPt);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ReadExclusivePolygon()
        {
            try
            {
                if (File.Exists(Constants.FilePath.BacksideExclusiveRegionFilePath))
                {
                    ClearExclusivePolygon();

                    using (var sr = new StreamReader(Constants.FilePath.BacksideExclusiveRegionFilePath))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(List<List<Point>>));
                        this.exclusivePolyMemPointsList = (List<List<Point>>)xs.Deserialize(sr);

                        foreach (List<Point> pointList in this.exclusivePolyMemPointsList)
                        {
                            Polygon polygon = new Polygon();

                            polygon.Stroke = ColorDefines.ExclusivePolyStroke;
                            polygon.StrokeThickness = 2;
                            polygon.Fill = ColorDefines.ExclusivePolyFill;
                            polygon.Opacity = 0.3;
                            this.ExclusivePolyList.Add(polygon);

                            foreach (Point pt in pointList)
                            {
                                CPoint canvasPt = GetCanvasPoint(pt);
                                polygon.Points.Add(new Point(canvasPt.X, canvasPt.Y));
                            }

                            this.p_UIElement.Add(polygon);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region [Draw Method]
        public void SetSearchedCenter(CPoint memPt)
        {
            this.searchedCenterMemoryPoint = memPt;
            if (this.p_UIElement.Contains(SearchedCenterPointUI))
                this.p_UIElement.Remove(SearchedCenterPointUI);

            //Canvas.SetZIndex(this.SearchedCenterPointUI, 99);
            this.p_UIElement.Add(SearchedCenterPointUI);

            DrawSearchedCircle();
        }

        public void DrawSearchedCircle()
        {
            if (this.p_UIElement.Contains(SearchedCenterPointUI))
            {
                CPoint canvasPt = GetCanvasPoint(this.searchedCenterMemoryPoint);

                Canvas.SetLeft(SearchedCenterPointUI, canvasPt.X - 15);
                Canvas.SetTop(SearchedCenterPointUI, canvasPt.Y - 15);
            }
        }

        public void DrawExclusivePolygon()
        {
            int i = 0;
            foreach (Polygon polygon in this.ExclusivePolyList)
            {
                polygon.Points.Clear();
                foreach (Point pt in this.exclusivePolyMemPointsList[i])
                {
                    CPoint canvasPt = GetCanvasPoint(pt);
                    polygon.Points.Add(new Point(canvasPt.X, canvasPt.Y));
                }
                i++;
            }
        }

        public void ClearExclusivePolygon()
        {
            foreach (Polygon polygon in this.ExclusivePolyList)
            {
                if (this.p_UIElement.Contains(polygon))
                {
                    this.p_UIElement.Remove(polygon);
                }
            }
            this.ExclusivePolyList.Clear();
            this.exclusivePolyMemPointsList.Clear();
        }


        public void AddDrawRect(CRect rect, SolidColorBrush color = null, string tag = "")
        {
            if (color == null)
            {
                color = Brushes.Yellow;
            }

            CPoint canvasLeftTop = GetCanvasPoint(new CPoint(rect.Left, rect.Top));
            CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rect.Right, rect.Bottom));

            Rectangle rt = new Rectangle();
            rt.Width = canvasRightBottom.X - canvasLeftTop.X;
            rt.Height = canvasRightBottom.Y - canvasLeftTop.Y;

            rt.Stroke = color;
            rt.StrokeThickness = DrawDefines.RectTickness;
            rt.Opacity = 1;
            rt.Tag = tag.Clone();

            Canvas.SetLeft(rt, canvasLeftTop.X);
            Canvas.SetTop(rt, canvasLeftTop.Y);

            TRect tRect = new TRect();
            tRect.UIElement = rt;
            tRect.MemoryRect.Left = rect.Left;
            tRect.MemoryRect.Top = rect.Top;
            tRect.MemoryRect.Right = rect.Right;
            tRect.MemoryRect.Bottom = rect.Bottom;

            rectList.Add(tRect);
            p_DrawElement.Add(rt);
        }

        public void AddDrawRectList(List<CRect> rectList, SolidColorBrush color = null, string tag = "")
        {
            foreach (CRect rect in rectList)
            {
                AddDrawRect(rect, color, tag);
            }
        }

        public void AddDrawRect(CPoint leftTop, CPoint rightBottom, SolidColorBrush color = null, string tag = "")
        {
            if (color == null)
            {
                color = Brushes.Yellow;
            }

            CPoint canvasLeftTop = GetCanvasPoint(leftTop);
            CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rightBottom));

            Rectangle rt = new Rectangle();
            rt.Width = canvasRightBottom.X - canvasLeftTop.X;
            rt.Height = canvasRightBottom.Y - canvasLeftTop.Y;

            rt.Stroke = color;
            rt.StrokeThickness = DrawDefines.RectTickness;
            rt.Opacity = 1;
            rt.Tag = tag.Clone();

            Canvas.SetLeft(rt, canvasLeftTop.X);
            Canvas.SetTop(rt, canvasLeftTop.Y);

            TRect tRect = new TRect();
            tRect.UIElement = rt;
            tRect.MemoryRect.Left = leftTop.X;
            tRect.MemoryRect.Top = leftTop.Y;
            tRect.MemoryRect.Right = rightBottom.X;
            tRect.MemoryRect.Bottom = rightBottom.Y;

            rectList.Add(tRect);

            if(this.IsDefectChecked == true)
                p_DrawElement.Add(rt);
        }


        public void AddDrawText(CRect rect, string text, SolidColorBrush color = null, string tag = "")
        {
            Grid grid = new Grid();
            TextBlock tb = new TextBlock();

            CPoint canvasLeftTop = GetCanvasPoint(new CPoint(rect.Left, rect.Top));
            CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rect.Right, rect.Bottom));

            tb.Text = text;
            tb.Width = canvasRightBottom.X - canvasLeftTop.X;
            tb.Height = canvasRightBottom.Y - canvasLeftTop.Y;
            tb.Foreground = color;
            tb.FontSize = 15;
            tb.Tag = tag.Clone();

            grid.Children.Add(tb);

            Canvas.SetLeft(grid, canvasLeftTop.X);
            Canvas.SetTop(grid, canvasLeftTop.Y);

            p_DrawElement.Add(grid);
        }

        public void AddDrawText(CPoint leftTop, CPoint rightBottom, string text, SolidColorBrush color = null)
        {
            Grid grid = new Grid();
            TextBlock tb = new TextBlock();

            CPoint canvasLeftTop = GetCanvasPoint(leftTop);
            CPoint canvasRightBottom = GetCanvasPoint(rightBottom);

            tb.Text = text;
            tb.Width = canvasRightBottom.X - canvasLeftTop.X;
            tb.Height = canvasRightBottom.Y - canvasLeftTop.Y;
            tb.Foreground = color;
            tb.FontSize = 15;
            grid.Children.Add(tb);

            Canvas.SetLeft(grid, canvasLeftTop.X);
            Canvas.SetTop(grid, canvasLeftTop.Y);

            p_DrawElement.Add(grid);
        }

        bool isRedrawing = false;
        private void RedrawShapes()
        {
            if (isRedrawing == true) return;
            isRedrawing = true;

            if(this.IsDefectChecked == true)
            {
                foreach (TRect rt in rectList)
                {
                    Rectangle rectangle = rt.UIElement as Rectangle;
                    CPoint canvasLeftTop = GetCanvasPoint(new CPoint(rt.MemoryRect.Left, rt.MemoryRect.Top));
                    CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rt.MemoryRect.Right, rt.MemoryRect.Bottom));

                    rectangle.Width = canvasRightBottom.X - canvasLeftTop.X;
                    rectangle.Height = canvasRightBottom.Y - canvasLeftTop.Y;

                    Canvas.SetLeft(rectangle, canvasLeftTop.X);
                    Canvas.SetTop(rectangle, canvasLeftTop.Y);

                    if (p_DrawElement.Contains(rt.UIElement) == false)
                    {
                        p_DrawElement.Add(rt.UIElement);
                    }
                }
            }

            DrawSearchedCircle();
            DrawExclusivePolygon();

            isRedrawing = false;
        }

        public void UpdateImageViewer()
        {
            foreach (TRect rt in rectList)
            {
                if (p_DrawElement.Contains(rt.UIElement) == true)
                {
                    Rectangle rectangle = rt.UIElement as Rectangle;
                    CPoint canvasLeftTop = GetCanvasPoint(new CPoint(rt.MemoryRect.Left, rt.MemoryRect.Top));
                    CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rt.MemoryRect.Right, rt.MemoryRect.Bottom));

                    rectangle.Width = canvasRightBottom.X - canvasLeftTop.X;
                    rectangle.Height = canvasRightBottom.Y - canvasLeftTop.Y;

                    Canvas.SetLeft(rectangle, canvasLeftTop.X);
                    Canvas.SetTop(rectangle, canvasLeftTop.Y);
                }
            }

            //foreach (TRect rt in boxList)
            //{
            //    if (p_UIElement.Contains(rt.UIElement) == true)
            //    {
            //        Rectangle rectangle = rt.UIElement as Rectangle;
            //        CPoint canvasLeftTop = GetCanvasPoint(new CPoint(rt.MemoryRect.Left, rt.MemoryRect.Top));
            //        CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rt.MemoryRect.Right, rt.MemoryRect.Bottom));

            //        rectangle.Width = Math.Abs(canvasRightBottom.X - canvasLeftTop.X);
            //        rectangle.Height = Math.Abs(canvasRightBottom.Y - canvasLeftTop.Y);

            //        Canvas.SetLeft(rectangle, canvasLeftTop.X);
            //        Canvas.SetTop(rectangle, canvasLeftTop.Y);
            //    }
            //}
        }

        public void RemoveObjectsByTag(string tag)
        {
            p_DrawElement.Clear();

            List<TRect> newRectList = new List<TRect>();

            foreach (TRect rt in rectList)
            {
                if ((string)rt.UIElement.Tag != tag)
                {
                    newRectList.Add(rt);
                    p_DrawElement.Add(rt.UIElement);
                }
            }

            this.rectList = newRectList;
        }

        public void ClearObjects()
        {
            rectList.Clear();
            p_DrawElement.Clear();
        }
        #endregion

        #region [Command]
        public RelayCommand btnOpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._openImage();
                });
            }
        }

        public RelayCommand btnSaveCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._saveImage();
                });
            }
        }

        public RelayCommand btnClearCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._clearImage();
                });
            }
        }
        #endregion
    }
}
