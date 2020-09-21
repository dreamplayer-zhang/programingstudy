using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.Temp_Recipe
{
    public class Recipe : IRecipe
    {
        List<IRecipe> recipes;

        public Recipe()
        {
            recipes = new List<IRecipe>();

            AddRecipe(new RecipeOrigin());
            AddRecipe(new RecipePosition());
        }



        private void AddRecipe(IRecipe recipe)
        {
            recipes.Add(recipe);
        }

        public IRecipe GetRecipe(Type type)
        {
            foreach(IRecipe recipe in recipes)
            {
                if(recipe.GetType() == type)
                {
                    return recipe;
                }
            }

            return null;
        }

        public void Save()
        {
            foreach(IRecipe recipe in recipes)
            {
                recipe.Save();
            }
        }

        public void Load()
        {
            foreach (IRecipe recipe in recipes)
            {
                recipe.Load();
            }
        }
    }
}
