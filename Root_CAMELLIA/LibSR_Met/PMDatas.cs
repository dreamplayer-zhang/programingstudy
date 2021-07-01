using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using RootTools;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using static Root_CAMELLIA.Module.Module_Camellia;
using System.IO.Ports;


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
    public enum CheckEdge
    {
        Left = 1,
        Right = 2,
        Up = 3,
        Dawn = 4
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
        public List<OpenCvSharp.CPlusPlus.Point> FittingPoints = new List<OpenCvSharp.CPlusPlus.Point>();
        public List<PMResult> Result;
        public List<SensorTiltDatas> SensorTiltData;
        //Sensor Tilt
        public int nCheckRangeStart; //350[nm] 반사율 반복성 체크 시작 파장
        public int nCheckRangeEnd;  //1500[nm] 반사율 반복성 체크 끝 파장

        public double dSensorTiltError;    // 반사율 반복성 에러 기준
        public int nSensorTiltRepeatNum; // 반사율 반복성 체크 반복 횟수

        public List<double> WavelengthRef; // 기준 반사율 파장 
        public List<double> ReflectanceRef; // 기준 반사율 

        public string sCheckResultWavelength;  //체크 반사율 특정 파장
        public string[] arrCheckWavelength; // 체크 반사율 특정 파장 목록
        public CalPMReflectance[] m_CalPMReflectance; // 반사율 반복성 계산 결과

        //Sensor-Camera Tilt Check
        public System.Windows.Point Align_CenterPosTop = new System.Windows.Point();  //Top 이미지 상에서의 센서 센터
        public System.Windows.Point Align_CenterPosBot = new System.Windows.Point(); //Bottom 이미지 상에서의 센서 센터
        public double dAlign_ResultDeg = 0.0; //Tilt 계산 결과

        public int nAlignAxisPosZ; // Camera Z Ready 위치
        public int nSensorCenterX; // 이미지 상의 Sensor Center X 좌표 값
        public int nSensorCenterY; // 이미지 상의 Sensor Center Y 좌표 값
        public double dCameraAxisOffsetZ; //[pulse] Tilt 계산 시 up Daum 이동 거리
        public double dDistSensorCamera; //[um] 카메라와 센서 사이 거리
        public int nAlignThreshold; // Sensor Grab 이미지 Threshold 값

        //Sensor-Hole Align Check
        public int nInHoleLeftMovePosX; // Hole 왼쪽 엣지 이동 Offset 펄스 값
        public int nInHoleRightMovePosX; // Hole 오른쪽 엣지 이동 Offset 펄스 값
        public int nInHoleUpMovePosY; // Hole 위 엣지 이동 Offset 펄스 값
        public int nInHoleDaumMovePosY; // Hole 아래 엣지 이동 Offset 펄스 값

        public double dFocusHoleEdgeLeftZ; //Hole 엣지에 포커스 맞기 위한 Z축 이동 펄스 값
        public double dFocusHoleEdgeRightZ;
        public double dFocusHoleEdgeUpZ;
        public double dFocusHoleEdgeDawnZ;
        public int nEdgeImageThreshold; // Hole 엣지 Grab 이미지 Threshold 값 
        public int nEdgePointThreshold;

        public int nCalcHoleCenterX = 0;
        public int nCalcHoleCenterY = 0;
        public int nCalcHoleRadius = 0;
        public double dCalcHoleCenterOffsetX = 0;
        public double dCalcHoleCenterOffsetY = 0;

        public double dCalcSensorOffsetX = 0;
        public double dCalcSensorOffsetY = 0;
        public double dCalcTotalOffsetX = 0;
        public double dCalcTotalOffsetY = 0;

        public OpenCvSharp.CPlusPlus.Mat EdgeDrawImg = new OpenCvSharp.CPlusPlus.Mat();

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

            //InitLamp();
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
                        // Align PM
                        case "nInHoleLeftMovePos":
                            nInHoleLeftMovePosX = Convert.ToInt32(datas[1]);
                            break;
                        case "nInHoleRightMovePos":
                            nInHoleRightMovePosX = Convert.ToInt32(datas[1]);
                            break;
                        case "nInHoleUpMovePos":
                            nInHoleUpMovePosY = Convert.ToInt32(datas[1]);
                            break;
                        case "nInHoleDaumMovePos":
                            nInHoleDaumMovePosY = Convert.ToInt32(datas[1]);
                            break;
                        case "dFocusHoleEdgeLeftZ":
                            dFocusHoleEdgeLeftZ = Convert.ToInt32(datas[1]);
                            break;
                        case "dFocusHoleEdgeRightZ":
                            dFocusHoleEdgeRightZ = Convert.ToInt32(datas[1]);
                            break;
                        case "dFocusHoleEdgeUpZ":
                            dFocusHoleEdgeUpZ = Convert.ToInt32(datas[1]);
                            break;
                        case "dFocusHoleEdgeDawnZ":
                            dFocusHoleEdgeDawnZ = Convert.ToInt32(datas[1]);
                            break;
                        case "nEdgeImageThreshold":
                            nEdgeImageThreshold = Convert.ToInt32(datas[1]);
                            break;
                        case "nEdgePointThreshold":
                            nEdgePointThreshold = Convert.ToInt32(datas[1]);
                            break;
                    }
                }
                sr.Close();
            }
            catch (Exception ex)
            {

            }
        }

        #region PM 반사율 반복성 
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
            if (nCurrentNum == 0)
            {
                arrCheckWavelength = sCheckResultWavelength.Split(',');
                for (int n = 0; n < arrCheckWavelength.Length; n++)
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
            if (double.IsNaN(m_DM.m_RawData[0].Reflectance[0]) == true)
            {
                m_DM.m_Log.WriteLog(LogType.PM, "[Error]CheckSensorTilt - MeasureData Error:");
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
            if (nCheckNum == arrCheckWavelength.Length)
            {
                m_DM.m_Log.WriteLog(LogType.PM, "All Check Wavelengh Cal done[" + nCurrentNum.ToString() + "]");
            }
            else
            {
                m_DM.m_Log.WriteLog(LogType.PM, "All Check Wavelengh Cal done[" + nCurrentNum.ToString() + "]");
            }
            SensorTiltData.Add(tiltData);
            dAvg = dDiffSum / (double)nCheckPMRange;
            rstPM.Measured = dAvg;
            m_CalPMReflectance[nCheckNum].dDiffReflectance[nCurrentNum] = dAvg;
            if (Math.Abs(dAvg) > dSensorTiltError)
            {
                m_DM.m_Log.WriteLog(LogType.PM, "[Error]CheckSensorTilt - RDiffAvg:" + dAvg.ToString() + "/SensorTiltError:" + dSensorTiltError.ToString());
                rstPM.Error = true;
                return CheckResult.Error;
            }
            m_DM.m_Log.WriteLog(LogType.PM, "[OK]CheckSensorTilt - RDiffAvg:" + dAvg.ToString() + "/SensorTiltError:" + dSensorTiltError.ToString());
            rstPM.Error = false;
            if (nCurrentNum == nSensorTiltRepeatNum - 1)
            {
                bool bCalDone = CalPMReflectanceResult(arrCheckWavelength.Length + 1);
                // m_DM.m_Log.WriteLog(LogType.PM, "Pm Result Data Save Done");


            }
            else
            {

            }

            return CheckResult.OK;
        }
        private bool CalPMReflectanceResult(int nCalResultNum)
        {
            for (int n = 0; n < nCalResultNum; n++)
            {

                m_CalPMReflectance[n].dMin = Math.Round(m_CalPMReflectance[n].dDiffReflectance.Min(), 3);
                m_CalPMReflectance[n].dMax = Math.Round(m_CalPMReflectance[n].dDiffReflectance.Max(), 3);
                if (m_CalPMReflectance[n].dMin > 0 && m_CalPMReflectance[n].dMax > 0)
                {
                    m_CalPMReflectance[n].dCop = Math.Round(-m_CalPMReflectance[n].dMin + m_CalPMReflectance[n].dMax, 3);
                }
                else if (m_CalPMReflectance[n].dMin < 0 && m_CalPMReflectance[n].dMax < 0)
                {
                    m_CalPMReflectance[n].dCop = Math.Round(-Math.Abs(m_CalPMReflectance[n].dMin) + Math.Abs(m_CalPMReflectance[n].dMax), 3);
                }
                else
                {
                    m_CalPMReflectance[n].dCop = Math.Round(Math.Abs(m_CalPMReflectance[n].dMin) + Math.Abs(m_CalPMReflectance[n].dMax), 3);
                }
                m_CalPMReflectance[n].dAvg = Math.Round(m_CalPMReflectance[n].dDiffReflectance.Average(), 3);

                m_CalPMReflectance[n].dSTD = Math.Round(CalReflectanceSTD(n, m_CalPMReflectance[n].dAvg), 3);
            }
            return true;
        }

        private double CalReflectanceSTD(int nCurrentResultNum, double nDataAvg)
        {
            double[] DataDeviation = new double[nSensorTiltRepeatNum];
            double[] DataDoubleDeviation = new double[nSensorTiltRepeatNum];
            double AvgDoubleDeviation = 0.0;
            double ResultSTD_P = 0.0;

            for (int n = 0; n < nSensorTiltRepeatNum; n++)
            {
                DataDeviation[n] = m_CalPMReflectance[nCurrentResultNum].dDiffReflectance[n] - m_CalPMReflectance[nCurrentResultNum].dAvg;

                DataDoubleDeviation[n] = Math.Pow(DataDeviation[n], 2);


            }

            AvgDoubleDeviation = DataDoubleDeviation.Average();
            ResultSTD_P = Math.Sqrt(AvgDoubleDeviation);

            return ResultSTD_P;
        }
        #endregion

        #region Sensor Tilt Check
        public bool CalcCenterPoint(bool bTop, Emgu.CV.Mat matImg, bool Tilt, double CameraCenterX = 0, double CameraCenterY = 0)
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
                if (Tilt)
                {
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

                        m_DM.m_Log.WriteLog(LogType.PM, "[OK]CalcCenterPoint<Top> - X: " + Align_CenterPosTop.X.ToString() + " , Y: " + Align_CenterPosTop.Y.ToString());
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

                        m_DM.m_Log.WriteLog(LogType.PM, "[OK]CalcCenterPoint<Bot> - X: " + Align_CenterPosTop.X.ToString() + " , Y: " + Align_CenterPosTop.Y.ToString());
                    }
                }
                else
                {
                    double X = ptResult.X; //Image pixel
                    double Y = ptResult.Y; //Image pixel

                    if (ptResult.X == 0 && ptResult.Y == 0)
                    {
                        m_DM.m_Log.WriteLog(LogType.PM, "[Error]CalcCenterPoint<Sensor>");
                        return false;
                    }
                    double ImageX = matImg.Width / 2; //Image pixel
                    double ImageY = matImg.Height / 2; //Image pixel
                    double Realum = ConstValue.PM_CAMERA_PIXEL_RESOLUTION; //Image pixel -> um
                    double RealPulse = ConstValue.PM_STAGE_PULSE; //um -> pulse

                    dCalcSensorOffsetX = Math.Round((X - ImageX) * Realum * RealPulse); //pulse
                    dCalcSensorOffsetY = Math.Round((ImageY - Y) * Realum * RealPulse); //pulse
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
                double dY = dDistSensorCamera - (double)((nAlignAxisPosZ + dCameraAxisOffsetZ) / 10.0);

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
        #endregion


        #region Sensor Hole Align
        // 1.각 이미지 받아와서 엣지 검출하는 것
        public int neee = 0;
        public bool FindEdgePointTest(OpenCvSharp.CPlusPlus.Mat sourceImage, RPoint HoleStagePos, int CurrentAxis)
        {
            try
            {
                //Emgu Mat -> Bitmap -> opencv Mat
                //Bitmap Imagbitmap = HoleEdgeImage.Bitmap;
                //OpenCvSharp.CPlusPlus.Mat sourceImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(Imagbitmap);



                Mat matImg = new Mat();
                sourceImage.CopyTo(matImg);

                neee++;
                List<OpenCvSharp.CPlusPlus.Point2d> ptEdge = GetHoleEdgePoints(matImg, CurrentAxis);

                double nCurrPosX = HoleStagePos.X;
                double nCurrPosY = HoleStagePos.Y;
                Point2d ptCurrPulse = new Point2d((double)nCurrPosX, (double)nCurrPosY);

                GetPulseHoleEdgePoints(ptEdge, ptCurrPulse, matImg.Width, matImg.Height);

                return true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("[Error]FindEdgePoints - " + ex.Message);
                return false;
            }

        }
        public bool FindEdgePoint(Emgu.CV.Mat HoleEdgeImage, RPoint HoleStagePos, int CurrentAxis)
        {
            try
            {
                //Emgu Mat -> Bitmap -> opencv Mat
                Bitmap Imagbitmap = HoleEdgeImage.Bitmap;
                OpenCvSharp.CPlusPlus.Mat sourceImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(Imagbitmap);

                Mat matImg = new Mat();
                sourceImage.CopyTo(matImg);

                neee++;
                List<OpenCvSharp.CPlusPlus.Point2d> ptEdge = GetHoleEdgePoints(matImg, CurrentAxis);

                double nCurrPosX = HoleStagePos.X;
                double nCurrPosY = HoleStagePos.Y;
                Point2d ptCurrPulse = new Point2d((double)nCurrPosX, (double)nCurrPosY);

                GetPulseHoleEdgePoints(ptEdge, ptCurrPulse, matImg.Width, matImg.Height);

                return true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("[Error]FindEdgePoints - " + ex.Message);
                return false;
            }

        }
        public void GetPulseHoleEdgePoints(List<Point2d> ptEdges, Point2d ptCurrPulse, int w, int h)
        {
            // Pulse Offset 구하기
            float dImgRes = 1.1f;
            float dPulse = 10;
            Point2d ptImageCenter = new Point2d(w / 2, h / 2);
            Point2d ptPulseOffset = new Point2d(ptCurrPulse.X - (ptImageCenter.X * dImgRes * dPulse), ptCurrPulse.Y - (ptImageCenter.Y * dImgRes * dPulse));

            List<OpenCvSharp.CPlusPlus.Point> ptTransformNPulseEdge = new List<OpenCvSharp.CPlusPlus.Point>();

            for (int n = nEdgePointThreshold; n < ptEdges.Count - nEdgePointThreshold; n++)
            {
                ptTransformNPulseEdge.Add(new OpenCvSharp.CPlusPlus.Point((ptEdges[n].X) * dImgRes * dPulse + ptPulseOffset.X, ptEdges[n].Y * dImgRes * dPulse + ptPulseOffset.Y));        // *11 함
            }

            FittingPoints.AddRange(ptTransformNPulseEdge);
        }
        public List<Point2d> GetHoleEdgePoints(Mat Image, int CurrentAxis)
        {
            try
            {
                Mat matImg = new Mat();
                Image.CopyTo(matImg);
                int nThreshold = nEdgeImageThreshold;
                Mat matThresholdImg;
                matThresholdImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC1);


                Mat matImg2 = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC1);

                // 1. Gaussian 처리
                Mat matGaussianImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC1);
                Cv2.GaussianBlur(matImg, matGaussianImg, new OpenCvSharp.CPlusPlus.Size(3, 3), 1, 1);
                Cv2.CvtColor(matGaussianImg, matGaussianImg, ColorConversion.BgrToGray);
                Cv2.ImWrite(@"D:\Temp\Input_" + neee.ToString() + ".jpg", matImg2);
                //Cv2.ImWrite(@"D:\Temp\Gaussian_" + neee.ToString() + ".jpg", matGaussianImg);

                Cv2.Threshold(matGaussianImg, matGaussianImg, nThreshold, 255, ThresholdType.Binary);
                Cv2.ImWrite(@"D:\Temp\Threshold_" + neee.ToString() + ".jpg", matGaussianImg);


                // 2. 각각 블록 합 구하기
                int max, sMin, idx, sIdx;
                idx = sIdx = 0;
                max = -99999999;
                sMin = 99999999;

                int[] eachBlockGV = new int[9];
                int[] sumEachBlockGV = new int[6];

                int nOutlier = 0;
                int nStartIdx = 0;
                int nMidIdx = 998;
                int nEndIdx = 1997;
                OpenCvSharp.CPlusPlus.Point[] initPt = new OpenCvSharp.CPlusPlus.Point[] {
                new OpenCvSharp.CPlusPlus.Point { X = nStartIdx, Y = nStartIdx }, new OpenCvSharp.CPlusPlus.Point { X = nMidIdx, Y = nStartIdx }, new OpenCvSharp.CPlusPlus.Point { X = nEndIdx - nOutlier, Y = nStartIdx },
                new OpenCvSharp.CPlusPlus.Point { X = nStartIdx, Y = nMidIdx }, new OpenCvSharp.CPlusPlus.Point { X = nMidIdx, Y = nMidIdx }, new OpenCvSharp.CPlusPlus.Point { X = nEndIdx - nOutlier, Y = nMidIdx },
                new OpenCvSharp.CPlusPlus.Point { X = nStartIdx, Y = nEndIdx }, new OpenCvSharp.CPlusPlus.Point { X = nMidIdx, Y = nEndIdx }, new OpenCvSharp.CPlusPlus.Point { X = nEndIdx - nOutlier, Y = nEndIdx } };

                for (int i = 0; i < 9; i++)
                    eachBlockGV[i] = SumNineBlockPixels(initPt[i], matGaussianImg);

                sumEachBlockGV[0] = eachBlockGV[0] + eachBlockGV[1] + eachBlockGV[2];
                sumEachBlockGV[1] = eachBlockGV[3] + eachBlockGV[4] + eachBlockGV[5];
                sumEachBlockGV[2] = eachBlockGV[6] + eachBlockGV[7] + eachBlockGV[8];
                sumEachBlockGV[3] = eachBlockGV[0] + eachBlockGV[3] + eachBlockGV[6];
                sumEachBlockGV[4] = eachBlockGV[1] + eachBlockGV[4] + eachBlockGV[7];
                sumEachBlockGV[5] = eachBlockGV[2] + eachBlockGV[5] + eachBlockGV[8];


                // 3. 각 블록 중 최대값 구하기 ( 최대값 - max / 최대 인덱스 - idx )
                for (int n = 0; n < 9; n++)
                {
                    if (eachBlockGV[n] > max)
                    {
                        max = eachBlockGV[n];
                        idx = n;
                    }
                }
                //오류 가능성 

                // 4. 시작 축 정하기 ( 최소값 - sMin / 최소 인덱스 - sIdx )
                for (int n = 0; n < sumEachBlockGV.Length; n++)
                {
                    if (sumEachBlockGV[n] < sMin)
                    {
                        // sIdx == 0,1,2 => 가로축
                        // sIdx == 3,4,5 => 세로축
                        sMin = sumEachBlockGV[n];
                        sIdx = n;
                    }
                }


                // 5. 이미지를 다 우하향 방향으로 나아가도록 각도 수정
                int[] startIdx = new int[] { 0, 3, 6, 0, 1, 2 };
                List<List<int>> nnSeries = new List<List<int>>();

                int degree = 0;
                Mat rotate = new Mat();
                Point2f ptCenter = new Point2f(matGaussianImg.Width / 2, matGaussianImg.Height / 2);
                //Point2f ptCenter = new Point2f(880, 970);

                if (CurrentAxis == (int)CheckEdge.Up)       // 위로 이동
                {
                    degree = 90;
                    rotate = Cv2.GetRotationMatrix2D(ptCenter, degree, 1);
                }
                else if (CurrentAxis == (int)CheckEdge.Dawn)                                                                    // 아래로 이동
                {
                    degree = -90;
                    rotate = Cv2.GetRotationMatrix2D(ptCenter, degree, 1);
                }

                else if (CurrentAxis == (int)CheckEdge.Right)       // 오른쪽으로 이동
                {
                    degree = 0;
                    rotate = Cv2.GetRotationMatrix2D(ptCenter, degree, 1);
                }
                else if (CurrentAxis == (int)CheckEdge.Left)                                                                 // 왼쪽으로 이동
                {
                    degree = 180;
                    rotate = Cv2.GetRotationMatrix2D(ptCenter, degree, 1);
                }


                // 6. 구한 각도로 이미지 회전
                Cv2.WarpAffine(matGaussianImg, matGaussianImg, rotate, matGaussianImg.Size());
                //Cv2.ImShow("Input", matGaussianImg);
                //Cv2.ImWrite(@"D:\Temp\CalImage" + neee.ToString() + degree.ToString() + ".jpg", matGaussianImg);
                // 7. 이미지 차분 값을 nnSeries에 담음 (  i x j 행렬 => nnSeries[i] : y좌표, nnSeries[i][j] : x좌표 )
                unsafe
                {
                    byte* data = (byte*)matGaussianImg.DataPointer;

                    for (int y = 0; y < matGaussianImg.Height; y++)
                    {
                        List<int> nSeries = new List<int>();
                        for (int x = nOutlier; x < matGaussianImg.Width - 1 - nOutlier; x++)
                        {
                            nSeries.Add(Math.Abs(data[y * matGaussianImg.Width + (x + 1)] - data[y * matGaussianImg.Width + x]));
                        }
                        nnSeries.Add(nSeries);
                    }
                }


                // 8. 차분 최대값 index 찾기 ( i x 1 벡터 => nDiffSeries[i] ) 
                List<int> nDiffSeries = new List<int>();    // 각 Col 당 최대 GV값 가지는 Index
                for (int n = 0; n < nnSeries.Count; n++)
                {
                    int nDiffMax = nnSeries[n].Max();

                    if (nDiffMax < 10)
                    {
                        nDiffSeries.Add(-777);
                    }
                    else
                    {
                        //y축 라인에 빛이 존재하면 그 y축 데이터(indexof)가 뭔지 모르겠음
                        //
                        int nDiffMaxIdx = nnSeries[n].IndexOf(nDiffMax);
                        nDiffSeries.Add(nDiffMaxIdx + nOutlier);
                    }
                }


                // 9. 차분 최대값  좌표변환해서 원본 이미지로 되돌리기
                List<Point2d> ptEdges = new List<Point2d>();
                for (int i = 0; i < nDiffSeries.Count; i++)
                {
                    if (nDiffSeries[i] != -777)
                    {
                        Point2d ptEdgee = RotatePoints(new Point2d((double)ptCenter.X, (double)ptCenter.Y), new Point2d(nDiffSeries[i], i), degree);
                        //Point2d ptEdgee = new Point2d(nDiffSeries[i], i);
                        ptEdges.Add(ptEdgee);
                    }
                }

                // 위치에 엑스표 칠한은 코드
                EdgeDrawImg = DrawEdgePoints(matImg, matGaussianImg, ptEdges);


                return ptEdges;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                return null;
            }
        }
        public int SumNineBlockPixels(OpenCvSharp.CPlusPlus.Point initPt, Mat matImg)
        {
            int sum = 0;
            unsafe
            {
                // 8 connectivity - initPt 중심으로 rightbottom 방향으로 픽셀 값 구하고 더할거임
                byte* data = (byte*)matImg.DataPointer;
                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        sum += data[(initPt.Y + y) * matImg.Width + (initPt.X + x)];
                    }
                }
            }

            return sum;
        }
        public Point2d RotatePoints(Point2d ptAxis, Point2d ptNew, int nAngle)
        {
            double radian = nAngle * Math.PI / 180;
            double dx = ptNew.X - ptAxis.X;
            double dy = ptNew.Y - ptAxis.Y;
            double cosR = Math.Cos(radian);
            double sinR = Math.Sin(radian);
            ptNew.X = (int)(ptAxis.X + (dx * cosR - dy * sinR));
            ptNew.Y = (int)(ptAxis.Y + (dx * sinR + dy * cosR));

            return ptNew;
        }

        public OpenCvSharp.CPlusPlus.Mat DrawEdgePoints(Mat matImg, Mat matGaussianImg, List<Point2d> ptEdges)
        {
            //쨋든 ptEdges 밝은 지점 경계의 좌표값
            Mat matPtEdgeImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC3);      // display
            matPtEdgeImg = matImg.Clone();
            //Cv2.ImWrite(@"D:\Temp\EdgeBF_" + neee.ToString() + ".jpg", matPtEdgeImg);
            int nWindowWidth = (matImg.Width) / 2;
            int nWindowHeight = (matImg.Height) / 2;

            // Monitoring
            for (int i = nEdgePointThreshold; i < ptEdges.Count - nEdgePointThreshold; i++)
            {
                Cv2.Line(matPtEdgeImg, new OpenCvSharp.CPlusPlus.Point(ptEdges[i].X - 10, ptEdges[i].Y - 10), new OpenCvSharp.CPlusPlus.Point(ptEdges[i].X + 10, ptEdges[i].Y + 10), Scalar.Red, 1);
                Cv2.Line(matPtEdgeImg, new OpenCvSharp.CPlusPlus.Point(ptEdges[i].X - 10, ptEdges[i].Y + 10), new OpenCvSharp.CPlusPlus.Point(ptEdges[i].X + 10, ptEdges[i].Y - 10), Scalar.Red, 1);
            }

            Cv2.Line(matPtEdgeImg, new OpenCvSharp.CPlusPlus.Point(nWindowWidth - 10, nWindowHeight - 10), new OpenCvSharp.CPlusPlus.Point(nWindowWidth + 10, nWindowHeight + 10), Scalar.Yellow, 10);
            Cv2.Line(matPtEdgeImg, new OpenCvSharp.CPlusPlus.Point(nWindowWidth - 10, nWindowHeight + 10), new OpenCvSharp.CPlusPlus.Point(nWindowWidth + 10, nWindowHeight - 10), Scalar.Yellow, 10);

            Cv2.ImWrite(@"D:\Temp\EdgeAF_" + neee.ToString() + ".jpg", matPtEdgeImg);

            return matPtEdgeImg;
        }
        // 2. 측정한 엣지로부터 Hole Center 구하는 알고리즘
        public CheckResult CheckHoleCenterOffset(double nOriHoleCenterX, double nOriHoleCenterY)
        {
            try
            {
                double nOriX = nOriHoleCenterX;
                double nOriY = nOriHoleCenterY;

                Fitting Circle = new Fitting();
                List<double> xyr = new List<double>();

                List<double> ptInit = new List<double>(new double[] { FittingPoints[0].X + 1, FittingPoints[0].Y + 1, 10000 });
                List<List<double>> ppInput = new List<List<double>>();

                for (int n = 0; n < FittingPoints.Count; n++)
                {
                    List<double> pInput = new List<double>();
                    pInput.Add(FittingPoints[n].X);
                    pInput.Add(FittingPoints[n].Y);
                    ppInput.Add(pInput);
                }
                xyr = Circle.LineFitting(100, 3, ptInit, ppInput, "LMA", "Circle", true);

                nCalcHoleCenterX = Convert.ToInt32(xyr[0]);
                nCalcHoleCenterY = Convert.ToInt32(xyr[1]);
                nCalcHoleRadius = Convert.ToInt32(xyr[2]);

                dCalcHoleCenterOffsetX = nCalcHoleCenterX - nOriX;
                dCalcHoleCenterOffsetY = nCalcHoleCenterY - nOriY;

                MessageBox.Show("[OK]CheckCameraSensorOffset - Hole Center X: " + nCalcHoleCenterX.ToString() + " / Hole Center Y: " + nCalcHoleCenterY.ToString() + " / Hole Radius: " + nCalcHoleRadius.ToString() + " / Hole Center Offset X: " + dCalcHoleCenterOffsetX.ToString() + " / Hole Center Offset Y: " + dCalcHoleCenterOffsetY.ToString());

                return CheckResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("[Error]CheckCameraSensorOffset - Msg: " + ex.Message);
                return CheckResult.Error;
            }
        }


        // 3. Hole 센터 이미지로부터 Offset 값 구하는 것

        // 4. 레디 위치에서 Sensor Center 구해서 Offset 구하는 것

        // 5. 레디 위치에서 찍은 이미지에 Sensor center, camera center, Hole center 좌표 올라오도록 하기

        public CheckResult Sensor_Camera_HoleOffset()
        {
            try
            {
                dCalcTotalOffsetX = dCalcHoleCenterOffsetX + dCalcSensorOffsetX;
                dCalcTotalOffsetY = dCalcHoleCenterOffsetY + dCalcSensorOffsetY;

                return CheckResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("[Error]CheckCameraSensorOffset - Msg: " + ex.Message);
                return CheckResult.Error;
            }
        }

        // 이미지에 해당 좌표 point 그리기
        public OpenCvSharp.CPlusPlus.Mat CenterPointCheck(OpenCvSharp.CPlusPlus.Mat ReadyImage)
        {
            //Bitmap Imagbitmap = ReadyImage.Bitmap;
            //OpenCvSharp.CPlusPlus.Mat imgMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(Imagbitmap);
            //Emgu.CV.Mat Result = new Emgu.CV.Mat();
            double HolCenterPointX, HoleCenterPointY, SensorCenterPointX, SensorCenterPointY;
            double Pixelum = ConstValue.PM_CAMERA_PIXEL_RESOLUTION;
            double PulseResolution = ConstValue.PM_STAGE_PULSE;

            //Hole Center Check




            //Sensor Center Check



            OpenCvSharp.CPlusPlus.Mat MatCenterImg = new Mat(ReadyImage.Rows, ReadyImage.Cols, MatType.CV_8UC3);
            MatCenterImg = ReadyImage.Clone();

            int nWindowWidth = (ReadyImage.Width) / 2;
            int nWindowHeight = (ReadyImage.Height) / 2;
            SensorCenterPointX = nWindowWidth + Math.Round((dCalcSensorOffsetX / Pixelum) / PulseResolution);
            SensorCenterPointY = nWindowHeight + Math.Round((-dCalcSensorOffsetY / Pixelum) / PulseResolution);
            HolCenterPointX = nWindowWidth - Math.Round((dCalcHoleCenterOffsetX / Pixelum) / PulseResolution);
            HoleCenterPointY = nWindowHeight - Math.Round((-dCalcHoleCenterOffsetY / Pixelum) / PulseResolution);

            Cv2.Line(MatCenterImg, new OpenCvSharp.CPlusPlus.Point(HolCenterPointX - 20, HoleCenterPointY - 20), new OpenCvSharp.CPlusPlus.Point(HolCenterPointX + 20, HoleCenterPointY + 20), Scalar.Red, 20);
            Cv2.Line(MatCenterImg, new OpenCvSharp.CPlusPlus.Point(HolCenterPointX - 20, HoleCenterPointY + 20), new OpenCvSharp.CPlusPlus.Point(HolCenterPointX + 20, HoleCenterPointY - 20), Scalar.Red, 20);

            Cv2.Line(MatCenterImg, new OpenCvSharp.CPlusPlus.Point(SensorCenterPointX - 20, SensorCenterPointY - 20), new OpenCvSharp.CPlusPlus.Point(SensorCenterPointX + 20, SensorCenterPointY + 20), Scalar.Blue, 20);
            Cv2.Line(MatCenterImg, new OpenCvSharp.CPlusPlus.Point(SensorCenterPointX - 20, SensorCenterPointY + 20), new OpenCvSharp.CPlusPlus.Point(SensorCenterPointX + 20, SensorCenterPointY - 20), Scalar.Blue, 20);


            Cv2.Line(MatCenterImg, new OpenCvSharp.CPlusPlus.Point(nWindowWidth - 20, nWindowHeight - 20), new OpenCvSharp.CPlusPlus.Point(nWindowWidth + 20, nWindowHeight + 20), Scalar.Yellow, 20);
            Cv2.Line(MatCenterImg, new OpenCvSharp.CPlusPlus.Point(nWindowWidth - 20, nWindowHeight + 20), new OpenCvSharp.CPlusPlus.Point(nWindowWidth + 20, nWindowHeight - 20), Scalar.Yellow, 20);
            Cv2.ImWrite(@"D:\Temp\EdgeAF_" + ".jpg", MatCenterImg);
            return MatCenterImg;

        }
    }
}
        //public Emgu.CV.Mat CenterPointCheck (Emgu.CV.Mat ReadyImage)
        //{
        //    Emgu.CV.Mat Result = new Emgu.CV.Mat();

        //    return Result;

        //}

        // 나중에 추가 필요한 것 

        // 이미지 내에 센서 위치 좌표 자동 찾기

        // 이미지 내에 센서 




        #endregion

  
