using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_AOP01_Inspection.Recipe
{
	public class AOP_RecipeSurface : RecipeBase
    {
        public override void Initilize()
        {
            WaferMap = new RecipeType_WaferMap();
            // Register Recipe Items
            RegisterRecipeItem<BacksideRecipe>();
            RegisterRecipeItem<OriginRecipe>();

            // Regiseter Parameter Items
            //RegisterParameterItem<PositionParameter>();
            RegisterParameterItem<ReticleSurfaceParameter>();
        }
    }
}
