using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RootTools.Database;
using RootTools.OHT;
using RootTools_Vision;

namespace Root_WIND2
{
	public class InspectionManagerEdge : WorkFactory
	{
		public InspectionManagerEdge(IntPtr _sharedBuffer, int _width, int _height, int _byteCnt = 1)
		{
			this.sharedBufferR_Gray = _sharedBuffer;
			this.sharedBufferWidth = _width;
			this.sharedBufferHeight = _height;
			this.sharedBufferByteCnt = _byteCnt;
		}

		private Recipe recipe;
		private IntPtr sharedBufferR_Gray;
		private IntPtr sharedBufferG;
		private IntPtr sharedBufferB;

		private int sharedBufferWidth;
		private int sharedBufferHeight;
		private int sharedBufferByteCnt;

		public Recipe Recipe { get => recipe; set => recipe = value; }
		public IntPtr SharedBufferR_Gray { get => sharedBufferR_Gray; set => sharedBufferR_Gray = value; }
		public IntPtr SharedBufferG { get => sharedBufferG; set => sharedBufferG = value; }
		public IntPtr SharedBufferB { get => sharedBufferB; set => sharedBufferB = value; }

		public int SharedBufferWidth { get => sharedBufferWidth; set => sharedBufferWidth = value; }
		public int SharedBufferHeight { get => sharedBufferHeight; set => sharedBufferHeight = value; }
		public int SharedBufferByteCnt { get => sharedBufferByteCnt; set => sharedBufferByteCnt = value; }

		public void SetWorkplaceBuffer(IntPtr ptrR, IntPtr ptrG, IntPtr ptrB)
		{
			this.SharedBufferR_Gray = ptrR;
			this.SharedBufferG = ptrG;
			this.SharedBufferB = ptrB;
		}

		public bool SetCameraInfo()
		{
			return true;
		}

		#region [Overrides]
		protected override void Initialize()
		{
			CreateWorkManager(WORK_TYPE.INSPECTION, 5);
			CreateWorkManager(WORK_TYPE.DEFECTPROCESS_ALL);
		}

		protected override WorkplaceBundle CreateWorkplaceBundle()
		{
			return CreateWorkplace_Edge();
		}

		protected override WorkBundle CreateWorkBundle()
		{
			WorkBundle workBundle = new WorkBundle();
			EdgeSurface edgeSurface = new EdgeSurface();
			ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();
			
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
			WorkplaceBundle workplaceBundle = new WorkplaceBundle();
			int partitionNum = recipe.GetRecipe<EdgeSurfaceParameter>().RoiHeightTop;  // 2000

			int index = 0;

			// top
			RootTools.ImageData imageDataTop = ProgramManager.Instance.GetEdgeMemory(Module.EdgeSideVision.EDGE_TYPE.EdgeTop);
			int memoryHeightTop = 10000;// imageDataSide.p_Size.Y;
			int memoryWidthTop = imageDataTop.p_Size.X;

			for (int i = 0; i < memoryHeightTop / partitionNum; i++)
			{
				Workplace workplace = new Workplace(0, i, 0, partitionNum * i, memoryWidthTop, partitionNum, index++);
				workplace.SetSharedBuffer(imageDataTop.GetPtr(0), memoryWidthTop, memoryHeightTop, imageDataTop.p_nByte, imageDataTop.GetPtr(1), imageDataTop.GetPtr(2));

				workplaceBundle.Add(workplace);
			}

			// side
			RootTools.ImageData imageDataSide = ProgramManager.Instance.GetEdgeMemory(Module.EdgeSideVision.EDGE_TYPE.EdgeSide);
			int memoryHeightSide = 10000;// imageDataSide.p_Size.Y;
			int memoryWidhtSide = imageDataSide.p_Size.X;
			for (int i = 0; i < memoryHeightSide / partitionNum; i++)
			{
				Workplace workplace = new Workplace((int)EdgeSurface.EdgeMapPositionX.Side, i, 0, memoryHeightSide / partitionNum * i, memoryWidhtSide, partitionNum, index++);
				workplace.SetSharedBuffer(imageDataSide.GetPtr(0), memoryWidhtSide, memoryHeightSide, imageDataSide.p_nByte, imageDataSide.GetPtr(1), imageDataSide.GetPtr(2));

				workplaceBundle.Add(workplace);
			}

			// bottom
			RootTools.ImageData imageDataBtm = ProgramManager.Instance.GetEdgeMemory(Module.EdgeSideVision.EDGE_TYPE.EdgeBottom);
			int memoryHeightBtm = 10000;// imageDataBtm.p_Size.Y;
			int memoryWidhtBtm = imageDataBtm.p_Size.X;
			for (int i = 0; i < memoryHeightBtm / partitionNum; i++)
			{
				Workplace workplace = new Workplace((int)EdgeSurface.EdgeMapPositionX.Btm, i, 0, memoryHeightBtm / partitionNum * i, memoryWidhtBtm, partitionNum, index++);
				workplace.SetSharedBuffer(imageDataBtm.GetPtr(0), memoryWidhtBtm, memoryHeightBtm, imageDataBtm.p_nByte, imageDataBtm.GetPtr(1), imageDataBtm.GetPtr(2));

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
