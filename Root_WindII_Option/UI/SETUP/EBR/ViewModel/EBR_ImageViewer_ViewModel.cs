using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Root_WindII_Option.UI
{
    public class EBR_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        List<TRect> rectList;

        public EBR_ImageViewer_ViewModel()
        {
            this.p_VisibleMenu = System.Windows.Visibility.Collapsed;

            rectList = new List<TRect>();
        }

        #region [Command]
        public RelayCommand btnOpen
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._openImage();
                });
            }
        }

        public RelayCommand btnSave
        {
            get
            {
                return new RelayCommand(() =>
                {
                    
                    this._saveImage();
                });
            }
        }

        public RelayCommand btnClear
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._clearImage();
                });
            }
        }

        public RelayCommand btnClearDefect
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ClearObjects();
                });
            }
        }
        #endregion

        #region [Overrides]
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
        }

        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            RedrawShapes();
        }

        public override void PreviewMouseUp(object sender, MouseEventArgs e)
        {
            base.PreviewMouseUp(sender, e);
        }

        public override void SetRoiRect()
        {
            base.SetRoiRect();
            RedrawShapes();
        }

		public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
		{
			base.CanvasMovePoint_Ref(point, nX, nY);
			RedrawShapes();
		}
		#endregion

		public void ClearObjects()
        {
            rectList.Clear();
            p_DrawElement.Clear();
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
        }

        private void RedrawShapes()
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
        }

        public void AddDrawRectList(List<CRect> rectList, SolidColorBrush color = null)
        {
            foreach (CRect rect in rectList)
            {
                AddDrawRect(rect, color);
            }
        }

        private void AddDrawRect(CRect rect, SolidColorBrush color = null)
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
            rt.StrokeThickness = 1;
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
    }
}
