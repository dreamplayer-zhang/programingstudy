using Root_CAMELLIA.Data;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Gem.XGem;
using RootTools.Module;
using RootTools.Trees;
using SSLNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        XGem_New p_xGem;
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

            p_xGem = (XGem_New)m_module.p_xGem;
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

        struct MeasureItem
        {
            public int m_index;
            public int m_repeat;

            public MeasureItem(int index, int repeat)
            {
                m_index = index;
                m_repeat = repeat;
            }
        }

        ConcurrentQueue<MeasureItem> thicknessQueue = new ConcurrentQueue<MeasureItem>();
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
            int nTotalRawDataIndex = 0;
            int nDataIndex = 0;
            int nRepeatCount = m_DataManager.recipeDM.MeasurementRD.MeasureRepeatCount;

            while (m_bStart)
            {
                MeasureItem item;
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
                    while (thicknessQueue.TryDequeue(out item))
                    {

                    }
                    m_CalcThicknessDone = true;
                    break;
                }

                if (thicknessQueue.TryDequeue(out item))
                {
                    sw.Start();
                    if (m_DataManager.recipeDM.MeasurementRD.UseThickness)
                    {
                        Thread.Sleep(1);
                        string sMeasurePoint = string.Empty;
                        if (nRepeatCount == 1)
                        {
                            nTotalRawDataIndex = (item.m_index + 1);
                            sMeasurePoint = nTotalRawDataIndex.ToString();
                        }
                        else
                        {
                            // 여기서의 nPointIndex는 Repeat * WaferMeasure Point 개수라는 뜻 (다시 확인해서 수정 필요)
                            nTotalRawDataIndex++;
                            sMeasurePoint += (item.m_index + 1);
                            sMeasurePoint += "-" + (item.m_repeat + 1).ToString();
                        }
                        nDataIndex = nTotalRawDataIndex - 1;
                        //nDataIndex = item.m_index;

                        Met.Nanoview.ERRORCODE_NANOVIEW rst = App.m_nanoView.GetThickness(nDataIndex, m_DataManager.recipeDM.MeasurementRD.LMIteration, m_DataManager.recipeDM.MeasurementRD.DampingFactor, sMeasurePoint, m_mwvm.SettingViewModel.p_ExceptNIR, useAlphafit);
                        if (rst != Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                        {
                            //string NANOVIEWERRORCODE =  rst.ToString();
                            m_log.Warn(Enum.GetName(typeof(Met.Nanoview.ERRORCODE_NANOVIEW), rst));
                        }
                        if (m_isPM)
                        {
                            m_mwvm.EngineerViewModel.p_pmParameter.p_alpha1 = App.m_nanoView.GetAlphaFit();
                        }
                    }

                    m_mwvm.p_Progress = (double)(item.m_index + 1) / m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count * 100;
                    //SaveRawData(item.m_index);
                    //.DataManager MetData = LibSR_Met.DataManager.GetInstance();
                    // Spectrum data Thread 추가 두개두개두개
                    //LibSR_Met.DataManager.GetInstance().SaveResultFileSlot(@"C:\Users\ATI\Desktop\SaveTest\" + index + "_" + DateTime.Now.ToString("HHmmss") + "test.csv", m_module.p_infoWafer.p_sCarrierID,
                    //    m_module.p_infoWafer.p_sLotID, BaseDefine.TOOL_NAME, m_module.p_infoWafer.p_sWaferID, m_module.p_infoWafer.p_sSlotID,
                    //    BaseDefine.Configuration.Version, m_DataManager.recipeDM.TeachRecipeName, index,
                    //    m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[index]].x,
                    //    m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[index]].y,
                    //    m_DataManager.recipeDM.MeasurementRD.LowerWaveLength,
                    //    m_DataManager.recipeDM.MeasurementRD.UpperWaveLength);
                    string sSlotSpectraDataPath = string.Empty;
                    if (nRepeatCount ==1)
                    {
                        int nDataNum = item.m_index + 1;
                        sSlotSpectraDataPath = m_resultDataSavePath[0] + "\\Slot." + m_module.p_infoWafer.m_nSlot + "\\SpectraData" + "\\" + nDataNum.ToString() + "_" + DateTime.Now.ToString("HHmmss");
                    }
                    else
                    {
                        int nPathIndex = item.m_repeat;
                        int nDataNum = item.m_index + 1;
                        sSlotSpectraDataPath = m_resultDataSavePath[nPathIndex] + "\\Slot." + m_module.p_infoWafer.m_nSlot + "\\SpectraData" + "\\" + nDataNum.ToString() + "_" + DateTime.Now.ToString("HHmmss");

                    }
                    LibSR_Met.DataManager.GetInstance().SaveResultFileSlot(sSlotSpectraDataPath, m_module.p_infoWafer, m_DataManager.recipeDM, nDataIndex, item.m_index);
                    
                    //SaveRT
                    LibSR_Met.DataManager.GetInstance().SaveRT(m_historyRTDataPath + "\\" + item.m_index + "_" + DateTime.Now.ToString("HHmmss") + "RawData.csv", item.m_index);

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
        string[] m_resultDataSavePath = new string[100];
        
        private bool MakeSaveDirectory(int nRepeatCount)
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
                string lotStartTime = m_module.p_dataSavePathDate;
                if (nRepeatCount == 1)
                {
                    rootPath += @"\" + lotStartTime;
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
                        //rootPath += @"\" + m_lotStartTime;
                        m_resultDataSavePath[n] = rootPath + @"\" + lotStartTime + "_" + (n + 1).ToString();
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

            
            Axis axisLifter = m_module.p_axisLifter;
            if (m_module.LifterDown() != "OK")
            {
                return p_sInfo;
            }

            m_thread = new Task(RunThread);
            m_thread.Start();

            InfoWafer info = m_module.p_infoWafer;


            marsLogManager.WritePRC(EQ.p_nRunLP, deviceID, SSLNet.PRC_EVENTID.StepProcess, SSLNet.STATUS.START, MATERIAL_TYPE.WAFER, "Measure", (int)BaseDefine.Process.Measure);

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
                int nRepeatCount = m_DataManager.recipeDM.MeasurementRD.MeasureRepeatCount;
                LibSR_Met.DataManager.GetInstance().nRepeatCount = nRepeatCount;
                LibSR_Met.DataManager.GetInstance().nPointCount = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count;
                
                if (!MakeSaveDirectory(nRepeatCount))
                {
                    return "Make Directory Error";
                }
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
                        marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.START, dataFormatter: dataFormatter);

                        if (m_module.Run(axisXY.StartMove(MeasurePoint)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.END, dataFormatter: dataFormatter);
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
                        string sMeasureIndex = string.Empty;
                        if (m_DataManager.recipeDM.MeasurementRD.MeasureRepeatCount == 1)
                        {
                            nTotalRawDataIndex = i+1;
                            sMeasureIndex = nTotalRawDataIndex.ToString();
                        }
                        else
                        {
                            // 여기서의 nPointIndex는 Repeat * WaferMeasure Point 개수라는 뜻 (다시 확인해서 수정 필요)
                            nTotalRawDataIndex++;
                            sMeasureIndex += (i + 1).ToString();
                            sMeasureIndex += "-" + (cnt + 1).ToString();
                        }
                        
                        dataFormatter.AddData("Measure Repeat", m_DataManager.recipeDM.MeasurementRD.MeasureRepeatCount);
                        marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Measure", SSLNet.STATUS.START);
                        Thread.Sleep(10);
                        Met.Nanoview.ERRORCODE_NANOVIEW rst = App.m_nanoView.SampleMeasure((nTotalRawDataIndex-1), x, y,
        m_mwvm.SettingViewModel.p_ExceptNIR, m_DataManager.recipeDM.MeasurementRD.UseTransmittance, m_DataManager.recipeDM.MeasurementRD.UseThickness,
        m_DataManager.recipeDM.MeasurementRD.LowerWaveLength, m_DataManager.recipeDM.MeasurementRD.UpperWaveLength, sMeasureIndex);
                        if (rst != Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                        {

                            if ((int)rst == 10)
                            {
                                m_log.Warn("Unknown Error");
                            }
                            else if(rst == Met. Nanoview.ERRORCODE_NANOVIEW.NANOVIEW_ERROR)
                            {
                                m_log.Warn("NANOVIEW_ERROR_NOT_FOUND_BETA_FILE");
                                // Init Calibration 시퀀스 추가 필요
                            }
                            else
                            {
                                m_log.Warn(Enum.GetName(typeof(Met.Nanoview.ERRORCODE_NANOVIEW), rst));
                            }
                           
                            for (int re = 0; re < 5; re++)
                            {
                                rst = App.m_nanoView.SampleMeasure((nTotalRawDataIndex - 1), x, y,
       m_mwvm.SettingViewModel.p_ExceptNIR, m_DataManager.recipeDM.MeasurementRD.UseTransmittance, m_DataManager.recipeDM.MeasurementRD.UseThickness,
       m_DataManager.recipeDM.MeasurementRD.LowerWaveLength, m_DataManager.recipeDM.MeasurementRD.UpperWaveLength, sMeasureIndex);
                                
                                if (rst != Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                                {
                                    
                                    string NANOVIEWERRORCODE = rst.ToString();
                                    m_log.Warn(sMeasureIndex + " RE-SNAP FAIL" + ":" + (re+1).ToString() + NANOVIEWERRORCODE);
                                }
                                else
                                {
                                    m_log.Warn(sMeasureIndex + " RE-SNAP DONE" + ":" +(re+1).ToString());
                                    break;
                                }
                            }
                           
                        }
                        marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Measure", SSLNet.STATUS.END);
                        dataFormatter.ClearData();
                        StopWatch sw = new StopWatch();
                        sw.Start();

                        if (i == 0 && cnt == 0)
                            marsLogManager.WriteFNC(EQ.p_nRunLP, BaseDefine.LOG_DEVICE_ID, "GetThicness", SSLNet.STATUS.START);

                        thicknessQueue.Enqueue(new MeasureItem(i, cnt));
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
                        marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.START, dataFormatter: dataFormatter);

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

                        marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Stage Move", SSLNet.STATUS.END, dataFormatter: dataFormatter);
                        dataFormatter.ClearData();
                        isMove = false;
                    }

                }

                m_mwvm.p_ArrowVisible = Visibility.Hidden;

            }
            else if (m_isPointMeasure)
            {
                LibSR_Met.DataManager.GetInstance().nRepeatCount = 1;
                if (!MakeSaveDirectory(1))
                {
                    return "Make Directory Error";
                }

                if (App.m_nanoView.SampleMeasure(0, m_ptMeasure.X, m_ptMeasure.Y,
                      m_mwvm.SettingViewModel.p_ExceptNIR, m_DataManager.recipeDM.MeasurementRD.UseTransmittance, m_DataManager.recipeDM.MeasurementRD.UseThickness,
                      m_DataManager.recipeDM.MeasurementRD.LowerWaveLength, m_DataManager.recipeDM.MeasurementRD.UpperWaveLength) != Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    return "Layer Model Not Ready";
                }
                thicknessQueue.Enqueue(new MeasureItem(0, 1));
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

            p_processEndDate = DateTime.Now.ToString("MM/dd/yyyy");
            p_processEndTime = DateTime.Now.ToString("HH:mm:ss");

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

            marsLogManager.WritePRC(EQ.p_nRunLP, deviceID, SSLNet.PRC_EVENTID.StepProcess, SSLNet.STATUS.END, MATERIAL_TYPE.WAFER, "Measure", (int)BaseDefine.Process.Measure);
            marsLogManager.WritePRC(EQ.p_nRunLP, deviceID, SSLNet.PRC_EVENTID.Process, SSLNet.STATUS.END, MATERIAL_TYPE.WAFER, this.p_id, 0);
            //marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Move Ready Position", SSLNet.STATUS.START);
            if (m_module.RunMoveReady() != "OK")
            {
                return "Move Ready pos Error";
            }
            //marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Move Ready Position", SSLNet.STATUS.END);
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


        private enum eDColData
        {
            LotID,
            CarrierID,
            SlotID,
            WaferID,
            Recipe_Name,
            Process_Start_Date,
            Process_Start_Time,
            Process_End_Date,
            Process_End_Time,
            Data_Upload_Date,
            Data_Upload_Time,
            Number_Of_OS,
            Number_Of_layer,
            GOF_Average,
            GOF_Minimum,
            GOF_Maximum,
            GOF_1_Sigma,
            GOF_3_Sigma,
            THK_Average,
            THK_Minimum,
            THK_Maximum,
            THK_1_Sigma,
            THK_3_Sigma,
            Layer_Meterial_Infomation,
        }

        private enum eDcolData_OS
        {
            OS_X_Position,
            OS_X_Position_Offset,
            OS_Y_Position,
            OS_Y_Position_Offset,
            Total_Thickness,
            Thickness_Detail,
            GOF,
            Wave_Length_for_Reflectance,
            Reflectance,
            Wave_Length_for_Transmittance,
            Transmittance,
        }
        private enum eDColIndex
        {
            GOF_Summery,
            THK_Summery,
            X,
            Y,
            X_Offset,
            Y_Offset,
            GOF,
            THK,
            THK_Layer,
            Reflectance_Range,
            Transmiitance_Range,
            Site,
        }

        int[] m_nDcolIndex = new int[Enum.GetNames(typeof(eDColIndex)).Length];
        string[] m_sDcolData = new string[Enum.GetNames(typeof(eDColData)).Length];
        string[,] m_sDcolData_OS = new string[1500, Enum.GetNames(typeof(eDcolData_OS)).Length];

        string p_processEndDate { get; set; } = "";
        string p_processEndTime { get; set; }

        private void DCOL_Set_Data()
        {
            InfoWafer infoWafer = m_module.p_infoWafer;
            m_sDcolData[(int)eDColData.LotID] = infoWafer.p_sLotID;
            m_sDcolData[(int)eDColData.CarrierID] = infoWafer.p_sCarrierID;
            m_sDcolData[(int)eDColData.Recipe_Name] = infoWafer.p_sRecipe;
            m_sDcolData[(int)eDColData.SlotID] = infoWafer.p_sLotID;
            m_sDcolData[(int)eDColData.WaferID] = infoWafer.p_sWaferID;
            m_sDcolData[(int)eDColData.Process_Start_Date] = m_module.p_processStartDate;
            m_sDcolData[(int)eDColData.Process_Start_Time] = m_module.p_processStartTime;
            m_sDcolData[(int)eDColData.Process_End_Date] = p_processEndDate;
            m_sDcolData[(int)eDColData.Process_End_Time] = p_processEndTime;
        }
        private void DCOL_SCVFile_Parsing(string file)
        {
            int m_nRefNum = 0;
            int m_nTransNum = 0;

            using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fileStream, Encoding.UTF8, false))
                {
                    string[] data = null;
                    string line = null;
                    int nTemp = 0;
                    float fTemp = 0;
                    bool bXREF = false;

                    int nParsing = 0;

                    while ((line = sr.ReadLine()) != null)
                    {
                        data = line.Split(',');

                        switch (data[0])
                        {
                            case "RESULT TYPE":
                                m_sDcolData[(int)eDColData.Number_Of_layer] = LayerNum(data).ToString();
                                m_nDcolIndex[(int)eDColIndex.GOF_Summery] = GetStringPosition(data, "NGOF");
                                m_nDcolIndex[(int)eDColIndex.THK_Summery] = GetStringPosition(data, "NGOF") + 1; //GetStringPosition(data, "Sum");
                                nParsing++;
                                break;
                            case "MEAN":
                                m_sDcolData[(int)eDColData.GOF_Average] = data[m_nDcolIndex[(int)eDColIndex.GOF_Summery]];
                                m_sDcolData[(int)eDColData.THK_Average] = data[m_nDcolIndex[(int)eDColIndex.THK_Summery]];
                                nParsing++;
                                break;
                            case "MIN":
                                m_sDcolData[(int)eDColData.GOF_Minimum] = data[m_nDcolIndex[(int)eDColIndex.GOF_Summery]];
                                m_sDcolData[(int)eDColData.THK_Minimum] = data[m_nDcolIndex[(int)eDColIndex.THK_Summery]];
                                nParsing++;
                                break;
                            case "MAX":
                                m_sDcolData[(int)eDColData.GOF_Maximum] = data[m_nDcolIndex[(int)eDColIndex.GOF_Summery]];
                                m_sDcolData[(int)eDColData.THK_Maximum] = data[m_nDcolIndex[(int)eDColIndex.THK_Summery]];
                                nParsing++;
                                break;
                            case "STDDEV":
                                m_sDcolData[(int)eDColData.GOF_1_Sigma] = data[m_nDcolIndex[(int)eDColIndex.GOF_Summery]];
                                m_sDcolData[(int)eDColData.THK_1_Sigma] = data[m_nDcolIndex[(int)eDColIndex.THK_Summery]];
                                nParsing++;
                                break;
                            case "3 SIGMA":
                                m_sDcolData[(int)eDColData.GOF_3_Sigma] = data[m_nDcolIndex[(int)eDColIndex.GOF_Summery]];
                                m_sDcolData[(int)eDColData.THK_3_Sigma] = data[m_nDcolIndex[(int)eDColIndex.THK_Summery]];
                                nParsing++;
                                break;
                            case "Site #":
                                m_nDcolIndex[(int)eDColIndex.X] = GetStringPosition(data, "X");
                                m_nDcolIndex[(int)eDColIndex.Y] = GetStringPosition(data, "Y");
                                m_nDcolIndex[(int)eDColIndex.X_Offset] = GetStringPosition(data, "OFFSET X");
                                m_nDcolIndex[(int)eDColIndex.Y_Offset] = GetStringPosition(data, "OFFSET Y");
                                m_nDcolIndex[(int)eDColIndex.THK_Layer] = GetStringPosition(data, "Site #") + 1;
                                m_nDcolIndex[(int)eDColIndex.GOF] = GetStringPosition(data, "NGOF");
                                m_nDcolIndex[(int)eDColIndex.THK] = GetStringPosition(data, "Sum");
                                nParsing++;
                                break;
                            case "X_Ref":
                                m_nRefNum = GetStringNum(data, "R_");
                                m_nTransNum = GetStringNum(data, "T_");
                                m_nDcolIndex[(int)eDColIndex.Reflectance_Range] = GetStringPosition_Indexof(data, "R_");
                                m_nDcolIndex[(int)eDColIndex.Transmiitance_Range] = GetStringPosition_Indexof(data, "T_");
                                m_nDcolIndex[(int)eDColIndex.Site] = GetStringPosition(data, "Site");

                                for (int i = 0; i < m_nRefNum; i++)
                                {
                                    m_sDcolData_OS[0, (int)eDcolData_OS.Wave_Length_for_Reflectance] += data[m_nDcolIndex[(int)eDColIndex.Reflectance_Range] + i].Replace("R_", "") + "_";

                                }

                                for (int i = 0; i < m_nTransNum; i++)
                                {
                                    m_sDcolData_OS[0, (int)eDcolData_OS.Wave_Length_for_Transmittance] += data[m_nDcolIndex[(int)eDColIndex.Transmiitance_Range] + i].Replace("T_", "") + "_";
                                }
                                bXREF = true;
                                nParsing++;
                                break;
                            default:
                                if (Int32.TryParse(data[0], out nTemp) && !bXREF)
                                {

                                    m_sDcolData_OS[nTemp, (int)eDcolData_OS.OS_X_Position] = data[m_nDcolIndex[(int)eDColIndex.X]];
                                    m_sDcolData_OS[nTemp, (int)eDcolData_OS.OS_Y_Position] = data[m_nDcolIndex[(int)eDColIndex.Y]]; //201102 JWS
                                    m_sDcolData_OS[nTemp, (int)eDcolData_OS.OS_X_Position_Offset] = data[m_nDcolIndex[(int)eDColIndex.X_Offset]];
                                    m_sDcolData_OS[nTemp, (int)eDcolData_OS.OS_Y_Position_Offset] = data[m_nDcolIndex[(int)eDColIndex.Y_Offset]];
                                    m_sDcolData_OS[nTemp, (int)eDcolData_OS.Total_Thickness] = data[m_nDcolIndex[(int)eDColIndex.THK]];
                                    m_sDcolData_OS[nTemp, (int)eDcolData_OS.GOF] = data[m_nDcolIndex[(int)eDColIndex.GOF]];
                                    for (int i = 0; i < Convert.ToInt32(m_sDcolData[(int)eDColData.Number_Of_layer]); i++)
                                    {
                                        m_sDcolData_OS[nTemp, (int)eDcolData_OS.Thickness_Detail] += data[m_nDcolIndex[(int)eDColIndex.THK_Layer] + i] + "_";
                                    }

                                    m_sDcolData[(int)eDColData.Number_Of_OS] = nTemp.ToString();
                                }
                                else if (float.TryParse(data[0], out fTemp) && bXREF)
                                {
                                    int nSite = 0;
                                    try
                                    {
                                        if (Int32.TryParse(data[m_nDcolIndex[(int)eDColIndex.Site]], out nSite))
                                        {
                                            for (int i = 0; i < m_nRefNum; i++)
                                            {
                                                m_sDcolData_OS[nSite, (int)eDcolData_OS.Reflectance] += data[m_nDcolIndex[(int)eDColIndex.Reflectance_Range] + i] + "_";

                                            }
                                            for (int i = 0; i < m_nTransNum; i++)
                                            {
                                                m_sDcolData_OS[nSite, (int)eDcolData_OS.Transmittance] += data[m_nDcolIndex[(int)eDColIndex.Transmiitance_Range] + i] + "_";
                                            }

                                        }
                                    }
                                    catch (Exception)
                                    {
                                        //m_log.Popup(sFile + "의 전송에 실패하였습니다.");
                                        return;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        public int GetStringPosition(string[] strs, string str)
        {
            for (int i = 0; i < strs.Length; i++)
            {
                if (strs[i] == str)
                    return i;
            }
            return 0;
        }

        public int GetStringNum(string[] strs, string str)
        {
            int nNum = 0;
            for (int i = 0; i < strs.Length; i++)
            {
                if (strs[i].IndexOf(str) >= 0)
                    nNum++;
            }

            return nNum;
        }

        public int GetStringPosition_Indexof(string[] strs, string str)
        {
            for (int i = 0; i < strs.Length; i++)
            {
                if (strs[i].IndexOf(str) >= 0)
                    return i;
            }
            return 0;
        }

        public int LayerNum(string[] str)
        {
            int num = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i].IndexOf("RESULT") >= 0 || str[i].IndexOf("NGOF") >= 0 || str[i] == "")
                    continue;
                else
                {
                    num++;
                    m_sDcolData[(int)eDColData.Layer_Meterial_Infomation] += str[i] + "_";
                }

            }
            return num;
        }

        private void SendDCOLMSG(string path)
        {
            if (!m_module.m_engineer.p_bUseXGem)
                return;
            DCOL_Set_Data();
            DCOL_SCVFile_Parsing(path);
            InfoWafer infoWafer = m_module.p_infoWafer;
            p_xGem.SetSV(1100, infoWafer.p_sCarrierID);
            p_xGem.SetSV(1101, infoWafer.p_sLotID);
            p_xGem.SetSV(1102, infoWafer.p_sRecipe);
            p_xGem.SetSV(1103, infoWafer.p_sSlotID);
            p_xGem.SetSV(1104, infoWafer.p_sWaferID);

            m_log.Info("CarrierID : " + infoWafer.p_sCarrierID);
            m_log.Info("LotID : " + infoWafer.p_sLotID);
            m_log.Info("Recipe : " + infoWafer.p_sRecipe);
            m_log.Info("SlotID : " + infoWafer.p_sSlotID);
            m_log.Info("WaferID : " + infoWafer.p_sWaferID);
            // PJID 로드포트 뭐 해야함


            SetResultList();

            p_xGem.SetCEID(8100);
            //p_xGem.SetSV(1110,);
        }

        private void SetStringData(long nObject, eDColData eData)
        {
            string[] strs = m_sDcolData[(int)eData].Split('_');

            p_xGem.SetListItem(nObject, 2);
            p_xGem.SetStringItem(nObject, NameChange(eData));

            if (strs.Length == 1)
            {

                p_xGem.SetStringItem(nObject, m_sDcolData[(int)eData]);
            }
            else if (strs.Length == 2)
            {
                if (eData == eDColData.Layer_Meterial_Infomation)
                {
                    p_xGem.SetListItem(nObject, 1);
                    p_xGem.SetStringItem(nObject, m_sDcolData[(int)eData].Replace("_", ""));
                }
                else
                    p_xGem.SetStringItem(nObject, strs[0]);
            }
            else
            {
                p_xGem.SetListItem(nObject, strs.Length - 1);
                for (int i = 0; i < strs.Length - 1; i++)
                {
                    p_xGem.SetStringItem(nObject, strs[i]);
                }
            }
        }

        private void SetFloatData(long nObject, eDcolData_OS eData, int nOS = 0)
        {
            string[] strs = m_sDcolData_OS[nOS, (int)eData].Split('_');

            p_xGem.SetListItem(nObject, 2);
            p_xGem.SetStringItem(nObject, NameChange(eData));

            if (strs.Length == 1)
            {

                p_xGem.SetFloat4Item(nObject, (float)(Math.Round(float.Parse(m_sDcolData_OS[nOS, (int)eData]), 3)));
            }
            else if (strs.Length == 2)
            {
                if (eData == eDcolData_OS.Thickness_Detail)
                {
                    p_xGem.SetListItem(nObject, 1);
                    p_xGem.SetFloat4Item(nObject, (float)(Math.Round(float.Parse(m_sDcolData_OS[nOS, (int)eData].Replace("_", "")), 3)));
                }
                else
                    p_xGem.SetFloat4Item(nObject, (float)(Math.Round(float.Parse(strs[0]), 3)));
            }
            else
            {
                p_xGem.SetListItem(nObject, strs.Length - 1);
                for (int i = 0; i < strs.Length - 1; i++)
                {
                    p_xGem.SetFloat4Item(nObject, (float)(Math.Round(float.Parse(strs[i]), 3)));
                }
            }
        }

        private void SetFloatData(long nObject, eDColData eData)
        {
            string[] strs = m_sDcolData[(int)eData].Split('_');

            p_xGem.SetListItem(nObject, 2);
            p_xGem.SetStringItem(nObject, NameChange(eData));

            if (strs.Length == 1)
            {
                p_xGem.SetFloat4Item(nObject, (float)(Math.Round(float.Parse(m_sDcolData[(int)eData]), 3)));
            }
            else if (strs.Length == 2)
            {
                p_xGem.SetFloat4Item(nObject, (float)(Math.Round(float.Parse(strs[0]), 3)));
            }
            else
            {
                p_xGem.SetListItem(nObject, strs.Length - 1);
                for (int i = 0; i < strs.Length - 1; i++)
                {
                    p_xGem.SetFloat4Item(nObject, (float)(Math.Round(float.Parse(strs[i]), 3)));
                }
            }
        }

        private string NameChange(eDColData eData)
        {
            return Enum.GetName(typeof(eDColData), (int)eData).Replace("_", " ");
        }
        private string NameChange(eDcolData_OS eData)
        {
            return Enum.GetName(typeof(eDcolData_OS), (int)eData).Replace("_", " ");
        }


        public void SetResultList()
        {
            long nObject = 0;
            p_xGem.MakeObject(nObject);

            m_sDcolData[(int)eDColData.Data_Upload_Date] = DateTime.Now.ToString("MM/dd/yyyy");
            m_sDcolData[(int)eDColData.Data_Upload_Time] = DateTime.Now.ToString("hh:mm:ss");

            p_xGem.SetListItem(nObject, 3);
            SetResultInfomation(nObject);
            //m_log.Add("Result Infomation Set Done ");
            SetOSResult(nObject);
            //m_log.Add("OS Result Set Done ");
            SetResultSummary(nObject);
            //m_log.Add("OS Result Summary Set Done ");

            //int nVID = ((XGem300Data)m_aSV[(int)eSV.ResultList]).m_nID;

            //p_xGem.GEMSetVariables(nObject, 8100);
        }

        public void SetResultInfomation(long nObject)
        {
            p_xGem.SetListItem(nObject, 1);
            p_xGem.SetListItem(nObject, 11);
            p_xGem.SetStringItem(nObject, "Result Infomation");

            SetStringData(nObject, eDColData.Recipe_Name);
            SetStringData(nObject, eDColData.Process_Start_Date);
            SetStringData(nObject, eDColData.Process_Start_Time);
            SetStringData(nObject, eDColData.Process_End_Date);
            SetStringData(nObject, eDColData.Process_End_Time);
            SetStringData(nObject, eDColData.Data_Upload_Date);
            SetStringData(nObject, eDColData.Data_Upload_Time);
            SetStringData(nObject, eDColData.Number_Of_OS);
            SetStringData(nObject, eDColData.Number_Of_layer);
            SetStringData(nObject, eDColData.Layer_Meterial_Infomation);

        }
        public void SetOSResult(long nObject)
        {
            p_xGem.SetListItem(nObject, 1);
            int nOS = Convert.ToInt32(m_sDcolData[(int)eDColData.Number_Of_OS]);
            p_xGem.SetListItem(nObject, nOS);

            for (int i = 1; i <= nOS; i++)
            {
                p_xGem.SetListItem(nObject, 11);
                SetFloatData(nObject, eDcolData_OS.OS_X_Position, i);
                SetFloatData(nObject, eDcolData_OS.OS_X_Position_Offset, i);
                SetFloatData(nObject, eDcolData_OS.OS_Y_Position, i);
                SetFloatData(nObject, eDcolData_OS.OS_Y_Position_Offset, i);
                SetFloatData(nObject, eDcolData_OS.Total_Thickness, i);
                SetFloatData(nObject, eDcolData_OS.Thickness_Detail, i);
                SetFloatData(nObject, eDcolData_OS.GOF, i);
                SetFloatData(nObject, eDcolData_OS.Wave_Length_for_Reflectance, 0);
                SetFloatData(nObject, eDcolData_OS.Reflectance, i);
                SetFloatData(nObject, eDcolData_OS.Wave_Length_for_Transmittance, 0);
                SetFloatData(nObject, eDcolData_OS.Transmittance, i);
                //m_log.Add("Site #" + i.ToString() + "  Data Set Done");
            }
        }
        public void SetResultSummary(long nObject)
        {
            p_xGem.SetListItem(nObject, 1);
            p_xGem.SetListItem(nObject, 2);
            p_xGem.SetStringItem(nObject, "Result Summary");
            p_xGem.SetListItem(nObject, 2);
            p_xGem.SetListItem(nObject, 2);
            p_xGem.SetStringItem(nObject, "Thickness Summary");
            p_xGem.SetListItem(nObject, 5);
            SetFloatData(nObject, eDColData.THK_Average);
            SetFloatData(nObject, eDColData.THK_Minimum);
            SetFloatData(nObject, eDColData.THK_Maximum);
            SetFloatData(nObject, eDColData.THK_1_Sigma);
            SetFloatData(nObject, eDColData.THK_3_Sigma);
            p_xGem.SetListItem(nObject, 2);
            p_xGem.SetStringItem(nObject, "GOF Summary");
            p_xGem.SetListItem(nObject, 5);
            SetFloatData(nObject, eDColData.GOF_Average);
            SetFloatData(nObject, eDColData.GOF_Minimum);
            SetFloatData(nObject, eDColData.GOF_Maximum);
            SetFloatData(nObject, eDColData.GOF_1_Sigma);
            SetFloatData(nObject, eDColData.GOF_3_Sigma);
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
                //string sDCOLDataPath = m_resultDataSavePath[0] + "\\" + m_module.p_infoWafer.p_sLotID + "-" + DateTime.Now.ToString("yyyy.MM.dd") + "_" + DateTime.Now.ToString("HH.mm.ss") + ".csv";
                //m_Met.SaveResultFileDCOL(sDCOLDataPath, m_module.p_infoWafer, m_DataManager.recipeDM, nPointIndex);
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
                    string sDCOLDataPath = m_resultDataSavePath[0] + "\\" + m_module.p_infoWafer.p_sLotID + "-" + DateTime.Now.ToString("yyyy.MM.dd") + "_" + DateTime.Now.ToString("HH.mm.ss") + ".csv";
                    m_Met.SaveResultFileDCOL(sDCOLDataPath, m_module.p_infoWafer, m_DataManager.recipeDM, nPointIndex);

                    SendDCOLMSG(sDCOLDataPath);  //추가 DCOL

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
                            m_Met.SaveCotourMapThicknessData(sSlotContourMapPath + "_THK_" + n.ToString() + "Layer_" + sLayerName + ".csv", n, nTotalPointIndex, nRepeatCount, cnt);
                        }
                        string sSummartPath = m_resultDataSavePath[cnt] + "\\ResultData_Summary" + "\\" + m_module.p_infoWafer.p_sLotID + "-" + DateTime.Now.ToString("yyyy.MM.dd") + "_" + DateTime.Now.ToString("HH.mm.ss") + "_Summary" + m_module.p_infoWafer.p_sSlotID + ".csv";
                        m_Met.SaveResultFileSummary(sSummartPath, m_module.p_infoWafer.p_sLotID, m_module.p_infoWafer.p_sSlotID, nTotalPointIndex, nRepeatCount, cnt);
                        
                        string sDCOLDataPath = m_resultDataSavePath[cnt] + "\\" + m_module.p_infoWafer.p_sLotID + "-" + DateTime.Now.ToString("yyyy.MM.dd") + "_" + DateTime.Now.ToString("HH.mm.ss") + ".csv";
                        m_Met.SaveResultFileDCOL(sDCOLDataPath, m_module.p_infoWafer, m_DataManager.recipeDM, nTotalPointIndex, nRepeatCount, cnt);
                        SendDCOLMSG(sDCOLDataPath); //추가 DCOL

                        if (m_module.p_infoWafer.p_eWaferOrder == InfoWafer.eWaferOrder.FirstWafer || m_module.p_infoWafer.p_eWaferOrder == InfoWafer.eWaferOrder.FirstLastWafer)
                        {
                            m_Met.m_LotDataPath[cnt] = m_resultDataSavePath[cnt] + "\\ResultData" + "\\" + m_module.p_infoWafer.p_sLotID + "-" + DateTime.Now.ToString("yyyy.MM.dd") + "_" + DateTime.Now.ToString("HH.mm.ss") + ".csv";
                        }
                       
                        m_Met.SaveResultFileLot(m_Met.m_LotDataPath[cnt], m_module.p_infoWafer, m_DataManager.recipeDM, nTotalPointIndex, nRepeatCount, cnt);
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
