using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace RootTools_Vision
{
    /// <summary>
    /// 
    /// 이미지 버퍼 관련
    /// - WorkManager에서 MMF에 접근하던지 아니면 이미지 Buffer의 주소를 가져오던지 일단 전체 이미지의 주소를 가지고 있어야함
    /// - WorkManager가 이미지에 종속되는 것을 피하기 위해서(또는 경우에 따라서 이미지 버퍼가 바뀔 수 있음을 고려하여서 생성자에서 무조건 이미지 버퍼를 받는 동작은 하지않음 
    /// 
    /// 
    /// 
    /// </summary>
    public class WorkManager : IWorkable
    {
        #region [Event]
        public event EventChangedWorkState ChangedWorkState;
        public event EventReadyToWork ReadyToWork;
        public event EventWorkCompleted WorkCompleted;
        #endregion


        #region [Variables]
        public List<WorkBundle> works; // 여러 작업들이 미리 셋팅되어 있고, 선택해서 사용가능?
        public List<WorkplaceBundle> workplaces; // Sample Map과 같이 여러 Map들이 등록되어 있고, 선택해서 사용가능?

        WorkerManager workerManager;

        int workerNumber = 4;
        int indexWork;
        int indexWorkplace;


        private WorkResource workResource = null;   // WorkResource는 매 검사 시작 전에 외부로 부터 할당 받는다.

        #endregion



        public WorkManager(int _workerNumber)
        {
            this.works = new List<WorkBundle>();
            this.workplaces = new List<WorkplaceBundle>();
            this.workerManager = new WorkerManager(_workerNumber);
            this.workerManager.ChangedWorkState += ChangedWorkState_Callback;
            this.workerManager.ReadyToWork += ReadyToWork_Callback;

            //DatabaseManager.Instance.SetDatabase(workerNumber);
        }

        // WorkManager를 공유되는 외부 리소스와 함게 사용하기 위해서 WorkResource에 데이터를 할당해줘야한다.
        public void SetWorkResource(IntPtr _ptrImageBuffer, int _nImageSizeX, int _nImageSizeY)
        {
            this.workResource = new WorkResource(_ptrImageBuffer, _nImageSizeX, _nImageSizeY);
        }

        public bool AddWorkBundle(WorkBundle workBundle)
        {
            works.Add(workBundle);
            return true;
        }
        public bool AddWorkplaceBundle(WorkplaceBundle workplaceBundle)
        {
            workplaces.Add(workplaceBundle);
            return true;
        }

        /// <summary>
        /// 하나의 단일 작업을 수행...?
        /// Inspectino Mode 하나를 사용할 때와 같은 기능
        /// 그럼 두가지 이상의 작업을 하기 위해서는?
        /// TMA와 같이 ROI가 다른 작업이 있음... 이것을 하나의 작업꾸러미로 만들어야함
        /// </summary>
        /// <param name="indexWork"></param>
        /// <param name="indexWorkpalce"></param>
        public void SetWork(int indexWork, int indexWorkpalce)
        {
            // 작업할당 규칙
            // 1. 작업이 있는지 확인
            // 2. 사용가능한 작업자(Worker)가 있는지 확인
            // 3. 작업자에게 작업하도록 할당

            if (works.Count == 0 || workplaces.Count == 0
                || indexWork > this.works.Count - 1
                || indexWorkpalce > this.workplaces.Count - 1)
            {
                // LOG 작업장/작업 할당 필요
                // Index 에러
                return;
            }

            this.indexWork = indexWork;
            this.indexWorkplace = indexWorkpalce;

            // 작업과 작업장 선택
            WorkBundle workbundle = works[this.indexWork];

            WorkplaceBundle workplacebundle = workplaces[this.indexWorkplace];

            // 작업이 있는지 확인하는 기준
            // 작업장과 작업이 모두 존재해야함
            if (workbundle.Count == 0 || workplacebundle.Count == 0)
            {
                // LOG 작업장/작업 할당 필요
                return;
            }

            // workplace 를 선택하고
            // 작업자에게 할당해야함
            this.workerManager.SetWork(workplacebundle, workbundle);
        }

        private void ChangedWorkState_Callback(WORK_TYPE work_type, WORKER_STATE worker_state, WORKPLACE_STATE workplace_state, int indexWorkplace, Point SubIndex)
        {
            if(ChangedWorkState != null)
            {
                ChangedWorkState(work_type, worker_state, workplace_state, indexWorkplace, SubIndex);
            }
        }

        private void ReadyToWork_Callback(WORK_TYPE work_type, Workplace workplace)
        {
            if (ReadyToWork != null)
                ReadyToWork(work_type, workplace);
        }

        #region IWorkable 멤버
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

        public void Cancel()
        {
            this.workerManager.Cancel();
        }

        #endregion
    }
}
