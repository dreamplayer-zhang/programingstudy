using System;

namespace Root_VEGA_P_Vision
{
    class VegaPEventManager
    {
        public static event EventHandler<SnapDoneArgs> SnapDone;

        public static void OnSnapDone(object obj, SnapDoneArgs args)
        {
            SnapDone?.Invoke(obj, args);
        }
        public static event EventHandler<RecipeEventArgs> RecipeUpdated;

        public static void OnRecipeUpdated(object obj, RecipeEventArgs args)
        {
            RecipeUpdated?.Invoke(obj, args);
        }
    }
}
