using RootTools.Trees;
using System;
using System.Windows.Controls;

namespace RootTools.Gem.XGem
{
    /// <summary>
    /// XGem_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class XGem_UI : UserControl
    {
        public XGem_UI()
        {
            InitializeComponent();
        }

        XGem m_xGem = null; 
        public void Init(XGem xGem)
        {
            m_xGem = xGem;
            this.DataContext = xGem;
            listViewLog.ItemsSource = xGem.m_aLog;
            treeRootUI.Init(xGem.m_treeRoot);
            xGem.RunTree(Tree.eMode.Init); 
            listViewLog.LayoutUpdated += ListViewLog_LayoutUpdated;
        }

        int m_nCount = 0;
        private void ListViewLog_LayoutUpdated(object sender, EventArgs e)
        {
            int nCount = listViewLog.Items.Count;
            if (m_nCount == nCount) return;
            m_nCount = nCount;
            listViewLog.SelectedIndex = nCount - 1;
            listViewLog.ScrollIntoView(listViewLog.SelectedItem);
            ListViewItem item = listViewLog.ItemContainerGenerator.ContainerFromIndex(m_nCount - 1) as ListViewItem;
            if (item == null) return;
            item.Focus();
        }
    }
}

