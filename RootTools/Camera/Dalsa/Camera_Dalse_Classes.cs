using System;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using DALSA.SaperaLT.SapClassBasic;

namespace RootTools.Camera.Dalsa
{
    public enum eCamState
    {
        Init,
        Ready,
        GrabMem,
        GrabLive,
        TriggerGrab,
        Done,
    };

    public class DalseCamInfo : ObservableObject
    {
        eCamState _eState = eCamState.Init;
        public eCamState p_eState
        {
            get
            {
                return _eState;
            }
            set
            {
                SetValueProperty(ref _eState, value);
            }
        }
        public Log m_log;
        string _sServer = "";
        public string p_sServer
        {
            get
            {
                return _sServer;
            }
            set
            {
                SetValueProperty(ref _sServer, value);
            }
        }
        string _sFile = "";
        public string p_sFile
        {
            get
            {
                return _sFile;
            }
            set
            {
                SetValueProperty(ref _sFile, value);
            }
        }
        string _sAreaCamfile = "";
        public string p_sAreaCamFile
        {
            get => _sAreaCamfile;
            set => SetValueProperty(ref _sAreaCamfile, value);
        }
        int _nResourceCnt = 0;
        public int p_nResourceIdx
        {
            get
            {
                return _nResourceCnt;
            }
            set
            {
                SetValueProperty(ref _nResourceCnt, value);
            }
        }

        public DalseCamInfo(Log log)
        {
            m_log = log;
        }

