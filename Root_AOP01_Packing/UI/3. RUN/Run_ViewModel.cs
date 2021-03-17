using Root_AOP01_Packing.Module;
using Root_EFEM.Module;
using RootTools;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_AOP01_Packing
{
    class Run_ViewModel : ObservableObject
    {
        MainWindow m_Mainwindow;
        AOP01_Engineer m_Engineer;
        public Run_ViewModel(MainWindow main, AOP01_Engineer engineer)
        {
            m_Mainwindow = main;
            m_Engineer = engineer;
            Init();
        }
        private void Init()
        {
            p_ModuleList = m_Engineer.ClassModuleList();
            p_RTRA = (m_Engineer.ClassHandler() as AOP01_Handler).m_aRTR[0] as WTR_RND;
            p_RTRB = (m_Engineer.ClassHandler() as AOP01_Handler).m_aRTR[1] as WTR_RND;
            p_LoadportA = (m_Engineer.ClassHandler() as AOP01_Handler).m_aLoadport[0] as Loadport_Cymechs;
            p_LoadportB = (m_Engineer.ClassHandler() as AOP01_Handler).m_aLoadport[1] as Loadport_AOP;
            p_Unloadport = (m_Engineer.ClassHandler() as AOP01_Handler).m_unloadport;
            p_Elevator = (m_Engineer.ClassHandler() as AOP01_Handler).m_elevator;
            p_TapePacker = (m_Engineer.ClassHandler() as AOP01_Handler).m_tapePacker;
            p_VacuumPacker = (m_Engineer.ClassHandler() as AOP01_Handler).m_vacuumPacker;
        }

        #region Property Module
        ModuleList _ModuleList;
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
        private string _sInitBtn = "INITIALZE";

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

        private void SetBtnDisable()
        {
            p_bEnablePodBtn = false;
            p_bEnableCaseBtn = false;
            p_bEnableStepBtn = false;
            p_bEnableInitBtn = false;
        }

        #endregion
        public ICommand cmdPodStart
        {
            get
            {
                return new RelayCommand(() => 
                {
                    if (p_sPodBtn == "POD START")
                    {
                        ModuleList moduleList = m_Engineer.ClassModuleList();
                        moduleList.p_sNowProgress = "POD PACKING SEQUENCE";
                        moduleList.m_moduleRunList.OpenJob(@"C:\Recipe\test.RunAOP01");
                        moduleList.StartModuleRuns();
                        p_sPodBtn = "STOP";
                        SetBtnDisable();
                        p_bEnablePodBtn = true;
                    }
                    else
                    {
                        p_sPodBtn = "POD START";
                        SetBtnDisable();
                        p_bEnablePodBtn = true;
                    }
                });
            }
        }
        public ICommand cmdCaseStart
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }

        public ICommand cmdStart
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //if (p_sNameStartBtn == "START")
                    //{
                    //    m_Engineer.ClassModuleList().StartModuleRuns();
                    //    p_sNameStartBtn = "STOP";
                    //}
                    //else
                    //{
                    //    m_Engineer.ClassModuleList().ThreadStop();
                    //    EQ.p_eState = EQ.eState.Error;
                    //    EQ.p_bStop = true;
                    //    p_sNameStartBtn = "START";
                    //}
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
                    moduleList.p_sNowProgress = "Initialize...";
                    moduleList.p_iRun = 0;
                    moduleList.p_Percent = "0";
                    EQ.p_bStop = false;
                    EQ.p_eState = EQ.eState.Home;
                });
            }
        }
        public ICommand cmdHome
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ModuleList moduleList = m_Engineer.ClassModuleList();
                    
                    moduleList.p_iRun = 0;
                    foreach (ModuleRunBase moduleRun in moduleList.p_moduleList)
                    {
                        moduleList.p_Percent = "0";
                        moduleRun.p_eRunState = ModuleRunBase.eRunState.Ready;
                    }
                    EQ.p_eState = EQ.eState.Home;
                });

            }
        }
    }
}
