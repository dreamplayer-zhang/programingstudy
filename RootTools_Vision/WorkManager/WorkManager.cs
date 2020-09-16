using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RootTools_Vision.UserTypes;

namespace RootTools_Vision
{
    public class WorkManager : IWorkable
    {
        private string id;
        private WORK_TYPE type;
        private List<Worker> workers;
        private WorkerManager workerManager;
        private WorkBundle workbundle;
        private WorkplaceBundle workplacebundle;

        public string Id { get => id; set => id = value; }


        /// <summary>
        /// WorkManager에서 객체를 생성해서 연결해주는 방식을 취함
        /// </summary>
        public WorkManager(string _id, WORK_TYPE _type,int workerNum = 1)
        {
            this.id = _id;
            if (workerNum < 1) workerNum = 1;

            this.type = _type;

            this.workers = new List<Worker>();
            for(int i= 0; i < workerNum; i++)
            {
                Worker worker = new Worker();
                workers.Add(worker);
            }

            this.workerManager = new WorkerManager(workers);
        }

        public void SetBundles(WorkBundle _workbundle, WorkplaceBundle _workplacebundle, WORKPLACE_STATE resultState, WORKPLACE_STATE excuteCondition)
        {
            if (_workbundle.Count == 0 || _workplacebundle.Count == 0)
            {
                // LOG 작업장/작업 할당 필요
                return;
            }

            this.workbundle = new WorkBundle();

            foreach (IWork work in _workbundle)
            {
                if(work.Type == this.type)
                    this.workbundle.Add(work);
            }

            //this.workbundle = _workbundle;
            this.workplacebundle = _workplacebundle;
            this.workplacebundle.Reset();

            this.workerManager.SetBundles(this.workbundle, this.workplacebundle, resultState, excuteCondition);
        }



        public void Start()
        {
            this.workerManager.Start();
        }

        public void Pause()
        {
            this.workerManager.Pause();
        }

        public void Stop()
        {
            this.workerManager.Stop();
        }
    }
}
