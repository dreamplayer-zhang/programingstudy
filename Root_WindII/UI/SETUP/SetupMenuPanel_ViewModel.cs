using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WindII
{
    public class SetupMenuPanel_ViewModel : ObservableObject
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

        private bool isEnableMeasurement = false;
        public bool IsEnabledMeasurement
        {
            get => this.isEnableMeasurement;
            set
            {
                SetProperty<bool>(ref this.isEnableMeasurement, value);
            }
        }
        #endregion

        #region [Views]
        // HOME
        public readonly HomeRecipe homeRecipe = new HomeRecipe();

        // FRONT
        public readonly FrontsideSummary frontsideSummary = new FrontsideSummary();
        public readonly FrontsideProduct frontsideProduct = new FrontsideProduct();
        public readonly FrontsideOrigin frontsideOrigin = new FrontsideOrigin();
        public readonly FrontsideAlignment frontsideAlignment = new FrontsideAlignment();
        public readonly FrontsideMask frontsideMask = new FrontsideMask();
        public readonly FrontsideSpec frontsideSpec = new FrontsideSpec();
        public readonly FrontsideInspect frontsideInspect = new FrontsideInspect();
        public readonly FrontsideMeasurement frontsideMeasurement = new FrontsideMeasurement();

        // BACK
        public readonly BacksideSetup backsideSetup = new BacksideSetup();
        public readonly BacksideInspect backsideInspect = new BacksideInspect();

        // EDGE
        public readonly EdgesideSetup edgesideSetup = new EdgesideSetup();
        public readonly EdgesideInspect edgesideInspect = new EdgesideInspect();

        // EBR
        public readonly EBRSetup ebrSetup = new EBRSetup();

        // Camera
        public readonly CameraVRS cameraVrs = new CameraVRS();
        public readonly CameraAlign cameraAlign = new CameraAlign();
        public readonly CameraRADS cameraRads = new CameraRADS();

        // RAC
        public readonly RACProduct racProduct = new RACProduct();
        public readonly RACSetup racSetup = new RACSetup();
        public readonly RACAlignKey racAlignKey = new RACAlignKey();
        public readonly RACCreate racCreate = new RACCreate();
        #endregion

        //#region [ViewModels]

        //#region [Home ViewModels]
        //private HomeRecipe_ViewModel homeRecipeVM = new HomeRecipe_ViewModel();
        //public HomeRecipe_ViewModel HomeRecipeVM
        //{
        //    get => homeRecipeVM;
        //}
        //#endregion

        #region [Front ViewModels]
        private FrontsideSummary_ViewModel frontsideSummaryVM = new FrontsideSummary_ViewModel();
        public FrontsideSummary_ViewModel FrontsideSummaryVM
        {
            get => frontsideSummaryVM;
        }

        private FrontsideProduct_ViewModel frontsideProductVM = new FrontsideProduct_ViewModel();
        public FrontsideProduct_ViewModel FrontsideProductVM
        {
            get => frontsideProductVM;
        }

        private FrontsideOrigin_ViewModel frontsideOriginVM = new FrontsideOrigin_ViewModel();
        public FrontsideOrigin_ViewModel FrontsideOriginVM
        {
            get => frontsideOriginVM;
        }

        private FrontsideAlignment_ViewModel frontsideAlignmentVM = new FrontsideAlignment_ViewModel();
        public FrontsideAlignment_ViewModel FrontsideAlignmentVM
        {
            get => frontsideAlignmentVM;
        }

        private FrontsideMask_ViewModel frontsideMaskVM = new FrontsideMask_ViewModel();
        public FrontsideMask_ViewModel FrontsideMaskVM
        {
            get => frontsideMaskVM;
        }

        private FrontsideSpec_ViewModel frontsideSpecVM = new FrontsideSpec_ViewModel();
        public FrontsideSpec_ViewModel FrontsideSpecVM
        {
            get => frontsideSpecVM;
        }

        private FrontsideInspect_ViewModel frontsideInspectVM = new FrontsideInspect_ViewModel();
        public FrontsideInspect_ViewModel FrontsideInspectVM
        {
            get => frontsideInspectVM;
        }

        private FrontsideMeasurement_ViewModel frontsideMeasurementVM = new FrontsideMeasurement_ViewModel();
        public FrontsideMeasurement_ViewModel FrontsideMeasurementVM
        {
            get => frontsideMeasurementVM;
        }
        #endregion

        #region [Camera ViewModes]
        private CameraVRS_ViewModel cameraVrsVM = new CameraVRS_ViewModel();
        public CameraVRS_ViewModel CameraVrsVM
        {
            get => cameraVrsVM;
        }

        //#region [Back ViewModels]
        //private .BacksideSetup_ViewModel backsideSetupVM = new .BacksideSetup_ViewModel();
        //public .BacksideSetup_ViewModel BacksideROIVM
        //{
        //    get => this.backsideSetupVM;
        //}

        //private .BacksideInspect_ViewModel backsideInspectVM = new .BacksideInspect_ViewModel();
        //public .BacksideInspect_ViewModel BacksideInspectVM
        //{
        //    get => this.backsideInspectVM;
        //}
        //#endregion

        //#region [Edge ViewModels]
        //private .EdgesideSetup_ViewModel edgesideSetupVM = new .EdgesideSetup_ViewModel();
        //public .EdgesideSetup_ViewModel EdgesideSetupVM
        //{
        //    get => edgesideSetupVM;
        //}
        //private .EdgesideInspect_ViewModel edgesideInspectionVM = new .EdgesideInspect_ViewModel();
        //public .EdgesideInspect_ViewModel EdgesideInspectionVM
        //{
        //    get => edgesideInspectionVM;
        //}
        //#endregion

        //#region [EBR ViewModels]
        //private .EBRSetup_ViewModel ebrSetupVM = new .EBRSetup_ViewModel();
        //public .EBRSetup_ViewModel EBRSetupVM
        //{
        //    get => ebrSetupVM;
        //}
        //#endregion

        private CameraAlign_ViewModel cameraAlignVM = new CameraAlign_ViewModel();
        public CameraAlign_ViewModel CameraAlignVM
        {
            get => cameraAlignVM;
        }

        private CameraRADS_ViewModel cameraRadsVM = new CameraRADS_ViewModel();
        public CameraRADS_ViewModel CameraRadsVM
        {
            get => cameraRadsVM;
        }
        #endregion

        #region [RAC ViewModels]
        private RACProduct_ViewModel racProductVM = new RACProduct_ViewModel();
        public RACProduct_ViewModel RACProductVM
        {
            get => racProductVM;
        }
        private RACSetup_ViewModel racSetupVM = new RACSetup_ViewModel();

        public RACSetup_ViewModel RACSetupVM
        {
            get => racSetupVM;
        }

        private RACAlignKey_ViewModel racAlignKeyVM = new RACAlignKey_ViewModel();
        public RACAlignKey_ViewModel RACAlignKeyVM
        {
            get => racAlignKeyVM;
        }

        private RACCreate_ViewModel racCreateVM = new RACCreate_ViewModel();
        public RACCreate_ViewModel RACCreateVM
        {
            get => racCreateVM;
        }
        #endregion

        public SetupMenuPanel_ViewModel()
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

            if (originRecipe.OriginWidth == 0 || originRecipe.OriginHeight == 0)
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
                this.IsEnabledMeasurement = false;
            }
            else
            {
                this.IsEnabledMeasurement = true;
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
                    //homeRecipe.DataContext = homeRecipeVM;
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
                    //backsideSetup.DataContext = backsideSetupVM;
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
                    //edgesideSetup.DataContext = edgesideSetupVM;
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
                    //ebrSetup.DataContext = ebrSetupVM;
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

        public ICommand btnRACClickedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(racProduct);
                    racProduct.DataContext = racProductVM;
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
                    //homeRecipe.DataContext = homeRecipeVM;
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

        public ICommand btnFrontMeasurement
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(frontsideMeasurement);
                    frontsideMeasurement.DataContext = frontsideMeasurementVM;
                });
            }
        }

        public ICommand btnNewRecipeFront
        {
            get
            {
                return new RelayCommand(() =>
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
                    //edgesideSetup.DataContext = edgesideSetupVM;
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
                    //edgesideInspect.DataContext = EdgesideInspectionVM;
                });
            }
        }


        public ICommand btnNewRecipeEdge
        {
            get => new RelayCommand(() =>
            {

            });
        }

        public ICommand btnSaveRecipeEdge
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }

        public ICommand btnLoadRecipeEdge
        {
            get
            {
                return new RelayCommand(() =>
                {

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
                    //ebrSetup.DataContext = ebrSetupVM;
                });
            }
        }

        public ICommand btnNewRecipeEBR
        {
            get => new RelayCommand(() =>
            {

            });
        }

        public ICommand btnSaveRecipeEBR
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }

        public ICommand btnLoadRecipeEBR
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }
        #endregion

        #region [Command Back]
        public ICommand btnBackSetup
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(backsideSetup);
                    //backsideSetup.DataContext = backsideSetupVM;
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
                    //backsideInspect.DataContext = backsideInspectVM;
                });
            }
        }

        public ICommand btnNewRecipeBack
        {
            get => new RelayCommand(() =>
            {

            });
        }

        public ICommand btnSaveRecipeBack
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }

        public ICommand btnLoadRecipeBack
        {
            get
            {
                return new RelayCommand(() =>
                {

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

        public ICommand btnCameraRADS
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(cameraRads);
                    cameraRads.DataContext = CameraRadsVM;
                });
            }
        }
        #endregion

        #region [Command RAC]
        public ICommand btnRACProduct
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(racProduct);
                    racProduct.DataContext = RACProductVM;
                });
            }
        }
        public ICommand btnRACSetup
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(racSetup);
                    racSetup.DataContext = RACSetupVM;
                });
            }
        }

        public ICommand btnRACCreate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(racCreate);
                    racCreate.DataContext = RACCreateVM;
                });
            }
        }

        public ICommand btnRACAlignKey
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(racAlignKey);
                    racAlignKey.DataContext = RACAlignKeyVM;
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

            if (p_CurrentPanel.DataContext is IPage)
                ((IPage)p_CurrentPanel.DataContext).LoadRecipe();
        }
    }
}
