using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using RootTools.Trees;

namespace RootTools.Control.Ajin
{
    public class AjinAxis : Axis
    {
        #region Property
        double _pulsepUnit = 1; 
        double p_pulsepUnit
        {
            get { return _pulsepUnit; }
            set
            {
                if (_pulsepUnit == value) return;
                double fRatio = _pulsepUnit / value;
                foreach (string sKey in p_asPos) m_aPos[sKey] *= fRatio;
                foreach (Speed speed in m_aSpeed) speed.m_v *= fRatio;
                m_trigger.m_aPos[0] *= fRatio;
                m_trigger.m_aPos[1] *= fRatio;
                m_trigger.m_dPos *= fRatio;
                _pulsepUnit = value;
                RunTree(Tree.eMode.Init);
                RunTree(Tree.eMode.RegWrite);
            }
        }

        public int m_nAxis = -1;
        bool m_bAbsoluteEncoder = false;
        void RunTreeSettingProperty(Tree tree)
        {
            int nAxis = m_nAxis;
            m_nAxis = tree.Set(m_nAxis, m_nAxis, "Axis Number", "Ajin Axis Number");
            m_sUnit = tree.Set(m_sUnit, m_sUnit, "Unit", "Ajin Axis Unit");
            double pulseUnit = tree.Set(p_pulsepUnit, p_pulsepUnit, "Pulse/Unit", "Pulse / Unit");
            if (tree.p_treeRoot.p_eMode != Tree.eMode.RegRead) p_pulsepUnit = pulseUnit; 
            m_bAbsoluteEncoder = tree.Set(m_bAbsoluteEncoder, m_bAbsoluteEncoder, "Absolute Encoder", "Absolute Encoder");
            if (nAxis != m_nAxis) m_listAxis.m_qSetAxis.Enqueue(this);
        }

        bool m_isPlus = false;
        #endregion

        #region Position & Velocity
        public override void SetCommandPosition(double fPos)
        {
            AXM("AxmStatusSetCmdPos", CAXM.AxmStatusSetCmdPos(m_nAxis, fPos * p_pulsepUnit));
        }

        public override void SetActualPosition(double fPos)
        {
            AXM("AxmStatusSetActPos", CAXM.AxmStatusSetActPos(m_nAxis, fPos * p_pulsepUnit));
        }

        public override double GetActualPosition()
        {
            double fPos = 0;
            AXM("AxmStatusGetActPos", CAXM.AxmStatusGetActPos(m_nAxis, ref fPos));
            return fPos / p_pulsepUnit;
        }

        public override void OverrideVelocity(double v)
        {
            AXM("AxmOverrideVel", CAXM.AxmOverrideVel(m_nAxis, v * p_pulsepUnit));
        }

        void RunThreadCheck_Position()
        {
            double dRead = 0;
            AXM("AxmStatusGetCmdPos", CAXM.AxmStatusGetCmdPos(m_nAxis, ref dRead));
            p_posCommand = dRead / p_pulsepUnit;
            AXM("AxmStatusGetCmdPos", CAXM.AxmStatusGetActPos(m_nAxis, ref dRead));
            p_posActual = dRead / p_pulsepUnit;
            AXM("AxmStatusReadVel", CAXM.AxmStatusReadVel(m_nAxis, ref dRead));
            p_vNow = dRead / p_pulsepUnit;
        }
        #endregion

        #region Jog
        public override string Jog(double fScale, string sSpeed = null)
        {
            p_log.Info(p_id + " Jog Start : " + fScale.ToString());
            if (IsInterlock()) return p_id + m_sCheckInterlock;

            if (fScale > 0)
                m_isPlus = true;
            else
                m_isPlus = false;

            p_sInfo = base.Jog(fScale, sSpeed);
            if (p_sInfo != "OK") return p_sInfo;
            if (m_nAxis < 0) return p_id + " Axis not Assigned";
            if (AXM("AxmMoveVel", CAXM.AxmMoveVel(m_nAxis, fScale * m_speedNow.m_v * p_pulsepUnit, m_speedNow.m_acc, m_speedNow.m_dec)) != 0)
            {
                p_eState = eState.Init;
                p_sInfo = p_id + " Axis Jog Start Error";
                return p_sInfo;
            }
            p_eState = eState.Jog;
            return "OK";
        }

        public override void StopAxis(bool bSlowStop = true)
        {
            if (m_nAxis < 0) return;
            if (bSlowStop) AXM("AxmMoveSStop", CAXM.AxmMoveSStop(m_nAxis));
            else AXM("AxmMoveEStop", CAXM.AxmMoveEStop(m_nAxis));
        }
        #endregion

