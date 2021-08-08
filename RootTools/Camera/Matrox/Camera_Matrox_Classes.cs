using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RootTools.Camera.Matrox
{
    public enum eCamState
    {
        Init,
        Ready,
        GrabMem,
        GrabLive,
        Done,
    };

    public class UserDataObject
    {
        public MIL_INT NbGrabStart;
    }

    public class MatroxCamInfo : ObservableObject
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

        int _nSystemNum = 0;
        public int p_nSystemNum
        {
            get
            {
                return _nSystemNum;
            }
            set
            {
                SetValueProperty(ref _nSystemNum, value);
            }
        }
        
        public MatroxCamInfo(Log log)
        {
            m_log = log;
        }

        public void SetValueProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return;
            m_log.Info("Matrox Cam Status Change : " + propertyName + " , " + value);
            SetProperty(ref storage, value, propertyName);
        }
    }
}
