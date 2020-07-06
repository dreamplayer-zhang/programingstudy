using RootTools;
using RootTools.Control;
using RootTools.Control.Ajin;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Gem.XGem;
using RootTools.Inspects;
using RootTools.Memory;
using RootTools.Module;
using RootTools.ToolBoxs;

namespace Root_Wind
{
    public class Wind_Engineer : IEngineer
    {
        #region IEngineer
        public Login m_login = new Login();
        public Login.User p_user { get { return m_login.p_user; } }

        public IGem ClassGem() { return m_xGem; }

        public IControl ClassControl() { return m_ajin; }

        GAF m_gaf = new GAF();
        public GAF ClassGAF() { return m_gaf; }

        ToolBox m_toolBox = new ToolBox();
        public ToolBox ClassToolBox() { return m_toolBox; }

        public MemoryTool ClassMemoryTool() { return m_toolBox.m_memoryTool; }

        public IHandler ClassHandler() { return m_handler; }

        public ModuleList ClassModuleList() { return m_handler.m_moduleList; }

        public MemoryData GetMemory(string sPool, string sGroup, string sMemory)
        {
            MemoryPool pool = m_toolBox.m_memoryTool.GetPool(sPool, false);
            return (pool == null) ? null : pool.GetMemory(sGroup, sMemory); 
        }

        #endregion

        #region Ajin
        Ajin m_ajin = new Ajin();
        Ajin_UI m_ajinUI = new Ajin_UI();
        void InitAjin()
        {
            m_ajin.Init("Ajin", this);
            m_ajinUI.Init(m_ajin);
            m_toolBox.AddToolSet(m_ajin, m_ajinUI);
            m_toolBox.m_toolDIO = m_ajin.m_dio;
        }
        #endregion

        #region XGem
        XGem m_xGem = new XGem();
        XGem_UI m_xGemUI = new XGem_UI();
        void InitXGem()
        {
            m_xGem.Init("XGem", this);
            m_xGemUI.Init(m_xGem);
            m_toolBox.AddToolSet(m_xGem, m_xGemUI);
        }
        #endregion

        public Recipe m_recipe = new Recipe();

        public Wind_Handler m_handler = new Wind_Handler();
        public void Init(string id)
        {
            EQ.m_sModel = id;
            LogView.Init();
            m_login.Init();
            m_toolBox.Init(id, this);
            InitAjin();
            InitXGem();
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

    }
}
