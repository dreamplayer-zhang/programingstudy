using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class StainRecipe : RecipeItemBase
    {
        #region [Parameter]
        int lightFront;
        int lightSide;
        int focusZPos;
        #endregion

        #region [Getter/Setter]
        public int LightFront
        {
            get => lightFront;
            set => SetProperty(ref lightFront, value);
        }
        public int LightSide
        {
            get => lightSide;
            set => SetProperty(ref lightSide, value);
        }
        public int FocusZPos
        {
            get => focusZPos;
            set => SetProperty(ref focusZPos, value);
        }
        #endregion
        public StainRecipe() { }
        public override void Clear()
        {
            lightFront = 0;
            lightSide = 0;
            focusZPos = 0;
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
