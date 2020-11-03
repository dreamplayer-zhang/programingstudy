using Root_AxisMapping.MainUI;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Root_AxisMapping
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

        #region Window Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\AxisMapping")) Directory.CreateDirectory(@"C:\Recipe\AxisMapping");
            Init();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
        }
        #endregion

        AxisMapping_Engineer m_engineer = new AxisMapping_Engineer();
        void Init()
        {
            m_engineer.Init("AxisMapping");
            engineerUI.Init(m_engineer);
            mainUI.Init(m_engineer); 
        }

        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }
    }
}
