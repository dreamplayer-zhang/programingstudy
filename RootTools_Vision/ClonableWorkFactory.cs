using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    class ClonableWorkFactory : WorkFactory
    {
        public ClonableWorkFactory() : base(REMOTE_MODE.Slave)
        {

        }

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
            
        }

        protected override bool Ready(WorkplaceBundle workplaces, WorkBundle works)
        {
            return true;
        }
    }
}
