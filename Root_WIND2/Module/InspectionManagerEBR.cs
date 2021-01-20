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
		#region [Members]
		private readonly RecipeEBR recipe;
		private readonly SharedBufferInfo sharedBufferInfo;
		#endregion

		#region [Properties]
		public RecipeEBR Recipe
		{
			get => this.recipe;
		}

		public SharedBufferInfo SharedBufferInfo
		{
			get => this.sharedBufferInfo;
		}
		#endregion

		public InspectionManagerEBR(RecipeEBR _recipe, SharedBufferInfo _bufferInfo)
		{
			this.recipe = _recipe;
			this.sharedBufferInfo = _bufferInfo;

		}

		#region [Overrides]

		protected override void Initialize()
		{
			CreateWorkManager(WORK_TYPE.INSPECTION, 5);
			CreateWorkManager(WORK_TYPE.DEFECTPROCESS_ALL, 1, true);
		}

		protected override WorkplaceBundle CreateWorkplaceBundle()
		{
			WorkplaceBundle workplaceBundle = new WorkplaceBundle();
			int notchY = recipe.GetItem<EBRParameter>().NotchY; // notch memory Y 좌표
			int stepDegree = recipe.GetItem<EBRParameter>().StepDegree;
			int workplaceCnt = 360 / stepDegree;
			int imageHeight = 270000;
			int imageHeightPerDegree = imageHeight / 360; // 1도 당 Image Height

			int width = recipe.GetItem<EBRParameter>().RoiWidth;
			int height = recipe.GetItem<EBRParameter>().RoiHeight;

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
