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

namespace Root_WIND2
{
	class EdgesideSetup_ViewModel : ObservableObject
	{
		private Setup_ViewModel setupVM;
		private RootViewer_ViewModel drawToolVM;

		private int roiHeight_Top;
		private int roiWidth_Top;
		private int threshold_Top;
		private int defectSizeMin_Top;
		private int mergeDist_Top;
		private int illumWhite_Top;
		private int illumSide_Top;

		private int roiHeight_Side;
		private int roiWidth_Side;
		private int threshold_Side;
		private int defectSizeMin_Side;
		private int mergeDist_Side;
		private int illumWhite_Side;
		private int illumSide_Side;

		private int roiHeight_Btm;
		private int roiWidth_Btm;
		private int threshold_Btm;
		private int defectSizeMin_Btm;
		private int mergeDist_Btm;
		private int illumWhite_Btm;
		private int illumSide_Btm;

		private int _ROIHeight = 0;
		private int _ROIWidth = 0;
		private int _Threshold = 0;
		private int _DefectSizeMin = 0;
		private int _MergeDist = 0;
		private int _illumWhite = 0;
		private int _illumSide = 0;
		private bool _IsTopChecked = true;
		private bool _IsSideChecked = false;
		private bool _IsBtmChecked = false;

		private DataTable defectDataTable;
		private object selectedDefect;
		private BitmapSource defectImage;

		#region [Getter / Setter]
		public RootViewer_ViewModel DrawToolVM
		{
			get { return drawToolVM; }
			set { SetProperty(ref drawToolVM, value); }
		}
		public int ROIHeight
		{
			get
			{
				return _ROIHeight;
			}
			set
			{
				SetProperty(ref _ROIHeight, value);
				if (IsTopChecked)
					roiHeight_Top = value;
				if (IsSideChecked)
					roiHeight_Side = value;
				if (IsBtmChecked)
					roiHeight_Btm = value;

                SetParameter();
            }
		}
		public int ROIWidth 
		{
			get
			{
				return _ROIWidth;
			}
			set
			{
				SetProperty(ref _ROIWidth, value);
				if (IsTopChecked)
					roiWidth_Top = value;
				if (IsSideChecked)
					roiWidth_Side = value;
				if (IsBtmChecked)
					roiWidth_Btm = value;

                SetParameter();
            }
		}
		public int Threshold
		{
			get
			{
				return _Threshold;
			}
			set
			{
				SetProperty(ref _Threshold, value);
				if (IsTopChecked)
					threshold_Top = value;
				if (IsSideChecked)
					threshold_Side = value;
				if (IsBtmChecked)
					threshold_Btm = value;

                SetParameter();
            }
		}
		public int DefectSizeMin
		{
			get
			{
				return _DefectSizeMin;
			}
			set
			{
				SetProperty(ref _DefectSizeMin, value);
				if (IsTopChecked)
					defectSizeMin_Top = value;
				if (IsSideChecked)
					defectSizeMin_Side = value;
				if (IsBtmChecked)
					defectSizeMin_Btm = value;

                SetParameter();
            }
		}
		public int MergeDist
		{
			get
			{
				return _MergeDist;
			}
			set
			{
				SetProperty(ref _MergeDist, value);
				if (IsTopChecked)
					mergeDist_Top = value;
				if (IsSideChecked)
					mergeDist_Side = value;
				if (IsBtmChecked)
					mergeDist_Btm = value;

                SetParameter();
            }
		}
		public int IllumWhite
		{
			get
			{
				return _illumWhite;
			}
			set
			{
				SetProperty(ref _illumWhite, value);
				if (IsTopChecked)
					illumWhite_Top = value;
				if (IsSideChecked)
					illumWhite_Side = value;
				if (IsBtmChecked)
					illumWhite_Btm = value;

                SetParameter();
            }
		}
		public int IllumSide
		{
			get
			{
				return _illumSide;
			}
			set
			{
				SetProperty(ref _illumSide, value);
				if (IsTopChecked)
					illumSide_Top = value;
				if (IsSideChecked)
					illumSide_Side = value;
				if (IsBtmChecked)
					illumSide_Btm = value;

                SetParameter();
            }
		}

