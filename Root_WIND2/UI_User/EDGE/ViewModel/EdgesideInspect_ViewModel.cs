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
				//MaxProgress = GlobalObjects.Instance.Get<InspectionManagerEdge>().GetWorkplaceCount();
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
				this.ImageViewerTopVM.ClearObjects();
				this.ImageViewerSideVM.ClearObjects();
				this.ImageViewerBtmVM.ClearObjects();
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

			if (GlobalObjects.Instance.Get<InspectionManagerEdge>() != null)
            {
				GlobalObjects.Instance.Get<InspectionManagerEdge>().InspectionDone += WorkEventManager_InspectionDone;
				GlobalObjects.Instance.Get<InspectionManagerEdge>().IntegratedProcessDefectDone += WorkEventManager_IntegratedProcessDefectDone;
			}

			dataViewerVM = new Database_DataView_VM();
			this.DataViewerVM.SelectedCellsChanged += SelectedCellsChanged_Callback;
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

			Run_GrabEdge grab = (Run_GrabEdge)edgeSideVision.CloneModuleRun("GrabEdge");
			edgeSideVision.StartRun(grab);
		}

		public void Inspect()
		{
			if (GlobalObjects.Instance.Get<InspectionManagerEdge>() != null)
				GlobalObjects.Instance.Get<InspectionManagerEdge>().Start();
		}

		private void WorkEventManager_InspectionDone(object sender, InspectionDoneEventArgs e)
		{
			Workplace workplace = sender as Workplace;
			//List<String> textList = new List<String>();
			//List<CRect> rectList = new List<CRect>();

			//foreach (RootTools.Database.Defect defectInfo in workplace.DefectList)
			//{
			//	String text = "";

			//	rectList.Add(new CRect((int)defectInfo.p_rtDefectBox.Left, (int)defectInfo.p_rtDefectBox.Top, (int)defectInfo.p_rtDefectBox.Right, (int)defectInfo.p_rtDefectBox.Bottom));
			//	textList.Add(text);
			//}
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				//DrawRectDefect(rectList, textList, e.reDraw);
				UpdateProgress();
			}));
		}

		private void WorkEventManager_IntegratedProcessDefectDone(object sender, IntegratedProcessDefectDoneEventArgs e)
		{
			Workplace workplace = sender as Workplace;
			List<CRect> rectList = new List<CRect>();
			List<string> textList = new List<string>();

			Edgeside_ImageViewer_ViewModel imageViewerVM = new Edgeside_ImageViewer_ViewModel();
			if (workplace.MapIndexX == (int)EdgeSurface.EdgeMapPositionX.Top)
				imageViewerVM = ImageViewerTopVM;
			else if (workplace.MapIndexX == (int)EdgeSurface.EdgeMapPositionX.Side)
				imageViewerVM = ImageViewerSideVM;
			else if (workplace.MapIndexX == (int)EdgeSurface.EdgeMapPositionX.Btm)
				imageViewerVM = ImageViewerBtmVM;

			foreach (RootTools.Database.Defect defectInfo in workplace.DefectList)
			{
				String text = "";

				rectList.Add(new CRect((int)defectInfo.p_rtDefectBox.Left, (int)defectInfo.p_rtDefectBox.Top, (int)defectInfo.p_rtDefectBox.Right, (int)defectInfo.p_rtDefectBox.Bottom));
				textList.Add(text);
			}

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				DatabaseManager.Instance.SelectData();
				dataViewerVM.pDataTable = DatabaseManager.Instance.pDefectTable;

				DrawRectDefect(imageViewerVM, rectList, textList);
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

		private void UpdateProgress()
		{
			if (GlobalObjects.Instance.Get<InspectionManagerEdge>() != null)
            {
				int workplaceCount = GlobalObjects.Instance.Get<InspectionManagerEdge>().GetWorkplaceCount();
				MaxProgress = workplaceCount - 1;
				Progress++;

				int proc = (int)(((double)Progress / MaxProgress) * 100);
				//if (proc == 100)
				//	Percentage = "Processing";
				Percentage = proc.ToString();
			}
		}

		private void DisplayDefectImage(string sInspectionID, string sDefectImageName)
		{
			string sDefectimagePath = @"D:\DefectImage";
			sDefectimagePath = Path.Combine(sDefectimagePath, sInspectionID, sDefectImageName);
			if (File.Exists(sDefectimagePath))
			{
				Bitmap defectImage = (Bitmap)Bitmap.FromFile(sDefectimagePath);
				DefectImage = ImageHelper.GetBitmapSourceFromBitmap(defectImage);
			}
			else
				DefectImage = null;
		}

	}
}
