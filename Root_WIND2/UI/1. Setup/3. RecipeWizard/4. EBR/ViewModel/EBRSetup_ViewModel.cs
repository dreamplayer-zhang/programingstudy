using LiveCharts;
using Root_WIND2.Module;
using RootTools;
using RootTools.Database;
using RootTools.Module;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Root_WIND2
{
	class EBRSetup_ViewModel : ObservableObject
	{
		Recipe recipe;
		private WIND2_Engineer engineer;
		private Setup_ViewModel setupVM;
		private RootViewer_ViewModel drawToolVM;

		private int roiWidth = 1500;
		private int roiHeight = 500;
		private int notchY = 0;
		private int stepDegree = 10;
		private int xRange = 10;
		private int diffEdge = 20;
		private int diffBevel = 20;
		private int diffEBR = 10;
		private int offsetBevel = 0;
		private int offsetEBR = 0;

		private DataTable measurementDataTable;
		private SeriesCollection measurementGraph;
		
		#region [Getter / Setter]
		public RootViewer_ViewModel DrawToolVM
		{
			get { return drawToolVM; }
			set { SetProperty(ref drawToolVM, value); }
		}
		public int ROIWidth
		{
			get
			{
				return roiWidth;
			}
			set
			{
				SetProperty(ref roiWidth, value);
				//SetParameter();
			}
		}
		public int ROIHeight
		{
			get
			{
				return roiHeight;
			}
			set
			{
				SetProperty(ref roiHeight, value);
				//SetParameter();
			}
		}
		public int NotchY
		{
			get
			{
				return notchY;
			}
			set
			{
				SetProperty(ref notchY, value);
				//SetParameter();
			}
		}
		public int StepDegree
		{
			get
			{
				return stepDegree;
			}
			set
			{
				SetProperty(ref stepDegree, value);
				//SetParameter();
			}
		}
		public int XRange
		{
			get
			{
				return xRange;
			}
			set
			{
				SetProperty(ref xRange, value);
				//SetParameter();
			}
		}
		public int DiffEdge
		{
			get
			{
				return diffEdge;
			}
			set
			{
				SetProperty(ref diffEdge, value);
				//SetParameter();
			}
		}
		public int DiffBevel
		{
			get
			{
				return diffBevel;
			}
			set
			{
				SetProperty(ref diffBevel, value);
				//SetParameter();
			}
		}
		public int DiffEBR
		{
			get
			{
				return diffEBR;
			}
			set
			{
				SetProperty(ref diffEBR, value);
				//SetParameter();
			}
		}
		public int OffsetBevel
		{
			get
			{
				return offsetBevel;
			}
			set
			{
				SetProperty(ref offsetBevel, value);
				//SetParameter();
			}
		}
		public int OffsetEBR
		{
			get
			{
				return offsetEBR;
			}
			set
			{
				SetProperty(ref offsetEBR, value);
				//SetParameter();
			}
		}

		public DataTable MeasurementDataTable
		{
			get => measurementDataTable;
			set => SetProperty(ref measurementDataTable, value);
		}
		public SeriesCollection MeasurementGraph
		{
			get
			{
				return measurementGraph;
			}
			set
			{
				measurementGraph = value;
				RaisePropertyChanged("Measurement");
			}
		}
		#endregion

		public EBRSetup_ViewModel()
		{
			engineer = ProgramManager.Instance.Engineer;
		}

		public void Init(Setup_ViewModel _setup)
		{
			this.setupVM = _setup;
			this.recipe = _setup.Recipe;

			DrawToolVM = new RootViewer_ViewModel();
			DrawToolVM.init(ProgramManager.Instance.GetEdgeMemory(EdgeSideVision.EDGE_TYPE.EBR), ProgramManager.Instance.DialogService);

			WIND2EventManager.BeforeRecipeSave += BeforeRecipeSave_Callback;
			WorkEventManager.InspectionDone += InspectionDone_Callback;
			WorkEventManager.ProcessMeasurementDone += ProcessMeasurementDone_Callback;
		}

		public void Scan()
		{
			EQ.p_bStop = false;
			EdgeSideVision edgeSideVision = ((WIND2_Handler)engineer.ClassHandler()).p_EdgeSideVision;
			if (edgeSideVision.p_eState != ModuleBase.eState.Ready)
			{
				MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
				return;
			}

			Run_GrabEBR grab = (Run_GrabEBR)edgeSideVision.CloneModuleRun("GrabEBR");
			edgeSideVision.StartRun(grab);
		}

		public void Inspect()
		{
			setupVM.InsperctionMgrEBR.SharedBufferR_Gray = DrawToolVM.p_ImageData.GetPtr();
			setupVM.InsperctionMgrEBR.SharedBufferByteCnt = DrawToolVM.p_ImageData.p_nByte;
			setupVM.InsperctionMgrEBR.InspectionMode = InspectionManagerEBR.InsepectionMode.EBR;

			if (setupVM.InsperctionMgrEBR.CreateInspection() == false)
			{
				return;
			}
			setupVM.InsperctionMgrEBR.Start();
		}

		public void LoadParameter()
		{
			if (recipe.GetRecipe<EBRParameter>() == null)
				return;

			roiWidth = recipe.GetRecipe<EBRParameter>().RoiWidth;
			roiHeight = recipe.GetRecipe<EBRParameter>().RoiHeight;
			notchY = recipe.GetRecipe<EBRParameter>().NotchY;
			stepDegree = recipe.GetRecipe<EBRParameter>().StepDegree;
			xRange = recipe.GetRecipe<EBRParameter>().XRange;
			diffEdge = recipe.GetRecipe<EBRParameter>().DiffEdge;
			diffBevel = recipe.GetRecipe<EBRParameter>().DiffBevel;
			diffEBR = recipe.GetRecipe<EBRParameter>().DiffEBR;
			offsetBevel = recipe.GetRecipe<EBRParameter>().OffsetBevel;
			offsetEBR = recipe.GetRecipe<EBRParameter>().OffsetEBR;
		}

		public void SetParameter()
		{
			EBRParameter param = new EBRParameter();
			param.RoiWidth = roiWidth;
			param.RoiHeight = roiHeight;
			param.NotchY = notchY;
			param.StepDegree = stepDegree;
			param.XRange = xRange;
			param.DiffEdge = diffEdge;
			param.DiffBevel = diffBevel;
			param.DiffEBR = diffEBR;
			param.OffsetBevel = offsetBevel;
			param.OffsetEBR = offsetEBR;

			//recipe.ParameterItemList.Clear();
			recipe.ParameterItemList.Add(param);
		}

		private void BeforeRecipeSave_Callback(object obj, RecipeEventArgs args)
		{
			SetParameter();
		}

		private void InspectionDone_Callback(object obj, InspectionDoneEventArgs args)
		{
			Workplace workplace = obj as Workplace;

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				UpdateProgress();
			}));
		}

		private void ProcessMeasurementDone_Callback(object obj, ProcessMeasurementDoneEventArgs e)
		{
			Workplace workplace = obj as Workplace;

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				UpdateDataGrid();
				DrawGraph();
			}));
		}

		private void UpdateProgress()
		{

		}

		private void UpdateDataGrid()
		{
			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			string sRecipeID = recipe.Name;
			string sReicpeFileName = sRecipeID + ".rcp";

			string sDefect = "defect";
			MeasurementDataTable = DatabaseManager.Instance.SelectTablewithInspectionID(sDefect, sInspectionID);
		}

		private void DrawGraph()
		{

		}
	}

}
