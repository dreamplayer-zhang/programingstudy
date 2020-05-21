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
        MemoryPool m_memoryPool;
        MemoryGroup m_memoryGroup;
        LightSet m_lightSet;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Rotate");
            p_sInfo = m_toolBox.Get(ref m_axisXZ, this, "Camera XZ"); 
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            foreach (eCam cam in Enum.GetValues(typeof(eCam))) m_lineScan[cam].GetTool(this);
            foreach (eCam cam in Enum.GetValues(typeof(eCam))) m_areaScan[cam].GetTool(this); 
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

            public string m_id;
            eCam m_eCam; 
            public LineScan(eCam cam)
            {
                m_eCam = cam;
                m_id = "Dalsa." + cam.ToString(); 
            }
        }

        Dictionary<eCam, LineScan> m_lineScan = new Dictionary<eCam, LineScan>(); 
        void InitLineScan()
        {
            foreach (eCam cam in Enum.GetValues(typeof(eCam))) m_lineScan.Add(cam, new LineScan(cam));
        }
        #endregion

        #region AreaScan
        public class AreaScan
        {
            CameraBasler m_camera = null;
            MemoryData m_memory = null;
            LightSet m_lightSet = null;

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
        void InitMemory()
        {
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
            RunTreeDalsa(tree.GetTree("Dalsa", false));
            RunTreeBasler(tree.GetTree("Basler", false));
        }
        #endregion

        public Siltron(string id, IEngineer engineer)
        {
            InitLineScan();
            InitAreaScan(); 
            base.InitBase(id, engineer);
            InitMemory(); 
        }

        public override void ThreadStop()
        {
            //
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            
        }
        #endregion
    }
}
