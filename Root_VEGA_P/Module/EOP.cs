using Root_VEGA_P.Engineer;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.CognexOCR;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Root_VEGA_P.Module
{
    public class EOP : ModuleBase
    {
        #region ToolBox
        MemoryPool memoryPool;

        Axis m_axis;
        DIO_Os m_doDomeCoverDown;
        DIO_Os m_doDoorCoverDown;

        LightSet lightSet;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref memoryPool, this, "Memory", 1);
            p_sInfo = m_toolBox.GetAxis(ref m_axis, this, "Y");
            //toolBox.Get(ref lightSet, m_EOP);
            p_sInfo = m_toolBox.Get(ref lightSet, this);
            p_sInfo = m_toolBox.GetDIO(ref m_doDomeCoverDown, this, "DomeCover", Enum.GetNames(typeof(eCover)));
            p_sInfo = m_toolBox.GetDIO(ref m_doDoorCoverDown, this, "DoorCover", Enum.GetNames(typeof(eCover)));
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
            if(ePos.Equals(ePos.Forward)) //앞으로 갈때
            {
                if (m_door.IsCylinder(true)) //실린더 올라가있으면 움직이지마 
                    return "Check Cylinder";

                if (!m_dome.IsCheckRotate()) //Dome 안돌아가져있으면 움직이지마
                    return "Check Dome Rotate";
            }

            m_axis.StartMove(ePos);
            return m_axis.WaitReady();
        }
        #endregion

        #region CoverDown
        public enum eCover
        {
            Up,
            Down
        }
        double m_secCoverDown = 3; 
        public string RunDomeCoverDown(bool bDown) 
        {
            //Dome 안돌아가져있으면 하면안됨
            if (!m_dome.IsCheckRotate())
                return "Dome is ReadyPos";

            m_doDomeCoverDown.Write(bDown ? eCover.Down : eCover.Up);
            StopWatch sw = new StopWatch();
            int msDown = (int)(1000 * m_secCoverDown);
            while (sw.ElapsedMilliseconds < msDown)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
                if (m_dome.IsCoverDown(bDown)) return "OK";
            }
            return "Run DomeCoverDown Timeout";
        }
        public string RunDoorCoverDown(bool bDown)        {

            m_doDoorCoverDown.Write(bDown ? eCover.Down : eCover.Up);
            StopWatch sw = new StopWatch();
            int msDown = (int)(1000 * m_secCoverDown);
            while (sw.ElapsedMilliseconds < msDown)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
                if (m_door.IsCylinderDown(bDown)) return "OK";
            }
            return "Run DoorCoverDown Timeout";
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
            Camera_Basler camDome;
            public Axis m_axisRotate;
            DIO_Is m_diCheckRotate;
            DIO_Is m_diCheckDome;
            DIO_Os m_doClamp;
            DIO_Is[] m_diClamp = new DIO_Is[2] { null, null };
            DIO_I[] m_diCoverDown = new DIO_I[2];
            //LightSet lightSet;

            public void GetTools(ToolBox toolBox, bool bInit)
            {
                toolBox.GetCamera(ref camDome, m_EOP, p_id + ".Cam Dome");
                //toolBox.Get(ref lightSet, m_EOP);
                toolBox.GetAxis(ref m_axisRotate, m_EOP, p_id + ".Rotate");
                toolBox.GetDIO(ref m_diCheckRotate, m_EOP, p_id + ".Rotate", new string[] { "0", "1" });
                toolBox.GetDIO(ref m_diCheckDome, m_EOP, p_id + ".Check", new string[] { "0", "1" });
                toolBox.GetDIO(ref m_doClamp, m_EOP, p_id + ".Clamp", Enum.GetNames(typeof(eClamp)));
                toolBox.GetDIO(ref m_diClamp[0], m_EOP, p_id + ".Unclamp", new string[] { "0", "1", "2", "3" });
                toolBox.GetDIO(ref m_diClamp[1], m_EOP, p_id + ".Clamp", new string[] { "0", "1", "2", "3" });
                toolBox.GetDIO(ref m_diCoverDown[0], m_EOP, p_id + ".CoverUp");
                toolBox.GetDIO(ref m_diCoverDown[1], m_EOP, p_id + ".CoverDown");
                m_particleCounterSet.GetTools(toolBox, bInit);
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
                if (IsCheckDome()) //돔 있는데
                {
                    if (IsClamp(false)) //클램프 열려있으면 돌지마
                        return "UnClamp";
                }

                //돌기전에 Y축 뒤로 밀어
                string str = m_EOP.RunMove(EOP.ePos.Backward);
                if (!str.Equals("OK"))
                    return str;

                m_axisRotate.StartMove(ePos);
                return m_axisRotate.WaitReady();
            }
            #endregion

            #region LightSet
            protected List<double> m_aLightPower = new List<double>();
            protected void RunTreeLight(Tree tree)
            {
                if (m_EOP.lightSet == null) return;

                while (m_aLightPower.Count < m_EOP.lightSet.m_aLight.Count)
                    m_aLightPower.Add(0);
                for (int n = 0; n < m_aLightPower.Count; n++)
                {
                    m_aLightPower[n] = tree.Set(m_aLightPower[n], m_aLightPower[n], m_EOP.lightSet.m_aLight[n].m_sName, "Light Power (0 ~ 100 %%)");
                }
            }

            public void SetLight(bool bOn)
            {
                for (int n = 0; n < m_aLightPower.Count; n++)
                {
                    if (m_EOP.lightSet.m_aLight[n].m_light != null)
                        m_EOP.lightSet.m_aLight[n].m_light.p_fSetPower = bOn ? m_aLightPower[n] : 0;
                }
            }
            #endregion

            #region Snap

            public string RunDomeSnap()
            {
                try
                {
                    if (camDome == null)
                        camDome.FunctionConnect();

                    MemoryData mem = GlobalObjects.Instance.GetNamed<ImageData>("Dome").m_MemData;

                    SetLight(true);

                    camDome.Grab();
                    IntPtr ptr = mem.GetPtr();
                    byte[] arr = camDome.p_ImageData.m_aBuf;
                    int byteperpxl = camDome.p_ImageData.GetBytePerPixel();
                    int nCamHeight = camDome.p_sz.Y;
                    int nCamWidth = camDome.p_sz.X;
                    Parallel.For(0, nCamHeight, (j) =>
                    {
                        Marshal.Copy(arr, j * nCamWidth*byteperpxl, (IntPtr)((long)ptr + (j * mem.W)), nCamWidth*byteperpxl);
                    });

                }
                finally
                {
                    if (camDome != null)
                        camDome.StopGrab();

                    SetLight(false);
                }
                return "OK";
            }
            public void InitCamera()
            {
                if (camDome != null)
                    camDome.FunctionConnect();
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
                if(bDown) //내려왔냐고 물어봤을때
                    return !m_diCoverDown[0].p_bIn && m_diCoverDown[1].p_bIn;

                else //올라왔냐 물어봤을때
                    return m_diCoverDown[0].p_bIn && !m_diCoverDown[1].p_bIn;
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
                if(!bClamp)
                {
                    if (IsCheckRotate())
                        return "Rotated";
                }

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

            public bool IsClamp(bool bClamp)
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
                    value?.WriteReg();
                    OnPropertyChanged();
                }
            }

            Registry m_reg = null;
            public void ReadPod_Registry()
            {
                m_reg = new Registry("InfoPod");
                int nPod = m_reg.Read(p_id, -1);
                if (nPod < 0) return; 
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
                //돔은 가져가기전에
                // ReadyPos -> Clamp Open

                string str = RunRotate(ePos.Ready);
                if (!str.Equals("OK"))
                    return str;

                str = RunClamp(false);
                if (!str.Equals("OK"))
                    return str;

                return "OK";
            }

            public string BeforePut(InfoPod infoPod)
            {
                //돔은 놓기전에 ReadyPos -> Clamp Open
                string str = RunRotate(ePos.Ready);
                if (!str.Equals("OK"))
                    return str;

                str = RunClamp(false);
                if (!str.Equals("OK"))
                    return str;
                return "OK";
            }

            public string AfterGet()
            {
                //돔이 있으면 문제 있는거
                if (IsCheckDome())
                    return "Dome Exist";

                return "OK";
            }

            public string AfterPut()
            {
                //놓고난 다음에 
                //돔이 있으면

                //Clamp Close -> Rotate Pos

                if (!IsCheckDome())
                    return "Dome doesn't Exist";

                string str = RunClamp(true);
                if (!str.Equals("OK"))
                    return str;
                str = RunRotate(ePos.Rotate);
                if (!str.Equals("OK"))
                    return str;

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
                m_teach = tree.GetTree("Particle Counter").Set(m_teach, m_teach, m_EOP.p_id + " " + p_id, "RND RTR Teach");
            }
            #endregion

            #region Tree
            public void RunTree(Tree tree)
            {
                m_secClamp = tree.Set(m_secClamp, m_secClamp, "Clamp", "Run Clamp Timeout (sec)");
                m_particleCounterSet.RunTree(tree.GetTree("Particle Counter"));
            }
            #endregion

            public string p_id { get; set; }
            EOP m_EOP;
            public ParticleCounterSet m_particleCounterSet;
            public Dome(string id, EOP EOP)
            {
                p_id = id;
                m_EOP = EOP;
                VEGA_P vegaP = EOP.m_handler.m_VEGA;
                m_particleCounterSet = new ParticleCounterSet(EOP, vegaP, "Dome.");
                if (camDome != null)
                    camDome.FunctionConnect();
            }

            public void ThreadStop()
            {
                m_particleCounterSet.ThreadStop();
            }

            #region ModuleRun
            public void InitModuleRuns(bool bRecipe)
            {
                m_EOP.AddModuleRunList(new Run_DomeSnap(m_EOP), bRecipe, "Run Dome Snap");
            }
            public class Run_DomeSnap : ModuleRunBase
            {
                EOP m_module;
                public Run_DomeSnap(EOP module)
                {
                    m_module = module;
                    InitModuleRun(module);
                }
                public override ModuleRunBase Clone()
                {
                    Run_DomeSnap run = new Run_DomeSnap(m_module);
                    return run;
                }
                public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
                {
                    m_module.m_dome.RunTreeLight(tree.GetTree("Dome"));
                }
                public override string Run()
                {
                    return m_module.m_dome.RunDomeSnap();
                }
            }
            #endregion
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
            Camera_Basler camDoor;
            DIO_Is m_diCheckDoor;
            DIO_Os m_doCylinder;
            DIO_Is[] m_diCylinder = new DIO_Is[2] { null, null };
            DIO_Os m_doVac;
            DIO_I m_diCheckVac;
            DIO_I[] m_diCoverDown = new DIO_I[2];

            public void GetTools(ToolBox toolBox, bool bInit)
            {
                toolBox.GetCamera(ref camDoor, m_EOP, p_id + ".Cam Door");
                toolBox.GetDIO(ref m_diCheckVac, m_EOP, p_id + ".CheckVac");
                toolBox.GetDIO(ref m_doVac, m_EOP, p_id + ".Vac",Enum.GetNames(typeof(eVac)));
                toolBox.GetDIO(ref m_diCheckDoor, m_EOP, p_id + ".Check", new string[] { "0", "1" });
                toolBox.GetDIO(ref m_doCylinder, m_EOP, p_id + ".Cylinder", Enum.GetNames(typeof(eCylinder)));
                toolBox.GetDIO(ref m_diCylinder[0], m_EOP, p_id + ".Cylinder Down", new string[] { "0", "1" });
                toolBox.GetDIO(ref m_diCylinder[1], m_EOP, p_id + ".Cylinder Up", new string[] { "0", "1" });
                toolBox.GetDIO(ref m_diCoverDown[0], m_EOP, p_id + ".CoverUp");
                toolBox.GetDIO(ref m_diCoverDown[1], m_EOP, p_id + ".CoverDown");
                m_particleCounterSet.GetTools(toolBox, bInit);
                if (bInit) { }
            }
            #endregion

            #region Check Input
            public bool IsCheckDoor()
            {
                return m_diCheckDoor.ReadDI(0) && m_diCheckDoor.ReadDI(1);
            }

            public bool IsCylinderDown(bool bDown)
            {
                for (int n = 0; n < 2; n++)
                {
                    if (m_diCylinder[0].ReadDI(n) == bDown) return false;
                    if (m_diCylinder[1].ReadDI(n) == !bDown) return false;
                }
                return true;
            }
            public bool IsVac()
            {
                return m_diCheckVac.p_bIn;
            }
            public bool IsCoverDown(bool bDown)
            {
                if (bDown) //내려왔냐고 물어봤을때
                    return !m_diCoverDown[0].p_bIn && m_diCoverDown[1].p_bIn;

                else //올라왔냐고 물어봤을때
                    return m_diCoverDown[0].p_bIn && !m_diCoverDown[1].p_bIn;
            }
            #endregion

            #region Vac
            public string RunVac(bool bOn)
            {
                m_doVac.Write(bOn ? eVac.On : eVac.Off);

                StopWatch sw = new StopWatch();
                int msUp = (int)(1000 * m_secUp);
                while (sw.ElapsedMilliseconds < msUp)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                    if(bOn)
                    {
                        if (IsVac())
                            return "OK";
                    }
                    else
                    {
                        if (!IsVac())
                            return "OK";
                    }
                }
                return "Run Door Vac Timeout";
            }

            public enum eVac
            {
                Off,On
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

            public bool IsCylinder(bool bUp)
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
                    value?.WriteReg();
                    OnPropertyChanged();
                }
            }

            Registry m_reg = null;
            public void ReadPod_Registry()
            {
                m_reg = new Registry("InfoPod");
                int nPod = m_reg.Read(p_id, -1);
                if (nPod < 0) return;
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
                string str = RunCylinderUp(true);
                if (!str.Equals("OK"))
                    return str;
                str = RunVac(false);
                if (!str.Equals("OK"))
                    return str;
                return "OK"; 
            }

            public string BeforePut(InfoPod infoPod)
            {
                string str = RunCylinderUp(true);
                if (!str.Equals("OK"))
                    return str;
                str = RunVac(false);
                if (!str.Equals("OK"))
                    return str;
                return "OK";
            }

            public string AfterGet()
            {
                return RunCylinderUp(false);
            }

            public string AfterPut()
            {
                string str = RunCylinderUp(false);
                if (!str.Equals("OK"))
                    return str;
                str = RunVac(true);
                if (!str.Equals("OK"))
                    return str;
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
                m_teach = tree.GetTree("Particle Counter").Set(m_teach, m_teach, m_EOP.p_id + " " + p_id, "RND RTR Teach");
            }
            #endregion

            #region Tree
            public void RunTree(Tree tree)
            {
                m_secUp = tree.Set(m_secUp, m_secUp, "Cylinder Up", "Run Cylinder UpDown Timeout (sec)");
                m_particleCounterSet.RunTree(tree.GetTree("Particle Counter"));
            }
            #endregion

            #region LightSet
            protected List<double> m_aLightPower = new List<double>();
            protected void RunTreeLight(Tree tree)
            {
                if (m_EOP.lightSet == null) return;

                while (m_aLightPower.Count < m_EOP.lightSet.m_aLight.Count)
                    m_aLightPower.Add(0);
                for (int n = 0; n < m_aLightPower.Count; n++)
                {
                    m_aLightPower[n] = tree.Set(m_aLightPower[n], m_aLightPower[n], m_EOP.lightSet.m_aLight[n].m_sName, "Light Power (0 ~ 100 %%)");
                }
            }

            public void SetLight(bool bOn)
            {
                for (int n = 0; n < m_aLightPower.Count; n++)
                {
                    if (m_EOP.lightSet.m_aLight[n].m_light != null)
                        m_EOP.lightSet.m_aLight[n].m_light.p_fSetPower = bOn ? m_aLightPower[n] : 0;
                }
            }
            #endregion

            #region Snap

            public string RunDoorSnap()
            {
                try
                {
                    if (camDoor == null)
                        camDoor.FunctionConnect();

                    SetLight(true);

                    MemoryData mem = GlobalObjects.Instance.GetNamed<ImageData>("Door").m_MemData;

                    camDoor.Grab();
                    IntPtr ptr = mem.GetPtr();
                    byte[] arr = camDoor.p_ImageData.m_aBuf;
                    int byteperpxl = camDoor.p_ImageData.GetBytePerPixel();
                    int nCamHeight = camDoor.p_sz.Y;
                    int nCamWidth = camDoor.p_sz.X;
                    Parallel.For(0, nCamHeight, (j) =>
                    {
                        Marshal.Copy(arr, (int)(j * nCamWidth* byteperpxl), (IntPtr)((long)ptr + (j * mem.W)), nCamWidth* byteperpxl);
                    });

                }
                finally
                {
                    if(camDoor!=null)
                        camDoor.StopGrab();
                    SetLight(false);
                }
                return "OK";
            }
            public void InitCamera()
            {
                if (camDoor != null)
                    camDoor.FunctionConnect();
            }
            #endregion
            public string p_id { get; set; }
            EOP m_EOP;
            public ParticleCounterSet m_particleCounterSet;
            public Door(string id, EOP EOP)
            {
                p_id = id;
                m_EOP = EOP;
                VEGA_P vegaP = EOP.m_handler.m_VEGA;
                m_particleCounterSet = new ParticleCounterSet(EOP, vegaP, "Door.");
            }

            public void ThreadStop()
            {
                m_particleCounterSet.ThreadStop();
            }
            #region ModuleRun
            public void InitModuleRuns(bool bRecipe)
            {
                m_EOP.AddModuleRunList(new Run_DoorSnap(m_EOP), bRecipe, "Run DoorSnap");
            }
            public class Run_DoorSnap : ModuleRunBase
            {
                EOP m_module;
                public Run_DoorSnap(EOP module)
                {
                    m_module = module;
                    InitModuleRun(module);
                }
                public override ModuleRunBase Clone()
                {
                    Run_DoorSnap run = new Run_DoorSnap(m_module);
                    return run;
                }
                public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
                {
                    m_module.m_door.RunTreeLight(tree.GetTree("Door"));
                }
                public override string Run()
                {
                    return m_module.m_door.RunDoorSnap();
                }
            }
            #endregion
        }
        public Door m_door;
        void InitDoor()
        {
            m_door = new Door("Door", this);
        }
        #endregion

        #region Particle Counter
        public string RunParticleCounter(Run_ParticleCount runCount)
        {
            if (runCount.m_bCheckPod)
            {
                if(runCount.p_id.Contains("Dome"))
                    if (m_dome.IsCheckDome() == false) return "Dome Check Error";
                else if(runCount.p_id.Contains("Door"))
                    if (m_door.IsCheckDoor() == false) return "Door Check Error";
            }
            if (Run(RunMove(ePos.Backward))) return p_sInfo; 
            if (Run(m_dome.RunClamp(true))) return p_sInfo;
            try
            {
                if(runCount.p_id.Contains("Dome"))
                {
                    if (Run(RunDomeParticleCounter()))
                        return p_sInfo;
                }

                else if(runCount.p_id.Contains("Door"))
                {
                    if (Run(RunDoorParticleCounter()))
                        return p_sInfo;
                }
            }
            finally
            {
                string sMove = RunDomeCoverDown(false);
                sMove = RunMove(ePos.Backward);
                //m_door.RunCylinderUp(true);
                //if (sMove == "OK")
                //{
                //    m_dome.RunRotate(Dome.ePos.Ready);
                //    m_dome.RunClamp(false);
                //}
            }
            return "OK";
        }
        public string RunDomeParticleCounter()
        {
            if (Run(m_dome.RunDomeSnap())) return p_sInfo;
            if (Run(RunMove(ePos.Forward))) return p_sInfo;
            if (Run(RunDomeCoverDown(true))) return p_sInfo;
            //if (Run(m_dome.m_particleCounterSet.RunParticleCounter(runCount.m_dataDome.m_asNozzle))) return p_sInfo;
            if (Run(RunDomeCoverDown(false))) return p_sInfo;
            if (Run(RunMove(ePos.Backward))) return p_sInfo;

            return "OK";
        }
        public string RunDoorParticleCounter()
        {
            if (Run(m_door.RunDoorSnap())) return p_sInfo;
            if (Run(RunMove(ePos.Forward))) return p_sInfo;
            if (Run(RunDomeCoverDown(true))) return p_sInfo;
            //if (Run(m_door.m_particleCounterSet.RunParticleCounter(runCount.m_dataDoor.m_asNozzle))) return p_sInfo;
            if (Run(RunDomeCoverDown(false))) return p_sInfo;
            if (Run(RunMove(ePos.Backward))) return p_sInfo;

            return "OK";
        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
            RunMove(ePos.Backward);
            m_axis.WaitReady();
            if(!m_dome.m_axisRotate.p_bServoOn)
            {
                m_dome.m_axisRotate.ServoOn(true);
                if (Run(m_dome.m_axisRotate.StartHome())) 
                    return;
            }    
                
            m_dome.RunRotate(Dome.ePos.Ready);
            m_door.RunCylinderUp(true); 
        }

        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            //여기
            //m_dome.InitCamera();
            //m_door.InitCamera();

            if (m_dome.IsCoverDown(true))
                RunDomeCoverDown(false);
            if (m_door.IsCoverDown(true))
                RunDoorCoverDown(false);

            string sHome = m_axis.StartHome();
            m_axis.WaitReady();
            p_eState = sHome.Equals("OK")? eState.Ready : eState.Error;

            Reset(); 
            return sHome;
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeCoverDown(tree.GetTree("Cover"));
            m_dome.RunTree(tree.GetTree("Dome"));
            m_door.RunTree(tree.GetTree("Door"));
        }
        #endregion

        VEGA_P_Handler m_handler; 
        public EOP(string id, IEngineer engineer)
        {
            p_id = id; 
            m_handler = (VEGA_P_Handler)engineer.ClassHandler();
            InitDome();
            InitDoor(); 
            InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            m_dome.ThreadStop();
            m_door.ThreadStop(); 
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            AddModuleRunList(new Run_ParticleCount(this), true, "Run Particle Counter");
            AddModuleRunList(new Run_RunSol(this), false, "Run Sol Test");
            m_dome.m_particleCounterSet.InitModuleRuns(false);
            m_dome.InitModuleRuns(false);
            m_door.m_particleCounterSet.InitModuleRuns(false);
            m_door.InitModuleRuns(false);
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

        public class Run_ParticleCount : ModuleRunBase
        {
            EOP m_module;
            public Run_ParticleCount(EOP module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public class Data
            {
                public int m_nCount = 0;
                public List<string> m_asNozzle = new List<string>();
                public Data Clone()
                {
                    Data data = new Data();
                    data.m_nCount = m_nCount;
                    foreach (string sNozzle in m_asNozzle) data.m_asNozzle.Add(sNozzle);
                    return data; 
                }

                public void RunTree(Tree tree, List<string> asFile, bool bVisible)
                {
                    m_nCount = tree.Set(m_nCount, m_nCount, "Count", "Particle Count Repeat Count", bVisible);
                    while (m_asNozzle.Count < m_nCount) m_asNozzle.Add("");
                    while (m_asNozzle.Count > m_nCount) m_asNozzle.RemoveAt(m_asNozzle.Count - 1);
                    RunTreeNozzle(tree.GetTree("NozzleSet", false, bVisible), asFile, bVisible);
                }

                void RunTreeNozzle(Tree tree, List<string> asFile, bool bVisible)
                {
                    for (int n = 0; n < m_asNozzle.Count; n++)
                    {
                        m_asNozzle[n] = tree.Set(m_asNozzle[n], m_asNozzle[n], asFile, (n + 1).ToString("00"), "NozzleSet Name", bVisible);
                    }
                }
            }
            public Data m_dataDome = new Data();
            public Data m_dataDoor = new Data(); 
            public bool m_bCheckPod = true;
            public override ModuleRunBase Clone()
            {
                Run_ParticleCount run = new Run_ParticleCount(m_module);
                run.m_bCheckPod = m_bCheckPod;
                run.m_dataDome = m_dataDome.Clone();
                run.m_dataDoor = m_dataDoor.Clone(); 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bCheckPod = tree.Set(m_bCheckPod, m_bCheckPod, "Check Pod", "Check Pod State", bVisible);
                m_dataDome.RunTree(tree.GetTree("Dome", true, bVisible), m_module.m_dome.m_particleCounterSet.m_nozzleSet.p_asFile, bVisible);
                m_dataDoor.RunTree(tree.GetTree("Door", true, bVisible), m_module.m_door.m_particleCounterSet.m_nozzleSet.p_asFile, bVisible);
            }

            public override string Run()
            {
                return m_module.RunParticleCounter(this);
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
                    case eSol.EOP_Cover: return m_module.RunDomeCoverDown(m_bOn); 
                }
                return "OK";
            }
        }
        #endregion
    }
}
