using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_AOP01_Inspection
{
    class GEM_ViewModel : ObservableObject
    {
        public GEM_Panel GEM = new GEM_Panel();

        Setup_ViewModel m_Setup;
        public GEM_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
        }
        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_HomePanel();
                });
            }
        }
    }
}
