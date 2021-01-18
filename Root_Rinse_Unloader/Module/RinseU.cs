using RootTools;
using RootTools.Comm;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Root_Rinse_Unloader.Module
{
    public class RinseU : ModuleBase
    {
        #region eRunMode
        public enum eRunMode
        {
            Magazine,
            Stack
        }
        string[] m_asRunMode = Enum.GetNames(typeof(eRunMode)); 

        eRunMode _eMode = eRunMode.Magazine;
        public eRunMode p_eMode
        {
            get { return _eMode; }
            set
            {
                if (_eMode == value) return;
                _eMode = value;
                OnPropertyChanged();
            }
        }

        double _widthStrip = 77;
        public double p_widthStrip
        {
            get { return _widthStrip; }
            set
            {
                if (_widthStrip == value) return;
                _widthStrip = value;
                OnPropertyChanged();
            }
        }

        int _iMagazin = 0;
        public int p_iMagazine
        {
            get { return _iMagazin; }
            set
            {
                if (_iMagazin == value) return;
                _iMagazin = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region ToolBox
        TCPIPServer m_tcpip; 
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_tcpip, this, "TCPIP");
            if (bInit) 
            {
                EQ.m_EQ.OnChanged += M_EQ_OnChanged; //forget
                m_tcpip.EventReciveData += M_tcpip_EventReciveData;
            }
        }

        private void M_EQ_OnChanged(_EQ.eEQ eEQ, dynamic value)
        {
            //m_remote.RemoteSend(Remote.eProtocol.EQ, eEQ.ToString(), value.ToString());
        }
        #endregion

        #region Protocol
        public enum eCmd
        {
            Unknown,
            SetMode,
        }
        public string[] m_asCmd = Enum.GetNames(typeof(eCmd)); 

        public class Protocol
        {
            public string p_sCmd
            {
                get { return m_id + "," + m_eCmd.ToString() + "," + m_value.ToString(); }
            }

            public string m_id; 
            public eCmd m_eCmd;
            public dynamic m_value; 
            public Protocol(string id, eCmd eCmd, dynamic value)
            {
                m_id = id;
                m_eCmd = eCmd;
                m_value = value; 
            }
        }
        #endregion

        #region Thread Send
        Protocol m_protocolSend = null;
        Queue<Protocol> m_qProtocol = new Queue<Protocol>();
        bool m_bRunSend = false;
        Thread m_threadSend;
        void InitThread()
        {
            m_threadSend = new Thread(new ThreadStart(RunThreadSend));
            m_threadSend.Start();
        }

        void RunThreadSend()
        {
            m_bRunSend = true;
            Thread.Sleep(1000);
            while (m_bRunSend)
            {
                Thread.Sleep(10);
                if (m_qProtocol.Count > 0)
                {
                    Protocol protocol = m_qProtocol.Peek(); 
                    if (protocol.m_id != p_id)
                    {
                        m_qProtocol.Dequeue();
                        m_tcpip.Send(protocol.p_sCmd);
                    }
                    else if (m_protocolSend == null)
                    {
                        m_protocolSend = m_qProtocol.Dequeue();
                        m_tcpip.Send(m_protocolSend.p_sCmd);
                    }
                }
            }
        }

        public Protocol AddProtocol(string id, eCmd eCmd, dynamic value)
        {
            Protocol protocol = new Protocol(id, eCmd, value);
            m_qProtocol.Enqueue(protocol);
            return protocol; 
        }
        #endregion

        #region TCPIP
        private void M_tcpip_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            try
            {
                string sRead = Encoding.Default.GetString(aBuf, 0, nSize);
                string[] asRead = sRead.Split(',');
                if (asRead.Length < 3)
                {
                    p_sInfo = "Invalid Protocol";
                    return;
                }
                eCmd eCmd = GetCmd(asRead[1]);
                if (asRead[0] != p_id)
                {
                    switch (eCmd)
                    {
                        case eCmd.SetMode:
                            SetMode(asRead[2]);
                            AddProtocol(asRead[0], eCmd, asRead[2]); 
                            break;
                    }
                }
                else
                {

                }

            }
            finally { }
        }

        eCmd GetCmd(string sCmd)
        {
            for (int n = 0; n < m_asCmd.Length; n++)
            {
                if (sCmd == m_asCmd[n]) return (eCmd)n;
            }
            return eCmd.Unknown; 
        }

        void SetMode(string sMode)
        {
            for (int n = 0; n < m_asRunMode.Length; n++)
            {
                if (sMode == m_asRunMode[n]) p_eMode = (eRunMode)n;
            }
        }
        #endregion

        public RinseU(string id, IEngineer engineer)
        {
            p_id = id;
            InitBase(id, engineer);
        }

    }
}
