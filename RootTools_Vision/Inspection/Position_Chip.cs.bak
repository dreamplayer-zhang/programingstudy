using RootTools;
using RootTools_CLR;
using RootTools_Vision.Temp_Recipe;
using RootTools_Vision.UserTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    public class Position_Chip : WorkBase
    {
        public override WORK_TYPE Type => WORK_TYPE.MAINWORK;

        Workplace workplace;

        RecipeData_Origin recipeData_Origin;
        RecipeData_Position recipeData_Position;

        ParamData_Position parameterData_Position;

        RecipeData recipeData;
        Parameter parameter;

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
            int chipAbsPosX = this.workplace.PositionX; // 칩영역의 Origin 좌표
            int chipAbsPosY = this.workplace.PositionY;

            return new CPoint(chipAbsPosX + ptRel.X, chipAbsPosY + ptRel.Y);
        }

        public bool DoPosition()
        {
            if (this.workplace.MapPositionX == -1 && this.workplace.MapPositionX == -1) // Master
                return true;

            int outX = 0, outY = 0;
            int maxX = 0, maxY = 0;
            int maxStartX = 0, maxStartY = 0;
            int maxEndX = 0, maxEndY = 0;
            float score = 0;
            float maxScore = 0;
            int i = 0;
            int maxIndex = -1;

            if (this.recipeData_Position.IndexMaxScoreChipFeature == -1) // Feature가 셋팅이 안되어 있는 경우 전체 Feature 사용
            {

                foreach (RecipeType_FeatureData feature in this.recipeData_Position.ListDieFeature)
                {
                    CPoint absPos = ConvertRelToAbs(new CPoint(feature.PositionX, feature.PositionY));

                    int startX = (absPos.X - this.parameterData_Position.SearchRangeX) < 0 ? 0 : (absPos.X - this.parameterData_Position.SearchRangeX);
                    int startY = (absPos.Y - this.parameterData_Position.SearchRangeY) < 0 ? 0 : (absPos.Y - this.parameterData_Position.SearchRangeY);
                    int endX = (absPos.X + feature.FeatureWidth + this.parameterData_Position.SearchRangeX) >= this.workplace.SharedBufferWidth ? this.workplace.SharedBufferWidth : (absPos.X + feature.FeatureWidth + this.parameterData_Position.SearchRangeX);
                    int endY = (absPos.Y + feature.FeatureHeight + this.parameterData_Position.SearchRangeY) >= this.workplace.SharedBufferHeight ? this.workplace.SharedBufferHeight : (absPos.Y + feature.FeatureHeight + this.parameterData_Position.SearchRangeY);

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

                        maxStartX = startX;
                        maxStartY = startY;
                        maxEndX = endX;
                        maxEndY = endY;
                    }

                    i++;
                }

            }
            else
            {
                maxIndex = this.recipeData_Position.IndexMaxScoreChipFeature;

                RecipeType_FeatureData feature = this.recipeData_Position.ListDieFeature[maxIndex];

                CPoint absPos = ConvertRelToAbs(new CPoint(feature.PositionX, feature.PositionY));

                int startX = (absPos.X - this.parameterData_Position.SearchRangeX) < 0 ? 0 : (absPos.X - this.parameterData_Position.SearchRangeX);
                int startY = (absPos.Y - this.parameterData_Position.SearchRangeY) < 0 ? 0 : (absPos.Y - this.parameterData_Position.SearchRangeY);
                int endX = (absPos.X + feature.FeatureWidth + this.parameterData_Position.SearchRangeX) >= this.workplace.SharedBufferWidth ? this.workplace.SharedBufferWidth : (absPos.X + feature.FeatureWidth + this.parameterData_Position.SearchRangeX);
                int endY = (absPos.Y + feature.FeatureHeight + this.parameterData_Position.SearchRangeY) >= this.workplace.SharedBufferHeight ? this.workplace.SharedBufferHeight : (absPos.Y + feature.FeatureHeight + this.parameterData_Position.SearchRangeY);

                unsafe
                {
                    score = CLR_IP.Cpp_TemplateMatching(
                        (byte*)this.workplace.SharedBuffer.ToPointer(), feature.RawData, &outX, &outY,
                        this.workplace.SharedBufferWidth, this.workplace.SharedBufferHeight,
                        feature.FeatureWidth, feature.FeatureHeight,
                        startX, startY, endX, endY, 5);
                }

                maxScore = score;
                maxX = outX;
                maxY = outY;
                maxStartX = startX;
                maxStartY = startY;
                maxEndX = endX;
                maxEndY = endY;

            }

            if (maxIndex == -1) return false;

            if (maxScore < this.parameterData_Position.MinScoreLimit)
            {
                this.workplace.SetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS, false);
                return false;
            }

            this.workplace.SetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS, true);

            CPoint ptAbs = ConvertRelToAbs(new CPoint(this.recipeData_Position.ListDieFeature[maxIndex].PositionX, this.recipeData_Position.ListDieFeature[maxIndex].PositionY));
            int tplStartX = ptAbs.X;
            int tplStartY = ptAbs.Y;
            int tplW = this.recipeData_Position.ListDieFeature[maxIndex].FeatureWidth;
            int tplH = this.recipeData_Position.ListDieFeature[maxIndex].FeatureHeight;

            float tplCenterX = (float)ptAbs.X + tplW / 2;
            float tplCenterY = (float)ptAbs.Y + tplH / 2;

            maxX += maxStartX; // ROI에서 image 좌표로 변환
            maxY += maxStartY;

            // ROI 중심 위치
            float centerROIX = (maxStartX + maxEndX) / 2;   //이거 필요없는듯
            float centerROIY = (maxStartY + maxEndY) / 2;

            // Matching 중심 위치
            float centerMatchingX = (int)(maxX + tplW / 2);
            float centerMatchingY = (int)(maxY + tplH / 2);

            //int transX = (int)(centerROIX - centerMatchingX);
            //int transY = (int)(centerROIY - centerMatchingY);

            int transX = (int)(centerMatchingX - tplCenterX);
            int transY = (int)(centerMatchingY - tplCenterY);

            this.workplace.MoveImagePosition(transX, transY);

            maxStartX += this.parameterData_Position.SearchRangeX;
            maxStartY -= this.parameterData_Position.SearchRangeY;
            maxEndX += this.parameterData_Position.SearchRangeX;
            maxEndY -= this.parameterData_Position.SearchRangeY;

            WorkEventManager.OnPositionDone(this.workplace, new PositionDoneEventArgs(new CPoint(tplStartX, tplStartY), new CPoint(tplStartX + tplW, tplStartY + tplH),
                    new CPoint(tplStartX + transX, tplStartY + transY), new CPoint(tplStartX + tplW + transX, tplStartY + tplH + transY)));

            return true;
        }

        public override WorkBase Clone()
        {
            Position_Chip clone = new Position_Chip();
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
