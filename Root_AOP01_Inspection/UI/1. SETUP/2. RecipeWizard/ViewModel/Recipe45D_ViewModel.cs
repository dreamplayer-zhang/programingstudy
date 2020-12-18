using System.Windows.Input;
using Root_AOP01_Inspection.Module;
namespace Root_AOP01_Inspection
{
    class Recipe45D_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;

        public Recipe45D_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
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
                    /*
                      EQ.p_bStop = false;
            PatternVision Patternvision = ((Vega_Handler)m_Engineer.ClassHandler()).m_patternVision;
            if (Patternvision.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            PatternVision.Run_Grab Grab = (PatternVision.Run_Grab)Patternvision.CloneModuleRun("Grab");
            var viewModel = new Dialog_Scan_ViewModel(Patternvision, Grab);
            Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
                     */

                    //MainVision mainVision = ((AOP01_Handler)m_eng)
                });
            }
        }
    }
}
