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

        public override WORK_TYPE Type => WORK_TYPE.MAINWORK;


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

            // BACKSIDE
            //if (this.workplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == false)
            //{
            //    return;
            //}

            int nChipH = this.workplace.BufferSizeY; // 현재는 ROI = Chip이기 때문에 사용. 추후 실제 Chip H, W를 Recipe에서 가지고 오자
            int nChipW = this.workplace.BufferSizeX;

            int nMemH = this.workplace.SharedBufferHeight;
            int nMemW = this.workplace.SharedBufferWidth;

            bool bGetDarkInsp = true; // Option
            int nGrayLevel = 100; // Option
            int nDefectSz = 1; // Option
            
            int Left = this.workplace.PositionX;
            int Top = this.workplace.PositionY - nChipH;
            int Right = this.workplace.PositionX + nChipW;
            int Bottom = this.workplace.PositionY;

            byte[] arrBinImg = new byte[nChipW * nChipH]; // Threashold 결과 array
            byte[] arrCopyImg = new byte[nChipW * nChipH];

            for (int cnt = Top; cnt < Bottom; cnt++)
                Marshal.Copy(new IntPtr(this.workplace.SharedBuffer.ToInt64() + (cnt * (Int64)nMemW + Left)), arrCopyImg, nChipW * (cnt - Top), nChipW); // << int 범위 초과 문제로 Long IntPtr로 변환
 

            CLR_IP.Cpp_Threshold(arrCopyImg, arrBinImg, nChipW, nChipH, bGetDarkInsp, nGrayLevel);
            //CLR_IP.Cpp_Morphology(arrBinImg, arrBinImg, nChipW, nChipH, 3, "OPEN", 1);
            var Label = CLR_IP.Cpp_Labeling(arrCopyImg, arrBinImg, nChipW, nChipH, bGetDarkInsp);

            string sInspectionID = DatabaseManager.Instance.GetInspectionID();

            for (int i = 0; i < Label.Length; i++)
            {
                if (Label[i].area > nDefectSz)
                {
                    this.workplace.AddDefect(sInspectionID,
                        10001,
                        Label[i].area,
                        Label[i].value,
                        this.workplace.PositionX + Label[i].boundLeft,
                        this.workplace.PositionY - (nChipH - Label[i].boundTop),
                        Math.Abs(Label[i].boundRight - Label[i].boundLeft),
                        Math.Abs(Label[i].boundBottom - Label[i].boundTop),                        
                        this.workplace.MapPositionX,
                        this.workplace.MapPositionY
                        );
                }

            }
            WorkEventManager.OnInspectionDone(this.workplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...


            //unsafe
            //{
            //    //float avg = CLR_IP.Cpp_Average(
            //    //    (byte*)this.workplace.SharedBuffer.ToPointer(), 
            //    //    nMemW, 
            //    //    nMemH, 
            //    //    this.workplace.PositionX + nTrasnX, 
            //    //    this.workplace.PositionY - nChipH + nTrasnY,
            //    //    this.workplace.PositionX + nChipW + nTrasnX,
            //    //    this.workplace.PositionY + nTrasnY);

            //    //if (bGetDarkInsp) //입력된 GrayLevel을 %로 사용하여 연산 // EX. 20이 입력되어 있다면 평균GV에서 20%감산.		
            //    //    nGrayLevel = (int)(avg * (1.0f - nGrayLevel / 100.0f));
            //    //else
            //    //    nGrayLevel = (int)(avg * (1.0f + nGrayLevel / 100.0f));

            //    CLR_IP.Cpp_Threshold(
            //        (byte*)this.workplace.SharedBuffer.ToPointer(),
            //        arrBinImg,
            //        nMemW,
            //        nMemH,
            //        this.workplace.PositionX + nTrasnX,
            //        this.workplace.PositionY - nChipH + nTrasnY,
            //        this.workplace.PositionX + nChipW + nTrasnX,
            //        this.workplace.PositionY + nTrasnY,
            //        bGetDarkInsp,
            //        nGrayLevel);

            //    //CLR_IP.Cpp_Morphology(arrBinImg, arrBinImg, nChipW, nChipH, 3, "Open", 3);

            //    var Label = CLR_IP.Cpp_Labeling(
            //        (byte*)this.workplace.SharedBuffer.ToPointer(),
            //        arrBinImg,
            //        nMemW,
            //        nMemH,
            //        this.workplace.PositionX + nTrasnX,
            //        this.workplace.PositionY - nChipH + nTrasnY,
            //        this.workplace.PositionX + nChipW + nTrasnX,
            //        this.workplace.PositionY + nTrasnY,
            //        bGetDarkInsp);

            //    sw.Stop();          // 종료

            //    for (int i = 0; i < Label.Length; i++)
            //    {
            //        if (Label[i].area > nDefectSz)
            //        {
            //            this.workplace.AddDefect(10001,
            //                Label[i].area,
            //                (int)sw.ElapsedMilliseconds, //[i].value,
            //                Math.Abs(Label[i].boundRight - Label[i].boundLeft),
            //                Math.Abs(Label[i].boundBottom - Label[i].boundTop),
            //                Label[i].boundLeft,
            //                nChipH - Label[i].boundBottom, // 상대 좌표는 chip의 좌 하단을 기준으로 Buffer의 좌표계와 Y값이 반대!
            //                this.workplace.PositionX + nTrasnX + Label[i].boundLeft,
            //                this.workplace.PositionY + nTrasnY - (nChipH - Label[i].boundBottom),
            //                this.workplace.MapPositionX,
            //                this.workplace.MapPositionY
            //                );
            //        }
            //    }
            //    WorkEventManager.OnInspectionDone(this.workplace, new InspectionDoneEventArgs(new List<CRect>()));
            //}
        }


        public override WorkBase Clone()
        {
            return (WorkBase)this.MemberwiseClone();
        }

        public override void SetWorkplaceBundle(WorkplaceBundle workplace)
        {
            return;
        }


    }
}
