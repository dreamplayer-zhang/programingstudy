using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using RootTools;
using RootTools.Database;
using RootTools_Vision;

namespace Root_AOP01_Inspection.Module
{
	public class InspectionManager_AOP : WorkFactory
	{
		SolidColorBrush brushSnap = System.Windows.Media.Brushes.LightSkyBlue;
		SolidColorBrush brushPosition = System.Windows.Media.Brushes.SkyBlue;
		SolidColorBrush brushPreInspection = System.Windows.Media.Brushes.Cornsilk;
		SolidColorBrush brushInspection = System.Windows.Media.Brushes.Gold;
		SolidColorBrush brushMeasurement = System.Windows.Media.Brushes.CornflowerBlue;
		SolidColorBrush brushComplete = System.Windows.Media.Brushes.YellowGreen;


		#region [Member Variables]
		WorkBundle workBundle;
		WorkplaceBundle workplaceBundle;

		#endregion

		public InspectionManager_AOP(IntPtr _sharedBuffer, int _width, int _height)
		{
			this.sharedBuffer = _sharedBuffer;
			this.sharedBufferWidth = _width;
			this.sharedBufferHeight = _height;
			sharedBufferByteCnt = 1;
		}

		//protected override void InitWorkManager()   //Del Temp LYJ
		//{
		//	//this.Add(new WorkManager("Position", WORK_TYPE.ALIGNMENT, WORK_TYPE.SNAP, STATE_CHECK_TYPE.CHIP));
		//	this.Add(new WorkManager("Inspection", WORK_TYPE.INSPECTION, WORK_TYPE.SNAP, STATE_CHECK_TYPE.CHIP, 10));
		//	this.Add(new WorkManager("ProcessDefect", WORK_TYPE.DEFECTPROCESS, WORK_TYPE.INSPECTION, STATE_CHECK_TYPE.CHIP));
		//	this.Add(new WorkManager("ProcessDefect_Wafer", WORK_TYPE.DEFECTPROCESS_ALL, WORK_TYPE.DEFECTPROCESS, STATE_CHECK_TYPE.WAFER));
		//
		//	AOPEventManager.SnapDone += SnapDone_Callback;
		//}

		private Recipe recipe;
		private IntPtr sharedBuffer;

		private IntPtr sharedBufferR_Gray;
		private IntPtr sharedBufferR;
		private IntPtr sharedBufferG;
		private IntPtr sharedBufferB;

		private int sharedBufferWidth;
		private int sharedBufferHeight;
		private int sharedBufferByteCnt;

		public RecipeBase Recipe { get => recipe; set => recipe = value; }
		public IntPtr SharedBufferR_Gray { get => sharedBufferR_Gray; set => sharedBufferR_Gray = value; }
		public IntPtr SharedBufferR { get => sharedBufferR; set => sharedBufferR = value; }
		public IntPtr SharedBufferG { get => sharedBufferG; set => sharedBufferG = value; }
		public IntPtr SharedBufferB { get => sharedBufferB; set => sharedBufferB = value; }

		public IntPtr SharedBuffer { get => sharedBuffer; set => sharedBuffer = value; }
		public int SharedBufferWidth { get => sharedBufferWidth; set => sharedBufferWidth = value; }
		public int SharedBufferHeight { get => sharedBufferHeight; set => sharedBufferHeight = value; }
		public int SharedBufferByteCnt { get => sharedBufferByteCnt; set => sharedBufferByteCnt = value; }

		public enum InspectionMode
		{
			FRONT,
			BACK,
			//EBR,
			//EDGE,
		}

		private InspectionMode inspectionMode = InspectionMode.FRONT;
		public InspectionMode p_InspectionMode { get => inspectionMode; set => inspectionMode = value; }

		public int[] mapdata = new int[14 * 14];


		//public bool CreateInspection() //Del Temp LYJ
		//{
		//	return CreateInspection(this.recipe);
		//}

		public void SetWorkplaceBuffer(IntPtr ptrR, IntPtr ptrG, IntPtr ptrB)
		{
			this.SharedBufferR_Gray = ptrR;
			this.SharedBufferG = ptrG;
			this.SharedBufferB = ptrB;
		}


