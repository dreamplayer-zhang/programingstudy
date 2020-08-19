using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2
{
    class Home_ViewModel : ObservableObject
    {
        public Setup_ViewModel m_Setup;
        public HomePanel Main;
        public HomeSummaryPage Summary;

        public Home_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            init();
        }
        private void init()
        {
            Main = new HomePanel();
            Summary = new HomeSummaryPage();

            SetPage(Summary);
        }

        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        public ICommand btnSummary
        {
            get
            {
                return new RelayCommand(() => 
                { 
                    SetPage(Summary); 
                });
            }
        }
        public ICommand btnInspection
        {
            get
            {
                return new RelayCommand(m_Setup.SetInspection);
            }
        }
        public ICommand btnRecipeWizard
        {
            get
            {
                return new RelayCommand(m_Setup.SetRecipeWizard);
            }
        }
        public ICommand btnMaintenance
        {
            get
            {
                return new RelayCommand(m_Setup.SetMaintenance);
            }
        }
        public ICommand btnGEM
        {
            get
            {
                return new RelayCommand(m_Setup.SetGEM);
            }
        }
    }
}