        public void SetValueProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return;
            m_log.Info("Dalsa Cam Status Change : " + propertyName + " , " + value);
            SetProperty(ref storage, value, propertyName);
        }
    }


    public class DalsaParameterSet : ObservableObject
    {
        public enum eDir
        {
            Forward,
            Reverse,
        }

        public enum eTDIMode
        {
            Tdi,
            TdiArea,
        }


        public enum eDeviceScanType
        {
            Linescan,
            Areascan,
        }

        public enum eTriggerMode
        {
            Internal,
            External,
        }

        public enum eUserSet
        {
            Default=0,
            UserSet1,
            UserSet2,
            UserSet3,
            UserSet4,
            UserSet5,
            UserSet6,
            UserSet7,
            UserSet8,
            UserSet9,
            UserSet10,
            UserSet11,
            UserSet12,
            UserSet13,
            UserSet14,
            UserSet15,
            UserSet16,
            Factory,
        }

        public enum eFlatFieldUserSet
        {
            Factory = 0,
            UserSet1,
            UserSet2,
            UserSet3,
            UserSet4,
            UserSet5,
            UserSet6,
            UserSet7,
            UserSet8,
            UserSet9,
            UserSet10,
            UserSet11,
            UserSet12,
            UserSet13,
            UserSet14,
            UserSet15,
            UserSet16,
        }

        public enum eAnalogGain
        {
            One,
            Two,
            Four,
            Eight,
        }
        public static string[] m_aAnalogGain = new string[4] { "1", "2", "4", "8" };

        public enum eGainSelector
        {
            System,
            All,
            Blue,
            Green,
            Red,
        }

        public Log m_log;
        SapAcqDevice m_sapCam;
        SapAcquisition m_SapGrabber;
        int m_Width = 0;
        int m_Height = 0;
        public int p_Width
        {
            get
            {
                return m_Width;
            }
            set
            {
                SetFeatureValue(ref m_Width, value, "Width", typeof(ulong));
            }
        }
        public int p_Height
        {
            get
            {
                return m_Height;
            }
            set
            {
                SetFeatureValue(ref m_Height, value, "Height", typeof(ulong));
                RaisePropertyChanged();
            }
        }
        //public ulong p_SensorWidth
        //{
        //    get
        //    {
        //        if (m_sapDevice == null)
        //            return 0;
        //        ulong value = 0;
        //        m_sapDevice.GetFeatureValue("SensorWidth", out value);
        //        return value;
        //    }
        //}
        //public ulong p_SensorHeight
        //{
        //    get
        //    {
        //        if (m_sapDevice == null)
        //            return 0;
        //        ulong value = 0;
        //        m_sapDevice.GetFeatureValue("SensorHeight", out value);
        //        return value;
        //    }
        //}

        //int _nUserSetNum = 0;
        //public int p_nUserSetNum
        //{
        //    get => _nUserSetNum;
        //    set
        //    {
        //        if (value < 17 && value > 0) 
        //        {
        //            _nUserSetNum = value;
        //            SetUserset(value);
        //        }
        //    }
        //}
        eUserSet m_eUserSetPowerup = eUserSet.Default;
        public eUserSet p_eUserSetPowerup
        {
            get { return m_eUserSetPowerup; }
            set
            {
                if(SetPowerupUserset(value))
                    m_eUserSetPowerup = value;
            }
        }
        eUserSet m_eUserSetCurrent = eUserSet.Default;
        public eUserSet p_eUserSetCurrent
        {
            get { return m_eUserSetCurrent; }
            set
            {
                if(SetCurrentUserset(value))
                {
                    if(LoadUserset())
                        m_eUserSetCurrent = value;
                }
            }
        }
        int m_nRotaryEncoderMultiplier = 1;
        public int p_nRotaryEncoderMultiplier
        {
            get { return m_nRotaryEncoderMultiplier; }
            set
            {
                if (SetRotaryEncoderMultiplier(value))
                    m_nRotaryEncoderMultiplier = value;
            }
        }
        int m_nRotaryEncoderDivider = 1;
        public int p_nRotaryEncoderDivider
        {
            get { return m_nRotaryEncoderDivider; }
            set
            {
                if (SetRotaryEncoderDivider(value))
                    m_nRotaryEncoderDivider = value;
            }
        }
        eFlatFieldUserSet m_eFlatFieldUserSet = eFlatFieldUserSet.Factory;
        public eFlatFieldUserSet p_eFlatFieldCorrection
        {
            get { return m_eFlatFieldUserSet; }
            set
            {
                //if (m_eFlatFieldUserSet == value) return;
                if (SetFlatFieldUserSet(value))
                {
                    if (LoadCalibration())
                        m_eFlatFieldUserSet = value;
                }
            }
        }


        eDir m_eDir = eDir.Forward;
        public eDir p_eDir
        {
            get
            {
                return m_eDir;
            }
            set
            {
                SetFeatureValue(ref m_eDir, value, "sensorScanDirection", typeof(string));
            }
        }

        eTDIMode m_eTDIMode = eTDIMode.Tdi;
        public eTDIMode p_eTDIMode
        {
            get
            {
                return m_eTDIMode;
            }
            set
            {
                SetFeatureValue(ref m_eTDIMode, value, "sensorTDIModeSelection", typeof(string));
            }
        }

        eDeviceScanType m_eDeviceScanType = eDeviceScanType.Linescan;
        public eDeviceScanType p_eDeviceScanType
        {
            get
            {
                return m_eDeviceScanType;
            }
            set
            {
                SetFeatureValue(ref m_eDeviceScanType, value, "DeviceScanType", typeof(string));
            }
        }

        eTriggerMode m_eTriggerMode = eTriggerMode.External;
        public eTriggerMode p_eTriggerMode
        {
            get
            {
                return m_eTriggerMode;
            }
            set
            {
                SetFeatureValue(ref m_eTriggerMode, value, "TriggerMode", typeof(string));
            }
        }

        void SetFeatureValue<T>(ref T Storage, T value, string sFeatureName, Type type)      //형태가 다를수 있어서 Featruevalue 는 실제 type을 넣는다.
        {
            if (m_sapCam == null)
                return;
            if (Object.Equals(Storage, value))
                return;
            m_log.Info("Dalsa Cam Feature Change  " + sFeatureName + " : " + Storage + " , " + value);
            string stype = type.ToString();
            switch (stype.ToLower())
            {
                case "system.bool":
                    bool bValue = Convert.ToBoolean(value);
                    m_sapCam.SetFeatureValue(sFeatureName, bValue);
                    m_sapCam.GetFeatureValue(sFeatureName, out bValue);
                    if (Object.Equals(bValue, value))       //value로 변했는지 확인
                    {
                        Storage = value;
                        //    RaisePropertyChanged();
                    }
                    else
                        m_log.Info("Dalsa Cam Feature Change " + sFeatureName + " : Fail");
                    break;
                case "system.int32":
                    int nValue = Convert.ToInt32(value);
                    m_sapCam.SetFeatureValue(sFeatureName, nValue);
                    m_sapCam.GetFeatureValue(sFeatureName, out nValue);
                    if (Object.Equals(nValue, value))
                    {
                        Storage = value;
                        // RaisePropertyChanged();
                    }
                    else
                        m_log.Info("Dalsa Cam Feature Change " + sFeatureName + " : Fail");
                    break;
                case "system.string":
                    string sValue = value.ToString();
                    m_sapCam.SetFeatureValue(sFeatureName, sValue);
                    m_sapCam.GetFeatureValue(sFeatureName, out sValue);
                    //if (Object.Equals(sValue, value))
                    //{
                    Storage = value;
                    //   RaisePropertyChanged();
                    //}
                    //else
                    //    m_log.Info("Dalsa Cam Feature Change " +  sFeatureName + " : Fail");
                    break;
                case "system.uint64":
                    ulong unValue = Convert.ToUInt64(value);
                    m_sapCam.SetFeatureValue(sFeatureName, unValue);
                    m_sapCam.GetFeatureValue(sFeatureName, out unValue);
                    if (Object.Equals(unValue, value))
                    {
                        Storage = value;
                        //   RaisePropertyChanged();
                    }
                    else
                        m_log.Info("Dalsa Cam Feature Change " + sFeatureName + " : Fail");
                    break;
                default:
                    m_log.Info("Dalsa Cam Feature Change Fail - Type Def Fail");
                    break;
            }
        }
        public void SetGrabDirection(eDir dir)
        {
            if (m_sapCam != null)
                m_sapCam.SetFeatureValue("sensorScanDirection", dir.ToString());
        }

        public DalsaParameterSet(Log log)
        {
            m_log = log;
        }

        public void ReadParamter()
        {
            m_SapGrabber.GetParameter(SapAcquisition.Prm.CROP_WIDTH, out m_Width);
            m_SapGrabber.GetParameter(SapAcquisition.Prm.CROP_HEIGHT, out m_Height);

            string res;
            if (m_sapCam != null)
            {
                m_sapCam.GetFeatureValue("DeviceScanType", out res);
                if (!res.Equals("Linescan"))
                    m_sapCam.SetFeatureValue("DeviceScanType", "Linescan");
            }
            p_eDeviceScanType = eDeviceScanType.Linescan;

            if(m_sapCam != null)
            {
                m_sapCam.GetFeatureValue("TriggerMode", out res);
                if (!res.Equals("External"))
                    m_sapCam.SetFeatureValue("TriggerMode", "External");
            }
            p_eTriggerMode = eTriggerMode.External;

            // Get default UserSet data from device
            eUserSet userset = eUserSet.Default;
            if (GetPowerupUserset(ref userset)) m_eUserSetPowerup = userset;
            if (GetCurrentUserset(ref userset)) m_eUserSetCurrent = userset;

            // Get default Encoder Multiplier, Divider
            int nEncoderVal = 1;
            if (GetRotaryEncoderMultiplier(ref nEncoderVal)) m_nRotaryEncoderMultiplier = nEncoderVal;
            if (GetRotaryEncoderDivider(ref nEncoderVal)) m_nRotaryEncoderDivider = nEncoderVal;

            // Get default Flat Field UserSet
            eFlatFieldUserSet FlatFieldUserset = eFlatFieldUserSet.Factory;
            if (GetFlatFieldoUserSet(ref FlatFieldUserset)) m_eFlatFieldUserSet = FlatFieldUserset;

        }
        public void SetAreaParams()
        {
            if (m_sapCam == null)
                return;

            string res;
            m_sapCam.GetFeatureValue("DeviceScanType", out res);
            if (!res.Equals("Areascan"))
                m_sapCam.SetFeatureValue("DeviceScanType", "Areascan");
            p_eDeviceScanType = eDeviceScanType.Areascan;

            m_sapCam.GetFeatureValue("TriggerMode", out res);
            if (!res.Equals("Internal"))
                m_sapCam.SetFeatureValue("TriggerMode", "Internal");
            p_eTriggerMode = eTriggerMode.Internal;
        }
        public bool SetPowerupUserset(eUserSet userset)
        {
            if (m_sapCam == null) return false;

            return m_sapCam.SetFeatureValue("UserSetDefaultSelector", userset.ToString());
        }
        public bool GetPowerupUserset(ref eUserSet userset)
        {
            if (m_sapCam == null) return false;

            string sReturn;
            bool bOk = m_sapCam.GetFeatureValue("UserSetDefaultSelector", out sReturn);
            if(bOk)
                userset = (eUserSet)Enum.Parse(typeof(eUserSet), sReturn);

            return bOk;
        }
        public bool SetCurrentUserset(eUserSet userset)
        {
            if (m_sapCam == null) return false;

            return m_sapCam.SetFeatureValue("UserSetSelector", userset.ToString());
        }
        public bool GetCurrentUserset(ref eUserSet userset)
        {
            if (m_sapCam == null) return false;

            string sReturn;
            bool bOk = m_sapCam.GetFeatureValue("UserSetSelector", out sReturn);
            if(bOk)
                userset = (eUserSet)Enum.Parse(typeof(eUserSet), sReturn);

            return bOk;
        }
        public bool LoadUserset()
        {
            if (m_sapCam == null) return false;

            return m_sapCam.SetFeatureValue("UserSetLoad", "");
        }
        public bool SetRotaryEncoderMultiplier(int mul)
        {
            if (m_sapCam == null) return false;

            return m_sapCam.SetFeatureValue("rotaryEncoderMultiplier", mul);
        }
        public bool GetRotaryEncoderMultiplier(ref int mul)
        {
            if (m_sapCam == null) return false;

            int tempMul;
            bool bOk = m_sapCam.GetFeatureValue("rotaryEncoderMultiplier", out tempMul);
            if (bOk)
                mul = tempMul;

            return bOk;
        }
        public bool SetRotaryEncoderDivider(int div)
        {
            if (m_sapCam == null) return false;

            return m_sapCam.SetFeatureValue("rotaryEncoderDivider", div);
        }
        public bool GetRotaryEncoderDivider(ref int div)
        {
            if (m_sapCam == null) return false;

            int tempDiv;
            bool bOk = m_sapCam.GetFeatureValue("rotaryEncoderDivider", out tempDiv);
            if (bOk)
                div = tempDiv;

            return bOk;
        }

        public bool SetFlatFieldUserSet(eFlatFieldUserSet userset)
        {
            if (m_sapCam == null) return false;

            return m_sapCam.SetFeatureValue("flatfieldCorrectionCurrentActiveSet", userset.ToString());
        }
        public bool GetFlatFieldoUserSet(ref eFlatFieldUserSet userset)
        {
            if (m_sapCam == null) return false;

            string sReturn;
            bool bOk = m_sapCam.GetFeatureValue("flatfieldCorrectionCurrentActiveSet", out sReturn);
            if (bOk)
                userset = (eFlatFieldUserSet)Enum.Parse(typeof(eFlatFieldUserSet), sReturn);

            return bOk;
        }
        public bool LoadCalibration()
        {
            if (m_sapCam == null) return false;

            return m_sapCam.SetFeatureValue("flatfieldCalibrationLoad", "");
        }

        public bool SetAnalogGain(eAnalogGain eGain)
        {
            if (m_sapCam == null) return false;

            return m_sapCam.SetFeatureValue("AnalogGain", eGain.ToString());
        }

        public bool ChangeGainSelector(eGainSelector eSelector)
        {
            if (m_sapCam == null) return false;

            return m_sapCam.SetFeatureValue("GainSelector", eSelector.ToString());
        }

        public bool SetGain(double dGain)
        {
            if (m_sapCam == null) return false;

            return m_sapCam.SetFeatureValue("Gain", dGain);
        }

        public void SetCamHandle(SapAcqDevice device, SapAcquisition acquisition)
        {
            m_sapCam = device;
            m_SapGrabber = acquisition;
        }

        public void DisconnectCamHandle()
        {
            m_sapCam = null;
            m_SapGrabber = null;
        }

        public void ValueChange(object value, [CallerMemberName] string propertyName = null)
        {
            m_log.Info("Basler Cam Param Change : " + propertyName + " , " + value);
            RaisePropertyChanged(propertyName);
        }
    }


}
