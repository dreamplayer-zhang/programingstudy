using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2
{
    class Backside_ViewModel : ObservableObject
    {
        public Backside_Panel Main;
        public BacksideSetup Setup;
        public BacksideROI ROI;


        private BacksideSetup_ViewModel m_BacksideSetup_VM;
        public BacksideSetup_ViewModel p_BacksideSetup_VM
        {
            get
            {
                return m_BacksideSetup_VM;
            }
            set
            {
                SetProperty(ref m_BacksideSetup_VM, value);
            }
        }
        private BacksideROI_ViewModel m_BacksideROI_VM;
        public BacksideROI_ViewModel p_BacksideROI_VM
        {
            get
            {
                return m_BacksideROI_VM;
            }
            set
            {
                SetProperty(ref m_BacksideROI_VM, value);
            }
        }


        Setup_ViewModel m_Setup;
        Recipe m_Recipe;
        public Backside_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            m_Recipe = setup.Recipe;

            p_BacksideSetup_VM = new BacksideSetup_ViewModel();

            p_BacksideROI_VM = new BacksideROI_ViewModel();
            p_BacksideROI_VM.init(m_Setup, m_Recipe);

            init();

        }
        public void init()
        {
            Main = new Backside_Panel();
            Setup = new BacksideSetup();
            ROI = new BacksideROI();

            SetPage(ROI);
            SetPage(Setup);


        }
        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        
        public ICommand btnBackSetup
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(Setup);
                });
            }
        }
        public ICommand btnBackROI
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(ROI);
                });
            }
        }
        public ICommand btnBackInspection
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.SetBacksideInspTest();
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
