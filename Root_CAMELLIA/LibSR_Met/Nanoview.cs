using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NanoView;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO.Ports;
using MathNet.Numerics.Interpolation;
using static Root_CAMELLIA.Module.Module_Camellia;
using Root_CAMELLIA;

namespace Root_CAMELLIA.LibSR_Met
{
    public partial class Nanoview
    {
        public enum ERRORCODE_NANOVIEW
        {
            SR_NO_ERROR = 0,
            SR_DO_HW_INITIALIZE_FIRST = -1,
            SR_CANNOT_FIND_DONGLE = -2,
            SR_SERIAL_PORT_NOT_OPENED = 1,
            SR_SPECTROMETER_NOT_FOUND = 2,// 반사율 함수에서 반사율 데이터 NaN출력되어 투과율 함수에서 에러코드 출력됨
            SR_SYSTEM_ERROR = 3,
            SR_REFERENCE_FILE_NOT_FOUND = 4,
            SR_LAYER_MODEL_NOT_READY = 11,
            SR_INVALID_NUMBER_OF_DATA = 12,
            SR_INVALID_FIT_LAYER = 13,
            SR_MATERIAL_FILE_ERROR = 14,
            SR_MATERIAL_FILE_LOAD_ERROR = 15,
            SR_MODELING_FAIL = 16,
            SR_SHUTTER_MOTION_ERROR = 17,

            REFLECTANCE_DATA_NOT_FOUND = 100,
            ATI_CAL_TRANSMITTANCE_FAIL = -7777,
            ATI_PARAMETER_ERROR = -8888,
            NANOVIEW_ERROR = -9999
        }

        public ARCNIR m_SR = null;
        Model m_Model = null;
        Model m_ModelSave = null;
        DataManager m_DM = DataManager.GetInstance();
        Calculation m_Calculation = new Calculation();
        public PMDatas m_PMDatas = new PMDatas();

        public MaterialList m_MaterialList;
        public LayerList m_LayerList;
        public MaterialList m_MaterialListSave;
        public LayerList m_LayerListSave;
        string m_sConfigPath = string.Empty;
        public bool isCalDCOLTransmittance = false;
        public bool isExceptNIR = false;
        public bool bCheckSampleCal = false;
        //double[] m_Spectrum;
        private bool m_bSRInitialized = false;
        private bool m_bPreLampOn = false;
        private bool m_bPreLaserOn = false;

        public Nanoview()
        {
            m_SR = new ARCNIR();
            m_Model = new Model();
            m_ModelSave = new Model();
            m_MaterialList = m_Model.m_MaterialList;
            m_LayerList = m_Model.m_LayerList;
            m_MaterialListSave = m_ModelSave.m_MaterialList;
            m_LayerListSave = m_ModelSave.m_LayerList;

            InitLamp();
        }



        [DllImport("kernel32")]

        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]

        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal,

