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

		// degree, 카메라 위치 각도 offset
		int topPositionOffset = 0;
		int sidePositionOffset = 45;
		int btmPositionOffset = 90;

		// 이미지 시작 지점 offset
		int topImageOffset = 0;
		int sideImageOffset = 0;
		int btmImageOffset = 0;

		// camera height
		int topCameraHeight = 2000;
		int sideCameraHeight = 2000;
		int btmCameraHeight = 2000;

		#region [Getter/Setter]
		public string RecipeName
		{
			get => recipeName;
			set => recipeName = value;
		}
		public int TopPositionOffset
		{
			get => topPositionOffset;
			set => topPositionOffset = value;
		}
		public int SidePositionOffset
		{
			get => sidePositionOffset;
			set => sidePositionOffset = value;
		}
		public int BtmPositionOffset
		{
			get => btmPositionOffset;
			set => btmPositionOffset = value;
		}
		public int TopImageOffset
		{
			get => topImageOffset;
			set => topImageOffset = value;
		}
		public int SideImageOffset
		{
			get => sideImageOffset;
			set => sideImageOffset = value;
		}
		public int BtmImageOffset
		{
			get => btmImageOffset;
			set => btmImageOffset = value;
		}
		public int TopCameraHeight
		{
			get => topCameraHeight;
			set => topCameraHeight = value;
		}
		public int SideCameraHeight
		{
			get => sideCameraHeight;
			set => sideCameraHeight = value;
		}
		public int BtmCameraHeight
		{
			get => btmCameraHeight;
			set => btmCameraHeight = value;
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

			run.topPositionOffset = topPositionOffset;
			run.sidePositionOffset = sidePositionOffset;
			run.btmPositionOffset = btmPositionOffset;

			run.topImageOffset = topImageOffset;
			run.sideImageOffset = sideImageOffset;
			run.btmImageOffset = btmImageOffset;

			run.topCameraHeight = topCameraHeight;
			run.sideCameraHeight = sideCameraHeight;
			run.btmCameraHeight = btmCameraHeight;

			return run;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
		{
			recipeName = tree.SetFile(recipeName, recipeName, "rcp", "Recipe", "Recipe Name", bVisible);
			topCameraHeight = (tree.GetTree("Camera Height", false, bVisible)).Set(topCameraHeight, topCameraHeight, "Top", "Camera Height", bVisible);
			sideCameraHeight = (tree.GetTree("Camera Height", false, bVisible)).Set(sideCameraHeight, sideCameraHeight, "Side", "Camera Height", bVisible);
			btmCameraHeight = (tree.GetTree("Camera Height", false, bVisible)).Set(btmCameraHeight, btmCameraHeight, "Bottom", "Camera Height", bVisible);

			topPositionOffset = (tree.GetTree("Camera Position Offset", false, bVisible)).Set(topPositionOffset, topPositionOffset, "Top Camera", "카메라 위치 offset (Degree)", bVisible);
			sidePositionOffset = (tree.GetTree("Camera Position Offset", false, bVisible)).Set(sidePositionOffset, sidePositionOffset, "Side Camera", "카메라 위치 offset (Degree)", bVisible);
			btmPositionOffset = (tree.GetTree("Camera Position Offset", false, bVisible)).Set(btmPositionOffset, btmPositionOffset, "Bottom Camera", "카메라 위치 offset (Degree)", bVisible);

			topImageOffset = (tree.GetTree("Image Offset", false, bVisible)).Set(topImageOffset, topImageOffset, "Top Camera", "Height offset (pxl)", bVisible);
			sideImageOffset = (tree.GetTree("Image Offset", false, bVisible)).Set(sideImageOffset, sideImageOffset, "Side Camera", "Height offset (pxl)", bVisible);
			btmImageOffset = (tree.GetTree("Image Offset", false, bVisible)).Set(btmImageOffset, btmImageOffset, "Bottom Camera", "Heightoffset (pxl)", bVisible);
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