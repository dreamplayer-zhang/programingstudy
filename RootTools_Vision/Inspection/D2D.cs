using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using RootTools;
using RootTools.Database;
using RootTools_CLR;

namespace RootTools_Vision
{
    public class D2D : WorkBase
    {
        #region [Member variables]
        WorkplaceBundle workplaceBundle;
        Workplace workplace;

        byte[] GoldenImage = null;
        float[] scaleMap = null;
        float[] histWeightMap = null;
        List<byte[]> GoldenImages = new List<byte[]>();

        public override WORK_TYPE Type => WORK_TYPE.MAINWORK;

        // D2D Recipe & Parameter
        private D2DParameter parameter;
        private D2DRecipe recipe;
        #endregion

        public D2D() : base()
        {
            m_sName = this.GetType().Name;
        }

        public override void SetRecipe(Recipe _recipe)
        {
            this.parameter = _recipe.GetRecipe<D2DParameter>();
            this.recipe = _recipe.GetRecipe<D2DRecipe>();
        }

        public override bool DoPrework()
        {
            if(this.workplace.Index == 0)
            {
                return base.DoPrework();
            }

            if (this.workplace.GetSubState(WORKPLACE_SUB_STATE.LINE_FIRST_CHIP) == true &&
                this.workplaceBundle.CheckStateLine(this.workplace.MapPositionX, WORKPLACE_STATE.READY) &&
                this.IsPreworkDone == false)
            {
                CreateGoldenImage();

                // Golden Image Workplace에 복사
                foreach(Workplace wp in this.workplaceBundle)
                {
                    if(wp.MapPositionX == this.workplace.MapPositionX)
                    {
                        wp.SetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE, (object)(GoldenImage) /*Tools.ByteArrayToObject(GoldenImage)*/);
                    }
                }
            }

