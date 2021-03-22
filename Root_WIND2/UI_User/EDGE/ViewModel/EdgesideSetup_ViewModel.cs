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
		
		// grab mode
		private int selectedGrabModeIndex = 0;
		private int topPositionOffset = 0;
		private int sidePositionOffset = 0;
		private int btmPositionOffset = 0;
		private int topImageOffset = 0;
		private int sideImageOffset = 0;
		private int btmImageOffset = 0;

		private bool isTopChecked = true;
		private bool isSideChecked = false;
		private bool isBtmChecked = false;

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

				topPositionOffset = inspect.TopPositionOffset;
				return topPositionOffset;
			}
			set => SetProperty(ref topPositionOffset, value);
		}

		public int SidePositionOffset
		{
			get
			{
				EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
				Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");

				sidePositionOffset = inspect.SidePositionOffset;
				return sidePositionOffset;
			}
			set => SetProperty(ref sidePositionOffset, value);
		}

		public int BtmPositionOffset
		{
			get
			{
				EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
				Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");

				btmPositionOffset = inspect.BtmPositionOffset;
				return btmPositionOffset;
			}
			set => SetProperty(ref btmPositionOffset, value);
		}

		public int TopImageOffset
		{
			get
			{
				EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
				Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");

				topImageOffset = inspect.TopImageOffset;
				return topImageOffset;
			}
			set => SetProperty(ref topImageOffset, value);
		}

		public int SideImageOffset
		{
			get
			{
				EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
				Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");

				sideImageOffset = inspect.SideImageOffset;
				return sideImageOffset;
			}
			set => SetProperty(ref sideImageOffset, value);
		}

		public int BtmImageOffset
		{
			get
			{
				EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
				Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");

				btmImageOffset = inspect.BtmImageOffset;
				return btmImageOffset;
			}
			set => SetProperty(ref btmImageOffset, value);
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
				//GrabMode mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[value];
				RootTools.GrabModeBase mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[value];
				Run_InspectEdge inspect = ((Run_InspectEdge)((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.CloneModuleRun("InspectEdge"));

				Recipe.GrabModeIndex = value;
				if (mode.m_camera != null)
				{
					Recipe.CameraWidth = mode.m_camera.GetRoiSize().X;
					Recipe.CameraHeight = mode.m_camera.GetRoiSize().Y;
				}

				if (Recipe.CameraHeight == 0)
				{
					if (isTopChecked)
						Recipe.CameraHeight = inspect.TopCameraHeight;
					else if (isSideChecked)
						Recipe.CameraHeight = inspect.SideCameraHeight;
					else if (isBtmChecked)
						Recipe.CameraHeight = inspect.BtmCameraHeight;
				}

				Recipe.Resolution = mode.m_dTargetResX_um;
				Recipe.TriggerRatio = mode.m_dCamTriggerRatio;

				SetProperty<int>(ref this.selectedGrabModeIndex, value);
			}
		}

		public bool IsTopChecked
		{
			get => isTopChecked;
			set
			{
				SetProperty(ref isTopChecked, value);
				if (isTopChecked)
				{
					IsSideChecked = false;
					IsBtmChecked = false;
				}
			}
		}

		public bool IsSideChecked
		{
			get => isSideChecked;
			set
			{
				SetProperty(ref isSideChecked, value);
				if (isSideChecked)
				{
					IsTopChecked = false;
					IsBtmChecked = false;
				}
			}
		}

		public bool IsBtmChecked
		{
			get => isBtmChecked;
			set
			{
				SetProperty(ref isBtmChecked, value);
				if (isBtmChecked)
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
				Recipe.ImageOffset = this.TopImageOffset;
				this.SelectedGrabModeIndex = Recipe.GrabModeIndex;
			}
			else if (dataName == "Side")
			{
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeSideImage"), GlobalObjects.Instance.Get<DialogService>());
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseSide;
				Recipe.PositionOffset = this.SidePositionOffset;
				Recipe.ImageOffset = this.SideImageOffset;
				this.SelectedGrabModeIndex = Recipe.GrabModeIndex;
			}
			else if (dataName == "Bottom")
			{
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeBottomImage"), GlobalObjects.Instance.Get<DialogService>());
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseBtm;
				Recipe.PositionOffset = this.BtmPositionOffset;
				Recipe.ImageOffset = this.BtmImageOffset;
				this.SelectedGrabModeIndex = Recipe.GrabModeIndex;
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
				this.SelectedGrabModeIndex = Recipe.GrabModeIndex;
			}
			else if (IsSideChecked)
			{
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseSide;
				this.SelectedGrabModeIndex = Recipe.GrabModeIndex;
			}
			else if (IsBtmChecked)
			{
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseBtm;
				this.SelectedGrabModeIndex = Recipe.GrabModeIndex;
			}
		}
	}
}
