using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class PMResult
    {
        public PMItem ItemName;
        public double Measured;
        public string Reference;
        public bool Error;
    }

    public class PMDatas
    {
        //Common
        public double CheckRangeStart = 350; //350[nm]
        public double CheckRangeEnd = 1500;  //1500[nm]

        //Sensor Tilt
        public double SensorTiltError = 1;    //1
        public int SensorTiltRepeatNum = 15;

        DataManager m_DM = DataManager.GetInstance();
        public PMDatas()
        {

        }


        //public CheckResult CheckSensorTilt()
        //{
        //    m_DM.m_Log.WriteLog(LogType.PM, "CheckSensorTilt()");

        //    if (m_DM.m_RawData[0].Wavelength.Count() == 0)
        //    {
        //        m_DM.m_Log.WriteLog(LogType.PM, "[CheckError]PM & Monitoring - No measurement data.");
        //        return CheckResult.CheckError;
        //    }

        //    PMResult rstPM = new PMResult();
        //    rstPM.ItemName = PMItem.SensorTilt;
        //    rstPM.Reference = "±" + SensorTiltError.ToString() + "%";

        //    int nStartNum = 0, nEndNum = 0;
        //    bool bFound = false;
        //    for (int n = 0; n < m_DM.m_RawData[0].Wavelength.Count(); n++)
        //    {
        //        if (CheckRangeStart <= m_DM.m_RawData[0].Wavelength[n] && m_DM.m_RawData[0].Wavelength[n] <= CheckRangeEnd)
        //        {
        //            if (bFound == false)
        //            {
        //                nStartNum = n;
        //                bFound = true;
        //            }
        //        }
        //        else if (bFound == true)
        //        {
        //            nEndNum = n - 1;
        //            break;
        //        }
        //    }
        //    int nWLCount = nEndNum - nStartNum + 1;
        //    SensorTiltDataCount = nWLCount;
        //    SensorTiltDataStartN = nStartNum;
        //    SensorTiltDataEndN = nEndNum;

        //    if (Math.Abs(m_DM.m_RawData[0].Wavelength[nStartNum] - WavelengthRef[nStartNum]) > 0.1)
        //    {
        //        //m_DM.m_LM.WriteLog(LOG.PM, "[CheckError]PM & Monitoring - CalData와 측정Data 갯수 불일치");
        //        m_DM.m_Log.WriteLog(LogType.PM, "[CheckError]PM & Monitoring - Cal data and measurement data number do not match");
        //        return CheckResult.CheckError;
        //    }

        //    SensorTiltDatas tiltData = new SensorTiltDatas();

        //    for (int n = nStartNum; n < nEndNum; n++)
        //    {
        //        tiltData.Wavelength.Add(WavelengthRef[n]);
        //        tiltData.Diff.Add(Math.Abs(ReflectanceRef[n] - m_DM.m_RawData[0].Reflectance[n]));
        //    }
        //    m_DM.m_PMData.SensorTiltData.Add(tiltData);

        //    m_DM.m_Log.WriteLog(LogType.PM, "[OK]CheckSensorTilt");
        //    rstPM.Error = false;
        //    return CheckResult.OK;


        //}
    }
}
