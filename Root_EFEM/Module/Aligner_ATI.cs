using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.CognexOCR;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Memory;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_EFEM.Module
{
    public class Aligner_ATI : ModuleBase, IWTRChild
    {
        #region ToolBox
        Axis m_axisRotate;
        AxisXY m_axisCamAlign;
        Axis m_axisCamOCRX;
        Axis m_axisCamOCRTopZ;
        Axis m_axisCamOCRBotZ;
        DIO_IO m_dioVac;
        DIO_O m_doBlow;
        DIO_O m_doLightCoaxial;
        DIO_O m_doLightSide;
        DIO_I2O2 m_dioLift;
        DIO_I m_diWaferExist;
        MemoryPool m_memoryPool;
        CameraBasler m_camAlign;
        Camera_CognexOCR m_camOCR;

        ALID m_alid_WaferExist;

        public void SetAlarm()
        {
            m_alid_WaferExist.Run(true, "Aligner Wafer Exist Error");
        }

        public override void GetTools(bool bInit)
        {
            m_flipper.GetTools(m_toolBox, this, bInit);
            p_sInfo = m_toolBox.GetAxis(ref m_axisRotate, this, "AxisRotate");
            p_sInfo = m_toolBox.GetAxis(ref m_axisCamAlign, this, "AxisCamera");
            p_sInfo = m_toolBox.GetAxis(ref m_axisCamOCRX, this, "AxisOCRX");
            p_sInfo = m_toolBox.GetAxis(ref m_axisCamOCRTopZ, this, "AxisOCRTopZ");
            p_sInfo = m_toolBox.GetAxis(ref m_axisCamOCRBotZ, this, "AxisOCRBotZ");
            p_sInfo = m_toolBox.GetDIO(ref m_dioVac, this, "Vacuum");
            p_sInfo = m_toolBox.GetDIO(ref m_doBlow, this, "Blow");
            p_sInfo = m_toolBox.GetDIO(ref m_doLightCoaxial, this, "LightCoaxial");
            p_sInfo = m_toolBox.GetDIO(ref m_doLightSide, this, "LightSide");
            p_sInfo = m_toolBox.GetDIO(ref m_dioLift, this, "Lift", "Down", "Up");
            p_sInfo = m_toolBox.GetDIO(ref m_diWaferExist, this, "WaferExist");
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory", 1);
            p_sInfo = m_toolBox.GetCamera(ref m_camAlign, this, "Align");
            p_sInfo = m_toolBox.GetCamera(ref m_camOCR, this, "OCR");

            m_alid_WaferExist = m_gaf.GetALID(this, "Aligner Wafer Exist", "Aligner Wafer Exist");
            if (bInit) InitTools();
        }

        public CPoint m_szAlignROI = new CPoint(1400, 512);
        void InitTools()
        {
            InitMemory();
            InitPosAlign();
            InitPosAlignRotate();
            InitPosOCRX();
            InitPosOCRTopZ();
            InitPosOCRBotZ();
        }
        #endregion

        #region Flipper
        public class Flipper
        {
            Axis m_axisX;
            Axis m_axisZ;
            Axis m_axisRotate;
            DIO_IO m_dioVacuum;
            DIO_O m_doBlow;
            DIO_I2O2 m_dioGuide;
            public void GetTools(ToolBox toolBox, Aligner_ATI module, bool bInit)
            {
                module.p_sInfo = toolBox.GetDIO(ref m_dioVacuum, module, p_id + "Vacuum");
                module.p_sInfo = toolBox.GetDIO(ref m_doBlow, module, p_id + "Blow");
                module.p_sInfo = toolBox.GetDIO(ref m_dioGuide, module, p_id + "Guide", "Back", "Push");
                module.p_sInfo = toolBox.GetAxis(ref m_axisX, module, p_id + "AxisX");
                module.p_sInfo = toolBox.GetAxis(ref m_axisZ, module, p_id + "AxisZ");
                module.p_sInfo = toolBox.GetAxis(ref m_axisRotate, module, p_id + "AxisRotate");
                if (bInit)
                {
                    InitPosX();
                    InitPosZ();
                    InitPosRotate();
                }
            }

            #region Vacuum
            double m_secBlow = 0.5;
            double m_secVac = 1;
            public string RunVacuum(bool bOn)
            {
                m_dioVacuum.Write(bOn);
                if (bOn == false)
                {
                    m_doBlow.Write(true);
                    Thread.Sleep((int)(500 * m_secBlow));
                    m_doBlow.Write(false);
                    return "OK";
                }
                int msVac = (int)(1000 * m_secVac);
                while (m_dioVacuum.p_bIn != bOn)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return p_id + " EQ Stop";
                    if (m_dioVacuum.m_swWrite.ElapsedMilliseconds > msVac) return "Flipper Vacuum Sensor Timeout";
                }
                return "OK";
            }
            #endregion

            #region Guide
            public string RunGuide(bool bGuide)
            {
                m_dioGuide.Write(bGuide);
                return m_dioGuide.WaitDone();
            }
            #endregion

            #region AxisX
            public double m_xOffset = 20000; // Forward + m_xOffset = 2170000
            public enum ePosX
            {
                Backward, // -115000
                Forward // 2150000
            }
            void InitPosX()
            {
                m_axisX.AddPos(Enum.GetNames(typeof(ePosX)));
            }

            public string RunMoveX(ePosX ePosX, double fOffset)
            {
                m_axisX.StartMove(ePosX, fOffset);
                return m_axisX.WaitReady();
            }
            #endregion

            #region AxisZ
            public enum ePosZ
            {
                PutReady, // -300000
                PutDown, //-106400
                GetReady, // 35000
                GetUp, // -126992
                InversePutReady, // -300000
                InversePutDown, // -155000
                InverseGetReady, //-300000
                InverseGetDown // -142500
            }
            void InitPosZ()
            {
                m_axisZ.AddPos(Enum.GetNames(typeof(ePosZ)));
            }

            public string RunMoveZ(ePosZ ePosZ)
            {
                m_axisZ.StartMove(ePosZ);
                return m_axisZ.WaitReady();
            }
            #endregion

            #region AxisRotate
            public enum ePosRotate
            {
                UpSide, // 0
                DownSide // 1800000
            }
            void InitPosRotate()
            {
                m_axisRotate.AddPos(Enum.GetNames(typeof(ePosRotate)));
            }

            public string RunMoveRotate(ePosRotate ePosRotate)
            {
                m_axisRotate.StartMove(ePosRotate);
                return m_axisRotate.WaitReady();
            }
            #endregion

            #region Tree
            public void RunTree(Tree tree)
            {
                m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Vaccum Blow Time (sec)");
                m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum On Timeout (sec)");
                m_xOffset = tree.Set(m_xOffset, m_xOffset, "X Offset", "Axis X Moving Offset");
            }
            #endregion

            public string p_id { get; set; }
            public Flipper(string id)
            {
                p_id = id;
            }

        }
        Flipper m_flipper = new Flipper("Flipper");

        public string RunPut()
        {
            // Start Init
            if (Run(AxisMoveAlign(ePosAlign.Ready, true))) return p_sInfo;
            if (Run(RunVacuum(false))) return p_sInfo;
            if (Run(m_flipper.RunGuide(true))) return p_sInfo;
            if (Run(m_flipper.RunVacuum(true))) return p_sInfo;
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Backward, 0))) return p_sInfo;
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.PutReady))) return p_sInfo;
            if (Run(m_flipper.RunMoveRotate(Flipper.ePosRotate.UpSide))) return p_sInfo;
            // End Init

            // Start Put
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Forward, 0))) return p_sInfo;
            if (Run(m_flipper.RunVacuum(false))) return p_sInfo;
            if (Run(m_flipper.RunGuide(false))) return p_sInfo;
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.PutDown))) return p_sInfo;
            if (Run(RunVacuum(true))) return p_sInfo;
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Forward, m_flipper.m_xOffset))) return p_sInfo;
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.GetReady))) return p_sInfo;
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Backward, 0))) return p_sInfo;
            // End Put

            return "OK";
        }

        public string RunGet()
        {
            // Start Init
            if (Run(AxisMoveAlign(ePosAlign.Ready, true))) return p_sInfo;
            if (Run(RunVacuum(true))) return p_sInfo;
            if (Run(m_flipper.RunVacuum(false))) return p_sInfo;
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Backward, 0))) return p_sInfo;
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.GetReady))) return p_sInfo;
            if (Run(m_flipper.RunMoveRotate(Flipper.ePosRotate.UpSide))) return p_sInfo;
            if (Run(m_flipper.RunGuide(false))) return p_sInfo;
            // End Init

            // Start Get
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Forward, m_flipper.m_xOffset))) return p_sInfo;
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.GetUp))) return p_sInfo;
            if (Run(RunVacuum(false))) return p_sInfo;
            if (Run(m_flipper.RunGuide(true))) return p_sInfo;
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.InversePutReady))) return p_sInfo;
            if (Run(m_flipper.RunVacuum(true))) return p_sInfo;
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Backward, 0))) return p_sInfo;
            // End Get

            // Test Code
            /*if (Run(m_flipper.RunMoveX(Flipper.ePosX.Forward, m_flipper.m_xOffset))) return p_sInfo;
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.GetUp))) return p_sInfo;
            if (Run(RunVacuum(false))) return p_sInfo;
            if (Run(m_flipper.RunGuide(true))) return p_sInfo;
            Thread.Sleep(10);
            if (Run(m_flipper.RunGuide(false))) return p_sInfo; // JEONG
            if (Run(m_flipper.RunVacuum(true))) return p_sInfo;
            if (Run(m_flipper.RunGuide(true))) return p_sInfo; // JEONG
            //if (Run(m_flipper.RunVacuum(true))) return p_sInfo; // JEONG : Vacuum ON 확인 위해서
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.InversePutReady))) return p_sInfo;
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Backward, 0))) return p_sInfo;*/

            return "OK";
        }

        public string RunInversePut()
        {
            // Start Init
            if (Run(AxisMoveAlign(ePosAlign.Ready, true))) return p_sInfo;
            if (Run(RunVacuum(false))) return p_sInfo;
            if (Run(m_flipper.RunGuide(true))) return p_sInfo;
            if (Run(m_flipper.RunVacuum(true))) return p_sInfo;
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Backward, 0))) return p_sInfo;
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.InversePutReady))) return p_sInfo;
            if (Run(m_flipper.RunMoveRotate(Flipper.ePosRotate.DownSide))) return p_sInfo;
            // End Init

            // Start InversePut
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Forward, 0))) return p_sInfo;
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.InversePutDown))) return p_sInfo;
            if (Run(m_flipper.RunVacuum(false))) return p_sInfo;
            if (Run(m_flipper.RunGuide(false))) return p_sInfo;
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Forward, m_flipper.m_xOffset))) return p_sInfo;
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.InverseGetReady))) return p_sInfo;
            if (Run(RunVacuum(true))) return p_sInfo;
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Backward, 0))) return p_sInfo;
            // End InversePut

            return "OK";
        }

        public string RunInverseGet()
        {
            // Start Init
            if (Run(AxisMoveAlign(ePosAlign.Ready, true))) return p_sInfo;
            if (Run(RunVacuum(true))) return p_sInfo;
            if (Run(m_flipper.RunVacuum(false))) return p_sInfo;
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Backward, 0))) return p_sInfo;
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.InverseGetReady))) return p_sInfo;
            if (Run(m_flipper.RunMoveRotate(Flipper.ePosRotate.DownSide))) return p_sInfo;
            if (Run(m_flipper.RunGuide(false))) return p_sInfo;
            // End Init

            // Start RunInverseGet
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Forward, m_flipper.m_xOffset))) return p_sInfo;
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.InversePutDown))) return p_sInfo;
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Forward, 0))) return p_sInfo;
            if (Run(RunVacuum(false))) return p_sInfo;
            if (Run(m_flipper.RunGuide(true))) return p_sInfo;
            Thread.Sleep(10);
            if (Run(m_flipper.RunGuide(false))) return p_sInfo; // JEONG
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.InverseGetDown))) return p_sInfo;
            if (Run(m_flipper.RunVacuum(true))) return p_sInfo;
            if (Run(m_flipper.RunGuide(true))) return p_sInfo; // JEONG
            //if (Run(m_flipper.RunVacuum(true))) return p_sInfo; // JEONG : Vacuum ON 확인 위해서
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.PutReady))) return p_sInfo;
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Backward, 0))) return p_sInfo;
            // End RunInverseGet

            return "OK";
        }

        public string RunRNR()
        {
            int count = 0;

            while (true)
            {
                if (EQ.IsStop())
                {
                    return p_id + " EQ Stop (RNR: " + count.ToString() + "counts)";
                }

                if (RunPut() != "OK")
                {
                    return "RNR RunPut failed at " + count.ToString() + " counts";
                }
                if (RunGet() != "OK")
                {
                    return "RNR RunGet failed at " + count.ToString() + " counts";
                }
                if (RunInversePut() != "OK")
                {
                    return "RNR RunInversePut failed at " + count.ToString() + " counts";
                }
                if (RunInverseGet() != "OK")
                {
                    return "RNR RunInverseGet failed at " + count.ToString() + " counts";
                }

                count = count + 1;
            }
        }

        public string RunRecovery()
        {
            if (Run(m_flipper.RunMoveZ(Flipper.ePosZ.PutReady))) return p_sInfo;
            if (Run(m_flipper.RunMoveX(Flipper.ePosX.Backward, 0))) return p_sInfo;
            if (Run(m_flipper.RunMoveRotate(Flipper.ePosRotate.UpSide))) return p_sInfo;
            if (Run(RunVacuum(false))) return p_sInfo;
            if (Run(m_flipper.RunVacuum(false))) return p_sInfo;
            if (Run(m_flipper.RunGuide(false))) return p_sInfo;

            return "OK";
        }
        #endregion

        #region DIO Function
        public void LightOff()
        {
            m_doLightCoaxial.Write(false);
            m_doLightSide.Write(false);
        }

        public bool p_bLightCoaxial
        {
            get { return m_doLightCoaxial.p_bOut; }
            set { m_doLightCoaxial.Write(value); }
        }

        public bool p_bLightSide
        {
            get { return m_doLightSide.p_bOut; }
            set { m_doLightSide.Write(value); }
        }

        double m_secBlow = 0.5;
        double m_secVac = 1;
        public string RunVacuum(bool bOn)
        {
            m_dioVac.Write(bOn);
            if (bOn == false)
            {
                m_doBlow.Write(true);
                Thread.Sleep((int)(500 * m_secBlow));
                m_doBlow.Write(false);
                return "OK";
            }
            int msVac = (int)(1000 * m_secVac);
            while (m_dioVac.p_bIn != bOn)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return p_id + " EQ Stop";
                if (m_dioVac.m_swWrite.ElapsedMilliseconds > msVac) return "Aligner Vacuum Sensor Timeout";
            }
            return "OK";
        }
        #endregion

        #region InfoWafer
        string m_sInfoWafer = "";
        InfoWafer _infoWafer = null;
        public InfoWafer p_infoWafer
        {
            get { return _infoWafer; }
            set
            {
                m_sInfoWafer = (value == null) ? "" : value.p_id;
                _infoWafer = value;
                if (m_reg != null) m_reg.Write("sInfoWafer", m_sInfoWafer);
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

        #region InfoWafer UI
        InfoWaferChild_UI m_ui;
        void InitInfoWaferUI()
        {
            m_ui = new InfoWaferChild_UI();
            m_ui.Init(this);
            m_aTool.Add(m_ui);
        }
        #endregion

        #region Camera Align
        public enum ePosAlign
        {
            Ready, // Z: -175000, Y: -250000
            Align // Z: 0 , Y: 0
        }
        void InitPosAlign()
        {
            m_axisCamAlign.AddPos(Enum.GetNames(typeof(ePosAlign)));
        }

        public enum ePosAlignRotate
        {
            Ready, // 0
            Align // 360000
        }
        void InitPosAlignRotate()
        {
            m_axisRotate.AddPos(Enum.GetNames(typeof(ePosAlignRotate)));
        }

        double m_mmWaferSize = 300;
        double m_umWaferThickness = 700;
        public string AxisMoveAlign(ePosAlign pos, bool bWait)
        {
            if (pos == ePosAlign.Ready) return AxisMoveAlign(pos, 0, 0, bWait);
            double dx = m_mmWaferSize - p_infoWafer.p_mmWaferSize;
            double dz = (m_umWaferThickness - p_infoWafer.p_umThickness) / 1000.0;
            return AxisMoveAlign(pos, dx, dz, bWait);
        }

        string AxisMoveAlign(ePosAlign pos, double xOffset, double zOffset, bool bWait)
        {
            m_axisCamAlign.StartMove(pos, new RPoint(xOffset, zOffset));
            if (bWait == false) return "OK";
            return m_axisCamAlign.WaitReady();
        }

        void RunTreeWafer(Tree tree)
        {
            m_mmWaferSize = tree.Set(m_mmWaferSize, m_mmWaferSize, "Size", "Default Wafer Size (mm)");
            m_umWaferThickness = tree.Set(m_umWaferThickness, m_umWaferThickness, "Thickness", "Default Wafer Thickness (um)");
        }
        #endregion

        #region Camera OCR
        public enum ePosOCR
        {
            Ready,
            OCR,
        }

        void InitPosOCRX()
        {
            m_axisCamOCRX.AddPos(Enum.GetNames(typeof(ePosOCR)));
        }

        public string AxisMoveOCRX(ePosOCR pos, double mmOCR)
        {
            double dx = m_mmWaferSize - p_infoWafer.p_mmWaferSize + mmOCR;
            m_axisCamOCRX.StartMove(pos, dx);
            return "OK";
        }

        void InitPosOCRTopZ()
        {
            m_axisCamOCRTopZ.AddPos(Enum.GetNames(typeof(ePosOCR)));
        }

        public string AxisMoveOCRTopZ(ePosOCR pos, double mmOCR)
        {
            double dx = m_mmWaferSize - p_infoWafer.p_mmWaferSize + mmOCR;
            m_axisCamOCRTopZ.StartMove(pos, dx);
            return "OK";
        }

        void InitPosOCRBotZ()
        {
            m_axisCamOCRBotZ.AddPos(Enum.GetNames(typeof(ePosOCR)));
        }

        public string AxisMoveOCRBotZ(ePosOCR pos, double mmOCR)
        {
            double dx = m_mmWaferSize - p_infoWafer.p_mmWaferSize + mmOCR;
            m_axisCamOCRBotZ.StartMove(pos, dx);
            return "OK";
        }
        #endregion

        #region Axis Rotate
        int m_lRotate = 360000;
        int m_nRotateBack = 0;
        string Rotate(double fPulse)
        {
            double fNow = m_axisRotate.p_posCommand;
            while ((fNow - fPulse) > m_lRotate / 2) fNow -= m_lRotate;
            while ((fPulse - fNow) > m_lRotate / 2) fNow += m_lRotate;
            SetRotatePosition(fNow);
            if (fPulse < fNow)
            {
                m_axisRotate.StartMove(fPulse - m_nRotateBack);
                if (Run(m_axisRotate.WaitReady())) return p_sInfo;
            }
            m_axisRotate.StartMove(fPulse);
            if (Run(m_axisRotate.WaitReady())) return p_sInfo;
            return "OK";
        }

        void SetRotatePosition(double fPos)
        {
            m_axisRotate.SetCommandPosition(fPos);
            m_axisRotate.SetActualPosition(fPos);
        }

        string RotateDeg(double dDeg)
        {
            if (m_aoiMax == null) return "Need Find Notch";
            double posNotch = m_aoiMax.GetNotchPos(m_lRotate);
            return Rotate(posNotch + dDeg * m_lRotate / 360);
        }

        void RunTreeRotate(Tree tree)
        {
            m_lRotate = tree.Set(m_lRotate, m_lRotate, "PpR", "Pulse per Round (pulse)");
            m_nRotateBack = tree.Set(m_nRotateBack, m_nRotateBack, "Rotate Back", "Rotate Back Pulse (pulse)");
            m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Stage Vacuum On Timeout (sec)");
            m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Stage Vacuum Blow Time (sec)");
            RunTreeAlign(tree.GetTree("Grab"));
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

        public List<string> p_asChildSlot { get { return null; } }

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

        public string BeforeGet(int nID)
        {
            //if (p_infoWafer == null) return m_id + " BeforeGet : InfoWafer = null";
            return CheckGetPut();
        }

        public string BeforePut(int nID)
        {
            if (p_infoWafer != null) return p_id + " BeforePut : InfoWafer != null";
            SetRotatePosition(0);
            return CheckGetPut();
        }

        public string AfterGet(int nID)
        {
            return CheckGetPut();
        }

        public string AfterPut(int nID)
        {
            return "OK";
        }

        string CheckGetPut()
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            return "OK";
        }

        enum eCheckWafer
        {
            InfoWafer,
            Sensor
        }
        eCheckWafer m_eCheckWafer = eCheckWafer.Sensor;
        public bool IsWaferExist(int nID)
        {
            switch (m_eCheckWafer)
            {
                case eCheckWafer.Sensor: return m_diWaferExist.p_bIn;
                default: return (p_infoWafer != null);
            }
        }

        InfoWafer.WaferSize m_waferSize;
        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }
        #endregion

        #region Override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup"));
        }

        void RunTreeSetup(Tree tree)
        {
            m_eCheckWafer = (eCheckWafer)tree.Set(m_eCheckWafer, m_eCheckWafer, "CheckWafer", "CheckWafer");
            m_szAlignROI = tree.Set(m_szAlignROI, m_szAlignROI, "Cameara AOI", "Camera AOI (pixel)");
            RunTreeWafer(tree.GetTree("Default Wafer", false));
            RunTreeRotate(tree.GetTree("Rotate", false));
            m_waferSize.RunTree(tree.GetTree("Wafer Size", false), true);
            m_flipper.RunTree(tree.GetTree("Flipper"));
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }
        #endregion

        #region AOI & Memory
        enum eAOI
        {
            Grab,
            Exact
        }
        const int c_lGrab = 100;
        const int c_lExact = 5;

        MemoryGroup m_memoryGroup;
        MemoryData m_memoryGrab;
        MemoryData m_memoryExact;
        Aligner_ATI_AOI m_aoi;
        List<Aligner_ATI_AOI.AOI> m_aGrabAOI = new List<Aligner_ATI_AOI.AOI>();
        List<Aligner_ATI_AOI.AOI> m_aExactAOI = new List<Aligner_ATI_AOI.AOI>();

        void InitMemory()
        {
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            m_memoryGrab = m_memoryGroup.CreateMemory("Grab", c_lGrab, 1, m_szAlignROI);
            for (int n = 0; n < c_lGrab; n++) m_aGrabAOI.Add(new Aligner_ATI_AOI.AOI(p_id + "Grab", m_memoryGrab, n));
            m_memoryExact = m_memoryGroup.CreateMemory("Exact", c_lExact, 1, m_szAlignROI);
            for (int n = 0; n < c_lExact; n++) m_aExactAOI.Add(new Aligner_ATI_AOI.AOI(p_id + "Exact", m_memoryExact, n));
        }
        #endregion

        #region Align Grab
        double m_secGrabAcc = 0.4;
        double m_secGrab = 5;
        public string AlignGrab(Aligner_ATI_AOI.Data data)
        {
            StopWatch stopWatch = new StopWatch();
            m_aoi.m_data = data;
            m_aoiMax = null;

            RunVacuum(true);
            p_bLightCoaxial = true;
            p_bLightSide = false;

            double vGrabPulse = m_lRotate / m_secGrab;
            double pulseAcc = vGrabPulse * (m_secGrabAcc + 0.1) / 2;
            SetRotatePosition(-pulseAcc);
            if (Run(AxisMoveAlign(ePosAlign.Align, true))) return p_sInfo;
            m_camAlign.SetMemoryData(m_memoryGrab);

            double posTrigger = 0;
            double dpTrigger = m_lRotate / c_lGrab;
            double dpMax = dpTrigger / 4;
            m_axisRotate.StartMove(m_lRotate + pulseAcc, vGrabPulse, m_secGrabAcc, m_secGrabAcc);
            for (int n = 0; n < c_lGrab;)
            {
                double dp = m_axisRotate.p_posCommand - posTrigger;
                if (dp > dpMax) return "Run Align Grab Time Error";
                if (dp >= 0)
                {
                    m_aGrabAOI[n].m_posGrab = m_axisRotate.p_posCommand;
                    m_camAlign.GrabOne(n);
                    if (data != null) m_aoi.StartInspect(m_aGrabAOI[n]);
                    posTrigger += dpTrigger;
                    n++;
                }
                else Thread.Sleep(1);
            }
            m_log.Info("Align Grab Done " + (stopWatch.ElapsedMilliseconds / 1000.0).ToString(".0") + " sec");
            if (Run(m_axisRotate.WaitReady())) return p_sInfo;
            p_bLightCoaxial = false;
            return "OK";
        }

        Aligner_ATI_AOI.AOI m_aoiMax = null;
        double m_secInspect = 3;
        public string FindMaxNotch()
        {
            StopWatch stopWatch = new StopWatch();
            int msInspect = (int)(1000 * m_secInspect);
            while (m_aoi.m_qAOI.Count > 0)
            {
                if (stopWatch.ElapsedMilliseconds > msInspect) return "Inspect Timeout";
                Thread.Sleep(10);
            }
            double m_fMaxScore = 0;
            foreach (Aligner_ATI_AOI.AOI aoi in m_aGrabAOI)
            {
                if (m_fMaxScore < aoi.p_fScore)
                {
                    m_fMaxScore = aoi.p_fScore;
                    m_aoiMax = aoi;
                }
            }
            if (m_aoiMax == null) return "Need Find Notch";
            return "OK";
        }

        public double m_posAlign = 0; 
        public string RunAlignExact(double degAccuracy)
        {
            if (m_aoiMax == null) return "Need Find Notch";
            m_camAlign.SetMemoryData(m_memoryExact);
            for (int n = 0; n < c_lExact; n++)
            {
                double posAlign = m_aoiMax.GetNotchPos(m_lRotate);
                string sRotate = Rotate(posAlign);
                if (sRotate != "OK") return "Run Align Exact Rotate : " + sRotate;
                m_aExactAOI[n].m_posGrab = m_axisRotate.p_posCommand;
                m_camAlign.GrabOne(n);
                m_aoi.Inspect(m_aExactAOI[n]);
                m_aoiMax = m_aExactAOI[n];
                if ((m_aoiMax.m_maxNotch != null) && (Math.Abs(m_aoiMax.m_maxNotch.m_degNotch) < degAccuracy))
                {
                    m_posAlign = m_aoiMax.m_posGrab; 
                    return "OK";
                }
            }
            return "Can't Find Notch"; //forget
        }

        void RunTreeAlign(Tree tree)
        {
            m_secGrab = tree.Set(m_secGrab, m_secGrab, "Speed", "Rotate Grab Speed (sec)");
            m_secGrabAcc = tree.Set(m_secGrabAcc, m_secGrabAcc, "Acc", "Align Grab Rotate Acceleration Time (sec)");
            m_secInspect = tree.Set(m_secInspect, m_secInspect, "Inspect", "Inspect Timeout (sec)");
        }
        #endregion

        #region OCR Grab
        public string RunOCR(double dTheta, double dR)
        {
            m_doLightCoaxial.Write(true);
            AxisMoveOCRX(ePosOCR.OCR, dR);
            AxisMoveOCRTopZ(ePosOCR.OCR, dR);
            AxisMoveOCRBotZ(ePosOCR.OCR, dR);
            if (Run(Rotate(dTheta))) return p_sInfo;
            if (Run(m_axisCamOCRX.WaitReady())) return p_sInfo;
            if (Run(m_axisCamOCRTopZ.WaitReady())) return p_sInfo;
            if (Run(m_axisCamOCRBotZ.WaitReady())) return p_sInfo;
            if (Run(m_camOCR.SendReadOCR())) return p_sInfo;
            AxisMoveOCRX(ePosOCR.Ready, 0);
            AxisMoveOCRTopZ(ePosOCR.Ready, 0);
            AxisMoveOCRBotZ(ePosOCR.Ready, 0);

            return "OK";
        }
        #endregion

        public Aligner_ATI(string id, IEngineer engineer)
        {
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            m_aoi = new Aligner_ATI_AOI(m_log);
            base.InitBase(id, engineer);
            InitInfoWaferUI();
        }

        public override void ThreadStop()
        {
            m_aoi.ThreadStop();
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
            AddModuleRunList(new Run_Flipper(this), true, "Flipper Get&Put Wafer");
            AddModuleRunList(new Run_Inspect(this), false, "Inspect Align Camera Image for Find Notch");
            AddModuleRunList(new Run_Grab(this), false, "Run Align Grab");
            AddModuleRunList(new Run_Align(this), true, "Run Align with Inspect");
        }

        public class Run_Delay : ModuleRunBase
        {
            Aligner_ATI m_module;
            public Run_Delay(Aligner_ATI module)
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

        public class Run_Flipper : ModuleRunBase
        {
            Aligner_ATI m_module;
            public Run_Flipper(Aligner_ATI module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            enum eFlipper
            {
                Put,
                Get,
                InversePut,
                InverseGet,
                RnR,
                Recovery
            }
            eFlipper m_eFlipper = eFlipper.Get;
            public override ModuleRunBase Clone()
            {
                Run_Flipper run = new Run_Flipper(m_module);
                run.m_eFlipper = m_eFlipper;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eFlipper = (eFlipper)tree.Set(m_eFlipper, m_eFlipper, "Flipper", "Flipper Run", bVisible);
            }
            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                switch (m_eFlipper)
                {
                    case eFlipper.Put: return m_module.RunPut();
                    case eFlipper.Get: return m_module.RunGet();
                    case eFlipper.InversePut: return m_module.RunInversePut();
                    case eFlipper.InverseGet: return m_module.RunInverseGet();
                    case eFlipper.RnR: return m_module.RunRNR();
                    case eFlipper.Recovery: return m_module.RunRecovery();
                }
                return "OK";
            }
        }

        public class Run_Inspect : ModuleRunBase
        {
            Aligner_ATI m_module;
            public Run_Inspect(Aligner_ATI module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public Aligner_ATI_AOI.Data m_aoiData = new Aligner_ATI_AOI.Data();
            public override ModuleRunBase Clone()
            {
                Run_Inspect run = new Run_Inspect(m_module);
                run.m_eAOI = m_eAOI;
                run.m_nAOI = m_nAOI;
                run.m_aoiData.Clone(m_aoiData);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                RunTreeAOI(tree.GetTree("AOI", true, bVisible), bVisible);
                m_aoiData.RunTree(tree.GetTree("Inspect", true, bVisible), bVisible, bRecipe);
            }

            eAOI m_eAOI = eAOI.Grab;
            int m_nAOI = 0;
            void RunTreeAOI(Tree tree, bool bVisible)
            {
                m_eAOI = (eAOI)tree.Set(m_eAOI, m_eAOI, "AOI", "Select AOI", bVisible);
                m_nAOI = tree.Set(m_nAOI, m_nAOI, "Index", "AOI Index", bVisible);
            }

            Aligner_ATI_AOI.AOI GetAOI()
            {
                if (m_nAOI < 0) return null;
                List<Aligner_ATI_AOI.AOI> aAOI = (m_eAOI == eAOI.Grab) ? m_module.m_aGrabAOI : m_module.m_aExactAOI;
                return (m_nAOI < aAOI.Count) ? aAOI[m_nAOI] : null;
            }

            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                Aligner_ATI_AOI.AOI aoi = GetAOI();
                if (aoi == null) return "AOI not Selected";
                m_module.m_aoi.m_data = m_aoiData;
                aoi.m_maxNotch = null;
                return m_module.m_aoi.Inspect(aoi);
            }
        }

        public class Run_Grab : ModuleRunBase
        {
            Aligner_ATI m_module;
            public Run_Grab(Aligner_ATI module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bGrab = true;
            public override ModuleRunBase Clone()
            {
                Run_Grab run = new Run_Grab(m_module);
                run.m_bGrab = m_bGrab;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bGrab = tree.Set(m_bGrab, m_bGrab, "Grab", "Grab for Align", bVisible, true);
            }
            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                return m_module.AlignGrab(null);
            }
        }

        public class Run_Align : ModuleRunBase
        {
            Aligner_ATI m_module;
            Run_Inspect m_inspect;
            public Run_Align(Aligner_ATI module)
            {
                m_module = module;
                InitModuleRun(module);
                m_inspect = new Run_Inspect(module);
            }

            double m_degAccuracy = 0.08;
            public override ModuleRunBase Clone()
            {
                Run_Align run = new Run_Align(m_module);
                run.m_degAccuracy = m_degAccuracy;
                run.m_inspect = (Run_Inspect)m_inspect.Clone();
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_degAccuracy = tree.Set(m_degAccuracy, m_degAccuracy, "Accuacy", "Degree of Align Accuracy", bVisible);
                m_inspect.m_aoiData.RunTree(tree.GetTree("Inspect", true, bVisible), bVisible, bRecipe);
            }

            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                if (m_module.Run(m_module.AlignGrab(m_inspect.m_aoiData))) return p_sInfo;
                if (m_module.Run(m_module.FindMaxNotch())) return p_sInfo;
                if (m_module.Run(m_module.RunAlignExact(m_degAccuracy))) return p_sInfo; 
                return "OK";
            }
        }
        #endregion
    }
}
