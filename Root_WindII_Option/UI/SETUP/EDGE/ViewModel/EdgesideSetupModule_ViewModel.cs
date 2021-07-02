using Root_WindII_Option.Engineer;
using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Root_WindII_Option.UI
{
	public class EdgesideSetupModule_ViewModel : ObservableObject
	{
		#region [Getter / Setter]
		private OriginRecipe originRecipe;
		public OriginRecipe OriginRecipe
		{
			get => originRecipe;
			set => SetProperty(ref originRecipe, value);
		}

		private EdgeSurfaceRecipeBase recipe;
		public EdgeSurfaceRecipeBase Recipe
		{
			get => recipe;
			set => SetProperty(ref recipe, value);
		}

		private EdgeSurfaceParameterBase parameter;
		public EdgeSurfaceParameterBase Parameter
		{
			get
			{
				return parameter;
			}
			set
			{
				SetProperty(ref parameter, value);
			}
		}

		private ProcessDefectEdgeParameter processDefectParameter;
		public ProcessDefectEdgeParameter ProcessDefectParameter
		{
			get
			{
				return processDefectParameter;
			}
			set
			{
				SetProperty(ref processDefectParameter, value);
			}
		}

		#region [Grab Mode]
		public List<string> GrabModeList
		{
			get
			{
				return ((WindII_Option_Handler)GlobalObjects.Instance.Get<WindII_Option_Engineer>().ClassHandler()).p_VisionEdge.p_asGrabMode;
			}
		}

		private int selectedGrabModeIndex = 0;
		public int SelectedGrabModeIndex
		{
			get => this.selectedGrabModeIndex;
			set
			{
				// new - CamInfo
				WindII_Option_Engineer engineer = GlobalObjects.Instance.Get<WindII_Option_Engineer>();
				RecipeEdge recipeEdge = GlobalObjects.Instance.Get<RecipeEdge>();
				recipeEdge.CameraInfoIndex = value;
				Recipe.GrabModeIndex = value;

				CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(engineer.m_handler.p_VisionEdge.GetGrabMode(Recipe.GrabModeIndex));
				this.CamInfoDataListVM.Init(camInfo);

				//CalculateInspectionROI();
				SetProperty<int>(ref this.selectedGrabModeIndex, value);
			}
		}

		private DataListView_ViewModel camInfoDataListVM;
		public DataListView_ViewModel CamInfoDataListVM
		{
			get => this.camInfoDataListVM;
			set
			{
				SetProperty(ref this.camInfoDataListVM, value);
			}
		}
		#endregion

		#region [Origin Information]
		private int originX;
		public int OriginX
		{
			get => originX;
			set
			{
				OriginRecipe.OriginX = value;
				SetProperty(ref originX, value);
			}
		}

		private int originY;
		public int OriginY
		{
			get => originY;
			set
			{
				OriginRecipe.OriginY = value;
				SetProperty(ref originY, value);
			}
		}

		private int originWidth;
		public int OriginWidth
		{
			get => originWidth;
			set
			{
				OriginRecipe.OriginWidth = value;
				OriginRecipe.DiePitchX = value;
				SetProperty(ref originWidth, value);
			}
		}

		private int originHeight;
		public int OriginHeight
		{
			get => originHeight;
			set
			{
				OriginRecipe.OriginHeight = value;
				OriginRecipe.DiePitchY = value;
				SetProperty(ref originHeight, value);
			}
		}
		#endregion
		#endregion

		public EdgesideSetupModule_ViewModel()
		{
			RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
			OriginRecipe = recipe.GetItem<OriginRecipe>();
			Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop;
			Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop;
			ProcessDefectParameter = recipe.GetItem<ProcessDefectEdgeParameter>();

			this.camInfoDataListVM = new DataListView_ViewModel();

			SetOriginInfo();
		}

		/// <summary>
		/// 전체 이미지를 1chip으로 계산.
		/// OriginY는 이미지의 전체 Height를 말함.
		/// </summary>
		private void SetOriginInfo()
		{
			ObservableCollection<GrabModeEdge> modeList = ((WindII_Option_Handler)GlobalObjects.Instance.Get<WindII_Option_Engineer>().ClassHandler()).p_VisionEdge.m_aGrabMode;
			if (modeList.Count == 0)
				return;

			GrabModeEdge mode = modeList[Recipe.GrabModeIndex];

			int imageHeight = mode.m_nImageHeight;  // 전체 이미지 Height
			OriginRecipe.OriginX = 0;
			OriginRecipe.OriginY = imageHeight;
			OriginRecipe.OriginHeight = imageHeight;
			OriginRecipe.DiePitchY = imageHeight;
		}

		private void CalculateInspectionROI()
		{
			GrabModeEdge mode = ((WindII_Option_Handler)GlobalObjects.Instance.Get<WindII_Option_Engineer>().ClassHandler()).p_VisionEdge.m_aGrabMode[Recipe.GrabModeIndex];
			
			int imageHeight = mode.m_nImageHeight;							// 전체 Image Height
			int heightPerDegree = (int)(imageHeight / mode.m_nScanDegree);  // 1도 Image Height
			int positionOffset = mode.m_nCameraPositionOffset;				// 카메라 위치 Offset

			int startPosition = mode.m_nCameraHeight + (heightPerDegree * positionOffset);	// 검사 시작 위치
			int endPosition = startPosition + (heightPerDegree * 360);                      // 검사 종료 위치

			Parameter.StartPosition = startPosition;
			Parameter.EndPosition = endPosition;
		}

		public void SetRecipeParameter(EdgeSurfaceRecipeBase recipe, EdgeSurfaceParameterBase param)
		{
			this.Recipe = recipe;
			this.Parameter = param;
			this.SelectedGrabModeIndex = recipe.GrabModeIndex;

			this.OriginX = OriginRecipe.OriginX;
			this.OriginY = OriginRecipe.OriginY;
			this.OriginWidth = OriginRecipe.OriginWidth;
			this.OriginHeight = OriginRecipe.OriginHeight;
		}
	}
}
