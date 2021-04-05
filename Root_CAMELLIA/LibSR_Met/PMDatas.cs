using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Drawing;
using System.Windows.Forms;

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

        DataManager m_DM = DataManager.GetInstance();
        public PMDatas()
        {
            Result = new List<PMResult>();
            SensorTiltData = new List<SensorTiltDatas>();

            WavelengthRef = new List<double>();
            ReflectanceRef = new List<double>();
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
                    string[] dataTemp;// = datas[1].Split(',');

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
        public CheckResult CheckSensorTilt()
        {
            m_DM.m_Log.WriteLog(LogType.PM, "CheckSensorTilt()");

            if (m_DM.m_RawData[0].Wavelength.Count() == 0)
            {
                m_DM.m_Log.WriteLog(LogType.PM, "[CheckError]PM & Monitoring - No measurement data.");
                return CheckResult.CheckError;
            }

            PMResult rstPM = new PMResult();
            rstPM.ItemName = PMItem.SensorTilt;
            rstPM.Reference = "±" + dSensorTiltError.ToString() + "%";


            //SensorTiltDataStartN = 350;
            //SensorTiltDataEndN = 1500;
            //int nWLCount = SensorTiltDataEndN - ensorTiltDataStartN + 1;
            //SensorTiltDataCount = nWLCount;

            if (Math.Abs(m_DM.m_RawData[0].Wavelength[0] - WavelengthRef[0]) > 0.1)
            {
                // 둘다 350 부터 시작 해야함
                //m_DM.m_LM.WriteLog(LOG.PM, "[CheckError]PM & Monitoring - CalData와 측정Data 갯수 불일치");
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
            for (int n = 0; n < nCheckPMRange; n++)
            {
                tiltData.Wavelength.Add(WavelengthRef[n]);
                tiltData.Diff.Add(Math.Abs(ReflectanceRef[n] - m_DM.m_RawData[0].Reflectance[n]));
                dDiffSum += (Math.Abs(ReflectanceRef[n] - m_DM.m_RawData[0].Reflectance[n]));
            }
            SensorTiltData.Add(tiltData);
            dAvg = dDiffSum / (double)nCheckPMRange;
            rstPM.Measured = dAvg;

            if (Math.Abs(dAvg) > dSensorTiltError)
            {
                m_DM.m_Log.WriteLog(LogType.PM, "[Error]CheckSensorTilt - RDiffAvg:" + dAvg.ToString() + "/SensorTiltError:" + dSensorTiltError.ToString());
                rstPM.Error = true;
                return CheckResult.Error;
            }
            m_DM.m_Log.WriteLog(LogType.PM, "[OK]CheckSensorTilt - RDiffAvg:" + dAvg.ToString() + "/SensorTiltError:" + dSensorTiltError.ToString());
            rstPM.Error = false;
            return CheckResult.OK;
        }
    }
}
