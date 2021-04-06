using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RootTools_Vision
{

    public class RecipeListViewerItem : ObservableObject
    {
        public string RecipeName { get; set; }
        public string RecipePath { get; set; }
    }

    public class RecipeListViewer_ViewModel : ObservableObject
    {

        #region [Properties]
        private ObservableCollection<RecipeListViewerItem> recipeListViewerItems = new ObservableCollection<RecipeListViewerItem>();
        public ObservableCollection<RecipeListViewerItem> RecipeListViewerItems
        {
            get => this.recipeListViewerItems;
            set
            {
                SetProperty(ref this.recipeListViewerItems, value);
            }
        }
        private string recipeRootPath = @"C:\Root\Recipe";
        public string RecipeRootPath
        {
            get => this.recipeRootPath;
            set
            {
                SetProperty(ref this.recipeRootPath, value);
            }
        }

        private string searchName = "";
        public string SearchName
        {
            get => this.searchName;
            set
            {
                SetProperty(ref this.searchName, value);
                Refresh();
            }
        }
        #endregion

        public RecipeListViewer_ViewModel()
        {

        }

        public void Refresh()
        {
            this.RecipeListViewerItems.Clear();

            DirectoryInfo di = new DirectoryInfo(RecipeRootPath);
            di.Create();

            foreach (FileInfo file in di.GetFiles())
            {
                string fileName = file.Name.ToLower();
                if (fileName.Contains(this.SearchName.ToLower()))
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        this.RecipeListViewerItems.Add(new RecipeListViewerItem() { RecipeName = file.Name, RecipePath = file.FullName });
                    });
                }
            }
        }


        #region [Command]
        //public ICommand LoadedCommand
        //{
        //    get => new RelayCommand(() =>
        //    {
        //        Refresh();
        //    });
        //}
        #endregion
    }
}
