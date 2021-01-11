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
		#region [Member Variables]
		WorkBundle workBundle;
		WorkplaceBundle workplaceBundle;

		#endregion

		public InspectionManagerEdge(IntPtr _sharedBuffer, int _width, int _height, int _byteCnt = 1)
		{
			this.sharedBufferR_Gray = _sharedBuffer;
			this.sharedBufferWidth = _width;
			this.sharedBufferHeight = _height;
			this.sharedBufferByteCnt = _byteCnt;
		}

		protected override void InitWorkManager()
		{
			//this.Add(new WorkManager("Snap", WORK_TYPE.SNAP, WORK_TYPE.NONE, STATE_CHECK_TYPE.CHIP, 5));
			this.Add(new WorkManager("EdgeSurface", WORK_TYPE.INSPECTION, WORK_TYPE.NONE, STATE_CHECK_TYPE.CHIP, 5));
			this.Add(new WorkManager("ProcessDefect", WORK_TYPE.DEFECTPROCESS_WAFER, WORK_TYPE.INSPECTION, STATE_CHECK_TYPE.WAFER));
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

		public void SetWorkplaceBuffer(IntPtr ptrR, IntPtr ptrG, IntPtr ptrB)
		{
			this.SharedBufferR_Gray = ptrR;
			this.SharedBufferG = ptrG;
			this.SharedBufferB = ptrB;
		}

		public bool CreateInspection()
		{
			return CreateInspection(this.recipe);
		}

		public override bool CreateInspection(Recipe _recipe)
		{
			workBundle = new WorkBundle();
			workplaceBundle = new WorkplaceBundle();

			int partitionNum = 2000;  // recipe
			try 
			{
				// top
				int memoryHeightTop = 10000;// this.SharedBufferHeight;
				int memoryWidthTop = this.SharedBufferWidth;
				
				for (int i = 0; i < memoryHeightTop / partitionNum; i++)
				{
					Workplace workplace = new Workplace(0, i, 0, partitionNum * i, memoryWidthTop, partitionNum);
					workplace.SetSharedBuffer(this.SharedBufferR_Gray, memoryWidthTop, this.SharedBufferHeight, this.SharedBufferByteCnt);
					workplace.SetSharedRGBBuffer(this.SharedBufferR_Gray, this.sharedBufferG, this.sharedBufferB);

					workplaceBundle.Add(workplace);
				}
				workplaceBundle[0].SetSharedBuffer(this.SharedBufferR_Gray, memoryWidthTop, this.SharedBufferHeight, this.SharedBufferByteCnt);
				workplaceBundle[0].SetSharedRGBBuffer(this.SharedBufferR_Gray, this.sharedBufferG, this.sharedBufferB);

				/*
				// side
				RootTools.ImageData imageDataSide = ProgramManager.Instance.GetEdgeMemory(Module.EdgeSideVision.EDGE_TYPE.EdgeSide);
				int memoryHeightSide = imageDataSide.p_Size.Y;
				int memoryWidhtSide = imageDataSide.p_Size.X;
				for (int i = 0; i < memoryHeightSide / partitionNum; i++)
				{
					Workplace workplace = new Workplace(0, i, 0, memoryHeightSide / partitionNum * i, memoryWidhtSide, partitionNum);
					workplace.SetSharedBuffer(imageDataSide.GetPtr(0), memoryWidhtSide, memoryHeightSide, imageDataSide.p_nByte);
					workplace.SetSharedRGBBuffer(imageDataSide.GetPtr(0), imageDataSide.GetPtr(1), imageDataSide.GetPtr(2));

					workplaceBundle.Add(workplace);
				}

				// bottom
				RootTools.ImageData imageDataBtm = ProgramManager.Instance.GetEdgeMemory(Module.EdgeSideVision.EDGE_TYPE.EdgeBottom);
				int memoryHeightBtm = imageDataBtm.p_Size.Y;
				int memoryWidhtBtm = imageDataBtm.p_Size.X;
				for (int i = 0; i < memoryHeightBtm / partitionNum; i++)
				{
					Workplace workplace = new Workplace(0, i, 0, memoryHeightBtm / partitionNum * i, memoryWidhtBtm, partitionNum);
					workplace.SetSharedBuffer(imageDataBtm.GetPtr(0), memoryWidhtBtm, memoryHeightBtm, imageDataBtm.p_nByte);
					workplace.SetSharedRGBBuffer(imageDataBtm.GetPtr(0), imageDataBtm.GetPtr(1), imageDataBtm.GetPtr(2));

					workplaceBundle.Add(workplace);
				}
				*/

				EdgeSurface edgeSurface = new EdgeSurface();
				edgeSurface.SetRecipe(this.recipe);
				edgeSurface.SetWorkplaceBundle(workplaceBundle);

				ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();
				processDefect_Wafer.SetRecipe(this.recipe);
				processDefect_Wafer.SetWorkplaceBundle(workplaceBundle);

				// 이걸로 바꿔야함
				//ProcessDefect processDefect = new ProcessDefect();
				//processDefect.SetRecipe(this.recipe);
				//processDefect.SetWorkplaceBundle(workplaceBundle);

				workBundle.Add(edgeSurface);
				workBundle.Add(processDefect_Wafer);
				//workBundle.Add(processDefect);

				if (this.SetBundles(this.workBundle, this.workplaceBundle) == false)
					return false;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Inspection 생성에 실패하였습니다.\n호출함수 : " + MethodBase.GetCurrentMethod().Name + "\nDetail : " + ex.Message);
				return false;
			}

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
	}
}
