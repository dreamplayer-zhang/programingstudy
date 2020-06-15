using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using RootTools.OHT.Semi;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_Vega.Module
{
    public class Loadport : ModuleBase, IRobotChild
    {
        #region ToolBox
        Axis m_axisZ;
        Axis m_axisTheta;
        AxisXY m_axisPodLifter; 
        AxisXY m_axisReticleLifter;
        public DIO_IO m_dioPlaced;
        public DIO_IO m_dioPresent;
        public DIO_I m_diFront;
        public DIO_I m_diBack;
        public DIO_I m_diReticle;
        public DIO_I m_diTilt;
        public DIO_I m_diEmpty;
        public DIO_O m_doManual;
        public DIO_O m_doAuto;
        public DIO_IO m_dioLoad;
        public DIO_IO m_dioUnload;
        public DIO_O m_doAlarm;
//        public DIO_Os m_doPodCylinder;
        public OHT_Semi m_OHT;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisZ, this, "Z");
            p_sInfo = m_toolBox.Get(ref m_axisTheta, this, "Theta");
            p_sInfo = m_toolBox.Get(ref m_axisPodLifter, this, "Pod Lifter");
            p_sInfo = m_toolBox.Get(ref m_axisReticleLifter, this, "Reticle Lifter");
            p_sInfo = m_toolBox.Get(ref m_dioPlaced, this, "Placed");
            p_sInfo = m_toolBox.Get(ref m_dioPresent, this, "Present");
            p_sInfo = m_toolBox.Get(ref m_diFront, this, "Front");
            p_sInfo = m_toolBox.Get(ref m_diBack, this, "Back");
            p_sInfo = m_toolBox.Get(ref m_diReticle, this, "Reticle");
            p_sInfo = m_toolBox.Get(ref m_diTilt, this, "Tilt");
            p_sInfo = m_toolBox.Get(ref m_diEmpty, this, "Empty");
            p_sInfo = m_toolBox.Get(ref m_doManual, this, "Manual");
            p_sInfo = m_toolBox.Get(ref m_doAuto, this, "Auto");
            p_sInfo = m_toolBox.Get(ref m_dioLoad, this, "Load");
            p_sInfo = m_toolBox.Get(ref m_dioUnload, this, "Unload");
            p_sInfo = m_toolBox.Get(ref m_doAlarm, this, "Alarm");
//            p_sInfo = m_toolBox.Get(ref m_doPodCylinder, this, "Alarm", Enum.GetNames(typeof(ePodCylinder)));
            p_sInfo = m_toolBox.Get(ref m_OHT, this, m_infoPod, "OHT");
            if (bInit)
            {
                InitPosZ();
                InitPosPod();
                InitPosPodLifter();
                InitPosReticleLifter(); 
            }
        }
        #endregion

        #region AxisZ
        public enum ePosZ
        {
            Ready,
            InnerPod,
            ReticleReady,
            Reticle, 
            Load
        }
        void InitPosZ()
        {
            m_axisZ.AddPos(Enum.GetNames(typeof(ePosZ)));
            m_axisZ.AddPosDone(); 
        }

        public string MoveZ(ePosZ pos)
        {
            string sMove = m_axisZ.Move(pos);
            if (sMove != "OK") return sMove;
            return m_axisZ.WaitReady(); 
        }
        #endregion

        #region AxisTheta
        public enum ePosTheta
        { 
            Open,
            Close
        }
        void InitPosPod()
        {
            m_axisTheta.AddPos(Enum.GetNames(typeof(ePosTheta)));
            m_axisTheta.AddPosDone();
        }

        public string MoveTheta(ePosTheta pos)
        {
            string sMove = m_axisTheta.Move(pos);
            if (sMove != "OK") return sMove;
            return m_axisTheta.WaitReady();
        }
        #endregion

        #region AxisPodLifter
        public enum ePosPodLifter
        {
            Ready,
            Lifting
        }
        void InitPosPodLifter()
        {
            m_axisPodLifter.AddPos(Enum.GetNames(typeof(ePosPodLifter)));
            m_axisPodLifter.AddPosDone();
        }

        public string MovePodLifter(ePosPodLifter pos)
        {
            string sMove = m_axisPodLifter.Move(pos);
            if (sMove != "OK") return sMove;
            return m_axisPodLifter.WaitReady();
        }
        #endregion

        #region AxisReticleLifter
        public enum ePosReticleLifter
        {
            Ready,
            Mid,
            Lifting
        }
        void InitPosReticleLifter()
        {
            m_axisReticleLifter.AddPos(Enum.GetNames(typeof(ePosReticleLifter)));
            m_axisReticleLifter.AddPosDone();
        }

        public string MoveReticleLifter(ePosReticleLifter pos)
        {
            string sMove = m_axisReticleLifter.Move(pos);
            if (sMove != "OK") return sMove;
            return m_axisReticleLifter.WaitReady();
        }
        #endregion

        #region DIO Function
        public bool CheckPlaced()
        {
            GemCarrierBase.ePresent present;
            if (m_dioPlaced.p_bIn != m_dioPresent.p_bIn) present = GemCarrierBase.ePresent.Unknown;
            else present = m_dioPlaced.p_bIn ? GemCarrierBase.ePresent.Exist : GemCarrierBase.ePresent.Empty;
            if (m_infoPod.CheckPlaced(present) != "OK")
            {
                m_alidPlaced.p_sMsg = "Placed Sensor Remain Checked while Pod State = " + m_infoPod.p_eState;
                m_alidPlaced.p_bSet = true;
            }
            switch (m_infoPod.p_ePresentSensor)
            {
                case GemCarrierBase.ePresent.Empty: m_svidPlaced.p_value = false; break;
                case GemCarrierBase.ePresent.Exist: m_svidPlaced.p_value = true; break;
            }
            return m_svidPlaced.p_value;
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

        public InfoReticle p_infoReticle
        {
            get { return m_infoPod.p_infoReticle; }
            set 
            { 
                m_infoPod.p_infoReticle = value;
            }
        }
        public string IsGetOK(ref int posRobot)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
//            if (m_diDoorOpen.p_bIn == false) return m_id + " Door Closed";
            return m_infoPod.IsGetOK(ref posRobot);
        }

        public string IsPutOK(ref int posRobot, InfoReticle infoReticle)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
