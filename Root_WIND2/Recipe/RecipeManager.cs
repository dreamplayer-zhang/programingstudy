using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Root_WIND2
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

        public void OpenRecipe() 
        {
            try
            {
                string paramPath = @"C:\Wind2\wind2.xml";
                XmlSerializer serializer = new XmlSerializer(typeof(Recipe));
                Recipe result = new Recipe();

                using (Stream reader = new FileStream(paramPath, FileMode.Open))
                {
                    // Call the Deserialize method to restore the object's state.
                    result = (Recipe)serializer.Deserialize(reader);
                }
            }
            catch(Exception ex)
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
                    
                    XmlSerializer xs = new XmlSerializer(typeof(Recipe));
                    xs.Serialize(wr, m_Recipe);
                }
            }
            catch(Exception ex)
            {
                string sError = ex.Message;
            }
        }

        public void ExportRecipe() { }
        public void ImportRecipe() { }


    }
}
