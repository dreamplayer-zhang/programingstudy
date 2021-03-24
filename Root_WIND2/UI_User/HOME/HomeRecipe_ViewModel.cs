using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_WIND2.UI_User
{
    public class HomeRecipe_ViewModel : ObservableObject
    {
        #region [Properties]

        private RecipeListViewer_ViewModel recipeListViewerVM = new RecipeListViewer_ViewModel();
        public RecipeListViewer_ViewModel RecipeListViewerVM
        {
            get => this.recipeListViewerVM;
            set
            {
                SetProperty(ref this.recipeListViewerVM, value);
            }
        }

        #endregion
        public HomeRecipe_ViewModel()
        {

        }


        #region [Command]
        public ICommand LoadedCommand
        {
            get => new RelayCommand(() =>
            {
                this.RecipeListViewerVM.Refresh();
            });
        }
        #endregion
    }
}
