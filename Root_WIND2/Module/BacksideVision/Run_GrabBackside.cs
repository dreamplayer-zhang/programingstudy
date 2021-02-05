using System;
using System.Threading;
using RootTools;
using RootTools.Camera;
using RootTools.Control;
using RootTools.Control.Ajin;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using static RootTools.Control.Axis;

namespace Root_WIND2.Module
{
    class Run_GrabBackside : ModuleRunBase
    {
        BackSideVision m_module;

        // Member
        public GrabMode m_grabMode = null;
        public GrabMode m_LADSgrabMode = null;
        string m_sGrabMode = "";
        string m_sLADSGrabMode = "";

        public string p_sGrabMode
        {
            get { return m_sGrabMode; }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }

        public string p_sLADSGrabMode
        {
            get { return m_sLADSGrabMode; }
            set
            {
                m_sLADSGrabMode = value;
                m_LADSgrabMode = m_module.GetGrabMode(value);
            }
        }

        public Run_GrabBackside(BackSideVision module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_GrabBackside run = new Run_GrabBackside(m_module);

            run.p_sGrabMode = p_sGrabMode;
            run.p_sLADSGrabMode = p_sLADSGrabMode;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode : Backside", "Select GrabMode", bVisible);
            p_sLADSGrabMode = tree.Set(p_sLADSGrabMode, p_sLADSGrabMode, m_module.p_asGrabMode, "Grab Mode : LADS", "Select GrabMode", bVisible);
            //     if (m_grabMode != null) m_grabMode.RunTree(tree.GetTree("Grab Mode", false), bVisible, true);
        }

