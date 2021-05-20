using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    public class RecipeManager_ViewModel:ObservableObject
    {
        public Setup_ViewModel setup;
        public RecipeManagerPanel Main;

        private UserControl m_CurrentPanel;

        public UserControl p_SubPanel
        {
            get => m_CurrentPanel;
            set => SetProperty(ref m_CurrentPanel, value);
        }
        public RecipeOrigin_ViewModel recipeOriginVM;
        public RecipeMask_ViewModel recipeMaskVM;
        public RecipeManager_ViewModel(Setup_ViewModel setup)
        {
            this.setup = setup;
            Main = new RecipeManagerPanel();
            Main.DataContext = this;
            recipeOriginVM = new RecipeOrigin_ViewModel(this);
            recipeMaskVM = new RecipeMask_ViewModel(this);
        }


        public void SetOrigin()
        {
            p_SubPanel = recipeOriginVM.Main;
            recipeOriginVM.SetOriginViewerTab();
        }
        public void SetPosition()
        {
            p_SubPanel = recipeOriginVM.Main;
            recipeOriginVM.SetPositionViewerTab();
        }
        public void SetRecipeMask()
        {
            setup.SetRecipeMask();
            setup.p_CurrentPanel = recipeMaskVM.Main;
            recipeMaskVM.Main.radiobtnStain.IsChecked = true;
            recipeMaskVM.SetStain();
        }
        #region [RelayCommand]
        public ICommand btnOrigin
        {
            get => new RelayCommand(() => SetOrigin());
        }
        public ICommand btnPosition
        {
            get => new RelayCommand(()=>SetPosition());
        }
        public ICommand btnMask
        {
            get => new RelayCommand(()=>SetRecipeMask());
        }
        public ICommand btnBack
        {
            get => new RelayCommand(() => { setup.SetHome(); });
        }
        #endregion
    }
}
