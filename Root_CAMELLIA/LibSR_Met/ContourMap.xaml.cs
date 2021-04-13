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
using ScottPlot;
using ChartDirector;

namespace Root_CAMELLIA.LibSR_Met
{
    /// <summary>
    /// ContourMap.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContourMap : UserControl
    {
        DataManager m_DM = DataManager.GetInstance();

        public ContourMap()
        {
            InitializeComponent();

            Dispatcher.Invoke(new Action(() =>
            {
                radioR.IsChecked = true;
                comboWL.SelectedIndex = 0;
                if (m_DM.bTransmittance)
                {
                    radioT.IsEnabled = true;

                }
                else
                {
                    radioT.Visibility = Visibility.Hidden;
                }
            }));
        }

        public void InitializeContourMap()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                radioR.IsChecked = true;
            }));
        }

        public void DrawAllDatas()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                double dSelectedWL = Convert.ToDouble(comboWL.SelectedItem);   //wavelength
                List<ContourMapData> mapData = new List<ContourMapData>();

                if (m_DM == null)
                {
                    return;
                }

                if (m_DM.m_ContourMapDataR.Count == 0 || m_DM.m_ContourMapDataT.Count == 0)
                {
                    return;
                }

                //해당 wavelength 찾기
                int nContourDataIdx = 0;   //wavelength

                FitValueType type = FitValueType.Reflectance;
                if (radioR.IsChecked == true)
                {
                    type = FitValueType.Reflectance;
                    mapData = m_DM.m_ContourMapDataR;
                }
                else if (radioT.IsChecked == true)
                {
                    type = FitValueType.Transmittance;
                    mapData = m_DM.m_ContourMapDataT;
                }

                for (int n = 0; n < mapData.Count; n++)
                {
                    if (mapData[n].Wavelength == dSelectedWL)
                    {
                        nContourDataIdx = n;
                    }
                }

                DrawContourMap(type, nContourDataIdx);
                DrawHistogram(type, nContourDataIdx);
            }));
        }

        private void DrawContourMap(FitValueType valueType, int nContourDataIndex)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                XYChart xyChart = new XYChart((int)gridChart.ActualWidth, (int)gridChart.ActualHeight);

                List<ContourMapData> mapData = new List<ContourMapData>();
                if (valueType == FitValueType.Reflectance)
                    mapData = m_DM.m_ContourMapDataR;
                else
                    mapData = m_DM.m_ContourMapDataT; 

                if (mapData.Count <= nContourDataIndex)
                {
                    m_DM.m_Log.WriteLog(LogType.Error, "Exceeded Contourmapdata Index.");
                    return;
                }

                ContourMapData data = mapData[nContourDataIndex];

                //xyChart.setPlotArea(70, 40, 300, 300, -1, -1, -1, unchecked((int)0xc0000000), -1);
                xyChart.setPlotArea(40, 40, (int)(gridChart.ActualWidth * 0.75), (int)(gridChart.ActualHeight * 0.75), -1, -1, -1, unchecked((int)0xc0000000), -1);

                ContourLayer layer = xyChart.addContourLayer(data.HoleData.Select(s => s.XPos).ToArray(), data.HoleData.Select(s => s.YPos).ToArray(), data.HoleData.Select(s => s.Value).ToArray());

                // Set the x-axis and y-axis scale

                xyChart.xAxis().setLinearScale(-150, 150, 50);
                xyChart.yAxis().setLinearScale(-150, 150, 50);

                //xyChart.addTitle("Spline Surface - Continuous Coloring", "Arial Bold Italic", 12);
                layer.setSmoothInterpolation(true);
                layer.setContourColor(Chart.Transparent);
                layer.colorAxis().setColorGradient(true);

                //ColorAxis cAxis = layer.setColorAxis(505, 40, Chart.TopLeft, 400, Chart.Right);
                ColorAxis cAxis = layer.setColorAxis((int)(gridChart.ActualWidth * 0.8), 30, Chart.TopLeft, (int)(gridChart.ActualHeight * 0.8), Chart.Right);

                // Add a title to the color axis using 12 points Arial Bold Italic font
                cAxis.setTitle(valueType.ToString() + " [%]", "Arial Bold", 12);

                // Set color axis labels to use Arial Bold font
                cAxis.setLabelStyle("Arial Bold");

                // Output the chart
                ContourMapViewer.Chart = xyChart;
            }));
        }

        public void DrawHistogram(FitValueType valueType, int nContourDataIndex)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                graphHisto.plt.Clear();

                List<ContourMapData> mapData = new List<ContourMapData>();
                if (valueType == FitValueType.Reflectance)
                    mapData = m_DM.m_ContourMapDataR;
                else
                    mapData = m_DM.m_ContourMapDataT;

                List<double> measuredValue = new List<double>();
                double dMin = 9999.0;
                double dMax = -9999.0;
                int nMinMaxOffset = 5;

                ContourMapData data = mapData[nContourDataIndex];

                for (int n = 0; n < data.HoleData.Count; n++)
                {
                    double dValue = data.HoleData[n].Value;
                    measuredValue.Add(dValue);
                    if (dValue < dMin)
                    {
                        dMin = dValue;
                    }
                    if (dValue > dMax)
                    {
                        dMax = dValue;
                    }
                }

                double dWavelength = Convert.ToDouble(comboWL.SelectedItem);
                int nUnit = 1;  //고정할것이다!!!
                int nXDataNum = (int)(100 / nUnit);
                double[] Xs = DataGen.Consecutive(nXDataNum);
                double[] Ys = new double[nXDataNum];
                Ys.Initialize();

                for (int i = 0; i < measuredValue.Count; i++)
                {
                    int nX = (int)Math.Round(measuredValue[i]);
                    if (nX >= nXDataNum)
                    {
                        nX = nXDataNum - 1;
                    }
                    Ys[nX]++;
                }

                //double[] xs = DataGen.Consecutive(nXDataNum);
                //double[] ys = new double[] { 27, 23, 21, 16, 6, 19, 18, 17, 20, 13};

                Ys = Tools.Round(Ys, 3);

                // add both bar plot
                graphHisto.plt.PlotBar(Xs, Ys, showValues: false);

                // customize the plot to make it look nicer
                graphHisto.plt.Axis(y1: 0);
                graphHisto.plt.Grid(enableVertical: false, lineStyle: LineStyle.Dot);
                graphHisto.plt.Axis(y1: 0);
                graphHisto.plt.Legend();

                graphHisto.plt.XLabel(valueType.ToString() + " [%]");
                graphHisto.plt.YLabel("Points");

                graphHisto.Render();
            }));
        }

        private void comboWL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboWL != null && comboWL.Items != null)
            {
                DrawAllDatas();
            }
        }

        private void comboWL_DropDownOpened(object sender, EventArgs e)
        {

        }

        private void radiobutton_Checked(object sender, RoutedEventArgs e)
        {
            List<double> listWavelength;
            if (radioR.IsChecked == true)
                listWavelength = m_DM.m_ScalesListR.Select(s => s.p_waveLength).ToList();
            else
                listWavelength = m_DM.m_ScalesListT.Select(s => s.p_waveLength).ToList();

            comboWL.Items.Clear();
            foreach (double wavelength in listWavelength)
            {
                comboWL.Items.Add(wavelength);
            }
            if (comboWL.Items.Count != 0)
            {
                comboWL.SelectedIndex = 0;
                DrawAllDatas();
            }
        }

        private void ContourMapViewer_ViewPortChanged(object sender, ChartDirector.WPFViewPortEventArgs e)
        {
            //if (e.NeedUpdateChart)
            //drawChart();
        }

        private void gridChart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawAllDatas();
        }
    }
}
