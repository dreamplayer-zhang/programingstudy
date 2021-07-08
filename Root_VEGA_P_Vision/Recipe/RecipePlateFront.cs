using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class RecipePlateFront:RecipeBase
    {
        public override void Initilize()
        {
            RegisterRecipeItem<EUVOriginRecipe>();
            RegisterRecipeItem<OriginRecipe>();//Recipe에서 필요함
            RegisterRecipeItem<EUVPositionRecipe>();
            RegisterRecipeItem<MaskRecipe>();
            RegisterRecipeItem<EUVPodSurfaceRecipe>();
            RegisterParameterItem<EUVPodSurfaceParameter>();
            RegisterRecipeItem<StainRecipe>();
            RegisterRecipeItem<LowResRecipe>();
            RegisterRecipeItem<HighResRecipe>();
            RegisterRecipeItem<SideRecipe>();

            WaferMap.MapSizeX = 1;
            WaferMap.MapSizeY = 1;
            WaferMap.CreateWaferMap(WaferMap.MapSizeX, WaferMap.MapSizeY, CHIP_TYPE.NORMAL);
        }
    }
}
