using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace RootTools.RTC5s
{
    public class DesignBase
    {
        public RTC5Design.eDesign m_eDesign;

        public enum eMark
        {
            Outline,
            Hatching,
            All
        }
        public eMark m_eMark = eMark.Outline;
        public bool m_bEnableHatch = false;
        public bool m_bEnableCode = false;

        #region virtual
        public string m_id;
        public int m_nIndex;
        public string m_sMark;
        protected LogWriter m_log;
        protected RTC5 m_rtc5;
        public virtual void Init(int nIndex, RTC5 rtc5)
        {
            m_rtc5 = rtc5;
            m_log = rtc5.m_log;
            m_dataList = new DataList(rtc5);
            m_hatching = new Hatching(rtc5);
            m_nIndex = nIndex;
            string[] asName = this.GetType().Name.Split('_');
            m_sMark = asName[asName.Length - 1];
            m_id = nIndex.ToString("00") + "." + m_sMark;
            MakeData();
        }

        public virtual void RunTree(Tree tree)
        {
            m_eMark = (eMark)tree.Set(m_eMark, m_eMark, "Mark Method", "Mark Method", m_bEnableHatch);
            m_dataList.RunTreeLaserParameter(tree.GetTree("Laser Parameter", false), m_eMark != eMark.Hatching);
            m_hatching.RunTree(tree.GetTree("Hatching", false), m_eMark != eMark.Outline);
            if (tree.IsUpdated()) MakeData();
        }

        public virtual DesignBase MakeData(string sCode = "")
        {
            m_dataList.ShiftCenter();
            m_hatching.m_dataList.Clear();
            if (m_eMark != eMark.Outline) m_hatching.MakeHatching(m_dataList);
            DesignBase design = new DesignBase();
            design.m_dataList = new DataList(m_dataList);
            design.m_hatching = new Hatching(m_rtc5);
            design.m_hatching.m_dataList = new DataList(m_hatching.m_dataList);
            return design;
        }
        #endregion

        public DataList m_dataList;

        #region Hatching
        public class Hatching
        {
            double m_fGap = 0.1;
            double m_fAngle = 0;
            bool m_bZigzag = false;

            public DataList m_dataList;
            RTC5 m_rtc5;
            public Hatching(RTC5 rtc5)
            {
                m_rtc5 = rtc5;
                m_dataList = new DataList(rtc5);
            }

            public void RunTree(Tree tree, bool bVisible)
            {
                m_fGap = tree.Set(m_fGap, m_fGap, "Gap", "Hatching Gap (mm)", bVisible);
                m_fAngle = tree.Set(m_fAngle, m_fAngle, "Angle", "Hatching Angle (deg)", bVisible);
                m_bZigzag = tree.Set(m_bZigzag, m_bZigzag, "Zig zag", "Use Zig zag Hatching", bVisible);
                m_dataList.RunTreeLaserParameter(tree.GetTree("Laser Parameter", false), bVisible);
            }

            public void MakeHatching(DataList dataList)
            {
                m_dataList.Clear();
                if (dataList.m_aData.Count == 0) return;
                dataList.Rotate(m_fAngle);
                CalcYMinMax(dataList);
                bool bReverse = false;
                for (double y = m_yMin; y < m_yMax; y += m_fGap)
                {
                    MakeHatching(dataList, y, bReverse && m_bZigzag);
                    bReverse = !bReverse;
                }


                dataList.Rotate(-m_fAngle);
                m_dataList.Rotate(-m_fAngle);
            }

            double m_yMin = 0;
            double m_yMax = 0;
            void CalcYMinMax(DataList dataList)
            {
                m_yMax = m_yMin = dataList.m_aData[0].m_y;
                foreach (DataList.Data data in dataList.m_aData)
                {
                    m_yMin = Math.Min(m_yMin, data.m_y);
                    m_yMax = Math.Max(m_yMax, data.m_y);
                }
                int nHatch = (int)((m_yMax - m_yMin) / m_fGap);
                double dGap = (m_yMax - m_yMin) - nHatch * m_fGap;
                if (dGap < (m_fGap / 4)) dGap += m_fGap;
                m_yMin += dGap / 2;
                m_yMax -= dGap / 4;
            }

            void MakeHatching(DataList dataList, double y, bool bReverse)
            {
                List<double> aJunction = new List<double>();
                DataList.Data data0 = dataList.m_aData[0];
                if (data0.m_y == y)
                {
                    MakeHatching(dataList, y + m_fGap / 1000, bReverse);
                    return;
                }
                for (int n = 1; n < dataList.m_aData.Count; n++)
                {
                    DataList.Data data = dataList.m_aData[n];
                    if (data.m_y == y)
                    {
                        MakeHatching(dataList, y + m_fGap / 1000, bReverse);
                        return;
                    }
                    if (data.m_eCmd == DataList.Data.eCmd.Mark)
                    {
                        if ((((data0.m_y - y) * (data.m_y - y)) < 0) && (data.m_y != data0.m_y))
                        {
                            double x = data0.m_x + (data.m_x - data0.m_x) * (y - data0.m_y) / (data.m_y - data0.m_y);
                            aJunction.Add(x);
                        }
                    }
                    data0 = data;
                }
                if ((aJunction.Count % 2) != 0) return;
                aJunction.Sort();
                if (bReverse) aJunction.Reverse();
                for (int n = 0; n < aJunction.Count; n += 2) MakeHatching(y, aJunction[n], aJunction[n + 1]);
            }

            void MakeHatching(double y, double x0, double x1)
            {
                DataList.Data data0 = new DataList.Data(DataList.Data.eCmd.Jump, x0, y);
                DataList.Data data1 = new DataList.Data(DataList.Data.eCmd.Mark, x1, y);
                m_dataList.m_aData.Add(data0);
                m_dataList.m_aData.Add(data1);
            }
        }
        public Hatching m_hatching;
        #endregion

        #region Function
        public string Send(double fRotate, RPoint rpShift, double zShift = 0)
        {
            string sSend = m_dataList.Send(fRotate, rpShift, zShift);
            if (sSend != "OK") return sSend;
            return m_hatching.m_dataList.Send(fRotate, rpShift, zShift);
        }

        public int p_lBuffer
        {
            get
            {
                return 100 + m_dataList.m_aData.Count + m_hatching.m_dataList.m_aData.Count;
            }
        }
        #endregion
    }
}
