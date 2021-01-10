using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools.Database;
using RootTools.OHT;
using RootTools_Vision;

namespace Root_WIND2
{
	public class InspectionManagerEdge : WorkFactory
	{
		public InspectionManagerEdge(IntPtr _sharedBuffer, int _width, int _height, int _byteCnt)
		{
			this.sharedBufferR_Gray = _sharedBuffer;
			this.sharedBufferWidth = _width;
			this.sharedBufferHeight = _height;
			this.sharedBufferByteCnt = _byteCnt;
		}

		protected override void InitWorkManager()
		{
			this.Add(new WorkManager("Snap", WORK_TYPE.ALIGNMENT, WORKPLACE_STATE.SNAP, WORKPLACE_STATE.NONE, STATE_CHECK_TYPE.CHIP, 5));
			this.Add(new WorkManager("EdgeSurface", WORK_TYPE.INSPECTION, WORKPLACE_STATE.INSPECTION, WORKPLACE_STATE.NONE, STATE_CHECK_TYPE.CHIP, 5));
			this.Add(new WorkManager("ProcessDefect", WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS, WORKPLACE_STATE.INSPECTION, STATE_CHECK_TYPE.WAFER));

			WIND2EventManager.SnapDone += SnapDone_Callback;
		}

		public enum InsepectionMode
		{
			EDGE,
			//BACK,
			//EBR,
		}

		private InsepectionMode inspectionMode = InsepectionMode.EDGE;
		public InsepectionMode InspectionMode { get => inspectionMode; set => inspectionMode = value; }

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

		
		public bool CreateInspection()
		{
			return CreateInspection(this.recipe);
		}

		public override bool CreateInspection(Recipe _recipe)
		{
			int partitionNum = 1000;  // recipe
			WorkplaceBundle workplaces = new WorkplaceBundle();

			// top
			int memoryHeightTop = this.SharedBufferHeight;
			int memoryWidthTop = this.SharedBufferWidth;
			for (int i = 0; i < memoryHeightTop / partitionNum; i++)
			{
				Workplace workplace = new Workplace(0, i, 0, memoryHeightTop / partitionNum * i, memoryWidthTop, memoryHeightTop / partitionNum);
				workplace.SetSharedBuffer(this.SharedBufferR_Gray, memoryWidthTop, this.SharedBufferHeight, this.SharedBufferByteCnt);
				workplace.SetSharedRGBBuffer(this.SharedBufferR_Gray, this.sharedBufferG, this.sharedBufferB);
				workplaces.Add(workplace);
			}

			// side
			RootTools.ImageData imageDataSide = ProgramManager.Instance.GetEdgeMemory(Module.EdgeSideVision.EDGE_TYPE.EdgeSide);
			int memoryHeightSide = imageDataSide.p_Size.Y;
			int memoryWidhtSide = imageDataSide.p_Size.X;
			for (int i = 0; i < memoryHeightSide / partitionNum; i++)
			{
				Workplace workplace = new Workplace(0, i, 0, memoryHeightSide / partitionNum * i, memoryWidhtSide, memoryHeightSide / partitionNum);
				workplace.SetSharedBuffer(imageDataSide.GetPtr(0), memoryWidhtSide, memoryHeightSide, imageDataSide.p_nByte);
				workplace.SetSharedRGBBuffer(imageDataSide.GetPtr(0), imageDataSide.GetPtr(1), imageDataSide.GetPtr(2));
				workplaces.Add(workplace);
			}

			// bottom
			RootTools.ImageData imageDataBtm = ProgramManager.Instance.GetEdgeMemory(Module.EdgeSideVision.EDGE_TYPE.EdgeSide);
			int memoryHeightBtm = imageDataBtm.p_Size.Y;
			int memoryWidhtBtm = imageDataBtm.p_Size.X;
			for (int i = 0; i<memoryHeightBtm / partitionNum; i++)
			{
				Workplace workplace = new Workplace(0, i, 0, memoryHeightBtm / partitionNum * i, memoryWidhtBtm, memoryHeightBtm / partitionNum);
				workplace.SetSharedBuffer(imageDataBtm.GetPtr(0), memoryWidhtBtm, memoryHeightBtm, imageDataBtm.p_nByte);
				workplace.SetSharedRGBBuffer(imageDataBtm.GetPtr(0), imageDataBtm.GetPtr(1), imageDataBtm.GetPtr(2));

				workplaces.Add(workplace);
			}

			EdgeSurface edgeSurface = new EdgeSurface();
			edgeSurface.SetRecipe(this.recipe);
			edgeSurface.SetWorkplaceBundle(workplaces);

			ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();
			processDefect_Wafer.SetRecipe(this.recipe);
			processDefect_Wafer.SetWorkplaceBundle(workplaces);

			WorkBundle works = new WorkBundle();
			works.Add(edgeSurface);
			works.Add(processDefect_Wafer);

			SetBundles(works, workplaces);
			return true;
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

		public new void Stop()
		{
			base.Stop();
		}

		private void SnapDone_Callback(object sender, SnapDoneArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
