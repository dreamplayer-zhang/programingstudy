using RootTools;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Root_WindII_Option.UI
{
	public class Edgeside_ImageViewer_ViewModel : RootViewer_ViewModel
	{
        List<TRect> rectList;

        public Edgeside_ImageViewer_ViewModel()
        {
            this.p_VisibleMenu = System.Windows.Visibility.Collapsed;

            rectList = new List<TRect>();
        }

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
        #endregion

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
            rt.StrokeThickness = 1;
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

        private void MoveDefectPt(CPoint point)
		{
            if (p_ImageData == null) return;

            CPoint MovePoint = new CPoint();
            MovePoint.X = point.X + p_View_Rect.Width;
        }
	}
}
