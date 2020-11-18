using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.Structure;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Camera.Matrox;
using RootTools.Camera.Silicon;
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
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DPoint = System.Drawing.Point;
using MBrushes = System.Windows.Media.Brushes;

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
        //public Camera_Matrox m_CamTest;
        public Camera_Silicon m_CamSilicon;

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

        public MemoryPool m_memoryPool;
        
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
            p_sInfo = m_toolBox.Get(ref m_CamVRS, this, "VRS");
            p_sInfo = m_toolBox.Get(ref m_CamAlign1, this, "Align1");
            p_sInfo = m_toolBox.Get(ref m_CamAlign2, this, "Align2");
            p_sInfo = m_toolBox.Get(ref m_CamRADS, this, "RADS");
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory", 1);
            p_sInfo = m_toolBox.Get(ref m_inspectTool, this);
            p_sInfo = m_toolBox.Get(ref m_ZoomLens, this, "ZoomLens");

            //p_sInfo = m_toolBox.Get(ref m_CamTest, this, "Test");
            p_sInfo = m_toolBox.Get(ref m_CamSilicon, this, "Silicon");
            

            p_sInfo = m_toolBox.Get(ref m_diPatternReticleExistSensor, this, "Pattern Reticle Sensor");

            // 노트북에서 구동시 RADS 내부에서 DataSocket 생성 후 무한루프에 빠지는 문제로 인해 조건 추가
            bool bUseRADS = false;
            if (m_CamRADS.p_CamInfo.m_Cam != null) bUseRADS = true;
            p_sInfo = m_toolBox.Get(ref m_RADSControl, this, "RADSControl", bUseRADS);

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
                bExist = m_diPatternReticleExistSensor.p_bIn;
                
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
            if (m_axisXY.p_axisY.IsInPos(eAxisPosY.ReticleCheck) == true)
            {
                if (m_diPatternReticleExistSensor.p_bIn == false) p_infoReticle = null;
            }
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
            if (EQ.p_bSimulate) 
                return "OK";
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

            // Y축 레티클체크포지션으로 이동
            if (Run(m_axisXY.p_axisY.StartMove(eAxisPosY.ReticleCheck))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            if (m_axisXY.p_axisY.IsInPos(eAxisPosY.ReticleCheck) == true)
            {
                if (p_infoReticle != null)
                {
                    if (m_diPatternReticleExistSensor.p_bIn == false) p_infoReticle = null;
                }
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
            AddModuleRunList(new Run_AutoIllumination(this), true, "Run AutoIllumination");
            AddModuleRunList(new Run_VRSReviewImagCapture(this), true, "Run VRSReviewImageCapture");
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
        //------------------------------------------------------
        public class Run_Grab : ModuleRunBase
        {
            //------------------------------------------------------
            PatternVision m_module;
            public _2_5_MainVisionViewModel m_mvvm;
            public RPoint m_rpReticleCenterPos_pulse = new RPoint();    // Reticle 중심의 XY Postiion [pulse]
            public CPoint m_cpMemoryOffset_pixel = new CPoint();        // Memory Offset [pixel]
            public bool m_bInvDir = false;                              // 역방향 스캔
            public double m_dResX_um = 1;                               // Camera X Resolution [um]
            public double m_dResY_um = 1;                               // Camera Y Resolution [um]
            public double m_dReticleSize_mm = 1000;                     // Reticle Size [mm]
            public double m_dFocusPosZ_pulse = 0;                       // Focus Z Position [pulse]
            public double m_dTriggerUptime = 5;                         // Trigger Uptime [us]
            public double m_dTriggerPeriod = 1;                         // Trigger Period [us]    
            public int m_nMaxFrame = 100;                               // Camera max Frame 스펙
            public int m_nScanRate = 100;                               // Camera Frame Spec 사용률 ? 1~100 %

            public bool m_bUseInspect = false;                          // 검사 유무
            
            public GrabMode m_grabMode = null;
            string m_sGrabMode = "";
            string p_sGrabMode
            {
                get { return m_sGrabMode; }
                set
                {
                    m_sGrabMode = value;
                    m_grabMode = m_module.GetGrabMode(value);
                }
            }
            //------------------------------------------------------
            public Run_Grab(PatternVision module)
            {
                m_module = module;
                m_mvvm = m_module.m_mvvm;
                InitModuleRun(module);
            }
            //------------------------------------------------------
            public override ModuleRunBase Clone()
            {
                Run_Grab run = new Run_Grab(m_module);
                run.p_sGrabMode = p_sGrabMode;
                run.m_rpReticleCenterPos_pulse = m_rpReticleCenterPos_pulse;
                run.m_cpMemoryOffset_pixel = m_cpMemoryOffset_pixel;
                run.m_bInvDir = m_bInvDir;
                run.m_dResX_um = m_dResX_um;
                run.m_dResY_um = m_dResY_um;
                run.m_dReticleSize_mm = m_dReticleSize_mm;
                run.m_dFocusPosZ_pulse = m_dFocusPosZ_pulse;
                run.m_dTriggerUptime = m_dTriggerUptime;
                run.m_dTriggerPeriod = m_dTriggerPeriod;
                run.m_nMaxFrame = m_nMaxFrame;
                run.m_nScanRate = m_nScanRate;

                run.m_bUseInspect = m_bUseInspect;

                return run;
            }
            //------------------------------------------------------
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_rpReticleCenterPos_pulse = tree.Set(m_rpReticleCenterPos_pulse, m_rpReticleCenterPos_pulse, "Center Axis Position [Pulse]", "Center Axis Position [Pulse]", bVisible);
                m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Camera X Resolution [um]", "Camera X Resolution [um]", bVisible);
                m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Camera Y Resolution [um]", "Camera Y Resolution [um]", bVisible);
                m_dFocusPosZ_pulse = tree.Set(m_dFocusPosZ_pulse, m_dFocusPosZ_pulse, "Focus Z Position [Pulse]", "Focus Z Position [Pulse]", bVisible);
                m_cpMemoryOffset_pixel = tree.Set(m_cpMemoryOffset_pixel, m_cpMemoryOffset_pixel, "Grab Start Memory Position [px]", "Grab Start Memory Position [px]", bVisible);
                m_bInvDir = tree.Set(m_bInvDir, m_bInvDir, "Inverse Direction", "Grab Direction", bVisible);
                m_dReticleSize_mm = tree.Set(m_dReticleSize_mm, m_dReticleSize_mm, "Reticle Size [mm]", "Reticle Size [mm]", bVisible);
                m_dTriggerUptime = (tree.GetTree("Trigger Parameter", false, bVisible)).Set(m_dTriggerUptime, m_dTriggerUptime, "Trigger Uptime [us]", "Trigger Uptime [us]", bVisible);
                m_dTriggerPeriod = (tree.GetTree("Trigger Parameter", false, bVisible)).Set(m_dTriggerPeriod, m_dTriggerPeriod, "Trigger Period", "Trigger Period", bVisible);
                m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
                m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
                p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
                if (m_grabMode != null) m_grabMode.RunTree(tree.GetTree("Grab Mode", false, bVisible), bVisible, true);

                m_bUseInspect = tree.Set(m_bUseInspect, m_bUseInspect, "Use Inspection", "Use Inspection", bVisible);
            }
            //------------------------------------------------------
            public override string Run()
            {
                // Scan variable
                AxisXY axisXY = m_module.p_axisXY;
                Axis axisZ = m_module.p_axisZ;
                CPoint cpMemoryOffset_pixel = new CPoint(m_cpMemoryOffset_pixel);
                int nScanLine = 0;
                int nMMPerUM = 1000;
                int nCamWidth = m_grabMode.m_camera.GetRoiSize().X;
                int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;
                nCamWidth = 3000;
                nCamHeight = 200;
                int nReticleYSize_px = Convert.ToInt32(m_dReticleSize_mm * nMMPerUM / m_dResY_um);    // 레티클 영역(150mm -> 150,000um)의 Y픽셀 갯수
                m_grabMode.m_dTrigger =m_dResY_um / 8 * 100;        // 축해상도 0.1um로 하드코딩.
                int nReticleRangePulse = Convert.ToInt32(m_grabMode.m_dTrigger * nReticleYSize_px);   // 스캔영역 중 레티클 스캔 구간에서 발생할 Trigger 갯수
                double dXScale = m_dResX_um / 10 * 100;
                bool bUseRADS = false;

                // Inspection variable
                bool bFeatureScanned = false;
                bool bFoundFeature = false;
                CPoint cptStandard = new CPoint(0, 0);
                int nRefStartOffsetX = 0;
                int nRefStartOffsetY = 0;
                int nInspectStartIndex = 0;

                Vega_Engineer engineer = (Vega_Engineer)m_module.m_engineer;


                // implement
                try
                {
                    if (m_grabMode == null) return "Grab Mode == null";
                    m_grabMode.SetLight(true);
                    if (bUseRADS && (m_grabMode.m_RADSControl.p_IsRun == false))
                    {
                        m_grabMode.m_RADSControl.p_IsRun = true;
                        m_grabMode.m_RADSControl.StartRADS();
                        m_module.m_CamRADS.GrabContinuousShot();
                    }

                    cpMemoryOffset_pixel.X += (m_grabMode.m_ScanStartLine * nCamWidth);

                    if (m_bUseInspect)
                    {
                        // 검사 Queue Monitoring 시작 -> Side에서 검사가 먼저 시작됐을 경우 그냥 Return
                        engineer.m_InspManager.StartInspection();
                    }

                    while (m_grabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop()) return "OK";

                        int nSpareDistancePulse = 300000;
                        double dStartAxisPos = m_rpReticleCenterPos_pulse.Y + (nReticleRangePulse / 2) + nSpareDistancePulse;
                        double dEndAxisPos = m_rpReticleCenterPos_pulse.Y - (nReticleRangePulse / 2) - nSpareDistancePulse;
                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;
                        if (m_grabMode.m_bUseBiDirectionScan && Math.Abs(axisXY.p_axisY.p_posActual - dStartAxisPos) > Math.Abs(axisXY.p_axisY.p_posActual - dEndAxisPos))
                        {
                            double dTemp = dStartAxisPos;
                            dStartAxisPos = dEndAxisPos;
                            dEndAxisPos = dTemp;
                            m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                        }
                        //double dAxisPosX = m_rpReticleCenterPos_pulse.X + (m_dReticleSize_mm * nMMPerUM / 0.08 / 2) - (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth * dXScale; //해상도추가필요
                        double dAxisPosX = m_rpReticleCenterPos_pulse.X + (150 * nMMPerUM / 0.1 / 2) - (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth * dXScale; //해상도추가필요

                        //if (m_module.Run(axisXY.StartMove(new RPoint(dAxisPosX, dStartAxisPos)))) return p_sInfo;
                        //if (m_module.Run(axisZ.StartMove(m_dFocusPosZ_pulse))) return p_sInfo;
                        //if (m_module.Run(axisXY.WaitReady())) return p_sInfo;
                        //if (m_module.Run(axisZ.WaitReady())) return p_sInfo;

                        double dStartTriggerPos = m_rpReticleCenterPos_pulse.Y + nReticleRangePulse / 2;
                        double dEndTriggerPos = m_rpReticleCenterPos_pulse.Y - nReticleRangePulse / 2;
                        //m_module.p_axisXY.p_axisY.SetTrigger(dStartTriggerPos, dEndTriggerPos, m_dTriggerPeriod, m_dTriggerUptime, true);

                        string strPool = m_grabMode.m_memoryPool.p_id;
                        string strGroup = m_grabMode.m_memoryGroup.p_id;
                        string strMem = m_grabMode.m_memoryData.p_id;
                        MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMem);
                        int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * (double)m_nScanRate / 100);

                        //m_grabMode.StartGrab(mem, cpMemoryOffset_pixel, nReticleYSize_px, m_grabMode.m_eGrabDirection == eGrabDirection.BackWard);
                        //if (m_module.Run(axisXY.p_axisY.StartMove(dEndAxisPos, nScanSpeed))) return p_sInfo;
                        //if (m_module.Run(axisXY.WaitReady())) return p_sInfo;
                        //axisXY.p_axisY.RunTrigger(false);

                        #region Inspection
                        // Inspection
                        if (m_bUseInspect == true)
                        {
                            if (bFeatureScanned == false)
                            {
                                // Feature가 스캔됐는지 확인
                                bFeatureScanned = m_mvvm.IsFeatureScanned(cpMemoryOffset_pixel.X, nCamWidth);
                            }
                            #region Feature
                            if (bFeatureScanned && (bFoundFeature == false))
                            {
                                // Feature 탐색 시작
                                Roi roiCurrent = m_mvvm.p_PatternRoiList[0];
                                foreach (var feature in roiCurrent.Position.ReferenceList)
                                {
                                    CRect crtSearchArea;
                                    Point ptMaxRelative;
                                    int nWidthDiff, nHeightDiff;
                                    bFoundFeature = m_mvvm.FindFeature(feature, out crtSearchArea, out ptMaxRelative, out nWidthDiff, out nHeightDiff);

                                    if (bFoundFeature == true)
                                    {
                                        //2. feature 중심위치가 확보되면 해당 좌표를 저장
                                        cptStandard.X = crtSearchArea.Left + (int)ptMaxRelative.X + (nWidthDiff / 2);
                                        cptStandard.Y = crtSearchArea.Top + (int)ptMaxRelative.Y + (nHeightDiff / 2);
                                        nRefStartOffsetX = feature.PatternDistX;
                                        nRefStartOffsetY = feature.PatternDistY;
                                        
                                        if (m_mvvm._dispatcher != null)
                                        {
                                            m_mvvm._dispatcher.Invoke(new Action(delegate ()
                                            {
                                                m_mvvm.DrawCross(new DPoint(cptStandard.X, cptStandard.Y), MBrushes.Red);
                                            }));
                                        }

                                        // Origin 생성
                                        CPoint cptOriginStart = new CPoint(cptStandard.X + nRefStartOffsetX, cptStandard.Y + nRefStartOffsetY);
                                        roiCurrent.Origin.OriginRect = new CRect(cptOriginStart.X, cptOriginStart.Y, cptOriginStart.X + (int)roiCurrent.Strip.ParameterList[0].InspAreaWidth, cptOriginStart.Y + (int)roiCurrent.Strip.ParameterList[0].InspAreaHeight);
                                        break;  // 찾았으니 중단
                                    }
                                    else
                                    {
                                        continue;   // 못 찾았으면 다음 Feature값으로 이동
                                    }
                                }
                            }
                            #endregion
                            #region Align Key
                            // Align Key 추가해야함
                            #endregion
                            #region Inspect Area
                            if (bFoundFeature)
                            {
                                Roi roiCurrent = m_mvvm.p_PatternRoiList[0];

                                // 1. 검사영역 생성
                                Point ptStartPos = new Point(cptStandard.X + nRefStartOffsetX + (nInspectStartIndex * nCamWidth), 0);
                                Point ptEndPos = new Point(ptStartPos.X + nCamWidth, nReticleYSize_px);
                                CRect crtCurrentArea = new CRect(ptStartPos, ptEndPos);

                                // 1.2 생성된 검사영역이 스캔됐는지 판단
                                bool bScanned = false;
                                if (crtCurrentArea.Right < (cpMemoryOffset_pixel.X + nCamWidth)) bScanned = true;
                                if (bScanned == true)
                                {
                                    nInspectStartIndex++;
                                    // 2. 생성된 검사영역과 ROI의 겹치는 Rect 추출
                                    CRect crtOverlapedRect = m_mvvm.GetOverlapedRect(crtCurrentArea, roiCurrent.Origin.OriginRect);
                                    if ((crtOverlapedRect.Width > 0) && (crtOverlapedRect.Height > 0))
                                    {
                                        // 3. Overlap된 Rect영역을 검사 쓰레드로 던져라
                                        if (m_mvvm._dispatcher != null)
                                        {
                                            m_mvvm._dispatcher.Invoke(new Action(delegate ()
                                            {
                                                // UI
                                                var temp = new UIElementInfo(new Point(crtOverlapedRect.Left, crtOverlapedRect.Top), new Point(crtOverlapedRect.Right, crtOverlapedRect.Bottom));
                                                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                                                rect.Width = crtOverlapedRect.Width;
                                                rect.Height = crtOverlapedRect.Height;
                                                System.Windows.Controls.Canvas.SetLeft(rect, crtOverlapedRect.Left);
                                                System.Windows.Controls.Canvas.SetTop(rect, crtOverlapedRect.Top);
                                                rect.StrokeThickness = 3;
                                                rect.Stroke = MBrushes.Orange;

                                                m_mvvm.p_RefFeatureDrawer.m_ListShape.Add(rect);
                                                m_mvvm.p_RefFeatureDrawer.m_Element.Add(rect);
                                                m_mvvm.p_RefFeatureDrawer.m_ListRect.Add(temp);

                                                m_mvvm.p_ImageViewer.SetRoiRect();
                                            }));
                                        }

                                        int nDefectCode = InspectionManager.MakeDefectCode(InspectionTarget.Chrome, InspectionType.Strip, 0);

                                        engineer.m_InspManager.SetStandardPos(nDefectCode, cptStandard);

                                        MemoryData memory = engineer.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem);
                                        IntPtr p = memory.GetPtr(0);
                                        engineer.m_InspManager.CreateInspArea(App.sPatternPool, App.sPatternGroup, App.sPatternmem, engineer.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem).GetMBOffset(),
                                            engineer.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem).p_sz.X,
                                            engineer.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem).p_sz.Y,
                                            crtOverlapedRect, 1000, roiCurrent.Strip.ParameterList[0], nDefectCode, engineer.m_recipe.VegaRecipeData.UseDefectMerge, engineer.m_recipe.VegaRecipeData.MergeDistance, p);
                                    }
                                }
                            }
                            #endregion
                        }
                        #endregion

                        nScanLine++;
                        cpMemoryOffset_pixel.X += nCamWidth;
                    }
                    //m_grabMode.m_camera.StopGrab();
                    return "OK";
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message + "\n" + e.StackTrace);
                    return e.Message +"\n"+ e.StackTrace;
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
            //------------------------------------------------------
        }
        //------------------------------------------------------
        //public class Run_Grab : ModuleRunBase
        //{
        //    PatternVision m_module;
        //    public Run_Grab(PatternVision module)
        //    {
        //        m_module = module;
        //        InitModuleRun(module);
        //    }

        //    public GrabMode m_grabMode = null;
        //    string _sGrabMode = "";
        //    string p_sGrabMode
        //    {
        //        get { return _sGrabMode; }
        //        set
        //        {
        //            _sGrabMode = value;
        //            m_grabMode = m_module.GetGrabMode(value);
        //        }
        //    }

        //    bool m_bInvDir = false;
        //    public RPoint m_rpAxis = new RPoint();
        //    public double m_fYRes = 1;
        //    public double m_fXRes = 1;
        //    public int m_nFocusPos = 0;
        //    public CPoint m_cpMemory = new CPoint();
        //    public int m_nMaxFrame = 100;  // Camera max Frame 스펙
        //    public int m_nScanRate = 100;   // Camera Frame Spec 사용률 ? 1~100 %
        //    public int m_yLine = 1000;
        //    public override ModuleRunBase Clone()
        //    {
        //        Run_Grab run = new Run_Grab(m_module);
        //        run.p_sGrabMode = p_sGrabMode;
        //        run.m_fYRes = m_fYRes;
        //        run.m_fXRes = m_fXRes;
        //        run.m_bInvDir = m_bInvDir;
        //        run.m_nFocusPos = m_nFocusPos;
        //        run.m_rpAxis = new RPoint(m_rpAxis);
        //        run.m_cpMemory = new CPoint(m_cpMemory);
        //        run.m_yLine = m_yLine;
        //        run.m_nMaxFrame = m_nMaxFrame;
        //        run.m_nScanRate = m_nScanRate;
        //        return run;
        //    }

        //    public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        //    {
        //        m_rpAxis = tree.Set(m_rpAxis, m_rpAxis, "Center Axis Position", "Center Axis Position (mm ?)", bVisible);
        //        m_fYRes = tree.Set(m_fYRes, m_fYRes, "Cam YResolution", "YResolution  um", bVisible);
        //        m_fXRes = tree.Set(m_fXRes, m_fXRes, "Cam XResolution", "XResolution  um", bVisible);
        //        m_nFocusPos = tree.Set(m_nFocusPos, 0, "Focus Z Pos", "Focus Z Pos", bVisible);
        //        m_cpMemory = tree.Set(m_cpMemory, m_cpMemory, "Memory Position", "Grab Start Memory Position (pixel)", bVisible);
        //        m_bInvDir = tree.Set(m_bInvDir, m_bInvDir, "Inverse Direction", "Grab Direction", bVisible);
        //        m_yLine = tree.Set(m_yLine, m_yLine, "WaferSize", "# of Grab Lines", bVisible);
        //        m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
        //        m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
        //        p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
        //        if (m_grabMode != null) m_grabMode.RunTree(tree.GetTree("Grab Mode", false), bVisible, true);
        //    }

        //    public override string Run()
        //    {
        //        if (m_grabMode == null) return "Grab Mode == null";
        //        bool bUseRADS = m_grabMode.GetUseRADS();
        //        try
        //        {
        //            int nScanLine = 0;
        //            m_grabMode.SetLight(true);
        //            if (bUseRADS && (m_grabMode.m_RADSControl.p_IsRun == false))
        //            {
        //                m_grabMode.m_RADSControl.p_IsRun = true;
        //                m_grabMode.m_RADSControl.StartRADS();
        //                m_module.m_CamRADS.GrabContinuousShot();
        //            }
        //            AxisXY axisXY = m_module.p_axisXY;
        //            Axis axisZ = m_module.p_axisZ;
        //            CPoint cpMemory = new CPoint(m_cpMemory);
        //            cpMemory.X += (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X;
        //            m_grabMode.m_dTrigger = 10 * m_fYRes;        // 축해상도 0.1um로 하드코딩.
        //            double XScal = m_fXRes*10;
        //            int nLines = Convert.ToInt32(m_yLine * 1000 / m_fYRes);
        //            while (m_grabMode.m_ScanLineNum > nScanLine)
        //            {
        //                if (EQ.IsStop())
        //                    return "OK";
        //                double yAxis = m_grabMode.m_dTrigger * nLines;     // 총 획득할 Image Y 
        //                /*위에서 아래로 찍는것을 정방향으로 함, 즉 Y 축 값이 큰쪽에서 작은쪽으로 찍는것이 정방향*/
        //                /* Grab하기 위해 이동할 Y축의 시작 끝 점*/
        //                double yPos1 = m_rpAxis.Y - yAxis / 2 - 300000;
        //                double yPos0 = m_rpAxis.Y + yAxis / 2 + 300000;

        //                m_grabMode.m_eGrabDirection = eGrabDirection.Forward;
        //                if (m_grabMode.m_bUseBiDirectionScan && Math.Abs(axisXY.p_axisY.p_posActual - yPos0) > Math.Abs(axisXY.p_axisY.p_posActual - yPos1))
        //                {
        //                    double buffer = yPos0;            //yp1 <--> yp0 바꿈.
        //                    yPos0 = yPos1;
        //                    yPos1 = buffer;
        //                    m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
        //                }

        //                /* 조명 Set하는거 Test해서 넣어야됨.*/
        //                //m_grabMode.SetLight(true);
        //                double nPosX = m_rpAxis.X + nLines * m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X * XScal; //해상도추가필요

        //                if (m_module.Run(axisZ.StartMove(m_nFocusPos)))
        //                    return p_sInfo;
        //                if (m_module.Run(axisXY.StartMove(new RPoint(nPosX, yPos0))))
        //                    return p_sInfo;
        //                if (m_module.Run(axisXY.WaitReady()))
        //                    return p_sInfo;
        //                if (m_module.Run(axisZ.WaitReady()))
        //                    return p_sInfo;

        //                double yTrigger0 = m_rpAxis.Y - yAxis / 2;
        //                double yTrigger1 = m_rpAxis.Y + yAxis / 2;
        //                //m_module.p_axisXY.p_axisY.SetTrigger(yTrigger0 - 100000, yTrigger1, m_grabMode.m_dTrigger, true);
        //                m_module.p_axisXY.p_axisY.SetTrigger(yTrigger0 - 100000, yTrigger1, 10, true);

        //                string sPool = m_grabMode.m_memoryPool.p_id;
        //                string sGroup = m_grabMode.m_memoryGroup.p_id;
        //                string sMem = m_grabMode.m_memoryData.p_id;
        //                MemoryData mem = m_module.m_engineer.GetMemory(sPool, sGroup, sMem);
        //                int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * m_grabMode.m_camera.GetRoiSize().Y * (double)m_nScanRate / 100);

        //                /* 방향 바꾸는 코드 들어가야함*/
        //                m_grabMode.StartGrab(mem, cpMemory, nLines, m_grabMode.m_eGrabDirection == eGrabDirection.BackWard);
        //                if (m_module.Run(axisXY.p_axisY.StartMove(yPos1, nScanSpeed)))
        //                    return p_sInfo;
        //                if (m_module.Run(axisXY.WaitReady()))
        //                    return p_sInfo;
        //                axisXY.p_axisY.RunTrigger(false);

        //                nScanLine++;
        //                cpMemory.X += m_grabMode.m_camera.GetRoiSize().X;

        //                // 1Strip Scan 후 검사

        //            }
        //            m_grabMode.m_camera.StopGrab();
        //            #region Grab1차 Test후에 코드 부분
        //            //double yp0 = m_bInvDir ? m_rpAxis.Y + yAxis + m_grabMode.m_mmAcc : m_rpAxis.Y - m_grabMode.m_mmAcc;
        //            //double yp1 = m_bInvDir ? m_rpAxis.Y - m_grabMode.m_mmAcc : m_rpAxis.Y + yAxis + m_grabMode.m_mmAcc;

        //            //m_grabMode.SetLight(true);

        //            //double yp0 = m_bInvDir ? m_rpAxis.Y + yAxis + m_grabMode.m_mmAcc : m_rpAxis.Y - m_grabMode.m_mmAcc;
        //            //double yp1 = m_bInvDir ? m_rpAxis.Y - m_grabMode.m_mmAcc : m_rpAxis.Y + yAxis + m_grabMode.m_mmAcc;
        //            //if (m_module.Run(axisXY.Move(new RPoint(m_rpAxis.X, yp1))))
        //            //    return p_sInfo;
        //            //if (m_module.Run(axisXY.WaitReady()))
        //            //    return p_sInfo;

        //            //double yTrigger0 = m_bInvDir ? m_rpAxis.Y - m_grabMode.m_mmAcc : m_rpAxis.Y;
        //            //double yTrigger1 = m_bInvDir ? m_rpAxis.Y + yAxis : m_rpAxis.Y + yAxis;
        //            //m_module.m_axisXY.m_axisY.SetTrigger(yTrigger0, yTrigger1, 10, true);
        //            ////m_grabMode.SetTrigger(m_module.m_axisXY.m_axisY, yTrigger0, yTrigger1);
        //            //string sPool = "pool";
        //            //string sGroup = "grou p";
        //            //string sMem = "mem";
        //            //MemoryData mem = m_module.m_engineer.ClassMemoryTool().GetPool(sPool).GetGroup(sGroup).GetMemory(sMem);

        //            ////m_grabMode.StartGrab(m_grabMode.m_memoryData, new CPoint(0, 0), m_yLine, m_bInvDir);
        //            //m_grabMode.StartGrab(mem, m_cpMemory, m_yLine, m_bInvDir);
        //            //if (m_module.Run(axisXY.m_axisY.Move(yp0)))
        //            //    return p_sInfo;
        //            //if (m_module.Run(axisXY.WaitReady()))
        //            //    return p_sInfo;
        //            //axisXY.m_axisY.ResetTrigger();
        //            #endregion
        //            return "OK";
        //        }
        //        finally
        //        {
        //            m_grabMode.SetLight(false);
        //            if (bUseRADS && (m_grabMode.m_RADSControl.p_IsRun == true))
        //            {
        //                m_grabMode.m_RADSControl.p_IsRun = false;
        //                m_grabMode.m_RADSControl.StopRADS();
        //                m_module.m_CamRADS.StopGrab();
        //            }
        //        }
        //    }
        //}

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

        public class Run_AutoIllumination : ModuleRunBase
        {
            PatternVision m_module;
            public _2_5_MainVisionViewModel m_mvvm;
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
            public RPoint m_rpCenterPos = new RPoint();
            public double m_fYRes = 1;
            public double m_fXRes = 1;
            public int m_nFocusPos = 0;
            public int m_nMaxFrame = 100;  // Camera max Frame 스펙
            public int m_nScanRate = 100;   // Camera Frame Spec 사용률 ? 1~100 %
            public int m_yLine = 1000;
            public int m_nThreshold = 128;  // Light Cal 원하는 밝기값
            public int m_nThreshTolereance = 3; // Light Cal 원하는 밝기값 +-허용치

            public Run_AutoIllumination(PatternVision module)
            {
                m_module = module;
                m_mvvm = m_module.m_mvvm;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_AutoIllumination run = new Run_AutoIllumination(m_module);
                run.p_sGrabMode = p_sGrabMode;
                run.m_fYRes = m_fYRes;
                run.m_fXRes = m_fXRes;
                run.m_bInvDir = m_bInvDir;
                run.m_nFocusPos = m_nFocusPos;
                run.m_rpCenterPos = new RPoint(m_rpCenterPos);
                run.m_yLine = m_yLine;
                run.m_nMaxFrame = m_nMaxFrame;
                run.m_nScanRate = m_nScanRate;
                run.m_nThreshold = m_nThreshold;
                run.m_nThreshTolereance = m_nThreshTolereance;
                return run;
            }

            public void RunTree(TreeRoot treeRoot, Tree.eMode mode)
            {
                treeRoot.p_eMode = mode;
                RunTree(treeRoot, true);
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_rpCenterPos = tree.Set(m_rpCenterPos, m_rpCenterPos, "Center Axis Position", "Center Axis Position (mm ?)", bVisible);
                m_fYRes = tree.Set(m_fYRes, m_fYRes, "Cam YResolution", "YResolution  um", bVisible);
                m_fXRes = tree.Set(m_fXRes, m_fXRes, "Cam XResolution", "XResolution  um", bVisible);
                m_nFocusPos = tree.Set(m_nFocusPos, 0, "Focus Z Pos", "Focus Z Pos", bVisible);
                m_bInvDir = tree.Set(m_bInvDir, m_bInvDir, "Inverse Direction", "Grab Direction", bVisible);
                m_yLine = tree.Set(m_yLine, m_yLine, "WaferSize", "# of Grab Lines", bVisible);
                m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
                m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
                p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
                if (m_grabMode != null) m_grabMode.RunTree(tree.GetTree("Grab Mode", false), bVisible, true);
                m_nThreshold = tree.Set(m_nThreshold, m_nThreshold, "Light Cal Threshold", "Light Cal Threshold", bVisible, true);
                m_nThreshTolereance = tree.Set(m_nThreshTolereance, m_nThreshTolereance, "Light Cal Threshod Tolerance", "Light Cal Threshod Tolerance", bVisible);
            }

            public override string Run()
            {
                // 0. Recipe Load 됐는지 체크
                if (!App.m_engineer.m_recipe.Loaded)
                    return "Recipe Not Loaded";

                // 1. Feature와 Light CAL.Key가 Scan되는 위치 Scan
                if (m_grabMode == null) return "Grab Mode == null";
                bool bUseRADS = m_grabMode.GetUseRADS();

                string strPool = m_grabMode.m_memoryPool.p_id;
                string strGroup = m_grabMode.m_memoryGroup.p_id;
                string strMem = m_grabMode.m_memoryData.p_id;
                MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMem);

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
                    CPoint cpMemory = new CPoint();
                    m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_fYRes); // 축해상도 0.1um로 하드코딩.
                    double XScal = m_fXRes * 10;
                    int nLines = Convert.ToInt32(m_yLine * 1000 / m_fYRes);

                    while (m_grabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";
                        double yAxis = m_grabMode.m_dTrigger * nLines; // 총 획득할 Image Y 
                        /*위에서 아래로 찍는것을 정방향으로 함, 즉 Y 축 값이 큰쪽에서 작은쪽으로 찍는것이 정방향*/
                        /* Grab하기 위해 이동할 Y축의 시작 끝 점*/
                        double dScanStartPosY = m_rpCenterPos.Y - yAxis / 2 + 300000;
                        double dScanEndPosY = m_rpCenterPos.Y - yAxis / 2 - 300000;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;
                        if (m_grabMode.m_bUseBiDirectionScan && Math.Abs(axisXY.p_axisY.p_posActual - dScanStartPosY) > Math.Abs(axisXY.p_axisY.p_posActual - dScanEndPosY))
                        {
                            double buffer = dScanStartPosY;
                            dScanStartPosY = dScanEndPosY;
                            dScanEndPosY = buffer;
                            m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                        }

                        double nPosX = m_rpCenterPos.X + nLines * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X * XScal; //해상도추가필요

                        if (m_module.Run(axisZ.StartMove(m_nFocusPos)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.StartMove(new RPoint(nPosX, dScanStartPosY))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;

                        double dTrigStartPosY = m_rpCenterPos.Y - yAxis / 2;
                        double dTrigEndPosY = m_rpCenterPos.Y + yAxis / 2;
                        m_module.p_axisXY.p_axisY.SetTrigger(dTrigStartPosY - 100000, dTrigEndPosY, m_grabMode.m_dTrigger, true);

                        int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * m_grabMode.m_camera.GetRoiSize().Y * (double)m_nScanRate / 100);

                        /* 방향 바꾸는 코드 들어가야함*/
                        m_grabMode.StartGrab(mem, cpMemory, nLines, m_grabMode.m_eGrabDirection == eGrabDirection.BackWard);
                        if (m_module.Run(axisXY.p_axisY.StartMove(dScanEndPosY, nScanSpeed)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        axisXY.p_axisY.RunTrigger(false);

                        nScanLine++;
                        cpMemory.X += m_grabMode.m_camera.GetRoiSize().X;
                    }
                    m_grabMode.m_camera.StopGrab();
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

                // 2. Feature 탐색
                CPoint cpStandardPos = new CPoint(0, 0);
                int nRefStartOffsetX = 0;
                int nRefStartOffsetY = 0;
                Point ptStartPos = new Point();
                Point ptEndPos = new Point();
                for (int k = 0; k < m_mvvm.p_PatternRoiList.Count; k++)
                {
                    var currentRoi = m_mvvm.p_PatternRoiList[k];
                    for (int j = 0; j<currentRoi.Strip.ParameterList.Count; j++)
                    {
                        foreach (var feature in currentRoi.Position.ReferenceList)
                        {
                            bool bFoundFeature = false;
                            CRect rtTargetRect;
                            Point ptMaxRelativePoint;
                            int nWidthDiff, nHeightDiff;
                            //foundFeature = FindFeature(feature, out targetRect, out maxRelativePoint, out widthDiff, out heightDiff);
                            bFoundFeature = m_mvvm.FindFeature(feature, out rtTargetRect, out ptMaxRelativePoint, out nWidthDiff, out nHeightDiff);

                            if (bFoundFeature)
                            {
                                cpStandardPos.X = rtTargetRect.Left + (int)ptMaxRelativePoint.X + nWidthDiff / 2;
                                cpStandardPos.Y = rtTargetRect.Top + (int)ptMaxRelativePoint.Y + nHeightDiff / 2;
                                nRefStartOffsetX = feature.LightCalDistX;
                                nRefStartOffsetY = feature.LightCalDistY;
                                m_mvvm.DrawCross(new DPoint(cpStandardPos.X, cpStandardPos.Y), MBrushes.Red);

                                ptStartPos = new Point(cpStandardPos.X + nRefStartOffsetX, cpStandardPos.Y + nRefStartOffsetY);
                                ptEndPos = new Point(ptStartPos.X + feature.LightCalWidth, ptStartPos.Y + feature.LightCalHeight);

                                break;//찾았으니 중단
                            }
                            else
                            {
                                continue;//못 찾았으면 다음 Feature값으로 이동
                            }
                        }
                    }
                }

                // 3. 탐색된 Feature위치를 기준으로 Light Cal.Key 영역 이미지를 AutoIllumination
                CRect rtLightCal = new CRect(ptStartPos, ptEndPos);
                int nResultThreshold = AutoIllumination(mem, rtLightCal);
                
                return "OK";
            }

            unsafe int AutoIllumination(MemoryData md, CRect rtROI)
            {
                // variable
                int nSum = 0;
                int nResult = 0;

                // implement
                byte* pSrc = (byte*)md.GetPtr(0, rtROI.Left, rtROI.Top).ToPointer();
                for (int i = 0; i<rtROI.Height; i++)
                {
                    byte* pDst = pSrc + (i * md.p_sz.X);
                    for (int j = 0; j < rtROI.Width; j++, pDst++)
                    {
                        nSum += *pDst;
                    }
                }
                nResult = nSum / (rtROI.Width * rtROI.Height);

                return nResult;
            }
        }
        #endregion

        #region VRS Review Image Capture
        public class Run_VRSReviewImagCapture : ModuleRunBase
        {
            PatternVision m_module;
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

            public RPoint m_rpReticleCenterPos = new RPoint();  // Pulse
            public RPoint m_rpDistanceOfTDIToVRS_pulse = new RPoint();      // Pulse
            public double m_dResY_um = 1;                       // um
            public double m_dResX_um = 1;                       // um
            public double m_dReticleSize_mm = 150;              // mm
            
            public Run_VRSReviewImagCapture(PatternVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_VRSReviewImagCapture run = new Run_VRSReviewImagCapture(m_module);
                run.p_sGrabMode = p_sGrabMode;
                run.m_rpReticleCenterPos = m_rpReticleCenterPos;
                run.m_rpDistanceOfTDIToVRS_pulse = m_rpDistanceOfTDIToVRS_pulse;
                run.m_dResY_um = m_dResY_um;
                run.m_dResX_um = m_dResX_um;
                run.m_dReticleSize_mm = m_dReticleSize_mm;
                return run;
            }

            public void RunTree(TreeRoot treeRoot, Tree.eMode mode)
            {
                treeRoot.p_eMode = mode;
                RunTree(treeRoot, true);
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_rpReticleCenterPos = tree.Set(m_rpReticleCenterPos, m_rpReticleCenterPos, "Center Axis Position", "Center Axis Position (Pulse)", bVisible);
                m_rpDistanceOfTDIToVRS_pulse = tree.Set(m_rpDistanceOfTDIToVRS_pulse, m_rpDistanceOfTDIToVRS_pulse, "Distance of TDI Camera to VRS Camera", "Distance of TDI Camera to VRS Camera (Pulse)", bVisible);
                m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Camera X Resolution", "Camera X Resolution (um)", bVisible);
                m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Camera Y Resolution", "Camera Y Resolution (um)", bVisible);
                m_dReticleSize_mm = tree.Set(m_dReticleSize_mm, m_dReticleSize_mm, "Reticle Size", "Reticle Size (mm)", bVisible);
            }

            public override string Run()
            {
                // variable
                AxisXY axisXY = m_module.m_axisXY;
                Axis axisZ = m_module.m_axisZ;
                Camera_Basler cam = m_module.m_CamVRS;
                ImageData img = cam.p_ImageViewer.p_ImageData;
                string strVRSImageDirectoryPath = "D:\\VRSImage\\";
                string strVRSImageFullPath = "";

                // implement
                List<CPoint> lstDefectPos = GetDefectPosList();
                for (int i = 0; i<lstDefectPos.Count; i++)
                {
                    // Defect 위치로 이동
                    RPoint rpDefectPos = GetAxisPosFromMemoryPos(lstDefectPos[i]);
                    if (m_module.Run(axisXY.StartMove(rpDefectPos)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;

                    // VRS 촬영 및 저장
                    string strRet = cam.Grab();
                    strVRSImageFullPath = string.Format(strVRSImageDirectoryPath + "VRSImage_{0}.bmp", i);
                    img.SaveImageSync(strVRSImageFullPath);
                }

                return "OK";
            }

            public List<CPoint> GetDefectPosList()
            {
                // variable
                DBConnector dbConnector = new DBConnector("localhost", "Inspections", "root", "`ati5344");
                List<CPoint> lstDefectPos = new List<CPoint>();

                // implement
                if (dbConnector.Open())
                {
                    DataSet dataSet = dbConnector.GetDataSet("tempdata");

                    foreach (System.Data.DataRow item in dataSet.Tables["tempdata"].Rows)
                    {
                        int posX = Convert.ToInt32(item["PosX"]);
                        int posY = Convert.ToInt32(item["PosY"]);
                        lstDefectPos.Add(new CPoint(posX, posY));
                    }
                }

                return lstDefectPos;
            }

            public RPoint GetAxisPosFromMemoryPos(CPoint cpMemory)
            {
                // variable
                int nMMPerUM = 1000;
                m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                int nReticleYSize_px = Convert.ToInt32(m_dReticleSize_mm * nMMPerUM / m_dResY_um);    // 레티클 영역(150mm -> 150,000um)의 Y픽셀 갯수
                int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nReticleYSize_px);   // 스캔영역 중 레티클 스캔 구간에서 발생할 Trigger 갯수
                double dTriggerStartPosY = m_rpReticleCenterPos.Y + nTotalTriggerCount / 2;
                int nScanLine = cpMemory.X / m_grabMode.m_camera.GetRoiSize().X;
                double dXScale = m_dResX_um * 10;
                double dTriggerStartPosX = m_rpReticleCenterPos.X + nReticleYSize_px * (double)m_grabMode.m_dTrigger / 2 - nScanLine * m_grabMode.m_camera.GetRoiSize().X * dXScale;
                int nSpareX = cpMemory.X % m_grabMode.m_camera.GetRoiSize().X;
                RPoint rpAxis = new RPoint();

                // implement
                rpAxis.X = dTriggerStartPosX + (10 * m_dResX_um * nSpareX) + m_rpDistanceOfTDIToVRS_pulse.X;
                rpAxis.Y = dTriggerStartPosY - (m_grabMode.m_dTrigger * cpMemory.Y) - m_rpDistanceOfTDIToVRS_pulse.Y;

                return rpAxis;
            }
        }
        #endregion 
    }
}
