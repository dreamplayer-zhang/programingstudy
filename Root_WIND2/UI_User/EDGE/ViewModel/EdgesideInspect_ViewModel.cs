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
		private EdgesideSetup_ImageViewer_ViewModel imgeViewerTopVM;
		private EdgesideSetup_ImageViewer_ViewModel imgeViewerSideVM;
		private EdgesideSetup_ImageViewer_ViewModel imgeViewerBtmVM;

		private DataTable defectDataTable;
		private object selectedDefect;
		private BitmapSource defectImage;

		#region [Getter / Setter]
		public EdgesideSetup_ImageViewer_ViewModel ImageViewerTopVM
		{
			get => imgeViewerTopVM;
			set => SetProperty(ref imgeViewerTopVM, value);
		}
		public EdgesideSetup_ImageViewer_ViewModel ImageViewerSideVM
		{
			get => imgeViewerSideVM;
			set => SetProperty(ref imgeViewerSideVM, value);
		}
		public EdgesideSetup_ImageViewer_ViewModel ImageViewerBtmVM
		{
			get => imgeViewerBtmVM;
			set => SetProperty(ref imgeViewerBtmVM, value);
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

				GlobalObjects.Instance.Get<InspectionManagerEdge>().Start();
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
			ImageViewerTopVM = new EdgesideSetup_ImageViewer_ViewModel();
			ImageViewerTopVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeTopImage"), GlobalObjects.Instance.Get<DialogService>());
			ImageViewerSideVM = new EdgesideSetup_ImageViewer_ViewModel();
			ImageViewerSideVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeSideImage"), GlobalObjects.Instance.Get<DialogService>());
			ImageViewerBtmVM = new EdgesideSetup_ImageViewer_ViewModel();
			ImageViewerBtmVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeBottomImage"), GlobalObjects.Instance.Get<DialogService>());

			WorkEventManager.InspectionDone += WorkEventManager_InspectionDone;
			WorkEventManager.ProcessDefectEdgeDone += WorkEventManager_ProcessDefectEdgeDone;
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

		private void WorkEventManager_ProcessDefectEdgeDone(object sender, ProcessDefectEdgeDoneEventArgs e)
		{
			Workplace workplace = sender as Workplace;
			List<CRect> rectList = new List<CRect>();
			List<string> textList = new List<string>();

			EdgesideSetup_ImageViewer_ViewModel imageViewerVM = new EdgesideSetup_ImageViewer_ViewModel();
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

		private void DrawRectDefect(EdgesideSetup_ImageViewer_ViewModel imageViewerVM, List<CRect> rectList, List<String> textList, bool reDraw = false)
		{
			imageViewerVM.AddDrawRectList(rectList, System.Windows.Media.Brushes.Red);
		}

		private void UpdateProgress()
		{

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
	}
}
