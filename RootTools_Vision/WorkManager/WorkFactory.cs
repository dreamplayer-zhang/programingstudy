using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class WorkFactory
    {
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
        }

        public void Add(WorkManager manager)
        {
            workManagers.Add(manager);
        }

        public void SetBundles(WorkBundle workbundle, WorkplaceBundle workplacebundle)
        {
            foreach (WorkManager manager in this.workManagers)
            {
                manager.SetBundles(workbundle, workplacebundle);
            }
        }

        public void Start()
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
