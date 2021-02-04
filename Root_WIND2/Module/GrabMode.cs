using RootTools;
using RootTools.Camera;
using RootTools.Lens.LinearTurret;
using RootTools.Light;
using RootTools.Memory;
using RootTools.RADS;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Root_WIND2.Module
{
    public enum eScanPos
    {
        Bottom = 0,
        Left,
        Top,
        Right,
    }
    public class GrabMode
    {
        #region Camera
        public event System.EventHandler Grabed;
        public bool m_bUseBiDirectionScan = false;
        public eGrabDirection m_eDefaultCamDirection = eGrabDirection.Forward;
        public int m_nReverseOffsetY = 800;
        public eGrabDirection m_eGrabDirection = eGrabDirection.Forward;
        string m_sCamera = "";
        public ICamera m_camera = null;
        CameraSet m_cameraSet;
        public RPoint m_ptXYAlignData = new RPoint(0, 0);
        public double m_dTDIToVRSOffsetX = 0;
        public double m_dTDIToVRSOffsetY = 0;
        public double m_dVRSFocusPos = 0;
        public RPoint m_rpAxisCenter = new RPoint();    // Wafer Center Position
        public CPoint m_cpMemoryOffset = new CPoint();  // Memory Offset
        public double m_dResX_um = 1;                   // Camera Resolution X
        public double m_dResY_um = 1;                   // Camera Resolution Y
        public int m_nFocusPosZ = 0;                    // Focus Position Z
        public int m_nWaferSize_mm = 1000;              // Wafer Size (mm)
        public int m_nMaxFrame = 100;                   // Camera max Frame 스펙
        public int m_nScanRate = 100;                   // Camera Frame Spec 사용률 ? 1~100 %
        public int m_nYOffset = 0;

        public GrabData m_GD = new GrabData();
        LensLinearTurret m_lens = null;
        public string m_sLens;
        void RunTreeOption(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);
            m_cpMemoryOffset = tree.Set(m_cpMemoryOffset, m_cpMemoryOffset, "Memory Offset", "Grab Start Memory Position (px)", bVisible);

            m_GD.m_nFovStart = tree.Set(m_GD.m_nFovStart, m_GD.m_nFovStart, "Cam Fov Star Pxl", "Pixel", bVisible);
            m_GD.m_nFovSize = tree.Set(m_GD.m_nFovSize, m_GD.m_nFovSize, "Cam Fov Size Pxl", "Pixel", bVisible);
            m_GD.m_nOverlap = tree.Set(m_GD.m_nOverlap, m_GD.m_nOverlap, "Cam Overlap Size Pxl", "Pixel", bVisible);
            m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Cam X Resolution", "X Resolution (um)", bVisible);
            m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Cam Y Resolution", "Y Resolution (um)", bVisible);
            m_nYOffset = tree.Set(m_nYOffset, m_nYOffset, "Cam Y Offset", "Y Tilt(pxl)", bVisible);

            //m_sLens = tree.Set(m_sLens, m_sLens, m_lens.p_asPos, "Lens Turret", "Turret", bVisible);
            
            m_GD.m_dScaleR = tree.Set(m_GD.m_dScaleR, m_GD.m_dScaleR, "XScaleR", "X Scale R Channel, Default = 1", bVisible);
            m_GD.m_dScaleG = tree.Set(m_GD.m_dScaleG, m_GD.m_dScaleG, "XScaleG", "X Scale G Channel, Default = 1", bVisible);
            m_GD.m_dScaleB = tree.Set(m_GD.m_dScaleB,  m_GD.m_dScaleB, "XScaleB", "X Scale B Channel, Default = 1", bVisible);

            m_GD.m_dShiftR = tree.Set(m_GD.m_dShiftR, m_GD.m_dShiftR, "XShiftR", "X Shift R Channel, Default = 0", bVisible);
            m_GD.m_dShiftG = tree.Set(m_GD.m_dShiftG, m_GD.m_dShiftG, "XShiftG", "X Shift G Channel, Default = 0", bVisible);
            m_GD.m_dShiftB = tree.Set(m_GD.m_dShiftB, m_GD.m_dShiftB, "XShiftB", "X Shift B Channel, Default = 0", bVisible);
            
            m_nFocusPosZ = tree.Set(m_nFocusPosZ, m_nFocusPosZ, "Focus Z Position", "Focus Z Position", bVisible);
            m_nWaferSize_mm = tree.Set(m_nWaferSize_mm, m_nWaferSize_mm, "Wafer Size Y", "Wafer Size Y", bVisible);
            m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
            m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
        }

        void RunTreeCamera(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_bUseBiDirectionScan = tree.Set(m_bUseBiDirectionScan, false, "Use BiDirectionScan", "Bi Direction Scan Use");
            m_nReverseOffsetY = tree.Set(m_nReverseOffsetY, 800, "ReverseOffsetY", "Reverse Scan 동작시 Y 이미지 Offset 설정");
            m_sCamera = tree.Set(m_sCamera, m_sCamera, m_cameraSet.p_asCamera, "Camera", "Select Camera", bVisible, bReadOnly);
            m_camera = m_cameraSet.Get(m_sCamera);
            m_ScanLineNum = tree.Set(m_ScanLineNum, m_ScanLineNum, "Scan Line Number", "Scan Line Number");
            m_ScanStartLine = tree.Set(m_ScanStartLine, m_ScanStartLine, "Scan Start Line", "Scan Start Line");
            m_ptXYAlignData = tree.Set(m_ptXYAlignData, m_ptXYAlignData, "XY Align Data", "XY Align Data", bVisible, true);
            m_dTDIToVRSOffsetX = tree.Set(m_dTDIToVRSOffsetX, m_dTDIToVRSOffsetX, "TDI To VRS Offset X", "TDI To VRS Offset X");
            m_dTDIToVRSOffsetY = tree.Set(m_dTDIToVRSOffsetY, m_dTDIToVRSOffsetY, "TDI To VRS Offset Y", "TDI To VRS Offset Y");
            m_dVRSFocusPos = tree.Set(m_dVRSFocusPos, m_dVRSFocusPos, "VRS Focus Z", "VRS Focus Z", bVisible, true);
        }

        public void StartGrab(MemoryData memory, CPoint cpScanOffset, int nLine , GrabData m_GrabData = null)
        {
            m_camera.GrabLineScan(memory, cpScanOffset, nLine, m_GrabData);
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
        LightSet m_lightSet;
        List<double> m_aLightPower = new List<double>();
        void RunTreeLight(Tree tree, bool bVisible, bool bReadOnly)
        {
            while (m_aLightPower.Count < m_lightSet.m_aLight.Count)
                m_aLightPower.Add(0);
            for (int n = 0; n < m_aLightPower.Count; n++)
            {
                m_aLightPower[n] = tree.Set(m_aLightPower[n], m_aLightPower[n], m_lightSet.m_aLight[n].m_sName, "Light Power (0 ~ 100 %%)", bVisible, bReadOnly);
            }
        }
        public void SetLens()
        {
            //m_lens.ChangePos(m_sLens);
            //m_lens.WaitReady();
        }
        public void SetLight(bool bOn)
        {
            for (int n = 0; n < m_aLightPower.Count; n++)
            {
                m_lightSet.m_aLight[n].m_light.p_fSetPower = bOn ? m_aLightPower[n] : 0;
            }
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

        #region RADS
        public RADSControl m_RADSControl;
        bool m_bUseRADS = false;
        void RunTreeRADS(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_bUseRADS = tree.Set(m_bUseRADS, m_bUseRADS, "Use", "Using RADS", bVisible, false);
        }

        public bool GetUseRADS()
        {
            return m_bUseRADS;
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

        public eScanPos m_eScanPos = eScanPos.Bottom;

        public string p_id
        {
            get;
            set;
        }
        void RunTreeScanPos(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_eScanPos = (eScanPos)tree.Set(m_eScanPos, m_eScanPos, "Scan 위치", "Scan 위치, 0 Position 이 Bottom", bVisible, bReadOnly);
        }

        public string p_sName { get; set; }
        public GrabMode(string id, CameraSet cameraSet, LightSet lightSet, MemoryPool memoryPool, RADSControl radsControl = null)
        {
            p_id = id;
            p_sName = id;
            m_cameraSet = cameraSet;
            m_lightSet = lightSet;
            m_memoryPool = memoryPool;
            m_RADSControl = radsControl;
        }

        public static GrabMode Copy(GrabMode src)
        {
            GrabMode dst = new GrabMode(src.p_id, src.m_cameraSet, src.m_lightSet, src.m_memoryPool, src.m_RADSControl);
            dst.Grabed = src.Grabed;
            dst.m_bUseBiDirectionScan = src.m_bUseBiDirectionScan;
            dst.m_camera = src.m_camera;
            dst.m_dTrigger = src.m_dTrigger;
            dst.m_eGrabDirection = src.m_eGrabDirection;
            dst.m_eScanPos = src.m_eScanPos;
            dst.m_intervalAcc = src.m_intervalAcc;
            dst.m_memoryData = src.m_memoryData;
            dst.m_memoryGroup = src.m_memoryGroup;
            dst.m_nReverseOffsetY = src.m_nReverseOffsetY;
            dst.m_aLightPower = src.m_aLightPower;
            dst.m_bUseRADS = src.m_bUseRADS;
            dst.m_sCamera = src.m_sCamera;
            dst.p_sName = src.p_sName;
            dst.m_ScanLineNum = src.m_ScanLineNum;
            dst.m_ScanStartLine = src.m_ScanStartLine;
            dst.m_sMemoryData = src.m_sMemoryData;
            dst.m_sMemoryGroup = src.m_sMemoryGroup;
            dst.m_rpAxisCenter = new RPoint(src.m_rpAxisCenter);
            dst.m_cpMemoryOffset = new CPoint(src.m_cpMemoryOffset);
            dst.m_dResX_um = src.m_dResX_um;
            dst.m_dResY_um = src.m_dResY_um;
            dst.m_nFocusPosZ = src.m_nFocusPosZ;
            dst.m_nWaferSize_mm = src.m_nWaferSize_mm;
            dst.m_nMaxFrame = src.m_nMaxFrame;
            dst.m_nScanRate = src.m_nScanRate;
            return dst;
        }

        public void RunTreeName(Tree tree)
        {
            string sName = p_sName;
            p_sName = tree.Set(p_sName, p_sName, p_id, "Grab Mode Name");
            //if (sName != p_sName)
            //    m_sMemoryGroup = p_sName;
        }

        public void RunTree(Tree tree, bool bVisible, bool bReadOnly)
        {
            RunTreeOption(tree, bVisible, bReadOnly);
            RunTreeCamera(tree, bVisible, bReadOnly);
            RunTreeLight(tree.GetTree("LightPower", false), bVisible, bReadOnly);
            RunTreeMemory(tree.GetTree("Memory", false), bVisible, bReadOnly);
            RunTreeScanPos(tree.GetTree("ScanPos", false), bVisible, bReadOnly);
            RunTreeRADS(tree.GetTree("RADS", false), bVisible, bReadOnly);
        }

        public virtual void RunTree(Tree.eMode mode) { }
    }
}
