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
		private Edgeside_ImageViewer_ViewModel imgeViewerTopVM;
		private Edgeside_ImageViewer_ViewModel imgeViewerSideVM;
		private Edgeside_ImageViewer_ViewModel imgeViewerBtmVM;

		private int progress = 0;
		private int maxProgress = 100;
		private string percentage = "0";
		private DataTable defectDataTable;
		private object selectedDefect;
		private BitmapSource defectImage;

		#region [Getter / Setter]
		public Edgeside_ImageViewer_ViewModel ImageViewerTopVM
		{
			get => imgeViewerTopVM;
			set => SetProperty(ref imgeViewerTopVM, value);
		}
		public Edgeside_ImageViewer_ViewModel ImageViewerSideVM
		{
			get => imgeViewerSideVM;
			set => SetProperty(ref imgeViewerSideVM, value);
		}
		public Edgeside_ImageViewer_ViewModel ImageViewerBtmVM
		{
			get => imgeViewerBtmVM;
			set => SetProperty(ref imgeViewerBtmVM, value);
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
		public DataTable DefectDataTable
		{
			get => defectDataTable;
			set => SetProperty(ref defectDataTable, value);
		}

		public object SelectedDefect
		{
			get => selectedDefect;
			set
			{
				SetProperty(ref selectedDefect, value);

				DataRowView selectedRow = (DataRowView)SelectedDefect;
				if (selectedRow != null)
				{
					int nIndex = (int)GetDataGridItem(DefectDataTable, selectedRow, "m_nDefectIndex");
					string sInspectionID = (string)GetDataGridItem(DefectDataTable, selectedRow, "m_strInspectionID");
					string sFileName = nIndex.ToString() + ".bmp";
					DisplayDefectImage(sInspectionID, sFileName);

					int mapIndexX = (int)GetDataGridItem(DefectDataTable, selectedRow, "m_nChipIndexX");
					double x = (double)GetDataGridItem(DefectDataTable, selectedRow, "m_fAbsX");
					double y = (double)GetDataGridItem(DefectDataTable, selectedRow, "m_fAbsY");

					Edgeside_ImageViewer_ViewModel imageViewerVM = new Edgeside_ImageViewer_ViewModel();
					if (mapIndexX == (int)EdgeSurface.EdgeMapPositionX.Top)
						imageViewerVM = ImageViewerTopVM;
					else if (mapIndexX == (int)EdgeSurface.EdgeMapPositionX.Side)
						imageViewerVM = ImageViewerSideVM;
					else if (mapIndexX == (int)EdgeSurface.EdgeMapPositionX.Btm)
						imageViewerVM = ImageViewerBtmVM;

					MoveDefect(imageViewerVM, (int)x, (int)y);
				}
			}
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

			GlobalObjects.Instance.Get<InspectionManagerEdge>().InspectionDone += WorkEventManager_InspectionDone;
			GlobalObjects.Instance.Get<InspectionManagerEdge>().IntegratedProcessDefectDone += WorkEventManager_IntegratedProcessDefectDone;
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
				DrawRectDefect(imageViewerVM, rectList, textList);
				UpdateDefectData();
			}));
		}

		private void DrawRectDefect(Edgeside_ImageViewer_ViewModel imageViewerVM, List<CRect> rectList, List<String> textList, bool reDraw = false)
		{
			imageViewerVM.AddDrawRectList(rectList, System.Windows.Media.Brushes.Red);
		}

		private void UpdateProgress()
		{
			int workplaceCount = GlobalObjects.Instance.Get<InspectionManagerEdge>().GetWorkplaceCount();
			MaxProgress = workplaceCount - 1;
			Progress++;

			int proc = (int)(((double)Progress / MaxProgress) * 100);
			//if (proc > 99)
			//	proc = 99;
			Percentage = proc.ToString();
		}

		private void UpdateDefectData()
		{
			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			string sDefect = "defect";
			DefectDataTable = DatabaseManager.Instance.SelectTablewithInspectionID(sDefect, sInspectionID);
		}

		private object GetDataGridItem(DataTable table, DataRowView selectedRow, string sColumnName)
		{
			object result;
			for (int i = 0; i < table.Columns.Count; i++)
			{
				if (table.Columns[i].ColumnName == sColumnName)
				{
					result = selectedRow.Row.ItemArray[i];
					return result;
				}
			}
			return null;
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

		private void MoveDefect(Edgeside_ImageViewer_ViewModel imageViewerVM, int x, int y)
		{
			imageViewerVM.CanvasMovePoint_Ref(new CPoint(x, y), 1, 1);
		}
	}
}
