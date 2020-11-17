using RootTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RootTools_Vision
{ 
    public class RecipeType_FeatureData
    {

        Bitmap featureBitmap;
        byte[] rawData;
        int positionX;
        int positionY;
        int centerPositionX;
        int centerPositionY;
        int featureWidth;
        int featureHeight;
        int byteCnt;

        [XmlIgnore] public Bitmap FeatureBitmap { get => featureBitmap; set => featureBitmap = value; }
        [XmlIgnore] public byte[] RawData { get => rawData; set => rawData = value; }
        public int PositionX { get => positionX; set => positionX = value; }
        public int PositionY { get => positionY; set => positionY = value; }
        public int CenterPositionX { get => centerPositionX; set => centerPositionX = value; }
        public int CenterPositionY { get => centerPositionY; set => centerPositionY = value; }
        public int FeatureWidth { get => featureWidth; set => featureWidth = value; }
        public int FeatureHeight { get => featureHeight; set => featureHeight = value; }
        public int ByteCnt { get => byteCnt; set => byteCnt = value; }
        public RecipeType_FeatureData()
        {
        }
        public RecipeType_FeatureData(int positionX, int positionY, int featureWidth, int featureHeight, int byteCnt)
        {
            this.positionX = positionX;
            this.positionY = positionY;
            this.featureWidth = featureWidth;
            this.featureHeight = featureHeight;
            this.byteCnt = byteCnt;
            this.centerPositionX = positionX + featureWidth / 2;
            this.centerPositionY = positionY + featureHeight / 2;
        }

        public void SetRawData(byte[] rawData)
        {
            this.RawData = rawData;
        }

        public void SetImageData(byte[] rawData, Bitmap bitmap)
        {
            this.FeatureBitmap = bitmap;
            this.RawData = rawData;
        }


        public RecipeType_FeatureData(int positionX, int positionY, int featureWidth, int featureHeight, byte[] rawData)
        {
            this.rawData = rawData;
            this.positionX = positionX;
            this.positionY = positionY;
            this.featureWidth = featureWidth;
            this.featureHeight = featureHeight;
            this.centerPositionX = positionX + featureWidth/2;
            this.centerPositionY = positionY + featureHeight/2;
        }
    }
}
