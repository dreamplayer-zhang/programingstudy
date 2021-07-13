using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.CognexDM150;
using RootTools.Camera.CognexOCR;
using RootTools.Camera.Dalsa;
using RootTools.Camera.Matrox;
using RootTools.Camera.Silicon;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Gem;
using RootTools.Gem.XGem;
using RootTools.Inspects;
using RootTools.Lens;
using RootTools.Lens.LinearTurret;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.OHT.Semi;
using RootTools.OHT.SSEM;
using RootTools.OHTNew;
using RootTools.ParticleCounter;
using RootTools.Printer;
using RootTools.RADS;
using RootTools.RFIDs;
using RootTools.RTC5s.LaserBright;
using RootTools.SQLogs;
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

        public string GetDIO(ref DIO_I value, ModuleBase module, string id, bool bLog = true, bool bEnableRun = false)
        {
            if (value == null) value = new DIO_I(m_toolDIO, module.p_id + "." + id, bLog ? module.m_log : null, bEnableRun);
            string sInfo = value.RunTree(module.m_treeRootTool.GetTree(id));
            if (sInfo != "OK") return sInfo;
            module.m_listDI.AddBit(value.m_bitDI);
            return "OK";
        }

        public string GetDIO(ref DIO_O value, ModuleBase module, string id, bool bLog = true, bool bEnableRun = false)
        {
            if (value == null) value = new DIO_O(m_toolDIO, module.p_id + "." + id, bLog ? module.m_log : null, bEnableRun);
            string sInfo = value.RunTree(module.m_treeRootTool.GetTree(id));
            if (sInfo != "OK") return sInfo;
            module.m_listDO.AddBit(value.m_bitDO);
            return "OK";
        }

        public string GetDIO(ref DIO_Is value, ModuleBase module, string id, string[] asDI, bool bLog = true, bool bEnableRun = false)
        {
            if (value == null) value = new DIO_Is(m_toolDIO, module.p_id + "." + id, bLog ? module.m_log : null, bEnableRun, asDI);
            string sInfo = value.RunTree(module.m_treeRootTool.GetTree(id));
            if (sInfo != "OK") return sInfo;
            for (int n = 0; n < asDI.Length; n++) module.m_listDI.AddBit(value.m_aBitDI[n]);
            return "OK";
        }

        public string GetDIO(ref DIO_Is value, ModuleBase module, string id, string sDI, int nCount, bool bLog = true, bool bEnableRun = false)
        {
            if (value == null) value = new DIO_Is(m_toolDIO, module.p_id + "." + id, bLog ? module.m_log : null, bEnableRun, sDI, nCount);
            string sInfo = value.RunTree(module.m_treeRootTool.GetTree(id));
            if (sInfo != "OK") return sInfo;
            for (int n = 0; n < nCount; n++) module.m_listDI.AddBit(value.m_aBitDI[n]);
            return "OK";
        }

        public string GetDIO(ref DIO_Os value, ModuleBase module, string id, string[] asDO, bool bLog = true, bool bEnableRun = false)
        {
            if (value == null) value = new DIO_Os(m_toolDIO, module.p_id + "." + id, bLog ? module.m_log : null, bEnableRun, asDO);
            string sInfo = value.RunTree(module.m_treeRootTool.GetTree(id));
            if (sInfo != "OK") return sInfo;
            for (int n = 0; n < value.m_aBitDO.Count; n++) module.m_listDO.AddBit(value.m_aBitDO[n]);
            return "OK";
        }

        public string GetDIO(ref DIO_IO value, ModuleBase module, string id, bool bLog = true, bool bEnableRun = false)
        {
            if (value == null) value = new DIO_IO(m_toolDIO, module.p_id + "." + id, bLog ? module.m_log : null, bEnableRun);
            string sInfo = value.RunTree(module.m_treeRootTool.GetTree(id));
            if (sInfo != "OK") return sInfo;
            module.m_listDI.AddBit(value.m_bitDI);
            module.m_listDO.AddBit(value.m_bitDO);
            return "OK";
        }

        public string GetDIO(ref DIO_I2O value, ModuleBase module, string id, string sFalse, string sTrue, bool bLog = true, bool bEnableRun = false)
        {
            if (value == null) value = new DIO_I2O(m_toolDIO, module.p_id + "." + id, bLog ? module.m_log : null, bEnableRun, sFalse, sTrue);
            string sInfo = value.RunTree(module.m_treeRootTool.GetTree(id));
            if (sInfo != "OK") return sInfo;
            module.m_listDI.AddBit(value.m_aBitDI[0]);
            module.m_listDI.AddBit(value.m_aBitDI[1]);
            module.m_listDO.AddBit(value.m_bitDO);
            return "OK";
        }

        public string GetDIO(ref DIO_I4O value, ModuleBase module, string id, string sFalse, string sTrue, bool bLog = true, bool bEnableRun = false)
        {
            if (value == null) value = new DIO_I4O(m_toolDIO, module.p_id + "." + id, bLog ? module.m_log : null, bEnableRun, sFalse, sTrue);
            string sInfo = value.RunTree(module.m_treeRootTool.GetTree(id));
            if (sInfo != "OK") return sInfo;
            module.m_listDI.AddBit(value.m_aBitDIA[0]);
            module.m_listDI.AddBit(value.m_aBitDIB[0]);
            module.m_listDI.AddBit(value.m_aBitDIA[1]);
            module.m_listDI.AddBit(value.m_aBitDIB[1]);
            module.m_listDO.AddBit(value.m_bitDO);
            return "OK";
        }

        public string GetDIO(ref DIO_I2O2 value, ModuleBase module, string id, string sFalse, string sTrue, bool bLog = true, bool bEnableRun = false)
        {
            if (value == null) value = new DIO_I2O2(m_toolDIO, module.p_id + "." + id, bLog ? module.m_log : null, bEnableRun, sFalse, sTrue);
            string sInfo = value.RunTree(module.m_treeRootTool.GetTree(id));
            if (sInfo != "OK") return sInfo;
            module.m_listDI.AddBit(value.m_aBitDI[0]);
            module.m_listDI.AddBit(value.m_aBitDI[1]);
            module.m_listDO.AddBit(value.m_aBitDO[0]);
            module.m_listDO.AddBit(value.m_aBitDO[1]);
            return "OK";
        }
        #endregion

        #region ITool Axis
        public string GetAxis(ref Axis value, ModuleBase module, string id)
        {
            if (value == null) value = m_control.GetAxis(module.p_id + "." + id, module.m_log);
            module.m_listAxis.Add(value);
            return "OK";
        }

        public string GetAxis(ref AxisXY value, ModuleBase module, string id)
        {
            if (value == null) value = m_control.GetAxisXY(module.p_id + "." + id, module.m_log);
            module.m_listAxis.Add(value.p_axisX);
            module.m_listAxis.Add(value.p_axisY);
            return "OK";
        }

        public string GetAxis(ref Axis3D value, ModuleBase module, string id)
        {
            if (value == null) value = m_control.GetAxis3D(module.p_id + "." + id, module.m_log);
            module.m_listAxis.Add(value.p_axisX);
            module.m_listAxis.Add(value.p_axisY);
            module.m_listAxis.Add(value.p_axisZ);
            return "OK";
        }
        #endregion

        #region ITool Memory
        public MemoryTool m_memoryTool = null;
        MemoryTool_UI m_memoryToolUI = null;
        void InitMemoryTool()
        {
            m_memoryTool = new MemoryTool("Memory", m_engineer);
            m_memoryToolUI = new MemoryTool_UI();
            m_memoryToolUI.Init(m_memoryTool);
            AddToolSet(m_memoryTool, m_memoryToolUI);
        }

        public string Get(ref MemoryPool value, ModuleBase module, string id, double fGB)
        {
            if (value == null)
            {
                value = m_memoryTool.CreatePool(module.p_id + "." + id, fGB);
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
            return value.RunTree(module.m_treeRootTool.GetTree("Light"));
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

        public string GetCamera(ref CameraBasler value, ModuleBase module, string id)
        {
            if (value == null)
            {
                InitCameraSet(module);
                value = new CameraBasler(module.p_id + "." + id, module.m_log, m_memoryTool);
                module.m_cameraSet.Add(value);
            }
            return "OK";
        }

        public string GetCamera(ref CameraDalsa value, ModuleBase module, string id)
        {
            if (value == null)
            {
                InitCameraSet(module);
                value = new CameraDalsa(module.p_id + "." + id, module.m_log, m_memoryTool);
                module.m_cameraSet.Add(value);
            }
            return "OK";
        }

        public string GetCamera(ref Camera_Basler value, ModuleBase module, string id)
        {
            if (value == null)
            {
				try
				{
                    InitCameraSet(module);
                    value = new Camera_Basler(module.p_id + "." + id, module.m_log);
                    module.m_cameraSet.Add(value);
				}
                catch (Exception ee)
				{
                    MessageBox.Show(ee.ToString());
                }
            }
            return "OK";
        }

        public string GetCamera(ref Camera_Dalsa value, ModuleBase module, string id)
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

        public string GetCamera(ref Camera_Matrox value, ModuleBase module, string id)
        {
            if (value == null)
            {
                try
                {
                    InitCameraSet(module);
                    value = new Camera_Matrox(module.p_id + "." + id, module.m_log);
                    module.m_cameraSet.Add(value);
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.ToString());
                }
            }
            return "OK";
        }

        public string GetCamera(ref Camera_Silicon value, ModuleBase module, string id)
        {
            if (value == null)
            {
                try
                {
                    InitCameraSet(module);
                    value = new Camera_Silicon(module.p_id + "." + id, module.m_log);
                    module.m_cameraSet.Add(value);
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.ToString());
                }
            }
            return "OK";
        }

        public string GetCamera(ref Camera_CognexOCR value, ModuleBase module, string id)
        {
            if (value == null)
            {
                InitCameraSet(module);
                value = new Camera_CognexOCR(module.p_id + "." + id, module.m_log);
                module.m_cameraSet.Add(value);
            }
            return "OK";
        }

        public string GetCamera(ref Camera_CognexDM150 value, ModuleBase module, string id)
        {
            if (value == null)
            {
                InitCameraSet(module);
                value = new Camera_CognexDM150(module.p_id + "." + id, module.m_log);
                module.m_cameraSet.Add(value);
            }
            return "OK";
        }
        #endregion

        #region ITool Lens
        ToolSetLens m_toolSetLens = null;
        ToolSetLens_UI m_toolSetLensUI = null;
        void InitLensTool()
        {
            m_toolSetLens = new ToolSetLens("Lens");
            m_toolSetLensUI = new ToolSetLens_UI();
            m_toolSetLensUI.Init(m_toolSetLens);
            AddToolSet(m_toolSetLens, m_toolSetLensUI);
        }

        void InitLensSet(ModuleBase module)
        {
            if (module.m_lensSet != null) return;
            module.m_lensSet = new LensSet(m_toolSetLens, module.p_id, module.m_log);
            module.m_aTool.Add(module.m_lensSet);
        }

        public string Get(ref LensLinearTurret value, ModuleBase module, string id)
        {
            if (value == null)
            {
                InitLensSet(module);
                value = new LensLinearTurret(module.p_id + "." + id, module.m_log);
                module.m_lensSet.Add(value);
            }
            return "OK";
        }
        #endregion

        #region ITool RFID
        ToolSet m_toolSetRFID = null; 
        public string Get(ref RFID value, ModuleBase module, string id)
        {
            if (m_toolSetRFID == null) m_toolSetRFID = InitToolSet("RFID");
            if (value == null)
            {
                value = new RFID(module.p_id + "." + id, module.m_log);
                m_toolSetRFID.AddTool(value);
                module.m_aTool.Add(value); 
            }
            return "OK";
        }
        #endregion

        #region ITool Comm
        ToolSet m_toolSetComm = null;
        public string GetComm(ref NamedPipe value, ModuleBase module, string id)
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

        public string GetComm(ref TCPIPServer value, ModuleBase module, string id)
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

        public string GetComm(ref TCPIPClient value, ModuleBase module, string id)
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

        public string GetComm(ref TCPAsyncServer value, ModuleBase module, string id, int lMaxBuffer = 4096)
        {
            if (m_toolSetComm == null) m_toolSetComm = InitToolSet("Comm");
            if (value == null)
            {
                value = new TCPAsyncServer(module.p_id + "." + id, module.m_log, lMaxBuffer);
                m_toolSetComm.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }

        public string GetComm(ref TCPAsyncClient value, ModuleBase module, string id, int lMaxBuffer = 4096)
        {
            if (m_toolSetComm == null) m_toolSetComm = InitToolSet("Comm");
            if (value == null)
            {
                value = new TCPAsyncClient(module.p_id + "." + id, module.m_log, lMaxBuffer);
                m_toolSetComm.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }

        public string GetComm(ref TCPSyncServer value, ModuleBase module, string id, int lMaxBuffer = 4096)
        {
            if (m_toolSetComm == null) m_toolSetComm = InitToolSet("Comm");
            if (value == null)
            {
                value = new TCPSyncServer(module.p_id + "." + id, module.m_log, lMaxBuffer);
                m_toolSetComm.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }

        public string GetComm(ref TCPSyncClient value, ModuleBase module, string id, int lMaxBuffer = 4096)
        {
            if (m_toolSetComm == null) m_toolSetComm = InitToolSet("Comm");
            if (value == null)
            {
                value = new TCPSyncClient(module.p_id + "." + id, module.m_log, lMaxBuffer);
                m_toolSetComm.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }

        public string GetComm(ref RS232 value, ModuleBase module, string id)
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

        public string GetComm(ref RS232byte value, ModuleBase module, string id)
        {
            if (m_toolSetComm == null) m_toolSetComm = InitToolSet("Comm");
            if (value == null)
            {
                value = new RS232byte(module.p_id + "." + id, module.m_log);
                m_toolSetComm.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }

        public string GetComm(ref Modbus value, ModuleBase module, string id)
        {
            if (m_toolSetComm == null) m_toolSetComm = InitToolSet("Comm");
            if (value == null)
            {
                value = new Modbus(module.p_id + "." + id, module.m_log);
                m_toolSetComm.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }
        #endregion

        #region ZoomLens
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

        public string GetOHT(ref OHT_Semi value, ModuleBase module, GemCarrierBase carrier, string id)
        {
            if (m_toolSetOHT == null) m_toolSetOHT = InitToolSet("OHT");
            if (value == null)
            {
                value = new OHT_Semi(module.p_id + "." + id, module, carrier, m_toolDIO);
                m_toolSetOHT.AddTool(value);
                module.m_aTool.Add(value); 
            }
            value.RunTreeToolBox(module.m_treeRootTool.GetTree(id)); 
            return "OK";
        }

        public string GetOHT(ref OHT_SSEM value, ModuleBase module, GemCarrierBase carrier, string id)
        {
            if (m_toolSetOHT == null) m_toolSetOHT = InitToolSet("OHT");
            if (value == null)
            {
                value = new OHT_SSEM(module.p_id + "." + id, module, carrier, m_toolDIO);
                m_toolSetOHT.AddTool(value);
                module.m_aTool.Add(value);
            }
            value.RunTreeToolBox(module.m_treeRootTool.GetTree(id));
            return "OK";
        }

        public string GetOHT(ref OHTNew.OHT value, ModuleBase module, GemCarrierBase carrier, string id)
        {
            if (m_toolSetOHT == null) m_toolSetOHT = InitToolSet("OHT");
            if (value == null)
            {
                value = new OHTNew.OHT(module.p_id + "." + id, module, carrier, m_toolDIO);
                m_toolSetOHT.AddTool(value);
                module.m_aTool.Add(value);
            }
            value.RunTreeToolBox(module.m_treeRootTool.GetTree(id));
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

        #region ITool Printer
        ToolSet m_toolSetPrinter = null;
        public string Get(ref SRP350 value, ModuleBase module, string id)
        {
            if (m_toolSetPrinter == null) m_toolSetPrinter = InitToolSet("Printer");
            if (value == null)
            {
                value = new SRP350(module.p_id + "." + id, module.m_log);
                m_toolSetPrinter.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK"; 
        }
        #endregion

        #region ITool ParticleCounter
        ToolSet m_toolSetParticleCounter = null;
        public string Get(ref LasairIII value, ModuleBase module, string id)
        {
            if (m_toolSetParticleCounter == null) m_toolSetParticleCounter = InitToolSet("ParticleCounter");
            if (value == null)
            {
                value = new LasairIII(module.p_id + "." + id, module.m_log);
                m_toolSetParticleCounter.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }

        public string Get(ref AirnetII_310 value, ModuleBase module, string id)
        {
            if (m_toolSetParticleCounter == null) m_toolSetParticleCounter = InitToolSet("ParticleCounter");
            if (value == null)
            {
                value = new AirnetII_310(module.p_id + "." + id, module.m_log);
                m_toolSetParticleCounter.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }
        #endregion

        #region ITool RADS
        ToolSet m_toolSetRADS = null;
        public string Get(ref RADSControl value, ModuleBase module, string id, bool bUseRADS)
        {
            if (m_toolSetRADS == null) m_toolSetRADS = InitToolSet("RADSControl");
            if (value == null)
            {
                value = new RADSControl(module.p_id + "." + id, module.m_log, bUseRADS);
                m_toolSetRADS.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }
        #endregion

        #region ITool TK4S
        ToolSet m_toolTK4S = null;
        public string Get(ref TK4SGroup value, ModuleBase module, string id , IDialogService dialogService = null)
        {
            if (m_toolTK4S == null)
                m_toolTK4S = InitToolSet("TK4S");
            if (value == null)
            {
                value = new TK4SGroup(module.p_id + "." + id, module.m_log, dialogService);
                m_toolTK4S.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }
        #endregion

        #region ITool FFU
        //ToolSet m_toolFFU = null;
        public string Get(ref FFU_Group value, ModuleBase module, string id, IDialogService dialogService = null)
        {
            if (m_toolTK4S == null)
                m_toolTK4S = InitToolSet("FFU");
            if (value == null)
            {
                value = new FFU_Group(module.p_id + "." + id, module.m_log, dialogService);
                m_toolTK4S.AddTool(value);
                module.m_aTool.Add(value);
            }
            return "OK";
        }
        #endregion


        #region IToolSet
        public Dictionary<IToolSet, UserControl> m_aToolSet = new Dictionary<IToolSet, UserControl>();
        public void AddToolSet(IToolSet toolSet, UserControl userControl)
        {
            m_aToolSet.Add(toolSet, userControl);
        }

        public IToolSet GetToolSet(string sName)
        {
            foreach (KeyValuePair<IToolSet, UserControl> kv in m_aToolSet)
            {
                if (kv.Key.p_id == sName)
                {
                    return kv.Key;
                }
            }
            return null;
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
        IControl m_control; 
        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = engineer;
            m_control = engineer.ClassControl(); 
            SQLog.Init(engineer);
            InitStringTable(id); 
            InitMemoryTool();
            InitInspectTool();
            InitLightTool();
            InitCameraTool();
            InitLensTool(); 
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
