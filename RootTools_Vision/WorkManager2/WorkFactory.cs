using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    public abstract class WorkFactory : IWorkStartable
    {
        private List<WorkManager> workManagers;

        //List<WorkplaceBundle> bundleList = new List<WorkplaceBundle>();
        //WorkplaceBundle workplaceBundle;

        //private WorkplaceBundle workplaces; // WorkplaceBundle은 유지하고 WorkBundle만 새로 생성하게...

        //List<SharedBufferInfo> bufferInfoList;

        public WorkFactory()
        {
            workManagers = new List<WorkManager>();

            Initialize();
        }

        /// <summary>
        /// Workplace는 객체를 유지하고, Work는 한번 사용 후 삭제
        /// </summary>
        /// <param name="bundle"></param>
        /// <returns></returns>
        //private WorkplaceBundle SetWorkplaceBundle(WorkplaceBundle bundle)
        //{
        //    if (bundle == null || bundle.Count == 0)
        //    {
        //        throw new ArgumentNullException("WorkplaceBundle이 null이거나 데이터가 없습니다.");
        //        return false;
        //    }

        //    workplaceBundle = bundle.Clone(); //Clone

        //    foreach(WorkManager wm in this.workManagers)
        //    {
        //        wm.SetWorkplaceBundle(this.workplaceBundle);
        //    }

        //    return true;
        //}

        /// <summary>
        /// Workplace는 객체를 유지하고, Work는 한번 사용 후 삭제
        /// </summary>
        //public bool SetWorkBundle(WorkBundle bundle)
        //{
        //    foreach(WorkManager wm in this.workManagers)
        //    {
        //        wm.SetWorkBundle(bundle);
        //    }
            
        //    return true;
        //}

        #region [abstract method]
        /// <summary>
        /// - CreateWorkManager 메서드를 사용해서 작업을 수행할 작업관리자(WorkManager)를 생성하세요
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// 검사 정보(Recipe)를 기반으로 검사영역(Workplace)과 검사작업(Work)를 생성합니다.
        /// - CreateWorkplaceBundle 메서드를 사용하여 검사 영역을 생성하십시오
        /// </summary>
        //public abstract bool CreateInspection();

        /// <summary>
        /// 개인 입맛에 맞게 WorkplaceBundle을 생성
        /// CreateWorkplaceBundle Inspection 시작전에 수행합니다.
        /// </summary>
        /// <returns>false 반환시에 에러 메시지를 띄웁니다.</returns>
        protected abstract WorkplaceBundle CreateWorkplaceBundle();

        /// <summary>
        /// 개인 입맛에 맞게 WorkBundle을 생성
        /// Inspection 시작전에 수행합니다.
        /// </summary>
        /// <returns>false 반환 시에 에러메시지를 띄웁니다.</returns>
        protected abstract WorkBundle CreateWorkBundle();



        protected abstract bool Ready(WorkplaceBundle workplaces, WorkBundle works);

        #endregion

        public void Exit()
        {
            foreach (WorkManager wm in this.workManagers)
                wm.Exit();
            this.workManagers.Clear();
        }


        /// <summary>
        /// WorkplaceBundle 생성
        /// WorkBundle 생성
        /// 검사 시작
        /// </summary>
        /// <param name="type">검사 시작 전 Workplace 상태(Default : NONE)</param>
        public void Start(WORK_TYPE type = WORK_TYPE.NONE)
        {
            Stop();
            //GC.Collect(2, GCCollectionMode.Optimized);

            WorkplaceBundle workplaces = CreateWorkplaceBundle();
            WorkBundle works = CreateWorkBundle();
            works.SetWorkplacBundle(workplaces);

            if (Ready(workplaces, works) == false)
            {
                MessageBox.Show("검사 정보 생성에 실패하였습니다");
                return;
            }

            // Workplace State 초기화
            workplaces.SetWorkState(type);

#if DEBUG
            Debug.WriteLine("[Start]");
            DebugOutput.PrintWorkplaceBundle(workplaces);
#endif

            foreach (WorkManager wm in this.workManagers)
            {
                wm.SetWorkplaceBundle(workplaces);
                wm.SetWorkBundle(works);
                wm.Start();
            }
        }

        public void Start()
        {
            //GC.Collect() 이거 해야하나

            WorkplaceBundle workplaces = CreateWorkplaceBundle();
            WorkBundle works = CreateWorkBundle();
            works.SetWorkplacBundle(workplaces);

            if (Ready(workplaces, works) == false)
            {
                MessageBox.Show("검사 정보 생성에 실패하였습니다");
                return;
            }

            foreach (WorkManager wm in this.workManagers)
            {
                wm.SetWorkplaceBundle(workplaces);
                wm.SetWorkBundle(works);
                wm.Start();
            }
        }

        public void Stop()
        {
            foreach (WorkManager wm in this.workManagers)
                wm.Stop();
        }


        // 데이터가 변경되지 않았을 때...?
        public void Restart()
        {
            // 중지 시키고
            // 다시 시작
        }

        /// <summary>
        /// - 검사를 실행하기 위해서 생성할 쓰레드 관리자(WorkManager)를 생성합니다.
        /// - WORK_TYPE에는 다음과 같이 실행 우선 순위가 있지만 스탭을 생략하고 생성할 수 있습니다.
        ///   1) SNAP
        ///   2) ALIGNMENT
        ///   3) INSPECTION
        ///   4) DEFECTPROCESS_CHIP
        ///   5) DEFECTPROCESS_WAFER
        /// - DefectProcess_WAFER는 이전 상태가 모두 완료된다음에 실행되므로 isFull 값을 true변경해야합니다.
        /// - 다른 검사 시에도 이전 검사가 모두 완료된 후에 진행하려면 isFull 값을 true하세요.
        /// ※ 중간에 생략된 State는 생성하지 않고, 다음 쓰레드 관리자를 실행시킵니다.
        /// </summary>
        /// <param name="works">생성할 WorkManager Type</param>
        public void CreateWorkManager(WORK_TYPE workType, int workerNumber = 1, bool isFull = false)
        {
            if(this.workManagers.Count ==  0)
                this.workManagers.Add(new WorkManager(workType, WORK_TYPE.NONE, workerNumber, isFull));
            else
                this.workManagers.Add(new WorkManager(workType, this.workManagers[this.workManagers.Count -1 ].WorkType, workerNumber, isFull));
        }


        /// <summary>
        /// - WorkManager들을 중단하고 WorkManager와 데이터를 모두 제거합니다.
        /// - 프로그램 종료하는 경우 외에는 사용 자제
        /// </summary>
        public void Clear()
        {
            foreach(WorkManager wm in this.workManagers)
            {
                wm.Exit();
            }

            this.workManagers.Clear();
        }

        /// <summary>
        /// - 현재 WorkManager는 유지 한채로 데이터만 초기화 시킵니다.
        /// - Workplace State를 초기화 시킵니다.
        /// </summary>
        public void Reset()
        {
            // 쓰레드로 동작하는 객체는 Stop시켜야하고
            // 그냥 객체는 Reset()으로 초기화
            // Workplace State 초기화
            //foreach(WorkplaceBundle wb in this.bundleList)
            //{
            //    wb.Reset();
            //}


            //this.workplaceBundle.Reset(); // ?? 없애도  되나...

            foreach(WorkManager wm in this.workManagers)
            {
                wm.Stop();
            }
        }

        // Edge쪽 이동예정
        public bool CreateWorkplaceBundle_RegionVertical(int imageWidth, int imageHeight, int imageByteCnt, int partitionNumber, IntPtr imageBufferR_GRAY, IntPtr imageBufferG, IntPtr imageBufferB)
        {
            //bundleList.Clear();
            int regionWidth = imageWidth;
            int regionHeight = (int)Math.Floor((double)imageHeight / partitionNumber);
            int regionLastHeight = imageHeight - regionHeight * (partitionNumber - 1); // 숫자가 딱 안나눠떨어지는 경우가있으므로


            WorkplaceBundle bundle = new WorkplaceBundle();
            int index = 0;
            for(int i = 0; i < partitionNumber - 1; i++)
            {
                bundle.Add(new Workplace(0, i, 0, regionHeight * i, regionWidth, regionHeight, index++));
            }

            // Last Region
            bundle.Add(new Workplace(0, partitionNumber - 1, 0, regionHeight * (partitionNumber - 1), regionWidth, regionLastHeight, index));


            //Set SharedBuffer;
            bundle.SetSharedBuffer(imageBufferR_GRAY, imageWidth, imageHeight, imageByteCnt, imageBufferG, imageBufferB);

            // Add Bundle
            //bundleList.Add(bundle);


            return true;            
        }

        public bool CreateWorkplaceBundle_RegionHorizontal(int imageWidth, int imageHeight, int imageByteCnt, int partitionNumber, IntPtr imageBufferR_GRAY, IntPtr imageBufferG, IntPtr imageBufferB)
        {
            //bundleList.Clear();
            int regionWidth = (int)Math.Floor((double)imageWidth / partitionNumber);
            int regionLastWidth = imageWidth - regionWidth * (partitionNumber - 1); // 숫자가 딱 안나눠떨어지는 경우가있으므로
            int regionHeight = imageHeight;
            
            WorkplaceBundle bundle = new WorkplaceBundle();
            int index = 0;
            for (int i = 0; i < partitionNumber - 1; i++)
            {
                bundle.Add(new Workplace(i, 0, regionWidth * i, 0, regionWidth, regionHeight, index++));
            }

            // Last Region
            bundle.Add(new Workplace(partitionNumber - 1, 0, regionWidth * (partitionNumber - 1), 0, regionLastWidth, regionHeight, index));

            //Set SharedBuffer;
            bundle.SetSharedBuffer(imageBufferR_GRAY, imageWidth, imageHeight, imageByteCnt, imageBufferG, imageBufferB);

            // Add Bundle
            //bundleList.Add(bundle);

            return true;
        }
    }
}
