using ATI;
using RootTools;
using RootTools.Inspects;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Root_Vega
{
	class _2_5_SurfaceViewModel : ObservableObject
	{
		/// <summary>
		/// 외부 Thread에서 UI를 Update하기 위한 Dispatcher
		/// </summary>
		protected Dispatcher _dispatcher;
		Vega_Engineer m_Engineer;
		MemoryTool m_MemoryModule;
		ImageData m_Image;
		DrawData m_DD;
		Recipe m_Recipe;

		SqliteDataDB VSDBManager;
		int currentDefectIdx;
		System.Data.DataTable VSDataInfoDT;
		System.Data.DataTable VSDataDT;

		private string inspDefaultDir;
		private string inspFileName;

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
		string sPool = "pool";
		string sGroup = "group";
		string sMem = "mem";

		int tempImageWidth = 640;
		int tempImageHeight = 480;

		public _2_5_SurfaceViewModel(Vega_Engineer engineer, IDialogService dialogService)
		{
			_dispatcher = Dispatcher.CurrentDispatcher;
			m_Engineer = engineer;
			Init(engineer, dialogService);

			m_Engineer.m_InspManager.AddDefect += M_InspManager_AddDefect;
			m_Engineer.m_InspManager.InspectionComplete += () =>
			{
				//VSDBManager.Commit();

				//여기서부터 DB Table데이터를 기준으로 tif 이미지 파일을 생성하는 구간
				//해당 기능은 여러개의 pool을 사용하는 경우에 대해서는 테스트가 진행되지 않았습니다
				//Concept은 검사 결과가 저장될 시점에 가지고 있던 Data Table을 저장하기 전 Image를 저장하는 형태
				int stride = tempImageWidth / 8;
				string target_path = System.IO.Path.Combine(inspDefaultDir, System.IO.Path.GetFileNameWithoutExtension(inspFileName) + ".tif");

				System.Windows.Media.Imaging.BitmapPalette myPalette = System.Windows.Media.Imaging.BitmapPalettes.WebPalette;

				System.IO.FileStream stream = new System.IO.FileStream(target_path, System.IO.FileMode.Create);
				System.Windows.Media.Imaging.TiffBitmapEncoder encoder = new System.Windows.Media.Imaging.TiffBitmapEncoder();
				encoder.Compression = System.Windows.Media.Imaging.TiffCompressOption.Zip;

				foreach (System.Data.DataRow row in VSDataDT.Rows)
				{
					//Data,@No(INTEGER),DCode(INTEGER),Size(INTEGER),Length(INTEGER),Width(INTEGER),Height(INTEGER),InspMode(INTEGER),FOV(INTEGER),PosX(INTEGER),PosY(INTEGER)
					double fPosX = Convert.ToDouble(row["PosX"]);
					double fPosY = Convert.ToDouble(row["PosY"]);

					CRect ImageSizeBlock = new CRect(
						(int)fPosX - tempImageWidth / 2,
						(int)fPosY - tempImageHeight / 2,
						(int)fPosX + tempImageWidth / 2,
						(int)fPosY + tempImageHeight / 2);

					encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(BitmapToBitmapSource(m_ImageViewer.p_ImageData.GetRectImage(ImageSizeBlock))));
				}

				encoder.Save(stream);
				stream.Dispose();
				//이미지 저장 완료

				//Data Table 저장 시작
				VSDBManager.SetDataTable(VSDataInfoDT);
				VSDBManager.SetDataTable(VSDataDT);
				VSDBManager.Disconnect();
				//Data Table 저장 완료
			};
			m_Engineer.m_InspManager.InspectionStart += () =>
			{
				//VSDBManager.BeginWrite();
			};
		}
		public System.Windows.Media.Imaging.BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
		{
			var bitmapData = bitmap.LockBits(
				new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
				System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

			var bitmapSource = System.Windows.Media.Imaging.BitmapSource.Create(
				bitmapData.Width, bitmapData.Height,
				bitmap.HorizontalResolution, bitmap.VerticalResolution,
				PixelFormats.Gray8, null,
				bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

			bitmap.UnlockBits(bitmapData);
			return bitmapSource;
		}
		/// <summary>
		/// UI에 추가된 Defect을 빨간색 상자로 표시할 수 있도록 추가하는 메소드
		/// </summary>
		/// <param name="source">UI에 추가할 Defect List</param>
		/// <param name="args">arguments. 사용이 필요한 경우 수정해서 사용</param>
		private void M_InspManager_AddDefect(DefectData[] source, EventArgs args)
		{
			//string tempInspDir = @"C:\vsdb\TEMP_IMAGE";

			foreach (var item in source)
			{
				CPoint ptStart = new CPoint(Convert.ToInt32(item.fPosX - item.nWidth / 2.0), Convert.ToInt32(item.fPosY - item.nHeight / 2.0));
				CPoint ptEnd = new CPoint(Convert.ToInt32(item.fPosX + item.nWidth / 2.0), Convert.ToInt32(item.fPosY + item.nHeight / 2.0));

				CRect resultBlock = new CRect(ptStart.X, ptStart.Y, ptEnd.X, ptEnd.Y);

				//CRect ImageSizeBlock = new CRect(
				//	(int)item.fPosX - tempImageWidth / 2,
				//	(int)item.fPosY - tempImageHeight / 2,
				//	(int)item.fPosX + tempImageWidth / 2,
				//	(int)item.fPosY + tempImageHeight / 2);

				//string filename = currentDefectIdx.ToString("D8") + ".bmp";
				//m_ImageViewer.p_ImageData.SaveRectImage(ImageSizeBlock, System.IO.Path.Combine(tempInspDir, filename));

				m_DD.AddRectData(resultBlock, System.Drawing.Color.Red);

				//여기서 DB에 Defect을 추가하는 부분도 구현한다
				System.Data.DataRow dataRow = VSDataDT.NewRow();

				//Data,@No(INTEGER),DCode(INTEGER),Size(INTEGER),Length(INTEGER),Width(INTEGER),Height(INTEGER),InspMode(INTEGER),FOV(INTEGER),PosX(INTEGER),PosY(INTEGER)

				dataRow["No"] = currentDefectIdx;
				currentDefectIdx++;
				dataRow["DCode"] = item.nClassifyCode;
				dataRow["Size"] = item.fSize;
				dataRow["Length"] = item.nLength;
				dataRow["Width"] = item.nWidth;
				dataRow["Height"] = item.nHeight;
				dataRow["InspMode"] = item.nInspMode;
				//dataRow["FOV"] = item.FOV;
				dataRow["PosX"] = item.fPosX;
				dataRow["PosY"] = item.fPosY;

				VSDataDT.Rows.Add(dataRow);
			}
			_dispatcher.Invoke(new Action(delegate ()
			{
				RedrawUIElement();
			}));
		}

		void Init(Vega_Engineer engineer, IDialogService dialogService)
		{
			m_DD = new DrawData();
			p_Recipe = engineer.m_recipe;

			m_MemoryModule = engineer.ClassMemoryTool();
			//m_MemoryModule.CreatePool(sPool, 8);
			//m_MemoryModule.GetPool(sPool).CreateGroup(sGroup);

			//m_MemoryModule.GetPool(sPool).p_gbPool = 2;
			m_Image = new ImageData(m_MemoryModule.GetMemory(sPool, sGroup, sMem));
			p_ImageViewer = new ImageViewer_ViewModel(m_Image, dialogService);
            m_DrawHistoryWorker_List.Add(new DrawHistoryWorker());
            m_DrawHistoryWorker_List.Add(new DrawHistoryWorker());

            p_SimpleShapeDrawer.Add(new SimpleShapeDrawerVM(p_ImageViewer));
            p_SimpleShapeDrawer.Add(new SimpleShapeDrawerVM(p_ImageViewer));
            p_SimpleShapeDrawer[0].RectangleKeyValue = Key.D1;
            p_SimpleShapeDrawer[1].RectangleKeyValue = Key.D1;
            p_ImageViewer.SetDrawer((DrawToolVM)p_SimpleShapeDrawer[0]);
            p_ImageViewer.m_HistoryWorker = m_DrawHistoryWorker_List[0];

			//p_ListRoi = m_Recipe.m_RD.p_Roi;

			//m_Recipe.m_RD.p_Roi = new List<Roi>(); //Mask#1, Mask#2... New List Mask
			Roi Mask, Mask2;
			Mask = new Roi("MASK1", Roi.Item.Test);  // Mask Number.. New Mask
			Mask.m_Surface.p_Parameter = new ObservableCollection<SurFace_ParamData>();
			Mask.m_Surface.m_NonPattern = new List<NonPattern>(); // List Rect in Mask
			NonPattern rect = new NonPattern(); // New Rect
			rect.m_rt = new CRect(); // Rect Info
			SurFace_ParamData param = new SurFace_ParamData();
			Mask.m_Surface.p_Parameter.Add(param);
			Mask.m_Surface.m_NonPattern.Add(rect); // Add Rect to Rect List
												   //m_Recipe.m_RD.p_Roi.Add(Mask);
												   //p_ListRoi.Add(m_Mask);

			Mask2 = new Roi("MASK2", Roi.Item.Test);  // Mask Number.. New Mask
			Mask2.m_Surface.p_Parameter = new ObservableCollection<SurFace_ParamData>();
			Mask2.m_Surface.m_NonPattern = new List<NonPattern>(); // List Rect in Mask
			NonPattern rect2 = new NonPattern(); // New Rect
			rect2.m_rt = new CRect(); // Rect Info
			SurFace_ParamData param2 = new SurFace_ParamData();
			Mask2.m_Surface.p_Parameter.Add(param2);
			Mask2.m_Surface.m_NonPattern.Add(rect2); // Add Rect to Rect List

			p_Recipe.p_RecipeData.p_Roi.Add(Mask);
			p_Recipe.p_RecipeData.p_Roi.Add(Mask2);
		}

		#region Property
		string _test;
		public string Test
		{
			get
			{

				return _test;
			}
			set
			{
				SetProperty(ref _test, value);
			}

		}
		string _test2;
		public string Test2
		{
			get
			{
				return _test2;
			}
			set
			{
				SetProperty(ref _test2, value);
			}
		}
       

		//private ObservableCollection<Roi> _ListRoi = new ObservableCollection<Roi>();
		//public ObservableCollection<Roi> p_ListRoi
		//{
		//    get
		//    {
		//        return _ListRoi;
		//    }
		//    set
		//    {
		//        SetProperty(ref _ListRoi, value);
		//        //m_Recipe.m_RD.p_Roi = p_ListRoi.ToList<Roi>();
		//    }
		//}

		public SurFace_ParamData p_SurFace_ParamData
		{
			get
			{
				if (m_Recipe.p_RecipeData.p_Roi[p_IndexMask].m_Surface.p_Parameter.Count != 0)
					return m_Recipe.p_RecipeData.p_Roi[p_IndexMask].m_Surface.p_Parameter[0];
				else
					return new SurFace_ParamData();
			}
			set
			{
				if (m_Recipe.p_RecipeData.p_Roi[p_IndexMask].m_Surface.p_Parameter.Count != 0)
					m_Recipe.p_RecipeData.p_Roi[p_IndexMask].m_Surface.p_Parameter[0] = value;
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
				//수정필요p_ImageViewer.SetRectElement_MemPos(p_Recipe.p_RecipeData.p_Roi[_IndexMask].m_Surface.m_NonPattern[0].m_rt);
                p_ImageViewer.SetDrawer((DrawToolVM)p_SimpleShapeDrawer[_IndexMask]);
                p_ImageViewer.m_HistoryWorker = m_DrawHistoryWorker_List[_IndexMask];
                p_ImageViewer.SetImageSource();
				p_SurFace_ParamData = p_Recipe.p_RecipeData.p_Roi[_IndexMask].m_Surface.p_Parameter[0];

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

		private ObservableCollection<UIElement> _UIelement = new ObservableCollection<UIElement>();
		public ObservableCollection<UIElement> p_UIElement
		{
			get
			{
				return _UIelement;
			}
			set
			{
				SetProperty(ref _UIelement, value);
			}
		}

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
			CRect rect = p_ImageViewer.GetCurrentRect_MemPos();
            p_Recipe.p_RecipeData.p_Roi[p_IndexMask].m_Surface.m_NonPattern[0].m_rt = rect;

		}


		#region Command
		
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
		public RelayCommand CommandSaveMask
		{
			get
			{
				return new RelayCommand(SaveCurrentMask);
			}
		}



		private void ClearUI()
		{
			if (p_UIElement != null)
				p_UIElement.Clear();
		}
		private void _btnClear()
		{
			p_Recipe.p_RecipeData.p_Roi[p_IndexMask].m_Surface.m_NonPattern[0].m_rt = new CRect();
			p_IndexMask = _IndexMask;
		}
		private void _btnDraw()
		{
			if (!Draw_IsChecked)
			{
				m_SurfaceProgress = SurfaceProgress.None;
			}
			else
			{
				m_SurfaceProgress = SurfaceProgress.Start;
				RecipeCursor = Cursors.Cross;
			}
		}
		private void _btnDone()
		{
			Draw_IsChecked = false;
			m_SurfaceProgress = SurfaceProgress.Done;
			RecipeCursor = Cursors.Arrow;
		}
		//insp 결과 display를 위해 임시 redrawUI 구현
		private void RedrawUIElement()
		{
			RedrawRect();
			RedrawStr();
		}
		private void RedrawRect()
		{
			if (m_DD.m_RectData.Count > 0)
			{
				p_UIElement.Clear();
				for (int i = 0; i < m_DD.m_RectData.Count; i++)
				{
					System.Windows.Shapes.Rectangle RedrawnRect = new System.Windows.Shapes.Rectangle();
					CPoint LeftTopPt = GetCanvasPoint(m_DD.m_RectData[i].m_rt.Left, m_DD.m_RectData[i].m_rt.Top);
					CPoint RighBottomPt = GetCanvasPoint(m_DD.m_RectData[i].m_rt.Right, m_DD.m_RectData[i].m_rt.Bottom);
					RedrawnRect.Stroke = new SolidColorBrush(ConvertColor(m_DD.m_RectData[i].m_color));
					RedrawnRect.StrokeThickness = 2;
					Canvas.SetLeft(RedrawnRect, LeftTopPt.X);
					Canvas.SetTop(RedrawnRect, LeftTopPt.Y);

					RedrawnRect.Width = Math.Abs(LeftTopPt.X - RighBottomPt.X);
					RedrawnRect.Height = Math.Abs(LeftTopPt.Y - RighBottomPt.Y);
					p_UIElement.Add(RedrawnRect);
				}
			}
		}
		private void RedrawStr()
		{
			if (m_DD.m_StringData.Count > 0)
			{
				for (int i = 0; i < m_DD.m_StringData.Count; i++)
				{
					TextBlock RedrawnTB = new TextBlock();
					CPoint TbPt = GetCanvasPoint(m_DD.m_StringData[i].m_pt.X, m_DD.m_StringData[i].m_pt.Y);
					RedrawnTB.Text = m_DD.m_StringData[i].m_str;
					RedrawnTB.Foreground = new SolidColorBrush(ConvertColor(m_DD.m_StringData[i].m_color));
					Canvas.SetLeft(RedrawnTB, TbPt.X);
					Canvas.SetTop(RedrawnTB, TbPt.Y);

					p_UIElement.Add(RedrawnTB);
				}

			}
		}
		List<CRect> DrawRectList;
		private void _btnInspTest()
		{
			ClearUI();//재검사 전 UI 정리

			if (m_DD != null)
				m_DD.Clear();//Draw Data정리

			if (DrawRectList != null)
				DrawRectList.Clear();//검사영역 draw용 Rect List 정리

			currentDefectIdx = 0;

			CRect Mask_Rect = p_Recipe.p_RecipeData.p_Roi[0].m_Surface.m_NonPattern[0].m_rt;
			int nblocksize = 500;


			DrawRectList = m_Engineer.m_InspManager.CreateInspArea(Mask_Rect, nblocksize, p_Recipe.p_RecipeData.p_Roi[0].m_Surface.p_Parameter[0]);

			for (int i = 0; i < DrawRectList.Count; i++)
			{
				CRect inspblock = DrawRectList[i];
				m_DD.AddRectData(inspblock, System.Drawing.Color.Orange);

			}
			System.Diagnostics.Debug.WriteLine("Start Insp");

			inspDefaultDir = @"C:\vsdb";
			if (!System.IO.Directory.Exists(inspDefaultDir))
			{
				System.IO.Directory.CreateDirectory(inspDefaultDir);
			}
			inspFileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_inspResult.vega_result";
			var targetVsPath = System.IO.Path.Combine(inspDefaultDir, inspFileName);
			string VSDB_configpath = @"C:/vsdb/init/vsdb.txt";

			if (VSDBManager == null)
			{
				VSDBManager = new SqliteDataDB(targetVsPath, VSDB_configpath);
			}
			else if (VSDBManager.IsConnected)
			{
				VSDBManager.Disconnect();
				VSDBManager = new SqliteDataDB(targetVsPath, VSDB_configpath);
			}
			if (VSDBManager.Connect())
			{
				VSDBManager.CreateTable("Datainfo");
				VSDBManager.CreateTable("Data");

				VSDataInfoDT = VSDBManager.GetDataTable("Datainfo");
				VSDataDT = VSDBManager.GetDataTable("Data");
			}

			m_Engineer.m_InspManager.StartInspection();

			return;
		}
		#endregion


		SurfaceProgress m_SurfaceProgress = SurfaceProgress.None;
		HitType m_MouseHitType = HitType.None;

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
		public class DrawHelper
		{
			public List<Rectangle> CanvasRectList = new List<System.Windows.Shapes.Rectangle>();
			public Rectangle NowRect;
			public CPoint Rect_StartPt;
			public CPoint Rect_EndPt;
			public CPoint PreMousePt;
			public CRect preRect = new CRect();
		}
	}
}