            if(this.workplace.GetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE) != null)
            {
                this.GoldenImage = this.workplace.GetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE) as byte[];
                return base.DoPrework();
            }
            else
            {
                this.IsPreworkDone = false;
                return false;
            }
        }

        public override void DoWork()
        {
            DoInspection();

            base.DoWork();
        }

        public void SetGoldenImage()
        {
            List<byte[]> wpDatas = new List<byte[]>();
            // Index 계산
            List<int> mapYIdx = new List<int>();
            foreach (Workplace wp in this.workplaceBundle)
                if (wp.MapPositionX == this.workplace.MapPositionX)
                    mapYIdx.Add(wp.MapPositionY);

            mapYIdx.Sort();

            int startY;
            int endY;

            if (mapYIdx.Count() < 5)
            {
                startY = mapYIdx[0];
                endY = mapYIdx[mapYIdx.Count() - 1];
            }
            else
            {
                int currentY = mapYIdx.IndexOf(this.workplace.MapPositionY);

                if (mapYIdx[currentY] - 2 < mapYIdx[0])
                {
                    startY = mapYIdx[0];
                    endY = mapYIdx[currentY] + 2 + (2 - (mapYIdx[currentY] - startY));
                }
                else if (mapYIdx[currentY] + 2 > mapYIdx[mapYIdx.Count() - 1])
                {
                    endY = mapYIdx[mapYIdx.Count() - 1];
                    startY = mapYIdx[currentY] - 2 - (2 - (endY - mapYIdx[currentY]));
                }
                else
                {
                    startY = mapYIdx[currentY] - 2;
                    endY = mapYIdx[currentY] + 2;
                }
            }

            foreach (Workplace wp in this.workplaceBundle)
                if (wp.MapPositionX == this.workplace.MapPositionX)
                    if (this.workplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == true)
                        if ((wp.MapPositionY >= startY) && (wp.MapPositionY <= endY) && wp.MapPositionY != this.workplace.MapPositionY)
                            wpDatas.Add(wp.WorkplaceBuffer);

            CLR_IP.Cpp_CreateGoldenImage_Avg(wpDatas.ToArray(), GoldenImage, wpDatas.Count, this.workplace.BufferSizeX, this.workplace.BufferSizeY);
            //CLR_IP.Cpp_CreateGoldenImage_NearAvg(wpDatas.ToArray(), GoldenImage, wpDatas.Count, this.workplace.BufferSizeX, this.workplace.BufferSizeY);
        }
        public void SetMultipleGoldenImages()
        {
            // Index 계산
            List<int> mapYIdx = new List<int>();
            foreach (Workplace wp in this.workplaceBundle)
                if (wp.MapPositionX == this.workplace.MapPositionX)
                    mapYIdx.Add(wp.MapPositionY);

            mapYIdx.Sort();

            int startY;
            int middleY;
            int endY;

            if (mapYIdx.Count() < 3)
            {
                startY = mapYIdx[0];
                middleY = mapYIdx[1];
                endY = mapYIdx[2];
            }
            else
            {
                int currentY = mapYIdx.IndexOf(this.workplace.MapPositionY);

                startY = mapYIdx[(currentY == 0) ? 1 : 0];
                middleY = mapYIdx[(currentY == mapYIdx.Count() / 2) ? mapYIdx.Count() / 2 + 1 : mapYIdx.Count() / 2];
                endY = mapYIdx[(currentY == mapYIdx.Count() - 1) ? mapYIdx.Count() - 2 : mapYIdx.Count() - 1];
            }

            foreach (Workplace wp in this.workplaceBundle)
                if (wp.MapPositionX == this.workplace.MapPositionX)
                    if (this.workplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == true)
                        if ((wp.MapPositionY == startY) || (wp.MapPositionY == middleY) || (wp.MapPositionY == endY))
                            GoldenImages.Add(wp.WorkplaceBuffer);
        }
        public override void SetWorkplace(Workplace _workplace)
        {
            this.workplace = _workplace;
        }
        
        public void DoInspection()
        {
            if (this.workplace.Index == 0)
                return;

            if (this.workplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == false)
            {
                return;
            }

            int memH = this.workplace.SharedBufferHeight;
            int memW = this.workplace.SharedBufferWidth;

            // Recipe
            int chipH = this.workplace.BufferSizeY; // 현재는 ROI = Chip이기 때문에 사용. 추후 실제 Chip H, W를 Recipe에서 가지고 오자
            int chipW = this.workplace.BufferSizeX;

            int grayLevel = 60; // Option
            int defectSz = 1; // Option

            byte[] binImg = new byte[chipW * chipH];
            byte[] diffImg = new byte[chipW * chipH];

            if (true) // Chip마다 Golden Image 생성 옵션
            {
                SetGoldenImage();
                // Diff Image 계산
                CLR_IP.Cpp_SubtractAbs(GoldenImage, workplace.WorkplaceBuffer, diffImg, chipW, chipH);

                if (true) // ScaleMap Option
                {
                    if (this.workplace.GetPreworkData(PREWORKDATA_KEY.D2D_SCALE_MAP) != null)
                    {
                        this.scaleMap = this.workplace.GetPreworkData(PREWORKDATA_KEY.D2D_SCALE_MAP) as float[];
                    }

                    else
                    {
                        if (scaleMap == null)
                            scaleMap = new float[chipW * chipH];

                        CLR_IP.Cpp_CreateDiffScaleMap(GoldenImage, scaleMap, chipW, chipH, 10, 10);

                        foreach (Workplace wp in this.workplaceBundle)
                        {
                            //if (wp.MapPositionX == this.workplace.MapPositionX)
                            //{
                            wp.SetPreworkData(PREWORKDATA_KEY.D2D_SCALE_MAP, (object)(scaleMap));
                            //}
                        }
                    }

                    CLR_IP.Cpp_Multiply(diffImg, scaleMap, diffImg, chipW, chipH);
                }

                if (false) // Histogram WeightMap
                {
                    histWeightMap = new float[chipW * chipH];
                    CLR_IP.Cpp_CreateHistogramWeightMap(workplace.WorkplaceBuffer, GoldenImage, histWeightMap, chipW, chipH, 5);        // -> 뭔가 결과가 이상함... 수정 필요
                    CLR_IP.Cpp_Multiply(diffImg, histWeightMap, diffImg, chipW, chipH);
                }
            }
            else if (false) // Choi D2D Algorithm 
            {
                SetMultipleGoldenImages();
                // Diff Image 계산
                CLR_IP.Cpp_SelectMinDiffinArea(workplace.WorkplaceBuffer, GoldenImages.ToArray(), diffImg, GoldenImages.Count(), chipW, chipH, 1);

            }

            // Filter
            CLR_IP.Cpp_Morphology(binImg, binImg, chipW, chipH, 3, "OPEN", 1);
            //CLR_IP.Cpp_AverageBlur(diffImg, diffImg, chipW, chipH);

            // Threshold 값으로 Defect 탐색
            CLR_IP.Cpp_Threshold(diffImg, binImg, chipW, chipH, grayLevel);
            // Labeling
            var Label = CLR_IP.Cpp_Labeling(workplace.WorkplaceBuffer, binImg, chipW, chipH);

            string sInspectionID = DatabaseManager.Instance.GetInspectionID();

            //Add Defect
            for (int i = 0; i < Label.Length; i++)
            {
                if (Label[i].area > defectSz)
                {
                    this.workplace.AddDefect(sInspectionID,
                        10010,
                        Label[i].area,
                        Label[i].value,
                        this.workplace.PositionX + Label[i].boundLeft,
                        this.workplace.PositionY - (chipH - Label[i].boundTop),
                        Label[i].width,
                        Label[i].height,
                        this.workplace.MapPositionX,
                        this.workplace.MapPositionY
                        );
                }

            }

            GoldenImages.Clear();
            WorkEventManager.OnInspectionDone(this.workplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
        }

        public void CreateGoldenImage()
        {
            if(GoldenImage == null)
                GoldenImage = new byte[this.workplace.BufferSizeX * this.workplace.BufferSizeY];

            if (false) // Line 별로 GoldenImage를 만들 경우
            {
                List<byte[]> wpDatas = new List<byte[]>();
                // Index 계산
                List<int> mapYIdx = new List<int>();
                foreach (Workplace wp in this.workplaceBundle)
                    if (wp.MapPositionX == this.workplace.MapPositionX)
                        mapYIdx.Add(wp.MapPositionY);

                mapYIdx.Sort();

                int startY = 0;
                int endY = 0; 

                if (mapYIdx.Count() < 5)
                {
                    startY = mapYIdx[0];
                    endY = mapYIdx[mapYIdx.Count() - 1];
                }
                else // 중심에 있는 4개의 칩으로만 Golden Image 생성
                {
                    
                    startY = mapYIdx[mapYIdx.Count() / 2 - 1 - 2];
                    endY = mapYIdx[mapYIdx.Count() / 2 - 1 + 2];
                }
                foreach (Workplace wp in this.workplaceBundle)
                    if (wp.MapPositionX == this.workplace.MapPositionX)
                        if (this.workplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == true)
                            if ((wp.MapPositionY >= startY) && (wp.MapPositionY <= endY) && wp.MapPositionY != this.workplace.MapPositionY)
                                wpDatas.Add(wp.WorkplaceBuffer);


                CLR_IP.Cpp_CreateGoldenImage_Avg(wpDatas.ToArray(), GoldenImage, wpDatas.Count, this.workplace.BufferSizeX, this.workplace.BufferSizeY);
                //CLR_IP.Cpp_CreateGoldenImage_NearAvg(wpDatas.ToArray(), GoldenImage, wpDatas.Count, this.workplace.BufferSizeX, this.workplace.BufferSizeY);
            }
        }

        public override WorkBase Clone()
        {
            return (WorkBase)this.MemberwiseClone();
        }

        public override void SetWorkplaceBundle(WorkplaceBundle _workplaceBundle)
        {

            this.workplaceBundle = _workplaceBundle;
            return;
        }
    }
}
