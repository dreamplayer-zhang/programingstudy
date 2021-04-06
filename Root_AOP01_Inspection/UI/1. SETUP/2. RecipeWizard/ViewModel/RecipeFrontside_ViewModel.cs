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
using Root_AOP01_Inspection.Recipe;

namespace Root_AOP01_Inspection
{
    public class RecipeFrontside_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;
        public Setup_ViewModel p_Setup
        {
            get { return m_Setup; }
            set { SetProperty(ref m_Setup, value); }
        }
        AOP01_Engineer m_Engineer;
        MainVision m_mainVision;
        public Dispatcher currentDispatcher;
		//RecipeType_WaferMap mapInfo;

		public MainVision p_mainVision
        {
            get { return m_mainVision; }
            set { SetProperty(ref m_mainVision, value); }
        }
        public RecipeFrontside_ViewModel(Setup_ViewModel setup, Dispatcher dispatcher)
        {
            m_Setup = setup;
            currentDispatcher = dispatcher;
            m_Engineer = GlobalObjects.Instance.Get<AOP01_Engineer>();
            m_mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;

            p_ImageViewer_VM = new RecipeFrontside_Viewer_ViewModel(currentDispatcher);
            p_ImageViewer_VM.init(GlobalObjects.Instance.GetNamed<ImageData>(App.MainRegName), GlobalObjects.Instance.Get<DialogService>());

            p_ImageViewer_VM.DrawDone += DrawDone_Callback;
            //TODO : GlobalObjects에서 가져오도록 수정해야 함
            GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.MainInspMgRegName).InspectionDone += SurfaceInspDone_Callback;

            MainVision.Run_SurfaceInspection surfaceInspection = (MainVision.Run_SurfaceInspection)p_mainVision.CloneModuleRun(App.MainModuleName);
            BrightGV = surfaceInspection.BrightGV;
            SurfaceGV = surfaceInspection.SurfaceGV;
            SurfaceSize = surfaceInspection.SurfaceSize;

