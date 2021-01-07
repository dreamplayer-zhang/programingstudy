using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_AOP01_Inspection
{
    class Run_ViewModel : ObservableObject
    {
        MainWindow m_Mainwindow;
        public Run_ViewModel(MainWindow main)
        {
            m_Mainwindow = main;
        }

        public ICommand cmdStart
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Dlg_Start dlg_Start = new Dlg_Start();
                    dlg_Start.ShowDialog();
                });
            }
        }
    }
}
