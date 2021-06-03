using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Root_VEGA_D.Engineer;
using Root_EFEM.Module;

namespace Root_VEGA_D.Module
{
    public class Run_PM : ModuleRunBase
    {
        Vision m_module;

        public GrabMode m_grabMode = null;
        string m_sGrabMode = "";
        double m_dScanDistance = 0;
        double m_dLengthFromScanCenterY = 0;
        int m_nCheckArea = 0;
        string m_sCoaxialLight = "";
        string m_sTransmittedLight = "";
        int m_nCoaxialLightPower = 0;
        int m_nTransmittedLightPower = 0;
        double m_dCoaxialZPos = 0;
        double m_dTransmittedZPos = 0;
        int m_nUSL = 0;
        int m_nLSL = 0;
        string m_sMemoryGroup = "";
        string m_sMemoryData = "";
        VEGA_D_Handler m_handler;

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

            run.p_sGrabMode = p_sGrabMode;
            run.m_dScanDistance = m_dScanDistance;
            run.m_dLengthFromScanCenterY = m_dLengthFromScanCenterY;
            run.m_nCheckArea = m_nCheckArea;
            run.m_sCoaxialLight = m_sCoaxialLight;
            run.m_nCoaxialLightPower = m_nCoaxialLightPower;
            run.m_sTransmittedLight = m_sTransmittedLight;
            run.m_nTransmittedLightPower = m_nTransmittedLightPower;
            run.m_dCoaxialZPos = m_dCoaxialZPos;
            run.m_dTransmittedZPos = m_dTransmittedZPos;
            run.m_nUSL = m_nUSL;
            run.m_nLSL = m_nLSL;
            run.m_sMemoryGroup = m_sMemoryGroup;
            run.m_sMemoryData = m_sMemoryData;

            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
            m_dScanDistance = tree.Set(m_dScanDistance, m_dScanDistance, "Scan Distance", "Scan Distance on Y Axis (mm)", bVisible);
            m_dLengthFromScanCenterY = tree.Set(m_dLengthFromScanCenterY, m_dLengthFromScanCenterY, "Length From Center Y", "Length from ScanCenterY for Check Area Center (mm)", bVisible);
            m_nCheckArea = tree.Set(m_nCheckArea, m_nCheckArea, "Check Area", "Check area length of width or height (px)", bVisible);
            m_sCoaxialLight = tree.Set(m_sCoaxialLight, m_sCoaxialLight, m_module.p_asLightSet, "Coaxial Light", "Coaxial Light for PM", bVisible);
            m_nCoaxialLightPower = tree.Set(m_nCoaxialLightPower, m_nCoaxialLightPower, "Coaxial Light Power", "Coaxial Light Power", bVisible);
            m_dCoaxialZPos = tree.Set(m_dCoaxialZPos, m_dCoaxialZPos, "Coaxial Z Pos", "Coaxial Z Pos", bVisible);
            m_sTransmittedLight = tree.Set(m_sTransmittedLight, m_sTransmittedLight, m_module.p_asLightSet, "Transmitted Light", "Transmitted Light for PM", bVisible);
            m_nTransmittedLightPower = tree.Set(m_nTransmittedLightPower, m_nTransmittedLightPower, "Transmitted Light Power", "Transmitted Light Power", bVisible);
            m_dTransmittedZPos = tree.Set(m_dTransmittedZPos, m_dTransmittedZPos, "Transmitted Z Pos", "Transmitted Z Pos", bVisible);
            m_nUSL = tree.Set(m_nUSL, m_nUSL, "USL", "Upper Specification Limit", bVisible);
            m_nLSL = tree.Set(m_nLSL, m_nLSL, "LSL", "Lower Specification Limit", bVisible);
            m_sMemoryGroup = tree.Set(m_sMemoryGroup, m_sMemoryGroup, m_module.MemoryPool.m_asGroup, "MemoryGroup", "Memory Group Name", bVisible);
            MemoryGroup memoryGroup = m_module.MemoryPool.GetGroup(m_sMemoryGroup);
            if (memoryGroup != null) m_sMemoryData = tree.Set(m_sMemoryData, m_sMemoryData, memoryGroup.m_asMemory, "MemoryData", "Memory Data Name", bVisible);
        }

        class PMData
        {
            public Light m_light = null;
            public int m_power = 0;
            public CRect m_rectCheckArea = null;
            public int m_average = 0;
            public double m_posZ = 0;

            public PMData(Light light, int power, CRect rectArea, double posZ)
            {
                m_light = light;
                m_power = power;
                m_rectCheckArea = rectArea;
                m_posZ = posZ;
            }
        }

