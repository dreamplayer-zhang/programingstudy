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

        public override WORK_TYPE Type => WORK_TYPE.INSPECTION;

        // D2D Recipe & Parameter
        private D2DParameter parameterD2D;
        private D2DRecipe recipeD2D;


        private IntPtr inspectionSharedBuffer;
        private byte[] inspectionWorkBuffer;
        #endregion

        public D2D() : base()
        {
            m_sName = this.GetType().Name;
        }

        protected override bool Preparation()
        {
            if (this.currentWorkplace == null) return false;

            if(this.parameterD2D == null || this.recipeD2D == null)
            {
                this.parameterD2D = (D2DParameter)this.parameter;
                this.recipeD2D = this.recipe.GetItem<D2DRecipe>();
            }
            

            if (this.currentWorkplace.Index == 0)
            {
                return true;
            }

            this.inspectionSharedBuffer = this.currentWorkplace.GetSharedBufferInfo(this.parameterD2D.IndexChannel);

            if (this.currentWorkplace.GetSubState(WORKPLACE_SUB_STATE.LINE_FIRST_CHIP) == true &&
                this.workplaceBundle.CheckStateLine(this.currentWorkplace.MapIndexX, WORK_TYPE.ALIGNMENT) &&
                this.IsPreworkDone == false)
            {
                if (parameterD2D.RefImageUpdate == RefImageUpdateFreq.Line) // Line 별로 GoldenImage를 만들 경우
                {
                    CreateGoldenImage();

                    // Golden Image 해당라인 Workplace에 복사
                    foreach (Workplace wp in this.workplaceBundle)
                    {
                        if (wp.MapIndexX == this.currentWorkplace.MapIndexX)
                        {
                            wp.SetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE, (object)(GoldenImage) /*Tools.ByteArrayToObject(GoldenImage)*/);
                        }
                    }
                }
                // Chip_trigger mode에서는 golden image를 생성하지 않으므로 첫 chip에서 golden image를 임시로 생성 후 scale map을 만들어줍니다.
                else if((parameterD2D.RefImageUpdate == RefImageUpdateFreq.Chip_Trigger) && (parameterD2D.ScaleMap == true))
                {
                    if (this.currentWorkplace.GetPreworkData(PREWORKDATA_KEY.D2D_SCALE_MAP) == null)
                    { 
                        // 미리 생성해둔 Golden Image가 있으면 불러와서 Scale Map 생성
                        if (recipeD2D.PreGoldenW == this.currentWorkplace.Width && recipeD2D.PreGoldenH == this.currentWorkplace.Height)
                        {
                            GoldenImage = recipeD2D.PreGolden[(int)parameterD2D.IndexChannel];
                        }
                        // 미리 생성해둔 Golden Image가 없으면 임시로 생성
                        else
                        { 
                            CreateGoldenImage();
                        }

                        if (scaleMap == null)
                            scaleMap = new float[this.currentWorkplace.Width * this.currentWorkplace.Height];

                        CLR_IP.Cpp_CreateDiffScaleMap(GoldenImage, scaleMap, this.currentWorkplace.Width, this.currentWorkplace.Height, 10, 10);

                        foreach (Workplace wp in this.workplaceBundle)
                            wp.SetPreworkData(PREWORKDATA_KEY.D2D_SCALE_MAP, (object)(scaleMap));
                    }
                }
                else if(parameterD2D.RefImageUpdate == RefImageUpdateFreq.PreCreate)
                {
                    if (this.currentWorkplace.GetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE) == null)
                    {
                        // 미리 생성해둔 Golden Image가 있으면 불러와서 Scale Map 생성
                        if (recipeD2D.PreGoldenW == this.currentWorkplace.Width && recipeD2D.PreGoldenH == this.currentWorkplace.Height)
                        {
                            GoldenImage = recipeD2D.PreGolden[(int)parameterD2D.IndexChannel];
                        }
                        // 미리 생성해둔 Golden Image가 반드시 있어야하는데 없으면 임시로 생성 (검사 잘 안될듯)
                        else
                        {
                            CreateGoldenImage();
                        }

                        // Golden Image 해당라인 Workplace에 복사
                        foreach (Workplace wp in this.workplaceBundle)
                            wp.SetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE, (object)(GoldenImage) /*Tools.ByteArrayToObject(GoldenImage)*/);
                    }
                     
                }
                else
                {
                    return true;
                }
            }
            else if(!this.workplaceBundle.CheckStateLine(this.currentWorkplace.MapIndexX, WORK_TYPE.ALIGNMENT) &&
                this.IsPreworkDone == false)
            {
                return false;
            }

            if (parameterD2D.RefImageUpdate == RefImageUpdateFreq.Line)
            {
                if (this.currentWorkplace.GetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE) != null)
                {
                    this.GoldenImage = this.currentWorkplace.GetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE) as byte[];
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if ((parameterD2D.RefImageUpdate == RefImageUpdateFreq.Chip_Trigger) && (parameterD2D.ScaleMap == true))
            {
                if (this.currentWorkplace.GetPreworkData(PREWORKDATA_KEY.D2D_SCALE_MAP) != null)
                {
                    this.scaleMap = this.currentWorkplace.GetPreworkData(PREWORKDATA_KEY.D2D_SCALE_MAP) as float[];
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (parameterD2D.RefImageUpdate == RefImageUpdateFreq.PreCreate)
            {
                if (this.currentWorkplace.GetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE) != null)
                {
                    this.GoldenImage = this.currentWorkplace.GetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE) as byte[];
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        protected override bool Execution()
        {
            DoInspection();

            return true;
        }
        object lockObj = new object();
        public void SetGoldenImage()
        {
            // Index 계산
            List<int> mapYIdx = new List<int>();
            foreach (Workplace wp in this.workplaceBundle)
                if (wp.MapIndexX == this.currentWorkplace.MapIndexX)
                    mapYIdx.Add(wp.MapIndexY);

            mapYIdx.Sort();

            int startY;
            int endY;

            bool isRefNotEnough = false;
            if (mapYIdx.Count() <= 2) 
            {
                // PreCreate Golden Image가 있으면 대신 사용
                if (recipeD2D.PreGoldenW == this.currentWorkplace.Width && recipeD2D.PreGoldenH == this.currentWorkplace.Height)
                {
                    GoldenImage = recipeD2D.PreGolden[(int)parameterD2D.IndexChannel];
                    return;
                }

                // Line에 Ref Chip이 한개면 Golden Image 생성시 현재 칩까지 넣어서 average로 만듦 
                isRefNotEnough = true;
            }

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

            List<Cpp_Point> wpROIData = new List<Cpp_Point>();

            foreach (Workplace wp in this.workplaceBundle)
                if (wp.MapIndexX == this.currentWorkplace.MapIndexX)
                    if (this.currentWorkplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == true)
                        if ((wp.MapIndexY >= startY) && (wp.MapIndexY <= endY) && wp.MapIndexY != this.currentWorkplace.MapIndexY)
                            wpROIData.Add(new Cpp_Point(wp.PositionX, wp.PositionY));

            if(isRefNotEnough) // Golden Image Method들은 칩 개수가 부족할 경우 모두 Average로 동작
                wpROIData.Add(new Cpp_Point(this.currentWorkplace.PositionX, this.currentWorkplace.PositionY));

            unsafe
            {
                //lock (this.lockObj)
                { 
                    switch (parameterD2D.CreateRefImage)
                    {
                        case CreateRefImageMethod.Average:
                            CLR_IP.Cpp_CreateGoldenImage_Avg((byte*)this.inspectionSharedBuffer.ToPointer(), GoldenImage, wpROIData.Count, 
                                this.currentWorkplace.SharedBufferInfo.Width, this.currentWorkplace.SharedBufferInfo.Height,  
                                wpROIData, this.currentWorkplace.Width, this.currentWorkplace.Height);
                            break;
                        case CreateRefImageMethod.MedianAverage:
                            CLR_IP.Cpp_CreateGoldenImage_MedianAvg((byte*)this.inspectionSharedBuffer.ToPointer(), GoldenImage, wpROIData.Count,
                                this.currentWorkplace.SharedBufferInfo.Width, this.currentWorkplace.SharedBufferInfo.Height,
                                wpROIData, this.currentWorkplace.Width, this.currentWorkplace.Height);

                            break;
                        case CreateRefImageMethod.Median:
                            CLR_IP.Cpp_CreateGoldenImage_Median((byte*)this.inspectionSharedBuffer.ToPointer(), GoldenImage, wpROIData.Count,
                                this.currentWorkplace.SharedBufferInfo.Width, this.currentWorkplace.SharedBufferInfo.Height,
                                wpROIData, this.currentWorkplace.Width, this.currentWorkplace.Height);
                            break;
                        default:
                            CLR_IP.Cpp_CreateGoldenImage_Avg((byte*)this.inspectionSharedBuffer.ToPointer(), GoldenImage, wpROIData.Count,
                                this.currentWorkplace.SharedBufferInfo.Width, this.currentWorkplace.SharedBufferInfo.Height,
                                wpROIData, this.currentWorkplace.Width, this.currentWorkplace.Height);
                            break;
                    }
                }
            }
        }
        public List<Cpp_Point> TriggerDiffImage()
        {
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

            List<Cpp_Point> wpROIData = new List<Cpp_Point>();

            foreach (Workplace wp in this.workplaceBundle)
                if (wp.MapIndexX == this.currentWorkplace.MapIndexX)
                    if (this.currentWorkplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == true)
                        if ((wp.MapIndexY >= startY) && (wp.MapIndexY <= endY) && wp.MapIndexY != this.currentWorkplace.MapIndexY)
                            wpROIData.Add(new Cpp_Point(wp.PositionX, wp.PositionY));

            return wpROIData;
        }

        public override WorkBase Clone()
        {
            D2D d2d = new D2D();
            d2d = (D2D)this.MemberwiseClone();
            d2d.GoldenImage = null;
            return d2d;
        }
        
        public void DoInspection()
        {
            if (this.currentWorkplace.Index == 0)
                return;

            if (this.currentWorkplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == false)
            {
                return;
            }

            this.inspectionSharedBuffer = this.currentWorkplace.GetSharedBufferInfo(this.parameterD2D.IndexChannel);
            this.inspectionWorkBuffer = this.GetWorkplaceBufferByColorChannel(this.parameterD2D.IndexChannel);

            int memH = this.currentWorkplace.SharedBufferInfo.Height;
            int memW = this.currentWorkplace.SharedBufferInfo.Width;

            // Recipe
            int chipH = this.currentWorkplace.Height; // 현재는 ROI = Chip이기 때문에 사용. 추후 실제 Chip H, W를 Recipe에서 가지고 오자
            int chipW = this.currentWorkplace.Width;

            byte[] binImg = new byte[chipW * chipH];
            byte[] diffImg = new byte[chipW * chipH];

            RefImageUpdateFreq refUpdateFreq = parameterD2D.RefImageUpdate;

            if (refUpdateFreq == RefImageUpdateFreq.Chip_Trigger) // D2D 4.0 Algorithm 
            {
                unsafe
                {
                    List<Cpp_Point> wpROIData = TriggerDiffImage();

                    if (wpROIData. Count <= 2)
                    {
                        // D2D 4.0은 Ref칩 개수가 1개일 경우 정상동작하지 않음. -> Golden Image Average로 생성
                        refUpdateFreq = RefImageUpdateFreq.Chip;
                    }
                    else
                    {
                        CLR_IP.Cpp_SelectMinDiffinArea((byte*)this.inspectionSharedBuffer.ToPointer(), diffImg, wpROIData.Count,
                                            this.currentWorkplace.SharedBufferInfo.Width, this.currentWorkplace.SharedBufferInfo.Height,
                                            wpROIData, new Cpp_Point(this.currentWorkplace.PositionX, this.currentWorkplace.PositionY)
                                            , 1, this.currentWorkplace.Width, this.currentWorkplace.Height);

                        if (parameterD2D.ScaleMap) // ScaleMap Option
                        { 
                            CLR_IP.Cpp_Multiply(diffImg, scaleMap, diffImg, chipW, chipH);
                        }
                    }
                }
            }

            if(refUpdateFreq != RefImageUpdateFreq.Chip_Trigger)
            {
                if (refUpdateFreq == RefImageUpdateFreq.Chip) // Chip마다 Golden Image 생성 옵션
                {
                    if(this.currentWorkplace.GetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE) == null)
                        this.GoldenImage = new byte[this.currentWorkplace.Width * this.currentWorkplace.Height];
                    SetGoldenImage();
                }
                // Diff Image 계산
                CLR_IP.Cpp_SubtractAbs(GoldenImage, inspectionWorkBuffer, diffImg, chipW, chipH);

                if (parameterD2D.ScaleMap) // ScaleMap Option
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

                if (parameterD2D.HistWeightMap) // Histogram WeightMap
                {
                    histWeightMap = new float[chipW * chipH];
                    CLR_IP.Cpp_CreateHistogramWeightMap(inspectionWorkBuffer, GoldenImage, histWeightMap, chipW, chipH, 5);
                    CLR_IP.Cpp_Multiply(diffImg, histWeightMap, diffImg, chipW, chipH);
                }
            }

            // Filter
            switch (parameterD2D.DiffFilter)
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
            CLR_IP.Cpp_Threshold(diffImg, binImg, chipW, chipH, parameterD2D.Intensity);
            
            // Mask
            MaskRecipe mask = this.recipe.GetItem<MaskRecipe>();

            Cpp_Point[] maskStartPoint = new Cpp_Point[mask.MaskList[this.parameterD2D.MaskIndex].PointLines.Count];
            int[] maskLength = new int[mask.MaskList[this.parameterD2D.MaskIndex].PointLines.Count];

            Cpp_Point tempPt = new Cpp_Point();
            for(int i = 0; i < mask.MaskList[this.parameterD2D.MaskIndex].PointLines.Count; i++)
            {
                maskStartPoint[i] = new Cpp_Point();
                maskStartPoint[i].x = mask.MaskList[this.parameterD2D.MaskIndex].PointLines[i].StartPoint.X;
                maskStartPoint[i].y = mask.MaskList[this.parameterD2D.MaskIndex].PointLines[i].StartPoint.Y;
                maskLength[i] = mask.MaskList[this.parameterD2D.MaskIndex].PointLines[i].Length;
            }
            CLR_IP.Cpp_Masking(binImg, binImg, maskStartPoint.ToArray(), maskLength.ToArray(), chipW, chipH);

            // Labeling
            var Label = CLR_IP.Cpp_Labeling(inspectionWorkBuffer, binImg, chipW, chipH);

            string sInspectionID = DatabaseManager.Instance.GetInspectionID();

            //Add Defect
            for (int i = 0; i < Label.Length; i++)
            {
                if (Label[i].area > parameterD2D.Size)
                {
                    this.currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.BAD_CHIP, true);

                    this.currentWorkplace.AddDefect(sInspectionID,
                        2000 + (int)this.parameterD2D.IndexChannel * 100 + this.parameterD2D.MaskIndex + 1,
                        Label[i].area,
                        Label[i].value,
                        this.currentWorkplace.PositionX + Label[i].boundLeft,
                        this.currentWorkplace.PositionY + Label[i].boundTop,
                        Label[i].width,
                        Label[i].height,
                        this.currentWorkplace.MapIndexX,
                        this.currentWorkplace.MapIndexY
                        );
                }
            }
            WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>(), this.currentWorkplace)); // 나중에 ProcessDefect쪽 EVENT로...
        }

        public void CreateGoldenImage()
        {
            if(GoldenImage == null)
                GoldenImage = new byte[this.currentWorkplace.Width * this.currentWorkplace.Height];

            // Index 계산
            List<int> mapYIdx = new List<int>();
            foreach (Workplace wp in this.workplaceBundle)
                if (wp.MapIndexX == this.currentWorkplace.MapIndexX)
                    mapYIdx.Add(wp.MapIndexY);

            mapYIdx.Sort();

            int startY = 0;
            int endY = 0;

            bool isRefNotEnough = false;
            if (mapYIdx.Count() <= 2) // Line에 Ref Chip이 한개면 Golden Image 생성시 현재 칩까지 넣어서 average로 만듦 
            {
                // PreCreate Golden Image가 있으면 대신 사용
                if (recipeD2D.PreGoldenW == this.currentWorkplace.Width && recipeD2D.PreGoldenH == this.currentWorkplace.Height)
                {
                    GoldenImage = recipeD2D.PreGolden[(int)parameterD2D.IndexChannel];
                    return;
                }

                // Line에 Ref Chip이 한개면 Golden Image 생성시 현재 칩까지 넣어서 average로 만듦 
                isRefNotEnough = true;
            }

            if (mapYIdx.Count() < 5)
            {
                startY = mapYIdx[0];
                endY = mapYIdx[mapYIdx.Count() - 1];
            }
            else
            {
                // 중심에 있는 4개의 칩으로만 Golden Image 생성
                startY = mapYIdx[(mapYIdx.Count() + mapYIdx.Count() % 2) / 2 - 1 - 2];
                endY = mapYIdx[(mapYIdx.Count() + mapYIdx.Count() % 2) / 2 - 1 + 2];
                // 칩 전체 다 쓰기
                //startY = mapYIdx[0];
                //endY = mapYIdx[mapYIdx.Count() - 1];
            }

            List<Cpp_Point> wpROIData = new List<Cpp_Point>();

            foreach (Workplace wp in this.workplaceBundle)
                if (wp.MapIndexX == this.currentWorkplace.MapIndexX)
                    if (this.currentWorkplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == true)
                        if ((wp.MapIndexY >= startY) && (wp.MapIndexY <= endY))
                            wpROIData.Add(new Cpp_Point(wp.PositionX, wp.PositionY));

            if (isRefNotEnough) // Golden Image Method들은 칩 개수가 부족할 경우 모두 Average로 동작
                wpROIData.Add(new Cpp_Point(this.currentWorkplace.PositionX, this.currentWorkplace.PositionY));

            unsafe
            {
                switch (parameterD2D.CreateRefImage)
                {
                    case CreateRefImageMethod.Average:
                        CLR_IP.Cpp_CreateGoldenImage_Avg((byte*)this.inspectionSharedBuffer.ToPointer(), GoldenImage, wpROIData.Count,
                            this.currentWorkplace.SharedBufferInfo.Width, this.currentWorkplace.SharedBufferInfo.Height,
                            wpROIData, this.currentWorkplace.Width, this.currentWorkplace.Height);
                        break;
                    case CreateRefImageMethod.MedianAverage:
                        CLR_IP.Cpp_CreateGoldenImage_MedianAvg((byte*)this.inspectionSharedBuffer.ToPointer(), GoldenImage, wpROIData.Count,
                            this.currentWorkplace.SharedBufferInfo.Width, this.currentWorkplace.SharedBufferInfo.Height,
                            wpROIData, this.currentWorkplace.Width, this.currentWorkplace.Height);
                        break;
                    case CreateRefImageMethod.Median:
                        CLR_IP.Cpp_CreateGoldenImage_Median((byte*)this.inspectionSharedBuffer.ToPointer(), GoldenImage, wpROIData.Count,
                            this.currentWorkplace.SharedBufferInfo.Width, this.currentWorkplace.SharedBufferInfo.Height,
                            wpROIData, this.currentWorkplace.Width, this.currentWorkplace.Height);
                        break;
                    default:
                        CLR_IP.Cpp_CreateGoldenImage_Avg((byte*)this.inspectionSharedBuffer.ToPointer(), GoldenImage, wpROIData.Count,
                            this.currentWorkplace.SharedBufferInfo.Width, this.currentWorkplace.SharedBufferInfo.Height,
                            wpROIData, this.currentWorkplace.Width, this.currentWorkplace.Height);
                        break;
                }
            }
        }
    }
}
