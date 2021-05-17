using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    public class RecipeEBR : RecipeBase
    {
        public override void Initilize()
        {
            // Regiseter Recipe Items
            RegisterRecipeItem<OriginRecipe>();
            RegisterRecipeItem<EBRRecipe>();

            // Regiseter Parameter Items
            RegisterParameterItem<EBRParameter>();
            RegisterParameterItem<ProcessDefectEdgeParameter>();
        }
    }
}
