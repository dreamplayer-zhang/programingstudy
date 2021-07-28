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

namespace Root_CAMELLIA.LibSR_Met
{
    /// <summary>
    /// PMRGraph.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PMRGraph : UserControl
    {
        DataManager m_DM = DataManager.GetInstance();
        public bool m_bDataDetect = false;
        ScottPlot.PlottableScatterHighlight m_ReflectancePlotTable;
        public PMRGraph()
        {
            InitializeComponent();
        }

        public void DrawReflectanceGraph(int nPointIndex, string xlabel, string ylabel)
        {
            PMGraphR.Reset();
            PMGraphR.plt.XLabel(xlabel);
            PMGraphR.plt.YLabel(ylabel);
            string sResult = "";
            RawData data = m_DM.m_RawData[nPointIndex];
            double[] NIR_Wavelength = new double[data.nNIRDataNum];
            double[] NIR_Reflectance = new double[data.nNIRDataNum];

            for (int n = 0; n < data.nNIRDataNum; n++)
            {
                NIR_Wavelength[n] = data.Wavelength[n];
                NIR_Reflectance[n] = data.Reflectance[n];
            }

            m_ReflectancePlotTable = PMGraphR.plt.PlotScatterHighlight(NIR_Wavelength, NIR_Reflectance, markerSize: 2.5);
            PMGraphR.plt.Axis(350, 1500, -10, 100);
            PMGraphR.Render();

            PMlabelR.Visibility = Visibility.Visible;

            m_bDataDetect = true;
        }

        private void PMGraphR_MouseMove(object sender, MouseEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (m_bDataDetect == true)
                {
                    var mousePos = e.MouseDevice.GetPosition(PMGraphR);
                    double mouseX = PMGraphR.plt.CoordinateFromPixelX(mousePos.X);
                    double mouseY = PMGraphR.plt.CoordinateFromPixelY(mousePos.Y);

                    m_ReflectancePlotTable.HighlightClear();
                    var (dX, dY, nIndex) = m_ReflectancePlotTable.HighlightPointNearest(mouseX, mouseY);
                    PMGraphR.Render();

                    PMlabelR.Visibility = Visibility.Visible;
                    PMlabelR.Content = $"(Wavelength[nm], Reflectance[%]) = ({dX:N1}, {dY:N3})";
                }
            }));
        }

        private void comboBoxPointIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if ((int)comboBoxPointIndex.SelectedIndex == -1)
                {
                    return;
                }
                DrawReflectanceGraph((int)comboBoxPointIndex.SelectedIndex, "Wavelength [nm]", "Reflectance [%]");
            }));
        }

        private void comboBoxPointIndex_DropDownOpened(object sender, EventArgs e)
        {
            comboBoxPointIndex.Items.Clear();
            if (m_DM.nRepeatCount == 1)
            {
                for (int n = 0; n < ConstValue.RAWDATA_POINT_MAX_SIZE; n++)
                {
                    if (m_DM.m_RawData[n].bDataExist)
                    {
                        comboBoxPointIndex.Items.Add(n + 1);
                    }
                }
            }
            else
            {
                for (int n = 0; n < ConstValue.RAWDATA_POINT_MAX_SIZE; n++)
                {
                    if (m_DM.m_RawData[n].bDataExist)
                    {
                        int nGraphIndex = (n % m_DM.nRepeatCount);
                        int nPointIndex = (n / m_DM.nRepeatCount);
                        string sGraphIndex = (nPointIndex + 1).ToString() + "-" + (nGraphIndex + 1).ToString();
                        comboBoxPointIndex.Items.Add(sGraphIndex);
                    }
                }
            }
        }
    }
}
