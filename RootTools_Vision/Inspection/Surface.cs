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

            bool isBackside = false;

            // BACKSIDE
            //if (this.workplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == false)
            //{
            //    return;
            //}

            // Inspection Param
            bool bGetDarkInsp = true; // Option
            int nGrayLevel = 15; // Option
            int nDefectSz = 0; // Option     

            int chipH = this.workplace.BufferSizeY; // 현재는 ROI = Chip이기 때문에 사용. 추후 실제 Chip H, W를 Recipe에서 가지고 오자
            int chipW = this.workplace.BufferSizeX;

            byte[] arrBinImg = new byte[chipW * chipH]; // Threashold 결과 array

            // Backside Test 할 때만 추가
            if(isBackside)
                ExtractCurrentWorkplace();

            // Dark
            CLR_IP.Cpp_Threshold(workplace.WorkplaceBuffer, arrBinImg, chipW, chipH, bGetDarkInsp, nGrayLevel);
            // Filtering
            CLR_IP.Cpp_Morphology(arrBinImg, arrBinImg, chipW, chipH, 3, "Close", 1);
            // Labeling
            //var Label = CLR_IP.Cpp_Labeling(workplace.WorkplaceBuffer, arrBinImg, chipW, chipH, bGetDarkInsp);
            var Label = CLR_IP.Cpp_Labeling_SubPix(workplace.WorkplaceBuffer, arrBinImg, chipW, chipH, bGetDarkInsp, nGrayLevel, 3);

            string sInspectionID = DatabaseManager.Instance.GetInspectionID();

            for (int i = 0; i < Label.Length; i++)
            {
                if (Label[i].area > nDefectSz)
                {
                    this.workplace.AddDefect(sInspectionID,
                        10011,
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

            //WorkEventManager.OnInspectionDone(this.workplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
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
                Marshal.Copy(new IntPtr(this.workplace.SharedBuffer.ToInt64() + (cnt * (Int64)memW + Left))
                    , this.workplace.WorkplaceBuffer, chipW * (cnt - Top), chipW);
        }

    }
}
