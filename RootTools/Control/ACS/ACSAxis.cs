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
        string m_sUnit = "mm"; 
        bool m_bAbsoluteEncoder = false;
        void RunTreeSettingProperty(Tree tree)
        {
            int nAxis = m_nAxis;
            m_nAxis = tree.Set(m_nAxis, m_nAxis, "Axis Number", "ACS Axis Number");
            m_sUnit = tree.Set(m_sUnit, m_sUnit, "Unit", "AXS Axis Unit"); 
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
                p_posCommand = (int)Math.Round(100000 * m_acs.m_channel.GetRPosition(m_nAxis)) / 100000.0;
                p_posActual = (int)Math.Round(100000 * m_acs.m_channel.GetFPosition(m_nAxis)) / 100000.0;
                p_vNow = (int)Math.Round(100000 * m_acs.m_channel.GetRVelocity(m_nAxis)) / 100000.0;
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
                p_channel.SetAccelerationImm(m_nAxis, Math.Abs(v / m_speedNow.m_acc));
                p_channel.SetDecelerationImm(m_nAxis, Math.Abs(v / m_speedNow.m_dec));
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
                p_channel.Halt(m_nAxis);
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

        #region Shift
        public override string StartShift(double dfPos, string sSpeed = null)
        {
            double fPos = p_posCommand + dfPos;
            p_sInfo = base.StartShift(dfPos, sSpeed);
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

        public override string StartShift(double dfPos, double v, double acc = -1, double dec = -1)
        {
            double fPos = p_posCommand + dfPos;
            acc = (acc < 0) ? GetSpeedValue(eSpeed.Move).m_acc : acc;
            dec = (dec < 0) ? GetSpeedValue(eSpeed.Move).m_dec : dec;
            p_sInfo = base.StartShift(dfPos, v, acc, dec);
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
            p_sInfo = m_acs.m_aBuffer[m_nAxis].Run();
            if (p_sInfo != "OK") return p_sInfo;
            p_eState = eState.Home;
            Thread.Sleep(10);
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
        #endregion

        #region Trigger
        bool m_bTriggerOn = false; 
        bool m_bLevel = true;
        double m_dTrigTime = 2;
        public override void RunTrigger(bool bOn)
        {
            if (m_nAxis < 0) return;
            if (p_bConnect == false) return;
            try
            {
                if (m_bTriggerOn == bOn) return; 
                if (bOn == false)
                {
                    p_channel.StopPeg(m_nAxis);
                    p_log.Info("Trigger Off");
                }
                else
                {
                    p_channel.PegInc(p_channel.ACSC_AMF_SYNCHRONOUS, m_nAxis, m_dTrigTime / 1000.0, m_trigger.m_aPos[0], m_trigger.m_dPos, m_trigger.m_aPos[1], 0, 0);
                    string sTrigger = m_trigger.m_aPos[0].ToString() + " ~ " + m_trigger.m_aPos[1].ToString() + ", " + m_trigger.m_dPos.ToString();
                    p_log.Info("Trigger On : " + sTrigger + ", " + m_dTrigTime.ToString());
                }
                m_bTriggerOn = bOn; 
            }
            catch (Exception e)
            {
                p_sInfo = p_id + " Set Trigger Error : " + e.Message;
                p_eState = eState.Init;
                m_bTriggerOn = false; 
            }
        }

        public void RunTreeSettingTrigger(Tree tree)
        {
            m_bLevel = tree.Set(m_bLevel, m_bLevel, "Level", "Trigger Level (true = Active High)");
            m_dTrigTime = tree.Set(m_dTrigTime, m_dTrigTime, "Time", "Trigger Out Time (us)");
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
                            if (m_acs.m_aBuffer[m_nAxis].m_bRun == false)
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
            RunTreeSpeed(m_treeRoot.GetTree("Speed"), m_sUnit);
            RunTreePos(m_treeRoot.GetTree("Position"), m_sUnit);
            m_trigger.RunTree(m_treeRoot.GetTree("Trigger"), m_sUnit);
        }

        public override void RunTreeSetting(Tree.eMode mode)
        {
            m_treeRootSetting.p_eMode = mode;
            RunTreeSettingProperty(m_treeRootSetting.GetTree("Property"));
            RunTreeSettingTrigger(m_treeRootSetting.GetTree("Trigger"));
        }
        #endregion

        //=======================================================

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
