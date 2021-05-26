using RootTools;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        int m_nUSL = 0;
        int m_nLSL = 0;
        string m_sMemoryGroup = "";
        string m_sMemoryData = "";

        public string p_sGrabMode
        {
            get { return m_sGrabMode; }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }
        public Run_PM(Vision module)
        {
            m_module = module;
            InitModuleRun(module);
        }
        public override ModuleRunBase Clone()
        {
            Run_PM run = new Run_PM(m_module);

            run.p_sGrabMode = p_sGrabMode;
            run.m_dScanDistance = m_dScanDistance;
            run.m_dLengthFromScanCenterY = m_dLengthFromScanCenterY;
            run.m_nCheckArea = m_nCheckArea;
            run.m_sCoaxialLight = m_sCoaxialLight;
            run.m_nCoaxialLightPower = m_nCoaxialLightPower;
            run.m_sTransmittedLight = m_sTransmittedLight;
            run.m_nTransmittedLightPower = m_nTransmittedLightPower;
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
            m_sTransmittedLight = tree.Set(m_sTransmittedLight, m_sTransmittedLight, m_module.p_asLightSet, "Transmitted Light", "Transmitted Light for PM", bVisible);
            m_nTransmittedLightPower = tree.Set(m_nTransmittedLightPower, m_nTransmittedLightPower, "Transmitted Light Power", "Transmitted Light Power", bVisible);
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

            public PMData(Light light, int power, CRect rectArea)
            {
                m_light = light;
                m_power = power;
                m_rectCheckArea = rectArea;
            }
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
            int nCheckPointLenFromCenter_px = (int)(m_dLengthFromScanCenterY * 1000 * 0.5 / m_grabMode.m_dResY_um);
            int nCoaxialCheckPosY_px = centerY_px + nCheckPointLenFromCenter_px;
            int nTransmittedCheckPosY_px = centerY_px - nCheckPointLenFromCenter_px;

            CRect rectCoaxial = new CRect((int)(m_grabMode.m_GD.m_nFovSize * 0.5), nCoaxialCheckPosY_px, m_nCheckArea);
            CRect rectTransmitted = new CRect((int)(m_grabMode.m_GD.m_nFovSize * 0.5), nTransmittedCheckPosY_px, m_nCheckArea);

            List<PMData> listPMData = new List<PMData>();
            listPMData.Add(new PMData(lightCoaxial, m_nCoaxialLightPower, rectCoaxial));
            listPMData.Add(new PMData(lightTransmitted, m_nTransmittedLightPower, rectTransmitted));

            // Collect GV Value
            try
            {
                foreach (PMData pmData in listPMData)
                {
                    if (pmData.m_light == null) return "Check Light Setting";

                    // Turn off lights
                    m_grabMode.SetLight(false);

                    // Turn on light
                    pmData.m_light.p_fPower = pmData.m_power;

                    // 포커스 높이로 Z축 이동
                    Axis axisZ = m_module.AxisZ;
                    if (m_module.Run(axisZ.StartMove(m_grabMode.m_dFocusPosZ)))
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
                                    sumGV += arrData[(mem.p_sz.X * y + x) * mem.p_nByte + i];
                                }
                            }
                        }
                    }

                    // Get Average GV
                    int nAverageGV = (int)(sumGV / (m_nCheckArea * m_nCheckArea));
                    pmData.m_average = nAverageGV;
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                m_grabMode.SetLight(false);
            }

            // PM check result
            bool bCoaxialResult = listPMData[0].m_average <= m_nUSL && listPMData[0].m_average >= m_nLSL;
            bool bTransmittedResult = listPMData[1].m_average <= m_nUSL && listPMData[1].m_average >= m_nLSL;
            
            m_log.Info(string.Format("Coaxial Light PM result : {0} (Average = {1})", bCoaxialResult ? "OK" : "Fail", listPMData[0].m_average));
            m_log.Info(string.Format("Transmitted Light PM result : {0} (Average = {1})", bTransmittedResult ? "OK" : "Fail", listPMData[1].m_average));

            return "OK";
        }
    }

    
}
