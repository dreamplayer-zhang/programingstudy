using System.Windows.Input;
using System.Windows.Threading;
using Root_AOP01_Inspection.Module;
using RootTools_Vision;

namespace Root_AOP01_Inspection
{
    class RecipeLADS_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;
        public Setup_ViewModel p_Setup
        {
            get { return m_Setup; }
            set { SetProperty(ref m_Setup, value); }
        }
        AOP01_Engineer m_Engineer;
        MainVision m_mainVision;
        public MainVision p_mainVision
        {
            get { return m_mainVision; }
            set { SetProperty(ref m_mainVision, value); }
        }
        public RecipeLADS_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            m_Engineer = GlobalObjects.Instance.Get<AOP01_Engineer>();
            m_mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;
            m_mainVision.dispatcher = Dispatcher.CurrentDispatcher;
        }

        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_RecipeWizardPanel();
                });
            }
        }

        #region Property

        #region PellicleExpanding Parameter
        int m_nPellicleExpandingSpec = 100;
        public int p_nPellicleExpandingSpec
        {
            get { return m_nPellicleExpandingSpec; }
            set { SetProperty(ref m_nPellicleExpandingSpec, value); }
        }
        #endregion

        #endregion

        public ICommand btnSnap
        {
            get
            {
                return new RelayCommand(() => {
                    MainVision mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;
                    MainVision.Run_LADS lads = (MainVision.Run_LADS)mainVision.CloneModuleRun("LADS");
                    mainVision.StartRun(lads);
                });
            }
        }

        public ICommand btnInspection
        {
            get
            {
                return new RelayCommand(() => {
                    MainVision mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;
                    if (true)
                    {
                        MainVision.Run_PellicleExpandingInspection pellicleExpandingInspection = (MainVision.Run_PellicleExpandingInspection)mainVision.CloneModuleRun("PellicleExpandingInspection");
                        mainVision.StartRun(pellicleExpandingInspection);
                    }
                });
            }
        }
    }
}
