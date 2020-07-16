using RootTools.Trees;
using SPIIPLUSCOM660Lib;
using System;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.Control.ACS
{
    public class ACSAxis : Axis
    {
        #region ACS
        bool p_bConnect { get { return m_acs.p_bConnect; } }
        Channel p_channel { get { return m_acs.m_channel; } }
        #endregion

        #region Property
        int m_nAxis = -1;
        bool m_bAbsoluteEncoder = false;
        void RunTreeSettingProperty(Tree tree)
        {
            int nAxis = m_nAxis;
            m_nAxis = tree.Set(m_nAxis, m_nAxis, "Axis Number", "ACS Axis Number");
            m_bAbsoluteEncoder = tree.Set(m_bAbsoluteEncoder, m_bAbsoluteEncoder, "Absolute Encoder", "Absolute Encoder");
        }
        #endregion

        #region Position & Velocity
        public override void SetCommandPosition(double fPos)
        {
            if (m_acs.p_bConnect == false) return;
            try 
            {
                m_acs.m_channel.SetRPosition(m_nAxis, fPos);
                p_log.Info(p_id + " SetRPosition " + m_nAxis.ToString() + " = " + fPos.ToString()); 
            }
            catch (Exception e) { p_sInfo = p_id + " SetRPosition Error : " + e.Message; }
        }

        public override void SetActualPosition(double fPos)
        {
            if (m_acs.p_bConnect == false) return;
            try 
            { 
                m_acs.m_channel.SetFPosition(m_nAxis, fPos);
                p_log.Info(p_id + " SetFPosition " + m_nAxis.ToString() + " = " + fPos.ToString());
            }
            catch (Exception e) { p_sInfo = p_id + " SetFPosition Error : " + e.Message; }
        }

        public override void OverrideVelocity(double v)
        {
            if (m_acs.p_bConnect == false) return;
            try 
            { 
                m_acs.m_channel.SetVelocityImm(m_nAxis, v);
                p_log.Info(p_id + " SetVelocityImm " + m_nAxis.ToString() + " = " + v.ToString());
            }
            catch (Exception e) { p_sInfo = p_id + " SetVelocityImm Error : " + e.Message; }
        }

        void RunThreadCheck_Position()
        {
            if (m_acs.p_bConnect == false) return;
            try
            {
                p_posCommand = m_acs.m_channel.GetRPosition(m_nAxis);
                p_posActual = m_acs.m_channel.GetFPosition(m_nAxis);
                p_vNow = m_acs.m_channel.GetRVelocity(m_nAxis); 
            }
            catch (Exception e) { LogErrorPosition(p_id + " Check Position & Velocity Error : " + e.Message); }
        }

        StopWatch m_swErrorPosition = new StopWatch();
        void LogErrorPosition(string sError)
        {
            if (m_swErrorPosition.ElapsedMilliseconds < 5000) return;
            m_swErrorPosition.Restart();
            p_sInfo = sError; 
        }
        #endregion

        #region Jog
        public override string Jog(double fScale, string sSpeed = null)
        {
            p_sInfo = base.Jog(fScale, sSpeed);
            if (p_sInfo != "OK") return p_sInfo;
            if (m_nAxis < 0) return p_id + " Axis not Assigned";
            if (p_bConnect == false) return "ACS not Connected";
            try
            {
                double v = fScale * m_speedNow.m_v; 
                p_channel.SetAccelerationImm(m_nAxis, v / m_speedNow.m_acc);
                p_channel.SetDecelerationImm(m_nAxis, v / m_speedNow.m_dec);
                p_channel.Jog(p_channel.ACSC_AMF_VELOCITY, m_nAxis, v);
                p_log.Info(p_id + " Jog Start : " + v.ToString()); 
            }
            catch (Exception e)
            {
                p_sInfo = p_id + " Jog Start Error : " + e.Message;
                p_eState = eState.Init;
                return p_sInfo; 
            }
            p_eState = eState.Jog;
            return "OK";
        }

        public override void StopAxis(bool bSlowStop = true)
        {
            if (m_nAxis < 0) return;
            if (p_bConnect == false) return;
            try
            {
                if (bSlowStop) p_channel.Break(m_nAxis);
                else p_channel.Halt(m_nAxis);
                p_log.Info(p_id + " Jog Stop");
            }
            catch (Exception e) { p_sInfo = p_id + " Jog Start Error : " + e.Message; }
        }
        #endregion

        #region Move
        public override string StartMove(double fPos, string sSpeed = null)
        {
            p_sInfo = base.StartMove(fPos, sSpeed);
            if (p_sInfo != "OK") return p_sInfo;
            if (m_nAxis < 0) return p_id + " Axis not Assigned";
            if (p_bConnect == false) return "ACS not Connected";
            try
            {
                p_channel.SetAccelerationImm(m_nAxis, m_speedNow.m_v / m_speedNow.m_acc);
                p_channel.SetDecelerationImm(m_nAxis, m_speedNow.m_v / m_speedNow.m_dec);
                p_channel.SetVelocityImm(m_nAxis, m_speedNow.m_v); 
                p_channel.ToPoint(0, m_nAxis, fPos);
                p_log.Info(p_id + " Move Start : " + fPos.ToString());
            }
            catch (Exception e)
            {
                p_sInfo = p_id + " Move Start Error : " + e.Message;
                p_eState = eState.Init;
                return p_sInfo; 
            }
            p_eState = eState.Move;
            Thread.Sleep(100);
            return "OK";
        }

        public override string StartMove(double fPos, double v, double acc = -1, double dec = -1)
        {
            acc = (acc < 0) ? GetSpeedValue(eSpeed.Move).m_acc : acc;
            dec = (dec < 0) ? GetSpeedValue(eSpeed.Move).m_dec : dec;
            p_sInfo = base.StartMove(fPos, v, acc, dec);
            if (p_sInfo != "OK") return p_sInfo;
            if (m_nAxis < 0) return p_id + " Axis not Assigned";
            if (p_bConnect == false) return "ACS not Connected";
            try
            {
                p_channel.SetAccelerationImm(m_nAxis, v / acc);
                p_channel.SetDecelerationImm(m_nAxis, v / dec);
                p_channel.SetVelocityImm(m_nAxis, v);
                p_channel.ToPoint(0, m_nAxis, fPos);
                p_log.Info(p_id + " Move Start : " + fPos.ToString());
            }
            catch (Exception e)
            {
                p_sInfo = p_id + " Move Start Error : " + e.Message;
                p_eState = eState.Init;
                return p_sInfo;
            }
            p_eState = eState.Move;
            Thread.Sleep(100);
            return "OK";
        }
        #endregion

        //=======================================================

        #region Home
        public override string StartHome()
        {
            if (m_bAbsoluteEncoder)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            p_sInfo = base.StartHome();
            if (p_sInfo != "OK") return p_sInfo;
            //if (AXM("AxmHomeSetMethod", CAXM.AxmHomeSetMethod(m_nAxis, (int)m_eHomeDir, (uint)m_eHomeSignal, (uint)m_eHomeZPhase, 1000, 0)) != 0) return p_sInfo;
            //Speed v0 = GetSpeedValue(eSpeed.Home_First);
            //Speed v1 = GetSpeedValue(eSpeed.Home_Last);
            //if (AXM("AxmHomeSetVel", CAXM.AxmHomeSetVel(m_nAxis, v0.m_v, v0.m_v, v1.m_v, v1.m_v, v0.m_acc, v1.m_acc)) != 0) return p_sInfo;
            //if (AXM("AxmHomeSetStart", CAXM.AxmHomeSetStart(m_nAxis)) != 0) return p_sInfo;
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
            //if (AXM("AxmSignalReadOutput", CAXM.AxmSignalReadOutput(m_nAxis, ref nOutput)) != 0) return p_sInfo;
            //nOutput |= c_nAlarmReset;
            //if (AXM("AxmSignalWriteOutput", CAXM.AxmSignalWriteOutput(m_nAxis, nOutput)) != 0) return p_sInfo;
            //Thread.Sleep(50);
            //nOutput -= c_nAlarmReset;
            //if (AXM("AxmSignalWriteOutput", CAXM.AxmSignalWriteOutput(m_nAxis, nOutput)) != 0) return p_sInfo;
            p_eState = eState.Init;
            Thread.Sleep(100);
            return "OK";
        }

        public override void ServoOn(bool bOn)
        {
            if (EQ.p_bSimulate) return;
            if (p_bConnect == false) return;
            try
            {
                if (bOn) p_channel.Enable(m_nAxis);
                else p_channel.Disable(m_nAxis);
                p_log.Info(p_id + " Servo On = " + bOn.ToString()); 
            }
            catch (Exception e) { p_sInfo = p_id + " ServoOn Error : " + e.Message; }
            if (bOn == false) p_eState = eState.Init;
            Thread.Sleep(100);
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
            int i = 0; uint u0 = 0, u1 = 0; double f0 = 0, f1 = 0;
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

        #region Trigger
        bool m_bLevel = true;
        double m_dTrigTime = 2;
        public override void RunTrigger(bool bOn)
        {
            if (m_nAxis < 0) return;
            //AXM("AxmTriggerSetReset", CAXM.AxmTriggerSetReset(m_nAxis));
            //if (bOn == false) return;
            //uint nLevel = (uint)(m_bLevel ? 1 : 0);
            //uint nEncoder = (uint)(m_trigger.m_bCmd ? 1 : 0);
            //AXM("AxmTriggerSetTimeLevel", CAXM.AxmTriggerSetTimeLevel(m_nAxis, m_dTrigTime, nLevel, nEncoder, 0));
            //AXM("AxmTriggerSetBlock", CAXM.AxmTriggerSetBlock(m_nAxis, m_trigger.m_aPos[0], m_trigger.m_aPos[1], m_trigger.m_dPos));
        }

        public void RunTreeSettingTrigger(Tree tree)
        {
            m_bLevel = tree.Set(m_bLevel, m_bLevel, "Level", "Trigger Level (true = Active High)");
            m_dTrigTime = tree.Set(m_dTrigTime, m_dTrigTime, "Time", "Trigger Out Time (ms)");
        }
        #endregion

        #region UI Binding
        public override UserControl p_ui
        {
            get
            {
                ACSAxis_UI ui = new ACSAxis_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        #region ACS Functions
        public string SetPosTypeBound(double fPositivePos, double fNegativePos)
        {
            if (m_nAxis < 0) return "Axis not Assigned";
            //if (AXM("AxmStatusSetPosType", CAXM.AxmStatusSetPosType(m_nAxis, 1, fPositivePos, fNegativePos)) != 0) return p_sInfo;
            return "OK";
        }

        public string SetGantry(ACSAxis axisSlave)
        {
            if (m_nAxis < 0) return "Axis not Assigned";
            if (axisSlave == null) return p_id + " SetGentry Slave Axis is null";
            if (axisSlave.m_nAxis < 0) return "Axis Slave not Assigned";
            //if (AXM("AxmLinkResetMode", CAXM.AxmLinkResetMode(0)) != 0) return p_sInfo;
            //if (AXM("AxmGantrySetDisable", CAXM.AxmGantrySetDisable(m_nAxis, axisSlave.m_nAxis)) != 0) return p_sInfo;
            //if (AXM("AxmGantrySetEnable", CAXM.AxmGantrySetEnable(m_nAxis, axisSlave.m_nAxis, 0, 0, 0)) != 0) return p_sInfo;
            //uint nOn = 0, nHome = 0;
            //double fOffset = 0, fRange = 0;
            //if (AXM("AxmGantryGetEnable", CAXM.AxmGantryGetEnable(m_nAxis, ref nHome, ref fOffset, ref fRange, ref nOn)) != 0) return p_sInfo;
            return "OK";
        }

        public void SetLoadRatio()
        {
            if (m_nAxis < 0) return;
            //AXM("AxmStatusSetReadServoLoadRatio", CAXM.AxmStatusSetReadServoLoadRatio(m_nAxis, (uint)2));
        }

        public void ReadLoadRatio(ref double fLoadRatio)
        {
            if (m_nAxis < 0) return;
            //AXM("AxmStatusReadServoLoadRatio", CAXM.AxmStatusReadServoLoadRatio(m_nAxis, ref fLoadRatio));
        }
        #endregion

        #region Thread Sensor
        void RunThreadCheck_Sensor(Array aLimit)
        {
            if (p_bConnect == false) return; 
            try
            {
                int nMotor = p_channel.GetMotorState(m_nAxis);
                p_bSeroOn = ((nMotor & p_channel.ACSC_MST_ENABLE) != 0);
                p_sensorInPos = ((nMotor & p_channel.ACSC_MST_INPOS) != 0);
                int nLimit = (int)aLimit.GetValue(m_nAxis);
                p_sensorMinusLimit = (nLimit & p_channel.ACSC_SAFETY_LL) != 0;
                p_sensorPlusLimit = (nLimit & p_channel.ACSC_SAFETY_RL) != 0;
            }
            catch (Exception e) { LogErrorSensor(p_id + " Thread Check Sensor Error : " + e.Message); }
            //uint uRead = 0;
            //uint uReadM = 0;
            //AXM("AxmHomeReadSignal", CAXM.AxmHomeReadSignal(m_nAxis, ref uRead));
            //p_sensorHome = (uRead > 0);
            //AXM("AxmSignalReadServoAlarm", CAXM.AxmSignalReadServoAlarm(m_nAxis, ref uRead));
            //p_sensorAlarm = (uRead > 0);
            //if (m_eEmergency != eSensorMethod.UNUSED)
            //{
            //    AXM("AxmSignalReadStop", CAXM.AxmSignalReadStop(m_nAxis, ref uRead));
            //    p_sensorEmergency = (uRead > 0);
            //}
            if (p_sensorEmergency || p_sensorEmergency)
            {
                p_eState = eState.Init;
                Thread.Sleep(100);
            }
        }

        StopWatch m_swErrorSensor = new StopWatch();
        void LogErrorSensor(string sError)
        {
            if (m_swErrorSensor.ElapsedMilliseconds < 5000) return;
            m_swErrorSensor.Restart();
            p_sInfo = sError;
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_threadRun;
        void InitThread()
        {
            if (m_bThread) return;
            m_bThread = true;
            m_threadRun = new Thread(new ThreadStart(RunThread));
            m_threadRun.Start();
        }

        void RunThread()
        {
            uint nStat = 0;
            Thread.Sleep(2000);
            while (m_bThread)
            {
                Thread.Sleep(100);
                switch (p_eState)
                {
                    case eState.Home:
                        if (EQ.IsStop())
                        {
                            StopAxis();
                            ServoOn(false);
                            p_eState = eState.Init;
                        }
                        else
                        {
                            //AXM("AxmHomeGetResult", CAXM.AxmHomeGetResult(m_nAxis, ref nStat));
                            if (nStat == 1)
                            {
                                p_sInfo = p_id + " -> Home Finished " + (m_swMove.ElapsedMilliseconds / 1000).ToString("0.0 sec");
                                p_eState = eState.Ready;
                            }
                        }
                        break;
                    case eState.Move:
                        if (p_sensorInPos) p_eState = eState.Ready;
                        break;
                    case eState.Jog:
                        if (p_sensorInPos) p_eState = eState.Ready;
                        if (EQ.IsStop())
                        {
                            StopAxis();
                            p_eState = eState.Ready;
                        }
                        break;
                    default: break;
                }
            }
        }

        public void RunThreadCheck(Array aLimit)
        {
            if (m_nAxis >= 0)
            {
                RunThreadCheck_Sensor(aLimit);
                RunThreadCheck_Position();
            }
        }
        #endregion

        #region Tree
        public override void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeSpeed(m_treeRoot.GetTree("Speed"));
            RunTreePos(m_treeRoot.GetTree("Position"));
            m_trigger.RunTree(m_treeRoot.GetTree("Trigger"));
        }

        public override void RunTreeSetting(Tree.eMode mode)
        {
            m_treeRootSetting.p_eMode = mode;
            RunTreeSettingProperty(m_treeRootSetting.GetTree("Property"));
            RunTreeSettingHome(m_treeRootSetting.GetTree("Home"));
            RunTreeSettingTrigger(m_treeRootSetting.GetTree("Trigger"));
        }
        #endregion

        ACSListAxis m_listAxis;
        ACS m_acs;
        public void Init(ACSListAxis listAxis, string id, ACS acs)
        {
            m_listAxis = listAxis;
            m_acs = acs; 
            InitBase(id, acs.m_log);
            InitThread();
        }

        public void ThreadStop()
        {
            ServoOn(false);
            if (m_bThread)
            {
                m_bThread = false;
                m_threadRun.Join();
            }
        }
    }
}
