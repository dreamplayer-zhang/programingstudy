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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;

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

        StopWatch m_swMouse = new StopWatch();
        CPoint m_ptViewBuffer = new CPoint();
        CPoint m_ptMouseBuffer = new CPoint();
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
        private System.Windows.Input.Cursor m_Cursor = System.Windows.Input.Cursors.Arrow;
        public System.Windows.Input.Cursor p_Cursor
        {
            get
            {
                return m_Cursor;
            }
            set
            {
                SetProperty(ref m_Cursor, value);
            }
        }

        protected int m_CanvasWidth = 100;
        public int p_CanvasWidth
        {
            get
            {
                return m_CanvasWidth;
            }
            set
            {
                if (value == 0)
                    return;

                SetProperty(ref m_CanvasWidth, value);
                //SetRoiRect();
            }
        }

        private int m_CanvasHeight = 100;
        public int p_CanvasHeight
        {
            get
            {
                return m_CanvasHeight;
            }
            set
            {
                if (value == 0)
                    return;
                SetProperty(ref m_CanvasHeight, value);
                //SetRoiRect();
            }
        }

        protected int m_MouseX = 0;
        public virtual int p_MouseX
        {
            get
            {
                return m_MouseX;
            }
            set
            {
                SetProperty(ref m_MouseX, value);
            }
        }

        private int m_MouseY = 0;
        public int p_MouseY
        {
            get
            {
                return m_MouseY;
            }
            set
            {

                SetProperty(ref m_MouseY, value);
            }
        }

        private double m_Zoom = 1;
        public double p_Zoom
        {
            get
            {
                return m_Zoom;
            }
            set
            {
                if (value < 0.0001)
                    value = 0.0001;
                SetProperty(ref m_Zoom, value);
                //SetRoiRect();
            }
        }

        private System.Drawing.Rectangle m_View_Rect = new System.Drawing.Rectangle();
        public System.Drawing.Rectangle p_View_Rect
        {
            get
            {
                return m_View_Rect;
            }
            set
            {
                SetProperty(ref m_View_Rect, value);
            }
        }

        private ImageData m_ImageData;
        public ImageData p_ImageData
        {
            get
            {
                return m_ImageData;
            }
            set
            {
                SetProperty(ref m_ImageData, value);
            }
        }

        int m_nBrightness = 0;
        public int p_nBrightness
        {
            get { return m_nBrightness; }
            set
            {
                // 설정하려는 값을 -100 ~ 100의 값으로 제한
                m_nBrightness = Clamp(value, -100, 100);

                // 화면에 표시되는 이미지에 반영
                SetImageSource();
            }
        }
        int m_nContrast = 0;
        public int p_nContrast
        {
            get { return m_nContrast; }
            set
            {
                // 설정하려는 값을 -100 ~ 100의 값으로 제한
                m_nContrast = Clamp(value, -100, 100);

                // 화면에 표시되는 이미지에 반영
                SetImageSource();
            }
        }

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

        #region [Viewer]
        public virtual void SetRoiRect()
        {
            if (p_ImageData != null)
            {
                CPoint StartPt = GetStartPoint_Center(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
                bool bRatio_WH = false;
                //if (p_ImageData.p_nByte == 1)
                bRatio_WH = (double)p_ImageData.p_Size.X / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;

                if (bRatio_WH)
                { //세로가 길어
                    p_View_Rect = new System.Drawing.Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom), Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom * p_CanvasHeight / p_CanvasWidth));
                }
                else
                {
                    p_View_Rect = new System.Drawing.Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom * p_CanvasWidth / p_CanvasHeight), Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom));
                }
                SetImageSource();
            }
        }

        CPoint GetStartPoint_Center(int nImgWidth, int nImgHeight)
        {
            bool bRatio_WH;

            bRatio_WH = (double)p_ImageData.p_Size.X / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;
            int viewrectwidth = 0;
            int viewrectheight = 0;
            int nX = 0;
            int nY = 0;
            if (bRatio_WH)
            { //세로가 길어
              //nX = p_View_Rect.X + Convert.ToInt32(p_View_Rect.Width - nImgWidth * p_Zoom) /2; 기존 중앙기준으로 확대/축소되는 코드. 
                nX = p_View_Rect.X + Convert.ToInt32(p_View_Rect.Width - nImgWidth * p_Zoom) * p_MouseX / p_CanvasWidth; // 마우스 커서기준으로 확대/축소
                nY = p_View_Rect.Y + Convert.ToInt32(p_View_Rect.Height - nImgWidth * p_Zoom * p_CanvasHeight / p_CanvasWidth) * p_MouseY / p_CanvasHeight;
                viewrectwidth = Convert.ToInt32(nImgWidth * p_Zoom);
                viewrectheight = Convert.ToInt32(nImgWidth * p_Zoom * p_CanvasHeight / p_CanvasWidth);
            }
            else
            {
                nX = p_View_Rect.X + Convert.ToInt32(p_View_Rect.Width - nImgHeight * p_Zoom * p_CanvasWidth / p_CanvasHeight) * p_MouseX / p_CanvasWidth;
                nY = p_View_Rect.Y + Convert.ToInt32(p_View_Rect.Height - nImgHeight * p_Zoom) * p_MouseY / p_CanvasHeight;
                viewrectwidth = Convert.ToInt32(nImgHeight * p_Zoom * p_CanvasWidth / p_CanvasHeight);
                viewrectheight = Convert.ToInt32(nImgHeight * p_Zoom);
            }

            if (nX < 0)
                nX = 0;
            else if (nX > nImgWidth - viewrectwidth)
                nX = nImgWidth - viewrectwidth;
            if (nY < 0)
                nY = 0;
            else if (nY > nImgHeight - viewrectheight)
                nY = nImgHeight - viewrectheight;
            return new CPoint(nX, nY);
        }

        public virtual void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            if (p_ImageData == null) return;

            CPoint MovePoint = new CPoint();
            MovePoint.X = point.X + p_View_Rect.Width * nX / p_CanvasWidth;
            MovePoint.Y = point.Y + p_View_Rect.Height * nY / p_CanvasHeight;

            if (MovePoint.X < 0)
                MovePoint.X = 0;
            else if (MovePoint.X > p_ImageData.p_Size.X - p_View_Rect.Width)
                MovePoint.X = p_ImageData.p_Size.X - p_View_Rect.Width;
            if (MovePoint.Y < 0)
                MovePoint.Y = 0;
            else if (MovePoint.Y > p_ImageData.p_Size.Y - p_View_Rect.Height)
                MovePoint.Y = p_ImageData.p_Size.Y - p_View_Rect.Height;

            SetViewRect(MovePoint);
            SetImageSource();
        }

        void SetViewRect(CPoint point)      //point image의 좌상단
        {
            bool bRatio_WH = false;

            bRatio_WH = (double)p_ImageData.p_Size.X / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;

            if (bRatio_WH)
            { 
                p_View_Rect = new System.Drawing.Rectangle(point.X, point.Y, Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom), Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom * p_CanvasHeight / p_CanvasWidth));
            }
            else
            {
                p_View_Rect = new System.Drawing.Rectangle(point.X, point.Y, Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom * p_CanvasWidth / p_CanvasHeight), Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom));
            }
        }

        public void InitRoiRect(int nWidth, int nHeight)
        {
            if (p_ImageData == null)
            {
                p_View_Rect = new System.Drawing.Rectangle(0, 0, nWidth, nHeight);
                p_Zoom = 1;
            }
            bool bRatio_WH = (double)p_View_Rect.Width / p_CanvasWidth < (double)p_View_Rect.Height / p_CanvasHeight;
            if (bRatio_WH)//세로가 길어
            {
                p_View_Rect = new System.Drawing.Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Width, p_View_Rect.Width * p_CanvasHeight / p_CanvasWidth);
            }
            else
            {
                p_View_Rect = new System.Drawing.Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Height * p_CanvasWidth / p_CanvasHeight, p_View_Rect.Height);
            }
        }

        public virtual unsafe void SetImageSource()
        {
            if (p_ImageData != null)
            {
                object o = new object();

                Image<Gray, byte> view = new Image<Gray, byte>(p_CanvasWidth, p_CanvasHeight);

                IntPtr ptrMem = p_ImageData.GetPtr();
                if (ptrMem == IntPtr.Zero)
                    return;

                int rectX, rectY, rectWidth, rectHeight, sizeX;
                byte[,,] viewptr = view.Data;

                rectX = p_View_Rect.X;
                rectY = p_View_Rect.Y;
                rectWidth = p_View_Rect.Width;
                rectHeight = p_View_Rect.Height;

                sizeX = p_ImageData.p_Size.X;

                Parallel.For(0, p_CanvasHeight, (yy) =>
                {
                    {
                        long pix_y = rectY + yy * rectHeight / p_CanvasHeight;

                        for (int xx = 0; xx < p_CanvasWidth; xx++)
                        {
                            long pix_x = rectX + xx * rectWidth / p_CanvasWidth;
                                        /*byte pixel = ((byte*)ptrMem)[pix_x + (long)pix_y * sizeX];*/
                            byte* arrByte = (byte*)ptrMem.ToPointer();
                            long idx = pix_x + (long)pix_y * sizeX;
                            byte pixel = arrByte[idx];
                            viewptr[yy, xx, 0] = ApplyContrastAndBrightness(pixel);
                        }
                    }
                });

                p_GoldenImgSource = ImageHelper.ToBitmapSource(view);
            }
        }

        public byte ApplyContrastAndBrightness(byte color)
        {
            if (p_nBrightness == 0 && p_nContrast == 0)
                return color;

            double contrastLevel = Math.Pow((100.0 + p_nContrast) / 100.0, 2);

            double newColor = (((((double)color / 255.0) - 0.5) * contrastLevel) + 0.5) * 255.0;
            newColor += p_nBrightness;

            return (byte)Clamp((int)Math.Round(newColor), 0, 255);
        }

        public int Clamp(int val, int min, int max)
        {
            int ret = Math.Max(val, min);
            ret = Math.Min(ret, max);

            return ret;
        }
        #endregion

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
                System.Windows.MessageBox.Show("          Feature Num < 1 \nPlease Add Feature Images", "WARNING");
                return;
            }
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
            
            int absFeatureX, absFeatureY;
            int absChipX, absChipY;

            int outX = 0, outY = 0;
            foreach(CPoint chipPos in _validChip)
            {
                absChipX = originRecipe.OriginX + chipPos.X * originRecipe.DiePitchX;
                absChipY = originRecipe.OriginY + chipPos.Y * originRecipe.DiePitchY - originRecipe.OriginY; // 기준 좌표가 좌하단인듯
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
                p_ImageData = null;
                p_ImageData = new ImageData(originRecipe.OriginWidth, originRecipe.OriginHeight, 1);
                
                IntPtr ptrMem = p_ImageData.GetPtr();

                unsafe { 
                    byte *p = (byte*)ptrMem.ToPointer();
                    for (int i = 0; i < originRecipe.OriginWidth * originRecipe.OriginHeight; i++, p++)
                    {
                        *p = _goldenImage[i];
                    }
                }

                SetRoiRect();
                InitRoiRect(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
                SetImageSource();
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
                    GlobalObjects.Instance.Get<RecipeFront>().GetItem<D2DRecipe>().SetPreGoldenImage(goldenImageColor, originRecipe.OriginWidth, originRecipe.OriginHeight);
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

        #region MethodAction

        public virtual void PreviewMouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            m_ptViewBuffer = new CPoint(p_View_Rect.X, p_View_Rect.Y);
            m_ptMouseBuffer = new CPoint(p_MouseX, p_MouseY);
            m_swMouse.Restart();
        }

        public virtual void MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

            var viewer = sender as Grid;
            viewer.Focus();

            var pt = e.GetPosition(sender as IInputElement);
            p_MouseX = (int)pt.X;
            p_MouseY = (int)pt.Y;

            if (e.LeftButton == MouseButtonState.Pressed && m_swMouse.ElapsedMilliseconds > 0)
            {
                CanvasMovePoint_Ref(m_ptViewBuffer, m_ptMouseBuffer.X - p_MouseX, m_ptMouseBuffer.Y - p_MouseY);
                return;
            }
        }

        public virtual void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var viewer = sender as Grid;
            viewer.Focus();


            int lines = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            double zoom = p_Zoom;

            if (lines < 0)
            {
                zoom *= 1.1F;
            }
            if (lines > 0)
            {
                zoom *= 0.9F;
            }

            if (zoom > 1)
            {
                zoom = 1;
            }
            if (zoom < 0.0001)
            {
                zoom = 0.0001;
            }
            p_Zoom = zoom;
            SetRoiRect();
        }

        #endregion
    }
}