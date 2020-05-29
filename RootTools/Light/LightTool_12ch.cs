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
        public string p_id { get { return m_id; } }

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
                return (UserControl)ui;
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

        List<LightBase> m_aLight = new List<LightBase>(); 
        public List<LightBase> p_aLight { get { return m_aLight; } }
        void InitLight()
        {
            for (int n = 0; n < c_lLight; n++)
            {
                Light light = new Light(m_id, n, m_usb);
                m_aLight.Add(light);
            }
        }

        public LightBase GetLight(int nCh, string sNewID)
        {
            if (nCh < 0) return null;
            if (nCh >= c_lLight) return null;
            if (m_aLight[nCh].p_sID != m_aLight[nCh].p_id) return null;
            m_aLight[nCh].p_sID = sNewID;
            if (OnChangeTool != null) OnChangeTool();
            return m_aLight[nCh];
        }

        public void Deselect(LightBase light)
        {
            light.Deselect();
            if (OnChangeTool != null) OnChangeTool();
        }
        #endregion

        string m_id;
        IEngineer m_engineer;
        Log m_log;
        public CyUSBTool m_usb;
        public CyUSBTool p_usb
        {
            get { return m_usb; }
            set { SetProperty(ref m_usb, value); }
        }
        public LightTool_12ch(int iDevice, string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = engineer; 
            m_log = LogView.GetLog(id);
            m_usb = new CyUSBTool(iDevice, m_id, m_log);
            InitLight();
        }

        public void ThreadStop()
        {
            m_usb.ThreadStop(); 
        }
    }
}
