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
        byte[] rawData = new byte[0];
        int positionX;
        int positionY;
        int centerPositionX;
        int centerPositionY;
        int width;
        int height;
        int byteCnt;
        string fileName = string.Empty;

        [XmlIgnore] public Bitmap FeatureBitmap { get => featureBitmap; set => featureBitmap = value; }
        [XmlIgnore] public byte[] RawData { get => rawData; set => rawData = value; }
        public int PositionX { get => positionX; set => positionX = value; }
        public int PositionY { get => positionY; set => positionY = value; }
        public int CenterPositionX { get => centerPositionX; set => centerPositionX = value; }
        public int CenterPositionY { get => centerPositionY; set => centerPositionY = value; }
        public int Width { get => width; set => width = value; }
        public int Height { get => height; set => height = value; }
        public int ByteCnt { get => byteCnt; set => byteCnt = value; }
        public string FileName { get => this.fileName; set => this.fileName = value; }
        public RecipeType_FeatureData()
        {
            this.FileName = string.Empty;
        }
        public RecipeType_FeatureData(int positionX, int positionY, int featureWidth, int featureHeight, int byteCnt, byte[] rawData)
        {
            this.positionX = positionX;
            this.positionY = positionY;
            this.width = featureWidth;
            this.height = featureHeight;
            this.byteCnt = byteCnt;
            this.centerPositionX = positionX + this.width / 2;
            this.centerPositionY = positionY + this.height / 2;
            this.rawData = rawData;
            this.FileName = string.Empty;
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

        public bool Save(string recipeFolderPath)
        {
            return Tools.SaveRawdataToBitmap(recipeFolderPath + this.FileName , RawData, Width, Height, ByteCnt);
        }

        public bool Read(string recipeFolderPath)
        {
            //this.rawData = new byte[this.featureWidth * this.featureHeight * this.byteCnt];
            return Tools.LoadBitmapToRawdata(recipeFolderPath + this.FileName, ref this.rawData, ref this.width, ref this.height, ref this.byteCnt);
        }

        public Bitmap GetFeatureBitmap()
        {
            return Tools.CovertArrayToBitmap(this.rawData, this.width, this.height, this.byteCnt);
        }
    }
}
