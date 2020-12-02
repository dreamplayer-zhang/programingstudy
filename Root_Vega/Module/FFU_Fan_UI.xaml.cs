using System.Windows.Controls;

namespace Root_Vega.Module
{
    /// <summary>
    /// FFU_Fan_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FFU_Fan_UI : UserControl
    {
        public FFU_Fan_UI()
        {
            InitializeComponent();
        }

        FFU.Unit.Fan m_Fan; 
        public void Init(FFU.Unit.Fan fan)
        {
            m_Fan = fan;
            DataContext = fan; 
        }
    }
}
