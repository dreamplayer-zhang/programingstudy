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

namespace Root_Vega
{
	class _2_6_SideViewModel : ObservableObject
	{
		/// <summary>
		/// 외부 Thread에서 UI를 Update하기 위한 Dispatcher
		/// </summary>
		protected Dispatcher _dispatcher;
		System.Threading.Timer resultTimer;
		Vega_Engineer m_Engineer;
		MemoryTool m_MemoryModule;
		List<ImageData> m_Image = new List<ImageData>();
		List<string> m_astrMem = new List<String> { "Top", "Left", "Right", "Bottom" };
		const string sPool = "SideVision.Memory";
		const string sGroup = "Side";
		bool bUsingInspection;


		#region Property

		public List<DrawHistoryWorker> m_DrawHistoryWorker_List;

		#region p_SideRoiList

		ObservableCollection<Roi> _SideRoiList;
		public ObservableCollection<Roi> p_SideRoiList
		{
			get { return _SideRoiList; }
			set
			{
				SetProperty(ref _SideRoiList, value);
			}
		}
		#endregion

		#region p_SimpleShapeDrawer_List
		private List<SimpleShapeDrawerVM> m_SimpleShapeDrawer_List;
		public List<SimpleShapeDrawerVM> p_SimpleShapeDrawer_List
		{
			get
			{
				return m_SimpleShapeDrawer_List;
			}
			set
			{
				SetProperty(ref m_SimpleShapeDrawer_List, value);
			}
		}
		#endregion

		#region p_InformationDrawerList

		private List<InformationDrawer> informationDrawerList;
		public List<InformationDrawer> p_InformationDrawerList
		{
			get
			{
				return informationDrawerList;
			}
			set
			{
				SetProperty(ref informationDrawerList, value);
			}
		}
		#endregion

		#region p_ImageViewer_List

		private List<ImageViewer_ViewModel> m_ImageViewer_List;
		public List<ImageViewer_ViewModel> p_ImageViewer_List
		{
			get
			{
				return m_ImageViewer_List;
			}
			set
			{
				SetProperty(ref m_ImageViewer_List, value);
			}
		}
		#endregion

		#region p_ImageViewer_Left
		private ImageViewer_ViewModel m_ImageViewer_Left;
		public ImageViewer_ViewModel p_ImageViewer_Left
		{
			get
			{
				return m_ImageViewer_Left;
			}
			set
			{
				SetProperty(ref m_ImageViewer_Left, value);
			}
		}
		#endregion

		#region p_ImageViewer_Top

		private ImageViewer_ViewModel m_ImageViewer_Top;
		public ImageViewer_ViewModel p_ImageViewer_Top
		{
			get
			{
				return m_ImageViewer_Top;
			}
			set
			{
				SetProperty(ref m_ImageViewer_Top, value);
			}
		}
		#endregion

		#region p_ImageViewer_Right
		private ImageViewer_ViewModel m_ImageViewer_Right;
		public ImageViewer_ViewModel p_ImageViewer_Right
		{
			get
			{
				return m_ImageViewer_Right;
			}
			set
			{
				SetProperty(ref m_ImageViewer_Right, value);
			}
		}
		#endregion

		#region p_ImageViewer_Bottom
		private ImageViewer_ViewModel m_ImageViewer_Bottom;

		public ImageViewer_ViewModel p_ImageViewer_Bottom
		{
			get
			{
				return m_ImageViewer_Bottom;
			}
			set
			{
				SetProperty(ref m_ImageViewer_Bottom, value);
			}
		}
		#endregion



		#region SideParamList

