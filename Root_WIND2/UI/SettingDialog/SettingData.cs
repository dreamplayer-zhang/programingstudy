using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    public abstract class SettingData : ISettingData
    {
        public SettingData(string[] _treeViewPath)
        {
            treeViewPath = _treeViewPath;
        }

        protected readonly string[] treeViewPath;
        public string[] GetTreeViewPath()
        {
            return this.treeViewPath;
        }

    }
}
