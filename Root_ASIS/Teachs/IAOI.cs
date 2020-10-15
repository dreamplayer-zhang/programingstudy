using Root_ASIS.AOI;
using RootTools.Trees;
using System.Collections.ObjectModel;

namespace Root_ASIS.Teachs
{
    public interface IAOI
    {
        string p_id { get; set; }

        int p_nID { get; set; }

        bool p_bEnable { get; set; }

        IAOI NewAOI();

        void AddROI(ObservableCollection<AOIData> aROI);

        void RunTree(Tree tree);
    }
}
