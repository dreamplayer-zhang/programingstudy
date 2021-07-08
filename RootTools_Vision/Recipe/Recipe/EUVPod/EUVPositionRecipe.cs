using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    [Serializable]
    public class FeatureLists // Align Info
    {
        public FeatureLists()
        {
            listAlignFeature = new List<RecipeType_ImageData>();
            listPositionFeature = new List<RecipeType_ImageData>();
            alignAngle = 0;
        }
        List<RecipeType_ImageData> listAlignFeature;
        List<RecipeType_ImageData> listPositionFeature;
        double alignAngle;
        public double AlignAngle
        {
            get => alignAngle;
            set => alignAngle = value;
        }
        public List<RecipeType_ImageData> ListAlignFeature { get => listAlignFeature; set => listAlignFeature = value; }
        public List<RecipeType_ImageData> ListPositionFeature { get => listPositionFeature; set => listPositionFeature = value; }
        public void AddAlignFeature(int positionX, int positionY, int featureWidth, int featureHeight, int byteCnt, byte[] rawData)
        {
            ListAlignFeature.Add(new RecipeType_ImageData(positionX, positionY, featureWidth, featureHeight, byteCnt, rawData));
        }
        public void AddPositionFeature(int positionX, int positionY, int featureWidth, int featureHeight, int byteCnt, byte[] rawData)
        {
            ListPositionFeature.Add(new RecipeType_ImageData(positionX, positionY, featureWidth, featureHeight, byteCnt, rawData));
        }
        public void RemoveAlignFeature(int index)
        {
            ListAlignFeature.RemoveAt(index);
        }
        public void RemovePositionFeature(int index)
        {
            ListPositionFeature.RemoveAt(index);
        }
        public void ClearAlignFeature()
        {
            ListAlignFeature.Clear();
        }
        public void ClearPositionFeature()
        {
            ListPositionFeature.Clear();
        }
        public void Clear()
        {
            ClearAlignFeature();
            ClearPositionFeature();
        }
        public bool Read(string recipePath)
        {
            bool rst = true;
            foreach (RecipeType_ImageData featureData in ListAlignFeature)
            {
                // Load
                if (!featureData.Read(recipePath))
                {
                    rst = false;
                    break;
                }
            }

            foreach (RecipeType_ImageData featureData in ListPositionFeature)
            {
                // Load
                if (!featureData.Read(recipePath))
                {
                    rst = false;
                    break;
                }
            }

            return rst;
        }
        public bool Save(string recipePath)
        {
            bool rst = true;

            foreach (RecipeType_ImageData featureData in ListAlignFeature)
            {
                // FileName Setting
                if (featureData.FileName == "")
                {
                    string filename = "alignment_feature_";
                    int num = 0;
                    while (File.Exists(recipePath + "\\" + filename + num.ToString() + ".bmp") == true)
                    {
                        num++;
                    }

                    featureData.FileName = filename + num.ToString() + ".bmp";
                }

                // Save
                if (!featureData.Save(recipePath))
                {
                    rst = false;
                    break;
                }
            }

            foreach (RecipeType_ImageData featureData in ListPositionFeature)
            {
                // FileName Setting
                if (featureData.FileName == "")
                {
                    string filename = "position_feature_";
                    int num = 0;
                    while (File.Exists(recipePath + "\\" + filename + num.ToString() + ".bmp") == true)
                    {
                        num++;
                    }

                    featureData.FileName = filename + num.ToString() + ".bmp";
                }

                // Save
                if (!featureData.Save(recipePath))
                {
                    rst = false;
                    break;
                }
            }

            return rst;
        }
    }
    public class EUVPositionRecipe : RecipeItemBase
    {
        #region Parameter
        FeatureLists partsFeatureList;

        #endregion
        #region [Getter Setter]
        public FeatureLists PartsFeatureList { get => partsFeatureList; set => partsFeatureList = value; }
        #endregion

        public EUVPositionRecipe()
        {
            partsFeatureList = new FeatureLists();
        }

        public override void Clear()
        {
            partsFeatureList.Clear();
        }

        public override bool Read(string recipePath)
        {
            if (!PartsFeatureList.Read(recipePath))
                return false;

            return true;
        }

        public override bool Save(string recipePath)
        {
            if (!PartsFeatureList.Save(recipePath))
                return false;

            return true;
        }
    }
}
