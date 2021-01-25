using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.delete
{
    public abstract class WorkFactory
    {
        private List<WorkManager> workManagers;

        private bool isStop = true;

        public bool IsStop
        {
            get => this.isStop;
        }

        public WorkFactory()
        {
            Init();
            InitWorkManager();
        }

        protected abstract void InitWorkManager();

        public abstract bool CreateInspection(RecipeBase _recipe);

        public void  Init()
        {
            workManagers = new List<WorkManager>();
        }

        protected void Add(WorkManager manager)
        {
            workManagers.Add(manager);
        }

        public bool SetBundles(WorkBundle workbundle, WorkplaceBundle workplacebundle)
        {
            foreach (WorkManager manager in this.workManagers)
            {
                if(manager.SetBundles(workbundle, workplacebundle) == false)
                    return false;
            }
            return true;
        }

        protected void Start()
        {
            this.isStop = false;
            foreach (WorkManager manager in this.workManagers)
            {
                manager.Start();
            }

            WorkEventManager.ProcessDefectWaferDone += OnProcessDefectWaferDone_Callback;
        }

        public void Stop()
        {
            this.isStop = true;
            foreach (WorkManager manager in this.workManagers)
            {
                manager.Stop();
            }

            WorkEventManager.ProcessDefectWaferDone -= OnProcessDefectWaferDone_Callback;
        }

        public void OnProcessDefectWaferDone_Callback(object obj, ProcessDefectWaferDoneEventArgs args)
        {
            Stop();
        }

        ~WorkFactory()
        {
            Stop();
        }
    }
}