                                                        int size, string filePath);

        ///<summary>
        ///기기 초기화 (program 부팅시 초기화 필요)
        ///</summary>
        public ERRORCODE_NANOVIEW InitializeSR(string nConfigureFilePath, int nPortnumber)
        {
            //초기화파일(*.cfg)과 시리얼 포트 번호를 지정하여 초기화
            try
            {
                ERRORCODE_NANOVIEW rst = (ERRORCODE_NANOVIEW)m_SR.Initialize(nConfigureFilePath, nPortnumber);

                if (rst == ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    m_sConfigPath = nConfigureFilePath;
                    m_bSRInitialized = true;
                    m_DM.m_Log.WriteLog(LogType.Operating, "SR Initialize Done");
                    SplashScreenHelper.ShowText("SR Initialize Done!");
                    //MessageBox.Show("Initialize Done"); //추후 제거

                    //double Boxcar_VIS = m_SR.Boxcar_VIS;
                    //double Average_VIS = m_SR.Average_VIS;
                    //double IntTime_VIS = m_SR.IntTime_VIS;
                    //double BackIntTime_VIS = m_SR.BackIntTime_VIS;

                    //double Boxcar_NIR = m_SR.Boxcar_NIR;
                    //double Average_NIR = m_SR.Average_NIR;
                    //double IntTime_NIR = m_SR.IntTime_NIR;
                    //double BackIntTime_NIR = m_SR.BackIntTime_NIR;

                }
                else
                {
                    string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), rst);
                    m_DM.m_Log.WriteLog(LogType.Error, sErr);
                    SplashScreenHelper.ShowText("SR Initialize Error!");
                    //MessageBox.Show(sErr); //추후 제거
                }

                return rst;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                //MessageBox.Show(ex.Message);
                SplashScreenHelper.ShowText("SR Initialize Error!");
                return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
            }
        }

        //public ERRORCODE_NANOVIEW ModellingFileLoad(string RecipeFilePath)
        //{
        //    //분석 모델링 파일 (*.erm) 불러오기
        //    try
        //    {
        //        ERRORCODE_NANOVIEW rst = (ERRORCODE_NANOVIEW)m_Model.FillFromFile(RecipeFilePath);
        //        if (rst == ERRORCODE_NANOVIEW.SR_NO_ERROR)
        //        {
        //            m_DM.m_Log.WriteLog(LogType.Operating, "Modelling File Load Done");
        //            MessageBox.Show("Modelling File Load Done"); //추후 제거
        //        }
        //        else
        //        {
        //            string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), rst);
        //            m_DM.m_Log.WriteLog(LogType.Error, sErr);
        //            MessageBox.Show(sErr); //추후 제거
        //            return rst;
        //        }
        //        return rst;
        //    }
        //    catch (Exception ex)
        //    {
        //        m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
        //        MessageBox.Show(ex.Message);
        //        return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
        //    }
        //}


        public ERRORCODE_NANOVIEW LoadModel(string sRecipeFilePath, bool bFileOpen = false)
        {
            try
            {
                string path_sclfile = "";
                string sExt = Path.GetExtension(sRecipeFilePath);
                if (sExt != ".rcp" && sExt != ".erm")
                {
                    m_DM.m_Log.WriteLog(LogType.Error, "올바른 파일 형식이 아닙니다. - " + sExt);
                    MessageBox.Show("올바른 파일 형식이 아닙니다. - " + sExt);
                    return ERRORCODE_NANOVIEW.ATI_PARAMETER_ERROR;
                }
                ERRORCODE_NANOVIEW rst;

                if (bFileOpen)
                {
                    //rst = (ERRORCODE_NANOVIEW)m_Model.FillFromFile(sRecipeFilePath);
                    //m_DM.m_LayerData = m_LayerList.ToLayerData();

                    rst = (ERRORCODE_NANOVIEW)m_ModelSave.FillFromFile(sRecipeFilePath);
                }
                else
                {
                    // 레시피 파일 로드 시, DM 에 Nano-View dll 데이터 저장
                    rst = (ERRORCODE_NANOVIEW)m_Model.FillFromFile(sRecipeFilePath);
                    m_DM.m_LayerData = m_LayerList.ToLayerData();
                    
                }

                if (rst == ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    m_DM.m_Log.WriteLog(LogType.Operating, "Modelling Done");
                    //MessageBox.Show("Modelling Done"); //추후 제거
                }
                else
                {
                    if (bFileOpen)
                    {
                        foreach (Material m in m_ModelSave.m_MaterialList)
                        {
                            m_ModelSave.m_MaterialList.Remove(m);
                        }
                    }
                    else
                    {
                        foreach (Material m in m_Model.m_MaterialList)
                        {
                            m_Model.m_MaterialList.Remove(m);
                        }
                    }
                    string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), rst);
                    m_DM.m_Log.WriteLog(LogType.Error, sErr);
                    MessageBox.Show(sErr); //추후 제거
                }
                if (bFileOpen)
                {
                    m_DM.m_ThicknessDataSave = new List<ThicknessScaleOffset>();
                }
                else
                {
                    m_DM.m_ThicknessData = new List<ThicknessScaleOffset>();
                }
                path_sclfile = Path.GetDirectoryName(sRecipeFilePath) + "\\" + Path.GetFileNameWithoutExtension(sRecipeFilePath) + ".scl";
                if (File.Exists(path_sclfile))
                {
                    StreamReader SR = new StreamReader(path_sclfile);

                    while (!SR.EndOfStream)
                    {
                        string str = SR.ReadLine();
                        string[] res = str.Split(',');
                        if (bFileOpen)
                        {
                            m_DM.m_ThicknessDataSave.Add(new ThicknessScaleOffset(Convert.ToDouble(res[0]), Convert.ToDouble(res[1])));
                        }
                        else
                        {
                            m_DM.m_ThicknessData.Add(new ThicknessScaleOffset(Convert.ToDouble(res[0]), Convert.ToDouble(res[1])));
                        }
                    }

                    SR.Close();
                }
                else
                {

                    if (bFileOpen)
                    {
                        for (int i = 0; i < m_LayerListSave.Count; i++)
                        {
                            m_DM.m_ThicknessDataSave.Add(new ThicknessScaleOffset());
                        }
                    }
                    else
                    {
                        for (int i = 0; i < m_LayerList.Count; i++)
                        {
                            m_DM.m_ThicknessData.Add(new ThicknessScaleOffset());
                        }
                    
                    }
                }
                

                    //string[] line = new string[20];

                    //int linecnt = 0;

                    //while ((line[linecnt] = SR.ReadLine()) != null)
                    //{
                    //    linecnt++;
                    //}

                    ////m_thk_scl_linecnt = linecnt;

                    //for (int j = 0; j < linecnt; j++)
                    //{
                    //    string[] result = line[j].Split(new char[] { ',' });


                    //    m_thk_scale[j] = Convert.ToDouble(result[0]);
                    //    m_thk_offset[j] = Convert.ToDouble(result[1]);


                    //}

                    

                return rst;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                //MessageBox.Show(ex.Message);
                return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
            }
        }

        //public ERRORCODE_NANOVIEW LoadModel(string sRecipeFilePath, bool bFileOpen)
        //{
        //    try
        //    {
        //        string path_sclfile = "";
        //        string sExt = Path.GetExtension(sRecipeFilePath);
        //        if (sExt != ".rcp" && sExt != ".erm")
        //        {
        //            m_DM.m_Log.WriteLog(LogType.Error, "올바른 파일 형식이 아닙니다. - " + sExt);
        //            MessageBox.Show("올바른 파일 형식이 아닙니다. - " + sExt);
        //            return ERRORCODE_NANOVIEW.ATI_PARAMETER_ERROR;
        //        }
        //        ERRORCODE_NANOVIEW rst;

        //        if (bFileOpen)
        //        {
        //            // 레시피 파일 로드 시, DM 에 Nano-View dll 데이터 저장
        //            rst = (ERRORCODE_NANOVIEW)m_Model.FillFromFile(sRecipeFilePath);
        //            m_DM.m_LayerData = m_LayerList.ToLayerData();
        //        }
        //        else
        //        {
        //            rst = (ERRORCODE_NANOVIEW)m_ModelSave.FillFromFile(sRecipeFilePath);
        //        }

        //        m_DM.m_LayerData = m_LayerList.ToLayerData();
        //        if (rst == ERRORCODE_NANOVIEW.SR_NO_ERROR)
        //        {
        //            m_DM.m_Log.WriteLog(LogType.Operating, "Modelling Done");
        //            //MessageBox.Show("Modelling Done"); //추후 제거
        //        }
        //        else
        //        {
        //            foreach (Material m in m_Model.m_MaterialList)
        //            {
        //                m_Model.m_MaterialList.Remove(m);
        //            }

        //            string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), rst);
        //            m_DM.m_Log.WriteLog(LogType.Error, sErr);
        //            MessageBox.Show(sErr); //추후 제거
        //        }

        //        m_DM.m_ThicknessData = new List<ThicknessScaleOffset>();
        //        path_sclfile = Path.GetDirectoryName(sRecipeFilePath) + "\\" + Path.GetFileNameWithoutExtension(sRecipeFilePath) + ".scl";
        //        if (File.Exists(path_sclfile))
        //        {
        //            StreamReader SR = new StreamReader(path_sclfile);

        //            while (!SR.EndOfStream)
        //            {
        //                string str = SR.ReadLine();
        //                string[] res = str.Split(',');

        //                m_DM.m_ThicknessData.Add(new ThicknessScaleOffset(Convert.ToDouble(res[0]), Convert.ToDouble(res[1])));
        //            }

        //            SR.Close();
        //        }
        //        else
        //        {
        //            for (int i = 0; i < m_DM.m_LayerData.Count; i++)
        //            {
        //                m_DM.m_ThicknessData.Add(new ThicknessScaleOffset());
        //            }
        //        }


        //        //string[] line = new string[20];

        //        //int linecnt = 0;

        //        //while ((line[linecnt] = SR.ReadLine()) != null)
        //        //{
        //        //    linecnt++;
        //        //}

        //        ////m_thk_scl_linecnt = linecnt;

        //        //for (int j = 0; j < linecnt; j++)
        //        //{
        //        //    string[] result = line[j].Split(new char[] { ',' });


        //        //    m_thk_scale[j] = Convert.ToDouble(result[0]);
        //        //    m_thk_offset[j] = Convert.ToDouble(result[1]);


        //        //}



        //        return rst;
        //    }
        //    catch (Exception ex)
        //    {
        //        m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
        //        MessageBox.Show(ex.Message);
        //        return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
        //    }
        //}

        public bool SaveModel(string sRecipeFilePath) //저장하거나 불러올때 NanoView따위의 추가 string이 필요함
        {
            try
            {
                //_layer[] layers = m_DM.m_LayerData.To_layer();

                //m_SR.Model(layers, layers.Length);
                if (UpdateModel(true) == false)
                {
                    MessageBox.Show("Save Modeling Fail! Please check the log.");
                    return false;
                }

                if (Path.GetExtension(sRecipeFilePath) != ".rcp")
                    sRecipeFilePath += ".rcp";

                File.WriteAllLines(sRecipeFilePath, m_ModelSave.m_LayerList.ToString());

                string path_sclfile = Path.GetDirectoryName(sRecipeFilePath) + "\\" + Path.GetFileNameWithoutExtension(sRecipeFilePath) + ".scl";

                try
                {
                    StreamWriter SW = new StreamWriter(path_sclfile);

                    for (int j = 0; j < m_DM.m_ThicknessDataSave.Count; j++)
                    {
                        string strline;

                        strline = string.Format("{0},{1}", m_DM.m_ThicknessDataSave[j].m_dThicknessScale, m_DM.m_ThicknessDataSave[j].m_dThicknessOffset);

                        SW.WriteLine(strline);
                    }

                    SW.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                    return false;

                }

                m_DM.m_Log.WriteLog(LogType.Operating, "Recipe Save Done");
                //MessageBox.Show("Recipe Save Done"); //추후 제거

                return true;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public Material GetMaterialFromName(string sMaterialName)
        {
            try
            {
                Material material = m_ModelSave.GetMaterialFromName(sMaterialName);

                return material;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                //MessageBox.Show(ex.Message);
                return null;
            }
        }

        public ERRORCODE_NANOVIEW SaveSettingParameters(SettingData datas)
        {
            try
            {
                if (m_bSRInitialized)
                {
                    string sPath = m_sConfigPath.Replace("cfg", "ini");

                    WritePrivateProfileString("ARC Detector Parameter", "Background Int. time", datas.nBGIntTime_VIS.ToString(), sPath);
                    WritePrivateProfileString("ARC Detector Parameter", "Average", datas.nAverage_VIS.ToString(), sPath);
                    WritePrivateProfileString("ARC Detector Parameter", "Integration time", datas.nInitCalIntTime_VIS.ToString(), sPath);
                    WritePrivateProfileString("ARCNIR Detector Parameter", "Background Int. time", datas.nBGIntTime_NIR.ToString(), sPath);
                    WritePrivateProfileString("ARCNIR Detector Parameter", "Average", datas.nAverage_NIR.ToString(), sPath);
                    WritePrivateProfileString("ARCNIR Detector Parameter", "Integration time", datas.nInitCalIntTime_NIR.ToString(), sPath);

                    m_DM.m_Log.WriteLog(LogType.Operating, "Save Done");

                    return ERRORCODE_NANOVIEW.SR_NO_ERROR;
                }
                else
                {
                    string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), -1);
                    m_DM.m_Log.WriteLog(LogType.Error, sErr);
                    return ERRORCODE_NANOVIEW.SR_DO_HW_INITIALIZE_FIRST;
                }
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Operating, "Save Fail - " + ex.Message);
                return ERRORCODE_NANOVIEW.ATI_PARAMETER_ERROR;
            }
        }

        public (SettingData, ERRORCODE_NANOVIEW) LoadSettingParameters()
        {
            try
            {
                if (m_bSRInitialized)
                {
                    string sPath = m_sConfigPath.Replace("cfg", "ini");

                    SettingData datas = new SettingData();

                    int nBufSize = 100;
                    StringBuilder key = new StringBuilder(nBufSize);
                    int nData;

                    GetPrivateProfileString("ARC Detector Parameter", "Background Int. time", "No Info.", key, nBufSize, sPath);
                    if (int.TryParse(key.ToString(), out nData)) datas.nBGIntTime_VIS = nData;

                    GetPrivateProfileString("ARC Detector Parameter", "Average", "No Info.", key, nBufSize, sPath);
                    if (int.TryParse(key.ToString(), out nData)) datas.nAverage_VIS = nData;

                    GetPrivateProfileString("ARC Detector Parameter", "Boxcar", "No Info.", key, nBufSize, sPath);
                    if (int.TryParse(key.ToString(), out nData)) datas.nBoxcar_VIS = nData;

                    GetPrivateProfileString("ARC Detector Parameter", "Integration time", "No Info.", key, nBufSize, sPath);
                    if (int.TryParse(key.ToString(), out nData)) datas.nInitCalIntTime_VIS = nData;

                    GetPrivateProfileString("ARCNIR Detector Parameter", "Background Int. time", "No Info.", key, nBufSize, sPath);
                    if (int.TryParse(key.ToString(), out nData)) datas.nBGIntTime_NIR = nData;

                    GetPrivateProfileString("ARCNIR Detector Parameter", "Average", "No Info.", key, nBufSize, sPath);
                    if (int.TryParse(key.ToString(), out nData)) datas.nAverage_NIR = nData;

                    GetPrivateProfileString("ARCNIR Detector Parameter", "Boxcar", "No Info.", key, nBufSize, sPath);
                    if (int.TryParse(key.ToString(), out nData)) datas.nBoxcar_NIR = nData;

                    GetPrivateProfileString("ARCNIR Detector Parameter", "Integration time", "No Info.", key, nBufSize, sPath);
                    if (int.TryParse(key.ToString(), out nData)) datas.nInitCalIntTime_NIR = nData;


                    return (datas, ERRORCODE_NANOVIEW.SR_NO_ERROR);
                }
                else
                {
                    string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), -1);
                    m_DM.m_Log.WriteLog(LogType.Error, sErr);
                    return (null, ERRORCODE_NANOVIEW.SR_DO_HW_INITIALIZE_FIRST);
                }
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Operating, "Load Fail - " + ex.Message);
                return (null, ERRORCODE_NANOVIEW.ATI_PARAMETER_ERROR);
            }
        }



        public ERRORCODE_NANOVIEW Calibration(bool bInitialCal)
        {
            //Init Calibration 아니고 Sample 측정 시 Measure Background
            try
            {
                //bool bCheckShutter = CheckShutter();
                //double[] spectrum = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                double[] VISBackCount = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                double[] NIRBackCount = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                double[] VISBackWavelength = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                double[] NIRBackWavelength = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                m_DM.BackGroundCalCountData = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                m_DM.BackGroundCalWavelength = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];

                if (isExceptNIR)
                {
                    m_SR.BackIntTime_NIR = 15;
                    m_SR.Average_NIR = 0;
                }
                else
                {

                    m_SR.BackIntTime_NIR = m_DM.m_SettngData.nBGIntTime_NIR;
                    m_SR.Average_NIR = m_DM.m_SettngData.nAverage_NIR;

                }
                if (bInitialCal == true)
                {
                    m_SR.IntTime_VIS = m_DM.m_SettngData.nInitCalIntTime_VIS;
                    m_SR.IntTime_NIR = m_DM.m_SettngData.nInitCalIntTime_NIR;
                }
                else
                {
                    m_SR.IntTime_VIS = m_DM.m_SettngData.nMeasureIntTime_VIS;
                    m_SR.IntTime_NIR = m_DM.m_SettngData.nMeasureIntTime_NIR;
                }
                m_SR.BackIntTime_VIS = m_DM.m_SettngData.nBGIntTime_VIS;
                m_SR.Average_VIS = m_DM.m_SettngData.nAverage_VIS;
                m_SR.bUpdateBeta = bInitialCal;


                //ERRORCODE_NANOVIEW rst = (ERRORCODE_NANOVIEW)m_SR.MeasureBackground(spectrum);
                ERRORCODE_NANOVIEW rst = (ERRORCODE_NANOVIEW)m_SR.MeasureBackground(VISBackWavelength, VISBackCount, NIRBackWavelength, NIRBackCount);
                Thread.Sleep(1000);
                if (rst == ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    string sLogPlus = "";
                    if (m_SR.bUpdateBeta)
                        sLogPlus = "Init ";

                    m_DM.m_Log.WriteLog(LogType.Operating, sLogPlus + "Calibration Done");
                    //MessageBox.Show(sLogPlus + "Calibration Done");
                    if (!bInitialCal)
                    {
                        bCheckSampleCal = true;
                    }
                    //추가 Test Count Data 남기기
                    if (!bInitialCal)
                    {
                        Array.Copy(VISBackWavelength, 0, m_DM.BackGroundCalWavelength, 0, 378);
                        Array.Copy(NIRBackWavelength, 12, m_DM.BackGroundCalWavelength, 378, 48);
                        Array.Copy(VISBackCount, 0, m_DM.BackGroundCalCountData, 0, 378);
                        Array.Copy(NIRBackCount, 12, m_DM.BackGroundCalCountData, 378, 48);

                        m_DM.CalibrationBackCountSave();
                        //m_PMDatas.LampCheck();
                    }
                }

                else
                {
                    string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), rst);
                    m_DM.m_Log.WriteLog(LogType.Error, sErr);
                    //MessageBox.Show(sErr); //추후 제거
                }

                if (m_SR.bUpdateBeta)
                {
                    double[] rs = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    double[] reflectance = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    double[] eV = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    int nNumofData = 0;
                    rst = (ERRORCODE_NANOVIEW)m_SR.Measure(rs, reflectance, eV, ref nNumofData);

                    if (rst == ERRORCODE_NANOVIEW.SR_NO_ERROR)
                    {
                        m_DM.m_Log.WriteLog(LogType.Operating, "Init Calibration Measure Done");
                        //MessageBox.Show("Init Calibration Measure Done");
                    }
                    else
                    {
                        string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), rst);
                        m_DM.m_Log.WriteLog(LogType.Error, sErr);
                        //MessageBox.Show(sErr); //추후 제거
                    }
                }
                // 임시
                //return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
                return rst;
                //}
                //else// measurebackground error code 제대로 나오면 없애버리기
                //{
                //    string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), -1);
                //    m_DM.m_Log.WriteLog(LogType.Error, sErr);
                //    MessageBox.Show(sErr); //추후 제거
                //    return ERRORCODE_NANOVIEW.SR_DO_HW_INITIALIZE_FIRST;
                //}
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                //MessageBox.Show(ex.Message);
                return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
            }
        }

        public ERRORCODE_NANOVIEW SampleMeasure(int nPointIndex, double dXPos, double dYPos, bool bExcept_NIR, bool bTransmittance, bool bThickness, float nLowerWaveLength, float nUpperWavelength, string sMeasureIndex = "")
        {
            try
            {
                //Shutter 체크 기능 추가
                bool bShutterOpen = true;
                if (nPointIndex == 0)
                {
                    //ShutterMotion(true);
                    bool bCheckShutter = CheckShutter();
                    //bool bShutterOpen = false;

                    if (!bCheckShutter)
                    {
                        ERRORCODE_NANOVIEW rst = Calibration(false);
                        if (rst == ERRORCODE_NANOVIEW.SR_NO_ERROR)
                        {
                            bShutterOpen = CheckShutter();
                        }
                        else
                        {
                            m_DM.m_Log.WriteLog(LogType.Error, "Sample Cal Error");
                        }

                    }
                    else
                    {
                        bShutterOpen = bCheckShutter;
                    }

                    bCheckShutter = CheckShutter();
                }
                if (bShutterOpen)
                {
                    Thread.Sleep(500);
                    if (m_bSRInitialized == true)
                    {
                        if (!bCheckSampleCal)
                        {
                            m_DM.m_Log.WriteLog(LogType.Warning, "Sample Calibration First");
                            MessageBox.Show("Sample Calibration First");
                        }
                        //fitting할때 들어가야함
                        //_layer[] layers = m_DM.m_LayerData.To_layer();
                        //m_SR.Model(layers, layers.Length);
                        m_DM.bExcept_NIR = bExcept_NIR;
                        m_DM.bThickness = bThickness;
                        m_DM.bTransmittance = bTransmittance;
                        if (m_DM.bExcept_NIR)
                        {
                            m_SR.IntTime_NIR = 15;
                        }
                        else
                        {
                            m_SR.IntTime_NIR = m_DM.m_SettngData.nMeasureIntTime_NIR;
                        }
                        m_SR.IntTime_VIS = m_DM.m_SettngData.nMeasureIntTime_VIS;


                        double[] rs = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                        double[] reflectance = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                        double[] eV = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                        int nNumOfData = 0;

                        ERRORCODE_NANOVIEW rst = (ERRORCODE_NANOVIEW)m_SR.Measure(reflectance, rs, eV, ref nNumOfData);
                        if (double.IsNaN(reflectance[0]))
                        {
                            rst = ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
                            m_DM.m_Log.WriteLog(LogType.Error, "Beta File Not Found");
                        }
                        if (rst == ERRORCODE_NANOVIEW.SR_NO_ERROR)
                        {

                            RawData data = m_DM.m_RawData[nPointIndex];
                            data.bDataExist = true;
                            data.nCalcDataNum = nNumOfData;
                            m_DM.nStartWavelegth = nLowerWaveLength;
                            m_DM.nThicknessDataNum = (int)(nUpperWavelength - nLowerWaveLength) + 1;
                            data.nNIRDataNum = m_SR.m_ExpNum;
                            data.dX = dXPos;
                            data.dY = dYPos;
                            Array.Reverse(m_SR.m_ExpR);
                            Array.Reverse(m_SR.m_Expnm);
                            m_SR.m_ExpR.CopyTo(data.Reflectance, 0);

                            m_SR.m_Expnm.CopyTo(data.Wavelength, 0);
                            Array.Reverse(m_SR.m_ExpR);
                            for (int i = 0; i < nNumOfData; i++)
                            {
                                if (Math.Round(ConstValue.EV_TO_WAVELENGTH_VALUE / eV[i]) == nUpperWavelength)
                                {
                                    Array.Copy(reflectance, i, data.VIS_Reflectance, 0, m_DM.nThicknessDataNum);
                                    Array.Copy(eV, i, data.eV, 0, m_DM.nThicknessDataNum);
                                    break;
                                }

                            }
                            //Array.Copy(reflectance, data.VIS_Reflectance, data.nThicknessDataNum);
                            //Array.Copy(eV, data.eV, data.nThicknessDataNum);

                            m_DM.m_Log.WriteLog(LogType.Operating, sMeasureIndex + " Measure Done");
                            //MessageBox.Show("Sample Measure Done"); //추후 제거
                        }
                        else
                        {
                            RawData data = m_DM.m_RawData[nPointIndex]; //데이터는 없지만 칸 수는 채워야 다음 반사율 데이터가 올바르게 저장됨
                            //string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), rst);
                            string ERRORCODE = rst.ToString();
                            m_DM.m_Log.WriteLog(LogType.Error, ERRORCODE);
                            // MessageBox.Show(sErr); //추후 제거
                        }

                        //string strPath;
                        //strPath = Application.StartupPath + "\\" + "Measure Data";
                        //DirectoryInfo di = new DirectoryInfo(strPath);
                        //if (di.Exists == false) di.Create();

                        //string filename = String.Format("{0}\\{1}", strPath, "sample");

                        //string expfilename = filename + ".exp";
                        //string txtfilename = filename + ".txt";

                        //StreamWriter writer = new StreamWriter(expfilename);

                        //for (int i = 0; i < nNumOfData; i++)
                        //{
                        //    writer.WriteLine("{0:f4} {1:f4} {2}", rs[i], reflectance[i], eV[i], Environment.NewLine);
                        //}
                        //writer.Close();

                        //StreamWriter writer1 = new StreamWriter(txtfilename);

                        //for (int i = 0; i < m_SR.m_ExpNum; i++)
                        //{
                        //    writer1.WriteLine("{0:f0} {1:f4}", m_SR.m_Expnm[i], m_SR.m_ExpR[i], Environment.NewLine);
                        //}
                        //writer1.Close();

                        return rst;
                    }
                    else
                    {
                        string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), -1);
                        m_DM.m_Log.WriteLog(LogType.Error, sErr);
                        //MessageBox.Show(sErr); //추후 제거
                        return ERRORCODE_NANOVIEW.SR_DO_HW_INITIALIZE_FIRST;
                    }
                }
                else
                {
                    string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), 17);
                    m_DM.m_Log.WriteLog(LogType.Error, sErr);
                    //MessageBox.Show(sErr); //추후 제거
                    return ERRORCODE_NANOVIEW.SR_SHUTTER_MOTION_ERROR;
                }
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                //MessageBox.Show(ex.Message);
                return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
            }
        }

        public double GetAlphaFit()
        {
            return m_SR.Alpha1;
        }


        public ERRORCODE_NANOVIEW GetThickness(int nPointIndex, int nIteration, double dDampingFactor,string sMeasurePoint,bool isCalDCOLTransmittance, bool isAlphafit = true)
        {
            try
            {
                if (m_DM.bThickness)
                {
                    m_DM.bCalDCOLTransmittance = isCalDCOLTransmittance;
                    m_SR.m_iteration = nIteration;
                    m_SR.m_divratio = Math.Round(dDampingFactor, 3);
                    m_SR.bAlpha1Fit = isAlphafit;
                    m_SR.Alpha1 = 1.0;

                    if (m_Model.m_LayerList.Count == 0)
                    {
                        m_DM.m_Log.WriteLog(LogType.Warning, "Model recipe is not opened.");
                        //MessageBox.Show("Open Model First!"); //추후 제거

                        return ERRORCODE_NANOVIEW.ATI_PARAMETER_ERROR;
                    }
                    if (!m_DM.m_RawData[nPointIndex].bDataExist)
                    {
                        m_DM.m_Log.WriteLog(LogType.Warning, "Point: " + sMeasurePoint + " Sample data is not exist.");
                        //MessageBox.Show("Do Measure First!");
                        return ERRORCODE_NANOVIEW.REFLECTANCE_DATA_NOT_FOUND;
                    }
                    if (m_bSRInitialized == true)
                    {
                        //fitting할때 들어가야함
                        if (nPointIndex == 0)
                        {
                            if (UpdateModel(false) == false)
                            {
                            //MessageBox.Show("Modeling Fail! Please check the log.");
                            m_DM.m_Log.WriteLog(LogType.Warning, "Modeling Recipe Ready Fail");
                            return ERRORCODE_NANOVIEW.SR_LAYER_MODEL_NOT_READY;
                            }
                        }
                        RawData data = m_DM.m_RawData[nPointIndex];
                        int m_NKFitLayer = 0;
                        m_SR.NKFitLayer = m_NKFitLayer;
                        //Stopwatch sw = new Stopwatch();
                        //sw.Start();
                        ERRORCODE_NANOVIEW rst = (ERRORCODE_NANOVIEW)m_SR.Fit(data.VIS_Reflectance, data.VIS_Reflectance, data.eV, m_DM.nThicknessDataNum);
                        //sw.Stop();
                        //Debug.WriteLine("Fit >> " + sw.ElapsedMilliseconds.ToString());
                        if (rst == ERRORCODE_NANOVIEW.SR_NO_ERROR)
                        {
                            Array.Copy(m_SR.FitY, data.CalcReflectance, m_DM.nThicknessDataNum);

                            data.Thickness.Clear();
                            for (int n = 0; n < m_SR.Thickness.Count(); n++)
                            {
                                data.Thickness.Add(m_SR.Thickness[n]);
                            }

                            double dAvgR = 0.0;
                            int nWLCount = 0;
                            double[] VIS_Wavelength = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];

                            for (int i = 0; i < data.nCalcDataNum; i++)
                            {
                                if (data.VIS_Reflectance[i] > 0)
                                {
                                    VIS_Wavelength[i] = ConstValue.EV_TO_WAVELENGTH_VALUE / data.eV[i];
                                    dAvgR += m_DM.m_RawData[0].VIS_Reflectance[i];
                                    nWLCount++;
                                }
                            }
                            VIS_Wavelength.CopyTo(data.VIS_Wavelength, 0);
                            dAvgR = dAvgR / nWLCount;

                            data.dGoF = m_Calculation.CalcGoF(data.VIS_Reflectance, data.CalcReflectance, 0, nWLCount, 0, nWLCount - 1, dAvgR);

                            if (data.dGoF > 0.99999)
                            {
                                data.dGoF = 0.9999;
                            }

                            //for (int n = 0; n < m_SR.Thickness.Count(); n++)
                            //{
                            //    m_DM.m_Log.WriteLog(LogType.Datas, "Thickness - " + m_Model.m_LayerList[n].m_Host.m_Name + ": " + m_SR.Thickness[n].ToString() + "A");
                            //}

                            m_DM.m_Log.WriteLog(LogType.Operating, sMeasurePoint + " Fit Done");
                        }
                        else
                        {
                            string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), rst);
                            m_DM.m_Log.WriteLog(LogType.Error, sErr);
                            //MessageBox.Show(sErr); //추후 제거

                        }

                        if (m_DM.bTransmittance)
                        {
                            if (GetTransmittance(nPointIndex))
                            {
                                m_DM.m_Log.WriteLog(LogType.Operating, sMeasurePoint+ "Transmittance Cal Done");
                            }
                            else
                            {
                                m_DM.m_Log.WriteLog(LogType.Error, sMeasurePoint+ "Cal Transmittance Fail!");
                                //MessageBox.Show("Cal Transmittance Fail!");
                                return ERRORCODE_NANOVIEW.ATI_CAL_TRANSMITTANCE_FAIL;
                            }
                        }


                        return rst;
                    }
                    else
                    {
                        string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), -1);
                        m_DM.m_Log.WriteLog(LogType.Error, sErr);
                        //MessageBox.Show(sErr); //추후 제거
                        return ERRORCODE_NANOVIEW.SR_DO_HW_INITIALIZE_FIRST;
                    }
                }
                else
                {
                    m_DM.m_Log.WriteLog(LogType.Others, "No Cal Thickenss");
                    ERRORCODE_NANOVIEW rst = 0;
                    return rst;
                }

            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                //MessageBox.Show(ex.Message);
                return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
            }
        }

        public bool UpdateModel(bool bFileSave)
        {


            bool isSave = false;
            int i, count;
            string str = string.Empty;
            Model LayerData;
            if (bFileSave)
            {
                LayerData = m_ModelSave;
            }
            else
            {
                LayerData = m_Model;
            }
            count = LayerData.m_LayerList.Count;
            _layer[] layer = new _layer[count];

            for (i = 0; i < count; i++)
            {
                layer[i].hostname = new char[128];
                layer[i].hostpath = new char[512];
                layer[i].guest1name = new char[128];
                layer[i].guest1path = new char[512];
                layer[i].guest2name = new char[128];
                layer[i].guest2path = new char[512];
            }

            i = 0;

            foreach (Layer l in LayerData.m_LayerList)
            {
                if (l.m_Host == null)
                {
                    m_DM.m_Log.WriteLog(LogType.Operating, "The Host is null");
                    return false;
                }

                layer[i].hostname = l.m_Host.m_Name.ToCharArray();
                layer[i].hostpath = l.m_Host.m_Path.ToCharArray();

                if (l.m_Guest1 != null)
                {
                    layer[i].guest1name = l.m_Guest1.m_Name.ToCharArray();
                    layer[i].guest1path = l.m_Guest1.m_Path.ToCharArray();
                }
                else
                {
                    str = "None";
                    layer[i].guest1name = str.ToCharArray();
                    layer[i].guest1path = str.ToCharArray();
                }
                if (l.m_Guest2 != null)
                {
                    layer[i].guest2name = l.m_Guest2.m_Name.ToCharArray();
                    layer[i].guest2path = l.m_Guest2.m_Path.ToCharArray();
                }
                else
                {
                    str = "None";
                    layer[i].guest2name = str.ToCharArray();
                    layer[i].guest2path = str.ToCharArray();
                }

                layer[i].fv1 = l.m_fv1;
                layer[i].fv2 = l.m_fv2;
                layer[i].thickness = l.m_Thickness;
                layer[i].fv1fit = l.m_bFitfv1;
                layer[i].fv2fit = l.m_bFitfv2;
                layer[i].thfit = l.m_bFitThickness;
                layer[i].emm = l.m_Emm;

                i++;
            }
            if (!bFileSave)
            {
                ERRORCODE_NANOVIEW rst = (ERRORCODE_NANOVIEW)m_SR.Model(layer, count);
                if (rst == 0)
                {
                    m_DM.m_Log.WriteLog(LogType.Operating, "Modelling done");
                    isSave = true;
                }
                else
                {
                    m_DM.m_Log.WriteLog(LogType.Operating, "Modelling failed : " + m_SR.ErrorString);
                    isSave = false;
                }
            }
            else
            {
                isSave = true;
            }


            return isSave;
        }

        public ERRORCODE_NANOVIEW LoadMaterial(string sMaterialFilePath, bool isSave = false)
        {
            try
            {
                string sFileName = Path.GetFileName(sMaterialFilePath);

                int nRst = -1;
                if (isSave)
                    nRst = m_ModelSave.LoadMaterialFile(sMaterialFilePath);
                else
                    nRst = m_Model.LoadMaterialFile(sMaterialFilePath);
                if (nRst == 0)
                {
                    m_DM.m_Log.WriteLog(LogType.Operating, "Material(" + sFileName + ") file load done.");
                    return ERRORCODE_NANOVIEW.SR_NO_ERROR;
                }
                else
                {
                    string sErrorText = string.Empty;

                    if (nRst == 1) sErrorText = String.Format("The material({0}) is already in the list.", sFileName);
                    if (nRst == 2) sErrorText = String.Format("Error : file extension of ({0}) is incorrect.", sFileName);
                    if (nRst == 3) sErrorText = String.Format("Error : file ({0}) not found.", sFileName);

                    MessageBox.Show(sErrorText);
                    m_DM.m_Log.WriteLog(LogType.Error, sErrorText);

                    return ERRORCODE_NANOVIEW.SR_MATERIAL_FILE_LOAD_ERROR;
                }
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                MessageBox.Show(ex.Message);
                return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
            }
        }

        public ERRORCODE_NANOVIEW GetSpectrum(ref double[] DataX, ref double[] DataY, ref int nNumOfSpectrum)
        {
            try
            {
                m_SR.BackIntTime_NIR = m_DM.m_SettngData.nBGIntTime_NIR;
                m_SR.Average_NIR = m_DM.m_SettngData.nAverage_NIR;
                m_SR.IntTime_VIS = m_DM.m_SettngData.nInitCalIntTime_VIS;
                m_SR.IntTime_NIR = m_DM.m_SettngData.nInitCalIntTime_NIR;
                m_SR.BackIntTime_VIS = m_DM.m_SettngData.nBGIntTime_VIS;
                m_SR.Average_VIS = m_DM.m_SettngData.nAverage_VIS;

                if (m_bSRInitialized == false)
                {
                    //MessageBox.Show("Initialize first");
                    m_DM.m_Log.WriteLog(LogType.Warning, "Initialize first");
                    return ERRORCODE_NANOVIEW.SR_DO_HW_INITIALIZE_FIRST;
                }


                int NumOfSpectrum = 0;
                double[] SpectrumData = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                double[] Wavelength = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                ERRORCODE_NANOVIEW rst = (ERRORCODE_NANOVIEW)m_SR.GetSpectrum(SpectrumData, Wavelength, ref NumOfSpectrum);


                if (rst == ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    //m_DM.m_Log.WriteLog(LogType.Operating, "GetSpectrum");
                    SpectrumData.CopyTo(DataX, 0);
                    Wavelength.CopyTo(DataY, 0);
                    nNumOfSpectrum = NumOfSpectrum;

                }
                else
                {
                    string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), rst);
                    m_DM.m_Log.WriteLog(LogType.Error, sErr);
                    //MessageBox.Show(sErr); //추후 제거
                }
                return rst;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                //MessageBox.Show(ex.Message);
                return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
            }
        }

        public bool GetTransmittance(int nPointIndex)
        {
            try
            {
                // 선택 파장 투과율 배열 생성하기
                double[] CalTWavelenghList = new double[m_DM.m_ContourMapDataT.Count];
                for (int i=0; i < m_DM.m_ContourMapDataT.Count; i++ )
                {
                    CalTWavelenghList[i] = m_DM.m_ContourMapDataT[i].Wavelength;
                }

                Stopwatch sw = new Stopwatch();
                if (nPointIndex == 0)
                {
                    sw.Start();
                    if (LoadNKDatas() == false)
                    {
                        m_DM.m_Log.WriteLog(LogType.Error, "NK Data Cal Error");
                        //MessageBox.Show("NK Data is not Found");

                        return false;
                    }
                    sw.Stop();
                    Debug.WriteLine("Cal nk >> " + sw.ElapsedMilliseconds.ToString());
                }
                int nDNum = m_DM.m_LayerData.Count - 2;
                double[] dThickness = new double[m_DM.m_LayerData.Count - 2];

                for (int i = 0; i < m_SR.Thickness.Count() - 2; i++)
                {
                    int nCalLayer = m_SR.Thickness.Count() - (i + 2);
                    dThickness[i] = m_SR.Thickness[nCalLayer];
                }
                Stopwatch sw1 = new Stopwatch();
                Stopwatch sw2 = new Stopwatch();
                if (!isCalDCOLTransmittance)
                {
                    sw1.Start();
                    m_Calculation.CalcTransmittance_OptimizingSi(nPointIndex, ConstValue.SI_AVG_OFFSET_RANGE, ConstValue.SI_AVG_OFFSET_STEP, nDNum, dThickness, CalTWavelenghList);
                    sw1.Stop();
                    Debug.WriteLine("Cal t >> " + sw1.ElapsedMilliseconds.ToString());
                }
                else
                {
                    
                    sw2.Start();
                    m_Calculation.PointCalcTransmittance_OptimizingSi(nPointIndex, ConstValue.SI_AVG_OFFSET_RANGE, ConstValue.SI_AVG_OFFSET_STEP, nDNum, dThickness, CalTWavelenghList);
                    sw2.Stop();
                    Debug.WriteLine("CalPoint t >> " + sw2.ElapsedMilliseconds.ToString());
                }
                Debug.WriteLine("CalPoint t >> " + sw2.ElapsedMilliseconds.ToString());
                return true;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Operating, "Cal Transmittance Fail!");
                return false;
            }
        }

        public bool LoadNKDatas()
        {
            try
            {
                bool bLoadNKData = true;
                int nWLStart = Convert.ToInt32(m_DM.nStartWavelegth);
                int nWLStop, nNKDataNum;
                //m_DM.m_LayerData = m_Model.m_LayerList.ToLayerData();

                for (int n = 0; n < m_Model.m_LayerList.Count; n++)
                {
                    m_DM.m_LayerData[n].wavelength.Clear();
                    m_DM.m_LayerData[n].n.Clear();
                    m_DM.m_LayerData[n].k.Clear();
                }

                m_Model.YType = Model.DataType.RPRS;
                m_Model.m_Angle = m_Model.m_AngleAfter = 0.0;
                if (m_DM.bExcept_NIR)
                {
                    nWLStop = nWLStart + m_DM.nThicknessDataNum;
                    nNKDataNum = ConstValue.NUM_OF_MATERIAL_DATANUM;
                    m_Model.m_eVMax = 4.0;
                    m_Model.m_eVMin = 1.3;
                }
                else
                {
                    nWLStop = nWLStart + m_DM.m_RawData[0].nNIRDataNum-1;
                    nNKDataNum = 2 * ConstValue.NUM_OF_MATERIAL_DATANUM;
                    m_Model.m_eVMax = 4.0;
                    m_Model.m_eVMin = 0.8;
                }
                m_Model.m_NumOfData = nNKDataNum;
                m_Model.AllocateMemory();
                m_Model.MakeModelData(Model.CalculateType.INIT);

                for (int n = 0; n < m_Model.m_LayerList.Count - 1; n++)
                {
                    string sMaterialName = m_Model.m_LayerList[n].m_Host.m_Name;
                    foreach (Material m in m_Model.m_MaterialList)
                    {
                        if (m.m_Name == sMaterialName)
                        {
                            if (CalNKDatas(n, nWLStart, nWLStop, m.m_e, m.m_eV))
                            {
                                bLoadNKData = true;
                            }
                            else
                            {
                                bLoadNKData = false;
                            }
                        }

                    }
                }
                return bLoadNKData;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                return false;
            }
        }
        public bool CalNKDatas(int nLayerIdx, int WLStart, int WLStop, Complex[] Epsilon, double[] eV)
        {
            try
            {
                List<double> wavelength = new List<double>();
                List<double> CalnData = new List<double>();
                List<double> CalkData = new List<double>();
                List<double> TempWavelength = new List<double>();
                List<double> Tempn = new List<double>();
                List<double> Tempk = new List<double>();

                // 엡실론 데이터 nk 데이터로 변환
                double dCaln = 0.0;
                double dCalk = 0.0;
                double temp_n = 0.0;
                double temp_k = 0.0;

                for (int n = 0; n < eV.Length; n++)
                {
                    wavelength.Add(1239.8116 / eV[n]);

                    temp_n = Math.Pow((Math.Pow(Epsilon[n].real, 2) + Math.Pow(Epsilon[n].imag, 2)), 0.5) + Epsilon[n].real;
                    dCaln = Math.Pow(0.5 * temp_n, 0.5);
                    CalnData.Add(dCaln);

                    temp_k = Math.Pow((Math.Pow(Epsilon[n].real, 2) + Math.Pow(Epsilon[n].imag, 2)), 0.5) - Epsilon[n].real;
                    dCalk = Math.Pow(0.5 * temp_k, 0.5);
                    CalkData.Add(dCalk);
                }

                //1nm 파장 간격에 맞추어 nk data spline 후 DataManager에 저장
                int nSplineScale = 0;
                double dInit_WL = Math.Truncate(wavelength[0]);
                bool bSplineCal = true;
                CubicSpline CSpline_nData = CubicSpline.InterpolateNatural(wavelength, CalnData);
                CubicSpline CSpline_kData = CubicSpline.InterpolateNatural(wavelength, CalkData);
                while (bSplineCal)
                {
                    TempWavelength.Add((-1 * nSplineScale) + dInit_WL);
                    Tempn.Add(CSpline_nData.Interpolate(TempWavelength.Last()));
                    Tempk.Add(CSpline_kData.Interpolate(TempWavelength.Last()));
                    nSplineScale++;
                    if (TempWavelength.Last() <= WLStart)
                    {
                        bSplineCal = false;
                    }
                }

                //DataManager에 데이터 세이브
                int nDataCount = Tempn.Count;
                double[] array_Wavelength = new double[nDataCount];
                double[] array_n = new double[nDataCount];
                double[] array_k = new double[nDataCount];

                TempWavelength.CopyTo(array_Wavelength);
                Tempn.CopyTo(array_n);
                Tempk.CopyTo(array_k);

                Array.Reverse(array_Wavelength);
                Array.Reverse(array_n);
                Array.Reverse(array_k);

                for (int n = 0; n < nDataCount; n++)
                {
                    if (array_Wavelength[n] >= WLStart && array_Wavelength[n] <= WLStop)
                    {
                        m_DM.m_LayerData[nLayerIdx].wavelength.Add(array_Wavelength[n]);
                        m_DM.m_LayerData[nLayerIdx].n.Add(array_n[n]);
                        m_DM.m_LayerData[nLayerIdx].k.Add(array_k[n]);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                return false;
            }
        }
       
        public bool SaveRawData(int NumofData, double[] expx, double[] expy)
        {
            try
            {
                int i;
                double x, y;

                for (i = 0; i < NumofData; i++)
                {
                    //x = hw / expx[i]; 
                    //expx가 Wavelength면 eV로, eV면 Wavelength로
                    x = expx[i];
                    y = expy[i];
                }

                SaveFileDialog saveDialog = new SaveFileDialog();

                saveDialog.DefaultExt = "csv";
                saveDialog.AddExtension = true;
                saveDialog.Filter = "Reflectance Raw Data (*.csv)|*.csv|All files (*.*)|*.*";
                saveDialog.FilterIndex = 0;
                saveDialog.FileName = "";
                saveDialog.InitialDirectory = "..\\..\\Data";
                saveDialog.OverwritePrompt = true;
                saveDialog.RestoreDirectory = false;
                saveDialog.Title = "Save Raw Data";
                saveDialog.ValidateNames = true;

                StreamWriter writer = new StreamWriter(saveDialog.FileName);

                for (i = 0; i < NumofData; i++)
                {
                    writer.WriteLine("{0} {1} {2}", expx[i], expy[i], Environment.NewLine);
                }
                writer.Close();

                m_DM.m_Log.WriteLog(LogType.Operating, "Save Done");
                //MessageBox.Show("Save Done"); //추후제거
                return true;
            }

            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                //MessageBox.Show(ex.Message); //추후제거
                return false;
            }
        }

        public string GetErrorString(ERRORCODE_NANOVIEW errCode)
        {
            string sErrString = string.Empty;

            sErrString = m_SR.ErrorString + "\n";

            switch (errCode)
            {
                case ERRORCODE_NANOVIEW.SR_NO_ERROR:
                    sErrString = "No Error."; break;
                case ERRORCODE_NANOVIEW.SR_SERIAL_PORT_NOT_OPENED:
                    sErrString = "SR serial port is not opened."; break;
                case ERRORCODE_NANOVIEW.SR_SPECTROMETER_NOT_FOUND:
                    sErrString = "Cannot find the spectrometer."; break;
                case ERRORCODE_NANOVIEW.SR_SYSTEM_ERROR:
                    sErrString = "The SR system error is occurred."; break;
                case ERRORCODE_NANOVIEW.SR_REFERENCE_FILE_NOT_FOUND:
                    sErrString = "Reference files are not found."; break;
                case ERRORCODE_NANOVIEW.SR_LAYER_MODEL_NOT_READY:
                    sErrString = "Layer model is not ready. Please open a recipe first."; break;
                case ERRORCODE_NANOVIEW.SR_INVALID_NUMBER_OF_DATA:
                    sErrString = "The number of data is invalid."; break;
                case ERRORCODE_NANOVIEW.SR_INVALID_FIT_LAYER:
                    sErrString = "Fit layers are invalid."; break;
                case ERRORCODE_NANOVIEW.SR_MATERIAL_FILE_ERROR:
                    sErrString = "Material files error."; break;
                case ERRORCODE_NANOVIEW.SR_MATERIAL_FILE_LOAD_ERROR:
                    sErrString = "Material file load error."; break;
                case ERRORCODE_NANOVIEW.SR_MODELING_FAIL:
                    sErrString = "Modeling Fail."; break;
                case ERRORCODE_NANOVIEW.SR_DO_HW_INITIALIZE_FIRST:
                    sErrString = "Please do initialize H/W first."; break;
                case ERRORCODE_NANOVIEW.SR_CANNOT_FIND_DONGLE:
                    sErrString = "Dongle key is not found."; break;
                case ERRORCODE_NANOVIEW.REFLECTANCE_DATA_NOT_FOUND:
                    sErrString = "Cannot find Reflectance data."; break;
                case ERRORCODE_NANOVIEW.ATI_CAL_TRANSMITTANCE_FAIL:
                    sErrString = "Transmittance data calculation fail."; break;
                case ERRORCODE_NANOVIEW.ATI_PARAMETER_ERROR:
                    sErrString = "Input parameter is wrong."; break;
                case ERRORCODE_NANOVIEW.NANOVIEW_ERROR:
                    sErrString = "Nanoview.dll error."; break;
                default:
                    sErrString = "Not Exist Error Code"; break;
            }

            return sErrString;
        }

        ///<summary>
        ///bLampOperation  (Lamp 정상동작 여부) /
        ///bControllerOperation (Controller 정상동작 여부) /
        ///bLampOn (Lamp On/Off 여부) /
        ///bLaserOn (Laser On/Off 여부)
        ///</summary>
        public ERRORCODE_NANOVIEW GetLightSourceStatus(ref bool bLampOperation, ref bool bControllerOperation, ref bool bLampOn, ref bool bLaserOn)
        {
            //Lamp fault/controller  o:NG/1:OK
            //Lamp On/ Laser ON 0:OFF/ 1:ON
            try
            {
                int[] LightSignal = new int[4];
                int pixelDepth = m_SR.PixelDepth;
                m_SR.CheckLight(LightSignal);

                if (LightSignal[0] == 0)
                {
                    bLampOperation = false;
                    m_DM.m_Log.WriteLog(LogType.Error, "Lamp Fault!");
                }
                else
                {
                    bLampOperation = true;
                }

                if (LightSignal[1] == 0)
                {
                    bControllerOperation = false;
                    m_DM.m_Log.WriteLog(LogType.Error, "Lamp Controller Fault!");
                }
                else
                {
                    bControllerOperation = true;

                }

                if (LightSignal[2] == 0)
                    bLampOn = false;
                else
                    bLampOn = true;

                if (m_bPreLampOn != bLampOn)
                {
                    m_DM.m_Log.WriteLog(LogType.Operating, "LampOn: " + bLampOn.ToString());
                    m_bPreLampOn = bLampOn;
                }

                if (LightSignal[3] == 0)
                    bLaserOn = false;
                else
                    bLaserOn = true;

                if (m_bPreLaserOn != bLaserOn)
                {
                    m_DM.m_Log.WriteLog(LogType.Operating, "LaserOn: " + bLaserOn.ToString());
                    m_bPreLaserOn = bLaserOn;
                }
                return ERRORCODE_NANOVIEW.SR_NO_ERROR;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
               // MessageBox.Show(ex.Message);
                return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
            }
        }

        //public void LightSourceLogging(string DirectoryPath)//input : Write Log 생성 및 작성 경로
        //{
        //    try
        //    {
        //        //로그상의 최신상태를 기록하는 불변수
        //        bool bError = false;
        //        bool bOn = false;
        //        bool bOff = false;

        //        string CurrentFilePath = DirectoryPath + "\\LightLog.txt";
        //        if (!File.Exists(CurrentFilePath))//로그파일이 없으면 새로 생성
        //        {
        //            StreamWriter LightLog = File.CreateText(CurrentFilePath);
        //            LightLog.Close();
        //        }
        //        else
        //        {
        //            StreamReader LogReader = new StreamReader(CurrentFilePath);

        //            while (!LogReader.EndOfStream)//로그에서 가장 마지막 상태를 기록
        //            {
        //                string StatusCode = LogReader.ReadLine().Split(',')[0];

        //                if (StatusCode == "켜짐")
        //                {
        //                    bOn = true;
        //                    bOff = false;
        //                    bError = false;
        //                }
        //                else if (StatusCode == "꺼짐")
        //                {
        //                    bOn = false;
        //                    bOff = true;
        //                    bError = false;
        //                }
        //                else if (StatusCode == "에러")
        //                {
        //                    bOn = false;
        //                    bOff = false;
        //                    bError = true;
        //                }
        //            }
        //            LogReader.Close();
        //        }

        //        int[] LightSignal = new int[4];
        //        string[] LightResult = new string[1];

        //        ERRORCODE_NANOVIEW rst = (ERRORCODE_NANOVIEW)m_SR.CheckLight(LightSignal);

        //        if ((LightSignal[0] != 1 || LightSignal[1] != 1) && !bError)//에러가 발생했고, 가장 최근 로그가 에러가 아니면
        //        {
        //            LightResult[0] = "에러," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //            File.AppendAllLines(CurrentFilePath, LightResult);
        //        }

        //        if (LightSignal[2] == 1 && !bOff)
        //        {
        //            LightResult[0] = "꺼짐," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //            File.AppendAllLines(CurrentFilePath, LightResult);
        //        }
        //        else if (LightSignal[2] != 1 && !bOn)
        //        {
        //            LightResult[0] = "켜짐," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //            File.AppendAllLines(CurrentFilePath, LightResult);
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
        //        MessageBox.Show(ex.Message);
        //    }
        //}

        //public void ChangeLightSourceLog(string DirectoryPath)//input으로 로그생성 폴더까지만입력, 기존 조명로그파일의 이름을 변경하여 백업함
        //{
        //    string CurrentFilePath = DirectoryPath + "\\LightLog.txt";
        //    if (File.Exists(CurrentFilePath))
        //    {
        //        string newFilePath = DirectoryPath + "\\LightLog.txt";
        //        int i = 1;
        //        while (File.Exists(newFilePath))
        //        {
        //            newFilePath = DirectoryPath.Replace(".txt", String.Format("({0:C}).txt", i));
        //            File.Move(DirectoryPath, newFilePath);
        //            i++;
        //        }

        //        File.Move(CurrentFilePath, newFilePath);
        //    }
        //}

        //public double GetLightSourceUsedTime(string DirectoryPath)// 몇시간사용했는지 나옴 Round로 소수 자리수 끊어서 사용
        //{
        //    string CurrentFilePath = DirectoryPath + "\\LightLog.txt";
        //    if (File.Exists(CurrentFilePath))
        //    {
        //        TimeSpan OnWorking = TimeSpan.Zero;
        //        StreamReader LogReader = new StreamReader(CurrentFilePath);
        //        string[] Line = LogReader.ReadLine().Split(',');
        //        string PreStatus = Line[0];
        //        string PreTime = Line[1];
        //        while (!LogReader.EndOfStream)
        //        {
        //            Line = LogReader.ReadLine().Split(',');
        //            string CurrentStatus = Line[0];
        //            string CurrentTime = Line[1];
        //            if (PreStatus == "켜짐" && (CurrentStatus != "켜짐")) // => 꺼짐 to 켜짐
        //            {
        //                OnWorking += DateTime.ParseExact(CurrentTime, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) - DateTime.ParseExact(PreTime, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        //            }
        //            PreStatus = CurrentStatus;
        //            PreTime = CurrentTime;
        //        }
        //        if (Line[0] == "켜짐") //로그에서 켜짐으로 끝나는 경우 현재와 시간계산 한 값 추가
        //        {
        //            OnWorking += DateTime.Now - DateTime.ParseExact(Line[1], "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        //        }

        //        return OnWorking.TotalHours;
        //    }
        //    else
        //    {
        //        return -1;//파일이 존재하지않을때
        //    }
        //}

        #region Shutter 관련 기능
        public bool ShutterMotion(bool bShutterOpen)
        {
            bool bOpen = false;
            if (bShutterOpen)
            {
                m_SR.ShutterMotion(1);
                bOpen = false;
                return bOpen;
            }
            else
            {
                m_SR.ShutterMotion(2);
                bOpen = true;
                return bOpen;
            }

        }

        public bool CheckShutter()
        {
            bool bOpen = false;

            if (m_SR.m_bShutterOpen == true)
            {
                bOpen = true;
                return bOpen;
            }
            else
            {
                bOpen = false;
                return bOpen;
            }
        }

        #endregion


        #region Lamp Controller  Check

        public enum CheckLampState
        {
            SignalError = -3,
            Error_Controller_Power_OFF = -2,
            Error_Lamp_Switch_OFF = -1,
            Error_Lamp_Temperature_High = 0,
            ON = 1,
            WarmUP = 2,
            OFF = 3
        }


        public SerialPort sp = new SerialPort();
        public void CloseLampSignal()
        {
            sp.Write("t");
            string OutputData = sp.ReadLine();
            string[] strtext = new string[7] { "H0:", "T0:", "L0:", "H1:", "T1:", "L1:", "Time:" };
            string[] arr = OutputData.Split(':');
            //string[] strarr = arr[1].Split(',');
            double LampUseTime = 0.0;
            double value = 0.0;
            int Hr, Min, Sec;
            int m_Hr_Org, m_Min_Org, m_Sec_Org;
            int m_Hr, m_Min, m_Sec;
            string filepath, timedata, strtime;
            foreach (string str in strtext)
            {
                if (OutputData.Contains(str))
                {
                    if (OutputData.Contains("Time:"))
                    {
                        filepath = string.Empty;
                        filepath = @"C:\Camellia2\Init\Timedata.txt";

                        FileInfo fi = new FileInfo(filepath);

                        m_Hr_Org = m_Min_Org = m_Sec_Org = 0;
                        m_Hr = m_Min = m_Sec = 0;

                        if (fi.Exists)
                        {
                            timedata = File.ReadAllText(@filepath);
                            char sp = ':';
                            string[] spstring = timedata.Split(sp);

                            m_Hr_Org = Convert.ToInt32(spstring[0]);
                            m_Min_Org = Convert.ToInt32(spstring[1]);
                            m_Sec_Org = Convert.ToInt32(spstring[2]);

                        }
                        Sec = Convert.ToInt32(arr[1]);

                        m_Sec = Sec + m_Sec_Org;

                        if (m_Sec >= 60)
                        {
                            m_Min = (m_Sec / 60) + m_Min_Org;

                            m_Sec = m_Sec % 60;
                        }
                        else
                        {
                            m_Min = m_Min_Org;
                        }

                        if (m_Min >= 60)
                        {
                            m_Hr = (m_Min / 60) + m_Hr_Org;

                            m_Min = m_Min % 60;
                        }
                        else
                        {
                            m_Hr = m_Hr_Org;
                        }
                        value = m_Hr;
                        strtime = string.Format("{ 0}:{1}:{2}", m_Hr, m_Min, m_Sec);
                        using (StreamWriter SW = new StreamWriter(filepath))
                        {
                            SW.WriteLine(strtime);
                            m_DM.m_Log.WriteLog(LogType.Datas, "Lamp Use Time" + strtime);
                        }
                    }

                }
            }
            sp.Write("c");
        }
        void InitLamp()
        {
            try
            {
                sp.PortName = "COM6";
                sp.BaudRate = 9600;
                sp.DataBits = 8;
                sp.StopBits = StopBits.One;
                sp.Parity = Parity.None;
                //sp.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);
                sp.Open();

                sp.Write("t");
                sp.ReadTimeout = 1000;
                string OutputData = sp.ReadLine();
                string[] strtext = new string[7] { "H0:", "T0:", "L0:", "H1:", "T1:", "L1:", "Time:" };
                string[] arr = OutputData.Split(':');
                //string[] strarr = arr[1].Split(',');
                double LampUseTime = 0.0;
                double value = 0.0;
                int Hr, Min, Sec;
                int m_Hr_Org, m_Min_Org, m_Sec_Org;
                int m_Hr, m_Min, m_Sec;
                string filepath, timedata, strtime;
                foreach (string str in strtext)
                {
                    if (OutputData.Contains(str))
                    {
                        if (OutputData.Contains("Time:"))
                        {
                            filepath = string.Empty;
                            filepath = @"C:\Camellia2\Init\Timedata.txt";

                            FileInfo fi = new FileInfo(filepath);

                            m_Hr_Org = m_Min_Org = m_Sec_Org = 0;
                            m_Hr = m_Min = m_Sec = 0;

                            if (!fi.Exists)
                            {
                                fi.Create();
                                using (StreamWriter SW = new StreamWriter(filepath))
                                {
                                    SW.WriteLine("00:00:00");
                                    SW.Close();
                                }
                                return;
                            }

                            timedata = File.ReadAllText(@filepath);

                            char split = ':';
                            string[] spstring = timedata.Split(split);

                            m_Hr_Org = Convert.ToInt32(spstring[0]);
                            m_Min_Org = Convert.ToInt32(spstring[1]);
                            m_Sec_Org = Convert.ToInt32(spstring[2]);


                            Sec = Convert.ToInt32(arr[1]);

                            m_Sec = Sec + m_Sec_Org;

                            if (m_Sec >= 60)
                            {
                                m_Min = (m_Sec / 60) + m_Min_Org;

                                m_Sec = m_Sec % 60;
                            }
                            else
                            {
                                m_Min = m_Min_Org;
                            }

                            if (m_Min >= 60)
                            {
                                m_Hr = (m_Min / 60) + m_Hr_Org;

                                m_Min = m_Min % 60;
                            }
                            else
                            {
                                m_Hr = m_Hr_Org;
                            }
                            value = m_Hr;
                            strtime = string.Format("{0}:{1}:{2}", m_Hr, m_Min, m_Sec);
                            using (StreamWriter SW = new StreamWriter(filepath))
                            {
                                SW.WriteLine(strtime);
                                m_DM.m_Log.WriteLog(LogType.Datas, "Lamp Use Time" + strtime);
                            }
                        }

                    }
                }
                sp.Write("c");
            }
            catch(Exception e)
            {
                
            }
          
        }

        public double UpdateLampData(string CheckWord)
        {
            sp.Write(CheckWord);
            string OutputData = sp.ReadLine();
            string[] strtext = new string[7] { "H0:", "T0:", "L0:", "H1:", "T1:", "L1:", "Time:" };
            string[] arr = OutputData.Split(':');
            //string[] strarr = arr[1].Split(',');
            double LampUseTime = 0.0;
            double value = 0.0;
            int Hr, Min, Sec;
            int m_Hr_Org, m_Min_Org, m_Sec_Org;
            int m_Hr, m_Min, m_Sec;
            string filepath, timedata, strtime;
            foreach (string str in strtext)
            {
                if (OutputData.Contains(str))
                {
                    if (OutputData.Contains("Time:"))
                    {
                        filepath = string.Empty;
                        filepath = @"C:\Camellia2\Init\Timedata.txt";

                        FileInfo fi = new FileInfo(filepath);

                        m_Hr_Org = m_Min_Org = m_Sec_Org = 0;
                        m_Hr = m_Min = m_Sec = 0;
                        
                        if (!fi.Exists)
                        {
                            fi.Create();
                            using (StreamWriter SW = new StreamWriter(filepath))
                            {
                                SW.WriteLine("00:00:00");
                                SW.Close();
                            }
                            return value;
                        }

                        timedata = File.ReadAllText(@filepath);
                        char split = ':';
                        string[] spstring = timedata.Split(split);

                        m_Hr_Org = Convert.ToInt32(spstring[0]);
                        m_Min_Org = Convert.ToInt32(spstring[1]);
                        m_Sec_Org = Convert.ToInt32(spstring[2]);

                        Sec = Convert.ToInt32(arr[1]);

                        m_Sec = Sec + m_Sec_Org;

                        if (m_Sec >= 60)
                        {
                            m_Min = (m_Sec / 60) + m_Min_Org;

                            m_Sec = m_Sec % 60;
                        }
                        else
                        {
                            m_Min = m_Min_Org;
                        }

                        if (m_Min >= 60)
                        {
                            m_Hr = (m_Min / 60) + m_Hr_Org;

                            m_Min = m_Min % 60;
                        }
                        else
                        {
                            m_Hr = m_Hr_Org;
                        }
                        value = m_Hr;
                        strtime = string.Format("{0}:{1}:{2}", m_Hr, m_Min, m_Sec);
                        using (StreamWriter SW = new StreamWriter(filepath))
                        {
                            SW.WriteLine(strtime);
                            m_DM.m_Log.WriteLog(LogType.Datas, "Lamp Use Time" + strtime);
                        }
                        sp.Write("c");
                        
                    }
                    else
                    {
                        decimal number1 = 0;
                        bool canConvert = decimal.TryParse(arr[1], out number1);
                        if (canConvert == true)
                            value = Convert.ToDouble(number1);
                    }


                }

            }
            
            return value;
        }
        private void UpdateLampTime(bool Initialize)
        {
            double LampUseTime = UpdateLampData("t");

            //p_LampTimeCount = LampUseTime;

            if (LampUseTime > 9500)
            {
                LampPMCheck(10000 - LampUseTime);
            }
        }
        private void LampPMCheck(double dLeftLampTime)
        {
            //if (dLeftLampTime >= 0)
            //{
            //    string sLeftLampTime = dLeftLampTime.ToString();
            //    p_LampTimeError = sLeftLampTime + " Hours Left until PM !";

            //}
            //else
            //{
            //    p_LampTimeError = "Please, Do Lamp PM";
            //}
        }


        #endregion

        #region 램프 상태 체크
        public CheckLampState LampState()
        {
            bool LampFault = false;
            bool Controller = false;
            bool LampON = false;
            bool LaserON = false;
            bool VISLightON = false;
            bool NIRLightON = false;
            //double rst = 0.0;
            CheckLampState rst = CheckLampState.ON;
            if (UpdateLampData("1") > 0)
            {
                VISLightON = true;
            }
            else
            {
                VISLightON = false;
            }
            if (UpdateLampData("2") > 0)
            {
                NIRLightON = true;
            }
            else
            {
                NIRLightON = false;
            }

            if (GetLightSourceStatus(ref LampFault, ref Controller, ref LampON, ref LaserON) != Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
            {
                // 나머지 작업할 항목
                if (LaserON == true && LampON == false)
                {
                    rst = CheckLampState.WarmUP;
                }
                else if (LaserON == true && LampON == true)
                {
                    rst = CheckLampState.ON;
                }
                else if (LampFault == false || Controller == false)
                {
                    if (!LampFault)
                    {
                        rst = CheckLampState.Error_Lamp_Switch_OFF;
                    }
                    if (!Controller)
                    {
                        rst = CheckLampState.Error_Lamp_Temperature_High;
                    }
                }

                else if (LampFault == false && Controller == false && LampON == true)
                {
                    rst = CheckLampState.Error_Controller_Power_OFF;
                }
                else
                {
                    if (VISLightON == true && NIRLightON == true)
                    {
                        rst = CheckLampState.SignalError;
                    }
                    else if (VISLightON == false && NIRLightON == false)
                    {
                        rst = CheckLampState.OFF;
                    }
                }

            }
            return rst;
        }
    }
    #endregion
}

