using System;
using System.Collections.Generic;
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
    public partial class CamelliaStage : UserControl
    {
        public CamelliaStage()
        {
            InitializeComponent();
        }

        Point offsetPt = new Point();
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition((UIElement)sender);
            offsetPt.X += 500 - pt.X;
            offsetPt.Y += 500 - pt.Y;
            CanvasScaleTransform.ScaleX *= 2;
            CanvasScaleTransform.ScaleY *= 2;

            //offsetPt.X *= 2;
            //offsetPt.Y *= 2;
            CanvasScaleTransform.CenterX = (offsetPt.X);
            CanvasScaleTransform.CenterY = (offsetPt.Y);
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition((UIElement)sender);
            CanvasScaleTransform.ScaleX /= 2;
            CanvasScaleTransform.ScaleY /= 2;
            //CanvasScaleTransform.CenterX = 500 +pt.X;
            //CanvasScaleTransform.CenterY = 500 - pt.Y;
        }
    }
}