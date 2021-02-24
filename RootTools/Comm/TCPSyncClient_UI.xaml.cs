using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.Comm
{
    /// <summary>
    /// TCPSyncClient_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TCPSyncClient_UI : UserControl
    {
        public TCPSyncClient_UI()
        {
            InitializeComponent();
        }

        TCPSyncClient m_client;
        public void Init(TCPSyncClient client)
        {
            m_client = client;
            this.DataContext = client;
            commLogUI.Init(client.m_commLog);
            treeRootUI.Init(client.m_treeRoot);
            m_client.RunTree(Tree.eMode.Init);
        }
    }
}
