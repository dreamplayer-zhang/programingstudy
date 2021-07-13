using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_EFEM.Module
{
    public class RecipeAlign : RecipeBase
    {
        public override void Initilize()
        {
            base.Initilize();

            RegisterRecipeItem<FrontAlignRecipe>();
            RegisterRecipeItem<FrontVRSAlignRecipe>();
        }
    }
}
