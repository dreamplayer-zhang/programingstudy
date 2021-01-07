using RootTools;
using RootTools.Gem;
using RootTools.Gem.XGem;
using RootTools.Memory;
using RootTools.Control.Ajin;
using RootTools.Inspects;
using RootTools.GAFs;
using RootTools.ToolBoxs;
using RootTools.Module;
using RootTools.Control;
using RootTools.Trees;

namespace Root_WIND2
{
    public class WIND2_Engineer : IEngineer
    {
        #region IEngineer
        public Login m_login = new Login();
        public Login.User p_user
        {
            get
            {
                return m_login.p_user;
            }
        }

        public IGem ClassGem()
        {
            return m_xGem;
        }

        public IControl ClassControl()
        {
            switch (m_eControl)
            {
                case eControl.Ajin: return m_ajin;
            }
            return m_ajin;
        }

        public GAF m_gaf = new GAF();
        public GAF ClassGAF()
        {
            return m_gaf;
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

        public IHandler ClassHandler()
        {
            return m_handler;
        }

        public ModuleList ClassModuleList()
        {
            return m_handler.p_moduleList;
        }

        public MemoryData GetMemory(string sPool, string sGroup, string sMemory)
        {
            MemoryPool pool = m_toolBox.m_memoryTool.GetPool(sPool);
            return (pool == null) ? null : pool.GetMemory(sGroup, sMemory);
        }
        #endregion

        #region Mode
        public enum eMode
        {
            Vision,
            EFEM
        }
        public eMode m_eMode = eMode.Vision; 

        void RunTreeMode(Tree tree)
        {
            m_eMode = (eMode)tree.Set(m_eMode, m_eMode, "Mode", "Run Mode"); 
        }
        #endregion

        #region Control
        enum eControl
        {
            Ajin
        };
        eControl m_eControl = eControl.Ajin;
        void InitControl()
        {
            switch (m_eControl)
            {
                case eControl.Ajin: InitAjin(); break;
            }
        }

        void RunTreeControl(Tree tree)
        {
            m_eControl = (eControl)tree.Set(m_eControl, m_eControl, "Control", "Select Control");
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
        }
        #endregion

        #region XGem
        bool m_bUseXGem = true;
        XGem m_xGem = null;
        XGem_UI m_xGemUI = new XGem_UI();
        void InitXGem()
        {
            if (m_bUseXGem == false) return;
            m_xGem = new XGem();
            m_xGem.Init("XGem", this);
            m_xGemUI.Init(m_xGem);
            m_toolBox.AddToolSet(m_xGem, m_xGemUI);
        }

        void RunTreeXGem(Tree tree)
        {
            m_bUseXGem = tree.Set(m_bUseXGem, m_bUseXGem, "Use", "Use XGem");
        }
        #endregion

        public WIND2_Handler m_handler = new WIND2_Handler();

        private InspectionManagerFrontside inspectionFront;
        public InspectionManagerFrontside InspectionFront { get => inspectionFront; set => inspectionFront = value; }

        private InspectionManagerBackside inspectionBack;
        public InspectionManagerBackside InspectionBack { get => inspectionBack; set => inspectionBack = value; }


        private InspectionManagerEdge inspectionEFEM;
        public InspectionManagerEdge InspectionEFEM { get => inspectionEFEM; set => inspectionEFEM = value; }

        #region Tree Setup
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
            RunTreeMode(m_treeRoot.GetTree("Mode"));
            RunTreeControl(m_treeRoot.GetTree("Control"));
            RunTreeXGem(m_treeRoot.GetTree("XGem"));
            m_handler.RunTreeModule(m_treeRoot.GetTree("Module"));
        }
        #endregion

        public void Init(string id)
        {
            EQ.m_sModel = id;
            LogView.Init();
            m_handler.m_engineer = this;
            InitTree();
            m_login.Init();
            m_toolBox.Init(id, this);
            InitControl();
            InitXGem();
            m_handler.Init(id, this);
            m_gaf.Init(id, this);
        }

        public void ThreadStop()
        {
            m_gaf.ThreadStop();
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
