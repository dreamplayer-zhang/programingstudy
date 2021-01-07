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
        private string recipePath = "";
        private string recipeFolderPath = "";

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

        [XmlIgnore]
        public string RecipePath { get => recipePath; set => recipePath = value; }

        [XmlIgnore]
        public string RecipeFolderPath { get => recipeFolderPath; set => recipeFolderPath = value; }
        #endregion
        //private List<WaferInfoBase> waferInfoItemList;


        public Recipe()
        {
            RecipeItemList = Tools.GetEnumerableOfType<RecipeBase>().ToList<RecipeBase>();
            ParameterItemList = new List<ParameterBase>();
        }

        public void Clear()
        {
            name = "";
            recipePath = "";
            recipeFolderPath = "";

            waferMap = new RecipeType_WaferMap();

            RecipeItemList.Clear();
            foreach(RecipeBase recipe in Tools.GetEnumerableOfType<RecipeBase>().ToList<RecipeBase>())
            {
                RecipeItemList.Add(recipe);
            }
            ParameterItemList.Clear();

            WorkEventManager.OnUIRedraw(this, new UIRedrawEventArgs());
        }

        public bool Read(string recipePath, bool bUpdateUI)
        {
            bool bRst = Read(recipePath);

            if(bUpdateUI == true)
                WorkEventManager.OnUIRedraw(this, new UIRedrawEventArgs());

            return bRst;
        }

        public bool Read(string recipePath)
        {
            bool rst = true;
           

            string recipeName;
            string recipeFolderPath;

            if (File.Exists(recipePath))
            {
                this.RecipePath = (string)recipePath.Clone();

                recipePath = recipePath.Replace(".rcp", "");
                this.name = recipePath.Substring(recipePath.LastIndexOf("\\") + 1);

                recipeName = recipePath.Substring(recipePath.LastIndexOf("\\") + 1);
                recipeFolderPath = recipePath.Substring(0, recipePath.LastIndexOf("\\") + 1);
            }
            else
                return false;

          
            this.RecipeFolderPath = (string)recipeFolderPath.Clone();

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
                MessageBox.Show("Recipe Open Error\nDetail : " + ex.Message);
                rst = false;
            }
            
            return rst;
        }

        public bool Save(string recipePath)
        {
            bool rst = true;
            this.RecipePath = (string)recipePath.Clone();

            recipePath = recipePath.Replace(".rcp", "");
            string recipeName = recipePath.Substring(recipePath.LastIndexOf("\\") + 1);
            string recipeFolderPath = recipePath.Substring(0 ,recipePath.LastIndexOf("\\") + 1);

            this.RecipeFolderPath = (string)recipeFolderPath.Clone();

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
                MessageBox.Show("Recipe Save Error!!\nDetail : " + ex.Message);
            }

            return rst;
        }

        public void SaveMasterImage(int _posX, int _posY, int _width, int _height, int _byteCnt, byte[] _rawData)
        {
            OriginRecipe recipe = this.GetRecipe<OriginRecipe>();

            recipe.MasterImage = new RecipeType_ImageData(_posX, _posY, _width, _height, _byteCnt, _rawData);
            recipe.MasterImage.FileName = "MasterImage.bmp";
            recipe.MasterImage.Save(this.RecipeFolderPath);
        }

        public void LoadMasterImage()
        {
            if (this.RecipeFolderPath == "") return;

            OriginRecipe recipe = this.GetRecipe<OriginRecipe>();

            recipe.MasterImage = new RecipeType_ImageData();
            recipe.MasterImage.FileName = "MasterImage.bmp";
            recipe.MasterImage.Read(this.RecipeFolderPath);
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
