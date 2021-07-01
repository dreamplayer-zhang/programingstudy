using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class LowResRecipe : RecipeItemBase
    {
        long focusZPos1, focusZPos2, focusZPos3;

        int white, ring;

        public long FocusZPos1
        {
            get => focusZPos1;
            set => SetProperty(ref focusZPos1, value);
        }
        public long FocusZPos2
        {
            get => focusZPos2;
            set => SetProperty(ref focusZPos2, value);
        }
        public long FocusZPos3
        {
            get => focusZPos3;
            set => SetProperty(ref focusZPos3, value);
        }
        public int White
        {
            get => white;
            set => SetProperty(ref white, value);
        }
        public int Ring
        {
            get => ring;
            set => SetProperty(ref ring, value);
        }
        public LowResRecipe() 
        {
        }

        public override void Clear()
        {
            white = ring = 0;
            focusZPos1 = focusZPos2 = focusZPos3 = long.MinValue;
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
