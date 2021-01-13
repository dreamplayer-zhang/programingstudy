using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using Root_AOP01_Inspection.Module;
using RootTools;
using RootTools_CLR;
using RootTools_Vision;
using static RootTools.RootViewer_ViewModel;

namespace Root_AOP01_Inspection
{
	class Recipe45D_ViewModel : ObservableObject
	{
		Setup_ViewModel m_Setup;
		AOP01_Engineer m_Engineer;
		object lockObj = new object();
		List<CPoint> PolygonPt = new List<CPoint>();
		public ObservableCollection<TShape> Shapes = new ObservableCollection<TShape>();
		private ObservableCollection<UIElement> m_UIElements = new ObservableCollection<UIElement>();
		Recipe recipe
		{
			get
			{
				return m_Setup.InspectionManager.Recipe;
			}
		}

		#region CenterPoint

		private CPoint centerPoint = new CPoint();
		public CPoint CenterPoint
		{
			get
			{
				return centerPoint;
			}
			set
			{
				SetProperty(ref centerPoint, value);
			}
		}
		#endregion

		#region p_eColorViewMode
		public eColorViewMode m_eColorViewMode = eColorViewMode.All;
		public eColorViewMode p_eColorViewMode
		{
			get
			{
				return m_eColorViewMode;
			}
			set
			{
				SetProperty(ref m_eColorViewMode, value);
				//SetImageSource();
			}
		}
		#endregion

		#region BrightGV
		private bool _BrightGV;
		public bool BrightGV
		{
			get
			{
				return _BrightGV;
			}
			set
			{
				SetProperty(ref _BrightGV, value);
			}
		}

		#endregion

		#region SurfaceGV
		private int _SurfaceGV;
		public int SurfaceGV
		{
			get
			{
				return _SurfaceGV;
			}
			set
			{
				SetProperty(ref _SurfaceGV, value);
			}
		}

		#endregion

		#region SurfaceSize
		private int _SurfaceSize;
		public int SurfaceSize
		{
			get
			{
				return _SurfaceSize;
			}
			set
			{
				SetProperty(ref _SurfaceSize, value);
			}
		}

		#endregion

		public ObservableCollection<UIElement> UIElements
		{
			get
			{
				return m_UIElements;
			}
			set
			{
				m_UIElements = value;
			}
		}

		Polygon WAFEREDGE_UI;
		CPoint canvasPoint;
		Grid CENTERPOINT_UI;
		BacksideRecipe backsideRecipe
		{
			get
			{
				return recipe.GetRecipe<BacksideRecipe>();
			}
		}
		RecipeType_WaferMap mapInfo;
		TShape rectInfo;

		public enum ColorType
		{
			Teaching,
			WaferEdge,
			WaferCenter,
			MapData,
		}
		private Recipe45D_ImageViewer_ViewModel m_ImageViewer_VM;
		public Recipe45D_ImageViewer_ViewModel p_ImageViewer_VM
		{
			get
			{
				return m_ImageViewer_VM;
			}
			set
			{
				SetProperty(ref m_ImageViewer_VM, value);
			}
		}
		public Recipe45D_ViewModel(Setup_ViewModel setup)
		{
			m_Setup = setup;
			m_Engineer = m_Setup.m_MainWindow.m_engineer;

			p_ImageViewer_VM = new Recipe45D_ImageViewer_ViewModel();
			p_ImageViewer_VM.init(ProgramManager.Instance.Image);
			p_ImageViewer_VM.DrawDone += DrawDone_Callback;

			CenterPoint.X = 66000;//p_ImageViewer_VM.p_ImageData.p_Size.X / 2;
			CenterPoint.Y = 41000;// p_ImageViewer_VM.p_ImageData.p_Size.Y / 2;

			WorkEventManager.PositionDone += PositionDone_Callback;
			WorkEventManager.InspectionDone += SurfaceInspDone_Callback;
			WorkEventManager.ProcessDefectDone += ProcessDefectDone_Callback;

			SurfaceSize = 5;
			SurfaceGV = 70;
		}

