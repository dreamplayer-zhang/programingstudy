using Root_CAMELLIA.Data;
using Met = Root_CAMELLIA.LibSR_Met;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Light;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Root_CAMELLIA.Module.Module_Camellia;
using static Root_CAMELLIA.Data.WaferCentering;
using SSLNet;

namespace Root_CAMELLIA.Module
{
    public class Run_CalibrationWaferCentering : ModuleRunBase
    {
        Module_Camellia m_module;
        DataManager m_DataManager;
        MainWindow_ViewModel m_Lamp;
        (Met.SettingData, Met.Nanoview.ERRORCODE_NANOVIEW) m_SettingDataWithErrorCode;
        RPoint m_WaferLT_pulse = new RPoint(); // Pulse
        RPoint m_WaferRT_pulse = new RPoint(); // Pulse
        RPoint m_WaferRB_pulse = new RPoint(); // Pulse
        int m_EdgeSearchRange = 20;
        int m_EdgeSearchLevel = 30;
        double m_dResX_um = 1;
        double m_dResY_um = 1;
        double m_dFocusZ = 0;

        int m_nCalibrationCnt = 1;

        bool m_bUseCustomSpeed = false;
        double m_dMoveSpeedX = 0;
        double m_dMoveSpeedY = 0;
        double m_dMoveAccX = -1;
        double m_dMoveDecX = -1;
        double m_dMoveAccY = -1;
        double m_dMoveDecY = -1;

        bool m_InitialCal = false;
        public bool m_useCal = true;
        public bool m_useCentering = true;

        public bool m_isPM = false;
        public Run_CalibrationWaferCentering(Module_Camellia module)
        {
            m_module = module;
            m_DataManager = module.m_DataManager;
            
            InitModuleRun(module);
        }

        void CalDoneEvent()
        {
            MarsLogManager logManager = MarsLogManager.Instance;
            DataFormatter dataFormatter = new DataFormatter();
            dataFormatter.AddData(nameof(m_InitialCal), m_InitialCal.ToString());
            dataFormatter.AddData(nameof(m_nCalibrationCnt), m_nCalibrationCnt);
            logManager.WriteFNC(EQ.p_nRunLP, BaseDefine.LOG_DEVICE_ID, "Background Calibration", STATUS.END, dataFormatter:dataFormatter);
            dataFormatter.ClearData();
        }

        public override ModuleRunBase Clone()
        {
            Run_CalibrationWaferCentering run = new Run_CalibrationWaferCentering(m_module);
            run.m_DataManager = m_module.m_DataManager;
            run.m_InitialCal = m_InitialCal;
            run.m_WaferLT_pulse = m_WaferLT_pulse;
            run.m_WaferRT_pulse = m_WaferRT_pulse;
            run.m_WaferRB_pulse = m_WaferRB_pulse;
            run.m_EdgeSearchRange = m_EdgeSearchRange;
            run.m_EdgeSearchLevel = m_EdgeSearchLevel;
            run.m_dResX_um = m_dResX_um;
            run.m_dResY_um = m_dResY_um;
            run.m_useCal = m_useCal;
            run.m_useCentering = m_useCentering;
            run.m_dFocusZ = m_dFocusZ;
            run.m_dMoveSpeedX = m_dMoveSpeedX;
            run.m_dMoveSpeedY = m_dMoveSpeedY;
            run.m_dMoveAccX = m_dMoveAccX;
            run.m_dMoveAccY = m_dMoveAccY;
            run.m_dMoveDecX = m_dMoveDecX;
            run.m_dMoveDecY = m_dMoveDecY;
            run.m_bUseCustomSpeed = m_bUseCustomSpeed;
            run.m_nCalibrationCnt = m_nCalibrationCnt;

            return run;
        }

