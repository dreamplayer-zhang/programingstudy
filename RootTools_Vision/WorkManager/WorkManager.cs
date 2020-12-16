using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    public enum WORK_TYPE
    {
        PREPARISON,
        MAINWORK,
        AFTERWORK,
        FINISHINGWORK
    }

    public enum STATE_CHECK_TYPE
    {
        CHIP = 0,
        LINE,
        WAFER
    }

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
        public WorkManager(string _id, WORK_TYPE _type, WORKPLACE_STATE _resultState, WORKPLACE_STATE _excuteCondition, STATE_CHECK_TYPE _state_check_type,  int workerNum = 1)
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

            this.workerManager = new WorkerManager(workers, _resultState, _excuteCondition, _state_check_type);
        }

        public bool SetBundles(WorkBundle _workbundle, WorkplaceBundle _workplacebundle)
        {
            if (_workbundle.Count == 0 || _workplacebundle.Count == 0)
            {
                // LOG 작업장/작업 할당 필요
                MessageBox.Show("Work/Workplace Bundle이 없습니다.");
                return false;
            }

            this.workbundle = new WorkBundle();

            foreach (WorkBase work in _workbundle)
            {
                work.IsPreworkDone = false;
                work.IsWorkDone = false;

                if (work.Type == this.type)
                    this.workbundle.Add(work);
            }

            //this.workbundle = _workbundle;
            this.workplacebundle = _workplacebundle;
            this.workplacebundle.Reset();

            this.workerManager.SetBundles(this.workbundle, this.workplacebundle);

            return true;
        }



        public void Start()
        {
            this.workerManager.Start();
        }

        public void Stop()
        {
            this.workerManager.Stop();
        }
    }
}
