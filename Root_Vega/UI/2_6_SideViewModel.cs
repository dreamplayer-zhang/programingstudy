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
namespace Root_Vega
{
	class _2_6_SideViewModel : ObservableObject
	{
		/// <summary>
		/// 외부 Thread에서 UI를 Update하기 위한 Dispatcher
		/// </summary>
		protected Dispatcher _dispatcher;
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

		public _2_6_SideViewModel(Vega_Engineer engineer, IDialogService dialogService)
		{
			_dispatcher = Dispatcher.CurrentDispatcher;
			m_Engineer = engineer;
			Init(dialogService);

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
		public ICommand btnInspTest
		{
			get
			{
				return new RelayCommand(_btnInspTest);
			}
		}
		private void _btnInspTest()
		{
			//currentDefectIdx = 0;

			//CRect Mask_Rect = p_Recipe.RecipeData.RoiList[0].Surface.NonPatternList[0].Area;
			//int nblocksize = 500;


			////DrawRectList = m_Engineer.m_InspManager.CreateInspArea(Mask_Rect, nblocksize,
			////	p_Recipe.p_RecipeData.p_Roi[0].m_Surface.p_Parameter[0],
			////	p_Recipe.p_RecipeData.p_bDefectMerge, p_Recipe.p_RecipeData.p_nMergeDistance);

			//for (int i = 0; i < DrawRectList.Count; i++)
			//{
			//	CRect inspblock = DrawRectList[i];
			//	m_DD.AddRectData(inspblock, System.Drawing.Color.Orange);

			//}
			//System.Diagnostics.Debug.WriteLine("Start Insp");

			//inspDefaultDir = @"C:\vsdb";
			//if (!System.IO.Directory.Exists(inspDefaultDir))
			//{
			//	System.IO.Directory.CreateDirectory(inspDefaultDir);
			//}
			//inspFileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_inspResult.vega_result";
			//var targetVsPath = System.IO.Path.Combine(inspDefaultDir, inspFileName);
			//string VSDB_configpath = @"C:/vsdb/init/vsdb.txt";

			//if (VSDBManager == null)
			//{
			//	VSDBManager = new SqliteDataDB(targetVsPath, VSDB_configpath);
			//}
			//else if (VSDBManager.IsConnected)
			//{
			//	VSDBManager.Disconnect();
			//	VSDBManager = new SqliteDataDB(targetVsPath, VSDB_configpath);
			//}
			//if (VSDBManager.Connect())
			//{
			//	VSDBManager.CreateTable("Datainfo");
			//	VSDBManager.CreateTable("Data");

			//	VSDataInfoDT = VSDBManager.GetDataTable("Datainfo");
			//	VSDataDT = VSDBManager.GetDataTable("Data");
			//}

			////m_Engineer.m_InspManager.StartInspection(InspectionType.Surface, m_Image.p_Size.X, m_Image.p_Size.Y);//사용 예시

			//return;
		}
		#endregion
	}
}
