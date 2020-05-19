using RootTools.Comm;
using RootTools.Trees;
using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace RootTools.DMC
{
    public class DMCControl : NotifyProperty, ITool, IComm
    {
        #region Property
        public enum eState
        {
            Ready,
            Running,
            Stop,
            Error
        }
        eState _eState = eState.Ready; 
        public eState p_eState
        { 
            get { return _eState; }
            set
            {
                if (_eState == value) return;
                m_log.Info("DMC State : " + _eState.ToString() + " -> " + value.ToString()); 
                _eState = value;
                OnPropertyChanged();
                m_bRunTreeInit = true; 
            }
        }

        string _sTask = ""; 
        public string p_sTask 
        { 
            get { return _sTask; }
            set
            {
                if (_sTask == value) return;
                _sTask = value;
                m_bRunTreeInit = true;
            } 
        }

        int _nStep = 0; 
        public int p_nStep 
        { 
            get { return _nStep; }
            set
            {
                if (_nStep == value) return;
                _nStep = value;
                m_bRunTreeInit = true;
            }
        }

        public string p_id { get; set; }

        string _sInfo = "";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                OnPropertyChanged();
                if (value != "OK") m_commLog.Add(CommLog.eType.Info, value);
            }
        }

        public UserControl p_ui
        {
            get
            {
                DMCControl_UI ui = new DMCControl_UI();
                ui.Init(this);
                return ui;
            }
        }

        void RunTreeState(Tree tree)
        {
            tree.Set(p_eState, p_eState, "State", "DMC State", true, true);
            tree.Set(p_sTask, p_sTask, "Task", "DMC Task Name", true, true);
            tree.Set(p_nStep, p_nStep, "Step", "DMC Run Step", true, true); 
        }
        #endregion

        #region Setting
        public bool p_bUse { get; set; }
        public int p_nRobot { get; set; }
        public string p_sIP { get; set; }
        public int p_nPort { get; set; }
        public int p_secInterval { get; set; }

        void InitSetting()
        {
            p_bUse = false;
            p_nRobot = 0; 
            p_sIP = "0,0,0,0";
            p_nPort = 0;
            p_secInterval = 3;
        }

        void TimerCheck_Connect()
        {
            if (p_bConnect && (p_eState == eState.Error))
            {
                if (p_nRobot == 0) return;
                CoreMon.resetError(p_nRobot);
                return;
            }
            if (p_bUse && (p_nRobot <= 0))
            {
                p_nRobot = CoreMon.createRobot(p_sIP);
                if (p_nRobot <= 0) return;
                CoreMon.startService(p_nRobot);
            }
            if (!p_bUse && (p_nRobot > 0))
            {
                CoreMon.stopService(p_nRobot);
                CoreMon.deleteRobot(p_nRobot);
                p_nRobot = 0;
            }
        }

        void RunTreeSet(Tree tree)
        {
            p_bUse = (tree.p_treeRoot.p_eMode != Tree.eMode.RegRead) ? tree.Set(p_bUse, false, "Use", "Use DMC") : false;
            tree.Set(p_nRobot, p_nRobot, "Robot", "Robot Index", true, true);
            p_sIP = tree.Set(p_sIP, "0.0.0.0", "IP", "IP Address");
            p_nPort = tree.Set(p_nPort, 0, "Port", "Port Number");
            p_secInterval = tree.Set(p_secInterval, 3, "Interval", "Connection Interval (sec)");
        }
        #endregion

        #region Control
        bool _bConnect = false;
        public bool p_bConnect
        {
            get { return _bConnect; }
            set
            {
                if (value == _bConnect) return;
                _bConnect = value;
                m_bRunTreeInit = true;
            }
        }

        bool _bGetLock = false; 
        public bool p_bGetLock
        { 
            get { return _bGetLock; }
            set
            {
                if (value == _bGetLock) return;
                _bGetLock = value;
                m_bRunTreeInit = true;
            }
        }

        bool _bSetLock = false;
        public bool p_bSetLock
        {
            get { return _bSetLock; }
            set
            {
                if (_bConnect)
                {
                    if (value == _bSetLock) return;
                    _bSetLock = value;
                    if (value) CoreMon.setLock(p_nRobot);
                    else CoreMon.setUnLock(p_nRobot);
                    m_bRunTreeInit = true;
                }
            }
        }

        bool _bGetEnableTP = false;
        public bool p_bGetEnableTP
        {
            get { return _bGetEnableTP; }
            set
            {
                if (value == _bGetEnableTP) return;
                _bGetEnableTP = value;
                m_bRunTreeInit = true;
            }
        }

        bool _bSetEnableTP = false;
        public bool p_bSetEnableTP
        {
            get { return _bSetEnableTP; }
            set
            {
                if (_bConnect)
                {
                    if (value == _bSetEnableTP) return;
                    _bSetEnableTP = value;
                    CoreMon.setTPEnable(p_nRobot, _bSetEnableTP);
                    m_bRunTreeInit = true;
                }
            }
        }

        public enum eCoordinate
        {
            Joint,
            Base,
            Tool,
            User,
        }
        eCoordinate _eGetCoordinate = eCoordinate.Joint;
        public eCoordinate p_eGetCoordinate
        {
            get { return _eGetCoordinate; }
            set
            {
                if (_eGetCoordinate == value) return;
                _eGetCoordinate = value; 
                m_bRunTreeInit = true;
            }
        }

        eCoordinate _eSetCoordinate = eCoordinate.Joint;
        public eCoordinate p_eSetCoordinate
        {
            get { return _eSetCoordinate; }
            set
            {
                if (_bConnect)
                {
                    if (_eSetCoordinate == value) return;
                    _eSetCoordinate = value;
                    CoreMon.setTeachCoordinate(p_nRobot, (int)value);
                    m_bRunTreeInit = true;
                }
            }
        }

        void RunTreeControl(Tree tree)
        {
            tree.Set(p_bConnect, p_bConnect, "Connect", "Connect State", true, true);
            tree.Set(p_bGetLock, false, "Lock", "Lock", true, true);
            tree.Set(p_bGetEnableTP, false, "EnableTP", "EnableTP", true, true);
            tree.Set(p_eGetCoordinate, p_eGetCoordinate, "Coordinate", "Coordinate", true, true);
            RunTreeSetControl(tree.GetTree("Set", false)); 
        }

        void RunTreeSetControl(Tree tree)
        {
            p_bSetLock = tree.Set(p_bSetLock, false, "Lock", "Lock");
            p_bSetEnableTP = tree.Set(p_bSetEnableTP, false, "EnableTP", "EnableTP");
            p_eSetCoordinate = (eCoordinate)tree.Set(p_eSetCoordinate, p_eSetCoordinate, "Coordinate", "Coordinate");
        }
        #endregion

        #region Axis
        public ObservableCollection<DMCAxis> m_aAxis = new ObservableCollection<DMCAxis>(); 
        int p_lAxis
        { 
            get { return m_aAxis.Count; }
            set
            {
                if (m_aAxis.Count == value) return; 
                while (m_aAxis.Count < value) m_aAxis.Add(new DMCAxis(m_aAxis.Count, this));
                if (value > 0) while (m_aAxis.Count > value) m_aAxis.RemoveAt(m_aAxis.Count - 1); 
            }
        }

        bool _bGetServo = false;
        public bool p_bGetServo
        {
            get { return _bGetServo; }
            set
            {
                if (value == _bGetServo) return;
                _bGetServo = value;
                OnPropertyChanged();
            }
        }

        public bool p_bSetServo
        {
            set
            {
                if (p_nRobot > 0 && _bConnect) CoreMon.setMotorOn(p_nRobot, value);
            }
        }

        public enum eTCRMode
        {
            Teach,
            Check,
            Repeat,
        }
        eTCRMode _eGetTCRMode = eTCRMode.Teach;
        public eTCRMode p_eGetTCRMode
        {
            get { return _eGetTCRMode; }
            set
            {
                if (_eGetTCRMode == value) return;
                _eGetTCRMode = value;
                OnPropertyChanged(); 
            }
        }

        eTCRMode _eSetTCRMode = eTCRMode.Teach;
        public eTCRMode p_eSetTCRMode
        {
            get { return _eSetTCRMode; }
            set
            {
                if (_bConnect)
                {
                    if (_eSetTCRMode == value) return;
                    _eSetTCRMode = value;
                    if (p_nRobot > 0) CoreMon.setTeachMode(p_nRobot, (value == eTCRMode.Teach));
                    OnPropertyChanged();
                }
            }
        }

        public enum eJogSpeed
        {
            Low,
            Mid,
            High,
        }
        eJogSpeed _eGetJogSpeed = eJogSpeed.Low;
        public eJogSpeed p_eGetJogSpeed
        {
            get { return _eGetJogSpeed; }
            set
            {
                if (_eGetJogSpeed == value) return;
                _eGetJogSpeed = value;
                OnPropertyChanged(); 
            }
        }

        eJogSpeed _eSetJogSpeed = eJogSpeed.Low;
        public eJogSpeed p_eSetJogSpeed
        {
            get { return _eSetJogSpeed; }
            set
            {
                if (_bConnect)
                {
                    if (_eSetJogSpeed == value) return;
                    _eSetJogSpeed = value;
                    if (p_nRobot > 0) CoreMon.setTeachSpeed(p_nRobot, (int)_eSetJogSpeed);
                    OnPropertyChanged();
                }
            }
        }


        void RunTreeAxis(Tree tree)
        {
            p_lAxis = tree.Set(p_lAxis, 6, "Count", "Axis Count");
            Tree treeID = tree.GetTree("AxisID", false);
            foreach (DMCAxis axis in m_aAxis) axis.RunTree(treeID); 
        }

        float[] m_aJoint = new float[20];
        float[] m_aTrans = new float[20];
        void TimerCheck_Axis()
        {
            if (p_nRobot <= 0) return;
            if (p_bConnect == false) return;
            p_bGetServo = CoreMon.getMotorOnStatus(p_nRobot);
            p_eGetTCRMode = (eTCRMode)CoreMon.getTCRMode(p_nRobot);
            p_eGetJogSpeed = (eJogSpeed)CoreMon.getTeachSpeed(p_nRobot);
            CoreMon.getCurJointPosition(p_nRobot, m_aJoint);
            CoreMon.getCurTransPosition(p_nRobot, m_aTrans);
            for (int n = 0; n < m_aAxis.Count; n++)
            {
                m_aAxis[n].p_fJoint = m_aJoint[n];
                m_aAxis[n].p_fCartesian = m_aTrans[n];
            }
        }
        #endregion

        #region DIO
        public DMCListDIO m_listDI = new DMCListDIO("Input");
        public DMCListDIO m_listDO = new DMCListDIO("Output");

        uint[] m_getDI = new uint[32];
        uint[] m_getDO = new uint[32];
        void TimerCheck_DIO()
        {
            if (p_nRobot <= 0) return;
            if (p_bConnect == false) return;
            CoreMon.getDIN(p_nRobot, m_getDI);
            for (int n = 0, bit = 1; n < m_listDI.p_lDIO; n++, bit *= 2)
            {
                m_listDI.m_aDIO[n].p_bOn = ((m_getDI[n / 32] & bit) != 0);
            }
            CoreMon.getDOUT(p_nRobot, m_getDO);
            for (int n = 0, bit = 1; n < m_listDO.p_lDIO; n++, bit *= 2)
            {
                m_listDO.m_aDIO[n].p_bOn = ((m_getDO[n / 32] & bit) != 0);
            }
        }

        void RunTreeDIO(Tree tree)
        {
            m_listDI.RunTree(tree.GetTree("Input")); 
            m_listDO.RunTree(tree.GetTree("Output"));
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        StopWatch m_swConnect = new StopWatch();
        StopWatch m_swTimer = new StopWatch();
        StopWatch m_swCheck = new StopWatch();
        int m_nTimer = 0;
        double m_msTimer = 0;
        private void M_timer_Tick(object sender, EventArgs e)
        {
            TimerCheck_DMC();
            int msInterval = 1000 * p_secInterval;
            if (m_swConnect.ElapsedMilliseconds > msInterval)
            {
                TimerCheck_Connect();
                m_swConnect.Restart();
            }
            TimerCheck_Axis();
            TimerCheck_DIO();
            if (m_bRunTreeInit) RunTree(Tree.eMode.Init);
            m_nTimer++;
            m_msTimer += m_swCheck.ElapsedMilliseconds;
            if (m_swTimer.ElapsedMilliseconds > 60000)
            {
                if (m_nTimer > 0) m_log.Info("DMC Average Period (ms) = " + (m_msTimer / m_nTimer).ToString());
                m_swTimer.Restart();
            }
            m_swCheck.Restart();
        }

        void TimerCheck_DMC()
        {
            if (p_nRobot == 0) return;
            p_bConnect = CoreMon.isConnected(p_nRobot);
            if (!p_bConnect) return;
            p_bGetLock = CoreMon.getLockStatus(p_nRobot);
            p_bGetEnableTP = CoreMon.getTPEnableStatus(p_nRobot);
            p_eGetCoordinate = (eCoordinate)CoreMon.getTeachCoordinate(p_nRobot);
            if (CoreMon.getErrorStatus(p_nRobot)) p_eState = eState.Error;
            else
            {
                p_sTask = CoreMon.getMainTaskName(p_nRobot);
                p_eState = (eState)CoreMon.getMainTaskStatus(p_nRobot);
                p_nStep = CoreMon.getMainTaskCurStep(p_nRobot);
            }
        }
        #endregion

        #region Send
        public string Send(string sMsg)
        {
            string strCmd = "Execute " + sMsg + ", 1";
            p_sInfo = strCmd;
            m_commLog.Add(CommLog.eType.Send, strCmd); 
            try { CoreMon.executeCommand(p_nRobot, strCmd); }
            catch (Exception e) { return "DMC Send Error : " + e.Message; }
            return "OK";
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot;
        private void m_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        bool m_bRunTreeInit = false;
        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeState(m_treeRoot.GetTree("State"));
            RunTreeControl(m_treeRoot.GetTree("Control"));
            RunTreeSet(m_treeRoot.GetTree("Setting"));
            RunTreeAxis(m_treeRoot.GetTree("Axis"));
            RunTreeDIO(m_treeRoot);
            if (mode == Tree.eMode.Init) m_bRunTreeInit = false;
        }
        #endregion

        public CommLog m_commLog = null;
        Log m_log;
        public DMCControl(string id, Log log)
        {
            p_id = id;
            m_log = log;
            m_commLog = new CommLog(this, m_log);

            InitSetting(); 

            m_treeRoot = new TreeRoot(id, log);
            m_treeRoot.UpdateTree += m_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);

            m_timer.Interval = TimeSpan.FromMilliseconds(10);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
            m_swCheck.Restart();
        }

        public void ThreadStop()
        {
            m_timer.Stop(); 
        }
    }
}
