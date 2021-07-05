using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WindII_Option
{
    public class RecipeEdge : RecipeBase
    {
        public override void Initilize()
        {
            // Regiseter Recipe Items
            RegisterRecipeItem<OriginRecipe>();
            RegisterRecipeItem<EdgeSurfaceRecipe>();

            // Regiseter Parameter Items
            RegisterParameterItem<EdgeSurfaceParameter>();
            RegisterParameterItem<ProcessDefectEdgeParameter>();

            CreateMap();
        }

        public void CreateMap()
        {
            this.WaferMap.MapSizeX = 1;
            this.WaferMap.MapSizeY = 1;
            this.WaferMap.CreateWaferMap(this.WaferMap.MapSizeX, this.WaferMap.MapSizeY, CHIP_TYPE.NORMAL);
        }
    }
}
