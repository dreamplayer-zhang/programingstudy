using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
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
using static Root_CAMELLIA.Module.Module_Camellia;
using Met = Root_CAMELLIA.LibSR_Met;
using System.Threading;

namespace Root_CAMELLIA.LibSR_Met
{
    /// <summary>
    /// CountGraph.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CountGraph : UserControl
    {
        DataManager m_DM = DataManager.GetInstance();
        public DispatcherTimer SpectrumTimer = null;
        (Met.SettingData, Met.Nanoview.ERRORCODE_NANOVIEW) m_SettingDataWithErrorCode;
        public bool m_bCountGraphAutoScale = false;
        private bool m_bDataDetect = false;


        ScottPlot.PlottableScatterHighlight m_CountDataPlotTable;

        Thread m_thread;
        public CountGraph()
        {

            InitializeComponent();
        }


        readonly object lockobj = new object();
        bool m_bThread = false;
        bool m_bUpdate = false;
        void RunThread()
        {
            m_bThread = true;
            while (m_bThread)
            {
                if (m_bUpdate)
                {
                    Thread.Sleep(200);
                    int nNumOfSpectrum = 0;
                    double[] SpectrumData = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    double[] Wavelength = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    if (App.m_nanoView.GetSpectrum(ref SpectrumData, ref Wavelength, ref nNumOfSpectrum) == Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                    {
                        lock (lockobj)
                        {
                            DrawCountGraph("Wavelength [nm]", "Counts", Wavelength, SpectrumData, nNumOfSpectrum);
                        }
                    }
                    else
                    {
                        m_bUpdate = false;
                    }
                }
            }
        }

        public void DrawCountGraph(string xlabel, string ylabel, double[] Wavelength, double[] SpectrumData, int nNumOfSpectrum)
        {
            //그래프 그리기
            Dispatcher.Invoke(new Action(() =>
            {
                GraphCount.Reset();
                GraphCount.plt.XLabel(xlabel);
                GraphCount.plt.YLabel(ylabel);

                double[] SpectrumGraph = new double[nNumOfSpectrum];
                double[] WavelengthGraph = new double[nNumOfSpectrum];

                Array.Copy(SpectrumData, SpectrumGraph, nNumOfSpectrum);
                Array.Copy(Wavelength, WavelengthGraph, nNumOfSpectrum);

                m_CountDataPlotTable = GraphCount.plt.PlotScatterHighlight(WavelengthGraph, SpectrumGraph, markerSize: 2.5);
                //x축 데이터, Y축 데이터
                if (m_bCountGraphAutoScale)
                {
                    GraphCount.plt.AxisAuto();
                }
                else
                {
                    GraphCount.plt.Axis(350, 1500, -10, 65000);
                }
                GraphCount.Render();
                //double dMaxCount =  SpectrumGraph.Max();
                //int nMaxCountIndex = Array.IndexOf(SpectrumGraph, dMaxCount);
                //string sMaxCountWL = Convert.ToString(Math.Round(WavelengthGraph[nMaxCountIndex],0));
                //string sMaxCount = Convert.ToString(Math.Round(dMaxCount,0));
                //labelCount.Visibility = Visibility.Visible;
                //labelCount.Content = $"(Wavelength, Counts) = ({sMaxCountWL:N1}, {sMaxCount:N1})";

                m_bDataDetect = true;
            }));

        }

        private void CountGraph_MouseMove(object sender, MouseEventArgs e)
        {
            // 커서 움직임에 따라 데이터 비율 달라기도록 하기
            if (m_bDataDetect == true)
            {
                var mousePos = e.MouseDevice.GetPosition(GraphCount);
                double mouseX = GraphCount.plt.CoordinateFromPixelX(mousePos.X);
                double mouseY = GraphCount.plt.CoordinateFromPixelY(mousePos.Y);

                m_CountDataPlotTable.HighlightClear();
                var (x, y, index) = m_CountDataPlotTable.HighlightPointNearest(mouseX, mouseY);
                GraphCount.Render();

                labelCount.Visibility = Visibility.Visible;
                labelCount.Content = $"(Wavelength [nm], Counts) = ({x:N1}, {y:N3})";

            }
        }
        private void btnLiveGraph_checkedChanged(object sender, RoutedEventArgs e)
        {
            if (btnLiveSpectrum.IsChecked == true)
            {
                m_bUpdate = true;
                chbAutoScale.IsEnabled = true;

            }
            else
            {
                m_bUpdate = false;
                chbAutoScale.IsEnabled = false;
            }
        }

        private void chbAutoScale_checkedChanged(object sender, RoutedEventArgs e)
        {
            if (chbAutoScale.IsChecked == false)
            {
                m_bCountGraphAutoScale = false;
            }
            else
            {
                m_bCountGraphAutoScale = true;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            m_bThread = false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }
    }
}