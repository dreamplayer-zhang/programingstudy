using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.Comm
{
    /// <summary>
    /// TCPSyncServer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TCPSyncServer_UI : UserControl
    {
        public TCPSyncServer_UI()
        {
            InitializeComponent();
        }

        TCPSyncServer m_server;
        public void Init(TCPSyncServer server)
        {
            m_server = server;
            DataContext = server;
            commLogUI.Init(server.m_commLog);
            treeRootUI.Init(server.m_treeRoot);
            m_server.RunTree(Tree.eMode.Init);
        }
    }
}
