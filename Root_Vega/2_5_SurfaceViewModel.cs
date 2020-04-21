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
		protected Dispatcher _dispatcher;
		Vega_Engineer m_Engineer;
		MemoryTool m_MemoryModule;
		ImageData m_Image;
		DrawHelper m_DrawHelper;
		DrawData m_DD;
		Recipe m_Recipe;
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

		public _2_5_SurfaceViewModel(Vega_Engineer engineer, IDialogService dialogService)
		{
			_dispatcher = Dispatcher.CurrentDispatcher;
			m_Engineer = engineer;
			Init(engineer, dialogService);
			p_ImageViewer.m_AfterLoaded += ReDrawRect;

			m_Engineer.m_InspManager.AddDefectToUI += M_InspManager_AddDefectToUI;
		}

		private void M_InspManager_AddDefectToUI(DefectData[] source, EventArgs args)
		{
			foreach (var item in source)
			{
				CPoint ptStart = new CPoint(Convert.ToInt32(item.fPosX - item.nWidth / 2.0), Convert.ToInt32(item.fPosY - item.nHeight / 2.0));
				CPoint ptEnd = new CPoint(Convert.ToInt32(item.fPosX + item.nWidth / 2.0), Convert.ToInt32(item.fPosY + item.nHeight / 2.0));
#if DEBUG
				//System.Diagnostics.Debug.WriteLine(string.Format("{0}/{1} {2}/{3}", ptStart.X, ptStart.Y, ptEnd.X, ptEnd.Y));
#endif
				CRect resultBlock = new CRect(ptStart.X, ptStart.Y, ptEnd.X, ptEnd.Y);
				m_DD.AddRectData(resultBlock, System.Drawing.Color.Red);
			}
			_dispatcher.Invoke(new Action(delegate() 
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
				p_ImageViewer.SetRectElement_MemPos(p_Recipe.p_RecipeData.p_Roi[_IndexMask].m_Surface.m_NonPattern[0].m_rt);
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
		private void ReDrawRect()
		{
			//if(m_Recipe.m_RD.p_Roi[p_IndexMask] != null)
			//{
			//    p_UIElement.Clear();
			//    for (int i = 0; i < m_Recipe.m_RD.p_Roi[p_IndexMask].m_Surface.m_NonPattern.Count; i++)
			//    {
			//        CRect rect = m_Recipe.m_RD.p_Roi[p_IndexMask].m_Surface.m_NonPattern[i].m_rt;
			//    }

			//}
			//if (m_DD.m_OriginData != null)
			//{
			//    p_UIElement.Clear();

			//    m_DrawHelper.NowRect = new System.Windows.Shapes.Rectangle();
			//    CPoint LeftTopPt = GetCanvasPoint(m_DD.m_OriginData.m_rt.Left, m_DD.m_OriginData.m_rt.Top);
			//    CPoint RighBottomPt = GetCanvasPoint(m_DD.m_OriginData.m_rt.Right, m_DD.m_OriginData.m_rt.Bottom);
			//    m_DrawHelper.NowRect.Stroke = new SolidColorBrush(ConvertColor(m_DD.m_OriginData.m_color));
			//    m_DrawHelper.NowRect.StrokeThickness = 2;

			//    Canvas.SetLeft(m_DrawHelper.NowRect, LeftTopPt.X);
			//    Canvas.SetTop(m_DrawHelper.NowRect, LeftTopPt.Y);

			//    m_DrawHelper.NowRect.Width = Math.Abs(LeftTopPt.X - RighBottomPt.X);
			//    m_DrawHelper.NowRect.Height = Math.Abs(LeftTopPt.Y - RighBottomPt.Y);
			//    p_UIElement.Add(m_DrawHelper.NowRect);
			//}
		}
		#endregion

		public void SaveCurrentMask()
		{
			CRect rect = p_ImageViewer.GetCurrentRect_MemPos();
			p_Recipe.p_RecipeData.p_Roi[p_IndexMask].m_Surface.m_NonPattern[0].m_rt = rect;
		}


		#region Command
		public ICommand CanvasMouseLeftDown
		{
			get
			{
				return new RelayCommand(_mouseLeftDown);
			}
		}
		public ICommand CanvasMouseLeftUp
		{
			get
			{
				return new RelayCommand(_mouseLeftUp);
			}
		}
		public ICommand CanvasMouseMove
		{
			get
			{
				return new RelayCommand(_mouseMove);
			}
		}
		public ICommand CanvasMouseRightDown
		{
			get
			{
				return new RelayCommand(_mouseRightDown);
			}
		}
		public ICommand CanvasMouseWheel
		{
			get
			{
				return new RelayCommand(ReDrawRect);
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
		public RelayCommand CommandSaveMask
		{
			get
			{
				return new RelayCommand(SaveCurrentMask);
			}
		}



		private void _mouseLeftDown()
		{
			switch (m_SurfaceProgress)
			{
				case SurfaceProgress.None:
					{
						ReDrawRect();
						break;
					}
				case SurfaceProgress.Start:
					{
						StartDrawingOrigin();
						m_SurfaceProgress = SurfaceProgress.Drawing;
						break;
					}
				case SurfaceProgress.Drawing:
					{
						ReDrawRect();
						break;
					}
				case SurfaceProgress.Done:
					{
						ReDrawRect();

						if (m_MouseHitType == HitType.Body)
						{
							if (p_UIElement.Count > 0)
							{
								int result = p_UIElement.IndexOf(m_DrawHelper.NowRect);
								System.Windows.Shapes.Rectangle rect = (System.Windows.Shapes.Rectangle)p_UIElement[result];
								rect.Stroke = m_DrawHelper.NowRect.Stroke = System.Windows.Media.Brushes.GreenYellow;
								rect.StrokeDashArray = new DoubleCollection { 3, 2 };
								//m_DD.m_OriginData.m_color = System.Drawing.Color.GreenYellow;
								m_SurfaceProgress = SurfaceProgress.Select;
							}
						}
						break;
					}
				case SurfaceProgress.Select:
					{
						if (m_MouseHitType == HitType.Body)
						{
							int result = p_UIElement.IndexOf(m_DrawHelper.NowRect);
							System.Windows.Shapes.Rectangle rect = (System.Windows.Shapes.Rectangle)p_UIElement[result];
							rect.Stroke = m_DrawHelper.NowRect.Stroke = System.Windows.Media.Brushes.Red;
							rect.StrokeDashArray = new DoubleCollection(1);
							m_SurfaceProgress = SurfaceProgress.Done;
						}
						if (m_MouseHitType != HitType.None)
						{
							p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.Tool;
							m_DrawHelper.PreMousePt = new CPoint(p_ImageViewer.p_MouseX, p_ImageViewer.p_MouseY);
							m_DrawHelper.preRect.Left = (int)Canvas.GetLeft(m_DrawHelper.NowRect);
							m_DrawHelper.preRect.Right = (int)(m_DrawHelper.preRect.Left + m_DrawHelper.NowRect.Width);
							m_DrawHelper.preRect.Top = (int)Canvas.GetTop(m_DrawHelper.NowRect);
							m_DrawHelper.preRect.Bottom = (int)(m_DrawHelper.preRect.Top + m_DrawHelper.NowRect.Height);

							m_SurfaceProgress = SurfaceProgress.Adjusting;
						}
						if (m_MouseHitType == HitType.None)
						{
							p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.None;
							m_SurfaceProgress = SurfaceProgress.Done;
							//m_DD.m_OriginData.m_color = System.Drawing.Color.Red;
							ReDrawRect();
							//Debug.WriteLine("Drag:State->Done // Move");
						}
						break;
					}
				case SurfaceProgress.Adjusting:
					{
						break;
					}
			}
		}
		private void _mouseMove()
		{
			Test = m_SurfaceProgress.ToString();
			Test2 = p_ImageViewer.p_Mode.ToString();
			CPoint MousePoint = new CPoint(p_ImageViewer.p_MouseX, p_ImageViewer.p_MouseY);
			switch (m_SurfaceProgress)
			{
				case SurfaceProgress.None:
					{
						ReDrawRect();
						RedrawUIElement();
						break;
					}
				case SurfaceProgress.Start:
					{
						ReDrawRect();
						break;
					}
				case SurfaceProgress.Drawing:
					{
						DrawingRectProgress();
						break;
					}
				case SurfaceProgress.Done:
					{
						ReDrawRect();
						if (MouseEvent.LeftButton == MouseButtonState.Released)
						{
							//m_MouseHitType = SetHitType(MousePoint);
							m_MouseHitType = MouseOnRect(MousePoint);
							if (m_MouseHitType == HitType.Body)
							{
								SetMouseCursor();
							}
							else
							{

								m_MouseHitType = HitType.None;
								SetMouseCursor();
							}
						}
						break;
					}
				case SurfaceProgress.Select:
					{
						ReDrawRect();
						if (MouseEvent.LeftButton == MouseButtonState.Pressed)
						{
							if (m_MouseHitType != HitType.None)
							{
								p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.Tool;
								m_DrawHelper.PreMousePt = new CPoint(p_ImageViewer.p_MouseX, p_ImageViewer.p_MouseY);
								m_DrawHelper.preRect.Left = (int)Canvas.GetLeft(m_DrawHelper.NowRect);
								m_DrawHelper.preRect.Right = (int)(m_DrawHelper.preRect.Left + m_DrawHelper.NowRect.Width);
								m_DrawHelper.preRect.Top = (int)Canvas.GetTop(m_DrawHelper.NowRect);
								m_DrawHelper.preRect.Bottom = (int)(m_DrawHelper.preRect.Top + m_DrawHelper.NowRect.Height);

								m_SurfaceProgress = SurfaceProgress.Adjusting;
							}
						}
						if (MouseEvent.LeftButton == MouseButtonState.Released)
						{
							m_MouseHitType = SetHitType(MousePoint);
							SetMouseCursor();
						}
						break;
					}
				case SurfaceProgress.Adjusting:
					{
						// m_MouseHitType = SetHitType(MousePoint);
						// SetMouseCursor();
						if (MouseEvent.LeftButton == MouseButtonState.Pressed)
						{
							AdjustOrigin(MousePoint);
						}
						break;
					}
			}
		}
		private void _mouseRightDown()
		{
			switch (m_SurfaceProgress)
			{
				case SurfaceProgress.None:
					{
						break;
					}
				case SurfaceProgress.Start:
					{
						break;
					}
				case SurfaceProgress.Drawing:
					{
						DrawingRectDone();
						m_SurfaceProgress = SurfaceProgress.Start;
						break;
					}
				case SurfaceProgress.Done:
					{
						break;
					}
				case SurfaceProgress.Select:
					{
						break;
					}
				case SurfaceProgress.Adjusting:
					{
						break;
					}
			}
		}
		private void _mouseLeftUp()
		{
			switch (m_SurfaceProgress)
			{
				case SurfaceProgress.None:
					{
						break;
					}
				case SurfaceProgress.Start:
					{
						break;
					}
				case SurfaceProgress.Drawing:
					{
						break;
					}
				case SurfaceProgress.Done:
					{
						break;
					}
				case SurfaceProgress.Select:
					{
						break;
					}
				case SurfaceProgress.Adjusting:
					{
						p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.None;
						m_SurfaceProgress = SurfaceProgress.Select;
						break;
					}
			}
		}
		private HitType MouseOnRect(CPoint point)
		{
			if (m_DrawHelper.CanvasRectList != null)
			{
				foreach (Rectangle rect in m_DrawHelper.CanvasRectList)
				{
					double left = Canvas.GetLeft(rect);
					double top = Canvas.GetTop(rect);
					double right = left + rect.Width;
					double bottom = top + rect.Height;
					if (left < point.X && point.X < right &&
						top < point.Y && point.Y < bottom)
					{
						m_DrawHelper.NowRect = rect;
						return HitType.Body;
					}
				}
			}

			return HitType.None;
		}
		private HitType SetHitType(CPoint point)
		{
			double left = Canvas.GetLeft(m_DrawHelper.NowRect);
			double top = Canvas.GetTop(m_DrawHelper.NowRect);
			double right = left + m_DrawHelper.NowRect.Width;
			double bottom = top + m_DrawHelper.NowRect.Height;

			const double GAP = 10;
			if (point.X < left) return HitType.None;
			if (point.X > right) return HitType.None;
			if (point.Y < top) return HitType.None;
			if (point.Y > bottom) return HitType.None;
			if (-1 * GAP <= point.X - left && point.X - left <= GAP)
			{
				if (-1 * GAP <= point.Y - top && point.Y - top <= GAP)
					return HitType.UL;
				if (-1 * GAP <= bottom - point.Y && bottom - point.Y <= GAP)
					return HitType.LL;
				return HitType.L;
			}
			if (-1 * GAP < right - point.X && right - point.X <= GAP)
			{
				if (-1 * GAP <= point.Y - top && point.Y - top <= GAP)
					return HitType.UR;
				if (-1 * GAP <= bottom - point.Y && bottom - point.Y <= GAP)
					return HitType.LR;
				return HitType.R;
			}
			if (-1 * GAP <= point.Y - top && point.Y - top <= GAP)
				return HitType.T;
			if (-1 * GAP <= bottom - point.Y && bottom - point.Y <= GAP)
				return HitType.B;
			if (left == 0)
				return HitType.None;


			return HitType.Body;
		}
		private void SetMouseCursor()
		{
			// See what cursor we should display.
			Cursor desired_cursor = Cursors.Arrow;
			switch (m_MouseHitType)
			{
				case HitType.None:
					desired_cursor = Cursors.Arrow;
					break;
				case HitType.Body:
					desired_cursor = Cursors.ScrollAll;
					break;
				case HitType.UL:
				case HitType.LR:
					desired_cursor = Cursors.SizeNWSE;
					break;
				case HitType.LL:
				case HitType.UR:
					desired_cursor = Cursors.SizeNESW;
					break;
				case HitType.T:
				case HitType.B:
					desired_cursor = Cursors.SizeNS;
					break;
				case HitType.L:
				case HitType.R:
					desired_cursor = Cursors.SizeWE;
					break;
			}
			// Display the desired cursor.
			if (RecipeCursor != desired_cursor) RecipeCursor = desired_cursor;
		}
		private void AdjustOrigin(CPoint CurrentPoint)
		{
			int offset_x = CurrentPoint.X - m_DrawHelper.PreMousePt.X;
			int offset_y = CurrentPoint.Y - m_DrawHelper.PreMousePt.Y;
			CPoint Offset = new CPoint(offset_x, offset_y);
			int new_x = m_DrawHelper.preRect.Left;
			int new_y = m_DrawHelper.preRect.Top;
			int new_width = m_DrawHelper.preRect.Width;
			int new_height = m_DrawHelper.preRect.Height;

			switch (m_MouseHitType)
			{
				case HitType.Body:
					new_x += Offset.X;
					new_y += Offset.Y;
					break;
				case HitType.UL:
					new_x += Offset.X;
					new_y += Offset.Y;
					new_width -= Offset.X;
					new_height -= Offset.Y;
					break;
				case HitType.UR:
					new_y += Offset.Y;
					new_width += Offset.X;
					new_height -= Offset.Y;
					break;
				case HitType.LR:
					new_width += Offset.X;
					new_height += Offset.Y;
					break;
				case HitType.LL:
					new_x += Offset.X;
					new_width -= Offset.X;
					new_height += Offset.Y;
					break;
				case HitType.L:
					new_x += Offset.X;
					new_width -= Offset.X;
					break;
				case HitType.R:
					new_width += Offset.X;
					break;
				case HitType.B:
					new_height += Offset.Y;
					break;
				case HitType.T:
					new_y += Offset.Y;
					new_height -= Offset.Y;
					break;
			}

			Canvas.SetLeft(m_DrawHelper.NowRect, new_x);
			Canvas.SetTop(m_DrawHelper.NowRect, new_y);

			if (new_height < 50)
			{
				new_height = 50;
			}
			if (new_width < 50)
			{
				new_width = 50;
			}
			m_DrawHelper.NowRect.Width = new_width;
			m_DrawHelper.NowRect.Height = new_height;

			CPoint MemLeftTop = GetMemPoint((int)new_x, (int)new_y);
			CPoint MemRightBot = GetMemPoint((int)(new_x + new_width), (int)(new_y + new_height));
			// m_DD.m_OriginData.m_rt.Left = MemLeftTop.X;
			//m_DD.m_OriginData.m_rt.Top = MemLeftTop.Y;
			//m_DD.m_OriginData.m_rt.Right = MemRightBot.X;
			//m_DD.m_OriginData.m_rt.Bottom = MemRightBot.Y;

		}

		private void StartDrawingOrigin()
		{
			if (m_DrawHelper == null)
				m_DrawHelper = new DrawHelper();


			m_DrawHelper.NowRect = new System.Windows.Shapes.Rectangle();


			m_DrawHelper.Rect_StartPt = new CPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);
			CPoint CanvasPt = GetCanvasPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);

			Canvas.SetLeft(m_DrawHelper.NowRect, CanvasPt.X);
			Canvas.SetTop(m_DrawHelper.NowRect, CanvasPt.Y);

			m_DrawHelper.NowRect.Stroke = System.Windows.Media.Brushes.Red;
			m_DrawHelper.NowRect.StrokeThickness = 2;
			m_DrawHelper.NowRect.StrokeDashArray = new DoubleCollection { 3, 2 };
			p_UIElement.Add(m_DrawHelper.NowRect);

		}
		private void DrawingRectProgress()
		{
			if (m_DrawHelper.NowRect != null)
			{
				m_DrawHelper.Rect_EndPt = new CPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);
				CPoint StartPt = GetCanvasPoint(m_DrawHelper.Rect_StartPt.X, m_DrawHelper.Rect_StartPt.Y);
				CPoint NowPt = GetCanvasPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);


				Canvas.SetLeft(m_DrawHelper.NowRect, StartPt.X);
				Canvas.SetTop(m_DrawHelper.NowRect, StartPt.Y);


				if (m_DrawHelper.Rect_EndPt.X < m_DrawHelper.Rect_StartPt.X)
				{
					Canvas.SetLeft(m_DrawHelper.NowRect, NowPt.X);
				}
				if (m_DrawHelper.Rect_EndPt.Y < m_DrawHelper.Rect_StartPt.Y)
				{
					Canvas.SetTop(m_DrawHelper.NowRect, NowPt.Y);
				}
				m_DrawHelper.NowRect.Width = Math.Abs(StartPt.X - NowPt.X);
				m_DrawHelper.NowRect.Height = Math.Abs(StartPt.Y - NowPt.Y);

			}
		}
		private void DrawingRectDone()
		{
			NonPattern nonPattern = new NonPattern(); // New Rect
			nonPattern.m_rt = new CRect(); // Rect Info

			nonPattern.m_rt.Left = m_DrawHelper.Rect_StartPt.X;
			nonPattern.m_rt.Top = m_DrawHelper.Rect_StartPt.Y;
			nonPattern.m_rt.Right = m_DrawHelper.Rect_EndPt.X;
			nonPattern.m_rt.Bottom = m_DrawHelper.Rect_EndPt.Y;



			if (m_DrawHelper.Rect_EndPt.X < m_DrawHelper.Rect_StartPt.X)
			{
				nonPattern.m_rt.Left = m_DrawHelper.Rect_EndPt.X;
				nonPattern.m_rt.Right = m_DrawHelper.Rect_StartPt.X;

			}
			if (m_DrawHelper.Rect_EndPt.Y < m_DrawHelper.Rect_StartPt.Y)
			{
				nonPattern.m_rt.Top = m_DrawHelper.Rect_EndPt.Y;
				nonPattern.m_rt.Bottom = m_DrawHelper.Rect_StartPt.Y;

			}
			m_DrawHelper.NowRect.StrokeDashArray = new DoubleCollection(1);

			m_DrawHelper.CanvasRectList.Add(m_DrawHelper.NowRect);
			//m_Mask.m_Surface.m_NonPattern.Add(nonPattern); // Add Rect to Rect List
		}

		private void ClearUI()
		{
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
			//RedrawPt();
			//RedrawLine();
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
					//if (ViewWidth < Canvas.GetLeft(RedrawnTB) + RedrawnTB.ActualWidth)
					//{
					//    if (ViewWidth > Canvas.GetLeft(RedrawnTB))
					//    {
					//        RedrawnTB.Width = ViewWidth - Canvas.GetLeft(RedrawnTB);
					//    }
					//    else
					//    {
					//        RedrawnTB.Width = 0;
					//    }

					//}
					//if (ViewHeight < Canvas.GetTop(RedrawnTB) + RedrawnTB.ActualHeight)
					//{
					//    if (ViewHeight > Canvas.GetTop(RedrawnTB))
					//    {
					//        RedrawnTB.Height = ViewHeight - Canvas.GetTop(RedrawnTB);
					//    }
					//    else
					//    {
					//        RedrawnTB.Height = 0;
					//    }
					//}
					//if (Canvas.GetLeft(RedrawnTB) < 0)
					//{
					//    if (Math.Abs(Canvas.GetLeft(RedrawnTB)) < RedrawnTB.ActualWidth)
					//    {
					//        RedrawnTB.Width = RedrawnTB.Width - Math.Abs(Canvas.GetLeft(RedrawnTB));
					//        Canvas.SetLeft(RedrawnTB, 0);
					//    }
					//    else
					//    {
					//        RedrawnTB.Height = 0;
					//    }
					//}
					//if (Canvas.GetTop(RedrawnTB) < 0)
					//{
					//    if (Math.Abs(Canvas.GetTop(RedrawnTB)) < RedrawnTB.ActualHeight)
					//    {
					//        RedrawnTB.Height = RedrawnTB.Height - Math.Abs(Canvas.GetTop(RedrawnTB));
					//        Canvas.SetTop(RedrawnTB, 0);
					//    }
					//    else
					//    {
					//        RedrawnTB.Height = 0;
					//    }
					//}
					p_UIElement.Add(RedrawnTB);
				}

			}
		}
		//CLR_Demo clrDemo = new CLR_Demo();
		VSDBManager m_VSDB = new VSDBManager();
		List<CRect> DrawRectList;
		private void _btnInspTest()
		{
			CRect Mask_Rect = p_Recipe.p_RecipeData.p_Roi[0].m_Surface.m_NonPattern[0].m_rt;
			int nblocksize = 500;


			DrawRectList = m_Engineer.m_InspManager.CreateInspArea(Mask_Rect, nblocksize, p_Recipe.p_RecipeData.p_Roi[0].m_Surface.p_Parameter[0]);

			for (int i = 0; i < DrawRectList.Count; i++)
			{
				CRect inspblock = DrawRectList[i];
				m_DD.AddRectData(inspblock, System.Drawing.Color.Orange);

			}

			m_Engineer.m_InspManager.PreInspection();
			m_Engineer.m_InspManager.StartInspection();










			//InspectionProperty ip = new InspectionProperty();
			//ip.p_InspType = RootTools_Inspect.InspectionType.Surface;
			//ip.p_Rect = p_Recipe.p_RecipeData.p_Roi[0].m_Surface.m_NonPattern[0].m_rt;
			//ip.p_Sur_Param = p_Recipe.p_RecipeData.p_Roi[0].m_Surface.p_Parameter[0];
			//ip.p_index = 1;
			//m_Engineer.m_InspManager.AddInspection(ip);

			//InspectionProperty ip2 = new InspectionProperty();
			//ip2.p_InspType = RootTools_Inspect.InspectionType.Surface;
			//ip2.p_Rect = p_Recipe.p_RecipeData.p_Roi[1].m_Surface.m_NonPattern[0].m_rt;
			//ip2.p_Sur_Param = p_Recipe.p_RecipeData.p_Roi[1].m_Surface.p_Parameter[0];
			//ip2.p_index = 2;
			//m_Engineer.m_InspManager.AddInspection(ip2);

			//m_InspManager.StartInspection();

			//InspectionProperty ip2 = new InspectionProperty();
			//ip2.index = 2;
			//InspectionManager.Instance.AddInspection(ip2);

			//InspectionProperty ip3 = new InspectionProperty();
			//ip3.index = 3;
			//InspectionManager.Instance.AddInspection(ip3);

			//InspectionProperty ip4 = new InspectionProperty();
			//ip4.index = 4;
			//InspectionManager.Instance.AddInspection(ip4);

			//InspectionProperty ip5 = new InspectionProperty();
			//ip5.index = 5;
			//InspectionManager.Instance.AddInspection(ip5);

			//InspectionProperty ip6 = new InspectionProperty();
			//ip6.index = 6;
			//InspectionManager.Instance.AddInspection(ip6);

			//InspectionProperty ip7 = new InspectionProperty();
			//ip7.index = 7;
			//InspectionManager.Instance.AddInspection(ip7);

			//InspectionProperty ip8 = new InspectionProperty();
			//ip8.index = 8;
			//InspectionManager.Instance.AddInspection(ip8);

			//InspectionProperty ip9 = new InspectionProperty();
			//ip9.index = 9;
			//InspectionManager.Instance.AddInspection(ip9);


			//CPoint mem = m_Image.p_Size;

			//int a = clrDemo.Test_Inspection(mem.X, mem.Y);

			//m_VSDB.ConnectVSDB();
			//DataTable ds;
			//ds = m_VSDB.DBFillToTable("TempData");
			//int x = 0, y = 0;
			//CRect temp;
			//temp = new CRect(0, 0, 10, 10);
			//int w = 0, h = 0;
			//for (int i = 0; i < ds.Rows.Count; i++)
			//{
			//    for (int j = 0; j < ds.Columns.Count; j++)
			//    {
			//        if (j == 1)
			//            x = (int)ds.Rows[i][j];
			//        if (j == 2)
			//            y = (int)ds.Rows[i][j];

			//        if (j == 6)
			//            w = (int)ds.Rows[i][j];
			//        if (j == 7)
			//            h = (int)ds.Rows[i][j];

			//        if (j == 9)
			//            temp.Left = (int)ds.Rows[i][j];
			//        if (j == 10)
			//            temp.Top = (int)ds.Rows[i][j];
			//        if (j == 11)
			//            temp.Right = (int)ds.Rows[i][j];
			//        if (j == 12)
			//            temp.Bottom = (int)ds.Rows[i][j];

			//        //string x = d[i][j]; <-- something like this.
			//    }
			//    CRect aa = new CRect(temp.Left, temp.Top, temp.Right, temp.Bottom);
			//    m_DD.AddRectData(aa, System.Drawing.Color.Orange);
			//    string str;
			//    str = string.Format("width:{0},height{1}", w, h);
			//    CPoint bb = new CPoint(temp.Right, temp.Bottom);
			//    m_DD.AddString(str, bb, System.Drawing.Color.Red);
			//}
			//m_DD.AddRectData()

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
