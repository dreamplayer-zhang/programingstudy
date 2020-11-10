using Root_ASIS.AOI;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Root_AxisMapping.MainUI
{
    /// <summary>
    /// Mapping_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Mapping_UI : UserControl
    {
        public Mapping_UI()
        {
            InitializeComponent();
        }

        Mapping m_mapping; 
        public void Init(Mapping mapping)
        {
            m_mapping = mapping;
            DataContext = mapping; 
            memoryViewerAlignUI.Init(mapping.m_axisMapping.m_memoryPool.m_viewer, false);
            InitArray();
            treeGrabUI.Init(mapping.m_treeRootGrab);
            mapping.RunTreeGrab(Tree.eMode.Init);
            listViewROI.ItemsSource = mapping.m_aROI;
            treeInspectUI.Init(mapping.m_treeRootInspect);
            mapping.RunTreeInspect(Tree.eMode.Init);
            InitTimer();
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(0.1);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            IsEnabled = (m_mapping.m_axisMapping.p_eState != ModuleBase.eState.Run) || EQ.p_bSimulate;
            buttonInspect.IsEnabled = (m_mapping.m_aROI[0].p_eROI == AOIData.eROI.Done) && (m_mapping.m_aROI[1].p_eROI == AOIData.eROI.Done);
        }
        #endregion

        #region Array
        private void buttonArray_Click(object sender, RoutedEventArgs e)
        {
            InitArray(); 
        }

        CPoint m_szArray = new CPoint(); 
        void InitArray()
        {
            m_mapping.InitArray();
            m_szArray = new CPoint(m_mapping.p_xArray, m_mapping.p_yArray);
            gridArray.Children.Clear();
            gridArray.RowDefinitions.Clear(); 
            gridArray.ColumnDefinitions.Clear();
            for (int y = 0; y < m_szArray.Y; y++) gridArray.RowDefinitions.Add(new RowDefinition());
            for (int x = 0; x < m_szArray.X; x++) gridArray.ColumnDefinitions.Add(new ColumnDefinition());
            for (int y = 0; y < m_szArray.Y; y++)
            {
                for (int x = 0; x < m_szArray.X; x++)
                {
                    Array_UI ui = m_mapping.m_aArray[x, y].p_ui;
                    Grid.SetColumn(ui, x);
                    Grid.SetRow(ui, y);
                    gridArray.Children.Add(ui); 
                }
            }
        }
        #endregion

        private void buttonGrab_Click(object sender, RoutedEventArgs e)
        {
            m_mapping.RunGrab();
            m_mapping.InvalidROI();
        }

        private void listViewROI_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int nSelect = listViewROI.SelectedIndex;
            if (nSelect < 0) return;
            if (nSelect >= m_mapping.m_aROI.Count) return;
            m_mapping.ClearActive();
            m_mapping.m_aROI[nSelect].p_eROI = AOIData.eROI.Active;
            m_mapping.Draw(AOIData.eDraw.ROI);
        }

        private void buttonInspect_Click(object sender, RoutedEventArgs e)
        {
            m_mapping.Inspect(m_mapping.m_xSelect);
            m_mapping.Draw(AOIData.eDraw.Inspect);
        }
    }
}
