using NanoView;
using Root_CAMELLIA.Data;
using RootTools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Root_CAMELLIA.LibSR_Met
{
    public enum FitValueType
    {
        Reflectance,
        Transmittance
    }

    public class RawData
    {
        public bool bDataExist = false;
        public int nCalcDataNum = 0;
        public int nNIRDataNum = 0;
        public double dX = 0.0;
        public double dY = 0.0;
        public double dGoF = 0.0;
        public List<double> Thickness;
        public double[] eV;
        public double[] Wavelength;
        public double[] Reflectance;
        public double[] VIS_Reflectance;
        public double[] VIS_Wavelength;
        public double[] Transmittance;
        public double[] CalcReflectance;
        public List<DCOLTransmittanceData> DCOLTransmittance;
       

        public RawData()
        {
            Thickness = new List<double>();
            eV = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
            Wavelength = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
            Reflectance = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
            VIS_Reflectance = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
            VIS_Wavelength = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
            Transmittance = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
            CalcReflectance = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
            DCOLTransmittance = new List<DCOLTransmittanceData>();
            
        }
    }
    public class DCOLTransmittanceData
    {
        public double Wavelength;
        public double RawTransmittance;
    }

    public class LayerData
    {
        public LinearScale linearScale;
        public char[] hostname;
        public char[] hostpath;
        public char[] guest1name;
        public char[] guest1path;
        public char[] guest2name;
        public char[] guest2path;
        public double dFv1;
        public double dFv2;
        public double dThickness;
        public bool bFv1fit;
        public bool bFv2fit;
        public bool bThicknessFit;
        public int nEmm;
        public double dThicknessScale;
        public double dThickenssOffset;

        public LayerData()
        {
            hostname = null;
            hostpath = null;
            guest1name = null;
            guest1path = null;
            guest2name = null;
            guest2path = null;
        }

        //Get Transmittance 
        public List<double> n = new List<double>();
        public List<double> k = new List<double>();
        public List<double> wavelength = new List<double>();

        //추후 제거 20.12.23 Met.DS 추가 calcassistant
        public string sRefName;   //물질 이름   //레시피 저장 필요
        public bool bFix;    //두꼐 고정 유무
        public double dInitThickness = 0.0;   //레시피 저장 필요
        public double dTHKRangeRate = 100;    //두께 범위 (%)-> CONSTVALUE 값으로 지정할 것
        public double dTargetThickness;   //투과율계산용 타겟 두께
        public bool bUseTargetTHK;  //투과율 계산시 타겟 두께를 쓸것인가 여부
        
        //
    }

    public class PM_SR_Parameter
    {

    }

    public class SettingData
    {
        public int nBGIntTime_VIS = 0;
        public int nAverage_VIS = 0;
        public int nBGIntTime_NIR = 0;
        public int nAverage_NIR = 0;
        public int nBoxcar_NIR = 0;
        public int nBoxcar_VIS = 0;
        public double dAlphaFit = 1.0;
        public int nInitCalIntTime_VIS = 0;
        public int nInitCalIntTime_NIR = 0;
        public int nMeasureIntTime_VIS = 0;
        public int nMeasureIntTime_NIR = 0;

        public SettingData()
        { 
        }
    }

    ///<summary>
    ///Thickness에 들어가는 선형 스케일 (Ax + B)
    ///</summary>
    public class LinearScale  //190627
    {
        public double dWaveLength { get; private set; } = 350;
        public double dScale { get; private set; } = 1.0; //Ax + B의 A  
        public double dOffset { get; private set; } = 0.0;  //Ax + B의 B

        public LinearScale(double waveLength, double scale, double offset)
        {
            dWaveLength = waveLength;
            dScale = scale;
            dOffset = offset;
        }
    }

    public class HoleData : ICloneable
    {
        public double XPos;
        public double YPos;
        public double Value;

        public HoleData()
        {
            XPos = 0.0;
            YPos = 0.0;
            Value = 0.0;
        }


        public object Clone()
        {
            HoleData data = new HoleData();
            data.XPos = XPos;
            data.YPos = YPos;
            data.Value = Value;

            return data;
        }
    }

    public class ContourMapData
    {
        public double Wavelength;
        public List<HoleData> HoleData = new List<HoleData>();
    }

    public class DataManager
    {
        private static DataManager instance = null;
        public LogManager m_Log;
        public RawData[] m_RawData;    //point 갯수만큼
        public List<ContourMapData> m_ContourMapDataR;
        public List<ContourMapData> m_ContourMapDataT;
        public List<WavelengthItem> m_ScalesListR;
        public List<WavelengthItem> m_ScalesListT;
        public List<LayerData> m_LayerData;
        public List<ThicknessScaleOffset> m_ThicknessData;
        public List<ThicknessScaleOffset> m_ThicknessDataSave;
        public bool bExcept_NIR = false;
        public bool bThickness = true;
        public bool bTransmittance = true;
        public bool bViewCalRGraph = true;
        public bool bCalDCOLTransmittance = false;
        public int nThicknessDataNum = 0;
        public float nStartWavelegth = 0;
        public double[] BackGroundCalWavelength;
        public double [] BackGroundCalCountData;
        public SettingData m_SettngData;
        public int nRepeatCount = 1;
        public int nPointCount = 0;
        public string[] m_LotDataPath;


        //추후 제거 DS 2021.01.05 추가
        // n,k Dll 을 받지 못하는 이상 guswo  바꾼 Data를 받아서 계산 할 수 밖에 없음

        public string m_sNKPath;
        public double m_dWavelengthStart;
        public double m_dWavelengthEnd;
        public double m_dEigenValue;
        public int m_nIteration;
        public bool m_bInitalGuess;
        
        public double[] m_CalcReflectance;
        public bool m_bCalcThickness = true;      //두께 계산 여부
        public bool m_bCalcTransmittance = true;  //투과율 계산 여부
        public List<double> m_Wavelength;//
        public List<double> m_Reflectance;
        //

        public static DataManager GetInstance()
        {
            if (instance == null)
            {
                instance = new DataManager();
            }
            return instance;
        }

        public DataManager()//Save 용 & 측정 파일 데이터 관리
        {
            m_Log = new LogManager();
            m_RawData = new RawData[ConstValue.RAWDATA_POINT_MAX_SIZE];
            m_ContourMapDataR = new List<ContourMapData>();
            m_ContourMapDataT = new List<ContourMapData>();
            m_ScalesListR = new List<WavelengthItem>(); //190627
            m_ScalesListT = new List<WavelengthItem>();
            m_LayerData = new List<LayerData>();
            m_ThicknessData = new List<ThicknessScaleOffset>();
            m_ThicknessDataSave = new List<ThicknessScaleOffset>();
            m_SettngData = new SettingData();
            m_LotDataPath = new string[30];

            for (int i = 0; i < ConstValue.RAWDATA_POINT_MAX_SIZE; i++)
            {
                m_RawData[i] = new RawData();
            }

            //추후 제거 DS 2021.01.05 추가
            m_sNKPath = string.Empty;
            m_bInitalGuess = false;
            m_LayerData = new List<LayerData>();
            m_Wavelength = new List<double>();
            m_Reflectance = new List<double>();
            //
        }


        public bool ClearRawData()
        {
            try
            {
                StopWatch sw = new StopWatch();

                sw.Start();
                for (int n = 0; n < ConstValue.RAWDATA_POINT_MAX_SIZE; n++)
                {
                    m_RawData[n].bDataExist = false;
                    m_RawData[n].Wavelength = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    m_RawData[n].Reflectance = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    m_RawData[n].VIS_Reflectance = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    m_RawData[n].VIS_Wavelength = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    m_RawData[n].Transmittance = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    m_RawData[n].CalcReflectance = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    m_RawData[n].eV = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    m_RawData[n].DCOLTransmittance = new List<DCOLTransmittanceData>();
                    m_RawData[n].Thickness.Clear();
                    m_RawData[n].dX = 0.0;
                    m_RawData[n].dY = 0.0;
                    m_RawData[n].dGoF = 0.0;
                    m_RawData[n].nCalcDataNum = 0;
                    m_RawData[n].nNIRDataNum = 0;
                }
                sw.Stop();

                System.Diagnostics.Debug.WriteLine("Clear Data " + sw.ElapsedMilliseconds);
                return true;
            }
            catch(Exception ex)
            {
                m_Log.WriteLog(LogType.Error, "Failed to clear RawData.");
                return false;
            }
        }

        public bool SaveRawData(string sPath, int nPointIndex)
        {
            try
            {
                if(!m_RawData[nPointIndex].bDataExist)
                {
                    throw new Exception("Point: "+ nPointIndex .ToString()+ " Data is not exist.");
                }
                string [] sFilePath = sPath.Split('\\');
                string sFolderPath = "";
                for (int i=0; i< sFilePath.Length-1; i++)
                {
                    sFolderPath += sFilePath[i]+"\\";
                }

                sFolderPath += nPointIndex.ToString() + "\\";


                if (!Directory.Exists(sFolderPath))
                {
                    Directory.CreateDirectory(sFolderPath);
                }

                string strPath = sFolderPath;
                if (!Directory.Exists(strPath))
                {
                    Directory.CreateDirectory(strPath);
                }

                sFolderPath += sFilePath[sFilePath.Length-1];
                sPath = sFolderPath;
                if (Path.GetExtension(sPath) != ".csv")
                    sPath += ".csv";
                StreamWriter sw = new StreamWriter (sPath);
                RawData data = m_RawData[nPointIndex];
                sw.WriteLine("Wavelength[nm],Reflectance[%],Transmittance[%]");
                for (int n = 0; n < data.nNIRDataNum; n++)
                {
                    sw.WriteLine("{0},{1},{2}", data.Wavelength[n], data.Reflectance[n], data.Transmittance[n]);
                }
                sw.Close();

                m_Log.WriteLog(LogType.Datas, "Point: " + nPointIndex.ToString() + " Raw data saved.");
                return true;
            }
            catch (Exception ex)
            {
                m_Log.WriteLog(LogType.Error, "Point: " + nPointIndex.ToString() + " Failed to save raw data.");
                return false;
            }
        }
        public bool SaveCheckSensorData(string sPath, int nPointIndex)
        {
            try
            {
                if (!m_RawData[nPointIndex].bDataExist)
                {
                    throw new Exception("Point: " + nPointIndex.ToString() + " Data is not exist.");
                }

                string[] sFilePath = sPath.Split('\\');
                string sFolderPath = "";
                for (int i = 0; i < sFilePath.Length - 1; i++)
                {
                    sFolderPath += sFilePath[i] + "\\";
                }

                if (!Directory.Exists(sFolderPath))
                {
                    Directory.CreateDirectory(sFolderPath);
                }

                string strPath = sFolderPath;
                if (!Directory.Exists(strPath))
                {
                    Directory.CreateDirectory(strPath);
                }


                if (Path.GetExtension(sPath) != ".csv")
                {
                    sPath += ".csv";
                }
                StreamWriter sw = new StreamWriter (sPath);
                RawData data = m_RawData[nPointIndex];

                sw.WriteLine("Wavelength[nm],Reflectance[%]");
                for (int n = 0; n < m_RawData[nPointIndex].nNIRDataNum; n++)
                {
                    sw.WriteLine("{0},{1}", data.Wavelength[n], data.Reflectance[n]);
                }
                sw.Close();

                m_Log.WriteLog(LogType.Datas, "Point: " + nPointIndex.ToString() + " Raw data saved.");
                return true;
            }
            catch (Exception ex)
            {
                m_Log.WriteLog(LogType.Error, "Point: " + nPointIndex.ToString() + " Failed to save raw data.");
                return false;
            }
        }

        #region Save 함수 추가 작업 할 것

        public bool SaveReflectance(string sPath, int nPointIdx)   //우리 포멧용
        {
            if (Path.GetExtension(sPath) != ".csv")
            {
                sPath += ".csv";
            }
            StreamWriter writer = new StreamWriter(sPath);
            RawData raw = m_RawData[nPointIdx];

            if (writer == null)
            {
                return false;
            }

            for (int n = 0; n < raw.nNIRDataNum; n++)
            {
                writer.WriteLine(raw.Wavelength[n].ToString() + "," + raw.Reflectance[n].ToString());
            }
            writer.Close();
            return true;
        }

        public bool SaveRT(string sPath, int nPointIdx)   //우리 포멧용
        {
            if (Path.GetExtension(sPath) != ".csv")
            {
                sPath += ".csv";
            }
            try
            {
                StreamWriter writer = new StreamWriter(sPath);
                RawData raw = m_RawData[nPointIdx];
                if (writer == null)
                {
                    return false;
                }
                writer.WriteLine("GoF," + Math.Round(raw.dGoF, 6).ToString());
                for (int n = 1; n < raw.Thickness.Count - 1; n++)
                {
                    writer.WriteLine(m_LayerData[n].sRefName + "," + (raw.Thickness[n]).ToString());
                }
                writer.WriteLine("Wavelength,Reflectance,Transmittance");
                for (int n = 0; n < raw.nNIRDataNum; n++)
                {
                    if (bTransmittance)
                    {
                        writer.WriteLine(raw.Wavelength[n].ToString() + "," + raw.Reflectance[n].ToString() + "," + raw.Transmittance[n].ToString());
                    }
                    else
                    {
                        writer.WriteLine(raw.Wavelength[n].ToString() + "," + raw.Reflectance[n].ToString());
                    }
                }
                writer.Close();
                return true;
            }
            catch (Exception e)
            {

                return false;
            }
           
        }

        public bool SaveResultFileDCOL(string sPath, InfoWafer infoWafer, RecipeDataManager recipeData, int nPointIdx, int nRepeatCount, int nRepeatIndex)
        {
            try
            {
                string[] sWaferNum = infoWafer.p_sWaferID.Split('.');
                StreamWriter sw = new StreamWriter(sPath);

                sw.WriteLine("LOT ID," + infoWafer.p_sCarrierID + "_" + infoWafer.p_sLotID);
                sw.WriteLine("DATE/TIME," + DateTime.Now.Month.ToString("00") + "/" + DateTime.Now.Day.ToString("00") + "/" + DateTime.Now.Year.ToString("0000") + " " + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00"));
                sw.WriteLine();
                sw.WriteLine("TOOL ID," + BaseDefine.TOOL_NAME);
                sw.WriteLine("SOFTWARE VERSION," + BaseDefine.Configuration.Version);
                sw.WriteLine("RECIPE," + infoWafer.p_sRecipe);
                sw.WriteLine("MACHINE TYPE," + "CAMELLIA");
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine("WAFER ID," + infoWafer.p_sWaferID);
                sw.WriteLine("LOT ID," + infoWafer.p_sCarrierID + "_" + infoWafer.p_sLotID);
                //sw.WriteLine("WAFER #," + sWaferNum[1]);
                sw.WriteLine("SLOT," + (infoWafer.m_nSlot + 1).ToString());
                sw.WriteLine("WAFER STATUS," + "Pass");
                sw.WriteLine("DATA TYPE," + "TF");
                sw.WriteLine("RECIPE," + infoWafer.p_sRecipe);
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();


                //////////////////////////////////////////Data////////////////////////
                string sHeader = "RESULT TYPE,NGOF";
                for (int n = 1; n < m_LayerData.Count - 1; n++)// Recipe 설정 Model data (박막 물질)
                {
                    sHeader += ",";
                    for (int s = 0; s < m_LayerData[n].hostname.Length; s++)
                    {
                        sHeader += m_LayerData[n].hostname[s];
                    }

                }
                sw.WriteLine(sHeader);

                int nTHKNum = m_LayerData.Count - 2;

                double dMeanGoF = 0, dMinGoF = 9999, dMaxGoF = 0, dStddevGoF = 0, d3SigmaGoF = 0;
                List<double> StddevGoFTmp = new List<double>();
                double[] dMeanThickness = new double[nTHKNum];
                double[] dMinThickness = new double[nTHKNum];
                double[] dMaxThickness = new double[nTHKNum];
                double[] dStddevThickness = new double[nTHKNum];
                double[] d3SigmaThickness = new double[nTHKNum];
                List<double>[] StddevTmp = new List<double>[nTHKNum];
                for (int n = 0; n < nTHKNum; n++)
                {
                    dMeanThickness[n] = 0;
                    dMinThickness[n] = 9999;
                    dMaxThickness[n] = 0;
                }

                int nDataNum = 0;
                for (int n = 0; n < nPointIdx; n++)
                {
                    if (n % nRepeatCount == nRepeatIndex)
                    {
                        double dGoF = m_RawData[n].dGoF;
                        if (m_RawData[n].Wavelength.Length != 0)
                        {
                            for (int i = 1; i < m_LayerData.Count - 1; i++)
                            {
                                if (StddevTmp[i - 1] == null)
                                    StddevTmp[i - 1] = new List<double>();
                                double dThickness = 0;
                                if (bThickness)
                                {
                                    dThickness = (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset);
                                }
                                else
                                {
                                    dThickness = 0.0000;
                                }
                                dMeanThickness[i - 1] += dThickness;
                                StddevTmp[i - 1].Add(dThickness);

                                if (dThickness < dMinThickness[i - 1])
                                {
                                    dMinThickness[i - 1] = dThickness;
                                }
                                if (dThickness > dMaxThickness[i - 1])
                                {
                                    dMaxThickness[i - 1] = dThickness;
                                }
                            }
                            dMeanGoF += dGoF;
                            StddevGoFTmp.Add(dGoF);
                            if (dGoF < dMinGoF)
                            {
                                dMinGoF = dGoF;
                            }
                            if (dGoF > dMaxGoF)
                            {
                                dMaxGoF = dGoF;
                            }
                            nDataNum++;
                        }
                    }
                }

                dMeanGoF /= (double)nDataNum;
                for (int i = 0; i < nTHKNum; i++)
                {
                    dMeanThickness[i] /= (double)nDataNum;
                    dStddevThickness[i] = StddevTmp[i].Stdev();
                    d3SigmaThickness[i] = dStddevThickness[i] * 3;
                }
                
                string sDataMean = "MEAN";
                string sDataMin = "MIN";
                string sDataMax = "MAX";
                string sDataStddev = "STDDEV";
                string sData3Sigma = "3 SIGMA";
                string sDataRange = "RANGE";

                sDataMean += "," + dMeanGoF.ToString("0.####");
                sDataMin += "," + dMinGoF.ToString("0.####");
                sDataMax += "," + dMaxGoF.ToString("0.####");
                sDataStddev += !double.IsNaN(dStddevGoF) ? "," + dStddevGoF.ToString("0.####") : ",0";
                sData3Sigma += !double .IsNaN(d3SigmaGoF)? "," + d3SigmaGoF.ToString("0.####") : ",0";
                sDataRange += "," + (dMaxGoF - dMinGoF).ToString("0.####");
                for (int i = 0; i < nTHKNum; i++)
                {
                    sDataMean += "," + dMeanThickness[i].ToString("0.####");
                    sDataMin += "," + dMinThickness[i].ToString("0.####");
                    sDataMax += "," + dMaxThickness[i].ToString("0.####");
                    sDataStddev += !double.IsNaN(dStddevGoF) ? "," + dStddevGoF.ToString("0.####") : ",0";
                    sData3Sigma += !double.IsNaN(d3SigmaGoF) ? "," + d3SigmaGoF.ToString("0.####") : ",0";
                    sDataRange += "," + (dMaxThickness[i] - dMinThickness[i]).ToString("0.####");
                }

                sw.WriteLine(sDataMean);
                sw.WriteLine(sDataMin);
                sw.WriteLine(sDataMax);
                sw.WriteLine(sDataStddev);
                sw.WriteLine(sData3Sigma);
                sw.WriteLine(sDataRange);

                sHeader = "Site #";
                for (int n = 1; n < m_LayerData.Count - 1; n++)
                {
                    sHeader += ",";
                    for (int s = 0; s < m_LayerData[n].hostname.Length; s++)
                    {
                        sHeader += m_LayerData[n].hostname[s];
                    }

                }
                sHeader += ",NGOF,Sum,X,Y,OFFSET X,OFFSET Y";
                sw.WriteLine(sHeader);

                string sData = string.Empty;
                for (int n = 0; n < nPointIdx; n++)
                {
                    if (n % nRepeatCount == nRepeatIndex)
                    {
                        int m = n / nRepeatCount;
                        if (m_RawData[n].Wavelength.Length != 0)
                        {
                            sData = (m + 1).ToString();
                            double dThicknessSum = 0;
                            for (int i = 1; i < m_LayerData.Count - 1; i++)
                            {
                                if (bThickness)
                                {
                                    sData += "," + (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset).ToString("0.####");
                                    dThicknessSum += (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset);
                                }
                                else
                                {
                                    sData += "," + (0).ToString("0.####");
                                    dThicknessSum += 0;
                                }
                            }
                            sData += "," + m_RawData[n].dGoF.ToString("0.####");
                            sData += "," + dThicknessSum.ToString("0.####");
                            sData += "," + m_RawData[n].dX.ToString("0.####");
                            sData += "," + m_RawData[n].dY.ToString("0.####");
                            sData += ",0";  //Offset x
                            sData += ",0";  //Offset y
                            sw.WriteLine(sData);
                        }
                    }
                }
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();

                sHeader = "X_Ref,Y_Ref,root lot ID,SLOT ID,Site";
                foreach (WavelengthItem R in m_ScalesListR)
                {
                    sHeader += ",R_" + R.p_waveLength.ToString("0.####");
                }
                foreach (WavelengthItem T in m_ScalesListT)
                {
                    sHeader += ",T_" + T.p_waveLength.ToString("0.####");
                }
                sw.WriteLine(sHeader);

                for (int n = 0; n < nPointIdx; n++)
                {
                    if (n % nRepeatCount == nRepeatIndex)
                    {
                        int m = n / nRepeatCount;
                        if (m_RawData[n].Wavelength.Count() != 0)
                        {
                            sData = string.Empty;
                            sData += m_RawData[n].dX.ToString("0.####") + "," + m_RawData[n].dY.ToString("0.####") + "," + infoWafer.p_sLotID + "," + infoWafer.p_sLotID + "," + (m + 1).ToString();
                            for (int i = 0; i < m_ContourMapDataR.Count; i++)
                            {
                                sData += "," + m_ContourMapDataR[i].HoleData[n].Value.ToString("0.####");
                            }
                            
                            for (int i = 0; i < m_ContourMapDataT.Count; i++)
                            {
                                sData += "," + m_ContourMapDataT[i].HoleData[n].Value.ToString("0.####");
                            }
                            sw.WriteLine(sData);
                        }
                    }
                }
                sw.Close();
                m_Log.WriteLog(LogType.Datas, " DCOL_Saved");
                return true;
            }
            catch (Exception ex)
            {
                m_Log.WriteLog(LogType.Error, " DCOL_Error");
                return false;
            }
        }
        public bool SaveResultFileDCOL(string sPath, InfoWafer infoWafer, RecipeDataManager recipeData, int nPointIdx)
        {
            try
            {
                string[] sWaferNum = infoWafer.p_sWaferID.Split('.');
                StreamWriter sw = new StreamWriter(sPath);

                sw.WriteLine("LOT ID," + infoWafer.p_sCarrierID + "_" + infoWafer.p_sLotID);
                sw.WriteLine("DATE/TIME," + DateTime.Now.Month.ToString("00") + "/" + DateTime.Now.Day.ToString("00") + "/" + DateTime.Now.Year.ToString("0000") + " " + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00"));
                sw.WriteLine();
                sw.WriteLine("TOOL ID," + BaseDefine.TOOL_NAME);
                sw.WriteLine("SOFTWARE VERSION," + BaseDefine.Configuration.Version);
                sw.WriteLine("RECIPE," + infoWafer.p_sRecipe);
                sw.WriteLine("MACHINE TYPE," + "CAMELLIA");
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine("WAFER ID," + infoWafer.p_sWaferID);
                sw.WriteLine("LOT ID," + infoWafer.p_sCarrierID + "_" + infoWafer.p_sLotID);
                //sw.WriteLine("WAFER #," + sWaferNum[1]);
                sw.WriteLine("SLOT," + infoWafer.p_sSlotID);
                sw.WriteLine("WAFER STATUS," + "Pass");
                sw.WriteLine("DATA TYPE," + "TF");
                sw.WriteLine("RECIPE," + infoWafer.p_sRecipe);
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();


                //////////////////////////////////////////Data////////////////////////
                string sHeader = "RESULT TYPE,NGOF";
                for (int n = 1; n < m_LayerData.Count - 1; n++)// Recipe 설정 Model data (박막 물질)
                {
                    sHeader += ",";
                    for (int s = 0; s < m_LayerData[n].hostname.Length; s++)
                    {
                        sHeader += m_LayerData[n].hostname[s];
                    }

                }
                sw.WriteLine(sHeader);

                int nTHKNum = m_LayerData.Count - 2;

                double dMeanGoF = 0, dMinGoF = 9999, dMaxGoF = 0, dStddevGoF = 0, d3SigmaGoF = 0;
                List<double> StddevGoFTmp = new List<double>();
                double[] dMeanThickness = new double[nTHKNum];
                double[] dMinThickness = new double[nTHKNum];
                double[] dMaxThickness = new double[nTHKNum];
                double[] dStddevThickness = new double[nTHKNum];
                double[] d3SigmaThickness = new double[nTHKNum];
                List<double>[] StddevTmp = new List<double>[nTHKNum];
                for (int n = 0; n < nTHKNum; n++)
                {
                    dMeanThickness[n] = 0;
                    dMinThickness[n] = 9999;
                    dMaxThickness[n] = 0;
                }

                int nDataNum = 0;
                for (int n = 0; n < nPointIdx; n++)
                {
                    double dGoF = m_RawData[n].dGoF;
                    if (m_RawData[n].Wavelength.Length != 0)
                    {
                        for (int i = 1; i < m_LayerData.Count - 1; i++)
                        {
                            if (StddevTmp[i - 1] == null)
                                StddevTmp[i - 1] = new List<double>();
                            double dThickness = 0;
                            if (bThickness)
                            {
                                dThickness = (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset);

                            }
                            else
                            {
                                dThickness = 0.0000;

                            }
                            dMeanThickness[i - 1] += dThickness;
                            StddevTmp[i - 1].Add(dThickness);

                            if (dThickness < dMinThickness[i - 1])
                            {
                                dMinThickness[i - 1] = dThickness;
                            }
                            if (dThickness > dMaxThickness[i - 1])
                            {
                                dMaxThickness[i - 1] = dThickness;
                            }
                        }
                        dMeanGoF += dGoF;
                        StddevGoFTmp.Add(dGoF);
                        if (dGoF < dMinGoF)
                        {
                            dMinGoF = dGoF;
                        }
                        if (dGoF > dMaxGoF)
                        {
                            dMaxGoF = dGoF;
                        }
                        nDataNum++;
                    }
                }

                dMeanGoF /= (double)nDataNum;
                for (int i = 0; i < nTHKNum; i++)
                {
                    dMeanThickness[i] /= (double)nDataNum;
                    dStddevThickness[i] = StddevTmp[i].Stdev();
                    d3SigmaThickness[i] = dStddevThickness[i] * 3;
                }

                string sDataMean = "MEAN";
                string sDataMin = "MIN";
                string sDataMax = "MAX";
                string sDataStddev = "STDDEV";
                string sData3Sigma = "3 SIGMA";
                string sDataRange = "RANGE";

                sDataMean += "," + dMeanGoF.ToString("0.####");
                sDataMin += "," + dMinGoF.ToString("0.####");
                sDataMax += "," + dMaxGoF.ToString("0.####");
                sDataStddev += !double.IsNaN(dStddevGoF) ? "," + dStddevGoF.ToString("0.####") : ",0";
                sData3Sigma += !double.IsNaN(d3SigmaGoF) ? "," + d3SigmaGoF.ToString("0.####") : ",0";
                sDataRange += "," + (dMaxGoF - dMinGoF).ToString("0.####");

                for (int i = 0; i < nTHKNum; i++)
                {
                    sDataMean += "," + dMeanThickness[i].ToString("0.####");
                    sDataMin += "," + dMinThickness[i].ToString("0.####");
                    sDataMax += "," + dMaxThickness[i].ToString("0.####");
                    sDataStddev += !double.IsNaN(dStddevGoF) ? "," + dStddevGoF.ToString("0.####") : ",0";
                    sData3Sigma += !double.IsNaN(d3SigmaGoF) ? "," + d3SigmaGoF.ToString("0.####") : ",0";
                    sDataRange += "," + (dMaxThickness[i] - dMinThickness[i]).ToString("0.####");
                }

                sw.WriteLine(sDataMean);
                sw.WriteLine(sDataMin);
                sw.WriteLine(sDataMax);
                sw.WriteLine(sDataStddev);
                sw.WriteLine(sData3Sigma);
                sw.WriteLine(sDataRange);

                sHeader = "Site #";
                for (int n = 1; n < m_LayerData.Count - 1; n++)
                {
                    sHeader += ",";
                    for (int s = 0; s < m_LayerData[n].hostname.Length; s++)
                    {
                        sHeader += m_LayerData[n].hostname[s];
                    }

                }
                sHeader += ",NGOF,Sum,X,Y,OFFSET X,OFFSET Y";
                sw.WriteLine(sHeader);

                string sData = string.Empty;
                for (int n = 0; n < nPointIdx; n++)
                {
                    if (m_RawData[n].Wavelength.Length != 0)
                    {
                        sData = (n + 1).ToString();
                        double dThicknessSum = 0;
                        for (int i = 1; i < m_LayerData.Count - 1; i++)
                        {
                            if (bThickness)
                            {
                                sData += "," + (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset).ToString("0.####");
                                dThicknessSum += (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset);
                            }
                            else
                            {
                                sData += "," + (0).ToString("0.####");
                                dThicknessSum += 0;
                            }
                        }
                        sData += "," + m_RawData[n].dGoF.ToString("0.####");
                        sData += "," + dThicknessSum.ToString("0.####");
                        sData += "," + m_RawData[n].dX.ToString("0.####");
                        sData += "," + m_RawData[n].dY.ToString("0.####");
                        sData += ",0";  //Offset x
                        sData += ",0";  //Offset y
                        sw.WriteLine(sData);
                    }
                }
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();

                sHeader = "X_Ref,Y_Ref,root lot ID,SLOT ID,Site";
                foreach (WavelengthItem R in m_ScalesListR)
                {
                    sHeader += ",R_" + R.p_waveLength.ToString("0.####");
                }
                foreach (WavelengthItem T in m_ScalesListT)
                {
                    sHeader += ",T_" + T.p_waveLength.ToString("0.####");
                }
                sw.WriteLine(sHeader);

                for (int n = 0; n < nPointIdx; n++)
                {
                    if (m_RawData[n].Wavelength.Count() != 0)
                    {
                        sData = string.Empty;
                        sData += m_RawData[n].dX.ToString("0.####") + "," + m_RawData[n].dY.ToString("0.####") + "," + infoWafer.p_sLotID + "," + infoWafer.p_sLotID + "," + (n + 1).ToString();
                        for (int i = 0; i < m_ContourMapDataR.Count; i++)
                        {
                            sData += "," + m_ContourMapDataR[i].HoleData[n].Value.ToString("0.####");
                        }
                        for (int i = 0; i < m_ContourMapDataT.Count; i++)
                        {
                            sData += "," + m_ContourMapDataT[i].HoleData[n].Value.ToString("0.####");
                        }
                        sw.WriteLine(sData);
                    }
                }
                sw.Close();
                m_Log.WriteLog(LogType.Datas, " DCOL_Saved");
                return true;
            }
            catch (Exception ex)
            {
                m_Log.WriteLog(LogType.Error, " DCOL_Error");
                return false;
            }
        }
        public bool SaveResultFileLot(string sPath, InfoWafer infoWafer, RecipeDataManager recipeData, int nPointIdx)
        {
            try
            {
                bool bFirst = true;
                if (File.Exists(sPath))
                {
                    bFirst = false;
                }
                StreamWriter sw = new StreamWriter(sPath, true);
                string[] sWaferNum = infoWafer.p_sWaferID.Split('.');
                if (infoWafer != null)
                {
                    if (bFirst == true)
                    {
                        sw.WriteLine("LOT ID," + infoWafer.p_sCarrierID + "_" + infoWafer.p_sLotID);
                        sw.WriteLine("DATE/TIME," + DateTime.Now.Month.ToString("00") + "/" + DateTime.Now.Day.ToString("00") + "/" + DateTime.Now.Year.ToString("0000") + " " + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00"));
                        sw.WriteLine();
                        sw.WriteLine("TOOL ID," + BaseDefine.TOOL_NAME);
                        sw.WriteLine("SOFTWARE VERSION," + BaseDefine.Configuration.Version);
                        sw.WriteLine("RECIPE," + recipeData.TeachRecipeName);
                        sw.WriteLine("MACHINE TYPE," + "CAMELLIA");
                        sw.WriteLine();
                        sw.WriteLine();
                        sw.WriteLine();
                    }
                    sw.WriteLine("WAFER ID," + infoWafer.p_sWaferID);
                    sw.WriteLine("LOT ID," + infoWafer.p_sCarrierID + "_" + infoWafer.p_sLotID);
                    if (sWaferNum.Length > 1)
                    {
                        sw.WriteLine("WAFER #," + sWaferNum[1]);
                    }
                    else
                    {
                        sw.WriteLine("WAFER #," + "");
                    }
                    sw.WriteLine("SLOT," + infoWafer.p_sSlotID);
                    sw.WriteLine("WAFER STATUS," + "Pass");
                    sw.WriteLine("DATA TYPE," + "TF");
                    sw.WriteLine("RECIPE," + recipeData.TeachRecipeName);
                    sw.WriteLine();
                    sw.WriteLine();
                    sw.WriteLine();
                }
                string sHeader = "RESULT TYPE,NGOF,Sum";
                for (int n = 1; n < m_LayerData.Count - 1; n++)// Recipe 설정 Model data (박막 물질)
                {
                    sHeader += ",";
                    for (int s = 0; s < m_LayerData[n].hostname.Length; s++)
                    {
                        sHeader += m_LayerData[n].hostname[s];
                    }

                }
                sw.WriteLine(sHeader);

                /////////////////Data////////////////////
                int nTHKNum = m_LayerData.Count - 2;

                double dMeanGoF = 0, dMinGoF = 9999, dMaxGoF = 0, dStddevGoF = 0, d3SigmaGoF = 0;
                List<double> StddevGoFTmp = new List<double>();
                double[] dMeanThickness = new double[nTHKNum];
                double[] dMinThickness = new double[nTHKNum];
                double[] dMaxThickness = new double[nTHKNum];
                double[] dStddevThickness = new double[nTHKNum];
                double[] d3SigmaThickness = new double[nTHKNum];
                List<double>[] StddevTmp = new List<double>[nTHKNum];

                double dMeanTHKSum = 0, dMinTHKSum = 0, dMaxTHKSum = 0, dStddevTHKSum = 0, d3SigmaTHKSum = 0, dRangeTHKSum = 0;
                for (int n = 0; n < nTHKNum; n++)
                {
                    dMeanThickness[n] = 0;
                    dMinThickness[n] = 9999;
                    dMaxThickness[n] = 0;
                }

                int nDataNum = 0;
                for (int n = 0; n < nPointIdx; n++)
                {
                    double dGoF = m_RawData[n].dGoF;
                    if (m_RawData[n].Wavelength.Length != 0)
                    {
                        for (int i = 1; i < m_LayerData.Count - 1; i++)
                        {
                            if (StddevTmp[i - 1] == null)
                                StddevTmp[i - 1] = new List<double>();

                            double dThickness = 0;
                            if (bThickness)
                            {
                                dThickness = (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset);

                            }
                            else
                            {
                                dThickness = 0.0000;

                            }
                            dMeanThickness[i - 1] += dThickness;
                            StddevTmp[i - 1].Add(dThickness);

                            if (dThickness < dMinThickness[i - 1])
                            {
                                dMinThickness[i - 1] = dThickness;
                            }
                            if (dThickness > dMaxThickness[i - 1])
                            {
                                dMaxThickness[i - 1] = dThickness;
                            }
                        }
                        dMeanGoF += dGoF;
                        StddevGoFTmp.Add(dGoF);
                        if (dGoF < dMinGoF)
                        {
                            dMinGoF = dGoF;
                        }
                        if (dGoF > dMaxGoF)
                        {
                            dMaxGoF = dGoF;
                        }
                        nDataNum++;
                    }
                }

                dMeanGoF /= (double)nDataNum;
                for (int i = 0; i < nTHKNum; i++)
                {
                    dMeanThickness[i] /= (double)nDataNum;
                    dStddevThickness[i] = StddevTmp[i].Stdev();
                    d3SigmaThickness[i] = dStddevThickness[i] * 3;

                    dMeanTHKSum += dMeanThickness[i];
                    dMinTHKSum += dMinThickness[i];
                    dMaxTHKSum += dMaxThickness[i];
                    dStddevTHKSum += dStddevThickness[i];
                    d3SigmaTHKSum += d3SigmaThickness[i];
                    dRangeTHKSum += dMaxThickness[i] - dMinThickness[i];
                }

                string sDataMean = "MEAN";
                string sDataMin = "MIN";
                string sDataMax = "MAX";
                string sDataStddev = "STDDEV";
                string sData3Sigma = "3 SIGMA";
                string sDataRange = "RANGE";

                sDataMean += "," + dMeanGoF.ToString("0.####") + "," + dMeanTHKSum.ToString("0.####");
                sDataMin += "," + dMinGoF.ToString("0.####") + "," + dMinTHKSum.ToString("0.####");
                sDataMax += "," + dMaxGoF.ToString("0.####") + "," + dMaxTHKSum.ToString("0.####");
                sDataStddev += !double.IsNaN(dStddevGoF) ? "," + dStddevGoF.ToString("0.####")+"," : ",0";
                sDataStddev += !double.IsNaN(dStddevTHKSum) ? "," + dStddevTHKSum.ToString("0.####") : ",0";
                sData3Sigma += !double.IsNaN(d3SigmaGoF) ? "," + d3SigmaGoF.ToString("0.####") +",": ",0";
                sData3Sigma += !double.IsNaN(d3SigmaTHKSum) ? "," + d3SigmaTHKSum.ToString("0.####") : ",0";
                sDataRange += "," + (dMaxGoF - dMinGoF).ToString("0.####") + "," + dRangeTHKSum.ToString("0.####");

                for (int i = 0; i < nTHKNum; i++)
                {
                    sDataMean += "," + dMeanThickness[i].ToString("0.####");
                    sDataMin += "," + dMinThickness[i].ToString("0.####");
                    sDataMax += "," + dMaxThickness[i].ToString("0.####");
                    sDataStddev += !double.IsNaN(dStddevThickness[i]) ? "," + dStddevThickness[i].ToString("0.####") : ",0";
                    sData3Sigma += !double.IsNaN(d3SigmaThickness[i]) ? "," + d3SigmaThickness[i].ToString("0.####") : ",0";
                    sDataRange += "," + (dMaxThickness[i] - dMinThickness[i]).ToString("0.####");
                }

                sw.WriteLine(sDataMean);
                sw.WriteLine(sDataMin);
                sw.WriteLine(sDataMax);
                sw.WriteLine(sDataStddev);
                sw.WriteLine(sData3Sigma);
                sw.WriteLine(sDataRange);
                sw.WriteLine();

                sHeader = "Site #";
                for (int n = 1; n < m_LayerData.Count - 1; n++)// Recipe 설정 Model data (박막 물질)
                {
                    sHeader += ",";
                    for (int s = 0; s < m_LayerData[n].hostname.Length; s++)
                    {
                        sHeader += m_LayerData[n].hostname[s];
                    }

                }
                sHeader += ",NGOF,Sum,X,Y,OFFSET X,OFFSET Y";
                sw.WriteLine(sHeader);

                string sData = string.Empty;
                for (int n = 0; n < nPointIdx; n++)
                {
                    if (m_RawData[n].Wavelength.Length != 0)
                    // 수정? Save 파일에 저장되는 파장 데이터들이 350~1500nm 범위 배열인지 확인할것
                    // 또한 저장되도록 지정된 파장 배열 범위가 350~1500nm 인지 확인할 것
                    {
                        sData = (n + 1).ToString();
                        double dThicknessSum = 0;
                        for (int i = 1; i < m_LayerData.Count - 1; i++)
                        {
                            if (bThickness)
                            {
                                sData += "," + (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset).ToString("0.####");
                                dThicknessSum += (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset);
                            }
                            else
                            {
                                sData += "," + (0).ToString("0.####");
                                dThicknessSum += 0;
                            }
                        }
                        sData += "," + m_RawData[n].dGoF.ToString("0.####");
                        sData += "," + dThicknessSum.ToString("0.####");
                        sData += "," + m_RawData[n].dX.ToString("0.####");
                        sData += "," + m_RawData[n].dY.ToString("0.####");
                        sData += ",0";  //Offset x
                        sData += ",0";  //Offset y
                        sw.WriteLine(sData);
                    }
                }
                sw.WriteLine();

                //summary
                sHeader = "X_Ref,Y_Ref,root lot ID,SLOT ID,Site";
                foreach (WavelengthItem R in m_ScalesListR)
                {
                    sHeader += ",R_" + R.p_waveLength.ToString("0.####");
                }
                foreach (WavelengthItem T in m_ScalesListT)
                {
                    sHeader += ",T_" + T.p_waveLength.ToString("0.####");
                }
                sHeader += ",GOF";
                for (int n = 1; n < m_LayerData.Count - 1; n++)
                {
                    sHeader += ",";
                    for (int s = 0; s < m_LayerData[n].hostname.Length; s++)
                    {
                        sHeader += m_LayerData[n].hostname[s];
                    }

                }
                sw.WriteLine(sHeader);

                for (int n = 0; n < nPointIdx; n++)
                {
                    if (m_RawData[n].Wavelength.Length != 0)
                    {
                        sData = string.Empty;
                        sData += m_RawData[n].dX.ToString("0.####") + "," + m_RawData[n].dY.ToString("0.####") + "," + infoWafer.p_sLotID + "," + infoWafer.p_sLotID + "," + (n + 1).ToString();
                        for (int i = 0; i < m_ContourMapDataR.Count; i++)
                        {
                            sData += "," + m_ContourMapDataR[i].HoleData[n].Value.ToString("0.####");
                        }
                        for (int i = 0; i < m_ContourMapDataT.Count; i++)
                        {
                            sData += "," + m_ContourMapDataT[i].HoleData[n].Value.ToString("0.####");
                        }
                        sData += "," + m_RawData[n].dGoF.ToString("0.####");
                        for (int i = 1; i < m_LayerData.Count - 1; i++)
                        {
                            if (bThickness)
                            {
                                sData += "," + (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset).ToString("0.####");
                            }
                            else
                            {
                                sData += "," + (0).ToString("0.####");
                            }
                        }
                        sw.WriteLine(sData);
                    }
                }

                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();
                sw.Close();

                m_Log.WriteLog(LogType.Datas, " SaveResultFileLot()_Saved");
                return true;
            }
            catch (Exception ex)
            {
                m_Log.WriteLog(LogType.Error, "SaveResultFileLot() - Error");
                return false;
            }
        }
        public bool SaveResultFileLot(string sPath, InfoWafer infoWafer, RecipeDataManager recipeData, int nPointIdx, int nRepeatCount, int nRepeatIndex)
        {
            try
            {
                bool bFirst = true;
                if (File.Exists(sPath))
                {
                    bFirst = false;
                }
                StreamWriter sw = new StreamWriter(sPath, true);
                string[] sWaferNum = infoWafer.p_sWaferID.Split('.');
                if (infoWafer != null)
                {
                    if (bFirst == true)

                    {
                        sw.WriteLine("LOT ID," + infoWafer.p_sCarrierID + "_" + infoWafer.p_sLotID);
                        sw.WriteLine("DATE/TIME," + DateTime.Now.Month.ToString("00") + "/" + DateTime.Now.Day.ToString("00") + "/" + DateTime.Now.Year.ToString("0000") + " " + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00"));
                        sw.WriteLine();
                        sw.WriteLine("TOOL ID," + BaseDefine.TOOL_NAME);
                        sw.WriteLine("SOFTWARE VERSION," + BaseDefine.Configuration.Version);
                        sw.WriteLine("RECIPE," + recipeData.TeachRecipeName);
                        sw.WriteLine("MACHINE TYPE," + "CAMELLIA");
                        sw.WriteLine();
                        sw.WriteLine();
                        sw.WriteLine();
                    }
                    sw.WriteLine("WAFER ID," + infoWafer.p_sWaferID);
                    sw.WriteLine("LOT ID," + infoWafer.p_sCarrierID + "_" + infoWafer.p_sLotID);
                    if (sWaferNum.Length > 1)
                    {
                        sw.WriteLine("WAFER #," + sWaferNum[1]);
                    }
                    else
                    {
                        sw.WriteLine("WAFER #," + "");
                    }
                    sw.WriteLine("SLOT," + infoWafer.p_sSlotID);
                    sw.WriteLine("WAFER STATUS," + "Pass");
                    sw.WriteLine("DATA TYPE," + "TF");
                    sw.WriteLine("RECIPE," + recipeData.TeachRecipeName);
                    sw.WriteLine();
                    sw.WriteLine();
                    sw.WriteLine();
                }
                string sHeader = "RESULT TYPE,NGOF,Sum";
                for (int n = 1; n < m_LayerData.Count - 1; n++)// Recipe 설정 Model data (박막 물질)
                {
                    sHeader += ",";
                    for (int s = 0; s < m_LayerData[n].hostname.Length; s++)
                    {
                        sHeader += m_LayerData[n].hostname[s];
                    }

                }
                sw.WriteLine(sHeader);

                /////////////////Data////////////////////
                int nTHKNum = m_LayerData.Count - 2;

                double dMeanGoF = 0, dMinGoF = 9999, dMaxGoF = 0, dStddevGoF = 0, d3SigmaGoF = 0;
                List<double> StddevGoFTmp = new List<double>();
                double[] dMeanThickness = new double[nTHKNum];
                double[] dMinThickness = new double[nTHKNum];
                double[] dMaxThickness = new double[nTHKNum];
                double[] dStddevThickness = new double[nTHKNum];
                double[] d3SigmaThickness = new double[nTHKNum];
                List<double>[] StddevTmp = new List<double>[nTHKNum];

                double dMeanTHKSum = 0, dMinTHKSum = 0, dMaxTHKSum = 0, dStddevTHKSum = 0, d3SigmaTHKSum = 0, dRangeTHKSum = 0;
                for (int n = 0; n < nTHKNum; n++)
                {
                    dMeanThickness[n] = 0;
                    dMinThickness[n] = 9999;
                    dMaxThickness[n] = 0;
                }

                int nDataNum = 0;
                for (int n = 0; n < nPointIdx; n++)
                {
                    if (n % nRepeatCount == nRepeatIndex)
                    {
                        double dGoF = m_RawData[n].dGoF;
                        if (m_RawData[n].Wavelength.Length != 0)
                        {
                            for (int i = 1; i < m_LayerData.Count - 1; i++)
                            {
                                if (StddevTmp[i - 1] == null)
                                    StddevTmp[i - 1] = new List<double>();
                                double dThickness = 0;
                                if (bThickness)
                                {
                                    dThickness = (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset);
                                }
                                else
                                {
                                    dThickness = 0.0000;
                                }
                                dMeanThickness[i - 1] += dThickness;
                                StddevTmp[i - 1].Add(dThickness);

                                if (dThickness < dMinThickness[i - 1])
                                {
                                    dMinThickness[i - 1] = dThickness;
                                }
                                if (dThickness > dMaxThickness[i - 1])
                                {
                                    dMaxThickness[i - 1] = dThickness;
                                }
                            }
                            dMeanGoF += dGoF;
                            StddevGoFTmp.Add(dGoF);
                            if (dGoF < dMinGoF)
                            {
                                dMinGoF = dGoF;
                            }
                            if (dGoF > dMaxGoF)
                            {
                                dMaxGoF = dGoF;
                            }
                            nDataNum++;
                        }
                    }
                }

                dMeanGoF /= (double)nDataNum;
                for (int i = 0; i < nTHKNum; i++)
                {
                    dMeanThickness[i] /= (double)nDataNum;
                    dStddevThickness[i] = StddevTmp[i].Stdev();
                    d3SigmaThickness[i] = dStddevThickness[i] * 3;

                    dMeanTHKSum += dMeanThickness[i];
                    dMinTHKSum += dMinThickness[i];
                    dMaxTHKSum += dMaxThickness[i];
                    dStddevTHKSum += dStddevThickness[i];
                    d3SigmaTHKSum += d3SigmaThickness[i];
                    dRangeTHKSum += dMaxThickness[i] - dMinThickness[i];
                }

                string sDataMean = "MEAN";
                string sDataMin = "MIN";
                string sDataMax = "MAX";
                string sDataStddev = "STDDEV";
                string sData3Sigma = "3 SIGMA";
                string sDataRange = "RANGE";

                sDataMean += "," + dMeanGoF.ToString("0.####") + "," + dMeanTHKSum.ToString("0.####");
                sDataMin += "," + dMinGoF.ToString("0.####") + "," + dMinTHKSum.ToString("0.####");
                sDataMax += "," + dMaxGoF.ToString("0.####") + "," + dMaxTHKSum.ToString("0.####");
                sDataStddev += !double.IsNaN(dStddevGoF) ? "," + dStddevGoF.ToString("0.####") : ",0";
                sDataStddev += !double.IsNaN(dStddevTHKSum) ? "," + dStddevTHKSum.ToString("0.####") : ",0";
                sData3Sigma += !double.IsNaN(d3SigmaGoF) ? "," + d3SigmaGoF.ToString("0.####") : ",0";
                sData3Sigma += !double.IsNaN(d3SigmaTHKSum) ? "," + d3SigmaTHKSum.ToString("0.####") : ",0";
                sDataRange += "," + (dMaxGoF - dMinGoF).ToString("0.####") + "," + dRangeTHKSum.ToString("0.####");

                for (int i = 0; i < nTHKNum; i++)
                {
                    sDataMean += "," + dMeanThickness[i].ToString("0.####");
                    sDataMin += "," + dMinThickness[i].ToString("0.####");
                    sDataMax += "," + dMaxThickness[i].ToString("0.####");
                    sDataStddev += !double.IsNaN(dStddevThickness[i]) ? "," + dStddevThickness[i].ToString("0.####") : ",0";
                    sData3Sigma += !double.IsNaN(d3SigmaThickness[i]) ? "," + d3SigmaThickness[i].ToString("0.####") : ",0";    
                    sDataRange += "," + (dMaxThickness[i] - dMinThickness[i]).ToString("0.####");
                }

                sw.WriteLine(sDataMean);
                sw.WriteLine(sDataMin);
                sw.WriteLine(sDataMax);
                sw.WriteLine(sDataStddev);
                sw.WriteLine(sData3Sigma);
                sw.WriteLine(sDataRange);
                sw.WriteLine();

                sHeader = "Site #";
                for (int n = 1; n < m_LayerData.Count - 1; n++)// Recipe 설정 Model data (박막 물질)
                {
                    sHeader += ",";
                    for (int s = 0; s < m_LayerData[n].hostname.Length; s++)
                    {
                        sHeader += m_LayerData[n].hostname[s];
                    }

                }
                sHeader += ",NGOF,Sum,X,Y,OFFSET X,OFFSET Y";
                sw.WriteLine(sHeader);

                string sData = string.Empty;
                for (int n = 0; n < nPointIdx; n++)
                {
                    if (n % nRepeatCount == nRepeatIndex)
                    {
                        int m = n / nRepeatCount;
                        if (m_RawData[n].Wavelength.Length != 0)
                        // 수정? Save 파일에 저장되는 파장 데이터들이 350~1500nm 범위 배열인지 확인할것
                        // 또한 저장되도록 지정된 파장 배열 범위가 350~1500nm 인지 확인할 것
                        {
                            sData = (m + 1).ToString();
                            double dThicknessSum = 0;
                            for (int i = 1; i < m_LayerData.Count - 1; i++)
                            {
                                if (bThickness)
                                {
                                    sData += "," + (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset).ToString("0.####");
                                    dThicknessSum += (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset);
                                }
                                else
                                {
                                    sData += "," + (0).ToString("0.####");
                                    dThicknessSum += 0;
                                }
                            }

                            sData += "," + m_RawData[n].dGoF.ToString("0.####");
                            sData += "," + dThicknessSum.ToString("0.####");
                            sData += "," + m_RawData[n].dX.ToString("0.####");
                            sData += "," + m_RawData[n].dY.ToString("0.####");
                            sData += ",0";  //Offset x
                            sData += ",0";  //Offset y
                            sw.WriteLine(sData);
                        }
                    }
                }
                sw.WriteLine();

                //summary
                sHeader = "X_Ref,Y_Ref,root lot ID,SLOT ID,Site";
                foreach (WavelengthItem R in m_ScalesListR)
                {
                    sHeader += ",R_" + R.p_waveLength.ToString("0.####");
                }
                foreach (WavelengthItem T in m_ScalesListT)
                {
                    sHeader += ",T_" + T.p_waveLength.ToString("0.####");
                }
                sHeader += ",GOF";
                for (int n = 1; n < m_LayerData.Count - 1; n++)
                {
                    sHeader += ",";
                    for (int s = 0; s < m_LayerData[n].hostname.Length; s++)
                    {
                        sHeader += m_LayerData[n].hostname[s];
                    }

                }
                sw.WriteLine(sHeader);

                for (int n = 0; n < nPointIdx; n++)
                {
                    if (n % nRepeatCount == nRepeatIndex)
                    {
                        int m = n / nRepeatCount;
                        if (m_RawData[n].Wavelength.Length != 0)
                        {
                            sData = string.Empty;
                            sData += m_RawData[n].dX.ToString("0.####") + "," + m_RawData[n].dY.ToString("0.####") + "," + infoWafer.p_sLotID + "," + infoWafer.p_sLotID + "," + (m + 1).ToString();
                            for (int i = 0; i < m_ContourMapDataR.Count; i++)
                            {
                                sData += "," + m_ContourMapDataR[i].HoleData[n].Value.ToString("0.####");
                            }
                            for (int i = 0; i < m_ContourMapDataT.Count; i++)
                            {
                                sData += "," + m_ContourMapDataT[i].HoleData[n].Value.ToString("0.####");
                            }
                            sData += "," + m_RawData[n].dGoF.ToString("0.####");
                            for (int i = 1; i < m_LayerData.Count - 1; i++)
                            {
                                if (bThickness)
                                {
                                    sData += "," + (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset).ToString("0.####");
                                }
                                else
                                {
                                    sData += "," + (0).ToString("0.####");
                                }
                            }
                            sw.WriteLine(sData);
                        }
                    }
                }

                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();
                sw.Close();

                m_Log.WriteLog(LogType.Datas, " SaveResultFileLot()_Saved");
                return true;
            }
            catch (Exception ex)
            {
                m_Log.WriteLog(LogType.Error, "SaveResultFileLot() - Error");
                return false;
            }
        }

        public bool SaveResultFileSlot(string sPath, InfoWafer infoWafer, RecipeDataManager recipeData, int nPointIdx, int nPointCount)
        {
            if (Path.GetExtension(sPath) != ".csv")
            {
                sPath += ".csv";
            }

            int n = 0;
            try
            {
                StreamWriter sw = new StreamWriter(sPath);
                RawData raw = m_RawData[nPointIdx];
                if (infoWafer != null)
                {
                    string[] sWaferNum = infoWafer.p_sWaferID.Split('.');
                    //RawData raw = m_RawData[nPointIdx];

                    //StreamWriter sw = new StreamWriter(sPath);
                    sw.WriteLine("LOT ID," + infoWafer.p_sCarrierID + "_" + infoWafer.p_sLotID);
                    //여기수정
                    sw.WriteLine("DATE/TIME," + DateTime.Now.Month.ToString("00") + "/" + DateTime.Now.Day.ToString("00") + "/" + DateTime.Now.Year.ToString("0000") + " " + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00"));
                    sw.WriteLine();
                    sw.WriteLine("TOOL ID," + BaseDefine.TOOL_NAME);
                    sw.WriteLine("SOFTWARE VERSION," + BaseDefine.Configuration.Version);
                    sw.WriteLine("RECIPE," + recipeData.TeachRecipeName);
                    sw.WriteLine("MACHINE TYPE," + "CAMELLIA");
                    sw.WriteLine();
                    sw.WriteLine();
                    sw.WriteLine();
                    sw.WriteLine("WAFER ID," + infoWafer.p_sWaferID);
                    sw.WriteLine("LOT ID," + infoWafer.p_sCarrierID + "_" + infoWafer.p_sLotID);
                    if (sWaferNum.Length > 1)
                    {
                        sw.WriteLine("WAFER #," + sWaferNum[1]);
                    }
                    else
                    {
                        sw.WriteLine("WAFER #," + "");
                    }
                    sw.WriteLine("SLOT," + infoWafer.p_sSlotID);
                    sw.WriteLine();
                    sw.WriteLine("WAFER STATUS," + "Pass");
                    sw.WriteLine("DATA TYPE," + "TF");
                    sw.WriteLine("RECIPE," + recipeData.TeachRecipeName);
                    // m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[index]].x
                    sw.WriteLine("X_Position," + recipeData.MeasurementRD.DataSelectedPoint[recipeData.MeasurementRD.DataMeasurementRoute[nPointCount]].x.ToString("F3"));
                    sw.WriteLine("Y_Position," + recipeData.MeasurementRD.DataSelectedPoint[recipeData.MeasurementRD.DataMeasurementRoute[nPointCount]].y.ToString("F3"));
                    sw.WriteLine();
                }


                sw.WriteLine("Wavelength [nm],Reflectance,Transmittance");
                for (n = 0; n < raw.nNIRDataNum; n++)
                {
                    if (bTransmittance)
                    {
                        sw.WriteLine(raw.Wavelength[n].ToString("0.####") + "," + raw.Reflectance[n].ToString("0.####") + "," + raw.Transmittance[n].ToString("0.####"));
                    }
                    else
                    {
                        sw.WriteLine(raw.Wavelength[n].ToString("0.####") + "," + raw.Reflectance[n].ToString("0.####") + ",0");
                    }
                }
                sw.Close();
                m_Log.WriteLog(LogType.Datas, " SaveResultFileSlot()_Saved");
                return true;
            }
            catch (Exception ex)
            {
                m_Log.WriteLog(LogType.Error, "SaveResultFileSlot() - Error / <" + n.ToString() + "> - " + ex.Message);
                return false;
            }
        }

        public bool SaveResultFileSlot(string sPath, string sFoupID, string sLotID, string sToolID, string sWaferID, string sSlotID, string sSWVersion, string sRCPName, int nPointIdx, double dXPos, double dYPos, double dLowerWavelength, double dUpperWavelength)
        {
            int n = 0;
            try
            {
                //string[] sWaferNum = sWaferID.Split('.');
                RawData raw = m_RawData[nPointIdx];

                StreamWriter sw = new StreamWriter(sPath);
                sw.WriteLine("LOT ID," + sFoupID + "_" + sLotID);
                //여기수정
                sw.WriteLine("DATE/TIME," + DateTime.Now.Month.ToString("00") + "/" + DateTime.Now.Day.ToString("00") + "/" + DateTime.Now.Year.ToString("0000") + " " + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00"));
                sw.WriteLine();
                sw.WriteLine("TOOL ID," + sToolID);
                sw.WriteLine("SOFTWARE VERSION," + sSWVersion);
                sw.WriteLine("RECIPE," + sRCPName);
                sw.WriteLine("MACHINE TYPE," + "CAMELLIA");
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine("WAFER ID," + sWaferID);
                sw.WriteLine("LOT ID," + sFoupID + "_" + sLotID);
                //  sw.WriteLine("WAFER #," + sWaferNum[1]);
                sw.WriteLine("SLOT," + sSlotID);
                sw.WriteLine();
                sw.WriteLine("WAFER STATUS," + "Pass");
                sw.WriteLine("DATA TYPE," + "TF");
                sw.WriteLine("RECIPE," + sRCPName);
                sw.WriteLine("X_Position," + dXPos.ToString("F3"));
                sw.WriteLine("Y_Position," + dYPos.ToString("F3"));
                sw.WriteLine();
                sw.WriteLine("Wavelength [nm],Reflectance,Transmittance");
                for (n = 0; n < raw.nNIRDataNum; n++)
                {
                    if (bTransmittance)
                    {
                        sw.WriteLine(raw.Wavelength[n].ToString("0.####") + "," + raw.Reflectance[n].ToString("0.####") + "," + raw.Transmittance[n].ToString("0.####"));
                    }
                    else
                    {
                        sw.WriteLine(raw.Wavelength[n].ToString("0.####") + "," + raw.Reflectance[n].ToString("0.####") + ",0");
                    }
                }
                sw.Close();
                m_Log.WriteLog(LogType.Datas, " SaveResultFileSlot()_Saved");
                return true;
            }
            catch (Exception ex)
            {
                m_Log.WriteLog(LogType.Error, "SaveResultFileSlot() - Error / <" + n.ToString() + "> - " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// R, T 각각 count 만큼 for문 돌려서 저장하시면 됩니다.
        /// </summary>
        /// <param name="sPath">contourmap 데이터 파일 저장경로</param>
        /// <param name="mapdata">저장할 contourmap data</param>
        /// <returns></returns>
        public bool SaveContourMapData(string sPath, ContourMapData mapdata, int nRepeatCount, int nRepeatIndex)
        {
            try
            {
                bool isRepeat = false;
                if (nRepeatCount == 1)
                {
                    isRepeat = false;
                }
                else
                {
                    isRepeat = true;
                }
                if (Path.GetExtension(sPath) != ".csv")
                {
                    sPath += ".csv";
                }

                StreamWriter writer = new StreamWriter(sPath);
                writer.WriteLine("X[mm],Y[mm],Value[%]");

                if (isRepeat == false)
                {
                    for (int n = 0; n < mapdata.HoleData.Count; n++)
                    {
                        writer.WriteLine(mapdata.HoleData[n].XPos.ToString() + "," + mapdata.HoleData[n].YPos.ToString() + "," + mapdata.HoleData[n].Value.ToString());
                    }
                }
                else
                {
                    for (int n = 0; n < mapdata.HoleData.Count; n++)
                    {
                        if (n % nRepeatCount == nRepeatIndex)
                        {
                            writer.WriteLine(mapdata.HoleData[n].XPos.ToString() + "," + mapdata.HoleData[n].YPos.ToString() + "," + mapdata.HoleData[n].Value.ToString());
                        }
                    }
                }
                writer.Close();

                m_Log.WriteLog(LogType.Datas, "Saved Successfully.");
                return true;
            }
            catch (Exception ex)
            {
                m_Log.WriteLog(LogType.Error, ex.Message);
                return false;
            }

        }
        public bool SaveCotourMapThicknessData(string sPath, int nLayerIndex, int nPointIndex, int nRepeatCount, int nRepeatIndex)
        {
            try
            {
                bool isRepeat = false;
                if (nRepeatCount == 1)
                {
                    isRepeat = false;
                }
                else
                {
                    isRepeat = true;
                }
                if (Path.GetExtension(sPath) != ".csv")
                {
                    sPath += ".csv";
                }

                StreamWriter writer = new StreamWriter(sPath);
                writer.WriteLine("X[mm],Y[mm],Value[Å]");
                string sData = string.Empty;
                string sThicknessData = string.Empty;
                if (isRepeat == false)
                {
                    for (int i = 0; i < nPointIndex; i++)
                    {
                        if (bThickness)
                        {
                            sThicknessData = (m_RawData[i].Thickness[nLayerIndex] * m_ThicknessData[nLayerIndex].m_dThicknessScale + m_ThicknessData[nLayerIndex].m_dThicknessOffset).ToString("0.####");
                            writer.WriteLine(m_RawData[i].dX.ToString() + "," + m_RawData[i].dY.ToString() + "," + sThicknessData);
                        }
                        else
                        {
                            sThicknessData = 0.ToString();
                            writer.WriteLine(m_RawData[i].dX.ToString() + "," + m_RawData[i].dY.ToString() + "," + sThicknessData);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < nPointIndex; i++)
                    {
                        if (i % nRepeatCount == nRepeatIndex)
                        {
                            if (bThickness)
                            {
                                sThicknessData = (m_RawData[i].Thickness[nLayerIndex] * m_ThicknessData[nLayerIndex].m_dThicknessScale + m_ThicknessData[nLayerIndex].m_dThicknessOffset).ToString("0.####");
                                writer.WriteLine(m_RawData[i].dX.ToString() + "," + m_RawData[i].dY.ToString() + "," + sThicknessData);
                            }
                            else
                            {
                                sThicknessData = Convert.ToString(0.0000);
                                writer.WriteLine(m_RawData[i].dX.ToString() + "," + m_RawData[i].dY.ToString() + "," + sThicknessData);
                            }
                        }
                    }
                }

                writer.Close();

                m_Log.WriteLog(LogType.Datas, "Saved Successfully.");
                return true;
            }
            catch (Exception ex)
            {
                m_Log.WriteLog(LogType.Error, ex.Message);
                return false;
            }
        }

        public bool SaveResultFileSummary(string sPath, string sLotID, string sSlotID, int nPointCount)
        {
            int n = 0;
            try
            {
                StreamWriter sw = new StreamWriter(sPath);
                string sHeader = "X_Ref,Y_Ref,root lot ID,SLOT ID,Site";
                foreach (WavelengthItem R in m_ScalesListR)
                {
                    sHeader += ",R_" + R.p_waveLength.ToString("0.####");
                }
                foreach (WavelengthItem T in m_ScalesListT)
                {
                    sHeader += ",T_" + T.p_waveLength.ToString("0.####");
                }
                sHeader += ",GOF";
                for (n = 1; n < m_LayerData.Count - 1; n++)// Recipe 설정 Model data (박막 물질)
                {
                    sHeader += ",";
                    for (int s = 0; s < m_LayerData[n].hostname.Length; s++)
                    {
                        sHeader += m_LayerData[n].hostname[s];
                    }

                }
                sw.WriteLine(sHeader);

                string sData;
                for (n = 0; n < nPointCount; n++)
                {
                    if (m_RawData[n].Wavelength.Length != 0)// 수정? .count 였는데 .length 사용
                    {
                        sData = string.Empty;// 수정? dX와 dY 가 데이터를 받도록 연결되어있는지 확인 필요
                        sData += m_RawData[n].dX.ToString("0.####") + "," + m_RawData[n].dY.ToString("0.####") + "," + sLotID + "," + sSlotID + "," + (n + 1).ToString();
                        for (int i = 0; i < m_ContourMapDataR.Count; i++)
                        {
                            sData += "," + m_ContourMapDataR[i].HoleData[n].Value.ToString("0.####");
                        }
                        for (int i = 0; i < m_ContourMapDataT.Count; i++)
                        {
                            sData += "," + m_ContourMapDataT[i].HoleData[n].Value.ToString("0.####");
                        }
                        sData += "," + m_RawData[n].dGoF.ToString("0.#####");

                        for (int i = 1; i < m_LayerData.Count - 1; i++)
                        {
                            if (bThickness)
                            {
                                sData += "," + (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset).ToString("0.####");
                            }
                            else
                            {
                                sData += "," + (0).ToString("0.####");
                            }
                        }
                        sw.WriteLine(sData);
                    }
                }
                sw.Close();
                m_Log.WriteLog(LogType.Datas, " SaveResultFileSummary()_Saved");
                return true;
            }
            catch (Exception ex)
            {
                // 수정? 다음고 같은 형식으로 메세지 출력해도 되는지 문의
                m_Log.WriteLog(LogType.Error, "SaveResultFileSummary() - Error / <" + n.ToString() + "> - " + ex.Message);// 로그 기록 어떻게 하는지 확인
                return false;
            }

        }
        public bool SaveResultFileSummary(string sPath, string sLotID, string sSlotID, int nPointCount, int nRepeatCount, int nRepeatIndex)
        {
            int n = 0;
            try
            {
                StreamWriter sw = new StreamWriter(sPath);
                string sHeader = "X_Ref,Y_Ref,root lot ID,SLOT ID,Site";
                foreach (WavelengthItem R in m_ScalesListR)
                {
                    sHeader += ",R_" + R.p_waveLength.ToString("0.####");
                }
                foreach (WavelengthItem T in m_ScalesListT)
                {
                    sHeader += ",T_" + T.p_waveLength.ToString("0.####");
                }
                sHeader += ",GOF";
                for (n = 1; n < m_LayerData.Count - 1; n++)// Recipe 설정 Model data (박막 물질)
                {
                    sHeader += ",";
                    
                        sHeader += string.Concat(m_LayerData[n].hostname);
                   
                }
                sw.WriteLine(sHeader);

                string sData;
                for (n = 0; n < nPointCount; n++)
                {
                    if (n % nRepeatCount == nRepeatIndex)
                    {
                        int m = n / nRepeatCount;

                        if (m_RawData[n].Wavelength.Length != 0)// 수정? .count 였는데 .length 사용
                        {
                            sData = string.Empty;// 수정? dX와 dY 가 데이터를 받도록 연결되어있는지 확인 필요
                            sData += m_RawData[n].dX.ToString("0.####") + "," + m_RawData[n].dY.ToString("0.####") + "," + sLotID + "," + sSlotID + "," + (m + 1).ToString();
                            for (int i = 0; i < m_ContourMapDataR.Count; i++)
                            {
                                sData += "," + m_ContourMapDataR[i].HoleData[n].Value.ToString("0.####");
                            }
                            for (int i = 0; i < m_ContourMapDataT.Count; i++)
                            {
                                sData += "," + m_ContourMapDataT[i].HoleData[n].Value.ToString("0.####");
                            }
                            sData += "," + m_RawData[n].dGoF.ToString("0.#####");

                            for (int i = 1; i < m_LayerData.Count  - 1; i++)
                            {
                                if (bThickness)
                                {
                                    sData += "," + (m_RawData[n].Thickness[i] * m_ThicknessData[i].m_dThicknessScale + m_ThicknessData[i].m_dThicknessOffset).ToString("0.####");
                                }
                                else
                                {
                                    sData += "," + (0).ToString("0.####");
                                }
                            }
                            sw.WriteLine(sData);
                        }
                    }
                }
                sw.Close();
                m_Log.WriteLog(LogType.Datas, " SaveResultFileSummary()_Saved");
                return true;
            }
            catch (Exception ex)
            {
                // 수정? 다음고 같은 형식으로 메세지 출력해도 되는지 문의
                m_Log.WriteLog(LogType.Error, "SaveResultFileSummary() - Error / <" + n.ToString() + "> - " + ex.Message);// 로그 기록 어떻게 하는지 확인
                return false;
            }
        }
        //21.06.08 추가
        public void ContourMapDataList(List<WavelengthItem> R, List<WavelengthItem> T, int measurePointCount)
        {
            m_ScalesListR = new List<WavelengthItem>(R.ToArray());
            m_ScalesListT = new List<WavelengthItem>(T.ToArray());
            m_ContourMapDataR.Clear();
            m_ContourMapDataT.Clear();
            double dWavelengthMAxR = 0;

            for (int i = 0; i < m_ScalesListR.Count; i++)
            {
                ContourMapData mapdata = new ContourMapData();
                mapdata.Wavelength = m_ScalesListR[i].p_waveLength;
                m_ContourMapDataR.Add(mapdata);
            }
            for (int i = 0; i < m_ScalesListT.Count; i++)
            {
                ContourMapData mapdata = new ContourMapData();
                mapdata.Wavelength = m_ScalesListT[i].p_waveLength;
                m_ContourMapDataT.Add(mapdata);
            }
        }
        public void AllContourMapDataFitting(List<WavelengthItem> R, List<WavelengthItem> T, int nPointIdx)
        {
            m_ScalesListR = new List<WavelengthItem>(R.ToArray());
            m_ScalesListT = new List<WavelengthItem>(T.ToArray());
            if (m_RawData[0].Wavelength[0] == 0)
                return;

            for (int n = 0; n < nPointIdx; n++)
            {
                int indexR = 0;
                bool isDoneR = false;
                for (int k = 0; k < m_RawData[n].nNIRDataNum; k++)
                {

                    double dRawDataWaveLength = Math.Round(m_RawData[n].Wavelength[k]);
                    if (!isDoneR && m_ScalesListR.Count != 0 && dRawDataWaveLength == m_ScalesListR[indexR].p_waveLength)
                    {
                        double dValue = m_RawData[n].Reflectance[k];
                        HoleData holedata = new HoleData();
                        holedata.XPos = m_RawData[n].dX;
                        holedata.YPos = m_RawData[n].dY;
                        dValue *= m_ScalesListR[indexR].p_scale;
                        dValue += m_ScalesListR[indexR].p_offset;
                        holedata.Value = dValue;
                        m_ContourMapDataR[indexR].HoleData.Add(holedata);

                        indexR++;
                    }

                    if (!isDoneR && indexR == m_ScalesListR.Count)
                    {
                        isDoneR = true;
                        break;
                    }
                }
                if (bTransmittance)
                {
                    for (int indexT = 0; indexT < m_ScalesListT.Count; indexT++)
                    {
                        if (m_RawData[n].DCOLTransmittance[indexT].Wavelength == m_ContourMapDataT[indexT].Wavelength)
                        {
                            double dValue = m_RawData[n].DCOLTransmittance[indexT].RawTransmittance;
                            HoleData holedata = new HoleData();
                            holedata.XPos = m_RawData[n].dX;
                            holedata.YPos = m_RawData[n].dY;
                            dValue *= m_ScalesListT[indexT].p_scale;
                            dValue += m_ScalesListT[indexT].p_offset;
                            holedata.Value = dValue;
                            m_ContourMapDataT[indexT].HoleData.Add(holedata);
                        }
                        else
                        {
                            double dValue = 0;
                            HoleData holedata = new HoleData();
                            holedata.XPos = m_RawData[n].dX;
                            holedata.YPos = m_RawData[n].dY;
                            dValue *= m_ScalesListT[indexT].p_scale;
                            dValue += m_ScalesListT[indexT].p_offset;
                            holedata.Value = dValue;
                            m_ContourMapDataT[indexT].HoleData.Add(holedata);
                        }
                    }

                }
                else
                {
                    for (int indexT = 0; indexT < m_ScalesListT.Count; indexT++)
                    {

                        double dValue = 0;
                        HoleData holedata = new HoleData();
                        holedata.XPos = m_RawData[n].dX;
                        holedata.YPos = m_RawData[n].dY;
                        dValue *= m_ScalesListT[indexT].p_scale;
                        dValue += m_ScalesListT[indexT].p_offset;
                        holedata.Value = dValue;
                        m_ContourMapDataT[indexT].HoleData.Add(holedata);
                    }
                }
            }
        }
        #endregion


        public void CalibrationBackCountSave()
        {
            try
            {
                if (BackGroundCalCountData[0] ==0)
                {
                    throw new Exception(" Data is not exist.");
                }
                string sPath = @"C:\Users\ATI\Desktop\MeasureData\SampleCalCountData";

                
                if (!Directory.Exists(sPath))
                {
                    Directory.CreateDirectory(sPath);
                }
                string sFileName = sPath;
                sFileName += "\\";
                sFileName += DateTime.Now.ToString("yyyyMMdd");
                sFileName += "_";
                sFileName += DateTime.Now.ToString("HH-mm-ss");
                sPath = sFileName;
                if (Path.GetExtension(sPath) != ".csv")
                    sPath += ".csv";
                StreamWriter sw = new StreamWriter(sPath);
                //StreamWriter sw = new StreamWriter(sFileName);
                
                sw.WriteLine("Wavelength[nm],CountData");
                for (int n = 0; n <425 ; n++)
                {
                    sw.WriteLine("{0},{1}", BackGroundCalWavelength[n], BackGroundCalCountData[n]);
                }
                sw.Close();

                m_Log.WriteLog(LogType.Datas,"SampleCal: BackGraoung Count data saved.");
            }
            catch
            {

            }
        }
    }
    public class ThicknessScaleOffset
    {
        public double m_dThicknessScale { get; set; } = 0.0;
        public double m_dThicknessOffset { get; set; } = 0.0;

        public ThicknessScaleOffset()
        {

        }
        public ThicknessScaleOffset(double scale, double offset)
        {
            m_dThicknessScale = scale;
            m_dThicknessOffset = offset;
        }
    }
}
