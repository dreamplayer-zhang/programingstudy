using RootTools;
using RootTools.Memory;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
//using RootTools_CLR;//CLR INSP 테스트
using System.Data;//DB 연동 테스트
using RootTools.Inspects;

namespace Root_Vega
{
	class _2_3_OriginViewModel : ObservableObject
	{
		Vega_Engineer m_Engineer;
		MemoryTool m_MemoryModule;
		public ImageData m_Image;
		DrawData m_DD;
		Recipe m_Recipe;

		string sPool = "pool";
		string sGroup = "group";
		string sMem = "mem";
		public int MemWidth = 60000;
		public int MemHeight = 360000;

		public _2_3_OriginViewModel(Vega_Engineer engineer, IDialogService dialogService)
		{
			m_Engineer = engineer;
			Init(engineer, dialogService);
			p_ImageViewer.m_AfterLoaded += RedrawUIElement;
		}

		void Init(Vega_Engineer engineer, IDialogService dialogService)
		{
			m_DD = new DrawData();
			m_Recipe = engineer.m_recipe;

			m_MemoryModule = engineer.ClassMemoryTool();
			//m_MemoryModule.CreatePool(sPool, 8);
			//m_MemoryModule.GetPool(sPool).CreateGroup(sGroup);
			m_MemoryModule.GetPool(sPool, true).p_gbPool = 50;
			m_MemoryModule.GetPool(sPool, true).GetGroup(sGroup).CreateMemory(sMem, 1, 1, new CPoint(MemWidth, MemHeight));
			m_MemoryModule.GetMemory(sPool, sGroup, sMem);

			m_Image = new ImageData(m_MemoryModule.GetMemory(sPool, sGroup, sMem));
			p_ImageViewer = new ImageViewer_ViewModel(m_Image, dialogService);

			p_OriginDrawerVM = new OriginDrawerVM(p_ImageViewer);
			p_OriginDrawerVM.RectangleKeyValue = Key.D1;
			p_OriginDrawerVM.SetStateDelegate += SetState;

			p_ImageViewer.SetDrawer((DrawToolVM)p_OriginDrawerVM);
		}

		#region Property
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

		private OriginDrawerVM m_OriginDrawerVM;
		public OriginDrawerVM p_OriginDrawerVM
		{
			get
			{
				return m_OriginDrawerVM;
			}
			set
			{
				SetProperty(ref m_OriginDrawerVM, value);
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


		private bool _origin_IsChecked = false;
		public bool Origin_IsChecked
		{
			get
			{
				return _origin_IsChecked;
			}
			set
			{
				SetProperty(ref _origin_IsChecked, value);
				p_OriginDrawerVM.p_ButtonCheckState = value;
				if (value == false)
				{

					if (p_ImageViewer.m_ModifyManager.p_SetState == true)
						p_ImageViewer.m_ModifyManager.DeleteModifyData();

					p_ImageViewer.m_HistoryWorker.Clear();

					p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.None;
					p_OriginDrawerVM.Clear();
					p_ImageViewer.SetImageSource();
				}
			}
		}

		string _test = "";
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
		string _test2 = "";
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
		#endregion

		#region Func
		//#사용유무 확인.
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
		//#사용유무 확인.

		private void MouseRightDown()
		{
			if (p_OriginDrawerVM.m_ListShape.Count > 0)
			{
				(p_OriginDrawerVM.m_ListShape[0]).StrokeDashArray = p_OriginDrawerVM.m_StrokeDashArray;
				p_ImageViewer.m_ModifyManager.SetModifyData(p_OriginDrawerVM, p_OriginDrawerVM.m_ListShape[0]);
				p_ImageViewer.m_ModifyManager.p_SetStateDone = true;
				p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.Modify;
			}

		}

		private void SetState(bool _State, int _Count)
		{
			if (_State && Origin_IsChecked)
				p_ImageViewer.p_MouseCursor = Cursors.Cross;
			else
				p_ImageViewer.p_MouseCursor = Cursors.Arrow;
		}

		void SaveRectImage()
		{
			m_Image.SaveRectImage(m_DD.m_OriginData.m_rt);
		}

		#endregion

		#region Command

		public ICommand btnSaveClick
		{
			get
			{
				return new RelayCommand(SaveRectImage);
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
		public ICommand CanvasMouseRightDown
		{
			get
			{
				return new RelayCommand(MouseRightDown);
			}
		}




		private void _btnClear()
		{
			if (Origin_IsChecked)
			{
				Origin_IsChecked = false;
			}
		}

		//insp 결과 display를 위해 임시 redrawUI 구현
		private void RedrawUIElement()
		{
			//RedrawRect();
			//RedrawStr();
			//RedrawPt();
			//RedrawLine();
		}



		//        CLR_Inspection clrDemo = new CLR_Inspection();
		private void _btnInspTest()
		{
			return;
		}

		#endregion

	}
}
