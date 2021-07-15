using GEM_XGem300Pro;
using RootTools.GAFs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;

//기존 XGem.cs와 차이점(시나리오에 맞추기 위해 따로 분리)
//p_ePresentSensor 변경 될 경우, SendCarrierOnOff(Material On/Off) 후 SendCarrierPresentSensor On/Off(Transferblocked) 하기 위해 두개 순서 변경

namespace RootTools.Gem.XGem
{
    public class XGem_New : NotifyProperty, IGem, IToolSet
    {
        #region eCommunicate
        public enum eCommunicate
        {
            NULL,
            DISABLE,
            WAITCR,
            WAITDELAY,
            WAITCRA,
            COMMUNICATING,
        }
        eCommunicate _eComm = eCommunicate.DISABLE;
        public eCommunicate p_eComm
        {
            get { return _eComm; }
            set
            {
                if (_eComm == value) return;
                p_sInfo = "eCommunicate : " + _eComm.ToString() + " -> " + value.ToString();
                _eComm = value;
                OnPropertyChanged();
            }
        }

        void InitEventCommunicate()
        {
            m_xGem.OnGEMCommStateChanged += M_xGem_OnGEMCommStateChanged;
        }

        private void M_xGem_OnGEMCommStateChanged(long nState)
        {
            LogRcv("OnGEMCommStateChanged", nState);
            p_eComm = (eCommunicate)nState;
        }

        void RunTreeCommunicate(Tree tree)
        {
            tree.Set(p_eComm, p_eComm, "State", "Communicate State", true, true);
        }
        #endregion

        #region eControl
        public enum eControl
        {
            NULL,
            OFFLINE,
            ATTEMPTONLINE,
            HOSTOFFLINE,
            LOCAL,
            ONLINEREMOTE,
        }

        XGem.eControl _eReqControl = XGem.eControl.OFFLINE;

        public XGem.eControl p_eReqControl
        {
            get { return _eReqControl; }
            set
            {
                if (m_treeRoot.p_eMode == Tree.eMode.RegRead) return;
                if (_eReqControl == value) return;
                p_sInfo = "eReqControl : " + _eReqControl.ToString() + " -> " + value.ToString();
                _eReqControl = value;
                OnPropertyChanged();
                long nError = 0;
                switch (_eReqControl)
                {
                    case XGem.eControl.OFFLINE: nError = m_bStart ? m_xGem.GEMReqOffline() : 0; break;
                    case XGem.eControl.LOCAL: nError = m_bStart ? m_xGem.GEMReqLocal() : 0; break;
                    case XGem.eControl.ONLINEREMOTE: nError = m_bStart ? m_xGem.GEMReqRemote() : 0; break;
                    default: return;
                }
                LogSend(nError, "Change Control State = " + _eReqControl.ToString());
            }
        }

        XGem.eControl _eControl = XGem.eControl.NULL;
        public XGem.eControl p_eControl
        {
            get { return _eControl; }
            set
            {
                if (_eControl == value) return;
                p_sInfo = "eControl : " + _eControl.ToString() + " -> " + value.ToString();
                _eControl = value;
                OnPropertyChanged();
                foreach (GemCarrierBase carrier in m_aCarrier)
                {
                    if (value == XGem.eControl.ONLINEREMOTE) carrier.p_eAccessLP = GemCarrierBase.eAccessLP.Auto;
                    carrier.RunTree(Tree.eMode.Init);
                }
            }
        }

        public bool p_bOffline
        {
            get
            {
                switch (p_eControl)
                {
                    case XGem.eControl.NULL:
                    case XGem.eControl.OFFLINE:
                    case XGem.eControl.LOCAL:
                    case XGem.eControl.HOSTOFFLINE: return true;
                    default: return false;
                }
            }
        }

        void InitEventControl()
        {
            m_xGem.OnGEMControlStateChanged += M_xGem_OnGEMControlStateChanged;
        }

        private void M_xGem_OnGEMControlStateChanged(long nState)
        {
            LogRcv("OnGEMControlStateChanged", nState);
            p_eControl = (XGem.eControl)nState;
        }

        void RunTreeControl(Tree tree)
        {
            p_eReqControl = (XGem.eControl)tree.Set(p_eReqControl, p_eReqControl, "Request", "Control Request State");
            tree.Set(p_eControl, p_eControl, "State", "Control State", true, true);
        }
        #endregion

        #region eState
        public enum eState
        {
            INIT,
            IDLE,
            SETUP,
            READY,
            EXCUTE,
        }
        eState _eState = eState.INIT;
        public eState p_eState
        {
            get { return _eState; }
            set
            {
                if (_eState == value) return;
                p_sInfo = "eState : " + _eState.ToString() + " -> " + value.ToString();
                _eState = value;
            }
        }

        void InitEventState()
        {
            m_xGem.OnXGEMStateEvent += M_xGem_OnXGEMStateEvent;
        }

        private void M_xGem_OnXGEMStateEvent(long nState)
        {
            LogRcv("OnXGEMStateEvent", nState);
            p_eState = (eState)nState;
            if (p_eState == eState.EXCUTE) p_bEnable = true;
        }

        bool _bEnable = false;
        public bool p_bEnable
        {
            get { return _bEnable; }
            set
            {
                if (_bEnable == value) return;
                p_sInfo = "p_bEnable : " + _bEnable.ToString() + " -> " + value.ToString();
                _bEnable = value;
                int nEnable = _bEnable ? 1 : 0;
                LogSend(m_xGem.GEMSetEstablish(nEnable), "GEMSetEstablish", nEnable);
            }
        }

        void RunTreeState(Tree tree)
        {
            tree.Set(p_eState, p_eState, "Equipment", "Equipment State", true, true);
            tree.Set(p_bEnable, p_bEnable, "Enable", "Equipment Enable", true, true);
        }
        #endregion

        #region Log & Error
        const int c_lLog = 256;
        public enum eType
        {
            Send,
            Receive,
            Info,
            Error,
        }

        public class LogData
        {
            string _sTime = "";
            public string p_sTime
            {
                get { return _sTime; }
            }

            string _sMsg = "";
            public string p_sMsg
            {
                get { return _sMsg; }
            }

            Brush _bColor = Brushes.Black;
            public Brush p_bColor
            {
                get { return _bColor; }
            }

            public LogData(eType type, string sMsg)
            {
                _sTime = DateTime.Now.ToLongTimeString();
                _sMsg = sMsg;
                switch (type)
                {
                    case eType.Send: _bColor = Brushes.Black; break;
                    case eType.Receive: _bColor = Brushes.DarkGreen; break;
                    case eType.Info: _bColor = Brushes.Gray; break;
                    default: _bColor = Brushes.Red; break;
                }
            }
        }
        public ObservableCollection<LogData> m_aLog = new ObservableCollection<LogData>();
        public Queue<LogData> m_qLog = new Queue<LogData>();

        DispatcherTimer m_timer = new DispatcherTimer();
        void InitLogTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick; ;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (m_qLog.Count == 0) return;
            int l = m_qLog.Count;
            for (int n = 0; n < l; n++) m_aLog.Add(m_qLog.Dequeue());
            while (m_aLog.Count > c_lLog) m_aLog.RemoveAt(0);
            RunTree(Tree.eMode.Init);
        }

        string _sInfo = "OK";
        string p_sInfo
        {
            set
            {
                if (value == "OK") return;
                if (value == _sInfo) return;
                _sInfo = value;
                m_log.Info("Info = " + _sInfo);
                m_qLog.Enqueue(new LogData(eType.Info, _sInfo));
            }
        }

