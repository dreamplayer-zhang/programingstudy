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
		private int grabModeIndex;

		// TEMP
		private int firstNotch;
		private int lastNotch;

		// Light
		private int lightWhite = 0;
		private int lightSide = 0;
		#endregion

		#region [Getter Setter]
		public int GrabModeIndex
		{
			get => this.grabModeIndex;
			set => SetProperty<int>(ref this.grabModeIndex, value);
		}
		public int FirstNotch
		{
			get => this.firstNotch;
			set => SetProperty<int>(ref this.firstNotch, value);
		}
		public int LastNotch
		{
			get => this.lastNotch;
			set => SetProperty<int>(ref this.lastNotch, value);
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
