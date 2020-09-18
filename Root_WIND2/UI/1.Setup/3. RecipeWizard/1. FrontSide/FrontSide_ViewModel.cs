using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2
{
    class FrontSide_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;

        public FrontSidePanel Main;
        public FrontSummaryPage Summary;

        public FrontSide_ViewModel(Setup_ViewModel setup)
        {
            init();
            m_Setup = setup;          
        }
        public void init()
        {
            Main = new FrontSidePanel();
            Summary = new FrontSummaryPage();
        }
        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        public ICommand btnFrontSummary
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(Summary);
                });
            }
        }
        public ICommand btnFrontAlignment
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.SetFrontAlignment();
                });
            }
        }
        public ICommand btnFrontGeneral
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.SetFrontGeneral();
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
