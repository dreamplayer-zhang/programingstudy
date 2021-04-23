using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using RootTools;
using RootTools_Vision;

namespace Root_VEGA_P_Vision
{
    public class Setup_ViewModel : ObservableObject, IDisposable
    {
        #region Property
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

        private ObservableCollection<UIElement> m_NaviButtons = new ObservableCollection<UIElement>();
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
        #endregion

        private Home_ViewModel homeVM;
        private RecipeManager_ViewModel recipeManagerVM;
        public Maintenance_ViewModel maintVM;

        public Setup_ViewModel()
        {
            init();
        }

        public void init()
        {
            InitAllPanel();
            InitAllNaviBtn();
            SetHome();
        }

        private void InitAllPanel()
        {
            homeVM = new Home_ViewModel(this);
            recipeManagerVM = new RecipeManager_ViewModel(this);
            maintVM = new Maintenance_ViewModel(this);
        }
        private void InitAllNaviBtn()
        {
            m_btnNaviInspection = new NaviBtn("Inspection");
            m_btnNaviInspection.Btn.Click += Navi_InspectionClick;

            //m_btnNaviRecipeWizard = new NaviBtn("Recipe Wizard");
            //m_btnNaviRecipeWizard.Btn.Click += Navi_RecipeWizardClick;

            m_btnNaviMaintenance = new NaviBtn("Maintenance");
            m_btnNaviMaintenance.Btn.Click += Navi_MaintClick;

            //m_btnNaviGEM = new NaviBtn("GEM");
            //m_btnNaviGEM.Btn.Click += Navi_GEMClick;
        }

        #region Navi Buttons

        // SetupHome Navi Buttons
        public NaviBtn m_btnNaviInspection;
        public NaviBtn m_btnNaviRecipeWizard;
        public NaviBtn m_btnNaviMaintenance;
        public NaviBtn m_btnNaviGEM;

        // 
        #endregion

        #region NaviBtn Event

        #region Main
        void Navi_InspectionClick(object sender, RoutedEventArgs e)
        {
            SetInspection();
        }
        void Navi_RecipeWizardClick(object sender, RoutedEventArgs e)
        {
            SetRecipeWizard();
        }
        void Navi_MaintClick(object sender, RoutedEventArgs e)
        {
            SetMaintenance();
        }
        void Navi_GEMClick(object sender, RoutedEventArgs e)
        {
            SetGEM();
        }
        #endregion

        #region Command
        public ICommand btnMode
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetHome();
                    UIManager.Instance.ChangeUIMode();
                });
            }
        }
        public ICommand btnNaviSetupHome
        {
            get
            {
                return new RelayCommand(SetHome);
            }
        }
        public ICommand btnNewRecipe
        {
            get
            {
                return new RelayCommand(()=>
                {
                    MessageBox.Show("WIND2 전체 레시피 저장하는거 구현해줘요");
                });
            }

        }

        public ICommand btnSaveRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    // 각패널 별로있어야함
                    //if (this.Recipe.RecipePath == "")
                    //{
                    //    ProgramManager.Instance.ShowDialogSaveRecipe();
                    //}
                    //else
                    //{
                    //    ProgramManager.Instance.SaveRecipe(this.Recipe.RecipePath);
                    //}
                });
            }
        }

        public ICommand btnLoadRecipe
        {
            get
            {
                return new RelayCommand(()=>
                {
                    MessageBox.Show("WIND2 전체 레시피 로드하는거 구현해줘요");
                });
            }
        }

       
        #endregion

        #region Panel Change Method

        #region Main
        public void SetHome()
        {
            p_NaviButtons.Clear();

            //homeVM.SetPage(homeVM.Main);
            p_CurrentPanel = homeVM.Main;
            p_CurrentPanel.DataContext = homeVM;
        }
        public void SetInspection()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviInspection);

            //p_CurrentPanel = inspectionVM.Main;
            //p_CurrentPanel.DataContext = inspectionVM;
        }
        public void SetRecipeWizard()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);

            //Wizard.SetPage(Wizard.Summary);

            p_CurrentPanel = recipeManagerVM.Main;
            p_CurrentPanel.DataContext = recipeManagerVM;
        }
        public void SetMaintenance()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviMaintenance);

            p_CurrentPanel = maintVM.Main;
            p_CurrentPanel.DataContext = maintVM;
        }
        public void SetGEM()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviGEM);

            //p_CurrentPanel = gemVM.Main;
            //p_CurrentPanel.DataContext = gemVM;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        //#region Recipe Wizard
        //public void SetWizardFrontSide()
        //{
        //    p_NaviButtons.Clear();
        //    p_NaviButtons.Add(m_btnNaviRecipeWizard);
        //    p_NaviButtons.Add(m_btnNaviFrontSide);

        //    frontsideVM.SetPage(frontsideVM.Summary);

        //    p_CurrentPanel = frontsideVM.Main;
        //    p_CurrentPanel.DataContext = frontsideVM;
        //}
        //public void SetWizardBackSide()
        //{
        //    p_NaviButtons.Clear();
        //    p_NaviButtons.Add(m_btnNaviRecipeWizard);
        //    p_NaviButtons.Add(m_btnNaviBackSide);

        //    p_CurrentPanel = backsideVM.Main;
        //    p_CurrentPanel.DataContext = backsideVM;
        //}
        //public void SetWizardEBR()
        //{
        //    p_NaviButtons.Clear();
        //    p_NaviButtons.Add(m_btnNaviRecipeWizard);
        //    p_NaviButtons.Add(m_btnNaviEBR);

        //    p_CurrentPanel = ebrVM.Main;
        //    p_CurrentPanel.DataContext = ebrVM;
        //}
        //public void SetWizardEdge()
        //{
        //    p_NaviButtons.Clear();
        //    p_NaviButtons.Add(m_btnNaviRecipeWizard);
        //    p_NaviButtons.Add(m_btnNaviEdge);

        //    p_CurrentPanel = edgeVM.Main;
        //    p_CurrentPanel.DataContext = edgeVM;
        //}

        //public void SetBacksideInspTest()
        //{
        //    p_NaviButtons.Clear();
        //    p_NaviButtons.Add(m_btnNaviRecipeWizard);
        //    p_NaviButtons.Add(m_btnNaviBackSide);
        //}

        //public void Dispose()
        //{
        //    throw new NotImplementedException();
        //}
        //#endregion

        #endregion
        #endregion

    }



}
