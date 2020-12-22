using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools.Database;
using RootTools.OHT;
using RootTools_Vision;
using RootTools_Vision.Inspection;

namespace Root_WIND2
{
	public class InspectionManager_EFEM : WorkFactory
	{
		public InspectionManager_EFEM(IntPtr _sharedBuffer, int _width, int _height, int _byteCnt)
		{
			this.sharedBuffer = _sharedBuffer;
			this.sharedBufferWidth = _width;
			this.sharedBufferHeight = _height;
			this.sharedBufferByteCnt = _byteCnt;
		}

		protected override void InitWorkManager()
		{
			//base.InitWorkManager();
			this.Add(new WorkManager("Snap", WORK_TYPE.PREPARISON, WORKPLACE_STATE.SNAP, WORKPLACE_STATE.NONE, STATE_CHECK_TYPE.CHIP, 5));
			this.Add(new WorkManager("EdgeSurface", WORK_TYPE.MAINWORK, WORKPLACE_STATE.INSPECTION, WORKPLACE_STATE.NONE, STATE_CHECK_TYPE.CHIP, 5));
			this.Add(new WorkManager("ProcessDefect", WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS, WORKPLACE_STATE.INSPECTION, STATE_CHECK_TYPE.WAFER));
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
		private IntPtr sharedBuffer;
		private int sharedBufferWidth;
		private int sharedBufferHeight;
		private int sharedBufferByteCnt;

		public Recipe Recipe { get => recipe; set => recipe = value; }
		public IntPtr SharedBuffer { get => sharedBuffer; set => sharedBuffer = value; }
		public int SharedBufferWidth { get => sharedBufferWidth; set => sharedBufferWidth = value; }
		public int SharedBufferHeight { get => sharedBufferHeight; set => sharedBufferHeight = value; }
		public int SharedBufferByteCnt { get => sharedBufferByteCnt; set => sharedBufferByteCnt = value; }

		
		public bool CreateInspection()
		{
			return CreateInspection(this.recipe);
		}

		public override bool CreateInspection(Recipe _recipe)
		{
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
			RootTools.ImageData imageDataSide = ProgramManager.Instance.GetEdgeMemory(Module.EdgeSideVision.EDGE_TYPE.EdgeSide);
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
			for (int i = 0; i<memoryHeightBtm / partitionNum; i++)
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

			/*
			// Top : 0
			WorkplaceBundle workplaces = WorkplaceBundle.CreateWorkplaceBundle(sharedBufferWidth, sharedBufferHeight);
			Workplace edgeTopWorkplace = new Workplace();
			edgeTopWorkplace.SetSharedBuffer(this.SharedBuffer, this.SharedBufferWidth, this.SharedBufferHeight, this.SharedBufferByteCnt);
			workplaces.Add(edgeTopWorkplace);

			// Side : 1
			Workplace edgeSideWorkplace = new Workplace();
			edgeTopWorkplace.SetSharedBuffer(this.SharedBuffer, this.SharedBufferWidth, this.SharedBufferHeight, this.SharedBufferByteCnt);
			workplaces.Add(edgeSideWorkplace);

			// Bottom : 2
			Workplace edgeBottomWorkplace = new Workplace();
			edgeTopWorkplace.SetSharedBuffer(this.SharedBuffer, this.SharedBufferWidth, this.SharedBufferHeight, this.SharedBufferByteCnt);
			workplaces.Add(edgeBottomWorkplace);

			// EBR : 3
			Workplace ebrWorkplace = new Workplace();
			edgeTopWorkplace.SetSharedBuffer(this.SharedBuffer, this.SharedBufferWidth, this.SharedBufferHeight, this.SharedBufferByteCnt);
			workplaces.Add(ebrWorkplace);
			*/

			/*
			WorkplaceBundle workplaces = WorkplaceBundle.CreateWorkplaceBundle(sharedBufferWidth, sharedBufferHeight);
			workplaces.SetSharedBuffer(this.SharedBuffer, this.SharedBufferWidth, this.SharedBufferHeight, this.SharedBufferByteCnt);

			EdgeSurface edgeSurface = new EdgeSurface();
			edgeSurface.SetRecipe(this.recipe);
			edgeSurface.SetWorkplaceBundle(workplaces);

			ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();
			processDefect_Wafer.SetRecipe(this.recipe);
			processDefect_Wafer.SetWorkplaceBundle(workplaces);
			//ProcessDefect processDefect = new ProcessDefect();
			//processDefect.SetData(recipe.GetRecipeData(), recipe.GetParameter());
			//works.Add(processDefect);
			
			WorkBundle works = new WorkBundle();
			works.Add(edgeSurface);
			works.Add(processDefect_Wafer);

			SetBundles(works, workplaces);
			*/
		}

		public override bool CreateInspecion_Backside(Recipe _recipe)
		{
			throw new NotImplementedException();
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
