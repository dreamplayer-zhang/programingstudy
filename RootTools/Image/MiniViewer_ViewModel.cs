using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RootTools
{
    public class MiniViewer_ViewModel : ObservableObject
    {
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
                //SetRoiRect();
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
                SetRoiRect();
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
        private double _Zoom = 1;
        public double p_Zoom
        {
            get
            {
                return _Zoom;
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

        public bool Rotate = false;

        public MiniViewer_ViewModel(ImageData image = null, bool bRotate = false)
        {
            if (image != null)
            {
                p_ImageData = image;
                Rotate = bRotate;
                //image.OnCreateNewImage += image_NewImage;
                //image.OnUpdateImage += image_OnUpdateImage;
                //image.UpdateOpenProgress += image_UpdateOpenProgress;
                InitRoiRect(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
                SetImageSource();
            }
        }
        public void SetImageData(ImageData image)
        {
            p_ImageData = null;
            p_ImageData = image;
            //image.OnCreateNewImage += image_NewImage;
            //image.OnUpdateImage += image_OnUpdateImage;
            //image.UpdateOpenProgress += image_UpdateOpenProgress;
            //InitRoiRect(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
            InitRoiRect(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
            SetImageSource();
        }
        public void InitRoiRect(int nWidth, int nHeight)
        {
            bool bRatio_WH = false;

            if (Rotate)
            {
                if (p_ImageData == null)
                {
                    p_View_Rect = new System.Drawing.Rectangle(0, 0, nHeight, nWidth);
                }
                bRatio_WH = (double)p_View_Rect.Width / p_CanvasWidth < (double)p_View_Rect.Height / p_CanvasHeight;
                //m_View_Rect = new Rectangle(m_View_Rect.X, m_View_Rect.Y, m_View_Rect.Width, m_View_Rect.Height * ViewWidth / imgWidth);
                if (bRatio_WH)//세로가 길어
                {
                    p_View_Rect = new System.Drawing.Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Width * p_CanvasHeight / p_CanvasWidth, p_View_Rect.Width);
                }
                else
                {
                    p_View_Rect = new System.Drawing.Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Height, p_View_Rect.Height * p_CanvasWidth / p_CanvasHeight);
                }
            }
            else
            {
                if (p_ImageData == null)
                {
                    p_View_Rect = new System.Drawing.Rectangle(0, 0, nWidth, nHeight);
                }
                bRatio_WH = (double)p_View_Rect.Width / p_CanvasWidth < (double)p_View_Rect.Height / p_CanvasHeight;
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
            }
        }

        public void SetRoiRect()
        {
            if (p_ImageData != null)
            {
                CPoint StartPt = new CPoint(0, 0);
                bool bRatio_WH = false;

                
                    //bRatio_WH = (double)p_ImageData.p_Size.X / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;
                    //if (bRatio_WH)
                    //{ //세로가 길어
                        p_View_Rect = new System.Drawing.Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(p_ImageData.p_Size.X), Convert.ToInt32(p_ImageData.p_Size.Y));
                    //}
                    //else
                    //{
                    //    p_View_Rect = new System.Drawing.Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(p_ImageData.p_Size.X * p_CanvasHeight / p_CanvasWidth), Convert.ToInt32(p_ImageData.p_Size.X ));
                    //}
                
                //	_View_Rect.Height += 1;
                SetImageSource();
            }
        }

        public unsafe void SetImageSource()
        {
            try
            {
                if (p_ImageData != null)
                {
                    if (p_ImageData.p_nByte == 1)
                    {
                        Image<Gray, byte> view = new Image<Gray, byte>(p_CanvasWidth, p_CanvasHeight);
                        IntPtr ptrMem = m_ImageData.GetPtr();
                        if (ptrMem == IntPtr.Zero)
                            return;
                        int pix_x = 0;
                        int pix_y = 0;
                        if (Rotate)
                        {
                            for (int yy = 0; yy < p_CanvasHeight; yy++)
                            {
                                for (int xx = 0; xx < p_CanvasWidth; xx++)
                                {
                                    pix_y = p_View_Rect.X + xx * p_View_Rect.Height / p_CanvasWidth;
                                    pix_x = p_View_Rect.Y + yy * p_View_Rect.Width / p_CanvasHeight;
                                    view.Data[yy, xx, 0] = ((byte*)ptrMem)[(long)pix_x + (long)pix_y * p_ImageData.p_Size.X];
                                }
                            }
                        }
                        else
                        {
                            for (int yy = 0; yy < p_CanvasHeight; yy++)
                            {
                                for (int xx = 0; xx < p_CanvasWidth; xx++)
                                {
                                    pix_x = p_View_Rect.X + xx * p_View_Rect.Width / p_CanvasWidth;
                                    pix_y = p_View_Rect.Y + yy * p_View_Rect.Height / p_CanvasHeight;
                                    view.Data[yy, xx, 0] = ((byte*)ptrMem)[(long)pix_x + (long)pix_y * p_ImageData.p_Size.X];
                                }
                            }
                        }

                        p_ImgSource = ImageHelper.ToBitmapSource(view);
                    }
                    else if (p_ImageData.p_nByte == 3)
                    {
                        Image<Rgb, byte> view = new Image<Rgb, byte>(p_CanvasWidth, p_CanvasHeight);
                        IntPtr ptrMem = m_ImageData.GetPtr();
                        if (ptrMem == IntPtr.Zero)
                            return;
                        int pix_x = 0;
                        int pix_y = 0;
                        long pix_rect;

                        for (int yy = 1; yy < p_CanvasHeight; yy++)
                        {
                            pix_y = p_View_Rect.Y + yy * p_View_Rect.Height / p_CanvasHeight;
                            pix_rect = (long)pix_y * p_ImageData.p_Size.X;
                            for (int xx = 0; xx < p_CanvasWidth; xx++)
                            {
                                pix_x = p_View_Rect.X + xx * p_View_Rect.Width / p_CanvasWidth;

                                view.Data[yy, xx, 2] = ((byte*)ptrMem)[0 + m_ImageData.p_nByte * ((long)pix_x + pix_rect)];
                                view.Data[yy, xx, 1] = ((byte*)ptrMem)[1 + m_ImageData.p_nByte * ((long)pix_x + pix_rect)];
                                view.Data[yy, xx, 0] = ((byte*)ptrMem)[2 + m_ImageData.p_nByte * ((long)pix_x + pix_rect)];

                            }
                        }
                        p_ImgSource = ImageHelper.ToBitmapSource(view);
                    }

                }
            }
            catch (Exception ee)
            {
                System.Windows.MessageBox.Show(ee.ToString());
            }
        }
    }
}
