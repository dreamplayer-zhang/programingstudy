using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace RootTools_Vision
{
    public class ParamData_Position : ObservableObject, IParameterData
    {
        int searchRangeX;
        int searchRangeY;

        int minScoreLimit;

        public int SearchRangeX { get => searchRangeX; set => searchRangeX = value; }
        public int SearchRangeY { get => searchRangeY; set => searchRangeY = value; }
        public int MinScoreLimit { get => minScoreLimit; set => minScoreLimit = value; }
    }
}
