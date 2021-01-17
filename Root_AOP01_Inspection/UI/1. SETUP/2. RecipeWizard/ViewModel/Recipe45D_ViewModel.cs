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
using MBrushes = System.Windows.Media.Brushes;
using DPoint = System.Drawing.Point;
using System.Data;
using MySql.Data.MySqlClient;

namespace Root_AOP01_Inspection
{
	enum ImageType
	{
		TDI,
		VRS,
		Mask,
	}
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

		#region EdgeDrawMode
		private bool _EdgeDrawMode;
		public bool EdgeDrawMode
		{
			get
			{
				return _EdgeDrawMode;
			}
			set
			{
				if (_EdgeDrawMode == value)
					return;

				if (m_ImageViewer_VM != null)
				{
					m_ImageViewer_VM.EdgeDrawMode = value;
					if (value)
					{
						m_ImageViewer_VM.Clear();
					}
				}
				SetProperty(ref _EdgeDrawMode, value);
			}
		}

		#endregion

		#region ResultDataTable
		//DataTable _OriginResultDataTable;
		DataTable _ResultDataTable;
		public DataTable ResultDataTable
		{
			get { return this._ResultDataTable; }
			set
			{
				this._ResultDataTable = value;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region SelectedDataTable
		DataRowView _SelectedDataTable;
		public DataRowView SelectedDataTable
		{
			get { return this._SelectedDataTable; }
			set
			{
				if (value != null)
				{
					this._SelectedDataTable = value;
					SetData(this.SelectedDataTable, ImageType.TDI);
				}
			}
		}
		#endregion

		#region SelectedTDIImage
		private ImageSource _SelectedTDIImage;
		public ImageSource SelectedTDIImage
		{
			get { return this._SelectedTDIImage; }
			set
			{
				this._SelectedTDIImage = value;
				this.RaisePropertyChanged();
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

		BacksideRecipe backsideRecipe
		{
			get
			{
				return recipe.GetRecipe<BacksideRecipe>();
			}
		}
		RecipeType_WaferMap mapInfo;
		TShape rectInfo;

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
			p_ImageViewer_VM.init(ProgramManager.Instance.Image45D);
			p_ImageViewer_VM.DrawDone += DrawDone_Callback;

			CenterPoint.X = 66000;//p_ImageViewer_VM.p_ImageData.p_Size.X / 2;
			CenterPoint.Y = 41000;// p_ImageViewer_VM.p_ImageData.p_Size.Y / 2;

			WorkEventManager.PositionDone += PositionDone_Callback;
			WorkEventManager.InspectionDone += SurfaceInspDone_Callback;
			WorkEventManager.ProcessDefectDone += ProcessDefectDone_Callback;
			WorkEventManager.ProcessDefectWaferDone += WorkEventManager_ProcessDefectWaferDone;

			SurfaceSize = 5;
			SurfaceGV = 70;

			EdgeDrawMode = false;
		}

		private void WorkEventManager_ProcessDefectWaferDone(object sender, ProcessDefectWaferDoneEventArgs e)
		{
			ResultDataTable = new DataTable();

			MySqlConnection _conn = new MySqlConnection();
			string query = "SELECT * FROM inspections.defect;";
			try
			{
				_conn = new MySqlConnection(App.connection);
				_conn.Open();
				var _cmd = new MySqlCommand
				{
					Connection = _conn,
					CommandText = query
				};
				_cmd.ExecuteNonQuery();

				var _da = new MySqlDataAdapter(_cmd);
				_da.Fill(ResultDataTable);

				var _cb = new MySqlCommandBuilder(_da);

				_conn.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				if (_conn != null) _conn.Close();
			}
		}

		private void ProcessDefectDone_Callback(object obj, ProcessDefectDoneEventArgs args)
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
			//List<string> textList = new List<string>();
			//List<CRect> rectList = new List<CRect>();

			if (workplace.DefectList.Count > 0)
			{
				Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
				{
					m_Setup.InspectionManager.AddDefect(workplace.DefectList);
				}));
			}
		}

		public void DrawRectDefect(List<CRect> rectList, List<String> text, bool reDraw = false)
		{
			if (reDraw)
				p_ImageViewer_VM.Clear();

			p_ImageViewer_VM.DrawRect(rectList, Recipe45D_ImageViewer_ViewModel.ColorType.Defect, text);
		}

