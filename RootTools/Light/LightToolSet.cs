using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Threading;

namespace RootTools.Light
{
    public class LightToolSet : ObservableObject, IToolSet
    {
        public delegate void dgOnToolChanged();
        public event dgOnToolChanged OnToolChanged;

        string m_strSelectedID;
        public string p_strSelectedID
        {
            get { return m_strSelectedID; }
            set { SetProperty(ref m_strSelectedID, value); }
        }

        #region ILightTool
        public List<string> m_asLightTool = new List<string>(); 
        public List<string> p_asLightTool
        {
            get { return m_asLightTool; }
            set { SetProperty(ref m_asLightTool, value); }
        }

        public ObservableCollection<ILightTool> m_aLightTool = new ObservableCollection<ILightTool>();
        public ObservableCollection<ILightTool> p_aLightTool
        {
            get { return m_aLightTool; }
            set { SetProperty(ref m_aLightTool, value); }
        }

        void AddTool(ILightTool lightTool)
        {
            if (lightTool == null) return; 
            p_aLightTool.Add(lightTool);
            p_asLightTool.Add(lightTool.p_id);
        }

        public ILightTool GetTool(string sID)
        {
            foreach (ILightTool lightTool in p_aLightTool)
            {
                if (sID == lightTool.p_id) return lightTool; 
            }
            return null; 
        }
        #endregion

        #region Light 12 Channel
        const string c_s12ch = "12ch"; 
        int m_n12Ch = 0;
        List<LightTool_12ch> m_aLightTool12ch = new List<LightTool_12ch>(); 
        bool Run12ChTree(Tree tree)
        {
            int n12Ch = m_n12Ch;
            m_n12Ch = tree.Set(m_n12Ch, m_n12Ch, "Count", "Light 12 Channel Count (CyUSB)");
            if (n12Ch == m_n12Ch) return false;
            while (m_aLightTool12ch.Count > m_n12Ch) m_aLightTool12ch.RemoveAt(m_aLightTool12ch.Count - 1);
            while (m_aLightTool12ch.Count < m_n12Ch)
            {
                LightTool_12ch lightTool = new LightTool_12ch(m_aLightTool12ch.Count, c_s12ch + "." + (char)('A' + m_aLightTool12ch.Count), m_engineer);
                m_aLightTool12ch.Add(lightTool);
            }
            return true; 
        }
        #endregion

        #region Light 4 Channel
        const string c_s4ch = "4ch";
        int m_n4Ch = 0;
        List<LightTool_4ch> m_aLightTool4ch = new List<LightTool_4ch>(); 

        bool Run4ChTree(Tree tree)
        {
            int n4Ch = m_n4Ch;
            m_n4Ch = tree.Set(m_n4Ch, m_n4Ch, "Count", "Light 4 Channel Count (RS232)");
            if (n4Ch == m_n4Ch) return false;
            while (m_aLightTool4ch.Count > m_n4Ch) m_aLightTool4ch.RemoveAt(m_aLightTool4ch.Count - 1); 
            while (m_aLightTool4ch.Count < m_n4Ch)
            {
                LightTool_4ch lightTool = new LightTool_4ch(c_s4ch + "." + (char)('A' + m_aLightTool4ch.Count), m_engineer);
                m_aLightTool4ch.Add(lightTool); 
            }
            return true; 
        }
        #endregion

        #region Light Kwangwoo Channel
        const string c_sKwangwoo = "Kwangwoo";
        int m_nKwangwoo = 0;
        List<LightTool_Kwangwoo> m_aLightToolKwangwoo = new List<LightTool_Kwangwoo>();

        bool RunKwangwooTree(Tree tree)
        {
            int nKwangwoo = m_nKwangwoo;
            m_nKwangwoo = tree.Set(m_nKwangwoo, m_nKwangwoo, "Count", "Light Kwangwoo Count (RS232)");
            if (nKwangwoo == m_nKwangwoo) return false;
            while (m_aLightToolKwangwoo.Count > m_nKwangwoo) m_aLightToolKwangwoo.RemoveAt(m_aLightToolKwangwoo.Count - 1);
            while (m_aLightToolKwangwoo.Count < m_nKwangwoo)
            {
                LightTool_Kwangwoo lightTool = new LightTool_Kwangwoo(c_sKwangwoo + "." + (char)('A' + m_aLightToolKwangwoo.Count), m_engineer);
                m_aLightToolKwangwoo.Add(lightTool);
            }
            return true; 
        }
        #endregion

