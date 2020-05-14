using RootTools;
using RootTools.Trees;

namespace Root
{
    public class TreeDemo
    {
        public TreeRoot m_treeRoot; 

        string m_id;
        Log m_log; 
        public void Init(string id)
        {
            m_id = id;
            m_log = LogViewer.GetLog(id); 
            m_treeRoot = new TreeRoot("TreeDemo", m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;

            RunTree(Tree.eMode.RegRead);
            
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeSimple(m_treeRoot.GetTree("Simple"));
            RunTreeList(m_treeRoot.GetTree("List"));
            RunTreeGroup(m_treeRoot.GetTree("Group"));
        }

        #region Tree Simple
        bool m_bool = false; 
        int m_int = 0;
        long m_long = 0;
        double m_double = 0;
        string m_string = "";
        CPoint m_cp = new CPoint();
        RPoint m_rp = new RPoint();
        void RunTreeSimple(Tree tree)
        {
            m_bool = tree.Set(m_bool, false, "bool", "Simple bool"); 
            m_int = tree.Set(m_int, 0, "int", "Simple Integer", m_bool);
            m_long = tree.Set(m_long, 0, "long", "Simple long", m_int != 0);
            m_double = tree.Set(m_double, 0, "double", "Simple double");
            m_string = tree.Set(m_string, m_string, "string", "Simple string");
            m_cp = tree.Set(m_cp, m_cp, "CPoint", "Simple CPoint");
            m_rp = tree.Set(m_rp, m_rp, "RPoint", "Simple RPoint");
        }
        #endregion

        #region Tree List
        enum eAnimal
        {
            Dog,
            Cat,
            Horse,
            Bird
        }
        eAnimal m_eAnimal = eAnimal.Dog; 
        string[] m_aNumber = { "One", "Two", "Three", "Four" };
        string m_sFile = ""; 
        void RunTreeList(Tree tree)
        {
            m_eAnimal = (eAnimal)tree.Set(m_eAnimal, eAnimal.Cat, "Enum", "Enum List"); 
            //m_sNumber = tree.Set(m_sNumber, "One", m_aNumber, "string", "String List");
            m_sFile = tree.SetFile(m_sFile, m_sFile, "txt", "File", "Select File"); 
        }
        #endregion

       
        #region Groups
        string m_sGroupA = "A";
        void RunTreeGroup(Tree tree)
        {
            Tree treeL1 = tree.GetTree("L1");
            Tree treeL2 = treeL1.GetTree("L2");
            Tree treeL3 = treeL2.GetTree("L3");
            m_sGroupA = treeL3.Set(m_sGroupA, "A", "string", "Multi Sub Group"); 
        }
        #endregion

        #region Job
        public void OpenTree()
        {
            Job job = new Job("d:\\Tree.txt", false, m_log);
            m_treeRoot.m_job = job;
            RunTree(Tree.eMode.JobOpen);
            job.Close();
            RunTree(Tree.eMode.Init);
        }

        public void SaveTree()
        {
            Job job = new Job("d:\\Tree.txt", true, m_log);
            m_treeRoot.m_job = job; 
            RunTree(Tree.eMode.JobSave);
            job.Close();
        }
        #endregion
    }
}