		private void showEdgeBox()
		{
		}
		List<TRect> tempList = new List<TRect>();
		private void saveEdgeBox()
		{
			if (m_ImageViewer_VM.TRectList.Count == 6)
			{
				tempList = new List<TRect>(m_ImageViewer_VM.TRectList);
			}
		}
		string AOPImageRootPath = @"D:\DefectImage";
		private void SetData(DataRowView selectedDataTable, ImageType type)
		{
			int idx = Convert.ToInt32(selectedDataTable["m_nDefectIndex"]);

			if (System.IO.Directory.Exists(System.IO.Path.Combine(AOPImageRootPath)))
			{
				string currentInspection = RootTools.Database.DatabaseManager.Instance.InspectionID;
				string imagePath = System.IO.Path.Combine(AOPImageRootPath, currentInspection, idx.ToString() + ".bmp");
				if (System.IO.File.Exists(imagePath))
				{
					System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(imagePath);
					SelectedTDIImage = ConvertImage(bmp);
					bmp.Dispose();
				}
			}
		}
		public System.Windows.Media.ImageSource ConvertImage(System.Drawing.Image image)
		{
			try
			{
				if (image != null)
				{
					var bitmap = new System.Windows.Media.Imaging.BitmapImage();
					bitmap.BeginInit();
					System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
					image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
					memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
					bitmap.StreamSource = memoryStream;
					bitmap.EndInit();
					return bitmap;
				}
			}
			catch { }
			return null;
		}
		CRect searchArea(int x_margin, int y_margin)
		{
			// variable
			List<Rect> arcROIs = new List<Rect>();
			List<DPoint> aptEdges = new List<DPoint>();
			Recipe45D_ImageViewer_ViewModel ivvm = m_ImageViewer_VM;
			RootTools.Inspects.eEdgeFindDirection eTempDirection = RootTools.Inspects.eEdgeFindDirection.TOP;
			DPoint ptLeft1, ptLeft2, ptBottom, ptRight1, ptRight2, ptTop;
			DPoint ptLT, ptRT, ptLB, ptRB;


			arcROIs.Clear();
			aptEdges.Clear();
			for (int j = 0; j < 6; j++)
			{
				if (tempList.Count < 6) break;
				arcROIs.Add(new Rect(
					tempList[j].MemoryRect.Left,
					tempList[j].MemoryRect.Top,
					tempList[j].MemoryRect.Width,
					tempList[j].MemoryRect.Height));
			}
			if (arcROIs.Count < 6) return new CRect(-1, -1, -1, -1);
			for (int j = 0; j < arcROIs.Count; j++)
			{
				eTempDirection = InspectionManager_AOP.GetDirection(ivvm.p_ImageData, arcROIs[j]);
				aptEdges.Add(InspectionManager_AOP.GetEdge(ivvm.p_ImageData, arcROIs[j], eTempDirection, true, true, 30));
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

			//m_ImageViewer_VM.DrawLine(ptLT, ptLB, MBrushes.Lime);
			//DrawLine(ptRB, ptRT, MBrushes.Lime);
			//DrawLine(ptLT, ptRT, MBrushes.Lime);
			//DrawLine(ptLB, ptRB, MBrushes.Lime);

			m_ImageViewer_VM.DrawRect(new CPoint(ptLeft1.X - 10, ptLeft1.Y - 10), new CPoint(ptLeft1.X + 10, ptLeft1.Y + 10), Recipe45D_ImageViewer_ViewModel.ColorType.Defect);
			m_ImageViewer_VM.DrawRect(new CPoint(ptLeft2.X - 10, ptLeft2.Y - 10), new CPoint(ptLeft2.X + 10, ptLeft2.Y + 10), Recipe45D_ImageViewer_ViewModel.ColorType.Defect);
			m_ImageViewer_VM.DrawRect(new CPoint(ptBottom.X - 10, ptBottom.Y - 10), new CPoint(ptBottom.X + 10, ptBottom.Y + 10), Recipe45D_ImageViewer_ViewModel.ColorType.Defect);
			m_ImageViewer_VM.DrawRect(new CPoint(ptRight1.X - 10, ptRight1.Y - 10), new CPoint(ptRight1.X + 10, ptRight1.Y + 10), Recipe45D_ImageViewer_ViewModel.ColorType.Defect);
			m_ImageViewer_VM.DrawRect(new CPoint(ptRight2.X - 10, ptRight2.Y - 10), new CPoint(ptRight2.X + 10, ptRight2.Y + 10), Recipe45D_ImageViewer_ViewModel.ColorType.Defect);
			m_ImageViewer_VM.DrawRect(new CPoint(ptTop.X - 100, ptTop.Y - 100), new CPoint(ptTop.X + 100, ptTop.Y + 100), Recipe45D_ImageViewer_ViewModel.ColorType.Defect);

			m_ImageViewer_VM.DrawRect(new CPoint(ptLT.X, ptLT.Y), new CPoint(ptRB.X, ptRB.Y), Recipe45D_ImageViewer_ViewModel.ColorType.ChipFeature);

			tempList.Clear();

			return new CRect(new Point(ptLT.X + x_margin, ptLT.Y + y_margin), new Point(ptRB.X - x_margin, ptRB.Y - y_margin));
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
			if (!EdgeDrawMode)
			{
				p_ImageViewer_VM.Clear();
				this.m_ImageViewer_VM.DrawRect(leftTop, rightBottom, Recipe45D_ImageViewer_ViewModel.ColorType.Defect);
			}
			else
			{
				//edge box draw mode. 최대개수는 6개로 고정한다
				this.m_ImageViewer_VM.DrawRect(leftTop, rightBottom, Recipe45D_ImageViewer_ViewModel.ColorType.FeatureMatching);

				if (this.m_ImageViewer_VM.Shapes.Count > 7)
				{
					m_ImageViewer_VM.Shapes.RemoveAt(0);
					m_ImageViewer_VM.p_DrawElement.RemoveAt(0);
				}
			}
			m_Setup.InspectionManager.RefreshDefect();

		}
		public ICommand commandShowEdgeBox
		{
			get
			{
				return new RelayCommand(showEdgeBox);
			}
		}
		public ICommand commandSaveEdgeBox
		{
			get
			{
				return new RelayCommand(saveEdgeBox);
			}
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

			int outMapX = 10, outMapY = 10;
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

			List<Cpp_Point> WaferEdge = new List<Cpp_Point>();
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

					var area = searchArea(200, 200);
					WaferEdge.Add(new Cpp_Point(area.Left / DownSample, area.Top / DownSample));
					WaferEdge.Add(new Cpp_Point(area.Left / DownSample, area.Bottom / DownSample));
					WaferEdge.Add(new Cpp_Point(area.Right / DownSample, area.Bottom / DownSample));
					WaferEdge.Add(new Cpp_Point(area.Right / DownSample, area.Top / DownSample));

					mapData = CLR_IP.Cpp_GenerateMapData(
						WaferEdge.ToArray(),
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
			for (int i = 0; i < mapX * mapY; i++)
			{
				if (y == 0 || y == mapY - 1)
				{
					mapInfo.Data[i] = 0;
				}
				else if (x == 0 || x == mapX - 1)
				{
					mapInfo.Data[i] = 0;
				}
				x++;
				if (x >= mapX)
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
			List<RootTools.Database.Defect> rectList = new List<RootTools.Database.Defect>();
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
						var data = new RootTools.Database.Defect();

						var left = OriginX + x * ChipSzX;
						var top = offsetY + y * ChipSzY;
						var right = OriginX + (x + 1) * ChipSzX;
						var bot = offsetY + (y + 1) * ChipSzY;

						var width = right - left;
						var height = bot - top;
						left = (int)(left - width / 2.0);
						top = (int)(top - height / 2.0);

						data.p_rtDefectBox = new Rect(left, top, width, height);
						rectList.Add(data);
					}


			//m_ImageViewer_VM.DrawRect(rectList, Recipe45D_ImageViewer_ViewModel.ColorType.MapData);
			m_Setup.InspectionManager.AddRect(rectList, null, new Pen(Brushes.Green, 2));
		}
		//private void DrawRect(List<CRect> RectList, ColorType color, List<String> textList = null, int FontSz = 15)
		//{
		//	foreach (CRect rectPoint in RectList)
		//	{
		//		SetShapeColor(color);
		//		TRect rect = rectInfo as TRect;

		//		rect.MemPointBuffer = new CPoint(rectPoint.Left, rectPoint.Top);
		//		rect.MemoryRect.Left = rectPoint.Left;
		//		rect.MemoryRect.Top = rectPoint.Top;
		//		rectInfo = Drawing(rectInfo, new CPoint(rectPoint.Right, rectPoint.Bottom));

		//		Shapes.Add(rectInfo);
		//		m_ImageViewer_VM.p_ViewElement.Add(rectInfo.UIElement);
		//	}
		//}
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
		private void startTestInsp()
		{
			saveEdgeBox();

			p_ImageViewer_VM.Clear();
			m_Setup.InspectionManager.ClearDefect();
			m_Setup.InspectionManager.ResetWorkManager();

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


			m_Setup.InspectionManager.InspectionStart();
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