		private void ProcessDefectDone_Callback(object obj, PocessDefectDoneEventArgs args)
		{
			Workplace workplace = obj as Workplace;

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				//UpdateDataGrid();
			}));
		}

		private void SurfaceInspDone_Callback(object obj, InspectionDoneEventArgs args)
		{
			Workplace workplace = obj as Workplace;
			List<string> textList = new List<string>();
			List<CRect> rectList = new List<CRect>();
			foreach (RootTools.Database.Defect defectInfo in workplace.DefectList)
			{
				string text = "";

				if (false) // Display Option : Rel Position
					text += "Pos : {" + defectInfo.m_fRelX.ToString() + ", " + defectInfo.m_fRelY.ToString() + "}" + "\n";
				if (false) // Display Option : Defect Size
					text += "Size : " + defectInfo.m_fSize.ToString() + "\n";
				if (false) // Display Option : GV Value
					text += "GV : " + defectInfo.m_fGV.ToString() + "\n";

				rectList.Add(new CRect((int)defectInfo.p_rtDefectBox.Left, (int)defectInfo.p_rtDefectBox.Top, (int)defectInfo.p_rtDefectBox.Right, (int)defectInfo.p_rtDefectBox.Bottom));
				textList.Add(text);
			}

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				DrawRectDefect(rectList, textList, args.reDraw);
			}));
		}

		public void DrawRectDefect(List<CRect> rectList, List<String> text, bool reDraw = false)
		{
			if (reDraw)
				p_ImageViewer_VM.Clear();

			p_ImageViewer_VM.DrawRect(rectList,  Recipe45D_ImageViewer_ViewModel.ColorType.Defect, text);
		}

		private void PositionDone_Callback(object obj, PositionDoneEventArgs args)
		{
			Workplace workplace = obj as Workplace;
			lock (this.lockObj)
			{
				Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
				{
					String test = "";
					if (true) // Display Option : Position Trans
					{
						test += "Trans : {" + workplace.TransX.ToString() + ", " + workplace.TransY.ToString() + "}" + "\n";
					}
					DrawRectMasterFeature(args.ptOldStart, args.ptOldEnd, args.ptNewStart, args.ptNewEnd, test, args.bSuccess);
					//else
					//	DrawRectChipFeature(args.ptOldStart, args.ptOldEnd, args.ptNewStart, args.ptNewEnd, test, args.bSuccess);
				}));
			}
		}
		public void DrawRectMasterFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text, bool bSuccess)
		{
			p_ImageViewer_VM.DrawRect(ptOldStart, ptOldEnd, Recipe45D_ImageViewer_ViewModel.ColorType.MasterFeature);
			p_ImageViewer_VM.DrawRect(ptNewStart, ptNewEnd, bSuccess ? Recipe45D_ImageViewer_ViewModel.ColorType.FeatureMatching : Recipe45D_ImageViewer_ViewModel.ColorType.FeatureMatchingFail, text);
		}

		private void DrawDone_Callback(CPoint leftTop, CPoint rightBottom)
		{
			p_ImageViewer_VM.Clear();
			this.m_ImageViewer_VM.DrawRect(leftTop, rightBottom, Recipe45D_ImageViewer_ViewModel.ColorType.Defect);
		}

		public ICommand commandInspTest
		{
			get
			{
				return new RelayCommand(startTestInsp);
			}
		}

		private bool _StartRecipeTeaching()
		{
#if !DEBUG
			try
			{
#endif
			int memH = p_ImageViewer_VM.p_ImageData.p_Size.Y;
				int memW = p_ImageViewer_VM.p_ImageData.p_Size.X;

				float centX = CenterPoint.X; // 레시피 티칭 값 가지고오기
				float centY = CenterPoint.Y;

				int outMapX = 40, outMapY = 40;
				float outOriginX, outOriginY;
				float outChipSzX, outChipSzY;
				float outRadius = 80000;
				bool isIncludeMode = true;

				IntPtr MainImage = new IntPtr();
				if (p_ImageViewer_VM.p_ImageData.p_nByte == 3)
				{
					if (p_eColorViewMode != eColorViewMode.All)
						MainImage = p_ImageViewer_VM.p_ImageData.GetPtr((int)p_eColorViewMode - 1);
					else
						MainImage = p_ImageViewer_VM.p_ImageData.GetPtr(0);
				}
				else
				{ // All 일때는 R채널로...
					MainImage = p_ImageViewer_VM.p_ImageData.GetPtr(0);
				}

				Cpp_Point[] WaferEdge = null;
				int[] mapData = null;
				unsafe
				{
					int DownSample = 40;

					fixed (byte* pImg = new byte[(long)(memW / DownSample) * (long)(memH / DownSample)]) // 원본 이미지 너무 커서 안열림
					{
						CLR_IP.Cpp_SubSampling((byte*)MainImage, pImg, memW, memH, 0, 0, memW, memH, DownSample);

						// Param Down Scale
						centX /= DownSample; centY /= DownSample;
						outRadius /= DownSample;
						memW /= DownSample; memH /= DownSample;

						WaferEdge = CLR_IP.Cpp_FindWaferEdge(pImg,
							&centX, &centY,
							&outRadius,
							memW, memH,
							1
							);

						mapData = CLR_IP.Cpp_GenerateMapData(
							WaferEdge,
							&outOriginX,
							&outOriginY,
							&outChipSzX,
							&outChipSzY,
							&outMapX,
							&outMapY,
							memW, memH,
							1,
							isIncludeMode
							);
					}

					// Param Up Scale
					centX *= DownSample; centY *= DownSample;
					outRadius *= DownSample;
					outOriginX *= DownSample; outOriginY *= DownSample;
					outChipSzX *= DownSample; outChipSzY *= DownSample;

					PolygonPt.Clear();
					if (WAFEREDGE_UI != null)
						WAFEREDGE_UI.Points.Clear();

					for (int i = 0; i < WaferEdge.Length; i++)
						PolygonPt.Add(new CPoint(WaferEdge[i].x * DownSample, WaferEdge[i].y * DownSample));

					// UI Data Update
					CenterPoint = new CPoint((int)centX, (int)centY);

					canvasPoint = m_ImageViewer_VM.GetCanvasPoint(new CPoint(CenterPoint.X, CenterPoint.Y));
					DrawCenterPoint(ColorType.WaferCenter);

					// Wafer Edge Draw                
					DrawPolygon(PolygonPt);
					ReDrawWFCenter(ColorType.WaferCenter);

					// Save Recipe
					SetRecipeMapData(mapData, (int)outMapX, (int)outMapY, (int)outOriginX, (int)outOriginY, (int)outChipSzX, (int)outChipSzY);

					backsideRecipe.CenterX = (int)centX;
					backsideRecipe.CenterY = (int)centY;
					backsideRecipe.Radius = (int)outRadius;

					//SaveContourMap((int)centX, (int)centY, (int)outRadius);
				}
				return true;
			#if !DEBUG
			}
			catch (Exception ex)
			{
				return false;
			}
			#endif
		}
		private void SetRecipeMapData(int[] mapData, int mapX, int mapY, int originX, int originY, int chipSzX, int chipSzY)
		{
			// Map Data Recipe 생성
			backsideRecipe.OriginX = originX;
			backsideRecipe.OriginY = originY;
			backsideRecipe.DiePitchX = chipSzX;
			backsideRecipe.DiePitchY = chipSzY;

			OriginRecipe originRecipe = recipe.GetRecipe<OriginRecipe>();
			originRecipe.DiePitchX = chipSzX;
			originRecipe.DiePitchY = chipSzY;
			originRecipe.OriginX = originX;
			originRecipe.OriginY = originY;

			mapInfo = new RecipeType_WaferMap(mapX, mapY, mapData);
			int x = 0; int y = 0;
			for (int i = 0; i < mapX*mapY; i++)
			{
				if(y==0 || y == mapY-1)
				{
					mapInfo.Data[i] = 0;
				}
				else if(x==0 || x == mapX-1)
				{
					mapInfo.Data[i] = 0;
				}
				x++;
				if(x >= mapX)
				{
					y++;
					x = 0;
				}
			}

			this.recipe.WaferMap = mapInfo;

			if (true) // Display Map Data Option화
				DrawMapData(mapData, mapX, mapY, originX, originY, chipSzX, chipSzY);
		}
		private void RemoveMapDataRect()
		{
			foreach (TShape shape in Shapes)
			{
				if (UIElements.Contains(shape.UIElement))
					UIElements.Remove(shape.UIElement);
			}
			Shapes.Clear();
		}
		private void DrawMapData(int[] mapData, int mapX, int mapY, int OriginX, int OriginY, int ChipSzX, int ChipSzY)
		{
			RemoveMapDataRect();
			// Map Display
			List<CRect> rectList = new List<CRect>();
			int offsetY = 0;
			bool isOrigin = true;

			for (int x = 0; x < mapX; x++)
				for (int y = 0; y < mapY; y++)
					if (mapData[y * mapX + x] == 1)
					{
						if (isOrigin)
						{
							offsetY = OriginY - (y + 1) * ChipSzY;
							mapInfo.MasterDieX = x;
							mapInfo.MasterDieY = y;
							isOrigin = false;
						}

						rectList.Add(new CRect(OriginX + x * ChipSzX, offsetY + y * ChipSzY, OriginX + (x + 1) * ChipSzX, offsetY + (y + 1) * ChipSzY));
					}


			DrawRect(rectList, ColorType.MapData);
		}
		private void DrawRect(List<CRect> RectList, ColorType color, List<String> textList = null, int FontSz = 15)
		{
			foreach (CRect rectPoint in RectList)
			{
				SetShapeColor(color);
				TRect rect = rectInfo as TRect;

				rect.MemPointBuffer = new CPoint(rectPoint.Left, rectPoint.Top);
				rect.MemoryRect.Left = rectPoint.Left;
				rect.MemoryRect.Top = rectPoint.Top;
				rectInfo = Drawing(rectInfo, new CPoint(rectPoint.Right, rectPoint.Bottom));

				Shapes.Add(rectInfo);
				m_ImageViewer_VM.p_ViewElement.Add(rectInfo.UIElement);
			}
		}
		private TShape Drawing(TShape shape, CPoint memPt)
		{
			TRect rect = shape as TRect;
			// memright가 0인상태로 canvas rect width가 정해져서 버그...
			// 0이면 min정해줘야되나
			if (rect.MemPointBuffer.X > memPt.X)
			{
				rect.MemoryRect.Left = memPt.X;
				rect.MemoryRect.Right = rect.MemPointBuffer.X;
			}
			else
			{
				rect.MemoryRect.Left = rect.MemPointBuffer.X;
				rect.MemoryRect.Right = memPt.X;
			}
			if (rect.MemPointBuffer.Y > memPt.Y)
			{
				rect.MemoryRect.Top = memPt.Y;
				rect.MemoryRect.Bottom = rect.MemPointBuffer.Y;
			}
			else
			{
				rect.MemoryRect.Top = rect.MemPointBuffer.Y;
				rect.MemoryRect.Bottom = memPt.Y;
			}

			CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
			CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);
			CPoint canvasLT = new CPoint(m_ImageViewer_VM.GetCanvasPoint(LT));
			CPoint canvasRB = new CPoint(m_ImageViewer_VM.GetCanvasPoint(RB));

			int width = Math.Abs(canvasRB.X - canvasLT.X);
			int height = Math.Abs(canvasRB.Y - canvasLT.Y);
			Canvas.SetLeft(rect.CanvasRect, canvasLT.X);
			Canvas.SetTop(rect.CanvasRect, canvasLT.Y);
			Canvas.SetRight(rect.CanvasRect, canvasRB.X);
			Canvas.SetBottom(rect.CanvasRect, canvasRB.Y);
			rect.CanvasRect.Width = width;
			rect.CanvasRect.Height = height;

			return shape;
		}
		private void SetShapeColor(ColorType color)
		{
			switch (color)
			{
				case ColorType.MapData:
					rectInfo = new TRect(Brushes.LimeGreen, 1, 1);
					break;
				default:
					rectInfo = new TRect(Brushes.Black, 1, 1);
					break;
			}

		}
		private void ReDrawWFCenter(ColorType color)
		{
			if (CENTERPOINT_UI == null)
				return;

			if (m_ImageViewer_VM.p_ViewElement.Contains(CENTERPOINT_UI))
				m_ImageViewer_VM.p_ViewElement.Remove(CENTERPOINT_UI);

			foreach (UIElement ui in CENTERPOINT_UI.Children)
			{
				Line line = ui as Line;
				line.Stroke = GetColorBrushType(color);
			}

			CPoint WFCenter = m_ImageViewer_VM.GetCanvasPoint(new CPoint(CenterPoint.X, CenterPoint.Y));
			Canvas.SetLeft(CENTERPOINT_UI, WFCenter.X - 10);
			Canvas.SetTop(CENTERPOINT_UI, WFCenter.Y - 10);

			m_ImageViewer_VM.p_ViewElement.Add(CENTERPOINT_UI);
		}
		private void DrawPolygon(List<CPoint> memPolyPt)
		{
			if (m_ImageViewer_VM.p_ViewElement.Contains(WAFEREDGE_UI))
				m_ImageViewer_VM.p_ViewElement.Remove(WAFEREDGE_UI);

			if (WAFEREDGE_UI == null)
			{
				WAFEREDGE_UI = new Polygon();
				WAFEREDGE_UI.Stroke = GetColorBrushType(ColorType.WaferEdge);
				WAFEREDGE_UI.StrokeThickness = 3;
				WAFEREDGE_UI.StrokeDashArray = new DoubleCollection { 2, 2 };
			}
			else
				WAFEREDGE_UI.Points.Clear();

			foreach (CPoint pt in memPolyPt)
			{
				CPoint a = new CPoint();
				a = m_ImageViewer_VM.GetCanvasPoint(new CPoint(pt.X, pt.Y));
				WAFEREDGE_UI.Points.Add(new Point(a.X, a.Y));
			}
			m_ImageViewer_VM.p_ViewElement.Add(WAFEREDGE_UI);
		}
		private void DrawCenterPoint(ColorType color)
		{
			if (m_ImageViewer_VM.p_ViewElement.Contains(CENTERPOINT_UI))
				m_ImageViewer_VM.p_ViewElement.Remove(CENTERPOINT_UI);

			if (CENTERPOINT_UI == null)
			{
				CENTERPOINT_UI = new Grid();
				CENTERPOINT_UI.Width = 20;
				CENTERPOINT_UI.Height = 20;

				Line line1 = new Line();
				line1.X1 = 0;
				line1.Y1 = 0;
				line1.X2 = 1;
				line1.Y2 = 1;
				//line1.Stroke = GetColorBrushType(color);
				line1.StrokeThickness = 1.5;
				line1.Stretch = Stretch.Fill;
				Line line2 = new Line();
				line2.X1 = 0;
				line2.Y1 = 1;
				line2.X2 = 1;
				line2.Y2 = 0;
				//line2.Stroke = GetColorBrushType(color);
				line2.StrokeThickness = 1.5;
				line2.Stretch = Stretch.Fill;
				CENTERPOINT_UI.Children.Add(line1);
				CENTERPOINT_UI.Children.Add(line2);
			}

			foreach (UIElement ui in CENTERPOINT_UI.Children)
			{
				Line line = ui as Line;
				line.Stroke = GetColorBrushType(color);
			}

			Canvas.SetLeft(CENTERPOINT_UI, canvasPoint.X - 10);
			Canvas.SetTop(CENTERPOINT_UI, canvasPoint.Y - 10);
			m_ImageViewer_VM.p_ViewElement.Add(CENTERPOINT_UI);
		}
		private System.Windows.Media.SolidColorBrush GetColorBrushType(ColorType color)
		{
			switch (color) // 색상 정리 다시...
			{
				case ColorType.Teaching:
					return Brushes.Blue;
				case ColorType.WaferEdge:
					return Brushes.Green;
				case ColorType.WaferCenter:
					return Brushes.Magenta;
				case ColorType.MapData:
					return Brushes.Yellow;
				default:
					return Brushes.Black;
			}
		}
		private void startTestInsp()
		{
			p_ImageViewer_VM.Clear();

			_StartRecipeTeaching();

			var temp = m_Setup.InspectionManager.Recipe.GetRecipe<BacksideRecipe>();
			temp = backsideRecipe;

			BacksideSurfaceParameter surParam = new BacksideSurfaceParameter();
			surParam.IsBright = BrightGV;
			surParam.Intensity = SurfaceGV;
			surParam.Size = SurfaceSize;
			recipe.ParameterItemList.Add(surParam);

			IntPtr SharedBuf = new IntPtr();
			if (p_ImageViewer_VM.p_ImageData.p_nByte == 3)
			{
				if (p_ImageViewer_VM.p_eColorViewMode != RootViewer_ViewModel.eColorViewMode.All)
					SharedBuf = p_ImageViewer_VM.p_ImageData.GetPtr((int)p_ImageViewer_VM.p_eColorViewMode - 1);
				else // All 일때는 R채널로...
					SharedBuf = p_ImageViewer_VM.p_ImageData.GetPtr(0);

				m_Setup.InspectionManager.SetColorSharedBuffer(p_ImageViewer_VM.p_ImageData.GetPtr(0), p_ImageViewer_VM.p_ImageData.GetPtr(1), p_ImageViewer_VM.p_ImageData.GetPtr(2));
			}
			else
			{
				SharedBuf = p_ImageViewer_VM.p_ImageData.GetPtr();
				m_Setup.InspectionManager.SharedBufferR_Gray = SharedBuf;
			}

			m_Setup.InspectionManager.SharedBufferByteCnt = p_ImageViewer_VM.p_ImageData.p_nByte;

			if (m_Setup.InspectionManager.CreateInspection() == false)
			{
				return;
			}
			m_Setup.InspectionManager.Start(false);
		}

		public ICommand btnBack
		{
			get
			{
				return new RelayCommand(() =>
				{
					m_Setup.Set_RecipeWizardPanel();
				});
			}
		}
		public ICommand btnSnap
		{
			get
			{
				return new RelayCommand(() =>
				{
					MainVision mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;
					MainVision.Run_Grab45 grab = (MainVision.Run_Grab45)mainVision.CloneModuleRun("Run Grab 45");
					mainVision.StartRun(grab);
				});
			}
		}
	}
}
