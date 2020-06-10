﻿using RootTools;
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
        public DIO_I m_diPlaced;
        public DIO_I m_diPresent;
        public DIO_I m_diLoad;
        public DIO_I m_diUnload; 
        public OHT_Semi m_OHT;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_diPlaced, this, "Placed");
            p_sInfo = m_toolBox.Get(ref m_diPresent, this, "Present");
            p_sInfo = m_toolBox.Get(ref m_diLoad, this, "Load");
            p_sInfo = m_toolBox.Get(ref m_diUnload, this, "Unload");
            p_sInfo = m_toolBox.Get(ref m_OHT, this, m_infoPod, "OHT");
        }
        #endregion

        #region DIO Function
        public bool CheckPlaced()
        {
            GemCarrierBase.ePresent present;
            if (m_diPlaced.p_bIn != m_diPresent.p_bIn) present = GemCarrierBase.ePresent.Unknown;
            else present = m_diPlaced.p_bIn ? GemCarrierBase.ePresent.Exist : GemCarrierBase.ePresent.Empty;
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
                //p_sInfo = WriteCmd(eCmd.Init);
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
            AddModuleRunList(new Run_PodOpen(this), false, "Pod Open");
            m_runLoad = AddModuleRunList(new Run_Load(this), false, "Load Pod to Work Position");
            m_runUnLoad = AddModuleRunList(new Run_Unload(this), false, "Unload Pod from Work Position");
            AddModuleRunList(new Run_PodClose(this), false, "Pod Close");
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
                m_nCh = tree.Set(m_nCh, m_nCh, "Channel", "RFID Channel"); 
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

        public class Run_PodOpen : ModuleRunBase
        {
            Loadport m_module;
            InfoPod m_infoPod;
            public Run_PodOpen(Loadport module)
            {
                m_module = module;
                m_infoPod = module.m_infoPod;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_PodOpen run = new Run_PodOpen(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                //if (m_module.Run(m_module.WriteCmd(eCmd.PodOpen))) return p_sInfo;
                //m_infoPod.p_eState = InfoPod.eState.Placed;
                return "OK";
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
                //if (m_module.Run(m_module.WriteCmd(eCmd.Load))) return p_sInfo;
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
                //if (m_module.Run(m_module.WriteCmd(eCmd.Unload))) return p_sInfo;
                m_infoPod.p_eState = InfoPod.eState.Placed;
                m_module.m_ceidClose.Send();
                m_module.m_ceidUnload.Send();
                return "OK";
            }
        }

        public class Run_PodClose : ModuleRunBase
        {
            Loadport m_module;
            InfoPod m_infoPod;
            public Run_PodClose(Loadport module)
            {
                m_module = module;
                m_infoPod = module.m_infoPod;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_PodClose run = new Run_PodClose(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                //if (m_module.Run(m_module.WriteCmd(eCmd.PodClose))) return p_sInfo;
                //m_infoPod.p_eState = InfoPod.eState.Placed;
                return "OK";
            }
        }
        #endregion
    }
}
