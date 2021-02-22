using RootTools;
using RootTools.Control;
using RootTools.Control.ACS;
using RootTools.Control.Ajin;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Gem.XGem;
using RootTools.Memory;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;

namespace Root_AxisMapping
{
    public class AxisMapping_Engineer : NotifyProperty, IEngineer
    {
        #region IEngineer
        public Login m_login = new Login();
        public Login.User p_user { get { return m_login.p_user; } }

        public IControl ClassControl()
        {
            switch (m_eControl)
            {
                case eControl.Ajin: return m_ajin;
                case eControl.ACS: return m_acs;
            }
            return m_ajin;
        }

        public GAF m_gaf = new GAF();
        public GAF ClassGAF() { return m_gaf; }

        public IGem ClassGem() { return m_xGem; }

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

        #region Control
        enum eControl
        {
            Ajin,
            ACS
        };
        eControl m_eControl = eControl.Ajin;

        Registry m_reg = new Registry("AxisMapping");
        void InitControl()
        {
            m_eControl = (eControl)m_reg.Read("Control", (int)0); 
            switch (m_eControl)
            {
                case eControl.Ajin: InitAjin(); break;
                case eControl.ACS: InitACS(); break; 
            }
        }

        void RunTreeControl(Tree tree)
        {
            m_eControl = (eControl)tree.Set(m_eControl, m_eControl, "Control", "Select Control");
            m_reg.Write("Control", (int)m_eControl); 
        }
        #endregion

        #region ACS
        ACS m_acs;
        ACS_UI m_acsUI;
        void InitACS()
        {
            m_acs = new ACS();
            m_acsUI = new ACS_UI();
            m_acs.Init("ACS", this);
            m_acsUI.Init(m_acs);
            m_toolBox.AddToolSet(m_acs, m_acsUI);
            m_toolBox.m_toolDIO = m_acs.m_dio;
        }
        #endregion

        #region Ajin
        public Ajin m_ajin;
        Ajin_UI m_ajinUI;
        void InitAjin()
        {
            m_ajin = new Ajin();
            m_ajinUI = new Ajin_UI();
            m_ajin.Init("Ajin", this);
            m_ajinUI.Init(m_ajin);
            m_toolBox.AddToolSet(m_ajin, m_ajinUI);
            m_toolBox.m_toolDIO = m_ajin.m_dio;
        }
        #endregion

        #region XGem
        //bool m_bUseXGem = false;
        bool _bUseXGem = false;
        public bool p_bUseXGem
        {
            get { return _bUseXGem; }
            set
            {
                if (_bUseXGem == value) return;
                _bUseXGem = value;
                OnPropertyChanged();
            }
        }
        XGem m_xGem = null;
        XGem_UI m_xGemUI = new XGem_UI();
        void InitXGem()
        {
            if (p_bUseXGem == false) return;
            m_xGem = new XGem();
            m_xGem.Init("XGem", this);
            m_xGemUI.Init(m_xGem);
            m_toolBox.AddToolSet(m_xGem, m_xGemUI);
        }

        void RunTreeXGem(Tree tree)
        {
            p_bUseXGem = tree.Set(p_bUseXGem, p_bUseXGem, "Use", "Use XGem");
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
            RunTreeControl(m_treeRoot.GetTree("Control"));
            RunTreeXGem(m_treeRoot.GetTree("XGem"));
        }
        #endregion

        public AxisMapping_Handler m_handler = new AxisMapping_Handler();
        public void Init(string id)
        {
            EQ.m_sModel = id;
            LogView.Init();
            InitTree();
            InitControl();
            m_login.Init();
            m_toolBox.Init(id, this);
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
