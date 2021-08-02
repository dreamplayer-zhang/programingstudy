using Root_Rinse_Loader.Module;
using System.Windows.Controls;

namespace Root_Rinse_Loader.MainUI
{
    /// <summary>
    /// Progress_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Progress_UI : UserControl
    {
        public Progress_UI()
        {
            InitializeComponent();
        }

        RinseL m_rinse; 
        public void Init(RinseL rinse)
        {
            m_rinse = rinse;
            DataContext = rinse;
        }
    }
}
