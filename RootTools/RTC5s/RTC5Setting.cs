using RootTools.Trees;
using System;

namespace RootTools.RTC5s
{
    public class RTC5Setting
    {
        #region Mark Position
        bool m_bXY = false;
        bool m_bX = false;
        bool m_bY = false;
        double m_fTheta = 0;
        double m_fArea = 150;
        double m_umImg = 10;

        void RunTreeMarkPos(Tree tree)
        {
            m_bXY = tree.Set(m_bXY, m_bXY, "X-Y", "Change X <-> Y");
            m_bX = tree.Set(m_bX, m_bX, "Inverse X", "Inverse X");
            m_bY = tree.Set(m_bY, m_bY, "Inverse Y", "Inverse Y");
            m_fTheta = tree.Set(m_fTheta, m_fTheta, "Theta", "Rotate Theta (deg)");
            m_fArea = tree.Set(m_fArea, m_fArea, "Area", "Marking Area (mm)");
            m_umImg = tree.Set(m_umImg, m_umImg, "Image", "Image Resolution (um)");
            m_fK = Math.Pow(2, 24) / m_fArea;
            m_fSin = Math.Sin(Math.PI * m_fTheta / 180);
            m_fCos = Math.Cos(Math.PI * m_fTheta / 180);
        }
        public double m_fK = 0;
        double m_fSin = 0;
        double m_fCos = 1;

        public CPoint CalcPos(double x, double y)
        {
            if (m_bXY)
            {
                double t = x;
                x = y;
                y = t;
            }
            if (m_bX) x = -x;
            if (m_bY) y = -y;
            int ix = (int)Math.Round(m_fK * (m_fCos * x - m_fSin * y));
            int iy = (int)Math.Round(m_fK * (m_fSin * x + m_fCos * y));
            return new CPoint(ix, iy);
        }
        #endregion

        #region Laser Power
        int m_minPower = 0;
        int m_maxPower = 4095;

        void RunTreePower(Tree tree)
        {
            m_minPower = tree.Set(m_minPower, m_minPower, "Minimum", "Minimum Power (0 ~ Max)");
            m_maxPower = tree.Set(m_maxPower, m_maxPower, "Maximun", "Maximum Power (Min ~ 4095)");
        }

        public uint CalcPower(double fPower)
        {
            fPower = (fPower < 0) ? 0 : fPower;
            fPower = (fPower > 100) ? 100 : fPower;
            return (uint)((m_maxPower - m_minPower) * fPower / 100 + m_minPower);
        }
        #endregion

        #region Standby Pulse
        double m_fFrequency = 10;
        double m_fPulseWidth = 0;

        void RunTreeStandby(Tree tree)
        {
            m_fFrequency = tree.Set(m_fFrequency, m_fFrequency, "Frequency", "Frequency (kHz)");
            m_fPulseWidth = tree.Set(m_fPulseWidth, m_fPulseWidth, "Pulse Width", "Pulse Width (us)");
            RTC5Wrap.n_set_standby(p_uHead, (uint)Math.Round(32000 / m_fFrequency), (uint)Math.Round(64 * m_fPulseWidth));
        }
        #endregion

        #region Mode
        public enum eLaserMode
        {
            CO2,
            YAG1,
            YAG2,
            YAG3,
            Laser4,
            YAG5,
            Laser6
        }
        eLaserMode _eLaserMode = eLaserMode.YAG2;
        public eLaserMode p_eLaserMode
        {
            get { return _eLaserMode; }
            set
            {
                _eLaserMode = value;
                RTC5Wrap.n_set_laser_mode(p_uHead, (uint)_eLaserMode);
            }
        }
        int _nControl = 0x18;
        public uint p_uControl
        {
            get { return (uint)_nControl; }
            set
            {
                _nControl = (int)value;
                RTC5Wrap.n_set_laser_control(p_uHead, value);
            }
        }
        public bool m_b3D = false;
        void RunTreeMode(Tree tree)
        {
            p_eLaserMode = (eLaserMode)tree.Set(p_eLaserMode, p_eLaserMode, "Laser Mode", "Laser Pulse Mode");
            p_uControl = (uint)tree.Set(_nControl, _nControl, "Control", "Set Laser Control Bit");
            m_b3D = tree.Set(m_b3D, m_b3D, "3D", "3D Scanner");
        }
        #endregion

        #region Head
        int _nHead = 1;
        public uint p_uHead { get { return (uint)_nHead; } }
        int m_nSerial = 0;
        void RunTreeHead(Tree tree)
        {
            _nHead = tree.Set(_nHead, _nHead, "Head ID", "Scan Head ID (1 ~)");
            m_nSerial = (int)RTC5Wrap.n_get_serial_number(p_uHead);
            m_nSerial = tree.Set(m_nSerial, m_nSerial, "Serial number", "Scan Head Serial Number", true, true);
        }
        #endregion 

        public void RunTree(Tree tree)
        {
            RunTreeHead(tree.GetTree("Head", false));
            RunTreeMode(tree.GetTree("Mode", false));
            RunTreeStandby(tree.GetTree("Standby Pulse", false));
            RunTreePower(tree.GetTree("Power", false));
            RunTreeMarkPos(tree.GetTree("Mark Position", false));
        }

        LogWriter m_log;
        public RTC5Setting(LogWriter log)
        {
            m_log = log;
        }
    }
}
