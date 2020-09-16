using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.Structure;
using RootTools;
using RootTools.ImageProcess;
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static RootTools.Control.Axis;
using System.Windows.Diagnostics;

namespace Root_Vega.Module
{
    public class SideVision : ModuleBase, IRobotChild
    {
        #region ViewModel
        public _2_6_SideViewModel m_sivm;
        public _2_9_BevelViewModel m_bevm;
        #endregion

        #region ToolBox
        public DIO_I m_diSideReticleExistSensor;

        public double m_dMaxScorePosX = 40000;
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
        Axis m_axisTheta;
        public Axis p_axisTheta
        {
            get
            {
                return m_axisTheta;
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

        public RADSControl m_RADSControl;
        MemoryPool m_memoryPool;
        InspectTool m_inspectTool;
        Camera_Dalsa m_CamSide;
        public Camera_Dalsa p_CamSide
        {
            get
            {
                return m_CamSide;
            }
        }
        Camera_Dalsa m_CamBevel;
        public Camera_Dalsa p_CamBevel
        {
            get
            {
                return m_CamBevel;
            }
        }
        Camera_Basler m_CamSideVRS;
        public Camera_Basler p_CamSideVRS
        {
            get
            {
                return m_CamSideVRS;
            }
        }
        Camera_Basler m_CamBevelVRS;
        public Camera_Basler p_CamBevelVRS
        {
            get
            {
                return m_CamBevelVRS;
            }
        }
        Camera_Basler m_CamAlign1;
        public Camera_Basler p_CamAlign1
        {
            get
            {
                return m_CamAlign1;
            }
        }
        Camera_Basler m_CamAlign2;
        public Camera_Basler p_CamAlign2
        {
            get
            {
                return m_CamAlign2;
            }
        }
        Camera_Basler m_CamLADS;
        public Camera_Basler p_CamLADS
        {
            get
            {
                return m_CamLADS;
            }
        }

        #region GrabMode
        int m_lGrabMode = 0;
        public ObservableCollection<GrabMode> m_aGrabMode = new ObservableCollection<GrabMode>();
        public List<string> p_asGrabMode
        {
            get
            {
                List<string> asGrabMode = new List<string>();
                foreach (GrabMode grabMode in m_aGrabMode)
                    asGrabMode.Add(grabMode.p_sName);
                return asGrabMode;
            }
        }
        public GrabMode GetGrabMode(string sGrabMode)
        {
            foreach (GrabMode grabMode in m_aGrabMode)
            {
                if (sGrabMode == grabMode.p_sName)
                    return grabMode;
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
            while (m_aGrabMode.Count > m_lGrabMode)
                m_aGrabMode.RemoveAt(m_aGrabMode.Count - 1);
            foreach (GrabMode grabMode in m_aGrabMode)
                grabMode.RunTreeName(tree.GetTree("Name", false));
            foreach (GrabMode grabMode in m_aGrabMode)
                grabMode.RunTree(tree.GetTree(grabMode.p_sName, false), true, false);
        }
        #endregion

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisXY, this, "AxisXY");
            p_sInfo = m_toolBox.Get(ref m_axisZ, this, "AxisZ");
            p_sInfo = m_toolBox.Get(ref m_axisTheta, this, "AxisTheta");
            p_sInfo = m_toolBox.Get(ref m_axisClamp, this, "AxisClamp");
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            //p_sInfo = m_toolBox.Get(ref m_RADSControl, this, "RADSControl");
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            p_sInfo = m_toolBox.Get(ref m_inspectTool, this);
            p_sInfo = m_toolBox.Get(ref m_CamBevel, this, "Bevel Main");
            p_sInfo = m_toolBox.Get(ref m_CamSide, this, "Side Main");
            p_sInfo = m_toolBox.Get(ref m_CamLADS, this, "LADS");
            p_sInfo = m_toolBox.Get(ref m_CamBevelVRS, this, "Bevel VRS");
            p_sInfo = m_toolBox.Get(ref m_CamSideVRS, this, "Side VRS");
            p_sInfo = m_toolBox.Get(ref m_CamAlign1, this, "Side_Align1");
            p_sInfo = m_toolBox.Get(ref m_CamAlign2, this, "Side_Align2");

            p_sInfo = m_toolBox.Get(ref m_diSideReticleExistSensor, this, "Side Reticle Sensor");

            if (bInit) m_inspectTool.OnInspectDone += M_inspectTool_OnInspectDone;
        }

        private void M_inspectTool_OnInspectDone(InspectTool.Data data)
        {
            p_sInfo = data.p_sInfo;
        }
        #endregion

        #region DIO Function
        public bool p_bStageVac { get; set; }

        void RunTreeDIODelay(Tree tree)
        {

        }
        #endregion

        #region Axis Function
        public enum eAxisPosTheta
        {
            Ready,
            Snap,
            Align,
        }
        public enum eAxisPosZ
        {
            Safety,
            Grab,
            Ready,
            Align,
            LADS,
        }

        public enum eAxisPosX
        {
            Safety,
            Grab,
            Ready,
            Align,
        }

        public enum eAxisPosY
        {
            Safety,
            Grab,
            Ready,
            Align,
        }

        public enum eAxisPosClamp
        {
            Open,
            Close,
            Safety,
            Home,
        }

        void InitPosAlign()
        {
            m_axisTheta.AddPos(Enum.GetNames(typeof(eAxisPosTheta)));
            m_axisZ.AddPos(Enum.GetNames(typeof(eAxisPosZ)));
            m_axisClamp.AddPos(Enum.GetNames(typeof(eAxisPosClamp)));

            if (m_axisXY.p_axisX != null) ((AjinAxis)m_axisXY.p_axisX).AddPos(Enum.GetNames(typeof(eAxisPosX)));
            if (m_axisXY.p_axisX != null) ((AjinAxis)m_axisXY.p_axisY).AddPos(Enum.GetNames(typeof(eAxisPosY)));
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
            //m_CamAlign1.FunctionConnect();
            m_CamAlign2.FunctionConnect();

            // 레티클 유무체크 촬영위치 이동
            if (Run(m_axisXY.p_axisX.StartMove(eAxisPosX.Safety))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            if (Run(m_axisXY.p_axisY.StartMove(eAxisPosY.Align))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            if (Run(m_axisZ.StartMove(eAxisPosZ.Align))) return p_sInfo;
            if (Run(m_axisZ.WaitReady())) return p_sInfo;

            //if (Run(m_axisTheta.StartMove(eAxisPosTheta.Align))) return p_sInfo;
            if (Run(m_axisTheta.StartHome())) return p_sInfo;
            if (Run(m_axisTheta.WaitReady())) return p_sInfo;

            if (Run(m_axisXY.p_axisX.StartMove(eAxisPosX.Align))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            // 레티클 유무 체크
            string strLightName = "Align1_1";
            //SetLightByName(strLightName, 10);
            //Thread.Sleep(100);
            //m_CamAlign1.GrabOneShot();
            //Thread.Sleep(100);
            //SetLightByName(strLightName, 0);

            strLightName = "Align2_1";
            SetLightByName(strLightName, 60);
            Thread.Sleep(100);
            m_CamAlign2.GrabOneShot();
            Thread.Sleep(100);
            SetLightByName(strLightName, 0);

            //bool bRet = ReticleExistCheck(m_CamAlign1);
            //if (bRet == false) return "Reticle Not Exist";
            //bool bRet = ReticleExistCheck(m_CamAlign2);
            //if (bRet == false) return "Reticle Not Exist";

            if (m_diSideReticleExistSensor.p_bIn == false) return "Reticle Not Exist";

            // 모든 축 Ready 위치로 이동
            if (Run(m_axisXY.p_axisX.StartMove(eAxisPosX.Safety))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            if (Run(m_axisXY.p_axisY.StartMove(eAxisPosY.Ready))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            if (Run(m_axisZ.StartMove(eAxisPosZ.Ready))) return p_sInfo;
            if (Run(m_axisZ.WaitReady())) return p_sInfo;

            if (Run(m_axisTheta.StartMove(eAxisPosTheta.Ready))) return p_sInfo;
            if (Run(m_axisTheta.WaitReady())) return p_sInfo;

            if (Run(m_axisXY.p_axisX.StartMove(eAxisPosX.Ready))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            // Clamp 축 열기
            if (Run(m_axisClamp.StartMove(eAxisPosClamp.Open))) return p_sInfo;
            if (Run(m_axisClamp.WaitReady())) return p_sInfo;

            if (p_infoReticle == null) return p_id + " BeforeGet : InfoReticle = null";
            return CheckGetPut();
        }

        public string BeforePut()
        {
            //m_CamAlign1.FunctionConnect();
            m_CamAlign2.FunctionConnect();

            // 레티클 유무체크 촬영위치 이동
            if (Run(m_axisXY.p_axisX.StartMove(eAxisPosX.Safety))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            if (Run(m_axisXY.p_axisY.StartMove(eAxisPosY.Align))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            if (Run(m_axisZ.StartMove(eAxisPosZ.Align))) return p_sInfo;
            if (Run(m_axisZ.WaitReady())) return p_sInfo;

            //if (Run(m_axisTheta.StartMove(eAxisPosTheta.Align))) return p_sInfo;
            if (Run(m_axisTheta.StartHome())) return p_sInfo;
            if (Run(m_axisTheta.WaitReady())) return p_sInfo;

            if (Run(m_axisXY.p_axisX.StartMove(eAxisPosX.Align))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            // 레티클 유무 체크
            string strLightName = "Align1_1";
            //SetLightByName(strLightName, 10);
            //Thread.Sleep(100);
            //m_CamAlign1.GrabOneShot();
            //Thread.Sleep(100);
            //SetLightByName(strLightName, 0);

            strLightName = "Align2_1";
            SetLightByName(strLightName, 60);
            Thread.Sleep(100);
            m_CamAlign2.GrabOneShot();
            Thread.Sleep(100);
            SetLightByName(strLightName, 0);

            //bool bRet = ReticleExistCheck(m_CamAlign1);
            //if (bRet == false) return "Reticle Not Exist";
            //bool bRet = ReticleExistCheck(m_CamAlign2);
            //if (bRet == false) return "Reticle Not Exist";

            if (m_diSideReticleExistSensor.p_bIn == true) return "Reticle Exist";

            // 모든 축 Ready 위치로 이동
            if (Run(m_axisXY.p_axisX.StartMove(eAxisPosX.Safety))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            if (Run(m_axisXY.p_axisY.StartMove(eAxisPosY.Ready))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            if (Run(m_axisZ.StartMove(eAxisPosZ.Ready))) return p_sInfo;
            if (Run(m_axisZ.WaitReady())) return p_sInfo;

            if (Run(m_axisTheta.StartMove(eAxisPosTheta.Ready))) return p_sInfo;
            if (Run(m_axisTheta.WaitReady())) return p_sInfo;

            if (Run(m_axisXY.p_axisX.StartMove(eAxisPosX.Ready))) return p_sInfo;
            if (Run(m_axisXY.WaitReady())) return p_sInfo;

            // Clamp 축 열기
            if (Run(m_axisClamp.StartMove(eAxisPosClamp.Open))) return p_sInfo;
            if (Run(m_axisClamp.WaitReady())) return p_sInfo;

            if (p_infoReticle != null) return p_id + " BeforePut : InfoReticle != null";
            return CheckGetPut();
        }

        public string AfterGet()
        {
            // Clamp 축 Home
            if (Run(m_axisClamp.StartMove(eAxisPosClamp.Home))) return p_sInfo;
            if (Run(m_axisClamp.WaitReady())) return p_sInfo;

            return CheckGetPut();
        }

        public string AfterPut()
        {
            // Clamp 축 Home
            if (Run(m_axisClamp.StartMove(eAxisPosClamp.Home))) return p_sInfo;
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
            CvInvoke.Threshold(matSrc, matBinary, /*p_nThreshold*/180, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

            // Blob Detection
            blobs = new CvBlobs();
            blobDetector = new CvBlobDetector();
            imgSrc = matBinary.ToImage<Gray, Byte>();
            blobDetector.Detect(imgSrc, blobs);

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
            if (m_diSideReticleExistSensor.p_bIn == false) p_infoReticle = null;
            else p_infoReticle = m_engineer.ClassHandler().GetGemSlot(m_sInfoReticle);
        }
        #endregion

        #region Inspect
        int m_lMaxGrab = 3000;
        CPoint m_szAlignROI = new CPoint();
        MemoryData m_memoryGrab;
        MemoryData m_memoryHeight;
        MemoryData m_memoryBright;

        MemoryData m_memorySideTop;
        MemoryData m_memorySideLeft;
        MemoryData m_memorySideRight;
        MemoryData m_memorySideBottom;
        MemoryData m_memoryBevelTop;
        MemoryData m_memoryBevelLeft;
        MemoryData m_memoryBevelRight;
        MemoryData m_memoryBevelBottom;

        public static ushort[] m_aHeight;
        double m_fScaleH = 0;
        public override void InitMemorys()
        {
            m_szAlignROI = p_CamLADS.p_szROI;
            m_memoryGrab = m_memoryPool.GetGroup("LADS").CreateMemory("Grab", 1, 1, 640, 480 * 1000);
            m_memoryHeight = m_memoryPool.GetGroup("LADS").CreateMemory("Height", 1, 1, 640, 480 * 1000);
            m_memoryBright = m_memoryPool.GetGroup("LADS").CreateMemory("Bright", 1, 1, 640, 480 * 1000);

            m_memorySideTop = m_memoryPool.GetGroup("Grab").CreateMemory("SideTop", 1, 1, 1000, 1000);
            m_memorySideLeft = m_memoryPool.GetGroup("Grab").CreateMemory("SideLeft", 1, 1, 1000, 1000);
            m_memorySideRight = m_memoryPool.GetGroup("Grab").CreateMemory("SideRight", 1, 1, 1000, 1000);
            m_memorySideBottom = m_memoryPool.GetGroup("Grab").CreateMemory("SideBottom", 1, 1, 1000, 1000);

            m_memoryBevelTop = m_memoryPool.GetGroup("Grab").CreateMemory("BevelTop", 1, 1, 1000, 1000);
            m_memoryBevelLeft = m_memoryPool.GetGroup("Grab").CreateMemory("BevelLeft", 1, 1, 1000, 1000);
            m_memoryBevelRight = m_memoryPool.GetGroup("Grab").CreateMemory("BevelRight", 1, 1, 1000, 1000);
            m_memoryBevelBottom = m_memoryPool.GetGroup("Grab").CreateMemory("BevelBottom", 1, 1, 1000, 1000);

            m_aHeight = new ushort[m_szAlignROI.X * m_lMaxGrab];
            m_fScaleH = 65535.0 / m_szAlignROI.Y;
        }

        bool m_bThreadInspect3D = false;
        Thread m_threadInspect3D;
        void InitThreadInspect()
        {
            m_threadInspect3D = new Thread(new ThreadStart(RunThreadInspect3D));
            m_threadInspect3D.Start();
        }

        Queue<int> m_qInspect = new Queue<int>();
        void RunThreadInspect3D()
        {
            m_bThreadInspect3D = true;
            Thread.Sleep(3000);
            while (m_bThreadInspect3D)
            {
                Thread.Sleep(10);
                while (m_qInspect.Count > 0)
                {
                    try { RunThreadInspect(m_qInspect.Dequeue()); }
                    catch (Exception) { m_qInspect.Clear(); }
                }
            }
        }

        unsafe void RunThreadInspect(int iInspect)
        {
            byte* pSrc = (byte*)m_memoryGrab.GetPtr(iInspect).ToPointer();
            byte* pHeight = (byte*)m_memoryHeight.GetPtr(0, 0, iInspect).ToPointer();
            byte* pBright = (byte*)m_memoryBright.GetPtr(0, 0, iInspect).ToPointer();
            for (int x = 0; x < m_szAlignROI.X; x++, pSrc++, pHeight++, pBright++)
            {
                byte* pSrcY = pSrc;
                int nSum = 0;
                int nYSum = 0;
                for (int y = 0; y < m_szAlignROI.Y; y++, pSrcY += m_szAlignROI.X)
                {
                    nSum += *pSrcY;
                    nYSum += *pSrcY * y;
                }
                int nAdd = x + iInspect * m_szAlignROI.X;
                m_aHeight[nAdd] = (nSum != 0) ? (ushort)(m_fScaleH * nYSum / nSum) : (ushort)0;
                *pHeight = (byte)(m_aHeight[nAdd] >> 8);
                int yAve = (nSum != 0) ? (int)Math.Round(1.0 * nYSum / nSum) : 0;
                *pBright = pSrc[x + yAve * m_szAlignROI.X];
            }
        }

        void StartInspect(int iInspect)
        {
            m_qInspect.Enqueue(iInspect);
        }


        void RunTreeInspect(Tree tree)
        {
            m_lMaxGrab = tree.Set(m_lMaxGrab, m_lMaxGrab, "Max Grab", "Max Grab Count for Memory Allocate");
        }
        #endregion

        #region AutoFocus
        AutoFocus m_AutoFocusModule;
        public AutoFocus p_AutoFocus
        {
            get
            {
                return m_AutoFocusModule;
            }
        }
        void InitAutoFocus()
        {
            m_AutoFocusModule = new AutoFocus();
            return;
        }
        #endregion

        #region override Function
        public override string StateHome()
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
                return "Error";

            m_axisXY.p_axisX.StartHome();
            if (m_axisXY.p_axisX.WaitReady() != "OK")
                p_bStageVac = false;

            m_axisXY.p_axisX.StartMove(-50000);
            if (m_axisXY.p_axisX.WaitReady() != "OK")
                return "Error";
            m_axisXY.p_axisY.StartHome();
            m_axisZ.StartHome();
            m_axisTheta.StartHome();
            if (m_axisZ.WaitReady() != "OK")
                return "Error";
            if (m_axisTheta.WaitReady() != "OK")
                return "Error";
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
            RunTreeInspect(tree.GetTree("Inspect", false));
        }
        #endregion

        public SideVision(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
            InitThreadInspect();
            InitPosAlign();
            InitAutoFocus();
        }

        public override void ThreadStop()
        {
            if (m_bThreadInspect3D)
            {
                m_qInspect.Clear();
                m_bThreadInspect3D = false;
                m_threadInspect3D.Join();
            }
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
            //            AddModuleRunList(new Run_Inspect(this), true, "3D Inspect");
            AddModuleRunList(new Run_Run(this), true, "Run Side Vision");
            AddModuleRunList(new Run_SideGrab(this), true, "Side Grab");
            AddModuleRunList(new Run_BevelGrab(this), true, "Bevel Grab");
            AddModuleRunList(new Run_AutoFocus(this), true, "Auto Focus");
            AddModuleRunList(new Run_LADS(this), true, "LADS");
            AddModuleRunList(new Run_Inspection(this), true, "Inspection");
        }

        public class Run_Delay : ModuleRunBase
        {
            SideVision m_module;
            public Run_Delay(SideVision module)
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
                m_module.m_gem.STSSetProcessing(m_module.p_infoReticle, RootTools.Gem.GemSlotBase.eSTSProcess.Processed);
                return "OK";
            }
        }

        public class Run_Run : ModuleRunBase
        {
            SideVision m_module;
            public Run_Run(SideVision module)
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
                if (EQ.p_bSimulate) Thread.Sleep(100);
                else
                {
                    //forget SideVision ModuleRun_Run
                }
                m_module.m_gem.STSSetProcessing(m_module.p_infoReticle, RootTools.Gem.GemSlotBase.eSTSProcess.Processed);
                return "OK";
            }
        }

        public class Run_SideGrab : ModuleRunBase
        {
            SideVision m_module;
            public Run_SideGrab(SideVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public GrabMode m_grabMode = null;
            string _sGrabMode = "";
            string p_sGrabMode
            {
                get
                {
                    return _sGrabMode;
                }
                set
                {
                    _sGrabMode = value;
                    //m_grabMode = m_module.GetGrabMode(value);
                }
            }

            public RPoint m_rpAxis = new RPoint();
            public double m_fRes = 1;       //단위 um
            public int m_nFocusPos = 0;
            public CPoint m_cpMemory = new CPoint();
            public int m_nScanGap = 1000;
            public int m_yLine = 1000;  // Y축 Reticle Size
            public int m_xLine = 1000;  // X축 Reticle Size
            public int m_nMaxFrame = 100;  // Camera max Frame 스펙
            public int m_nScanRate = 100;   // Camera Frame Spec 사용률 ? 1~100 %
            public double m_dThetaOffset = 0.0;
            public double m_dXOffset = 0.0;

            //public eScanPos m_eScanPos = eScanPos.Bottom;
            public override ModuleRunBase Clone()
            {
                Run_SideGrab run = new Run_SideGrab(m_module);
                run.p_sGrabMode = p_sGrabMode;
                run.m_fRes = m_fRes;
                run.m_nFocusPos = m_nFocusPos;
                run.m_rpAxis = new RPoint(m_rpAxis);
                run.m_cpMemory = new CPoint(m_cpMemory);
                run.m_yLine = m_yLine;
                run.m_xLine = m_xLine;
                run.m_nMaxFrame = m_nMaxFrame;
                run.m_nScanGap = m_nScanGap;
                run.m_dThetaOffset = m_dThetaOffset;
                run.m_dXOffset = m_dXOffset;
                run.m_grabMode = m_module.GetGrabMode(p_sGrabMode);

                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_rpAxis = tree.Set(m_rpAxis, m_rpAxis, "Center Axis Position", "Center Axis Position (mm ?)", bVisible);
                m_fRes = tree.Set(m_fRes, m_fRes, "Cam Resolution", "Resolution  um", bVisible);
                m_nFocusPos = tree.Set(m_nFocusPos, 0, "Focus Z Pos", "Focus Z Pos", bVisible);
                m_cpMemory = tree.Set(m_cpMemory, m_cpMemory, "Memory Position", "Grab Start Memory Position (pixel)", bVisible);
                m_nScanGap = tree.Set(m_nScanGap, m_nScanGap, "Scan Gab", "Scan 방향간의 Memory 상 Gab (Bottom, Left 간의 Memory 위치 차이)", bVisible);
                m_yLine = tree.Set(m_yLine, m_yLine, "Reticle YSize", "# of Grab Lines", bVisible); // 120
                m_xLine = tree.Set(m_xLine, m_xLine, "Reticle XSize", "# of Grab Lines", bVisible); // 120
                m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
                m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
                m_dThetaOffset = tree.Set(m_dThetaOffset, m_dThetaOffset, "Theta Offset", "Theta Offset", bVisible);
                m_dXOffset = tree.Set(m_dXOffset, m_dXOffset, "X Offset", "X Offset", bVisible);

                string strTemp = p_sGrabMode;
                p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
                if (strTemp != p_sGrabMode)
                {
                    m_grabMode = m_module.GetGrabMode(p_sGrabMode);
                }
            }

            public override string Run()
            {
                if (m_grabMode == null)
                    return "Grab Mode == null";

                AxisXY axisXY = m_module.p_axisXY;
                Axis axisZ = m_module.p_axisZ;
                Axis axisTheta = m_module.p_axisTheta;

                try
                {
                    int nScanLine = 0;
                    m_grabMode.SetLight(true);
                    m_grabMode.SetLightByName("Side Coax", 700);
                    m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_fRes);        // 축해상도 0.1um로 하드코딩. 트리거 발생 주기.
                    int nLinesY = Convert.ToInt32(m_yLine * 1000 / m_fRes);      // Grab 할 총 Line 갯수.
                    int nLinesX = Convert.ToInt32(m_xLine * 1000 / m_fRes);      // Grab 할 총 Line 갯수.
                    m_cpMemory.X += (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X;
                    //m_cpMemory.X += (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X + (int)m_eScanPos * (nLinesX + m_nScanGap);

                    while (m_grabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";

                        double yAxis = m_grabMode.m_dTrigger * nLinesY;     // 총 획득할 Image Y  
                        /*왼쪽에서 오른쪽으로 찍는것을 정방향으로 함, 즉 Y 축 값이 큰쪽에서 작은쪽으로 찍는것이 정방향*/
                        /* Grab하기 위해 이동할 Y축의 시작 끝 점*/
                        double yPos1 = m_rpAxis.Y - yAxis / 2 - m_grabMode.m_intervalAcc;   //y 축 이동 시작 지점 
                        double yPos0 = m_rpAxis.Y + yAxis / 2 + m_grabMode.m_intervalAcc;  // Y 축 이동 끝 지점.
                        //double nPosX = m_rpAxis.X;   // X축 찍을 위치 
                        double nPosX = m_module.m_dMaxScorePosX + m_dXOffset;   // AF로 찾은 포커스 맞는 X위치
                        double nPosZ = m_nFocusPos + nLinesX * m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X * m_grabMode.m_dTrigger; //해상도추가필요
                        //double nPosZ = m_nFocusPos;
                        //double nPosX = m_rpAxis.X + nLines * m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X * m_grabMode.m_dTrigger; //해상도추가필요
                        double dPosTheta = axisTheta.GetPosValue(eAxisPosTheta.Snap) + (int)m_grabMode.m_eScanPos * 360000 / 4 + m_dThetaOffset;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;

                        if (m_module.Run(axisXY.p_axisX.StartMove(-50000)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.p_axisX.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisTheta.StartMove(dPosTheta)))
                            return p_sInfo;
                        if (m_module.Run(axisTheta.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisZ.StartMove(nPosZ)))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisXY.StartMove(new RPoint(nPosX, yPos0))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        //if (m_module.Run(axisXY.p_axisX.WaitReady()))
                        //    return p_sInfo;
                        //if (m_module.Run(axisXY.p_axisY.WaitReady()))
                        //    return p_sInfo;

                        /* Trigger Set*/
                        double yTrigger0 = m_rpAxis.Y - yAxis / 2;
                        double yTrigger1 = m_rpAxis.Y + yAxis / 2;
                        m_module.p_axisXY.p_axisY.SetTrigger(yPos1, yTrigger1 + 100000, m_grabMode.m_dTrigger, true);

                        string sPool = m_grabMode.m_memoryPool.p_id;
                        string sGroup = m_grabMode.m_memoryGroup.p_id;
                        string sMem = m_grabMode.m_eScanPos.ToString();
                        MemoryData mem = m_module.m_engineer.ClassMemoryTool().GetMemory(sPool, sGroup, "Side" + sMem);

                        int nTest = m_grabMode.m_camera.GetRoiSize().Y;
                        int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * m_grabMode.m_camera.GetRoiSize().Y * (double)m_nScanRate / 100);
                        /* 방향 바꾸는 코드 들어가야함*/
                        m_grabMode.StartGrab(mem, m_cpMemory, nLinesY);
                        if (m_module.Run(axisXY.p_axisY.StartMove(yPos1, nScanSpeed)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.p_axisY.WaitReady()))
                            return p_sInfo;
                        axisXY.p_axisY.RunTrigger(false);

                        nScanLine++;
                        m_cpMemory.X += m_grabMode.m_camera.GetRoiSize().X;
                    }

                    return "OK";
                }
                finally
                {
                    axisXY.p_axisY.RunTrigger(false);
                    m_grabMode.SetLight(false);
                }
            }
        }

        public class Run_BevelGrab : ModuleRunBase
        {
            SideVision m_module;
            public Run_BevelGrab(SideVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public GrabMode m_grabMode = null;
            string _sGrabMode = "";
            string p_sGrabMode
            {
                get
                {
                    return _sGrabMode;
                }
                set
                {
                    _sGrabMode = value;
                    //m_grabMode = m_module.GetGrabMode(value);
                }
            }

            public RPoint m_rpAxis = new RPoint();
            public double m_fRes = 1;       //단위 um
            public int m_nFocusPos = 0;
            public CPoint m_cpMemory = new CPoint();
            public int m_nScanGap = 1000;
            public int m_yLine = 1000;  // Y축 Reticle Size
            public int m_xLine = 1000;  // X축 Reticle Size
            public int m_nMaxFrame = 100;  // Camera max Frame 스펙
            public int m_nScanRate = 100;   // Camera Frame Spec 사용률 ? 1~100 %
            public double m_dThetaOffset = 0.0; // Theta Offset
            public double m_dXOffset = 0.0; // X Offset

            public eScanPos m_eScanPos = eScanPos.Bottom;
            public override ModuleRunBase Clone()
            {
                Run_BevelGrab run = new Run_BevelGrab(m_module);
                run.p_sGrabMode = p_sGrabMode;
                run.m_fRes = m_fRes;
                run.m_nFocusPos = m_nFocusPos;
                run.m_rpAxis = new RPoint(m_rpAxis);
                run.m_cpMemory = new CPoint(m_cpMemory);
                run.m_yLine = m_yLine;
                run.m_xLine = m_xLine;
                run.m_nMaxFrame = m_nMaxFrame;
                run.m_nScanRate = m_nScanRate;
                run.m_nScanGap = m_nScanGap;
                run.m_dThetaOffset = m_dThetaOffset;
                run.m_dXOffset = m_dXOffset;
                run.m_grabMode = m_module.GetGrabMode(p_sGrabMode);

                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_rpAxis = tree.Set(m_rpAxis, m_rpAxis, "Center Axis Position", "Center Axis Position (mm ?)", bVisible);
                m_fRes = tree.Set(m_fRes, m_fRes, "Cam Resolution", "Resolution  um", bVisible);
                m_nFocusPos = tree.Set(m_nFocusPos, 0, "Focus Z Pos", "Focus Z Pos", bVisible);
                m_cpMemory = tree.Set(m_cpMemory, m_cpMemory, "Memory Position", "Grab Start Memory Position (pixel)", bVisible);
                m_nScanGap = tree.Set(m_nScanGap, m_nScanGap, "Scan Gab", "Scan 방향간의 Memory 상 Gab (Bottom, Left 간의 Memory 위치 차이)", bVisible);
                m_yLine = tree.Set(m_yLine, m_yLine, "Reticle YSize", "# of Grab Lines", bVisible);
                m_xLine = tree.Set(m_xLine, m_xLine, "Reticle XSize", "# of Grab Lines", bVisible);
                m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
                m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
                m_dThetaOffset = tree.Set(m_dThetaOffset, m_dThetaOffset, "Theta Offset", "Theta Offset", bVisible);
                m_dXOffset = tree.Set(m_dXOffset, m_dXOffset, "X Offset", "X Offset", bVisible);
                string strTemp = p_sGrabMode;
                p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
                if (strTemp != p_sGrabMode)
                {
                    m_grabMode = m_module.GetGrabMode(p_sGrabMode);
                }

            }

            public override string Run()
            {
                if (m_grabMode == null)
                    return "Grab Mode == null";

                AxisXY axisXY = m_module.p_axisXY;
                Axis axisZ = m_module.p_axisZ;
                Axis axisTheta = m_module.p_axisTheta;

                try
                {
                    int nScanLine = 0;
                    m_grabMode.SetLight(true);
                    m_grabMode.SetLightByName("Bevel Coax", 700);
                    m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_fRes);        // 축해상도 0.1um로 하드코딩. 트리거 발생 주기.
                    int nLinesY = Convert.ToInt32(m_yLine * 1000 / m_fRes);      // Grab 할 총 Line 갯수.
                    int nLinesX = Convert.ToInt32(m_xLine * 1000 / m_fRes);      // Grab 할 총 Line 갯수.
                    m_cpMemory.X += (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X;
                    //m_cpMemory.X += (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X + (int)m_eScanPos * (nLinesX + m_nScanGap);


                    while (m_grabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";

                        double yAxis = m_grabMode.m_dTrigger * nLinesY;     // 총 획득할 Image Y  
                        /*왼쪽에서 오른쪽으로 찍는것을 정방향으로 함, 즉 Y 축 값이 큰쪽에서 작은쪽으로 찍는것이 정방향*/
                        /* Grab하기 위해 이동할 Y축의 시작 끝 점*/
                        double yPos1 = m_rpAxis.Y - yAxis / 2 - m_grabMode.m_intervalAcc;   //y 축 이동 시작 지점 
                        double yPos0 = m_rpAxis.Y + yAxis / 2 + m_grabMode.m_intervalAcc;  // Y 축 이동 끝 지점.
                        double nPosX = m_rpAxis.X;   // X축 찍을 위치 
                        //double nPosX = m_module.m_dMaxScorePosX + m_dXOffset;   // AF로 찾은 포커스 맞는 X위치
                        //double nPosZ = m_nFocusPos + nLinesX * m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X * m_grabMode.m_dTrigger; //해상도추가필요
                        double nPosZ = m_nFocusPos;
                        //double nPosX = m_rpAxis.X + nLines * m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X * m_grabMode.m_dTrigger; //해상도추가필요
                        double dPosTheta = axisTheta.GetPosValue(eAxisPosTheta.Snap) + (int)m_grabMode.m_eScanPos * 360000 / 4 + m_dThetaOffset;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;

                        if (m_module.Run(axisXY.p_axisX.StartMove(-50000)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.p_axisX.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisTheta.StartMove(dPosTheta)))
                            return p_sInfo;
                        if (m_module.Run(axisTheta.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisZ.StartMove(nPosZ)))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisXY.StartMove(new RPoint(nPosX, yPos0))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;

                        /* Trigger Set*/
                        double yTrigger0 = m_rpAxis.Y - yAxis / 2;
                        double yTrigger1 = m_rpAxis.Y + yAxis / 2;

                        m_module.p_axisXY.p_axisY.SetTrigger(yPos1, yTrigger1, m_grabMode.m_dTrigger, true);

                        string sPool = m_grabMode.m_memoryPool.p_id;
                        string sGroup = m_grabMode.m_memoryGroup.p_id;
                        string sMem = m_grabMode.m_eScanPos.ToString();
                        //string sMem = m_eScanPos.ToString();
                        MemoryData mem = m_module.m_engineer.ClassMemoryTool().GetMemory(sPool, sGroup, "Bevel" + sMem);

                        int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * m_grabMode.m_camera.GetRoiSize().Y * (double)m_nScanRate / 100);
                        /* 방향 바꾸는 코드 들어가야함*/
                        m_grabMode.StartGrab(mem, m_cpMemory, nLinesY);
                        if (m_module.Run(axisXY.p_axisY.StartMove(yPos1, nScanSpeed)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.p_axisY.WaitReady()))
                            return p_sInfo;
                        axisXY.p_axisY.RunTrigger(false);

                        nScanLine++;
                        m_cpMemory.X += m_grabMode.m_camera.GetRoiSize().X;
                    }

                    return "OK";
                }
                finally
                {
                    axisXY.p_axisY.RunTrigger(false);
                    m_grabMode.SetLight(false);
                }
            }
        }

        #region CStepInfo
        public class CStepInfo : ObservableObject
        {
            string m_strInfo;
            public string p_strInfo
            {
                get { return m_strInfo; }
                set { SetProperty(ref m_strInfo, value); }
            }
            BitmapSource m_img;
            public BitmapSource p_img
            {
                get { return m_img; }
                set { SetProperty(ref m_img, value); }
            }

            public CStepInfo(string strInfo, BitmapSource img)
            {
                p_strInfo = strInfo;
                p_img = img;
            }
        }
        #endregion
        #region CAutoFocusStatus
        public class CAutoFocusStatus : ObservableObject
        {
            public double m_dTheta;
            public double p_dTheta
            {
                get { return m_dTheta; }
                set { SetProperty(ref m_dTheta, value); }
            }

            public string m_strStatus;
            public string p_strStatus
            {
                get { return m_strStatus; }
                set { SetProperty(ref m_strStatus, value); }
            }

            public CAutoFocusStatus(double dTheta, string strStatus)
            {
                p_dTheta = dTheta;
                p_strStatus = strStatus;
            }
        }
        #endregion

        public class Run_AutoFocus : ModuleRunBase
        {
            public Dispatcher _dispatcher;

            ObservableCollection<CStepInfo> m_lstLeftStepInfo;
            public ObservableCollection<CStepInfo> p_lstLeftStepInfo
            {
                get { return m_lstLeftStepInfo; }
                set { SetProperty(ref m_lstLeftStepInfo, value); }
            }
            ObservableCollection<CStepInfo> m_lstRightStepInfo;
            public ObservableCollection<CStepInfo> p_lstRightStepInfo
            {
                get { return m_lstRightStepInfo; }
                set { SetProperty(ref m_lstRightStepInfo, value); }
            }
            
            CAutoFocusStatus m_afs;
            public CAutoFocusStatus p_afs
            {
                get { return m_afs; }
                set { SetProperty(ref m_afs, value); }
            }
            SideVision m_module;
            public double m_dLeftStartPosX = 0.0;
            public double m_dLeftEndPosX = 0.0;
            public double m_dLeftPosY = 0.0;
            public double m_dLeftPosZ = 0.0;
            public double m_dRightStartPosX = 0.0;
            public double m_dRightEndPosX = 0.0;
            public double m_dRightPosY = 0.0;
            public double m_dRightPosZ = 0.0;
            public int m_nStep = 0;
            public int m_nVarianceSize = 0;
            public bool m_bUsingSobel = true;
            public eScanPos m_eScanPos = eScanPos.Bottom;
            public bool m_bFindXPos = false;

            public Run_AutoFocus(SideVision module)
            {
                m_module = module;
                InitModuleRun(module);
                p_lstLeftStepInfo = new ObservableCollection<CStepInfo>();
                p_lstRightStepInfo = new ObservableCollection<CStepInfo>();
                p_afs = new CAutoFocusStatus(0.0, "Ready");
            }

            public override ModuleRunBase Clone()
            {
                Run_AutoFocus run = new Run_AutoFocus(m_module);
                run.m_dLeftStartPosX = m_dLeftStartPosX;
                run.m_dLeftEndPosX = m_dLeftEndPosX;
                run.m_dLeftPosY = m_dLeftPosY;
                run.m_dLeftPosZ = m_dLeftPosZ;
                run.m_dRightStartPosX = m_dRightStartPosX;
                run.m_dRightEndPosX = m_dRightEndPosX;
                run.m_dRightPosY = m_dRightPosY;
                run.m_dRightPosZ = m_dRightPosZ;
                run.m_nStep = m_nStep;
                run.m_nVarianceSize = m_nVarianceSize;
                run.m_bUsingSobel = m_bUsingSobel;
                run.m_eScanPos = m_eScanPos;
                run.m_bFindXPos = m_bFindXPos;

                run.p_lstLeftStepInfo = p_lstLeftStepInfo;
                run.p_lstRightStepInfo = p_lstRightStepInfo;
                run.p_afs = p_afs;

                return run;
            }

            public void RunTree(TreeRoot treeRoot, Tree.eMode mode)
            {
                treeRoot.p_eMode = mode;
                RunTree(treeRoot, true);
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_dLeftStartPosX = tree.Set(m_dLeftStartPosX, m_dLeftStartPosX, "Left Start X Position", "Left Start X Position", bVisible);
                m_dLeftEndPosX = tree.Set(m_dLeftEndPosX, m_dLeftEndPosX, "Left End X Position", "Left End X Position", bVisible);
                m_dLeftPosY = tree.Set(m_dLeftPosY, m_dLeftPosY, "Left Y Position", "Left Start Y Position", bVisible);
                m_dLeftPosZ = tree.Set(m_dLeftPosZ, m_dLeftPosZ, "Left Z Position", "Left Start Z Position", bVisible);
                m_dRightStartPosX = tree.Set(m_dRightStartPosX, m_dRightStartPosX, "Right Start X Position", "Right Start X Position", bVisible);
                m_dRightEndPosX = tree.Set(m_dRightEndPosX, m_dRightEndPosX, "Right End X Position", "Right End X Position", bVisible);
                m_dRightPosY = tree.Set(m_dRightPosY, m_dRightPosY, "Right Y Position", "Right Start Y Position", bVisible);
                m_dRightPosZ = tree.Set(m_dRightPosZ, m_dRightPosZ, "Right Z Position", "Right Start Z Position", bVisible);
                m_nStep = tree.Set(m_nStep, m_nStep, "AutoFocus Step", "AutoFocus Step", bVisible);
                m_nVarianceSize = tree.Set(m_nVarianceSize, m_nVarianceSize, "Variance Size", "Variance Size", bVisible);
                m_bUsingSobel = tree.Set(m_bUsingSobel, m_bUsingSobel, "Using Sovel Filter", "Using Sovel Filter", bVisible);

                m_eScanPos = (eScanPos)tree.Set(m_eScanPos, m_eScanPos, "Scan 위치", "Scan 위치, 0 Position 이 Bottom", bVisible);
                m_bFindXPos = tree.Set(m_bFindXPos, m_bFindXPos, "Find X Position Again", "Find X Position Again", bVisible);

                base.RunTree(tree, bVisible, bRecipe);
            }

            public override string Run()
            {
                AutoFocus af = m_module.p_AutoFocus;
                Camera_Basler cam = m_module.p_CamSideVRS;
                ImageData img = cam.p_ImageViewer.p_ImageData;
                AxisXY axisXY = m_module.p_axisXY;
                Axis axisZ = m_module.p_axisZ;
                Axis axisTheta = m_module.p_axisTheta;
                double dLeftCurrentScore = 0.0;
                double dLeftMaxScore = -1.0;
                double dLeftMaxScorePosX = m_dLeftStartPosX;
                double dRightCurrentScore = 0.0;
                double dRightMaxScore = -1.0;
                double dRightMaxScorePosX = m_dRightStartPosX;
                double dCenterCurrentScore = 0.0;
                double dCenterMaxScore = -1.0;
                double dCenterMaxScorePosX = m_dRightStartPosX;
                p_afs.p_dTheta = 0.0;
                p_afs.p_strStatus = "Ready";

                if (_dispatcher != null)
                {
                    _dispatcher.Invoke(new Action(delegate ()
                    {
                        p_lstLeftStepInfo.Clear();
                        p_lstRightStepInfo.Clear();
                    }));
                }

                try
                {
                    m_module.SetLightByName("SideVRS Side", 10);

                    // 0. 스캔 포지션으로 Theta 돌리기
                    double dPosTheta = (int)m_eScanPos * 360000 / 4;
                    if (m_module.Run(axisXY.p_axisX.StartMove(-50000)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.p_axisX.WaitReady()))
                        return p_sInfo;
                    if (m_module.Run(axisTheta.StartMove(dPosTheta)))
                        return p_sInfo;
                    if (m_module.Run(axisTheta.WaitReady()))
                        return p_sInfo;

                    //1.Reticle 좌측 위치로 이동 후 AF
                    int nStepCount = (int)Math.Abs(m_dLeftEndPosX - m_dLeftStartPosX) / m_nStep;
                    p_afs.p_strStatus = "Left Side AF...";
                    for (int i = 0; i < nStepCount; i++)
                    {
                        // Axis Move
                        if (m_module.Run(axisXY.StartMove(new RPoint(m_dLeftStartPosX + (m_nStep * i), m_dLeftPosY)))) return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady())) return p_sInfo;
                        if (m_module.Run(axisZ.StartMove(m_dLeftPosZ))) return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady())) return p_sInfo;

                        // Grab
                        string strRet = cam.Grab();

                        // Calculating Score
                        System.Drawing.Bitmap bmp = null;
                        if (m_bUsingSobel) dLeftCurrentScore = af.GetImageFocusScoreWithSobel(img, out bmp);
                        else
                        {
                            dLeftCurrentScore = af.GetImageVarianceScore(img, m_nVarianceSize);
                            bmp = img.GetRectImage(new CRect(0, 0, img.p_Size.X, img.p_Size.Y));
                        }

                        if (_dispatcher != null)
                        {
                            _dispatcher.Invoke(new Action(delegate ()
                            {
                                string strTemp = String.Format("Current Position={0} Current Score={1:N4}", (m_dLeftStartPosX + (m_nStep * i)), dLeftCurrentScore);
                                BitmapSource bmpSrc = GetBitmapSource(bmp);
                                p_lstLeftStepInfo.Add(new CStepInfo(strTemp, bmpSrc));
                            }));
                        }

                        if (dLeftCurrentScore > dLeftMaxScore)
                        {
                            dLeftMaxScore = dLeftCurrentScore;
                            dLeftMaxScorePosX = m_dLeftStartPosX + (m_nStep * i);
                        }
                    }

                    // 2. Reticle 우측 위치로 이동 후 AF
                    nStepCount = (int)Math.Abs(m_dRightEndPosX - m_dRightStartPosX) / m_nStep;
                    p_afs.p_strStatus = "Right Side AF...";
                    for (int i = 0; i < nStepCount; i++)
                    {
                        // Axis Move
                        if (m_module.Run(axisXY.StartMove(new RPoint(m_dRightStartPosX + (m_nStep * i), m_dRightPosY)))) return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady())) return p_sInfo;
                        if (m_module.Run(axisZ.StartMove(m_dRightPosZ))) return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady())) return p_sInfo;

                        // Grab
                        string strRet = cam.Grab();

                        // Calculating Score
                        System.Drawing.Bitmap bmp = null;
                        if (m_bUsingSobel) dRightCurrentScore = af.GetImageFocusScoreWithSobel(img, out bmp);
                        else
                        {
                            dRightCurrentScore = af.GetImageVarianceScore(img, m_nVarianceSize);
                            bmp = img.GetRectImage(new CRect(0, 0, img.p_Size.X, img.p_Size.Y));
                        }

                        if (dRightCurrentScore > dRightMaxScore)
                        {
                            dRightMaxScore = dRightCurrentScore;
                            dRightMaxScorePosX = m_dRightStartPosX + (m_nStep * i);
                        }

                        if (_dispatcher != null)
                        {
                            _dispatcher.Invoke(new Action(delegate ()
                            {
                                string strTemp = String.Format("Current Position={0} Current Score={1:N4}", (m_dRightStartPosX + (m_nStep * i)), dRightCurrentScore);
                                BitmapSource bmpSrc = GetBitmapSource(bmp);
                                p_lstRightStepInfo.Add(new CStepInfo(strTemp, bmpSrc));
                            }));
                        }
                    }

                    // 3. 좌우측 AF편차 구하기
                    bool bThetaClockwise = true;    // Theta+ = Anticlockwise
                                                    // Theta- = Clockwise
                    if (dLeftMaxScorePosX > dRightMaxScorePosX) bThetaClockwise = false;
                    af.p_dDifferenceOfFocusDistance = Math.Abs(dRightMaxScorePosX - dLeftMaxScorePosX);
                    if (m_dRightPosY >= m_dLeftPosY) return "AutoFocus - Right Y Position is bigger than Left Y Position";
                    af.p_dDistanceOfLeftPointToRightPoint = Math.Abs(m_dRightPosY - m_dLeftPosY);

                    // 4. 좌우측 위치 사이의 거리와 좌우측 AF편차를 이용하여 Theta 계산
                    double dThetaRadian = af.GetThetaRadian();
                    double dThetaDegree = af.GetThetaDegree(dThetaRadian);
                    if (bThetaClockwise) p_afs.m_dTheta = -dThetaDegree;
                    else p_afs.p_dTheta = dThetaDegree;

                    // 5. Radian 값을 Theta 모터 포지션 값으로 Scaling
                    double dMinValue = 0.0;
                    double dMaxValue = 2 * Math.PI;
                    double dMinScaleValue = 0.0;
                    double dMaxScaleValue = 360000.0;
                    double dScaled = dMinScaleValue + (Math.Abs(dThetaRadian) - dMinValue) / (dMaxValue - dMinValue) * (dMaxScaleValue - dMinScaleValue);

                    // 6. Theta축 돌리기
                    double dActualPos = m_module.p_axisTheta.p_posActual;
                    if (bThetaClockwise) dScaled = -dScaled;
                    m_module.p_axisTheta.StartMove(dActualPos + dScaled);
                    m_module.p_axisTheta.m_aPos["Snap"] = (int)dScaled;

                    // 7. Theta 돌린 후 X Position 다시 찾는 옵션이 켜져있으면
                    if (m_bFindXPos == true)
                    {
                        // Reticle 중심 위치로 이동 후 AF
                        p_afs.p_strStatus = "Find X Position...";
                        double dCenterPosY = m_dLeftPosY - (af.p_dDistanceOfLeftPointToRightPoint / 2);
                        for (int i = 0; i < nStepCount; i++)
                        {
                            // Axis Move
                            if (m_module.Run(axisXY.StartMove(new RPoint(m_dRightStartPosX + (m_nStep * i), dCenterPosY)))) return p_sInfo;
                            if (m_module.Run(axisXY.WaitReady())) return p_sInfo;
                            if (m_module.Run(axisZ.StartMove(m_dRightPosZ))) return p_sInfo;
                            if (m_module.Run(axisZ.WaitReady())) return p_sInfo;

                            // Grab
                            string strRet = cam.Grab();

                            // Calculating Score
                            System.Drawing.Bitmap bmp = null;
                            if (m_bUsingSobel) dCenterCurrentScore = af.GetImageFocusScoreWithSobel(img, out bmp);
                            else
                            {
                                dCenterCurrentScore = af.GetImageVarianceScore(img, m_nVarianceSize);
                                bmp = img.GetRectImage(new CRect(0, 0, img.p_Size.X, img.p_Size.Y));
                            }

                            if (dCenterCurrentScore > dCenterMaxScore)
                            {
                                dCenterMaxScore = dCenterCurrentScore;
                                dCenterMaxScorePosX = m_dRightStartPosX + (m_nStep * i);
                            }
                        }
                        m_module.m_dMaxScorePosX = dCenterMaxScorePosX;
                    }
                    p_afs.p_strStatus = "Success";
                }
                finally
                {
                    m_module.SetLightByName("SideVRS Side", 0);
                }

                return base.Run();
            }

            private BitmapSource GetBitmapSource(System.Drawing.Bitmap bitmap)
            {
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap
                (
                    bitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );

                return bitmapSource;
            }
        }
        #endregion

        #region Run_LADS
        public class Run_LADS : ModuleRunBase
        {
            public Dispatcher _dispatcher;

            ObservableCollection<CStepInfo> m_lstLeftStepInfo;
            public ObservableCollection<CStepInfo> p_lstLeftStepInfo
            {
                get { return m_lstLeftStepInfo; }
                set { SetProperty(ref m_lstLeftStepInfo, value); }
            }
            ObservableCollection<CStepInfo> m_lstRightStepInfo;
            public ObservableCollection<CStepInfo> p_lstRightStepInfo
            {
                get { return m_lstRightStepInfo; }
                set { SetProperty(ref m_lstRightStepInfo, value); }
            }
            ObservableCollection<CStepInfo> m_lstCenterStepInfo;
            public ObservableCollection<CStepInfo> p_lstCenterStepInfo
            {
                get { return m_lstCenterStepInfo; }
                set { SetProperty(ref m_lstCenterStepInfo, value); }
            }

            BitmapSource m_bmpSrcLeftViewer;
            public BitmapSource p_bmpSrcLeftViewer
            {
                get { return m_bmpSrcLeftViewer; }
                set { SetProperty(ref m_bmpSrcLeftViewer, value); }
            }

            BitmapSource m_bmpSrcRightViewer;
            public BitmapSource p_bmpSrcRightViewer
            {
                get { return m_bmpSrcRightViewer; }
                set { SetProperty(ref m_bmpSrcRightViewer, value); }
            }

            BitmapSource m_bmpSrcCenterViewer;
            public BitmapSource p_bmpSrcCenterViewer
            {
                get { return m_bmpSrcCenterViewer; }
                set { SetProperty(ref m_bmpSrcCenterViewer, value); }
            }

            CAutoFocusStatus m_afs;
            public CAutoFocusStatus p_afs
            {
                get { return m_afs; }
                set { SetProperty(ref m_afs, value); }
            }
            SideVision m_module;

            public RPoint m_rpCenterAxisPos = new RPoint();
            public double m_dRes = 1;       //단위 um
            public double m_dReticleSizeX = 150;    // 단위 mm
            public double m_dInnerOffsetMM = 25;    // 단위 mm

            public double m_dLeftPosZ = 0.0;
            public double m_dRightPosZ = 0.0;
            public eScanPos m_eScanPos = eScanPos.Bottom;
            public bool m_bFindXPos = false;

            public Run_LADS(SideVision module)
            {
                m_module = module;
                InitModuleRun(module);
                p_lstLeftStepInfo = new ObservableCollection<CStepInfo>();
                p_lstRightStepInfo = new ObservableCollection<CStepInfo>();
                p_lstCenterStepInfo = new ObservableCollection<CStepInfo>();
                p_afs = new CAutoFocusStatus(0.0, "Ready");
            }

            public override ModuleRunBase Clone()
            {
                Run_LADS run = new Run_LADS(m_module);
                run.m_rpCenterAxisPos = new RPoint(m_rpCenterAxisPos);
                run.m_dRes = m_dRes;
                run.m_dReticleSizeX = m_dReticleSizeX;
                run.m_dInnerOffsetMM = m_dInnerOffsetMM;

                run.m_dLeftPosZ = m_dLeftPosZ;
                run.m_dRightPosZ = m_dRightPosZ;
                run.m_eScanPos = m_eScanPos;
                run.m_bFindXPos = m_bFindXPos;

                return run;
            }

            public void RunTree(TreeRoot treeRoot, Tree.eMode mode)
            {
                treeRoot.p_eMode = mode;
                RunTree(treeRoot, true);
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_rpCenterAxisPos = tree.Set(m_rpCenterAxisPos, m_rpCenterAxisPos, "Center Axis Position", "Center Axis Position (mm ?)", bVisible);
                m_dRes = tree.Set(m_dRes, m_dRes, "Cam Resolution", "Resolution  um", bVisible);
                m_dReticleSizeX = tree.Set(m_dReticleSizeX, m_dReticleSizeX, "Reticle Size X", "Reticle Size X (mm)", bVisible);
                m_dInnerOffsetMM = tree.Set(m_dInnerOffsetMM, m_dInnerOffsetMM, "Inner Offset X", "Inner Offset X (mm)", bVisible);
                m_dLeftPosZ = tree.Set(m_dLeftPosZ, m_dLeftPosZ, "Left Z Position", "Left Start Z Position", bVisible);
                m_dRightPosZ = tree.Set(m_dRightPosZ, m_dRightPosZ, "Right Z Position", "Right Start Z Position", bVisible);
                m_eScanPos = (eScanPos)tree.Set(m_eScanPos, m_eScanPos, "Scan 위치", "Scan 위치, 0 Position 이 Bottom", bVisible);
                m_bFindXPos = tree.Set(m_bFindXPos, m_bFindXPos, "Find X Position Again", "Find X Position Again", bVisible);
                
                base.RunTree(tree, bVisible, bRecipe);
            }

            public override string Run()
            {
                Camera_Basler cam = m_module.p_CamLADS;
                ImageData img = cam.p_ImageViewer.p_ImageData;
                AxisXY axisXY = m_module.p_axisXY;
                Axis axisZ = m_module.p_axisZ;
                Axis axisTheta = m_module.p_axisTheta;
                p_afs.p_dTheta = 0.0;
                p_afs.p_strStatus = "Ready";
                double dLeftHeight = 0;
                double dRightHeight = 0;
                double dCenterHeight = 0;
                double dFocusHeight = 67;
                int nVerticalPixelPerPulse = 375;
                double dPulsePerMM = 10000;
                double dLeftSnapPosY = m_rpCenterAxisPos.Y + (m_dReticleSizeX / 2 * dPulsePerMM) - (m_dInnerOffsetMM * dPulsePerMM);
                double dRightSnapPosY = m_rpCenterAxisPos.Y - (m_dReticleSizeX / 2 * dPulsePerMM) + (m_dInnerOffsetMM * dPulsePerMM);

                if (_dispatcher != null)
                {
                    _dispatcher.Invoke(new Action(delegate ()
                    {
                        p_lstLeftStepInfo.Clear();
                        p_lstRightStepInfo.Clear();
                        p_lstCenterStepInfo.Clear();
                    }));
                }

                try
                {
                    m_module.SetLightByName("LADS", 50);

                    // 0. 스캔 포지션으로 Theta 돌리기
                    double dPosTheta = (int)m_eScanPos * 360000 / 4;
                    if (m_module.Run(axisXY.p_axisX.StartMove(-50000)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.p_axisX.WaitReady()))
                        return p_sInfo;
                    if (m_module.Run(axisTheta.StartMove(dPosTheta)))
                        return p_sInfo;
                    if (m_module.Run(axisTheta.WaitReady()))
                        return p_sInfo;

                    // 1.Reticle 좌측 위치로 이동
                    p_afs.p_strStatus = "Left Side Snap...";
                    if (m_module.Run(axisXY.StartMove(new RPoint(m_rpCenterAxisPos.X, dLeftSnapPosY)))) return p_sInfo;
                    if (m_module.Run(axisZ.StartMove(m_dLeftPosZ))) return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady())) return p_sInfo;
                    if (m_module.Run(axisZ.WaitReady())) return p_sInfo;
                    string strRet = cam.Grab();
                    dLeftHeight = CalculatingHeight(img);
                    System.Drawing.Bitmap bmpLeft = null;
                    bmpLeft = img.GetRectImage(new CRect(0, 0, img.p_Size.X, img.p_Size.Y));
                    if (_dispatcher != null)
                    {
                        _dispatcher.Invoke(new Action(delegate ()
                        {
                            string strTemp = String.Format("Left Laser Height = {0}", dLeftHeight);
                            BitmapSource bmpSrc = GetBitmapSource(bmpLeft);
                            p_bmpSrcLeftViewer = bmpSrc;
                            p_lstLeftStepInfo.Add(new CStepInfo(strTemp, bmpSrc));
                        }));
                    }

                    // 2. Reticle 우측 위치로 이동
                    p_afs.p_strStatus = "Right Side Snap...";
                    if (m_module.Run(axisXY.StartMove(new RPoint(m_rpCenterAxisPos.X, dRightSnapPosY)))) return p_sInfo;
                    double dPosZ = axisZ.GetPosValue(eAxisPosZ.LADS);
                    if (m_module.Run(axisZ.StartMove(dPosZ))) return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady())) return p_sInfo;
                    if (m_module.Run(axisZ.WaitReady())) return p_sInfo;
                    strRet = cam.Grab();
                    dRightHeight = CalculatingHeight(img);
                    System.Drawing.Bitmap bmpRight = null;
                    bmpRight = img.GetRectImage(new CRect(0, 0, img.p_Size.X, img.p_Size.Y));
                    if (_dispatcher != null)
                    {
                        _dispatcher.Invoke(new Action(delegate ()
                        {
                            string strTemp = String.Format("Right Laser Height = {0}", dRightHeight);
                            BitmapSource bmpSrc = GetBitmapSource(bmpRight);
                            p_bmpSrcRightViewer = bmpSrc;
                            p_lstRightStepInfo.Add(new CStepInfo(strTemp, bmpSrc));
                        }));
                    }

                    // 3. 좌우측 높이차 구하기   - 정확한 Resolution 알아서 다시 구해야 함.
                    int nDiff = (int)dLeftHeight - (int)dRightHeight;

                    return "OK";    // nDiff를 Pulse로 환산 후에 아래 코드 실행해야함.

                    //// 4. Theta 돌리기
                    //double dActualPos = m_module.p_axisTheta.p_posActual;
                    //double dScaled = nDiff * 21.125;
                    //m_module.p_axisTheta.StartMove(dActualPos + dScaled);
                    //m_module.p_axisTheta.m_aPos["Snap"] = (int)dScaled;

                    //// 5. Y축 Center 위치에서 Laser의 높이가 67(LineScan카메라 Focus가 맞는 위치)에 맞도록 X위치 찾기
                    //p_afs.p_strStatus = "Center Snap...";
                    //if (m_module.Run(axisXY.StartMove(new RPoint(m_rpCenterAxisPos.X, m_rpCenterAxisPos.Y)))) return p_sInfo;
                    //if (m_module.Run(axisXY.WaitReady())) return p_sInfo;
                    //strRet = cam.Grab();
                    //dCenterHeight = CalculatingHeight(img);
                    //System.Drawing.Bitmap bmpCenter = null;
                    //bmpCenter = img.GetRectImage(new CRect(0, 0, img.p_Size.X, img.p_Size.Y));
                    //if (_dispatcher != null)
                    //{
                    //    _dispatcher.Invoke(new Action(delegate ()
                    //    {
                    //        string strTemp = String.Format("Center Laser Height = {0}", dCenterHeight);
                    //        BitmapSource bmpSrc = GetBitmapSource(bmpCenter);
                    //        p_bmpSrcCenterViewer = bmpSrc;
                    //        p_lstCenterStepInfo.Add(new CStepInfo(strTemp, bmpSrc));
                    //    }));
                    //}
                    //m_module.m_dMaxScorePosX = m_rpCenterAxisPos.X + (dCenterHeight - dFocusHeight) * nVerticalPixelPerPulse;

                    //// 6. 찾은 위치로 이동해서 한번 더 촬영
                    //if (m_module.Run(axisXY.StartMove(new RPoint(m_module.m_dMaxScorePosX, m_rpCenterAxisPos.Y)))) return p_sInfo;
                    //if (m_module.Run(axisXY.WaitReady())) return p_sInfo;
                    //strRet = cam.Grab();
                    //dCenterHeight = CalculatingHeight(img);
                    //bmpCenter = null;
                    //bmpCenter = img.GetRectImage(new CRect(0, 0, img.p_Size.X, img.p_Size.Y));
                    //if (_dispatcher != null)
                    //{
                    //    _dispatcher.Invoke(new Action(delegate ()
                    //    {
                    //        string strTemp = String.Format("Center Laser Height = {0}", dCenterHeight);
                    //        BitmapSource bmpSrc = GetBitmapSource(bmpCenter);
                    //        p_bmpSrcCenterViewer = bmpSrc;
                    //        p_lstCenterStepInfo.Add(new CStepInfo(strTemp, bmpSrc));
                    //    }));
                    //}
                }
                finally
                {
                    m_module.SetLightByName("LADS", 0);
                }

