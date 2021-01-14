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
        private SurfaceParameter parameter;
        private SurfaceRecipe recipeSurface;

        public Surface() : base()
        {
            m_sName = this.GetType().Name;
        }

        protected override bool Preparation()
        {
            if(this.parameter == null || this.recipeSurface == null)
            {
                this.parameter = this.recipe.GetRecipe<SurfaceParameter>();
                this.recipeSurface = this.recipe.GetRecipe<SurfaceRecipe>();
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
            if (this.workplace.Index == 0)
                return;

            this.inspectionSharedBuffer = this.workplace.GetSharedBuffer(this.parameter.IndexChannel);
            byte[] workplaceBuffer = GetWorkplaceBuffer(this.parameter.IndexChannel);

            // Inspection Param
            bool isDarkInsp = !parameter.IsBright; // Option
            int nGrayLevel = parameter.Intensity; // Option
            int nDefectSz = parameter.Size; // Option     

            int chipH = this.workplace.Width; // 현재는 ROI = Chip이기 때문에 사용. 추후 실제 Chip H, W를 Recipe에서 가지고 오자
            int chipW = this.workplace.Height;

            byte[] arrBinImg = new byte[chipW * chipH]; // Threashold 결과 array

            // Dark
            CLR_IP.Cpp_Threshold(workplaceBuffer, arrBinImg, chipW, chipH, isDarkInsp, nGrayLevel);

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
            var Label = CLR_IP.Cpp_Labeling_SubPix(workplaceBuffer, arrBinImg, chipW, chipH, isDarkInsp, nGrayLevel, 3);

            string sInspectionID = DatabaseManager.Instance.GetInspectionID();


            //if (Label.Length > 0)
            //{
            //    this.workplace.SetSubState(WORKPLACE_SUB_STATE.BAD_CHIP, true);

            //    //Add Defect
            //    for (int i = 0; i < Label.Length; i++)
            //    {
            //        if (Label[i].area > nDefectSz)
            //        {
            //            this.workplace.AddDefect(sInspectionID,
            //                10001,
            //                Label[i].area,
            //                Label[i].value,
            //                this.workplace.PositionX + Label[i].boundLeft,
            //                this.workplace.PositionY - (chipH - Label[i].boundTop),
            //                Label[i].width,
            //                Label[i].height,
            //                this.workplace.MapIndexX,
            //                this.workplace.MapIndexY
            //                );
            //        }
            //    }
            //}


            WorkEventManager.OnInspectionDone(this.workplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
        }
    }
}
