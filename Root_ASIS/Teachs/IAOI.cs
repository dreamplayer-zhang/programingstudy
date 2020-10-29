using Root_ASIS.AOI;
using RootTools.Memory;
using RootTools.Trees;

namespace Root_ASIS.Teachs
{
    public interface IAOI
    {
        string p_id { get; set; }

        int p_nID { get; set; }

        bool p_bEnable { get; set; }

        void Draw(MemoryDraw draw, AOIData.eDraw eDraw);

        IAOI NewAOI();

        void ClearActive();

        void CalcROICount(ref int nReady, ref int nActive);

        AOIData GetAOIData(AOIData.eROI eROI); 

        void RunTreeAOI(Tree tree);

        void RunTreeROI(Tree tree);
    }
}
