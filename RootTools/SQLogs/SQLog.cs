using RootTools.GAFs;
using RootTools.Trees;
using System;
using System.Data.SQLite;
using System.IO;

namespace RootTools.SQLogs
{
    public static class SQLog
    {
        public static _SQLog m_sqLog;
        public static void Init(IEngineer engineer)
        {
            // System.BadImageFormatException -> 속성 -> 빌드 -> 32 비트 기본 사용 X
            m_sqLog = new _SQLog(engineer);
        }

        public static SQTable_ALID Get(ALID alid)
        {
            return new SQTable_ALID(m_sqLog, alid); 
        }
    }

    public class _SQLog : NotifyProperty, IToolSet
    {
        public string p_id { get; set; }

        string _sInfo = ""; 
        public string p_sInfo
        { 
            get { return _sInfo; }
            set
            {
                if (_sInfo == value) return;
                _sInfo = value;
                OnPropertyChanged(); 
            }
        }

        #region Setting
        SQLiteConnection m_sqConnection = null;

        public string p_sFileDB
        {
            get { return (m_sqConnection == null) ? "" : m_sqConnection.FileName; }
            set
            {
                if (File.Exists(value) == false)
                {
                    SQLiteConnection.CreateFile(value);
                    m_sqConnection = null;
                }
                if (File.Exists(value) == false) return;
                if (m_sqConnection == null)
                {
                    SQLiteConnectionStringBuilder sqCSB = new SQLiteConnectionStringBuilder();
                    sqCSB.DataSource = value;
                    sqCSB.Version = 3;
                    m_sqConnection = new SQLiteConnection(sqCSB.ConnectionString);
                    try { m_sqConnection = m_sqConnection.OpenAndReturn(); }
                    catch (Exception) { m_sqConnection = null; }
                    CreateTables();
                }
            }
        }

        void RunTreeSetting(Tree tree)
        {
            p_sFileDB = tree.SetFile(p_sFileDB, "C:\\Log\\SQLog.sqLite", "sqLite", "File DB", "File DB Name");
        }
        #endregion

        #region Tables
        void CreateTables()
        {
            if (m_sqConnection == null) return;
            p_sInfo = CreateTable(new SQTable_ALID(this, null));
        }

        string CreateTable(SQTableBase table)
        {
            if (IsTableExist(table) == false)
            {
                SQLiteCommand cmdCreate = new SQLiteCommand(table.p_sCreateTable, m_sqConnection);
                if (cmdCreate.ExecuteNonQuery() <= 0) return "Create Table Fail : " + table.p_sCreateTable;
            }
            SQLiteCommand cmdDelete = new SQLiteCommand("Delete FROM " + table.m_sTable + " where Time < datetime('now', '-100 days')", m_sqConnection);
            return (cmdDelete.ExecuteNonQuery() > 0) ? "OK" : "Delete Old Data Fail"; 
        }

        bool IsTableExist(SQTableBase table)
        {
            SQLiteCommand cmdCheck = new SQLiteCommand(table.p_sCheckTable, m_sqConnection);
            return (Convert.ToInt32(cmdCheck.ExecuteScalar()) == 1);
        }
        #endregion

        #region Insert
        public string Insert(SQTableBase table) 
        {
            SQLiteCommand cmd = new SQLiteCommand(table.p_sInsertTable, m_sqConnection);
            return (cmd.ExecuteNonQuery() >= 0) ? "OK" : "Insert Table Fail : " + table.p_sInsertTable;
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot;
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeSetting(m_treeRoot.GetTree("Setting", false));
        }
        #endregion

        Log m_log;
        public _SQLog(IEngineer engineer)
        {
            p_id = "SQLog";
            m_log = LogView.GetLog(p_id, p_id);
            m_treeRoot = new TreeRoot(p_id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);
        }

        public void ThreadStop()
        {
            if (m_sqConnection != null) m_sqConnection.Close();
        }
    }
}
