using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace RootTools_Vision
{
    public class RecipeData_Origin : IRecipeData
    {
        // FrontSide
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

        // Backside
        int backside_CenterX;
        int backside_CenterY;
        int backside_Radius;

        public int Backside_CenterX { get => backside_CenterX; set => backside_CenterX = value; }
        public int Backside_CenterY { get => backside_CenterY; set => backside_CenterY = value; }
        public int Backside_Radius { get => backside_Radius; set => backside_Radius = value; }

        public RecipeData_Origin()
        {

        }
    }
}
