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

		public void CreateInspection_Edgeside()
		{
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

        public override bool CreateInspection(Recipe _recipe)
        {
            throw new NotImplementedException();
        }
    }
}
