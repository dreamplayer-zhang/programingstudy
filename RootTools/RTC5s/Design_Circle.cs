using RootTools.Trees;
using System;

namespace RootTools.RTC5s
{
    public class Design_Circle : DesignBase
    {
        RPoint m_szCircle = new RPoint(3, 3);
        public override void RunTree(Tree tree)
        {
            m_szCircle = tree.Set(m_szCircle, m_szCircle, "Size", "Circle diameter (mm)");
            base.RunTree(tree);
        }

        public override DesignBase MakeData(string sCode = "")
        {
            m_dataList.Clear();
            int nL = (int)Math.Round(Math.PI * (m_szCircle.X + m_szCircle.Y));
            if (nL < 36) nL = 36;
            if (nL > 360) nL = 360;
            double sx = m_szCircle.X / 2;
            double sy = m_szCircle.Y / 2;
            m_dataList.AddMove(sx, 0);
            double dt = 2 * Math.PI / nL;
            double t = dt;
            for (int n = 1; n < nL; n++, t += dt)
            {
                m_dataList.AddLine(sx * Math.Cos(t), sy * Math.Sin(t));
            }
            m_dataList.AddLine(sx, 0);
            return base.MakeData(sCode);
        }

        public Design_Circle()
        {
            m_bEnableHatch = true;
            m_bEnableCode = false;
        }
    }
}
