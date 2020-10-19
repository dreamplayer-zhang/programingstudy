using RootTools;
using RootTools.Trees;
using System.Collections.Generic;

namespace Root_TactTime
{
    public class TactTime : NotifyProperty
    {
        #region Property
        double _secRun = 0;
        public double p_secRun
        {
            get { return _secRun; }
            set
            {
                _secRun = value;
                OnPropertyChanged();
                foreach (Module module in m_aModule) module.OnPropertyChanged("p_fProgress"); 
            }
        }
        #endregion 

        #region Module
        public List<Module> m_aModule = new List<Module>();
        #endregion

        #region Sequence
        public void ClearSequence()
        {
            foreach (Module module in m_aModule) module.p_sStrip = "";
            p_secRun = 0;
            m_iStrip = 0; 
        }
        #endregion

        #region Pine2
        void InitPine2()
        {
            m_aModule.Add(new Module(this, "MGZ Load", Module.eType.Module, 2, new CPoint(50, 100), true));
            m_aModule.Add(new Module(this, "MGZ Unoad", Module.eType.Module, 2, new CPoint(50, 200)));
            m_aModule.Add(new Module(this, "Turnover", Module.eType.Module, 4, new CPoint(50, 350)));
            m_aModule.Add(new Module(this, "Picker0", Module.eType.Picker, 1, new CPoint(250, 200)));
            m_aModule.Add(new Module(this, "Picker1", Module.eType.Picker, 1, new CPoint(250, 300)));
            m_aModule.Add(new Module(this, "Boat0", Module.eType.Module, 12, new CPoint(450, 100)));
            m_aModule.Add(new Module(this, "Boat1", Module.eType.Module, 12, new CPoint(450, 200)));
            m_aModule.Add(new Module(this, "Boat2", Module.eType.Module, 12, new CPoint(450, 300)));
            m_aModule.Add(new Module(this, "Boat3", Module.eType.Module, 12, new CPoint(450, 400)));
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot("TactTime", null);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);

        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode eMode)
        {
            m_treeRoot.p_eMode = eMode;
            RunTreePicker(m_treeRoot.GetTree("Picker")); 
            RunTreeRunTime(m_treeRoot.GetTree("ModuleRun")); 
        }

        void RunTreePicker(Tree tree)
        {
            Module.m_secPickerGet = tree.Set(Module.m_secPickerGet, Module.m_secPickerGet, "Get", "Picker Get Time (sec)");
            Module.m_secPickerPut = tree.Set(Module.m_secPickerPut, Module.m_secPickerPut, "Put", "Picker Put Time (sec)");
        }

        void RunTreeRunTime(Tree tree)
        {
            foreach (Module module in m_aModule)
            {
                module.m_secRun[1] = tree.Set(module.m_secRun[1], module.m_secRun[1], module.p_id, "RunTime (sec)"); 
            }
        }
        #endregion

        public int m_iStrip = 0; 

        public TactTime()
        {
            p_secRun = 0; 
            InitPine2();

            InitTree();
        }
    }
}
