using Root_CAMELLIA.Data;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Light;
using RootTools.Module;
using RootTools.Trees;
using System;
using Met = Root_CAMELLIA.LibSR_Met;
using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.Structure;
using RootTools.ImageProcess;
using RootTools.Camera.Dalsa;
using RootTools.Control.Ajin;
using RootTools.Inspects;
using RootTools.Memory;
using RootTools.RADS;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Diagnostics;
using Root_EFEM.Module;
using Root_EFEM;
using static RootTools.Control.Axis;
using RootTools.GAFs;
using RootTools.OHTNew;
using RootTools.Gem;

namespace Root_CAMELLIA.Module
{
    public class Module_Camellia : ModuleBase, IWTRChild
    {
        public DataManager m_DataManager;
        public MainWindow_ViewModel mwvm;
        
        InfoCarrier[] infoCarrier = new InfoCarrier[2];
        Log m_camelliaLog;
        #region ToolBox

        AxisXY m_axisXY;
        public AxisXY p_axisXY
        {
            get
            {
                return m_axisXY;
            }
            set
            {
                m_axisXY = value;
            }
        }

        Axis m_axisZ;
        public Axis p_axisZ
        {
            get
            {
                return m_axisZ;
            }
            set
            {
                m_axisZ = value;
            }
        }
        Axis m_axisLifter;
        public Axis p_axisLifter
        {
            get
            {
                return m_axisLifter;
            }
            set
            {
                m_axisLifter = value;
            }
        }

        AxisXY m_tiltAxisXY;
        public AxisXY p_tiltAxisXY
        {
            get
            {
                return m_tiltAxisXY;
            }
            set
            {
                m_tiltAxisXY = value;
            }
        }

        Axis m_stageAxisZ;
        public Axis p_stageAxisZ
        {
            get
            {
                return m_stageAxisZ;
            }
            set
            {
                m_stageAxisZ = value;
            }
        }

        private bool _bLock = false;
        public bool p_bLock
        {
            get
            {
                return _bLock;
            }
            set
            {
                if (_bLock == value)
                    return;
                _bLock = value;
            }
        }

        private string _dataSavePath = "";
        public string p_dataSavePath
        {
            get
            {
                return _dataSavePath;
            }
            set
            {
                _dataSavePath = value;
            }
        }

        private string _dataSavePathDate = "";
        public string p_dataSavePathDate
        {
            get
            {
                return _dataSavePathDate;
            }
            set
            {
                _dataSavePathDate = value;
            }
        }

        public IGem p_xGem { get; private set; }

