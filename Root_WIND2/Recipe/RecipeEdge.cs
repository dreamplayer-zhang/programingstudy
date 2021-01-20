using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    public class RecipeEdge : RecipeBase
    {
        public override void Initilize()
        {
            // Regiseter Recipe Items
            RegisterRecipeItem<EdgeSurfaceRecipe>();

            // Regiseter Parameter Items
            RegisterParameterItem<EdgeSurfaceParameter>();
        }
    }
}
