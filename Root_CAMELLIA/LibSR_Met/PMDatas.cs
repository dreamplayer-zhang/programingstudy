using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Drawing;
using System.Windows.Forms;
using OpenCvSharp;

namespace Root_CAMELLIA.LibSR_Met
{
    //여기서 PM prarmeter 로드하고
    // 계산하고
    // 판정 내리고
    public enum CheckResult
    {
        Error = 0,  //PM결과 하드웨어에 이상이 있음
        OK = 1, //PM결과 정상
        CheckError = 2  //측정중 내부 에러 발생
    }

    public enum PMItem
    {
        LightSource,
        SensorTilt,
        Thickness,
        THKRepeatability,
        VacVariation,
        CameraSensorOffset,
        CSSAlign    //Camera Stage Sensor
    }
    public class SensorTiltDatas
    {
        public List<double> Wavelength = new List<double>();
        public List<double> Diff = new List<double>();
    }
    public class PMResult
    {
        public PMItem ItemName;
        public double Measured;
        public string Reference;
        public bool Error;
    }
    public class CalPMReflectance
    {
        public double[] dDiffReflectance;
        public double dWavelength = 0.0;
        public double dMin = 0.0;
        public double dMax = 0.0;
        public double dCop = 0.0;
        public double dAvg = 0.0;
        public double dSTD = 0.0;

        public CalPMReflectance()
        {
            dDiffReflectance = new double[15];
        }
    }
    public class PMDatas
    {
        public List<PMResult> Result;
        public List<SensorTiltDatas> SensorTiltData;
        //Common
        public int nCheckRangeStart; //350[nm]
        public int nCheckRangeEnd;  //1500[nm]

        //Sensor Tilt
        public double dSensorTiltError;    //1
        public int nSensorTiltRepeatNum;

        public List<double> WavelengthRef;
        public List<double> ReflectanceRef;

        public string sCheckResultWavelength;
        public string[] arrCheckWavelength;
        public CalPMReflectance[] m_CalPMReflectance;

        //Sensor-Camera Tilt Check
        public System.Windows.Point Align_CenterPosTop = new System.Windows.Point();    //이미지 상에서의 센서 센터
        public System.Windows.Point Align_CenterPosBot = new System.Windows.Point();
        public double dAlign_ResultDeg = 0.0;

        public int nAlignAxisPosZ;
        public int nSensorCenterX;
        public int nSensorCenterY;
        public double dCameraAxisOffsetZ;//[pulse]
        public double dDistSensorCamera; //[um]
        public int nAlignThreshold;

