using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_WIND2
{
    class GEM_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;
        public GEMPanel Main;

        public GEM_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            init();

        }
        private void init()
        {
            Main = new GEMPanel();
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
