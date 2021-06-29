using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class VisionSurfaceRecipe : RecipeItemBase
    {
        public override void Clear()
        {
        }

        public override bool Read(string recipePath)
        {
            return true;
        }

        public override bool Save(string recipePath)
        {
            return true;
        }
    }

    public class VisionSurfaceRecipeBase:ObservableObject
    {

    }
}
