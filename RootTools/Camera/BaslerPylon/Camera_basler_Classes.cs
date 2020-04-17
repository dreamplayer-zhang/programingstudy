using System;
using System.Collections.Generic;
using Basler.Pylon;
using System.Runtime.CompilerServices;

namespace RootTools.Camera.BaslerPylon
{
    public class BaslerCamInfo : ObservableObject
    {
        public string Name = "";
        public string _Name
        {
            get
            {
                return Name;
            }
            set
            {
                SetValueProperty(ref Name, value);
            }
        }
        public string DeviceUserID = "";
        public string _DeviceUserID
        {
            get
            {
                return DeviceUserID;
            }
            set
            {
                SetValueProperty(ref DeviceUserID, value);
            }
        }
        public DeviceAccessibilityInfo ConnectStatus = DeviceAccessibilityInfo.Unknown;
        public DeviceAccessibilityInfo _ConnectStatus
        {
            get
            {
                return ConnectStatus;
            }
            set
            {
                SetValueProperty(ref ConnectStatus, value);
            }
        }
        public bool OpenStatus =false;
        public bool _OpenStatus
        {
            get
            {
                return OpenStatus;
            }
            set
            {
                SetValueProperty(ref OpenStatus, value);
            }
        }
        public string IPAddress ="";
        public string _IPAddress
        {
            get
            {
                return IPAddress;
            }
            set
            {
                SetValueProperty(ref IPAddress, value);
            }
        }
        public LogWriter m_log;
        public Basler.Pylon.Camera m_Cam = null;

        //string IsCanGrab = "";
        public bool _IsCanGrab
        {
            get
            {
                if (m_Cam == null)
                    return false;
                if (m_Cam != null && m_Cam.IsOpen && m_Cam.StreamGrabber.IsGrabbing)
                    return false;
                else
                    return true;
            }
            set
            {
                RaisePropertyChanged();
            }
        }

        public BaslerCamInfo(LogWriter log)
        {
            m_log = log;
        }

        public void SetCamera(Basler.Pylon.Camera cam)
        {
            m_Cam = cam;
        }

