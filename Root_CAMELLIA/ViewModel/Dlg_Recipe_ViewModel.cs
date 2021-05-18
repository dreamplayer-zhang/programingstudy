using Microsoft.Win32;
using Root_CAMELLIA.Data;
using RootTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_CAMELLIA
{
    public class Dlg_Recipe_ViewModel : ObservableObject, IDialogRequestClose
    {
        MainWindow_ViewModel main;
        Dlg_RecipeManager_ViewModel recipeManager_ViewModel;
        SequenceRecipe_ViewModel sequenceRecipe_ViewModel;
        public Dlg_Recipe_ViewModel(MainWindow_ViewModel main)
        {
            this.main = main;
            this.sequenceRecipe_ViewModel = main.SequenceViewModel;
            this.recipeManager_ViewModel = main.RecipeViewModel;
            p_DataContext = sequenceRecipe_ViewModel;
        }

        public enum RecipeMode
        {
            Sequence,
            Measure
        }

        RecipeMode m_currentMode = RecipeMode.Sequence;

        bool m_isSequenceRecipe = true;
        public bool p_isSequenceRecipe
        {
            get
            {
                return m_isSequenceRecipe;
            }
            set
            {
                SetProperty(ref m_isSequenceRecipe, value);
            }
        }

        bool m_isMeasureRecipe = false;
        public bool p_isMeasureRecipe
        {
            get
            {
                return m_isMeasureRecipe;
            }
            set
            {
                SetProperty(ref m_isMeasureRecipe, value);
            }
        }

        object m_DataContext;
        public object p_DataContext
        {
            get
            {
                return m_DataContext;
            }
            set
            {
                SetProperty(ref m_DataContext, value);
            }
        }

        string m_recipePath = "";
        public string p_recipePath
        {
            get
            {
                return m_recipePath;
            }
            set
            {
                SetProperty(ref m_recipePath, value);
            }
        }

        string m_displayRecipePath = "";
        public string p_displayRecipePath
        {
            get
            {
                return m_displayRecipePath;
            }
            set
            {
                SetProperty(ref m_displayRecipePath, value);
            }
        }

        bool m_isEnableSave = false;
        public bool p_isEnableSave
        {
            get
            {
                return m_isEnableSave;
            }
            set
            {
                SetProperty(ref m_isEnableSave, value);
            }
        }

        double m_opacity = 0.0;
        public double p_opacity
        {
            get
            {
                return m_opacity;
            }
            set
            {
                SetProperty(ref m_opacity, value);
            }
        }



        public ICommand CmdChangeMode
        {
            get
            {
                return new RelayCommand(() =>
                {
                    p_opacity = 1.0;
                    if (p_isSequenceRecipe && m_currentMode != RecipeMode.Sequence)
                    {
                        p_DataContext = sequenceRecipe_ViewModel;
                        m_currentMode = RecipeMode.Sequence;
                    }
                    else if(p_isMeasureRecipe && m_currentMode != RecipeMode.Measure)
                    {

                        p_DataContext = recipeManager_ViewModel;
                        m_currentMode = RecipeMode.Measure;
                        //StopWatch sw = new StopWatch();

                        //sw.Start();
                        //bool open = main.RecipeOpen;
                        //recipeManager_ViewModel.UpdateListView(open);
                        //sw.Stop();
                        //System.Diagnostics.Debug.WriteLine("UpdateListView " + sw.ElapsedMilliseconds);

                        //sw.Start();
                        //try
                        //{
                        //    recipeManager_ViewModel.UpdateLayerGridView();
                        //}
                        //catch
                        //{

                        //}
                        //sw.Stop();
                        //System.Diagnostics.Debug.WriteLine("UpdateLayer " + sw.ElapsedMilliseconds);

                        //sw.Start();
                        //recipeManager_ViewModel.UpdateView(open);
                        //p_DataContext = recipeManager_ViewModel;
                        //sw.Stop();
                        //System.Diagnostics.Debug.WriteLine("UpdateView " + sw.ElapsedMilliseconds);


                    }
                    SetDisplayPath();
                    p_opacity = 0.0;
                });
            }
        }

        public ICommand CmdClose
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (p_isEnableSave && !recipeManager_ViewModel.CheckSaveLayerData())
                    {
                        return;
                    }
                    else
                    {
                        CloseRequested(this, new DialogCloseRequestedEventArgs(false));
                    }
                });
            }
        }

        public ICommand UnloadedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    recipeManager_ViewModel.Unloaded();
                });
            }
        }

        public ICommand LoadedCommnad
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }

        //string m_sPath = "c:\\Recipe\\CAMELLIA2\\";
        public ICommand CmdRecipeCreate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    string sModel = EQ.m_sModel;
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.InitialDirectory = BaseDefine.Dir_SequenceInitialPath;
                    dlg.Filter = sModel + " Recipe (." + sModel + ")|*." + sModel;
                    if (dlg.ShowDialog() == true)
                    {
                        string filename = dlg.FileName;
                        string path = dlg.FileName.Replace(Path.GetExtension(filename), "");
                        Directory.CreateDirectory(path);
                        string file = Path.GetFileName(dlg.FileName);
                        path += "\\" + file.Replace(Path.GetExtension(file), ".aco");
                        sequenceRecipe_ViewModel.CreateRecipe(filename);
                        recipeManager_ViewModel.CreateRecipe(path);
                        p_recipePath = filename.Replace(Path.GetExtension(path), "");
                        p_isEnableSave = true;

                        SetDisplayPath();
                    }


                    //if (dlg.ShowDialog() == true) sequenceRecipe_ViewModel.p_moduleRunList.SaveJob(dlg.FileName);
                    //m_recipe.m_moduleRunList.RunTree(Tree.eMode.Init);

                });
            }
        }

        public ICommand CmdRecipeLoad
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeLoad();
                });
            }
        }

        public ICommand CmdRecipeSave
        {
            get
            {
                return new RelayCommand(() =>
                {
                    string InitPath = BaseDefine.Dir_SequenceInitialPath;
                    if (!Directory.Exists(InitPath))
                    {
                        Directory.CreateDirectory(InitPath);
                    }
                    if (p_isEnableSave)
                    {
                       // string path = p_recipePath.Replace(Path.GetFileName(p_recipePath), "");
                       
                        //path += Path.GetFileName(p_recipePath);
                        sequenceRecipe_ViewModel.SaveRecipe(p_recipePath);
                        recipeManager_ViewModel.SaveRecipe();
                    }
                    
                    //m_recipe.m_moduleRunList.RunTree(Tree.eMode.Init);
                });
            }
        }

        public ICommand CmdRecipeSaveAs
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeSaveAs();
                });
            }
        }

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        #region Function
        public bool RecipeSaveAs()
        {
            string InitPath = BaseDefine.Dir_SequenceInitialPath;
            string sModel = EQ.m_sModel;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = InitPath;
            dlg.Filter = sModel + " Recipe (." + sModel + ")|*." + sModel;
            if (dlg.ShowDialog() == true)
            {
                //string path = dlg.FileName.Replace(Path.GetExtension(dlg.FileName), "");
                //Directory.CreateDirectory(path);
                //sequenceRecipe_ViewModel.SaveRecipe(path);
                //path += "\\" + Path.GetFileName(dlg.FileName);
                //recipeManager_ViewModel.SaveRecipe(path);

                string filename = dlg.FileName;
                string path = dlg.FileName.Replace(Path.GetExtension(filename), "");
                Directory.CreateDirectory(path);
                string file = Path.GetFileName(dlg.FileName);
                path += "\\" + file.Replace(Path.GetExtension(file), ".aco");
                sequenceRecipe_ViewModel.SaveRecipe(filename);
                recipeManager_ViewModel.SaveRecipe(path);
                
                return true;
            }
            return false;
        }
        public bool RecipeLoad()
        {
            string sModel = EQ.m_sModel;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = BaseDefine.Dir_SequenceInitialPath;
            dlg.Filter = sModel + " Recipe (." + sModel + ")|*." + sModel;
            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;
                sequenceRecipe_ViewModel.LoadRecipe(filename);

                string path = dlg.FileName.Replace(Path.GetExtension(filename), "");
                string file = Path.GetFileName(dlg.FileName);
                path += "\\" + file.Replace(Path.GetExtension(file), ".aco");
                recipeManager_ViewModel.LoadRecipe(path);
          
                p_recipePath = dlg.FileName;
                p_isEnableSave = true;
                return true;
            }
            return false;
        }

        void SetDisplayPath()
        {
            if(m_currentMode == RecipeMode.Measure)
            {
                string fileName = Path.GetFileName(p_recipePath);
                string path = p_recipePath.Replace(Path.GetExtension(p_recipePath), "");
                fileName = fileName.Replace(Path.GetExtension(fileName), ".aco");
                p_displayRecipePath = path + "\\" + fileName;
            }
            else
            {
                p_displayRecipePath = p_recipePath;
            }
        }
        #endregion
    }
}
