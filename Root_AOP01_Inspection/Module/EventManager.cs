using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_AOP01_Inspection.Module
{
	class AOPEventManager
	{
		///
		///     SnapDone
		///
		public static event EventHandler<SnapDoneArgs> SnapDone;

		public static void OnSnapDone(object obj, SnapDoneArgs args)
		{
			SnapDone?.Invoke(obj, args);
		}

		public static event EventHandler<RecipeEventArgs> BeforeRecipeSave;

		public static void OnBeforeRecipeSave(object obj, RecipeEventArgs args)
		{
			BeforeRecipeSave?.Invoke(obj, args);
		}

		public static event EventHandler<RecipeEventArgs> AfterRecipeSave;

		public static void OnAfterRecipeSave(object obj, RecipeEventArgs args)
		{
			AfterRecipeSave?.Invoke(obj, args);
		}

		public static event EventHandler<RecipeEventArgs> BeforeRecipeRead;

		public static void OnBeforeRecipeRead(object obj, RecipeEventArgs args)
		{
			BeforeRecipeRead?.Invoke(obj, args);
		}

		public static event EventHandler<RecipeEventArgs> AfterRecipeRead;

		public static void OnAfterRecipeRead(object obj, RecipeEventArgs args)
		{
			AfterRecipeRead?.Invoke(obj, args);
		}
	}
}
