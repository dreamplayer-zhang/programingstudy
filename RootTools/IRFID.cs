using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools
{
    public interface IRFID
    {
        bool ReadID(ref string sID);
    }
}
