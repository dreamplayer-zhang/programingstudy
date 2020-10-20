using Root_ASIS.AOI;
using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            treeUI.Init(mapping.m_treeRoot);
            mapping.RunTree(Tree.eMode.Init); 
        }

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

        private void buttonInspect_Click(object sender, RoutedEventArgs e)
        {
            m_mapping.Inspect();
            m_mapping.Draw(AOIData.eDraw.Inspect);
        }
    }
}
