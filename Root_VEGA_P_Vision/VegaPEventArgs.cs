using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;

namespace Root_VEGA_P_Vision
{
    public class SnapDoneArgs:EventArgs
    {
        public readonly CPoint startPos;
        public readonly CPoint endPos;

        public SnapDoneArgs(CPoint startPos, CPoint endPos)
        {
            this.startPos = startPos;
            this.endPos = endPos;
        }
    }
    public class RecipeEventArgs : EventArgs
    {
        public readonly RecipeBase recipe;

        public RecipeEventArgs(RecipeBase recipe)
        {
            this.recipe = recipe;
        }
    }
    public class ImageROIEventArgs:EventArgs
    {
        public readonly RecipeBase recipe;
        public readonly RecipeItemBase recipeItem;
        public readonly EUVPodSurfaceParameterBase parameterBase;
        
        public ImageROIEventArgs(RecipeBase recipe,RecipeItemBase recipeItem, EUVPodSurfaceParameterBase parameterBase)
        {
            this.recipe = recipe;
            this.recipeItem = recipeItem;
            this.parameterBase = parameterBase; 
        }
    }
    public class LoadAllRecipeEventArgs:EventArgs
    {
        public readonly List<RecipeCoverFront> CoverFrontRecipes;
        public readonly List<RecipeCoverBack> CoverBackRecipes;
        public readonly List<RecipePlateFront> PlateFrontRecipes;
        public readonly List<RecipePlateBack> PlateBackRecipes;

        public LoadAllRecipeEventArgs(List<RecipeCoverFront> coverFront, List<RecipeCoverBack> coverBack, List<RecipePlateFront> plateFront, List<RecipePlateBack> plateBack)
        {
            CoverFrontRecipes = coverFront;
            CoverBackRecipes = coverBack;
            PlateFrontRecipes = plateFront;
            PlateBackRecipes = plateBack;
        }

    }
}
