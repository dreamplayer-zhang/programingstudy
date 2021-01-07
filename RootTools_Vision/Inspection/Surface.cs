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
    public class Surface : WorkBase
    {
        Workplace workplace;

        public override WORK_TYPE Type => WORK_TYPE.INSPECTION;

        private IntPtr inspectionSharedBuffer;
        private byte[] inspectionWorkplaceBuffer;
        private SurfaceParameter parameter;
        private SurfaceRecipe recipe;

        public Surface() : base()
        {
            m_sName = this.GetType().Name;
        }

        public override void SetRecipe(Recipe _recipe)
        {
            this.parameter = _recipe.GetRecipe<SurfaceParameter>();
            this.recipe = _recipe.GetRecipe<SurfaceRecipe>();
        }

        public override void DoWork()
        {
            DoInspection();
        }

        public override void SetWorkplace(Workplace _workplace)
        {
            this.workplace = _workplace;
        }
        public void DoInspection()
        {
            if (this.workplace.Index == 0)
                return;

            this.inspectionSharedBuffer = this.workplace.GetSharedBuffer(this.parameter.IndexChannel);
            this.inspectionWorkplaceBuffer = this.workplace.GetWorkplaceBuffer(this.parameter.IndexChannel);

            // Inspection Param
            bool isDarkInsp = !parameter.IsBright; // Option
            int nGrayLevel = parameter.Intensity; // Option
            int nDefectSz = parameter.Size; // Option     

            int chipH = this.workplace.BufferSizeY; // 현재는 ROI = Chip이기 때문에 사용. 추후 실제 Chip H, W를 Recipe에서 가지고 오자
            int chipW = this.workplace.BufferSizeX;

            byte[] arrBinImg = new byte[chipW * chipH]; // Threashold 결과 array

            // Dark
            CLR_IP.Cpp_Threshold(workplace.WorkplaceBufferR_GRAY, arrBinImg, chipW, chipH, isDarkInsp, nGrayLevel);

            // Filter
            switch (parameter.DiffFilter)
            {
                case DiffFilterMethod.Average:
                    CLR_IP.Cpp_AverageBlur(arrBinImg, arrBinImg, chipW, chipH);
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
            //var Label = CLR_IP.Cpp_Labeling(workplace.WorkplaceBuffer, arrBinImg, chipW, chipH, bGetDarkInsp);
            var Label = CLR_IP.Cpp_Labeling_SubPix(workplace.WorkplaceBufferR_GRAY, arrBinImg, chipW, chipH, isDarkInsp, nGrayLevel, 3);

            string sInspectionID = DatabaseManager.Instance.GetInspectionID();


            if (Label.Length > 0)
            {
                this.workplace.SetSubState(WORKPLACE_SUB_STATE.BAD_CHIP, true);

                //Add Defect
                for (int i = 0; i < Label.Length; i++)
                {
                    if (Label[i].area > nDefectSz)
                    {
                        this.workplace.AddDefect(sInspectionID,
                            10001,
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
            }


            WorkEventManager.OnInspectionDone(this.workplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
        }


        public override WorkBase Clone()
        {
            return (WorkBase)this.MemberwiseClone();
        }

        public override void SetWorkplaceBundle(WorkplaceBundle workplace)
        {
            return;
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
                Marshal.Copy(new IntPtr(this.workplace.SharedBufferR_GRAY.ToInt64() + (cnt * (Int64)memW + Left))
                    , this.workplace.WorkplaceBufferR_GRAY, chipW * (cnt - Top), chipW);
        }

    }
}
