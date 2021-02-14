using System;
using System.Threading;
using RootTools;
using RootTools.Camera;
using RootTools.Control;
using RootTools.Control.Ajin;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using static RootTools.Control.Axis;
using System.Windows;

namespace Root_WIND2.Module
{
    class Run_GrabBackside : ModuleRunBase
    {
        // Member
        BackSideVision m_module;
        GrabMode m_MaingrabMode = null;
        GrabMode m_LADSgrabMode = null;
        string m_sGrabMode = "";
        string m_sLADSGrabMode = "";

        string p_sMainGrabMode
        {
            get { return m_sGrabMode; }
            set
            {
                m_sGrabMode = value;
                m_MaingrabMode = m_module.GetGrabMode(value);
            }
        }

        string p_sLADSGrabMode
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

            run.p_sMainGrabMode = p_sMainGrabMode;
            run.p_sLADSGrabMode = p_sLADSGrabMode;

            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sMainGrabMode = tree.Set(p_sMainGrabMode, p_sMainGrabMode, m_module.p_asGrabMode, "Grab Mode : Backside", "Select GrabMode", bVisible);
            p_sLADSGrabMode = tree.Set(p_sLADSGrabMode, p_sLADSGrabMode, m_module.p_asGrabMode, "Grab Mode : LADS", "Select GrabMode", bVisible);
            //     if (m_grabMode != null) m_grabMode.RunTree(tree.GetTree("Grab Mode", false), bVisible, true);
        }

