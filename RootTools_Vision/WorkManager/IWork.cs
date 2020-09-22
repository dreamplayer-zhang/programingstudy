using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public interface IWork
    {
        UserTypes.WORK_TYPE Type { get; }

        void DoWork();

        void SetWorkplace(Workplace workplace);

        void SetData(IRecipeData _recipeData, IParameterData _parameterData);
    }
}
