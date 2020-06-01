using RootTools.Comm;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace RootTools.Light
{
    public class LightTool_LVS : ObservableObject, ILightTool
    {
        const int c_lLight = 4;

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
                LightTool_LVS_UI ui = new LightTool_LVS_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        #region Light
        public class Light : LightBase
        {
            protected override void GetPower() { }

            string m_sSend = "";
            public override void SetPower()
            {
                double fPower = p_bOn ? p_fSetPower : 0;
                fPower *= p_fScalePower;
                m_sSend = GetCommand(fPower);
                p_rs232.Send(m_sSend); 
            }

            const int c_lMaxPower = 255;
            string GetCommand(double fPower)
            {
                int nPower = (int)Math.Round(c_lMaxPower * fPower / 100);
                return ":1" + m_nCh.ToString("0") + nPower.ToString("000");
            }

            private void M_rs232_OnRecieve(string sRead)
            {
                p_fGetPower = p_fSetPower; 
            }

            int m_nCh = 0;
            RS232 m_rs232;
            public RS232 p_rs232
            {
                get { return m_rs232; }
                set { SetProperty(ref m_rs232, value); }
            }
            public Light(string id, int nCh, RS232 rs232)
            {
                m_nCh = nCh;
                p_rs232 = rs232;
                Init(id + "." + nCh.ToString("00"), nCh);
                p_rs232.OnRecieve += M_rs232_OnRecieve;
            }
        }

        public List<LightBase> p_aLight { get; set; }
        void InitLight()
        {
            for (int n = 0; n < c_lLight; n++)
            {
                Light light = new Light(p_id, n, p_rs232);
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
        public RS232 m_rs232;
        public RS232 p_rs232
        {
            get { return m_rs232; }
            set { SetProperty(ref m_rs232, value); }
        }
        public LightTool_LVS(string id, IEngineer engineer)
        {
            p_aLight = new List<LightBase>(); 
            p_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id);
            p_rs232 = new RS232(id, m_log);
            InitLight();
        }

        public void ThreadStop()
        {
            p_rs232.ThreadStop();
        }
    }
}