        #region Move
        public override string StartMove(double fPos, string sSpeed = null)
        {
            if (IsInterlock()) return p_id + m_sCheckInterlock;
            p_sInfo = base.StartMove(fPos, sSpeed);
            if (p_sInfo != "OK") return p_sInfo;
            if (m_nAxis < 0) return p_id + " Axis not Assigned";
            if (AXM("AxmMoveStartPos", CAXM.AxmMoveStartPos(m_nAxis, fPos * p_pulsepUnit, m_speedNow.m_v * p_pulsepUnit, m_speedNow.m_acc, m_speedNow.m_dec)) != 0)
            {
                p_eState = eState.Init;
                p_sInfo = p_id + " Axis MoveStartPos Error";
                return p_sInfo;
            }
            p_eState = eState.Move;
            Thread.Sleep(10);
            return "OK";
        }

        public override string StartMove(double fPos, double v, double acc = -1, double dec = -1)
        {
            if (IsInterlock()) return p_id + m_sCheckInterlock;
            acc = (acc < 0) ? GetSpeedValue(eSpeed.Move).m_acc : acc;
            dec = (dec < 0) ? GetSpeedValue(eSpeed.Move).m_dec : dec;
            p_sInfo = base.StartMove(fPos, v, acc, dec);
            if (p_sInfo != "OK") return p_sInfo;
            if (m_nAxis < 0) return p_id + " Axis not Assigned";
            if (AXM("AxmMoveStartPos", CAXM.AxmMoveStartPos(m_nAxis, fPos * p_pulsepUnit, v * p_pulsepUnit, acc, dec)) != 0)
            {
                p_eState = eState.Init;
                p_sInfo = p_id + " Axis MoveStartPos Error";
                return p_sInfo;
            }
            p_eState = eState.Move;
            Thread.Sleep(10);
            return "OK";
        }

        public override string StartMoveV(double vStart, double posAt, double vChange, double posTo, double acc = -1, double dec = -1)
        {
            if (IsInterlock()) return p_id + m_sCheckInterlock;
            acc = (acc < 0) ? GetSpeedValue(eSpeed.Move).m_acc : acc;
            dec = (dec < 0) ? GetSpeedValue(eSpeed.Move).m_dec : dec;
            p_sInfo = base.StartMoveV(vStart, posAt, vChange, posTo, acc, dec);
            if (p_sInfo != "OK") return p_sInfo;
            if (m_nAxis < 0) return p_id + " Axis not Assigned";
            if (AXM("AxmOverrideVelAtPos", CAXM.AxmOverrideVelAtPos(m_nAxis, posTo, vStart, acc, dec, posAt, vChange, 0)) != 0)
            {
                p_eState = eState.Init;
                p_sInfo = p_id + " Axis AxmOverrideVelAtPos Error";
                return p_sInfo;
            }
            p_eState = eState.Move;
            Thread.Sleep(10);
            return "OK";
        }
        #endregion

        #region Shift
        public override string StartShift(double dfPos, string sSpeed = null)
        {
            double fPos = p_posCommand + dfPos;
            if (IsInterlock()) return p_id + m_sCheckInterlock;

            p_sInfo = base.StartShift(dfPos, sSpeed);
            if (p_sInfo != "OK") return p_sInfo;
            if (m_nAxis < 0) return p_id + " Axis not Assigned";
            if (AXM("AxmMoveStartPos", CAXM.AxmMoveStartPos(m_nAxis, fPos * p_pulsepUnit, m_speedNow.m_v * p_pulsepUnit, m_speedNow.m_acc, m_speedNow.m_dec)) != 0)
            {
                p_eState = eState.Init;
                p_sInfo = p_id + " Axis MoveStartPos Error";
                return p_sInfo;
            }
            p_eState = eState.Move;
            Thread.Sleep(10);
            return "OK";
        }

        public override string StartShift(double dfPos, double v, double acc = -1, double dec = -1)
        {
            double fPos = p_posCommand + dfPos;
            if (IsInterlock()) return p_id + m_sCheckInterlock;

            acc = (acc < 0) ? GetSpeedValue(eSpeed.Move).m_acc : acc;
            dec = (dec < 0) ? GetSpeedValue(eSpeed.Move).m_dec : dec;
            p_sInfo = base.StartShift(dfPos, v, acc, dec);
            if (p_sInfo != "OK") return p_sInfo;
            if (m_nAxis < 0) return p_id + " Axis not Assigned";
            if (AXM("AxmMoveStartPos", CAXM.AxmMoveStartPos(m_nAxis, fPos * p_pulsepUnit, v * p_pulsepUnit, acc, dec)) != 0)
            {
                p_eState = eState.Init;
                p_sInfo = p_id + " Axis MoveStartPos Error";
                return p_sInfo;
            }
            p_eState = eState.Move;
            Thread.Sleep(10);
            return "OK";
        }
        #endregion

