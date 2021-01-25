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
    class Backside_ViewModel : ObservableObject
    {
        public Backside_Panel Main;
        public BacksideSetup Setup;
        public BacksideROI ROI;
        public BacksideInspection Inspection;


        private BacksideInspection_ViewModel m_BacksideInspection_VM;
        public BacksideInspection_ViewModel p_BacksideInspection_VM
        {
            get
            {
                return m_BacksideInspection_VM;
            }
            set
            {
                SetProperty(ref m_BacksideInspection_VM, value);
            }
        }

        private BacksideSetup_ViewModel m_BacksideSetup_VM;
        public BacksideSetup_ViewModel p_BacksideSetup_VM
        {
            get
            {
                return m_BacksideSetup_VM;
            }
            set
            {
                SetProperty(ref m_BacksideSetup_VM, value);
            }
        }
        private BacksideROI_ViewModel m_BacksideROI_VM;
        public BacksideROI_ViewModel p_BacksideROI_VM
        {
            get
            {
                return m_BacksideROI_VM;
            }
            set
            {
                SetProperty(ref m_BacksideROI_VM, value);
            }
        }


        Setup_ViewModel m_Setup;
        public Backside_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;

            p_BacksideSetup_VM = new BacksideSetup_ViewModel();

            p_BacksideROI_VM = new BacksideROI_ViewModel();
            p_BacksideROI_VM.init(m_Setup);

            p_BacksideInspection_VM = new BacksideInspection_ViewModel();
            p_BacksideInspection_VM.init(m_Setup);

            init();

        }
        public void init()
        {
            Main = new Backside_Panel();
            Setup = new BacksideSetup();
            ROI = new BacksideROI();
            Inspection = new BacksideInspection();

            SetPage(ROI);
            SetPage(Setup);
            SetPage(Inspection);
        }
        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        
        public ICommand btnBackSetup
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(Setup);
                });
            }
        }
        public ICommand btnBackROI
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(ROI);
                });
            }
        }
        public ICommand btnBackInspection
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(Inspection);
                    m_BacksideInspection_VM.SetPage(Inspection);
                });
            }
        }
        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.SetRecipeWizard();
                });
            }
        }

        public ICommand btnBackNewRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeBack recipe = GlobalObjects.Instance.Get<RecipeBack>();

                    System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                    dlg.InitialDirectory = Constants.Path.RecipeBackRootPath;
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

        public ICommand btnBackSaveRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeBack recipe = GlobalObjects.Instance.Get<RecipeBack>();
                    if (recipe.RecipePath != "")
                    {
                        recipe.Save(recipe.RecipePath);
                    }
                    else
                    {
                        System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                        dlg.InitialDirectory = Constants.Path.RecipeBackRootPath;
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

        public ICommand btnBackLoadRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                    dlg.InitialDirectory = Constants.Path.RecipeBackRootPath;
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

                        RecipeBack recipe = GlobalObjects.Instance.Get<RecipeBack>();
                        recipe.Read(sFullPath);
                    }
                });
            }
        }
    }
}
