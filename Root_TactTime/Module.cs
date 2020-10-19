using RootTools;
using System;

namespace Root_TactTime
{
    public class Module : NotifyProperty
    {
        #region Type
        public enum eType
        {
            Module,
            Picker
        }
        public eType m_eType = eType.Module;
        #endregion

        #region Picker
        public static double m_secPickerGet = 1;
        public static double m_secPickerPut = 0.7; 
        #endregion

        public string p_id { get; set; }

        string _sStrip = "Init"; 
        public string p_sStrip
        {
            get { return _sStrip; }
            set
            {
                if (_sStrip == value) return;
                if (m_bAutoLoad && (value == ""))
                {
                    _sStrip = "Strip " + m_tact.m_iStrip.ToString("00");
                    m_tact.m_iStrip++; 
                }
                else _sStrip = value; 
                m_secRun[0] = m_tact.p_secRun;
                OnPropertyChanged();
                OnPropertyChanged("p_fProgress");
            }
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

        public void MoveFrom(Module moduleFrom)
        {
            if (p_sStrip != "")
            {
                double sec = GetSec();
                if (sec > 0) m_tact.p_secRun += sec;
            }
            double secFrom = moduleFrom.GetSec();
            if (secFrom > 0) m_tact.p_secRun += secFrom; 
            p_sStrip = moduleFrom.p_sStrip; 
            moduleFrom.p_sStrip = ""; 
        }

        double GetSec()
        {
            return m_secRun[1] - (m_tact.p_secRun - m_secRun[0]);
        }

        public bool m_bAutoLoad = false; 
        public CPoint m_cpLoc;
        TactTime m_tact; 
        public Module(TactTime tact, string id, eType eType, double secRun, CPoint cpLoc, bool bAutoLoad = false)
        {
            m_tact = tact; 
            p_id = id;
            m_eType = eType; 
            m_secRun[1] = secRun;
            m_cpLoc = cpLoc;
            m_bAutoLoad = bAutoLoad;
            p_sStrip = ""; 
        }
    }
}
