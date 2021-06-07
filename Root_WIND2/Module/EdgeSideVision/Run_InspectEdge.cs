using RootTools;
using RootTools.Database;
using RootTools.Module;
using RootTools.Trees;
using RootTools_Vision;
using RootTools_Vision.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2.Module
{
	public class Run_InspectEdge : ModuleRunBase
	{
		EdgeSideVision module;
		
		string recipeName = string.Empty;

		#region [Getter/Setter]
		public string RecipeName
		{
			get => recipeName;
			set => recipeName = value;
		}
		#endregion


		#region [Klarf]
		private static KlarfData_Lot klarfData = new KlarfData_Lot();


		private static void LotStart(string klarfPath, RecipeBase recipe, InfoWafer infoWafer, GrabModeBase grabMode)
		{
			if (klarfData == null) klarfData = new KlarfData_Lot();

			if (Directory.Exists(klarfPath)) Directory.CreateDirectory(klarfPath);


			klarfData.LotStart(klarfPath, infoWafer, recipe.WaferMap, grabMode);
		}

		private void CreateKlarf(RecipeBase recipe, InfoWafer infoWafer, List<Defect> defectList, bool useTDIReview = false, bool useVrsReview = false)
		{
			//klarfData.SetResolution((float)camInfo.RealResX, (float)camInfo.RealResY);
			// Product 정보 셋팅

			klarfData.WaferStart(recipe.WaferMap, infoWafer);
			klarfData.AddSlot(recipe.WaferMap, defectList, recipe.GetItem<OriginRecipe>(), useTDIReview, useVrsReview);
			klarfData.SaveKlarf();

		}

		private void LotEnd(InfoWafer infoWafer)
		{
			klarfData.CreateLotEnd();
		}
		#endregion

		public Run_InspectEdge(EdgeSideVision module)
		{
			this.module = module;
			InitModuleRun(module);
		}

		public override ModuleRunBase Clone()
		{
			Run_InspectEdge run = new Run_InspectEdge(module);
			run.recipeName = recipeName;
			return run;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
		{
			recipeName = tree.SetFile(recipeName, recipeName, "rcp", "Recipe", "Recipe Name", bVisible);
		}

		public override string Run()
		{
			try
			{
				if (EQ.IsStop())
					return "OK";

				RootTools_Vision.WorkManager3.WorkManager workManager = GlobalObjects.Instance.GetNamed<RootTools_Vision.WorkManager3.WorkManager>("edgeInspection");
				if (workManager == null)
				{
					throw new ArgumentException("WorkManager가 초기화되지 않았습니다(null)");
				}
				workManager.Stop();

				if (EQ.IsStop() == false)
				{
					if (workManager.OpenRecipe(recipeName) == false)
						return "Recipe Open Fail";

					workManager.Start(false);

				}
				else
				{
					workManager.Stop();
				}
				return "OK";
			}
			finally
			{
			}
		}
	}
}