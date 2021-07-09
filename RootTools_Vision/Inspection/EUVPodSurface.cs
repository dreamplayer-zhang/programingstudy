#undef IMG_DEBUG

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
    public class EUVPodSurface : WorkBase
    {
        public override WORK_TYPE Type => WORK_TYPE.INSPECTION;

        private IntPtr inspectionSharedBuffer;
        private EUVPodSurfaceParameter parameterPod;
        private EUVPodSurfaceRecipe recipePod;

        public EUVPodSurface():base()
        {
            m_sName = GetType().Name;
        }

        public enum Insp
        {
            Stain, Side, TDI, Stack
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

            EUVOriginRecipe originRecipe = recipe.GetItem<EUVOriginRecipe>();


            if (parameterPod.PodStain.BrightParam.IsEnable)
                DoInspection(parameterPod.PodStain.BrightParam,originRecipe.StainOriginInfo, false,parameterPod.PodStain.MaskIndex);
            if(parameterPod.PodStain.DarkParam.IsEnable)
                DoInspection(parameterPod.PodStain.BrightParam, originRecipe.StainOriginInfo,true, parameterPod.PodStain.MaskIndex);

            if (parameterPod.PodTDI.BrightParam.IsEnable)
                DoInspection(parameterPod.PodTDI.BrightParam, originRecipe.TDIOriginInfo, false, parameterPod.PodTDI.MaskIndex,2000,2000);
            if (parameterPod.PodTDI.DarkParam.IsEnable)
                DoInspection(parameterPod.PodTDI.BrightParam, originRecipe.TDIOriginInfo, true, parameterPod.PodTDI.MaskIndex,2000,2000);

            if (parameterPod.PodStacking.BrightParam.IsEnable)
                DoInspection(parameterPod.PodStacking.BrightParam, originRecipe.TDIOriginInfo, false, parameterPod.PodStacking.MaskIndex);
            if (parameterPod.PodStacking.DarkParam.IsEnable)
                DoInspection(parameterPod.PodStacking.BrightParam, originRecipe.TDIOriginInfo, true, parameterPod.PodStacking.MaskIndex);

            if (parameterPod.PodSideLR.BrightParam.IsEnable)
                DoInspection(parameterPod.PodSideLR.BrightParam, originRecipe.SideLROriginInfo, false, parameterPod.PodSideLR.MaskIndex);
            if (parameterPod.PodSideLR.DarkParam.IsEnable)
                DoInspection(parameterPod.PodSideLR.BrightParam, originRecipe.SideLROriginInfo, true, parameterPod.PodSideLR.MaskIndex);

            if (parameterPod.PodSideTB.BrightParam.IsEnable)
                DoInspection(parameterPod.PodSideTB.BrightParam, originRecipe.SideTBOriginInfo, false, parameterPod.PodSideTB.MaskIndex);
            if (parameterPod.PodSideTB.DarkParam.IsEnable)
                DoInspection(parameterPod.PodSideTB.BrightParam, originRecipe.SideTBOriginInfo, true, parameterPod.PodSideTB.MaskIndex);
        }

        unsafe void DoInspection(SurfaceParam param, OriginInfo originInfo, bool IsDark, int MaskNum,int WorkerWidth, int WorkerHeight) //이미지 사이즈가 큰애들
        {
            // 오리진 영역 내에서 WorkerWidth,Height 나눈 갯수만큼 할꺼임
            // 이함수에서 WorkplaceBuffer로 나누고 그걸 작은 단위 애한테 넘겨줄거임
            if (currentWorkplace.Index == 0)
                return;

            WorkEventManager.OnInspectionStart(currentWorkplace, new InspectionStartArgs());
            inspectionSharedBuffer = currentWorkplace.SharedBufferInfo.PtrList[MaskNum];
            uint nGVLevel = param.PitLevel;
            uint nDefectsz = param.PitSize;
            uint nMaxsz = param.SizeMax;
            uint nMaxGV = param.LevelMax;
            uint nMinGV = param.LevelMin;
            IntPtr ptr = currentWorkplace.SharedBufferInfo.PtrList[1];

            int bufferwidth = currentWorkplace.SharedBufferWidth;
            int bufferheight = currentWorkplace.SharedBufferHeight;
            int originwidth = originInfo.OriginSize.X;
            int originheight = originInfo.OriginSize.Y;
            byte[] arrBinImg = new byte[WorkerWidth * WorkerHeight];
            byte[] workplaceBuffer = new byte[WorkerWidth * WorkerHeight];
            int originLeft = originInfo.Origin.X;
            int originTop = originInfo.Origin.Y;

            byte* pptr = (byte*)ptr.ToPointer();
            byte* p = (byte*)inspectionSharedBuffer.ToPointer();

            Parallel.For(originTop, originheight + originTop, i => {
                Marshal.Copy(inspectionSharedBuffer + i * bufferwidth + originLeft, workplaceBuffer, (i - originTop) * originwidth, originwidth);
            });

            int totalX = originwidth / WorkerWidth;
            totalX = (totalX * WorkerWidth < originwidth) ? totalX : totalX + 1;
            int totalY = originheight / WorkerHeight;
            totalY = (totalY * WorkerWidth < originwidth) ? totalY : totalY + 1;

            for (int y=0;y<totalY; y++)
            {
                for(int x=0;x<totalX;x++)
                {
                    int startX = originLeft + x * WorkerWidth;
                    int startY = originTop + y * WorkerHeight;

                    Parallel.For(startY, WorkerHeight + startY, i => {
                        Marshal.Copy(inspectionSharedBuffer + i * bufferwidth + startX, workplaceBuffer, (i - startY) * WorkerWidth, WorkerWidth);
                    });

                    Inspection(workplaceBuffer, arrBinImg, originInfo, param, MaskNum, IsDark, WorkerWidth, WorkerHeight, x, y);
                }
            }

            WorkEventManager.OnInspectionDone(currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
        }

        //실질적으로 검사만 해서 currentWorkplace.DefectList에 추가하는 애
        unsafe void Inspection(byte[] workplaceBuffer,byte[] arrBinImg, OriginInfo originInfo, SurfaceParam param, int MaskNum,bool IsDark,
            int WorkWidth,int WorkHeight,int cntX=0, int cntY=0) 
        {
            int originwidth = originInfo.OriginSize.X;
            int originheight = originInfo.OriginSize.Y;
            int originLeft = originInfo.Origin.X;
            int originTop = originInfo.Origin.Y;

            uint nGVLevel = param.PitLevel;
            uint nDefectsz = param.PitSize;
            uint nMaxsz = param.SizeMax;
            uint nMaxGV = param.LevelMax;
            uint nMinGV = param.LevelMin;

            CLR_IP.Cpp_Threshold(workplaceBuffer, arrBinImg, WorkWidth, WorkHeight, IsDark, (int)nGVLevel);

#if IMG_DEBUG
            for (int i = 0; i < WorkHeight; i++)
            {
                for (int j = 0; j < WorkWidth; j++)
                    pptr[(i * currentWorkplace.SharedBufferWidth) + j] = workplaceBuffer[(i * WorkWidth) + j];
            }
#endif

            // Filter
            switch (param.DiffFilter)
            {
                case DiffFilterMethod.Average:
                    CLR_IP.Cpp_AverageBlur(arrBinImg, arrBinImg, WorkWidth, WorkHeight);
                    break;
                case DiffFilterMethod.Gaussian:
                    CLR_IP.Cpp_GaussianBlur(arrBinImg, arrBinImg, WorkWidth, WorkHeight, 2);
                    break;
                case DiffFilterMethod.Median:
                    CLR_IP.Cpp_MedianBlur(arrBinImg, arrBinImg, WorkWidth, WorkHeight, 3);
                    break;
                case DiffFilterMethod.Morphology:
                    CLR_IP.Cpp_Morphology(arrBinImg, arrBinImg, WorkWidth, WorkHeight, 3, "OPEN", 1);
                    break;
                default:
                    break;
            }
#if IMG_DEBUG
            for (int i = 0; i < WorkHeight; i++)
            {
                for (int j = 0; j < WorkWidth; j++)
                    pptr[(i * currentWorkplace.SharedBufferWidth) + j] = workplaceBuffer[(i * WorkWidth) + j];
            }
#endif
            MaskRecipe mask = recipe.GetItem<MaskRecipe>();

            Cpp_Point[] maskStartPoint = new Cpp_Point[mask.MaskList[MaskNum].PointLines.Count];
            int[] maskLength = new int[mask.MaskList[MaskNum].PointLines.Count];

            Cpp_Point tempPt = new Cpp_Point();
            for (int i = 0; i < mask.MaskList[MaskNum].PointLines.Count; i++)
            {
                maskStartPoint[i] = new Cpp_Point();
                maskStartPoint[i].x = mask.MaskList[MaskNum].PointLines[i].StartPoint.X - originLeft;
                maskStartPoint[i].y = mask.MaskList[MaskNum].PointLines[i].StartPoint.Y - originTop;
                maskLength[i] = mask.MaskList[MaskNum].PointLines[i].Length;
            }

            CLR_IP.Cpp_Masking(arrBinImg, arrBinImg, maskStartPoint.ToArray(), maskLength.ToArray(), WorkWidth, WorkHeight);
#if IMG_DEBUG
            for (int i = 0; i < WorkHeight; i++)
            {
                for (int j = 0; j < WorkWidth; j++)
                    pptr[(i * currentWorkplace.SharedBufferWidth) + j] = workplaceBuffer[(i * WorkWidth) + j];
            }
#endif
            // Labeling
            var Label = CLR_IP.Cpp_Labeling(arrBinImg, arrBinImg, WorkWidth, WorkHeight, IsDark);
            if (Label.Length > 0)
            {
                currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.BAD_CHIP, true);
                for (int i = 0; i < Label.Length; i++)
                {
                    if (Label[i].area >= nDefectsz && Label[i].area <= nMaxsz)
                    {
                        if (Label[i].value <= nMaxGV && Label[i].value >= nMinGV)
                        {
                            Defect defect = new Defect(param.DefectName,
                                (int)param.DefectCode,
                                Label[i].area,
                                Label[i].value,
                                Label[i].boundLeft + originLeft +WorkWidth*cntX,
                                Label[i].boundTop + originTop+WorkHeight*cntY,
                                Label[i].width,
                                Label[i].height,
                                currentWorkplace.MapIndexX,
                                currentWorkplace.MapIndexY);
                            currentWorkplace.DefectList.Add(defect);
                        }
                    }
                }
            }

        }
        unsafe void DoInspection(SurfaceParam param,OriginInfo originInfo,bool IsDark,int MaskNum) //이미지 사이즈가 작은 애들
        {
            if (currentWorkplace.Index == 0)
                return;

            WorkEventManager.OnInspectionStart(currentWorkplace, new InspectionStartArgs());
            inspectionSharedBuffer = currentWorkplace.SharedBufferInfo.PtrList[MaskNum];
            uint nGVLevel = param.PitLevel;
            uint nDefectsz = param.PitSize;
            uint nMaxsz = param.SizeMax;
            uint nMaxGV = param.LevelMax;
            uint nMinGV = param.LevelMin;
            IntPtr ptr = currentWorkplace.SharedBufferInfo.PtrList[1];

            int bufferwidth = currentWorkplace.SharedBufferWidth;
            int bufferheight = currentWorkplace.SharedBufferHeight;
            int originwidth = originInfo.OriginSize.X;
            int originheight = originInfo.OriginSize.Y;
            byte[] arrBinImg = new byte[originwidth * originheight];
            byte[] workplaceBuffer = new byte[originwidth * originheight];
            int originLeft = originInfo.Origin.X;
            int originTop = originInfo.Origin.Y;

            byte* pptr = (byte*)ptr.ToPointer();
            byte* p = (byte*)inspectionSharedBuffer.ToPointer();

            Parallel.For(originTop, originheight + originTop, i => { 
                Marshal.Copy(inspectionSharedBuffer + i * bufferwidth+originLeft,workplaceBuffer,(i-originTop)*originwidth,originwidth);
            });

            Inspection(workplaceBuffer, arrBinImg, originInfo, param, MaskNum, IsDark,originwidth,originheight);

            //Tools.SaveDefectImageParallel(@"C:\Defects", currentWorkplace.DefectList, currentWorkplace.SharedBufferInfo, 1, new System.Windows.Point(100, 100));

            WorkEventManager.OnInspectionDone(currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...

            //var Label = CLR_IP.Cpp_Labeling_SubPix(workplaceBuffer, arrBinImg, chipW, chipH, isDarkInsp, nGrayLevel, 3);

            //string sInspectionID = DatabaseManager.Instance.GetInspectionID();
        }
    }
}