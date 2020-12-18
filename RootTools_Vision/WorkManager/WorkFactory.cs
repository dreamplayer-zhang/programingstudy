using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public abstract class WorkFactory
    {
        private List<WorkManager> workManagers;

        public WorkFactory()
        {
            Init();
            InitWorkManager();
        }

        protected abstract void InitWorkManager();

        public abstract bool CreateInspection(Recipe _recipe);
        public abstract bool CreateInspecion_Backside(Recipe _recipe);
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
            foreach (WorkManager manager in this.workManagers)
            {
                manager.Start();
            }
        }

        public void Stop()
        {
            foreach (WorkManager manager in this.workManagers)
            {
                manager.Stop();
            }
        }

        public void Pause()
        {
            foreach (WorkManager manager in this.workManagers)
            {
                manager.Pause();
            }
        }
    }
}
