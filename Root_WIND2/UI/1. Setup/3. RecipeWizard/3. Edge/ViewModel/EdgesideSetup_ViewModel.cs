﻿using Root_WIND2.Module;
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

		private EdgeSurfaceParameterBase parameter;
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

		}

		public void Init(Setup_ViewModel _setup)
		{
			this.setupVM = _setup;

			DrawToolVM = new RootViewer_ViewModel();
			DrawToolVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeTopImage"), GlobalObjects.Instance.Get<DialogService>());

			RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
			parameter = recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop;

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
	}
}
