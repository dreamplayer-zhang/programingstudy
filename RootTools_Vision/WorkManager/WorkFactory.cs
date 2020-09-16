using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class WorkFactory
    {
        WorkManager workAlignment;
        WorkManager workInspection;
        WorkManager workProcessDefect;   

        public WorkFactory()
        {
            Init();
        }

        public void  Init()
        {
            workAlignment = new WorkManager("Position", UserTypes.WORK_TYPE.PREPARISON);

            workInspection = new WorkManager("Inspection", UserTypes.WORK_TYPE.MAINWORK, 8);

            workProcessDefect = new WorkManager("ProcessDefect", UserTypes.WORK_TYPE.FINISHINGWORK);
        }

        public void SetBundles(WorkBundle workbundle, WorkplaceBundle workplacebundle)
        {
            workAlignment.SetBundles(workbundle, workplacebundle, WORKPLACE_STATE.READY, WORKPLACE_STATE.NONE);
            workInspection.SetBundles(workbundle, workplacebundle, WORKPLACE_STATE.INSPECTION, WORKPLACE_STATE.READY);
            workProcessDefect.SetBundles(workbundle, workplacebundle, WORKPLACE_STATE.DEFECTPROCESS, WORKPLACE_STATE.INSPECTION);
        }

        public void Start()
        {
            workAlignment.Start();
            workInspection.Start();
            workProcessDefect.Start();
        }

        public void Stop()
        {
            workAlignment.Stop();
            workInspection.Stop();
            workProcessDefect.Stop();
        }

        public void Pause()
        {
            workAlignment.Pause();
            workInspection.Pause();
            workProcessDefect.Pause();
        }
    }
}
