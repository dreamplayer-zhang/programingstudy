using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.Temp_Recipe
{
    public class ParameterPosition : IParameter
    {
        int searchRangeX;
        int searchRangeY;

        int minScoreLimit;

        public int SearchRangeX { get => searchRangeX; set => searchRangeX = value; }
        public int SearchRangeY { get => searchRangeY; set => searchRangeY = value; }
        public int MinScoreLimit { get => minScoreLimit; set => minScoreLimit = value; }

        public void Load()
        {
          
        }

        public void Save()
        {
            
        }
    }
}
