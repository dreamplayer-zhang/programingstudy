using Root_WIND2.Module;
using RootTools;
using RootTools.Database;
using RootTools.Module;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using RootTools_Vision.WorkManager3;
using RootTools_Vision.Utility;

namespace Root_WIND2.UI_User
{
	public class EdgesideInspect_ViewModel : ObservableObject
	{
		private Edgeside_ImageViewer_ViewModel imageViewerTopVM;
		private Edgeside_ImageViewer_ViewModel imageViewerSideVM;
		private Edgeside_ImageViewer_ViewModel imageViewerBtmVM;
		private Database_DataView_VM dataViewerVM;
		private BitmapSource defectImage;

		private int progress = 0;
		private int maxProgress = 100;
		private string percentage = "0";

		#region [Getter / Setter]
		public Edgeside_ImageViewer_ViewModel ImageViewerTopVM
		{
			get => imageViewerTopVM;
			set => SetProperty(ref imageViewerTopVM, value);
		}
		public Edgeside_ImageViewer_ViewModel ImageViewerSideVM
		{
			get => imageViewerSideVM;
			set => SetProperty(ref imageViewerSideVM, value);
		}
		public Edgeside_ImageViewer_ViewModel ImageViewerBtmVM
		{
			get => imageViewerBtmVM;
			set => SetProperty(ref imageViewerBtmVM, value);
		}
		public int Progress
		{
			get => progress;
			set => SetProperty(ref progress, value);
		}
		public int MaxProgress
		{
			get => maxProgress;
			set => SetProperty(ref maxProgress, value);
		}
		public string Percentage
		{
			get => percentage;
			set => SetProperty(ref percentage, value);
		}
		public Database_DataView_VM DataViewerVM
		{
			get { return this.dataViewerVM; }
			set { SetProperty(ref dataViewerVM, value); }
		}
		public BitmapSource DefectImage
		{
			get => defectImage;
			set
			{
				SetProperty(ref defectImage, value);
			}

		}
		#endregion

		#region [Command]
		public RelayCommand btnStart
		{
			get => new RelayCommand(() =>
			{
				this.ImageViewerTopVM.ClearObjects();
				this.ImageViewerSideVM.ClearObjects();
				this.ImageViewerBtmVM.ClearObjects();
				Progress = 0;

				if (GlobalObjects.Instance.GetNamed<WorkManager>("edgeInspection") != null)
				{
					WIND2_Engineer engineer = GlobalObjects.Instance.Get<WIND2_Engineer>();
					RecipeEdge recipeEdge = GlobalObjects.Instance.Get<RecipeEdge>();
					CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(engineer.m_handler.p_EdgeSideVision.GetGrabMode(recipeEdge.CameraInfoIndex));

					GlobalObjects.Instance.GetNamed<WorkManager>("edgeInspection").SetCameraInfo(camInfo);
					GlobalObjects.Instance.GetNamed<WorkManager>("edgeInspection").Start();
				}
				return;
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

				Run_GrabEdge grab = (Run_GrabEdge)edgeSideVision.CloneModuleRun("GrabEdge");
				edgeSideVision.StartRun(grab);
			});
		}

		public RelayCommand btnStop
		{
			get => new RelayCommand(() =>
			{
				if (GlobalObjects.Instance.GetNamed<WorkManager>("edgeInspection") != null)
					GlobalObjects.Instance.GetNamed<WorkManager>("edgeInspection").Stop();
			});
		}

		public RelayCommand btnClear
		{
			get => new RelayCommand(() =>
			{
				this.ImageViewerTopVM.ClearObjects();
				this.ImageViewerSideVM.ClearObjects();
				this.ImageViewerBtmVM.ClearObjects();
			});
		}

