using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using RootTools;
using RootTools.Camera;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;

namespace Root_EFEM.Module.BacksideVision
{
    class Run_LADS : ModuleRunBase
    {
        // Member
        Vision_Backside m_module;
        GrabModeBack m_grabMode = null;
        string m_sGrabMode = "";

        public string p_sGrabMode
        {
            get
            {
                return m_sGrabMode;
            }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }

        public Run_LADS(Vision_Backside module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_LADS run = new Run_LADS(m_module);
            run.p_sGrabMode = p_sGrabMode;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode : LADS", "Select GrabMode", bVisible);
            //if (m_grabMode != null) m_grabMode.RunTree(tree.GetTree("Grab Mode", false), bVisible, true);
        }

        public override string Run()
        {
            if (m_grabMode == null)
                return "Grab Mode == null";

            try
            {
                m_grabMode.SetLight(true);

                #region Local Variable
                AxisXY axisXY = m_module.AxisXY;
                Axis axisZ = m_module.AxisZ;
                CPoint cpMemoryOffset = new CPoint(m_grabMode.m_cpMemoryOffset);
                int nScanLine = 0;
                int nMMPerUM = 1000;
                int nCamWidth = m_grabMode.m_camera.GetRoiSize().X;
                int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;

                double dXScale = m_grabMode.m_dTargetResX_um * 10;
                cpMemoryOffset.X += (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth;
                m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_grabMode.m_dTargetResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                int nWaferSizeY_px = Convert.ToInt32(m_grabMode.m_nWaferSize_mm * nMMPerUM / m_grabMode.m_dTargetResY_um);  // 웨이퍼 영역의 Y픽셀 갯수 
                int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간에서 발생할 Trigger 갯수
                int nScanOffset_pulse = 10000;
                string strPool = m_grabMode.m_memoryPool.p_id;
                string strGroup = m_grabMode.m_memoryGroup.p_id;
                string strMemory = m_grabMode.m_memoryData.p_id;

                MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
                int nScanSpeed = Convert.ToInt32((double)m_grabMode.m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * m_grabMode.m_nScanRate / 100);

                double dStartPosY = m_grabMode.m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                double dEndPosY = m_grabMode.m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                double dTriggerStartPosY = m_grabMode.m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                double dTriggerEndPosY = m_grabMode.m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;
                
                if (Math.Abs(dTriggerEndPosY - dStartPosY) > Math.Abs(dTriggerStartPosY - dStartPosY))
                    dTriggerEndPosY += nScanOffset_pulse;
                else
                    dTriggerStartPosY -= nScanOffset_pulse;

                GrabData grabData = m_grabMode.m_GD;
                grabData.ReverseOffsetY = m_grabMode.m_nReverseOffsetY;

                #endregion

                if(m_module.LadsInfos.Count ==0)
                {  //lads 초기 생성시에 dummy 생성
                    int lisize = nWaferSizeY_px / nCamWidth;
                    for (int i = 0; i < lisize; i++)
                        m_module.LadsInfos.Add(new List<double>());
                }

                while (m_grabMode.m_ScanLineNum > nScanLine)
                {
                    if (EQ.IsStop())
                        return "OK";                    

                    double dPosX = m_grabMode.m_rpAxisCenter.X - nWaferSizeY_px * (double)m_grabMode.m_dTrigger / 2
                        + (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth * dXScale;

                    if (m_grabMode.m_bUseBiDirectionScan && (nScanLine % 2 != 0))
                    {
                        GeneralFunction.Swap(ref dStartPosY, ref dEndPosY);
                        m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                    }
                    else
                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;

                    grabData.bInvY = m_grabMode.m_eGrabDirection == eGrabDirection.Forward;

                    if (m_module.Run(axisZ.StartMove(m_grabMode.m_nFocusPosZ)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
                    if (m_module.Run(axisZ.WaitReady()))
                        return p_sInfo;

                    axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger*nCamHeight /2,5, true);

                    m_grabMode.StartGrab(mem, cpMemoryOffset, nWaferSizeY_px, grabData);

                    if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;

                    axisXY.p_axisY.RunTrigger(false);

                    CalculateHeight(cpMemoryOffset.X, mem, nWaferSizeY_px, 20,nScanLine+m_grabMode.m_ScanStartLine,grabData.bInvY);
                    nScanLine++;
                    cpMemoryOffset.X += nCamWidth;
                }
                m_grabMode.m_camera.StopGrab();

                if(nScanLine == (nWaferSizeY_px / nCamWidth))
                    CreateFocusMap();

                //앞뒤 0인애들 고치는거
                for(int i=0;i<m_module.LadsInfos.Count;i++)
                {
                    List<double> li = m_module.LadsInfos[i];
                    for(int y=0;y<li.Count;y++)
                    {
                        if (li[y] == 0)
                            continue;

                        for (int yy = 0; yy < y; yy++)
                            li[yy] = li[y];

                        break;
                    }

                    for(int y = li.Count-1;y>=0;y--)
                    {
                        if (li[y] == 0)
                            continue;

                        for (int yy = li.Count - 1; yy > y; yy--)
                            li[yy] = li[y];

                        break;
                    }
                }

                return "OK";
            }
            finally
            {
                m_grabMode.SetLight(false);
            }
        }

        unsafe void CalculateHeight(int xmempos, MemoryData mem, int WaferHeight, int gv, int nScanNum,bool InvY)
        {
            int nCamWidth = m_grabMode.m_camera.GetRoiSize().X;
            int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;
            int hCnt = WaferHeight / nCamHeight;

            int s = (int)(nCamHeight * 0.25);
            byte* ptr = (byte*)mem.GetPtr().ToPointer();
            List<double> ladsinfo = new List<double>();
            List<double> profile = new List<double>();

            for (int cnt = 0; cnt < hCnt; cnt++)
            {
                profile.Clear();

                for (int h = (int)(nCamHeight * 0.25); h < nCamHeight * 0.75; h++)
                {
                    double sum = 0;
                    for (int w = 0; w < nCamWidth; w++)
                    {
                        //ptr[cnt*nCamHeight+h][w+xmempos]
                        int curgv = ptr[w + xmempos + (cnt * nCamHeight + h) * mem.W];

                        if (curgv >= gv)
                            sum += curgv;
                    }

                    profile.Add(sum / nCamWidth);
                }

                double M = double.MinValue;
                double res = 0;
                Parallel.For (1, profile.Count - 1, (i) => 
                {
                    if (M < profile[i] && profile[i] != 0)
                    {
                        M = profile[i];
                        res = (profile[i - 1] * (i + s - 1) + profile[i] * (i + s) + profile[i + 1] * (i + s + 1)) / (profile[i - 1] + profile[i] + profile[i + 1]);
                    }
                }) ;

                if (res != 0)
                    ladsinfo.Add((res - nCamHeight/2));
                else
                    ladsinfo.Add(0);
            }

            if (InvY)
                ladsinfo.Reverse();

            m_module.LadsInfos.RemoveAt(nScanNum);
            m_module.LadsInfos.Insert(nScanNum, ladsinfo);
        }

        unsafe void CreateFocusMap()
        {
            Mat ResultMat = new Mat();

            int nX = m_module.LadsInfos.Count;
            int nY = m_module.LadsInfos[0].Count;
            int thumsize = 30;

            double mHeight = double.MaxValue, MHeight = double.MinValue;
            for (int x = 0; x < nX; x++)
            {
                mHeight = Math.Min(mHeight, m_module.LadsInfos[x].Min());
                MHeight = Math.Max(MHeight, m_module.LadsInfos[x].Max());
            }
            double rate = 255 / (MHeight - mHeight);

            for (int x = 0; x < nX; x++)
            {
                Mat Vmat = new Mat();

                for (int y = 0; y < nY; y++)
                {
                    Mat ColorImg = new Mat(thumsize, (thumsize * nY / nX), DepthType.Cv8U, 1);
                    MCvScalar color;
                    if (m_module.LadsInfos[x][y] == 0)
                        color = new MCvScalar(255);
                    else
                        color = new MCvScalar(rate * (m_module.LadsInfos[x][y] + Math.Abs(mHeight)));
                    ColorImg.SetTo(color);

                    if (y == 0)
                        Vmat = ColorImg;
                    else
                        CvInvoke.VConcat(ColorImg, Vmat, Vmat);
                }

                if (x == 0)
                    ResultMat = Vmat;
                else
                    CvInvoke.HConcat(ResultMat, Vmat, ResultMat);
            }

            GeneralFunction.WriteINIFile("LADS", "Count", m_module.LadsInfos.Count.ToString(), @"C:\WIND2\Init\LADSInfo.ini");

            for (int i = 0; i < m_module.LadsInfos.Count; i++)
            {
                GeneralFunction.WriteINIFile("LADS", "Info"+i.ToString("00"), m_module.LadsInfos[i].ToString(), @"C:\WIND2\Init\LADSInfo.ini");
            }

            CvInvoke.ApplyColorMap(ResultMat, ResultMat, ColorMapType.Ocean);
            CvInvoke.Imwrite(@"D:\FocusMap.bmp", ResultMat);
        }
    }
}
