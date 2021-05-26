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
        double m_dCenterX = 0;
        double m_dCenterY = 0;
        double m_dScanDistance = 0;
        double m_dFocusZ = 0;
        string m_sLightFirst = "";
        string m_sLightSecond = "";
        int m_nFirstLightPower = 0;
        int m_nSecondLightPower = 0;
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
            run.m_dCenterX = m_dCenterX;
            run.m_dCenterY = m_dCenterY;
            run.m_dScanDistance = m_dScanDistance;
            run.m_dFocusZ = m_dFocusZ;
            run.m_sLightFirst = m_sLightFirst;
            run.m_sLightSecond = m_sLightSecond;
            run.m_nFirstLightPower = m_nFirstLightPower;
            run.m_nSecondLightPower = m_nSecondLightPower;

            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
            m_dCenterX = tree.Set(m_dCenterX, m_dCenterX, "Center X", "Center X Position", bVisible);
            m_dCenterY = tree.Set(m_dCenterY, m_dCenterY, "Center Y", "Center Y Position", bVisible);
            m_dScanDistance = tree.Set(m_dScanDistance, m_dScanDistance, "Scan Distance", "Scan Distance on Y Axis", bVisible);
            m_dFocusZ = tree.Set(m_dFocusZ, m_dFocusZ, "Focus Z", "Select GrabMode", bVisible);
            m_sLightFirst = tree.Set(m_sLightFirst, m_sLightFirst, m_module.p_asLightSet, "1st Light", "First Light Checked", bVisible);
            m_nFirstLightPower = tree.Set(m_nFirstLightPower, m_nFirstLightPower, "1st Light Power", "First Light Power", bVisible);
            m_sLightSecond = tree.Set(m_sLightSecond, m_sLightSecond, m_module.p_asLightSet, "2nd Light", "Second Light Checked", bVisible);
            m_nSecondLightPower = tree.Set(m_nSecondLightPower, m_nSecondLightPower, "2nd Light Power", "Second Light Power", bVisible);
            m_sMemoryGroup = tree.Set(m_sMemoryGroup, m_sMemoryGroup, m_module.MemoryPool.m_asGroup, "MemoryGroup", "Memory Group Name", bVisible);
            MemoryGroup memoryGroup = m_module.MemoryPool.GetGroup(m_sMemoryGroup);
            if (memoryGroup != null) m_sMemoryData = tree.Set(m_sMemoryData, m_sMemoryData, memoryGroup.m_asMemory, "MemoryData", "Memory Data Name", bVisible);
        }

        public override string Run()
        {
            if (m_grabMode == null) return "Grab Mode == null";

            // Memory
            MemoryData mem = m_module.MemoryPool.GetMemory(m_sMemoryGroup, m_sMemoryData);
            if (mem == null) return "Set Memory Setting";

            // Light
            Light lightFirst = m_module.GetLight(m_sLightFirst);
            Light lightSecond = m_module.GetLight(m_sLightSecond);
            if (lightFirst == null || lightSecond == null) return "Set First & Second Light";

            Dictionary<Light, int> dictLight = new Dictionary<Light, int>();
            dictLight.Add(lightFirst, m_nFirstLightPower);
            dictLight.Add(lightSecond, m_nSecondLightPower);

            // Position
            AxisXY axisXY = m_module.AxisXY;
            Axis.Speed speedY = axisXY.p_axisY.GetSpeedValue(Axis.eSpeed.Move);

            double accDistance = speedY.m_acc * speedY.m_v * 0.5 * 2.0;
            double decDistance = speedY.m_dec * speedY.m_v * 0.5 * 2.0;

            double dStartTriggerY = m_dCenterY - m_dScanDistance * 0.5;
            double dEndTriggerY = m_dCenterY + m_dScanDistance * 0.5;

            double dStartPosY = dStartTriggerY - accDistance;
            double dEndPosY = dEndTriggerY + decDistance;

            try
            {
                foreach (KeyValuePair<Light, int> item in dictLight)
                {
                    if (item.Key == null) return "Set First & Second Light";

                    // Turn off lights
                    m_grabMode.SetLight(false);

                    // Turn on light
                    item.Key.p_fPower = item.Value;

                    // Scan Target Image
                    double dPosX = m_dCenterX - m_grabMode.m_GD.m_nFovSize * m_grabMode.m_dResX_um * 0.5;
                    if(m_module.Run(m_module.RunLineScan(m_grabMode, mem, new RootTools.CPoint(0, 0), 1, dPosX, dStartPosY, dEndPosY, dStartTriggerY, dEndTriggerY)))
                        return p_sInfo;


                }
            }
            catch(Exception e)
            {

            }
            finally
            {
                m_grabMode.SetLight(false);
            }

            return "OK";
        }
    }

    
}
