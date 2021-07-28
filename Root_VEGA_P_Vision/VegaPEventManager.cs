using System;

namespace Root_VEGA_P_Vision
{
    public static class VegaPEventManager
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
        public static event EventHandler<ImageROIEventArgs> ImageROIBtn;
        
        public static void OnImageROIBtnClicked(object obj, ImageROIEventArgs args)
        {
            ImageROIBtn?.Invoke(obj, args);
        }
        public static event EventHandler<LoadAllRecipeEventArgs> LoadedAllRecipe;
        public static void OnLoadedAllRecipe(object obj, LoadAllRecipeEventArgs args)
        {
            LoadedAllRecipe?.Invoke(obj, args);
        }
    }
}
