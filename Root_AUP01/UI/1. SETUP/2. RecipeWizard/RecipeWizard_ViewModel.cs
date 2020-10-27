using System.Windows.Input;

namespace Root_AOP01
{
    class RecipeWizard_ViewModel : ObservableObject
    {

        public RecipeWizard_Panel RecipeWizard = new RecipeWizard_Panel();
        public RecipeOption_Panel RecipeOption = new RecipeOption_Panel();
        public Recipe45D_Panel Recipe45D = new Recipe45D_Panel();
        public RecipeBackside_Panel RecipeBackside = new RecipeBackside_Panel();
        public RecipeEdge_Panel RecipeEdge = new RecipeEdge_Panel();
        public RecipeLADS_Panel RecipeLADS = new RecipeLADS_Panel();

        Setup_ViewModel m_Setup;
        public RecipeWizard_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;        
        }
        public ICommand btnRecipeOption
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_RecipeOptionPanel();
                });
            }
        }
        public ICommand btn45D
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_Recipe45DPanel();
                });
            }
        }
        public ICommand btnBackside
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_RecipeBacksidePanel();
                });
            }
        }
        public ICommand btnEdge
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_RecipeEdgePanel();
                });
            }
        }
        public ICommand btnLADS
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_RecipeLADSPanel();
                });
            }
        }
        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_HomePanel();
                });
            }
        }
    }
}
