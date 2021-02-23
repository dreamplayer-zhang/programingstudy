using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    class SettingItem_SetupFrontside : SettingItem
    {
        public SettingItem_SetupFrontside(string[] _treeViewPath): base(_treeViewPath)
        {
        
        }
        private string path = "D:\\";

        [EditorAttribute(typeof(System.Windows.Forms.Design.FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
