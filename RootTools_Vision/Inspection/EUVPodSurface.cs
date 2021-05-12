using RootTools_CLR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class EUVPodSurface : WorkBase
    {
        public override WORK_TYPE Type => WORK_TYPE.INSPECTION;

        private IntPtr inspectionSharedBuffer;
        private EUVPodSurfaceParameter parameterPod;
        private EUVPodSurfaceRecipe recipePod;

        public enum Insp
        {
            Stain,Side,TDI,Stack
        }
        protected override bool Execution()
        {
            DoInspection();
            return true;
        }

        protected override bool Preparation()
        {
            if (parameterPod == null || recipePod == null)
            {
                parameterPod = parameter as EUVPodSurfaceParameter;
                recipePod = recipe.GetItem<EUVPodSurfaceRecipe>();
            }
            return true;
        }

        void DoInspection()
        {
            if (currentWorkplace.Index == 0) 
                return;

            inspectionSharedBuffer = currentWorkplace.GetSharedBufferInfo(0);
            byte[] workplaceBuffer = GetWorkplaceBuffer(0);
            uint nGVLevel = parameterPod.PodStain.PitLevel;
            uint nDefectsz = parameterPod.PodStain.PitSize;

            int width = 350;
            int height = 180;
            //int width = currentWorkplace.Width;
            //int height = currentWorkplace.Height;
            byte[] arrBinImg = new byte[width*height];

            CLR_IP.Cpp_Threshold(workplaceBuffer, arrBinImg, width, height, false, (int)nGVLevel);

            // Filter
            switch (parameterPod.PodStain.DiffFilter)
            {
                case DiffFilterMethod.Average:
                    CLR_IP.Cpp_AverageBlur(arrBinImg, arrBinImg, width, height);
                    break;
                case DiffFilterMethod.Gaussian:
                    CLR_IP.Cpp_GaussianBlur(arrBinImg, arrBinImg, width, height, 2);
                    break;
                case DiffFilterMethod.Median:
                    CLR_IP.Cpp_MedianBlur(arrBinImg, arrBinImg, width, height, 3);
                    break;
                case DiffFilterMethod.Morphology:
                    CLR_IP.Cpp_Morphology(arrBinImg, arrBinImg, width, height, 3, "OPEN", 1);
                    break;
                default:
                    break;
            }
            MaskRecipe mask = recipe.GetItem<MaskRecipe>();

            Cpp_Point[] maskStartPoint = new Cpp_Point[mask.MaskList[parameterPod.PodStain.MaskIndex].PointLines.Count];
            int[] maskLength = new int[mask.MaskList[parameterPod.PodStain.MaskIndex].PointLines.Count];

            Cpp_Point tempPt = new Cpp_Point();
            for (int i = 0; i < mask.MaskList[parameterPod.PodStain.MaskIndex].PointLines.Count; i++)
            {
                maskStartPoint[i] = new Cpp_Point();
                maskStartPoint[i].x = mask.MaskList[parameterPod.PodStain.MaskIndex].PointLines[i].StartPoint.X;
                maskStartPoint[i].y = mask.MaskList[parameterPod.PodStain.MaskIndex].PointLines[i].StartPoint.Y;
                maskLength[i] = mask.MaskList[parameterPod.PodStain.MaskIndex].PointLines[i].Length;
            }

            CLR_IP.Cpp_Masking(arrBinImg, arrBinImg, maskStartPoint.ToArray(), maskLength.ToArray(), width, height);



            // Labeling
            var Label = CLR_IP.Cpp_Labeling(workplaceBuffer, arrBinImg, width, height, false);
            //var Label = CLR_IP.Cpp_Labeling_SubPix(workplaceBuffer, arrBinImg, chipW, chipH, isDarkInsp, nGrayLevel, 3);

            //string sInspectionID = DatabaseManager.Instance.GetInspectionID();
        }
    }
}
