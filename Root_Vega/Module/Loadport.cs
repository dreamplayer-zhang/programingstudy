using RootTools;
using RootTools.Control;
using RootTools.DMC;
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
        DMCControl m_dmc;
        OHT_Semi m_OHT;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_diPlaced, this, "Placed");
            p_sInfo = m_toolBox.Get(ref m_diPresent, this, "Present");
            p_sInfo = m_toolBox.Get(ref m_diLoad, this, "Load");
            p_sInfo = m_toolBox.Get(ref m_diUnload, this, "Unload");
            p_sInfo = m_toolBox.Get(ref m_dmc, this, "DMC");
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
                m_alidPlaced.p_bSet = true;
                m_alidPlaced.p_sMsg = "Placed Sensor Remain Checked while Pod State = " + m_infoPod.p_eState;
            }
            switch (m_infoPod.p_ePresentSensor)
            {
                case GemCarrierBase.ePresent.Empty: m_svidPlaced.p_value = false; break;
                case GemCarrierBase.ePresent.Exist: m_svidPlaced.p_value = true; break;
            }
            return m_svidPlaced.p_value;
        }
        #endregion

        #region Protocol
        public enum eCmd
        {
            None,
            PodOpen,
            Load,
            Unload,
            PodClose,
            Init,
        };
        public class Command
        {
            public eCmd m_cmd;
            string m_sCmd;
            double m_secRun = 10;

            public void RunTree(Tree tree)
            {
                m_sCmd = tree.Set(m_sCmd, m_sCmd, "Command", "DMC Excute Command String");
                m_secRun = tree.Set(m_secRun, m_secRun, "Timeout", "DMC Run Timeout (sec)"); 
            }

            public string Run()
            {
                string sCmd = m_cmd.ToString(); 
                string sSend = m_dmc.Send(m_sCmd);
                if (sSend != "OK") return sSend;
                StopWatch sw = new StopWatch();
                int msWait = (int)(1000 * m_secRun);
                Thread.Sleep(100);
                while (m_dmc.p_eState == DMCControl.eState.Running)
                {
                    Thread.Sleep(10);
                    if (sw.ElapsedMilliseconds > msWait)
                    {
                        //forget
                        return "DMC Run Timeout : " + sCmd;
                    }
                }
                switch (m_dmc.p_eState)
                {
                    case DMCControl.eState.Error: return "DMC Run Error : " + sCmd;
                    case DMCControl.eState.Running: return "DMC Run Timeout : " + sCmd;
                    default: return "OK";
                }
            }

            DMCControl m_dmc;
            public Command(eCmd cmd, DMCControl dmc)
            {
                m_cmd = cmd;
                m_sCmd = cmd.ToString().ToLower();
                m_dmc = dmc; 
            }
        }

        List<Command> m_aCmd = new List<Command>(); 
        Command GetCommand(eCmd cmd)
        {
            foreach (Command command in m_aCmd)
            {
                if (cmd == command.m_cmd) return command; 
            }
            return null; 
        }

        void InitCmd()
        {
            foreach (eCmd cmd in Enum.GetValues(typeof(eCmd))) m_aCmd.Add(new Command(cmd, m_dmc));
        }

        void RunTreeCmd(Tree tree)
        {
            foreach (Command command in m_aCmd) command.RunTree(tree.GetTree(command.m_cmd.ToString(), false)); 
        }
        #endregion

        #region DMC
        string WriteCmd(eCmd cmd)
        {
            if (EQ.IsStop()) return "EQ Stop";
            switch (m_dmc.p_eState)
            {
                case DMCControl.eState.Error: return "DMC State is Error, Cancel : " + cmd.ToString();
                case DMCControl.eState.Running: return "DMC Task is Running : " + m_dmc.p_sTask + ", Cancel : " + cmd.ToString();
            }
            Thread.Sleep(10);
            if (EQ.IsStop()) return "EQ Stop";
            Command command = GetCommand(cmd);
            if (command == null) return "Command not Found : " + cmd.ToString();
            return command.Run(); 
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
                p_sInfo = WriteCmd(eCmd.Init);
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
            if (m_infoPod.m_bReqDocking)
            {
                m_infoPod.m_bReqDocking = false;
                StartRun(m_runDocking);
            }
            if (m_infoPod.m_bReqUnDocking)
            {
                m_infoPod.m_bReqUnDocking = false;
                StartRun(m_runUnDocking);
            }
            return "OK";
        }
        #endregion

        #region GAF
        SVID m_svidPlaced;
        CEID m_ceidDocking;
        CEID m_ceidUnDocking;
        CEID m_ceidOpen;
        CEID m_ceidClose; 
        ALID m_alidPlaced;
        void InitGAF()
        {
            m_svidPlaced = m_gaf.GetSVID(this, "Placed");
            m_ceidDocking = m_gaf.GetCEID(this, "Docking");
            m_ceidUnDocking = m_gaf.GetCEID(this, "UnDocking");
            m_ceidOpen = m_gaf.GetCEID(this, "Door Open");
            m_ceidClose = m_gaf.GetCEID(this, "Door Close");
            m_alidPlaced = m_gaf.GetALID(this, "Placed Sensor Error", "Placed & Plesent Sensor Should be Checked"); 
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeCmd(tree.GetTree("DMC Command", false)); 
        }
        #endregion

        public InfoPod m_infoPod; 
        public Loadport(string id, string sLocID, IEngineer engineer, string sLogGroup = "")
        {
            p_id = id; 
            m_infoPod = new InfoPod(this, sLocID, engineer, sLogGroup);
            m_aTool.Add(m_infoPod);
            InitCmd();
            base.InitBase(id, engineer, sLogGroup);
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
        ModuleRunBase m_runDocking;
        ModuleRunBase m_runUnDocking;
        ModuleRunBase m_runReadPodID; 
        protected override void InitModuleRuns()
        {
            m_runDocking = AddModuleRunList(new Run_Docking(this), false, "Docking Pod to Work Position");
            m_runUnDocking = AddModuleRunList(new Run_Undocking(this), false, "Undocking Pod from Work Position");
            m_runReadPodID = AddModuleRunList(new Run_ReadPodID(this), false, "Read Pod ID");
            AddModuleRunList(new Run_PodOpen(this), false, "Pod Open");
            AddModuleRunList(new Run_PodClose(this), false, "Pod Close");
        }

        public class Run_Docking : ModuleRunBase
        {
            Loadport m_module;
            InfoPod m_infoPod;
            public Run_Docking(Loadport module)
            {
                m_module = module;
                m_infoPod = module.m_infoPod;
                InitModuleRun(module);
            }

            bool m_bMapping = true;
            public override ModuleRunBase Clone()
            {
                Run_Docking run = new Run_Docking(m_module);
                run.m_bMapping = m_bMapping;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bMapping = tree.Set(m_bMapping, m_bMapping, "Mapping", "Wafer Mapping When Docking", bVisible);
            }

            public override string Run()
            {
                if (m_module.Run(m_module.WriteCmd(eCmd.Load))) return p_sInfo;
                m_infoPod.p_eState = InfoPod.eState.Dock; 
                m_module.m_ceidDocking.Send();
                m_module.m_ceidOpen.Send();
                m_module.m_infoPod.p_infoReticle.p_eState = GemSlotBase.eState.Exist; 
                m_module.m_infoPod.SendSlotMap(); 
                return "OK";
            }
        }

        public class Run_Undocking : ModuleRunBase
        {
            Loadport m_module;
            InfoPod m_infoPod;
            public Run_Undocking(Loadport module)
            {
                m_module = module;
                m_infoPod = module.m_infoPod;
                InitModuleRun(module);
            }

            string m_sUndocking = "UnDocking";
            public override ModuleRunBase Clone()
            {
                Run_Undocking run = new Run_Undocking(m_module);
                run.m_sUndocking = m_sUndocking;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sUndocking = tree.Set(m_sUndocking, m_sUndocking, "UnDocking", "Pod UnDocking", bVisible, true);
            }

            public override string Run()
            {
                if (m_module.Run(m_module.WriteCmd(eCmd.Unload))) return p_sInfo;
                m_infoPod.p_eState = InfoPod.eState.Placed;
                m_module.m_ceidClose.Send();
                m_module.m_ceidUnDocking.Send(); 
                return "OK";
            }
        }

        public class Run_ReadPodID : ModuleRunBase
        {
            Loadport m_module;
            public Run_ReadPodID(Loadport module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            string m_sSimulCarrierID = "CarrierID";
            public override ModuleRunBase Clone()
            {
                Run_ReadPodID run = new Run_ReadPodID(m_module);
                run.m_sSimulCarrierID = m_sSimulCarrierID;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sSimulCarrierID = tree.Set(m_sSimulCarrierID, m_sSimulCarrierID, "Simulation CarrierID", "CarrierID When p_bSimulation", bVisible && EQ.p_bSimulate);
            }

            public override string Run()
            {
                string sCarrierID = "";
                if (EQ.p_bSimulate) sCarrierID = m_sSimulCarrierID; 
                else
                {
                    //forget Read CarrierID
                }
                m_module.m_infoPod.SendCarrierID(sCarrierID); 
                return "OK"; 
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

            string m_sPodOpen = "Pod Open";
            public override ModuleRunBase Clone()
            {
                Run_PodOpen run = new Run_PodOpen(m_module);
                run.m_sPodOpen = m_sPodOpen;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sPodOpen = tree.Set(m_sPodOpen, m_sPodOpen, "Pod Open", "Pod Open", bVisible, true);
            }

            public override string Run()
            {
                if (m_module.Run(m_module.WriteCmd(eCmd.PodOpen))) return p_sInfo;
                //m_infoPod.p_eState = InfoPod.eState.Placed;
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

            string m_sPodClose = "Pod Close";
            public override ModuleRunBase Clone()
            {
                Run_PodClose run = new Run_PodClose(m_module);
                run.m_sPodClose = m_sPodClose;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sPodClose = tree.Set(m_sPodClose, m_sPodClose, "Pod Close", "Pod Close", bVisible, true);
            }

            public override string Run()
            {
                if (m_module.Run(m_module.WriteCmd(eCmd.PodClose))) return p_sInfo;
                //m_infoPod.p_eState = InfoPod.eState.Placed;
                return "OK";
            }
        }
        #endregion
    }
}
