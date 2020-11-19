using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_AOP01_Packing
{
    class SetupHome_ViewModel : ObservableObject
    {
        public Home_Panel Home = new Home_Panel();
        public Engineer_Page Engineer = new Engineer_Page();
        public GEM_page GEM = new GEM_page();

        Setup_ViewModel m_Setup;
        public SetupHome_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
        }

        public ICommand btnEngineer
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_EngineerPage();
                });
            }
        }
        public ICommand btnGEM
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_GEMPage();
                });
            }
        }

    }
}
