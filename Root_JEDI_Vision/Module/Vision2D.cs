using RootTools;
using RootTools.Camera;
using RootTools.Camera.Dalsa;
using RootTools.Comm;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;

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
                m_process.GetTools(m_toolBox, bInit); 
                if (bInit)
                {
                    m_rs232RGBW.p_bConnect = true;
                    m_camera.Connect();
                    InitMemory();
                }
            }
            m_remote.GetTools(bInit);
        }
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

        #region override
        public override void Reset()
        {
            m_process?.Reset(); 
            foreach (Remote.Protocol protocol in m_remote.m_aProtocol) protocol.m_bDone = true;
            base.Reset();
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            if (p_eRemote == eRemote.Client) return;
            p_lLight = tree.GetTree("Light", false).Set(p_lLight, p_lLight, "Channel", "Light Channel Count");
            RunCameraTree(tree.GetTree("Camera", true));
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
        public Vision2D(eVision eVision, IEngineer engineer)
        {
            p_eVision = eVision;
            InitBase("Vision " + eVision.ToString(), engineer, eRemote.Client);
        }

        VisionProcess m_process = null; 
        public Vision2D(string id, IEngineer engineer)
        {
            m_process = new VisionProcess(this); 
            InitBase(id, engineer, eRemote.Server);
        }

        public override void ThreadStop()
        {
            //if (m_bThreadCheck)
            //{
                //m_bThreadCheck = false;
                //m_threadCheck.Join();
            //}
            m_process?.ThreadStop(); 
            base.ThreadStop();
        }
    }
}
