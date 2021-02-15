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

        private string _sNameStartBtn = "START";
        public string p_sNameStartBtn
        {
            get
            {
                return _sNameStartBtn;
            }
            set
            {
                SetProperty(ref _sNameStartBtn, value);
            }
        }

        public ICommand cmdStart
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (p_sNameStartBtn == "START")
                    {
                        m_Engineer.ClassModuleList().StartModuleRuns();
                        p_sNameStartBtn = "STOP";
                    }
                    else
                    {
                        m_Engineer.ClassModuleList().ThreadStop();
                        EQ.p_eState = EQ.eState.Error;
                        EQ.p_bStop = true;
                        p_sNameStartBtn = "START";
                    }
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