        public void SetValueProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return;
            m_log.Info("Basler Cam Status Change : " + propertyName + " , " + value);
            SetProperty(ref storage, value, propertyName);
        }
    }


    public class BaslerParameterSet : ObservableObject
    {
        public Basler.Pylon.Camera m_Cam;
        public LogWriter m_log;
        public long _HeartbeatTimeout
        {
            get
            {
                return (m_Cam == null) ? 0 : m_Cam.Parameters[PLTransportLayer.HeartbeatTimeout].GetValue();
            }
            set{
                if (m_Cam != null)
                {
                    if (m_Cam.Parameters[PLTransportLayer.HeartbeatTimeout].GetValue() == value)
                        return;
                    m_Cam.Parameters[PLTransportLayer.HeartbeatTimeout].TrySetValue(value, IntegerValueCorrection.Nearest);
                    ValueChange(value);
                }
            }
        }
        public long _Width
        {
            get
            {
                if (m_Cam == null)
                    return 0;
                return m_Cam.Parameters[PLCamera.Width].GetValue();
            }
            set
            {
                if (_Width == value)
                    return;
                m_Cam.Parameters[PLCamera.Width].TrySetValue(value, IntegerValueCorrection.Nearest);
                ValueChange(value);
            }
        }
        public long _Height
        {
            get
            {
                if (m_Cam == null)
                    return 0;
                return m_Cam.Parameters[PLCamera.Height].GetValue();
            }
            set
            {
                if (_Height == value)
                    return;
                m_Cam.Parameters[PLCamera.Height].TrySetValue(value, IntegerValueCorrection.Nearest);
                ValueChange(value);
            }
        }
        public string _GainAuto
        {
            get
            {
                try
                {
                    return m_Cam.Parameters[PLCamera.GainAuto].GetValue();
                }
                catch
                {
                    return "Off";
                }
            }
            set
            {
                if (_GainAuto == value)
                    return;
                m_Cam.Parameters[PLCamera.GainAuto].TrySetValue(value);
                ValueChange(value);
            }
        }
        public List<string> _GainAutoEnum
        {
            get
            {
                try
                {
                    List<string> result = new List<string>();

                    foreach (string str in m_Cam.Parameters[PLCamera.GainAuto].GetAllValues())
                    {
                        result.Add(str);
                    }
                    return result;
                }
                catch
                {
                    return new List<string>();
                }
            }
            set
            {  
            }
        }
        public long _GainRaw
        {
            get
            {
                return m_Cam.Parameters[PLCamera.GainRaw].GetValue();
            }
            set
            {
                if (_GainRaw == value)
                    return;
                m_Cam.Parameters[PLCamera.GainRaw].TrySetValue(value, IntegerValueCorrection.Nearest);
                ValueChange(value);
            }
        }
        public bool _GammaEnable
        {
            get
            {
                return m_Cam.Parameters[PLCamera.GammaEnable].GetValue();
            }
            set
            {
                if (_GammaEnable == value)
                    return;
                m_Cam.Parameters[PLCamera.GammaEnable].TrySetValue(value);
                ValueChange(value);
            }
        }
        public double _Gamma
        {
            get
            {
                return m_Cam.Parameters[PLCamera.Gamma].GetValue();
            }
            set
            {
                if (_Gamma  == value)
                    return;
                m_Cam.Parameters[PLCamera.Gamma].TrySetValue(value,FloatValueCorrection.ClipToRange);
                ValueChange(value);
            }
        }
        public long _XOffset
        {
            get
            {
                return m_Cam.Parameters[PLCamera.OffsetX].GetValue();
            }
            set
            {
                if (_XOffset == value)
                    return;
                m_Cam.Parameters[PLCamera.OffsetX].TrySetValue(value, IntegerValueCorrection.Nearest);
                ValueChange(value);
            }
        }
        public long _YOffset
        {
            get
            {
                return m_Cam.Parameters[PLCamera.OffsetY].GetValue();
            }
            set
            {
                if (_YOffset == value)
                    return;
                m_Cam.Parameters[PLCamera.OffsetY].TrySetValue(value, IntegerValueCorrection.Nearest);
                ValueChange(value);
            }
        }
        public bool _ReverseX
        {
            get
            {
                return m_Cam.Parameters[PLCamera.ReverseX].GetValue();
            }
            set
            {
                if (_ReverseX == value)
                    return;
                m_Cam.Parameters[PLCamera.ReverseX].TrySetValue(value);
                ValueChange(value);
            }
        }
        public string _ModelName
        {
            get
            {
                return m_Cam.Parameters[PLCamera.DeviceModelName].GetValue();
            }
            set
            {
                RaisePropertyChanged();
            }
        }
        public string _DeviceID
        {
            get
            {
                return m_Cam.Parameters[PLCamera.DeviceID].GetValue();
            }
            set
            {
                RaisePropertyChanged();
            }
        }
        public string _DeviceUserID
        {
            get
            {
                return m_Cam.Parameters[PLCamera.DeviceUserID].GetValue();
            }
            set
            {
                if (_DeviceUserID== value)
                    return;
                m_Cam.Parameters[PLCamera.DeviceUserID].TrySetValue(value);
                RaisePropertyChanged();
            }
        }
        public string _DeviceScanType
        {
            get
            {
                return m_Cam.Parameters[PLCamera.DeviceScanType].GetValue();
            }
            set
            {
                RaisePropertyChanged();
            }
        }
        public long _SensorWidth
        {
            get
            {
                return m_Cam.Parameters[PLCamera.SensorWidth].GetValue();
            }
            set
            {
                RaisePropertyChanged();
            }
        }
        public long _SensorHeight
        {
            get
            {
                return m_Cam.Parameters[PLCamera.SensorHeight].GetValue();
            }
            set
            {
                RaisePropertyChanged();
            }
        }
        public long _MaxWidth
        {
            get
            {
                return m_Cam.Parameters[PLCamera.WidthMax].GetValue();
            }
            set
            {
                RaisePropertyChanged();
            }
        }
        public long _MaxHeight
        {
            get
            {
                return m_Cam.Parameters[PLCamera.HeightMax].GetValue();
            }
            set
            {
                RaisePropertyChanged();
            }
        }
        public string _UserSetSelector
        {
            get
            {
                    return m_Cam.Parameters[PLCamera.UserSetSelector].GetValue();
            }
            set
            {
                if (_UserSetSelector == value)
                    return;
                m_Cam.Parameters[PLCamera.UserSetSelector].TrySetValue(value);
                ValueChange(value);
            }
        }
        public List<string> _UserSetSelectorEnum
        {
            get
            {
                try
                {
                    List<string> result = new List<string>();

                    foreach (string str in m_Cam.Parameters[PLCamera.UserSetSelector].GetAllValues())
                    {
                        result.Add(str);
                    }
                    return result;
                }
                catch
                {
                    return new List<string>();
                }
            }
            set
            {
            }
        }
        public string _UserSetDefault
        {
            get
            {
                return m_Cam.Parameters[PLCamera.UserSetDefaultSelector].GetValue();
            }
            set
            {
                if (_UserSetDefault == value)
                    return;
                m_Cam.Parameters[PLCamera.UserSetDefaultSelector].TrySetValue(value);
                ValueChange(value);
            }
        }
        public List<string> _UserSetDefaultEnum
        {
            get
            {
                try
                {
                    List<string> result = new List<string>();

                    foreach (string str in m_Cam.Parameters[PLCamera.UserSetDefaultSelector].GetAllValues())
                    {
                        result.Add(str);
                    }
                    return result;
                }
                catch
                {
                    return new List<string>();
                }
            }
            set
            {
            }
        }
        

        public string p_PixelFormat
        {
            get
            {
                return m_Cam.Parameters[PLCamera.PixelFormat].GetValue();
            }
            set
            {
                
            }
        }

        public string _TransmissionType
        {
            get
            {
                
                return m_Cam.Parameters[PLStream.TransmissionType].GetValue();
            }
            set
            {
                if (_TransmissionType == value)
                    return;
                m_Cam.Parameters[PLStream.TransmissionType].TrySetValue(value);
                ValueChange(value);
            }
        }
        //public List<string> _UserSetDefaultEnum
        //{
        //    get
        //    {
        //        try
        //        {
        //            List<string> result = new List<string>();

        //            foreach (string str in m_Cam.Parameters[PLCamera.UserSetDefault].GetAllValues())
        //            {
        //                result.Add(str);
        //            }
        //            return result;
        //        }
        //        catch
        //        {
        //            return new List<string>();
        //        }
        //    }
        //    set
        //    {
        //    }
        //}


       
        public BaslerParameterSet()
        {
        
        }
        public BaslerParameterSet (Basler.Pylon.Camera cam, LogWriter log)
        {
            m_Cam = cam;
            m_log = log;
        }

        public void ValueChange(object value, [CallerMemberName] string propertyName = null)
        {
            m_log.Info("Basler Cam Param Change : " + propertyName + " , " + value);
            RaisePropertyChanged(propertyName);
        }
    }

   
}
