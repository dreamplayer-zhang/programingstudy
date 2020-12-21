using ATI;
using Emgu.CV.Structure;
using Microsoft.Win32;
using Root_Vega.Module;
using RootTools;
using RootTools.Inspects;
using RootTools.Memory;
using RootTools_CLR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using DPoint = System.Drawing.Point;
using MBrushes = System.Windows.Media.Brushes;

namespace Root_Vega
{


	public class _2_8_D2D_InspViewModel : ObservableObject
	{
		/// <summary>
		/// 외부 Thread에서 UI를 Update하기 위한 Dispatcher
		/// </summary>
		public Dispatcher _dispatcher;
		Vega_Engineer m_Engineer;
		MemoryTool m_MemoryModule;
		ImageData m_Image;
		Recipe m_Recipe;

		bool refEnabled;
		bool alignEnabled;

		SqliteDataDB VSDBManager;
		int currentDefectIdx;
		System.Data.DataTable VSDataInfoDT;
		System.Data.DataTable VSDataDT;

		private string inspDefaultDir;
		private string inspFileName;
		bool bUsingInspection;
		D2DInspect m_D2DInspect = new D2DInspect();
		ImageData DoubleSize;

		
		public Recipe p_Recipe
		{
			get
			{
				return m_Recipe;
			}
			set
			{
				SetProperty(ref m_Recipe, value);
			}
		}

		int tempImageWidth = 640;
		int tempImageHeight = 480;

		int currentSnap;
		int wLimit;

		public _2_8_D2D_InspViewModel(Vega_Engineer engineer, IDialogService dialogService)
		{
			_dispatcher = Dispatcher.CurrentDispatcher;
			m_Engineer = engineer;
			Init(engineer, dialogService);

			//m_Engineer.m_InspManager.AddDefect += M_InspManager_AddDefect;
			bUsingInspection = false;
		}
		/// <summary>
		/// UI에 추가된 Defect을 빨간색 상자로 표시할 수 있도록 추가하는 메소드
		/// </summary>
		/// <param name="source">UI에 추가할 Defect List</param>
		/// <param name="args">arguments. 사용이 필요한 경우 수정해서 사용</param>
		private void M_InspManager_AddDefect(DefectDataWrapper item)
		{
			if (InspectionManager.GetInspectionTarget(item.nClassifyCode) == InspectionTarget.D2D)
			{
				_dispatcher.BeginInvoke(new Action(delegate ()
				{
					p_InformationDrawer.AddDefectInfo(item);
					//p_ImageViewer.RedrawingElement();
				}));
			}
		}

		void Init(Vega_Engineer engineer, IDialogService dialogService)
		{
			p_Recipe = engineer.m_recipe;

			m_MemoryModule = engineer.ClassMemoryTool();

			m_Image = new ImageData(m_MemoryModule.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem));
			//p_ImageViewer = new ImageViewer_ViewModel(m_Image, dialogService);
			p_ImageViewer = new ImageViewer_ViewModel(m_Image, dialogService);
			m_DrawHistoryWorker_List.Add(new DrawHistoryWorker());
			m_DrawHistoryWorker_List.Add(new DrawHistoryWorker());

			p_InformationDrawer = new InformationDrawer(p_ImageViewer);

			p_SimpleShapeDrawer.Add(new SimpleShapeDrawerVM(p_ImageViewer));
			p_SimpleShapeDrawer.Add(new SimpleShapeDrawerVM(p_ImageViewer));
			p_SimpleShapeDrawer[0].RectangleKeyValue = Key.D1;
			p_SimpleShapeDrawer[1].RectangleKeyValue = Key.D1;
			p_ImageViewer.SetDrawer((DrawToolVM)p_SimpleShapeDrawer[0]);
			p_ImageViewer.SetInformationViewer(informationDrawer);
			p_ImageViewer.m_HistoryWorker = m_DrawHistoryWorker_List[0];

