using Root_VEGA_D.Engineer;
using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_D
{
    class Information_ViewModel : ObservableObject
    {
        public VEGA_D_Handler m_handler { get; set; }
        public MainWindow_ViewModel p_main { get; set; }
        public Information_ViewModel(MainWindow_ViewModel main)
        {
            p_main = main;
            m_handler = (VEGA_D_Handler)App.m_engineer.ClassHandler();
        }


        #region Method
        void InitFFU()
        {
            //FanUI0.DataContext = m_handler.m_FFU.p_aUnit[0].p_aFan[0];
            //FanUI1.DataContext = m_handler.m_FFU.p_aUnit[0].p_aFan[1];
            //FanUI2.DataContext = m_handler.m_FFU.p_aUnit[0].p_aFan[2];
            //FanUI3.DataContext = m_handler.m_FFU.p_aUnit[0].p_aFan[3];
            //FDC_CDA1.DataContext = m_handler.m_interlock;
            //FDC_CDA2.DataContext = m_handler.m_interlock;
        }
        #endregion
    }
}
