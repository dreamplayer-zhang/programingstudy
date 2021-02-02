using RootTools;
using RootTools_Vision;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Root_WIND2.UI_Temp
{
    class FrontsideAlignment_ViewModel : ObservableObject
    {
        private readonly FrontsideAlignment_ImageViewer_ViewModel imageViewerVM;
        public FrontsideAlignment_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
        }

        public FrontsideAlignment_ViewModel()
        {
            this.imageViewerVM = new FrontsideAlignment_ImageViewer_ViewModel();
            this.imageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());

            this.ImageViewerVM.FeatureBoxDone += FeatureBoxDone_Callback;
        }

        public void SetPage()
        {
            this.ImageViewerVM.SetViewRect();
        }

        #region [Members]
        ImageData boxImage;


        #endregion

        #region [Properties]

        private BitmapSource m_BoxImgSource;
        public BitmapSource p_BoxImgSource
        {
            get
            {
                return m_BoxImgSource;
            }
            set
            {
                SetProperty(ref m_BoxImgSource, value);
            }
        }

        private CPoint m_PointXY = new CPoint();
        public CPoint p_PointXY
        {
            get
            {
                return m_PointXY;
            }
            set
            {
                SetProperty(ref m_PointXY, value);
            }
        }
        
        private CPoint m_SizeWH = new CPoint();
        public CPoint p_SizeWH
        {
            get
            {
                return m_SizeWH;
            }
            set
            {
                SetProperty(ref m_SizeWH, value);
            }
        }

        private CPoint m_Offset = new CPoint();
        public CPoint p_Offset
        {
            get
            {
                return m_Offset;
            }
            set
            {
                SetProperty(ref m_Offset, value);
            }
        }



        private ObservableCollection<UIElement> m_WaferFeatureList = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_WaferFeatureList
        {
            get
            {
                return m_WaferFeatureList;
            }
            set
            {
                m_WaferFeatureList = value;
            }
        }
        private ObservableCollection<UIElement> m_ShotFeatureList = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_ShotFeatureList
        {
            get
            {
                return m_ShotFeatureList;
            }
            set
            {
                m_ShotFeatureList = value;
            }
        }
        private ObservableCollection<UIElement> m_ChipFeatureList = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_ChipFeatureList
        {
            get
            {
                return m_ChipFeatureList;
            }
            set
            {
                m_ChipFeatureList = value;
            }
        }

        private int m_SelectedWaferIndex = 0;
        public int p_SelectedWaferIndex
        {
            get
            {
                return m_SelectedWaferIndex;
            }
            set
            {
                SetProperty<int>(ref m_SelectedWaferIndex, value);
            }
        }

        private int m_SelectedShotIndex = 0;
        public int p_SelectedShotIndex
        {
            get
            {
                return m_SelectedShotIndex;
            }
            set
            {
                SetProperty<int>(ref m_SelectedShotIndex, value);
            }
        }

        private int m_SelectedChipIndex = 0;
        public int p_SelectedChipIndex
        {
            get
            {
                return m_SelectedChipIndex;
            }
            set
            {
                SetProperty<int>(ref m_SelectedChipIndex, value);
            }
        }
        #endregion

        #region [Command]

        public RelayCommand btnFeatureBoxClearCommand
        {
            get => new RelayCommand(() =>
            {
               
                FeatureBoxViewerClear();
            });
        }

        public RelayCommand btnAddWaferFeatureCommend
        {
            get => new RelayCommand(() =>
            {
                if (boxImage == null)
                    return;

                PositionRecipe postionRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<PositionRecipe>();
                postionRecipe.AddMasterFeature(m_Offset.X, m_Offset.Y, m_SizeWH.X, m_SizeWH.Y, boxImage.p_nByte, boxImage.GetByteArray());

                FeatureControl fc = new FeatureControl();
                fc.p_Offset = m_Offset;
                fc.p_ImageSource = p_BoxImgSource;
                fc.DataContext = this;

                p_WaferFeatureList.Add(fc);
            });
        }

        public RelayCommand btnAddShotFeatureCommend
        {
            get => new RelayCommand(() =>
            {
                if (boxImage == null)
                    return;

                PositionRecipe postionRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<PositionRecipe>();
                postionRecipe.AddShotFeature(m_Offset.X, m_Offset.Y, m_SizeWH.X, m_SizeWH.Y, boxImage.p_nByte, boxImage.GetByteArray());

                FeatureControl fc = new FeatureControl();
                fc.p_Offset = m_Offset;
                fc.p_ImageSource = p_BoxImgSource;
                fc.DataContext = this;

                p_ShotFeatureList.Add(fc);
            });
        }

        public RelayCommand btnAddChipFeatureCommend
        {
            get => new RelayCommand(() =>
            {
                if (boxImage == null)
                    return;

                PositionRecipe postionRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<PositionRecipe>();
                postionRecipe.AddChipFeature(m_Offset.X, m_Offset.Y, m_SizeWH.X, m_SizeWH.Y, boxImage.p_nByte, boxImage.GetByteArray());

                FeatureControl fc = new FeatureControl();
                fc.p_Offset = m_Offset;
                fc.p_ImageSource = p_BoxImgSource;
                fc.DataContext = this;

                p_ChipFeatureList.Add(fc);
            });
        }

        public RelayCommand btnDeleteWaferFeatureCommend
        {
            get => new RelayCommand(() =>
            {
                if (p_WaferFeatureList.Count == 0 || p_SelectedWaferIndex == -1) return;

                PositionRecipe postionRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<PositionRecipe>();
                postionRecipe.RemoveMasterFeature(p_SelectedWaferIndex);
                p_WaferFeatureList.RemoveAt(p_SelectedWaferIndex);
            });
        }

        public RelayCommand btnDeleteShotFeatureCommend
        {
            get => new RelayCommand(() =>
            {
                if (p_ShotFeatureList.Count == 0 || p_SelectedShotIndex == -1) return;

                PositionRecipe postionRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<PositionRecipe>();
                postionRecipe.RemoveShotFeature(p_SelectedShotIndex);
                p_ShotFeatureList.RemoveAt(p_SelectedShotIndex);
            });
        }

        public RelayCommand btnDeleteChipFeatureCommend
        {
            get => new RelayCommand(() =>
            {
                if (p_ChipFeatureList.Count == 0 || p_SelectedChipIndex == -1) return;

                PositionRecipe postionRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<PositionRecipe>();
                postionRecipe.RemoveChipFeature(p_SelectedChipIndex);
                p_ChipFeatureList.RemoveAt(p_SelectedChipIndex);
            });
        }

        public RelayCommand btnClearWaferFeatureCommend
        {
            get => new RelayCommand(() =>
            {
                if (p_WaferFeatureList.Count == 0) return;

                PositionRecipe postionRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<PositionRecipe>();
                postionRecipe.ListMasterFeature.Clear();
                p_WaferFeatureList.Clear();
            });
        }

        public RelayCommand btnClearShotFeatureCommend
        {
            get => new RelayCommand(() =>
            {
                if (p_ShotFeatureList.Count == 0) return;

                PositionRecipe postionRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<PositionRecipe>();
                postionRecipe.ListShotFeature.Clear();
                p_ShotFeatureList.Clear();
            });
        }

        public RelayCommand btnClearChipFeatureCommend
        {
            get => new RelayCommand(() =>
            {
                if (p_ChipFeatureList.Count == 0) return;

                PositionRecipe postionRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<PositionRecipe>();
                postionRecipe.ListDieFeature.Clear();
                p_ChipFeatureList.Clear();
            });
        }
        #endregion


        #region [ImageViewer Callback Method]
        private void FeatureBoxViewerClear()
        {
            this.ImageViewerVM.FeatureBoxClear();

            if (boxImage != null)
                boxImage.Clear();
            if (p_BoxImgSource != null)
                p_BoxImgSource = null;

        }

        private void FeatureBoxDone_Callback(object obj)
        {
            TRect Box = obj as TRect;
            int byteCnt = ImageViewerVM.p_ImageData.p_nByte;

            boxImage = new ImageData(Box.MemoryRect.Width, Box.MemoryRect.Height, byteCnt);

            boxImage.m_eMode = ImageData.eMode.ImageBuffer;
            boxImage.SetData(ImageViewerVM.p_ImageData
                , new CRect(Box.MemoryRect.Left, Box.MemoryRect.Top, Box.MemoryRect.Right, Box.MemoryRect.Bottom)
                , (int)ImageViewerVM.p_ImageData.p_Stride, byteCnt);

            Dispatcher.CurrentDispatcher.BeginInvoke(new ThreadStart(() =>
            {
                p_BoxImgSource = boxImage.GetBitMapSource(byteCnt);
            }));

            p_PointXY = new CPoint(Box.MemoryRect.Left, Box.MemoryRect.Top);
            p_SizeWH = new CPoint(Box.MemoryRect.Width, Box.MemoryRect.Height);

            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
            CPoint origin = new CPoint(originRecipe.OriginX, originRecipe.OriginY);

            p_Offset = m_PointXY - origin;
        }
        #endregion
    }
}
