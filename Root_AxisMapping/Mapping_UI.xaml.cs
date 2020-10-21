using Root_ASIS.AOI;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Root_AxisMapping
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

        AxisMapping_Engineer m_engineer;
        AxisMapping_Handler m_handler; 
        Mapping m_mapping; 
        public void Init(Mapping mapping, AxisMapping_Engineer engineer)
        {
            m_engineer = engineer;
            m_handler = (AxisMapping_Handler)engineer.ClassHandler();
            m_mapping = mapping;
            memoryViewerAlignUI.Init(mapping.m_axisMapping.m_memoryPool.m_viewer, false);
            //memoryViewerAlignUI.Init()
            listViewROI.ItemsSource = mapping.m_aROI;
            treeGrabUI.Init(mapping.m_treeRootGrab);
            mapping.RunTreeGrab(Tree.eMode.Init);
            treeInspectUI.Init(mapping.m_treeRootInspect);
            mapping.RunTreeInspect(Tree.eMode.Init);
            treeRotateUI.Init(mapping.m_treeRootRotate);
            mapping.RunTreeRotate(Tree.eMode.Init);
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
            IsEnabled = (m_mapping.m_axisMapping.p_eState != ModuleBase.eState.Run); 
            buttonInspect.IsEnabled = (m_mapping.m_aROI[0].p_eROI == AOIData.eROI.Done) && (m_mapping.m_aROI[1].p_eROI == AOIData.eROI.Done);
            buttonRotate.IsEnabled = (m_mapping.m_fRotateAngle != 0);
        }
        #endregion

        #region Grab
        private void buttonGrab_Click(object sender, RoutedEventArgs e)
        {
            m_mapping.RunGrab(); 
            m_mapping.InvalidROI(); 
        }
        #endregion

        #region ROI
        private void listViewROI_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int nSelect = listViewROI.SelectedIndex;
            if (nSelect < 0) return;
            if (nSelect >= m_mapping.m_aROI.Count) return;
            m_mapping.ClearActive();
            m_mapping.m_aROI[nSelect].p_eROI = AOIData.eROI.Active;
            m_mapping.Draw(AOIData.eDraw.ROI);
        }
        #endregion

        #region Inspect
        private void buttonInspect_Click(object sender, RoutedEventArgs e)
        {
            m_mapping.Inspect();
            m_mapping.Draw(AOIData.eDraw.Inspect);
            m_mapping.RunTreeRotate(Tree.eMode.Init);
        }
        #endregion

        private void buttonRotate_Click(object sender, RoutedEventArgs e)
        {
            m_mapping.InvalidROI();
        }

    }
}
