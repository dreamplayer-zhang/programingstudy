using Root_EFEM.Module;
using Root_VEGA_D.Engineer;
using RootTools;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Root_VEGA_D.Module
{
    public class Run_PM : ModuleRunBase
    {
        Vision m_module;

        bool m_bIsRun = true;
        public GrabMode m_grabMode = null;
        string m_sGrabMode = "";
        double m_dScanDistance = 0;
        double m_dLengthFromScanCenterY = 0;
        int m_nCheckArea = 0;
        double m_dCoaxialZPos = 0;
        double m_dTransmittedZPos = 0;
        int m_nUSL = 0;
        int m_nLSL = 0;
        string m_sMemoryGroup = "";
        string m_sMemoryData = "";
        VEGA_D_Handler m_handler;

        List<LightData> m_lCoaxialLightData = new List<LightData>();
        List<LightData> m_lTransmittedLightData = new List<LightData>();

        int m_nCoaxialLightCount = 0;
        int m_nTransmittedLightCount = 0;

        public string p_sGrabMode
        {
            get { return m_sGrabMode; }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }
        public Run_PM(Vision module, VEGA_D_Handler handler)
        {
            m_module = module;
            m_handler = handler;
            InitModuleRun(module);
        }
        public override ModuleRunBase Clone()
        {
            Run_PM run = new Run_PM(m_module,m_handler);

            run.m_bIsRun = m_bIsRun;
            run.p_sGrabMode = p_sGrabMode;
            run.m_dScanDistance = m_dScanDistance;
            run.m_dLengthFromScanCenterY = m_dLengthFromScanCenterY;
            run.m_nCheckArea = m_nCheckArea;
            run.m_dCoaxialZPos = m_dCoaxialZPos;
            run.m_dTransmittedZPos = m_dTransmittedZPos;
            run.m_nUSL = m_nUSL;
            run.m_nLSL = m_nLSL;
            run.m_sMemoryGroup = m_sMemoryGroup;
            run.m_sMemoryData = m_sMemoryData;
            run.m_lCoaxialLightData = m_lCoaxialLightData;
            run.m_lTransmittedLightData = m_lTransmittedLightData;
            run.m_nCoaxialLightCount = m_nCoaxialLightCount;
            run.m_nTransmittedLightCount = m_nTransmittedLightCount;

            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_bIsRun = tree.Set(m_bIsRun, m_bIsRun, "Run PM", "Run PM", bVisible);
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
            m_dScanDistance = tree.Set(m_dScanDistance, m_dScanDistance, "Scan Distance", "Scan Distance on Y Axis (mm)", bVisible);
            m_dLengthFromScanCenterY = tree.Set(m_dLengthFromScanCenterY, m_dLengthFromScanCenterY, "Length From Center Y", "Length from ScanCenterY for Check Area Center (mm)", bVisible);
            m_nCheckArea = tree.Set(m_nCheckArea, m_nCheckArea, "Check Area", "Check area length of width or height (px)", bVisible);
            m_nUSL = tree.Set(m_nUSL, m_nUSL, "USL", "Upper Specification Limit", bVisible);
            m_nLSL = tree.Set(m_nLSL, m_nLSL, "LSL", "Lower Specification Limit", bVisible);
            m_sMemoryGroup = tree.Set(m_sMemoryGroup, m_sMemoryGroup, m_module.MemoryPool.m_asGroup, "MemoryGroup", "Memory Group Name", bVisible);
            MemoryGroup memoryGroup = m_module.MemoryPool.GetGroup(m_sMemoryGroup);
            if (memoryGroup != null) m_sMemoryData = tree.Set(m_sMemoryData, m_sMemoryData, memoryGroup.m_asMemory, "MemoryData", "Memory Data Name", bVisible);

            RunTreeLight(tree.GetTree("Coaxial", false, bVisible), bVisible, m_lCoaxialLightData, "Coaxial", ref m_dCoaxialZPos, ref m_nCoaxialLightCount);
            RunTreeLight(tree.GetTree("Transmitted", false, bVisible), bVisible, m_lTransmittedLightData, "Transmitted", ref m_dTransmittedZPos, ref m_nTransmittedLightCount);
        }

        class LightData
        {
            public string m_sLight = "";
            public int m_nLightPower;
        }

        void RunTreeLight(Tree tree, bool bVisible, List<LightData> lLightData, string strLightName, ref double dPosZ, ref int nRefCount)
        {
            dPosZ = tree.Set(dPosZ, dPosZ, "Z Pos", "Z Pos", bVisible);
            nRefCount = tree.Set(nRefCount, nRefCount, "Count", "Light Count", bVisible);

            while(lLightData.Count < nRefCount)
            {
                LightData data = new LightData();
                lLightData.Add(data);
            }

            while(lLightData.Count > nRefCount)
            {
                lLightData.RemoveAt(lLightData.Count - 1);
            }

            for (int i = 0; i < nRefCount; i++)
            {
                LightData data = lLightData[i];

                Tree treeLight = tree.GetTree(strLightName + " " + i, false, bVisible);
                data.m_sLight = treeLight.Set(data.m_sLight, data.m_sLight, m_module.p_asLightSet, "Light", "Light", bVisible);
                data.m_nLightPower = treeLight.Set(data.m_nLightPower, data.m_nLightPower, "Power", "Power", bVisible);
            }
        }

        class PMData
        {
            public List<LightData> m_lLightData = new List<LightData>();
            public CRect m_rectCheckArea = null;
            public int m_average = 0;
            public double m_posZ = 0;

            public PMData(List<LightData> lLightData, CRect rectArea, double posZ)
            {
                m_lLightData = lLightData;
                m_rectCheckArea = rectArea;
                m_posZ = posZ;
            }
        }

        void WritePMLog(bool bCoaxialResult, bool bTransmittedResult, int nCoaxialAvg, int nTransmittedAvg)
        {
            // Create Directory, Log File
            DateTime dt = DateTime.Now;
            string sPath = LogView._logView.p_sPath;

            Directory.CreateDirectory(sPath + "\\PM");

            string strFile = sPath + "\\PM" + "\\" + dt.ToShortDateString() + ".txt";
            bool bIsLogExist = File.Exists(strFile);
            using (StreamWriter writer = new StreamWriter(strFile, true, Encoding.Default))
            {
                if(!bIsLogExist)
                {
                    // 파일 첫 줄 작성 시 헤더 작성
                    writer.WriteLine("Time,PM_Success,Coaxial_Result,Transmitted_Result,Coaxial_Avg,Tranmitted_Avg,USL,LSL");
                }

                string strTime = dt.Hour.ToString("00") + '.' + dt.Minute.ToString("00") + '.' + dt.Second.ToString("00") + '.' + dt.Millisecond.ToString("000");
                string strPMSuccess = (bCoaxialResult && bTransmittedResult).ToString();
                string strCoaxialResult = bCoaxialResult.ToString();
                string strTransmittedResult = bTransmittedResult.ToString();
                string strCoaxialAvg = nCoaxialAvg.ToString();
                string strTransmittedAvg = nTransmittedAvg.ToString();
                string strUSL = m_nUSL.ToString();
                string strLSL = m_nLSL.ToString();

                writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7}", strTime, strPMSuccess, strCoaxialResult, strTransmittedResult, strCoaxialAvg, strTransmittedAvg, strUSL, strLSL);
            }
        }

        public override string Run()
        {
            // 동축, 투과조명 시료 PM 기능 결과
            bool bCoaxialResult = false;
            bool bTransmittedResult = false;

            if (!m_bIsRun)
            {
                ((Loadport_Cymechs)m_handler.m_loadport[EQ.p_nRunLP]).m_CommonFunction();

                return "OK";
            }

            // Grabmode
            if (m_grabMode == null) return "Grab Mode == null";

            // Memory
            MemoryData mem = m_module.MemoryPool.GetMemory(m_sMemoryGroup, m_sMemoryData);
            if (mem == null) return "Set Memory Setting";

            // USL & LSL Check
            if (m_nLSL > m_nUSL) return "Check USL & LSL setting";

            try
            {
                // Position
                AxisXY axisXY = m_module.AxisXY;
                Axis.Speed speedY = axisXY.p_axisY.GetSpeedValue(Axis.eSpeed.Move);

                double dPosX = m_grabMode.m_rpAxisCenter.X + m_grabMode.m_GD.m_nFovSize * m_grabMode.m_dResX_um * 0.001 * 0.5;

                double accDistance = speedY.m_acc * speedY.m_v * 0.5 * 2.0;
                double decDistance = speedY.m_dec * speedY.m_v * 0.5 * 2.0;

                double dStartTriggerY = m_grabMode.m_rpAxisCenter.Y - m_dScanDistance * 0.5;
                double dEndTriggerY = m_grabMode.m_rpAxisCenter.Y + m_dScanDistance * 0.5;

                double dStartPosY = dStartTriggerY - accDistance;
                double dEndPosY = dEndTriggerY + decDistance;

                int centerY_px = (int)(m_dScanDistance * 1000 * 0.5 / m_grabMode.m_dResY_um);

                int nScanLen_px = (int)Math.Round(m_dScanDistance * 1000 / m_grabMode.m_dResY_um);

                m_grabMode.m_dTrigger = Math.Round(m_grabMode.m_dResY_um * m_grabMode.m_dCamTriggerRatio, 1);

                // Make PM Data List
                int nCheckPointLenFromCenter_px = (int)(m_dLengthFromScanCenterY * 1000 / m_grabMode.m_dResY_um);
                int nCoaxialCheckPosY_px = centerY_px - nCheckPointLenFromCenter_px;
                int nTransmittedCheckPosY_px = centerY_px + nCheckPointLenFromCenter_px;

                CRect rectCoaxial = new CRect((int)(m_grabMode.m_GD.m_nFovSize * 0.5), nCoaxialCheckPosY_px, m_nCheckArea);
                CRect rectTransmitted = new CRect((int)(m_grabMode.m_GD.m_nFovSize * 0.5), nTransmittedCheckPosY_px, m_nCheckArea);

                List<PMData> listPMData = new List<PMData>();
                listPMData.Add(new PMData(m_lCoaxialLightData, rectCoaxial, m_dCoaxialZPos));
                listPMData.Add(new PMData(m_lTransmittedLightData, rectTransmitted, m_dTransmittedZPos));

                // RADS 연결
                if (m_module.Run(m_module.StartRADS()))
                    return p_sInfo;

                // Collect GV Value
                foreach (PMData pmData in listPMData)
                {
                    // Turn off lights
                    m_grabMode.SetLight(false);

                    // Turn on light
                    foreach(LightData data in pmData.m_lLightData)
                    {
                        Light light = m_module.GetLight(data.m_sLight);
                        if (light == null)
                            return "Check Light Setting";

                        light.p_fPower = data.m_nLightPower;
                    }

                    // 포커스 높이로 Z축 이동
                    Axis axisZ = m_module.AxisZ;
                    if (m_module.Run(axisZ.StartMove(pmData.m_posZ)))
                        return p_sInfo;

                    // 이동 대기
                    if (m_module.Run(axisZ.WaitReady()))
                        return p_sInfo;

                    // Scan Target Image
                    if (m_module.Run(m_module.RunLineScan(m_grabMode, mem, new RootTools.CPoint(0, 0), nScanLen_px, dPosX, dStartPosY, dEndPosY, dStartTriggerY, dEndTriggerY)))
                        return p_sInfo;

                    // Sum GV
                    long sumGV = 0;

                    IntPtr ptr = mem.GetPtr(0);
                    for (int y = pmData.m_rectCheckArea.Top; y < pmData.m_rectCheckArea.Bottom; y++)
                    {
                        for (int x = pmData.m_rectCheckArea.Left; x < pmData.m_rectCheckArea.Right; x++)
                        {
                            unsafe
                            {
                                byte* arrData = (byte*)ptr.ToPointer();
                                for (int i = 0; i < mem.p_nByte; i++)
                                {
                                    int val = arrData[((long)mem.p_sz.X * y + x) * mem.p_nByte + i];
                                    long shiftedVal = val << (i * 8);

                                    sumGV += shiftedVal;
                                }
                            }
                        }
                    }

                    // Get Average GV
                    int nAverageGV = (int)(sumGV / (m_nCheckArea * m_nCheckArea));
                    pmData.m_average = nAverageGV;
                }


                // PM check result
                bCoaxialResult = listPMData[0].m_average <= m_nUSL && listPMData[0].m_average >= m_nLSL;
                bTransmittedResult = listPMData[1].m_average <= m_nUSL && listPMData[1].m_average >= m_nLSL;

                m_log.Info(string.Format("Coaxial Light PM result : {0} (Average = {1})", bCoaxialResult ? "OK" : "Fail", listPMData[0].m_average));
                m_log.Info(string.Format("Transmitted Light PM result : {0} (Average = {1})", bTransmittedResult ? "OK" : "Fail", listPMData[1].m_average));

                WritePMLog(bCoaxialResult, bTransmittedResult, listPMData[0].m_average, listPMData[1].m_average);

                // Alarm
                m_module.m_alidPMCoaxialError.Run(!bCoaxialResult, m_module.m_alidPMCoaxialError.p_sDesc);
                m_module.m_alidPMTransmittedError.Run(!bTransmittedResult, m_module.m_alidPMTransmittedError.p_sDesc);
            }
            catch (Exception e)
            {
                m_log.Info(e.Message);
            }
            finally
            {
                // Turn off light
                m_grabMode.SetLight(false);

                // RADS 기능 off
                m_module.StopRADS();

                // PM 기능 이후 loadport 제어
                if (bCoaxialResult && bTransmittedResult)
                {
                    m_log.Info("PM Success");
                    ((Loadport_Cymechs)m_handler.m_loadport[EQ.p_nRunLP]).m_CommonFunction();
                }
                else
                {
                    m_log.Info("PM Failed");
                    m_module.m_alidPMFail.Run(true, "PM Failed");
                }
            }
            if (bCoaxialResult && bTransmittedResult)
                return "OK";
            else
                return "PM Failed";
        }
    }
}
