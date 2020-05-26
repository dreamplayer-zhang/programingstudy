using RootTools.Comm;
using System;
using System.Windows.Controls;

namespace RootTools.ZoomLens
{
    public class ZoomLens : ObservableObject, ITool
    {
        #region Property
        public string p_id { get; set; }

        string _sInfo = "OK";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                RaisePropertyChanged();
                if (value == "OK") return;
                m_log.Error(value);
            }
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                ZoomLens_UI ui = new ZoomLens_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        int m_nCurrentPos;
        public int p_nCurrentPos
        {
            get { return m_nCurrentPos; }
            set { SetProperty(ref m_nCurrentPos, value); }
        }
        public Log m_log;
        public RS232 m_rs232;
        public ZoomLens(string id, Log log)
        {
            p_id = id;
            m_log = log;

            m_rs232 = new RS232(id, m_log);
            m_rs232.OnRecieve += M_rs232_OnRecieve;
        }

        private void M_rs232_OnRecieve(string sRead)
        {
            // 반환값이 있는 명령에 대한 처리
            switch (eLastCommand)
            {
                case eCommand.SET_SPEED:
                    {
                        break;
                    }
                case eCommand.GET_SENSOR_STATUS:
                    {
                        break;
                    }
                case eCommand.GET_PARAMETER:
                    {
                        break;
                    }
                case eCommand.SET_PARAMETER:
                    {
                        break;
                    }
                case eCommand.GET_STATUS:
                    {
                        string strResult = m_rs232.m_sRead;
                        string strPos = strResult.Substring(0, 8);
                        p_nCurrentPos = int.Parse(strPos);
                        break;
                    }
                case eCommand.TOGGLE_RESPONSE:
                    {
                        break;
                    }
            }
        }

        public void ThreadStop()
        {
            //TODO : 구현하거나 지우거나
        }

        #region Command
        // ZoomLens Manual Link = https://teams.microsoft.com/l/file/C90A7049-FA40-48B4-8FB3-D1746BA76443?tenantId=9dff1e71-4997-4d9b-84cc-ad9df6d78706&fileType=pdf&objectUrl=https%3A%2F%2Fati5344.sharepoint.com%2Fsites%2FSW1%2FShared%20Documents%2FGeneral%2F14.VEGA%2FQT-ADL1_S3%ED%86%B5%EC%8B%A0%EC%A0%9C%EC%96%B4%ED%8E%B8_%ED%95%9C%EA%B8%80.pdf&baseUrl=https%3A%2F%2Fati5344.sharepoint.com%2Fsites%2FSW1&serviceName=teams&threadId=19:6c82bda1676547f8bfbb3ce429ac3755@thread.skype&groupId=b795c55c-d063-4717-b935-4a39197b5bcb

        eCommand eLastCommand = eCommand.NONE;
        enum eCommand 
        { 
            NONE,
            SET_ABSOLUTE_COORDINATE,
            ABSOLUTE_GO, 
            SET_SPEED, 
            GET_SPEED, 
            EMERGENCY_STOP, 
            FLASH_BACKUP,
            GO, 
            HOME,
            GET_SENSOR_STATUS, 
            SET_JOG_DIRECTION,
            JOG_MOVE,
            STOP,
            SET_STEP,
            STEP_MOVE,
            GET_PARAMETER,
            SET_PARAMETER,
            GET_STATUS,
            CHANGE_COODINATE,
            SET_WAIT,
            TOGGLE_RESPONSE,
            RESET,
            RESTART
        }
        public void SetAbsoluteCoordinate(int nAbsoluteCoordinate)
        {
            string strResult;
            string strCommand = "A:";
            string strAxisName = "A";
            string strAbsoluteCoordinate = nAbsoluteCoordinate.ToString();
            strResult = strCommand + strAxisName + strAbsoluteCoordinate;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.SET_ABSOLUTE_COORDINATE;
        }
        public void AbsoluteGo(int nAbsoluteCoordinate)
        {
            string strResult;
            string strCommand = "AGO:";
            string strAxisName = "A";
            string strAbsoluteCoordinate = nAbsoluteCoordinate.ToString();
            strResult = strCommand + strAxisName + strAbsoluteCoordinate;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.ABSOLUTE_GO;
        }
        public void SetSpeed(int nLowSpeed, int nHighSpeed, int nAcceleration)
        {
            string strResult;
            string strCommand = "D:";
            string strAxisName = "A";
            string strComma = ",";
            strResult = strCommand + strAxisName + nLowSpeed.ToString() + strComma + nHighSpeed.ToString() + strComma + nAcceleration.ToString();
            string strRet = m_rs232.Send(strResult.Trim());
            if (strRet == "OK") FlashBackup();
            eLastCommand = eCommand.SET_SPEED;
        }

        public void GetSpeed()
        {
            string strResult;
            string strCommand = "D:";
            string strAxisName = "A";
            string strReturn = "R";
            strResult = strCommand + strAxisName + strReturn;
            m_rs232.Send(strResult.Trim());
            // Received Data = <저속pps>,<고속pps>,<가감속시간ms> = 5byte 1byte 5byte 1byte 4byte = 16byte
            // Example : 01500,09000,0800
            eLastCommand = eCommand.GET_SPEED;
        }

