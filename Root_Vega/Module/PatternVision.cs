﻿using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.Structure;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Control.Ajin;
using RootTools.Inspects;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.RADS;
using RootTools.Trees;
using RootTools.ZoomLens;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Root_Vega.Module
{
    public class PatternVision : ModuleBase, IRobotChild
    {
        #region ViewModel
        public _2_5_MainVisionViewModel m_mvvm;
        #endregion

        #region ToolBox
        public DIO_I m_diPatternReticleExistSensor;

        AxisXY m_axisXY;
        public AxisXY p_axisXY
        {
            get
            {
                return m_axisXY;
            }
        }
        Axis m_axisZ;
        public Axis p_axisZ
        {
            get
            {
                return m_axisZ;
            }
        }

        Axis m_axisClamp;
        public Axis p_axisClamp
        {
            get
            {
                return m_axisClamp;
            }
        }

        public Camera_Dalsa m_CamMain;
        public Camera_Basler m_CamVRS;
        public Camera_Basler m_CamAlign1;
        public Camera_Basler m_CamAlign2;
        public Camera_Basler m_CamRADS;

        #region Light
        public LightSet m_lightSet;
        public int GetLightByName(string str)
        {
            for (int i = 0; i < m_lightSet.m_aLight.Count; i++)
            {
                if (m_lightSet.m_aLight[i].m_sName.IndexOf(str) >= 0)
                {
                    return Convert.ToInt32(m_lightSet.m_aLight[i].p_fPower);
                }
            }
            return 0;
        }
        public void SetLightByName(string str, int nValue)
        {
            for (int i = 0; i < m_lightSet.m_aLight.Count; i++)
            {
                if (m_lightSet.m_aLight[i].m_sName.IndexOf(str) >= 0)
                {
                    m_lightSet.m_aLight[i].m_light.p_fSetPower = nValue;
                }
            }
        }
        #endregion

        MemoryPool m_memoryPool;
        MemoryData m_memoryMain;
        InspectTool m_inspectTool;
        ZoomLens m_ZoomLens;
        public RADSControl m_RADSControl;
        public ZoomLens p_ZoomLens;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisXY, this, "StageXY");
            p_sInfo = m_toolBox.Get(ref m_axisZ, this, "StageZ");
            p_sInfo = m_toolBox.Get(ref m_axisClamp, this, "StageClamp");
            p_sInfo = m_toolBox.Get(ref m_CamMain, this, "MainCam");
            if (m_CamMain != null) m_CamMain.Connect();
            p_sInfo = m_toolBox.Get(ref m_CamVRS, this, "VRS");
            p_sInfo = m_toolBox.Get(ref m_CamAlign1, this, "Align1");
            p_sInfo = m_toolBox.Get(ref m_CamAlign2, this, "Align2");
            p_sInfo = m_toolBox.Get(ref m_CamRADS, this, "RADS");
            if (m_CamRADS != null)
            {
                m_CamRADS.Connect();
            }
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            p_sInfo = m_toolBox.Get(ref m_inspectTool, this);
            p_sInfo = m_toolBox.Get(ref m_ZoomLens, this, "ZoomLens");

            p_sInfo = m_toolBox.Get(ref m_diPatternReticleExistSensor, this, "Pattern Reticle Sensor");
            p_sInfo = m_toolBox.Get(ref m_RADSControl, this, "RADSControl", true);
            if (bInit) m_inspectTool.OnInspectDone += M_inspectTool_OnInspectDone;
        }

        void bgw_Connect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private void M_inspectTool_OnInspectDone(InspectTool.Data data)
        {
            p_sInfo = data.p_sInfo; 
        }

        public override void InitMemorys()
        {
            //forget
            m_memoryMain = m_memoryPool.GetGroup("PatternVision").CreateMemory("Main", 1, 1, 1000, 1000);
        }
        #endregion

        #region DIO Function
        public bool p_bStageVac { get; set; }

        void RunTreeDIODelay(Tree tree)
        {

        }
        #endregion

        #region Grab Mode
        int m_lGrabMode = 0;
        public ObservableCollection<GrabMode> m_aGrabMode = new ObservableCollection<GrabMode>();
        public List<string> p_asGrabMode
        {
            get
            {
                List<string> asGrabMode = new List<string>();
                foreach (GrabMode grabMode in m_aGrabMode) asGrabMode.Add(grabMode.p_sName);
                return asGrabMode;
            }
        }

        public GrabMode GetGrabMode(string sGrabMode)
        {
            foreach (GrabMode grabMode in m_aGrabMode)
            {
                if (sGrabMode == grabMode.p_sName) return grabMode;
            }
            return null;
        }

        void RunTreeGrabMode(Tree tree)
        {
            m_lGrabMode = tree.Set(m_lGrabMode, m_lGrabMode, "Count", "Grab Mode Count");
            while (m_aGrabMode.Count < m_lGrabMode)
            {
                string id = "Mode." + m_aGrabMode.Count.ToString("00");
                GrabMode grabMode = new GrabMode(id, m_cameraSet, m_lightSet, m_memoryPool, m_RADSControl);
                m_aGrabMode.Add(grabMode);
            }
            while (m_aGrabMode.Count > m_lGrabMode) m_aGrabMode.RemoveAt(m_aGrabMode.Count - 1);
            foreach (GrabMode grabMode in m_aGrabMode) grabMode.RunTreeName(tree.GetTree("Name", false));
            foreach (GrabMode grabMode in m_aGrabMode) grabMode.RunTree(tree.GetTree(grabMode.p_sName, false), true, false);
        }
        #endregion

        #region Axis Function
        public enum eAxisPosZ
        {
            Safety,
            Grab,
            Ready,
            Align,
            ReticleCheck,
        }

        public enum eAxisPosX
        {
            Safety,
            Grab,
            Ready,
            Align,
            ReticleCheck,
        }

        public enum eAxisPosY
        {
            Safety,
            Grab,
            Ready,
            Align,
            ReticleCheck,
        }

        public enum eAxisPosClamp
        {
            Home,
            Open,
        }

        void InitPosAlign()
        {
            m_axisZ.AddPos(Enum.GetNames(typeof(eAxisPosZ)));
            m_axisClamp.AddPos(Enum.GetNames(typeof(eAxisPosClamp)));

            if(m_axisXY.p_axisX != null)
            {
                ((AjinAxis)m_axisXY.p_axisX).AddPos(Enum.GetNames(typeof(eAxisPosX)));
            }
            if(m_axisXY.p_axisY != null)
            {
                ((AjinAxis)m_axisXY.p_axisY).AddPos(Enum.GetNames(typeof(eAxisPosY)));
            }
        }
        #endregion

        #region IRobotChild
        bool _bLock = false;
        public bool p_bLock
        {
            get { return _bLock; }
            set
            {
                if (_bLock == value) return;
                _bLock = value;
            }
        }

        bool IsLock()
        {
            for (int n = 0; n < 10; n++)
            {
                if (p_bLock == false) return false;
                Thread.Sleep(100);
            }
            return true;
        }

        public string IsGetOK(ref int posRobot)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_infoReticle == null) return p_id + " IsGetOK - InfoReticle not Exist";
            posRobot = m_nTeachRobot;
            return "OK";
        }

        public string IsPutOK(ref int posRobot, InfoReticle infoReticle)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_infoReticle != null) return p_id + " IsPutOK - InfoReticle Exist";
            posRobot = m_nTeachRobot;
            return "OK";
        }

        public string BeforeGet()
        {
            // Clamp축 Home
            if (Run(m_axisClamp.StartHome())) return p_sInfo;
            if (Run(m_axisClamp.WaitReady())) return p_sInfo;
            if (m_axisClamp.p_sensorHome == false)   // 인터락 추가
            {
                p_sInfo = "Clamp Home이 완료되지 않았습니다.";
                return p_sInfo;
            }

            // 레티클 유무 체크 위치로 이동
            if (Run(m_axisXY.p_axisX.StartMove(eAxisPosX.Ready))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            if (Run(m_axisXY.p_axisY.StartMove(eAxisPosY.ReticleCheck))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            if (Run(m_axisZ.StartMove(eAxisPosZ.Ready))) return p_sInfo;
            if (Run(m_axisZ.WaitReady())) return p_sInfo;

            // 레티클 유무 체크
            string strLightName = "Align1_1";
            SetLightByName(strLightName, 10);
            Thread.Sleep(100);
            m_CamAlign1.GrabOneShot();
            Thread.Sleep(100);
            SetLightByName(strLightName, 0);

            strLightName = "Align2_1";
            SetLightByName(strLightName, 10);
            Thread.Sleep(100);
            m_CamAlign2.GrabOneShot();
            Thread.Sleep(100);
            SetLightByName(strLightName, 0);

            //bool bRet = ReticleExistCheck(m_CamAlign1);
            //if (bRet == false) return "Reticle Not Exist";
            //bRet = ReticleExistCheck(m_CamAlign2);
            //if (bRet == false) return "Reticle Not Exist";

            if (m_diPatternReticleExistSensor.p_bIn == false) return "Reticle Not Exist";

            // Clamp 축 열기
            if (Run(m_axisClamp.StartMove(eAxisPosClamp.Open))) return p_sInfo;
            if (Run(m_axisClamp.WaitReady())) return p_sInfo;

            if (p_infoReticle == null) return p_id + " BeforeGet : InfoReticle = null";
            return CheckGetPut();
        }

        public string BeforePut()
        {
            // Clamp축 Home
            if (Run(m_axisClamp.StartHome())) return p_sInfo;
            if (Run(m_axisClamp.WaitReady())) return p_sInfo;
            if (m_axisClamp.p_sensorHome == false)
            {
                p_sInfo = "Clamp축 Home 완료되지 않았습니다.";
                return p_sInfo;
            }

            // 레티클 유무 체크 위치로 이동
            if (Run(m_axisXY.p_axisX.StartMove(eAxisPosX.Ready))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            if (Run(m_axisXY.p_axisY.StartMove(eAxisPosY.ReticleCheck))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            if (Run(m_axisZ.StartMove(eAxisPosZ.Ready))) return p_sInfo;
            if (Run(m_axisZ.WaitReady())) return p_sInfo;

            // 레티클 유무 체크
            string strLightName = "Align1_1";
            SetLightByName(strLightName, 10);
            Thread.Sleep(100);
            m_CamAlign1.GrabOneShot();
            Thread.Sleep(100);
            SetLightByName(strLightName, 0);

            strLightName = "Align2_1";
            SetLightByName(strLightName, 10);
            Thread.Sleep(100);
            m_CamAlign2.GrabOneShot();
            Thread.Sleep(100);
            SetLightByName(strLightName, 0);

            //bool bRet = ReticleExistCheck(m_CamAlign1);
            //if (bRet == false) return "Reticle Not Exist";
            //bRet = ReticleExistCheck(m_CamAlign2);
            //if (bRet == false) return "Reticle Not Exist";

            if (m_diPatternReticleExistSensor.p_bIn == true) return "Reticle Exist";

            // Clamp 축 열기
            if (Run(m_axisClamp.StartMove(eAxisPosClamp.Open))) return p_sInfo;
            if (Run(m_axisClamp.WaitReady())) return p_sInfo;

            if (p_infoReticle != null) return p_id + " BeforePut : InfoReticle != null";
            return CheckGetPut();
        }

        public string AfterGet()
        {
            // Clamp Home
            if (Run(m_axisClamp.StartHome())) return p_sInfo;
            if (Run(m_axisClamp.WaitReady())) return p_sInfo;

            return CheckGetPut();
        }

        public string AfterPut()
        {
            // Clamp Home
            if (Run(m_axisClamp.StartHome())) return p_sInfo;
            if (Run(m_axisClamp.WaitReady())) return p_sInfo;

            return CheckGetPut();
        }

        string CheckGetPut()
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            //LoadPos ??
            return "OK";
        }

        public bool IsReticleExist(bool bIgnoreExistSensor = false)
        {
            bool bExist = false;
            if (bIgnoreExistSensor) bExist = (p_infoReticle != null);
            else
            {
                //forget
            }
            p_brushReticleExist = bExist ? Brushes.Yellow : Brushes.Green;
            return bExist;
        }

        bool ReticleExistCheck(Camera_Basler cam)
        {
            // variable
            ImageData img = cam.p_ImageViewer.p_ImageData;
            Point pt1, pt2;
            Rect rcROI;
            bool bFindReticle = false;
            Mat matSrc;
            Mat matBinary;
            CvBlobs blobs;
            CvBlobDetector blobDetector;
            Image<Gray, Byte> imgSrc;

            // implement
            if (cam.p_ImageViewer.m_BasicTool.m_ListRect.Count < 1) return false; // ROI 없으면 return
            pt1 = cam.p_ImageViewer.m_BasicTool.m_ListRect[0].StartPos;
            pt2 = cam.p_ImageViewer.m_BasicTool.m_ListRect[0].EndPos;
            rcROI = new Rect(pt1, pt2);

            // Binarization
            matSrc = new Mat((int)rcROI.Height, (int)rcROI.Width, Emgu.CV.CvEnum.DepthType.Cv8U, img.p_nByte, img.GetPtr((int)rcROI.Top, (int)rcROI.Left), (int)img.p_Stride);
            matBinary = new Mat();
            CvInvoke.Threshold(matSrc, matBinary, /*p_nThreshold*/200, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

            // Blob Detection
            blobs = new CvBlobs();
            blobDetector = new CvBlobDetector();
            imgSrc = matBinary.ToImage<Gray, Byte>();
            blobDetector.Detect(imgSrc, blobs);

            matSrc.Save($"D:\\SRC.BMP");
            matBinary.Save($"D:\\BINARY.BMP");

            // Bounding Box
            foreach (CvBlob blob in blobs.Values)
            {
                if (blob.BoundingBox.Width == rcROI.Width)
                {
                    bFindReticle = true;
                    break;
                }
            }

            return bFindReticle;
        }

        Brush _brushReticleExist = Brushes.Green;
        public Brush p_brushReticleExist
        {
            get { return _brushReticleExist; }
            set
            {
                if (_brushReticleExist == value) return;
                _brushReticleExist = value;
                OnPropertyChanged();
            }
        }

        int m_nTeachRobot = 0;
        public void RunTeachTree(Tree tree)
        {
            m_nTeachRobot = tree.Set(m_nTeachRobot, m_nTeachRobot, p_id, "Robot Teach Index");
        }
        #endregion 

        #region InfoReticle
        string m_sInfoReticle = "";
        InfoReticle _infoReticle = null;
        public InfoReticle p_infoReticle
        {
            get { return _infoReticle; }
            set
            {
                m_sInfoReticle = (value == null) ? "" : value.p_id;
                _infoReticle = value;
                if (m_reg != null) m_reg.Write("sInfoReticle", m_sInfoReticle);
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadInfoReticle_Registry()
        {
            m_reg = new Registry(p_id + ".InfoReticle");
            m_sInfoReticle = m_reg.Read("sInfoReticle", m_sInfoReticle);
            //p_infoReticle = m_engineer.ClassHandler().GetGemSlot(m_sInfoReticle);
            if (m_diPatternReticleExistSensor.p_bIn == false) p_infoReticle = null;
            else p_infoReticle = m_engineer.ClassHandler().GetGemSlot(m_sInfoReticle);
        }
        #endregion

        #region override Function
        //public override string StateHome()
        //{
        //    if (EQ.p_bSimulate) return "OK";
        //    p_bStageVac = true;
        //    Thread.Sleep(200);
        //    p_sInfo = base.StateHome();
        //    p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
        //    p_bStageVac = false;
        //    return p_sInfo;
        //}

        public override string StateHome()  // 200804 ESCHO - 기존 Home Sequence의 경우 축들이 동시에 Home을 하기 때문에 충돌위험이 SideVision과 동일하게 새로 만듦.
        {
            p_sInfo = "OK";
            if (EQ.p_bSimulate) return "OK";
            p_bStageVac = true;
            Thread.Sleep(200);

            if (m_listAxis.Count == 0) return "OK";
            if (p_eState == eState.Run) return "Invalid State : Run";
            if (EQ.IsStop()) return "Home Stop";
            foreach (Axis axis in m_listAxis)
            {
                if (axis != null) axis.ServoOn(true);
            }
            Thread.Sleep(200);
            if (EQ.IsStop()) return "Home Stop";

            m_axisClamp.StartHome();
            if (m_axisClamp.WaitReady() != "OK")
            {
                p_bStageVac = false;
                p_eState = eState.Error;
                p_sInfo = "AxisClamp Home Error";
                return p_sInfo;
            }

            m_axisXY.p_axisX.StartHome();
            m_axisXY.p_axisY.StartHome();
            m_axisZ.StartHome();

            if (m_axisXY.p_axisX.WaitReady() != "OK")
            {
                p_bStageVac = false;
                p_eState = eState.Error;
                return "AxisX Home Error";
            }

            if (m_axisXY.p_axisY.WaitReady() != "OK")
            {
                p_bStageVac = false;
                p_eState = eState.Error;
                return "AxisY Home Error";
            }

            if (m_axisZ.WaitReady() != "OK")
            {
                p_bStageVac = false;
                p_eState = eState.Error;
                return "AxisZ Home Error";
            }

            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            p_bStageVac = false;
            return p_sInfo;
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            RunTreeDIODelay(tree.GetTree("DIO Delay", false));
            m_memoryPool.RunTreeModule(tree.GetTree("Memory", false));
            RunTreeGrabMode(tree.GetTree("Grab Mode", false));
        }
        #endregion

        #region RelayCommand
        void Jog_Plus_Fast()
        {
            if (p_axisClamp.p_sensorHome == false) return;
            p_axisClamp.Jog(1); 
        }
        void Jog_Minus_Fast()
        {
            if (p_axisClamp.p_sensorHome == false) return;
            p_axisClamp.Jog(-1);
        }
        void MovePosition()
        {
            if (p_axisClamp.p_sensorHome == false) return;
            //            ((AjinAxis)p_axisXY.p_axisY).MovePosition();
        }
        void PlusRelativeMove()
        {
            if (p_axisClamp.p_sensorHome == false) return;
            //            ((AjinAxis)p_axisXY.p_axisY).PlusRelativeMove();
        }
        void MinusRelativeMove()
        {
            if (p_axisClamp.p_sensorHome == false) return;
            //            ((AjinAxis)p_axisXY.p_axisY).MinusRelativeMove();
        }

        public RelayCommand PJogFastCommand
        {
            get
            {
                return new RelayCommand(Jog_Plus_Fast);
            }
        }
        public RelayCommand MJogFastCommand
        {
            get
            {
                return new RelayCommand(Jog_Minus_Fast);
            }
        }
        public RelayCommand MoveCommand
        {
            get
            {
                return new RelayCommand(MovePosition);
            }
        }
        public RelayCommand PlusRelativeMoveCommand
        {
            get
            {
                return new RelayCommand(PlusRelativeMove);
            }
        }
        public RelayCommand MinusRelativeMoveCommand
        {
            get
            {
                return new RelayCommand(MinusRelativeMove);
            }
        }
        #endregion

        public PatternVision(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
            InitPosAlign();
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        public void SetLightMainCam1(int nPower)
        {  
            m_lightSet.m_aLight[0].m_light.p_fSetPower = nPower;
        }
        public void SetLightMainCam2(int nPower)
        {
            m_lightSet.m_aLight[1].m_light.p_fSetPower = nPower;
        }
        public void SetLightVRS(int nPower)
        {
            m_lightSet.m_aLight[2].m_light.p_fSetPower = nPower;
        }
        public void SetLightINC1(int nPower)
        {
            m_lightSet.m_aLight[3].m_light.p_fSetPower = nPower;
            m_lightSet.m_aLight[4].m_light.p_fSetPower = nPower;
        }
        public void SetLightINC2(int nPower)
        {
            m_lightSet.m_aLight[5].m_light.p_fSetPower = nPower;
            m_lightSet.m_aLight[6].m_light.p_fSetPower = nPower;
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
            AddModuleRunList(new Run_Run(this), true, "Run Side Vision");
            AddModuleRunList(new Run_Grab(this), true, "Run Grab");
            AddModuleRunList(new Run_Inspection(this), true, "Run Inspection");
        }

        public class Run_Delay : ModuleRunBase
        {
            PatternVision m_module;
            public Run_Delay(PatternVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_secDelay = 2;
            public override ModuleRunBase Clone()
            {
                Run_Delay run = new Run_Delay(m_module);
                run.m_secDelay = m_secDelay;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }
            public override string Run()
            {
                Thread.Sleep((int)(1000 * m_secDelay));
                return "OK";
            }
        }

        public class Run_Run : ModuleRunBase
        {
            PatternVision m_module;
            public Run_Run(PatternVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_secDelay = 2;
            public override ModuleRunBase Clone()
            {
                Run_Run run = new Run_Run(m_module);
                run.m_secDelay = m_secDelay;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }
            public override string Run()
            {
                Thread.Sleep((int)(1000 * m_secDelay));
                return "OK";
            }
        }

        public class Run_Grab : ModuleRunBase
        {
            PatternVision m_module;
            public Run_Grab(PatternVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public GrabMode m_grabMode = null;
            string _sGrabMode = "";
            string p_sGrabMode
            {
                get { return _sGrabMode; }
                set
                {
                    _sGrabMode = value;
                    m_grabMode = m_module.GetGrabMode(value);
                }
            }

            bool m_bInvDir = false;
            public RPoint m_rpAxis = new RPoint();
            public double m_fYRes = 1;
            public double m_fXRes = 1;
            public int m_nFocusPos = 0;
            public CPoint m_cpMemory = new CPoint();
            public int m_nMaxFrame = 100;  // Camera max Frame 스펙
            public int m_nScanRate = 100;   // Camera Frame Spec 사용률 ? 1~100 %
            public int m_yLine = 1000;
            public override ModuleRunBase Clone()
            {
                Run_Grab run = new Run_Grab(m_module);
                run.p_sGrabMode = p_sGrabMode;
                run.m_fYRes = m_fYRes;
                run.m_fXRes = m_fXRes;
                run.m_bInvDir = m_bInvDir;
                run.m_nFocusPos = m_nFocusPos;
                run.m_rpAxis = new RPoint(m_rpAxis);
                run.m_cpMemory = new CPoint(m_cpMemory);
                run.m_yLine = m_yLine;
                run.m_nMaxFrame = m_nMaxFrame;
                run.m_nScanRate = m_nScanRate;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_rpAxis = tree.Set(m_rpAxis, m_rpAxis, "Center Axis Position", "Center Axis Position (mm ?)", bVisible);
                m_fYRes = tree.Set(m_fYRes, m_fYRes, "Cam YResolution", "YResolution  um", bVisible);
                m_fXRes = tree.Set(m_fXRes, m_fXRes, "Cam XResolution", "XResolution  um", bVisible);
                m_nFocusPos = tree.Set(m_nFocusPos, 0, "Focus Z Pos", "Focus Z Pos", bVisible);
                m_cpMemory = tree.Set(m_cpMemory, m_cpMemory, "Memory Position", "Grab Start Memory Position (pixel)", bVisible);
                m_bInvDir = tree.Set(m_bInvDir, m_bInvDir, "Inverse Direction", "Grab Direction", bVisible);
                m_yLine = tree.Set(m_yLine, m_yLine, "WaferSize", "# of Grab Lines", bVisible);
                m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
                m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
                p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
                if (m_grabMode != null) m_grabMode.RunTree(tree.GetTree("Grab Mode", false), bVisible, true);
            }

            public override string Run()
            {
                if (m_grabMode == null) return "Grab Mode == null";
                bool bUseRADS = m_grabMode.GetUseRADS();
                try
                {
                    int nScanLine = 0;
                    m_grabMode.SetLight(true);
                    if (bUseRADS && (m_grabMode.m_RADSControl.p_IsRun == false))
                    {
                        m_grabMode.m_RADSControl.p_IsRun = true;
                        m_grabMode.m_RADSControl.StartRADS();
                        m_module.m_CamRADS.GrabContinuousShot();
                    }
                    AxisXY axisXY = m_module.p_axisXY;
                    Axis axisZ = m_module.p_axisZ;
                    m_cpMemory.X += (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X;
                    m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_fYRes);        // 축해상도 0.1um로 하드코딩.
                    double XScal = m_fXRes*10;
                    int nLines = Convert.ToInt32(m_yLine * 1000 / m_fYRes);
                    while (m_grabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";
                        double yAxis = m_grabMode.m_dTrigger * nLines;     // 총 획득할 Image Y 
                        /*위에서 아래로 찍는것을 정방향으로 함, 즉 Y 축 값이 큰쪽에서 작은쪽으로 찍는것이 정방향*/
                        /* Grab하기 위해 이동할 Y축의 시작 끝 점*/
                        double yPos1 = m_rpAxis.Y - yAxis / 2 - 300000;
                        double yPos0 = m_rpAxis.Y + yAxis / 2 + 300000;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;
                        if (m_grabMode.m_bUseBiDirectionScan && Math.Abs(axisXY.p_axisY.p_posActual - yPos0) > Math.Abs(axisXY.p_axisY.p_posActual - yPos1))
                        {
                            double buffer = yPos0;            //yp1 <--> yp0 바꿈.
                            yPos0 = yPos1;
                            yPos1 = buffer;
                            m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                        }

                        /* 조명 Set하는거 Test해서 넣어야됨.*/
                        //m_grabMode.SetLight(true);
                        double nPosX = m_rpAxis.X + nLines * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X * XScal; //해상도추가필요

                        if (m_module.Run(axisZ.StartMove(m_nFocusPos)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.StartMove(new RPoint(nPosX, yPos0))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;

                        double yTrigger0 = m_rpAxis.Y - yAxis / 2;
                        double yTrigger1 = m_rpAxis.Y + yAxis / 2;
                        m_module.p_axisXY.p_axisY.SetTrigger(yTrigger0 - 100000, yTrigger1, m_grabMode.m_dTrigger, true);

                        string sPool = m_grabMode.m_memoryPool.p_id;
                        string sGroup = m_grabMode.m_memoryGroup.p_id;
                        string sMem = m_grabMode.m_memoryData.p_id;
                        MemoryData mem = m_module.m_engineer.GetMemory(sPool, sGroup, sMem);
                        int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * m_grabMode.m_camera.GetRoiSize().Y * (double)m_nScanRate / 100);

                        /* 방향 바꾸는 코드 들어가야함*/
                        m_grabMode.StartGrab(mem, m_cpMemory, nLines, m_grabMode.m_eGrabDirection == eGrabDirection.BackWard);
                        if (m_module.Run(axisXY.p_axisY.StartMove(yPos1, nScanSpeed)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        axisXY.p_axisY.RunTrigger(false);

                        nScanLine++;
                        m_cpMemory.X += m_grabMode.m_camera.GetRoiSize().X;
                    }
                    m_grabMode.m_camera.StopGrab();
                    #region Grab1차 Test후에 코드 부분
                    //double yp0 = m_bInvDir ? m_rpAxis.Y + yAxis + m_grabMode.m_mmAcc : m_rpAxis.Y - m_grabMode.m_mmAcc;
                    //double yp1 = m_bInvDir ? m_rpAxis.Y - m_grabMode.m_mmAcc : m_rpAxis.Y + yAxis + m_grabMode.m_mmAcc;

                    //m_grabMode.SetLight(true);

                    //double yp0 = m_bInvDir ? m_rpAxis.Y + yAxis + m_grabMode.m_mmAcc : m_rpAxis.Y - m_grabMode.m_mmAcc;
                    //double yp1 = m_bInvDir ? m_rpAxis.Y - m_grabMode.m_mmAcc : m_rpAxis.Y + yAxis + m_grabMode.m_mmAcc;
                    //if (m_module.Run(axisXY.Move(new RPoint(m_rpAxis.X, yp1))))
                    //    return p_sInfo;
                    //if (m_module.Run(axisXY.WaitReady()))
                    //    return p_sInfo;

                    //double yTrigger0 = m_bInvDir ? m_rpAxis.Y - m_grabMode.m_mmAcc : m_rpAxis.Y;
                    //double yTrigger1 = m_bInvDir ? m_rpAxis.Y + yAxis : m_rpAxis.Y + yAxis;
                    //m_module.m_axisXY.m_axisY.SetTrigger(yTrigger0, yTrigger1, 10, true);
                    ////m_grabMode.SetTrigger(m_module.m_axisXY.m_axisY, yTrigger0, yTrigger1);
                    //string sPool = "pool";
                    //string sGroup = "grou p";
                    //string sMem = "mem";
                    //MemoryData mem = m_module.m_engineer.ClassMemoryTool().GetPool(sPool).GetGroup(sGroup).GetMemory(sMem);

                    ////m_grabMode.StartGrab(m_grabMode.m_memoryData, new CPoint(0, 0), m_yLine, m_bInvDir);
                    //m_grabMode.StartGrab(mem, m_cpMemory, m_yLine, m_bInvDir);
                    //if (m_module.Run(axisXY.m_axisY.Move(yp0)))
                    //    return p_sInfo;
                    //if (m_module.Run(axisXY.WaitReady()))
                    //    return p_sInfo;
                    //axisXY.m_axisY.ResetTrigger();
                    #endregion
                    return "OK";
                }
                finally
                {
                    m_grabMode.SetLight(false);
                    if (bUseRADS && (m_grabMode.m_RADSControl.p_IsRun == true))
                    {
                        m_grabMode.m_RADSControl.p_IsRun = false;
                        m_grabMode.m_RADSControl.StopRADS();
                        m_module.m_CamRADS.StopGrab();
                    }
                }
            }
        }

        public class Run_Inspection : ModuleRunBase
        {
            PatternVision m_module;
            public _2_5_MainVisionViewModel m_mvvm;

            public Run_Inspection(PatternVision module)
            {
                m_module = module;
                m_mvvm = m_module.m_mvvm;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Inspection run = new Run_Inspection(m_module);
                return run;
            }

            public void RunTree(TreeRoot treeRoot, Tree.eMode mode)
            {
                treeRoot.p_eMode = mode;
                RunTree(treeRoot, true);
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                // 검사시작 전 확인사항 조건 추가해야함

                // 검사시작
                m_mvvm._dispatcher.Invoke(new Action(delegate ()
                {
                    m_mvvm._startInsp();
                }));

                // DB에 Write완료될때까지 기다리는 루틴 추가해야함

                return "OK";
            }
        }
        #endregion
    }
}
