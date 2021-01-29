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
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Threading;


namespace RootTools.Camera.BaslerPylon
{
    /// <summary>
    /// ChannelGraph.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChannelGraph : UserControl
    {
        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> YFormatter { get; set; }

        DispatcherTimer m_timer = new DispatcherTimer();
        int m_timerTickTime = 300;

        ImageData m_img;

        public ChannelGraph()
        {
            InitializeComponent();

            // RGB Series Setting
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Red",
                    Values = new ChartValues<double> { 0, },
                    PointGeometry = null,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Red,
                    StrokeThickness = 0.5,
                    LineSmoothness = 0,
                },
                new LineSeries
                {
                    Title = "Green",
                    Values = new ChartValues<double> { 0, },
                    PointGeometry = null,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Green,
                    StrokeThickness = 0.5,
                    LineSmoothness = 0,
                },
                new LineSeries
                {
                    Title = "Blue",
                    Values = new ChartValues<double> { 0, },
                    PointGeometry = null,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 0.5,
                    LineSmoothness = 0,
                }
            };

            // Y Axis Label Formatter
            YFormatter = value => (value*100) + "%";

            // Update timer Start (freq 500ms)
            m_timer.Interval = TimeSpan.FromMilliseconds(m_timerTickTime);
            m_timer.Tick += new EventHandler(UpdateGraphTimer);

            DataContext = this;
        }

        private void UpdateGraphTimer(object obj, EventArgs e)
        {
            UpdateGraph();
        }

        void UpdateGraph()
        {
            // RGB histogram data array
            int[][] arrRGB = CreateHistogram();
            int imgSize = (m_img.p_Size.X * m_img.p_Size.Y) / m_img.p_nByte;

            // Set histogram data to apply
            for (int rgb = 0; rgb < 3; rgb++)
            {
                SeriesCollection[rgb].Values.Clear();

                for (int i = 0; i < 256; i++)
                {
                    SeriesCollection[rgb].Values.Add((double)arrRGB[rgb][i] / imgSize);
                }
            }
        }

        int[][] CreateHistogram()
        {
            // Create RGB array
            int[][] arrRGB = new int[3][]
            {
                new int[256],
                new int[256],
                new int[256]
            };

            // Orgarnize histogram
            for(int row = 0; row < m_img.p_Size.Y; row++)
            {
                for(int col = 0; col < m_img.p_Size.X; col++)
                {
                    int idx = ((m_img.p_Size.Y * row) + col) * m_img.p_nByte;

                    byte r = m_img.m_aBuf[idx + 2];
                    byte g = m_img.m_aBuf[idx + 1];
                    byte b = m_img.m_aBuf[idx + 0];

                    arrRGB[0][r]++;
                    arrRGB[1][g]++;
                    arrRGB[2][b]++;
                }
            }

            return arrRGB;
        }

        public void SetTickTime(int nTickTime)
        {
            m_timerTickTime = nTickTime;
        }

        public void SetImage(ImageData img)
        {
            m_img = img;
        }

        public void ShowRGB(int idxRGB)
        {
            LineSeries ls = (LineSeries)SeriesCollection[idxRGB];
            ls.Visibility = Visibility.Visible;
        }

        public void HideRGB(int idxRGB)
        {
            LineSeries ls = (LineSeries)SeriesCollection[idxRGB];
            ls.Visibility = Visibility.Hidden;
        }

        public void StartUpdateGraph()
        {
            m_timer.Start();
        }

        public void StopUpdateGraph()
        {
            m_timer.Stop();
        }
    }
}
