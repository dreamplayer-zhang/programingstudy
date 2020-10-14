using Root_ASIS.Teachs;
using RootTools;
using RootTools.Trees;

namespace Root_ASIS
{
    public class MainTeach
    {
        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(m_id, m_log);
            RunTree(Tree.eMode.RegRead);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode eMode)
        {
            m_treeRoot.p_eMode = eMode;
            RunTreeArray(m_treeRoot.GetTree("Array")); 
        }

        public void RunTreeArray(Tree tree)
        {
            Strip.p_szBlock = tree.Set(Strip.p_szBlock, Strip.p_szBlock, "Block", "Number of Block");
            Strip.p_szUnit = tree.Set(Strip.p_szUnit, Strip.p_szUnit, "Unit", "Number of Unit");
            Strip.p_eUnitOrder = (Strip.eUnitOrder)tree.Set(Strip.p_eUnitOrder, Strip.p_eUnitOrder, "Order", "Unit Numbering Order");
        }
        #endregion

        string m_id; 
        ASIS_Engineer m_engineer;
        Log m_log; 
        Teach[] m_aTeach = new Teach[2];
        public MainTeach(string id, ASIS_Engineer engineer)
        {
            m_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id); 
            ASIS_Handler handler = (ASIS_Handler)m_engineer.ClassHandler();
            m_aTeach[0] = handler.m_aBoat[Module.Boat.eBoat.Boat0].m_teach; 
            m_aTeach[1] = handler.m_aBoat[Module.Boat.eBoat.Boat1].m_teach;
            InitTree(); 
        }
    }
}
