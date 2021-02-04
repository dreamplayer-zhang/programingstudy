using RootTools.Memory;
using RootTools.Trees;
using SisoSDKLib;
using SiSoFramegrabber;
using PylonC.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;
using System.Windows;
using System.ComponentModel;

namespace RootTools.Camera.Silicon
{
    public class Camera_Silicon : ObservableObject, ICamera
    {
        public int m_Width;
        public int m_Height;
        public const int StopGrabCheck = 0;
        public const int StartGrabCheck = 1;

        //public delegate void Dele_ProgressBar(int value);
        //public event Dele_ProgressBar ProgressBarUpdate;

        BackgroundWorker bgw_Connect = new BackgroundWorker();

        Log m_log;

        public Framegrabber m_fgSiso;
        PYLON_DEVICE_HANDLE m_hDev;  // Handle for the pylon device
        NODE_HANDLE m_hNode;
        NODEMAP_HANDLE m_hNodeMap;

        public bool m_bDeviceOpenError = false;
        int m_nDeviceIndex;
        public int p_nDeviceIndex
        {
            get
            {
                return m_nDeviceIndex;
            }
            set
            {
                SetProperty(ref m_nDeviceIndex, value);
            }
        }
        string m_sMCF = "";
        public string p_sMCF
        {
            get { return m_sMCF; }
            set
            {
                SetProperty(ref m_sMCF, value);
            }
        }
        public int m_nByte = 1;
        public IntPtr m_pBufGrab = IntPtr.Zero; //여기로 이미지 버퍼 땡겨오는거
        public System.Drawing.Point m_szBuf = new System.Drawing.Point(0, 0);

        public int m_nGrab = -1; //버퍼에서 몇번째 이미지 가져올건지 
        public int m_lGrab = 0; //그랩 몇번할건지?
        public int m_nImgToGrab = 1;
        GCHandle userObjectHandle;

        public bool IsLIVE = false;
        System.Drawing.Point szBuf = new System.Drawing.Point();
        //Thread m_thread;

        ImageData m_ImageLive;
        ImageViewer_ViewModel m_ImageViewer;
        public ImageViewer_ViewModel p_ImageViewer
        {
            get
            {
                return m_ImageViewer;
            }
            set
            {
                SetProperty(ref m_ImageViewer, value);
            }
        }

        public string p_id { get; set; }
        int m_nGrabProgress=0;
        public int p_nGrabProgress 
        {
            get
            {
                return m_nGrabProgress;
            }
            set
            {
                SetProperty(ref m_nGrabProgress, value);
            }
        }
        TreeRoot m_treeRoot = null;
        public TreeRoot p_treeRoot 
        {
            get
            {
                return m_treeRoot;
            }
            set
            {
                SetProperty(ref m_treeRoot, value);
            }
        }
        public CPoint p_sz { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #region UI
        public UserControl p_ui
        {
            get
            {
                Camera_Silicon_UI ui = new Camera_Silicon_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        public event EventHandler Grabed;

        // 생성자
        public Camera_Silicon(string id, Log log)
        {
            p_id = id;
            m_log = log;
            p_treeRoot = new TreeRoot(id, m_log);
            bgw_Connect.DoWork += bgw_Connect_DoWork;
            bgw_Connect.RunWorkerCompleted += bgw_Connect_RunWorkerCompleted;
            m_ImageLive = new ImageData(640, 480);
            p_ImageViewer = new ImageViewer_ViewModel(m_ImageLive);
            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
            p_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            m_fgSiso = new Framegrabber();
            m_hDev = new PYLON_DEVICE_HANDLE();   
        }

        public void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.Init);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            p_treeRoot.p_eMode = mode;
            RunTree(p_treeRoot);
        }

        public void RunTree(Tree treeRoot)
        {
            RunIndexTree(treeRoot.GetTree("Camera Index"));
            RunSetTree(treeRoot.GetTree("Connect Set"));
        }

        void RunIndexTree(Tree tree)
        {
            p_nDeviceIndex = tree.Set(p_nDeviceIndex, 0, "Camera Index", "Camera Index");
        }

        void RunSetTree(Tree tree)
        {
            p_sMCF = tree.SetFile(p_sMCF, p_sMCF, "MCF", "CamFile", "CamFile");
        }

        void bgw_Connect_DoWork(object sender, DoWorkEventArgs e)
        {
            ConnectCamera();
        }

        public void Connect()
        {
            if (!bgw_Connect.IsBusy)
                bgw_Connect.RunWorkerAsync();
        }

        void bgw_Connect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunTree(Tree.eMode.Init);
        }