        string _sLastError = "OK";
        public string p_sLastError
        {
            get { return _sLastError; }
            set
            {
                if (_sLastError == value) return;
                if (value == "OK") return;
                _sLastError = value;
                m_log.Error("Error = " + _sLastError);
                m_qLog.Enqueue(new LogData(eType.Error, _sLastError));
            }
        }

        string LogSend(long nError, string sCmd, params object[] objs)
        {
            string sLog = sCmd;
            foreach (object obj in objs) sLog += ", " + obj.ToString();
            m_log.Info(" --> " + sLog);
            m_qLog.Enqueue(new LogData(eType.Send, sLog));
            if (nError >= 0) return "OK";
            p_sLastError = sCmd + " Error # = " + nError.ToString() + " : " + GetErrerString(nError);
            return p_sLastError;
        }

        void LogRcv(string sCmd, params object[] objs)
        {
            string sLog = sCmd;
            foreach (object obj in objs) sLog += ", " + obj.ToString();
            m_log.Info(" <-- " + sLog);
            m_qLog.Enqueue(new LogData(eType.Receive, sLog));
        }

        string GetErrerString(long nError)
        {
            switch (nError)
            {
                case 0: return "No Error";
                case -10001: return "Already Started";              // Already initialized 이미 XGem300Pro 이 초기화되어 있는데 다시 불린 상태입니다.
                case -10002: return "Invalid Thread";               // Socket is not initialized.
                case -10003: return "Invalid DomDoc";               // Fail to load xml. XGem300Pro Process 로 부터 xml format이 아닌 message를 받았을 때 발생합니다.
                case -10004: return "Invalid Attribute";            // Fail to invalid attribute. XGem300Pro Process 로부터 받은 message의 attribute가 맞지 않을 경우 발생합니다.
                case -10005: return "Invalid Command";              // Fail to invalid command. XGem300Pro Process 로부터 invalid message를 받았을 경우 발생합니다.
                case -10006: return "Create Failed DomDoc";         // Fail to initiate DomDocument Dom 생성 실패 시 발생합니다.
                case -10007: return "Invalid Argument Value";       // Invalid argument value. 함수의 인자의 값이 유효하지 않은 값 입니다. 인자의 값을 확인하시기 바랍니다.
                case -10008: return "Not Ready XGem";               // Not ready XGem. XGem300Pro state 가 execute 가 아닌 상태에서 함수가 호출되었습니다.
                case -10009: return "Not Initialized";              // Not initialized yet. XGem300Pro 이 아직 초기화 되어 있지 않은 상태입니다. Initialize() 함수가 실패했거나 불리지 않은 상태입니다.
                case -10010: return "Not Started";                  // Not started yet. XGem300Pro 의 내부 프로세스가 아직 시작되지 않은 상태입니다. Start()가 호출되지 않았거나 실패한 상태입니다.
                case -10011: return "Not Connected";                // Not used.
                case -10012: return "Read Config File Error";       // Fail to read config file. cfg 파일을 읽기 실패하였습니다. cfg 파일의 경로와 item 을 확인하기 바랍니다.
                case -10013: return "Invalid Message Format";       // Invalid message format. Complex  type 의  message 를XGem300Pro Process 로 전송 시 발생하며 invalid message format 을 전송하려고 할 때 발생합니다.
                case -10014: return "Delete Complex List Error";    // Not used.
                case -10015: return "Item not Found";               // Not found item. data item 을 찾을 수 없습니다.
                case -10016: return "Item Type Mismatch";           // Mismatch item type 얻고자 하는 data type과 맞지 않습니다.
                case -10017: return "Item Count Mismatch";          // Mismatch item count. item 의 개수가 맞지 않습니다.
                case -10018: return "Invalid Message ID";           // Invalid message.
                case -10019: return "Argument Out of Range";        // Argument is out of range 인자 값의 범위를 벗어 납니다.
                case -10020: return "Invalid Parameter";            // Invalid Parameter.유효하지 않은 Parameter name
                case -10021: return "License Error";                // Not used.
                case -10022: return "Fail to Create Window";        // Fail to create window. window 생성 실패했습니다. 회사로 문의 바랍니다.
                case -10023: return "Invalid Receive Data";         // Not used.
                case -10024: return "Mismatch Message Name";        // Mismatch message name. 얻고자 하는 message name 과 XGem300Pro Process 에서 받은 message name 과 동일하지 않습니다.
                case -10026: return "VID does not use file memory"; // VID does not use file memory.
                case -10027: return "VID file memory is included";  // VID for using file memory is included.
                case -10028: return "Invalid VID";                  // invalid VID 유효하지 않은 VID.
                case -10029: return "Exceed Maximum Item Size";     // exceed maximum item size.
                case -10030: return "Fail to Create Mutex";         // Fail to create mutex.
                case -10031: return "Invalid SECS2 Message";        // invalid SECS2 message. SECS2 message format error.
                case -10032: return "Fail to Delete MSG List";      // Fail to delete msg list
                case -10033: return "Create Event Handle Error";    // Fail to create API’s event handle.
                case -10034: return "Fail to Start Process";        // Fail to start XGem300Pro process.
                case -30001: return "Fail to Read Rtartup Info";    // Fail to read startup information
                case -30002: return "Fail to Initialize XCom";      // Fail to initialize XCom
                case -30003: return "Fail to Initialize EQComm";    // Fail to initialize EQComm
                case -30004: return "Fail to Initialize EDAComm";   // Fail to initialize EDAComm
                case -30005: return "Fail to Start XCom";           // Fail to start XCom
                case -30006: return "Fail to Start EQComm";         // Fail to start EQComm
                case -30007: return "Fail to Start EDAComm";        // Fail to start EDAComm
                case -30008: return "Disconnected with Equipment";  // disconnected with Equipment
                case -30009: return "Disconnected with EDAComm";
                case -30010: return "Control Off Line";             // Can't send message because controloffline.
                case -30011: return "Control State not Chamged";    // Control state can't be changed to ONLINE_LOCAL in OFFLINE state.
                case -30012: return "File Create Error";            // cfg, sml file not created
                case -30013: return "Fail to Open Database";        // Fail to open database
                case -30014: return "Error in Execute SQL";         // Error in Execute SQL.
                case -30015: return "Error in Open SQL";            // Error in Open SQL.
                case -30016: return "CEID disabled";                // Ceid disabled
                case -30017: return "Report Buffer is Full";        // Report buffer is full.
                case -30018: return "System does not Use Spooling"; // This system does not use spooling now.
                case -30019: return "Spool Buffer is Full";         // Spool buffer is full.
                case -30020: return "Spool Undefined";              // Spool undefined
                case -30021: return "Spool is not Active";          // Spool is not active, so message cannot spooling.
                case -30022: return "Operating to 200mm spec";      // XGem300Pro is operating to 200mm spec, so message of 300mm spec can't send.
                case -30023: return "File does not Exist";          // File does not exist.
                case -30024: return "Shared Memory Start Error";    // Failed to open/start shared memory.
                case -30025: return "Shared Memory Stop Error";     // Failed to close/stop shared memory.
                case -30201: return "Fail Create DomDoc2";
                case -30202: return "Invalid DomDoc2";
                case -30203: return "Invalid Command2";
                case -30204: return "Invalid Attribute2";
                case -30205: return "Invalid Set Command";
                case -30251: return "VID does not Exist";           // VID does not exist.
                case -30252: return "ALID does not Exist";          // ALID does not exist.
                case -30253: return "CEID does not Exist";          // CEID does not exist.
                case -30254: return "RPTID does not Exist";         // RPTID does not exist.
                case -30255: return "Limit VID does not Exist";     // Limit VID does not exist.
                case -30256: return "Limit ID does not Exist";      // Limit ID does not exist.
                case -30257: return "Data Item does not Exist";     // Data Item does not exist.
                case -30258: return "RCmd does not Exist";          // RCmd does not exist.
                case -30259: return "Stream does not Exist";        // Stream does not exist.
                case -30260: return "Function does not Exist";      // Function does not exist.
                case -30261: return "SECS Param does not Exist";    // SECSParameters does not exist.
                case -30262: return "Invalid Error Code";
                case -30263: return "ECID does not Exist";          // ECID does not exist.
                case -30264: return "Format is Invalid";            // Format is invalid.
                case -30265: return "ConfigItem does not Exist";    // ConfigItem does not exist.
                case -30266: return "Invalid Structure";
                case -30267: return "PORTID does not Exist";        // PORTID does not exist.
                case -30268: return "Invalid EQ MSG ID";
                case -30269: return "Data Type is Invalid";         // Data Type is invalid.
                case -30270: return "PPID does not Exist";          // PPID does not exist.
                case -30271: return "Invalid State";                // State is invalid.
                case -30272: return "TRID does not Exist";          // TRID does not exist
                case -30273: return "Buffer does not Found";        // Buffer does not found.
                case -30274: return "Value is out of Range";        // Value is out of range.
                case -30275: return "SVID does not Exist";          // SVID does not exist.
                default: return "Unknown Error";
            }
        }
        #endregion

