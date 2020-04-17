using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace RootTools.Memory
{
    /// <summary>
    /// MemoryTool_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MemoryTool_UI : UserControl
    {
        public MemoryTool_UI()
        {
            InitializeComponent();
        }

        MemoryTool m_memoryTool;
        public void Init(MemoryTool memoryTool)
        {
            m_memoryTool = memoryTool;
            this.DataContext = memoryTool;
            treeRootUI.Init(memoryTool.m_treeRoot);
            memoryTool.RunTree(Tree.eMode.Init); 
            treeRootSetupUI.Init(memoryTool.m_treeRootSetup);
            memoryTool.RunSetupTree(Tree.eMode.Init);
            m_memoryTool.OnChangeTool += M_memoryTool_OnChangeTool;
            InitTabControl();
            namedPipeUI.Init(memoryTool.m_aNamedPipe[0]); 
        }

        private void M_memoryTool_OnChangeTool()
        {
            InitTabControl();
        }

        void InitTabControl()
        {
            tabControlPool.Items.Clear(); 
            foreach (MemoryPool pool in m_memoryTool.m_aPool)
            {
                TabItem item = new TabItem();
                item.Header = pool.p_id;
                item.Content = pool.p_ui;
                tabControlPool.Items.Add(item); 
            }
        }

        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            m_memoryTool.ButtonRun(); 
        }
    }
}
