using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Root_CAMELLIA.Data
{
    public class RecipeDataManager
    {
        private DataManager dataManager { get; set; }
        public PresetData PresetData { get; set; }
        public RecipeData TeachingRD { get; set; }
        public string TeachingRecipePath { get; set; }
        public RecipeDataManager(DataManager DM)
        {
            dataManager = DM;

            PresetData = new PresetData();
            TeachingRD = new RecipeData();
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
                string strTeachingRecipeName = System.IO.Path.GetFileName(dialog.FileName);
                strTeachingRecipeName = strTeachingRecipeName.Remove(strTeachingRecipeName.Length - 4);
                dataManager.recipeDM.TeachingRD = null;
                dataManager.recipeDM.TeachingRD = new RecipeData();
                dataManager.Main.m_RecipeManagerViewModel.UpdateView();
                GeneralFunction.Save(dataManager.recipeDM.TeachingRD, dialog.FileName);
            }
        }
        public void RecipeOpen(string path = null)
        {
            
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
            
            }
        }
        public void RecipeSaveAs(string path = null)
        {

        }
    }
}
