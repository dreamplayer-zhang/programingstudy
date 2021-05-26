﻿using LiveCharts;
using LiveCharts.Wpf;
using Root_WIND2.Module;
using RootTools;
using RootTools.Database;
using RootTools.Module;
using RootTools_Vision;
using RootTools_Vision.WorkManager3;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Root_WIND2.UI_User
{
	public class EBRInspect_ViewModel : ObservableObject
	{
		#region [Property]
		private EBR_ImageViewer_ViewModel imageViewerVM;
		public EBR_ImageViewer_ViewModel ImageViewerVM
		{
			get { return imageViewerVM; }
			set { SetProperty(ref imageViewerVM, value); }
		}

		#region Measurement Graph
		private SeriesCollection ebrGraph;
		public SeriesCollection EBRGraph
		{
			get => ebrGraph;
			set
			{
				ebrGraph = value;
				RaisePropertyChanged("EBRGraph");
			}
		}

		private string[] xLabels;
		public string[] XLabels
		{
			get => xLabels;
			set
			{
				xLabels = value;
				RaisePropertyChanged("XLabels");
			}
		}

		private int sizeYMinVal = 0;
		public int SizeYMinVal
		{
			get => sizeYMinVal;
			set
			{
				sizeYMinVal = value;
				RaisePropertyChanged("SizeYMinVal");
			}
		}

		private int sizeYMaxVal = 1000;
		public int SizeYMaxVal
		{
			get => sizeYMaxVal;
			set
			{
				sizeYMaxVal = value;
				RaisePropertyChanged("SizeYMaxVal");
			}
		}

		private double sizeFrom = 0;
		public double SizeFrom
		{
			get => sizeFrom;
			set
			{
				sizeFrom = value;
				RaisePropertyChanged("SizeFrom");
			}
		}

		private double sizeTo = 50;
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

		private SeriesCollection bevelGraph;
		public SeriesCollection BevelGraph
		{
			get => bevelGraph;
			set
			{
				bevelGraph = value;
				RaisePropertyChanged("BevelGraph");
			}
		}

		private SeriesCollection rawGraph;
		public SeriesCollection RawGraph
		{
			get => rawGraph;
			set
			{
				rawGraph = value;
				RaisePropertyChanged("RawGraph");
			}
		}
		#endregion

		private Database_DataView_VM dataViewerVM;
		public Database_DataView_VM DataViewerVM
		{
			get { return this.dataViewerVM; }
			set { SetProperty(ref dataViewerVM, value); }
		}

		private BitmapSource measurementImage;
		public BitmapSource MeasurementImage
		{
			get => measurementImage;
			set
			{
				SetProperty(ref measurementImage, value);
			}
		}
		#endregion

		#region [Command]
		public RelayCommand btnStart
		{
			get => new RelayCommand(() =>
			{
				this.ImageViewerVM.ClearObjects();

				if (GlobalObjects.Instance.GetNamed<WorkManager>("ebrInspection") != null)
					GlobalObjects.Instance.GetNamed<WorkManager>("ebrInspection").Start();
			});
		}

		public RelayCommand btnSnap
		{
			get => new RelayCommand(() =>
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
			});
		}

		public RelayCommand btnStop
		{
			get => new RelayCommand(() =>
			{
				if (GlobalObjects.Instance.GetNamed<WorkManager>("ebrInspection") != null)
					GlobalObjects.Instance.GetNamed<WorkManager>("ebrInspection").Stop();
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

		public EBRInspect_ViewModel()
		{
			ImageViewerVM = new EBR_ImageViewer_ViewModel();
			ImageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EBRImage"), GlobalObjects.Instance.Get<DialogService>());

			dataViewerVM = new Database_DataView_VM();
			this.DataViewerVM.SelectedCellsChanged += DataViewerVM_SelectedCellsChanged;

			if (GlobalObjects.Instance.GetNamed<WorkManager>("ebrInspection") != null)
			{
				GlobalObjects.Instance.GetNamed<WorkManager>("ebrInspection").InspectionStart += EBRInspect_ViewModel_InspectionStart;
				GlobalObjects.Instance.GetNamed<WorkManager>("ebrInspection").InspectionDone += EBRInspect_ViewModel_InspectionDone;
				GlobalObjects.Instance.GetNamed<WorkManager>("ebrInspection").ProcessMeasurementDone += EBRInspect_ViewModel_ProcessMeasurementDone; ;
			}
		}

		private void EBRInspect_ViewModel_InspectionStart(object sender, InspectionStartArgs e)
		{
		}

		private void EBRInspect_ViewModel_InspectionDone(object sender, InspectionDoneEventArgs e)
		{
			//throw new NotImplementedException();
		}

		private void EBRInspect_ViewModel_ProcessMeasurementDone(object sender, ProcessMeasurementDoneEventArgs e)
		{
			Workplace workplace = sender as Workplace;
			List<CRect> rectList = new List<CRect>();
			List<string> textList = new List<string>();

			foreach (RootTools.Database.Measurement measure in workplace.MeasureList)
			{
				String text = "";

				rectList.Add(new CRect((int)measure.p_rtDefectBox.Left, (int)measure.p_rtDefectBox.Top, (int)measure.p_rtDefectBox.Right, (int)measure.p_rtDefectBox.Bottom));
				textList.Add(text);
			}

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				DatabaseManager.Instance.SelectData("measurement");
				dataViewerVM.pDataTable = DatabaseManager.Instance.pDefectTable;

				DrawEBRGraph();
				DrawBevelGraph();
				DrawRectMeasurementROI(rectList, textList);
			}));
		}

		private void DrawEBRGraph()
		{
			EBRGraph = null;
			if (EBRGraph == null)
			{
				EBRGraph = new SeriesCollection
				{
					new LineSeries
					{
						//Title = "EBR",
						Fill = System.Windows.Media.Brushes.Transparent,
					},
				};
			}

			RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
			EBRParameter parameter = recipeEBR.GetItem<EBRParameter>();

			int binCount = dataViewerVM.pDataTable.Rows.Count;
			XLabels = new string[binCount];
			for (int i = 1; i <= binCount; i++)
			{
				XLabels[i - 1] = (parameter.StepDegree * i).ToString();
			}
			YLabel = value => value.ToString("N");

			DataRow[] datas = (DataRow[])dataViewerVM.pDataTable.Select();
			ChartValues<float> values = new ChartValues<float>();
			foreach (DataRow table in datas)
			{
				if (table[5].ToString() == Measurement.EBRMeasureItem.EBR.ToString())
				{
					string data = table[6].ToString();
					values.Add(float.Parse(data));
				}
			}
			EBRGraph[0].Values = values;

			//if (values != null)
			//{
			//	SizeYMinVal = (int)values.Min();
			//	SizeYMaxVal = (int)values.Max();
			//}
		}

		private void DrawBevelGraph()
		{
			BevelGraph = null;
			if (BevelGraph == null)
			{
				BevelGraph = new SeriesCollection
				{
					new LineSeries
					{
						//Title = "EBR",
						Fill = System.Windows.Media.Brushes.Transparent,
					},
				};
			}

			RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
			EBRParameter parameter = recipeEBR.GetItem<EBRParameter>();

			int binCount = dataViewerVM.pDataTable.Rows.Count;
			XLabels = new string[binCount];
			for (int i = 1; i <= binCount; i++)
			{
				XLabels[i - 1] = (parameter.StepDegree * i).ToString();
			}
			YLabel = value => value.ToString("N");

			DataRow[] datas = (DataRow[])dataViewerVM.pDataTable.Select();
			ChartValues<float> values = new ChartValues<float>();
			foreach (DataRow table in datas)
			{
				if (table[5].ToString() == Measurement.EBRMeasureItem.Bevel.ToString())
				{
					string data = table[6].ToString();
					values.Add(float.Parse(data));
				}
			}
			BevelGraph[0].Values = values;

			if (values != null)
			{
				SizeYMinVal = (int)values.Min();
				SizeYMaxVal = (int)values.Max();
			}
		}

		private void DrawRectMeasurementROI(List<CRect> rectList, List<String> textList, bool reDraw = false)
		{
			imageViewerVM.AddDrawRectList(rectList, System.Windows.Media.Brushes.Red);
		}

		private void DataViewerVM_SelectedCellsChanged(object obj)
		{
			DataRowView row = (DataRowView)obj;
			if (row == null)
				return;

			int nIndex = (int)row["m_nMeasurementIndex"];
			string sInspectionID = (string)row["m_strInspectionID"];
			string sFileName = nIndex.ToString() + ".bmp";
			DisplayDefectImage(sInspectionID, sFileName);
			DisplayRawData(sInspectionID, sFileName);

			System.Drawing.Rectangle m_View_Rect = new System.Drawing.Rectangle((int)(double)row["m_fAbsX"] - imageViewerVM.p_View_Rect.Width / 2, (int)(double)row["m_fAbsY"] - imageViewerVM.p_View_Rect.Height / 2, imageViewerVM.p_View_Rect.Width, imageViewerVM.p_View_Rect.Height);
			ImageViewerVM.p_View_Rect = m_View_Rect;
			ImageViewerVM.SetImageSource();
			ImageViewerVM.UpdateImageViewer();
		}

		private void DisplayDefectImage(string sInspectionID, string sDefectImageName)
		{
			string sDefectimagePath = @"D:\MeasurementImage";
			sDefectimagePath = Path.Combine(sDefectimagePath, sInspectionID, sDefectImageName);
			if (File.Exists(sDefectimagePath))
			{
				Bitmap defectImage = (Bitmap)Image.FromFile(sDefectimagePath);
				MeasurementImage = ImageHelper.GetBitmapSourceFromBitmap(defectImage);
			}
			else
				MeasurementImage = null;
		}

		private void DisplayRawData(string sInspectionID, string sDefectName)
		{
			string rawDataPath = @"D:\EBRRawData";
			rawDataPath = Path.Combine(rawDataPath, sInspectionID, sDefectName);
		}
	}
}
