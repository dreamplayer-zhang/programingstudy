using Microsoft.Win32;
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
    public class FolderItem : ObservableObject
    {
        public string FolderName { get; set; }
    }


    public delegate void RecipeSelectedHandler(string recipePath);

    public class RecipeSelectionViewer_ViewModel : ObservableObject
    {
        #region [Event]
        public event RecipeSelectedHandler RecipeSelected;
        public event RecipeSelectedHandler RecipeCreated;
        #endregion

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
                if(value != null)
                    CurrentPath = RecipeRootPath + "\\" + selectedProductItem.FolderName;
                RefreshStepItemList();
            }
        }

        string currentPath = @"C:\Root\Recipe";
        public string CurrentPath
        {
            get => currentPath;
            set
            {
                SetProperty(ref currentPath, value);
            }
        }

        string currentOpenPath;
        public string CurrentOpenPath
        {
            get => currentOpenPath;
            set
            {
                SetProperty(ref currentOpenPath, value);
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

        private string searchProductName = "";
        public string SearchProductName
        {
            get => this.searchProductName;
            set
            {
                SetProperty(ref this.searchProductName, value);
                SearchStepName = "";
                RefreshProductItemList();
            }
        }

        private string searchStepName = "";
        public string SearchStepName
        {
            get => this.searchStepName;
            set
            {
                SetProperty(ref this.searchStepName, value);
                RefreshStepItemList();
            }
        }
        #endregion

        public RecipeSelectionViewer_ViewModel()
        {

        }

        public void RefreshProductItemList()
        {
            this.ProductListItems.Clear();

            DirectoryInfo di = new DirectoryInfo(RecipeRootPath);
            di.Create();

            foreach (DirectoryInfo info in di.GetDirectories())
            {
                string fileName = info.Name.ToLower();
                if (fileName.Contains(this.SearchProductName.ToLower()))
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        this.ProductListItems.Add(new FolderItem() { FolderName = info.Name });
                    });
                }
            }
        }

        public void CreateStepFolder()
        {
            if (CurrentPath == RecipeRootPath)
                return;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = CurrentPath;

            if (dlg.ShowDialog() == false) return;

            string[] lastPath = CurrentPath.Split('\\');

            string path = CurrentPath + "\\" + lastPath[lastPath.Length - 1] + "." + Path.GetFileName(dlg.FileName);
            Directory.CreateDirectory(path + "\\" + "Back");
            Directory.CreateDirectory(path + "\\" + "EBR");
            Directory.CreateDirectory(path + "\\" + "Edge");
            Directory.CreateDirectory(path + "\\" + "Front");

            RefreshStepItemList();

            if (RecipeCreated != null)
                RecipeCreated(path);
        }

        public bool CheckPath()
        {
            if(currentPath == RecipeRootPath)
            {
                return false;
            }
            return true;
        }

        private void RefreshStepItemList()
        {
            if (this.SelectedProductItem != null)
            {
                this.StepListItems.Clear();

                ObservableCollection<FolderItem> newItems = new ObservableCollection<FolderItem>();

                DirectoryInfo di_Step = new DirectoryInfo(RecipeRootPath + "\\" + SelectedProductItem.FolderName);

                foreach (DirectoryInfo info in di_Step.GetDirectories())
                {
                    string fileName = info.Name.ToLower();

                   if (fileName.Contains(this.SearchStepName.ToLower()))
                    {
                        newItems.Add(new FolderItem() { FolderName = info.Name });
                    }
                }

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    this.StepListItems = newItems;
                });
            }

        }


        #region [Command]
        public ICommand btnCreateProductCommand
        {
            get => new RelayCommand(() =>
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(RecipeRootPath + "\\" + SearchProductName);
                    di.Create();

                    RefreshProductItemList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                
            });
        }

        public ICommand btnCreateStepCommand
        {
            get => new RelayCommand(() =>
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(RecipeRootPath + "\\" + SelectedProductItem.FolderName + "\\" + SearchStepName);
                    di.Create();

                    RefreshStepItemList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            });
        }

        public ICommand stepItemDoubleClickCommand
        {
            get => new RelayCommand(() =>
            {
                try
                {
                    if (RecipeSelected != null)
                    {
                        CurrentOpenPath = CurrentPath + "\\" + this.SelectedStepItem.FolderName;
                        RecipeSelected(CurrentOpenPath);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            });
        }
        #endregion
    }
}
