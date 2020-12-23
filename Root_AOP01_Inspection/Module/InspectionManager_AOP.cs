using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_AOP01_Inspection.Module
{
	public class InspectionManager_AOP : WorkFactory
	{
		#region Field

		private IntPtr sharedBuffer;
		private int sharedBufferWidth;
		private int sharedBufferHeight;
		private int sharedBufferByteCnt;

		#endregion

		#region Property

		private InspectionMode inspectionMode;
		public InspectionMode InspectionMode { get => inspectionMode; set => inspectionMode = value; }

		//private Recipe recipe;
		//public Recipe Recipe { get => recipe; set => recipe = value; }

		public IntPtr SharedBuffer { get => sharedBuffer; set => sharedBuffer = value; }
		public int SharedBufferWidth { get => sharedBufferWidth; set => sharedBufferWidth = value; }
		public int SharedBufferHeight { get => sharedBufferHeight; set => sharedBufferHeight = value; }
		public int SharedBufferByteCnt { get => sharedBufferByteCnt; set => sharedBufferByteCnt = value; }

		#endregion

		public InspectionManager_AOP(IntPtr _sharedBuffer, int _width, int _height, int _byteCnt)
		{
			this.sharedBuffer = _sharedBuffer;
			this.sharedBufferWidth = _width;
			this.sharedBufferHeight = _height;
			this.sharedBufferByteCnt = _byteCnt;
		}
		public override bool CreateInspecion_Backside(Recipe _recipe)
		{
			return false;//사용하지 않음? 할수도있음 ㅇㅇ
		}
		public bool CreateInspection()
		{
			//검사영역생성. 레시피 없을때?

			return true;
		}
		public override bool CreateInspection(Recipe _recipe)
		{
			//검사영역생성. 레시피 있을때?

			int partitionNum = 10;  // recipe?
			WorkplaceBundle workplaces = new WorkplaceBundle();

			// top
			int memoryHeightTop = this.SharedBufferHeight;
			int memoryWidthTop = this.SharedBufferWidth;

			for (int i = 0; i < memoryHeightTop / partitionNum; i++)
			{
				Workplace workplace = new Workplace(0, i, 0, memoryHeightTop / partitionNum * i, memoryWidthTop, memoryHeightTop / partitionNum);
				workplace.SetSharedBuffer(this.SharedBuffer, memoryWidthTop, this.SharedBufferHeight, this.SharedBufferByteCnt);
				workplaces.Add(workplace);
			}

			// side
			RootTools.ImageData imageDataSide = ProgramManager.Instance.GetMemory();
			int memoryHeightSide = imageDataSide.p_Size.Y;
			int memoryWidhtSide = imageDataSide.p_Size.X;
			for (int i = 0; i < memoryHeightSide / partitionNum; i++)
			{
				Workplace workplace = new Workplace(0, i, 0, memoryHeightSide / partitionNum * i, memoryWidhtSide, memoryHeightSide / partitionNum);
				workplace.SetSharedBuffer(imageDataSide.GetPtr(), memoryWidhtSide, memoryHeightSide, imageDataSide.p_nByte);
				workplaces.Add(workplace);
			}

			// bottom
			RootTools.ImageData imageDataBtm = ProgramManager.Instance.GetEdgeMemory(Module.EdgeSideVision.EDGE_TYPE.EdgeSide);
			int memoryHeightBtm = imageDataBtm.p_Size.Y;
			int memoryWidhtBtm = imageDataBtm.p_Size.X;
			for (int i = 0; i < memoryHeightBtm / partitionNum; i++)
			{
				Workplace workplace = new Workplace(0, i, 0, memoryHeightBtm / partitionNum * i, memoryWidhtBtm, memoryHeightBtm / partitionNum);
				workplace.SetSharedBuffer(imageDataBtm.GetPtr(), memoryWidhtBtm, memoryHeightBtm, imageDataBtm.p_nByte);
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
			return true;
		}

		protected override void InitWorkManager()
		{
			//초기화
			this.Add(new WorkManager("Snap", WORK_TYPE.ALIGNMENT, WORKPLACE_STATE.SNAP, WORKPLACE_STATE.NONE, STATE_CHECK_TYPE.CHIP, 5));
			//this.Add(new WorkManager("EdgeSurface", WORK_TYPE.INSPECTION, WORKPLACE_STATE.INSPECTION, WORKPLACE_STATE.NONE, STATE_CHECK_TYPE.CHIP, 5));//검사부에 맞춰서 넣어야하는것으로 보이는데...
			this.Add(new WorkManager("ProcessDefect", WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS, WORKPLACE_STATE.INSPECTION, STATE_CHECK_TYPE.WAFER));
		}
	}
}
