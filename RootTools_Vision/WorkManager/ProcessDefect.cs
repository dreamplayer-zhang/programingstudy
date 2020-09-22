using RootTools_Vision.Temp_Recipe;
using RootTools_Vision.UserTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class ProcessDefect : IWork
    {
        public WORK_TYPE Type => WORK_TYPE.FINISHINGWORK;

        public void DoWork()
        {
            DoProcessDefect();
        }

        public void SetWorkplace(Workplace workplace)
        {
            
        }

        public void DoProcessDefect()
        {
            Thread.Sleep(2000);
        }

        public void SetData(IRecipeData _recipeData, IParameterData _parameterData)
        {
            throw new NotImplementedException();
        }
    }
}
