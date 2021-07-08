using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class FrontVRSAlignRecipe : RecipeItemBase
    {
        #region [Parameter]
        private List<RecipeType_ImageData> alignFeatureVRSList;

        private long firstSearchPointX;
        private long firstSearchPointY;

        private long secondSearchPointX;
        private long secondSearchPointY;


        #endregion

        #region [Getter Setter]
        //[XmlArray("FeatureMaster")]
        //[XmlArrayItem("Feature")]
        //[XmlIgnore]
        public List<RecipeType_ImageData> AlignFeatureVRSList { get => alignFeatureVRSList; set => alignFeatureVRSList = value; }

        public long FirstSearchPointX { get => this.firstSearchPointX; set => this.firstSearchPointY = value; }
        public long FirstSearchPointY { get => this.firstSearchPointY; set => this.firstSearchPointY = value; }

        public long SecondSearchPointX { get => this.secondSearchPointX; set => this.secondSearchPointX = value; }
        public long SecondSearchPointY { get => this.secondSearchPointY; set => this.secondSearchPointY = value; }



        #endregion

        public FrontVRSAlignRecipe()
        {
            this.FirstSearchPointX = 0;
            this.FirstSearchPointY = 0;
            this.SecondSearchPointX = 0;
            this.SecondSearchPointY = 0;

            this.alignFeatureVRSList = new List<RecipeType_ImageData>();
        }

        public void AddAlignFeature(int positionX, int positionY, int featureWidth, int featureHeight, int byteCnt, byte[] rawData)
        {
            alignFeatureVRSList.Add(new RecipeType_ImageData(positionX, positionY, featureWidth, featureHeight, byteCnt, rawData));
        }

        public void RemoveMasterFeature(int index)
        {
            alignFeatureVRSList.RemoveAt(index);
        }

        public override bool Save(string recipePath)
        {
            bool rst = true;
            foreach (RecipeType_ImageData featureData in alignFeatureVRSList)
            {
                // FileName Setting
                if (featureData.FileName == "")
                {
                    string filename = "aligner_alignfeature_";
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
            foreach (RecipeType_ImageData featureData in alignFeatureVRSList)
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

        public override void Clear()
        {
            this.alignFeatureVRSList.Clear();

            this.FirstSearchPointX = 0;
            this.FirstSearchPointY = 0;
            this.SecondSearchPointX = 0;
            this.SecondSearchPointY = 0;
        }
    }
}
