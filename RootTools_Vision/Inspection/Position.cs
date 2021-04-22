
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
using RootTools.Database;

namespace RootTools_Vision
{
    public class Position : WorkBase
    {
        #region [Member Variables]
        public override WORK_TYPE Type => WORK_TYPE.ALIGNMENT;

        PositionRecipe positionRecipe;
        PositionParameter parameterPosition;

        OriginRecipe recipeOrigin;

        IntPtr InspectionSharedBuffer;

        #endregion

        protected override bool Preparation()
        {
            if (this.currentWorkplace == null) return false;

            if (this.positionRecipe == null || this.parameterPosition == null)
            {
                this.positionRecipe = this.recipe.GetItem<PositionRecipe>();
                this.parameterPosition = this.parameter as PositionParameter;
            }

            this.recipeOrigin = this.recipe.GetItem<OriginRecipe>();

            // WorkplaceBundle 0번에서 MapPositionX/Y가 -1이 아니면 Master Position을 안하는 것으로 간주한다.
            if (this.workplaceBundle[0].Index == 0 &&
                (this.workplaceBundle[0].MapIndexX != -1 || this.workplaceBundle[0].MapIndexY != -1))
                return true;

            if (this.currentWorkplace.Index != 0)
            {
                if (this.workplaceBundle[0].GetSubState(WORKPLACE_SUB_STATE.WAFER_POSITION_SUCCESS) == false)
                {
                    this.IsPreworkDone = false;
                    return false;
                }
            }
            
            return true;
        }
        protected override bool Execution()
        {
            if (this.currentWorkplace == null) return false;
            DoPosition();

            return true;
        }

        public CPoint ConvertRelToAbs_Wafer(CPoint ptRel)
        {
            return new CPoint(this.recipeOrigin.OriginX + ptRel.X, this.recipeOrigin.OriginY + ptRel.Y);
        }

        public CPoint ConvertRelToAbs_Chip(CPoint ptRel)
        {
            return new CPoint(this.currentWorkplace.PositionX + ptRel.X, this.currentWorkplace.PositionY + this.recipeOrigin.OriginHeight + ptRel.Y);
        }

        public bool DoPosition()
        {
            //if (recipe == null)
            //    return true;

            if (this.currentWorkplace.MapIndexX == -1 && this.currentWorkplace.MapIndexY == -1) // Master
            {
                bool rst = DoPosition_Wafer();
                this.currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.WAFER_POSITION_SUCCESS, rst);

                return rst;
            }
            else  // Position Chip
            {

                bool rst = false;
                switch (this.parameterPosition.Method)
                {
                    case POSITION_METHOD.Dependent:
                        rst = DoPosition_Chip_Dependent();
                        break;
                    case POSITION_METHOD.Independent:
                        rst = DoPosition_Chip_Independent();
                        break;
                    default:
                        rst = DoPosition_Chip_Dependent();
                        break;

                }
               
                return rst;
            }
        }

