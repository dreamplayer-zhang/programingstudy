using ATI;
using RootTools;
using RootTools.Inspects;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using MBrushes = System.Windows.Media.Brushes;
using DPoint = System.Drawing.Point;
using System.Configuration;
using System.Diagnostics;
using System.Data;
using System.Drawing.Configuration;
using Emgu.CV.Stitching;

namespace Root_Vega
{
	public class _2_11_EBRViewModel : ObservableObject
	{
		/// <summary>
		/// 외부 Thread에서 UI를 Update하기 위한 Dispatcher
		/// </summary>
		public Dispatcher _dispatcher;
		Vega_Engineer m_Engineer;
		MemoryTool m_MemoryModule;
		List<ImageData> m_Image = new List<ImageData>();
		//bool bUsingInspection;
		
		#region Property

		public DrawHistoryWorker m_DrawHistoryWorker;

		#region p_EBRRoiList

		ObservableCollection<Roi> _EBRRoiList;
		public ObservableCollection<Roi> p_EBRRoiList
		{
			get { return _EBRRoiList; }
			set
			{
				SetProperty(ref _EBRRoiList, value);
			}
		}
		#endregion

		#region p_InformationDrawer

		//private InformationDrawer informationDrawer;
		//public InformationDrawer p_InformationDrawer
		//{
		//	get
		//	{
		//		return informationDrawer;
		//	}
		//	set
		//	{
		//		SetProperty(ref informationDrawer, value);
		//	}
		//}

		#endregion

		#region p_SimpleShapeDrawer_List
		private SimpleShapeDrawerVM m_SimpleShapeDrawer;
		public SimpleShapeDrawerVM p_SimpleShapeDrawer
		{
			get
			{
				return m_SimpleShapeDrawer;
			}
			set
			{
				SetProperty(ref m_SimpleShapeDrawer, value);
			}
		}
		#endregion

		#region p_ImageViewer

		private ImageViewer_ViewModel m_ImageViewer;
		public ImageViewer_ViewModel p_ImageViewer
		{
			get
			{
				return m_ImageViewer;
			}
			set
			{
				SetProperty(ref m_ImageViewer, value);
			}
		}
		#endregion


		#region EBRParamList

		ObservableCollection<SurfaceParamData> _EBRParamList;
		public ObservableCollection<SurfaceParamData> EBRParamList
		{
			get { return _EBRParamList; }
			set
			{
				SetProperty(ref _EBRParamList, value);
			}
		}
		#endregion

		#region SelectedROI
		Roi _SelectedROI;
		public Roi SelectedROI
		{
			get { return _SelectedROI; }
			set
			{
				SetProperty(ref _SelectedROI, value);

				if (value != null)
				{
					EBRParamList = new ObservableCollection<SurfaceParamData>(value.Surface.ParameterList);
				}
			}
		}
		#endregion

		#region SelectedParam
		SurfaceParamData _SelectedParam;
		public SurfaceParamData SelectedParam
		{
			get { return _SelectedParam; }
			set
			{
				if (value != null)
				{
					SetProperty(ref _SelectedParam, value);
				}
			}
		}
		#endregion

		#region SelectedRecipe
		VegaRecipe _SelectedRecipe;
		public VegaRecipe SelectedRecipe
		{
			get { return _SelectedRecipe; }
			set
			{
				SetProperty(ref _SelectedRecipe, value);
			}
		}
		#endregion

		#endregion

