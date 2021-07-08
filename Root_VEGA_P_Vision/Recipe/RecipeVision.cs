using RootTools_Vision;

namespace Root_VEGA_P_Vision
{
    public class RecipeVision:RecipeBase
    {
        public override void Initilize()
        {
            RegisterRecipeItem<EUVOriginRecipe>();
            RegisterRecipeItem<EUVPositionRecipe>();
            RegisterRecipeItem<MaskRecipe>();
            RegisterRecipeItem<EUVPodSurfaceRecipe>();
            RegisterParameterItem<EUVPodSurfaceParameter>();

            WaferMap.MapSizeX = 1;
            WaferMap.MapSizeY = 1;
            WaferMap.CreateWaferMap(WaferMap.MapSizeX, WaferMap.MapSizeY, CHIP_TYPE.NORMAL);
        }
    }
}
