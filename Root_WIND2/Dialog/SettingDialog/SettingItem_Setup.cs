using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    class SettingItem_Setup : SettingItem
    {
        public enum TestEnum
        {
            One,
            Two,
            Three
        }

        private TestEnum test;
        public TestEnum Test
        {
            get
            {
                return this.test;
            }
            set
            {
                this.test = value;
            }
        }

        public SettingItem_Setup(string[] _treeViewPath) : base(_treeViewPath)
        {
            
        }
    }
}