        public override string Run()
        {
            if (m_MaingrabMode == null) return "Grab Mode : Backside == null";

            AxisXY axisXY = m_module.AxisXY;
            Axis axisZ = m_module.AxisZ;
            CPoint cpMemoryOffset = new CPoint(m_MaingrabMode.m_cpMemoryOffset);

            int nScanLine = 0;
            int nMMPerUM = 1000;
            int nCamWidth = m_MaingrabMode.m_camera.GetRoiSize().X;
            int nCamHeight = m_MaingrabMode.m_camera.GetRoiSize().Y;
            m_MaingrabMode.m_dTrigger = Convert.ToInt32(10 * m_MaingrabMode.m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
            double dXScale = m_MaingrabMode.m_dResX_um * 10; //resolution to pulse
            int nWaferSizeY_px = Convert.ToInt32(m_MaingrabMode.m_nWaferSize_mm * nMMPerUM / m_MaingrabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
            int nTotalTriggerCount = Convert.ToInt32(m_MaingrabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간의 Trigger pulse
            int nScanSpeed = Convert.ToInt32((double)m_MaingrabMode.m_nMaxFrame * m_MaingrabMode.m_dTrigger * nCamHeight * m_MaingrabMode.m_nScanRate / 100);
            int nScanOffset_pulse = 40000;

            string strPool = m_MaingrabMode.m_memoryPool.p_id;
            string strGroup = m_MaingrabMode.m_memoryGroup.p_id;
            string strMemory = m_MaingrabMode.m_memoryData.p_id;

            MemoryData Mainmem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);

            double dStartPosY = m_MaingrabMode.m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
            double dEndPosY = m_MaingrabMode.m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

            double dTriggerStartPosY = m_MaingrabMode.m_rpAxisCenter.Y - nTotalTriggerCount / 2;
            double dTriggerEndPosY = m_MaingrabMode.m_rpAxisCenter.Y + nTotalTriggerCount / 2;

            if (Math.Abs(dTriggerEndPosY - dStartPosY) > Math.Abs(dTriggerStartPosY - dStartPosY))
                dTriggerEndPosY += nScanOffset_pulse;
            else
                dTriggerStartPosY -= nScanOffset_pulse;

            GrabData grabData = m_MaingrabMode.m_GD;
            grabData.ReverseOffsetY = m_MaingrabMode.m_nReverseOffsetY;
            if (!m_MaingrabMode.m_bUseLADS) //LADS 안쓸때
            {
                try
                {
                    m_MaingrabMode.SetLight(true);

                    cpMemoryOffset.X += m_MaingrabMode.m_ScanStartLine * nCamWidth;
                    grabData.m_nSkipGrabCount = -1;

                    while (m_MaingrabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";
                        //if (m_MaingrabMode.m_bUseBiDirectionScan && (Math.Abs(axisXY.p_axisY.p_posActual - dStartPosY) > Math.Abs(axisXY.p_axisY.p_posActual - dEndPosY)))
                        if (m_MaingrabMode.m_bUseBiDirectionScan && (nScanLine % 2 != 0))
                        {
                            GeneralFunction.Swap(ref dStartPosY, ref dEndPosY);
                            m_MaingrabMode.m_eGrabDirection = eGrabDirection.BackWard;
                        }
                        else
                            m_MaingrabMode.m_eGrabDirection = eGrabDirection.Forward;

                        grabData.bInvY = m_MaingrabMode.m_eGrabDirection == eGrabDirection.Forward;

                        double dPosX = m_MaingrabMode.m_rpAxisCenter.X - nWaferSizeY_px * (double)m_MaingrabMode.m_dTrigger / 2
                            + (nScanLine + m_MaingrabMode.m_ScanStartLine) * nCamWidth * dXScale;

                        if (m_module.Run(axisZ.StartMove(m_MaingrabMode.m_nFocusPosZ)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;

                        axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_MaingrabMode.m_dTrigger,5, true);

                        m_MaingrabMode.StartGrab(Mainmem, cpMemoryOffset, nWaferSizeY_px, grabData);
                        m_MaingrabMode.Grabed += GrabMode_Grabed;

                        if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;

                        axisXY.p_axisY.RunTrigger(false);

                        nScanLine++;
                        cpMemoryOffset.X += nCamWidth;
                    }

                    m_MaingrabMode.m_camera.StopGrab();

                    return "OK";
                }
                finally
                {
                    m_MaingrabMode.SetLight(false);
                }
            }
            else //LADS 쓸 때
            {
                if (m_module.LadsInfos.Count == 0)
                {
                    MessageBox.Show("LADS 정보가 없습니다.");
                    return "Error";                
                }
                try
                {
                    m_MaingrabMode.SetLight(true);

                    cpMemoryOffset.X += m_MaingrabMode.m_ScanStartLine * nCamWidth;
                    grabData.m_nSkipGrabCount = -1;
                    //Convert.ToInt32(m_MaingrabMode.m_nWaferSize_mm * nMMPerUM / m_MaingrabMode.m_dResY_um)
                    int nLADSMaxScanNum = Convert.ToInt32((m_LADSgrabMode.m_nWaferSize_mm * nMMPerUM / m_LADSgrabMode.m_dResY_um)/m_LADSgrabMode.m_camera.GetRoiSize().X);
                    int nMainMaxScanNum = nWaferSizeY_px / m_MaingrabMode.m_camera.GetRoiSize().X;

                    while (m_MaingrabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";
                        //if (m_MaingrabMode.m_bUseBiDirectionScan && (Math.Abs(axisXY.p_axisY.p_posActual - dStartPosY) > Math.Abs(axisXY.p_axisY.p_posActual - dEndPosY)))
                        if (m_MaingrabMode.m_bUseBiDirectionScan && (nScanLine % 2 != 0))
                        {
                            GeneralFunction.Swap(ref dStartPosY, ref dEndPosY);
                            m_MaingrabMode.m_eGrabDirection = eGrabDirection.BackWard;
                        }
                        else
                            m_MaingrabMode.m_eGrabDirection = eGrabDirection.Forward;

                        grabData.bInvY = m_MaingrabMode.m_eGrabDirection == eGrabDirection.Forward;

                        double dPosX = m_MaingrabMode.m_rpAxisCenter.X - nWaferSizeY_px * (double)m_MaingrabMode.m_dTrigger / 2
                            + (nScanLine + m_MaingrabMode.m_ScanStartLine) * nCamWidth * dXScale;

                        if (m_module.Run(axisZ.StartMove(m_MaingrabMode.m_nFocusPosZ)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;

                        axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_MaingrabMode.m_dTrigger, 5, true);

                        int ladsinfonum = nLADSMaxScanNum * nScanLine / nMainMaxScanNum;
                        SetFocusMap(((AjinAxis)axisXY.p_axisY).m_nAxis, ((AjinAxis)axisZ).m_nAxis, SetScanAxisPos(nScanLine, dTriggerStartPosY, dTriggerEndPosY),
                            m_module.LadsInfos[ladsinfonum], m_module.LadsInfos[ladsinfonum].Count, false, nScanSpeed);

                        m_MaingrabMode.StartGrab(Mainmem, cpMemoryOffset, nWaferSizeY_px, grabData);
                        m_MaingrabMode.Grabed += GrabMode_Grabed;

                        string res = MoveAxisToEndPos(true, axisXY, dEndPosY, nScanSpeed);

                        if (!res.Equals("OK"))
                            return res;

                        axisXY.p_axisY.RunTrigger(false);

                        nScanLine++;
                        cpMemoryOffset.X += nCamWidth;
                    }

                    m_MaingrabMode.m_camera.StopGrab();

                    return "OK";
                }
                finally
                {
                    m_MaingrabMode.SetLight(false);
                }
            }
        }

        private void GrabMode_Grabed(object sender, EventArgs e)
        {
            GrabedArgs ga = (GrabedArgs)e;
            m_module.p_nProgress = ga.nProgress;
        }

        private double[] SetScanAxisPos(int nScanLine, double dTriggerStartPosY, double dTriggerEndPosY)
        {
            double dTriggerDistance = Math.Abs(dTriggerEndPosY - dTriggerStartPosY);
            double dSection = dTriggerDistance / m_module.LadsInfos[nScanLine].Count;
            double[] darrScanAxisPos = new double[m_module.LadsInfos[nScanLine].Count];
            for (int i = 0; i < darrScanAxisPos.Length; i++)
            {
                if (dTriggerStartPosY > dTriggerEndPosY)
                    darrScanAxisPos[i] = dTriggerStartPosY - (dSection * i);
                else
                    darrScanAxisPos[i] = dTriggerStartPosY + (dSection * i);
            }
            return darrScanAxisPos;
        }
        private string MoveAxisToEndPos(bool isCompensated, AxisXY axisXY, double dEndPosY, double nScanSpeed)
        {
            if (isCompensated)
            {
                //uint res = CAXM.AxmContiStart(((AjinAxis)axisXY.p_axisY).m_nAxis, 0, 0);
                uint res = CAXM.AxmContiStart(0, 0, 0);

                Thread.Sleep(10);
                uint unRunning = 0;
                while (true)
                {
                    //res = CAXM.AxmContiIsMotion(((AjinAxis)axisXY.p_axisY).m_nAxis, ref unRunning);
                    res = CAXM.AxmContiIsMotion(0, ref unRunning);

                    if (unRunning == 0) 
                        break;
                    Thread.Sleep(100);
                }
            }
            else
            {
                if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
            }
            return "OK";
        }
        private void SetFocusMap(int nScanAxisNo, int nZAxisNo, double[] darrScanAxisPos, List<double> darrZAxisPos, int nPointCount, bool bReverse, int speed)
        {
            int iIdxScan = 0;
            int iIdxZ = 1;
            int[] narrAxisNo = new int[2];
            double[] darrPosition = new double[2];
            double dMaxVelocity = speed;/*m_module.AxisXY.p_axisY.GetSpeedValue(eSpeed.Move).m_v;*/
            double dMaxAccel = m_module.AxisXY.p_axisY.GetSpeedValue(eSpeed.Move).m_acc;
            double dMaxDecel = m_module.AxisXY.p_axisY.GetSpeedValue(eSpeed.Move).m_dec;

            //축번호가 작은게 앞으로 와야됨
            if (nZAxisNo < nScanAxisNo)
            {
                iIdxZ = 0;
                iIdxScan = 1;
            }

            narrAxisNo[iIdxScan] = nScanAxisNo;
            narrAxisNo[iIdxZ] = nZAxisNo;

            nScanAxisNo = 0;
            // Queue 초기화
            uint res = CAXM.AxmContiWriteClear(0);

            // 보간구동 축 맵핑
            res = CAXM.AxmContiSetAxisMap(0, (uint)2, narrAxisNo);
            // 구동모드 설정 -> [0] : 절대위치구동, [1] : 상대위치구동
            uint unAbsRelMode = 0;
            res = CAXM.AxmContiSetAbsRelMode(0, unAbsRelMode);
            // Conti 작성 시작 -> AxmContiBeginNode ~ AxmContiEndNode 사이의 AXM관련 함수들이 Conti Queue에 등록된다.
            res = CAXM.AxmContiBeginNode(0);

            int m_nFocusPosZ = m_MaingrabMode.m_nFocusPosZ;

            // 축별 구동위치 등록
            if (bReverse)
            {
                for (int i = nPointCount - 1; i >= 0; i--)
                {
                    darrPosition[iIdxScan] = darrScanAxisPos[i];
                    darrPosition[iIdxZ] = m_nFocusPosZ- darrZAxisPos[i] * 400;/* 10*m_LADSgrabMode.m_dResX_um*/;
                    res = CAXM.AxmLineMove(nScanAxisNo, darrPosition, dMaxVelocity, dMaxAccel, dMaxDecel);
                }
            }
            else
            {
                for (int i = 0; i < nPointCount; i++)
                {
                    darrPosition[iIdxScan] = darrScanAxisPos[i];
                    darrPosition[iIdxZ] = m_nFocusPosZ - darrZAxisPos[i] * 400; /*10*m_LADSgrabMode.m_dResX_um*/;
                    res = CAXM.AxmLineMove(nScanAxisNo, darrPosition, dMaxVelocity, dMaxAccel, dMaxDecel);
                }
            }

            // Conti 작성 종료
            CAXM.AxmContiEndNode(nScanAxisNo);
        }
        unsafe void CalculateHeight(int xmempos, MemoryData mem, int WaferHeight, int gv,bool InvY)
        {
            int nCamWidth = m_LADSgrabMode.m_camera.GetRoiSize().X;
            int nCamHeight = m_LADSgrabMode.m_camera.GetRoiSize().Y;
            int hCnt = WaferHeight / nCamHeight;

            byte* ptr = (byte*)mem.GetPtr().ToPointer();

            List<double> ladsinfo = new List<double>();
            List<double> li = new List<double>();// 뭔가 값이 있는곳에 대한 정보

            if(!InvY)
            {//정방향 스캔시
                for (int cnt = 0; cnt < hCnt; cnt++)
                {
                    li.Clear();

                    for (int h = (int)(nCamHeight*0.25); h < nCamHeight*0.75; h++)
                    {
                        int curpxl = 0;
                        for (int w = 0; w < nCamWidth; w++)
                        {
                            /*arr[cnt*camheight+h][w+(m_granCurLine*nCamWidth)]*/
                            if (ptr[w + xmempos + (cnt * nCamHeight + h) * mem.W] >= gv)
                                curpxl++;
                        }

                        if (curpxl > 0)
                            li.Add(h);
                    }

                    if (li.Count > 0)
                        ladsinfo.Add(((li[li.Count - 1] + li[0]) / 2 - (double)nCamHeight / 2) * Math.Sqrt(2)); //Frame의 가운데가 Focus가 맞는 지점이라고 생각
                    else
                        ladsinfo.Add(0);
                }
            }
            else
            {
                for(int cnt = hCnt-1; cnt >= 0; cnt--)
                {
                    li.Clear();

                    for (int h = 0; h < nCamHeight; h++)
                    {
                        int curpxl = 0;
                        for (int w = 0; w < nCamWidth; w++)
                        {
                            /*arr[cnt*camheight+h][w+(m_granCurLine*nCamWidth)]*/
                            if (ptr[w + xmempos + (cnt * nCamHeight + h) * mem.W] >= gv)
                                curpxl++;
                        }

                        if (curpxl > 0)
                            li.Add(h);
                    }

                    if (li.Count > 0)
                        ladsinfo.Add(((li[li.Count - 1] + li[0]) / 2 - (double)nCamHeight / 2) * Math.Sqrt(2)); //Frame의 가운데가 Focus가 맞는 지점이라고 생각
                    else
                        ladsinfo.Add(0);
                }
            }

            m_module.LadsInfos.Add(ladsinfo);
        }
    }
}
#region 한줄씩찍으면서 LADS 정보획득
/*if (m_LADSgrabMode == null) return "Grab Mode : LADS == null";

try
{
    ladsinfos.Clear();

    m_MaingrabMode.SetLight(true);

    CPoint cpLADSMemoryOffset = new CPoint(m_LADSgrabMode.m_cpMemoryOffset);

    int nLADSCamWidth = m_LADSgrabMode.m_camera.GetRoiSize().X;
    int nLADSCamHeight = m_LADSgrabMode.m_camera.GetRoiSize().Y;

    cpMemoryOffset.X += nWaferSizeY_px - m_MaingrabMode.m_ScanStartLine * nCamWidth;
    cpLADSMemoryOffset.X += Convert.ToInt32(m_MaingrabMode.m_nWaferSize_mm * nMMPerUM / m_LADSgrabMode.m_dResY_um)
        - m_MaingrabMode.m_ScanStartLine * nLADSCamWidth;

    string strLADSPool = m_LADSgrabMode.m_memoryPool.p_id;
    string strLADSGroup = m_LADSgrabMode.m_memoryGroup.p_id;
    string strLADSMemory = m_LADSgrabMode.m_memoryData.p_id;

    MemoryData LADSmem = m_module.m_engineer.GetMemory(strLADSPool, strLADSGroup, strLADSMemory);

    while (m_MaingrabMode.m_ScanLineNum >= nScanLine)
    {
        if (EQ.IsStop())
            return "OK";

        if (m_MaingrabMode.m_bUseBiDirectionScan && (nScanLine % 2 != 0))
        {
            GeneralFunction.Swap(ref dStartPosY, ref dEndPosY);
            m_MaingrabMode.m_eGrabDirection = eGrabDirection.BackWard;
        }
        else
            m_MaingrabMode.m_eGrabDirection = eGrabDirection.Forward;

        grabData.bInvY = m_MaingrabMode.m_eGrabDirection == eGrabDirection.Forward;

        double dPosX = m_MaingrabMode.m_rpAxisCenter.X + nWaferSizeY_px * (double)m_MaingrabMode.m_dTrigger/2
                    - (nScanLine + m_MaingrabMode.m_ScanStartLine) * nCamWidth * dXScale;

        if (m_module.Run(axisZ.StartMove(m_MaingrabMode.m_nFocusPosZ)))
            return p_sInfo;
        if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
            return p_sInfo;
        if (m_module.Run(axisXY.WaitReady()))
            return p_sInfo;
        if (m_module.Run(axisZ.WaitReady()))
            return p_sInfo;


        if (nScanLine > 0)
        {
            SetFocusMap(((AjinAxis)axisXY.p_axisY).m_nAxis, ((AjinAxis)axisZ).m_nAxis, SetScanAxisPos(nScanLine, dTriggerStartPosY, dTriggerEndPosY),
            ladsinfos[nScanLine - 1], ladsinfos[nScanLine - 1].Count, false, nScanSpeed);
            m_MaingrabMode.StartGrab(Mainmem, cpMemoryOffset, nWaferSizeY_px, m_MaingrabMode.m_GD);
            m_MaingrabMode.Grabed += GrabMode_Grabed;
            cpMemoryOffset.X -= nCamWidth;
        }
        axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_MaingrabMode.m_dTrigger, 5, true);
        //m_LADSgrabMode.StartGrab(LADSmem, cpLADSMemoryOffset, Convert.ToInt32(m_LADSgrabMode.m_nWaferSize_mm * nMMPerUM / m_LADSgrabMode.m_dResY_um), grabData);
        m_LADSgrabMode.StartGrab(LADSmem, cpMemoryOffset,nWaferSizeY_px, grabData);
        string res = MoveAxisToEndPos(nScanLine > 0, axisXY, dEndPosY, nScanSpeed);

        if (!res.Equals("OK"))
            return res;

        axisXY.p_axisY.RunTrigger(false);

        CalculateHeight(cpLADSMemoryOffset.X, LADSmem, Convert.ToInt32(m_LADSgrabMode.m_nWaferSize_mm * nMMPerUM / m_LADSgrabMode.m_dResY_um), 230, m_MaingrabMode.m_bUseBiDirectionScan && (nScanLine % 2 != 0));

        nScanLine++;
        cpLADSMemoryOffset.X -= nLADSCamWidth;
    }

    m_MaingrabMode.m_camera.StopGrab();

    return "OK";
}
finally
{
    m_MaingrabMode.SetLight(false);
}*/
#endregion