                return base.Run();
            }
            //------------------------------------
            unsafe int CalculatingHeight(ImageData img)
            {
                // variable
                int nImgWidth = 640;
                int nImgHeight = 102;
                double dScale = 65535.0 / nImgHeight;
                double[] daHeight = new double[nImgWidth];
                
                // implement
                byte* pSrc = (byte*)img.GetPtr().ToPointer();
                for (int x = 0; x < nImgWidth; x++, pSrc++)
                {
                    byte* pSrcY = pSrc;
                    int nSum = 0;
                    int nYSum = 0;
                    for (int y = 0; y < nImgHeight; y++, pSrcY += nImgWidth)
                    {
                        if (*pSrcY < 70) continue;
                        nSum += *pSrcY;
                        nYSum += *pSrcY * y;
                    }
                    int iIndex = x;
                    daHeight[iIndex] = (nSum != 0) ? (double)((ushort)(dScale * nYSum / nSum) >> 8) : (ushort)0;
                }
                GetHeightAverage(daHeight);

                return 0;
            }
            //------------------------------------
            double GetHeightAverage(double[] daHeight)
            {
                // variable
                double dSum = 0.0;
                int nHitCount = 0;

                // implement
                for (int i = 0; i<daHeight.Length; i++)
                {
                    if (daHeight[i] == 0) continue;
                    nHitCount++;
                    dSum += daHeight[i];
                }
                return dSum / nHitCount;
            }
            //------------------------------------
            private BitmapSource GetBitmapSource(System.Drawing.Bitmap bitmap)
            {
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap
                (
                    bitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );

                return bitmapSource;
            }
        }
        #endregion

        #region Run_Inspection
        public class Run_Inspection : ModuleRunBase
        {
            SideVision m_module;
            public _2_6_SideViewModel m_sivm;
            //public _2_9_BevelViewModel m_bevm;

            public Run_Inspection(SideVision module)
            {
                m_module = module;
                m_sivm = m_module.m_sivm;
                //m_bevm = m_module.m_bevm;
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
                m_sivm._dispatcher.Invoke(new Action(delegate ()
                {
                    m_sivm._startInsp();
                }));

                while (true)
                {
                    DBConnector connector = new DBConnector("localhost", "Inspections", "root", "`ati5344");
                    if (connector.Open())
                    {
                        string strQuery = "SELECT inspStatusNum FROM inspections.inspstatus where idx=0;";
                        string strTemp = string.Empty;
                        var result = connector.SendQuery(strQuery, ref strTemp);
                        if (result == 0 && strTemp == "1") break;
                    }
                    Thread.Sleep(1000);
                }

                return "OK";
            }
        }
        #endregion
    }
}
