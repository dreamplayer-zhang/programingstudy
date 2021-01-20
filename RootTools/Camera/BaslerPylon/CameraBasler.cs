using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Controls;
using Basler.Pylon;
using RootTools.Memory;
using RootTools.Trees;

namespace RootTools.Camera.BaslerPylon
{
    public class CameraBasler : NotifyProperty, RootTools.Camera.ICamera
    {
        public event EventHandler Grabed;
        #region Property
        public string p_id { get; set; }

        string _sInfo = "OK";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                OnPropertyChanged(); 
                if (value == "OK") return;
                m_log.Warn(value);
            }
        }

        public UserControl p_ui
        {
            get
            {
                CameraBasler_UI ui = new CameraBasler_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        #region Connect
        public delegate void dgOnConnect();
        public event dgOnConnect OnConnect;

        BackgroundWorker m_bgwConnect = new BackgroundWorker(); 
        void InitConnect()
        {
            m_bgwConnect.DoWork += M_bgwConnect_DoWork;
            m_bgwConnect.RunWorkerCompleted += M_bgwConnect_RunWorkerCompleted;
            p_bConnect = true; 
        }

        private void M_bgwConnect_DoWork(object sender, DoWorkEventArgs e) 
        {
            p_sInfo = Connect(); 
            OnPropertyChanged("p_bConnect"); 
        }

        string Connect()
        {
            try
            {
                if (m_cam != null) m_cam.Close();
                m_cam = null;
                m_sIPAddress = "";
                m_sCamModel = "";
                m_sAccessibility = "";
                if (m_sDeviceUserID == "") return "Device User ID not Defined";
                List<ICameraInfo> aCamera = CameraFinder.Enumerate(); //SEHException 에러 나는경우 Lib/BaslerRuntime 내 파일들을 실행위치로 복사
                foreach (ICameraInfo cameraInfo in aCamera)
                {
                    if (m_sDeviceUserID == cameraInfo[CameraInfoKey.UserDefinedName])
                    {
                        m_cam = new Basler.Pylon.Camera(cameraInfo);
                        m_cam.Open();
                        m_cam.StreamGrabber.ImageGrabbed += StreamGrabber_ImageGrabbed;
                        m_sIPAddress = cameraInfo[CameraInfoKey.DeviceIpAddress];
                        m_sCamModel = cameraInfo[CameraInfoKey.ModelName];
                        m_sAccessibility = CameraFinder.GetDeviceAccessibilityInfo(cameraInfo).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                m_cam = null;
                return "Basler Connect Exception Error : " + ex.Message;
            }
            return "OK";
        }

        private void M_bgwConnect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) 
        {
            RunTree(Tree.eMode.Init);
            if (OnConnect != null) OnConnect(); 
        }

        public bool p_bConnect
        {
            get { return (m_cam != null); }
            set
            {
                if (p_bConnect == value) return;
                OnPropertyChanged(); 
                if (value)
                {
                    if (m_bgwConnect.IsBusy == false) m_bgwConnect.RunWorkerAsync();
                }
                else
                {
                    if (m_cam != null) m_cam.Close();
                    m_cam = null;
                }
            }
        }

        public long p_HeartbeatTimeout
        {
            get { return (m_cam != null) ? m_cam.Parameters[PLTransportLayer.HeartbeatTimeout].GetValue() : 0; }
            set
            {
                if (m_cam == null) return;
                if (m_cam.Parameters[PLTransportLayer.HeartbeatTimeout].GetValue() == value) return;
                m_cam.Parameters[PLTransportLayer.HeartbeatTimeout].TrySetValue(value, IntegerValueCorrection.Nearest);
                OnPropertyChanged(); 
            }
        }

        string m_sDeviceUserID = "";
        string m_sIPAddress = "";
        string m_sCamModel = "";
        string m_sAccessibility = ""; 
        void RunTreeConnect(Tree tree)
        {
            m_sDeviceUserID = tree.Set(m_sDeviceUserID, m_sDeviceUserID, "DeviceID", "Device User ID");
            tree.Set(m_sIPAddress, m_sIPAddress, "IP Address", "TCP/IP Address", true, true);
            tree.Set(m_sCamModel, m_sCamModel, "Model", "Camera Model", true, true);
            tree.Set(m_sAccessibility, m_sAccessibility, "Accessibility", "Camera Accessibility", true, true);
            p_HeartbeatTimeout = tree.Set(p_HeartbeatTimeout, p_HeartbeatTimeout, "Heatbeat", "Heartbeat Timeout (ms)"); 
        }
        #endregion

        #region ConfigurationSet
        string p_sUserSet
        {
            get { return (m_cam != null) ? m_cam.Parameters[PLCamera.UserSetSelector].GetValue() : ""; }
            set
            {
                if (m_cam == null) return;
                m_cam.Parameters[PLCamera.UserSetSelector].TrySetValue(value); 
            }
        }

        string p_sUserSetDefault
        {
            get { return (m_cam != null) ? m_cam.Parameters[PLCamera.UserSetDefaultSelector].GetValue() : ""; }
            set
            {
                if (m_cam == null) return;
                m_cam.Parameters[PLCamera.UserSetDefaultSelector].TrySetValue(value);
            }
        }

        List<string> m_asUserSet = new List<string>();
        List<string> m_asUserSetDefault = new List<string>(); 
        void RunTreeConfigurationSet(Tree tree)
        {
            if (m_cam == null) return;
            if (m_cam.IsOpen == false) return; 
            if (m_asUserSet.Count == 0)
            {
                foreach (string str in m_cam.Parameters[PLCamera.UserSetSelector].GetAllValues()) m_asUserSet.Add(str);
                foreach (string str in m_cam.Parameters[PLCamera.UserSetDefaultSelector].GetAllValues()) m_asUserSetDefault.Add(str);
            }
            p_sUserSetDefault = tree.Set(p_sUserSetDefault, "User Set 1", m_asUserSetDefault, "Default Set", "Default Camera Configration Set");
            p_sUserSet = tree.Set(p_sUserSet, "User Set 1", m_asUserSet, "User Set", "Select Camera Configration Set");
            tree.SetButton(new RelayCommand(LoadConfigrationSet), "Load", "Load", "Load User Configration Set");
            tree.SetButton(new RelayCommand(SaveConfigrationSet), "Save", "Save", "Save User Configration Set");
        }

        void LoadConfigrationSet()
        {
            m_cam.Parameters[PLCamera.UserSetLoad].Execute();
            RunTree(Tree.eMode.Init); 
        }

        void SaveConfigrationSet()
        {
            m_cam.Parameters[PLCamera.UserSetSave].Execute();
            RunTree(Tree.eMode.Init);
        }
        #endregion

        #region AOI
        public int p_nByte
        {
            get
            {
                if (m_cam == null) return 1;
                switch (m_cam.Parameters[PLCamera.PixelSize].GetValue())
                {
                    case "Bpp8": return 1;
                    case "Bpp16": return 2;
                    case "Bpp24": return 3;
                    case "Bpp32": return 4;
                    default: return 1;
                }
            }
        }

        public CPoint p_sz
        {
            get
            {
                if (m_cam == null) return new CPoint(1024, 1024);
                return new CPoint((int)m_cam.Parameters[PLCamera.Width].GetValue(), (int)m_cam.Parameters[PLCamera.Height].GetValue());
            }
            set
            {
                if (m_cam == null) return;
                if ((int)m_cam.Parameters[PLCamera.Width].GetValue() != value.X)
                {
                    m_cam.Parameters[PLCamera.Width].TrySetValue(value.X, IntegerValueCorrection.Nearest);
                    OnPropertyChanged();
                }
                if ((int)m_cam.Parameters[PLCamera.Height].GetValue() != value.Y)
                {
                    m_cam.Parameters[PLCamera.Height].TrySetValue(value.Y, IntegerValueCorrection.Nearest);
                    OnPropertyChanged();
                }
            }
        }

        public CPoint p_cpOffset
        {
            get
            {
                if (m_cam == null) return new CPoint(0, 0);
                return new CPoint((int)m_cam.Parameters[PLCamera.OffsetX].GetValue(), (int)m_cam.Parameters[PLCamera.OffsetY].GetValue());
            }
            set
            {
                if (m_cam == null) return;
                if ((int)m_cam.Parameters[PLCamera.OffsetX].GetValue() != value.X)
                {
                    m_cam.Parameters[PLCamera.OffsetX].TrySetValue(value.X, IntegerValueCorrection.Nearest);
                    OnPropertyChanged();
                }
                if ((int)m_cam.Parameters[PLCamera.OffsetY].GetValue() != value.Y)
                {
                    m_cam.Parameters[PLCamera.OffsetY].TrySetValue(value.Y, IntegerValueCorrection.Nearest);
                    OnPropertyChanged();
                }
            }
        }


        void RunTreeAOI(Tree tree)
        {
            if (m_cam == null) return;
            if (m_cam.IsOpen == false) return;
            tree.Set(p_nByte, p_nByte, "Pixel Byte", "Camera Pixel Depth (Byte)", true, true); 
            p_sz = tree.Set(p_sz, p_sz, "ROI", "Camera ROI Size");
            p_cpOffset = tree.Set(p_cpOffset, p_cpOffset, "Offset", "Camera ROI Offset");
        }
        #endregion

        #region Image Format Controls
        string p_sPixelFormat
        {
            get
            {
                return (m_cam != null) ? m_cam.Parameters[PLCamera.PixelFormat].GetValue() : "";
            }
            set
            {
                if (m_cam == null) return;
                m_cam.Parameters[PLCamera.PixelFormat].TrySetValue(value);
                if (m_cam.Parameters[PLCamera.PixelFormat].GetValue().Equals(PLCamera.PixelFormat.Mono8))
                OnPropertyChanged();
            }
        }
        public bool p_bReverseX
        {
            get { return (m_cam != null) ? m_cam.Parameters[PLCamera.ReverseX].GetValue() : false; }
            set
            {
                if (m_cam == null) return;
                if (m_cam.Parameters[PLCamera.ReverseX].GetValue() == value) return;
                m_cam.Parameters[PLCamera.ReverseX].TrySetValue(value);
                OnPropertyChanged();
            }
        }
        void RunTreeImageFormatControls(Tree tree)
        {
            if (m_cam == null) return;
            if (m_cam.IsOpen == false) return;
            p_sPixelFormat = tree.Set(p_sPixelFormat, p_sPixelFormat, "Pixel Format", "Sets the Format of the pixel data trasmitted for acquired images");
            p_bReverseX = tree.Set(p_bReverseX, p_bReverseX, "ReverseX", "Enableds the horizontal flipping of the image");
        }
        #endregion

        #region Acquisition Controls
        public long _ExposureTimeRaw
        {
            get
            {
                return (m_cam != null) ? m_cam.Parameters[PLCamera.ExposureTimeRaw].GetValue() : 0;
            }
            set
        {
            if (m_cam == null) return;
                if (m_cam.Parameters[PLCamera.ExposureTimeRaw].GetValue() == value) return;

                if(m_cam.Parameters[PLCamera.PixelFormat].GetValue().Equals(PLCamera.PixelFormat.Mono8))
                    m_cam.Parameters[PLCamera.ExposureTimeRaw].TrySetValue(value);
                else
                {
                    //color인 경우 frame rate를 30 이상 사용하지 않도록 제한
                    if(value >= 33300.000000)
                        m_cam.Parameters[PLCamera.ExposureTimeRaw].TrySetValue(value);
                }
                OnPropertyChanged();
            }
        }
        #endregion

        #region Analog Control
        public bool p_bGainAuto
        { 
            get { return (m_cam != null) ? (m_cam.Parameters[PLCamera.GainAuto].GetValue() != "Off") : false; }
            set
            {
                if (m_cam == null) return;
                m_cam.Parameters[PLCamera.GainAuto].TrySetValue(value ? "On" : "Off");
            }
        }

        public long p_nGainRaw
        { 
            get { return (m_cam != null) ? m_cam.Parameters[PLCamera.GainRaw].GetValue() : 0; }
            set
            {
                if (m_cam == null) return;
                m_cam.Parameters[PLCamera.GainRaw].TrySetValue(value, IntegerValueCorrection.Nearest);
            }
        }

        public bool p_bGammaEnable
        { 
            get { return (m_cam != null) ? m_cam.Parameters[PLCamera.GammaEnable].GetValue() : false; }
            set
            {
                if (m_cam == null) return;
                m_cam.Parameters[PLCamera.GammaEnable].TrySetValue(value);
            }
        }

        public double p_fGamma
        { 
            get { return (m_cam != null) ? m_cam.Parameters[PLCamera.Gamma].GetValue() : 0; }
            set
            {
                if (m_cam == null) return;
                m_cam.Parameters[PLCamera.Gamma].TrySetValue(value, FloatValueCorrection.ClipToRange);
            }
        }

        void RunTreeAnalog(Tree tree)
        {
            p_bGainAuto = tree.Set(p_bGainAuto, p_bGainAuto, "Auto Gain", "Enable Auto Gain");
            p_nGainRaw = tree.Set(p_nGainRaw, p_nGainRaw, "Gain", "Set Gain Value", p_bGainAuto == false);
            p_bGammaEnable = tree.Set(p_bGammaEnable, p_bGammaEnable, "Gamma Enable", "Enable Gamma Correction");
            p_fGamma = tree.Set(p_fGamma, p_fGamma, "Gamma", "Set Gamma Value", p_bGammaEnable); 
        }
        #endregion

        #region MemoryData
        public MemoryTool m_memoryTool; 
        public MemoryPool m_memoryPool = null; 
        public string p_sMemoryPool
        {
            get { return (m_memoryPool != null) ? m_memoryPool.p_id : ""; }
            set 
            { 
                m_memoryPool = m_memoryTool.GetPool(value);
                OnPropertyChanged();
                if (m_memoryPool != null)
                {
                    foreach (string sGroup in m_memoryPool.m_asGroup)
                    {
                        if (p_sMemoryGroup == sGroup) return;
                    }
                }
                p_sMemoryGroup = "";
            }
        }

        public MemoryGroup m_memoryGroup = null;
        public string p_sMemoryGroup
        {
            get { return (m_memoryGroup != null) ? m_memoryGroup.p_id : ""; }
            set
            {
                if (m_memoryPool == null) return; 
                m_memoryGroup = m_memoryPool.GetGroup(value);
                OnPropertyChanged();
                if (m_memoryGroup != null)
                {
                    foreach (string sMemory in m_memoryGroup.m_asMemory)
                    {
                        if (p_sMemoryData == sMemory) return; 
                    }
                }
                p_sMemoryData = "";
            }
        }

        public MemoryData m_memoryData = null;
        public string p_sMemoryData
        {
            get { return (m_memoryData != null) ? m_memoryData.p_id : ""; }
            set 
            {
                if (m_memoryGroup == null) return; 
                m_memoryData = m_memoryGroup.GetMemory(value);
                OnPropertyChanged();
            }
        }

        public void SetMemoryData(MemoryData memoryData)
        {
            if (memoryData == null)
            {
                p_sMemoryData = "";
                return;
            }
            p_sMemoryPool = memoryData.m_group.m_pool.p_id;
            p_sMemoryGroup = memoryData.m_group.p_id;
            p_sMemoryData = memoryData.p_id;
            RunTree(Tree.eMode.Init); 
        }

        void RunTreeMemory(Tree tree)
        {
            p_sMemoryPool = tree.Set(p_sMemoryPool, p_sMemoryPool, m_memoryTool.m_asPool, "Pool", "Memory Pool ID");
            if (m_memoryPool == null) return;
            p_sMemoryGroup = tree.Set(p_sMemoryGroup, p_sMemoryGroup, m_memoryPool.m_asGroup, "Group", "Memory Group ID");
            if (m_memoryGroup == null) return;
            p_sMemoryData = tree.Set(p_sMemoryData, p_sMemoryData, m_memoryGroup.m_asMemory, "Memory", "Memory Data ID"); 
        }
        #endregion

        #region Grab
        bool m_bLive = false; 
        public string StartGrab(int nGrab, int nOffset = 0)
        {
            if (m_cam == null) return "Camera not Connected";
            if (m_cam.IsOpen == false) return "Camera not Opened";
            if (m_memoryData == null) return "MemoryData not Assigned";
            if (m_cam.StreamGrabber.IsGrabbing) return "Camera is OnGrabbing";
            m_qGrab.Clear();
            m_bLive = false;
            p_nGrabProgress = 0;
            if (nGrab <= 0) nGrab = 1;
            if ((nGrab + nOffset) > m_memoryData.p_nCount) return "Grab Count Larger then Memory Count";
            m_nGrabCount = nGrab; 
            m_nGrabOffset = nOffset; 
            string sMode = (nGrab <= 1) ? PLCamera.AcquisitionMode.SingleFrame : PLCamera.AcquisitionMode.Continuous; 
            m_cam.Parameters[PLCamera.AcquisitionMode].SetValue(sMode);
            m_cam.StreamGrabber.Start(nGrab, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            return "OK"; 
        }

        public string StartLive(int nOffset)
        {
            if (m_cam == null) return "Camera not Connected";
            if (m_cam.IsOpen == false) return "Camera not Opened";
            if (m_memoryData == null) return "MemoryData not Assigned";
            if (m_cam.StreamGrabber.IsGrabbing) return "Camera is OnGrabbing";
            m_bLive = true;
            if (nOffset > m_memoryData.p_nCount) return "Grab Offset Larger then Memory Count";
            m_nGrabOffset = nOffset;
            m_cam.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
            m_cam.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            return "OK";
        }

        public string StopGrab()
        {
            if (m_cam == null) return "Camera not Connected";
            if (m_cam.IsOpen == false) return "Camera not Opened";
            m_cam.StreamGrabber.Stop();
            m_qGrab.Clear();
            return "OK";
        }

        Queue<byte[]> m_qGrab = new Queue<byte[]>(); 
        private void StreamGrabber_ImageGrabbed(object sender, ImageGrabbedEventArgs e)
        {
            try
            {
                IGrabResult result = e.GrabResult;
                if (result.IsValid == false)
                {
                    p_sInfo = "Grab Invalid : " + e.GrabResult.ImageNumber.ToString();
                    return; 
                }
                m_qGrab.Enqueue(result.PixelData as byte[]);
            }
            catch (Exception ex) { p_sInfo = "Grab Exception : " + ex.Message; }
            finally { e.DisposeGrabResultIfClone(); }
        }

        int m_msGrab = 2000; 
        public string GrabOne(int iMemory)
        {
            if (m_cam == null) return "Camera not Connected";
            if (m_cam.IsOpen == false) return "Camera not Opened";
            if (m_memoryData == null) return "MemoryData not Assigned";
            if (m_cam.StreamGrabber.IsGrabbing) return "Camera is OnGrabbing";
            try
            {
                m_cam.StreamGrabber.Start();
                IGrabResult result = m_cam.StreamGrabber.RetrieveResult(m_msGrab, TimeoutHandling.ThrowException);
                using (result)
                {
                    if (result.GrabSucceeded == false) return p_id + " Grab Error : " + result.ErrorDescription;
                    IntPtr intPtr = m_memoryData.GetPtr(iMemory);
                    CopyBuf(result.PixelData as byte[], intPtr);
                    return "OK";
                }
            }
            finally
            {
                m_cam.StreamGrabber.Stop();
            }
        }

        public int m_nGrabOffset = 0;
        public int m_nGrabCount = 1;
        void RunTreeGrab(Tree tree)
        {
            if (m_cam == null) return;
            if (m_cam.IsOpen == false) return;
            m_nGrabOffset = tree.Set(m_nGrabOffset, m_nGrabOffset, "Offset", "MemoryData Offset"); 
            m_nGrabCount = tree.Set(m_nGrabCount, m_nGrabCount, "Count", "Grab Count for Continuous Grab");
            m_msGrab = tree.Set(m_msGrab, m_msGrab, "Timeout", "Single Grab Timeout (ms)"); 
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_thread; 
        void RunThread()
        {
            m_bThread = true;
            Thread.Sleep(2000);
            while (m_bThread)
            {
                Thread.Sleep(5);
                while (m_qGrab.Count > 0)
                {
                    if (m_bLive) p_sInfo = ThreadLive();
                    else p_sInfo = ThreadGrab(); 
                }
            }
        }

        int _nGrabProgress = 0;
        public int p_nGrabProgress
        {
            get { return _nGrabProgress; }
            set
            {
                if (_nGrabProgress == value) return;
                _nGrabProgress = value;
                OnPropertyChanged();
            }
        }

        string ThreadGrab()
        {
            IntPtr intPtr = m_memoryData.GetPtr(m_nGrabOffset + p_nGrabProgress);
            p_sInfo = CopyBuf(m_qGrab.Dequeue(), intPtr); 
            p_nGrabProgress++; 
            return p_sInfo; 
        }

        string ThreadLive()
        {
            while (m_qGrab.Count > 1) m_qGrab.Dequeue();
            IntPtr intPtr = m_memoryData.GetPtr(m_nGrabOffset);
            return CopyBuf(m_qGrab.Dequeue(), intPtr); 
        }

        string CopyBuf(byte[] aBuf, IntPtr intPtr)
        {
            if (intPtr == null) return "CopyBuf Error : IntPtr == null"; 
            int nSize = p_sz.X * p_sz.Y * p_nByte;
            Marshal.Copy(aBuf, 0, intPtr, nSize);
            //forget MemoryData Invalid View
            return "OK";
        }
        #endregion

        #region Tree
        public TreeRoot p_treeRoot { get; set; }
        private void P_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite); 
        }

        public void RunTree(Tree.eMode mode)
        {
            p_treeRoot.p_eMode = mode;
            bool bOpen = ((m_cam != null) && m_cam.IsOpen);
            RunTreeConnect(p_treeRoot.GetTree("Connect", false));
            RunTreeMemory(p_treeRoot.GetTree("Memory", false));
            RunTreeGrab(p_treeRoot.GetTree("Start Grab", true, bOpen));
            RunTreeConfigurationSet(p_treeRoot.GetTree("Configuration Set", false, bOpen));
            RunTreeImageFormatControls(p_treeRoot.GetTree("Image Format Controls", false, bOpen));
            RunTreeAOI(p_treeRoot.GetTree("AOI", false, bOpen));
            RunTreeAnalog(p_treeRoot.GetTree("Analog", false, bOpen));
        }
        #endregion

        public Basler.Pylon.Camera m_cam = null;
        Log m_log; 
        public CameraBasler(string id, Log log, MemoryTool memoryTool)
        {
            p_id = id;
            m_log = log;
            m_memoryTool = memoryTool; 

            p_treeRoot = new TreeRoot(id, m_log);
            RunTree(Tree.eMode.RegRead);
            p_treeRoot.UpdateTree += P_treeRoot_UpdateTree;

            InitConnect();

            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_qGrab.Clear();
                EQ.p_bStop = true;
                m_thread.Join();
            }
            if (m_cam != null)
            {
                m_cam.Close();
                m_cam = null; 
            }
        }

        public void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, int nScanOffsetY = 0, bool bInvY = false, int ReserveOffsetY = 0) { }
        public void GrabLineScanColor(MemoryData memory, CPoint cpScanOffset, int nLine, int nScanOffsetY = 0, bool bInvY = false, int ReserveOffsetY = 0) { }
        public CPoint GetRoiSize() { return null; }

        public double GetFps()
        {
            throw new NotImplementedException();
        }
    }
}
