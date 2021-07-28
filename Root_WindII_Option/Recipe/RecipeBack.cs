using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WindII_Option
{
    public class RecipeBack : RecipeBase
    {
        public override void Initilize()
        {
            // Register Recipe Items
            RegisterRecipeItem<OriginRecipe>();
            RegisterRecipeItem<BacksideRecipe>();

            // Regiseter Parameter Items
            //RegisterParameterItem<PositionParameter>();
            RegisterParameterItem<BacksideSurfaceParameter>();

            this.UseExclusiveRegion = true;
            this.ExclusiveRegionFilePath = Constants.FilePath.BacksideExclusiveRegionFilePath;
        }
    }
}
