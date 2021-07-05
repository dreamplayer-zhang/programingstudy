﻿using Root_CAMELLIA.Data;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using SSLNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static Root_CAMELLIA.Module.Module_Camellia;
using Met = Root_CAMELLIA.LibSR_Met;

namespace Root_CAMELLIA.Module
{
    public class Run_Measure : ModuleRunBase
    {
        Module_Camellia m_module;
        MainWindow_ViewModel m_mwvm;
        DataManager m_DataManager;
        public RPoint m_StageCenterPos_pulse = new RPoint(); // Pulse
        double m_dResX_um = 1;
        double m_dResY_um = 1;
        double m_dFocusZ_pulse = 1; // Pulse

        Task m_thread;
        bool m_bStart = false;
        bool m_CalcThicknessDone = false;

        public Dlg_Engineer_ViewModel.PM_SR_Parameter SR_Parameter = new Dlg_Engineer_ViewModel.PM_SR_Parameter();
        public bool m_isPM = false;
        public bool m_isAlphaFit = false;
        public bool m_isPointMeasure = false;
        public RPoint m_ptMeasure = new RPoint();
        public Run_Measure(Module_Camellia module)
        {
            m_module = module;
            m_mwvm = module.mwvm;
            m_DataManager = module.m_DataManager;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_Measure run = new Run_Measure(m_module);
            run.m_StageCenterPos_pulse = m_StageCenterPos_pulse;
            run.m_dResX_um = m_dResX_um;
            run.m_dResY_um = m_dResY_um;
            run.m_dFocusZ_pulse = m_dFocusZ_pulse;
            return run;
        }
        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_StageCenterPos_pulse = tree.Set(m_StageCenterPos_pulse, m_StageCenterPos_pulse, "Stage Center Axis Position", "Stage Center Axis Position(Pulse)", bVisible);
            m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Camera X Resolution", "Camera X Resolution(um)", bVisible);
            m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Camera Y Resolution", "Camera Y Resolution(um)", bVisible);
            m_dFocusZ_pulse = tree.Set(m_dFocusZ_pulse, m_dFocusZ_pulse, "Focus Z Position", "Focus Z Position(pulse)", bVisible);
        }