        private bool CheckVaildParameter()
        {
            if(m_dMoveSpeedX == 0 || m_dMoveSpeedY == 0)
            {
                return false;
            }
            if(m_dMoveAccX == -1 || m_dMoveDecX == -1)
            {
                return false;
            }
            if(m_dMoveAccY == -1 || m_dMoveDecY == -1)
            {
                return false;
            }
            return true;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_WaferLT_pulse = tree.Set(m_WaferLT_pulse, m_WaferLT_pulse, "Wafer Left Top", "Wafer Left Top (Pulse)", bVisible);
            m_WaferRT_pulse = tree.Set(m_WaferRT_pulse, m_WaferRT_pulse, "Wafer Right Top", "Wafer Right Top (Pulse)", bVisible);
            m_WaferRB_pulse = tree.Set(m_WaferRB_pulse, m_WaferRB_pulse, "Wafer Right Bottom", "Wafer Right Bottom (Pulse)", bVisible);
            m_EdgeSearchRange = tree.Set(m_EdgeSearchRange, m_EdgeSearchRange, "Edge Search Range", "Edge Search Range", bVisible);
            m_EdgeSearchLevel = tree.Set(m_EdgeSearchLevel, m_EdgeSearchLevel, "Edge Search Level", "Edge Search Level", bVisible);
            m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Camera X Resolution", "Camera X Resolution(um)", bVisible);
            m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Camera Y Resolution", "Camera Y Resolution(um)", bVisible);
            m_InitialCal = tree.Set(m_InitialCal, m_InitialCal, "Initial Calibration", "Initial Calibration", bVisible);
            m_useCal = tree.Set(m_useCal, m_useCal, "Use Calibration", "Use Calibration", bVisible);
            m_useCentering = tree.Set(m_useCentering, m_useCentering, "Use Centering", "Use Centering", bVisible);
            m_dFocusZ = tree.Set(m_dFocusZ, m_dFocusZ, "Focus Z Pos", "Focus Z Pos", bVisible);

            m_nCalibrationCnt = tree.Set(m_nCalibrationCnt, m_nCalibrationCnt, "Calibration Retry Count", "Calibration Retry", bVisible);

            m_dMoveSpeedX = tree.Set(m_dMoveSpeedX, m_dMoveSpeedX, "Move Speed X", "Move Speed X", bVisible);
            m_dMoveSpeedY = tree.Set(m_dMoveSpeedY, m_dMoveSpeedY, "Move Speed Y", "Move Speed Y", bVisible);
            m_dMoveAccX = tree.Set(m_dMoveAccX, m_dMoveAccX, "Move Acc X", "Move Acc X", bVisible);
            m_dMoveAccY = tree.Set(m_dMoveAccY, m_dMoveAccY, "Move Acc Y", "Move Acc Y", bVisible);
            m_dMoveDecX = tree.Set(m_dMoveDecX, m_dMoveDecX, "Move Dec X", "Move Dec X", bVisible);
            m_dMoveDecY = tree.Set(m_dMoveDecY, m_dMoveDecY, "Move Dec Y", "Move Dec Y", bVisible);
            m_bUseCustomSpeed = tree.Set(m_bUseCustomSpeed, m_bUseCustomSpeed, "Use Custom Speed", "Use Custom Speed", bVisible);
            //RunTreeLight(tree.GetTree("LightPower", false), bVisible, bReadOnly);
        }