        #region Light LVS Channel
        const string c_sLVS = "LVS";
        int m_nLVS = 0;
        List<LightTool_LVS> m_aLightToolLVS = new List<LightTool_LVS>();

        bool RunLVSTree(Tree tree)
        {
            int nLVS = m_nLVS;
            m_nLVS = tree.Set(m_nLVS, m_nLVS, "Count", "Light LVS Count (RS232)");
            if (nLVS == m_nLVS) return false;
            while (m_aLightToolLVS.Count > m_nLVS) m_aLightToolLVS.RemoveAt(m_aLightToolLVS.Count - 1);
            while (m_aLightToolLVS.Count < m_nLVS)
            {
                LightTool_LVS lightTool = new LightTool_LVS(c_sLVS + "." + (char)('A' + m_aLightToolLVS.Count), m_engineer);
                m_aLightToolLVS.Add(lightTool);
            }
            return true;
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_thread;

        void RunThreadCheck()
        {
            m_bThread = true;
            Thread.Sleep(2000); 
            while (m_bThread)
            {
                Thread.Sleep(10);
                //int nDifferent = (int)(m_secDifferent * 100); 
                //foreach (ILightTool lightTool in m_aLightTool)
                //{
                //    foreach (LightBase light in lightTool.p_aLight)
                //    {
                //        if (light.p_nDifferent > nDifferent)
                //        {
                //            m_log.Warn("Light Power Miss Match : " + light.p_id);
                //            light.p_fSetPower = light.p_fSetPower;
                //            EQ.p_bStop = true; 
                //        }
                //    }
                //}
            }
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
            RunTree(Tree.eMode.Init);
        }

        public void RunTree(Tree.eMode mode)
        {
            bool bChange = false; 
            m_treeRoot.p_eMode = mode;
            RunSetupTree(m_treeRoot.GetTree("Setup")); 
            bChange |= Run12ChTree(m_treeRoot.GetTree(c_s12ch)); 
            bChange |= Run4ChTree(m_treeRoot.GetTree(c_s4ch));
            bChange |= RunKwangwooTree(m_treeRoot.GetTree(c_sKwangwoo));
            bChange |= RunLVSTree(m_treeRoot.GetTree(c_sLVS));
            if (bChange)
            {
                p_asLightTool.Clear();
                p_aLightTool.Clear();
                foreach (LightTool_12ch lightTool in m_aLightTool12ch) AddTool(lightTool); 
                foreach (LightTool_4ch lightTool in m_aLightTool4ch) AddTool(lightTool);
                foreach (LightTool_Kwangwoo lightTool in m_aLightToolKwangwoo) AddTool(lightTool);
                foreach (LightTool_LVS lightTool in m_aLightToolLVS) AddTool(lightTool);
                if (OnToolChanged != null) OnToolChanged();
            }
        }

        double m_secDifferent = 1;
        void RunSetupTree(Tree tree)
        {
            m_secDifferent = tree.Set(m_secDifferent, 1, "Timeout", "Light Power Set -> Get Timeout (sec)"); 
        }
        #endregion

        public string p_id { get { return m_id; } }
        string m_id;
        IEngineer m_engineer;
        Log m_log; 
        public TreeRoot m_treeRoot; 
        public TreeRoot p_treeRoot
        {
            get { return m_treeRoot; }
            set { SetProperty(ref m_treeRoot, value); }
        }
        public LightToolSet(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = engineer; 
            m_log = LogView.GetLog(id); 
            m_treeRoot = new TreeRoot(id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);

            m_thread = new Thread(new ThreadStart(RunThreadCheck));
            m_thread.Start(); 
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Join(); 
            }
        }
    }
}
