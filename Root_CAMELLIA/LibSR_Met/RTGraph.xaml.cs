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
using System.Drawing;
using System.IO;
using NanoView;

namespace Root_CAMELLIA.LibSR_Met
{
    /// <summary>
    /// RTGraph.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RTGraph : UserControl
    {
        DataManager m_DM = DataManager.GetInstance();
        string m_strCurrentDir = string.Empty;
        public bool m_bDataDetect = false;

        bool m_bUseFile = false;

        ScottPlot.PlottableScatterHighlight m_ReflectancePlotTable;
        ScottPlot.PlottableScatterHighlight m_ReflectancePlotTable_Calc;
        ScottPlot.PlottableScatterHighlight m_TransmittancePlotTable;
        ScottPlot.PlottableScatter m_DCOLTranmittancePoitTable;

        public RTGraph()
        {
            InitializeComponent();

            if (m_bUseFile)
            {
                btnDrawMode.Content = FindResource("File");
                btnOpenDatas.IsEnabled = true;
            }
            else
            {
                btnDrawMode.Content = FindResource("Measure");
                btnOpenDatas.IsEnabled = false;
            }
        }

        public void DrawReflectanceGraph(int nPointIndex, string xlabel, string ylabel, int nRepeatCount, double[] xvalues = null, double[] yvalues = null, double[] yvalues2 = null )
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (m_DM.bThickness)
                {
                    GroupThicknessResult.IsEnabled = true;
                }
                else
                {
                    GroupThicknessResult.IsEnabled = false;
                    GroupTransmittance.IsEnabled = false;
                    GraphT.Visibility = Visibility.Hidden;
                }

                GraphR.Reset();
                GraphR.plt.XLabel(xlabel);
                GraphR.plt.YLabel(ylabel);
                string sResult = "";
                if (xvalues == null || yvalues == null)
                {
                    RawData data = m_DM.m_RawData[nPointIndex];

                    double[] VIS_Wavelength = new double[m_DM.nThicknessDataNum];
                    double[] VIS_Reflectance = new double[m_DM.nThicknessDataNum];
                    double[] CalcReflectance = new double[m_DM.nThicknessDataNum];
                    double[] NIR_Wavelength = new double[data.nNIRDataNum];
                    double[] NIR_Reflectance = new double[data.nNIRDataNum];
                    for (int n = 0; n < m_DM.nThicknessDataNum; n++)
                    {
                        VIS_Wavelength[n] = data.VIS_Wavelength[n];
                        VIS_Reflectance[n] = data.VIS_Reflectance[n];
                        CalcReflectance[n] = data.CalcReflectance[n];
                    }
                    for (int n = 0; n < data.nNIRDataNum; n++)
                    {
                        NIR_Wavelength[n] = data.Wavelength[n];
                        NIR_Reflectance[n] = data.Reflectance[n];
                    }
                    try
                    {
                        if (m_DM.bExcept_NIR)
                        {
                            m_ReflectancePlotTable = GraphR.plt.PlotScatterHighlight(VIS_Wavelength, VIS_Reflectance, markerSize: 2.5);
                            GraphR.plt.Axis(350, 950, -10, 100);
                        }
                        else
                        {
                            m_ReflectancePlotTable = GraphR.plt.PlotScatterHighlight(NIR_Wavelength, NIR_Reflectance, markerSize: 2.5);
                            GraphR.plt.Axis(350, 1500, -10, 100);
                        }
                    }
                    catch(Exception)
                    {

                    }

                   
                    if (m_DM.bThickness && m_DM.bViewCalRGraph)
                    {
                        m_ReflectancePlotTable_Calc = GraphR.plt.PlotScatterHighlight(VIS_Wavelength, CalcReflectance, markerSize: 2.5);
                    }

                    if (m_DM.bThickness)
                    {
                        int nPointIdx = 0;
                        if (nRepeatCount == 1)
                        {
                            nPointIdx = nPointIndex+1;
                        }
                        else
                        {
                            nPointIdx = (nPointIndex % nRepeatCount)+1;
                        }
                                                sResult = string.Format("Point: {0}\nX Pos: {1}\nY Pos: {2}\nThickness:\n", nPointIdx, data.dX, data.dY);

                        if (data.Thickness != null && data.Thickness.Count > 0)
                        {
                            for (int n = 1; n < m_DM.m_LayerData.Count-1; n++)
                            {
                                sResult += string.Concat(m_DM.m_LayerData[n].hostname) + " : " + data.Thickness[n].ToString("0.####") + "Å\n";
                            }
                        }

                        if (data.dGoF < 0.0)
                        {
                            data.dGoF = 0.00000;
                        }
                        
                        sResult += "GOF: " + data.dGoF.ToString("0.#####") + "\n";
                    }

                }
                else
                {
                    m_ReflectancePlotTable = GraphR.plt.PlotScatterHighlight(xvalues, yvalues, markerSize: 2.5);
                }
                GraphR.Render();

                m_bDataDetect = true;
                FlowDocument flowDoc = new FlowDocument();
                flowDoc.Blocks.Add(new Paragraph(new Run(sResult)));
                textBoxResult.Document = flowDoc;
            }));
        }

        public void DrawTransmittanceGraph(int nPointIndex, string xlabel, string ylabel, double[] xvalues = null, double[] yvalues = null)
        {
            
            Dispatcher.Invoke(new Action(() =>
            {
                if (m_DM.bTransmittance)
                {
                    GroupTransmittance.IsEnabled = true;

                }
                else
                {
                    GroupTransmittance.IsEnabled = false;
                    GraphT.Visibility = Visibility.Hidden;
                }
                GraphT.Reset();
                GraphT.plt.XLabel(xlabel);
                GraphT.plt.YLabel(ylabel);
                RawData data = m_DM.m_RawData[nPointIndex];
                if (xvalues == null || yvalues == null)
                {
                    if (m_DM.bTransmittance)
                    {
                        if (!m_DM.bCalDCOLTransmittance)
                        {
                            if (m_DM.bExcept_NIR)
                            {
                                double[] VIS_Wavelength = new double[m_DM.nThicknessDataNum];
                                double[] VIS_Transmittance = new double[m_DM.nThicknessDataNum];
                                for (int n = 0; n < m_DM.nThicknessDataNum; n++)
                                {
                                    VIS_Wavelength[n] = data.VIS_Wavelength[n];
                                    VIS_Transmittance[n] = data.Transmittance[n];
                                }
                                m_TransmittancePlotTable = GraphT.plt.PlotScatterHighlight(VIS_Wavelength, VIS_Transmittance, markerSize: 2.5);
                                GraphT.plt.Axis(350, 950, -10, 100);
                            }
                            else
                            {
                                double[] NIR_Wavelength = new double[data.nNIRDataNum];
                                double[] NIR_Transmittance = new double[data.nNIRDataNum];
                                for (int n = 0; n < data.nNIRDataNum; n++)
                                {
                                    NIR_Wavelength[n] = data.Wavelength[n];
                                    NIR_Transmittance[n] = data.Transmittance[n];
                                }
                                m_TransmittancePlotTable = GraphT.plt.PlotScatterHighlight(NIR_Wavelength, NIR_Transmittance, markerSize: 2.5);
                                GraphT.plt.Axis(350, 1550, -10, 100);
                            }
                        }
                        else
                        {
                            int DCOLWavelengthCount = m_DM.m_ScalesListT.Count;
                            double[] DCOL_Wavelength = new double[DCOLWavelengthCount];
                            double[] DCOL_Transmittance = new double[DCOLWavelengthCount];

                            for ( int n=0; n< DCOLWavelengthCount; n++)
                            {
                                DCOL_Wavelength[n] = data.DCOLTransmittance[n].Wavelength;
                                DCOL_Transmittance[n] = data.DCOLTransmittance[n].RawTransmittance;
                                m_DCOLTranmittancePoitTable = GraphT.plt.PlotPoint(DCOL_Wavelength[n], DCOL_Transmittance[n]);
                            }
                            GraphT.plt.Axis(350, 1500, -10, 100);
                        }
                    }
                }
                else
                {
                    m_TransmittancePlotTable = GraphT.plt.PlotScatterHighlight(xvalues, yvalues, markerSize: 2.5);
                }
                GraphT.Render();
            }));
        }

        private void comboBoxDataIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (m_bUseFile)
                {
                    if (comboBoxDataIndex.Items.Count <= 0)
                        return;

                    if (!string.IsNullOrEmpty(m_strCurrentDir))
                    {
                        GraphR.Reset();
                        GraphT.Reset();

                        m_bDataDetect = true;
                        string path = m_strCurrentDir + "\\" + comboBoxDataIndex.SelectedItem + ".csv";
                        StreamReader sr = new StreamReader(path);

                        var column1 = new List<string>();
                        var column2 = new List<string>();
                        var column3 = new List<string>();

                        while (sr.EndOfStream == false)
                        {
                            var splits = sr.ReadLine().Split(',');

                            column1.Add(splits[0]);
                            column2.Add(splits[1]);
                            column3.Add(splits[2]);
                        }

                        //배열 선언
                        double[] xvalues = new double[column1.Count - 1];
                        double[] yvalues = new double[column1.Count - 1];
                        double[] yvalues2 = new double[column1.Count - 1];

                        //(x,y)에 넣을 배열 만들기
                        for (int a = 0; a < column1.Count - 1; a++)
                        {
                            xvalues[a] = Convert.ToDouble(column1[a + 1]);
                            yvalues[a] = Convert.ToDouble(column2[a + 1]);
                            yvalues2[a] = Convert.ToDouble(column3[a + 1]);
                        }

                        DrawReflectanceGraph((int)comboBoxDataIndex.SelectedIndex, "Wavelength [nm]", "Reflectance [%]",1 ,xvalues, yvalues);
                        if (m_DM.bTransmittance)
                        {
                            DrawTransmittanceGraph((int)comboBoxDataIndex.SelectedIndex, "Wavelength [nm]", "Transmittance [%]", xvalues, yvalues2);
                        }
                    }
                    else
                    {
                        MessageBox.Show("[Warning] Open Files First!", "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    }
                }
                else
                {
                    if ((int)comboBoxDataIndex.SelectedIndex == -1)
                    {
                        return;
                    }
                    DrawReflectanceGraph((int)comboBoxDataIndex.SelectedIndex, "Wavelength [nm]", "Reflectance [%]", m_DM.nRepeatCount);
                    if (m_DM.bTransmittance)
                    {
                        DrawTransmittanceGraph((int)comboBoxDataIndex.SelectedIndex, "Wavelength [nm]", "Transmittance [%]");

                    }
                }
            }));
        }

        private void btnDrawMode_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                m_bUseFile = !m_bUseFile;

                if (m_bUseFile)
                {
                    btnDrawMode.Content = FindResource("File");
                    btnOpenDatas.IsEnabled = true;
                }
                else
                {
                    btnDrawMode.Content = FindResource("Measure");
                    btnOpenDatas.IsEnabled = false;
                }
            }));
        }

        private void btnOpenDatas_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Reflectance Data (*.csv)|*.csv";
            ofd.FilterIndex = 0;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                comboBoxDataIndex.Items.Clear();

                var fileinfos = ofd.FileNames.Select(x => new FileInfo(x)).ToArray();

                foreach (FileInfo file in fileinfos)
                {
                    comboBoxDataIndex.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file.Name));
                    m_strCurrentDir = System.IO.Path.GetDirectoryName(file.FullName);
                }

                if(comboBoxDataIndex.Items.Count > 0)
                    comboBoxDataIndex.SelectedIndex = 0;
            }
        }

        private void GraphR_MouseMove(object sender, MouseEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (m_bDataDetect == true)
                {
                    var mousePos = e.MouseDevice.GetPosition(GraphR);
                    double mouseX = GraphR.plt.CoordinateFromPixelX(mousePos.X);
                    double mouseY = GraphR.plt.CoordinateFromPixelY(mousePos.Y);

                    m_ReflectancePlotTable.HighlightClear();
                    var (dX, dY, nIndex) = m_ReflectancePlotTable.HighlightPointNearest(mouseX, mouseY);

                    double dX2 = 0.0, dY2 = 0.0;
                    int nIndex2 = 0;
                    if (m_ReflectancePlotTable_Calc != null)
                    {
                        m_ReflectancePlotTable_Calc.HighlightClear();
                        (dX2, dY2, nIndex2) = m_ReflectancePlotTable_Calc.HighlightPointNearest(mouseX, mouseY);
                    }

                    GraphR.Render();

                    labelR.Visibility = Visibility.Visible;
                    labelR.Content = $"(Wavelength[nm], Reflectance[%]) = ({dX:N1}, {dY:N3})" + Environment.NewLine + $"(Wavelength[nm], CalcReflectance[%]) = ({dX2:N1}, {dY2:N3})";
                }
            }));
        }

        private void GraphT_MouseMove(object sender, MouseEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (m_bDataDetect == true)
                {
                    if (m_DM.bTransmittance)
                    {
                        if (!m_DM.bCalDCOLTransmittance)
                        {
                            var mousePos = e.MouseDevice.GetPosition(GraphT);
                            double mouseX = GraphT.plt.CoordinateFromPixelX(mousePos.X);
                            double mouseY = GraphT.plt.CoordinateFromPixelY(mousePos.Y);

                            m_TransmittancePlotTable.HighlightClear();
                            var (x, y, index) = m_TransmittancePlotTable.HighlightPointNearest(mouseX, mouseY);
                            GraphT.Render();

                            labelT.Visibility = Visibility.Visible;
                            labelT.Content = $"(Wavelength[nm], Transmittance[%]) = ({x:N1}, {y:N3})";
                        }
                        else
                        {
                            var mousePos = e.MouseDevice.GetPosition(GraphT);
                            double mouseX = GraphT.plt.CoordinateFromPixelX(mousePos.X);
                            double mouseY = GraphT.plt.CoordinateFromPixelY(mousePos.Y);

                            m_TransmittancePlotTable.HighlightClear();
                            //var (x, y, index) = m_TransmittancePlotTable.HighlightPointNearest(mouseX, mouseY);
                            //var (x,y,index) = m_DCOLTranmittancePoitTable.
                            //GraphT.Render();

                            //labelT.Visibility = Visibility.Visible;
                            //labelT.Content = $"(Wavelength[nm], Transmittance[%]) = ({x:N1}, {y:N3})";
                        }
                    }
                }
            }));
        }

        private void comboBoxDataIndex_DropDownOpened(object sender, EventArgs e)
        {
            if (!m_bUseFile)
            {
                comboBoxDataIndex.Items.Clear();
                if (m_DM.nRepeatCount == 1)
                {
                    for (int n = 0; n < ConstValue.RAWDATA_POINT_MAX_SIZE; n++)
                    {
                        if (m_DM.m_RawData[n].bDataExist)
                        {
                            comboBoxDataIndex.Items.Add(n + 1);
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
                            string sGraphIndex = (nPointIndex+ 1).ToString() +"-"+ (nGraphIndex+1).ToString();                            
                            comboBoxDataIndex.Items.Add(sGraphIndex);
                        }
                    }
                }
            }
        }
    }
}