        public void EmergencyStop()
        {
            string strResult;
            string strCommand = "E:";
            strResult = strCommand;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.EMERGENCY_STOP;
        }

        public void FlashBackup()
        {
            string strResult;
            string strCommand = "F:";
            strResult = strCommand;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.FLASH_BACKUP;
        }

        public void Go()
        {
            string strResult;
            string strCommand = "G:";
            string strAxisName = "A";
            strResult = strCommand + strAxisName;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.GO;
        }

        public void Home()
        {
            string strResult;
            string strCommand = "H:";
            string strAxisName = "A";
            strResult = strCommand + strAxisName;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.HOME;
        }

        public void GetSensorStatus()
        {
            string strResult;
            string strCommand = "I:";
            string strAxisName = "A";
            strResult = strCommand + strAxisName;
            m_rs232.Send(strResult.Trim());
            // Received Data = <+리미트><-리미트><원점전><원점> = 1byte 1byte 1byte 1byte = 4byte
            // Example : 0110
            eLastCommand = eCommand.GET_SENSOR_STATUS;
        }

        public void SetJogDirection(bool bDirection)
        {
            string strResult;
            string strCommand = "J:";
            string strAxisName = "A";
            string strDirection = bDirection ? "+" : "-";
            strResult = strCommand + strAxisName + strDirection;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.SET_JOG_DIRECTION;
        }

        public void JogMove(bool bDirection)
        {
            string strResult;
            string strCommand = "JGO:";
            string strAxisName = "A";
            string strDirection = bDirection ? "+" : "-";
            strResult = strCommand + strAxisName + strDirection;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.JOG_MOVE;
        }

        public void Stop()
        {
            string strResult;
            string strCommand = "L:";
            string strAxisName = "A";
            strResult = strCommand + strAxisName;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.STOP;
        }

        public void SetStep(int nPulse)
        {
            string strResult;
            string strCommand = "M:";
            string strAxisName = "A";
            string strpulse = nPulse.ToString();
            strResult = strCommand + strAxisName + strpulse;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.SET_STEP;
        }

        public void StepMove(int nPulse)
        {
            string strResult;
            string strCommand = "MGO:";
            string strAxisName = "A";
            string strPulse = nPulse.ToString();
            strResult = strCommand + strAxisName + strPulse;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.STEP_MOVE;
        }

        public void GetParameter(int nParamNumber)
        {
            string strResult;
            string strCommand = "P:";
            string strParamNumber = nParamNumber.ToString("00");
            string strReceive = "R";
            strResult = strCommand + strParamNumber + strReceive;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.GET_PARAMETER;
        }

        public void SetParameter(int nParamNumber, int? nValue1 = null, int? nValue2 = null, int? nValue3 = null, int? nValue4 = null)
        {
            string strResult;
            string strCommand = "P:";
            string strAxisName = "A";
            string strParamNumber = nParamNumber.ToString("00");
            string strComma = ",";
            strResult = strCommand + strParamNumber;
            if (nParamNumber > 50) // 시스템 파라미터(50번대) -> P:<파라미터No.>,<설정값1>,<설정값2>,...
            {
                if (nValue1 != null) strResult += strComma + nValue1.ToString();
                if (nValue2 != null) strResult += strComma + nValue2.ToString();
                if (nValue3 != null) strResult += strComma + nValue3.ToString();
                if (nValue4 != null) strResult += strComma + nValue4.ToString();
            }
            else // A축의 설정 파라미터
            {
                strResult += (strAxisName + nValue1.ToString());
            }
            m_rs232.Send(strResult.Trim());
            FlashBackup();
            Restart();
            eLastCommand = eCommand.SET_PARAMETER;
        }

        public enum eZoomLensStatus { COORD_AND_STATUS, COORD_ONLY, STATUS_ONLY}
        public void GetStatus(int nRequestNmuber)
        {
            string strResult;
            string strCommand = "Q:";
            string strAxisName = "A";
            string strRequestNumber = nRequestNmuber.ToString("0");
            strResult = strCommand + strAxisName + strRequestNumber;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.GET_STATUS;
        }

        public void ChangeCoordinate(int nCoordinate)
        {
            string strResult;
            string strCommand = "R:";
            string strAxisName = "A";
            string strCoordinate = nCoordinate.ToString();
            strResult = strCommand + strAxisName + strCoordinate;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.CHANGE_COODINATE;
        }

        public void SetWait(int nMiliSec)
        {
            string strResult;
            string strCommand = "W:";
            string strTime = (nMiliSec / 100).ToString();
            strResult = strCommand + strTime;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.SET_WAIT;
        }

        enum eResponseStatus { NONE, EXIST, REQUEST};
        public void ToggleResponse(int nResponse)
        {
            string strResult;
            string strCommand = "X:";
            string strResponse = nResponse.ToString("0");
            strResult = strCommand + strResponse;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.TOGGLE_RESPONSE;
        }

        public void Reset()
        {
            string strResult;
            string strCommand = "RESET:";
            strResult = strCommand;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.RESET;
        }

        public void Restart()
        {
            string strResult;
            string strCommand = "RESTA:";
            strResult = strCommand;
            m_rs232.Send(strResult.Trim());
            eLastCommand = eCommand.RESTART;
        }
        #endregion

    }
}
