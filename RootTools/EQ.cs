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
            Run,
            Error,
            Null
        }

        public static _EQ m_EQ = new _EQ();

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
            set { m_EQ.p_bStop = value; }
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
            DoorOpen
        }
        public delegate void dgOnChanged(eEQ eEQ, dynamic value);
        public event dgOnChanged OnChanged;
        #endregion

        EQ.eState _eState = EQ.eState.Init;
        public EQ.eState p_eState
        {
            get { return _eState; }
            set
            {
                if (_eState == value) return;
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
    }
}
