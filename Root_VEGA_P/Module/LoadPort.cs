using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Camera.CognexDM150;
using RootTools.Camera.CognexOCR;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Gem;
using RootTools.Module;
using RootTools.OHTNew;
using RootTools.RFIDs;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_VEGA_P.Module
{
    public class Loadport : ModuleBase, IRTRChild
    {
        #region ToolBox
        Camera_CognexDM150 m_camBCD; 
        OHT m_OHT;
        RFID m_RFID; 
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetCamera(ref m_camBCD, this, "Barcode Camera");
            p_sInfo = m_toolBox.GetOHT(ref m_OHT, this, m_infoPods, "OHT");
            p_sInfo = m_toolBox.Get(ref m_RFID, this, "RFID");
            m_stage.GetTools(m_toolBox, this, bInit);
            m_door.GetTools(m_toolBox, this);
            m_interlock.GetTools(m_toolBox, this);
            m_led.GetTools(m_toolBox, this); 
        }
        #endregion

        #region Stage
        public class Stage : NotifyProperty
        {
            enum eCheck
            {
                Placed,
                Present,
                BadA,
                BadB,
            }
            LoadCell m_loadCell;
            Axis m_axis;
            DIO_I2O2 m_dioPodOpen;
            DIO_Is m_diCheck;
            DIO_O m_doVacuum;
            DIO_O m_doBlow; 
            public void GetTools(ToolBox toolBox, Loadport module, bool bInit)
            {
                m_loadCell.GetTools(toolBox, bInit);
                module.p_sInfo = toolBox.GetAxis(ref m_axis, module, p_id);
                module.p_sInfo = toolBox.GetDIO(ref m_dioPodOpen, module, p_id + ".PodOpen", "Close", "Open");
                module.p_sInfo = toolBox.GetDIO(ref m_diCheck, module, p_id + ".Check", Enum.GetNames(typeof(eCheck)));
                module.p_sInfo = toolBox.GetDIO(ref m_doVacuum, module, p_id + ".Vacuum");
                module.p_sInfo = toolBox.GetDIO(ref m_doBlow, module, p_id + ".Blow");
                if (bInit) InitPos();
            }

            public enum ePos
            {
                Outside,
                Barcode,
                Inside
            }
            void InitPos()
            {
                m_axis.AddPos(Enum.GetNames(typeof(ePos)));
            }

            public string RunMove(ePos ePos)
            {
                m_axis.StartMove(ePos);
                return m_axis.WaitReady(); 
            }

            public string RunPodOpen(bool bOpen)
            {
                m_dioPodOpen.Write(bOpen);
                return m_dioPodOpen.WaitDone(); 
            }

            public string RunWeigh()
            {
                return m_loadCell.Run_GetWeight();
            }
            bool _bPlaced = false; 
            public bool p_bPlaced
            {
                get { return _bPlaced; }
                set
                {
                    if (_bPlaced == value) return;
                    _bPlaced = value;
                    OnPropertyChanged(); 
                }
            }

            bool _bPresent = false;
            public bool p_bPresent
            {
                get { return _bPresent; }
                set
                {
                    if (_bPresent == value) return;
                    _bPresent = value;
                    OnPropertyChanged();
                }
            }

            public void ThreadCheck()
            {
                bool bBad = m_diCheck.ReadDI(eCheck.BadA) || m_diCheck.ReadDI(eCheck.BadB); 
                p_bPlaced = (bBad == false) && m_diCheck.ReadDI(eCheck.Placed);
                p_bPresent = (bBad == false) && m_diCheck.ReadDI(eCheck.Present);
            }

            bool _bVacuumCheck = false;
            public bool p_bVacuumCheck
            {
                get { return _bVacuumCheck; }
                set
                {
                    if (_bVacuumCheck == value) return;
                    _bVacuumCheck = value;
                    OnPropertyChanged();
                }
            }

            #region Vacuum
            double m_secBlow = 0.5;
            double m_secVac = 1;
            public string RunVacuum(bool bOn)
            {
                m_doVacuum.Write(bOn);
                if (bOn) 
                    Thread.Sleep((int)(1000 * m_secVac)); 
                else
                {
                    m_doBlow.Write(true);
                    //Thread.Sleep((int)(500 * m_secBlow));
                    //m_doBlow.Write(false);
                }
                return "OK";
            }
            #endregion

            public void RunTree(Tree tree)
            {
                m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum on Delay (sec)");
                m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Blow Delay (sec)");
            }

            public string p_id { get; set; }
            public Stage(string id,Loadport loadport)
            {
                p_id = id;
                InitloadCell(loadport);
            }
            void InitloadCell(Loadport loadport)
            {
                m_loadCell = new LoadCell(loadport);
            }
        }
        Stage m_stage;
        public bool p_bPlaced
        {
            get { return m_stage.p_bPlaced; }
        }

        public bool p_bPresent
        {
            get { return m_stage.p_bPresent; }
        }

        #endregion

        #region Door
        public class Door
        {
            enum eDoorSeal
            {
                Seal,
                Unseal
            }
            DIO_Is[] m_diDoorSeal = new DIO_Is[2] { null, null };
            DIO_Os m_doDoorSeal;
            DIO_I2O2 m_dioDoor; 
            public void GetTools(ToolBox toolBox, Loadport module)
            {
                module.p_sInfo = toolBox.GetDIO(ref m_dioDoor, module, p_id + ".Door", "Close", "Open");
                module.p_sInfo = toolBox.GetDIO(ref m_diDoorSeal[0], module, p_id + "SealA", Enum.GetNames(typeof(eDoorSeal)));
                module.p_sInfo = toolBox.GetDIO(ref m_diDoorSeal[1], module, p_id + "SealB", Enum.GetNames(typeof(eDoorSeal)));
                module.p_sInfo = toolBox.GetDIO(ref m_doDoorSeal, module, p_id + ".Seal", Enum.GetNames(typeof(eDoorSeal)));
            }

            public string RunDoor(bool bOpen)
            {
                if (bOpen)
                {
                    string sSeal = RunSeal(eDoorSeal.Unseal);
                    if (sSeal != "OK") return sSeal;
                    Thread.Sleep(500);
                    return RunOpen(bOpen); 
                }
                else
                {
                    string sOpen = RunOpen(bOpen);
                    if (sOpen != "OK") return sOpen;
                    Thread.Sleep(500);
                    return RunSeal(eDoorSeal.Seal); 
                }
            }

            double m_secSeal = 2;
            string RunSeal(eDoorSeal eDoorSeal)
            {
                StopWatch sw = new StopWatch(); 
                m_doDoorSeal.Write(eDoorSeal);
                int msSeal = (int)(1000 * m_secSeal); 
                while (sw.ElapsedMilliseconds < msSeal)
                {
                    if (EQ.IsStop()) return "EQ Stop";
                    if (IsSeal(eDoorSeal)) return "OK"; 
                }
                return "Door Seal Timeover";
                //return "OK";
            }

            bool IsSeal(eDoorSeal eDoorSeal)
            {
                if (m_diDoorSeal[0].ReadDI(eDoorSeal) == false) return false;
                if (m_diDoorSeal[1].ReadDI(eDoorSeal) == false) return false;
                if (m_diDoorSeal[0].ReadDI(1 - eDoorSeal)) return false;
                if (m_diDoorSeal[1].ReadDI(1 - eDoorSeal)) return false;
                return true; 
            }

            string RunOpen(bool bOpen)
            {
                m_dioDoor.Write(bOpen);
                return m_dioDoor.WaitDone(); 
            }

            public void RunTree(Tree tree)
            {
                m_secSeal = tree.Set(m_secSeal, m_secSeal, "Seal Timeout", "Door Seal Sol Timeout (sec)");
            }

            public string p_id { get; set; }
            public Door(string id)
            {
                p_id = id;
            }
        }
        Door m_door = new Door("Door");
        #endregion

        #region Interlock
        public class Interlock : NotifyProperty
        {
            public DIO_I m_diCDA;
            public DIO_I m_diLightCurtain;
            public DIO_I m_diProtectionBar;
            public DIO_I m_diOHTGuide; 
            public void GetTools(ToolBox toolBox, Loadport module)
            {
                module.p_sInfo = toolBox.GetDIO(ref m_diCDA, module, "CDA");
                module.p_sInfo = toolBox.GetDIO(ref m_diLightCurtain, module, "LightCurtain");
                module.p_sInfo = toolBox.GetDIO(ref m_diProtectionBar, module, "ProtectionBar");
                module.p_sInfo = toolBox.GetDIO(ref m_diOHTGuide, module, "OHT Guide");
            }

            void SetError()
            {
                EQ.p_bStop = true;
                m_loadport.p_eState = eState.Error; 
            }

            bool _bCDA = false;
            public bool p_bCDA
            {
                get { return _bCDA; }
                set
                {
                    if (_bCDA == value) return;
                    _bCDA = value;
                    OnPropertyChanged();
                    if (value) SetError(); 
                }
            }

            bool _bLightCurtain = false;
            public bool p_bLightCurtain
            {
                get { return _bLightCurtain; }
                set
                {
                    if (_bLightCurtain == value) return;
                    _bLightCurtain = value;
                    OnPropertyChanged();
                }
            }

            bool _bProtectionBar = false;
            public bool p_bProtectionBar
            {
                get { return _bProtectionBar; }
                set
                {
                    if (_bProtectionBar == value) return;
                    _bProtectionBar = value;
                    OnPropertyChanged();
                }
            }

            bool _bOHTGuide = false;
            public bool p_bOHTGuide
            {
                get { return _bOHTGuide; }
                set
                {
                    if (_bOHTGuide == value) return;
                    _bOHTGuide = value;
                    OnPropertyChanged();
                }
            }

            public void ThreadCheck()
            {
                p_bCDA = m_diCDA.p_bIn;
                p_bLightCurtain = m_diLightCurtain.p_bIn;
                p_bProtectionBar = m_diProtectionBar.p_bIn;
                p_bOHTGuide = m_diOHTGuide.p_bIn;
            }

            public string p_id { get; set; }
            Loadport m_loadport; 
            public Interlock(string id, Loadport loadport)
            {
                p_id = id;
                m_loadport = loadport; 
            }
        }
        Interlock m_interlock; 
        #endregion

        #region LED
        public class LED
        { 
            public enum eLED
            {
                Manual,
                Auto,
                Present,
                Placed,
                Load,
                Unload,
                Alarm
            }
            DIO_Os m_doLED;
            public void GetTools(ToolBox toolBox, Loadport module)
            {
                module.p_sInfo = toolBox.GetDIO(ref m_doLED, module, p_id, Enum.GetNames(typeof(eLED)), false);
            }

            public void RunLED(eLED eLED, bool bOn)
            {
                m_doLED.Write(eLED, bOn && m_bBlink); 
            }

            public bool m_bBlink = false;
            public string p_id { get; set; }
            public LED(string id)
            {
                p_id = id;
            }
        }
        LED m_led = new LED("LED");
        #endregion

        #region ThreadCheck
        bool m_bThreadCheck = false;
        Thread m_threadCheck;
        void InitThreadCheck()
        {
            m_threadCheck = new Thread(new ThreadStart(RunThreadCheck));
            m_threadCheck.Start(); 
        }

        void RunThreadCheck()
        {
            StopWatch swBlink = new StopWatch(); 
            m_bThreadCheck = true;
            Thread.Sleep(2000); 
            while (m_bThreadCheck)
            {
                Thread.Sleep(10);
                m_stage.ThreadCheck();
                m_interlock.ThreadCheck(); 
                if (swBlink.ElapsedMilliseconds > 400)
                {
                    swBlink.Start();
                    m_led.m_bBlink = !m_led.m_bBlink; 
                }
                m_infoPods.p_ePresentSensor = (m_stage.p_bPlaced && m_stage.p_bPresent) ? GemCarrierBase.ePresent.Exist : GemCarrierBase.ePresent.Empty;
                m_led.RunLED(LED.eLED.Manual, (EQ.p_eState == EQ.eState.Ready) && (EQ.IsStop() == false));
                m_led.RunLED(LED.eLED.Auto, EQ.p_eState == EQ.eState.Run);
                m_led.RunLED(LED.eLED.Placed, m_stage.p_bPlaced);
                m_led.RunLED(LED.eLED.Present, m_stage.p_bPresent);
                m_led.RunLED(LED.eLED.Load, m_bDocking);
                m_led.RunLED(LED.eLED.Unload, m_bUndockng);
                m_led.RunLED(LED.eLED.Alarm, EQ.p_eState == EQ.eState.Error);
            }
        }
        #endregion

        #region LoadCell
        public class LoadCell : NotifyProperty
        {
            Loadport m_loadPort;
            RS232 m_rs232;

            public void GetTools(ToolBox toolBox, bool bInit)
            {
                toolBox.GetComm(ref m_rs232, m_loadPort, "LoadCell");

                ConnectRS232();

                if (bInit)
                    m_rs232.p_bConnect = true;
                
            }
            public string Run_GetWeight()
            {
                string sInfo = ConnectRS232();
                sInfo = m_rs232.Send("00RW");
                return sInfo;

                //if (sInfo != "OK") return sInfo;
                //return "OK";
            }

            private void M_rs232_OnReceive(string sRead)
            {
                string str = sRead.Trim();

                m_rs232.m_commLog.Add(CommLog.eType.Receive, "CAS Receive = " + str);
                //m_loadPort.m_infoPods. = int.Parse(str);
            }

            string ConnectRS232()
            {
                if (m_rs232.p_bConnect) return "OK";
                m_rs232.p_bConnect = true;
                m_rs232.OnReceive += M_rs232_OnReceive;
                Thread.Sleep(100);
                return m_rs232.p_bConnect ? "OK" : "RS232 Connect Error";
            }
            public LoadCell(Loadport loadport)
            {
                m_loadPort = loadport;
            }
        }
        #endregion

        #region InfoPods
        InfoPods m_infoPods; 
        void InitInfoPods(string id, IEngineer engineer)
        {
            m_infoPods = new InfoPods(this, id, engineer); 
        }

        public InfoPod p_infoPod
        {
            get { return m_infoPods.p_infoPod; }
            set { m_infoPods.p_infoPod = value; }
        }

        public void ReadPod_Registry()
        {
            m_infoPods.ReadPod_Registry(); 
        }
        #endregion

        #region Docking
        bool m_bDocking = false;
        public string RunDocking()
        {
            try
            {
                if (EQ.p_bSimulate)
                {
                    m_infoPods.p_ePresentSensor = GemCarrierBase.ePresent.Exist;
                    m_infoPods.p_sCarrierID = "Simulation";
                    m_infoPods.NewInfoPod(4);
                    return "OK";
                }

                m_infoPods.CheckPlaced(m_infoPods.p_ePresentSensor);

                switch (m_infoPods.p_eState)
                {
                    case InfoPods.eState.Dock: 
                        return "OK";
                    case InfoPods.eState.Empty:
                        return "Pod not Exist";
                }

                m_stage.RunWeigh();
                string sRFID = "";
                m_RFID.Read(out sRFID);

                //m_infoPods.p_sCarrierID = sRFID;
                //m_infoPods.SendCarrierID(m_infoPods.p_sCarrierID);

                m_bDocking = true;
                if (m_stage.p_bPlaced == false) return "Not Placed";
                if (m_stage.p_bPresent == false) return "Not Present";
                if (Run(m_stage.RunVacuum(true))) return p_sInfo;
                if (Run(m_door.RunDoor(true))) return p_sInfo;
                if (Run(m_stage.RunMove(Stage.ePos.Barcode))) return p_sInfo;
                if (Run(m_camBCD.ReadBCD())) return p_sInfo;
                if (Run(m_stage.RunMove(Stage.ePos.Inside))) return p_sInfo;
                if (Run(m_door.RunDoor(false))) return p_sInfo;
                if (Run(m_stage.RunPodOpen(true))) return p_sInfo;
                if (Run(m_stage.RunVacuum(false))) return p_sInfo;
                m_infoPods.NewInfoPod(4);
                
                return "OK";
            }
            finally { m_bDocking = false; }
        }
        #endregion

        #region Undocking
        bool m_bUndockng = false;
        public string RunUndocking()
        {
            try
            {
                if (Run(m_stage.RunVacuum(true))) return p_sInfo;
                if (Run(m_stage.RunPodOpen(false))) return p_sInfo;
                if (Run(m_door.RunDoor(true))) return p_sInfo;
                if (Run(m_stage.RunMove(Stage.ePos.Outside))) return p_sInfo;
                if (Run(m_door.RunDoor(false))) return p_sInfo;
                if (Run(m_stage.RunVacuum(false))) return p_sInfo;

                m_infoPods.ClearInfoPod();
                return "OK";
            }
            finally { m_bUndockng = false; }
        }
        #endregion

        #region IRTRChild
        public bool p_bLock { get; set; }

        public string IsGetOK()
        {
            if (p_eState != eState.Ready) 
                return p_id + " eState not Ready";
            return (p_infoPod != null) ? "OK" : p_id + " IsGetOK - Pod not Exist";
        }

        public string IsPutOK(InfoPod infoPod)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            int nPod = (int)infoPod.p_ePod;
            if (nPod == m_infoPods.GetPodCount()) return "OK";
            return p_id + " Invalid Pod Type : " + infoPod.p_ePod.ToString();
        }

        public string BeforeGet()
        {
            //LoadPort Blow 상태 확인
            //Door Lock 상태 확인
            //if()
            return "OK"; 
        }

        public string BeforePut(InfoPod infoPod)
        {
            return "OK";
        }

        public string AfterGet()
        {
            return "OK";
        }

        public string AfterPut()
        {
            return "OK";
        }

        public bool IsPodExist(InfoPod.ePod ePod)
        {
            return (m_infoPods.GetPodCount() > 0);
        }

        public bool IsEnableRecovery()
        {
            return false;
        }
        #endregion

        #region Teach RTR
        int[] m_teachRTR = new int[4] { 0, 0, 0, 0 }; 
        public int GetTeachRTR(InfoPod infoPod)
        {
            if (infoPod != null)
                return m_teachRTR[(int)infoPod.p_ePod];
            else 
                return -1;
        }

        public void RunTreeTeach(Tree tree)
        {
            Tree treeTeach = tree.GetTree(p_id); 
            for (int n = 0; n < 4; n++)
            {
                m_teachRTR[n] = treeTeach.Set(m_teachRTR[n], m_teachRTR[n], ((InfoPod.ePod)n).ToString(), "RND RTR Teach"); 
            }
        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
        }

        public override void InitMemorys()
        {
        }
        #endregion

        #region State Home
        public override string StateHome() 
        {
            if (EQ.p_bSimulate) return "OK";
            if (Run(m_door.RunDoor(true))) return p_sInfo;
            if (Run(base.StateHome())) return p_sInfo;
            if (Run(m_stage.RunMove(Stage.ePos.Outside))) return p_sInfo;

            return m_door.RunDoor(false);
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_stage.RunTree(tree.GetTree("Stage"));
            m_door.RunTree(tree.GetTree("Door"));
        }
        #endregion

        public Loadport(string id, IEngineer engineer)
        {
            InitInfoPods(id, engineer); 
            m_interlock = new Interlock("Interlock", this);
            m_stage = new Stage("Stage", this);

            InitBase(id, engineer);
            InitThreadCheck(); 
        }

        public override void ThreadStop()
        {
            m_stage.RunMove(Stage.ePos.Outside); 
            if (m_bThreadCheck)
            {
                m_bThreadCheck = false;
                m_threadCheck.Join(); 
            }
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Docking(this), true, "Docking Pod to Work Position");
            AddModuleRunList(new Run_Undocking(this), true, "Undocking Pod from Work Position");
        }

        public class Run_Docking : ModuleRunBase
        {
            Loadport m_module;
            public Run_Docking(Loadport module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Docking run = new Run_Docking(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunDocking();
            }
        }

        public class Run_Undocking : ModuleRunBase
        {
            Loadport m_module;
            public Run_Undocking(Loadport module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Undocking run = new Run_Undocking(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunUndocking();
            }
        }
        #endregion
    }
}
