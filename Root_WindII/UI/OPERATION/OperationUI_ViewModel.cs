using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WindII
{
    public class OperationUI_ViewModel : ObservableObject
    {
        public string ttt = "test";
        public string test
        {
            get
            {
                return ttt;
            }
            set
            {
                SetProperty(ref ttt, value);
            }
        }
    }
}
