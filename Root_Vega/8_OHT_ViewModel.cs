using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_Vega
{
    class _8_OHT_ViewModel : ObservableObject
    {
        Vega_Engineer m_Engineer;



        public _8_OHT_ViewModel(Vega_Engineer engineer, IDialogService service)
        {
            m_Engineer = engineer;

        }
    }
}
