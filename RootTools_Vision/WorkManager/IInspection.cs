using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public enum INSPECTION_TYPE
    {
        D2D = 1000,
        Surface = 2000,
    }

    interface IInspection
    {
        INSPECTION_TYPE TYPE { get; }

        void SetParameter(IParameter parameter);
        void DoInspection(Workplace workplace, out InspectionResultCollection result);
    }
}
