using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class RecipePlateBack:RecipeBase
    {
        public override void Initilize()
        {
            RegisterRecipeItem<EUVOriginRecipe>();
            RegisterRecipeItem<OriginRecipe>();//Recipe에서 필요함
            RegisterRecipeItem<EUVPositionRecipe>();
            RegisterRecipeItem<StainRecipe>();
            RegisterRecipeItem<LowResRecipe>();
            RegisterRecipeItem<MaskRecipe>();
            RegisterRecipeItem<EUVPodSurfaceRecipe>();
            RegisterParameterItem<EUVPodSurfaceParameter>();

            WaferMap.MapSizeX = 1;
            WaferMap.MapSizeY = 1;
            WaferMap.CreateWaferMap(WaferMap.MapSizeX, WaferMap.MapSizeY, CHIP_TYPE.NORMAL);
        }
    }
}
