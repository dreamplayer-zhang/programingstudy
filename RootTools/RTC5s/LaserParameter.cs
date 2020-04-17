using RootTools.Trees;
using System;

namespace RootTools.RTC5s
{
    public class LaserParameter
    {
        uint p_uHead
        {
            get { return m_setting.p_uHead; }
        }

        #region Pulse
        double m_fFrequency = 10;
        double m_fPulseWidth = 5;
        double m_fPower = 70;
        double m_fFPS = 1.5;

        void RunTreePulse(Tree tree, bool bVisible)
        {
            m_fFrequency = tree.Set(m_fFrequency, m_fFrequency, "Frequency", "Frequency (kHz)", bVisible);
            m_fPulseWidth = tree.Set(m_fPulseWidth, m_fPulseWidth, "Pulse Width", "Pulse Width (us)", bVisible);
            m_fPower = tree.Set(m_fPower, m_fPower, "Power", "Laser Power (%)", bVisible);
            m_fFPS = tree.Set(m_fFPS, m_fFPS, "FPS", "First Pulse suppression Width (us)", bVisible);
        }

        void SendPulse(bool bList)
        {
            uint uPower = m_setting.CalcPower(m_fPower);
            if (bList)
            {
                RTC5Wrap.n_set_laser_pulses(p_uHead, (uint)Math.Round(32000 / m_fFrequency), (uint)Math.Round(64 * m_fPulseWidth));
                RTC5Wrap.n_set_firstpulse_killer_list(p_uHead, (uint)Math.Round(64 * m_fFPS));
                RTC5Wrap.n_write_da_x_list(p_uHead, 1, uPower);
            }
            else
            {
                RTC5Wrap.n_set_laser_pulses_ctrl(p_uHead, (uint)Math.Round(32000 / m_fFrequency), (uint)Math.Round(64 * m_fPulseWidth));
                RTC5Wrap.n_set_firstpulse_killer(p_uHead, (uint)Math.Round(64 * m_fFPS));
                RTC5Wrap.n_write_da_x(p_uHead, 1, uPower);
            }
        }
        #endregion

        #region Jump & Mark
        class Move
        {
            public double m_fSpeed;
            public int m_usDelay;
            public Move(double fSpeed, int usDelay)
            {
                m_fSpeed = fSpeed;
                m_usDelay = usDelay;
            }

            public void RunTree(Tree tree, bool bVisible)
            {
                m_fSpeed = tree.Set(m_fSpeed, m_fSpeed, "Speed", "Scanner Move Speed (mm/ms)", bVisible);
                m_usDelay = tree.Set(m_usDelay, m_usDelay, "Delay", "Scanner Move Delay (us)", bVisible);
            }
        }
        Move m_moveJump = new Move(500, 200);
        Move m_moveMark = new Move(100, 100);
        void SendSpeed(bool bList)
        {
            if (bList)
            {
                RTC5Wrap.n_set_mark_speed(p_uHead, m_setting.m_fK * m_moveMark.m_fSpeed / 1000.0);
                RTC5Wrap.n_set_jump_speed(p_uHead, m_setting.m_fK * m_moveJump.m_fSpeed / 1000.0);
            }
            else
            {
                RTC5Wrap.n_set_mark_speed_ctrl(p_uHead, m_setting.m_fK * m_moveMark.m_fSpeed / 1000.0);
                RTC5Wrap.n_set_jump_speed_ctrl(p_uHead, m_setting.m_fK * m_moveJump.m_fSpeed / 1000.0);
            }
        }
        #endregion

        #region Delay
        int m_usPolygon = 50;
        int m_usLaserOn = 50;
        int m_usLaserOff = 100;
        void RunTreeDelay(Tree tree, bool bVisible)
        {
            m_usPolygon = tree.Set(m_usPolygon, m_usPolygon, "Polygon", "Scanner Polygon Delay (us)", bVisible);
            m_usLaserOn = tree.Set(m_usLaserOn, m_usLaserOn, "Laser On", "Laser On Delay (us)", bVisible);
            m_usLaserOff = tree.Set(m_usLaserOff, m_usLaserOff, "Laser Off", "Laser On Delay (us)", bVisible);
        }

        void SendDelay()
        {
            RTC5Wrap.n_set_scanner_delays(p_uHead, (uint)(m_moveJump.m_usDelay / 10), (uint)(m_moveMark.m_usDelay / 10), (uint)(m_usPolygon / 10));
            RTC5Wrap.n_set_laser_delays(p_uHead, 2 * m_usLaserOn, (uint)(2 * m_usLaserOff));
        }
        #endregion

        #region Tree
        public void RunTree(Tree tree, bool bVisible)
        {
            RunTreePulse(tree.GetTree("Pulse", false), bVisible);
            m_moveMark.RunTree(tree.GetTree("Mark", false), bVisible);
            m_moveJump.RunTree(tree.GetTree("Jump", false), bVisible);
            RunTreeDelay(tree.GetTree("Delay", false), bVisible);
        }
        #endregion

        public void SendLaserParam(bool bList)
        {
            SendPulse(bList);
            SendSpeed(bList);
            SendDelay();
        }

        LogWriter m_log;
        RTC5Setting m_setting;
        public LaserParameter(RTC5 rtc5)
        {
            m_log = rtc5.m_log;
            m_setting = rtc5.m_setting;
            if (rtc5.m_laserParameter == null) return;
            m_fFrequency = rtc5.m_laserParameter.m_fFrequency;
            m_fPulseWidth = rtc5.m_laserParameter.m_fPulseWidth;
            m_fPower = rtc5.m_laserParameter.m_fPower;
            m_fFPS = rtc5.m_laserParameter.m_fFPS;
            m_moveJump.m_fSpeed = rtc5.m_laserParameter.m_moveJump.m_fSpeed;
            m_moveJump.m_usDelay = rtc5.m_laserParameter.m_moveJump.m_usDelay;
            m_moveMark.m_fSpeed = rtc5.m_laserParameter.m_moveMark.m_fSpeed;
            m_moveMark.m_usDelay = rtc5.m_laserParameter.m_moveMark.m_usDelay;
            m_usPolygon = rtc5.m_laserParameter.m_usPolygon;
            m_usLaserOn = rtc5.m_laserParameter.m_usLaserOn;
            m_usLaserOff = rtc5.m_laserParameter.m_usLaserOff;
        }
    }
}
