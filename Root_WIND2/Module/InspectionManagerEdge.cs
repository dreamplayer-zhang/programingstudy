using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Root_WIND2.Module;
using RootTools.Database;
using RootTools.OHT;
using RootTools_Vision;

namespace Root_WIND2
{
	public class InspectionManagerEdge : WorkFactory
	{
		#region [Members]
		private readonly RecipeEdge recipe;
		private readonly SharedBufferInfo[] bufferInfoArray;

		private WorkplaceBundle workplaceBundle;
		#endregion

		#region [Properties]
		public RecipeEdge Recipe
        {
			get => this.recipe;
        }
		public SharedBufferInfo[] SharedBufferInfoArray
		{
			get => this.bufferInfoArray;
        }
		#endregion

		public InspectionManagerEdge(RecipeEdge _recipe, SharedBufferInfo[] _bufferInfo)
		{
			this.recipe = _recipe;
			this.bufferInfoArray = _bufferInfo;
		}

		#region [Overrides]
		protected override void Initialize()
		{
			CreateWorkManager(WORK_TYPE.INSPECTION, 5);
			CreateWorkManager(WORK_TYPE.DEFECTPROCESS_ALL, 1, true);
		}

		protected override WorkplaceBundle CreateWorkplaceBundle()
		{
			return CreateWorkplace_Edge();
		}

		protected override WorkBundle CreateWorkBundle()
		{
			List<ParameterBase> paramList = recipe.ParameterItemList;
			WorkBundle workBundle = new WorkBundle();
			EdgeSurface edgeSurface = new EdgeSurface();
			ProcessDefect_Edge processDefect_Edge = new ProcessDefect_Edge();

			foreach (ParameterBase param in paramList)
				edgeSurface.SetParameter(param);

			workBundle.Add(edgeSurface);
			workBundle.Add(processDefect_Edge);
			workBundle.SetRecipe(this.Recipe);

			return workBundle;
		}

		protected override bool Ready(WorkplaceBundle workplaces, WorkBundle works)
		{
			return true;
		}
		#endregion

		private WorkplaceBundle CreateWorkplace_Edge()
		{
			workplaceBundle = new WorkplaceBundle();
			CreateWorkplace_Edge(recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop, recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop, EdgeSurface.EdgeMapPositionX.Top, this.SharedBufferInfoArray[0], ref workplaceBundle);
			CreateWorkplace_Edge(recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseSide, recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide, EdgeSurface.EdgeMapPositionX.Side, this.SharedBufferInfoArray[1], ref workplaceBundle);
			CreateWorkplace_Edge(recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseBtm, recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm, EdgeSurface.EdgeMapPositionX.Btm, this.SharedBufferInfoArray[2], ref workplaceBundle);

			return workplaceBundle;
		}

		private void CreateWorkplace_Edge(EdgeSurfaceRecipeBase recipe, EdgeSurfaceParameterBase param, EdgeSurface.EdgeMapPositionX mapX, SharedBufferInfo sharedBufferInfo, ref WorkplaceBundle workplaceBundle)
		{
			// 360도 memory height
			int bufferHeight = (int)(360000 / recipe.TriggerRatio) + recipe.ImageOffset;
			// 검사 시작/끝 Y좌표 설정
			int startY = recipe.PositionOffset * (bufferHeight / 360);
			int endY = bufferHeight + startY;
			// ROI
			int roiWidth = param.ROIWidth;
			int roiHeight = param.ROIHeight;

			Workplace tempPlace = new Workplace((int)mapX, -1, 0, 0, 0, 0, workplaceBundle.Count);
			tempPlace.SetSharedBuffer(sharedBufferInfo);
			workplaceBundle.Add(tempPlace);

			for (int i = 0; i < bufferHeight / roiHeight; i++)
			{
				int calcStartY = (roiHeight * i) + startY + recipe.ImageOffset;
				int calcHeight = roiHeight;

				if ((calcStartY + roiHeight) > endY)
					calcHeight = endY - calcStartY;

				if (calcHeight <= 0) break;

				Workplace workplace = new Workplace((int)mapX, i, 0, calcStartY, roiWidth, calcHeight, workplaceBundle.Count);
				workplace.SetSharedBuffer(sharedBufferInfo);

				workplaceBundle.Add(workplace);
			}
		}

		public int GetWorkplaceCount()
		{
			return workplaceBundle.Count();
		}

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
