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
            treeRootUI.Init(memoryTool.m_treeRootRun);
            memoryTool.RunTreeRun(Tree.eMode.Init); 
            treeRootMemoryUI.Init(memoryTool.m_treeRootMemory);
            memoryTool.RunTreeMemory(Tree.eMode.Init);
            m_memoryTool.OnChangeMemoryPool += M_memoryTool_OnChangeTool;
            InitTabControl();
        }

        private void M_memoryTool_OnChangeTool()
        {
            InitTabControl();
        }

        void InitTabControl()
        {
            tabControlPool.Items.Clear(); 
            foreach (MemoryPool pool in m_memoryTool.p_aPool)
            {
                TabItem item = new TabItem();
                item.Header = pool.p_id;
                item.Content = pool.p_ui;
                tabControlPool.Items.Add(item); 
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            m_memoryTool.p_sInfo = m_memoryTool.SaveMemory(); 
        }

        private void buttonRead_Click(object sender, RoutedEventArgs e)
        {
            m_memoryTool.p_sInfo = m_memoryTool.ReadMemory();
        }
    }
}
