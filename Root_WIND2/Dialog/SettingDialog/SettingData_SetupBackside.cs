﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    class SettingData_SetupBackside : SettingData
    {
        public SettingData_SetupBackside(string[] _treeViewPath) : base(_treeViewPath)
        {
            
        }

        private bool boolTest;

        public bool BoolTest
        {
            get { return boolTest; }
            set { boolTest = value; }
        }
    }
}
