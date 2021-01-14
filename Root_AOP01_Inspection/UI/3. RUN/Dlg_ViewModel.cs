using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_AOP01_Inspection
{
    class Dlg_ViewModel : ObservableObject
    {
        MainWindow m_Mainwindow;
        public Dlg_ViewModel(MainWindow main)
        {
            m_Mainwindow = main;
        }
    }
}
