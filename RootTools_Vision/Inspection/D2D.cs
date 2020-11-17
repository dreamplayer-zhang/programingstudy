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
using RootTools_Vision.Temp_Recipe;
using RootTools_Vision.UserTypes;

namespace RootTools_Vision
{
    public class D2D : WorkBase
    {
        WorkplaceBundle workplaceBundle;
        Workplace workplace;

        byte[] GoldenImage = null;

        public override WORK_TYPE Type => WORK_TYPE.MAINWORK;

        public D2D() : base()
        {
            m_sName = this.GetType().Name;
        }

        public override bool DoPrework()
        {
            if(this.workplace.Index == 0)
            {
                return base.DoPrework();
            }

            if (this.workplace.GetSubState(WORKPLACE_SUB_STATE.LINE_FIRST_CHIP) == true &&
                this.workplaceBundle.CheckStateLine(this.workplace.MapPositionX, WORKPLACE_STATE.READY) &&
                this.IsPreworkDone == false)
            {
                CreateGoldenImage();

                // Golden Image Workplace에 복사
                foreach(Workplace wp in this.workplaceBundle)
                {
                    if(wp.MapPositionX == this.workplace.MapPositionX)
                    {
                        wp.SetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE, (object)(GoldenImage) /*Tools.ByteArrayToObject(GoldenImage)*/);
                    }
                }
            }

            if(this.workplace.GetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE) != null)
            {
                this.GoldenImage = this.workplace.GetPreworkData(PREWORKDATA_KEY.D2D_GOLDEN_IMAGE) as byte[];
                return base.DoPrework();
            }
            else
            {
                this.IsPreworkDone = false;
                return false;
            }
        }

        public override void DoWork()
        {
            DoInspection();

            base.DoWork();
        }

        public void SetGoldenImage(byte[] currentImage, int ChipW, int ChipH)
        {
            if (GoldenImage == null)
            {
                GoldenImage = new byte[ChipW * ChipH];
                currentImage.CopyTo(GoldenImage, 0);
            }
            else // Update Golden Image
            { 

            }
            
            // Golden 
        }

        public override void SetWorkplace(Workplace _workplace)
        {
            this.workplace = _workplace;
        }
        public void DoInspection()
        {
            if (this.workplace.Index == 0)
                return;

            if (this.workplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == false)
            {
                return;
            }

            WorkEventManager.OnInspectionDone(this.workplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...

            return;

            int nChipH = this.workplace.BufferSizeY; // 현재는 ROI = Chip이기 때문에 사용. 추후 실제 Chip H, W를 Recipe에서 가지고 오자
            int nChipW = this.workplace.BufferSizeX;

            int nMemH = this.workplace.SharedBufferHeight;
            int nMemW = this.workplace.SharedBufferWidth;

            int nGrayLevel = 160; // Option
            int nDefectSz = 5; // Option
            

            int Left = this.workplace.PositionX;
            int Top = this.workplace.PositionY - nChipH;
            int Right = this.workplace.PositionX + nChipW;
            int Bottom = this.workplace.PositionY;

            byte[] arrBinImg = new byte[nChipW * nChipH];           
            byte[] arrCopyImg = new byte[nChipW * nChipH];

            int nD2DTrigger = 0;
            int TransX = 0, TransY = 0;
            /// Split Color Image Test
            unsafe
            {
                for (int cnt = Top; cnt < Bottom; cnt++)
                    Marshal.Copy(new IntPtr(this.workplace.SharedBuffer.ToInt64() + (cnt * (Int64)nMemW + Left))
                        , arrCopyImg, nChipW * (cnt - Top), nChipW);

                if (nD2DTrigger > 0)
                {
                    byte[] arrTriggerImg = new byte[(nChipW + (nD2DTrigger * 2)) * (nChipH + (nD2DTrigger * 2))];

                    for (int cnt = Top - nD2DTrigger; cnt < Bottom + nD2DTrigger; cnt++)
                        Marshal.Copy(new IntPtr(this.workplace.SharedBuffer.ToInt64() + (cnt * (Int64)nMemW + Left - nD2DTrigger))
                            , arrTriggerImg, (nChipW + (nD2DTrigger * 2)) * (cnt - (Top - nD2DTrigger)), nChipW + (nD2DTrigger * 2));


                    CLR_IP.Cpp_FindMinDiffLoc(arrTriggerImg, arrCopyImg, TransX, TransY, nChipW, nChipH, nD2DTrigger);
                }
            }
            SetGoldenImage(arrCopyImg, nChipW, nChipH);

            // Diff Image 계산
            CLR_IP.Cpp_SubtractAbs(GoldenImage, arrCopyImg, arrCopyImg, nChipW, nChipH);
            // Threshold 값으로 Defect 탐색
            CLR_IP.Cpp_Threshold(arrCopyImg, arrBinImg, nChipW, nChipH, nGrayLevel);
            // Filter
            CLR_IP.Cpp_Morphology(arrBinImg, arrBinImg, nChipW, nChipH, 3, "OPEN", 1);
            var Label = CLR_IP.Cpp_Labeling(arrCopyImg, arrBinImg, nChipW, nChipH);

            string sInspectionID = DatabaseManager.Instance.GetInspectionID();

            //Add Defect
            for (int i = 0; i < Label.Length; i++)
            {
                if (Label[i].area > nDefectSz)
                {
                    this.workplace.AddDefect(sInspectionID,
                        10010,
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
        }

        public void CreateGoldenImage()
        {
            GoldenImage = new byte[this.workplace.BufferSizeX * this.workplace.BufferSizeY];
        }

        public override void SetData(IRecipeData _recipeData, IParameterData _parameterData)
        {

            RecipeData recipe = _recipeData as RecipeData;
            Parameter parameter = _parameterData as Parameter;
            //throw new NotImplementedException();
        }

        public override WorkBase Clone()
        {
            return (WorkBase)this.MemberwiseClone();
        }

        public override void SetWorkplaceBundle(WorkplaceBundle _workplaceBundle)
        {

            this.workplaceBundle = _workplaceBundle;
            return;
        }
    }
}
