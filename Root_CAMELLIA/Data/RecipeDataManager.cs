using Microsoft.Win32;
using NanoView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_CAMELLIA.Data
{
    public class RecipeDataManager
    {
        private DataManager dataManager { get; set; }
        public PresetData PresetData { get; set; }
        public RecipeData TeachingRD { get; set; }
        public RecipeData MeasurementRD { get; set; }
        public ModelData ModelData { get; set; }
        public string TeachingRecipePath { get; set; }
        public string TeachRecipeName { get; set; }
        public RecipeDataManager(DataManager DM)
        {
            dataManager = DM;

            PresetData = new PresetData();
            TeachingRD = new RecipeData();
            MeasurementRD = new RecipeData();
            ModelData = new ModelData();
        }
        public void RecipeNew()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = "aco";
            dialog.Filter = "*.aco|*.aco";
            dialog.InitialDirectory = BaseDefine.Dir_Recipe;
            if (dialog.ShowDialog() == true)
            {
                TeachingRecipePath = dialog.FileName;
                string strTeachingRecipeName = Path.GetFileName(dialog.FileName);
                strTeachingRecipeName = strTeachingRecipeName.Remove(strTeachingRecipeName.Length - 4);
                dataManager.recipeDM.TeachingRD = null;
                dataManager.recipeDM.TeachingRD = new RecipeData();

                dialog.FileName = AddFolderPath(dialog.FileName);
                GeneralFunction.Save(dataManager.recipeDM.TeachingRD, dialog.FileName);
            }
        }
        public void ReadRecipe(string path)
        {

        }
        public void RecipeOpen(string path = null)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = "aco";
            dialog.Filter = "*.aco|*.aco";
            dialog.InitialDirectory = BaseDefine.Dir_Recipe;
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    //m_DM.m_LM.WriteLog(LOG.CAMELLIA, "[Open Recipe] Recipe File Path - " + dialog.FileName);
                    TeachRecipeName = Path.GetFileName(dialog.FileName);
                    TeachRecipeName = TeachRecipeName.Remove(TeachRecipeName.Length - 4);
                    //m_DM.m_LM.WriteLog(LOG.CAMELLIA, "[Open Recipe] Recipe File Name - " + strTeachingRecipeName);

                    dataManager.recipeDM.MeasurementRD.ClearPoint();
                    //Read(dialog.FileName);                        
                    dataManager.recipeDM.MeasurementRD = (RecipeData)GeneralFunction.Read(dataManager.recipeDM.MeasurementRD, dialog.FileName);
                    dataManager.recipeDM.MeasurementRD.CheckCircleSize();
                    if (dataManager.recipeDM.MeasurementRD.ModelRecipePath != "")
                    {
                        LoadModel(dataManager.recipeDM.MeasurementRD.ModelRecipePath);
                    }
                    dataManager.recipeDM.MeasurementRD.Clone(dataManager.recipeDM.TeachingRD);

                    // dataManager.recipeDM.TeachingRD.CheckLayerData();
                    // dataManager.recipeDM.TeachingRD.CheckWavelengthData();
                    //m_DM.Main.m_DlgRecipeManager.CheckWavelengthBound();
                    TeachingRecipePath = dialog.FileName;
                    //m_DM.Main.TeachingRecipeOpenDone(dialog.FileName);
                    //UnlockFile();
                    //LockFile(dialog.FileName);
                    // m_DM.Main.m_DlgRecipeManager.Text = "Recipe Manager" + "  " + m_DM.m_RDM.strTeachingRecipe;

                    //  m_DM.m_LM.WriteLog(LOG.CAMELLIA, "[Open Recipe] Done");
                    // m_DM.m_LM.WriteLog(LOG.PARAMETER, "[Recipe Manager] Open - Recipe : " + dialog.FileName);
                    //  MessageBox.Show("Open Recipe Done!");
                   
                }
                catch (Exception ex)
                {
                    // m_DM.m_LM.WriteLog(LOG.CAMELLIA, "[Open Recipe] Exception Caught");
                    // MessageBox.Show("Open Exception Caught! - " + ex.Message);
                }
            }
            //else
            //{
            //    m_DM.m_LM.WriteLog(LOG.CAMELLIA, "[Open Recipe] Fail");
            //    MessageBox.Show("Open Recipe Fail!");
            //    return false;
            //}
        }
        public void RecipeSave()
        {
            if (TeachingRecipePath == null)
            {
                RecipeSaveAs();
            }
            else
            {
                //SaveFileDialog dialog = new SaveFileDialog();
                //dialog.DefaultExt = "aco";
                //dialog.Filter = "*.aco|*.aco";
                //dialog.InitialDirectory = BaseDefine.Dir_Recipe;
                //if (dialog.ShowDialog() == true)
                //{
                //string strTeachingRecipeName = System.IO.Path.GetFileName(dialog.FileName);
                //strTeachingRecipeName = strTeachingRecipeName.Remove(strTeachingRecipeName.Length - 4);

                GeneralFunction.Save(dataManager.recipeDM.TeachingRD, TeachingRecipePath);
                dataManager.recipeDM.TeachingRD.Clone(dataManager.recipeDM.MeasurementRD);
                MessageBox.Show("Save Done!");

            }
        }
        public void RecipeSaveAs(string path = null)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = "aco";
            dialog.Filter = "*.aco|*.aco";
            dialog.InitialDirectory = BaseDefine.Dir_Recipe;
            if (dialog.ShowDialog() == true)
            {
                dialog.FileName = AddFolderPath(dialog.FileName);

                //da.m_LM.WriteLog(LOG.CAMELLIA, "[Save As Recipe] Recipe File Path - " + dialog.FileName);
                TeachRecipeName = System.IO.Path.GetFileName(dialog.FileName);
                TeachRecipeName = TeachRecipeName.Remove(TeachRecipeName.Length - 4);
                //m_DM.m_LM.WriteLog(LOG.CAMELLIA, "[Save As Recipe] Recipe File Name - " + strTeachingRecipeName);

                GeneralFunction.Save(dataManager.recipeDM.TeachingRD, dialog.FileName);
                dataManager.recipeDM.TeachingRD.Clone(dataManager.recipeDM.MeasurementRD);
                MessageBox.Show("Save Done!");
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

        public int AddMaterial()
        {
            int index = 0, res = 0;
            string name = "", ext = "";

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
                    res = App.m_nanoView.m_Model.LoadMaterialFile(filename);
                    if (res == 0)
                    {
                        dataManager.recipeDM.ModelData.MaterialList.Add(filename);
                    }
                    else
                    {
                        if (res == 1) name = String.Format("The material({0}) is already in the list.", fileDlg.FileName);
                        if (res == 2) name = String.Format("Error : file extension of ({0}) is incorrect.", fileDlg.FileName);
                        if (res == 3) name = String.Format("Error : file ({0}) not found.", fileDlg.FileName);
                        MessageBox.Show(name);
                    }
                }
                return res;
            }
            return -1;
        }

        public void DeleteMaterial(int nIndex)
        {
            App.m_nanoView.m_Model.m_MaterialList.RemoveAt(nIndex);
            dataManager.recipeDM.ModelData.MaterialList.RemoveAt(nIndex);
        }

        public void LoadModel(string path)
        {
            App.m_nanoView.LoadModel(path);
            //int bl;

            //bl = App.m_nanoView.m_Model.FillFromFile(dataManager.recipeDM.MeasurementRD.ModelRecipePath);   

            //if (bl != 0)
            //{
            //    foreach (Material m in App.m_nanoView.m_Model.m_MaterialList)
            //    {
            //        App.m_nanoView.m_Model.m_MaterialList.Remove(m);
            //    }
            //    return;
            //}

            dataManager.recipeDM.ModelData.MaterialList.Clear();
            //// delete and add material list control
            ////foreach (ListViewItem item in materialList.Items)
            ////{
            ////    item.Remove();
            ////}
            //// DeleteComboStrings();
            foreach (Material m in App.m_nanoView.m_Model.m_MaterialList)
            {
                //if (m.m_Type == NanoView.Material.MaterialType.DISPERSION) index = 0;
                //else index = 1;
                //AddMaterialListItem(m.m_Name, index);
                // dataManager.recipeDM.MeasurementRD.MaterialList.Add(m.m_Path);
                dataManager.recipeDM.ModelData.MaterialList.Add(m.m_Path);
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



            if (fileDlg.ShowDialog() == true)
            {
                int bl;

                bl = App.m_nanoView.m_Model.FillFromFile(fileDlg.FileName);

                if (bl != 0)
                {
                    foreach (Material m in App.m_nanoView.m_Model.m_MaterialList)
                    {
                        App.m_nanoView.m_Model.m_MaterialList.Remove(m);
                    }
                    return false;
                }

                dataManager.recipeDM.ModelData.MaterialList.Clear();
                foreach (Material m in App.m_nanoView.m_Model.m_MaterialList)
                {
                    dataManager.recipeDM.ModelData.MaterialList.Add(m.m_Path);
                }
                dataManager.recipeDM.TeachingRD.ModelRecipePath = fileDlg.FileName;
                return true;
            }
            return false;
        }

        public void SaveModel()
        {
            SaveFileDialog fileDlg = new SaveFileDialog();

            fileDlg.Filter = "Model files (*.rcp)|*.rcp|All files(*.*)|*.*";
            fileDlg.RestoreDirectory = true;
            fileDlg.AddExtension = true;

            if (fileDlg.ShowDialog() == true)
            {
                App.m_nanoView.SaveModel(fileDlg.FileName);
            }
            dataManager.recipeDM.TeachingRD.ModelRecipePath = fileDlg.FileName;
            //if(dataManager.recipeDM.TeachingRD.ModelRecipePath == "")
            //{
            //    SaveAsModel();
            //}
            ////SaveFileDialog fileDlg = new SaveFileDialog();

            ////fileDlg.Filter = "Model files (*.rcp)|*.rcp|All files(*.*)|*.*";
            ////fileDlg.RestoreDirectory = true;
            ////fileDlg.AddExtension = true;

            ////if (fileDlg.ShowDialog() == true)
            ////{
            //if(MessageBox.Show("Are you sure?", "Save", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            //{
            //    File.WriteAllLines(dataManager.recipeDM.TeachingRD.ModelRecipePath, App.m_nanoView.m_Model.m_LayerList.ToString());
            //}
            ////}
        }

        //public bool SaveAsModel()
        //{
        //    SaveFileDialog fileDlg = new SaveFileDialog();

        //    fileDlg.Filter = "Model files (*.rcp)|*.rcp|All files(*.*)|*.*";
        //    fileDlg.RestoreDirectory = true;
        //    fileDlg.AddExtension = true;

        //    if (fileDlg.ShowDialog() == true)
        //    {
        //        File.WriteAllLines(fileDlg.FileName, App.m_nanoView.m_Model.m_LayerList.ToString());
        //        dataManager.recipeDM.TeachingRD.ModelRecipePath = fileDlg.FileName;
        //        return true;
        //    }
        //    return false;
        //}
    }
}
