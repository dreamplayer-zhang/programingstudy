using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WindII
{
    public class OperationUI_ViewModel : ObservableObject
    {
        public void EQHome()
        {
            EQ.p_bStop = false;
            EQ.p_eState = EQ.eState.Home;
        }

        public RelayCommand CommandHome
        {
            get
            {
                return new RelayCommand(EQHome);
            }
        }
    }
}