//            if (m_diDoorOpen.p_bIn == false) return m_id + " Door Closed";
            return m_infoPod.IsPutOK(ref posRobot);
        }

        public string BeforeGet()
        {
            if (p_infoReticle == null) return p_id + " BeforeGet : InfoWafer = null";
            return IsRunOK();
        }

        public string BeforePut()
        {
            if (p_infoReticle != null) return p_id + " BeforePut : InfoWafer != null";
            return IsRunOK();
        }

        public string AfterGet()
        {
            return IsRunOK();
        }

        public string AfterPut()
        {
            return IsRunOK();
        }

        public bool IsReticleExist(bool bIgnoreExistSensor = false)
        {
            if (m_infoPod.p_ePresentSensor != GemCarrierBase.ePresent.Exist) return false; 
            return (m_infoPod.p_infoReticle != null); 
        }

        string IsRunOK()
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
//            if (m_diDoorOpen.p_bIn == false) return m_id + " Door Closed";
            return m_infoPod.IsRunOK();
        }

        public void RunTeachTree(Tree tree)
        {
            m_infoPod.RunTreeTeach(tree);
        }

        public void ReadInfoReticle_Registry()
        {
            m_infoPod.ReadInfoReticle_Registry();
        }
        #endregion

        #region Load & Unload
        public string RunLoad()
        {
            if (m_axisZ.IsInPos(ePosZ.Ready) == false) return "AxisZ Position not Ready";
            if (m_axisTheta.IsInPos(ePosTheta.Close) == false) return "AxisTheta Position not Close";
            if (m_axisPodLifter.IsInPos(ePosPodLifter.Ready) == false) return "AxisPodLifter Position not Ready";
            if (m_axisReticleLifter.IsInPos(ePosReticleLifter.Ready) == false) return "AxisReticleLifter Position not Ready";
            if (Run(MoveTheta(ePosTheta.Open))) return p_sInfo;
            if (Run(MoveZ(ePosZ.InnerPod))) return p_sInfo;
            if (Run(MovePodLifter(ePosPodLifter.Lifting))) return p_sInfo;
            if (Run(MoveZ(ePosZ.ReticleReady))) return p_sInfo;
            if (Run(MoveReticleLifter(ePosReticleLifter.Mid))) return p_sInfo;
            if (Run(MoveZ(ePosZ.Reticle))) return p_sInfo;
            if (Run(MoveReticleLifter(ePosReticleLifter.Lifting))) return p_sInfo;
            if (Run(MoveZ(ePosZ.Load))) return p_sInfo;
            return "OK"; 
        }

        public string RunUnload()
        {
            if (m_axisZ.IsInPos(ePosZ.Load) == false) return "AxisZ Position not Load";
            if (m_axisTheta.IsInPos(ePosTheta.Open) == false) return "AxisTheta Position not Open";
            if (m_axisPodLifter.IsInPos(ePosPodLifter.Lifting) == false) return "AxisPodLifter Position not Lifting";
            if (m_axisReticleLifter.IsInPos(ePosReticleLifter.Lifting) == false) return "AxisReticleLifter Position not Lifting";
            if (Run(MoveZ(ePosZ.Reticle))) return p_sInfo;
            if (Run(MoveReticleLifter(ePosReticleLifter.Mid))) return p_sInfo;
            if (Run(MoveZ(ePosZ.ReticleReady))) return p_sInfo;
            if (Run(MoveReticleLifter(ePosReticleLifter.Ready))) return p_sInfo;
            if (Run(MoveZ(ePosZ.InnerPod))) return p_sInfo;
            if (Run(MovePodLifter(ePosPodLifter.Ready))) return p_sInfo;
            if (Run(MoveZ(ePosZ.Ready))) return p_sInfo;
            if (Run(MoveTheta(ePosTheta.Close))) return p_sInfo;
            return "OK"; 
        }
        #endregion

        #region override
        public override void Reset()
        {
            p_eState = eState.Init;
            base.Reset();
        }

        public override void ButtonHome()
        {
            base.ButtonHome();
        }
        #endregion

        #region State Home & Ready
        public override string StateHome()
        {
            if (EQ.p_bSimulate == false)
            {
                p_sInfo = StateHome(m_axisPodLifter.p_axisX, m_axisPodLifter.p_axisY, m_axisReticleLifter.p_axisX, m_axisReticleLifter.p_axisY);
                if (p_sInfo != "OK") return p_sInfo;
                p_sInfo = StateHome(m_axisZ.p_axis, m_axisTheta.p_axis);
                if (p_sInfo != "OK") return p_sInfo; 
            }
            m_infoPod.AfterHome(); 
            return "OK";
        }

        public override string StateReady()
        {
            CheckPlaced(); 
            if (m_infoPod.m_bReqReadCarrierID)
            {
                m_infoPod.m_bReqReadCarrierID = false;
                StartRun(m_runReadPodID);
            }
            if (m_infoPod.m_bReqLoad)
            {
                m_infoPod.m_bReqLoad = false;
                StartRun(m_runLoad);
            }
            if (m_infoPod.m_bReqUnload)
            {
                m_infoPod.m_bReqUnload = false;
                StartRun(m_runUnLoad);
            }
            return "OK";
        }
        #endregion

        #region GAF
        SVID m_svidPlaced;
        CEID m_ceidLoad;
        public CEID m_ceidUnload;
        CEID m_ceidOpen;
        CEID m_ceidClose; 
        ALID m_alidPlaced;
        void InitGAF()
        {
            m_svidPlaced = m_gaf.GetSVID(this, "Placed");
            m_ceidLoad = m_gaf.GetCEID(this, "Load");
            m_ceidUnload = m_gaf.GetCEID(this, "Unload");
            m_ceidOpen = m_gaf.GetCEID(this, "Door Open");
            m_ceidClose = m_gaf.GetCEID(this, "Door Close");
            m_alidPlaced = m_gaf.GetALID(this, "Placed Sensor Error", "Placed & Plesent Sensor Should be Checked"); 
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
        }
        #endregion

        public InfoPod m_infoPod;
        public Vega.RFID m_RFID = null; 
        public Loadport(string id, string sLocID, IEngineer engineer)
        {
            p_id = id;
            m_log = LogView.GetLog(id, id);
            m_infoPod = new InfoPod(this, sLocID, engineer);
            m_RFID = ((Vega_Engineer)engineer).m_handler.m_vega.m_RFID; 
            m_aTool.Add(m_infoPod);
            base.InitBase(id, engineer);
            InitGAF();
            if (m_gem != null) m_gem.OnGemRemoteCommand += M_gem_OnRemoteCommand;
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        private void M_gem_OnRemoteCommand(string sCmd, Dictionary<string, string> dicParam, long[] pnResult)
        {
            //
        }

        public ModuleRunBase GetRunUndocking()
        {
            return CloneModuleRun("Undocking");
        }

        #region ModuleRun
        public ModuleRunBase m_runReadPodID;
        public ModuleRunBase m_runLoad;
        ModuleRunBase m_runUnLoad;
        protected override void InitModuleRuns()
        {
            m_runReadPodID = AddModuleRunList(new Run_ReadRFID(this), false, "Read RFID");
            m_runLoad = AddModuleRunList(new Run_Load(this), false, "Load Pod to Work Position");
            m_runUnLoad = AddModuleRunList(new Run_Unload(this), false, "Unload Pod from Work Position");
        }

        public class Run_ReadRFID : ModuleRunBase
        {
            Loadport m_module;
            public Run_ReadRFID(Loadport module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            int m_nCh = 1; 
            string m_sSimulCarrierID = "CarrierID";
            public override ModuleRunBase Clone()
            {
                Run_ReadRFID run = new Run_ReadRFID(m_module);
                run.m_nCh = m_nCh; 
                run.m_sSimulCarrierID = m_sSimulCarrierID;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nCh = tree.Set(m_nCh, m_nCh, "Channel", "RFID Channel", bVisible); 
                m_sSimulCarrierID = tree.Set(m_sSimulCarrierID, m_sSimulCarrierID, "Simulation CarrierID", "CarrierID When p_bSimulation", bVisible && EQ.p_bSimulate);
            }

            public override string Run()
            {
                string sResult = "OK"; 
                string sCarrierID = "";
                if (EQ.p_bSimulate) sCarrierID = m_sSimulCarrierID; 
                else
                {
                    sResult = m_module.m_RFID.ReadRFID((byte)m_nCh, out sCarrierID);
                    m_module.m_infoPod.p_sCarrierID = (sResult == "OK") ? sCarrierID : ""; 
                }
                m_module.m_infoPod.SendCarrierID(sCarrierID); 
                return sResult; 
            }
        }

        public class Run_Load : ModuleRunBase
        {
            Loadport m_module;
            InfoPod m_infoPod;
            public Run_Load(Loadport module)
            {
                m_module = module;
                m_infoPod = module.m_infoPod;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Load run = new Run_Load(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                string sRun = m_module.RunLoad();
                if (sRun != "OK") return "RunLoad Error : " + sRun; 
                m_infoPod.p_eState = InfoPod.eState.Load;
                m_module.m_ceidLoad.Send();
                m_module.m_ceidOpen.Send();
                m_module.m_infoPod.SetInfoReticleExist(); 
                m_module.m_infoPod.SendSlotMap();
                return "OK";
            }
        }

        public class Run_Unload : ModuleRunBase
        {
            Loadport m_module;
            InfoPod m_infoPod;
            public Run_Unload(Loadport module)
            {
                m_module = module;
                m_infoPod = module.m_infoPod;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Unload run = new Run_Unload(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                string sRun = m_module.RunUnload();
                if (sRun != "OK") return "RunUnload Error : " + sRun;
                m_infoPod.p_eState = InfoPod.eState.Placed;
                m_module.m_ceidClose.Send();
                m_module.m_ceidUnload.Send();
                return "OK";
            }
        }
        #endregion
    }
}
