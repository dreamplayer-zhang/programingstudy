﻿using Root_WIND2.Module;
using RootTools.Database;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_WIND2
{
	public class InspectionManagerEBR : WorkFactory
	{
		#region [Members]
		private readonly RecipeEBR recipe;
		private readonly SharedBufferInfo sharedBufferInfo;

		private WorkplaceBundle workplaceBundle;
		#endregion

		#region [Properties]
		public RecipeEBR Recipe
		{
			get => this.recipe;
		}

		public SharedBufferInfo SharedBufferInfo
		{
			get => this.sharedBufferInfo;
		}
		#endregion

		public InspectionManagerEBR(RecipeEBR _recipe, SharedBufferInfo _bufferInfo)
		{
			this.recipe = _recipe;
			this.sharedBufferInfo = _bufferInfo;
		}

		#region [Overrides]
		protected override void Initialize()
		{
			CreateWorkManager(WORK_TYPE.INSPECTION, 5);
			CreateWorkManager(WORK_TYPE.DEFECTPROCESS_ALL, 1, true);
		}

		protected override WorkplaceBundle CreateWorkplaceBundle()
		{
			int cameraHeight = recipe.GetItem<EBRRecipe>().CameraHeight;
			int bufferHeight = (int)(360000 / recipe.GetItem<EBRRecipe>().TriggerRatio);
			int bufferHeightPerDegree = bufferHeight / 360; // 1도 당 Image Height

			int width = recipe.GetItem<EBRParameter>().ROIWidth;
			int height = recipe.GetItem<EBRParameter>().ROIHeight;
			int stepDegree = recipe.GetItem<EBRParameter>().StepDegree;
			int workplaceCnt = 360 / stepDegree;

			workplaceBundle = new WorkplaceBundle();
			workplaceBundle.Add(new Workplace(0, 0, 0, 0, 0, 0, workplaceBundle.Count));
			
			for (int i = 0; i < workplaceCnt; i++)
			{
				int posY = (bufferHeightPerDegree * i) - (height / 2);
				Workplace workplace = new Workplace(0, 0, 0, posY + cameraHeight, width, height, workplaceBundle.Count);
				workplaceBundle.Add(workplace);
			}

			workplaceBundle.SetSharedBuffer(this.sharedBufferInfo);
			return workplaceBundle;
		}

		protected override WorkBundle CreateWorkBundle()
		{
			List<ParameterBase> paramList = recipe.ParameterItemList;
			WorkBundle workBundle = new WorkBundle();
			EBR ebr = new EBR();
			ProcessMeasurement processMeasurement = new ProcessMeasurement();

			foreach (ParameterBase param in paramList)
				ebr.SetParameter(param);

			workBundle.Add(ebr);
			workBundle.Add(processMeasurement);
			workBundle.SetRecipe(recipe);

			return workBundle;
		}
		public int GetWorkplaceCount()
		{
			return workplaceBundle.Count();
		}

		protected override bool Ready(WorkplaceBundle workplaces, WorkBundle works)
		{
			return true;
		}
		#endregion

		public new void Start()
		{
			if (this.Recipe == null)
				return;

			string lotId = "Lotid";
			string partId = "Partid";
			string setupId = "SetupID";
			string cstId = "CSTid";
			string waferId = "WaferID";
			//string sRecipe = "RecipeID";
			string recipeName = recipe.Name;

			DatabaseManager.Instance.SetLotinfo(lotId, partId, setupId, cstId, waferId, recipeName);

			base.Start();
		}
    }
}
