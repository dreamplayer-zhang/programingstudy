﻿
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
using RootTools;
using System.Windows;

namespace RootTools_Vision
{
    public class Position : WorkBase
    {
        #region [Member Variables]
        public override WORK_TYPE Type => WORK_TYPE.PREPARISON;

        WorkplaceBundle workplaceBundle;
        Workplace workplace;

        Recipe recipe;

        PositionRecipe positionRecipe;
        PositionParameter parameter;

        OriginRecipe recipeOrigin;

        #endregion

        public override bool DoPrework()
        {
            if(this.workplace.Index != 0)
            {
                if (this.workplaceBundle[0].GetSubState(WORKPLACE_SUB_STATE.WAFER_POSITION_SUCCESS) == false)
                {
                    this.IsPreworkDone = false;
                    return false;
                }
            }
            
            return base.DoPrework();
        }
        public override void DoWork()
        {
            DoPosition();

            base.DoWork();
        }


        public override void SetRecipe(Recipe _recipe)
        {
            //if (recipe == null)
            //    return;

            m_sName = this.GetType().Name;

            this.recipe = _recipe;
            this.positionRecipe = _recipe.GetRecipe<PositionRecipe>();
            this.parameter = _recipe.GetRecipe<PositionParameter>();

            this.recipeOrigin = _recipe.GetRecipe<OriginRecipe>();

        }

        public override void SetWorkplace(Workplace _workplace)
        {
            this.workplace = _workplace;
        }

        public CPoint ConvertRelToAbs_Wafer(CPoint ptRel)
        {
            return new CPoint(this.recipeOrigin.OriginX + ptRel.X, this.recipeOrigin.OriginY + ptRel.Y);
        }

        public CPoint ConvertRelToAbs_Chip(CPoint ptRel)
        {
            return new CPoint(this.workplace.PositionX + ptRel.X, this.workplace.PositionY + ptRel.Y);
        }

        public bool DoPosition()
        {
            //if (recipe == null)
            //    return true;

            if (this.workplace.MapPositionX == -1 && this.workplace.MapPositionX == -1) // Master
            {
                bool rst = DoPosition_Wafer();
                this.workplace.SetSubState(WORKPLACE_SUB_STATE.WAFER_POSITION_SUCCESS, rst);

                return rst;
            }
            else  // Position Chip
            {
                return DoPosition_Chip();
            }
        }