        DataManager m_DM = DataManager.GetInstance();
        public PMDatas()
        {
            Result = new List<PMResult>();
            SensorTiltData = new List<SensorTiltDatas>();

            WavelengthRef = new List<double>();
            ReflectanceRef = new List<double>();


            m_CalPMReflectance = new CalPMReflectance[ConstValue.PM_REFLECTANCE_CHECK_WAVELENGTH_COUNT];
            for (int i = 0; i < ConstValue.PM_REFLECTANCE_CHECK_WAVELENGTH_COUNT; i++)
            {
                m_CalPMReflectance[i] = new CalPMReflectance();
            }
        }
        public void LoadPMData()
        {
            try
            {
                LoadReflectanceData();

                if (!File.Exists(ConstValue.PATH_PM_FILE))
                {
                    m_DM.m_Log.WriteLog(LogType.PM, "[Error]PM & Monitoring - PM.cpm file is not exist.");
                    return;
                }

                StreamReader sr = new StreamReader(ConstValue.PATH_PM_FILE);

                while (!sr.EndOfStream)
                {
                    string str = sr.ReadLine();
                    string[] datas = str.Split(':');
                    

                    if (datas[1] == string.Empty)
                        continue;

                    switch (datas[0])
                    {
                        case "nCheckRangeStart":
                            nCheckRangeStart = Convert.ToInt32(datas[1]);
                            break;
                        case "nCheckRangeEnd":
                            nCheckRangeEnd = Convert.ToInt32(datas[1]);
                            break;
                        case "dSensorTiltError":
                            dSensorTiltError = Convert.ToDouble(datas[1]);
                            break;
                        case "nSensorTiltRepeatNum":
                            nSensorTiltRepeatNum = Convert.ToInt32(datas[1]);
                            break;
                        case "sCheckResultWavelength":
                            sCheckResultWavelength = datas[1];
                            break;
                        case "nSensorCenterX":
                            nSensorCenterX = Convert.ToInt32(datas[1]);
                            break;
                        case "nSensorCenterY":
                            nSensorCenterY = Convert.ToInt32(datas[1]);
                            break;
                        case "dCameraAxisOffsetZ":
                            dCameraAxisOffsetZ = Convert.ToInt32(datas[1]);
                            break;
                        case "dDistSensorCamera":
                            dDistSensorCamera = Convert.ToDouble(datas[1]);
                            break;
                        case "nAlignThreshold":
                            nAlignThreshold = Convert.ToInt32(datas[1]);
                            break;

                    }
                }
                sr.Close();
            }
            catch (Exception ex)
            {

            }
        }
        private void LoadReflectanceData()
        {
            if (!File.Exists(ConstValue.PATH_PM_REFLECTANCE_FILE))
            {
                m_DM.m_Log.WriteLog(LogType.PM, "[Error]PM & Monitoring - PMReflectance.csv file is not exist.");
                return;
            }
            StreamReader sr = new StreamReader(ConstValue.PATH_PM_REFLECTANCE_FILE);

            sr.ReadLine();
            while (!sr.EndOfStream)
            {
                string sTemp = sr.ReadLine();
                string[] srDatas = sTemp.Split(',');

                WavelengthRef.Add(Convert.ToDouble(srDatas[0]));
                ReflectanceRef.Add(Convert.ToDouble(srDatas[1]));
            }
            sr.Close();
        }
        public CheckResult CheckSensorTilt(int nCurrentNum)
        {
            if(nCurrentNum==0)
            {
                arrCheckWavelength = sCheckResultWavelength.Split(',');
                for(int n=0; n< arrCheckWavelength.Length; n++)
                {
                    m_CalPMReflectance[n].dWavelength = Convert.ToDouble(arrCheckWavelength[n]);
                }

            }

            m_DM.m_Log.WriteLog(LogType.PM, "CheckSensorTilt()");

            if (m_DM.m_RawData[0].Wavelength.Count() == 0)
            {
                m_DM.m_Log.WriteLog(LogType.PM, "[CheckError]PM & Monitoring - No measurement data.");
                return CheckResult.CheckError;
            }

            PMResult rstPM = new PMResult();
            rstPM.ItemName = PMItem.SensorTilt;
            rstPM.Reference = "±" + dSensorTiltError.ToString() + "%";

            if (Math.Abs(m_DM.m_RawData[0].Wavelength[0] - WavelengthRef[0]) > 0.1)
            {
                
                m_DM.m_Log.WriteLog(LogType.PM, "[CheckError]PM & Monitoring - Cal data and measurement data number do not match");
                return CheckResult.CheckError;
            }
            if(double.IsNaN(m_DM.m_RawData[0].Reflectance[0]) ==true)
            {
                m_DM.m_Log.WriteLog(LogType.PM, "[Error]CheckSensorTilt - MeasureData Error:" );
                rstPM.Error = true;
                return CheckResult.CheckError;
            }
            SensorTiltDatas tiltData = new SensorTiltDatas();
            int nCheckPMRange = nCheckRangeEnd - nCheckRangeStart;
            double dDiffSum = 0.0, dAvg = 0.0;
            int nCheckNum = 0;
            for (int n = 0; n < nCheckPMRange; n++)
            {
                if (WavelengthRef[n] == m_CalPMReflectance[nCheckNum].dWavelength)
                {
                    m_CalPMReflectance[nCheckNum].dDiffReflectance[nCurrentNum] = ReflectanceRef[n] - m_DM.m_RawData[0].Reflectance[n];
                    nCheckNum++;
                }
                
                tiltData.Wavelength.Add(WavelengthRef[n]);
                tiltData.Diff.Add(Math.Abs(ReflectanceRef[n] - m_DM.m_RawData[0].Reflectance[n]));
                dDiffSum += (Math.Abs(ReflectanceRef[n] - m_DM.m_RawData[0].Reflectance[n]));
            }
            if(nCheckNum== arrCheckWavelength.Length)
            {
                m_DM.m_Log.WriteLog(LogType.PM, "All Check Wavelengh Cal done["+nCurrentNum.ToString()+"]");
            }
            else
            {
                m_DM.m_Log.WriteLog(LogType.PM, "All Check Wavelengh Cal done[" + nCurrentNum.ToString() + "]");
            }
            SensorTiltData.Add(tiltData);
            dAvg = dDiffSum / (double)nCheckPMRange;
            rstPM.Measured = dAvg;
            m_CalPMReflectance[nCheckNum].dDiffReflectance[nCurrentNum]= dAvg;
            if (Math.Abs(dAvg) > dSensorTiltError)
            {
                m_DM.m_Log.WriteLog(LogType.PM, "[Error]CheckSensorTilt - RDiffAvg:" + dAvg.ToString() + "/SensorTiltError:" + dSensorTiltError.ToString());
                rstPM.Error = true;
                return CheckResult.Error;
            }
            m_DM.m_Log.WriteLog(LogType.PM, "[OK]CheckSensorTilt - RDiffAvg:" + dAvg.ToString() + "/SensorTiltError:" + dSensorTiltError.ToString());
            rstPM.Error = false;
            if(nCurrentNum == nSensorTiltRepeatNum-1)
            {
                bool bCalDone = CalPMReflectanceResult(arrCheckWavelength.Length+1);
               // m_DM.m_Log.WriteLog(LogType.PM, "Pm Result Data Save Done");
                

            }
            else
            {
               
            }

            return CheckResult.OK;
        }
        private bool CalPMReflectanceResult(int nCalResultNum)
        {
            for (int n = 0; n < nCalResultNum ; n++)
            {

                m_CalPMReflectance[n].dMin = Math.Round( m_CalPMReflectance[n].dDiffReflectance.Min(),3);
                m_CalPMReflectance[n].dMax = Math.Round(m_CalPMReflectance[n].dDiffReflectance.Max(),3);
                if (m_CalPMReflectance[n].dMin > 0 && m_CalPMReflectance[n].dMax > 0)
                {
                    m_CalPMReflectance[n].dCop = Math.Round(-m_CalPMReflectance[n].dMin + m_CalPMReflectance[n].dMax,3);
                }
                else if (m_CalPMReflectance[n].dMin < 0 && m_CalPMReflectance[n].dMax < 0)
                {
                    m_CalPMReflectance[n].dCop = Math.Round(-Math.Abs(m_CalPMReflectance[n].dMin) + Math.Abs(m_CalPMReflectance[n].dMax),3);
                }
                else
                {
                    m_CalPMReflectance[n].dCop = Math.Round(Math.Abs(m_CalPMReflectance[n].dMin) + Math.Abs(m_CalPMReflectance[n].dMax),3);
                }
                m_CalPMReflectance[n].dAvg = Math.Round(m_CalPMReflectance[n].dDiffReflectance.Average(),3);

                m_CalPMReflectance[n].dSTD = Math.Round(CalReflectanceSTD(n, m_CalPMReflectance[n].dAvg),3);
            }
            return true;
        }

