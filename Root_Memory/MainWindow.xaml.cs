using RootTools;
using RootTools.Memory;
using System.ComponentModel;
using System.Windows;

namespace Root_Memory
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            EQ.m_sModel = Title; 
            Init(); 
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop(); 
        }

        Memory_Engineer m_engineer = new Memory_Engineer();
        void Init()
        {
            m_engineer.Init();
            logViewUI.Init(LogView.m_logView);
            memoryToolUI.Init(m_engineer.m_memoryTool);
        }

        void ThreadStop()
        {
            m_engineer.ThreadStop(); 
        }
    }
}
