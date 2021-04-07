using Root_Rinse_Unloader.Module;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_Rinse_Unloader.MainUI
{
    /// <summary>
    /// MagazineClamp_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MagazineClamp_UI : UserControl
    {
        public MagazineClamp_UI()
        {
            InitializeComponent();
        }

        Storage.Magazine m_magazine;
        public void Init(Storage.Magazine magazine)
        {
            m_magazine = magazine;
            groupBox.Header = magazine.m_id;
            DataContext = magazine;
        }

        private void buttonClamp_Click(object sender, RoutedEventArgs e)
        {
            m_magazine.RunClamp(!m_magazine.p_bClamp);
        }

        public void OnTimer(bool bBlink)
        {
            if (m_magazine.p_bCheck == false) buttonClamp.Foreground = Brushes.LightGray;
            else buttonClamp.Foreground = (bBlink && m_magazine.p_bClamp) ? Brushes.Red : Brushes.Black;
        }
    }
}
