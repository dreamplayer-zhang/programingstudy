using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2.Module
{
	public class Run_InspectEdgeSide : ModuleRunBase
	{
		EdgeSideVision module;

		InspectionManager_EFEM inspectionEdge;
		public InspectionManager_EFEM InspectionEdge
		{
			get => inspectionEdge;
			set => inspectionEdge = value;
		}

		public Run_InspectEdgeSide(EdgeSideVision module)
		{
			this.module = module;
			inspectionEdge = ((WIND2_Engineer)module.m_engineer).InspectionEFEM;
			InitModuleRun(module);
		}

		public override ModuleRunBase Clone()
		{
			Run_InspectEdgeSide run = new Run_InspectEdgeSide(module);

			run.InspectionEdge = ProgramManager.Instance.InspectionEFEM;

			return run;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
		{

		}

		public override string Run()
		{
			return base.Run();
		}
	}
}
