using Root_VEGA_D.Engineer;
using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_D
{
    class Run_ViewModel : ObservableObject
    {
        public MainWindow_ViewModel p_main { get; set; }
        public VEGA_D_Handler m_handler { get; set; }
        public Run_ViewModel()
        {
            m_handler = (VEGA_D_Handler)App.m_engineer.ClassHandler();
        }

        public Run_ViewModel(MainWindow_ViewModel main)
        {
            p_main = main;
        }
    }
}
