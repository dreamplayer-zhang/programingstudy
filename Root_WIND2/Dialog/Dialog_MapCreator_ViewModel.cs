using RootTools;
using RootTools_Vision;
using RootTools_CLR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace Root_WIND2
{
    class Dialog_MapCreator_ViewModel : ObservableObject, IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
        private readonly Dialog_MapCreator_ImageViewer_ViewModel imageViewerVM;
        public Dialog_MapCreator_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
        }

        private readonly UI_User.FrontsideProduct_ViewModel frontsideProductVM;
        public UI_User.FrontsideProduct_ViewModel FrontsideProductVM
        {
            get => this.frontsideProductVM;
        }

        public Dialog_MapCreator_ViewModel()
        {
            if (GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr() == IntPtr.Zero && GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").m_eMode != ImageData.eMode.OtherPCMem)
                return;

            this.imageViewerVM = new Dialog_MapCreator_ImageViewer_ViewModel();
            this.imageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());

            this.imageViewerVM.SelectChipPointDone += SelectChipPointDone_Callback;
            this.imageViewerVM.SelectRoiPointDone += SelectRoiPointDone_Callback;
            this.imageViewerVM.SelectChipBoxDone += SelectChipBoxDone_Callback;
            this.imageViewerVM.SelectChipBoxReset += SelectChipBoxReset_Callback;
            this.imageViewerVM.SelectRoiBoxDone += SelectRoiBoxDone_Callback;
            this.imageViewerVM.SelectRoiBoxReset += SelectRoiBoxReset_Callback;

            this.frontsideProductVM = new UI_User.FrontsideProduct_ViewModel();
        }

        public void SelectChipPointDone_Callback()
        {
            this.ChipX = this.imageViewerVM.selectChipLeftTop.X;
            this.ChipY = this.imageViewerVM.selectChipLeftTop.Y;
        }

        public void SelectRoiPointDone_Callback()
        {
            this.RoiX = this.imageViewerVM.selectRoiLeftTop.X;
            this.RoiY = this.imageViewerVM.selectRoiLeftTop.Y;
        }

        public void SelectChipBoxDone_Callback()
        {
            this.ChipWidth = this.imageViewerVM.selectChipBox.Width;
            this.ChipHeight = this.imageViewerVM.selectChipBox.Height;
        }

        public void SelectChipBoxReset_Callback()
        {

        }

        public void SelectRoiBoxDone_Callback()
        {
            this.RoiWidth = this.imageViewerVM.selectRoiBox.Width;
            this.RoiHeight = this.imageViewerVM.selectRoiBox.Height;
        }

        public void SelectRoiBoxReset_Callback()
        {

        }

        #region [Properties]
        private int chipX = 0;
        public int ChipX
        {
            get => this.chipX;
            set
            {
                SetProperty<int>(ref this.chipX, value);
            }
        }

        private int chipY = 0;
        public int ChipY
        {
            get => this.chipY;
            set
            {
                SetProperty<int>(ref this.chipY, value);
            }
        }

        private int chipWidth = 0;
        public int ChipWidth
        {
            get => this.chipWidth;
            set
            {
                SetProperty<int>(ref this.chipWidth, value);
            }
        }

        private int chipHeight = 0;
        public int ChipHeight
        {
            get => this.chipHeight;
            set
            {
                SetProperty<int>(ref this.chipHeight, value);
            }
        }

        private int roiX = 0;
        public int RoiX
        {
            get => this.roiX;
            set
            {
                SetProperty<int>(ref this.roiX, value);
            }
        }

        private int roiY = 0;
        public int RoiY
        {
            get => this.roiY;
            set
            {
                SetProperty<int>(ref this.roiY, value);
            }
        }

        private int roiWidth = 0;
        public int RoiWidth
        {
            get => this.roiWidth;
            set
            {
                SetProperty<int>(ref this.roiWidth, value);
            }
        }

        private int roiHeight = 0;
        public int RoiHeight
        {
            get => this.roiHeight;
            set
            {
                SetProperty<int>(ref this.roiHeight, value);
            }
        }

        private int resizedChipWidth = 0;
        public int ResizedChipWidth
        {
            get => this.resizedChipWidth;
            set
            {
                SetProperty<int>(ref this.resizedChipWidth, value);
            }
        }

        private int resizedChipHeight = 0;
        public int ResizedChipHeight
        {
            get => this.resizedChipHeight;
            set
            {
                SetProperty<int>(ref this.resizedChipHeight, value);
            }
        }

        private int resizedRoiWidth = 0;
        public int ResizedRoiWidth
        {
            get => this.resizedRoiWidth;
            set
            {
                SetProperty<int>(ref this.resizedRoiWidth, value);
            }
        }

        private int resizedRoiHeight = 0;
        public int ResizedRoiHeight
        {
            get => this.resizedRoiHeight;
            set
            {
                SetProperty<int>(ref this.resizedRoiHeight, value);
            }
        }

        private int chipSimilarity = 70;
        public int ChipSimilarity
        {
            get => this.chipSimilarity;
            set
            {
                SetProperty<int>(ref this.chipSimilarity, value);
            }
        }

        private int resizeScaleFactor = 20;
        public int ResizeScaleFactor
        {
            get => this.resizeScaleFactor;
            set
            {
                SetProperty<int>(ref this.resizeScaleFactor, value);
            }
        }

        List<C3Point> ptOffset = new List<C3Point>();
        public List<C3Point> PtOffset
        {
            get => this.ptOffset;
            set
            {
                SetProperty<List<C3Point>>(ref this.ptOffset, value);
            }
        }

        int mapWidth = 0;
        public int MapWidth
        {
            get => this.mapWidth;
            set
            {
                SetProperty<int>(ref this.mapWidth, value);
            }
        }

        int mapHeight = 0;
        public int MapHeight
        {
            get => this.mapHeight;
            set
            {
                SetProperty<int>(ref this.mapHeight, value);
            }
        }

        int[] searchedWaferMap = null;
        public int[] SearchedWaferMap
        {
            get => this.searchedWaferMap;
            set
            {
                SetProperty<int[]>(ref this.searchedWaferMap, value);
            }
        }
        #endregion

        #region [Command]
        public RelayCommand btnStartFindChipCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ptOffset.Clear();
                    MapWidth = 0;
                    MapHeight = 0;
                    FindChip();
                });
            }
        }

        public void OnOkButton()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(true));
        }

        public void OnCancelButton()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(false));
        }
        #endregion

        #region [Function]
        public void FindChip()
        {
            if (this.ChipWidth > 0 && this.ChipHeight > 0 && this.roiWidth > 0 && this.roiHeight > 0 && this.ChipX >= this.RoiX && this.ChipX + this.ChipWidth <= this.RoiX + this.RoiWidth && this.ChipY >= this.RoiY && this.ChipY + this.ChipHeight <= this.RoiY + this.RoiHeight)
            {
                CPoint ptOriginChip = new CPoint(this.ChipX, this.ChipY);
                CPoint ptOffsetChip = new CPoint((this.ChipX - this.RoiX) % this.ChipWidth, (this.ChipY - this.RoiY) % this.ChipHeight);
                C3Point ptSearchedChip = new C3Point();
                int idx = 0;
                int chipCount = 0;
                int[] tempWaferMap = new int[30000];

                this.ResizedChipWidth = this.ChipWidth / this.ResizeScaleFactor;
                this.ResizedChipHeight = this.ChipHeight / this.ResizeScaleFactor;
                this.ResizedRoiWidth = this.RoiWidth / this.ResizeScaleFactor;
                this.ResizedRoiHeight = this.RoiHeight / this.ResizeScaleFactor;

                byte[] byteChipBoxResized = new byte[this.ResizedChipWidth * this.ResizedChipHeight];
                byte[] byteRoiBoxResized = new byte[this.ResizedRoiWidth * this.ResizedRoiHeight];

                ImageData imageData = this.ImageViewerVM.p_ImageData;
                IntPtr mainImage = new IntPtr();
                mainImage = imageData.GetPtr(0);

                unsafe
                {
                    fixed (byte* ptrChipBoxResized = new byte[this.ResizedChipWidth * this.ResizedChipHeight])
                    {
                        CLR_IP.Cpp_SubSampling((byte*)mainImage, ptrChipBoxResized, this.ImageViewerVM.p_ImageData.p_Size.X, this.ImageViewerVM.p_ImageData.p_Size.Y, this.ChipX, this.ChipY, this.ChipX + this.ChipWidth, this.ChipY + this.ChipHeight, this.ResizeScaleFactor);
                        Marshal.Copy((IntPtr)ptrChipBoxResized, byteChipBoxResized, 0, this.ResizedChipWidth * this.ResizedChipHeight);
                    }
                    fixed (byte* ptrRoiBoxResized = new byte[this.ResizedRoiWidth * this.ResizedRoiHeight])
                    {
                        CLR_IP.Cpp_SubSampling((byte*)mainImage, ptrRoiBoxResized, this.ImageViewerVM.p_ImageData.p_Size.X, this.ImageViewerVM.p_ImageData.p_Size.Y, this.RoiX, this.RoiY, this.RoiX + this.RoiWidth, this.RoiY + this.RoiHeight, this.ResizeScaleFactor);
                        Marshal.Copy((IntPtr)ptrRoiBoxResized, byteRoiBoxResized, 0, this.ResizedRoiWidth * this.ResizedRoiHeight);
                    }
                }

                for (int y = this.RoiY + ptOffsetChip.Y; y < this.RoiY + this.RoiHeight - this.ChipHeight + 1; y += this.ChipHeight)
                {
                    this.MapHeight = this.MapHeight + 1;
                    for (int x = this.RoiX + ptOffsetChip.X; x < this.RoiX + this.RoiWidth - this.ChipWidth + 1; x += this.ChipWidth)
                    {
                        ptSearchedChip = GetDiffSum(byteChipBoxResized, byteRoiBoxResized, new CPoint(x, y), this.ResizeScaleFactor);
                        if (ptSearchedChip.Z > 0)
                        {
                            if (Math.Abs(ptSearchedChip.X - ptOriginChip.X) > 5 * this.ResizeScaleFactor || Math.Abs(ptSearchedChip.Y - ptOriginChip.Y) > 5 * this.ResizeScaleFactor) // exception for offset pixel error range
                            {
                                this.ptOffset.Add(ptSearchedChip);
                            }
                            else
                            {
                                this.ptOffset.Add(new C3Point(ptOriginChip.X, ptOriginChip.Y, 100));
                            }
                            chipCount = chipCount + 1;
                            tempWaferMap[idx] = 1;
                        }
                        else
                        {
                            this.ptOffset.Add(ptSearchedChip);
                            tempWaferMap[idx] = 0;
                        }
                        idx = idx + 1;
                    }
                }

                this.MapWidth = idx / this.MapHeight;
                this.SearchedWaferMap = new int[this.MapWidth * this.MapHeight];

                for (int j = 0; j < this.MapHeight; j++)
                {
                    for (int i = 0; i < this.MapWidth; i++)
                    {
                        this.SearchedWaferMap[this.MapWidth * j + i] = tempWaferMap[this.MapWidth * j + i];
                    }
                }

                this.ImageViewerVM.searchedChipPoint = ptOffset;
                this.ImageViewerVM.MapWidth = this.MapWidth;
                this.ImageViewerVM.MapHeight = this.MapHeight;
                this.ImageViewerVM.SearchedWaferMap = this.SearchedWaferMap;

                foreach (var item in this.ptOffset)
                {
                    this.ImageViewerVM.DrawSearchedChipBox(item);
                }
                this.ImageViewerVM.RedrawShapes();
                this.ImageViewerVM.IsFindDone = true;
                MessageBox.Show("Found " + chipCount.ToString() + " Chip(s)!");

                RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
                //Array.Copy(this.SearchedWaferMap, this.SearchedWaferMap, this.MapWidth * this.MapHeight);
                waferMap.CreateWaferMap(this.MapWidth, this.MapHeight, this.SearchedWaferMap);
            }
            else
            {
                this.ImageViewerVM.IsFindDone = false;
                MessageBox.Show("Check Chip and ROI Location!");
            }
        }

        public static byte[] GetBitmapToArray(Bitmap bitmap, bool isColorMode = false)
        {
            BitmapData bmpdata = null;

            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes;
                if (isColorMode == true)
                {
                    numbytes = bmpdata.Stride * bitmap.Height;
                }
                else
                {
                    numbytes = bmpdata.Width * bitmap.Height;
                }
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                if (isColorMode == true)
                {
                    Marshal.Copy(ptr, bytedata, 0, numbytes);
                }
                else
                {
                    for (int j = 0; j < bmpdata.Height; j++)
                    {
                        for (int i = 0; i < bmpdata.Width; i++)
                        {
                            Marshal.Copy(ptr + 4 * j * bmpdata.Width + 4 * i, bytedata, j * bmpdata.Width + i, 1);
                        }
                    }
                }

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }
        }

        public C3Point GetDiffSum(byte[] byteChipBoxResized, byte[] byteRoiBoxResized, CPoint ptStart, double resizeFactor)
        {
            UInt64 TotalSum = 0;
            UInt64 PrevMinSum = 18446744073709551615;
            C3Point ptOriginChip = new C3Point();
            CPoint ptStartResized = new CPoint((int)((ptStart.X - this.RoiX) / resizeFactor), (int)((ptStart.Y - this.RoiY) / resizeFactor));

            int nSize = this.ResizedChipHeight * this.ResizedChipWidth;
            double m_fSimilarityRev = (double)100 / (double)255 / nSize;
            UInt64 m_lSimilarity = (UInt64)((255 - this.ChipSimilarity * 255 / 100) * nSize);
            int m_btScore = 0;


            for (int y = ptStartResized.Y; y <= ptStartResized.Y; y++)
            {
                for (int x = ptStartResized.X; x <= ptStartResized.X; x++)
                {
                    for (int j = 0; j < this.ResizedChipHeight; j++)
                    {
                        for (int i = 0; i < this.ResizedChipWidth; i++)
                        {
                            TotalSum = TotalSum + (UInt64)Math.Abs(byteChipBoxResized[this.ResizedChipWidth * j + i] - byteRoiBoxResized[this.ResizedRoiWidth * (y + j) + (x + i)]);
                        }
                    }
                    if (TotalSum < PrevMinSum)
                    {
                        PrevMinSum = TotalSum;
                        ptOriginChip.X = (int)(x * resizeFactor) + this.RoiX;
                        ptOriginChip.Y = (int)(y * resizeFactor) + this.RoiY;
                    }
                    TotalSum = 0;
                }
            }

            m_btScore = (int)(100 - (PrevMinSum * m_fSimilarityRev));
            if (m_btScore > this.ChipSimilarity)
            {
                ptOriginChip.Z = m_btScore;
            }
            else
            {
                ptOriginChip.Z = 0;
            }
            return ptOriginChip;
        }
        #endregion
    }
}
