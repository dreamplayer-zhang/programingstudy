using Root_WIND2.Module;
using RootTools;
using RootTools.Database;
using RootTools.Module;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Data;
using RootTools_Vision.WorkManager3;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using RootTools_CLR;

namespace Root_WIND2.UI_User
{
    public class FrontsideOthers_ViewModel : ObservableObject, IPage
    {
        /// <summary>
        ///  정상 Seq. 외 (Recipe에 등록한 Spec과 다른...) 기타 기능을 테스트 하기 위함
        ///  Position은 모든 기능의 공통이니 해당클래스에서 포함시켜 둠
        ///  나머지 부가 기능에 대해서는 클래스를 추가하여 결과를 받아오는식으로 작성하면 됩니다.
        /// </summary>

        #region [member variables]
        private IntPtr _inspectionSharedBuffer;

        private PreCreateGolden _preCreateGolden;
        private PreCreateGoldenParameter _preCreateGoldenParameter;
        private FrontsideOthersParameterBase _selectedParam;

        private List<CPoint> _posData = new List<CPoint>();
        private List<CPoint> _validChip = new List<CPoint>();

        private byte[] _goldenImage = null;
        #endregion

        #region [ColorDefines]
        private class ImageViewerColorDefines
        {
            public static SolidColorBrush MasterPosition = Brushes.Magenta;
            public static SolidColorBrush MasterPostionMove = Brushes.Yellow;
            public static SolidColorBrush ChipPosition = Brushes.Blue;
            public static SolidColorBrush ChipPositionMove = Brushes.Yellow;
            public static SolidColorBrush PostionFail = Brushes.Red;
            public static SolidColorBrush Defect = Brushes.Red;
        }

        private class MapViewerColorDefines
        {
            public static SolidColorBrush NoChip = Brushes.LightGray;
            public static SolidColorBrush Normal = Brushes.DimGray;
            public static SolidColorBrush Snap = Brushes.LightSkyBlue;
            public static SolidColorBrush Position = Brushes.LightYellow;
            public static SolidColorBrush Inspection = Brushes.Gold;
            public static SolidColorBrush ProcessDefect = Brushes.YellowGreen;
            public static SolidColorBrush ProcessDefectWafer = Brushes.Green;

            public static SolidColorBrush GetWorkplaceStateColor(WORK_TYPE state)
            {
                switch (state)
                {
                    case WORK_TYPE.NONE:
                        return Normal;
                    case WORK_TYPE.SNAP:
                        return Snap;
                    case WORK_TYPE.ALIGNMENT:
                        return Position;
                    case WORK_TYPE.INSPECTION:
                        return Inspection;
                    case WORK_TYPE.DEFECTPROCESS:
                        return ProcessDefect;
                    case WORK_TYPE.DEFECTPROCESS_ALL:
                        return ProcessDefectWafer;
                    default:
                        return Normal;
                }
            }
        }

        #endregion

