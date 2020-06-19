using ATI;
using Emgu.CV;
using Root_Vega.Module;
using RootTools;
using RootTools.Inspects;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DPoint = System.Drawing.Point;
using MBrushes = System.Windows.Media.Brushes;

namespace Root_Vega
{
	class _2_7_EdgeBoxViewModel : ObservableObject
	{
		enum eEdgeFindDirection { LEFT, TOP, RIGHT, BOTTOM };
		enum eBrightSide { LEFT, TOP, RIGHT, BOTTOM };

		bool m_bUseB2D = true;
		bool bUsingInspection;

		int tempImageWidth = 640;
		int tempImageHeight = 480;

		public bool p_bUseB2D
		{
			get { return m_bUseB2D; }
			set { SetProperty(ref m_bUseB2D, value); }
		}

		bool m_bUseAutoThreshold = true;
		public bool p_bUseAutoThreshold
		{
			get { return m_bUseAutoThreshold; }
			set { SetProperty(ref m_bUseAutoThreshold, value); }
		}

		int m_nThreshold = 40;
		public int p_nThreshold
		{
			get { return m_nThreshold; }
			set { SetProperty(ref m_nThreshold, value); }
		}

		protected Dispatcher _dispatcher;
		Vega_Engineer m_Engineer;
		DialogService m_DialogService;
		MemoryTool m_MemoryModule;
		List<string> m_astrMem = new List<String> { "Top", "Left", "Right", "Bottom" };
		public List<DrawHistoryWorker> m_DrawHistoryWorker_List = new List<DrawHistoryWorker>();
		private List<SimpleShapeDrawerVM> m_SimpleShapeDrawer_List = new List<SimpleShapeDrawerVM>();
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

		#region SelectedROI
		Roi _SelectedROI;
		public Roi SelectedROI
		{
			get { return _SelectedROI; }
			set
			{
				if (value != null)
				{
					for (int i = 0; i < 4; i++)
					{
						if (p_SimpleShapeDrawer_List[i] == null) continue;
						p_SimpleShapeDrawer_List[i].m_ListRect.Clear();
						var targetList = value.EdgeBox.EdgeList.Where(x => x.SavePoint == i).ToList();
						foreach (EdgeElement item in targetList)
						{
							var temp = new UIElementInfo(new Point(item.Rect.Left, item.Rect.Top), new Point(item.Rect.Right, item.Rect.Bottom));

							System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
							rect.Width = item.Rect.Width;
							rect.Height = item.Rect.Height;
							System.Windows.Controls.Canvas.SetLeft(rect, item.Rect.Left);
							System.Windows.Controls.Canvas.SetTop(rect, item.Rect.Top);
							rect.StrokeThickness = 2;
							rect.Stroke = MBrushes.Red;

							p_SimpleShapeDrawer_List[i].m_ListShape.Add(rect);
							p_SimpleShapeDrawer_List[i].m_Element.Add(rect);
							p_SimpleShapeDrawer_List[i].m_ListRect.Add(temp);
						}
						p_ImageViewer_List[i].SetRoiRect();
					}
				}

				SetProperty(ref _SelectedROI, value);
			}
		}
		#endregion


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
			lock(VSDataDT)
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
					int targetIdx = InspectionManager.GetInspectionTarget(item.nClassifyCode) - InspectionTarget.SideInspection - 1;
					//System.Diagnostics.Debug.WriteLine(string.Format("GetInspectionTarget() - targetIdx : {0}", targetIdx));

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
		}

		public _2_7_EdgeBoxViewModel(Vega_Engineer engineer, IDialogService dialogService)
		{
			_dispatcher = Dispatcher.CurrentDispatcher;
			m_Engineer = engineer;
			m_DialogService = (DialogService)dialogService;
			Init(dialogService);

			m_Engineer.m_InspManager.AddDefect += M_InspManager_AddDefect;
			bUsingInspection = false;
		}

