using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;


namespace RootTools
{
    public delegate void LoadedDelegate();
    public class ImageViewer_ViewModel : ObservableObject
    {
         public enum DrawingMode
        {
            None,
            Drawing,
            Tool,
        }
        #region Property
         private ObservableCollection<UIElement> _ViewerUIelement = new ObservableCollection<UIElement>();
         public ObservableCollection<UIElement> p_ViewerUIElement
         {
             get
             {
                 return _ViewerUIelement;
             }
             set
             {
                 SetProperty(ref _ViewerUIelement, value);
             }
         }

		public void DrawDefect(DefectData item)
		{

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

        private int _CanvasWidth = 100;
        public int p_CanvasWidth
        {
            get
            {
                return _CanvasWidth;
            }
            set
            {
                if (value == 0)
                    return;
                SetProperty(ref _CanvasWidth, value);
            }
        }
        private int _CanvasHeight = 100;
        public int p_CanvasHeight
        {
            get
            {
                return _CanvasHeight;
            }
            set
            {
                if (value == 0)
                    return;
                //_CanvasHeight = value / 10 * 10;
                //RaisePropertyChanged();
                SetProperty(ref _CanvasHeight, value);
            }
        }

        private int _ThumbWidth = 100;
        public int p_ThumbWidth
        {
            get
            {
                return _ThumbWidth;
            }
            set
            {
                SetProperty(ref _ThumbWidth, value);
            }
        }
        private int _ThumbHeight = 100;
        public int p_ThumbHeight
        {
            get
            {
                return _ThumbHeight;
            }
            set
            {
                //_CanvasHeight = value / 10 * 10;
                //RaisePropertyChanged();
                SetProperty(ref _ThumbHeight, value);
            }
        }

        string _test;
        public string test
        {
            get
            {

                return _test;
            }
            set
            {
                SetProperty(ref _test, value);
            }

        }

        private System.Windows.Input.KeyEventArgs _keyEvent;
        public System.Windows.Input.KeyEventArgs KeyEvent
        {
            get
            {
                return _keyEvent;
            }
            set
            {
                SetProperty(ref _keyEvent, value);
            }

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

        private int _MouseX = 0;
        public int p_MouseX
        {
            get
            {
                return _MouseX;
            }
            set
            {
                if (p_ImgSource != null)
                {
                    byte[] pixel = new byte[1];
                    if (_CanvasWidth != 0 && _CanvasHeight != 0)
                    {
                        if (p_MouseX < p_ImgSource.Width && p_MouseY < p_ImgSource.Height)
                        {
                            p_ImgSource.CopyPixels(new Int32Rect(p_MouseX, p_MouseY, 1, 1), pixel, 1, 0);
                            p_GV = pixel[0];
                            p_MouseMemY = p_View_Rect.Y + p_MouseY * p_View_Rect.Height / _CanvasHeight;
                            p_MouseMemX = p_View_Rect.X + p_MouseX * p_View_Rect.Width / _CanvasWidth;
                        }
                    }

                }
                SetProperty(ref _MouseX, value);
            }
        }
        private int _MouseY = 0;
        public int p_MouseY
        {
            get
            {
                return _MouseY;
            }
            set
            {
                SetProperty(ref _MouseY, value);
            }
        }
        private int _MouseMemX = 0;
        public int p_MouseMemX
        {
            get
            {
                return _MouseMemX;
            }
            set
            {
                SetProperty(ref _MouseMemX, value);
            }
        }
        private int _MouseMemY = 0;
        public int p_MouseMemY
        {
            get
            {
                return _MouseMemY;
            }
            set
            {
                SetProperty(ref _MouseMemY, value);
            }
        }
        private int _TumbMouseX = 0;
        public int p_TumbMouseX
        {
            get
            {
                return _TumbMouseX;
            }
            set
            {
                SetProperty(ref _TumbMouseX, value);
            }
        }
        private int _TumbMouseY = 0;
        public int p_TumbMouseY
        {
            get
            {
                return _TumbMouseY;
            }
            set
            {
                SetProperty(ref _TumbMouseY, value);
            }
        }

        private int _GV = 0;
        public int p_GV
        {
            get
            {
                return _GV;
            }
            set
            {
                SetProperty(ref _GV, value);
            }
        }

        private double _Zoom = 1;
        public double p_Zoom
        {
            get
            {
                return _Zoom;
            }
            set
            {
                SetProperty(ref _Zoom, value);
                SetRoiRect();
            }
        }

        private CPoint m_CopyOffset = new CPoint(0, 0);
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

        private int m_nProgress = 0;
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

        private System.Drawing.Rectangle _View_Rect = new System.Drawing.Rectangle();
        public System.Drawing.Rectangle p_View_Rect
        {
            get
            {
                return _View_Rect;
            }
            set
            {
                SetProperty(ref _View_Rect, value);
            }
        }

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

        private Thickness _TumbnailImgMargin = new Thickness(0, 0, 0, 0);
        public Thickness p_TumbnailImgMargin
        {
            get
            {
                return _TumbnailImgMargin;
            }
            set
            {
                SetProperty(ref _TumbnailImgMargin, value);
            }
        }


        private System.Drawing.Rectangle _TumbnailImg_Rect = new System.Drawing.Rectangle();
        public System.Drawing.Rectangle p_TumbnailImg_Rect
        {
            get
            {
                return _TumbnailImg_Rect;
            }
            set
            {
                SetProperty(ref _TumbnailImg_Rect, value);
            }
        }

        private BitmapSource _ImageThumnail;
        public BitmapSource p_ImageThumnail
        {
            get
            {
                return _ImageThumnail;
            }
            set
            {
                SetProperty(ref _ImageThumnail, value);
            }
        }

        DrawingMode m_Mode = DrawingMode.None;
        public DrawingMode p_Mode
        {
            get
            {
                    return m_Mode;
            }
            set
            {
                SetProperty(ref m_Mode, value);
            }
        }

        private readonly IDialogService m_DialogService;
        #endregion 
        DrawHelper m_DrawHelper = new DrawHelper();
        public ImageViewer_ViewModel(ImageData image = null, IDialogService dialogService= null)
        {       
            if (image != null)
            {
                p_ImageData = image;
                image.OnCreateNewImage += image_NewImage;
                image.OnUpdateImage += image_OnUpdateImage;
                image.UpdateOpenProgress += image_UpdateOpenProgress;
                InitRoiRect(p_ImageData.p_Size.X,p_ImageData.p_Size.Y);
                SetImageSource();
                
            }
            if (dialogService != null)
            {
                m_DialogService = dialogService;
            }
        }

        public void SetImageData(ImageData image)
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
            var asdf = ImageViewer.DataContextProperty;
            //((ImageViewer_ViewModel)(this.DataContext)).SetRoiRect();
            InitRoiRect(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
            SetImageSource();
        }
        void InitRoiRect(int nWidth, int nHeight)
        {
            p_View_Rect = new System.Drawing.Rectangle(0, 0, nWidth, nHeight);
            p_Zoom = 1;

            bool bRatio_WH = (double)p_View_Rect.Width / p_CanvasWidth < (double)p_View_Rect.Height / p_CanvasHeight;
            //m_View_Rect = new Rectangle(m_View_Rect.X, m_View_Rect.Y, m_View_Rect.Width, m_View_Rect.Height * ViewWidth / imgWidth);
            if (bRatio_WH)//세로가 길어
            {
                p_View_Rect = new System.Drawing.Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Width, p_View_Rect.Width * p_CanvasHeight / p_CanvasWidth);
            }
            else
            {
                p_View_Rect = new System.Drawing.Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Height * p_CanvasWidth / p_CanvasHeight, p_View_Rect.Height);
                //p_View_Rect = new Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Width, p_View_Rect.Width * p_CanvasHeight / p_CanvasWidth);
            }
            SetThumNailIamge();
        }

