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
        private UserControl m_CurrentPanel;

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
        private ObservableCollection<UIElement> m_NaviButtons = new ObservableCollection<UIElement>();

        private Home_ViewModel homeVM;
        public RecipeManager_ViewModel recipeManagerVM;
        public Maintenance_ViewModel maintVM;

        public Setup Main;
        public Setup_ViewModel()
        {
            Main = new Setup();
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

            m_btnNaviRecipeWizard = new NaviBtn("Recipe Wizard");
            m_btnNaviRecipeWizard.Btn.Click += Navi_RecipeWizardClick;

            m_btnNaviRecipeMask = new NaviBtn("Recipe Mask");
            m_btnNaviRecipeMask.Btn.Click += Navi_RecipeMaskClick; ;

            m_btnNaviMaintenance = new NaviBtn("Maintenance");
            m_btnNaviMaintenance.Btn.Click += Navi_MaintClick;
        }

        #region Navi Buttons

        // SetupHome Navi Buttons
        public NaviBtn m_btnNaviInspection;
        public NaviBtn m_btnNaviRecipeWizard;
        public NaviBtn m_btnNaviRecipeMask;
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
        void Navi_RecipeMaskClick(object sender, RoutedEventArgs e)
        {
            SetRecipeMask();
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
                return new RelayCommand(() =>
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
                return new RelayCommand(() =>
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
            p_CurrentPanel = homeVM.Main;
        }
        public void SetInspection()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviInspection);
        }
        public void SetRecipeWizard()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_CurrentPanel = recipeManagerVM.Main;
            recipeManagerVM.Main.radiobtnOrigin.IsChecked = true;
            recipeManagerVM.SetOrigin();
        }
        public void SetRecipeMask()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviRecipeMask);

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


        #endregion
        #endregion

    }



}
