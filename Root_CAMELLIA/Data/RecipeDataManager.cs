using Microsoft.Win32;
using NanoView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RootTools;

namespace Root_CAMELLIA.Data
{
    public class RecipeDataManager
    {
        public event EventHandler RecipeLoaded;
        private DataManager dataManager { get; set; }
        public PresetData PresetData { get; set; }
        public RecipeData TeachingRD { get; set; }
        public RecipeData SaveRecipeRD { get; set; }
        public RecipeData MeasurementRD { get; set; }
        public ModelData SaveModelData { get; set; }
        public ModelData MeasureModelData { get; set; }
        public string TeachingRecipePath { get; set; }
        public string TeachRecipeName { get; set; } = "";
        public string LoadRecipePath { get; set; }
        public string LoadRecipeName { get; set; } = "";
        public RecipeDataManager(DataManager DM)
        {
            dataManager = DM;

            PresetData = new PresetData();
            TeachingRD = new RecipeData();
            SaveRecipeRD = new RecipeData();
            MeasurementRD = new RecipeData();
            SaveModelData = new ModelData();
            MeasureModelData = new ModelData();
        }
        public bool RecipeNew()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = "aco";
            dialog.Filter = "*.aco|*.aco";
            dialog.InitialDirectory = BaseDefine.Dir_Recipe;
            if (dialog.ShowDialog() == true)
            {
                TeachingRecipePath = dialog.FileName;
                string strTeachingRecipeName = Path.GetFileName(dialog.FileName);
                TeachRecipeName = strTeachingRecipeName.Remove(strTeachingRecipeName.Length - 4);
                dataManager.recipeDM.TeachingRD = null;
                dataManager.recipeDM.TeachingRD = new RecipeData();

                dataManager.recipeDM.TeachingRD.Clone(dataManager.recipeDM.SaveRecipeRD);
                dialog.FileName = AddFolderPath(dialog.FileName);
                GeneralFunction.Save(dataManager.recipeDM.TeachingRD, dialog.FileName);
                return true;
            }
            return false;
        }

        public bool CreateRecipe(string path)
        {
            TeachingRecipePath = path;
            string strTeachingRecipeName = Path.GetFileName(path);
            TeachRecipeName = strTeachingRecipeName.Remove(strTeachingRecipeName.Length - 4);
            dataManager.recipeDM.TeachingRD = null;
            dataManager.recipeDM.TeachingRD = new RecipeData();

            dataManager.recipeDM.TeachingRD.Clone(dataManager.recipeDM.SaveRecipeRD);
            //path = AddFolderPath(path);
            GeneralFunction.Save(dataManager.recipeDM.TeachingRD, path);


            return true;
        }

        public bool RecipeOpen(string path = null)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = "aco";
            dialog.Filter = "*.aco|*.aco";
            dialog.InitialDirectory = BaseDefine.Dir_Recipe;
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    LoadRecipeName = Path.GetFileName(dialog.FileName);
                    LoadRecipeName = LoadRecipeName.Remove(LoadRecipeName.Length - 4);

                    dataManager.recipeDM.SaveRecipeRD.ClearPoint();                
                    dataManager.recipeDM.SaveRecipeRD = (RecipeData)GeneralFunction.Read(dataManager.recipeDM.SaveRecipeRD, dialog.FileName);
                    dataManager.recipeDM.SaveRecipeRD.CheckCircleSize();

                    //dataManager.recipeDM.MeasurementRD.Clone(dataManager.recipeDM.SaveRecipeRD);

