using RootTools.Comm;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Controls;

namespace RootTools.Light
{
    public class LightTool_4ch : ObservableObject, ILightTool
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
                LightTool_4ch_UI ui = new LightTool_4ch_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        #region Light
        public class Light : LightBase
        {
            string m_sGet = ""; 
            protected override void GetPower() { }

            string m_sSet = "";
            string m_sSetPower = "";
            public override void SetPower()
            {
                double fPower = p_fSetPower;
                fPower *= p_fScalePower;
                m_sSetPower = ((int)(fPower * p_fScalePower)).ToString("000");
                string str = "led " + m_nCh.ToString() + " " + m_sSetPower;
                m_rs232.Send(str);
            }

            //private void M_rs232_OnReceive(string sRead)
            //{
            //    if (sRead.Length < 11) return;
            //    if (sRead.Substring(0, 6) != m_sGet) return;
            //    string sPower = sRead.Substring(7, 3);
            //    if (sPower == m_sSetPower) p_fGetPower = p_fSetPower;
            //    else p_fGetPower = Convert.ToDouble(sPower) / p_fScalePower;
            //}

            private void M_rs232_OnReceive(string sRead)
            {
                if (sRead.Length < 9) return;
                string sPower = sRead.Substring(6, 3);
                if (sPower == m_sSetPower) p_fGetPower = p_fSetPower;
                else p_fGetPower = Convert.ToDouble(sPower) / p_fScalePower;
                p_rs232.m_commLog.Add(CommLog.eType.Receive, "4CH Receive = " + sRead.Trim());
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
                m_rs232 = rs232;
                Init(id + "." + nCh.ToString("00"), nCh);
                m_sGet = "<Get " + m_nCh.ToString();
                m_sSet = "<Set " + m_nCh.ToString();
                
                m_rs232.OnReceive += M_rs232_OnReceive;
            }
        }

        public List<LightBase> p_aLight { get; set; }
        void InitLight()
        {  
            p_rs232.p_bConnect = true;
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
        public LightTool_4ch(string id, IEngineer engineer)
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
