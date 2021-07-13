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
        public RecipeManagerPanel Main { get; set; }
        public Home_ViewModel home;
        private UserControl m_CurrentPanel;

        public UserControl p_SubPanel
        {
            get => m_CurrentPanel;
            set => SetProperty(ref m_CurrentPanel, value);
        }
        public RecipeOrigin_ViewModel recipeOriginVM;
        public RecipeMask_ViewModel recipeMaskVM;
        public RecipeManager_ViewModel(Home_ViewModel home)
        {
            Main = new RecipeManagerPanel();
            Main.DataContext = this;
            this.home = home;
            recipeOriginVM = new RecipeOrigin_ViewModel(this);
            recipeMaskVM = new RecipeMask_ViewModel(this);
            
            SetOrigin();
        }


        public void SetOrigin()
        {
            p_SubPanel = recipeOriginVM.Main;
            recipeOriginVM.SetOriginViewerTab();
            recipeOriginVM.OriginInfoVisible = true;
            recipeOriginVM.PositionInfoVisible = false;

        }
        public void SetPosition()
        {
            p_SubPanel = recipeOriginVM.Main;
            recipeOriginVM.SetPositionViewerTab();
            recipeOriginVM.PositionInfoVisible = true;
            recipeOriginVM.OriginInfoVisible = false;

        }
        public void SetRecipeMask()
        {
            p_SubPanel = recipeMaskVM.Main;
            //home.m_Setup.SetRecipeMask();
            //recipeMaskVM.SetStain();
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
        public ICommand btnAlign
        {
            get => new RelayCommand(() => { }/*SetAlign()*/);
        }
        public ICommand btnMask
        {
            get => new RelayCommand(()=>SetRecipeMask());
        }
        public ICommand btnBack
        {
            get => new RelayCommand(() => { home.m_Setup.SetHome(); });
        }
        public ICommand btnStain
        {
            get => new RelayCommand(() =>
            {
                Main.MaskRadio.IsChecked = true;
                recipeMaskVM.SetStain();
            });
        }
        public ICommand btn6um
        {
            get => new RelayCommand(() => {
                Main.MaskRadio.IsChecked = true;
                recipeMaskVM.SetTDI();
            });
        }
        public ICommand btn1um
        {
            get => new RelayCommand(() => {
                Main.MaskRadio.IsChecked = true;
                recipeMaskVM.SetStacking();
            });

        }
        public ICommand btnSide
        {
            get => new RelayCommand(() => {
                Main.MaskRadio.IsChecked = true;
                recipeMaskVM.SetSide();
            });
        }
        #endregion
    }
}
