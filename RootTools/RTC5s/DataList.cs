using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace RootTools.RTC5s
{
    public class DataList
    {
        #region Property
        uint p_uHead
        {
            get { return m_setting.p_uHead; }
        }

        bool p_b3D
        {
            get { return m_setting.m_b3D; }
        }
        #endregion

        #region Data
        public class Data
        {
            public enum eCmd
            {
                Jump,
                Mark,
                Dot
            }
            public eCmd m_eCmd = eCmd.Jump;

            public double m_x;
            public double m_y;
            public double m_z;

            public int m_nDot = 1;
            public int m_usPeriod = 100;

            public double p_L
            {
                get
                {
                    return Math.Sqrt(m_x * m_x + m_y * m_y);
                }
            }

            public static Data operator +(Data data0, Data data1)
            {
                Data data = new Data(data0);
                data.m_x += data1.m_x;
                data.m_y += data1.m_y;
                data.m_z += data1.m_z;
                return data;
            }

            public static Data operator -(Data data0, Data data1)
            {
                Data data = new Data(data0);
                data.m_x -= data1.m_x;
                data.m_y -= data1.m_y;
                data.m_z -= data1.m_z;
                return data;
            }

            public static Data operator *(Data data0, double fMul)
            {
                data0.m_x *= fMul;
                data0.m_y *= fMul;
                data0.m_z *= fMul;
                return data0;
            }

            public static Data operator /(Data data0, double fDiv)
            {
                data0.m_x /= fDiv;
                data0.m_y /= fDiv;
                data0.m_z /= fDiv;
                return data0;
            }

            public Data(eCmd cmd, double x, double y, double z = 0)
            {
                m_eCmd = cmd;
                m_x = x;
                m_y = y;
                m_z = z;
            }

            public Data(int nDot, int usPeriod, double x, double y, double z = 0)
            {
                m_eCmd = eCmd.Dot;
                m_nDot = nDot;
                m_usPeriod = usPeriod;
                m_x = x;
                m_y = y;
                m_z = z;
            }

            public Data(Data data)
            {
                m_eCmd = data.m_eCmd;
                m_nDot = data.m_nDot;
                m_usPeriod = data.m_usPeriod;
                m_x = data.m_x;
                m_y = data.m_y;
                m_z = data.m_z;
            }
        }
        public List<Data> m_aData = new List<Data>();
        #endregion

        #region LaserParameter
        bool m_bDefault = true;
        LaserParameter _laserParameter = null;
        public LaserParameter p_laserParameter { get { return m_bDefault ? m_rtc5.m_laserParameter : _laserParameter; } }

        public void RunTreeLaserParameter(Tree tree, bool bVisible)
        {
            m_bDefault = tree.Set(m_bDefault, m_bDefault, "Use Default", "Use Default Laser Parameter", bVisible);
            if (m_bDefault == false)
            {
                if (_laserParameter == null) _laserParameter = new LaserParameter(m_rtc5);
                p_laserParameter.RunTree(tree, bVisible);
            }
        }
        #endregion

        #region Rotate & Shift
        public RPoint m_szData = new RPoint();
        public void ShiftCenter()
        {
            if (m_aData.Count == 0) return;
            RPoint rpMin = new RPoint(m_aData[0].m_x, m_aData[0].m_y);
            RPoint rpMax = new RPoint(m_aData[0].m_x, m_aData[0].m_y);
            foreach (Data data in m_aData)
            {
                rpMin.X = Math.Min(rpMin.X, data.m_x);
                rpMin.Y = Math.Min(rpMin.Y, data.m_y);
                rpMax.X = Math.Max(rpMax.X, data.m_x);
                rpMax.Y = Math.Max(rpMax.Y, data.m_y);
            }
            m_szData = rpMax - rpMin;
            double dx = (rpMin.X + rpMax.X) / 2;
            double dy = (rpMin.Y + rpMax.Y) / 2;
            foreach (Data data in m_aData)
            {
                data.m_x -= dx;
                data.m_y -= dy;
            }
        }

        public void Rotate(double fAngle)
        {
            double fCos = Math.Cos(Math.PI * fAngle / 180);
            double fSin = Math.Sin(Math.PI * fAngle / 180);
            foreach (Data data in m_aData)
            {
                double x = fCos * data.m_x - fSin * data.m_y;
                data.m_y = fCos * data.m_y + fSin * data.m_x;
                data.m_x = x;
            }
        }
        #endregion

        #region Functions
        public void Clear()
        {
            m_aData.Clear();
        }

        public void Add(Data data)
        {
            m_aData.Add(data);
        }

        public void AddMove(double x, double y, double z = 0)
        {
            m_aData.Add(new Data(Data.eCmd.Jump, x, y, z));
        }

        public void AddLine(double x, double y, double z = 0)
        {
            m_aData.Add(new Data(Data.eCmd.Mark, x, y, z));
        }

        public void AddDot(int nDot, int usPeriod, double x, double y, double z = 0)
        {
            m_aData.Add(new Data(nDot, usPeriod, x, y, z));
        }
        #endregion

        #region Send
        double m_fCos;
        double m_fSin;
        RPoint m_rpShift;
        double m_zShift;
        public string Send(double fRotate, RPoint rpShift, double zShift = 0)
        {
            m_fCos = Math.Cos(Math.PI * fRotate / 180);
            m_fSin = Math.Sin(Math.PI * fRotate / 180);
            m_rpShift = rpShift;
            m_zShift = zShift;
            p_laserParameter.SendLaserParam(true);
            foreach (Data data in m_aData)
            {
                if (m_rtc5.m_setting.m_b3D) Send3D(data);
                else Send2D(data);
            }
            return "OK";
        }

        void Send3D(Data data)
        {
            double x = m_fCos * data.m_x - m_fSin * data.m_y + m_rpShift.X;
            double y = m_fSin * data.m_x + m_fCos * data.m_y + m_rpShift.Y;
            CPoint cpSend = m_setting.CalcPos(x, y);
            int z = (int)Math.Round(data.m_z + m_zShift);
            switch (data.m_eCmd)
            {
                case Data.eCmd.Jump: RTC5Wrap.n_jump_abs_3d(p_uHead, cpSend.X, cpSend.Y, z); break;
                case Data.eCmd.Mark: RTC5Wrap.n_mark_abs_3d(p_uHead, cpSend.X, cpSend.Y, z); break;
                case Data.eCmd.Dot:
                    RTC5Wrap.n_jump_abs_3d(p_uHead, cpSend.X, cpSend.Y, z);
                    RTC5Wrap.n_laser_on_pulses_list(p_uHead, (uint)Math.Ceiling(data.m_usPeriod / 10.0), (uint)data.m_nDot);
                    break;
            }
        }

        void Send2D(Data data)
        {
            double x = m_fCos * data.m_x - m_fSin * data.m_y + m_rpShift.X;
            double y = m_fSin * data.m_x + m_fCos * data.m_y + m_rpShift.Y;
            CPoint cpSend = m_setting.CalcPos(x, y);
            switch (data.m_eCmd)
            {
                case Data.eCmd.Jump: RTC5Wrap.n_jump_abs(p_uHead, cpSend.X, cpSend.Y); break;
                case Data.eCmd.Mark: RTC5Wrap.n_mark_abs(p_uHead, cpSend.X, cpSend.Y); break;
                case Data.eCmd.Dot:
                    RTC5Wrap.n_jump_abs(p_uHead, cpSend.X, cpSend.Y);
                    RTC5Wrap.n_laser_on_pulses_list(p_uHead, (uint)Math.Ceiling(data.m_usPeriod / 10.0), (uint)data.m_nDot);
                    break;
            }
        }
        #endregion

        RTC5 m_rtc5;
        RTC5Setting m_setting;
        public DataList(RTC5 rtc5)
        {
            m_rtc5 = rtc5;
            m_setting = m_rtc5.m_setting;
        }

        public DataList(DataList dataList)
        {
            m_rtc5 = dataList.m_rtc5;
            m_aData.Clear();
            foreach (Data data in dataList.m_aData)
            {
                m_aData.Add(new Data(data));
            }
        }
    }
}
