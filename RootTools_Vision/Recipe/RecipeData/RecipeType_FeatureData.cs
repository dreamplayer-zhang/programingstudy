using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{ 
    public class RecipeType_FeatureData
    {
        byte[] rawData;
        int positionX;
        int positionY;
        int centerPositionX;
        int centerPositionY;
        int featureWidth;
        int featureHeight;

        public byte[] RawData { get => rawData; set => rawData = value; }
        public int PositionX { get => positionX; set => positionX = value; }
        public int PositionY { get => positionY; set => positionY = value; }
        public int CenterPositionX { get => centerPositionX; set => centerPositionX = value; }
        public int CenterPositionY { get => centerPositionY; set => centerPositionY = value; }
        public int FeatureWidth { get => featureWidth; set => featureWidth = value; }
        public int FeatureHeight { get => featureHeight; set => featureHeight = value; }

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
