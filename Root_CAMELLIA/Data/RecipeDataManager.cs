using Microsoft.Win32;
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
        public string TeachingRecipePath { get; set; }
        public string TeachRecipeName { get; set; }
        public RecipeDataManager(DataManager DM)
        {
            dataManager = DM;

            PresetData = new PresetData();
            TeachingRD = new RecipeData();
            MeasurementRD = new RecipeData();
        }
        public void RecipeNew()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = "aco";
            dialog.Filter = "*.aco|*.aco";
            dialog.InitialDirectory = BaseDefine.Dir_Recipe;
            if(dialog.ShowDialog() == true)
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

                    dataManager.recipeDM.TeachingRD.ClearPoint();
                    //Read(dialog.FileName);                        
                    dataManager.recipeDM.TeachingRD = (RecipeData)GeneralFunction.Read(dataManager.recipeDM.TeachingRD, dialog.FileName);
                    dataManager.recipeDM.TeachingRD.CheckCircleSize();
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
            if(TeachingRecipePath == null)
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
    }
}
