using Root_JEDI.Engineer;
using System.ComponentModel;
using System.Windows;

namespace Root_JEDI
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

        JEDI_Engineer m_engineer = new JEDI_Engineer();
        JEDI_Handler m_handler;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_engineer.Init("Pine2");
            engineerUI.Init(m_engineer);
            m_handler = (JEDI_Handler)m_engineer.ClassHandler();
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
