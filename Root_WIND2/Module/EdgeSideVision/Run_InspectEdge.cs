using RootTools.Module;
using RootTools.Trees;
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

		InspectionManagerEdge inspectionEdge;
		string recipeName = string.Empty;

		int roiHeight = 1000;
		int roiWidth = 3000;
		int threshhold = 12;
		int size = 5;
		int mergeDist = 5;

		#region [Getter/Setter]
		public InspectionManagerEdge InspectionEdge
		{
			get => inspectionEdge;
			set => inspectionEdge = value;
		}
		public string RecipeName
		{
			get => recipeName;
			set => recipeName = value;
		}
		#endregion

		public Run_InspectEdge(EdgeSideVision module)
		{
			this.module = module;
			inspectionEdge = ((WIND2_Engineer)module.m_engineer).InspectionEdge;
			InitModuleRun(module);
		}

		public override ModuleRunBase Clone()
		{
			Run_InspectEdge run = new Run_InspectEdge(module);
			run.inspectionEdge = ProgramManager.Instance.InspectionEdge;
			run.recipeName = recipeName;

			run.roiHeight = roiHeight;
			run.roiWidth = roiWidth;
			run.size = size;
			run.mergeDist = mergeDist;
			run.threshhold = threshhold;
			return run;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
		{
			roiHeight = tree.Set(roiHeight, roiHeight, "ROI Height", "", bVisible);
			roiWidth = tree.Set(roiWidth, roiWidth, "ROI Width", "", bVisible);
			threshhold = tree.Set(threshhold, threshhold, "Theshold", "", bVisible);
			size = tree.Set(size, size, "Defect Size", "pixel", bVisible);
			mergeDist = tree.Set(mergeDist, mergeDist, "Merge Distance", "pixel", bVisible);
		}

		public override string Run()
		{
			try
			{
				//if (this.inspectionEdge.Recipe.Read(m_sRecipeName, true) == false)
				//	return "Recipe Open Fail";

				if (this.inspectionEdge.CreateInspection() == false)
					return "Create Inspection Fail";

				inspectionEdge.Start();
				return "OK";
			}
			finally
			{
			}
		}
	}
}