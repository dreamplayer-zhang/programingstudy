using RootTools;
using RootTools.Gem;
using RootTools.Gem.XGem;
using RootTools.Memory;
using RootTools.Control.Ajin;
using RootTools.Inspects;
using RootTools.GAFs;
using RootTools.ToolBoxs;
using RootTools.Module;
using RootTools.SQLogs;

namespace Root_Vega
{
    public class Vega_Engineer : IEngineer
    {
        #region IEngineer
        LogView m_logView = new LogView();
        public LogView ClassLogView() { return m_logView; }

        public Login m_login = new Login();
        public Login.User p_user
        { 
            get { return m_login.p_user; }
        }

        public IGem ClassGem() { return m_xGem; }

        GAF m_gaf = new GAF();
        public GAF ClassGAF() { return m_gaf; }

        //GAF_Manager m_GAFManager = new GAF_Manager();
        //public object ClassGAFManager() { return m_GAFManager; }

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

        #region Ajin
        public Ajin m_ajin = new Ajin();
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
        public Vega_Handler m_handler = new Vega_Handler();
        public InspectionManager m_InspManager = new InspectionManager();
        public void Init(string id)
        {
            EQ.m_sModel = id;
            m_logView.Init();
            m_login.Init(m_logView);
            m_toolBox.Init(id, this);
            InitAjin();
            InitXGem(); 
            m_handler.Init(id, this);
            m_gaf.Init(id, this);
        }

        public void ThreadStop()
        {
            m_gaf.ThreadStop();
            m_xGem.ThreadStop(); 
            m_handler.ThreadStop();
            m_toolBox.ThreadStop();
            m_login.ThreadStop();
            m_logView.ThreadStop();
        }
    }
}
