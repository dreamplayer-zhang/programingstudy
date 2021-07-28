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

namespace RootTools
{
    /// <summary>
    /// RootViewer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RootViewer : UserControl
    {
        public RootViewer()
        {
            InitializeComponent();
        }

        //public List<Rect> rectList = new List<Rect>();

        //protected override void OnRender(DrawingContext drawingContext)
        //{
        //    base.OnRender(drawingContext);

        //    if (this.rectList.Count == 0) return;
        //    double width = mainGrid.ActualWidth;
        //    double height = mainGrid.ActualHeight;

        //    double rectWidth =  width / 1000;
        //    double rectHeight = height / 100;


        //    for (int i = 0; i < 100; i++)
        //        for (int j = 0; j < 1000; j++)
        //        {
        //            drawingContext.DrawRectangle(Brushes.Red, new Pen(Brushes.Blue, 2),
        //             new Rect(j * rectWidth, i * rectHeight, rectWidth, rectHeight));
        //        }

        //    //foreach (Rect rect in this.rectList)
        //    //{
        //    //    double ratioX = width / 100;
        //    //    double ratioY = height / 100;

        //    //    drawingContext.DrawRectangle(Brushes.Red, new Pen(Brushes.Blue, 2),
        //    //        new Rect(rect.Left * ratioX, rect.Top * ratioY, rectWidth, rectHeight));
        //    //}
        //}
    }
}
