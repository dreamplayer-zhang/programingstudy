using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NanoView;
using System.Runtime.InteropServices;
using System.Diagnostics;

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
            SR_SPECTROMETER_NOT_FOUND = 2,
            SR_SYSTEM_ERROR = 3,
            SR_REFERENCE_FILE_NOT_FOUND = 4,
            SR_LAYER_MODEL_NOT_READY = 11,
            SR_INVALID_NUMBER_OF_DATA = 12,
            SR_INVALID_FIT_LAYER = 13,
            SR_MATERIAL_FILE_ERROR = 14,
            SR_MATERIAL_FILE_LOAD_ERROR = 15,
            SR_MODELING_FAIL = 16,

            MEASURED_DATA_NOT_FOUND = 100,

            ATI_PARAMETER_ERROR = -8888,
            NANOVIEW_ERROR = -9999
        }

        public ARCNIR m_SR = null;
        Model m_Model = null;
        DataManager m_DM = DataManager.GetInstance();
        Calculation m_Calculation = new Calculation();
        public MaterialList m_MaterialList;
        public LayerList m_LayerList;
        string m_sConfigPath = string.Empty;

        public bool isExceptNIR = false;

        //double[] m_Spectrum;
        private bool m_bSRInitialized = false;
        private bool m_bPreLampOn = false;
        private bool m_bPreLaserOn = false;

        public Nanoview()
        {
            m_SR = new ARCNIR();
            m_Model = new Model();
            m_MaterialList = m_Model.m_MaterialList;
            m_LayerList = m_Model.m_LayerList;
            //Thickenss Alphafit
            m_SR.bAlpha1Fit = true;
            m_SR.Alpha1 = 1.0;
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
                    MessageBox.Show("Initialize Done"); //추후 제거
                }
                else
                {
                    string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), rst);
                    m_DM.m_Log.WriteLog(LogType.Error, sErr);
                    MessageBox.Show(sErr); //추후 제거
                }

                return rst;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                MessageBox.Show(ex.Message);
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



        public ERRORCODE_NANOVIEW LoadModel(string sRecipeFilePath)
        {
            try
            {
                string sExt = Path.GetExtension(sRecipeFilePath);
                if (sExt != ".rcp" && sExt != ".erm")
                {
                    m_DM.m_Log.WriteLog(LogType.Error, "올바른 파일 형식이 아닙니다. - " + sExt);
                    MessageBox.Show("올바른 파일 형식이 아닙니다. - " + sExt);
                    return ERRORCODE_NANOVIEW.ATI_PARAMETER_ERROR;
                }

                ERRORCODE_NANOVIEW rst = (ERRORCODE_NANOVIEW)m_Model.FillFromFile(sRecipeFilePath);

                m_DM.m_LayerData = m_LayerList.ToLayerData();

                if (rst == ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    m_DM.m_Log.WriteLog(LogType.Operating, "Modelling Done");
                    //MessageBox.Show("Modelling Done"); //추후 제거
                }
                else
                {
                    foreach (Material m in m_Model.m_MaterialList)
                    {
                        m_Model.m_MaterialList.Remove(m);
                    }

                    string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), rst);
                    m_DM.m_Log.WriteLog(LogType.Error, sErr);
                    MessageBox.Show(sErr); //추후 제거
                }

                return rst;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                MessageBox.Show(ex.Message);
                return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
            }
        }

        public bool SaveModel(string sRecipeFilePath) //저장하거나 불러올때 NanoView따위의 추가 string이 필요함
        {
            try
            {
                //_layer[] layers = m_DM.m_LayerData.To_layer();

                //m_SR.Model(layers, layers.Length);
                if (UpdateModel() == false)
                {
                    MessageBox.Show("Save Modeling Fail! Please check the log.");
                    return false;
                }

                if (Path.GetExtension(sRecipeFilePath) != ".rcp")
                    sRecipeFilePath += ".rcp";

                File.WriteAllLines(sRecipeFilePath, m_Model.m_LayerList.ToString());

                m_DM.m_Log.WriteLog(LogType.Operating, "Recipe Save Done");
                MessageBox.Show("Recipe Save Done"); //추후 제거

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
                Material material = m_Model.GetMaterialFromName(sMaterialName);

                return material;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                MessageBox.Show(ex.Message);
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
                    WritePrivateProfileString("ARCNIR Detector Parameter", "Background Int. time", datas.nBGIntTime_NIR.ToString(), sPath);
                    WritePrivateProfileString("ARCNIR Detector Parameter", "Average", datas.nAverage_NIR.ToString(), sPath);

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

                    GetPrivateProfileString("ARCNIR Detector Parameter", "Background Int. time", "No Info.", key, nBufSize, sPath);
                    if (int.TryParse(key.ToString(), out nData)) datas.nBGIntTime_NIR = nData;

                    GetPrivateProfileString("ARCNIR Detector Parameter", "Average", "No Info.", key, nBufSize, sPath);
                    if (int.TryParse(key.ToString(), out nData)) datas.nAverage_NIR = nData;

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

       

        public ERRORCODE_NANOVIEW Calibration(int nBGIntTime_VIS, int nBGIntTime_NIR, int nAverage_VIS, int nAverage_NIR, bool bInitialCal)
        {
            //Init Calibration 아니고 Sample 측정 시 Measure Background
            try
            {
                //if (bInitialCal == true)
                //{
                double[] spectrum = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                if (isExceptNIR)
                {
                    nBGIntTime_NIR = 15;
                    nAverage_NIR = 0;
                }
                m_SR.BackIntTime_VIS = nBGIntTime_VIS;
                m_SR.BackIntTime_NIR = nBGIntTime_NIR;
                m_SR.Average_VIS = nAverage_VIS;
                m_SR.Average_NIR = nAverage_NIR;
                m_SR.bUpdateBeta = bInitialCal;
              

                ERRORCODE_NANOVIEW rst = (ERRORCODE_NANOVIEW)m_SR.MeasureBackground(spectrum);
                if (rst == ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    string sLogPlus = "";
                    if (m_SR.bUpdateBeta)
                        sLogPlus = "Init ";

                    m_DM.m_Log.WriteLog(LogType.Operating, sLogPlus + "Calibration Done");
                    //MessageBox.Show(sLogPlus + "Calibration Done");
                }
                else
                {
                    string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), rst);
                    m_DM.m_Log.WriteLog(LogType.Error, sErr);
                    MessageBox.Show(sErr); //추후 제거
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
                        MessageBox.Show(sErr); //추후 제거
                    }
                }

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
                MessageBox.Show(ex.Message);
                return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
            }
        }

        public ERRORCODE_NANOVIEW SampleMeasure(int nPointIndex, double dXPos, double dYPos, int nIntTime_VIS, int nAverage_VIS, int nIntTime_NIR, int nAverage_NIR, bool bExcept_NIR, bool bTransmittance, bool bThickness, float nLowerWaveLength, float nUpperWavelength) //num of data를 반환할 필요가 있는지 모르겠음
        {
            try
            {
                if (m_bSRInitialized == true)
                {
                    //fitting할때 들어가야함
                    //_layer[] layers = m_DM.m_LayerData.To_layer();
                    //m_SR.Model(layers, layers.Length);
                    m_DM.bExcept_NIR = bExcept_NIR;
                    m_DM.bThickness = bThickness;
                    m_DM.bTransmittance = bTransmittance;
                    if (m_DM.bExcept_NIR)
                    {
                        nIntTime_NIR = 15;
                        nAverage_NIR = 0;
                    }
                    m_SR.IntTime_VIS = nIntTime_VIS;
                    m_SR.Average_VIS = nAverage_VIS;
                    m_SR.IntTime_NIR = nIntTime_NIR;
                    m_SR.Average_NIR = nAverage_NIR;
                   
                    double[] rs = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    double[] reflectance = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    double[] eV = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];
                    int nNumOfData = 0;

                    ERRORCODE_NANOVIEW rst = (ERRORCODE_NANOVIEW)m_SR.Measure(reflectance, rs, eV, ref nNumOfData);

                    if (rst == ERRORCODE_NANOVIEW.SR_NO_ERROR)
                    {
                        RawData data = m_DM.m_RawData[nPointIndex];
                        data.bDataExist = true;
                        data.nCalcDataNum = nNumOfData;
                        m_DM.nStartWavelegth = nLowerWaveLength;
                        m_DM.nThicknessDataNum= (int)(nUpperWavelength - nLowerWaveLength)+1;
                        data.nNIRDataNum = m_SR.m_ExpNum;
                        data.dX = dXPos;
                        data.dY = dYPos;
                        m_SR.m_ExpR.CopyTo(data.Reflectance, 0);
                        m_SR.m_Expnm.CopyTo(data.Wavelength, 0);
                        for (int i=0; i<nNumOfData;i++)
                        {
                            if( Math.Round(ConstValue.EV_TO_WAVELENGTH_VALUE/ eV[i])== nUpperWavelength)
                            {
                                Array.Copy(reflectance,i, data.VIS_Reflectance, 0,m_DM.nThicknessDataNum);
                                Array.Copy(eV,i, data.eV,0, m_DM.nThicknessDataNum);
                                break;
                            }

                        }
                        //Array.Copy(reflectance, data.VIS_Reflectance, data.nThicknessDataNum);
                        //Array.Copy(eV, data.eV, data.nThicknessDataNum);

                        m_DM.m_Log.WriteLog(LogType.Operating, "Sample Measure Done");
                        //MessageBox.Show("Sample Measure Done"); //추후 제거
                    }
                    else
                    {
                        string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), rst);
                        m_DM.m_Log.WriteLog(LogType.Error, sErr);
                        MessageBox.Show(sErr); //추후 제거
                    }

                    string strPath;
                    strPath = Application.StartupPath + "\\" + "Measure Data";
                    DirectoryInfo di = new DirectoryInfo(strPath);
                    if (di.Exists == false) di.Create();

                    string filename = String.Format("{0}\\{1}", strPath, "sample");

                    string expfilename = filename + ".exp";
                    string txtfilename = filename + ".txt";

                    StreamWriter writer = new StreamWriter(expfilename);

                    for (int i = 0; i < nNumOfData; i++)
                    {
                        writer.WriteLine("{0:f4} {1:f4} {2}", rs[i], reflectance[i], eV[i], Environment.NewLine);
                    }
                    writer.Close();

                    StreamWriter writer1 = new StreamWriter(txtfilename);

                    for (int i = 0; i < m_SR.m_ExpNum; i++)
                    {
                        writer1.WriteLine("{0:f0} {1:f4}", m_SR.m_Expnm[i], m_SR.m_ExpR[i], Environment.NewLine);
                    }
                    writer1.Close();

                    return rst;
                }
                else
                {
                    string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), -1);
                    m_DM.m_Log.WriteLog(LogType.Error, sErr);
                    MessageBox.Show(sErr); //추후 제거
                    return ERRORCODE_NANOVIEW.SR_DO_HW_INITIALIZE_FIRST;
                }
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                MessageBox.Show(ex.Message);
                return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
            }
        }

        public ERRORCODE_NANOVIEW GetThickness(int nPointIndex, int nIteration, double dDampingFactor)
        {
            try
            {
                if (m_DM.bThickness)
                {
                    m_SR.m_iteration = nIteration;
                    m_SR.m_divratio = dDampingFactor;

                    if (m_Model.m_LayerList.Count == 0)
                    {
                        m_DM.m_Log.WriteLog(LogType.Operating, "Model recipe is not opened.");
                        MessageBox.Show("Open Model First!"); //추후 제거

                        return ERRORCODE_NANOVIEW.ATI_PARAMETER_ERROR;
                    }
                    if (!m_DM.m_RawData[nPointIndex].bDataExist)
                    {
                        m_DM.m_Log.WriteLog(LogType.Operating, "Point: " + nPointIndex.ToString() + " Sample data is not exist.");
                        MessageBox.Show("Do Measure First!");

                        return ERRORCODE_NANOVIEW.MEASURED_DATA_NOT_FOUND;
                    }
                    if (m_bSRInitialized == true)
                    {
                        //fitting할때 들어가야함
                        if (nPointIndex == 0)
                        {
                            if (UpdateModel() == false)
                            {
                                MessageBox.Show("Modeling Fail! Please check the log.");
                                return ERRORCODE_NANOVIEW.SR_MODELING_FAIL;
                            }
                        }
                        RawData data = m_DM.m_RawData[nPointIndex];
                        //m_SR.bDispersionFit = false;
                        //m_SR.WavelengthForNK = 633;
                        int m_NKFitLayer = 0;
                        m_SR.NKFitLayer = m_NKFitLayer;
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        ERRORCODE_NANOVIEW rst = (ERRORCODE_NANOVIEW)m_SR.Fit(data.VIS_Reflectance, data.VIS_Reflectance, data.eV, m_DM.nThicknessDataNum);

                        sw.Stop();
                        Debug.WriteLine("Fit >> " + sw.ElapsedMilliseconds.ToString());

                     
                        Array.Copy(m_SR.FitY, data.CalcReflectance, m_DM.nThicknessDataNum);

                        data.Thickness.Clear();
                        for (int n = 0; n < m_SR.Thickness.Count(); n++)
                        {
                            data.Thickness.Add(m_SR.Thickness[n]);
                        }

                        double dAvgR = 0.0;
                        int nWLCount = 0;
                        double[] VIS_Wavelength = new double[ConstValue.SPECTROMETER_MAX_PIXELSIZE];

                        for (int i = 0; i < data.VIS_Reflectance.Length; i++)
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


                        if (rst == ERRORCODE_NANOVIEW.SR_NO_ERROR)
                        {
                            for (int n = 0; n < m_SR.Thickness.Count(); n++)
                            {
                                m_DM.m_Log.WriteLog(LogType.Datas, "Thickness - " + m_Model.m_LayerList[n].m_Host.m_Name + ": " + m_SR.Thickness[n].ToString() + "A");
                            }

                            m_DM.m_Log.WriteLog(LogType.Operating, "Nanoview Fit Done");
                        }
                        else
                        {
                            string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), rst);
                            m_DM.m_Log.WriteLog(LogType.Error, sErr);
                            MessageBox.Show(sErr); //추후 제거
                        }
                       
                        if (m_DM.bTransmittance)
                        {
                            if (GetTransmittance(nPointIndex))
                            {
                                m_DM.m_Log.WriteLog(LogType.Operating, "Transmittance Cal Done");
                            }
                            else
                            {
                                m_DM.m_Log.WriteLog(LogType.Operating, "Cal Transmittance Fail!");
                                MessageBox.Show("Cal Transmittance Fail!");

                                return ERRORCODE_NANOVIEW.MEASURED_DATA_NOT_FOUND;
                            }

                        }


                        return rst;
                    }
                    else
                    {
                        string sErr = Enum.GetName(typeof(ERRORCODE_NANOVIEW), -1);
                        m_DM.m_Log.WriteLog(LogType.Error, sErr);
                        MessageBox.Show(sErr); //추후 제거
                        return ERRORCODE_NANOVIEW.SR_DO_HW_INITIALIZE_FIRST;
                    }
                }
                else
                {
                    m_DM.m_Log.WriteLog(LogType.Others, "No Cal Thickenss");
                    ERRORCODE_NANOVIEW rst = 0;
                    return rst ;
                }
                
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                MessageBox.Show(ex.Message);
                return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
            }
        }

        public bool UpdateModel()
        {
            int i, count;
            string str = string.Empty;

            count = m_Model.m_LayerList.Count;
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

            foreach (Layer l in m_Model.m_LayerList)
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

            int nRst = m_SR.Model(layer, count);
            if (nRst == 0)
            {
                m_DM.m_Log.WriteLog(LogType.Operating, "Modelling done");
                return true;
            }
            else
            {
                m_DM.m_Log.WriteLog(LogType.Operating, "Modelling failed : " + m_SR.ErrorString);
                return false;
            }
        }

        public ERRORCODE_NANOVIEW LoadMaterial(string sMaterialFilePath)
        {
            try
            {
                string sFileName = Path.GetFileName(sMaterialFilePath);

                int nRst = m_Model.LoadMaterialFile(sMaterialFilePath);
                if(nRst == 0)
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
                if (m_bSRInitialized == false)
                {
                    MessageBox.Show("Initialize first");
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
                    MessageBox.Show(sErr); //추후 제거
                }
                return rst;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                MessageBox.Show(ex.Message);
                return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
            }
        }

        public bool GetTransmittance(int nPointIndex)
        {
            try
            {
                if (nPointIndex == 0)
                {
                    if (LoadNKDatas() == false)
                    {
                        m_DM.m_Log.WriteLog(LogType.Operating, "NK Data is not find");
                        MessageBox.Show("NK Data is not Found");

                        return false;
                    }
                }
                int nDNum = m_DM.m_LayerData.Count - 2;
                double[] dThickness = new double[m_DM.m_LayerData.Count - 2];

                for (int i = 0; i < m_SR.Thickness.Count() - 2; i++)
                {
                    int nCalLayer = m_SR.Thickness.Count() - (i + 2);
                    dThickness[i] = m_SR.Thickness[nCalLayer];
                }

                m_Calculation.CalcTransmittance_OptimizingSi(nPointIndex, ConstValue.SI_AVG_OFFSET_RANGE, ConstValue.SI_AVG_OFFSET_STEP, nDNum, dThickness);

                return true;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Operating, "Cal Transmittance Fail!");
                //MessageBox.Show("Cal Transmittance Fail!");

                return false;
            }
        }

        public bool LoadNKDatas()
        {
            bool bLoadNKData = true;

            m_DM.m_LayerData = m_Model.m_LayerList.ToLayerData();

            for (int n = 0; n < m_Model.m_LayerList.Count; n++)
            {
                m_DM.m_LayerData[n].wavelength.Clear();
                m_DM.m_LayerData[n].n.Clear();
                m_DM.m_LayerData[n].k.Clear();
            }
            string strTempPath = string.Empty;
            string strNKFilePath = string.Empty;
            string strNKFileFormat = string.Empty; 
            int nWLStart = Convert .ToInt32(m_DM.nStartWavelegth);
            int nWLStop;

            if (m_DM.bExcept_NIR)
            {
                nWLStop = nWLStart + m_DM.nThicknessDataNum;
            }
            else
            {
                nWLStop = nWLStart + m_DM.m_RawData[0].nNIRDataNum;
            }

            for (int n = 0; n < m_Model.m_LayerList.Count - 1; n++)
            {
                strTempPath = m_Model.m_LayerList[n].m_Host.m_Path;
                strNKFileFormat = Path.GetExtension(strTempPath);
                bool bNKFileFormat = strNKFileFormat.Contains(".ref");
                if (bNKFileFormat)
                {
                    strNKFilePath = strTempPath.Replace(".ref", ".csv");
                }
                else
                {
                    strNKFilePath = strTempPath.Replace(".dis", ".csv");
                }

                if (ReadNKDatas(n, strNKFilePath, nWLStart, nWLStop))
                {
                    bLoadNKData = true;
                }
                else
                {
                    bLoadNKData = false;
                }
            }

            return bLoadNKData;
        }

        public bool ReadNKDatas(int nLayerIdx, string NKFilePath, int WLStart, int WLStop)
        {
            FileInfo FileCheck = new FileInfo(NKFilePath);

            if (FileCheck.Exists)
            {
                StreamReader NKData = new StreamReader(NKFilePath);

                while (!NKData.EndOfStream)
                {
                    string strTempNK = NKData.ReadLine();
                    string[] strSplitNK = strTempNK.Split(',');
                    // 이부분 수정 할것 NIR 사용할 것인지 아닌지에 따라서 nCalDataNUm 과 nDataNum_NIR 로 바꿔서 데이터 수집 할것
                    if (Convert.ToInt32(strSplitNK[0]) >= WLStart && Convert.ToInt32(strSplitNK[0]) < WLStop)
                    {
                        m_DM.m_LayerData[nLayerIdx].wavelength.Add(Convert.ToDouble(strSplitNK[0]));
                        m_DM.m_LayerData[nLayerIdx].n.Add(Convert.ToDouble(strSplitNK[1]));
                        m_DM.m_LayerData[nLayerIdx].k.Add(Convert.ToDouble(strSplitNK[2]));
                    }
                }
                return true;
            }
            else
            {
                m_DM.m_Log.WriteLog(LogType.Operating, "NK 파장 범위가 맞지 않습니다.");
                System.Windows.Forms.MessageBox.Show("NK 파장 범위가 맞지 않습니다.");
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
                MessageBox.Show("Save Done"); //추후제거
                return true;
            }

            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                MessageBox.Show(ex.Message); //추후제거
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
                case ERRORCODE_NANOVIEW.MEASURED_DATA_NOT_FOUND:
                    sErrString = "Cannot find measured data."; break;
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
            try
            {
                int[] LightSignal = new int[4];
                int pixelDepth = m_SR.PixelDepth;
                ERRORCODE_NANOVIEW rst = (ERRORCODE_NANOVIEW)m_SR.CheckLight(LightSignal);

                if (LightSignal[0] == 0)
                    bLampOperation = true;
                else
                {
                    bLampOperation = false;
                    m_DM.m_Log.WriteLog(LogType.Error, "Lamp Fault!");
                }

                if (LightSignal[1] == 0)
                    bControllerOperation = true;
                else
                {
                    bControllerOperation = false;
                    m_DM.m_Log.WriteLog(LogType.Error, "Lamp Controller Fault!");
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
                return rst;
            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                MessageBox.Show(ex.Message);
                return ERRORCODE_NANOVIEW.NANOVIEW_ERROR;
            }
        }

        public void LightSourceLogging(string DirectoryPath)//input : Write Log 생성 및 작성 경로
        {
            try
            {
                //로그상의 최신상태를 기록하는 불변수
                bool bError = false;
                bool bOn = false;
                bool bOff = false;

                string CurrentFilePath = DirectoryPath + "\\LightLog.txt";
                if (!File.Exists(CurrentFilePath))//로그파일이 없으면 새로 생성
                {
                    StreamWriter LightLog = File.CreateText(CurrentFilePath);
                    LightLog.Close();
                }
                else
                {
                    StreamReader LogReader = new StreamReader(CurrentFilePath);

                    while (!LogReader.EndOfStream)//로그에서 가장 마지막 상태를 기록
                    {
                        string StatusCode = LogReader.ReadLine().Split(',')[0];

                        if (StatusCode == "켜짐")
                        {
                            bOn = true;
                            bOff = false;
                            bError = false;
                        }
                        else if (StatusCode == "꺼짐")
                        {
                            bOn = false;
                            bOff = true;
                            bError = false;
                        }
                        else if (StatusCode == "에러")
                        {
                            bOn = false;
                            bOff = false;
                            bError = true;
                        }
                    }
                    LogReader.Close();
                }

                int[] LightSignal = new int[4];
                string[] LightResult = new string[1];

                ERRORCODE_NANOVIEW rst = (ERRORCODE_NANOVIEW)m_SR.CheckLight(LightSignal);

                if ((LightSignal[0] != 1 || LightSignal[1] != 1) && !bError)//에러가 발생했고, 가장 최근 로그가 에러가 아니면
                {
                    LightResult[0] = "에러," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    File.AppendAllLines(CurrentFilePath, LightResult);
                }

                if (LightSignal[2] == 1 && !bOff)
                {
                    LightResult[0] = "꺼짐," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    File.AppendAllLines(CurrentFilePath, LightResult);
                }
                else if (LightSignal[2] != 1 && !bOn)
                {
                    LightResult[0] = "켜짐," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    File.AppendAllLines(CurrentFilePath, LightResult);
                }


            }
            catch (Exception ex)
            {
                m_DM.m_Log.WriteLog(LogType.Error, ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        public void ChangeLightSourceLog(string DirectoryPath)//input으로 로그생성 폴더까지만입력, 기존 조명로그파일의 이름을 변경하여 백업함
        {
            string CurrentFilePath = DirectoryPath + "\\LightLog.txt";
            if (File.Exists(CurrentFilePath))
            {
                string newFilePath = DirectoryPath + "\\LightLog.txt";
                int i = 1;
                while (File.Exists(newFilePath))
                {
                    newFilePath = DirectoryPath.Replace(".txt", String.Format("({0:C}).txt", i));
                    File.Move(DirectoryPath, newFilePath);
                    i++;
                }

                File.Move(CurrentFilePath, newFilePath);
            }
        }

        public double GetLightSourceUsedTime(string DirectoryPath)// 몇시간사용했는지 나옴 Round로 소수 자리수 끊어서 사용
        {
            string CurrentFilePath = DirectoryPath + "\\LightLog.txt";
            if (File.Exists(CurrentFilePath))
            {
                TimeSpan OnWorking = TimeSpan.Zero;
                StreamReader LogReader = new StreamReader(CurrentFilePath);
                string[] Line = LogReader.ReadLine().Split(',');
                string PreStatus = Line[0];
                string PreTime = Line[1];
                while (!LogReader.EndOfStream)
                {
                    Line = LogReader.ReadLine().Split(',');
                    string CurrentStatus = Line[0];
                    string CurrentTime = Line[1];
                    if (PreStatus == "켜짐" && (CurrentStatus != "켜짐")) // => 꺼짐 to 켜짐
                    {
                        OnWorking += DateTime.ParseExact(CurrentTime, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) - DateTime.ParseExact(PreTime, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    PreStatus = CurrentStatus;
                    PreTime = CurrentTime;
                }
                if (Line[0] == "켜짐") //로그에서 켜짐으로 끝나는 경우 현재와 시간계산 한 값 추가
                {
                    OnWorking += DateTime.Now - DateTime.ParseExact(Line[1], "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                }

                return OnWorking.TotalHours;
            }
            else
            {
                return -1;//파일이 존재하지않을때
            }
        }

    }
}
