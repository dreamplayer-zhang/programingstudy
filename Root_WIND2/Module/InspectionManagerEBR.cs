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
			this.Add(new WorkManager("EBR", WORK_TYPE.INSPECTION, WORK_TYPE.NONE, STATE_CHECK_TYPE.CHIP, 1));
		}

		public bool CreateInspection()
		{
			return CreateInspection(this.recipe);
		}

		public override bool CreateInspection(Recipe _recipe)
		{
			try
			{

				Workplace workplace = new Workplace(0, 0, 0, 0, 1500, 2000);
				workplaceBundle = new WorkplaceBundle();
				workplaceBundle.Add(workplace);
				workplaceBundle.SetSharedBuffer(this.sharedBufferR_Gray, this.SharedBufferWidth, this.SharedBufferHeight);

				EBR ebr = new EBR();
				ebr.SetRecipe(this.recipe);
				ebr.SetWorkplaceBundle(workplaceBundle);
				
				workBundle = new WorkBundle();
				workBundle.Add(ebr);

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
