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

				InspectionManagerEdge inspectionEdge = GlobalObjects.Instance.Get<InspectionManagerEdge>();

				if (inspectionEdge.Recipe.Read(recipeName) == false)
					return "Recipe Open Fail";

				inspectionEdge.Start();

				while (inspectionEdge.CheckAllWorkDone() == false)
				{
					if (EQ.IsStop())
						return "OK";

					Task.Delay(1000);
				}

				return "OK";
			}
			finally
			{
			}
		}
	}
}