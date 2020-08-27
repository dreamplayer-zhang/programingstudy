using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_WIND2
{
    class Maintenance_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;
        public MaintenancePanel Main;

        public Maintenance_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            init();
        }
        public void init()
        {
            Main = new MaintenancePanel();
        }

        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.SetHome();
                });
            }
        }
    }
}
