using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace RootTools_Vision
{
    public class Recipe : IRecipe
    {
        #region [Member Variables]
        private string name = "";

        RecipeType_WaferMap waferMap = new RecipeType_WaferMap();

        private List<RecipeBase> recipeItemList;
        private List<ParameterBase> parameterItemList;
        #endregion

        #region [Getter Setter]
        public string Name { get => name; set => name = value; }
        public RecipeType_WaferMap WaferMap { get => waferMap; set => waferMap = value; }

        [XmlIgnore]
        public List<RecipeBase> RecipeItemList { get => recipeItemList; set => recipeItemList = value; }

        [XmlIgnore]
        public List<ParameterBase> ParameterItemList { get => parameterItemList; set => parameterItemList = value; }
        #endregion
        //private List<WaferInfoBase> waferInfoItemList;


        public Recipe()
        {
            RecipeItemList = Tools.GetEnumerableOfType<RecipeBase>().ToList<RecipeBase>();
            //ParameterItemList = Tools.GetEnumerableOfType<ParameterBase>().ToList<ParameterBase>();
            //waferInfoItemList = Tools.GetEnumerableOfType<WaferInfoBase>().ToList<WaferInfoBase>();

            ParameterItemList = new List<ParameterBase>();
        }

        public bool Read(string recipePath)
        {
            bool rst = true;

            string recipeName;
            string recipeFolderPath;

            if (File.Exists(recipePath))
            {
                recipePath = recipePath.Replace(".rcp", "");
                this.name = recipePath.Substring(recipePath.LastIndexOf("\\") + 1);

                recipeName = recipePath.Substring(recipePath.LastIndexOf("\\") + 1);
                recipeFolderPath = recipePath.Substring(0, recipePath.LastIndexOf("\\") + 1);
            }
            else
                return false;

            try
            {
                using (Stream reader = new FileStream(recipeFolderPath + "Base.xml", FileMode.Open))
                {
                    XmlSerializer xml = new XmlSerializer(this.GetType());
                    Recipe temp = this;
                    temp = (Recipe)xml.Deserialize(reader);

                    this.WaferMap = temp.WaferMap;
                }

                // Parameter
                using (Stream reader = new FileStream(recipeFolderPath + "Parameter.xml", FileMode.Open))
                {
                    XmlSerializer xml = new XmlSerializer(this.ParameterItemList.GetType());
                    this.ParameterItemList = (List<ParameterBase>)xml.Deserialize(reader);
                }
                // Recipe
                using (Stream reader = new FileStream(recipeFolderPath + "Recipe.xml", FileMode.Open))
                {
                    XmlSerializer xml = new XmlSerializer(this.RecipeItemList.GetType());
                    this.RecipeItemList = (List<RecipeBase>)xml.Deserialize(reader);
                }

                // Inspection Info(?)



                // Xml 파일을 읽은 뒤 이미지나 ROI 등을 불러오기 위해서 각 class에 대한 Read 함수를 호출한다.
                foreach(ParameterBase param in this.ParameterItemList)
                {
                    param.Read(recipeFolderPath);
                }

                foreach(RecipeBase recipe in this.RecipeItemList)
                {
                    recipe.Read(recipeFolderPath);
                }
            }
            catch(Exception ex)
            {
                rst = false;
            }
            
            return rst;
        }

        public bool Save(string recipePath)
        {
            bool rst = true;

            recipePath = recipePath.Replace(".rcp", "");
            string recipeName = recipePath.Substring(recipePath.LastIndexOf("\\") + 1);
            string recipeFolderPath = recipePath.Substring(0 ,recipePath.LastIndexOf("\\") + 1);

            // Xml 파일을 읽은 뒤 이미지나 ROI 등을 불러오기 위해서 각 class에 대한 Save 함수를 호출한다.
            foreach (ParameterBase param in this.ParameterItemList)
            {
                param.Save(recipeFolderPath);
            }

            foreach (RecipeBase recipe in this.RecipeItemList)
            {
                recipe.Save(recipeFolderPath);
            }

            try
            {
                using (TextWriter tw = new StreamWriter(recipeFolderPath + "Base.xml", false))
                {
                    XmlSerializer xml = new XmlSerializer(this.GetType());
                    xml.Serialize(tw, this);
                }

                using (TextWriter tw = new StreamWriter(recipeFolderPath + "Parameter.xml", false))
                {
                    XmlSerializer xml = new XmlSerializer(this.ParameterItemList.GetType());
                    xml.Serialize(tw, this.ParameterItemList);
                }

                // Recipe
                using (TextWriter tw = new StreamWriter(recipeFolderPath + "Recipe.xml", false))
                {
                    XmlSerializer xml = new XmlSerializer(this.RecipeItemList.GetType());
                    xml.Serialize(tw, this.RecipeItemList);
                }
            }
            catch(Exception ex)
            {
                rst = false;
            }

            return rst;
        }

        public T GetRecipe<T>()
        {
            foreach (IRecipe recipe in RecipeItemList)
            {
                if (recipe.GetType() == typeof(T))
                    return (T)recipe;
            }

            foreach (IRecipe recipe in this.ParameterItemList)
            {
                if (recipe.GetType() == typeof(T))
                    return (T)recipe;
            }

            return default(T);
        }

        public int CompareTo(IRecipe other)
        {
            if (this == other)
                return 0;
            else
                return 1;
        }
    }
}
