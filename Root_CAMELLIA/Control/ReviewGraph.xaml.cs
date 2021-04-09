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

namespace Root_CAMELLIA
{
    /// <summary>
    /// Review_RGraph.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ReviewGraph : UserControl
    {
        ScottPlot.PlottableScatterHighlight m_ReviewPlotTable;
        public bool m_bDataDetect = false;
        public ReviewGraph()
        {
            InitializeComponent();
        }

        public void DrawReviewGraph( string xlabel, string ylabel, double[] xvalus, double[] yvalues)
        {
            ReviewGraphDraw.Reset();
            ReviewGraphDraw.plt.XLabel(xlabel);
            ReviewGraphDraw.plt.YLabel(ylabel);
            double dStartWL = xvalus[0];
            double dStopWL = xvalus[xvalus.Length - 1];
            m_ReviewPlotTable = ReviewGraphDraw.plt.PlotScatterHighlight(xvalus, yvalues, markerSize: 2.5);

            ReviewGraphDraw.plt.Axis(dStartWL, dStopWL, -10, 100);
            ReviewGraphDraw.Render();

            m_bDataDetect = true;

        }

        public void ReviewGraphReset()
        {
            ReviewGraphDraw.Reset();
        }

        private void ReviewGraph_MouseMove(object sender, MouseEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (m_bDataDetect == true)
                {
                    var mousePos = e.MouseDevice.GetPosition(ReviewGraphDraw);
                    double mouseX = ReviewGraphDraw.plt.CoordinateFromPixelX(mousePos.X);
                    double mouseY = ReviewGraphDraw.plt.CoordinateFromPixelY(mousePos.Y);

                    m_ReviewPlotTable.HighlightClear();
                    var (dX, dY, nIndex) = m_ReviewPlotTable.HighlightPointNearest(mouseX, mouseY);
                    ReviewGraphDraw.Render();

                    labelReview.Visibility = Visibility.Visible;
                    labelReview.Content = $"({dX:N1}, {dY:N3})";
                }
            }));
        }

    }
}
