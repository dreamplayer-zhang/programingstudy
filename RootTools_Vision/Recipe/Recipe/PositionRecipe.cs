using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RootTools_Vision
{
    public class PositionRecipe : RecipeBase
    {
        #region [Parameter]
        private List<RecipeType_FeatureData> listMasterFeature;
        private List<RecipeType_FeatureData> listShotFeature;
        private List<RecipeType_FeatureData> listChipFeature;

        private int indexMaxScoreMasterFeature;
        private int indexMaxScoreShotFeature;
        private int indexMaxScoreChipFeature;
        #endregion

        #region [Getter Setter]
        //[XmlArray("FeatureMaster")]
        //[XmlArrayItem("Feature")]
        //[XmlIgnore]
        public List<RecipeType_FeatureData> ListMasterFeature { get => listMasterFeature; set => listMasterFeature = value; }

        //[XmlArray("FeatureShot")]
        //[XmlArrayItem("Feature")]
        //[XmlIgnore]
        public List<RecipeType_FeatureData> ListShotFeature { get => listShotFeature; set => listShotFeature = value; }

        //[XmlArray("FeatureChip")]
        //[XmlArrayItem("Feature")]
        //[XmlIgnore]
        public List<RecipeType_FeatureData> ListDieFeature { get => listChipFeature; set => listChipFeature = value; }

        public int IndexMaxScoreMasterFeature { get => indexMaxScoreMasterFeature; set => indexMaxScoreMasterFeature = value; }
        public int IndexMaxScoreShotFeature { get => indexMaxScoreShotFeature; set => indexMaxScoreShotFeature = value; }
        public int IndexMaxScoreChipFeature { get => indexMaxScoreChipFeature; set => indexMaxScoreChipFeature = value; }
        #endregion

        public PositionRecipe()
        {
            listMasterFeature = new List<RecipeType_FeatureData>();
            listShotFeature = new List<RecipeType_FeatureData>();
            listChipFeature = new List<RecipeType_FeatureData>();

            indexMaxScoreMasterFeature = -1;
            indexMaxScoreShotFeature = -1;
            indexMaxScoreChipFeature = -1;
        }

        public void AddMasterFeature(int positionX, int positionY, int featureWidth, int featureHeight, int byteCnt, byte[] rawData)
        {
            listMasterFeature.Add(new RecipeType_FeatureData(positionX, positionY, featureWidth, featureHeight, byteCnt, rawData));
        }

        public void AddShotFeature(int positionX, int positionY, int featureWidth, int featureHeight, int byteCnt, byte[] rawData)
        {
            listShotFeature.Add(new RecipeType_FeatureData(positionX, positionY, featureWidth, featureHeight, byteCnt, rawData));
        }

        public void AddChipFeature(int positionX, int positionY, int featureWidth, int featureHeight, int byteCnt, byte[] rawData)
        {
            listChipFeature.Add(new RecipeType_FeatureData(positionX, positionY, featureWidth, featureHeight, byteCnt, rawData));
        }


        public override bool Save(string recipePath)
        {
            bool rst = true;
            foreach (RecipeType_FeatureData featureData in listMasterFeature)
            {
                // FileName Setting
                if (featureData.FileName == "")
                {
                    string filename = "alignment_master_feature_";
                    int num = 0;
                    while(File.Exists(recipePath + "\\" + filename + num.ToString() + ".bmp") == true)
                    {
                        num++; 
                    }

                    featureData.FileName = filename + num.ToString() + ".bmp";
                }

                // Save
                if(!featureData.Save(recipePath))
                {
                    rst = false;
                    break;
                }
            }

            foreach (RecipeType_FeatureData featureData in listShotFeature)
            {
                // FileName Setting
                if (featureData.FileName == "")
                {
                    string filename = "alignment_shot_feature_";
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

            foreach (RecipeType_FeatureData featureData in listChipFeature)
            {
                // FileName Setting
                if (featureData.FileName == "")
                {
                    string filename = "alignment_chip_feature_";
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

        public override bool Read(string recipePath)
        {
            bool rst = true;
            foreach(RecipeType_FeatureData featureData in listMasterFeature)
            {
                // Load
                if(!featureData.Read(recipePath))
                {
                    rst = false;
                    break;
                }
            }

            foreach (RecipeType_FeatureData featureData in listShotFeature)
            {
                // Load
                if (!featureData.Read(recipePath))
                {
                    rst = false;
                    break;
                }
            }

            foreach (RecipeType_FeatureData featureData in listChipFeature)
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
    }
}
