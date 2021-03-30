using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2.UI_User
{
    public class ModuleView_ViewModel : ObservableObject
    {
        public string ModuleName
        {
            get;
            set;
        }

        public bool IsChecked
        {
            get;
            set;
        }
    }
}
