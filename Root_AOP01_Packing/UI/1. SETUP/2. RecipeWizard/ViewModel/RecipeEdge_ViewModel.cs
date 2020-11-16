using System.Windows.Input;

namespace Root_AOP01_Packing
{
    class RecipeEdge_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;
        public RecipeEdge_ViewModel(Setup_ViewModel setup)
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
    }
}
