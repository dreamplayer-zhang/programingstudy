using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace Root.Module
{
    public class Siltron : ModuleBase
    {
        enum eCam
        {
            Side,
            Top,
            Bottom
        }
        #region ToolBox
        Axis m_axisRotate;
        AxisXY m_axisXZ; 
        MemoryPool m_memoryPool;
        CameraDalsa[] m_aCamDalsa = new CameraDalsa[3]; 
        CameraBasler[] m_aCamBasler = new CameraBasler[3];
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Rotate");
            p_sInfo = m_toolBox.Get(ref m_axisXZ, this, "Camera XZ"); 
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            foreach (eCam cam in Enum.GetValues(typeof(eCam)))
            {
                p_sInfo = m_toolBox.Get(ref m_aCamDalsa[(int)cam], this, "Dalsa " + cam.ToString()); 
            }
            foreach (eCam cam in Enum.GetValues(typeof(eCam)))
            {
                p_sInfo = m_toolBox.Get(ref m_aCamBasler[(int)cam], this, "Basler " + cam.ToString());
            }
        }
        #endregion

        #region Memory
        MemoryGroup m_memoryGroup;
        Dictionary<eCam, CameraDalsa> m_camDalsa = new Dictionary<eCam, CameraDalsa>();
        Dictionary<eCam, CameraBasler> m_camBasler = new Dictionary<eCam, CameraBasler>();
        Dictionary<eCam, MemoryData> m_memDalsa = new Dictionary<eCam, MemoryData>();
        Dictionary<eCam, MemoryData> m_memBasler = new Dictionary<eCam, MemoryData>();
        void InitMemory()
        {
            foreach (eCam cam in Enum.GetValues(typeof(eCam)))
            {
                m_camDalsa.Add(cam, m_aCamDalsa[(int)cam]);
                m_memDalsa.Add(cam, m_memoryGroup.CreateMemory(m_camDalsa[cam].p_id, 1, m_camDalsa[cam].p_nByte, m_szDalsaGrab));
                m_camDalsa[cam].SetMemoryData(m_memDalsa[cam]);
            }
            foreach (eCam cam in Enum.GetValues(typeof(eCam)))
            {
                m_camBasler.Add(cam, m_aCamBasler[(int)cam]);
                m_memBasler.Add(cam, m_memoryGroup.CreateMemory(m_camBasler[cam].p_id, m_nBaslerGrab, m_camBasler[cam].p_nByte, m_aCamBasler[(int)cam].p_sz));
                m_camBasler[cam].SetMemoryData(m_memBasler[cam]);
            }
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