        #region Home
        public override string StartHome()
        {
            if (IsInterlock()) return p_id + m_sCheckInterlock;

            if (m_bAbsoluteEncoder)
            {
                p_eState = eState.Ready;
                return "OK";
            }

            bool useLimit = m_bSWBoardLimit;
            if (useLimit)
            {
                CAXM.AxmSignalSetSoftLimit(m_nAxis, Convert.ToUInt32(!useLimit), 0, 1, m_aPos[p_asPos[3]], m_aPos[p_asPos[2]]);

                uint a = 0, b = 0;
                CAXM.AxmSignalReadSoftLimit(m_nAxis, ref a, ref b);
            }
          
            p_sInfo = base.StartHome();
            if (p_sInfo != "OK") return p_sInfo;
            if (AXM("AxmHomeSetMethod", CAXM.AxmHomeSetMethod(m_nAxis, (int)m_eHomeDir, (uint)m_eHomeSignal, (uint)m_eHomeZPhase, 1000, 0)) != 0) return p_sInfo;
            Speed[] speed = new Speed[2] { GetSpeedValue(eSpeed.Home_First), GetSpeedValue(eSpeed.Home_Last) };
            double[] v = new double[2] { speed[0].m_v * p_pulsepUnit, speed[1].m_v * p_pulsepUnit };
            if (AXM("AxmHomeSetVel", CAXM.AxmHomeSetVel(m_nAxis, v[0], v[0], v[1], v[1], speed[0].m_acc, speed[1].m_acc)) != 0) return p_sInfo;
            if (AXM("AxmHomeSetStart", CAXM.AxmHomeSetStart(m_nAxis)) != 0) return p_sInfo;
            p_eState = eState.Home;
            Thread.Sleep(10);

            return "OK";
        }

        const uint c_nAlarmReset = 0x02;
        public override string ResetAlarm()
        {
            uint nOutput = 0;
            if (p_sensorAlarm == false) return "OK";
            p_sInfo = "Reset Alarm";
            if (AXM("AxmSignalReadOutput", CAXM.AxmSignalReadOutput(m_nAxis, ref nOutput)) != 0) return p_sInfo;
            nOutput |= c_nAlarmReset;
            if (AXM("AxmSignalWriteOutput", CAXM.AxmSignalWriteOutput(m_nAxis, nOutput)) != 0) return p_sInfo;
            Thread.Sleep(50);
            nOutput -= c_nAlarmReset;
            if (AXM("AxmSignalWriteOutput", CAXM.AxmSignalWriteOutput(m_nAxis, nOutput)) != 0) return p_sInfo;
            p_eState = eState.Init;
            Thread.Sleep(100);
            return "OK";
        }

        public override void ServoOn(bool bOn)
        {
            if (EQ.p_bSimulate) return;
            if (bOn && m_bAbsoluteEncoder) CAXM.AxmM3ServoSensOn(m_nAxis);
            uint uOn = (uint)(bOn ? 1 : 0);
            if (AXM("AxmSignalServoOn", CAXM.AxmSignalServoOn(m_nAxis, uOn)) != 0) return;
            if (m_nBrakeSignalBit >= 0)
            {
                if (AXM("AxmSignalWriteOutputBit", CAXM.AxmSignalWriteOutputBit(m_nAxis, m_nBrakeSignalBit, uOn)) != 0) return;
            }
            if (bOn == false) p_eState = eState.Init;
            for (int n = 0; n < 200; n++)
            {
                Thread.Sleep(10); 
                if (bOn == p_bServoOn)
                {
                    Thread.Sleep(10);
                    return;
                }
            }
        }

        int _progressHome = 0; 
        public int p_progressHome
        {
            get { return _progressHome; }
            set
            {
                if (_progressHome == value) return;
                _progressHome = value;
                OnPropertyChanged(); 
            }
        }

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

        void GetAxisStatusHome()
        {
            //int i = 0; uint u0 = 0, u1 = 0; double f0 = 0, f1 = 0;
            //AXM("AxmHomeGetMethod", CAXM.AxmHomeGetMethod(m_nAxis, ref i, ref u0, ref u1, ref f0, ref f1));
            //m_eHomeDir = (eMoveDir)i;
            //m_eHomeSignal = (eHomeSignal)u0;
            //m_eHomeZPhase = (eHomeZPhase)u1;
        }

