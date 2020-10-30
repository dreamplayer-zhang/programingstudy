using System.Windows.Controls;

namespace Root_AxisMapping.MainUI
{
    /// <summary>
    /// Main_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Main_UI : UserControl
    {
        public Main_UI()
        {
            InitializeComponent();
        }

        Align m_align;
        Mapping m_mapping; 
        public void Init(AxisMapping_Engineer engineer)
        {
            m_align = new Align("Align", engineer);
            alignUI.Init(m_align);
            m_mapping = new Mapping("Mapping", engineer);
            mappingUI.Init(m_mapping); 
        }
    }
}
