using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_CAMELLIA
{
    public class SequenceRecipe_ViewModel : ObservableObject
    {
        MainWindow_ViewModel main;

        CAMELLIA_Recipe m_recipe;
        public CAMELLIA_Recipe p_recipe
        {
            get
            {
                return m_recipe;
            }
            set
            {
                SetProperty(ref m_recipe, value);
            }
        }

        List<string> m_moduleList = new List<string>();
        public List<string> p_moduleList
        {
            get
            {
                return m_moduleList;
            }
            set
            {
                SetProperty(ref m_moduleList, value);
            }
        }

        //ModuleList m_moduleList;
        //public ModuleList p_moduleList
        //{
        //    get 
        //    {
        //        return m_moduleList;
        //    }
        //    set
        //    {
        //        SetProperty(ref m_moduleList, value);
        //    }
        //}

        private ModuleRunList m_moduleRunList;

        public ModuleRunList p_moduleRunList
        {
            get 
            {
                return m_moduleRunList; 
            }
            set
            {
                SetProperty(ref m_moduleRunList, value);
            }
        }

        private ModuleRunList m_moduleTempList;
        public ModuleRunList p_moduleTempList
        {
            get
            {
                return m_moduleTempList;
            }
            set
            {
                SetProperty(ref m_moduleTempList, value);
            }
        }

        private string m_selectModule;
        public string p_selectModule
        {
            get 
            {
                return m_selectModule;
            }
            set 
            {
                SetProperty(ref m_selectModule, value);
                p_aModuleRun = p_moduleTempList.GetRecipeRunNames(value);
            }
        }

        private List<string> m_aModuleRun;

        public List<string> p_aModuleRun
        {
            get 
            {
                return m_aModuleRun;
            }
            set 
            {
                SetProperty(ref m_aModuleRun, value);
            }
        }


        private string m_selectModuleRun;

        public string p_selectModuleRun
        {
            get 
            {
                return m_selectModuleRun;
            }
            set 
            {
                SetProperty(ref m_selectModuleRun, value);
                if(value != null)
                {
                    p_addVisibility = Visibility.Visible;
                }
                else{
                    p_addVisibility = Visibility.Hidden;
                }
            }
        }

        Visibility m_addVisibility = Visibility.Hidden;
        public Visibility p_addVisibility
        {
            get
            {
                return m_addVisibility;
            }
            set
            {
                SetProperty(ref m_addVisibility, value);
            }
        }



        public SequenceRecipe_ViewModel(MainWindow_ViewModel main)
        {
            this.main = main;
            p_recipe = App.m_engineer.m_handler.m_recipe;
            //m_moduleList = App.m_engineer.m_handler.m_moduleList;
            p_moduleRunList = p_recipe.m_moduleRunList;
            p_moduleTempList = p_moduleRunList.Copy();
            InitModuleList();
        }

        #region Command
        public ICommand CmdAddModuleRun
        {
            get
            {
                return new RelayCommand(() =>
                {
                    AddModuleRun();
                });
            }
        }

        public ICommand CmdClearModuleRun
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ClearModuleRun();
                });
            }
        }
        #endregion

        #region Function

        void AddModuleRun()
        {
            p_moduleTempList.Add(p_selectModule, p_selectModuleRun);
            p_moduleTempList.RunTree(Tree.eMode.Init);
        }

        void ClearModuleRun()
        {
            p_moduleTempList.Clear();
            p_moduleTempList.RunTree(Tree.eMode.Init);
        }
        void InitModuleList()
        {
            List<string> temp = new List<string>();
            foreach (string val in p_recipe.m_asModule)
            {
                
                if(p_moduleTempList.GetRecipeRunNames(val).Count != 0)
                {
                    temp.Add(val);
                }
            }

            p_moduleList = temp;
        }

        public void CreateRecipe(string fileName)
        {
            p_moduleTempList.Clear();
            p_moduleTempList.RunTree(Tree.eMode.Init);

            SaveRecipe(fileName);
        } 

        public void SaveRecipe(string fileName)
        {
            p_moduleRunList.p_aModuleRun = p_moduleTempList.CopyModuleRun();
            p_moduleRunList.SaveJob(fileName);
            p_moduleRunList.RunTree(Tree.eMode.Init);
        }

        public void SaveAsRecipe()
        {
            
        }

        public void LoadRecipe(string fileName)
        {
            p_moduleTempList.OpenJob(fileName);
            p_moduleTempList.RunTree(Tree.eMode.Init);
            //p_moduleRunList.OpenJob(fileName);
            //p_moduleRunList.RunTree(Tree.eMode.Init);
        }

        #endregion
    }
}
