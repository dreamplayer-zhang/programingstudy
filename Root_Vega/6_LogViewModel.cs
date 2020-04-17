using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_Vega
{
    class _6_LogViewModel : ObservableObject
    {
        Vega_Engineer m_Engineer;
        LogView m_LogView;
        public LogView p_LogView
        {
            get
            {
                return m_LogView;
            }
            set
            {
                SetProperty(ref m_LogView, value);
            }
        }

        public _6_LogViewModel(Vega_Engineer engineer)
        {
            m_Engineer = engineer;
            p_LogView = m_Engineer.ClassLogView();
        }
    }
}
