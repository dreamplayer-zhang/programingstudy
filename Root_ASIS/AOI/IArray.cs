using RootTools;
using RootTools.Memory;
using RootTools.Trees;
using System.Collections.Generic;

namespace Root_ASIS.AOI
{
    public interface IArray : IAOI
    {
        void InvalidROI();

        string ReAllocate(MemoryData memory);

        List<CPoint> p_aArray { get; set; }

        void RunTreeArray(Tree tree);
    }
}
