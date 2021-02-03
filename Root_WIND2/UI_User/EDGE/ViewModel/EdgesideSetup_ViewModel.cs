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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Root_WIND2.UI_Temp
{
	class EdgesideSetup_ViewModel : ObservableObject
	{
		private RootViewer_ViewModel drawToolVM;

		private EdgeSurfaceParameterBase parameter;
		private int selectedGrabModeIndexTop = 0;
		private int selectedGrabModeIndexSide = 0;
		private int selectedGrabModeIndexBtm = 0;

		private int camWidth;
		private int camHeight;
		private double camResolution;
		private double camTriggerRatio;

		private int topOffset;
		private int sideOffset;
		private int btmOffset;

		private bool _IsTopChecked = true;
		private bool _IsSideChecked = false;
		private bool _IsBtmChecked = false;

		private DataTable defectDataTable;
		private object selectedDefect;
		private BitmapSource defectImage;

		#region [Getter / Setter]
		public RootViewer_ViewModel DrawToolVM
		{
			get => drawToolVM;
			set => SetProperty(ref drawToolVM, value);
		}

		public EdgeSurfaceParameterBase Parameter
		{
			get => parameter;
			set 
			{
				SetProperty(ref parameter, value);
			}
		}
		public int CamWidth
		{
			get => camWidth;
			set => camWidth = value;
			//{
			//	SetProperty<int>(ref this.camWidth, value);
			//}
		}

		public int CamHeight
		{
			get => camHeight;
			set => camHeight = value;
			//{
			//	SetProperty<int>(ref this.camHeight, value);
			//}
		}

		public double CamResolution
		{
			get => camResolution;
			set
			{
				SetProperty<double>(ref this.camResolution, value);
			}
		}

		public double CamTriggerRatio
		{
			get => camTriggerRatio;
			set
			{
				SetProperty<double>(ref this.camTriggerRatio, value);
			}
		}

		public int TopOffset
		{
			get
			{
				EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
				Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");
				this.topOffset = inspect.TopOffset;

				RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
				recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop.Offset = this.topOffset;

				return this.topOffset;
			}
			set
			{

			}
		}

		public int SideOffset
		{
			get
			{
				EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
				Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");
				this.sideOffset = inspect.SideOffset;

				RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
				recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide.Offset = this.sideOffset;

				return this.sideOffset;
			}
			set
			{

			}
		}

		public int BtmOffset
		{
			get
			{
				EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
				Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");
				this.btmOffset = inspect.BtmOffset;

				RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
				recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm.Offset = this.btmOffset;

				return this.btmOffset;
			}
			set
			{

			}
		}

		public List<string> GrabModeList
		{
			get
			{
				return ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.p_asGrabMode;
			}
			set
			{

			}
		}

		public int SelectedGrabModeIndexTop
		{
			get => this.selectedGrabModeIndexTop;
			set
			{
				GrabMode mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[value];
				if (mode.m_camera != null)
				{
					this.CamWidth = mode.m_camera.GetRoiSize().X;
					this.CamHeight = mode.m_camera.GetRoiSize().Y;
				}
				this.CamResolution = mode.m_dResX_um;
				this.CamTriggerRatio = mode.m_dCamTriggerRatio;

				RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
				recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop.CamWidth = this.CamWidth;
				recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop.CamHeight = this.CamHeight;
				recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop.CamResolution = this.CamResolution;
				recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop.TriggerRatio = this.CamTriggerRatio;

				SetProperty<int>(ref this.selectedGrabModeIndexTop, value);
			}
		}

		public int SelectedGrabModeIndexSide
		{
			get => this.selectedGrabModeIndexSide;
			set
			{
				GrabMode mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[value];
				if (mode.m_camera != null)
				{
					this.CamWidth = mode.m_camera.GetRoiSize().X;
					this.CamHeight = mode.m_camera.GetRoiSize().Y;
				}
				this.CamResolution = mode.m_dResX_um;
				this.CamTriggerRatio = mode.m_dCamTriggerRatio;

				RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
				recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide.CamWidth = this.CamWidth;
				recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide.CamHeight = this.CamHeight;
				recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide.CamResolution = this.CamResolution;
				recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide.TriggerRatio = this.CamTriggerRatio;

				SetProperty<int>(ref this.selectedGrabModeIndexSide, value);
			}
		}

		public int SelectedGrabModeIndexBtm
		{
			get => this.selectedGrabModeIndexBtm;
			set
			{
				GrabMode mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[value];
				if (mode.m_camera != null)
				{
					this.CamWidth = mode.m_camera.GetRoiSize().X;
					this.CamHeight = mode.m_camera.GetRoiSize().Y;
				}
				this.CamResolution = mode.m_dResX_um;
				this.CamTriggerRatio = mode.m_dCamTriggerRatio;

				RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
				recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm.CamWidth = this.CamWidth;
				recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm.CamHeight = this.CamHeight;
				recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm.CamResolution = this.CamResolution;
				recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm.TriggerRatio = this.CamTriggerRatio;

				SetProperty<int>(ref this.selectedGrabModeIndexBtm, value);
			}
		}

		public bool IsTopChecked
		{
			get => _IsTopChecked;
			set
			{
				SetProperty(ref _IsTopChecked, value);
				if (_IsTopChecked)
				{
					IsSideChecked = false;
					IsBtmChecked = false;
				}
			}
		}

		public bool IsSideChecked
		{
			get => _IsSideChecked;
			set
			{
				SetProperty(ref _IsSideChecked, value);
				if (_IsSideChecked)
				{
					IsTopChecked = false;
					IsBtmChecked = false;
				}
			}
		}

		public bool IsBtmChecked
		{
			get => _IsBtmChecked;
			set
			{
				SetProperty(ref _IsBtmChecked, value);
				if (_IsBtmChecked)
				{
					IsTopChecked = false;
					IsSideChecked = false;
				}
			}
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

		public ICommand btnTop
		{
			get
			{
				return new RelayCommand(() => 
				{ 
					ChangeViewer("Top");
				});  
			}
		}

		public ICommand btnSide
		{
			get
			{
				return new RelayCommand(() =>
				{
					ChangeViewer("Side");
				});
			}
		}

		public ICommand btnBottom
		{
			get
			{
				return new RelayCommand(() =>
				{
					ChangeViewer("Bottom");
				});
			}
		}

		public EdgesideSetup_ViewModel()
		{
			DrawToolVM = new RootViewer_ViewModel();
			DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeTopImage"), GlobalObjects.Instance.Get<DialogService>());
			DrawToolVM.p_ROILayer = GlobalObjects.Instance.GetNamed<ImageData>("EdgeTopImage");

			RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
			parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop;

			WorkEventManager.InspectionDone += WorkEventManager_InspectionDone;
			WorkEventManager.ProcessDefectWaferDone += WorkEventManager_ProcessDefectWaferDone;
		}

		private void WorkEventManager_InspectionDone(object sender, InspectionDoneEventArgs e)
		{
			Workplace workplace = sender as Workplace;
			List<string> textList = new List<string>();
			List<CRect> rectList = new List<CRect>();

			foreach (RootTools.Database.Defect defectInfo in workplace.DefectList)
			{
				string text = "";
				rectList.Add(new CRect((int)defectInfo.p_rtDefectBox.Left, (int)defectInfo.p_rtDefectBox.Top, (int)defectInfo.p_rtDefectBox.Right, (int)defectInfo.p_rtDefectBox.Bottom));
				textList.Add(text);
			}

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				DrawRectDefect(rectList, textList, e.reDraw);
				UpdateProgress();
			}));
		}

		private void WorkEventManager_ProcessDefectWaferDone(object sender, ProcessDefectWaferDoneEventArgs e)
		{
			Workplace workplace = sender as Workplace;

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				UpdateDefectData();
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

			Run_GrabEdge grab = (Run_GrabEdge)edgeSideVision.CloneModuleRun("GrabEdge");
			edgeSideVision.StartRun(grab);
		}

		public void Inspect()
		{
			GlobalObjects.Instance.Get<InspectionManagerEdge>().Start();			
		}

		private void ChangeViewer(string dataName)
		{
			RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();

			if (dataName == "Top")
			{
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeTopImage"), GlobalObjects.Instance.Get<DialogService>());
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop;
			}
			else if (dataName == "Side")
			{
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeSideImage"), GlobalObjects.Instance.Get<DialogService>());
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide;
			}
			else if (dataName == "Bottom")
			{
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeBottomImage"), GlobalObjects.Instance.Get<DialogService>());
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm;
			}
			else
				return;
        }

		public void LoadParameter()
		{
			RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();

			if (recipe.GetItem<EdgeSurfaceParameter>() == null)
				return;

			if (IsTopChecked)
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop;
			else if (IsSideChecked)
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide;
			else if (IsBtmChecked)
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm;
		}

		private void DrawRectDefect(List<CRect> rectList, List<String> text, bool reDraw = false)
		{
			//if (reDraw)
			//	drawToolVM.Clear;

			foreach (CRect rect in rectList)
			{
				DrawToolVM.DrawRectBitmap(rect, 0, 0, 0, 0, new CPoint());
			}
		}

		private void UpdateProgress()
		{

		}

		private void UpdateDefectData()
		{
			RecipeEBR recipe = GlobalObjects.Instance.Get<RecipeEBR>();

			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			string sRecipeID = recipe.Name;
			string sReicpeFileName = sRecipeID + ".rcp";

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
