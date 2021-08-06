using Microsoft.Win32;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_WindII
{
    public class RACRecipeCreateViewer_ViewModel : ObservableObject
    {
        #region [PROPERTY]
        string RecipeRootPath = Constants.RootPath.RecipeRACRootPath;

        string baseRecipeName = "";
        public string BaseRecipeName
        {
            get => baseRecipeName;
            set
            {
                SetProperty(ref baseRecipeName, value);
            }
        }


        string xmlFileName = "";
        public string XMLFileName
        {
            get => xmlFileName;
            set
            {
                SetProperty(ref xmlFileName, value);
            }
        }


        string createRecipeName = "";
        public string CreateRecipeName
        {
            get => createRecipeName;
            set
            {
                SetProperty(ref createRecipeName, value);
                Refresh();
            }
        }

        ObservableCollection<string> stepList = new ObservableCollection<string>();
        public ObservableCollection<string> StepList
        {
            get => stepList;
            set
            {
                SetProperty(ref stepList, value);
            }
        }

        private RecipeBase recipeFront = new RecipeBase();
        #endregion
        #region [COMMAND]
        public ICommand CmdLoaded
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Refresh();
                });
            }
        }
        public ICommand CmdUnloaded
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }
        public ICommand CmdOpenBaseRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (OpenBaseRecipe())
                    {
                    }
                });
            }
        }

        public ICommand CmdDelete
        {
            get
            {
                return new RelayCommand(() =>
                {
                    int t = 10;
                });
            }
        }

        public ICommand CmdCreate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    int t = 10;
                });
            }
        }
        #endregion

        #region [FUNCTION]
        bool OpenBaseRecipe()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Recipe Files (*.rcp)|*.rcp";
            dlg.InitialDirectory = RecipeRootPath;
            if (dlg.ShowDialog() == true)
            {
                BaseRecipeName = Path.GetFileNameWithoutExtension(dlg.FileName);
                recipeFront.Read(dlg.FileName);
                recipeFront.CloneBase();
            }
            return true;
        }

        public void Refresh()
        {
            //FileInfoList.Clear();

            if (XMLFileName == "")
                return;

            DirectoryInfo di = new DirectoryInfo(RecipeRootPath + XMLFileName);
            if (!Directory.Exists(RecipeRootPath + XMLFileName))
                di.Create();

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    if (!Path.GetExtension(file.Name).Contains(".rcp"))
                        continue;
                    string fileName = file.Name.ToLower();
                    if (fileName.Contains(this.CreateRecipeName.ToLower()))
                    {
                        StepList.Add("tettt");
                        //FileInfoList.Add(new ListFileInfo(Path.GetFileNameWithoutExtension(file.Name), file.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"), file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")));
                    }
                }
            });
        }
        #endregion

        #region StepListInfoClass
        public class StepInfoList
        {

        }
        #endregion
    }
}
