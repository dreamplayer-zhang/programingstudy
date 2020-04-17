using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_Vega
{
    class _8_Optic_ViewModel : ObservableObject
    {
        Vega_Engineer m_Engineer;

        _8_1_Optic_MainVisionViewModel m_MainVisionViewModel;
        public _8_1_Optic_MainVisionViewModel p_MainVisionViewModel
        {
            get
            {
                return m_MainVisionViewModel;
            }
            set
            {
                SetProperty(ref m_MainVisionViewModel, value);
            }
        }
        _8_2_Optic_SideVisionViewModel m_SideVisionViewModel;
        public _8_2_Optic_SideVisionViewModel p_SideVisionViewModel
        {
            get
            {
                return m_SideVisionViewModel;
            }
            set
            {
                SetProperty(ref m_SideVisionViewModel, value);
            }
        }

        public _8_Optic_ViewModel(Vega_Engineer engineer, IDialogService service)
        {
            m_Engineer = engineer;
            p_MainVisionViewModel = new _8_1_Optic_MainVisionViewModel(engineer);
            p_SideVisionViewModel = new _8_2_Optic_SideVisionViewModel(engineer, service);
        }
    }
}
