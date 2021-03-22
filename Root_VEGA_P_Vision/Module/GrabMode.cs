using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace Root_VEGA_P_Vision.Module
{
    public class GrabMode
    {
        #region Camera
        public event EventHandler Grabed;
        public bool m_bUseBiDirectionScan = false;
        public Vision.SideOptic.eSide side;
        public eGrabDirection m_eDefaultCamDirection = eGrabDirection.Forward;
        public eGrabDirection m_eGrabDirection = eGrabDirection.Forward;
        public ICamera m_camera = null;
        CameraSet m_cameraSet;
        string m_sCamera = "";
        public double m_dTDIToVRSOffsetX = 0;
        public double m_dTDIToVRSOffsetY = 0;
        public double m_dVRSFocusPos = 0;
        public CPoint m_cpMemoryOffset = new CPoint();  // Memory Offset
        public RPoint m_rpAxisCenter = new RPoint();    // Pod Center Position
        public double m_dResX_um = 1;                   // Camera Resolution X
        public double m_dResY_um = 1;                   // Camera Resolution Y
        public int m_nFocusPosZ = 0;                    // Focus Position Z
        public int m_nPodSize_mm = 1000;                // Pod Size (mm)
        public int m_nMaxFrame = 100;                   // Camera max Frame 스펙
        public int m_nScanRate = 100;                   // Camera Frame Spec 사용률 ? 1~100 %
        public int m_nReverseOffsetY = 800;
        public int m_nYOffset = 0;

        public GrabData m_GD = new GrabData();
        void RunTreeOption(Tree tree, bool bVisible)
        {
            m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);            
            m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Cam X Resolution", "X Resolution (um)", bVisible);
            m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Cam Y Resolution", "Y Resolution (um)", bVisible);            
            m_nFocusPosZ = tree.Set(m_nFocusPosZ, m_nFocusPosZ, "Focus Z Position", "Focus Z Position", bVisible);
            m_nPodSize_mm = tree.Set(m_nPodSize_mm, m_nPodSize_mm, "Pod Size Y", "Pod Size Y", bVisible);            
        }
        void RunTreeLinescanOption(Tree tree,bool bVisible)
        {
            m_cpMemoryOffset = tree.Set(m_cpMemoryOffset, m_cpMemoryOffset, "Memory Offset", "Grab Start Memory Position (px)", bVisible);
            m_GD.m_nFovStart = tree.Set(m_GD.m_nFovStart, m_GD.m_nFovStart, "Cam Fov Star Pxl", "Pixel", bVisible);
            m_GD.m_nFovSize = tree.Set(m_GD.m_nFovSize, m_GD.m_nFovSize, "Cam Fov Size Pxl", "Pixel", bVisible);
            m_GD.m_nOverlap = tree.Set(m_GD.m_nOverlap, m_GD.m_nOverlap, "Cam Overlap Size Pxl", "Pixel", bVisible);
            m_nYOffset = tree.Set(m_nYOffset, m_nYOffset, "Cam Y Offset", "Y Tilt(pxl)", bVisible);
            m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
            m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
            m_bUseBiDirectionScan = tree.Set(m_bUseBiDirectionScan, false, "Use BiDirectionScan", "Bi Direction Scan Use");
            m_nReverseOffsetY = tree.Set(m_nReverseOffsetY, 800, "ReverseOffsetY", "Reverse Scan 동작시 Y 이미지 Offset 설정");
            m_ScanLineNum = tree.Set(m_ScanLineNum, m_ScanLineNum, "Scan Line Number", "Scan Line Number");
            m_ScanStartLine = tree.Set(m_ScanStartLine, m_ScanStartLine, "Scan Start Line", "Scan Start Line");
            m_dTDIToVRSOffsetX = tree.Set(m_dTDIToVRSOffsetX, m_dTDIToVRSOffsetX, "TDI To VRS Offset X", "TDI To VRS Offset X");
            m_dTDIToVRSOffsetY = tree.Set(m_dTDIToVRSOffsetY, m_dTDIToVRSOffsetY, "TDI To VRS Offset Y", "TDI To VRS Offset Y");
            m_dVRSFocusPos = tree.Set(m_dVRSFocusPos, m_dVRSFocusPos, "VRS Focus Z", "VRS Focus Z", bVisible, true);

        }
        void RunTreeCamera(Tree tree, bool bVisible)
        {
            if (m_cameraSet != null)
            {
                m_sCamera = tree.Set(m_sCamera, m_sCamera, m_cameraSet.p_asCamera, "Camera", "Select Camera", bVisible, false);
                m_camera = m_cameraSet.Get(m_sCamera);
            }            
        }

        public void StartGrab(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData m_GrabData = null, bool bTest = false)
        {
            m_camera.GrabLineScan(memory, cpScanOffset, nLine, m_GrabData, bTest);
            m_camera.Grabed += m_camera_Grabed;
        }
        public void StartGrabColor(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData m_GrabData = null)
        {
            m_camera.GrabLineScanColor(memory, cpScanOffset, nLine, m_GrabData);
            m_camera.Grabed += m_camera_Grabed;
        }
        void m_camera_Grabed(object sender, System.EventArgs e)
        {
            if (Grabed != null)
                Grabed.Invoke(sender, e);
        }
        public void StopGrab()
        {
            m_camera.StopGrab();
            m_camera.Grabed -= m_camera_Grabed;
            Grabed = null;
        }
        #endregion

        #region Light
        public LightSet m_lightSet;
        List<double> m_aLightPower = new List<double>();
        void RunTreeLight(Tree tree, bool bVisible, bool bReadOnly)
        {
            if (m_lightSet == null) return;

            while (m_aLightPower.Count < m_lightSet.m_aLight.Count)
                m_aLightPower.Add(0);
            for (int n = 0;  n < m_aLightPower.Count; n++)
            {
                m_aLightPower[n] = tree.Set(m_aLightPower[n], m_aLightPower[n], m_lightSet.m_aLight[n].m_sName, "Light Power (0 ~ 100 %%)", bVisible, bReadOnly);
            }
        }
        public void SetLight(bool bOn)
        {
            for (int n = 0; n < m_aLightPower.Count; n++)
            {
                m_lightSet.m_aLight[n].m_light.p_fSetPower = bOn ? m_aLightPower[n] : 0;
            }
        }
        public void SetLight(int n,bool bOn)
        {
            m_lightSet.m_aLight[n].m_light.p_fSetPower = bOn ? m_aLightPower[n] : 0;
        }
        public int GetLightByName(string str)
        {
            for (int i = 0; i < m_lightSet.m_aLight.Count; i++)
            {
                if (m_lightSet.m_aLight[i].m_sName.IndexOf(str) >= 0)
                {
                    return Convert.ToInt32(m_lightSet.m_aLight[i].p_fPower);
                }
            }
            return 0;
        }

        public void SetLightByName(string str, int nValue)
        {
            for (int i = 0; i < m_lightSet.m_aLight.Count; i++)
            {
                if (m_lightSet.m_aLight[i].m_sName.IndexOf(str) >= 0)
                {
                    m_lightSet.m_aLight[i].m_light.p_fSetPower = nValue;
                }
            }
        }
        #endregion

        #region Memory
        public MemoryPool m_memoryPool;
        public MemoryGroup m_memoryGroup;
        public MemoryData m_memoryData;
        string m_sMemoryGroup = "";
        string m_sMemoryData = "Grab";

        void RunTreeMemory(Tree tree, bool bVisible, bool bReadOnly)
        {
            if (m_sMemoryGroup == "") m_sMemoryGroup = m_memoryPool.m_asGroup[0];
            m_sMemoryGroup = tree.Set(m_sMemoryGroup, m_sMemoryGroup, m_memoryPool.m_asGroup, "Group", "Memory Group Name", bVisible, bReadOnly);
            m_memoryGroup = m_memoryPool.GetGroup(m_sMemoryGroup);
            if (m_memoryGroup == null) return;
            m_sMemoryData = tree.Set(m_sMemoryData, m_sMemoryData, m_memoryGroup.m_asMemory, "Data", "Memory Data Name", bVisible, bReadOnly);
            m_memoryData = m_memoryGroup.GetMemory(m_sMemoryData);
        }
        #endregion

        #region Axis
        public int m_dTrigger = 10;
        public int m_intervalAcc = 100000;        // 가속 구간 point,  단위 0.1um
        public int m_ScanLineNum = 1;
        public int m_ScanStartLine = 0;
        #endregion

        public string p_id
        {
            get;
            set;
        }

        public string p_sName { get; set; }

        public GrabMode(string id, CameraSet camSet,LightSet lightSet, MemoryPool memoryPool)
        {
            p_id = id;
            p_sName = id;
            m_cameraSet = camSet;
            m_lightSet = lightSet;
            m_memoryPool = memoryPool;
        }

        public void RunTreeName(Tree tree)
        {
            p_sName = tree.Set(p_sName, p_sName, p_id, "Grab Mode Name");
        }

        public void RunTree(Tree tree, bool bVisible, bool bReadOnly)
        {
            RunTreeOption(tree, bVisible);
            RunTreeCamera(tree, bVisible);
            if (m_camera != null)
            {
                if(m_camera.p_id.Contains("Main"))
                    RunTreeLinescanOption(tree, bVisible);
                if (m_camera.p_id.Contains("Side"))
                    side = (Vision.SideOptic.eSide)tree.Set(side, side, "Side Scan Pos", "Side Scan Pos");
            }

            RunTreeLight(tree.GetTree("LightPower", false), bVisible, bReadOnly);
        }
    }
}
