using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
using Emgu.CV;
using Emgu.Util;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Xml.Serialization;
using System.IO;

namespace RootTools_Vision
{
    public class RecipeData_Position : IRecipeData
    {
        List<RecipeType_FeatureData> listMasterFeature;
        List<RecipeType_FeatureData> listShotFeature;
        List<RecipeType_FeatureData> listChipFeature;

        int indexMaxScoreMasterFeature;
        int indexMaxScoreShotFeature;
        int indexMaxScoreChipFeature;

        #region [Getter Setter]
        public List<RecipeType_FeatureData> ListMasterFeature { get => listMasterFeature; set => listMasterFeature = value; }
        public List<RecipeType_FeatureData> ListShotFeature { get => listShotFeature; set => listShotFeature = value; }
        public List<RecipeType_FeatureData> ListDieFeature { get => listChipFeature; set => listChipFeature = value; }
        public int IndexMaxScoreMasterFeature { get => indexMaxScoreMasterFeature; set => indexMaxScoreMasterFeature = value; }
        public int IndexMaxScoreShotFeature { get => indexMaxScoreShotFeature; set => indexMaxScoreShotFeature = value; }
        public int IndexMaxScoreChipFeature { get => indexMaxScoreChipFeature; set => indexMaxScoreChipFeature = value; }
        #endregion

        public RecipeData_Position()
        {
            listMasterFeature = new List<RecipeType_FeatureData>();
            listShotFeature = new List<RecipeType_FeatureData>();
            listChipFeature = new List<RecipeType_FeatureData>();

            indexMaxScoreMasterFeature = -1;
            indexMaxScoreShotFeature = -1;
            indexMaxScoreChipFeature = -1;
        }

        public void SaveFeatures(string sRecipePath)
        {
            int nCount = 0;
            string sFileName = Path.GetFileNameWithoutExtension(sRecipePath);
            string sResultFileName = "";
            for (int i = 0; i < listMasterFeature.Count; i++)
            {
                sResultFileName = String.Format("{0}_Master_{1}.bmp", sFileName, nCount++);
                byte[] rawdata = listMasterFeature[i].RawData;
                int nWidth = listMasterFeature[i].FeatureWidth;
                int nHeight = listMasterFeature[i].FeatureHeight;
                int nByteCnt = listMasterFeature[i].ByteCnt;
                ImageHelper.FileSaveBitmap(Path.Combine(sRecipePath, sResultFileName), rawdata, nWidth, nHeight, nByteCnt);
            }

            nCount = 0;
            for (int i = 0; i < listShotFeature.Count; i++)
            {
                sResultFileName = String.Format("{0}_Shot_{1}.bmp", sFileName, nCount++);
                byte[] rawdata = listShotFeature[i].RawData;
                int nWidth = listShotFeature[i].FeatureWidth;
                int nHeight = listShotFeature[i].FeatureHeight;
                int nByteCnt = listShotFeature[i].ByteCnt;
                ImageHelper.FileSaveBitmap(Path.Combine(sRecipePath, sResultFileName), rawdata, nWidth, nHeight, nByteCnt);
            }

            nCount = 0;
            for (int i = 0; i < listChipFeature.Count; i++)
            {
                sResultFileName = String.Format("{0}_Die_{1}.bmp", sFileName, nCount++);
                byte[] rawdata = listChipFeature[i].RawData;
                int nWidth = listChipFeature[i].FeatureWidth;
                int nHeight = listChipFeature[i].FeatureHeight;
                int nByteCnt = listChipFeature[i].ByteCnt;
                ImageHelper.FileSaveBitmap(Path.Combine(sRecipePath, sResultFileName), rawdata, nWidth, nHeight, nByteCnt);
            }
        }

        public void LoadFeatures(string sRecipePath)
        {
            int nMaster =0, nShot = 0, nDie = 0;
            DirectoryInfo di = new DirectoryInfo(sRecipePath);
            CPoint ptOffset = new CPoint(0, 0);
            foreach(FileInfo fi in di.GetFiles())
            {
                if (fi.Extension.ToLower().CompareTo(".bmp") == 0)
                {
                    if(fi.Name.Contains("_Master_"))
                    {
                        int nWidth = ListMasterFeature[nMaster].FeatureWidth;
                        int nHeight = ListMasterFeature[nMaster].FeatureHeight;
                        byte[] loadimage = ImageHelper.FileLoadBitmap(fi.FullName, nWidth, nHeight);
                        Bitmap bitmap = new Bitmap(fi.FullName);
                        ListMasterFeature[nMaster++].SetImageData(loadimage, bitmap);
                    }
                    else if (fi.Name.Contains("_Shot_"))
                    {
                        int nWidth = ListShotFeature[nMaster].FeatureWidth;
                        int nHeight = ListShotFeature[nMaster].FeatureHeight;
                        byte[] loadimage = ImageHelper.FileLoadBitmap(fi.FullName, nWidth, nHeight);
                        Bitmap bitmap = new Bitmap(fi.FullName);
                        ListShotFeature[nShot++].SetImageData(loadimage, bitmap);
                    }
                    else
                    {
                        //Die
                        int nWidth = ListDieFeature[nMaster].FeatureWidth;
                        int nHeight = ListDieFeature[nMaster].FeatureHeight;
                        byte[] loadimage = ImageHelper.FileLoadBitmap(fi.FullName, nWidth, nHeight);
                        Bitmap bitmap = new Bitmap(fi.FullName);
                        ListDieFeature[nDie++].SetImageData(loadimage, bitmap);
                    }
                }
            }
        }


        public void AddMasterFeature(RecipeType_FeatureData featureData, ImageData imageData)
        {
            featureData.SetRawData(imageData.m_aBuf);
            listMasterFeature.Add(featureData);
        }

        public void AddShotFeature(RecipeType_FeatureData featureData, ImageData imageData)
        {
            featureData.SetRawData(imageData.m_aBuf);

            listShotFeature.Add(featureData);
        }
        public void AddChipFeature(RecipeType_FeatureData featureData, ImageData imageData)
        {
            featureData.SetRawData(imageData.m_aBuf);

            listChipFeature.Add(featureData);
        }

        public void AddMasterFeature(int positionX, int positionY, int featureWidth, int featureHeight, byte[] rawData)
        {
            listMasterFeature.Add(new RecipeType_FeatureData(positionX, positionY, featureWidth, featureHeight, rawData));
        }

        public void AddShotFeature(int positionX, int positionY, int featureWidth, int featureHeight, byte[] rawData)
        {
            listShotFeature.Add(new RecipeType_FeatureData(positionX, positionY, featureWidth, featureHeight, rawData));
        }

        public void AddChipFeature(int positionX, int positionY, int featureWidth, int featureHeight, byte[] rawData)
        {
            listChipFeature.Add(new RecipeType_FeatureData(positionX, positionY, featureWidth, featureHeight, rawData));
        }
    }
}