        string ConnectRADS()
        {
            Camera_Basler camRADS = (Camera_Basler)m_module.CamRADS;

            if (m_grabMode.pUseRADS && m_module.RADSControl.p_IsRun == false)
            {
                m_module.RADSControl.m_timer.Start();
                m_module.RADSControl.p_IsRun = true;
                m_module.RADSControl.StartRADS();

                StopWatch sw = new StopWatch();
                if (camRADS.p_CamInfo._OpenStatus == false) camRADS.Connect();
                while (camRADS.p_CamInfo._OpenStatus == false)
                {
                    if (sw.ElapsedMilliseconds > 15000)
                    {
                        sw.Stop();
                        return "RADS Camera Not Connected";
                    }
                }
                sw.Stop();

                // Offset 설정
                m_module.RADSControl.p_connect.SetADSOffset(m_grabMode.pRADSOffset);

                // RADS 카메라 설정
                camRADS.SetMulticast();
                camRADS.GrabContinuousShot();
            }

            return "OK";
        }

        enum ePMLogType
        {
            PM_Start,
            PM_Error,
            PM_End,
            PM_CoaxialLight_Result,
            PM_Transmitted_Result,
        }
        StreamWriter m_writerLog = null;
        DateTime m_dtNow;
        void PreparePMLog()
        {
            m_dtNow = DateTime.Now;
            string sPath = LogView._logView.p_sPath;

            Directory.CreateDirectory(sPath + "\\PM");

            StreamWriter writer = new StreamWriter(sPath + "\\PM" + "\\" + m_dtNow.ToShortDateString() + ".txt", true, Encoding.Default);
            if(writer != null)
            {
                m_writerLog = writer;
            }
        }
        void WritePMLog(ePMLogType type, string strLog)
        {
            string log = m_dtNow.Hour.ToString("00") + '.' + m_dtNow.Minute.ToString("00") + '.' + m_dtNow.Second.ToString("00") + '.' + m_dtNow.Millisecond.ToString("000");
            log += "\t" + type.ToString() + "\t" + strLog;
            if(m_writerLog != null)
                m_writerLog.WriteLine(log);
        }

        public override string Run()
        {
            if (m_grabMode == null) return "Grab Mode == null";

            // Memory
            MemoryData mem = m_module.MemoryPool.GetMemory(m_sMemoryGroup, m_sMemoryData);
            if (mem == null) return "Set Memory Setting";

            // Light
            Light lightCoaxial = m_module.GetLight(m_sCoaxialLight);
            Light lightTransmitted = m_module.GetLight(m_sTransmittedLight);
            if (lightCoaxial == null || lightTransmitted == null) return "Check Coaxial or Transmitted light setting";

            // USL & LSL Check
            if (m_nLSL > m_nUSL) return "Check USL & LSL setting";

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
            listPMData.Add(new PMData(lightCoaxial, m_nCoaxialLightPower, rectCoaxial, m_dCoaxialZPos));
            listPMData.Add(new PMData(lightTransmitted, m_nTransmittedLightPower, rectTransmitted, m_dTransmittedZPos));

            // Prepare Log
            PreparePMLog();

            WritePMLog(ePMLogType.PM_Start, "");
            bool bCoaxialResult = true;
            bool bTransmittedResult = true;
            // Collect GV Value
            try
            {
                // RADS 연결
                if (m_module.Run(ConnectRADS()))
                    return p_sInfo;

                foreach (PMData pmData in listPMData)
                {
                    if (pmData.m_light == null)
                    {
                        string sMsg = "Check Light Setting";
                        WritePMLog(ePMLogType.PM_Error, sMsg);
                        return sMsg;
                    }

                    // Turn off lights
                    m_grabMode.SetLight(false);

                    // Turn on light
                    pmData.m_light.p_fPower = pmData.m_power;

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

                WritePMLog(ePMLogType.PM_CoaxialLight_Result, bCoaxialResult.ToString() + "\t" + listPMData[0].m_average.ToString());
                WritePMLog(ePMLogType.PM_Transmitted_Result, bTransmittedResult.ToString() + "\t" + listPMData[1].m_average.ToString());

                // Alarm
                if (bCoaxialResult) m_module.m_alidPMCoaxialError.Run(true, m_module.m_alidPMCoaxialError.p_sDesc);
                if (bTransmittedResult) m_module.m_alidPMTransmittedError.Run(true, m_module.m_alidPMTransmittedError.p_sDesc);
            }
            catch (Exception e)
            {
            }
            finally
            {
                // Log
                WritePMLog(ePMLogType.PM_End, "");
                m_writerLog.Close();

                // Turn off light
                m_grabMode.SetLight(false);

                // RADS 기능 off
                Camera_Basler camRADS = (Camera_Basler)m_module.CamRADS;
                if (m_grabMode.pUseRADS && m_module.RADSControl.p_IsRun == true)
                {
                    m_module.RADSControl.m_timer.Stop();
                    m_module.RADSControl.p_IsRun = false;
                    m_module.RADSControl.StopRADS();
                    if (camRADS.p_CamInfo._IsGrabbing == true) camRADS.StopGrab();
                }
            }
            if (!bCoaxialResult && !bTransmittedResult) ((Loadport_Cymechs)m_handler.m_loadport[EQ.p_nRunLP]).m_CommonFunction();
            else m_module.m_alidPMFail.Run(true, "PM is Fail");
            return "OK";
        }
    }

    
}
