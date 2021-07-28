using RootTools;
using RootTools.Camera;
using RootTools.Camera.Dalsa;
using RootTools.Camera.Matrox;
using RootTools.Comm;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Root_Pine2_Vision.Module
{
    public class Vision2D : ModuleBase
    {
        public enum eVision
        {
            Top3D,
            Top2D,
            Bottom
        }

        #region ToolBox
        Camera_Dalsa m_camera;
        public LightSet m_lightSet;
        RS232 m_rs232RGBW;
        Camera_Matrox m_CamMatrox;
        public override void GetTools(bool bInit)
        {
            if (p_eRemote == eRemote.Server)
            {
                p_sInfo = m_toolBox.GetCamera(ref m_camera, this, "Camera");
                p_sInfo = m_toolBox.Get(ref m_lightSet, this);
                m_aWorks[eWorks.A].GetTools(m_toolBox, bInit);
                m_aWorks[eWorks.B].GetTools(m_toolBox, bInit);
                p_sInfo = m_toolBox.GetComm(ref m_rs232RGBW, this, "RGBW"); 
                p_sInfo = m_toolBox.GetComm(ref m_tcpRequest, this, "Request");
                if (bInit)
                {
                    m_tcpRequest.EventReceiveData += M_tcpRequest_EventReceiveData;
                    m_rs232RGBW.p_bConnect = true;
                    if(m_camera != null)
                    m_camera.Connect();   
                    //
                   // RootTools.Camera.Matrox.Camera_Matrox camera_Matrox = new RootTools.Camera.Matrox.Camera_Matrox("dd",m_log); 
                }
            }
            m_remote.GetTools(bInit);
        }
        #endregion

        #region Light
        public class LightPower
        {
            public eRGBW m_eRGBW = eRGBW.White; 
            public List<double> m_aPower = new List<double>();

            public LightPower Clone()
            {
                LightPower power = new LightPower(m_vision);
                power.m_eRGBW = m_eRGBW; 
                for (int n = 0; n < m_vision.p_lLight; n++) power.m_aPower[n] = m_aPower[n];
                return power;
            }

            public void RunTree(Tree tree, bool bVisible)
            {
                m_eRGBW = (eRGBW)tree.Set(m_eRGBW, m_eRGBW, "RGBW", "Set RGBW", bVisible); 
                for (int n = 0; n < m_aPower.Count; n++)
                {
                    m_aPower[n] = tree.Set(m_aPower[n], m_aPower[n], n.ToString("00"), "Light Power (0 ~ 100)", bVisible);
                }
            }

            Vision2D m_vision;
            public LightPower(Vision2D vision)
            {
                m_vision = vision;
                while (m_aPower.Count < m_vision.p_lLight) m_aPower.Add(0);
            }
        }

        int _lLight = 6;
        public int p_lLight
        {
            get
            {
                return _lLight;
                //if (p_eRemote == eRemote.Client) return _lLight;
                //return m_lightSet.m_aLight.Count;
            }
            set
            {
                _lLight = value;
                //if (p_eRemote == eRemote.Client) _lLight = value;
            }
        }

        public void RunLight(LightPower lightPower)
        {
            if (p_eRemote == eRemote.Client) RemoteRun(eRemoteRun.RunLight, eRemote.Client, lightPower);
            else
            {
                SetRGBW(lightPower.m_eRGBW); 
                for (int n = 0; n < p_lLight; n++)
                {
                    Light light = m_lightSet.m_aLight[n];
                    if (light.m_light != null) light.m_light.p_fSetPower = lightPower.m_aPower[n];
                }
            }
        }

        public void RunLightOff()
        {
            for (int n = 0; n < p_lLight; n++)
            {
                Light light = m_lightSet.m_aLight[n];
                if (light.m_light != null) light.m_light.p_fSetPower = 0;
            }
        }
        #endregion

        #region RGBW
        public enum eRGBW
        {
            Red,
            Green,
            Blue,
            White
        }
        string SetRGBW(eRGBW eRGBW)
        {
            switch (eRGBW)
            {
                case eRGBW.Red: m_rs232RGBW.Send("r"); break;
                case eRGBW.Green: m_rs232RGBW.Send("g"); break;
                case eRGBW.Blue: m_rs232RGBW.Send("b"); break;
                case eRGBW.White: m_rs232RGBW.Send("w"); break;
            }
            return "OK";
        }
        #endregion

        #region p_sRecipe
        string _sRecipe = ""; 
        public string p_sRecipe
        {
            get { return _sRecipe; }
            set
            {
                if (_sRecipe == value) return;
                _sRecipe = value;
                m_aWorks[eWorks.A].SendRecipe(value);
                m_aWorks[eWorks.B].SendRecipe(value);
            }
        }

        public void SetRecipe(string sRecipe)
        {
            if (p_eRemote == eRemote.Client) RemoteRun(eRemoteRun.Recipe, eRemote.Client, sRecipe);
            else p_sRecipe = sRecipe; 
        }
        #endregion

        #region GrabData
        public int m_nLine = 78800;
        public class Grab
        {
            public int m_nFovStart = 0;
            public int m_nFovSize = 12000;
            public double[] m_dScale = new double[3] { 1, 1, 1 };
            public double[] m_dShift = new double[3] { 0, 0, 0 };
            public int[] m_yShift = new int[3] { 0, 0, 0 };

            public void SetData(GrabData data)
            {
                data.m_nFovStart = m_nFovStart;
                data.m_nFovSize = m_nFovSize;
                data.m_dScaleR = m_dScale[0];
                data.m_dScaleG = m_dScale[1];
                data.m_dScaleB = m_dScale[2];
                data.m_dShiftR = m_dShift[0];
                data.m_dShiftG = m_dShift[1];
                data.m_dShiftB = m_dShift[2];
                data.m_nYShiftR = m_yShift[0];
                data.m_nYShiftG = m_yShift[1];
                data.m_nYShiftB = m_yShift[2];
            }

            public void RunTree(Tree tree)
            {
                RunTreeFOV(tree.GetTree("FOV"));
                RunTreeColor(tree.GetTree("Red"), 0);
                RunTreeColor(tree.GetTree("Green"), 1);
                RunTreeColor(tree.GetTree("Blue"), 2);
            }

            void RunTreeFOV(Tree tree)
            {
                m_nFovStart = tree.Set(m_nFovStart, m_nFovStart, "Start", "FOV Start (pixel)");
                m_nFovSize = tree.Set(m_nFovSize, m_nFovSize, "Size", "FOV Size (pixel)");
            }

            void RunTreeColor(Tree tree, int i)
            {
                m_dScale[i] = tree.Set(m_dScale[i], m_dScale[i], "Scale", "Color Scale (ratio)");
                m_dShift[i] = tree.Set(m_dShift[i], m_dShift[i], "Shift", "Color Shift");
                m_yShift[i] = tree.Set(m_yShift[i], m_yShift[i], "Y Shift", "Color Shift");
            }
        }
        public Dictionary<eWorks, Grab> m_aGrabData = new Dictionary<eWorks, Grab>();
        void InitGrabData()
        {
            m_aGrabData.Add(eWorks.A, new Grab());
            m_aGrabData.Add(eWorks.B, new Grab());
        }
        #endregion

        #region Recipe
        public class Recipe
        {
            public class Snap
            {
                public RPoint m_dpAxis = new RPoint();

                public enum eDirection
                {
                    Forward,
                    Backward
                }
                public eDirection m_eDirection = eDirection.Forward;
                public enum eEXT
                {
                    EXT1,
                    EXT2,
                }
                public eEXT m_eEXT = eEXT.EXT1;
                public CPoint m_cpMemory = new CPoint();
                public int m_nOverlap = 0;
                public LightPower m_lightPower;

                public Snap Clone()
                {
                    Snap snap = new Snap(m_vision);
                    snap.m_dpAxis = new RPoint(m_dpAxis);
                    snap.m_eDirection = m_eDirection;
                    snap.m_eEXT = m_eEXT;
                    snap.m_cpMemory = new CPoint(m_cpMemory);
                    snap.m_nOverlap = m_nOverlap;
                    snap.m_lightPower = m_lightPower.Clone();
                    return snap;
                }

                public GrabData GetGrabData(eWorks eWorks)
                {
                    GrabData data = new GrabData();
                    data.bInvY = (m_eDirection == eDirection.Forward);
                    data.m_nOverlap = m_nOverlap;
                    data.nScanOffsetY = m_cpMemory.Y;
                    data.ReverseOffsetY = m_cpMemory.Y; /* + m_vision.m_nLine */
                    m_vision.m_aGrabData[eWorks].SetData(data); 
                    return data;
                }

                public CPoint GetMemoryOffset()
                {
                    CPoint cp = new CPoint(m_cpMemory);
                    if (m_eDirection == eDirection.Backward) cp.Y += m_vision.m_nLine;
                    return cp;
                }

                public void RunTree(Tree tree, bool bVisible)
                {
                    RunTreeStage(tree.GetTree("Stage", true, bVisible), bVisible);
                    RunTreeMemory(tree.GetTree("Memory", true, bVisible), bVisible);
                    m_lightPower.RunTree(tree.GetTree("Light", true, bVisible), bVisible);
                }

                void RunTreeStage(Tree tree, bool bVisible)
                {
                    m_dpAxis = tree.Set(m_dpAxis, m_dpAxis, "Offset", "Axis Offset (pulse)", bVisible);
                    m_eDirection = (eDirection)tree.Set(m_eDirection, m_eDirection, "Direction", "Scan Direction", bVisible);
                }

                void RunTreeMemory(Tree tree, bool bVisible)
                {
                    m_eEXT = (eEXT)tree.Set(m_eEXT, m_eEXT, "EXT", "Select EXT", bVisible);
                    m_cpMemory = tree.Set(m_cpMemory, m_cpMemory, "Offset", "Memory Offset Address (pixel)", bVisible);
                    m_nOverlap = tree.Set(m_nOverlap, m_nOverlap, "Overlap", "Memory Overlap Size (pixel)", bVisible);
                }

                Vision2D m_vision;
                public Snap(Vision2D vision)
                {
                    m_vision = vision;
                    m_lightPower = new LightPower(vision);
                }
            }
            public List<Snap> m_aSnap = new List<Snap>();

            public int p_lSnap
            {
                get { return m_aSnap.Count; }
                set
                {
                    if (m_aSnap.Count == value) return;
                    while (m_aSnap.Count > value) m_aSnap.RemoveAt(m_aSnap.Count - 1);
                    while (m_aSnap.Count < value) m_aSnap.Add(new Snap(m_vision));
                }
            }
            public eWorks m_eWorks = eWorks.A;

            public Recipe Clone()
            {
                Recipe recipe = new Recipe(m_vision, m_eWorks);
                recipe.m_eWorks = m_eWorks;
                foreach (Snap snap in m_aSnap) recipe.m_aSnap.Add(snap.Clone());
                return recipe; 
            }

            const string c_sExt = ".pine2";
            public void RecipeSave(string sRecipe)
            {
                string sPath = EQ.c_sPathRecipe + "\\" + sRecipe;
                Directory.CreateDirectory(sPath); 
                string sFile = sPath + "\\" + m_vision.m_eVision.ToString() + m_eWorks.ToString() + c_sExt;
                m_treeRecipe.m_job = new Job(sFile, true, m_vision.m_log);
                RunTreeRecipe(Tree.eMode.JobSave);
                m_treeRecipe.m_job.Close();
            }

            public void RecipeOpen(string sRecipe)
            {
                string sPath = EQ.c_sPathRecipe + "\\" + sRecipe;
                Directory.CreateDirectory(sPath);
                string sFile = sPath + "\\" + m_vision.m_eVision.ToString() + m_eWorks.ToString() + c_sExt;
                m_treeRecipe.m_job = new Job(sFile, false, m_vision.m_log);
                RunTreeRecipe(Tree.eMode.JobOpen);
                m_treeRecipe.m_job.Close();
            }


            #region TreeRecipe
            public TreeRoot m_treeRecipe;
            void InitTreeRecipe()
            {
                m_treeRecipe = new TreeRoot(m_vision.p_id, m_vision.m_log);
                m_treeRecipe.UpdateTree += M_treeRecipe_UpdateTree;
            }

            private void M_treeRecipe_UpdateTree()
            {
                int lSnap = p_lSnap;
                RunTreeRecipe(Tree.eMode.Update);
                if (lSnap != p_lSnap) RunTreeRecipe(Tree.eMode.Init);
            }


            public void RunTreeRecipe(Tree.eMode eMode)
            {
                m_treeRecipe.p_eMode = eMode;
                RunTreeRecipe(m_treeRecipe, true, true); 
            }

            public void RunTreeRecipe(Tree tree, bool bVisible, bool bReadOnly = false)
            {
                tree.Set(m_eWorks, m_eWorks, "Works", "Vision eWorks", bVisible, bReadOnly);
                p_lSnap = tree.Set(p_lSnap, p_lSnap, "Count", "Snap Count", bVisible);
                for (int n = 0; n < m_aSnap.Count; n++) m_aSnap[n].RunTree(tree.GetTree("Snap" + n.ToString("00"), true, bVisible), bVisible);

            }
            #endregion

            Vision2D m_vision; 
            public Recipe(Vision2D vision, eWorks eWorks)
            {
                m_vision = vision;
                m_eWorks = eWorks;
                InitTreeRecipe(); 
            }
        }
        public Dictionary<eWorks, Recipe> m_recipe = new Dictionary<eWorks, Recipe>();
        void InitRecipe()
        {
            m_recipe.Add(eWorks.A, new Recipe(this, eWorks.A));
            m_recipe.Add(eWorks.B, new Recipe(this, eWorks.B));
        }

        public List<string> GetRecipeList()
        {
            List<string> asRecipe = new List<string>();
            DirectoryInfo info = new DirectoryInfo(EQ.c_sPathRecipe);
            foreach (DirectoryInfo dir in info.GetDirectories()) asRecipe.Add(dir.Name);
            return asRecipe;
        }

        #endregion

        #region Works
        public enum eWorks
        {
            A,
            B,
        }
        public Dictionary<eWorks, Works2D> m_aWorks = new Dictionary<eWorks, Works2D>(); 
        void InitVisionWorks()
        {
            m_aWorks.Add(eWorks.A, new Works2D(eWorks.A, this));
            m_aWorks.Add(eWorks.B, new Works2D(eWorks.B, this));
        }
        #endregion

        #region RunSnap
        public string StartSnap(Recipe.Snap recipe, eWorks eWorks, int iSnap)
        {
            Run_Snap run = (Run_Snap)m_runSnap.Clone();
            run.m_eWorks = eWorks; 
            run.m_recipe = recipe;
            run.m_iSnap = iSnap; 
            return StartRun(run); 
        }

        public string RunSnap(Recipe.Snap recipe, eWorks eWorks, int iSnap)
        {
            EQ.p_bStop = false; 
            MemoryData memory = m_aWorks[eWorks].p_memSnap[(int)recipe.m_eEXT];
            CPoint cpOffset = recipe.GetMemoryOffset();
            GrabData grabData = recipe.GetGrabData(eWorks);
            try
            {

                m_camera.GrabLineScan(memory, cpOffset, m_nLine, grabData);
                Thread.Sleep(200);
                while (m_camera.p_CamInfo.p_eState != RootTools.Camera.Dalsa.eCamState.Ready)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                }
                //m_aWorks[eWorks].SendSnapDone(iSnap); 
            }
            catch
            {
                m_camera.StopGrab(); 
                //RunLightOff(); 
            }
            return "OK";
        }
        #endregion

        #region Request
        int m_nReq = 0;
        string m_sReceive = ""; 
        TCPAsyncClient m_tcpRequest;
        private void M_tcpRequest_EventReceiveData(byte[] aBuf, int nSize, Socket socket)
        {
            m_sReceive = Encoding.Default.GetString(aBuf, 0, nSize);
        }

        public string ReqSnap(string sRecipe, eWorks eWorks)
        {
            string sSend = m_nReq.ToString("000") + "," + Works2D.eProtocol.Snap.ToString() + "," + sRecipe + "," + eWorks.ToString();
            m_sReceive = "";
            m_tcpRequest.Send(sSend); 
            while (sSend != m_sReceive)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
            }
            return "OK"; 
        }
        #endregion

        #region override
        public override void Reset()
        {
            if (p_eRemote == eRemote.Client) RemoteRun(eRemoteRun.Reset, eRemote.Client, null);
            m_aWorks[eWorks.A].Reset();
            m_aWorks[eWorks.B].Reset();
            base.Reset();
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.StateHome, eRemote.Client, null);
            else p_eState = eState.Ready;
            return "OK";
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            if (p_eRemote == eRemote.Client)
            {
                p_lLight = tree.GetTree("Light").Set(p_lLight, p_lLight, "Channel", "Light Channel Count");
            }
            else
            {
                p_lLight = tree.GetTree("Light").Set(p_lLight, p_lLight, "Channel", "Light Channel Count");
                m_eVision = (eVision)tree.GetTree("Vision").Set(m_eVision, m_eVision, "Type", "Vision Type"); 
                m_nLine = tree.GetTree("Camera").Set(m_nLine, m_nLine, "Line", "Memory Snap Lines (pixel)");
                m_aWorks[eWorks.A].RunTree(tree.GetTree("Works " + m_aWorks[eWorks.A].p_id));
                m_aWorks[eWorks.B].RunTree(tree.GetTree("Works " + m_aWorks[eWorks.B].p_id));
                m_aGrabData[eWorks.A].RunTree(tree.GetTree("GrabData A"));
                m_aGrabData[eWorks.B].RunTree(tree.GetTree("GrabData B"));
            }
        }
        #endregion

        #region Vision_Snap_UI
        Recipe_UI m_ui;
        void InitVision_Snap_UI()
        {
            m_ui = new Recipe_UI();
            m_ui.Init(this);
            m_aTool.Add(m_ui);
        }
        #endregion

        public eVision m_eVision = eVision.Top2D; 
        public Vision2D(eVision eVision, IEngineer engineer, eRemote eRemote)
        {
            m_eVision = eVision;
            InitVisionWorks();
            InitBase("Vision " + eVision.ToString(), engineer, eRemote);
        }

        public Vision2D(string id, IEngineer engineer, eRemote eRemote)
        {
            InitGrabData();
            InitVisionWorks();
            InitRecipe();
            InitBase(id, engineer, eRemote);
            InitVision_Snap_UI();
        }

        public override void ThreadStop()
        {
            foreach (Works2D works in m_aWorks.Values) works.ThreadStop(); 
            base.ThreadStop();
        }

        #region RemoteRun
        public enum eRemoteRun
        {
            StateHome,
            Reset,
            RunLight,
            Recipe
        }

        Run_Remote GetRemoteRun(eRemoteRun eRemoteRun, eRemote eRemote, dynamic value)
        {
            Run_Remote run = new Run_Remote(this);
            run.m_eRemoteRun = eRemoteRun;
            run.m_eRemote = eRemote;
            switch (eRemoteRun)
            {
                case eRemoteRun.StateHome: break;
                case eRemoteRun.Reset: break;
                case eRemoteRun.RunLight: run.m_lightPower = value; break;
                case eRemoteRun.Recipe: run.m_sRecipe = value; break; 
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
            Vision2D m_module;
            public Run_Remote(Vision2D module)
            {
                m_module = module;
                m_lightPower = new LightPower(m_module); 
                InitModuleRun(module);
            }

            public eRemoteRun m_eRemoteRun = eRemoteRun.StateHome;
            public LightPower m_lightPower;
            public string m_sRecipe = ""; 
            public override ModuleRunBase Clone()
            {
                Run_Remote run = new Run_Remote(m_module);
                run.m_eRemoteRun = m_eRemoteRun;
                run.m_lightPower = m_lightPower.Clone();
                run.m_sRecipe = m_sRecipe; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eRemoteRun = (eRemoteRun)tree.Set(m_eRemoteRun, m_eRemoteRun, "RemoteRun", "Select Remote Run", bVisible);
                m_eRemote = (eRemote)tree.Set(m_eRemote, m_eRemote, "Remote", "Remote", false);
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.RunLight: m_lightPower.RunTree(tree.GetTree("Light Power", true, bVisible), bVisible); break;
                    case eRemoteRun.Recipe: m_sRecipe = tree.Set(m_sRecipe, m_sRecipe, "Recipe", "Recipe", bVisible); break; 
                    default: break; 
                }
            }

            public override string Run()
            {
                EQ.p_bStop = false; 
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.StateHome: return m_module.StateHome();
                    case eRemoteRun.Reset: m_module.Reset(); break;
                    case eRemoteRun.RunLight: m_module.RunLight(m_lightPower); break;
                    case eRemoteRun.Recipe: m_module.SetRecipe(m_sRecipe); break; 
                }
                return "OK";
            }
        }
        #endregion

        #region ModuleRun
        ModuleRunBase m_runSnap; 
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Remote(this), true, "Remote Run");
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            m_runSnap = AddModuleRunList(new Run_Snap(this), true, "Snap");
            AddModuleRunList(new Run_ReqSnap(this), true, "Snap Request");
        }

        public class Run_Delay : ModuleRunBase
        {
            Vision2D m_module;
            public Run_Delay(Vision2D module)
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

        public class Run_Snap : ModuleRunBase
        {
            Vision2D m_module;
            public Run_Snap(Vision2D module)
            {
                m_module = module;
                m_recipe = new Recipe.Snap(module); 
                InitModuleRun(module);
            }

            public eWorks m_eWorks = eWorks.A;
            public int m_iSnap = 0; 
            public Recipe.Snap m_recipe; 
            public override ModuleRunBase Clone()
            {
                Run_Snap run = new Run_Snap(m_module);
                run.m_eWorks = m_eWorks; 
                run.m_recipe = m_recipe.Clone();
                run.m_iSnap = m_iSnap; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eWorks = (eWorks)tree.Set(m_eWorks, m_eWorks, "Works", "Vision eWorks", bVisible);
                m_iSnap = tree.Set(m_iSnap, m_iSnap, "Snap Index", "Snap Index", bVisible); 
                m_recipe.RunTree(tree, bVisible); 
            }

            public override string Run()
            {
                return m_module.RunSnap(m_recipe, m_eWorks, m_iSnap);
            }
        }

        public class Run_ReqSnap : ModuleRunBase
        {
            Vision2D m_module;
            public Run_ReqSnap(Vision2D module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eWorks m_eWorks = eWorks.A;
            public string m_sRecipe = "";
            public override ModuleRunBase Clone()
            {
                Run_ReqSnap run = new Run_ReqSnap(m_module);
                run.m_eWorks = m_eWorks;
                run.m_sRecipe = m_sRecipe;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eWorks = (eWorks)tree.Set(m_eWorks, m_eWorks, "Works", "Vision eWorks", bVisible);
                m_sRecipe = tree.Set(m_sRecipe, m_sRecipe, m_module.GetRecipeList(), "Recipe", "Recipe", bVisible);
            }

            public override string Run()
            {
                return m_module.ReqSnap(m_sRecipe, m_eWorks);
            }
        }
        #endregion

    }
}
