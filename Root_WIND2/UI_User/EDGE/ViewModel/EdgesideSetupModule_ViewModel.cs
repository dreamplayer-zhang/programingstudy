using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Root_WIND2.UI_User
{
	public class EdgesideSetupModule_ViewModel : ObservableObject
	{
		#region [Getter / Setter]

		private OriginRecipe originRecipe;
		public OriginRecipe OriginRecipe
		{
			get => originRecipe;
			set
			{
				SetProperty(ref originRecipe, value);
			}
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
			get => parameter;
			set => SetProperty(ref parameter, value);
		}

		private string side = "TOP";
		public string Side
		{
			get => side;
			set => SetProperty(ref side, value);
		}

		#region Scan Infomation

		private int cameraWidth;
		public int CameraWidth
		{
			get => cameraWidth;
			set => SetProperty(ref cameraWidth, value);
		}

		private int cameraHeight;
		public int CameraHeight
		{
			get => cameraHeight;
			set => SetProperty(ref cameraHeight, value);
		}

		private int imageHeight;
		public int ImageHeight
		{
			get => imageHeight;
			set => SetProperty(ref imageHeight, value);
		}

		private double resolution;
		public double Resolution
		{
			get => resolution;
			set => SetProperty(ref resolution, value);
		}

		private int positionOffset;
		public int PositionOffset
		{
			get => positionOffset;
			set => SetProperty(ref positionOffset, value);
		}

		private int selectedGrabModeIndex = 0;
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

				//DialogResult result = MessageBox.Show("GrabMode 바꾸면 계산 바뀜", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				//if (result == DialogResult.Yes)
				CalculateInspectionROI();
				//else if (result == DialogResult.No)
				//	SetOriginInfo();

				SetProperty<int>(ref this.selectedGrabModeIndex, value);
			}
		}

		public List<string> GrabModeList
		{
			get
			{
				return ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.p_asGrabMode;
			}
		}

		#endregion

		#region Origin Information

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
			Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop;
			Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop;
		}

		/// <summary>
		/// 전체 이미지를 1chip으로 계산.
		/// OriginY는 이미지의 전체 Height를 말함.
		/// </summary>
		private void SetOriginInfo()
		{
			GrabModeEdge mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[Recipe.GrabModeIndex];

			if (mode == null)
				return;

			int imageHeight = mode.m_nImageHeight;  // 전체 이미지 Height
			OriginRecipe.OriginX = 0;
			OriginRecipe.OriginY = imageHeight;
			OriginRecipe.OriginHeight = imageHeight;
			OriginRecipe.DiePitchY = imageHeight;
		}

		private void CalculateInspectionROI()
		{
			GrabModeEdge mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[Recipe.GrabModeIndex];

			int imageHeight = mode.m_nImageHeight;
			int positionOffset = mode.m_nCameraPositionOffset;
			int heightPerDegree = (int)(imageHeight / mode.m_nScanDegree);
			int startPosition = heightPerDegree * positionOffset;

			OriginX = 0;
			OriginY = imageHeight;
			OriginHeight = imageHeight;
			Parameter.StartPosition = startPosition;
		}

		public void SetRecipeParameter(EdgeSurfaceRecipeBase recipe, EdgeSurfaceParameterBase param)
		{
			this.Recipe = recipe;
			this.Parameter = param;
			this.SelectedGrabModeIndex = recipe.GrabModeIndex;

			this.OriginWidth = OriginRecipe.OriginWidth;
		}

		public void SetName(string name)
		{
			this.Side = name;
		}
	}
}
