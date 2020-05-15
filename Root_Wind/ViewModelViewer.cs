using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
using System.Collections.ObjectModel;
using System.Windows.Input;
using RootTools.Control.Ajin;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;
using System.Globalization;
using System.Windows.Media;
using System.Windows;
using System.Reflection;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.Util;
using System.Windows.Interactivity;
using System.Threading;
using Emgu.CV.Util;
using RootTools.Memory;


/* eventtriger eventname 
 * SelectionChanged
 * MouseDoub,leClick
 * MouseLeftButtonDown
 */

namespace Root_Wind
{

    public class ViewModelViewer : ObservableObject
    {

        Wind_Engineer m_Engineer;
        public MemoryTool m_MemoryModule;
        System.Windows.Forms.Timer m_Timer = new System.Windows.Forms.Timer();
        int m_ProgessPercentage = 0;
        public int p_ProgessPercentage
        {
            get
            {
                return m_ProgessPercentage;
            }
            set
            {
                m_ProgessPercentage = value;
                RaisePropertyChanged("p_ProgessPercentage");
            }
        }
        BackgroundWorker Worker_MemoryCopy = new BackgroundWorker();
        BackgroundWorker Worker_MemorySave = new BackgroundWorker();

        int m_PoolSelectedIndex = 0;
        public int p_PoolSelectedIndex
        {
            get
            {
                return m_PoolSelectedIndex;
            }
            set
            {
                SetProperty(ref m_PoolSelectedIndex, value);
            }
        }
        ObservableCollection<MemoryPool> m_PoolData = new ObservableCollection<MemoryPool>();
        public ObservableCollection<MemoryPool> p_PoolData
        {
            get
            {
                return m_PoolData;
            }
            set
            {
                SetProperty(ref m_PoolData, value);

            }
        }

        int m_GroupSelectedIndex = 0;
        public int p_GroupSelectedIndex
        {
            get
            {
                return m_GroupSelectedIndex;
            }
            set
            {
                SetProperty(ref m_GroupSelectedIndex, value);
            }
        }
        ObservableCollection<MemoryGroup> m_GroupData = new ObservableCollection<MemoryGroup>();
        public ObservableCollection<MemoryGroup> p_GroupData
        {
            get
            {
                return m_GroupData;
            }
            set
            {
                SetProperty(ref m_GroupData, value);
            }
        }

        int m_MemorySelectedIndex = 0;
        public int p_MemorySelectedIndex
        {
            get
            {
                return m_MemorySelectedIndex;
            }
            set
            {
                SetProperty(ref m_MemorySelectedIndex, value);
            }
        }
        ObservableCollection<MemoryData> m_MemoryData = new ObservableCollection<MemoryData>();
        public ObservableCollection<MemoryData> p_MemoryData
        {
            get
            {
                return m_MemoryData;
            }
            set
            {
                SetProperty(ref m_MemoryData, value);
            }
        }

        string m_sFileName = "NewPool";
        public string p_sFileName
        {
            get
            {
                return m_sFileName;
            }
            set
            {
                SetProperty(ref m_sFileName, value);
            }
        }

        string m_sPoolName = "NewPool";
        public string p_sPoolName
        {
            get
            {
                return m_sPoolName;
            }
            set
            {
                SetProperty(ref m_sPoolName, value);
            }
        }
        string m_sGroupName = "NewGroup";
        public string p_sGroupName
        {
            get
            {
                return m_sGroupName;
            }
            set
            {
                SetProperty(ref m_sGroupName, value);
            }
        }
        string m_sMemName = "NewMemory";
        public string p_sMemName
        {
            get
            {
                return m_sMemName;
            }
            set
            {
                SetProperty(ref m_sMemName, value);
            }
        }

        int m_nNewMemHeigth = 30000;

        public int p_nNewMemHeigth
        {
            get
            {
                return m_nNewMemHeigth;
            }
            set
            {
                SetProperty(ref m_nNewMemHeigth, value);
            }
        }
        int m_nNewMemWidth = 30000;
        public int p_nNewMemWidth
        {
            get
            {
                return m_nNewMemWidth;
            }
            set
            {
                SetProperty(ref m_nNewMemWidth, value);
            }
        }

