using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class WorkFactory
    {
        //WorkManager workAlignment;
        //WorkManager workInspection;
        //WorkManager workProcessDefect;

        private List<WorkManager> workManagers;

        public WorkFactory()
        {
            Init();
            InitWorkManager();
        }

        protected virtual void InitWorkManager()
        {
        }

        public void  Init()
        {
            workManagers = new List<WorkManager>();

            //workAlignment = new WorkManager("Position", UserTypes.WORK_TYPE.PREPARISON, WORKPLACE_STATE.READY, WORKPLACE_STATE.NONE);

            //workInspection = new WorkManager("Inspection", UserTypes.WORK_TYPE.MAINWORK, WORKPLACE_STATE.INSPECTION, WORKPLACE_STATE.READY, 8);

            //workProcessDefect = new WorkManager("ProcessDefect", UserTypes.WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS, WORKPLACE_STATE.INSPECTION);
        }

        public void Add(WorkManager manager)
        {
            workManagers.Add(manager);
        }

        public void SetBundles(WorkBundle workbundle, WorkplaceBundle workplacebundle)
        {

            foreach(WorkManager manager in this.workManagers)
            {
                manager.SetBundles(workbundle, workplacebundle);
            }

            //workAlignment.SetBundles(workbundle, workplacebundle);
            //workInspection.SetBundles(workbundle, workplacebundle);
            //workProcessDefect.SetBundles(workbundle, workplacebundle);
        }

        public void Start()
        {
            foreach (WorkManager manager in this.workManagers)
            {
                manager.Start();
            }
            //workAlignment.Start();
            //workInspection.Start();
            //workProcessDefect.Start();
        }

        public void Stop()
        {
            foreach (WorkManager manager in this.workManagers)
            {
                manager.Stop();
            }
            //workAlignment.Stop();
            //workInspection.Stop();
            //workProcessDefect.Stop();
        }

        public void Pause()
        {
            foreach (WorkManager manager in this.workManagers)
            {
                manager.Pause();
            }
            //workAlignment.Pause();
            //workInspection.Pause();
            //workProcessDefect.Pause();
        }
    }
}
