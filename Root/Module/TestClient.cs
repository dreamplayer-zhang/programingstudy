using RootTools;
using RootTools.Comm;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Root.Module
{
    public class TestClient : ModuleBase
    {
        #region ToolBox
        TCPIPClient m_tcpip;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_tcpip, this, "Client");
            if (bInit)
            {
                //m_tcpip.Co
                m_tcpip.EventReciveData += M_tcpip_EventReciveData;
            }
        }
        #endregion

        #region TCPIP
        StopWatch m_swTCPIP = new StopWatch(); 
        private void M_tcpip_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            p_sInfo = "Time : " + m_swTCPIP.ElapsedMilliseconds.ToString(); 
        }

        public void Send(string sSend)
        {
            m_swTCPIP.Start(); 
            m_tcpip.Send(sSend); 
        }
        #endregion
    }
}
