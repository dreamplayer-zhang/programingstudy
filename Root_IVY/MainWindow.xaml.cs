using Root_IVY.Engineer;
using System.ComponentModel;
using System.Windows;

namespace Root_IVY
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        IVY_Engineer m_engineer = new IVY_Engineer();
        IVY_Handler m_handler;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_engineer.Init("IVY");
            engineerUI.Init(m_engineer);
            m_handler = (IVY_Handler)m_engineer.ClassHandler();
            Init();
        }

        void Init()
        {
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_engineer.ThreadStop();
        }
    }
}
