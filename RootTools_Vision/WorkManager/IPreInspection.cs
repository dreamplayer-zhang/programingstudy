using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public enum PREINSP_TYPE
    {
        NORMAL = 100,
    }

    interface IPreInspection
    {
        PREINSP_TYPE TYPE { get; }

        void DoPreInspection();
    }

}
