using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2
{
    class Edge_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;

        public Edge_Panel Main;
        public EdgePanel_ViewModel m_EdgePanel_VM;
        public EdgeSetupPage m_SetupPage;
        public Edge_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            init();
        }
        public void init()
        {
            Main = new Edge_Panel();
            m_EdgePanel_VM = new EdgePanel_ViewModel(m_Setup);
            
            m_EdgePanel_VM.Init();

            m_SetupPage = new EdgeSetupPage();
            m_SetupPage.DataContext = m_EdgePanel_VM;
        }
        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        public ICommand btnEdgeSummary
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(m_SetupPage);
                });
            }
        }

        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.SetRecipeWizard();
                });
            }
        }
    }
}
