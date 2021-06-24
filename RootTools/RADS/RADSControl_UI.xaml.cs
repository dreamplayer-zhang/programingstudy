﻿using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RootTools.RADS
{
    /// <summary>
    /// RADSControl_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RADSControl_UI : UserControl
    {
        public RADSControl_UI()
        {
            InitializeComponent();
        }

        RADSControl m_RADSControl;
        public Chart m_voltChart = null;
        public DataPointCollection m_voltPoints = null;

        public void Init(RADSControl radsControl)
        {
            m_RADSControl = radsControl;
            this.DataContext = radsControl.p_connect.p_CurrentController;
            if (radsControl.p_connect.p_CurrentController != null)
            {
                treeUI.Init(radsControl.p_connect.p_CurrentController.p_TreeRoot);
                radsControl.p_connect.p_CurrentController.RunTree(Tree.eMode.Init);
            }

            // voltage graph
            m_voltChart = MyWinformChart;

            Series series = m_voltChart.Series["series"];
            m_voltPoints = series.Points;
            m_voltPoints.Add(0);

            ChartArea chartArea = m_voltChart.ChartAreas["chartarea"];
            //chartArea.AxisX.IsInterlaced = true;
            chartArea.AxisX.IsReversed = true;
            //chartArea.AxisX.IsStartedFromZero = false;
            chartArea.AxisX.Maximum = 1000;
            chartArea.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chartArea.AxisX.IsMarginVisible = false;
            chartArea.AxisX.LabelStyle.Enabled = false;
            chartArea.AxisX.LabelStyle.IsEndLabelVisible = true;

            //chartArea.AxisY.IsInterlaced = true;
            chartArea.AxisY.Minimum = 0;
            chartArea.AxisY.Maximum = 1000;
            chartArea.AxisY.IsMarginVisible = false;
            chartArea.AxisY.LabelStyle.Enabled = false;
            chartArea.AxisY.LabelStyle.IsEndLabelVisible = true;
        }
    }
}