			m_RefFeatureDrawer = new SimpleShapeDrawerVM(m_ImageViewer);
			m_RefFeatureDrawer.RectangleKeyValue = Key.D1;
			refEnabled = false;

			_AlignFeatureDrawer = new SimpleShapeDrawerVM(m_ImageViewer);
			_AlignFeatureDrawer.m_Stroke = MBrushes.BlueViolet;
			_AlignFeatureDrawer.RectangleKeyValue = Key.D1;
			alignEnabled = false;

			_SetRefDreawer();

			m_Engineer.m_recipe.LoadComplete += () =>
			{
				SelectedRecipe = m_Engineer.m_recipe;
				p_PatternRoiList = new ObservableCollection<Roi>(m_Engineer.m_recipe.VegaRecipeData.RoiList.Where(x => x.RoiType == Roi.Item.ReticleSide));
				StripParamList = new ObservableCollection<StripParamData>();

				_SelectedROI = null;

				SelectedParam = new StripParamData();//UI 초기화를 위한 코드
				SelectedParam = null;
			};
		}

		#region Property

		#region SelectedRecipe
		VegaRecipe _SelectedRecipe;
		public VegaRecipe SelectedRecipe
		{
			get { return this._SelectedRecipe; }
			set
			{
				SetProperty(ref _SelectedRecipe, value);
			}
		}
		#endregion

