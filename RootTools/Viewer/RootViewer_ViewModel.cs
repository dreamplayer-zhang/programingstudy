using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RootTools
{
    public class RootViewer_ViewModel : ObservableObject
    {
        public RootViewer_ViewModel()
        {
        }

        public void init(ImageData image = null, IDialogService dialogService = null)
        {
            if (image != null)
            {
                p_ImageData = image;
                image.OnCreateNewImage += image_NewImage;
                image.OnUpdateImage += image_OnUpdateImage;
                image.UpdateOpenProgress += image_UpdateOpenProgress;
                SetRoiRect();
                InitRoiRect(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
                SetImageSource();

            }
            if (dialogService != null)
            {
                m_DialogService = dialogService;
            }
            InitCrossLine();
        }
        private IDialogService m_DialogService;

        StopWatch m_swMouse = new StopWatch();
        CPoint m_ptViewBuffer = new CPoint();
        CPoint m_ptMouseBuffer = new CPoint();
        Line Horizon, Vertical;


        public System.Windows.Input.KeyEventArgs m_KeyEvent;

        Key m_keyMove = Key.LeftCtrl;
        Key m_keyZoom = Key.LeftCtrl;
        Key m_keyDrawBasic = Key.LeftShift;
        public enum eColorViewMode
        {
            All,
            R,
            G,
            B,
        }
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
                SetImageSource();
            }
        }
        #region Property
        /// <summary>
        /// Global UI Shapes
        /// </summary>
        public ObservableCollection<UIElement> p_ViewElement
        {
            get
            {
                return m_ViewElement;
            }
            set
            {
                m_ViewElement = value;
            }
        }
        private ObservableCollection<UIElement> m_ViewElement = new ObservableCollection<UIElement>();
        /// <summary>
        /// Main Image Data in Viewer
        /// </summary>
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
        private ImageData m_ImageData;
        /// <summary>
        /// ROI 4Channel BitmapLayer
        /// </summary>
        public ImageData p_ROILayer
        {
            get
            {
                return m_ROILayer;
            }
            set
            {
                SetProperty(ref m_ROILayer, value);
            }
        }
        private ImageData m_ROILayer;
        /// <summary>
        /// Image Offset in Memory
        /// </summary>
        public CPoint p_CopyOffset
        {
            get
            {
                return m_CopyOffset;
            }
            set
            {
                SetProperty(ref m_CopyOffset, value);
            }
        }
        private CPoint m_CopyOffset = new CPoint(0, 0);

        /// <summary>
        /// Image Load Prgress
        /// </summary>
        public int p_nProgress
        {
            get
            {
                return m_nProgress;
            }
            set
            {
                SetProperty(ref m_nProgress, value);
            }
        }
        private int m_nProgress = 0;

        #region View
        private BitmapSource m_imgSource;
        public BitmapSource p_ImgSource
        {
            get
            {
                return m_imgSource;
            }
            set
            {
                SetProperty(ref m_imgSource, value);
            }
        }
        private BitmapSource m_LayerSource;
        public BitmapSource p_LayerSource
        {
            get
            {
                return m_LayerSource;
            }
            set
            {
                SetProperty(ref m_LayerSource, value);
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
        private int m_CanvasWidth = 100;
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
                SetRoiRect();
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
                SetRoiRect();
            }
        }
        #endregion

        #region Thumbnail
        private BitmapSource m_ThumNailImgSource;
        public BitmapSource p_ThumNailImgSource
        {
            get
            {
                return m_ThumNailImgSource;
            }
            set
            {
                SetProperty(ref m_ThumNailImgSource, value);
            }
        }

        private System.Drawing.Rectangle m_TumbnailImg_Rect = new System.Drawing.Rectangle();
        public System.Drawing.Rectangle p_TumbnailImg_Rect
        {
            get
            {
                return m_TumbnailImg_Rect;
            }
            set
            {
                SetProperty(ref m_TumbnailImg_Rect, value);
            }
        }

        private Thickness m_TumbnailImgMargin = new Thickness(0, 0, 0, 0);
        public Thickness p_TumbnailImgMargin // TumbnailRect Margin
        {
            get
            {
                return m_TumbnailImgMargin;
            }
            set
            {
                SetProperty(ref m_TumbnailImgMargin, value);
            }
        }

        private int m_ThumbWidth = 100;
        public int p_ThumbWidth
        {
            get
            {
                return m_ThumbWidth;
            }
            set
            {
                SetProperty(ref m_ThumbWidth, value);
            }
        }
        private int m_ThumbHeight = 100;
        public int p_ThumbHeight
        {
            get
            {
                return m_ThumbHeight;
            }
            set
            {
                //_CanvasHeight = value / 10 * 10;
                //RaisePropertyChanged();
                SetProperty(ref m_ThumbHeight, value);
            }
        }

        #endregion

        #region Mouse
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
        private int m_MouseX = 0;
        public int p_MouseX
        {
            get
            {
                return m_MouseX;
            }
            set
            {
                if (p_ImgSource != null)
                {
                    if (p_CanvasWidth != 0 && p_CanvasHeight != 0)
                    {
                        if (p_MouseX < p_ImgSource.Width && p_MouseY < p_ImgSource.Height)
                        {
                            if (p_ImgSource.Format.BitsPerPixel == 24)
                            {
                                System.Windows.Media.Color c_Pixel = GetPixelColor(p_ImgSource, p_MouseX, p_MouseY);
                                p_PixelData = "R " + c_Pixel.R + " G " + c_Pixel.G + " B " + c_Pixel.B;
                            }
                            else if (p_ImgSource.Format.BitsPerPixel == 8)
                            {
                                byte[] pixel = new byte[1];
                                p_ImgSource.CopyPixels(new Int32Rect(p_MouseX, p_MouseY, 1, 1), pixel, 1, 0);
                                p_PixelData = "GV " + pixel[0];
                            }
                            p_MouseMemY = p_View_Rect.Y + p_MouseY * p_View_Rect.Height / p_CanvasHeight; 
                            p_MouseMemX = p_View_Rect.X + p_MouseX * p_View_Rect.Width / p_CanvasWidth;
                        }
                    }
                }

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
        private int m_MouseMemX = 0;
        public int p_MouseMemX
        {
            get
            {
                return m_MouseMemX;
            }
            set
            {
                SetProperty(ref m_MouseMemX, value);
            }
        }
        private int m_MouseMemY = 0;
        public int p_MouseMemY
        {
            get
            {
                return m_MouseMemY;
            }
            set
            {
                SetProperty(ref m_MouseMemY, value);
            }
        }
        private int m_TumbMouseX = 0;
        public int p_TumbMouseX
        {
            get
            {
                return m_TumbMouseX;
            }
            set
            {
                SetProperty(ref m_TumbMouseX, value);
            }
        }
        private int m_TumbMouseY = 0;
        public int p_TumbMouseY
        {
            get
            {
                return m_TumbMouseY;
            }
            set
            {
                SetProperty(ref m_TumbMouseY, value);
            }
        }

        private string m_PixelData = "0";
        public string p_PixelData
        {
            get
            {
                return m_PixelData;
            }
            set
            {
                SetProperty(ref m_PixelData, value);
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
                SetRoiRect();
            }
        }
        #endregion

        #region Visibility
        private Visibility m_VisibleMenu = Visibility.Visible;
        public Visibility p_VisibleMenu
        {
            get
            {
                return m_VisibleMenu;
            }
            set
            {
                SetProperty(ref m_VisibleMenu, value);
            }
        }

        #endregion

        #region 레시피툴 아이디어
        // Stack
        // Object -> Mem Point / w,h

        // 이동 -> 1개의 동작
        // 그리기 시작/ 그리는 중/ 그리기 완료 -> 1개의 동작
        // List<Object> 변경사항 -> 1개의 동작

        // List<Object> Property Set(동작완료)들어왔을때  -> Stack에 저장 
        //-> Canvas 좌표로 p_Zoom비례 변환하면서 Obs<UIElement>로 복사 (이게 Redraw?)

        // 확대/축소 -> Redraw?
        // 확대/축소 이 후 실행취소는?
        // List<Object> = Stack<이전꺼> 하면됨?
        // -> Set으로 들어오니까 다시 Redraw 자동으로되나?


        // 그리기/선택/수정은 1개의 클래스?
        // 그리기
        // 선택 Mode에따라 List<Object>의 Object 생성(Rect,Ellipse, Line)
        // Mouse Down => Object.StartPt = Mem Mouse Pt
        // Mouse Move => Object.w/h = Now Mem Mouse Pt
        // Mouse UP  => Object.w/h = Now Mem Mouse Pt
        // List<Object>.Add(Object) 
        // Set -> Undo(Stack) -> Redraw -> UI반영

        // 선택 / 수정
        // Redraw 할 때 각 UIElement객체의 Mouse Enter Event 생성?
        // Mouse Enter & Mouse Down = 선택? // isMouseOver && Mouse Down = 선택?

        // ModifyManger 


        // 복사/붙여넣기
        // 그리기/선택(복수)/수정(복수)
        #endregion

        #endregion

        #region Image Method
        public virtual void SetImageData(ImageData image)
        {
            p_ImageData = null;
            p_ImageData = image;
            image.OnCreateNewImage += image_NewImage;
            image.OnUpdateImage += image_OnUpdateImage;
            image.UpdateOpenProgress += image_UpdateOpenProgress;
            InitRoiRect(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
            SetImageSource();
        }
        void image_OnUpdateImage()
        {
            SetImageSource();
        }
        void image_UpdateOpenProgress(int nInt)
        {
            p_nProgress = nInt;
        }
        void image_NewImage()
        {
            SetRoiRect();
        }

        public unsafe void SetImageSource()
        {
            try
            {
                if (p_ImageData != null)
                {
                    if (p_ImageData.m_eMode == ImageData.eMode.OtherPCMem)
                    {
                        Image<Gray, byte> view = new Image<Gray, byte>(p_CanvasWidth, p_CanvasHeight);
                        byte[,,] viewptr = view.Data;
                        byte[] image = p_ImageData.GetData(p_View_Rect,p_CanvasWidth, p_CanvasHeight);
                        for (int xx = 0; xx < p_CanvasWidth; xx++)
                        {   
                            viewptr[xx, xx, 0] = image[p_View_Rect.Width * xx];
                        }
                        p_ImgSource = ImageHelper.ToBitmapSource(view);
                    }
                    else
                    {
                        object o = new object();
                        if (p_ImageData.p_nByte == 1)
                        {
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
                                        viewptr[yy, xx, 0] = ((byte*)ptrMem)[pix_x + (long)pix_y * sizeX];
                                    }
                                }
                            });

                            p_ImgSource = ImageHelper.ToBitmapSource(view);

                            p_TumbnailImgMargin = new Thickness(Convert.ToInt32((double)p_View_Rect.X * p_ThumbWidth / p_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Y * p_ThumbHeight / p_ImageData.p_Size.Y), 0, 0);

                            if (Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / p_ImageData.p_Size.Y) == 0)
                                p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / p_ImageData.p_Size.X), 2);
                            else
                                p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / p_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / p_ImageData.p_Size.Y));

                        }
                        else if (p_ImageData.p_nByte == 3)
                        {
                            if (p_eColorViewMode == eColorViewMode.All)
                            {
                                Image<Rgb, byte> view = new Image<Rgb, byte>(p_CanvasWidth, p_CanvasHeight);
                                IntPtr ptrMemR = p_ImageData.GetPtr(0);
                                IntPtr ptrMemG = p_ImageData.GetPtr(1);
                                IntPtr ptrMemB = p_ImageData.GetPtr(2);


                                if (ptrMemR == IntPtr.Zero)
                                    return;

                                byte[,,] viewPtr = view.Data;
                                byte* imageptrR = (byte*)ptrMemR.ToPointer();
                                byte* imageptrG = (byte*)ptrMemG.ToPointer();
                                byte* imageptrB = (byte*)ptrMemB.ToPointer();

                                int viewrectY = p_View_Rect.Y;
                                int viewrectX = p_View_Rect.X;
                                int viewrectHeight = p_View_Rect.Height;
                                int viewrectWidth = p_View_Rect.Width;
                                int sizeX = p_ImageData.p_Size.X;

                                if (imageptrR == null)
                                    return;
                                if (imageptrG == null)
                                    return;
                                if (imageptrB == null)
                                    return;
                                Parallel.For(0, p_CanvasHeight, (yy) =>
                                {
                                    //lock (o)
                                    {
                                        long pix_y = viewrectY + yy * viewrectHeight / p_CanvasHeight;
                                        for (int xx = 0; xx < p_CanvasWidth; xx++)
                                        {
                                            long pix_x = viewrectX + xx * viewrectWidth / p_CanvasWidth;

                                            viewPtr[yy, xx, 0] = imageptrR[pix_x + (long)pix_y * sizeX];
                                            viewPtr[yy, xx, 1] = imageptrG[pix_x + (long)pix_y * sizeX];
                                            viewPtr[yy, xx, 2] = imageptrB[pix_x + (long)pix_y * sizeX];
                                        }
                                    }
                                });

                                p_ImgSource = ImageHelper.ToBitmapSource(view);
                            }
                            else
                            {
                                Image<Gray, byte> view = new Image<Gray, byte>(p_CanvasWidth, p_CanvasHeight);
                                IntPtr ptrMem = IntPtr.Zero;
                                switch (p_eColorViewMode)
                                {
                                    case eColorViewMode.R:
                                        ptrMem = p_ImageData.GetPtr(0);
                                        break;
                                    case eColorViewMode.G:
                                        ptrMem = p_ImageData.GetPtr(1);
                                        break;
                                    case eColorViewMode.B:
                                        ptrMem = p_ImageData.GetPtr(2);
                                        break;
                                }

                                if (ptrMem == IntPtr.Zero)
                                    return;

                                byte[,,] viewPtr = view.Data;
                                byte* imageptr = (byte*)ptrMem.ToPointer();
                                
                                int viewrectY = p_View_Rect.Y;
                                int viewrectX = p_View_Rect.X;
                                int viewrectHeight = p_View_Rect.Height;
                                int viewrectWidth = p_View_Rect.Width;
                                int sizeX = p_ImageData.p_Size.X;

                                Parallel.For(0, p_CanvasHeight, (yy) =>
                                {
                                    //lock (o)
                                    {
                                        long pix_y = viewrectY + yy * viewrectHeight / p_CanvasHeight;
                                        for (int xx = 0; xx < p_CanvasWidth; xx++)
                                        {
                                            long pix_x = viewrectX + xx * viewrectWidth / p_CanvasWidth;
                                            viewPtr[yy, xx, 0] = imageptr[pix_x + (long)pix_y * sizeX];
                                        }
                                    }
                                });

                                p_ImgSource = ImageHelper.ToBitmapSource(view);
                            }
                        }
                    }
                }
                if (p_ROILayer != null)
                    SetLayerSource();
            }
            catch (Exception ee)
            {
                System.Windows.MessageBox.Show(ee.ToString());
            }

        }
        public virtual unsafe void SetLayerSource()
        {
            try
            {
                if (p_ROILayer != null)
                {
                    object o = new object();
                    if (p_ROILayer.p_nByte == 1)
                    {
                        Image<Gray, byte> view = new Image<Gray, byte>(p_CanvasWidth, p_CanvasHeight);

                        IntPtr ptrMem = p_ROILayer.GetPtr();
                        if (ptrMem == IntPtr.Zero)
                            return;

                        int rectX, rectY, rectWidth, rectHeight, sizeX;
                        byte[,,] viewptr = view.Data;
                        byte* imageptr = (byte*)ptrMem.ToPointer();

                        rectX = p_View_Rect.X;
                        rectY = p_View_Rect.Y;
                        rectWidth = p_View_Rect.Width;
                        rectHeight = p_View_Rect.Height;

                        sizeX = p_ROILayer.p_Size.X;

                        Parallel.For(0, p_CanvasHeight, (yy) =>
                        {
                           // lock (o)
                            {
                                long pix_y = rectY + yy * rectHeight / p_CanvasHeight;

                                for (int xx = 0; xx < p_CanvasWidth; xx++)
                                {
                                    long pix_x = rectX + xx * rectWidth / p_CanvasWidth;
                                    viewptr[yy, xx, 0] = imageptr[pix_x + pix_y * sizeX];
                                }
                            }
                        });

                        p_LayerSource = ImageHelper.ToBitmapSource(view);
                    }
                    if (p_ROILayer.p_nByte == 4)
                    {
                        Image<Rgba, byte> view = new Image<Rgba, byte>(p_CanvasWidth, p_CanvasHeight);
                        byte[,,] viewPtr = view.Data;

                        IntPtr ptrMem = p_ROILayer.GetPtr();

                        if (ptrMem == IntPtr.Zero)
                            return;

                        byte* imageptr = (byte*)ptrMem.ToPointer();

                        int viewrectY = p_View_Rect.Y;
                        int viewrectX = p_View_Rect.X;
                        int viewrectHeight = p_View_Rect.Height;
                        int viewrectWidth = p_View_Rect.Width;
                        int sizeX = p_ROILayer.p_Size.X;

                        Parallel.For(1, p_CanvasHeight, (yy) =>
                        {
                            {
                                long pix_y = viewrectY + yy * viewrectHeight / p_CanvasHeight;
                                long pix_rect = pix_y * sizeX;
                                for (int xx = 0; xx < p_CanvasWidth; xx++)
                                {
                                    long pix_x = viewrectX + xx * viewrectWidth / p_CanvasWidth;

                                    viewPtr[yy, xx, 3] = imageptr[3 + 4 * (pix_x + pix_rect)]; //0;
                                    viewPtr[yy, xx, 2] = imageptr[2 + 4 * (pix_x + pix_rect)]; //0;//imageptr[0 + 3 * (pix_x + pix_rect)];
                                    viewPtr[yy, xx, 1] = imageptr[1 + 4 * (pix_x + pix_rect)]; //0;//imageptr[1 + 3 * (pix_x + pix_rect)];
                                    viewPtr[yy, xx, 0] = imageptr[0 + 4 * (pix_x + pix_rect)]; //0;//imageptr[2 + 3 * (pix_x + pix_rect)];
                                }
                            }
                        });
                        byte[] pixels1d = new byte[p_CanvasHeight * p_CanvasWidth * 4];
                        WriteableBitmap wbitmap = new WriteableBitmap(p_CanvasWidth, p_CanvasHeight, 96, 96, PixelFormats.Bgra32, null);
                        int index = 0;
                        for (int row = 0; row < p_CanvasHeight; row++)
                        {
                            for (int col = 0; col < p_CanvasWidth; col++)
                            {
                                for (int i = 0; i < 4; i++)
                                    pixels1d[index++] = viewPtr[row, col, i];
                            }
                        }
                        Int32Rect rect = new Int32Rect(0, 0, p_CanvasWidth, p_CanvasHeight);
                        int stride = 4 * p_CanvasWidth;
                        wbitmap.WritePixels(rect, pixels1d, stride, 0);
                        p_LayerSource = wbitmap;
                        //p_LayerSource = ImageHelper.ToBitmapSource(view);
                    }

                }
            }
            catch (Exception ee)
            {
                System.Windows.MessageBox.Show(ee.ToString());
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
            SetThumNailIamgeSource();
        }
        public virtual void SetRoiRect()
        {
            if (p_ImageData != null)
            {
                CPoint StartPt = GetStartPoint_Center(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
                bool bRatio_WH = false;
                //if (p_ImageData.p_nByte == 1)
                    bRatio_WH = (double)p_ImageData.p_Size.X / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;
                //else
                //    bRatio_WH = (double)p_ImageData.p_Size.X / 3 / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;

                //bool bRatio_WH = (double)p_ImageData.p_Size.X / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;
                if (bRatio_WH)
                { //세로가 길어
                    p_View_Rect = new System.Drawing.Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom), Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom * p_CanvasHeight / p_CanvasWidth));
                }
                else
                {
                    p_View_Rect = new System.Drawing.Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom * p_CanvasWidth / p_ImageData.p_nByte / p_CanvasHeight), Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom));
                }
                //if (p_View_Rect.Height % 2 != 0)
                //{
                //    m_View_Rect.Height += 1;
                //}
                SetImageSource();
            }
        }
        void SetViewRect(CPoint point)      //point image의 좌상단
        {
            bool bRatio_WH = false;
            //if (p_ImageData.p_nByte == 1)
                bRatio_WH = (double)p_ImageData.p_Size.X / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;
            //else
            //    bRatio_WH = (double)p_ImageData.p_Size.X /3 / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;

            if (bRatio_WH)
            { //세로가 길어
             //   if (p_ImageData.p_nByte == 1)
                    p_View_Rect = new System.Drawing.Rectangle(point.X, point.Y, Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom), Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom * p_CanvasHeight / p_CanvasWidth));
                //else
                //    p_View_Rect = new System.Drawing.Rectangle(point.X, point.Y, Convert.ToInt32(p_ImageData.p_Size.X/3 * p_Zoom), Convert.ToInt32(p_ImageData.p_Size.X/3 * p_Zoom * p_CanvasHeight / p_CanvasWidth));
            }
            else
            {
                //if (p_ImageData.p_nByte == 1)
                    p_View_Rect = new System.Drawing.Rectangle(point.X, point.Y, Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom * p_CanvasWidth / p_CanvasHeight), Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom));
                //else
                //    p_View_Rect = new System.Drawing.Rectangle(point.X, point.Y, Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom * p_CanvasWidth/3 / p_CanvasHeight), Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom));
            }
        }
        public unsafe void SetThumNailIamgeSource()
        {
            if (p_ImageData.p_nByte == 1)
            {
                Image<Gray, byte> view = new Image<Gray, byte>(p_ThumbWidth, p_ThumbHeight);
                IntPtr ptrMem = p_ImageData.GetPtr();
                if (ptrMem == IntPtr.Zero)
                    return;
                int pix_x = 0;
                int pix_y = 0;

                for (int yy = 0; yy < p_ThumbHeight; yy++)
                {
                    for (int xx = 0; xx < p_ThumbWidth; xx++)
                    {
                        pix_x = xx * p_ImageData.p_Size.X / p_ThumbWidth;
                        pix_y = yy * p_ImageData.p_Size.Y / p_ThumbHeight;
                        view.Data[yy, xx, 0] = ((byte*)ptrMem)[pix_x + (long)pix_y * p_ImageData.p_Size.X];
                    }
                }
                if (view.Width != 0 && view.Height != 0)
                    p_ThumNailImgSource = ImageHelper.ToBitmapSource(view);
            }
            else if (p_ImageData.p_nByte == 3)
            {
                Image<Rgb, byte> view = new Image<Rgb, byte>(p_ThumbWidth, p_ThumbHeight);
                IntPtr ptrMem = p_ImageData.GetPtr();
                if (ptrMem == IntPtr.Zero)
                    return;
                int pix_x = 0;
                int pix_y = 0;

                for (int yy = 0; yy < p_ThumbHeight; yy++)
                {
                    pix_y = yy * p_ImageData.p_Size.Y / p_ThumbHeight;
                    for (int xx = 0; xx < p_ThumbWidth; xx++)
                    {
                        pix_x = xx * p_ImageData.p_Size.X / p_ThumbWidth;
                        view.Data[yy, xx, 2] = ((byte*)ptrMem)[0 + p_ImageData.p_nByte * (pix_x + (long)pix_y * p_ImageData.p_Size.X)];
                        view.Data[yy, xx, 1] = ((byte*)ptrMem)[1 + p_ImageData.p_nByte * (pix_x + (long)pix_y * p_ImageData.p_Size.X)];
                        view.Data[yy, xx, 0] = ((byte*)ptrMem)[2 + p_ImageData.p_nByte * (pix_x + (long)pix_y * p_ImageData.p_Size.X)];
                    }
                }
                if (view.Width != 0 && view.Height != 0)
                    p_ThumNailImgSource = ImageHelper.ToBitmapSource(view);
            }

        }
        public System.Windows.Media.Color GetPixelColor(BitmapSource source, int x, int y)
        {
            System.Windows.Media.Color c = System.Windows.Media.Colors.White;
            if (source != null)
            {
                try
                {
                    CroppedBitmap cb = new CroppedBitmap(source, new Int32Rect(x, y, 1, 1));
                    var pixels = new byte[4];
                    cb.CopyPixels(pixels, 4, 0);
                    c = System.Windows.Media.Color.FromRgb(pixels[2], pixels[1], pixels[0]);
                }
                catch (Exception) { }
            }
            return c;
        }
        #endregion

        #region Draw Method
        public virtual unsafe void CropRectSetData(ImageData imageData, CRect nowRect, CPoint offset = null)
        {
            IntPtr ptrMem = p_ROILayer.GetPtr();
            if (ptrMem == IntPtr.Zero)
                return;

            CPoint StartPt = new CPoint(nowRect.Left - offset.X, nowRect.Top - offset.Y);
            
            IntPtr rectPtr = (IntPtr)((long)ptrMem + (long)StartPt.Y * p_ROILayer.p_Size.X * p_ROILayer.p_nByte + StartPt.X * p_ROILayer.p_nByte);
            for (int i = 0; i < nowRect.Height; i++)
            {
                Marshal.Copy(imageData.m_aBuf, i * nowRect.Width*4, (IntPtr)((long)rectPtr + (long)p_ROILayer.p_Size.X * p_ROILayer.p_nByte * i), nowRect.Width * 4);
            }

        }
        public virtual unsafe void DrawRectBitmap(CRect rect, byte r, byte g, byte b, byte a, CPoint offset = null)
        {
            Parallel.For(rect.Left, rect.Right + 1, x =>
              {
                  Parallel.For(rect.Top, rect.Bottom + 1, y =>
                  {
                      CPoint pixelPt = new CPoint(x - offset.X, y - offset.Y);
                      DrawPixelBitmap(pixelPt, r, g, b, a);
                  });
              });
        }
        public virtual unsafe void DrawPixelBitmap(CPoint memPt, byte r, byte g, byte b, byte a)
        {
            IntPtr ptrMem = p_ROILayer.GetPtr();
            if (ptrMem == IntPtr.Zero)
                return;
            byte* imagePtr = (byte*)ptrMem.ToPointer();

            imagePtr[(memPt.Y * p_ROILayer.p_Size.X + memPt.X) * 4 + 0] = b; // b
            imagePtr[(memPt.Y * p_ROILayer.p_Size.X + memPt.X) * 4 + 1] = g; // g
            imagePtr[(memPt.Y * p_ROILayer.p_Size.X + memPt.X) * 4 + 2] = r; // r
            imagePtr[(memPt.Y * p_ROILayer.p_Size.X + memPt.X) * 4 + 3] = a; // a
        }
        public virtual unsafe void DrawPixelBitmap(CPoint memPt, byte value, CPoint offset = null)
        {
            IntPtr ptrMem = p_ROILayer.GetPtr();
            if (ptrMem == IntPtr.Zero)
                return;
            byte* imagePtr = (byte*)ptrMem.ToPointer();

            imagePtr[memPt.Y * p_ROILayer.p_Size.X + memPt.X] = value;
        }
        #endregion

        #region Mouse Method
        public virtual void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            //CPoint StartPt = GetCurrentPoint();
            if (p_ImageData == null) return;

            CPoint MovePoint = new CPoint();
            MovePoint.X = point.X + p_View_Rect.Width * nX / p_CanvasWidth;
            MovePoint.Y = point.Y + p_View_Rect.Height * nY / p_CanvasHeight;
            //if (p_ImageData.p_nByte == 1)
            //{
                if (MovePoint.X < 0)
                    MovePoint.X = 0;
                else if (MovePoint.X > p_ImageData.p_Size.X - p_View_Rect.Width)
                    MovePoint.X = p_ImageData.p_Size.X - p_View_Rect.Width;
                if (MovePoint.Y < 0)
                    MovePoint.Y = 0;
                else if (MovePoint.Y > p_ImageData.p_Size.Y - p_View_Rect.Height)
                    MovePoint.Y = p_ImageData.p_Size.Y - p_View_Rect.Height;
            //}
            //else
            //{
            //    if (MovePoint.X < 0)
            //        MovePoint.X = 0;
            //    else if (MovePoint.X > p_ImageData.p_Size.X - p_View_Rect.Width)
            //        MovePoint.X = p_ImageData.p_Size.X - p_View_Rect.Width;
            //    if (MovePoint.Y < 0)
            //        MovePoint.Y = 0;
            //    else if (MovePoint.Y > p_ImageData.p_Size.Y - p_View_Rect.Height)
            //        MovePoint.Y = p_ImageData.p_Size.Y - p_View_Rect.Height;
            //}
            SetViewRect(MovePoint);
            SetImageSource();
        }
        void CanvasMoveMousePoint()
        {
            CPoint StartPt = GetStartPoint(p_MouseX, p_MouseY);
            SetViewRect(StartPt);
            SetImageSource();
        }
        void CanvasMoveCanvasPoint(int mouseX, int mouseY)
        {
            Thread.Sleep(50);
            CPoint StartPt = GetStartPoint(mouseX, mouseY);
            SetViewRect(StartPt);
            SetImageSource();
        }
        void CanvasMovePoint(double nPercentX, double nPercentY)        // 0.xxx
        {
            CPoint StartPt = new CPoint();
            StartPt.X = Convert.ToInt32(p_ImageData.p_Size.X * nPercentX - p_View_Rect.Width / 2);
            StartPt.Y = Convert.ToInt32(p_ImageData.p_Size.Y * nPercentY - p_View_Rect.Height / 2);
            if (StartPt.X < 0)
                StartPt.X = 0;
            else if (StartPt.X > p_ImageData.p_Size.X - p_View_Rect.Width)
                StartPt.X = p_ImageData.p_Size.X - p_View_Rect.Width;
            if (StartPt.Y < 0)
                StartPt.Y = 0;
            else if (StartPt.Y > p_ImageData.p_Size.Y - p_View_Rect.Height)
                StartPt.Y = p_ImageData.p_Size.Y - p_View_Rect.Height;
            SetViewRect(StartPt);
            SetImageSource();

        }
        CPoint GetStartPoint(int MouseX, int MouseY)
        {
            int nX = p_View_Rect.X + p_View_Rect.Width * MouseX / p_CanvasWidth - p_View_Rect.Width / 2;
            int nY = p_View_Rect.Y + p_View_Rect.Height * MouseY / p_CanvasHeight - p_View_Rect.Height / 2;

            if (nX < 0)
                nX = 0;
            else if (nX > p_ImageData.p_Size.X - p_View_Rect.Width)
                nX = p_ImageData.p_Size.X - p_View_Rect.Width;
            if (nY < 0)
                nY = 0;
            else if (nY > p_ImageData.p_Size.Y - p_View_Rect.Height)
                nY = p_ImageData.p_Size.Y - p_View_Rect.Height;
            return new CPoint(nX, nY);
        }
        CPoint GetStartPoint_Center(int nImgWidth, int nImgHeight)
        {
            bool bRatio_WH;
            //if(p_ImageData.p_nByte == 3)
            //    bRatio_WH = (double)p_ImageData.p_Size.X / 3 / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;
            //else
                bRatio_WH = (double)p_ImageData.p_Size.X  / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;
            int viewrectwidth = 0;
            int viewrectheight = 0;
            int nX = 0;
            int nY = 0;
            if (bRatio_WH)
            { //세로가 길어
              //nX = p_View_Rect.X + Convert.ToInt32(p_View_Rect.Width - nImgWidth * p_Zoom) /2; 기존 중앙기준으로 확대/축소되는 코드. 
                nX = p_View_Rect.X + Convert.ToInt32(p_View_Rect.Width - nImgWidth * p_Zoom) * p_MouseX / p_CanvasWidth; // 마우스 커서기준으로 확대/축소
                nY = p_View_Rect.Y + Convert.ToInt32(p_View_Rect.Height - nImgWidth  * p_Zoom * p_CanvasHeight / p_CanvasWidth) * p_MouseY / p_CanvasHeight;
                viewrectwidth = Convert.ToInt32(nImgWidth  * p_Zoom);
                viewrectheight = Convert.ToInt32(nImgWidth* p_Zoom * p_CanvasHeight / p_CanvasWidth);
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
            else if (nX > nImgWidth  - viewrectwidth)
                nX = nImgWidth - viewrectwidth;
            if (nY < 0)
                nY = 0;
            else if (nY > nImgHeight - viewrectheight)
                nY = nImgHeight - viewrectheight;
            return new CPoint(nX, nY);
        }

        public void InitCrossLine()
        {
            if (p_ViewElement.Contains(Vertical) && p_ViewElement.Contains(Horizon))
            {
                p_ViewElement.Remove(Vertical);
                p_ViewElement.Remove(Horizon);
            }

            Vertical = new Line();
            Horizon = new Line();

            Brush LineBrush = Brushes.Silver;
            double LineThick = 1;
            DoubleCollection LineDash = new DoubleCollection { 3, 4 };

            Vertical.Stroke = LineBrush;
            Vertical.StrokeThickness = LineThick;
            Vertical.StrokeDashArray = LineDash;
            Horizon.Stroke = LineBrush;
            Horizon.StrokeThickness = LineThick;
            Horizon.StrokeDashArray = LineDash;

            p_ViewElement.Add(Vertical);
            p_ViewElement.Add(Horizon);
        }
        public void DrawCrossLine(CPoint canvasPt)
        {
            try
            {
                if (Vertical == null || Horizon == null)
                    return;
                Vertical.X1 = canvasPt.X;
                Vertical.X2 = canvasPt.X;

                Horizon.Y1 = canvasPt.Y;
                Horizon.Y2 = canvasPt.Y;


                Vertical.Y2 = p_CanvasHeight;
                Horizon.X2 = p_CanvasWidth;
            }
            catch (Exception e)
            {
                return;
            }
            return;
        }
        #endregion

        #region ICommand
        public virtual void _openImage()
        {
            if (p_ImageData == null)
            {
                System.Windows.Forms.MessageBox.Show("Image를 열어주세요");
                return;
            }
            _CancelCopy();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files(*.bmp;*.jpg)|*.bmp;*.jpg";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var viewModel = new Dialog_ImageOpenViewModel(this as RootViewer_ViewModel);
                Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
                if (result.HasValue)
                {
                    if (result.Value)
                    {
                        p_ImageData.OpenFile(ofd.FileName, p_CopyOffset);
                    }
                    else
                    {
                        // Cancelled
                    }
                }
            }
        }
        void _saveImage()
        {
            if (p_ImageData == null)
            {
                System.Windows.Forms.MessageBox.Show("Image를 열어주세요");
                return;
            }
            _CancelCopy();

            ////if (m_BasicTool.m_Element.Count == 0 || m_BasicTool.m_Element[0].GetType() != typeof(System.Windows.Shapes.Rectangle))
            ////	m_ImageData.SaveWholeImage();
            ////else
            ////{
            //var temp = m_BasicTool.m_TempRect;
            //int left = (int)temp.StartPos.X;
            //int top = (int)temp.StartPos.Y;

            //int right = (int)temp.EndPos.X;
            //int bot = (int)temp.EndPos.Y;

            //m_ImageData.SaveRectImage(new CRect(left, top, right, bot));
            ////}
        }
        void _clearImage()
        {
            if (p_ImageData != null)
                p_ImageData.ClearImage();
        }
        void _CancelCopy()
        {
            if (p_ImageData.Worker_MemoryCopy.IsBusy)
            {
                p_ImageData.Worker_MemoryCopy.CancelAsync();
            }
            if (p_ImageData.Worker_MemoryClear.IsBusy)
            {
                p_ImageData.Worker_MemoryClear.CancelAsync();
            }
        }
        public ICommand OpenImage
        {
            get
            {
                return new RelayCommand(_openImage);
            }
        }
        public ICommand ClearImage
        {
            get
            {
                return new RelayCommand(_clearImage);
            }
        }
        public ICommand CancelCommand
        {
            get
            {
                return new RelayCommand(_CancelCopy);
            }
        }

        public ICommand CommandColorAll
        {
            get
            {
                return new RelayCommand(() =>
                {
                    p_eColorViewMode = eColorViewMode.All;
                });
            }
        }
        public ICommand CommandColorR
        {
            get
            {
                return new RelayCommand(() =>
                {
                    p_eColorViewMode = eColorViewMode.R;
                });
            }
        }
        public ICommand CommandColorG
        {
            get
            {
                return new RelayCommand(() =>
                {
                    p_eColorViewMode = eColorViewMode.G;
                });
            }
        }
        public ICommand CommandColorB
        {
            get
            {
                return new RelayCommand(() =>
                {
                    p_eColorViewMode = eColorViewMode.B;
                });
            }
        }

        #endregion

        #region MethodAction

        public void KeyEvent(object sender, System.Windows.Input.KeyEventArgs e)
        {
            m_KeyEvent = e;
            switch (m_KeyEvent.Key)
            {
                case Key.Up:
                    CanvasMoveCanvasPoint(Convert.ToInt32(p_CanvasWidth / 2), Convert.ToInt32(p_CanvasHeight / 4));
                    break;
                case Key.Down:
                    CanvasMoveCanvasPoint(Convert.ToInt32(p_CanvasWidth / 2), Convert.ToInt32(p_CanvasHeight * 3 / 4));
                    break;
                case Key.Left:
                    CanvasMoveCanvasPoint(Convert.ToInt32(p_CanvasWidth / 4), Convert.ToInt32(p_CanvasHeight / 2));
                    break;
                case Key.Right:
                    CanvasMoveCanvasPoint(Convert.ToInt32(p_CanvasWidth * 3 / 4), Convert.ToInt32(p_CanvasHeight / 2));
                    break;
            }
        }
        public virtual void PreviewMouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            m_ptViewBuffer = new CPoint(p_View_Rect.X, p_View_Rect.Y);
            m_ptMouseBuffer = new CPoint(p_MouseX, p_MouseY);
            m_swMouse.Restart();

            //p_BasicTool.DrawTool(m_ptMouseBuffer, GetMemPoint(m_ptMouseBuffer), e);

            if (m_KeyEvent == null)
                return;
        }
        public virtual void MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var viewer = sender as Grid;
            viewer.Focus();

            var pt = e.GetPosition(sender as IInputElement);
            p_MouseX = (int)pt.X;
            p_MouseY = (int)pt.Y;
            CPoint nowPt = new CPoint(p_MouseX, p_MouseY);

            DrawCrossLine(nowPt);

            if (m_KeyEvent == null)
                return;
            if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                if (e.LeftButton == MouseButtonState.Pressed && m_swMouse.ElapsedMilliseconds > 0)
                {
                    CanvasMovePoint_Ref(m_ptViewBuffer, m_ptMouseBuffer.X - p_MouseX, m_ptMouseBuffer.Y - p_MouseY);
                    return;
                }

        }
        public virtual void PreviewMouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }
        public virtual void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (m_KeyEvent == null)
                return;
            var viewer = sender as Grid;
            viewer.Focus();

            if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
            {
                try
                {
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
                    //SetRoiRect();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                }
            }
        }


        #endregion

        protected CPoint GetMemPoint(CPoint canvasPt)
        {
            double nX = p_View_Rect.X + canvasPt.X * p_View_Rect.Width / p_CanvasWidth;
            double nY = p_View_Rect.Y + canvasPt.Y * p_View_Rect.Height / p_CanvasHeight;
            return new CPoint((int)nX, (int)nY);
        }
        protected CPoint GetCanvasPoint(CPoint memPt)
        {
            if (p_View_Rect.Width > 0 && p_View_Rect.Height > 0)
            {
                int nX = (memPt.X - p_View_Rect.X) * p_CanvasWidth / p_View_Rect.Width + (p_CanvasWidth / p_View_Rect.Width) / 2;
                int nY = (memPt.Y - p_View_Rect.Y) * p_CanvasHeight / p_View_Rect.Height + (p_CanvasHeight / p_View_Rect.Height) / 2;
                return new CPoint(nX, nY);
            }
            return new CPoint(0, 0);
        }
        protected Point GetCanvasDoublePoint(CPoint memPt)
        {
            if (p_View_Rect.Width > 0 && p_View_Rect.Height > 0)
            {
                double nX = ((double)memPt.X - (double)p_View_Rect.X) * (double)p_CanvasWidth / (double)p_View_Rect.Width + ((double)p_CanvasWidth / (double)p_View_Rect.Width) / 2;
                double nY = ((double)memPt.Y - (double)p_View_Rect.Y) * (double)p_CanvasHeight / (double)p_View_Rect.Height + ((double)p_CanvasHeight / (double)p_View_Rect.Height) / 2;
                return new Point(nX, nY);
            }
            return new Point(0, 0);
        }
    }
}
