using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root.Module
{
    public class Siltron : ModuleBase
    {
        public enum eCam
        {
            Side,
            Top,
            Bottom
        }

        #region ToolBox
        Axis m_axisRotate;
        AxisXY m_axisXZ;
        DIO_O m_doVac;
        DIO_O m_doBlow; 
        MemoryPool m_memoryPool;
        MemoryGroup m_memoryGroup;
        LightSet m_lightSet;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Rotate");
            p_sInfo = m_toolBox.Get(ref m_axisXZ, this, "Camera XZ");
            p_sInfo = m_toolBox.Get(ref m_doVac, this, "Stage Vacuum");
            p_sInfo = m_toolBox.Get(ref m_doBlow, this, "Stage Blow");
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            foreach (eCam cam in Enum.GetValues(typeof(eCam))) m_lineScan[cam].GetTool(this);
            foreach (eCam cam in Enum.GetValues(typeof(eCam))) m_areaScan[cam].GetTool(this); 
        }
        #endregion

        #region DIO
        public bool p_bStageVac
        {
            get { return m_doVac.p_bOut; }
            set
            {
                if (m_doVac.p_bOut == value) return;
                m_doVac.Write(value); 
            }
        }

        public bool p_bStageBlow
        {
            get { return m_doBlow.p_bOut; }
            set
            {
                if (m_doBlow.p_bOut == value) return;
                m_doBlow.Write(value); 
            }
        }

        public void RunBlow(int msDelay)
        {
            m_doBlow.DelayOff(msDelay); 
        }
        #endregion

        #region Axis
        public int m_pulseRound = 1000; 
        void RunTreeAxis(Tree tree)
        {
            m_pulseRound = tree.Set(m_pulseRound, m_pulseRound, "Rotate Pulse / Round", "Rotate Axis Pulse / 1 Round (pulse)"); 
        }
        #endregion

        #region LineScan
        public class LineScan
        {
            public CameraDalsa m_camera = null;
            public MemoryData m_memory = null;
            public LightSet m_lightSet = null;

            public void GetTool(Siltron siltron)
            {
                siltron.p_sInfo = siltron.m_toolBox.Get(ref m_camera, siltron, m_id);
            }

            public void InitMemory(Siltron siltron, CPoint szDalsaGrab)
            {
                m_memory = siltron.m_memoryGroup.CreateMemory(m_id, 1, m_camera.p_nByte, szDalsaGrab);
                m_camera.SetMemoryData(m_memory); 
            }

            public bool m_bRunGrab = false; 
            public void ModuleRunTree(Tree tree)
            {
                m_bRunGrab = tree.Set(m_bRunGrab, m_bRunGrab, m_id, "Module Run Grab"); 
            }

            public string m_id;
            eCam m_eCam; 
            public LineScan(eCam cam, string id)
            {
                m_eCam = cam;
                m_id = id;
            }
        }

        Dictionary<eCam, LineScan> m_lineScan = new Dictionary<eCam, LineScan>(); 
        void InitLineScan()
        {
            foreach (eCam cam in Enum.GetValues(typeof(eCam)))
            {
                string id = m_stringDalsa.p_sValue + "." + m_stringCam[(int)cam].p_sValue; 
                m_lineScan.Add(cam, new LineScan(cam, id));
            }
        }
        #endregion

        #region AreaScan
        public class AreaScan
        {
            CameraBasler m_camera = null;
            MemoryData m_memory = null;
            //LightSet m_lightSet = null;

            public void GetTool(Siltron siltron)
            {
                siltron.p_sInfo = siltron.m_toolBox.Get(ref m_camera, siltron, m_id);
            }

            public void InitMemory(Siltron siltron, int nBaslerGrab)
            {
                m_memory = siltron.m_memoryGroup.CreateMemory(m_id, nBaslerGrab, m_camera.p_nByte, m_camera.p_sz);
                m_camera.SetMemoryData(m_memory);
            }

            public string m_id; 
            eCam m_eCam;
            public AreaScan(eCam cam)
            {
                m_eCam = cam;
                m_id = "Basler." + cam.ToString();
            }
        }

        Dictionary<eCam, AreaScan> m_areaScan = new Dictionary<eCam, AreaScan>();
        void InitAreaScan()
        {
            foreach (eCam cam in Enum.GetValues(typeof(eCam))) m_areaScan.Add(cam, new AreaScan(cam));
        }
        #endregion

        #region Memory
        public override void InitMemorys()
        {
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            foreach (eCam cam in Enum.GetValues(typeof(eCam))) m_lineScan[cam].InitMemory(this, m_szDalsaGrab);
            foreach (eCam cam in Enum.GetValues(typeof(eCam))) m_areaScan[cam].InitMemory(this, m_nBaslerGrab);
        }

        CPoint m_szDalsaGrab = new CPoint(1024, 1024);
        void RunTreeDalsa(Tree tree)
        {
            m_szDalsaGrab = tree.Set(m_szDalsaGrab, m_szDalsaGrab, "Grab Size", "Dalsa Grab Size (pixel)"); 
        }

        int m_nBaslerGrab = 10; 
        void RunTreeBasler(Tree tree)
        {
            m_nBaslerGrab = tree.Set(m_nBaslerGrab, m_nBaslerGrab, "Grab Count", "Basler Continuous Grab Count");
        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeDalsa(tree.GetTree(m_stringDalsa.p_sValue, false));
            RunTreeBasler(tree.GetTree(m_stringBasler.p_sValue, false));
            RunTreeAxis(tree.GetTree("Axis", false));
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            p_bStageBlow = false; 
            p_bStageVac = true;
            Thread.Sleep(200);
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            //p_bStageVac = false;
            return "OK"; 
        }
        #endregion

        #region String Table
        StringTable.String m_stringDalsa = StringTable.Get("Dalsa", "Siltron");
        StringTable.String m_stringBasler = StringTable.Get("Basler", "Siltron");
        StringTable.String[] m_stringCam = new StringTable.String[3];
        StringTable.String m_stringSide = StringTable.Get("Side", "Siltron");
        StringTable.String m_stringTop = StringTable.Get("Top", "Siltron");
        StringTable.String m_stringBottom = StringTable.Get("Bottom", "Siltron");
        #endregion

        public Siltron(string id, IEngineer engineer)
        {
            m_stringCam[0] = m_stringSide;
            m_stringCam[1] = m_stringTop;
            m_stringCam[2] = m_stringBottom;
            InitLineScan();
            InitAreaScan(); 
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            //
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            AddModuleRunList(new Run_Rotate(this), false, "Rotate Axis");
            AddModuleRunList(new Run_GrabLineScan(this), false, "Run Grab LineScan Camera");
        }

        public class Run_Delay : ModuleRunBase
        {
            Siltron m_module;
            public Run_Delay(Siltron module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Delay run = new Run_Delay(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return "OK";
            }
        }

        public class Run_Rotate : ModuleRunBase
        {
            Siltron m_module; 
            public Run_Rotate(Siltron module)
            {
                m_module = module;
                InitModuleRun(module); 
            }

            double m_fDeg = 0; 
            public override ModuleRunBase Clone()
            {
                Run_Rotate run = new Run_Rotate(m_module);
                run.m_fDeg = m_fDeg; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_fDeg = tree.Set(m_fDeg, m_fDeg, "Degree", "Rotation Degree (0 ~ 360)", bVisible);
            }

            public override string Run()
            {
                int pulseRound = m_module.m_pulseRound;
                Axis axis = m_module.m_axisRotate; 
                int pulse = (int)Math.Round(m_module.m_pulseRound * m_fDeg / 360);
                while (pulse < axis.p_posCommand) pulse += pulseRound; 
                {
                    axis.p_posCommand -= pulseRound;
                    axis.p_posActual -= pulseRound;
                }
                if (m_module.Run(axis.StartMove(pulse))) return p_sInfo;
                return axis.WaitReady(); 
            }
        }

        public class Run_GrabLineScan : ModuleRunBase
        {
            Siltron m_module;
            public Run_GrabLineScan(Siltron module)
            {
                m_module = module;
                m_lineScan = module.m_lineScan; 
                InitModuleRun(module);
            }

            Dictionary<eCam, LineScan> m_lineScan = new Dictionary<eCam, LineScan>();
            public override ModuleRunBase Clone()
            {
                Run_GrabLineScan run = new Run_GrabLineScan(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                foreach (LineScan line in m_lineScan.Values) line.ModuleRunTree(tree.GetTree("Grab Camera")); 
            }

            public override string Run()
            {
                m_module.RunBlow(2000);
                return "OK"; 
            }
        }
        #endregion
    }
}
