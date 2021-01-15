using RootTools.Trees;
using System;
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
            get
            {
                return m_strSelectedID;
            }
            set
            {
                SetProperty(ref m_strSelectedID, value);
            }
        }

        #region ILightTool
        public List<string> m_asLightTool = new List<string>();
        public List<string> p_asLightTool
        {
            get
            {
                return m_asLightTool;
            }
            set
            {
                SetProperty(ref m_asLightTool, value);
            }
        }

        public ObservableCollection<ILightTool> m_aLightTool = new ObservableCollection<ILightTool>();
        public ObservableCollection<ILightTool> p_aLightTool
        {
            get
            {
                return m_aLightTool;
            }
            set
            {
                SetProperty(ref m_aLightTool, value);
            }
        }

        void AddTool(ILightTool lightTool)
        {
            if (lightTool == null)
                return;
            p_aLightTool.Add(lightTool);
            p_asLightTool.Add(lightTool.p_id);
        }

        public ILightTool GetTool(string sID)
        {
            foreach (ILightTool lightTool in p_aLightTool)
            {
                if (sID == lightTool.p_id)
                    return lightTool;
            }
            return null;
        }
        #endregion

        #region Light 12 Channel
        const string c_s12ch = "12ch";
        int m_n12Ch = 0;
        ObservableCollection<LightTool_12ch> m_aLightTool12ch = new ObservableCollection<LightTool_12ch>();
        public ObservableCollection<LightTool_12ch> p_aLightTool12ch
        {
            get
            {
                return m_aLightTool12ch;
            }
            set
            {
                SetProperty(ref m_aLightTool12ch, value);
            }
        }

        bool Run12ChTree(Tree tree)
        {
            m_n12Ch = tree.Set(m_n12Ch, m_n12Ch, "Count", "Light 12 Channel Count (CyUSB)");
            while (m_aLightTool12ch.Count > m_n12Ch)
                p_aLightTool12ch.RemoveAt(m_aLightTool12ch.Count - 1);
            while (m_aLightTool12ch.Count < m_n12Ch)
            {
                LightTool_12ch lightTool = new LightTool_12ch(p_aLightTool12ch.Count, c_s12ch + "." + (char)('A' + p_aLightTool12ch.Count), m_engineer);
                p_aLightTool12ch.Add(lightTool);
            }
            return true;
        }
        #endregion

        #region Light 4 Channel
        const string c_s4ch = "4ch";
        int m_n4Ch = 0;
        ObservableCollection<LightTool_4ch> m_aLightTool4ch = new ObservableCollection<LightTool_4ch>();
        public ObservableCollection<LightTool_4ch> p_aLightTool4ch
        {
            get
            {
                return m_aLightTool4ch;
            }
            set
            {
                SetProperty(ref m_aLightTool4ch, value);
            }
        }

        bool Run4ChTree(Tree tree)
        {
            m_n4Ch = tree.Set(m_n4Ch, m_n4Ch, "Count", "Light 4 Channel Count (RS232)");
            while (p_aLightTool4ch.Count > m_n4Ch)
                p_aLightTool4ch.RemoveAt(m_aLightTool4ch.Count - 1);
            while (p_aLightTool4ch.Count < m_n4Ch)
            {
                LightTool_4ch lightTool = new LightTool_4ch(c_s4ch + "." + (char)('A' + p_aLightTool4ch.Count), m_engineer);
                p_aLightTool4ch.Add(lightTool);
            }
            return true;
        }
        #endregion

        #region Light Kwangwoo Channel
        const string c_sKwangwoo = "Kwangwoo";
        int m_nKwangwoo = 0;
        ObservableCollection<LightTool_Kwangwoo> m_aLightToolKwangwoo = new ObservableCollection<LightTool_Kwangwoo>();
        public ObservableCollection<LightTool_Kwangwoo> p_aLightToolKwangwoo
        {
            get
            {
                return m_aLightToolKwangwoo;
            }
            set
            {
                SetProperty(ref m_aLightToolKwangwoo, value);
            }
        }

        bool RunKwangwooTree(Tree tree)
        {  
            m_nKwangwoo = tree.Set(m_nKwangwoo, m_nKwangwoo, "Count", "Light Kwangwoo Count (RS232)");
            while (p_aLightToolKwangwoo.Count > m_nKwangwoo)
                p_aLightToolKwangwoo.RemoveAt(p_aLightToolKwangwoo.Count - 1);
            while (p_aLightToolKwangwoo.Count < m_nKwangwoo)
            {
                LightTool_Kwangwoo lightTool = new LightTool_Kwangwoo(c_sKwangwoo + "." + (char)('A' + p_aLightToolKwangwoo.Count), m_engineer);
                p_aLightToolKwangwoo.Add(lightTool);
            }
            return true;
        }
        #endregion

        #region Light LVS Channel
        const string c_sLVS = "LVS";
        int m_nLVS = 0;
        ObservableCollection<LightTool_LVS> m_aLightToolLVS = new ObservableCollection<LightTool_LVS>();
        public ObservableCollection<LightTool_LVS> p_aLightToolLVS
        {
            get
            {
                return m_aLightToolLVS;
            }
            set
            {
                SetProperty(ref m_aLightToolLVS, value);
            }
        }

        bool RunLVSTree(Tree tree)
        {
            m_nLVS = tree.Set(m_nLVS, m_nLVS, "Count", "Light LVS Count (RS232)");
            while (p_aLightToolLVS.Count > m_nLVS)
                p_aLightToolLVS.RemoveAt(p_aLightToolLVS.Count - 1);
            while (p_aLightToolLVS.Count < m_nLVS)
            {
                LightTool_LVS lightTool = new LightTool_LVS(c_sLVS + "." + (char)('A' + p_aLightToolLVS.Count), m_engineer);
                p_aLightToolLVS.Add(lightTool);
            }
            return true;
        }
        #endregion

        #region Light DAWOO Channel
        const string c_sDAWOO = "DAWOO";
        int m_nDAWOO = 0;
        ObservableCollection<LightTool_DAWOO> m_aLightToolDAWOO = new ObservableCollection<LightTool_DAWOO>();
        public ObservableCollection<LightTool_DAWOO> p_aLightToolDAWOO
        {
            get
            {
                return m_aLightToolDAWOO;
            }
            set
            {
                SetProperty(ref m_aLightToolDAWOO, value);
            }
        }

        bool RunTreeDAWOO(Tree tree)
        {
            m_nDAWOO = tree.Set(m_nDAWOO, m_nDAWOO, "Count", "Light DAWOO Count");
            while (p_aLightToolDAWOO.Count > m_nDAWOO)
                p_aLightToolDAWOO.RemoveAt(p_aLightToolDAWOO.Count - 1);
            while (p_aLightToolDAWOO.Count < m_nDAWOO)
            {
                LightTool_DAWOO lightTool = new LightTool_DAWOO(c_sDAWOO + "." + (char)('A' + p_aLightToolDAWOO.Count), m_engineer);
                p_aLightToolDAWOO.Add(lightTool);
            }
            return true;
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
            bChange |= RunTreeDAWOO(m_treeRoot.GetTree(c_sDAWOO));
            if (bChange)
            {
                p_asLightTool.Clear();
                p_aLightTool.Clear();
                foreach (LightTool_12ch lightTool in p_aLightTool12ch)
                    AddTool(lightTool);
                foreach (LightTool_4ch lightTool in p_aLightTool4ch)
                    AddTool(lightTool);
                foreach (LightTool_Kwangwoo lightTool in p_aLightToolKwangwoo)
                    AddTool(lightTool);
                foreach (LightTool_LVS lightTool in p_aLightToolLVS)
                    AddTool(lightTool);
                foreach (LightTool_DAWOO lightTool in p_aLightToolDAWOO)
                    AddTool(lightTool);
                if (OnToolChanged != null)
                    OnToolChanged();
            }
        }

        double m_secDifferent = 1;
        void RunSetupTree(Tree tree)
        {
            m_secDifferent = tree.Set(m_secDifferent, 1, "Timeout", "Light Power Set -> Get Timeout (sec)");
        }
        #endregion

        public string p_id
        {
            get;
            set;
        }
        IEngineer m_engineer;
        Log m_log;
        public TreeRoot m_treeRoot;
        public TreeRoot p_treeRoot
        {
            get
            {
                return m_treeRoot;
            }
            set
            {
                SetProperty(ref m_treeRoot, value);
            }
        }
        public LightToolSet(string id, IEngineer engineer)
        {
            p_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id);
            m_treeRoot = new TreeRoot(id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);
        }

        public void ThreadStop()
        {
            foreach (ILightTool iLignt in p_aLightTool) iLignt.ThreadStop(); 
        }

        public void AddContoroller(Type type)
        {
            if (type == typeof(LightTool_12ch)) m_n12Ch++;
            else if (type == typeof(LightTool_4ch)) m_n4Ch++;
            else if (type == typeof(LightTool_Kwangwoo)) m_nKwangwoo++;
            else if (type == typeof(LightTool_LVS)) m_nLVS++;
            else if (type == typeof(LightTool_DAWOO)) m_nDAWOO++;
            RunTree(Tree.eMode.Init);
        }

        public void RemoveContoroller(Type type)
        {
            if (type == typeof(LightTool_12ch))
            {
                if (m_n12Ch == 0)
                    return;
                m_n12Ch--;
            }
            else if (type == typeof(LightTool_4ch))
            {
                if (m_n4Ch == 0)
                    return;
                m_n4Ch--;
            }
            else if (type == typeof(LightTool_Kwangwoo))
            {
                if (m_nKwangwoo == 0)
                    return;
                m_nKwangwoo--;
            }
            else if (type == typeof(LightTool_LVS))
            {
                if (m_nLVS == 0)
                    return;
                m_nLVS--;
            }
            else if (type == typeof(LightTool_DAWOO))
            {
                if (m_nDAWOO == 0)
                    return;
                m_nDAWOO--;
            }
            RunTree(Tree.eMode.Init);
        }


    }
}
