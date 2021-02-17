using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    public delegate void WorkManagerAllWorkDoneEvent();

    public abstract class WorkFactory : IWorkStartable
    {
        #region [Members]
        private List<WorkManager> workManagers;
        private readonly RemoteProcess remote = null;
        private readonly REMOTE_MODE remoteMode;

        private WorkplaceBundle workplaceBundle;
        private WorkBundle workBundle;

        private RecipeBase recipe;
        #endregion


        #region [Event]
        public event WorkManagerAllWorkDoneEvent AllWorkDone;

        public event EventHandler<PositionDoneEventArgs> PositionDone;

        public event EventHandler<InspectionStartArgs> InspectionStart;

        public event EventHandler<InspectionDoneEventArgs> InspectionDone;

        public event EventHandler<IntegratedProcessDefectDoneEventArgs> IntegratedProcessDefectDone;

        public event EventHandler<ProcessDefectWaferStartEventArgs> ProcessDefectWaferStart;

        public event EventHandler<ProcessDefectDoneEventArgs> ProcessDefectDone;

        public event EventHandler<ProcessMeasurementDoneEventArgs> ProcessMeasurementDone;

        public event EventHandler<WorkplaceStateChangedEventArgs> WorkplaceStateChanged;
        #endregion

        //List<WorkplaceBundle> bundleList = new List<WorkplaceBundle>();
        //WorkplaceBundle workplaceBundle;

        //private WorkplaceBundle workplaces; // WorkplaceBundle은 유지하고 WorkBundle만 새로 생성하게...

        //List<SharedBufferInfo> bufferInfoList;
        public bool IsStop
        {
            get
            {                 
               foreach(WorkManager wm in this.workManagers)
               {
                    if (wm.IsStop == false)
                        return false;
               }
               return true;
            }
        }

        public WorkFactory(REMOTE_MODE mode = REMOTE_MODE.None)
        {
            workManagers = new List<WorkManager>();
            this.remote = new RemoteProcess(mode);
            this.remoteMode = mode;
                
            Initialize();

            WorkEventManager.RequestStop += OnRequestStop_Callback;

            WorkEventManager.PositionDone += PositionDone_Callback;

            WorkEventManager.InspectionStart += InspectionStart_Callback;
            WorkEventManager.InspectionDone += InspectionDone_Callback;

            WorkEventManager.ProcessDefectDone += ProcessDefectDone_Callback;

            WorkEventManager.ProcessDefectWaferStart += ProcessDefectWaferStart_Callback;
            WorkEventManager.IntegratedProcessDefectDone += IntegratedProcessDefectDone_Callback;

            WorkEventManager.ProcessMeasurementDone += ProcessMeasurementDone_Callback;

            WorkEventManager.WorkplaceStateChanged += WorkplaceStateChanged_Callback;
        }

        #region [WorkEventManager Callback Link]
        private void PositionDone_Callback(object obj, PositionDoneEventArgs args)
        {
            this.PositionDone?.Invoke(obj, args);
        }

        private void InspectionStart_Callback(object obj, InspectionStartArgs args)
        {
            this.InspectionStart?.Invoke(obj, args);
        }

        private void InspectionDone_Callback(object obj, InspectionDoneEventArgs args)
        {
            this.InspectionDone?.Invoke(obj, args);
        }

        private void ProcessDefectDone_Callback(object obj, ProcessDefectDoneEventArgs args)
        {
            this.ProcessDefectDone?.Invoke(obj, args);
        }

        private void ProcessDefectWaferStart_Callback(object obj, ProcessDefectWaferStartEventArgs args)
        {
            this.ProcessDefectWaferStart?.Invoke(obj, args);
        }

        private void IntegratedProcessDefectDone_Callback(object obj, IntegratedProcessDefectDoneEventArgs args)
        {
            this.IntegratedProcessDefectDone?.Invoke(obj, args);
        }

        private void ProcessMeasurementDone_Callback(object obj, ProcessMeasurementDoneEventArgs args)
        {
            this.ProcessMeasurementDone?.Invoke(obj, args);
        }

        private void WorkplaceStateChanged_Callback(object obj, WorkplaceStateChangedEventArgs args)
        {
            this.WorkplaceStateChanged?.Invoke(obj, args);
        }

        #endregion

        public void SetRecipe(RecipeBase recipeBase)
        {
            this.recipe = recipeBase;
        }

        public void OnRequestStop_Callback(object obj, RequestStopEventArgs args)
        {
            Stop();
        }

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

        #region [Control]
        /// <summary>
        /// WorkplaceBundle 생성
        /// WorkBundle 생성
        /// 검사 시작
        /// </summary>
        /// <param name="type">검사 시작 전 Workplace 상태(Default : NONE)</param>
        public void Start(WORK_TYPE type = WORK_TYPE.NONE)
        {
            WaitStop(); /// 타임 아웃 걸어야함

            GC.Collect(2, GCCollectionMode.Optimized);

            WorkplaceBundle workplaces = CreateWorkplaceBundle();
            if(workplaces == null)
            {
                MessageBox.Show("맵 정보가 없습니다.");
                return;
            }
            
            this.workplaceBundle = workplaces;

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

            WorkEventManager.OnInspectionStart(this, new InspectionStartArgs());

            foreach (WorkManager wm in this.workManagers)
            {
                wm.SetWorkplaceBundle(workplaces);
                wm.SetWorkBundle(works);
                wm.Start();
            }
        }

        private bool WaitStop() // 구현해야함.
        {
            Stop();

            Task.Delay(100);

            return true;
        }

        public void Start()
        {
            if(this.remoteMode == REMOTE_MODE.Slave)
            {
                WaitStop(); /// 타임 아웃 걸어야함

                GC.Collect(2, GCCollectionMode.Optimized);

                WorkplaceBundle workplaces = this.workplaceBundle;
                if (workplaces == null)
                {
                    MessageBox.Show("맵 정보가 없습니다.");
                    return;
                }

                WorkBundle works = CreateWorkBundle();
                works.SetWorkplacBundle(workplaces);


                this.workplaceBundle = workplaces;

                if (Ready(workplaces, works) == false)
                {
                    MessageBox.Show("검사 정보 생성에 실패하였습니다");
                    return;
                }

                // Workplace State 초기화
                workplaces.SetWorkState(WORK_TYPE.NONE);

#if DEBUG
                Debug.WriteLine("[Start]");
                DebugOutput.PrintWorkplaceBundle(workplaces);
#endif

                WorkEventManager.OnInspectionStart(this, new InspectionStartArgs());

                foreach (WorkManager wm in this.workManagers)
                {
                    wm.SetWorkplaceBundle(workplaces);
                    wm.SetWorkBundle(works);
                    wm.Start();
                }
            }
            else
            {
                WaitStop(); /// 타임 아웃 걸어야함

                GC.Collect(2, GCCollectionMode.Optimized);

                WorkplaceBundle workplaces = CreateWorkplaceBundle();
                if (workplaces == null)
                {
                    MessageBox.Show("맵 정보가 없습니다.");
                    return;
                }

                WorkBundle works = CreateWorkBundle();
                works.SetWorkplacBundle(workplaces);


                this.workplaceBundle = workplaces;

                if (Ready(workplaces, works) == false)
                {
                    MessageBox.Show("검사 정보 생성에 실패하였습니다");
                    return;
                }

                // Workplace State 초기화
                workplaces.SetWorkState(WORK_TYPE.NONE);

#if DEBUG
                Debug.WriteLine("[Start]");
                DebugOutput.PrintWorkplaceBundle(workplaces);
#endif
                WorkEventManager.OnInspectionStart(this, new InspectionStartArgs());

                foreach (WorkManager wm in this.workManagers)
                {
                    wm.SetWorkplaceBundle(workplaces);
                    wm.SetWorkBundle(works);
                    wm.Start();
                }
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

        public void Exit()
        {
            foreach (WorkManager wm in this.workManagers)
                wm.Exit();
            this.workManagers.Clear();
        }

        public void WaitAllWorkDone()
        {
            bool isDone = false;
            while (!isDone)
            {
                Task.Delay(1000);
                isDone = true;
                foreach (Workplace wp in workplaceBundle)
                {
                    if (wp.WorkState != workManagers[workManagers.Count - 1].WorkType)
                    {
                        isDone = false;
                        break;
                    }
                }
            }

            if (this.AllWorkDone != null)
                AllWorkDone();
        }

        public bool CheckAllWorkDone(int timeoutMinutes = 10)
        {
            bool isDone = true;
            foreach (Workplace wp in workplaceBundle)
            {
                if (wp.WorkState != workManagers[workManagers.Count - 1].WorkType)
                {
                    isDone = false;
                    break;
                }
            }

            return isDone;
        }

        #endregion

        #region [Remote Process]

        private void PipeCommMessageReceived_Callback(PipeProtocol protocol)
        {
            //MessageBox.Show(protocol.msgType.ToString());
            //MessageBox.Show(protocol.msg);
            //MessageBox.Show(protocol.data?.ToString());
            //MessageBox.Show(protocol.dataType?.ToString());

            switch (protocol.msgType)
            {
                case PIPE_MESSAGE_TYPE.Message:
                    RemoteMessage_MessageHandler(protocol);
                    break;
                case PIPE_MESSAGE_TYPE.Command:
                    RemoteMessage_CommandHandler(protocol);
                    break;
                case PIPE_MESSAGE_TYPE.Data:
                    break;
                case PIPE_MESSAGE_TYPE.Event:
                    
                    break;
            }
        }


        public void StartRemoteProcess()
        {
            if (this.remoteMode != REMOTE_MODE.Master) return;

            this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Command, REMOTE_MASTER_MESSAGES.StartRemote));
        }

        private void RemoteMessage_MessageHandler(PipeProtocol protocol)
        {
            if(this.remoteMode == REMOTE_MODE.Master)
            {

            }
            else
            {

            }
        }

        private void RemoteMessage_CommandHandler(PipeProtocol protocol)
        {
            if (this.remoteMode == REMOTE_MODE.Master)  // Slave에서 보낸 메시지를 처리
            {
                REMOTE_SLAVE_MESSAGES msg = (REMOTE_SLAVE_MESSAGES)Enum.Parse(typeof(REMOTE_SLAVE_MESSAGES), protocol.msg, true);
                switch (msg)
                {
                    case REMOTE_SLAVE_MESSAGES.StartRemoteAck:
                        this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Command, REMOTE_MASTER_MESSAGES.StartCreateCloneFactory));
                        break;
                        
                        //
                        // StartCreate CloneFactory
                        //
                    case REMOTE_SLAVE_MESSAGES.StartCreateCloneFactoryAck:

                        // Send WorkManager
                        this.remote.Send(new PipeProtocol(
                            PIPE_MESSAGE_TYPE.Data, 
                            REMOTE_DATA_TYPE.WorkManagerList, 
                            typeof(List<WorkManager>).ToString(), 
                            this.workManagers
                            ));
                        break;
                }
            }
            else  // 마스터에서 보낸 메시지를 처리
            {
                REMOTE_MASTER_MESSAGES msg = (REMOTE_MASTER_MESSAGES)Enum.Parse(typeof(REMOTE_MASTER_MESSAGES), protocol.msg, true);
                switch (msg)
                {
                    case REMOTE_MASTER_MESSAGES.StartRemote:
                        this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Command, REMOTE_SLAVE_MESSAGES.StartRemoteAck));
                        break;
                    case REMOTE_MASTER_MESSAGES.StartCreateCloneFactory:
                        this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Command, REMOTE_SLAVE_MESSAGES.StartCreateCloneFactoryAck));
                        break;
                    default:
                        break;
                }
            }
        }

        private void RemoteMessage_EventHandler(PipeProtocol protocol)
        {
            if (this.remoteMode == REMOTE_MODE.Master)
            {

            }
            else
            {

            }
        }

        private void RemoteMessage_DataHandler(PipeProtocol protocol)
        {
            if (this.remoteMode == REMOTE_MODE.Master)
            {

            }
            else
            {

            }
        }




        public void RemoteStart()
        {
            remote.StartProcess();
            remote.MessageReceived += PipeCommMessageReceived_Callback;
            //WorkplaceBundle wb = this.CreateWorkplaceBundle();
            //if (wb == null) return;

            //remote.Send<WorkplaceBundle>(wb);
        }

        public void WriteTest()
        {
            remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Message, "Start"));

            WorkplaceBundle wb = new WorkplaceBundle();
            wb.Add(new Workplace(0, 0, 0, 0, 0, 0, 0));
            wb.Add(new Workplace(0, 1, 0, 0, 0, 0, 0));
            wb.Add(new Workplace(0, 2, 0, 0, 0, 0, 0));
            wb.Add(new Workplace(0, 3, 0, 0, 0, 0, 0));

            remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Data, "WorkplaceBundle", typeof(WorkplaceBundle).ToString(), wb));

            remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Command, "Command!!!!"));


            remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Event, "Event!!", typeof(Workplace).ToString(), new Workplace(10, 10, 10, 10, 10, 10, 10)));

            //remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Message, typeof(string).ToString(), "AAAAAAAAAAA"));

            //InitializeRemoteWorkFactory();
        }

        public void InitializeRemoteWorkFactory()
        {
            const string InitializeStartMsg = "InitializeStart";
            const string InitializeDoneMsg = "InitializeDone";

            if (remoteMode == REMOTE_MODE.Master)
            {
                //remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Message, InitializeStartMsg));
                //remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Data, this.recipe.RecipePath));
                //remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Data, this.workManagers));
                //remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Data, this.CreateWorkplaceBundle()));
                //remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Data, this.CreateWorkBundle().CloneForRemote()));

                PipeProtocol protocol = new PipeProtocol();
                remote.Read(protocol);


                switch (protocol.msgType)
                {
                    case PIPE_MESSAGE_TYPE.Message:
                        MessageBox.Show((string)protocol.data);
                        break;
                    case PIPE_MESSAGE_TYPE.Command:
                        break;
                    case PIPE_MESSAGE_TYPE.Data:
                        break;
                    case PIPE_MESSAGE_TYPE.Event:
                        break;
                }
                //string message = "";
                //remote.Send<string>(InitializeStartMsg);

                //remote.Send<string>(this.recipe.RecipePath);
                //remote.Send<List<WorkManager>>(this.workManagers);
                //remote.Send<WorkplaceBundle>(this.CreateWorkplaceBundle());
                //remote.Send<WorkBundle>(this.CreateWorkBundle().CloneForRemote());

                //if (remote.Read<string>() == InitializeDoneMsg)
                //{
                //    Debug.WriteLine(message);
                //    return;
                //}
                //Debug.WriteLine(message);
            }
            else // Slave
            {
                PipeProtocol protocol = new PipeProtocol();

                remote.Read(protocol);


                //string message = remote.Read<string>();
                //if (message != InitializeStartMsg)
                //{
                //    Debug.WriteLine(message);
                //    return;
                //}
                //Debug.WriteLine(message);

                //string recipePath = remote.Read<string>();
                //Debug.WriteLine(recipePath);

                //this.recipe = new CloneableRecipe();
                //this.recipe.RecipePath = recipePath;
                //this.recipe.Read(recipePath);


                //List<WorkManager> workManagers = remote.Read<List<WorkManager>>();
                //foreach (WorkManager wm in workManagers)
                //{
                //    this.workManagers.Add(wm.Clone());
                //}

                //if (this.workManagers == null)
                //{
                //    Debug.WriteLine("workManagers == null");
                //}

                //this.workplaceBundle = remote.Read<WorkplaceBundle>();
                //if (this.workplaceBundle == null)
                //{
                //    Debug.WriteLine("WorkplaceBundle == null");
                //}

                //WorkBundle wb = remote.Read<WorkBundle>();
                //if (wb == null)
                //{
                //    Debug.WriteLine("workBundle == null");
                //}
                //this.workBundle = new WorkBundle();
                //int indexParam = 0;
                //foreach (CloneableWorkBase clone in wb)
                //{
                //    WorkBase work = (WorkBase)Tools.CreateInstance(Type.GetType(clone.InspectionName));
                //    this.workBundle.Add(work);
                //    if (indexParam < this.recipe.ParameterItemList.Count)
                //    {
                //        if (this.recipe.ParameterItemList[indexParam].InspectionType.ToString() == clone.InspectionName)
                //        {
                //            work.SetParameter(this.recipe.ParameterItemList[indexParam]);
                //            indexParam++;
                //        }
                //    }
                //}


                //Debug.WriteLine(this.workplaceBundle.Count.ToString());

                //remote.Send<string>(InitializeDoneMsg);
                //string message = remote.Read<string>();
                //if (message != InitializeStartMsg)
                //{
                //    Debug.WriteLine(message);
                //    return;
                //}
                //Debug.WriteLine(message);

                //string recipePath = remote.Read<string>();
                //Debug.WriteLine(recipePath);

                //this.recipe = new CloneableRecipe();
                //this.recipe.RecipePath = recipePath;
                //this.recipe.Read(recipePath);


                //List<WorkManager> workManagers = remote.Read<List<WorkManager>>();
                //foreach(WorkManager wm in workManagers)
                //{
                //    this.workManagers.Add(wm.Clone());
                //}

                //if (this.workManagers == null)
                //{
                //    Debug.WriteLine("workManagers == null");
                //}

                //this.workplaceBundle = remote.Read<WorkplaceBundle>();
                //if(this.workplaceBundle == null)
                //{
                //    Debug.WriteLine("WorkplaceBundle == null");
                //}

                //WorkBundle wb= remote.Read<WorkBundle>();
                //if (wb == null)
                //{
                //    Debug.WriteLine("workBundle == null");
                //}
                //this.workBundle = new WorkBundle();
                //int indexParam = 0;
                //foreach (CloneableWorkBase clone in wb)
                //{
                //    WorkBase work = (WorkBase)Tools.CreateInstance(Type.GetType(clone.InspectionName));
                //    this.workBundle.Add(work);
                //    if( indexParam < this.recipe.ParameterItemList.Count)
                //    {
                //        if (this.recipe.ParameterItemList[indexParam].InspectionType.ToString() == clone.InspectionName)
                //        {
                //            work.SetParameter(this.recipe.ParameterItemList[indexParam]);
                //            indexParam++;
                //        }
                //    }  
                //}


                //Debug.WriteLine(this.workplaceBundle.Count.ToString());

                //remote.Send<string>(InitializeDoneMsg);
            }
        }


        public void ExitRemoteProcess()
        {
            remote.ExitProcess();
        }

        ~WorkFactory()
        {
            ExitRemoteProcess();
        }
        #endregion

    }
}
