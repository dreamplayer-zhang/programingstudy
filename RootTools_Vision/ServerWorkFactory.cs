using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    class ServerWorkFactory : WorkFactory
    {
        public ServerWorkFactory() : base(REMOTE_MODE.Master)
        {

        }
        protected override WorkBundle CreateWorkBundle()
        {
            WorkBundle works = new WorkBundle();
            works.Add(new Position());
            works.Add(new D2D());

            return works;
        }

        protected override WorkplaceBundle CreateWorkplaceBundle()
        {
            WorkplaceBundle workplaces = new WorkplaceBundle();
            workplaces.Add(new Workplace(10, 10, 10, 10, 10, 10, 10));
            workplaces.Add(new Workplace(20, 20, 10, 10, 10, 10, 10));
            workplaces.Add(new Workplace(30, 30, 10, 10, 10, 10, 10));
            workplaces.Add(new Workplace(40, 40, 10, 10, 10, 10, 10));
            return workplaces;
        }

        protected override void Initialize()
        {
            this.CreateWorkManager(WORK_TYPE.ALIGNMENT, 2);
            this.CreateWorkManager(WORK_TYPE.INSPECTION, 2);
            this.CreateWorkManager(WORK_TYPE.DEFECTPROCESS_ALL, 2, true);
        }

        protected override bool Ready(WorkplaceBundle workplaces, WorkBundle works)
        {
            return true;
        }
    }
}
