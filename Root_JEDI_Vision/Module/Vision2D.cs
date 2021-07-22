using Root_JEDI_Sorter.Module;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.Dalsa;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Root_JEDI_Vision.Module
{
    public class Vision2D : ModuleBase, IVision
    {
        #region ToolBox
        Camera_Dalsa m_camera;
        public LightSet m_lightSet;
        RS232 m_rs232RGBW;
        MemoryPool m_memoryPool;
        public override void GetTools(bool bInit)
        {
            if (p_eRemote == eRemote.Server)
            {
                p_sInfo = m_toolBox.GetCamera(ref m_camera, this, "Camera");
                p_sInfo = m_toolBox.Get(ref m_lightSet, this);
                p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory", 1);
                p_sInfo = m_toolBox.GetComm(ref m_rs232RGBW, this, "RGBW");
                m_boat.GetTools(m_toolBox, this, bInit); 
                m_camAxis.GetTools(m_toolBox, this, bInit); 
                m_process.GetTools(m_toolBox, bInit);
                if (bInit)
                {
                    m_rs232RGBW.p_bConnect = true;
                    m_camera?.Connect();
                    InitMemory();
                }
            }
            m_remote.GetTools(bInit);
        }
        #endregion

        #region Camera Axis
        public enum eLine
        {
            Single, 
            Double,
            Triple,
        }
        public class CameraAxis
        {
            public AxisXY m_axis;
            public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
            {
                toolBox.GetAxis(ref m_axis, module, "Camera");
                if (bInit) InitPosition(); 
            }

            #region InitPosition
            public enum ePos
            {
                Ready,
                Snap
            }
            void InitPosition()
            {
                m_axis.AddPos(Enum.GetNames(typeof(ePos)));
            }
            #endregion

            #region Offset
            double m_pulseum = 10; 
            double m_mmSpace = 100; 
            public Dictionary<eLine, RPoint[]> m_umOffset = new Dictionary<eLine, RPoint[]>();
            void InitOffset()
            {
                m_umOffset.Add(eLine.Single, new RPoint[1] { new RPoint() });
                m_umOffset.Add(eLine.Double, new RPoint[2] { new RPoint(), new RPoint() });
                m_umOffset.Add(eLine.Triple, new RPoint[3] { new RPoint(), new RPoint(), new RPoint() });
            }
            public void RunTree(Tree tree)
            {
                m_pulseum = tree.Set(m_pulseum, m_pulseum, "pulse/um", "pulse per um");
                m_mmSpace = tree.Set(m_mmSpace, m_mmSpace, "Space", "Grab Sapce (mm)");
                RPoint[] umOffset = m_umOffset[eLine.Double];
                for (int n = 0; n < 2; n++) umOffset[n] = tree.GetTree("Double").Set(umOffset[n], umOffset[n], n.ToString(), "Camera Axis Offset (um)");
                umOffset = m_umOffset[eLine.Triple];
                for (int n = 0; n < 3; n++) umOffset[n] = tree.GetTree("Triple").Set(umOffset[n], umOffset[n], n.ToString(), "Camera Axis Offset (um)");
            }
            #endregion

            #region RunMove
            public string RunMove(ePos ePos, bool bWait = true)
            {
                m_axis.StartMove(ePos);
                return bWait ? m_axis.WaitReady() : "OK";
            }

            public string RunMove(eLine eLine, int iLine, bool bWait = true)
            {
                RPoint umOffset = new RPoint(m_umOffset[eLine][iLine]); 
                switch (eLine)
                {
                    case eLine.Single: break; 
                    case eLine.Double: umOffset.X += 1000 * m_mmSpace * (iLine - 0.5); break;
                    case eLine.Triple: umOffset.X += 1000 * m_mmSpace * (iLine - 1); break; 
                }
                m_axis.StartMove(ePos.Snap, new RPoint(m_pulseum * umOffset.X, m_pulseum * umOffset.Y));
                return bWait ? m_axis.WaitReady() : "OK";
            }
            #endregion

            public CameraAxis()
            {
                InitOffset(); 
            }
        }
        public CameraAxis m_camAxis = new CameraAxis(); 
        #endregion

        #region Memory
        public MemoryGroup m_memoryGroup;
        MemoryData[] m_memoryExt = new MemoryData[2] { null, null };
        MemoryData[] m_memoryColor = new MemoryData[3] { null, null, null };
        MemoryData[] m_memoryRGB = new MemoryData[3] { null, null, null };
        MemoryData[] m_memoryAPS = new MemoryData[3] { null, null, null };
        MemoryData[] m_memoryHSI = new MemoryData[3] { null, null, null };
        //MemoryData m_memoryGerbber;
        List<MemoryData> m_aMemory = new List<MemoryData>();
        void InitMemory()
        {
            m_memoryGroup = m_memoryPool.GetGroup("Pine2");
            m_aMemory.Add(m_memoryExt[0] = m_memoryGroup.CreateMemory("EXT1", 3, 1, new CPoint(50000, 90000))); // Red Green Blue      -> VisionWorks2 Gerbber/RGBtoG/CtoG
            m_aMemory.Add(m_memoryExt[1] = m_memoryGroup.CreateMemory("EXT2", 3, 1, new CPoint(50000, 90000))); // Axial Pad Side      -> VisionWorks2 Ext1/Ext2/SideTemp

            m_aMemory.Add(m_memoryColor[0] = m_memoryGroup.CreateMemory("Color1", 1, 3, new CPoint(50000, 90000))); // RGB 합성 이미지 버퍼
            m_aMemory.Add(m_memoryColor[1] = m_memoryGroup.CreateMemory("Color2", 1, 3, new CPoint(50000, 90000))); // APS 합성 이미지 버퍼
            m_aMemory.Add(m_memoryColor[2] = m_memoryGroup.CreateMemory("Color3", 1, 3, new CPoint(50000, 90000))); // HSI 합성 이미지 버퍼
            m_aMemory.Add(m_memoryRGB[0] = m_memoryGroup.CreateMemory("Red", 1, 1, new CPoint(50000, 90000)));
            m_aMemory.Add(m_memoryRGB[1] = m_memoryGroup.CreateMemory("Green", 1, 1, new CPoint(50000, 90000)));
            m_aMemory.Add(m_memoryRGB[2] = m_memoryGroup.CreateMemory("Blue", 1, 1, new CPoint(50000, 90000)));
            m_aMemory.Add(m_memoryAPS[0] = m_memoryGroup.CreateMemory("Axial", 1, 1, new CPoint(50000, 90000)));
            m_aMemory.Add(m_memoryAPS[1] = m_memoryGroup.CreateMemory("Pad", 1, 1, new CPoint(50000, 90000)));
            m_aMemory.Add(m_memoryAPS[2] = m_memoryGroup.CreateMemory("Side", 1, 1, new CPoint(50000, 90000)));
            m_aMemory.Add(m_memoryHSI[0] = m_memoryGroup.CreateMemory("Hue", 1, 1, new CPoint(50000, 90000)));
            m_aMemory.Add(m_memoryHSI[1] = m_memoryGroup.CreateMemory("Saturation", 1, 1, new CPoint(50000, 90000)));
            m_aMemory.Add(m_memoryHSI[2] = m_memoryGroup.CreateMemory("Intensity", 1, 1, new CPoint(50000, 90000)));

            string regGroup = "MMF Data " + p_id;   // MMF Data A, MMF Data B
            Registry reg = new Registry(false, regGroup, "MemoryOffset");
            foreach (MemoryData mem in m_aMemory) reg.Write(mem.p_id, mem.p_mbOffset);
            reg = new Registry(false, regGroup, "MemoryDepth");
            foreach (MemoryData mem in m_aMemory) reg.Write(mem.p_id, mem.p_nByte);
            reg = new Registry(false, regGroup, "MemorySizeX");
            foreach (MemoryData mem in m_aMemory) reg.Write(mem.p_id, mem.p_sz.X);
            reg = new Registry(false, regGroup, "MemorySizeY");
            foreach (MemoryData mem in m_aMemory) reg.Write(mem.p_id, mem.p_sz.Y);
        }

        public MemoryData[] p_memSnap
        {
            get { return m_memoryExt; }
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

            public void RunTree(Tree tree, bool bVisible, bool bReadOnly = false)
            {
                m_eRGBW = (eRGBW)tree.Set(m_eRGBW, m_eRGBW, "RGBW", "Set RGBW", bVisible, bReadOnly);
                for (int n = 0; n < m_aPower.Count; n++)
                {
                    m_aPower[n] = tree.Set(m_aPower[n], m_aPower[n], n.ToString("00"), "Light Power (0 ~ 100)", bVisible, bReadOnly);
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

        LightPower m_prevLightPower = null;
        public void RunLight(LightPower lightPower)
        {
            if (m_prevLightPower != null && LightPower.IsSameLight(m_prevLightPower, lightPower)) return;

            m_prevLightPower = lightPower.Clone();
            SetRGBW(lightPower.m_eRGBW);
            for (int n = 0; n < p_lLight; n++)
            {
                Light light = m_lightSet.m_aLight[n];
                if (light.m_light != null) light.m_light.p_fSetPower = lightPower.m_aPower[n];
            }
        }

        public void RunLightOff()
        {
            for (int n = 0; n < p_lLight; n++)
            {
                Light light = m_lightSet.m_aLight[n];
                if (light.m_light != null) light.m_light.p_fSetPower = 0;
                m_prevLightPower.m_aPower[n] = 0;
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

        #region Camera Data
        public double m_dResolution = 3.8;
        public int m_nLine = 78800;
        public bool m_bUseBiDirectional = true;
        public DalsaParameterSet.eUserSet m_eCamUserSet = DalsaParameterSet.eUserSet.Default;
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
            public DalsaParameterSet.eFlatFieldUserSet m_eForwardUserSet = DalsaParameterSet.eFlatFieldUserSet.Factory;
            public DalsaParameterSet.eFlatFieldUserSet m_eBackwardUserSet = DalsaParameterSet.eFlatFieldUserSet.Factory;
            public DalsaParameterSet.eAnalogGain m_eAnalogGain = DalsaParameterSet.eAnalogGain.One;
            public string _sAnalogGain = "1";
            public string m_sAnalogGain
            {
                get { return _sAnalogGain; }
                set
                {
                    _sAnalogGain = value;
                    for (int i = 0; i < DalsaParameterSet.m_aAnalogGain.Length; i++)
                    {
                        if (value == DalsaParameterSet.m_aAnalogGain[i])
                        {
                            m_eAnalogGain = (DalsaParameterSet.eAnalogGain)i;
                            return;
                        }
                    }
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
                m_eForwardUserSet = (DalsaParameterSet.eFlatFieldUserSet)tree.Set(m_eForwardUserSet, m_eForwardUserSet, "Forward UserSet", "Select Flat Field Correction UserSet");
                m_eBackwardUserSet = (DalsaParameterSet.eFlatFieldUserSet)tree.Set(m_eBackwardUserSet, m_eBackwardUserSet, "Reverse UserSet", "Select Flat Field Correction UserSet");
                m_sAnalogGain = tree.Set(m_sAnalogGain, m_sAnalogGain, DalsaParameterSet.m_aAnalogGain, "Analog Gain", "Analog Gain");
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
            public int m_nFovSize = 12000;
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
        public Grab m_grabData = new Grab();
        #endregion

        #region Recipe
        public class Recipe
        {
            #region Snap
            public class Snap
            {
                public eLine m_eLine = eLine.Single;
                public int m_iLine = 0;
                public eCalMode m_eCalMode = eCalMode.RGB; 
                public Boat.eDirection m_eDirection = Boat.eDirection.Forward;
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
                    snap.m_eLine = m_eLine;
                    snap.m_iLine = m_iLine;
                    snap.m_eCalMode = m_eCalMode; 
                    snap.m_eDirection = m_eDirection;
                    snap.m_eEXT = m_eEXT;
                    snap.m_cpMemory = new CPoint(m_cpMemory);
                    snap.m_nOverlap = m_nOverlap;
                    snap.m_lightPower = m_lightPower.Clone();
                    return snap;
                }

                public GrabData GetGrabData(CPoint cpOffset, int nOverlap)
                {
                    GrabData data = new GrabData();
                    data.bInvY = (m_eDirection == Boat.eDirection.Forward);
                    data.m_nOverlap = nOverlap;
                    data.nScanOffsetY = 0;   /*m_cpMemory.Y;*/
                    data.ReverseOffsetY = cpOffset.Y; /*m_cpMemory.Y;*/ /* + m_vision.m_nLine */
                    data.m_bUseFlipVertical = true;
                    m_vision.m_grabData.SetData(data);
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
                    m_eLine = (eLine)tree.Set(m_eLine, m_eLine, "Line", "Line", bVisible, bReadOnly);
                    m_iLine = tree.Set(m_iLine, m_iLine, "Line Index", "Line Index", bVisible, bReadOnly);
                    m_eCalMode = (eCalMode)tree.Set(m_eCalMode, m_eCalMode, "Cal Mode", "Calibration Mode", bVisible, bReadOnly); 
                    m_eDirection = (Boat.eDirection)tree.Set(m_eDirection, m_eDirection, "Direction", "Scan Direction", bVisible, false);
                }

                void RunTreeMemory(Tree tree, bool bVisible, bool bReadOnly = false)
                {
                    m_eEXT = (eEXT)tree.Set(m_eEXT, m_eEXT, "EXT", "Select EXT", bVisible, bReadOnly);
                    m_cpMemory = tree.Set(m_cpMemory, m_cpMemory, "Offset", "Memory Offset Address (pixel)", bVisible, bReadOnly);
                    m_nOverlap = tree.Set(m_nOverlap, m_nOverlap, "Overlap", "Memory Overlap Size (pixel)", bVisible, bReadOnly);
                }

                Vision2D m_vision;
                public Snap(Vision2D vision)
                {
                    m_vision = vision;
                    m_lightPower = new LightPower(vision);
                }
            }
            public List<Snap> m_aSnap = new List<Snap>();
            #endregion

            #region Property
            public int _lSnap = 0;
            public int p_lSnap
            {
                get { return _lSnap; }
                set
                {
                    _lSnap = value;
                    if (m_treeRecipe.p_eMode == Tree.eMode.JobOpen && m_vision.p_eRemote == eRemote.Client)
                    {
                        while (m_aSnap.Count > value) m_aSnap.RemoveAt(m_aSnap.Count - 1);
                        while (m_aSnap.Count < value) m_aSnap.Add(new Snap(m_vision));
                    }
                }
            }

            public enum eSnapMode
            {
                RGB,
                APS,
                ALL
            }
            public eSnapMode m_eSnapMode = eSnapMode.RGB;

            public double _dProductWidth = 0;
            public double p_dProductWidth
            {
                get { return _dProductWidth; }
                set
                {
                    _dProductWidth = value;
                    if (m_treeRecipe.p_eMode == Tree.eMode.JobOpen && m_vision.p_eRemote == eRemote.Client) return;
                    m_aSnap.Clear();

                    double dResolution = m_vision.m_dResolution;
                    double dFOVmm = m_vision.m_grabData.m_nFovSize * dResolution / 1000;
                    eLine eLine = (eLine)((int)Math.Ceiling(_dProductWidth / dFOVmm) - 1);  // 제품 전체를 찍기위한 스냅 횟수

                    m_aSnap.Clear();
                    switch (m_eSnapMode)
                    {
                        case eSnapMode.RGB: AddSnap(eSnapMode.RGB, eLine, Snap.eEXT.EXT1, m_lightPowerRGB); break;
                        case eSnapMode.APS: AddSnap(eSnapMode.APS, eLine, Snap.eEXT.EXT2, m_lightPowerAPS); break;
                        case eSnapMode.ALL:
                            AddSnap(eSnapMode.RGB, eLine, Snap.eEXT.EXT1, m_lightPowerRGB);
                            AddSnap(eSnapMode.APS, eLine, Snap.eEXT.EXT2, m_lightPowerAPS);
                            break; 
                    }
                }
            }

            void AddSnap(eSnapMode eSnapMode, eLine eLine, Snap.eEXT eEXT, LightPower lightPower)
            {
                for (int i = 0; i <= (int)eLine; i++)
                {
                    Snap snap = new Snap(m_vision);
                    snap.m_eLine = eLine;
                    snap.m_iLine = i;
                    snap.m_eCalMode = (eSnapMode == eSnapMode.RGB) ? eCalMode.RGB : eCalMode.APS; 
                    snap.m_nOverlap = m_vision.m_grabData.m_nOverlap;
                    if (m_vision.m_bUseBiDirectional == false) snap.m_eDirection = Boat.eDirection.Forward;
                    else snap.m_eDirection = (i % 2 == 0) ? Boat.eDirection.Forward : Boat.eDirection.Backward;
                    snap.m_eEXT = eEXT;
                    snap.m_lightPower = lightPower.Clone(); 
                }
            }
            #endregion

            #region Tree
            public TreeRoot m_treeRecipe;
            void InitTreeRecipe()
            {
                m_treeRecipe = new TreeRoot(m_vision.p_id, m_vision.m_log);
                m_treeRecipe.UpdateTree += M_treeRecipe_UpdateTree;
            }

            private void M_treeRecipe_UpdateTree()
            {
                RunTreeRecipe(Tree.eMode.Update);
                if (m_treeRecipe.IsUpdated()) RunTreeRecipe(Tree.eMode.Init);
            }

            public void RunTreeRecipe(Tree.eMode eMode)
            {
                m_treeRecipe.p_eMode = eMode;
                RunTreeRecipe(m_treeRecipe, true, true);
            }

            public LightPower m_lightPowerRGB, m_lightPowerAPS;
            public void RunTreeRecipe(Tree tree, bool bVisible, bool bReadOnly = false)
            {
                m_eSnapMode = (eSnapMode)tree.Set(m_eSnapMode, m_eSnapMode, "Snap Mode", "Select Snap Mode", bVisible);
                p_dProductWidth = tree.Set(p_dProductWidth, p_dProductWidth, "Product Width", "Product Width(mm)", bVisible);
                p_lSnap = tree.Set(p_lSnap, p_lSnap, "Count", "Snap Count", bVisible, true);

                if (!(m_treeRecipe.p_eMode == Tree.eMode.JobOpen && m_vision.p_eRemote == eRemote.Client))
                {
                    if (m_eSnapMode == eSnapMode.RGB || m_eSnapMode == eSnapMode.ALL)
                        m_lightPowerRGB.RunTree(tree.GetTree("Light").GetTree("RGB Light", true, bVisible), bVisible);
                    if (m_eSnapMode == eSnapMode.APS || m_eSnapMode == eSnapMode.ALL)
                        m_lightPowerAPS.RunTree(tree.GetTree("Light").GetTree("APS Light", true, bVisible), bVisible);
                }

                for (int n = 0; n < m_aSnap.Count; n++)
                    m_aSnap[n].RunTree(tree.GetTree("Snap").GetTree("Snap" + n.ToString("00"), false, bVisible), bVisible, true);

            }
            #endregion

            #region File
            public string m_sRecipe = "";
            const string c_sExt = ".JEDI";
            public void RecipeSave(string sRecipe)
            {
                string sPath = EQ.c_sPathRecipe + "\\" + sRecipe;
                Directory.CreateDirectory(sPath);
                string sFile = sPath + "\\" + m_vision.p_eVision.ToString() + c_sExt;
                m_treeRecipe.m_job = new Job(sFile, true, m_vision.m_log);
                RunTreeRecipe(Tree.eMode.JobSave);
                m_treeRecipe.m_job.Close();
            }

            public void RecipeOpen(string sRecipe)
            {
                string sPath = EQ.c_sPathRecipe + "\\" + sRecipe;
                Directory.CreateDirectory(sPath);
                string sFile = sPath + "\\" + m_vision.p_eVision.ToString() + c_sExt;
                m_treeRecipe.m_job = new Job(sFile, false, m_vision.m_log);
                RunTreeRecipe(Tree.eMode.JobOpen);
                m_treeRecipe.m_job.Close();
            }
            #endregion

            Vision2D m_vision; 
            public Recipe(Vision2D vision)
            {
                m_vision = vision;
                m_lightPowerRGB = new LightPower(vision);
                m_lightPowerAPS = new LightPower(vision);
                InitTreeRecipe();
            }
        }
        Recipe m_recipe;

        public string p_sRecipe
        {
            get { return m_recipe.m_sRecipe; }
            set
            {
                if (m_recipe.m_sRecipe == value) return;
                m_recipe.RecipeOpen(value);
                OnPropertyChanged(); 
            }
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

        #region Snap
        public string RunSnap(SnapInfo snapInfo, bool bReadRecipe)
        {
            StopWatch sw = new StopWatch();
            try
            {
                if (bReadRecipe) m_recipe.RecipeOpen(p_sRecipe);
                bool bSendSnapInfo = false;
                foreach (Recipe.Snap snap in m_recipe.m_aSnap)
                {
                    RunLight(snap.m_lightPower);
                    if (Run(m_camAxis.RunMove(snap.m_eLine, snap.m_iLine, false))) return p_sInfo;
                    double mmOffsetY = m_camAxis.m_umOffset[snap.m_eLine][snap.m_iLine].Y / 1000.0;
                    if (Run(m_boat.RunMove(snap.m_eDirection, mmOffsetY, false))) return p_sInfo; 
                    if (bSendSnapInfo == false)
                    {
                        if (Run(m_process.SendSnapInfo(snapInfo))) return p_sInfo;
                        bSendSnapInfo = true; 
                    }
                    if (Run(m_boat.m_axis.WaitReady())) return p_sInfo;
                    if (Run(m_camAxis.m_axis.WaitReady())) return p_sInfo;
                    if (Run(StartSnap(snap))) return p_sInfo;
                    if (Run(m_boat.StartSnap())) return p_sInfo; 
                    if (Run(WaitSnap())) return p_sInfo;
                    if (Run(m_boat.WaitSnap())) return p_sInfo;
                }
                RunLightOff();
                m_log.Info("Run Snap End : " + (sw.ElapsedMilliseconds / 1000.0).ToString("0.00") + " sec");
                if (Run(m_boat.RunMove(Boat.ePos.Done))) return p_sInfo;
                return "OK";
            }
            finally
            {
                m_boat.m_axis.RunTrigger(false);
                m_boat.RunMove(Boat.ePos.Done);
            }
        }

        string StartSnap(Recipe.Snap snap)
        {
            try
            {
                m_log.Info("Snap Start");
                if (snap.m_iLine == 0)
                {
                    SetGain(snap.m_eCalMode);
                    m_log.Info("Set Gain Done");
                }
                SetCalUserSet(snap);
                m_log.Info("Set Cal Userset Done");

                MemoryData memory = p_memSnap[(int)snap.m_eEXT];
                CPoint cpOffset = CalcOffset(snap);
                GrabData grabData = snap.GetGrabData(cpOffset, m_grabData.m_nOverlap);
                m_camera.GrabLineScan(memory, cpOffset, m_nLine, grabData);
                return "OK";
            }
            catch (Exception e)
            {
                m_camera.StopGrab();
                return e.Message; 
            }
        }

        string WaitSnap()
        {
            while (m_camera.p_CamInfo.p_eState != eCamState.Ready)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
            }
            return "OK";
        }

        void SetGain(eCalMode eMode)
        {
            CalibrationData data = m_aCalData[eMode];
            m_camera.p_CamParam.SetAnalogGain(data.m_eAnalogGain);
            m_camera.p_CamParam.ChangeGainSelector(DalsaParameterSet.eGainSelector.System);
            m_camera.p_CamParam.SetGain(data.m_dSystemGain);
            m_camera.p_CamParam.ChangeGainSelector(DalsaParameterSet.eGainSelector.Blue);
            m_camera.p_CamParam.SetGain(data.m_dBlueGain);
            m_camera.p_CamParam.ChangeGainSelector(DalsaParameterSet.eGainSelector.Green);
            m_camera.p_CamParam.SetGain(data.m_dGreenGain);
            m_camera.p_CamParam.ChangeGainSelector(DalsaParameterSet.eGainSelector.Red);
            m_camera.p_CamParam.SetGain(data.m_dRedGain);
        }

        void SetCalUserSet(Recipe.Snap snap)
        {
            switch (snap.m_eDirection)
            {
                case Boat.eDirection.Forward: m_camera.p_CamParam.p_eFlatFieldCorrection = m_aCalData[snap.m_eCalMode].m_eForwardUserSet; break;
                case Boat.eDirection.Backward: m_camera.p_CamParam.p_eFlatFieldCorrection = m_aCalData[snap.m_eCalMode].m_eBackwardUserSet; break;
            }
        }

        CPoint CalcOffset(Recipe.Snap snap)
        {
            switch (snap.m_eDirection)
            {
                case Boat.eDirection.Forward: return new CPoint(snap.m_iLine * m_grabData.m_nFovSize, m_grabData.m_nReverseOffset);
                case Boat.eDirection.Backward: return new CPoint(snap.m_iLine * m_grabData.m_nFovSize, 0);
            }
            return new CPoint(snap.m_iLine * m_grabData.m_nFovSize, m_grabData.m_nReverseOffset);
        }
        #endregion

        #region override
        public override void Reset()
        {
            m_process?.Reset(); 
            foreach (Remote.Protocol protocol in m_remote.m_aProtocol) protocol.m_bDone = true;
            base.Reset();
        }

        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            string sRun = base.StateHome();
            p_eState = (sRun == "OK") ? eState.Ready : eState.Error;
            return sRun; 
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            if (p_eRemote == eRemote.Client) return;
            p_lLight = tree.GetTree("Light", false).Set(p_lLight, p_lLight, "Channel", "Light Channel Count");
            RunCameraTree(tree.GetTree("Camera", true));
            m_camAxis.RunTree(tree.GetTree("Camera Axis")); 
            m_process?.RunTree(tree.GetTree("Process"));
            RunGrabDataTree(tree.GetTree("GrabData", false));
        }

        void RunCameraTree(Tree tree)
        {
            m_dResolution = tree.Set(m_dResolution, m_dResolution, "Pixel Resolution", "Pixel Resolution (um/pixel)");
            m_nLine = tree.Set(m_nLine, m_nLine, "Line", "Memory Snap Lines (pixel)");
            m_bUseBiDirectional = tree.Set(m_bUseBiDirectional, m_bUseBiDirectional, "BiDirectional Scan", "Use BiDirectional Scan");
            m_eCamUserSet = (DalsaParameterSet.eUserSet)tree.Set(m_eCamUserSet, m_eCamUserSet, "UserSet", "Select Camera UserSet");
            RunCalibrationTree(tree.GetTree("Calibration"));
        }

        void RunCalibrationTree(Tree tree)
        {
            m_aCalData[eCalMode.RGB].RunTree(tree.GetTree("RGB Setting"));
            m_aCalData[eCalMode.APS].RunTree(tree.GetTree("APS Setting"));
        }

        void RunGrabDataTree(Tree tree)
        {
            m_grabData.RunTree(tree.GetTree("GrabData"));
        }
        #endregion

        public eVision p_eVision { get; set; }
        public Boat m_boat; 
        public Vision2D(eVision eVision, IEngineer engineer)
        {
            p_eVision = eVision;
            InitBase(eVision.ToString(), engineer, eRemote.Client);
        }

        VisionProcess m_process = null; 
        public Vision2D(string id, IEngineer engineer)
        {
            m_process = new VisionProcess(this);
            m_boat = new Boat(id);
            m_recipe = new Recipe(this);
            InitCalData(); 
            InitBase(id, engineer, eRemote.Server);
        }

        public override void ThreadStop()
        {
            m_process?.ThreadStop(); 
            base.ThreadStop();
        }
    }
}
