using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Camellia2Stage
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class CamelliaStage : UserControl, INotifyPropertyChanged
    {
        public CamelliaStage()
        {
            InitializeComponent();
        }

        double _zoomScale = 1;
        public double ZoomScale
        {
            get
            {
                return _zoomScale;
            }
            set
            {
                _zoomScale = value;
                OnPropertyChanged("ZoomScale");
            }
        }

        double _renderCenterX = 0;
        public double RenderCenterX
        {
            get
            {
                return _renderCenterX;
            }
            set
            {
                _renderCenterX = value;
                OnPropertyChanged("RenderCenterX");
            }
        }

        double _renderCenterY = 0;
        public double RenderCenterY
        {
            get
            {
                return _renderCenterY;
            }
            set
            {
                _renderCenterY = value;
                OnPropertyChanged("RenderCenterY");
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(CanvasScaleTransform.ScaleX == 8)
            {
                return;
            }
            Point pt = e.GetPosition((UIElement)sender);
            CanvasScaleTransform.ScaleX *= 2;
            CanvasScaleTransform.ScaleY *= 2;
            CanvasScaleTransform.CenterX = pt.X;
            CanvasScaleTransform.CenterY = pt.Y;
            //Point pt = e.GetPosition((UIElement)sender);
            //offsetPt.X = 500 - pt.X;
            //offsetPt.Y = 500 - pt.Y;
            //CanvasScaleTransform.ScaleX *= 2;
            //CanvasScaleTransform.ScaleY *= 2;

            ////offsetPt.X *= CanvasScaleTransform.ScaleX;
            //// offsetPt.Y *= CanvasScaleTransform.ScaleX;
            ////CanvasScaleTransform.CenterX = 500 - (offsetPt.X);
            ////CanvasScaleTransform.CenterY = 500 - (offsetPt.Y);
            //CanvasScaleTransform.CenterX = pt.X;
            //CanvasScaleTransform.CenterY = pt.Y;
            //var element = sender as UIElement;
            //var position = e.GetPosition(element);
            //var transform = element.RenderTransform as MatrixTransform;
            //var matrix = transform.Matrix;
            //var scale = nScale * 2;
            //var scale = e.Delta >= 0 ? 1.1 : (1.0 / 1.1); // choose appropriate scaling factor

            //matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
            //transform.Matrix = matrix;
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(CanvasScaleTransform.ScaleX == 1)
            {
                return;
            }
            CanvasScaleTransform.ScaleX /= 2;
            CanvasScaleTransform.ScaleY /= 2;
            //Point pt = e.GetPosition((UIElement)sender);
            //offsetPt.X += 500 - pt.X;
            //offsetPt.Y += 500 - pt.Y;
            //CanvasScaleTransform.ScaleX /= 2;
            //CanvasScaleTransform.ScaleY /= 2;

            //offsetPt.X /= 2;
            //offsetPt.Y /= 2;
            //CanvasScaleTransform.CenterX = 500 - (offsetPt.X);
            //CanvasScaleTransform.CenterY = 500 - (offsetPt.Y);
            //var element = sender as UIElement;
            //var position = e.GetPosition(element);
            //var transform = element.RenderTransform as MatrixTransform;
            //var matrix = transform.Matrix;
            //var scale = nScale / 2;
            ////var scale = e.Delta >= 0 ? 1.1 : (1.0 / 1.1); // choose appropriate scaling factor

            //matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
            //transform.Matrix = matrix;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}