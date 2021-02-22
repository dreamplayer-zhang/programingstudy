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
        private string path = "D:\\";
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
            }
        }

        private string path2 = "C:\\";
        public string Path2
        {
            get
            {
                return path2;
            }
            set
            {
                path2 = value;
            }
        }
    }
}
