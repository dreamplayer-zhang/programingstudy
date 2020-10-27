using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_AOP01
{
    class Home_ViewModel : ObservableObject
    {
        public Home_Panel Home = new Home_Panel();

        Setup_ViewModel m_Setup;
        public Home_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;      
        }

        public ICommand btnRecipeWizard
        {
            get
            {
                return new RelayCommand(m_Setup.Set_RecipeWizardPanel);
            }
        }
        public ICommand btnMaintenance
        {
            get
            {
                return new RelayCommand(m_Setup.Set_MaintenancePanel);
            }
        }
        public ICommand btnGEM
        {
            get
            {
                return new RelayCommand(m_Setup.Set_GEMPanel);
            }
        }
    }
}
