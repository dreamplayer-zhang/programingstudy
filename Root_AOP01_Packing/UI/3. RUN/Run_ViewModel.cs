using Root_AOP01_Packing.Module;
using Root_EFEM.Module;
using RootTools;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Root_AOP01_Packing
{
    class Run_ViewModel : ObservableObject
    {
        MainWindow m_Mainwindow;
        public AOP01_Engineer m_Engineer;
        public Run_ViewModel(MainWindow main, AOP01_Engineer engineer)
        {
            m_Mainwindow = main;
            m_Engineer = engineer;
            Init();
        }
        private void Init()
        {
            p_ModuleList = m_Engineer.ClassModuleList();
            p_AOP01 = (m_Engineer.ClassHandler() as AOP01_Handler).m_aop01P as AOP01_P;
            p_RTRA = (m_Engineer.ClassHandler() as AOP01_Handler).m_aRTR[0] as WTR_RND;
            p_RTRB = (m_Engineer.ClassHandler() as AOP01_Handler).m_aRTR[1] as WTR_RND;
            p_LoadportA = (m_Engineer.ClassHandler() as AOP01_Handler).m_aLoadport[0] as Loadport_Cymechs;
            p_LoadportB = (m_Engineer.ClassHandler() as AOP01_Handler).m_aLoadport[1] as Loadport_AOP;
            p_Unloadport = (m_Engineer.ClassHandler() as AOP01_Handler).m_unloadport;
            p_Elevator = (m_Engineer.ClassHandler() as AOP01_Handler).m_elevator;
            p_TapePacker = (m_Engineer.ClassHandler() as AOP01_Handler).m_tapePacker;
            p_VacuumPacker = (m_Engineer.ClassHandler() as AOP01_Handler).m_vacuumPacker;

            p_bDoorLock = p_AOP01.do_door_Lock.p_bOut;

            if (EQ.p_eState != EQ.eState.Ready)
                SetBtnEnable(false);

            p_bEnableInitBtn = true;
        }

        #region Property Module
        ModuleList _ModuleList;
        AOP01_P _AOP01;
        WTR_RND _RTRA;
        WTR_RND _RTRB;
        Loadport_Cymechs _LoadportA;
        Loadport_AOP _LoadportB;
        Unloadport_AOP _Unloadport;
        IndividualElevator _Elevator;
        TapePacker _TapePacker;
        VacuumPacker _VacuumPacker;

        public ModuleList p_ModuleList
        {
            get
            {
                return _ModuleList;
            }
            set
            {
                SetProperty(ref _ModuleList, value);
            }
        }
        public AOP01_P p_AOP01
        {
            get
            {
                return _AOP01;
            }
            set
            {
                SetProperty(ref _AOP01, value);
            }
        }
        public WTR_RND p_RTRA
        {
            get
            {
                return _RTRA;
            }
            set
            {
                SetProperty(ref _RTRA, value);
            }
        }
        public WTR_RND p_RTRB
        {
            get
            {
                return _RTRB;
            }
            set
            {
                SetProperty(ref _RTRB, value);
            }
        }
        public Loadport_Cymechs p_LoadportA
        {
            get
            {
                return _LoadportA;
            }
            set
            {
                SetProperty(ref _LoadportA, value);
            }
        }
        public Loadport_AOP p_LoadportB
        {
            get
            {
                return _LoadportB;
            }
            set
            {
                SetProperty(ref _LoadportB, value);
            }
        }
        public Unloadport_AOP p_Unloadport
        {
            get
            {
                return _Unloadport;
            }
            set
            {
                SetProperty(ref _Unloadport, value);
            }
        }
        public IndividualElevator p_Elevator
        {
            get
            {
                return _Elevator;
            }
            set
            {
                SetProperty(ref _Elevator, value);
            }
        }
        public TapePacker p_TapePacker
        {
            get
            {
                return _TapePacker;
            }
            set
            {
                SetProperty(ref _TapePacker, value);
            }
        }
        public VacuumPacker p_VacuumPacker
        {
            get
            {
                return _VacuumPacker;
            }
            set
            {
                SetProperty(ref _VacuumPacker, value);
            }

        }
        #endregion

        #region UI Property
        public string p_sPodBtn
        {
            get
            {
                return _sPodBtn;
            }
            set
            {
                SetProperty(ref _sPodBtn, value);
            }
        }
        private string _sPodBtn = "POD START";

        public string p_sCaseBtn
        {
            get
            {
                return _sCaseBtn;
            }
            set
            {
                SetProperty(ref _sCaseBtn, value);
            }
        }
        private string _sCaseBtn = "CASE START";

        public string p_sStepbtn
        {
            get
            {
                return _sStepbtn;
            }
            set
            {
                SetProperty(ref _sStepbtn, value);
            }
        }
        private string _sStepbtn = "RUN STEP";

        public string p_sInitBtn
        {
            get
            {
                return _sInitBtn;
            }
            set
            {
                SetProperty(ref _sInitBtn, value); 
            }
        }
        private string _sInitBtn = "INITIALIZE";

        public string p_sPauseBtn
        {
            get
            {
                return _sPauseBtn;
            }
            set
            {
                SetProperty(ref _sPauseBtn, value);
            }
        }
        private string _sPauseBtn = "PAUSE";

        public string p_sDoorBtn
        {
            get
            {
                return _sDoorBtn;
            }
            set
            {
                SetProperty(ref _sDoorBtn, value);
            }
        }
        private string _sDoorBtn = "DOOR UNLOCK";

        public string p_sOnlineBtn
        {
            get
            {
                return _sOnlineBtn;
            }
            set
            {
                SetProperty(ref _sOnlineBtn, value);
            }
        }
        private string _sOnlineBtn = "OFFLINE";

        public bool p_bEnablePodBtn
        {
            get
            {
                return _bEnablePodBtn;
            }
            set
            {
                SetProperty(ref _bEnablePodBtn, value);
            }
        }
        private bool _bEnablePodBtn = true;

        public bool p_bEnableCaseBtn
        {
            get
            {
                return _bEnableCaseBtn;
            }
            set
            {
                SetProperty(ref _bEnableCaseBtn, value);
            }
        }
        private bool _bEnableCaseBtn = true;

        public bool p_bEnableStepBtn
        {
            get
            {
                return _bEnableStepBtn;
            }
            set
            {
                SetProperty(ref _bEnableStepBtn, value);
            }
        }
        private bool _bEnableStepBtn = true;

        public bool p_bEnableInitBtn
        {
            get
            {
                return _bEnableInitBtn;
            }
            set
            {
                SetProperty(ref _bEnableInitBtn, value);
            }
        }
        private bool _bEnableInitBtn = true;

        public bool p_bEnablePauseBtn
        {
            get
            {
                return _bEnablePauseBtn;
            }
            set
            {
                SetProperty(ref _bEnablePauseBtn, value);
            }
        }
        private bool _bEnablePauseBtn = true;

        public bool p_bDoorLock
        {
            get
            {
                return _bDoorLock;
            }
            set
            {
                if (value)
                {
                    if (p_AOP01.bDoorClosedCheck())
                    {
                        p_sDoorBtn = "DOOR LOCK";
                        p_AOP01.do_door_Lock.Write(value);
                    }
                    else
                    {
                        p_sDoorBtn = "DOOR UNLOCK";
                        SetProperty(ref _bDoorLock, !value);
                    }
                }
                else
                {
                    p_sDoorBtn = "DOOR UNLOCK";
                    SetProperty(ref _bDoorLock, value);
                }

                
            }
        }
        private bool _bDoorLock = true;

        private void SetBtnEnable(bool bEnable)
        {
            p_bEnablePodBtn = bEnable;
            p_bEnableCaseBtn = bEnable;
            p_bEnableStepBtn = bEnable;
            p_bEnableInitBtn = bEnable;
            p_bEnablePauseBtn = bEnable;
        }

        #endregion
        BackgroundWorker BgwWait;
        public ICommand cmdPodStart
        {
            get
            {
                return new RelayCommand(() => 
                {
                    ModuleList moduleList = m_Engineer.ClassModuleList();
                    if (EQ.p_eState == EQ.eState.Ready && p_sPodBtn != "STOP")
                    {                     
                        moduleList.p_sNowProgress = "POD PACKING SEQUENCE";
                        moduleList.m_moduleRunList.OpenJob(@"C:\Recipe\test.RunAOP01");
                        moduleList.StartModuleRuns();

                        SetBtnEnable(false);
                        p_bEnablePodBtn = true;
                        p_bEnablePauseBtn = true;
                        p_sPodBtn = "STOP";

                        BgwWait = new BackgroundWorker();
                        BgwWait.WorkerSupportsCancellation = true;
                        BgwWait.DoWork += BgwWaitPod_DoWork;
                        BgwWait.RunWorkerAsync();
                    }
                    else
                    {
                        BgwWait.CancelAsync();
                        BgwWait.Dispose();

                        EQ.p_bStop = true;
                        moduleList.m_qModuleRun.Clear();
                        moduleList.p_sNowProgress = "STOP";
                        SetBtnEnable(false);
                        p_bEnableInitBtn = true;
                    }
                });
            }
        }
        private void BgwWaitPod_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                Thread.Sleep(100);
                if ((sender as BackgroundWorker).CancellationPending)
                {
                    p_sPodBtn = "POD START";
                    SetBtnEnable(false);
                    p_bEnableInitBtn = true;
                    BgwWait.Dispose();
                    break;
                }
                if (p_ModuleList.m_qModuleRun.Count == 0)
                {
                    SetBtnEnable(true);
                    p_sPodBtn = "POD START";
                    BgwWait.Dispose();
                    break;
                }
            }

        }
        public ICommand cmdCaseStart
        {
            get
            {
                return new RelayCommand(() =>
                {
                    while (EQ.p_eState != EQ.eState.Ready)               
                        Thread.Sleep(100);

                    SetBtnEnable(true);
                    p_sCaseBtn = "CASE START";
                    BgwWait.Dispose();
                });
            }
        }

        public ICommand cmdInit
        {
            get
            {
                return new RelayCommand(()=>
                {
                    ModuleList moduleList = m_Engineer.ClassModuleList();
                    moduleList.p_moduleList.Clear();
                    moduleList.p_sNowProgress = "INITIALZIE...";
                    moduleList.p_iRun = 0;
                    moduleList.p_Percent = "0";
                    EQ.p_bStop = false;
                    EQ.p_eState = EQ.eState.Home;

                    p_sInitBtn = "STOP";

                    BgwWait = new BackgroundWorker();
                    BgwWait.WorkerSupportsCancellation = true;
                    BgwWait.DoWork += BgwWaitInit_DoWork;
                    BgwWait.RunWorkerAsync();
                });
            }
        }

        public ICommand cmdRunStep
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Nullable<bool> result = m_Mainwindow.dialogService.ShowDialog(m_Mainwindow.m_Dlg_RunStepViewModel);
                    if (result.HasValue)
                    {
                        if (result.Value)
                        {

                        }
                        else
                        {

                        }
                    }
                });
            }
        }
        private void BgwWaitInit_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                Thread.Sleep(100);
                if ((sender as BackgroundWorker).CancellationPending)
                {
                    p_sInitBtn = "INITIALIZE";
                    SetBtnEnable(false);
                    p_bEnableInitBtn = true;
                    BgwWait.Dispose();
                    break;
                }
                if (EQ.p_eState == EQ.eState.Ready)
                {
                    p_sInitBtn = "INITIALIZE";
                    SetBtnEnable(true);
                    BgwWait.Dispose();
                    break;
                }
                if (EQ.p_bStop)
                {
                    p_sInitBtn = "INITIALIZE";
                    SetBtnEnable(false);
                    p_bEnableInitBtn = true;
                    BgwWait.Dispose();
                    break;
                }
            }
        }

        public ICommand cmdPause
        {
            get
            {
                return new RelayCommand(() =>
                {

                    if (EQ.p_eState == EQ.eState.Run)
                    {
                        p_sPauseBtn = "RESUME";
                        EQ.p_eState = EQ.eState.Ready;
                    }

                    else if (EQ.p_eState == EQ.eState.Ready)
                    {
                        p_sPauseBtn = "PAUSE";
                        EQ.p_eState = EQ.eState.Run;
                    }
                });
            }
        }
        public ICommand cmdAlarm
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Engineer.m_handler.m_gaf.m_listALID.ShowPopup();
                });
            }
        }
        public ICommand cmdOnline
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (p_sOnlineBtn == "ONLINE")
                        p_sOnlineBtn = "OFFLINE";
                    else
                        p_sOnlineBtn = "ONLINE";
                });
            }
        }
    }
}
