﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class D2DRecipe : RecipeBase
    {
        public D2DRecipe()
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
}