		public RelayCommand btnSaveKlarf
		{
			get => new RelayCommand(() =>
			{
				WorkManager workManager = GlobalObjects.Instance.GetNamed<WorkManager>("edgeInspection");
				RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();

				WIND2_Engineer engineer = GlobalObjects.Instance.Get<WIND2_Engineer>();
				GrabModeEdge grabMode = engineer.m_handler.p_EdgeSideVision.GetGrabMode(recipe.CameraInfoIndex);
				InfoWafer infoWafer = engineer.m_handler.p_EdgeSideVision.p_infoWafer;
				if (infoWafer == null)
				{
					infoWafer = new InfoWafer("null", 0, engineer);
				}

				Settings settings = new Settings(); 
				SettingItem_SetupEdgeside settings_edgeside = settings.GetItem<SettingItem_SetupEdgeside>();

				DataTable table = DatabaseManager.Instance.SelectCurrentInspectionDefect();
				List<Defect> defects = Tools.DataTableToDefectList(table);

				KlarfData_Lot klarfData = new KlarfData_Lot();
				Directory.CreateDirectory(settings_edgeside.KlarfSavePath);

				klarfData.SetModuleName("Edgeside");
				klarfData.LotStart(settings_edgeside.KlarfSavePath, infoWafer, recipe.WaferMap, grabMode);
				klarfData.WaferStart(recipe.WaferMap, infoWafer);
				klarfData.AddSlot(recipe.WaferMap, defects, recipe.GetItem<OriginRecipe>());
				klarfData.SetResultTimeStamp();
				klarfData.SaveKlarf();
				klarfData.SaveTiffImageFromFiles(Path.Combine(settings_edgeside.DefectImagePath, DatabaseManager.Instance.GetInspectionID()));
			});
		}
		#endregion

		public EdgesideInspect_ViewModel()
		{
			ImageViewerTopVM = new Edgeside_ImageViewer_ViewModel();
			ImageViewerTopVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeTopImage"), GlobalObjects.Instance.Get<DialogService>());
			ImageViewerSideVM = new Edgeside_ImageViewer_ViewModel();
			ImageViewerSideVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeSideImage"), GlobalObjects.Instance.Get<DialogService>());
			ImageViewerBtmVM = new Edgeside_ImageViewer_ViewModel();
			ImageViewerBtmVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeBottomImage"), GlobalObjects.Instance.Get<DialogService>());

			dataViewerVM = new Database_DataView_VM();
			this.DataViewerVM.SelectedCellsChanged += SelectedCellsChanged_Callback;

			if (GlobalObjects.Instance.GetNamed<WorkManager>("edgeInspection") != null)
			{
				GlobalObjects.Instance.GetNamed<WorkManager>("edgeInspection").InspectionStart += EdgesideInspect_ViewModel_InspectionStart;
				GlobalObjects.Instance.GetNamed<WorkManager>("edgeInspection").InspectionDone += EdgesideInspect_ViewModel_InspectionDone; ;
				GlobalObjects.Instance.GetNamed<WorkManager>("edgeInspection").IntegratedProcessDefectDone += EdgesideInspect_ViewModel_IntegratedProcessDefectDone; ;
			}
		}
		private void EdgesideInspect_ViewModel_InspectionStart(object sender, InspectionStartArgs e)
		{
			if (GlobalObjects.Instance.GetNamed<WorkManager>("edgeInspection") != null)
			{
				Progress = 1;
				Percentage = "Inspect...";
			}
		}

