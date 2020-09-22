using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using RootTools_CLR;
using RootTools_Vision.Temp_Recipe;
using RootTools_Vision.UserTypes;

namespace RootTools_Vision
{
    public class Surface : IWork
    {
        int index;
        int mapPositionX;
        int mapPositionY;

        public WORK_TYPE Type => WORK_TYPE.MAINWORK;

        public void DoWork()
        {
            DoInspection();
        }

        public void SetWorkplace(Workplace workplace)
        {
            index = workplace.Index;
            mapPositionX = workplace.MapPositionX;
            mapPositionY = workplace.MapPositionY;

        }

        public void DoInspection()
        {
            int nCount = 8000;
            double sum = 0;
            for(int i = 0; i < nCount; i++)
            {
                for (int j = 0; j < nCount; j++)
                {
                    sum += Math.Log(i) * Math.Log(j);
                }
            }
        }

        public void SetData(IRecipeData _recipeData, IParameter _parameterData)
        {
            throw new NotImplementedException();
        }
    }
}
