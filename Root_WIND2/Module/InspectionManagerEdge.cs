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

			int cameraEmptyBufferHeight_Top = 0;
			int cameraEmptyBufferHeight_Side = 0;
			int cameraEmptyBufferHeight_Btm = 0;

			if (top.m_camera != null)
				cameraEmptyBufferHeight_Top = top.m_camera.GetRoiSize().Y;
			if (side.m_camera != null)
				cameraEmptyBufferHeight_Side = side.m_camera.GetRoiSize().Y;
			if (btm.m_camera != null)
				cameraEmptyBufferHeight_Btm = btm.m_camera.GetRoiSize().Y;

			WorkplaceBundle workplaceBundle = new WorkplaceBundle();

			Workplace tempPlace = new Workplace(-1, -1, 0, 0, 0, 0, workplaceBundle.Count);
			tempPlace.SetSharedBuffer(this.SharedBufferInfoArray[0]);
			workplaceBundle.Add(tempPlace);
						
			// top
			int memoryHeightTop = this.SharedBufferInfoArray[0].Height;
			int roiWidthTop = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop.ROIWidth;
			int roiHeightTop = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop.ROIHeight;
			for (int i = 0; i < memoryHeightTop / roiHeightTop; i++)
			{
				Workplace workplace = new Workplace((int)EdgeSurface.EdgeMapPositionX.Top, i, 0, (roiHeightTop * i) + cameraEmptyBufferHeight_Top, roiWidthTop, roiHeightTop, workplaceBundle.Count);
				workplace.SetSharedBuffer(this.SharedBufferInfoArray[0]);

				workplaceBundle.Add(workplace);
			}

			// side
			int memoryHeightSide = this.SharedBufferInfoArray[1].Height;
			int roiWidthSide = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide.ROIWidth;
			int roiHeightSide = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide.ROIHeight;
			for (int i = 0; i < memoryHeightSide / roiHeightSide; i++)
			{
				Workplace workplace = new Workplace((int)EdgeSurface.EdgeMapPositionX.Side, i, 0, (roiHeightSide * i) + cameraEmptyBufferHeight_Side, roiWidthSide, roiHeightSide, workplaceBundle.Count);
				workplace.SetSharedBuffer(this.SharedBufferInfoArray[1]);

				workplaceBundle.Add(workplace);
			}

			// bottom
			int memoryHeightBtm = this.SharedBufferInfoArray[2].Height;
			int roiWidthBtm = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm.ROIWidth;
			int roiHeightBtm = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm.ROIHeight;
			for (int i = 0; i < memoryHeightBtm / roiHeightBtm; i++)
			{
				Workplace workplace = new Workplace((int)EdgeSurface.EdgeMapPositionX.Btm, i, 0, (roiHeightBtm * i) + cameraEmptyBufferHeight_Btm, roiWidthBtm, roiHeightBtm, workplaceBundle.Count);
				workplace.SetSharedBuffer(this.SharedBufferInfoArray[2]);

				workplaceBundle.Add(workplace);
			}

			return workplaceBundle;
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
