using RootTools;
using RootTools.Inspects;
using RootTools.Memory;
using System.Windows;

namespace Root_Inspect
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Title = EQ.m_sModel; 
            Init();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ThreadStop();
        }

        #region Memory
        MemoryTool m_memoryTool;
        void InitMemory()
        {
            m_memoryTool = m_engineer.m_memoryTool;
            memoryToolUI.Init(m_memoryTool);
            namedPipeUI.Init(m_memoryTool.m_aNamedPipe[0]);
        }
        #endregion

        #region Inspect
        InspectTool m_inspectTool;
        void InitInspect()
        {
            m_inspectTool = new InspectTool(EQ.m_sModel, m_engineer, false);
            inspectToolUI.Init(m_inspectTool); 
        }
        #endregion

        Inspect_Engineer m_engineer = new Inspect_Engineer();
        void Init()
        {
            string id = EQ.m_sModel; 
            m_engineer.Init();
            logViewUI.DataContext = m_engineer.ClassLogView();
            InitMemory();
            InitInspect(); 
        }

        void ThreadStop()
        {
            m_inspectTool.ThreadStop(); 
            m_engineer.ThreadStop();
        }
    }
}
