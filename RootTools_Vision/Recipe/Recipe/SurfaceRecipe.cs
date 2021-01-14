using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class SurfaceRecipe : RecipeBase
    {
        int test = 0;

        public int Test { get => test; set => test = value; }

        public SurfaceRecipe()
        {

        }

        public override bool Save(string recipePath)
        {
            return true;
        }

        public override bool Read(string recipePath)
        {
            return true;
        }

        public override void Clear()
        {
            
        }
    }
}
