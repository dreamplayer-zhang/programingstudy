using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Controls;

namespace RootTools.Light
{
    public class LightTool_12ch : ObservableObject, ILightTool
    {
        const int c_lLight = 12;

        public delegate void dgOnChangeTool();
        public event dgOnChangeTool OnChangeTool;

        #region Property
        public string p_id { get; set; }

        string _sInfo = "OK";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                RaisePropertyChanged();
                if (value == "OK") return;
                m_log.Error(value);
            }
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                LightTool_12ch_UI ui = new LightTool_12ch_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        #region Light
        public class Light : LightBase
        {
            protected override void GetPower()
            {
                double fPower = 0; 
                m_usb.p_sInfo = m_usb.Read(m_nCh, ref fPower); 
                fPower /= p_fScalePower;
                p_fGetPower = fPower; 
            }
            public override void SetPower()
            {
                double fPower = p_fSetPower;
                m_usb.p_sInfo = m_usb.Write(m_nCh, fPower * p_fScalePower); 
            }

            int m_nCh = 0;
            CyUSBTool m_usb;
            public CyUSBTool p_usb
            {
                get { return m_usb; }
                set { SetProperty(ref m_usb, value); }
            }
            public Light(string id, int nCh, CyUSBTool usb)
            {
                m_nCh = nCh;
                p_usb = usb;
                Init(id + "." + nCh.ToString("00"), nCh); 
            }
        }

        public List<LightBase> p_aLight { get; set; }
        void InitLight()
        {
            for (int n = 0; n < c_lLight; n++)
            {
                Light light = new Light(p_id, n, m_usb);
                p_aLight.Add(light);
            }
        }

        public LightBase GetLight(int nCh, string sNewID)
        {
            if (nCh < 0) return null;
            if (nCh >= c_lLight) return null;
            if (p_aLight[nCh].p_sID != p_aLight[nCh].p_id) return null;
            p_aLight[nCh].p_sID = sNewID;
            if (OnChangeTool != null) OnChangeTool();
            return p_aLight[nCh];
        }

        public void Deselect(LightBase light)
        {
            light.Deselect();
            if (OnChangeTool != null) OnChangeTool();
        }
        #endregion

        IEngineer m_engineer;
        Log m_log;
        public string strr = "111";
        public string p_String
        {
            get
            {
                return strr;
            }
            set
            {
                SetProperty(ref strr, value);
            }
        }
        public CyUSBTool m_usb;
        public CyUSBTool p_usb
        {
            get { return m_usb; }
            set { SetProperty(ref m_usb, value); }
        }
        public LightTool_12ch(int iDevice, string id, IEngineer engineer)
        {
            p_aLight = new List<LightBase>(); 
            p_id = id;
            m_engineer = engineer; 
            m_log = LogView.GetLog(id);
            m_usb = new CyUSBTool(iDevice, p_id, m_log);
            InitLight();
        }

        public void ThreadStop()
        {
            m_usb.ThreadStop(); 
        }
    }
}
