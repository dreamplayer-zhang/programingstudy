using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public enum POSITION_TYPE
    {
        Feature = 100,
        Edge = 200,
    }

    interface IPosition
    {
        POSITION_TYPE TYPE { get; }

        void DoPosition();
    }
}
