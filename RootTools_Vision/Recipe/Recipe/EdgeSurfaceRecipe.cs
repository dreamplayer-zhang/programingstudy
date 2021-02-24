using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
	public class EdgeSurfaceRecipe : RecipeItemBase
	{
		private EdgeSurfaceRecipeBase edgeRecipeBaseTop;
		private EdgeSurfaceRecipeBase edgeRecipeBaseSide;
		private EdgeSurfaceRecipeBase edgeRecipeBaseBtm;

		#region [Getter/Setter]
		public EdgeSurfaceRecipeBase EdgeRecipeBaseTop
		{
			get => this.edgeRecipeBaseTop;
			set => SetProperty(ref edgeRecipeBaseTop, value);
		}
		public EdgeSurfaceRecipeBase EdgeRecipeBaseSide
		{
			get => this.edgeRecipeBaseSide;
			set => SetProperty(ref edgeRecipeBaseSide, value);
		}
		public EdgeSurfaceRecipeBase EdgeRecipeBaseBtm
		{
			get => this.edgeRecipeBaseBtm;
			set => SetProperty(ref edgeRecipeBaseBtm, value);
		}
		#endregion

		public EdgeSurfaceRecipe()
		{
			edgeRecipeBaseTop = new EdgeSurfaceRecipeBase();
			edgeRecipeBaseSide = new EdgeSurfaceRecipeBase();
			edgeRecipeBaseBtm = new EdgeSurfaceRecipeBase();
		}

        public override void Clear()
        {
            
        }

        public override bool Read(string recipePath)
		{
			return true;
		}

		public override bool Save(string recipePath)
		{
			return true;
		}
	}

	public class EdgeSurfaceRecipeBase : ObservableObject
	{
		#region [Parameter]
		private string grabModeName;
		// Camera
		private int cameraWidth;
		private int cameraHeight;
		private double cameraResolution;
		private double cameraTriggerRatio;
		private int positionOffset;	// top side bottom 위치 offset
		private int imageOffset;
		// Light
		private int lightWhite = 0;
		private int lightSide = 0;

		#endregion

		#region [Getter Setter]
		public string GrabModeName
		{
			get => this.grabModeName;
			set => SetProperty<string>(ref this.grabModeName, value);
		}
		public int CameraWidth
		{
			get => this.cameraWidth;
			set => SetProperty<int>(ref this.cameraWidth, value);
		}
		public int CameraHeight
		{
			get => this.cameraHeight;
			set => SetProperty<int>(ref this.cameraHeight, value);
		}
		public double Resolution
		{
			get => this.cameraResolution;
			set => SetProperty<double>(ref this.cameraResolution, value);	
		}
		public double TriggerRatio
		{
			get => this.cameraTriggerRatio;
			set => SetProperty<double>(ref this.cameraTriggerRatio, value);
		}
		public int PositionOffset
		{
			get => this.positionOffset;
			set => SetProperty<int>(ref this.positionOffset, value);
		}
		public int ImageOffset
		{
			get => this.imageOffset;
			set => SetProperty<int>(ref this.imageOffset, value);
		}
		public int LightWhite
		{
			get => this.lightWhite;
			set => SetProperty<int>(ref this.lightWhite, value);
		}
		public int LightSide
		{
			get => this.lightSide;
			set => SetProperty<int>(ref this.lightSide, value);
		}
		#endregion
	}
}
