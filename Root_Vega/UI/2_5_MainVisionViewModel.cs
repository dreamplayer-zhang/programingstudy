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
//using System.Drawing;
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
		int currentTotalIdx;

		bool refEnabled;
		bool alignEnabled;

		SqliteDataDB VSDBManager;
		int currentDefectIdx;
		System.Data.DataTable VSDataInfoDT;
		System.Data.DataTable VSDataDT;

		private string inspDefaultDir;
		private string inspFileName;
		bool bUsingInspection;

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

		public _2_5_MainVisionViewModel(Vega_Engineer engineer, IDialogService dialogService)
		{
			_dispatcher = Dispatcher.CurrentDispatcher;
			m_Engineer = engineer;
			Init(engineer, dialogService);

			m_Engineer.m_InspManager.AddDefect += M_InspManager_AddDefect;
			bUsingInspection = false;
		}
		/// <summary>
		/// UI에 추가된 Defect을 빨간색 상자로 표시할 수 있도록 추가하는 메소드
		/// </summary>
		/// <param name="source">UI에 추가할 Defect List</param>
		/// <param name="args">arguments. 사용이 필요한 경우 수정해서 사용</param>
		private void M_InspManager_AddDefect(DefectDataWrapper item)
		{
			if (InspectionManager.GetInspectionType(item.nClassifyCode) != InspectionType.Strip)
			{
				return;
			}
			//string tempInspDir = @"C:\vsdb\TEMP_IMAGE";

			System.Data.DataRow dataRow = VSDataDT.NewRow();

			//Data,@No(INTEGER),DCode(INTEGER),Size(INTEGER),Length(INTEGER),Width(INTEGER),Height(INTEGER),InspMode(INTEGER),FOV(INTEGER),PosX(INTEGER),PosY(INTEGER)

			dataRow["No"] = currentDefectIdx;
			currentDefectIdx++;
			dataRow["DCode"] = item.nClassifyCode;
			dataRow["AreaSize"] = item.fAreaSize;
			dataRow["Length"] = item.nLength;
			dataRow["Width"] = item.nWidth;
			dataRow["Height"] = item.nHeight;
			//dataRow["FOV"] = item.FOV;
			dataRow["PosX"] = item.fPosX;
			dataRow["PosY"] = item.fPosY;

			VSDataDT.Rows.Add(dataRow);
			_dispatcher.Invoke(new Action(delegate ()
			{
				p_InformationDrawer.AddDefectInfo(item);
				p_ImageViewer.RedrawingElement();
			}));
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
			for(int n = 0; n<10; n++)
			{
				ptTest = new System.Drawing.PointF(100, 100 + n * 10);
				ptRst = AffineTransform(ptTest, Coef);
				DrawCross(new DPoint((int)ptTest.X, (int)ptTest.Y), MBrushes.Green);
				DrawCross(new DPoint((int)ptRst.X, (int)ptRst.Y), MBrushes.Orange);
			}
			
			return;
		}

		System.Drawing.PointF AffineTransform(System.Drawing.PointF ptSrc, float[] Coef)
		{
			System.Drawing.PointF ptRst = new System.Drawing.PointF();
			ptRst.X = RoundingUp((float)(ptSrc.X * Coef[0] + ptSrc.Y * Coef[1] + Coef[2]));
			ptRst.Y = RoundingUp((float)(ptSrc.X * Coef[3] + ptSrc.Y * Coef[4] + Coef[5]));
			return ptRst;
		}

		int RoundingUp(double d)
		{
			// 소수점 올림
			int n = (int)d;
			if (d > n)
				return n + 1;

			return n;
		}

		void _clearInspReslut()
		{
			currentTotalIdx = 0;

			DBConnector connector = new DBConnector("localhost", "Inspections", "root", "`ati5344");
			if (connector.Open())
			{
				string dropQuery = "DROP TABLE Inspections.tempdata";
				var result = connector.SendNonQuery(dropQuery);
				string createQuery = "CREATE TABLE tempdata(idx INT NOT NULL AUTO_INCREMENT, ClassifyCode INT NULL, AreaSize DOUBLE NULL,  Length INT NULL,  Width INT NULL, Height INT NULL, FOV INT NULL, PosX DOUBLE NULL, PosY DOUBLE NULL, memPOOL longtext DEFAULT NULL, memGROUP longtext DEFAULT NULL, memMEMORY longtext DEFAULT NULL, PRIMARY KEY (idx), UNIQUE INDEX idx_UNIQUE (idx ASC) VISIBLE);";
				connector.SendNonQuery(createQuery);
				Debug.WriteLine(string.Format("tempdata Table Drop : {0}", result));
				result = connector.SendNonQuery("INSERT INTO inspections.inspstatus (idx, inspStatusNum) VALUES ('0', '0') ON DUPLICATE KEY UPDATE idx='0', inspStatusNum='0';");
				Debug.WriteLine(string.Format("Status Clear : {0}", result));
			}
			connector.Close();
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

		public void _startInsp()
		{
			//TODO EdgeboxViewModel과 SideViewModel에 중복된 메소드가 존재하므로 통합이 가능한경우 정리하도록 합시다
			if (!m_Engineer.m_recipe.Loaded)
				return;

			//0. 개수 초기화 및 Table Drop
			_clearInspReslut();

			ClearDrawList();

			//2. 획득한 영역을 기준으로 검사영역을 생성하고 검사를 시작한다
			for (int k = 0; k < p_PatternRoiList.Count; k++)
			{
				var currentRoi = p_PatternRoiList[k];
				//ROI 개수만큼 회전하면서 검사영역을 생성한다
				for (int j = 0; j < currentRoi.Strip.ParameterList.Count; j++)
				{
					//검사영역 생성 기준
					//1. 등록된 feature를 탐색한다. 지정된 score에 부합하는 feature가 없을 경우 2차, 3차로 넘어갈 수도 있다. 
					//1.1. 만약 등록된 Feature가 없는 경우 기준 위치는 0,0으로한다
					CPoint standardPos = new CPoint(0, 0);
					int refStartXOffset = 0;
					int refStartYOffset = 0;

                    #region Feature
                    foreach (var feature in currentRoi.Position.ReferenceList)
					{
						bool foundFeature = false;
						CRect targetRect;
						Point maxRelativePoint;
						int widthDiff, heightDiff;
						foundFeature = FindFeature(feature, out targetRect, out maxRelativePoint, out widthDiff, out heightDiff);

						if (foundFeature)
						{
							//2. feature 중심위치가 확보되면 해당 좌표를 저장
							standardPos.X = targetRect.Left + (int)maxRelativePoint.X + widthDiff / 2;
							standardPos.Y = targetRect.Top + (int)maxRelativePoint.Y + heightDiff / 2;
							refStartXOffset = feature.PatternDistX;
							refStartYOffset = feature.PatternDistY;
							DrawCross(new DPoint(standardPos.X, standardPos.Y), MBrushes.Red);

							break;//찾았으니 중단
						}
						else
						{
							continue;//못 찾았으면 다음 Feature값으로 이동
						}
					}
                    #endregion

                    #region Align Key
                    //3. 등록된 Align Key 3개를 탐색한다. feature의 위치 정보도 참조하여 회전 보정 시에 들어갈 값을 준비해둔다
                    List<CPoint> alignKeyList = new List<CPoint>();

					if (currentRoi.Position.AlignList.Count == 3)
					{
						for (int n = 0; n < 3; n++)
						{
							//TODO : Reference와 중복되므로 나중에 별도 메소드로 만들어서 코드 중복을 최소화
							var align = currentRoi.Position.AlignList[n];
							var bmp = align.m_Feature.GetRectImage(new CRect(0, 0, align.m_Feature.p_Size.X, align.m_Feature.p_Size.Y));
							Emgu.CV.Image<Gray, byte> featureImage = new Emgu.CV.Image<Gray, byte>(bmp);
							var laplaceFeature = featureImage.Laplace(1);

							CRect targetRect = new CRect(
								new Point(align.RoiRect.Center().X - align.FeatureFindArea / 2.0, align.RoiRect.Center().Y - align.FeatureFindArea / 2.0),
								new Point(align.RoiRect.Center().X + align.FeatureFindArea / 2.0, align.RoiRect.Center().Y + align.FeatureFindArea / 2.0));
							Emgu.CV.Image<Gray, byte> sourceImage = new Emgu.CV.Image<Gray, byte>(p_ImageViewer.p_ImageData.GetRectImage(targetRect));
							var laplaceSource = sourceImage.Laplace(1);

							var resultImage = laplaceSource.MatchTemplate(laplaceFeature, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);

							int widthDiff = laplaceSource.Width - resultImage.Width;
							int heightDiff = laplaceSource.Height - resultImage.Height;

							float[,,] matches = resultImage.Data;

							Point maxRelativePoint = new Point();//상대위치

							bool foundFeature = false;
							float maxScore = float.MinValue;

							for (int x = 0; x < matches.GetLength(1); x++)
							{
								for (int y = 0; y < matches.GetLength(0); y++)
								{
									if (maxScore < matches[y, x, 0] && align.FeatureTargetScore <= matches[y, x, 0])
									{
										maxScore = matches[y, x, 0];
										maxRelativePoint.X = x;
										maxRelativePoint.Y = y;
										foundFeature = true;
									}
									//matches[y, x, 0] *= 256;
								}
							}
							if (foundFeature)
							{
								//2. feature 중심위치가 확보되면 해당 좌표를 저장
								CPoint tempPos = new CPoint();
								tempPos.X = targetRect.Left + (int)maxRelativePoint.X + widthDiff / 2;
								tempPos.Y = targetRect.Top + (int)maxRelativePoint.Y + heightDiff / 2;
								DrawCross(new DPoint(tempPos.X, tempPos.Y), MBrushes.Crimson);
								alignKeyList.Add(tempPos);
							}
						}
					}
					//TODO : 회전보정은 나중에하기
					if (alignKeyList.Count != 2)
					{
						//align 실패. 에러를 띄우거나 회전 좌표 보정을 하지 않음
					}
					else
					{
						//align 탐색 성공. 좌표 보정 계산 시작
					}
                    #endregion

                    //4. 저장된 좌표를 기준으로 PatternDistX, PatternDistY만큼 더한다. 이 좌표가 Start Position이 된다
                    var startPos = new Point(standardPos.X + refStartXOffset, standardPos.Y + refStartYOffset);
					//5. Start Position에 InspAreaWidth와 InspAreaHeight만큼 더해준다. 이 좌표가 End Position이 된다
					var endPos = new Point(startPos.X + (int)currentRoi.Strip.ParameterList[j].InspAreaWidth, startPos.Y + (int)currentRoi.Strip.ParameterList[j].InspAreaHeight);
					//6. Start Postiion과 End Position, Inspection Offset을 이용하여 검사 영역을 생성한다. 우선은 일괄 생성을 대상으로 한다
					var inspRect = new CRect(startPos, endPos);

					var temp = new UIElementInfo(new Point(inspRect.Left, inspRect.Top), new Point(inspRect.Right, inspRect.Bottom));

					System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
					rect.Width = inspRect.Width;
					rect.Height = inspRect.Height;
					System.Windows.Controls.Canvas.SetLeft(rect, inspRect.Left);
					System.Windows.Controls.Canvas.SetTop(rect, inspRect.Top);
					rect.StrokeThickness = 3;
					rect.Stroke = MBrushes.Orange;

					p_RefFeatureDrawer.m_ListShape.Add(rect);
					p_RefFeatureDrawer.m_Element.Add(rect);
					p_RefFeatureDrawer.m_ListRect.Add(temp);

					p_ImageViewer.SetRoiRect();

					int nDefectCode = InspectionManager.MakeDefectCode(InspectionTarget.Chrome, InspectionType.Strip, k);

					m_Engineer.m_InspManager.SetStandardPos(nDefectCode, standardPos);

					MemoryData memory = m_Engineer.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem);
					IntPtr p = memory.GetPtr(0);
					m_Engineer.m_InspManager.CreateInspArea(App.sPatternPool, App.sPatternGroup, App.sPatternmem, m_Engineer.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem).GetMBOffset(),
							m_Engineer.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem).p_sz.X,
							m_Engineer.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem).p_sz.Y,
							inspRect, 500, currentRoi.Strip.ParameterList[j], nDefectCode, m_Engineer.m_recipe.VegaRecipeData.UseDefectMerge, m_Engineer.m_recipe.VegaRecipeData.MergeDistance, p);
					//7. Strip검사를 시작한다
				}
			}
			m_Engineer.m_InspManager.StartInspection();//검사 시작!
		}

		public bool FindFeature(Feature feature, out CRect targetRect, out Point maxRelativePoint, out int widthDiff, out int heightDiff)
		{
			//TODO : Align과 중복되므로 나중에 별도 메소드로 만들어서 코드 중복을 최소화
			var bmp = feature.m_Feature.GetRectImage(new CRect(0, 0, feature.m_Feature.p_Size.X, feature.m_Feature.p_Size.Y));
			Emgu.CV.Image<Gray, byte> featureImage = new Emgu.CV.Image<Gray, byte>(bmp);
			var laplaceFeature = featureImage.Laplace(1);
			//laplaceFeature.Save(@"D:\Test\feature.bmp");

			targetRect = new CRect(
				new Point(feature.RoiRect.Left - (feature.FeatureFindArea + feature.RoiRect.Width) / 2.0, feature.RoiRect.Top - (feature.FeatureFindArea + feature.RoiRect.Height) / 2.0),
				new Point(feature.RoiRect.Right + (feature.FeatureFindArea + feature.RoiRect.Width) / 2.0, feature.RoiRect.Bottom + (feature.FeatureFindArea + feature.RoiRect.Height) / 2.0));
			Emgu.CV.Image<Gray, byte> sourceImage = new Emgu.CV.Image<Gray, byte>(p_ImageViewer.p_ImageData.GetRectImagePattern(targetRect));
			var laplaceSource = sourceImage.Laplace(1);
			//laplaceSource.Save(@"D:\Test\source.bmp");

			var resultImage = laplaceSource.MatchTemplate(laplaceFeature, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);

			widthDiff = laplaceSource.Width - resultImage.Width;
			heightDiff = laplaceSource.Height - resultImage.Height;

			float[,,] matches = resultImage.Data;

			maxRelativePoint = new Point();//상대위치

			bool foundFeature = false;
			float maxScore = float.MinValue;

			for (int x = 0; x < matches.GetLength(1); x++)
			{
				for (int y = 0; y < matches.GetLength(0); y++)
				{
					if (maxScore < matches[y, x, 0] && feature.FeatureTargetScore <= matches[y, x, 0])
					{
						maxScore = matches[y, x, 0];
						maxRelativePoint.X = x;
						maxRelativePoint.Y = y;
						foundFeature = true;
					}
					//matches[y, x, 0] *= 256;
				}
			}
			resultImage.Data = matches;
			//resultImage.Save(@"D:\Test\result.bmp");

			return foundFeature;
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

		void _SaveAlignFeature()
		{
			//그려진 첫번째 빨간색영역 내의 이미지를 Feature 정보로 저장한다
			//그린 정보는 SelectedFeature가 변경되면 Update되어야한다
			if (!alignEnabled)
				return;

			if (p_AlignFeatureDrawer.m_ListRect.Count >= 1 && p_AlignFeatureDrawer.m_ListRect.Count <= 3)
			{
				for (int i = 0; i< p_AlignFeatureDrawer.m_ListRect.Count; i++)
				{
					var featureArea = p_AlignFeatureDrawer.m_ListRect[i];
					var featureRect = new CRect(featureArea.StartPos, featureArea.EndPos);
					var featureImageArr = p_ImageViewer.p_ImageData.GetRectByteArray(featureRect);
					var targetName = string.Format("{0}_Align_{1}.bmp", SelectedROI.Name, i);
					//TODO 이상하게 구현함. 나중에 수정 필요
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

		void _FindAlignFeature()
		{
			Roi roi = SelectedROI;
			List<CPoint> alignKeyList = new List<CPoint>();
			for (int j = 0; j<roi.Position.AlignList.Count; j++)
			{
				var align = roi.Position.AlignList[j];
				var bmp = align.m_Feature.GetRectImage(new CRect(0, 0, align.m_Feature.p_Size.X, align.m_Feature.p_Size.Y));
				Emgu.CV.Image<Gray, byte> featureImage = new Emgu.CV.Image<Gray, byte>(bmp);
				var laplaceFeature = featureImage.Laplace(1);

				CPoint ptCenter = align.RoiRect.Center();
				Point ptStart = new Point(align.RoiRect.Center().X - align.FeatureFindArea / 2.0, align.RoiRect.Center().Y - align.FeatureFindArea / 2.0);
				Point ptEnd = new Point(align.RoiRect.Center().X + align.FeatureFindArea / 2.0, align.RoiRect.Center().Y + align.FeatureFindArea / 2.0);
				CRect targetRect = new CRect(ptStart, ptEnd);
				Emgu.CV.Image<Gray, byte> sourceImage = new Emgu.CV.Image<Gray, byte>(p_ImageViewer.p_ImageData.GetRectImage(targetRect));
				var laplaceSource = sourceImage.Laplace(1);

				var resultImage = laplaceSource.MatchTemplate(laplaceFeature, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);
				
				int widthDiff = laplaceSource.Width - resultImage.Width;
				int heightDiff = laplaceSource.Height - resultImage.Height;

				float[,,] matches = resultImage.Data;

				Point maxRelativePoint = new Point();//상대위치

				bool foundFeature = false;
				float maxScore = float.MinValue;

				for (int x = 0; x < matches.GetLength(1); x++)
				{
					for (int y = 0; y < matches.GetLength(0); y++)
					{
						if (maxScore < matches[y, x, 0] && align.FeatureTargetScore <= matches[y, x, 0])
						{
							maxScore = matches[y, x, 0];
							maxRelativePoint.X = x;
							maxRelativePoint.Y = y;
							foundFeature = true;
						}
						//matches[y, x, 0] *= 256;
					}
				}
				if (foundFeature)
				{
					//2. feature 중심위치가 확보되면 해당 좌표를 저장
					CPoint tempPos = new CPoint();
					tempPos.X = targetRect.Left + (int)maxRelativePoint.X + widthDiff / 2;
					tempPos.Y = targetRect.Top + (int)maxRelativePoint.Y + heightDiff / 2;
					DrawCross(new DPoint(tempPos.X, tempPos.Y), MBrushes.Crimson);
					alignKeyList.Add(tempPos);
				}
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
				return new RelayCommand(_startInsp);
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

		public ICommand SaveAlignFeatureCommand
		{
			get
			{
				return new RelayCommand(_SaveAlignFeature);
			}
		}

		public ICommand FindAlignFeatureCommand
		{
			get
			{
				return new RelayCommand(_FindAlignFeature);
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

		public RelayCommand CommandSaveAutoIlluminationFeature
		{
			get
			{
				return new RelayCommand(Test);
			}
		}

		private void SaveAutoIlluminationFeature()
		{
			string strFileName = "AutoIlluminationFeature.bmp";
			string strSaveTargetPath = $"C:/Recipe/AutoIllumination/";
			if (!Directory.Exists(strSaveTargetPath))
			{
				Directory.CreateDirectory(strSaveTargetPath);
			}
			string strFullPath = System.IO.Path.Combine(strSaveTargetPath, strFileName);

			if (p_ImageViewer.m_BasicTool.m_TempRect == null) return;
			UIElementInfo uieROI = p_ImageViewer.m_BasicTool.m_TempRect;
			int nLeft = (int)uieROI.StartPos.X;
			int nTop = (int)uieROI.StartPos.Y;
			int nRight = (int)uieROI.EndPos.X;
			int nBottom = (int)uieROI.EndPos.Y;

			p_ImageViewer.p_ImageData.SaveRectImage(new CRect(nLeft, nTop, nRight, nBottom), strFullPath);
			return;
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
