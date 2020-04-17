using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools.Control
{
    public interface IDIO
    {
        bool p_bEnableRun { get; set; }
        void RunDIO();
    }
}
