using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class BacksideRecipe : RecipeBase
    {
        #region [Parameter]
        //
        int originX;
        int originY;
        int diePitchX;
        int diePitchY;

        int centerX;
        int centerY;
        int radius;
        #endregion

        #region [Getter Setter]
        public int CenterX { get => centerX; set => centerX = value; }
        public int CenterY { get => centerY; set => centerY = value; }
        public int Radius { get => radius; set => radius = value; }
        public int OriginX { get => originX; set => originX = value; }
        public int OriginY { get => originY; set => originY = value; }
        public int DiePitchX { get => diePitchX; set => diePitchX = value; }
        public int DiePitchY { get => diePitchY; set => diePitchY = value; }
        #endregion

        public BacksideRecipe()
        {

        }

        public override bool Read(string recipePath)
        {
            return true;
        }

        public override bool Save(string recipePath)
        {
            return true;
        }
    }
}