        public override string Run()
        {
            if (m_grabMode == null) return "Grab Mode : Backside == null";
            if (!m_grabMode.GetUseRADS())
            {
                try
                {
                    m_grabMode.SetLight(true);

                    AxisXY axisXY = m_module.AxisXY;
                    Axis axisZ = m_module.AxisZ;
                    CPoint cpMemoryOffset = new CPoint(m_grabMode.m_cpMemoryOffset);
                    int nScanLine = 0;
                    int nMMPerUM = 1000;
                    int nCamWidth = m_grabMode.m_camera.GetRoiSize().X;
                    int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;

                    double dXScale = m_grabMode.m_dResX_um * 10;
                    m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_grabMode.m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                    int nWaferSizeY_px = Convert.ToInt32(m_grabMode.m_nWaferSize_mm * nMMPerUM / m_grabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                    int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간에서 발생할 Trigger 갯수
                    int nScanOffset_pulse = 50000;

                    cpMemoryOffset.X += (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth;

                    while (m_grabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";

                        double dStartPosY = m_grabMode.m_rpAxisCenter.Y - nTotalTriggerCount / 2;
                        double dEndPosY = m_grabMode.m_rpAxisCenter.Y + nTotalTriggerCount / 2;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;
                        if (m_grabMode.m_bUseBiDirectionScan && Math.Abs(axisXY.p_axisY.p_posActual - dStartPosY) > Math.Abs(axisXY.p_axisY.p_posActual - dEndPosY))
                        {
                            double dTemp = dStartPosY;
                            dStartPosY = dEndPosY;
                            dEndPosY = dTemp;
                            dStartPosY += nScanOffset_pulse;
                            dEndPosY -= nScanOffset_pulse;
                            m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                        }
                        else
                        {
                            dStartPosY -= nScanOffset_pulse;
                            dEndPosY += nScanOffset_pulse;
                        }

                        double dPosX = m_grabMode.m_rpAxisCenter.X + nWaferSizeY_px * (double)m_grabMode.m_dTrigger / 2
                                - (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth * dXScale;

                        if (m_module.Run(axisZ.StartMove(m_grabMode.m_nFocusPosZ)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;

                        double dTriggerStartPosY = m_grabMode.m_rpAxisCenter.Y - nTotalTriggerCount / 2;
                        double dTriggerEndPosY = m_grabMode.m_rpAxisCenter.Y + nTotalTriggerCount / 2;
                        if (Math.Abs(dTriggerEndPosY - dStartPosY) > Math.Abs(dTriggerStartPosY - dStartPosY))
                        {
                            dTriggerEndPosY += nScanOffset_pulse;
                        }
                        else
                        {
                            dTriggerStartPosY -= nScanOffset_pulse;
                        }

                        axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, true);

                        string strPool = m_grabMode.m_memoryPool.p_id;
                        string strGroup = m_grabMode.m_memoryGroup.p_id;
                        string strMemory = m_grabMode.m_memoryData.p_id;

                        MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);

                        int nScanSpeed = Convert.ToInt32((double)m_grabMode.m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * m_grabMode.m_nScanRate / 100);
                        GrabData gd = new GrabData();
                        gd.bInvY = m_grabMode.m_eGrabDirection == eGrabDirection.BackWard;
                        gd.nScanOffsetY = 0;
                        gd.ReverseOffsetY = 0;

                        //m_grabMode.StartGrab(mem, cpMemoryOffset, nWaferSizeY_px, gd);
                        m_grabMode.Grabed += M_grabMode_Grabed;

                        if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        axisXY.p_axisY.RunTrigger(false);

                        nScanLine++;

                        cpMemoryOffset.X += nCamWidth;

                    }
                    //m_grabMode.m_camera.StopGrab();
                    return "OK";
                }
                finally
                {
                    m_grabMode.SetLight(false);
                }
            }
            else
            {
                if (m_grabMode == null) return "Grab Mode : Backside == null";
                if (m_LADSgrabMode == null) return "Grab Mode : LADS == null";

                try
                {
                    m_module.ladsinfos.Clear();

                    m_grabMode.SetLight(true);
                    m_LADSgrabMode.SetLight(true);

                    AxisXY axisXY = m_module.AxisXY;
                    Axis axisZ = m_module.AxisZ;
                    CPoint cpMemoryOffset = new CPoint(m_grabMode.m_cpMemoryOffset);
                    CPoint cpLADSMemoryOffset = new CPoint(m_LADSgrabMode.m_cpMemoryOffset);

                    int nScanLine = 0;
                    int nMMPerUM = 1000;
                    int nCamWidth = m_grabMode.m_camera.GetRoiSize().X;
                    int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;

                    double dXScale = m_grabMode.m_dResX_um * 10;
                    m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_grabMode.m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                    int nWaferSizeY_px = Convert.ToInt32(m_grabMode.m_nWaferSize_mm * nMMPerUM / m_grabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                    int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간에서 발생할 Trigger 갯수
                    int nScanOffset_pulse = 50000;

                    cpMemoryOffset.X += nWaferSizeY_px - (m_grabMode.m_ScanStartLine) * nCamWidth;
                    cpLADSMemoryOffset.X += Convert.ToInt32(m_LADSgrabMode.m_nWaferSize_mm * nMMPerUM / m_LADSgrabMode.m_dResY_um) - (m_grabMode.m_ScanStartLine)*m_LADSgrabMode.m_camera.GetRoiSize().X;

                    while (m_grabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";

                        double dStartPosY = m_grabMode.m_rpAxisCenter.Y - nTotalTriggerCount / 2;
                        double dEndPosY = m_grabMode.m_rpAxisCenter.Y + nTotalTriggerCount / 2;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;
                        if (m_grabMode.m_bUseBiDirectionScan && Math.Abs(axisXY.p_axisY.p_posActual - dStartPosY) > Math.Abs(axisXY.p_axisY.p_posActual - dEndPosY))
                        {
                            double dTemp = dStartPosY;
                            dStartPosY = dEndPosY;
                            dEndPosY = dTemp;
                            dStartPosY += nScanOffset_pulse;
                            dEndPosY -= nScanOffset_pulse;
                            m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                        }
                        else
                        {
                            dStartPosY -= nScanOffset_pulse;
                            dEndPosY += nScanOffset_pulse;
                        }
                        double dPosX = m_grabMode.m_rpAxisCenter.X + nWaferSizeY_px * (double)m_grabMode.m_dTrigger
                                    - (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth * dXScale ;

                        if (m_module.Run(axisZ.StartMove(m_grabMode.m_nFocusPosZ)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;

                        double dTriggerStartPosY = m_grabMode.m_rpAxisCenter.Y - nTotalTriggerCount / 2;
                        double dTriggerEndPosY = m_grabMode.m_rpAxisCenter.Y + nTotalTriggerCount / 2;
                        if (Math.Abs(dTriggerEndPosY - dStartPosY) > Math.Abs(dTriggerStartPosY - dStartPosY))
                        {
                            dTriggerEndPosY += nScanOffset_pulse;
                        }
                        else
                        {
                            dTriggerStartPosY -= nScanOffset_pulse;
                        }

                        if(nScanLine>0)
                        {
                            double dTriggerDistance = Math.Abs(dTriggerEndPosY - dTriggerStartPosY);
                            double dSection = dTriggerDistance / m_module.ladsinfos[nScanLine-1].m_Heightinfo.Length;
                            double[] darrScanAxisPos = new double[m_module.ladsinfos[nScanLine-1].m_Heightinfo.Length];
                            for (int i = 0; i < darrScanAxisPos.Length; i++)
                            {
                                if (dTriggerStartPosY > dTriggerEndPosY)
                                    darrScanAxisPos[i] = dTriggerStartPosY - (dSection * i);
                                else
                                    darrScanAxisPos[i] = dTriggerStartPosY + (dSection * i);
                            }

                            SetFocusMap(((AjinAxis)axisXY.p_axisY).m_nAxis, ((AjinAxis)axisZ).m_nAxis, darrScanAxisPos, m_module.ladsinfos[nScanLine-1].m_Heightinfo, m_module.ladsinfos[nScanLine-1].m_Heightinfo.Length, false);

                            axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, true);

                            string strPool = m_grabMode.m_memoryPool.p_id;
                            string strGroup = m_grabMode.m_memoryGroup.p_id;
                            string strMemory = m_grabMode.m_memoryData.p_id;

                            string strLADSPool = m_LADSgrabMode.m_memoryPool.p_id;
                            string strLADSGroup = m_LADSgrabMode.m_memoryGroup.p_id;
                            string strLADSMemory = m_LADSgrabMode.m_memoryData.p_id;

                            MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
                            MemoryData LADSmem = m_module.m_engineer.GetMemory(strLADSPool, strLADSGroup, strLADSMemory);

                            int nScanSpeed = Convert.ToInt32((double)m_grabMode.m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * m_grabMode.m_nScanRate / 100);
                            GrabData gd = new GrabData();
                            gd.bInvY = m_grabMode.m_eGrabDirection == eGrabDirection.BackWard;
                            gd.nScanOffsetY = 0;
                            gd.ReverseOffsetY = 0;

                            m_grabMode.StartGrab(mem, cpMemoryOffset, nWaferSizeY_px, gd);
                            m_grabMode.Grabed += M_grabMode_Grabed;

                            CAXM.AxmContiStart(((AjinAxis)axisXY.p_axisY).m_nAxis, 0, 0);
                            Thread.Sleep(10);
                            uint unRunning = 0;
                            while (true)
                            {
                                CAXM.AxmContiIsMotion(((AjinAxis)axisXY.p_axisY).m_nAxis, ref unRunning);
                                if (unRunning == 0)
                                    break;
                                Thread.Sleep(100);
                            }

                            m_LADSgrabMode.StartGrab(LADSmem, cpLADSMemoryOffset, Convert.ToInt32(m_LADSgrabMode.m_nWaferSize_mm * nMMPerUM / m_LADSgrabMode.m_dResY_um), gd);

                            if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                                return p_sInfo;
                            if (m_module.Run(axisXY.WaitReady()))
                                return p_sInfo;
                            axisXY.p_axisY.RunTrigger(false);

                            CalculateHeight(cpLADSMemoryOffset.X, LADSmem, Convert.ToInt32(m_LADSgrabMode.m_nWaferSize_mm * nMMPerUM / m_LADSgrabMode.m_dResY_um), 230);

                            nScanLine++;
                            cpMemoryOffset.X -= nCamWidth;
                            cpLADSMemoryOffset.X -= m_LADSgrabMode.m_camera.GetRoiSize().X;
                        }
                        else
                        {
                            axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, true);

                            string strLADSPool = m_LADSgrabMode.m_memoryPool.p_id;
                            string strLADSGroup = m_LADSgrabMode.m_memoryGroup.p_id;
                            string strLADSMemory = m_LADSgrabMode.m_memoryData.p_id;

                            MemoryData LADSmem = m_module.m_engineer.GetMemory(strLADSPool, strLADSGroup, strLADSMemory);

                            int nScanSpeed = Convert.ToInt32((double)m_grabMode.m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * m_grabMode.m_nScanRate / 100);
                            GrabData gd = new GrabData();
                            gd.bInvY = m_grabMode.m_eGrabDirection == eGrabDirection.BackWard;
                            gd.nScanOffsetY = 0;
                            gd.ReverseOffsetY = 0;

                            m_LADSgrabMode.StartGrab(LADSmem, cpLADSMemoryOffset, Convert.ToInt32(m_LADSgrabMode.m_nWaferSize_mm * nMMPerUM / m_LADSgrabMode.m_dResY_um), gd);

                            if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                                return p_sInfo;
                            if (m_module.Run(axisXY.WaitReady()))
                                return p_sInfo;

                            axisXY.p_axisY.RunTrigger(false);


                            CalculateHeight(cpLADSMemoryOffset.X, LADSmem, Convert.ToInt32(m_LADSgrabMode.m_nWaferSize_mm * nMMPerUM / m_LADSgrabMode.m_dResY_um), 230);
                            cpLADSMemoryOffset.X -= m_LADSgrabMode.m_camera.GetRoiSize().X;
                            nScanLine++;
                        }
                    }
                    m_grabMode.m_camera.StopGrab();
                    return "OK";
                }
                finally
                {
                    m_grabMode.SetLight(false);
                }
            }

            
        }

        private void M_grabMode_Grabed(object sender, EventArgs e)
        {
            GrabedArgs ga = (GrabedArgs)e;
            m_module.p_nProgress = ga.nProgress;
        }

        private void SetFocusMap(int nScanAxisNo, int nZAxisNo, double[] darrScanAxisPos, double[] darrZAxisPos, int nPointCount, bool bReverse)
        {
            // variable
            int iIdxScan = 0;
            int iIdxZ = 1;
            int[] narrAxisNo = new int[2];
            double[] darrPosition = new double[2];
            double dMaxVelocity = m_module.AxisXY.p_axisY.GetSpeedValue(eSpeed.Move).m_v;
            double dMaxAccel = m_module.AxisXY.p_axisY.GetSpeedValue(eSpeed.Move).m_acc;
            double dMaxDecel = m_module.AxisXY.p_axisY.GetSpeedValue(eSpeed.Move).m_dec;

            // implement
            if (nZAxisNo < nScanAxisNo)
            {
                iIdxZ = 0;
                iIdxScan = 1;
            }
            narrAxisNo[iIdxScan] = nScanAxisNo;
            narrAxisNo[iIdxZ] = nZAxisNo;

            // Min-Max 구하기
            //double dMin = 128;
            //double dMax = 0;
            //double dCenter = 64;
            //double dPixelPerPulse = 500;
            //for (int i = 0; i < darrZAxisPos.Length; i++)
            //{
            //    if (darrZAxisPos[i] < dMin) dMin = darrZAxisPos[i];
            //    if (darrZAxisPos[i] > dMax) dMax = darrZAxisPos[i];
            //}
            //dCenter = ((dMax - dMin) / 2) + dMin;

            // Queue 초기화
            uint res = CAXM.AxmContiWriteClear(nScanAxisNo);
            // 보간구동 축 맵핑
            res = CAXM.AxmContiSetAxisMap(nScanAxisNo, (uint)narrAxisNo.Length, narrAxisNo);
            // 구동모드 설정 -> [0] : 절대위치구동, [1] : 상대위치구동
            uint unAbsRelMode = 0;
            res = CAXM.AxmContiSetAbsRelMode(nScanAxisNo, unAbsRelMode);
            // Conti 작성 시작 -> AxmContiBeginNode ~ AxmContiEndNode 사이의 AXM관련 함수들이 Conti Queue에 등록된다.
            res = CAXM.AxmContiBeginNode(nScanAxisNo);

            int m_nFocusPosZ = m_grabMode.m_nFocusPosZ;

            // 축별 구동위치 등록
            for (int i = 0; i < nPointCount; i++)
            {
                darrPosition[iIdxScan] = darrScanAxisPos[i];
                darrPosition[iIdxZ] = m_nFocusPosZ + darrZAxisPos[i] * 10;/** 10((darrZAxisPos[i] - dCenter) * dPixelPerPulse) + */
                //m_module.m_axisZ.GetPosValue(eAxisPos.ScanPos);
                res = CAXM.AxmLineMove(nScanAxisNo, darrPosition, dMaxVelocity, dMaxAccel, dMaxDecel);
            }
            // Conti 작성 종료
            CAXM.AxmContiEndNode(nScanAxisNo);

            return;
        }

        unsafe void CalculateHeight(int xmempos, MemoryData mem, int WaferHeight, int gv)
        {
            int nCamWidth = m_LADSgrabMode.m_camera.GetRoiSize().X;
            int nCamHeight = m_LADSgrabMode.m_camera.GetRoiSize().Y;

            byte* ptr = (byte*)mem.GetPtr().ToPointer();

            int hCnt = WaferHeight / nCamHeight;
            BackSideVision.LADSInfo ladsinfo = new BackSideVision.LADSInfo(new RPoint(), 0, hCnt);

            for (int cnt = 0; cnt < hCnt; cnt++)
            {
                int maxLine = 0;
                int maxpixels = 0;

                for (int h = 0; h < nCamHeight; h++)
                {
                    int curpxl = 0;
                    for (int w = 0; w < nCamWidth; w++)
                    {
                        /*arr[cnt*camheight][w+(m_granCurLine*nCamWidth)]*/
                        if (ptr[w + xmempos + (cnt * nCamHeight+h) * mem.W] >= gv)
                            curpxl++;
                    }

                    if (maxpixels < curpxl)
                    {
                        maxLine = h;
                        maxpixels = curpxl;
                    }
                }

                ladsinfo.m_Heightinfo[cnt] = (double)(maxLine - nCamHeight) * Math.Sqrt(2);

            }
            m_module.ladsinfos.Add(ladsinfo);
        }
    }
}
