using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.Comm
{
    /// <summary>
    /// TCPAsyncClient_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TCPAsyncClient_UI : UserControl
    {
        public TCPAsyncClient_UI()
        {
            InitializeComponent();
        }

        TCPAsyncClient m_client;
        public void Init(TCPAsyncClient client)
        {
            m_client = client;
            this.DataContext = client;
            commLogUI.Init(client.m_commLog);
            treeRootUI.Init(client.m_treeRoot);
            m_client.RunTree(Tree.eMode.Init);
        }
    }
}