        public void SetRoiRect()
        {
            if (p_ImageData != null)
            {
                CPoint StartPt = GetStartPoint_Center(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
                bool bRatio_WH = (double)p_ImageData.p_Size.X / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;
                if (bRatio_WH)
                { //세로가 길어
                    p_View_Rect = new System.Drawing.Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom), Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom * p_CanvasHeight / p_CanvasWidth));
                }
                else
                {
                    p_View_Rect = new System.Drawing.Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom * p_CanvasWidth / p_CanvasHeight), Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom));
                }
                if (p_View_Rect.Height % 2 != 0)
                    _View_Rect.Height += 1;
                SetImageSource();
            }
        }
        public void SetThumbNailSize(int width, int height)
        {  
            p_ThumbWidth = width;
            p_ThumbHeight = height;
        }
        public unsafe void SetThumNailIamge()
        {
            Image<Gray, byte> view = new Image<Gray, byte>(p_ThumbWidth, p_ThumbHeight);
            IntPtr ptrMem = m_ImageData.GetPtr();
            if (ptrMem == IntPtr.Zero) return; 
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

        public unsafe void SetImageSource()
        {
            try
            {
                if (p_ImageData != null)
                {
                    Image<Gray, byte> view = new Image<Gray, byte>(p_CanvasWidth, p_CanvasHeight);
                    IntPtr ptrMem = m_ImageData.GetPtr();
                    if (ptrMem == IntPtr.Zero)
                        return;
                    int pix_x = 0;
                    int pix_y = 0;

                    for (int yy = 0; yy < p_CanvasHeight; yy++)
                    {
                        for (int xx = 0; xx < p_CanvasWidth; xx++)
                        {
                            pix_x = p_View_Rect.X + xx * p_View_Rect.Width / p_CanvasWidth;
                            pix_y = p_View_Rect.Y + yy * p_View_Rect.Height / p_CanvasHeight;
                            view.Data[yy, xx, 0] = ((byte*)ptrMem)[(long)pix_x + (long)pix_y * p_ImageData.p_Size.X];
                        }
                    }
                    p_ImgSource = ImageHelper.ToBitmapSource(view);

                    p_TumbnailImgMargin = new Thickness(Convert.ToInt32((double)p_View_Rect.X * p_ThumbWidth / m_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Y * p_ThumbHeight / m_ImageData.p_Size.Y), 0, 0);
                    if (Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / m_ImageData.p_Size.Y) == 0)
                        p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / m_ImageData.p_Size.X), 2);
                    else
                        p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / m_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / m_ImageData.p_Size.Y));
                }
            }
            catch (Exception ee)
            {
                System.Windows.MessageBox.Show(ee.ToString());
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

        void _btnOpenImage()
        {
            if(m_ImageData == null)
            {
                System.Windows.Forms.MessageBox.Show("Image를 열어주세요");
                return;
            }
            CancelCommand();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files(*.bmp;*.jpg)|*.bmp;*.jpg";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var viewModel = new Dialog_ImageOpenViewModel(this);
                Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
                if (result.HasValue)
                {
                    if (result.Value)
                    {
                        m_ImageData.OpenFile(ofd.FileName, p_CopyOffset);
                    }
                    else
                    {
                        // Cancelled
                    }
                }
            }
        }
        void _btnSaveImage()
        {
            if (m_ImageData == null)
            {
                System.Windows.Forms.MessageBox.Show("Image를 열어주세요");
                return;
            }
            CancelCommand();
          
            if (m_DrawHelper.DrawnRect == null)
                m_ImageData.SaveWholeImage();
            else
                m_ImageData.SaveRectImage(new CRect(m_DrawHelper.Rect_StartPt.X, m_DrawHelper.Rect_StartPt.Y, m_DrawHelper.Rect_EndPt.X, m_DrawHelper.Rect_EndPt.Y));
        }
        void ImageClear()
        {
            if(m_ImageData != null)
                m_ImageData.ClearImage();
        }

        void CancelCommand()
        {
            if (m_ImageData.Worker_MemoryCopy.IsBusy)
            {
                m_ImageData.Worker_MemoryCopy.CancelAsync();
            }
            if (m_ImageData.Worker_MemoryClear.IsBusy)
            {
                m_ImageData.Worker_MemoryClear.CancelAsync();
            }
        }
        void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            //CPoint StartPt = GetCurrentPoint();
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
            StartPt.X = Convert.ToInt32(p_ImageData.p_Size.X * nPercentX - p_View_Rect.Width/2);
            StartPt.Y = Convert.ToInt32(p_ImageData.p_Size.Y * nPercentY - p_View_Rect.Height/ 2);
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

        void SetViewRect(CPoint point)      //point image의 좌상단
        {
            bool bRatio_WH = (double)p_ImageData.p_Size.X / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;
            if (bRatio_WH)
            { //세로가 길어
                p_View_Rect = new System.Drawing.Rectangle(point.X, point.Y, Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom), Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom * p_CanvasHeight / p_CanvasWidth));
            }
            else
            {
                p_View_Rect = new System.Drawing.Rectangle(point.X, point.Y, Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom * p_CanvasWidth / p_CanvasHeight), Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom));
            }
        }

        #region Point Helper
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
        CPoint GetCurrentPoint()
        {
            int nX = p_View_Rect.X + p_View_Rect.Width/2;
            int nY = p_View_Rect.Y +  p_View_Rect.Height / 2;

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
            bool bRatio_WH = (double)p_ImageData.p_Size.X / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;
            int viewrectwidth =0;
            int viewrectheight = 0;
            int nX = 0;
            int nY = 0;
            if (bRatio_WH)
            { //세로가 길어
                nX = p_View_Rect.X + Convert.ToInt32(p_View_Rect.Width - nImgWidth * p_Zoom) / 2;
                nY = p_View_Rect.Y + Convert.ToInt32(p_View_Rect.Height - nImgWidth * p_Zoom * p_CanvasHeight / p_CanvasWidth) / 2;
                viewrectwidth = Convert.ToInt32(nImgWidth * p_Zoom);
                viewrectheight = Convert.ToInt32(nImgWidth * p_Zoom * p_CanvasHeight / p_CanvasWidth);
            }
            else
            {
                nX = p_View_Rect.X + Convert.ToInt32(p_View_Rect.Width - nImgHeight * p_Zoom * p_CanvasWidth / p_CanvasHeight) /2;
                nY = p_View_Rect.Y + Convert.ToInt32(p_View_Rect.Height - nImgHeight * p_Zoom)/2 ;
                viewrectwidth = Convert.ToInt32(nImgHeight * p_Zoom * p_CanvasWidth / p_CanvasHeight);
                viewrectheight = Convert.ToInt32(nImgHeight * p_Zoom);
            }

            if (nX < 0)
                nX = 0;
            else if (nX > nImgWidth -viewrectwidth)
                nX = nImgWidth - viewrectwidth;
            if (nY < 0)
                nY = 0;
            else if (nY > nImgHeight - viewrectheight)
                nY = nImgHeight - viewrectheight;
            return new CPoint(nX, nY);
        }
        CPoint GetCanvasPoint(int memX, int memY)
        {
            if (p_View_Rect.Width > 0 && p_View_Rect.Height > 0)
            {

                int nX = (int)Math.Round((double)(memX - p_View_Rect.X) * p_CanvasWidth / p_View_Rect.Width, MidpointRounding.ToEven);
                //int xx = (memX - p_ROI_Rect.X) * ViewWidth / p_ROI_Rect.Width;
                int nY = (int)Math.Round((double)(memY - p_View_Rect.Y) * p_CanvasHeight / p_View_Rect.Height, MidpointRounding.AwayFromZero);
                return new CPoint(nX, nY);
            }
            return new CPoint(0, 0);
        }
        #endregion

        StopWatch m_swMouse = new StopWatch();
        CPoint m_ptBuffer1 = new CPoint();
        CPoint m_PtMouseBuffer = new CPoint();

        private void StartImageMove()
        {
            m_ptBuffer1 = new CPoint(p_View_Rect.X, p_View_Rect.Y);
            m_PtMouseBuffer = new CPoint(p_MouseX, p_MouseY);
            m_swMouse.Restart();
        }
        private void StartDrawingRect()
        {
            if (m_DrawHelper == null)
                m_DrawHelper = new DrawHelper();

            p_ViewerUIElement.Clear();
            m_DrawHelper.DrawnRect = new System.Windows.Shapes.Rectangle();
            m_DrawHelper.DrawnTb = new TextBlock();

            m_DrawHelper.Rect_StartPt = new CPoint(p_MouseMemX, p_MouseMemY);
            CPoint CanvasPt = GetCanvasPoint(p_MouseMemX, p_MouseMemY);

            Canvas.SetLeft(m_DrawHelper.DrawnRect, CanvasPt.X);
            Canvas.SetTop(m_DrawHelper.DrawnRect, CanvasPt.Y);
            SetRectStyle(3,2);

            p_ViewerUIElement.Add(m_DrawHelper.DrawnTb);
            p_ViewerUIElement.Add(m_DrawHelper.DrawnRect);

        }

        private void SetRectStyle(int nDash1, int nDash2)
        {
            m_DrawHelper.DrawnRect.Stroke = System.Windows.Media.Brushes.Yellow;
            m_DrawHelper.DrawnRect.StrokeThickness = 2;
            m_DrawHelper.DrawnRect.StrokeDashArray = new DoubleCollection { nDash1, nDash2 };
            m_DrawHelper.DrawnTb.Foreground = System.Windows.Media.Brushes.Red;
            m_DrawHelper.DrawnTb.FontSize = 18;
        }

        private void DrawingRectProgress()
        {
            if (m_DrawHelper.DrawnRect != null)
            {
                m_DrawHelper.Rect_EndPt = new CPoint(p_MouseMemX, p_MouseMemY);
                CPoint StartPt = GetCanvasPoint(m_DrawHelper.Rect_StartPt.X, m_DrawHelper.Rect_StartPt.Y);
                CPoint NowPt = GetCanvasPoint(p_MouseMemX, p_MouseMemY);


                Canvas.SetLeft(m_DrawHelper.DrawnRect, StartPt.X);
                Canvas.SetTop(m_DrawHelper.DrawnRect, StartPt.Y);

                if (m_DrawHelper.Rect_EndPt.X < m_DrawHelper.Rect_StartPt.X)
                {
                    Canvas.SetLeft(m_DrawHelper.DrawnRect, NowPt.X);
                }
                if (m_DrawHelper.Rect_EndPt.Y < m_DrawHelper.Rect_StartPt.Y)
                {
                    Canvas.SetTop(m_DrawHelper.DrawnRect, NowPt.Y);
                }

                m_DrawHelper.DrawnRect.Width = Math.Abs(StartPt.X - NowPt.X);
                m_DrawHelper.DrawnRect.Height = Math.Abs(StartPt.Y - NowPt.Y);

                int w = m_DrawHelper.Rect_EndPt.X - m_DrawHelper.Rect_StartPt.X;
                int h = m_DrawHelper.Rect_EndPt.Y - m_DrawHelper.Rect_StartPt.Y;

                string msg = "Width :" + w.ToString() + " Height :" + h.ToString();
                m_DrawHelper.DrawnTb.Text = msg;
                m_DrawHelper.DrawnRect.StrokeDashArray = new DoubleCollection(1);
            }
        }
        private void DrawRectDone()
        {
            try
            {
                int w = m_DrawHelper.Rect_EndPt.X - m_DrawHelper.Rect_StartPt.X;
                int h = m_DrawHelper.Rect_EndPt.Y - m_DrawHelper.Rect_StartPt.Y;
                string msg = "Width :" + w.ToString() + " Height :" + h.ToString();
                m_DrawHelper.DrawnTb.Text = msg;
                m_DrawHelper.DrawnRect.StrokeDashArray = new DoubleCollection(1);
            }
            catch
            {
                return;
            }

        }

        public void SetRectElement_MemPos(CRect rect)
        {
            p_ViewerUIElement.Clear();
            m_DrawHelper.DrawnRect = new System.Windows.Shapes.Rectangle();
            m_DrawHelper.DrawnTb = new TextBlock();


            m_DrawHelper.Rect_StartPt = new CPoint(rect.Left,rect.Top);
            m_DrawHelper.Rect_EndPt = new CPoint(rect.Right, rect.Bottom);

            int w = m_DrawHelper.Rect_EndPt.X - m_DrawHelper.Rect_StartPt.X;
            int h = m_DrawHelper.Rect_EndPt.Y - m_DrawHelper.Rect_StartPt.Y;
            string msg = "Width :" + w.ToString() + " Height :" + h.ToString();
            m_DrawHelper.DrawnTb.Text = msg;

            CPoint StartPt = GetCanvasPoint(m_DrawHelper.Rect_StartPt.X, m_DrawHelper.Rect_StartPt.Y);
            CPoint EndPt = GetCanvasPoint(m_DrawHelper.Rect_EndPt.X, m_DrawHelper.Rect_EndPt.Y);
            Canvas.SetLeft(m_DrawHelper.DrawnRect, StartPt.X);
            Canvas.SetTop(m_DrawHelper.DrawnRect, StartPt.Y);
            m_DrawHelper.DrawnRect.Width = Math.Abs(StartPt.X - EndPt.X);
            m_DrawHelper.DrawnRect.Height = Math.Abs(StartPt.Y - EndPt.Y);
            SetRectStyle(3,2);
            p_ViewerUIElement.Add(m_DrawHelper.DrawnTb);
            m_DrawHelper.DrawnRect.StrokeDashArray = new DoubleCollection(1);
            p_ViewerUIElement.Add(m_DrawHelper.DrawnRect);
        }

        public CRect GetCurrentRect_MemPos()
        {
            try
            {
                CPoint StartPt = new CPoint(m_DrawHelper.Rect_StartPt.X, m_DrawHelper.Rect_StartPt.Y);
                CPoint EndPt = new CPoint(m_DrawHelper.Rect_EndPt.X, m_DrawHelper.Rect_EndPt.Y);

                return new CRect(StartPt.X, StartPt.Y, EndPt.X, EndPt.Y);
            }
            catch {
                return new CRect(0,0,0,0);
            }
        }
        private void RedrawRect()
        {
            if (m_DrawHelper == null)
                return;
            if (m_DrawHelper.DrawnRect == null)
                return;


            p_ViewerUIElement.Remove(m_DrawHelper.DrawnRect);
            m_DrawHelper.DrawnRect = new System.Windows.Shapes.Rectangle();
            CPoint StartPt = GetCanvasPoint(m_DrawHelper.Rect_StartPt.X, m_DrawHelper.Rect_StartPt.Y);
            CPoint EndPt = GetCanvasPoint(m_DrawHelper.Rect_EndPt.X, m_DrawHelper.Rect_EndPt.Y);
            if (EndPt.X < StartPt.X)
                Canvas.SetLeft(m_DrawHelper.DrawnRect, EndPt.X);
            else
                Canvas.SetLeft(m_DrawHelper.DrawnRect, StartPt.X);
            if (EndPt.Y < StartPt.Y)
                Canvas.SetTop(m_DrawHelper.DrawnRect, EndPt.Y);
            else
                Canvas.SetTop(m_DrawHelper.DrawnRect, StartPt.Y);

            SetRectStyle(1,0);
            //m_DrawHelper.DrawnRect.Stroke = System.Windows.Media.Brushes.Red;
            //m_DrawHelper.DrawnRect.StrokeThickness = 2;
            m_DrawHelper.DrawnRect.Width = Math.Abs(StartPt.X - EndPt.X);
            m_DrawHelper.DrawnRect.Height = Math.Abs(StartPt.Y - EndPt.Y);
            p_ViewerUIElement.Add(m_DrawHelper.DrawnRect);


            


        }

        private void LeftDown()
        {

            if (m_ImageData == null)
                return;
            switch (p_Mode)
            {
                case DrawingMode.None:
                    {
                        if (KeyEvent != null && KeyEvent.Key == Key.LeftShift && KeyEvent.IsDown)
                        {
                            StartDrawingRect();
                            p_Mode = DrawingMode.Drawing;
                            break;
                        }
                        else
                            StartImageMove();
                        break;
                    }
                case DrawingMode.Drawing:
                    {
                        break;
                    }
                case DrawingMode.Tool:
                    {
                        break;
                    }
            }
        }
        private void MouseMove()
        {

            if (m_ImageData == null)
                return;

            int m_nMouseMoveDelay = 0;
            switch(p_Mode)
            {
                case DrawingMode.None:
                    {                       
                        if (MouseEvent.LeftButton == MouseButtonState.Pressed && m_swMouse.ElapsedMilliseconds > m_nMouseMoveDelay)
                        {
                            CPoint point = m_ptBuffer1;
                            CanvasMovePoint_Ref(point, m_PtMouseBuffer.X - p_MouseX, m_PtMouseBuffer.Y - p_MouseY);
                            RedrawRect();
                        }
                        break;
                    }
                case DrawingMode.Drawing:
                    {
                        if (KeyEvent.IsDown)
                        {
                            DrawingRectProgress();
                        }
                        else
                        {
                            DrawRectDone();
                            p_Mode = DrawingMode.None;
                        }
                        break;
                    }
                case DrawingMode.Tool:
                    {
                        break;
                    }
            }
            
        }
        private void RightDown()
        {
            DrawRectDone();
            p_Mode = DrawingMode.None;
        }
        public void KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            KeyEvent = e;
        }
        public void OnMouseDown(Object sender, System.Windows.Input.MouseEventArgs e)
        {
            var viewer = (Canvas)sender;
            viewer.Focus();
        }
        public void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            
            var viewer = (Canvas)sender;
            viewer.Focus();
            try
            {
                int lines = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
                double zoom = _Zoom;

                if(lines < 0)
                {
                    zoom *= 1.1F;    
                }
                if(lines > 0)
                {
                    zoom *= 0.9F;
                }

                //double nDev = 0;
                //int absDelta = Math.Abs(e.Delta);
                //if ((minMouseDelta > absDelta) && (absDelta > 0))
                //    minMouseDelta = absDelta;
                //int factor = absDelta / minMouseDelta;
                //if (factor < 1)
                //    factor = 1;
                //if (factor > 6)
                //    factor = 6;
                //if (e.Delta > 0)
                //    nDev = -0.01;
                //else 
                //    nDev = 0.01;

                //    //p_Zoom += (nDev * Factor[factor-1]);
                //   p_Zoom += nDev;
                if (zoom > 1)
                {
                    zoom = 1;
                }
                else if (p_Zoom < 0.0001)
                {
                    zoom = 0.0001;
                }
                p_Zoom = zoom;
                //SetRoiRect();
            }
            catch(Exception ee)
            {
                System.Windows.Forms.MessageBox.Show(ee.ToString());
            }
            RedrawRect();
        }

        #region ICommand
        
        public ICommand SaveImageCommand
        {
            get
            {
                return new RelayCommand(p_ImageData.SaveWholeImage);
            }
        }
        public ICommand CanvasMouseLeftDown
        {
            get
            {
                return new RelayCommand(LeftDown);
            }
        }
        public ICommand CanvasMouseMove
        {
            get
            {
                return new RelayCommand(MouseMove);
            }
        }
        public ICommand btnClickOpenImage
        {
            get
            {
                return new RelayCommand(_btnOpenImage);
            }
        }
        public ICommand btnClickSaveImage
        {
            get
            {
                return new RelayCommand(_btnSaveImage);
            }
        }
        public ICommand CopyCancelCommand
        {
            get
            {
                return new RelayCommand(CancelCommand);
            }
        }
        public ICommand CommandImageClear
        {
            get
            {
                return new RelayCommand(ImageClear);
            }
        }
        public ICommand TumbNailMouseMove
        {
            get{
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
        public ICommand KeyDelCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    p_ViewerUIElement.Clear();
                }
                );
            }
        }
        public ICommand KeyUpCommand
        {
            get
            {
                return new RelayCommand(delegate {
                    CanvasMoveCanvasPoint(Convert.ToInt32(p_CanvasWidth / 2), Convert.ToInt32(p_CanvasHeight / 4));
                }
                );
            }
        }
        public ICommand KeyDownCommand
        {
            get
        {
                return new RelayCommand(delegate {

                    CanvasMoveCanvasPoint(Convert.ToInt32(p_CanvasWidth / 2), Convert.ToInt32(p_CanvasHeight *3/ 4));
                }
                );
            }
        }
        public ICommand KeyLeftCommand
        {
            get
        {
                return new RelayCommand(delegate {
                    CanvasMoveCanvasPoint(Convert.ToInt32(p_CanvasWidth / 4), Convert.ToInt32(p_CanvasHeight / 2));
        }
                );
            }
        }
        public ICommand KeyRightCommand
        {
            get
        {
                return new RelayCommand(delegate {
                    CanvasMoveCanvasPoint(Convert.ToInt32(p_CanvasWidth * 3/ 4), Convert.ToInt32(p_CanvasHeight / 2));
                }
                );
            }
        }
        public ICommand CanvasMouseRightDown
        {
            get
            {
                return new RelayCommand(RightDown);
            }
        }
        public ICommand LoadedCommand
        {
            get
            {
                return new RelayCommand(Loaded_function);
            }
        }
        public event LoadedDelegate m_AfterLoaded;
        void Loaded_function()
        {
            if (p_ImageData != null)
            {
                SetRoiRect();
                SetThumNailIamge();
            }
            if(m_AfterLoaded!=null)
                m_AfterLoaded();
            RedrawRect();
        }
        #endregion 
    }
    public class DrawHelper
    {
        public TextBlock DrawnTb = new TextBlock();
        public System.Windows.Shapes.Rectangle DrawnRect = new System.Windows.Shapes.Rectangle();
        public CPoint Rect_StartPt = new CPoint();
        public CPoint Rect_EndPt = new CPoint();
        public CRect preRect = new CRect();
    }
}
