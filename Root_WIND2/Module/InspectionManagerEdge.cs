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
			int partitionNum = recipe.GetItem<EdgeSurfaceParameter>().RoiHeightTop;  // 2000

			int index = 0;

			// top
			int memoryHeightTop = this.SharedBufferInfoArray[0].Height;
			int memoryWidthTop = this.SharedBufferInfoArray[0].Width;

			workplaceBundle.Add(new Workplace(-1, -1, 0, 0, 0, 0, index++));
			for (int i = 0; i < memoryHeightTop / partitionNum; i++)
			{
				Workplace workplace = new Workplace(0, i, 0, partitionNum * i, memoryWidthTop, partitionNum, index++);
				workplace.SetSharedBuffer(this.SharedBufferInfoArray[0]);

				workplaceBundle.Add(workplace);
			}

			// side
			int memoryHeightSide = 10000;// imageDataSide.p_Size.Y;
			int memoryWdithSide = this.SharedBufferInfoArray[1].Width;
			for (int i = 0; i < memoryHeightSide / partitionNum; i++)
			{
				Workplace workplace = new Workplace((int)EdgeSurface.EdgeMapPositionX.Side, i, 0, memoryHeightSide / partitionNum * i, memoryWdithSide, partitionNum, index++);
				workplace.SetSharedBuffer(this.SharedBufferInfoArray[1]);

				workplaceBundle.Add(workplace);
			}

			// bottom
			int memoryHeightBtm = 10000;// imageDataBtm.p_Size.Y;
			int memoryWidhtBtm = this.SharedBufferInfoArray[2].Width;
			for (int i = 0; i < memoryHeightBtm / partitionNum; i++)
			{
				Workplace workplace = new Workplace((int)EdgeSurface.EdgeMapPositionX.Btm, i, 0, memoryHeightBtm / partitionNum * i, memoryWidhtBtm, partitionNum, index++);
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
