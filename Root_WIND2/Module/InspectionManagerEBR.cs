using RootTools.Database;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_WIND2
{
	public class InspectionManagerEBR : WorkFactory
	{
		public InspectionManagerEBR(IntPtr _sharedBuffer, int _width, int _height, int _byteCnt = 1)
		{
			this.sharedBufferR_Gray = _sharedBuffer;
			this.sharedBufferWidth = _width;
			this.sharedBufferHeight = _height;
			this.sharedBufferByteCnt = _byteCnt;
		}

		public enum InsepectionMode
		{
			EBR,
		}

		private InsepectionMode inspectionMode = InsepectionMode.EBR;
		public InsepectionMode InspectionMode { get => inspectionMode; set => inspectionMode = value; }

		private Recipe recipe;
		private IntPtr sharedBufferR_Gray;
		private int sharedBufferWidth;
		private int sharedBufferHeight;
		private int sharedBufferByteCnt;

		public Recipe Recipe { get => recipe; set => recipe = value; }
		public IntPtr SharedBufferR_Gray { get => sharedBufferR_Gray; set => sharedBufferR_Gray = value; }
		public int SharedBufferWidth { get => sharedBufferWidth; set => sharedBufferWidth = value; }
		public int SharedBufferHeight { get => sharedBufferHeight; set => sharedBufferHeight = value; }
		public int SharedBufferByteCnt { get => sharedBufferByteCnt; set => sharedBufferByteCnt = value; }

		#region [Overrides]

		protected override void Initialize()
		{
			CreateWorkManager(WORK_TYPE.INSPECTION, 5);
			CreateWorkManager(WORK_TYPE.DEFECTPROCESS_ALL, 1, true);
		}

		protected override WorkplaceBundle CreateWorkplaceBundle()
		{
			WorkplaceBundle workplaceBundle = new WorkplaceBundle();
			int notchY = recipe.GetRecipe<EBRParameter>().NotchY; // notch memory Y 좌표
			int stepDegree = recipe.GetRecipe<EBRParameter>().StepDegree;
			int workplaceCnt = 360 / stepDegree;
			int imageHeight = 270000;
			int imageHeightPerDegree = imageHeight / 360; // 1도 당 Image Height

			int width = recipe.GetRecipe<EBRParameter>().RoiWidth;
			int height = recipe.GetRecipe<EBRParameter>().RoiHeight;

			int index = 0;
			workplaceBundle.Add(new Workplace(0, 0, 0, 0, 0, 0, index++));
			for (int i = 0; i < workplaceCnt; i++)
			{
				int posY = (imageHeightPerDegree * i) - (height / 2);
				if (posY <= 0)
					posY = 0;
				Workplace workplace = new Workplace(0, 0, 0, posY, width, height, index++);
				workplaceBundle.Add(workplace);
			}
			workplaceBundle.SetSharedBuffer(this.sharedBufferR_Gray, this.sharedBufferWidth, this.sharedBufferHeight, this.sharedBufferByteCnt, IntPtr.Zero, IntPtr.Zero);

			return workplaceBundle;
		}

		protected override WorkBundle CreateWorkBundle()
		{
			WorkBundle workBundle = new WorkBundle();
			EBR ebr = new EBR();
			ProcessMeasurement processMeasurement = new ProcessMeasurement();

			workBundle.Add(ebr);
			workBundle.Add(processMeasurement);
			workBundle.SetRecipe(recipe);

			return workBundle;
		}

		protected override bool Ready(WorkplaceBundle workplaces, WorkBundle works)
		{
			return true;
		}
		#endregion

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
