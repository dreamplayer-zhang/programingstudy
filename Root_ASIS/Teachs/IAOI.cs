using RootTools.Trees;

namespace Root_ASIS.Teachs
{
    public interface IAOI
    {
        string p_id { get; set; }

        int p_nID { get; set; }

        bool p_bEnable { get; set; }

        IAOI NewAOI(); 

        void RunTree(Tree tree);
    }
}
