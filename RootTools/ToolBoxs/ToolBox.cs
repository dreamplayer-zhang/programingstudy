﻿using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.CognexOCR;
using RootTools.Camera.Dalsa;
using RootTools.Comm;
using RootTools.Control;
using RootTools.DMC;
using RootTools.Gem;
using RootTools.Inspects;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.OHT.Semi;
using RootTools.OHT.SSEM;
using RootTools.RTC5s.LaserBright;
using RootTools.SQLogs;
using RootTools.ZoomLens;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RootTools.ToolBoxs
{
    public class ToolBox
    {
        #region StringTable
        void InitStringTable(string sModel)
        {
            StringTable._stringTable.Init(sModel); 
            StringTable_UI ui = new StringTable_UI();
            ui.Init(StringTable._stringTable);
            AddToolSet(StringTable._stringTable, ui); 
        }
        #endregion 

        #region ITool DIO
        public IToolDIO m_toolDIO = null;

        public string Get(ref DIO_I value, ModuleBase module, string id, bool bLog = true, bool bEnableRun = false)
        {
            if (value == null) value = new DIO_I(m_toolDIO, module.p_id + "." + id, bLog ? module.m_log : null, bEnableRun);
            string sInfo = value.RunTree(module.m_treeToolBox.GetTree(id));
            if (sInfo != "OK") return sInfo;
            module.m_listDI.AddBit(value.m_bitDI);
            return "OK";
        }

        public string Get(ref DIO_O value, ModuleBase module, string id, bool bLog = true, bool bEnableRun = false)
        {
            if (value == null) value = new DIO_O(m_toolDIO, module.p_id + "." + id, bLog ? module.m_log : null, bEnableRun);
            string sInfo = value.RunTree(module.m_treeToolBox.GetTree(id));
            if (sInfo != "OK") return sInfo;
            module.m_listDO.AddBit(value.m_bitDO);
            return "OK";
        }

        public string Get(ref DIO_Os value, ModuleBase module, string id, string[] asDO, bool bLog = true, bool bEnableRun = false)
        {
            if (value == null) value = new DIO_Os(m_toolDIO, module.p_id + "." + id, bLog ? module.m_log : null, bEnableRun, asDO);
            string sInfo = value.RunTree(module.m_treeToolBox.GetTree(id));
            if (sInfo != "OK") return sInfo;
            for (int n = 0; n < asDO.Length; n++) module.m_listDO.AddBit(value.m_aBitDO[n]);
            return "OK";
        }

        public string Get(ref DIO_IO value, ModuleBase module, string id, bool bLog = true, bool bEnableRun = false)
        {
            if (value == null) value = new DIO_IO(m_toolDIO, module.p_id + "." + id, bLog ? module.m_log : null, bEnableRun);
            string sInfo = value.RunTree(module.m_treeToolBox.GetTree(id));
            if (sInfo != "OK") return sInfo;
            module.m_listDI.AddBit(value.m_bitDI);
            module.m_listDO.AddBit(value.m_bitDO);
            return "OK";
        }

        public string Get(ref DIO_I2O2 value, ModuleBase module, string id, string sFalse, string sTrue, bool bLog = true, bool bEnableRun = false)
        {
            if (value == null) value = new DIO_I2O2(m_toolDIO, module.p_id + "." + id, bLog ? module.m_log : null, bEnableRun, sFalse, sTrue);
            string sInfo = value.RunTree(module.m_treeToolBox.GetTree(id));
            if (sInfo != "OK") return sInfo;
            module.m_listDI.AddBit(value.m_aBitDI[0]);
            module.m_listDI.AddBit(value.m_aBitDI[1]);
            module.m_listDO.AddBit(value.m_aBitDO[0]);
            module.m_listDO.AddBit(value.m_aBitDO[1]);
            return "OK";
        }
        #endregion

        #region ITool Axis
        public List<IAxis> m_toolAxis = null;

        public string Get(ref Axis value, ModuleBase module, string id)
        {
            if (value == null) value = new Axis(m_toolAxis, module.p_id + "." + id, module.m_log);
            string sInfo = value.RunTree(module.m_treeToolBox.GetTree(id));
            if (sInfo != "OK") return sInfo;
            module.m_listAxis.Add(value.p_axis);
            return "OK";
        }

        public string Get(ref AxisXY value, ModuleBase module, string id)
        {
            if (value == null) value = new AxisXY(m_toolAxis, module.p_id + "." + id, module.m_log);
            string sInfo = value.RunTree(module.m_treeToolBox.GetTree(id));
            if (sInfo != "OK") return sInfo;
            module.m_listAxis.Add(value.p_axisX);
            module.m_listAxis.Add(value.p_axisY);
            return "OK";
        }
        #endregion

        #region ITool Memory
        public MemoryTool m_memoryTool = null;
        MemoryTool_UI m_memoryToolUI = null;
        void InitMemoryTool()
        {
            m_memoryTool = new MemoryTool("Memory", m_engineer, false);
            m_memoryToolUI = new MemoryTool_UI();
            m_memoryToolUI.Init(m_memoryTool);
            AddToolSet(m_memoryTool, m_memoryToolUI);
        }

        public string Get(ref MemoryPool value, ModuleBase module, string id)
        {
            if (value == null)
            {
                value = m_memoryTool.GetPool(module.p_id + "." + id, true);
                module.m_aTool.Add(value);
            }
            value.RunTree(module.m_treeToolBox.GetTree(id));
            return "OK";
        }
        #endregion

        #region ITool MemoryViewer
        public string Get(ref MemoryViewer value, ModuleBase module, string id)
        {
            if (value == null)
            {
                value = new MemoryViewer(module.p_id + "." + id, m_memoryTool, module.m_log);
                module.m_aTool.Add(value);
            }
            return "OK";
        }
        #endregion

        #region ITool Inspect
        InspectToolSet m_inspectToolSet = null; 
        InspectToolSet_UI m_inspectToolSetUI = null;
        void InitInspectTool()
        {
            m_inspectToolSet = new InspectToolSet("Inspect", m_engineer);
            m_inspectToolSetUI = new InspectToolSet_UI();
            m_inspectToolSetUI.Init(m_inspectToolSet);
            AddToolSet(m_inspectToolSet, m_inspectToolSetUI);
        }

        public string Get(ref InspectTool value, ModuleBase module)
        {
            if (value == null)
            {
                value = m_inspectToolSet.GetInspect(module.p_id + ".Inspect");
                module.m_aTool.Add(value);
            }
            return "OK";
        }
        #endregion

        #region ITool Light
        LightToolSet m_lightToolSet = null;
        LightToolSet_UI m_lightToolSetUI = null;
        void InitLightTool()
        {
            m_lightToolSet = new LightToolSet("Light", m_engineer);
            m_lightToolSetUI = new LightToolSet_UI();
            m_lightToolSetUI.Init(m_lightToolSet);
            AddToolSet(m_lightToolSet, m_lightToolSetUI);
        }

        public string Get(ref LightSet value, ModuleBase module)
        {
            if (value == null)
            {
                value = new LightSet(m_lightToolSet, module.p_id, module.m_log);
                module.m_aTool.Add(value);
            }
            return value.RunTree(module.m_treeToolBox.GetTree("Light"));
        }
        #endregion

        #region ITool Camera
        ToolSetCamera m_toolSetCamera = null;
        ToolSetCamera_UI m_toolSetCameraUI = null;
        void InitCameraTool()
        {
            m_toolSetCamera = new ToolSetCamera("Camera");
            m_toolSetCameraUI = new ToolSetCamera_UI();
            m_toolSetCameraUI.Init(m_toolSetCamera);
            AddToolSet(m_toolSetCamera, m_toolSetCameraUI);
        }

        void InitCameraSet(ModuleBase module)
        {
            if (module.m_cameraSet != null) return;
            module.m_cameraSet = new CameraSet(m_toolSetCamera, module.p_id, module.m_log);
            module.m_aTool.Add(module.m_cameraSet);
        }

        public string Get(ref Camera_Basler value, ModuleBase module, string id)
        {
            if (value == null)
            {
                InitCameraSet(module);
                value = new Camera_Basler(module.p_id + "." + id, module.m_log);
                module.m_cameraSet.Add(value);
            }
            return "OK";
        }

        public string Get(ref Camera_Dalsa value, ModuleBase module, string id)
        {
            if (value == null)
            {
                try
                {
                    InitCameraSet(module);
                    value = new Camera_Dalsa(module.p_id + "." + id, module.m_log);
                    module.m_cameraSet.Add(value);
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.ToString());
                }
            }
            return "OK";
        }

        public string Get(ref Camera_CognexOCR value, ModuleBase module, string id)
        {
            if (value == null)
            {
                InitCameraSet(module);
                value = new Camera_CognexOCR(module.p_id + "." + id, module.m_log);
                module.m_cameraSet.Add(value);
            }
            return "OK";
        }
        #endregion

        #region ITool Comm
        ToolSet m_toolSetComm = null;
        public string Get(ref NamedPipe value, ModuleBase module, string id)
        {
            if (m_toolSetComm == null) m_toolSetComm = InitToolSet("Comm");
            if (value == null)
            {
                value = new NamedPipe(module.p_id + "." + id, module.m_log);
                m_toolSetComm.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }

        public string Get(ref TCPIPServer value, ModuleBase module, string id)
        {
            if (m_toolSetComm == null) m_toolSetComm = InitToolSet("Comm");
            if (value == null)
            {
                value = new TCPIPServer(module.p_id + "." + id, module.m_log);
                m_toolSetComm.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }

        public string Get(ref TCPIPClient value, ModuleBase module, string id)
        {
            if (m_toolSetComm == null) m_toolSetComm = InitToolSet("Comm");
            if (value == null)
            {
                value = new TCPIPClient(module.p_id + "." + id, module.m_log);
                m_toolSetComm.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }

        public string Get(ref DMCControl value, ModuleBase module, string id)
        {
            if (m_toolSetComm == null) m_toolSetComm = InitToolSet("Comm");
            if (value == null)
            {
                value = new DMCControl(module.p_id + "." + id, module.m_log);
                m_toolSetComm.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }

        public string Get(ref RS232 value, ModuleBase module, string id)
        {
            if (m_toolSetComm == null) m_toolSetComm = InitToolSet("Comm");
            if (value == null)
            {
                value = new RS232(module.p_id + "." + id, module.m_log);
                m_toolSetComm.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }

        public string Get(ref ZoomLens.ZoomLens value, ModuleBase module, string id)
        {
            if (m_toolSetComm == null) m_toolSetComm = InitToolSet("Comm");
            if (value == null)
            {
                value = new ZoomLens.ZoomLens(module.p_id + "." + id, module.m_log);
                m_toolSetComm.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }


        #endregion

        #region ITool OHT
        ToolSet m_toolSetOHT = null;

        public string Get(ref OHT_Semi value, ModuleBase module, GemCarrierBase carrier, string id)
        {
            if (m_toolSetOHT == null) m_toolSetOHT = InitToolSet("OHT");
            if (value == null)
            {
                value = new OHT_Semi(module.p_id + "." + id, module, carrier, m_toolDIO);
                m_toolSetOHT.AddTool(value);
                module.m_aTool.Add(value); 
            }
            value.RunTreeToolBox(module.m_treeToolBox.GetTree(id)); 
            return "OK";
        }

        public string Get(ref OHT_SSEM value, ModuleBase module, GemCarrierBase carrier, string id)
        {
            if (m_toolSetOHT == null) m_toolSetOHT = InitToolSet("OHT");
            if (value == null)
            {
                value = new OHT_SSEM(module.p_id + "." + id, module, carrier, m_toolDIO);
                m_toolSetOHT.AddTool(value);
                module.m_aTool.Add(value);
            }
            value.RunTreeToolBox(module.m_treeToolBox.GetTree(id));
            return "OK";
        }
        #endregion

        #region ITool Laser
        ToolSet m_toolSetLaser = null;
        public string Get(ref Laser_Bright value, ModuleBase module, string id)
        {
            if (m_toolSetLaser == null) m_toolSetLaser = InitToolSet("Laser");
            if (value == null)
            {
                value = new Laser_Bright(module.p_id + "." + id, module.m_log);
                m_toolSetLaser.AddTool(value);
                module.m_aTool.Add(value);
            }
            return value.GetTools(module);
        }
        #endregion

        #region IToolSet
        public Dictionary<IToolSet, UserControl> m_aToolSet = new Dictionary<IToolSet, UserControl>();
        public void AddToolSet(IToolSet toolSet, UserControl userControl)
        {
            m_aToolSet.Add(toolSet, userControl);
        }

        ToolSet InitToolSet(string sToolSet)
        {
            ToolSet toolSet = new ToolSet(sToolSet);
            ToolSet_UI toolSetUI = new ToolSet_UI();
            toolSetUI.Init(toolSet);
            AddToolSet(toolSet, toolSetUI);
            return toolSet;
        }
        #endregion

        string m_id;
        IEngineer m_engineer;
        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = engineer;
            SQLog.Init(engineer);
            InitStringTable(id); 
            InitMemoryTool();
            InitInspectTool();
            InitLightTool();
            InitCameraTool();
        }

        public void ThreadStop()
        {
            foreach (IToolSet tool in m_aToolSet.Keys)
            {
                tool.ThreadStop();
            }
        }
    }
}
