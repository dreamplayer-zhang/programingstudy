using System.Windows.Controls;

namespace Root_ASIS
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

        ASIS_Engineer m_engineer = null;
        ASIS_Handler m_handler;
        public void Init(ASIS_Engineer engineer)
        {
            m_engineer = engineer;
            m_handler = (ASIS_Handler)engineer.ClassHandler();
            traysUI.Init(m_handler.m_trays);
            traysCountUI.Init(m_handler.m_trays); 
        }
    }
}
