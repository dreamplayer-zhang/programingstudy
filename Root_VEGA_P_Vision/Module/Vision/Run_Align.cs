using RootTools;
using RootTools.Camera;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using RootTools_CLR;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision.Module
{
    class Run_Align:ModuleRunBase
    {
        Vision m_module;
        Vision.MainOptic mainOpt;
        GrabMode AlignGrabMode;
        string sAlignGrabMode;
        int nxtrigger, nytrigger;
        double nscore;

        public Run_Align(Vision module)
        {
            m_module = module;
            Init();
            InitModuleRun(module);
        }
        void Init()
        {
            mainOpt = m_module.m_mainOptic;
            sAlignGrabMode = "";
        }
        #region Property
        public string p_sAlignGrabMode
        {
            get => sAlignGrabMode;
            set
            {
                sAlignGrabMode = value;
                AlignGrabMode = m_module.GetGrabMode(value);
            }
        }
        public int nXTrigger
        {
            get => nxtrigger;
            set => SetProperty(ref nxtrigger, value);
        }
        public int nYTrigger
        {
            get => nytrigger;
            set => SetProperty(ref nytrigger, value);
        }
        public double nScore
        {
            get => nscore;
            set => SetProperty(ref nscore, value);
        }
        #endregion
        public override ModuleRunBase Clone()
        {
            Run_Align run = new Run_Align(m_module);
            run.p_sAlignGrabMode = p_sAlignGrabMode;
            run.nXTrigger = nXTrigger;
            run.nYTrigger = nYTrigger;
            run.nScore = nScore;

            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sAlignGrabMode = tree.Set(p_sAlignGrabMode, p_sAlignGrabMode, m_module.p_asGrabMode, "Grab Mode : Align Grab", "Select GrabMode", bVisible);
            nXTrigger = tree.Set(nXTrigger, nXTrigger, "Trigger X", "Trigger X", bVisible);
            nYTrigger = tree.Set(nYTrigger, nYTrigger, "Trigger X", "Trigger X", bVisible);
            nScore = tree.Set(nScore, nScore, "Match Score", "Match Score", bVisible);

            if (AlignGrabMode != null)
                AlignGrabMode.RunTreeLinescanOption(tree, bVisible);
        }

        public override string Run()
        {
            if (p_sAlignGrabMode == null) return "Grab Mode : Align Grab == NULL";

            try
            {
                #region [Local Variable]
                AxisXY axisXY = m_module.m_stage.m_axisXY;
                Axis axisZ = mainOpt.m_axisZ;
                CPoint cpMemoryOffset = new CPoint(AlignGrabMode.m_cpMemoryOffset);

                int nScanLine = 0;
                int nMMPerUM = 1000;
                AlignGrabMode.m_dTrigger = Convert.ToInt32(10 * AlignGrabMode.m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                int nCamWidth = AlignGrabMode.m_camera.GetRoiSize().X;
                int nCamHeight = AlignGrabMode.m_camera.GetRoiSize().Y;
                int nFOV = AlignGrabMode.m_GD.m_nFovSize;
                int nPodSizeY_px = Convert.ToInt32(AlignGrabMode.m_nPodSize_mm * nMMPerUM / AlignGrabMode.m_dResY_um);  //파드 영역의 Y픽셀 갯수
                int nPulsePerWidth = nFOV * AlignGrabMode.m_dTrigger;
                int nPulsePerHeight = nCamHeight * AlignGrabMode.m_dTrigger;
                double dXScale = AlignGrabMode.m_dResX_um * 10;
                int nTotalTriggerCount = Convert.ToInt32(AlignGrabMode.m_dTrigger * nPodSizeY_px);
                int nScanSpeed = Convert.ToInt32((double)AlignGrabMode.m_nMaxFrame * AlignGrabMode.m_dTrigger * nCamHeight * AlignGrabMode.m_nScanRate / 100);
                int nScanOffset_pulse = 40000;

                //InfoPod.ePod parts = m_module.p_infoPod.p_ePod;
                //Vision.eUpDown upDown = (Vision.eUpDown)Enum.ToObject(typeof(Vision.eUpDown), m_module.p_infoPod.p_bTurn);

                //MemoryData mem = mainOpt.GetMemoryData(parts,Vision.MainOptic.eInsp.Main,upDown);

                MemoryData mem = mainOpt.GetMemoryData(InfoPod.ePod.EIP_Cover, Vision.MainOptic.eInsp.Main, Vision.eUpDown.Front);

                double dStartPosY = AlignGrabMode.m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                double dEndPosY = AlignGrabMode.m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                double dTriggerStartPosY = AlignGrabMode.m_rpAxisCenter.Y - nTotalTriggerCount / 2;
                double dTriggerEndPosY = AlignGrabMode.m_rpAxisCenter.Y + nTotalTriggerCount / 2;

                if (Math.Abs(dTriggerEndPosY - dStartPosY) > Math.Abs(dTriggerStartPosY - dStartPosY))
                    dTriggerEndPosY += nScanOffset_pulse;
                else
                    dTriggerStartPosY -= nScanOffset_pulse;

                GrabData grabData = AlignGrabMode.m_GD;
                grabData.ReverseOffsetY = AlignGrabMode.m_nReverseOffsetY;
                #endregion

                AlignGrabMode.SetLight(true);
                cpMemoryOffset.X += AlignGrabMode.m_ScanStartLine * nFOV;

                if (m_module.Run(axisZ.StartMove(AlignGrabMode.m_nFocusPosZ)))
                    return p_sInfo;
                if (m_module.Run(axisZ.WaitReady()))
                    return p_sInfo;

                while (AlignGrabMode.m_ScanLineNum > nScanLine)
                {
                    if (EQ.IsStop()) return "OK";

                    if (AlignGrabMode.m_bUseBiDirectionScan && (nScanLine % 2 != 0))
                    {
                        GeneralFunction.Swap(ref dStartPosY, ref dEndPosY);
                        AlignGrabMode.m_eGrabDirection = eGrabDirection.BackWard;
                    }
                    else
                        AlignGrabMode.m_eGrabDirection = eGrabDirection.Forward;

                    grabData.bInvY = AlignGrabMode.m_eGrabDirection == eGrabDirection.Forward;

                    double dPosX = AlignGrabMode.m_rpAxisCenter.X + nPodSizeY_px * (double)AlignGrabMode.m_dTrigger / 2 -
                        (nScanLine + AlignGrabMode.m_ScanStartLine) * nFOV * dXScale;

                    if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;

                    axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, AlignGrabMode.m_dTrigger, 5, true);
                    AlignGrabMode.StartGrab(mem, cpMemoryOffset, nPodSizeY_px, grabData);
                    AlignGrabMode.Grabed += GrabMode_Grabed;

                    if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.p_axisY.WaitReady()))
                        return p_sInfo;

                    axisXY.p_axisY.RunTrigger(false);
                    nScanLine++;
                    cpMemoryOffset.X += nFOV;
                }

                AlignGrabMode.m_camera.StopGrab();

                m_module.m_stage.Rotate(GetAlignAngle());

                return "OK";
            }
            finally
            {
                AlignGrabMode.SetLight(false);
            }
        }

        public class Result
        {
            public CPoint Pos;
            public double Score;

            public Result(CPoint Pos,double Score)
            {
                this.Pos = Pos;
                this.Score = Score;
            }
        }
        unsafe double GetAlignAngle()
        {
            EUVPositionRecipe positionRecipe = GlobalObjects.Instance.Get<RecipeVision>().GetItem<EUVPositionRecipe>();
            MemoryData mem = mainOpt.GetMemoryData(InfoPod.ePod.EIP_Cover, Vision.MainOptic.eInsp.Main, Vision.eUpDown.Front);
            byte* srcPtr = (byte*)mem.GetPtr().ToPointer();

            List<Result> resli = new List<Result>();
            foreach (RecipeType_ImageData template in positionRecipe.EIPCoverTopFeature.ListAlignFeature)
            {
                int posX = 0, posY = 0; //results
                CPoint Abspt = ConvertRelToAbs(new CPoint(template.PositionX,template.PositionY));
                double result = CLR_IP.Cpp_TemplateMatching(srcPtr, template.RawData, &posX, &posY,
                    (int)mem.W, mem.p_sz.Y, template.Width, template.Height,Abspt.X,Abspt.Y- template.Height,Abspt.X+template.Width,Abspt.Y,5,1,0);

                if(result>=nScore)
                    if (resli[0].Score < result)
                        resli.Insert(0, new Result(Abspt, result));
            }

            return CalcAngle(resli[1].Pos, resli[0].Pos);
        }

        private double CalcAngle(CPoint firstPos, CPoint secondPos)
        {
            double radian = Math.Atan2(firstPos.Y - secondPos.Y, firstPos.X-secondPos.Y);
            double angle = radian * (180 / Math.PI);
            double resAngle;
            if (secondPos.Y - firstPos.Y < 0)
                resAngle = angle + 180;
            else
                resAngle = angle - 180;

            return resAngle;
        }

        public CPoint ConvertRelToAbs(CPoint ptRel)
        {
            EUVOriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeVision>().GetItem<EUVOriginRecipe>();

            return new CPoint(originRecipe.TDIOrigin.Origin.X + ptRel.X, originRecipe.TDIOrigin.Origin.Y - originRecipe.TDIOrigin.OriginSize.Y + ptRel.Y);
        }
        private void GrabMode_Grabed(object sender, EventArgs e)
        {
            GrabedArgs ga = (GrabedArgs)e;
            m_module.p_nProgress = ga.nProgress;
        }
    }
}
