using System.Collections.Generic;

namespace RootTools.SQLogs
{
    public class SQTableBase
    {
        public string p_sCheckTable
        {
            get { return "SELECT COUNT(*) FROM sqlite_master WHERE Name='" + m_sTable + "'"; }
        }

        public string m_sTable = "";
        protected List<string> p_asCreateTable { get; set; }

        public string p_sCreateTable 
        { 
            get 
            {
                string sCreate = "CREATE TABLE " + m_sTable + " (";
                foreach (string sColume in p_asCreateTable) sCreate += sColume;
                sCreate += ");";
                return sCreate;
            }
        }

        public virtual string p_sInsertTable { get { return ""; } }

        public string Insert()
        {
            return m_sqLog.Insert(this); 
        }

        protected _SQLog m_sqLog;
    }
}
