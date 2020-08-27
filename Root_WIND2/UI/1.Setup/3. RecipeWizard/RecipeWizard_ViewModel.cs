using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2
{
    class RecipeWizard_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;

        public RecipeWizardPanel Main;
        public RecipeSummaryPage Summary;

        public RecipeWizard_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            init();
        }
        private void init()
        {
            Main = new RecipeWizardPanel();
            Summary = new RecipeSummaryPage();

            SetPage(Summary);
        }

        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        public ICommand btnWizardSummary
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(Summary);
                });
            }
        }
        public ICommand btnWizardPreAlign
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.SetWizardPreAlign();
                });
            }
        }
        public ICommand btnWizardFrontSide
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.SetWizardFrontSide();
                });
            }
        }
        public ICommand btnWizardBackSide
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.SetWizardBackSide();
                });
            }
        }
        public ICommand btnWizardEBR
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.SetWizardEBR();
                });
            }
        }
        public ICommand btnWizardEdge
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.SetWizardEdge();
                });
            }
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
