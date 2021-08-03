using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
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
            public static int BoxTickness = 1;
        }

        public class ColorDefines
        {
            public static SolidColorBrush Box = Brushes.Yellow;
        }


        bool isBoxDrawing = false;
        List<TRect> boxList;
        List<TRect> rectList;

        public FrontsideInspect_ImageViewer_ViewModel()
        {
            this.p_VisibleMenu = System.Windows.Visibility.Collapsed;

            rectList = new List<TRect>();
            boxList = new List<TRect>();
        }

        #region [Properties]
        private bool isColorChecked = true;
        public bool IsColorChecked
        {
            get => this.isColorChecked;
            set
            {
                if(value == true)
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

        private bool isBoxChecked = false;
        public bool IsBoxChecked
        {
            get => this.isBoxChecked;
            set
            {
                if (value == true)
                {
                    this.IsRularChecked = false;
                }
                SetProperty<bool>(ref this.isBoxChecked, value);
            }
        }

        private bool isRularChecked = false;
        public bool IsRularChecked
        {
            get => this.isRularChecked;
            set
            {
                if (value == true)
                {
                    this.IsBoxChecked = false;
                }
                SetProperty<bool>(ref this.isRularChecked, value);
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

        public RelayCommand btnToolClearCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.ClearUIElement();
                });
            }
        }
        #endregion

        #region [Overrides]

        private CPoint boxFirstPoint = new CPoint();

        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);

            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;

            if (this.isBoxChecked == true )
            {
                if(this.isBoxDrawing == false)
                {
                    this.boxFirstPoint = new CPoint(p_MouseMemX, p_MouseMemY);

                    CPoint canvasLeftTop = GetCanvasPoint(new CPoint(p_MouseMemX, p_MouseMemY));
                    CPoint canvasRightBottom = GetCanvasPoint(new CPoint(p_MouseMemX, p_MouseMemY));

                    Rectangle rt = new Rectangle();
                    rt.Width = canvasRightBottom.X - canvasLeftTop.X;
                    rt.Height = canvasRightBottom.Y - canvasLeftTop.Y;

                    rt.Stroke = ColorDefines.Box;
                    rt.StrokeThickness = DrawDefines.BoxTickness;
                    rt.Opacity = 1;
                    rt.Fill = Brushes.Transparent;
                    rt.Tag = "box";

                    Canvas.SetLeft(rt, canvasLeftTop.X);
                    Canvas.SetTop(rt, canvasLeftTop.Y);

                    TRect tRect = new TRect();
                    tRect.UIElement = rt;
                    tRect.MemoryRect.Left = p_MouseMemX;
                    tRect.MemoryRect.Top = p_MouseMemY;
                    tRect.MemoryRect.Right = p_MouseMemX;
                    tRect.MemoryRect.Bottom = p_MouseMemY;

                    boxList.Add(tRect);
                    p_UIElement.Add(rt);
                    this.isBoxDrawing = true;
                }
                else
                {
                    this.isBoxDrawing = false;

                    TRect tRect = boxList[boxList.Count - 1];
                    if (this.boxFirstPoint.Y > p_MouseMemY)
                    {
                        tRect.MemoryRect.Top = p_MouseMemY;
                        tRect.MemoryRect.Bottom = this.boxFirstPoint.Y;
                    }
                    else
                    {
                        tRect.MemoryRect.Bottom = p_MouseMemY;
                        tRect.MemoryRect.Top = this.boxFirstPoint.Y;
                    }

                    CPoint canvasLeftTop = GetCanvasPoint(new CPoint(tRect.MemoryRect.Left, tRect.MemoryRect.Top));
                    CPoint canvasRightBottom = GetCanvasPoint(new CPoint(tRect.MemoryRect.Right, tRect.MemoryRect.Bottom));

                    Rectangle rt = tRect.UIElement as Rectangle;
                    rt.Width = Math.Abs(canvasRightBottom.X - canvasLeftTop.X);
                    rt.Height = Math.Abs(canvasRightBottom.Y - canvasLeftTop.Y);

                    Canvas.SetLeft(rt, canvasLeftTop.X < canvasRightBottom.X ? canvasLeftTop.X : canvasRightBottom.X);
                    Canvas.SetTop(rt, canvasLeftTop.Y < canvasRightBottom.Y ? canvasLeftTop.Y : canvasRightBottom.Y);

                    
                    IntPtr ptrR = this.p_ImageData.GetPtr(0);
                    IntPtr ptrG = this.p_ImageData.GetPtr(1);
                    IntPtr ptrB = this.p_ImageData.GetPtr(2);

                    long avrR = 0;
                    long avrG = 0;
                    long avrB = 0;

                    for(int i = tRect.MemoryRect.Top; i < tRect.MemoryRect.Bottom; i++ )
                    {
                        for(int j = tRect.MemoryRect.Left; j < tRect.MemoryRect.Right; j++)
                        {
                            byte byteR = Marshal.ReadByte(ptrR + i * this.p_ImageData.p_Size.X + j);
                            byte byteG = Marshal.ReadByte(ptrG + i * this.p_ImageData.p_Size.X + j);
                            byte byteB = Marshal.ReadByte(ptrB + i * this.p_ImageData.p_Size.X + j);

                            avrR += (long)byteR;
                            avrG += (long)byteG;
                            avrB += (long)byteB;
                        }
                    }

                    avrR /= (tRect.MemoryRect.Width * tRect.MemoryRect.Height);
                    avrG /= (tRect.MemoryRect.Width * tRect.MemoryRect.Height);
                    avrB /= (tRect.MemoryRect.Width * tRect.MemoryRect.Height);

                    rt.ToolTip = string.Format(
                        "W: {0}  H: {1}\n" +
                        "Avg R : {2}\n" +
                        "Avg G : {3}\n" +
                        "Avg B : {4}", 
                        tRect.MemoryRect.Width, tRect.MemoryRect.Height,
                        avrR, avrG, avrB);

                    this.IsBoxChecked = false;
                }
            }

            RedrawShapes();
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);

            if(this.isBoxChecked == true && this.isBoxDrawing == true)
            {
                TRect tRect = boxList[boxList.Count - 1];
                if (this.boxFirstPoint.X > p_MouseMemX)
                {
                    tRect.MemoryRect.Left = p_MouseMemX;
                    tRect.MemoryRect.Right = this.boxFirstPoint.X;
                }
                else
                {
                    tRect.MemoryRect.Right = p_MouseMemX;
                    tRect.MemoryRect.Left = this.boxFirstPoint.X;
                }

                if (this.boxFirstPoint.Y > p_MouseMemY)
                {
                    tRect.MemoryRect.Top = p_MouseMemY;
                    tRect.MemoryRect.Bottom = this.boxFirstPoint.Y;
                }
                else
                {
                    tRect.MemoryRect.Bottom = p_MouseMemY;
                    tRect.MemoryRect.Top = this.boxFirstPoint.Y;
                }

                CPoint canvasLeftTop = GetCanvasPoint(new CPoint(tRect.MemoryRect.Left, tRect.MemoryRect.Top));
                CPoint canvasRightBottom = GetCanvasPoint(new CPoint(tRect.MemoryRect.Right, tRect.MemoryRect.Bottom));

                Rectangle rt = tRect.UIElement as Rectangle;
                rt.Width = Math.Abs(canvasRightBottom.X - canvasLeftTop.X);
                rt.Height = Math.Abs(canvasRightBottom.Y - canvasLeftTop.Y);

                Canvas.SetLeft(rt, canvasLeftTop.X < canvasRightBottom.X ? canvasLeftTop.X : canvasRightBottom.X);
                Canvas.SetTop(rt, canvasLeftTop.Y < canvasRightBottom.Y ? canvasLeftTop.Y : canvasRightBottom.Y);


                
            }
            //if(m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
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

        public void AddDrawRect(CRect rect, SolidColorBrush color = null, string tag = "")
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

            if(IsDefectChecked == true)
                p_DrawElement.Add(rt);
        }

        public void AddDrawRectList(List<CRect> rectList, SolidColorBrush color = null, string tag = "")
        {
            foreach(CRect rect in rectList)
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

            if(IsDefectChecked == true)
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

            if(IsDefectChecked == true)
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

            if (IsDefectChecked == true)
                p_DrawElement.Add(grid);
        }

        bool InViewRect(TRect rt)
        {
            return true;

            if (p_View_Rect.Contains(rt.MemoryRect.Left, rt.MemoryRect.Top))
            {
                return true;
            }
            return false;
        }

        bool isRedrawing = false;
        private void RedrawShapes()
        {
            if (isRedrawing)
                return;
            isRedrawing = true;
            if (this.IsDefectChecked)
            {
                foreach (TRect rt in rectList)
                {
                    if (InViewRect(rt) && p_DrawElement.Contains(rt.UIElement) == true)
                    {
                        rt.UIElement.Visibility = Visibility.Visible;
                        Rectangle rectangle = rt.UIElement as Rectangle;
                        CPoint canvasLeftTop = GetCanvasPoint(new CPoint(rt.MemoryRect.Left, rt.MemoryRect.Top));
                        CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rt.MemoryRect.Right, rt.MemoryRect.Bottom));

                        rectangle.Width = canvasRightBottom.X - canvasLeftTop.X;
                        rectangle.Height = canvasRightBottom.Y - canvasLeftTop.Y;

                        Canvas.SetLeft(rectangle, canvasLeftTop.X);
                        Canvas.SetTop(rectangle, canvasLeftTop.Y);
                    }
                    else
                    {
                        rt.UIElement.Visibility = Visibility.Hidden;
                    }
                }
            }
            foreach (TRect rt in boxList)
            {
                if (InViewRect(rt) && p_UIElement.Contains(rt.UIElement) == true)
                {
                    Rectangle rectangle = rt.UIElement as Rectangle;
                    CPoint canvasLeftTop = GetCanvasPoint(new CPoint(rt.MemoryRect.Left, rt.MemoryRect.Top));
                    CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rt.MemoryRect.Right, rt.MemoryRect.Bottom));

                    rectangle.Width = Math.Abs(canvasRightBottom.X - canvasLeftTop.X);
                    rectangle.Height = Math.Abs(canvasRightBottom.Y - canvasLeftTop.Y);

                    Canvas.SetLeft(rectangle, canvasLeftTop.X);
                    Canvas.SetTop(rectangle, canvasLeftTop.Y);
                }
            }
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

            foreach (TRect rt in boxList)
            {
                if (p_UIElement.Contains(rt.UIElement) == true)
                {
                    Rectangle rectangle = rt.UIElement as Rectangle;
                    CPoint canvasLeftTop = GetCanvasPoint(new CPoint(rt.MemoryRect.Left, rt.MemoryRect.Top));
                    CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rt.MemoryRect.Right, rt.MemoryRect.Bottom));

                    rectangle.Width = Math.Abs(canvasRightBottom.X - canvasLeftTop.X);
                    rectangle.Height = Math.Abs(canvasRightBottom.Y - canvasLeftTop.Y);

                    Canvas.SetLeft(rectangle, canvasLeftTop.X);
                    Canvas.SetTop(rectangle, canvasLeftTop.Y);
                }
            }
        }

        public void RemoveObjectsByTag(string tag)
        {
            p_DrawElement.Clear();

            List<TRect> newRectList = new List<TRect>();

            foreach (TRect rt in rectList)
            {
                if((string)rt.UIElement.Tag != tag)
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

    }
}
