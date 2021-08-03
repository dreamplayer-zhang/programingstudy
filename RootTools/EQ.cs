using System.ComponentModel;
using System.Threading;

namespace RootTools
{
    public static class EQ
    {

        public enum eState
        {
            Init,
            Home,
            Ready,
            Idle,//LYJ 210203 add
            Run,
            Recovery,  //LYJ 210128 add
            Error,
            ModuleRunList,
            Null
        }

        public static _EQ m_EQ = new _EQ();
        public static bool m_bRun = false;

        public static string m_sModel = "Model"; 
        public static string c_sPathRecipe = "c:\\Recipe";

        public static eState p_eState
        {
            get { return m_EQ.p_eState; }
            set { m_EQ.p_eState = value; }
        }

        public static string p_sInfo
        {
            get { return m_EQ.p_sInfo; }
            set { m_EQ.p_sInfo = value; }
        }

        public static bool p_bStop
        {
            get { return m_EQ.p_bStop; }
            set { 
                m_EQ.p_bStop = value; }
        }

        public static bool p_bPause
        {
            get { return m_EQ.p_bPause; }
            set { m_EQ.p_bPause = value; }
        }

        public static bool p_bSimulate
        {
            get { return m_EQ.p_bSimulate; }
            set { m_EQ.p_bSimulate = value; }
        }

        public static bool p_bDoorOpen
        { 
            get { return m_EQ.p_bDoorOpen; }
            set { m_EQ.p_bDoorOpen = value; }
        }
        
        public static bool IsStop(int msSimulate = 0)
        {
            while (m_EQ.p_bPause) Thread.Sleep(10);
            if (msSimulate > 0) Thread.Sleep(msSimulate); 
            return m_EQ.p_bStop;
        }

        public static bool p_bRecovery
        {
            get { return m_EQ.p_bRecovery; }
            set { m_EQ.p_bRecovery = value; }
        }

        public static bool p_bPickerSet
        {
            get { return m_EQ.p_bPickerSet; }
            set { m_EQ.p_bPickerSet = value; }
        }

        public static int p_nRnR
        {
            get { return m_EQ.p_nRnR; }
            set { m_EQ.p_nRnR = value; }
        }

        public static int p_nRunLP
        {
            get { return m_EQ.p_nRunLP; }
            set { m_EQ.p_nRunLP = value; }
        }
    }

    public class _EQ : NotifyProperty
    {
        #region Deligate

        public enum eEQ
        {
            State,
            Stop,
            Pause,
            Simulate,
            DoorOpen,
            Recovery,
            PickerSet
        }
        public delegate void dgOnChanged(eEQ eEQ, dynamic value);
        public event dgOnChanged OnChanged;
        #endregion

        EQ.eState _eState = EQ.eState.Init;
        public EQ.eState m_eStateOld = EQ.eState.Init;
        public EQ.eState p_eState
        {
            get { return _eState; }
            set
            {
                if (_eState == value) return;
                m_eStateOld = _eState; 
                _eState = value;
                OnPropertyChanged();
                if (OnChanged != null) OnChanged(eEQ.State, value); 
            }
        }

        string _sInfo = "Last Error";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                _sInfo = value;
                OnPropertyChanged();
            }
        }

        bool _bStop = false;
        public bool p_bStop
        {
            get { return _bStop; }
            set
            {
                if (_bStop == value) return;
                _bStop = value;
                OnPropertyChanged();
                if (OnChanged != null) OnChanged(eEQ.Stop, value);
            }
        }

        bool _bPause = false;
        public bool p_bPause
        {
            get { return _bPause; }
            set
            {
                if (_bPause == value) return;
                _bPause = value;
                OnPropertyChanged();
                if (OnChanged != null) OnChanged(eEQ.Pause, value);
            }
        }

        bool _bSimulate = false;
        public bool p_bSimulate
        {
            get { return _bSimulate; }
            set
            {
                if (_bSimulate == value) return;
                _bSimulate = value;
                OnPropertyChanged();
                if (OnChanged != null) OnChanged(eEQ.Simulate, value);
            }
        }

        bool _bDoorOpen = false;
        public bool p_bDoorOpen
        {
            get { return _bDoorOpen; }
            set
            {
                if (_bDoorOpen == value) return;
                _bDoorOpen = value;
                OnPropertyChanged();
                if (OnChanged != null) OnChanged(eEQ.DoorOpen, value);
            }
        }

        bool _bRecovery = false;
        public bool p_bRecovery
        {
            get { return _bRecovery; }
            set
            {
                if (_bRecovery == value) return;
                _bRecovery = value;
                OnPropertyChanged();
                if (OnChanged != null) OnChanged(eEQ.Recovery, value);
            }
        }

        bool _bPickerSet = false;
        public bool p_bPickerSet
        {
            get { return _bPickerSet; }
            set
            {
                _bPickerSet = value;
                OnPropertyChanged();
                if (OnChanged != null) OnChanged(eEQ.PickerSet, value);
            }
        }

        int _nRnR = 1;
        public int p_nRnR 
        {
            get { return _nRnR; }
            set
            {
                _nRnR = value;
                OnPropertyChanged();
            }
        }

        int _nRunLP = 0;
        public int p_nRunLP
        {
            get { return _nRunLP; }
            set
            {
                if (_nRunLP == value) return;
                _nRunLP = value;
                OnPropertyChanged();
            }
        }

        public ref bool StopToken()
        {
            return ref this._bStop;
        }
    }
}
