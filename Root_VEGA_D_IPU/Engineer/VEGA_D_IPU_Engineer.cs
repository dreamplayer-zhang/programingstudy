using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Memory;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;

namespace Root_VEGA_D_IPU.Engineer
{
    public class VEGA_D_IPU_Engineer : IEngineer
    {
        #region IEngineer
        public Login m_login = new Login();
        public Login.User p_user { get { return m_login.p_user; } }

        public IControl ClassControl()
        {
            return null;
        }

        public GAF m_gaf = new GAF();
        public GAF ClassGAF() { return m_gaf; }

        public IGem ClassGem() { return null; }

        public IHandler ClassHandler() { return m_handler; }

        public MemoryTool ClassMemoryTool() { return m_toolBox.m_memoryTool; }

        public ModuleList ClassModuleList() { return m_handler.p_moduleList; }

        ToolBox m_toolBox = new ToolBox();
        public ToolBox ClassToolBox() { return m_toolBox; }

        public MemoryData GetMemory(string sPool, string sGroup, string sMemory)
        {
            MemoryPool pool = m_toolBox.m_memoryTool.GetPool(sPool);
            return (pool == null) ? null : pool.GetMemory(sGroup, sMemory);
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(EQ.m_sModel, null);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
            RunTree(Tree.eMode.Init);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
        }
        #endregion

        public VEGA_D_IPU_Handler m_handler = new VEGA_D_IPU_Handler();
        public void Init(string id)
        {
            EQ.m_sModel = id;
            LogView.Init();
            InitTree();
            m_login.Init();
            m_toolBox.Init(id, this);
            m_handler.Init(id, this);
            m_gaf.Init(id, this);
        }

        public void ThreadStop()
        {
            m_gaf.ThreadStop();
            //m_xGem.ThreadStop();
            m_handler.ThreadStop();
            m_toolBox.ThreadStop();
            m_login.ThreadStop();
            LogView.ThreadStop();
        }

        public string BuzzerOff()
        {
            return "OK";
        }

        public string Recovery()
        {
            return "OK";
        }
    }
}
