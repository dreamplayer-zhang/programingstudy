using RootTools;
using RootTools.Trees;
using System.Windows.Controls;

namespace Root_Wind
{
    /// <summary>
    /// InfoCarrier_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InfoCarrier_UI : UserControl
    {
        public InfoCarrier_UI()
        {
            InitializeComponent();
        }

        InfoCarrier m_infoCarrier; 
        public void Init(InfoCarrier infoCarrier)
        {
            m_infoCarrier = infoCarrier;
            this.DataContext = infoCarrier;
            treeWaferUI.Init(infoCarrier.m_treeRootWafer);
            infoCarrier.RunTreeWafer(Tree.eMode.Init);
            treeUI.Init(infoCarrier.m_treeRoot);
            infoCarrier.RunTree(Tree.eMode.Init);
            comboBoxIndex.ItemsSource = infoCarrier.m_asInfoWafer; 
        }

        int GetIndex(string sWafer)
        {
            for (int n = 0; n < m_infoCarrier.m_asInfoWafer.Count; n++)
            {
                if (sWafer == m_infoCarrier.m_asInfoWafer[n]) return n; 
            }
            return -1; 
        }

        private void buttonAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (EQ.p_bSimulate == false) return;
            if (comboBoxIndex.SelectedValue == null) return;
            string sWafer = comboBoxIndex.SelectedValue.ToString();
            int nIndex = GetIndex(sWafer);
            if (nIndex < 0) return;
            if (m_infoCarrier.GetInfoWafer(nIndex) != null) return;
            m_infoCarrier.SetInfoWafer(nIndex);
            m_infoCarrier.RunTreeWafer(Tree.eMode.Init); 
        }

        private void buttonRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (EQ.p_bSimulate == false) return;
            if (comboBoxIndex.SelectedValue == null) return;
            string sWafer = comboBoxIndex.SelectedValue.ToString();
            int nIndex = GetIndex(sWafer);
            if (nIndex < 0) return;
            if (m_infoCarrier.GetInfoWafer(nIndex) == null) return;
            m_infoCarrier.SetInfoWafer(nIndex, null);
            m_infoCarrier.RunTreeWafer(Tree.eMode.Init);
        }

        private void buttonRemoveAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_infoCarrier.ClearInfoWafer();
            m_infoCarrier.RunTreeWafer(Tree.eMode.Init);
        }

        private void buttonRefresh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_infoCarrier.RunTreeWafer(Tree.eMode.Init);
        }

        private void buttonStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (comboBoxIndex.SelectedValue == null) return;
            string sWafer = comboBoxIndex.SelectedValue.ToString();
            m_infoCarrier.StartProcess(sWafer);
            m_infoCarrier.RunTreeWafer(Tree.eMode.Init);
        }

        private void buttonStartAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_infoCarrier.StartAllProcess();
            m_infoCarrier.RunTreeWafer(Tree.eMode.Init);
        }
    }
}
