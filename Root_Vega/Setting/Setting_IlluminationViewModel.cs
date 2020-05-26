using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_Vega
{
    class Setting_IlluminationViewModel : ObservableObject
    {
        Vega_Engineer m_Engineer;

        public Setting_IlluminationViewModel(Vega_Engineer engineer)
        {
            m_Engineer = engineer;
        }
    }
}
