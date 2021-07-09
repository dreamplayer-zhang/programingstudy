using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RootTools_Vision;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{

    public class InspectionOneItem_ViewModel:ObservableObject
    {
        private ObservableCollection<UIElement> listItem = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> ListItem
        {
            get
            {
                return listItem;
            }
            set
            {
                SetProperty(ref listItem, value);
            }
        }

        public InspectionOneItem Main;
        string itemName="";
        bool isopenitem = false;
        public bool IsOpenItem
        {
            get => isopenitem;
            set => SetProperty(ref isopenitem, value);
        }
        public string ItemName
        {
            get => itemName;
            set => SetProperty(ref itemName, value);
        }
        InspectionItem_ViewModel inspectionItem;
        RecipeItemBase recipeItemBase;
        public RecipeItemBase RecipeItemBase
        {
            get => recipeItemBase;
            set => SetProperty(ref recipeItemBase, value);
        }
        RecipeBase recipe;
        public InspectionOneItem_ViewModel(string itemName,InspectionItem_ViewModel inspectionItem,RecipeBase recipe,RecipeItemBase recipeItemBase)
        {
            Main = new InspectionOneItem();
            Main.DataContext = this;
            this.recipeItemBase = recipeItemBase;
            this.inspectionItem = inspectionItem;
            ItemName = itemName;
            this.recipe = recipe;
        }

        public ICommand btnHeader
        {
            get => new RelayCommand(() => {
                IsOpenItem = !IsOpenItem;
                inspectionItem.PodInfo.SetRecipe(RecipeItemBase);
                inspectionItem.PodInfo.selectedRecipeItem = RecipeItemBase;
            });
        }
        public ICommand btnAdd
        {
            get => new RelayCommand(() => {
                AddItem(recipe, recipeItemBase);
            });
        }
        public void AddItem(RecipeBase recipe, RecipeItemBase recipeItemBase)
        {
            ConditionItem_ViewModel item = new ConditionItem_ViewModel(ListItem.Count + 1, recipe, recipeItemBase,this);
            ListItem.Add(item.Main);
        }
    }
}