		public bool IsTopChecked
		{
			get
			{
				return _IsTopChecked;
			}
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
			get
			{
				return _IsSideChecked;
			}
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
			get
			{
				return _IsBtmChecked;
			}
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
			get
			{
				return defectImage;
			}
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
					ROIHeight = roiHeight_Top;
					ROIWidth = roiWidth_Top;
					Threshold = threshold_Top;
					DefectSizeMin = defectSizeMin_Top;
					MergeDist = mergeDist_Top;
					IllumWhite = illumWhite_Top;
					IllumSide = illumSide_Top;
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
					ROIHeight = roiHeight_Side;
					ROIWidth = roiWidth_Side;
					Threshold = threshold_Side;
					DefectSizeMin = defectSizeMin_Side;
					MergeDist = mergeDist_Side;
					IllumWhite = illumWhite_Side;
					IllumSide = illumSide_Side;
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
					ROIHeight = roiHeight_Btm;
					ROIWidth = roiWidth_Btm;
					Threshold = threshold_Btm;
					DefectSizeMin = defectSizeMin_Btm;
					MergeDist = mergeDist_Btm;
					IllumWhite = illumWhite_Btm;
					IllumSide = illumSide_Btm;
				});
			}
		}

		public EdgesideSetup_ViewModel()
		{

		}

		public void Init(Setup_ViewModel _setup)
		{
			this.setupVM = _setup;

			DrawToolVM = new RootViewer_ViewModel();
			DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeTopImage"), GlobalObjects.Instance.Get<DialogService>());

			WIND2EventManager.BeforeRecipeSave += BeforeRecipeSave_Callback;
			WorkEventManager.ProcessDefectWaferDone += ProcessDefectWaferDone;
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

			
			if (dataName == "Top")
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeTopImage"), GlobalObjects.Instance.Get<DialogService>());
			else if (dataName == "Side")
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeSideImage"), GlobalObjects.Instance.Get<DialogService>());
			else if (dataName == "Bottom")
				DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeBottomImage"), GlobalObjects.Instance.Get<DialogService>());

            SetParameter();
        }

		public void LoadParameter()
		{
			RecipeEBR recipe = GlobalObjects.Instance.Get<RecipeEBR>();

			if (recipe.GetItem<EdgeSurfaceParameter>() == null)
				return;

			roiHeight_Top = recipe.GetItem<EdgeSurfaceParameter>().RoiHeightTop;
			roiWidth_Top = recipe.GetItem<EdgeSurfaceParameter>().RoiWidthTop;
			threshold_Top = recipe.GetItem<EdgeSurfaceParameter>().ThesholdTop;
			defectSizeMin_Top = recipe.GetItem<EdgeSurfaceParameter>().SizeMinTop;
			mergeDist_Top = recipe.GetItem<EdgeSurfaceParameter>().MergeDistTop;
			illumWhite_Top = recipe.GetItem<EdgeSurfaceParameter>().IllumWhiteTop;
			illumSide_Top = recipe.GetItem<EdgeSurfaceParameter>().IllumSideTop;

			roiHeight_Side = recipe.GetItem<EdgeSurfaceParameter>().RoiHeightSide;
			roiWidth_Side = recipe.GetItem<EdgeSurfaceParameter>().RoiWidthSide;
			threshold_Side = recipe.GetItem<EdgeSurfaceParameter>().ThesholdSide;
			defectSizeMin_Side = recipe.GetItem<EdgeSurfaceParameter>().SizeMinSide;
			mergeDist_Side = recipe.GetItem<EdgeSurfaceParameter>().MergeDistSide;
			illumWhite_Side = recipe.GetItem<EdgeSurfaceParameter>().IllumWhiteSide;
			illumSide_Side = recipe.GetItem<EdgeSurfaceParameter>().IllumSideSide;

			roiHeight_Btm = recipe.GetItem<EdgeSurfaceParameter>().RoiHeightBtm;
			roiWidth_Btm = recipe.GetItem<EdgeSurfaceParameter>().RoiWidthBtm;
			threshold_Btm = recipe.GetItem<EdgeSurfaceParameter>().ThesholdBtm;
			defectSizeMin_Btm = recipe.GetItem<EdgeSurfaceParameter>().SizeMinBtm;
			mergeDist_Btm = recipe.GetItem<EdgeSurfaceParameter>().MergeDistBtm;
			illumWhite_Btm = recipe.GetItem<EdgeSurfaceParameter>().IllumWhiteBtm;
			illumSide_Btm = recipe.GetItem<EdgeSurfaceParameter>().IllumSideBtm;

			if (IsTopChecked)
			{
				ROIHeight = roiHeight_Top;
				ROIWidth = roiWidth_Top;
				Threshold = threshold_Top;
				DefectSizeMin = defectSizeMin_Top;
				MergeDist = mergeDist_Top;
				IllumWhite = illumWhite_Top;
				IllumSide = illumSide_Top;
			}
			else if (IsSideChecked)
			{
				ROIHeight = roiHeight_Side;
				ROIWidth = roiWidth_Side;
				Threshold = threshold_Side;
				DefectSizeMin = defectSizeMin_Side;
				MergeDist = mergeDist_Side;
				IllumWhite = illumWhite_Side;
				IllumSide = illumSide_Side;
			}
			else if (IsBtmChecked)
			{
				ROIHeight = roiHeight_Btm;
				ROIWidth = roiWidth_Btm;
				Threshold = threshold_Btm;
				DefectSizeMin = defectSizeMin_Btm;
				MergeDist = mergeDist_Btm;
				IllumWhite = illumWhite_Btm;
				IllumSide = illumSide_Btm;
			}
		}

		public void SetParameter()
		{
			RecipeEBR recipe = GlobalObjects.Instance.Get<RecipeEBR>();

			EdgeSurfaceParameter param = new EdgeSurfaceParameter();
			param.RoiHeightTop = roiHeight_Top;
			param.RoiWidthTop = roiWidth_Top;
			param.ThesholdTop = threshold_Top;
			param.SizeMinTop = defectSizeMin_Top;
			param.MergeDistTop = mergeDist_Top;
			param.IllumWhiteTop = illumSide_Top;
			param.IllumSideTop = illumSide_Top;

			param.RoiHeightSide = roiHeight_Side;
			param.RoiWidthSide = roiWidth_Side;
			param.ThesholdSide = threshold_Side;
			param.SizeMinSide = defectSizeMin_Side;
			param.MergeDistSide = mergeDist_Side;
			param.IllumWhiteSide = illumSide_Side;
			param.IllumSideSide = illumSide_Side;

			param.RoiHeightBtm = roiHeight_Btm;
			param.RoiWidthBtm = roiWidth_Btm;
			param.ThesholdBtm = threshold_Btm;
			param.SizeMinBtm = defectSizeMin_Btm;
			param.MergeDistBtm = mergeDist_Btm;
			param.IllumWhiteBtm = illumSide_Btm;
			param.IllumSideBtm = illumSide_Btm;

			//recipe.ParameterItemList.Clear();
			recipe.ParameterItemList.Add(param);
		}

		public object GetDataGridItem(DataTable table, DataRowView selectedRow, string sColumnName)
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

		public void DisplayDefectImage(string sInspectionID, string sDefectImageName)
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

		private void BeforeRecipeSave_Callback(object obj, RecipeEventArgs args)
		{
            //SetParameter(); // 이건 이제.. 필요없을듯
        }

		private void ProcessDefectWaferDone(object obj, ProcessDefectWaferDoneEventArgs e)
		{
			Workplace workplace = obj as Workplace;

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				UpdateDefectData();
			}));
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
	}
}
