using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RootTools_Vision
{
    /// <summary>
    /// Recipe 클래스 기반 부가 기능 상세.
    /// </summary>
    /// 

    public class RecipeManager
    {
        // EXPORT
        // IMPORT
        // SAVE
        // LOAD
        // Convert Recipe
      
        public Recipe m_Recipe; // Serialize를 위해서 Public 선언이 필요
        public RecipeManager()
        {
            m_Recipe = new Recipe();
        }

        public ref Recipe GetRecipe()
        {
            return ref m_Recipe;
        }


        #region [Recipe Info Load / Save]
        public void LoadRecipe()
        {
            try
            {
                string paramPath = @"C:\Wind2\wind2.xml";
                XmlSerializer serializer = new XmlSerializer(typeof(RecipeInfo));
                RecipeInfo result = new RecipeInfo();

                using (Stream reader = new FileStream(paramPath, FileMode.Open))
                {
                    // Call the Deserialize method to restore the object's state.
                    result = (RecipeInfo)serializer.Deserialize(reader);
                }
                m_Recipe.m_RecipeInfo = result;
            }
            catch (Exception ex)
            {
                string sError = ex.Message;
            }
        }
        public void SaveRecipe()
        {
            try
            {
                string paramPath = @"C:\Wind2\wind2.xml";
                using (StreamWriter wr = new StreamWriter(paramPath))
                {

                    XmlSerializer xs = new XmlSerializer(typeof(RecipeInfo));
                    xs.Serialize(wr, m_Recipe.m_RecipeInfo);
                }
            }
            catch (Exception ex)
            {
                string sError = ex.Message;
            }
        }
        #endregion

        #region [Recipe Graphics Load / Save]
        public void LoadGraphicsRecipe()
        {
            try
            {
                string paramPath = @"C:\Wind2\wind2_Graphics.xml";
                XmlSerializer serializer = new XmlSerializer(typeof(RecipeData));
                RecipeData result = m_Recipe.GetRecipeData();

                using (Stream reader = new FileStream(paramPath, FileMode.Open))
                {
                    // Call the Deserialize method to restore the object's state.
                    result = (RecipeData)serializer.Deserialize(reader);
                }
                m_Recipe.m_ReicpeData = result;
            }
            catch (Exception ex)
            {
                string sError = ex.Message;
                // log
            }

        }
        public void SaveGraphicsFile()
        {
            // 그리기파일
            try
            {
                RecipeData recipeData = m_Recipe.GetRecipeData();
                string paramPath = @"C:\Wind2\wind2_Graphics.xml";
                using (StreamWriter wr = new StreamWriter(paramPath))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(RecipeData));
                    xs.Serialize(wr, recipeData);
                }
            }
            catch (Exception ex)
            {
                string sError = ex.Message;
                // log
            }
        }
        #endregion




        public void ExportRecipe() { }
        public void ImportRecipe() { }


    }
}
