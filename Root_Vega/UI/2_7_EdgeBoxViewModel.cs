using ATI;
using Emgu.CV;
using Root_Vega.Module;
using RootTools;
using RootTools.Inspects;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MBrushes = System.Windows.Media.Brushes;
using DPoint = System.Drawing.Point;

namespace Root_Vega
{
	class _2_7_EdgeBoxViewModel : ObservableObject
	{
		protected Dispatcher _dispatcher;
		Vega_Engineer m_Engineer;
		DialogService m_DialogService;
		MemoryTool m_MemoryModule;
		List<string> m_astrMem = new List<String> { "Top", "Left", "Right", "Bottom" };
		public List<DrawHistoryWorker> m_DrawHistoryWorker_List;

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
					this.UseCustomEdgeBox = value.EdgeBox.UseCustomEdgeBox;
					this.SearchBrightToDark = value.EdgeBox.SearchBrightToDark;
					this.UseAutoGV = value.EdgeBox.UseAutoGV;

					DrawEdgeBox(value, value.EdgeBox.UseCustomEdgeBox);
				}
			}
		}
		#endregion

		#region SearchBrightToDark
		private bool _SearchBrightToDark;
		public bool SearchBrightToDark
		{
			get { return this._SearchBrightToDark; }
			set
			{
				if (SelectedROI != null)
				{
					if (_SearchBrightToDark != value)
					{
						SetProperty(ref _SearchBrightToDark, value);
						SelectedROI.EdgeBox.SearchBrightToDark = value;
					}
				}
			}
		}
		#endregion

		#region UseCustomEdgeBox
		private bool _UseCustomEdgeBox;
		public bool UseCustomEdgeBox
		{
			get { return this._UseCustomEdgeBox; }
			set
			{
				if (SelectedROI != null)
				{
					if (_UseCustomEdgeBox != value)
					{
						SetProperty(ref _UseCustomEdgeBox, value);
						SelectedROI.EdgeBox.UseCustomEdgeBox = value;

						DrawEdgeBox(SelectedROI, value);
					}
				}
			}
		}
		#endregion

		#region UseAutoGV
		private bool _UseAutoGV;
		public bool UseAutoGV
		{
			get { return this._UseAutoGV; }
			set
			{
				if (SelectedROI != null)
				{
					SetProperty(ref _UseAutoGV, value);
					SelectedROI.EdgeBox.UseAutoGV = value;
				}
			}
		}
		#endregion

		#region EdgeThreshold
		int edgeThreshold = 40;
		public int EdgeThreshold
		{
			get { return edgeThreshold; }
			set
			{
				SetProperty(ref edgeThreshold, value);
				SelectedROI.EdgeBox.EdgeThreshold = value;
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

		public _2_7_EdgeBoxViewModel(Vega_Engineer engineer, IDialogService dialogService)
		{
			_dispatcher = Dispatcher.CurrentDispatcher;
			m_Engineer = engineer;
			m_DialogService = (DialogService)dialogService;
			Init(dialogService);
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
					p_ImageViewer_List[i].SetDrawer((DrawToolVM)p_SimpleShapeDrawer_List[i]);
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
				p_SideRoiList = new ObservableCollection<Roi>(m_Engineer.m_recipe.RecipeData.RoiList.Where(x => x.RoiType == Roi.Item.ReticleSide));
			};


			return;
		}
		void _commandDeleteEdgeInfo()
		{
			ClearDrawList();
		}
		public void _saveEdgeRcp()
		{
			//recipe에 저장되는 것 한정임. Init에 있는걸 레시피로 복사하는건 별도 기능으로 구현해야 함
			int checkCount = 0;
			for (int i = 0; i < 4; i++)
			{
				checkCount += p_SimpleShapeDrawer_List[i].m_ListRect.Count;
			}
			if (checkCount != 24 && checkCount != 0)
			{
				//현재 그려진 Rect를 기준으로 저장 시퀀스에 들어간다. rect 구성이 정상적이지 않으면 에러가 발생하고 저장진행을 중단한다
				return;
			}

			if (SelectedROI == null)
			{
				//ERROR가 뜨면서 저장 방지
				return;
			}
			//커스텀 EdgeBox 설정이 켜져있을 때만 recipe에 EdgeList를 업데이트한다
			if (SelectedROI.EdgeBox != null && SelectedROI.EdgeBox.EdgeList != null)
			{
				SelectedROI.EdgeBox.EdgeList.Clear();

				//여기서 그려진 모든 rect목록을 현재 엔지니어가 들고있는 레시피에 반영한다
				for (int i = 0; i < 4; i++)
				{
					if (p_SimpleShapeDrawer_List[i] == null) continue;
					for (int j = 0; j < 6; j++)
					{
						if (p_SimpleShapeDrawer_List[i].m_ListRect.Count < 6) break;
						SelectedROI.EdgeBox.EdgeList.Add(new EdgeElement(i, new CRect(p_SimpleShapeDrawer_List[i].m_ListRect[j].StartPos, p_SimpleShapeDrawer_List[i].m_ListRect[j].EndPos)));

					}
				}
			}

			//TODO : 원하는 파라메터만 갱신해서 저장할 수 있는 기능이 있으면 좋을 것 같음!
			var target = System.IO.Path.Combine(System.IO.Path.Combine(@"C:\VEGA\Recipe", m_Engineer.m_recipe.RecipeName));
			m_Engineer.m_recipe.Save(target);
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
		private void _initSave()
		{
			var tempToolset = (InspectToolSet)m_Engineer.ClassToolBox().GetToolSet("Inspect");
			var tempInspect = tempToolset.GetInspect("SideVision.Inspect");

			//현재 화면에서 보이는 rect목록을 Init에 반영한다
			for (int i = 0; i < 4; i++)
			{
				if (p_SimpleShapeDrawer_List[i] == null) continue;
				for (int j = 0; j < 6; j++)
				{
					if (p_SimpleShapeDrawer_List[i].m_ListRect.Count < 6) break;
					//SelectedROI.EdgeBox.EdgeList.Add();
					var temp = new EdgeElement(i, new CRect(p_SimpleShapeDrawer_List[i].m_ListRect[j].StartPos, p_SimpleShapeDrawer_List[i].m_ListRect[j].EndPos));

					tempInspect.nTopLeftXLIst[i * 6 + j] = temp.Rect.Left;
					tempInspect.nTopLeftYLIst[i * 6 + j] = temp.Rect.Top;
					tempInspect.nWidthLIst[i * 6 + j] = temp.Rect.Width;
					tempInspect.nHeighLIst[i * 6 + j] = temp.Rect.Height;
				}
			}

			bool bVisible = (m_Engineer.p_user.m_eLevel >= Login.eLevel.Admin);

			tempInspect.UpdateRegData();
		}
		private void _copyInitToRcp()
		{
			//Init의 edge 데이터를 현재 선택된 ROI Recipe Data에 덮어쓴다
			if (!m_Engineer.m_recipe.Loaded)
			{
				return;
			}
			if (SelectedROI == null)
			{
				return;
			}
			if (SelectedROI.EdgeBox == null)
			{
				return;
			}
			if (SelectedROI.EdgeBox.EdgeList == null)
			{
				return;
			}

			SelectedROI.EdgeBox.EdgeList.Clear();

			var tempToolset = (InspectToolSet)m_Engineer.ClassToolBox().GetToolSet("Inspect");
			var tempInspect = tempToolset.GetInspect("SideVision.Inspect");


			for (int i = 0; i < 4; i++)
			{
				if (p_SimpleShapeDrawer_List[i] == null) continue;
				for (int j = 0; j < 6; j++)
				{
					var temp = new EdgeElement(i, new CRect(
						new Point(tempInspect.nTopLeftXLIst[i * 6 + j], tempInspect.nTopLeftYLIst[i * 6 + j]),
						new Point(tempInspect.nTopLeftXLIst[i * 6 + j] + tempInspect.nWidthLIst[i * 6 + j], tempInspect.nTopLeftYLIst[i * 6 + j] + tempInspect.nHeighLIst[i * 6 + j])));
					SelectedROI.EdgeBox.EdgeList.Add(temp);

					var tempElement = new UIElementInfo(new Point(temp.Rect.Left, temp.Rect.Top), new Point(temp.Rect.Right, temp.Rect.Bottom));

					System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
					rect.Width = temp.Rect.Width;
					rect.Height = temp.Rect.Height;
					System.Windows.Controls.Canvas.SetLeft(rect, temp.Rect.Left);
					System.Windows.Controls.Canvas.SetTop(rect, temp.Rect.Top);
					rect.StrokeThickness = 2;
					rect.Stroke = MBrushes.Red;

					p_SimpleShapeDrawer_List[i].m_ListShape.Add(rect);
					p_SimpleShapeDrawer_List[i].m_Element.Add(rect);
					p_SimpleShapeDrawer_List[i].m_ListRect.Add(tempElement);
				}
				p_ImageViewer_List[i].SetRoiRect();
			}
		}
		private void _copyRcpToInit()
		{
			//현재 선택된 ROI Recipe Data에 Init의 edge 데이터를 덮어쓴다
			if (!m_Engineer.m_recipe.Loaded)
			{
				return;
			}
			if (SelectedROI == null)
			{
				return;
			}
			if (SelectedROI.EdgeBox == null)
			{
				return;
			}
			if (SelectedROI.EdgeBox.EdgeList == null)
			{
				return;
			}
			if (SelectedROI.EdgeBox.EdgeList.Count != 24)
			{
				return;
			}

			var tempToolset = (InspectToolSet)m_Engineer.ClassToolBox().GetToolSet("Inspect");
			var tempInspect = tempToolset.GetInspect("SideVision.Inspect");

			int count = 0;
			foreach (var item in SelectedROI.EdgeBox.EdgeList)
			{
				if (count >= 6)
				{
					count = 0;
				}
				var temp = new EdgeElement(item.SavePoint, new CRect(new Point(item.Rect.Left, item.Rect.Top), new Point(item.Rect.Right, item.Rect.Bottom)));

				tempInspect.nTopLeftXLIst[temp.SavePoint * 6 + count] = temp.Rect.Left;
				tempInspect.nTopLeftYLIst[temp.SavePoint * 6 + count] = temp.Rect.Top;
				tempInspect.nWidthLIst[temp.SavePoint * 6 + count] = temp.Rect.Width;
				tempInspect.nHeighLIst[temp.SavePoint * 6 + count] = temp.Rect.Height;
				count++;
			}

			bool bVisible = (m_Engineer.p_user.m_eLevel >= Login.eLevel.Admin);

			tempInspect.UpdateRegData();
		}
		void searchArea()
		{
			// variable
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
					aptEdges.Add(InspectionManager.GetEdge(ivvm.p_ImageData, arcROIs[j], eTempDirection, UseAutoGV, SearchBrightToDark, EdgeThreshold));
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

				p_ImageViewer_List[i].SetRoiRect();
			}
			//m_Engineer.m_InspManager.StartInspection();
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

		#region RelayCommand
		public RelayCommand CommandSearch
		{
			get
			{
				return new RelayCommand(searchArea);
			}
		}
		public RelayCommand CommandSave
		{
			get
			{
				return new RelayCommand(_saveEdgeRcp);
			}
		}
		public RelayCommand CommandDeleteEdgeInfo
		{
			get
			{
				return new RelayCommand(_commandDeleteEdgeInfo);
			}
		}
		public RelayCommand CommandAddRoi
		{
			get { return new RelayCommand(_addRoi); }
		}
		public RelayCommand CmdCopyRecipeToInit
		{
			get
			{
				return new RelayCommand(_copyRcpToInit);
			}
		}
		public RelayCommand CommandInitSave
		{
			get
			{
				return new RelayCommand(_initSave);
			}
		}
		public RelayCommand CmdCopyInitToRecipe
		{
			get
			{
				return new RelayCommand(_copyInitToRcp);
			}
		}
		#endregion
	}
}
