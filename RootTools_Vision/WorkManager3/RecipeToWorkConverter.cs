using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.WorkManager3
{
    public class RecipeToWorkConverter
    {
        public static WorkBundle Convert(RecipeBase recipe)
        {
            try
            {
                List<ParameterBase> parameters = recipe.ParameterItemList;

                WorkBundle bundle = new WorkBundle();
                foreach (ParameterBase param in parameters)
                {
                    WorkBase work = (WorkBase)Tools.CreateInstance(param.InspectionType);

                    work.SetRecipe(recipe);
                    work.SetParameter(param);

                    bundle.Add(work);
                }

                return bundle;
            }
            catch(Exception ex)
            {
                throw new ArgumentException(ex.StackTrace + " " + ex.Message);
            }
        }
    }
}
