using RootTools;
using RootTools.Camera;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Trees;
using System.Collections.Generic;

namespace Root_Wind
{  
    public class GrabMode
    {
        #region Camera
        public enum eDirection
        {
            Forward,
            BackWard,
        }

        public bool m_bUseBiDirectionScan = false;
        public int m_nReverseOffsetY = 800;
        public eDirection m_eGrabDirection = eDirection.Forward;
        string m_sCamera = "";
        public ICamera m_camera = null;
        CameraSet m_cameraSet;
        void RunTreeCamera(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_bUseBiDirectionScan = tree.Set(m_bUseBiDirectionScan, false, "Use BiDirectionScan", "Bi Direction Scan Use");
            m_nReverseOffsetY = tree.Set(m_nReverseOffsetY, 800, "ReverseOffsetY", "Reverse Scan 동작시 Y 이미지 Offset 설정");
            m_sCamera = tree.Set(m_sCamera, m_sCamera, m_cameraSet.p_asCamera, "Camera", "Select Camera", bVisible, bReadOnly);
            m_camera = m_cameraSet.Get(m_sCamera);
        }

        public void StartGrab(MemoryData memory, CPoint cpScanOffset, int nLine, bool bInvY = false)
        {
            m_camera.GrabLineScan(memory, cpScanOffset, nLine, bInvY, m_nReverseOffsetY);
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

        public void SetLight(bool bOn)
        {
            for (int n = 0; n < m_aLightPower.Count; n++)
            {
                m_lightSet.m_aLight[n].p_fPower = bOn ? m_aLightPower[n] : 0;
                m_lightSet.m_aLight[n].p_bOn = bOn;
            }
        }
        #endregion

        #region Memory
        MemoryPool m_memoryPool;
        MemoryGroup m_memoryGroup;
        public MemoryData m_memoryData;
        string m_sMemoryGroup = "";
        string m_sMemoryData = "Grab";
        int m_nByte = 1;
        CPoint m_szROI = new CPoint(1024, 1024);
        void RunTreeMemory(Tree tree, bool bVisible, bool bReadOnly)
        {
            if (m_sMemoryGroup == "")
                m_sMemoryGroup = p_sName;
            m_sMemoryGroup = tree.Set(m_sMemoryGroup, m_sMemoryGroup, "Group", "Memory Group Name", bVisible, bReadOnly);
            m_memoryGroup = m_memoryPool.GetGroup(m_sMemoryGroup);
            if (m_memoryGroup == null)
                return;
            m_sMemoryData = tree.Set(m_sMemoryData, m_sMemoryData, "Data", "Memory Data Name", bVisible, bReadOnly);
            m_szROI = tree.Set(m_szROI, m_szROI, "Size", "Image Size (pixel)", bVisible, bReadOnly);
            m_nByte = tree.Set(m_nByte, m_nByte, "Depth", "Image Depth (RGB = 3, 256 Gray = 1)", bVisible, bReadOnly);
            m_memoryData = m_memoryGroup.CreateMemory(m_sMemoryData, 1, m_nByte, m_szROI);
        }
        #endregion

        #region Axis
        public int m_dTrigger = 10;
        public int m_intervalAcc = 300000;        // 가속 구간 point
        public int m_ScanLineNum = 1;
        public int m_ScanStartLine = 0;
        #endregion

        public string p_id { get; set; }
        
        public string p_sName { get; set; }
        public GrabMode(string id, CameraSet cameraSet, LightSet lightSet, MemoryPool memoryPool)
        {
            p_id = id;
            p_sName = id;
            m_cameraSet = cameraSet;
            m_lightSet = lightSet;
            m_memoryPool = memoryPool;
        }

        public void RunTreeName(Tree tree)
        {
            string sName = p_sName;
            p_sName = tree.Set(p_sName, p_sName, p_id, "Grab Mode Name");
            if (sName != p_sName)
                m_sMemoryGroup = p_sName;
        }

        public void RunTree(Tree tree, bool bVisible, bool bReadOnly)
        {
            RunTreeCamera(tree, bVisible, bReadOnly);
            RunTreeLight(tree.GetTree("LightPower", false), bVisible, bReadOnly);
            RunTreeMemory(tree.GetTree("Memory", false), bVisible, bReadOnly);
        }
    }
}