		#region p_PatternRoiList
		ObservableCollection<Roi> _p_PatternRoiList;
		public ObservableCollection<Roi> p_PatternRoiList
		{
			get { return _p_PatternRoiList; }
			set
			{
				SetProperty(ref _p_PatternRoiList, value);
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
					StripParamList = new ObservableCollection<StripParamData>(value.Strip.ParameterList);
					p_PatternReferenceList = new ObservableCollection<Reference>(value.Position.ReferenceList);
					p_PatternAlignList = new ObservableCollection<AlignData>(value.Position.AlignList);
				}
			}
		}
		#endregion

		#region StripParamList

		ObservableCollection<StripParamData> _StripParamList;
		public ObservableCollection<StripParamData> StripParamList
		{
			get { return _StripParamList; }
			set
			{
				SetProperty(ref _StripParamList, value);
			}
		}
		#endregion



		public DieInfo p_DieInfo
		{
			get
			{

				return m_D2DInspect.m_D2DInfo.m_DieInfo;
			}
			set
			{
				SetProperty(ref m_D2DInspect.m_D2DInfo.m_DieInfo, value);
			}
		}

		public AlignInfo p_AlignInfo
		{
			get
			{

				return m_D2DInspect.m_D2DInfo.m_AlignInfo;
			}
			set
			{
				SetProperty(ref m_D2DInspect.m_D2DInfo.m_AlignInfo, value);
			}
		}



		#region SelectedParam
		StripParamData _SelectedParam;
		public StripParamData SelectedParam
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

		#region p_PatternReferenceList

		ObservableCollection<Reference> _PatternReferenceList;
		public ObservableCollection<Reference> p_PatternReferenceList
		{
			get { return _PatternReferenceList; }
			set
			{
				SetProperty(ref _PatternReferenceList, value);
			}
		}
		#endregion

		#region SelectedFeature
		Feature _SelectedFeature;
		public Feature SelectedFeature
		{
			get { return this._SelectedFeature; }
			set
			{
				SetProperty(ref _SelectedFeature, value);
			}
		}
		#endregion

		#region p_PatternAlignList

		ObservableCollection<AlignData> _PatternAlignList;
		public ObservableCollection<AlignData> p_PatternAlignList
		{
			get { return _PatternAlignList; }
			set
			{
				SetProperty(ref _PatternAlignList, value);
			}
		}
		#endregion

		#region SelectedAlign
		AlignData _SelectedAlign;
		public AlignData SelectedAlign
		{
			get { return this._SelectedAlign; }
			set
			{
				SetProperty(ref _SelectedAlign, value);
			}
		}
		#endregion

		public StripParamData p_StripParamData
		{
			get
			{
				if (m_Recipe.RecipeData.RoiList.Count == 0) return new StripParamData();
				if (m_Recipe.RecipeData.RoiList[p_IndexMask].Strip.ParameterList.Count != 0)
					return m_Recipe.RecipeData.RoiList[p_IndexMask].Strip.ParameterList[0];
				else
					return new StripParamData();
			}
			set
			{
				if (m_Recipe.RecipeData.RoiList[p_IndexMask].Strip.ParameterList.Count != 0)
					m_Recipe.RecipeData.RoiList[p_IndexMask].Strip.ParameterList[0] = value;
				RaisePropertyChanged();
			}
		}

		private List<SimpleShapeDrawerVM> m_SimpleShapeDrawer = new List<SimpleShapeDrawerVM>();
		public List<SimpleShapeDrawerVM> p_SimpleShapeDrawer
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
		public List<DrawHistoryWorker> m_DrawHistoryWorker_List = new List<DrawHistoryWorker>();

		private int _IndexMask = 0;
		public int p_IndexMask
		{
			get
			{
				return _IndexMask;
			}
			set
			{
				SetProperty(ref _IndexMask, value);
				//수정필요p_ImageViewer.SetRectElement_MemPos(p_Recipe.p_RecipeData.p_Roi[_IndexMask].m_Strip.m_NonPattern[0].m_rt);
				p_ImageViewer.SetDrawer((DrawToolVM)p_SimpleShapeDrawer[_IndexMask]);
				p_ImageViewer.m_HistoryWorker = m_DrawHistoryWorker_List[_IndexMask];
				p_ImageViewer.SetImageSource();
				p_StripParamData = p_Recipe.RecipeData.RoiList[_IndexMask].Strip.ParameterList[0];

			}
		}

		private InformationDrawer informationDrawer;
		public InformationDrawer p_InformationDrawer
		{
			get
			{
				return informationDrawer;
			}
			set
			{
				SetProperty(ref informationDrawer, value);
			}
		}

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

		#region p_RefFeatureDrawer
		private SimpleShapeDrawerVM m_RefFeatureDrawer;
		public SimpleShapeDrawerVM p_RefFeatureDrawer
		{
			get
			{
				return m_RefFeatureDrawer;
			}
			set
			{
				SetProperty(ref m_RefFeatureDrawer, value);
			}
		}
		#endregion

		#region p_AlignFeatureDrawer
		private SimpleShapeDrawerVM _AlignFeatureDrawer;
		public SimpleShapeDrawerVM p_AlignFeatureDrawer
		{
			get
			{
				return _AlignFeatureDrawer;
			}
			set
			{
				SetProperty(ref _AlignFeatureDrawer, value);
			}
		}
		#endregion

		private System.Windows.Input.Cursor _recipeCursor;
		public System.Windows.Input.Cursor RecipeCursor
		{
			get
			{
				return _recipeCursor;
			}
			set
			{
				SetProperty(ref _recipeCursor, value);
			}
		}

		private System.Windows.Input.MouseEventArgs _mouseEvent;
		public System.Windows.Input.MouseEventArgs MouseEvent
		{
			get
			{
				return _mouseEvent;
			}
			set
			{
				SetProperty(ref _mouseEvent, value);
			}
		}

		private bool _draw_IsChecked = false;
		public bool Draw_IsChecked
		{
			get
			{
				return _draw_IsChecked;
			}
			set
			{
				SetProperty(ref _draw_IsChecked, value);
				_btnDraw();
			}
		}
		#endregion

		#region Func
		private void _SetAlignDrawer()
		{
			p_ImageViewer.SetDrawer(p_AlignFeatureDrawer);
			alignEnabled = true;
			refEnabled = false;
		}

		private void _SetRefDreawer()
		{
			p_ImageViewer.SetDrawer(p_RefFeatureDrawer);
			alignEnabled = false;
			refEnabled = true;
		}

		void _addParam()
		{
			if (!m_Engineer.m_recipe.Loaded)
				return;

			int paramCount = SelectedROI.Strip.ParameterList.Count;
			string defaultName = string.Format("StripParam #{0}", paramCount);

			StripParamData temp = new StripParamData();
			temp.Name = defaultName;

			SelectedROI.Strip.ParameterList.Add(temp);

			StripParamList = new ObservableCollection<StripParamData>(SelectedROI.Strip.ParameterList);
		}
		void _addRoi()
		{
			if (!m_Engineer.m_recipe.Loaded)
				return;

			int roiCount = m_Engineer.m_recipe.VegaRecipeData.RoiList.Where(x => x.RoiType == Roi.Item.ReticlePattern).Count();
			string defaultName = string.Format("Pattern ROI #{0}", roiCount);

			Roi temp = new Roi(defaultName, Roi.Item.ReticlePattern);
			m_Engineer.m_recipe.VegaRecipeData.RoiList.Add(temp);

			p_PatternRoiList = new ObservableCollection<Roi>(m_Engineer.m_recipe.VegaRecipeData.RoiList.Where(x => x.RoiType == Roi.Item.ReticlePattern));
			if (m_Engineer.m_recipe.RecipeData.AddComplete != null)
			{
				m_Engineer.m_recipe.RecipeData.AddComplete();
			}
		}

		CPoint GetMemPoint(int canvasX, int canvasY)
		{
			int nX = p_ImageViewer.p_View_Rect.X + canvasX * p_ImageViewer.p_View_Rect.Width / p_ImageViewer.p_CanvasWidth;
			int nY = p_ImageViewer.p_View_Rect.Y + canvasY * p_ImageViewer.p_View_Rect.Height / p_ImageViewer.p_CanvasHeight;
			return new CPoint(nX, nY);
		}
		CPoint GetCanvasPoint(int memX, int memY)
		{
			if (p_ImageViewer.p_View_Rect.Width > 0 && p_ImageViewer.p_View_Rect.Height > 0)
			{

				int nX = (int)Math.Round((double)(memX - p_ImageViewer.p_View_Rect.X) * p_ImageViewer.p_CanvasWidth / p_ImageViewer.p_View_Rect.Width, MidpointRounding.ToEven);
				//int xx = (memX - p_ROI_Rect.X) * ViewWidth / p_ROI_Rect.Width;
				int nY = (int)Math.Round((double)(memY - p_ImageViewer.p_View_Rect.Y) * p_ImageViewer.p_CanvasHeight / p_ImageViewer.p_View_Rect.Height, MidpointRounding.AwayFromZero);
				return new CPoint(nX, nY);
			}
			return new CPoint(0, 0);
		}
		System.Windows.Media.Color ConvertColor(System.Drawing.Color color)
		{
			System.Windows.Media.Color c = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
			return c;
		}
		#endregion

		public void SaveCurrentMask()
		{
			var temp = p_SimpleShapeDrawer[0].m_ListRect[0];
			int left = (int)temp.StartPos.X;
			int top = (int)temp.StartPos.Y;
			int right = (int)temp.EndPos.X;
			int bot = (int)temp.EndPos.Y;
			CRect rect = new CRect(left, top, right, bot);
			p_Recipe.RecipeData.RoiList[p_IndexMask].Strip.NonPatternList[0].Area = rect;

		}

		void _clearInspReslut()
		{
			m_Engineer.m_InspManager._clearInspReslut();
		}

		void ClearDrawList()
		{
			if (refEnabled)
			{
				p_RefFeatureDrawer.Clear();
			}
			if (alignEnabled)
			{
				p_AlignFeatureDrawer.Clear();
			}
			p_InformationDrawer.Clear();

			p_ImageViewer.SetRoiRect();
			p_InformationDrawer.Redrawing();
		}


		public void D2DInsp()
		{
			//m_D2DInspect.set



			//MemoryData sD2Dmemdata = ((Vega_Handler)(m_Engineer.ClassHandler())).m_patternVision.m_memoryPool2.GetGroup(App.sD2DGroup).GetMemory(App.sD2Dmem);
			//MemoryData sD2DABSmemdata = ((Vega_Handler)(m_Engineer.ClassHandler())).m_patternVision.m_memoryPool2.GetGroup(App.sD2DGroup).GetMemory(App.sD2DABSmem);
			//m_D2DInspect.StartInsp(m_Image, sD2Dmemdata, sD2DABSmemdata);
			//메모리 defult 사이즈 어디서 지정해야하지?

			//MemoryPool pool =  m_Engineer.ClassMemoryTool().GetPool(App.sD2DPool, true);
			//pool.p_gbPool = 3;

			//memdata = m_Engineer.ClassMemoryTool().GetPool(App.sD2DPool,true).GetGroup(App.sD2DGroup).CreateMemory(App.sD2Dmem, 100, 1, 1000, 1000);

			//m_D2DInspect.DoubleImage[0][0] = new ImageData(m_MemoryModule.GetMemory(App.sD2DPool, App.sD2DGroup, App.sD2Dmem[0]));
		}








		void DrawCross(System.Drawing.Point pt, System.Windows.Media.SolidColorBrush brsColor)
		{
			DPoint ptLT = new DPoint(pt.X - 10, pt.Y - 10);
			DPoint ptRB = new DPoint(pt.X + 10, pt.Y + 10);
			DPoint ptLB = new DPoint(pt.X - 10, pt.Y + 10);
			DPoint ptRT = new DPoint(pt.X + 10, pt.Y - 10);

			DrawLine(ptLT, ptRB, brsColor);
			DrawLine(ptLB, ptRT, brsColor);
		}
		void DrawLine(System.Drawing.Point pt1, System.Drawing.Point pt2, System.Windows.Media.SolidColorBrush brsColor)
		{
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


			m_ImageViewer.SelectedTool.m_ListShape.Add(myLine);
			UIElementInfo uei = new UIElementInfo(new System.Windows.Point(myLine.X1, myLine.Y1), new System.Windows.Point(myLine.X2, myLine.Y2));
			m_ImageViewer.SelectedTool.m_ListRect.Add(uei);
			m_ImageViewer.SelectedTool.m_Element.Add(myLine);
		}

		void _SaveFeature()
		{
			//그려진 첫번째 빨간색영역 내의 이미지를 Feature 정보로 저장한다
			//그린 정보는 SelectedFeature가 변경되면 Update되어야한다
			if (!refEnabled)
				return;

			if (p_RefFeatureDrawer.m_ListRect.Count >= 1)
			{
				var featureArea = p_RefFeatureDrawer.m_ListRect[0];
				var featureRect = new CRect(featureArea.StartPos, featureArea.EndPos);
				var featureImageArr = p_ImageViewer.p_ImageData.GetRectByteArray(featureRect);
				var targetName = string.Format("{0}_Ref_{1}.bmp", SelectedROI.Name, SelectedROI.Position.ReferenceList.Count);
				//TODO 이상하게 구현함. 나중에 수정 필요
				Emgu.CV.Image<Gray, byte> temp = new Emgu.CV.Image<Gray, byte>(featureRect.Width, featureRect.Height);
				temp.Bytes = featureImageArr;
				temp.Save(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(SelectedRecipe.RecipePath), targetName));

				Reference tempFeature = new Reference();
				tempFeature.Name = targetName;
				tempFeature.RoiRect = new CRect(featureArea.StartPos, featureArea.EndPos);
				tempFeature.m_Feature = new ImageData(featureRect.Width, featureRect.Height);
				tempFeature.m_Feature.LoadImageSync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(SelectedRecipe.RecipePath), targetName), new CPoint(0, 0));
				SelectedROI.Position.ReferenceList.Add(tempFeature);

				p_PatternReferenceList = new ObservableCollection<Reference>(SelectedROI.Position.ReferenceList);
			}
		}

		#region Command
		public ICommand AddROICommand
		{
			get
			{
				return new RelayCommand(_addRoi);
			}
		}

		public ICommand ClearResultCommand
		{
			get
			{
				return new RelayCommand(_clearInspReslut);
			}
		}

		public ICommand ClearDrawingCommand
		{
			get
			{
				return new RelayCommand(ClearDrawList);
			}
		}

		public ICommand CommandStartInsp
		{
			get
			{
				return new RelayCommand(D2DInsp);
			}
		}

		public ICommand CommandAddParam
		{
			get
			{
				return new RelayCommand(_addParam);
			}
		}

		public ICommand SaveFeatureCommand
		{
			get
			{
				return new RelayCommand(_SaveFeature);
			}
		}

		public ICommand ChangeToolForRef
		{
			get
			{
				return new RelayCommand(_SetRefDreawer);
			}
		}

		public ICommand btnDraw
		{
			get
			{
				return new RelayCommand(_btnDraw);
			}
		}
		public ICommand btnDone
		{
			get
			{
				return new RelayCommand(_btnDone);
			}
		}
		public ICommand btnClear
		{
			get
			{
				return new RelayCommand(_btnClear);
			}
		}
		public ICommand btnInspTest
		{
			get
			{
				return new RelayCommand(_btnInspTest);
			}
		}
		public ICommand btnInspDone
		{
			get
			{
				return new RelayCommand(_btnInspDone);
			}
		}
		public RelayCommand CommandSaveMask
		{
			get
			{
				return new RelayCommand(SaveCurrentMask);
			}
		}



		private void ClearUI()
		{
			if (p_InformationDrawer != null)
				p_InformationDrawer.Clear();
		}
		private void _btnClear()
		{
			p_Recipe.RecipeData.RoiList[p_IndexMask].Strip.NonPatternList[0].Area = new CRect();

			p_ImageViewer.ClearShape();
			p_ImageViewer.SetImageSource();

			p_IndexMask = _IndexMask;
		}
		private void _btnDraw()
		{
			if (!Draw_IsChecked)
			{
			}
			else
			{
				RecipeCursor = Cursors.Cross;
			}
		}
		private void _btnDone()
		{
			Draw_IsChecked = false;
			RecipeCursor = Cursors.Arrow;
		}
		//private void _btnStartInsp()
		//{
		//	ClearUI();//재검사 전 UI 정리

		//	if (DrawRectList != null)
		//		DrawRectList.Clear();//검사영역 draw용 Rect List 정리

		//	currentDefectIdx = 0;
		//	currentSnap = 0;
		//	m_Engineer.m_InspManager.ClearInspection();

		//	CRect Mask_Rect = p_Recipe.RecipeData.RoiList[0].Strip.NonPatternList[0].Area;
		//	int nblocksize = 500;

		//	int AreaWidth = Mask_Rect.Width;

		//	wLimit = AreaWidth / nblocksize;
		//	System.Diagnostics.Debug.WriteLine(string.Format("Set wLimit : {0}", wLimit));

		//	DrawRectList = m_Engineer.m_InspManager.CreateInspArea(Mask_Rect, nblocksize,
		//		p_Recipe.RecipeData.RoiList[0].Strip.ParameterList[0],
		//		p_Recipe.RecipeData.UseDefectMerge, p_Recipe.RecipeData.MergeDistance, currentSnap, currentSnap + 1);

		//	currentSnap++;//한줄 추가

		//	System.Diagnostics.Debug.WriteLine("Start Insp");

		//	inspDefaultDir = @"C:\vsdb";
		//	if (!System.IO.Directory.Exists(inspDefaultDir))
		//	{
		//		System.IO.Directory.CreateDirectory(inspDefaultDir);
		//	}
		//	inspFileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_inspResult.vega_result";
		//	var targetVsPath = System.IO.Path.Combine(inspDefaultDir, inspFileName);
		//	string VSDB_configpath = @"C:/vsdb/init/vsdb.txt";

		//	if (VSDBManager != null && VSDBManager.IsConnected)
		//	{
		//		VSDBManager.Disconnect();
		//	}
		//	VSDBManager = new SqliteDataDB(targetVsPath, VSDB_configpath);

		//	if (VSDBManager.Connect())
		//	{
		//		VSDBManager.CreateTable("Datainfo");
		//		VSDBManager.CreateTable("Data");

		//		VSDataInfoDT = VSDBManager.GetDataTable("Datainfo");
		//		VSDataDT = VSDBManager.GetDataTable("Data");
		//	}
		//	int nDefectCode = InspectionManager.MakeDefectCode(InspectionTarget.Chrome, InspectionType.Strip, 0);
		//	m_Engineer.m_InspManager.StartInspection(nDefectCode, m_Image.p_Size.X, m_Image.p_Size.Y);
		//}
		private void _btnInspDone()
		{

		}
		//private void _btnNextSnap()
		//{
		//	int nDefectCode = InspectionManager.MakeDefectCode(InspectionTarget.Chrome, InspectionType.Strip, 0);
		//	if (wLimit == currentSnap)
		//	{
		//		return;
		//	}

		//	CRect Mask_Rect = p_Recipe.RecipeData.RoiList[0].Strip.NonPatternList[0].Area;
		//	int nblocksize = 500;

		//	DrawRectList = m_Engineer.m_InspManager.CreateInspArea(Mask_Rect, nblocksize,
		//		p_Recipe.RecipeData.RoiList[0].Strip.ParameterList[0],
		//		p_Recipe.RecipeData.UseDefectMerge, p_Recipe.RecipeData.MergeDistance, currentSnap, currentSnap + 1);

		//	currentSnap++;//한줄 추가
		//	m_Engineer.m_InspManager.StartInspection(nDefectCode, m_Image.p_Size.X, m_Image.p_Size.Y);
		//}
		List<CRect> DrawRectList;
		private void _btnInspTest()
		{
			ClearUI();//재검사 전 UI 정리
			bUsingInspection = true;
			currentSnap = 0;
			wLimit = 0;
			System.Diagnostics.Debug.WriteLine(string.Format("Set wLimit : {0}", wLimit));

			if (DrawRectList != null)
				DrawRectList.Clear();//검사영역 draw용 Rect List 정리

			m_Engineer.m_InspManager.ClearInspection();

			currentDefectIdx = 0;

			CRect Mask_Rect = p_Recipe.RecipeData.RoiList[0].Strip.NonPatternList[0].Area;
			int nblocksize = 500;

			var memOffset = m_Engineer.GetMemory("pool", "group", "mem").GetMBOffset();
			int nDefectCode = InspectionManager.MakeDefectCode(InspectionTarget.Chrome, InspectionType.Strip, 0);

			//DrawRectList = m_Engineer.m_InspManager.CreateInspArea("pool", "group", "mem", memOffset,
			//	m_Engineer.GetMemory("pool", "group", "mem").p_sz.X,
			//	m_Engineer.GetMemory("pool", "group", "mem").p_sz.Y,
			//	Mask_Rect, nblocksize,
			//	p_Recipe.RecipeData.RoiList[0].Strip.ParameterList[0],
			//	nDefectCode,
			//	p_Recipe.RecipeData.UseDefectMerge, p_Recipe.RecipeData.MergeDistance);

			System.Diagnostics.Debug.WriteLine("Start Insp");
			m_Engineer.m_InspManager.StartInspection();

			return;
		}
		#endregion



		enum SurfaceProgress
		{
			None,
			Start,
			Drawing,
			Done,
			Select,
			Adjusting,
		}
		enum HitType
		{
			None, Body, UL, UR, LR, LL, L, R, T, B
		};

	}
}
