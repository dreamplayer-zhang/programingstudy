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
    public class PodInfo_ViewModel:ObservableObject
    {
        public PodInfo_Panel Main;
        public Home_ViewModel home;
        ImageNROI_ViewModel imageNROI;
        InspectionItem_ViewModel cfItem,cbItem,bfItem,bbItem;
        UserControl subPanel;
        string selectedItem;
        public RecipeItemBase selectedRecipeItem { get; set; }
        public string SelectedItem
        {
            get => selectedItem;
            set
            {
                SetProperty(ref selectedItem, value);
            }
        }

        #region Property
        public UserControl SubPanel
        {
            get => subPanel;
            set => SetProperty(ref subPanel, value);
        }
        public InspectionItem_ViewModel CFItem
        {
            get => cfItem;
            set => SetProperty(ref cfItem, value);
        }
        public InspectionItem_ViewModel CBItem
        {
            get => cbItem;
            set => SetProperty(ref cbItem, value);
        }
        public InspectionItem_ViewModel BFItem
        {
            get => bfItem;
            set => SetProperty(ref bfItem, value);
        }
        public InspectionItem_ViewModel BBItem
        {
            get => bbItem;
            set => SetProperty(ref bbItem, value);
        }
        #endregion
        public PodInfo_ViewModel(Home_ViewModel home)
        {
            Main = new PodInfo_Panel();
            Main.DataContext = this;
            imageNROI = new ImageNROI_ViewModel(this);
            CFItem = new InspectionItem_ViewModel(this, GlobalObjects.Instance.Get<RecipeCoverFront>(), false);
            CBItem = new InspectionItem_ViewModel(this, GlobalObjects.Instance.Get<RecipeCoverBack>(),true,false);
            BFItem = new InspectionItem_ViewModel(this, GlobalObjects.Instance.Get<RecipePlateFront>());
            BBItem = new InspectionItem_ViewModel(this, GlobalObjects.Instance.Get<RecipePlateBack>(), false, false);

            selectedRecipeItem = CFItem.ParticleItem.RecipeItemBase;
            SubPanel = imageNROI.Main;

            VegaPEventManager.LoadedAllRecipe += VegaPEventManager_LoadedAllRecipe;
        }

        private void VegaPEventManager_LoadedAllRecipe(object sender, LoadAllRecipeEventArgs e)
        {
            foreach(var v in e.CoverFrontRecipes)
                CFItem.UpdateLoadedRecipe(v);

            foreach (var v in e.CoverBackRecipes)
                CBItem.UpdateLoadedRecipe(v);
            
            foreach (var v in e.PlateFrontRecipes)
                BFItem.UpdateLoadedRecipe(v);
            
            foreach (var v in e.PlateBackRecipes)
                BBItem.UpdateLoadedRecipe(v);

            MessageBox.Show("All the Recipes are Loaded!!");
        } 

        #region RelayCommand
        public ICommand btnBack
        {
            get => new RelayCommand(() => { home.m_Setup.SetHome(); });
        }
        public ICommand btnCoverFront
        {
            get => new RelayCommand(() => {
                CFItem.Visible = !CFItem.Visible;
            });
        }
        public ICommand btnCoverBack
        {
            get => new RelayCommand(() => {
                CBItem.Visible = !CBItem.Visible;
            });
        }
        public ICommand btnPlateFront
        {
            get => new RelayCommand(() => {
                BFItem.Visible = !BFItem.Visible;
            });
        }
        public ICommand btnPlateBack
        {
            get => new RelayCommand(() => {
                BBItem.Visible = !BBItem.Visible;
            });
        }
        #endregion

        public void SetRecipe(RecipeItemBase selectedItem)
        {
            imageNROI.SelectedItemList.Init(selectedItem, true);
        }
    }
}
