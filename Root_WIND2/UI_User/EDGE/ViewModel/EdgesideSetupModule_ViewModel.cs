using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Root_WIND2.UI_User
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

		private ObservableCollection<OptionDefect> optionDefectList;
		public ObservableCollection<OptionDefect> OptionDefectList
		{
			get
			{
				return optionDefectList;
			}
			set
			{
				SetParameter();
				SetProperty(ref optionDefectList, value);
			}
		}
				
		#region [Grab Mode]
		public List<string> GrabModeList
		{
			get
			{
				return ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.p_asGrabMode;
			}
		}

		private int selectedGrabModeIndex = 0;
		public int SelectedGrabModeIndex
		{
			get => this.selectedGrabModeIndex;
			set
			{
				// new - CamInfo
				WIND2_Engineer engineer = GlobalObjects.Instance.Get<WIND2_Engineer>();
				RecipeEdge recipeEdge = GlobalObjects.Instance.Get<RecipeEdge>();
				recipeEdge.CameraInfoIndex = value;
				Recipe.GrabModeIndex = value;

				CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(engineer.m_handler.p_EdgeSideVision.GetGrabMode(Recipe.GrabModeIndex));
				this.CamInfoDataListVM.Init(camInfo);

				//CalculateInspectionROI();
				SetProperty<int>(ref this.selectedGrabModeIndex, value);

				// old - GrabMode
				//GrabModeEdge mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[value];
				//if (mode.m_camera != null)
				//{
				//	CameraWidth = mode.m_camera.GetRoiSize().X;
				//	CameraHeight = mode.m_camera.GetRoiSize().Y;
				//}
				//else
				//{
				//	CameraWidth = 0;
				//	CameraHeight = mode.m_nCameraHeight;
				//}

				//ImageHeight = mode.m_nImageHeight;
				//Resolution = mode.m_dTargetResX_um;
				//PositionOffset = mode.m_nCameraPositionOffset;
				//Recipe.GrabModeIndex = value;
				//CalculateInspectionROI();
				//SetProperty<int>(ref this.selectedGrabModeIndex, value);
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
			OptionDefectList = new ObservableCollection<OptionDefect>();

			this.camInfoDataListVM = new DataListView_ViewModel();
			SetOriginInfo();
		}

		/// <summary>
		/// 전체 이미지를 1chip으로 계산.
		/// OriginY는 이미지의 전체 Height를 말함.
		/// </summary>
		private void SetOriginInfo()
		{
			ObservableCollection<GrabModeEdge> modeList = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode;
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
			GrabModeEdge mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[Recipe.GrabModeIndex];
			
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

		public void SetOptionDefect()
		{
			RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
			int count = recipe.GetItem<ProcessDefectEdgeParameter>().Angles.Count;

			if (OptionDefectList == null)
				OptionDefectList = new ObservableCollection<OptionDefect>();

			OptionDefectList.Clear();
			for (int i = 0; i < count; i++)
			{
				OptionDefectList.Add(new OptionDefect(recipe.GetItem<ProcessDefectEdgeParameter>().Angles[i], recipe.GetItem<ProcessDefectEdgeParameter>().DefectCodes[i]));
			}
		}

		public void SetParameter()
		{
			if (OptionDefectList == null)
				return;

			List<double> angleList = new List<double>();
			List<int> defectCodeList = new List<int>();

			foreach (OptionDefect item in OptionDefectList)
			{
				angleList.Add(item.Angle);
				defectCodeList.Add(item.Code);
			}

			RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
			recipe.GetItem<ProcessDefectEdgeParameter>().Angles = angleList;
			recipe.GetItem<ProcessDefectEdgeParameter>().DefectCodes = defectCodeList;
			SetOptionDefect();
		}

		public ICommand btnAddOptionDefect
		{
			get
			{
				return new RelayCommand(() =>
				{
					OptionDefect item = new OptionDefect(0,0);
					OptionDefectList.Add(item);
					//SetParameter();
				});
			}
		}

		public ICommand btnSaveOptionDefect
		{
			get
			{
				return new RelayCommand(() =>
				{
					SetParameter();
				});
			}
		}
	}
	
	public class OptionDefect
	{
		public double Angle { get; set; }
		public int Code { get; set; }

		public OptionDefect(double angle, int code)
		{
			this.Angle = angle;
			this.Code = code;
		}
	}
}
