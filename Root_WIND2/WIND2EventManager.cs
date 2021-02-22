using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    class WIND2EventManager
    {
        ///
        ///     SnapDone
        ///
        public static event EventHandler<SnapDoneArgs> SnapDone;

        public static void OnSnapDone(object obj, SnapDoneArgs args)
        {
            SnapDone?.Invoke(obj, args);
        }

        /// <summary>
        /// 무분별한 사용 조심해야함
        /// </summary>
        public static event EventHandler<RecipeEventArgs> RecipeUpdated;

        public static void OnRecipeUpdated(object obj, RecipeEventArgs args)
        {
            RecipeUpdated?.Invoke(obj, args);
        }
    }
}
