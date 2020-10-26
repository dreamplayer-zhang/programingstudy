using RootTools;
using RootTools.Trees;
using System;

namespace Root_TactTime
{
    public class Module : NotifyProperty
    {
        #region UI
        Module_UI _ui = null;
        public Module_UI p_ui
        {
            get
            {
                if (_ui == null)
                {
                    _ui = new Module_UI();
                    _ui.Init(this);
                }
                return _ui; 
            }
        }

        TactTime.eColor _eColor = TactTime.eColor.None;
        public TactTime.eColor p_eColor
        {
            get { return _eColor; }
            set
            {
                if (_eColor == value) return;
                _eColor = value;
                p_ui.Background = m_tact.m_aColor[value];
            }
        }
        #endregion
        public string p_id { get; set; }

        string _sStrip = "Init"; 
        public string p_sStrip
        {
            get { return _sStrip; }
            set
            {
                if (_sStrip == value) return;
                if (value == "") m_secRun[0] = 0; 
                switch (m_eType)
                {
                    case eType.Load:
                        if (value == "")
                        {
                            _sStrip = "Strip " + m_tact.m_iStrip.ToString("00");
                            m_tact.m_iStrip++;
                        }
                        break;
                    case eType.Unload:
                        if (value != "") m_tact.p_nUnload++; 
                        _sStrip = ""; 
                        break;
                    default: _sStrip = value; break; 
                }
                m_secRun[0] = m_tact.p_secRun; //forget Delete
                OnPropertyChanged();
                OnPropertyChanged("p_fProgress");
            }
        }

        public void Clear()
        {
            p_sStrip = "";
            m_secReady = 0;
        }

        public double[] m_secRun = new double[2] { 0, 1 }; 
        public double p_fProgress
        {
            get
            {
                if (p_sStrip == "") return 0;
                return Math.Min(100, 100.0 * (m_tact.p_secRun - m_secRun[0]) / m_secRun[1]); 
            }
            set { }
        }

        public void MoveFrom(Picker picker, bool bDrag)
        {
            m_tact.ClearColor(); 
            picker.p_eColor = TactTime.eColor.From;
            p_eColor = TactTime.eColor.To;
            if (bDrag) m_tact.AddSequence(picker.p_id, p_id);
            double secNow = Math.Max(picker.m_loader.m_secReady, m_secReady); 
            picker.m_loader.Move(ref secNow, m_rpLoc - picker.m_rpLoc); 
            picker.m_loader.AddEvent(ref secNow, picker.m_secPickerPut, "Picker Put"); 
            p_sStrip = picker.p_sStrip; 
            picker.p_sStrip = "";
            picker.m_loader.m_secReady = secNow; 
        }

        public double m_secReady = 0; 
        public void WaitDone(ref double secNow, Loader loader)
        {
            double secDone = m_secRun[1] - (secNow - m_secRun[0]);
            if (secDone > 0) loader.AddEvent(ref secNow, secDone, p_id + " Wait Done");
            m_secReady = secNow; 
        }

        public enum eType
        {
            Load, 
            Run,
            Unload
        }
        eType m_eType = eType.Run; 
        public CPoint m_cpLoc;
        public RPoint m_rpLoc; 
        public TactTime m_tact; 
        public Module(TactTime tact, string id, double secRun, CPoint cpLoc, RPoint rpLoc, eType eType = eType.Run)
        {
            m_tact = tact; 
            p_id = id;
            m_secRun[1] = secRun;
            m_cpLoc = cpLoc;
            m_rpLoc = rpLoc; 
            m_eType = eType;
            p_sStrip = ""; 
        }
    }
}
