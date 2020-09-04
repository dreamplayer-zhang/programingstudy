using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class D2D : Inspection, IInspection
    {
        #region IInspection 멤버

        public new INSPECTION_TYPE TYPE
        {
            get { return INSPECTION_TYPE.D2D; }
        }

        public void DoInspection(Workplace workplace, out InspectionResultCollection result)
        {
            throw new NotImplementedException();
        }

        public void SetParameter(IParameter parameter)
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}
