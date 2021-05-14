using RootTools;
using RootTools.Module;
using RootTools.Trees;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2.Module
{
	public class Run_InspectEBR : ModuleRunBase
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

		public Run_InspectEBR(EdgeSideVision module)
		{
			this.module = module;
			InitModuleRun(module);
		}

		public override ModuleRunBase Clone()
		{
			Run_InspectEBR run = new Run_InspectEBR(module);
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

				RootTools_Vision.WorkManager3.WorkManager workManager = GlobalObjects.Instance.GetNamed<RootTools_Vision.WorkManager3.WorkManager>("ebrInspection");
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
