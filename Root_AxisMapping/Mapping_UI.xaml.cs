using System.Windows.Controls;

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
        }

        private void listViewROI_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
