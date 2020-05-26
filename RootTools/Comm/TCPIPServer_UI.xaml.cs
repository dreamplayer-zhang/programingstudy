using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Net;
using System.Net.Sockets;
using RootTools.Trees;
using RootTools.Comm;

namespace RootTools
{
    /// <summary>
    /// TCPIPServer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TCPIPServer_UI : UserControl
    {
        DispatcherTimer m_timer = new DispatcherTimer();
        TCPIPServer m_server;

        public TCPIPServer_UI()
        {
            InitializeComponent();
        }

        public void Init(TCPIPServer server)
        {
            m_server = server;
            treeRootUI.Init(m_server.m_treeRoot);
            this.DataContext = m_server;
            m_server.RunTree(Tree.eMode.Init);

            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        List<CommLog_UI> m_aCommLogUI = new List<CommLog_UI>(); 
        void CheckConnect()
        {
            if ((m_server.m_aSocket.Count + 1) == tabComm.Items.Count) return;
            m_aCommLogUI.Clear(); 
            AddCommLog(m_server.m_commLog);
            foreach (TCPIPServer.TCPSocket tcpSocket in m_server.m_aSocket) AddCommLog(tcpSocket.m_commLog);
            tabComm.Items.Clear();
            foreach (CommLog_UI ui in m_aCommLogUI) tabComm.Items.Add(ui); 
        }

        void AddCommLog(CommLog commLog)
        {
            foreach (CommLog_UI ui in tabComm.Items)
            {
                if (ui.m_commLog.m_comm.p_id == commLog.m_comm.p_id)
                {
                    m_aCommLogUI.Add(ui);
                    return; 
                }
            }
            CommLog_UI commLogUI = new CommLog_UI();
            commLogUI.Init(commLog);
            m_aCommLogUI.Add(commLogUI); 
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            CheckConnect();
        }
    }
}
