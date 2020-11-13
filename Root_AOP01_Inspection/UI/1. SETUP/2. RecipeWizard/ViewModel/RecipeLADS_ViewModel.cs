using System.Windows.Input;

namespace Root_AOP01
{
    class RecipeLADS_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;
        public RecipeLADS_ViewModel(Setup_ViewModel setup)
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
