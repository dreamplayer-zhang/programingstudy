using Root_WIND2.Module;
using RootTools;
using RootTools.Module;
using RootTools_Vision;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Root_WIND2.UI_User
{
	class EdgesideSetup_ViewModel : ObservableObject
	{
		private Edgeside_ImageViewer_ViewModel drawToolVM;
		private EdgeSurfaceRecipeBase recipe;
		private EdgeSurfaceParameterBase parameter;
		private int selectedGrabModeIndex = 0;

		private bool _IsTopChecked = true;
		private bool _IsSideChecked = false;
		private bool _IsBtmChecked = false;

		#region [Getter / Setter]
		public Edgeside_ImageViewer_ViewModel DrawToolVM
		{
			get => drawToolVM;
			set => SetProperty(ref drawToolVM, value);
		}

		public EdgeSurfaceRecipeBase Recipe
		{
			get => recipe;
			set => SetProperty(ref recipe, value);
		}

		public EdgeSurfaceParameterBase Parameter
		{
			get => parameter;
			set => SetProperty(ref parameter, value);
		}

		public int TopPositionOffset
		{
			get
			{
				EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
				Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");

				return inspect.TopPositionOffset;
			}
			set
			{

			}
		}

		public int SidePositionOffset
		{
			get
			{
				EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
				Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");

				return inspect.SidePositionOffset;
			}
			set
			{

			}
		}

		public int BtmPositionOffset
		{
			get
			{
				EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
				Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");
				
				return inspect.BtmPositionOffset;
			}
			set
			{

			}
		}

		public int TopImageOffset
		{
			get
			{
				EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
				Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");

				return inspect.TopImageOffset;
			}
			set
			{

			}
		}

		public int SideImageOffset
		{
			get
			{
				EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
				Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");

				return inspect.SideImageOffset;
			}
			set
			{

			}
		}

		public int BtmImageOffset
		{
			get
			{
				EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
				Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");

				return inspect.BtmImageOffset;
			}
			set
			{

			}
		}

		public List<string> GrabModeList
		{
			get
			{
				return ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.p_asGrabMode;
			}
		}

		public int SelectedGrabModeIndex
		{
			get => this.selectedGrabModeIndex;
			set
			{
				GrabMode mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[value];
				Run_InspectEdge inspect = ((Run_InspectEdge)((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.CloneModuleRun("InspectEdge"));

				Recipe.GrabModeName = mode.p_sName;
				if (mode.m_camera != null)
				{
					Recipe.CameraWidth = mode.m_camera.GetRoiSize().X;
					Recipe.CameraHeight = mode.m_camera.GetRoiSize().Y;
				}

				if (Recipe.CameraHeight == 0)
				{
					if (_IsTopChecked)
						Recipe.CameraHeight = inspect.TopCameraHeight;
					else if (_IsSideChecked)
						Recipe.CameraHeight = inspect.SideCameraHeight;
					else if (_IsBtmChecked)
						Recipe.CameraHeight = inspect.BtmCameraHeight;
				}

				Recipe.TriggerRatio = mode.m_dCamTriggerRatio;
				Parameter.CamResolution = mode.m_dResX_um;

				SetProperty<int>(ref this.selectedGrabModeIndex, value);
			}
		}

		public bool IsTopChecked
		{
			get => _IsTopChecked;
			set
			{
				SetProperty(ref _IsTopChecked, value);
				if (_IsTopChecked)
				{
					IsSideChecked = false;
					IsBtmChecked = false;
				}
			}
		}

		public bool IsSideChecked
		{
			get => _IsSideChecked;
			set
			{
				SetProperty(ref _IsSideChecked, value);
				if (_IsSideChecked)
				{
					IsTopChecked = false;
					IsBtmChecked = false;
				}
			}
		}

		public bool IsBtmChecked
		{
			get => _IsBtmChecked;
			set
			{
				SetProperty(ref _IsBtmChecked, value);
				if (_IsBtmChecked)
				{
					IsTopChecked = false;
					IsSideChecked = false;
				}
			}
		}
		#endregion

		#region [Command]
		public ICommand btnTop
		{
			get
			{
				return new RelayCommand(() => 
				{ 
					ChangeViewer("Top");
				});  
			}
		}

		public ICommand btnSide
		{
			get
			{
				return new RelayCommand(() =>
				{
					ChangeViewer("Side");
				});
			}
		}

		public ICommand btnBottom
		{
			get
			{
				return new RelayCommand(() =>
				{
					ChangeViewer("Bottom");
				});
			}
		}
		#endregion

		public EdgesideSetup_ViewModel()
		{
			DrawToolVM = new Edgeside_ImageViewer_ViewModel();
			DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeTopImage"), GlobalObjects.Instance.Get<DialogService>());

			RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
			Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop;
			Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop;
		}

		private void ChangeViewer(string dataName)
		{
			RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();

			if (dataName == "Top")
			{
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeTopImage"), GlobalObjects.Instance.Get<DialogService>());
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop;
				Recipe.PositionOffset = this.TopPositionOffset;
			}
			else if (dataName == "Side")
			{
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeSideImage"), GlobalObjects.Instance.Get<DialogService>());
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseSide;
				Recipe.PositionOffset = this.SidePositionOffset;
			}
			else if (dataName == "Bottom")
			{
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeBottomImage"), GlobalObjects.Instance.Get<DialogService>());
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseBtm;
				Recipe.PositionOffset = this.BtmPositionOffset;
			}
			else
				return;
        }

		public void LoadParameter()
		{
			RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
			if (recipe.GetItem<EdgeSurfaceParameter>() == null)
				return;

			if (IsTopChecked)
			{
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop;
			}
			else if (IsSideChecked)
			{
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseSide;
			}
			else if (IsBtmChecked)
			{
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseBtm;
			}
		}
	}
}
