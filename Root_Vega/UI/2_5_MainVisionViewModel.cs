using ATI;
using Microsoft.Win32;
using RootTools;
using RootTools.Inspects;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Root_Vega
{
	class _2_5_MainVisionViewModel : ObservableObject
	{
		/// <summary>
		/// 외부 Thread에서 UI를 Update하기 위한 Dispatcher
		/// </summary>
		protected Dispatcher _dispatcher;
		Vega_Engineer m_Engineer;
		MemoryTool m_MemoryModule;
		ImageData m_Image;
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

		int currentSnap;
		int wLimit;

		public _2_5_MainVisionViewModel(Vega_Engineer engineer, IDialogService dialogService)
		{
			_dispatcher = Dispatcher.CurrentDispatcher;
			m_Engineer = engineer;
			Init(engineer, dialogService);

			m_Engineer.m_InspManager.AddDefect += M_InspManager_AddDefect;
			m_Engineer.m_InspManager.InspectionComplete += (nDCode) =>
			{
				if (InspectionManager.GetInspectionType(nDCode) != InspectionType.Strip)
				{
					return;
				}
				if (wLimit <= currentSnap)
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
					if (VSDataDT.Rows.Count > 0)
					{
						encoder.Save(stream);
					}
					stream.Dispose();
					//이미지 저장 완료

					//Data Table 저장 시작
					VSDBManager.SetDataTable(VSDataInfoDT);
					VSDBManager.SetDataTable(VSDataDT);
					VSDBManager.Disconnect();
					//Data Table 저장 완료
					m_Engineer.m_InspManager.Dispose();
					VSDataDT.Clear();
				}
			};
			m_Engineer.m_InspManager.InspectionStart += (nDCode) =>
			{
				if (InspectionManager.GetInspectionType(nDCode) != InspectionType.Strip)
				{
					return;
				}
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
		private void M_InspManager_AddDefect(DefectDataWrapper[] source, int nDCode)
		{
			if (InspectionManager.GetInspectionType(nDCode) != InspectionType.Strip)
			{
				return;
			}
			//string tempInspDir = @"C:\vsdb\TEMP_IMAGE";

			foreach (var item in source)
			{
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
		}

		void Init(Vega_Engineer engineer, IDialogService dialogService)
		{
			p_Recipe = engineer.m_recipe;

			m_MemoryModule = engineer.ClassMemoryTool();
			//m_MemoryModule.CreatePool(sPool, 8);
			//m_MemoryModule.GetPool(sPool).CreateGroup(sGroup);

			//m_MemoryModule.GetPool(sPool).p_gbPool = 2;
			m_Image = new ImageData(m_MemoryModule.GetMemory(sPool, sGroup, sMem));
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

			//p_ListRoi = m_Recipe.m_RD.p_Roi;

			//m_Recipe.m_RD.p_Roi = new List<Roi>(); //Mask#1, Mask#2... New List Mask
			Roi Mask, Mask2;
			Mask = new Roi("Strip MASK1", Roi.Item.ReticlePattern);  // Mask Number.. New Mask
			Mask.Strip.ParameterList = new ObservableCollection<StripParamData>();
			Mask.Strip.NonPatternList = new List<NonPattern>(); // List Rect in Mask
			NonPattern rect = new NonPattern(); // New Rect
			rect.Area = new CRect(); // Rect Info
			StripParamData param = new StripParamData();
			Mask.Strip.ParameterList.Add(param);
			Mask.Strip.NonPatternList.Add(rect); // Add Rect to Rect List
												 //m_Recipe.m_RD.p_Roi.Add(Mask);
												 //p_ListRoi.Add(m_Mask);

			Mask2 = new Roi("Strip MASK2", Roi.Item.ReticlePattern);  // Mask Number.. New Mask
			Mask2.Strip.ParameterList = new ObservableCollection<StripParamData>();
			Mask2.Strip.NonPatternList = new List<NonPattern>(); // List Rect in Mask
			NonPattern rect2 = new NonPattern(); // New Rect
			rect2.Area = new CRect(); // Rect Info
			StripParamData param2 = new StripParamData();
			Mask2.Strip.ParameterList.Add(param2);
			Mask2.Strip.NonPatternList.Add(rect2); // Add Rect to Rect List

			p_Recipe.RecipeData.RoiList.Add(Mask);
			p_Recipe.RecipeData.RoiList.Add(Mask2);
		}

		#region Property
		public RecipeData p_RecipeData
		{
			get
			{
				if (m_Recipe.RecipeData != null)
				{
					return m_Recipe.RecipeData;
				}
				else
				{
					return new RecipeData();
				}
			}
			set
			{
				if (m_Recipe.RecipeData != null)
				{
					m_Recipe.RecipeData = value;
					RaisePropertyChanged();
				}
			}
		}
		public StripParamData p_StripParamData
		{
			get
			{
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
			var temp = p_SimpleShapeDrawer[0].m_ListRect[0];
			int left = (int)temp.StartPos.X;
			int top = (int)temp.StartPos.Y;
			int right = (int)temp.EndPos.X;
			int bot = (int)temp.EndPos.Y;
			CRect rect = new CRect(left, top, right, bot);
			p_Recipe.RecipeData.RoiList[p_IndexMask].Strip.NonPatternList[0].Area = rect;

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
		//public ICommand btnStartInsp
		//{
		//	get
		//	{
		//		return new RelayCommand(_btnStartInsp);
		//	}
		//}
		//public ICommand btnNextSnap
		//{
		//	get
		//	{
		//		return new RelayCommand(_btnNextSnap);
		//	}
		//}
		public ICommand btnRcpSaveTest
		{
			get
			{
				return new RelayCommand(_btnRcpSaveTest);
			}
		}
		public ICommand btnRcpLoadTest
		{
			get
			{
				return new RelayCommand(_btnRcpLoadTest);
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
		private void _btnRcpLoadTest()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Vega Vision Recipe (*.VegaVision)|*.VegaVision";
			dlg.InitialDirectory = @"C:\VEGA\Recipe";
			if (dlg.ShowDialog() == true)
			{
				m_Engineer.m_recipe.Load(dlg.FileName);
				p_Recipe = m_Engineer.m_recipe;
			}

		}
		private void _btnRcpSaveTest()
		{
			this.p_Recipe.MapData = new MapData(50, 50);

			foreach (var item in p_Recipe.RecipeData.RoiList)
			{
				item.Position = new Position();
				item.Position.FeatureList = new List<Feature>();

				Feature data = new Feature();
				System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(@"D:\test.bmp");
				data.m_Feature = new ImageData(bmp.Width, bmp.Height);
				data.RoiRect.Top = 0;
				data.RoiRect.Left = 0;
				data.RoiRect.Bottom = bmp.Height;
				data.RoiRect.Right = bmp.Width;
				bmp.Dispose();
				data.m_Feature.LoadImageSync(@"D:\test.bmp", new CPoint(0, 0));
				item.Position.FeatureList.Add(data);

				Feature data2 = new Feature();
				System.Drawing.Bitmap bmp2 = new System.Drawing.Bitmap(@"D:\test2.bmp");
				data2.m_Feature = new ImageData(bmp2.Width, bmp2.Height);
				data2.RoiRect.Top = 0;
				data2.RoiRect.Left = 0;
				data2.RoiRect.Bottom = bmp2.Height;
				data2.RoiRect.Right = bmp2.Width;
				bmp.Dispose();
				data2.m_Feature.LoadImageSync(@"D:\test2.bmp", new CPoint(0, 0));
				item.Position.FeatureList.Add(data2);
			}

			System.Threading.Tasks.Parallel.For(0, 50, y =>
			{
				System.Threading.Tasks.Parallel.For(0, 50, x =>
				{
					var temp = new Unit();
					temp.X = x;
					temp.Y = y;

					Random rand = new Random();
					Thread.Sleep(1);
					temp.Exist = Convert.ToBoolean(rand.Next(0, 2));
					Thread.Sleep(1);
					temp.Selected = Convert.ToBoolean(rand.Next(0, 2));
					Thread.Sleep(1);
					temp.Progress = (Unit.UnitProgress)rand.Next(0, 4);
					Thread.Sleep(1);
					temp.Result = (Unit.UnitResult)rand.Next(0, 2);
					Thread.Sleep(1);
					this.p_Recipe.MapData.Map[y, x] = temp;
				});
			});
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.Filter = "Vega Vision Recipe (*.VegaVision)|*.VegaVision";
			dlg.InitialDirectory = @"C:\VEGA\Recipe";
			if (dlg.ShowDialog() == true)
			{
				var target = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(dlg.FileName), System.IO.Path.GetFileNameWithoutExtension(dlg.FileName));
				this.p_Recipe.Save(target);
			}
			//this.p_Recipe.Save();
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
			currentSnap = 0;
			wLimit = 0;
			System.Diagnostics.Debug.WriteLine(string.Format("Set wLimit : {0}", wLimit));

			if (DrawRectList != null)
				DrawRectList.Clear();//검사영역 draw용 Rect List 정리

			m_Engineer.m_InspManager.ClearInspection();

			currentDefectIdx = 0;

			CRect Mask_Rect = p_Recipe.RecipeData.RoiList[0].Strip.NonPatternList[0].Area;
			int nblocksize = 500;

			DrawRectList = m_Engineer.m_InspManager.CreateInspArea(Mask_Rect, nblocksize,
				p_Recipe.RecipeData.RoiList[0].Strip.ParameterList[0],
				InspectionType.Strip,
				p_Recipe.RecipeData.UseDefectMerge, p_Recipe.RecipeData.MergeDistance);

			//for (int i = 0; i < DrawRectList.Count; i++)
			//{
			//	CRect inspblock = DrawRectList[i];
			//	m_DD.AddRectData(inspblock, System.Drawing.Color.Orange);

			//}
			System.Diagnostics.Debug.WriteLine("Start Insp");

			inspDefaultDir = @"C:\vsdb";
			if (!System.IO.Directory.Exists(inspDefaultDir))
			{
				System.IO.Directory.CreateDirectory(inspDefaultDir);
			}
			inspFileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_inspResult.vega_result";
			var targetVsPath = System.IO.Path.Combine(inspDefaultDir, inspFileName);
			string VSDB_configpath = @"C:/vsdb/init/vsdb.txt";

			if (VSDBManager != null && VSDBManager.IsConnected)
			{
				VSDBManager.Disconnect();
			}
			VSDBManager = new SqliteDataDB(targetVsPath, VSDB_configpath);

			if (VSDBManager.Connect())
			{
				VSDBManager.CreateTable("Datainfo");
				VSDBManager.CreateTable("Data");

				VSDataInfoDT = VSDBManager.GetDataTable("Datainfo");
				VSDataDT = VSDBManager.GetDataTable("Data");
			}
			int nDefectCode = InspectionManager.MakeDefectCode(InspectionTarget.Chrome, InspectionType.Strip, 0);
			m_Engineer.m_InspManager.StartInspection(nDefectCode, m_Image.p_Size.X, m_Image.p_Size.Y);

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