        public bool DoPosition_Wafer()
        {
            this.InspectionSharedBuffer = this.currentWorkplace.GetSharedBufferInfo(this.parameterPosition.IndexChannel);

            if (this.positionRecipe.ListMasterFeature.Count == 0) 
                return false;

            //if (this.positionRecipe.IndexMaxScoreMasterFeature == -1) // Feature가 셋팅이 안되어 있는 경우 전체 Feature 사용
            {
                int outX = 0, outY = 0;
                int maxX = 0, maxY = 0;
                int maxStartX = 0, maxStartY = 0;
                int maxEndX = 0, maxEndY = 0;
                float score = 0;
                float maxScore = 0;
                int i = 0;
                int maxIndex = 0;

                foreach(RecipeType_ImageData feature in this.positionRecipe.ListMasterFeature)
                {
                    CPoint absPos = ConvertRelToAbs_Wafer(new CPoint(feature.PositionX, feature.PositionY));
                    int startX = (absPos.X - this.parameterPosition.WaferSearchRangeX) < 0 ? 0 : (absPos.X - this.parameterPosition.WaferSearchRangeX);
                    int startY = (absPos.Y - this.parameterPosition.WaferSearchRangeY) < 0 ? 0 : (absPos.Y - this.parameterPosition.WaferSearchRangeY);
                    int endX = (absPos.X + feature.Width + this.parameterPosition.WaferSearchRangeX) >= this.currentWorkplace.SharedBufferInfo.Width ? this.currentWorkplace.SharedBufferInfo.Width : (absPos.X + feature.Width + this.parameterPosition.WaferSearchRangeX);
                    int endY = (absPos.Y + feature.Height + this.parameterPosition.WaferSearchRangeY) >= this.currentWorkplace.SharedBufferInfo.Height ? this.currentWorkplace.SharedBufferInfo.Height : (absPos.Y + feature.Height + this.parameterPosition.WaferSearchRangeY);

                    unsafe
                    {
                        score =  CLR_IP.Cpp_TemplateMatching(
                            (byte*)this.InspectionSharedBuffer.ToPointer(), feature.GetColorRowData(parameterPosition.IndexChannel), &outX, &outY, 
                            this.currentWorkplace.SharedBufferInfo.Width, this.currentWorkplace.SharedBufferInfo.Height,
                            feature.Width, feature.Height,
                            startX, startY, endX, endY, 5, 1, (int)parameterPosition.IndexChannel);
                    }

                    if( score >= maxScore)
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



                CPoint ptAbs = ConvertRelToAbs_Wafer(new CPoint(this.positionRecipe.ListMasterFeature[maxIndex].PositionX, this.positionRecipe.ListMasterFeature[maxIndex].PositionY));
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
                float centerMatchingX = (int)maxX + tplW / 2;
                float centerMatchingY = (int)maxY + tplH / 2;

                //int transX = (int)(centerROIX - centerMatchingX);
                //int transY = (int)(centerROIY - centerMatchingY);

                int transX = (int)(centerMatchingX - tplCenterX);
                int transY = (int)(centerMatchingY - tplCenterY);


                if (maxScore >= this.parameterPosition.WaferMinScoreLimit) // Position 성공 시
                {
                    foreach(Workplace wp in this.workplaceBundle)
                    {
                        wp.SetOffset(transX, transY);
                    }

                    WorkEventManager.OnPositionDone(this.currentWorkplace, new PositionDoneEventArgs(new CPoint(tplStartX, tplStartY), new CPoint(tplStartX + tplW, tplStartY + tplH),
                            new CPoint(tplStartX + transX, tplStartY + transY), new CPoint(tplStartX + tplW + transX, tplStartY + tplH + transY), true));

                    //WorkEventManager.OnPositionDone(this.currentWorkplace, new PositionDoneEventArgs(new CPoint(maxStartX, maxStartY), new CPoint(maxEndX, maxEndY),
                    //        new CPoint(maxStartX + transX, maxStartY + transY), new CPoint(maxEndX + transX, maxEndY + transY), true));
                }
                else  // Position Fail
                {
                    string sInspectionID = DatabaseManager.Instance.GetInspectionID();

                    this.currentWorkplace.AddDefect(sInspectionID,
                       90000,
                       0,
                       0,
                       maxStartX,
                       maxStartY,
                       maxEndX,
                       maxEndY,
                       maxEndX - maxStartX,
                       maxEndY - maxStartY,
                       //this.currentWorkplace.PositionX,
                       //this.currentWorkplace.PositionY,
                       //this.currentWorkplace.Width,
                       //this.currentWorkplace.Height,
                       this.currentWorkplace.MapIndexX,
                       this.currentWorkplace.MapIndexY
                       );

                    WorkEventManager.OnPositionDone(this.currentWorkplace, new PositionDoneEventArgs(new CPoint(maxStartX, maxStartY), new CPoint(maxEndX, maxEndY),
                            new CPoint(maxStartX + transX, maxStartY + transY), new CPoint(maxEndX + transX, maxEndY + transY), false));
                }     
            }

            return true;
        }

        public bool DoPosition_Chip_Independent()
        {
            this.InspectionSharedBuffer = this.currentWorkplace.GetSharedBufferInfo(this.parameterPosition.IndexChannel);

            int outX = 0, outY = 0;
            int maxX = 0, maxY = 0;
            int maxStartX = 0, maxStartY = 0;
            int maxEndX = 0, maxEndY = 0;
            float score = 0;
            float maxScore = 0;
            int i = 0;
            int maxIndex = 0;

            try
            {
                if (this.positionRecipe.IndexMaxScoreChipFeature == -1) // Feature가 셋팅이 안되어 있는 경우 전체 Feature 사용
                {

                    foreach (RecipeType_ImageData feature in this.positionRecipe.ListDieFeature)
                    {
                        CPoint absPos = ConvertRelToAbs_Chip(new CPoint(feature.PositionX, feature.PositionY));

                        int startX = (absPos.X - this.parameterPosition.ChipSearchRangeX) < 0 ? 0 : (absPos.X - this.parameterPosition.ChipSearchRangeX);
                        int startY = (absPos.Y - this.parameterPosition.ChipSearchRangeY) < 0 ? 0 : (absPos.Y - this.parameterPosition.ChipSearchRangeY);
                        int endX = (absPos.X + feature.Width + this.parameterPosition.ChipSearchRangeX) >= this.currentWorkplace.SharedBufferInfo.Width ? this.currentWorkplace.SharedBufferInfo.Width : (absPos.X + feature.Width + this.parameterPosition.ChipSearchRangeX);
                        int endY = (absPos.Y + feature.Height + this.parameterPosition.ChipSearchRangeY) >= this.currentWorkplace.SharedBufferInfo.Height ? this.currentWorkplace.SharedBufferInfo.Height : (absPos.Y + feature.Height + this.parameterPosition.ChipSearchRangeY);

                        unsafe
                        {
                            score = CLR_IP.Cpp_TemplateMatching(
                                (byte*)this.InspectionSharedBuffer.ToPointer(), feature.GetColorRowData(parameterPosition.IndexChannel), &outX, &outY,
                                this.currentWorkplace.SharedBufferInfo.Width, this.currentWorkplace.SharedBufferInfo.Height,
                                feature.Width, feature.Height,
                                startX, startY, endX, endY, 5, 1,(int)parameterPosition.IndexChannel);
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

                    int startX = (absPos.X - this.parameterPosition.ChipSearchRangeX) < 0 ? 0 : (absPos.X - this.parameterPosition.ChipSearchRangeX);
                    int startY = (absPos.Y - this.parameterPosition.ChipSearchRangeY) < 0 ? 0 : (absPos.Y - this.parameterPosition.ChipSearchRangeY);
                    int endX = (absPos.X + feature.Width + this.parameterPosition.ChipSearchRangeX) >= this.currentWorkplace.SharedBufferInfo.Width ? this.currentWorkplace.SharedBufferInfo.Width : (absPos.X + feature.Width + this.parameterPosition.ChipSearchRangeX);
                    int endY = (absPos.Y + feature.Height + this.parameterPosition.ChipSearchRangeY) >= this.currentWorkplace.SharedBufferInfo.Height ? this.currentWorkplace.SharedBufferInfo.Height : (absPos.Y + feature.Height + this.parameterPosition.ChipSearchRangeY);

                    unsafe
                    {
                        score = CLR_IP.Cpp_TemplateMatching(
                            (byte*)this.InspectionSharedBuffer.ToPointer(), feature.GetColorRowData(parameterPosition.IndexChannel), &outX, &outY,
                            this.currentWorkplace.SharedBufferInfo.Width, this.currentWorkplace.SharedBufferInfo.Height,
                            feature.Width, feature.Height,
                            startX, startY, endX, endY, 5, this.currentWorkplace.SharedBufferInfo.ByteCnt, (int)parameterPosition.IndexChannel);
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
                float centerMatchingX = (int)maxX + tplW / 2;
                float centerMatchingY = (int)maxY + tplH / 2;

                //int transX = (int)(centerROIX - centerMatchingX);
                //int transY = (int)(centerROIY - centerMatchingY);

                int transX = (int)(centerMatchingX - tplCenterX);
                int transY = (int)(centerMatchingY - tplCenterY);


                if (maxScore >= this.parameterPosition.ChipMinScoreLimit) // Position 성공 시
                {
                    this.currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS, true);
                    this.currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.BAD_CHIP, false);

                    this.currentWorkplace.SetTrans(transX, transY);

                    WorkEventManager.OnPositionDone(this.currentWorkplace, new PositionDoneEventArgs(new CPoint(tplStartX, tplStartY), new CPoint(tplStartX + tplW, tplStartY + tplH),
                            new CPoint(tplStartX + transX, tplStartY + transY), new CPoint(tplStartX + tplW + transX, tplStartY + tplH + transY), true));
                }
                else  // Position 실패
                {
                    this.currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS, false);
                    this.currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.BAD_CHIP, true);

                    string sInspectionID = DatabaseManager.Instance.GetInspectionID();

                    this.currentWorkplace.AddDefect(sInspectionID,
                       90001,
                       0,
                       0,
                       this.currentWorkplace.PositionX,
                       this.currentWorkplace.PositionY,
                       this.currentWorkplace.Width,
                       this.currentWorkplace.Height,
                       this.currentWorkplace.MapIndexX,
                       this.currentWorkplace.MapIndexY
                       );

                    WorkEventManager.OnPositionDone(this.currentWorkplace, new PositionDoneEventArgs(new CPoint(tplStartX, tplStartY), new CPoint(tplStartX + tplW, tplStartY + tplH),
                            new CPoint(tplStartX + transX, tplStartY + transY), new CPoint(tplStartX + tplW + transX, tplStartY + tplH + transY), false));
                }
            }
            catch(Exception ex)
            {
                TempLogger.Write("Position", ex);
                //MessageBox.Show(ex.Message);
            }

            return true;
        }


        public bool DoPosition_Chip_Dependent()
        {
            this.InspectionSharedBuffer = this.currentWorkplace.GetSharedBufferInfo(this.parameterPosition.IndexChannel);

            int outX = 0, outY = 0;
            int maxX = 0, maxY = 0;
            int maxStartX = 0, maxStartY = 0;
            int maxEndX = 0, maxEndY = 0;
            float score = 0;
            float maxScore = 0;
            int i = 0;
            int maxIndex = 0;

            try
            {
                int startChipIndex = (this.workplaceBundle[0].MapIndexX == -1) && (this.workplaceBundle[0].MapIndexY == -1) ? 1 : 0;
                if (this.workplaceBundle.GetWorkplace(this.currentWorkplace.MapIndexX - 1, this.workplaceBundle[startChipIndex].MapIndexY) != null &&
                    this.currentWorkplace.MapIndexY == this.workplaceBundle[startChipIndex].MapIndexY) // 각 라인 마스터 칩
                {

                    Workplace workplaceLeft = this.workplaceBundle.GetWorkplace(this.currentWorkplace.MapIndexX - 1, this.currentWorkplace.MapIndexY);
                    this.currentWorkplace.SetOffset(workplaceLeft.OffsetX, workplaceLeft.OffsetY);
                    this.currentWorkplace.AddOffset(workplaceLeft.TransX, workplaceLeft.TransY);
                }
                else if(this.currentWorkplace.MapIndexY > this.workplaceBundle[startChipIndex].MapIndexY)
                {
                    Workplace workplaceUp = this.workplaceBundle.GetWorkplace(this.currentWorkplace.MapIndexX, this.currentWorkplace.MapIndexY - 1);
                    if(workplaceUp != null)
                    {
                        this.currentWorkplace.SetOffset(workplaceUp.OffsetX, workplaceUp.OffsetY);
                        this.currentWorkplace.AddOffset(workplaceUp.TransX, workplaceUp.TransY);
                    }
                }
                else if (this.currentWorkplace.MapIndexY < this.workplaceBundle[startChipIndex].MapIndexY)
                {
                    Workplace workplaceDown = this.workplaceBundle.GetWorkplace(this.currentWorkplace.MapIndexX, this.currentWorkplace.MapIndexY + 1);
                    if(workplaceDown != null)
                    {
                        this.currentWorkplace.SetOffset(workplaceDown.OffsetX, workplaceDown.OffsetY);
                        this.currentWorkplace.AddOffset(workplaceDown.TransX, workplaceDown.TransY);
                    }
                }

                if (this.positionRecipe.IndexMaxScoreChipFeature == -1) // Feature가 셋팅이 안되어 있는 경우 전체 Feature 사용
                {

                    foreach (RecipeType_ImageData feature in this.positionRecipe.ListDieFeature)
                    {
                        CPoint absPos = ConvertRelToAbs_Chip(new CPoint(feature.PositionX, feature.PositionY));

                        int startX = (absPos.X - this.parameterPosition.ChipSearchRangeX) < 0 ? 0 : (absPos.X - this.parameterPosition.ChipSearchRangeX);
                        int startY = (absPos.Y - this.parameterPosition.ChipSearchRangeY) < 0 ? 0 : (absPos.Y - this.parameterPosition.ChipSearchRangeY);
                        int endX = (absPos.X + feature.Width + this.parameterPosition.ChipSearchRangeX) >= this.currentWorkplace.SharedBufferInfo.Width ? this.currentWorkplace.SharedBufferInfo.Width : (absPos.X + feature.Width + this.parameterPosition.ChipSearchRangeX);
                        int endY = (absPos.Y + feature.Height + this.parameterPosition.ChipSearchRangeY) >= this.currentWorkplace.SharedBufferInfo.Height ? this.currentWorkplace.SharedBufferInfo.Height : (absPos.Y + feature.Height + this.parameterPosition.ChipSearchRangeY);

                        unsafe
                        {
                            score = CLR_IP.Cpp_TemplateMatching(
                                (byte*)this.InspectionSharedBuffer.ToPointer(), feature.GetColorRowData(parameterPosition.IndexChannel), &outX, &outY,
                                this.currentWorkplace.SharedBufferInfo.Width, this.currentWorkplace.SharedBufferInfo.Height,
                                feature.Width, feature.Height,
                                startX, startY, endX, endY, 5, 1, (int)parameterPosition.IndexChannel);
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

                    int startX = (absPos.X - this.parameterPosition.ChipSearchRangeX) < 0 ? 0 : (absPos.X - this.parameterPosition.ChipSearchRangeX);
                    int startY = (absPos.Y - this.parameterPosition.ChipSearchRangeY) < 0 ? 0 : (absPos.Y - this.parameterPosition.ChipSearchRangeY);
                    int endX = (absPos.X + feature.Width + this.parameterPosition.ChipSearchRangeX) >= this.currentWorkplace.SharedBufferInfo.Width ? this.currentWorkplace.SharedBufferInfo.Width : (absPos.X + feature.Width + this.parameterPosition.ChipSearchRangeX);
                    int endY = (absPos.Y + feature.Height + this.parameterPosition.ChipSearchRangeY) >= this.currentWorkplace.SharedBufferInfo.Height ? this.currentWorkplace.SharedBufferInfo.Height : (absPos.Y + feature.Height + this.parameterPosition.ChipSearchRangeY);

                    unsafe
                    {
                        score = CLR_IP.Cpp_TemplateMatching(
                            (byte*)this.InspectionSharedBuffer.ToPointer(), feature.GetColorRowData(parameterPosition.IndexChannel), &outX, &outY,
                            this.currentWorkplace.SharedBufferInfo.Width, this.currentWorkplace.SharedBufferInfo.Height,
                            feature.Width, feature.Height,
                            startX, startY, endX, endY, 5, this.currentWorkplace.SharedBufferInfo.ByteCnt, (int)parameterPosition.IndexChannel);
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

                float tplCenterX = (float)ptAbs.X /*+ tplW / 2*/;
                float tplCenterY = (float)ptAbs.Y /*+ tplH / 2*/;

                maxX += maxStartX; // ROI에서 image 좌표로 변환
                maxY += maxStartY;

                // ROI 중심 위치
                float centerROIX = (maxStartX + maxEndX) / 2;   //이거 필요없는듯
                float centerROIY = (maxStartY + maxEndY) / 2;

                // Matching 중심 위치
                float centerMatchingX = (int)maxX /*+ tplW / 2*/;
                float centerMatchingY = (int)maxY /*+ tplH / 2*/;

                int transX = (int)(centerMatchingX - tplCenterX);
                int transY = (int)(centerMatchingY - tplCenterY);


                if (maxScore >= this.parameterPosition.ChipMinScoreLimit) // Position 성공 시
                {
                    this.currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS, true);
                    this.currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.BAD_CHIP, false);

                    this.currentWorkplace.SetTrans(transX, transY);

                    WorkEventManager.OnPositionDone(this.currentWorkplace, new PositionDoneEventArgs(new CPoint(tplStartX, tplStartY), new CPoint(tplStartX + tplW, tplStartY + tplH),
                            new CPoint(tplStartX + transX, tplStartY + transY), new CPoint(tplStartX + tplW + transX, tplStartY + tplH + transY), true));
                }
                else  // Position 실패
                {
                    this.currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS, false);
                    this.currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.BAD_CHIP, true);

                    string sInspectionID = DatabaseManager.Instance.GetInspectionID();

                    this.currentWorkplace.AddDefect(sInspectionID,
                       90001,
                       0,
                       0,
                       this.currentWorkplace.PositionX,
                       this.currentWorkplace.PositionY,
                       this.currentWorkplace.Width,
                       this.currentWorkplace.Height,
                       this.currentWorkplace.MapIndexX,
                       this.currentWorkplace.MapIndexY
                       );

                    WorkEventManager.OnPositionDone(this.currentWorkplace, new PositionDoneEventArgs(new CPoint(tplStartX, tplStartY), new CPoint(tplStartX + tplW, tplStartY + tplH),
                            new CPoint(tplStartX + transX, tplStartY + transY), new CPoint(tplStartX + tplW + transX, tplStartY + tplH + transY), false));
                }
            }
            catch (Exception ex)
            {
                TempLogger.Write("Position", ex);
                //MessageBox.Show(ex.Message);
            }

            return true;
        }
    }
}