                    //LoadRecipePath = dialog.FileName;
                    TeachingRecipePath = dialog.FileName;
                    return true;
                }
                catch (Exception)
                {
                }
            }

            return false;
        }

        

        public bool RecipeLoad(string path, bool isSave = true)
        {
            //OpenFileDialog dialog = new OpenFileDialog();
            //dialog.DefaultExt = "aco";
            //dialog.Filter = "*.aco|*.aco";
            //dialog.InitialDirectory = BaseDefine.Dir_Recipe;
            //if (dialog.ShowDialog() == true)
            //{
            //    try
            //    {
            if (File.Exists(path))
            {
                LoadRecipeName = Path.GetFileName(path);
                LoadRecipeName = LoadRecipeName.Remove(LoadRecipeName.Length - 4);

                if (isSave)
                {
                    dataManager.recipeDM.SaveRecipeRD.ClearPoint();
                    dataManager.recipeDM.SaveRecipeRD = (RecipeData)GeneralFunction.Read(dataManager.recipeDM.SaveRecipeRD, path);
                    dataManager.recipeDM.SaveRecipeRD.CheckCircleSize();
                    TeachingRecipePath = path;
                }
                else
                {
                    dataManager.recipeDM.MeasurementRD.ClearPoint();
                    dataManager.recipeDM.MeasurementRD = (RecipeData)GeneralFunction.Read(dataManager.recipeDM.MeasurementRD, path);
                    dataManager.recipeDM.MeasurementRD.CheckCircleSize();
                    LoadRecipePath = path;

                    if (RecipeLoaded != null)
                    {
                        RecipeLoaded.Invoke(this, new EventArgs());
                    }
                }
                

                //dataManager.recipeDM.MeasurementRD.Clone(dataManager.recipeDM.SaveRecipeRD);

                //LoadRecipePath = path;
          
                return true;

            }

            //    catch (Exception)
            //    {
            //    }
            //}

            return false;
        }

        public void RecipeSave()
        {
            if (TeachingRecipePath == null)
            {
                RecipeSaveAs();
            }
            else
            {
                GeneralFunction.Save(dataManager.recipeDM.TeachingRD, TeachingRecipePath);
                dataManager.recipeDM.TeachingRD.Clone(dataManager.recipeDM.SaveRecipeRD);
                CustomMessageBox.Show("Save Done!");
            }
        }
        
        public void RecipeSaveAs(string path = null)
        {
            if(path == null)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.DefaultExt = "aco";
                dialog.Filter = "*.aco|*.aco";
                dialog.InitialDirectory = BaseDefine.Dir_InitialPath;
                if (dialog.ShowDialog() == true)
                {
                    dialog.FileName = AddFolderPath(dialog.FileName);

                    TeachRecipeName = System.IO.Path.GetFileName(dialog.FileName);
                    TeachRecipeName = TeachRecipeName.Remove(TeachRecipeName.Length - 4);

                    GeneralFunction.Save(dataManager.recipeDM.TeachingRD, dialog.FileName);
                    // 여기 제거하고 테스트 진행 필요
                    // Recipe Save & Measure 분리
                    dataManager.recipeDM.TeachingRD.Clone(dataManager.recipeDM.SaveRecipeRD);
                    CustomMessageBox.Show("Save Done!");
                }
            }
            else
            {
                TeachRecipeName = System.IO.Path.GetFileName(path);
                TeachRecipeName = TeachRecipeName.Remove(TeachRecipeName.Length - 4);

                GeneralFunction.Save(dataManager.recipeDM.TeachingRD, path);
                dataManager.recipeDM.TeachingRD.Clone(dataManager.recipeDM.SaveRecipeRD);
                CustomMessageBox.Show("Save Done!");
            }

        }

        public string AddFolderPath(string FileName)
        {
            string[] str = FileName.Split('\\');
            string sRecipeName = str[str.Count() - 1].Replace(".aco", "");
            string[] sRecipeFolder = sRecipeName.Split('_');

            string sFileName = "";
            for (int i = 0; i < str.Count() - 1; i++)
            {
                sFileName += str[i];
                sFileName += "\\";
            }
            sFileName += sRecipeFolder[0];
            if (!Directory.Exists(sFileName))
            {
                Directory.CreateDirectory(sFileName);
            }
            sFileName += "\\";
            sFileName += str[str.Count() - 1];

            return sFileName;
        }

        public bool AddMaterial()
        {
            LibSR_Met.Nanoview.ERRORCODE_NANOVIEW res;
            OpenFileDialog fileDlg = new OpenFileDialog();

            fileDlg.Multiselect = true;
            fileDlg.Title = "Choose material file";
            //fileDlg.InitialDirectory = "..\\MaterialRef"; // 변경필요
            fileDlg.Filter = "material reference files (*.dis;*.ref)|*.dis;*.ref|All files (*.*)|*.*";
            fileDlg.FilterIndex = 0;

            if (fileDlg.ShowDialog() == true)
            {
                foreach (string filename in fileDlg.FileNames)
                {
                    res = App.m_nanoView.LoadMaterial(filename, true);
                    if (res == LibSR_Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                    {
                        dataManager.recipeDM.SaveModelData.MaterialList.Add(filename);
                    }
                }
                return true;
            }
            return false;
        }

        public void DeleteMaterial(int nIndex)
        {
            App.m_nanoView.m_MaterialListSave.RemoveAt(nIndex);
            dataManager.recipeDM.SaveModelData.MaterialList.RemoveAt(nIndex);
        }

        public bool LoadModel(string path, bool isSave = false)
        {
            if (isSave)
            {
                App.m_nanoView.LoadModel(path, true);

                dataManager.recipeDM.SaveModelData.MaterialList.Clear();

                foreach (Material m in App.m_nanoView.m_MaterialListSave)
                {
                    dataManager.recipeDM.SaveModelData.MaterialList.Add(m.m_Path);
                }
                return true;
            }
            else
            {
                LibSR_Met.Nanoview.ERRORCODE_NANOVIEW res;
                res = App.m_nanoView.LoadModel(path);
                if(res == LibSR_Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    dataManager.recipeDM.MeasureModelData.MaterialList.Clear();

                    foreach (Material m in App.m_nanoView.m_MaterialList)
                    {
                        dataManager.recipeDM.MeasureModelData.MaterialList.Add(m.m_Path);
                    }
                    
                    return true;
                }
               
                return false;
            }
         
        }

        public bool OpenModel()
        {
            string path = "..\\Recipe";
            OpenFileDialog fileDlg = new OpenFileDialog();

            fileDlg.InitialDirectory = Path.GetFullPath(path);
            fileDlg.Filter = "Model files (*.rcp;*.erm)|*.rcp;*.erm|All files (*.*)|*.*";
            fileDlg.FilterIndex = 1;
            fileDlg.RestoreDirectory = false;


            LibSR_Met.Nanoview.ERRORCODE_NANOVIEW res;
            if (fileDlg.ShowDialog() == true)
            {
                res = App.m_nanoView.LoadModel(fileDlg.FileName, true);
                if (res == LibSR_Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    dataManager.recipeDM.SaveModelData.MaterialList.Clear();
                    foreach (Material m in App.m_nanoView.m_MaterialListSave)
                    {
                        dataManager.recipeDM.SaveModelData.MaterialList.Add(m.m_Path);
                    }
                    dataManager.recipeDM.TeachingRD.ModelRecipePath = fileDlg.FileName;
                    return true;
                }
            }
            return false;
        }

        public bool SaveModel()
        {
            string path = BaseDefine.Dir_InitialLayerPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            SaveFileDialog fileDlg = new SaveFileDialog();

            fileDlg.Filter = "Model files (*.rcp)|*.rcp|All files(*.*)|*.*";
            fileDlg.RestoreDirectory = true;
            fileDlg.AddExtension = true;
            fileDlg.InitialDirectory = path;

            if (fileDlg.ShowDialog() == true)
            {
                if (App.m_nanoView.SaveModel(fileDlg.FileName))
                {
                    dataManager.recipeDM.TeachingRD.ModelRecipePath = fileDlg.FileName;
                }
                else
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
