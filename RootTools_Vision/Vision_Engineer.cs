using RootTools;
using RootTools.Memory;
using RootTools.ToolBoxs;
using RootTools.Module;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Control;
using RootTools.Control.Ajin;

namespace RootTools_Vision
{
    public class Vision_Engineer : IEngineer
    {
        public Login m_login = new Login();
        public Login.User p_user => m_login.p_user;


        public GAF ClassGAF()
        {
            return null;
        }

        public IGem ClassGem()
        {
            return null;
        }

        public IHandler ClassHandler()
        {
            return null;
        }

        ToolBox m_toolBox = new ToolBox();
        public ToolBox ClassToolBox()
        {
            return m_toolBox;
        }
        public MemoryTool ClassMemoryTool()
        {
            return m_toolBox.m_memoryTool;
        }

        public ModuleList ClassModuleList()
        {
            return null;
        }

        public MemoryData GetMemory(string sPool, string sGroup, string sMemory)
        {
            MemoryPool pool = m_toolBox.m_memoryTool.GetPool(sPool);
            return (pool == null) ? null : pool.GetMemory(sGroup, sMemory);
        }


        public void Init(string id)
        {
            EQ.m_sModel = id;
            LogView.Init();
            m_login.Init();
            m_toolBox.Init(id, this);
        }

        public IControl ClassControl()
        {
            return new Ajin();
        }
    }
}
