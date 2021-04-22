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
		private int grabModeIndex;
		// Light
		private int lightWhite = 0;
		private int lightSide = 0;
		#endregion

		#region [Getter Setter]
		public int GrabModeIndex
		{
			get => this.grabModeIndex;
			set => SetProperty(ref grabModeIndex, value);
		}
		public int LightWhite
		{
			get => this.lightWhite;
			set => SetProperty(ref lightWhite, value);
		}
		public int LightSide
		{
			get => this.lightSide;
			set => SetProperty(ref lightSide, value);
		}
		#endregion
	}
}
