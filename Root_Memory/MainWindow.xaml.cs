using RootTools;
using RootTools.Comm;
using RootTools.Memory;
using System.Windows;
using System.Windows.Controls;

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
            Init(); 
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ThreadStop(); 
        }

        Memory_Engineer m_engineer = new Memory_Engineer();
        MemoryTool m_memoryTool; 
        void Init()
        {
            string id = "Root_Memory"; 
            m_engineer.Init(id);
            logViewUI.Init(LogViewer.m_logView);
            m_memoryTool = m_engineer.m_memoryTool; 
            memoryToolUI.Init(m_memoryTool);
            foreach (NamedPipe namedPipe in m_memoryTool.m_aNamedPipe)
            {
                TabItem item = new TabItem();
                item.Header = namedPipe.p_id;
                item.Content = namedPipe.p_ui;
                tabControlNamedPipe.Items.Add(item);
            }
        }

        void ThreadStop()
        {
            m_engineer.ThreadStop(); 
        }
    }
}
