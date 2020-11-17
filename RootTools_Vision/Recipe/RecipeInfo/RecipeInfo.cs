using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RootTools;

namespace RootTools_Vision
{
    public class RecipeInfo : IRecipeInfo
    {

        string recipename = "Default_Recipe";
        public string m_RecipeName { get => recipename; set => recipename = value; }

        string recipePath = "";
        public string m_RecipePath { get => recipePath; set => recipePath = value; }

        public RecipeInfo_MapData m_RecipeInfoMapData; // Wafer map

        [XmlArray("RecipeInfo")]
        [XmlArrayItem("MapData", typeof(RecipeInfo_MapData))]
        //[XmlArrayItem("Position", typeof(ParamData_Position))]
        public List<IRecipeInfo> recipeinfos;

        public RecipeInfo()
        {
        }

        public RecipeInfo(bool bFirst)
        {
            recipeinfos = new List<IRecipeInfo>();
            AddRecipeInfo(new RecipeInfo_MapData());
        }

        public void SetRecipeInfo(string srcpName)
        {
            this.m_RecipeName = srcpName;
        }

        private void AddRecipeInfo(IRecipeInfo recipeInfo)
        {
            recipeinfos.Add(recipeInfo);
        }

        public IRecipeInfo GetRecipeInfo(Type type)
        {
            foreach (IRecipeInfo recipeInfo in recipeinfos)
            {
                if (recipeInfo.GetType() == type)
                {
                    return recipeInfo;
                }
            }
            return null;
        }

    }
}
