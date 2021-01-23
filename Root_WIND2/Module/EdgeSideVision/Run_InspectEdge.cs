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

		// degree, 카메라 위치 각도 offset
		int topOffset = 0;
		int sideOffset = 45;
		int btmOffset = 90;

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

			run.topOffset = topOffset;
			run.sideOffset = sideOffset;
			run.btmOffset = btmOffset;
			return run;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
		{
			recipeName = tree.SetFile(recipeName, recipeName, "rcp", "Recipe", "Recipe Name", bVisible);

			topOffset = (tree.GetTree("Camera Offset", false, bVisible)).Set(topOffset, topOffset, "Top Camera", "카메라 위치 offset (Degree)", bVisible);
			sideOffset = (tree.GetTree("Camera Offset", false, bVisible)).Set(sideOffset, sideOffset, "Side Camera", "카메라 위치 offset (Degree)", bVisible);
			btmOffset = (tree.GetTree("Camera Offset", false, bVisible)).Set(btmOffset, btmOffset, "Bottom Camera", "카메라 위치 offset (Degree)", bVisible);
		}

		public override string Run()
		{
			try
			{
				InspectionManagerEdge inspectionEdge = GlobalObjects.Instance.Get<InspectionManagerEdge>();
				if (inspectionEdge.Recipe.Read(recipeName, true) == false)
					return "Recipe Open Fail";

				if (inspectionEdge.SetCameraInfo() == false)
					return "Set Camera Info Fail";

				inspectionEdge.Start();
				return "OK";
			}
			finally
			{
			}
		}
	}
}