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
	class RecipeFrontside_ViewModel : ObservableObject
	{
		Setup_ViewModel m_Setup;
		AOP01_Engineer m_Engineer;
		MainVision m_mainVision;
		RecipeType_WaferMap mapInfo;

		BacksideRecipe backsideRecipe
		{
			get
			{
				return recipe.GetRecipe<BacksideRecipe>();
			}
		}
		Recipe recipe
		{
			get
			{
				return m_Setup.PatternInspectionManager.Recipe;
			}
		}
		public MainVision p_mainVision
		{
			get { return m_mainVision; }
			set { SetProperty(ref m_mainVision, value); }
		}
		public RecipeFrontside_ViewModel(Setup_ViewModel setup)
		{
			m_Setup = setup;
			m_Engineer = setup.m_MainWindow.m_engineer;
			m_mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;

			p_ImageViewer_VM = new RecipeFrontside_Viewer_ViewModel();
			p_ImageViewer_VM.init(ProgramManager.Instance.ImageMain);
			p_ImageViewer_VM.DrawDone += DrawDone_Callback;

			CenterPoint.X = 66000;//p_ImageViewer_VM.p_ImageData.p_Size.X / 2;
			CenterPoint.Y = 41000;// p_ImageViewer_VM.p_ImageData.p_Size.Y / 2;

			WorkEventManager.PositionDone += PositionDone_Callback;
			WorkEventManager.InspectionDone += SurfaceInspDone_Callback;
			WorkEventManager.ProcessDefectDone += ProcessDefectDone_Callback;
			WorkEventManager.ProcessDefectWaferDone += WorkEventManager_ProcessDefectWaferDone;
			InspectionManager_AOP.PatternInspectionDone += InspectionManager_AOP_PatternInspectionDone;


			SurfaceSize = 5;
			SurfaceGV = 60;
			InspectionOffsetX_Left = 900;
			InspectionOffsetX_Right = 900;
			InspectionOffsetY = 900;
			OutmapX = 40;
			OutmapY = 40;

			EdgeDrawMode = false;
		}

		private void InspectionManager_AOP_PatternInspectionDone()
		{
			ResultDataTable = null;
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
			MessageBox.Show("Inspection Done");

		}

		private void WorkEventManager_ProcessDefectWaferDone(object sender, ProcessDefectWaferDoneEventArgs e)
		{
			
		}

		private void ProcessDefectDone_Callback(object obj, ProcessDefectDoneEventArgs e)
		{
			Workplace workplace = obj as Workplace;

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
			{
				//UpdateDataGrid();
			}));
		}

		private void SurfaceInspDone_Callback(object obj, InspectionDoneEventArgs e)
		{
			Workplace workplace = obj as Workplace;
			//List<string> textList = new List<string>();
			//List<CRect> rectList = new List<CRect>();

			if (workplace.DefectList.Count > 0)
			{
				Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
				{
					m_Setup.PatternInspectionManager.AddDefect(workplace.DefectList);
				}));
			}
		}

		object lockObj = new object();
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
			p_ImageViewer_VM.DrawRect(ptOldStart, ptOldEnd, RecipeFrontside_Viewer_ViewModel.ColorType.MasterFeature);
			p_ImageViewer_VM.DrawRect(ptNewStart, ptNewEnd, bSuccess ? RecipeFrontside_Viewer_ViewModel.ColorType.FeatureMatching : RecipeFrontside_Viewer_ViewModel.ColorType.FeatureMatchingFail, text);
		}

		#region Property


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

		#region InspectionOffsetX_Left

		private int _InspectionOffsetX_Left;
		public int InspectionOffsetX_Left
		{
			get
			{
				return _InspectionOffsetX_Left;
			}
			set
			{
				SetProperty(ref _InspectionOffsetX_Left, value);
			}
		}
		#endregion

		#region InspectionOffsetX_Right

		private int _InspectionOffsetX_Right;
		public int InspectionOffsetX_Right
		{
			get
			{
				return _InspectionOffsetX_Right;
			}
			set
			{
				SetProperty(ref _InspectionOffsetX_Right, value);
			}
		}
		#endregion

		#region InspectionOffsetY

		private int _InspectionOffsetY;
		public int InspectionOffsetY
		{
			get
			{
				return _InspectionOffsetY;
			}
			set
			{
				SetProperty(ref _InspectionOffsetY, value);
			}
		}
		#endregion

		#region OutmapX

		private int _OutmapX;
		public int OutmapX
		{
			get
			{
				return _OutmapX;
			}
			set
			{
				SetProperty(ref _OutmapX, value);
			}
		}
		#endregion

		#region OutmapY

		private int _OutmapY;
		public int OutmapY
		{
			get
			{
				return _OutmapY;
			}
			set
			{
				SetProperty(ref _OutmapY, value);
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
						tempList.Clear();
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
		bool m_bEnablePatternDiscolor = false;
		public bool p_bEnablePatternDiscolor
		{
			get { return m_bEnablePatternDiscolor; }
			set { SetProperty(ref m_bEnablePatternDiscolor, value); }
		}
		bool m_bEnableAlignKeyInsp = false;
		public bool p_bEnableAlignKeyInsp
		{
			get { return m_bEnableAlignKeyInsp; }
			set { SetProperty(ref m_bEnableAlignKeyInsp, value); }
		}
		bool m_bEnableBarcodeInsp = false;
		public bool p_bEnableBarcodeInsp
		{
			get { return m_bEnableBarcodeInsp; }
			set { SetProperty(ref m_bEnableBarcodeInsp, value); }
		}
		bool m_bEnablePatternShift = false;
		public bool p_bEnablePatternShift
		{
			get { return m_bEnablePatternShift; }
			set { SetProperty(ref m_bEnablePatternShift, value); }
		}

		#region Align Key Parameter
		double m_dAlignKeyTemplateMatchingScore = 90;
		public double p_dAlignKeyTemplateMatchingScore
		{
			get { return m_dAlignKeyTemplateMatchingScore; }
			set { SetProperty(ref m_dAlignKeyTemplateMatchingScore, value); }
		}

		int m_nAlignKeyNGSpec_um = 30;
		public int p_nAlignKeyNGSpec_um
		{
			get { return m_nAlignKeyNGSpec_um; }
			set { SetProperty(ref m_nAlignKeyNGSpec_um, value); }
		}
		#endregion

		#region Pattern Shift & Rotation Parameter
		double m_dPatternShiftAndRotationTemplateMatchingScore = 90;
		public double p_dPatternShiftAndRotationTemplateMatchingScore
		{
			get { return m_dPatternShiftAndRotationTemplateMatchingScore; }
			set { SetProperty(ref m_dPatternShiftAndRotationTemplateMatchingScore, value); }
		}

		double m_dPatternShiftAndRotationShiftSpec = 0.5;
		public double p_dPatternShiftAndRotationShiftSpec
		{
			get { return m_dPatternShiftAndRotationShiftSpec; }
			set { SetProperty(ref m_dPatternShiftAndRotationShiftSpec, value); }
		}

		double m_dPatternShiftAndRotationRotationSpec = 0.5;
		public double p_dPatternShiftAndRotationRotationSpec
		{
			get { return m_dPatternShiftAndRotationRotationSpec; }
			set { SetProperty(ref m_dPatternShiftAndRotationRotationSpec, value); }
		}
		#endregion

		#region Barcode Inspection Parameter
		int m_nBarcodeThreshold = 70;
		public int p_nBarcodeThreshold
		{
			get { return m_nBarcodeThreshold; }
			set { SetProperty(ref m_nBarcodeThreshold, value); }
		}
		#endregion

		#endregion

		#region RootViewer
		private RecipeFrontside_Viewer_ViewModel m_ImageViewer_VM;
		public RecipeFrontside_Viewer_ViewModel p_ImageViewer_VM
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
		#endregion

		#region Method
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
		private void SetData(DataRowView selectedDataTable, ImageType type)
		{
			int idx = Convert.ToInt32(selectedDataTable["m_nDefectIndex"]);

			if (System.IO.Directory.Exists(System.IO.Path.Combine(App.AOPImageRootPath)))
			{
				string currentInspection = RootTools.Database.DatabaseManager.Instance.InspectionID;
				string imagePath = System.IO.Path.Combine(App.AOPImageRootPath, currentInspection, idx.ToString() + ".bmp");
				if (System.IO.File.Exists(imagePath))
				{
					System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(imagePath);
					SelectedTDIImage = ConvertImage(bmp);
					bmp.Dispose();
				}
			}
		}
		List<TRect> tempList = new List<TRect>();
		private void saveEdgeBox()
		{
			if (m_ImageViewer_VM.TRectList.Count == 6)
			{
				tempList = new List<TRect>(m_ImageViewer_VM.TRectList);
			}
		}

		CRect searchArea(int x_left_margin, int y_top_margin, int x_right_margin, int y_bot_margin)
		{
			// variable
			List<Rect> arcROIs = new List<Rect>();
			List<DPoint> aptEdges = new List<DPoint>();
			RecipeFrontside_Viewer_ViewModel ivvm = m_ImageViewer_VM;
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

			m_ImageViewer_VM.DrawRect(new CPoint(ptLeft1.X - 10, ptLeft1.Y - 10), new CPoint(ptLeft1.X + 10, ptLeft1.Y + 10), RecipeFrontside_Viewer_ViewModel.ColorType.Defect);
			m_ImageViewer_VM.DrawRect(new CPoint(ptLeft2.X - 10, ptLeft2.Y - 10), new CPoint(ptLeft2.X + 10, ptLeft2.Y + 10), RecipeFrontside_Viewer_ViewModel.ColorType.Defect);
			m_ImageViewer_VM.DrawRect(new CPoint(ptBottom.X - 10, ptBottom.Y - 10), new CPoint(ptBottom.X + 10, ptBottom.Y + 10), RecipeFrontside_Viewer_ViewModel.ColorType.Defect);
			m_ImageViewer_VM.DrawRect(new CPoint(ptRight1.X - 10, ptRight1.Y - 10), new CPoint(ptRight1.X + 10, ptRight1.Y + 10), RecipeFrontside_Viewer_ViewModel.ColorType.Defect);
			m_ImageViewer_VM.DrawRect(new CPoint(ptRight2.X - 10, ptRight2.Y - 10), new CPoint(ptRight2.X + 10, ptRight2.Y + 10), RecipeFrontside_Viewer_ViewModel.ColorType.Defect);
			m_ImageViewer_VM.DrawRect(new CPoint(ptTop.X - 100, ptTop.Y - 100), new CPoint(ptTop.X + 100, ptTop.Y + 100), RecipeFrontside_Viewer_ViewModel.ColorType.Defect);

			m_ImageViewer_VM.DrawRect(new CPoint(ptLT.X, ptLT.Y), new CPoint(ptRB.X, ptRB.Y), RecipeFrontside_Viewer_ViewModel.ColorType.ChipFeature);

			return new CRect(new Point(ptLT.X + x_left_margin, ptLT.Y + y_top_margin), new Point(ptRB.X - x_right_margin, ptRB.Y - y_top_margin));
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
					var area = searchArea(InspectionOffsetX_Left, InspectionOffsetY, _InspectionOffsetX_Right, InspectionOffsetY);


					string sDefectimagePath = @"D:\DefectImage";
					string sInspectionID = RootTools.Database.DatabaseManager.Instance.GetInspectionID();
					string outputSize = string.Format("Left:{0} Top:{1} Right:{2} Bottom:{3}", area.Left,area.Top,area.Right,area.Bottom);
					if(!System.IO.Directory.Exists(System.IO.Path.Combine(sDefectimagePath, sInspectionID)))
					{
						System.IO.Directory.CreateDirectory(System.IO.Path.Combine(sDefectimagePath, sInspectionID));
					}
					System.IO.File.WriteAllText(System.IO.Path.Combine(sDefectimagePath, sInspectionID, "TotalSize.txt"), outputSize);
					//그다음 이미지 축소 저장
					Image<Gray, byte> mapImage = new Image<Gray, byte>(memW/ DownSample, memH/ DownSample, memW/ DownSample, (IntPtr)pImg);
					mapImage.Save(System.IO.Path.Combine(sDefectimagePath, sInspectionID, "mapImage.bmp"));

					// Param Down Scale
					centX /= DownSample; centY /= DownSample;
					outRadius /= DownSample;
					memW /= DownSample; memH /= DownSample;

					int outmap_x = OutmapX;
					int outmap_y = OutmapY;
					
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
						&outmap_x,
						&outmap_y,
						memW, memH,
						1,
						isIncludeMode
						);
						OutmapX = outmap_x;
						OutmapY = outmap_y;
					}

				//// Param Up Scale
				centX *= DownSample; centY *= DownSample;
				outRadius *= DownSample;
				outOriginX *= DownSample; outOriginY *= DownSample;
				outChipSzX *= DownSample; outChipSzY *= DownSample;

				// Save Recipe
				SetRecipeMapData(mapData, (int)OutmapX, (int)OutmapY, (int)outOriginX, (int)outOriginY, (int)outChipSzX, (int)outChipSzY);

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
		private void DrawMapData(int[] mapData, int mapX, int mapY, int OriginX, int OriginY, int ChipSzX, int ChipSzY)
		{
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
			m_Setup.PatternInspectionManager.AddRect(rectList, null, new Pen(Brushes.Green, 2));
		}
		private void startTestInsp()
		{
			ResultDataTable = null;
			ResultDataTable = new DataTable();
			SelectedDataTable = null;

			saveEdgeBox();

			p_ImageViewer_VM.Clear();
			m_Setup.PatternInspectionManager.ClearDefect();
			m_Setup.PatternInspectionManager.ResetWorkManager();
			m_Setup.PatternInspectionManager.InitInspectionInfo();

			_StartRecipeTeaching();

			var temp = m_Setup.PatternInspectionManager.Recipe.GetRecipe<BacksideRecipe>();
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

				m_Setup.PatternInspectionManager.SetColorSharedBuffer(p_ImageViewer_VM.p_ImageData.GetPtr(0), p_ImageViewer_VM.p_ImageData.GetPtr(1), p_ImageViewer_VM.p_ImageData.GetPtr(2));
			}
			else
			{
				SharedBuf = p_ImageViewer_VM.p_ImageData.GetPtr();
				m_Setup.PatternInspectionManager.SharedBufferR_Gray = SharedBuf;
			}

			m_Setup.PatternInspectionManager.SharedBufferByteCnt = p_ImageViewer_VM.p_ImageData.p_nByte;


			m_Setup.PatternInspectionManager.InspectionStart();
		}
		#endregion

		#region RelayCommand
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
					MainVision.Run_Grab grab = (MainVision.Run_Grab)mainVision.CloneModuleRun("Run Grab");
					mainVision.StartRun(grab);
				});
			}
		}

		public ICommand btnInspection
		{
			get
			{
				return new RelayCommand(() =>
				{
					MainVision mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;
					if (p_bEnableAlignKeyInsp)
					{
						MainVision.Run_AlignKeyInspection alignKeyInspection = (MainVision.Run_AlignKeyInspection)mainVision.CloneModuleRun("AlignKeyInspection");
						alignKeyInspection.m_dMatchScore = p_dAlignKeyTemplateMatchingScore / 100;
						alignKeyInspection.m_nNGSpec_um = p_nAlignKeyNGSpec_um;
						mainVision.StartRun(alignKeyInspection);
					}

					if (p_bEnablePatternShift)
					{
						MainVision.Run_PatternShiftAndRotation patternShiftAndRotation = (MainVision.Run_PatternShiftAndRotation)mainVision.CloneModuleRun("PatternShiftAndRotation");
						patternShiftAndRotation.m_dMatchScore = p_dPatternShiftAndRotationTemplateMatchingScore / 100;
						patternShiftAndRotation.m_dNGSpecDistance_um = p_dPatternShiftAndRotationShiftSpec;
						patternShiftAndRotation.m_dNGSpecDegree = p_dPatternShiftAndRotationRotationSpec;
						mainVision.StartRun(patternShiftAndRotation);
					}

					if (p_bEnableBarcodeInsp)
					{
						MainVision.Run_BarcodeInspection barcodeInspection = (MainVision.Run_BarcodeInspection)mainVision.CloneModuleRun("BarcodeInspection");
						barcodeInspection.m_nThreshold = p_nBarcodeThreshold;
						mainVision.StartRun(barcodeInspection);
					}
					if (p_bEnablePatternDiscolor)
					{
						startTestInsp();
					}
				});
			}
		}
		#endregion

		List<CPoint> PolygonPt = new List<CPoint>();
		public ObservableCollection<TShape> Shapes = new ObservableCollection<TShape>();
		private ObservableCollection<UIElement> m_UIElements = new ObservableCollection<UIElement>();


		TShape rectInfo;

		public void DrawRectDefect(List<CRect> rectList, List<String> text, bool reDraw = false)
		{
			if (reDraw)
				p_ImageViewer_VM.Clear();

			p_ImageViewer_VM.DrawRect(rectList, RecipeFrontside_Viewer_ViewModel.ColorType.Defect, text);
		}

		private void DrawDone_Callback(CPoint leftTop, CPoint rightBottom)
		{
			if (!EdgeDrawMode)
			{
				p_ImageViewer_VM.Clear();
				this.m_ImageViewer_VM.DrawRect(leftTop, rightBottom, RecipeFrontside_Viewer_ViewModel.ColorType.Defect);
			}
			else
			{
				//edge box draw mode. 최대개수는 6개로 고정한다
				this.m_ImageViewer_VM.DrawRect(leftTop, rightBottom, RecipeFrontside_Viewer_ViewModel.ColorType.FeatureMatching);

				if (this.m_ImageViewer_VM.Shapes.Count > 7)
				{
					m_ImageViewer_VM.Shapes.RemoveAt(0);
					m_ImageViewer_VM.p_DrawElement.RemoveAt(0);
				}
			}
			m_Setup.PatternInspectionManager.RefreshDefect();

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
	}
}
