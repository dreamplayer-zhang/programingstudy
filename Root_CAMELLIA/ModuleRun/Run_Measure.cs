﻿using Root_CAMELLIA.Data;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
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
        bool m_bStart = false;
        bool m_CalcThicknessDone = false;

        public Run_Measure(Module_Camellia module)
        {
            m_module = module;
            m_mwvm = module.mwvm;
            m_DataManager = module.m_DataManager;
            InitModuleRun(module);
        }

        public void RunThread()
        {
            m_bStart = true;
            m_IsCalcThicknessDone = false;
            while (m_bStart)
            {
                int index;
                if (EQ.IsStop())
                {
                    while (ThicknessQueue.TryDequeue(out index)) ;
                }
               
                if (ThicknessQueue.TryDequeue(out index))
                {
                    App.m_nanoView.GetThickness(index, m_DataManager.recipeDM.MeasurementRD.LMIteration, m_DataManager.recipeDM.MeasurementRD.DampingFactor);
                    SaveRawData(index);
                }

                if (ThicknessQueue.Count() < 0)
                {
                    m_IsCalcThicknessDone = true;
                }
            }
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
        bool isEQStop = false;
        private void RunThread()
        {
            m_bStart = true;
            m_CalcThicknessDone = false;
            MeasureDone = false;
            isEQStop = false;
            StopWatch sw = new StopWatch();
            while (m_bStart)
            {
                int index;
                //if (EQ.p_eState == EQ.eState.Error)
                //{
                //    while (thicknessQueue.TryDequeue(out index)) ;
                //    m_CalcThicknessDone = true;
                //    break;
                //}
                if (EQ.IsStop())
                {
                    while (thicknessQueue.TryDequeue(out index)) ;
                    m_CalcThicknessDone = true;
                    break;
                }
                
                if (thicknessQueue.TryDequeue(out index))
                {
                    sw.Start();
                    if (m_DataManager.recipeDM.MeasurementRD.UseThickness)
                    {
                        if (App.m_nanoView.GetThickness(index, m_DataManager.recipeDM.MeasurementRD.LMIteration, m_DataManager.recipeDM.MeasurementRD.DampingFactor) != Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                        {
                            isEQStop = true;
                        }
                    }
                    else
                    {
                        //20210308
                        //m_mwvm.p_RTGraph.DrawReflectanceGraph(index, "Wavelength(nm)", "Reflectance(%)");
                        //m_mwvm.p_RTGraph.DrawTransmittanceGraph(index, "Wavelength(nm)", "Reflectance(%)");
                       
                    }
                    SaveRawData(index);
                    sw.Stop();
                    System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
                }
               
                if (MeasureDone && thicknessQueue.Count() == 0)
                {
                    m_CalcThicknessDone = true;
                    break;
                }
            }
        }
        public override string Run()
        {
          

            Axis axisLifter = m_module.p_axisLifter;
            if (m_module.LifterDown() != "OK")
            {
                return p_sInfo;
            }

            m_thread = new Task(RunThread);
            m_thread.Start();

            m_SettingDataWithErrorCode = App.m_nanoView.LoadSettingParameters();
            Met.SettingData setting = null;
            if (m_SettingDataWithErrorCode.Item2 == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
            {
                setting = m_SettingDataWithErrorCode.Item1;
            }
            else
            {
                return "SettingDataLoad Error";
            }

            //m_module.p_eState = (eState)5;
            AxisXY axisXY = m_module.p_axisXY;
            Axis axisZ = m_module.p_axisZ;

            // stage 48724 , wafer 47932
            if (m_module.Run(axisZ.StartMove(m_dFocusZ_pulse)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;

            //return "OK";

            Camera_Basler VRS = m_module.p_CamVRS;
            ImageData img = VRS.p_ImageViewer.p_ImageData;
            //string strVRSImageDir = "D:\\";
            //string strVRSImageFullPath = "";
            RPoint MeasurePoint;


            Met.DataManager dm = Met.DataManager.GetInstance();
            //App.m_nanoView.UpdateModel();
            dm.ClearRawData();
            if (!m_bUseTestSequence)
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
                    centerX = m_StageCenterPos_pulse.X + (m_DataManager.m_waferCentering.m_ptCenter.X - m_StageCenterPos_pulse.X);
                    centerY = m_StageCenterPos_pulse.Y + (m_DataManager.m_waferCentering.m_ptCenter.Y - m_StageCenterPos_pulse.Y);
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
                //StopWatch sw = new StopWatch();
                for (int i = 0; i < m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count; i++)
                {
                    if (EQ.IsStop())
                    {
                        isEQStop = false;
                        return "EQ Stop";
                    }
                    if (i == 0)
                    {
                        MeasurePoint = new RPoint(dX, dY);
                        if (m_module.Run(axisXY.StartMove(MeasurePoint)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;

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
                        isSaveDone = true;
                    }
                    //  sw.Start();
                   // while (!isSaveDone) ;
                    isSaveDone = false;
                    if (App.m_nanoView.SampleMeasure(i, x, y, m_DataManager.recipeDM.MeasurementRD.VISIntegrationTime, setting.nAverage_VIS, m_DataManager.recipeDM.MeasurementRD.NIRIntegrationTime, setting.nAverage_NIR,
                        m_mwvm.SettingViewModel.p_ExceptNIR, m_DataManager.recipeDM.MeasurementRD.UseTransmittance, m_DataManager.recipeDM.MeasurementRD.UseThickness,
                        m_DataManager.recipeDM.MeasurementRD.LowerWaveLength, m_DataManager.recipeDM.MeasurementRD.UpperWaveLength) != Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                    {
                        isEQStop = false;
                        return "Layer Model Not Ready";
                    }
                    //pp.m_nanoView.
                    StopWatch sw = new StopWatch();
                    sw.Start();

                    //App.m_nanoView.m_SR.m_iteration = 30;
                    //App.m_nanoView.m_SR.m_divratio = 0.01;
                    ////App.m_nanoView.GetThickness(i, m_DataManager.recipeDM.MeasurementRD.LMIteration, m_DataManager.recipeDM.MeasurementRD.DampingFactor);
                    //LibSR_Met.RawData data = LibSR_Met.DataManager.GetInstance().m_RawData[i];
                    ////App.m_nanoView.m_SR.bDispersionFit = false;
                    ////App.m_nanoView.m_SR.WavelengthForNK = 633;
                    //int m_NKFitLayer = 0;
                    //App.m_nanoView.m_SR.NKFitLayer = m_NKFitLayer;
                    //App.m_nanoView.m_SR.Fit(data.VIS_Reflectance, data.VIS_Reflectance, data.eV, LibSR_Met.DataManager.GetInstance().nThicknessDataNum);
                    //sw.Stop();
                    //double thickness = App.m_nanoView.m_SR.Thickness[1];
                    //System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds.ToString());
                    //System.Diagnostics.Debug.WriteLine("Thick : " + thickness.ToString());
                    thicknessQueue.Enqueue(i);
                    //if (m_DataManager.recipeDM.MeasurementRD.UseThickness)
                    //{
                    //    App.m_nanoView.GetThickness(i, m_DataManager.recipeDM.MeasurementRD.LMIteration, m_DataManager.recipeDM.MeasurementRD.DampingFactor);
                    //}
                    //   sw.Stop();
                    //   System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
                    if (i < m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count - 1)
                    {
                        x = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i + 1]].x;
                        y = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i + 1]].y;
                        dX = centerX - x * 10000;
                        dY = centerY - y * 10000;

                        MeasurePoint = new RPoint(dX, dY);

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
                    //obj = i;
                    //ThreadPool.QueueUserWorkItem(SaveRawData, obj);
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
                    //if (VRS.Grab() == "OK")
                    //{
                    //    strVRSImageFullPath = string.Format(strVRSImageDir + "VRSImage_{0}.bmp", i);
                    //    img.SaveImageSync(strVRSImageFullPath);
                    //    //Grab error
                    //}
                    ////Thread.Sleep(600);

                    m_mwvm.p_Progress = (((double)(i + 1) / m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count) * 100);
                }
                m_mwvm.p_ArrowVisible = Visibility.Hidden;

            }
            else
            {
                if (m_module.Run(axisXY.StartMove(m_ptTestMeasurePoint)))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;

                if (App.m_nanoView.SampleMeasure(0, m_ptTestMeasurePoint.X, m_ptTestMeasurePoint.Y, m_DataManager.recipeDM.MeasurementRD.VISIntegrationTime, setting.nAverage_VIS, m_DataManager.recipeDM.MeasurementRD.NIRIntegrationTime, setting.nAverage_NIR,
                       m_mwvm.SettingViewModel.p_ExceptNIR, m_DataManager.recipeDM.MeasurementRD.UseTransmittance, m_DataManager.recipeDM.MeasurementRD.UseThickness,
                       m_DataManager.recipeDM.MeasurementRD.LowerWaveLength, m_DataManager.recipeDM.MeasurementRD.UpperWaveLength) != Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    return "Layer Model Not Ready";
                }
                thicknessQueue.Enqueue(0);
                //App.m_nanoView.GetThickness(0, m_DataManager.recipeDM.MeasurementRD.LMIteration, m_DataManager.recipeDM.MeasurementRD.DampingFactor);
                //object obj;
                //obj = 0;
                //ThreadPool.QueueUserWorkItem(SaveRawData, obj);
            }

            if (isEQStop)
            {
                return "Get Tickness Error";
            }

            MeasureDone = true;
            while (!m_CalcThicknessDone) ;

            //? 세이브?

            //if (m_module.Run(axisXY.StartMove(eAxisPos.Ready)))
            //{
            //    return p_sInfo;
            //}
            if (m_module.Run(axisZ.StartMove(0)))
            {
                return p_sInfo;
            }
            //if (m_module.Run(axisXY.WaitReady()))
            //    return p_sInfo;
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;

            while(!m_IsCalcThicknessDone) ;
            m_bStart = false;

            return "OK";
        }

        void SaveRawData(object obj)
        {
            int i = (int)obj;
            
            if (m_module.p_infoWafer != null)
            {
                Met.DataManager.GetInstance().SaveRawData(@"C:\Users\ATI\Desktop\MeasureData\" + m_module.p_infoWafer.p_id + "_" + DateTime.Now.ToString("HH-mm-ss") + "_" + i, i);
                //Thread.Sleep(3000);
            }
            else
            {
                Met.DataManager.GetInstance().SaveRawData(@"C:\Users\ATI\Desktop\MeasureData\" + "test_" + DateTime.Now.ToString("HH-mm-ss") + "_" + i, i);
                //Thread.Sleep(3000);
            }
            //20210308
            //m_mwvm.p_RTGraph.DrawReflectanceGraph(i, "Wavelength(nm)", "Reflectance(%)");
            //m_mwvm.p_RTGraph.DrawTransmittanceGraph(i, "Wavelength(nm)", "Reflectance(%)");
            //isSaveDone = true;
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
        void SaveRawData(int index)
        {
            Met.DataManager.GetInstance().SaveRawData(@"C:\Users\ATI\Desktop\MeasureData\test" + index, index);
            m_mwvm.p_RTGraph.DrawReflectanceGraph(index, "Wavelength(nm)", "Reflectance(%)");
            m_mwvm.p_RTGraph.DrawTransmittanceGraph(index, "Wavelength(nm)", "Reflectance(%)");
            isSaveDone = true;
        }
    }
}
