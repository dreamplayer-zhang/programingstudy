using RootTools;
using RootTools.Comm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_D_IPU.Module
{
    public class TCPIPComm_VEGA_D
    {
        public event TCPIPServer.OnReciveData EventReciveData;

        TCPIPServer m_server;
        TCPIPClient m_client;

        public TCPIPServer server
        {
            get { return m_server; }
            set { m_server = value; }
        }

        public TCPIPClient client
        {
            get { return m_client; }
            set { m_client = value; }
        }

        public TCPIPComm_VEGA_D(TCPIPServer server)
        {
            m_server = server;
            server.EventReciveData += EventReceiveData;
            m_bIsServer = true;
        }

        public TCPIPComm_VEGA_D(TCPIPClient client)
        {
            m_client = client;
            client.EventReciveData += EventReceiveData;
            m_bIsServer = false;
        }

        bool m_bIsServer = true;

        public enum Command
        {
            none,
            start,
            end,
            rcpname,
            result,
            alive,
            ready,
        }

        const string COMMAND_NAME = "CMD";

        public bool ParseMessage(byte[] aBuf, int nSize, ref int nStartIdx, ref Command cmd, Dictionary<string, string> mapParam)
        {
            string sRecv = Encoding.Default.GetString(aBuf, nStartIdx, nSize - nStartIdx);
            if (sRecv.Length <= 0)
                return false;

            // ';'으로 메세지 단위 구분
            int msgSeperatorIdx = sRecv.IndexOf(';');
            string strFullMsg = sRecv.Substring(0, msgSeperatorIdx);
            nStartIdx = msgSeperatorIdx + 1;

            // ','으로 파라미터 단위 구분
            string[] arrStrMsgUnit = strFullMsg.Split(',');

            // Command 얻어오기
            string strCmd = arrStrMsgUnit[0];
            string[] arrStrCmd = strCmd.Split(':');

            if (arrStrCmd.Length <= 1)
                return false;

            if (arrStrCmd[0] != COMMAND_NAME)
                return false;

            try
            {
                cmd = (Command)Enum.Parse(typeof(Command), arrStrCmd[1]);
            }
            catch(ArgumentException ex)
            {
                // arrStrCmd[1] 문자열이 빈문자열이거나
                // Command에 정의된 Enum 값으로 파싱될 수 없는 경우
                return false;
            }

            // Parameter 얻어오기
            for(int i=1; i< arrStrMsgUnit.Length; i++)
            {
                string strParam = arrStrMsgUnit[i];

                string[] arrStrParam = strParam.Split(':');

                if (arrStrParam.Length <= 1)
                    continue;

                mapParam.Add(arrStrParam[0], arrStrParam[1]);
            }

            return true;
        }

        private void EventReceiveData(byte[] aBuf, int nSize, Socket socket)
        {
            EventReciveData(aBuf, nSize, socket);
        }

        public void SendMessage(Command cmd)
        {
            // 메세지 만들기
            string strMsg = "";

            // Command 추가
            strMsg += "CMD:" + cmd.ToString();

            // 마무리 메세지 구분자 추가
            strMsg += ";";

            // 메세지 전달
            if (m_bIsServer)
                m_server.Send(strMsg);
            else
                m_client.Send(strMsg);
        }

        public void SendMessage(Command cmd, Dictionary<string, string> mapParam)
        {
            // 메세지 만들기
            string strMsg = "";

            // Command 추가
            strMsg += "CMD:" + cmd.ToString();

            // Parameter 추가
            foreach(KeyValuePair<string, string> items in mapParam)
            {
                strMsg += ",";
                strMsg += items.Key;
                strMsg += ":";
                strMsg += items.Value;
            }

            // 마무리 메세지 구분자 추가
            strMsg += ";";

            // 메세지 전달
            if (m_bIsServer)
                m_server.Send(strMsg);
            else
                m_client.Send(strMsg);
        }

        public bool IsConnected()
        {
            if (m_bIsServer && m_server != null)
                return m_server.IsConnected();
            else if (!m_bIsServer && m_client != null)
                return m_client.IsConnected();
            else
                return false;
        }
    }
}
