using RootTools;
using RootTools.Camera;
using RootTools.Camera.Dalsa;
using RootTools.Comm;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace Root_Pine2_Vision.Module
{
    public class Vision2D : ModuleBase
    {
        #region Property
        public enum eVision
        {
            Top3D,
            Top2D,
            Bottom
        }
        #endregion

        #region ToolBox
        Camera_Dalsa m_camera;
        public LightSet m_lightSet;
        RS232 m_rs232RGBW;
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
                    m_camera.Connect();
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

            public static bool IsSameLight(LightPower lightPower1, LightPower lightPower2)
            {
                if (lightPower1.m_aPower.Count != lightPower2.m_aPower.Count) return false;
                if (lightPower1.m_eRGBW != lightPower2.m_eRGBW) return false;

                for(int i = 0; i < lightPower1.m_aPower.Count; i++)
                {
                    if (lightPower1.m_aPower[i] != lightPower2.m_aPower[i]) return false;
                }

                return true;
            }
        }

        int _lLight = 6;
        public int p_lLight
        {
            get { return _lLight; }
            set
            {
                _lLight = value;
                OnPropertyChanged();
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

        #region Send Recipe
        public string SendRecipe(string sRecipe)
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.SendRecipe, eRemote.Client, sRecipe);
            else
            {
                //m_recipe[eWorks.A].RecipeOpen(sRecipe);
                //m_recipe[eWorks.B].RecipeOpen(sRecipe);
                m_RunningRecipe[eWorks.A].RecipeOpen(sRecipe);
                m_RunningRecipe[eWorks.B].RecipeOpen(sRecipe);
                string sRunA = m_aWorks[eWorks.A].SendRecipe(sRecipe);
                string sRunB = m_aWorks[eWorks.B].SendRecipe(sRecipe);
                if ((sRunA == "OK") && (sRunB == "OK")) return "OK";
                return "A = " + sRunA + ", B = " + sRunB; 
            }
        }
        #endregion

        #region Send SnapInfo
        public string SendSnapInfo(eWorks eWorks)
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.SendSnapInfo, eRemote.Client, eWorks);
            else
            {
                //int nSnapCount = m_recipe[eWorks].p_lSnap;               // 총 Snap 횟수
                //int nSnapMode = (int)m_recipe[eWorks].p_eSnapMode;       // Snap Mode (RGB, APS, ALL)
                //return m_aWorks[eWorks].SendSnapInfo(m_recipe[eWorks].m_sRecipe, nSnapMode, nSnapCount); // 3. VisionWorks2 Receive SnapInfo
                
                int nSnapCount = m_RunningRecipe[eWorks].p_lSnap;               // 총 Snap 횟수
                int nSnapMode = (int)m_RunningRecipe[eWorks].p_eSnapMode;       // Snap Mode (RGB, APS, ALL)
                return m_aWorks[eWorks].SendSnapInfo(m_RunningRecipe[eWorks].m_sRecipe, nSnapMode, nSnapCount); // 3. VisionWorks2 Receive SnapInfo
            }
        }
        #endregion

        #region GrabData
        public int m_nLine = 78800;
        public bool m_bUseBiDirectional = true;
        public class Grab
        {
            public int m_nFovStart = 0;
            public int m_nFovSize = 12000;
            public int m_nReverseOffset = 0;
            public int m_nOverlap = 0;
            public int m_nYOffset = 0;
            public double m_dResolution = 3.8;
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
                RunTreeImage(tree.GetTree("Image"));
                RunTreeFOV(tree.GetTree("FOV"));
                RunTreeColor(tree.GetTree("Red"), 0);
                RunTreeColor(tree.GetTree("Green"), 1);
                RunTreeColor(tree.GetTree("Blue"), 2);
            }

            void RunTreeImage(Tree tree)
            {
                m_dResolution = tree.Set(m_dResolution, m_dResolution, "Pixel Resolution", "Pixel Resolution (um/pixel)");
                m_nReverseOffset = tree.Set(m_nReverseOffset, m_nReverseOffset, "Reverse Offset", "Reverse Offset (pixel)");
                m_nOverlap = tree.Set(m_nOverlap, m_nOverlap, "Overlap", "Overlap (pixel)");
                m_nYOffset = tree.Set(m_nYOffset, m_nYOffset, "YOffset", "YOffset (pixel)");
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
            public enum eSnapMode
            {
                RGB,
                APS,
                ALL
            }

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

                public GrabData GetGrabData(eWorks eWorks, CPoint cpOffset, int nOverlap)
                {
                    GrabData data = new GrabData();
                    data.bInvY = (m_eDirection == eDirection.Forward);
                    data.m_nOverlap = nOverlap;
                    data.nScanOffsetY = 0;   /*m_cpMemory.Y;*/
                    data.ReverseOffsetY = cpOffset.Y; /*m_cpMemory.Y;*/ /* + m_vision.m_nLine */
                    data.m_bUseFlipVertical = true;
                    m_vision.m_aGrabData[eWorks].SetData(data);
                    return data;
                }

                public CPoint GetMemoryOffset()
                {
                    CPoint cp = new CPoint(m_cpMemory);
                    //if (m_eDirection == eDirection.Backward) cp.Y += m_vision.m_nLine;
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
                    m_dpAxis = tree.Set(m_dpAxis, m_dpAxis, "Offset", "Axis Offset (mm)", bVisible);
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
            public string m_sRecipe = "";
            public int _lSnap = 0;
            public int p_lSnap
            {
                get { return _lSnap; /*m_aSnap.Count;*/ }
                set
                {
                    _lSnap = value;

                    if (m_treeRecipe.p_eMode == Tree.eMode.JobOpen && m_vision.p_eRemote == eRemote.Client)
                    {
                        while (m_aSnap.Count > value) m_aSnap.RemoveAt(m_aSnap.Count - 1);
                        while (m_aSnap.Count < value) m_aSnap.Add(new Snap(m_vision));
                    }

                    //if (m_aSnap.Count == value) return;
                    //while (m_aSnap.Count > value) m_aSnap.RemoveAt(m_aSnap.Count - 1);
                    //while (m_aSnap.Count < value) m_aSnap.Add(new Snap(m_vision));

                    //double nAxisOffset = m_vision.m_aGrabData[m_eWorks].m_nFovSize * 3.8 / 1000;
                    //for (int i = 0; i < m_aSnap.Count; i++)
                    //{
                    //    RPoint rp = new RPoint(i * nAxisOffset, 0);
                    //    m_aSnap[i].m_dpAxis = rp;
                    //}
                }
            }

            public eSnapMode _eSnapMode = 0;
            public eSnapMode p_eSnapMode
            {
                get { return _eSnapMode; }
                set
                {
                    _eSnapMode = value;
                }
            }

            public double _dProductWidth = 0;
            public double p_dProductWidth
            {
                get { return _dProductWidth; }
                set
                {
                    _dProductWidth = value;
                    if (m_treeRecipe.p_eMode == Tree.eMode.JobOpen && m_vision.p_eRemote == eRemote.Client) return;
                    
                    m_aSnap.Clear();
                    int nFOVpx = m_vision.m_aGrabData[m_eWorks].m_nFovSize;
                    int nOverlap = m_vision.m_aGrabData[m_eWorks].m_nOverlap;
                    double dResolution = m_vision.m_aGrabData[m_eWorks].m_dResolution;
                    double dFOVmm = nFOVpx * dResolution / 1000;
                    int nSnapCount = (int)Math.Ceiling(_dProductWidth / dFOVmm);    // 제품 전체를 찍기위한 스냅 횟수

                    if (p_eSnapMode == eSnapMode.ALL)
                        p_lSnap = nSnapCount * 2;   // ALL인 경우 총 Snap횟수. (RGB + APS)
                    else
                        p_lSnap = nSnapCount;       // RGB 또는 APS인 경우 총 Snap 횟수.

                    double dSnapStartXPos = (dFOVmm / 2) * (nSnapCount - 1); // Stage Center에서부터 첫 Snap 위치까지 거리
                    double dStageXOffset = 0;
                    int nSnapLineIndex = 0;
                    for (int i = 0; i < p_lSnap; i++)
                    {
                        nSnapLineIndex = i % nSnapCount;    // nSnapCount = 3인경우, RGB(0,1,2), APS(0,1,2), ALL(0,1,2,0,1,2)
                        m_aSnap.Add(new Snap(m_vision));
                        dStageXOffset = dSnapStartXPos - dFOVmm * nSnapLineIndex;
                        m_aSnap[i].m_dpAxis = new RPoint(dStageXOffset, 0);
                        m_aSnap[i].m_nOverlap = nOverlap;

                        if (nSnapLineIndex % 2 == 0)  // 정방향
                        {
                            m_aSnap[i].m_eDirection = Snap.eDirection.Forward;
                            //m_aSnap[i].m_cpMemory = new CPoint(nSnapLineIndex * nFOVpx, 0);
                        }
                        else  // 역방향
                        {
                            m_aSnap[i].m_eDirection = Snap.eDirection.Backward;
                            //m_aSnap[i].m_cpMemory = new CPoint(nSnapLineIndex * nFOVpx, nReverseOffset);
                        }

                        if (p_eSnapMode == eSnapMode.RGB)
                        {
                            m_aSnap[i].m_eEXT = Snap.eEXT.EXT1;
                            m_aSnap[i].m_lightPower = m_lightPowerRGB.Clone();
                        }
                        else if (p_eSnapMode == eSnapMode.APS)
                        {
                            m_aSnap[i].m_eEXT = Snap.eEXT.EXT2;
                            m_aSnap[i].m_lightPower = m_lightPowerAPS.Clone();
                        }
                        else if (p_eSnapMode == eSnapMode.ALL)
                        {
                            if (i / nSnapCount == 0)  // RGB
                            {
                                m_aSnap[i].m_eEXT = Snap.eEXT.EXT1;
                                m_aSnap[i].m_lightPower = m_lightPowerRGB.Clone();
                            }
                            else  // APS
                            {
                                m_aSnap[i].m_eEXT = Snap.eEXT.EXT2;
                                m_aSnap[i].m_lightPower = m_lightPowerAPS.Clone();
                            }
                        }
                    }
                }
            }

            public eWorks m_eWorks = eWorks.A;

            public Recipe Clone()
            {
                Recipe recipe = new Recipe(m_vision, m_eWorks);
                recipe.m_eWorks = m_eWorks;
                recipe.m_sRecipe = m_sRecipe;
                recipe.m_lightPowerRGB = m_lightPowerRGB.Clone();
                recipe.m_lightPowerAPS = m_lightPowerAPS.Clone();
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
                m_sRecipe = sRecipe;
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
                //int lSnap = p_lSnap;
                //RunTreeRecipe(Tree.eMode.Update);
                //if (lSnap != p_lSnap) RunTreeRecipe(Tree.eMode.Init);

                double dProductWidth = p_dProductWidth;
                eSnapMode eSnapMode = p_eSnapMode;
                LightPower lightPowerRGB = m_lightPowerRGB.Clone();
                LightPower lightPowerAPS = m_lightPowerAPS.Clone();

                RunTreeRecipe(Tree.eMode.Update);
                if (dProductWidth != p_dProductWidth || eSnapMode != p_eSnapMode || !LightPower.IsSameLight(lightPowerRGB, m_lightPowerRGB) || !LightPower.IsSameLight(lightPowerAPS, m_lightPowerAPS))
                    RunTreeRecipe(Tree.eMode.Init);
            }


            public void RunTreeRecipe(Tree.eMode eMode)
            {
                m_treeRecipe.p_eMode = eMode;
                RunTreeRecipe(m_treeRecipe, true, true);
            }

            public LightPower m_lightPowerRGB, m_lightPowerAPS;
            public void RunTreeRecipe(Tree tree, bool bVisible, bool bReadOnly = false)
            {
                tree.Set(m_eWorks, m_eWorks, "Works", "Vision eWorks", bVisible, bReadOnly);
                p_eSnapMode = (eSnapMode)tree.Set(p_eSnapMode, p_eSnapMode, "Snap Mode", "Select Snap Mode", bVisible);
                p_dProductWidth = tree.Set(p_dProductWidth, p_dProductWidth, "Product Width", "Product Width(mm)", bVisible);
                p_lSnap = tree.Set(p_lSnap, p_lSnap, "Count", "Snap Count", bVisible, true);

                if (!(m_treeRecipe.p_eMode == Tree.eMode.JobOpen && m_vision.p_eRemote == eRemote.Client))
                {
                    if (p_eSnapMode == eSnapMode.RGB || p_eSnapMode == eSnapMode.ALL)
                        m_lightPowerRGB.RunTree(tree.GetTree("Light").GetTree("RGB Light", true, bVisible), bVisible);
                    if (p_eSnapMode == eSnapMode.APS || p_eSnapMode == eSnapMode.ALL)
                        m_lightPowerAPS.RunTree(tree.GetTree("Light").GetTree("APS Light", true, bVisible), bVisible);
                }

                for (int n = 0; n < m_aSnap.Count; n++)
                    m_aSnap[n].RunTree(tree.GetTree("Snap").GetTree("Snap" + n.ToString("00"), false, bVisible, true), bVisible);

            }
            #endregion

            Vision2D m_vision;
            public Recipe(Vision2D vision, eWorks eWorks)
            {
                m_vision = vision;
                m_eWorks = eWorks;
                m_lightPowerRGB = new LightPower(vision);
                m_lightPowerAPS = new LightPower(vision);
                InitTreeRecipe();
            }
        }

        public Dictionary<eWorks, Recipe> m_UIRecipe = new Dictionary<eWorks, Recipe>();          // Recipe Teach용 Recipe (UI) 
        public Dictionary<eWorks, Recipe> m_RunningRecipe = new Dictionary<eWorks, Recipe>();   // 현재 Run 중인 Recipe
        void InitRecipe()
        {
            m_RunningRecipe.Add(eWorks.A, new Recipe(this, eWorks.A));
            m_RunningRecipe.Add(eWorks.B, new Recipe(this, eWorks.B));
        }

        public List<string> p_asRecipe
        {
            get
            {
                List<string> asRecipe = new List<string>();
                DirectoryInfo info = new DirectoryInfo(EQ.c_sPathRecipe);
                foreach (DirectoryInfo dir in info.GetDirectories()) asRecipe.Add(dir.Name);
                return asRecipe;
            }
            set { }
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
            int nFOVpx = m_aGrabData[eWorks].m_nFovSize;
            int nReverseOffset = m_aGrabData[eWorks].m_nReverseOffset;
            int nOverlap = m_aGrabData[eWorks].m_nOverlap;
            int nYOffset = m_aGrabData[eWorks].m_nYOffset;
            //Recipe.eSnapMode nSnapMode = m_recipe[eWorks].p_eSnapMode;
            //int nTotalSnap = m_recipe[eWorks].p_lSnap;
            Recipe.eSnapMode nSnapMode = m_RunningRecipe[eWorks].p_eSnapMode;
            int nTotalSnap = m_RunningRecipe[eWorks].p_lSnap;
            int nSnapLineIndex = (nSnapMode == Recipe.eSnapMode.ALL) ? iSnap % (nTotalSnap / 2) : iSnap % (nTotalSnap);

            CPoint cpOffset;    // 이미지 시작점
            if (m_bUseBiDirectional)
            {
                if (recipe.m_eDirection == Recipe.Snap.eDirection.Forward)
                    cpOffset = new CPoint(nSnapLineIndex * nFOVpx, nReverseOffset);
                else
                    cpOffset = new CPoint(nSnapLineIndex * nFOVpx, 0);
            }
            else
            {
                recipe.m_eDirection = Recipe.Snap.eDirection.Forward;
                cpOffset = new CPoint(nSnapLineIndex * nFOVpx, nReverseOffset);
            }

            MemoryData memory = m_aWorks[eWorks].p_memSnap[(int)recipe.m_eEXT];
            GrabData grabData = recipe.GetGrabData(eWorks, cpOffset, nOverlap);
            grabData.nScanOffsetY = (nSnapLineIndex ) * nYOffset;

            DalsaParameterSet.eUserSet nUserset = (recipe.m_eEXT == Recipe.Snap.eEXT.EXT1) ? DalsaParameterSet.eUserSet.UserSet2 : DalsaParameterSet.eUserSet.UserSet3;  // RGB : Userset2 , APS : Userset3
            
            try
            {
                if (m_camera.p_CamParam.p_eUserSetCurrent != nUserset)
                    m_camera.p_CamParam.p_eUserSetCurrent = nUserset;
                m_camera.GrabLineScan(memory, cpOffset, m_nLine, grabData);
                while (m_camera.p_CamInfo.p_eState != eCamState.Ready)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                }

                // Root Vision -> VisionWorks2
                if (m_aWorks[eWorks].IsProcessRun())
                    m_aWorks[eWorks].SendSnapDone(iSnap);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                m_camera.StopGrab();
            }
            finally
            {
                RunLightOff();
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
            m_aWorks[eWorks.A].Reset();
            m_aWorks[eWorks.B].Reset();
            base.Reset();
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            p_eState = eState.Ready;
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
                m_bUseBiDirectional = tree.GetTree("Camera").Set(m_bUseBiDirectional, m_bUseBiDirectional, "BiDirectional Scan", "Use BiDirectional Scan");
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
            m_UIRecipe.Add(eWorks.A, new Recipe(this, eWorks.A));
            m_UIRecipe.Add(eWorks.B, new Recipe(this, eWorks.B));
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
            RunLight,
            SendRecipe,
            SendSnapInfo
        }

        Run_Remote GetRemoteRun(eRemoteRun eRemoteRun, eRemote eRemote, dynamic value)
        {
            Run_Remote run = new Run_Remote(this);
            run.m_eRemoteRun = eRemoteRun;
            run.m_eRemote = eRemote;
            switch (eRemoteRun)
            {
                case eRemoteRun.StateHome: break;
                case eRemoteRun.RunLight: run.m_lightPower = value; break;
                case eRemoteRun.SendRecipe: run.m_sRecipe = value; break;
                case eRemoteRun.SendSnapInfo: run.m_eWorks = value; break;
            }
            return run;
        }

        string RemoteRun(eRemoteRun eRemoteRun, eRemote eRemote, dynamic value)
        {
            if (m_remote.p_bEnable == false) return "OK";
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
            public eWorks m_eWorks = eWorks.A;
            public override ModuleRunBase Clone()
            {
                Run_Remote run = new Run_Remote(m_module);
                run.m_eRemoteRun = m_eRemoteRun;
                run.m_lightPower = m_lightPower.Clone();
                run.m_sRecipe = m_sRecipe;
                run.m_eWorks = m_eWorks;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eRemoteRun = (eRemoteRun)tree.Set(m_eRemoteRun, m_eRemoteRun, "RemoteRun", "Select Remote Run", bVisible);
                m_eRemote = (eRemote)tree.Set(m_eRemote, m_eRemote, "Remote", "Remote", false);
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.RunLight: m_lightPower.RunTree(tree.GetTree("Light Power", true, bVisible), bVisible); break;
                    case eRemoteRun.SendRecipe: m_sRecipe = tree.Set(m_sRecipe, m_sRecipe, "Recipe", "Recipe", bVisible); break;
                    case eRemoteRun.SendSnapInfo: m_eWorks = (eWorks)tree.Set(m_eWorks, m_eWorks, "Works", "Works", bVisible); break;
                    default: break;
                }
            }

            public override string Run()
            {
                EQ.p_bStop = false;
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.StateHome: return m_module.StateHome();
                    case eRemoteRun.RunLight: m_module.RunLight(m_lightPower); break;
                    case eRemoteRun.SendRecipe: return m_module.SendRecipe(m_sRecipe); 
                    case eRemoteRun.SendSnapInfo: return m_module.SendSnapInfo(m_eWorks); 
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
                m_sRecipe = tree.Set(m_sRecipe, m_sRecipe, m_module.p_asRecipe, "Recipe", "Recipe", bVisible);
            }

            public override string Run()
            {
                m_module.m_RunningRecipe[m_eWorks].RecipeOpen(m_sRecipe);                  // 1. Root Vision Recipe Open

                // Root Vision -> VisionWorks2
                if (m_module.m_aWorks[m_eWorks].IsProcessRun())
                {
                    m_module.m_aWorks[m_eWorks].SendRecipe(m_sRecipe);                  // 2. VisionWorks2 Recipe Open 
                    int nSnapCount = m_module.m_RunningRecipe[m_eWorks].p_lSnap;               // 총 Snap 횟수
                    int nSnapMode = (int)m_module.m_RunningRecipe[m_eWorks].p_eSnapMode;       // Snap Mode (RGB, APS, ALL)
                    m_module.m_aWorks[m_eWorks].SendSnapInfo(m_sRecipe, nSnapMode, nSnapCount); // 3. VisionWorks2 Receive SnapInfo
                }
                return m_module.ReqSnap(m_sRecipe, m_eWorks);

                //m_module.m_recipe[m_eWorks].RecipeOpen(m_sRecipe);                  // 1. Root Vision Recipe Open

                //// Root Vision -> VisionWorks2
                //if (m_module.m_aWorks[m_eWorks].IsProcessRun())
                //{
                //    m_module.m_aWorks[m_eWorks].SendRecipe(m_sRecipe);                  // 2. VisionWorks2 Recipe Open 
                //    int nSnapCount = m_module.m_recipe[m_eWorks].p_lSnap;               // 총 Snap 횟수
                //    int nSnapMode = (int)m_module.m_recipe[m_eWorks].p_eSnapMode;       // Snap Mode (RGB, APS, ALL)
                //    m_module.m_aWorks[m_eWorks].SendSnapInfo(m_sRecipe, nSnapMode, nSnapCount); // 3. VisionWorks2 Receive SnapInfo
                //}
                //return m_module.ReqSnap(m_sRecipe, m_eWorks);
            }
        }
        #endregion

    }
}
