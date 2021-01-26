using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2.UI_Temp
{
    class RecipeWizardPanel_ViewModel : ObservableObject
    {
        #region [Bindings Objects]
        private UserControl m_CurrentPanel;
        public UserControl p_CurrentPanel
        {
            get
            {
                return m_CurrentPanel;
            }
            set
            {
                SetProperty(ref m_CurrentPanel, value);
            }
        }
        #endregion

        #region [Views]
        public readonly RecipeWizardPanel Main = new RecipeWizardPanel();

        // FRONT
        public readonly UI_Temp.FrontsideSummary frontsideSummary = new UI_Temp.FrontsideSummary();
        public readonly UI_Temp.FrontsideProduct frontsideProduct = new UI_Temp.FrontsideProduct();
        public readonly UI_Temp.FrontsideOrigin frontsideOrigin = new UI_Temp.FrontsideOrigin();


        // BACK

        // EDGE

        // EBR
        #endregion

        #region [ViewModels]

        private UI_Temp.FrontsideProduct_ViewModel frontsideProductVM = new FrontsideProduct_ViewModel();
        public UI_Temp.FrontsideProduct_ViewModel FrontsideProductVM
        {
            get => frontsideProductVM;
        }

        private UI_Temp.FrontsideOrigin_ViewModel frontsideOriginVM = new FrontsideOrigin_ViewModel();
        public UI_Temp.FrontsideOrigin_ViewModel FrontsideOriginVM
        {
            get => frontsideOriginVM;
        }


        #endregion
        public RecipeWizardPanel_ViewModel()
        {
            Initialize();


        }

        public void Initialize()
        {

        }

        #region [Command]
        public ICommand btnFrontSummary
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(frontsideSummary);
                    //p_Summary_VM.SetPage();
                });
            }
        }

        public ICommand btnFrontProduct
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(frontsideProduct);
                    frontsideProduct.DataContext = frontsideProductVM;
                    //p_Summary_VM.SetPage();
                });
            }
        }

        public ICommand btnFrontOrigin
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(frontsideOrigin);
                    frontsideOrigin.DataContext = frontsideOriginVM;
                    //p_Summary_VM.SetPage();
                });
            }
        }
        #endregion

        public void SetPage(UserControl page)
        {
            p_CurrentPanel = page;
        }
    }
}
