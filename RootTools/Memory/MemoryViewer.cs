using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace RootTools.Memory
{
    public class MemoryViewer : NotifyProperty, ITool
    {
        #region Invalidate
        public delegate void dgOnInvalidDraw();
        public event dgOnInvalidDraw OnInvalidDraw;
        public void InvalidDraw()
        {
            if (OnInvalidDraw != null) OnInvalidDraw(); 
        }

        public delegate void dgOnLBD(bool bDown, CPoint cpImg);
        public event dgOnLBD OnLBD;
        public void LBD(bool bDown, CPoint cpImg)
        {
            if (OnLBD != null) OnLBD(bDown, cpImg); 
        }

        public delegate void dgOnMouseMove(CPoint cpImg);
        public event dgOnMouseMove OnMouseMove;
        public void MouseMove(CPoint cpImg)
        {
            if (OnMouseMove != null) OnMouseMove(cpImg);
        }

        DispatcherTimer m_timer = new DispatcherTimer(); 
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(1);
            m_timer.Tick += M_timer_Tick;
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            m_timer.Stop();
            m_gridDraw.Children.Clear();
            if (p_memoryData == null) return;
            if (p_nMemoryIndex < 0) return;
            if (p_nMemoryIndex >= p_memoryData.m_aDraw.Count) return;
            MemoryDraw draw = p_memoryData.m_aDraw[p_nMemoryIndex];
            draw.Draw(m_gridDraw, m_cpOffset, p_fZoom);
        }

        Grid m_gridDraw; 
        public void Draw(Grid grid)
        {
            if (grid != null) m_gridDraw = grid;
            m_timer.Start(); 
        }
        #endregion

        #region Property
        public string p_id { get; set; }

        string _sGroup = ""; 
        public string p_sGroup
        { 
            get { return _sGroup; }
            set
            {
                if (_sGroup == value) return;
                _sGroup = value;
                OnPropertyChanged(); 
            }
        }

        string _sMemory = "";
        public string p_sMemory
        {
            get { return _sMemory; }
            set
            {
                if (_sMemory == value) return;
                _sMemory = value;
                OnPropertyChanged();
            }
        }

        public int p_nProgress
        {
            get
            {
                return m_nProgress;
            }
            set
            {
                if (m_nProgress== value)
                    return;
                m_nProgress = value;
                OnPropertyChanged();
                //SetProperty(ref m_nProgress, value);
            }
        }
        private int m_nProgress = 0;
        #endregion

        #region MemoryData
        public void SetMemory(MemoryData memoryData, int nIndex)
        {
            p_memoryData = memoryData;
            p_nMemoryIndex = nIndex;
        }

        private void P_memoryData_UpdateOpenProgress(int nInt)
        {
            p_nProgress = nInt;
        }

        MemoryData _memoryData = null;
        public MemoryData p_memoryData
        { 
            get { return _memoryData; }
            set
            {
                if (_memoryData == value) return;
                _memoryData = value;
                p_nMemoryIndex = 0; 
                OnPropertyChanged();
                UpdateBitmapSource();
            }
        }

        int _nMemoryIndex = 0; 
        public int p_nMemoryIndex 
        { 
            get { return _nMemoryIndex; }
            set
            {
                if (_nMemoryIndex == value) return;
                _nMemoryIndex = value; 
                OnPropertyChanged();
                UpdateBitmapSource();
                InvalidDraw(); 
            }
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                MemoryViewer_UI ui = new MemoryViewer_UI();
                ui.Init(this); 
                return ui; 
            }
        }
        #endregion

        #region File Open & Save
        public void FileOpen(string sFile)
        {
            if (p_memoryData == null) return;
            p_memoryData.UpdateOpenProgress += P_memoryData_UpdateOpenProgress1;
            p_memoryData.FileOpen(sFile ,p_nMemoryIndex);
            //switch (GetUpperExt(sFile))
            //{
            //    case "BAYER": p_memoryData.p_sInfo = p_memoryData.FileOpenBayer(sFile, p_nMemoryIndex); break;
            //    case "BMP": p_memoryData.p_sInfo = p_memoryData.FileOpenBMP(sFile, p_nMemoryIndex); break;
            //    case "JPG": p_memoryData.p_sInfo = p_memoryData.FileOpenJPG(sFile, p_nMemoryIndex); break; 
            //}
            UpdateBitmapSource(); 
        }

        private void P_memoryData_UpdateOpenProgress1(int nInt)
        {
            p_nProgress = nInt;
        }

        public void FileSave(string sFile, int nByte = 1)
        {
            if (p_memoryData == null) return;
            p_memoryData.UpdateOpenProgress += P_memoryData_UpdateOpenProgress1;
            switch (GetUpperExt(sFile))
            {
                case "BMP": p_memoryData.p_sInfo = p_memoryData.FileSaveBMP(sFile, p_nMemoryIndex, nByte); break;
                case "JPG": p_memoryData.p_sInfo = p_memoryData.FileSaveJPG(sFile, p_nMemoryIndex); break; 
            }
        }

        string GetUpperExt(string sFile)
        {
            string[] sFiles = sFile.Split('.');
            return sFiles[sFiles.Length - 1].ToUpper(); 
        }
        #endregion

        #region ImageSource
        BitmapSource _bitmapSrc; 
        public BitmapSource p_bitmapSrc
        {
            get { return _bitmapSrc; }
            set
            {
                if (_bitmapSrc == value) return;
                _bitmapSrc = value;
                OnPropertyChanged(); 
            }
        }

        CPoint _szWindow = new CPoint();
        public CPoint p_szWindow
        {
            get { return _szWindow; }
            set
            {
                if (_szWindow == value) return;
                _szWindow = value;
                OnPropertyChanged();
                UpdateBitmapSource();
            }
        }

        CPoint m_cpOffset = new CPoint();
        double[] m_aZoom = { 8, 6, 5, 4, 3, 2.5, 2, 1.5, 1.25, 1, 0.8, 0.64, 0.5, 0.4, 0.3, 0.25, 0.2, 0.15, 0.13, 0.1, 0.08, 0.064, 0.05, 0.04, 0.03, 0.025, 0.02, 0.015, 0.01, 0.008, 0.0064, 0.005, 0.004, 0.003, 0.0025 };
        int m_iZoom = 9;
        double _fZoom = 1; 
        public double p_fZoom
        { 
            get { return _fZoom; }
            set
            {
                if (_fZoom == value) return;
                _fZoom = value;
                double dfMin = 1e20; 
                for (int n = 0; n < m_aZoom.Length; n++)
                {
                    double df = Math.Abs(m_aZoom[n] - value);
                    if (dfMin > df)
                    {
                        dfMin = df;
                        m_iZoom = n;
                    }
                }
                OnPropertyChanged();
                OnPropertyChanged("p_sZoom");
            }
        }

        public string p_sZoom
        {
            get { return p_fZoom.ToString("0.0000"); }
            set { }
        }

        public void ZoomIn(CPoint cpWindow)
        {
            if (m_iZoom <= 0) return;
            CPoint cpImage = GetImagePos(cpWindow);
            m_iZoom--;
            p_fZoom = m_aZoom[m_iZoom];
            m_cpOffset.X = cpImage.X - (int)Math.Round(cpWindow.X / p_fZoom);
            m_cpOffset.Y = cpImage.Y - (int)Math.Round(cpWindow.Y / p_fZoom);
            UpdateBitmapSource(); 
        }

        public void ZoomOut(CPoint cpWindow)
        {
            if (p_memoryData == null) return; 
            if (m_iZoom >= (m_aZoom.Length - 1)) return;
            CPoint cpImage = GetImagePos(cpWindow);
            m_iZoom++;
            p_fZoom = m_aZoom[m_iZoom];
            double sx = p_memoryData.p_sz.X * p_fZoom / p_szWindow.X;
            double sy = p_memoryData.p_sz.Y * p_fZoom / p_szWindow.Y;
            if (Math.Max(sx, sy) < 1) p_fZoom = Math.Min(1.0 * p_szWindow.X / p_memoryData.p_sz.X, 1.0 * p_szWindow.Y / p_memoryData.p_sz.Y);
            m_cpOffset.X = cpImage.X - (int)Math.Round(cpWindow.X / p_fZoom);
            m_cpOffset.Y = cpImage.Y - (int)Math.Round(cpWindow.Y / p_fZoom);
            UpdateBitmapSource();
        }

        string _sGV = ""; 
        public string p_sGV
        { 
            get { return _sGV; }
            set
            {
                if (_sGV == value) return;
                _sGV = value;
                OnPropertyChanged(); 
            }
        }

        CPoint _cpWindow = new CPoint(); 
        public CPoint p_cpWindow
        {
            get { return _cpWindow; }
            set
            {
                if (_cpWindow == value) return;
                _cpWindow = value;
                OnPropertyChanged();
                Shift(_cpWindow);
                p_cpImage = GetImagePos(_cpWindow);
                if (m_aBufDisplay == null) return;
                if (p_memoryData == null) return; 
                int nAdd = p_memoryData.p_nByte * (_cpWindow.X + _cpWindow.Y * m_szBitmapSource.X);
                if ((nAdd < 0) || (nAdd >= m_aBufDisplay.Length)) return;
                switch (p_memoryData.p_nByte)
                {
                    case 1: p_sGV = m_aBufDisplay[nAdd].ToString(); break;
                    case 3:
                    case 4:
                        p_sGV = m_aBufDisplay[nAdd + 2].ToString() + ", " + m_aBufDisplay[nAdd + 1].ToString() + ", " + m_aBufDisplay[nAdd].ToString();
                        break;
                }
                if (p_bShiftKeyDown && p_bLBD) MouseMove(p_cpImage); 
            }
        }

        CPoint _cpImage = new CPoint(); 
        public CPoint p_cpImage
        { 
            get { return _cpImage; }
            set
            {
                if (_cpImage == value) return;
                _cpImage = value;
                OnPropertyChanged(); 
            }
        }

        CPoint GetImagePos(CPoint cpWindow)
        {
            int x = m_cpOffset.X + (int)Math.Round(cpWindow.X / p_fZoom);
            int y = m_cpOffset.Y + (int)Math.Round(cpWindow.Y / p_fZoom);
            return new CPoint(x, y); 
        }

        public CPoint GetWindowPos(CPoint cpImage)
        {
            int x = (int)Math.Round((cpImage.X - m_cpOffset.X) * p_fZoom);
            int y = (int)Math.Round((cpImage.Y - m_cpOffset.Y) * p_fZoom);
            return new CPoint(x, y); 
        }

        bool _bLBD = false; 
        public bool p_bLBD
        {
            get { return _bLBD; }
            set
            {
                _bLBD = value;
                LBD(p_bShiftKeyDown && value, GetImagePos(p_cpLBD));
            }
        }

        public bool p_bShiftKeyDown
        {
            get { return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift); }
        }

        CPoint _cpLBDOffset = new CPoint();
        CPoint _cpLBD = new CPoint();
        public CPoint p_cpLBD
        {
            get { return _cpLBD; }
            set
            {
                _cpLBD = value;
                _cpLBDOffset = m_cpOffset; 
            }
        }

        bool m_bOnShift = false; 
        void Shift(CPoint cpWindow)
        {
            if (m_bOnShift == false)
            {
                if (p_bShiftKeyDown) return;
            }
            if (p_bShiftKeyDown) return;
            if (p_bLBD == false) return;
            CPoint dp = cpWindow - p_cpLBD;
            dp /= p_fZoom;
            m_cpOffset = _cpLBDOffset - dp;
            UpdateBitmapSource(); 
        }

        CPoint m_szImage = new CPoint(); 
        CPoint m_szBitmapSource = new CPoint();
        byte[] m_aBufDisplay = null; 
        unsafe void UpdateBitmapSource()
        {
            Parallel.For(0, 4, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (i) =>
            {
                Debug.WriteLine(i.ToString());
            });
            if (p_memoryData == null) return;
            if (p_memoryData.GetPtr(p_nMemoryIndex) == null) return;
            if (p_szWindow.X * p_szWindow.Y == 0) return;
            p_bitmapSrc = null;

            m_szImage = new CPoint((int)(p_memoryData.p_sz.X * p_fZoom), (int)(p_memoryData.p_sz.Y * p_fZoom));

            int nBytePerPixel = p_memoryData.p_nByte * p_memoryData.p_nCount;
            CPoint sz = m_szBitmapSource = new CPoint(Math.Min(p_szWindow.X, m_szImage.X), Math.Min(p_szWindow.Y, m_szImage.Y));
            byte[] aBuf = m_aBufDisplay = new byte[(long)nBytePerPixel * sz.Y * sz.X];
            FixOffset();

            int[] aX = new int[sz.X];
            for (int x = 0; x < sz.X; x++)
            {
                aX[x] = nBytePerPixel * (m_cpOffset.X + (int)Math.Round(x / p_fZoom));
            }

            for (int y = 0, iy = 0; y < sz.Y; y++, iy += nBytePerPixel * sz.X)
            {
                int pY = m_cpOffset.Y + (int)Math.Round(y / p_fZoom);
                byte* pSrc = (byte*)p_memoryData.GetPtr(p_nMemoryIndex, 0, pY).ToPointer();
                switch (nBytePerPixel)
                {
                    case 1:
                        for (int x = 0; x < sz.X; x++)
                        {
                            aBuf[x + iy] = *(pSrc + aX[x]);
                        }   
                        break;
                    case 2:
                        for (int x = 0, ix = 0; x < sz.X; x++, ix += 2)
                        {
                            aBuf[ix + iy] = *(pSrc + aX[x]);
                            aBuf[ix + iy + 1] = *(pSrc + aX[x] + 1);
                        }
                        break;
                    case 3:
                    case 4:
                        for (int x = 0, ix = 0; x < sz.X; x++, ix += 3)
                        {
                            aBuf[ix + iy] = *(pSrc + aX[x]);
                            aBuf[ix + iy + 1] = *(pSrc + aX[x] + 1);
                            aBuf[ix + iy + 2] = *(pSrc + aX[x] + 2);
                        }
                        break;
                }
            }
            PixelFormat pixelFormat;
            int bytePerPixel = p_memoryData.p_nByte * p_memoryData.p_nCount;
            switch (bytePerPixel)
            {
                case 1: pixelFormat = PixelFormats.Gray8;   break;
                case 2: pixelFormat = PixelFormats.Gray16;  break;
                case 3:
                case 4: pixelFormat = PixelFormats.Bgr24;   break;
                default:    return;
            }
            p_bitmapSrc = BitmapSource.Create(sz.X, sz.Y, 96, 96, pixelFormat, null, aBuf, nBytePerPixel * sz.X);
        }

        void FixOffset()
        {
            CPoint sz = (m_szImage - p_szWindow) / p_fZoom;
            if (m_cpOffset.X > sz.X) m_cpOffset.X = sz.X;
            if (m_cpOffset.Y > sz.Y) m_cpOffset.Y = sz.Y;
            if (m_cpOffset.X < 0) m_cpOffset.X = 0;
            if (m_cpOffset.Y < 0) m_cpOffset.Y = 0;
        }
        #endregion

        public MemoryPool m_memoryPool;
        Log m_log; 
        public MemoryViewer(string id, MemoryPool memoryPool, Log log)
        {
            p_id = id; 
            m_memoryPool = memoryPool;
            m_log = log;
            InitTimer(); 
        }

        

        public void ThreadStop()
        {
        }
    }
}
