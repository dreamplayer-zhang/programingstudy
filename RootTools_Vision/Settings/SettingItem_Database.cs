using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class SettingItem_Database : SettingItem
    {
        public SettingItem_Database(string[] _treeViewPath) : base(_treeViewPath)
        {

        }

        [Category("Database")]
        [DisplayName("Server Name")]
        public string SerevrName
        {
            get
            {
                return serverName;
            }
            set
            {
                serverName = value;
            }
        }
        private string serverName = "localhost";

        [Category("Database")]
        [DisplayName("Database Name")]
        public string DBName
        {
            get
            {
                return dbName;
            }
            set
            {
                dbName = value;
            }
        }
        private string dbName = "wind2";

        [Category("Database")]
        [DisplayName("User ID")]
        public string DBUserID
        {
            get
            {
                return dbUserID;
            }
            set
            {
                dbUserID = value;
            }
        }
        private string dbUserID = "root";

        [Category("Database")]
        [DisplayName("Password")]
        public string DBPassword
        {
            get
            {
                return dbPassword;
            }
            set
            {
                dbPassword = value;
            }
        }
        private string dbPassword = "root";
    }
}
