using RootTools;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    public delegate void WorkManagerAllWorkDoneEvent();

    public abstract class WorkFactory : IWorkStartable
    {
        #region [Members]
        private List<WorkManager> workManagers = new List<WorkManager>();
        private readonly RemoteProcess remote = null;
        private readonly REMOTE_MODE remoteMode;

        private WorkplaceBundle workplaceBundle;
        private WorkBundle workBundle;

        private RecipeBase recipe;

        public List<SharedBufferInfo> sharedBufferInfoList = new List<SharedBufferInfo>();

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
            Initialize();

            if (mode != REMOTE_MODE.None)
            {
                this.remote = new RemoteProcess(mode);
                this.remoteMode = mode;
                this.remote.MessageReceived += PipeCommMessageReceived_Callback;
                this.remote.Connected += Connected_Callback;
                this.remote.Disconnected += Disconnected_Callback;
            }

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
            if(this.remoteMode == REMOTE_MODE.Slave && this.remote.IsConnected == true)
            {
                this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Data, REMOTE_MESSAGE_DATA.InspectionDone, typeof(InspectionDoneEventArgs).ToString(), (object)args));
            }

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
            if (this.remoteMode == REMOTE_MODE.Slave && this.remote.IsConnected == true)
            {
                this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Data, REMOTE_MESSAGE_DATA.IntergratedProcessDefectDone, typeof(IntegratedProcessDefectDoneEventArgs).ToString(), (object)args));
            }
            this.IntegratedProcessDefectDone?.Invoke(obj, args);
        }

        private void ProcessMeasurementDone_Callback(object obj, ProcessMeasurementDoneEventArgs args)
        {
            this.ProcessMeasurementDone?.Invoke(obj, args);
        }

        private void WorkplaceStateChanged_Callback(object obj, WorkplaceStateChangedEventArgs args)
        {
            if (this.remoteMode == REMOTE_MODE.Slave && this.remote.IsConnected == true)
            {
                this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Data, REMOTE_MESSAGE_DATA.WorkplaceStateChanged, typeof(WorkplaceStateChangedEventArgs).ToString(), (object)args));

                // Master에 WorkplaceBundle이 없어서 Slave에서 보내야함
                if (this.workplaceBundle.CheckStateAll(WORK_TYPE.DEFECTPROCESS_ALL) == true)
                {
                    this.remote.EventChange(PipeCommMessageReceived_Callback);
                    this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Process, REMOTE_PROCESS_MESSAGE_TYPE.EndWork));
                }
            }


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
            works.SetRecipe(this.recipe);

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

            WorkEventManager.OnInspectionStart(this, new InspectionStartArgs(this.workplaceBundle));

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
                WorkEventManager.OnInspectionStart(this, new InspectionStartArgs(this.workplaceBundle));

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

            ExitRemoteProcess();
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

        public bool IsConnected
        {
            get => this.remote.IsConnected;
        }


        private void Connected_Callback()
        {
            if(this.remoteMode == REMOTE_MODE.Master)
                StartSync();
        }

        private void Disconnected_Callback()
        {

        }




        #region [ProcessSync]
        public void StartSync()
        {
            if(this.remoteMode == REMOTE_MODE.Master)
            {
                this.remote.EventChange(ProcessSync);
                this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Process, REMOTE_PROCESS_MESSAGE_TYPE.StartSync));
            }
            else
            {
                MessageBox.Show("Master WorkFacotry에서만 StartSync 메서드를 실행할 수 있습니다");
            }
        }

        private void ProcessSync(PipeProtocol protocol)
        {
            try
            {
                // Slave로부터 응답 메시지 전달 받고 시작
                if (this.remoteMode == REMOTE_MODE.Master)
                {
                    // Send Memory Data
                    foreach (SharedBufferInfo info in this.sharedBufferInfoList)
                    {
                        this.remote.Send(new PipeProtocol(
                        PIPE_MESSAGE_TYPE.Data,
                        REMOTE_MESSAGE_DATA.MemoryID,
                        typeof(MemoryID).ToString(),
                        this.sharedBufferInfoList[0].MemoryID
                        ));
                    }

                    // Send WorkManager
                    this.remote.Send(new PipeProtocol(
                        PIPE_MESSAGE_TYPE.Data,
                        REMOTE_MESSAGE_DATA.WorkManagerList,
                        typeof(List<WorkManager>).ToString(),
                        this.workManagers
                        ));

                    this.remote.EventChange(PipeCommMessageReceived_Callback);
                    this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Process, REMOTE_PROCESS_MESSAGE_TYPE.EndSync));

                }
                else
                {
                    if (protocol.msg == REMOTE_PROCESS_MESSAGE_TYPE.EndSync.ToString())
                    {
                        this.remote.EventChange(PipeCommMessageReceived_Callback);
                        this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Process, REMOTE_PROCESS_MESSAGE_TYPE.EndSync));
                        return;
                    }

                    REMOTE_MESSAGE_DATA dataMsg = (REMOTE_MESSAGE_DATA)Enum.Parse(typeof(REMOTE_MESSAGE_DATA), protocol.msg);
                    switch (dataMsg)
                    {
                        case REMOTE_MESSAGE_DATA.MemoryID:
                            if (typeof(MemoryID).ToString() == protocol.dataType)
                            {
                                WorkEventManager.OnReceivedMemoryID(this, new MemoryIDArgs((MemoryID)protocol.data));

                                MemoryID memoryID = (MemoryID)protocol.data;

                                MemoryPool pool = new MemoryPool(memoryID.Pool);
                                ImageData imageData = new ImageData(pool.GetMemory(memoryID.Group, memoryID.Data));
                                this.sharedBufferInfoList.Add(new SharedBufferInfo(imageData.GetPtr(0), imageData.p_Size.X, imageData.p_Size.Y, imageData.GetBytePerPixel(), imageData.GetPtr(1), imageData.GetPtr(2)));
                            }
                            else
                            {
                                Logger.AddMsg(LOG_MESSAGE_TYPE.Network, "데이터 타입이 다릅니다 : " + protocol.msg);
                            }
                            break;
                        case REMOTE_MESSAGE_DATA.WorkManagerList:
                            if (protocol.msg == REMOTE_MESSAGE_DATA.WorkManagerList.ToString())
                            {                // Receive Workplace
                                List<WorkManager> wms = (List<WorkManager>)protocol.data;
                                this.workManagers.Clear();
                                foreach (WorkManager wm in wms)
                                {
                                    this.workManagers.Add(wm.Clone());
                                }
                            }
                            else
                            {
                                Logger.AddMsg(LOG_MESSAGE_TYPE.Network, "데이터 타입이 다릅니다 : " + protocol.msg);
                            }
                            break;
                        default:
                            {
                                this.remote.EventChange(PipeCommMessageReceived_Callback);
                                Logger.AddMsg(LOG_MESSAGE_TYPE.Network, "정의되지 않은 메시지 입니다. " + protocol.msg);
                            }
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.AddMsg(LOG_MESSAGE_TYPE.Network, ex);
                this.remote.EventChange(PipeCommMessageReceived_Callback);
            }
            finally
            {

            }
        }
        #endregion

        #region [ProcessReadyWork]
        public void ReadyWork()
        {
            if (this.remoteMode == REMOTE_MODE.Master)
            {
                this.remote.EventChange(ProcessReadyWork);
                this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Process, REMOTE_PROCESS_MESSAGE_TYPE.StartReadyWork));
            }
            else
            {
                MessageBox.Show("Master WorkFacotry에서만 StartWork 메서드를 실행할 수 있습니다");
            }
        }

        public void ProcessReadyWork(PipeProtocol protocol)
        {
            try
            { 
                if (this.remoteMode == REMOTE_MODE.Master)
                {
                    this.remote.Send(new PipeProtocol(
                                    PIPE_MESSAGE_TYPE.Data,
                                    REMOTE_MESSAGE_DATA.RecipeName,
                                    typeof(string).ToString(),
                                    this.recipe.RecipePath
                                    ));

                    // WorkplaceBundle
                    this.remote.Send(new PipeProtocol(
                        PIPE_MESSAGE_TYPE.Data,
                        REMOTE_MESSAGE_DATA.WorkplaceBundle,
                        typeof(WorkplaceBundle).ToString(),
                        this.CreateWorkplaceBundle()
                        ));

                    // WorkBundle
                    this.remote.Send(new PipeProtocol(
                        PIPE_MESSAGE_TYPE.Data,
                        REMOTE_MESSAGE_DATA.WorkBundle,
                        typeof(WorkBundle).ToString(),
                        this.CreateWorkBundle().CloneForRemote()
                        ));

                    this.remote.EventChange(PipeCommMessageReceived_Callback);
                    this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Process, REMOTE_PROCESS_MESSAGE_TYPE.EndReadyWork));
                }
                else
                {
                    if (protocol.msg == REMOTE_PROCESS_MESSAGE_TYPE.EndReadyWork.ToString())
                    {
                        this.remote.EventChange(PipeCommMessageReceived_Callback);
                        this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Process, REMOTE_PROCESS_MESSAGE_TYPE.EndReadyWork));
                        return;
                    }

                    REMOTE_MESSAGE_DATA dataMsg = (REMOTE_MESSAGE_DATA)Enum.Parse(typeof(REMOTE_MESSAGE_DATA), protocol.msg);
                    switch (dataMsg)
                    {
                        case REMOTE_MESSAGE_DATA.RecipeName:
                            string recipePath = (string)protocol.data;
                            this.recipe = new CloneableRecipe();
                            this.recipe.RecipePath = recipePath;
                            this.recipe.Read(recipePath);
                            break;
                        case REMOTE_MESSAGE_DATA.WorkplaceBundle:
                            this.workplaceBundle = (WorkplaceBundle)protocol.data;
                            break;
                        case REMOTE_MESSAGE_DATA.WorkBundle:
                            WorkBundle wb = (WorkBundle)protocol.data;
                            this.workBundle = new WorkBundle();
                            int indexParam = 0;
                            foreach (CloneableWorkBase clone in wb)
                            {
                                WorkBase work = (WorkBase)Tools.CreateInstance(Type.GetType(clone.InspectionName));
                                this.workBundle.Add(work);
                                if (indexParam < this.recipe.ParameterItemList.Count)
                                {
                                    if (this.recipe.ParameterItemList[indexParam].InspectionType.ToString() == clone.InspectionName)
                                    {
                                        work.SetParameter(this.recipe.ParameterItemList[indexParam]);
                                        indexParam++;
                                    }
                                }
                            }
                            break;
                        default:
                            {
                                this.remote.EventChange(PipeCommMessageReceived_Callback);
                                Logger.AddMsg(LOG_MESSAGE_TYPE.Network, "정의되지 않은 메시지 입니다. " + protocol.msg);
                            }
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.AddMsg(LOG_MESSAGE_TYPE.Network, ex);
                this.remote.EventChange(PipeCommMessageReceived_Callback);
            }
            finally
            {
               
            }
        }
        #endregion

        #region [ProcessStartWork]
        public void StartWork()
        {
            if (this.remoteMode == REMOTE_MODE.Master)
            {
                this.remote.EventChange(ProcessWork);
                this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Process, REMOTE_PROCESS_MESSAGE_TYPE.StartWork));
            }
            else
            {
                MessageBox.Show("Master WorkFacotry에서만 StartWork 메서드를 실행할 수 있습니다");
            }
        }

        public void ProcessWork(PipeProtocol protocol)
        {
            try 
            { 
                if (this.remoteMode == REMOTE_MODE.Master)
                {
                    if(protocol.msg == REMOTE_PROCESS_MESSAGE_TYPE.StartWork.ToString())
                    {
                        this.remote.ProcessState = REMOTE_PROCESS_STATE.Working;
                        return;
                    }
                    
                    if (protocol.msg == REMOTE_PROCESS_MESSAGE_TYPE.EndWork.ToString())
                    {
                        isWorking = false;
                        this.remote.EventChange(PipeCommMessageReceived_Callback);
                        this.remote.ProcessState = REMOTE_PROCESS_STATE.WorkDone;
                        return;
                    }


                    REMOTE_MESSAGE_DATA dataMsg = (REMOTE_MESSAGE_DATA)Enum.Parse(typeof(REMOTE_MESSAGE_DATA), protocol.msg);
                    switch (dataMsg)
                    {
                        case REMOTE_MESSAGE_DATA.InspectionDone:
                            {
                                InspectionDoneEventArgs args = (InspectionDoneEventArgs)protocol.data;
                                this.InspectionDone?.Invoke(args.workplace, args);
                            }
                            break;
                        case REMOTE_MESSAGE_DATA.WorkplaceStateChanged:
                            {
                                WorkplaceStateChangedEventArgs args = (WorkplaceStateChangedEventArgs)protocol.data;
                                this.WorkplaceStateChanged?.Invoke(args.workplace, args);
                            }
                            break;
                        case REMOTE_MESSAGE_DATA.IntergratedProcessDefectDone:
                            {
                                IntegratedProcessDefectDoneEventArgs args = (IntegratedProcessDefectDoneEventArgs)protocol.data;
                                this.IntegratedProcessDefectDone?.Invoke(args, args);
                            }
                            break;
                        default:
                            {
                                this.remote.EventChange(PipeCommMessageReceived_Callback);
                                Logger.AddMsg(LOG_MESSAGE_TYPE.Network, "정의되지 않은 메시지 입니다. " + protocol.msg);
                            }
                            break;
                    }
                }
                else
                {
                    //if (protocol.msg == REMOTE_PROCESS_MESSAGE_TYPE.EndWork.ToString())
                    //{
                    //    this.remote.EventChange(PipeCommMessageReceived_Callback);
                    //    this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Process, REMOTE_PROCESS_MESSAGE_TYPE.EndWork));
                    //    return;
                    //}

                    REMOTE_MESSAGE_DATA dataMsg = (REMOTE_MESSAGE_DATA)Enum.Parse(typeof(REMOTE_MESSAGE_DATA), protocol.msg);
                    switch (dataMsg)
                    {
                        case REMOTE_MESSAGE_DATA.InspectionDone:
                            break;
                        case REMOTE_MESSAGE_DATA.WorkplaceStateChanged:
                            break;
                        default:
                            {
                                this.remote.EventChange(PipeCommMessageReceived_Callback);
                                Logger.AddMsg(LOG_MESSAGE_TYPE.Network, "정의되지 않은 메시지 입니다. " + protocol.msg);
                            }
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.AddMsg(LOG_MESSAGE_TYPE.Network, ex);
                this.remote.EventChange(PipeCommMessageReceived_Callback);
            }
            finally
            {
               
            }
}
        #endregion

        private void PipeCommMessageReceived_Callback(PipeProtocol protocol)
        {
            if(this.remoteMode == REMOTE_MODE.Slave)
            {
                REMOTE_PROCESS_MESSAGE_TYPE proc = (REMOTE_PROCESS_MESSAGE_TYPE)Enum.Parse(typeof(REMOTE_PROCESS_MESSAGE_TYPE), protocol.msg, true);
                switch (proc)
                {
                    case REMOTE_PROCESS_MESSAGE_TYPE.StartSync:
                        this.remote.EventChange(ProcessSync);
                        this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Process, REMOTE_PROCESS_MESSAGE_TYPE.StartSync));
                        break;
                    case REMOTE_PROCESS_MESSAGE_TYPE.StartReadyWork:
                        this.remote.EventChange(ProcessReadyWork);
                        this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Process, REMOTE_PROCESS_MESSAGE_TYPE.StartReadyWork));
                        break;
                    case REMOTE_PROCESS_MESSAGE_TYPE.StartWork:
                        this.remote.EventChange(ProcessWork);
                        this.remote.Send(new PipeProtocol(PIPE_MESSAGE_TYPE.Process, REMOTE_PROCESS_MESSAGE_TYPE.StartWork));

                        this.RemoteSlaveStartWork();
                        break;
                }
            }
            else // Master
            {
                REMOTE_PROCESS_MESSAGE_TYPE proc = (REMOTE_PROCESS_MESSAGE_TYPE)Enum.Parse(typeof(REMOTE_PROCESS_MESSAGE_TYPE), protocol.msg, true);
                switch (proc)
                {
                    case REMOTE_PROCESS_MESSAGE_TYPE.EndSync:
                        this.remote.ProcessState = REMOTE_PROCESS_STATE.Sync;
                        break;
                    case REMOTE_PROCESS_MESSAGE_TYPE.EndReadyWork:
                        this.remote.ProcessState = REMOTE_PROCESS_STATE.ReadyWork;
                        break;
                    case REMOTE_PROCESS_MESSAGE_TYPE.EndWork:
                        this.remote.ProcessState = REMOTE_PROCESS_STATE.WorkDone;
                        break;
                }
            }




            //RemoteMessage_CommandHandler(protocol);

            //switch (protocol.msgType)
            //{
            //    case PIPE_MESSAGE_TYPE.Message:
            //        RemoteMessage_MessageHandler(protocol);
            //        break;
            //    case PIPE_MESSAGE_TYPE.Command:
            //        RemoteMessage_CommandHandler(protocol);
            //        break;
            //    case PIPE_MESSAGE_TYPE.Data:
            //        break;
            //    case PIPE_MESSAGE_TYPE.Event:

            //        break;
            //}
        }

        System.Windows.Forms.Timer timer = null;

        public void RemoteProcessStart(WORK_TYPE initWorkType = WORK_TYPE.NONE)        
        {
            if (this.remoteMode != REMOTE_MODE.Master) return;

            // Master 쪽 시퀀스임

            this.remote.ProcessState = REMOTE_PROCESS_STATE.None;

            int tryCount = 0;
            int tryOut = 10;
            while(!this.remote.CheckProcess() && this.IsConnected == false)
            {
                if(tryCount < tryOut)
                {
                    this.remote.StartProcess();
                    Thread.Sleep(1000);
                    tryCount++;
                }
                else
                {
                    MessageBox.Show(this.remote.ProcessName + "을 실행할 수 없습니다.\n실행 시도 횟수 초과 : "+ tryOut);
                    return;
                }
            }

            this.remote.ProcessState = REMOTE_PROCESS_STATE.Start;

            if(this.IsConnected == false)
                this.TryConnect(); // Listen Start

            int time100Millisec = 0;
            int timeOutSec = 100; //10초
            
            while(!this.IsConnected)
            {
                if( time100Millisec < timeOutSec)
                {
                    Thread.Sleep(100);
                    time100Millisec++;
                }
                else
                {
                    MessageBox.Show("Remote Process와 연결할 수 없습니다.\ntime out :" + timeOutSec);
                    return;
                }
            }

            Thread.Sleep(100);  //Read Thread 실행 대기

            StartSync();
            if(WaitProcess(REMOTE_PROCESS_STATE.Sync) == false)
            {
                return;
            }

            //RemoteProcessStartWork();

            if (this.timer == null)
            {
                this.timer = new System.Windows.Forms.Timer();
                this.timer.Interval = 5000;
                this.timer.Tick += timer_tick;
                timer.Start();
            }
        }

        bool isWorking = false;

        public void RemoteProcessStartWork()
        {
            if (WaitProcess(REMOTE_PROCESS_STATE.Sync) == false)
            {
                return;
            }

            ReadyWork();

            if (WaitProcess(REMOTE_PROCESS_STATE.ReadyWork) == false)
            {
                return;
            }

            isWorking = true;

            StartWork();
        }

        public void timer_tick(object obj, EventArgs args)
        {
            if(this.remote.CheckProcess() == false)
            {
                
                RemoteProcessStart();

                if (this.isWorking == true)
                {
                    if (WaitProcess(REMOTE_PROCESS_STATE.Sync) == false)
                    {
                        return;
                    }

                    RemoteProcessStartWork();
                }
            }
        }

        public bool WaitProcess(REMOTE_PROCESS_STATE state, int timeOut100Millisec = 100)
        {
            int time100Millisec = 0;
            while (this.remote.ProcessState != state)
            {
                if (time100Millisec < timeOut100Millisec)
                {
                    Thread.Sleep(100);
                    time100Millisec++;
                }
                else
                {
                    MessageBox.Show("Remote Process를 진행할 수 없습니다 : " + state.ToString() + "\ntime out :" + timeOut100Millisec);
                    return false;
                }
            }

            return true;
        }

        private void RemoteSlaveStartWork()
        {
            this.workplaceBundle.SetWorkState(WORK_TYPE.SNAP);
            this.workplaceBundle.SetSharedBuffer(this.sharedBufferInfoList[0]);
#if DEBUG
            Debug.WriteLine("[Start]");

            DebugOutput.PrintWorkplaceBundle(workplaceBundle);
#endif
            WorkEventManager.OnInspectionStart(this, new InspectionStartArgs(this.workplaceBundle));

            workBundle.SetRecipe(this.recipe);
            workBundle.SetWorkplacBundle(this.workplaceBundle);
            foreach (WorkManager wm in this.workManagers)
            {
                wm.SetWorkplaceBundle(workplaceBundle);
                wm.SetWorkBundle(workBundle);
                wm.Start();
            }
        }

        /// <summary>
        /// Server는 Listen Client는 Connect를 시도한다.
        /// </summary>
        public void TryConnect()
        {
            if (this.remoteMode == REMOTE_MODE.Master)
            {
                this.remote.ListenStart();
            }
            else
            {
                this.remote.TryConnect();
            }
        }


        public void ExitRemoteProcess()
        {
            remote.Exit();
        }

        ~WorkFactory()
        {
            ExitRemoteProcess();
        }
        #endregion

    }
}