        int m_nMemHeigth = 0;
        public int p_nMemHeight
        {
            get
            {
                return m_nMemHeigth;
            }
            set
            {
                SetProperty(ref m_nMemHeigth, value);
            }
        }
        int m_nMemWidth = 0;
        public int p_nMemWidth
        {
            get
            {
                return m_nMemWidth;
            }
            set
            {
                SetProperty(ref m_nMemWidth, value);
            }
        }
        int m_nMemSize = 0;
        public int p_nMemSize
        {
            get
            {
                return m_nMemSize;
            }
            set
            {
                SetProperty(ref m_nMemSize, value);
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

        int ViewHeight = 100;
        int ViewWidth = 100;
        public int p_ViewHeight
        {
            get
            {
                return ViewHeight;
            }
            set
            {
                SetProperty(ref ViewHeight, value);
            }
        }
        public int p_ViewWidth
        {
            get
            {
                return ViewWidth;
            }
            set
            {
                SetProperty(ref ViewWidth, value);
            }
        }

        private int _GV;
        private int _ptMemX = 0;
        private int _ptMemY = 0;
        public int p_ptMemX
        {
            get
            {
                return _ptMemX;
            }
            set
            {
                SetProperty(ref _ptMemX, value);
            }
        }
        public int p_ptMemY
        {
            get
            {
                return _ptMemY;
            }
            set
            {
                SetProperty(ref _ptMemY, value);
            }
        }

        private int _panelX;
        private int _panelY;
        public int PanelX
        {
            get
            {
                return _panelX;
            }
            set
            {
                SetProperty(ref _panelX, value);
            }
        }
        public int PanelY
        {
            get
            {
                return _panelY;
            }
            set
            {
                SetProperty(ref _panelY, value);
            }
        }
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

        Rectangle _View_Rect = new Rectangle();
        public Rectangle p_View_Rect
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

        Thickness _TumbnailImgMargin = new Thickness(0, 0, 0, 0);
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

        Rectangle _TumbnailImg_Rect = new Rectangle();
        public Rectangle p_TumbnailImg_Rect
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

        double _Zoom = 1;
        public double m_Zoom
        {
            get
            {
                return _Zoom;
            }
            set
            {
                SetProperty(ref _Zoom, value);
                //SetImage();
            }
        }

        ObservableCollection<RectItem> _RectItems = new ObservableCollection<RectItem>();
        public ObservableCollection<RectItem> p_RectItems
        {
            get
            {
                return _RectItems;
            }
            set
            {
                SetProperty(ref _RectItems, value);
            }
        }

        public ViewModelViewer(Wind_Engineer engineer)
        {
            m_Engineer = engineer;
            m_MemoryModule = engineer.ClassMemoryTool();

            //m_MemoryModule.CreateMemoryPool(m_sPoolName, 1);
            MemoryPool memoryPool = m_MemoryModule.GetPool(m_sPoolName, true);
            memoryPool.p_gbPool = 1;
            memoryPool.GetGroup("TestGroup").CreateMemory(p_sMemName, 1, 1, new CPoint(p_nNewMemWidth, p_nNewMemHeigth));
            memoryPool.GetGroup(p_sGroupName).CreateMemory(p_sMemName, 1, 1, new CPoint(p_nNewMemWidth, p_nNewMemHeigth));
            //m_MemoryModule.GetMemoryPool(m_sPoolName).NewMemoryData(p_sGroupName, p_sMemName, 1, 1, new CPoint(p_nNewMemWidth, p_nNewMemHeigth));
            Worker_MemoryCopy.DoWork += Worker_MemoryCopy_DoWork;
            Worker_MemoryCopy.RunWorkerCompleted += Worker_MemoryCopy_RunWorkerCompleted;
            Worker_MemorySave.DoWork += Worker_MemorySave_DoWork;

            m_Timer.Tick += m_Timer_Tick;
            m_Timer.Interval = 100;
            m_Timer.Enabled = true;
        }

        int nThumbNailWidth = 140;
        int nThumbNailHeigth = 140;
        unsafe void SetImage()
        {
            if (m_GroupSelectedIndex >= 0)
            {
                string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
                string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
                string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

                IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
                int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
                int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;


                Image<Gray, byte> img = new Image<Gray, byte>(imgWidth, imgHeight, imgWidth, (ptrMem));   
                img.ROI = new Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Width, p_View_Rect.Height);
                Image<Gray, byte> view = img.Resize(ViewWidth, ViewHeight, Inter.Nearest);



                //double nHeightStep = ((double)p_View_Rect.Height - p_View_Rect.Y) / ViewHeight;
                //Image<Gray, byte> view = new Image<Gray, byte>(ViewWidth, ViewHeight);
                //if (nHeightStep < 1) {
                //    Image<Gray, byte> img = new Image<Gray, byte>(imgWidth, imgHeight, imgWidth, (ptrMem));
                //    img.ROI = new Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Width, p_View_Rect.Height);
                //    view = img.Copy().Resize(ViewWidth, ViewHeight, Inter.Nearest);
                //}
                //else {
                //    for (int yy = 0; yy < ViewHeight; yy += 1) {
                //        Image<Gray, byte> img = new Image<Gray, byte>(p_View_Rect.Width, 1, p_View_Rect.Width, (ptrMem + (Convert.ToInt32(yy * nHeightStep) + p_View_Rect.Y) * imgWidth + p_View_Rect.X));
                //        Image<Gray, byte> resize = img.Resize(ViewWidth, 1, Inter.Nearest);
                //        view.ROI = new Rectangle(0, yy, ViewWidth, 1);
                //        resize.CopyTo(view);
                //        //CvInvoke.cvCopy(resize, view, (IntPtr)yy); 
                //    }
                //    view.ROI = new Rectangle(0, 0, ViewWidth, ViewHeight);
                //}


                p_ImgSource = ImageHelper.ToBitmapSource(view);

                p_TumbnailImgMargin = new Thickness(Convert.ToInt32((double)p_View_Rect.X * nThumbNailWidth / imgWidth), Convert.ToInt32((double)p_View_Rect.Y * nThumbNailHeigth / imgHeight), 0, 0);
                if (Convert.ToInt32((double)p_View_Rect.Height * nThumbNailHeigth / imgHeight) == 0)
                    p_TumbnailImg_Rect = new Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * nThumbNailWidth / imgWidth), 2);
                else
                    p_TumbnailImg_Rect = new Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * nThumbNailWidth / imgWidth), Convert.ToInt32((double)p_View_Rect.Height * nThumbNailHeigth / imgHeight));

                #region test
                //Image<Gray, byte> img;
                //img = new Image<Gray, byte>(imgWidth, imgHeight, imgWidth, (ptrMem));
                ////img.ROI = new Rectangle(imgWidth / 2, imgHeight / 2, imgWidth / 2, imgHeight / 2);
                //Image<Gray, byte> crop_Img = img.Copy();
                //Image<Gray, byte> view = new Image<Gray, byte>(ViewWidth, ViewHeight);


                //view = crop_Img.Resize(ViewWidth, ViewHeight, Inter.Nearest, true);

                //bool bRatio_WH = m_View_Rect.Width / ViewWidth > m_View_Rect.Height / ViewHeight;

                //m_AspectRatioX = bRatio_WH ? (double)m_View_Rect.Height / ViewHeight : (double)m_View_Rect.Width / ViewWidth;
                //double nRatio = Convert.ToInt32(m_AspectRatioX);
                //view.Data[yy, xx, 0] = ((byte*)ptrMem)[Convert.ToInt32(m_View_Rect.X + (xx * nRatio) + imgWidth * (m_View_Rect.Y + (yy*nRatio)))];
                //  view.Data[yy, xx, 0] = ((byte*)ptrMem)[xx + yy *ViewWidth];

                // img.Save(@"C:\Image\test1.bmp");
                //for (int i = 0; i < ViewHeight; i++) {
                //    int HH = i * ratey;
                //    if(HH > imgHeight)
                //        HH = imgHeight-1;
                //    img = new Image<Gray, byte>(imgWidth, 1, imgWidth, (ptrMem + imgWidth*HH));
                //   // img.Save(@"C:\Image\test1.bmp");
                //    viewimg = img.Resize(ViewWidth, 1, Inter.Nearest);

                //    if(HH == 100)
                //        viewimg.Save(@"C:\Image\test2.bmp");
                ////    CvInvoke.VConcat(view, viewimg, view);
                //    //view += viewimg;
                //    //Emgu.CV.Util.CvToolbox.Memcpy(view.Ptr + i * ViewWidth, viewimg.Ptr, ViewWidth);

                //    memcpy((byte*)(view.Ptr + i * ViewWidth), (byte*)(viewimg.Ptr), ViewWidth);
                //    a++;
                //}

                //view.Save(@"C:\Image\test3.bmp");



                //Emgu.CV.Util.CvToolbox.Memcpy(pt, ptrMem, imgWidth * imgHeight);
                //bitmap.UnlockBits(bd);
                //view.Save(@"C:\Image\test3.bmp");

                //Bitmap tete = view.ToBitmap(view.Width,view.Height);

                //tete.Save(@"C:\Image\test4.bmp");



                //                 WriteableBitmap writeableBitmap = new WriteableBitmap(view.Width, view.Height, 96.0, 96.0, PixelFormats.Gray8, null);

