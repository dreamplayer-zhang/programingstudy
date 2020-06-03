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

        private void ListViewLog_LayoutUpdated(object sender, EventArgs e)
        {
            p_nCount = listViewLog.Items.Count;
        }

        int _nCount = 0;
        int p_nCount
        {
            set
            {
                if (_nCount == value) return;
                _nCount = value;
                if (value > 0) listViewLog.ScrollIntoView(listViewLog.Items[value - 1]);
                //listViewLog.Focus();
            }
        }
    }
}

