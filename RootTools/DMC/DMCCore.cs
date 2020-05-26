using RootTools.Comm;
using System;
using System.Net.Sockets;
using System.Windows.Controls;

namespace RootTools.DMC
{
    public class DMCCore : NotifyProperty, ITool, IComm
    {
        #region Property
        public string p_id { get; set; }

        public UserControl p_ui
        {
            get
            {
                DMCCore_UI ui = new DMCCore_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        #region TCPIP
        public TCPIPClient m_tcpip;
        private void M_tcpip_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            throw new NotImplementedException();
        }
        #endregion

        public string Send(string sMsg)
        {
            throw new NotImplementedException();
        }

        public void ThreadStop()
        {
            m_tcpip.ThreadStop(); 
        }

        Log m_log;
        public DMCCore(string id, Log log)
        {
            p_id = id;
            m_log = log;
            m_tcpip = new TCPIPClient(id, log);
            m_tcpip.EventReciveData += M_tcpip_EventReciveData;
        }
    }
}
