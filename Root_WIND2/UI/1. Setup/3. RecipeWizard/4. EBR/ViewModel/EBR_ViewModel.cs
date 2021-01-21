using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2
{
    class EBR_ViewModel : ObservableObject
    {
        Setup_ViewModel setupVM;
        RecipeBase recipe;

        public EBR_Panel Main;
        public EBRSetup_ViewModel SetupVM;
        public EBRSetupPage SetupPage;

        public EBR_ViewModel(Setup_ViewModel setup)
        {
            this.setupVM = setup;
            Init();
        }

        public void Init()
        {
            Main = new EBR_Panel();
            SetupVM = new EBRSetup_ViewModel();
            SetupVM.Init(setupVM);

            SetupPage = new EBRSetupPage();
            SetupPage.DataContext = SetupVM;
            SetPage(SetupPage);
        }

        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        public ICommand btnEBRSetup
        {
            get
            {
                return new RelayCommand(() => SetPage(SetupPage));
            }
        }
        public ICommand btnEBRSnap
        {
            get
            {
                return new RelayCommand(() => SetupVM.Scan());
            }
        }

        public ICommand btnEBRInsp
        {
            get
            {
                return new RelayCommand(() => SetupVM.Inspect());
            }
        }

        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(() => setupVM.SetRecipeWizard());
            }
        }


        public ICommand btnEBRNewRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeEBR recipe = GlobalObjects.Instance.Get<RecipeEBR>();

                    System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                    dlg.InitialDirectory = Constants.Path.RecipeEBRRootPath;
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

                        recipe.Clear();
                        recipe.Name = sFileNameNoExt;
                        recipe.RecipePath = sFullPath;
                        recipe.RecipeFolderPath = sRecipeFolderPath;

                        recipe.Save(sFullPath);
                    }

                });
            }
        }

        public ICommand btnEBRSaveRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeEBR recipe = GlobalObjects.Instance.Get<RecipeEBR>();
                    if (recipe.RecipePath != "")
                    {
                        recipe.Save(recipe.RecipePath);
                    }
                    else
                    {
                        System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                        dlg.InitialDirectory = Constants.Path.RecipeEBRRootPath;
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

                });
            }
        }

        public ICommand btnEBRLoadRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                    dlg.InitialDirectory = Constants.Path.RecipeEBRRootPath;
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

                        RecipeEBR recipe = GlobalObjects.Instance.Get<RecipeEBR>();
                        recipe.Read(sFullPath);

                        UpdateUI();
                    }
                });
            }
        }

        public void UpdateUI()
        {
            SetupVM.LoadParameter();
        }
    }
}
