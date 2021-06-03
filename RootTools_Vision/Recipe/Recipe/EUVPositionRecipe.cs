using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class FeatureLists
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
            listAlignFeature.Add(new RecipeType_ImageData(positionX, positionY, featureWidth, featureHeight, byteCnt, rawData));
        }
        public void AddPositionFeature(int positionX, int positionY, int featureWidth, int featureHeight, int byteCnt, byte[] rawData)
        {
            listPositionFeature.Add(new RecipeType_ImageData(positionX, positionY, featureWidth, featureHeight, byteCnt, rawData));
        }
        public void RemoveAlignFeature(int index)
        {
            listAlignFeature.RemoveAt(index);
        }
        public void RemovePositionFeature(int index)
        {
            listPositionFeature.RemoveAt(index);
        }
        public void ClearAlignFeature()
        {
            ListAlignFeature.Clear();
        }
        public void ClearPositionFeature()
        {
            listPositionFeature.Clear();
        }
        public void Clear()
        {
            ClearAlignFeature();
            ClearPositionFeature();
        }
    }
    public class EUVPositionRecipe : RecipeItemBase
    {
        #region Parameter
        FeatureLists EIPCoverTopfeature, EIPCoverBtmfeature, EIPBaseTopfeature, EIPBaseBtmfeature;

        #endregion
        #region [Getter Setter]
        public FeatureLists EIPCoverTopFeature { get => EIPCoverTopfeature; set => EIPCoverTopfeature = value; }
        public FeatureLists EIPCoverBtmFeature { get => EIPCoverBtmfeature; set => EIPCoverBtmfeature = value; }
        public FeatureLists EIPBaseTopFeature { get => EIPBaseTopfeature; set => EIPBaseTopfeature = value; }
        public FeatureLists EIPBaseBtmFeature { get => EIPBaseBtmfeature; set => EIPBaseBtmfeature = value; }
        #endregion

        public EUVPositionRecipe()
        {
            EIPCoverTopfeature = new FeatureLists();
            EIPCoverBtmfeature = new FeatureLists();
            EIPBaseTopfeature = new FeatureLists();
            EIPBaseBtmfeature = new FeatureLists();
        }

        public override void Clear()
        {
            EIPCoverTopfeature.Clear();
            EIPCoverBtmfeature.Clear();
            EIPBaseTopfeature.Clear();
            EIPBaseBtmfeature.Clear();
        }

        public override bool Read(string recipePath)
        {
            bool rst = true;
            //foreach (RecipeType_ImageData featureData in listAlignFeature)
            //{
            //    // Load
            //    if (!featureData.Read(recipePath))
            //    {
            //        rst = false;
            //        break;
            //    }
            //}

            //foreach (RecipeType_ImageData featureData in listPositionFeature)
            //{
            //    // Load
            //    if (!featureData.Read(recipePath))
            //    {
            //        rst = false;
            //        break;
            //    }
            //}

            return rst;
        }

        public override bool Save(string recipePath)
        {
            bool rst = true;
            //foreach (RecipeType_ImageData featureData in listAlignFeature)
            //{
            //    // FileName Setting
            //    if (featureData.FileName == "")
            //    {
            //        string filename = "alignment_feature_";
            //        int num = 0;
            //        while (File.Exists(recipePath + "\\" + filename + num.ToString() + ".bmp") == true)
            //        {
            //            num++;
            //        }

            //        featureData.FileName = filename + num.ToString() + ".bmp";
            //    }

            //    // Save
            //    if (!featureData.Save(recipePath))
            //    {
            //        rst = false;
            //        break;
            //    }
            //}

            //foreach (RecipeType_ImageData featureData in listPositionFeature)
            //{
            //    // FileName Setting
            //    if (featureData.FileName == "")
            //    {
            //        string filename = "position_feature_";
            //        int num = 0;
            //        while (File.Exists(recipePath + "\\" + filename + num.ToString() + ".bmp") == true)
            //        {
            //            num++;
            //        }

            //        featureData.FileName = filename + num.ToString() + ".bmp";
            //    }

            //    // Save
            //    if (!featureData.Save(recipePath))
            //    {
            //        rst = false;
            //        break;
            //    }
            //}

            return rst;
        }
    }
}