        #region [TestModeDefines & Setting]
        private void SetTestModeList()
        {
            _modeList.Add("Create Golden Image"); // selectedMode = GoldenImageMode
            _modeList.Add("Engineer Test1"); // seletedMode = EngineerTest1 
        }
        enum TestModeDefeine
        {
            GoldenImageMode = 0,
            EngineerTest1,
        };
        private void SetParamPage()
        {
            switch ((TestModeDefeine)_selectedMode)
            {
                case TestModeDefeine.GoldenImageMode:
                    p_selectedMethodItem = _preCreateGoldenParameter;
                    _selectedParam = _preCreateGoldenParameter;
                    break;
                // Test용
                case TestModeDefeine.EngineerTest1:
                    p_selectedMethodItem = new D2DParameter();
                    //_selectedParam = ...;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region [Properites]
        private FrontsideOthers_ImageViewer_ViewModel _imageViewerVM;
        public FrontsideOthers_ImageViewer_ViewModel ImageViewerVM
        {
            get => this._imageViewerVM;
        }

        private MapViewer_ViewModel _mapViewerVM;
        public MapViewer_ViewModel MapViewerVM
        {
            get => this._mapViewerVM;
            set
            {
                SetProperty<MapViewer_ViewModel>(ref this._mapViewerVM, value);
            }
        }

        private BitmapSource _goldenImgSource;
        public BitmapSource p_GoldenImgSource
        {
            get
            {
                return _goldenImgSource;
            }
            set
            {
                SetProperty(ref _goldenImgSource, value);
            }
        }

        private int _selectedMode = 0;
        public int SelectedMode
        {
            get
            {
                SetParamPage();
                return this._selectedMode;
            }
            set
            {
                SetProperty<int>(ref this._selectedMode, value);
            }
        }

        private List<string> _modeList = new List<string>();
        public List<string> ModeList
        {
            get
            {
                return _modeList;
            }
            set { }
        }

        private ParameterBase _selectedMethodItem;
        public ParameterBase p_selectedMethodItem
        {
            get
            {
                return _selectedMethodItem;
            }
            set
            {
                SetProperty(ref _selectedMethodItem, value);
            }
        }

        private bool _isEnableCreate = false;
        public bool IsEnabledCreate
        {
            get => this._isEnableCreate;
            set
            {
                SetProperty<bool>(ref this._isEnableCreate, value);
            }
        }

        private bool _isEnableSave = false;
        public bool IsEnabledSave
        {
            get => this._isEnableSave;
            set
            {
                SetProperty<bool>(ref this._isEnableSave, value);
            }
        }

        #endregion

        public FrontsideOthers_ViewModel()
        {
            if (GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr() == IntPtr.Zero && GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").m_eMode != ImageData.eMode.OtherPCMem)
                return;

            // Initialize ImageViewer
            this._imageViewerVM = new FrontsideOthers_ImageViewer_ViewModel();
            this._imageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());

            SetTestModeList();
            p_selectedMethodItem = null;

            _isEnableCreate = false;
            _isEnableSave = false;

            if (_posData.Count >= 1)
                this._isEnableCreate = true;

            // Initialize MapViewer
            this._mapViewerVM = new MapViewer_ViewModel();

            this._preCreateGolden = new PreCreateGolden();
            this._preCreateGoldenParameter = new PreCreateGoldenParameter();
            this._selectedParam = _preCreateGoldenParameter;
        }
        private void SelectedCellsChanged_Callback(object obj)
        {
            DataRowView row = (DataRowView)obj;
            if (row == null) return;

            System.Drawing.Rectangle m_View_Rect = new System.Drawing.Rectangle((int)(double)row["m_fAbsX"] - ImageViewerVM.p_View_Rect.Width / 2, (int)(double)row["m_fAbsY"] - this._imageViewerVM.p_View_Rect.Height / 2, this._imageViewerVM.p_View_Rect.Width, this._imageViewerVM.p_View_Rect.Height);
            ImageViewerVM.p_View_Rect = m_View_Rect;
            ImageViewerVM.SetImageSource();
            ImageViewerVM.UpdateImageViewer(); // replace RedrawShapes()
        }

        private string currentRecipe = "";
        public void LoadRecipe()
        {
            if (currentRecipe != GlobalObjects.Instance.Get<RecipeFront>().Name)
            {
                RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
                currentRecipe = GlobalObjects.Instance.Get<RecipeFront>().Name;
                this.MapViewerVM.CreateMap(waferMap.MapSizeX, waferMap.MapSizeY);
            }
            else
            {
                RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
                if (waferMap.MapSizeX != this.MapViewerVM.MapSizeX || waferMap.MapSizeY != this.MapViewerVM.MapSizeY)
                {
                    this.MapViewerVM.CreateMap(waferMap.MapSizeX, waferMap.MapSizeY);
                }
            }
            ResetMapColor();
        }

        public void ResetMapColor()
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
            for (int i = 0; i < waferMap.MapSizeY; i++)
            {
                for (int j = 0; j < waferMap.MapSizeX; j++)
                {
                    int index = j + i * waferMap.MapSizeX;
                    CHIP_TYPE type = (CHIP_TYPE)waferMap.Data[index];
                    switch (type)
                    {
                        case CHIP_TYPE.NO_CHIP:
                            this._mapViewerVM.SetChipColor(j, i, MapViewerColorDefines.NoChip);
                            break;
                        case CHIP_TYPE.NORMAL:
                            this._mapViewerVM.SetChipColor(j, i, MapViewerColorDefines.Normal);
                            break;
                    }
                }
            }
        }


        #region [Position Seq]
        private void SetPositionData()
        {
            int[] mapData = GlobalObjects.Instance.Get<RecipeFront>().WaferMap.Data;
            int mapX = GlobalObjects.Instance.Get<RecipeFront>().WaferMap.MapSizeX;
            int mapY = GlobalObjects.Instance.Get<RecipeFront>().WaferMap.MapSizeY;

            _validChip.Clear();
            _posData.Clear();
            for (int x = 0; x < mapX; x++)
            {
                if (x < _selectedParam.StartLine || x > _selectedParam.EndLine)
                    continue;

                for (int y = 0; y < mapY; y++)
                {
                    if (mapData[y * mapX + x] != 0)
                    {
                        _validChip.Add(new CPoint(x, y));
                    }
                }
            }
        }
        private void SetPositionDoneMapColor()
        {
            foreach(CPoint pt in _validChip)
                this.MapViewerVM.SetChipColor(pt.X, pt.Y, MapViewerColorDefines.GetWorkplaceStateColor(WORK_TYPE.INSPECTION));
        }
        private void StartPosition()
        {
            int selectedCh = ((int)_imageViewerVM.p_eColorViewMode - 1 < 0) ? 0 : (int)_imageViewerVM.p_eColorViewMode - 1;

            _inspectionSharedBuffer = GlobalObjects.Instance.GetNamed<ImageData>("frontImage").GetPtr(selectedCh);
            CPoint sharedBufferSz = GlobalObjects.Instance.GetNamed<ImageData>("frontImage").p_Size; 
            List<RecipeType_ImageData> feature = GlobalObjects.Instance.Get<RecipeFront>().GetItem<PositionRecipe>().ListDieFeature;

            if (feature.Count < 1)
            {
                MessageBox.Show("          Feature Num < 1 \nPlease Add Feature Images", "WARNING");
                return;
            }
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
            
            int absFeatureX, absFeatureY;
            int absChipX, absChipY;

            int outX = 0, outY = 0;
            foreach(CPoint chipPos in _validChip)
            {
                absChipX = originRecipe.OriginX + chipPos.X * originRecipe.DiePitchX;
                absChipY = originRecipe.OriginY + (chipPos.Y - 1) * originRecipe.DiePitchY; // 기준 좌표가 좌하단인듯
                absFeatureX = originRecipe.OriginX + feature[0].PositionX + chipPos.X * originRecipe.DiePitchX;
                absFeatureY = originRecipe.OriginY + feature[0].PositionY + chipPos.Y * originRecipe.DiePitchY;
                
                unsafe
                { 
                    CLR_IP.Cpp_TemplateMatching(
                        (byte*)_inspectionSharedBuffer.ToPointer(), feature[0].GetColorRowData((IMAGE_CHANNEL)selectedCh), &outX, &outY,
                        sharedBufferSz.X, sharedBufferSz.Y,
                        feature[0].Width, feature[0].Height,
                        absFeatureX - _selectedParam.ChipSearchRange, absFeatureY - _selectedParam.ChipSearchRange,
                        absFeatureX + feature[0].Width + _selectedParam.ChipSearchRange, absFeatureY + feature[0].Height + _selectedParam.ChipSearchRange, 
                        5, 1, ((int)_imageViewerVM.p_eColorViewMode - 1 < 0) ? 0 : (int)_imageViewerVM.p_eColorViewMode - 1);
                }

                DrawRectChipFeature(
                    new CPoint(absFeatureX, absFeatureY), 
                    new CPoint(absFeatureX + feature[0].Width, absFeatureY + feature[0].Height), 
                    new CPoint(absFeatureX - _selectedParam.ChipSearchRange + outX, absFeatureY - _selectedParam.ChipSearchRange + outY), 
                    new CPoint(absFeatureX - _selectedParam.ChipSearchRange + outX + feature[0].Width, absFeatureY - _selectedParam.ChipSearchRange + outY + feature[0].Height));


                int transX = outX - _selectedParam.ChipSearchRange;
                int transY = outY - _selectedParam.ChipSearchRange;

                _posData.Add(new CPoint(absChipX + transX, absChipY + transY));

                this.MapViewerVM.SetChipColor(chipPos.X, chipPos.Y, MapViewerColorDefines.GetWorkplaceStateColor(WORK_TYPE.INSPECTION));
            }

            IsEnabledCreate = true; 
        }
        #endregion

        #region [Command]
        public RelayCommand LoadedCommand
        {
            get => new RelayCommand(() =>
            {
                LoadRecipe();
            });
        }
        public RelayCommand UnloadedCommand
        {
            get => new RelayCommand(() =>
            {
            });
        }

        public RelayCommand btnStartPos
        {
            get => new RelayCommand(() =>
            {
                this.ImageViewerVM.ClearObjects();
                ResetMapColor();
                SetPositionData();
                StartPosition();
            });
        }
        public RelayCommand btnCreateGolden
        {
            get => new RelayCommand(() =>
            {
                ResetMapColor();
                SetPositionDoneMapColor();
                int selectedCh = ((int)_imageViewerVM.p_eColorViewMode - 1 < 0) ? 0 : (int)_imageViewerVM.p_eColorViewMode - 1;

                _inspectionSharedBuffer = GlobalObjects.Instance.GetNamed<ImageData>("frontImage").GetPtr(selectedCh);

                CPoint sharedBufferSz = GlobalObjects.Instance.GetNamed<ImageData>("frontImage").p_Size;
                OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

                this._preCreateGolden.SetInspectionBuffer(this._inspectionSharedBuffer, sharedBufferSz.X, sharedBufferSz.Y);
                this._preCreateGolden.SetParameter(this._preCreateGoldenParameter);
                List<CPoint> useChipMapData = this._preCreateGolden.SetChipPositionList(this._posData, this._validChip, GlobalObjects.Instance.Get<RecipeFront>().WaferMap.MapSizeY);

                _goldenImage = this._preCreateGolden.CreateGoldenImage(originRecipe.OriginWidth, originRecipe.OriginHeight);

                if (p_GoldenImgSource != null)
                    p_GoldenImgSource = null;

                p_GoldenImgSource = BitmapSource.Create(originRecipe.OriginWidth, originRecipe.OriginHeight, 96d, 96d,
                                 PixelFormats.Gray8, null, _goldenImage, originRecipe.OriginWidth);

                foreach(CPoint pt in useChipMapData)
                {
                    this.MapViewerVM.SetChipColor(pt.X, pt.Y, MapViewerColorDefines.GetWorkplaceStateColor(WORK_TYPE.DEFECTPROCESS_ALL));
                }

                IsEnabledSave = true;
            });
        }
        public RelayCommand btnSaveGolden
        {
            get => new RelayCommand(() =>
            {
                if(p_GoldenImgSource != null)
                {
                    // Color Golden Image Save
                    OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
                    string path = GlobalObjects.Instance.Get<RecipeFront>().RecipeFolderPath + "preGoldenImage.bmp";
                    CPoint sharedBufferSz = GlobalObjects.Instance.GetNamed<ImageData>("frontImage").p_Size;
                    IntPtr inspectionSharedBuffer;
                    int selectedCh = ((int)_imageViewerVM.p_eColorViewMode - 1 < 0) ? 0 : (int)_imageViewerVM.p_eColorViewMode - 1;

                    byte[] goldenImageColor = new byte[originRecipe.OriginWidth * originRecipe.OriginHeight * 3];
                    byte[][] golden = new byte[3][];

                    for(int ch = 0; ch < 3; ch++)
                    {
                        golden[ch] = new byte[originRecipe.OriginWidth * originRecipe.OriginHeight];
                        
                        if (selectedCh == ch)
                        {
                            golden[ch] = _goldenImage;
                            continue;
                        }

                        inspectionSharedBuffer = GlobalObjects.Instance.GetNamed<ImageData>("frontImage").GetPtr(ch);
                        this._preCreateGolden.SetInspectionBuffer(inspectionSharedBuffer, sharedBufferSz.X, sharedBufferSz.Y);
                        golden[ch] = this._preCreateGolden.CreateGoldenImage(originRecipe.OriginWidth, originRecipe.OriginHeight);
                    }

                    Parallel.For(0, originRecipe.OriginHeight, i =>
                    {
                        for(int j = 0; j < originRecipe.OriginWidth; j++)
                        {
                            goldenImageColor[(Int64)i * originRecipe.OriginWidth * 3 + j * 3 + 0] = golden[0][(Int64)i * originRecipe.OriginWidth + j];
                            goldenImageColor[(Int64)i * originRecipe.OriginWidth * 3 + j * 3 + 1] = golden[1][(Int64)i * originRecipe.OriginWidth + j];
                            goldenImageColor[(Int64)i * originRecipe.OriginWidth * 3 + j * 3 + 2] = golden[2][(Int64)i * originRecipe.OriginWidth + j];
                        }
                    });

                    Tools.SaveRawdataToBitmap(path, goldenImageColor, originRecipe.OriginWidth, originRecipe.OriginHeight, 3);
                }
            });
        }
        public RelayCommand btnClear
        {
            get => new RelayCommand(() =>
            {
                this.ImageViewerVM.ClearObjects();
            });
        }

        #endregion

        #region [ImageView Draw Method]

        public void DrawRectChipFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd)
        {
            ImageViewerVM.AddDrawRect(ptOldStart, ptOldEnd, ImageViewerColorDefines.ChipPosition, "position");
            ImageViewerVM.AddDrawRect(ptNewStart, ptNewEnd, ImageViewerColorDefines.ChipPositionMove, "position");
        }

        #endregion
    }
}