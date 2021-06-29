using Root_CAMELLIA.Data;
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
        (Met.SettingData, Met.Nanoview.ERRORCODE_NANOVIEW) m_SettingDataWithErrorCode;
        public RPoint m_StageCenterPos_pulse = new RPoint(); // Pulse
        double m_dResX_um = 1;
        double m_dResY_um = 1;
        double m_dFocusZ_pulse = 1; // Pulse
        bool isSaveDone = false;

        bool m_bUseTestSequence = false;
        RPoint m_ptTestMeasurePoint = new RPoint();

        Task m_thread;
        Task m_taskSave;
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
            run.m_bUseTestSequence = m_bUseTestSequence;
            run.m_ptTestMeasurePoint = m_ptTestMeasurePoint;
            return run;
        }
        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_StageCenterPos_pulse = tree.Set(m_StageCenterPos_pulse, m_StageCenterPos_pulse, "Stage Center Axis Position", "Stage Center Axis Position(Pulse)", bVisible);
            m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Camera X Resolution", "Camera X Resolution(um)", bVisible);
            m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Camera Y Resolution", "Camera Y Resolution(um)", bVisible);
            m_dFocusZ_pulse = tree.Set(m_dFocusZ_pulse, m_dFocusZ_pulse, "Focus Z Position", "Focus Z Position(pulse)", bVisible);
            m_bUseTestSequence = tree.Set(m_bUseTestSequence, m_bUseTestSequence, "Use Test Sequence", "Use Test Sequence", bVisible);
            m_ptTestMeasurePoint = tree.Set(m_ptTestMeasurePoint, m_ptTestMeasurePoint, "Test Point Pulse", "Test Point Pulse", bVisible);

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
                   
                //if (EQ.p_eState == EQ.eState.Error)
                //{
                //    while (thicknessQueue.TryDequeue(out index)) ;
                //    m_CalcThicknessDone = true;
                //    break;
                //}
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
                    else
                    {
                        //isEQStop = false;
                        //20210308
                        //m_mwvm.p_RTGraph.DrawReflectanceGraph(index, "Wavelength(nm)", "Reflectance(%)");
                        //m_mwvm.p_RTGraph.DrawTransmittanceGraph(index, "Wavelength(nm)", "Reflectance(%)");

                    }
                    m_mwvm.p_Progress = (((double)(index + 1) / m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count) * 100);
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
                    LibSR_Met.DataManager.GetInstance().SaveResultFileSlot(m_slotSpectraDataPath + "\\" +index+"_"+DateTime.Now.ToString("HHmmss"), m_module.p_infoWafer, m_DataManager.recipeDM, index);
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

                if (MeasureDone && nThicknessCnt == m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count)
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
        private bool MakeSaveDirectory()
        {
            string rootPath = m_module.p_dataSavePath;
            try
            {
                if(m_module.p_infoWafer == null)
                {
                    return true;
                }
                string[] path = rootPath.Split('\\');
                if(m_module.p_dataSavePath == "")
                {
                    rootPath = BaseDefine.Dir_MeasureSaveRootPath + m_module.p_infoWafer.p_sRecipe;
                }
                //if (System.IO.Directory.Exists(rootPath))
                //{
                //    rootPath = rootPath.Replace(path[path.Length - 1],DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH-mm-ss"));
                //}
                rootPath += @"\" + DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH-mm-ss");
                m_summaryPath = rootPath + "\\ResultData_Summary";
                GeneralTools.MakeDirectory(m_summaryPath);
                m_resultPath = rootPath + "\\ResultData";
                GeneralTools.MakeDirectory(m_resultPath);
                m_slotContourMapPath = rootPath + "\\Slot." + m_module.p_infoWafer.m_nSlot + "\\ContourMap";
                GeneralTools.MakeDirectory(m_slotContourMapPath);
                m_slotSpectraDataPath = rootPath + "\\Slot." + m_module.p_infoWafer.m_nSlot + "\\SpectraData";
                GeneralTools.MakeDirectory(m_slotSpectraDataPath);
                m_historyRTDataPath = BaseDefine.Dir_HistorySaveRootPath + m_module.p_infoWafer.p_sRecipe + "\\RawData\\" + DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("HH-mm-ss");
                GeneralTools.MakeDirectory(m_historyRTDataPath);
            }
            catch(Exception e)
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
            


            if (!MakeSaveDirectory())
            {
                return "Make Directory Error";
            }


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

            //marsLogManager.WritePRC(EQ.p_nRunLP, deviceID, SSLNet.PRC_EVENTID.Process, SSLNet.STATUS.START, this.p_id, 0, materialID:m_module.p_infoWafer.p_id);
            InfoWafer info = m_module.p_infoWafer;


            marsLogManager.WritePRC(EQ.p_nRunLP, deviceID, SSLNet.PRC_EVENTID.StepProcess, SSLNet.STATUS.START, "Measure", (int)BaseDefine.Process.Measure);
            //m_SettingDataWithErrorCode = App.m_nanoView.LoadSettingParameters();
            //Met.SettingData setting = null;
            //if (m_SettingDataWithErrorCode.Item2 == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
            //{
            //    setting = m_SettingDataWithErrorCode.Item1;
            //}
            //else
            //{
            //    return "SettingDataLoad Error";
            //}

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
           
            if (!m_bUseTestSequence && !m_isPointMeasure)
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
                    //centerX = m_DataManager.m_waferCentering.m_ptCenter.X - (m_StageCenterPos_pulse.X - m_DataManager.m_waferCentering.m_ptCenter.X);
                    //centerY = m_DataManager.m_waferCentering.m_ptCenter.Y - (m_StageCenterPos_pulse.Y- m_DataManager.m_waferCentering.m_ptCenter.Y);
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

                    marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Measure", SSLNet.STATUS.START);
                    Met.Nanoview.ERRORCODE_NANOVIEW rst = App.m_nanoView.SampleMeasure(i, x, y,
    m_mwvm.SettingViewModel.p_ExceptNIR, m_DataManager.recipeDM.MeasurementRD.UseTransmittance, m_DataManager.recipeDM.MeasurementRD.UseThickness,
    m_DataManager.recipeDM.MeasurementRD.LowerWaveLength, m_DataManager.recipeDM.MeasurementRD.UpperWaveLength);
                    if (rst != Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                    {
                        //isEQStop = false;
                        m_log.Warn(Enum.GetName(typeof(Met.Nanoview.ERRORCODE_NANOVIEW), rst));
                    }
                    //Thread.Sleep(3);
                    marsLogManager.WriteFNC(EQ.p_nRunLP, deviceID, "Measure", SSLNet.STATUS.END);

                    // SaveReflectance
                    //LibSR_Met.DataManager.GetInstance().SaveReflectance(m_resultPath + "\\" + i + "_" + DateTime.Now.ToString("HHmmss") + "Reflectance.csv", i);

                    StopWatch sw = new StopWatch();
                    sw.Start();

                    if(i == 0)
                        marsLogManager.WriteFNC(EQ.p_nRunLP, BaseDefine.LOG_DEVICE_ID, "GetThicness", SSLNet.STATUS.START);

                    thicknessQueue.Enqueue(i);

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
                if (App.m_nanoView.SampleMeasure(0, m_ptMeasure.X, m_ptMeasure.Y,
                      m_mwvm.SettingViewModel.p_ExceptNIR, m_DataManager.recipeDM.MeasurementRD.UseTransmittance, m_DataManager.recipeDM.MeasurementRD.UseThickness,
                      m_DataManager.recipeDM.MeasurementRD.LowerWaveLength, m_DataManager.recipeDM.MeasurementRD.UpperWaveLength) != Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    return "Layer Model Not Ready";
                }
                thicknessQueue.Enqueue(0);
            }
            else
            {
                if (m_module.Run(axisXY.StartMove(m_ptTestMeasurePoint)))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
                Thread.Sleep(1000);

                double x = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[0]].x;
                double y = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[0]].y;

                if (App.m_nanoView.SampleMeasure(0, x, y,
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

            //if (m_module.Run(axisXY.StartMove(eAxisPos.Ready)))
            //{
            //    return p_sInfo;
            //}
            //if (m_module.Run(axisZ.StartMove(0)))
            //{
            //    return p_sInfo;
            //}
            //if (m_module.Run(axisXY.WaitReady()))
            //    return p_sInfo;
            //if (m_module.Run(axisZ.WaitReady()))
            //    return p_sInfo;

            m_bStart = false;
            test.Stop();
            m_log.Warn("Measure End >> " + test.ElapsedMilliseconds);

            // 레드로 빼버림?  contour는 일단 보류..
            LibSR_Met.DataManager.GetInstance().AllContourMapDataFitting(m_DataManager.recipeDM.MeasurementRD.WaveLengthReflectance, m_DataManager.recipeDM.MeasurementRD.WaveLengthTransmittance, m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count);
            //m_mwvm.p_ContourMapGraph.InitializeContourMap();
           // m_mwvm.p_ContourMapGraph.DrawAllDatas();
            //  DCOL 세이브 필요
            if(m_module.p_infoWafer != null)
            {
                LibSR_Met.DataManager MetData = LibSR_Met.DataManager.GetInstance();
                foreach (LibSR_Met.ContourMapData mapdata in MetData.m_ContourMapDataR)
                    LibSR_Met.DataManager.GetInstance().SaveContourMapData(m_slotContourMapPath + "\\R_" + mapdata.Wavelength.ToString() + "_" + DateTime.Now.ToString("HHmmss") + "_ContourMapData.csv", mapdata);

                foreach (LibSR_Met.ContourMapData mapdata in MetData.m_ContourMapDataT)
                    LibSR_Met.DataManager.GetInstance().SaveContourMapData(m_slotContourMapPath + "\\T_" + mapdata.Wavelength.ToString() + "_" + DateTime.Now.ToString("HHmmss") + "_ContourMapData.csv", mapdata);
                for (int n=1;n< MetData.m_LayerData.Count-1; n++)
                {
                    string sLayerName="";
                    for(int s=0; s< MetData.m_LayerData[n].hostname.Length; s++)
                    {
                        sLayerName += MetData.m_LayerData[n].hostname[s];
                    }
                    LibSR_Met.DataManager.GetInstance().SaveCotourMapThicknessData(m_slotContourMapPath + "\\" + n.ToString() + "Layer_" + sLayerName + "_" + DateTime.Now.ToString("HHmmss") + "_ContourMapData.csv", n, m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count);
                }
                //LibSR_Met.DataManager.GetInstance().AllContourMapDataFitting(m_DataManager.recipeDM.MeasurementRD.WaveLengthReflectance, m_DataManager.recipeDM.MeasurementRD.WaveLengthTransmittance);
                LibSR_Met.DataManager.GetInstance().SaveResultFileSummary(m_summaryPath + "\\" + DateTime.Now.ToString("HHmmss") + "Summary.csv", m_module.p_infoWafer.p_sLotID, m_module.p_infoWafer.p_sSlotID, m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count) ;
               
            }

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
    }
}
