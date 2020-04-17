using RootTools.Trees;

namespace RootTools.RTC5s
{
    public class Design_DXF : DesignBase
    {
        string m_sFile = "";
        public override void RunTree(Tree tree)
        {
            m_sFile = tree.SetFile(m_sFile, m_sFile, "DXF", "DXF File", "DXF File Name");
            base.RunTree(tree);
        }

        public override DesignBase MakeData(string sCode = "")
        {
            //forget
            //m_dataList.AddMove(x, y);
            //m_dataList.AddLine(x, y);
            return base.MakeData(sCode);
        }

        public Design_DXF()
        {
            m_bEnableHatch = true;
            m_bEnableCode = false;
        }
    }
}
