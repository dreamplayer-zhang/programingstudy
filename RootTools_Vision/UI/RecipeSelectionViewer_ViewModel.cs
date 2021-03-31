using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    public class FolderItem : ObservableObject
    {
        public string FolderName { get; set; }
    }

    public class RecipeSelectionViewer_ViewModel : ObservableObject
    {
        #region [Properties]
        private ObservableCollection<FolderItem> productListItems = new ObservableCollection<FolderItem>();
        public ObservableCollection<FolderItem> ProductListItems
        {
            get => this.productListItems;
            set
            {
                SetProperty(ref this.productListItems, value);
            }
        }

        private FolderItem selectedProductItem;
        public FolderItem SelectedProductItem
        {
            get => this.selectedProductItem;
            set
            {
                SetProperty(ref this.selectedProductItem, value);
                RefreshStepItemList();
            }
        }

        private ObservableCollection<FolderItem> stepListItems = new ObservableCollection<FolderItem>();
        public ObservableCollection<FolderItem> StepListItems
        {
            get => this.stepListItems;
            set
            {
                SetProperty(ref this.stepListItems, value);
            }
        }

        private FolderItem selectedStepItem;
        public FolderItem SelectedStepItem
        {
            get => this.selectedStepItem;
            set
            {
                SetProperty(ref this.selectedStepItem, value);
                
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

        public RecipeSelectionViewer_ViewModel()
        {

        }

        public void Refresh()
        {
            this.ProductListItems.Clear();

            DirectoryInfo di = new DirectoryInfo(RecipeRootPath);
            di.Create();

            foreach (DirectoryInfo info in di.GetDirectories())
            {
                string fileName = info.Name.ToLower();
                if (fileName.Contains(this.SearchName.ToLower()))
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        this.ProductListItems.Add(new FolderItem() { FolderName = info.Name });
                    });
                }
            }

            RefreshStepItemList();
        }

        private void RefreshStepItemList()
        {
            if (this.SelectedProductItem != null)
            {
                this.StepListItems.Clear();

                DirectoryInfo di_Step = new DirectoryInfo(RecipeRootPath + "\\" + SelectedProductItem.FolderName);

                foreach (DirectoryInfo info in di_Step.GetDirectories())
                {
                    string fileName = info.Name.ToLower();

                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        this.StepListItems.Add(new FolderItem() { FolderName = info.Name });
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
