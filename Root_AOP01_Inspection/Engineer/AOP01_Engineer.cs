using Root_AOP01_Inspection.Module;
using RootTools;
using RootTools.Control;
using RootTools.Control.Ajin;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Gem.XGem;
using RootTools.Memory;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System.Collections.ObjectModel;

namespace Root_AOP01_Inspection
{
    public class AOP01_Engineer : NotifyProperty, IEngineer
    {
        #region IEngineer
        public Login m_login = new Login();
        public Login.User p_user { get { return m_login.p_user; } }

        public IControl ClassControl()
        {
            switch (m_eControl)
            {
                case eControl.Ajin: return m_ajin;
            }
            return m_ajin;
        }

        public GAF m_gaf = new GAF();
        public GAF ClassGAF() { return m_gaf; }

        public IGem ClassGem() { return m_xGem; }

        public IHandler ClassHandler() { return m_handler; }

        public MemoryTool ClassMemoryTool() { return m_toolBox.m_memoryTool; }

        public ModuleList ClassModuleList() { return m_handler.p_moduleList; }

        public ToolBox m_toolBox = new ToolBox();
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
        //bool m_bUseXGem = true;
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
        XGem_New m_xGem = null;
        XGem_New_UI m_xGemUI = new XGem_New_UI();
        void InitXGem()
        {
            if (p_bUseXGem == false) return;
            m_xGem = new XGem_New();
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
            m_handler.RunTreeModule(m_treeRoot.GetTree("Module")); 
        }
        #endregion

        #region IEngineer Function
        public string BuzzerOff()
        {
            m_handler.m_aop01.BuzzerOff();
            return "OK";
        }

        public string Recovery()
        {
            if (IsEnableRecovery() == false) return "Recovery is impossible";
            m_handler.m_bIsPossible_Recovery = false;
            m_handler.m_process.CalcRecover();
            EQ.p_eState = EQ.eState.Run;
            return "OK";
        }
        bool IsEnableRecovery()
        {
            if (IsRunModule()) return false;
            if (IsErrorModule()) return false;
            if (m_handler.m_bIsPossible_Recovery == false) return false;
            // Daniel check
            if (EQ.p_eState != EQ.eState.Ready && EQ.p_eState != EQ.eState.Idle) return false;
            if (EQ.p_bStop == true) return false;
            return m_handler.IsEnableRecovery();
        }
        bool IsRunModule()
        {
            if (IsRunModule((ModuleBase)m_handler.m_aLoadport[0]) || IsRunModule((ModuleBase)m_handler.m_aLoadport[1]) || IsRunModule(m_handler.m_wtr) || IsRunModule(m_handler.m_mainVision) || IsRunModule(m_handler.m_backsideVision))
                return true;
            return false;
        }
        bool IsRunModule(ModuleBase module)
        {
            if (module.p_eState == ModuleBase.eState.Run) return true;
            if (module.p_eState == ModuleBase.eState.Home) return true;
            if (module.p_eState == ModuleBase.eState.Error) return false;
            return (module.m_qModuleRun.Count > 0);
        }
        bool IsErrorModule()
        {
            if (IsErrorModule((ModuleBase)m_handler.m_aLoadport[0]) || IsErrorModule((ModuleBase)m_handler.m_aLoadport[1]) || IsErrorModule(m_handler.m_wtr) || IsErrorModule(m_handler.m_mainVision) || IsErrorModule(m_handler.m_backsideVision))
                return true;
            else
                return false;
        }
        bool IsErrorModule(ModuleBase module)
        {
            if (module.p_eState == ModuleBase.eState.Error)
                return true;
            else
                return false;
        }
        #endregion

        public AOP01_Handler m_handler = new AOP01_Handler();

        public void Init(string id)
        {
            EQ.m_sModel = id;
            LogView.Init();
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
            m_ajin.ThreadStop();
            LogView.ThreadStop();
        }
    }
}
