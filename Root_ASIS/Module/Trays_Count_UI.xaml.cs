using System.Windows.Controls;

namespace Root_ASIS.Module
{
    /// <summary>
    /// Trays_Count_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Trays_Count_UI : UserControl
    {
        public Trays_Count_UI()
        {
            InitializeComponent();
        }

        Trays m_trays; 
        public void Init(Trays trays)
        {
            m_trays = trays;
            DataContext = trays;
            listView.ItemsSource = trays.m_aCount; 
        }

        private void buttonClear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_trays.ClearCount(); 
        }
    }
}
