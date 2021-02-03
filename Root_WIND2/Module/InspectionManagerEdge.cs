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
		#region [Memberws]
		private readonly RecipeEdge recipe;
		private readonly SharedBufferInfo[] bufferInfoArray;
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
			ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();

			foreach (ParameterBase param in paramList)
				edgeSurface.SetParameter(param);

			workBundle.Add(edgeSurface);
			workBundle.Add(processDefect_Wafer);
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
			EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
			Run_GrabEdge grab = (Run_GrabEdge)module.CloneModuleRun("GrabEdge");
			GrabMode top = grab.GetGrabMode(grab.p_sGrabModeTop);
			GrabMode side = grab.GetGrabMode(grab.p_sGrabModeSide);
			GrabMode btm = grab.GetGrabMode(grab.p_sGrabModeBtm);

			// camera 비어있는 첫번쨰 Buffer 검사 영역 제외
			int cameraEmptyBufferHeight_Top = 0;
			int cameraEmptyBufferHeight_Side = 0;
			int cameraEmptyBufferHeight_Btm = 0;
			if (top.m_camera != null)
				cameraEmptyBufferHeight_Top = top.m_camera.GetRoiSize().Y;
			if (side.m_camera != null)
				cameraEmptyBufferHeight_Side = side.m_camera.GetRoiSize().Y;
			if (btm.m_camera != null)
				cameraEmptyBufferHeight_Btm = btm.m_camera.GetRoiSize().Y;

			// 검사 시작/끝 Y좌표 설정
			Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");
			int bufferY_Top = (int)(module.Pulse360 / module.EdgeCamTriggerRatio) + cameraEmptyBufferHeight_Top;    // 360도 memory height
			int bufferY_Side = (int)(module.Pulse360 / module.EdgeCamTriggerRatio) + cameraEmptyBufferHeight_Side;    // 360도 memory height
			int bufferY_Btm = (int)(module.Pulse360 / module.EdgeCamTriggerRatio) + cameraEmptyBufferHeight_Btm;    // 360도 memory height

			int startPtY_Top = inspect.TopOffset * (bufferY_Top / 360);
			int startPtY_Side = inspect.SideOffset * (bufferY_Side / 360);
			int startPtY_Btm = inspect.BtmOffset * (bufferY_Btm / 360);

			int endPtY_Top = bufferY_Top + startPtY_Top;
			int endPtY_Side = bufferY_Side + startPtY_Side;
			int endPtY_Btm = bufferY_Btm + startPtY_Btm;

			// ROI
			int roiWidth_Top = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop.ROIWidth;
			int roiHeight_Top = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop.ROIHeight;
			int roiWidth_Side = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide.ROIWidth;
			int roiHeight_Side = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide.ROIHeight;
			int roiWidth_Btm = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm.ROIWidth;
			int roiHeight_Btm = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm.ROIHeight;


			WorkplaceBundle workplaceBundle = new WorkplaceBundle();
			Workplace tempPlace = new Workplace(-1, -1, 0, 0, 0, 0, workplaceBundle.Count);
			tempPlace.SetSharedBuffer(this.SharedBufferInfoArray[0]);
			workplaceBundle.Add(tempPlace);

			CreateWorkplace_Edge(EdgeSurface.EdgeMapPositionX.Top, cameraEmptyBufferHeight_Top + startPtY_Top, endPtY_Top, roiWidth_Top, roiHeight_Top, ref workplaceBundle);
			CreateWorkplace_Edge(EdgeSurface.EdgeMapPositionX.Side, cameraEmptyBufferHeight_Side + startPtY_Side, endPtY_Side, roiWidth_Side, roiHeight_Side, ref workplaceBundle);
			CreateWorkplace_Edge(EdgeSurface.EdgeMapPositionX.Btm, cameraEmptyBufferHeight_Btm + startPtY_Btm, endPtY_Btm, roiWidth_Btm, roiHeight_Btm, ref workplaceBundle);

			return workplaceBundle;
		}

		public void CreateWorkplace_Edge(EdgeSurface.EdgeMapPositionX mapX, int startY, int endY, int roiWidth, int roiHeight, ref WorkplaceBundle workplaces)
		{
			SharedBufferInfo sharedBufferInfo;

			if (mapX == EdgeSurface.EdgeMapPositionX.Top)
				sharedBufferInfo = this.SharedBufferInfoArray[0];
			else if (mapX == EdgeSurface.EdgeMapPositionX.Side)
				sharedBufferInfo = this.SharedBufferInfoArray[1];
			else if (mapX == EdgeSurface.EdgeMapPositionX.Btm)
				sharedBufferInfo = this.SharedBufferInfoArray[2];
			else
				return;

			for (int i = 0; i < endY / roiHeight; i++)
			{
				int calStartY = (roiHeight * i) + startY;
				int height = roiHeight;
				if ((calStartY + roiHeight) > endY)
					height = endY - calStartY;

				if (height <= 0)
					break;

				Workplace workplace = new Workplace(
						(int)mapX, i,
						0, calStartY,
						roiWidth, height,
						workplaces.Count);
				workplace.SetSharedBuffer(sharedBufferInfo);

				workplaces.Add(workplace);
			}
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
