using RootTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace RootTools_Vision
{ 
    public class RecipeType_ImageData
    {
        byte[] rawData = new byte[0];
        byte[] rawDataR = new byte[0];
        byte[] rawDataG = new byte[0];
        byte[] rawDataB = new byte[0];

        int positionX;
        int positionY;
        int centerPositionX;
        int centerPositionY;
        int width;
        int height;
        int byteCnt;
        string fileName = string.Empty;


        [XmlIgnore] public byte[] RawData { get => rawData; set => rawData = value; }
        public int PositionX { get => positionX; set => positionX = value; }
        public int PositionY { get => positionY; set => positionY = value; }
        public int CenterPositionX { get => centerPositionX; set => centerPositionX = value; }
        public int CenterPositionY { get => centerPositionY; set => centerPositionY = value; }
        public int Width { get => width; set => width = value; }
        public int Height { get => height; set => height = value; }
        public int ByteCnt { get => byteCnt; set => byteCnt = value; }
        public string FileName { get => this.fileName; set => this.fileName = value; }
        public RecipeType_ImageData()
        {
            this.FileName = string.Empty;
        }
        public RecipeType_ImageData(int positionX, int positionY, int featureWidth, int featureHeight, int byteCnt, byte[] rawData)
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


            if(this.byteCnt == 3)
            {
                this.rawDataR = new byte[this.Width * this.Height];
                this.rawDataG = new byte[this.Width * this.Height];
                this.rawDataB = new byte[this.Width * this.Height];

                Tools.SpliteColor(this.rawData, this.rawDataR, this.rawDataG, this.rawDataB);

                Tools.SaveRawdataToBitmap("D:\\Image.bmp", this.rawData, this.width, this.height, this.byteCnt);
                Tools.SaveRawdataToBitmap("D:\\ImageR.bmp", this.rawDataR, this.width, this.height, 1);
                Tools.SaveRawdataToBitmap("D:\\ImageG.bmp", this.rawDataG, this.width, this.height, 1);
                Tools.SaveRawdataToBitmap("D:\\ImageB.bmp", this.rawDataB, this.width, this.height, 1);
            }
        }

        public void SetRawData(byte[] rawData)
        {
            this.RawData = rawData;
        }

        public bool Save(string recipeFolderPath)
        {
            return Tools.SaveRawdataToBitmap(recipeFolderPath + this.FileName , RawData, Width, Height, ByteCnt);
        }

        public bool Read(string recipeFolderPath)  
        {
            if(File.Exists(recipeFolderPath + this.FileName) == false)
            {
                MessageBox.Show("Master Image가 없습니다.", "Error Master Image Laod",MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            Bitmap bmp = new Bitmap(recipeFolderPath + this.FileName);

            this.Width = bmp.Width;
            this.Height = bmp.Height;

            this.rawData = new byte[this.Width * this.Height * this.byteCnt];
            Tools.LoadBitmapToRawdata(recipeFolderPath + this.FileName, this.rawData, this.Width, this.Height, this.ByteCnt);
            if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                this.ByteCnt = 1;
                this.rawDataR = new byte[this.Width * this.Height];

                Array.Copy(this.rawData, this.rawDataR, rawData.Length);
            }
            else
            {
                this.ByteCnt = 3;
               
                this.rawDataR = new byte[this.Width * this.Height];
                this.rawDataG = new byte[this.Width * this.Height];
                this.rawDataB = new byte[this.Width * this.Height];

                Tools.SpliteColor(this.rawData, this.rawDataR, this.rawDataG, this.rawDataB);
            }
            
            //Tools.SaveRawdataToBitmap("D:\\Image.bmp", this.rawData, this.width, this.height, this.byteCnt);
            //Tools.SaveRawdataToBitmap("D:\\ImageR.bmp", this.rawDataR, this.width, this.height, 1);
            //Tools.SaveRawdataToBitmap("D:\\ImageG.bmp", this.rawDataG, this.width, this.height, 1);
            //Tools.SaveRawdataToBitmap("D:\\ImageB.bmp", this.rawDataB, this.width, this.height, 1);

            return true;
        }

        public byte[] GetColorRowData(IMAGE_CHANNEL channel)
        {
            switch (channel)
            {
                case IMAGE_CHANNEL.R_GRAY:
                    return this.rawDataR;
                case IMAGE_CHANNEL.G:
                    return this.rawDataG;
                case IMAGE_CHANNEL.B:
                    return this.rawDataB;
            }
            return this.rawDataR;
        }

        public Bitmap GetFeatureBitmap()
        {
            return Tools.CovertArrayToBitmap(this.rawData, this.width, this.height, this.byteCnt);
        }
        
    }
}