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
        public event TCPIPServer.OnReciveData EventReceiveData;
        public event TCPIPServer.OnAccept EventAccept;
        public event TCPIPServer.OnConnect EventConnect;

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
            server.EventReciveData += EventReceiveDataFunc;
            server.EventAccept += EventAcceptFunc;

            m_bIsServer = true;
        }

        public TCPIPComm_VEGA_D(TCPIPClient client)
        {
            m_client = client;
            client.EventReciveData += EventReceiveDataFunc;
            client.EventConnect += EventConnectFunc;

            m_bIsServer = false;
        }

        bool m_bIsServer = true;

        public enum Command
        {
            None,
            LineStart,
            LineEnd,
            RcpName,
            ScanInfo,
            Result,
            resume,
            InspStatus,
        }

        // start command
        public const string PARAM_NAME_OFFSETX = "OFFSETX";
        public const string PARAM_NAME_OFFSETY = "OFFSETY";
        public const string PARAM_NAME_SCANDIR = "DIR";
        public const string PARAM_NAME_FOV = "FOV";
        public const string PARAM_NAME_OVERLAP = "OVERLAP";
        public const string PARAM_NAME_RCPNAME = "RCPNAME";
        public const string PARAM_NAME_LINE = "LINE";
        public const string PARAM_NAME_TOTALSCANLINECOUNT = "TOTALSCANLINECOUNT";
        public const string PARAM_NAME_CURRENTSCANLINE = "CURRENTSCANLINE";
        public const string PARAM_NAME_STARTSCANLINE = "STARTSCANLINE";
        public const string PARAM_NAME_LEFTTOP_ALIGN_MARKER_POS_X = "LEFTTOP_ALIGN_MARKER_POS_X";
        public const string PARAM_NAME_LEFTTOP_ALIGN_MARKER_POS_Y = "LEFTTOP_ALIGN_MARKER_POS_Y";
        public const string PARAM_NAME_LEFTBOT_ALIGN_MARKER_POS_X = "LEFTBOT_ALIGN_MARKER_POS_X";
        public const string PARAM_NAME_LEFTBOT_ALIGN_MARKER_POS_Y = "LEFTBOT_ALIGN_MARKER_POS_Y";
        public const string PARAM_NAME_INSPENDLINE = "INSPENDLINE";

        public enum eParamName
        {
            OffsetX,
            OffsetY,
            Scandir,
            FOV,
            Overlap,
            RcpName,
            Line,
            TotalScanLineCount,
            CurrentScanLine,
            StartScanLine,
            LeftTop_Align_Key_Pos_X,
            LeftTop_Align_Key_Pos_Y,
            LeftBot_Align_Key_Pos_X,
            LeftBot_Align_Key_Pos_Y,
            InspEndLine,

            RCP_Left,
            RCP_Top,
            RCP_Right,
            RCP_Bottom,
            RCP_AlignPosX,
            RCP_AlignPosY,
            RCP_FirstDieLeft,
            RCP_FirstDieRight,
            RCP_SecondDieLeft,
            RCP_LastDieRight,
            RCP_FirstDieBottom,
            RCP_FirstDieTop,
            RCP_SecondDieBottom,
            RCP_LastDieTop,
            RCP_TriggerSizeX,
            RCP_TriggerSizeY,
            RCP_RectSizeX,
            RCP_RectSizeY,
            RCP_Padding,
            RCP_AfterEffectMorpology,
            RCP_AfterEffectBright,
            RCP_AfterEffectHistogram,
            RCP_ConsiderLinearPosY,
            RCP_LoadScore,
            RCP_DefectImagePath,
            RCP_D2DThresholdGV,
            RCP_SurfaceGV_Min,
            RCP_SurfaceGV_Max,
            RCP_DefectImagePadding,
            RCP_DefectSizeX,
            RCP_DefectSizeY,
            RCP_MergeLength,
            RCP_SwatheLength,
            RCP_SwatheIgnore,
            RCP_SwathePadding,
            RCP_StartSwathe,
            RCP_SwatheMaxCount,
        }

        const string COMMAND_NAME = "CMD";

        public bool ParseMessage(byte[] aBuf, int nSize, ref int nStartIdx, ref Command cmd, Dictionary<string, string> mapParam)
        {
            string sRecv = Encoding.Default.GetString(aBuf, nStartIdx, nSize - nStartIdx);
            if (sRecv.Length <= 0)
                return false;

            // ';'으로 메세지 단위 구분
            int msgSeperatorIdx = sRecv.IndexOf(';');
            if (msgSeperatorIdx == -1)
                msgSeperatorIdx = sRecv.Length; // ';' 문자가 없을 경우

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

        private void EventReceiveDataFunc(byte[] aBuf, int nSize, Socket socket)
        {
            if(EventReceiveData != null)
                EventReceiveData(aBuf, nSize, socket);
        }

        private void EventAcceptFunc(Socket socket)
        {
            if (EventAccept != null)
                EventAccept(socket);
        }

        private void EventConnectFunc(Socket socket)
        {
            if (EventConnect != null)
                EventConnect(socket);
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
