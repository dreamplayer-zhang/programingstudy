using Root_WindII_Option.UI;
using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WindII_Option
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
		#endregion

		#region [Views]
		// HOME
		//public readonly HomeRecipe homeRecipe = new HomeRecipe();

		//// FRONT
		//public readonly FrontsideSummary frontsideSummary = new FrontsideSummary();
		//public readonly FrontsideProduct frontsideProduct = new FrontsideProduct();
		//public readonly FrontsideOrigin frontsideOrigin = new FrontsideOrigin();
		//public readonly FrontsideAlignment frontsideAlignment = new FrontsideAlignment();
		//public readonly FrontsideMask frontsideMask = new FrontsideMask();
		//public readonly FrontsideSpec frontsideSpec = new FrontsideSpec();
		//public readonly FrontsideInspect frontsideInspect = new FrontsideInspect();

		// BACK
		public readonly BacksideProduct backsideProduct = new BacksideProduct();
		public readonly BacksideSetup backsideSetup = new BacksideSetup();
		public readonly BacksideInspect backsideInspect = new BacksideInspect();
		public readonly BacksideSpec backsideSpec = new BacksideSpec();

		// EDGE
		public readonly EdgesideSetup edgesideSetup = new EdgesideSetup();
		public readonly EdgesideInspect edgesideInspect = new EdgesideInspect();

		// EBR
		public readonly EBRSetup ebrSetup = new EBRSetup();
		public readonly EBRInspect ebrInspect = new EBRInspect();


		//// Camera
		//public readonly CameraVRS cameraVrs = new CameraVRS();
		//public readonly CameraAlign cameraAlign = new CameraAlign();
		#endregion

		//#region [ViewModels]

		//#region [Home ViewModels]
		//private HomeRecipe_ViewModel homeRecipeVM = new HomeRecipe_ViewModel();
		//public HomeRecipe_ViewModel HomeRecipeVM
		//{
		//    get => homeRecipeVM;
		//}
		//#endregion

		//#region [Front ViewModels]
		//private .FrontsideProduct_ViewModel frontsideSummaryVM = new FrontsideProduct_ViewModel();
		//public .FrontsideProduct_ViewModel FrontsideSummaryVM
		//{
		//    get => frontsideSummaryVM;
		//}

		//private .FrontsideProduct_ViewModel frontsideProductVM = new FrontsideProduct_ViewModel();
		//public .FrontsideProduct_ViewModel FrontsideProductVM
		//{
		//    get => frontsideProductVM;
		//}

		//private .FrontsideOrigin_ViewModel frontsideOriginVM = new .FrontsideOrigin_ViewModel();
		//public .FrontsideOrigin_ViewModel FrontsideOriginVM
		//{
		//    get => frontsideOriginVM;
		//}

		//private .FrontsideAlignment_ViewModel frontsideAlignmentVM = new .FrontsideAlignment_ViewModel();
		//public .FrontsideAlignment_ViewModel FrontsideAlignmentVM
		//{
		//    get => frontsideAlignmentVM;
		//}

		//private .FrontsideMask_ViewModel frontsideMaskVM = new .FrontsideMask_ViewModel();
		//public .FrontsideMask_ViewModel FrontsideMaskVM
		//{
		//    get => frontsideMaskVM;
		//}

		//private .FrontsideSpec_ViewModel frontsideSpecVM = new .FrontsideSpec_ViewModel();
		//public .FrontsideSpec_ViewModel FrontsideSpecVM
		//{
		//    get => frontsideSpecVM;
		//}

		//private .FrontsideInspect_ViewModel frontsideInspectVM = new .FrontsideInspect_ViewModel();
		//public .FrontsideInspect_ViewModel FrontsideInspectVM
		//{
		//    get => frontsideInspectVM;
		//}

		//#region [Camera ViewModes]
		//private .CameraVRS_ImageViewer_ViewModel cameraVrsVM = new .CameraVRS_ImageViewer_ViewModel();
		//public .CameraVRS_ImageViewer_ViewModel CameraVrsVM
		//{
		//    get => cameraVrsVM;
		//}

		//#endregion


		//#endregion

		#region [Back ViewModels]
		private BacksideProduct_ViewModel backsideProductVM = new BacksideProduct_ViewModel();
		public BacksideProduct_ViewModel BacksideProductVM
		{
			get => this.backsideProductVM;
		}

		private BacksideSetup_ViewModel backsideSetupVM = new BacksideSetup_ViewModel();
		public BacksideSetup_ViewModel BacksideROIVM
		{
			get => this.backsideSetupVM;
		}

		private BacksideSpec_ViewModel backsideSpecVM = new BacksideSpec_ViewModel();
		public BacksideSpec_ViewModel BacksideSpecVM
		{
			get => this.backsideSpecVM;
		}

		private BacksideInspect_ViewModel backsideInspectVM = new BacksideInspect_ViewModel();
		public BacksideInspect_ViewModel BacksideInspectVM
		{
			get => this.backsideInspectVM;
		}
		#endregion

		#region [Edge ViewModels]
		private EdgesideSetup_ViewModel edgesideSetupVM = new EdgesideSetup_ViewModel();
		public EdgesideSetup_ViewModel EdgesideSetupVM
		{
			get => edgesideSetupVM;
		}
		private EdgesideInspect_ViewModel edgesideInspectionVM = new EdgesideInspect_ViewModel();
		public EdgesideInspect_ViewModel EdgesideInspectionVM
		{
			get => edgesideInspectionVM;
		}
		#endregion

		#region [EBR ViewModels]
		private EBRSetup_ViewModel ebrSetupVM = new EBRSetup_ViewModel();
		public EBRSetup_ViewModel EBRSetupVM
		{
			get => ebrSetupVM;
		}

		private EBRInspect_ViewModel ebrInspectVM = new EBRInspect_ViewModel();
		public EBRInspect_ViewModel EBRInspectVM
		{
			get => ebrInspectVM;
		}
		#endregion

		//#region [Camera ViewModels]
		//private .CameraAlign_ViewModel cameraAlignVM = new .CameraAlign_ViewModel();
		//public .CameraAlign_ViewModel CameraAlignVM
		//{
		//	get => cameraAlignVM;
		//}
		//#endregion

		//#endregion


		public SetupMenuPanel_ViewModel()
        {
            Initialize();

            //frontsideOrigin.DataContext = frontsideOriginVM;

            //WIND2EventManager.RecipeUpdated += RecipeUpdated_Callback;
        }

        public void Initialize()
        {
            SetButtonEnable();
        }

        //private void RecipeUpdated_Callback(object obj, RecipeEventArgs args)
        //{
        //    SetButtonEnable();
        //}

        public void SetButtonEnable()
        {
            //OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            //if (originRecipe.OriginWidth == 0 || originRecipe.OriginHeight == 0)
            //{
            //    this.IsEnabledAlignment = false;
            //}
            //else
            //{
            //    this.IsEnabledAlignment = true;
            //}

            //if (originRecipe.OriginWidth == 0 || originRecipe.OriginHeight == 0)
            //{
            //    this.IsEnabledMask = false;
            //}
            //else
            //{
            //    this.IsEnabledMask = true;
            //}

            //// Mask는 Default로 전체 영역 추가
            //if (originRecipe.OriginWidth == 0 || originRecipe.OriginHeight == 0)
            //{
            //    this.IsEnabledSpec = false;
            //}
            //else
            //{
            //    this.IsEnabledSpec = true;
            //}
        }


		#region [Command]

		//#region [Command Parent RadioButton]
		//public ICommand btnHomeClickedCommand
		//{
		//    get
		//    {
		//        return new RelayCommand(() =>
		//        {
		//            SetPage(homeRecipe);
		//            //homeRecipe.DataContext = homeRecipeVM;
		//        });
		//    }
		//}
		//public ICommand btnFrontClickedCommand
		//{
		//    get
		//    {
		//        return new RelayCommand(() =>
		//        {
		//            SetPage(frontsideProduct);
		//            //frontsideProduct.DataContext = frontsideProductVM;
		//        });
		//    }
		//}

		//public ICommand btnBackClickedCommand
		//{
		//    get
		//    {
		//        return new RelayCommand(() =>
		//        {
		//            SetPage(backsideSetup);
		//            //backsideSetup.DataContext = backsideSetupVM;
		//        });
		//    }
		//}

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

        //public ICommand btnCameraClickedCommand
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            SetPage(cameraVrs);
        //            //cameraVrs.DataContext = cameraVrsVM;
        //        });
        //    }
        //}

        //#endregion

        //#region [Command Home]
        //public ICommand btnHomeRecipe
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            SetPage(homeRecipe);
        //            //homeRecipe.DataContext = homeRecipeVM;
        //        });
        //    }
        //}
        //#endregion

        //#region [Command Front]
        //public ICommand btnFrontSummary
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            SetPage(frontsideSummary);
        //            //frontsideSummary.DataContext = frontsideSummaryVM;
        //        });
        //    }
        //}

        //public ICommand btnFrontProduct
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            SetPage(frontsideProduct);
        //            //frontsideProduct.DataContext = frontsideProductVM;
        //        });
        //    }
        //}

        //public ICommand btnFrontOrigin
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            SetPage(frontsideOrigin);
        //            //frontsideOrigin.DataContext = frontsideOriginVM;
        //        });
        //    }
        //}

        //public ICommand btnFrontAlignment
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            SetPage(frontsideAlignment);
        //            //frontsideAlignment.DataContext = frontsideAlignmentVM;
        //        });
        //    }
        //}

        //public ICommand btnFrontMask
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            SetPage(frontsideMask);
        //            //frontsideMask.DataContext = frontsideMaskVM;
        //        });
        //    }
        //}

        //public ICommand btnFrontSpec
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            SetPage(frontsideSpec);
        //            //frontsideSpec.DataContext = frontsideSpecVM;
        //        });
        //    }
        //}

        //public ICommand btnFrontInspect
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            SetPage(frontsideInspect);
        //            //frontsideInspect.DataContext = frontsideInspectVM;
        //        });
        //    }
        //}

        //public ICommand btnNewRecipeFront
        //{
        //    get => new RelayCommand(() =>
        //    {
        //        //System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
        //        //dlg.InitialDirectory = Constants.RootPath.RecipeFrontRootPath;
        //        //dlg.Title = "Save Recipe";
        //        //dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
        //        //if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //        //{
        //        //    string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
        //        //    string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
        //        //    string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
        //        //    string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명
        //        //    string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

        //        //    DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
        //        //    if (!dir.Exists)
        //        //        dir.Create();

        //        //    RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
        //        //    recipe.Clear();

        //        //    recipe.Name = sFileNameNoExt;
        //        //    recipe.RecipePath = sFullPath;
        //        //    recipe.RecipeFolderPath = sRecipeFolderPath;

        //        //    recipe.Save(sFullPath);
        //        //}
        //    });
        //}

        //public ICommand btnSaveRecipeFront
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            //RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
        //            //if (recipe.RecipePath != "")
        //            //{
        //            //    recipe.Save(recipe.RecipePath);
        //            //}
        //            //else
        //            //{
        //            //    System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
        //            //    dlg.InitialDirectory = Constants.RootPath.RecipeFrontRootPath;
        //            //    dlg.Title = "Save Recipe";
        //            //    dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
        //            //    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //            //    {
        //            //        string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
        //            //        string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
        //            //        string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
        //            //        string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명
        //            //        string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

        //            //        DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
        //            //        if (!dir.Exists)
        //            //            dir.Create();

        //            //        recipe.Name = sFileNameNoExt;
        //            //        recipe.RecipePath = sFullPath;
        //            //        recipe.RecipeFolderPath = sRecipeFolderPath;

        //            //        recipe.Save(sFullPath);
        //            //    }
        //            //}
        //        });
        //    }
        //}

        //public ICommand btnLoadRecipeFront
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {

        //        });
        //    }
        //}
        //#endregion

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
			get => new RelayCommand(() =>
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

		public ICommand btnLoadRecipeEBR
		{
			get => new RelayCommand(() =>
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
						//WIND2EventManager.OnRecipeUpdated(this, new RecipeEventArgs());
					}
				});
			}
		}
		#endregion

		//#region [Command Camera]
		//public ICommand btnCameraVRS
		//{
		//    get
		//    {
		//        return new RelayCommand(() =>
		//        {
		//            SetPage(cameraVrs);
		//            //cameraVrs.DataContext = CameraVrsVM;
		//        });
		//    }
		//}

		//public ICommand btnCameraAlign
		//{
		//    get
		//    {
		//        return new RelayCommand(() =>
		//        {
		//            SetPage(cameraAlign);
		//            //cameraAlign.DataContext = CameraAlignVM;
		//        });
		//    }
		//}
		//#endregion

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
