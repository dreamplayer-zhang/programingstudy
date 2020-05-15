using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;

namespace Root.Module
{
    public class Siltron : ModuleBase
    {
        #region ToolBox
        MemoryPool m_memoryPool;
        MemoryGroup m_memoryGroup; 
        Camera_Dalsa m_camDalsaSide;
        Camera_Dalsa m_camDalsaTop;
        Camera_Dalsa m_camDalsaBottom;
        Camera_Basler m_camBaslerSide;
        Camera_Basler m_camBaslerTop;
        Camera_Basler m_camBaslerBottom;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            p_sInfo = m_toolBox.Get(ref m_camDalsaSide, this, "Dalsa Side");
            p_sInfo = m_toolBox.Get(ref m_camDalsaTop, this, "Dalsa Top");
            p_sInfo = m_toolBox.Get(ref m_camDalsaBottom, this, "Dalsa Bottom");
            p_sInfo = m_toolBox.Get(ref m_camBaslerSide, this, "Basler Side");
            p_sInfo = m_toolBox.Get(ref m_camBaslerTop, this, "Basler Top");
            p_sInfo = m_toolBox.Get(ref m_camBaslerBottom, this, "Basler Bottom");
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
            //
        }
        #endregion

        public Siltron(string id, IEngineer engineer)
        {
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
            
        }
        #endregion
    }
}
