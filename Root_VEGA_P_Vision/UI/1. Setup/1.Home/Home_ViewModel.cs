using System.Windows.Controls;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    public class Home_ViewModel : ObservableObject
    {
        public Setup_ViewModel m_Setup;
        public HomePanel Main;
        UserControl m_CurrentPanel;

        public RecipeManager_ViewModel recipeManagerVM;
        public Maintenance_ViewModel maintVM;
        public PodInfo_ViewModel podInfoVM;

        public UserControl SubPanel
        {
            get => m_CurrentPanel;
            set => SetProperty(ref m_CurrentPanel, value);
        }

        public Home_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            Init();
        }
        private void Init()
        {
            Main = new HomePanel();
            Main.DataContext = this;
            recipeManagerVM = new RecipeManager_ViewModel(this);
            maintVM = new Maintenance_ViewModel(this);
            podInfoVM = new PodInfo_ViewModel(this);

            SubPanel = podInfoVM.Main;
            m_Setup.SetPodInfo();
        }

        public ICommand btnRecipeWizard
        {
            get => new RelayCommand(() =>
            {
                SubPanel = recipeManagerVM.Main;
                m_Setup.SetRecipeWizard();
            });
        }
        public ICommand btnMaintenance
        {
            get => new RelayCommand(() =>
            {
                SubPanel = maintVM.Main;
                m_Setup.SetMaintenance();
            });
        }
        public ICommand btnPodInfo
        {
            get => new RelayCommand(() =>
            {
                SubPanel = podInfoVM.Main;
                m_Setup.SetPodInfo();
            });
        }
    }
}