		public System.Windows.Media.Imaging.BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
		{
			var bitmapData = bitmap.LockBits(
				new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
				System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

			var bitmapSource = System.Windows.Media.Imaging.BitmapSource.Create(
				bitmapData.Width, bitmapData.Height,
				bitmap.HorizontalResolution, bitmap.VerticalResolution,
				System.Windows.Media.PixelFormats.Gray8, null,
				bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

			bitmap.UnlockBits(bitmapData);
			return bitmapSource;
		}
		void Init(IDialogService dialogService)
		{
			m_MemoryModule = m_Engineer.ClassMemoryTool();
			if (m_MemoryModule != null)
			{
				for (int i = 0; i < 4; i++)
				{
					p_ImageViewer_List.Add(new ImageViewer_ViewModel(new ImageData(m_MemoryModule.GetMemory("SideVision.Memory", "SideVision", m_astrMem[i])), dialogService)); //!! m_Image 는 추후 각 part에 맞는 이미지가 들어가게 수정.
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
				//p_InformationDrawer[0] = new InformationDrawer(p_ImageViewer);
			}
			m_Engineer.m_recipe.LoadComplete += () =>
			{
				p_SideRoiList = new ObservableCollection<Roi>(m_Engineer.m_recipe.RecipeData.RoiList.Where(x => x.RoiType == Roi.Item.ReticleSide));
			};


			return;
		}

		private List<ImageViewer_ViewModel> m_ImageViewer_List = new List<ImageViewer_ViewModel>();
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

		private ImageViewer_ViewModel m_ImageViewer_Bottom;
		private string inspDefaultDir;
		private string inspFileName;
		SqliteDataDB VSDBManager;
		int currentDefectIdx;
		System.Data.DataTable VSDataInfoDT;
		System.Data.DataTable VSDataDT;

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
		public void _saveRcp()
		{
			if (SelectedROI == null)
			{
				//ERROR가 뜨면서 저장 방지
				return;
			}
			//여기서 그려진 모든 rect목록을 현재 엔지니어가 들고있는 레시피에 반영한다
			for (int i = 0; i < 4; i++)
			{
				if (p_SimpleShapeDrawer_List[i] == null) continue;
				for (int j = 0; j < 6; j++)
				{
					if (p_SimpleShapeDrawer_List[i].m_ListRect.Count < 6) break;
					SelectedROI.EdgeBox.EdgeList.Add(new EdgeElement(i, new CRect(p_SimpleShapeDrawer_List[i].m_ListRect[j].StartPos, p_SimpleShapeDrawer_List[i].m_ListRect[j].EndPos)));
				}
			}

			var target = System.IO.Path.Combine(System.IO.Path.Combine(@"C:\VEGA\Recipe", m_Engineer.m_recipe.RecipeName));
			m_Engineer.m_recipe.Save(target);
		}
		public void _addRoi()
		{
			int roiCount = m_Engineer.m_recipe.RecipeData.RoiList.Where(x => x.RoiType == Roi.Item.ReticleSide).Count();
			string defaultName = string.Format("Side ROI #{0}", roiCount);

			Roi temp = new Roi(defaultName, Roi.Item.ReticleSide);
			m_Engineer.m_recipe.RecipeData.RoiList.Add(temp);

			p_SideRoiList = new ObservableCollection<Roi>(m_Engineer.m_recipe.RecipeData.RoiList.Where(x => x.RoiType == Roi.Item.ReticleSide));
		}
		void _InspectComplete()
		{
			if (!bUsingInspection)
			{
				return;
			}
			else
			{
				bUsingInspection = false;
			}
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

				 int targetIdx = InspectionManager.GetInspectionTarget(Convert.ToInt32(row["DCode"])) - InspectionTarget.SideInspection - 1;

				 switch (targetIdx)
				 {
					 case 0:
						 encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(BitmapToBitmapSource(p_ImageViewer_Top.p_ImageData.GetRectImage(ImageSizeBlock))));
						 break;
					 case 1:
						 encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(BitmapToBitmapSource(p_ImageViewer_Left.p_ImageData.GetRectImage(ImageSizeBlock))));
						 break;
					 case 2:
						 encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(BitmapToBitmapSource(p_ImageViewer_Right.p_ImageData.GetRectImage(ImageSizeBlock))));
						 break;
					 case 3:
						 encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(BitmapToBitmapSource(p_ImageViewer_Bottom.p_ImageData.GetRectImage(ImageSizeBlock))));
						 break;
				 }
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
		void Inspect()
		{
			// variable
			List<Rect> arcROIs = new List<Rect>();
			List<DPoint> aptEdges = new List<DPoint>();
			ImageViewer_ViewModel ivvm = p_ImageViewer_Top;
			eEdgeFindDirection eTempDirection = eEdgeFindDirection.TOP;
			DPoint ptLeft1, ptLeft2, ptBottom, ptRight1, ptRight2, ptTop;
			DPoint ptLT, ptRT, ptLB, ptRB;
			System.Diagnostics.Debug.WriteLine("Start Insp");
			bUsingInspection = true;

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
			m_Engineer.m_InspManager.ClearInspection();

			// implement
			for (int i = 0; i < 4; i++)
			{
				if (p_SimpleShapeDrawer_List[i] == null) continue;
				arcROIs.Clear();
				aptEdges.Clear();
				for (int j = 0; j < 6; j++)
				{
					if (p_SimpleShapeDrawer_List[i].m_ListRect.Count < 6) break;
					arcROIs.Add(new Rect(p_SimpleShapeDrawer_List[i].m_ListRect[j].StartPos, p_SimpleShapeDrawer_List[i].m_ListRect[j].EndPos));
				}
				if (arcROIs.Count < 6) continue;
				switch (i)
				{
					case 1:
						ivvm = p_ImageViewer_Left;
						break;
					case 2:
						ivvm = p_ImageViewer_Right;
						break;
					case 3:
						ivvm = p_ImageViewer_Bottom;
						break;
					case 0:
					default:
						ivvm = p_ImageViewer_Top;
						break;
				}
				for (int j = 0; j < arcROIs.Count; j++)
				{
					eTempDirection = GetDirection(ivvm.p_ImageData, arcROIs[j]);
					aptEdges.Add(GetEdge(ivvm.p_ImageData, arcROIs[j], eTempDirection));
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

				if (true)//Merge를 위한 동작 방지 코드
				{
					//TODO : 여기서 생성되는 사각형 정보를 engineer한테 넘겨서 검사를 진행할 수 있도록 만들어야 함

					CRect inspArea = new CRect(ptLT.X, ptLT.Y, ptRB.X, ptRB.Y);
					List<CRect> DrawRectList = new List<CRect>();

					//TODO : 일단 테스트로 강제로 서페이스 검사 파라메터를 생성한다. 추후 설정창 필요함!
					SurfaceParamData paramTemp = new SurfaceParamData();
					SelectedROI.Surface.ParameterList.Add(paramTemp);

					foreach (var param in SelectedROI.Surface.ParameterList)
					{
						InspectionType type = InspectionType.AbsoluteSurface;

						if (!param.UseAbsoluteInspection)
						{
							type = InspectionType.RelativeSurface;
						}
						int nDefectCode = InspectionManager.MakeDefectCode((InspectionTarget)(10 + i), type, 0);

						DrawRectList.AddRange(m_Engineer.m_InspManager.CreateInspArea("SideVision.Memory", m_Engineer.GetMemory("SideVision.Memory", "SideVision", m_astrMem[i]).GetMBOffset(),
							m_Engineer.GetMemory("SideVision.Memory", "SideVision", m_astrMem[i]).p_sz.X,
							m_Engineer.GetMemory("SideVision.Memory", "SideVision", m_astrMem[i]).p_sz.Y,
							inspArea, 500, param, nDefectCode, m_Engineer.m_recipe.RecipeData.UseDefectMerge, m_Engineer.m_recipe.RecipeData.MergeDistance));
					}
				}

				DrawLine(ptLT, ptLB, MBrushes.Lime, i);
				DrawLine(ptRB, ptRT, MBrushes.Lime, i);
				DrawLine(ptLT, ptRT, MBrushes.Lime, i);
				DrawLine(ptLB, ptRB, MBrushes.Lime, i);

				DrawCross(ptLeft1, MBrushes.Yellow, i);
				DrawCross(ptLeft2, MBrushes.Yellow, i);
				DrawCross(ptBottom, MBrushes.Yellow, i);
				DrawCross(ptRight1, MBrushes.Yellow, i);
				DrawCross(ptRight2, MBrushes.Yellow, i);
				DrawCross(ptTop, MBrushes.Yellow, i);

				p_ImageViewer_List[i].SetRoiRect();
			}
			m_Engineer.m_InspManager.StartInspection();
		}
		void DrawCross(System.Drawing.Point pt, System.Windows.Media.SolidColorBrush brsColor, int nTLRB)
		{
			DPoint ptLT = new DPoint(pt.X - 40, pt.Y - 40);
			DPoint ptRB = new DPoint(pt.X + 40, pt.Y + 40);
			DPoint ptLB = new DPoint(pt.X - 40, pt.Y + 40);
			DPoint ptRT = new DPoint(pt.X + 40, pt.Y - 40);

			DrawLine(ptLT, ptRB, brsColor, nTLRB);
			DrawLine(ptLB, ptRT, brsColor, nTLRB);
		}
		void DrawLine(System.Drawing.Point pt1, System.Drawing.Point pt2, System.Windows.Media.SolidColorBrush brsColor, int nTLRB)
		{
			// variable
			ImageViewer_ViewModel ivvm;

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

			switch (nTLRB)
			{
				case 0: ivvm = m_ImageViewer_Top; break;
				case 1: ivvm = m_ImageViewer_Left; break;
				case 2: ivvm = m_ImageViewer_Right; break;
				case 3: ivvm = m_ImageViewer_Bottom; break;
				default: ivvm = m_ImageViewer_Top; break;
			}

			ivvm.SelectedTool.m_ListShape.Add(myLine);
			UIElementInfo uei = new UIElementInfo(new System.Windows.Point(myLine.X1, myLine.Y1), new System.Windows.Point(myLine.X2, myLine.Y2));
			ivvm.SelectedTool.m_ListRect.Add(uei);
			ivvm.SelectedTool.m_Element.Add(myLine);
		}
		unsafe DPoint GetEdge(ImageData img, Rect rcROI, eEdgeFindDirection eDirection)
		{
			// variable
			int nSum = 0;
			double dAverage = 0.0;
			int nEdgeY = 0;
			int nEdgeX = 0;

			// implement

			if (p_bUseAutoThreshold == true)
			{
				p_nThreshold = GetThresholdAverage(img, rcROI, eDirection);
			}

			switch (eDirection)
			{
				case eEdgeFindDirection.TOP:
					for (int i = 0; i < rcROI.Height; i++)
					{
						byte* bp;
						if (p_bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
						else bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
						for (int j = 0; j < rcROI.Width; j++)
						{
							nSum += *bp;
							bp++;
						}
						dAverage = nSum / rcROI.Width;
						if (p_bUseB2D == true)
						{
							if (dAverage < p_nThreshold)
							{
								nEdgeY = (int)rcROI.Bottom - i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}
						else
						{
							if (dAverage > p_nThreshold)
							{
								nEdgeY = (int)rcROI.Top + i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}
						nSum = 0;
					}
					break;
				case eEdgeFindDirection.LEFT:
					for (int i = 0; i < rcROI.Width; i++)
					{
						byte* bp;
						if (p_bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i));
						else bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
						for (int j = 0; j < rcROI.Height; j++)
						{
							nSum += *bp;
							bp += img.p_Stride;
						}
						dAverage = nSum / rcROI.Height;
						if (p_bUseB2D == true)
						{
							if (dAverage < p_nThreshold)
							{
								nEdgeX = (int)rcROI.Right - i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						else
						{
							if (dAverage > p_nThreshold)
							{
								nEdgeX = (int)rcROI.Left + i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						nSum = 0;
					}
					break;
				case eEdgeFindDirection.RIGHT:
					for (int i = 0; i < rcROI.Width; i++)
					{
						byte* bp;
						if (p_bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
						else bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i));
						for (int j = 0; j < rcROI.Height; j++)
						{
							nSum += *bp;
							bp += img.p_Stride;
						}
						dAverage = nSum / rcROI.Height;
						if (p_bUseB2D == true)
						{
							if (dAverage < p_nThreshold)
							{
								nEdgeX = (int)rcROI.Left + i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						else
						{
							if (dAverage > p_nThreshold)
							{
								nEdgeX = (int)rcROI.Right - i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						nSum = 0;
					}
					break;
				case eEdgeFindDirection.BOTTOM:
					for (int i = 0; i < rcROI.Height; i++)
					{
						byte* bp;
						if (p_bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
						else bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
						for (int j = 0; j < rcROI.Width; j++)
						{
							nSum += *bp;
							bp++;
						}
						dAverage = nSum / rcROI.Width;
						if (p_bUseB2D == true)
						{
							if (dAverage < p_nThreshold)
							{
								nEdgeY = (int)rcROI.Top + i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}
						else
						{
							if (dAverage > p_nThreshold)
							{
								nEdgeY = (int)rcROI.Bottom - i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}

						nSum = 0;
					}
					break;
			}

			return new System.Drawing.Point(nEdgeX, nEdgeY);
		}
		unsafe int GetThresholdAverage(ImageData img, Rect rcROI, eEdgeFindDirection eDirection)
		{
			// variable
			int nSum = 0;
			int nThreshold = 40;

			// implement

			if (eDirection == eEdgeFindDirection.TOP || eDirection == eEdgeFindDirection.BOTTOM)
			{
				double dRatio = rcROI.Height * 0.1;
				double dAverage1 = 0.0;
				double dAverage2 = 0.0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
					for (int j = 0; j < rcROI.Width; j++)
					{
						nSum += *bp;
						bp++;
					}
				}
				dAverage1 = nSum / (rcROI.Width * (int)dRatio);
				nSum = 0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
					for (int j = 0; j < rcROI.Width; j++)
					{
						nSum += *bp;
						bp++;
					}
				}
				dAverage2 = nSum / (rcROI.Width * (int)dRatio);
				nSum = 0;
				////////////////////////////////////////////////
				nThreshold = (int)(dAverage1 + dAverage2) / 2;
			}
			else
			{
				double dRatio = rcROI.Width * 0.1;
				double dAverage1 = 0.0;
				double dAverage2 = 0.0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i));
					for (int j = 0; j < rcROI.Height; j++)
					{
						nSum += *bp;
						bp += img.p_Stride;
					}
				}
				dAverage1 = nSum / (rcROI.Height * (int)dRatio);
				nSum = 0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
					for (int j = 0; j < rcROI.Height; j++)
					{
						nSum += *bp;
						bp += img.p_Stride;
					}
				}
				dAverage2 = nSum / (rcROI.Height * (int)dRatio);
				nSum = 0;
				////////////////////////////////////////////////
				nThreshold = (int)(dAverage1 + dAverage2) / 2;
			}

			return nThreshold;
		}
		unsafe eEdgeFindDirection GetDirection(ImageData img, Rect rcROI)
		{
			// variable
			double dRatio = 0.0;
			int nSum = 0;
			double dAverageTemp = 0.0;
			Dictionary<eBrightSide, double> dic = new Dictionary<eBrightSide, double>();

			// implement
			// Left
			dRatio = rcROI.Width * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
				for (int j = 0; j < rcROI.Height; j++)
				{
					nSum += *bp;
					bp += img.p_Stride;
				}
			}
			dAverageTemp = nSum / (rcROI.Height * (int)dRatio);
			dic.Add(eBrightSide.LEFT, dAverageTemp);
			nSum = 0;

			// Top
			dRatio = rcROI.Height * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
				for (int j = 0; j < rcROI.Width; j++)
				{
					nSum += *bp;
					bp++;
				}
			}
			dAverageTemp = nSum / (rcROI.Width * (int)dRatio);
			dic.Add(eBrightSide.TOP, dAverageTemp);
			nSum = 0;

			// Right
			dRatio = rcROI.Width * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i).ToPointer());
				for (int j = 0; j < rcROI.Height; j++)
				{
					nSum += *bp;
					bp += img.p_Stride;
				}
			}
			dAverageTemp = nSum / (rcROI.Height * (int)dRatio);
			dic.Add(eBrightSide.RIGHT, dAverageTemp);
			nSum = 0;

			// Bottom
			dRatio = rcROI.Height * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
				for (int j = 0; j < rcROI.Width; j++)
				{
					nSum += *bp;
					bp++;
				}
			}
			dAverageTemp = nSum / (rcROI.Width * (int)dRatio);
			dic.Add(eBrightSide.BOTTOM, dAverageTemp);
			nSum = 0;

			var maxKey = dic.Keys.Max();
			var maxValue = dic.Values.Max();
			// Value값이 가장 큰 Key값 찾기
			var keyOfMaxValue = dic.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

			if (keyOfMaxValue == eBrightSide.TOP) return eEdgeFindDirection.BOTTOM;
			else if (keyOfMaxValue == eBrightSide.BOTTOM) return eEdgeFindDirection.TOP;
			else if (keyOfMaxValue == eBrightSide.LEFT) return eEdgeFindDirection.RIGHT;
			else return eEdgeFindDirection.LEFT;
		}

		#region RelayCommand
		public RelayCommand CommandInspect
		{
			get
			{
				return new RelayCommand(Inspect);
			}
		}
		public RelayCommand CommandInspectComplete
		{
			get
			{
				return new RelayCommand(_InspectComplete);
			}
		}
		public RelayCommand CommandSave
		{
			get
			{
				return new RelayCommand(_saveRcp);
			}
		}
		public RelayCommand CommandAddRoi
		{
			get { return new RelayCommand(_addRoi); }
		}
		#endregion
	}
}