		ObservableCollection<SurfaceParamData> _SideParamList;
		public ObservableCollection<SurfaceParamData> SideParamList
		{
			get { return _SideParamList; }
			set
			{
				SetProperty(ref _SideParamList, value);
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
					SideParamList = new ObservableCollection<SurfaceParamData>(value.Surface.ParameterList);
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
		Recipe _SelectedRecipe;
		public Recipe SelectedRecipe
		{
			get { return _SelectedRecipe; }
			set
			{
				SetProperty(ref _SelectedRecipe, value);
			}
		}
		#endregion

		#endregion

		public _2_6_SideViewModel(Vega_Engineer engineer, IDialogService dialogService)
		{
			_dispatcher = Dispatcher.CurrentDispatcher;
			m_Engineer = engineer;
			Init(dialogService);

			//m_Engineer.m_InspManager.AddDefect += M_InspManager_AddDefect;
			bUsingInspection = false;
		}
		void Init(IDialogService dialogService)
		{
			m_MemoryModule = m_Engineer.ClassMemoryTool();
			m_ImageViewer_List = new List<ImageViewer_ViewModel>();
			m_DrawHistoryWorker_List = new List<DrawHistoryWorker>();
			m_SimpleShapeDrawer_List = new List<SimpleShapeDrawerVM>();

			if (m_MemoryModule != null)
			{
				for (int i = 0; i < 4; i++)
				{
					p_ImageViewer_List.Add(new ImageViewer_ViewModel(new ImageData(m_MemoryModule.GetMemory("SideVision.Memory", "Side", m_astrMem[i])), dialogService)); //!! m_Image 는 추후 각 part에 맞는 이미지가 들어가게 수정.
					m_DrawHistoryWorker_List.Add(new DrawHistoryWorker());
				}

				for (int i = 0; i < 4; i++)
				{
					p_SimpleShapeDrawer_List.Add(new SimpleShapeDrawerVM(p_ImageViewer_List[i]));
					p_SimpleShapeDrawer_List[i].RectangleKeyValue = Key.D1;
				}

				for (int i = 0; i < 4; i++)
				{
					p_ImageViewer_List[i].SetDrawer(p_SimpleShapeDrawer_List[i]);
					p_ImageViewer_List[i].m_HistoryWorker = m_DrawHistoryWorker_List[i];
				}

				p_ImageViewer_Top = p_ImageViewer_List[0];
				p_ImageViewer_Left = p_ImageViewer_List[1];
				p_ImageViewer_Right = p_ImageViewer_List[2];
				p_ImageViewer_Bottom = p_ImageViewer_List[3];


				p_InformationDrawerList = new List<InformationDrawer>();
				p_InformationDrawerList.Add(new InformationDrawer(p_ImageViewer_Top));
				p_InformationDrawerList.Add(new InformationDrawer(p_ImageViewer_Left));
				p_InformationDrawerList.Add(new InformationDrawer(p_ImageViewer_Right));
				p_InformationDrawerList.Add(new InformationDrawer(p_ImageViewer_Bottom));
			}
			m_Engineer.m_recipe.LoadComplete += () =>
			{
				SelectedRecipe = m_Engineer.m_recipe;
				p_SideRoiList = new ObservableCollection<Roi>(m_Engineer.m_recipe.RecipeData.RoiList.Where(x => x.RoiType == Roi.Item.ReticleSide));
				SideParamList = new ObservableCollection<SurfaceParamData>();

				_SelectedROI = null;

				SelectedParam = new SurfaceParamData();//UI 초기화를 위한 코드
				SelectedParam = null;
			};

			m_Engineer.m_InspManager.nInspectionCount = 0;//Inspection total count 초기화. 임시 db에서 데이터값을 정상적으로 끌어올때 사용한다

			resultTimer = new System.Threading.Timer(checkAddDefect);
			resultTimer.Change(0, 1000);

			return;
		}
		private void checkAddDefect(object state)
		{
			//_dispatcher
			//concept
			//db에 주기적으로 접근하여 tempTable의 최대 개수를 확인
			//최대개수와 currentDefectIndex의 차이가 발생한다면 currentDefectIndex와 최대 개수 사이의 defect UI를 갱신하고 currentDefectIndex를 최대 개수로 변경한다

		}
		/// <summary>
		/// UI에 추가된 Defect을 빨간색 상자로 표시할 수 있도록 추가하는 메소드
		/// </summary>
		/// <param name="source">UI에 추가할 Defect List</param>
		/// <param name="args">arguments. 사용이 필요한 경우 수정해서 사용</param>
		private void M_InspManager_AddDefect(DefectDataWrapper item)
		{
			if (InspectionManager.GetInspectionType(item.nClassifyCode) != InspectionType.AbsoluteSurface && InspectionManager.GetInspectionType(item.nClassifyCode) != InspectionType.RelativeSurface)
			{
				return;
			}
			//string tempInspDir = @"C:\vsdb\TEMP_IMAGE";
			//System.Data.DataRow dataRow = VSDataDT.NewRow();

			////Data,@No(INTEGER),DCode(INTEGER),Size(INTEGER),Length(INTEGER),Width(INTEGER),Height(INTEGER),InspMode(INTEGER),FOV(INTEGER),PosX(INTEGER),PosY(INTEGER)

			//dataRow["No"] = currentDefectIdx;
			//currentDefectIdx++;
			//dataRow["DCode"] = item.nClassifyCode;
			//dataRow["AreaSize"] = item.fAreaSize;
			//dataRow["Length"] = item.nLength;
			//dataRow["Width"] = item.nWidth;
			//dataRow["Height"] = item.nHeight;
			////dataRow["FOV"] = item.FOV;
			//dataRow["PosX"] = item.fPosX;
			//dataRow["PosY"] = item.fPosY;

			//VSDataDT.Rows.Add(dataRow);
			_dispatcher.Invoke(new Action(delegate ()
			{
				int targetIdx = InspectionManager.GetInspectionTarget(item.nClassifyCode) - InspectionTarget.SideInspection - 1;

				p_InformationDrawerList[targetIdx].AddDefectInfo(item);

				switch (targetIdx)
				{
					case 0:
						p_ImageViewer_Top.RedrawingElement();
						break;
					case 1:
						p_ImageViewer_Left.RedrawingElement();
						break;
					case 2:
						p_ImageViewer_Right.RedrawingElement();
						break;
					case 3:
						p_ImageViewer_Bottom.RedrawingElement();
						break;
				}
			}));
		}

		void ClearDrawList()
		{
			for (int i = 0; i < 4; i++)
			{
				p_SimpleShapeDrawer_List[i].Clear();
				p_InformationDrawerList[i].Clear();

				p_ImageViewer_List[i].SetRoiRect();
				p_InformationDrawerList[i].Redrawing();
			}
		}
		void _clearInspReslut()
		{

		}

		private void _startInsp()
		{
			//TODO EdgeboxViewModel과 SideViewModel에 중복된 메소드가 존재하므로 통합이 가능한경우 정리하도록 합시다
			if (!m_Engineer.m_recipe.Loaded)
				return;
			if (SelectedROI == null)
				return;

			//1. edge box정보를 가져와서 edge 확보 후 전체 검사영역을 그린다
			DrawEdgeBox(SelectedROI, SelectedROI.EdgeBox.UseCustomEdgeBox);
			var inspAreaList = searchArea();

			//2. 획득한 영역을 기준으로 검사영역을 생성하고 검사를 시작한다
			for (int i = 0; i < inspAreaList.Count; i++)
			{
				for (int j = 0; j < SelectedROI.Surface.ParameterList.Count; j++)
				{
					var param = SelectedROI.Surface.ParameterList[j];
					InspectionType type = InspectionType.AbsoluteSurface;

					if (!param.UseAbsoluteInspection)
					{
						type = InspectionType.RelativeSurface;
					}
					int nDefectCode = InspectionManager.MakeDefectCode((InspectionTarget)(10 + i), type, 0);

					m_Engineer.m_InspManager.CreateInspArea(sPool, m_Engineer.GetMemory("SideVision.Memory", "Side", m_astrMem[i]).GetMBOffset(),
						m_Engineer.GetMemory("SideVision.Memory", "Side", m_astrMem[i]).p_sz.X,
						m_Engineer.GetMemory("SideVision.Memory", "Side", m_astrMem[i]).p_sz.Y,
						inspAreaList[i], 1000, param, nDefectCode, m_Engineer.m_recipe.RecipeData.UseDefectMerge, m_Engineer.m_recipe.RecipeData.MergeDistance);
				}
			}
			m_Engineer.m_InspManager.StartInspection();
		}
		void DrawEdgeBox(Roi roi, bool useRecipeEdgeBox)
		{
			List<EdgeElement> targetList = new List<EdgeElement>();
			var tempToolset = (InspectToolSet)m_Engineer.ClassToolBox().GetToolSet("Inspect");
			var tempInspect = tempToolset.GetInspect("SideVision.Inspect");

			ClearDrawList();

			for (int i = 0; i < 4; i++)
			{
				if (p_SimpleShapeDrawer_List[i] == null) continue;

				if (!useRecipeEdgeBox)
				{
					//Init에서 정보를 가져온다
					targetList.Clear();
					for (int j = 0; j < 6; j++)
					{
						var x = tempInspect.nTopLeftXLIst[i * 6 + j];
						var y = tempInspect.nTopLeftYLIst[i * 6 + j];
						var w = tempInspect.nWidthLIst[i * 6 + j];
						var h = tempInspect.nHeighLIst[i * 6 + j];
						EdgeElement tempElement = new EdgeElement(i, new CRect(x, y, x + w, y + h));
						targetList.Add(tempElement);
					}
				}
				else
				{
					//Recipe에서 불러온다. 저장된 Parameter가 정상이 아니면 로드하지 않는다
					if (roi.EdgeBox != null && roi.EdgeBox.EdgeList.Count == 24)
					{
						targetList = roi.EdgeBox.EdgeList.Where(x => x.SavePoint == i).ToList();
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

					p_SimpleShapeDrawer_List[i].m_ListShape.Add(rect);
					p_SimpleShapeDrawer_List[i].m_Element.Add(rect);
					p_SimpleShapeDrawer_List[i].m_ListRect.Add(temp);
				}
				p_ImageViewer_List[i].SetRoiRect();
			}
		}

		private void _endInsp()
		{

		}

		public void _addRoi()
		{
			if (!m_Engineer.m_recipe.Loaded)
				return;

			int roiCount = m_Engineer.m_recipe.RecipeData.RoiList.Where(x => x.RoiType == Roi.Item.ReticleSide).Count();
			string defaultName = string.Format("Side ROI #{0}", roiCount);

			Roi temp = new Roi(defaultName, Roi.Item.ReticleSide);
			m_Engineer.m_recipe.RecipeData.RoiList.Add(temp);

			p_SideRoiList = new ObservableCollection<Roi>(m_Engineer.m_recipe.RecipeData.RoiList.Where(x => x.RoiType == Roi.Item.ReticleSide));
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

			SideParamList = new ObservableCollection<SurfaceParamData>(SelectedROI.Surface.ParameterList);
		}
		List<CRect> searchArea()
		{
			// variable
			List<CRect> result = new List<CRect>();

			List<Rect> arcROIs = new List<Rect>();
			List<DPoint> aptEdges = new List<DPoint>();
			ImageViewer_ViewModel ivvm = p_ImageViewer_Top;
			eEdgeFindDirection eTempDirection = eEdgeFindDirection.TOP;
			DPoint ptLeft1, ptLeft2, ptBottom, ptRight1, ptRight2, ptTop;
			DPoint ptLT, ptRT, ptLB, ptRB;

			// implement
			for (int i = 0; i < 4; i++)
			{
				if (p_SimpleShapeDrawer_List[i] == null) continue;
				arcROIs.Clear();
				aptEdges.Clear();
				for (int j = 0; j < 6; j++)
				{
					if (p_SimpleShapeDrawer_List[i].m_ListRect.Count < 6) break;
					arcROIs.Add(new Rect(p_SimpleShapeDrawer_List[i].m_ListRect[j].StartPos, p_SimpleShapeDrawer_List[i].m_ListRect[j].EndPos));
				}
				if (arcROIs.Count < 6) continue;
				switch (i)
				{
					case 1:
						ivvm = p_ImageViewer_Left;
						break;
					case 2:
						ivvm = p_ImageViewer_Right;
						break;
					case 3:
						ivvm = p_ImageViewer_Bottom;
						break;
					case 0:
					default:
						ivvm = p_ImageViewer_Top;
						break;
				}
				for (int j = 0; j < arcROIs.Count; j++)
				{
					eTempDirection = InspectionManager.GetDirection(ivvm.p_ImageData, arcROIs[j]);
					aptEdges.Add(InspectionManager.GetEdge(ivvm.p_ImageData, arcROIs[j], eTempDirection, SelectedROI.EdgeBox.UseAutoGV, SelectedROI.EdgeBox.SearchBrightToDark, SelectedROI.EdgeBox.EdgeThreshold));
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

				DrawLine(ptLT, ptLB, MBrushes.Lime, i);
				DrawLine(ptRB, ptRT, MBrushes.Lime, i);
				DrawLine(ptLT, ptRT, MBrushes.Lime, i);
				DrawLine(ptLB, ptRB, MBrushes.Lime, i);

				DrawCross(ptLeft1, MBrushes.Yellow, i);
				DrawCross(ptLeft2, MBrushes.Yellow, i);
				DrawCross(ptBottom, MBrushes.Yellow, i);
				DrawCross(ptRight1, MBrushes.Yellow, i);
				DrawCross(ptRight2, MBrushes.Yellow, i);
				DrawCross(ptTop, MBrushes.Yellow, i);

				result.Add(new CRect(new Point(ptLT.X, ptLT.Y), new Point(ptRB.X, ptRB.Y)));

				p_ImageViewer_List[i].SetRoiRect();
			}
			return result;
		}
		void DrawCross(System.Drawing.Point pt, System.Windows.Media.SolidColorBrush brsColor, int nTLRB)
		{
			DPoint ptLT = new DPoint(pt.X - 40, pt.Y - 40);
			DPoint ptRB = new DPoint(pt.X + 40, pt.Y + 40);
			DPoint ptLB = new DPoint(pt.X - 40, pt.Y + 40);
			DPoint ptRT = new DPoint(pt.X + 40, pt.Y - 40);

			DrawLine(ptLT, ptRB, brsColor, nTLRB);
			DrawLine(ptLB, ptRT, brsColor, nTLRB);
		}
		void DrawLine(System.Drawing.Point pt1, System.Drawing.Point pt2, System.Windows.Media.SolidColorBrush brsColor, int nTLRB)
		{
			// variable
			ImageViewer_ViewModel ivvm;

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

			switch (nTLRB)
			{
				case 0: ivvm = m_ImageViewer_Top; break;
				case 1: ivvm = m_ImageViewer_Left; break;
				case 2: ivvm = m_ImageViewer_Right; break;
				case 3: ivvm = m_ImageViewer_Bottom; break;
				default: ivvm = m_ImageViewer_Top; break;
			}

			ivvm.SelectedTool.m_ListShape.Add(myLine);
			UIElementInfo uei = new UIElementInfo(new System.Windows.Point(myLine.X1, myLine.Y1), new System.Windows.Point(myLine.X2, myLine.Y2));
			ivvm.SelectedTool.m_ListRect.Add(uei);
			ivvm.SelectedTool.m_Element.Add(myLine);
		}


		//public System.Windows.Media.Imaging.BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
		//{
		//	var bitmapData = bitmap.LockBits(
		//		new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
		//		System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

		//	var bitmapSource = System.Windows.Media.Imaging.BitmapSource.Create(
		//		bitmapData.Width, bitmapData.Height,
		//		bitmap.HorizontalResolution, bitmap.VerticalResolution,
		//		PixelFormats.Gray8, null,
		//		bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

		//	bitmap.UnlockBits(bitmapData);
		//	return bitmapSource;
		//}

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
		public RelayCommand CommandAddRoi
		{
			get { return new RelayCommand(_addRoi); }
		}
		public RelayCommand CommandAddParam
		{
			get { return new RelayCommand(_addParam); }
		}
		#endregion
	}
}
