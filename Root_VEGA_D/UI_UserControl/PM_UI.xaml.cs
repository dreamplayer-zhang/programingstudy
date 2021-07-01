﻿using Root_VEGA_D.Module;
using RootTools;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
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

namespace Root_VEGA_D
{
    /// <summary>
    /// PM_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PM_UI : UserControl
    {
        Vision m_vision;
        
        DataTable dataTable;

        public TreeRoot m_treeRootRun;

        public PM_UI()
        {
            InitializeComponent();
        }

        public void Init(Vision vision)
        {
            m_vision = vision;

            m_treeRootRun = new TreeRoot("PM", m_vision.m_log);
            m_vision.m_treeRootRun.UpdateTree += M_treeRootRun_UpdateTree;
            m_treeRootRun.UpdateTree += M_treeRootRun_UpdateTree;

            RunTreeRun(Tree.eMode.RegRead);

            treePMSetting.Init(m_vision.m_treeRootRun);
            RunTreeRun(Tree.eMode.Init);

            // Data table column setting
            dataTable = new DataTable();
            dataTable.Columns.Add("Date");
            dataTable.Columns.Add("Time");
            dataTable.Columns.Add("PM_Success");
            dataTable.Columns.Add("Coaxial_Result");
            dataTable.Columns.Add("Transmitted_Result");
            dataTable.Columns.Add("Coaxial_Avg");
            dataTable.Columns.Add("Transmitted_Avg");
            dataTable.Columns.Add("USL");
            dataTable.Columns.Add("LSL");
            dgPMResult.ItemsSource = dataTable.DefaultView;

            // Update chart
            UpdatePMLogChart();
        }

        private void M_treeRootRun_UpdateTree()
        {
            RunTreeRun(Tree.eMode.Update);
            RunTreeRun(Tree.eMode.RegWrite);
            RunTreeRun(Tree.eMode.Init);
        }

        public void RunTreeRun(Tree.eMode mode)
        {
            Run_PM moduleRun = GetModuleRun();
            if(moduleRun != null)
            {
                //m_vision.m_treeRootRun.p_eMode = mode;
                m_treeRootRun.p_eMode = mode;
                //moduleRun.RunTree(m_vision.m_treeRootRun.GetTree(moduleRun.m_sModuleRun, true, true), true);
                moduleRun.RunTree(m_treeRootRun.GetTree(moduleRun.m_sModuleRun, true, true), true);
            }
        }

        Run_PM GetModuleRun()
        {
            return m_vision.CloneModuleRun("PM") as Run_PM;
        }

        private void btnPM_Click(object sender, RoutedEventArgs e)
        {
            Run_PM moduleRun = m_vision.CloneModuleRun("PM") as Run_PM;
            if (moduleRun != null)
            {
                m_vision.StartRun(moduleRun);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdatePMLogChart();
        }

        void UpdatePMLogChart()
        {
            string sPath = LogView._logView.p_sPath;
            string sPathPM = sPath + "\\PM";

            string[] arrStr = Directory.GetFiles(sPathPM);

            DirectoryInfo dir = new DirectoryInfo(sPathPM);
            FileInfo[] files = dir.GetFiles();
            Array.Sort(files, (x, y) => x.Name.CompareTo(y.Name));

            dataTable.Rows.Clear();

            MyWinformChart.Series["seriesSuccess"].Points.Clear();
            MyWinformChart.Series["seriesFailed"].Points.Clear();

            for (int i = files.Length - 5; i < files.Length; i++)
            {
                if (i < 0)
                    continue;

                int nTotalCount = 0;
                int nSuccess = 0;

                string date = System.IO.Path.GetFileNameWithoutExtension(sPathPM + "\\" + files[i]);

                using (StreamReader sr = new StreamReader(sPathPM + "\\" + files[i]))
                {
                    try
                    {
                        // 첫줄의 헤더 데이터 읽어오기
                        sr.ReadLine();

                        string sRead;
                        while (true)
                        {
                            sRead = sr.ReadLine();
                            if (sRead == null)
                                break;

                            string[] arrSplit = sRead.Split(',');
                            string[] arrData = new string[arrSplit.Length + 1];

                            arrData[0] = date;
                            Array.Copy(arrSplit, 0, arrData, 1, arrSplit.Length);

                            dataTable.Rows.Add(arrData);

                            nTotalCount++;
                            if (arrSplit[1] == "True") nSuccess++;
                        }
                    }
                    catch (Exception)
                    {
                        return;
                    }
                };

                MyWinformChart.Series["seriesSuccess"].Points.AddXY(date, nSuccess);
                MyWinformChart.Series["seriesFailed"].Points.AddXY(date, nTotalCount - nSuccess);

                MyWinformChart.AlignDataPointsByAxisLabel();
            }
        }
    }
}
