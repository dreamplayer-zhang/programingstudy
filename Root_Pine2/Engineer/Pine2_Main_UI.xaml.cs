using System.Windows.Controls;

namespace Root_Pine2.Engineer
{
    /// <summary>
    /// Pine2_Main_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Pine2_Main_UI : UserControl
    {
        public Pine2_Main_UI()
        {
            InitializeComponent();
        }

        Pine2_Handler m_handler = null;
        public void Init(Pine2_Handler handler)
        {
            m_handler = handler;
            processUI.Init(handler);
        }

    }
}
