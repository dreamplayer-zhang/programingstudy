using RootTools.Trees;

namespace Root_Vega
{
    public interface IRobotChild
    {
        string p_id { get; set; }

        bool p_bLock { get; set; }

        InfoReticle p_infoReticle { get; set; }

        string IsGetOK(ref int posRobot);

        string IsPutOK(ref int posRobot, InfoReticle infoReticle);

        string BeforeGet();

        string BeforePut();

        string AfterGet();

        string AfterPut();

        bool IsReticleExist();

        void RunTeachTree(Tree tree);

        void ReadInfoReticle_Registry();
    }
}
