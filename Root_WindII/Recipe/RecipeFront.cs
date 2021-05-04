using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WindII
{
    public class RecipeFront : RecipeBase
    {
        public override void Initilize()
        {
            RegisterRecipeItem<OriginRecipe>();
            RegisterRecipeItem<PositionRecipe>();
            RegisterRecipeItem<MaskRecipe>();
            RegisterRecipeItem<D2DRecipe>();
            RegisterRecipeItem<SurfaceRecipe>();

            // Frontside에서 Parameter는 동적으로 추가되므로 생성하지 않음
            // 사용 예시) RegisterParameterItem<D2DParameter>();
        }
    }
}
