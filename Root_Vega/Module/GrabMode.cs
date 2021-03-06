using RootTools;
using RootTools.Camera;
using RootTools.Light;
using RootTools.Memory;
using RootTools.RADS;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Root_Vega.Module
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

        public void StartGrab(MemoryData memory, CPoint cpScanOffset, int nLine, int nScanOffsetY = 0, bool bInvY = false)
        {
            GrabData gd = new GrabData();
            gd.bInvY = bInvY;
            gd.ReverseOffsetY = m_nReverseOffsetY;
            gd.nScanOffsetY = nScanOffsetY;
            m_camera.GrabLineScan(memory, cpScanOffset, nLine, gd);
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
        public List<double> m_aLightPower = new List<double>();
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
                    m_aLightPower[i] = nValue;
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
        public double m_dTrigger = 10;
        public int m_intervalAcc = 100000;        // 가속 구간 point,  단위 0.1um
        public int m_ScanLineNum = 1;
        public int m_ScanStartLine = 0;
        public int m_nXOffset = 0;
        public int m_nThetaOffset = 0;
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
            m_nXOffset = tree.Set(m_nXOffset, m_nXOffset, "X Offset", "X Offset", bVisible, bReadOnly);
            m_nThetaOffset = tree.Set(m_nThetaOffset, m_nThetaOffset, "Theta Offset", "Theta Offset", bVisible, bReadOnly);
        }

        public string p_sName{get;set;}
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

            dst.m_nXOffset = src.m_nXOffset;
            dst.m_nThetaOffset = src.m_nThetaOffset;

            return dst;
        }

        public void RunTreeName(Tree tree)
        {
            string sName = p_sName;
            p_sName = tree.Set(p_sName, p_sName, p_id, "Grab Mode Name");
        }

        public void RunTree(Tree tree, bool bVisible, bool bReadOnly)
        {
            RunTreeCamera(tree, bVisible, bReadOnly);
            RunTreeLight(tree.GetTree("LightPower", false), bVisible, bReadOnly);
            RunTreeMemory(tree.GetTree("Memory", false), bVisible, bReadOnly);
            RunTreeScanPos(tree.GetTree("ScanPos", false), bVisible, bReadOnly);
            RunTreeRADS(tree.GetTree("RADS", false), bVisible, bReadOnly);
        }
    }
}
