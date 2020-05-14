using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace RootTools.RTC5s
{
    public class RTC5Design
    {
        #region List Design
        public enum eDesign
        {
            Line,
            Rectangle,
            Circle,
            DXF,
            DataMatrix,
            QRcode,
            String,
            StringDot,
            StringLine,
        }
        DesignBase New(eDesign design)
        {
            DesignBase designBase = null;
            switch (design)
            {
                case eDesign.Line: designBase = new Design_Line(); break;
                case eDesign.Rectangle: designBase = new Design_Rectangle(); break;
                case eDesign.Circle: designBase = new Design_Circle(); break;
                case eDesign.DXF: designBase = new Design_DXF(); break;
                case eDesign.DataMatrix: designBase = new Design_DataMatrix(); break;
                case eDesign.QRcode: designBase = new Design_QRcode(); break;
                case eDesign.String: designBase = new Design_String(); break;
                case eDesign.StringDot: designBase = new Design_StringDot(); break;
                case eDesign.StringLine: designBase = new Design_StringLine(); break;
            }
            if (designBase != null) designBase.m_eDesign = design;
            return designBase;
        }

        eDesign GetDesign(string sDesign)
        {
            foreach (eDesign design in Enum.GetValues(typeof(eDesign)))
            {
                if (design.ToString() == sDesign) return design;
            }
            return eDesign.Circle;
        }
        #endregion 

        #region List
        /// <summary> m_aDesign : RTC5에 등록된 Design들의 이름 List </summary>
        public List<string> p_asDesign
        {
            get
            {
                List<string> asDesign = new List<string>();
                foreach (DesignBase design in m_aDesign)
                {
                    if (design != null) asDesign.Add(design.m_id);
                }
                return asDesign;
            }
        }
        /// <summary> m_aDesign : RTC5에 등록된 Design들의 List </summary>
        public List<DesignBase> m_aDesign = new List<DesignBase>();
        public void Add(eDesign design)
        {
            int nIndex = GetNewIndex();
            DesignBase designBase = New(design);
            if (designBase == null) return;
            designBase.Init(nIndex, m_rtc5);
            m_aDesign[nIndex] = designBase;
            RunTree(Tree.eMode.Init);
        }

        int GetNewIndex()
        {
            for (int n = 0; n < m_aDesign.Count; n++)
            {
                if (m_aDesign[n] == null) return n;
            }
            m_aDesign.Add(null);
            return m_aDesign.Count - 1;
        }

        public DesignBase Get(string sDesign)
        {
            foreach (DesignBase design in m_aDesign)
            {
                if (design.m_id == sDesign) return design;
            }
            return null;
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
            m_treeRoot.p_eMode = mode;
            foreach (DesignBase design in m_aDesign)
            {
                if (design != null) design.RunTree(m_treeRoot.GetTree(design.m_id, false));
            }
        }
        #endregion

        #region File Open Save
        public string m_sExt;
        public void FileSave(string sFile)
        {
            Job job = null;
            try
            {
                job = new Job(GetFileName(sFile), true, m_log);
                if (job == null) return;
                job.Set(m_id, "Count", m_aDesign.Count);
                for (int n = 0; n < m_aDesign.Count; n++)
                {
                    string sKey = n.ToString("00");
                    DesignBase design = m_aDesign[n];
                    job.Set("Design", sKey, design.m_eDesign);
                }
                m_treeRoot.m_job = job;
                RunTree(Tree.eMode.JobSave);
            }
            finally
            {
                if (job != null) job.Close();
            }
        }

        public void FileOpen(string sFile)
        {
            m_aDesign.Clear();
            eDesign designDef = eDesign.Circle;
            Job job = null;
            if (sFile == "") return;
            try
            {
                job = new Job(GetFileName(sFile), false, m_log);
                int lCount = job.Set(m_id, "Count", m_aDesign.Count);
                for (int n = 0; n < lCount; n++)
                {
                    string sKey = n.ToString("00");
                    string sDesign = job.Set("Design", sKey, designDef);
                    Add(GetDesign(sDesign));
                }
                m_treeRoot.m_job = job;
                RunTree(Tree.eMode.JobOpen);
                RunTree(Tree.eMode.Init);
                foreach (DesignBase design in m_aDesign) design.MakeData();
            }
            finally
            {
                if (job != null) job.Close();
            }
        }

        string GetFileName(string sFile)
        {
            string[] sFiles = sFile.Split('.');
            int nExt = sFiles[sFiles.Length - 1].Length;
            return sFile.Substring(0, sFile.Length - nExt) + m_sExt;
        }
        #endregion

        string m_id;
        Log m_log;
        public TreeRoot m_treeRoot;
        RTC5 m_rtc5;
        public RTC5Design(string id, RTC5 rtc5)
        {
            m_id = id;
            string[] sIDs = id.Split('.');
            m_sExt = sIDs[0] + "_RTC5";
            m_log = rtc5.m_log;
            m_rtc5 = rtc5;
            m_treeRoot = new TreeRoot(m_id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }
    }
}
