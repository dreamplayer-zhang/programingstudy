using RootTools;
using RootTools.Camera;
using RootTools.Camera.Matrox;
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
    public class Vision3D : ModuleBase, IVision
    {
        #region ToolBox
        Camera_Matrox m_camera;
        public LightSet m_lightSet;
        public override void GetTools(bool bInit)
        {
            if (p_eRemote == eRemote.Server)
            {
                p_sInfo = m_toolBox.GetCamera(ref m_camera, this, "Camera");
                p_sInfo = m_toolBox.Get(ref m_lightSet, this);
                m_aWorks[eWorks.A].GetTools(m_toolBox, bInit);
                m_aWorks[eWorks.B].GetTools(m_toolBox, bInit);
                p_sInfo = m_toolBox.GetComm(ref m_tcpRequest, this, "Request");
                if (bInit)
                {
                    m_tcpRequest.EventReciveData += M_tcpRequest_EventReceiveData;
                    m_camera.Connect();                  
                }
            }
            m_remote.GetTools(bInit);
        }
        #endregion

        #region Light
        public class LightPower
        {
            public List<double> m_aPower = new List<double>();

            public LightPower Clone()
            {
                LightPower power = new LightPower(m_vision);
                for (int n = 0; n < m_vision.p_lLight; n++) power.m_aPower[n] = m_aPower[n];
                return power;
            }

            public void RunTree(Tree tree, bool bVisible, bool bReadOnly = false)
            {
                for (int n = 0; n < m_aPower.Count; n++)
                {
                    m_aPower[n] = tree.Set(m_aPower[n], m_aPower[n], n.ToString("00"), "Light Power (0 ~ 100)", bVisible, bReadOnly);
                }
            }

            Vision3D m_vision;
            public LightPower(Vision3D vision)
            {
                m_vision = vision;
                while (m_aPower.Count < m_vision.p_lLight) m_aPower.Add(0);
            }

            public static bool IsSameLight(LightPower lightPower1, LightPower lightPower2)
            {
                if (lightPower1.m_aPower.Count != lightPower2.m_aPower.Count) return false;
                for (int i = 0; i < lightPower1.m_aPower.Count; i++)
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

        LightPower prevLightPower = null;
        public void RunLight(LightPower lightPower)
        {
            if (p_eRemote == eRemote.Client) RemoteRun(eRemoteRun.RunLight, eRemote.Client, lightPower);
            else
            {
                if (prevLightPower != null && LightPower.IsSameLight(prevLightPower, lightPower))
                    return;

                prevLightPower = lightPower.Clone();
                for (int n = 0; n < p_lLight; n++)
                {
                    Light light = m_lightSet.m_aLight[n];
                    if (light.m_light != null) light.m_light.p_fSetPower = lightPower.m_aPower[n];
                }
            }
        }

        public void RunLightOff()
        {
            if (p_eRemote == eRemote.Client) RemoteRun(eRemoteRun.RunLightOff, eRemote.Client, 0);
            else
            {
                for (int n = 0; n < p_lLight; n++)
                {
                    Light light = m_lightSet.m_aLight[n];
                    if (light.m_light != null) light.m_light.p_fSetPower = 0;
                    prevLightPower.m_aPower[n] = 0;
                }
            }
        }
        #endregion

        #region Send Info
        public string SendSnapInfo(SnapInfo snapInfo)
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.SendSnapInfo, eRemote.Client, snapInfo);
            else
            {
                return m_aWorks[snapInfo.m_eWorks].SendSnapInfo(snapInfo); // 3. VisionWorks2 Receive SnapInfo
            }
        }

        public LotInfo m_lotInfo = null;
        public string SendLotInfo(LotInfo lotInfo)
        {
            m_lotInfo = lotInfo;
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.SendLotInfo, eRemote.Client, lotInfo);
            else
            {
                string sRunA = m_aWorks[eWorks.A].SendLotInfo(lotInfo);
                string sRunB = m_aWorks[eWorks.B].SendLotInfo(lotInfo);
                if ((sRunA == "OK") && (sRunB == "OK")) return "OK";
                return sRunA + ", " + sRunB;
            }
        }

        public string SendSortInfo(SortInfo sortInfo)
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.SendSortInfo, eRemote.Client, sortInfo);
            else
            {
                return m_aWorks[sortInfo.m_eWorks].SendSortInfo(sortInfo);
            }
        }
        #endregion

        #region Camera Data
        public double m_dResolution = 2;
        public int m_nLine = 150000;
        public bool m_bUseBiDirectional = false;
        //public DalsaParameterSet.eUserSet m_eCamUserSet = DalsaParameterSet.eUserSet.Default;
        public Dictionary<eCalMode, CalibrationData> m_aCalData = new Dictionary<eCalMode, CalibrationData>();

        public enum eCalMode
        {
            RGB,
            APS,
        }
        public void InitCalData()
        {
            m_aCalData.Add(eCalMode.RGB, new CalibrationData());
            m_aCalData.Add(eCalMode.APS, new CalibrationData());
        }


        public class CalibrationData
        {
            //public DalsaParameterSet.eFlatFieldUserSet m_eForwardUserSet = DalsaParameterSet.eFlatFieldUserSet.Factory;
            //public DalsaParameterSet.eFlatFieldUserSet m_eBackwardUserSet = DalsaParameterSet.eFlatFieldUserSet.Factory;
            //public DalsaParameterSet.eAnalogGain m_eAnalogGain = DalsaParameterSet.eAnalogGain.One;
            public string _sAnalogGain = "1";
            public string m_sAnalogGain
            {
                get { return _sAnalogGain; }
                set
                {
                    _sAnalogGain = value;
                    //for (int i = 0; i < DalsaParameterSet.m_aAnalogGain.Length; i++)
                    //{
                        //if (value == DalsaParameterSet.m_aAnalogGain[i])
                        //{
                            //m_eAnalogGain = (DalsaParameterSet.eAnalogGain)i;
                            //return;
                        //}
                    //}
                }
            }

            public double _dSystemGain = 1;
            public double _dAllRowsGain = 1;
            public double _dRedGain = 1;
            public double _dGreenGain = 1;
            public double _dBlueGain = 1;
            public double m_dSystemGain
            {
                get { return _dSystemGain; }
                set { if (CheckGainRange(value)) _dSystemGain = value; }
            }
            public double m_dAllRowsGain
            {
                get { return _dAllRowsGain; }
                set { if (CheckGainRange(value)) _dAllRowsGain = value; }
            }
            public double m_dRedGain
            {
                get { return _dRedGain; }
                set { if (CheckGainRange(value)) _dRedGain = value; }
            }
            public double m_dGreenGain
            {
                get { return _dGreenGain; }
                set { if (CheckGainRange(value)) _dGreenGain = value; }
            }
            public double m_dBlueGain
            {
                get { return _dBlueGain; }
                set { if (CheckGainRange(value)) _dBlueGain = value; }
            }

            public bool CheckGainRange(double dGain)
            {
                if (dGain >= 1 && dGain <= 9.99902)
                    return true;
                else
                    return false;
            }

            public void RunTree(Tree tree)
            {
                //m_eForwardUserSet = (DalsaParameterSet.eFlatFieldUserSet)tree.Set(m_eForwardUserSet, m_eForwardUserSet, "Forward UserSet", "Select Flat Field Correction UserSet");
                //m_eBackwardUserSet = (DalsaParameterSet.eFlatFieldUserSet)tree.Set(m_eBackwardUserSet, m_eBackwardUserSet, "Reverse UserSet", "Select Flat Field Correction UserSet");
                //m_sAnalogGain = tree.Set(m_sAnalogGain, m_sAnalogGain, DalsaParameterSet.m_aAnalogGain, "Analog Gain", "Analog Gain");
                m_dSystemGain = tree.Set(m_dSystemGain, m_dSystemGain, "System Gain", "System Gain");
                m_dAllRowsGain = tree.Set(m_dAllRowsGain, m_dAllRowsGain, "AllRows Gain", "AllRows Gain");
                m_dRedGain = tree.Set(m_dRedGain, m_dRedGain, "Red Gain", "Red Gain");
                m_dGreenGain = tree.Set(m_dGreenGain, m_dGreenGain, "Green Gain", "Green Gain");
                m_dBlueGain = tree.Set(m_dBlueGain, m_dBlueGain, "Blue Gain", "Blue Gain");
            }
        }
        #endregion

        #region GrabData
        public class Grab
        {
            public int m_nFovStart = 0;
            public int m_nFovSize = 1920;
            public int m_nReverseOffset = 0;
            public int m_nOverlap = 0;
            public int m_nYOffset = 0;
            public double[] m_dScale = new double[3] { 1, 1, 1 };
            public double[] m_dShift = new double[3] { 0, 0, 0 };
            public int[] m_yShift = new int[3] { 0, 0, 0 };

            public void SetData(GrabData data)
            {
                data.m_nFovStart = m_nFovStart;
                data.m_nFovSize = m_nFovSize;
                //data.m_dScaleR = m_dScale[0];
                //data.m_dScaleG = m_dScale[1];
                //data.m_dScaleB = m_dScale[2];
                //data.m_dShiftR = m_dShift[0];
                //data.m_dShiftG = m_dShift[1];
                //data.m_dShiftB = m_dShift[2];
                //data.m_nYShiftR = m_yShift[0];
                //data.m_nYShiftG = m_yShift[1];
                //data.m_nYShiftB = m_yShift[2];
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
                MATROX
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
                    RAWIMAGE
                }
                public eEXT m_eEXT = eEXT.RAWIMAGE;
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
                    //data.ReverseOffsetY = cpOffset.Y; /*m_cpMemory.Y;*/ /* + m_vision.m_nLine */
                    //data.m_bUseFlipVertical = true;
                    m_vision.m_aGrabData[eWorks].SetData(data);
                    return data;
                }

                public void RunTree(Tree tree, bool bVisible, bool bReadOnly = false)
                {
                    RunTreeStage(tree.GetTree("Stage", true, bVisible), bVisible, bReadOnly);
                    RunTreeMemory(tree.GetTree("Memory", true, bVisible), bVisible, bReadOnly);
                    m_lightPower.RunTree(tree.GetTree("Light", true, bVisible), bVisible, bReadOnly);
                }

                void RunTreeStage(Tree tree, bool bVisible, bool bReadOnly = false)
                {
                    m_dpAxis = tree.Set(m_dpAxis, m_dpAxis, "Offset", "Axis Offset (mm)", bVisible, bReadOnly);
                    m_eDirection = (eDirection)tree.Set(m_eDirection, m_eDirection, "Direction", "Scan Direction", bVisible, false);
                }

                void RunTreeMemory(Tree tree, bool bVisible, bool bReadOnly = false)
                {
                    m_eEXT = (eEXT)tree.Set(m_eEXT, m_eEXT, "EXT", "Select EXT", bVisible, bReadOnly);
                    m_cpMemory = tree.Set(m_cpMemory, m_cpMemory, "Offset", "Memory Offset Address (pixel)", bVisible, bReadOnly);
                    m_nOverlap = tree.Set(m_nOverlap, m_nOverlap, "Overlap", "Memory Overlap Size (pixel)", bVisible, bReadOnly);
                }

                Vision3D m_vision;
                public Snap(Vision3D vision)
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
                    double dResolution = m_vision.m_dResolution;
                    double dFOVmm = nFOVpx * dResolution / 1000;
                    int nSnapCount = (int)Math.Ceiling(_dProductWidth / dFOVmm);    // 제품 전체를 찍기위한 스냅 횟수
                    p_lSnap = nSnapCount;

                    double dSnapStartXPos = (dFOVmm / 2) * (nSnapCount - 1); // Stage Center에서부터 첫 Snap 위치까지 거리
                    double dStageXOffset;
                    for (int i = 0; i < p_lSnap; i++)
                    {
                        m_aSnap.Add(new Snap(m_vision));
                        dStageXOffset = dSnapStartXPos - dFOVmm * i;
                        m_aSnap[i].m_dpAxis = new RPoint(dStageXOffset, 0);
                        m_aSnap[i].m_nOverlap = nOverlap;
                        m_aSnap[i].m_eDirection = Snap.eDirection.Forward;

                        m_aSnap[i].m_eEXT = Snap.eEXT.RAWIMAGE;
                        m_aSnap[i].m_lightPower = m_lightPower3D.Clone();
                    }
                }
            }

            public eWorks m_eWorks = eWorks.A;

            public Recipe Clone()
            {
                Recipe recipe = new Recipe(m_vision, m_eWorks);
                recipe.m_eWorks = m_eWorks;
                recipe.m_sRecipe = m_sRecipe;
                recipe.m_lightPower3D = m_lightPower3D.Clone();
                foreach (Snap snap in m_aSnap) recipe.m_aSnap.Add(snap.Clone());
                return recipe;
            }

            const string c_sExt = ".pine2";
            public void RecipeSave(string sRecipe)
            {
                string sPath = EQ.c_sPathRecipe + "\\" + sRecipe;
                Directory.CreateDirectory(sPath);
                string sFile = sPath + "\\" + m_vision.p_eVision.ToString() + m_eWorks.ToString() + c_sExt;
                m_treeRecipe.m_job = new Job(sFile, true, m_vision.m_log);
                RunTreeRecipe(Tree.eMode.JobSave);
                m_treeRecipe.m_job.Close();
            }

            public void RecipeOpen(string sRecipe)
            {
                m_sRecipe = sRecipe;
                string sPath = EQ.c_sPathRecipe + "\\" + sRecipe;
                Directory.CreateDirectory(sPath);
                string sFile = sPath + "\\" + m_vision.p_eVision.ToString() + m_eWorks.ToString() + c_sExt;
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
                LightPower lightPower3D = m_lightPower3D.Clone();

                RunTreeRecipe(Tree.eMode.Update);
                if (dProductWidth != p_dProductWidth || eSnapMode != p_eSnapMode || !LightPower.IsSameLight(lightPower3D, m_lightPower3D))
                    RunTreeRecipe(Tree.eMode.Init);
            }


            public void RunTreeRecipe(Tree.eMode eMode)
            {
                m_treeRecipe.p_eMode = eMode;
                RunTreeRecipe(m_treeRecipe, true, true);
            }

            public LightPower m_lightPower3D;
            public void RunTreeRecipe(Tree tree, bool bVisible, bool bReadOnly = false)
            {
                tree.Set(m_eWorks, m_eWorks, "Works", "Vision eWorks", bVisible, bReadOnly);
                p_eSnapMode = (eSnapMode)tree.Set(p_eSnapMode, p_eSnapMode, "Snap Mode", "Select Snap Mode", bVisible);
                p_dProductWidth = tree.Set(p_dProductWidth, p_dProductWidth, "Product Width", "Product Width(mm)", bVisible);
                p_lSnap = tree.Set(p_lSnap, p_lSnap, "Count", "Snap Count", bVisible, true);

                if (!(m_treeRecipe.p_eMode == Tree.eMode.JobOpen && m_vision.p_eRemote == eRemote.Client))
                {
                   if (p_eSnapMode == eSnapMode.MATROX || p_eSnapMode == eSnapMode.MATROX)
                        m_lightPower3D.RunTree(tree.GetTree("Light").GetTree("3D Light", true, bVisible), bVisible);
                }

                for (int n = 0; n < m_aSnap.Count; n++)
                    m_aSnap[n].RunTree(tree.GetTree("Snap").GetTree("Snap" + n.ToString("00"), false, bVisible), bVisible, true);

            }
            #endregion

            Vision3D m_vision;
            public Recipe(Vision3D vision, eWorks eWorks)
            {
                m_vision = vision;
                m_eWorks = eWorks;
                m_lightPower3D = new LightPower(vision);
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
                if (info.Exists == true)
                {
                    foreach (DirectoryInfo dir in info.GetDirectories()) asRecipe.Add(dir.Name);
                }
                else
                {
                    MessageBox.Show("Recipe Path Error " + EQ.c_sPathRecipe);
                }
                return asRecipe;
            }
            set { }
        }
        #endregion

        #region Works
        public Dictionary<eWorks, Works3D> m_aWorks = new Dictionary<eWorks, Works3D>();
        void InitVisionWorks()
        {
            m_aWorks.Add(eWorks.A, new Works3D(eWorks.A, this));
            m_aWorks.Add(eWorks.B, new Works3D(eWorks.B, this));
        }
        #endregion

        #region RunSnap
        bool m_bCanChangeUserSet = true;
        bool m_bDoneChangeUserSet = true;
        int nWaitTime = 3 * 1000;
        int nWaitInterval = 10;
        int nTimeCount = 0;
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
            Recipe.eSnapMode nSnapMode = m_RunningRecipe[eWorks].p_eSnapMode;
            int nTotalSnap = m_RunningRecipe[eWorks].p_lSnap;
           if (nTotalSnap < 2) 
                 nTotalSnap = 2;
            int nSnapLineIndex = (nSnapMode == Recipe.eSnapMode.MATROX) ? iSnap % (nTotalSnap / 2) : iSnap;

            // 이미지 시작점 설정
            CPoint cpOffset = CalcOffset(nSnapLineIndex, nFOVpx, nReverseOffset, recipe);
            MemoryData memory = m_aWorks[eWorks].p_memSnap[(int)recipe.m_eEXT];

            MemoryData memConv = m_aWorks[eWorks].m_memoryGroup.GetMemory(Works3D.Mem3DViewH);
            MemoryData memB = m_aWorks[eWorks].m_memoryGroup.GetMemory(Works3D.Mem3DBright);
            MemoryData memH = m_aWorks[eWorks].m_memoryGroup.GetMemory(Works3D.Mem3DHeight);
            MemoryData memR = m_aWorks[eWorks].m_memoryGroup.GetMemory(Works3D.Mem3DRaw);

            m_camera.Init3D(memConv, memH, memB, memR, nOverlap, m_nLine);

            GrabData grabData = recipe.GetGrabData(eWorks, cpOffset, nOverlap);
            grabData.nScanOffsetY = (nSnapLineIndex) * nYOffset;
            //grabData.nUserSet = (int)m_eCamUserSet;

            try
            {
                m_log.Info("Start");

                // Set Camera GrabThread On
                m_camera.m_bGrabThreadOn = false;
                m_camera.Grab3DScan(memory, cpOffset, m_nLine, grabData);
                while (m_camera.m_bGrabThreadOn != true)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                }
                m_log.Info("Grab Thread On Done");

                // Send SnapReady to Handler (Handler move Axis After receive this msg)
                ReqSnapReady(eWorks);
                m_log.Info("Send Snap Ready Done");

                // Wait for Grab
                while (m_camera.p_CamInfo.p_eState != eCamState.Ready)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                }
                m_log.Info("Grab Done");

                // Send Snap Done to VisionWorks2
                if (m_aWorks[eWorks].IsProcessRun())
                    m_aWorks[eWorks].SendSnapDone(iSnap);
                m_log.Info("Send Snap Done Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                m_camera.StopGrab();
            }
            return "OK";
        }

        private void SetFirstCalUserSet(Recipe.eSnapMode nSnapMode, Recipe.Snap recipe)
        {
            /*if (nSnapMode == Recipe.eSnapMode.ALL || nSnapMode == Recipe.eSnapMode.RGB)
            {
                if (recipe.m_eDirection == Recipe.Snap.eDirection.Forward)
                    m_camera.p_CamParam.p_eFlatFieldCorrection = m_aCalData[eCalMode.RGB].m_eForwardUserSet;
                else
                    m_camera.p_CamParam.p_eFlatFieldCorrection = m_aCalData[eCalMode.RGB].m_eBackwardUserSet;
            }
            else if (nSnapMode == Recipe.eSnapMode.APS)
            {
                if (recipe.m_eDirection == Recipe.Snap.eDirection.Forward)
                    m_camera.p_CamParam.p_eFlatFieldCorrection = m_aCalData[eCalMode.APS].m_eForwardUserSet;
                else
                    m_camera.p_CamParam.p_eFlatFieldCorrection = m_aCalData[eCalMode.APS].m_eBackwardUserSet;
            }*/
        }

        private CPoint CalcOffset(int nSnapLineIndex, int nFOVpx, int nReverseOffset, Recipe.Snap recipe)
        {
            CPoint cpOffset;
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

            return cpOffset;
        }

        private void UpdateCalUserset(object param)
        {
            //m_camera.p_CamParam.p_eFlatFieldCorrection = (DalsaParameterSet.eFlatFieldUserSet)param;
            m_bDoneChangeUserSet = true;
        }

        private void SetCameraGain(int iSnap, Recipe.eSnapMode nSnapMode, Recipe.Snap.eDirection eDir)
        {
            if (nSnapMode == Recipe.eSnapMode.MATROX)
            {
                if (iSnap == 0)
                    SetGain(eCalMode.RGB);
                else
                    SetGain(eCalMode.APS);
            }
            else
                SetGain((eCalMode)nSnapMode);
        }

        private void SetGain(eCalMode eMode)
        {
            CalibrationData data = m_aCalData[eMode];
            /*m_camera.p_CamParam.SetAnalogGain(data.m_eAnalogGain);
            m_camera.p_CamParam.ChangeGainSelector(DalsaParameterSet.eGainSelector.System);
            m_camera.p_CamParam.SetGain(data.m_dSystemGain);
            m_camera.p_CamParam.ChangeGainSelector(DalsaParameterSet.eGainSelector.Blue);
            m_camera.p_CamParam.SetGain(data.m_dBlueGain);
            m_camera.p_CamParam.ChangeGainSelector(DalsaParameterSet.eGainSelector.Green);
            m_camera.p_CamParam.SetGain(data.m_dGreenGain);
            m_camera.p_CamParam.ChangeGainSelector(DalsaParameterSet.eGainSelector.Red);
            m_camera.p_CamParam.SetGain(data.m_dRedGain);*/
        }

        #endregion

        #region Request  
        int m_nReq = 0; //forget
        string m_sReceive = "";
        TCPIPClient m_tcpRequest;
        private void M_tcpRequest_EventReceiveData(byte[] aBuf, int nSize, Socket socket)
        {
            m_sReceive = Encoding.Default.GetString(aBuf, 0, nSize);
            if (m_sReceive.Length <= 0) return;
            ReadReceive(m_sReceive);
        }

        void ReadReceive(string sReceive)
        {
            string[] asRead = sReceive.Split(',');
            if (asRead.Length < 2) return;
            if (asRead[1] == eProtocol.ChangeUserset.ToString())
            {
                m_bCanChangeUserSet = true;
            }
        }

        public string ReqSnap(string sRecipe, eWorks eWorks)
        {
            string sSend = m_nReq.ToString("000") + "," + eProtocol.Snap.ToString() + "," + sRecipe + "," + eWorks.ToString() + "," + m_bUseBiDirectional.ToString();
            m_sReceive = "";
            m_tcpRequest.Send(sSend);
            while (sSend != m_sReceive)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
            }
            return "OK";
        }

        public string ReqSnapReady(eWorks eWorks)
        {
            string sSend = m_nReq.ToString("000") + "," + eProtocol.SnapReady.ToString() + "," + eWorks.ToString();
            m_sReceive = "";
            m_tcpRequest.Send(sSend);
            return "OK";
        }

        public string ReqInspDone(string sStripID, string sStripResult, string sX, string sY, string sMapResult, eWorks eWorks)
        {
            string sSend = m_nReq.ToString("000") + "," + eProtocol.InspDone.ToString() + ",";
            sSend += sStripID + "," + sStripResult + "," + sX + "," + sY + "," + sMapResult + "," + eWorks.ToString();
            m_sReceive = "";
            m_tcpRequest.Send(sSend);
            return "OK";
        }

        public string ReqWorksConnect(eWorks eWorks, bool bConnect)
        {
            string sSend = m_nReq.ToString("000") + "," + eProtocol.WorksConnect.ToString() + "," + eWorks.ToString() + "," + (bConnect ? "1" : "0");
            m_sReceive = "";
            m_tcpRequest.Send(sSend);
            return "OK";
        }
        #endregion

        #region Thread Check Works Connect
        bool m_bThreadCheck = false;
        Thread m_threadCheck;
        void InitThreadCheck()
        {
            m_threadCheck = new Thread(new ThreadStart(RunThreadCheckConnect));
            m_threadCheck.Start();
        }

        bool[] m_bWorksConnect = new bool[2] { false, false };
        void RunThreadCheckConnect()
        {
            m_bThreadCheck = true;
            Thread.Sleep(3000);
            while (m_bThreadCheck)
            {
                Thread.Sleep(200);
                if (m_tcpRequest.p_bConnect == false)
                {
                    m_bWorksConnect[0] = false;
                    m_bWorksConnect[1] = false;
                }
                else
                {
                    if (m_bWorksConnect[0] != m_aWorks[eWorks.A].m_tcpip.p_bConnect)
                    {
                        ReqWorksConnect(eWorks.A, m_aWorks[eWorks.A].m_tcpip.p_bConnect);
                        m_bWorksConnect[0] = m_aWorks[eWorks.A].m_tcpip.p_bConnect;
                    }
                    if (m_bWorksConnect[1] != m_aWorks[eWorks.B].m_tcpip.p_bConnect)
                    {
                        ReqWorksConnect(eWorks.B, m_aWorks[eWorks.B].m_tcpip.p_bConnect);
                        m_bWorksConnect[1] = m_aWorks[eWorks.B].m_tcpip.p_bConnect;
                    }
                }
            }
        }
        #endregion

        #region override
        public override void Reset()
        {
            if (p_eRemote == eRemote.Client)
            {
                foreach (Remote.Protocol protocol in m_remote.m_aProtocol) protocol.m_bDone = true;
                m_aWorks[eWorks.A].Reset();
                m_aWorks[eWorks.B].Reset();
                RemoteRun(eRemoteRun.Reset, eRemote.Client, 0);
            }
            else
            {
                base.Reset();
                m_aWorks[eWorks.A].SendReset();
                m_aWorks[eWorks.B].SendReset();
            }
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
                p_lLight = tree.GetTree("Light", false).Set(p_lLight, p_lLight, "Channel", "Light Channel Count");
                p_eVision = (eVision)tree.GetTree("Vision").Set(p_eVision, p_eVision, "Type", "Vision Type");
                RunCameraTree(tree.GetTree("Camera", true));
                RunProcessTree(tree.GetTree("Process", false));
                RunGrabDataTree(tree.GetTree("GrabData", false));
            }
        }

        void RunCameraTree(Tree tree)
        {
            m_dResolution = tree.Set(m_dResolution, m_dResolution, "Pixel Resolution", "Pixel Resolution (um/pixel)");
            m_nLine = tree.Set(m_nLine, m_nLine, "Line", "Memory Snap Lines (pixel)");
            //m_bUseBiDirectional = tree.Set(m_bUseBiDirectional, m_bUseBiDirectional, "BiDirectional Scan", "Use BiDirectional Scan");
            //m_eCamUserSet = (DalsaParameterSet.eUserSet)tree.Set(m_eCamUserSet, m_eCamUserSet, "UserSet", "Select Camera UserSet");
            RunCalibrationTree(tree.GetTree("Calibration"));
        }

        void RunCalibrationTree(Tree tree)
        {
            //m_aCalData[eCalMode.RGB].RunTree(tree.GetTree("RGB Setting"));
            //m_aCalData[eCalMode.APS].RunTree(tree.GetTree("APS Setting"));
        }

        void RunProcessTree(Tree tree)
        {
            m_aWorks[eWorks.A].RunTree(tree.GetTree("Works " + m_aWorks[eWorks.A].p_id));
            m_aWorks[eWorks.B].RunTree(tree.GetTree("Works " + m_aWorks[eWorks.B].p_id));
        }

        void RunGrabDataTree(Tree tree)
        {
            //m_aGrabData[eWorks.A].RunTree(tree.GetTree("GrabData A"));
            //m_aGrabData[eWorks.B].RunTree(tree.GetTree("GrabData B"));
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

        #region IVision
        public eVision p_eVision { get; set; }
        public TreeRoot p_treeRootQueue { get { return m_treeRootQueue; } }
        public Remote p_remote { get { return m_remote; } }
        #endregion

        public Vision3D(eVision eVision, IEngineer engineer, eRemote eRemote)
        {
            p_eVision = eVision;
            InitVisionWorks();
            //InitCalData();
            InitBase("Vision " + eVision.ToString(), engineer, eRemote);
        }

        public Vision3D(string id, IEngineer engineer, eRemote eRemote)
        {
            InitGrabData();
            InitVisionWorks();
            //InitCalData();
            InitRecipe();
            InitBase(id, engineer, eRemote);
            InitVision_Snap_UI();
            InitThreadCheck();
            p_eState = eState.Ready;
        }

        public override void ThreadStop()
        {
            if (m_bThreadCheck)
            {
                m_bThreadCheck = false;
                m_threadCheck.Join();
            }
            foreach (Works3D works in m_aWorks.Values) works.ThreadStop();
            base.ThreadStop();
        }

        #region RemoteRun
        public enum eRemoteRun
        {
            StateHome,
            Reset,
            RunLight,
            RunLightOff,
            SendSnapInfo,
            SendLotInfo,
            SendSortInfo,
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
                case eRemoteRun.SendSnapInfo: run.m_snapInfo = value; break;
                case eRemoteRun.SendLotInfo: run.m_lotInfo = value; break;
                case eRemoteRun.SendSortInfo: run.m_sortInfo = value; break;
            }
            return run;
        }

        string RemoteRun(eRemoteRun eRemoteRun, eRemote eRemote, dynamic value)
        {
            if (m_remote.p_bEnable == false) return "OK";
            if (m_remote.m_client.p_bConnect == false) return "Remote TCPIP not Connected";
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
            Vision3D m_module;
            public Run_Remote(Vision3D module)
            {
                m_module = module;
                m_lightPower = new LightPower(m_module);
                InitModuleRun(module);
            }

            public eRemoteRun m_eRemoteRun = eRemoteRun.StateHome;
            public LightPower m_lightPower;
            public eWorks m_eWorks = eWorks.A;
            public SnapInfo m_snapInfo = new SnapInfo(eWorks.A, 0, "", 0, true);
            public LotInfo m_lotInfo = new LotInfo(0, "", "", false, false, 0, 0);
            public SortInfo m_sortInfo = new SortInfo(eWorks.A, "", "");
            public override ModuleRunBase Clone()
            {
                Run_Remote run = new Run_Remote(m_module);
                run.m_eRemoteRun = m_eRemoteRun;
                run.m_lightPower = m_lightPower.Clone();
                run.m_eWorks = m_eWorks;
                run.m_lotInfo = m_lotInfo.Clone();
                run.m_sortInfo = m_sortInfo.Clone();
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eRemoteRun = (eRemoteRun)tree.Set(m_eRemoteRun, m_eRemoteRun, "RemoteRun", "Select Remote Run", bVisible);
                m_eRemote = (eRemote)tree.Set(m_eRemote, m_eRemote, "Remote", "Remote", false);
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.RunLight: m_lightPower.RunTree(tree.GetTree("Light Power", true, bVisible), bVisible); break;
                    case eRemoteRun.SendSnapInfo: m_snapInfo.RunTree(tree.GetTree("SnapInfo"), bVisible); break;
                    case eRemoteRun.SendLotInfo: m_lotInfo.RunTree(tree.GetTree("LotInfo"), bVisible); break;
                    case eRemoteRun.SendSortInfo: m_sortInfo.RunTree(tree.GetTree("SortInfo"), bVisible); break;
                    default: break;
                }
            }

            public override string Run()
            {
                EQ.p_bStop = false;
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.StateHome: return m_module.StateHome();
                    case eRemoteRun.Reset: m_module.Reset(); return "OK";
                    case eRemoteRun.RunLight: m_module.RunLight(m_lightPower); break;
                    case eRemoteRun.RunLightOff: m_module.RunLightOff(); break;
                    case eRemoteRun.SendSnapInfo: return m_module.SendSnapInfo(m_snapInfo);
                    case eRemoteRun.SendLotInfo: return m_module.SendLotInfo(m_lotInfo);
                    case eRemoteRun.SendSortInfo: return m_module.SendSortInfo(m_sortInfo);
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
            Vision3D m_module;
            public Run_Delay(Vision3D module)
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
            Vision3D m_module;
            public Run_Snap(Vision3D module)
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
            Vision3D m_module;
            public Run_ReqSnap(Vision3D module)
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
