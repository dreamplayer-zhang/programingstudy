using Root_Rinse_Unloader.Module;
using System.Windows;
using System.Windows.Controls;

namespace Root_Rinse_Unloader.MainUI
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

        RinseU m_rinse;
        public void Init(RinseU rinse)
        {
            m_rinse = rinse;
            DataContext = rinse;
            listViewSend.ItemsSource = rinse.p_aSend;
            listViewReceive.ItemsSource = rinse.p_aReceive;
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            m_rinse.p_aSend.Clear();
            m_rinse.p_aReceive.Clear(); 
        }
    }
}
