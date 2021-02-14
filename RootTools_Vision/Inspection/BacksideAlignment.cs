using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class BacksideAlignment : WorkBase
    {
        #region [Member Variables]
        public override WORK_TYPE Type => WORK_TYPE.ALIGNMENT;

        OriginRecipe recipeOrigin;
        IntPtr InspectionSharedBuffer;

        protected override bool Preparation()
        {
            return true;
        }

        protected override bool Execution()
        {
            return true;
        }

        #endregion
    }
}
