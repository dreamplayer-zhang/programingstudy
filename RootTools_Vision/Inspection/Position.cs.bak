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
using RootTools;
using System.Windows;

namespace RootTools_Vision
{
    public class Position : WorkBase
    {

        public override WORK_TYPE Type => WORK_TYPE.PREPARISON;

        Workplace workplace;

        RecipeData recipeData;
        Parameter parameter;

        RecipeData_Origin recipeData_Origin;
        RecipeData_Position recipeData_Position;

        ParamData_Position parameterData_Position;

        public override void DoWork()
        {
            DoPosition();
        }

        public override void SetData(IRecipeData _recipeData, IParameterData _parameterData)
        {
            m_sName = this.GetType().Name;

            this.recipeData = _recipeData as RecipeData;
            this.parameter = _parameterData as Parameter;

            this.recipeData_Origin = recipeData.GetRecipeData(typeof(RecipeData_Origin)) as RecipeData_Origin;
            this.recipeData_Position = recipeData.GetRecipeData(typeof(RecipeData_Position)) as RecipeData_Position;

            this.parameterData_Position = parameter.GetParameter(typeof(ParamData_Position)) as ParamData_Position;
        }

        public override void SetWorkplace(Workplace _workplace)
        {
            this.workplace = _workplace;
        }

        public CPoint ConvertRelToAbs(CPoint ptRel)
        {
            return new CPoint(this.recipeData_Origin.OriginX + ptRel.X, this.recipeData_Origin.OriginY + ptRel.Y);
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
                        CPoint absPos = ConvertRelToAbs(new CPoint(feature.PositionX, feature.PositionY));
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

                    CPoint ptAbs = ConvertRelToAbs(new CPoint(this.recipeData_Position.ListMasterFeature[maxIndex].PositionX, this.recipeData_Position.ListMasterFeature[maxIndex].PositionY));
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
                        this.workplace.SetImagePositionByTrans(transX, transY, true);
                    }

                    WorkEventManager.OnPositionDone(this.workplace, new PositionDoneEventArgs(new CPoint(maxStartX, maxStartY), new CPoint(maxEndX, maxEndY),
                            new CPoint(maxStartX + transX, maxStartY + transY), new CPoint(maxEndX + transX, maxEndY + transY)));
                        
                }
                else
                {

                }
            }

            return true;
        }

        public override WorkBase Clone()
        {

            Position clone = new Position();
            try
            {
                clone.SetData(this.recipeData, this.parameter);
                clone.SetWorkplace(this.workplace);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            

            return clone;
        }

        public override void SetWorkplaceBundle(WorkplaceBundle workplace)
        {
            return;
        }
    }
}
