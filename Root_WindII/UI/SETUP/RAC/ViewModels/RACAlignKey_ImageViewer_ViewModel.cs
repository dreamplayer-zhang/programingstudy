using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Root_WindII
{
    public class RACAlignKey_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        private class ColorDefines
        {
            public static SolidColorBrush Box = Brushes.Yellow;
        }

        public RACAlignKey_ImageViewer_ViewModel()
        {
            this.p_VisibleMenu = Visibility.Collapsed;

            Initialize();
        }

        public void OnUpdateImage(Object sender, EventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.SetImageSource();
            });
        }

        #region [Properties]
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
        #endregion


        #region [Command]
        public RelayCommand btnToolClearCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.ClearViewElement();
                });
            }
        }
        #endregion

        #region [Draw]


        private CPoint boxFirstPoint = new CPoint();
        bool isBoxDrawing = false;
        CRect boxMemRect;
        Rectangle BoxUI;

        public void Initialize()
        {
            boxMemRect = new CRect();
            BoxUI = new Rectangle();
            BoxUI.Stroke = ColorDefines.Box;
            BoxUI.StrokeThickness = 3;
            BoxUI.Opacity = 1;
            BoxUI.Fill = Brushes.Transparent;
            BoxUI.Tag = "box";
        }


        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);

            if (this.p_ImageData == null)
                return;

            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;

            if (this.isBoxChecked == true)
            {
                if (this.isBoxDrawing == false)
                {
                    this.boxFirstPoint = new CPoint(p_MouseMemX, p_MouseMemY);

                    CPoint canvasLeftTop = GetCanvasPoint(new CPoint(p_MouseMemX, p_MouseMemY));
                    CPoint canvasRightBottom = GetCanvasPoint(new CPoint(p_MouseMemX, p_MouseMemY));

                    BoxUI.Width = canvasRightBottom.X - canvasLeftTop.X;
                    BoxUI.Height = canvasRightBottom.Y - canvasLeftTop.Y;

                    Canvas.SetLeft(BoxUI, canvasLeftTop.X);
                    Canvas.SetTop(BoxUI, canvasLeftTop.Y);

                    this.boxMemRect.Left = p_MouseMemX;
                    this.boxMemRect.Right = p_MouseMemX;
                    this.boxMemRect.Top = p_MouseMemY;
                    this.boxMemRect.Bottom = p_MouseMemY;

                    if (!this.p_ViewElement.Contains(BoxUI))
                    {
                        this.p_ViewElement.Add(BoxUI);
                    }

                    this.isBoxDrawing = true;
                }
                else
                {
                    this.isBoxDrawing = false;

                    if (this.boxFirstPoint.Y > p_MouseMemY)
                    {
                        this.boxMemRect.Top = p_MouseMemY;
                        this.boxMemRect.Bottom = this.boxFirstPoint.Y;
                    }
                    else
                    {
                        this.boxMemRect.Bottom = p_MouseMemY;
                        this.boxMemRect.Top = this.boxFirstPoint.Y;
                    }

                    CPoint canvasLeftTop = GetCanvasPoint(new CPoint(boxMemRect.Left, boxMemRect.Top));
                    CPoint canvasRightBottom = GetCanvasPoint(new CPoint(boxMemRect.Right, boxMemRect.Bottom));

                    BoxUI.Width = Math.Abs(canvasRightBottom.X - canvasLeftTop.X);
                    BoxUI.Height = Math.Abs(canvasRightBottom.Y - canvasLeftTop.Y);

                    Canvas.SetLeft(BoxUI, canvasLeftTop.X < canvasRightBottom.X ? canvasLeftTop.X : canvasRightBottom.X);
                    Canvas.SetTop(BoxUI, canvasLeftTop.Y < canvasRightBottom.Y ? canvasLeftTop.Y : canvasRightBottom.Y);

                    BoxUI.ToolTip = string.Format("W: {0}  H: {1}", boxMemRect.Width, boxMemRect.Height);

                    FeatureBoxDone();

                    this.IsBoxChecked = false;
                }
            }

            RedrawShapes();
        }

        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);

            if (this.isBoxChecked == true && this.isBoxDrawing == true)
            {
                if (this.boxFirstPoint.X > p_MouseMemX)
                {
                    boxMemRect.Left = p_MouseMemX;
                    boxMemRect.Right = this.boxFirstPoint.X;
                }
                else
                {
                    boxMemRect.Right = p_MouseMemX;
                    boxMemRect.Left = this.boxFirstPoint.X;
                }

                if (this.boxFirstPoint.Y > p_MouseMemY)
                {
                    boxMemRect.Top = p_MouseMemY;
                    boxMemRect.Bottom = this.boxFirstPoint.Y;
                }
                else
                {
                    boxMemRect.Bottom = p_MouseMemY;
                    boxMemRect.Top = this.boxFirstPoint.Y;
                }

                CPoint canvasLeftTop = GetCanvasPoint(new CPoint(boxMemRect.Left, boxMemRect.Top));
                CPoint canvasRightBottom = GetCanvasPoint(new CPoint(boxMemRect.Right, boxMemRect.Bottom));


                BoxUI.Width = Math.Abs(canvasRightBottom.X - canvasLeftTop.X);
                BoxUI.Height = Math.Abs(canvasRightBottom.Y - canvasLeftTop.Y);

                Canvas.SetLeft(BoxUI, canvasLeftTop.X < canvasRightBottom.X ? canvasLeftTop.X : canvasRightBottom.X);
                Canvas.SetTop(BoxUI, canvasLeftTop.Y < canvasRightBottom.Y ? canvasLeftTop.Y : canvasRightBottom.Y);
            }

            RedrawShapes();
        }

        private void RedrawShapes()
        {
            if (this.p_ViewElement.Contains(BoxUI) == true)
            {
                CPoint canvasLeftTop = GetCanvasPoint(new CPoint(boxMemRect.Left, boxMemRect.Top));
                CPoint canvasRightBottom = GetCanvasPoint(new CPoint(boxMemRect.Right, boxMemRect.Bottom));

                BoxUI.Width = Math.Abs(canvasRightBottom.X - canvasLeftTop.X);
                BoxUI.Height = Math.Abs(canvasRightBottom.Y - canvasLeftTop.Y);

                Canvas.SetLeft(BoxUI, canvasLeftTop.X);
                Canvas.SetTop(BoxUI, canvasLeftTop.Y);
            }
        }

        #endregion

        public ImageData BoxImage
        {
            get;
            set;
        }

        private void FeatureBoxDone()
        {
            int byteCnt = p_ImageData.GetBytePerPixel();

            BoxImage = new ImageData(boxMemRect.Width, boxMemRect.Height, byteCnt);

            BoxImage.m_eMode = ImageData.eMode.ImageBuffer;
            BoxImage.SetData(p_ImageData
                , new CRect(boxMemRect.Left, boxMemRect.Top, boxMemRect.Right, boxMemRect.Bottom)
                , (int)p_ImageData.p_Stride, byteCnt);
        }
    }
}
