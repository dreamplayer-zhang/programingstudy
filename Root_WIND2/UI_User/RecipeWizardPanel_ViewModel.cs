using RootTools_Vision;
using System.IO;
using System.Windows;
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

        private bool isEnableOthers = false;
        public bool IsEnabledOthers
        {
            get => this.isEnableOthers;
            set
            {
                SetProperty<bool>(ref this.isEnableOthers, value);
            }
        }

        #endregion

        #region [Views]
        public readonly RecipeWizardPanel Main = new RecipeWizardPanel();

        // HOME
        public readonly UI_User.HomeRecipe homeRecipe = new HomeRecipe();

        // FRONT
        public readonly UI_User.FrontsideSummary frontsideSummary = new UI_User.FrontsideSummary();
        public readonly UI_User.FrontsideProduct frontsideProduct = new UI_User.FrontsideProduct();
        public readonly UI_User.FrontsideOrigin frontsideOrigin = new UI_User.FrontsideOrigin();
        public readonly UI_User.FrontsideAlignment frontsideAlignment = new UI_User.FrontsideAlignment();
        public readonly UI_User.FrontsideMask frontsideMask = new UI_User.FrontsideMask();
        public readonly UI_User.FrontsideSpec frontsideSpec = new UI_User.FrontsideSpec();
        public readonly UI_User.FrontsideInspect frontsideInspect = new UI_User.FrontsideInspect();
        public readonly UI_User.FrontsideOthers frontsideOthers = new UI_User.FrontsideOthers();

        // BACK
        //btnBackProduct
        public readonly UI_User.BacksideProduct backsideProduct = new UI_User.BacksideProduct();
        public readonly UI_User.BacksideSetup backsideSetup = new UI_User.BacksideSetup();
        public readonly UI_User.BacksideInspect backsideInspect = new UI_User.BacksideInspect();
        public readonly UI_User.BacksideSpec backsideSpec = new BacksideSpec();

        // EDGE
        public readonly UI_User.EdgesideSetup edgesideSetup = new UI_User.EdgesideSetup();
		public readonly UI_User.EdgesideInspect edgesideInspect = new UI_User.EdgesideInspect();

		// EBR
		public readonly UI_User.EBRSetup ebrSetup = new UI_User.EBRSetup();
		public readonly UI_User.EBRInspect ebrInspect = new UI_User.EBRInspect();

        // Camera
        public readonly UI_User.CameraVRS cameraVrs = new UI_User.CameraVRS();
        public readonly UI_User.CameraAlign cameraAlign = new UI_User.CameraAlign();
        #endregion

        #region [ViewModels]

        #region [Home ViewModels]
        private UI_User.HomeRecipe_ViewModel homeRecipeVM = new HomeRecipe_ViewModel();
        public UI_User.HomeRecipe_ViewModel HomeRecipeVM
        {
            get => homeRecipeVM;
        }
        #endregion

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

        private UI_User.FrontsideOthers_ViewModel frontsideOthersVM = new UI_User.FrontsideOthers_ViewModel();
        public UI_User.FrontsideOthers_ViewModel FrontsideOthersVM
        {
            get => frontsideOthersVM;
        }
        
        #endregion

        #region [Back ViewModels]
        private UI_User.BacksideProduct_ViewModel backsideProductVM = new BacksideProduct_ViewModel();
        public UI_User.BacksideProduct_ViewModel BacksideProductVM
        {
            get => this.backsideProductVM;
        }

        private UI_User.BacksideSetup_ViewModel backsideSetupVM = new UI_User.BacksideSetup_ViewModel();
        public UI_User.BacksideSetup_ViewModel BacksideROIVM
        {
            get => this.backsideSetupVM;
        }

        private UI_User.BacksideSpec_ViewModel backsideSpecVM = new UI_User.BacksideSpec_ViewModel();
        public UI_User.BacksideSpec_ViewModel BacksideSpecVM
        {
            get => this.backsideSpecVM;
        }

        private UI_User.BacksideInspect_ViewModel backsideInspectVM = new UI_User.BacksideInspect_ViewModel();
        public UI_User.BacksideInspect_ViewModel BacksideInspectVM
        {
            get => this.backsideInspectVM;
        }
        #endregion

        #region [Edge ViewModels]
        private UI_User.EdgesideSetup_ViewModel edgesideSetupVM = new UI_User.EdgesideSetup_ViewModel();
        public UI_User.EdgesideSetup_ViewModel EdgesideSetupVM
        {
            get => edgesideSetupVM;
        }
        private UI_User.EdgesideInspect_ViewModel edgesideInspectionVM = new UI_User.EdgesideInspect_ViewModel();
        public UI_User.EdgesideInspect_ViewModel EdgesideInspectionVM
        {
            get => edgesideInspectionVM;
        }
		#endregion

		#region [EBR ViewModels]
        private UI_User.EBRSetup_ViewModel ebrSetupVM = new UI_User.EBRSetup_ViewModel();
        public UI_User.EBRSetup_ViewModel EBRSetupVM
        {
            get => ebrSetupVM;
        }

        private UI_User.EBRInspect_ViewModel ebrInspectVM = new UI_User.EBRInspect_ViewModel();
        public UI_User.EBRInspect_ViewModel EBRInspectVM
        {
            get => ebrInspectVM;
        }
        #endregion

        #region [Camera ViewModels]
        private UI_User.CameraAlign_ViewModel cameraAlignVM = new UI_User.CameraAlign_ViewModel();
        public UI_User.CameraAlign_ViewModel CameraAlignVM
        {
            get => cameraAlignVM;
        }

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

            frontsideOrigin.DataContext = frontsideOriginVM;

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
            if (originRecipe.OriginWidth == 0 || originRecipe.OriginHeight == 0)
            {
                this.IsEnabledOthers = false;
            }
            else
            {
                this.IsEnabledOthers = true;
            }
        }


        #region [Command]

        #region [Command Parent RadioButton]
        public ICommand btnHomeClickedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(homeRecipe);
                    homeRecipe.DataContext = homeRecipeVM;
                });
            }
        }
        public ICommand btnFrontClickedCommand
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

        public ICommand btnBackClickedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(backsideSetup);
                    backsideSetup.DataContext = backsideSetupVM;
                });
            }
        }

        public ICommand btnEdgeClickedCommand
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

        public ICommand btnEBRClickedCommand
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

        public ICommand btnCameraClickedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(cameraVrs);
                    cameraVrs.DataContext = cameraVrsVM;
                });
            }
        }

        #endregion

        #region [Command Home]
        public ICommand btnHomeRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(homeRecipe);
                    homeRecipe.DataContext = homeRecipeVM;
                });
            }
        }
        #endregion

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

        public ICommand btnFrontOthers
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(frontsideOthers);
                    frontsideOthers.DataContext = frontsideOthersVM;
                });
            }
        }        

        public ICommand btnNewRecipeFront
        {
            get => new RelayCommand(() =>
            {
                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.InitialDirectory = Constants.RootPath.RecipeFrontRootPath;
                dlg.Title = "Save Recipe";
                dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                    string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                    string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                    string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명
                    string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                    DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
                    if (!dir.Exists)
                        dir.Create();

                    RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
                    recipe.Clear();

                    recipe.Name = sFileNameNoExt;
                    recipe.RecipePath = sFullPath;
                    recipe.RecipeFolderPath = sRecipeFolderPath;

                    recipe.Save(sFullPath);
                }
            });
        }

        public ICommand btnSaveRecipeFront
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
                    if (recipe.RecipePath != "")
                    {
                        recipe.Save(recipe.RecipePath);
                    }
                    else
                    {
                        System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                        dlg.InitialDirectory = Constants.RootPath.RecipeFrontRootPath;
                        dlg.Title = "Save Recipe";
                        dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                            string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                            string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                            string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명
                            string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                            DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
                            if (!dir.Exists)
                                dir.Create();

                            recipe.Name = sFileNameNoExt;
                            recipe.RecipePath = sFullPath;
                            recipe.RecipeFolderPath = sRecipeFolderPath;

                            recipe.Save(sFullPath);
                        }
                    }
                });
            }
        }

        public ICommand btnLoadRecipeFront
        {
            get
            {
                return new RelayCommand(() =>
                {
                    System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                    dlg.InitialDirectory = Constants.RootPath.RecipeFrontRootPath;
                    dlg.Title = "Load Recipe";
                    dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                        string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                        string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                        string sFullPath = Path.Combine(sFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                        DirectoryInfo dir = new DirectoryInfo(sFolderPath);
                        if (!dir.Exists)
                            dir.Create();

                        RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
                        recipe.Read(sFullPath);

                        UpdateCurrentPanel();
                        WIND2EventManager.OnRecipeUpdated(this, new RecipeEventArgs());
                    }
                });
            }
        }
        #endregion

        #region [Command Edge]
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

        public ICommand btnEdgeInspect
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(edgesideInspect);
                    edgesideInspect.DataContext = EdgesideInspectionVM;
                });
            }
        }
        

        public ICommand btnNewRecipeEdge
        {
            get => new RelayCommand(() =>
            {
                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.InitialDirectory = Constants.RootPath.RecipeEdgeRootPath;
                dlg.Title = "Save Recipe";
                dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                    string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                    string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                    string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명
                    string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                    DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
                    if (!dir.Exists)
                        dir.Create();

                    RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
                    recipe.Clear();

                    recipe.Name = sFileNameNoExt;
                    recipe.RecipePath = sFullPath;
                    recipe.RecipeFolderPath = sRecipeFolderPath;

                    recipe.Save(sFullPath);
                }
            });
        }

        public ICommand btnSaveRecipeEdge
		{
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
                    if (recipe.RecipePath != "")
                    {
                        recipe.Save(recipe.RecipePath);
                    }
                    else
                    {
                        System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                        dlg.InitialDirectory = Constants.RootPath.RecipeEdgeRootPath;
                        dlg.Title = "Save Recipe";
                        dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                            string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                            string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                            string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명
                            string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                            DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
                            if (!dir.Exists)
                                dir.Create();

                            recipe.Name = sFileNameNoExt;
                            recipe.RecipePath = sFullPath;
                            recipe.RecipeFolderPath = sRecipeFolderPath;

                            recipe.Save(sFullPath);
                        }
                    }
                });
            }
        }

        public ICommand btnLoadRecipeEdge
		{
            get
            {
                return new RelayCommand(() =>
                {
                    System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                    dlg.InitialDirectory = Constants.RootPath.RecipeEdgeRootPath;
                    dlg.Title = "Load Recipe";
                    dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                        string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                        string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                        string sFullPath = Path.Combine(sFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                        DirectoryInfo dir = new DirectoryInfo(sFolderPath);
                        if (!dir.Exists)
                            dir.Create();

                        RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
                        recipe.Read(sFullPath);

                        edgesideSetupVM.LoadParameter();
                        //UpdateUI();
                    }
                });
            }
        }
        #endregion

        #region [Command EBR]
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

        public ICommand btnEBRInspect
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(ebrInspect);
                    ebrInspect.DataContext = ebrInspectVM;
                });
            }
        }
        
        public ICommand btnNewRecipeEBR
        {
            get => new RelayCommand(() =>
            {
                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.InitialDirectory = Constants.RootPath.RecipeEBRRootPath;
                dlg.Title = "Save Recipe";
                dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                    string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                    string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                    string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명
                    string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                    DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
                    if (!dir.Exists)
                        dir.Create();

                    RecipeEBR recipe = GlobalObjects.Instance.Get<RecipeEBR>();
                    recipe.Clear();

                    recipe.Name = sFileNameNoExt;
                    recipe.RecipePath = sFullPath;
                    recipe.RecipeFolderPath = sRecipeFolderPath;

                    recipe.Save(sFullPath);
                }
            });
        }

        public ICommand btnSaveRecipeEBR
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeEBR recipe = GlobalObjects.Instance.Get<RecipeEBR>();
                    if (recipe.RecipePath != "")
                    {
                        recipe.Save(recipe.RecipePath);
                    }
                    else
                    {
                        System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                        dlg.InitialDirectory = Constants.RootPath.RecipeEBRRootPath;
                        dlg.Title = "Save Recipe";
                        dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                            string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                            string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                            string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명
                            string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                            DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
                            if (!dir.Exists)
                                dir.Create();

                            recipe.Name = sFileNameNoExt;
                            recipe.RecipePath = sFullPath;
                            recipe.RecipeFolderPath = sRecipeFolderPath;

                            recipe.Save(sFullPath);
                        }
                    }
                });
            }
        }

        public ICommand btnLoadRecipeEBR
        {
            get
            {
                return new RelayCommand(() =>
                {
                    System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                    dlg.InitialDirectory = Constants.RootPath.RecipeEBRRootPath;
                    dlg.Title = "Load Recipe";
                    dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                        string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                        string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                        string sFullPath = Path.Combine(sFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                        DirectoryInfo dir = new DirectoryInfo(sFolderPath);
                        if (!dir.Exists)
                            dir.Create();

                        RecipeEBR recipe = GlobalObjects.Instance.Get<RecipeEBR>();
                        recipe.Read(sFullPath);

                        ebrSetupVM.LoadParameter();
                    }
                });
            }
        }
        #endregion

        #region [Command Back]

        public ICommand btnBackProduct
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(backsideProduct);
                    backsideProduct.DataContext = backsideProductVM;
                });
            }
        }
        public ICommand btnBackSetup
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(backsideSetup);
                    backsideSetup.DataContext = backsideSetupVM;
                });
            }
        }

        public ICommand btnBackSpec
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(backsideSpec);
                    backsideSpec.DataContext = backsideSpecVM;
                });
            }
        }

        public ICommand btnBackInspect
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(backsideInspect);
                    backsideInspect.DataContext = backsideInspectVM;
                });
            }
        }

        public ICommand btnNewRecipeBack
        {
            get => new RelayCommand(() =>
            {
                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.InitialDirectory = Constants.RootPath.RecipeBackRootPath;
                dlg.Title = "Save Recipe";
                dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                    string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                    string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                    string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명
                    string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                    DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
                    if (!dir.Exists)
                        dir.Create();

                    RecipeBack recipe = GlobalObjects.Instance.Get<RecipeBack>();
                    recipe.Clear();

                    recipe.Name = sFileNameNoExt;
                    recipe.RecipePath = sFullPath;
                    recipe.RecipeFolderPath = sRecipeFolderPath;

                    recipe.Save(sFullPath);
                }
            });
        }

        public ICommand btnSaveRecipeBack
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeBack recipe = GlobalObjects.Instance.Get<RecipeBack>();
                    if (recipe.RecipePath != "")
                    {
                        recipe.Save(recipe.RecipePath);
                    }
                    else
                    {
                        System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                        dlg.InitialDirectory = Constants.RootPath.RecipeBackRootPath;
                        dlg.Title = "Save Recipe";
                        dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                            string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                            string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                            string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명
                            string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                            DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
                            if (!dir.Exists)
                                dir.Create();

                            recipe.Name = sFileNameNoExt;
                            recipe.RecipePath = sFullPath;
                            recipe.RecipeFolderPath = sRecipeFolderPath;

                            recipe.Save(sFullPath);
                        }
                    }
                });
            }
        }

        public ICommand btnLoadRecipeBack
        {
            get
            {
                return new RelayCommand(() =>
                {
                    System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                    dlg.InitialDirectory = Constants.RootPath.RecipeBackRootPath;
                    dlg.Title = "Load Recipe";
                    dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                        string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                        string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                        string sFullPath = Path.Combine(sFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                        DirectoryInfo dir = new DirectoryInfo(sFolderPath);
                        if (!dir.Exists)
                            dir.Create();

                        RecipeBack recipe = GlobalObjects.Instance.Get<RecipeBack>();
                        recipe.Read(sFullPath);

                        UpdateCurrentPanel();
                        WIND2EventManager.OnRecipeUpdated(this, new RecipeEventArgs());
                    }
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

        public ICommand btnCameraAlign
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(cameraAlign);
                    cameraAlign.DataContext = CameraAlignVM;
                });
            }
        }

        public ICommand btnNewRecipeAlign
        {
            get => new RelayCommand(() =>
            {
                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.InitialDirectory = Constants.RootPath.RecipeAlignRootPath;
                dlg.Title = "Save Recipe";
                dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                    string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                    string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                    string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명
                    string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                    DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
                    if (!dir.Exists)
                        dir.Create();

                    RecipeAlign recipe = GlobalObjects.Instance.Get<RecipeAlign>();
                    recipe.Clear();

                    recipe.Name = sFileNameNoExt;
                    recipe.RecipePath = sFullPath;
                    recipe.RecipeFolderPath = sRecipeFolderPath;

                    recipe.Save(sFullPath);
                }
            });
        }

        public ICommand btnSaveRecipeAlign
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeAlign recipe = GlobalObjects.Instance.Get<RecipeAlign>();
                    if (recipe.RecipePath != "")
                    {
                        recipe.Save(recipe.RecipePath);
                    }
                    else
                    {
                        System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                        dlg.InitialDirectory = Constants.RootPath.RecipeAlignRootPath;
                        dlg.Title = "Save Recipe";
                        dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                            string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                            string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                            string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명
                            string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                            DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
                            if (!dir.Exists)
                                dir.Create();

                            recipe.Name = sFileNameNoExt;
                            recipe.RecipePath = sFullPath;
                            recipe.RecipeFolderPath = sRecipeFolderPath;

                            recipe.Save(sFullPath);
                        }
                    }
                });
            }
        }

        public ICommand btnLoadRecipeAlign
        {
            get
            {
                return new RelayCommand(() =>
                {
                    System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                    dlg.InitialDirectory = Constants.RootPath.RecipeAlignRootPath;
                    dlg.Title = "Load Recipe";
                    dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                        string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                        string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                        string sFullPath = Path.Combine(sFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                        DirectoryInfo dir = new DirectoryInfo(sFolderPath);
                        if (!dir.Exists)
                            dir.Create();

                        RecipeAlign recipe = GlobalObjects.Instance.Get<RecipeAlign>();
                        recipe.Read(sFullPath);

                        UpdateCurrentPanel();
                        WIND2EventManager.OnRecipeUpdated(this, new RecipeEventArgs());
                    }
                });
            }
        }
        #endregion

        #endregion

        public void SetPage(UserControl page)
        {
            p_CurrentPanel = page;
        }

        public void UpdateCurrentPanel()
        {
            if (p_CurrentPanel == null) return;

            if(p_CurrentPanel.DataContext is IPage)
                ((IPage)p_CurrentPanel.DataContext).LoadRecipe();
        }
    }
}
