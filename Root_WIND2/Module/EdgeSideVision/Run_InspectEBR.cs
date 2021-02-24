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
		int cameraHeight = 2000; // camera height
		int imageOffset = 0;	// 이미지 시작 지점 offset

		#region [Getter/Setter]

		public string RecipeName
		{
			get => recipeName;
			set => recipeName = value;
		}
		public int CameraHeight
		{
			get => cameraHeight;
			set => cameraHeight = value;
		}

		public int ImageOffset
		{
			get => imageOffset;
			set => imageOffset = value;
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
			run.cameraHeight = cameraHeight;
			run.imageOffset = imageOffset;
			return run;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
		{
			recipeName = tree.SetFile(recipeName, recipeName, "rcp", "Recipe", "Recipe Name", bVisible);
			cameraHeight = (tree.GetTree("Camera Height", false, bVisible)).Set(cameraHeight, cameraHeight, "Height", "Camera Height", bVisible);
			imageOffset = (tree.GetTree("Image Offset", false, bVisible)).Set(imageOffset, imageOffset, "Offset", "Height offset (pxl)", bVisible);
		}

		public override string Run()
		{
			try
			{
				if (EQ.IsStop())
					return "OK";

				InspectionManagerEBR inspectionEBR = GlobalObjects.Instance.Get<InspectionManagerEBR>();

				if (inspectionEBR.Recipe.Read(recipeName) == false)
					return "Recipe Open Fail";

				inspectionEBR.Start();
				
				while (inspectionEBR.CheckAllWorkDone() == false)
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
