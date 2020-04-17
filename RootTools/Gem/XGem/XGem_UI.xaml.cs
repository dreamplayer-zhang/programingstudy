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

        int nViewCnt = 0;
        private void ListViewLog_LayoutUpdated(object sender, EventArgs e)
        {
            int count = listViewLog.Items.Count;
            if (nViewCnt == count) return;
            nViewCnt = count;
            listViewLog.SelectedIndex = count - 1;
            listViewLog.ScrollIntoView(listViewLog.SelectedItem);
            ListViewItem item = listViewLog.ItemContainerGenerator.ContainerFromIndex(nViewCnt - 1) as ListViewItem;
            if (item == null) return;
            item.Focus();
        }
    }
}

