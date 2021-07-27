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
    [Serializable]
    public class RecipeBase : IRecipe
    {
        #region [Member Variables]
        private string name = "";
        private string recipePath = "";
        private string recipeFolderPath = "";
        private string waferID = "";

        RecipeType_WaferMap waferMap = new RecipeType_WaferMap();

        private List<RecipeItemBase> recipeItemList;
        private List<ParameterBase> parameterItemList;

        private int cameraInfoIndex = 0;

        private bool useExclusiveRegion = false;
        private string exclusiveRegionFilePath = "";
        #endregion

        #region [Getter Setter]
        public string Name { get => name; set => name = value; }
        public string WaferID { get => this.waferID; set => this.waferID = value; }
        public RecipeType_WaferMap WaferMap { get => waferMap; set => waferMap = value; }

        public int CameraInfoIndex
        {
            get => this.cameraInfoIndex;
            set => this.cameraInfoIndex = value;
        }

        public bool UseExclusiveRegion
        {
            get => this.useExclusiveRegion;
            set => this.useExclusiveRegion = value;
        }

        public string ExclusiveRegionFilePath
        {
            get => this.exclusiveRegionFilePath;
            set => this.exclusiveRegionFilePath = value;
        }


        [XmlIgnore]
        public List<RecipeItemBase> RecipeItemList { get => recipeItemList; set => recipeItemList = value; }

        [XmlIgnore]
        public List<ParameterBase> ParameterItemList { get => parameterItemList; set => parameterItemList = value; }

        [XmlIgnore]
        public string RecipePath { get => recipePath; set => recipePath = value; }

        [XmlIgnore]
        public string RecipeFolderPath { get => recipeFolderPath; set => recipeFolderPath = value; }
        #endregion
        //private List<WaferInfoBase> waferInfoItemList;


        /// <summary>
        /// RecipeItem의 각 class는 단일 객체만 허용
        /// ParameterItem의 각 class는 다중 객체 허용 
        /// </summary>
        public RecipeBase()
        {
            //RecipeItemList = Tools.GetEnumerableOfType<RecipeBase>().ToList<RecipeBase>();
            recipeItemList = new List<RecipeItemBase>();
            parameterItemList = new List<ParameterBase>();

            Initilize();
        }

        /// <summary>
        /// 레시피에 파라매터와 레시피 항목을 추가합니다.
        /// </summary>
        public virtual void Initilize() { }


        /// <summary>
        /// 레시피 항목을 추가합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>class가 RecipeBase를 상속하지 않을 경우 false 반환</returns>
        public bool RegisterRecipeItem<T>()
        {
            if(typeof(T).BaseType != typeof(RecipeItemBase))
            {
                MessageBox.Show("등록하려는 레시피 항목의 RecipeBase 클래스를 상속받지 않습니다.");
                return false;
            }
            this.recipeItemList.Add((RecipeItemBase)Tools.CreateInstance(typeof(T)));

            return true;
        }

        /// <summary>
        /// ※※※※※ 이 메서드는 같은 타입의 parameter를 여러개 사용 않지 않는 경우에만 사용하세요. ※※※※※※
        /// 파라매터 항목을 추가합니다.
        /// 파라매터는 하나의 class가 여러개의 객체를 생성할 수 있습니다.
        /// 파라매터를 기준으로 Inspection작업들을 생성합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>class가 RecipeBase를 상속하지 않을 경우 false 반환</returns>
        public bool RegisterParameterItem<T>()
        {
            if (typeof(T).BaseType != typeof(ParameterBase))
            {
                MessageBox.Show("등록하려는 레시피 항목의 ParameterBase 클래스를 상속받지 않습니다.");
                return false;
            }
            this.parameterItemList.Add((ParameterBase)Tools.CreateInstance(typeof(T)));

            return true;
        }


        /// <summary>
        /// 중복되는 Parameter 타입일 경우 GetItem을 통해서 불러올 경우 첫번째 항목만 불러와집니다.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool RegsiterParameterItem(ParameterBase param)
        {
            this.parameterItemList.Add(param.Clone());
            return true;
        }

        /// <summary>
        /// 중복되는 Parameter 타입일 경우 GetItem을 통해서 불러올 경우 첫번째 항목만 불러와집니다.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool RegsiterParameterItems(List<ParameterBase>paramList)
        {
            foreach(ParameterBase param in paramList)
            {
                this.parameterItemList.Add(param.Clone());
            }
            return true;
        }

        public void Clear()
        {
            name = "";
            recipePath = "";
            recipeFolderPath = "";

            waferMap = new RecipeType_WaferMap();

            recipeItemList = new List<RecipeItemBase>();
            parameterItemList = new List<ParameterBase>();

            Initilize();
        }

        public bool Read(string recipePath)
        {
            bool rst = true;
           

            string recipeName;
            string recipeFolderPath;

            if (File.Exists(recipePath))
            {
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                using (StreamWriter writer = new StreamWriter(recipePath, true))
                {
                    writer.WriteLine(time + " - LoadRecipe()");
                }

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
                    XmlSerializer xml = new XmlSerializer(typeof(RecipeBase));
                    RecipeBase temp = this;
                    temp = (RecipeBase)xml.Deserialize(reader);

                    this.Name = temp.Name;
                    this.WaferMap = temp.WaferMap;
                    this.CameraInfoIndex = temp.CameraInfoIndex;
                    this.UseExclusiveRegion = temp.UseExclusiveRegion;
                    this.ExclusiveRegionFilePath = temp.ExclusiveRegionFilePath;
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
                    this.RecipeItemList = (List<RecipeItemBase>)xml.Deserialize(reader);
                }

                // Inspection Info(?)



                // Xml 파일을 읽은 뒤 이미지나 ROI 등을 불러오기 위해서 각 class에 대한 Read 함수를 호출한다.
                foreach(ParameterBase param in this.ParameterItemList)
                {
                    param.Read(recipeFolderPath);
                }

                foreach(RecipeItemBase recipe in this.RecipeItemList)
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

        public bool Save(string recipePath = "")
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            bool rst = true;

            if (recipePath != "")
            {
                using (StreamWriter writer = new StreamWriter(recipePath, true))
                {
                    writer.WriteLine(time + " - SaveRecipe()");
                }

                this.RecipePath = (string)recipePath.Clone();

                recipePath = recipePath.Replace(".rcp", "");
                string recipeName = recipePath.Substring(recipePath.LastIndexOf("\\") + 1);
                string recipeFolderPath = recipePath.Substring(0, recipePath.LastIndexOf("\\") + 1);

                this.RecipeFolderPath = (string)recipeFolderPath.Clone();
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(this.RecipePath, true))
                {
                    writer.WriteLine(time + " - SaveRecipe()");
                }
            }

            // Xml 파일을 읽은 뒤 이미지나 ROI 등을 불러오기 위해서 각 class에 대한 Save 함수를 호출한다.
            foreach (ParameterBase param in this.ParameterItemList)
            {
                param.Save(recipeFolderPath);
            }

            foreach (RecipeItemBase recipe in this.RecipeItemList)
            {
                recipe.Save(recipeFolderPath);
            }

            try
            {

                RecipeBase recipeBase = CloneBase();
                using (TextWriter tw = new StreamWriter(recipeFolderPath + "Base.xml", false))
                {
                    XmlSerializer xml = new XmlSerializer(recipeBase.GetType());
                    xml.Serialize(tw, recipeBase);
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

        public RecipeBase CloneBase()
        {
            RecipeBase recipeBase = new RecipeBase();
            recipeBase.Name = this.Name;
            recipeBase.RecipeFolderPath = this.RecipeFolderPath;
            recipeBase.RecipePath = this.RecipePath;
            recipeBase.WaferMap = this.WaferMap;
            recipeBase.CameraInfoIndex = this.CameraInfoIndex;
            recipeBase.UseExclusiveRegion = this.UseExclusiveRegion;
            recipeBase.ExclusiveRegionFilePath = this.ExclusiveRegionFilePath;

            return recipeBase;
        }
        public void SaveMasterImage(int _posX, int _posY, int _width, int _height, int _byteCnt, byte[] _rawData)
        {
            OriginRecipe recipe = this.GetItem<OriginRecipe>();

            recipe.MasterImage = new RecipeType_ImageData(_posX, _posY, _width, _height, _byteCnt, _rawData);
            recipe.MasterImage.FileName = "MasterImage.bmp";
            recipe.MasterImage.Save(this.RecipeFolderPath);
        }


        public void LoadMasterImage()
        {
            if (this.RecipeFolderPath == "") return;

            OriginRecipe recipe = this.GetItem<OriginRecipe>();

            recipe.MasterImage = new RecipeType_ImageData();
            recipe.MasterImage.FileName = "MasterImage.bmp";
            recipe.MasterImage.Read(this.RecipeFolderPath);
        }

        public void LoadMasterImage(string fileName)
        {
            if (this.RecipeFolderPath == "") return;

            OriginRecipe recipe = this.GetItem<OriginRecipe>();

            recipe.MasterImage = new RecipeType_ImageData();
            recipe.MasterImage.FileName = fileName;
            recipe.MasterImage.Read(this.RecipeFolderPath);
        }

        public T GetItem<T>()
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

            //MessageBox.Show("Recipe 항목이나 Parameter 항목이 존재하지 않습니다. \nRegisterRecipe(혹은 RegisterParameter) 메서드를 통해 등록하십시오.");

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
