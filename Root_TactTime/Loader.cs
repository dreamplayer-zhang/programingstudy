using RootTools;
using RootTools.Trees;
using System;

namespace Root_TactTime
{
    public class Loader
    {
        #region Loader
        public static double m_v = 1;
        public static double m_secAcc = 0.3;
        public static double m_secDec = 0.3;
        public static void RunTreeLoader(Tree tree)
        {
            m_v = tree.Set(m_v, m_v, "Speed", "Axis Move Speed (m / sec)");
            m_secAcc = tree.Set(m_secAcc, m_secAcc, "Acc", "Axis Acc Time (sec)");
            m_secDec = tree.Set(m_secDec, m_secDec, "Dec", "Axis Dec Time (sec)");
        }
        #endregion

        #region Move
        public RPoint m_rp = new RPoint();
        public void Move(RPoint rpDst)
        {
            RPoint dp = rpDst - m_rp;
            double dL = Math.Max(dp.X, dp.Y);
            double fTime = m_secAcc + m_secDec;
            dL -= fTime * m_v;
            if (dL > 0) fTime += dL / m_v;
            m_tact.AddEvent(fTime, "Loader Axis Move");
            m_rp = rpDst; 
        }

        #endregion

        public void Add(Picker picker)
        {
            m_tact.m_aPicker.Add(picker); 
        }

        public TactTime m_tact;
        public string p_id { get; set; }
        public Loader(TactTime tact, string id)
        {
            m_tact = tact;
            p_id = id; 
        }
    }
}
