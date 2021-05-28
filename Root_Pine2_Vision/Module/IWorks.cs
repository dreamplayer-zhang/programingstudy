using RootTools.ToolBoxs;
using RootTools.Trees;

namespace Root_Pine2_Vision.Module
{
    public interface IWorks
    {
        string p_id { get; set; }

        Vision.eWorks p_eWorks { get; set; }

        void ThreadStop(); 

        void GetTools(ToolBox toolBox, bool bInit);

        void RunTree(Tree tree);
    }
}