        #region InfoWafer
        string m_sInfoWafer = "";
        InfoWafer _infoWafer = null;
        public InfoWafer p_infoWafer
        {
            get
            {
                return _infoWafer;
            }
            set
            {
                m_sInfoWafer = (value == null) ? "" : value.p_id;
                _infoWafer = value;
                if (m_reg != null)
                    m_reg.Write("sInfoWafer", m_sInfoWafer);
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadInfoWafer_Registry()
        {
            m_reg = new Registry(p_id + ".InfoWafer");
            m_sInfoWafer = m_reg.Read("sInfoWafer", m_sInfoWafer);
            p_infoWafer = m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
        }
        #endregion

        #region InfoWafer UI
        InfoWaferChild_UI m_ui;
        void InitInfoWaferUI()
        {
            m_ui = new InfoWaferChild_UI();
            m_ui.Init(this);
            m_aTool.Add(m_ui);
        }
        #endregion

        public List<string> p_asChildSlot
        {
            get
            {
                return null;
            }
        }

        InfoWafer.WaferSize m_waferSize;

        DIO_I m_axisXReady;
        DIO_I m_axisYReady;
        DIO_I m_vacuum;
        DIO_I m_homeExistWafer;
        DIO_I m_loadExistWafer;
        DIO_O m_vacuumOnOff;
        DIO_I m_axisLifterHome1;
        DIO_I m_axisLifterHome2;
        DIO_I m_axisLifterHome3;

        private Camera_Basler m_CamVRS;
        public Camera_Basler p_CamVRS
        {
            get
            {
                return m_CamVRS;
            }
            set
            {
                m_CamVRS = value;
            }
        }
        

        #region Light
        public LightSet m_lightSet;
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

        public void SetLight(bool bOn)
        {
            for (int n = 0; n < m_lightSet.m_aLight.Count; n++)
            {
                m_lightSet.m_aLight[n].m_light.p_fSetPower = bOn ? m_aLightPower[n] : 0;
            }
        }
        #endregion

        #region Axis WorkPoint
        public enum eAxisPos
        {
            Ready,
            Home,
            InitCal
        }
        private void InitWorkPoint()
        {
            m_axisXY.p_axisX.AddPos(Enum.GetNames(typeof(eAxisPos)));
            m_axisXY.p_axisY.AddPos(Enum.GetNames(typeof(eAxisPos)));
            m_axisZ.AddPos(Enum.GetNames(typeof(eAxisPos)));
            m_axisLifter.AddPos(Enum.GetNames(typeof(eAxisPos)));
            m_axisLifter.AddIO(m_axisXReady);
            m_axisLifter.AddIO(m_axisYReady);
            m_stageAxisZ.AddPos(Enum.GetNames(typeof(eAxisPos)));
            m_tiltAxisXY.p_axisX.AddPos(Enum.GetNames(typeof(eAxisPos)));
            m_tiltAxisXY.p_axisY.AddPos(Enum.GetNames(typeof(eAxisPos)));

            //m_axisXY.p_axisX.AddIO(m_axisLifterHome1);
            //m_axisXY.p_axisX.AddIO(m_axisLifterHome2);
            //m_axisXY.p_axisX.AddIO(m_axisLifterHome3);

            //m_axisXY.p_axisY.AddIO(m_axisLifterHome1);
            //m_axisXY.p_axisY.AddIO(m_axisLifterHome2);
            //m_axisXY.p_axisY.AddIO(m_axisLifterHome3);
            //m_axisLifter.AddIO(m_vaccum);
            m_axisLifter.p_vaccumDIO_I = m_vacuum;
        }
        #endregion



        #endregion

        ALID m_alid_WaferExist;
        public void SetAlarm()
        {
            if (m_homeExistWafer.p_bIn)
                m_alid_WaferExist.Run(true, "Vision Home Position Wafer Exist");
            else if (p_infoWafer != null && !m_homeExistWafer.p_bIn)
                m_alid_WaferExist.Run(true, "Vision Home Position Wafer Not Exist");
            else if (m_loadExistWafer.p_bIn)
                m_alid_WaferExist.Run(true, "Vision Load Position Wafer Exist");
            else if (p_infoWafer != null && !m_loadExistWafer.p_bIn)
                m_alid_WaferExist.Run(true, "Vision Load Position Wafer Not Exist!");
        }

        public void SetAlarmLoad()
        {
            if (m_loadExistWafer.p_bIn)
                m_alid_WaferExist.Run(true, "Vision Load Position Wafer Exist");
            else if (p_infoWafer != null && !m_loadExistWafer.p_bIn)
                m_alid_WaferExist.Run(true, "Vision Load Position Wafer Not Exist!");
        }


        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetAxis(ref m_axisXY, this, "StageXY");
            p_sInfo = m_toolBox.GetAxis(ref m_axisZ, this, "NavigationZ");
            p_sInfo = m_toolBox.GetAxis(ref m_axisLifter, this, "StageLifter");
            p_sInfo = m_toolBox.GetAxis(ref m_tiltAxisXY, this, "TiltXY");
            p_sInfo = m_toolBox.GetAxis(ref m_stageAxisZ, this, "StageZ");
            p_sInfo = m_toolBox.GetCamera(ref m_CamVRS, this, "VRS");
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            p_sInfo = m_toolBox.GetDIO(ref m_axisXReady, this, "Stage X Ready");
            p_sInfo = m_toolBox.GetDIO(ref m_axisYReady, this, "Stage Y Ready");
            p_sInfo = m_toolBox.GetDIO(ref m_axisLifterHome1, this, "Lifter 1 Home");
            p_sInfo = m_toolBox.GetDIO(ref m_axisLifterHome2, this, "Lifter 2 Home");
            p_sInfo = m_toolBox.GetDIO(ref m_axisLifterHome3, this, "Lifter 3 Home");
            p_sInfo = m_toolBox.GetDIO(ref m_vacuum, this, "Vaccum On");
            p_sInfo = m_toolBox.GetDIO(ref m_vacuumOnOff, this, "Vaccum OnOff");
            p_sInfo = m_toolBox.GetDIO(ref m_homeExistWafer, this, "Home Wafer Exist");
            p_sInfo = m_toolBox.GetDIO(ref m_loadExistWafer, this, "Load Wafer Exist");
            m_alid_WaferExist = m_gaf.GetALID(this, "Vision Wafer Exist", "Vision Wafer Exist");

        }
        public Module_Camellia(string id, IEngineer engineer, List<ILoadport> loadports)
        {
            m_camelliaLog = LogView.GetLog(id, id);
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            base.InitBase(id, engineer);
            InitWorkPoint();
            InitInfoWaferUI();
            m_DataManager = DataManager.Instance;
            
            for (int i = 0; i < loadports.Count; i++)
            {
                infoCarrier[i] = loadports[i].p_infoCarrier;
                CanInitCal[i] = false;
                CheckDocking[i] = false;
            }

            p_xGem = App.m_engineer.m_handler.m_gem;

        }



        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            AddModuleRunList(new Run_InitCalibration(this), true, "InitCalCentering");
            AddModuleRunList(new Run_CalibrationWaferCentering(this), true, "Background Calibration_Centering");
            AddModuleRunList(new Run_Measure(this), true, "Measurement");
            AddModuleRunList(new Run_PMReflectance (this), true, "PM Reflectance");
            AddModuleRunList(new Run_PMThickness (this), true, "PM Thickness");
            AddModuleRunList(new Run_PMSensorStageAlign (this), true, "PM Sensor_Stage Align");
            AddModuleRunList(new Run_PMSensorCameraTilt (this), true, "PM Sensor_Camera Tilt");
        }