        void ConnectCamera()
        {
            //if (m_hDev != null)
            //{
            //    if (!PylonC.NET.Pylon.DeviceIsOpen(m_hDev)) //disconnect
                {
                    Initialize();
                }
            //    else
            //    {
            //        MessageBox.Show("Device is open");
            //    }
            //}
            
        }

        void DisconnectCamera()
        {
            if (m_hDev != null)
            {
                if (PylonC.NET.Pylon.DeviceIsOpen(m_hDev)) //disconnect
                {
                    PylonC.NET.Pylon.DeviceClose(m_hDev);
                    if (m_pBufGrab != IntPtr.Zero)
                    {
                        m_fgSiso.FgFreeMemEx(m_pBufGrab);
                        m_pBufGrab = IntPtr.Zero;
                    }
                    DeleteGrabberMem();
                    m_fgSiso.FgFreeGrabber();
                }
            }
            else
            {
                MessageBox.Show("카메라 연결을 확인하세요");
            }
        }

        public void Initialize()
        {
            // Basler Camera Connect
            try
            {
                if(m_hDev.IsValid)
                {
                    MessageBox.Show("Device is already connected");
                    return;
                }

                PylonC.NET.Pylon.Initialize();
                uint unNumbDevices = PylonC.NET.Pylon.EnumerateDevices();
                PylonCreateDevice();

                if (m_hDev == null)
                { 
                    MessageBox.Show(p_nDeviceIndex.ToString("DeviceID 0 not Found!!"));
                    return;
                }

                m_bDeviceOpenError = true;
                PylonC.NET.Pylon.DeviceOpen(m_hDev, PylonC.NET.Pylon.cPylonAccessModeControl | PylonC.NET.Pylon.cPylonAccessModeStream);
                m_bDeviceOpenError = false;
                PylonC.NET.Pylon.DeviceFeatureFromString(m_hDev, "UserSetSelector", "UserSet1");
                PylonC.NET.Pylon.DeviceExecuteCommandFeature(m_hDev, "UserSetLoad");

                NODEMAP_HANDLE hNodeMap = PylonC.NET.Pylon.DeviceGetNodeMap(m_hDev);
                NODE_HANDLE hNode = GenApi.NodeMapGetNode(hNodeMap, "Width");   // Handle for a camrera parameter
                szBuf.X = (int)GenApi.IntegerGetValue(hNode);
                hNode = GenApi.NodeMapGetNode(hNodeMap, "Height");
                szBuf.Y = (int)GenApi.IntegerGetValue(hNode);
                m_hNode = hNode;
                m_hNodeMap = hNodeMap;
            }
            catch (Exception)
            {
                MessageBox.Show("Camera Init Fail");
            }

            // Silicon FrameGrabber Connect
            fgErrorType ec = m_fgSiso.FgInitConfig(p_sMCF, (uint)p_nDeviceIndex);
            AllocateGrabberMem(szBuf);
            RegisterAPC();
            SetSisoParam();

            MessageBox.Show("Camera is Connected!");
        }

        void PylonCreateDevice()
        {
            if (p_nDeviceIndex >= 0)
                m_hDev = PylonC.NET.Pylon.CreateDeviceByIndex((uint)p_nDeviceIndex);
            if (p_nDeviceIndex < 0)
                MessageBox.Show("Device Index not Defined !!");
            if (m_hDev == null)
                MessageBox.Show("Device Create Error !!");
        }

        void AllocateGrabberMem(System.Drawing.Point szBuf)
        {
            if (m_szBuf == szBuf)
                return;
            DeleteGrabberMem();
            ulong lSize = (ulong)(szBuf.X * szBuf.Y * m_nByte);
            m_pBufGrab = m_fgSiso.FgAllocMemEx(lSize, m_nByte);
            m_szBuf = szBuf;

            if (m_pBufGrab == IntPtr.Zero)
                MessageBox.Show("Allocate Grabber Mem Error");
        }

        fgErrorType DeleteGrabberMem()
        {
            if (m_pBufGrab != IntPtr.Zero)
            {
                fgErrorType fgError = m_fgSiso.FgFreeMemEx(m_pBufGrab);
                if (fgError != fgErrorType.fgeOK)
                    MessageBox.Show("Delete Grabber Mem Error : " + fgError.ToString());
            }
            m_pBufGrab = IntPtr.Zero;
            m_szBuf.X = m_szBuf.Y = 0;
            return fgErrorType.fgeOK;
        }

        void RegisterAPC()
        {
            FgApcControlFlags flags = FgApcControlFlags.FG_APC_CONTROL_BASIC;
            FgApcControlCtrlFlags ctrlFlags = FgApcControlCtrlFlags.FG_APC_IGNORE_STOP | FgApcControlCtrlFlags.FG_APC_IGNORE_TIMEOUTS;
            fgErrorType rc = m_fgSiso.FgRegisterAPCHandler(this, ApcEventHandler, (uint)p_nDeviceIndex, flags, ctrlFlags, m_pBufGrab);
        }



