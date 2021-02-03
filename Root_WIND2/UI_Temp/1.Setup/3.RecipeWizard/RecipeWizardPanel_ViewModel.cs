using RootTools_Vision;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2.UI_Temp
{
    class RecipeWizardPanel_ViewModel : ObservableObject
    {
        #region [Properties]
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


        private bool isEnableAlignment = false;
        public bool IsEnabledAlignment
        { 
            get => this.isEnableAlignment;
            set 
            {
                SetProperty<bool>(ref this.isEnableAlignment, value);   
            } 
        }

        private bool isEnableMask = false;
        public bool IsEnabledMask
        {
            get => this.isEnableMask;
            set
            {
                SetProperty<bool>(ref this.isEnableMask, value);
            }
        }

        private bool isEnableSpec = false;
        public bool IsEnabledSpec
        {
            get => this.isEnableSpec;
            set
            {
                SetProperty<bool>(ref this.isEnableSpec, value);
            }
        }



        #endregion

        #region [Views]
        public readonly RecipeWizardPanel Main = new RecipeWizardPanel();

        // FRONT
        public readonly UI_Temp.FrontsideSummary frontsideSummary = new UI_Temp.FrontsideSummary();
        public readonly UI_Temp.FrontsideProduct frontsideProduct = new UI_Temp.FrontsideProduct();
        public readonly UI_Temp.FrontsideOrigin frontsideOrigin = new UI_Temp.FrontsideOrigin();
        public readonly UI_Temp.FrontsideAlignment frontsideAlignment = new UI_Temp.FrontsideAlignment();
        public readonly UI_Temp.FrontsideMask frontsideMask = new UI_Temp.FrontsideMask();
        public readonly UI_Temp.FrontsideSpec frontsideSpec = new UI_Temp.FrontsideSpec();
        public readonly UI_Temp.FrontsideInspect frontsideInspect = new UI_Temp.FrontsideInspect();


        // BACK

        // EDGE
        public readonly UI_Temp.EdgesideSetup edgesideSetup = new UI_Temp.EdgesideSetup();

        // EBR
        public readonly UI_Temp.EBRSetup ebrSetup = new UI_Temp.EBRSetup();
        #endregion

        #region [ViewModels]
        private UI_Temp.FrontsideProduct_ViewModel frontsideSummaryVM = new FrontsideProduct_ViewModel();
        public UI_Temp.FrontsideProduct_ViewModel FrontsideSummaryVM
        {
            get => frontsideSummaryVM;
        }

        private UI_Temp.FrontsideProduct_ViewModel frontsideProductVM = new FrontsideProduct_ViewModel();
        public UI_Temp.FrontsideProduct_ViewModel FrontsideProductVM
        {
            get => frontsideProductVM;
        }

        private UI_Temp.FrontsideOrigin_ViewModel frontsideOriginVM = new UI_Temp.FrontsideOrigin_ViewModel();
        public UI_Temp.FrontsideOrigin_ViewModel FrontsideOriginVM
        {
            get => frontsideOriginVM;
        }

        private UI_Temp.FrontsideAlignment_ViewModel frontsideAlignmentVM = new UI_Temp.FrontsideAlignment_ViewModel();
        public UI_Temp.FrontsideAlignment_ViewModel FrontsideAlignmentVM
        {
            get => frontsideAlignmentVM;
        }

        private UI_Temp.FrontsideMask_ViewModel frontsideMaskVM = new UI_Temp.FrontsideMask_ViewModel();
        public UI_Temp.FrontsideMask_ViewModel FrontsideMaskVM
        {
            get => frontsideMaskVM;
        }

        private UI_Temp.FrontsideSpec_ViewModel frontsideSpecVM = new UI_Temp.FrontsideSpec_ViewModel();
        public UI_Temp.FrontsideSpec_ViewModel FrontsideSpecVM
        {
            get => frontsideSpecVM;
        }

        private UI_Temp.FrontsideInspect_ViewModel frontsideInspectVM = new UI_Temp.FrontsideInspect_ViewModel();
        public UI_Temp.FrontsideInspect_ViewModel FrontsideInspectVM
        {
            get => frontsideInspectVM;
        }

        private UI_Temp.EdgesideSetup_ViewModel edgesideSetupVM = new UI_Temp.EdgesideSetup_ViewModel();
        public UI_Temp.EdgesideSetup_ViewModel EdgesideSetupVM
		{
            get => edgesideSetupVM;
		}

        private UI_Temp.EBRSetup_ViewModel ebrSetupVM = new UI_Temp.EBRSetup_ViewModel();
        public UI_Temp.EBRSetup_ViewModel EBRSetupVM
        {
            get => ebrSetupVM;
        }
        #endregion


        public RecipeWizardPanel_ViewModel()
        {
            Initialize();

            WIND2EventManager.RecipeUpdated += RecipeUpdated_Callback;
        }

        public void Initialize()
        {
            SetButtonEnable();
        }

        private void RecipeUpdated_Callback(object obj, RecipeEventArgs args)
        {
            SetButtonEnable();
        }

        public void SetButtonEnable()
        {
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            if(originRecipe.OriginWidth == 0 || originRecipe.OriginHeight == 0)
            {
                this.IsEnabledAlignment = false;
            }
            else
            {
                this.IsEnabledAlignment = true;
            }

            if (originRecipe.OriginWidth == 0 || originRecipe.OriginHeight == 0)
            {
                this.IsEnabledMask = false;
            }
            else
            {
                this.IsEnabledMask = true;
            }

            // Mask는 Default로 전체 영역 추가
            if (originRecipe.OriginWidth == 0 || originRecipe.OriginHeight == 0)
            {
                this.IsEnabledSpec = false;
            }
            else
            {
                this.IsEnabledSpec = true;
            }
        }


        #region [Command]
        public ICommand btnFrontSummary
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(frontsideSummary);
                    frontsideSummary.DataContext = frontsideSummaryVM;
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
                });
            }
        }

        public ICommand btnFrontAlignment
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(frontsideAlignment);
                    frontsideAlignment.DataContext = frontsideAlignmentVM;
                    frontsideAlignmentVM.SetPage();
                });
            }
        }

        public ICommand btnFrontMask
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(frontsideMask);
                    frontsideMask.DataContext = frontsideMaskVM;
                    frontsideMaskVM.SetPage(); // 이거 제거하자 아닌가
                });
            }
        }

        public ICommand btnFrontSpec
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(frontsideSpec);
                    frontsideSpec.DataContext = frontsideSpecVM;
                    frontsideSpecVM.SetPage();
                });
            }
        }

        public ICommand btnFrontInspect
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(frontsideInspect);
                    frontsideInspect.DataContext = frontsideInspectVM;
                });
            }
        }

        public ICommand btnEdgeSetup
		{
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(edgesideSetup);
                    edgesideSetup.DataContext = edgesideSetupVM;
                });
            }
        }
        public ICommand btnEBRSetup
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(ebrSetup);
                    ebrSetup.DataContext = ebrSetupVM;
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
