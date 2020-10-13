using RootTools.Trees;

namespace Root_ASIS.Teachs
{
    public interface IAOI
    {
        string p_id { get; set; }

        bool p_bEnable { get; set; }

        void RunTree(Tree tree);
    }
}
