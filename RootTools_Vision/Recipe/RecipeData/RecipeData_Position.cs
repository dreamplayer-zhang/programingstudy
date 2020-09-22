﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
using Emgu.CV;
using Emgu.Util;
using System.Runtime.InteropServices;
using System.Drawing;

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

        public void AddMasterFeature(RecipeType_FeatureData featureData)
        {
            listMasterFeature.Add(featureData);
        }

        public void AddMasterFeature(int positionX, int positionY, int featureWidth, int featureHeight, byte[] rawData)
        {
            listMasterFeature.Add(new RecipeType_FeatureData(positionX, positionY, featureWidth, featureHeight, rawData));
        }

        public void AddShotFeature(RecipeType_FeatureData featureData)
        {
            listShotFeature.Add(featureData);
        }

        public void AddShotFeature(int positionX, int positionY, int featureWidth, int featureHeight, byte[] rawData)
        {
            listShotFeature.Add(new RecipeType_FeatureData(positionX, positionY, featureWidth, featureHeight, rawData));
        }

        public void AddChipFeature(RecipeType_FeatureData featureData)
        {
            listChipFeature.Add(featureData);
        }

        public void AddChipFeature(int positionX, int positionY, int featureWidth, int featureHeight, byte[] rawData)
        {
            listChipFeature.Add(new RecipeType_FeatureData(positionX, positionY, featureWidth, featureHeight, rawData));
        }
    }
}
