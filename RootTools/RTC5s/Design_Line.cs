using RootTools.Trees;

namespace RootTools.RTC5s
{
    public class Design_Line : DesignBase
    {
        RPoint m_dpLine = new RPoint(3, 3);

        public override void RunTree(Tree tree)
        {
            m_dpLine = tree.Set(m_dpLine, m_dpLine, "Line", "Line (mm)");
            base.RunTree(tree);
        }

        public override DesignBase MakeData(string sCode = "")
        {
            m_dataList.Clear();
            m_dataList.AddMove(0, 0);
            m_dataList.AddLine(m_dpLine.X, m_dpLine.Y);
            return base.MakeData(sCode);
        }

        public Design_Line()
        {
            m_bEnableHatch = false;
            m_bEnableCode = false;
        }
    }
}
