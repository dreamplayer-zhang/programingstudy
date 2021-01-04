using System.Windows.Input;
using Root_AOP01_Inspection.Module;
using RootTools;

namespace Root_AOP01_Inspection
{
    class Recipe45D_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;
        AOP01_Engineer m_Engineer;

        public Recipe45D_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            m_Engineer = setup.m_MainWindow.m_engineer;
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
        public ICommand btnSnap
        {
            get
            {
                return new RelayCommand(()=> {
                    MainVision mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;
                    MainVision.Run_Grab45 grab = (MainVision.Run_Grab45)mainVision.CloneModuleRun("Run Grab 45");
                    mainVision.StartRun(grab);
                });
            }
        }
    }
}
