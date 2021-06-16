﻿using RootTools;
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
                DoInspection(parameterPod.PodTDI.BrightParam, originRecipe.TDIOriginInfo, false, parameterPod.PodTDI.MaskIndex);
            if (parameterPod.PodTDI.DarkParam.IsEnable)
                DoInspection(parameterPod.PodTDI.BrightParam, originRecipe.TDIOriginInfo, true, parameterPod.PodTDI.MaskIndex);

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


        unsafe void DoInspection(SurfaceParam param,OriginInfo originInfo,bool IsDark,int MaskNum)
        {
            if (currentWorkplace.Index == 0)
                return;

            WorkEventManager.OnInspectionStart(currentWorkplace, new InspectionStartArgs());
            //inspectionSharedBuffer = currentWorkplace.GetSharedBufferInfo(0/*param.IllumCondition*/);
            inspectionSharedBuffer = currentWorkplace.SharedBufferInfo.PtrList[0];
            //byte[] workplaceBuffer = GetWorkplaceBufferByIndex(0/*parameterPod.PodStain.IllumCondition*/);
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
            byte[] arrBinImg = new byte[bufferwidth * bufferheight];
            byte[] workplaceBuffer = new byte[bufferwidth*bufferheight];
            int originLeft = originInfo.Origin.X;
            int originTop = originInfo.Origin.Y;

            byte* pptr = (byte*)ptr.ToPointer();
            byte* p = (byte*)inspectionSharedBuffer.ToPointer();

            for (int i = 0; i < originheight; i++)
            {
                for (int j = 0; j < originwidth; j++)
                {
                    workplaceBuffer[(i * bufferwidth) + j] = p[((i+ originTop) * bufferwidth) + j+ originTop];
                //Buffer.MemoryCopy((void*)arrBinImg, (void*)ptr, i * width, i * width);
                }
            }

            //for (int i = 0; i < originheight; i++)
            //{
            //    for (int j = 0; j < originwidth; j++)
            //        pptr[(i * bufferwidth) + j] = workplaceBuffer[(i * bufferwidth) + j];
            //    //Buffer.MemoryCopy((void*)arrBinImg, (void*)ptr, i * width, i * width);

            //}

            CLR_IP.Cpp_Threshold(workplaceBuffer, arrBinImg, bufferwidth, bufferheight, IsDark, (int)nGVLevel);
            //System.Drawing.Bitmap bitmap = Tools.CovertBufferToBitmap(currentWorkplace.SharedBufferInfo, new System.Windows.Rect(0, 0, width, height));

            //for (int i = 0; i < bufferheight; i++)
            //{
            //    for (int j = 0; j < bufferwidth; j++)
            //        pptr[(i * bufferwidth) + j] = arrBinImg[(i * bufferwidth) + j];
            //    //Buffer.MemoryCopy((void*)arrBinImg, (void*)ptr, i * width, i * width);

            //}
            // Filter
            switch (param.DiffFilter)
            {
                case DiffFilterMethod.Average:
                    CLR_IP.Cpp_AverageBlur(arrBinImg, arrBinImg, bufferwidth, bufferheight);
                    break;
                case DiffFilterMethod.Gaussian:
                    CLR_IP.Cpp_GaussianBlur(arrBinImg, arrBinImg, bufferwidth, bufferheight, 2);
                    break;
                case DiffFilterMethod.Median:
                    CLR_IP.Cpp_MedianBlur(arrBinImg, arrBinImg, bufferwidth, bufferheight, 3);
                    break;
                case DiffFilterMethod.Morphology:
                    CLR_IP.Cpp_Morphology(arrBinImg, arrBinImg, bufferwidth, bufferheight, 3, "OPEN", 1);
                    break;
                default:
                    break;
            }

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

            CLR_IP.Cpp_Masking(arrBinImg, arrBinImg, maskStartPoint.ToArray(), maskLength.ToArray(), bufferwidth, bufferheight);

            for (int i = 0; i < originheight; i++)
            {
                for (int j = 0; j < originwidth; j++)
                    pptr[(i * bufferwidth) + j] = arrBinImg[(i * bufferwidth) + j];
            }

            // Labeling
            var Label = CLR_IP.Cpp_Labeling(arrBinImg, arrBinImg, bufferwidth, bufferheight, IsDark);

            List<Defect> li = new List<Defect>();
            if (Label.Length > 0)
            {
                currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.BAD_CHIP, true);
                                //Add Defect
                for (int i = 0; i < Label.Length; i++)
                {
                    //int gv = p[Label[i].boundTop * width + Label[i].boundLeft];
                    if (Label[i].area >= nDefectsz && Label[i].area <= nMaxsz)
                    {
                        if (Label[i].value <= nMaxGV && Label[i].value >= nMinGV)
                        {
                            Defect defect = new Defect(i.ToString(),
                                10001,
                                Label[i].area,
                                Label[i].value,
                                Label[i].boundLeft + originLeft,
                                Label[i].boundTop + originTop,
                                Label[i].width,
                                Label[i].height,
                                currentWorkplace.MapIndexX,
                                currentWorkplace.MapIndexY);
                            currentWorkplace.DefectList.Add(defect);
                        }

                        //li.Add(defect);
                    }
                }
            }
            Tools.SaveDefectImageParallel(@"C:\Defects", currentWorkplace.DefectList, currentWorkplace.SharedBufferInfo, 1, new System.Windows.Point(100, 100));

            WorkEventManager.OnInspectionDone(currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...

            //var Label = CLR_IP.Cpp_Labeling_SubPix(workplaceBuffer, arrBinImg, chipW, chipH, isDarkInsp, nGrayLevel, 3);

            //string sInspectionID = DatabaseManager.Instance.GetInspectionID();
        }
    }
}

