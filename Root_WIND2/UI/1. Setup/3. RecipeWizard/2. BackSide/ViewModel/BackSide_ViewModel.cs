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
        public BacksideInspection Inspection;


        private BacksideInspection_ViewModel m_BacksideInspection_VM;
        public BacksideInspection_ViewModel p_BacksideInspection_VM
        {
            get
            {
                return m_BacksideInspection_VM;
            }
            set
            {
                SetProperty(ref m_BacksideInspection_VM, value);
            }
        }

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
        public Backside_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;

            p_BacksideSetup_VM = new BacksideSetup_ViewModel();

            p_BacksideROI_VM = new BacksideROI_ViewModel();
            p_BacksideROI_VM.init(m_Setup);

            p_BacksideInspection_VM = new BacksideInspection_ViewModel();
            p_BacksideInspection_VM.init(m_Setup);

            init();

        }
        public void init()
        {
            Main = new Backside_Panel();
            Setup = new BacksideSetup();
            ROI = new BacksideROI();
            Inspection = new BacksideInspection();

            SetPage(ROI);
            SetPage(Setup);
            SetPage(Inspection);
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
                    SetPage(Inspection);
                    m_BacksideInspection_VM.SetPage(Inspection);
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