        public bool p_isClearInfoWafer { get; set; } = false;
        public override string StateHome()
        {
            p_isClearInfoWafer = true;
            p_sInfo = "OK";
            if (EQ.p_bSimulate)
                return "OK";

            m_tiltAxisXY.p_axisX.p_eState = Axis.eState.Ready;
            m_tiltAxisXY.p_axisY.p_eState = Axis.eState.Ready;
            m_stageAxisZ.p_eState = Axis.eState.Ready;

            Thread.Sleep(200);
            if (m_listAxis.Count == 0) return "OK";
            if (p_eState == eState.Run) return "Invalid State : Run";
            if (EQ.IsStop()) return "Home Stop";

            foreach (Axis axis in m_listAxis)
            {
                if (axis != null) axis.ServoOn(true);
            }
            Thread.Sleep(200);
            if (EQ.IsStop()) return "Home Stop";

            for (int i = 0; i < p_axisLifter.m_bDIO_I.Count; i++)
            {
                p_axisLifter.m_bDIO_I[i] = false;
            }
            if (!LifterMoveVacuumCheck())
            {
                p_eState = eState.Error;
                p_sInfo = "Vacuum is not turn off";
                MessageBox.Show(p_sInfo);
                return p_sInfo;
            }
            p_axisLifter.StartHome();
            if (p_axisLifter.WaitReady() != "OK")
            {
                p_eState = eState.Error;
                p_sInfo = "Lifter Home Error";
                MessageBox.Show(p_sInfo);
                return p_sInfo;
            }

            for (int i = 0; i < p_axisLifter.m_bDIO_I.Count; i++)
            {
                p_axisLifter.m_bDIO_I[i] = true;
            }


            p_axisXY.p_axisX.StartHome();
            p_axisXY.p_axisY.StartHome();
            p_axisZ.StartHome();

            if (p_axisXY.p_axisX.WaitReady() != "OK")
            {
                p_eState = eState.Error;
                return "AxisX Home Error";
            }

            if (p_axisXY.p_axisY.WaitReady() != "OK")
            {
                p_eState = eState.Error;
                return "AxisY Home Error";
            }

            if (p_axisZ.WaitReady() != "OK")
            {
                p_eState = eState.Error;
                return "AxisZ Home Error";
            }

            p_stageAxisZ.StartHome();

            if(p_stageAxisZ.WaitReady() != "OK")
            {
                p_eState = eState.Error;
                return "Axis StageZ Home Error";
            }

            p_stageAxisZ.StartMove(eAxisPos.Ready);

            if(p_stageAxisZ.WaitReady() != "OK")
            {
                p_eState = eState.Error;
                return "Axis StageZ Move Ready Error";
            }

            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;

            return p_sInfo;
        }


