using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Root_WIND2.Module;
using RootTools;
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
			ProcessDefect_Edge processDefect_Edge = new ProcessDefect_Edge("defect");

			// top/side/btm 각 Set
			foreach (ParameterBase param in paramList)
			{
				edgeSurface.SetParameter(param);
				processDefect_Edge.SetParameter(param);
			}

			//edgeSurface.SetGrabMode(((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop.GrabModeIndex],
			//						((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseSide.GrabModeIndex],
			//						((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseBtm.GrabModeIndex]);
				
			//processDefect_Edge.SetGrabMode(((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop.GrabModeIndex],
			//							   ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseSide.GrabModeIndex],
			//							   ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseBtm.GrabModeIndex]);

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
			
			Workplace tempPlace = new Workplace(0, 0, 0, 0, 0, 0, workplaceBundle.Count);
			tempPlace.SetSharedBuffer(this.SharedBufferInfoArray[0]);
			workplaceBundle.Add(tempPlace);

			CreateWorkplace_Edge(recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop, recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop, EdgeSurface.EdgeMapPositionX.Top, this.SharedBufferInfoArray[0], ref workplaceBundle);
			CreateWorkplace_Edge(recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseSide, recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide, EdgeSurface.EdgeMapPositionX.Side, this.SharedBufferInfoArray[1], ref workplaceBundle);
			CreateWorkplace_Edge(recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseBtm, recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm, EdgeSurface.EdgeMapPositionX.Btm, this.SharedBufferInfoArray[2], ref workplaceBundle);

			return workplaceBundle;
		}

		private void CreateWorkplace_Edge(EdgeSurfaceRecipeBase recipe, EdgeSurfaceParameterBase param, EdgeSurface.EdgeMapPositionX mapX, SharedBufferInfo sharedBufferInfo, ref WorkplaceBundle workplaceBundle)
		{
			GrabModeEdge grabMode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[recipe.GrabModeIndex];
			if (grabMode == null)
			{ 
				MessageBox.Show("Grab Mode를 설정해주세요.");
				return;
			}
			
			int imgHeight = grabMode.m_nImageHeight;    // Image 전체 Height
			int heightPerDegree = (int)(imgHeight / grabMode.m_nScanDegree);    // 1도 Image Height
			int startY = heightPerDegree * grabMode.m_nCameraPositionOffset;
			int endY = (heightPerDegree * 360) + startY;

			int roiWidth = 500;/*param.ROIWidth;*/
			int roiHeight = param.ROIHeight;

			for (int i = 0; i < endY / roiHeight; i++)
			{
				int calcStartY = (roiHeight * i) + startY;
				int calcHeight = roiHeight;

				if ((calcStartY + roiHeight) > imgHeight)
					calcHeight = imgHeight - calcStartY;

				if (calcHeight <= 0) break;

				Workplace workplace = new Workplace((int)mapX, i, 0, calcStartY, roiWidth, calcHeight, workplaceBundle.Count);
				workplace.SetSharedBuffer(sharedBufferInfo);
				workplaceBundle.Add(workplace);
			}

			// Notch to Notch 방식
			/*
			int firstNotch = 0;	// FindNotch
			int lastNotch = 0;	// FindNotch
			int bufferHeight = lastNotch - firstNotch;

			int roiWidth = param.ROIWidth;
			int roiHeight = param.ROIHeight;
			for (int i = 0; i < bufferHeight / roiHeight; i++)
			{
				int calcStartY = (roiHeight * i) + firstNotch;
				int calcHeight = roiHeight;

				if (calcStartY + roiHeight > lastNotch)
					calcHeight = lastNotch - calcStartY;

				if (calcHeight <= 0) break;

				Workplace workplace = new Workplace((int)mapX, i, 0, calcStartY, roiWidth, calcHeight, workplaceBundle.Count);
				workplace.SetSharedBuffer(sharedBufferInfo);
				workplaceBundle.Add(workplace);
			}
			*/
		}

		public int GetWorkplaceCount()
		{
			if (workplaceBundle == null)
				return 1;
			return workplaceBundle.Count();
		}

		public new void Start()
		{
			if (this.Recipe == null)
				return;

			DateTime inspectionStart = DateTime.Now;
			DateTime inspectionEnd = DateTime.Now;
			string lotId = "Lotid";
			string partId = "Partid";
			string setupId = "SetupID";
			string cstId = "CSTid";
			string waferId = "WaferID";
			//string sRecipe = "RecipeID";
			string recipeName = recipe.Name;

			DatabaseManager.Instance.SetLotinfo(inspectionStart, inspectionEnd, lotId, partId, setupId, cstId, waferId, recipeName);

			base.Start();
		}
    }
}
