﻿using RootTools;
using RootTools.Camera;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Trees;
using System.Collections.Generic;

namespace Root_Vega
{  
    public class GrabMode
    {
        #region Camera
        public bool m_bUseBiDirectionScan = false;
        public int m_nReverseOffsetY = 800;
        public eGrabDirection m_eGrabDirection = eGrabDirection.Forward;
        string m_sCamera = "";
        public ICamera m_camera = null;
        CameraSet m_cameraSet;
        void RunTreeCamera(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_bUseBiDirectionScan = tree.Set(m_bUseBiDirectionScan, false, "Use BiDirectionScan", "Bi Direction Scan Use");
            m_nReverseOffsetY = tree.Set(m_nReverseOffsetY, 800, "ReverseOffsetY", "Reverse Scan 동작시 Y 이미지 Offset 설정");
            m_sCamera = tree.Set(m_sCamera, m_sCamera, m_cameraSet.p_asCamera, "Camera", "Select Camera", bVisible, bReadOnly);
            m_camera = m_cameraSet.Get(m_sCamera);
            m_ScanLineNum = tree.Set(m_ScanLineNum, m_ScanLineNum, "Scan Line Number", "Scan Line Number");
            m_ScanStartLine = tree.Set(m_ScanStartLine, m_ScanStartLine, "Scan Start Line", "Scan Start Line");
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
                m_lightSet.m_aLight[n].m_light.p_fSetPower = bOn ? m_aLightPower[n] : 0;
            }
        }
        #endregion

        #region Memory
        public MemoryPool m_memoryPool;
        public MemoryGroup m_memoryGroup;
        public MemoryData m_memoryData;
        string m_sMemoryGroup = "";
        public string p_sMemoryGroup
        {
            get
            {
                return m_sMemoryGroup;
            }
        }
        string m_sMemoryData = "Grab";
        public string p_sMemoryData
        {
            get
            {
                return m_sMemoryData;
            }
        }

        void RunTreeMemory(Tree tree, bool bVisible, bool bReadOnly)
        {
            if (m_sMemoryGroup == "") m_sMemoryGroup = p_sName;
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

        public enum eScanPos
        {
            Bottom = 0,
            Left,
            Top,
            Right,
        }
        public eScanPos m_eScanPos = eScanPos.Bottom;

        public string p_id
        {
            get;
            set;
        }
        void RunTreeScanPos(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_eScanPos = (eScanPos)tree.Set(m_eScanPos, m_eScanPos, "Scan 위치", "Scan 위치, 0 Position 이 Bottom", bVisible);
        }

        public string p_sName{get;set;}
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
            RunTreeScanPos(tree.GetTree("ScanPos", false), bVisible, bReadOnly);
        }
    }
}
