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
        }
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
            //HoleData data = source

            HoleData data = new HoleData();
            data.XPos = XPos;
            data.YPos = YPos;
            data.Value = Value;

            return data;
        }
    }

    ///<summary>
    ///contourMap 그릴 때의 데이터 & 결과파일에도 써줌
    ///</summary>
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
        public bool bExcept_NIR = false;
        public bool bThickness = true;
        public bool bTransmittance = true;
        public bool bViewCalRGraph = true;
        public int nThicknessDataNum = 0;
        public float nStartWavelegth = 0;
        public double[] BackGroundCalWavelength;
        public double [] BackGroundCalCountData;
        public SettingData m_SettngData;


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

            m_SettngData = new SettingData();

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

        #region 추후 제거 Calassistant 추가 대상

        public bool LoadNKDatas(string sPath)   //folder path
        {
            if (string.IsNullOrEmpty(sPath))
            {
                System.Windows.Forms.MessageBox.Show("nk 파일 경로가 입력되지 않았습니다.", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
            if (!Directory.Exists(sPath))
            {
                System.Windows.Forms.MessageBox.Show("해당 nk 파일 경로가 없습니다. - " + sPath, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
            if (m_LayerData.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("LayerData가 없습니다.", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }

            for (int n = 0; n < m_LayerData.Count; n++)
            {
                m_LayerData[n].wavelength.Clear();
                m_LayerData[n].n.Clear();
                m_LayerData[n].k.Clear();
            }

            string sFilePath = string.Empty;
            for (int n = 0; n < m_LayerData.Count; n++)
            {
                sFilePath = sPath + @"\" + m_LayerData[n].sRefName + ".csv";
                if (!File.Exists(sFilePath))
                {
                    System.Windows.Forms.MessageBox.Show("nk 파일이 없습니다. - " + sFilePath, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
                LoadNKFile(sFilePath, n);
            }
            return true;
        }

        public bool LoadNKFile(string sPath, int nLayerIdx)
        {
            if (m_LayerData.Count <= nLayerIdx)
            {
                return false;
            }

            string strLine = string.Empty;
            string[] keys = null;

            StreamReader sr = new StreamReader(sPath);
            while ((strLine = sr.ReadLine()) != null)
            {
                keys = strLine.Split(',');
                if (keys.Length != 3 || keys[0] == string.Empty || keys[1] == string.Empty || keys[2] == string.Empty)
                {
                    System.Windows.Forms.MessageBox.Show("nk 파일이 좀 이상합니다. - " + sPath, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
                double dTmp = 0.0;
                if (double.TryParse(keys[0], out dTmp) == false)
                {
                    continue;
                }

                double dWavelength = Convert.ToDouble(keys[0]);
                double dN = Convert.ToDouble(keys[1]);
                double dK = Convert.ToDouble(keys[2]);
                m_LayerData[nLayerIdx].wavelength.Add(dWavelength);
                m_LayerData[nLayerIdx].n.Add(dN);
                m_LayerData[nLayerIdx].k.Add(dK);
            }
            sr.Close();
            return true;
        }

        public bool LoadReflectanceData(string sPath)
        {
            m_Wavelength.Clear();
            m_Reflectance.Clear();

            bool bData = false;
            StreamReader sr = new StreamReader(sPath);
            while (!sr.EndOfStream)
            {
                string sTemp = sr.ReadLine();
                string[] keys = sTemp.Split(',');

                if (bData)
                {
                    m_Wavelength.Add(Convert.ToDouble(keys[0]));
                    m_Reflectance.Add(Convert.ToDouble(keys[1]));
                }
                if (keys[0].Contains("Wavelength") || keys[0].Contains("wavelength"))
                {
                    bData = true;
                }

                /* 
                if (keys.Length != 2)
                {
                    System.Windows.Forms.MessageBox.Show("reflectance 파일이 좀 이상합니다. - " + sPath, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
                */

            }
            sr.Close();

            return true;
        }

        //DS 2021.01.05 반사율 계싼에 필요한 InitThickness 
        public void AddLayerData(string sName, double dThickness, bool bFix, double dTHKRange = 0.0, bool bUseTargetTHK = false, double dTargetThickness = 0.0)
        {
            LayerData layerdata = new LayerData();
            layerdata.sRefName = sName;
            layerdata.dInitThickness = dThickness;
            layerdata.bFix = bFix;
            layerdata.dTHKRangeRate = dTHKRange;
            layerdata.bUseTargetTHK = bUseTargetTHK;
            layerdata.dTargetThickness = dTargetThickness;
            m_LayerData.Add(layerdata);
        }
        #endregion 
        public bool ClearRawData()
        {
            try
            {
                for (int n = 0; n < ConstValue.RAWDATA_POINT_MAX_SIZE; n++)
                {
                    m_RawData[n].bDataExist = false;
                    m_RawData[n].Wavelength = Enumerable.Repeat(0.0, ConstValue.SPECTROMETER_MAX_PIXELSIZE).ToArray();
                    //m_RawData[n].Reflectance = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    m_RawData[n].Reflectance = Enumerable.Repeat(0.0, ConstValue.SPECTROMETER_MAX_PIXELSIZE).ToArray();
                    //m_RawData[n].VIS_Reflectance = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    m_RawData[n].VIS_Reflectance = Enumerable.Repeat(0.0, ConstValue.SPECTROMETER_MAX_PIXELSIZE).ToArray();
                    //m_RawData[n].VIS_Wavelength = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    m_RawData[n].VIS_Wavelength = Enumerable.Repeat(0.0, ConstValue.SPECTROMETER_MAX_PIXELSIZE).ToArray();
                    //m_RawData[n].Transmittance = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    m_RawData[n].Transmittance = Enumerable.Repeat(0.0, ConstValue.SPECTROMETER_MAX_PIXELSIZE).ToArray();
                    //m_RawData[n].CalcReflectance = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    m_RawData[n].CalcReflectance = Enumerable.Repeat(0.0, ConstValue.SPECTROMETER_MAX_PIXELSIZE).ToArray();
                    //m_RawData[n].eV = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    m_RawData[n].eV = Enumerable.Repeat(0.0, ConstValue.SPECTROMETER_MAX_PIXELSIZE).ToArray();
                    m_RawData[n].Thickness.Clear();
                    m_RawData[n].dX = 0.0;
                    m_RawData[n].dY = 0.0;
                    m_RawData[n].dGoF = 0.0;
                    m_RawData[n].nCalcDataNum = 0;
                    m_RawData[n].nNIRDataNum = 0;
                    
    }

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
            StreamWriter writer = new StreamWriter(sPath);
            RawData raw = m_RawData[nPointIdx];
            if (writer == null)
            {
                return false;
            }
            writer.WriteLine("GoF," + Math.Round(raw.dGoF, 6).ToString());
            for (int n = 0; n < raw.Thickness.Count; n++)
            {
                writer.WriteLine(m_LayerData[n].sRefName + "," + raw.Thickness[n].ToString());
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

        public bool SaveResultFileDCOL(string sPath, string sFoupID, string sLotID, string sToolID, string sWaferID, string sSlotID, string sSWVersion, string sRCPName)
        {
            try
            {
                string[] sWaferNum = sWaferID.Split('.');
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
                //sw.WriteLine("WAFER #," + sWaferNum[1]);
                sw.WriteLine("SLOT," + sSlotID);
                sw.WriteLine("WAFER STATUS," + "Pass");
                sw.WriteLine("DATA TYPE," + "TF");
                sw.WriteLine("RECIPE," + sRCPName);
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();


                //////////////////////////////////////////Data////////////////////////
                string sHeader = "RESULT TYPE,NGOF";
                for (int n = 0; n < m_LayerData.Count - 1; n++)
                {
                    sHeader += "," + m_LayerData[n].hostname;
                }
                sw.WriteLine(sHeader);

                int nTHKNum = m_LayerData.Count - 1;

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
                for (int n = 0; n < m_RawData.Length; n++)
                {
                    double dGoF = m_RawData[n].dGoF;
                    if (m_RawData[n].Wavelength.Count() != 0)//수정? .Count 에서 .Count() 으로 수정함
                    {
                        for (int i = 0; i < nTHKNum; i++)
                        {
                            if (StddevTmp[i] == null)
                                StddevTmp[i] = new List<double>();

                            double dThickness = m_RawData[n].Thickness[i];
                            dMeanThickness[i] += dThickness;
                            StddevTmp[i].Add(dThickness);

                            if (dThickness < dMinThickness[i])
                            {
                                dMinThickness[i] = dThickness;
                            }
                            if (dThickness > dMaxThickness[i])
                            {
                                dMaxThickness[i] = dThickness;
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
                sDataStddev += "," + dStddevGoF.ToString("0.####");
                sData3Sigma += "," + d3SigmaGoF.ToString("0.####");
                sDataRange += "," + (dMaxGoF - dMinGoF).ToString("0.####");

                for (int i = 0; i < nTHKNum; i++)
                {
                    sDataMean += "," + dMeanThickness[i].ToString("0.####");
                    sDataMin += "," + dMinThickness[i].ToString("0.####");
                    sDataMax += "," + dMaxThickness[i].ToString("0.####");
                    sDataStddev += "," + dStddevThickness[i].ToString("0.####");
                    sData3Sigma += "," + d3SigmaThickness[i].ToString("0.####");
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
                for (int n = 0; n < m_RawData.Length; n++)
                {
                    if (m_RawData[n].Wavelength.Count() != 0)
                    {
                        sData = (n + 1).ToString();
                        double dThicknessSum = 0;
                        for (int i = 0; i < nTHKNum; i++)
                        {
                            sData += "," + m_RawData[n].Thickness[i].ToString("0.####");
                            dThicknessSum += m_RawData[n].Thickness[i];
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

                for (int n = 0; n < m_RawData.Length; n++)
                {
                    if (m_RawData[n].Wavelength.Count() != 0)
                    {
                        sData = string.Empty;
                        sData += m_RawData[n].dX.ToString("0.####") + "," + m_RawData[n].dY.ToString("0.####") + "," + sLotID + "," + sSlotID + "," + (n + 1).ToString();
                        for (int i = 0; i < m_ContourMapDataR.Count; i++)
                        {
                            sData += "," + m_ContourMapDataR[i].HoleData[n].Value.ToString("0.####");
                        }
                        sw.WriteLine(sData);
                        for (int i = 0; i < m_ContourMapDataT.Count; i++)
                        {
                            sData += "," + m_ContourMapDataT[i].HoleData[n].Value.ToString("0.####");
                        }
                        sw.WriteLine(sData);
                    }
                }
                sw.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SaveResultFileLot(string sPath, string sFoupID, string sLotID, string sToolID, string sWaferID, string sSlotID, string sSWVersion, string sRCPName)
        {
            try
            {
                bool bFirst = true;
                if (File.Exists(sPath))
                {
                    bFirst = false;
                }
                string[] sWaferNum = sWaferID.Split('.');
                StreamWriter sw = new StreamWriter(sPath, true);

                if (bFirst == true)
                {
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
                }
                sw.WriteLine("WAFER ID," + sWaferID);
                sw.WriteLine("LOT ID," + sFoupID + "_" + sLotID);
                sw.WriteLine("WAFER #," + sWaferNum[1]);
                sw.WriteLine("SLOT," + sSlotID);
                sw.WriteLine("WAFER STATUS," + "Pass");
                sw.WriteLine("DATA TYPE," + "TF");
                sw.WriteLine("RECIPE," + sRCPName);
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();

                string sHeader = "RESULT TYPE,NGOF,Sum";
                for (int n = 0; n < m_LayerData.Count - 1; n++)
                {
                    sHeader += "," + m_LayerData[n].hostname;
                }
                sw.WriteLine(sHeader);

                /////////////////Data////////////////////
                int nTHKNum = m_LayerData.Count - 1;

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
                for (int n = 0; n < m_RawData.Length; n++)
                {
                    double dGoF = m_RawData[n].dGoF;
                    if (m_RawData[n].Wavelength.Length != 0)
                    {
                        for (int i = 0; i < nTHKNum; i++)
                        {
                            if (StddevTmp[i] == null)
                                StddevTmp[i] = new List<double>();

                            double dThickness = m_RawData[n].Thickness[i];
                            dMeanThickness[i] += dThickness;
                            StddevTmp[i].Add(dThickness);

                            if (dThickness < dMinThickness[i])
                            {
                                dMinThickness[i] = dThickness;
                            }
                            if (dThickness > dMaxThickness[i])
                            {
                                dMaxThickness[i] = dThickness;
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
                sDataStddev += "," + dStddevGoF.ToString("0.####") + "," + dStddevTHKSum.ToString("0.####");
                sData3Sigma += "," + d3SigmaGoF.ToString("0.####") + "," + d3SigmaTHKSum.ToString("0.####");
                sDataRange += "," + (dMaxGoF - dMinGoF).ToString("0.####") + "," + dRangeTHKSum.ToString("0.####");

                for (int i = 0; i < nTHKNum; i++)
                {
                    sDataMean += "," + dMeanThickness[i].ToString("0.####");
                    sDataMin += "," + dMinThickness[i].ToString("0.####");
                    sDataMax += "," + dMaxThickness[i].ToString("0.####");
                    sDataStddev += "," + dStddevThickness[i].ToString("0.####");
                    sData3Sigma += "," + d3SigmaThickness[i].ToString("0.####");
                    sDataRange += "," + (dMaxThickness[i] - dMinThickness[i]).ToString("0.####");
                }

                sw.WriteLine(sDataMean);
                sw.WriteLine(sDataMin);
                sw.WriteLine(sDataMax);
                sw.WriteLine(sDataStddev);
                sw.WriteLine(sData3Sigma);
                sw.WriteLine(sDataRange);

                sHeader = "Site #";
                for (int n = 0; n < m_LayerData.Count - 1; n++)
                {
                    sHeader += "," + m_LayerData[n].sRefName;
                }
                sHeader += ",NGOF,Sum,X,Y,OFFSET X,OFFSET Y";
                sw.WriteLine(sHeader);

                string sData = string.Empty;
                for (int n = 0; n < m_RawData.Length; n++)
                {
                    if (m_RawData[n].Wavelength.Length!= 0)
                        // 수정? Save 파일에 저장되는 파장 데이터들이 350~1500nm 범위 배열인지 확인할것
                        // 또한 저장되도록 지정된 파장 배열 범위가 350~1500nm 인지 확인할 것
                    {
                        sData = (n + 1).ToString();
                        double dThicknessSum = 0;
                        for (int i = 0; i < nTHKNum; i++)
                        {
                            sData += "," + m_RawData[n].Thickness[i].ToString("0.####");
                            dThicknessSum += m_RawData[n].Thickness[i];
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
                for (int n = 1; n < m_LayerData.Count -1; n++)
                {
                    sHeader += ",";
                    for (int s = 0; s < m_LayerData[n].hostname.Length; s++)
                    {
                        sHeader += m_LayerData[n].hostname[s];
                    }

                }
                sw.WriteLine(sHeader);

                for (int n = 0; n < m_RawData.Length; n++)
                {
                    if (m_RawData[n].Wavelength.Length != 0)
                    {
                        sData = string.Empty;
                        sData += m_RawData[n].dX.ToString("0.####") + "," + m_RawData[n].dY.ToString("0.####") + "," + sLotID + "," + sSlotID + "," + (n + 1).ToString();
                        for (int i = 0; i < m_ContourMapDataR.Count; i++)
                        {
                            sData += "," + m_ContourMapDataR[i].HoleData[n].Value.ToString("0.####");
                        }
                        for (int i = 0; i < m_ContourMapDataT.Count; i++)
                        {
                            sData += "," + m_ContourMapDataT[i].HoleData[n].Value.ToString("0.####");
                        }
                        sData += "," + m_RawData[n].dGoF.ToString("0.####");
                        for (int i = 1; i < m_RawData[n].Thickness.Count-1; i++)
                        {
                            // Si 기판, Air 제외
                            sData += "," + m_RawData[n].Thickness[i].ToString("0.####");
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
                m_Log.WriteLog(LogType.Error, "SaveResultFileLot() - Error" );
                return false;
            }
        }
        public bool SaveResultFileLot(string sPath, string sFoupID, string sLotID, string sToolID, string sWaferID, string sSlotID, string sSWVersion, string sRCPName, int nRepeatCount, int nRepeatIndex)
        {
            try
            {
                bool bFirst = true;
                if (File.Exists(sPath))
                {
                    bFirst = false;
                }
                string[] sWaferNum = sWaferID.Split('.');
                StreamWriter sw = new StreamWriter(sPath, true);

                if (bFirst == true)
                {
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
                }
                sw.WriteLine("WAFER ID," + sWaferID);
                sw.WriteLine("LOT ID," + sFoupID + "_" + sLotID);
                sw.WriteLine("WAFER #," + sWaferNum[1]);
                sw.WriteLine("SLOT," + sSlotID);
                sw.WriteLine("WAFER STATUS," + "Pass");
                sw.WriteLine("DATA TYPE," + "TF");
                sw.WriteLine("RECIPE," + sRCPName);
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();

                string sHeader = "RESULT TYPE,NGOF,Sum";
                for (int n = 0; n < m_LayerData.Count - 1; n++)
                {
                    sHeader += "," + m_LayerData[n].hostname;
                }
                sw.WriteLine(sHeader);

                /////////////////Data////////////////////
                int nTHKNum = m_LayerData.Count - 1;

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
                for (int n = 0; n < m_RawData.Length; n++)
                {
                    if (n % nRepeatCount == nRepeatIndex)
                    {
                        double dGoF = m_RawData[n].dGoF;
                        if (m_RawData[n].Wavelength.Length != 0)
                        {
                            for (int i = 0; i < nTHKNum; i++)
                            {
                                if (StddevTmp[i] == null)
                                    StddevTmp[i] = new List<double>();

                                double dThickness = m_RawData[n].Thickness[i];
                                dMeanThickness[i] += dThickness;
                                StddevTmp[i].Add(dThickness);

                                if (dThickness < dMinThickness[i])
                                {
                                    dMinThickness[i] = dThickness;
                                }
                                if (dThickness > dMaxThickness[i])
                                {
                                    dMaxThickness[i] = dThickness;
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
                sDataStddev += "," + dStddevGoF.ToString("0.####") + "," + dStddevTHKSum.ToString("0.####");
                sData3Sigma += "," + d3SigmaGoF.ToString("0.####") + "," + d3SigmaTHKSum.ToString("0.####");
                sDataRange += "," + (dMaxGoF - dMinGoF).ToString("0.####") + "," + dRangeTHKSum.ToString("0.####");

                for (int i = 0; i < nTHKNum; i++)
                {
                    sDataMean += "," + dMeanThickness[i].ToString("0.####");
                    sDataMin += "," + dMinThickness[i].ToString("0.####");
                    sDataMax += "," + dMaxThickness[i].ToString("0.####");
                    sDataStddev += "," + dStddevThickness[i].ToString("0.####");
                    sData3Sigma += "," + d3SigmaThickness[i].ToString("0.####");
                    sDataRange += "," + (dMaxThickness[i] - dMinThickness[i]).ToString("0.####");
                }

                sw.WriteLine(sDataMean);
                sw.WriteLine(sDataMin);
                sw.WriteLine(sDataMax);
                sw.WriteLine(sDataStddev);
                sw.WriteLine(sData3Sigma);
                sw.WriteLine(sDataRange);

                sHeader = "Site #";
                for (int n = 0; n < m_LayerData.Count - 1; n++)
                {
                    sHeader += "," + m_LayerData[n].hostname;
                }
                sHeader += ",NGOF,Sum,X,Y,OFFSET X,OFFSET Y";
                sw.WriteLine(sHeader);

                string sData = string.Empty;
                for (int n = 0; n < m_RawData.Length; n++)
                {
                    if (n % nRepeatCount == nRepeatIndex)
                    {
                        int m = n / nRepeatCount;
                        if (m_RawData[n].Wavelength.Length != 0)
                        {
                            sData = (m + 1).ToString();
                            double dThicknessSum = 0;
                            for (int i = 0; i < nTHKNum; i++)
                            {
                                sData += "," + m_RawData[n].Thickness[i].ToString("0.####");
                                dThicknessSum += m_RawData[n].Thickness[i];
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

                for (int n = 0; n < m_RawData.Length; n++)
                {
                    if (n % nRepeatCount == nRepeatIndex)
                    {
                        int m = n / nRepeatCount;
                        if (m_RawData[n].Wavelength.Length != 0)
                        {
                            sData = string.Empty;
                            sData += m_RawData[n].dX.ToString("0.####") + "," + m_RawData[n].dY.ToString("0.####") + "," + sLotID + "," + sSlotID + "," + (m + 1).ToString();
                            for (int i = 0; i < m_ContourMapDataR.Count; i++)
                            {
                                sData += "," + m_ContourMapDataR[i].HoleData[n].Value.ToString("0.####");
                            }
                            for (int i = 0; i < m_ContourMapDataT.Count; i++)
                            {
                                sData += "," + m_ContourMapDataT[i].HoleData[n].Value.ToString("0.####");
                            }
                            sData += "," + m_RawData[n].dGoF.ToString("0.####");
                            for (int i = 1; i < m_RawData[n].Thickness.Count-1; i++)
                            {
                                sData += "," + m_RawData[n].Thickness[i].ToString("0.####");
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

        public bool SaveResultFileSlot(string sPath, InfoWafer infoWafer, RecipeDataManager recipeData, int nPointIdx)
        {
            if (Path.GetExtension(sPath) != ".csv")
            {
                sPath += ".csv";
            }

            int n = 0;
            try
            {
                string[] sWaferNum = infoWafer.p_sWaferID.Split('.');
                RawData raw = m_RawData[nPointIdx];

                StreamWriter sw = new StreamWriter(sPath);
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
                if(sWaferNum.Length > 1)
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
                sw.WriteLine("X_Position," + recipeData.MeasurementRD.DataSelectedPoint[recipeData.MeasurementRD.DataMeasurementRoute[nPointIdx]].x.ToString("F3"));
                sw.WriteLine("Y_Position," + recipeData.MeasurementRD.DataSelectedPoint[recipeData.MeasurementRD.DataMeasurementRoute[nPointIdx]].y.ToString("F3"));
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
            catch(Exception ex)
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
                    for (n = 0 ; n < raw.nNIRDataNum; n++)
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
        public bool SaveContourMapData(string sPath, List<ContourMapData> mapdata, bool bUseMinusData = false)
        {
            // ContourMap 완성된 이후 작업하겠습니다.
            return true;
        }

        public bool SaveResultFileSummary(string sPath, string sLotID, string sSlotID)
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
                    for ( int s = 0; s < m_LayerData[n].hostname.Length; s++ )
                    {
                        sHeader += m_LayerData[n].hostname[s];
                    }
                    
                }
                sw.WriteLine(sHeader);

                string sData;
                for (n = 0; n < m_RawData.Length; n++)
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
                        sData += "," + m_RawData[n].dGoF.ToString("0.####");

                        for (int i = 1; i < m_RawData[n].Thickness.Count-1; i++)
                        {
                            sData += "," + m_RawData[n].Thickness[i].ToString("0.####");
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

        public bool SaveResultFileSummary(string sPath, string sLotID, string sSlotID, int nRepeatCount, int nRepeatIndex)
        {
            int n = 0;
            try
            {
                //AllContourMapDataFitting();

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
                for (n = 1; n < m_LayerData.Count - 1; n++)
                {
                    sHeader += ",";
                    for (int s = 0; s < m_LayerData[n].hostname.Length; s++)
                    {
                        sHeader += m_LayerData[n].hostname[s];
                    }

                }
                sw.WriteLine(sHeader);

                string sData;
                for (n = 0; n < m_RawData.Length; n++)
                {
                    if (n % nRepeatCount == nRepeatIndex)
                    {
                        int m = n / nRepeatCount;

                        if (m_RawData[n].Wavelength.Length != 0)// 수정? .count 였는데 .length 사용
                        {
                            sData = string.Empty;
                            sData += m_RawData[n].dX.ToString("0.####") + "," + m_RawData[n].dY.ToString("0.####") + "," + sLotID + "," + sSlotID + "," + (m + 1).ToString();
                            for (int i = 0; i < m_ContourMapDataR.Count; i++)
                            {
                                sData += "," + m_ContourMapDataR[i].HoleData[n].Value.ToString("0.####");
                            }
                            for (int i = 0; i < m_ContourMapDataT.Count; i++)
                            {
                                sData += "," + m_ContourMapDataT[i].HoleData[n].Value.ToString("0.####");
                            }
                            sData += "," + m_RawData[n].dGoF.ToString("0.####");

                            for (int i = 1; i < m_RawData[n].Thickness.Count-1; i++)
                            {
                                sData += "," + m_RawData[n].Thickness[i].ToString("0.####");
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
                m_Log.WriteLog(LogType.Error, "SaveResultFileSummary() - Error / <" + n.ToString() + "> - " + ex.Message);
                return false;
            }
        }

        public void AllContourMapDataFitting(List<WavelengthItem> R, List<WavelengthItem> T, int measurePointCount)
        {
            m_ScalesListR = new List<WavelengthItem>(R.ToArray());
            m_ScalesListT = new List<WavelengthItem>(T.ToArray());
            if (m_RawData[0].Wavelength[0] == 0)
                return;

            m_ContourMapDataR.Clear();
            m_ContourMapDataT.Clear();

            for(int i = 0; i < m_ScalesListR.Count; i++)
            {
                ContourMapData mapdata = new ContourMapData();
                mapdata.Wavelength = m_ScalesListR[i].p_waveLength;
                m_ContourMapDataR.Add(mapdata);
            }
            for(int i = 0; i < m_ScalesListT.Count; i++)
            {
                ContourMapData mapdata = new ContourMapData();
                mapdata.Wavelength = m_ScalesListT[i].p_waveLength;
                m_ContourMapDataT.Add(mapdata);
            }


            for (int n = 0; n < measurePointCount; n++)
            {
                int indexR = 0;
                int indexT = 0;
                bool isDoneR = false;
                bool isDoneT = false;
                for (int k = 0; k < m_RawData[n].nNIRDataNum; k++)
                {

                    double dRawDataWaveLength = m_RawData[n].Wavelength[k];
                    if (m_ScalesListR.Count != 0 && dRawDataWaveLength == m_ScalesListR[indexR].p_waveLength && !isDoneR)
                    {
                        double dValue = m_RawData[n].Reflectance[k];
                        //double dWaveLength = m_ScalesListR[indexR].p_waveLength;
                        HoleData holedata = new HoleData();
                        holedata.XPos = m_RawData[n].dX;
                        holedata.YPos = m_RawData[n].dY;
                        dValue *= m_ScalesListR[indexR].p_scale;  //190627
                        dValue += m_ScalesListR[indexR].p_offset; //190627
                        holedata.Value = dValue;
                        m_ContourMapDataR[indexR].HoleData.Add(holedata);                    

                        indexR++;
                    }

                    if(indexR == m_ScalesListR.Count)
                    {
                        isDoneR = true;
                    }

                    if(m_ScalesListT.Count != 0 && dRawDataWaveLength == m_ScalesListT[indexT].p_waveLength && !isDoneT)
                    {
                        double dValue = m_RawData[n].Reflectance[k];
                        //double dWaveLength = m_ScalesListT[indexT].p_waveLength;
                        HoleData holedata = new HoleData();
                        holedata.XPos = m_RawData[n].dX;
                        holedata.YPos = m_RawData[n].dY;
                        dValue *= m_ScalesListT[indexT].p_scale;  //190627
                        dValue += m_ScalesListT[indexT].p_offset; //190627
                        holedata.Value = dValue;
                        m_ContourMapDataT[indexT].HoleData.Add(holedata);



                        indexT++;
                    }

                    if (indexT == m_ScalesListT.Count)
                    {
                        isDoneT = true;
                    }

                    if (isDoneR && isDoneT)
                    {
                        break;
                    }
                }
            }


            //for (int n = 0; n < m_RawData.Length; n++)
            //{
            //    int index = 0;
            //    for (int k = 0; k < m_RawData[n].nNIRDataNum; k++)
            //    {
            //        double dValue = m_RawData[n].Transmittance[k];
            //        double dWaveLength = m_ScalesListT[index].p_waveLength;
            //        if (dValue == m_ScalesListT[index].p_waveLength)
            //        {


            //            HoleData holedata = new HoleData();
            //            holedata.XPos = m_RawData[n].dX;
            //            holedata.YPos = m_RawData[n].dY;
            //            dValue *= m_ScalesListT[index].p_scale;  //190627
            //            dValue += m_ScalesListT[index].p_offset; //190627
            //            holedata.Value = dValue;
            //            m_ContourMapData[m_ScalesListT.Count].HoleData.Add(holedata);

            //            ContourMapData mapdata = new ContourMapData();
            //            mapdata.Valuetype = FitValueType.Transmittance;
            //            mapdata.Wavelength = dWaveLength;
            //            m_ContourMapData.Add(mapdata);
            //        }
            //    }
            //}
              
        }
        #endregion
        //public MainWindow_ViewModel mwvm;
        //Test  
        //PMDatas m_PMData = new PMDatas();
        //public void LampCheck()
        //{
        //    PMDatas m_PMData = new PMDatas();
            
        //    string sLampT = m_PMData.UpdateSR(false,"4");
        //    string sLampH = m_PMData.UpdateSR(false, "3");
        //    string sVISLampLux = m_PMData.UpdateSR(false, "0");
        //    string sIRLampLux = m_PMData.UpdateSR(false, "1");
        //    string sControlT = m_PMData.UpdateSR(false, "6");
        //    string sControlH = m_PMData.UpdateSR(false, "5");

        //    string sPath = @"C:\Users\ATI\Desktop\MeasureData\SRData";


        //    if (!Directory.Exists(sPath))
        //    {
        //        Directory.CreateDirectory(sPath);
        //    }
        //    string sFileName = sPath;
        //    //sFileName += "\\";
        //    //sFileName += DateTime.Now.ToString("yyyyMMdd");
        //    sPath = sFileName;
        //    if (Path.GetExtension(sPath) != ".txt")
        //        sPath += ".txt";
        //    StreamWriter writer;
        //    writer = File.AppendText(sPath);
        //    writer.WriteLine("[" + DateTime.Now.ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("HH-mm-ss") + "]" + "," + "Lamp T" + "," + sLampT + "," + "Lamp H" + "," + sLampH + "," + "Control T" + "," + sControlT + "," + "Control H" + "," + sControlH + "," + "VISLampLux" + "," + sVISLampLux + "," + "IRLampLux" + "," + sIRLampLux);
        //    writer.Close();


        //}

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
}
