﻿using RootTools;
using RootTools.Comm;
using RootTools.Module;
using RootTools.Trees;
using System.Net.Sockets;
using System.Text;

namespace Root.Module
{
    public class TestServer : ModuleBase
    {
        #region ToolBox
        TCPIPServer m_tcpip;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_tcpip, this, "Server");
            if (bInit) m_tcpip.EventReciveData += M_tcpip_EventReciveData;
        }
        #endregion

        #region TCPIP
        byte[] m_aBuf = new byte[2000000];
        string m_sBuf = ""; 
        void InitBuffer()
        {
            for (int n = 0; n < 2000000; n++) m_aBuf[n] = (byte)'a';
            m_sBuf = Encoding.Default.GetString(m_aBuf); 
        }

        private void M_tcpip_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            m_tcpip.Send(m_sBuf);
        }
        #endregion

        #region override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        public TestServer(string id, IEngineer engineer)
        {
            InitBuffer(); 
            InitBase(id, engineer);
        }

        #region ModuleRun

        #endregion
    }
}
