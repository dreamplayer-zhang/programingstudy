﻿using System.Windows.Controls;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    class Home_ViewModel : ObservableObject
    {
        public Setup_ViewModel m_Setup;
        public HomePanel Main;

        public Home_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            Init();
        }
        private void Init()
        {
            Main = new HomePanel();

            //SetPage(Main);
        }

        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
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
