using RootTools.ToolBoxs;
using RootTools.Trees;

namespace Root_Pine2_Vision.Module
{
    public class Works3D : IWorks //forget
    {
        public string p_id { get; set; }

        public void GetTools(ToolBox toolBox, bool bInit)
        {
        }

        public void RunTree(Tree tree)
        {
        }

        public Vision.eWorks p_eWorks { get; set; }
        public Works3D(Vision.eWorks eWorks, Vision vision)
        {
            p_eWorks = eWorks; 
        }

        public void ThreadStop()
        {
        }
    }
}
