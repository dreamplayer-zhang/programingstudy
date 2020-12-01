using Root_EFEM.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Root_EFEM
{
    /// <summary>
    /// InfoWaferWTR_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InfoWaferWTR_UI : UserControl
    {
        public InfoWaferWTR_UI()
        {
            InitializeComponent();
        }

        IWTR m_wtr; 
        public void Init(IWTR wtr)
        {
            m_wtr = wtr;
            DataContext = wtr; 
        }

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(m_wtr.p_id, null);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            treeRootUI.Init(m_treeRoot);
            RunTree(Tree.eMode.Init);
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
        }

        public void RunTree(Tree.eMode eMode)
        {
            m_treeRoot.p_eMode = eMode;
            //if (m_wtr.p_infoWafer != null) m_wtr.p_infoWafer.RunTree(m_treeRoot.GetTree(m_wtr.p_infoWafer.p_id));
            //forget
        }
        #endregion

        #region Button Click
        private void buttonUpperAramAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonUpperArmRemove_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonLowerAramAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonLowerArmRemove_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