        public override string Run()
        {
            DataManager.Instance.m_calibration.CalDoneEvent += CalDoneEvent;
            try
            {
                string deviceID = BaseDefine.LOG_DEVICE_ID;

                StopWatch test = new StopWatch();
                test.Start();
                m_log.Warn("Calibration Start");


                AxisXY axisXY = m_module.p_axisXY;
                Axis axisZ = m_module.p_axisZ;
                string strVRSImageDir = "D:\\";
                string strVRSImageFullPath = "";

                string materialID = "";
                if (m_module.p_infoWafer == null)
                    materialID = "test";

                MarsLogManager logManager = MarsLogManager.Instance;


                m_DataManager.m_waferCentering.FindEdgeInit();

                Camera_Basler VRS = m_module.p_CamVRS;
                ImageData img = VRS.p_ImageViewer.p_ImageData;

                StopWatch sw = new StopWatch();


                if (!VRS.p_CamInfo._OpenStatus)
                {
                    logManager.WriteFNC(EQ.p_nRunLP, deviceID, "VRS Connect", SSLNet.STATUS.START);
                    VRS.Connect();

                    while (!VRS.p_CamInfo._OpenStatus)
                    {
                        if (sw.ElapsedMilliseconds > 15000)
                        {
                            sw.Stop();
                            logManager.WriteFNC(EQ.p_nRunLP, deviceID, "VRS Connect", SSLNet.STATUS.END);
                            return "Navigation Camera Not Connected";
                        }
                    }
                    sw.Stop();
                    logManager.WriteFNC(EQ.p_nRunLP, deviceID, "VRS Connect", SSLNet.STATUS.END);
                }


                DataFormatter dataformatter = new DataFormatter();
                //
                if (m_module.LifterDown() != "OK")
                {
                    return p_sInfo;
                }
                //logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Lifter Down", SSLNet.STATUS.END);


                //
                logManager.WritePRC(EQ.p_nRunLP, deviceID, SSLNet.PRC_EVENTID.StepProcess, SSLNet.STATUS.START, MATERIAL_TYPE.WAFER, "Calibration WaferCentering", (int)BaseDefine.Process.CalbrationWaferCentering);
                DataFormatter dataFormatter = new DataFormatter();
                dataFormatter.AddData("Z Axis", m_dFocusZ, "Pulse");
                logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Navigation Cam Move", SSLNet.STATUS.START, dataFormatter: dataFormatter);

                if (m_module.Run(axisZ.StartMove(m_dFocusZ)))
                    return p_sInfo;
                if (m_module.Run(axisZ.WaitReady()))
                    return p_sInfo;

                logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Navigation Cam Move", SSLNet.STATUS.END, dataFormatter: dataFormatter);
                dataFormatter.ClearData();


                if (m_useCal)
                {
                    //logManager.WritePRC(EQ.p_nRunLP, deviceID, PRC_EVENTID., STATUS.START, "Background Calibration", 1);
                    dataFormatter.AddData(nameof(m_InitialCal), m_InitialCal.ToString());
                    dataFormatter.AddData(nameof(m_nCalibrationCnt), m_nCalibrationCnt);
                    logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Background Calibration", SSLNet.STATUS.START, dataFormatter: dataFormatter);

                    if (m_useCentering)
                    {

                        if (m_DataManager.m_calibration.Run(m_InitialCal, m_isPM, retryCount: m_nCalibrationCnt) != "OK")
                        {
                            return "Calibration fail";
                        }
                    }
                    else
                    {
                        //Thread.Sleep(6);
                        bool success = false;
                        for (int i = 0; i < m_nCalibrationCnt; i++)
                        {
                            if (m_DataManager.m_calibration.Run(m_InitialCal, m_isPM, false) != "OK")
                            {
                                success = false;
                                //return "Calibration fail";
                            }
                            else
                            {
                                success = true;
                                break;
                            }
                            if (!success)
                                return "Calibration Fail";
                        }
                        logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Background Calibration", SSLNet.STATUS.END, dataFormatter: dataFormatter);
                        logManager.WritePRC(EQ.p_nRunLP, deviceID, PRC_EVENTID.StepProcess, STATUS.END, MATERIAL_TYPE.WAFER, "Calibration WaferCentering", (int)BaseDefine.Process.CalbrationWaferCentering);
                    }
                    dataFormatter.ClearData();
                }

                if (!m_useCentering)
                {
                    dataFormatter.AddData("Z Axis", axisZ.GetPosValue(eAxisPos.Home), "Pulse");
                    logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Navigation Cam Move", SSLNet.STATUS.START, dataFormatter: dataFormatter);

                    if (m_module.Run(axisZ.StartMove(eAxisPos.Home)))
                        return p_sInfo;
                    if (m_module.Run(axisZ.WaitReady()))
                        return p_sInfo;
                    logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Navigation Cam Move", SSLNet.STATUS.END, dataFormatter: dataFormatter);
                    dataformatter.ClearData();
                    //logManager.WritePRC(EQ.p_nRunLP, deviceID, SSLNet.PRC_EVENTID.Process, SSLNet.STATUS.END, this.p_id, 0);
                    return "OK";
                }



                m_module.SetLight(true);


                DataFormatter stageDataFormatter = new DataFormatter();
                stageDataFormatter.AddData(nameof(m_bUseCustomSpeed), m_bUseCustomSpeed.ToString());

                //logManager.WritePRC(EQ.p_nRunLP, deviceID, PRC_EVENTID., STATUS.START, "Move To FOV", 2);
                if (m_bUseCustomSpeed && CheckVaildParameter())
                {

                    stageDataFormatter.AddData("X Axis", m_WaferLT_pulse.X, "Pulse");
                    stageDataFormatter.AddData(nameof(m_dMoveSpeedX), m_dMoveSpeedX);
                    stageDataFormatter.AddData(nameof(m_dMoveAccX), m_dMoveAccX);
                    stageDataFormatter.AddData(nameof(m_dMoveDecX), m_dMoveDecX);
                    stageDataFormatter.AddData("Y Axis", m_WaferLT_pulse.Y, "Pulse");
                    stageDataFormatter.AddData(nameof(m_dMoveSpeedY), m_dMoveSpeedY);
                    stageDataFormatter.AddData(nameof(m_dMoveAccY), m_dMoveAccY);
                    stageDataFormatter.AddData(nameof(m_dMoveDecY), m_dMoveDecY);

                    logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.START, dataFormatter: stageDataFormatter);

                    if (m_module.Run(axisXY.p_axisX.StartMove(m_WaferLT_pulse.X, m_dMoveSpeedX, m_dMoveAccX, m_dMoveDecX)))
                    {
                        return p_sInfo;
                    }
                    if (m_module.Run(axisXY.p_axisY.StartMove(m_WaferLT_pulse.Y, m_dMoveSpeedY, m_dMoveAccY, m_dMoveDecY)))
                    {
                        return p_sInfo;
                    }
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
                }
                else
                {
                    stageDataFormatter.AddData("X Axis", m_WaferLT_pulse.X, "Pulse");
                    stageDataFormatter.AddData("Y Axis", m_WaferLT_pulse.Y, "Pulse");
                    logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.START, dataFormatter: stageDataFormatter);
                    if (m_module.Run(axisXY.StartMove(m_WaferLT_pulse)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
                }
                logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.END, dataFormatter: stageDataFormatter);
                stageDataFormatter.ClearData();

                logManager.WriteFNC(EQ.p_nRunLP, deviceID, "GrabOneShot", SSLNet.STATUS.START);
                VRS.GrabOneShot();
                logManager.WriteFNC(EQ.p_nRunLP, deviceID, "GrabOneShot", SSLNet.STATUS.END);


                CenteringParam param = new CenteringParam(img, VRS.GetRoiSize(), m_EdgeSearchRange, m_EdgeSearchLevel, WaferCentering.eDir.LT);
                //logManager.WritePRC(EQ.p_nRnR, m_module.p_id, SSLNet.PRC_EVENTID.Process, SSLNet.STATUS.START, this.p_id, sequence++, m_module.p_infoWafer.p_id);
                //logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Find Edge", SSLNet.STATUS.START);
                ThreadPool.QueueUserWorkItem(m_DataManager.m_waferCentering.FindEdge, param);


                stageDataFormatter.AddData(nameof(m_bUseCustomSpeed), m_bUseCustomSpeed.ToString());
                if (m_bUseCustomSpeed && CheckVaildParameter())
                {
                    stageDataFormatter.AddData("X Axis", m_WaferRT_pulse.X, "Pulse");
                    stageDataFormatter.AddData(nameof(m_dMoveSpeedX), m_dMoveSpeedX);
                    stageDataFormatter.AddData(nameof(m_dMoveAccX), m_dMoveAccX);
                    stageDataFormatter.AddData(nameof(m_dMoveDecX), m_dMoveDecX);
                    stageDataFormatter.AddData("Y Axis", m_WaferRT_pulse.Y, "Pulse");
                    stageDataFormatter.AddData(nameof(m_dMoveSpeedY), m_dMoveSpeedY);
                    stageDataFormatter.AddData(nameof(m_dMoveAccY), m_dMoveAccY);
                    stageDataFormatter.AddData(nameof(m_dMoveDecY), m_dMoveDecY);

                    logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.START, dataFormatter: stageDataFormatter);

                    if (m_module.Run(axisXY.p_axisX.StartMove(m_WaferRT_pulse.X, m_dMoveSpeedX, m_dMoveAccX, m_dMoveDecX)))
                    {
                        return p_sInfo;
                    }
                    if (m_module.Run(axisXY.p_axisY.StartMove(m_WaferRT_pulse.Y, m_dMoveSpeedY, m_dMoveAccY, m_dMoveDecY)))
                    {
                        return p_sInfo;
                    }
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
                }
                else
                {
                    stageDataFormatter.AddData("X Axis", m_WaferRT_pulse.X, "Pulse");
                    stageDataFormatter.AddData("Y Axis", m_WaferRT_pulse.Y, "Pulse");
                    logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.START, dataFormatter: stageDataFormatter);

