using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace RootTools.Memory
{
    /// <summary>
    /// MemoryPool_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MemoryPool_UI : UserControl
    {
        public MemoryPool_UI()
        {
            InitializeComponent();
        }

        MemoryPool m_memoryPool;
        public void Init(MemoryPool memoryPool)
        {
            m_memoryPool = memoryPool;
            DataContext = memoryPool;
            memoryViewerUI.Init(memoryPool.m_viewer);
            treeRootUI.Init(m_memoryPool.m_treeRoot);
            m_memoryPool.RunTree(Tree.eMode.Init); 
        }

        private void ListViewGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MemoryGroup group = (MemoryGroup)listViewGroup.SelectedItem;
            m_memoryPool.m_sSelectedGroup = (group == null) ? "" : group.p_id;
            m_memoryPool.RunTree(Tree.eMode.Init);
        }

        private void textBoxGB_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            DependencyProperty property = TextBox.TextProperty;
            BindingExpression binding = BindingOperations.GetBindingExpression((TextBox)sender, property);
            if (binding != null) binding.UpdateSource(); 
        }
    }
}
