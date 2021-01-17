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
		#region [Member Variables]
		WorkBundle workBundle;
		WorkplaceBundle workplaceBundle;
		#endregion

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

		protected override void InitWorkManager()
		{
			this.Add(new WorkManager("EBR", WORK_TYPE.INSPECTION, WORK_TYPE.NONE, STATE_CHECK_TYPE.CHIP, 5));
			//this.Add(new WorkManager("ProcessDefect", WORK_TYPE.DEFECTPROCESS_WAFER, WORK_TYPE.INSPECTION, STATE_CHECK_TYPE.WAFER));
			this.Add(new WorkManager("ProcessMeasurement", WORK_TYPE.MEASUREMENTPROCESS, WORK_TYPE.INSPECTION, STATE_CHECK_TYPE.WAFER));
		}

		public bool CreateInspection()
		{
			return CreateInspection(this.recipe);
		}

		public override bool CreateInspection(Recipe _recipe)
		{
			workBundle = new WorkBundle();
			workplaceBundle = new WorkplaceBundle();

			int notchY = _recipe.GetRecipe<EBRParameter>().NotchY; // notch memory Y 좌표
			int stepDegree = _recipe.GetRecipe<EBRParameter>().StepDegree;
			int workplaceCnt = 360 / stepDegree;
			int imageHeight = 270000;
			int imageHeightPerDegree = imageHeight / 360; // 1도 당 Image Height

			int width = _recipe.GetRecipe<EBRParameter>().RoiWidth;
			int height = _recipe.GetRecipe<EBRParameter>().RoiHeight;

			try
			{
				for (int i = 0; i < 5/*workplaceCnt*/; i++)
				{
					int posY = (imageHeightPerDegree * i) - (height / 2);
					if (posY <= 0)
						posY = 0;
					Workplace workplace = new Workplace(0, 0, 0, posY, width, height);
					workplaceBundle.Add(workplace);
				}
				workplaceBundle.SetSharedBuffer(this.sharedBufferR_Gray, this.SharedBufferWidth, this.SharedBufferHeight);

				EBR ebr = new EBR();
				ebr.SetRecipe(this.recipe);
				ebr.SetWorkplaceBundle(workplaceBundle);

				ProcessMeasurement processMeasurement = new ProcessMeasurement();
				processMeasurement.SetRecipe(this.recipe);
				processMeasurement.SetWorkplaceBundle(workplaceBundle);

				workBundle.Add(ebr);
				workBundle.Add(processMeasurement);

				if (this.SetBundles(workBundle, workplaceBundle) == false)
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