                    if (m_module.Run(axisXY.StartMove(m_WaferRT_pulse)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
                }
                logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.END, dataFormatter: stageDataFormatter);
                stageDataFormatter.ClearData();
                // return "OK";
                //m_DataManager.m_waferCentering.FindEdge(param);
                //Thread.Sleep(1000);





                //if (VRS.Grab() == "OK")
                //{
                //    //strVRSImageFullPath = string.Format(strVRSImageDir + "VRSImage_{0}.bmp", 1);
                //    //img.SaveImageSync(strVRSImageFullPath);
                //    //Grab error
                //}
                //else
                //{
                //    return "Grab Error";
                //}

                logManager.WriteFNC(EQ.p_nRunLP, deviceID, "GrabOneShot", SSLNet.STATUS.START);
                VRS.GrabOneShot();
                logManager.WriteFNC(EQ.p_nRunLP, deviceID, "GrabOneShot", SSLNet.STATUS.END);

                param = new CenteringParam(img, VRS.GetRoiSize(), m_EdgeSearchRange, m_EdgeSearchLevel, WaferCentering.eDir.RT);
                ThreadPool.QueueUserWorkItem(m_DataManager.m_waferCentering.FindEdge, param);
                //m_DataManager.m_waferCentering.FindEdge(param);

