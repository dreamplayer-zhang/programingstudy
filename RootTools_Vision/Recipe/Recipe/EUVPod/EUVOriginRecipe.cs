using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class EUVOriginRecipe : RecipeItemBase
    {
        OriginInfo TDIoriginInfo, stainOriginInfo, sideLROriginInfo, sideTBOriginInfo;
        #region [Property]
        public OriginInfo TDIOriginInfo
        {
            get => TDIoriginInfo;
            set => SetProperty(ref TDIoriginInfo, value);
        }
        public OriginInfo StainOriginInfo
        {
            get => stainOriginInfo;
            set => SetProperty(ref stainOriginInfo, value);
        }
        public OriginInfo SideLROriginInfo
        {
            get => sideLROriginInfo;
            set => SetProperty(ref sideLROriginInfo, value);
        }
        public OriginInfo SideTBOriginInfo
        {
            get => sideTBOriginInfo;
            set => SetProperty(ref sideTBOriginInfo, value);
        }
        #endregion
        public EUVOriginRecipe()
        {
            TDIoriginInfo = new OriginInfo();
            stainOriginInfo = new OriginInfo();
            sideLROriginInfo = new OriginInfo();
            sideTBOriginInfo = new OriginInfo();
        }
        public override bool Read(string recipePath)
        {
            return true;
        }

        public override bool Save(string recipePath)
        {
            return true;
        }

        public override void Clear()
        {
            TDIOriginInfo.Clear();
            StainOriginInfo.Clear();
            SideLROriginInfo.Clear();
            SideTBOriginInfo.Clear();
        }
        public void SaveMasterImage(string RecipeFolderPath, string fileName, int _posX, int _posY, int _width, int _height, int _byteCnt, byte[] _rawData,OriginInfo originInfo)
        {
            originInfo.SaveMasterImage(RecipeFolderPath, fileName,_posX,_posY,_width,_height,_byteCnt,_rawData);
        }
        public void LoadMasterImage(string RecipeFolderPath, string fileName,OriginInfo originInfo)
        {
            originInfo.LoadMasterImage(RecipeFolderPath, fileName);
        }
    }
    [Serializable]
    public class OriginInfo : ObservableObject
    {
        CPoint origin, originSize;
        public OriginInfo()
        {
            Origin = new CPoint(0, 0);
            OriginSize = new CPoint(0, 0);
        }
        public CPoint Origin
        {
            get => origin;
            set => SetProperty(ref origin, value);
        }
        public CPoint OriginSize { get => originSize; set => SetProperty(ref originSize, value); }
        RecipeType_ImageData masterImage;
        public RecipeType_ImageData MasterImage
        {
            get => masterImage;
            set => masterImage = value;
        }
        public void LoadMasterImage(string RecipeFolderPath, string fileName)
        {
            if (RecipeFolderPath == "") return;

            MasterImage = new RecipeType_ImageData();
            MasterImage.FileName = fileName;
            MasterImage.Read(RecipeFolderPath);
        }

        public void SaveMasterImage(string RecipeFolderPath, string fileName, int _posX, int _posY, int _width, int _height, int _byteCnt, byte[] _rawData)
        {
            MasterImage = new RecipeType_ImageData(_posX, _posY, _width, _height, _byteCnt, _rawData);
            MasterImage.FileName = fileName;
            MasterImage.Save(RecipeFolderPath);
        }
        public void Clear()
        {
            origin.X = 0;
            origin.Y = 0;
            originSize.X = 0;
            originSize.Y = 0;
        }
    }

}
