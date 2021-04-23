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
        private DataManager dataManager { get; set; }
        public PresetData PresetData { get; set; }
        public RecipeData TeachingRD { get; set; }
        public RecipeData MeasurementRD { get; set; }
        public ModelData ModelData { get; set; }
        public string TeachingRecipePath { get; set; }
        public string TeachRecipeName { get; set; } = "";
        public RecipeDataManager(DataManager DM)
        {
            dataManager = DM;

            PresetData = new PresetData();
            TeachingRD = new RecipeData();
            MeasurementRD = new RecipeData();
            ModelData = new ModelData();
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

                dataManager.recipeDM.TeachingRD.Clone(dataManager.recipeDM.MeasurementRD);
                dialog.FileName = AddFolderPath(dialog.FileName);
                GeneralFunction.Save(dataManager.recipeDM.TeachingRD, dialog.FileName);
                return true;
            }
            return false;
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
                    TeachRecipeName = Path.GetFileName(dialog.FileName);
                    TeachRecipeName = TeachRecipeName.Remove(TeachRecipeName.Length - 4);

                    dataManager.recipeDM.MeasurementRD.ClearPoint();                
                    dataManager.recipeDM.MeasurementRD = (RecipeData)GeneralFunction.Read(dataManager.recipeDM.MeasurementRD, dialog.FileName);
                    dataManager.recipeDM.MeasurementRD.CheckCircleSize();

                    dataManager.recipeDM.MeasurementRD.Clone(dataManager.recipeDM.TeachingRD);

                    TeachingRecipePath = dialog.FileName;

                    return true;
                }
                catch (Exception)
                {
                }
            }

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

                TeachRecipeName = System.IO.Path.GetFileName(dialog.FileName);
                TeachRecipeName = TeachRecipeName.Remove(TeachRecipeName.Length - 4);

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
                    res = App.m_nanoView.LoadMaterial(filename);
                    if (res == LibSR_Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                    {
                        dataManager.recipeDM.ModelData.MaterialList.Add(filename);
                    }
                }
                return true;
            }
            return false;
        }

        public void DeleteMaterial(int nIndex)
        {
            App.m_nanoView.m_MaterialList.RemoveAt(nIndex);
            dataManager.recipeDM.ModelData.MaterialList.RemoveAt(nIndex);
        }

        public bool LoadModel(string path)
        {
            App.m_nanoView.LoadModel(path);

            dataManager.recipeDM.ModelData.MaterialList.Clear();

            foreach (Material m in App.m_nanoView.m_MaterialList)
            {
                dataManager.recipeDM.ModelData.MaterialList.Add(m.m_Path);
            }
            return true;
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
                res = App.m_nanoView.LoadModel(fileDlg.FileName);
                if (res == LibSR_Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    dataManager.recipeDM.ModelData.MaterialList.Clear();
                    foreach (Material m in App.m_nanoView.m_MaterialList)
                    {
                        dataManager.recipeDM.ModelData.MaterialList.Add(m.m_Path);
                    }
                    dataManager.recipeDM.TeachingRD.ModelRecipePath = fileDlg.FileName;
                    return true;
                }
            }
            return false;
        }

        public bool SaveModel()
        {
            SaveFileDialog fileDlg = new SaveFileDialog();

            fileDlg.Filter = "Model files (*.rcp)|*.rcp|All files(*.*)|*.*";
            fileDlg.RestoreDirectory = true;
            fileDlg.AddExtension = true;

            if (fileDlg.ShowDialog() == true)
            {
                App.m_nanoView.SaveModel(fileDlg.FileName);
                dataManager.recipeDM.TeachingRD.ModelRecipePath = fileDlg.FileName;
                return true;
            }
            return false;
        }
    }
}