                // Thread.Sleep(1000);

                stageDataFormatter.AddData(nameof(m_bUseCustomSpeed), m_bUseCustomSpeed.ToString());
                if (m_bUseCustomSpeed && CheckVaildParameter())
                {
                    stageDataFormatter.AddData("X Axis", m_WaferRT_pulse.X, "Pulse");
                    stageDataFormatter.AddData(nameof(m_dMoveSpeedX), m_dMoveSpeedX);
                    stageDataFormatter.AddData(nameof(m_dMoveAccX), m_dMoveAccX);
                    stageDataFormatter.AddData(nameof(m_dMoveDecX), m_dMoveDecX);
                    stageDataFormatter.AddData("Y Axis", m_WaferRT_pulse.Y, "Pulse");
                    stageDataFormatter.AddData(nameof(m_dMoveSpeedY), m_dMoveSpeedY);
                    stageDataFormatter.AddData(nameof(m_dMoveAccY), m_dMoveAccY);
                    stageDataFormatter.AddData(nameof(m_dMoveDecY), m_dMoveDecY);

                    logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.START, dataFormatter: stageDataFormatter);

                    if (m_module.Run(axisXY.p_axisX.StartMove(m_WaferRB_pulse.X, m_dMoveSpeedX, m_dMoveAccX, m_dMoveDecX)))
                    {
                        return p_sInfo;
                    }
                    if (m_module.Run(axisXY.p_axisY.StartMove(m_WaferRB_pulse.Y, m_dMoveSpeedY, m_dMoveAccY, m_dMoveDecY)))
                    {
                        return p_sInfo;
                    }
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
                }
                else
                {
                    stageDataFormatter.AddData("X Axis", m_WaferRT_pulse.X, "Pulse");
                    stageDataFormatter.AddData("Y Axis", m_WaferRT_pulse.Y, "Pulse");
                    logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.START, dataFormatter: stageDataFormatter);