        public static int ApcEventHandler(object sender, APCEvent ev)//xfercallback
        {
            unsafe
            {
                Framegrabber grabber = sender as Framegrabber;
                FgAPCTransferData data = grabber.APCCallbackPins[2].Target as FgAPCTransferData;
                GCHandle handle = GCHandle.FromIntPtr(data.mReceiverObject);

                Camera_Silicon cam = handle.Target as Camera_Silicon;
                if (cam != null)
                    cam.m_nGrabTrigger++;

                //System.Diagnostics.Debug.WriteLine("m_nTrigger : " + cam.m_nGrabTrigger);
            }
            return 0;
        }

        [DllImport("msvcrt.dll", SetLastError = false)]
        unsafe static extern byte* memcpy(byte* dest, byte* src, int count);

        unsafe void GrabDone(int nGrab, IntPtr pBuf)
        {
            if (IsLIVE) //라이브 모드
            {
                memcpy((byte*)p_ImageViewer.p_ImageData.GetPtr().ToPointer(), (byte*)pBuf.ToPointer(), m_Width * m_Height);
            }
            else //트리거 모드
            {
                memcpy((byte*)m_MemPtr.ToPointer()+(m_Width * nGrab), (byte*)pBuf.ToPointer(), m_Width * m_Height);
                //memcpy(m_DM.Acquired3DImage + (m_szBuf.X * m_szBuf.Y) * nGrab, (byte*)pBuf.ToPointer(), m_szBuf.X * m_szBuf.Y);
            }
        }

        public void SetSisoParam()
        {
            NODEMAP_HANDLE hNodeMap = PylonC.NET.Pylon.DeviceGetNodeMap(m_hDev);   // Get the node map containing all parameters
            NODE_HANDLE hNode = GenApi.NodeMapGetNode(hNodeMap, "Width");   // Handle for a camrera parameter
            m_Width = szBuf.X = (int)GenApi.IntegerGetValue(hNode);
            hNode = GenApi.NodeMapGetNode(hNodeMap, "Height");
            m_Height = szBuf.Y = (int)GenApi.IntegerGetValue(hNode);

            fgErrorType rc = fgErrorType.fgeOK;
            //rc = m_fgSiso.FgSetParameterByName("FG_GEN_WIDTH", m_Width, (uint)p_nDeviceIndex);
            rc = m_fgSiso.FgSetParameterByName("FG_WIDTH", m_Width, (uint)p_nDeviceIndex);
            //rc = m_fgSiso.FgSetParameterByName("FG_GEN_HEIGHT", m_Height, (uint)p_nDeviceIndex);
            rc = m_fgSiso.FgSetParameterByName("FG_HEIGHT", m_Height, (uint)p_nDeviceIndex);
            rc = m_fgSiso.FgSetParameterByName("FG_TRIGGERSTATE", 0/*TS_ACTIVE*/, (uint)p_nDeviceIndex);
        }

        public void StartGrab(long lGrab, bool Trigger = true)
        {
            unsafe
            {
                SetSisoParam();
                AllocateGrabberMem(szBuf);

                if (Trigger == false)
                {
                    IsLIVE = true;
                    PylonC.NET.Pylon.DeviceFeatureFromString(m_hDev, "TriggerMode", "Off");
                }
                else
                {
                    //m_DM.StartGrab[0] = (byte)StartGrabCheck;
                    IsLIVE = false;
                    PylonC.NET.Pylon.DeviceFeatureFromString(m_hDev, "TriggerMode", "On");
                }

                //m_DM.TotalCnt[0] = (int)lGrab;
                m_nGrab = -1;
                m_lGrab = (int)lGrab;

                int nFlag = (int)FgAcquisitionFlags.ACQ_STANDARD;
                fgErrorType rc = m_fgSiso.FgAcquireEx((uint)p_nDeviceIndex, m_lGrab, nFlag, m_pBufGrab);

                if (rc != fgErrorType.fgeOK)
                {
                    rc = m_fgSiso.FgStopAcquireEx((uint)p_nDeviceIndex, m_pBufGrab, 0);
                    Thread.Sleep(100);
                    rc = m_fgSiso.FgAcquireEx((uint)p_nDeviceIndex, m_lGrab, nFlag, m_pBufGrab);
                    if (rc != fgErrorType.fgeOK)
                        MessageBox.Show("Start Grab Fail : " + rc.ToString() + m_fgSiso.GetLastError());
                }
            }
        }

        public double GetFps()
        {
            
            throw new NotImplementedException();
        }

