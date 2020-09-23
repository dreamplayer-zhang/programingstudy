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
            RecipeData recipe = _recipeData as RecipeData;
            Parameter parameter = _parameterData as Parameter;

            this.recipeData_Origin = recipe.GetRecipeData(typeof(RecipeData_Origin)) as RecipeData_Origin;
            this.recipeData_Position = recipe.GetRecipeData(typeof(RecipeData_Position)) as RecipeData_Position;

            this.parameterData_Position = parameter.GetParameter(typeof(ParamData_Position)) as ParamData_Position;
        }

        public void SetWorkplace(Workplace _workplace)
        {
            this.workplace = _workplace;
        }

        public Point ConvertRelToAbs(Point ptRel)
        {
            return new Point(this.recipeData_Origin.OriginX - ptRel.X, this.recipeData_Origin.OriginY - ptRel.Y);
        }

        public bool DoPosition()
        {
            if(this.workplace.MapPositionX == -1 && this.workplace.MapPositionX == -1) // Master
            {
                if (this.recipeData_Position.IndexMaxScoreMasterFeature == -1) // Feature가 셋팅이 안되어 있는 경우 전체 Feature 사용
                {
                    int outX = 0, outY = 0;
                    int maxX = 0, maxY = 0;
                    int maxStartX = 0, maxStartY = 0;
                    int maxEndX = 0, maxEndY = 0;
                    float score = 0;
                    float maxScore = 0;
                    int i = 0;
                    int maxIndex = -1;

                    foreach(RecipeType_FeatureData feature in this.recipeData_Position.ListMasterFeature)
                    {
                        Point absPos = ConvertRelToAbs(new Point(feature.PositionX, feature.PositionY));
                        int startX = (absPos.X - this.parameterData_Position.SearchRangeX) < 0 ? 0 : (absPos.X - this.parameterData_Position.SearchRangeX);
                        int startY = (absPos.Y - this.parameterData_Position.SearchRangeY) < 0 ? 0 : (absPos.Y - this.parameterData_Position.SearchRangeY);
                        int endX = (absPos.X + feature.FeatureWidth + this.parameterData_Position.SearchRangeX) >= this.workplace.SharedBufferWidth ? this.workplace.SharedBufferWidth : (absPos.X + feature.FeatureWidth + this.parameterData_Position.SearchRangeX);
                        int endY = (absPos.Y + feature.FeatureHeight + this.parameterData_Position.SearchRangeY) >= this.workplace.SharedBufferHeight ? this.workplace.SharedBufferHeight : (absPos.Y + feature.FeatureHeight + this.parameterData_Position.SearchRangeY);

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

                            maxStartX = startX;
                            maxStartY = startY;
                            maxEndX = endX;
                            maxEndY = endY;
                        }

                        i++;
                    }

                    if (maxIndex == -1) return false;

                    if (maxScore < this.parameterData_Position.MinScoreLimit)
                    {
                        return false;
                    }

                    Point ptAbs = ConvertRelToAbs(new Point(this.recipeData_Position.ListMasterFeature[maxIndex].PositionX, this.recipeData_Position.ListMasterFeature[maxIndex].PositionY));
                    int tplStartX = ptAbs.X;
                    int tplStartY = ptAbs.Y;
                    int tplW = this.recipeData_Position.ListMasterFeature[maxIndex].FeatureWidth;
                    int tplH = this.recipeData_Position.ListMasterFeature[maxIndex].FeatureHeight;

                    float tplCenterX = (float)ptAbs.X + tplW/2;
                    float tplCenterY = (float)ptAbs.Y + tplH/2;

                    maxX += maxStartX; // ROI에서 image 좌표로 변환
                    maxY += maxStartY;

                    // ROI 중심 위치
                    float centerROIX = (maxStartX + maxEndX)/2;
                    float centerROIY = (maxStartY + maxEndY)/ 2;

                    // Matching 중심 위치
                    float centerMatchingX = (int)(maxX + tplW / 2);
                    float centerMatchingY = (int)(maxY + tplH / 2);

                    //int transX = (int)(centerROIX - centerMatchingX);
                    //int transY = (int)(centerROIY - centerMatchingY);

                    int transX = (int)(centerMatchingX - centerROIX);
                    int transY = (int)(centerMatchingY - centerROIY);

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
