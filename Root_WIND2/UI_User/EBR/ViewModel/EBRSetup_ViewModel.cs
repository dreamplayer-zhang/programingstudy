using LiveCharts;
using LiveCharts.Wpf;
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
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_WIND2.UI_User
{
	class EBRSetup_ViewModel : ObservableObject
	{
		private EBR_ImageViewer_ViewModel imageViewerVM;
		private EBRRecipe recipe;
		private EBRParameter parameter;
		private int selectedGrabModeIndex = 0;

		private DataTable measurementDataTable;
		private SeriesCollection measurementGraph;
		private string[] xLabels;

		private int sizeYMaxVal = 1000; 
		private double sizeFrom = 0;
		private double sizeTo = 50;

		#region [Getter / Setter]
		public EBR_ImageViewer_ViewModel ImageViewerVM
		{
			get { return imageViewerVM; }
			set { SetProperty(ref imageViewerVM, value); }
		}
		public EBRRecipe Recipe
		{
			get => recipe;
			set => SetProperty(ref recipe, value);
		}

		public EBRParameter Parameter
		{
			get => parameter;
			set
			{
				SetProperty(ref parameter, value);
			}
		}

		public List<string> GrabModeList
		{
			get
			{
				return ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.p_asGrabMode;
			}
		}

		public int SelectedGrabModeIndex
		{
			get => this.selectedGrabModeIndex;
			set
			{
				GrabMode mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[value];
				Run_InspectEBR inspect = ((Run_InspectEBR)((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.CloneModuleRun("InspectEBR"));

				if (mode.m_camera != null)
				{
					Recipe.CameraWidth = mode.m_camera.GetRoiSize().X;
					Recipe.CameraHeight = mode.m_camera.GetRoiSize().Y;
				}

				if (Recipe.CameraHeight == 0)
					Recipe.CameraHeight = inspect.Height;

				Recipe.TriggerRatio = mode.m_dCamTriggerRatio;
				Parameter.CamResolution = mode.m_dResX_um;

				SetProperty<int>(ref this.selectedGrabModeIndex, value);
			}
		}

		public DataTable MeasurementDataTable
		{
			get => measurementDataTable;
			set => SetProperty(ref measurementDataTable, value);
		}

		public SeriesCollection MeasurementGraph
		{
			get => measurementGraph;
			set
			{
				measurementGraph = value;
				RaisePropertyChanged("MeasurementGraph");
			}
		}

		public string[] XLabels
		{
			get => xLabels;
			set
			{
				xLabels = value;
				RaisePropertyChanged("XLabels");
			}
		}

		public int SizeYMaxVal
		{
			get => sizeYMaxVal;
			set
			{
				sizeYMaxVal = value;
				RaisePropertyChanged("SizeYMaxVal");
			}
		}

		public double SizeFrom
		{
			get => sizeFrom;
			set
			{
				sizeFrom = value;
				RaisePropertyChanged("SizeFrom");
			}
		}

		public double SizeTo
		{
			get => sizeTo;
			set
			{
				sizeTo = value;
				RaisePropertyChanged("SizeTo");
			}
		}

		public Func<float, string> YLabel { get; set; }
		public string XTitle { get; set; }
		public string YTitle { get; set; }
		#endregion

		#region [Command]
		public RelayCommand btnStart
		{
			get => new RelayCommand(() =>
			{
				this.ImageViewerVM.ClearObjects();
				Inspect();
			});
		}

		public RelayCommand btnSnap
		{
			get => new RelayCommand(() =>
			{
				Scan();
			});
		}

		public RelayCommand btnStop
		{
			get => new RelayCommand(() =>
			{

			});
		}

		public RelayCommand btnClear
		{
			get => new RelayCommand(() =>
			{
				this.ImageViewerVM.ClearObjects();
			});
		}
		#endregion

		public EBRSetup_ViewModel()
		{
			ImageViewerVM = new EBR_ImageViewer_ViewModel();
			ImageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EBRImage"), GlobalObjects.Instance.Get<DialogService>());

			RecipeEBR recipe = GlobalObjects.Instance.Get<RecipeEBR>();
			Recipe = recipe.GetItem<EBRRecipe>();
			Parameter = recipe.GetItem<EBRParameter>();


			GlobalObjects.Instance.Get<InspectionManagerEBR>().InspectionDone += WorkEventManager_InspectionDone;
			GlobalObjects.Instance.Get<InspectionManagerEBR>().ProcessMeasurementDone += WorkEventManager_ProcessMeasurementDone;
		}

		private void WorkEventManager_InspectionDone(object sender, InspectionDoneEventArgs e)
		{
			Workplace workplace = sender as Workplace;

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				UpdateProgress();
			}));
		}

		private void WorkEventManager_ProcessMeasurementDone(object sender, ProcessMeasurementDoneEventArgs e)
		{
			Workplace workplace = sender as Workplace;

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				UpdateDataGrid();
				DrawGraph();
			}));
		}

		public void Scan()
		{
			EQ.p_bStop = false;
			EdgeSideVision edgeSideVision = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
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
			GlobalObjects.Instance.Get<InspectionManagerEBR>().Start();			
		}

		public void LoadParameter()
		{
			RecipeEBR recipe = GlobalObjects.Instance.Get<RecipeEBR>();

			if (recipe.GetItem<EBRParameter>() == null)
				return;

			Recipe = recipe.GetItem<EBRRecipe>();
			Parameter = recipe.GetItem<EBRParameter>();
		}

		private void UpdateProgress()
		{

		}

		private void UpdateDataGrid()
		{
			RecipeEBR recipe = GlobalObjects.Instance.Get<RecipeEBR>();

			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			string sRecipeID = recipe.Name;
			string sReicpeFileName = sRecipeID + ".rcp";

			string sDefect = "defect";
			MeasurementDataTable = DatabaseManager.Instance.SelectTablewithInspectionID(sDefect, sInspectionID);
		}

		private void DrawGraph()
		{
			MeasurementGraph = null;
			if (MeasurementGraph == null)
			{
				MeasurementGraph = new SeriesCollection
				{
					new LineSeries
					{
						Title = "Bevel",
						Fill = Brushes.Transparent,
						
					},
					new LineSeries
					{
						Title = "EBR",
						Fill = Brushes.Transparent,
					},
				};
			}

			int binCount = measurementDataTable.Rows.Count;
			XLabels = new string[binCount];
			for (int i = 1; i <= binCount; i++)
			{
				XLabels[i - 1] = (parameter.StepDegree * i).ToString();
			}
			YLabel = value => value.ToString("N");

			DataRow[] datas;
			datas = measurementDataTable.Select();
			ChartValues<float> bevels = new ChartValues<float>();
			foreach (DataRow table in datas)
			{
				string data = table[5].ToString();
				bevels.Add(float.Parse(data));
			}
			MeasurementGraph[0].Values = bevels;

			ChartValues<float> ebrs = new ChartValues<float>();
			foreach (DataRow table in datas)
			{
				string data = table[6].ToString();
				ebrs.Add(float.Parse(data));
			}
			MeasurementGraph[1].Values = ebrs;

			SizeYMaxVal = (int)ebrs.Max();
		}
	}
}
