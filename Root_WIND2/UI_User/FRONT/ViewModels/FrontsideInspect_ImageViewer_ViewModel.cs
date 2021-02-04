using RootTools;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Root_WIND2.UI_User
{
    public class FrontsideInspect_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        public class DrawDefines
        {
            public static int RectTickness = 4;
        }


        List<TRect> rectList;

        public FrontsideInspect_ImageViewer_ViewModel()
        {
            this.p_VisibleMenu = System.Windows.Visibility.Collapsed;

            rectList = new List<TRect>();
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

        #endregion

        #region [Draw Method]

        public void AddDrawRect(CRect rect, SolidColorBrush color = null)
        {
            if(color == null)
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

        public void AddDrawRectList(List<CRect> rectList, SolidColorBrush color = null)
        {
            foreach(CRect rect in rectList)
            {
                AddDrawRect(rect, color);
            }
        }

        public void AddDrawRect(CPoint leftTop, CPoint rightBottom, SolidColorBrush color = null)
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

            Canvas.SetLeft(rt, canvasLeftTop.X);
            Canvas.SetTop(rt, canvasLeftTop.Y);

            TRect tRect = new TRect();
            tRect.UIElement = rt;
            tRect.MemoryRect.Left = leftTop.X;
            tRect.MemoryRect.Top = leftTop.Y;
            tRect.MemoryRect.Right = rightBottom.X;
            tRect.MemoryRect.Bottom = rightBottom.Y;

            rectList.Add(tRect);

            p_DrawElement.Add(rt);
        }


        public void AddDrawText(CRect rect, string text, SolidColorBrush color = null)
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

        private void RedrawShapes()
        {
            foreach(TRect rt in rectList)
            {
                if(p_DrawElement.Contains(rt.UIElement) == true)
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
        }

        public void ClearObjects()
        {
            rectList.Clear();
            p_DrawElement.Clear();
        }
        #endregion

    }
}
