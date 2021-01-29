using System.Windows.Input;
using Root_AOP01_Inspection.Module;
namespace Root_AOP01_Inspection
{
    class RecipeLADS_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;
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
            m_Engineer = setup.m_MainWindow.m_engineer;
            m_mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;
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
        bool m_bEnablePellicleExpanding = false;
        public bool p_bEnablePellicleExpanding
        {
            get { return m_bEnablePellicleExpanding; }
            set { SetProperty(ref m_bEnablePellicleExpanding, value); }
        }
        #endregion

        public ICommand btnSnap
        {
            get
            {
                return new RelayCommand(() => {
                    MainVision mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;
                    MainVision.Run_GrabSideScan grab = (MainVision.Run_GrabSideScan)mainVision.CloneModuleRun("Run LADS");
                    mainVision.StartRun(grab);
                });
            }
        }
    }
}
