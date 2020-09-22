using RootTools_Vision.UserTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RootTools_CLR;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using RootTools_Vision.Temp_Recipe;

namespace RootTools_Vision
{
    public class Position : IWork
    {
        public WORK_TYPE Type => WORK_TYPE.PREPARISON;

        Workplace workplace;

        RecipeData_Origin recipeData_Origin;
        RecipeData_Position recipeData_Position;

        ParamData_Position parameterData_Position;

        public void DoWork()
        {
            DoPosition();
        }

        public void SetData(IRecipeData _recipeData, IParameterData _parameterData)
        {
            Recipe recipe = _recipeData as Recipe;
            Parameter parameter = _parameterData as Parameter;

            this.recipeData_Origin = recipe.GetRecipeData(typeof(RecipeData_Origin)) as RecipeData_Origin;
            this.recipeData_Position = recipe.GetRecipeData(typeof(RecipeData_Position)) as RecipeData_Position;

            this.parameterData_Position = parameter.GetParameter(typeof(ParamData_Position)) as ParamData_Position;
        }

        public void SetWorkplace(Workplace _workplace)
        {
            this.workplace = _workplace;
        }

        public bool DoPosition()
        {
            if(this.workplace.MapPositionX == -1 && this.workplace.MapPositionX == -1) // Master
            {
                if (this.recipeData_Position.IndexMaxScoreMasterFeature == -1) // Feature가 셋팅이 안되어 있는 경우 전체 Feature 사용
                {
                    int outX = 0, outY = 0;
                    int maxX = 0, maxY = 0;
                    float score = 0;
                    float maxScore = 0;
                    int i = 0;
                    int maxIndex = 0;

                    foreach(RecipeType_FeatureData feature in this.recipeData_Position.ListMasterFeature)
                    {
                        int startX = (feature.PositionX - this.parameterData_Position.SearchRangeX) < 0 ? 0 : (feature.PositionX - this.parameterData_Position.SearchRangeX);
                        int startY = (feature.PositionY - this.parameterData_Position.SearchRangeY) < 0 ? 0 : (feature.PositionY - this.parameterData_Position.SearchRangeY);
                        int endX = (feature.PositionX + feature.FeatureWidth + this.parameterData_Position.SearchRangeX) >= this.workplace.SharedBufferWidth ? this.workplace.SharedBufferWidth : (feature.PositionX + feature.FeatureWidth + this.parameterData_Position.SearchRangeX);
                        int endY = (feature.PositionY + feature.FeatureHeight + this.parameterData_Position.SearchRangeY) >= this.workplace.SharedBufferHeight ? this.workplace.SharedBufferHeight : (feature.PositionY + feature.FeatureHeight + this.parameterData_Position.SearchRangeY);

                        unsafe
                        {
                            score =  CLR_IP.Cpp_TemplateMatching(
                                (byte*)this.workplace.SharedBuffer.ToPointer(), feature.RawData, &outX, &outY, 
                                this.workplace.SharedBufferWidth, this.workplace.SharedBufferHeight,
                                feature.FeatureWidth, feature.FeatureHeight,
                                startX, startY, endX, endY, 5);
                        }

                        if( score > maxScore)
                        {
                            maxScore = score;
                            maxX = outX;
                            maxY = outY;
                            maxIndex = i;
                        }

                        i++;
                    }


                    if (maxScore < this.parameterData_Position.MinScoreLimit)
                    {
                        return false;
                    }


                    int tplStartX = this.recipeData_Position.ListMasterFeature[maxIndex].PositionX;
                    int tplStartY = this.recipeData_Position.ListMasterFeature[maxIndex].PositionY;
                    int tplW = this.recipeData_Position.ListMasterFeature[maxIndex].FeatureWidth;
                    int tplH = this.recipeData_Position.ListMasterFeature[maxIndex].FeatureHeight;

                    float tplCenterX = (float)this.recipeData_Position.ListMasterFeature[maxIndex].CenterPositionX;
                    float tplCenterY = (float)this.recipeData_Position.ListMasterFeature[maxIndex].CenterPositionY;

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

                }
            }

            return true;
        }




    }
}
