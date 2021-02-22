using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    [Serializable]
    public class CloneableWorkBase : WorkBase
    {
        public CloneableWorkBase(string inspectionName)
        {
            this.InspectionName = inspectionName;
        }

        public readonly string InspectionName = "";

        public override WORK_TYPE Type => WORK_TYPE.NONE;

        protected override bool Execution()
        {
            return true;
            //throw new NotImplementedException();
        }

        protected override bool Preparation()
        {
            return true;
            //throw new NotImplementedException();
        }
    }
}
