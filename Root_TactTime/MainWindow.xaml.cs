using System.Windows;

namespace Root_TactTime
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        TactTime m_tactTime; 
        public MainWindow()
        {
            InitializeComponent();
            m_tactTime = new TactTime();
            tactTimeUI.Init(m_tactTime); 
        }
    }
}
