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
        public bool Circle = false;

        public MiniViewer_ViewModel(ImageData image = null, bool bRotate = false, bool bCircle = false)
        {
            if (image != null)
            {
                p_ImageData = image;
                Rotate = bRotate;
                Circle = bCircle;
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
                p_View_Rect = new System.Drawing.Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(p_ImageData.p_Size.X), Convert.ToInt32(p_ImageData.p_Size.Y));
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
                        else if (Circle)
                        {
                            IntPtr ptrMemSide = m_ImageData.GetPtr(1);
                            IntPtr ptrMemBtm = m_ImageData.GetPtr(2);
                            CPoint center = new CPoint(p_CanvasWidth / 2, p_CanvasHeight / 2);
                            int nLen = 30;
                            int nRadius = p_CanvasWidth < p_CanvasHeight ? p_CanvasWidth : p_CanvasHeight;
                            int nRadius2 = nRadius - nLen;
                            int nRadius3 = nRadius - nLen *2;
                            
                            double theta = 0;
                            double max = Math.Pow(nRadius / 2, 2);
                            double min = Math.Pow(nRadius / 2 - nLen/2, 2);
                            double max2 = Math.Pow(nRadius2 / 2, 2);
                            double min2 = Math.Pow(nRadius2 / 2 - nLen / 2, 2);
                            double max3 = Math.Pow(nRadius3 / 2, 2);
                            double min3 = Math.Pow(nRadius3 / 2 - nLen / 2, 2);
                            double dist = 0;
                            double offset = 51;
                            for (int yy = 0; yy < p_CanvasHeight; yy++)
                            {
                                for (int xx = 0; xx < p_CanvasWidth; xx++)
                                {
                                    dist = Math.Pow(Math.Abs(center.X - xx), 2) + Math.Pow(Math.Abs(center.Y - yy), 2);
                                    if (dist < max && dist > min)
                                    {
                                        theta = Math.Atan2(((double)xx - center.X), ((double)center.Y - yy)) * 180 / Math.PI +offset;
                                        if (theta < 0)
                                        {
                                            theta = theta + 360;
                                        }
                                        pix_x = 3000 - Convert.ToInt32((dist - min) * p_View_Rect.Width / (max - min));
                                        pix_y = Convert.ToInt32(theta * p_View_Rect.Height / 360);
                                        if (pix_y < 500)
                                            pix_y = 500;
                                        view.Data[yy, xx, 0] = ((byte*)ptrMem)[(long)pix_x + (long)pix_y * p_ImageData.p_Size.X];
                                    }
                                    else if (dist < max2 && dist > min2)
                                    {
                                        theta = Math.Atan2(((double)xx - center.X), ((double)center.Y - yy)) * 180 / Math.PI +45 + offset;
                                        if (theta < 0)
                                        {
                                            theta = theta + 360;
                                        }
                                        pix_x = (Convert.ToInt32((dist - min2) * p_View_Rect.Width / (max2 - min2)) * 600) / 3000 + 1200;
                                        pix_y = Convert.ToInt32(theta * p_View_Rect.Height / 360);
                                        if (pix_y < 500)
                                            pix_y = 500;
                                        view.Data[yy, xx, 0] = ((byte*)ptrMemSide)[(long)pix_x + (long)pix_y * p_ImageData.p_Size.X];
                                    }
                                    else if (dist < max3 && dist > min3)
                                    {
                                        theta = Math.Atan2(((double)xx - center.X), ((double)center.Y - yy)) * 180 / Math.PI + 90 + offset;
                                        if (theta < 0)
                                        {
                                            theta = theta + 360;
                                        }
                                        pix_x = Convert.ToInt32((dist - min3) * p_View_Rect.Width / (max3 - min3));
                                        pix_y = Convert.ToInt32(theta * p_View_Rect.Height / 360);
                                        if (pix_y < 500)
                                            pix_y = 500;
                                        view.Data[yy, xx, 0] = ((byte*)ptrMemBtm)[(long)pix_x + (long)pix_y * p_ImageData.p_Size.X];
                                    }

                                    //else if (dist < min)
                                    //{
                                    //    view.Data[yy, xx, 0] = 200;
                                    //}
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
