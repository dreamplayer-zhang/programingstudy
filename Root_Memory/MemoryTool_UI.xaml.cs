using RootTools.Memory;
using System.Windows.Controls;

namespace Root_Memory
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
            m_memoryTool.OnChangeTool += M_memoryTool_OnChangeTool;
            InitTabControl();
        }

        private void M_memoryTool_OnChangeTool()
        {
            InitTabControl();
        }

        void InitTabControl()
        {
            tabControlMemory.Items.Clear();
            foreach (MemoryPool pool in m_memoryTool.m_aPool)
            {
                TabItem item = new TabItem();
                item.Header = pool.p_id;
                item.Content = pool.p_ui;
                tabControlMemory.Items.Add(item);
            }
        }
    }
}