                //    Messenger.Default.Register<Bitmap>(this, (bmp) =>
                //    {
                //        ImageTarget.Dispatcher.BeginInvoke((Action)(() =>
                //       {
                //            BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                //            writeableBitmap.Lock();
                //            CopyMemory(writeableBitmap.BackBuffer, data.Scan0,
                //                (writeableBitmap.BackBufferStride * bmp.Height));
                //            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, bmp.Width, bmp.Height));
                //            writeableBitmap.Unlock();
                //            bmp.UnlockBits(data);
                //        }));
                //    });
                //}
                //p_ImgSource = 
                //p_ImgSource = ImageHelper.GetImageStream(img.Bitmap);
                //p_ImgSource = ImageHelper.ToBitmapSource(view);
                // p_ImgSource = ImageHelper.GetImageStream(view.ToBitmap());
                #endregion
            }
        }

        void PoolSelectChange()
        {
        }

        void GroupSelectChange()
        {
        }

        //void CanvasZoom(double nDev)
        //{
        //    //m_cpCanvasClick = new CPoint(PanelX, PanelY);
        //    //nZoom = nZoom - nDev;
        //    //string sPool = m_PoolData[m_PoolSelectedIndex].p_sPool;
        //    //string sGroup = m_GroupData[m_GroupSelectedIndex];
        //    //string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;
        //    //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
        //    //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);


        //    //if (Convert.ToInt32(imgWidth * nZoom) <= ViewWidth || Convert.ToInt32(imgHeight * nZoom) <= ViewHeight)
        //    //    nZoom = nZoom + nDev;

        //    //int nX = Convert.ToInt32(m_View_Rect.X + (double)m_cpCanvasClick.X / ViewWidth * m_View_Rect.Width);
        //    //int nY = Convert.ToInt32(m_View_Rect.Y + (double)m_cpCanvasClick.Y / ViewHeight * m_View_Rect.Height);

        //    //if (nX + Convert.ToInt32(imgWidth * nZoom) > imgWidth) {
        //    //    nX = imgWidth - Convert.ToInt32(imgWidth * nZoom);
        //    //}
        //    //if (nY + Convert.ToInt32(imgHeight * nZoom) > imgHeight) {
        //    //    nY = imgHeight - Convert.ToInt32(imgHeight * nZoom);
        //    //}

        //    //m_View_Rect = new Rectangle(nX, nY, Convert.ToInt32(imgWidth * nZoom), Convert.ToInt32(imgHeight * nZoom));
        //    //m_AspectRatioX
        //   // nZoom = nZoom - nDev;
        //    //m_View_Rect = new Rectangle(1500, 500,Convert.ToInt32(10000 * nZoom), Convert.ToInt32(10000 * nZoom));
        //    SetImage();
        //}

        void Zoomin()
        {
            double nDev = 0;
            if (m_Zoom <= 0.02)
                nDev = -0.001;
            else
                nDev = -0.01;
            m_Zoom += nDev;

            if (m_Zoom > 1)
            {
                m_Zoom = 1;
            }
            else if (m_Zoom < 0.001)
            {
                m_Zoom = 0.001;
            }
            SetRoiRect();
        }

        void Zoomout()
        {
            Double nDev = 0.1;
            m_Zoom += nDev;

            if (m_Zoom > 1)
            {
                m_Zoom = 1;
            }
            else if (m_Zoom < 0.001)
            {
                m_Zoom = 0.001;
            }
            SetRoiRect();
        }

        public void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double nDev = 0;
            if (e.Delta > 120)
            {
                nDev = -0.1;
            }
            else if (e.Delta > 0)
            {
                if (m_Zoom <= 0.02)
                    nDev = -0.001;
                else
                    nDev = -0.01;
            }
            else if (e.Delta < -120)
            {
                nDev = 0.1;
            }
            else
            {
                nDev = 0.01;
            }
            m_Zoom += nDev;

            if (m_Zoom > 1)
            {
                m_Zoom = 1;
            }
            else if (m_Zoom < 0.001)
            {
                m_Zoom = 0.001;
            }
            SetRoiRect();
        }

        CPoint GetStartPoint(int nImgWidth, int nImgHeight)
        {
            int nX = p_View_Rect.X + p_View_Rect.Width * PanelX / ViewWidth - p_View_Rect.Width / 2;
            int nY = p_View_Rect.Y + p_View_Rect.Height * PanelY / ViewHeight - p_View_Rect.Height / 2;
            if (nX < 0)
                nX = 0;
            else if (nX > nImgWidth - p_View_Rect.Width)
                nX = nImgWidth - p_View_Rect.Width;
            if (nY < 0)
                nY = 0;
            else if (nY > nImgHeight - p_View_Rect.Height)
                nY = nImgHeight - p_View_Rect.Height;
            return new CPoint(nX, nY);
        }
        CPoint GetStartPointLeft(int nImgWidth, int nImgHeight)
        {
            int nX = p_View_Rect.X + p_View_Rect.Width * (ViewWidth / 4) / ViewWidth - p_View_Rect.Width / 2;
            int nY = p_View_Rect.Y + p_View_Rect.Height * (ViewHeight / 2) / ViewHeight - p_View_Rect.Height / 2;
            if (nX < 0)
                nX = 0;
            else if (nX > nImgWidth - p_View_Rect.Width)
                nX = nImgWidth - p_View_Rect.Width;
            if (nY < 0)
                nY = 0;
            else if (nY > nImgHeight - p_View_Rect.Height)
                nY = nImgHeight - p_View_Rect.Height;
            return new CPoint(nX, nY);
        }
        CPoint GetStartPointRight(int nImgWidth, int nImgHeight)
        {
            int nX = p_View_Rect.X + p_View_Rect.Width * (ViewWidth * 3 / 4) / ViewWidth - p_View_Rect.Width / 2;
            int nY = p_View_Rect.Y + p_View_Rect.Height * (ViewHeight / 2) / ViewHeight - p_View_Rect.Height / 2;
            if (nX < 0)
                nX = 0;
            else if (nX > nImgWidth - p_View_Rect.Width)
                nX = nImgWidth - p_View_Rect.Width;
            if (nY < 0)
                nY = 0;
            else if (nY > nImgHeight - p_View_Rect.Height)
                nY = nImgHeight - p_View_Rect.Height;
            return new CPoint(nX, nY);
        }

        CPoint GetStartPointDown(int nImgWidth, int nImgHeight)
        {
            int nX = p_View_Rect.X + p_View_Rect.Width * (ViewWidth / 2) / ViewWidth - p_View_Rect.Width / 2;
            //int nY = p_View_Rect.Y + p_View_Rect.Height * (ViewHeight *3/ 4) / ViewHeight - p_View_Rect.Height / 2;
            int nY = p_View_Rect.Y + 72;
            if (nX < 0)
                nX = 0;
            else if (nX > nImgWidth - p_View_Rect.Width)
                nX = nImgWidth - p_View_Rect.Width;
            if (nY < 0)
                nY = 0;
            else if (nY > nImgHeight - p_View_Rect.Height)
                nY = nImgHeight - p_View_Rect.Height;
            return new CPoint(nX, nY);
        }

        CPoint GetStartPointDown2(int nImgWidth, int nImgHeight)
        {
            int nX = p_View_Rect.X + p_View_Rect.Width * (ViewWidth / 2) / ViewWidth - p_View_Rect.Width / 2;
            //int nY = p_View_Rect.Y + p_View_Rect.Height * (ViewHeight *3/ 4) / ViewHeight - p_View_Rect.Height / 2;
            int nY = p_View_Rect.Y + 10;
            if (nX < 0)
                nX = 0;
            else if (nX > nImgWidth - p_View_Rect.Width)
                nX = nImgWidth - p_View_Rect.Width;
            if (nY < 0)
                nY = 0;
            else if (nY > nImgHeight - p_View_Rect.Height)
                nY = nImgHeight - p_View_Rect.Height;
            return new CPoint(nX, nY);
        }

        CPoint GetStartPointDown3(int nImgWidth, int nImgHeight)
        {
            int nX = p_View_Rect.X + p_View_Rect.Width * (ViewWidth / 2) / ViewWidth - p_View_Rect.Width / 2;
            //int nY = p_View_Rect.Y + p_View_Rect.Height * (ViewHeight *3/ 4) / ViewHeight - p_View_Rect.Height / 2;
            int nY = p_View_Rect.Y + 5013;
            if (nX < 0)
                nX = 0;
            else if (nX > nImgWidth - p_View_Rect.Width)
                nX = nImgWidth - p_View_Rect.Width;
            if (nY < 0)
                nY = 0;
            else if (nY > nImgHeight - p_View_Rect.Height)
                nY = nImgHeight - p_View_Rect.Height;
            return new CPoint(nX, nY);
        }

        CPoint GetStartPointUp2(int nImgWidth, int nImgHeight)
        {
            int nX = p_View_Rect.X + p_View_Rect.Width * (ViewWidth / 2) / ViewWidth - p_View_Rect.Width / 2;
            //int nY = p_View_Rect.Y + p_View_Rect.Height * (ViewHeight  / 4) / ViewHeight - p_View_Rect.Height / 2;
            int nY = p_View_Rect.Y - 5013;
            if (nX < 0)
                nX = 0;
            else if (nX > nImgWidth - p_View_Rect.Width)
                nX = nImgWidth - p_View_Rect.Width;
            if (nY < 0)
                nY = 0;
            else if (nY > nImgHeight - p_View_Rect.Height)
                nY = nImgHeight - p_View_Rect.Height;
            return new CPoint(nX, nY);
        }

        CPoint GetStartPointUp3(int nImgWidth, int nImgHeight)
        {
            int nX = p_View_Rect.X + p_View_Rect.Width * (ViewWidth / 2) / ViewWidth - p_View_Rect.Width / 2;
            //int nY = p_View_Rect.Y + p_View_Rect.Height * (ViewHeight  / 4) / ViewHeight - p_View_Rect.Height / 2;
            int nY = p_View_Rect.Y - 10;
            if (nX < 0)
                nX = 0;
            else if (nX > nImgWidth - p_View_Rect.Width)
                nX = nImgWidth - p_View_Rect.Width;
            if (nY < 0)
                nY = 0;
            else if (nY > nImgHeight - p_View_Rect.Height)
                nY = nImgHeight - p_View_Rect.Height;
            return new CPoint(nX, nY);
        }

        CPoint GetStartPointUp(int nImgWidth, int nImgHeight)
        {
            int nX = p_View_Rect.X + p_View_Rect.Width * (ViewWidth / 2) / ViewWidth - p_View_Rect.Width / 2;
            //int nY = p_View_Rect.Y + p_View_Rect.Height * (ViewHeight  / 4) / ViewHeight - p_View_Rect.Height / 2;
            int nY = p_View_Rect.Y - 72;
            if (nX < 0)
                nX = 0;
            else if (nX > nImgWidth - p_View_Rect.Width)
                nX = nImgWidth - p_View_Rect.Width;
            if (nY < 0)
                nY = 0;
            else if (nY > nImgHeight - p_View_Rect.Height)
                nY = nImgHeight - p_View_Rect.Height;
            return new CPoint(nX, nY);
        }

        CPoint GetStartPoint_Center(int nImgWidth, int nImgHeight)
        {
            bool bRatio_WH = (double)nImgWidth / ViewWidth < (double)nImgHeight / ViewHeight;

            int nX = 0;
            int nY = 0;
            if (bRatio_WH)
            { //세로가 길어
                nX = p_View_Rect.X + Convert.ToInt32(p_View_Rect.Width - nImgWidth * m_Zoom) / 2;
                nY = p_View_Rect.Y + Convert.ToInt32(p_View_Rect.Height - nImgWidth * m_Zoom * ViewHeight / ViewWidth) / 2;
            }
            else
            {
                nX = p_View_Rect.X + Convert.ToInt32(p_View_Rect.Width - nImgHeight * m_Zoom * ViewWidth / ViewHeight);
                nY = p_View_Rect.Y + Convert.ToInt32(p_View_Rect.Height - nImgHeight * m_Zoom);
            }

            if (nX < 0)
                nX = 0;
            else if (nX > nImgWidth - p_View_Rect.Width)
                nX = nImgWidth - p_View_Rect.Width;
            if (nY < 0)
                nY = 0;
            else if (nY > nImgHeight - p_View_Rect.Height)
                nY = nImgHeight - p_View_Rect.Height;
            return new CPoint(nX, nY);
        }


        void SetRoiRect()
        {
            string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
            string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
            string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

            //IntPtr ptrMem = m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).GetIntPtr(0);
            //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
            //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
            int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
            int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;

            CPoint StartPt = GetStartPoint_Center(imgWidth, imgHeight);
            bool bRatio_WH = (double)imgWidth / ViewWidth < (double)imgHeight / ViewHeight;
            if (bRatio_WH)
            { //세로가 길어
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgWidth * m_Zoom), Convert.ToInt32(imgWidth * m_Zoom * ViewHeight / ViewWidth));
            }
            else
            {
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgHeight * m_Zoom * ViewWidth / ViewHeight), Convert.ToInt32(imgHeight * m_Zoom));
            }

            SetImage();
        }

        void CanvasMovePointUp()
        {

            string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
            string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
            string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

            //IntPtr ptrMem = m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).GetIntPtr(0);
            //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
            //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
            int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
            int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;

            CPoint StartPt = GetStartPointUp(imgWidth, imgHeight);
            bool bRatio_WH = (double)imgWidth / ViewWidth < (double)imgHeight / ViewHeight;
            if (bRatio_WH)
            { //세로가 길어
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgWidth * m_Zoom), Convert.ToInt32(imgWidth * m_Zoom * ViewHeight / ViewWidth));
            }
            else
            {
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgHeight * m_Zoom * ViewWidth / ViewHeight), Convert.ToInt32(imgHeight * m_Zoom));
            }
            SetImage();
        }

        void testest()
        {
            string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
            string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
            string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

            //IntPtr ptrMem = m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).GetIntPtr(0);
            //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
            //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
            int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
            int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;

            CPoint StartPt = GetStartPointDown2(imgWidth, imgHeight);
            bool bRatio_WH = (double)imgWidth / ViewWidth < (double)imgHeight / ViewHeight;
            if (bRatio_WH)
            { //세로가 길어
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgWidth * m_Zoom), Convert.ToInt32(imgWidth * m_Zoom * ViewHeight / ViewWidth));
            }
            else
            {
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgHeight * m_Zoom * ViewWidth / ViewHeight), Convert.ToInt32(imgHeight * m_Zoom));
            }
            SetImage();
        }
        void testest6()
        {

        }
        void testest4()
        {
            string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
            string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
            string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

            //IntPtr ptrMem = m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).GetIntPtr(0);
            //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
            //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
            int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
            int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;

            CPoint StartPt = GetStartPointUp3(imgWidth, imgHeight);
            bool bRatio_WH = (double)imgWidth / ViewWidth < (double)imgHeight / ViewHeight;
            if (bRatio_WH)
            { //세로가 길어
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgWidth * m_Zoom), Convert.ToInt32(imgWidth * m_Zoom * ViewHeight / ViewWidth));
            }
            else
            {
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgHeight * m_Zoom * ViewWidth / ViewHeight), Convert.ToInt32(imgHeight * m_Zoom));
            }
            SetImage();
        }

        void testest2()
        {
            string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
            string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
            string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

            //IntPtr ptrMem = m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).GetIntPtr(0);
            //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
            //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
            int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
            int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;

            CPoint StartPt = GetStartPointDown3(imgWidth, imgHeight);
            bool bRatio_WH = (double)imgWidth / ViewWidth < (double)imgHeight / ViewHeight;
            if (bRatio_WH)
            { //세로가 길어
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgWidth * m_Zoom), Convert.ToInt32(imgWidth * m_Zoom * ViewHeight / ViewWidth));
            }
            else
            {
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgHeight * m_Zoom * ViewWidth / ViewHeight), Convert.ToInt32(imgHeight * m_Zoom));
            }
            SetImage();
        }
        void testest3()
        {
            string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
            string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
            string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

            //IntPtr ptrMem = m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).GetIntPtr(0);
            //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
            //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
            int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
            int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;

            CPoint StartPt = GetStartPointUp2(imgWidth, imgHeight);
            bool bRatio_WH = (double)imgWidth / ViewWidth < (double)imgHeight / ViewHeight;
            if (bRatio_WH)
            { //세로가 길어
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgWidth * m_Zoom), Convert.ToInt32(imgWidth * m_Zoom * ViewHeight / ViewWidth));
            }
            else
            {
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgHeight * m_Zoom * ViewWidth / ViewHeight), Convert.ToInt32(imgHeight * m_Zoom));
            }
            SetImage();
        }

        void CanvasMovePointDown()
        {
            Thread.Sleep(200);
            string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
            string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
            string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

            //IntPtr ptrMem = m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).GetIntPtr(0);
            //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
            //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
            int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
            int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;

            CPoint StartPt = GetStartPointDown(imgWidth, imgHeight);
            bool bRatio_WH = (double)imgWidth / ViewWidth < (double)imgHeight / ViewHeight;
            if (bRatio_WH)
            { //세로가 길어
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgWidth * m_Zoom), Convert.ToInt32(imgWidth * m_Zoom * ViewHeight / ViewWidth));
            }
            else
            {
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgHeight * m_Zoom * ViewWidth / ViewHeight), Convert.ToInt32(imgHeight * m_Zoom));
            }
            SetImage();
        }

        public Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap
            (
              source.PixelWidth,
              source.PixelHeight,
              System.Drawing.Imaging.PixelFormat.Format8bppIndexed
            );

            BitmapData data = bmp.LockBits
            (
                new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format8bppIndexed
            );

            source.CopyPixels
            (
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride
            );

            bmp.UnlockBits(data);

            return bmp;
        }

        void testest5()
        {
            Thread.Sleep(200);
            string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
            string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
            string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

            //IntPtr ptrMem = m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).GetIntPtr(0);
            //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
            //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
            int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
            int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;

            CPoint StartPt = GetStartPointDown(imgWidth, imgHeight);
            bool bRatio_WH = (double)imgWidth / ViewWidth < (double)imgHeight / ViewHeight;
            if (bRatio_WH)
            { //세로가 길어
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgWidth * m_Zoom), Convert.ToInt32(imgWidth * m_Zoom * ViewHeight / ViewWidth));
            }
            else
            {
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgHeight * m_Zoom * ViewWidth / ViewHeight), Convert.ToInt32(imgHeight * m_Zoom));
            }
            BitmapSource preimage = p_ImgSource;
            SetImage();
            BitmapSource curimage = p_ImgSource;


            Image<Gray, byte> g_image = new Image<Gray, byte>(GetBitmap(preimage));
            Image<Gray, byte> g_gray = new Image<Gray, byte>(GetBitmap(curimage));

            CvInvoke.NamedWindow("image1");
            CvInvoke.NamedWindow("image2");
            CvInvoke.Imshow("image1", g_image);
            CvInvoke.Imshow("image2", g_gray);
            CvInvoke.NamedWindow("cvAbsDiff");
            //  CvInvoke.NamedWindow("Result");

            CvInvoke.AbsDiff(g_image, g_gray, g_image);
            CvInvoke.Threshold(g_image, g_image, 50, 255, ThresholdType.Binary);

            CvInvoke.Imshow("cvAbsDiff", g_image);


            IntPtr ptr = CvInvoke.cvCreateImage(CvInvoke.cvGetSize(g_image), IplDepth.IplDepth_8U, 1);
            g_gray = new Image<Gray, byte>(g_image.Width, g_image.Height, g_image.Width, ptr);

            //g_gray = cvCreateImage( cvGetSize(g_image), 8, 1 );
            //CvMemStorage
            //CvInvoke.cvcrea
            //g_storage = cvCreateMemStorage(0);
            Image<Gray, byte> g_storage = new Image<Gray, byte>(g_image.Width, g_image.Height);

            //CvSeq* contours = 0;
            VectorOfVectorOfPoint markers = new VectorOfVectorOfPoint();


            //CvInvoke.DestroyWindow("cvAbsDiff");
            //cvCvtColor(g_image, g_gray, CV_BGR2GRAY);
            //cvFindContours(g_gray, g_storage, , sizeof(CvContour), CV_RETR_EXTERNAL, CV_CHAIN_APPROX_SIMPLE);
            ////  CvInvoke.FindContours(g_image, g_storage, markers, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            //  g_image =  new Image<Gray, byte>(GetBitmap(preimage));

            //for (int i = 0; i < markers.Size; i++){
            //      CvInvoke.DrawContours(g_image, markers, i, new MCvScalar(255, 255, 0), 2) ;
            //      //cvDrawContours(g_image, contours, CV_RGB(255, 255, 0), CV_RGB(0, 255, 0), 100, -1, CV_AA);
            //  }

            //  CvInvoke.Imshow("Result", g_image);

            //CvInvoke.1 cvWaitKey();

            //cvReleaseImage(&g_image);
            //cvReleaseImage(&g_gray);
            //cvReleaseMemStorage(&g_storage);
            //CvInvoke.DestroyWindow("cvAbsDiff");
            //CvInvoke.DestroyWindow("Result");
        }

        void CanvasMovePointRight()
        {
            string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
            string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
            string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

            //IntPtr ptrMem = m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).GetIntPtr(0);
            //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
            //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
            int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
            int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;

            CPoint StartPt = GetStartPointRight(imgWidth, imgHeight);
            bool bRatio_WH = (double)imgWidth / ViewWidth < (double)imgHeight / ViewHeight;
            if (bRatio_WH)
            { //세로가 길어
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgWidth * m_Zoom), Convert.ToInt32(imgWidth * m_Zoom * ViewHeight / ViewWidth));
            }
            else
            {
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgHeight * m_Zoom * ViewWidth / ViewHeight), Convert.ToInt32(imgHeight * m_Zoom));
            }
            SetImage();
        }
        void CanvasMovePointLeft()
        {
            string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
            string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
            string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

            //IntPtr ptrMem = m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).GetIntPtr(0);
            //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
            //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
            int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
            int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;

            CPoint StartPt = GetStartPointLeft(imgWidth, imgHeight);
            bool bRatio_WH = (double)imgWidth / ViewWidth < (double)imgHeight / ViewHeight;
            if (bRatio_WH)
            { //세로가 길어
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgWidth * m_Zoom), Convert.ToInt32(imgWidth * m_Zoom * ViewHeight / ViewWidth));
            }
            else
            {
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgHeight * m_Zoom * ViewWidth / ViewHeight), Convert.ToInt32(imgHeight * m_Zoom));
            }
            SetImage();
        }
        public bool bClick = false;

        void CanvasMovePoint()
        {
            string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
            string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
            string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

            //IntPtr ptrMem = m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).GetIntPtr(0);
            //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
            //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
            int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
            int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;

            CPoint StartPt = GetStartPoint(imgWidth, imgHeight);
            bool bRatio_WH = (double)imgWidth / ViewWidth < (double)imgHeight / ViewHeight;
            if (bRatio_WH)
            { //세로가 길어
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgWidth * m_Zoom), Convert.ToInt32(imgWidth * m_Zoom * ViewHeight / ViewWidth));
            }
            else
            {
                p_View_Rect = new Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(imgHeight * m_Zoom * ViewWidth / ViewHeight), Convert.ToInt32(imgHeight * m_Zoom));
            }
            SetImage();
        }


        void InitRoiRect(int nWidth, int nHeight)
        {
            p_View_Rect = new Rectangle(0, 0, nWidth, nHeight);
            m_Zoom = 1;

            bool bRatio_WH = (double)p_View_Rect.Width / ViewWidth < (double)p_View_Rect.Height / ViewHeight;
            //m_View_Rect = new Rectangle(m_View_Rect.X, m_View_Rect.Y, m_View_Rect.Width, m_View_Rect.Height * ViewWidth / imgWidth);
            if (bRatio_WH)
            { //세로가 길어
                p_View_Rect = new Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Width, p_View_Rect.Height * ViewHeight / ViewWidth);
            }
            else
            {
                p_View_Rect = new Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Width * ViewWidth / ViewHeight, p_View_Rect.Height);
            }

            string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
            string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
            string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

            //IntPtr ptrMem = m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).GetIntPtr(0);
            //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
            //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
            int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
            int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;

            Image<Gray, byte> img = new Image<Gray, byte>(imgWidth, imgHeight, imgWidth, (ptrMem));
            Image<Gray, byte> view = img.Resize(nThumbNailWidth, nThumbNailHeigth, Inter.Nearest);
            p_ThumNailImgSource = ImageHelper.ToBitmapSource(view);
        }

        void MemoryOpen()
        {
            string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
            string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
            string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

            //IntPtr ptrMem = m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).GetIntPtr(0);
            //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
            //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
            int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
            int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;

            p_nMemWidth = imgWidth;
            p_nMemHeight = imgHeight;
            p_nMemSize = imgWidth * imgHeight;
            InitRoiRect(imgWidth, imgHeight);
            SetImage();

        }

        void m_Timer_Tick(object sender, EventArgs e)
        {
            if (m_MemoryModule.m_aPool.Count != m_PoolData.Count)
            {
                m_PoolData.Clear();
                m_GroupData.Clear();
                m_MemoryData.Clear();
                foreach (MemoryPool data in m_MemoryModule.m_aPool)
                    m_PoolData.Add(data);
            }

            if (m_PoolSelectedIndex >= 0)
            {
                if (m_MemoryModule.GetPool(m_PoolData[m_PoolSelectedIndex].p_id, true).p_aGroup.Count != m_GroupData.Count)
                {
                    m_GroupData.Clear();
                    m_MemoryData.Clear();
                    foreach (MemoryGroup data in m_MemoryModule.GetPool(m_PoolData[m_PoolSelectedIndex].p_id, true).p_aGroup)
                        m_GroupData.Add(data);
                }
            }

            if (m_GroupSelectedIndex >= 0)
            {
                if (m_MemoryModule.GetPool(m_PoolData[m_PoolSelectedIndex].p_id, true).GetGroup(m_GroupData[m_GroupSelectedIndex].p_id).p_aMemory.Count != m_MemoryData.Count)
                {
                    m_MemoryData.Clear();
                    foreach (MemoryData data in m_MemoryModule.GetPool(m_PoolData[m_PoolSelectedIndex].p_id, true).GetGroup(m_GroupData[m_GroupSelectedIndex].p_id).p_aMemory)
                    {
                        m_MemoryData.Add(data);
                    }
                }
            }

            if (p_ImgSource != null)
            {
                byte[] pixel = new byte[1];
                if (PanelX < p_ImgSource.Width && PanelY < p_ImgSource.Height)
                {
                    p_ImgSource.CopyPixels(new Int32Rect(PanelX, PanelY, 1, 1), pixel, ViewWidth, 0);
                    p_GV = pixel[0];
                    p_ptMemY = p_View_Rect.Y + PanelY * p_View_Rect.Height / ViewHeight;
                    p_ptMemX = p_View_Rect.X + PanelX * p_View_Rect.Width / ViewWidth;
                }
            }
        }

        void AddMemory()
        {
            //m_MemoryModule.GetMemoryPool(m_sPoolName).NewMemoryData(p_sGroupName, p_sMemName, 1, 1, new CPoint(p_nNewMemWidth, p_nNewMemHeigth));
        }

        void SaveImage()
        {
            SaveFileDialog ofd = new SaveFileDialog();
            ofd.Filter = "BMP파일|*.bmp";
            ;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                SaveBMP(ofd.FileName, p_ImgSource);
            }
        }

        void SaveWholeImage()
        {
            //p_RectItems[0].Width = 200;

            SaveFileDialog ofd = new SaveFileDialog();
            ofd.Filter = "BMP파일|*.bmp";
            ;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                List<object> arguments = new List<object>();
                arguments.Add(ofd.FileName);
                Worker_MemorySave.RunWorkerAsync(arguments);

            }
        }

        void SaveBMP(string fileName, BitmapSource bmp)
        {
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            BitmapFrame outputFrame = BitmapFrame.Create(bmp);
            encoder.Frames.Add(outputFrame);
            using (FileStream file = File.OpenWrite(fileName))
            {
                encoder.Save(file);
            }
        }


        void OpenImage()
        {
            if (p_GroupSelectedIndex >= 0 && p_GroupSelectedIndex < p_GroupData.Count)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "BMP파일|*.bmp";
                ;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    List<object> arguments = new List<object>();
                    arguments.Add(ofd.FileName);
                    Worker_MemoryCopy.RunWorkerAsync(arguments);
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("왼쪽의 Memory 공간을 선택하여 주세요.");
            }
        }

        public ICommand btnSnapDialogShow
        {
            get
            {
                return new RelayCommand(ShowSnapDialog);
            }
        }

        private void ShowSnapDialog()
        {
       
        }

        public ICommand btnClickAddMemory
        {
            get
            {
                return new RelayCommand(AddMemory);
            }
        }

        public RelayCommand btnClickOpenImage
        {
            get
            {
                return new RelayCommand(OpenImage);
            }
        }

        public ICommand btnClickSaveImage
        {
            get
            {
                return new RelayCommand(SaveImage);
            }
        }

        public ICommand btnClickSaveWholeImage
        {
            get
            {
                return new RelayCommand(SaveWholeImage);
            }
        }

        public ICommand PoolListviewItemDoubleClick
        {
            get
            {
                return new RelayCommand(PoolSelectChange);
            }
        }

        public ICommand GroupListviewItemDoubleClick
        {
            get
            {
                return new RelayCommand(GroupSelectChange);
            }
        }

        public ICommand ListviewItemDoubleClick
        {
            get
            {
                return new RelayCommand(MemoryOpen);
            }
        }

        public ICommand CanvasLClick
        {
            get
            {
                return new RelayCommand(CanvasMovePoint);
            }
        }
        public ICommand GoLeftCommand
        {
            get
            {
                return new RelayCommand(CanvasMovePointLeft);
            }
        }
        public ICommand GoRightCommand
        {
            get
            {
                return new RelayCommand(CanvasMovePointRight);
            }
        }

        public ICommand GoBottomCommand
        {
            get
            {
                return new RelayCommand(CanvasMovePointDown);
            }
        }

        public ICommand GoUPCommand
        {
            get
            {
                return new RelayCommand(CanvasMovePointUp);
            }
        }

        public ICommand ZoomOutCommand
        {
            get
            {
                return new RelayCommand(Zoomout);
            }
        }
        public ICommand ZoomInCommand
        {
            get
            {
                return new RelayCommand(Zoomin);
            }
        }

        public ICommand testCommand
        {
            get
            {
                return new RelayCommand(testest);
            }
        }
        public ICommand testCommand2
        {
            get
            {
                return new RelayCommand(testest2);
            }
        }

        public ICommand testCommand3
        {
            get
            {
                return new RelayCommand(testest3);
            }
        }
        public ICommand testCommand4
        {
            get
            {
                return new RelayCommand(testest4);
            }
        }
        public ICommand testCommand5
        {
            get
            {
                return new RelayCommand(testest5);
            }
        }
        public ICommand testCommand6
        {
            get
            {
                return new RelayCommand(testest6);
            }
        }


        bool bDrawRect = false;
        public ICommand CanvasRClickDown
        {
            get
            {
                return new RelayCommand(SetDrawRect);
            }
        }
        public ICommand CanvasRClickUp
        {
            get
            {
                return new RelayCommand(EndDrawRect);
            }
        }

