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

        public void DrawReviewGraph(string Title, string xlabel, string ylabel, double[] xvalus, double[] yvalues)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                ReviewGraphDraw.Reset();
                ReviewGraphDraw.plt.XLabel(xlabel, null, null, null, 15, null);
                ReviewGraphDraw.plt.YLabel(ylabel, null, null, 15, null, null);
                ReviewGraphDraw.plt.Title(Title);
                double dStartWL = xvalus[0];
                double dStopWL = xvalus[xvalus.Length-1];
                m_ReviewPlotTable = ReviewGraphDraw.plt.PlotScatterHighlight(xvalus, yvalues, markerSize: 2.5);
                //ReviewGraphDraw.plt.AutoScale();
                ReviewGraphDraw.plt.Axis(dStartWL, dStopWL,-0.1, 0.1);
                ReviewGraphDraw.Render();

                m_bDataDetect = true;
            }));

        }

        public void ReviewGraphReset()
        {
            ReviewGraphDraw.Reset();
        }

       

    }
}
