using RootTools.Module;
using System.Collections.Generic;

namespace RootTools.GAFs
{
    public class GAF
    {
        #region GAF Group
        public class Group
        {
            int c_lGroup = 1000;
            public string m_sGroup;
            int m_nID = -1;
            public int p_nID
            {
                get { return c_lGroup * m_nID; }
            }

            public Group(string sGroup, int nID)
            {
                m_sGroup = sGroup;
                m_nID = nID;
            }

            #region CEID
            public List<CEID> m_aCEID = new List<CEID>();
            public int GetNextCEID()
            {
                for (int n = 0; n < c_lGroup; n++)
                {
                    if (IsUseCEID(p_nID + n) == false) return p_nID + n;
                }
                return -1;
            }

            bool IsUseCEID(int nID)
            {
                foreach (CEID ceid in m_aCEID)
                {
                    if (ceid.p_nID == nID) return true;
                }
                return false;
            }

            public CEID GetCEID(string id)
            {
                foreach (CEID ceid in m_aCEID)
                {
                    if (ceid.p_id == id) return ceid;
                }
                return null;
            }
            #endregion

            #region SVID
            public List<SVID> m_aSVID = new List<SVID>();
            public int GetNextSVID()
            {
                for (int n = 0; n < c_lGroup; n++)
                {
                    if (IsUseSVID(p_nID + n) == false) return p_nID + n;
                }
                return -1;
            }

            bool IsUseSVID(int nID)
            {
                foreach (SVID svid in m_aSVID)
                {
                    if (svid.p_nID == nID) return true;
                }
                return false;
            }

            public SVID GetSVID(string id)
            {
                foreach (SVID svid in m_aSVID)
                {
                    if (svid.p_id == id) return svid;
                }
                return null;
            }
            #endregion

            #region ALID
            public List<ALID> m_aALID = new List<ALID>();
            public int GetNextALID()
            {
                for (int n = 0; n < c_lGroup; n++)
                {
                    if (IsUseALID(p_nID + n) == false) return p_nID + n;
                }
                return -1;
            }

            bool IsUseALID(int nID)
            {
                foreach (ALID alid in m_aALID)
                {
                    if (alid.p_nID == nID) return true;
                }
                return false;
            }

            public ALID GetALID(string id)
            {
                foreach (ALID alid in m_aALID)
                {
                    if (alid.p_id == id) return alid;
                }
                return null;
            }
            #endregion
        }
        public List<Group> m_aGroup = new List<Group>();
        Group GetGroup(string sGroup)
        {
            foreach (Group group in m_aGroup)
            {
                if (group.m_sGroup == sGroup) return group;
            }
            Group newGroup = new Group(sGroup, m_aGroup.Count + 1);
            m_aGroup.Add(newGroup);
            return newGroup;
        }
        #endregion

        #region CEID
        public CEIDList m_listCEID = new CEIDList();
        public CEID GetCEID(ModuleBase module, string id)
        {
            Group group = GetGroup(module.p_id);
            CEID ceid = group.GetCEID(id);
            if (ceid != null) return ceid;
            ceid = new CEID(module, id);
            group.m_aCEID.Add(ceid);
            m_listCEID.p_aCEID.Add(ceid);
            return ceid;
        }
        #endregion

        #region SVID
        public SVIDList m_listSVID = new SVIDList();
        public SVID GetSVID(ModuleBase module, string id)
        {
            Group group = GetGroup(module.p_id);
            SVID svid = group.GetSVID(id);
            if (svid != null) return svid;
            svid = new SVID(module, id);
            group.m_aSVID.Add(svid);
            m_listSVID.p_aSVID.Add(svid);
            return svid;
        }
        #endregion

        #region ALID
        public ALIDList m_listALID = new ALIDList();
        public ALID GetALID(ModuleBase module, string id, string sDesc)
        {
            Group group = GetGroup(module.p_id);
            ALID alid = group.GetALID(id);
            if (alid != null) return alid;
            alid = new ALID(module, m_listALID, id, sDesc);
            group.m_aALID.Add(alid);
            m_listALID.p_aALID.Add(alid);
            return alid;
        }

        public void ClearALID()
        {
            m_listALID.ClearALID();
        }
        #endregion

        string m_id;
        public Log m_log;
        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_log = LogViewer.GetLog(id, "GAF");
            m_listCEID.Init(id + ".CEID", this);
            m_listSVID.Init(id + ".SVID", this);
            m_listALID.Init(id + ".ALID", this);
        }

        public void ThreadStop()
        {
            m_listCEID.ThreadStop();
            m_listSVID.ThreadStop();
            m_listALID.ThreadStop();
        }
    }
}
