using RootTools_Vision;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    class MainVM:ObservableObject
    {
        public MainWindow mainWindow;
        public SelectMode selectMode;
        public WindowState mainWindowState;

        public MainVM(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            mainWindowState = mainWindow.WindowState;
        }
        public MainVM(SelectMode selectMode)
        {
            this.selectMode = selectMode;
        }

        public WindowState MWindowState
        {
            get => mainWindowState;
            set => SetProperty(ref mainWindowState, value);
        }
        #region [Relay Command]
        public ICommand ModeSelectCommand
        {
            get => new RelayCommand(() => UIManager.Instance.ChangeMainUI(UIManager.Instance.ModeWindow));
        }
        public ICommand ChangeUISetupCommand
        {
            get => new RelayCommand(() => UIManager.Instance.ChangeUISetup());
        }
        public ICommand ChangeUIReviewCommand
        {
            get => new RelayCommand(() => UIManager.Instance.ChangeUIReview());
        }
        public ICommand ChangeUIRunCommand
        {
            get => new RelayCommand(() => UIManager.Instance.ChangeUIRun());
        }
        public ICommand WindowLoadedCommand
        {
            get => new RelayCommand(() =>mainWindow.Window_Loaded());
        }
        public ICommand WindowClosingCommand
        {
            get => new RelayCommand(() => mainWindow.Window_Closing());
        }
        public ICommand MenuItemExitCommand
        {
            get => new RelayCommand(() =>
            {
                mainWindow.Close();
                Application.Current.Shutdown();
            });
        }
        public ICommand MenuFileNew
        {
            get => new RelayCommand(() => {
                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.InitialDirectory = Constants.RootPath.RecipeRootPath;
                dlg.Title = "Save Recipe";
                dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                    string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                    string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                    string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명
                    string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                    DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
                    if (!dir.Exists)
                        dir.Create();

                    CreateRecipe(GlobalObjects.Instance.Get<RecipeCoverFront>(), sFileNameNoExt, sFullPath, sRecipeFolderPath);
                    CreateRecipe(GlobalObjects.Instance.Get<RecipeCoverBack>(), sFileNameNoExt, sFullPath, sRecipeFolderPath);
                    CreateRecipe(GlobalObjects.Instance.Get<RecipePlateFront>(), sFileNameNoExt, sFullPath, sRecipeFolderPath);
                    CreateRecipe(GlobalObjects.Instance.Get<RecipePlateBack>(), sFileNameNoExt, sFullPath, sRecipeFolderPath);
                }
            });
        }

        void CreateRecipe(RecipeBase recipe,string sFileNameNoExt, string sFullPath, string sRecipeFolderPath)
        {
            recipe.Clear();

            recipe.Name = sFileNameNoExt;
            recipe.RecipePath = sFullPath;
            recipe.RecipeFolderPath = sRecipeFolderPath;

            recipe.Save(sFullPath);
        }
        public ICommand MenuFileLoad
        {
            get => new RelayCommand(() => {

                System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                dlg.InitialDirectory = Constants.RootPath.RecipeRootPath;
                dlg.Title = "Load Recipe";
                dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                    string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                    string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                    string sFullPath = Path.Combine(sFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                    DirectoryInfo dir = new DirectoryInfo(sFolderPath);
                    if (!dir.Exists)
                        dir.Create();

                    RecipeBase recipe = GlobalObjects.Instance.Get<RecipeCoverFront>();
                    recipe.Read(sFullPath);
                    VegaPEventManager.OnRecipeUpdated(this, new RecipeEventArgs(recipe));

                    recipe = GlobalObjects.Instance.Get<RecipeCoverBack>();
                    recipe.Read(sFullPath);
                    VegaPEventManager.OnRecipeUpdated(this, new RecipeEventArgs(recipe));

                    recipe = GlobalObjects.Instance.Get<RecipePlateFront>();
                    recipe.Read(sFullPath);
                    VegaPEventManager.OnRecipeUpdated(this, new RecipeEventArgs(recipe));

                    recipe = GlobalObjects.Instance.Get<RecipePlateBack>();
                    recipe.Read(sFullPath);
                    VegaPEventManager.OnRecipeUpdated(this, new RecipeEventArgs(recipe));
                }
            });
        }
        public ICommand MenuFileSave
        {
            get => new RelayCommand(() =>
            {
                RecipeBase recipe = GlobalObjects.Instance.Get<RecipeCoverFront>();
                SaveRecipe(recipe);
                recipe = GlobalObjects.Instance.Get<RecipeCoverBack>();
                SaveRecipe(recipe);
                recipe = GlobalObjects.Instance.Get<RecipePlateFront>();
                SaveRecipe(recipe);
                recipe = GlobalObjects.Instance.Get<RecipePlateBack>();
                SaveRecipe(recipe);
            });
        }

        void SaveRecipe(RecipeBase recipe)
        {
            if (recipe.RecipePath != "")
            {
                recipe.Save(recipe.RecipePath);
            }
            else
            {
                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.InitialDirectory = Constants.RootPath.RecipeRootPath;
                dlg.Title = "Save Recipe";
                dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                    string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                    string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                    string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명
                    string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                    DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
                    if (!dir.Exists)
                        dir.Create();

                    recipe.Name = sFileNameNoExt;
                    recipe.RecipePath = sFullPath;
                    recipe.RecipeFolderPath = sRecipeFolderPath;

                    recipe.Save(sFullPath);
                }
            }
        }
        public ICommand MenuFileSaveAs
        {
            get => new RelayCommand(() => { });
        }
        #endregion
    }
}
