using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    public class RecipeSetting_ViewModel:ObservableObject
    {
        public RecipeManager_ViewModel RecipeManager;
        public RecipeSetting_Panel Main;
        private UserControl m_CurrentPanel;
        public UserControl p_SubPanel
        {
            get => m_CurrentPanel;
            set => SetProperty(ref m_CurrentPanel, value);
        }
        public RecipeSetting_ViewModel(RecipeManager_ViewModel RecipeManager)
        {
            this.RecipeManager = RecipeManager;
            Main = new RecipeSetting_Panel();
            recipeStainVM = new RecipeStain_ViewModel(this);
            recipe6umVM = new Recipe6um_ViewModel(this);
            recipe1umVM = new Recipe1um_ViewModel(this);
            recipeSideVM = new RecipeSide_ViewModel(this);
        }


        #region Recipe Wizard
        private RecipeStain_ViewModel recipeStainVM;
        private Recipe6um_ViewModel recipe6umVM;
        private Recipe1um_ViewModel recipe1umVM;
        private RecipeSide_ViewModel recipeSideVM;
        public void SetStain()
        {
            p_SubPanel = recipeStainVM.Main;
            p_SubPanel.DataContext = recipeStainVM;
        }
        public void Set6um()
        {
            p_SubPanel = recipe6umVM.Main;
            p_SubPanel.DataContext = recipe6umVM;
        }

        public void Set1um()
        {
            p_SubPanel = recipe1umVM.Main;
            p_SubPanel.DataContext = recipe1umVM;
        }
        public void SetSide()
        {
            p_SubPanel = recipeSideVM.Main;
            p_SubPanel.DataContext = recipeSideVM;
        }
        #endregion

        public ICommand btnBack
        {
            get => new RelayCommand(() => { RecipeManager.m_Setup.SetRecipeWizard(); });
        }
    }
}
