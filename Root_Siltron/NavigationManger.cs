using Root_Siltron.Button;
using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_Siltron
{
    public class NavigationManger : ObservableObject
    {
        private UserControl m_CurrentPanel;
        public UserControl p_CurrentPanel
        {
            get
            {
                return m_CurrentPanel;
            }
            set
            {
                SetProperty(ref m_CurrentPanel, value);
            }
        }
        private ObservableCollection<UIElement> m_NaviButtons;
        public ObservableCollection<UIElement> p_NaviButtons
        {
            get
            {
                return m_NaviButtons;
            }
            set
            {
                SetProperty(ref m_NaviButtons, value);
            }
        }

        public HomeUI m_Home;
        public InspectionUI m_Insp;
        public RecipeWizardUI m_RecipeWizard;
        public AlignmentUI m_AlignMent;
        public GeneralUI m_General;
        public MaintenanceUI m_Maint;
        public GEMUI m_Gem;

        public NavigationManger()
        {
            p_NaviButtons = new ObservableCollection<UIElement>();
            init();
        }
        public void init()
        {
            m_Home = new HomeUI(this);
            m_Insp = new InspectionUI(this);
            m_RecipeWizard = new RecipeWizardUI(this);
            m_AlignMent = new AlignmentUI(this);
            m_General = new GeneralUI(this);
            m_Maint = new MaintenanceUI(this);
            m_Gem = new GEMUI(this);

            p_CurrentPanel = m_Home.Main;
            p_CurrentPanel.DataContext = m_Home;

            SetHome();
        }

        public void SetHome()
        {
            p_NaviButtons.Clear();

            m_Home.SetSummary();
            p_CurrentPanel = m_Home.Main;
            p_CurrentPanel.DataContext = m_Home;
       
        }
        public void SetInspection()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_Insp.NaviInsepction);

            p_CurrentPanel = m_Insp.Main;
            p_CurrentPanel.DataContext = m_Insp;
        }
        public void SetRecipeWizard()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_RecipeWizard.NaviRcpWizard);

            m_RecipeWizard.SetSummary();

            p_CurrentPanel = m_RecipeWizard.Main;
            p_CurrentPanel.DataContext = m_RecipeWizard;
        }
        public void SetAlignMent()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_RecipeWizard.NaviRcpWizard);
            p_NaviButtons.Add(m_AlignMent.NaviAlignment);

            m_AlignMent.SetSummary();

            p_CurrentPanel = m_AlignMent.Main;
            p_CurrentPanel.DataContext = m_AlignMent;
        }
        public void SetGeneral()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_RecipeWizard.NaviRcpWizard);
            p_NaviButtons.Add(m_General.NaviGeneral);

            m_General.SetSummary();

            p_CurrentPanel = m_General.Main;
            p_CurrentPanel.DataContext = m_General;
        }
        public void SetMaintenance()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_Maint.NaviMaint);

            p_CurrentPanel = m_Maint.Main;
            p_CurrentPanel.DataContext = m_Maint;
        }
        public void SetGEM()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_Gem.naviGEM);

            p_CurrentPanel = m_Gem.Main;
            p_CurrentPanel.DataContext = m_Gem;
        }

        public class HomeUI
        {
            NavigationManger m_Navigation;
            public NavigationPanelUI.HomeUI.HomePanel Main;
            public NavigationPanelUI.HomeUI.HomeSummaryPage Summary;

            public NaviBtn NaviSummary;
            public NaviBtn NaviInspection;
            public NaviBtn NaviMaintenance;
            public NaviBtn NaviGEM;

            public HomeUI(NavigationManger navi)
            {
                m_Navigation = navi;
                init();
                
            }
            private void init()
            {
                Main = new NavigationPanelUI.HomeUI.HomePanel();
                Summary = new NavigationPanelUI.HomeUI.HomeSummaryPage();

                NaviSummary = new NaviBtn("Summary");
                NaviInspection = new NaviBtn("Inspection");
                NaviMaintenance = new NaviBtn("Maintenance");
                NaviGEM = new NaviBtn("GEM");

               //Main.SubPanel.Children.Add(Summary);
            }
            public void SetSummary()
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(Summary);
                if(m_Navigation.p_NaviButtons.Count > 0)
                {
                    int index = m_Navigation.p_NaviButtons.Count -1;
                    m_Navigation.p_NaviButtons.RemoveAt(index);
                }
                m_Navigation.p_NaviButtons.Add(NaviSummary);
            }
            public ICommand btnSummary
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        SetSummary();
                    });
                }
            }
            public ICommand btnInspection
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetInspection);
                }
            }
            public ICommand btnRecipeWizard
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetRecipeWizard);
                }
            }
            public ICommand btnMaintenance
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetMaintenance);
                }
            }
            public ICommand btnGEM
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetGEM);
                }
            }
        }
        public class InspectionUI
        {
            NavigationManger m_Navigation;
            public NavigationPanelUI.InspectionUI.InspectionPanel Main;
            public NaviBtn NaviInsepction;
            public InspectionUI(NavigationManger navi)
            {
                m_Navigation = navi;
                init();
            }
            private void init()
            {
                Main = new NavigationPanelUI.InspectionUI.InspectionPanel();
                NaviInsepction = new NaviBtn("Inspection");
            }

            public ICommand btnInspStart
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                    });
                }
            }
            public ICommand btnInspLoad
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                    });
                }
            }
            public ICommand btnInspSnap
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                    });
                }
            }
            public ICommand btnInspBack
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetHome);
                }
            }
        }
        public class RecipeWizardUI
        {
            NavigationManger m_Navigation;

            public NavigationPanelUI.RecipeWizardUI.RecipeWizardPanel Main;
            public NavigationPanelUI.RecipeWizardUI.RecipeSummaryPage Summary;

            public NaviBtn NaviRcpWizard;
            public NaviBtn NaviSummary;
            public NaviBtn NaviAlignment;


            public RecipeWizardUI(NavigationManger navi)
            {
                m_Navigation = navi;
                init();
            }
            private void init()
            {
                Main = new NavigationPanelUI.RecipeWizardUI.RecipeWizardPanel();
                Summary = new NavigationPanelUI.RecipeWizardUI.RecipeSummaryPage();

                NaviRcpWizard = new NaviBtn("RecipeWizard");
                NaviSummary = new NaviBtn("Summary");
                NaviAlignment = new NaviBtn("Alignment");

                Main.SubPanel.Children.Add(Summary);
            }

            public void SetSummary()
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(Summary);

                if (m_Navigation.p_NaviButtons.Count > 1)
                {
                    int index = m_Navigation.p_NaviButtons.Count - 1;
                    m_Navigation.p_NaviButtons.RemoveAt(index);
                }

                m_Navigation.p_NaviButtons.Add(NaviSummary);
                
            }

             
            public ICommand btnWizardSummary
            {
                get
                {
                    return new RelayCommand(SetSummary);
                }
            }
            public ICommand btnWizardAlignnment
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        m_Navigation.SetAlignMent();
                    });
                }
            }
            public ICommand btnWizardGeneral
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        m_Navigation.SetGeneral();
                    });
                }
            }
            public ICommand btnWizardBack
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        m_Navigation.SetHome();
                    });
                }
            }
            
        }
        public class AlignmentUI
        {
            NavigationManger m_Navigation;

            public NavigationPanelUI.RecipeWizardUI.AlignmentUI.AlignmentPanel Main;
            public NavigationPanelUI.RecipeWizardUI.AlignmentUI.AlignmentSummaryPage Summary;
            public NavigationPanelUI.RecipeWizardUI.AlignmentUI.AlignmentSetupPage Setup;

            public NaviBtn NaviAlignment;
            public NaviBtn NaviSummary;
            public NaviBtn NaviSetup;

            public AlignmentUI(NavigationManger navi)
            {
                m_Navigation = navi;
                init();
            }
            public void init()
            {
                Main = new NavigationPanelUI.RecipeWizardUI.AlignmentUI.AlignmentPanel();
                Summary = new NavigationPanelUI.RecipeWizardUI.AlignmentUI.AlignmentSummaryPage();
                Setup = new NavigationPanelUI.RecipeWizardUI.AlignmentUI.AlignmentSetupPage();

                NaviAlignment = new NaviBtn("Alignment");
                NaviSummary = new NaviBtn("Summary");
                NaviSetup = new NaviBtn("AlignmentSetup");

                SetSummary();
            }
            public void SetSummary()
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(Summary);

                if(!m_Navigation.p_NaviButtons.Contains(NaviSummary))
                {
                    m_Navigation.p_NaviButtons.Add(NaviSummary);
                }
                else
                {
                    if (m_Navigation.p_NaviButtons.Count > 1)
                    {
                        int index = m_Navigation.p_NaviButtons.Count - 1;
                        m_Navigation.p_NaviButtons.RemoveAt(index);
                    }
                    m_Navigation.p_NaviButtons.Add(NaviSummary);
                }
            }
            public void SetAlignSetup()
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(Setup);
                if(m_Navigation.p_NaviButtons.Count >1)
                {
                    int index = m_Navigation.p_NaviButtons.Count - 1;
                    m_Navigation.p_NaviButtons.RemoveAt(index);
                }
                m_Navigation.p_NaviButtons.Add(NaviSetup);
            }

            public ICommand btnAlignSummary
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetAlignMent);
                }
            }           
            public ICommand btnAlignLoad
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        
                    });
                }
            }
            public ICommand btnAlignSaveAs
            {
                get
                {
                    return new RelayCommand(() =>
                    {

                    });
                }
            }
            public ICommand btnAlignSetup
            {
                get
                {
                    return new RelayCommand(SetAlignSetup);
                }
            }
            public ICommand btnAlignBack
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        m_Navigation.SetRecipeWizard();
                    });
                }
            }
        }
        public class GeneralUI
        {
            NavigationManger m_Navigation;

            public NavigationPanelUI.RecipeWizardUI.GeneralUI.GeneralPanel Main;
            public NavigationPanelUI.RecipeWizardUI.GeneralUI.GeneralSummaryPage Summary;

            public NaviBtn NaviGeneral;
            public NaviBtn NaviSummary;

            public GeneralUI(NavigationManger navi)
            {
                m_Navigation = navi;
                init();
            }
            public void init()
            {
                Main = new NavigationPanelUI.RecipeWizardUI.GeneralUI.GeneralPanel();
                Summary = new NavigationPanelUI.RecipeWizardUI.GeneralUI.GeneralSummaryPage();

                NaviGeneral = new NaviBtn("General");
                NaviSummary = new NaviBtn("Summary");

                SetSummary();
                
            }

            public void SetSummary()
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(Summary);

                if (!m_Navigation.p_NaviButtons.Contains(NaviSummary))
                {
                    m_Navigation.p_NaviButtons.Add(NaviSummary);
                }
                else
                {
                    if (m_Navigation.p_NaviButtons.Count > 1)
                    {
                        int index = m_Navigation.p_NaviButtons.Count - 1;
                        m_Navigation.p_NaviButtons.RemoveAt(index);
                    }
                    m_Navigation.p_NaviButtons.Add(NaviSummary);
                }

            }

            public ICommand btnGeneralSummary
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetGeneral);
                }
            }
            public ICommand btnGeneralAlginRecipe
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        
                    });
                }
            }
            public ICommand btnGeneralMask
            {
                get
                {
                    return new RelayCommand(() =>
                    {

                    });
                }
            }
            public ICommand btnGeneralSetup
            {
                get
                {
                    return new RelayCommand(() =>
                    {

                    });
                }
            }
            public ICommand btnGeneralBack
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        m_Navigation.SetRecipeWizard();
                    });
                }
            }

        }
        public class MaintenanceUI
        {
            NavigationManger m_Navigation;
            public NavigationPanelUI.MaintenanceUI.MaintenancePanel Main;
            public NaviBtn NaviMaint;
            public MaintenanceUI(NavigationManger navi)
            {
                m_Navigation = navi;
                init();
            }
            public void init()
            {
                Main = new NavigationPanelUI.MaintenanceUI.MaintenancePanel();
                NaviMaint = new NaviBtn("Maintenance");             
            }
            
            public ICommand btnMaintBack
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        m_Navigation.SetHome();
                    });
                }
            }
        }
        public class GEMUI
        {
            NavigationManger m_Navigation;
            public NavigationPanelUI.GEMUI.GEMPanel Main;
            public NaviBtn naviGEM;

            public GEMUI(NavigationManger navi)
            {
                m_Navigation = navi;
                init();

            }
            private void init()
            {
                Main = new NavigationPanelUI.GEMUI.GEMPanel();
                naviGEM = new NaviBtn("GEM");
            }
            public ICommand btnGEMBack
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        m_Navigation.SetHome();
                    });
                }
            }
        }
    }



}
