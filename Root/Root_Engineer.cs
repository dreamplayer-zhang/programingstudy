using RootTools;
using RootTools.Control.Ajin;
using RootTools.Gem;
using RootTools.Memory;
using RootTools.GAFs;
using RootTools.ToolBoxs;
using RootTools.Module;

namespace Root
{
    public class Root_Engineer : IEngineer
    {
        #region IEngineer
        public Login m_login = new Login();
        public Login.User p_user { get { return m_login.p_user; } }

        public IGem ClassGem() { return null; }

        GAF m_gaf = new GAF();
        public GAF ClassGAF() { return m_gaf; }

        ToolBox m_toolBox = new ToolBox(); 
        public ToolBox ClassToolBox() { return m_toolBox; }

        public MemoryTool ClassMemoryTool() { return m_toolBox.m_memoryTool; }

        public IHandler ClassHandler() { return m_handler; }

        public ModuleList ClassModuleList() { return m_handler.m_moduleList; }

        public MemoryData GetMemory(string sPool, string sGroup, string sMemory)
        {
            MemoryPool pool = m_toolBox.m_memoryTool.GetPool(sPool);
            return (pool == null) ? null : pool.GetMemory(sGroup, sMemory);
        }

        #endregion

        #region ToolBox
        Ajin m_ajin = new Ajin();
        Ajin_UI m_ajinUI = new Ajin_UI(); 
        void InitAjin()
        {
            m_ajin.Init("Ajin", this);
            m_ajinUI.Init(m_ajin);
            m_toolBox.AddToolSet(m_ajin, m_ajinUI); 
            m_toolBox.m_toolDIO = m_ajin.m_dio;
            m_toolBox.m_toolAxis = m_ajin.m_listAxis.m_aAxis; 
        }
        #endregion

        public Root_Handler m_handler = new Root_Handler();
        public void Init(string id)
        {
            EQ.m_sModel = id; 
            LogViewer.Init();
            m_login.Init();
            m_toolBox.Init(id, this);
            InitAjin();
            m_handler.Init(id, this);
        }

        public void ThreadStop()
        {
            m_handler.ThreadStop(); 
            m_toolBox.ThreadStop();
            m_login.ThreadStop();
            LogViewer.ThreadStop();
        }
    }
}