        void RunTreeSettingHome(Tree tree)
        {   
            m_eHomeDir = (eMoveDir)tree.Set(m_eHomeDir, m_eHomeDir, "Dir", "Search Home Direction");
            m_eHomeSignal = (eHomeSignal)tree.Set(m_eHomeSignal, m_eHomeSignal, "Signal", "Search Home Sensor");
            m_eHomeZPhase = (eHomeZPhase)tree.Set(m_eHomeZPhase, m_eHomeZPhase, "ZPhase", "Search Home ZPhase");
        }
        #endregion

        #region Setting Pulse & Encoder Mode
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
        eProfile m_eProfile = eProfile.SYM_S_CURVE_MODE;

        void GetAxisStatusMode()
        {
            uint u = 0;
            if (CAXM.AxmMotGetPulseOutMethod(m_nAxis, ref u) == 0) m_ePulse = (ePulseOutMethod)u;
            if (CAXM.AxmMotGetEncInputMethod(m_nAxis, ref u) == 0) m_eEncoder = (eEncoderMethod)u;
            AXM("AxmMotGetProfileMode", CAXM.AxmMotGetProfileMode(m_nAxis, ref u));
            m_eProfile = (eProfile)u;
            AXM("AxmMotGetMaxVel", CAXM.AxmMotGetMaxVel(m_nAxis, ref m_maxV));
        }

        int m_nBrakeSignalBit = -1;
        int m_nServoOnLevel = 1;
        double m_maxV = 4000000.0;
        void RunTreeSettingMode(Tree tree, bool bVisible = true, bool bReadOnly = false)
        {
            m_ePulse = (ePulseOutMethod)tree.Set(m_ePulse, m_ePulse, "Pulse", "Pulse Mode", bVisible, bReadOnly);
            m_eEncoder = (eEncoderMethod)tree.Set(m_eEncoder, m_eEncoder, "Encoder", "Encoder Mode", bVisible, bReadOnly);
            m_eProfile = (eProfile)tree.Set(m_eProfile, m_eProfile, "Profile", "Axis Velocity Profile", bVisible, bReadOnly);
            m_nBrakeSignalBit = tree.Set(m_nBrakeSignalBit, m_nBrakeSignalBit, "BrakeBit", "UNUSED(-1) USED(0~4)", bVisible, bReadOnly);
            m_nServoOnLevel = tree.Set(m_nServoOnLevel, m_nServoOnLevel, "ServoOnLevel", "LOW(0) HIGH(1)", bVisible, bReadOnly);
            m_maxV = tree.Set(m_maxV, m_maxV, "Max Velocity", "Max Velocity (pulse/sec)", bVisible, bReadOnly);
        }
        #endregion

        #region Setting Sensors
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

        void GetAxisStatusSensor()
        {
            uint u0 = 0, u1 = 0, u2 = 0;
            AXM("AxmSignalGetLimit", CAXM.AxmSignalGetLimit(m_nAxis, ref u0, ref u1, ref u2));
            m_eLimitP = (eSensorMethod)u1;
            m_eLimitM = (eSensorMethod)u2;
            AXM("AxmSignalGetInpos", CAXM.AxmSignalGetInpos(m_nAxis, ref u0));
            m_eInPos = (eSensorMethod)u0;
            AXM("AxmSignalGetServoAlarm", CAXM.AxmSignalGetServoAlarm(m_nAxis, ref u0));
            m_eAlarm = (eSensorMethod)u0;
            AXM("AxmSignalGetStop", CAXM.AxmSignalGetStop(m_nAxis, ref u0, ref u1));
            m_eEmergency = (eSensorMethod)u1;
            AXM("AxmHomeGetSignalLevel", CAXM.AxmHomeGetSignalLevel(m_nAxis, ref u0));
            m_eHome = (eSensorMethod)u0;
        }

        void RunTreeSettingSensor(Tree tree, bool bVisible = true, bool bReadOnly = false)
        {
            m_eHome = (eSensorMethod)tree.Set(m_eHome, m_eHome, "Home", "Home Sensor", bVisible, bReadOnly);
            m_eLimitM = (eSensorMethod)tree.Set(m_eLimitM, m_eLimitM, "Limit-", "Limit- Sensor", bVisible, bReadOnly);
            m_eLimitP = (eSensorMethod)tree.Set(m_eLimitP, m_eLimitP, "Limit+", "Limit+ Sensor", bVisible, bReadOnly);
            m_eInPos = (eSensorMethod)tree.Set(m_eInPos, m_eInPos, "InPos", "In Position Sensor", bVisible, bReadOnly);
            m_eAlarm = (eSensorMethod)tree.Set(m_eAlarm, m_eAlarm, "Alarm", "Alarm Sensor", bVisible, bReadOnly);
            m_eEmergency = (eSensorMethod)tree.Set(m_eEmergency, m_eEmergency, "Emergency", "Emergency Sensor", bVisible, bReadOnly);
        }
        #endregion

