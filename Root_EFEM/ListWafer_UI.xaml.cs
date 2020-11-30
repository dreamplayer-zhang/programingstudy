using Root_EFEM.Module;
using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace Root_EFEM
{
    /// <summary>
    /// ListWafer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ListWafer_UI : UserControl
    {
        public ListWafer_UI()
        {
            InitializeComponent();
        }

        IWTRChild m_module; 
        public void Init(IWTRChild module)
        {
            m_module = module;
            DataContext = module; 
        }

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTreeGrab()
        {
            m_treeRoot = new TreeRoot(m_module.p_id, null);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.Init); 
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
        }

        public void RunTree(Tree.eMode eMode)
        {
            m_treeRoot.p_eMode = eMode;
            foreach (InfoWafer infoWafer in m_module.p_aInfoWafer)
            {
                if (infoWafer != null) infoWafer.RunTree(m_treeRoot.GetTree(infoWafer.p_id)); 
            }
        }
        #endregion

        #region Button
        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RunTree(Tree.eMode.Init); 
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
