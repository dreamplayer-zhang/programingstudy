using RootTools;
using RootTools.Database;
using RootTools_CLR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class BacksideSurface : WorkBase
    {
        public override WORK_TYPE Type => WORK_TYPE.INSPECTION;

        private IntPtr inspectionSharedBuffer;
        private BacksideSurfaceParameter parameterBackside;
        private BacksideRecipe recipeBackside;

        public BacksideSurface() : base()
        {
            m_sName = this.GetType().Name;
        }
        protected override bool Preparation()
        {
            if(this.parameterBackside == null || this.recipeBackside == null)
            {
                this.parameterBackside = this.parameter as BacksideSurfaceParameter;
                this.recipeBackside = recipe.GetItem<BacksideRecipe>();
            }
            return true;
        }

        protected override bool Execution()
        {
            DoInspection();

            return true;
        }

        public void DoInspection()
        {
            if (this.currentWorkplace.Index == 0)
                return;
            if(this.currentWorkplace.MapIndexX == -1 && this.currentWorkplace.MapIndexY == -1)
			{
                return;
			}


            this.inspectionSharedBuffer = this.currentWorkplace.GetSharedBufferInfo(this.parameterBackside.IndexChannel);
            byte[] workplaceBuffer = GetWorkplaceBufferByColorChannel(this.parameterBackside.IndexChannel);


            //Tools.SaveRawdataToBitmap(@"D:\test\buffer\workplace_" + string.Format("{0}_{1}.bmp", currentWorkplace.MapIndexY, currentWorkplace.MapIndexX),
            //    workplaceBuffer,
            //    currentWorkplace.Width, currentWorkplace.Height, 1);
            

            // Inspection Param
            bool isDarkInsp = !parameterBackside.IsBright; // Option
            int nGrayLevel = parameterBackside.Intensity; // Option
            int nDefectSz = parameterBackside.Size; // Option     
            bool bAdaptiveIntensity = parameterBackside.IsAdaptiveIntensity;
            int nAdaptiveOffset = parameterBackside.AdaptiveOffset;
            int chipW = this.currentWorkplace.Width; // 현재는 ROI = Chip이기 때문에 사용. 추후 실제 Chip H, W를 Recipe에서 가지고 오자
            int chipH = this.currentWorkplace.Height;

            byte[] arrBinImg = Enumerable.Repeat<byte>(255, chipH * chipW).ToArray<byte>(); // Threashold 결과 array
            // Masking - 검사영역 255, 비검사 영역 0
            OuterMasking(ref arrBinImg);

            if(bAdaptiveIntensity)
            { 
                nGrayLevel = CLR_IP.Cpp_FindDominantIntensity(workplaceBuffer, arrBinImg, chipW, chipH);

                if (isDarkInsp)
                {  
                    nGrayLevel = (nGrayLevel - nAdaptiveOffset <= 0) ? 1 : nGrayLevel - nAdaptiveOffset;
                }
                else
                { 
                    nGrayLevel = (nGrayLevel + nAdaptiveOffset >= 255) ? 254 : nGrayLevel + nAdaptiveOffset;
                }
            }

            if (isDarkInsp)
            {
                // 비검사 영역을 255로 만들어 Threhosld에 걸리지 않게 해줌
                CLR_IP.Cpp_Bitwise_NOT(arrBinImg, arrBinImg, chipW, chipH);
                CLR_IP.Cpp_Bitwise_OR(workplaceBuffer, arrBinImg, workplaceBuffer, chipW, chipH);
            }
            else
            {
                // 비검사 영역을 0으로 만들어 Threshold에 걸리지 않게 해줌
                CLR_IP.Cpp_Bitwise_AND(workplaceBuffer, arrBinImg, workplaceBuffer, chipW, chipH);
            }

            // Dark
            CLR_IP.Cpp_Threshold(workplaceBuffer, arrBinImg, chipW, chipH, isDarkInsp, nGrayLevel);

            // Filter
            switch (parameterBackside.DiffFilter)
            {
                case DiffFilterMethod.Average:
                    CLR_IP.Cpp_AverageBlur(arrBinImg, arrBinImg, chipW, chipH);//문제 있음. ipp exception발생중
                    break;
                case DiffFilterMethod.Gaussian:
                    CLR_IP.Cpp_GaussianBlur(arrBinImg, arrBinImg, chipW, chipH, 2);
                    break;
                case DiffFilterMethod.Median:
                    CLR_IP.Cpp_MedianBlur(arrBinImg, arrBinImg, chipW, chipH, 3);
                    break;
                case DiffFilterMethod.Morphology:
                    CLR_IP.Cpp_Morphology(arrBinImg, arrBinImg, chipW, chipH, 3, "OPEN", 1);
                    break;
                default:
                    break;
            }

            // Labeling
            var Label = CLR_IP.Cpp_Labeling(workplaceBuffer, arrBinImg, chipW, chipH, isDarkInsp);
            //var Label = CLR_IP.Cpp_Labeling_SubPix(workplaceBuffer, arrBinImg, chipW, chipH, isDarkInsp, nGrayLevel, 3);

            string sInspectionID = DatabaseManager.Instance.GetInspectionID();


            if (Label.Length > 0)
            {
                this.currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.BAD_CHIP, true);

                //Add Defect
                for (int i = 0; i < Label.Length; i++)
                {
                    if (Label[i].area > nDefectSz)
                    {
                        this.currentWorkplace.AddDefect(sInspectionID,
                            10001,
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
            }


            WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
        }


        private void OuterMasking(ref byte[] imgArr)
        {
            // Outer Area Mask
            double centerX = recipeBackside.CenterX;
            double centerY = recipeBackside.CenterY;
            double radius = recipeBackside.Radius;
            double radius_2 = radius * radius;

            double left = this.currentWorkplace.PositionX;
            double top = this.currentWorkplace.PositionY;
            double right = this.currentWorkplace.PositionX + this.currentWorkplace.Width;
            double bottom = this.currentWorkplace.PositionY + this.currentWorkplace.Height;

            int width = this.currentWorkplace.Width;
            int height = this.currentWorkplace.Height;


            // Mask 생성


            // 포함이 안된 경우
            long posX = (long)left, posY = (long)top;

            if (((left - centerX) * (left - centerX) + (top - centerX) * (top - centerX) <= radius_2) &&
                ((right - centerX) * (right - centerX) + (top - centerX) * (top - centerX) <= radius_2) &&
                ((right - centerX) * (right - centerX) + (bottom - centerX) * (bottom - centerX) <= radius_2) &&
                ((left - centerX) * (left - centerX) + (bottom - centerX) * (bottom - centerX) <= radius_2)
                )
            {
                return;
            }


            // Masking
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    long index = x + y * width;

                    long absX = (long)left + x - (long)centerX;
                    long absY = (long)centerY - (long)(top + y);

                    long absX_2 = (absX * absX);
                    long absY_2 = (absY * absY);

                    if (absX_2 + absY_2 > radius_2)
                    {
                        imgArr[(long)index] = 0;
                    }
                    else
                    {
                        imgArr[(long)index] = 255;
                    }
                }
            }
            
        }

        public override WorkBase Clone()
        {
            return (WorkBase)this.MemberwiseClone();
        }
    }
}
