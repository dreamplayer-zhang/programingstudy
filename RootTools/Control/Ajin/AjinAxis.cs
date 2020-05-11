using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using RootTools.Trees;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using System.Windows;

namespace RootTools.Control.Ajin
{
    public class AjinAxis : ObservableObject, IAxis
    {
        #region Property
        Axis.eState _eState = Axis.eState.Init;
        public Axis.eState p_eState
        {
            get { return _eState; }
            set
            {
                if (_eState == value) return;
                m_log.Info(p_sID + " State : " + _eState.ToString() + " -> " + value.ToString());
                _eState = value;
                RaisePropertyChanged();
            }
        }
        List<string> m_aInfo = new List<string>();
        StopWatch m_swInfo = new StopWatch();
        string _sInfo = "OK";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (_sInfo == value) return;
                if (m_swInfo.ElapsedMilliseconds > 3000) m_aInfo.Clear();
                foreach (string sInfo in m_aInfo) { if (value == sInfo) return; }
                m_swInfo.Restart();
                _sInfo = value;
                RaisePropertyChanged();
                if (value == "OK") return; 
                m_aInfo.Add(value);
                m_log.Info("p_sInfo = " + value);
            }
        }
        string _sID = "";
        public string p_sID
        {
            get { return _sID; }
            set
            {
                _sID = p_nAxisID.ToString("00") + "." + value;
                RaisePropertyChanged();
            }
        }
        int _nAxisID = 0;
        public int p_nAxisID
        {
            get { return _nAxisID; }
            set
            {
                _nAxisID = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region UI Binding
        public UserControl p_ui
        {
            get
            {
                AjinAxis_UI ui = new AjinAxis_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }

        public string p_sState { get { return p_eState.ToString(); } }
        #endregion

        #region AxisPos
        Dictionary<string, double> m_aPos = new Dictionary<string, double>();
        public Dictionary<string, double> p_aPos
        {
            get
            {
                return m_aPos;
            }
            set
            {
                m_aPos = value;
                RaisePropertyChanged();
            }
        }
        public void ClearPos()
        {
            p_aPos.Clear();
            InitPosition();
        }

        public void AddPos(string sPosName)
        {
            foreach (KeyValuePair<string, double> sKeyPos in p_aPos)
            {
                if (sKeyPos.Key == sPosName)
                    return;
            }
            p_aPos.Add(sPosName, 0);
        }

        public double GetPos(string sPos)
        {
            foreach (KeyValuePair<string, double> kv in m_aPos)
            {
                if (kv.Key == sPos) return kv.Value;
            }
            return 0;
        }

        public void SaveCurrPos()
        {
            p_aPos[p_SelMovePos] = p_posCommand;
            RunTree(Tree.eMode.RegWrite);
            RaisePropertyChanged("p_aPos");
        }

        void RunPosTree(Tree tree, bool bReadOnly)
        {
            List<string> keyList = new List<string>(p_aPos.Keys);
            for (int i = 0; i < p_aPos.Count; i++ )
            {
                p_aPos[keyList[i]] = tree.Set(p_aPos[keyList[i]], 0, keyList[i], "Position (mm)", true, bReadOnly);
            }
        }
        #endregion

        #region AxisSpeed
        Dictionary<string, double> m_aSpeed = new Dictionary<string, double>();
        public Dictionary<string, double> p_aSpeed
        {
            get
            {
                return m_aSpeed;
            }
            set
            {
                m_aSpeed = value;
                RaisePropertyChanged();
                //SetProperty(ref m_aSpeed, value);
            }
        }

        public void ClearSpeed()
        {
            p_aSpeed.Clear();
            InitSpeed();
        }

        public void AddSpeed(string sSpeedName)
        {
            foreach (KeyValuePair<string, double> dicSpeed in p_aSpeed)
            {
                if (dicSpeed.Key == sSpeedName)
                    return;
            }
            p_aSpeed.Add(sSpeedName, 0);
        }

        public double GetSpeed(Enum sSpeedName)
        {
            return GetSpeed(sSpeedName.ToString());
        }

        public double GetSpeed(string sSpeedName)
        {
            foreach (KeyValuePair<string, double> dicSpeed in p_aSpeed)
            {
                if (dicSpeed.Key == sSpeedName)
                    return dicSpeed.Value;
            }
            return 0;
        }

        void RunSpeedTree(Tree tree, bool bReadOnly)
        {
            List<string> keyList = new List<string>(p_aSpeed.Keys);
            for (int i = 0; i < p_aSpeed.Count; i++)
            {
                p_aSpeed[keyList[i]] = tree.Set(p_aSpeed[keyList[i]], 0, keyList[i], "Speed (pulse/ses)", true, bReadOnly);
            }
            p_dMaxSpeed = tree.Set(p_dMaxSpeed, 4000000, "Max Speed", "Max Speed(pulse/sec)", true, true);
        }
        #endregion

        #region AxisAcc
        Dictionary<string, double> m_aAcc = new Dictionary<string, double>();
        public Dictionary<string, double> p_aAcc
        {
            get
            {
                return m_aAcc;
            }
            set
            {
                SetProperty(ref m_aAcc, value);
            }
        }

        public void ClearAcc()
        {
            p_aAcc.Clear();
            InitAcc();
        }

        public void AddAcc(string sAccName)
        {
            foreach (KeyValuePair<string, double> dicAcc in p_aAcc)
            {
                if (dicAcc.Key == sAccName)
                    return;
            }
            p_aAcc.Add(sAccName, 0);
        }

        public double GetAcc(Enum sAccName)
        {
            return GetAcc(sAccName.ToString());
        }

        public double GetAcc(string sAccName)
        {
            foreach (KeyValuePair<string, double> dicAcc in p_aAcc)
            {
                if (dicAcc.Key == sAccName)
                    return dicAcc.Value;
            }
            return 0;
        }

        void RunAccTree(Tree tree, bool bReadOnly)
        {
            List<string> keyList = new List<string>(p_aAcc.Keys);
            for (int i = 0; i < p_aAcc.Count; i++)
            {
                p_aAcc[keyList[i]] = tree.Set(p_aAcc[keyList[i]], 0.0, keyList[i], "Acceleration (unit : sec) \n 해당 설정동안 움직일 속도에 도달하도록 가속", true, bReadOnly);
            }
        }
        #endregion

        #region Sensor Setting
        enum ePulseOutMethod
        {
            One_High_Low_High,
            One_High_High_Low,
            One_Low_Low_High,
            One_Low_High_Low,
            Two_Ccw_Cw_High,
            Two_Ccw_Cw_Low,
            Two_Cw_Ccw_High,
            Two_Cw_Ccw_Low,
            Two_Phase,
            Two_Phase_Reverse
        }
        ePulseOutMethod m_ePulse = ePulseOutMethod.One_High_Low_High;

        enum eEncoderMethod
        {
            Obverse_UpDown,
            Obverse_Sqr1,
            Obverse_Sqr2,
            Obverse_Sqr4,
            Reverse_UpDown,
            Reverse_Sqr1,
            Reverse_Sqr2,
            Reverse_Sqr4
        }
        eEncoderMethod m_eEncoder = eEncoderMethod.Obverse_UpDown;

        enum eSensorMethod
        {
            LOW,
            HIGH,
            UNUSED,
            USED
        }
        eSensorMethod m_eLimitP = eSensorMethod.UNUSED;
        eSensorMethod m_eLimitM = eSensorMethod.UNUSED;
        eSensorMethod m_eInPos = eSensorMethod.UNUSED;
        eSensorMethod m_eAlarm = eSensorMethod.UNUSED;
        eSensorMethod m_eEmergency = eSensorMethod.UNUSED;
        eSensorMethod m_eHome = eSensorMethod.UNUSED;

        enum eMoveDir
        {
            DIR_CCW,
            DIR_CW
        }
        eMoveDir m_eHomeDir = eMoveDir.DIR_CCW;

        enum eHomeSignal
        {
            PosEndLimit,
            NegEndLimit,
            notUse2,
            notUse3,
            HomeSensor,
            EncodZPhase,
            UniInput02,
            UniInput03
        }
        eHomeSignal m_eHomeSignal = eHomeSignal.HomeSensor;

        enum eHomeZPhase
        {
            NotUse,
            Plus,
            Minus
        }
        eHomeZPhase m_eHomeZPhase = eHomeZPhase.NotUse;
        double m_dMaxSpeed = 4000000;
        public double p_dMaxSpeed
        {
            get
            {
                return m_dMaxSpeed;
            }
            set
            {
                SetProperty(ref m_dMaxSpeed, value);
            }
        }


        int m_nPulse_mm = 1000;
        void RunSetupModeTree(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_ePulse = (ePulseOutMethod)tree.Set(m_ePulse, m_ePulse, "Pulse", "Pulse Mode", bVisible, bReadOnly);
            m_eEncoder = (eEncoderMethod)tree.Set(m_eEncoder, m_eEncoder, "Encoder", "Encoder Mode", bVisible, bReadOnly);
            m_nBrakeSignalBit = tree.Set(m_nBrakeSignalBit, m_nBrakeSignalBit, "BrakeBit", "UNUSED(-1) USED(0~4)", bVisible, bReadOnly);
            m_nServoOnLevel = tree.Set(m_nServoOnLevel, m_nServoOnLevel, "ServoOnLevel", "LOW(0) HIGH(1)", bVisible, bReadOnly);
            m_nPulse_mm = tree.Set(m_nPulse_mm, m_nPulse_mm, "Scale", "Position Scale (pulse / mm)", bVisible, bReadOnly);
        }

        void RunSetupSensorTree(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_eHome = (eSensorMethod)tree.Set(m_eHome, m_eHome, "Home", "Home Sensor", bVisible, bReadOnly);
            m_eLimitM = (eSensorMethod)tree.Set(m_eLimitM, m_eLimitM, "Limit-", "Limit- Sensor", bVisible, bReadOnly);
            m_eLimitP = (eSensorMethod)tree.Set(m_eLimitP, m_eLimitP, "Limit+", "Limit+ Sensor", bVisible, bReadOnly);
            m_eInPos = (eSensorMethod)tree.Set(m_eInPos, m_eInPos, "InPos", "In Position Sensor", bVisible, bReadOnly);
            m_eAlarm = (eSensorMethod)tree.Set(m_eAlarm, m_eAlarm, "Alarm", "Alarm Sensor", bVisible, bReadOnly);
            m_eEmergency = (eSensorMethod)tree.Set(m_eEmergency, m_eEmergency, "Emergency", "Emergency Sensor", bVisible, bReadOnly);
        }

        void RunSetupHomeTree(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_eHomeDir = (eMoveDir)tree.Set(m_eHomeDir, m_eHomeDir, "Dir", "Search Home Direction", bVisible, bReadOnly);
            m_eHomeSignal = (eHomeSignal)tree.Set(m_eHomeSignal, m_eHomeSignal, "Signal", "Search Home Sensor", bVisible, bReadOnly);
            m_eHomeZPhase = (eHomeZPhase)tree.Set(m_eHomeZPhase, m_eHomeZPhase, "ZPhase", "Search Home ZPhase", bVisible, bReadOnly);
        }

        public void GetAxisStatus()
        {
            int i0 = 0;
            uint u0 = 0, u1 = 0, u2 = 0;
            double f0 = 0, f1 = 0; //, f2 = 0;
            AXM("AxmMotGetPulseOutMethod", CAXM.AxmMotGetPulseOutMethod(p_nAxisID, ref u0));
            m_ePulse = (ePulseOutMethod)u0;
            AXM("AxmMotGetEncInputMethod", CAXM.AxmMotGetEncInputMethod(p_nAxisID, ref u0));
            m_eEncoder = (eEncoderMethod)u0;
            AXM("AxmSignalGetLimit", CAXM.AxmSignalGetLimit(p_nAxisID, ref u0, ref u1, ref u2));
            m_eLimitP = (eSensorMethod)u1;
            m_eLimitM = (eSensorMethod)u2;
            AXM("AxmSignalGetInpos", CAXM.AxmSignalGetInpos(p_nAxisID, ref u0));
            m_eInPos = (eSensorMethod)u0;
            AXM("AxmSignalGetServoAlarm", CAXM.AxmSignalGetServoAlarm(p_nAxisID, ref u0));
            m_eAlarm = (eSensorMethod)u0;
            AXM("AxmSignalGetStop", CAXM.AxmSignalGetStop(p_nAxisID, ref u0, ref u1));
            m_eEmergency = (eSensorMethod)u1;
            AXM("AxmHomeGetSignalLevel", CAXM.AxmHomeGetSignalLevel(p_nAxisID, ref u0));
            m_eHome = (eSensorMethod)u0;
            AXM("AxmHomeGetMethod", CAXM.AxmHomeGetMethod(p_nAxisID, ref i0, ref u1, ref u2, ref f0, ref f1));
            m_eHomeDir = (eMoveDir)i0;
            m_eHomeSignal = (eHomeSignal)u1;
            m_eHomeZPhase = (eHomeZPhase)u2;
            //AXM("AxmHomeGetVel", CAXM.AxmHomeGetVel(p_nAxisID, ref GetSpeed(eSpeed.HomeStart), ref f0, ref m_vHome[0], ref f1, ref m_secHomeAcc, ref f2))
            AXM("AxmMotGetMaxVel", CAXM.AxmMotGetMaxVel(p_nAxisID, ref f0));
            p_dMaxSpeed = f0;
            RunSetupTree(Tree.eMode.RegWrite);
            RunSetupTree(Tree.eMode.Init);
            RunSubTree(Tree.eMode.RegWrite);
            RunSubTree(Tree.eMode.Init);
            ThreadStart();
        }

        public void SetAxisStatus()
        {
            m_log.Info(m_id + " SetAxisStatus");
            if (m_bEnable == false) return; 
            //AXM("AxmMotSetPulseOutMethod", CAXM.AxmMotSetPulseOutMethod(p_nAxisID, (uint)m_ePulse)); // 이상함
            //AXM("AxmMotSetEncInputMethod", CAXM.AxmMotSetEncInputMethod(p_nAxisID, (uint)m_eEncoder));
            //AXM("AxmMotSetMoveUnitPerPulse", CAXM.AxmMotSetMoveUnitPerPulse(p_nAxisID, 1, m_nPulse_mm));
            //AXM("AxmSignalSetServoOnLevel", CAXM.AxmSignalSetServoOnLevel(p_nAxisID, (uint)m_nServoOnLevel));
            //AXM("AxmSignalSetLimit", CAXM.AxmSignalSetLimit(p_nAxisID, 0, (uint)m_eLimitP, (uint)m_eLimitM));
            //AXM("AxmSignalSetInpos", CAXM.AxmSignalSetInpos(p_nAxisID, (uint)m_eInPos));
            //AXM("AxmSignalSetServoAlarm", CAXM.AxmSignalSetServoAlarm(p_nAxisID, (uint)m_eAlarm));
            //AXM("AxmSignalSetStop", CAXM.AxmSignalSetStop(p_nAxisID, 0, (uint)m_eEmergency));
            //AXM("AxmHomeSetSignalLevel", CAXM.AxmHomeSetSignalLevel(p_nAxisID, (uint)m_eHome));
            //AXM("AxmHomeSetMethod", CAXM.AxmHomeSetMethod(p_nAxisID, (int)m_eHomeDir, (uint)m_eHomeSignal, (uint)m_eHomeZPhase, 1000, 0));
            //AXM("AxmHomeSetVel", CAXM.AxmHomeSetVel(p_nAxisID, m_vHome[1], m_vHome[1], m_vHome[0], m_vHome[0], m_secHomeAcc, m_secHomeAcc));
            ThreadStart();
        }

        void InitAxis()
        {
            //AXM("AxmMotSetMaxVel", CAXM.AxmMotSetMaxVel(p_nAxisID, 4000000.0));
            AXM("AxmMotSetAbsRelMode", CAXM.AxmMotSetAbsRelMode(p_nAxisID, 0));
            SetProfileMode(eProfile.SYM_S_CURVE_MODE);
            AXM("AxmMotSetAccelUnit", CAXM.AxmMotSetAccelUnit(p_nAxisID, 1)); // [00h]unit/sec2 - [01h] sec 
            while (p_sensorU0utput.Count < 5) p_sensorU0utput.Add(false);
        }

        enum eProfile
        {
            SYM_TRAPEZOIDE_MODE,
            ASYM_TRAPEZOIDE_MODE,
            QUASI_S_CURVE_MODE,
            SYM_S_CURVE_MODE,
            ASYM_S_CURVE_MODE,
            SYM_TRAP_M3_SW_MODE,
            ASYM_TRAP_M3_SW_MODE,
            SYM_S_M3_SW_MODE,
            ASYM_S_M3_SW_MODE
        };
        string SetProfileMode(eProfile mode)
        {
            uint uCode = (uint)AXM("AxmMotSetProfileMode", CAXM.AxmMotSetProfileMode(p_nAxisID, (uint)mode));
            if (uCode == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return "OK";
            return p_sID + " SetProfileMode Error = " + uCode.ToString();
        }
        #endregion

        #region Sensor Input Check
        bool _sensorHome = false;
        public bool p_sensorHome
        {
            get { return _sensorHome; }
            set
            {
                if (_sensorHome == value) return;
                _sensorHome = value;
                RaisePropertyChanged();
            }
        }

        int _prgHome = 0;
        public int p_prgHome
        {
            get { return _prgHome; }
            set
            {
                if (_prgHome == value) return;
                _prgHome = value;
                RaisePropertyChanged();
            }
        }

        bool _sensorLimitM = false;
        public bool p_sensorLimitM
        {
            get { return _sensorLimitM; }
            set
            {
                if (_sensorLimitM == value) return;
                _sensorLimitM = value;
                RaisePropertyChanged();
            }
        }

        bool _sensorLimitP = false;
        public bool p_sensorLimitP
        {
            get { return _sensorLimitP; }
            set
            {
                if (_sensorLimitP == value) return;
                _sensorLimitP = value;
                RaisePropertyChanged();
            }
        }

        bool _sensorInPos = false;
        public bool p_sensorInPos
        {
            get { return _sensorInPos; }
            set
            {
                if (_sensorInPos == value) return;
                _sensorInPos = value;
                RaisePropertyChanged();
            }
        }

        bool _sensorAlarm = false;
        public bool p_sensorAlarm
        {
            get { return _sensorAlarm; }
            set
            {
                if (_sensorAlarm == value) return;
                _sensorAlarm = value;
                RaisePropertyChanged();
            }
        }

        bool _sensorEmergency = false;
        public bool p_sensorEmergency
        {
            get { return _sensorEmergency; }
            set
            {
                if (_sensorEmergency == value) return;
                _sensorEmergency = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<bool> p_sensorU0utput = new ObservableCollection<bool>();

        void ThreadCheck_Sensor()
        {
            uint uRead = 0;
            uint uReadM = 0;
            AXM("AxmHomeReadSignal", CAXM.AxmHomeReadSignal(p_nAxisID, ref uRead));
            p_sensorHome = (uRead > 0);
            AXM("AxmSignalReadLimit", CAXM.AxmSignalReadLimit(p_nAxisID, ref uRead, ref uReadM));
            p_sensorLimitP = (uRead > 0);
            p_sensorLimitM = (uReadM > 0);
            if (p_sensorLimitM) p_sInfo = p_sID + ": Servo HW limit(-) !!";
            if (p_sensorLimitP) p_sInfo = p_sID + ": Servo HW limit(+) !!";
            AXM("AxmSignalReadInpos", CAXM.AxmSignalReadInpos(p_nAxisID, ref uRead));
            p_sensorInPos = (uRead > 0);
            AXM("AxmSignalReadServoAlarm", CAXM.AxmSignalReadServoAlarm(p_nAxisID, ref uRead));
            p_sensorAlarm = (uRead > 0);
            if (p_sensorAlarm) p_sInfo = p_sID + ": Servo Alarm !!";
            if (m_eEmergency == eSensorMethod.UNUSED) p_sensorEmergency = false;
            else
            {
                AXM("AxmSignalReadStop", CAXM.AxmSignalReadStop(p_nAxisID, ref uRead));
                p_sensorEmergency = (uRead > 0);
                if (p_sensorEmergency) p_sInfo = p_sID + ": Servo Emergency !!";
            }
            if (p_sensorAlarm || p_sensorEmergency)
            {
                p_eState = Axis.eState.Init;
                Thread.Sleep(100);
            }
            //for (int n = 0; n < 5; n++)
            //{
            //    AXM("AxmSignalReadOutputBit", CAXM.AxmSignalReadOutputBit(p_nAxisID, n, ref uRead));
            //    p_sensorU0utput[n] = (uRead > 0);
            //}
            double dRead = 0;
            uint uRead2 = 0;
            AXM("AxmStatusGetCmdPos", CAXM.AxmStatusGetCmdPos(p_nAxisID, ref dRead));
            p_posCommand = dRead;
            AXM("AxmStatusGetCmdPos", CAXM.AxmStatusGetActPos(p_nAxisID, ref dRead));
            p_posActual = dRead;
            AXM("AxmStatusReadVel", CAXM.AxmStatusReadVel(p_nAxisID, ref dRead));
            p_vNow = dRead; 
            AXM("AxmSignalIsServoOn", CAXM.AxmSignalIsServoOn(p_nAxisID, ref uRead));
            p_bServoOn = (uRead > 0) || EQ.p_bSimulate;
    
            //AXM("AxmTriggerSetTimeLevel", CAXM.AxmTriggerGetTimeLevel(p_nAxisID,ref m_Trginfo.m_dTriggerTime, ref uRead, ref uRead2, ref uRead3));
            //AXM("AxmTriggerSetBlock", CAXM.AxmTriggerGetBlock(p_nAxisID,ref m_Trginfo.m_dTrigStart, ref m_Trginfo.m_dTrigEnd, ref m_Trginfo.m_dTrigPeriod));
            //RaisePropertyChanged("p_TrgInfo");

            if (p_eState == Axis.eState.Home)
            {
                dRead = CAXM.AxmHomeGetRate(p_nAxisID, ref uRead, ref uRead2);
                if (dRead == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                {
                    p_prgHome = (int)uRead2;
                }
                else
                {
                    p_prgHome = 0;
                }
            }

        }

        public bool WriteOutputBit(int nBitNo, bool bOn)
        {
            uint uCode = AXM("AxmSignalWriteOutputBit", CAXM.AxmSignalWriteOutputBit(p_nAxisID, nBitNo, (uint)(bOn ? 1 : 0)));
            if (uCode != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return true;
            return false;
        }

        public bool ToggleOutputBit(int nBitNo)
        {
            uint uOut = (uint)(p_sensorU0utput[nBitNo] ? 1 : 0);
            uint uCode = AXM("AxmSignalWriteOutputBit", CAXM.AxmSignalWriteOutputBit(p_nAxisID, nBitNo, uOut));
            if (uCode != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return true;
            return false;
        }
        #endregion

        #region Sensor Cmd
        const uint c_nAlarmReset = 0x02;
        public void ResetAlarm()
        {
            uint nOutput = 0;
            if (p_sensorAlarm == false) return;
            m_log.Info("Reset Alarm");
            if (AXM("AxmSignalReadOutput", CAXM.AxmSignalReadOutput(p_nAxisID, ref nOutput)) != 0) return;
            nOutput |= c_nAlarmReset;
            if (AXM("AxmSignalWriteOutput", CAXM.AxmSignalWriteOutput(p_nAxisID, nOutput)) != 0) return;
            Thread.Sleep(50);
            nOutput -= c_nAlarmReset;
            if (AXM("AxmSignalWriteOutput", CAXM.AxmSignalWriteOutput(p_nAxisID, nOutput)) != 0) return;
            m_aInfo.Clear();
            p_eState = Axis.eState.Init;
        }

        bool _bServoOn = false;
        public bool p_bServoOn
        {
            get { return _bServoOn; }
            set
            {
                
                if (_bServoOn == value) return;
                m_log.WriteLog("Servo : " + value);
                _bServoOn = value;
                RaisePropertyChanged();
            }
        }

        int m_nBrakeSignalBit = -1;
        int m_nServoOnLevel = 1;
        public void ServoOn(bool bOn, bool bAbsoluteEncoder = false)
        {
            //uint uRead = 0;
            if (EQ.p_bSimulate) return;
            if (bOn && bAbsoluteEncoder) AXM("AxmM3ServoSensOn", CAXM.AxmM3ServoSensOn(p_nAxisID));
            if (AXM("AxmSignalServoOn", CAXM.AxmSignalServoOn(p_nAxisID, bOn ? (uint)1 : (uint)0)) != 0) return;
            if (m_nBrakeSignalBit >= 0)
            {
                uint uOn = (bOn ? (uint)1 : (uint)0);
                if (AXM("AxmSignalWriteOutputBit", CAXM.AxmSignalWriteOutputBit(p_nAxisID, m_nBrakeSignalBit, uOn)) != 0) return;
            }
            if (bOn == false) p_eState = Axis.eState.Init;
        }
        #endregion

        #region Trigger
        Trigger m_Trginfo = new Trigger();
        public Trigger p_TrgInfo
        {
            get
            {
                return m_Trginfo;
            }
            set
            {
                SetProperty(ref m_Trginfo, value);
            }
        }
        Trigger m_TrgSet = new Trigger();
        public Trigger p_TrgSet
        {
            get
            {
                return m_TrgSet;
            }
            set
            {
                SetProperty(ref m_TrgSet, value);
            }
        }
#endregion

        #region SW Limit
        ObservableCollection<bool> m_bSWLimit = new ObservableCollection<bool>(){false,false};
        public ObservableCollection<bool> p_bSWLimit
        {
            get
            {
                return m_bSWLimit;
            }
            set
            {
                SetProperty(ref m_bSWLimit, value);
            }
        }
        ObservableCollection<int> m_posLimitSW = new ObservableCollection<int>() { 0, 0 };
        public ObservableCollection<int> p_posLimitSW
        {
            get
            {
                return m_posLimitSW;
            }
            set
            {
                SetProperty(ref m_posLimitSW, value);
            }
        }

        void ThreadCheck_SWLimit()
        {
            double fPos = p_posActual;
            bool bSWLimit0 = p_bSWLimit[0] && (fPos > p_posLimitSW[0]);
            if (bSWLimit0) p_sInfo = p_sID + ": Servo SW limit(-) !!";
            bool bSWLimit1 = p_bSWLimit[1] && (fPos > p_posLimitSW[1]);
            if (bSWLimit1) p_sInfo = p_sID + ": Servo SW limit(+) !!";
            if (bSWLimit0 || bSWLimit1) SStopAxis();
        }

        bool CheckSWLimit(ref double fPos)
        {
            if (EQ.p_bSimulate) return false;
            double fPosNow = p_posCommand;
            if (p_bSWLimit[0])
            {
                if (fPos >= p_posLimitSW[0]) return false;
                if (fPosNow < p_posLimitSW[0]) return true;
                fPos = p_posLimitSW[0];
                return false;
            }
            if (p_bSWLimit[1])
            {
                if (fPos <= p_posLimitSW[1]) return false;
                if (fPosNow > p_posLimitSW[1]) return true;
                fPos = p_posLimitSW[1];
                return false;
            }
            return false;
        }

        bool CheckSWLimit(double vJog)
        {
            if (vJog == 0) return true;
            double fPosNow = p_posCommand;
            if (p_bSWLimit[0] && (vJog < 0) && (fPosNow <= p_posLimitSW[0])) return true;
            if (p_bSWLimit[1] && (vJog > 0) && (fPosNow >= p_posLimitSW[1])) return true;
            return false;
        }

        void RunSetupSWLimitTree(Tree tree, int nIndex, bool bVisible, bool bReadOnly)
        {
            p_bSWLimit[nIndex] = tree.Set(p_bSWLimit[nIndex], p_bSWLimit[nIndex], "Enable", "Enable SW Limit", bVisible, bReadOnly);
            p_posLimitSW[nIndex] = tree.Set(p_posLimitSW[nIndex], p_posLimitSW[nIndex], "Pos", "SW Limit Position", bVisible && p_bSWLimit[nIndex], bReadOnly);
        }
        #endregion

        #region Home
        public string HomeStart()
        {
            switch (p_eState)
            {
                case Axis.eState.Error:
                case Axis.eState.Init:
                case Axis.eState.Ready:
                    p_eState = Axis.eState.Home;
                    return "OK";
                default: return p_sID + " Home Cancel eState = " + p_eState.ToString();
            }
        }

        string StateHome()
        {
            m_swMove.Restart();
            ResetAlarm();
            Thread.Sleep(100);
            ServoOn(true);
            Thread.Sleep(100);
            AXM("AxmHomeSetMethod", CAXM.AxmHomeSetMethod(p_nAxisID, (int)m_eHomeDir, (uint)m_eHomeSignal, (uint)m_eHomeZPhase, 1000, 0));
            double startspeed = GetSpeed(eSpeed.HomeStart);
            double endspeed = GetSpeed(eSpeed.HomeEnd);
            AXM("AxmHomeSetMethod", CAXM.AxmHomeSetVel(p_nAxisID, startspeed, startspeed, endspeed, endspeed, 1 ,1));
            if (AXM("AxmHomeSetStart", CAXM.AxmHomeSetStart(p_nAxisID)) != 0) return "Start Home Error";
            while (true)
            {
                Thread.Sleep(1); 
                if (EQ.IsStop())
                {
                    EStopAxis();
                    ServoOn(false);
                    return "EQ Stoped";
                }
                uint nStat = 0;
                AXM("AxmHomeGetResult", CAXM.AxmHomeGetResult(p_nAxisID, ref nStat));
                if (nStat == 1)
                {
                    m_log.Info(p_sID + " -> Home Finished " + (m_swMove.ElapsedMilliseconds / 1000).ToString("0.0 sec"));
                    return "OK";
                }
            }
        }
        #endregion

        #region Trigger
        public void SetTrigger(double fPos0, double fPos1, double dPos, bool bCmd ,bool bLevel= true , double dTrigTime = 2)
        {
            uint nLevel = bLevel ? (uint)1 : (uint)0;
            uint nEncoder = bCmd ? (uint)1 : (uint)0;
            AXM("AxmTriggerSetReset", CAXM.AxmTriggerSetReset(p_nAxisID));
            AXM("AxmTriggerSetTimeLevel", CAXM.AxmTriggerSetTimeLevel(p_nAxisID, dTrigTime, nLevel, nEncoder, 0));
            AXM("AxmTriggerSetBlock", CAXM.AxmTriggerSetBlock(p_nAxisID, fPos0, fPos1, dPos));
            //m_log.Info(string.Format("Start Trigger (Level = {1}, Cmd = {2}, Uptime = {3})", bLevel, bCmd, dTrigTime));
        }

        public void _SetTrigger()
        {
            // variable
            Trigger trigger = p_TrgSet;
            double dTrigStart = trigger.p_dTrigStart;
            double dTrigEnd = trigger.p_dTrigEnd;
            double dTrigPeriod = trigger.p_dTrigPeriod;
            bool bCmd = !trigger.p_bActTrigger;

            // implement
            SetTrigger(dTrigStart, dTrigEnd, dTrigPeriod, bCmd);

            return;
        }

        public void ResetTrigger()
        {
            AXM("AxmTriggerSetReset", CAXM.AxmTriggerSetReset(p_nAxisID));
         //   m_log.Info("Stop Trigger");
        }
        #endregion

        #region Posision & V
        int _msMoveTimeout = 0;
        int p_msMoveTimeout
        {
            get { return _msMoveTimeout; }
            set
            {
                _msMoveTimeout = value + 2000;
            }
        }

        double _posCommand = 0;
        public double p_posCommand
        {
            get { return _posCommand; }
            set
            {
                if (_posCommand == value) return;
                _posCommand = value;
                RaisePropertyChanged();
            }
        }

        double _posActual = 0;
        public double p_posActual
        {
            get { return _posActual; }
            set
            {
                if (_posActual == value) return;
                _posActual = value;
                RaisePropertyChanged();
            }
        }

        double _vNow = 0; 
        public double p_vNow
        {
            get { return _vNow; }
            set
            {
                if (_vNow == value) return;
                _vNow = value;
                RaisePropertyChanged(); 
            }
        }

        public void SetPos(bool bCmd, double fPos)
        {
            if (bCmd) AXM("AxmStatusSetCmdPos", CAXM.AxmStatusSetCmdPos(p_nAxisID, fPos));
            else AXM("AxmStatusSetCmdPos", CAXM.AxmStatusSetActPos(p_nAxisID, fPos));
        }

        bool m_bEnableVRate = false;
        double _vRate = 1;
        public double p_vRate
        {
            get
            {
                if (m_bEnableVRate) return Math.Min(_vRate, AjinListAxis.s_vRate);
                return 1.0;
            }
            set
            {
                //if (m_bEnableVRate == false) return;
                //if (_vRate == value) return;
                //if (m_vStart <= 0) return;
                //double rate = value;
                //if (rate < 0.1) rate = 0.1;
                //if (rate > 1) rate = 1;
                //m_log.Info(p_sID + " V Rate Change : " + _vRate.ToString() + " -> " + rate.ToString());
                //_vRate = rate;
                //OverrideV(); 
            }
        }

        public void OverrideV()
        {
        //    AXM("AxmOverrideVel", CAXM.AxmOverrideVel(p_nAxisID, m_vStart * p_vRate));
        }

        void RunSetupVRate(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_bEnableVRate = tree.Set(m_bEnableVRate, m_bEnableVRate, "Enable", "Enable Axis V Rate", bVisible, bReadOnly);
            p_vRate = tree.Set(p_vRate, p_vRate, "Rate", "Axis Velocity Rate (0 ~ 1)", m_bEnableVRate, bReadOnly);
            if (p_vRate < 0.1) p_vRate = 0.1;
            if (p_vRate > 1) p_vRate = 1; 
        }
        #endregion

        #region Jog
        string Jog(double vJog, double secAcc = -1, double secDec = -1)
        {
            if (!m_bEnable)
                return p_sID + " Axis not Enable";
            if (CheckSWLimit(vJog))
                return p_sID + " Check Software Limit";
            if (secAcc < 0)
                secAcc = 0.1;
            if (secDec < 0)
                secDec = 0.1;// GetAcc(eAcc.MoveAcc);
            if (AXM("AxmMoveVel", CAXM.AxmMoveVel(p_nAxisID, vJog, secAcc, secDec)) != 0){
                p_eState = Axis.eState.Init;
                return p_sID + " Axis Jog Start Error";
            }
            m_swJogContinuos.Restart();
            p_eState = Axis.eState.Jog;
            return "OK";
        }
        StopWatch m_swJogContinuos = new StopWatch();

        void Jog_Plus_Fast()
        {
            if (p_eState == Axis.eState.Jog)
            {
                m_swJogContinuos.Restart();
            }
            else
                Jog(GetSpeed(eSpeed.JogMove));
        }
        void Jog_Plus_Slow()
        {
            if (p_eState == Axis.eState.Jog)
            {
                m_swJogContinuos.Restart();
            }
            else
                Jog(GetSpeed(eSpeed.JogMove) / 10);
        }
        void Jog_Minus_Fast()
        {
            if (p_eState == Axis.eState.Jog)
            {
                m_swJogContinuos.Restart();
            }
            else
                Jog(GetSpeed(eSpeed.JogMove) * -1);
        }
        void Jog_Minus_Slow()
        {
            if (p_eState == Axis.eState.Jog)
            {
                m_swJogContinuos.Restart();
            }
            else
                Jog(GetSpeed(eSpeed.JogMove) * -1 /10);
        }

        public void SStopAxis()
        {
            AXM("AxmMoveSStop", CAXM.AxmMoveSStop(p_nAxisID));
        }

        public void EStopAxis()
        {
            AXM("AxmMoveEStop", CAXM.AxmMoveEStop(p_nAxisID));
        }
        #endregion

        #region Move
        StopWatch m_swMove = new StopWatch();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vMove"> if (vMove > 1) pulse/sec else vRate </param>
        /// <returns></returns>
        public string Move(double fPos, double vMove = -1, double secAcc = -1, double secDec = -1)
        {
            if (EQ.IsStop()) return p_sID + " EQ Stop";
            if ((EQ.p_bSimulate == false) && !m_bEnable) return p_sID + " Axis not Enable";
            if (CheckSWLimit(ref fPos)) return p_sID + " Check Software Limit";
            if (p_eState != Axis.eState.Ready) return p_sID + " State not Ready"; 
            if (vMove <= 0) vMove = GetSpeed(eSpeed.Move);
            //else if (vMove <= 1) vMove = vMove * m_vMove;
            if (secAcc < 0) secAcc = GetAcc(eAcc.MoveAcc);
            if (secDec < 0) secDec = GetAcc(eAcc.MoveAcc);
           // p_msMoveTimeout = (int)((fPos - p_posActual) / vMove + 1000 * (secAcc + secDec));
            m_swMove.Restart();
            if (EQ.p_bSimulate == false)
            {
                if (AXM("AxmMoveStartPos", CAXM.AxmMoveStartPos(p_nAxisID, fPos, vMove, secAcc, secDec)) != 0)
                {
                    p_eState = Axis.eState.Init;
                    return p_sID + " Axis Move Start Error";
                }
            }
            p_eState = Axis.eState.Move;
            Thread.Sleep(5);
            return "OK";
        }

        public string Move(string pos, double fOffset = 0, double vMove = -1, double secAcc = -1, double secDec = -1)
        {
            double fPos = GetPos(pos) + fOffset;
            return Move(fPos, vMove, secAcc, secDec);
        }
        #endregion

        #region Repeat
        bool _bRepeat = false; 
        public bool p_bRepeat
        {
            get { return _bRepeat; }
            set
            {
                if (_bRepeat == value) return;
                if ((value == true) && (p_eState != Axis.eState.Ready)) return;
                _bRepeat = value;
                RaisePropertyChanged(); 
            }
        }

        int m_iRepeat = 1; 
        void StateRepeat()
        {
            //Move(m_asPosRepeat[m_iRepeat]);
            switch (m_iRepeat)
            {
                case 0:
                    Move(p_strRepeatStartPos);
                    break;
                case 1:
                    Move(p_strRepeatEndPos);
                    break;
            }
            m_iRepeat = 1 - m_iRepeat; 
        }

        string[] m_asPosRepeat = new string[] { "= Pos0", "= Pos1" };
        
        void RunRepeatTree(Tree tree, bool bReadOnly)
        {
            //m_asPosRepeat[0] = tree.Set(m_asPosRepeat[0], "= Pos0", m_asPos, "From", "Axis Position Name");
            //m_asPosRepeat[1] = tree.Set(m_asPosRepeat[1], "= Pos1", m_asPos, "To", "Axis Position Name");
        }
        public string RepeatStart()
        {
            switch (p_eState)
            {
                case Axis.eState.Ready:
                    p_bRepeat = true;
                    return "OK";
                default: return p_sID + " Repeat Cancel eState = " + p_eState.ToString();
            }
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_threadRun;
        Thread m_threadCheck;
        void ThreadStart()
        {
            if (m_bThread) return;
            m_bThread = true;
            InitAxis();
            m_threadRun = new Thread(new ThreadStart(RunThread));
            m_threadCheck = new Thread(new ThreadStart(CheckThread));
            m_threadRun.Start();
            m_threadCheck.Start(); 
        }

        void RunThread()
        {
            Thread.Sleep(2000); 
            while (m_bThread)
            {
                Thread.Sleep(1);
                switch (p_eState)
                {
                    case Axis.eState.Home:
                        p_sInfo = StateHome();
                        if (p_sInfo == "OK") p_eState = Axis.eState.Ready;
                        else p_eState = Axis.eState.Error; 
                        break;
                    case Axis.eState.Move:
                        uint nStat = 0;
                        AXM("AxmStatusReadInMotion", CAXM.AxmStatusReadInMotion(p_nAxisID, ref nStat));
                        if (nStat == 0) p_eState = Axis.eState.Ready;
                        break;
                    case Axis.eState.Jog:
                        uint nStats = 0; 
                        AXM("AxmStatusReadInMotion", CAXM.AxmStatusReadInMotion(p_nAxisID, ref nStats));
                        if (nStats == 0) p_eState = Axis.eState.Ready; 
                        if (m_swJogContinuos.ElapsedMilliseconds > 300 || EQ.IsStop())
                        {
                            SStopAxis();
                            p_eState = Axis.eState.Ready; 
                        }
                        break;
                    case Axis.eState.Ready:
                        if (p_bRepeat) StateRepeat(); 
                        break;
                    default: break; 
                }
            }
        }

        void CheckThread()
        {
            AXM("AxmSignalWriteOutput", CAXM.AxmSignalWriteOutput(p_nAxisID, 0));
            //AXM("AxmMotSetAccelUnit", CAXM.AxmMotSetAccelUnit(p_nAxisID, 1));
            Thread.Sleep(2000);
            while (m_bThread)
            {
                Thread.Sleep(1);
           //     ThreadCheck_SWLimit();
                ThreadCheck_Sensor();
            }
        }
        #endregion

        #region Ajin Function
        public string SetPosTypeBound(double fPositivePos, double fNegativePos)
        {
            uint uCode = AXM("AxmStatusSetPosType", CAXM.AxmStatusSetPosType(p_nAxisID, 1, fPositivePos, fNegativePos));
            if (uCode == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return "OK";
            return p_sID + " SetPosTypeBound Error = " + uCode.ToString();
        }

        public string SetGantry(IAxis axisSlave)
        {
            if (axisSlave == null) return p_sID + " SetGentry Slave Axis is null";
            uint nError = AXM("AxmLinkResetMode", CAXM.AxmLinkResetMode(0));
            if (nError != 0) return p_sID + " AxmLinkResetMode Error" + nError.ToString();
            nError = AXM("AxmGantrySetDisable", CAXM.AxmGantrySetDisable(p_nAxisID, axisSlave.p_nAxisID));
            if (nError != 0) return p_sID + " AxmGantrySetDisable Error" + nError.ToString();
            nError = AXM("AxmGantrySetEnable", CAXM.AxmGantrySetEnable(p_nAxisID, axisSlave.p_nAxisID, 0, 0, 0));
            if (nError != 0) return p_sID + " AxmGantrySetEnable Error" + nError.ToString();
            uint nOn = 0, nHome = 0;
            double fOffset = 0, fRange = 0;
            nError = AXM("AxmGantryGetEnable", CAXM.AxmGantryGetEnable(p_nAxisID, ref nHome, ref fOffset, ref fRange, ref nOn));
            if (nError != 0) return p_sID + " AxmGantryGetEnable Error" + nError.ToString();
            return "OK";
        }

        public void SetLoadRatio()
        {
            AXM("AxmStatusSetReadServoLoadRatio", CAXM.AxmStatusSetReadServoLoadRatio(p_nAxisID, (uint)2));
        }

        public void ReadLoadRatio(ref double fLoadRatio)
        {
            AXM("AxmStatusReadServoLoadRatio", CAXM.AxmStatusReadServoLoadRatio(p_nAxisID, ref fLoadRatio));
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
            RaisePropertyChanged("p_aPos");
        }

        private void M_treeRootSpeed_UpdateTree()
        {
            RunSubTree(Tree.eMode.Update);
            RunSubTree(Tree.eMode.RegWrite);
            RaisePropertyChanged("p_aSpeed");
       
        }

        public void RunTree(Tree.eMode mode)
        {
            bool bReadOnly = (m_engineer.p_user.m_eLevel < Login.eLevel.Operator);
            p_treeRootMain.p_eMode = mode;
            RunPosTree(p_treeRootMain.GetTree("Position"), bReadOnly);
            RunSetupSWLimitTree(p_treeRootMain.GetTree("SW Limit-"), 0, true, bReadOnly);
            RunSetupSWLimitTree(p_treeRootMain.GetTree("SW Limit+"), 1, true, bReadOnly);
        }

        public void RunSubTree(Tree.eMode mode)
        {
            bool bReadOnly = (m_engineer.p_user.m_eLevel < Login.eLevel.Operator);
            p_treeRootSub.p_eMode = mode;
            RunSpeedTree(p_treeRootSub.GetTree("Speed"), bReadOnly);
            RunAccTree(p_treeRootSub.GetTree("Accelation"), bReadOnly);
            //bool bReadOnly = (m_engineer.p_user.m_eLevel < Login.eLevel.Operator);
            //m_treeRootSpeed.p_eMode = mode;
            //RunMoveTree(m_treeRootSpeed.GetTree("Move"), bReadOnly);
            //RunRepeatTree(m_treeRootSpeed.GetTree("Repeat"), bReadOnly);
            //RunJogTree(m_treeRootSpeed.GetTree("Jog"), bReadOnly);
            //RunHomeTree(m_treeRootSpeed.GetTree("Home"), bReadOnly);
        }

        private void M_treeSetup_UpdateTree()
        {
            RunSetupTree(Tree.eMode.Update);
            RunSetupTree(Tree.eMode.Init);
            RunSetupTree(Tree.eMode.RegWrite);
        }

        public void RunSetupTree(Tree.eMode mode)
        {
            bool bReadOnly = (m_engineer.p_user.m_eLevel < Login.eLevel.Admin);
            p_treeSetup.p_eMode = mode;
            RunSetupModeTree(p_treeSetup.GetTree("Mode"), true, bReadOnly);
            RunSetupSensorTree(p_treeSetup.GetTree("Sensor"), true, bReadOnly);
            RunSetupHomeTree(p_treeSetup.GetTree("Home"), true, bReadOnly);
            RunSetupVRate(p_treeSetup.GetTree("V Rate"), true, bReadOnly);
            SetAxisStatus(); 
        }
        #endregion

        public enum ePosition
        {
            Position_0,
            Position_1,
            Position_2,
        }
        public enum eSpeed
        {
            HomeStart,
            HomeEnd,
            JogMove,
            Move,
        }
        public enum eAcc
        {
            HomeAcc,
            MoveAcc,
        }

        string m_id;
        IEngineer m_engineer;
        LogWriter m_log;
        bool m_bEnable = false;
        
        TreeRoot m_treeRootMain;
        public TreeRoot p_treeRootMain
        {
            get { return m_treeRootMain; }
            set { SetProperty(ref m_treeRootMain, value); }
        }
        
        TreeRoot m_treeRootSub;
        public TreeRoot p_treeRootSub
        {
            get { return m_treeRootSub; }
            set { SetProperty(ref m_treeRootSub, value); }
        }
        
        TreeRoot m_treeSetup;
        public TreeRoot p_treeSetup
        {
            get { return m_treeSetup; }
            set { SetProperty(ref m_treeSetup, value); }
        }

        public void Init(string id, int nAxisID, IEngineer engineer, LogWriter log, bool bEnable)
        {
            m_id = id + "." + nAxisID.ToString("00");
            p_nAxisID = nAxisID;
            p_sID = "Axis";
            m_engineer = engineer;
            m_log = log;
            m_bEnable = bEnable;
            InitPosition();
            InitSpeed();
            InitAcc();

            p_treeRootMain = new TreeRoot(m_id, m_log);
            p_treeRootMain.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);

            p_treeRootSub = new TreeRoot(m_id, m_log);
            p_treeRootSub.UpdateTree += M_treeRootSpeed_UpdateTree;
            RunSubTree(Tree.eMode.RegRead);
            RunSubTree(Tree.eMode.Init);

            p_treeSetup = new TreeRoot(m_id + ".Setup", m_log);
            p_treeSetup.UpdateTree += M_treeSetup_UpdateTree;
            RunSetupTree(Tree.eMode.RegRead); 
        }

        void InitPosition()
        {
            for (int i = 0; i < Enum.GetNames(typeof(ePosition)).Length; i++)
            {
                AddPos(((ePosition)i).ToString());
            }
        }
        
        void InitSpeed()
        {
            for (int i = 0; i < Enum.GetNames(typeof(eSpeed)).Length; i++)
            {
                AddSpeed(((eSpeed)i).ToString());
            }
        }
        void InitAcc()
        {
            for (int i = 0; i < Enum.GetNames(typeof(eAcc)).Length; i++)
            {
                AddAcc(((eAcc)i).ToString());
            }
        }

        public void ThreadStop()
        {
            ServoOn(false);
            if (m_bThread)
            {
                m_bThread = false;
                m_threadRun.Join(); 
                m_threadCheck.Join();
            }
        }

        uint AXM(string sFunc, uint uResult)
        {
            if (uResult == 0) return uResult;
            if (m_log == null) return uResult;
            p_sInfo = sFunc + ", Error # = " + uResult.ToString();
            return uResult;
        }

        public void ServoCommand()
        {
            ServoOn(!_bServoOn);
        }

        public void PlusRelativeMove()
        {
            double dRead = 0.0;
            AXM("AxmStatusGetCmdPos", CAXM.AxmStatusGetActPos(p_nAxisID, ref dRead));
            p_posActual = dRead;
            Move(p_posActual + p_dRelPos, GetSpeed(eSpeed.Move));
        }

        public void MinusRelativeMove()
        {
            double dRead = 0.0;
            AXM("AxmStatusGetCmdPos", CAXM.AxmStatusGetActPos(p_nAxisID, ref dRead));
            p_posActual = dRead;
            Move(p_posActual - p_dRelPos, GetSpeed(eSpeed.Move));
        }

        public void MovePosition()
        {
            Move(p_SelMovePos,0, GetSpeed(eSpeed.Move));
        }

        public void RepeatMovePosition()
        {
            RepeatStart();
        }

        public void AlarmCommand()
        {
            if (p_sensorAlarm)
                ResetAlarm();
        }
        public void HomeCommand()
        {
            HomeStart();
        }

        double m_dRelPos = 0;
        public double p_dRelPos
        {
            get
            {
                return m_dRelPos;
            }
            set
            {
                SetProperty(ref m_dRelPos, value);
            }
        }

        string _SelMovePos = "";
        public string p_SelMovePos
        {
            get
            {
                return _SelMovePos;
            }
            set
            {
                SetProperty(ref _SelMovePos, value);
            }
        }

        string m_strRepeatStartPos;
        public string p_strRepeatStartPos
        {
            get
            {
                return m_strRepeatStartPos;
            }
            set
            {
                SetProperty(ref m_strRepeatStartPos, value);
            }
        }

        string m_strRepeatEndPos;
        public string p_strRepeatEndPos
        {
            get
            {
                return m_strRepeatEndPos;
            }
            set
            {
                SetProperty(ref m_strRepeatEndPos, value);
            }
        }

        public RelayCommand SavePosCommand
        {
            get
            {
                return new RelayCommand(SaveCurrPos);
            }
        }

        public RelayCommand StopJogCommand
        {
            get
            {
                return new RelayCommand(EStopAxis);
            }
        }
        public RelayCommand PJogFastCommand
        {
            get{
                return new RelayCommand(Jog_Plus_Fast);
            }
        }
        public RelayCommand PJogSlowCommand
        {
            get
            {
                return new RelayCommand(Jog_Plus_Slow);
            }
        }
        public RelayCommand MJogFastCommand
        {
            get
            {
                return new RelayCommand(Jog_Minus_Fast);
            }
        }
        public RelayCommand MJogSlowCommand
        {
            get
            {
                return new RelayCommand(Jog_Minus_Slow);
            }
        }

        public RelayCommand PlusRelativeMoveCommand
        {
            get
            {
                return new RelayCommand(PlusRelativeMove);
            }
        }

        public RelayCommand MinusRelativeMoveCommand
        {
            get
            {
                return new RelayCommand(MinusRelativeMove);
            }
        }

        public RelayCommand MoveCommand
        {
            get
            {
                return new RelayCommand(MovePosition);
            }
        }

        public RelayCommand RepeatCommand
        {
            get
            {
                return new RelayCommand(RepeatMovePosition);
            }
        }

        public RelayCommand StopCommand
        {
            get
            {
                return new RelayCommand(SStopAxis);
            }
        }

        public RelayCommand AlarmOffCommand
        {
           get{
               return new RelayCommand(AlarmCommand);
           }

        }
        public RelayCommand ServoOnCommand
        {
           get{
               return new RelayCommand(ServoCommand);
           }

        }
        public RelayCommand HomeClickCommand
        {
            get
            {
                return new RelayCommand(HomeCommand);
            }
        }
        public RelayCommand EStopCommand
        {
            get
            {
                return new RelayCommand(EStopAxis);
            }
        }

        public RelayCommand SetTriggerCommand
        {
            get
            {
                return new RelayCommand(_SetTrigger);
            }
        }

        public RelayCommand ResetTriggerCommand
        {
            get
            {
                return new RelayCommand(ResetTrigger);
            }
        }
    }

    public class Trigger: ObservableObject
    {
        public bool m_bSetTrigger = false;
        public bool p_bSetTrigger
        {
            get
            {
                return m_bSetTrigger;
            }
            set
            {
                SetProperty(ref m_bSetTrigger, value);
            }
        }
        public bool m_bActTrigger = false;
        public bool p_bActTrigger
        {
            get
            {
                return m_bActTrigger;
            }
            set
            {
                SetProperty(ref m_bActTrigger, value);
            }
        }
        public double m_dTriggerTime = 5;
        public double p_bTriggerTime
        {
            get
            {
                return m_dTriggerTime;
            }
            set
            {
                SetProperty(ref m_dTriggerTime, value);
            }
        }
        public double m_dTrigStart = 0;
        public double p_dTrigStart
        {
            get
            {
                return m_dTrigStart;
            }
            set
            {
                SetProperty(ref m_dTrigStart,value);
            }
        }
        public double m_dTrigEnd = 0;
        public double p_dTrigEnd
        {
            get
            {
                return m_dTrigEnd;
            }
            set
            {
                SetProperty(ref m_dTrigEnd, value);
            }
        }
        public double m_dTrigPeriod = 0;
        public double p_dTrigPeriod
        {
            get
            {
                return m_dTrigPeriod;
            }
            set
            {
                SetProperty(ref m_dTrigPeriod, value);
            }
        }
    }

    public class DictionaryItemValueCanvasConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
             System.Globalization.CultureInfo culture)
        {
            try
            {
                double width = (double)values[1];
                bool bUseLimit = false;
                double dLimit_M = 0;
                double dLimit_P = 0;
                RootTools.Control.Ajin.AjinAxis axis = (RootTools.Control.Ajin.AjinAxis)(((TabControl)values[2]).DataContext);
                double pos = axis.GetPos((string)values[0]);
                bUseLimit = axis.p_bSWLimit[0] && axis.p_bSWLimit[1];
                if (bUseLimit)
                {
                    dLimit_M = axis.p_posLimitSW[0];
                    dLimit_P = axis.p_posLimitSW[1];
                    return (pos) * width / (dLimit_P - dLimit_M);
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
         System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DictionaryItemGetPosConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
             System.Globalization.CultureInfo culture)
        {
            try
            {
                Dictionary<string, double> aPos = (Dictionary<string, double>)values[0];
                string sPos = (string)values[1];

                foreach (KeyValuePair<string, double> kv in aPos)
                {
                    if (kv.Key == sPos)
                        return kv.Value.ToString();
                }
                return "0";
            }
            catch
            {
                return "0";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
         System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bvalue = (bool)value;
            Brush backColor;
            BrushConverter bc = new BrushConverter();
            if(bvalue)
            {
                backColor = (Brush)bc.ConvertFrom("#FF68E03A");
            }
            else
            {
                backColor = (Brush)bc.ConvertFrom("#FFBBBBBB");
            }
            return backColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ColorToBoolConverterRed : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bvalue = (bool)value;
            Brush backColor;
            BrushConverter bc = new BrushConverter();
            if (bvalue)
            {
                backColor = (Brush)bc.ConvertFrom("#FFC94A4A");
            }
            else
            {
                backColor = (Brush)bc.ConvertFrom("#FFBBBBBB");
            }
            return backColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class DataContextToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility result = Visibility.Visible;
            if (value == null)
            {
                result = Visibility.Hidden;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class AjinModuleToPackIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PackIconKind result = PackIconKind.AlphaXBox;
            string str = value.ToString();
            AXT_MODULE type = AXT_MODULE.AXT_SIO_DI32;
            if (Enum.TryParse(value.ToString(), out type))
            {
                switch (type)
                {
                    case AXT_MODULE.AXT_SIO_DI32:
                    case AXT_MODULE.AXT_SIO_RDI32MLIII:
                    case AXT_MODULE.AXT_SIO_RDI32PMLIII:
                        result = PackIconKind.AlphaIBox;
                        break;
                    case AXT_MODULE.AXT_SIO_DO32P:
                    case AXT_MODULE.AXT_SIO_RDO32MLIII:
                    case AXT_MODULE.AXT_SIO_RDO32PMLIII:
                        result = PackIconKind.AlphaOBox;
                        break;
                    case AXT_MODULE.AXT_SIO_DB32P:
                    case AXT_MODULE.AXT_SIO_RDB32MLIII:
                    case AXT_MODULE.AXT_SIO_RDB32PMLIII:
                        result = PackIconKind.AlphaBBox;
                        break;
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