        ConcurrentQueue<int> thicknessQueue = new ConcurrentQueue<int>();
        bool MeasureDone = false;
        //bool isEQStop = false;
        private void RunThread()
        {
            m_bStart = true;
            m_CalcThicknessDone = false;
            MeasureDone = false;
            MarsLogManager marsLogManager = MarsLogManager.Instance;
            //isEQStop = false;
            StopWatch sw = new StopWatch();
            int nThicknessCnt = 0;
            while (m_bStart)
            {
                int index;
                bool useAlphafit = true;
                if (m_isPM)
                {
                    useAlphafit = m_isAlphaFit;
                }
                else
                {
                    LibSR_Met.DataManager.GetInstance().m_SettngData.dAlphaFit = 1.0;
                }

                if (EQ.IsStop())
                {
                    while (thicknessQueue.TryDequeue(out index))
                    {

                    }
                    m_CalcThicknessDone = true;
                    break;
                }

                if (thicknessQueue.TryDequeue(out index))
                {
                    sw.Start();
                    if (m_DataManager.recipeDM.MeasurementRD.UseThickness)
                    {
                        //Thread.Sleep(1);
                        Met.Nanoview.ERRORCODE_NANOVIEW rst = App.m_nanoView.GetThickness(index, m_DataManager.recipeDM.MeasurementRD.LMIteration, m_DataManager.recipeDM.MeasurementRD.DampingFactor, useAlphafit);
                        if (rst != Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                        {
                            //isEQStop = false;

                            // "Get Thickness Error";
                            m_log.Warn(Enum.GetName(typeof(Met.Nanoview.ERRORCODE_NANOVIEW), rst));
                        }

                        if (m_isPM)
                        {
                            m_mwvm.EngineerViewModel.p_pmParameter.p_alpha1 = App.m_nanoView.GetAlphaFit();
                        }
                    }

                    m_mwvm.p_Progress = (double)(index + 1) / m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count * 100;
                    SaveRawData(index);
                    //.DataManager MetData = LibSR_Met.DataManager.GetInstance();
                    // Spectrum data Thread 추가 두개두개두개
                    //LibSR_Met.DataManager.GetInstance().SaveResultFileSlot(@"C:\Users\ATI\Desktop\SaveTest\" + index + "_" + DateTime.Now.ToString("HHmmss") + "test.csv", m_module.p_infoWafer.p_sCarrierID,
                    //    m_module.p_infoWafer.p_sLotID, BaseDefine.TOOL_NAME, m_module.p_infoWafer.p_sWaferID, m_module.p_infoWafer.p_sSlotID,
                    //    BaseDefine.Configuration.Version, m_DataManager.recipeDM.TeachRecipeName, index,
                    //    m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[index]].x,
                    //    m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[index]].y,
                    //    m_DataManager.recipeDM.MeasurementRD.LowerWaveLength,
                    //    m_DataManager.recipeDM.MeasurementRD.UpperWaveLength);
                     
                    LibSR_Met.DataManager.GetInstance().SaveResultFileSlot(m_slotSpectraDataPath + "\\" + index + "_" + DateTime.Now.ToString("HHmmss"), m_module.p_infoWafer, m_DataManager.recipeDM, index);
                    
                    //SaveRT
                    LibSR_Met.DataManager.GetInstance().SaveRT(m_historyRTDataPath + "\\" + index + "_" + DateTime.Now.ToString("HHmmss") + "RawData.csv", index);

                    //foreach (LibSR_Met.ContourMapData mapdata in MetData.m_ContourMapDataR)
                    //    LibSR_Met.DataManager.GetInstance().SaveContourMapData(@"C:\Users\ATI\Desktop\SaveTest\" + index + "_R_" + mapdata.Wavelength.ToString() + "_" + DateTime.Now.ToString("HHmmss") + "_ContourMapData.csv", mapdata);

                    //foreach (LibSR_Met.ContourMapData mapdata in MetData.m_ContourMapDataT)
                    //    LibSR_Met.DataManager.GetInstance().SaveContourMapData(@"C:\Users\ATI\Desktop\SaveTest\" + index + "_T_" + mapdata.Wavelength.ToString() + "_" + DateTime.Now.ToString("HHmmss") + "_ContourMapData.csv", mapdata);

                    sw.Stop();
                    System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
                    nThicknessCnt++;
                }

                if (MeasureDone && (nThicknessCnt == (m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count * m_DataManager.recipeDM.MeasurementRD.MeasureRepeatCount) || m_isPointMeasure))
                {
                    m_CalcThicknessDone = true;
                    break;
                }
            }
        }

        string m_summaryPath = "";
        string m_resultPath = "";
        string m_slotContourMapPath = "";
        string m_slotSpectraDataPath = "";
        string m_historyRTDataPath = "";
        string m_lotStartTime = "";
        string[] m_resultDataSavePath = new string[100];

        private bool MakeSaveDirectory(bool isLotStart, int nRepeatCount)
        {
            string rootPath = m_module.p_dataSavePath;
            // RnR 처음 또는 마지막 웨이퍼 측정 할 때, 
            try
            {
                if (m_module.p_infoWafer == null)
                {
                    return true;
                }
                string[] path = rootPath.Split('\\');
                if (m_module.p_dataSavePath == "")
                {
                    rootPath = BaseDefine.Dir_MeasureSaveRootPath + m_module.p_infoWafer.p_sRecipe;
                }

                if (nRepeatCount == 1)
                {
                    if (isLotStart)
                    {
                        m_lotStartTime = string.Empty;
                        m_lotStartTime = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH-mm");
                        rootPath += @"\" + m_lotStartTime; 
                        isLotStart = false;
                    }
                    m_resultDataSavePath[0] = rootPath;
                    m_summaryPath = m_resultDataSavePath[0] + "\\ResultData_Summary";
                    GeneralTools.MakeDirectory(m_summaryPath);
                    m_resultPath = m_resultDataSavePath[0] + "\\ResultData";
                    GeneralTools.MakeDirectory(m_resultPath);

                    m_slotContourMapPath = m_resultDataSavePath[0] + "\\Slot." + m_module.p_infoWafer.m_nSlot + "\\ContourMap";
                    GeneralTools.MakeDirectory(m_slotContourMapPath);
                    m_slotSpectraDataPath = m_resultDataSavePath[0] + "\\Slot." + m_module.p_infoWafer.m_nSlot + "\\SpectraData";
                    GeneralTools.MakeDirectory(m_slotSpectraDataPath);

                    //히스토리 데이터는 데이터 저장 경로 재확인 필요
                    m_historyRTDataPath = BaseDefine.Dir_HistorySaveRootPath + m_module.p_infoWafer.p_sRecipe + "\\RawData\\" + DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH-mm-ss");
                    GeneralTools.MakeDirectory(m_historyRTDataPath);
                }
                else
                {
                    for (int n = 0; n < nRepeatCount; n++)
                    {
                        if (isLotStart && n==0)
                        {
                            m_lotStartTime = string.Empty;
                            m_lotStartTime = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH-mm");
                            rootPath += @"\" + m_lotStartTime;
                            isLotStart = false;
                        }
                        m_resultDataSavePath[n] = rootPath + "_" + n.ToString();
                        m_summaryPath = m_resultDataSavePath[n] + "\\ResultData_Summary";
                        GeneralTools.MakeDirectory(m_summaryPath);
                        m_resultPath = m_resultDataSavePath[n] + "\\ResultData";
                        GeneralTools.MakeDirectory(m_resultPath);
                        m_slotContourMapPath = m_resultDataSavePath[n] + "\\Slot." + m_module.p_infoWafer.m_nSlot + "\\ContourMap";
                        GeneralTools.MakeDirectory(m_slotContourMapPath);
                        m_slotSpectraDataPath = m_resultDataSavePath[n] + "\\Slot." + m_module.p_infoWafer.m_nSlot + "\\SpectraData";
                        GeneralTools.MakeDirectory(m_slotSpectraDataPath);

                        //히스토리 데이터는 데이터 저장 경로 재확인 필요
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
          
            return true;
        } 
        public override string Run()
        {
            MarsLogManager marsLogManager = MarsLogManager.Instance;
            DataFormatter dataFormatter = new DataFormatter();

            string deviceID = BaseDefine.LOG_DEVICE_ID;

            StopWatch test = new StopWatch();
            test.Start();
            m_log.Warn("Measure Start >> ");

            marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Lifter Down", SSLNet.STATUS.START);
            Axis axisLifter = m_module.p_axisLifter;
            if (m_module.LifterDown() != "OK")
            {
                return p_sInfo;
            }
            marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Lifter Down", SSLNet.STATUS.END);
            m_thread = new Task(RunThread);
            m_thread.Start();

            InfoWafer info = m_module.p_infoWafer;


            marsLogManager.WritePRC(EQ.p_nRunLP, deviceID, SSLNet.PRC_EVENTID.StepProcess, SSLNet.STATUS.START, "Measure", (int)BaseDefine.Process.Measure);

            if (!m_isPM)
            {
                Met.DataManager.GetInstance().m_SettngData.nMeasureIntTime_NIR = m_DataManager.recipeDM.MeasurementRD.NIRIntegrationTime;
                Met.DataManager.GetInstance().m_SettngData.nMeasureIntTime_VIS = m_DataManager.recipeDM.MeasurementRD.VISIntegrationTime;
            }

            AxisXY axisXY = m_module.p_axisXY;
            Axis axisZ = m_module.p_axisZ;

            RPoint MeasurePoint;


            Met.DataManager dm = Met.DataManager.GetInstance();
            dm.ClearRawData();

            LibSR_Met.DataManager.GetInstance().ContourMapDataList(m_DataManager.recipeDM.MeasurementRD.WaveLengthReflectance, m_DataManager.recipeDM.MeasurementRD.WaveLengthTransmittance, m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count);

            if (!m_isPointMeasure)
            {
                double centerX;
                double centerY;
                if (m_DataManager.m_waferCentering.m_ptCenter.X == 0 && m_DataManager.m_waferCentering.m_ptCenter.Y == 0)
                {
                    centerX = m_StageCenterPos_pulse.X;
                    centerY = m_StageCenterPos_pulse.Y;
                }
                else
                {
                    centerX = m_DataManager.m_waferCentering.m_ptCenter.X;
                    centerY = m_DataManager.m_waferCentering.m_ptCenter.Y;
                }

                double RatioX = (int)(BaseDefine.CanvasWidth / BaseDefine.ViewSize);
                double RatioY = (int)(BaseDefine.CanvasHeight / BaseDefine.ViewSize);

                m_mwvm.p_Progress = 0;


                double x = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[0]].x;
                double y = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[0]].y;
                double dX = centerX - x * 10000;
                double dY = centerY - y * 10000;
                object obj;
                bool isMove = false;
                //StopWatch sw = new StopWatch();
                int nTotalRawDataIndex = 0;
                for (int i = 0; i < m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count; i++)
                {
                    if (EQ.IsStop())
                    {
                        m_bStart = false;
                        //isEQStop = false;
                        return "EQ Stop";
                    }
                    if (i == 0)
                    {
                        MeasurePoint = new RPoint(dX, dY);
                        dataFormatter.AddData("X Axis", dX, "Pulse");
                        dataFormatter.AddData("Y Axis", dY, "Pulse");
                        marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.START, dataFormatter);

                        if (m_module.Run(axisXY.StartMove(MeasurePoint)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.END, dataFormatter);
                        dataFormatter.ClearData();
                        m_mwvm.p_ArrowX1 = x * RatioX;
                        m_mwvm.p_ArrowY1 = -y * RatioY;
                        if (i < m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count - 1)
                        {
                            double x2 = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i + 1]].x;
                            double y2 = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i + 1]].y;
                            m_mwvm.p_ArrowX2 = x2 * RatioX;
                            m_mwvm.p_ArrowY2 = -y2 * RatioY;
                            m_mwvm.p_ArrowVisible = Visibility.Visible;
                        }


                    }

                    for(int cnt = 0; cnt < m_DataManager.recipeDM.MeasurementRD.MeasureRepeatCount; cnt++)
                    {
                        LibSR_Met.DataManager.GetInstance().nRepeatCount = m_DataManager.recipeDM.MeasurementRD.MeasureRepeatCount;
                        LibSR_Met.DataManager.GetInstance().nPointCount = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count;
                        int nRepeatCount = m_DataManager.recipeDM.MeasurementRD.MeasureRepeatCount;
                       
                        if (m_DataManager.recipeDM.MeasurementRD.MeasureRepeatCount == 1)
                        {
                            nTotalRawDataIndex = i;
                        }
                        else
                        {
                            // 여기서의 nPointIndex는 Repeat * WaferMeasure Point 개수라는 뜻 (다시 확인해서 수정 필요)
                            nTotalRawDataIndex++;
                            
                        }
                        if (!MakeSaveDirectory(true, nRepeatCount))
                        {
                            return "Make Directory Error";
                        }

                        dataFormatter.AddData("Measure Repeat", m_DataManager.recipeDM.MeasurementRD.MeasureRepeatCount);
                        marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Measure", SSLNet.STATUS.START);
                        Met.Nanoview.ERRORCODE_NANOVIEW rst = App.m_nanoView.SampleMeasure((nTotalRawDataIndex-1), x, y,
        m_mwvm.SettingViewModel.p_ExceptNIR, m_DataManager.recipeDM.MeasurementRD.UseTransmittance, m_DataManager.recipeDM.MeasurementRD.UseThickness,
        m_DataManager.recipeDM.MeasurementRD.LowerWaveLength, m_DataManager.recipeDM.MeasurementRD.UpperWaveLength);
                        if (rst != Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                        {
                            m_log.Warn(Enum.GetName(typeof(Met.Nanoview.ERRORCODE_NANOVIEW), rst));
                        }
                        marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Measure", SSLNet.STATUS.END);
                        dataFormatter.ClearData();
                        StopWatch sw = new StopWatch();
                        sw.Start();

                        if (i == 0 && cnt == 0)
                            marsLogManager.WriteFNC(EQ.p_nRunLP, BaseDefine.LOG_DEVICE_ID, "GetThicness", SSLNet.STATUS.START);

                        thicknessQueue.Enqueue(i);
                    }
                  

                    if (i < m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count - 1)
                    {
                        x = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i + 1]].x;
                        y = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i + 1]].y;
                        dX = centerX - x * 10000;
                        dY = centerY - y * 10000;

                        MeasurePoint = new RPoint(dX, dY);

                        dataFormatter.AddData("X Axis", dX, "Pulse");
                        dataFormatter.AddData("Y Axis", dY, "Pulse");
                        marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.START, dataFormatter);

                        isMove = true;
                        if (m_module.Run(axisXY.StartMove(MeasurePoint)))
                            return p_sInfo;


                        m_mwvm.p_ArrowX1 = x * RatioX;
                        m_mwvm.p_ArrowY1 = -y * RatioY;
                        if (i < m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count - 2)
                        {
                            double x2 = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i + 2]].x;
                            double y2 = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i + 2]].y;
                            m_mwvm.p_ArrowX2 = x2 * RatioX;
                            m_mwvm.p_ArrowY2 = -y2 * RatioY;
                            m_mwvm.p_ArrowVisible = Visibility.Visible;
                        }
                    }

                    if (isMove)
                    {
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;

                        marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.END, dataFormatter);
                        dataFormatter.ClearData();
                        isMove = false;
                    }

                }

                m_mwvm.p_ArrowVisible = Visibility.Hidden;

            }
            else if (m_isPointMeasure)
            {
                LibSR_Met.DataManager.GetInstance().nRepeatCount = 1;
                if (!MakeSaveDirectory(true, 1))
                {
                    return "Make Directory Error";
                }

                if (App.m_nanoView.SampleMeasure(0, m_ptMeasure.X, m_ptMeasure.Y,
                      m_mwvm.SettingViewModel.p_ExceptNIR, m_DataManager.recipeDM.MeasurementRD.UseTransmittance, m_DataManager.recipeDM.MeasurementRD.UseThickness,
                      m_DataManager.recipeDM.MeasurementRD.LowerWaveLength, m_DataManager.recipeDM.MeasurementRD.UpperWaveLength) != Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    return "Layer Model Not Ready";
                }
                thicknessQueue.Enqueue(0);
            }

            MeasureDone = true;

            m_log.Warn("Calc Thickness 대기 >> " + test.ElapsedMilliseconds);
            while (!m_CalcThicknessDone)
            {
                Thread.Sleep(0);
                if (EQ.IsStop())
                {
                    return "EQ Stop";
                }
            }
            m_log.Warn("Calc Thickness 끝 >> " + test.ElapsedMilliseconds);
            marsLogManager.WriteFNC(EQ.p_nRunLP, BaseDefine.LOG_DEVICE_ID, "GetThicness", SSLNet.STATUS.END);
            //? 세이브?

            m_bStart = false;
            test.Stop();
            m_log.Warn("Measure End >> " + test.ElapsedMilliseconds);

            if (!m_isPointMeasure)
            {
                SaveSlotData(m_DataManager.recipeDM.MeasurementRD.MeasureRepeatCount);
            }
            else
            {
                SaveSlotData(1);
            }
                //// 레드로 빼버림?  contour는 일단 보류..
                //LibSR_Met.DataManager.GetInstance().AllContourMapDataFitting(m_DataManager.recipeDM.MeasurementRD.WaveLengthReflectance, m_DataManager.recipeDM.MeasurementRD.WaveLengthTransmittance, m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count);
                ////m_mwvm.p_ContourMapGraph.InitializeContourMap();
                //// m_mwvm.p_ContourMapGraph.DrawAllDatas();
                ////  DCOL 세이브 필요
                ////if(m_module.p_infoWafer != null)
                ////{
                //LibSR_Met.DataManager MetData = LibSR_Met.DataManager.GetInstance();
                //foreach (LibSR_Met.ContourMapData mapdata in MetData.m_ContourMapDataR)
                //    LibSR_Met.DataManager.GetInstance().SaveContourMapData(m_slotContourMapPath + "\\R_" + mapdata.Wavelength.ToString() + "_" + DateTime.Now.ToString("HHmmss") + "_ContourMapData.csv", mapdata);

                //foreach (LibSR_Met.ContourMapData mapdata in MetData.m_ContourMapDataT)
                //    LibSR_Met.DataManager.GetInstance().SaveContourMapData(m_slotContourMapPath + "\\T_" + mapdata.Wavelength.ToString() + "_" + DateTime.Now.ToString("HHmmss") + "_ContourMapData.csv", mapdata);
                //for (int n = 1; n < MetData.m_LayerData.Count - 1; n++)
                //{
                //    string sLayerName = "";
                //    for (int s = 0; s < MetData.m_LayerData[n].hostname.Length; s++)
                //    {
                //        sLayerName += MetData.m_LayerData[n].hostname[s];
                //    }
                //    LibSR_Met.DataManager.GetInstance().SaveCotourMapThicknessData(m_slotContourMapPath + "\\" + n.ToString() + "Layer_" + sLayerName + "_" + DateTime.Now.ToString("HHmmss") + "_ContourMapData.csv", n, m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count);
                //}

                //if (m_module.p_infoWafer != null)
                //{
                //    LibSR_Met.DataManager.GetInstance().SaveResultFileSummary(m_summaryPath + "\\" + DateTime.Now.ToString("HHmmss") + "Summary.csv", m_module.p_infoWafer.p_sLotID, m_module.p_infoWafer.p_sSlotID, m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count);

                //}
                //else
                //{
                //    LibSR_Met.DataManager.GetInstance().SaveResultFileSummary(m_summaryPath + "\\" + DateTime.Now.ToString("HHmmss") + "Summary.csv", "NoInfowaferLot", "NoInfowaferSlot", m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count);
                //}

                ////}

                marsLogManager.WritePRC(EQ.p_nRunLP, deviceID, SSLNet.PRC_EVENTID.StepProcess, SSLNet.STATUS.END, "Measure", (int)BaseDefine.Process.Measure);
            marsLogManager.WritePRC(EQ.p_nRunLP, deviceID, SSLNet.PRC_EVENTID.Process, SSLNet.STATUS.END, this.p_id, 0);
            marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Move Ready Position", SSLNet.STATUS.START);
            if (m_module.RunMoveReady() != "OK")
            {
                return "Move Ready pos Error";
            }
            marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Move Ready Position", SSLNet.STATUS.END);
            return "OK";
        }

        void SaveRawData(int index)
        {

            if (m_module.p_infoWafer != null)
            {
                Met.DataManager.GetInstance().SaveRawData(@"C:\Users\ATI\Desktop\MeasureData\" + m_module.p_infoWafer.p_id + "_" + DateTime.Now.ToString("HH-mm-ss") + "_" + index, index);
                //Thread.Sleep(3000);
            }
            else
            {
                Met.DataManager.GetInstance().SaveRawData(@"C:\Users\ATI\Desktop\MeasureData\" + "test_" + DateTime.Now.ToString("HH-mm-ss") + "_" + index, index);
                //Thread.Sleep(3000);
            }


            //isSaveDone = true;
        }

        private bool SaveSlotData(int nRepeatCount)
        {
            int nTotalRawDataIndex = 0;
            int nPointIndex = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count;
            if (nRepeatCount == 1)
            {
                nTotalRawDataIndex = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count;
            }
            else
            {
                // 여기서의 nPointIndex는 Repeat * WaferMeasure Point 개수라는 뜻 (다시 확인해서 수정 필요)
                nTotalRawDataIndex = Convert.ToInt32(m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count * nRepeatCount);
            }
            LibSR_Met.DataManager.GetInstance().AllContourMapDataFitting(m_DataManager.recipeDM.MeasurementRD.WaveLengthReflectance, m_DataManager.recipeDM.MeasurementRD.WaveLengthTransmittance, nTotalRawDataIndex);

            // DCOL DATA Save 추가하기

            try
            {
                if (m_module.p_infoWafer == null)
                {
                    return false;
                }
                LibSR_Met.DataManager m_Met = LibSR_Met.DataManager.GetInstance();

                if (nRepeatCount == 1)
                {

                   
                    string sSlotContourMapPath = m_slotContourMapPath + "\\" + m_module.p_infoWafer.p_sLotID + "-" + DateTime.Now.ToString("yyyy.MM.dd") + "_" + DateTime.Now.ToString("HH.mm.ss") + "_ContourMapData";

                    // ContourMap foder fail Save
                    foreach (LibSR_Met.ContourMapData mapdata in m_Met.m_ContourMapDataR)
                        m_Met.SaveContourMapData(sSlotContourMapPath + "_R" + mapdata.Wavelength.ToString() + ".csv", mapdata, nRepeatCount, 1);
                    foreach (LibSR_Met.ContourMapData mapdata in m_Met.m_ContourMapDataT)
                        m_Met.SaveContourMapData(sSlotContourMapPath + "_T" + mapdata.Wavelength.ToString() + ".csv", mapdata, nRepeatCount, 1);
                    for (int n = 1; n < m_Met.m_LayerData.Count - 1; n++)
                    {
                        string sLayerName = string.Empty;
                        for (int s = 0; s < m_Met.m_LayerData[n].hostname.Length; s++)
                        {
                            sLayerName += m_Met.m_LayerData[n].hostname[s];
                        }
                        m_Met.SaveCotourMapThicknessData(sSlotContourMapPath + "_THK_" + n.ToString() + "Layer_" + sLayerName + ".csv", n, nPointIndex, nRepeatCount, 1);
                    }
                    // 함수 인자 정리 하기 Slot 파일 처럼
                    string sSummartPath = m_summaryPath + "\\" + m_module.p_infoWafer.p_sLotID + "-" + DateTime.Now.ToString("yyyy.MM.dd") + "_" + DateTime.Now.ToString("HH.mm.ss") + "_Summary" + m_module.p_infoWafer.p_sSlotID + ".csv";
                    m_Met.SaveResultFileSummary(sSummartPath, m_module.p_infoWafer.p_sLotID, m_module.p_infoWafer.p_sSlotID, nPointIndex);

                    // 데이터 새로 추가 
                    string sLotResultPath = m_resultPath + "\\" + m_module.p_infoWafer.p_sLotID + "-" + DateTime.Now.ToString("yyyy.MM.dd") + "_" + DateTime.Now.ToString("HH.mm.ss") + ".csv";
                    m_Met.SaveResultFileLot(sLotResultPath, m_module.p_infoWafer, m_DataManager.recipeDM, nPointIndex);
                }
                else
                {
                    // 여기서의 nPointIndex는 Repeat * WaferMeasure Point 개수라는 뜻 (다시 확인해서 수정 필요)
                    int nTotalPointIndex = Convert.ToInt32(m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count * nRepeatCount);
                    for (int cnt = 0; cnt< nRepeatCount; cnt++)
                    {
                        string sSlotContourMapPath = m_resultDataSavePath[cnt] + "\\Slot." + m_module.p_infoWafer.m_nSlot + "\\ContourMap" + "\\" + m_module.p_infoWafer.p_sLotID + "-" + DateTime.Now.ToString("yyyy.MM.dd") + "_" + DateTime.Now.ToString("HH.mm.ss") + "_ContourMapData";
                        // ContourMap foder fail Save
                        foreach (LibSR_Met.ContourMapData mapdata in m_Met.m_ContourMapDataR)
                            m_Met.SaveContourMapData(sSlotContourMapPath + "_R" + mapdata.Wavelength.ToString() + ".csv", mapdata, nRepeatCount, cnt);
                        foreach (LibSR_Met.ContourMapData mapdata in m_Met.m_ContourMapDataT)
                            m_Met.SaveContourMapData(sSlotContourMapPath + "_T" + mapdata.Wavelength.ToString() + ".csv", mapdata, nRepeatCount, cnt);
                        for (int n = 1; n < m_Met.m_LayerData.Count - 1; n++)
                        {
                            string sLayerName = string.Empty;
                            for (int s = 0; s < m_Met.m_LayerData[n].hostname.Length; s++)
                            {
                                sLayerName += m_Met.m_LayerData[n].hostname[s];
                            }
                            m_Met.SaveCotourMapThicknessData(sSlotContourMapPath + "_THK_" + n.ToString() + "Layer_" + sLayerName + ".csv", n, nPointIndex, nRepeatCount, cnt);
                        }
                        string sSummartPath = m_resultDataSavePath[cnt] + "\\ResultData_Summary" + "\\" + m_module.p_infoWafer.p_sLotID + "-" + DateTime.Now.ToString("yyyy.MM.dd") + "_" + DateTime.Now.ToString("HH.mm.ss") + "_Summary" + m_module.p_infoWafer.p_sSlotID + ".csv";
                        m_Met.SaveResultFileSummary(sSummartPath, m_module.p_infoWafer.p_sLotID, m_module.p_infoWafer.p_sSlotID, nPointIndex, nRepeatCount, cnt);


                        string sLotResultPath = m_resultDataSavePath[cnt] + "\\ResultData" + "\\" + m_module.p_infoWafer.p_sLotID + "-" + DateTime.Now.ToString("yyyy.MM.dd") + "_" + DateTime.Now.ToString("HH.mm.ss") + ".csv";
                        m_Met.SaveResultFileLot(sLotResultPath, m_module.p_infoWafer, m_DataManager.recipeDM, nPointIndex, nRepeatCount, cnt);
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

    }
}
