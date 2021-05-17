using RootTools;
using RootTools.Database;
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

        unsafe void DoInspection()
        {
            if (currentWorkplace.Index == 0) 
                return;

            inspectionSharedBuffer = currentWorkplace.GetSharedBufferInfo(0);
            byte[] workplaceBuffer = GetWorkplaceBufferByIndex(0);
            uint nGVLevel = parameterPod.PodStain.PitLevel = 200;
            uint nDefectsz = parameterPod.PodStain.PitSize = 2;

            IntPtr ptr = currentWorkplace.GetSharedBufferInfo(1);

            int width = currentWorkplace.Width;
            int height = currentWorkplace.Height;
            byte[] arrBinImg = new byte[width*height];

            byte* pptr = (byte*)ptr.ToPointer();
            byte* p = (byte*)inspectionSharedBuffer.ToPointer();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                    workplaceBuffer[(i * width) + j] = p[(i * width) + j];
                //Buffer.MemoryCopy((void*)arrBinImg, (void*)ptr, i * width, i * width);
            }

            CLR_IP.Cpp_Threshold(workplaceBuffer, arrBinImg, width, height, false, (int)250);
            //System.Drawing.Bitmap bitmap = Tools.CovertBufferToBitmap(currentWorkplace.SharedBufferInfo, new System.Windows.Rect(0, 0, width, height));

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                    pptr[(i * width) + j] = arrBinImg[(i * width) + j];
                //Buffer.MemoryCopy((void*)arrBinImg, (void*)ptr, i * width, i * width);

            }

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

            Cpp_Point[] maskStartPoint = new Cpp_Point[mask.MaskList[0].PointLines.Count];
            int[] maskLength = new int[mask.MaskList[0].PointLines.Count];

            Cpp_Point tempPt = new Cpp_Point();
            for (int i = 0; i < mask.MaskList[0].PointLines.Count; i++)
            {
                maskStartPoint[i] = new Cpp_Point();
                maskStartPoint[i].x = mask.MaskList[0].PointLines[i].StartPoint.X;
                maskStartPoint[i].y = mask.MaskList[0].PointLines[i].StartPoint.Y;
                maskLength[i] = mask.MaskList[0].PointLines[i].Length;
            }

            CLR_IP.Cpp_Masking(arrBinImg, arrBinImg, maskStartPoint.ToArray(), maskLength.ToArray(), width, height);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                    pptr[(i * width) + j] = arrBinImg[(i * width) + j];
            }

            // Labeling
            var Label = CLR_IP.Cpp_Labeling(workplaceBuffer, arrBinImg, width, height, true);

            List<Defect> li = new List<Defect>();
            if (Label.Length > 0)
            {
                this.currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.BAD_CHIP, true);

                //Add Defect
                for (int i = 0; i < Label.Length; i++)
                {
                    if (Label[i].area > 1)
                    {

                        Defect defect = new Defect(i.ToString(),
                            10001,
                            Label[i].area,
                            Label[i].value,
                            Label[i].boundLeft,
                            Label[i].boundTop,
                            Label[i].width,
                            Label[i].height,
                            this.currentWorkplace.MapIndexX,
                            this.currentWorkplace.MapIndexY);
                        currentWorkplace.DefectList.Add(defect);
                        //li.Add(defect);
                    }
                }
            }
            Tools.SaveDefectImageParallel(@"C:\Defects", currentWorkplace.DefectList, currentWorkplace.SharedBufferInfo, 1, new System.Windows.Point(100,100));

            //var Label = CLR_IP.Cpp_Labeling_SubPix(workplaceBuffer, arrBinImg, chipW, chipH, isDarkInsp, nGrayLevel, 3);

            //string sInspectionID = DatabaseManager.Instance.GetInspectionID();
        }
    }
}
