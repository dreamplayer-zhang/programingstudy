using RootTools.Module;
using RootTools.Trees;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Root_Pine2.Module
{
    /// <summary>
    /// Transfer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Transfer_UI : UserControl
    {
        public Transfer_UI()
        {
            InitializeComponent();
        }

        Transfer m_transfer;
        public void Init(Transfer transfer)
        {
            m_transfer = transfer;
            DataContext = transfer;
            treeRootUI.Init(transfer.m_treeRootQueue);
            transfer.RunTreeQueue(Tree.eMode.Init);
        }

        public void OnTimer()
        {
            switch (m_transfer.p_eState)
            {
                case ModuleBase.eState.Init: Background = Brushes.White; break;
                case ModuleBase.eState.Home: Background = Brushes.MediumPurple; break;
                case ModuleBase.eState.Ready: Background = Brushes.LightGreen; break;
                case ModuleBase.eState.Run: Background = Brushes.Yellow; break;
                case ModuleBase.eState.Error: Background = Brushes.OrangeRed; break;
            }
            textBlockA.Text = (m_transfer.m_gripper.p_infoStrip != null) ? m_transfer.m_gripper.p_infoStrip.p_id : "";
            textBlockB.Text = (m_transfer.m_pusher.p_infoStrip != null) ? m_transfer.m_pusher.p_infoStrip.p_id : "";
            bool bInspect = (m_transfer.m_pusher.p_infoStrip != null) ? m_transfer.m_pusher.p_infoStrip.p_bInspect : false;
            gridPusher.Background = bInspect ? Brushes.Orange : Brushes.Beige;
            OnRunTree();
        }

        int[] m_nQueue = new int[2] { 0, 0 };
        private void OnRunTree()
        {
            if ((m_transfer.m_qModuleRun.Count == m_nQueue[0]) && (m_transfer.m_qModuleRemote.Count == m_nQueue[1])) return;
            m_nQueue[0] = m_transfer.m_qModuleRun.Count;
            m_nQueue[1] = m_transfer.m_qModuleRemote.Count;
            m_transfer.RunTreeQueue(Tree.eMode.Init);
        }

        private void gridInfo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (m_transfer.p_eState)
            {
                case ModuleBase.eState.Init: m_transfer.p_eState = ModuleBase.eState.Home; break;
                case ModuleBase.eState.Error: m_transfer.Reset(); break;
            }
        }
    }
}