        #region Trigger
        bool m_bLevel = true;
        double m_dTrigTime = 2;
        public override void RunTrigger(bool bOn, Trigger trigger = null)
        {
            if (trigger == null) trigger = m_trigger;
            double dUpTime = (trigger.m_dUpTime < 0) ? m_dTrigTime : trigger.m_dUpTime; 
            if (m_nAxis < 0) return;
            AXM("AxmTriggerSetReset", CAXM.AxmTriggerSetReset(m_nAxis));
            if (bOn == false) return;
            uint nLevel = (uint)(m_bLevel ? 1 : 0);
            uint nEncoder = (uint)(trigger.m_bCmd ? 1 : 0);
            AXM("AxmTriggerSetTimeLevel", CAXM.AxmTriggerSetTimeLevel(m_nAxis, dUpTime, nLevel, nEncoder, 0));
            AXM("AxmTriggerSetBlock", CAXM.AxmTriggerSetBlock(m_nAxis, trigger.m_aPos[0] * p_pulsepUnit, trigger.m_aPos[1] * p_pulsepUnit, trigger.m_dPos * p_pulsepUnit));
        }

        public void RunTreeSettingTrigger(Tree tree)
        {
            m_bLevel = tree.Set(m_bLevel, m_bLevel, "Level", "Trigger Level, true = Active High");
            m_dTrigTime = tree.Set(m_dTrigTime, m_dTrigTime, "Time", "Trigger Out Time (us)");
        }
        #endregion

