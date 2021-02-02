using RootTools;
using RootTools.Control.Ajin;
using RootTools.Gem;
using RootTools.Memory;
using RootTools.GAFs;
using RootTools.ToolBoxs;
using RootTools.Module;
using RootTools.Control;
using RootTools.Control.ACS;

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

        public ModuleList ClassModuleList() { return m_handler.p_moduleList; }

        public MemoryData GetMemory(string sPool, string sGroup, string sMemory)
        {
            MemoryPool pool = m_toolBox.m_memoryTool.GetPool(sPool);
            return (pool == null) ? null : pool.GetMemory(sGroup, sMemory);
        }
        #endregion

        public IControl ClassControl() { return m_acs; }

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

        #region ACS
        ACS m_acs = new ACS();
        ACS_UI m_acsUI = new ACS_UI(); 
        void InitACS()
        {
            m_acs.Init("ACS", this);
            m_acsUI.Init(m_acs);
            m_toolBox.AddToolSet(m_acs, m_acsUI);
            m_toolBox.m_toolDIO = m_acs.m_dio;
        }
        #endregion

        public Root_Handler m_handler = new Root_Handler();
        public void Init(string id)
        {
            EQ.m_sModel = id; 
            LogView.Init();
            m_login.Init();
            m_toolBox.Init(id, this);
            //InitAjin();
            InitACS();
            m_handler.Init(id, this);

        }

        public void ThreadStop()
        {
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
