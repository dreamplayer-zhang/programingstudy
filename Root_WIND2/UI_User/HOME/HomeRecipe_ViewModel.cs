using Root_EFEM.Module;
using Root_WIND2.Module;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Root_WIND2.UI_User
{
    public class HomeRecipe_ViewModel : ObservableObject
    {
        #region [Properties]

        private RecipeSelectionViewer_ViewModel recipeSelectionViewerVM = new RecipeSelectionViewer_ViewModel();
        public RecipeSelectionViewer_ViewModel RecipeSelectionViewerVM
        {
            get => this.recipeSelectionViewerVM;
            set
            {
                SetProperty(ref this.recipeSelectionViewerVM, value);
            }
        }

        private SequenceRecipe_ViewModel sequenceRecipeVM = new SequenceRecipe_ViewModel();
        public SequenceRecipe_ViewModel SequenceRecipeVM
        {
            get => sequenceRecipeVM;
            set
            {
                SetProperty(ref sequenceRecipeVM, value);
            }
        }


        private ObservableCollection<ModuleView> moduleList = new ObservableCollection<ModuleView>();
        public ObservableCollection<ModuleView> ModuleList
        {
            get => this.moduleList;
            set
            {
                SetProperty(ref moduleList, value);
            }
        }

        private ObservableCollection<ModuleView_ViewModel> moduleViewModels = new ObservableCollection<ModuleView_ViewModel>();
        public ObservableCollection<ModuleView_ViewModel> ModuleViewModels
        {
            get => this.moduleViewModels;
            set
            {
                SetProperty(ref moduleViewModels, value);
            }
        }
        #endregion

        Vision vision = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_Vision;
        BackSideVision backside = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_BackSideVision;
        EdgeSideVision edgeside = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision;
        ModuleBase aligner =  (((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_Aligner);

        WIND2_Recipe m_recipe;
        ModuleRunList m_moduleRunList;

        

        public HomeRecipe_ViewModel()
        {
            m_recipe = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).m_recipe;
            m_moduleRunList = m_recipe.p_moduleRunList;
            m_moduleRunList.Add(vision.p_id, "GrabLineScan");
            m_moduleRunList.RunTree(Tree.eMode.Init);

            ModuleList.Add(new ModuleView());
            ModuleList.Add(new ModuleView());
            ModuleList.Add(new ModuleView());

            ModuleView_ViewModel model = new ModuleView_ViewModel(vision);
            //model.AddMode("OnlySnap", vision.GetModuleruns());
            //model.AddMode("Alignment", vision.GetModuleruns());
            //model.AddMode("Inspection", vision.GetModuleruns());
            ModuleViewModels.Add(model);
            model = new ModuleView_ViewModel(backside);
            //model.AddMode("OnlySnap", backside.GetModuleruns());
            //model.AddMode("Inspection", backside.GetModuleruns());
            ModuleViewModels.Add(model);
            model = new ModuleView_ViewModel(edgeside);
            //model.AddMode("OnlySnap", edgeside.GetModuleruns());
            //model.AddMode("Inspection", edgeside.GetModuleruns());
            ModuleViewModels.Add(model);

            for (int i = 0; i < moduleList.Count; i++)
            {
                moduleList[i].DataContext = ModuleViewModels[i];
            }

            RecipeSelectionViewerVM.RecipeSelected += RecipeSelect;
            RecipeSelectionViewerVM.RecipeCreated += RecipeCreated;
        }

        #region Event
        public void RecipeSelect(string path)
        {
            string sPath = path + "\\" + "Front";
            //string sFolderPath = Path.GetDirectoryName(sPath); // 디렉토리명

            string[] sFilePath = path.Split('\\');
            string sFileNameNoExt = sFilePath[sFilePath.Length - 1]; // Only 파일이름

            string sequencePath = path + "\\" + sFileNameNoExt + "." + EQ.m_sModel;
            SequenceRecipeVM.LoadRecipe(sequencePath);

            string sFileName = Path.GetFileName(path) + ".rcp"; // 파일이름 + 확장자
            string sRecipeFolderPath = sPath; // 디렉토리명
            string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            recipe.Clear();

            recipe.Name = sFileNameNoExt;
            recipe.RecipePath = sFullPath;
            recipe.RecipeFolderPath = sRecipeFolderPath;

            recipe.Read(sFullPath);

            sRecipeFolderPath = path + "\\" + "Back";
            sFullPath = Path.Combine(sRecipeFolderPath, sFileName);

            RecipeBack recipeBack = GlobalObjects.Instance.Get<RecipeBack>();
            recipeBack.Clear();

            recipeBack.Name = sFileNameNoExt;
            recipeBack.RecipePath = sFullPath;
            recipeBack.RecipeFolderPath = sRecipeFolderPath;

            recipeBack.Read(sFullPath);

            sRecipeFolderPath = path + "\\" + "EBR";
            sFullPath = Path.Combine(sRecipeFolderPath, sFileName);

            RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
            recipeEBR.Clear();

            recipeEBR.Name = sFileNameNoExt;
            recipeEBR.RecipePath = sFullPath;
            recipeEBR.RecipeFolderPath = sRecipeFolderPath;

            recipeEBR.Read(sFullPath);

            sRecipeFolderPath = path + "\\" + "Edge";
            sFullPath = Path.Combine(sRecipeFolderPath, sFileName);
            RecipeEdge recipeEdge = GlobalObjects.Instance.Get<RecipeEdge>();
            recipeEdge.Clear();

            recipeEdge.Name = sFileNameNoExt;
            recipeEdge.RecipePath = sFullPath;
            recipeEdge.RecipeFolderPath = sRecipeFolderPath;

            recipeEdge.Read(sFullPath);


            WIND2EventManager.OnRecipeUpdated(this, new RecipeEventArgs());
        }

        public void RecipeCreated(string path)
        {
            CreateRecipe(path);
        }

        void SaveRecipe()
        {
            string path = RecipeSelectionViewerVM.CurrentOpenPath;

            if (path != null){
                string sPath = path + "\\" + "Front";
                //string sFolderPath = Path.GetDirectoryName(sPath); // 디렉토리명

                string[] sFilePath = path.Split('\\');
                string sFileNameNoExt = sFilePath[sFilePath.Length - 1]; // Only 파일이름

                string sequencePath = path + "\\" + sFileNameNoExt + "." + EQ.m_sModel;
                SequenceRecipeVM.SaveRecipe(sequencePath);

                string sFileName = Path.GetFileName(path) + ".rcp"; // 파일이름 + 확장자
                string sRecipeFolderPath = sPath; // 디렉토리명
                string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
                recipe.Name = sFileNameNoExt;
                recipe.RecipePath = sFullPath;
                recipe.RecipeFolderPath = sRecipeFolderPath;

                recipe.Save(sFullPath);

                sRecipeFolderPath = path + "\\" + "Back";
                sFullPath = Path.Combine(sRecipeFolderPath, sFileName);

                RecipeBack recipeBack = GlobalObjects.Instance.Get<RecipeBack>();

                recipeBack.Name = sFileNameNoExt;
                recipeBack.RecipePath = sFullPath;
                recipeBack.RecipeFolderPath = sRecipeFolderPath;

                recipeBack.Save(sFullPath);

                sRecipeFolderPath = path + "\\" + "EBR";
                sFullPath = Path.Combine(sRecipeFolderPath, sFileName);

                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();

                recipeEBR.Name = sFileNameNoExt;
                recipeEBR.RecipePath = sFullPath;
                recipeEBR.RecipeFolderPath = sRecipeFolderPath;

                recipeEBR.Save(sFullPath);

                sRecipeFolderPath = path + "\\" + "Edge";
                sFullPath = Path.Combine(sRecipeFolderPath, sFileName);
                RecipeEdge recipeEdge = GlobalObjects.Instance.Get<RecipeEdge>();

                recipeEdge.Name = sFileNameNoExt;
                recipeEdge.RecipePath = sFullPath;
                recipeEdge.RecipeFolderPath = sRecipeFolderPath;

                recipeEdge.Save(sFullPath);

                MessageBox.Show("Save Done!", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (RecipeSelectionViewerVM.CheckPath())
                {
                    RecipeSelectionViewerVM.CreateStepFolder();
                }
                else
                {
                    MessageBox.Show("Please Create PartID Folder", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
        }

        void CreateRecipe(string path)
        {
            string sPath = path +"\\" + "Front";
            //string sFolderPath = Path.GetDirectoryName(sPath); // 디렉토리명

            string[] sFilePath = path.Split('\\');
            string sFileNameNoExt = sFilePath[sFilePath.Length - 1]; // Only 파일이름

            string sequencePath = path + "\\" + sFileNameNoExt + "." + EQ.m_sModel;
            SequenceRecipeVM.CreateRecipe(sequencePath);

            string sFileName = Path.GetFileName(path) + ".rcp"; // 파일이름 + 확장자
            string sRecipeFolderPath = sPath; // 디렉토리명
            string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

            //DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
            //if (!dir.Exists)
            //    dir.Create();

            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            recipe.Clear();

            recipe.Name = sFileNameNoExt;
            recipe.RecipePath = sFullPath;
            recipe.RecipeFolderPath = sRecipeFolderPath;

            recipe.Save(sFullPath);

            sRecipeFolderPath = path + "\\" + "Back";
            sFullPath = Path.Combine(sRecipeFolderPath, sFileName);

            RecipeBack recipeBack = GlobalObjects.Instance.Get<RecipeBack>();
            recipeBack.Clear();

            recipeBack.Name = sFileNameNoExt;
            recipeBack.RecipePath = sFullPath;
            recipeBack.RecipeFolderPath = sRecipeFolderPath;

            recipeBack.Save(sFullPath);

            sRecipeFolderPath = path + "\\" + "EBR";
            sFullPath = Path.Combine(sRecipeFolderPath, sFileName);

            RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
            recipeEBR.Clear();

            recipeEBR.Name = sFileNameNoExt;
            recipeEBR.RecipePath = sFullPath;
            recipeEBR.RecipeFolderPath = sRecipeFolderPath;

            recipeEBR.Save(sFullPath);

            sRecipeFolderPath = path + "\\" + "Edge";
            sFullPath = Path.Combine(sRecipeFolderPath, sFileName);
            RecipeEdge recipeEdge = GlobalObjects.Instance.Get<RecipeEdge>();
            recipeEdge.Clear();

            recipeEdge.Name = sFileNameNoExt;
            recipeEdge.RecipePath = sFullPath;
            recipeEdge.RecipeFolderPath = sRecipeFolderPath;

            recipeEdge.Save(sFullPath);

            RecipeSelectionViewerVM.CurrentOpenPath = path;

            MessageBox.Show("Save Done!","Save",MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region [Command]
        public ICommand LoadedCommand
        {
            get => new RelayCommand(() =>
            {
                this.RecipeSelectionViewerVM.RefreshProductItemList();
            });
        }

        public ICommand CmdNew
        {
            get => new RelayCommand(() =>
            {
                RecipeSelectionViewerVM.CreateStepFolder();
            });
        }

        public ICommand CmdSave
        {
            get => new RelayCommand(() =>
            {
                SaveRecipe();
            });
        }

        public ICommand CmdOpen
        {
            get => new RelayCommand(() =>
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.InitialDirectory = recipeSelectionViewerVM.CurrentPath;
                dlg.Filter = EQ.m_sModel + "Files (*." + EQ.m_sModel + ")|*." + EQ.m_sModel;

                if (dlg.ShowDialog() == false) return;
                string file = Path.GetFileName(dlg.FileName);
                string path = dlg.FileName.Replace("\\" + file, "");
                RecipeSelect(path);
            });
        }
        #endregion
    }
}