		public _2_11_EBRViewModel(Vega_Engineer engineer, IDialogService dialogService)
		{
			_dispatcher = Dispatcher.CurrentDispatcher;
			m_Engineer = engineer;
			Init(dialogService);

			//m_Engineer.m_InspManager.AddDefect += M_InspManager_AddDefect;
			//bUsingInspection = false;
		}
		void Init(IDialogService dialogService)
		{
			m_MemoryModule = m_Engineer.ClassMemoryTool();
			m_DrawHistoryWorker = new DrawHistoryWorker();

			if (m_MemoryModule != null)
			{
				m_ImageViewer = new ImageViewer_ViewModel(new ImageData(m_MemoryModule.GetMemory(App.sEBRPool, App.sEBRGroup, App.sEBRmem)), dialogService);
				m_SimpleShapeDrawer = new SimpleShapeDrawerVM(m_ImageViewer);
				m_SimpleShapeDrawer.RectangleKeyValue = Key.D1;

				m_ImageViewer.SetDrawer(p_SimpleShapeDrawer);
				m_ImageViewer.m_HistoryWorker = m_DrawHistoryWorker;

				//p_InformationDrawer = new InformationDrawer(p_ImageViewer);
				//p_ImageViewer.informationDrawer = p_InformationDrawer;
			}
			m_Engineer.m_recipe.LoadComplete += () =>
			{
				SelectedRecipe = m_Engineer.m_recipe;
				p_EBRRoiList = new ObservableCollection<Roi>(m_Engineer.m_recipe.VegaRecipeData.RoiList.Where(x => x.RoiType == Roi.Item.EBR));
				EBRParamList = new ObservableCollection<SurfaceParamData>();

				_SelectedROI = null;

				SelectedParam = new SurfaceParamData();//UI 초기화를 위한 코드
				SelectedParam = null;
			};
			m_Engineer.m_recipe.RecipeData.AddComplete += () =>
			{
				SelectedROI = null;
				p_EBRRoiList = new ObservableCollection<Roi>(m_Engineer.m_recipe.VegaRecipeData.RoiList.Where(x => x.RoiType == Roi.Item.EBR));
				EBRParamList = new ObservableCollection<SurfaceParamData>();

				SelectedParam = new SurfaceParamData();//UI 초기화를 위한 코드
				SelectedParam = null;
			};

			return;
		}
		