        #region UI Binding
        public override UserControl p_ui
        {
            get
            {
                AjinAxis_UI ui = new AjinAxis_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        #region Initialize Functions
        public void GetAxisStatus()
        {
            if (m_nAxis < 0) return;
            GetAxisStatusHome();
            GetAxisStatusMode();
            GetAxisStatusSensor();
            RunTreeSetting(Tree.eMode.Init);
        }

        public void SetAxisStatus()
        {
            if (m_nAxis < 0) return;
            //AXM("AxmMotSetPulseOutMethod", CAXM.AxmMotSetPulseOutMethod(m_nAxis, (uint)m_ePulse));
            //AXM("AxmMotSetEncInputMethod", CAXM.AxmMotSetEncInputMethod(m_nAxis, (uint)m_eEncoder));
            //AXM("AxmSignalSetServoOnLevel", CAXM.AxmSignalSetServoOnLevel(m_nAxis, (uint)m_nServoOnLevel));
            //AXM("AxmSignalSetLimit", CAXM.AxmSignalSetLimit(m_nAxis, 0, (uint)m_eLimitP, (uint)m_eLimitM));
            //AXM("AxmSignalSetInpos", CAXM.AxmSignalSetInpos(m_nAxis, (uint)m_eInPos));
            //AXM("AxmSignalSetServoAlarm", CAXM.AxmSignalSetServoAlarm(m_nAxis, (uint)m_eAlarm));
            //AXM("AxmSignalSetStop", CAXM.AxmSignalSetStop(m_nAxis, 0, (uint)m_eEmergency));
            //AXM("AxmHomeSetSignalLevel", CAXM.AxmHomeSetSignalLevel(m_nAxis, (uint)m_eHome));
        }

        void InitAxis()
        {
            if (m_nAxis < 0) return;
            AXM("AxmMotSetAbsRelMode", CAXM.AxmMotSetAbsRelMode(m_nAxis, 0));
            AXM("AxmMotSetAccelUnit", CAXM.AxmMotSetAccelUnit(m_nAxis, 1)); // [00h]unit/sec2 - [01h] sec 
            AXM("AxmMotSetProfileMode", CAXM.AxmMotSetProfileMode(m_nAxis, (uint)m_eProfile));
            AXM("AxmMotSetMaxVel", CAXM.AxmMotSetMaxVel(m_nAxis, m_maxV));
        }
        #endregion

        #region Ajin Functions
        public string SetGantry(AjinAxis axisSlave)
        {
            if (m_nAxis < 0) return "Axis not Assigned";
            if (axisSlave == null) return p_id + " SetGentry Slave Axis is null";
            if (axisSlave.m_nAxis < 0) return "Axis Slave not Assigned";
            if (AXM("AxmLinkResetMode", CAXM.AxmLinkResetMode(0)) != 0) return p_sInfo;
            if (AXM("AxmGantrySetDisable", CAXM.AxmGantrySetDisable(m_nAxis, axisSlave.m_nAxis)) != 0) return p_sInfo;
            if (AXM("AxmGantrySetEnable", CAXM.AxmGantrySetEnable(m_nAxis, axisSlave.m_nAxis, 0, 0, 0)) != 0) return p_sInfo;
            uint nOn = 0, nHome = 0;
            double fOffset = 0, fRange = 0;
            if (AXM("AxmGantryGetEnable", CAXM.AxmGantryGetEnable(m_nAxis, ref nHome, ref fOffset, ref fRange, ref nOn)) != 0) return p_sInfo;
            return "OK";
        }

        public void SetLoadRatio()
        {
            if (m_nAxis < 0) return;
            AXM("AxmStatusSetReadServoLoadRatio", CAXM.AxmStatusSetReadServoLoadRatio(m_nAxis, (uint)2));
        }

        public void ReadLoadRatio(ref double fLoadRatio)
        {
            if (m_nAxis < 0) return;
            AXM("AxmStatusReadServoLoadRatio", CAXM.AxmStatusReadServoLoadRatio(m_nAxis, ref fLoadRatio));
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_threadRun;
        Thread m_threadCheck;
        void InitThread()
        {
            if (m_bThread) return;
            m_bThread = true;
            m_threadRun = new Thread(new ThreadStart(RunThread));
            m_threadCheck = new Thread(new ThreadStart(RunThreadCheck));
            m_threadRun.Start();
            m_threadCheck.Start();
        }

        void RunThread()
        {
            uint nStat = 0;
            Thread.Sleep(2000);
            while (m_bThread)
            {
                Thread.Sleep(10);
                switch (p_eState)
                {
                    case eState.Home:
                        uint uMainStep = 0, uStep = 0;
                        CAXM.AxmHomeGetRate(m_nAxis, ref uMainStep, ref uStep);
                        p_progressHome = (int)uStep;
                        if (EQ.IsStop())
                        {
                            StopAxis();
                            ServoOn(false);
                            p_eState = eState.Init;
                        }
                        else
                        {
                            AXM("AxmHomeGetResult", CAXM.AxmHomeGetResult(m_nAxis, ref nStat));
                            if (nStat == 1)
                            {
                                p_sInfo = p_id + " -> Home Finished " + (m_swMove.ElapsedMilliseconds / 1000).ToString("0.0 sec");

                                if (m_bSWBoardLimit)
                                {
                                    CAXM.AxmSignalSetSoftLimit(m_nAxis, Convert.ToUInt32(m_bSWBoardLimit), 0, 1, m_aPos[p_asPos[3]], m_aPos[p_asPos[2]]);

                                    uint a = 0, b = 0;
                                    CAXM.AxmSignalReadSoftLimit(m_nAxis, ref a, ref b);

                                }

                                p_eState = eState.Ready;
                            }
                        }
                        break;
                    case eState.Move:
                        AXM("AxmStatusReadInMotion", CAXM.AxmStatusReadInMotion(m_nAxis, ref nStat));
                        if (nStat == 0) p_eState = eState.Ready;
                        break;
                    case eState.Jog:
                        AXM("AxmStatusReadInMotion", CAXM.AxmStatusReadInMotion(m_nAxis, ref nStat));
                        if (nStat == 0) p_eState = eState.Ready;
                        if (EQ.IsStop())
                        {
                            StopAxis();
                            p_eState = eState.Ready;
                        }
                        break;
                    default: break;
                }
                ThreadCheck_SWLimit(m_isPlus);
            }
        }

        void RunThreadCheck()
        {
            AXM("AxmSignalWriteOutput", CAXM.AxmSignalWriteOutput(m_nAxis, 0));
            Thread.Sleep(2000);
            while (m_bThread)
            {
                Thread.Sleep(1);
                if (m_nAxis >= 0)
                {
                    RunThreadCheck_Sensor();
                    RunThreadCheck_Position();
                }
            }
        }
        #endregion

        #region Thread Sensor
        void RunThreadCheck_Sensor()
        {
            uint uRead = 0;
            uint uReadM = 0;
            AXM("AxmSignalIsServoOn", CAXM.AxmSignalIsServoOn(m_nAxis, ref uRead));
            p_bServoOn = (uRead > 0);
            AXM("AxmHomeReadSignal", CAXM.AxmHomeReadSignal(m_nAxis, ref uRead));
            p_sensorHome = (uRead > 0); 
            AXM("AxmSignalReadLimit", CAXM.AxmSignalReadLimit(m_nAxis, ref uRead, ref uReadM));
            p_sensorMinusLimit = (uReadM > 0); 
            p_sensorPlusLimit = (uRead > 0);
            AXM("AxmSignalReadInpos", CAXM.AxmSignalReadInpos(m_nAxis, ref uRead));
            p_sensorInPos = (uRead > 0);
            AXM("AxmSignalReadServoAlarm", CAXM.AxmSignalReadServoAlarm(m_nAxis, ref uRead));
            p_sensorAlarm = (uRead > 0);
            if (m_eEmergency != eSensorMethod.UNUSED)
            {
                AXM("AxmSignalReadStop", CAXM.AxmSignalReadStop(m_nAxis, ref uRead));
                p_sensorEmergency = (uRead > 0);
            }
            if (p_sensorEmergency || p_sensorEmergency)
            {
                p_eState = eState.Init;
                Thread.Sleep(100);
            }
        }
        #endregion

        #region Interlock
        public class CSensor
        {
            public bool m_bHome = false;
            public bool m_bPlus = false;
            public bool m_bMinus = false;
            public string m_strAxisName = "";
            public CSensor(string strAxisName)
            {
                m_strAxisName = strAxisName;
            }
        }

        public List<CSensor> m_aSensors = new List<CSensor>();

        string m_sCheckInterlock = ""; 
        bool IsInterlock()
        {
            m_sCheckInterlock = CheckInterlock();
            return (m_sCheckInterlock != "OK"); 
        }

        string CheckInterlock()
        {
            for (int i = 0; i < m_aDIO_I.Count; i++)
            {
                if (m_bDIO_I[i])
                {
                    if (!m_aDIO_I[i].p_bIn)
                    {
                        string[] id = m_aDIO_I[i].m_id.Split('.');
                        return " : " + id[1] + " Interlock Error";
                    }
                }
            }
            if (p_vaccumDIO_I != null)
            {
                if (!p_IsLifterDown && p_vaccumDIO_I.p_bIn)
                {
                    string[] id = p_vaccumDIO_I.m_id.Split('.');
                    return " : " + id[1] + "Interlock Error";
                }
            }
            // IOList for true
            for (int i = 0; i < m_aSensors.Count; i++)
            {
                if (m_aSensors[i].m_bHome == true)
                {
                    if (!m_listAxis.m_aAxis[i].p_sensorHome) return " : HomeSensor Interlock Error";
                }
                if (m_aSensors[i].m_bPlus == true)
                {
                    if (!m_listAxis.m_aAxis[i].p_sensorPlusLimit) return " : Plus Limit Interlock Error";
                }
                if (m_aSensors[i].m_bMinus == true)
                {
                    if (!m_listAxis.m_aAxis[i].p_sensorMinusLimit) return " : Minus Limit Interlock Error";
                }
            }
            return "OK";
        }

        public override void RunTreeInterlock(Tree.eMode mode)
        {
            m_treeRootInterlock.p_eMode = mode;
            RunTreeInterlockAxis(m_treeRootInterlock.GetTree("Axis"));
        }
        
        void RunTreeInterlockAxis(Tree tree)
        {
            for (int i = 0; i < m_listAxis.m_aAxis.Count; i++)
            {
                CSensor sensor = new CSensor(m_listAxis.m_aAxis[i].p_id);
                int iIndex = m_aSensors.FindIndex(x => x.m_strAxisName == m_listAxis.m_aAxis[i].p_id);
                if (iIndex < 0) m_aSensors.Add(sensor);
                RunTreeSensor(m_treeRootInterlock.GetTree(m_aSensors[i].m_strAxisName), i);
            }
        }

        void RunTreeSensor(Tree tree, int iIndex)
        {
            m_aSensors[iIndex].m_bHome = tree.Set(m_aSensors[iIndex].m_bHome, m_aSensors[iIndex].m_bHome, "Home", "Home Sensor");
            m_aSensors[iIndex].m_bPlus = tree.Set(m_aSensors[iIndex].m_bPlus, m_aSensors[iIndex].m_bPlus, "Plus", "Plus Sensor");
            m_aSensors[iIndex].m_bMinus = tree.Set(m_aSensors[iIndex].m_bMinus, m_aSensors[iIndex].m_bMinus, "Minus", "Minus Sensor");
        }
        #endregion

        #region correction
        public void SetCorrection(bool bSet, double[] aPos, double[] adPos)
        {
            AXM("AxmCompensationEnable", CAXM.AxmCompensationEnable(m_nAxis, 0));
            if (bSet == false) return;
            if ((aPos == null) || (adPos == null)) return;
            if (aPos.Length != adPos.Length) return;
            int nL = aPos.Length + 2; 
            double[] aP = new double[nL-1]; 
            double[] adP = new double[nL];
            for (int n = 0; n < nL; n++) adP[n] = 0;
            aP[0] = 0;

            for (int n = nL - 1, nIdx = 1; n > 1; n--, nIdx++)
            {
                //aP[n] = aPos[n - 1];
                adP[nIdx] = adPos[n - 2]; 
            }
            for (int n = nL - 2  , nIdx = 1; n > 0; n-- , nIdx++)
            {
                aP[nIdx] = aPos[n - 1];
                //adP[n] = adPos[n - 1];
            }
            //aP[nL - 2] = aP[nL - 2] + aP[1]; 
            AXM("AxmCompensationSet", CAXM.AxmCompensationSet(m_nAxis, nL - 1, 0, aP, adP, 1)); 
            AXM("AxmCompensationEnable", CAXM.AxmCompensationEnable(m_nAxis, 1));
        }
        #endregion

        #region Tree
        public override void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeSpeed(m_treeRoot.GetTree("Speed"), m_sUnit);
            RunTreePos(m_treeRoot.GetTree("Position"), m_sUnit);
            m_trigger.RunTree(m_treeRoot.GetTree("Trigger",false), m_sUnit);

            bool bIOVisible = m_aDIO_I.Count > 0;
            RunTreeIOLock(m_treeRoot.GetTree("I/O Lock", true, bIOVisible), m_sUnit);
        }

        public override void RunTreePos(Tree tree, string sUnit)
        {
            base.RunTreePos(tree, sUnit);

            bool useLimit = m_bSWBoardLimit;
            uint use = Convert.ToUInt32(useLimit);
            uint res = CAXM.AxmSignalSetSoftLimit(m_nAxis, use, 0, 1, m_aPos[p_asPos[3]], m_aPos[p_asPos[2]]);

            uint a = 0, b = 0;
            CAXM.AxmSignalReadSoftLimit(m_nAxis, ref a, ref b);

        }

        public override void RunTreeSetting(Tree.eMode mode)
        {
            m_treeRootSetting.p_eMode = mode;
            RunTreeSettingHome(m_treeRootSetting.GetTree("Home"));
            RunTreeSettingProperty(m_treeRootSetting.GetTree("Property"));
            RunTreeSettingMode(m_treeRootSetting.GetTree("Mode"));
            RunTreeSettingSensor(m_treeRootSetting.GetTree("Sensor"));
            RunTreeSettingTrigger(m_treeRootSetting.GetTree("Trigger"));
            if (mode == Tree.eMode.RegRead) InitAxis(); 
            if (mode == Tree.eMode.Update) SetAxisStatus();
        }
        #endregion

        #region Compensation
        
        public override bool CompensationSet(double dstartpos, double[] dpPosition, double[] dpCorrection)
        {
            uint res = AXM("AxmCompensationSet", CAXM.AxmCompensationSet(m_nAxis, dpPosition.Length, dstartpos, dpPosition, dpCorrection, 0));
            if (res == 0)
                return true;
            else
                return false;        
        }

        public override bool EnableCompensation(int isenable)
        {
            uint res = AXM("AxmCompensationEnable", CAXM.AxmCompensationEnable(m_nAxis, (uint)isenable));
            if (res == 0)
                return true;
            else
                return false;
        }
        #endregion

        public AjinListAxis m_listAxis; 
        public void Init(AjinListAxis listAxis, string id, Log log)
        {
            m_listAxis = listAxis; 
            InitBase(id, log); 
            InitThread(); 
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

        static readonly object m_csLock = new object();
        List<string> m_aAXM = new List<string>(); 
        uint AXM(string sFunc, uint uResult)
        {
            lock (m_csLock)
            {
                if (uResult == 0)
                {
                    for (int n = 0; n < m_aAXM.Count; n++)
                    {
                        if (sFunc == m_aAXM[n])
                        {
                            m_aAXM.RemoveAt(n);
                            return uResult;
                        }
                    }
                    return uResult;
                }
                foreach (string sAXM in m_aAXM)
                {
                    if (sAXM == sFunc) return uResult;
                }
                m_aAXM.Add(sFunc);
                p_log.Error(sFunc + ", Error # = " + uResult.ToString());
                return uResult;
            }
        }
    }
}