            InspectionOffsetX_Left = surfaceInspection.InspectionOffsetX_Left;
            InspectionOffsetX_Right = surfaceInspection.InspectionOffsetX_Right;
            InspectionOffsetY = surfaceInspection.InspectionOffsetY;
            BlockSizeWidth = surfaceInspection.BlockSizeWidth;
            BlockSizeHeight = surfaceInspection.BlockSizeHeight;
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
                    //m_Setup.PellInspectionManager.AddDefect(workplace.DefectList);//이벤트 추가 필요
                    GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.MainInspMgRegName).AddDefect(workplace.DefectList);
                    GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.MainInspMgRegName).RefreshDefect();
                }));
            }
        }

		#region Property
		bool m_bEnableAlignKeyInsp = false;
        
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

        #region Pelicle Shift & Rotation Parameter
        double m_dPellicleShiftAndRotationShiftSpec = 0.5;
        public double p_dPellicleShiftAndRotationShiftSpec
        {
            get { return m_dPellicleShiftAndRotationShiftSpec; }
            set { SetProperty(ref m_dPellicleShiftAndRotationShiftSpec, value); }
        }

        double m_dPellicleShiftAndRotationRotationSpec = 0.5;
        public double p_dPellicleShiftAndRotationRotationSpec
        {
            get { return m_dPellicleShiftAndRotationRotationSpec; }
            set { SetProperty(ref m_dPellicleShiftAndRotationRotationSpec, value); }
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
                        //tempList.Clear();
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

        #region BlockSizeWidth

        private int _BlockSizeWidth;
        public int BlockSizeWidth
        {
            get
            {
                return _BlockSizeWidth;
            }
            set
            {
                SetProperty(ref _BlockSizeWidth, value);
            }
        }
        #endregion

        #region BlockSizeHeight

        private int _BlockSizeHeight;
        public int BlockSizeHeight
        {
            get
            {
                return _BlockSizeHeight;
            }
            set
            {
                SetProperty(ref _BlockSizeHeight, value);
            }
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

		//private void DrawMapData(int[] mapData, int mapX, int mapY, int OriginX, int OriginY, int ChipSzX, int ChipSzY)
		//{
		//	// Map Display
		//	List<RootTools.Database.Defect> rectList = new List<RootTools.Database.Defect>();
		//	int offsetY = 0;
		//	bool isOrigin = true;

		//	for (int x = 0; x < mapX; x++)
		//		for (int y = 0; y < mapY; y++)
		//			if (mapData[y * mapX + x] == 1)
		//			{
		//				if (isOrigin)
		//				{
		//					offsetY = OriginY - (y + 1) * ChipSzY;
		//					mapInfo.MasterDieX = x;
		//					mapInfo.MasterDieY = y;
		//					isOrigin = false;
		//				}
		//				var data = new RootTools.Database.Defect();

		//				var left = OriginX + x * ChipSzX;
		//				var top = offsetY + y * ChipSzY;
		//				var right = OriginX + (x + 1) * ChipSzX;
		//				var bot = offsetY + (y + 1) * ChipSzY;

		//				var width = right - left;
		//				var height = bot - top;
		//				left = (int)(left - width / 2.0);
		//				top = (int)(top - height / 2.0);

		//				data.p_rtDefectBox = new Rect(left, top, width, height);
		//				rectList.Add(data);
		//			}


		//	//m_ImageViewer_VM.DrawRect(rectList, Recipe45D_ImageViewer_ViewModel.ColorType.MapData);
		//	GlobalObjects.Instance.Get<InspectionManager_Front>().AddRect(rectList, null, new Pen(Brushes.Green, 2));
		//}
		private void saveCurrentEdge()
		{
			//현재 ViewModel에 있는 edgebox를 저장한다.
			if (m_ImageViewer_VM.TRectList.Count == 6)
            {
                MainVision mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;
                mainVision.SetRectInfo(m_ImageViewer_VM.TRectList,App.MainModuleName);
            }
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
            GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.MainInspMgRegName).RefreshDefect();
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
                return new RelayCommand(() => {
                    MainVision mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;
                    MainVision.Run_Grab grab = (MainVision.Run_Grab)mainVision.CloneModuleRun("Run Grab");
                    mainVision.StartRun(grab);
                });
            }
        }
		public ICommand commandSaveEdgeBox
		{
			get
			{
				return new RelayCommand(saveCurrentEdge);
			}
		}

		public ICommand btnInspection
        {
            get
            {
                return new RelayCommand(() =>
                {
                    MainVision mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;

                    if (true)
                    {
                        MainVision.Run_AlignKeyInspection alignKeyInspection = (MainVision.Run_AlignKeyInspection)mainVision.CloneModuleRun("AlignKeyInspection");
                        alignKeyInspection.m_dMatchScore = p_dAlignKeyTemplateMatchingScore / 100;
                        mainVision.StartRun(alignKeyInspection);
                    }

                    if (true)
                    {
                        MainVision.Run_PatternShiftAndRotation patternShiftAndRotation = (MainVision.Run_PatternShiftAndRotation)mainVision.CloneModuleRun("PatternShiftAndRotation");
                        patternShiftAndRotation.m_dMatchScore = p_dPatternShiftAndRotationTemplateMatchingScore / 100;
                        mainVision.StartRun(patternShiftAndRotation);
                    }

                    if (true)
                    {
                        MainVision.Run_PellicleShiftAndRotation pellicleShiftAndRotation = (MainVision.Run_PellicleShiftAndRotation)mainVision.CloneModuleRun("PellicleShiftAndRotation");
                        mainVision.StartRun(pellicleShiftAndRotation);
                    }

                    if (true)
                    {
                        MainVision.Run_BarcodeInspection barcodeInspection = (MainVision.Run_BarcodeInspection)mainVision.CloneModuleRun("BarcodeInspection");
                        barcodeInspection.m_nThreshold = p_nBarcodeThreshold;
                        mainVision.StartRun(barcodeInspection);
					}
					if (true)
					{
                        //startTestInsp();
                        //RootTools.Database.DatabaseManager.Instance.ClearTableData("defect");

						ResultDataTable = null;
						ResultDataTable = new DataTable();
						SelectedDataTable = null;
                        MainVision.Run_SurfaceInspection surfaceInspection = (MainVision.Run_SurfaceInspection)mainVision.CloneModuleRun(App.MainModuleName);

                        GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.MainInspMgRegName).InitInspectionInfo();

                        //현재 ViewModel에 있는 edgebox를 저장한다.
                        if (m_ImageViewer_VM.TRectList.Count == 6)
                        {
                            //tempList = new List<TRect>(viewer.TRectList);
                            surfaceInspection.EdgeList = new List<TRect>(m_ImageViewer_VM.TRectList).ToArray();
                        }
                        p_ImageViewer_VM.Clear();

                        surfaceInspection.BrightGV = BrightGV;
                        surfaceInspection.SurfaceGV = SurfaceGV;
                        surfaceInspection.SurfaceSize = SurfaceSize;
                        mainVision.SetSurfaceParam(BrightGV, SurfaceGV, SurfaceSize, App.MainModuleName);

                        // surfaceInspection.InspectionOffsetX_Left = InspectionOffsetX_Left;
                        //surfaceInspection.InspectionOffsetX_Right = InspectionOffsetX_Right;
                        // surfaceInspection.InspectionOffsetY = InspectionOffsetY;
                        // surfaceInspection.BlockSizeWidth = BlockSizeWidth;
                        //surfaceInspection.BlockSizeHeight = BlockSizeHeight;
                        // surfaceInspection.UpdateTree();

                        mainVision.StartRun(surfaceInspection);


						MainVision.Run_SurfaceInspection pell = (MainVision.Run_SurfaceInspection)mainVision.CloneModuleRun(App.PellicleModuleName);

						pell.BrightGV = BrightGV;
						pell.SurfaceGV = SurfaceGV;
						pell.SurfaceSize = SurfaceSize;
                        mainVision.SetSurfaceParam(BrightGV, SurfaceGV, SurfaceSize, App.PellicleModuleName);

                        mainVision.StartRun(pell);




						MainVision.Run_SurfaceInspection left = (MainVision.Run_SurfaceInspection)mainVision.CloneModuleRun(App.SideLeftModuleName);

						left.BrightGV = BrightGV;
						left.SurfaceGV = SurfaceGV;
						left.SurfaceSize = SurfaceSize;
                        mainVision.SetSurfaceParam(BrightGV, SurfaceGV, SurfaceSize, App.SideLeftModuleName);

                        mainVision.StartRun(left);


						MainVision.Run_SurfaceInspection right = (MainVision.Run_SurfaceInspection)mainVision.CloneModuleRun(App.SideRightModuleName);

						right.BrightGV = BrightGV;
						right.SurfaceGV = SurfaceGV;
						right.SurfaceSize = SurfaceSize;
                        mainVision.SetSurfaceParam(BrightGV, SurfaceGV, SurfaceSize, App.SideRightModuleName);

                        mainVision.StartRun(right);


						MainVision.Run_SurfaceInspection bot = (MainVision.Run_SurfaceInspection)mainVision.CloneModuleRun(App.SideBotModuleName);

						bot.BrightGV = BrightGV;
						bot.SurfaceGV = SurfaceGV;
						bot.SurfaceSize = SurfaceSize;
                        mainVision.SetSurfaceParam(BrightGV, SurfaceGV, SurfaceSize, App.SideBotModuleName);

                        mainVision.StartRun(bot);


						MainVision.Run_SurfaceInspection top = (MainVision.Run_SurfaceInspection)mainVision.CloneModuleRun(App.SideTopModuleName);

						top.BrightGV = BrightGV;
						top.SurfaceGV = SurfaceGV;
						top.SurfaceSize = SurfaceSize;
                        mainVision.SetSurfaceParam(BrightGV, SurfaceGV, SurfaceSize, App.SideTopModuleName);

                        mainVision.StartRun(top);
					}
				});
            }
        }
        #endregion
    }
}