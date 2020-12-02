using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.ShapeDraw
{
    interface IShapeManager<T>
    {
        void Set(T Shape);
        void Transform(double dScaleX, double dScaleY);
        void ScaleOffset(int nScale, int nOffsetX, int nOffsetY);
        void ScaleOffset(double dScale, int nOffsetX, int nOffsetY);

    }
}
