using RootTools;
using RootTools.Trees;
using System;

namespace Root_TactTime
{
    public class Picker : NotifyProperty
    {
        #region UI
        Picker_UI _ui = null;
        public Picker_UI p_ui
        {
            get
            {
                if (_ui == null)
                {
                    _ui = new Picker_UI();
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

        #region Tree
        public double m_secPickerGet = 1;
        public double m_secPickerPut = 0.7;
        public void RunTree(Tree tree)
        {
            m_secPickerGet = tree.Set(m_secPickerGet, m_secPickerGet, "Get", "Picker Get Time (sec)");
            m_secPickerPut = tree.Set(m_secPickerPut, m_secPickerPut, "Put", "Picker Put Time (sec)");
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
                _sStrip = value;
                OnPropertyChanged();
            }
        }

        public void MoveFrom(Module module, bool bDrag)
        {
            m_tact.ClearColor();
            module.p_eColor = TactTime.eColor.From;
            p_eColor = TactTime.eColor.To;
            if (bDrag) m_tact.AddSequence(module.p_id, p_id);
            double secNow = Math.Max(m_loader.m_secReady, module.m_secReady);
            m_loader.Move(ref secNow, module.m_rpLoc - m_rpLoc); 
            module.WaitDone(ref secNow, m_loader);
            m_loader.AddEvent(ref secNow, m_secPickerGet, "Picker Get"); 
            p_sStrip = module.p_sStrip;
            module.p_sStrip = "";
            m_loader.m_secReady = secNow; 
            module.m_secReady = secNow; 
        }

        public void MoveFrom(Picker picker, bool bDrag)
        {
            m_tact.ClearColor();
            picker.p_eColor = TactTime.eColor.From;
            p_eColor = TactTime.eColor.To;
            if (bDrag) m_tact.AddSequence(picker.p_id, p_id);
            double secNow = Math.Max(m_loader.m_secReady, picker.m_loader.m_secReady);
            m_loader.Move(ref secNow, picker.m_loader.m_rp - m_rpLoc);
            m_loader.AddEvent(ref secNow, picker.m_secPickerPut, "Picker Put");
            p_sStrip = picker.p_sStrip;
            picker.p_sStrip = "";
            m_loader.m_secReady = secNow; 
            picker.m_loader.m_secReady = secNow; 
        }

        public Loader m_loader; 
        public CPoint m_cpLoc;
        public RPoint m_rpLoc;
        public TactTime m_tact;
        public Picker(Loader loader, string id, CPoint cpLoc, RPoint rpLoc)
        {
            m_loader = loader; 
            m_tact = loader.m_tact;
            p_id = id;
            m_cpLoc = cpLoc;
            m_rpLoc = rpLoc; 
            p_sStrip = "";
        }
    }
}
