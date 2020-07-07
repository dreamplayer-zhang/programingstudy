
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
            this.DataContext = m_server;
            treeRootUI.Init(m_server.m_treeRoot);
            m_server.RunTree(Tree.eMode.Init);

            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (m_server.m_aSocket.Count == tabComm.Items.Count) return;
            foreach (TCPIPServer.TCPSocket tcpSocket in m_server.m_aSocket) AddCommLog(tcpSocket);
        }

        void AddCommLog(TCPIPServer.TCPSocket tcpSocket)
        {
            foreach (TabItem item in tabComm.Items)
            {
                if ((string)item.Header == tcpSocket.p_id) return;
            }
            CommLog_UI commLogUI = new CommLog_UI();
            commLogUI.Init(tcpSocket.m_commLog);
            TabItem newItem = new TabItem();
            newItem.Header = tcpSocket.p_id;
            newItem.Content = commLogUI;
            tabComm.Items.Add(newItem);
        }
    }
}