		//public override bool CreateInspection(Recipe _recipe) //Del Temp LYJ
		//{
		//	try
		//	{
		//		RecipeType_WaferMap waferMap = recipe.WaferMap;
		//
		//		if (waferMap == null || waferMap.MapSizeX == 0 || waferMap.MapSizeY == 0)
		//		{
		//			MessageBox.Show("Map 정보가 없습니다.");
		//			return false;
		//		}
		//
		//		workBundle = new WorkBundle();
		//
		//		//Position position = new Position();
		//		//PositionParameter posParam = new PositionParameter();
		//		//_recipe.ParameterItemList.Add(posParam);
		//		//position.SetRecipe(_recipe);
		//		//workBundle.Add(position);
		//
		//		BacksideSurface surface = new BacksideSurface();
		//		BacksideSurfaceParameter surParam = new BacksideSurfaceParameter();
		//		surParam.IsBright = false;
		//		surParam.Intensity = 80;
		//		surParam.Size = 1;
		//		recipe.ParameterItemList.Add(surParam);
		//		surface.SetRecipe(recipe);
		//
		//		workBundle.Add(surface);
		//
		//		ProcessDefect processDefect = new ProcessDefect();
		//		processDefect.SetRecipe(_recipe);
		//		workBundle.Add(processDefect);
		//
		//		workplaceBundle = WorkplaceBundle.CreateWaferMap(_recipe);
		//		workplaceBundle.SetSharedBuffer(this.SharedBuffer, this.SharedBufferWidth, this.SharedBufferHeight, this.SharedBufferByteCnt);
		//		workplaceBundle.SetSharedRGBBuffer(this.SharedBufferR, this.SharedBufferG, this.SharedBufferB);
		//
		//		ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();
		//		processDefect_Wafer.SetRecipe(recipe);
		//		processDefect_Wafer.SetWorkplaceBundle(workplaceBundle);
		//		workBundle.Add(processDefect_Wafer);
		//
		//		workplaceBundle.SetSharedBuffer(this.SharedBuffer, this.SharedBufferWidth, this.SharedBufferHeight, this.SharedBufferByteCnt);
		//
		//		if (this.SetBundles(workBundle, workplaceBundle) == false)
		//			return false;
		//	}
		//	catch (Exception ex)
		//	{
		//		MessageBox.Show("Inspection 생성에 실패하였습니다.\nDetail : " + ex.Message);
		//		return false;
		//	}
		//	return true;
		//}

		//public void SnapDone_Callback(object obj, SnapDoneArgs args) //Del Temp LYJ
		//{
		//	if (this.workplaceBundle == null) return; // 검사 진행중인지 확인하는 조건으로 바꿔야함
		//
		//	Rect snapArea = new Rect(new Point(args.startPosition.X, args.startPosition.Y), new Point(args.endPosition.X, args.endPosition.Y));
		//
		//	foreach (Workplace wp in this.workplaceBundle)
		//	{
		//		Rect checkArea = new Rect(new Point(wp.PositionX, wp.PositionY + wp.BufferSizeY), new Point(wp.PositionX + wp.BufferSizeX, wp.PositionY));
		//
		//		if (snapArea.Contains(checkArea) == true)
		//		{
		//			wp.STATE = WORK_TYPE.SNAP;
		//		}
		//	}
		//
		//}

		private new void Start()
		{
			string lotId = "Lotid";
			string partId = "Partid";
			string setupId = "SetupID";
			string cstId = "CSTid";
			string waferId = "WaferID";
			//string sRecipe = "RecipeID";
			string recipeName = recipe.Name;
			var temp = DatabaseManager.Instance.GetConnectionStatus();
			DatabaseManager.Instance.SetLotinfo(lotId, partId, setupId, cstId, waferId, recipeName);

			base.Start();
		}

		public void SetWorkplaceBuffer(IntPtr inspPtr, IntPtr ptrR, IntPtr ptrG, IntPtr ptrB)
		{
			this.SharedBuffer = inspPtr;
			this.SharedBufferR = ptrR;
			this.SharedBufferG = ptrG;
			this.SharedBufferB = ptrB;
		}
		//public void Start(bool Snap) //Del Temp LYJ
		//{
		//	if (this.Recipe == null && this.Recipe.WaferMap == null)
		//		return;
		//
		//	if (Snap == false)
		//	{
		//		foreach (Workplace wp in this.workplaceBundle)
		//		{
		//			wp.STATE = WORK_TYPE.SNAP;
		//		}
		//	}
		//
		//	Start();
		//}

		public new void Stop()
		{
			base.Stop();
		}

		public void SetColorSharedBuffer(IntPtr ptrR, IntPtr ptrG, IntPtr ptrB)
		{
			this.SharedBufferR_Gray = ptrR;
			this.SharedBufferG = ptrG;
			this.SharedBufferB = ptrB;
			this.SharedBufferByteCnt = 3;
		}

        protected override void Initialize()
        {
            throw new NotImplementedException();
        }

        protected override WorkplaceBundle CreateWorkplaceBundle()
        {
            throw new NotImplementedException();
        }

        protected override WorkBundle CreateWorkBundle()
        {
            throw new NotImplementedException();
        }

        protected override bool Ready(WorkplaceBundle workplaces, WorkBundle works)
        {
            throw new NotImplementedException();
        }
    }
}
