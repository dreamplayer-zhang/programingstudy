using RootTools.Memory;
using RootTools.ToolBoxs;
using RootTools.Trees;

namespace Root_Pine2_Vision.Module
{
    public class Works3D
    {
        public string p_id { get; set; }

        public void GetTools(ToolBox toolBox, bool bInit)
        {
        }

        public MemoryData[] p_memSnap
        {
            get { return null; }
        }

        public string SendRecipe(string sRecipe)
        {
            return "OK";
        }

        public string SendSnapDone(string sRecipe, int iSnap)
        {
            return "OK";
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }

        public void RunTree(Tree tree)
        {
        }

        public eWorks p_eWorks { get; set; }
        public Works3D(eWorks eWorks, Vision2D vision)
        {
            p_eWorks = eWorks; 
        }

        public void ThreadStop()
        {
        }
    }
}
