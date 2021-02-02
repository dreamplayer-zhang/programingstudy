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
            //ulong buff= 0;
            int nBuff =0;
            //m_sapCam.GetFeatureValue("Width", out buff);
            //m_Width = Convert.ToInt32( buff);
            //m_sapCam.GetFeatureValue("Height", out buff);
            //m_Height = Convert.ToInt32(buff);
            //m_SapGrabber.GetParameter(SapAcquisition.Prm.HACTIVE, out nBuff);
            //m_Height = nBuff;
            
            m_SapGrabber.GetCapability(SapAcquisition.Cap.HACTIVE_MAX, out nBuff);
            m_SapGrabber.GetParameter(SapAcquisition.Prm.CROP_WIDTH, out m_Width);
            m_SapGrabber.GetParameter(SapAcquisition.Prm.CROP_HEIGHT, out m_Height);
            if (m_sapCam != null)
            {
                string strTriggerMode = "";
                m_sapCam.GetFeatureValue("TriggerMode", out strTriggerMode);
                if (strTriggerMode == "Internal")
                    m_sapCam.SetFeatureValue("TriggerMode", "External");
            }
        }
        
        public void SetCamHandle(SapAcqDevice device, SapAcquisition acquisition)
        {
            m_sapCam = device;
            m_SapGrabber = acquisition;
        }

        public void ValueChange(object value, [CallerMemberName] string propertyName = null)
        {
            m_log.Info("Basler Cam Param Change : " + propertyName + " , " + value);
            RaisePropertyChanged(propertyName);
        }
    }

   
}
