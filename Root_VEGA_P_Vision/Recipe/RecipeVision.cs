using RootTools_Vision;

namespace Root_VEGA_P_Vision
{
    class RecipeVision:RecipeBase
    {
        public override void Initilize()
        {
            RegisterRecipeItem<MaskRecipe>();
            RegisterRecipeItem<SurfaceRecipe>();
        }
    }
}
