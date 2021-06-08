using RootTools;
using RootTools.Camera;
using RootTools.Camera.Matrox;
using RootTools.Lens.LinearTurret;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Trees;

namespace Root_VEGA_P_Vision.Module
{
    public class GrabMode:GrabModeBase
    {
        #region Camera
        public bool m_bUseBiDirectionScan = false;
        string m_sCamera = "";
        public RPoint m_rpAxisCenter = new RPoint();    // Pod Center Position
        public double m_dResX_um = 1;                   // Camera Resolution X
        public double m_dResY_um = 1;                   // Camera Resolution Y
        public int m_nFocusPosZ = 0;                    // Focus Position Z
        public int m_nPodYSize_mm = 1000;                // Pod Size Y (mm)
        public int m_nPodXSize_mm = 1000;                // Pod Size X (mm)
        public int m_nReverseOffsetY = 800;

        void RunTreeOption(Tree tree, bool bVisible)
        {
            m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);            
            m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Cam X Resolution", "X Resolution (um)", bVisible);
            m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Cam Y Resolution", "Y Resolution (um)", bVisible);            
            m_nFocusPosZ = tree.Set(m_nFocusPosZ, m_nFocusPosZ, "Focus Z Position", "Focus Z Position", bVisible);
            m_nPodYSize_mm = tree.Set(m_nPodYSize_mm, m_nPodYSize_mm, "Pod Size Y", "Pod Size Y", bVisible);
            m_nPodXSize_mm = tree.Set(m_nPodXSize_mm, m_nPodXSize_mm, "Pod Size X", "Pod Size X", bVisible);
        }
        public void RunTreeLinescanOption(Tree tree,bool bVisible)
        {
            m_cpMemoryOffset = tree.Set(m_cpMemoryOffset, m_cpMemoryOffset, "Memory Offset", "Grab Start Memory Position (px)", bVisible);
            m_GD.m_nFovStart = tree.Set(m_GD.m_nFovStart, m_GD.m_nFovStart, "Cam Fov Start Pxl", "Pixel", bVisible);
            m_GD.m_nFovSize = tree.Set(m_GD.m_nFovSize, m_GD.m_nFovSize, "Cam Fov Size Pxl", "Pixel", bVisible);
            m_GD.m_nOverlap = tree.Set(m_GD.m_nOverlap, m_GD.m_nOverlap, "Cam Overlap Size Pxl", "Pixel", bVisible);
            m_nYOffset = tree.Set(m_nYOffset, m_nYOffset, "Cam Y Offset", "Y Tilt(pxl)", bVisible);
            m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
            m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
            m_bUseBiDirectionScan = tree.Set(m_bUseBiDirectionScan, false, "Use BiDirectionScan", "Bi Direction Scan Use", bVisible);
            m_nReverseOffsetY = tree.Set(m_nReverseOffsetY, 800, "ReverseOffsetY", "Reverse Scan 동작시 Y 이미지 Offset 설정", bVisible);
            m_ScanLineNum = tree.Set(m_ScanLineNum, m_ScanLineNum, "Scan Line Number", "Scan Line Number", bVisible);
            m_ScanStartLine = tree.Set(m_ScanStartLine, m_ScanStartLine, "Scan Start Line", "Scan Start Line", bVisible);
        }
        void RunTreeCamera(Tree tree, bool bVisible)
        {
            if (m_cameraSet != null)
            {
                m_sCamera = tree.Set(m_sCamera, m_sCamera, m_cameraSet.p_asCamera, "Camera", "Select Camera", bVisible, false);
                m_camera = m_cameraSet.Get(m_sCamera);
            }            
        }

        #endregion

        #region Light
        public void SetLight(int n,bool bOn)
        {
            m_lightSet.m_aLight[n].m_light.p_fSetPower = bOn ? m_aLightPower[n] : 0;
        }
        public double GetLight(int n)
        {
            return m_aLightPower[n];
        }
        #endregion

        #region Axis
        public int m_ScanLineNum = 1;
        public int m_ScanStartLine = 0;
        #endregion

        public GrabMode(string id, CameraSet camSet,LightSet lightSet, MemoryPool memoryPool, LensLinearTurret lensTurret = null): base(id, camSet, lightSet, memoryPool, lensTurret)
        {
            p_id = id;
            p_sName = id;
            m_cameraSet = camSet;
            m_lightSet = lightSet;
            m_memoryPool = memoryPool;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bReadOnly)
        {
            RunTreeOption(tree, bVisible);
            RunTreeCamera(tree, bVisible);
            RunTreeLight(tree.GetTree("LightPower", false), bVisible, bReadOnly);
        }


        public void StartZGrab(MemoryData memory,int nGrabcnt,CPoint memOffset)
        {
            ((Camera_Matrox)m_camera).GrabZScan(memory, nGrabcnt,memOffset);
            m_camera.Grabed += m_camera_Grabed;
        }
    }
}
