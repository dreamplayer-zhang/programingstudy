using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.Comm
{
    /// <summary>
    /// TCPAsyncServer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TCPAsyncServer_UI : UserControl
    {
        public TCPAsyncServer_UI()
        {
            InitializeComponent();
        }

        TCPAsyncServer m_server;
        public void Init(TCPAsyncServer server)
        {
            m_server = server;
            DataContext = server;
            commLogUI.Init(server.m_commLog);
            treeRootUI.Init(server.m_treeRoot);
            m_server.RunTree(Tree.eMode.Init);
        }

    }
}
