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

		// camera height
		int topCameraHeight = 2000;
		int sideCameraHeight = 2000;
		int btmCameraHeight = 2000;

		// degree, 카메라 위치 각도 offset
		int topPositionOffset = 0;
		int sidePositionOffset = 45;
		int btmPositionOffset = 90;

		// 이미지 시작 지점 offset
		int topImageOffset = 0;
		int sideImageOffset = 0;
		int btmImageOffset = 0;

		int imageHeight = 0;

		#region [Getter/Setter]
		public string RecipeName
		{
			get => recipeName;
			set => recipeName = value;
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

		public int ImageHeight
		{
			get => imageHeight;
			set => imageHeight = value;
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

			run.topCameraHeight = topCameraHeight;
			run.sideCameraHeight = sideCameraHeight;
			run.btmCameraHeight = btmCameraHeight;

			run.topPositionOffset = topPositionOffset;
			run.sidePositionOffset = sidePositionOffset;
			run.btmPositionOffset = btmPositionOffset;

			run.topImageOffset = topImageOffset;
			run.sideImageOffset = sideImageOffset;
			run.btmImageOffset = btmImageOffset;

			run.imageHeight = imageHeight;
			return run;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
		{
			recipeName = tree.SetFile(recipeName, recipeName, "rcp", "Recipe", "Recipe Name", bVisible);
			topCameraHeight = (tree.GetTree("Camera Height", false, bVisible)).Set(topCameraHeight, topCameraHeight, "Top", "Camera Height", bVisible);
			sideCameraHeight = (tree.GetTree("Camera Height", false, bVisible)).Set(sideCameraHeight, sideCameraHeight, "Side", "Camera Height", bVisible);
			btmCameraHeight = (tree.GetTree("Camera Height", false, bVisible)).Set(btmCameraHeight, btmCameraHeight, "Bottom", "Camera Height", bVisible);

			topPositionOffset = (tree.GetTree("Position Offset", false, bVisible)).Set(topPositionOffset, topPositionOffset, "Top", "카메라 위치 offset (Degree)", bVisible);
			sidePositionOffset = (tree.GetTree("Position Offset", false, bVisible)).Set(sidePositionOffset, sidePositionOffset, "Side", "카메라 위치 offset (Degree)", bVisible);
			btmPositionOffset = (tree.GetTree("Position Offset", false, bVisible)).Set(btmPositionOffset, btmPositionOffset, "Bottom", "카메라 위치 offset (Degree)", bVisible);

			topImageOffset = (tree.GetTree("Image Offset", false, bVisible)).Set(topImageOffset, topImageOffset, "Top", "Height offset (pxl)", bVisible);
			sideImageOffset = (tree.GetTree("Image Offset", false, bVisible)).Set(sideImageOffset, sideImageOffset, "Side", "Height offset (pxl)", bVisible);
			btmImageOffset = (tree.GetTree("Image Offset", false, bVisible)).Set(btmImageOffset, btmImageOffset, "Bottom", "Heightoffset (pxl)", bVisible);

			imageHeight = (tree.GetTree("Image Height", false, bVisible)).Set(imageHeight, imageHeight, "Image Height", "전체 Image Height", bVisible);
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