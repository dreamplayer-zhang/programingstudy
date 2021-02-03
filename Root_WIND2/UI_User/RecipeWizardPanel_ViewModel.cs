using RootTools_Vision;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2.UI_User
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
        public readonly UI_User.FrontsideSummary frontsideSummary = new UI_User.FrontsideSummary();
        public readonly UI_User.FrontsideProduct frontsideProduct = new UI_User.FrontsideProduct();
        public readonly UI_User.FrontsideOrigin frontsideOrigin = new UI_User.FrontsideOrigin();
        public readonly UI_User.FrontsideAlignment frontsideAlignment = new UI_User.FrontsideAlignment();
        public readonly UI_User.FrontsideMask frontsideMask = new UI_User.FrontsideMask();
        public readonly UI_User.FrontsideSpec frontsideSpec = new UI_User.FrontsideSpec();
        public readonly UI_User.FrontsideInspect frontsideInspect = new UI_User.FrontsideInspect();


        // BACK

        // EDGE

        // EBR


        // Camera
        public readonly UI_User.Camera_VRS cameraVrs = new UI_User.Camera_VRS();
        #endregion

        #region [ViewModels]

        #region [Front ViewModels]
        private UI_User.FrontsideProduct_ViewModel frontsideSummaryVM = new FrontsideProduct_ViewModel();
        public UI_User.FrontsideProduct_ViewModel FrontsideSummaryVM
        {
            get => frontsideSummaryVM;
        }

        private UI_User.FrontsideProduct_ViewModel frontsideProductVM = new FrontsideProduct_ViewModel();
        public UI_User.FrontsideProduct_ViewModel FrontsideProductVM
        {
            get => frontsideProductVM;
        }

        private UI_User.FrontsideOrigin_ViewModel frontsideOriginVM = new UI_User.FrontsideOrigin_ViewModel();
        public UI_User.FrontsideOrigin_ViewModel FrontsideOriginVM
        {
            get => frontsideOriginVM;
        }

        private UI_User.FrontsideAlignment_ViewModel frontsideAlignmentVM = new UI_User.FrontsideAlignment_ViewModel();
        public UI_User.FrontsideAlignment_ViewModel FrontsideAlignmentVM
        {
            get => frontsideAlignmentVM;
        }

        private UI_User.FrontsideMask_ViewModel frontsideMaskVM = new UI_User.FrontsideMask_ViewModel();
        public UI_User.FrontsideMask_ViewModel FrontsideMaskVM
        {
            get => frontsideMaskVM;
        }

        private UI_User.FrontsideSpec_ViewModel frontsideSpecVM = new UI_User.FrontsideSpec_ViewModel();
        public UI_User.FrontsideSpec_ViewModel FrontsideSpecVM
        {
            get => frontsideSpecVM;
        }

        private UI_User.FrontsideInspect_ViewModel frontsideInspectVM = new UI_User.FrontsideInspect_ViewModel();
        public UI_User.FrontsideInspect_ViewModel FrontsideInspectVM
        {
            get => frontsideInspectVM;
        }
        #endregion

        #region [Camera ViewModes]
        private UI_User.CameraVRS_ViewModel cameraVrsVM = new UI_User.CameraVRS_ViewModel();
        public UI_User.CameraVRS_ViewModel CameraVrsVM
        {
            get => cameraVrsVM;
        }

        #endregion


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

        #region [Command Front]
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
        #endregion

        #region [Command Camera]
        public ICommand btnCameraVRS
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(cameraVrs);
                    cameraVrs.DataContext = CameraVrsVM;
                });
            }
        }
        #endregion
        #endregion

        public void SetPage(UserControl page)
        {
            p_CurrentPanel = page;
        }
    }
}
