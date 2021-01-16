using RootTools;
using RootTools.Comm;
using RootTools.Module;
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
            SetMode,
        }

        public enum eReply
        {
            OK,
            Command,
            InvalidProtocol,
            InvalidCommand,
            InvalidValue,
        }

        public class Protocol
        {
            public string p_sCmd
            {
                get { return m_id + "," + m_eCmd.ToString() + "," + m_value.ToString(); }
            }

            public eReply IsReply(string sRead)
            {
                string[] asRead = sRead.Split(',');
                if (asRead.Length < 3) return eReply.InvalidProtocol; 
                if (asRead[0] != m_id) return eReply.Command;
                if (asRead[1] != m_eCmd.ToString()) return eReply.InvalidCommand;
                if (asRead[2] != m_value.ToString()) return eReply.InvalidValue; 
                return eReply.OK; 
            }

            string m_id; 
            eCmd m_eCmd;
            dynamic m_value; 
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
                if ((m_protocolSend == null) && (m_qProtocol.Count > 0))
                {
                    m_protocolSend = m_qProtocol.Dequeue();
                    m_tcpip.Send(m_protocolSend.p_sCmd); 
                    Thread.Sleep(10);
                    if (p_sInfo != "OK")
                    {
                        m_protocolSend = null;
                        m_qProtocol.Clear();
                    }
                }
            }
        }

        public Protocol AddProtocol(eCmd eCmd, dynamic value)
        {
            Protocol protocol = new Protocol(p_id, eCmd, value);
            m_qProtocol.Enqueue(protocol);
            return protocol; 
        }
        #endregion

        #region TCPIP
        private void M_tcpip_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            try
            {
                string sMsg = Encoding.Default.GetString(aBuf, 0, nSize);
//                switch ()
//                if (m_protocolSend.IsReply(sMsg))
            }
            finally { }
        }

        #endregion

        public RinseU(string id, IEngineer engineer)
        {
            p_id = id;
            InitBase(id, engineer);
        }

    }
}
