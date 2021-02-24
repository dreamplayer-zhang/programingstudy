﻿using System.ComponentModel;

namespace RootTools_Vision
{
    public class SettingItem_SetupFrontside : SettingItem
    {
        public SettingItem_SetupFrontside(string[] _treeViewPath): base(_treeViewPath)
        {
        
        }

        [Category("Klarf")]
        [DisplayName("Use Klarf")]
        public bool UseKlarf
        {
            get => this.useKlarf;
            set => this.useKlarf = value;
        }
        private bool useKlarf = true;

        [Category("Klarf")]
        [DisplayName("Klarf Save Path")]
        public string KlarfSavePath
        {
            get
            {
                return klarfSavePath;
            }
            set
            {
                klarfSavePath = value;
            }
        }
        private string klarfSavePath = "D:\\Klarf";

        [Category("Common")]
        [DisplayName("Defect Image Path")]
        public string DefectImagePath
        {
            get
            {
                return defectImagePath;
            }
            set
            {
                defectImagePath = value;
            }
        }
        private string defectImagePath = "D:\\DefectImage";

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