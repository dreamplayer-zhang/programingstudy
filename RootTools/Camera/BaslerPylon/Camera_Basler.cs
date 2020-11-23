﻿using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System;
using System.Windows;
using Basler.Pylon;
using System.Runtime.InteropServices;
using RootTools.Memory;
using RootTools.Trees;
using System.Diagnostics;
using System.Windows.Threading;

namespace RootTools.Camera.BaslerPylon
{
    public class Camera_Basler : ObservableObject, RootTools.Camera.ICamera
    {
        public Dispatcher _dispatcher;

        public event System.EventHandler Grabed;

        #region Property
        public string p_id { get; set; }
        public int p_nGrabProgress
        {
            get { return 0; }
            set { }
        }

        string _sInfo = "";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                if (value == "OK") return;
                //AddCommLog(Brushes.Red, value);
                m_log.Warn(value);
            }
        }

        public CPoint p_szROI
        {
            get { return new CPoint(1024, 1024); }
        }



        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                Camera_Basler_UI ui = new Camera_Basler_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        Log m_log;
        Basler.Pylon.Camera m_cam;
        int m_nGrabTimeout = 2000;
        ImageData m_ImageGrab;
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

        BaslerCamInfo m_Caminfo;
        BaslerParameterSet m_CamParam = new BaslerParameterSet();
        public BaslerParameterSet p_CamParam
        {
            get
            {
                return m_CamParam;
            }
            set
            {
                SetProperty(ref m_CamParam, value);
            }
        }
        public BaslerCamInfo p_CamInfo
        {
            get
            {
                return m_Caminfo;
            }
            set
            {
                SetProperty(ref m_Caminfo, value);
            }
        }
        BackgroundWorker bgw_Connect = new BackgroundWorker();
        BackgroundWorker bgw_Grab = new BackgroundWorker();

        public Camera_Basler(string id, Log log)
        {
            p_id = id;
            m_log = log;
            p_CamInfo = new BaslerCamInfo(m_log);
            p_treeRoot = new TreeRoot(id, m_log);
            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
            p_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            bgw_Connect.DoWork += bgw_Connect_DoWork;
            bgw_Connect.RunWorkerCompleted += bgw_Connect_RunWorkerCompleted;
            m_ImageGrab = new ImageData(1000, 700);
            p_ImageViewer = new ImageViewer_ViewModel(m_ImageGrab, null, _dispatcher);
        }


        #region Tree
        private void M_treeRoot_UpdateTree()
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
        void RunTree(Tree treeRoot)
        {
            RunSetTree(treeRoot.GetTree("Connect Set"));
            RunAnalogControlTree(treeRoot.GetTree("Analog Control", false, p_CamInfo._OpenStatus));
            RunAOIControlsTree(treeRoot.GetTree("AOI Contorls", false, p_CamInfo._OpenStatus));
            RunImageFormatControlsTree(treeRoot.GetTree("Image Format Controls", false,p_CamInfo._OpenStatus));
            RunAcquisitionControlsTree(treeRoot.GetTree("Acquisition Controls", false, p_CamInfo._OpenStatus));
            RunDeviceInfomationTree(treeRoot.GetTree("Device Infomation", false, p_CamInfo._OpenStatus));
            RunConfigurationSetTree(treeRoot.GetTree("Configuration Set", false, p_CamInfo._OpenStatus));
            RunHostTransportLayerTree(treeRoot.GetTree("HostTransportLayer", false, p_CamInfo._OpenStatus));
        }

        void RunConfigurationSetTree(Tree tree)
        {
            if (p_CamInfo._OpenStatus)
            {
                p_CamParam._UserSetSelector = tree.Set(p_CamParam._UserSetSelector, "User Set 1", p_CamParam._UserSetSelectorEnum, "UserSetSelect", "Load Save 할 UserSet 선택");
                UserSetLoadCommand = tree.SetButton(UserSetLoadCommand, "Load", "Load Config", "저장해 놓은 Userset(Camera Parameter들)을 Load한다.");
                UserSetSaveCommand = tree.SetButton(UserSetSaveCommand, "Save", "Save Config", "Userset(Camera Parameter들)을 Save한다.");
                p_CamParam._UserSetDefault = tree.Set(p_CamParam._UserSetDefault, "User Set 1", p_CamParam._UserSetDefaultEnum, "Default Startup Set", "처음 카메라 연결시 Load 할 UserSet");
            }
            else
            {
                tree.HideAllItem();
            }
        }

        void RunAnalogControlTree(Tree tree)
        {
            if (p_CamInfo._OpenStatus)
            {
                p_CamParam._GainAuto = tree.Set(p_CamParam._GainAuto, "Off", p_CamParam._GainAutoEnum, "Gain Auto", "Auto Gain Use");
                p_CamParam._GainRaw = tree.Set(p_CamParam._GainRaw, 0, "Gain(Raw)", "Gain: 카메라 센서의 신호 증폭\n 어두운 상황에서 센서 신호를 올려 밝은 영상을 획득하지만 노이즈도 증폭되어 이미지 품질도 저하된다.");
                p_CamParam._GammaEnable = tree.Set(p_CamParam._GammaEnable, false, "GammaEnable", "Gamma Correction 사용 유무");
                //p_CamParam._Gamma = tree.Set(p_CamParam._Gamma, 1.0, "Gamma", "Gamma : intensity(밝기)를 비선형적으로 변형하는 역활,");
            }
            else
            {
                tree.HideAllItem();
            }
        }
        void RunImageFormatControlsTree(Tree tree)
        {
            if(m_Caminfo._OpenStatus)
            {
                p_CamParam.p_PixelFormat = tree.Set(p_CamParam.p_PixelFormat, "Mono 8",p_CamParam._PixelFormatEnum , "Pixel Format", "The Format of the pixel data transmitted for acquired images");
                p_CamParam._ReverseX = tree.Set(p_CamParam._ReverseX, false, "Reverse X", "The Horizontal flipping of the image");
            }
            else
            {
                tree.HideAllItem();
            }
        }
        void RunAOIControlsTree(Tree tree)
        {
            if (p_CamInfo._OpenStatus)
            {
                p_CamParam._Width = tree.Set(p_CamParam._Width, 640, "Width", "Camera Width");
                p_CamParam._Height = tree.Set(p_CamParam._Height, 480, "Height", "Camera Height");
                p_CamParam._XOffset = tree.Set(p_CamParam._XOffset, 0, "X Offset", "Camera Width Offset");
                p_CamParam._YOffset = tree.Set(p_CamParam._YOffset, 0, "Y Offset", "Camera Height Offset");
                p_CamParam._ReverseX = tree.Set(p_CamParam._ReverseX, false, "Reverse X", "Camera image 좌우 반전");
            }
            else
            {
                tree.HideAllItem();
            }
        }
        void RunAcquisitionControlsTree(Tree tree)
        {
            if(p_CamInfo._OpenStatus)
            {
                p_CamParam._ExposureTimeRaw = tree.Set(p_CamParam._ExposureTimeRaw, 0, "Exposure Time (Raw)","The 'Raw' Exposure Time");
                p_CamParam._ResultingFrameRateAbs = tree.Set(p_CamParam._ResultingFrameRateAbs, 0, "Resulting Frame Rate (Abs) [Hz]", "The Maximum Allowed Frame Acquisition Rate",true,true);
            }
            else
            {
                tree.HideAllItem();
            }
        }
        void RunDeviceInfomationTree(Tree tree)
        {
            if (p_CamInfo._OpenStatus)
            {
                p_CamParam._ModelName = tree.Set(p_CamParam._ModelName, "", "Model Name", "Model Name", true, true);
                p_CamParam._DeviceID = tree.Set(p_CamParam._DeviceID, "", "DeviceID", "DeviceID", true, true);
                p_CamParam._DeviceUserID = tree.Set(p_CamParam._DeviceUserID, "", "DeviceUserID", "DeviceUserID", true, true);
                p_CamParam._DeviceScanType = tree.Set(p_CamParam._DeviceScanType, "", "DeviceScanType", "DeviceScanType", true, true);
                p_CamParam._SensorWidth = tree.Set(p_CamParam._SensorWidth, 0, "SensorWidth", "SensorWidth", true, true);
                p_CamParam._SensorHeight = tree.Set(p_CamParam._SensorHeight, 0, "SensorHeight", "SensorHeight", true, true);
            }
            else
            {
                tree.HideAllItem();
            }
        }

        void RunHostTransportLayerTree(Tree tree)
        {
            if (p_CamInfo._OpenStatus)
            {
                p_CamParam._HeartbeatTimeout = tree.Set(p_CamParam._HeartbeatTimeout, 1000, "Heartbeat", "HeartbeatTimeout (단위: ms)  - 컴퓨터와 카메라 간의 연결 상태 확인의 응답 확인 시간 ex) 프로그램 강제 종료시 해당 시간 후 Camera에서 연결을 끊는다.", p_CamInfo._OpenStatus);
            }
            else
            {
                tree.HideAllItem();
            }
        }

        void RunSetTree(Tree tree)
        {
            p_CamInfo._DeviceUserID = tree.Set(p_CamInfo._DeviceUserID, "Basler", "ID", "Device User ID");
            m_nGrabTimeout = tree.Set(m_nGrabTimeout, 2000, "Timeout", "Grab Timeout (ms)");

            //p_eAcquireMode = (eMode)grid.Set(p_id, "AcquireMode", "Acquire Mode", _eAcquireMode); 
            //p_ePixelFormat = (ePixelFormat)grid.Set(p_id, "PixelFormat", "Camera Pixel Format", p_ePixelFormat);
            //p_nBuffer = grid.Set(p_id, "Buffer", "Camera Buffer Count", _nBuffer);
            //grid.Set(p_id, "Offset", "ROI Offset (pixel)", p_cpOffset);
            //grid.Set(p_id, "ROI", "ROI Size (pixel)", p_szROI);
            //p_usExposure = grid.Set(p_id, "Exposure", "Exposure Time (us)", p_usExposure);
            //p_bTrigger = grid.Set(p_id, "Trigger", "Trigger Mode", p_bTrigger);
        }
        #endregion

        public void UpdateCamInfo(ICameraInfo caminfo, Basler.Pylon.Camera cam)
        {
            if (caminfo != null)
            {
                p_CamInfo._IPAddress = caminfo[CameraInfoKey.DeviceIpAddress];
                p_CamInfo._Name = caminfo[CameraInfoKey.ModelName];
                p_CamInfo._ConnectStatus = CameraFinder.GetDeviceAccessibilityInfo(caminfo);
            }
            if (cam == null)
                p_CamInfo._OpenStatus = false;
            else
            {
                p_CamInfo._OpenStatus = cam.IsOpen;
                p_CamInfo._IsCanGrab = cam.IsOpen && !cam.StreamGrabber.IsGrabbing;
                p_CamInfo._IsGrabbing = cam.IsOpen && cam.StreamGrabber.IsGrabbing;
            }
        }

        void bgw_Connect_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (p_CamInfo._DeviceUserID == "") return;
                List<ICameraInfo> allCameras = CameraFinder.Enumerate();    //SEHException 에러 나는경우 Lib/BaslerRuntime 내 파일들을 실행위치로 복사 요망
                ICameraInfo ConnectCamInfo = null;
                foreach (ICameraInfo cameraInfo in allCameras)
                {
                    if (p_CamInfo._DeviceUserID == cameraInfo[CameraInfoKey.UserDefinedName])
                    {
                        ConnectCamInfo = cameraInfo;
                        UpdateCamInfo(ConnectCamInfo, m_cam);
                        break;
                    }
                }
                if (ConnectCamInfo != null)
                {
                    m_cam = new Basler.Pylon.Camera(ConnectCamInfo);
                    m_cam.Open();
                    p_CamInfo.SetCamera(m_cam);
                    p_CamParam = new BaslerParameterSet(m_cam, m_log);
                    p_CamParam._HeartbeatTimeout = 10000;
                    UpdateCamInfo(ConnectCamInfo, m_cam);
                    m_ImageGrab.p_nByte = ((m_CamParam.p_PixelFormat == PLCamera.PixelFormat.Mono8.ToString()) ? 1 : 3);
                    m_ImageGrab.p_Size = new CPoint((int)m_CamParam._Width, (int)m_CamParam._Height);
                    m_cam.Parameters[PLStream.TransmissionType].TrySetValue(PLStream.TransmissionType.Multicast);
                    string strTemp = m_cam.Parameters[PLStream.TransmissionType].GetValue();

                }
                UpdateCamInfo(ConnectCamInfo, m_cam);
            }
            catch (Exception) { }
        }

        void bgw_Connect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunTree(Tree.eMode.Init);
        }

        public void Init()
        {
            Connect();
            
        }

        public void Connect()
        {
            if (!bgw_Connect.IsBusy)
                bgw_Connect.RunWorkerAsync();
        }

        void UpdateValue()
        {
        }

        public void ThreadStop()
        {
            if (m_cam != null)
            {
                m_cam.Close();
            }
        }

        public CPoint GetRoiSize()
        {
            return new CPoint(Convert.ToInt32(m_CamParam._Width), Convert.ToInt32(m_CamParam._Height));
        }

        public double GetFps()
        {
            if (m_cam != null && m_cam.IsOpen)
            {
                double d = m_cam.Parameters[PLCamera.ResultingFrameRateAbs].GetValue();
                return d;
            }
            return 1;
        }

        public CPoint p_sz
        {
            get { return new CPoint(Convert.ToInt32(m_CamParam._Width), Convert.ToInt32(m_CamParam._Height)); }
            set { }
        }

        public string Grab()
        {  
            if (!m_cam.IsOpen)
                return p_id + " Camera not Connected";
            try
            {

                m_cam.StreamGrabber.Start();
                IGrabResult result = m_cam.StreamGrabber.RetrieveResult(m_nGrabTimeout, TimeoutHandling.ThrowException);
                using (result)
                {
                    if (result.GrabSucceeded == false)
                        return p_id + " Grab Error : " + result.ErrorDescription;

                    if (m_CamParam.p_PixelFormat == PLCamera.PixelFormat.Mono8.ToString())
                    {
                        m_ImageGrab.p_nByte = 1;
                        m_ImageGrab.p_Size = new CPoint((int)m_CamParam._Width, (int)m_CamParam._Height);
                        byte[] aBuf = result.PixelData as byte[];
                        Marshal.Copy(aBuf, 0, m_ImageGrab.GetPtr(), m_ImageGrab.p_Size.X * m_ImageGrab.p_Size.Y * m_ImageGrab.p_nByte);
                    }
                    else
                    {
                        PixelDataConverter converter = new PixelDataConverter();
                        converter.OutputPixelFormat = PixelType.BGR8packed;
                        converter.Convert(m_ImageGrab.GetPtr(), m_ImageGrab.p_Size.X * m_ImageGrab.p_Size.Y * m_ImageGrab.p_nByte, result);
                    }
                    //p_ImageViewer.SetImageSource();
                    //Array.Copy(aBuf, m_ImageGrab.m_aBuf, m_ImageGrab.p_sz.X * m_ImageGrab.p_sz.Y);
                }
                return "OK";
            }
            finally
            {
                m_cam.StreamGrabber.Stop();
            }
        }


        public RelayCommand UserSetSaveCommand
        {
            get
            {
                return new RelayCommand(FunctionUserSetSave);
            }
            set
            {
            }
        }
        public RelayCommand UserSetLoadCommand
        {
            get
            {
                return new RelayCommand(FunctionUserSetLoad);
            }
            set
            {
            }
        }
        public RelayCommand ConnectCommand
        {
            get
            {
                return new RelayCommand(FunctionConnect);
            }
            set
            {
            }
        }
        public RelayCommand GrabCommand
        {
            get
            {
                return new RelayCommand(GrabOneShot);
            }
            set
            {
            }
        }
        public RelayCommand ContinousGrabCommand
        {
            get
            {
                return new RelayCommand(GrabContinuousShot);
            }
            set
            {
            }
        }
        public RelayCommand StopGrabCommand
        {
            get
            {
                return new RelayCommand(GrabStop);
            }
            set
            {
            }
        }

        public void GrabOneShot()
        {
            try
            {
                if (m_cam.IsOpen)
                {
                    stopWatch.Reset();
                    m_cam.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                }
                // Starts the grabbing of one image.
                m_cam.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                m_cam.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
            catch (Exception) { }
        }


        // Starts the continuous grabbing of images and handles exceptions.
        public void GrabContinuousShot()
        {
            try
            {
                if (m_cam.IsOpen)
                {
                    m_bLive = true;
                    stopWatch.Reset();
                    m_cam.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                    // Start the grabbing of images until grabbing is stopped.
                    m_cam.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                    string s_curPixelFormat = m_cam.Parameters[PLCamera.PixelFormat].GetValue();
                    int width = (int)m_cam.Parameters[PLCamera.Width].GetValue();
                    int height = (int)m_cam.Parameters[PLCamera.Height].GetValue();
                    CPoint sz = new CPoint(width, height);

                    if (s_curPixelFormat.Equals(PLCamera.PixelFormat.Mono8.ToString()))
                        m_ImageGrab.ReAllocate(sz, 1);
                    else if (s_curPixelFormat.Equals(PLCamera.PixelFormat.YUV422Packed.ToString()))
                        m_ImageGrab.ReAllocate(sz, 3);

                    if (_dispatcher != null)
                    {
                        _dispatcher.Invoke(new Action(delegate ()
                        {
                            p_ImageViewer.SetRoiRect();
                        }));
                    }
                    //RunTree를 하여, Enable해야할 항목 Update

                    m_cam.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                    p_CamInfo._IsCanGrab = false;
                    p_CamInfo._IsGrabbing = true;

                }
            }
            catch (Exception e) 
            {
                MessageBox.Show(e.Message.ToString());
            }
        }

        private void GrabStop()
        {
            if (m_cam == null) return;
            m_cam.StreamGrabber.Stop();
            m_cam.StreamGrabber.ImageGrabbed -= OnImageGrabbed;
            p_CamInfo._IsCanGrab = true;
            p_CamInfo._IsGrabbing = false;
        }

        private Stopwatch stopWatch = new Stopwatch();
        private bool m_bLive = true;
        private CRect m_LastROI = new CRect(0, 0, 0, 0);
        private void OnImageGrabbed(Object sender, ImageGrabbedEventArgs e)
        {
            try
            {
                IGrabResult grabResult = e.GrabResult;
                // Check if the image can be displayed.
                if (grabResult.IsValid)
                {
                    if (!stopWatch.IsRunning || stopWatch.ElapsedMilliseconds > 100)
                    {  
                        if (m_bLive)
                        {
                            m_ImageGrab.p_nByte = ((m_CamParam.p_PixelFormat == PLCamera.PixelFormat.Mono8.ToString()) ? 1 : 3);
                            m_ImageGrab.p_Size = new CPoint((int)m_CamParam._Width, (int)m_CamParam._Height);

                            if (m_ImageGrab.p_nByte == 3)
                            {
                                PixelDataConverter converter = new PixelDataConverter();
                                converter.OutputPixelFormat = PixelType.BGR8packed;
                                converter.Convert(m_ImageGrab.GetPtr(), m_ImageGrab.p_Size.X * m_ImageGrab.p_Size.Y * m_ImageGrab.p_nByte, grabResult);
                            }
                            else
                            {
                                byte[] aBuf = grabResult.PixelData as byte[];
                                Marshal.Copy(aBuf, 0, m_ImageGrab.GetPtr(), m_ImageGrab.p_Size.X * m_ImageGrab.p_Size.Y);
                            }

                            stopWatch.Reset();

                            if (_dispatcher != null)
                            {
                                _dispatcher.Invoke(new Action(delegate ()
                                {
                                    m_ImageGrab.UpdateImage();
                                }));
                            }
                            else
                            {
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    m_ImageGrab.UpdateImage();
                                });
                            }
                        }
                        else
                        {
                            int nFrameOffsetCntY = m_cpScanOffset.Y + m_nFrameCnt;
                            if (m_nFrameCnt < m_nFrameTotal)
                            {
                                if (m_Memory.p_sz.Y > (nFrameOffsetCntY + 1) * m_ImageGrab.p_Size.Y)
                                {
                                    byte[] aBuf = grabResult.PixelData as byte[];
                                    IntPtr dstPtr = (IntPtr)((long)m_Memory.GetPtr() + m_cpScanOffset.X + (m_ImageGrab.p_Size.Y * (long)m_Memory.W * nFrameOffsetCntY));
                                    for (int n = 0; n < m_ImageGrab.p_Size.Y; n++, dstPtr = (IntPtr)((long)dstPtr + m_Memory.W))
                                    {
                                        Marshal.Copy(aBuf, m_ImageGrab.p_Size.X * n, dstPtr, m_ImageGrab.p_Size.X);
                                    }

                                    m_LastROI.Left = m_cpScanOffset.X;
                                    m_LastROI.Right = m_cpScanOffset.X + m_ImageGrab.p_Size.X;
                                    m_LastROI.Top = m_cpScanOffset.Y;
                                    m_LastROI.Bottom = m_cpScanOffset.Y + m_ImageGrab.p_Size.Y;
                                    m_nFrameCnt++;
                                    GrabEvent();
                                    p_nGrabProgress = Convert.ToInt32((double)m_nFrameCnt * 100 / m_nFrameTotal);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
            finally
            {
                // Dispose the grab result if needed for returning it to the grab loop.
                e.DisposeGrabResultIfClone();
            }
        }
        void GrabEvent()
        {
            if (Grabed != null)
                OnGrabed(new GrabedArgs(m_Memory, m_nFrameCnt, m_LastROI));
        }
        protected virtual void OnGrabed(GrabedArgs e)
        {
            if (Grabed != null)
                Grabed.Invoke(this, e);
        }
        public void FunctionConnect()
        {
            if (m_cam != null && m_cam.IsOpen)
            {
                m_cam.Close();
                List<ICameraInfo> allCameras = CameraFinder.Enumerate();
                ICameraInfo ConnectCamInfo = null;
                foreach (ICameraInfo cameraInfo in allCameras)
                {
                    if (p_CamInfo._DeviceUserID == cameraInfo[CameraInfoKey.UserDefinedName])
                    {
                        ConnectCamInfo = cameraInfo;
                        UpdateCamInfo(ConnectCamInfo, m_cam);
                        RunTree(Tree.eMode.Init);
                        break;
                    }
                }
            }
            else
            {
                Connect();
            }
        }
        public void FunctionUserSetLoad()
        {
            if (m_cam != null && m_cam.IsOpen)
            {
                m_cam.Parameters[PLCamera.UserSetLoad].Execute();
                RunTree(Tree.eMode.Init);
            }
        }
        public void FunctionUserSetSave()
        {
            if (m_cam != null && m_cam.IsOpen)
            {
                m_cam.Parameters[PLCamera.UserSetSave].Execute();
                RunTree(Tree.eMode.Init);
            }
        }
        int m_nFrameCnt;
        int m_nFrameTotal;
        MemoryData m_Memory;
        CPoint m_cpScanOffset;
        public void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, bool bInvY = false, int ReverseOffsetY = 0)
        {
            try
            {
                if (m_cam.IsOpen)
                {
                    m_cpScanOffset = cpScanOffset;
                    m_nFrameCnt = 0;
                    m_Memory = memory;
                    m_nFrameTotal = nLine;
                    m_bLive = false;
                    stopWatch.Reset();
                    m_cam.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                    // Start the grabbing of images until grabbing is stopped.
                    m_cam.Parameters[PLCamera.TriggerSelector].SetValue(PLCamera.TriggerSelector.AcquisitionStart);
                    m_cam.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
                    m_cam.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                    p_CamInfo._IsCanGrab = false;
                    p_CamInfo._IsGrabbing = false;

                }
            }
            catch (Exception e) 
            {
                string strError = e.Message.ToString();
                Console.WriteLine(strError);
            }
        }
        public string StopGrab()
        {
            GrabStop();
            return "OK"; 
        }
    }
}
