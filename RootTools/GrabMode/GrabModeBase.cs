using RootTools;
using RootTools.Camera;
using RootTools.Lens.LinearTurret;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace RootTools
{
    public enum eScanPosition
    {
        Bottom = 0,
        Left,
        Top,
        Right,
    }

    public class GrabModeBase
    {
        public string p_id
        {
            get;
            set;
        }

        public string p_sName { get; set; }

        #region Camera
        public int m_nWaferSize_mm = 1000;              // Wafer Size (mm)
        public int m_nMaxFrame = 100;                   // Camera max Frame 스펙
        public int m_nScanRate = 100;                   // Camera Frame Spec 사용률 ? 1~100 %
        public double m_dCamTriggerRatio = 1;           // Camera 분주비
        public double m_dTargetResX_um = 1;             // Pixel Resolution X
        public double m_dTargetResY_um = 1;             // Pixel Resolution Y
        public double m_dRealResX_um = 1;               // 실제 Camera Resolution X
        public double m_dRealResY_um = 1;               // 실제 Camera Resolution Y
        public CPoint m_cpMemoryOffset = new CPoint();  // Memory Offset
        public int m_nYOffset = 0;                      // 일단 안써 

        string m_sCamera = "";
        public CameraSet m_cameraSet;
        public ICamera m_camera = null;
        public eGrabDirection m_eDefaultCamDirection = eGrabDirection.Forward;
        public eGrabDirection m_eGrabDirection = eGrabDirection.Forward;

        public GrabData m_GD = new GrabData();
        public event System.EventHandler Grabed;

        void RunTreeOption(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_nWaferSize_mm = tree.Set(m_nWaferSize_mm, m_nWaferSize_mm, "Wafer Size Y", "Wafer Size Y", bVisible);
            m_cpMemoryOffset = tree.Set(m_cpMemoryOffset, m_cpMemoryOffset, "Memory Offset", "Grab Start Memory Position (px)", bVisible);
            m_nYOffset = tree.Set(m_nYOffset, m_nYOffset, "Cam Y Offset", "Y Tilt(pxl)", bVisible);

            m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
            m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
                        
            m_GD.m_nFovStart = (tree.GetTree("FOV", false, bVisible)).Set(m_GD.m_nFovStart, m_GD.m_nFovStart, "Cam Fov Start Pxl", "Pixel", bVisible);
            m_GD.m_nFovSize = (tree.GetTree("FOV", false, bVisible)).Set(m_GD.m_nFovSize, m_GD.m_nFovSize, "Cam Fov Size Pxl", "Pixel", bVisible);
            m_GD.m_nOverlap = (tree.GetTree("FOV", false, bVisible)).Set(m_GD.m_nOverlap, m_GD.m_nOverlap, "Cam Overlap Size Pxl", "Pixel", bVisible);

            m_GD.m_dScaleR = (tree.GetTree("Scale", false, bVisible)).Set(m_GD.m_dScaleR, m_GD.m_dScaleR, "XScaleR", "X Scale R Channel, Default = 1", bVisible);
            m_GD.m_dScaleG = (tree.GetTree("Scale", false, bVisible)).Set(m_GD.m_dScaleG, m_GD.m_dScaleG, "XScaleG", "X Scale G Channel, Default = 1", bVisible);
            m_GD.m_dScaleB = (tree.GetTree("Scale", false, bVisible)).Set(m_GD.m_dScaleB,  m_GD.m_dScaleB, "XScaleB", "X Scale B Channel, Default = 1", bVisible);

            m_GD.m_dShiftR = (tree.GetTree("Shift", false, bVisible)).Set(m_GD.m_dShiftR, m_GD.m_dShiftR, "XShiftR", "X Shift R Channel, Default = 0", bVisible);
            m_GD.m_dShiftG = (tree.GetTree("Shift", false, bVisible)).Set(m_GD.m_dShiftG, m_GD.m_dShiftG, "XShiftG", "X Shift G Channel, Default = 0", bVisible);
            m_GD.m_dShiftB = (tree.GetTree("Shift", false, bVisible)).Set(m_GD.m_dShiftB, m_GD.m_dShiftB, "XShiftB", "X Shift B Channel, Default = 0", bVisible);
            
            m_GD.m_nYShiftR = (tree.GetTree("Shift", false, bVisible)).Set(m_GD.m_nYShiftR, m_GD.m_nYShiftR, "YShiftR", "Y Shift R Channel, Default = 0", bVisible);
            m_GD.m_nYShiftG = (tree.GetTree("Shift", false, bVisible)).Set(m_GD.m_nYShiftG, m_GD.m_nYShiftG, "YShiftG", "Y Shift G Channel, Default = 0", bVisible);
            m_GD.m_nYShiftB = (tree.GetTree("Shift", false, bVisible)).Set(m_GD.m_nYShiftB, m_GD.m_nYShiftB, "YShiftB", "Y Shift B Channel, Default = 0", bVisible);
        }

        void RunTreeCamera(Tree tree, bool bVisible, bool bReadOnly)
        {
            if (m_cameraSet != null)
            {
                m_sCamera = tree.Set(m_sCamera, m_sCamera, m_cameraSet.p_asCamera, "Camera", "Select Camera", bVisible, bReadOnly);
                m_camera = m_cameraSet.Get(m_sCamera);
            }
            m_dCamTriggerRatio = tree.Set(m_dCamTriggerRatio, m_dCamTriggerRatio, "Trigger Ratio", "Trigger Ratio", bVisible);

            m_dTargetResX_um = (tree.GetTree("Resolution [um]", false, bVisible)).Set(m_dTargetResX_um, m_dTargetResX_um, "X Target", "X Target Resolution", bVisible);
            m_dTargetResY_um = (tree.GetTree("Resolution [um]", false, bVisible)).Set(m_dTargetResY_um, m_dTargetResY_um, "Y Target", "Y Target Resolution", bVisible);
            m_dRealResX_um = (tree.GetTree("Resolution [um]", false, bVisible)).Set(m_dRealResX_um, m_dRealResX_um, "X Real", "X Real Resolution", bVisible);
            m_dRealResY_um = (tree.GetTree("Resolution [um]", false, bVisible)).Set(m_dRealResY_um, m_dRealResY_um, "Y Real", "Y Real Resolution", bVisible);
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

        protected void m_camera_Grabed(object sender, System.EventArgs e)
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
        public List<double> m_aLightPower = new List<double>(); 

        protected void RunTreeLight(Tree tree, bool bVisible, bool bReadOnly)
        {
            if (m_lightSet == null) return;

            while (m_aLightPower.Count < m_lightSet.m_aLight.Count)
                m_aLightPower.Add(0);
            for (int n = 0; n < m_aLightPower.Count; n++)
            {
                m_aLightPower[n] = tree.Set(m_aLightPower[n], m_aLightPower[n], m_lightSet.m_aLight[n].m_sName, "Light Power (0 ~ 100 %%)", bVisible, bReadOnly);
            }
        }

        public void SetLight(bool bOn)
        {
            for (int n = 0; n < m_aLightPower.Count; n++)
            {
                if(m_lightSet.m_aLight[n].m_light!=null)
                    m_lightSet.m_aLight[n].m_light.p_fSetPower = bOn ? m_aLightPower[n] : 0;
            }
        }

        public void SetLightByIdx(int nIdx)
        {
            m_lightSet.m_aLight[nIdx].m_light.p_fSetPower = m_aLightPower[nIdx];

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
        #endregion

        public GrabModeBase(string id, CameraSet cameraSet, LightSet lightSet, MemoryPool memoryPool, LensLinearTurret lensTurret = null)
        {
            p_id = id;
            p_sName = id;
            m_cameraSet = cameraSet;
            m_lightSet = lightSet;
            m_memoryPool = memoryPool;
        }

        public virtual object Copy(object grabMode)
        {
            GrabModeBase src = (GrabModeBase)grabMode;
           
            GrabModeBase dst = new GrabModeBase(src.p_id, src.m_cameraSet, src.m_lightSet, src.m_memoryPool);
            dst.p_id = src.p_id;
            dst.p_sName = src.p_sName;

            // camera
            dst.m_nWaferSize_mm = src.m_nWaferSize_mm;
            dst.m_nMaxFrame = src.m_nMaxFrame;
            dst.m_nScanRate = src.m_nScanRate;
            dst.m_dCamTriggerRatio = src.m_dCamTriggerRatio;
            dst.m_dTargetResX_um = src.m_dTargetResX_um;
            dst.m_dTargetResY_um = src.m_dTargetResY_um;
            dst.m_dRealResX_um = src.m_dRealResX_um;
            dst.m_dRealResY_um = src.m_dRealResY_um;
            dst.m_cpMemoryOffset = new CPoint(src.m_cpMemoryOffset);
            dst.m_nYOffset = src.m_nYOffset;

            dst.m_sCamera = src.m_sCamera;
            dst.m_cameraSet = src.m_cameraSet;
            dst.m_camera = src.m_camera;
            dst.m_eDefaultCamDirection = src.m_eDefaultCamDirection;
            dst.m_eGrabDirection = src.m_eGrabDirection;
            dst.m_GD = src.m_GD;
            dst.Grabed = src.Grabed;

            // light
            dst.m_lightSet = src.m_lightSet;
            dst.m_aLightPower = src.m_aLightPower;

            // memory
            dst.m_memoryPool = src.m_memoryPool;
            dst.m_memoryGroup = src.m_memoryGroup;
            dst.m_memoryData = src.m_memoryData;
            dst.m_sMemoryData = src.m_sMemoryData;
            dst.m_sMemoryGroup = src.m_sMemoryGroup;

            // axis
            dst.m_dTrigger = src.m_dTrigger;
            dst.m_intervalAcc = src.m_intervalAcc;
            return dst;
        }

        public void RunTreeName(Tree tree)
        {
            string sName = p_sName;
            p_sName = tree.Set(p_sName, p_sName, p_id, "Grab Mode Name");
            //if (sName != p_sName)
            //    m_sMemoryGroup = p_sName;
        }

        public virtual void RunTree(Tree tree, bool bVisible, bool bReadOnly)
        {
            RunTreeOption(tree.GetTree("Option", false), bVisible, bReadOnly);
            RunTreeCamera(tree.GetTree("Camera", false), bVisible, bReadOnly);
            RunTreeLight(tree.GetTree("Light", false), bVisible, bReadOnly);
            RunTreeMemory(tree.GetTree("Memory", false), bVisible, bReadOnly);
        }

        public virtual void RunTree(Tree.eMode mode) { }
    }
}