        #region GEM
        public long SetAlarm(ALID alid, bool bSet)
        {
            long nSet = bSet ? 1 : 0;
            long nError = p_bEnable ? m_xGem.GEMSetAlarm(alid.p_nID, nSet) : 0;
            LogSend(nError, "GEMSetAlarm", alid.p_nID, nSet);
            p_sInfo = "SetAlarm " + alid.p_sModule + "." + alid.p_id + " = " + nSet.ToString();
            return nError;
        }

        public long SetCEID(CEID ecv)
        {
            if (p_bEnable == false) return -1;
            long nError = p_bEnable ? m_xGem.GEMSetEvent(ecv.p_nID) : 0;
            LogSend(nError, "GEMSetEvent", ecv.p_nID);
            p_sInfo = "SetCEID " + ecv.p_sModule + "." + ecv.p_id;

            return nError;
        }

        public long SetCEID(long nCEID)
        {
            if (p_bEnable == false) return -1;
            long nError = p_bEnable ? m_xGem.GEMSetEvent(nCEID) : 0;
            LogSend(nError, "GEMSetEvent", nCEID);
            p_sInfo = "SetCEID " + nCEID;

            return nError;
        }

        long[] m_svID = new long[1];
        string[] m_svValue = new string[1];
        public long SetSV(SVID sv, dynamic value)
        {
            if (p_bEnable == false) return -1;
            m_svID[0] = sv.p_nID;
            m_svValue[0] = value.ToString();
            long nError = p_bEnable ? m_xGem.GEMSetVariable(1, m_svID, m_svValue) : 0;
            LogSend(nError, "GEMSetVariable", m_svID[0], m_svValue[0]);
            //p_sInfo = "SetSV " + sv.p_sModule + "." + sv.p_id + " : " + sv.p_value.ToString() + " -> " + value.ToString(); 
            return nError;
        }

        public long SetSV(long svid, dynamic value)
        {
            if (p_bEnable == false) return -1;
            m_svID[0] = svid;
            m_svValue[0] = value.ToString();
            long nError = p_bEnable ? m_xGem.GEMSetVariable(1, m_svID, m_svValue) : 0;
            LogSend(nError, "GEMSetVariable", m_svID[0], m_svValue[0]);
            //p_sInfo = "SetSV " + sv.p_sModule + "." + sv.p_id + " : " + sv.p_value.ToString() + " -> " + value.ToString(); 
            return nError;
        }
        #endregion

        #region Terminal Message
        void InitEventMessage()
        {
            m_xGem.OnGEMTerminalMessage += M_xGem_OnGEMTerminalMessage;
            m_xGem.OnGEMTerminalMultiMessage += M_xGem_OnGEMTerminalMultiMessage;
            m_xGem.OnSECSMessageReceived += M_xGem_OnSECSMessageReceived;
        }

        private void M_xGem_OnGEMTerminalMessage(long nTid, string sMsg)
        {
            //TODO : Terminal Message Parsing해서 Job Reserved 처리하기.
            //Terminal Message 예시
            /*
            -----------TKIN PASS------------------
            JOBID = PJ001
            CSTID = tstatd
            RECIPE = asdfasdf
            SLOT = 111110000000000
            -------------------------------------- =

            ----------JOB RESERVED------------ -

            ---------------------------------------
            // if (Job Reserved가 있으면) m_ceidJobReserved.Send();

            */
            if (sMsg.Contains("Reserved") == true)
            {
                SetCEID(8211);
            }

            LogRcv("OnGEMTerminalMessage", nTid, sMsg);
        }

        private void M_xGem_OnGEMTerminalMultiMessage(long nTid, long nCount, string[] psMsg)
        {
            LogRcv("OnGEMTerminalMultiMessage", nTid, nCount);
            for (int n = 0; n < nCount; n++) M_xGem_OnGEMTerminalMessage(nTid, psMsg[n]);
        }

        public event dgGemSECSMessageReceived OnGemSECSMessageReceived;
        private void M_xGem_OnSECSMessageReceived(long nObjectID, long nStream, long nFunction, long nSysbyte)
        {
            LogRcv("OnSECSMessageReceived", nObjectID, nStream, nFunction, nSysbyte);
            OnGemSECSMessageReceived(nObjectID, nStream, nFunction, nSysbyte);
        }
        #endregion