        bool[] CanInitCal = new bool[2];
        bool[] CheckDocking = new bool[2];
        bool m_InitCalDone = false;
        protected override void RunThread()
        {
            base.RunThread();
            if (m_bUseInitCal)
            {
                if (EQ.p_eState == EQ.eState.Run && !EQ.p_bRecovery)
                {
                    if (!CanInitCal[EQ.p_nRunLP] && infoCarrier[EQ.p_nRunLP].p_eState == InfoCarrier.eState.Dock)
                    {
                        CanInitCal[EQ.p_nRunLP] = true;

                        if (((Run_InitCalibration)CloneModuleRun("InitCalibration")).Run() != "OK")
                        {
                            p_sInfo = "Init Cal Error";
                            m_InitCalDone = false;
                            CanInitCal[EQ.p_nRunLP] = false;
                        }
                        else
                        {
                            MoveReadyPos();
                            m_InitCalDone = true;
                        }
                    }
                    else if (CanInitCal[EQ.p_nRunLP] && infoCarrier[EQ.p_nRunLP].p_eState != InfoCarrier.eState.Dock)
                    {
                        CanInitCal[EQ.p_nRunLP] = false;
                        m_InitCalDone = false;
                    }
                }
            }
            else
            {
                if (EQ.p_eState == EQ.eState.Run && !EQ.p_bRecovery)
                {
                    if (!CheckDocking[EQ.p_nRunLP] && infoCarrier[EQ.p_nRunLP].p_eState == InfoCarrier.eState.Dock)
                    {
                        CheckDocking[EQ.p_nRunLP] = true;
                        MoveReadyPos();
                    }
                    else if (CheckDocking[EQ.p_nRunLP] && infoCarrier[EQ.p_nRunLP].p_eState != InfoCarrier.eState.Dock)
                    {
                        CheckDocking[EQ.p_nRunLP] = false;
                    }
                }
            }
        }

        public string LifterDown()
        {
            try
            {
                if (p_axisLifter.IsInPos(eAxisPos.Home))
                {
                    if (!m_vacuum.p_bIn)
                    {
                        VaccumOnOff(true);
                    }
                    return "OK";
                }
                else
                {
                    if (!m_vacuum.p_bIn)
                    {
                        VaccumOnOff(true);
                    }
                    //p_axisLifter.p_vaccumDIO_I.p_bIn = false;
                    p_axisLifter.p_IsLifterDown = true;
                    MarsLogManager.Instance.WriteFNC(EQ.p_nRunLP, BaseDefine.LOG_DEVICE_ID, "Lifter Down", SSLNet.STATUS.START);
                    if (p_axisLifter.StartMove(eAxisPos.Home) != "OK")
                    {
                        return p_sInfo;
                    }
                    if (p_axisLifter.WaitReady() != "OK")
                        return p_sInfo;
                    MarsLogManager.Instance.WriteFNC(EQ.p_nRunLP, BaseDefine.LOG_DEVICE_ID, "Lifter Down", SSLNet.STATUS.END);
                }
            }
            finally
            {
                p_axisLifter.p_IsLifterDown = false;
            }
          

           // if (m_loadExistWafer.p_bIn)
            {
                
            }
            //else
            //{
            //    p_sInfo = p_id + " Wafer Not Exist Error";
            //    return p_sInfo;
            //}

            //if (LifterMoveVacuumCheck())
            //{
            //    if (!m_vacuum.p_bIn)
            //    {
            //        if (p_axisLifter.StartMove(eAxisPos.Ready) != "OK")
            //        {
            //            return p_sInfo;
            //        }
            //        if (p_axisLifter.WaitReady() != "OK")
            //            return p_sInfo;
            //    }
            //    else
            //    {
            //        p_sInfo = p_id + " Vacuum is not turn off";
            //        return p_sInfo;
            //    }
            //}
            //else
            //{
            //    p_sInfo = p_id + " Vacuum is not turn off";
            //    return p_sInfo;
            //}

            //if (!m_vacuum.p_bIn)
            //{
            //    VaccumOnOff(true);
            //}

            return "OK";
        }

