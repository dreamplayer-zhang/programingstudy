using RootTools.GAFs;
using System.Collections.Generic;

namespace RootTools.SQLogs
{
    public class SQTable_ALID : SQTableBase
    {
        #region Create Table
        static List<string> m_asCreateTable = new List<string>()
        {
            { "Time TIMESTAMP, " },
            { "Module VARCHAR(32), " },
            { "Name VARCHAR(32), " },
            { "ALID INTEGER, " },
            { "Error VARCHAR(5), " },
            { "Message VARCHAR(320)" }
        };
        #endregion

        public override string p_sInsertTable
        {
            get
            {
                if (m_alid == null) return ""; 
                string sInsert = "INSERT INTO " + m_sTable + " VALUES (";
                sInsert += "datetime('now', 'localtime'), ";
                sInsert += "'" + m_alid.p_sModule + "', ";
                sInsert += "'" + m_alid.p_id + "', ";
                sInsert += m_alid.p_nID.ToString() + ", ";
                sInsert += "'" + m_alid.p_bSet.ToString() + "', ";
                sInsert += "'" + m_alid.p_sMsg + "');";
                return sInsert; 
            }
        }

        ALID m_alid; 
        public SQTable_ALID(_SQLog sqLog, ALID alid)
        {
            m_sTable = "ALID";
            m_sqLog = sqLog;
            m_alid = alid;
            p_asCreateTable = m_asCreateTable;
        }
    }
}
