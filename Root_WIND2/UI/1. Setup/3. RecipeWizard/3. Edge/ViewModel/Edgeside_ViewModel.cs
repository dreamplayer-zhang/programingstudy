using RootTools;
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
	class Edgeside_ViewModel : ObservableObject
	{
		private Setup_ViewModel setupVM;

		public Edgeside_Panel Main;
		public EdgesideSetup_ViewModel SetupVM;
		public EdgesideSetup SetupPage;

		public Edgeside_ViewModel(Setup_ViewModel setup)
		{
			this.setupVM = setup;
			Init();
		}

		public void Init()
		{
			Main = new Edgeside_Panel();
			SetupVM = new EdgesideSetup_ViewModel();
			SetupVM.Init(setupVM);

			SetupPage = new EdgesideSetup();
			SetupPage.DataContext = SetupVM;
			SetPage(SetupPage);
		}

		public ICommand btnEdgeSetup
		{
			get
			{
				return new RelayCommand(() => SetPage(SetupPage));
			}
		}

		public ICommand btnEdgeSnap
		{
			get
			{
				return new RelayCommand(() => SetupVM.Scan());
			}
		}

		public ICommand btnEdgeInsp
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
		public RelayCommand btnWaferLoad
        {
            get
            {
				return new RelayCommand(btnWaferLoadClick);
            }
        }

        public ICommand btnEdgeNewRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();

                    System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                    dlg.InitialDirectory = Constants.RootPath.RecipeEdgeRootPath;
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

        public ICommand btnEdgeSaveRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
                    if (recipe.RecipePath != "")
                    {
                        recipe.Save(recipe.RecipePath);
                    }
                    else
                    {
                        System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                        dlg.InitialDirectory = Constants.RootPath.RecipeEdgeRootPath;
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

        public ICommand btnEdgeLoadRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                    dlg.InitialDirectory = Constants.RootPath.RecipeEdgeRootPath;
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

                        RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();
                        recipe.Read(sFullPath);

                        UpdateUI();
                    }
                });
            }
        }

        private void btnWaferLoadClick()
        {
			setupVM.maintVM.HandlerUI.GetModuleList_UI().ModuleListRunOpen();
			setupVM.maintVM.HandlerUI.GetModuleList_UI().ModuleListRun();
        }

		private void SetPage(UserControl page)
		{
			Main.SubPanel.Children.Clear();
			Main.SubPanel.Children.Add(page);
		}

		public void UpdateUI()
		{
            SetupVM.LoadParameter();
        }

	}
}
