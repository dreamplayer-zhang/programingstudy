﻿using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2.UI_Temp
{
    class Setup_ViewModel : ObservableObject
    {

        #region [Binding Objects]
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
        #endregion

        #region [Panels]
        private UI_Temp.RecipeWizardPanel_ViewModel recipeWizardVM;
        #endregion

        #region [Method]

        public Setup_ViewModel()
        {
            Initialize();

            SetRecipeWizard();
        }

        public void Initialize()
        {
            InitializePanel();
        }

        public void InitializePanel()
        {
            this.recipeWizardVM = new UI_Temp.RecipeWizardPanel_ViewModel();
        }

        public void SetHome()
        {
            
        }

        public void SetInspection()
        {

        }

        public void SetRecipeWizard()
        {
            this.p_CurrentPanel = this.recipeWizardVM.Main;
            this.p_CurrentPanel.DataContext = this.recipeWizardVM;
        }

        public void SetMaintance()
        {

        }
        #endregion

        #region [Command]
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
        #endregion
    }
}