//        Thread m_Thread;

        void RunThreadProcess()
        {
            while (bDrawRect)
            {
                Thread.Sleep(10);
                if (bDrawRect && p_RectItems[0] != null)
                {
                    int xx = p_RectItems[0].X;
                    int yy = p_RectItems[0].Y;
                    if (PanelX - xx > 0)
                        p_RectItems[0].Width = PanelX - xx;

                    if (PanelY - yy > 0)
                        p_RectItems[0].Height = PanelY - yy;
                }
            }
        }

        void SetDrawRect()
        {
            //if (!bDrawRect) {
            //    p_RectItems.Clear();
            //    bDrawRect = true;
            //    m_Thread = new Thread(new ThreadStart(RunThreadProcess));
            //    p_RectItems.Add(new RectItem(PanelX, PanelY, 100, 100, System.Windows.Media.Brushes.Red));
            //    m_Thread.Start();
            //    p_RectItems.Add(new RectItem(PanelX + 100, PanelY + 100, 100, 100, System.Windows.Media.Brushes.Blue));
            //}
        }
        void EndDrawRect()
        {
            //bDrawRect = false;
            //m_Thread.Join();
        }





        void Worker_MemoryCopy_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            p_ProgessPercentage = e.ProgressPercentage;
        }

        unsafe void FileSaveBMP(string sFile, IntPtr ptr, Rectangle rect)
        {
            FileStream fs = new FileStream(sFile, FileMode.CreateNew, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);

            bw.Write(Convert.ToUInt16(0x4d42));
            bw.Write(Convert.ToUInt32(54 + 1024 + rect.Width * rect.Height));
            //image 크기 bw.Write();   bmfh.bfSize = sizeof(14byte) + nSizeHdr + rect.right * rect.bottom;
            bw.Write(Convert.ToUInt16(0));   //reserved
            bw.Write(Convert.ToUInt16(0));   //reserved
            bw.Write(Convert.ToUInt32(1078));

            bw.Write(Convert.ToUInt32(40));
            bw.Write(Convert.ToInt32(rect.Width));
            bw.Write(Convert.ToInt32(rect.Height));
            bw.Write(Convert.ToUInt16(1));
            bw.Write(Convert.ToUInt16(8));     //byte                      
            bw.Write(Convert.ToUInt32(0));      //compress
            bw.Write(Convert.ToUInt32(rect.Width * rect.Height));
            bw.Write(Convert.ToInt32(0));
            bw.Write(Convert.ToInt32(0));
            bw.Write(Convert.ToUInt32(256));      //color
            bw.Write(Convert.ToUInt32(256));      //import
            for (int i = 0; i < 256; i++)
            {
                bw.Write(Convert.ToByte(i));
                bw.Write(Convert.ToByte(i));
                bw.Write(Convert.ToByte(i));
                bw.Write(Convert.ToByte(255));
            }
            byte[] aBuf = new byte[rect.Width];
            for (int i = rect.Height - 1; i >= 0; i--)
            {
                //Emgu.CV.Util.CvToolbox.Memcpy(aBuf, ptr, rect.Width);
                Marshal.Copy(ptr + i * rect.Width, aBuf, 0, rect.Width);
                p_ProgessPercentage = Convert.ToInt32(((double)(rect.Height - i) / rect.Height) * 100);
                bw.Write(aBuf);
            }
            bw.Close();
            fs.Close();
        }


        unsafe void FileOpenBMP(string sFile, ref IntPtr ptr, ref int width, ref int height)
        {
            p_sFileName = sFile;
            int nByte;
            CPoint szImg = new CPoint(0, 0);
            FileStream fs = null;
            try
            {
                fs = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true);
            }
            catch (Exception)
            {
                return;
            }

            int a = 0;
            UInt32 b = 0;
            BinaryReader br = new BinaryReader(fs);
            ushort bfType = br.ReadUInt16();
            uint bfSize = br.ReadUInt32();
            br.ReadUInt16();
            br.ReadUInt16();
            uint bfOffBits = br.ReadUInt32();
            if (bfType != 0x4D42)
                return;
            uint biSize = br.ReadUInt32();
            width = br.ReadInt32();
            height = br.ReadInt32();
            a = br.ReadUInt16();
            nByte = br.ReadUInt16() / 8;
            b = br.ReadUInt32();
            b = br.ReadUInt32();
            a = br.ReadInt32();
            a = br.ReadInt32();
            b = br.ReadUInt32();
            b = br.ReadUInt32();
            m_aBuf = new byte[width * height];

            fixed (byte* pp = &m_aBuf[0])
            {
                byte[] hRGB = br.ReadBytes(256 * 4);
                for (int y = 0; y < height; y++)
                {
                    byte[] pBuf = br.ReadBytes(width);
                    Buffer.BlockCopy(pBuf, 0, m_aBuf, (int)((height - y - 1) * width), (int)width);
                    p_ProgessPercentage = Convert.ToInt32(((double)y / height * 50));
                }
                ptr = new IntPtr(pp);
            }
            br.Close();
        }

        byte[] m_aBuf;

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        static extern IntPtr memcpy(IntPtr dst, IntPtr src, UIntPtr count);

        void Worker_MemoryCopy_DoWork(object sender, DoWorkEventArgs e)
        {
            List<object> arguments = (List<object>)(e.Argument);

            string sPath = arguments[0].ToString();

            IntPtr ptr = new IntPtr();
            int imgWidth = 0;
            int imgHeight = 0;

            FileOpenBMP(sPath, ref ptr, ref imgWidth, ref imgHeight);
            //Image image = Image.FromFile(sPath);
            //Bitmap ggg = new Bitmap(sPath);
            //Rectangle rect = new Rectangle(0, 0, ggg.Width, ggg.Height);
            //Bitmap gray = ggg.Clone(rect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            //BitmapData data = gray.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

             string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
             string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
            string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

            //IntPtr ptrMem = m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).GetIntPtr(0);
            //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
            //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
         
            unsafe
            {
                int lowwidth = 0, lowheight = 0;
                //lowwidth = gray.Width < m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X ? gray.Width : m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X;
                //lowheight = gray.Height < m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y ? gray.Height : m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y;
                lowwidth = imgWidth < m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X ? imgWidth : m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
                lowheight = imgHeight < m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y ? imgHeight : m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;
                UIntPtr wwww = new UIntPtr(Convert.ToUInt32(lowwidth));
                for (int i = 0; i < lowheight; i++)
                {

                    Emgu.CV.Util.CvToolbox.Memcpy(ptrMem + i * m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X, ptr + i * imgWidth, lowwidth);
                    //Emgu.CV.Util.CvToolbox.Memcpy( ptrMem + i *lowwidth, data.Scan0 + i * gray.Width  ,lowwidth);
                    //memcpy(ptrMem, ptr + i * imgWidth, wwww);
                    //memcpy((byte*)(ptrMem + i * m_MemoryModule.m_aMemory[p_SelectedIndex].p_szImage.X), (byte*)(data.Scan0 + i * gray.Width), lowwidth);
                    p_ProgessPercentage = Convert.ToInt32(50 + ((double)i / lowheight * 50));
                }

            }
            m_aBuf = new byte[1];
        }


        void Worker_MemoryCopy_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MemoryOpen();
        }

        void Worker_MemorySave_DoWork(object sender, DoWorkEventArgs e)
        {
            List<object> arguments = (List<object>)(e.Argument);

            string sPath = arguments[0].ToString();

            string sPool = m_PoolData[m_PoolSelectedIndex].p_id;
            string sGroup = m_GroupData[m_GroupSelectedIndex].p_id;
            string sMem = m_MemoryData[m_MemorySelectedIndex].p_id;

            //IntPtr ptrMem = m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).GetIntPtr(0);
            //int imgWidth = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.X);
            //int imgHeight = (m_MemoryModule.GetMemoryPool(sPool).GetMemoryData(sGroup, sMem).p_szImage.Y);

            IntPtr ptrMem = m_MemoryModule.GetMemory(sPool, sGroup, sMem).GetPtr(0);
            int imgWidth = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.X;
            int imgHeight = m_MemoryModule.GetMemory(sPool, sGroup, sMem).p_sz.Y;

            Image<Gray, byte> img = new Image<Gray, byte>(imgWidth, imgHeight, imgWidth, (ptrMem));
            FileSaveBMP(sPath, ptrMem, new Rectangle(0, 0, imgWidth, imgHeight));
        }

        public ImageSource ToImageSource(byte[] iconBytes)
        {
            if (iconBytes == null)
                System.Windows.Forms.MessageBox.Show("ToImageSource Error");
            using (var ms = new MemoryStream(iconBytes))
            {
                return System.Windows.Media.Imaging.BitmapFrame.Create(ms);
            }
        }
    }

    public class RectItem : ObservableObject
    {
        public RectItem(int x, int y, int width, int height, System.Windows.Media.Brush color)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            LineColor = color;
        }

        int _X = 0;
        int _Y = 0;
        int _Width = 0;
        int _Height = 0;

        public int X
        {
            get
            {
                return _X;
            }
            set
            {
                SetProperty(ref _X, value);
            }
        }
        public int Y
        {
            get
            {
                return _Y;
            }
            set
            {
                SetProperty(ref _Y, value);
            }
        }
        public int Width
        {
            get
            {
                return _Width;
            }
            set
            {
                SetProperty(ref _Width, value);
            }
        }
        public int Height
        {
            get
            {
                return _Height;
            }
            set
            {
                SetProperty(ref _Height, value);
            }
        }
        System.Windows.Media.Brush _LineColor;
        public System.Windows.Media.Brush LineColor
        {
            get
            {
                return _LineColor;
            }
            set
            {
                SetProperty(ref _LineColor, value);
            }
        }
    }

    
}




