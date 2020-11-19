using RootTools;
using RootTools.Trees;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Root_AxisMapping.MainUI
{
    /// <summary>
    /// Result_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Result_UI : UserControl
    {
        public Result_UI()
        {
            InitializeComponent();
        }

        Result m_result; 
        public void Init(Result result)
        {
            m_result = result;
            DataContext = result;
            memoryViewerResultUI.Init(m_result.m_axisMapping.m_memoryPoolResult.m_viewer, false);
            treeUI.Init(result.m_treeRoot);
            result.RunTree(Tree.eMode.Init); 

            OpenResult();
            result.Draw(); 
        }

        void OpenResult()
        {
            for (int ix = 0; ix < m_result.p_xArray; ix++)
            {
                try
                {
                    FileStream fs = new FileStream("c:\\Log\\Mapping" + ix.ToString("00") + ".txt", FileMode.Open);
                    StreamReader sr = new StreamReader(fs);
                    for (int iy = 0; iy < m_result.p_yArray; iy++)
                    {
                        Array array = m_result.p_aArray[ix, iy];
                        string sRead = sr.ReadLine();
                        string[] asRead = sRead.Split(',');
                        array.m_bInspect = (asRead[0] == "True");
                        if (array.m_bInspect)
                        {
                            double x = Convert.ToDouble(asRead[1].Replace('(', ' '));
                            double y = Convert.ToDouble(asRead[2].Replace(')', ' '));
                            array.m_rpCenter = new RPoint(x, y);
                        }
                    }
                    sr.Close();
                    fs.Close();
                }
                catch (Exception) { }
            }
        }

        private void buttonDraw_Click(object sender, RoutedEventArgs e)
        {
            m_result.Draw(); 
        }

    }
}
