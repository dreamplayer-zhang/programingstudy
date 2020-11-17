using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.Temp_Recipe
{
    public class RecipeOrigin : IRecipe
    {
        int originX;
        int originY;

        int diePitchX;
        int diePitchY;

        int inspectionBufferOffsetX;
        int inspectionBufferOffsetY;

        public int OriginX { get => originX; set => originX = value; }
        public int OriginY { get => originY; set => originY = value; }
        public int DiePitchX { get => diePitchX; set => diePitchX = value; }
        public int DiePitchY { get => diePitchY; set => diePitchY = value; }
        public int InspectionBufferOffsetX { get => inspectionBufferOffsetX; set => inspectionBufferOffsetX = value; }
        public int InspectionBufferOffsetY { get => inspectionBufferOffsetY; set => inspectionBufferOffsetY = value; }

        public void Load()
        {
            
        }

        public void Save()
        {
            
        }
    }
}
