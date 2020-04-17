using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Net;
using System.Net.Sockets;
using RootTools.Trees;

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

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            try
            {
                m_server.Send(textBox.Text, m_server.GetSocket((IPEndPoint)comboBoxSocket.SelectedItem));
            }
            catch
            {

            }
        }

        void CheckConnect()
        {
            if (m_server.GetClientSocketList().Count != comboBoxSocket.Items.Count) comboBoxSocket.Items.Clear();
            foreach (Socket socket in m_server.GetClientSocketList())
            {
                bool bExsit = false;
                for (int n = 0; n < comboBoxSocket.Items.Count; n++)
                {
                    IPEndPoint comboSocket = (IPEndPoint)comboBoxSocket.Items[n];
                    if (comboSocket.ToString() == socket.LocalEndPoint.ToString())
                    {
                        bExsit = true;
                        break;
                    }
                }
                if (!bExsit)
                {
                    comboBoxSocket.Items.Add(socket.LocalEndPoint);
                }
            }
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            CheckConnect();
            CheckCommLog();
        }

        #region List Log
        List<TCPIPServer.CommLog> m_aCommLog = new List<TCPIPServer.CommLog>();
        void CheckCommLog()
        {
            if (m_server.m_aCommLog.Count == 0) return;
            while (m_server.m_aCommLog.Count > 0)
            {
                m_aCommLog.Insert(0, m_server.m_aCommLog.Dequeue());
                if (m_aCommLog.Count > 1024) m_aCommLog.RemoveAt(m_aCommLog.Count - 1);
            }
            listViewLog.ItemsSource = null;
            listViewLog.ItemsSource = m_aCommLog;
        }
        #endregion
    }
}
