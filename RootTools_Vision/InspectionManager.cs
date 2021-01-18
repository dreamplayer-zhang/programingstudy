using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    internal class InspectionManager : WorkFactory
    {
        protected override WorkBundle CreateWorkBundle()
        {
            return new WorkBundle();
        }

        protected override WorkplaceBundle CreateWorkplaceBundle()
        {
            return new WorkplaceBundle();
        }

        protected override void Initialize()
        {
            CreateWorkManager(WORK_TYPE.SNAP);
            CreateWorkManager(WORK_TYPE.ALIGNMENT);
            CreateWorkManager(WORK_TYPE.INSPECTION);
        }

        protected override bool Ready(WorkplaceBundle workplaces, WorkBundle works)
        {
            return true;
        }
    }
}
