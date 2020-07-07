
using RootTools.Comm;
using RootTools.Trees;
using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RootTools
{
    /// <summary>
    /// TCPIPServer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TCPIPServer_UI : UserControl
    {
        public TCPIPServer_UI()
        {
            InitializeComponent();
        }

        TCPIPServer m_server;
        public void Init(TCPIPServer server)
        {
            m_server = server;
            this.DataContext = server;
            commLogUI.Init(server.m_commLog);
            treeRootUI.Init(server.m_treeRoot);
            m_server.RunTree(Tree.eMode.Init);
        }
    }
}
