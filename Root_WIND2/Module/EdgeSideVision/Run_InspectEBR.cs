using RootTools.Module;
using RootTools.Trees;
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

		InspectionManagerEBR inspectionEBR;
		string recipeName = string.Empty;

		#region [Getter/Setter]
		public InspectionManagerEBR InspectionEBR
		{
			get => inspectionEBR;
			set => inspectionEBR = value;
		}
		public string RecipeName
		{
			get => recipeName;
			set => recipeName = value;
		}
		#endregion

		public Run_InspectEBR(EdgeSideVision module)
		{
			this.module = module;
			inspectionEBR = ((WIND2_Engineer)module.m_engineer).InspectionEBR;
			InitModuleRun(module);
		}

		public override ModuleRunBase Clone()
		{
			Run_InspectEBR run = new Run_InspectEBR(module);
			run.InspectionEBR = ProgramManager.Instance.InspectionEBR;
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
				if (this.inspectionEBR.Recipe.Read(recipeName, true) == false)
					return "Recipe Open Fail";

				if (this.inspectionEBR.CreateInspection() == false)
					return "Create Inspection Fail";

				inspectionEBR.Start();
				return "OK";
			}
			finally
			{
			}
		}
	}
}