        public CPoint GetRoiSize()
        {
            return new CPoint(m_Width, m_Height);
        }

        MemoryData m_Memory;
        IntPtr m_MemPtr;
        CPoint m_cpScanOffset;
        int m_nFrameCnt;
        int m_nGrabTrigger = 0;
        CRect m_LastROI;
        Thread m_GrabThread;
        public void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData m_GrabData = null)
        {
            //SetSisoParam();
            AllocateGrabberMem(szBuf);

            PylonC.NET.Pylon.DeviceFeatureFromString(m_hDev, "TriggerMode", "On");

            m_cpScanOffset = cpScanOffset;
            m_cpScanOffset.Y = 0;
            m_Memory = memory;
            m_MemPtr = memory.GetPtr();
            m_lGrab = nLine/m_Height;
            m_LastROI = new CRect();
            fgErrorType rc = m_fgSiso.FgAcquireEx((uint)p_nDeviceIndex, m_lGrab, (int)FgAcquisitionFlags.ACQ_STANDARD, m_pBufGrab);

            m_GrabThread = new Thread(new ThreadStart(RunGrabLineScanThread));
           m_GrabThread.Start();
        }
        unsafe void RunGrabLineScanThread()
        {
            int iBlock = 0;
            while(iBlock <= m_lGrab)
            {
                if (iBlock < m_nGrabTrigger)
                {
                    int nY = iBlock * m_Height;

                    bool Scandir = true;
                    IntPtr ipSrc = m_fgSiso.FgGetImagePtrEx(iBlock+1, (uint)p_nDeviceIndex, m_pBufGrab);
                    System.Diagnostics.Debug.WriteLine("mem y : " + nY);
                    /*Parallel.For(0, m_Height, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (y) =>
                    {
                        int yp;
                  //      if (!Scandir)
                   //         yp = lY - (y + (iBlock) * m_Height) + m_nInverseYOffset;
                  //      else
                            yp = y + (iBlock) * m_Height;

                        IntPtr srcPtr = ipSrc + m_Width * y;
                        //memptr[yp+m_scanoffset.y][m_cpScanOffset.x]
                        IntPtr dstPtr = (IntPtr)((long)m_MemPtr + m_cpScanOffset.X + (yp + m_cpScanOffset.Y) * m_Memory.W);
                        Buffer.MemoryCopy((void*)srcPtr, (void*)dstPtr, m_Width, m_Width);                      
                    });*/

                    for (int y = 0; y < m_Height; y++)
                    {
                        int yp = y + (iBlock) * m_Height;
                        
                        IntPtr srcPtr = ipSrc + m_Width * y;
                        IntPtr dstPtr = (IntPtr)((long)m_MemPtr + m_cpScanOffset.X + (yp + m_cpScanOffset.Y) * m_Memory.W);

                        Buffer.MemoryCopy((void*)srcPtr, (void*)dstPtr, m_Width, m_Width);
                    }

                    //m_LastROI.Left = m_cpScanOffset.X;
                    //m_LastROI.Right = m_cpScanOffset.X + m_Width;
                    //m_LastROI.Top = m_cpScanOffset.Y;
                    //m_LastROI.Bottom = m_cpScanOffset.Y + m_Height;

                    iBlock++;

                    GrabEvent();
                }
                //System.Diagnostics.Debug.WriteLine("iBlock = " + iBlock);
            }
        }
        void GrabEvent()
        {
            if (Grabed != null)
                OnGrabed(new GrabedArgs(m_Memory, m_nGrabTrigger, m_LastROI, p_nGrabProgress));
        }
        protected virtual void OnGrabed(GrabedArgs e)
        {
            if (Grabed != null)
                Grabed.Invoke(this, e);
        }
        public string StopGrab()
        {
            unsafe
            {
                fgErrorType rc = m_fgSiso.FgStopAcquireEx((uint)p_nDeviceIndex, m_pBufGrab, 0);
                if (rc != fgErrorType.fgeOK)
                {
                    return "Stop Grab Error : " + rc.ToString();
                }

                m_nGrabTrigger = -1;
                m_lGrab = 0;
                //m_DM.StartGrab[0] = (byte)StopGrabCheck;
                return "OK";
            }
        }

        public void ThreadStop()
        {
        }

        public void GrabLineScanColor(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData m_GrabData = null) { }

        #region RelayCommand
        public RelayCommand ConnectCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    if (!bgw_Connect.IsBusy)
                        bgw_Connect.RunWorkerAsync();
                });
            }
        }
        public RelayCommand DisConnectCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    DisconnectCamera();
                });
            }
        }

        public RelayCommand LiveGrabCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    
                });
            }
        }

        public RelayCommand StopGrabCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    
                });
            }
        }
        #endregion
    }
}