		private void EdgesideInspect_ViewModel_InspectionDone(object sender, InspectionDoneEventArgs e)
		{
			Workplace workplace = sender as Workplace;
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				UpdateProgress();
			}));
		}

		private void UpdateProgress()
		{
			if (GlobalObjects.Instance.GetNamed<WorkManager>("edgeInspection") != null)
			{
				Progress += 30;
				Percentage = Progress.ToString() + "%";
			}

			/*
			// 기존
			if (GlobalObjects.Instance.Get<InspectionManagerEdge>() != null)
			{
				int workplaceCount = GlobalObjects.Instance.Get<InspectionManagerEdge>().GetWorkplaceCount();
				MaxProgress = workplaceCount - 1;
				Progress++;

				int proc = (int)(((double)Progress / MaxProgress) * 100);
				if (proc == 100)
					Percentage = "Processing";
				Percentage = proc.ToString();
			}
			*/
		}

		private void EdgesideInspect_ViewModel_IntegratedProcessDefectDone(object sender, IntegratedProcessDefectDoneEventArgs e)
		{
			Workplace workplace = sender as Workplace;
			List<CRect> rectListTop = new List<CRect>();
			List<CRect> rectListSide = new List<CRect>();
			List<CRect> rectListBtm = new List<CRect>();

			List<string> textListTop = new List<string>();
			List<string> textListSide = new List<string>();
			List<string> textListBtm = new List<string>();

			foreach (RootTools.Database.Defect defect in workplace.DefectList)
			{
				String text = "";

				int index = (defect.m_nDefectCode % 10000) / 100;
				if (index == 0)
				{
					rectListTop.Add(new CRect((int)defect.p_rtDefectBox.Left, (int)defect.p_rtDefectBox.Top, (int)defect.p_rtDefectBox.Right, (int)defect.p_rtDefectBox.Bottom));
					textListTop.Add(text);
				}
				if (index == 1)
				{
					rectListSide.Add(new CRect((int)defect.p_rtDefectBox.Left, (int)defect.p_rtDefectBox.Top, (int)defect.p_rtDefectBox.Right, (int)defect.p_rtDefectBox.Bottom));
					textListSide.Add(text);
				}
				if (index == 2)
				{
					rectListBtm.Add(new CRect((int)defect.p_rtDefectBox.Left, (int)defect.p_rtDefectBox.Top, (int)defect.p_rtDefectBox.Right, (int)defect.p_rtDefectBox.Bottom));
					textListBtm.Add(text);
				}

				//String text = "";

				//int index = (defect.m_nDefectCode - 10000) / 100;
				//if (index >= 0 && index < 3)
				//{
				//	rectListTop.Add(new CRect((int)defect.p_rtDefectBox.Left, (int)defect.p_rtDefectBox.Top, (int)defect.p_rtDefectBox.Right, (int)defect.p_rtDefectBox.Bottom));
				//	textListTop.Add(text);
				//}
				//if (index >= 3 && index < 6)
				//{
				//	rectListBtm.Add(new CRect((int)defect.p_rtDefectBox.Left, (int)defect.p_rtDefectBox.Top, (int)defect.p_rtDefectBox.Right, (int)defect.p_rtDefectBox.Bottom));
				//	textListBtm.Add(text);
				//}
				//if (index >= 6 && index < 10)
				//{
				//	rectListSide.Add(new CRect((int)defect.p_rtDefectBox.Left, (int)defect.p_rtDefectBox.Top, (int)defect.p_rtDefectBox.Right, (int)defect.p_rtDefectBox.Bottom));
				//	textListSide.Add(text);
				//}
			}

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				//DatabaseManager.Instance.SelectData();
				dataViewerVM.pDataTable = DatabaseManager.Instance.SelectCurrentInspectionDefect();

				DrawRectDefect(ImageViewerTopVM, rectListTop, textListTop);
				DrawRectDefect(imageViewerSideVM, rectListSide, textListSide);
				DrawRectDefect(ImageViewerBtmVM, rectListBtm, textListBtm);

				Progress = 100;
				Percentage = "Done";
			}));
		}

		private void SelectedCellsChanged_Callback(object obj)
		{
			DataRowView row = (DataRowView)obj;
			if (row == null)
				return;
			
			int nIndex = (int)row["m_nDefectIndex"];
			string sInspectionID = (string)row["m_strInspectionID"];
			string sFileName = nIndex.ToString() + ".bmp";
			DisplayDefectImage(sInspectionID, sFileName);
			
			Edgeside_ImageViewer_ViewModel imageViewerVM = new Edgeside_ImageViewer_ViewModel();
			int mapX = (int)row["m_nChipIndexX"];
			if (mapX == (int)EdgeSurface.EdgeMapPositionX.Top)
				imageViewerVM = ImageViewerTopVM;
			else if (mapX == (int)EdgeSurface.EdgeMapPositionX.Side)
				imageViewerVM = ImageViewerSideVM;
			else if (mapX == (int)EdgeSurface.EdgeMapPositionX.Btm)
				imageViewerVM = ImageViewerBtmVM;

			System.Drawing.Rectangle m_View_Rect = new System.Drawing.Rectangle((int)(double)row["m_fAbsX"] - imageViewerVM.p_View_Rect.Width / 2, (int)(double)row["m_fAbsY"] - imageViewerVM.p_View_Rect.Height / 2, imageViewerVM.p_View_Rect.Width, imageViewerVM.p_View_Rect.Height);
			imageViewerVM.p_View_Rect = m_View_Rect;
			imageViewerVM.SetImageSource();
			imageViewerVM.UpdateImageViewer();
		}

		private void DrawRectDefect(Edgeside_ImageViewer_ViewModel imageViewerVM, List<CRect> rectList, List<String> textList, bool reDraw = false)
		{
			imageViewerVM.AddDrawRectList(rectList, System.Windows.Media.Brushes.Red);
		}

		private void DisplayDefectImage(string sInspectionID, string sDefectImageName)
		{
			string sDefectimagePath = @"D:\DefectImage";
			sDefectimagePath = Path.Combine(sDefectimagePath, sInspectionID, sDefectImageName);
			if (File.Exists(sDefectimagePath))
			{
				Bitmap defectImage = (Bitmap)Image.FromFile(sDefectimagePath);
				DefectImage = ImageHelper.GetBitmapSourceFromBitmap(defectImage);
			}
			else
				DefectImage = null;
		}

	}
}
