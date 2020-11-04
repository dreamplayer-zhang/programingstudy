using System.Windows.Controls;
using System.Windows.Input;

namespace Root_AOP01
{
    class RecipeWizard_ViewModel : ObservableObject
    {
        public RecipeWizard_Panel RecipeWizard = new RecipeWizard_Panel();

        public RecipeSummary_Page RecipeSummary = new RecipeSummary_Page();
        public RecipeSpec_Page RecipeSpec = new RecipeSpec_Page();

        public Recipe45D_Panel Recipe45D = new Recipe45D_Panel();
        public RecipeFrontside_Panel RecipeFrontside = new RecipeFrontside_Panel();
        public RecipeEdge_Panel RecipeEdge = new RecipeEdge_Panel();
        public RecipeLADS_Panel RecipeLADS = new RecipeLADS_Panel();

        Setup_ViewModel m_Setup;
        public RecipeWizard_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;        
        }


        public ICommand btnSummary
        {
            get
            {
                return new RelayCommand(() =>
                {
                    bool check = (bool)RecipeWizard.btnSummary.IsChecked;
                    if (check)
                    {
                        RecipeWizard.btnSpec.IsChecked = false;
                        RecipeWizard.btnSummary.IsChecked = true;
                        m_Setup.Set_RecipeSummary();
                    }
                });
            }
        }
        public ICommand btnRecipeSpec
        {
            get
            {
                return new RelayCommand(() =>
                {
                    bool check = (bool)RecipeWizard.btnSpec.IsChecked;
                    if (check)
                    {
                        RecipeWizard.btnSummary.IsChecked = false;
                        RecipeWizard.btnSpec.IsChecked = true;
                        m_Setup.Set_RecipeSpec();
                    }
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
        public ICommand btnFrontside
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_RecipeFrontsidePanel();
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

        public ICommand btnRun
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.m_MainWindow.MainPanel.Children.Clear();
                    m_Setup.m_MainWindow.MainPanel.Children.Add(m_Setup.m_MainWindow.Review);
                });
            }
        }
    }
}
