﻿using RootTools.Module;
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

		// degree, 카메라 위치 각도 offset
		int topOffset = 0;
		int sideOffset = 45;
		int btmOffset = 90;

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
				if (this.inspectionEdge.Recipe.Read(recipeName, true) == false)
					return "Recipe Open Fail";

				if (this.inspectionEdge.SetCameraInfo() == false)
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