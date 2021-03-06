using ATI;
using Emgu.CV.Structure;
using Microsoft.Win32;
using RootTools;
using RootTools.Inspects;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
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
	public class _2_5_MainVisionViewModel : ObservableObject
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

		DispatcherTimer m_dispatcherTimer;
		double m_dProgressTotal = 0.0;
		double m_dProgressValue = 0.0;
		public double p_dProgressValue
        {
            get { return m_dProgressValue; }
            set { SetProperty(ref m_dProgressValue, value); }
        }

		//SqliteDataDB VSDBManager;
		//int currentDefectIdx;
		//System.Data.DataTable VSDataInfoDT;
		//System.Data.DataTable VSDataDT;

		//private string inspDefaultDir;
		//private string inspFileName;
		//bool bUsingInspection;

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

		//int tempImageWidth = 640;
		//int tempImageHeight = 480;

		//int currentSnap;
		//int wLimit;

		public _2_5_MainVisionViewModel(Vega_Engineer engineer, IDialogService dialogService)
		{
			_dispatcher = Dispatcher.CurrentDispatcher;
			m_Engineer = engineer;
			Init(engineer, dialogService);

			m_dispatcherTimer = new DispatcherTimer();
			m_dispatcherTimer.Interval = TimeSpan.FromTicks(10000000);
			m_dispatcherTimer.Tick += new EventHandler(timer_Tick);
			m_dispatcherTimer.Start();

			//m_Engineer.m_InspManager.AddDefect += M_InspManager_AddDefect;
			//bUsingInspection = false;
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (m_dProgressTotal == 0) p_dProgressValue = 0.0;
			else p_dProgressValue = (double)m_Engineer.m_InspManager.p_nPatternInspDoneNum / m_dProgressTotal * 100;
		}

		/// <summary>
		/// UI에 추가된 Defect을 빨간색 상자로 표시할 수 있도록 추가하는 메소드
		/// </summary>
		/// <param name="source">UI에 추가할 Defect List</param>
		/// <param name="args">arguments. 사용이 필요한 경우 수정해서 사용</param>
		private void M_InspManager_AddDefect(DefectDataWrapper item)
		{
			if (InspectionManager.GetInspectionType(item.nClassifyCode) == InspectionType.Strip && InspectionManager.GetInspectionTarget(item.nClassifyCode) == InspectionTarget.Chrome)
			{
				_dispatcher.BeginInvoke(new Action(delegate ()
				{
					//p_InformationDrawer.AddDefectInfo(item);
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

			m_AlignFeatureDrawer = new SimpleShapeDrawerVM(m_ImageViewer);
			m_AlignFeatureDrawer.m_Stroke = MBrushes.BlueViolet;
			m_AlignFeatureDrawer.RectangleKeyValue = Key.D1;
			alignEnabled = false;

			_SetRefDreawer();

			m_Engineer.m_recipe.LoadComplete += () =>
			{
				SelectedRecipe = m_Engineer.m_recipe;

				_SelectedROI = null;
				SelectedParam = new StripParamData();//UI 초기화를 위한 코드
				SelectedParam = null;

				p_PatternRoiList = new ObservableCollection<Roi>(m_Engineer.m_recipe.VegaRecipeData.RoiList.Where(x => x.RoiType == Roi.Item.ReticlePattern));
				SelectedROI = p_PatternRoiList.FirstOrDefault();

				if (SelectedROI != null)
				{
					StripParamList = new ObservableCollection<StripParamData>(SelectedROI.Strip.ParameterList);
					p_PatternReferenceList = new ObservableCollection<Reference>(SelectedROI.Position.ReferenceList);
					p_PatternAlignList = new ObservableCollection<AlignData>(SelectedROI.Position.AlignList);
				}
			};
			m_Engineer.m_recipe.RecipeData.AddComplete += () => 
			{
				p_PatternRoiList = new ObservableCollection<Roi>(m_Engineer.m_recipe.VegaRecipeData.RoiList.Where(x => x.RoiType == Roi.Item.ReticlePattern));
				StripParamList = new ObservableCollection<StripParamData>();
				p_PatternReferenceList = new ObservableCollection<Reference>();
				p_PatternAlignList = new ObservableCollection<AlignData>();

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
				if (p_PatternRoiList.Count > 0)
				{
					SelectedROI = p_PatternRoiList[0];
				}
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

		#region SelectedParam
		StripParamData _SelectedParam;
		public StripParamData SelectedParam
		{
			get { return _SelectedParam; }
			set
			{
				SetProperty(ref _SelectedParam, value);
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
		System.Windows.Media.Imaging.BitmapSource m_bmpFeatureSrc = null;
		public System.Windows.Media.Imaging.BitmapSource p_bmpFeatureSrc
		{
			get
			{
				return m_bmpFeatureSrc;
			}
			set
			{
				SetProperty(ref m_bmpFeatureSrc, value);
			}
		}
		Feature _SelectedFeature;
		public Feature SelectedFeature
		{
			get { return this._SelectedFeature; }
			set
			{
				SetProperty(ref _SelectedFeature, value);
				if (_SelectedFeature != null)
                {
					if (_SelectedFeature.m_Feature != null)
						p_bmpFeatureSrc = _SelectedFeature.m_Feature.GetBitMapSource();
				}
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
        System.Windows.Media.Imaging.BitmapSource m_bmpAlignSrc = null;
		public System.Windows.Media.Imaging.BitmapSource p_bmpAlignSrc
        {
            get
            {
				return m_bmpAlignSrc;
            }
            set
            {
				SetProperty(ref m_bmpAlignSrc, value);
            }
        }
		AlignData _SelectedAlign;
		public AlignData SelectedAlign
		{
			get { return this._SelectedAlign; }
			set
			{
				SetProperty(ref _SelectedAlign, value);
				if (_SelectedAlign != null)
                {
					if (_SelectedAlign.m_Feature != null) p_bmpAlignSrc = _SelectedAlign.m_Feature.GetBitMapSource();
				}
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
		private SimpleShapeDrawerVM m_AlignFeatureDrawer;
		public SimpleShapeDrawerVM p_AlignFeatureDrawer
		{
			get
			{
				return m_AlignFeatureDrawer;
			}
			set
			{
				SetProperty(ref m_AlignFeatureDrawer, value);
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

        #region AffineTransform
		//-----------------------------------------------------------------
        public void Test()
		{
			System.Drawing.PointF[] ptSrc = new System.Drawing.PointF[3];
			ptSrc[0] = new System.Drawing.PointF(50, 50);
			ptSrc[1] = new System.Drawing.PointF(200, 50);
			ptSrc[2] = new System.Drawing.PointF(50, 200);

			System.Drawing.PointF[] ptDst = new System.Drawing.PointF[3];
			ptDst[0] = new System.Drawing.PointF(310, 100);
			ptDst[1] = new System.Drawing.PointF(500, 50);
			ptDst[2] = new System.Drawing.PointF(400, 250);

			Emgu.CV.Mat matAffine = Emgu.CV.CvInvoke.GetAffineTransform(ptSrc, ptDst);

			float[] Coef = GetAffineArrayFromMat(matAffine);

			DrawCross(new DPoint((int)ptSrc[0].X, (int)ptSrc[0].Y), MBrushes.Red);
			DrawCross(new DPoint((int)ptSrc[1].X, (int)ptSrc[1].Y), MBrushes.Red);
			DrawCross(new DPoint((int)ptSrc[2].X, (int)ptSrc[2].Y), MBrushes.Red);
			DrawLine(new DPoint((int)ptSrc[0].X, (int)ptSrc[0].Y), new DPoint((int)ptSrc[1].X, (int)ptSrc[1].Y), MBrushes.White);
			DrawLine(new DPoint((int)ptSrc[1].X, (int)ptSrc[1].Y), new DPoint((int)ptSrc[2].X, (int)ptSrc[2].Y), MBrushes.White);
			DrawLine(new DPoint((int)ptSrc[2].X, (int)ptSrc[2].Y), new DPoint((int)ptSrc[0].X, (int)ptSrc[0].Y), MBrushes.White);

			DrawCross(new DPoint((int)ptDst[0].X, (int)ptDst[0].Y), MBrushes.Red);
			DrawCross(new DPoint((int)ptDst[1].X, (int)ptDst[1].Y), MBrushes.Red);
			DrawCross(new DPoint((int)ptDst[2].X, (int)ptDst[2].Y), MBrushes.Red);
			DrawLine(new DPoint((int)ptDst[0].X, (int)ptDst[0].Y), new DPoint((int)ptDst[1].X, (int)ptDst[1].Y), MBrushes.Yellow);
			DrawLine(new DPoint((int)ptDst[1].X, (int)ptDst[1].Y), new DPoint((int)ptDst[2].X, (int)ptDst[2].Y), MBrushes.Yellow);
			DrawLine(new DPoint((int)ptDst[2].X, (int)ptDst[2].Y), new DPoint((int)ptDst[0].X, (int)ptDst[0].Y), MBrushes.Yellow);

			System.Drawing.PointF ptTest;
			System.Drawing.PointF ptRst;
			for (int n = 0; n < 10; n++)
			{
				ptTest = new System.Drawing.PointF(100, 100 + n * 10);
				ptRst = AffineTransform(ptTest, Coef);
				DrawCross(new DPoint((int)ptTest.X, (int)ptTest.Y), MBrushes.Green);
				DrawCross(new DPoint((int)ptRst.X, (int)ptRst.Y), MBrushes.Orange);
			}

			return;
		}
		//-----------------------------------------------------------------
		System.Drawing.PointF AffineTransform(System.Drawing.PointF ptSrc, float[] Coef)
		{
			System.Drawing.PointF ptRst = new System.Drawing.PointF();
			ptRst.X = (int)Math.Ceiling(ptSrc.X * Coef[0] + ptSrc.Y * Coef[1] + Coef[2]);
			ptRst.Y = (int)Math.Ceiling(ptSrc.X * Coef[3] + ptSrc.Y * Coef[4] + Coef[5]);
			return ptRst;
		}
		//-----------------------------------------------------------------
		float[] GetAffineArrayFromMat(Emgu.CV.Mat matAffine)
        {
			var data = matAffine.GetData();
			float[] Coef = new float[matAffine.Width * matAffine.Height];
			int k = 0;
			for (int i = 0; i<matAffine.Height; i++)
            {
				for (int j = 0; j<matAffine.Width; j++)
                {
					double dTemp = (double)data.GetValue(i, j);
					Coef[k] = (float)dTemp;
					k++;
                }
            }
			return Coef;
        }
		//-----------------------------------------------------------------
		#endregion

		public void _clearInspReslut()
		{
			m_Engineer.m_InspManager._clearInspReslut();
		}

		public void ClearDrawList()
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



		public void _startInsp()
		{
			//TODO EdgeboxViewModel과 SideViewModel에 중복된 메소드가 존재하므로 통합이 가능한경우 정리하도록 합시다
			if (!m_Engineer.m_recipe.Loaded)
				return;

			//0. 개수 초기화 및 Table Drop
			//_clearInspReslut();

			ClearDrawList();
			m_dProgressTotal = 0;

			((Vega_Handler)m_Engineer.ClassHandler()).m_patternVision.p_nTotalBlockCount = 0;
			App.m_engineer.m_InspManager.m_bFeatureSearchFail = false;
			App.m_engineer.m_InspManager.m_bAlignFail = false;

			//2. 획득한 영역을 기준으로 검사영역을 생성하고 검사를 시작한다
			for (int k = 0; k < p_PatternRoiList.Count; k++)
			{
				var roiCurrent = p_PatternRoiList[k];
				//ROI 개수만큼 회전하면서 검사영역을 생성한다
				for (int j = 0; j < roiCurrent.Strip.ParameterList.Count; j++)
				{
					//검사영역 생성 기준
					//1. 등록된 feature를 탐색한다. 지정된 score에 부합하는 feature가 없을 경우 2차, 3차로 넘어갈 수도 있다. 
					//1.1. 만약 등록된 Feature가 없는 경우 기준 위치는 0,0으로한다
					CPoint cptStandard = new CPoint(0, 0);
					int nRefStartOffsetX = 0;
					int nRefStartOffsetY = 0;

					#region Feature
					foreach (var feature in roiCurrent.Position.ReferenceList)
					{
						bool bFoundFeature = false;
						CRect crtSearchArea;
						Point ptMaxRelative;
						int nWidthDiff, nHeightDiff;
						bFoundFeature = FindFeature(feature, out crtSearchArea, out ptMaxRelative, out nWidthDiff, out nHeightDiff);

						if (bFoundFeature)
						{
							//2. feature 중심위치가 확보되면 해당 좌표를 저장
							cptStandard.X = crtSearchArea.Left + (int)ptMaxRelative.X + nWidthDiff / 2;
							cptStandard.Y = crtSearchArea.Top + (int)ptMaxRelative.Y + nHeightDiff / 2;
							nRefStartOffsetX = feature.PatternDistX;
							nRefStartOffsetY = feature.PatternDistY;
							DrawCross(new DPoint(cptStandard.X, cptStandard.Y), MBrushes.Red);

							break;//찾았으니 중단
						}
						else
						{
							App.m_engineer.m_InspManager.m_bFeatureSearchFail = true;
							continue;//못 찾았으면 다음 Feature값으로 이동
						}
					}
					#endregion

					//4. 저장된 좌표를 기준으로 PatternDistX, PatternDistY만큼 더한다. 이 좌표가 Start Position이 된다
					var startPos = new Point(cptStandard.X + nRefStartOffsetX, cptStandard.Y + nRefStartOffsetY);
					//5. Start Position에 InspAreaWidth와 InspAreaHeight만큼 더해준다. 이 좌표가 End Position이 된다
					var endPos = new Point(startPos.X + (int)roiCurrent.Strip.ParameterList[j].InspAreaWidth, startPos.Y + (int)roiCurrent.Strip.ParameterList[j].InspAreaHeight);
					//6. Start Postiion과 End Position, Inspection Offset을 이용하여 검사 영역을 생성한다. 우선은 일괄 생성을 대상으로 한다
					var inspRect = new CRect(startPos, endPos);

					//var temp = new UIElementInfo(new Point(inspRect.Left, inspRect.Top), new Point(inspRect.Right, inspRect.Bottom));

					//System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
					//rect.Width = inspRect.Width;
					//rect.Height = inspRect.Height;
					//System.Windows.Controls.Canvas.SetLeft(rect, inspRect.Left);
					//System.Windows.Controls.Canvas.SetTop(rect, inspRect.Top);
					//rect.StrokeThickness = 3;
					//rect.Stroke = MBrushes.Orange;

					//p_RefFeatureDrawer.m_ListShape.Add(rect);
					//p_RefFeatureDrawer.m_Element.Add(rect);
					//p_RefFeatureDrawer.m_ListRect.Add(temp);

					//p_ImageViewer.SetRoiRect();

					int nDefectCode = InspectionManager.MakeDefectCode(InspectionTarget.Chrome, InspectionType.Strip, k);

					m_Engineer.m_InspManager.SetStandardPos(nDefectCode, cptStandard);

					MemoryData memory = m_Engineer.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem);
					IntPtr p = memory.GetPtr(0);
					m_dProgressTotal = m_Engineer.m_InspManager.CreateInspArea(App.sPatternPool, App.sPatternGroup, App.sPatternmem, m_Engineer.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem).GetMBOffset(),
							m_Engineer.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem).p_sz.X,
							m_Engineer.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem).p_sz.Y,
							inspRect, 500, roiCurrent.Strip.ParameterList[j], nDefectCode, m_Engineer.m_recipe.VegaRecipeData.UseDefectMerge, m_Engineer.m_recipe.VegaRecipeData.MergeDistance, 0, p).Count;
					//7. Strip검사를 시작한다
				}
			}
			m_Engineer.m_InspManager.StartInspection();//검사 시작!
		}

		int GetTotalBlockCountInPatternInspArea(CRect crtArea, int nBlockSize)
		{
			// variable
			int nOriginWidth = crtArea.Width;
			int nOriginHeight = crtArea.Height;
			int nHorizontalBlockCount = 0;
			int nVerticalBlockCount = 0;
			int nTotalBlockCount = 0;

			// implement
			nHorizontalBlockCount = nOriginWidth / nBlockSize;
			if (nOriginWidth % nBlockSize != 0) nHorizontalBlockCount++;

			nVerticalBlockCount = nOriginHeight / nBlockSize;
			if (nOriginHeight % nBlockSize != 0) nVerticalBlockCount++;

			nTotalBlockCount = nHorizontalBlockCount * nVerticalBlockCount;

			return nTotalBlockCount;
		}

		public void _endInsp()
		{
			#region Align Key
			// 등록된 Align Key 3개를 탐색한다. feature의 위치 정보도 참조하여 회전 보정 시에 들어갈 값을 준비해둔다
			float[] farrAffineMatrix = null;
			var roiCurrent = p_PatternRoiList[0];
			if (roiCurrent.Position.AlignList.Count == 3)
			{
				farrAffineMatrix = GetAffineArray(roiCurrent);
			}

			// 회전보정
			if (farrAffineMatrix != null)
			{
				//align 탐색 성공. InspectionManager로 farrCoef 던져줘라
				m_Engineer.m_InspManager.m_farrAfineMatrix = farrAffineMatrix;
			}
			else
			{
				// align 실패
				m_Engineer.m_InspManager.m_bAlignFail = true;
			}
			#endregion
			m_Engineer.m_InspManager.InspectionDone(App.indexFilePath, m_Engineer.m_recipe.VegaRecipeData.UseDefectMerge, m_Engineer.m_recipe.VegaRecipeData.MergeDistance);
		}

		public CRect GetOverlapedRect(CRect crtFirst, CRect crtSecond)
		{
			System.Drawing.Rectangle rtFirst = new System.Drawing.Rectangle(crtFirst.Left, crtFirst.Top, crtFirst.Width, crtFirst.Height);
			System.Drawing.Rectangle rtSecond = new System.Drawing.Rectangle(crtSecond.Left, crtSecond.Top, crtSecond.Width, crtSecond.Height);
			System.Drawing.Rectangle rtResult = System.Drawing.Rectangle.Intersect(rtFirst, rtSecond);
			CRect crtResult = new CRect(rtResult.Left, rtResult.Top, rtResult.Right, rtResult.Bottom);

			return crtResult;
		}

		public bool IsFeatureScanned(int nMemoryOffset, int nCamWidth)
		{
			// variable
			CRect crtSearchArea;
			CPoint cptCenter;
			Point ptStart;
			Point ptEnd;

			// implement
			if (m_Engineer.m_recipe.Loaded)
			{
				for (int k = 0; k < p_PatternRoiList.Count; k++)
				{
					var roiCurrent = p_PatternRoiList[k];
					for (int j = 0; j < roiCurrent.Strip.ParameterList.Count; j++)
					{
						foreach (var feature in roiCurrent.Position.ReferenceList)
						{
							cptCenter = feature.RoiRect.Center();
							ptStart = new Point(cptCenter.X - (feature.FeatureFindArea / 2.0), cptCenter.Y - (feature.FeatureFindArea / 2.0));
							ptEnd = new Point(cptCenter.X + (feature.FeatureFindArea / 2.0), cptCenter.Y + (feature.FeatureFindArea / 2.0));
							crtSearchArea = new CRect(ptStart, ptEnd);
							if (crtSearchArea.Right < (nMemoryOffset + nCamWidth))
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		public bool FindFeature(Feature feature, out CRect crtSearchArea, out Point ptMaxRelative, out int nWidthDiff, out int nHeightDiff)
		{
			System.Drawing.Bitmap bmp = feature.m_Feature.GetRectImage(new CRect(0, 0, feature.m_Feature.p_Size.X, feature.m_Feature.p_Size.Y));
			Emgu.CV.Image<Gray, byte> imgFeature = new Emgu.CV.Image<Gray, byte>(bmp);
			//Emgu.CV.Image<Gray, float> imgLaplaceFeature = imgFeature.Laplace(1);

			CPoint cptCenter = feature.RoiRect.Center();
			Point ptStart = new Point(cptCenter.X - (feature.FeatureFindArea / 2.0), cptCenter.Y - (feature.FeatureFindArea / 2.0));
			Point ptEnd = new Point(cptCenter.X + (feature.FeatureFindArea / 2.0), cptCenter.Y + (feature.FeatureFindArea / 2.0));
			crtSearchArea = new CRect(ptStart, ptEnd);
			Emgu.CV.Image<Gray, byte> imgSrc = new Emgu.CV.Image<Gray, byte>(p_ImageViewer.p_ImageData.GetRectImagePattern(crtSearchArea));
			//Emgu.CV.Image<Gray, float> imgSrcLaplace = imgSrc.Laplace(1);
			//Emgu.CV.Image<Gray, float> imgResult = imgSrcLaplace.MatchTemplate(imgLaplaceFeature, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);
			Emgu.CV.Image<Gray, float> imgResult = imgSrc.MatchTemplate(imgFeature, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);

			nWidthDiff = imgSrc.Width - imgResult.Width;
			nHeightDiff = imgSrc.Height - imgResult.Height;

			float[,,] matches = imgResult.Data;

			ptMaxRelative = new Point();//상대위치

			bool bFoundFeature = false;
			float fMaxScore = float.MinValue;

			for (int x = 0; x < matches.GetLength(1); x++)
			{
				for (int y = 0; y < matches.GetLength(0); y++)
				{
					if (fMaxScore < matches[y, x, 0] && feature.FeatureTargetScore <= matches[y, x, 0])
					{
						fMaxScore = matches[y, x, 0];
						ptMaxRelative.X = x;
						ptMaxRelative.Y = y;
						bFoundFeature = true;
					}
					//matches[y, x, 0] *= 256;
				}
			}
			imgResult.Data = matches;

			return bFoundFeature;
		}

		public void DrawCross(System.Drawing.Point pt, System.Windows.Media.SolidColorBrush brsColor)
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

		void _SaveReferenceFeature()
		{
			if (!refEnabled)
				return;

			if (p_RefFeatureDrawer.m_ListRect.Count >= 1)
			{
				var featureArea = p_RefFeatureDrawer.m_ListRect[0];
				var featureRect = new CRect(featureArea.StartPos, featureArea.EndPos);
				var featureImageArr = p_ImageViewer.p_ImageData.GetRectByteArray(featureRect);
				var targetName = string.Format("{0}_Ref_{1}.bmp", SelectedROI.Name, SelectedROI.Position.ReferenceList.Count);
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

		void _DeleteReferenceFeature()
        {
			_SetRefDreawer();
			
			if (p_PatternReferenceList.Count >= 1)
            {
				for (int i = 0; i<p_PatternReferenceList.Count; i++)
                {
					string strFileName = string.Format("{0}_Ref_{1}.bmp", SelectedROI.Name, i);
					File.Delete(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(SelectedRecipe.RecipePath), strFileName));
				}

				p_RefFeatureDrawer.m_ListShape.Clear();
				p_RefFeatureDrawer.m_Element.Clear();
				p_RefFeatureDrawer.m_ListRect.Clear();
				
				if (SelectedROI != null) SelectedROI.Position.ReferenceList.Clear();
				p_PatternReferenceList = new ObservableCollection<Reference>(SelectedROI.Position.ReferenceList);

				p_ImageViewer.SetRoiRect();
			}
		}

		void _DeleteAlignFeature()
        {
			_SetAlignDrawer();

			if (p_PatternAlignList.Count >= 1)
            {
				for (int i = 0; i<p_PatternAlignList.Count; i++)
                {
					string strFileName = string.Format("{0}_Align_{1}.bmp", SelectedROI.Name, i);
					File.Delete(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(SelectedRecipe.RecipePath), strFileName));
				}

				p_AlignFeatureDrawer.m_ListRect.Clear();
				p_AlignFeatureDrawer.m_Element.Clear();
				p_AlignFeatureDrawer.m_ListRect.Clear();

				if (SelectedROI != null) SelectedROI.Position.AlignList.Clear();
				p_PatternAlignList = new ObservableCollection<AlignData>(SelectedROI.Position.AlignList);

				p_ImageViewer.SetRoiRect();
            }
        }

		void _SaveAlignFeature()
		{
			if (!alignEnabled)
				return;

			if (p_AlignFeatureDrawer.m_ListRect.Count >= 1 && p_AlignFeatureDrawer.m_ListRect.Count <= 3)
			{
				for (int i = 0; i < p_AlignFeatureDrawer.m_ListRect.Count; i++)
				{
					var featureArea = p_AlignFeatureDrawer.m_ListRect[i];
					var featureRect = new CRect(featureArea.StartPos, featureArea.EndPos);
					var featureImageArr = p_ImageViewer.p_ImageData.GetRectByteArray(featureRect);
					var targetName = string.Format("{0}_Align_{1}.bmp", SelectedROI.Name, i);
					Emgu.CV.Image<Gray, byte> temp = new Emgu.CV.Image<Gray, byte>(featureRect.Width, featureRect.Height);
					temp.Bytes = featureImageArr;
					temp.Save(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(SelectedRecipe.RecipePath), targetName));

					AlignData tempFeature = new AlignData();
					tempFeature.Name = targetName;
					tempFeature.RoiRect = new CRect(featureArea.StartPos, featureArea.EndPos);
					tempFeature.m_Feature = new ImageData(featureRect.Width, featureRect.Height);
					tempFeature.m_Feature.LoadImageSync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(SelectedRecipe.RecipePath), targetName), new CPoint(0, 0));
					SelectedROI.Position.AlignList.Add(tempFeature);

					p_PatternAlignList = new ObservableCollection<AlignData>(SelectedROI.Position.AlignList);
				}
			}
		}

		public float[] GetAffineArray(Roi selectedRoi)
		{
			// variable
			Roi roi;
			AlignData alignData;
			CRect crtSearchArea;
			CPoint cptOriginCenter;
			Point ptMaxRelative;
			System.Drawing.PointF[] arrAlignKeyOriginPointF;
			System.Drawing.PointF[] arrAlignKeyTempPointF;
			System.Drawing.PointF ptfTemp;
			System.Drawing.PointF ptfOriginCenter;
			int nWidthDiff, nHeightDiff;
			bool bFoundFeature = false;
			Emgu.CV.Mat matAffine;
			float[] farrCoef;

			// implement
			roi = selectedRoi;
			arrAlignKeyOriginPointF = new System.Drawing.PointF[3];
			arrAlignKeyTempPointF = new System.Drawing.PointF[3];
			for (int j = 0; j < roi.Position.AlignList.Count; j++)
			{
				alignData = roi.Position.AlignList[j];
				bFoundFeature = FindFeature(alignData, out crtSearchArea, out ptMaxRelative, out nWidthDiff, out nHeightDiff);
				if (bFoundFeature)
				{
					// Recipe에 저장된 Align Feature
					cptOriginCenter = alignData.RoiRect.Center();
					ptfOriginCenter = new System.Drawing.PointF(cptOriginCenter.X, cptOriginCenter.Y);
					arrAlignKeyOriginPointF[j] = ptfOriginCenter;

					// 새로 Scan된 Align Feature
					ptfTemp = new System.Drawing.PointF();
					ptfTemp.X = crtSearchArea.Left + (float)ptMaxRelative.X + (float)nWidthDiff / 2;
					ptfTemp.Y = crtSearchArea.Top + (float)ptMaxRelative.Y + (float)nHeightDiff / 2;
					DrawCross(new DPoint((int)ptfTemp.X, (int)ptfTemp.Y), MBrushes.Crimson);
					arrAlignKeyTempPointF[j] = ptfTemp;
				}
                else
                {
					m_Engineer.m_InspManager.m_bAlignFail = true;
					return null;
                }
			}
			matAffine = Emgu.CV.CvInvoke.GetAffineTransform(arrAlignKeyTempPointF, arrAlignKeyOriginPointF);
			farrCoef = GetAffineArrayFromMat(matAffine);

			return farrCoef;
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
				return new RelayCommand(_startInsp);
			}
		}

		public ICommand CommandEndInsp
		{
			get
			{
				return new RelayCommand(_endInsp);
			}
		}

		public ICommand CommandAddParam
		{
			get
			{
				return new RelayCommand(_addParam);
			}
		}

		public ICommand SaveReferenceFeatureCommand
		{
			get
			{
				return new RelayCommand(_SaveReferenceFeature);
			}
		}

		public ICommand DeleteReferenceFeatureCommand
		{
			get
			{
				return new RelayCommand(_DeleteReferenceFeature);
			}
		}

		public ICommand DeleteAlignFeatureCommand
		{
			get
			{
				return new RelayCommand(_DeleteAlignFeature);
			}
		}

		public ICommand SaveAlignFeatureCommand
		{
			get
			{
				return new RelayCommand(_SaveAlignFeature);
			}
		}

		public ICommand ChangeToolForRef
		{
			get
			{
				return new RelayCommand(_SetRefDreawer);
			}
		}

		public ICommand ChangeToolForAlign
		{
			get
			{
				return new RelayCommand(_SetAlignDrawer);
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
