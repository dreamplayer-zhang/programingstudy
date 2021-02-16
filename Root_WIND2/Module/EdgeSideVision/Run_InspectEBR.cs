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
		int height = 2000; // camera height

		#region [Getter/Setter]

		public string RecipeName
		{
			get => recipeName;
			set => recipeName = value;
		}
		public int Height
		{
			get => height;
			set => height = value;
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
			run.height = height;

			return run;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
		{
			recipeName = tree.SetFile(recipeName, recipeName, "rcp", "Recipe", "Recipe Name", bVisible);
			height = (tree.GetTree("Camera Height", false, bVisible)).Set(height, height, "Height", "Camera Height", bVisible);
		}

		public override string Run()
		{
			try
			{
				InspectionManagerEBR inspectionEBR = GlobalObjects.Instance.Get<InspectionManagerEBR>();

				if (inspectionEBR.Recipe.Read(recipeName, true) == false)
					return "Recipe Open Fail";

				inspectionEBR.Start();
				return "OK";
			}
			finally
			{
			}
		}
	}
}
