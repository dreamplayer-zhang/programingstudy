using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_AOP01
{
    class RecipeOption_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;
        public RecipeOption_ViewModel(Setup_ViewModel setup)
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
