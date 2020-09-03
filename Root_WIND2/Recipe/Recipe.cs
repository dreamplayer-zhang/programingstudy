using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_WIND2
{
    /// <summary>
    /// Recipe 각 항목의 묶음.
    /// Recipe에 대한 기능 자체는 Manager에서 다룸.
    /// </summary>
    public class Recipe
    {
        // ORIGIN, Die Pitch
        // WAFER MAP
        // POSITION
        // ROI (SURFACE, D2D)
        // INSPECTION PARAMETER

        RecipeData_Origin RecipeData_Origin;
        RecipeData_Position RecipeData_Position;
        RecipeData_Parameter RecipeData_Parameter;
        RecipeData_ROI RecipeData_ROI;

        public Recipe()
        {
            Init();
        }

        public void Init()
        {
            RecipeData_Origin = new RecipeData_Origin();
            RecipeData_Position = new RecipeData_Position();
            RecipeData_Parameter = new RecipeData_Parameter();
            RecipeData_ROI = new RecipeData_ROI();
        }

    }
}
