using System;
using System.Windows.Controls;

namespace RootTools.Light
{
    public class LightBase : NotifyProperty
    {
        #region Property
        string _sID = "";
        public string p_sID
        {
            get { return _sID; }
            set
            {
                _sID = value;
                OnPropertyChanged();
            }
        }

        double _fGetPower = 0; 
        public double p_fGetPower
        {
            get { return _fGetPower; }
            set
            {
                if (_fGetPower == value) return;
                _fGetPower = Math.Round(100 * value) / 100.0;
                OnPropertyChanged(); 
            }
        }

        bool _bOn = false;
        public bool p_bOn
        {
            get { return _bOn; }
            set
            {
                if (_bOn == value) return;
                _bOn = value;
                OnPropertyChanged();
                SetPower(); 
            }
        }

        double _fSetPower = 0; 
        public double p_fSetPower
        {
            get { return _fSetPower; }
            set
            {
                double fSetPower = value * p_fScalePower;
                if (fSetPower < 0) fSetPower = 0;
                if (fSetPower > p_fMaxPower) fSetPower = p_fMaxPower;
                _fSetPower = Math.Round(100 * fSetPower / p_fScalePower) / 100.0; 
                OnPropertyChanged();
                SetPower();
                _nDifferent = 0; 
            }
        }

        int _nDifferent = 0; 
        public int p_nDifferent
        {
            get
            {
                _nDifferent = (p_fGetPower == p_fSetPower) ? 0 : _nDifferent + 1;
                return _nDifferent; 
            }
        }
        #endregion

        #region Registry
        double _fMaxPower = 100;
        public double p_fMaxPower
        {
            get { return _fMaxPower; }
            set
            {
                if (_fMaxPower == value) return;
                _fMaxPower = value;
                if (_fMaxPower < 0) _fMaxPower = 0;
                if (_fMaxPower > 1000) _fMaxPower = 1000;
                OnPropertyChanged();
                m_reg.Write("MaxPower", p_fMaxPower);
            }
        }

        double _fScalePower = 1;
        public double p_fScalePower
        {
            get { return _fScalePower; }
            set
            {
                if (_fScalePower == value) return;
                _fScalePower = value;
                if (_fScalePower < 0) _fScalePower = 0;
                if (_fScalePower > 2) _fScalePower = 2;
                OnPropertyChanged();
                m_reg.Write("ScalePower", p_fScalePower);
            }
        }

        Registry m_reg;
        void InitRegistry()
        {
            m_reg = new Registry("Light." + p_id);
            p_fMaxPower = m_reg.Read("MaxPower", (double)100.0);
            p_fScalePower = m_reg.Read("ScalePower", (double)1.0);
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                LightBase_UI ui = new LightBase_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        #region Virtual
        protected virtual void GetPower() { }
        public virtual void SetPower() { }

        public string p_id { get; set; }
        public int p_nChannel { get; set; }
        public virtual void Init(string id, int nChannel)
        {
            p_id = id;
            p_sID = id;
            p_nChannel = nChannel; 
            InitRegistry();
        }
        #endregion

        public void Deselect()
        {
            p_sID = p_id; 
        }


    }
}