        public bool DoPosition_Wafer()
        {
            //if (this.positionRecipe.IndexMaxScoreMasterFeature == -1) // Feature가 셋팅이 안되어 있는 경우 전체 Feature 사용
            {
                int outX = 0, outY = 0;
                int maxX = 0, maxY = 0;
                int maxStartX = 0, maxStartY = 0;
                int maxEndX = 0, maxEndY = 0;
                float score = 0;
                float maxScore = 0;
                int i = 0;
                int maxIndex = -1;

                foreach(RecipeType_ImageData feature in this.positionRecipe.ListMasterFeature)
                {
                    CPoint absPos = ConvertRelToAbs_Wafer(new CPoint(feature.PositionX, feature.PositionY));
                    int startX = (absPos.X - this.parameter.SearchRangeX) < 0 ? 0 : (absPos.X - this.parameter.SearchRangeX);
                    int startY = (absPos.Y - this.parameter.SearchRangeY) < 0 ? 0 : (absPos.Y - this.parameter.SearchRangeY);
                    int endX = (absPos.X + feature.Width + this.parameter.SearchRangeX) >= this.workplace.SharedBufferWidth ? this.workplace.SharedBufferWidth : (absPos.X + feature.Width + this.parameter.SearchRangeX);
                    int endY = (absPos.Y + feature.Height + this.parameter.SearchRangeY) >= this.workplace.SharedBufferHeight ? this.workplace.SharedBufferHeight : (absPos.Y + feature.Height + this.parameter.SearchRangeY);

                    unsafe
                    {
                        score =  CLR_IP.Cpp_TemplateMatching(
                            (byte*)this.workplace.SharedBuffer.ToPointer(), feature.RawData, &outX, &outY, 
                            this.workplace.SharedBufferWidth, this.workplace.SharedBufferHeight,
                            feature.Width, feature.Height,
                            startX, startY, endX, endY, 5, this.workplace.SharedBufferByteCnt);
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



                CPoint ptAbs = ConvertRelToAbs_Chip(new CPoint(this.positionRecipe.ListMasterFeature[maxIndex].PositionX, this.positionRecipe.ListMasterFeature[maxIndex].PositionY));
                int tplStartX = ptAbs.X;
                int tplStartY = ptAbs.Y;
                int tplW = this.positionRecipe.ListMasterFeature[maxIndex].Width;
                int tplH = this.positionRecipe.ListMasterFeature[maxIndex].Height;

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


                if (maxScore >= this.parameter.MinScoreLimit) // Position 성공 시
                {
                    this.workplace.SetImagePositionByTrans(transX, transY, true);

                    WorkEventManager.OnPositionDone(this.workplace, new PositionDoneEventArgs(new CPoint(maxStartX, maxStartY), new CPoint(maxEndX, maxEndY),
                            new CPoint(maxStartX + transX, maxStartY + transY), new CPoint(maxEndX + transX, maxEndY + transY), true));
                }
                else  // Position Fail
                {
                    WorkEventManager.OnPositionDone(this.workplace, new PositionDoneEventArgs(new CPoint(maxStartX, maxStartY), new CPoint(maxEndX, maxEndY),
                            new CPoint(maxStartX + transX, maxStartY + transY), new CPoint(maxEndX + transX, maxEndY + transY), false));
                }     
            }

            return true;
        }

        public bool DoPosition_Chip()
        {
            int outX = 0, outY = 0;
            int maxX = 0, maxY = 0;
            int maxStartX = 0, maxStartY = 0;
            int maxEndX = 0, maxEndY = 0;
            float score = 0;
            float maxScore = 0;
            int i = 0;
            int maxIndex = -1;

            try
            {
                if (this.positionRecipe.IndexMaxScoreChipFeature == -1) // Feature가 셋팅이 안되어 있는 경우 전체 Feature 사용
                {

                    foreach (RecipeType_ImageData feature in this.positionRecipe.ListDieFeature)
                    {
                        CPoint absPos = ConvertRelToAbs_Chip(new CPoint(feature.PositionX, feature.PositionY));

                        int startX = (absPos.X - this.parameter.SearchRangeX) < 0 ? 0 : (absPos.X - this.parameter.SearchRangeX);
                        int startY = (absPos.Y - this.parameter.SearchRangeY) < 0 ? 0 : (absPos.Y - this.parameter.SearchRangeY);
                        int endX = (absPos.X + feature.Width + this.parameter.SearchRangeX) >= this.workplace.SharedBufferWidth ? this.workplace.SharedBufferWidth : (absPos.X + feature.Width + this.parameter.SearchRangeX);
                        int endY = (absPos.Y + feature.Height + this.parameter.SearchRangeY) >= this.workplace.SharedBufferHeight ? this.workplace.SharedBufferHeight : (absPos.Y + feature.Height + this.parameter.SearchRangeY);

                        unsafe
                        {
                            score = CLR_IP.Cpp_TemplateMatching(
                                (byte*)this.workplace.SharedBuffer.ToPointer(), feature.RawData, &outX, &outY,
                                this.workplace.SharedBufferWidth, this.workplace.SharedBufferHeight,
                                feature.Width, feature.Height,
                                startX, startY, endX, endY, 5, this.workplace.SharedBufferByteCnt);
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
                    maxIndex = this.positionRecipe.IndexMaxScoreChipFeature;

                    RecipeType_ImageData feature = this.positionRecipe.ListDieFeature[maxIndex];

                    CPoint absPos = ConvertRelToAbs_Chip(new CPoint(feature.PositionX, feature.PositionY));

                    int startX = (absPos.X - this.parameter.SearchRangeX) < 0 ? 0 : (absPos.X - this.parameter.SearchRangeX);
                    int startY = (absPos.Y - this.parameter.SearchRangeY) < 0 ? 0 : (absPos.Y - this.parameter.SearchRangeY);
                    int endX = (absPos.X + feature.Width + this.parameter.SearchRangeX) >= this.workplace.SharedBufferWidth ? this.workplace.SharedBufferWidth : (absPos.X + feature.Width + this.parameter.SearchRangeX);
                    int endY = (absPos.Y + feature.Height + this.parameter.SearchRangeY) >= this.workplace.SharedBufferHeight ? this.workplace.SharedBufferHeight : (absPos.Y + feature.Height + this.parameter.SearchRangeY);

                    unsafe
                    {
                        score = CLR_IP.Cpp_TemplateMatching(
                            (byte*)this.workplace.SharedBuffer.ToPointer(), feature.RawData, &outX, &outY,
                            this.workplace.SharedBufferWidth, this.workplace.SharedBufferHeight,
                            feature.Width, feature.Height,
                            startX, startY, endX, endY, 5, this.workplace.SharedBufferByteCnt);
                    }

                    maxScore = score;
                    maxX = outX;
                    maxY = outY;
                    maxStartX = startX;
                    maxStartY = startY;
                    maxEndX = endX;
                    maxEndY = endY;

                }

                CPoint ptAbs = ConvertRelToAbs_Chip(new CPoint(this.positionRecipe.ListDieFeature[maxIndex].PositionX, this.positionRecipe.ListDieFeature[maxIndex].PositionY));
                int tplStartX = ptAbs.X;
                int tplStartY = ptAbs.Y;
                int tplW = this.positionRecipe.ListDieFeature[maxIndex].Width;
                int tplH = this.positionRecipe.ListDieFeature[maxIndex].Height;

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


                if (maxScore >= this.parameter.MinScoreLimit) // Position 성공 시
                {
                    this.workplace.SetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS, true);
                    this.workplace.SetSubState(WORKPLACE_SUB_STATE.BAD_CHIP, false);

                    this.workplace.MoveImagePosition(transX, transY);

                    ExtractCurrentWorkplace();

                    WorkEventManager.OnPositionDone(this.workplace, new PositionDoneEventArgs(new CPoint(tplStartX, tplStartY), new CPoint(tplStartX + tplW, tplStartY + tplH),
                            new CPoint(tplStartX + transX, tplStartY + transY), new CPoint(tplStartX + tplW + transX, tplStartY + tplH + transY), true));
                }
                else  // Position 실패
                {
                    this.workplace.SetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS, false);
                    this.workplace.SetSubState(WORKPLACE_SUB_STATE.BAD_CHIP, true);

                    WorkEventManager.OnPositionDone(this.workplace, new PositionDoneEventArgs(new CPoint(tplStartX, tplStartY), new CPoint(tplStartX + tplW, tplStartY + tplH),
                            new CPoint(tplStartX + transX, tplStartY + transY), new CPoint(tplStartX + tplW + transX, tplStartY + tplH + transY), false));
                }


            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return true;
        }

        public override WorkBase Clone()
        {

            Position clone = new Position();
            try
            {
                clone.SetRecipe(this.recipe);
                clone.SetWorkplace(this.workplace);
                clone.SetWorkplaceBundle(this.workplaceBundle);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            

            return clone;
        }

        public override void SetWorkplaceBundle(WorkplaceBundle workplace)
        {
            this.workplaceBundle = workplace;
        }

        private void ExtractCurrentWorkplace()
        {
            // Copy 
            int chipH = this.workplace.BufferSizeY;
            int chipW = this.workplace.BufferSizeX;

            int Left = this.workplace.PositionX;
            int Top = this.workplace.PositionY - chipH;
            int Right = this.workplace.PositionX + chipW;
            int Bottom = this.workplace.PositionY;

            int memH = this.workplace.SharedBufferHeight;
            int memW = this.workplace.SharedBufferWidth;

            for (int cnt = Top; cnt < Bottom; cnt++)
                Marshal.Copy(new IntPtr(this.workplace.SharedBuffer.ToInt64() + (cnt * (Int64)memW + Left))
                    , this.workplace.WorkplaceBuffer, chipW * (cnt - Top), chipW);
        }
    }
}
