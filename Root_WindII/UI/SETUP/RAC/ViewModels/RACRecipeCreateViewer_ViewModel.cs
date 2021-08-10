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

        string xmlFilePath = "";
        public string XMLFilePath
        {
            get => xmlFilePath;
            set
            {
                SetProperty(ref xmlFilePath, value);
            }
        }


        string createStepName = "";
        public string CreateStepName
        {
            get => createStepName;
            set
            {
                SetProperty(ref createStepName, value);
                //Refresh();
            }
        }

        object selectItem;
        public object SelectItem
        {
            get => selectItem;
            set
            {
                SetProperty(ref selectItem, value);
            }
        }

        int selectStepIndex = -1;
        public int SelectStepIndex
        {
            get => selectStepIndex;
            set
            {
                SetProperty(ref selectStepIndex, value);
            }
        }

        ObservableCollection<StepInfoList> stepList = new ObservableCollection<StepInfoList>();
        public ObservableCollection<StepInfoList> StepList
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
                    if(SelectStepIndex != -1)
                    {
                        StepList.RemoveAt(selectStepIndex);
                        SelectStepIndex = -1;
                    }
                    //for(int i = 0; i < StepList.Count; i++)
                    //{
                    //    StepList[i].
                    //}
                });
            }
        }

        public ICommand CmdCreate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CreateRecipe();
                });
            }
        }
        
        public ICommand CmdAdd
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (CreateStepName == "")
                        return;
                    StepList.Add(new StepInfoList(CreateStepName)); 
                });
            }
        }
        #endregion

        #region [FUNCTION]

        void CreateRecipe()
        {
            if (BaseRecipeName == "")
            {
                MessageBox.Show("Select Base Recipe","Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (XMLFilePath == "")
            {
                MessageBox.Show("Select XML Data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<RecipeType_Mask> maskList = recipeFront.GetItem<MaskRecipe>().MaskList;
            XMLData data = GlobalObjects.Instance.Get<XMLData>();

            XMLParser.ParseMapInfo(XMLFilePath, data);
            for (int i = 0; i < stepList.Count; i++)
            {
                RecipeFront recipe = new RecipeFront();
                foreach (RecipeType_Mask mask in maskList)
                {
                    recipe.GetItem<MaskRecipe>().AddMask(mask);
                }
                recipe.GetItem<MaskRecipe>().OriginPoint = recipeFront.GetItem<MaskRecipe>().OriginPoint;

                recipe.GetItem<OriginRecipe>().DiePitchX = recipeFront.GetItem<OriginRecipe>().DiePitchX;
                recipe.GetItem<OriginRecipe>().DiePitchY = recipeFront.GetItem<OriginRecipe>().DiePitchY;

                recipe.GetItem<OriginRecipe>().OriginWidth = recipeFront.GetItem<OriginRecipe>().OriginWidth;
                recipe.GetItem<OriginRecipe>().OriginHeight = recipeFront.GetItem<OriginRecipe>().OriginHeight;

                recipe.GetItem<OriginRecipe>().OriginX = recipeFront.GetItem<OriginRecipe>().OriginX;
                recipe.GetItem<OriginRecipe>().OriginY = recipeFront.GetItem<OriginRecipe>().OriginY;
                //recipe.GetItem<OriginRecipe>().OriginX = recipeFront.GetItem<MaskRecipe>().;

                recipe.WaferMap = new RecipeType_WaferMap(data.MapSizeX, data.MapSizeY, data.GetWaferMap());

                string path = RecipeRootPath + XMLFileName + "\\" + XMLFileName + "." + stepList[i].Step + "\\";

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                recipe.Save(path + XMLFileName + "." + stepList[i].Step + ".rcp");
            }

        }
        bool OpenBaseRecipe()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Recipe Files (*.rcp)|*.rcp";
            dlg.InitialDirectory = RecipeRootPath;
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    BaseRecipeName = Path.GetFileNameWithoutExtension(dlg.FileName);
                    recipeFront.Read(dlg.FileName);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                //recipeFront.GetItem<MaskRecipe>().MaskList;
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
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        if (!Path.GetExtension(file.Name).Contains(".rcp"))
                            continue;
                        string fileName = file.Name.ToLower();
                        if (fileName.Contains(this.CreateStepName.ToLower()))
                        {
                            //StepList.Add("tettt");
                            //FileInfoList.Add(new ListFileInfo(Path.GetFileNameWithoutExtension(file.Name), file.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"), file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")));
                        }
                    }
                }
            });
        }
        #endregion

        #region StepListInfoClass
        public class StepInfoList
        {
            public string Step { get; private set; }
            //public string Master { get; private set; }
            //public string Slave { get; private set; }
            //public string ROI { get; private set; }
            //public string Side { get; private set; }
            //public StepInfoList(string master, string slave, string roi, string side)
            //{
            //    this.Master = master;
            //    this.Slave = slave;
            //    this.ROI = roi;
            //    this.Side = side;
            //}
            public StepInfoList(string step)
            {
                this.Step = step;
            }
        }
        #endregion
    }
}