                    if (m_module.Run(axisXY.StartMove(m_WaferRB_pulse)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
                }
                logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.END, dataFormatter: stageDataFormatter);
                stageDataFormatter.ClearData();

                //if (VRS.Grab() == "OK")
                //{
                //    //strVRSImageFullPath = string.Format(strVRSImageDir + "VRSImage_{0}.bmp", 2);
                //    //img.SaveImageSync(strVRSImageFullPath);
                //    //Grab error
                //}
                //else
                //{
                //    return "Grab Error";
                //}
                logManager.WriteFNC(EQ.p_nRunLP, deviceID, "GrabOneShot", SSLNet.STATUS.START);
                VRS.GrabOneShot();
                logManager.WriteFNC(EQ.p_nRunLP, deviceID, "GrabOneShot", SSLNet.STATUS.END);


                param = new CenteringParam(img, VRS.GetRoiSize(), m_EdgeSearchRange, m_EdgeSearchLevel, WaferCentering.eDir.RB);
                ThreadPool.QueueUserWorkItem(m_DataManager.m_waferCentering.FindEdge, param);
                //m_DataManager.m_waferCentering.FindEdge(param);
                while ((!m_DataManager.m_waferCentering.FindLTEdgeDone || !m_DataManager.m_waferCentering.FindRTEdgeDone || !m_DataManager.m_waferCentering.FindRBEdgeDone) && m_useCentering)
                {
                    if (m_DataManager.m_waferCentering.ErrorString != "OK")
                    {
                        return "Find Edge Error";
                    }
                    if (EQ.IsStop())
                    {
                        return "EQ Stop";
                    }
                }
                //logManager.WritePRC(EQ.p_nRunLP, deviceID, SSLNet.PRC_EVENTID.StepProcess, SSLNet.STATUS.END, "Find Edge", 3);

                logManager.WriteFNC(EQ.p_nRunLP, deviceID, "CalcCenterPoint", STATUS.START);
                m_DataManager.m_waferCentering.CalCenterPoint(VRS.GetRoiSize(), m_dResX_um, m_dResY_um, m_WaferLT_pulse, m_WaferRT_pulse, m_WaferRB_pulse);
                logManager.WriteFNC(EQ.p_nRunLP, deviceID, "CalcCenterPoint", STATUS.END);
                if (m_InitialCal)
                {
                    while (!m_DataManager.m_calibration.InItCalDone && m_useCal)
                    {
                        if (EQ.IsStop())
                        {
                            return "EQ Stop";
                        }
                    }
                }
                else
                {
                    while (!m_DataManager.m_calibration.CalDone && m_useCal)
                    {
                        if (EQ.IsStop())
                        {
                            return "EQ Stop";
                        }
                    }
                }

                if (m_DataManager.m_calibration.ErrorString != "OK" && m_useCal)
                {
                    return "Calibration Fail";
                }


                stageDataFormatter.AddData("Z Axis", axisZ.GetPosValue(eAxisPos.Home), "Pulse");
                logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Navigation Cam Move", SSLNet.STATUS.START, dataFormatter: stageDataFormatter);

                if (m_module.Run(axisZ.StartMove(eAxisPos.Home)))
                    return p_sInfo;
                if (m_module.Run(axisZ.WaitReady()))
                    return p_sInfo;
                logManager.WriteFNC(EQ.p_nRunLP, deviceID, "Navigation Cam Move", SSLNet.STATUS.END, dataFormatter: stageDataFormatter);
                stageDataFormatter.ClearData();
                m_module.SetLight(false);

                test.Stop();
                m_log.Warn("Calibration End >> " + test.ElapsedMilliseconds);

                logManager.WritePRC(EQ.p_nRunLP, deviceID, SSLNet.PRC_EVENTID.StepProcess, SSLNet.STATUS.END, MATERIAL_TYPE.WAFER, "Calibration WaferCentering", (int)BaseDefine.Process.CalbrationWaferCentering);
            }
            finally
            {
                DataManager.Instance.m_calibration.CalDoneEvent -= CalDoneEvent;
            }
           
            return "OK";
        }
    }
}
