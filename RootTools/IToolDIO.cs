using System.Collections.Generic;
using RootTools.Control; 

namespace RootTools
{
    public interface IToolDIO
    {
        ListDIO p_listDI { get; }
        ListDIO p_listDO { get; }
        List<IDIO> p_listIDIO { get; }
    }
}
