using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    /// <summary>
    /// Edit 클래스에서 파생된 데이터를 검사 데이터로 옮김.
    /// 이 클래스는 Recipe의 하부로 들어감
    /// </summary>
    public class RecipeData : IRecipeData
    {
        List<IRecipeData> recipes;

        public List<IRecipeData> Recipes { get => recipes; set => recipes = value; }

        public RecipeData()
        {
            Recipes = new List<IRecipeData>();

            AddRecipe(new RecipeData_Origin());
            AddRecipe(new RecipeData_Position());
        }

        private void AddRecipe(IRecipeData recipe)
        {
            Recipes.Add(recipe);
        }

        public IRecipeData GetRecipeData(Type type)
        {
            foreach (IRecipeData recipe in Recipes)
            {
                if (recipe.GetType() == type)
                {
                    return recipe;
                }
            }

            return null;
        }
    }
}
