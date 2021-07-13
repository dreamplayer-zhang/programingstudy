using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Root_EFEM.Module;
using Root_VEGA_D_IPU.Module;
using Root_VEGA_D.Engineer;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Control.Ajin;
using RootTools.GAFs;
using RootTools.Lens.LinearTurret;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.RADS;
using RootTools.Trees;
using RootTools_Vision;
using RootTools_Vision.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Root_VEGA_D.Module
{
    public class Vision : ModuleBase, IWTRChild
    {
        #region GAF
        public ALID m_visionHomeError;
        public ALID m_visionInspectError;
        public ALID m_alidShutterSensor;
        public ALID m_alidReticleLocateInfoError;
        public ALID m_alidShutterDownError;
        public ALID m_alidShutterUpError;
        ALID m_alid_WaferExist;
        public ALID m_alidPMCoaxialError;
        public ALID m_alidPMTransmittedError;
        public ALID m_alidPMFail;

        void InitGAF()
        {
            m_visionHomeError = m_gaf.GetALID(this, "Vision Home Error", "Vision Home Error");
            m_visionInspectError = m_gaf.GetALID(this, "Vision Inspect Error", "Vision Inspect Error");
            m_alidShutterDownError = m_gaf.GetALID(this, "VS Shutter Error", "Shutter is not down");
            m_alidShutterUpError = m_gaf.GetALID(this, "VS Shutter Error", "Shutter is not up");
            m_alidPMCoaxialError = m_gaf.GetALID(this, "PM Error", "Coaxial Light PM Test is failed");
            m_alidPMTransmittedError = m_gaf.GetALID(this, "PM Error", "Transmitted Light PM Test is failed");
            m_alidPMFail = m_gaf.GetALID(this, "PM Fail", "PM is Fail, Pod is not load");
        }
        public void SetAlarm()
        {
            m_alid_WaferExist.Run(true, "Vision Wafer Exist Error");
        }
		#endregion

		#region ToolBox
		Axis m_axisRotate;
        Axis m_axisZ;
        AxisXY m_axisXY;
        DIO_O m_doVac;
        DIO_O m_doBlow;
        DIO_I m_diMaskProtrude1;//docking shutter에 존재하는 센서
        DIO_I m_diMaskProtrude2;//docking shutter에 존재하는 센서
        DIO_I m_diRobotHandProtrude;//docking shutter에 존재하는 센서
        DIO_O m_doShutterDown;
        DIO_O m_doShutterUp;
        DIO_I m_diShutterDownCheck;
        DIO_I m_diShutterUpCheck;
        DIO_I m_diStageReticleCheck;
        DIO_I m_diStageReticleTilt1;
        DIO_I m_diStageReticleTilt2;

        MemoryPool m_memoryPool;
        MemoryGroup m_memoryGroup;
        MemoryData m_memoryMain;
        MemoryData m_memoryLayer;
        MemoryData m_memoryOtherPC;
        LightSet m_lightSet;

        Camera_Dalsa m_CamMain;
        Camera_Basler m_CamAlign;
        Camera_Basler m_CamRADS;

        KlarfData_Lot m_KlarfData_Lot;
        LensLinearTurret m_LensLinearTurret;

        TCPIPComm_VEGA_D m_tcpipCommServer;
        RADSControl m_RADSControl;


        #region [Getter Setter]
        public Axis AxisRotate { get => m_axisRotate; private set => m_axisRotate = value; }
        public Axis AxisZ { get => m_axisZ; private set => m_axisZ = value; }
        public AxisXY AxisXY { get => m_axisXY; private set => m_axisXY = value; }
        public DIO_O DoVac { get => m_doVac; private set => m_doVac = value; }
        public DIO_O DoBlow { get => m_doBlow; private set => m_doBlow = value; }
        public MemoryPool MemoryPool { get => m_memoryPool; private set => m_memoryPool = value; }
        public MemoryGroup MemoryGroup { get => m_memoryGroup; private set => m_memoryGroup = value; }
        public MemoryData MemoryMain { get => m_memoryMain; private set => m_memoryMain = value; }
        public MemoryData MemoryLayer { get => m_memoryLayer; private set => m_memoryLayer = value; }
        public MemoryData MemoryOtherPC { get => m_memoryOtherPC; private set => m_memoryOtherPC = value; }
        public LightSet LightSet { get => m_lightSet; private set => m_lightSet = value; }
        public Camera_Dalsa CamMain { get => m_CamMain; private set => m_CamMain = value; }
        public Camera_Basler CamAlign { get => m_CamAlign; private set => m_CamAlign = value; }
        public Camera_Basler CamRADS { get => m_CamRADS; private set => m_CamRADS = value; }
        public KlarfData_Lot KlarfData_Lot { get => m_KlarfData_Lot; private set => m_KlarfData_Lot = value; }
        public TCPIPComm_VEGA_D TcpipCommServer { get => m_tcpipCommServer; private set => m_tcpipCommServer = value; }
        public RADSControl RADSControl { get => m_RADSControl; private set => m_RADSControl = value; }
        #endregion

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetAxis(ref m_axisRotate, this, "Axis Rotate");
            p_sInfo = m_toolBox.GetAxis(ref m_axisZ, this, "Axis Z");
            p_sInfo = m_toolBox.GetAxis(ref m_axisXY, this, "Axis XY");
            p_sInfo = m_toolBox.GetDIO(ref m_doVac, this, "Stage Vacuum");
            p_sInfo = m_toolBox.GetDIO(ref m_doBlow, this, "Stage Blow");
            p_sInfo = m_toolBox.GetDIO(ref m_diMaskProtrude1, this, "Shutter Mask Protrude 1");
            p_sInfo = m_toolBox.GetDIO(ref m_diMaskProtrude2, this, "Shutter Mask Protrude 2");
            p_sInfo = m_toolBox.GetDIO(ref m_diRobotHandProtrude, this, "Shutter Robot Hand Protrude");
            p_sInfo = m_toolBox.GetDIO(ref m_doShutterUp, this, "Shutter Up");
            p_sInfo = m_toolBox.GetDIO(ref m_doShutterDown, this, "Shutter Down");
            p_sInfo = m_toolBox.GetDIO(ref m_diShutterUpCheck, this, "Shutter Up Check");
            p_sInfo = m_toolBox.GetDIO(ref m_diShutterDownCheck, this, "Shutter Down Check");
            p_sInfo = m_toolBox.GetDIO(ref m_diStageReticleCheck, this, "Stage Reticle Check");
            p_sInfo = m_toolBox.GetDIO(ref m_diStageReticleTilt1, this, "Stage Reticle Tilt Check 1");
            p_sInfo = m_toolBox.GetDIO(ref m_diStageReticleTilt2, this, "Stage Reticle Tilt Check 2");
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            p_sInfo = m_toolBox.GetCamera(ref m_CamMain, this, "MainCam");
            p_sInfo = m_toolBox.GetCamera(ref m_CamAlign, this, "AlignCam");
            p_sInfo = m_toolBox.GetCamera(ref m_CamRADS, this, "RADS");
            p_sInfo = m_toolBox.Get(ref m_LensLinearTurret, this, "LensTurret");

            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory", 1);
            //m_remote.GetTools(bInit);

            InitALID();

            p_sInfo = m_toolBox.Get(ref m_RADSControl, this, "RADSControl", true);
        }
        void InitALID()
        {
            m_visionHomeError = m_gaf.GetALID(this, "Vision Home Error", "Vision Home Error");
            m_visionInspectError = m_gaf.GetALID(this, "Vision Inspect Error", "Vision Inspect Error");
            m_alidShutterSensor = m_gaf.GetALID(this, "Shutter Sensor Error", "Shutter Sensor is detected");
            m_alidShutterDownError = m_gaf.GetALID(this, "VS Shutter Error", "Shutter is not down");
            m_alidShutterUpError = m_gaf.GetALID(this, "VS Shutter Error", "Shutter is not up");
            m_alidReticleLocateInfoError = m_gaf.GetALID(this, "Reticle Locate Info is not Correct", "Reticle Locate Info is not Correct");
            m_alidPMCoaxialError = m_gaf.GetALID(this, "PM Coaxial Check Error", "Coaxial Light PM Test is failed");
            m_alidPMTransmittedError = m_gaf.GetALID(this, "PM Transmitted Check Error", "Transmitted Light PM Test is failed");
            m_alidPMFail = m_gaf.GetALID(this, "PM Fail", "PM is Fail, Pod is not load");
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

        public void ClearData()
        {
            foreach (GrabMode grabMode in m_aGrabMode)
            {
                grabMode.m_ptXYAlignData = new RPoint(0, 0);
                //grabMode.m_dVRSFocusPos = 0;
            }
            this.RunTree(Tree.eMode.RegWrite);
            this.RunTree(Tree.eMode.Init);
        }

        void RunTreeGrabMode(Tree tree)
        {
            m_lGrabMode = tree.Set(m_lGrabMode, m_lGrabMode, "Count", "Grab Mode Count");
            while (m_aGrabMode.Count < m_lGrabMode)
            {
                string id = "Mode." + m_aGrabMode.Count.ToString("00");
                GrabMode grabMode = new GrabMode(id, m_cameraSet, m_lightSet, m_memoryPool, m_LensLinearTurret);
                m_aGrabMode.Add(grabMode);
            }
            while (m_aGrabMode.Count > m_lGrabMode) m_aGrabMode.RemoveAt(m_aGrabMode.Count - 1);
            foreach (GrabMode grabMode in m_aGrabMode) grabMode.RunTreeName(tree.GetTree("Name", false));
            foreach (GrabMode grabMode in m_aGrabMode) grabMode.RunTree(tree.GetTree(grabMode.p_sName, false), true, false);
        }
        void RunTreeSetup(Tree tree)
        {
            m_eCheckWafer = (eCheckWafer)tree.Set(m_eCheckWafer, m_eCheckWafer, "CheckWafer", "CheckWafer");
        }
        #endregion

        #region Light
        public List<string> p_asLightSet
        {
            get
            {
                List<string> asLight = new List<string>();
                foreach (Light light in m_lightSet.m_aLight) asLight.Add(light.m_sName);
                return asLight;
            }
        }
        public Light GetLight(string sLight)
        {
            foreach (Light light in m_lightSet.m_aLight)
            {
                if (sLight == light.m_sName) return light;
            }
            return null;
        }
        #endregion

        #region DIO
        public bool p_bStageVac
        {
            get { return m_doVac.p_bOut; }
            set
            {
                if (m_doVac.p_bOut == value) return;
                m_doVac.Write(value);
            }
        }

        public bool p_bStageBlow
        {
            get { return m_doBlow.p_bOut; }
            set
            {
                if (m_doBlow.p_bOut == value) return;
                m_doBlow.Write(value);
            }
        }

        public void RunBlow(int msDelay)
        {
            m_doBlow.DelayOff(msDelay);
        }
        #endregion

        #region override
        public override void Reset()
        {
            if (p_eRemote == eRemote.Client) RemoteRun(eRemoteRun.Reset, eRemote.Client, null);
            else
            {
                base.Reset();
            }
        }

        public override void InitMemorys()
        {
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            m_memoryMain = m_memoryGroup.CreateMemory("Main", 3, 1, 40000, 40000);
            //m_memoryGroup2 = m_memoryPool2.GetGroup("group");
            //m_memoryGroup2.CreateMemory("ROI", 1, 4, 30000, 30000); // Chip 크기 최대 30,000 * 30,000 고정 Origin ROI 메모리 할당 20.11.02 JTL 

            m_memoryOtherPC = m_memoryGroup.CreateMemory("OtherPC", 1, 3, 40000, 40000);
            //ImageData(string sPool, string sGroup, string sMem, MemoryTool tool, int nPlane, int nByte)
        }
        #endregion

        #region Axis
        private int m_pulseRound = 1000;
        public int PulseRound { get => this.m_pulseRound; set => this.m_pulseRound = value; }
        void RunTreeAxis(Tree tree)
        {
            m_pulseRound = tree.Set(m_pulseRound, m_pulseRound, "Rotate Pulse / Round", "Rotate" +
                " Axis Pulse / 1 Round (pulse)");
        }

        public enum eAxisPosX
        { 
            Ready,
        }
        public enum eAxisPosY
        {
            Ready,
        }
        public enum eAxisPosZ
        {
            Ready,
        }
        public enum eAxisPosRotate
        {
            Ready,
        }
        void InitPosAlign()
        {
            m_axisZ.AddPos(Enum.GetNames(typeof(eAxisPosZ)));
            m_axisRotate.AddPos(Enum.GetNames(typeof(eAxisPosRotate)));
            if (m_axisXY.p_axisX != null)
            {
                (m_axisXY.p_axisX).AddPos(Enum.GetNames(typeof(eAxisPosX)));
            }
            if (m_axisXY.p_axisY != null)
            {
                (m_axisXY.p_axisY).AddPos(Enum.GetNames(typeof(eAxisPosY)));
            }
        }

        #endregion

        #region IWTRChild
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

        public List<string> p_asChildSlot
        {
            get { return null; }
        }

        public InfoWafer GetInfoWafer(int nID)
        {
            return p_infoWafer;
        }

        public void SetInfoWafer(int nID, InfoWafer infoWafer)
        {
            p_infoWafer = infoWafer;
        }

        public string IsGetOK(int nID)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_infoWafer == null) return p_id + " IsGetOK - InfoWafer not Exist";
            return "OK";
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_infoWafer != null) return p_id + " IsPutOK - InfoWafer Exist";
            if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false) return p_id + " not Enable Wafer Size";
            return "OK";
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            if (infoWafer == null) infoWafer = p_infoWafer;
            return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        }

        public string ShutterSafety()
		{
            if (!m_diMaskProtrude1.p_bIn) return "Mask Protrude 1 = "+ m_diMaskProtrude1.p_bIn;
            if (!m_diMaskProtrude2.p_bIn) return "Mask Protrude 2 = "+ m_diMaskProtrude2.p_bIn;
            if (!m_diRobotHandProtrude.p_bIn) return "Mask Protrude 1 = "+ m_diRobotHandProtrude.p_bIn;
            Thread.Sleep(1000);
            return "OK";
        }
        public string StageReticleCheck(bool bOn)
        {
            if (m_diStageReticleCheck.p_bIn == bOn) return "Reticle Check = " + m_diStageReticleCheck.p_bIn;
            if (m_diStageReticleTilt1.p_bIn == bOn) return "Reticle Tilt 1 = " + m_diStageReticleTilt1.p_bIn;
            if (m_diStageReticleTilt2.p_bIn == bOn) return "Reticle Tilt 2 = " + m_diStageReticleTilt2.p_bIn;
            return "OK";
        }

        public string BeforeGet(int nID)
        {
            //shutter 
            p_sInfo = ShutterSafety();
            if (p_sInfo != "OK")
            {
                m_alidShutterSensor.Run(true, p_sInfo);
                return p_sInfo;
            }
            m_doShutterUp.Write(false);
            Thread.Sleep(100);
            m_doShutterDown.Write(true);
            StopWatch sw = new StopWatch();
            sw.Start();
            while (!m_diShutterDownCheck.p_bIn || m_diShutterUpCheck.p_bIn)
            {
                if (sw.ElapsedMilliseconds > 5000)
                {
                    m_alidShutterDownError.Run(true, "Shutter error in Beforeget");
                }
            }
            //

            // 레티클 유무 체크
            p_sInfo = StageReticleCheck(false);//shutter Sensor check
            if (p_sInfo != "OK")
            {
                m_alidReticleLocateInfoError.Run(true, p_sInfo);
                return p_sInfo;
            }

            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.BeforeGet, eRemote.Client, nID);
            else
            {
                if (Run(m_axisZ.StartMove(eAxisPosZ.Ready))) return p_sInfo;
                if (Run(m_axisRotate.StartMove(eAxisPosRotate.Ready))) return p_sInfo;
                if (Run(m_axisXY.p_axisX.StartMove(eAxisPosX.Ready))) return p_sInfo;
                if (Run(m_axisXY.p_axisY.StartMove(eAxisPosY.Ready))) return p_sInfo;
                if (Run(m_axisXY.p_axisX.WaitReady()))
                    return p_sInfo;
                if (Run(m_axisXY.p_axisY.WaitReady()))
                    return p_sInfo;
                if (Run(m_axisRotate.WaitReady()))
                    return p_sInfo;
                if (Run(m_axisZ.WaitReady()))
                    return p_sInfo;
                //m_axisXY.StartMove("Position_0");
                //m_axisRotate.StartMove("Position_0");
                //m_axisZ.StartMove("Position_0");

                //m_axisXY.WaitReady();
                //m_axisRotate.WaitReady();
                //m_axisZ.WaitReady();

                ClearData();

                return "OK";
            }
        }

        public string BeforePut(int nID)
        {
            //shutter
            p_sInfo = ShutterSafety();
            if (p_sInfo != "OK")
            {
                m_alidShutterSensor.Run(true, p_sInfo);
                return p_sInfo;
            }
            m_doShutterUp.Write(false);
            Thread.Sleep(100);
            m_doShutterDown.Write(true);
            StopWatch sw = new StopWatch();
            sw.Start();
            while (!m_diShutterDownCheck.p_bIn || m_diShutterUpCheck.p_bIn)
            {
                if (sw.ElapsedMilliseconds > 5000)
                {
                    m_alidShutterDownError.Run(true, "Shutter error in Beforeput");
                }
            }
            // 레티클 유무 체크
            p_sInfo = StageReticleCheck(true);//shutter Sensor check
            if (p_sInfo != "OK")
            {
                m_alidReticleLocateInfoError.Run(true, p_sInfo);
                return p_sInfo;
            }

            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.BeforePut, eRemote.Client, nID);
            else
            {
                if (Run(m_axisZ.StartMove(eAxisPosZ.Ready))) return p_sInfo;
                if (Run(m_axisRotate.StartMove(eAxisPosRotate.Ready))) return p_sInfo;
                if (Run(m_axisXY.p_axisX.StartMove(eAxisPosX.Ready))) return p_sInfo;
                if (Run(m_axisXY.p_axisY.StartMove(eAxisPosY.Ready))) return p_sInfo;

                if (Run(m_axisXY.p_axisX.WaitReady()))
                    return p_sInfo;
                if (Run(m_axisXY.p_axisY.WaitReady()))
                    return p_sInfo;
                if (Run(m_axisRotate.WaitReady()))
                    return p_sInfo;
                if (Run(m_axisZ.WaitReady()))
                    return p_sInfo;
                //m_axisXY.StartMove("Position_0");
                //m_axisRotate.StartMove("Position_0");
                //m_axisZ.StartMove("Position_0");

                //m_axisXY.WaitReady();
                //m_axisRotate.WaitReady();
                //m_axisZ.WaitReady();

                return "OK";
            }
        }

        public string AfterGet(int nID)
        {
            p_sInfo = StageReticleCheck(true);
            if (p_sInfo != "OK")
            {
                m_alidReticleLocateInfoError.Run(true, p_sInfo);
                return p_sInfo;
            }
            ////shutter
            p_sInfo = ShutterSafety();
            if (p_sInfo != "OK")
            {
                m_alidShutterSensor.Run(true, p_sInfo);
                return p_sInfo;
            }
            m_doShutterDown.Write(false);
            Thread.Sleep(100);
            m_doShutterUp.Write(true);
            StopWatch sw = new StopWatch();
            sw.Start();
            while (m_diShutterDownCheck.p_bIn || !m_diShutterUpCheck.p_bIn)
            {
                if (sw.ElapsedMilliseconds > 5000)
                {
                    m_alidShutterDownError.Run(true, "Shutter error in Afterget");
                }
            }
            return "OK";
        }

        public string AfterPut(int nID)
        {
            p_sInfo = StageReticleCheck(false);
            if (p_sInfo != "OK")
            {
                m_alidReticleLocateInfoError.Run(true, p_sInfo);
                return p_sInfo;
            }
            ////shutter
            p_sInfo = ShutterSafety();
            if (p_sInfo != "OK")
            {
                m_alidShutterSensor.Run(true, p_sInfo);
                return p_sInfo;
            }
            m_doShutterDown.Write(false);
            Thread.Sleep(100);
            m_doShutterUp.Write(true);
            StopWatch sw = new StopWatch();
            sw.Start();
            while (m_diShutterDownCheck.p_bIn || !m_diShutterUpCheck.p_bIn)
            {
                if (sw.ElapsedMilliseconds > 5000)
                {
                    m_alidShutterDownError.Run(true, "Shutter error in Afterput");
                }
            }
            return "OK";
        }

        enum eCheckWafer
        {
            InfoWafer,
            Sensor
        }
        eCheckWafer m_eCheckWafer = eCheckWafer.InfoWafer;
        public bool IsWaferExist(int nID)
        {
            switch (m_eCheckWafer)
            {
                case eCheckWafer.Sensor:
                    if (!(m_diStageReticleCheck.p_bIn == m_diStageReticleCheck.p_bIn && m_diStageReticleCheck.p_bIn == m_diStageReticleCheck.p_bIn))
                    {
                        m_alidReticleLocateInfoError.Run(true, "Reticle Sensor value is not same.");
                    }
                    return m_diStageReticleCheck.p_bIn || m_diStageReticleCheck.p_bIn || m_diStageReticleCheck.p_bIn;//jws tilt체크필요
                default: return (p_infoWafer != null);
            }
        }

        InfoWafer.WaferSize m_waferSize;
        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }

        string m_sInfoWafer = "";
        InfoWafer _infoWafer = null;
        public InfoWafer p_infoWafer
        {
            get{ return _infoWafer; }
            set
            {
                m_sInfoWafer = (value == null) ? "" : value.p_id;
                _infoWafer = value;
                if (m_reg != null)
                    m_reg.Write("sInfoWafer", m_sInfoWafer);
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadInfoWafer_Registry()
        {
            m_reg = new Registry(p_id + ".InfoWafer");
            m_sInfoWafer = m_reg.Read("sInfoWafer", m_sInfoWafer);
            p_infoWafer = m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.StateHome, eRemote.Client, null);
            else
            {
                Thread.Sleep(200);
                if (m_CamMain != null && m_CamMain.p_CamInfo.p_eState == eCamState.Init) m_CamMain.Connect();

                p_sInfo = base.StateHome();
                m_visionHomeError.Run(p_sInfo != "OK", "Vision Home Error");
                p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;

                ClearData();

                return "OK";
            }
        }
        public int m_secHome = 60;
        void RunTreeTimeout(Tree tree)
        {
            m_secHome = tree.Set(m_secHome, m_secHome, "Home", "Timeout (sec)");
        }

        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeAxis(tree.GetTree("Axis", false));
            RunTreeGrabMode(tree.GetTree("Grab Mode", false));
            RunTreeSetup(tree.GetTree("Setup", false));
        }
        #endregion

        #region Vision Algorithm
        Image<Gray, byte> GetGrayByteImageFromMemory(MemoryData mem, CRect crtROI)
        {
            if (crtROI.Width < 1 || crtROI.Height < 1) return null;
            if (crtROI.Left < 0 || crtROI.Top < 0) return null;
            if (crtROI.Width % 4 != 0)
            {
                int nSpare = 0;
                while (((crtROI.Width + nSpare) % 4) != 0) nSpare++;
                crtROI = new CRect(crtROI.Left, crtROI.Top, crtROI.Right + nSpare, crtROI.Bottom);
            }
            ImageData img = new ImageData(crtROI.Width, crtROI.Height, 1);
            IntPtr p = mem.GetPtr();
            img.SetData(p, crtROI, (int)mem.W);

            byte[] barr = img.GetByteArray();
            System.Drawing.Bitmap bmp = img.GetBitmapToArray(crtROI.Width, crtROI.Height, barr);
            Image<Gray, byte> imgReturn = new Image<Gray, byte>(bmp);

            return imgReturn;
        }

        public Image<Gray, byte> GetGrayByteImageFromMemory_12bit(MemoryData mem, CRect crtROI)
        {
            if (crtROI.Width < 1 || crtROI.Height < 1) return null;
            if (crtROI.Left < 0 || crtROI.Top < 0) return null;
            if (crtROI.Width % 4 != 0)
            {
                int nSpare = 0;
                while (((crtROI.Width + nSpare) % 4) != 0) nSpare++;
                crtROI = new CRect(crtROI.Left, crtROI.Top, crtROI.Right + nSpare, crtROI.Bottom);
            }
            string strTempFile = "D:\\AlignMarkTemplateImage\\TempAreaImage.bmp";
            ImageData tempImgData = new ImageData(mem);
            tempImgData.FileSaveGrayBMP(strTempFile, crtROI, 1, ImageData.eRgbChannel.None, 4);

            return new Image<Gray, byte>(strTempFile);
            //ImageData img = new ImageData(crtROI.Width, crtROI.Height, 1);
            //IntPtr p = mem.GetPtr();
            //img.SetData_12bit(p, crtROI, (int)(mem.W / mem.p_nByte));

            //byte[] barr = img.GetByteArray();
            //System.Drawing.Bitmap bmp = img.GetBitmapToArray(crtROI.Width, crtROI.Height, barr);
            //Image<Gray, byte> imgReturn = new Image<Gray, byte>(bmp);

            //return imgReturn;
        }

        public bool TemplateMatching(MemoryData mem, CRect crtSearchArea, Image<Gray, byte> imgSrc, Image<Gray, byte> imgTemplate, out CPoint cptCenter, double dMatchScore)
        {
            // variable
            int nWidthDiff = 0;
            int nHeightDiff = 0;
            Point ptMaxRelative = new Point();
            float fMaxScore = float.MinValue;
            bool bFoundTemplate = false;

            // implement
            if (imgTemplate.Width > imgSrc.Width || imgTemplate.Height > imgSrc.Height)
            {
                cptCenter = new CPoint();
                cptCenter.X = 0;
                cptCenter.Y = 0;
                return false;
            }
            Image<Gray, float> imgResult = imgSrc.MatchTemplate(imgTemplate, TemplateMatchingType.CcorrNormed);
            nWidthDiff = imgSrc.Width - imgResult.Width;
            nHeightDiff = imgSrc.Height - imgResult.Height;
            float[,,] matches = imgResult.Data;

            for (int x = 0; x < matches.GetLength(1); x++)
            {
                for (int y = 0; y < matches.GetLength(0); y++)
                {
                    if (fMaxScore < matches[y, x, 0] && dMatchScore <= matches[y, x, 0])
                    {
                        fMaxScore = matches[y, x, 0];
                        ptMaxRelative.X = x;
                        ptMaxRelative.Y = y;
                        bFoundTemplate = true;
                    }
                }
            }
            cptCenter = new CPoint();
            cptCenter.X = (int)(crtSearchArea.Left + ptMaxRelative.X) + (int)(nWidthDiff / 2);
            cptCenter.Y = (int)(crtSearchArea.Top + ptMaxRelative.Y) + (int)(nHeightDiff / 2);

            return bFoundTemplate;
        }
        public string RunLineScan(GrabMode grabmode, MemoryData mem, CPoint memOffset, int nSnapCount, double posX, double startPosY, double endPosY, double startTriggerY, double endTriggerY)
        {
            if (grabmode == null) return "Grabmode of RunLineScan is null.";

            Camera_Dalsa camMain = grabmode.m_camera as Camera_Dalsa;
            if (camMain == null)
                return "Main Camara is null";

            // 시작위치로 이동
            if (Run(AxisXY.StartMove(posX, startPosY)))
                return p_sInfo;
            if (Run(AxisXY.WaitReady()))
                return p_sInfo;

            // 분주비 재설정
            int nEncoderMul = 1;
            int nEncoderDiv = 1;

            camMain.p_CamParam.GetRotaryEncoderMultiplier(ref nEncoderMul);
            camMain.p_CamParam.GetRotaryEncoderDivider(ref nEncoderDiv);

            camMain.p_CamParam.SetRotaryEncoderMultiplier(1);
            camMain.p_CamParam.SetRotaryEncoderDivider(1);
            camMain.p_CamParam.SetRotaryEncoderMultiplier(nEncoderMul);
            camMain.p_CamParam.SetRotaryEncoderDivider(nEncoderDiv);

            // 트리거 설정
            AxisXY.p_axisY.SetTrigger(startTriggerY, endTriggerY, grabmode.m_dTrigger, 0.001, true);

            // 카메라 스냅 시작
            grabmode.StartGrab(mem, memOffset, (int)(nSnapCount * 0.98), grabmode.m_GD);

            // 이동하면서 그랩
            if (Run(AxisXY.p_axisY.StartMove(endPosY)))
                return p_sInfo;

            // 라인스캔 완료 대기
            if (Run(AxisXY.p_axisY.WaitReady()))
                return p_sInfo;

            // 이미지 스냅 스레드 동작중이라면 중지
            while (camMain.p_CamInfo.p_eState != eCamState.Ready && !EQ.IsStop())
            {
                Thread.Sleep(10);
            }
            return "OK";
        }
        public string StartRADS(int nOffset = 0)
        {
            if (CamRADS == null) return "RADS Cam is null";

            RADSControl.StartRADS();

            StopWatch sw = new StopWatch();
            if (CamRADS.p_CamInfo._OpenStatus == false) CamRADS.Connect();
            while (CamRADS.p_CamInfo._OpenStatus == false)
            {
                if (sw.ElapsedMilliseconds > 15000)
                {
                    sw.Stop();
                    return "RADS Camera Not Connected";
                }
            }
            sw.Stop();

            // Offset 설정
            RADSControl.p_connect.SetADSOffset(nOffset);

            // RADS 카메라 설정
            CamRADS.SetMulticast();
            CamRADS.GrabContinuousShot();

            return "OK";
        }

        public string StopRADS()
        {
            if (CamRADS == null) return "RADS Cam is null";

            RADSControl.StopRADS();
            if (CamRADS.p_CamInfo._IsGrabbing == true) CamRADS.StopGrab();

            return "OK";
        }
        #endregion

        public Vision(string id, IEngineer engineer, eRemote eRemote = eRemote.Local)
        {
            base.InitBase(id, engineer);
            InitPosAlign();
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            OnChangeState += Vision_OnChangeState;

            // IPU PC와 연결될 Server Socket 생성
            TCPIPServer server = null;
            m_toolBox.GetComm(ref server, this, "TCPIP");

            m_tcpipCommServer = new TCPIPComm_VEGA_D(server);
            m_tcpipCommServer.EventReceiveData += EventReceiveData;
            m_tcpipCommServer.EventAccept += EventAccept;

            // IPU와 통신 중단 시 처리 위한 타이머
            m_timerWaitReconnect.Tick += new EventHandler(WaitReconnectTimerTick);

            // GrabLineScan 이벤트 설정
            foreach (ModuleRunBase moduleRunBase in m_aModuleRun)
            {
                Run_GrabLineScan moduleRun = moduleRunBase as Run_GrabLineScan;
                if (moduleRun != null)
                {
                    moduleRun.LineScanInit += ModuleRun_LineScanInit;
                    moduleRun.AlignCompleted += Run_GrabLineScan_AlignCompleted;
                    moduleRun.LineScanStarting += Run_GrabLineScan_LineScanStarting;
                    moduleRun.LineScanCompleted += Run_GrabLineScan_LineScanCompleted;
                    moduleRun.LineScanEnd += ModuleRun_LineScanEnd;
                }
            }
        }

        private void Vision_OnChangeState(eState eState)
        {
            switch (p_eState)
            {
                case eState.Init:
                case eState.Error:
                    RemoteRun(eRemoteRun.ServerState, eRemote.Server, eState);
                    break;
            }
        }
        ModuleRunBase PeekModuleRun()
        {
            if (m_qModuleRun.Count > 0)
            {
                return m_qModuleRun.Peek();
            }
            else
                return null;
        }

        bool _bWaitReconn = false;              // IPU와 소켓통신 연결이 끊어졌을 때의 재연결 대기하는 상태값
        public bool p_bWaitReconn
        {
            get { return _bWaitReconn; }
            set
            {
                if (_bWaitReconn == value) return;

                m_log.Info(string.Format("{0}.p_bWaitReconn {1} -> {2}", p_id, _bWaitReconn, !_bWaitReconn));
                _bWaitReconn = value;
            }
        }

        DispatcherTimer m_timerWaitReconnect = new DispatcherTimer();
        private void WaitReconnectTimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = sender as DispatcherTimer;
            if (timer != null)
                timer.Stop();

            p_bWaitReconn = false;
        }

        const int m_cnDisconnectedStateResetTime = 5 * 60 * 1000;              // IPU와 연결이 끊어지고 재개될때까지 대기하는 시간
        private void EventAccept(Socket socket)
        {
            if (p_bWaitReconn)
            {
                Run_GrabLineScan runGrabLineScan = PeekModuleRun() as Run_GrabLineScan;
                if (runGrabLineScan != null)
                {
                    lock (runGrabLineScan.m_lockWaitRun)
                    {
                    }
                }
                else
                {
                    m_timerWaitReconnect.Stop();
                    p_bWaitReconn = false;
                }
            }
        }
        private void EventReceiveData(byte[] aBuf, int nSize, Socket socket)
        {
            if (nSize <= 0)
            {
                // 연결이 종료되었을 때
                if (socket.Connected)
                    return;

                p_bWaitReconn = true;

                m_timerWaitReconnect.Stop();
                m_timerWaitReconnect.Interval = TimeSpan.FromMilliseconds(m_cnDisconnectedStateResetTime);
                m_timerWaitReconnect.Start();
            }
            else
            {
                int nStartIdx = 0;
                TCPIPComm_VEGA_D.Command cmd = TCPIPComm_VEGA_D.Command.None;
                Dictionary<string, string> mapParam = new Dictionary<string, string>();

                // 도착한 메세지를 파싱하여 메세지 단위마다 처리
                while (m_tcpipCommServer.ParseMessage(aBuf, nSize, ref nStartIdx, ref cmd, mapParam))
                {
                    switch (cmd)
                    {
                        case TCPIPComm_VEGA_D.Command.resume:
                            {
                                if (p_bWaitReconn)
                                {
                                    // 이전에 이미지 그랩 작업을 이어서 할 수 있도록 처리
                                    int totalScanLine = int.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_TOTALSCANLINECOUNT]);
                                    int curScanLine = int.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_CURRENTSCANLINE]);
                                    int startScanLine = int.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_STARTSCANLINE]);

                                    Run_GrabLineScan runGrabLineScan = PeekModuleRun() as Run_GrabLineScan;
                                    if (runGrabLineScan != null)
                                    {
                                        lock (runGrabLineScan.m_lockWaitRun)
                                        {
                                            runGrabLineScan.m_nCurScanLine = curScanLine;
                                            runGrabLineScan.m_grabMode.m_ScanLineNum = totalScanLine;
                                            runGrabLineScan.m_grabMode.m_ScanStartLine = startScanLine;
                                        }
                                    }
                                }

                                m_timerWaitReconnect.Stop();
                                p_bWaitReconn = false;
                            }
                            break;
                        case TCPIPComm_VEGA_D.Command.Result:
                            {
                                Run_GrabLineScan runGrabLineScan = PeekModuleRun() as Run_GrabLineScan;
                                if (runGrabLineScan != null)
                                {
                                    // 현재 GrabLineScan 진행중이었다면
                                    if (runGrabLineScan.p_eRunState == ModuleRunBase.eRunState.Run)
                                    {
                                        // IPU에서 이미지 검사 완료되었기 때문에 해당 상태변수 true로 변경
                                        runGrabLineScan.m_bIPUCompleted = true;
                                    }
                                }
                            }
                            break;
                        case TCPIPComm_VEGA_D.Command.InspStatus:
                            {
                                Run_GrabLineScan runGrabLineScan = PeekModuleRun() as Run_GrabLineScan;
                                if (runGrabLineScan != null)
                                {
                                    int nEndLine = int.Parse(mapParam[TCPIPComm_VEGA_D.PARAM_NAME_INSPENDLINE]);
                                    
                                    // Call Line Inspection End Event Function
                                    if (LineScanStatusChanged != null)
                                        LineScanStatusChanged(this, runGrabLineScan, LineScanStatus.LineInspCompleted, new int[2] { runGrabLineScan.m_grabMode.m_ScanLineNum, nEndLine });
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public override void ThreadStop()
        {
            //
            base.ThreadStop();
        }

        #region RemoteRun
        public enum eRemoteRun
        {
            ServerState,
            StateHome,
            Reset,
            BeforeGet,
            BeforePut,
        }

        Run_Remote GetRemoteRun(eRemoteRun eRemoteRun, eRemote eRemote, dynamic value)
        {
            Run_Remote run = new Run_Remote(this);
            run.m_eRemoteRun = eRemoteRun;
            run.m_eRemote = eRemote;
            switch (eRemoteRun)
            {
                case eRemoteRun.ServerState: run.m_eState = value; break;
                case eRemoteRun.StateHome: break;
                case eRemoteRun.Reset: break;
                case eRemoteRun.BeforeGet: run.m_nID = value; break;
                case eRemoteRun.BeforePut: run.m_nID = value; break;
            }
            return run;
        }

        string RemoteRun(eRemoteRun eRemoteRun, eRemote eRemote, dynamic value)
        {
            Run_Remote run = GetRemoteRun(eRemoteRun, eRemote, value);
            StartRun(run);
            while (run.p_eRunState != ModuleRunBase.eRunState.Done)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
            }
            return p_sInfo;
        }

        public class Run_Remote : ModuleRunBase
        {
            Vision m_module;
            public Run_Remote(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eRemoteRun m_eRemoteRun = eRemoteRun.StateHome;
            public eState m_eState = eState.Init;
            public int m_nID = 0;
            public override ModuleRunBase Clone()
            {
                Run_Remote run = new Run_Remote(m_module);
                run.m_eRemoteRun = m_eRemoteRun;
                run.m_eState = m_eState;
                run.m_nID = m_nID;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eRemoteRun = (eRemoteRun)tree.Set(m_eRemoteRun, m_eRemoteRun, "RemoteRun", "Select Remote Run", bVisible);
                m_eRemote = (eRemote)tree.Set(m_eRemote, m_eRemote, "Remote", "Remote", false);
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.ServerState:
                        m_eState = (eState)tree.Set(m_eState, m_eState, "State", "Module State", bVisible);
                        break;
                    case eRemoteRun.BeforeGet:
                    case eRemoteRun.BeforePut:
                        m_nID = tree.Set(m_nID, m_nID, "SlotID", "Slot ID", false);
                        break;
                }
            }

            public override string Run()
            {
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.ServerState: m_module.p_eState = m_eState; break;
                    case eRemoteRun.StateHome: return m_module.StateHome();
                    case eRemoteRun.Reset: m_module.Reset(); break;
                    case eRemoteRun.BeforeGet: return m_module.BeforeGet(m_nID);
                    case eRemoteRun.BeforePut: return m_module.BeforePut(m_nID);
                }
                return "OK";
            }
        }
        #endregion
        #region Test_Run
        public class Run_Test : ModuleRunBase
        {
            Vision m_module;
            public RPoint m_rpAxisCenter = new RPoint();
            public Run_Test(Vision module)
            {
                m_module = module;
                InitModuleRun(module);

            }
            public override ModuleRunBase Clone()
            {
                Run_Test run = new Run_Test(m_module);
                run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
                return run;
            }
            string m_sFlip = "Test";
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);
                m_sFlip = tree.Set(m_sFlip, m_sFlip, "Test", "Bottom", bVisible, true);
            }

            public override string Run()
            {
                Thread.Sleep(1000);
                AxisXY axisXY = m_module.m_axisXY;
                double dStartPosY = m_rpAxisCenter.Y;



                double dPosX = m_rpAxisCenter.X;


                if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
                Thread.Sleep(2000);
                //m_module.p_eState = eState.Ready;
                return "OK";
            }
        }
        #endregion

        #region ModuleRun
        public ModuleRunBase m_runPM;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Test(this), true, "Test");
            AddModuleRunList(new Run_Remote(this), true, "Remote Run");
            //AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            //AddModuleRunList(new Run_Rotate(this), false, "Rotate Axis");
            AddModuleRunList(new Run_GrabLineScan(this), true, "Run Grab LineScan Camera");
            //AddModuleRunList(new Run_Inspect(this), true, "Run Inspect");
            //AddModuleRunList(new Run_VisionAlign(this), true, "Run VisionAlign");
            //AddModuleRunList(new Run_AutoFocus(this), false, "Run AutoFocus");
            AddModuleRunList(new Run_MakeTemplateImage(this), true, "Run Make TemplateImage");
            AddModuleRunList(new Run_PatternAlign(this), true, "Run Pattern Align");
            m_runPM = AddModuleRunList(new Run_PM(this,(VEGA_D_Handler)m_engineer.ClassHandler()), true, "Run PM");
        }
        #endregion

        #region Event
        public delegate void OnLineScanEvent(Vision vision, Run_GrabLineScan moduleRun, LineScanStatus status, object data);

        public enum LineScanStatus
        {
            Init,
            AlignCompleted,
            LineScanStarting,
            LineScanCompleted,
            LineInspCompleted,
            End,
        }
        public event OnLineScanEvent LineScanStatusChanged;

        void Run_GrabLineScan_AlignCompleted(Run_GrabLineScan moduleRun, object data)
        {
            if (LineScanStatusChanged != null)
                LineScanStatusChanged(this, moduleRun, LineScanStatus.AlignCompleted, data);
        }
        void Run_GrabLineScan_LineScanStarting(Run_GrabLineScan moduleRun, object data)
        {
            if (LineScanStatusChanged != null)
                LineScanStatusChanged(this, moduleRun, LineScanStatus.LineScanStarting, data);
        }
        void Run_GrabLineScan_LineScanCompleted(Run_GrabLineScan moduleRun, object data)
        {
            if (LineScanStatusChanged != null)
                LineScanStatusChanged(this, moduleRun, LineScanStatus.LineScanCompleted, data);
        }
        private void ModuleRun_LineScanInit(Run_GrabLineScan moduleRun, object data)
        {
            if (LineScanStatusChanged != null)
                LineScanStatusChanged(this, moduleRun, LineScanStatus.Init, data);
        }
        private void ModuleRun_LineScanEnd(Run_GrabLineScan moduleRun, object data)
        {
            if (LineScanStatusChanged != null)
                LineScanStatusChanged(this, moduleRun, LineScanStatus.End, data);
        }

        #endregion
    }
}