        public string LifterUp()
        {

            if (LifterMoveVacuumCheck())
            {
                if (!m_vacuum.p_bIn)
                {
                    if (p_axisLifter.IsInPos(eAxisPos.Ready))
                    {
                        return "OK";
                    }
                    MarsLogManager.Instance.WriteFNC(EQ.p_nRunLP, BaseDefine.LOG_DEVICE_ID, "Lifter Up", SSLNet.STATUS.START);
                    if (Run(p_axisLifter.StartMove(eAxisPos.Ready)))
                    {
                        return p_sInfo;
                    }
                    if (Run(p_axisLifter.WaitReady()))
                        return p_sInfo;
                    MarsLogManager.Instance.WriteFNC(EQ.p_nRunLP, BaseDefine.LOG_DEVICE_ID, "Lifter Up", SSLNet.STATUS.END);
                }
                else
                {
                    p_sInfo = p_id + " Vacuum is not turn off";
                    return p_sInfo;
                }
            }
            else
            {
                p_sInfo = p_id + " Vacuum is not turn off";
                return p_sInfo;
            }
            return "OK";
        }

        public bool LifterMoveVacuumCheck()
        {
            if (m_vacuum.p_bIn)
            {
                VaccumOnOff(false);
            }

            if (m_vacuum.p_bIn)
            {
                return false;
            }

            return true;
        }

        public void VaccumOnOff(bool onOff)
        {
            m_vacuumOnOff.Write(onOff);
            Thread.Sleep(1000);
        }

        public InfoWafer GetInfoWafer(int nID)
        {
            return p_infoWafer;
        }

        public void SetInfoWafer(int nID, InfoWafer infoWafer)
        {
            p_infoWafer = infoWafer;
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            if (infoWafer == null)
                infoWafer = p_infoWafer;
            return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        }

        public string IsGetOK(int nID)
        {
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            if (p_infoWafer == null)
                return p_id + " IsGetOK - InfoWafer not Exist";
            return "OK";
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            if (p_infoWafer != null)
                return p_id + " IsPutOK - InfoWafer Exist";
            if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false)
                return p_id + " not Enable Wafer Size";
            return "OK";
        }

        private string MoveReadyPos()
        {
            if (p_axisLifter.IsInPos(eAxisPos.Ready)) return "OK";
            MarsLogManager.Instance.WriteFNC(EQ.p_nRunLP, BaseDefine.LOG_DEVICE_ID, "Move Ready Position", SSLNet.STATUS.START);
            /* XY Ready 위치 이동 */
            if (Run(p_axisXY.p_axisX.StartMove(eAxisPos.Ready)))
                return p_sInfo;
            if (Run(p_axisXY.p_axisY.StartMove(eAxisPos.Ready)))
                return p_sInfo;
            if (Run(p_axisZ.StartMove(eAxisPos.Ready)))
                return p_sInfo;
            if (Run(p_axisXY.WaitReady()))
                return p_sInfo;
            if (Run(p_axisZ.WaitReady()))
                return p_sInfo;
            MarsLogManager.Instance.WriteFNC(EQ.p_nRunLP, BaseDefine.LOG_DEVICE_ID, "Move Ready Position", SSLNet.STATUS.END);
            /* Vaccum Check 후Lifter Up */
            if (LifterUp() != "OK")
                return p_sInfo;

            return "OK";
        }

        public string RunMoveReady()
        {
            string info = MoveReadyPos();
            if (info != "OK")
                return info;
            return "OK";
        }

