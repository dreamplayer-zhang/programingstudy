using RootTools_CLR;
using RootTools_Vision.Temp_Recipe;
using RootTools_Vision.UserTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class Position_Chip : IWork
    {
        public WORK_TYPE Type => WORK_TYPE.MAINWORK;

        Workplace workplace;

        RecipePosition recipeData;
        ParameterPosition parameterData;

        public void DoWork()
        {
            DoPosition();
        }

        public void SetData(IRecipeData _recipeData, IParameterData _parameterData)
        {
            //this.recipeData = _recipeData as RecipePosition;
            //this.parameterData = _parameterData as ParameterPosition;
        }

        public void SetWorkplace(Workplace _workplace)
        {
            this.workplace = _workplace;
        }

        public bool DoPosition()
        {
            if (this.workplace.MapPositionX == -1 && this.workplace.MapPositionX == -1) // Master
                return true;

            if (this.recipeData.IndexMaxScoreChipFeature == -1) // Feature가 셋팅이 안되어 있는 경우 전체 Feature 사용
            {
                int outX = 0, outY = 0;
                int maxX = 0, maxY = 0;
                float score = 0;
                float maxScore = 0;
                int i = 0;
                int maxIndex = 0;

                foreach (RecipeType_FeatureData feature in this.recipeData.ListMasterFeature)
                {
                    int startX = (feature.PositionX - this.parameterData.SearchRangeX) < 0 ? 0 : (feature.PositionX - this.parameterData.SearchRangeX);
                    int startY = (feature.PositionY - this.parameterData.SearchRangeY) < 0 ? 0 : (feature.PositionY - this.parameterData.SearchRangeY);
                    int endX = (feature.PositionX + feature.FeatureWidth + this.parameterData.SearchRangeX) >= this.workplace.SharedBufferWidth ? this.workplace.SharedBufferWidth : (feature.PositionX + feature.FeatureWidth + this.parameterData.SearchRangeX);
                    int endY = (feature.PositionY + feature.FeatureHeight + this.parameterData.SearchRangeY) >= this.workplace.SharedBufferHeight ? this.workplace.SharedBufferHeight : (feature.PositionY + feature.FeatureHeight + this.parameterData.SearchRangeY);

                    unsafe
                    {
                        score = CLR_IP.Cpp_TemplateMatching(
                            (byte*)this.workplace.SharedBuffer.ToPointer(), feature.RawData, &outX, &outY,
                            this.workplace.SharedBufferWidth, this.workplace.SharedBufferHeight,
                            feature.FeatureWidth, feature.FeatureHeight,
                            startX, startY, endX, endY, 5);
                    }

                    if (score > maxScore)
                    {
                        maxScore = score;
                        maxX = outX;
                        maxY = outY;
                        maxIndex = i;
                    }

                    i++;
                }


                if (maxScore < this.parameterData.MinScoreLimit)
                {
                    return false;
                }


                int tplStartX = this.recipeData.ListMasterFeature[maxIndex].PositionX;
                int tplStartY = this.recipeData.ListMasterFeature[maxIndex].PositionY;
                int tplW = this.recipeData.ListMasterFeature[maxIndex].FeatureWidth;
                int tplH = this.recipeData.ListMasterFeature[maxIndex].FeatureHeight;

                float tplCenterX = (float)this.recipeData.ListMasterFeature[maxIndex].CenterPositionX;
                float tplCenterY = (float)this.recipeData.ListMasterFeature[maxIndex].CenterPositionY;

                maxX -= tplStartX;
                maxY -= tplStartY;


                // ROI 중심 위치
                float centerROIX = tplStartX + tplW / 2;
                float centerROIY = tplStartY + tplH / 2;

                // Matching 중심 위치
                float centerMatchingX = (int)(maxX + tplCenterX);
                float centerMatchingY = (int)(maxY + tplCenterY);

                int transX = (int)(centerROIX - centerMatchingX);
                int transY = (int)(centerROIY - centerMatchingY);

                if (this.workplace.Index == 0)  // Master
                {
                    this.workplace.SetImagePositionByTrans(transX, transY);
                }
            }
            else
            {
                int maxX = 0, maxY = 0;
                float maxScore = 0;

                RecipeType_FeatureData feature = this.recipeData.ListMasterFeature[this.recipeData.IndexMaxScoreChipFeature];

                int startX = (feature.PositionX - this.parameterData.SearchRangeX) < 0 ? 0 : (feature.PositionX - this.parameterData.SearchRangeX);
                int startY = (feature.PositionY - this.parameterData.SearchRangeY) < 0 ? 0 : (feature.PositionY - this.parameterData.SearchRangeY);
                int endX = (feature.PositionX + feature.FeatureWidth + this.parameterData.SearchRangeX) >= this.workplace.SharedBufferWidth ? this.workplace.SharedBufferWidth : (feature.PositionX + feature.FeatureWidth + this.parameterData.SearchRangeX);
                int endY = (feature.PositionY + feature.FeatureHeight + this.parameterData.SearchRangeY) >= this.workplace.SharedBufferHeight ? this.workplace.SharedBufferHeight : (feature.PositionY + feature.FeatureHeight + this.parameterData.SearchRangeY);

                unsafe
                {
                    maxScore = CLR_IP.Cpp_TemplateMatching(
                        (byte*)this.workplace.SharedBuffer.ToPointer(), feature.RawData, &maxX, &maxY,
                        this.workplace.SharedBufferWidth, this.workplace.SharedBufferHeight,
                        feature.FeatureWidth, feature.FeatureHeight,
                        startX, startY, endX, endY, 5);
                }



                if (maxScore < this.parameterData.MinScoreLimit)
                {
                    return false;
                }


                int tplStartX = feature.PositionX;
                int tplStartY = feature.PositionY;
                int tplW = feature.FeatureWidth;
                int tplH = feature.FeatureHeight;

                float tplCenterX = (float)feature.CenterPositionX;
                float tplCenterY = (float)feature.CenterPositionY;

                maxX -= tplStartX;
                maxY -= tplStartY;


                // ROI 중심 위치
                float centerROIX = tplStartX + tplW / 2;
                float centerROIY = tplStartY + tplH / 2;

                // Matching 중심 위치
                float centerMatchingX = (int)(maxX + tplCenterX);
                float centerMatchingY = (int)(maxY + tplCenterY);

                int transX = (int)(centerROIX - centerMatchingX);
                int transY = (int)(centerROIY - centerMatchingY);

                if (this.workplace.Index == 0)  // Master
                {
                    this.workplace.SetImagePositionByTrans(transX, transY);
                }
            }

            return true;
        }
    }
}
