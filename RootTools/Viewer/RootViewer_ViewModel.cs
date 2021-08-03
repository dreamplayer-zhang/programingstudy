using Emgu.CV;
using Emgu.CV.Structure;
using RootTools.Database;
using System;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.Collections.Generic;
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
using System.Windows.Threading;

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
        protected IDialogService m_DialogService;

        StopWatch m_swMouse = new StopWatch();
        CPoint m_ptViewBuffer = new CPoint();
        CPoint m_ptMouseBuffer = new CPoint();
        Line Horizon, Vertical;


        public System.Windows.Input.KeyEventArgs m_KeyEvent;

        //Key m_keyMove = Key.LeftCtrl;
        //Key m_keyZoom = Key.LeftCtrl;
        //Key m_keyDrawBasic = Key.LeftShift;
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
        /// 이거는 RootViewer !!!자체 기능!!! 마우스 따라다니는 점선같은 것들 추가할 때 사용하고
        /// 외부에서 수정하거나 초기화하지 않도록 하자.
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
        /// Global UI Shapes
        /// 이거는 레시피 티칭과 같이 사용자가 커스터마이징할 수 있는 UI Element
        /// </summary>
        public ObservableCollection<UIElement> p_UIElement
        {
            get
            {
                return m_UIElement;
            }
            set
            {
                m_UIElement = value;
            }
        }
        private ObservableCollection<UIElement> m_UIElement = new ObservableCollection<UIElement>();

        /// <summary>
        /// 이것도 마찬가지 인데.... 사용자와 Interaction(상호작용)하는 경우(레시피 티칭 등)를 제외한
        /// 그리기를 사용할 때 사용하자
        /// </summary>
        public ObservableCollection<UIElement> p_DrawElement
        {
            get
            {
                return m_DrawElement;
            }
            set
            {
                m_DrawElement = value;
            }
        }
        private ObservableCollection<UIElement> m_DrawElement = new ObservableCollection<UIElement>();

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

        
        public int p_LayerCanvasOffsetX
        {
            get => this.m_LayerCanvasOffsetX;
            set
            {
                if (value < 0)
                {
                    SetProperty<int>(ref m_LayerCanvasOffsetX, 0);
                }
                else
                {
                    SetProperty<int>(ref m_LayerCanvasOffsetX, value);
                }
               
            }
        }
        private int m_LayerCanvasOffsetX;

        public int p_LayerCanvasOffsetY
        {
            get => this.m_LayerCanvasOffsetY;
            set
            {
                if (value < 0)
                {
                    SetProperty<int>(ref m_LayerCanvasOffsetY, 0);
                }
                else
                {
                    SetProperty<int>(ref m_LayerCanvasOffsetY, value);
                }
            }
        }
        private int m_LayerCanvasOffsetY;


        public int p_LayerMemoryOffsetX
        {
            get => this.m_LayerMemoryOffsetX;
            set
            {
                if (value < 0)
                {
                    SetProperty<int>(ref m_LayerMemoryOffsetX, 0);
                }
                else
                {
                    SetProperty<int>(ref m_LayerMemoryOffsetX, value);
                }
            }
        }
        private int m_LayerMemoryOffsetX;

        public int p_LayerMemoryOffsetY
        {
            get => this.m_LayerMemoryOffsetY;
            set
            {
                if (value < 0)
                {
                    SetProperty<int>(ref m_LayerMemoryOffsetY, 0);
                }
                else
                {
                    SetProperty<int>(ref m_LayerMemoryOffsetY, value);
                }
            }
        }
        private int m_LayerMemoryOffsetY;

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


        private int m_LayerCanvasWidth = 100;
        public int p_LayerCanvasWidth
        {
            get
            {
                return m_LayerCanvasWidth;
            }
            set
            {
                if (value == 0)
                    return;

                SetProperty(ref m_LayerCanvasWidth, value);
            }
        }

        private int m_LayerCanvasHeight = 100;
        public int p_LayerCanvasHeight
        {
            get
            {
                return m_LayerCanvasHeight;
            }
            set
            {
                if (value == 0)
                    return;

                SetProperty(ref m_LayerCanvasHeight, value);
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
        protected int m_MouseX = 0;
        public virtual int p_MouseX
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
                            p_MouseMemY = p_View_Rect.Y + p_MouseY * p_View_Rect.Height / p_CanvasHeight;
                            p_MouseMemX = p_View_Rect.X + p_MouseX * p_View_Rect.Width / p_CanvasWidth;

                            if (p_ImgSource.Format.BitsPerPixel == 24)
                            {
                                System.Windows.Media.Color c_Pixel = GetPixelColor(p_ImgSource, p_MouseX, p_MouseY);
                                p_PixelData = "R " + c_Pixel.R + " G " + c_Pixel.G + " B " + c_Pixel.B;
                            }
                            else if (p_ImgSource.Format.BitsPerPixel == 8)
                            {
                                if(m_ImageData.GetBytePerPixel() == 1)
                                {
                                    byte[] pixel = new byte[1];
                                    p_ImgSource.CopyPixels(new Int32Rect(p_MouseX, p_MouseY, 1, 1), pixel, 1, 0);
                                    p_PixelData = "GV " + pixel[0];
                                }
                                else
                                {
                                    unsafe
                                    {
                                        IntPtr ptrImg = m_ImageData.GetPtr();
                                        byte* arrByte = (byte*)ptrImg.ToPointer();
                                        if(arrByte != null)
                                        {
                                            long idx = ((long)p_MouseMemY * m_ImageData.p_Size.X + p_MouseMemX) * m_ImageData.p_nByte;
                                            byte b1 = arrByte[idx + 0];
                                            byte b2 = arrByte[idx + 1];

                                            p_PixelData = "GV " + BitConverter.ToUInt16(new byte[2] { b1, b2 }, 0);
                                        }
                                        else
                                        {
                                            p_PixelData = "GV ";
                                        }
                                    }
                                }
                            }
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
        private Visibility m_VisibleSlider = Visibility.Visible;
        public Visibility p_VisibleSlider
        {
            get => m_VisibleSlider;
            set => SetProperty(ref m_VisibleSlider, value);
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
        public int Clamp(int val, int min, int max)
        {
            int ret = Math.Max(val, min);
            ret = Math.Min(ret, max);

            return ret;
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

        bool isUpdate = false;
        public virtual unsafe void SetImageSource()
        {
			try
			{
				if (p_ImageData != null)
				{

					if (p_ImageData.m_eMode == ImageData.eMode.OtherPCMem)
					{
						if (p_ImageData.GetBytePerPixel() == 1)
						{
							if (p_View_Rect != new System.Drawing.Rectangle(0, 0, 0, 0))
							{
								Image<Gray, byte> view = new Image<Gray, byte>(p_CanvasWidth, p_CanvasHeight);
								byte[,,] viewptr = view.Data;
								byte[] image = p_ImageData.GetData(p_View_Rect, p_CanvasWidth, p_CanvasHeight);
								//for (int yy = 0; yy < p_CanvasHeight; yy++)
                                if(image != null)
                                {
                                    Parallel.For(0, p_CanvasHeight, (yy) =>
                                    {
                                        for (int xx = 0; xx < p_CanvasWidth; xx++)
                                        {
                                            viewptr[yy, xx, 0] = ApplyContrastAndBrightness(image[p_CanvasWidth * yy + xx]);
                                        }
                                    });
                                }
								p_ImgSource = ImageHelper.ToBitmapSource(view);
							}
						}
                        else if(p_ImageData.GetBytePerPixel() == 2)
                        {
                            if (p_View_Rect != new System.Drawing.Rectangle(0, 0, 0, 0))
                            {
                                Image<Gray, byte> view = new Image<Gray, byte>(p_CanvasWidth, p_CanvasHeight);
                                byte[,,] viewptr = view.Data;
                                byte[] image = p_ImageData.GetData(p_View_Rect, p_CanvasWidth, p_CanvasHeight);

                                if (image != null)
                                {
                                    Parallel.For(0, p_CanvasHeight, (yy) =>
                                    {
                                        for (int xx = 0; xx < p_CanvasWidth; xx++)
                                        {
                                            byte b1 = image[(p_CanvasWidth * yy + xx) * 2];
                                            byte b2 = image[(p_CanvasWidth * yy + xx) * 2 + 1];

                                            ushort us = BitConverter.ToUInt16(new byte[] { b1, b2 }, 0);
                                            byte b = (byte)(((double)us / (Math.Pow(2, 16) - 1)) * (Math.Pow(2, 8) - 1));

                                            viewptr[yy, xx, 0] = ApplyContrastAndBrightness(b);
                                        }
                                    });
                                }
                                p_ImgSource = ImageHelper.ToBitmapSource(view);
                            }
                        }
						else if (p_ImageData.GetBytePerPixel() == 3)
						{
							if (p_View_Rect != new System.Drawing.Rectangle(0, 0, 0, 0))
							{
								Image<Rgb, byte> view = new Image<Rgb, byte>(p_CanvasWidth, p_CanvasHeight);
								byte[,,] viewptr = view.Data;
								byte[] image = p_ImageData.GetData(p_View_Rect, p_CanvasWidth, p_CanvasHeight);
								int nTerm = p_CanvasWidth * p_CanvasHeight;
								if (image != null)
									Parallel.For(0, p_CanvasHeight, (yy) =>
									{
										for (int xx = 0; xx < p_CanvasWidth; xx++)
										{
											viewptr[yy, xx, 0] = ApplyContrastAndBrightness(image[p_CanvasWidth * yy + xx]);
											viewptr[yy, xx, 1] = ApplyContrastAndBrightness(image[p_CanvasWidth * yy + xx + nTerm]);
											viewptr[yy, xx, 2] = ApplyContrastAndBrightness(image[p_CanvasWidth * yy + xx + nTerm * 2]);
										}
									});
								p_ImgSource = ImageHelper.ToBitmapSource(view);
							}
						}
						//        p_ImgSource = p_ImageData.GetData(p_View_Rect, p_CanvasWidth, p_CanvasHeight);
						//Image<Gray, byte> view = new Image<Gray, byte>(p_CanvasWidth, p_CanvasHeight);
						//byte[,,] viewptr = view.Data;
						//byte[] image = p_ImageData.GetData(p_View_Rect,p_CanvasWidth, p_CanvasHeight);
						//for (int xx = 0; xx < p_CanvasWidth; xx++)
						//{   
						//    viewptr[xx, xx, 0] = image[p_View_Rect.Width * xx];
						//}
						//p_ImgSource = ImageHelper.ToBitmapSource(view);
					}
					else
					{
						object o = new object();
						if (p_ImageData.GetBytePerPixel() == 1 && p_ImageData.p_nByte == 1)
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
										/*byte pixel = ((byte*)ptrMem)[pix_x + (long)pix_y * sizeX];*/
										byte* arrByte = (byte*)ptrMem.ToPointer();
										long idx = pix_x + (long)pix_y * sizeX;
										byte pixel = arrByte[idx];
										viewptr[yy, xx, 0] = ApplyContrastAndBrightness(pixel);
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
						else if (p_ImageData.GetBytePerPixel() == 2)
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

							byte* arrByte = (byte*)ptrMem.ToPointer();

							Parallel.For(0, p_CanvasHeight, (yy) =>
							{
								{
									long pix_y = rectY + yy * rectHeight / p_CanvasHeight;

									for (int xx = 0; xx < p_CanvasWidth; xx++)
									{
										long pix_x = rectX + xx * rectWidth / p_CanvasWidth;
										byte b1 = arrByte[(pix_y * sizeX + pix_x) * 2 + 0];
										byte b2 = arrByte[(pix_y * sizeX + pix_x) * 2 + 1];
										ushort us = BitConverter.ToUInt16(new byte[] { b1, b2 }, 0);
										byte b = (byte)(((double)us / (Math.Pow(2, 16) - 1)) * (Math.Pow(2, 8) - 1));

										viewptr[yy, xx, 0] = ApplyContrastAndBrightness(b);
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
						else if (p_ImageData.GetBytePerPixel() == 3)
						{
							if (p_eColorViewMode == eColorViewMode.All)
							{
								if (p_ImageData.m_eMode == ImageData.eMode.MemoryRead)
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

                                    StopWatch sw = new StopWatch();

                                    sw.Start();
                                    Parallel.For(0, p_CanvasHeight, (yy) =>
                                    {
                                        {
                                            //unsafe
                                            {
                                                long pix_y = viewrectY + yy * viewrectHeight / p_CanvasHeight;
                                                for (int xx = 0; xx < p_CanvasWidth; xx++)
                                                {
                                                    long pix_x = viewrectX + xx * viewrectWidth / p_CanvasWidth;

                                                    if (pix_x + (long)pix_y * sizeX >= 0)
                                                    {
                                                        viewPtr[yy, xx, 0] = ApplyContrastAndBrightness(imageptrR[(long)pix_x + (long)pix_y * sizeX]);
                                                        viewPtr[yy, xx, 1] = ApplyContrastAndBrightness(imageptrG[(long)pix_x + (long)pix_y * sizeX]);
                                                        viewPtr[yy, xx, 2] = ApplyContrastAndBrightness(imageptrB[(long)pix_x + (long)pix_y * sizeX]);
                                                    }
                                                }
                                            }
                                        }
                                    });
                                    sw.Stop();
                                    System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);

                                    p_ImgSource = ImageHelper.ToBitmapSource(view);

									p_TumbnailImgMargin = new Thickness(Convert.ToInt32((double)p_View_Rect.X * p_ThumbWidth / p_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Y * p_ThumbHeight / p_ImageData.p_Size.Y), 0, 0);
									if (Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / p_ImageData.p_Size.Y) == 0)
										p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / p_ImageData.p_Size.X), 2);
									else
										p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / p_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / p_ImageData.p_Size.Y));
								}
								else if (!isUpdate && p_ImageData.m_eMode == ImageData.eMode.ImageBuffer)
								{
									isUpdate = true;
									int canvasWidth = p_CanvasWidth; // 여기 잠시 수정
									int canvasHeight = p_CanvasHeight;
									Image<Rgb, byte> view = new Image<Rgb, byte>(canvasWidth, canvasHeight);

									if (this.p_ImageData == null)
										return;

									byte[,,] viewPtr = view.Data;
									byte* imageptr = (byte*)p_ImageData.GetPtr();

									int viewrectY = p_View_Rect.Y;
									int viewrectX = p_View_Rect.X;
									int viewrectHeight = p_View_Rect.Height;
									int viewrectWidth = p_View_Rect.Width;
									int sizeX = p_ImageData.p_Size.X;

									Parallel.For(0, canvasHeight, (yy) =>
									{
										{
											long pix_y = viewrectY + yy * viewrectHeight / canvasHeight;
											for (int xx = 0; xx < canvasWidth; xx++)
											{
												long pix_x = viewrectX + xx * viewrectWidth / canvasWidth;
												if (p_ImageData.m_aBuf.Length <= (pix_x * this.p_ImageData.GetBytePerPixel() + 2) + (long)pix_y * (sizeX * 3))
												{
													viewPtr[yy, xx, 0] = 0;
													viewPtr[yy, xx, 1] = 0;
													viewPtr[yy, xx, 2] = 0;
												}
												else
												{
													viewPtr[yy, xx, 0] = ApplyContrastAndBrightness(imageptr[(pix_x * this.p_ImageData.GetBytePerPixel() + 2) + (long)pix_y * (sizeX * 3)]);
													viewPtr[yy, xx, 1] = ApplyContrastAndBrightness(imageptr[(pix_x * this.p_ImageData.GetBytePerPixel() + 1) + (long)pix_y * (sizeX * 3)]);
													viewPtr[yy, xx, 2] = ApplyContrastAndBrightness(imageptr[(pix_x * this.p_ImageData.GetBytePerPixel() + 0) + (long)pix_y * (sizeX * 3)]);
												}

											}
										}
									});

									p_ImgSource = ImageHelper.ToBitmapSource(view);
									isUpdate = false;
								}
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
											viewPtr[yy, xx, 0] = ApplyContrastAndBrightness(imageptr[pix_x + (long)pix_y * sizeX]);
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
				TempLogger.Write("RootViewer", ee);
				//System.Windows.MessageBox.Show(ee.ToString());
			}

		}
        public virtual unsafe void SetLayerSource()
        {
            try
            {
                if (p_ROILayer != null)
                {
                    object o = new object();
                    if (p_ROILayer.GetBytePerPixel() == 1)
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
                            {
                                long pix_y = rectY + yy * rectHeight / p_CanvasHeight;

                                for (int xx = 0; xx < p_CanvasWidth; xx++)
                                {
                                    long pix_x = rectX + xx * rectWidth / p_CanvasWidth;
                                    viewptr[yy, xx, 0] = ApplyContrastAndBrightness(imageptr[pix_x + pix_y * sizeX]);
                                }
                            }
                        });
                        p_LayerSource = ImageHelper.ToBitmapSource(view);
                    }
                    if (p_ROILayer.GetBytePerPixel() == 4)
                    {
                        int CanvasWidth = p_LayerCanvasWidth;
                        int CanvasHeight = p_LayerCanvasHeight;

                        Image<Rgba, byte> view = new Image<Rgba, byte>(CanvasWidth, CanvasHeight);
                        byte[,,] viewPtr = view.Data;

                        IntPtr ptrMem = p_ROILayer.GetPtr();

                        if (ptrMem == IntPtr.Zero)
                            return;

                        byte* imageptr = (byte*)ptrMem.ToPointer();

                        CPoint memOffset = new CPoint(p_LayerMemoryOffsetX, p_LayerMemoryOffsetY);
                        int viewrectX = ((p_View_Rect.X - memOffset.X) <= 0)?0:(p_View_Rect.X - memOffset.X);
                        int viewrectY = ((p_View_Rect.Y - memOffset.Y) <= 0)?0:(p_View_Rect.Y - memOffset.Y);

                        int layerMemWidth = p_ROILayer.p_Size.X;
                        int layerMemHeight = p_ROILayer.p_Size.Y;
                        
                       int viewrectWidth = p_View_Rect.Width;
                       int viewrectHeight = p_View_Rect.Height;



                       Parallel.For(0, CanvasHeight, (yy) =>
                       {
                           long pix_y = viewrectY + yy * viewrectHeight / CanvasHeight;
                           long pix_rect = pix_y * layerMemWidth;

                           if(pix_y < layerMemHeight)
                           {
                               for (int xx = 0; xx < CanvasWidth; xx++)
                               {
                                   long pix_x = viewrectX + xx * viewrectWidth / p_CanvasWidth;

                                   viewPtr[yy, xx, 3] = ApplyContrastAndBrightness(imageptr[3 + 4 * (pix_x + pix_rect)]); //0;
                                   viewPtr[yy, xx, 2] = ApplyContrastAndBrightness(imageptr[2 + 4 * (pix_x + pix_rect)]); //0;//imageptr[0 + 3 * (pix_x + pix_rect)];
                                   viewPtr[yy, xx, 1] = ApplyContrastAndBrightness(imageptr[1 + 4 * (pix_x + pix_rect)]); //0;//imageptr[1 + 3 * (pix_x + pix_rect)];
                                   viewPtr[yy, xx, 0] = ApplyContrastAndBrightness(imageptr[0 + 4 * (pix_x + pix_rect)]); //0;//imageptr[2 + 3 * (pix_x + pix_rect)];
                               }
                           }
                       });

                       byte[] pixels1d = new byte[(long)CanvasHeight * CanvasWidth * 4];
                       WriteableBitmap wbitmap = new WriteableBitmap(CanvasWidth, CanvasHeight, 96, 96, PixelFormats.Bgra32, null);

                       Parallel.For(0, CanvasHeight, (row) =>
                       {
                           for (int col = 0; col < CanvasWidth; col++)
                           {
                               int index = col * 4 + row * CanvasWidth * 4;
                               pixels1d[index] = ApplyContrastAndBrightness(viewPtr[row, col, 0]);
                               pixels1d[index + 1] = ApplyContrastAndBrightness(viewPtr[row, col, 1]);
                               pixels1d[index + 2] = ApplyContrastAndBrightness(viewPtr[row, col, 2]);
                               pixels1d[index + 3] = ApplyContrastAndBrightness(viewPtr[row, col, 3]);
                           }
                       });

                       Int32Rect rect = new Int32Rect(0, 0, CanvasWidth, CanvasHeight);
                       int stride = 4 * CanvasWidth;
                       wbitmap.WritePixels(rect, pixels1d, stride, 0);
                       p_LayerSource = wbitmap;
                       //p_LayerSource = ImageHelper.ToBitmapSource(view);
                   }

               }
           }
           catch (Exception ee)
           {
               TempLogger.Write("RootViewer", ee);
               //System.Windows.MessageBox.Show(ee.ToString());
           }
        }

        public virtual unsafe void SetMaskLayerSource()
        {
            try
            {
                if (p_ROILayer != null)
                {
                    object o = new object();
                    {
                        int CanvasWidth = p_LayerCanvasWidth;
                        int CanvasHeight = p_LayerCanvasHeight;

                        ImageData view = new ImageData((int)CanvasWidth, (int)CanvasHeight, 4);
                        byte* viewPtr = (byte*)view.GetPtr();

                        IntPtr ptrMem = p_ROILayer.GetPtr();

                        if (ptrMem == IntPtr.Zero)
                            return;

                        byte* imageptr = (byte*)ptrMem.ToPointer();

                        CPoint memOffset = new CPoint(p_LayerMemoryOffsetX, p_LayerMemoryOffsetY);
                        int viewrectX = ((p_View_Rect.X - memOffset.X) <= 0) ? 0 : (p_View_Rect.X - memOffset.X);
                        int viewrectY = ((p_View_Rect.Y - memOffset.Y) <= 0) ? 0 : (p_View_Rect.Y - memOffset.Y);

                        int layerMemWidth = p_ROILayer.p_Size.X;
                        int layerMemHeight = p_ROILayer.p_Size.Y;

                        int viewrectWidth = p_View_Rect.Width;
                        int viewrectHeight = p_View_Rect.Height;

                        int nstride = view.p_Size.X;

                        Parallel.For(0, CanvasHeight, (yy) =>
                        {
                            long pix_y = viewrectY + yy * viewrectHeight / CanvasHeight;
                            long pix_rect = pix_y * layerMemWidth;
                            long dd = viewrectX + pix_rect;
                            if (pix_y < layerMemHeight)
                            {
                                for (int xx = 0; xx < CanvasWidth; xx++)
                                {
                                    //long pix_x = viewrectX + xx * viewrectWidth / p_CanvasWidth;
                                    long xpos = 4 * (dd + xx * viewrectWidth / p_CanvasWidth);
                                    long ptrpos = (yy * nstride + xx)*4;

                                    viewPtr[ptrpos + 3] = imageptr[3 + xpos]; //0;
                                    viewPtr[ptrpos + 2] = imageptr[2 + xpos]; //0;//imageptr[0 + 3 * (pix_x + pix_rect)];
                                    viewPtr[ptrpos + 1] = imageptr[1 + xpos]; //0;//imageptr[1 + 3 * (pix_x + pix_rect)];
                                    viewPtr[ptrpos + 0] = imageptr[0 + xpos]; //0;//imageptr[2 + 3 * (pix_x + pix_rect)];
                                }
                            }
                        });

                        byte[] pixels1d = new byte[(long)CanvasHeight * CanvasWidth * 4];
                        WriteableBitmap wbitmap = new WriteableBitmap((int)CanvasWidth, (int)CanvasHeight, 96, 96, PixelFormats.Bgra32, null);

                        Parallel.For(0, CanvasHeight, (row) =>
                        {
                            for (int col = 0; col < CanvasWidth; col++)
                            {
                                long index = (col + row * CanvasWidth) * 4;
                                long oi = (row * nstride + col)*4;


                                pixels1d[index] = viewPtr[oi+ 0];
                                pixels1d[index + 1] = viewPtr[oi + 1];
                                pixels1d[index + 2] = viewPtr[oi + 2];
                                pixels1d[index + 3] = viewPtr[oi + 3];
                            }
                        });

                        Int32Rect rect = new Int32Rect(0, 0, (int)CanvasWidth, (int)CanvasHeight);
                        int stride = (int)(4 * CanvasWidth);
                        wbitmap.WritePixels(rect, pixels1d, stride, 0);
                        p_LayerSource = wbitmap;
                    }

                }
            }
            catch (Exception ee)
            {
                TempLogger.Write("RootViewer", ee);
                //System.Windows.MessageBox.Show(ee.ToString());
            }
        }

        public void ClearViewElement()
        {
            this.p_ViewElement.Clear();
        }

        public void ClearUIElement()
        {
            this.p_UIElement.Clear();
        }

        public void ClearDrawElement()
        {
            this.p_DrawElement.Clear();
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
                    p_View_Rect = new System.Drawing.Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom * p_CanvasWidth / p_CanvasHeight), Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom));
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
            SetThumNailIamgeSource();
        }
        public unsafe void SetThumNailIamgeSource()
        {
			if (p_ImageData.GetBytePerPixel() == 1)
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
						view.Data[yy, xx, 0] = ApplyContrastAndBrightness(((byte*)ptrMem)[pix_x + (long)pix_y * p_ImageData.p_Size.X]);
					}
				}
				if (view.Width != 0 && view.Height != 0)
					p_ThumNailImgSource = ImageHelper.ToBitmapSource(view);
			}
			else if (p_ImageData.GetBytePerPixel() == 3)
			{
				Image<Rgb, byte> view = new Image<Rgb, byte>(p_ThumbWidth, p_ThumbHeight);
				IntPtr ptrMemR = p_ImageData.GetPtr(2);
				IntPtr ptrMemG = p_ImageData.GetPtr(1);
				IntPtr ptrMemB = p_ImageData.GetPtr(0);


				if (ptrMemR == IntPtr.Zero)
					return;

				byte[,,] viewPtr = view.Data;
				byte* imageptrR = (byte*)ptrMemR.ToPointer();
				byte* imageptrG = (byte*)ptrMemG.ToPointer();
				byte* imageptrB = (byte*)ptrMemB.ToPointer();

				if (ptrMemR == IntPtr.Zero)
					return;
				int pix_x = 0;
				int pix_y = 0;


				for (int yy = 0; yy < p_ThumbHeight; yy++)
				{
					pix_y = yy * p_ImageData.p_Size.Y / p_ThumbHeight;
					for (int xx = 0; xx < p_ThumbWidth; xx++)
					{
						pix_x = xx * p_ImageData.p_Size.X / p_ThumbWidth;
						//view.Data[yy, xx, 2] = ApplyContrastAndBrightness(((byte*)imageptrR)[0 + (pix_x + (long)pix_y * p_ImageData.p_Size.X)]);
						//view.Data[yy, xx, 1] = ApplyContrastAndBrightness(((byte*)imageptrG)[1 + (pix_x + (long)pix_y * p_ImageData.p_Size.X)]);
						//view.Data[yy, xx, 0] = ApplyContrastAndBrightness(((byte*)imageptrB)[2 + (pix_x + (long)pix_y * p_ImageData.p_Size.X)]);
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
            if (offset is null) 
                offset = new CPoint(0, 0);

            IntPtr ptrMem = p_ROILayer.GetPtr();
            if (ptrMem == IntPtr.Zero)
                return;

            CPoint StartPt = new CPoint(nowRect.Left - offset.X, nowRect.Top - offset.Y);

            IntPtr rectPtr = (IntPtr)((long)ptrMem + (long)StartPt.Y * p_ROILayer.p_Size.X * p_ROILayer.GetBytePerPixel() + StartPt.X * p_ROILayer.GetBytePerPixel());
            for (int i = 0; i < nowRect.Height; i++)
            {
                Marshal.Copy(imageData.m_aBuf, i * nowRect.Width * 4, (IntPtr)((long)rectPtr + (long)p_ROILayer.p_Size.X * p_ROILayer.GetBytePerPixel() * i), nowRect.Width * 4);
            }

        }

        public virtual unsafe void DrawLineBitmap(CPoint startPt, CPoint endPt, int thickness, byte r, byte g, byte b, byte a, CPoint offset = null)
        {
            if (offset == null) offset = new CPoint(0, 0);

            startPt.X = startPt.X - offset.X - 1;
            startPt.Y = startPt.Y - offset.Y - 1;
            endPt.X = endPt.X - offset.X - 1;
            endPt.Y = endPt.Y - offset.Y - 1;

            List<CPoint> ListMemoryPoint = new List<CPoint>();

            double angle = Math.Atan2(endPt.Y - startPt.Y, endPt.X - startPt.X);

            ListMemoryPoint.Add(new CPoint((int)(startPt.X + thickness * Math.Cos(angle + Math.PI / 2)), (int)(startPt.Y + thickness * Math.Sin(angle + Math.PI / 2))));
            ListMemoryPoint.Add(new CPoint((int)(startPt.X + thickness * Math.Cos(angle - Math.PI / 2)), (int)(startPt.Y + thickness * Math.Sin(angle - Math.PI / 2))));
            ListMemoryPoint.Add(new CPoint((int)(endPt.X + thickness * Math.Cos(angle - Math.PI / 2)), (int)(endPt.Y + thickness * Math.Sin(angle - Math.PI / 2))));
            ListMemoryPoint.Add(new CPoint((int)(endPt.X + thickness * Math.Cos(angle + Math.PI / 2)), (int)(endPt.Y + thickness * Math.Sin(angle + Math.PI / 2))));

            DrawPolygonBitmap(ListMemoryPoint, r, g, b, a);
        }

        public virtual unsafe void DrawRectBitmap(CRect rect, byte r, byte g, byte b, byte a, CPoint offset = null)
        {
            if (offset == null) offset = new CPoint(0, 0);

            rect.Left = rect.Left - offset.X - 1;
            rect.Right = rect.Right - offset.X - 1;
            rect.Top = rect.Top - offset.Y - 1;
            rect.Bottom = rect.Bottom - offset.Y - 1;

            Parallel.For(rect.Top, rect.Bottom + 1 , y =>
            {
                for (int x = rect.Left; x <= rect.Right; x++)
                {
                    CPoint pixelPt = new CPoint(x, y);
                    DrawPixelBitmap(pixelPt, r, g, b, a);
                }
            });
        }

        public virtual unsafe void DrawPolygonBitmap(List<CPoint> ListMemoryPoint, byte r, byte g, byte b, byte a, CPoint offset = null)
        {
            if (offset == null) offset = new CPoint(0, 0);

            for (int i = 0; i < ListMemoryPoint.Count; i++)
            {
                ListMemoryPoint[i].X = ListMemoryPoint[i].X - offset.X - 1;
                ListMemoryPoint[i].Y = ListMemoryPoint[i].Y - offset.Y - 1;
            }

            int maxY = 0;
            int minY = 0;
            int k = 0;
            int temp = 0;
            int[] ptX = new int[p_CanvasWidth];
            List<ScanLine> ListScanLines = new List<ScanLine>();

            for (int i = 0; i < ListMemoryPoint.Count; i++)
            {
                if (i == ListMemoryPoint.Count - 1)
                {
                    if (ListMemoryPoint[i].Y == ListMemoryPoint[0].Y)
                    {
                        continue;
                    }

                    ListScanLines.Add(new ScanLine(ListMemoryPoint[i], ListMemoryPoint[0]));
                    maxY = Math.Max(maxY, Math.Max(ListMemoryPoint[i].Y, ListMemoryPoint[0].Y));
                    minY = Math.Min(minY, Math.Min(ListMemoryPoint[i].Y, ListMemoryPoint[0].Y));
                }
                else
                {
                    if (ListMemoryPoint[i].Y == ListMemoryPoint[i + 1].Y)
                    {
                        continue;
                    }

                    ListScanLines.Add(new ScanLine(ListMemoryPoint[i], ListMemoryPoint[i + 1]));
                    maxY = Math.Max(maxY, Math.Max(ListMemoryPoint[i].Y, ListMemoryPoint[0].Y));
                    minY = Math.Min(minY, Math.Min(ListMemoryPoint[i].Y, ListMemoryPoint[0].Y));
                }
            }

            for (int y = minY; y < maxY; y++)
            {
                k = 0;
                for (int i = 0; i < ListScanLines.Count; i++)
                {
                    if ((ListScanLines[i].StartPt.Y <= y && ListScanLines[i].EndPt.Y > y) || (ListScanLines[i].StartPt.Y > y && ListScanLines[i].EndPt.Y <= y))
                    {
                        ptX[k] = (int)(ListScanLines[i].StartPt.X + ListScanLines[i].slope * (y - ListScanLines[i].StartPt.Y));
                        k = k + 1;
                    }
                }

                for (int i = 0; i < k - 1; i++)
                {
                    if (ptX[i] > ptX[i + 1])
                    {
                        temp = ptX[i];
                        ptX[i] = ptX[i + 1];
                        ptX[i + 1] = temp;
                    }
                }

                for (int i = 0; i < k; i += 2)
                {
                    for (int x = ptX[i]; x <= ptX[i + 1]; x++)
                    {
                        CPoint pixelPt = new CPoint(x, y);
                        DrawPixelBitmap(pixelPt, r, g, b, a);
                    }
                }
            }
        }

        public virtual unsafe void DrawCircleBitmap(TEllipse circle, byte r, byte g, byte b, byte a, CPoint offset = null)
        {
            if (offset == null) offset = new CPoint(0, 0);

            circle.MemoryRect.Left = circle.MemoryRect.Left - offset.X - 1;
            circle.MemoryRect.Right = circle.MemoryRect.Right - offset.X - 1;
            circle.MemoryRect.Top = circle.MemoryRect.Top - offset.Y - 1;
            circle.MemoryRect.Bottom = circle.MemoryRect.Bottom - offset.Y - 1;

            int temp = 0;
            int[] ptX = new int[2];

            int x0 = circle.MemoryRect.Left + circle.MemoryRect.Width / 2;
            int y0 = circle.MemoryRect.Top + circle.MemoryRect.Height / 2;

            for (int y = circle.MemoryRect.Top; y < circle.MemoryRect.Bottom + 1; y++)
            {
                ptX[0] = (int)Math.Sqrt(Math.Pow(circle.MemoryRect.Width / 2, 2) * (1 - Math.Pow(y - y0, 2) / Math.Pow(circle.MemoryRect.Height / 2, 2))) + x0;
                ptX[1] = 2 * x0 - ptX[0];

                if (ptX[0] > ptX[1])
                {
                    temp = ptX[0];
                    ptX[0] = ptX[1];
                    ptX[1] = temp;
                }

                for (int x = ptX[0]; x <= ptX[1]; x++)
                {
                    CPoint pixelPt = new CPoint(x, y);
                    DrawPixelBitmap(pixelPt, r, g, b, a);
                }
            }
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

        public virtual unsafe void DrawPixelBitmap(byte* imagePtr, CPoint memPt, byte r, byte g, byte b, byte a)
        {
            imagePtr[(memPt.Y * p_ROILayer.p_Size.X + memPt.X) * 4 + 0] = b; // b
            imagePtr[(memPt.Y * p_ROILayer.p_Size.X + memPt.X) * 4 + 1] = g; // g
            imagePtr[(memPt.Y * p_ROILayer.p_Size.X + memPt.X) * 4 + 2] = r; // r
            imagePtr[(memPt.Y * p_ROILayer.p_Size.X + memPt.X) * 4 + 3] = a; // a
        }

        public virtual unsafe void DrawPixelBitmap(CPoint memPt, byte value, CPoint offset = null)
        {
            if (offset == null) offset = new CPoint(0, 0);

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
            catch (Exception)
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
                int channel = 0;
                if (p_ImageData.GetBytePerPixel() == 3)
                {
                    if (p_eColorViewMode == eColorViewMode.G)
                        channel = 1;
                    else if (p_eColorViewMode == eColorViewMode.B)
                        channel = 2;
                }

                if (m_DialogService != null)
                {
                    var viewModel = new Dialog_ImageOpenViewModel(this as RootViewer_ViewModel);
                    Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
                    if (!result.HasValue || !result.Value)
                    {
                        return;
                    }
                }

                p_ImageData.OpenFile(ofd.FileName, p_CopyOffset, channel);
            }
        }
        public void _saveImage()
        {
            if (p_ImageData == null)
            {
                System.Windows.Forms.MessageBox.Show("Image를 열어주세요");
                return;
            }
            _CancelCopy();

            // 그레이/컬러 이미지에 따라 선택 가능 필터 변경
            bool isGrayImg = p_ImageData.GetBytePerPixel() != 3;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter += "컬러 24비트 이미지(*.bmp)|*.bmp";
            sfd.Filter += "|단색 8비트 이미지(*.bmp)|*.bmp";
            if (isGrayImg == true)
                sfd.Filter += "|단색 16비트 이미지(*.bmp)|*.bmp";

            // ImageData 변수에 따라 SaveFileDialog에 기본 선택된 필터 조정
            switch (p_ImageData.GetBytePerPixel())
            {
                case 1:
                    sfd.FilterIndex = 2;
                    break;
                case 2:
                    sfd.FilterIndex = 3;
                    break;
                case 3:
                default:
                    sfd.FilterIndex = 1;
                    break;
            }

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                bool isGrayScale = (sfd.FilterIndex != 1 ? true : false);
                int nByte = (sfd.FilterIndex != 3) ? 1 : 2;

                // 선택된 RGB에 따라 boolean 변수 설정
                bool[] rgbEnable = new bool[3] { false, false, false };
                if (isGrayImg == false)
                {
                    switch (p_eColorViewMode)
                    {
                        case eColorViewMode.R: rgbEnable[0] = true; break;
                        case eColorViewMode.G: rgbEnable[1] = true; break;
                        case eColorViewMode.B: rgbEnable[2] = true; break;
                        case eColorViewMode.All:
                        default:
                            rgbEnable[0] = rgbEnable[1] = rgbEnable[2] = true;
                            break;
                    }
                }

                p_ImageData.SaveWholeImage(sfd.FileName, isGrayScale, nByte, rgbEnable[0], rgbEnable[1], rgbEnable[2]);
            }

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
        public void _clearImage()
        {
            if (p_ImageData != null)
                p_ImageData.ClearImage();
        }
        protected void _CancelCopy()
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
        public ICommand SaveImage
        {
            get
            {
                return new RelayCommand(_saveImage);
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
                    TempLogger.Write("RootViewer", ex);
                    //System.Windows.Forms.MessageBox.Show(ex.ToString());
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
        public CPoint GetCanvasPoint(CPoint memPt)
        {
            if (p_View_Rect.Width > 0 && p_View_Rect.Height > 0)
            {
                int nX = (memPt.X - p_View_Rect.X) * p_CanvasWidth / p_View_Rect.Width + (p_CanvasWidth / p_View_Rect.Width) / 2;
                int nY = (memPt.Y - p_View_Rect.Y) * p_CanvasHeight / p_View_Rect.Height + (p_CanvasHeight / p_View_Rect.Height) / 2;
                return new CPoint(nX, nY);
            }
            return new CPoint(0, 0);
        }

        public CPoint GetCanvasPoint(Point memPt)
        {
            if (p_View_Rect.Width > 0 && p_View_Rect.Height > 0)
            {
                int nX = ((int)memPt.X - p_View_Rect.X) * p_CanvasWidth / p_View_Rect.Width + (p_CanvasWidth / p_View_Rect.Width) / 2;
                int nY = ((int)memPt.Y - p_View_Rect.Y) * p_CanvasHeight / p_View_Rect.Height + (p_CanvasHeight / p_View_Rect.Height) / 2;
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
        private System.Windows.Input.MouseEventArgs _mouseEvent;
        public System.Windows.Input.MouseEventArgs MouseEvent
        {
            get
            {
                return _mouseEvent;
            }
            set
            {
                SetProperty(ref _mouseEvent, value);
            }
        }

        void TumbNailMove()
        {
            if (MouseEvent.LeftButton == MouseButtonState.Pressed)
            {
                double perX = (double)p_TumbMouseX / p_ThumbWidth;
                double perY = (double)p_TumbMouseY / p_ThumbHeight;
                CanvasMovePoint(perX, perY);
            }
        }
        void ThumNailMoveStart()
        {
            TumbNailMove();
        }

        public ICommand TumbNailMouseMove
        {
            get
            {
                return new RelayCommand(TumbNailMove);
            }
        }
        public ICommand TumbNailMouseLeftDown
        {
            get
            {
                return new RelayCommand(ThumNailMoveStart);
            }
        }

    }
}