        public string BeforeGet(int nID)
        {
            //App.m_SSLoggerNet.WriteXFRLog(nID, SSLNet.XFR_EVENTID.GET, SSLNet.STATUS.START,);
            //m_CamVRS.FunctionConnect();
            string info = MoveReadyPos();
            if (info != "OK")
                return info;
            //MarsLogManager.Instance.WriteFNC(EQ.p_nRunLP, BaseDefine.LOG_DEVICE_ID, "Move Ready Position", SSLNet.STATUS.END);
            return "OK";
        }

        
        public string BeforePut(int nID)
        {

            //if (CanInitCal[EQ.p_nRunLP])
            //{

            //    //CanInitCal[EQ.p_nRunLP] = false;
            //}
            if (m_bUseInitCal)
            {
                while (!m_InitCalDone)
                {
                    if (EQ.IsStop())
                    {
                        return "Cal Error";
                    }
                }
            }

            string info = MoveReadyPos();
            if (info != "OK")
                return info;

            if (m_loadExistWafer.p_bIn)
            {
                SetAlarmLoad();
                return "Check Vision Load Position";
            }


            return "OK";
        }

        public string p_processStartDate { get; set; }
        public string p_processStartTime { get; set; }
        public string AfterGet(int nID)
        {
            // Make Directory
            //App.m_SSLoggerNet.WriteXFRLog(nID, SSLNet.XFR_EVENTID.GET, SSLNet.STATUS.END,);

            return "OK";
        }

        public string AfterPut(int nID)
        {
            if (p_infoWafer.p_eWaferOrder == InfoWafer.eWaferOrder.FirstWafer || p_infoWafer.p_eWaferOrder == InfoWafer.eWaferOrder.FirstLastWafer)
            {
                p_dataSavePathDate = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH-mm");
                p_dataSavePath = BaseDefine.Dir_MeasureSaveRootPath + p_infoWafer.p_sRecipe;
                GeneralTools.MakeDirectory(p_dataSavePath);
            }

            if (m_engineer.p_bUseXGem)
            {
                p_processStartDate = DateTime.Now.ToString("MM\\/dd\\/yyyy");
                p_processStartTime = DateTime.Now.ToString("HH:mm:ss");
            }

            MarsLogManager.Instance.ChangeMaterialSlot(EQ.p_nRunLP, p_infoWafer.m_nSlot + 1);
            MarsLogManager.Instance.WritePRC(EQ.p_nRunLP, BaseDefine.LOG_DEVICE_ID, SSLNet.PRC_EVENTID.Process, SSLNet.STATUS.START, SSLNet.MATERIAL_TYPE.WAFER, this.p_id, 0);
            return "OK";
        }

        enum eCheckWafer
        {
            InfoWafer,
            Sensor
        }
        eCheckWafer m_eCheckWafer = eCheckWafer.InfoWafer;
        public bool IsWaferExist(int nID)
        {
            switch (m_eCheckWafer)
            {
                case eCheckWafer.Sensor:
                    if (m_homeExistWafer.p_bIn || m_loadExistWafer.p_bIn)
                        return true;
                    return false;
                default: return (p_infoWafer != null);
            }
        }

        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
            RunTreeLight(tree.GetTree("LightPower", false));
            RunTreeInitCal(tree.GetTree("Calibration", false));
        }

        bool m_bUseInitCal = false;
        void RunTreeInitCal(Tree tree)
        {
            m_bUseInitCal = tree.Set(m_bUseInitCal, m_bUseInitCal,"Use Init Cal", "Use Init Cal");
        }
        void RunTreeSetup(Tree tree)
        {
            m_eCheckWafer = (eCheckWafer)tree.Set(m_eCheckWafer, m_eCheckWafer, "CheckWafer", "CheckWafer");
            m_waferSize.RunTree(tree.GetTree("Wafer Size", false), true);
        }

        List<double> m_aLightPower = new List<double>();
        void RunTreeLight(Tree tree)
        {
            if (m_lightSet == null) return;

            while (m_aLightPower.Count < m_lightSet.m_aLight.Count)
                m_aLightPower.Add(0);
            for (int n = 0; n < m_aLightPower.Count; n++)
            {
                m_aLightPower[n] = tree.Set(m_aLightPower[n], m_aLightPower[n], m_lightSet.m_aLight[n].m_sName, "Light Power (0 ~ 100 %%)");
            }
        }

    }


}