		/// <summary>
		/// UI에 추가된 Defect을 빨간색 상자로 표시할 수 있도록 추가하는 메소드
		/// </summary>
		/// <param name="source">UI에 추가할 Defect List</param>
		/// <param name="args">arguments. 사용이 필요한 경우 수정해서 사용</param>
		private void M_InspManager_AddDefect(DefectDataWrapper item)
		{
			if ((InspectionManager.GetInspectionType(item.nClassifyCode) == InspectionType.AbsoluteSurface || InspectionManager.GetInspectionType(item.nClassifyCode) == InspectionType.RelativeSurface) ||
				InspectionManager.GetInspectionTarget(item.nClassifyCode) == InspectionTarget.EBR)
			{
				try
				{
					_dispatcher.Invoke(new Action(delegate ()
					{
						//p_ImageViewer.informationDrawer.AddDefectInfo(item);//TODO master merge후 활성화해야함
					}));
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		void ClearDrawList()
		{
			p_SimpleShapeDrawer.Clear();
			p_ImageViewer.SetRoiRect();
			App.m_engineer.m_InspManager.ClearDefectList();
		}
		void _clearInspReslut()
		{
			m_Engineer.m_InspManager._clearInspReslut();
		}

		public void _startInsp()
		{
			//TODO EdgeboxViewModel과 EBRViewModel에 중복된 메소드가 존재하므로 통합이 가능한경우 정리하도록 합시다
			if (!m_Engineer.m_recipe.Loaded)
				return;

			//0. 개수 초기화 및 Table Drop
			//_clearInspReslut();

			ClearDrawList();

			//2. 획득한 영역을 기준으로 검사영역을 생성하고 검사를 시작한다
			for (int i = 0; i < 4; i++)
			{
				for (int k = 0; k < p_EBRRoiList.Count; k++)
				{
					//1. edge box정보를 가져와서 edge 확보 후 전체 검사영역을 그린다//잘 생각해보니 서순인듯
					DrawEdgeBox(p_EBRRoiList[k], p_EBRRoiList[k].EdgeBox.UseCustomEdgeBox);
					var inspAreaList = searchArea(p_EBRRoiList[k]);

					var tempRoi = p_EBRRoiList[k];

					int inspMargin = SelectedRecipe.VegaRecipeData.EBRInspMargin;

					CRect adjustArea = AdjustArea(inspAreaList, inspMargin);

					for (int j = 0; j < tempRoi.Surface.ParameterList.Count; j++)
					{
						var param = tempRoi.Surface.ParameterList[j];
						InspectionType type = InspectionType.AbsoluteSurface;

						if (!param.UseAbsoluteInspection)
						{
							type = InspectionType.RelativeSurface;
						}
						int nDefectCode = InspectionManager.MakeDefectCode(InspectionTarget.EBR, type, 0);


						var temp = new UIElementInfo(new Point(adjustArea.Left, adjustArea.Top), new Point(adjustArea.Right, adjustArea.Bottom));

						System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
						rect.Width = adjustArea.Width;
						rect.Height = adjustArea.Height;
						System.Windows.Controls.Canvas.SetLeft(rect, adjustArea.Left);
						System.Windows.Controls.Canvas.SetTop(rect, adjustArea.Top);
						rect.StrokeThickness = 3;
						rect.Stroke = MBrushes.Orange;

						p_SimpleShapeDrawer.m_ListShape.Add(rect);
						p_SimpleShapeDrawer.m_Element.Add(rect);
						p_SimpleShapeDrawer.m_ListRect.Add(temp);

						MemoryData memory = m_Engineer.GetMemory(App.sEBRPool, App.sEBRGroup, App.sEBRmem);
						IntPtr p = memory.GetPtr(0);

						m_Engineer.m_InspManager.CreateInspArea(App.sEBRPool, App.sEBRGroup, App.sEBRmem, m_Engineer.GetMemory(App.sEBRPool, App.sEBRGroup, App.sEBRmem).GetMBOffset(),
							m_Engineer.GetMemory(App.sEBRPool, App.sEBRGroup, App.sEBRmem).p_sz.X,
							m_Engineer.GetMemory(App.sEBRPool, App.sEBRGroup, App.sEBRmem).p_sz.Y,
							adjustArea, 500, param, nDefectCode, m_Engineer.m_recipe.VegaRecipeData.UseDefectMerge, m_Engineer.m_recipe.VegaRecipeData.MergeDistance, SelectedRecipe.VegaRecipeData.EBRInspAreaSize, p);

						p_ImageViewer.SetRoiRect();
					}
				}
			}
			m_Engineer.m_InspManager.StartInspection();
		}
		private CRect AdjustArea(CRect originArea, int inspMargin)
		{
			CRect result = new CRect();
			//우선 offset만큼 일괄 제거
			result = new CRect(new Point(originArea.Left + inspMargin, originArea.Top + inspMargin), new Point(originArea.Right - inspMargin, originArea.Bottom - inspMargin));

			return result;
		}
		void DrawEdgeBox(Roi roi, bool useRecipeEdgeBox)
		{
			List<EdgeElement> targetList = new List<EdgeElement>();
			var tempToolset = (InspectToolSet)m_Engineer.ClassToolBox().GetToolSet("Inspect");
			var tempInspect = tempToolset.GetInspect("MainVision.Inspect");

			if (p_SimpleShapeDrawer == null) return;

			if (!useRecipeEdgeBox)
			{
				//Init에서 정보를 가져온다
				targetList.Clear();
				for (int j = 0; j < 6; j++)
				{
					var x = tempInspect.nTopLeftXLIst[j];
					var y = tempInspect.nTopLeftYLIst[j];
					var w = tempInspect.nWidthLIst[j];
					var h = tempInspect.nHeighLIst[j];
					EdgeElement tempElement = new EdgeElement(0, new CRect(x, y, x + w, y + h));
					targetList.Add(tempElement);
				}
			}
			else
			{
				//Recipe에서 불러온다. 저장된 Parameter가 정상이 아니면 로드하지 않는다
				if (roi.EdgeBox != null && roi.EdgeBox.EdgeList.Count == 6)
				{
					targetList = roi.EdgeBox.EdgeList.Where(x => x.SavePoint == 0).ToList();
				}
			}
			foreach (EdgeElement item in targetList)
			{
				var temp = new UIElementInfo(new Point(item.Rect.Left, item.Rect.Top), new Point(item.Rect.Right, item.Rect.Bottom));

				System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
				rect.Width = item.Rect.Width;
				rect.Height = item.Rect.Height;
				System.Windows.Controls.Canvas.SetLeft(rect, item.Rect.Left);
				System.Windows.Controls.Canvas.SetTop(rect, item.Rect.Top);
				rect.StrokeThickness = 2;
				rect.Stroke = MBrushes.Red;

				p_SimpleShapeDrawer.m_ListShape.Add(rect);
				p_SimpleShapeDrawer.m_Element.Add(rect);
				p_SimpleShapeDrawer.m_ListRect.Add(temp);
			}
			p_ImageViewer.SetRoiRect();
		}

		public void _addRoi()
		{
			if (!m_Engineer.m_recipe.Loaded)
				return;

			int roiCount = m_Engineer.m_recipe.VegaRecipeData.RoiList.Where(x => x.RoiType == Roi.Item.EBR).Count();
			string defaultName = string.Format("EBR ROI #{0}", roiCount);

			Roi temp = new Roi(defaultName, Roi.Item.EBR);
			m_Engineer.m_recipe.VegaRecipeData.RoiList.Add(temp);

			p_EBRRoiList = new ObservableCollection<Roi>(m_Engineer.m_recipe.VegaRecipeData.RoiList.Where(x => x.RoiType == Roi.Item.EBR));
			if (m_Engineer.m_recipe.RecipeData.AddComplete != null)
			{
				m_Engineer.m_recipe.RecipeData.AddComplete();
			}
		}
		private void _endInsp()
		{
			m_Engineer.m_InspManager.InspectionDone(App.indexFilePath);
		}
		void _addParam()
		{
			if (!m_Engineer.m_recipe.Loaded)
				return;

			int paramCount = SelectedROI.Surface.ParameterList.Count;
			string defaultName = string.Format("SurfaceParam #{0}", paramCount);

			SurfaceParamData temp = new SurfaceParamData();
			temp.Name = defaultName;

			SelectedROI.Surface.ParameterList.Add(temp);

			EBRParamList = new ObservableCollection<SurfaceParamData>(SelectedROI.Surface.ParameterList);
		}
		CRect searchArea(Roi roi)
		{
			// variable
			CRect result = new CRect();

			List<Rect> arcROIs = new List<Rect>();
			List<DPoint> aptEdges = new List<DPoint>();
			ImageViewer_ViewModel ivvm = p_ImageViewer;
			eEdgeFindDirection eTempDirection = eEdgeFindDirection.TOP;
			DPoint ptLeft1, ptLeft2, ptBottom, ptRight1, ptRight2, ptTop;
			DPoint ptLT, ptRT, ptLB, ptRB;

			// implement
			if (p_SimpleShapeDrawer == null) return result;
			arcROIs.Clear();
			aptEdges.Clear();
			for (int j = 0; j < 6; j++)
			{
				if (p_SimpleShapeDrawer.m_ListRect.Count < 6) break;
				arcROIs.Add(new Rect(p_SimpleShapeDrawer.m_ListRect[j].StartPos, p_SimpleShapeDrawer.m_ListRect[j].EndPos));
			}
			if (arcROIs.Count < 6) return result;

			for (int j = 0; j < arcROIs.Count; j++)
			{
				eTempDirection = InspectionManager.GetDirection(ivvm.p_ImageData, arcROIs[j]);
				aptEdges.Add(InspectionManager.GetEdge(ivvm.p_ImageData, arcROIs[j], eTempDirection, roi.EdgeBox.UseAutoGV, roi.EdgeBox.SearchBrightToDark, roi.EdgeBox.EdgeThreshold));
			}
			// aptEeges에 있는 DPoint들을 좌표에 맞게 분배
			List<DPoint> aSortedByX = aptEdges.OrderBy(x => x.X).ToList();
			List<DPoint> aSortedByY = aptEdges.OrderBy(x => x.Y).ToList();
			if (aSortedByX[0].Y < aSortedByX[1].Y)
			{
				ptLeft1 = aSortedByX[0];
				ptLeft2 = aSortedByX[1];
			}
			else
			{
				ptLeft1 = aSortedByX[1];
				ptLeft2 = aSortedByX[0];
			}
			if (aSortedByX[4].Y < aSortedByX[5].Y)
			{
				ptRight1 = aSortedByX[4];
				ptRight2 = aSortedByX[5];
			}
			else
			{
				ptRight1 = aSortedByX[5];
				ptRight2 = aSortedByX[4];
			}
			ptTop = aSortedByY[0];
			ptBottom = aSortedByY[5];

			ptLT = new DPoint(ptLeft1.X, ptTop.Y);
			ptLB = new DPoint(ptLeft2.X, ptBottom.Y);
			ptRB = new DPoint(ptRight2.X, ptBottom.Y);
			ptRT = new DPoint(ptRight1.X, ptTop.Y);

			DrawLine(ptLT, ptLB, MBrushes.Lime);
			DrawLine(ptRB, ptRT, MBrushes.Lime);
			DrawLine(ptLT, ptRT, MBrushes.Lime);
			DrawLine(ptLB, ptRB, MBrushes.Lime);

			DrawCross(ptLeft1, MBrushes.Yellow);
			DrawCross(ptLeft2, MBrushes.Yellow);
			DrawCross(ptBottom, MBrushes.Yellow);
			DrawCross(ptRight1, MBrushes.Yellow);
			DrawCross(ptRight2, MBrushes.Yellow);
			DrawCross(ptTop, MBrushes.Yellow);

			result = new CRect(new Point(ptLT.X, ptLT.Y), new Point(ptRB.X, ptRB.Y));

			p_ImageViewer.SetRoiRect();

			return result;
		}
		void DrawCross(System.Drawing.Point pt, System.Windows.Media.SolidColorBrush brsColor)
		{
			DPoint ptLT = new DPoint(pt.X - 40, pt.Y - 40);
			DPoint ptRB = new DPoint(pt.X + 40, pt.Y + 40);
			DPoint ptLB = new DPoint(pt.X - 40, pt.Y + 40);
			DPoint ptRT = new DPoint(pt.X + 40, pt.Y - 40);

			DrawLine(ptLT, ptRB, brsColor);
			DrawLine(ptLB, ptRT, brsColor);
		}
		void DrawLine(System.Drawing.Point pt1, System.Drawing.Point pt2, System.Windows.Media.SolidColorBrush brsColor)
		{
			// variable
			ImageViewer_ViewModel ivvm = p_ImageViewer;

			// implement
			Line myLine = new Line();
			myLine.Stroke = brsColor;
			myLine.X1 = pt1.X;
			myLine.X2 = pt2.X;
			myLine.Y1 = pt1.Y;
			myLine.Y2 = pt2.Y;
			myLine.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
			myLine.VerticalAlignment = VerticalAlignment.Center;
			myLine.StrokeThickness = 2;

			ivvm.SelectedTool.m_ListShape.Add(myLine);
			UIElementInfo uei = new UIElementInfo(new System.Windows.Point(myLine.X1, myLine.Y1), new System.Windows.Point(myLine.X2, myLine.Y2));
			ivvm.SelectedTool.m_ListRect.Add(uei);
			ivvm.SelectedTool.m_Element.Add(myLine);
		}

		#region Command
		public RelayCommand CommandClearInspResult
		{
			get { return new RelayCommand(_clearInspReslut); }
		}
		public RelayCommand CommandStartInsp
		{
			get { return new RelayCommand(_startInsp); }
		}
		public RelayCommand CommandEndInsp
		{
			get { return new RelayCommand(_endInsp); }
		}
		public RelayCommand CommandAddParam
		{
			get { return new RelayCommand(_addParam); }
		}
		public RelayCommand CommandAddRoi
		{
			get { return new RelayCommand(_addRoi); }
		}
		#endregion
	}
}
