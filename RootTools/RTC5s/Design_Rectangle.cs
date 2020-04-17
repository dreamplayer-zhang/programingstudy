using RootTools.Trees;

namespace RootTools.RTC5s
{
    public class Design_Rectangle : DesignBase
    {
        RPoint m_szRectange = new RPoint(3, 3);

        public override void RunTree(Tree tree)
        {
            m_szRectange = tree.Set(m_szRectange, m_szRectange, "Size", "Rectangle Size (mm)");
            base.RunTree(tree);
        }

        public override DesignBase MakeData(string sCode = "")
        {
            m_dataList.Clear();
            m_dataList.AddMove(0, 0);
            m_dataList.AddLine(m_szRectange.X, 0);
            m_dataList.AddLine(m_szRectange.X, m_szRectange.Y);
            m_dataList.AddLine(0, m_szRectange.Y);
            m_dataList.AddLine(0, 0);
            return base.MakeData(sCode);
        }

        public Design_Rectangle()
        {
            m_bEnableHatch = true;
            m_bEnableCode = false;

        }
    }
}