        #region DateTime
        public struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }
        [DllImport("kernel32")]
        public static extern int SetSystemTime(ref SYSTEMTIME lpSystemTime);

        void InitEventDateTime()
        {
            m_xGem.OnGEMReqDateTime += M_xGem_OnGEMReqDateTime;
            m_xGem.OnGEMReqGetDateTime += M_xGem_OnGEMReqGetDateTime;
        }

        private void M_xGem_OnGEMReqDateTime(long nMsgId, string sSystemTime)
        {
            LogRcv("OnGEMReqDateTime", nMsgId, sSystemTime);
            long nResult = 1;
            if (sSystemTime.Length == 14)
            {
                SYSTEMTIME syetemTime = new SYSTEMTIME();
                syetemTime.wYear = Convert.ToUInt16(sSystemTime.Substring(0, 4));
                syetemTime.wMonth = Convert.ToUInt16(sSystemTime.Substring(4, 2));
                syetemTime.wDay = Convert.ToUInt16(sSystemTime.Substring(6, 2));
                syetemTime.wHour = Convert.ToUInt16(sSystemTime.Substring(8, 2));
                syetemTime.wMinute = Convert.ToUInt16(sSystemTime.Substring(10, 2));
                syetemTime.wSecond = Convert.ToUInt16(sSystemTime.Substring(12, 2));
                if (SetSystemTime(ref syetemTime) != 0) nResult = 0;
            }
            long nError = m_xGem.GEMRspDateTime(nMsgId, nResult);
            LogSend(nError, "GEMRspDateTime", nMsgId, nResult);
        }

        private void M_xGem_OnGEMReqGetDateTime(long nMsgId)
        {
            LogRcv("OnGEMReqGetDateTime", nMsgId);
            string sTime = DateTime.Now.ToString("yyyyMMddhhmmss");
            long nError = m_xGem.GEMRspGetDateTime(nMsgId, sTime);
            LogSend(nError, "GEMRspGetDateTime", nMsgId, sTime);
        }
        #endregion

        #region Remote Command
        void InitEventRemoteCommand()
        {
            m_xGem.OnGEMReqRemoteCommand += M_xGem_OnGEMReqRemoteCommand;
        }

        public event dgGemRemoteCommand OnGemRemoteCommand;
        private void M_xGem_OnGEMReqRemoteCommand(long nMsgId, string sRcmd, long nCount, string[] psNames, string[] psVals)
        {
            LogRcv("OnGEMReqRemoteCommand", nMsgId, sRcmd, nCount, psNames[0], psVals[0]);
            Dictionary<string, string> dicParam = new Dictionary<string, string>();
            for (int n = 0; n < nCount; n++) dicParam.Add(psNames[n], psVals[n]);
            long[] pnResult = new long[nCount];
            for (int n = 0; n < nCount; n++) pnResult[n] = 0;
            OnGemRemoteCommand(sRcmd, dicParam, pnResult);
            long nError = m_xGem.GEMRspRemoteCommand(nMsgId, sRcmd, 1, nCount, psNames, pnResult);
        }
        #endregion

        #region GemCarrier
        List<GemCarrierBase> m_aCarrier = new List<GemCarrierBase>();

        public void AddGemCarrier(GemCarrierBase carrier)
        {
            if (FindGemCarrier(carrier.p_sLocID) != null) return;
            m_aCarrier.Add(carrier);
        }

        GemCarrierBase FindGemCarrier(string sLocID)
        {
            foreach (GemCarrierBase carrier in m_aCarrier)
            {
                if (carrier.p_sLocID == sLocID) return carrier;
            }
            return null;
        }

        GemCarrierBase GetGemCarrier(string sLocID)
        {
            foreach (GemCarrierBase carrier in m_aCarrier)
            {
                if (carrier.p_sLocID == sLocID) return carrier;
            }
            p_sLastError = "Can't Find Carrier : " + sLocID;
            return null;
        }

        string GetGemLocID(string sCarrierID)
        {
            foreach (GemCarrierBase carrier in m_aCarrier)
            {
                if (carrier.p_sCarrierID == sCarrierID) return carrier.p_sLocID;
            }
            p_sLastError = "Can't Find Carrier : " + sCarrierID;
            return null;
        }

        public void SendLPInfo(GemCarrierBase carrier)
        {
            long nError = m_xGem.CMSSetLPInfo(carrier.p_sLocID, (long)carrier.p_eReqTransfer, (long)carrier.p_eReqAccessLP, 0, (long)carrier.p_eReqAssociated, carrier.p_sCarrierID);
            LogSend(nError, "CMSSetLPInfo", carrier.p_sLocID, carrier.p_eTransfer, carrier.p_eAccessLP, 0, carrier.p_eReqAssociated, carrier.p_sCarrierID);
        }

        void InitEventGemCarrier()
        {
            m_xGem.OnCMSCarrierIDStatusChanged += M_xGem_OnCMSCarrierIDStatusChanged;
            m_xGem.OnCMSSlotMapStatusChanged += M_xGem_OnCMSSlotMapStatusChanged;
            m_xGem.OnCMSCarrierVerifySucceeded += M_xGem_OnCMSCarrierVerifySucceeded;
            m_xGem.OnCMSTransferStateChanged += M_xGem_OnCMSTransferStateChanged;
            m_xGem.OnCMSCarrierAccessStatusChanged += M_xGem_OnCMSCarrierAccessStatusChanged;
            m_xGem.OnCMSAccessModeStateChanged += M_xGem_OnCMSAccessModeStateChanged;
            m_xGem.OnCMSCarrierDeleted += M_xGem_OnCMSCarrierDeleted;
            m_xGem.OnCMSAssociationStateChanged += M_xGem_OnCMSAssociationStateChanged;
        }

        private void M_xGem_OnCMSCarrierIDStatusChanged(string sLocID, long nState, string sCarrierID)
        {
            LogRcv("OnCMSCarrierIDStatusChanged", sLocID, nState, sCarrierID);
            GemCarrierBase carrier = GetGemCarrier(sLocID);
            if (carrier == null) return;
            //p_sInfo = "eGemState : " + carrier.p_eGemState.ToString() + " -> " + ((GemCarrierBase.eGemState)nState).ToString(); 
            p_sInfo = "eStateCarrierID : " + carrier.p_eStateCarrierID.ToString() + " -> " + ((GemCarrierBase.eGemState)nState).ToString();
            carrier.p_eStateCarrierID = (GemCarrierBase.eGemState)nState;
            p_sInfo = "sCarrierID : " + carrier.p_sCarrierID + " -> " + sCarrierID;
            carrier.p_sCarrierID = sCarrierID;
        }

        private void M_xGem_OnCMSSlotMapStatusChanged(string sLocID, long nState, string sCarrierID)
        {
            LogRcv("OnCMSSlotMapStatusChanged", sLocID, nState, sCarrierID);
            GemCarrierBase carrier = GetGemCarrier(sLocID);
            if (carrier == null) return;
            //p_sInfo = "eSlotMapState : " + carrier.p_eSlotMapState.ToString() + " -> " + ((GemCarrierBase.eGemState)nState).ToString(); 
            p_sInfo = "eStateSlotMap : " + carrier.p_eStateSlotMap.ToString() + " -> " + ((GemCarrierBase.eGemState)nState).ToString();
            carrier.p_eStateSlotMap = (GemCarrierBase.eGemState)nState;
            p_sInfo = "sCarrierID : " + carrier.p_sCarrierID + " -> " + sCarrierID;
            carrier.p_sCarrierID = sCarrierID;
        }

        public void SendCarrierID(GemCarrierBase carrier, string sCarrierID)
        {
            long nError = m_xGem.CMSSetCarrierID(carrier.p_sLocID, sCarrierID, 0);
            LogSend(nError, "CMSSetCarrierID", carrier.p_sLocID, sCarrierID, 0);
        }

        public void SendSlotMap(GemCarrierBase carrier, List<GemSlotBase.eState> aMap)
        {
            string sMap = "";
            foreach (GemSlotBase.eState state in aMap)
            {
                char cSlot = (char)state;
                cSlot += '0';
                sMap += cSlot;
            }
            long nError = m_xGem.CMSSetSlotMap(carrier.p_sLocID, sMap, carrier.p_sCarrierID, 0);
            LogSend(nError, "CMSSetSlotMap", carrier.p_sLocID, sMap, carrier.p_sCarrierID, 0);
        }

        private void M_xGem_OnCMSCarrierVerifySucceeded(long nVerifyType, string sLocID, string sCarrierID, string sSlotMap, long nCount, string[] psLotID, string[] psSubstrateID, string sUsage)
        {
            LogRcv("OnCMSCarrierVerifySucceeded", nVerifyType, sLocID, sCarrierID, sSlotMap, nCount, psLotID[0], psSubstrateID[0], sUsage);
            GemCarrierBase carrier = GetGemCarrier(sLocID);
            if (carrier == null) return;
            switch (nVerifyType)
            {
                case 0:
                    //carrier.m_bReqLoad = true; //Docking 시퀀스 내에 CarrierID Read 포함되어 있어 OHT 종료 후로 이동.
                    break;
                case 1:
                    if (sSlotMap.Length < nCount) return;
                    for (int n = 0; n < nCount; n++)
                    {
                        char ch = sSlotMap[n];
                        GemSlotBase.eState state = (GemSlotBase.eState)(ch - '0');
                        p_sLastError = carrier.SetSlotInfo(n, state, psLotID[n], psSubstrateID[n]);
                    }
                    break;
            }
        }

        private void M_xGem_OnCMSTransferStateChanged(string sLocID, long nState)
        {
            LogRcv("OnCMSTransferStateChanged", sLocID, nState);
            GemCarrierBase carrier = GetGemCarrier(sLocID);
            if (carrier == null) return;
            p_sInfo = "eTransfer : " + carrier.p_eTransfer.ToString() + " -> " + ((GemCarrierBase.eTransfer)nState).ToString();
            carrier.p_eTransfer = (GemCarrierBase.eTransfer)nState;
        }

        public void SendCarrierAccessing(GemCarrierBase carrier, GemCarrierBase.eAccess access)
        {
            long nErr = m_xGem.CMSSetCarrierAccessing(carrier.p_sLocID, (long)access, carrier.p_sCarrierID);
            LogSend(nErr, "CMSSetCarrierAccessing", carrier.p_sLocID, access, carrier.p_sCarrierID);
        }

        private void M_xGem_OnCMSCarrierAccessStatusChanged(string sLocID, long nState, string sCarrierID)
        {
            LogRcv("OnCMSCarrierAccessStatusChanged", sLocID, nState, sCarrierID);
            GemCarrierBase carrier = GetGemCarrier(sLocID);
            if (carrier == null) return;
            p_sInfo = "eAccess : " + carrier.p_eAccess.ToString() + " -> " + ((GemCarrierBase.eAccess)nState).ToString();
            carrier.p_eAccess = (GemCarrierBase.eAccess)nState;
        }

        public string SendCarrierAccessLP(GemCarrierBase carrier, GemCarrierBase.eAccessLP accessLP)
        {
            if (carrier.p_eTransfer == GemCarrierBase.eTransfer.OutOfService) return "Invalid AccessLP when eTransfer.OutOfService";
            long nErr = m_xGem.CMSReqChangeAccess((long)accessLP, carrier.p_sLocID);
            LogSend(nErr, "CMSSetCarrierAccessing", accessLP, carrier.p_sLocID);
            return "OK";
        }

        private void M_xGem_OnCMSAccessModeStateChanged(string sLocID, long nState)
        {
            LogRcv("OnCMSAccessModeStateChanged", sLocID, nState);
            GemCarrierBase carrier = GetGemCarrier(sLocID);
            if (carrier == null) return;
            p_sInfo = "eAccessLP : " + carrier.p_eAccessLP.ToString() + " -> " + ((GemCarrierBase.eAccessLP)nState).ToString();
            carrier.p_eAccessLP = (GemCarrierBase.eAccessLP)nState;
        }

        public string SendCarrierPresentSensor(GemCarrierBase carrier, bool bPresent)
        {
            //AccessLP Auto/Manual 모두 PresentSensor 사용
            //if (carrier.p_eAccessLP != GemCarrierBase.eAccessLP.Manual) return "Invalid Carrier PresentSensor when AccessLP = Auto";
            long nPresent = bPresent ? 1 : 0;
            //long nError = m_xGem.CMSSetPresenceSensor(carrier.p_sLocID, nPresent);
            long nError = m_xGem.CMSSetCarrierOnOff(carrier.p_sLocID, nPresent);

            return LogSend(nError, "CMSSetCarrierOnOff", carrier.p_sLocID, nPresent);
        }

        public void SendCarrierOn(GemCarrierBase carrier, bool bOn)
        {
            long nOn = bOn ? 1 : 0;
            //long nError = m_xGem.CMSSetCarrierOnOff(carrier.p_sLocID, nOn);
            
            long nError = m_xGem.CMSSetPresenceSensor(carrier.p_sLocID, nOn);
            LogSend(nError, "CMSSetPresenceSensor", carrier.p_sLocID, nOn);
        }

        public string CMSSetReadyToLoad(GemCarrierBase carrier)
        {
            if (carrier.p_eTransfer == GemCarrierBase.eTransfer.OutOfService) return "Invalid ReadyToLoad when eTransfer.OutOfService";
            long nError = m_xGem.CMSSetReadyToLoad(carrier.p_sLocID);
            return LogSend(nError, "CMSSetReadyToLoad", carrier.p_sLocID);
        }

        public string CMSSetReadyToUnload(GemCarrierBase carrier)
        {
            if (carrier.p_eTransfer == GemCarrierBase.eTransfer.OutOfService) return "Invalid ReadyToUnload when eTransfer.OutOfService";
            long nError = m_xGem.CMSSetReadyToUnload(carrier.p_sLocID);
            return LogSend(nError, "CMSSetReadyToUnload", carrier.p_sLocID);
        }

        public void CMSDelCarrierInfo(GemCarrierBase carrier)
        {
            long nError = m_xGem.CMSDelCarrierInfo(carrier.p_sCarrierID);
            LogSend(nError, "CMSDeleteCarrierInfo", carrier.p_sCarrierID);
        }

        private void M_xGem_OnCMSCarrierDeleted(string sCarrierID)
        {
            LogRcv("OnCMSCarrierDeleted", sCarrierID);
            foreach (GemCarrierBase carrier in m_aCarrier)
            {
                if (carrier.p_sCarrierID == sCarrierID)
                {
                    p_sInfo = "eReqTransfer : " + carrier.p_eReqTransfer.ToString() + " -> " + GemCarrierBase.eTransfer.ReadyToLoad.ToString();
                    carrier.p_eReqTransfer = GemCarrierBase.eTransfer.ReadyToLoad;
                }
            }
        }

        private void M_xGem_OnCMSAssociationStateChanged(string sLocID, long nState)
        {
            LogRcv("OnCMSAssociationStateChanged", sLocID, nState);
            GemCarrierBase carrier = GetGemCarrier(sLocID);
            if (carrier == null) return;
            p_sInfo = "eAssociated : " + carrier.p_eAssociated.ToString() + " -> " + ((GemCarrierBase.eAssociated)nState).ToString();
            carrier.p_eAssociated = (GemCarrierBase.eAssociated)nState;
        }

        public void RemoveCarrierInfo(string sLocID)
        {
            long nMsgID = 0;
            long nCarrier = 0;
            long nError = m_xGem.CMSGetAllCarrierInfo(ref nMsgID, ref nCarrier);
            LogSend(nError, "CMSGetAllCarrierInfo", nMsgID, nCarrier);
            string sLoc = "";
            for (int n = 0; n < nCarrier; n++)
            {
                nError = m_xGem.GetCarrierLocID(nMsgID, n, ref sLoc);
                LogSend(nError, "GetCarrierLocID", nMsgID, n, sLoc);
                if (sLoc == sLocID)
                {
                    string sCarrierID = "";
                    nError = m_xGem.GetCarrierID(nMsgID, n, ref sCarrierID);
                    LogSend(nError, "GetCarrierID", nMsgID, n, sCarrierID);
                    m_xGem.CMSDelCarrierInfo(sCarrierID);
                    return;
                }
            }
        }
        #endregion

        #region Process Job
        List<GemPJ> m_aPJ = new List<GemPJ>();

        GemPJ GetPJ(string sPJobID)
        {
            foreach (GemPJ pj in m_aPJ)
            {
                if (pj.m_sPJobID == sPJobID) return pj;
            }
            return null;
        }

        void InitEventProcessJob()
        {
            m_xGem.OnPJReqVerify += M_xGem_OnPJReqVerify;
            m_xGem.OnPJCreated += M_xGem_OnPJCreated;
            m_xGem.OnPJDeleted += M_xGem_OnPJDeleted;
            m_xGem.OnPJStateChanged += M_xGem_OnPJStateChanged;
            m_xGem.OnPJSettingUpStart += M_xGem_OnPJSettingUpStart;
            m_xGem.OnPJReqCommand += M_xGem_OnPJReqCommand;
        }

        private void M_xGem_OnPJReqVerify(long nMsgID, long nPJobCount, string[] psPJobID, long[] pnMtrlFormat, long[] pnAutoStart, long[] pnMtrlOrder, long[] pnMtrlCount, string[] psMtrlID, string[] psSlotInfo, long[] pnRcpMethod, string[] psRcpID, long[] pnRcpParCount, string[] psRcpParName, string[] psRcpParValue)
        {
            int iMtrl = 0;
            List<long> aErrorCode = new List<long>();
            List<string> asErrorMsg = new List<string>();
            LogRcv("OnPJReqVerify", nMsgID, nPJobCount);
            for (int n = 0; n < nPJobCount; n++)
            {
                LogRcv("OnPJReqVerify", nMsgID, n, psPJobID[n], pnMtrlFormat[n], pnAutoStart[n], pnMtrlOrder[n], pnMtrlCount[n], pnRcpMethod[n], psRcpID[n]);
                string[] sFiles = Directory.GetFiles(EQ.c_sPathRecipe, psRcpID[n] + "." + m_sRecipeExt, SearchOption.TopDirectoryOnly);
                if (sFiles.Length <= 0)
                {
                    aErrorCode.Add((long)GemPJ.eError.Invalid_AttibuteValue);
                    p_sLastError = "RecipeID not Found : " + psRcpID[n];
                    asErrorMsg.Add(p_sLastError);
                }
                for (int i = 0; i < pnMtrlCount[n]; i++, iMtrl++)
                {
                    LogRcv("OnPJReqVerify", nMsgID, n, psPJobID[n], psMtrlID[iMtrl], psSlotInfo[iMtrl]);
                    //GemCarrierBase carrier = GetGemCarrier(psMtrlID[iMtrl]); 
                    GemCarrierBase carrier = GetGemCarrier(GetGemLocID(psMtrlID[iMtrl]));
                    if (carrier == null)
                    {
                        aErrorCode.Add((long)GemPJ.eError.Invalid_AttibuteValue);
                        p_sLastError = "MatrialID not Found : " + psMtrlID[iMtrl] + " in Recipe : " + psRcpID[n];
                        asErrorMsg.Add(p_sLastError);
                    }
                    /* //PJReqVerifySlot 함수 내부에선 PJCreate 된 후 Slot의 상태를 확인하여, 순서 상 맞지 않아 주석 처리함.
                    else 
                    {
                        string sVerify = carrier.PJReqVerifySlot(psSlotInfo[iMtrl]); 
                        if (sVerify != "OK")
                        {
                            aErrorCode.Add((long)GemPJ.eError.Invalid_AttibuteValue);
                            p_sLastError = "Invalid Slot at " + psMtrlID[iMtrl] + " in Recipe : " + psRcpID[n] + " : " + sVerify;
                            asErrorMsg.Add(p_sLastError);
                        }
                    }
                    */
                }
            }
            long nResult = 1;
            if (aErrorCode.Count == 0)
            {
                nResult = 0;
                aErrorCode.Add((long)GemPJ.eError.NO_ERROR);
                asErrorMsg.Add("OnPJReqVerify Done");
            }
            long nError = m_xGem.PJRspVerify(nMsgID, nPJobCount, psPJobID, nResult, aErrorCode.Count, aErrorCode.ToArray(), asErrorMsg.ToArray());
            LogSend(nError, "PJRspVerify", nMsgID, nPJobCount, nResult, aErrorCode.Count);
        }

        private void M_xGem_OnPJCreated(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, string[] psRcpParValue)
        {
            LogRcv("OnPJCreated", sPJobID, psMtrlID[0], nAutoStart, sRcpID);
            PJDelete(sPJobID);
            GemPJ pj = new GemPJ(sPJobID, (GemPJ.eAutoStart)nAutoStart, sRcpID, m_log, m_sRecipeExt);
            m_aPJ.Add(pj);

            for (int n = 0; n < nMtrlCount; n++)
            {
                GemCarrierBase carrier = GetGemCarrier(GetGemLocID(psMtrlID[n]));
                if (carrier != null)
                {
                    for (int i = 0; i < psSlotInfo[n].Length; i++)
                    {
                        if (psSlotInfo[n][i] == '3') pj.SetSlotExist(carrier, i);
                    }
                }
            }

            long nError = m_xGem.PJSetState(sPJobID, (long)GemPJ.eState.Queued);
            LogSend(nError, "PJSetState", sPJobID, GemPJ.eState.Queued);
        }

        private void M_xGem_OnPJDeleted(string sPJobID)
        {
            LogRcv("OnPJDeleted", sPJobID);
            PJDelete(sPJobID);

            long nError = m_xGem.PJSetState(sPJobID, (long)GemPJ.eState.JobComplete);
            LogSend(nError, "PJSetState", sPJobID, GemPJ.eState.JobComplete);
        }

        void PJDelete(string sPJobID)
        {
            for (int n = m_aPJ.Count - 1; n >= 0; n--)
            {
                if (m_aPJ[n].m_sPJobID == sPJobID) m_aPJ.RemoveAt(n);
            }
        }

        private void M_xGem_OnPJStateChanged(string sPJobID, long nState)
        {
            LogRcv("OnPJStateChanged", sPJobID, nState);
            GemPJ pj = GetPJ(sPJobID);
            GemPJ.eState state = (GemPJ.eState)nState;
            pj.p_eState = state;
            switch (state)
            {
                case GemPJ.eState.SettingUp:
                    pj.SettingUp();
                    long nError = m_xGem.PJSettingUpCompt(sPJobID);
                    LogSend(nError, "PJSettingUpCompt", sPJobID);

                    break;
            }
        }

        private void M_xGem_OnPJSettingUpStart(string sPJobID)
        {
            LogRcv("OnPJSettingUpStart", sPJobID);
            LogSend(m_xGem.PJSettingUpStart(sPJobID), "OnPJSettingUpStart", sPJobID);
            long nError = m_xGem.PJSetState(sPJobID, (long)GemPJ.eState.SettingUp);
            LogSend(nError, "PJSetState", sPJobID, GemPJ.eState.SettingUp);
        }

        private void M_xGem_OnPJReqCommand(long nMsgID, string sPJobID, long nCommand)
        {
            LogRcv("OnPJReqCommand", nMsgID, sPJobID, nCommand);
            GemPJ.eCommand cmd = (GemPJ.eCommand)nCommand;
            GetPJ(sPJobID).p_eCommand = cmd;
            PJRspCommand(nMsgID, cmd, sPJobID, 1, 0, 0, "");
            Thread.Sleep(20);
        }

        void PJRspCommand(long nMsgId, GemPJ.eCommand cmd, string sPJobID, long nAck, long nErrorCount, long nErrorCode, string sError)
        {
            long[] aErrorCode = new long[1] { nErrorCode };
            string[] asError = new string[1] { sError };
            long nError = m_xGem.PJRspCommand(nMsgId, (long)cmd, sPJobID, nAck, nErrorCount, aErrorCode, asError);
            LogSend(nError, "PJRspCommand", nMsgId, cmd, sPJobID, nAck, nErrorCount, aErrorCode[0], asError[0]);

            nError = m_xGem.PJSetState(sPJobID, (long)GemPJ.eState.Processing);
            LogSend(nError, "PJSetState", sPJobID, GemPJ.eState.Processing);
        }

        public void SendPJComplete(string sPJobID)
        {
            long nError = m_xGem.PJSetState(sPJobID, (long)GemPJ.eState.ProcessingComplete);
            LogSend(nError, "PJSetState", sPJobID, GemPJ.eState.ProcessingComplete);
        }

        void RunTreeProcessJob(Tree tree)
        {
            if (m_aPJ.Count == 0)
                return;
            foreach (GemPJ pj in m_aPJ) pj.RunTree(tree.GetTree(pj.m_sPJobID));
        }

        #endregion

        #region Control Job
        GemCJ _cjRun = null;
        public GemCJ p_cjRun
        {
            get { return _cjRun; }
            set
            {
                if (_cjRun == value) return;
                _cjRun = value;
            }
        }

        public Queue<GemCJ> m_qCJ = new Queue<GemCJ>();
        public List<GemCJ> m_aCJFinish = new List<GemCJ>();

        GemCJ GetCJ(string sCJobID)
        {
            foreach (GemCJ cj in m_qCJ)
            {
                if (cj.m_sCJobID == sCJobID) return cj;
            }
            return null;
        }

        void InitEventControlJob()
        {
            m_xGem.OnCJCreated += M_xGem_OnCJCreated;
            m_xGem.OnCJDeleted += M_xGem_OnCJDeleted;
            m_xGem.OnCJStateChanged += M_xGem_OnCJStateChanged;
            m_xGem.OnCJRspSelect += M_xGem_OnCJRspSelect;
            m_xGem.OnGEMReqPPList += M_xGem_OnGEMReqPPList;
        }

        private void M_xGem_OnGEMReqPPList(long nMsgId)
        {
            string[] files = Directory.GetFiles(@"C:\Recipe", "*." + m_sRecipeExt);
            m_xGem.GEMRspPPList(nMsgId, files.Length, files);
        }

        private void M_xGem_OnCJCreated(string sCJobID, long nStartMethod, long nCountPRJob, string[] psPRJobID)
        {
            LogRcv("OnCJCreated", sCJobID, nStartMethod, nCountPRJob, psPRJobID[0]);
            CJDeleted(sCJobID);
            GemPJ.eAutoStart autoStart = (GemPJ.eAutoStart)nStartMethod;
            GemCJ cj = new GemCJ(sCJobID, autoStart, m_log);
            for (int n = 0; n < nCountPRJob; n++) cj.m_aPJ.Add(GetPJ(psPRJobID[n]));
            m_qCJ.Enqueue(cj);
            RunTree(Tree.eMode.Init);
        }

        private void M_xGem_OnCJDeleted(string sCJobID)
        {
            LogRcv("OnCJDeleted", sCJobID);
            CJDeleted(sCJobID);
            //m_xGem.CMSSetCarrierAccessing() //forget 종료조건 ?
            RunTree(Tree.eMode.Init);
        }

        void CJDeleted(string sCJobID)
        {
            if (p_cjRun != null && p_cjRun.m_sCJobID == sCJobID)
            {
                //m_aCJFinish.Add(p_cjRun);
                m_aCJFinish.Add(m_qCJ.Dequeue());
                p_cjRun = null;
                return;
            }
            GemCJ[] aCJ = m_qCJ.ToArray();
            m_qCJ.Clear();
            foreach (GemCJ cj in aCJ)
            {
                if (cj.m_sCJobID != sCJobID) m_qCJ.Enqueue(cj);
            }
        }

        private void M_xGem_OnCJStateChanged(string sCJobID, long nState)
        {
            LogRcv("OnCJStateChanged", sCJobID, nState);
            GemCJ.eState state = (GemCJ.eState)nState;
            GemCJ cj = GetCJ(sCJobID);
            if (cj == null) return;
            cj.p_eState = state;
            switch (state)
            {
                case GemCJ.eState.Excuting:
                    LogSend(m_xGem.PJSettingUpStart(sCJobID), "PJSettingUpStart", sCJobID);
                    break;
                default: break;
            }
            RunTree(Tree.eMode.Init);
        }

        void SendCJReqSelect(GemCJ cj)
        {
            LogSend(m_xGem.CJReqSelect(cj.m_sCJobID), "CJReqSelect", cj.m_sCJobID);
        }

        private void M_xGem_OnCJRspSelect(string sCJobID, long nResult)
        {
            LogRcv("OnCJRspSelect", sCJobID, nResult);
            if (sCJobID != p_cjRun.m_sCJobID) p_sLastError = "Selected CJobId MisMatch";
            if (nResult != 0) p_sLastError = "OnCJRspSelect Abnormal";
        }

        void RunTreeControlJob(Tree tree)
        {
            if (m_qCJ.Count == 0)
                return;
            foreach (GemCJ cj in m_qCJ) cj.RunTree(tree.GetTree(cj.m_sCJobID));
        }
        #endregion

        #region STS
        public bool p_bUseSTS { get; set; }
        void InitEventSTS()
        {
            m_xGem.OnSTSTransportChanged += M_xGem_OnSTSTransportChanged;
            m_xGem.OnSTSSubstLocStateChanged += M_xGem_OnSTSSubstLocStateChanged;
            m_xGem.OnSTSProcessingChanged += M_xGem_OnSTSProcessingChanged;
        }

        List<GemSlotBase> m_aSTS = new List<GemSlotBase>();
        void AddSTS(GemSlotBase gemSlot)
        {
            foreach (GemSlotBase slot in m_aSTS)
            {
                if (slot.p_id == gemSlot.p_id) return;
            }
            m_aSTS.Add(gemSlot);
        }

        public void STSSetTransport(string sSubstLocID, GemSlotBase gemSlot, GemSlotBase.eSTS state)
        {
            if (p_bUseSTS == false) return;
            AddSTS(gemSlot);
            long nError = m_xGem.STSSetTransport(sSubstLocID, gemSlot.p_id, (long)state);
            LogSend(nError, "STSSetTransport", sSubstLocID, gemSlot.p_id, state);
        }

        private void M_xGem_OnSTSTransportChanged(string sSubstLocID, string sSubstrateID, long nState)
        {
            foreach (GemSlotBase slot in m_aSTS)
            {
                if (slot.p_id == sSubstrateID)
                {
                    slot.p_sLocID = sSubstLocID;
                    slot.p_eSTS = (GemSlotBase.eSTS)nState;
                }
            }
        }

        private void M_xGem_OnSTSSubstLocStateChanged(string sSubstLocID, long nState)
        {
            if (nState == 0) return;
            for (int n = m_aSTS.Count - 1; n >= 0; n--)
            {
                GemSlotBase slot = m_aSTS[n];
                if (slot.p_sLocID == sSubstLocID)
                {
                    switch (slot.p_eSTS)
                    {
                        case GemSlotBase.eSTS.atWork:
                            STSSetProcessing(slot, GemSlotBase.eSTSProcess.InProcess);
                            return;
                        case GemSlotBase.eSTS.atDestination:
                            m_handler.CheckFinish();
                            m_aSTS.RemoveAt(n);
                            return;
                    }
                }
            }
        }

        public void STSSetProcessing(GemSlotBase gemSlot, GemSlotBase.eSTSProcess process)
        {
            if (p_bUseSTS == false) return;
            m_xGem.STSSetProcessing(gemSlot.p_sLocID, gemSlot.p_id, (long)process);
        }

        private void M_xGem_OnSTSProcessingChanged(string sSubstLocID, string sSubstrateID, long nState)
        {
            foreach (GemSlotBase slot in m_aSTS)
            {
                if (slot.p_sLocID == sSubstLocID)
                {
                    slot.p_eSTSProcess = (GemSlotBase.eSTSProcess)nState;
                }
            }
        }
        #endregion

        #region Init
        void CopyDllFile()
        {
            string[] sFindDll = Directory.GetFiles(Directory.GetCurrentDirectory(), "XLogCS_M40_01_64.dll");
            if (sFindDll.Length != 0) return;
            if (CopyDllFile(@"C:\Program Files\Linkgenesis\XGem300Pro v3.x\SE\Bin", "XLogCS_M40_01_64.dll")) return;
            return;
        }

        bool CopyDllFile(string sPath, string sFileName)
        {
            DirectoryInfo dir = new DirectoryInfo(sPath);
            if (dir.Exists == false) return false;
            FileInfo[] file = dir.GetFiles();
            for (int i = 0; i < file.Length; i++)
            {
                if (sFileName == file[i].Name) file[i].CopyTo(Directory.GetCurrentDirectory() + @"\" + file[i].Name, true);
            }
            return true;
        }

        void ProcessKill(string id)
        {
            Process[] ProcessList = Process.GetProcessesByName(id);
            foreach (Process process in ProcessList) process.Kill();
        }

        bool m_bStart = false;
        string m_sPathConfig = "C:\\Init\\GEM300.cfg";
        void XGemConfigFile()
        {
            long nError;
            try
            {
                nError = m_xGem.Initialize(m_sPathConfig);
                LogSend(nError, "Initialize", m_sPathConfig);
                if (nError == 0) m_bStart = true;
            }
            catch (Exception e)
            {
                p_sInfo = "XGem Config File Open Error : " + e.Message + "Path : " + m_sPathConfig;
            }
        }

        public void DeleteAllJobInfo()
        {
            m_xGem.PJDelAllJobInfo();
            m_xGem.CJDelAllJobInfo();
        }

        void XGemStart()
        {
            long nError = m_xGem.Start();
            LogSend(nError, "Start");
        }

        void RunTreeInit(Tree tree)
        {
            m_sPathConfig = tree.SetFile(m_sPathConfig, m_sPathConfig, "cfg", "File", "Config File Path");
            m_sRecipeExt = tree.Set(m_sRecipeExt, m_sRecipeExt, "File Extension", "Recipe File Extension");
            p_bUseSTS = tree.Set(p_bUseSTS, p_bUseSTS, "STS", "Use STS");
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_thread;
        void InitThread()
        {
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }

        enum eThreadStep
        {
            Ready,
            CheckPJ,
            InAccessing,
            Processing,
            Complete
        }

        void RunThread()
        {
            m_bThread = true;

            XGemConfigFile();
            XGemStart();
            InitLogTimer();

            Thread.Sleep(1000);
            eThreadStep step = eThreadStep.Ready;
            while (m_bThread)
            {
                Thread.Sleep(20);
                switch (step)
                {
                    case eThreadStep.Ready:
                        if ((p_cjRun == null) && (m_qCJ.Count > 0))
                        {
                            //p_cjRun = m_qCJ.Dequeue();
                            p_cjRun = m_qCJ.Peek();
                            SendCJReqSelect(p_cjRun);
                            step = eThreadStep.CheckPJ;
                        }
                        break;
                    case eThreadStep.CheckPJ:
                        if (IsPJProcessing()) step = eThreadStep.InAccessing;
                        break;
                    case eThreadStep.InAccessing:
                        foreach (GemPJ pj in p_cjRun.m_aPJ)
                        {
                            foreach (GemCarrierBase carrier in pj.m_aCarrier)
                            {
                                carrier.p_eReqAccess = GemCarrierBase.eAccess.InAccessed;
                            }
                        }
                        
                        //if (!EQ.p_bSimulate) m_engineer.ClassHandler().CalcSequence();
                        
                        foreach (GemPJ pj in p_cjRun.m_aPJ)
                        {
                            foreach (GemCarrierBase carrier in pj.m_aCarrier)
                            {
                                carrier.m_bReqGem = true;
                            }
                        }
                        
                        step = eThreadStep.Processing;
                        break;
                    case eThreadStep.Processing:
                        if (p_cjRun == null)
                            step = eThreadStep.Complete;
                        break;
                    case eThreadStep.Complete:
                        GemCJ cj = m_aCJFinish[m_aCJFinish.Count - 1];
                        foreach (GemPJ pj in cj.m_aPJ)
                        {
                            foreach (GemCarrierBase carrier in pj.m_aCarrier)
                            {
                                carrier.p_eReqAccess = GemCarrierBase.eAccess.CarrierCompleted;
                            }
                        }
                        step = eThreadStep.Ready;
                        break;
                }
            }
        }

        bool IsPJProcessing()
        {
            foreach (GemPJ pj in p_cjRun.m_aPJ)
            {
                if (pj.p_eState != GemPJ.eState.Processing) return false;
            }
            return true;
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot;
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeControlJob(m_treeRoot.GetTree("Control Job"));
            RunTreeProcessJob(m_treeRoot.GetTree("Process Job"));
            RunTreeCommunicate(m_treeRoot.GetTree("Communicate"));
            RunTreeControl(m_treeRoot.GetTree("Control"));
            RunTreeState(m_treeRoot.GetTree("State"));
            RunTreeInit(m_treeRoot.GetTree("Init"));
        }
        #endregion

        public string p_id { get; set; }

        XGem300ProNet m_xGem = new XGem300ProNet();
        IEngineer m_engineer;
        IHandler m_handler;
        string m_sRecipeExt = "ASL";
        Log m_log;
        public void Init(string id, IEngineer engineer)
        {
            p_id = id;
            m_engineer = engineer;
            m_handler = engineer.ClassHandler();
            m_log = LogView.GetLog(id, id);

            CopyDllFile();
            ProcessKill("XGem");

            InitEventCommunicate();
            InitEventControl();
            InitEventState();
            InitEventMessage();
            InitEventDateTime();
            InitEventRemoteCommand();
            InitEventGemCarrier();
            InitEventControlJob();
            InitEventProcessJob();
            InitEventSTS();

            m_treeRoot = new TreeRoot(p_id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);

            InitThread();
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Join();
            }
            if (m_xGem == null) return;
            try
            {
                if (m_bStart == false) return; //forget 정상 종료 조건 ??
                m_xGem.Stop();
                m_xGem.Close();
            }
            catch (Exception)
            {
            }
        }

        public void MakeObject(long nObject)
        {
            m_xGem.MakeObject(ref nObject);
        }

        public void SetListItem(long nObject, int listCnt)
        {
            m_xGem.SetListItem(nObject, listCnt);
        }

        public void SetStringItem(long nObject, string strItem)
        {
            m_xGem.SetStringItem(nObject, strItem);
        }

        public void SetInt4Item(long nObject, int nitem)
        {
            m_xGem.SetInt4Item(nObject, nitem);
        }

        public void SetFloat4Item(long nObject, float fItem)
        {
            m_xGem.SetFloat4Item(nObject, fItem);
        }

        public void GEMSetVariables(long nObject, long nVid)
        {
            m_xGem.GEMSetVariables(nObject, nVid);
        }
    }
}
