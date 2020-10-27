using System.Windows.Input;

namespace Root_AOP01
{
    class Maintenance_ViewModel : ObservableObject
    {
        public Maintenance_Panel Maintenance = new Maintenance_Panel();

        Setup_ViewModel m_Setup;
        public Maintenance_ViewModel(Setup_ViewModel setup)
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