/*unsafe void DoInspection()
        {
            if (currentWorkplace.Index == 0)
                return;


            WorkEventManager.OnInspectionStart(currentWorkplace, new InspectionStartArgs());
            inspectionSharedBuffer = currentWorkplace.GetSharedBufferInfo(parameterPod.PodStain..IllumCondition);
            byte[] workplaceBuffer = GetWorkplaceBufferByIndex(parameterPod.PodStain.IllumCondition);
            uint nGVLevel = parameterPod.PodStain.PitLevel;
            uint nDefectsz = parameterPod.PodStain.PitSize;
            uint nMaxsz = parameterPod.PodStain.SizeMax;
            uint nMaxGV = parameterPod.PodStain.LevelMax;
            uint nMinGV = parameterPod.PodStain.LevelMin;
            IntPtr ptr = currentWorkplace.GetSharedBufferInfo(1);

            int width = currentWorkplace.Width;
            int height = currentWorkplace.Height;
            int _width = 1500;
            int _height = 1300;
            byte[] arrBinImg = new byte[width * height];

            byte* pptr = (byte*)ptr.ToPointer();
            byte* p = (byte*)inspectionSharedBuffer.ToPointer();
            //for (int i = 0; i < height; i++)
            //{
            //    for (int j = 0; j < width; j++)
            //        workplaceBuffer[(i * width) + j] = p[(i * width) + j];
            //    //Buffer.MemoryCopy((void*)arrBinImg, (void*)ptr, i * width, i * width);
            //}

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                    pptr[(i * _width) + j] = workplaceBuffer[(i * width) + j];
                //Buffer.MemoryCopy((void*)arrBinImg, (void*)ptr, i * width, i * width);

            }

            CLR_IP.Cpp_Threshold(workplaceBuffer, arrBinImg, width, height, false, (int)nGVLevel);
            //System.Drawing.Bitmap bitmap = Tools.CovertBufferToBitmap(currentWorkplace.SharedBufferInfo, new System.Windows.Rect(0, 0, width, height));

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                    pptr[(i * _width) + j] = arrBinImg[(i * width) + j];
                //Buffer.MemoryCopy((void*)arrBinImg, (void*)ptr, i * width, i * width);

            }
            parameterPod.PodStain.DiffFilter = DiffFilterMethod.Average;
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

            EUVOriginRecipe originRecipe = recipe.GetItem<EUVOriginRecipe>();

            MaskRecipe mask = recipe.GetItem<MaskRecipe>();

            Cpp_Point[] maskStartPoint = new Cpp_Point[mask.MaskList[0].PointLines.Count];
            int[] maskLength = new int[mask.MaskList[0].PointLines.Count];

            int left = originRecipe.StainOrigin.Origin.X;
            int Top = originRecipe.StainOrigin.Origin.Y - originRecipe.StainOrigin.OriginSize.Y;
            Cpp_Point tempPt = new Cpp_Point();
            for (int i = 0; i < mask.MaskList[0].PointLines.Count; i++)
            {
                maskStartPoint[i] = new Cpp_Point();
                maskStartPoint[i].x = mask.MaskList[0].PointLines[i].StartPoint.X - left;
                maskStartPoint[i].y = mask.MaskList[0].PointLines[i].StartPoint.Y - Top;
                maskLength[i] = mask.MaskList[0].PointLines[i].Length;
            }

            CLR_IP.Cpp_Masking(arrBinImg, arrBinImg, maskStartPoint.ToArray(), maskLength.ToArray(), width, height);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                    pptr[(i * _width) + j] = arrBinImg[(i * width) + j];
            }

            // Labeling
            var Label = CLR_IP.Cpp_Labeling(arrBinImg, arrBinImg, width, height, false);

            List<Defect> li = new List<Defect>();
            if (Label.Length > 0)
            {
                this.currentWorkplace.SetSubState(WORKPLACE_SUB_STATE.BAD_CHIP, true);
                //Add Defect
                for (int i = 0; i < Label.Length; i++)
                {
                    //int gv = p[Label[i].boundTop * width + Label[i].boundLeft];
                    if (Label[i].area >= nDefectsz && Label[i].area <= nMaxsz)
                    {
                        if (Label[i].value <= nMaxGV && Label[i].value >= nMinGV)
                        {
                            Defect defect = new Defect(i.ToString(),
                                10001,
                                Label[i].area,
                                Label[i].value,
                                Label[i].boundLeft + left,
                                Label[i].boundTop + Top,
                                Label[i].width,
                                Label[i].height,
                                this.currentWorkplace.MapIndexX,
                                this.currentWorkplace.MapIndexY);
                            currentWorkplace.DefectList.Add(defect);
                        }

                        //li.Add(defect);
                    }
                }
            }
            Tools.SaveDefectImageParallel(@"C:\Defects", currentWorkplace.DefectList, currentWorkplace.SharedBufferInfo, 1, new System.Windows.Point(100, 100));

            WorkEventManager.OnInspectionDone(currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...

            //var Label = CLR_IP.Cpp_Labeling_SubPix(workplaceBuffer, arrBinImg, chipW, chipH, isDarkInsp, nGrayLevel, 3);

            //string sInspectionID = DatabaseManager.Instance.GetInspectionID();
        }*/
