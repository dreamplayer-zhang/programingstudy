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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Root_WIND2.UI_User
{
	class EdgesideSetup_ViewModel : ObservableObject
	{
		private EdgesideSetup_ImageViewer_ViewModel drawToolVM;

		private EdgeSurfaceParameterBase parameter;
		private EdgeSurfaceRecipeBase recipe;

		private int selectedGrabModeIndex = 0;

		private bool _IsTopChecked = true;
		private bool _IsSideChecked = false;
		private bool _IsBtmChecked = false;

		private DataTable defectDataTable;
		private object selectedDefect;
		private BitmapSource defectImage;

		#region [Getter / Setter]
		public EdgesideSetup_ImageViewer_ViewModel DrawToolVM
		{
			get => drawToolVM;
			set => SetProperty(ref drawToolVM, value);
		}

		public EdgeSurfaceParameterBase Parameter
		{
			get => parameter;
			set => SetProperty(ref parameter, value);
		}

		public EdgeSurfaceRecipeBase Recipe
		{
			get => recipe;
			set => SetProperty(ref recipe, value);
		}

		public int TopOffset
		{
			get
			{
				EdgeSideVision module = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
				Run_InspectEdge inspect = (Run_InspectEdge)module.CloneModuleRun("InspectEdge");

				return inspect.TopOffset;
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

				return inspect.SideOffset;
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
				
				return inspect.BtmOffset;
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

		public int SelectedGrabModeIndex
		{
			get => this.selectedGrabModeIndex;
			set
			{
				GrabMode mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[value];
				Run_InspectEdge inspect = ((Run_InspectEdge)((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.CloneModuleRun("InspectEdge"));

				Recipe.GrabModeName = mode.p_sName;
				if (mode.m_camera != null)
				{
					Recipe.CameraWidth = mode.m_camera.GetRoiSize().X;
					Recipe.CameraHeight = mode.m_camera.GetRoiSize().Y;
				}

				if (Recipe.CameraHeight == 0)
				{
					if (_IsTopChecked)
						Recipe.CameraHeight = inspect.TopHeight;
					else if (_IsSideChecked)
						Recipe.CameraHeight = inspect.SideHeight;
					else if (_IsBtmChecked)
						Recipe.CameraHeight = inspect.BtmHeight;
				}

				Recipe.TriggerRatio = mode.m_dCamTriggerRatio;
				Parameter.CamResolution = mode.m_dResX_um;

				SetProperty<int>(ref this.selectedGrabModeIndex, value);
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
			DrawToolVM = new EdgesideSetup_ImageViewer_ViewModel();
			DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeTopImage"), GlobalObjects.Instance.Get<DialogService>());

			RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
			Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop;
			Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop;

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

			foreach (RootTools.Database.Defect defectInfo in workplace.DefectList)
			{
				String text = "";

				rectList.Add(new CRect((int)defectInfo.p_rtDefectBox.Left, (int)defectInfo.p_rtDefectBox.Top, (int)defectInfo.p_rtDefectBox.Right, (int)defectInfo.p_rtDefectBox.Bottom));
				textList.Add(text);
			}

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				DrawRectDefect(rectList, textList);
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
			this.DrawToolVM.ClearObjects();
			GlobalObjects.Instance.Get<InspectionManagerEdge>().Start();			
		}

		private void ChangeViewer(string dataName)
		{
			RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();

			if (dataName == "Top")
			{
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeTopImage"), GlobalObjects.Instance.Get<DialogService>());
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop;
				Recipe.Offset = this.TopOffset;
			}
			else if (dataName == "Side")
			{
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeSideImage"), GlobalObjects.Instance.Get<DialogService>());
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseSide;
				Recipe.Offset = this.SideOffset;
			}
			else if (dataName == "Bottom")
			{
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeBottomImage"), GlobalObjects.Instance.Get<DialogService>());
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseBtm;
				Recipe.Offset = this.BtmOffset;
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
			{
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop;
			}
			else if (IsSideChecked)
			{
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseSide;
			}
			else if (IsBtmChecked)
			{
				Parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm;
				Recipe = recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseBtm;
			}
		}

		private void DrawRectDefect(List<CRect> rectList, List<String> textList, bool reDraw = false)
		{
			DrawToolVM.AddDrawRectList(rectList, System.Windows.Media.Brushes.Red);
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
