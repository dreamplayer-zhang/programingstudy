using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Basler.Pylon;
using RootTools.Memory;
using RootTools.Trees;

namespace RootTools.Camera.BaslerPylon
{
    public class CameraBasler : NotifyProperty, RootTools.Camera.ICamera
    {
        #region Property
        public string p_id { get; set; }

        string _sInfo = "";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
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


        public void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, bool bInvY = false, int ReserveOffsetY = 0)
        {
            throw new NotImplementedException();
        }

        public void StopGrabbing()
        {
            throw new NotImplementedException();
        }

        #region Connect
        BackgroundWorker m_bgwConnect = new BackgroundWorker(); 
        void InitConnect()
        {
            m_bgwConnect.DoWork += M_bgwConnect_DoWork;
            m_bgwConnect.RunWorkerCompleted += M_bgwConnect_RunWorkerCompleted;
        }

        private void M_bgwConnect_DoWork(object sender, DoWorkEventArgs e) 
        {
            try
            {
                if (m_cam != null) m_cam.Close();
                m_cam = null; 
                m_sIPAddress = "";
                m_sCamModel = "";
                m_sAccessibility = ""; 
                if (m_sDeviceUserID == "") return;
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
                p_sInfo = "Basler Connect Exception Error : " + ex.Message;
                m_cam = null;
            }
            OnPropertyChanged("p_bConnect"); 
        }

        private void M_bgwConnect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) 
        {
            RunTree(Tree.eMode.Init); 
        }

        public bool p_bConnect
        {
            get { return (m_cam != null); }
            set
            {
                if (p_bConnect == value) return;
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
            p_bConnect = tree.Set(p_bConnect, p_bConnect, "Connect", "Camera Connected");
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

        void RunTreeAOI(Tree tree)
        {
            if (m_cam == null) return;
            if (m_cam.IsOpen == false) return;
            p_sz = tree.Set(p_sz, p_sz, "ROI", "Camera ROI Size");
            p_cpOffset = tree.Set(p_cpOffset, p_cpOffset, "Offset", "Camera ROI Offset");
            p_bReverseX = tree.Set(p_bReverseX, p_bReverseX, "ReverseX", "Camera Image Mirror X"); 
        }
        #endregion

        #region MemoryData
        MemoryTool m_memoryTool; 
        MemoryPool m_memoryPool = null; 
        public string p_sMemoryPool
        {
            get { return (m_memoryPool != null) ? m_memoryPool.p_id : ""; }
            set
            {

            }
        }
        #endregion

        #region Grab


        private void StreamGrabber_ImageGrabbed(object sender, ImageGrabbedEventArgs e)
        {
            throw new NotImplementedException();
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
            try
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    p_treeRoot.p_eMode = mode;
                    bool bOpen = ((m_cam != null) && m_cam.IsOpen);
                    RunTreeConnect(p_treeRoot.GetTree("Connect", false));
                    RunTreeConfigurationSet(p_treeRoot.GetTree("Configuration Set", false, bOpen));
                    RunTreeAOI(p_treeRoot.GetTree("AOI", false, bOpen));
                }); 
            }
            catch (Exception) { }
        }
        #endregion

        public Basler.Pylon.Camera m_cam = null;
        Log m_log; 
        public CameraBasler(string id, Log log)
        {
            p_id = id;
            m_log = log;

            p_treeRoot = new TreeRoot(id, m_log);
            RunTree(Tree.eMode.RegRead);
            p_treeRoot.UpdateTree += P_treeRoot_UpdateTree;

            InitConnect(); 
        }

        public void ThreadStop()
        {
            if (m_cam != null)
            {
                m_cam.Close();
                m_cam = null; 
            }
        }
    }
}
