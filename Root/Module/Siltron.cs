using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;

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
        MemoryPool m_memoryPool;
        CameraDalsa[] m_camDalsa = new CameraDalsa[3]; 
        CameraBasler[] m_camBasler = new CameraBasler[3];
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            foreach (eCam cam in Enum.GetValues(typeof(eCam)))
            {
                p_sInfo = m_toolBox.Get(ref m_camDalsa[(int)cam], this, "Dalsa " + cam.ToString()); 
            }
            foreach (eCam cam in Enum.GetValues(typeof(eCam)))
            {
                p_sInfo = m_toolBox.Get(ref m_camBasler[(int)cam], this, "Basler " + cam.ToString());
            }
        }
        #endregion

        #region Memory
        MemoryGroup m_memoryGroup;
        MemoryData[] m_memDalsa = new MemoryData[3];
        MemoryData[] m_memBasler = new MemoryData[3];
        void InitMemory()
        {
            foreach (eCam cam in Enum.GetValues(typeof(eCam)))
            {
                m_memDalsa[(int)cam] = m_memoryGroup.CreateMemory("Dalsa " + cam.ToString(), 1, 1, m_szDalsaGrab);
                m_camDalsa[(int)cam].SetMemoryData(m_memBasler[(int)cam]); 
            }
            foreach (eCam cam in Enum.GetValues(typeof(eCam)))
            {
                m_memBasler[(int)cam] = m_memoryGroup.CreateMemory("Basler " + cam.ToString(), m_nBaslerGrab, m_camBasler[(int)cam].p_nByte, m_camBasler[(int)cam].p_sz);
                m_camBasler[(int)cam].SetMemoryData(m_memBasler[(int)cam]); 
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
