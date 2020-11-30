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
        string m_sMCF;
        public string p_sMCF
        {
            get { return m_sMCF; }
            set
            {
                SetProperty(ref m_sMCF, value);
            }
        }


        public int m_lBufGrab = 10;
        public IntPtr m_pBufGrab = IntPtr.Zero;
        public System.Drawing.Point m_szBuf = new System.Drawing.Point(0, 0);

        public int m_nGrab = -1;
        public int m_lGrab = 0;
        public int m_nImgToGrab = 1;

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
        public int p_nGrabProgress { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
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

        void bgw_Connect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunTree(Tree.eMode.Init);
        }

        void ConnectCamera()
        {
            Initialize();
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
            ulong lSize = (ulong)(szBuf.X * szBuf.Y * m_lBufGrab);
            m_pBufGrab = m_fgSiso.FgAllocMemEx(lSize, m_lBufGrab);
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

        unsafe public int ApcEventHandler(object sender, APCEvent ev)
        {
            unsafe
            {
                m_nGrab = (int)ev.imageNo;
                IntPtr pBuf = m_fgSiso.FgGetImagePtrEx(ev.imageNo, (uint)p_nDeviceIndex, m_pBufGrab);
                GrabDone((int)ev.imageNo - 1, pBuf); //forget
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
                // pool->group->mem
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
            rc = m_fgSiso.FgSetParameterByName("FG_GEN_WIDTH", m_Width, (uint)p_nDeviceIndex);
            rc = m_fgSiso.FgSetParameterByName("FG_WIDTH", m_Width, (uint)p_nDeviceIndex);
            rc = m_fgSiso.FgSetParameterByName("FG_GEN_HEIGHT", m_Height, (uint)p_nDeviceIndex);
            rc = m_fgSiso.FgSetParameterByName("FG_HEIGHT", m_Height, (uint)p_nDeviceIndex);
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
            throw new NotImplementedException();
        }

        public void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, bool bInvY = false, int ReserveOffsetY = 0)
        {
            throw new NotImplementedException();
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

                m_nGrab = -1;
                m_lGrab = 0;
                //m_DM.StartGrab[0] = (byte)StopGrabCheck;
                return "OK";
            }
        }

        public void ThreadStop()
        {
        }

        public void GrabLineScanColor(MemoryData memory, CPoint cpScanOffset, int nLine, bool bInvY = false, int ReserveOffsetY = 0) { }

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