        private double CalReflectanceSTD(int nCurrentResultNum, double nDataAvg)
        {
            double[] DataDeviation = new double[nSensorTiltRepeatNum];
            double[] DataDoubleDeviation = new double[nSensorTiltRepeatNum];
            double AvgDoubleDeviation = 0.0;
            double ResultSTD_P = 0.0;

            for(int n=0; n < nSensorTiltRepeatNum; n++)
            {
                DataDeviation[n] = m_CalPMReflectance[nCurrentResultNum].dDiffReflectance[n] - m_CalPMReflectance[nCurrentResultNum].dAvg;

                DataDoubleDeviation[n] = Math.Pow(DataDeviation[n], 2);


            }

            AvgDoubleDeviation = DataDoubleDeviation.Average();
            ResultSTD_P = Math.Sqrt(AvgDoubleDeviation);

            return ResultSTD_P;
        }
        public bool CalcCenterPoint(bool bTop, Emgu.CV.Mat matImg)
        {
            try
            {
                int nThreshold = nAlignThreshold;
                double dDiameter = 0.0;
                //Hole Point
                List<OpenCvSharp.CPlusPlus.Point> ptSelected = new List<OpenCvSharp.CPlusPlus.Point>();
                ptSelected.Add(new OpenCvSharp.CPlusPlus.Point(nSensorCenterX, nSensorCenterY));
                
                //Convert Emgu to OpenCV 
                Bitmap Imagbitmap = matImg.Bitmap;
                OpenCvSharp.CPlusPlus.Mat imgMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(Imagbitmap);
                
                OpenCvSharp.CPlusPlus.Cv2.CvtColor(imgMat, imgMat, OpenCvSharp.ColorConversion.BgrToGray);

                OpenCvSharp.CPlusPlus.Point ptResult = ImageProcess.FindCircleCenter(imgMat, ptSelected, nThreshold, ref dDiameter);

                if (bTop == true)
                {
                    double X = ptResult.X;
                    double Y = ptResult.Y;

                    Align_CenterPosTop.X = X;
                    Align_CenterPosTop.Y = Y;

                    if (ptResult.X == 0 && ptResult.Y == 0)
                    {
                        m_DM.m_Log.WriteLog(LogType.PM, "[Error]CalcCenterPoint<Top>");
                        return false;
                    }

                    m_DM.m_Log.WriteLog(LogType.PM, "[OK]CalcCenterPoint<Top> - X: " + Align_CenterPosTop.X.ToString() + " , Y: " +Align_CenterPosTop.Y.ToString());
                }
                else
                {
                    double X = ptResult.X;
                    double Y = ptResult.Y;

                    Align_CenterPosBot.X = X;
                    Align_CenterPosBot.Y = Y;

                    if (ptResult.X == 0 && ptResult.Y == 0)
                    {
                        m_DM.m_Log.WriteLog(LogType.PM, "[Error]CalcCenterPoint<Bot>");
                        return false;
                    }

                    m_DM.m_Log.WriteLog(LogType.PM, "[OK]CalcCenterPoint<Bot> - X: " +Align_CenterPosTop.X.ToString() + " , Y: " +Align_CenterPosTop.Y.ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.PM, "[Error]CalcCenterPoint - " + ex.Message);
                return false;
            }
        }
        public CheckResult CheckCSSAlign()
        {
            try
            {
                double dX = Math.Sqrt(Math.Pow(Align_CenterPosTop.X - Align_CenterPosBot.X, 2) + Math.Pow(Align_CenterPosTop.Y - Align_CenterPosBot.Y, 2)) * 1.098;
                double dY = dDistSensorCamera  - (double)((nAlignAxisPosZ + dCameraAxisOffsetZ) / 10.0);

                if (dX > 0.0 && dY > 0.0)
                {
                    dAlign_ResultDeg = Math.Atan2(dX, dY) * (180 / Math.PI);

                    m_DM.m_Log.WriteLog(LogType.PM, "[OK]CheckCSSAlign X = " + dX.ToString() + "[um], Y = " + dY.ToString() + "[um] / Align Deg. = " + dAlign_ResultDeg.ToString() + "[Deg.]");
                    return CheckResult.OK;
                }
                else if (dX == 0.0)
                {
                    dAlign_ResultDeg = 0.0;
                    m_DM.m_Log.WriteLog(LogType.PM, "[OK]CheckCSSAlign X = " + dX.ToString() + "[um], Y = " + dY.ToString() + "[um] / Align Deg. = " + dAlign_ResultDeg.ToString() + "[Deg.]");
                    return CheckResult.OK;
                }
                else
                {
                    dAlign_ResultDeg = -9999.0;
                    m_DM.m_Log.WriteLog(LogType.PM, "[Error]CheckCSSAlign X = " + dX.ToString() + "[um], Y = " + dY.ToString() + "[um]");
                    return CheckResult.Error;
                }
            }
            catch (Exception ex)
            {
                dAlign_ResultDeg = -9999.0;
                m_DM.m_Log.WriteLog(LogType.PM, "[Error]CheckCSSAlign - Msg: " + ex.Message);
                return CheckResult.Error;
            }
        }

    }
}
