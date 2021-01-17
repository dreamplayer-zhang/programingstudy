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

        byte[] GoldenImage = null;
        float[] scaleMap = null;
        float[] histWeightMap = null;
        List<byte[]> GoldenImages = new List<byte[]>();

        public override WORK_TYPE Type => WORK_TYPE.INSPECTION;

        // D2D Recipe & Parameter
        private D2DParameter parameter;
        private D2DRecipe recipeD2D;

        private IntPtr inspectionSharedBuffer;
        #endregion

        public D2D() : base()
        {
            m_sName = this.GetType().Name;
        }

        protected override bool Preparation()
        {
            if(this.parameter == null || this.recipeD2D == null)
            {
                this.parameter = this.recipe.GetRecipe<D2DParameter>();
                this.recipeD2D = this.recipe.GetRecipe<D2DRecipe>();
            }
            

            if (this.currentWorkplace.Index == 0)
            {
                return true;
            }

            this.inspectionSharedBuffer = this.currentWorkplace.GetSharedBuffer(this.parameter.IndexChannel);

            if (this.currentWorkplace.GetSubState(WORKPLACE_SUB_STATE.LINE_FIRST_CHIP) == true &&
                this.workplaceBundle.CheckStateLine(this.currentWorkplace.MapIndexX, WORK_TYPE.ALIGNMENT) &&
                this.IsPreworkDone == false)
            {
                CreateGoldenImage();

                // Golden Image Workplace에 복사
                foreach(Workplace wp in this.workplaceBundle)
                {
                    if(wp.MapIndexX == this.currentWorkplace.MapIndexX)
                    {
                        wp.SetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE, (object)(GoldenImage) /*Tools.ByteArrayToObject(GoldenImage)*/);
                    }
                }
            }

            if(this.currentWorkplace.GetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE) != null)
            {
                this.GoldenImage = this.currentWorkplace.GetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE) as byte[];
                return true;
            }
            else
            {
                this.IsPreworkDone = false;
                return false;
            }
        }

        protected override bool Execution()
        {
            DoInspection();

            return true;
        }

        public void SetGoldenImage()
        {
            List<byte[]> wpDatas = new List<byte[]>();
            // Index 계산
            List<int> mapYIdx = new List<int>();
            foreach (Workplace wp in this.workplaceBundle)
                if (wp.MapIndexX == this.currentWorkplace.MapIndexX)
                    mapYIdx.Add(wp.MapIndexY);

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
                int currentY = mapYIdx.IndexOf(this.currentWorkplace.MapIndexY);

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
                if (wp.MapIndexX == this.currentWorkplace.MapIndexX)
                    if (this.currentWorkplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == true)
                        if ((wp.MapIndexY >= startY) && (wp.MapIndexY <= endY) && wp.MapIndexY != this.currentWorkplace.MapIndexY)
                            wpDatas.Add(GetWorkplaceBuffer(this.parameter.IndexChannel));

            int width = currentWorkplace.Width;
            int height = currentWorkplace.Height;

            switch (parameter.CreateRefImage)
            {
                case CreateRefImageMethod.Average:
                    CLR_IP.Cpp_CreateGoldenImage_Avg(wpDatas.ToArray(), GoldenImage, wpDatas.Count, width, height);
                    break;
                case CreateRefImageMethod.NearAverage:
                    CLR_IP.Cpp_CreateGoldenImage_NearAvg(wpDatas.ToArray(), GoldenImage, wpDatas.Count, width, height);               
                    break;
                case CreateRefImageMethod.MedianAverage:
                    CLR_IP.Cpp_CreateGoldenImage_MedianAvg(wpDatas.ToArray(), GoldenImage, wpDatas.Count, width, height);
                    
                    break;
                case CreateRefImageMethod.Median:
                    CLR_IP.Cpp_CreateGoldenImage_Median(wpDatas.ToArray(), GoldenImage, wpDatas.Count, width, height);
                    break;
                default:
                    CLR_IP.Cpp_CreateGoldenImage_Avg(wpDatas.ToArray(), GoldenImage, wpDatas.Count, width, height);
                    break;
            }
        }
        public void SetMultipleGoldenImages()
        {
            // Index 계산
            List<int> mapYIdx = new List<int>();
            foreach (Workplace wp in this.workplaceBundle)
                if (wp.MapIndexX == this.currentWorkplace.MapIndexX)
                    mapYIdx.Add(wp.MapIndexY);

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
                int currentY = mapYIdx.IndexOf(this.currentWorkplace.MapIndexY);

                startY = mapYIdx[(currentY == 0) ? 1 : 0];
                middleY = mapYIdx[(currentY == mapYIdx.Count() / 2) ? mapYIdx.Count() / 2 + 1 : mapYIdx.Count() / 2];
                endY = mapYIdx[(currentY == mapYIdx.Count() - 1) ? mapYIdx.Count() - 2 : mapYIdx.Count() - 1];
            }

            foreach (Workplace wp in this.workplaceBundle)
                if (wp.MapIndexX == this.currentWorkplace.MapIndexX)
                    if (this.currentWorkplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == true)
                        if ((wp.MapIndexY == startY) || (wp.MapIndexY == middleY) || (wp.MapIndexY == endY))
                            GoldenImages.Add(GetWorkplaceBuffer(this.parameter.IndexChannel));
        }
        
        public void DoInspection()
        {
            if (this.currentWorkplace.Index == 0)
                return;

            if (this.currentWorkplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == false || this.currentWorkplace.GetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE) == null)
            {
                return;
            }

            this.inspectionSharedBuffer = this.currentWorkplace.GetSharedBuffer(this.parameter.IndexChannel);
            byte[] workplaceBuffer = GetWorkplaceBuffer(this.parameter.IndexChannel);

            int memH = this.currentWorkplace.SharedBufferHeight;
            int memW = this.currentWorkplace.SharedBufferWidth;

            // Recipe
            int chipH = this.currentWorkplace.Height; // 현재는 ROI = Chip이기 때문에 사용. 추후 실제 Chip H, W를 Recipe에서 가지고 오자
            int chipW = this.currentWorkplace.Width;

            byte[] binImg = new byte[chipW * chipH];
            byte[] diffImg = new byte[chipW * chipH];

            if (parameter.RefImageUpdate == RefImageUpdateFreq.Chip_Trigger) // JHChoi D2D Algorithm 
            {
                SetMultipleGoldenImages();
                // Diff Image 계산
                CLR_IP.Cpp_SelectMinDiffinArea(workplaceBuffer, GoldenImages.ToArray(), diffImg, GoldenImages.Count(), chipW, chipH, 1);
            }
            else
            {
                if (parameter.RefImageUpdate == RefImageUpdateFreq.Chip) // Chip마다 Golden Image 생성 옵션
                    SetGoldenImage();
                // Diff Image 계산
                CLR_IP.Cpp_SubtractAbs(GoldenImage, workplaceBuffer, diffImg, chipW, chipH);

                if (parameter.ScaleMap) // ScaleMap Option
                {
                    if (this.currentWorkplace.GetPreworkData(PREWORKDATA_KEY.D2D_SCALE_MAP) != null)
                    {
                        this.scaleMap = this.currentWorkplace.GetPreworkData(PREWORKDATA_KEY.D2D_SCALE_MAP) as float[];
                    }

                    else
                    {
                        if (scaleMap == null)
                            scaleMap = new float[chipW * chipH];

                        CLR_IP.Cpp_CreateDiffScaleMap(GoldenImage, scaleMap, chipW, chipH, 10, 10);

                        foreach (Workplace wp in this.workplaceBundle)
                            wp.SetPreworkData(PREWORKDATA_KEY.D2D_SCALE_MAP, (object)(scaleMap));
                    }

                    CLR_IP.Cpp_Multiply(diffImg, scaleMap, diffImg, chipW, chipH);
                }

                if (parameter.HistWeightMap) // Histogram WeightMap
                {
                    histWeightMap = new float[chipW * chipH];
                    CLR_IP.Cpp_CreateHistogramWeightMap(workplaceBuffer, GoldenImage, histWeightMap, chipW, chipH, 5);        // -> 뭔가 결과가 이상함... 수정 필요
                    CLR_IP.Cpp_Multiply(diffImg, histWeightMap, diffImg, chipW, chipH);
                }
            }
            

            // Filter
            switch (parameter.DiffFilter)
            {
                case DiffFilterMethod.Average:
                    CLR_IP.Cpp_AverageBlur(diffImg, diffImg, chipW, chipH);
                    break;
                case DiffFilterMethod.Gaussian:
                    CLR_IP.Cpp_GaussianBlur(diffImg, diffImg, chipW, chipH, 2);
                    break;
                case DiffFilterMethod.Median:
                    CLR_IP.Cpp_MedianBlur(diffImg, diffImg, chipW, chipH, 3);
                    break;
                case DiffFilterMethod.Morphology:
                    CLR_IP.Cpp_Morphology(diffImg, diffImg, chipW, chipH, 3, "OPEN", 1);
                    break;
                default:
                    break;
            }

            // Threshold 값으로 Defect 탐색
            CLR_IP.Cpp_Threshold(diffImg, binImg, chipW, chipH, parameter.Intensity);


            // Mask
            MaskRecipe mask = this.recipe.GetRecipe<MaskRecipe>(); //요기다 추가해줘용

            Cpp_Point[] maskStartPoint = new Cpp_Point[mask.MaskList[this.parameter.MaskIndex].PointLines.Count];
            int[] maskLength = new int[mask.MaskList[this.parameter.MaskIndex].PointLines.Count];

            Cpp_Point tempPt = new Cpp_Point();
            for(int i = 0; i < mask.MaskList[this.parameter.MaskIndex].PointLines.Count; i++)
            {
                maskStartPoint[i] = new Cpp_Point();
                maskStartPoint[i].x = mask.MaskList[this.parameter.MaskIndex].PointLines[i].StartPoint.X;
                maskStartPoint[i].y = mask.MaskList[this.parameter.MaskIndex].PointLines[i].StartPoint.Y;
                maskLength[i] = mask.MaskList[this.parameter.MaskIndex].PointLines[i].Length;
            }
            CLR_IP.Cpp_Masking(binImg, binImg, maskStartPoint.ToArray(), maskLength.ToArray(), chipW, chipH);

            // Labeling
            var Label = CLR_IP.Cpp_Labeling(workplaceBuffer, binImg, chipW, chipH);

            string sInspectionID = DatabaseManager.Instance.GetInspectionID();

            //Add Defect
            for (int i = 0; i < Label.Length; i++)
            {
                if (Label[i].area > parameter.Size)
                {
                    this.currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.BAD_CHIP, true);

                    this.currentWorkplace.AddDefect(sInspectionID,
                        10010,
                        Label[i].area,
                        Label[i].value,
                        this.currentWorkplace.PositionX + Label[i].boundLeft,
                        this.currentWorkplace.PositionY - (chipH - Label[i].boundTop),
                        Label[i].width,
                        Label[i].height,
                        this.currentWorkplace.MapIndexX,
                        this.currentWorkplace.MapIndexY
                        );
                }

            }

            GoldenImages.Clear();
            WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
        }

        public void CreateGoldenImage()
        {
            if(GoldenImage == null)
                GoldenImage = new byte[this.currentWorkplace.Width * this.currentWorkplace.Height];

            if (parameter.RefImageUpdate == RefImageUpdateFreq.Line) // Line 별로 GoldenImage를 만들 경우
            {
                List<byte[]> wpDatas = new List<byte[]>();
                // Index 계산
                List<int> mapYIdx = new List<int>();
                foreach (Workplace wp in this.workplaceBundle)
                    if (wp.MapIndexX == this.currentWorkplace.MapIndexX)
                        mapYIdx.Add(wp.MapIndexY);

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
                    if (wp.MapIndexX == this.currentWorkplace.MapIndexX)
                        if (this.currentWorkplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == true)
                            if ((wp.MapIndexY >= startY) && (wp.MapIndexY <= endY) && wp.MapIndexY != this.currentWorkplace.MapIndexY)
                                wpDatas.Add(GetWorkplaceBuffer(this.parameter.IndexChannel));

                int width = currentWorkplace.Width;
                int height = currentWorkplace.Height;

                switch (parameter.CreateRefImage)
                {
                    case CreateRefImageMethod.Average:
                        CLR_IP.Cpp_CreateGoldenImage_Avg(wpDatas.ToArray(), GoldenImage, wpDatas.Count, width, height);
                        break;
                    case CreateRefImageMethod.NearAverage:
                        CLR_IP.Cpp_CreateGoldenImage_NearAvg(wpDatas.ToArray(), GoldenImage, wpDatas.Count, width, height);
                        break;
                    case CreateRefImageMethod.MedianAverage:
                        CLR_IP.Cpp_CreateGoldenImage_MedianAvg(wpDatas.ToArray(), GoldenImage, wpDatas.Count, width, height);

                        break;
                    case CreateRefImageMethod.Median:
                        CLR_IP.Cpp_CreateGoldenImage_Median(wpDatas.ToArray(), GoldenImage, wpDatas.Count, width, height);
                        break;
                    default:
                        CLR_IP.Cpp_CreateGoldenImage_Avg(wpDatas.ToArray(), GoldenImage, wpDatas.Count, width, height);
                        break;
                }
            }
        }
    }
}
