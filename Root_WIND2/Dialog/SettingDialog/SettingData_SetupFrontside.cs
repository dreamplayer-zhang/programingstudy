using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    class SettingData_SetupFrontside : SettingData
    {
        public SettingData_SetupFrontside(string[] _treeViewPath): base(_treeViewPath)
        {
        
        }
        private int test;

        public int Test
        {
            get { return test; }
            set { test = value; }
        }
    }
}
