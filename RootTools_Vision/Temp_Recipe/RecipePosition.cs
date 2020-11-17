using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{ 
    public class RecipePosition : IRecipe
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

        public RecipePosition()
        {
            listMasterFeature = new List<RecipeType_FeatureData>();
            listShotFeature = new List<RecipeType_FeatureData>();
            listChipFeature = new List<RecipeType_FeatureData>();

            indexMaxScoreMasterFeature = -1;
            indexMaxScoreShotFeature = -1;
            indexMaxScoreChipFeature =-1;
        }

        void AddMasterFeature(RecipeType_FeatureData featureData)
        {
            listMasterFeature.Add(featureData);
        }

        void AddMasterFeature(int positionX, int positionY, int featureWidth, int featureHeight, byte[] rawData)
        {
            listMasterFeature.Add(new RecipeType_FeatureData(positionX, positionY, featureWidth, featureHeight, rawData));
        }

        void AddShotFeature(RecipeType_FeatureData featureData)
        {
            listShotFeature.Add(featureData);
        }

        void AddShotFeature(int positionX, int positionY, int featureWidth, int featureHeight, byte[] rawData)
        {
            listShotFeature.Add(new RecipeType_FeatureData(positionX, positionY, featureWidth, featureHeight, rawData));
        }

        void AddChipFeature(RecipeType_FeatureData featureData)
        {
            listChipFeature.Add(featureData);
        }

        void AddChipFeature(int positionX, int positionY, int featureWidth, int featureHeight, byte[] rawData)
        {
            listChipFeature.Add(new RecipeType_FeatureData(positionX, positionY, featureWidth, featureHeight, rawData));
        }

        public void Load()
        {

        }

        public void Save()
        {

        }
    }
}
