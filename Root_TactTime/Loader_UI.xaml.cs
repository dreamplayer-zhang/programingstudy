using System.Windows.Controls;

namespace Root_TactTime
{
    /// <summary>
    /// Loader_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Loader_UI : UserControl
    {
        public Loader_UI()
        {
            InitializeComponent();
        }

        Loader m_loader; 
        public void Init(Loader loader)
        {
            m_loader = loader;
            DataContext = loader;
            listView.ItemsSource = loader.m_aEvent; 
        }
    }
}
