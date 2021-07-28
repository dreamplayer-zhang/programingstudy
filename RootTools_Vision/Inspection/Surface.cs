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
        public override WORK_TYPE Type => WORK_TYPE.INSPECTION;

        private IntPtr inspectionSharedBuffer;
        private SurfaceParameter parameterSurface;
        private SurfaceRecipe recipeSurface;

        public Surface() : base()
        {
            m_sName = this.GetType().Name;
        }

        protected override bool Preparation()
        {
            if(this.parameterSurface == null || this.recipeSurface == null)
            {
                this.parameterSurface = (SurfaceParameter)this.parameter;
                this.recipeSurface = this.recipe.GetItem<SurfaceRecipe>();
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

            this.inspectionSharedBuffer = this.currentWorkplace.GetSharedBufferInfo(this.parameterSurface.IndexChannel);
            byte[] workplaceBuffer = GetWorkplaceBufferByColorChannel(this.parameterSurface.IndexChannel);

            // Inspection Param
            bool isDarkInsp = !parameterSurface.IsBright; // Option
            int nGrayLevel = parameterSurface.Intensity; // Option
            int nDefectSz = parameterSurface.Size; // Option     

            int chipW = this.currentWorkplace.Width; // 현재는 ROI = Chip이기 때문에 사용. 추후 실제 Chip H, W를 Recipe에서 가지고 오자
            int chipH = this.currentWorkplace.Height;

            byte[] arrBinImg = new byte[chipW * chipH]; // Threashold 결과 array

            // Dark
            CLR_IP.Cpp_Threshold(workplaceBuffer, arrBinImg, chipW, chipH, isDarkInsp, nGrayLevel);

            // Filter
            switch (parameterSurface.DiffFilter)
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

            MaskRecipe mask = this.recipe.GetItem<MaskRecipe>(); //요기다 추가해줘용

            Cpp_Point[] maskStartPoint = new Cpp_Point[mask.MaskList[this.parameterSurface.MaskIndex].PointLines.Count];
            int[] maskLength = new int[mask.MaskList[this.parameterSurface.MaskIndex].PointLines.Count];

            Cpp_Point tempPt = new Cpp_Point();
            for (int i = 0; i < mask.MaskList[this.parameterSurface.MaskIndex].PointLines.Count; i++)
            {
                maskStartPoint[i] = new Cpp_Point();
                maskStartPoint[i].x = mask.MaskList[this.parameterSurface.MaskIndex].PointLines[i].StartPoint.X;
                maskStartPoint[i].y = mask.MaskList[this.parameterSurface.MaskIndex].PointLines[i].StartPoint.Y;
                maskLength[i] = mask.MaskList[this.parameterSurface.MaskIndex].PointLines[i].Length;
            }

            CLR_IP.Cpp_Masking(arrBinImg, arrBinImg, maskStartPoint.ToArray(), maskLength.ToArray(), chipW, chipH);

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
                        if(parameterSurface.SizeLimit > 0)
                        {
                            if(parameterSurface.SizeLimit < Label[i].area)
                                continue;
                        }

                        this.currentWorkplace.AddDefect(sInspectionID,
                            1000 + (int)this.parameterSurface.IndexChannel * 100 + this.parameterSurface.MaskIndex,
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
    }
}
