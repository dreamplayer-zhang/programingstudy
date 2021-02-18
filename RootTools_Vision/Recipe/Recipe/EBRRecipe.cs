using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
	public class EBRRecipe : RecipeItemBase
	{
		#region [Parameter]
		// Camera
		private int cameraWidth;
		private int cameraHeight;
		private double cameraTriggerRatio;
		private int imageOffset;

		// Light
		private int lightWhite = 0;
		private int lightSide = 0;
		#endregion

		#region [Getter Setter]
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
		public double TriggerRatio
		{
			get => this.cameraTriggerRatio;
			set => SetProperty<double>(ref this.cameraTriggerRatio, value);
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
}
