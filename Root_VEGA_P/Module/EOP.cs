using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_VEGA_P.Module
{
    public class EOP : ModuleBase
    {
        #region ToolBox
        Axis m_axis;
        DIO_Os m_doCoverDown;
        DIO_Os m_doCoverDownX;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetAxis(ref m_axis, this, "Y");
            p_sInfo = m_toolBox.GetDIO(ref m_doCoverDown, this, "Cover", Enum.GetNames(typeof(eCoverDown)));
            p_sInfo = m_toolBox.GetDIO(ref m_doCoverDownX, this, "Cover X", Enum.GetNames(typeof(eCoverDown)));
            m_dome.GetTools(m_toolBox, bInit);
            m_door.GetTools(m_toolBox, bInit); 
            if (bInit) InitPos();
        }
        #endregion

        #region AxisY
        public enum ePos
        {
            Backward,
            Forward,
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
        #endregion

        #region CoverDown
        public enum eCoverDown
        {
            Up,
            Down
        }
        double m_secCoverDown = 3; 
        public string RunCoverDown(bool bDown)
        {
            m_doCoverDown.Write(bDown ? eCoverDown.Down : eCoverDown.Up);
            m_doCoverDownX.Write(bDown ? eCoverDown.Down : eCoverDown.Up);
            StopWatch sw = new StopWatch();
            int msDown = (int)(1000 * m_secCoverDown);
            while (sw.ElapsedMilliseconds < msDown)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
                if (m_dome.IsCoverDown(bDown) && m_door.IsCoverDown(bDown)) return "OK";
            }
            return "Run CoverDown Timeout";
        }

        void RunTreeCoverDown(Tree tree)
        {
            m_secCoverDown = tree.Set(m_secCoverDown, m_secCoverDown, "Timeout", "Run Cover UpDown Timeout (sec)"); 
        }
        #endregion 

        #region Dome
        public class Dome : NotifyProperty, IRTRChild
        {
            #region ToolBox
            public Axis m_axisRotate;
            DIO_Is m_diCheckRotate;
            DIO_Is m_diCheckDome;
            DIO_Os m_doClamp;
            DIO_Is[] m_diClamp = new DIO_Is[2] { null, null };
            DIO_Is[] m_diCoverDown = new DIO_Is[2] { null, null };
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                toolBox.GetAxis(ref m_axisRotate, m_EOP, p_id + ".Rotate");
                toolBox.GetDIO(ref m_diCheckRotate, m_EOP, p_id + ".Rotate", new string[] { "0", "1" });
                toolBox.GetDIO(ref m_diCheckDome, m_EOP, p_id + ".Check", new string[] { "0", "1" });
                toolBox.GetDIO(ref m_doClamp, m_EOP, p_id + ".Clamp", Enum.GetNames(typeof(eClamp)));
                toolBox.GetDIO(ref m_diClamp[0], m_EOP, p_id + ".Unclamp", new string[] { "0", "1", "2", "3" });
                toolBox.GetDIO(ref m_diClamp[1], m_EOP, p_id + ".Clamp", new string[] { "0", "1", "2", "3" });
                toolBox.GetDIO(ref m_diCoverDown[0], m_EOP, p_id + ".CoverUp", new string[] { "0", "1" });
                toolBox.GetDIO(ref m_diCoverDown[1], m_EOP, p_id + ".CoverDown", new string[] { "0", "1" });
                if (bInit) InitPos();
            }
            #endregion

            #region Axis Rotate
            public enum ePos
            {
                Ready,
                Rotate,
            }
            void InitPos()
            {
                m_axisRotate.AddPos(Enum.GetNames(typeof(ePos)));
            }

            public string RunRotate(ePos ePos)
            {
                m_axisRotate.StartMove(ePos);
                return m_axisRotate.WaitReady();
            }
            #endregion

            #region Check Input
            public bool IsCheckRotate()
            {
                return m_diCheckRotate.ReadDI(0) && m_diCheckRotate.ReadDI(1); 
            }

            public bool IsCheckDome()
            {
                return m_diCheckDome.ReadDI(0) && m_diCheckDome.ReadDI(1); 
            }

            public bool IsCoverDown(bool bDown)
            {
                for (int n = 0; n < 2; n++)
                {
                    if (m_diCoverDown[0].ReadDI(n) == bDown) return false;
                    if (m_diCoverDown[1].ReadDI(n) == !bDown) return false;
                }
                return true;
            }
            #endregion

            #region Clamp
            public enum eClamp
            {
                Unclamp,
                Clamp
            }
            double m_secClamp = 3;
            public string RunClamp(bool bClamp)
            {
                m_doClamp.Write(bClamp ? eClamp.Clamp : eClamp.Unclamp); 
                StopWatch sw = new StopWatch();
                int msClamp = (int)(1000 * m_secClamp); 
                while (sw.ElapsedMilliseconds < msClamp)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                    if (IsClamp(bClamp)) return "OK"; 
                }
                return "Run Clamp Timeout"; 
            }

            bool IsClamp(bool bClamp)
            {
                for (int n = 0; n < 4; n++)
                {
                    if (m_diClamp[0].ReadDI(n) == bClamp) return false; 
                    if (m_diClamp[1].ReadDI(n) == !bClamp) return false;
                }
                return true; 
            }
            #endregion

            #region InfoPod
            InfoPod _infoPod = null;
            public InfoPod p_infoPod
            {
                get { return _infoPod; }
                set
                {
                    int nPod = (value != null) ? (int)value.p_ePod : -1;
                    _infoPod = value;
                    m_reg.Write("InfoPod", nPod);
                    value.WriteReg();
                    OnPropertyChanged();
                }
            }

            Registry m_reg = null;
            public void ReadPod_Registry()
            {
                m_reg = new Registry("InfoPod");
                int nPod = m_reg.Read(p_id, -1);
                p_infoPod = new InfoPod((InfoPod.ePod)nPod);
                p_infoPod.ReadReg();
            }

            public bool IsEnableRecovery()
            {
                return p_infoPod != null;
            }
            #endregion

            #region IRTRChild
            public bool p_bLock { get; set; }
            public eState p_eState { get { return m_EOP.p_eState; } }

            public string IsGetOK()
            {
                if (p_eState != eState.Ready) return p_id + " eState not Ready";
                return (p_infoPod != null) ? "OK" : p_id + " IsGetOK - Pod not Exist";
            }

            public string IsPutOK(InfoPod infoPod)
            {
                if (p_eState != eState.Ready) return p_id + " eState not Ready";
                switch (infoPod.p_ePod)
                {
                    case InfoPod.ePod.EOP_Door:
                    case InfoPod.ePod.EIP_Cover:
                    case InfoPod.ePod.EIP_Plate:
                        return p_id + " Invalid Pod Type";
                }
                return (p_infoPod == null) ? "OK" : p_id + " IsPutOK - Pod Exist";
            }

            public string BeforeGet() 
            { 
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
                return IsCheckDome();
            }
            #endregion

            #region Teach RTR
            int m_teach = -1;
            public int GetTeachRTR(InfoPod infoPod)
            {
                return m_teach; 
            }

            public void RunTreeTeach(Tree tree)
            {
                m_teach = tree.Set(m_teach, m_teach, m_EOP.p_id + "." + p_id, "RND RTR Teach");
            }
            #endregion

            #region Tree
            public void RunTree(Tree tree)
            {
                m_secClamp = tree.Set(m_secClamp, m_secClamp, "Clamp", "Run Clamp Timeout (sec)");
            }
            #endregion

            public string p_id { get; set; }
            EOP m_EOP; 
            public Dome(string id, EOP EOP)
            {
                p_id = id;
                m_EOP = EOP; 
            }
        }
        public Dome m_dome; 
        void InitDome()
        {
            m_dome = new Dome("Dome", this); 
        }
        #endregion

        #region Door
        public class Door : NotifyProperty, IRTRChild
        {
            #region ToolBox
            DIO_Is m_diCheckDoor;
            DIO_Os m_doCylinder;
            DIO_Is[] m_diCylinder = new DIO_Is[2] { null, null };
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                toolBox.GetDIO(ref m_diCheckDoor, m_EOP, p_id + ".Check", new string[] { "0", "1" });
                toolBox.GetDIO(ref m_doCylinder, m_EOP, p_id + ".Cylinder", Enum.GetNames(typeof(eCylinder)));
                toolBox.GetDIO(ref m_diCylinder[0], m_EOP, p_id + ".Cylinder Down", new string[] { "0", "1" });
                toolBox.GetDIO(ref m_diCylinder[1], m_EOP, p_id + ".Cylinder Up", new string[] { "0", "1" });
                if (bInit) { }
            }
            #endregion

            #region Check Input
            public bool IsCheckDoor()
            {
                return m_diCheckDoor.ReadDI(0) && m_diCheckDoor.ReadDI(1);
            }

            public bool IsCoverDown(bool bDown)
            {
                for (int n = 0; n < 2; n++)
                {
                    if (m_diCylinder[0].ReadDI(n) == bDown) return false;
                    if (m_diCylinder[1].ReadDI(n) == !bDown) return false;
                }
                return true;
            }
            #endregion

            #region Cylinder Up
            public enum eCylinder
            {
                Down,
                Up
            }
            double m_secUp = 3;
            public string RunCylinderUp(bool bUp)
            {
                m_doCylinder.Write(bUp ? eCylinder.Up : eCylinder.Down); 
                StopWatch sw = new StopWatch();
                int msUp = (int)(1000 * m_secUp);
                while (sw.ElapsedMilliseconds < msUp)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                    if (IsCylinder(bUp)) return "OK";
                }
                return "Run Cylinder Up Timeout";
            }

            bool IsCylinder(bool bUp)
            {
                for (int n = 0; n < 2; n++)
                {
                    if (m_diCylinder[0].ReadDI(n) == bUp) return false;
                    if (m_diCylinder[1].ReadDI(n) == !bUp) return false;
                }
                return true;
            }
            #endregion

            #region InfoPod
            InfoPod _infoPod = null;
            public InfoPod p_infoPod
            {
                get { return _infoPod; }
                set
                {
                    int nPod = (value != null) ? (int)value.p_ePod : -1;
                    _infoPod = value;
                    m_reg.Write("InfoPod", nPod);
                    value.WriteReg();
                    OnPropertyChanged();
                }
            }

            Registry m_reg = null;
            public void ReadPod_Registry()
            {
                m_reg = new Registry("InfoPod");
                int nPod = m_reg.Read(p_id, -1);
                p_infoPod = new InfoPod((InfoPod.ePod)nPod);
                p_infoPod.ReadReg();
            }

            public bool IsEnableRecovery()
            {
                return p_infoPod != null;
            }
            #endregion

            #region IRTRChild
            public bool p_bLock { get; set; }
            public eState p_eState { get { return m_EOP.p_eState; } }

            public string IsGetOK()
            {
                if (p_eState != eState.Ready) return p_id + " eState not Ready";
                return (p_infoPod != null) ? "OK" : p_id + " IsGetOK - Pod not Exist";
            }

            public string IsPutOK(InfoPod infoPod)
            {
                if (p_eState != eState.Ready) return p_id + " eState not Ready";
                switch (infoPod.p_ePod)
                {
                    case InfoPod.ePod.EOP_Dome:
                    case InfoPod.ePod.EIP_Cover:
                    case InfoPod.ePod.EIP_Plate:
                        return p_id + " Invalid Pod Type";
                }
                return (p_infoPod == null) ? "OK" : p_id + " IsPutOK - Pod Exist";
            }

            public string BeforeGet()
            {
                return RunCylinderUp(true); 
            }

            public string BeforePut(InfoPod infoPod)
            {
                return RunCylinderUp(true);
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
                return IsCheckDoor();
            }
            #endregion

            #region Teach RTR
            int m_teach = -1;
            public int GetTeachRTR(InfoPod infoPod)
            {
                return m_teach;
            }

            public void RunTreeTeach(Tree tree)
            {
                m_teach = tree.Set(m_teach, m_teach, m_EOP.p_id + "." + p_id, "RND RTR Teach");
            }
            #endregion

            #region Tree
            public void RunTree(Tree tree)
            {
                m_secUp = tree.Set(m_secUp, m_secUp, "Cylinder Up", "Run Cylinder UpDown Timeout (sec)");
            }
            #endregion

            public string p_id { get; set; }
            EOP m_EOP;
            public Door(string id, EOP EOP)
            {
                p_id = id;
                m_EOP = EOP;
            }
        }
        public Door m_door;
        void InitDoor()
        {
            m_door = new Door("Door", this);
        }
        #endregion

        #region Particle Counter
        public string RunParticleCounter(bool bCheckPod)
        {
            if (bCheckPod)
            {
                if (m_dome.IsCheckDome() == false) return "Dome Check Error";
                if (m_door.IsCheckDoor() == false) return "Door Check Error";
            }
            if (Run(RunMove(ePos.Backward))) return p_sInfo; 
            if (Run(m_dome.RunClamp(true))) return p_sInfo;
            try
            {
                if (Run(m_dome.RunRotate(Dome.ePos.Rotate))) return p_sInfo;
                if (Run(m_door.RunCylinderUp(false))) return p_sInfo;
                if (Run(RunMove(ePos.Forward))) return p_sInfo;
                if (Run(RunCoverDown(true))) return p_sInfo;
                // Particle Count
                if (Run(RunCoverDown(false))) return p_sInfo;
                if (Run(RunMove(ePos.Backward))) return p_sInfo;
                if (Run(m_door.RunCylinderUp(true))) return p_sInfo;
                if (Run(m_dome.RunRotate(Dome.ePos.Ready))) return p_sInfo;
                if (Run(m_dome.RunClamp(false))) return p_sInfo;
            }
            finally
            {
                string sMove = RunMove(ePos.Backward);
                m_door.RunCylinderUp(true);
                if (sMove == "OK")
                {
                    m_dome.RunRotate(Dome.ePos.Ready);
                    m_dome.RunClamp(false);
                }
            }
            return "OK";
        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
            RunMove(ePos.Backward); 
            m_dome.RunRotate(Dome.ePos.Ready);
            m_door.RunCylinderUp(true); 
        }

        public override void InitMemorys()
        {
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            string sHome = base.StateHome(m_axis);
            p_eState = (sHome == "OK") ? eState.Ready : eState.Error;
            Reset(); 
            return "OK";
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeCoverDown(tree.GetTree("Cover Down"));
            m_dome.RunTree(tree.GetTree("Dome"));
            m_door.RunTree(tree.GetTree("Door"));
        }
        #endregion

        public EOP(string id, IEngineer engineer)
        {
            InitDome();
            InitDoor(); 
            InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            AddModuleRunList(new Run_Run(this), true, "Run Particle Counter");
            AddModuleRunList(new Run_RunSol(this), false, "Run Sol Test");
        }

        public class Run_Delay : ModuleRunBase
        {
            EOP m_module;
            public Run_Delay(EOP module)
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
                Thread.Sleep((int)(1000 * m_secDelay / 2));
                return "OK";
            }
        }

        public class Run_Run : ModuleRunBase
        {
            EOP m_module;
            public Run_Run(EOP module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bCheckPod = true; 
            public override ModuleRunBase Clone()
            {
                Run_Run run = new Run_Run(m_module);
                run.m_bCheckPod = m_bCheckPod; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bCheckPod = tree.Set(m_bCheckPod, m_bCheckPod, "Check Pod", "Check Pod State", bVisible);
            }

            public override string Run()
            {
                return m_module.RunParticleCounter(m_bCheckPod);
            }
        }

        public class Run_RunSol : ModuleRunBase
        {
            EOP m_module;
            public Run_RunSol(EOP module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            enum eSol
            {
                EOP_Cover,
                Dome_Clamp,
                Door_Cylinder
            }
            eSol m_eSol = eSol.Dome_Clamp; 
            bool m_bOn = false;
            public override ModuleRunBase Clone()
            {
                Run_RunSol run = new Run_RunSol(m_module);
                run.m_eSol = m_eSol;
                run.m_bOn = m_bOn; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eSol = (eSol)tree.Set(m_eSol, m_eSol, "SolValve", "Select SolValve", bVisible);
                m_bOn = tree.Set(m_bOn, m_bOn, "On", "SolValve On", bVisible);
            }

            public override string Run()
            {
                switch (m_eSol)
                {
                    case eSol.Dome_Clamp: return m_module.m_dome.RunClamp(m_bOn);
                    case eSol.Door_Cylinder: return m_module.m_door.RunCylinderUp(m_bOn);
                    case eSol.EOP_Cover: return m_module.RunCoverDown(m_bOn); 
                }
                return "OK";
            }
        }
        #endregion
    }
}
