using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_VEGA_P_Vision
{
    public class InspectionItem_ViewModel:ObservableObject
    {
        public InspectionItem Main;
        InspectionOneItem_ViewModel particle, highres, stain, side;
        PodInfo_ViewModel podInfo;
        bool visible, bhighRes, bside;
        #region Property

        public PodInfo_ViewModel PodInfo
        {
            get => podInfo;
            set => SetProperty(ref podInfo, value);
        }
        public bool bHighRes
        {
            get => bhighRes;
            set => SetProperty(ref bhighRes, value);
        }
        public bool bSide
        {
            get => bside;
            set => SetProperty(ref bside, value);
        }
        public bool Visible
        {
            get => visible;
            set => SetProperty(ref visible, value);
        }
        public InspectionOneItem_ViewModel ParticleItem
        {
            get => particle;
            set => SetProperty(ref particle, value);
        }
        public InspectionOneItem_ViewModel HighResItem
        {
            get => highres;
            set => SetProperty(ref highres, value);
        }
        public InspectionOneItem_ViewModel StainItem
        {
            get => stain;
            set => SetProperty(ref stain, value);
        }
        public InspectionOneItem_ViewModel SideItem
        {
            get => side;
            set => SetProperty(ref side, value);
        }
        public RecipeBase recipeBase { get; set; }
        #endregion
        public InspectionItem_ViewModel(PodInfo_ViewModel podInfo, RecipeBase recipeBase,bool bhighRes = true, bool bside = true) //하나의 Recipe에 대한 것
        {
            Main = new InspectionItem();
            Main.DataContext = this;

            bHighRes = bhighRes;
            bSide = bside; 
            this.podInfo = podInfo;
            this.recipeBase = recipeBase;

            particle = new InspectionOneItem_ViewModel("Particle",this,recipeBase,recipeBase.GetItem<LowResRecipe>());
            if(bhighRes)
                highres = new InspectionOneItem_ViewModel("HighRes",this,recipeBase,recipeBase.GetItem<HighResRecipe>());
            stain = new InspectionOneItem_ViewModel("Stain",this,recipeBase,recipeBase.GetItem<StainRecipe>());
            if(bside)
                side = new InspectionOneItem_ViewModel("Side",this,recipeBase,recipeBase.GetItem<SideRecipe>());
             
            VegaPEventManager.RecipeUpdated += VegaPEventManager_RecipeUpdated;
        }
        public void Clear()
        {
            particle.ListItem.Clear();
            highres.ListItem.Clear();
            stain.ListItem.Clear();
            side.ListItem.Clear();
        }
        public void UpdateLoadedRecipe(RecipeBase recipeBase)
        {
            particle.AddItem(recipeBase, recipeBase.GetItem<LowResRecipe>());
            if (bHighRes)
                highres.AddItem(recipeBase, recipeBase.GetItem<HighResRecipe>());
            stain.AddItem(recipeBase, recipeBase.GetItem<StainRecipe>());
            if (bside)
                side.AddItem(recipeBase, recipeBase.GetItem<SideRecipe>());
        }

        private void VegaPEventManager_RecipeUpdated(object sender, RecipeEventArgs e) //e.recipe가 지금 Recipe랑 일치한다면 
        {
            if(e.recipe.GetType().Name.Equals(recipeBase.GetType().Name))
            {
                recipeBase = e.recipe;

                particle.RecipeItemBase = e.recipe.GetItem<LowResRecipe>();
                if(bhighRes)
                    highres.RecipeItemBase = e.recipe.GetItem<HighResRecipe>();
                stain.RecipeItemBase = e.recipe.GetItem<StainRecipe>();
                if(bside)
                    side.RecipeItemBase = e.recipe.GetItem<SideRecipe>();

                string selectedItemName = podInfo.selectedRecipeItem.GetType().Name;

                if (selectedItemName.Equals(particle.RecipeItemBase.GetType().Name))
                    podInfo.selectedRecipeItem = particle.RecipeItemBase;
                else if (selectedItemName.Equals(highres.RecipeItemBase.GetType().Name))
                    podInfo.selectedRecipeItem = highres.RecipeItemBase;
                else if (selectedItemName.Equals(stain.RecipeItemBase.GetType().Name))
                    podInfo.selectedRecipeItem = stain.RecipeItemBase;
                else if (selectedItemName.Equals(side.RecipeItemBase.GetType().Name))
                    podInfo.selectedRecipeItem = side.RecipeItemBase;

                podInfo.SetRecipe(podInfo.selectedRecipeItem);
            }
        }
    }
}
