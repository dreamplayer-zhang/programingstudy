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
		// grab mode data
		private int cameraWidth;
		private int cameraHeight;
		private int imageHeight;
		private double resolution;
		private int positionOffset;

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

		public List<string> GrabModeList
		{
			get
			{
				return ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.p_asGrabMode;
			}
		}

		public int CameraWidth
		{
			get => cameraWidth;
			set => SetProperty(ref cameraWidth, value);
		}

		public int CameraHeight
		{
			get => cameraHeight;
			set => SetProperty(ref cameraHeight, value);
		}

		public int ImageHeight
		{
			get => imageHeight;
			set => SetProperty(ref imageHeight, value);
		}

		public double Resolution
		{
			get => resolution;
			set => SetProperty(ref resolution, value);
		}

		public int PositionOffset
		{
			get => positionOffset;
			set => SetProperty(ref positionOffset, value);
		}

		public int SelectedGrabModeIndex
		{
			get => this.selectedGrabModeIndex;
			set
			{
				GrabModeEdge mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[value];

				if (mode.m_camera != null)
				{
					CameraWidth = mode.m_camera.GetRoiSize().X;
					CameraHeight = mode.m_camera.GetRoiSize().Y;
				}
				else
				{
					CameraWidth = 0;
					CameraHeight = mode.m_nCameraHeight;
				}
				ImageHeight = mode.m_nImageHeight;
				Resolution = mode.m_dTargetResX_um;
				PositionOffset = mode.m_nCameraPositionOffset;

				Recipe.GrabModeIndex = value;
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
				this.SelectedGrabModeIndex = Recipe.GrabModeIndex;
			}
			else if (dataName == "Side")
			{
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeSideImage"), GlobalObjects.Instance.Get<DialogService>());
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseSide;
				this.SelectedGrabModeIndex = Recipe.GrabModeIndex;
			}
			else if (dataName == "Bottom")
			{
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeBottomImage"), GlobalObjects.Instance.Get<DialogService>());
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseBtm;
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
